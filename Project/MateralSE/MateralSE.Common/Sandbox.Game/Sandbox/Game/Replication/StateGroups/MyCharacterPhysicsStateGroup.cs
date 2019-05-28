namespace Sandbox.Game.Replication.StateGroups
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.History;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRageMath;

    public class MyCharacterPhysicsStateGroup : MyEntityPhysicsStateGroupBase
    {
        public static MyTimeSpan ParentChangeTimeOut = MyTimeSpan.FromMilliseconds(100.0);
        public static readonly MyEntityPhysicsStateGroupBase.ParentingSetup JetpackParentingSetup;
        private const float FallMaxParentDisconnectDistance = 10f;
        private readonly List<MyEntity> m_tmpEntityResults;
        private MyTimeSpan m_lastTimestamp;
        private static readonly MyPredictedSnapshotSyncSetup m_controlledJetPackSettings;
        private static readonly MyPredictedSnapshotSyncSetup m_controlledJetPackMovingSettings;
        private static readonly MyPredictedSnapshotSyncSetup m_controlledJetPackNewParentSettings;
        private static readonly MyPredictedSnapshotSyncSetup m_controlledSettings;
        private static readonly MyPredictedSnapshotSyncSetup m_deadSettings;
        private static readonly MyPredictedSnapshotSyncSetup m_controlledAnimatedSettings;
        private static readonly MyPredictedSnapshotSyncSetup m_controlledMovingSettings;
        private static readonly MyPredictedSnapshotSyncSetup m_controlledNewParentSettings;
        private static readonly MyPredictedSnapshotSyncSetup m_settings;
        private static readonly MyPredictedSnapshotSyncSetup m_controlledLadderSettings;
        private readonly MyPredictedSnapshotSync m_predictedSync;
        private readonly IMySnapshotSync m_animatedSync;
        private long m_lastParentId;
        private MyTimeSpan m_lastParentTime;
        private MyTimeSpan m_lastAnimatedTime;
        private byte m_syncLinearVelocity;
        private const float NEW_PARENT_TIMEOUT = 3f;
        private const float SYNC_CHANGE_TIMEOUT = 0.1f;
        public static float EXCESSIVE_CORRECTION_THRESHOLD;

        static MyCharacterPhysicsStateGroup()
        {
            MyEntityPhysicsStateGroupBase.ParentingSetup setup1 = new MyEntityPhysicsStateGroupBase.ParentingSetup();
            setup1.MaxParentDistance = 100f;
            setup1.MinParentSpeed = 20f;
            setup1.MaxParentAcceleration = 6f;
            setup1.MinInsideParentSpeed = 20f;
            setup1.MaxParentDisconnectDistance = 100f;
            setup1.MinDisconnectParentSpeed = 15f;
            setup1.MaxDisconnectParentAcceleration = 30f;
            setup1.MinDisconnectInsideParentSpeed = 10f;
            JetpackParentingSetup = setup1;
            MyPredictedSnapshotSyncSetup setup2 = new MyPredictedSnapshotSyncSetup();
            setup2.ProfileName = "ControlledJetpack";
            setup2.ApplyPosition = true;
            setup2.ApplyRotation = false;
            setup2.ApplyPhysicsAngular = false;
            setup2.ApplyPhysicsLinear = true;
            setup2.ExtrapolationSmoothing = true;
            setup2.InheritRotation = false;
            setup2.ApplyPhysicsLocal = true;
            setup2.IsControlled = true;
            setup2.AllowForceStop = true;
            setup2.MaxPositionFactor = 100f;
            setup2.MinPositionFactor = 100f;
            setup2.MaxLinearFactor = 1f;
            setup2.MaxRotationFactor = 1f;
            setup2.MaxAngularFactor = 1f;
            setup2.IterationsFactor = 0.3f;
            setup2.IgnoreParentId = true;
            setup2.UserTrend = true;
            m_controlledJetPackSettings = setup2;
            MyPredictedSnapshotSyncSetup setup3 = new MyPredictedSnapshotSyncSetup();
            setup3.ProfileName = "ControlledJetpackMoving";
            setup3.ApplyPosition = true;
            setup3.ApplyRotation = false;
            setup3.ApplyPhysicsAngular = false;
            setup3.ApplyPhysicsLinear = true;
            setup3.ExtrapolationSmoothing = true;
            setup3.InheritRotation = false;
            setup3.ApplyPhysicsLocal = true;
            setup3.IsControlled = true;
            setup3.AllowForceStop = true;
            setup3.MaxPositionFactor = 100f;
            setup3.MaxLinearFactor = 1f;
            setup3.MaxRotationFactor = 1f;
            setup3.MaxAngularFactor = 1f;
            setup3.IterationsFactor = 0.3f;
            setup3.IgnoreParentId = true;
            setup3.UserTrend = true;
            m_controlledJetPackMovingSettings = setup3;
            MyPredictedSnapshotSyncSetup setup4 = new MyPredictedSnapshotSyncSetup();
            setup4.ProfileName = "ControlledJetpackNewParent";
            setup4.ApplyPosition = true;
            setup4.ApplyRotation = false;
            setup4.ApplyPhysicsAngular = false;
            setup4.ApplyPhysicsLinear = true;
            setup4.ExtrapolationSmoothing = true;
            setup4.InheritRotation = false;
            setup4.ApplyPhysicsLocal = true;
            setup4.IsControlled = true;
            setup4.AllowForceStop = true;
            setup4.MaxPositionFactor = 100f;
            setup4.MaxLinearFactor = 1f;
            setup4.MaxRotationFactor = 1f;
            setup4.MaxAngularFactor = 1f;
            setup4.IterationsFactor = 1.5f;
            setup4.IgnoreParentId = true;
            setup4.UserTrend = true;
            m_controlledJetPackNewParentSettings = setup4;
            MyPredictedSnapshotSyncSetup setup5 = new MyPredictedSnapshotSyncSetup();
            setup5.ProfileName = "ControlledCharacter";
            setup5.ApplyPosition = true;
            setup5.ApplyRotation = false;
            setup5.ApplyPhysicsAngular = false;
            setup5.ApplyPhysicsLinear = false;
            setup5.ExtrapolationSmoothing = true;
            setup5.InheritRotation = true;
            setup5.ApplyPhysicsLocal = true;
            setup5.IsControlled = true;
            setup5.AllowForceStop = true;
            setup5.MinPositionFactor = 100f;
            setup5.MaxPositionFactor = 10f;
            setup5.MaxLinearFactor = 100f;
            setup5.MaxRotationFactor = 100f;
            setup5.MaxAngularFactor = 1f;
            setup5.IterationsFactor = 0.3f;
            setup5.IgnoreParentId = true;
            setup5.UserTrend = true;
            m_controlledSettings = setup5;
            MyPredictedSnapshotSyncSetup setup6 = new MyPredictedSnapshotSyncSetup();
            setup6.ProfileName = "DeadCharacter";
            setup6.ApplyPosition = false;
            setup6.ApplyRotation = false;
            setup6.ApplyPhysicsAngular = false;
            setup6.ApplyPhysicsLinear = false;
            setup6.ExtrapolationSmoothing = true;
            setup6.InheritRotation = true;
            setup6.ApplyPhysicsLocal = true;
            setup6.IsControlled = true;
            setup6.AllowForceStop = true;
            setup6.MinPositionFactor = 100f;
            setup6.MaxPositionFactor = 100f;
            setup6.MaxLinearFactor = 100f;
            setup6.MaxRotationFactor = 100f;
            setup6.MaxAngularFactor = 1f;
            setup6.IterationsFactor = 0.3f;
            setup6.IgnoreParentId = true;
            setup6.UserTrend = false;
            m_deadSettings = setup6;
            MyPredictedSnapshotSyncSetup setup7 = new MyPredictedSnapshotSyncSetup();
            setup7.ProfileName = "ControlledAnimatedCharacter";
            setup7.ApplyPosition = true;
            setup7.ApplyRotation = false;
            setup7.ApplyPhysicsAngular = true;
            setup7.ApplyPhysicsLinear = true;
            setup7.ExtrapolationSmoothing = true;
            setup7.InheritRotation = true;
            setup7.ApplyPhysicsLocal = true;
            setup7.IsControlled = true;
            setup7.AllowForceStop = true;
            setup7.MinPositionFactor = 100f;
            setup7.MaxPositionFactor = 10f;
            setup7.MaxLinearFactor = 100f;
            setup7.MaxRotationFactor = 100f;
            setup7.MaxAngularFactor = 1f;
            setup7.IterationsFactor = 0.3f;
            setup7.IgnoreParentId = true;
            setup7.UserTrend = true;
            m_controlledAnimatedSettings = setup7;
            MyPredictedSnapshotSyncSetup setup8 = new MyPredictedSnapshotSyncSetup();
            setup8.ProfileName = "ControlledCharacterMoving";
            setup8.ApplyPosition = true;
            setup8.ApplyRotation = false;
            setup8.ApplyPhysicsAngular = false;
            setup8.ApplyPhysicsLinear = false;
            setup8.ExtrapolationSmoothing = true;
            setup8.InheritRotation = true;
            setup8.ApplyPhysicsLocal = true;
            setup8.IsControlled = true;
            setup8.AllowForceStop = true;
            setup8.MaxPositionFactor = 10f;
            setup8.MaxLinearFactor = 100f;
            setup8.MaxRotationFactor = 100f;
            setup8.MaxAngularFactor = 1f;
            setup8.IterationsFactor = 0.3f;
            setup8.IgnoreParentId = true;
            setup8.UserTrend = true;
            m_controlledMovingSettings = setup8;
            MyPredictedSnapshotSyncSetup setup9 = new MyPredictedSnapshotSyncSetup();
            setup9.ProfileName = "ControlledCharacterNewParent";
            setup9.ApplyPosition = true;
            setup9.ApplyRotation = false;
            setup9.ApplyPhysicsAngular = false;
            setup9.ApplyPhysicsLinear = false;
            setup9.ExtrapolationSmoothing = true;
            setup9.InheritRotation = true;
            setup9.ApplyPhysicsLocal = true;
            setup9.IsControlled = true;
            setup9.AllowForceStop = true;
            setup9.MaxPositionFactor = 100f;
            setup9.MaxLinearFactor = 100f;
            setup9.MaxRotationFactor = 100f;
            setup9.MaxAngularFactor = 1f;
            setup9.IterationsFactor = 1.5f;
            setup9.IgnoreParentId = true;
            setup9.UserTrend = true;
            m_controlledNewParentSettings = setup9;
            MyPredictedSnapshotSyncSetup setup10 = new MyPredictedSnapshotSyncSetup();
            setup10.ProfileName = "GeneralCharacter";
            setup10.ApplyPosition = true;
            setup10.ApplyRotation = true;
            setup10.ApplyPhysicsAngular = false;
            setup10.ApplyPhysicsLinear = true;
            setup10.ExtrapolationSmoothing = true;
            setup10.ApplyPhysicsLocal = true;
            setup10.IsControlled = false;
            setup10.MaxPositionFactor = 100f;
            setup10.MaxLinearFactor = 100f;
            setup10.MaxRotationFactor = 180f;
            setup10.MaxAngularFactor = 1f;
            setup10.IterationsFactor = 0.25f;
            setup10.IgnoreParentId = true;
            setup10.UserTrend = true;
            m_settings = setup10;
            MyPredictedSnapshotSyncSetup setup11 = new MyPredictedSnapshotSyncSetup();
            setup11.ProfileName = "ControlledLadderCharacter";
            setup11.ApplyPosition = false;
            setup11.ApplyRotation = false;
            setup11.ApplyPhysicsAngular = false;
            setup11.ApplyPhysicsLinear = false;
            setup11.ExtrapolationSmoothing = true;
            setup11.InheritRotation = true;
            setup11.ApplyPhysicsLocal = true;
            setup11.IsControlled = true;
            setup11.AllowForceStop = true;
            setup11.MinPositionFactor = 100f;
            setup11.MaxPositionFactor = 10f;
            setup11.MaxLinearFactor = 100f;
            setup11.MaxRotationFactor = 100f;
            setup11.MaxAngularFactor = 1f;
            setup11.IterationsFactor = 0.3f;
            setup11.IgnoreParentId = true;
            setup11.UserTrend = true;
            m_controlledLadderSettings = setup11;
            EXCESSIVE_CORRECTION_THRESHOLD = 20f;
        }

        public MyCharacterPhysicsStateGroup(MyEntity entity, IMyReplicable ownerReplicable) : base(entity, ownerReplicable, false)
        {
            this.m_tmpEntityResults = new List<MyEntity>();
            this.m_syncLinearVelocity = 2;
            this.m_predictedSync = new MyPredictedSnapshotSync(this.Entity);
            this.m_animatedSync = new MyAnimatedSnapshotSync(this.Entity);
            base.m_snapshotSync = this.m_animatedSync;
            if (Sync.IsServer)
            {
                this.Entity.Hierarchy.OnParentChanged += new Action<MyHierarchyComponentBase, MyHierarchyComponentBase>(this.OnEntityParentChanged);
            }
        }

        public override void ClientUpdate(MyTimeSpan clientTimestamp)
        {
            bool isControlledLocally = base.IsControlledLocally;
            bool inited = true;
            MyPredictedSnapshotSyncSetup settings = m_settings;
            if (!this.Entity.IsClientPredicted)
            {
                if (isControlledLocally)
                {
                    settings = m_controlledAnimatedSettings;
                }
                base.m_snapshotSync = this.m_animatedSync;
            }
            else
            {
                if (!ReferenceEquals(base.m_snapshotSync, this.m_predictedSync))
                {
                    this.m_lastAnimatedTime = MySandboxGame.Static.SimulationTime;
                }
                base.m_snapshotSync = this.m_predictedSync;
                long parentId = this.m_predictedSync.GetParentId();
                if (parentId != -1L)
                {
                    this.Entity.ClosestParentId = parentId;
                }
                bool flag3 = MySandboxGame.Static.SimulationTime < (this.m_lastParentTime + MyTimeSpan.FromSeconds(3.0));
                if (MySandboxGame.Static.SimulationTime < (this.m_lastAnimatedTime + MyTimeSpan.FromSeconds(0.10000000149011612)))
                {
                    this.m_predictedSync.Reset(true);
                }
                inited = this.m_predictedSync.Inited;
                bool flag4 = this.Entity.MoveIndicator != Vector3.Zero;
                if (isControlledLocally)
                {
                    if (!this.Entity.InheritRotation)
                    {
                        settings = flag3 ? m_controlledJetPackNewParentSettings : (flag4 ? m_controlledJetPackMovingSettings : m_controlledJetPackSettings);
                    }
                    else
                    {
                        MyPredictedSnapshotSyncSetup controlledLadderSettings;
                        if (this.Entity.IsOnLadder || (this.Entity.CurrentMovementState == MyCharacterMovementEnum.LadderOut))
                        {
                            controlledLadderSettings = m_controlledLadderSettings;
                        }
                        else
                        {
                            controlledLadderSettings = flag3 ? m_controlledNewParentSettings : (flag4 ? m_controlledMovingSettings : m_controlledSettings);
                        }
                        settings = controlledLadderSettings;
                    }
                }
            }
            if (this.Entity.IsDead)
            {
                settings = m_deadSettings;
            }
            long num = base.m_snapshotSync.Update(clientTimestamp, settings);
            if (num != -1L)
            {
                this.Entity.ClosestParentId = num;
            }
            this.Entity.AlwaysDisablePrediction = MyPredictedSnapshotSync.ForceAnimated;
            this.Entity.LastSnapshotFlags = settings;
            if (this.m_predictedSync.AverageCorrection.Sum > EXCESSIVE_CORRECTION_THRESHOLD)
            {
                this.Entity.ForceDisablePrediction = true;
                this.m_predictedSync.AverageCorrection.Reset();
            }
            if ((this.m_predictedSync.Inited && (!inited && (this.Entity.Physics != null))) && (this.Entity.Physics.CharacterProxy != null))
            {
                this.Entity.Physics.CharacterProxy.SetSupportedState(true);
            }
        }

        public override void Destroy()
        {
            if (Sync.IsServer)
            {
                this.Entity.Hierarchy.OnParentChanged -= new Action<MyHierarchyComponentBase, MyHierarchyComponentBase>(this.OnEntityParentChanged);
            }
            base.Destroy();
        }

        public override bool IsStillDirty(Endpoint forClient) => 
            ReferenceEquals(this.Entity.Parent, null);

        private void OnEntityParentChanged(MyHierarchyComponentBase oldParent, MyHierarchyComponentBase newParent)
        {
            if ((oldParent != null) && (newParent == null))
            {
                MyMultiplayer.GetReplicationServer().AddToDirtyGroups(this);
            }
        }

        public override void Serialize(BitStream stream, Endpoint forClient, MyTimeSpan serverTimestamp, MyTimeSpan lastClientTimestamp, byte packetId, int maxBitPosition, HashSet<string> cachedData)
        {
            if (stream.Writing)
            {
                this.UpdateEntitySupport();
                MySnapshot snapshot = new MySnapshot(this.Entity, false, this.Entity.InheritRotation);
                if (this.Entity.Parent != null)
                {
                    snapshot.Active = false;
                }
                snapshot.Write(stream);
                stream.WriteBool(true);
                this.Entity.SerializeControls(stream);
            }
            else
            {
                MySnapshot item = new MySnapshot(stream);
                if (item.ParentId != this.m_lastParentId)
                {
                    this.m_lastParentId = item.ParentId;
                    if (base.m_supportInited)
                    {
                        this.m_lastParentTime = MySandboxGame.Static.SimulationTime;
                    }
                    else
                    {
                        base.m_supportInited = true;
                    }
                }
                this.m_animatedSync.Read(ref item, serverTimestamp);
                this.m_predictedSync.Read(ref item, lastClientTimestamp);
                if (stream.ReadBool())
                {
                    this.Entity.DeserializeControls(stream, false);
                }
            }
        }

        private void UpdateEntitySupport()
        {
            MyTimeSpan simulationTime = MySandboxGame.Static.SimulationTime;
            if (((this.m_lastTimestamp + ParentChangeTimeOut) <= simulationTime) && (this.Entity.Physics != null))
            {
                if (this.Entity.Parent != null)
                {
                    base.m_lastSupportId = this.Entity.Parent.EntityId;
                }
                else
                {
                    this.m_lastTimestamp = simulationTime;
                    if (this.Entity.JetpackRunning || this.Entity.IsDead)
                    {
                        this.Entity.ClosestParentId = base.UpdateParenting(JetpackParentingSetup, this.Entity.ClosestParentId);
                    }
                    else
                    {
                        bool flag = false;
                        if ((this.Entity.Physics.CharacterProxy != null) && this.Entity.Physics.CharacterProxy.Supported)
                        {
                            List<MyEntity> tmpEntityResults = this.m_tmpEntityResults;
                            this.Entity.Physics.CharacterProxy.GetSupportingEntities(tmpEntityResults);
                            bool flag2 = false;
                            foreach (MyEntity entity in tmpEntityResults)
                            {
                                if ((entity is MyCubeGrid) || (entity is MyVoxelBase))
                                {
                                    base.m_supportInited = true;
                                    flag2 = true;
                                    if (entity.Physics.IsStatic)
                                    {
                                        this.Entity.ClosestParentId = base.m_lastSupportId = 0L;
                                    }
                                    else
                                    {
                                        this.Entity.ClosestParentId = base.m_lastSupportId = entity.EntityId;
                                        flag = true;
                                    }
                                    break;
                                }
                            }
                            if ((tmpEntityResults.Count > 0) && !flag2)
                            {
                                this.Entity.ClosestParentId = base.UpdateParenting(JetpackParentingSetup, this.Entity.ClosestParentId);
                            }
                            tmpEntityResults.Clear();
                        }
                        if (!flag && (this.Entity.ClosestParentId != 0))
                        {
                            MyEntity entity2;
                            MyEntities.TryGetEntityById(this.Entity.ClosestParentId, out entity2, false);
                            MyCubeGrid grid = entity2 as MyCubeGrid;
                            if ((grid != null) && (((float) grid.PositionComp.WorldAABB.DistanceSquared(this.Entity.PositionComp.GetPosition())) > 100f))
                            {
                                this.Entity.ClosestParentId = 0L;
                            }
                        }
                    }
                }
            }
        }

        public MyCharacter Entity =>
            ((MyCharacter) base.Entity);

        public double AverageCorrection =>
            this.m_predictedSync.AverageCorrection.Sum;
    }
}

