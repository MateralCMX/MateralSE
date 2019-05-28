namespace Sandbox.Game.Components
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation | MyUpdateOrder.BeforeSimulation, 600), StaticEventOwner]
    public sealed class MyRopeComponent : MySessionComponentBase
    {
        public const float DEFAULT_RELEASE_THRESHOLD = 0.1745329f;
        private static float UNLOCK_OFFSET = 0.1125f;
        public static MyRopeComponent Static;
        private static readonly List<MyPhysics.HitInfo> m_hitInfoBuffer = new List<MyPhysics.HitInfo>();
        private static readonly List<long> m_tmpRopesInitialized = new List<long>();
        private static readonly HashSet<long> m_ropesToRemove = new HashSet<long>();
        private static readonly Dictionary<long, InternalRopeData> m_ropeIdToRope = new Dictionary<long, InternalRopeData>();
        private static readonly Dictionary<long, InternalRopeData> m_ropeIdToRayCastRelease = new Dictionary<long, InternalRopeData>();
        private static readonly Dictionary<long, long> m_hookIdToRopeId = new Dictionary<long, long>();
        private static readonly Dictionary<long, HookData> m_hookIdToHook = new Dictionary<long, HookData>();
        private static readonly Dictionary<long, WindingData> m_hookIdToWinding = new Dictionary<long, WindingData>();
        private static readonly Dictionary<long, ReleaseData> m_hookIdToRelease = new Dictionary<long, ReleaseData>();
        private static readonly Dictionary<long, WindingData> m_hookIdToActiveWinding = new Dictionary<long, WindingData>();
        private static readonly Dictionary<long, ReleaseData> m_hookIdToActiveRelease = new Dictionary<long, ReleaseData>();
        private static readonly Dictionary<long, RopeDrumLimits> m_hookIdToRopeLimits = new Dictionary<long, RopeDrumLimits>();
        private static readonly Dictionary<long, UnlockedWindingData> m_hookIdToUnlockedWinding = new Dictionary<long, UnlockedWindingData>();
        private static readonly HashSet<long> m_ropeIdToInit = new HashSet<long>();
        private static MyRopeAttacher m_ropeAttacher;

        private static void ActivateRelease(long hookEntityId)
        {
            ReleaseData data;
            if (m_hookIdToRelease.TryGetValue(hookEntityId, out data) && Sync.IsServer)
            {
                m_hookIdToActiveRelease.Add(hookEntityId, data);
            }
        }

        private static void ActivateWinding(long hookEntityId, WindingData winding, MyRopeDefinition attachedRope)
        {
            m_hookIdToActiveWinding[hookEntityId] = winding;
            VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(hookEntityId, false);
            UpdateDummyWorld(entityById.PositionComp, winding);
            if (attachedRope.WindingSound != null)
            {
                winding.Sound = attachedRope.WindingSound;
                winding.Emitter = new MyEntity3DSoundEmitter(entityById, false, 1f);
            }
        }

        public static void AddHook(long hookEntityId, float size, Vector3 localPivot)
        {
            m_hookIdToHook.Add(hookEntityId, new HookData(size, localPivot));
        }

        public static void AddHookRelease(long hookEntityId, Vector3 localAxis, float thresholdAngleCos)
        {
            m_hookIdToRelease.Add(hookEntityId, new ReleaseData(localAxis, thresholdAngleCos));
        }

        public static void AddHookWinding(long hookEntityId, float radius, ref Matrix localDummy)
        {
            m_hookIdToWinding.Add(hookEntityId, new WindingData(radius, ref localDummy));
        }

        public static void AddOrSetDrumRopeLimits(long hookEntityId, float minRopeLength, float maxRopeLength)
        {
            RopeDrumLimits limits;
            long num;
            if (!m_hookIdToRopeLimits.TryGetValue(hookEntityId, out limits))
            {
                limits = new RopeDrumLimits();
                m_hookIdToRopeLimits[hookEntityId] = limits;
            }
            limits.MinLength = minRopeLength;
            limits.MaxLength = maxRopeLength;
            if (m_hookIdToRopeId.TryGetValue(hookEntityId, out num))
            {
                ApplyRopeLimits(m_ropeIdToRope[num], limits);
            }
        }

        public static long AddRopeData(MyRopeData publicData, long ropeId)
        {
            if (ropeId == 0)
            {
                ropeId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            }
            InternalRopeData data1 = new InternalRopeData();
            data1.Public = publicData;
            data1.RopeId = ropeId;
            InternalRopeData data = data1;
            m_ropeIdToRope[data.RopeId] = data;
            m_hookIdToRopeId[data.Public.HookEntityIdA] = ropeId;
            m_hookIdToRopeId[data.Public.HookEntityIdB] = ropeId;
            if (data.Public.Definition.EnableRayCastRelease && Sync.IsServer)
            {
                m_ropeIdToRayCastRelease.Add(ropeId, data);
            }
            m_ropeIdToInit.Add(ropeId);
            return ropeId;
        }

        public static long AddRopeData(long hookEntityIdA, long hookEntityIdB, MyRopeDefinition ropeDefinition, long ropeId)
        {
            VRage.Game.Entity.MyEntity entity;
            VRage.Game.Entity.MyEntity entity2;
            if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById(hookEntityIdA, out entity, false))
            {
                return 0L;
            }
            if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById(hookEntityIdB, out entity2, false))
            {
                return 0L;
            }
            HookData data = m_hookIdToHook[hookEntityIdB];
            float num = (float) (Vector3D.Transform(m_hookIdToHook[hookEntityIdA].LocalPivot, entity.WorldMatrix) - Vector3D.Transform(data.LocalPivot, entity2.WorldMatrix)).Length();
            MyRopeData publicData = new MyRopeData {
                HookEntityIdA = hookEntityIdA,
                HookEntityIdB = hookEntityIdB,
                MaxRopeLength = num,
                CurrentRopeLength = num,
                Definition = ropeDefinition
            };
            return AddRopeData(publicData, ropeId);
        }

        public static void AddRopeRequest(long entityId1, long entityId2, MyDefinitionId ropeDefinitionId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, long, DefinitionIdBlit>(x => new Action<long, long, DefinitionIdBlit>(MyRopeComponent.AddRopeRequest_Implementation), entityId1, entityId2, ropeDefinitionId, targetEndpoint, position);
        }

        [Event(null, 0x545), Reliable, Server]
        private static void AddRopeRequest_Implementation(long entityId1, long entityId2, DefinitionIdBlit ropeDefinitionId)
        {
            MyRopeDefinition ropeDefinition = (MyRopeDefinition) MyDefinitionManager.Static.GetDefinition((MyDefinitionId) ropeDefinitionId);
            if (CanConnectHooks(entityId1, entityId2, ropeDefinition))
            {
                AddRopeData(entityId1, entityId2, ropeDefinition, 0L);
            }
        }

        private static void ApplyRopeLimits(InternalRopeData ropeData, RopeDrumLimits limits = null)
        {
            if (limits == null)
            {
                m_hookIdToRopeLimits.TryGetValue(ropeData.Public.HookEntityIdA, out limits);
            }
            if (limits == null)
            {
                m_hookIdToRopeLimits.TryGetValue(ropeData.Public.HookEntityIdB, out limits);
            }
            if (limits != null)
            {
                WindingData data;
                float maxLength;
                ropeData.Public.MinRopeLength = limits.MinLength;
                ropeData.Public.MaxRopeLength = limits.MaxLength;
                if (m_hookIdToWinding.TryGetValue(ropeData.Public.HookEntityIdA, out data) && data.IsUnlocked)
                {
                    maxLength = limits.MaxLength;
                }
                else if (!m_hookIdToWinding.TryGetValue(ropeData.Public.HookEntityIdB, out data) || !data.IsUnlocked)
                {
                    maxLength = MathHelper.Clamp(ropeData.Public.CurrentRopeLength, limits.MinLength, limits.MaxLength);
                }
                else
                {
                    maxLength = limits.MaxLength;
                }
                ropeData.Public.CurrentRopeLength = maxLength;
                if (ropeData.ConstraintData != null)
                {
                    ropeData.ConstraintData.LinearLimit = maxLength;
                    ropeData.Constraint.RigidBodyA.Activate();
                    ropeData.Constraint.RigidBodyB.Activate();
                }
            }
            ropeData.TargetRopeLength = ropeData.Public.CurrentRopeLength;
        }

        public static bool AreGridsConnected(MyCubeGrid grid1, MyCubeGrid grid2)
        {
            using (Dictionary<long, InternalRopeData>.ValueCollection.Enumerator enumerator = m_ropeIdToRope.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    InternalRopeData current = enumerator.Current;
                    if ((ReferenceEquals(current.GridA, grid1) && ReferenceEquals(current.GridB, grid2)) || (ReferenceEquals(current.GridA, grid2) && ReferenceEquals(current.GridB, grid1)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [Conditional("DEBUG")]
        private static void AssertWindingLocksConsistent()
        {
            foreach (KeyValuePair<long, WindingData> pair in m_hookIdToWinding)
            {
                long key = pair.Key;
                bool isUnlocked = pair.Value.IsUnlocked;
                m_hookIdToRopeId.ContainsKey(key);
                m_hookIdToUnlockedWinding.ContainsKey(key);
                m_hookIdToActiveWinding.ContainsKey(key);
            }
        }

        public static bool CanConnectHooks(long hookIdFrom, long hookIdTo, MyRopeDefinition ropeDefinition)
        {
            RopeDrumLimits limits;
            RopeDrumLimits limits2;
            bool flag = true;
            m_hookIdToRopeLimits.TryGetValue(hookIdFrom, out limits);
            m_hookIdToRopeLimits.TryGetValue(hookIdTo, out limits2);
            if ((limits != null) && (limits2 != null))
            {
                flag = false;
            }
            else if ((limits != null) || (limits2 != null))
            {
                VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(hookIdFrom, false);
                double num = (Sandbox.Game.Entities.MyEntities.GetEntityById(hookIdTo, false).PositionComp.GetPosition() - entityById.PositionComp.GetPosition()).LengthSquared();
                if (limits != null)
                {
                    flag = flag && (num < (limits.MaxLength * limits.MaxLength));
                }
                if (limits2 != null)
                {
                    flag = flag && (num < (limits2.MaxLength * limits2.MaxLength));
                }
            }
            return flag;
        }

        public static void CloseRopeRequest(long ropeId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(MyRopeComponent.CloseRopeRequest_Implementation), ropeId, targetEndpoint, position);
        }

        [Event(null, 0x552), Reliable, Server]
        private static void CloseRopeRequest_Implementation(long ropeId)
        {
            MyRope rope;
            RemoveRopeData(ropeId);
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRope>(ropeId, out rope, false))
            {
                rope.Close();
            }
        }

        private static unsafe void ComputeLocalPosition(MyCubeBlock block, HookData hook, out Vector3D localPosition)
        {
            Vector3 localPivot = hook.LocalPivot;
            Vector3* vectorPtr1 = (Vector3*) ref localPivot;
            Vector3.TransformNormal(ref (Vector3) ref vectorPtr1, block.Orientation, out localPivot);
            localPosition = localPivot + (((block.Min + block.Max) / 2.0) * block.CubeGrid.GridSize);
        }

        public static void ComputeLocalReleaseAxis(Vector3 localBaseAxis, Vector2 orientation, out Vector3 localAxis)
        {
            localAxis = Vector3.TransformNormal(localBaseAxis, Matrix.CreateRotationY(orientation.X) * Matrix.CreateRotationX(orientation.Y));
        }

        private static Vector3 ComputeTorqueFromRopeImpulse(WindingData windingData, InternalRopeData ropeData, Vector3 ropeDirectionVector, Vector3 centerDelta)
        {
            Vector3 vector2;
            Vector3 vector4;
            Vector3.Normalize(ref ropeDirectionVector, out ropeDirectionVector);
            Vector3 backward = (Vector3) windingData.CurrentDummyWorld.Backward;
            Vector3.Cross(ref ropeDirectionVector, ref backward, out vector2);
            vector2 = (vector2 * windingData.Radius) + centerDelta;
            Vector3 vector3 = ropeDirectionVector * ropeData.ImpulseApplied;
            Vector3.Cross(ref vector2, ref vector3, out vector4);
            return vector4;
        }

        private static void CreateConstraint(InternalRopeData ropeData)
        {
            CreateConstraint(ropeData, m_hookIdToHook[ropeData.Public.HookEntityIdA], m_hookIdToHook[ropeData.Public.HookEntityIdB], (MyCubeBlock) Sandbox.Game.Entities.MyEntities.GetEntityById(ropeData.Public.HookEntityIdA, false), (MyCubeBlock) Sandbox.Game.Entities.MyEntities.GetEntityById(ropeData.Public.HookEntityIdB, false));
        }

        private static void CreateConstraint(InternalRopeData ropeData, HookData hookA, HookData hookB, MyCubeBlock blockA, MyCubeBlock blockB)
        {
            if (!ReferenceEquals(ropeData.GridA, ropeData.GridB))
            {
                MyGridPhysics bodyA = blockA.CubeGrid.Physics;
                MyGridPhysics physics = blockB.CubeGrid.Physics;
                if (((bodyA != null) && ((physics != null) && bodyA.RigidBody.InWorld)) && physics.RigidBody.InWorld)
                {
                    Vector3D vectord;
                    Vector3D vectord2;
                    ComputeLocalPosition(blockA, hookA, out vectord);
                    ComputeLocalPosition(blockB, hookB, out vectord2);
                    ropeData.ConstraintData = new HkRopeConstraintData();
                    Vector3 pivotA = (Vector3) vectord;
                    ropeData.ConstraintData.SetInBodySpace(pivotA, (Vector3) vectord2, bodyA, physics);
                    vectord = Vector3D.Transform(vectord, bodyA.GetWorldMatrix());
                    vectord2 = Vector3D.Transform(vectord2, physics.GetWorldMatrix());
                    ropeData.ConstraintData.LinearLimit = ropeData.Public.CurrentRopeLength;
                    ropeData.ConstraintData.Strength = 0.6f;
                    ropeData.Constraint = new HkConstraint(bodyA.RigidBody, physics.RigidBody, ropeData.ConstraintData);
                    bodyA.AddConstraint(ropeData.Constraint);
                    ropeData.Constraint.Enabled = true;
                    ropeData.ConstraintData.Update(ropeData.Constraint);
                    MyCubeGrid.CreateGridGroupLink(GridLinkTypeEnum.Physical, ropeData.RopeId, blockA.CubeGrid, blockB.CubeGrid);
                    bodyA.RigidBody.Activate();
                    physics.RigidBody.Activate();
                }
            }
        }

        private static void DeactivateWinding(long hookEntityId, WindingData winding = null)
        {
            if (winding == null)
            {
                m_hookIdToActiveWinding.TryGetValue(hookEntityId, out winding);
            }
            if ((winding != null) && (winding.Emitter != null))
            {
                winding.Emitter.StopSound(false, true);
                winding.Emitter = null;
                winding.Sound = null;
            }
            m_hookIdToActiveWinding.Remove(hookEntityId);
        }

        public static void GetDrumRopeLimits(long hookEntityId, out float minRopeLength, out float maxRopeLength)
        {
            RopeDrumLimits limits = m_hookIdToRopeLimits[hookEntityId];
            minRopeLength = limits.MinLength;
            maxRopeLength = limits.MaxLength;
        }

        public static void GetHookData(long hookEntityId, out Vector3 localPivot)
        {
            localPivot = m_hookIdToHook[hookEntityId].LocalPivot;
        }

        public static void GetReleaseData(long hookEntityId, out Vector3 localBaseAxis, out Vector2 orientation, out float thresholdAngleCos)
        {
            ReleaseData data = m_hookIdToRelease[hookEntityId];
            localBaseAxis = data.LocalBaseAxis;
            orientation = data.Orientation;
            thresholdAngleCos = data.ThresholdAngleCos;
        }

        public static void GetRopeData(long ropeEntityId, out MyRopeData ropeData)
        {
            ropeData = m_ropeIdToRope[ropeEntityId].Public;
        }

        public void GetRopesForGrids(HashSet<MyCubeGrid> grids, HashSet<MyRope> outRopes)
        {
            foreach (KeyValuePair<long, InternalRopeData> pair in m_ropeIdToRope)
            {
                if ((grids.Contains(pair.Value.GridA) || grids.Contains(pair.Value.GridB)) && ((pair.Value.Constraint != null) && pair.Value.Constraint.InWorld))
                {
                    MyRope entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(pair.Key, false) as MyRope;
                    if (entityById != null)
                    {
                        outRopes.Add(entityById);
                    }
                }
            }
        }

        public static bool HasEntityWinding(long hookEntityId) => 
            m_hookIdToWinding.ContainsKey(hookEntityId);

        public bool HasGridAttachedRope(MyCubeGrid grid)
        {
            using (Dictionary<long, InternalRopeData>.Enumerator enumerator = m_ropeIdToRope.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<long, InternalRopeData> current = enumerator.Current;
                    if (ReferenceEquals(grid, current.Value.GridA) || ReferenceEquals(grid, current.Value.GridB))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HasRelease(long hookEntityId) => 
            m_hookIdToRelease.ContainsKey(hookEntityId);

        public static bool HasRope(long hookEntityId) => 
            m_hookIdToRopeId.ContainsKey(hookEntityId);

        private static void hookEntity_OnClosing(VRage.Game.Entity.MyEntity hookEntity)
        {
            RemoveRopeOnHookInternal(hookEntity.EntityId);
        }

        public static bool IsWindingUnlocked(long hookEntityId) => 
            m_hookIdToWinding[hookEntityId].IsUnlocked;

        public override void LoadData()
        {
            Static = this;
            if (MySession.Static.CreativeMode)
            {
                MyRopeDefinition ropeDefinition = null;
                foreach (MyRopeDefinition definition2 in MyDefinitionManager.Static.GetRopeDefinitions())
                {
                    if (definition2.IsDefaultCreativeRope)
                    {
                        ropeDefinition = definition2;
                        break;
                    }
                }
                if (ropeDefinition != null)
                {
                    m_ropeAttacher = new MyRopeAttacher(ropeDefinition);
                }
            }
            base.LoadData();
        }

        public static void LockWinding(long hookEntityId)
        {
            WindingData winding = m_hookIdToWinding[hookEntityId];
            if (winding.IsUnlocked)
            {
                long num;
                m_hookIdToUnlockedWinding.Remove(hookEntityId);
                UnlockedWindingData local1 = m_hookIdToUnlockedWinding[hookEntityId];
                MoveSubpart(local1.LeftLock, new Vector3(UNLOCK_OFFSET, 0f, 0f));
                MoveSubpart(local1.RightLock, new Vector3(-UNLOCK_OFFSET, 0f, 0f));
                if (m_hookIdToRopeId.TryGetValue(hookEntityId, out num))
                {
                    MyRope rope;
                    InternalRopeData data2 = m_ropeIdToRope[num];
                    ActivateWinding(hookEntityId, winding, data2.Public.Definition);
                    float maxRopeLength = data2.Public.MaxRopeLength;
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRope>(num, out rope, false))
                    {
                        MyRenderComponentRope render = rope.Render as MyRenderComponentRope;
                        maxRopeLength = (float) (render.WorldPivotB - render.WorldPivotA).Length();
                    }
                    data2.Public.CurrentRopeLength = MathHelper.Clamp(maxRopeLength, data2.Public.MinRopeLength, data2.Public.MaxRopeLength);
                    data2.TargetRopeLength = data2.Public.CurrentRopeLength;
                }
                winding.IsUnlocked = false;
            }
        }

        private static unsafe void MoveSubpart(MyEntitySubpart subpart, Vector3 offset)
        {
            if (subpart != null)
            {
                MyPositionComponentBase positionComp = subpart.PositionComp;
                Matrix localMatrix = positionComp.LocalMatrix;
                Matrix* matrixPtr1 = (Matrix*) ref localMatrix;
                matrixPtr1.Translation += offset;
                positionComp.LocalMatrix = localMatrix;
            }
        }

        private static void RemoveConstraint(InternalRopeData ropeData)
        {
            if (ropeData.Constraint != null)
            {
                if (ropeData.Constraint.RigidBodyA != null)
                {
                    ropeData.Constraint.RigidBodyA.Activate();
                }
                if (ropeData.Constraint.RigidBodyB != null)
                {
                    ropeData.Constraint.RigidBodyB.Activate();
                }
                MyGridPhysics physics = ropeData.GridA.Physics;
                if (physics != null)
                {
                    physics.RemoveConstraint(ropeData.Constraint);
                }
                if (!ropeData.Constraint.IsDisposed)
                {
                    ropeData.Constraint.Dispose();
                }
                ropeData.Constraint = null;
                ropeData.ConstraintData = null;
                MyCubeGrid.BreakGridGroupLink(GridLinkTypeEnum.Physical, ropeData.RopeId, ropeData.GridA, ropeData.GridB);
            }
        }

        public static void RemoveDrumRopeLimits(long hookEntityId)
        {
            m_hookIdToRopeLimits.Remove(hookEntityId);
        }

        public static void RemoveHook(long hookEntityId)
        {
            RemoveRopeOnHookInternal(hookEntityId);
            m_hookIdToHook.Remove(hookEntityId);
            m_hookIdToRelease.Remove(hookEntityId);
            m_hookIdToWinding.Remove(hookEntityId);
            m_hookIdToUnlockedWinding.Remove(hookEntityId);
        }

        public static void RemoveRopeData(long ropeId)
        {
            if (m_ropeIdToRope.ContainsKey(ropeId))
            {
                VRage.Game.Entity.MyEntity entity;
                InternalRopeData ropeData = m_ropeIdToRope[ropeId];
                m_ropeIdToInit.Remove(ropeId);
                m_ropeIdToRope.Remove(ropeId);
                m_ropeIdToRayCastRelease.Remove(ropeId);
                m_hookIdToRopeId.Remove(ropeData.Public.HookEntityIdA);
                m_hookIdToRopeId.Remove(ropeData.Public.HookEntityIdB);
                DeactivateWinding(ropeData.Public.HookEntityIdA, null);
                DeactivateWinding(ropeData.Public.HookEntityIdB, null);
                m_hookIdToActiveRelease.Remove(ropeData.Public.HookEntityIdA);
                m_hookIdToActiveRelease.Remove(ropeData.Public.HookEntityIdB);
                RemoveConstraint(ropeData);
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(ropeData.Public.HookEntityIdA, out entity, false))
                {
                    entity.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(MyRopeComponent.hookEntity_OnClosing);
                }
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(ropeData.Public.HookEntityIdB, out entity, false))
                {
                    entity.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(MyRopeComponent.hookEntity_OnClosing);
                }
                if ((ropeData.GridA != null) && (ropeData.GridB != null))
                {
                    ropeData.GridA.OnPhysicsChanged -= ropeData.HandlePhysicsChanged;
                    if (!ReferenceEquals(ropeData.GridB, ropeData.GridA))
                    {
                        ropeData.GridB.OnPhysicsChanged -= ropeData.HandlePhysicsChanged;
                    }
                }
            }
        }

        private static void RemoveRopeOnHookInternal(long hookEntityId)
        {
            long num;
            if (m_hookIdToRopeId.TryGetValue(hookEntityId, out num))
            {
                if (Sync.IsServer)
                {
                    CloseRopeRequest(num);
                }
                else if (Sandbox.Game.Entities.MyEntities.CloseAllowed)
                {
                    RemoveRopeData(num);
                }
            }
        }

        [Event(null, 0x57c), Reliable, Broadcast]
        public static void SetDrumRopeLimits_Implementation(long hookEntityId, float lengthMin, float lengthMax)
        {
            AddOrSetDrumRopeLimits(hookEntityId, lengthMin, lengthMax);
        }

        public static void SetDrumRopeLimitsRequest(long hookEntityId, float lengthMin, float lengthMax)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, float, float>(x => new Action<long, float, float>(MyRopeComponent.SetDrumRopeLimitsRequest_Implementation), hookEntityId, lengthMin, lengthMax, targetEndpoint, position);
        }

        [Event(null, 0x575), Reliable, Server]
        private static void SetDrumRopeLimitsRequest_Implementation(long hookEntityId, float lengthMin, float lengthMax)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, float, float>(x => new Action<long, float, float>(MyRopeComponent.SetDrumRopeLimits_Implementation), hookEntityId, lengthMin, lengthMax, targetEndpoint, position);
            AddOrSetDrumRopeLimits(hookEntityId, lengthMin, lengthMax);
        }

        public static void SetReleaseData(long hookEntityId, Vector2 orientation, float thresholdAngleCos)
        {
            ReleaseData data;
            if (m_hookIdToRelease.TryGetValue(hookEntityId, out data))
            {
                data.Orientation = orientation;
                data.ThresholdAngleCos = thresholdAngleCos;
                ComputeLocalReleaseAxis(data.LocalBaseAxis, data.Orientation, out data.LocalAxis);
            }
        }

        [Event(null, 0x56a), Reliable, Broadcast]
        public static void SetReleaseData_Implementation(long hookEntityId, Vector2 orientation, float thresholdCos)
        {
            SetReleaseData(hookEntityId, orientation, thresholdCos);
        }

        public static void SetReleaseDataRequest(long hookEntityId, Vector2 orientation, float thresholdCos)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector2, float>(x => new Action<long, Vector2, float>(MyRopeComponent.SetReleaseDataRequest_Implementation), hookEntityId, orientation, thresholdCos, targetEndpoint, position);
        }

        [Event(null, 0x563), Reliable, Server]
        private static void SetReleaseDataRequest_Implementation(long hookEntityId, Vector2 orientation, float thresholdCos)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector2, float>(x => new Action<long, Vector2, float>(MyRopeComponent.SetReleaseData_Implementation), hookEntityId, orientation, thresholdCos, targetEndpoint, position);
            SetReleaseData(hookEntityId, orientation, thresholdCos);
        }

        public static void SetRopeData(long ropeEntityId, float minRopeLength, float maxRopeLength)
        {
            InternalRopeData ropeData = m_ropeIdToRope[ropeEntityId];
            ropeData.Public.MinRopeLength = minRopeLength;
            ropeData.Public.MaxRopeLength = maxRopeLength;
            ApplyRopeLimits(ropeData, null);
        }

        public void SetRopeLengthSynced(long ropeId, float currentLength)
        {
            InternalRopeData data;
            if (m_ropeIdToRope.TryGetValue(ropeId, out data))
            {
                data.Public.CurrentRopeLength = currentLength;
                data.TargetRopeLength = currentLength;
                if (data.ConstraintData != null)
                {
                    data.ConstraintData.LinearLimit = data.Public.CurrentRopeLength;
                }
                if (data.Constraint != null)
                {
                    data.Constraint.RigidBodyA.Activate();
                    data.Constraint.RigidBodyB.Activate();
                }
            }
        }

        public static bool TryGetRopeData(long ropeEntityId, out MyRopeData ropeData)
        {
            InternalRopeData data;
            ropeData = new MyRopeData();
            if (!m_ropeIdToRope.TryGetValue(ropeEntityId, out data))
            {
                return false;
            }
            ropeData = data.Public;
            return true;
        }

        public static bool TryGetRopeForHook(long hookEntityId, out long ropeEntityId) => 
            m_hookIdToRopeId.TryGetValue(hookEntityId, out ropeEntityId);

        public static void TryRemoveRopeOnHook(long hookEntityId)
        {
            long num;
            if (m_hookIdToRopeId.TryGetValue(hookEntityId, out num))
            {
                CloseRopeRequest(num);
            }
        }

        protected override void UnloadData()
        {
            m_ropeIdToRope.Clear();
            m_hookIdToHook.Clear();
            m_hookIdToWinding.Clear();
            m_hookIdToRelease.Clear();
            m_hookIdToRopeId.Clear();
            m_ropesToRemove.Clear();
            foreach (WindingData data in m_hookIdToActiveWinding.Values)
            {
                if (data.Emitter != null)
                {
                    data.Emitter.StopSound(true, true);
                }
            }
            m_hookIdToActiveWinding.Clear();
            m_hookIdToActiveRelease.Clear();
            m_ropeIdToRayCastRelease.Clear();
            m_hookIdToRopeLimits.Clear();
            m_hookIdToUnlockedWinding.Clear();
            m_ropeIdToInit.Clear();
            if (m_ropeAttacher != null)
            {
                m_ropeAttacher.Clear();
                m_ropeAttacher = null;
            }
            Static = null;
            base.UnloadData();
        }

        public static void UnlockWinding(long hookEntityId)
        {
            WindingData windingData = m_hookIdToWinding[hookEntityId];
            if (!windingData.IsUnlocked)
            {
                long num;
                UnlockedWindingData data2 = new UnlockedWindingData();
                if (m_hookIdToRopeId.TryGetValue(hookEntityId, out num))
                {
                    MyRope rope;
                    InternalRopeData ropeData = m_ropeIdToRope[num];
                    ropeData.TargetRopeLength = ropeData.Public.CurrentRopeLength = ropeData.Public.MaxRopeLength;
                    if (ropeData.Constraint != null)
                    {
                        ropeData.ConstraintData.LinearLimit = ropeData.Public.CurrentRopeLength;
                        if (ropeData.Constraint.RigidBodyA != null)
                        {
                            ropeData.Constraint.RigidBodyA.Activate();
                        }
                        if (ropeData.Constraint.RigidBodyB != null)
                        {
                            ropeData.Constraint.RigidBodyB.Activate();
                        }
                    }
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRope>(num, out rope, false))
                    {
                        MyRenderComponentRope render = rope.Render as MyRenderComponentRope;
                        MyCubeGrid grid = (ropeData.Public.HookEntityIdA != hookEntityId) ? ropeData.GridB : ropeData.GridA;
                        MyPhysicsBody physics = grid?.Physics;
                        if (physics != null)
                        {
                            data2.AngularVelocity = ComputeTorqueFromRopeImpulse(windingData, ropeData, (Vector3) (render.WorldPivotB - render.WorldPivotA), (Vector3) (windingData.CurrentDummyWorld.Translation - physics.CenterOfMassWorld)).Length();
                        }
                    }
                }
                DeactivateWinding(hookEntityId, windingData);
                Dictionary<string, MyEntitySubpart> subparts = Sandbox.Game.Entities.MyEntities.GetEntityById(hookEntityId, false).Subparts;
                subparts.TryGetValue("LeftLock", out data2.LeftLock);
                subparts.TryGetValue("RightLock", out data2.RightLock);
                subparts.TryGetValue("Drum", out data2.Drum);
                m_hookIdToUnlockedWinding.Add(hookEntityId, data2);
                MoveSubpart(data2.LeftLock, new Vector3(-UNLOCK_OFFSET, 0f, 0f));
                MoveSubpart(data2.RightLock, new Vector3(UNLOCK_OFFSET, 0f, 0f));
                windingData.IsUnlocked = true;
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            foreach (KeyValuePair<long, InternalRopeData> pair in m_ropeIdToRope)
            {
                VRage.Game.Entity.MyEntity entity;
                VRage.Game.Entity.MyEntity entity2;
                InternalRopeData data = pair.Value;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(data.Public.HookEntityIdA, out entity, false) && ((entity != null) && (Sandbox.Game.Entities.MyEntities.TryGetEntityById(data.Public.HookEntityIdB, out entity2, false) && (entity2 != null))))
                {
                    MyRope rope;
                    Vector3D vectord = Vector3D.Transform(m_hookIdToHook[data.Public.HookEntityIdA].LocalPivot, entity.WorldMatrix);
                    Vector3D vectord2 = Vector3D.Transform(m_hookIdToHook[data.Public.HookEntityIdB].LocalPivot, entity2.WorldMatrix);
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRope>(data.RopeId, out rope, false))
                    {
                        MyRenderComponentRope render = rope.Render as MyRenderComponentRope;
                        if ((vectord != render.WorldPivotA) || (vectord2 != render.WorldPivotB))
                        {
                            render.WorldPivotA = vectord;
                            render.WorldPivotB = vectord2;
                            Vector3D vectord3 = (vectord + vectord2) * 0.5;
                            Vector3 point = (Vector3) (vectord - vectord3);
                            Vector3 vector2 = (Vector3) (vectord2 - vectord3);
                            BoundingBox box = BoundingBox.CreateInvalid();
                            box.Include(ref point);
                            box.Include(ref vector2);
                            box.Inflate((float) 0.25f);
                            rope.PositionComp.LocalAABB = box;
                            MatrixD worldMatrix = rope.PositionComp.WorldMatrix;
                            worldMatrix.Translation = vectord3;
                            rope.PositionComp.SetWorldMatrix(worldMatrix, null, true, true, true, false, false, false);
                        }
                    }
                }
            }
            foreach (KeyValuePair<long, UnlockedWindingData> pair2 in m_hookIdToUnlockedWinding)
            {
                long key = pair2.Key;
                UnlockedWindingData data3 = pair2.Value;
                if ((data3.Drum != null) && Sandbox.Game.Entities.MyEntities.EntityExists(key))
                {
                    MyPositionComponentBase positionComp = data3.Drum.PositionComp;
                    if (data3.AngularVelocity > 0f)
                    {
                        positionComp.LocalMatrix *= Matrix.CreateRotationX(data3.AngularVelocity * 0.01666667f);
                        data3.AngularVelocity -= 0.1f + (data3.AngularVelocity * 0.1f);
                        if (data3.AngularVelocity < 0f)
                        {
                            data3.AngularVelocity = 0f;
                        }
                    }
                    WindingData winding = m_hookIdToWinding[key];
                    UpdateDummyWorld(Sandbox.Game.Entities.MyEntities.GetEntityById(key, false).PositionComp, winding);
                    UpdateWindingAngleDelta(winding);
                    positionComp.LocalMatrix *= Matrix.CreateRotationX(-winding.AngleDelta);
                }
            }
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_ROPES)
            {
                foreach (KeyValuePair<long, WindingData> pair3 in m_hookIdToWinding)
                {
                    WindingData data5 = pair3.Value;
                    Matrix worldMatrix = Matrix.Multiply(data5.LocalDummy, (Matrix) Sandbox.Game.Entities.MyEntities.GetEntityById(pair3.Key, false).WorldMatrix);
                    MyRenderProxy.DebugDrawCylinder(worldMatrix, (Vector3D) (-0.5 * Vector3D.UnitZ), (Vector3D) (0.5 * Vector3D.UnitZ), data5.Radius, Color.White, 1f, true, false, false);
                    MyRenderProxy.DebugDrawAxis(worldMatrix, 1f, false, false, false);
                }
                foreach (KeyValuePair<long, ReleaseData> pair4 in m_hookIdToRelease)
                {
                    VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(pair4.Key, false);
                    Vector3 v = Vector3.TransformNormal(pair4.Value.LocalAxis, entityById.WorldMatrix);
                    Vector3D baseVec = Vector3D.CalculatePerpendicularVector(v) * Math.Sin(Math.Acos((double) pair4.Value.ThresholdAngleCos));
                    v *= pair4.Value.ThresholdAngleCos;
                    MyRenderProxy.DebugDrawCone(Vector3.Transform(m_hookIdToHook[pair4.Key].LocalPivot, entityById.WorldMatrix) + v, -v, baseVec, Color.White, true, false);
                }
                foreach (KeyValuePair<long, InternalRopeData> pair5 in m_ropeIdToRope)
                {
                    MyRope rope3;
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRope>(pair5.Value.RopeId, out rope3, false))
                    {
                        MyRenderComponentRope render = rope3.Render as MyRenderComponentRope;
                        Vector3D vectord5 = Vector3D.Normalize(render.WorldPivotA - render.WorldPivotB);
                        Vector3D position = rope3.PositionComp.GetPosition();
                        MyRenderProxy.DebugDrawLine3D(position - ((0.5 * vectord5) * pair5.Value.Public.CurrentRopeLength), position + ((0.5 * vectord5) * pair5.Value.Public.CurrentRopeLength), Color.Red, Color.Green, false, false);
                        MyRenderProxy.DebugDrawText3D(position, $"Impulse: {pair5.Value.ImpulseApplied.ToString("#.00")}, Min: {pair5.Value.Public.MinRopeLength.ToString("#.00")}, Max: {pair5.Value.Public.MaxRopeLength.ToString("#.00")}, Current: {pair5.Value.Public.CurrentRopeLength.ToString("#.00")}", Color.White, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1, false);
                    }
                }
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            foreach (long num in m_ropeIdToInit)
            {
                InternalRopeData data;
                HookData data2;
                HookData data3;
                VRage.Game.Entity.MyEntity entity;
                VRage.Game.Entity.MyEntity entity2;
                if (!m_ropeIdToRope.TryGetValue(num, out data))
                {
                    continue;
                }
                if ((data != null) && (m_hookIdToHook.TryGetValue(data.Public.HookEntityIdA, out data2) && ((data2 != null) && (m_hookIdToHook.TryGetValue(data.Public.HookEntityIdB, out data3) && ((data3 != null) && (Sandbox.Game.Entities.MyEntities.TryGetEntityById(data.Public.HookEntityIdA, out entity, false) && ((entity != null) && (Sandbox.Game.Entities.MyEntities.TryGetEntityById(data.Public.HookEntityIdB, out entity2, false) && (entity2 != null)))))))))
                {
                    WindingData data4;
                    data.GridA = (MyCubeGrid) entity.Parent;
                    data.GridB = (MyCubeGrid) entity2.Parent;
                    if (data.Public.MinRopeLength == 0f)
                    {
                        data.Public.MinRopeLength = (data2.Size + data3.Size) * 0.85f;
                        data.Public.MinRopeLengthFromDummySizes = data.Public.MinRopeLength;
                        data.Public.MinRopeLengthStatic = null;
                        if (data.IsStaticConnection())
                        {
                            float num2 = (float) Vector3D.Distance(Vector3D.Transform(data2.LocalPivot, entity.WorldMatrix), Vector3D.Transform(data3.LocalPivot, entity2.WorldMatrix));
                            data.Public.MinRopeLength = Math.Max(data.Public.MinRopeLength, num2);
                            data.Public.MinRopeLengthStatic = new float?(data.Public.MinRopeLength);
                        }
                    }
                    ApplyRopeLimits(data, null);
                    data.GridA.OnPhysicsChanged += data.HandlePhysicsChanged;
                    if (!ReferenceEquals(data.GridB, data.GridA))
                    {
                        data.GridB.OnPhysicsChanged += data.HandlePhysicsChanged;
                    }
                    entity.OnClosing += new Action<VRage.Game.Entity.MyEntity>(MyRopeComponent.hookEntity_OnClosing);
                    entity2.OnClosing += new Action<VRage.Game.Entity.MyEntity>(MyRopeComponent.hookEntity_OnClosing);
                    if (!Sync.IsServer)
                    {
                        if (!Sandbox.Game.Entities.MyEntities.EntityExists(num))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        MyRope rope;
                        if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRope>(num, out rope, false))
                        {
                            rope = new MyRope {
                                EntityId = num
                            };
                            Sandbox.Game.Entities.MyEntities.RaiseEntityCreated(rope);
                            Sandbox.Game.Entities.MyEntities.Add(rope, true);
                        }
                    }
                    CreateConstraint(data);
                    if (m_hookIdToWinding.TryGetValue(data.Public.HookEntityIdA, out data4) && !data4.IsUnlocked)
                    {
                        ActivateWinding(data.Public.HookEntityIdA, data4, data.Public.Definition);
                    }
                    if (m_hookIdToWinding.TryGetValue(data.Public.HookEntityIdB, out data4) && !data4.IsUnlocked)
                    {
                        ActivateWinding(data.Public.HookEntityIdB, data4, data.Public.Definition);
                    }
                    ActivateRelease(data.Public.HookEntityIdA);
                    ActivateRelease(data.Public.HookEntityIdB);
                    m_tmpRopesInitialized.Add(num);
                }
            }
            foreach (long num3 in m_tmpRopesInitialized)
            {
                m_ropeIdToInit.Remove(num3);
            }
            m_tmpRopesInitialized.Clear();
            foreach (KeyValuePair<long, WindingData> pair in m_hookIdToActiveWinding)
            {
                VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(pair.Key, false);
                if (entityById != null)
                {
                    UpdateDummyWorld(entityById.PositionComp, pair.Value);
                    WindingData winding = pair.Value;
                    UpdateWindingAngleDelta(winding);
                    InternalRopeData local1 = m_ropeIdToRope[m_hookIdToRopeId[pair.Key]];
                    local1.TargetRopeLength += winding.AngleDelta * winding.Radius;
                    if (winding.Sound != null)
                    {
                        bool flag = (winding.AngleDelta < -0.0001f) || (0.0001f < winding.AngleDelta);
                        if (flag != winding.Emitter.IsPlaying)
                        {
                            if (!flag)
                            {
                                winding.Emitter.StopSound(false, true);
                                continue;
                            }
                            bool? nullable = null;
                            winding.Emitter.PlaySound(winding.Sound, false, false, false, false, false, nullable);
                        }
                    }
                }
            }
            if (Sync.IsServer)
            {
                foreach (KeyValuePair<long, ReleaseData> pair2 in m_hookIdToActiveRelease)
                {
                    VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(pair2.Key, false);
                    if (entityById != null)
                    {
                        MyRope rope2;
                        long entityId = m_hookIdToRopeId[pair2.Key];
                        InternalRopeData data6 = m_ropeIdToRope[entityId];
                        if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRope>(entityId, out rope2, false) && (rope2 != null))
                        {
                            MyRenderComponentRope render = rope2.Render as MyRenderComponentRope;
                            Vector3 vector = Vector3.Normalize(render.WorldPivotB - render.WorldPivotA);
                            if (pair2.Key == data6.Public.HookEntityIdB)
                            {
                                vector *= -1f;
                            }
                            if (Vector3.Dot(Vector3.TransformNormal(pair2.Value.LocalAxis, entityById.WorldMatrix), vector) > pair2.Value.ThresholdAngleCos)
                            {
                                m_ropesToRemove.Add(entityId);
                            }
                        }
                    }
                }
                foreach (KeyValuePair<long, InternalRopeData> pair3 in m_ropeIdToRayCastRelease)
                {
                    MyRope rope4;
                    InternalRopeData data7 = pair3.Value;
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRope>(data7.RopeId, out rope4, false) && (rope4 != null))
                    {
                        MyRenderComponentRope render = (MyRenderComponentRope) rope4.Render;
                        using (m_hitInfoBuffer.GetClearToken<MyPhysics.HitInfo>())
                        {
                            MyPhysics.CastRay(render.WorldPivotA, render.WorldPivotB, m_hitInfoBuffer, 0);
                            using (List<MyPhysics.HitInfo>.Enumerator enumerator6 = m_hitInfoBuffer.GetEnumerator())
                            {
                                while (enumerator6.MoveNext())
                                {
                                    IMyEntity hitEntity = enumerator6.Current.HkHitInfo.GetHitEntity();
                                    if (!ReferenceEquals(hitEntity, data7.GridA) && ((hitEntity.EntityId != data7.Public.HookEntityIdA) && (!ReferenceEquals(hitEntity, data7.GridB) && (hitEntity.EntityId != data7.Public.HookEntityIdB))))
                                    {
                                        m_ropesToRemove.Add(data7.RopeId);
                                    }
                                }
                            }
                        }
                    }
                }
                using (HashSet<long>.Enumerator enumerator = m_ropesToRemove.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        CloseRopeRequest(enumerator.Current);
                    }
                }
                m_ropesToRemove.Clear();
            }
            using (Dictionary<long, InternalRopeData>.ValueCollection.Enumerator enumerator7 = m_ropeIdToRope.Values.GetEnumerator())
            {
                InternalRopeData current;
                HkConstraint constraint;
                MyRenderComponentRope render;
                float num5;
                float num6;
                goto TR_0021;
            TR_0010:
                if (!constraint.InWorld)
                {
                    RemoveConstraint(current);
                    constraint = null;
                    CreateConstraint(current);
                    constraint = current.Constraint;
                    if (constraint == null)
                    {
                        goto TR_0021;
                    }
                }
                if (constraint.InWorld)
                {
                    current.ImpulseApplied = current.ConstraintData.Update(constraint);
                }
                goto TR_0021;
            TR_0011:
                num6 = (float) (render.WorldPivotA - render.WorldPivotB).Length();
                current.Public.CurrentRopeLength = num6 + Math.Max((float) (MathHelper.Clamp(current.Public.CurrentRopeLength + num5, current.Public.MinRopeLength, current.Public.MaxRopeLength) - num6), (float) -0.1f);
                current.TargetRopeLength = current.Public.CurrentRopeLength;
                current.ConstraintData.LinearLimit = current.Public.CurrentRopeLength;
                constraint.RigidBodyA.Activate();
                constraint.RigidBodyB.Activate();
                goto TR_0010;
            TR_0021:
                while (true)
                {
                    if (enumerator7.MoveNext())
                    {
                        MyRope rope6;
                        current = enumerator7.Current;
                        if (current.Constraint == null)
                        {
                            continue;
                        }
                        constraint = current.Constraint;
                        if ((constraint.RigidBodyA == null) || (constraint.RigidBodyB == null))
                        {
                            RemoveConstraint(current);
                            CreateConstraint(current);
                            constraint = current.Constraint;
                        }
                        if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRope>(current.RopeId, out rope6, false))
                        {
                            goto TR_0010;
                        }
                        else if (rope6 == null)
                        {
                            goto TR_0010;
                        }
                        else
                        {
                            render = rope6.Render as MyRenderComponentRope;
                            num5 = current.TargetRopeLength - current.Public.CurrentRopeLength;
                            if (((num5 >= -0.001f) || (current.Public.CurrentRopeLength <= current.Public.MinRopeLength)) && ((num5 <= 0.001f) || (current.Public.CurrentRopeLength >= current.Public.MaxRopeLength)))
                            {
                                current.TargetRopeLength = current.Public.CurrentRopeLength;
                                goto TR_0010;
                            }
                        }
                    }
                    else
                    {
                        goto TR_000A;
                    }
                    break;
                }
                goto TR_0011;
            }
        TR_000A:
            if (MyFakes.ENABLE_ROPE_UNWINDING_TORQUE)
            {
                foreach (KeyValuePair<long, WindingData> pair4 in m_hookIdToActiveWinding)
                {
                    MyRope rope8;
                    WindingData windingData = pair4.Value;
                    InternalRopeData ropeData = m_ropeIdToRope[m_hookIdToRopeId[pair4.Key]];
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyRope>(ropeData.RopeId, out rope8, false) && (rope8 != null))
                    {
                        MyRenderComponentRope render = rope8.Render as MyRenderComponentRope;
                        MyPhysicsBody body = (ropeData.Public.HookEntityIdA != pair4.Key) ? ropeData.GridB.Physics : ropeData.GridA.Physics;
                        Vector3? force = null;
                        Vector3D? position = null;
                        float? maxSpeed = null;
                        body.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, force, position, new Vector3?(ComputeTorqueFromRopeImpulse(windingData, ropeData, (Vector3) (render.WorldPivotB - render.WorldPivotA), (Vector3) (windingData.CurrentDummyWorld.Translation - body.CenterOfMassWorld)) * -0.75f), maxSpeed, true, false);
                    }
                }
            }
        }

        private static void UpdateDummyWorld(MyPositionComponentBase windingEntityPositionComponent, WindingData winding)
        {
            winding.LastDummyWorld = winding.CurrentDummyWorld;
            winding.CurrentDummyWorld = MatrixD.Multiply(winding.LocalDummy, windingEntityPositionComponent.WorldMatrix);
        }

        private static void UpdateWindingAngleDelta(WindingData winding)
        {
            double y = winding.CurrentDummyWorld.Right.Dot(winding.LastDummyWorld.Up);
            winding.AngleDelta = (float) Math.Atan2(y, winding.CurrentDummyWorld.Right.Dot(winding.LastDummyWorld.Right));
        }

        public static MyRopeAttacher RopeAttacher =>
            m_ropeAttacher;

        public override bool IsRequiredByGame =>
            (MyPerGameSettings.Game == GameEnum.ME_GAME);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyRopeComponent.<>c <>9 = new MyRopeComponent.<>c();
            public static Func<IMyEventOwner, Action<long, long, DefinitionIdBlit>> <>9__77_0;
            public static Func<IMyEventOwner, Action<long>> <>9__79_0;
            public static Func<IMyEventOwner, Action<long, Vector2, float>> <>9__81_0;
            public static Func<IMyEventOwner, Action<long, Vector2, float>> <>9__82_0;
            public static Func<IMyEventOwner, Action<long, float, float>> <>9__84_0;
            public static Func<IMyEventOwner, Action<long, float, float>> <>9__85_0;

            internal Action<long, long, DefinitionIdBlit> <AddRopeRequest>b__77_0(IMyEventOwner x) => 
                new Action<long, long, DefinitionIdBlit>(MyRopeComponent.AddRopeRequest_Implementation);

            internal Action<long> <CloseRopeRequest>b__79_0(IMyEventOwner x) => 
                new Action<long>(MyRopeComponent.CloseRopeRequest_Implementation);

            internal Action<long, float, float> <SetDrumRopeLimitsRequest_Implementation>b__85_0(IMyEventOwner x) => 
                new Action<long, float, float>(MyRopeComponent.SetDrumRopeLimits_Implementation);

            internal Action<long, float, float> <SetDrumRopeLimitsRequest>b__84_0(IMyEventOwner x) => 
                new Action<long, float, float>(MyRopeComponent.SetDrumRopeLimitsRequest_Implementation);

            internal Action<long, Vector2, float> <SetReleaseDataRequest_Implementation>b__82_0(IMyEventOwner x) => 
                new Action<long, Vector2, float>(MyRopeComponent.SetReleaseData_Implementation);

            internal Action<long, Vector2, float> <SetReleaseDataRequest>b__81_0(IMyEventOwner x) => 
                new Action<long, Vector2, float>(MyRopeComponent.SetReleaseDataRequest_Implementation);
        }

        private sealed class HookData
        {
            public readonly float Size;
            public readonly Vector3 LocalPivot;

            public HookData(float size, Vector3 localPivot)
            {
                this.Size = size;
                this.LocalPivot = localPivot;
            }
        }

        private sealed class InternalRopeData
        {
            public MyRopeData Public;
            public float TargetRopeLength;
            public HkRopeConstraintData ConstraintData;
            public HkConstraint Constraint;
            public long RopeId;
            public MyCubeGrid GridA;
            public MyCubeGrid GridB;
            public float ImpulseApplied;
            public readonly Action<VRage.Game.Entity.MyEntity> HandlePhysicsChanged;

            public InternalRopeData()
            {
                this.HandlePhysicsChanged = a => this.HandleChange();
            }

            private void HandleChange()
            {
                VRage.Game.Entity.MyEntity entity;
                if ((this.GridA != null) && (this.GridB != null))
                {
                    MyRopeComponent.RemoveConstraint(this);
                    this.GridA.OnPhysicsChanged -= this.HandlePhysicsChanged;
                    if (!ReferenceEquals(this.GridB, this.GridA))
                    {
                        this.GridB.OnPhysicsChanged -= this.HandlePhysicsChanged;
                    }
                }
                this.GridA = null;
                this.GridB = null;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(this.Public.HookEntityIdA, out entity, false))
                {
                    this.GridA = (MyCubeGrid) entity.Parent;
                }
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(this.Public.HookEntityIdB, out entity, false))
                {
                    this.GridB = (MyCubeGrid) entity.Parent;
                }
                if ((this.GridA != null) && (this.GridB != null))
                {
                    if (((this.Public.MinRopeLengthStatic != null) && !this.IsStaticConnection()) && (this.Public.MinRopeLength == this.Public.MinRopeLengthStatic.Value))
                    {
                        this.Public.MinRopeLength = this.Public.MinRopeLengthFromDummySizes;
                    }
                    this.GridA.OnPhysicsChanged += this.HandlePhysicsChanged;
                    if (!ReferenceEquals(this.GridB, this.GridA))
                    {
                        this.GridB.OnPhysicsChanged += this.HandlePhysicsChanged;
                    }
                    MyRopeComponent.CreateConstraint(this);
                }
            }

            private static bool IsGridGroupStatic(MyCubeGrid grid)
            {
                List<MyCubeGrid> groupNodes = MyCubeGridGroups.Static.GetGroups(GridLinkTypeEnum.Logical).GetGroupNodes(grid);
                if (groupNodes == null)
                {
                    return grid.IsStatic;
                }
                using (List<MyCubeGrid>.Enumerator enumerator = groupNodes.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (enumerator.Current.IsStatic)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public bool IsStaticConnection()
            {
                if ((this.GridA == null) || (this.GridB == null))
                {
                    return false;
                }
                if (!this.GridA.IsStatic || !IsGridGroupStatic(this.GridB))
                {
                    return (this.GridB.IsStatic && IsGridGroupStatic(this.GridA));
                }
                return true;
            }
        }

        private sealed class ReleaseData
        {
            public readonly Vector3 LocalBaseAxis;
            public Vector2 Orientation;
            public float ThresholdAngleCos;
            public Vector3 LocalAxis;

            public ReleaseData(Vector3 localBaseAxis, float thresholdAngleCos)
            {
                this.LocalAxis = this.LocalBaseAxis = localBaseAxis;
                this.ThresholdAngleCos = thresholdAngleCos;
            }
        }

        private sealed class RopeDrumLimits
        {
            public float MinLength;
            public float MaxLength;
        }

        private sealed class UnlockedWindingData
        {
            public MyEntitySubpart LeftLock;
            public MyEntitySubpart RightLock;
            public MyEntitySubpart Drum;
            public float AngularVelocity;
        }

        private sealed class WindingData
        {
            public readonly float Radius;
            public readonly Matrix LocalDummy;
            public MatrixD LastDummyWorld;
            public MatrixD CurrentDummyWorld;
            public bool IsUnlocked;
            public float AngleDelta;
            public MySoundPair Sound;
            public MyEntity3DSoundEmitter Emitter;

            public WindingData(float radius, ref Matrix localDummy)
            {
                this.Radius = radius;
                this.LocalDummy = localDummy;
            }
        }
    }
}

