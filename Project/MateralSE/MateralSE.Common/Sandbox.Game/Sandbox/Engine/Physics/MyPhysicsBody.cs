namespace Sandbox.Engine.Physics
{
    using Havok;
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageMath.Spatial;
    using VRageRender;

    [MyComponentBuilder(typeof(MyObjectBuilder_PhysicsBodyComponent), true)]
    public class MyPhysicsBody : MyPhysicsComponentBase, MyClusterTree.IMyActivationHandler
    {
        private PhysicsContactHandler m_contactPointCallbackHandler;
        private Action<MyPhysicsComponentBase, bool> m_onBodyActiveStateChangedHandler;
        private bool m_activationCallbackRegistered;
        private bool m_contactPointCallbackRegistered;
        private HkdBreakableBody m_breakableBody;
        private List<HkdBreakableBodyInfo> m_tmpLst;
        protected HkRagdoll m_ragdoll;
        private bool m_ragdollDeadMode;
        private readonly MyWeldInfo m_weldInfo;
        private List<HkMassElement> m_tmpElements;
        private List<HkShape> m_tmpShapeList;
        private static MyStringHash m_character = MyStringHash.GetOrCompute("Character");
        private int m_motionCounter;
        protected float m_angularDamping;
        protected float m_linearDamping;
        private ulong m_clusterObjectID;
        private Vector3D m_offset;
        protected Matrix m_bodyMatrix;
        protected HkWorld m_world;
        private HkWorld m_lastWorld;
        private HkRigidBody m_rigidBody;
        private HkRigidBody m_rigidBody2;
        private float m_animatedClientMass;
        private readonly HashSet<HkConstraint> m_constraints;
        private readonly List<HkConstraint> m_constraintsAddBatch;
        private readonly List<HkConstraint> m_constraintsRemoveBatch;
        public HkSolverDeactivation InitialSolverDeactivation;
        private bool m_isInWorld;
        private bool m_shapeChangeInProgress;
        private HashSet<IMyEntity> m_batchedChildren;
        private List<MyPhysicsBody> m_batchedBodies;
        private Vector3D? m_lastComPosition;
        private Vector3 m_lastComLocal;
        private bool m_isStaticForCluster;
        private bool m_batchRequest;
        private static List<HkConstraint> m_notifyConstraints = new List<HkConstraint>();

        public event PhysicsContactHandler ContactPointCallback
        {
            add
            {
                bool isInWorld = this.IsInWorld;
                this.m_contactPointCallbackHandler = (PhysicsContactHandler) Delegate.Combine(this.m_contactPointCallbackHandler, value);
                this.RegisterContactPointCallbackIfNeeded();
            }
            remove
            {
                bool isInWorld = this.IsInWorld;
                this.m_contactPointCallbackHandler = (PhysicsContactHandler) Delegate.Remove(this.m_contactPointCallbackHandler, value);
                if (!this.NeedsContactPointCallback)
                {
                    this.UnregisterContactPointCallback();
                }
            }
        }

        public event Action<MyPhysicsComponentBase, bool> OnBodyActiveStateChanged
        {
            add
            {
                this.m_onBodyActiveStateChangedHandler = (Action<MyPhysicsComponentBase, bool>) Delegate.Combine(this.m_onBodyActiveStateChangedHandler, value);
                this.RegisterActivationCallbacksIfNeeded();
            }
            remove
            {
                this.m_onBodyActiveStateChangedHandler = (Action<MyPhysicsComponentBase, bool>) Delegate.Remove(this.m_onBodyActiveStateChangedHandler, value);
                if (!this.NeedsActivationCallback)
                {
                    this.UnregisterActivationCallbacks();
                }
            }
        }

        public MyPhysicsBody()
        {
            this.m_tmpLst = new List<HkdBreakableBodyInfo>();
            this.m_weldInfo = new MyWeldInfo();
            this.m_tmpElements = new List<HkMassElement>();
            this.m_tmpShapeList = new List<HkShape>();
            this.m_clusterObjectID = ulong.MaxValue;
            this.m_offset = Vector3D.Zero;
            this.m_constraints = new HashSet<HkConstraint>();
            this.m_constraintsAddBatch = new List<HkConstraint>();
            this.m_constraintsRemoveBatch = new List<HkConstraint>();
            this.InitialSolverDeactivation = HkSolverDeactivation.Low;
            this.m_batchedChildren = new HashSet<IMyEntity>();
            this.m_batchedBodies = new List<MyPhysicsBody>();
        }

        public MyPhysicsBody(IMyEntity entity, RigidBodyFlag flags)
        {
            this.m_tmpLst = new List<HkdBreakableBodyInfo>();
            this.m_weldInfo = new MyWeldInfo();
            this.m_tmpElements = new List<HkMassElement>();
            this.m_tmpShapeList = new List<HkShape>();
            this.m_clusterObjectID = ulong.MaxValue;
            this.m_offset = Vector3D.Zero;
            this.m_constraints = new HashSet<HkConstraint>();
            this.m_constraintsAddBatch = new List<HkConstraint>();
            this.m_constraintsRemoveBatch = new List<HkConstraint>();
            this.InitialSolverDeactivation = HkSolverDeactivation.Low;
            this.m_batchedChildren = new HashSet<IMyEntity>();
            this.m_batchedBodies = new List<MyPhysicsBody>();
            base.Entity = entity;
            base.m_enabled = false;
            base.Flags = flags;
            this.IsSubpart = false;
        }

        public override void Activate()
        {
            if (this.Enabled)
            {
                IMyEntity topMostParent = base.Entity.GetTopMostParent(null);
                if (!ReferenceEquals(topMostParent, base.Entity) && (topMostParent.Physics != null))
                {
                    this.Activate(((MyPhysicsBody) topMostParent.Physics).HavokWorld, ulong.MaxValue);
                }
                else if (this.ClusterObjectID == ulong.MaxValue)
                {
                    ulong? customId = null;
                    this.ClusterObjectID = MyPhysics.AddObject(base.Entity.WorldAABB, this, customId, ((VRage.Game.Entity.MyEntity) base.Entity).DebugName, base.Entity.EntityId, this.m_batchRequest);
                }
            }
        }

        public virtual void Activate(object world, ulong clusterObjectID)
        {
            this.m_world = (HkWorld) world;
            if (this.m_world != null)
            {
                this.ClusterObjectID = clusterObjectID;
                this.ActivateCollision();
                this.IsInWorld = true;
                this.GetRigidBodyMatrix(out this.m_bodyMatrix, true);
                if (this.BreakableBody == null)
                {
                    if (this.RigidBody != null)
                    {
                        this.RigidBody.SetWorldMatrix(this.m_bodyMatrix);
                        this.m_world.AddRigidBody(this.RigidBody);
                    }
                }
                else
                {
                    if (this.RigidBody != null)
                    {
                        this.RigidBody.SetWorldMatrix(this.m_bodyMatrix);
                    }
                    if (Sync.IsServer)
                    {
                        this.m_world.DestructionWorld.AddBreakableBody(this.BreakableBody);
                    }
                    else if (this.RigidBody != null)
                    {
                        this.m_world.AddRigidBody(this.RigidBody);
                    }
                }
                if (this.RigidBody2 != null)
                {
                    this.RigidBody2.SetWorldMatrix(this.m_bodyMatrix);
                    this.m_world.AddRigidBody(this.RigidBody2);
                }
                if (this.CharacterProxy != null)
                {
                    this.RagdollSystemGroupCollisionFilterID = 0;
                    this.CharacterSystemGroupCollisionFilterID = this.m_world.GetCollisionFilter().GetNewSystemGroup();
                    this.CharacterCollisionFilter = HkGroupFilter.CalcFilterInfo(0x12, this.CharacterSystemGroupCollisionFilterID, 0, 0);
                    this.CharacterProxy.SetCollisionFilterInfo(this.CharacterCollisionFilter);
                    this.CharacterProxy.SetRigidBodyTransform(ref this.m_bodyMatrix);
                    this.CharacterProxy.Activate(this.m_world);
                }
                if (this.ReactivateRagdoll)
                {
                    this.GetRigidBodyMatrix(out this.m_bodyMatrix, false);
                    this.ActivateRagdoll(this.m_bodyMatrix);
                    this.ReactivateRagdoll = false;
                }
                if (this.SwitchToRagdollModeOnActivate)
                {
                    bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
                    this.SwitchToRagdollModeOnActivate = false;
                    this.SwitchToRagdollMode(this.m_ragdollDeadMode, 1);
                }
                this.m_world.LockCriticalOperations();
                foreach (HkConstraint constraint in this.m_constraints)
                {
                    if (IsConstraintValid(constraint))
                    {
                        this.m_world.AddConstraint(constraint);
                    }
                }
                this.m_world.UnlockCriticalOperations();
                base.m_enabled = true;
            }
        }

        public virtual void ActivateBatch(object world, ulong clusterObjectID)
        {
            IMyEntity topMostParent = base.Entity.GetTopMostParent(null);
            if (ReferenceEquals(topMostParent, base.Entity) || (topMostParent.Physics == null))
            {
                this.ClusterObjectID = clusterObjectID;
                using (List<MyPhysicsBody>.Enumerator enumerator = this.m_batchedBodies.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.ActivateBatchInternal(world);
                    }
                }
                this.m_batchedBodies.Clear();
                this.ActivateBatchInternal(world);
            }
        }

        private void ActivateBatchInternal(object world)
        {
            this.m_world = (HkWorld) world;
            this.IsInWorld = true;
            this.GetRigidBodyMatrix(out this.m_bodyMatrix, true);
            if (this.RigidBody != null)
            {
                this.RigidBody.SetWorldMatrix(this.m_bodyMatrix);
                this.m_world.AddRigidBodyBatch(this.RigidBody);
            }
            if (this.RigidBody2 != null)
            {
                this.RigidBody2.SetWorldMatrix(this.m_bodyMatrix);
                this.m_world.AddRigidBodyBatch(this.RigidBody2);
            }
            if (this.CharacterProxy != null)
            {
                this.RagdollSystemGroupCollisionFilterID = 0;
                this.CharacterSystemGroupCollisionFilterID = this.m_world.GetCollisionFilter().GetNewSystemGroup();
                this.CharacterCollisionFilter = HkGroupFilter.CalcFilterInfo(0x12, this.CharacterSystemGroupCollisionFilterID, 1, 1);
                this.CharacterProxy.SetCollisionFilterInfo(this.CharacterCollisionFilter);
                this.CharacterProxy.SetRigidBodyTransform(ref this.m_bodyMatrix);
                this.CharacterProxy.Activate(this.m_world);
            }
            if (this.SwitchToRagdollModeOnActivate)
            {
                bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
                this.SwitchToRagdollModeOnActivate = false;
                this.SwitchToRagdollMode(this.m_ragdollDeadMode, 1);
            }
            foreach (HkConstraint constraint in this.m_constraints)
            {
                this.m_constraintsAddBatch.Add(constraint);
            }
            base.m_enabled = true;
        }

        protected virtual void ActivateCollision()
        {
            ((VRage.Game.Entity.MyEntity) base.Entity).RaisePhysicsChanged();
        }

        private void ActivateRagdoll(Matrix worldMatrix)
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyPhysicsBody.ActivateRagdoll");
            }
            if (((this.Ragdoll != null) && (this.HavokWorld != null)) && !this.IsRagdollModeActive)
            {
                using (List<HkRigidBody>.Enumerator enumerator = this.Ragdoll.RigidBodies.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.UserObject = this;
                    }
                }
                this.Ragdoll.SetWorldMatrix(worldMatrix, false, false);
                this.HavokWorld.AddRagdoll(this.Ragdoll);
                if (!MyFakes.ENABLE_JETPACK_RAGDOLL_COLLISIONS)
                {
                    this.DisableRagdollBodiesCollisions();
                }
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyPhysicsBody.ActivateRagdoll - FINISHED");
                }
            }
        }

        public void AddConstraint(HkConstraint constraint)
        {
            if (this.IsWelded)
            {
                this.WeldInfo.Parent.AddConstraint(constraint);
            }
            else if (((this.HavokWorld != null) && (this.RigidBody != null)) && IsConstraintValid(constraint))
            {
                this.m_constraints.Add(constraint);
                this.HavokWorld.AddConstraint(constraint);
                if (!MyFakes.MULTIPLAYER_CLIENT_CONSTRAINTS && !Sync.IsServer)
                {
                    this.UpdateConstraintForceDisable(constraint);
                }
            }
        }

        public override void AddForce(MyPhysicsForceType type, Vector3? force, Vector3D? position, Vector3? torque, float? maxSpeed = new float?(), bool applyImmediately = true, bool activeOnly = false)
        {
            if (applyImmediately)
            {
                this.AddForceInternal(type, force, position, torque, maxSpeed, activeOnly);
            }
            else
            {
                if (!activeOnly && !this.IsActive)
                {
                    if (this.RigidBody != null)
                    {
                        this.RigidBody.Activate();
                    }
                    else if (this.CharacterProxy != null)
                    {
                        this.CharacterProxy.GetHitRigidBody().Activate();
                    }
                    else if (this.Ragdoll != null)
                    {
                        this.Ragdoll.Activate();
                    }
                }
                MyPhysics.QueuedForces.Enqueue(new MyPhysics.ForceInfo(this, activeOnly, maxSpeed, force, torque, position, type));
            }
        }

        private void AddForceInternal(MyPhysicsForceType type, Vector3? force, Vector3D? position, Vector3? torque, float? maxSpeed, bool activeOnly)
        {
            if (!this.IsStatic && (!activeOnly || this.IsActive))
            {
                float? nullable1;
                if (MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_FORCES)
                {
                    MyPhysicsDebugDraw.DebugDrawAddForce(this, type, force, position, torque, false);
                }
                switch (type)
                {
                    case MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE:
                        this.ApplyImplusesWorld(force, position, torque, this.RigidBody);
                        if ((this.CharacterProxy != null) && (force != null))
                        {
                            this.CharacterProxy.ApplyLinearImpulse(force.Value);
                        }
                        if (((this.Ragdoll != null) && this.Ragdoll.InWorld) && !this.Ragdoll.IsKeyframed)
                        {
                            this.ApplyImpuseOnRagdoll(force, position, torque, this.Ragdoll);
                        }
                        break;

                    case MyPhysicsForceType.ADD_BODY_FORCE_AND_BODY_TORQUE:
                        Matrix rigidBodyMatrix;
                        Vector3D? nullable;
                        Vector3? nullable2;
                        if (this.RigidBody != null)
                        {
                            Vector3? nullable7;
                            rigidBodyMatrix = this.RigidBody.GetRigidBodyMatrix();
                            nullable = position;
                            if (nullable != null)
                            {
                                nullable7 = new Vector3?(nullable.GetValueOrDefault());
                            }
                            else
                            {
                                nullable2 = null;
                                nullable7 = nullable2;
                            }
                            this.AddForceTorqueBody(force, torque, nullable7, this.RigidBody, ref rigidBodyMatrix);
                        }
                        if ((this.CharacterProxy != null) && (this.CharacterProxy.GetHitRigidBody() != null))
                        {
                            Vector3? nullable8;
                            rigidBodyMatrix = (Matrix) base.Entity.WorldMatrix;
                            nullable = position;
                            if (nullable != null)
                            {
                                nullable8 = new Vector3?(nullable.GetValueOrDefault());
                            }
                            else
                            {
                                nullable2 = null;
                                nullable8 = nullable2;
                            }
                            this.AddForceTorqueBody(force, torque, nullable8, this.CharacterProxy.GetHitRigidBody(), ref rigidBodyMatrix);
                        }
                        if (((this.Ragdoll != null) && this.Ragdoll.InWorld) && !this.Ragdoll.IsKeyframed)
                        {
                            rigidBodyMatrix = (Matrix) base.Entity.WorldMatrix;
                            this.ApplyForceTorqueOnRagdoll(force, torque, this.Ragdoll, ref rigidBodyMatrix);
                        }
                        break;

                    case MyPhysicsForceType.APPLY_WORLD_FORCE:
                        this.ApplyForceWorld(force, position, this.RigidBody);
                        if (this.CharacterProxy != null)
                        {
                            if (this.CharacterProxy.GetState() == HkCharacterStateType.HK_CHARACTER_ON_GROUND)
                            {
                                this.CharacterProxy.ApplyLinearImpulse((force.Value / this.Mass) * 10f);
                            }
                            else
                            {
                                this.CharacterProxy.ApplyLinearImpulse(force.Value / this.Mass);
                            }
                        }
                        if (((this.Ragdoll != null) && this.Ragdoll.InWorld) && !this.Ragdoll.IsKeyframed)
                        {
                            this.ApplyForceOnRagdoll(force, position, this.Ragdoll);
                        }
                        break;

                    default:
                        break;
                }
                float? nullable4 = maxSpeed;
                float? nullable5 = maxSpeed;
                if ((nullable4 != null) & (nullable5 != null))
                {
                    nullable1 = new float?(nullable4.GetValueOrDefault() * nullable5.GetValueOrDefault());
                }
                else
                {
                    nullable1 = null;
                }
                float? nullable3 = nullable1;
                if ((this.LinearVelocity.LengthSquared() > nullable3.GetValueOrDefault()) & (nullable3 != null))
                {
                    VRage.Game.Entity.MyEntity entity;
                    Vector3 linearVelocity = this.LinearVelocity;
                    linearVelocity.Normalize();
                    linearVelocity *= maxSpeed.Value;
                    if (((this.RigidBody != null) && ((MyMultiplayer.Static != null) && (!Sync.IsServer && (base.Entity is MyCubeGrid)))) && Sandbox.Game.Entities.MyEntities.TryGetEntityById(((MyCubeGrid) base.Entity).ClosestParentId, out entity, false))
                    {
                        linearVelocity -= entity.Physics.LinearVelocity;
                    }
                    this.LinearVelocity = linearVelocity;
                }
            }
        }

        private unsafe void AddForceTorqueBody(Vector3? force, Vector3? torque, Vector3? position, HkRigidBody rigidBody, ref Matrix transform)
        {
            if ((force != null) && !MyUtils.IsZero(force.Value, 1E-05f))
            {
                Vector3 result = force.Value;
                Vector3* vectorPtr1 = (Vector3*) ref result;
                Vector3.TransformNormal(ref (Vector3) ref vectorPtr1, ref transform, out result);
                if (position == null)
                {
                    rigidBody.ApplyLinearImpulse((result * 0.01666667f) * MyFakes.SIMULATION_SPEED);
                }
                else
                {
                    Vector3 vector2 = position.Value;
                    Vector3* vectorPtr2 = (Vector3*) ref vector2;
                    Vector3.Transform(ref (Vector3) ref vectorPtr2, ref transform, out vector2);
                    this.ApplyForceWorld(new Vector3?(result), new Vector3D?(vector2 + this.Offset), rigidBody);
                }
            }
            if ((torque != null) && !MyUtils.IsZero(torque.Value, 1E-05f))
            {
                Vector3 vector3 = Vector3.TransformNormal(torque.Value, (Matrix) transform);
                rigidBody.ApplyAngularImpulse((vector3 * 0.01666667f) * MyFakes.SIMULATION_SPEED);
                Vector3 angularVelocity = rigidBody.AngularVelocity;
                float maxAngularVelocity = rigidBody.MaxAngularVelocity;
                if (angularVelocity.LengthSquared() > (maxAngularVelocity * maxAngularVelocity))
                {
                    angularVelocity.Normalize();
                    angularVelocity *= maxAngularVelocity;
                    rigidBody.AngularVelocity = angularVelocity;
                }
            }
        }

        private void ApplyForceOnRagdoll(Vector3? force, Vector3D? position, HkRagdoll ragdoll)
        {
            foreach (HkRigidBody body in ragdoll.RigidBodies)
            {
                Vector3 vector = (force.Value * body.Mass) / ragdoll.Mass;
                this.ApplyForceWorld(new Vector3?(vector), position, body);
            }
        }

        private void ApplyForceTorqueOnRagdoll(Vector3? force, Vector3? torque, HkRagdoll ragdoll, ref Matrix transform)
        {
            foreach (HkRigidBody body in ragdoll.RigidBodies)
            {
                if (body != null)
                {
                    Vector3 vector = (force.Value * body.Mass) / ragdoll.Mass;
                    transform = body.GetRigidBodyMatrix();
                    Vector3? position = null;
                    this.AddForceTorqueBody(new Vector3?(vector), torque, position, body, ref transform);
                }
            }
        }

        private void ApplyForceWorld(Vector3? force, Vector3D? position, HkRigidBody rigidBody)
        {
            if (((rigidBody != null) && (force != null)) && !MyUtils.IsZero(force.Value, 1E-05f))
            {
                if (position == null)
                {
                    rigidBody.ApplyForce(0.01666667f, force.Value);
                }
                else
                {
                    Vector3 point = position.Value - this.Offset;
                    rigidBody.ApplyForce(0.01666667f, force.Value, point);
                }
            }
        }

        private void ApplyImplusesWorld(Vector3? force, Vector3D? position, Vector3? torque, HkRigidBody rigidBody)
        {
            if (rigidBody != null)
            {
                if ((force != null) && (position != null))
                {
                    rigidBody.ApplyPointImpulse(force.Value, position.Value - this.Offset);
                }
                if (torque != null)
                {
                    rigidBody.ApplyAngularImpulse((torque.Value * 0.01666667f) * MyFakes.SIMULATION_SPEED);
                }
            }
        }

        public override void ApplyImpulse(Vector3 impulse, Vector3D pos)
        {
            Vector3? torque = null;
            float? maxSpeed = null;
            this.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, new Vector3?(impulse), new Vector3D?(pos), torque, maxSpeed, true, false);
        }

        private void ApplyImpuseOnRagdoll(Vector3? force, Vector3D? position, Vector3? torque, HkRagdoll ragdoll)
        {
            foreach (HkRigidBody body in ragdoll.RigidBodies)
            {
                Vector3 vector = (force.Value * body.Mass) / ragdoll.Mass;
                this.ApplyImplusesWorld(new Vector3?(vector), position, torque, body);
            }
        }

        public virtual void ChangeQualityType(HkCollidableQualityType quality)
        {
            this.RigidBody.Quality = quality;
        }

        private void CheckRBNotInWorld()
        {
            if ((this.RigidBody != null) && this.RigidBody.InWorld)
            {
                MyAnalyticsHelper.ReportActivityStart((VRage.Game.Entity.MyEntity) base.Entity, "RigidBody in world after deactivation", "", "DevNote", "", false);
                this.RigidBody.RemoveFromWorld();
            }
            if ((this.RigidBody2 != null) && this.RigidBody2.InWorld)
            {
                this.RigidBody2.RemoveFromWorld();
            }
        }

        [Conditional("DEBUG")]
        private void CheckUnlockedSpeeds()
        {
            if ((base.IsPhantom || this.IsSubpart) || (base.Entity.Parent != null))
            {
                bool flag1 = this.RigidBody == null;
            }
        }

        public override void Clear()
        {
            this.ClearSpeed();
        }

        public override void ClearSpeed()
        {
            if (this.RigidBody != null)
            {
                this.RigidBody.LinearVelocity = Vector3.Zero;
                this.RigidBody.AngularVelocity = Vector3.Zero;
            }
            if (this.CharacterProxy != null)
            {
                this.CharacterProxy.LinearVelocity = Vector3.Zero;
                this.CharacterProxy.AngularVelocity = Vector3.Zero;
                this.CharacterProxy.PosX = 0f;
                this.CharacterProxy.PosY = 0f;
                this.CharacterProxy.Elevate = 0f;
            }
        }

        public override void Close()
        {
            this.CloseRagdoll();
            base.Close();
            if (this.CharacterProxy != null)
            {
                this.CharacterProxy.Dispose();
                this.CharacterProxy = null;
            }
        }

        public void CloseRagdoll()
        {
            if (this.Ragdoll != null)
            {
                if (this.IsRagdollModeActive)
                {
                    this.CloseRagdollMode(this.HavokWorld);
                }
                if (this.Ragdoll.InWorld)
                {
                    this.HavokWorld.RemoveRagdoll(this.Ragdoll);
                }
                this.Ragdoll.Dispose();
                this.Ragdoll = null;
            }
        }

        public void CloseRagdollMode()
        {
            this.CloseRagdollMode(this.HavokWorld);
        }

        public void CloseRagdollMode(HkWorld world)
        {
            bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
            if (this.IsRagdollModeActive && (world != null))
            {
                using (List<HkRigidBody>.Enumerator enumerator = this.Ragdoll.RigidBodies.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.UserObject = null;
                    }
                }
                this.Ragdoll.Deactivate();
                world.RemoveRagdoll(this.Ragdoll);
                bool flag2 = MyFakes.ENABLE_RAGDOLL_DEBUG;
            }
        }

        protected override void CloseRigidBody()
        {
            if (this.IsWelded)
            {
                this.WeldInfo.Parent.Unweld(this, false, true);
            }
            if (this.WeldInfo.Children.Count != 0)
            {
                MyWeldingGroups.ReplaceParent(MyWeldingGroups.Static.GetGroup((VRage.Game.Entity.MyEntity) base.Entity), (VRage.Game.Entity.MyEntity) base.Entity, null);
            }
            this.CheckRBNotInWorld();
            if (this.RigidBody != null)
            {
                if (!this.RigidBody.IsDisposed)
                {
                    this.RigidBody.Dispose();
                }
                this.RigidBody = null;
            }
            if (this.RigidBody2 != null)
            {
                this.RigidBody2.Dispose();
                this.RigidBody2 = null;
            }
            if (this.BreakableBody != null)
            {
                this.BreakableBody.Dispose();
                this.BreakableBody = null;
            }
            if (this.WeldedRigidBody != null)
            {
                this.WeldedRigidBody.Dispose();
                this.WeldedRigidBody = null;
            }
        }

        public override Vector3D ClusterToWorld(Vector3 clusterPos) => 
            (clusterPos + this.Offset);

        protected virtual void CreateBody(ref HkShape shape, HkMassProperties? massProperties)
        {
            HkRigidBodyCinfo rbInfo = new HkRigidBodyCinfo {
                AngularDamping = this.m_angularDamping,
                LinearDamping = this.m_linearDamping,
                Shape = shape,
                SolverDeactivation = this.InitialSolverDeactivation,
                ContactPointCallbackDelay = base.ContactPointDelay
            };
            if (massProperties != null)
            {
                this.m_animatedClientMass = massProperties.Value.Mass;
                rbInfo.SetMassProperties(massProperties.Value);
            }
            GetInfoFromFlags(rbInfo, base.Flags);
            this.RigidBody = new HkRigidBody(rbInfo);
        }

        public override void CreateCharacterCollision(Vector3 center, float characterWidth, float characterHeight, float crouchHeight, float ladderHeight, float headSize, float headHeight, MatrixD worldTransform, float mass, ushort collisionLayer, bool isOnlyVertical, float maxSlope, float maxImpulse, float maxSpeedRelativeToShip, bool networkProxy, float? maxForce = new float?())
        {
            base.Center = center;
            base.CanUpdateAccelerations = false;
            if (!networkProxy)
            {
                Matrix matrix = Matrix.CreateWorld(Vector3.TransformNormal(base.Center, worldTransform) + worldTransform.Translation, (Vector3) worldTransform.Forward, (Vector3) worldTransform.Up);
                this.CharacterProxy = new MyCharacterProxy(true, true, characterWidth, characterHeight, crouchHeight, ladderHeight, headSize, headHeight, matrix.Translation, (Vector3) worldTransform.Up, (Vector3) worldTransform.Forward, mass, this, isOnlyVertical, maxSlope, maxImpulse, maxSpeedRelativeToShip, maxForce, null);
                this.CharacterProxy.GetRigidBody().ContactPointCallbackDelay = 0;
            }
            else
            {
                float downOffset = ((MyCharacter) base.Entity).IsCrouching ? 1f : 0f;
                HkShape shape = MyCharacterProxy.CreateCharacterShape(characterHeight, characterWidth, characterHeight + headHeight, headSize, 0f, downOffset, false);
                HkMassProperties properties = new HkMassProperties {
                    Mass = mass,
                    InertiaTensor = Matrix.Identity,
                    Volume = (characterWidth * characterWidth) * (characterHeight + (2f * characterWidth))
                };
                this.CreateFromCollisionObject(shape, center, worldTransform, new HkMassProperties?(properties), collisionLayer);
                base.CanUpdateAccelerations = false;
            }
        }

        public virtual void CreateFromCollisionObject(HkShape shape, Vector3 center, MatrixD worldTransform, HkMassProperties? massProperties = new HkMassProperties?(), int collisionFilter = 15)
        {
            this.CloseRigidBody();
            base.Center = center;
            base.CanUpdateAccelerations = true;
            this.CreateBody(ref shape, massProperties);
            this.RigidBody.UserObject = this;
            this.RigidBody.SetWorldMatrix((Matrix) worldTransform);
            this.RigidBody.Layer = collisionFilter;
            if ((base.Flags & RigidBodyFlag.RBF_DISABLE_COLLISION_RESPONSE) > RigidBodyFlag.RBF_DEFAULT)
            {
                this.RigidBody.Layer = 0x13;
            }
        }

        public override void Deactivate()
        {
            if (this.ClusterObjectID != ulong.MaxValue)
            {
                if (this.IsWelded)
                {
                    this.Unweld(false);
                }
                else
                {
                    MyPhysics.RemoveObject(this.ClusterObjectID);
                    this.ClusterObjectID = ulong.MaxValue;
                    this.CheckRBNotInWorld();
                }
            }
            else
            {
                IMyEntity topMostParent = base.Entity.GetTopMostParent(null);
                if ((topMostParent.Physics != null) && this.IsInWorld)
                {
                    if (((MyPhysicsBody) topMostParent.Physics).HavokWorld != null)
                    {
                        this.Deactivate(this.m_world);
                    }
                    else
                    {
                        this.RigidBody = null;
                        this.RigidBody2 = null;
                        this.CharacterProxy = null;
                    }
                }
            }
        }

        public virtual void Deactivate(object world)
        {
            if ((this.RigidBody == null) || this.RigidBody.InWorld)
            {
                if (this.IsRagdollModeActive)
                {
                    this.ReactivateRagdoll = true;
                    this.CloseRagdollMode(world as HkWorld);
                }
                if ((this.IsInWorld && (this.RigidBody != null)) && !this.RigidBody.IsActive)
                {
                    if (!this.RigidBody.IsFixed)
                    {
                        this.RigidBody.Activate();
                    }
                    else
                    {
                        BoundingBoxD worldAABB = base.Entity.PositionComp.WorldAABB;
                        worldAABB.Inflate((double) 0.5);
                        MyPhysics.ActivateInBox(ref worldAABB);
                    }
                }
                if (this.m_constraints.Count > 0)
                {
                    this.m_world.LockCriticalOperations();
                    foreach (HkConstraint constraint in this.m_constraints)
                    {
                        if (!constraint.IsDisposed)
                        {
                            this.m_world.RemoveConstraint(constraint);
                        }
                    }
                    this.m_world.UnlockCriticalOperations();
                }
                if ((this.BreakableBody != null) && (this.m_world.DestructionWorld != null))
                {
                    this.m_world.DestructionWorld.RemoveBreakableBody(this.BreakableBody);
                }
                else if ((this.RigidBody != null) && !this.RigidBody.IsDisposed)
                {
                    this.m_world.RemoveRigidBody(this.RigidBody);
                }
                if ((this.RigidBody2 != null) && !this.RigidBody2.IsDisposed)
                {
                    this.m_world.RemoveRigidBody(this.RigidBody2);
                }
                if (this.CharacterProxy != null)
                {
                    this.CharacterProxy.Deactivate(this.m_world);
                }
                this.CheckRBNotInWorld();
                this.m_world = null;
                this.IsInWorld = false;
            }
        }

        public virtual void DeactivateBatch(object world)
        {
            MyHierarchyComponentBase hierarchy = base.Entity.Hierarchy;
            if (hierarchy != null)
            {
                hierarchy.GetChildrenRecursive(this.m_batchedChildren);
                foreach (IMyEntity entity in this.m_batchedChildren)
                {
                    if (entity.Physics == null)
                    {
                        continue;
                    }
                    if (entity.Physics.Enabled)
                    {
                        this.m_batchedBodies.Add((MyPhysicsBody) entity.Physics);
                    }
                }
                this.m_batchedChildren.Clear();
            }
            using (List<MyPhysicsBody>.Enumerator enumerator2 = this.m_batchedBodies.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.DeactivateBatchInternal(world);
                }
            }
            this.DeactivateBatchInternal(world);
        }

        private void DeactivateBatchInternal(object world)
        {
            if (this.m_world != null)
            {
                if (this.IsRagdollModeActive)
                {
                    this.ReactivateRagdoll = true;
                    this.CloseRagdollMode(world as HkWorld);
                }
                if ((this.BreakableBody != null) && (this.m_world.DestructionWorld != null))
                {
                    this.m_world.DestructionWorld.RemoveBreakableBody(this.BreakableBody);
                }
                else if (this.RigidBody != null)
                {
                    this.m_world.RemoveRigidBodyBatch(this.RigidBody);
                }
                if (this.RigidBody2 != null)
                {
                    this.m_world.RemoveRigidBodyBatch(this.RigidBody2);
                }
                if (this.CharacterProxy != null)
                {
                    this.CharacterProxy.Deactivate(this.m_world);
                }
                foreach (HkConstraint constraint in this.m_constraints)
                {
                    if (IsConstraintValid(constraint, false))
                    {
                        this.m_constraintsRemoveBatch.Add(constraint);
                    }
                }
                base.m_enabled = false;
                if (base.EnabledChanged != null)
                {
                    base.EnabledChanged();
                }
                this.m_world = null;
                this.IsInWorld = false;
            }
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_CONSTRAINTS)
                {
                    int num = 0;
                    foreach (HkConstraint constraint in this.Constraints)
                    {
                        if (!constraint.IsDisposed)
                        {
                            Vector3 vector;
                            Vector3 vector2;
                            Color green = Color.Green;
                            if (!IsConstraintValid(constraint))
                            {
                                green = Color.Red;
                            }
                            else if (!constraint.Enabled)
                            {
                                green = Color.Yellow;
                            }
                            constraint.GetPivotsInWorld(out vector, out vector2);
                            Vector3D pointTo = this.ClusterToWorld(vector2);
                            Vector3D pointFrom = this.ClusterToWorld(vector);
                            MyRenderProxy.DebugDrawLine3D(pointFrom, pointTo, green, green, false, false);
                            MyRenderProxy.DebugDrawSphere(pointFrom, 0.2f, green, 1f, false, false, true, false);
                            MyRenderProxy.DebugDrawText3D(pointFrom, num + " A", Color.White, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                            MyRenderProxy.DebugDrawSphere(pointTo, 0.2f, green, 1f, false, false, true, false);
                            MyRenderProxy.DebugDrawText3D(pointTo, num + " B", Color.White, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                            num++;
                        }
                    }
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_INERTIA_TENSORS && (this.RigidBody != null))
                {
                    Vector3D pointFrom = this.ClusterToWorld(this.RigidBody.CenterOfMassWorld);
                    MyRenderProxy.DebugDrawLine3D(pointFrom, this.ClusterToWorld(this.RigidBody.CenterOfMassWorld) + this.RigidBody.AngularVelocity, Color.Blue, Color.Red, false, false);
                    float num2 = 1f / this.RigidBody.Mass;
                    Vector3 scale = this.RigidBody.InertiaTensor.Scale;
                    float num3 = (((scale.X - scale.Y) + scale.Z) * num2) * 6f;
                    float num4 = ((scale.X * num2) * 12f) - num3;
                    float num5 = 0.505f;
                    Vector3 max = new Vector3(Math.Sqrt((double) (((scale.Z * num2) * 12f) - num3)), Math.Sqrt((double) num3), Math.Sqrt((double) num4)) * num5;
                    MyOrientedBoundingBoxD obb = new MyOrientedBoundingBoxD(new BoundingBoxD(-max, max), MatrixD.Identity);
                    obb.Transform(this.RigidBody.GetRigidBodyMatrix());
                    obb.Center = this.CenterOfMassWorld;
                    MyRenderProxy.DebugDrawOBB(obb, Color.Purple, 0.05f, false, false, false);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_MOTION_TYPES && (this.RigidBody != null))
                {
                    MyRenderProxy.DebugDrawText3D(this.CenterOfMassWorld, this.RigidBody.GetMotionType().ToString(), Color.Purple, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SHAPES && !this.IsWelded)
                {
                    int num6;
                    if ((this.RigidBody != null) && (this.BreakableBody != null))
                    {
                        Vector3D vectord3 = Vector3D.Transform(this.BreakableBody.BreakableShape.CoM, this.RigidBody.GetRigidBodyMatrix()) + this.Offset;
                        Color color = (this.RigidBody.GetMotionType() != HkMotionType.Box_Inertia) ? Color.Gray : (this.RigidBody.IsActive ? Color.Red : Color.Blue);
                        MyRenderProxy.DebugDrawSphere(this.RigidBody.CenterOfMassWorld + this.Offset, 0.2f, color, 1f, false, false, true, false);
                        MyRenderProxy.DebugDrawAxis(base.Entity.PositionComp.WorldMatrix, 0.2f, false, false, false);
                    }
                    if (this.RigidBody != null)
                    {
                        num6 = 0;
                        Matrix rigidBodyMatrix = this.RigidBody.GetRigidBodyMatrix();
                        MatrixD worldMatrix = MatrixD.CreateWorld(rigidBodyMatrix.Translation + this.Offset, rigidBodyMatrix.Forward, rigidBodyMatrix.Up);
                        MyPhysicsDebugDraw.DrawCollisionShape(this.RigidBody.GetShape(), worldMatrix, 0.3f, ref num6, null, false);
                    }
                    if (this.RigidBody2 != null)
                    {
                        num6 = 0;
                        Matrix rigidBodyMatrix = this.RigidBody2.GetRigidBodyMatrix();
                        MatrixD worldMatrix = MatrixD.CreateWorld(rigidBodyMatrix.Translation + this.Offset, rigidBodyMatrix.Forward, rigidBodyMatrix.Up);
                        MyPhysicsDebugDraw.DrawCollisionShape(this.RigidBody2.GetShape(), worldMatrix, 0.3f, ref num6, null, false);
                    }
                    if (this.CharacterProxy != null)
                    {
                        num6 = 0;
                        Matrix rigidBodyTransform = this.CharacterProxy.GetRigidBodyTransform();
                        MatrixD worldMatrix = MatrixD.CreateWorld(rigidBodyTransform.Translation + this.Offset, rigidBodyTransform.Forward, rigidBodyTransform.Up);
                        MyPhysicsDebugDraw.DrawCollisionShape(this.CharacterProxy.GetShape(), worldMatrix, 0.3f, ref num6, null, false);
                    }
                }
            }
        }

        internal void DisableRagdollBodiesCollisions()
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                HkWorld havokWorld = this.HavokWorld;
            }
            if (this.Ragdoll != null)
            {
                foreach (HkRigidBody body in this.Ragdoll.RigidBodies)
                {
                    uint info = HkGroupFilter.CalcFilterInfo(0x1f, 0, 0, 0);
                    body.SetCollisionFilterInfo(info);
                    body.LinearVelocity = Vector3.Zero;
                    body.AngularVelocity = Vector3.Zero;
                    this.HavokWorld.RefreshCollisionFilterOnEntity(body);
                }
            }
        }

        public void EnableBatched()
        {
            if (!base.m_enabled)
            {
                base.m_enabled = true;
                if (base.Entity.InScene)
                {
                    this.m_batchRequest = true;
                    this.Activate();
                    this.m_batchRequest = false;
                }
                if (base.EnabledChanged != null)
                {
                    base.EnabledChanged();
                }
            }
        }

        private void Entity_OnClose(IMyEntity obj)
        {
            this.UnweldAll(true);
        }

        public void FinishAddBatch()
        {
            this.ActivateCollision();
            if (base.EnabledChanged != null)
            {
                base.EnabledChanged();
            }
            foreach (HkConstraint constraint in this.m_constraintsAddBatch)
            {
                if (IsConstraintValid(constraint))
                {
                    this.m_world.AddConstraint(constraint);
                }
            }
            this.m_constraintsAddBatch.Clear();
            if (this.CharacterProxy != null)
            {
                HkEntity rigidBody = this.CharacterProxy.GetRigidBody();
                if (rigidBody != null)
                {
                    this.m_world.RefreshCollisionFilterOnEntity(rigidBody);
                }
            }
            if (this.ReactivateRagdoll)
            {
                this.GetRigidBodyMatrix(out this.m_bodyMatrix, false);
                this.ActivateRagdoll(this.m_bodyMatrix);
                this.ReactivateRagdoll = false;
            }
        }

        public void FinishRemoveBatch(object userData)
        {
            HkWorld world = (HkWorld) userData;
            foreach (HkConstraint constraint in this.m_constraintsRemoveBatch)
            {
                if (IsConstraintValid(constraint, false))
                {
                    world.RemoveConstraint(constraint);
                }
            }
            if (this.IsRagdollModeActive)
            {
                this.ReactivateRagdoll = true;
                this.CloseRagdollMode(world);
            }
            this.m_constraintsRemoveBatch.Clear();
        }

        public override void ForceActivate()
        {
            if (this.IsInWorld && (this.RigidBody != null))
            {
                this.RigidBody.ForceActivate();
                this.m_world.ActiveRigidBodies.Add(this.RigidBody);
            }
        }

        public virtual unsafe void FracturedBody_AfterReplaceBody(ref HkdReplaceBodyEvent e)
        {
            if (Sync.IsServer)
            {
                e.GetNewBodies(this.m_tmpLst);
                if (this.m_tmpLst.Count != 0)
                {
                    MyPhysics.RemoveDestructions(this.RigidBody);
                    foreach (HkdBreakableBodyInfo info in this.m_tmpLst)
                    {
                        HkdBreakableBody breakableBody = MyFracturedPiecesManager.Static.GetBreakableBody(info);
                        MatrixD rigidBodyMatrix = breakableBody.GetRigidBody().GetRigidBodyMatrix();
                        MatrixD* xdPtr1 = (MatrixD*) ref rigidBodyMatrix;
                        xdPtr1.Translation = this.ClusterToWorld((Vector3) rigidBodyMatrix.Translation);
                        if (MyDestructionHelper.CreateFracturePiece(breakableBody, ref rigidBodyMatrix, (base.Entity as MyFracturedPiece).OriginalBlocks, null, true) == null)
                        {
                            MyFracturedPiecesManager.Static.ReturnToPool(breakableBody);
                        }
                    }
                    this.m_tmpLst.Clear();
                    this.BreakableBody.AfterReplaceBody -= new BreakableBodyReplaced(this.FracturedBody_AfterReplaceBody);
                    MyFracturedPiecesManager.Static.RemoveFracturePiece(base.Entity as MyFracturedPiece, 0f, false, true);
                }
            }
        }

        protected static void GetInfoFromFlags(HkRigidBodyCinfo rbInfo, RigidBodyFlag flags)
        {
            if ((flags & RigidBodyFlag.RBF_STATIC) > RigidBodyFlag.RBF_DEFAULT)
            {
                rbInfo.MotionType = HkMotionType.Fixed;
                rbInfo.QualityType = HkCollidableQualityType.Fixed;
            }
            else if ((flags & RigidBodyFlag.RBF_BULLET) > RigidBodyFlag.RBF_DEFAULT)
            {
                rbInfo.MotionType = HkMotionType.Dynamic;
                rbInfo.QualityType = HkCollidableQualityType.Bullet;
            }
            else if ((flags & RigidBodyFlag.RBF_KINEMATIC) > RigidBodyFlag.RBF_DEFAULT)
            {
                rbInfo.MotionType = HkMotionType.Keyframed;
                rbInfo.QualityType = HkCollidableQualityType.Keyframed;
            }
            else if ((flags & RigidBodyFlag.RBF_DOUBLED_KINEMATIC) > RigidBodyFlag.RBF_DEFAULT)
            {
                rbInfo.MotionType = HkMotionType.Dynamic;
                rbInfo.QualityType = HkCollidableQualityType.Moving;
            }
            else if ((flags & RigidBodyFlag.RBF_DISABLE_COLLISION_RESPONSE) > RigidBodyFlag.RBF_DEFAULT)
            {
                rbInfo.MotionType = HkMotionType.Fixed;
                rbInfo.QualityType = HkCollidableQualityType.Fixed;
            }
            else if ((flags & RigidBodyFlag.RBF_DEBRIS) > RigidBodyFlag.RBF_DEFAULT)
            {
                rbInfo.MotionType = HkMotionType.Dynamic;
                rbInfo.QualityType = HkCollidableQualityType.Debris;
                rbInfo.SolverDeactivation = HkSolverDeactivation.Max;
            }
            else if ((flags & RigidBodyFlag.RBF_KEYFRAMED_REPORTING) > RigidBodyFlag.RBF_DEFAULT)
            {
                rbInfo.MotionType = HkMotionType.Keyframed;
                rbInfo.QualityType = HkCollidableQualityType.KeyframedReporting;
            }
            else
            {
                rbInfo.MotionType = HkMotionType.Dynamic;
                rbInfo.QualityType = HkCollidableQualityType.Moving;
            }
            if ((flags & RigidBodyFlag.RBF_UNLOCKED_SPEEDS) > RigidBodyFlag.RBF_DEFAULT)
            {
                rbInfo.MaxLinearVelocity = MyGridPhysics.LargeShipMaxLinearVelocity() * 10f;
                rbInfo.MaxAngularVelocity = MyGridPhysics.GetLargeShipMaxAngularVelocity() * 10f;
            }
        }

        private static unsafe HkMassProperties? GetMassPropertiesFromDefinition(MyPhysicsBodyComponentDefinition physicsBodyComponentDefinition, MyModelComponentDefinition modelComponentDefinition)
        {
            HkMassProperties?* nullablePtr1;
            HkMassProperties? nullable = null;
            MyObjectBuilder_PhysicsComponentDefinitionBase.MyMassPropertiesComputationType massPropertiesComputation = physicsBodyComponentDefinition.MassPropertiesComputation;
            if ((massPropertiesComputation != MyObjectBuilder_PhysicsComponentDefinitionBase.MyMassPropertiesComputationType.None) && (massPropertiesComputation == MyObjectBuilder_PhysicsComponentDefinitionBase.MyMassPropertiesComputationType.Box))
            {
                nullablePtr1 = (HkMassProperties?*) new HkMassProperties?(HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(modelComponentDefinition.Size / 2f, MyPerGameSettings.Destruction ? MyDestructionHelper.MassToHavok(modelComponentDefinition.Mass) : modelComponentDefinition.Mass));
            }
            nullablePtr1 = (HkMassProperties?*) ref nullable;
            return nullable;
        }

        protected Matrix GetRigidBodyMatrix()
        {
            Vector3D objectOffset = MyPhysics.GetObjectOffset(this.ClusterObjectID);
            return Matrix.CreateWorld((Vector3.TransformNormal(base.Center, base.Entity.WorldMatrix) + base.Entity.GetPosition()) - objectOffset, (Vector3) base.Entity.WorldMatrix.Forward, (Vector3) base.Entity.WorldMatrix.Up);
        }

        protected unsafe Matrix GetRigidBodyMatrix(MatrixD worldMatrix)
        {
            if (base.Center != Vector3.Zero)
            {
                MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                xdPtr1.Translation += Vector3D.TransformNormal(base.Center, ref worldMatrix);
            }
            MatrixD* xdPtr2 = (MatrixD*) ref worldMatrix;
            xdPtr2.Translation -= this.Offset;
            return (Matrix) worldMatrix;
        }

        protected unsafe void GetRigidBodyMatrix(out Matrix m, bool useCenterOffset = true)
        {
            MatrixD worldMatrix = base.Entity.WorldMatrix;
            if ((base.Center != Vector3.Zero) & useCenterOffset)
            {
                MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                xdPtr1.Translation += Vector3.TransformNormal(base.Center, worldMatrix);
            }
            MatrixD* xdPtr2 = (MatrixD*) ref worldMatrix;
            xdPtr2.Translation -= this.Offset;
            m = (Matrix) worldMatrix;
        }

        public virtual unsafe HkShape GetShape()
        {
            if (this.WeldedRigidBody != null)
            {
                return this.WeldedRigidBody.GetShape();
            }
            HkShape shape = this.RigidBody.GetShape();
            if (shape.ShapeType == HkShapeType.List)
            {
                HkShapeContainerIterator container = this.RigidBody.GetShape().GetContainer();
                while (container.IsValid)
                {
                    HkShape shape3 = ((HkShapeContainerIterator*) ref container).GetShape(container.CurrentShapeKey);
                    if (this.RigidBody.GetGcRoot() == shape3.UserData)
                    {
                        return shape3;
                    }
                    container.Next();
                }
            }
            return shape;
        }

        public override Vector3 GetVelocityAtPoint(Vector3D worldPos)
        {
            Vector3 vector = (Vector3) this.WorldToCluster(worldPos);
            return ((this.RigidBody == null) ? Vector3.Zero : this.RigidBody.GetVelocityAtPoint(vector));
        }

        public override void GetVelocityAtPointLocal(ref Vector3D worldPos, out Vector3 linearVelocity)
        {
            Vector3 vector = (Vector3) (worldPos - this.CenterOfMassWorld);
            linearVelocity = Vector3.Cross(this.AngularVelocityLocal, vector);
            linearVelocity.Add(this.LinearVelocity);
        }

        public override unsafe MatrixD GetWorldMatrix()
        {
            MatrixD rigidBodyMatrix;
            if (this.WeldInfo.Parent != null)
            {
                return (this.WeldInfo.Transform * this.WeldInfo.Parent.GetWorldMatrix());
            }
            if (this.RigidBody != null)
            {
                rigidBodyMatrix = this.RigidBody.GetRigidBodyMatrix();
                MatrixD* xdPtr1 = (MatrixD*) ref rigidBodyMatrix;
                xdPtr1.Translation += this.Offset;
            }
            else if (this.RigidBody2 != null)
            {
                rigidBodyMatrix = this.RigidBody2.GetRigidBodyMatrix();
                MatrixD* xdPtr2 = (MatrixD*) ref rigidBodyMatrix;
                xdPtr2.Translation += this.Offset;
            }
            else if (this.CharacterProxy != null)
            {
                MatrixD rigidBodyTransform = this.CharacterProxy.GetRigidBodyTransform();
                rigidBodyTransform.Translation = this.CharacterProxy.Position + this.Offset;
                rigidBodyMatrix = rigidBodyTransform;
            }
            else
            {
                if ((this.Ragdoll != null) & this.IsRagdollModeActive)
                {
                    rigidBodyMatrix = this.Ragdoll.WorldMatrix;
                    MatrixD* xdPtr3 = (MatrixD*) ref rigidBodyMatrix;
                    xdPtr3.Translation = rigidBodyMatrix.Translation + this.Offset;
                    return rigidBodyMatrix;
                }
                rigidBodyMatrix = MatrixD.Identity;
            }
            if (base.Center != Vector3.Zero)
            {
                MatrixD* xdPtr4 = (MatrixD*) ref rigidBodyMatrix;
                xdPtr4.Translation -= Vector3D.TransformNormal(base.Center, ref rigidBodyMatrix);
            }
            return rigidBodyMatrix;
        }

        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
            this.Definition = definition as MyPhysicsBodyComponentDefinition;
        }

        private void InitializeRigidBodyFromModel()
        {
            if (((this.Definition != null) && ((this.RigidBody == null) && this.Definition.CreateFromCollisionObject)) && base.Container.Has<MyModelComponent>())
            {
                MyModelComponent component = base.Container.Get<MyModelComponent>();
                if (((component.Definition != null) && (component.ModelCollision != null)) && (component.ModelCollision.HavokCollisionShapes.Length >= 1))
                {
                    HkMassProperties? massPropertiesFromDefinition = GetMassPropertiesFromDefinition(this.Definition, component.Definition);
                    this.CreateFromCollisionObject(component.ModelCollision.HavokCollisionShapes[0], Vector3.Zero, base.Entity.WorldMatrix, massPropertiesFromDefinition, (this.Definition.CollisionLayer != null) ? MyPhysics.GetCollisionLayer(this.Definition.CollisionLayer) : 15);
                }
            }
        }

        protected void InvokeOnBodyActiveStateChanged(bool active)
        {
            this.m_onBodyActiveStateChangedHandler.InvokeIfNotNull<MyPhysicsComponentBase, bool>(this, active);
        }

        public static bool IsConstraintValid(HkConstraint constraint) => 
            IsConstraintValid(constraint, true);

        private static bool IsConstraintValid(HkConstraint constraint, bool checkBodiesInWorld)
        {
            if (constraint == null)
            {
                return false;
            }
            if (constraint.IsDisposed)
            {
                return false;
            }
            HkRigidBody rigidBodyA = constraint.RigidBodyA;
            HkRigidBody rigidBodyB = constraint.RigidBodyB;
            return ((rigidBodyA != null) && ((rigidBodyB != null) && (!checkBodiesInWorld || (rigidBodyA.InWorld && (rigidBodyB.InWorld && ReferenceEquals(((MyPhysicsBody) rigidBodyA.UserObject).HavokWorld, ((MyPhysicsBody) rigidBodyB.UserObject).HavokWorld))))));
        }

        private static bool IsPhantomOrSubPart(HkRigidBody rigidBody)
        {
            MyPhysicsBody userObject = (MyPhysicsBody) rigidBody.UserObject;
            return (userObject.IsPhantom || userObject.IsSubpart);
        }

        protected void NotifyConstraintsAddedToWorld()
        {
            using (List<HkConstraint>.Enumerator enumerator = m_notifyConstraints.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.NotifyAddedToWorld();
                }
            }
            m_notifyConstraints.Clear();
        }

        protected void NotifyConstraintsRemovedFromWorld()
        {
            m_notifyConstraints.AssertEmpty<HkConstraint>();
            HkRigidBody rigidBody = this.RigidBody;
            if (rigidBody != null)
            {
                HkConstraint.GetAttachedConstraints(rigidBody, m_notifyConstraints);
            }
            using (List<HkConstraint>.Enumerator enumerator = m_notifyConstraints.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.NotifyRemovedFromWorld();
                }
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            if (this.Definition != null)
            {
                this.InitializeRigidBodyFromModel();
                this.RegisterForEntityEvent(MyModelComponent.ModelChanged, new MyEntityContainerEventExtensions.EntityEventHandler(this.OnModelChanged));
            }
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            if (this.Definition != null)
            {
                this.Enabled = true;
                if (this.Definition.ForceActivate)
                {
                    this.ForceActivate();
                }
            }
        }

        private void OnContactPointCallback(ref HkContactPointEvent e)
        {
            if (this.m_contactPointCallbackHandler != null)
            {
                MyPhysics.MyContactPointEvent event2 = new MyPhysics.MyContactPointEvent {
                    ContactPointEvent = e,
                    Position = e.ContactPoint.Position + this.Offset
                };
                this.m_contactPointCallbackHandler(ref event2);
            }
        }

        private void OnContactSoundCallback(ref HkContactPointEvent e)
        {
            if (Sync.IsServer && MyAudioComponent.ShouldPlayContactSound(base.Entity.EntityId, e.EventType))
            {
                ContactPointWrapper wrap = new ContactPointWrapper(ref e);
                wrap.WorldPosition = this.ClusterToWorld(wrap.position);
                MySandboxGame.Static.Invoke(() => MyAudioComponent.PlayContactSound(wrap, this.Entity), "MyAudioComponent::PlayContactSound");
            }
        }

        private void OnDynamicRigidBodyActivated(HkEntity entity)
        {
            this.SynchronizeKeyframedRigidBody();
            this.InvokeOnBodyActiveStateChanged(true);
        }

        private void OnDynamicRigidBodyDeactivated(HkEntity entity)
        {
            this.SynchronizeKeyframedRigidBody();
            this.InvokeOnBodyActiveStateChanged(false);
        }

        private void OnModelChanged(MyEntityContainerEventExtensions.EntityEventParams eventParams)
        {
            this.Close();
            this.InitializeRigidBodyFromModel();
        }

        public virtual void OnMotion(HkRigidBody rbo, float step, bool fromParent = false)
        {
            if (rbo != this.RigidBody2)
            {
                int num1;
                if (base.Entity == null)
                {
                    return;
                }
                if (!this.IsSubpart && (base.Entity.Parent != null))
                {
                    return;
                }
                if (base.Flags == RigidBodyFlag.RBF_DISABLE_COLLISION_RESPONSE)
                {
                    return;
                }
                if (base.IsPhantom)
                {
                    return;
                }
                if (base.CanUpdateAccelerations)
                {
                    base.UpdateAccelerations();
                }
                if (this.RigidBody2 != null)
                {
                    if (this.IsSubpart)
                    {
                        goto TR_0005;
                    }
                    else if (ReferenceEquals(this.m_lastWorld, this.HavokWorld))
                    {
                        Matrix matrix = rbo.PredictRigidBodyMatrix(0.01666667f, this.HavokWorld);
                        Quaternion nextOrientation = Quaternion.CreateFromRotationMatrix(matrix);
                        VRageMath.Vector4 nextPosition = new VRageMath.Vector4(matrix.Translation.X, matrix.Translation.Y, matrix.Translation.Z, 0f);
                        HkKeyFrameUtility.ApplyHardKeyFrame(ref nextPosition, ref nextOrientation, 60f, this.RigidBody2);
                        this.RigidBody2.AngularVelocity = (this.RigidBody2.AngularVelocity * 0.1f) + (rbo.AngularVelocity * 0.9f);
                    }
                    else
                    {
                        goto TR_0005;
                    }
                }
                this.m_lastWorld = this.HavokWorld;
                int num = (base.Entity is MyFloatingObject) ? 600 : 60;
                Vector3D vectord = this.GetWorldMatrix().Translation - base.Entity.PositionComp.GetPosition();
                this.m_motionCounter++;
                float num2 = this.LinearVelocity.LengthSquared();
                if (((num2 > 0.0001f) || (this.m_motionCounter > num)) || (vectord.LengthSquared() > 9.9999999747524271E-07))
                {
                    num1 = 1;
                }
                else
                {
                    num1 = (int) (this.AngularVelocity.LengthSquared() > 0.0001f);
                }
                if ((num1 | fromParent) == 0)
                {
                    this.UpdateInterpolatedVelocities(rbo, false);
                }
                else
                {
                    float num3 = rbo.MaxLinearVelocity * 1.1f;
                    if (num2 > (num3 * num3))
                    {
                        object[] objArray1 = new object[] { "Clamping velocity for: ", base.Entity.EntityId, " ", num2.ToString("F2"), "->", num3.ToString("F2") };
                        MyLog.Default.WriteLine(string.Concat(objArray1));
                        rbo.LinearVelocity *= (1f / ((float) Math.Sqrt((double) num2))) * num3;
                    }
                    MatrixD worldMatrix = this.GetWorldMatrix();
                    base.Entity.PositionComp.SetWorldMatrix(worldMatrix, this, false, true, true, false, false, false);
                    this.UpdateInterpolatedVelocities(rbo, true);
                    this.m_motionCounter = 0;
                    this.m_bodyMatrix = rbo.GetRigidBodyMatrix();
                    using (HashSet<MyPhysicsBody>.Enumerator enumerator = this.WeldInfo.Children.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.OnMotion(rbo, step, true);
                        }
                    }
                    if (this.WeldInfo.Parent == null)
                    {
                        this.UpdateCluster();
                        return;
                    }
                }
            }
            return;
        TR_0005:
            this.RigidBody2.Motion.SetWorldMatrix(rbo.GetRigidBodyMatrix());
            this.RigidBody2.LinearVelocity = rbo.LinearVelocity;
            this.RigidBody2.AngularVelocity = rbo.AngularVelocity;
            this.m_lastWorld = this.HavokWorld;
        }

        public virtual void OnMotionKinematic(HkRigidBody rbo)
        {
            if (rbo.MarkedForVelocityRecompute)
            {
                this.RigidBody.SetCustomVelocity(this.RigidBody.LinearVelocity, true);
            }
        }

        private void OnRagdollAddedToWorld(HkRagdoll ragdoll)
        {
            bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
            this.Ragdoll.Activate();
            this.Ragdoll.EnableConstraints();
            HkConstraintStabilizationUtil.StabilizeRagdollInertias(ragdoll, 1f, 0f);
        }

        protected virtual void OnUnwelded(MyPhysicsBody other)
        {
        }

        protected virtual void OnWelded(MyPhysicsBody other)
        {
        }

        public override void OnWorldPositionChanged(object source)
        {
            if (this.IsInWorld)
            {
                Matrix matrix;
                Vector3 zero = Vector3.Zero;
                IMyEntity topMostParent = base.Entity.GetTopMostParent(null);
                if (topMostParent.Physics != null)
                {
                    zero = topMostParent.Physics.LinearVelocity;
                }
                if (!this.IsWelded && (this.ClusterObjectID != ulong.MaxValue))
                {
                    MyPhysics.MoveObject(this.ClusterObjectID, topMostParent.WorldAABB, zero);
                }
                this.GetRigidBodyMatrix(out matrix, true);
                if (!matrix.EqualsFast(ref this.m_bodyMatrix, 0.0001f) || (this.CharacterProxy != null))
                {
                    this.m_bodyMatrix = matrix;
                    if (this.RigidBody != null)
                    {
                        this.RigidBody.SetWorldMatrix(this.m_bodyMatrix);
                    }
                    if (this.RigidBody2 != null)
                    {
                        this.RigidBody2.SetWorldMatrix(this.m_bodyMatrix);
                    }
                    if (this.CharacterProxy != null)
                    {
                        this.CharacterProxy.Speed = 0f;
                        this.CharacterProxy.SetRigidBodyTransform(ref this.m_bodyMatrix);
                    }
                    if ((this.Ragdoll != null) && this.IsRagdollModeActive)
                    {
                        bool flag = source is MyCockpit;
                        bool flag2 = source == MyGridPhysicalHierarchy.Static;
                        if (flag | flag2)
                        {
                            Matrix matrix2;
                            int num1;
                            if (flag)
                            {
                                this.Ragdoll.ResetToRigPose();
                            }
                            this.GetRigidBodyMatrix(out matrix2, false);
                            MyCharacter entity = (MyCharacter) base.Entity;
                            bool flag3 = flag2 && !entity.IsClientPredicted;
                            if (!entity.m_positionResetFromServer)
                            {
                                num1 = (int) (Vector3D.DistanceSquared(this.Ragdoll.WorldMatrix.Translation, matrix2.Translation) > 0.5);
                            }
                            else
                            {
                                num1 = 1;
                            }
                            bool flag4 = (bool) num1;
                            this.Ragdoll.SetWorldMatrix(matrix2, !flag4 & flag3, true);
                            if (flag)
                            {
                                this.SetRagdollVelocities(null, null);
                            }
                        }
                    }
                }
            }
        }

        public void RecreateWeldedShape()
        {
            if (this.WeldInfo.Children.Count != 0)
            {
                this.RecreateWeldedShape(this.GetShape());
            }
        }

        private void RecreateWeldedShape(HkShape thisShape)
        {
            if ((this.RigidBody != null) && !this.RigidBody.IsDisposed)
            {
                if (this.WeldInfo.Children.Count == 0)
                {
                    this.RigidBody.SetShape(thisShape);
                    if (this.RigidBody2 != null)
                    {
                        this.RigidBody2.SetShape(thisShape);
                    }
                }
                else
                {
                    this.m_tmpShapeList.Add(thisShape);
                    foreach (MyPhysicsBody body in this.WeldInfo.Children)
                    {
                        HkTransformShape shape2 = new HkTransformShape(body.WeldedRigidBody.GetShape(), ref body.WeldInfo.Transform);
                        HkShape.SetUserData((HkShape) shape2, body.WeldedRigidBody);
                        this.m_tmpShapeList.Add((HkShape) shape2);
                        if (this.m_tmpShapeList.Count == 0x80)
                        {
                            break;
                        }
                    }
                    HkSmartListShape shape = new HkSmartListShape(0);
                    foreach (HkShape shape3 in this.m_tmpShapeList)
                    {
                        shape.AddShape(shape3);
                    }
                    this.RigidBody.SetShape((HkShape) shape);
                    if (this.RigidBody2 != null)
                    {
                        this.RigidBody2.SetShape((HkShape) shape);
                    }
                    shape.Base.RemoveReference();
                    this.WeldedMarkBreakable();
                    int num = 1;
                    while (true)
                    {
                        if (num >= this.m_tmpShapeList.Count)
                        {
                            this.m_tmpShapeList.Clear();
                            this.UpdateMassProps();
                            break;
                        }
                        this.m_tmpShapeList[num].RemoveReference();
                        num++;
                    }
                }
            }
        }

        private void RegisterActivationCallbacksIfNeeded()
        {
            if ((!this.m_activationCallbackRegistered && this.NeedsActivationCallback) && (this.m_rigidBody != null))
            {
                this.m_activationCallbackRegistered = true;
                this.m_rigidBody.Activated += new HkEntityHandler(this.OnDynamicRigidBodyActivated);
                this.m_rigidBody.Deactivated += new HkEntityHandler(this.OnDynamicRigidBodyDeactivated);
            }
        }

        private void RegisterContactPointCallbackIfNeeded()
        {
            if ((!this.m_contactPointCallbackRegistered && this.NeedsContactPointCallback) && (this.m_rigidBody != null))
            {
                this.m_contactPointCallbackRegistered = true;
                this.m_rigidBody.ContactPointCallback += new ContactPointEventHandler(this.OnContactPointCallback);
            }
        }

        public void RemoveConstraint(HkConstraint constraint)
        {
            if (this.IsWelded)
            {
                this.m_constraints.Remove(constraint);
                this.WeldInfo.Parent.RemoveConstraint(constraint);
            }
            else
            {
                this.m_constraints.Remove(constraint);
                if (this.HavokWorld != null)
                {
                    this.HavokWorld.RemoveConstraint(constraint);
                }
            }
        }

        private void RemoveConstraints(HkRigidBody hkRigidBody)
        {
            foreach (HkConstraint constraint in this.m_constraints)
            {
                if ((constraint.IsDisposed || (constraint.RigidBodyA == hkRigidBody)) || (constraint.RigidBodyB == hkRigidBody))
                {
                    this.m_constraintsRemoveBatch.Add(constraint);
                }
            }
            foreach (HkConstraint constraint2 in this.m_constraintsRemoveBatch)
            {
                this.m_constraints.Remove(constraint2);
                if (!constraint2.IsDisposed && constraint2.InWorld)
                {
                    this.HavokWorld.RemoveConstraint(constraint2);
                }
            }
            this.m_constraintsRemoveBatch.Clear();
        }

        public void SetRagdollDefaults()
        {
            float num;
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyPhysicsBody.SetRagdollDefaults");
            }
            bool isKeyframed = this.Ragdoll.IsKeyframed;
            this.Ragdoll.SetToDynamic();
            if ((base.Entity as MyCharacter).Definition.Mass <= 1f)
            {
                num = 80f;
            }
            float num2 = 0f;
            using (List<HkRigidBody>.Enumerator enumerator = this.Ragdoll.RigidBodies.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    VRageMath.Vector4 vector;
                    VRageMath.Vector4 vector2;
                    enumerator.Current.GetShape().GetLocalAABB(0.01f, out vector, out vector2);
                    VRageMath.Vector4 vector3 = vector2 - vector;
                    num2 += vector3.Length();
                }
            }
            if (num2 <= 0f)
            {
                num2 = 1f;
            }
            foreach (HkRigidBody body in this.Ragdoll.RigidBodies)
            {
                VRageMath.Vector4 vector4;
                VRageMath.Vector4 vector5;
                body.MaxLinearVelocity = 1000f;
                body.MaxAngularVelocity = 1000f;
                HkShape shape = body.GetShape();
                shape.GetLocalAABB(0.01f, out vector4, out vector5);
                float num4 = (vector5 - vector4).Length();
                float m = (num / num2) * num4;
                body.Mass = MyPerGameSettings.Destruction ? MyDestructionHelper.MassToHavok(m) : m;
                float convexRadius = shape.ConvexRadius;
                if (shape.ShapeType != HkShapeType.Capsule)
                {
                    body.InertiaTensor = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties((Vector3.One * num4) * 0.5f, body.Mass).InertiaTensor;
                }
                else
                {
                    HkCapsuleShape shape3 = (HkCapsuleShape) shape;
                    body.InertiaTensor = HkInertiaTensorComputer.ComputeCapsuleVolumeMassProperties(shape3.VertexA, shape3.VertexB, convexRadius, body.Mass).InertiaTensor;
                }
                body.AngularDamping = 0.005f;
                body.LinearDamping = 0f;
                body.Friction = 6f;
                body.AllowedPenetrationDepth = 0.1f;
                body.Restitution = 0.05f;
            }
            this.Ragdoll.OptimizeInertiasOfConstraintTree();
            if (isKeyframed)
            {
                this.Ragdoll.SetToKeyframed();
            }
            foreach (HkConstraint constraint in this.Ragdoll.Constraints)
            {
                switch (constraint.ConstraintData)
                {
                    case (HkRagdollConstraintData _):
                        (constraint.ConstraintData as HkRagdollConstraintData).MaxFrictionTorque = MyPerGameSettings.Destruction ? MyDestructionHelper.MassToHavok(0.5f) : 3f;
                        continue;
                        break;
                }
                if (constraint.ConstraintData is HkFixedConstraintData)
                {
                    HkFixedConstraintData constraintData = constraint.ConstraintData as HkFixedConstraintData;
                    constraintData.MaximumLinearImpulse = 3.40282E+28f;
                    constraintData.MaximumAngularImpulse = 3.40282E+28f;
                    continue;
                }
                if (constraint.ConstraintData is HkHingeConstraintData)
                {
                    HkHingeConstraintData constraintData = constraint.ConstraintData as HkHingeConstraintData;
                    constraintData.MaximumAngularImpulse = 3.40282E+28f;
                    constraintData.MaximumLinearImpulse = 3.40282E+28f;
                    continue;
                }
                if (constraint.ConstraintData is HkLimitedHingeConstraintData)
                {
                    (constraint.ConstraintData as HkLimitedHingeConstraintData).MaxFrictionTorque = MyPerGameSettings.Destruction ? MyDestructionHelper.MassToHavok(0.5f) : 3f;
                }
            }
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyPhysicsBody.SetRagdollDefaults FINISHED");
            }
        }

        public void SetRagdollVelocities(List<int> bodiesToUpdate = null, HkRigidBody leadingBody = null)
        {
            List<HkRigidBody> rigidBodies = this.Ragdoll.RigidBodies;
            if ((leadingBody == null) && (this.CharacterProxy != null))
            {
                HkRigidBody hitRigidBody = this.CharacterProxy.GetHitRigidBody();
                if (hitRigidBody != null)
                {
                    leadingBody = hitRigidBody;
                }
            }
            if (leadingBody == null)
            {
                leadingBody = rigidBodies[0];
            }
            Vector3 angularVelocity = leadingBody.AngularVelocity;
            if (bodiesToUpdate != null)
            {
                foreach (int num in bodiesToUpdate)
                {
                    HkRigidBody body2 = rigidBodies[num];
                    body2.AngularVelocity = angularVelocity;
                    body2.LinearVelocity = leadingBody.GetVelocityAtPoint(body2.Position);
                }
            }
            else
            {
                foreach (HkRigidBody body3 in rigidBodies)
                {
                    body3.AngularVelocity = angularVelocity;
                    body3.LinearVelocity = leadingBody.GetVelocityAtPoint(body3.Position);
                }
            }
        }

        public unsafe void SwitchToRagdollMode(bool deadMode = true, int firstRagdollSubID = 1)
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyPhysicsBody.SwitchToRagdollMode");
            }
            if ((this.HavokWorld == null) || !this.Enabled)
            {
                this.SwitchToRagdollModeOnActivate = true;
                this.m_ragdollDeadMode = deadMode;
            }
            else if (!this.IsRagdollModeActive)
            {
                MatrixD worldMatrix = base.Entity.WorldMatrix;
                MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                xdPtr1.Translation = this.WorldToCluster(worldMatrix.Translation);
                if (this.RagdollSystemGroupCollisionFilterID == 0)
                {
                    this.RagdollSystemGroupCollisionFilterID = this.m_world.GetCollisionFilter().GetNewSystemGroup();
                }
                this.Ragdoll.SetToKeyframed();
                this.Ragdoll.GenerateRigidBodiesCollisionFilters(deadMode ? 0x12 : 0x1f, this.RagdollSystemGroupCollisionFilterID, firstRagdollSubID);
                this.Ragdoll.ResetToRigPose();
                this.Ragdoll.SetWorldMatrix(worldMatrix, false, false);
                if (!deadMode)
                {
                    this.SetRagdollVelocities(null, null);
                }
                else
                {
                    this.Ragdoll.SetToDynamic();
                    this.SetRagdollVelocities(null, this.RigidBody);
                }
                if ((this.CharacterProxy != null) & deadMode)
                {
                    this.CharacterProxy.Deactivate(this.HavokWorld);
                    this.CharacterProxy.Dispose();
                    this.CharacterProxy = null;
                }
                if ((this.RigidBody != null) & deadMode)
                {
                    this.RigidBody.Deactivate();
                    this.HavokWorld.RemoveRigidBody(this.RigidBody);
                    this.RigidBody.Dispose();
                    this.RigidBody = null;
                }
                if ((this.RigidBody2 != null) & deadMode)
                {
                    this.RigidBody2.Deactivate();
                    this.HavokWorld.RemoveRigidBody(this.RigidBody2);
                    this.RigidBody2.Dispose();
                    this.RigidBody2 = null;
                }
                foreach (HkRigidBody local1 in this.Ragdoll.RigidBodies)
                {
                    local1.UserObject = this;
                    local1.Motion.SetDeactivationClass(deadMode ? HkSolverDeactivation.High : HkSolverDeactivation.Medium);
                }
                this.Ragdoll.OptimizeInertiasOfConstraintTree();
                if (!this.Ragdoll.InWorld)
                {
                    this.HavokWorld.AddRagdoll(this.Ragdoll);
                }
                this.Ragdoll.EnableConstraints();
                this.Ragdoll.Activate();
                this.m_ragdollDeadMode = deadMode;
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyPhysicsBody.SwitchToRagdollMode - FINISHED");
                }
            }
        }

        public void SynchronizeKeyframedRigidBody()
        {
            if (((this.RigidBody != null) && (this.RigidBody2 != null)) && (this.RigidBody.IsActive != this.RigidBody2.IsActive))
            {
                if (this.RigidBody.IsActive)
                {
                    this.RigidBody2.IsActive = true;
                }
                else
                {
                    this.RigidBody2.LinearVelocity = Vector3.Zero;
                    this.RigidBody2.AngularVelocity = Vector3.Zero;
                    this.RigidBody2.IsActive = false;
                }
            }
        }

        private void UnregisterActivationCallbacks()
        {
            if (this.m_activationCallbackRegistered)
            {
                this.m_activationCallbackRegistered = false;
                if (this.m_rigidBody != null)
                {
                    this.m_rigidBody.Activated -= new HkEntityHandler(this.OnDynamicRigidBodyActivated);
                    this.m_rigidBody.Deactivated -= new HkEntityHandler(this.OnDynamicRigidBodyDeactivated);
                }
            }
        }

        private void UnregisterContactPointCallback()
        {
            if (this.m_contactPointCallbackRegistered)
            {
                this.m_contactPointCallbackRegistered = false;
                if (this.m_rigidBody != null)
                {
                    this.m_rigidBody.ContactPointCallback -= new ContactPointEventHandler(this.OnContactPointCallback);
                }
            }
        }

        public void Unweld(bool insertInWorld = true)
        {
            this.WeldInfo.Parent.Unweld(this, insertInWorld, true);
        }

        public void Unweld(MyPhysicsBody other, bool insertToWorld = true, bool recreateShape = true)
        {
            if (this.IsWelded)
            {
                this.WeldInfo.Parent.Unweld(other, insertToWorld, recreateShape);
            }
            else if ((other.IsInWorld || (this.RigidBody == null)) || (other.WeldedRigidBody == null))
            {
                this.WeldInfo.Children.Remove(other);
            }
            else
            {
                Matrix rigidBodyMatrix = this.RigidBody.GetRigidBodyMatrix();
                other.WeldInfo.Parent = null;
                this.WeldInfo.Children.Remove(other);
                HkRigidBody rigidBody = other.RigidBody;
                other.RigidBody = other.WeldedRigidBody;
                other.WeldedRigidBody = null;
                if (!other.RigidBody.IsDisposed)
                {
                    other.RigidBody.SetWorldMatrix(other.WeldInfo.Transform * rigidBodyMatrix);
                    other.RigidBody.LinearVelocity = rigidBody.LinearVelocity;
                    other.WeldInfo.MassElement.Tranform = Matrix.Identity;
                    other.WeldInfo.Transform = Matrix.Identity;
                    if (other.RigidBody2 != null)
                    {
                        other.OnMotion(other.RigidBody, 0f, false);
                    }
                }
                other.ClusterObjectID = ulong.MaxValue;
                if (insertToWorld)
                {
                    other.Activate();
                    other.OnMotion(other.RigidBody, 0f, false);
                }
                if (this.WeldInfo.Children.Count == 0)
                {
                    recreateShape = false;
                    base.Entity.OnPhysicsChanged -= new Action<IMyEntity>(this.WeldedEntity_OnPhysicsChanged);
                    base.Entity.OnClose -= new Action<IMyEntity>(this.Entity_OnClose);
                    this.WeldedRigidBody.LinearVelocity = this.RigidBody.LinearVelocity;
                    this.WeldedRigidBody.AngularVelocity = this.RigidBody.AngularVelocity;
                    if (this.HavokWorld != null)
                    {
                        this.HavokWorld.RemoveRigidBody(this.RigidBody);
                    }
                    this.RigidBody.Dispose();
                    this.RigidBody = this.WeldedRigidBody;
                    this.WeldedRigidBody = null;
                    this.RigidBody.SetWorldMatrix(rigidBodyMatrix);
                    this.WeldInfo.Transform = Matrix.Identity;
                    if (this.HavokWorld != null)
                    {
                        this.HavokWorld.AddRigidBody(this.RigidBody);
                        this.ActivateCollision();
                    }
                    else if (!base.Entity.MarkedForClose)
                    {
                        this.Activate();
                    }
                    if (this.RigidBody2 != null)
                    {
                        this.RigidBody2.SetShape(this.RigidBody.GetShape());
                    }
                }
                if ((this.RigidBody != null) & recreateShape)
                {
                    this.RecreateWeldedShape(this.GetShape());
                }
                this.OnUnwelded(other);
                other.OnUnwelded(this);
            }
        }

        public void UnweldAll(bool insertInWorld)
        {
            while (this.WeldInfo.Children.Count > 1)
            {
                this.Unweld(this.WeldInfo.Children.First<MyPhysicsBody>(), insertInWorld, false);
            }
            if (this.WeldInfo.Children.Count > 0)
            {
                this.Unweld(this.WeldInfo.Children.First<MyPhysicsBody>(), insertInWorld, true);
            }
        }

        public void UpdateCluster()
        {
            if ((!MyPerGameSettings.LimitedWorld && (base.Entity != null)) && !base.Entity.Closed)
            {
                MyPhysics.MoveObject(this.ClusterObjectID, base.Entity.WorldAABB, this.LinearVelocity);
            }
        }

        public void UpdateConstraintForceDisable(HkConstraint constraint)
        {
            if (!(constraint.RigidBodyA.GetEntity(0) is MyCharacter) && !(constraint.RigidBodyB.GetEntity(0) is MyCharacter))
            {
                MyCubeGrid entity = base.Entity as MyCubeGrid;
                if ((((entity != null) && entity.IsClientPredicted) || IsPhantomOrSubPart(constraint.RigidBodyA)) || IsPhantomOrSubPart(constraint.RigidBodyB))
                {
                    constraint.ForceDisabled = false;
                }
                else
                {
                    constraint.ForceDisabled = true;
                }
            }
        }

        public void UpdateConstraintsForceDisable()
        {
            if (!MyFakes.MULTIPLAYER_CLIENT_CONSTRAINTS && !Sync.IsServer)
            {
                foreach (HkConstraint constraint in this.m_constraints)
                {
                    this.UpdateConstraintForceDisable(constraint);
                }
            }
        }

        public override void UpdateFromSystem()
        {
            if (((this.Definition != null) && (((this.Definition.UpdateFlags & MyObjectBuilder_PhysicsComponentDefinitionBase.MyUpdateFlags.Gravity) != 0) && (MyFakes.ENABLE_PLANETS && ((base.Entity != null) && ((base.Entity.PositionComp != null) && this.Enabled))))) && (this.RigidBody != null))
            {
                this.RigidBody.Gravity = MyGravityProviderSystem.CalculateNaturalGravityInPoint(base.Entity.PositionComp.GetPosition());
            }
        }

        private void UpdateInterpolatedVelocities(HkRigidBody rb, bool moved)
        {
            if (!rb.MarkedForVelocityRecompute)
            {
                if (this.m_lastComPosition != null)
                {
                    this.m_lastComPosition = null;
                    rb.SetCustomVelocity(Vector3.Zero, false);
                }
            }
            else
            {
                Vector3D centerOfMassWorld = this.CenterOfMassWorld;
                if ((this.m_lastComPosition != null) && (this.m_lastComLocal == rb.CenterOfMassLocal))
                {
                    Vector3 zero = Vector3.Zero;
                    if (moved)
                    {
                        zero = (Vector3) ((centerOfMassWorld - this.m_lastComPosition.Value) / 0.01666666753590107);
                    }
                    rb.SetCustomVelocity(zero, true);
                }
                rb.MarkedForVelocityRecompute = false;
                this.m_lastComPosition = new Vector3D?(centerOfMassWorld);
                this.m_lastComLocal = rb.CenterOfMassLocal;
            }
        }

        public void UpdateMassProps()
        {
            if (!this.RigidBody.IsFixedOrKeyframed)
            {
                if (this.WeldInfo.Parent != null)
                {
                    this.WeldInfo.Parent.UpdateMassProps();
                }
                else
                {
                    this.m_tmpElements.Add(this.WeldInfo.MassElement);
                    foreach (MyPhysicsBody body in this.WeldInfo.Children)
                    {
                        this.m_tmpElements.Add(body.WeldInfo.MassElement);
                    }
                    HkMassProperties properties = HkInertiaTensorComputer.CombineMassProperties(this.m_tmpElements);
                    this.RigidBody.SetMassProperties(ref properties);
                    this.m_tmpElements.Clear();
                }
            }
        }

        void MyClusterTree.IMyActivationHandler.Activate(object userData, ulong clusterObjectID)
        {
            this.Activate(userData, clusterObjectID);
        }

        void MyClusterTree.IMyActivationHandler.ActivateBatch(object userData, ulong clusterObjectID)
        {
            this.ActivateBatch(userData, clusterObjectID);
        }

        void MyClusterTree.IMyActivationHandler.Deactivate(object userData)
        {
            this.Deactivate(userData);
        }

        void MyClusterTree.IMyActivationHandler.DeactivateBatch(object userData)
        {
            this.DeactivateBatch(userData);
        }

        void MyClusterTree.IMyActivationHandler.FinishAddBatch()
        {
            this.FinishAddBatch();
        }

        void MyClusterTree.IMyActivationHandler.FinishRemoveBatch(object userData)
        {
            this.FinishRemoveBatch(userData);
        }

        public void Weld(MyPhysicsBody other, bool recreateShape = true)
        {
            if (!ReferenceEquals(other.WeldInfo.Parent, this))
            {
                if (other.IsWelded && !this.IsWelded)
                {
                    other.Weld(this, true);
                }
                else if (this.IsWelded)
                {
                    this.WeldInfo.Parent.Weld(other, true);
                }
                else
                {
                    if (other.WeldInfo.Children.Count > 0)
                    {
                        other.UnweldAll(false);
                    }
                    if (this.WeldInfo.Children.Count != 0)
                    {
                        this.GetShape();
                    }
                    else
                    {
                        this.WeldedRigidBody = this.RigidBody;
                        if (this.HavokWorld != null)
                        {
                            this.HavokWorld.RemoveRigidBody(this.WeldedRigidBody);
                        }
                        this.RigidBody = HkRigidBody.Clone(this.WeldedRigidBody);
                        if (this.HavokWorld != null)
                        {
                            this.HavokWorld.AddRigidBody(this.RigidBody);
                        }
                        HkShape.SetUserData(this.RigidBody.GetShape(), this.RigidBody);
                        base.Entity.OnPhysicsChanged += new Action<IMyEntity>(this.WeldedEntity_OnPhysicsChanged);
                        this.WeldInfo.UpdateMassProps(this.RigidBody);
                    }
                    other.Deactivate();
                    MatrixD xd = other.Entity.WorldMatrix * base.Entity.WorldMatrixInvScaled;
                    other.WeldInfo.Transform = (Matrix) xd;
                    other.WeldInfo.UpdateMassProps(other.RigidBody);
                    other.WeldedRigidBody = other.RigidBody;
                    other.RigidBody = this.RigidBody;
                    other.WeldInfo.Parent = this;
                    other.ClusterObjectID = this.ClusterObjectID;
                    this.WeldInfo.Children.Add(other);
                    this.OnWelded(other);
                    other.OnWelded(this);
                }
            }
        }

        public void Weld(MyPhysicsComponentBase other, bool recreateShape = true)
        {
            this.Weld(other as MyPhysicsBody, recreateShape);
        }

        private void WeldedEntity_OnPhysicsChanged(IMyEntity obj)
        {
            if ((base.Entity != null) && (base.Entity.Physics != null))
            {
                foreach (MyPhysicsBody body in this.WeldInfo.Children)
                {
                    if (body.Entity == null)
                    {
                        body.WeldInfo.Parent = null;
                        this.WeldInfo.Children.Remove(body);
                        if (obj.Physics != null)
                        {
                            this.Weld(obj.Physics as MyPhysicsBody, true);
                        }
                        break;
                    }
                }
                this.RecreateWeldedShape(this.GetShape());
            }
        }

        private void WeldedMarkBreakable()
        {
            if (this.HavokWorld != null)
            {
                MyGridPhysics physics = this as MyGridPhysics;
                if ((physics != null) && (physics.Entity as MyCubeGrid).BlocksDestructionEnabled)
                {
                    this.HavokWorld.BreakOffPartsUtil.MarkPieceBreakable(this.RigidBody, 0, physics.Shape.BreakImpulse);
                }
                uint shapeKey = 1;
                foreach (MyGridPhysics physics in this.WeldInfo.Children)
                {
                    if ((physics != null) && (physics.Entity as MyCubeGrid).BlocksDestructionEnabled)
                    {
                        this.HavokWorld.BreakOffPartsUtil.MarkPieceBreakable(this.RigidBody, shapeKey, physics.Shape.BreakImpulse);
                    }
                    shapeKey++;
                }
            }
        }

        public override Vector3D WorldToCluster(Vector3D worldPos) => 
            (worldPos - this.Offset);

        private bool NeedsActivationCallback =>
            ((this.m_rigidBody2 != null) || (this.m_onBodyActiveStateChangedHandler != null));

        private bool NeedsContactPointCallback =>
            (this.m_contactPointCallbackHandler != null);

        public override HkdBreakableBody BreakableBody
        {
            get => 
                this.m_breakableBody;
            set
            {
                this.m_breakableBody = value;
                this.RigidBody = (HkRigidBody) value;
            }
        }

        public int RagdollSystemGroupCollisionFilterID { get; private set; }

        public bool IsRagdollModeActive =>
            ((this.Ragdoll != null) ? this.Ragdoll.InWorld : false);

        public HkRagdoll Ragdoll
        {
            get => 
                this.m_ragdoll;
            set
            {
                this.m_ragdoll = value;
                if (this.m_ragdoll != null)
                {
                    this.m_ragdoll.AddedToWorld += new Action<HkRagdoll>(this.OnRagdollAddedToWorld);
                }
            }
        }

        public bool ReactivateRagdoll { get; set; }

        public bool SwitchToRagdollModeOnActivate { get; set; }

        public bool IsWelded =>
            (this.WeldInfo.Parent != null);

        [Obsolete]
        public MyWeldInfo WeldInfo =>
            this.m_weldInfo;

        public HkRigidBody WeldedRigidBody { get; protected set; }

        protected ulong ClusterObjectID
        {
            get => 
                this.m_clusterObjectID;
            set
            {
                this.m_clusterObjectID = value;
                this.Offset = (value == ulong.MaxValue) ? Vector3D.Zero : MyPhysics.GetObjectOffset(value);
                using (HashSet<MyPhysicsBody>.Enumerator enumerator = this.WeldInfo.Children.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Offset = this.Offset;
                    }
                }
            }
        }

        protected Vector3D Offset
        {
            get
            {
                IMyEntity topMostParent = base.Entity.GetTopMostParent(null);
                if (ReferenceEquals(topMostParent, base.Entity) || (topMostParent.Physics == null))
                {
                    return this.m_offset;
                }
                return ((MyPhysicsBody) topMostParent.Physics).Offset;
            }
            set => 
                (this.m_offset = value);
        }

        public MyPhysicsBodyComponentDefinition Definition { get; private set; }

        public HkWorld HavokWorld
        {
            get
            {
                if (this.IsWelded)
                {
                    return this.WeldInfo.Parent.m_world;
                }
                IMyEntity topMostParent = base.Entity.GetTopMostParent(null);
                if (ReferenceEquals(topMostParent, base.Entity) || (topMostParent.Physics == null))
                {
                    return this.m_world;
                }
                return ((MyPhysicsBody) topMostParent.Physics).HavokWorld;
            }
        }

        public virtual int HavokCollisionSystemID
        {
            get => 
                ((this.RigidBody != null) ? HkGroupFilter.GetSystemGroupFromFilterInfo(this.RigidBody.GetCollisionFilterInfo()) : 0);
            protected set
            {
                if (this.RigidBody != null)
                {
                    this.RigidBody.SetCollisionFilterInfo(HkGroupFilter.CalcFilterInfo(this.RigidBody.Layer, value, 1, 1));
                }
                if (this.RigidBody2 != null)
                {
                    this.RigidBody2.SetCollisionFilterInfo(HkGroupFilter.CalcFilterInfo(this.RigidBody2.Layer, value, 1, 1));
                }
            }
        }

        public override HkRigidBody RigidBody
        {
            get => 
                ((this.WeldInfo.Parent != null) ? this.WeldInfo.Parent.RigidBody : this.m_rigidBody);
            protected set
            {
                if (this.m_rigidBody != value)
                {
                    if ((this.m_rigidBody != null) && !this.m_rigidBody.IsDisposed)
                    {
                        this.m_rigidBody.ContactSoundCallback -= new ContactPointEventHandler(this.OnContactSoundCallback);
                        this.UnregisterContactPointCallback();
                        this.UnregisterActivationCallbacks();
                    }
                    this.m_rigidBody = value;
                    this.m_activationCallbackRegistered = false;
                    this.m_contactPointCallbackRegistered = false;
                    if (this.m_rigidBody != null)
                    {
                        this.RegisterActivationCallbacksIfNeeded();
                        this.RegisterContactPointCallbackIfNeeded();
                        this.m_rigidBody.ContactSoundCallback += new ContactPointEventHandler(this.OnContactSoundCallback);
                    }
                }
            }
        }

        public override HkRigidBody RigidBody2
        {
            get => 
                ((this.WeldInfo.Parent != null) ? this.WeldInfo.Parent.RigidBody2 : this.m_rigidBody2);
            protected set
            {
                if (this.m_rigidBody2 != value)
                {
                    this.m_rigidBody2 = value;
                    if (this.NeedsActivationCallback)
                    {
                        this.RegisterActivationCallbacksIfNeeded();
                    }
                    else
                    {
                        this.UnregisterActivationCallbacks();
                    }
                }
            }
        }

        public override float Mass
        {
            get
            {
                if (this.CharacterProxy != null)
                {
                    return this.CharacterProxy.Mass;
                }
                if (this.RigidBody == null)
                {
                    return ((this.Ragdoll == null) ? 0f : this.Ragdoll.Mass);
                }
                if ((MyMultiplayer.Static == null) || Sync.IsServer)
                {
                    return this.RigidBody.Mass;
                }
                return this.m_animatedClientMass;
            }
        }

        public override float Speed =>
            this.LinearVelocity.Length();

        public override float Friction
        {
            get => 
                this.RigidBody.Friction;
            set => 
                (this.RigidBody.Friction = value);
        }

        public override bool IsStatic =>
            ((this.RigidBody != null) && this.RigidBody.IsFixed);

        public override bool IsKinematic =>
            ((this.RigidBody != null) && (!this.RigidBody.IsFixed && this.RigidBody.IsFixedOrKeyframed));

        public bool IsSubpart { get; set; }

        public override bool IsActive =>
            ((this.RigidBody == null) ? ((this.CharacterProxy == null) ? ((this.Ragdoll != null) && this.Ragdoll.IsActive) : this.CharacterProxy.GetHitRigidBody().IsActive) : this.RigidBody.IsActive);

        public MyCharacterProxy CharacterProxy { get; set; }

        public int CharacterSystemGroupCollisionFilterID { get; private set; }

        public uint CharacterCollisionFilter { get; private set; }

        public override bool IsInWorld
        {
            get => 
                this.m_isInWorld;
            protected set => 
                (this.m_isInWorld = value);
        }

        public override bool ShapeChangeInProgress
        {
            get => 
                this.m_shapeChangeInProgress;
            set => 
                (this.m_shapeChangeInProgress = value);
        }

        public override Vector3 AngularVelocityLocal
        {
            get
            {
                if (!this.Enabled)
                {
                    return Vector3.Zero;
                }
                if (this.RigidBody != null)
                {
                    if (((MyMultiplayer.Static == null) || Sync.IsServer) || !this.IsStatic)
                    {
                        return this.RigidBody.AngularVelocity;
                    }
                    return base.AngularVelocity;
                }
                if (this.CharacterProxy != null)
                {
                    return this.CharacterProxy.AngularVelocity;
                }
                if ((this.Ragdoll == null) || !this.Ragdoll.IsActive)
                {
                    return base.AngularVelocity;
                }
                return this.Ragdoll.GetRootRigidBody().AngularVelocity;
            }
        }

        public override Vector3 LinearVelocityLocal
        {
            get
            {
                if (!this.Enabled)
                {
                    return Vector3.Zero;
                }
                if (this.RigidBody != null)
                {
                    if (((MyMultiplayer.Static == null) || Sync.IsServer) || !this.IsStatic)
                    {
                        return this.RigidBody.LinearVelocity;
                    }
                    return base.LinearVelocity;
                }
                if (this.CharacterProxy != null)
                {
                    return this.CharacterProxy.LinearVelocity;
                }
                if ((this.Ragdoll == null) || !this.Ragdoll.IsActive)
                {
                    return base.LinearVelocity;
                }
                return this.Ragdoll.GetRootRigidBody().LinearVelocity;
            }
        }

        public override Vector3 LinearVelocity
        {
            get
            {
                if (!this.Enabled)
                {
                    return Vector3.Zero;
                }
                if (this.RigidBody == null)
                {
                    if (this.CharacterProxy == null)
                    {
                        if ((this.Ragdoll == null) || !this.Ragdoll.IsActive)
                        {
                            return base.LinearVelocity;
                        }
                        return this.Ragdoll.GetRootRigidBody().LinearVelocity;
                    }
                    if ((MyMultiplayer.Static != null) && !Sync.IsServer)
                    {
                        MyCharacter entity = (MyCharacter) base.Entity;
                        VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(entity.ClosestParentId, false);
                        if ((entityById != null) && (entityById.Physics != null))
                        {
                            Vector3 vector2;
                            if (!entity.InheritRotation)
                            {
                                return (entityById.Physics.LinearVelocity + this.CharacterProxy.LinearVelocity);
                            }
                            Vector3D position = base.Entity.PositionComp.GetPosition();
                            entityById.Physics.GetVelocityAtPointLocal(ref position, out vector2);
                            return (vector2 + this.CharacterProxy.LinearVelocity);
                        }
                    }
                    return this.CharacterProxy.LinearVelocity;
                }
                if ((MyMultiplayer.Static != null) && !Sync.IsServer)
                {
                    MyCubeGrid grid = base.Entity as MyCubeGrid;
                    if (grid != null)
                    {
                        VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(grid.ClosestParentId, false);
                        if ((entityById != null) && (entityById.Physics != null))
                        {
                            return (entityById.Physics.LinearVelocity + this.RigidBody.LinearVelocity);
                        }
                        if (this.IsStatic)
                        {
                            return base.LinearVelocity;
                        }
                    }
                    else if (this.IsStatic)
                    {
                        MyCharacter entity = base.Entity as MyCharacter;
                        if (entity != null)
                        {
                            VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(entity.ClosestParentId, false);
                            if ((entityById != null) && (entityById.Physics != null))
                            {
                                Vector3 vector;
                                if (!entity.InheritRotation)
                                {
                                    return (entityById.Physics.LinearVelocity + base.LinearVelocity);
                                }
                                Vector3D position = base.Entity.PositionComp.GetPosition();
                                entityById.Physics.GetVelocityAtPointLocal(ref position, out vector);
                                return (vector + base.LinearVelocity);
                            }
                        }
                        return base.LinearVelocity;
                    }
                }
                return this.RigidBody.LinearVelocity;
            }
            set
            {
                if (this.RigidBody != null)
                {
                    this.RigidBody.LinearVelocity = value;
                }
                if (this.CharacterProxy != null)
                {
                    this.CharacterProxy.LinearVelocity = value;
                }
                if ((this.Ragdoll != null) && this.Ragdoll.IsActive)
                {
                    using (List<HkRigidBody>.Enumerator enumerator = this.Ragdoll.RigidBodies.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.LinearVelocity = value;
                        }
                    }
                }
                base.LinearVelocity = value;
            }
        }

        public override float LinearDamping
        {
            get => 
                this.RigidBody.LinearDamping;
            set
            {
                if (this.RigidBody != null)
                {
                    this.RigidBody.LinearDamping = value;
                }
                this.m_linearDamping = value;
            }
        }

        public override float AngularDamping
        {
            get => 
                this.RigidBody.AngularDamping;
            set
            {
                if (this.RigidBody != null)
                {
                    this.RigidBody.AngularDamping = value;
                }
                this.m_angularDamping = value;
            }
        }

        public override Vector3 AngularVelocity
        {
            get
            {
                if (this.RigidBody != null)
                {
                    if (((MyMultiplayer.Static == null) || Sync.IsServer) || !this.IsStatic)
                    {
                        return this.RigidBody.AngularVelocity;
                    }
                    return base.AngularVelocity;
                }
                if (this.CharacterProxy != null)
                {
                    return this.CharacterProxy.AngularVelocity;
                }
                if ((this.Ragdoll == null) || !this.Ragdoll.IsActive)
                {
                    return base.AngularVelocity;
                }
                return this.Ragdoll.GetRootRigidBody().AngularVelocity;
            }
            set
            {
                if (this.RigidBody != null)
                {
                    this.RigidBody.AngularVelocity = value;
                }
                if (this.CharacterProxy != null)
                {
                    this.CharacterProxy.AngularVelocity = value;
                }
                if ((this.Ragdoll != null) && this.Ragdoll.IsActive)
                {
                    using (List<HkRigidBody>.Enumerator enumerator = this.Ragdoll.RigidBodies.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.AngularVelocity = value;
                        }
                    }
                }
                base.AngularVelocity = value;
            }
        }

        public override Vector3 SupportNormal
        {
            get => 
                ((this.CharacterProxy == null) ? base.SupportNormal : this.CharacterProxy.SupportNormal);
            set => 
                (base.SupportNormal = value);
        }

        public override bool IsMoving =>
            (!Vector3.IsZero(this.LinearVelocity) || !Vector3.IsZero(this.AngularVelocity));

        public override Vector3 Gravity
        {
            get => 
                (this.Enabled ? ((this.RigidBody == null) ? ((this.CharacterProxy == null) ? Vector3.Zero : this.CharacterProxy.Gravity) : this.RigidBody.Gravity) : Vector3.Zero);
            set
            {
                HkRigidBody rigidBody = this.RigidBody;
                if (rigidBody != null)
                {
                    rigidBody.Gravity = value;
                }
                if (this.CharacterProxy != null)
                {
                    this.CharacterProxy.Gravity = value;
                }
            }
        }

        public override bool HasRigidBody =>
            (this.RigidBody != null);

        public override Vector3 CenterOfMassLocal =>
            this.RigidBody.CenterOfMassLocal;

        public override Vector3D CenterOfMassWorld =>
            (this.RigidBody.CenterOfMassWorld + this.Offset);

        public HashSetReader<HkConstraint> Constraints =>
            this.m_constraints;

        public virtual bool IsStaticForCluster
        {
            get => 
                this.m_isStaticForCluster;
            set => 
                (this.m_isStaticForCluster = value);
        }

        bool MyClusterTree.IMyActivationHandler.IsStaticForCluster =>
            this.IsStaticForCluster;

        public Vector3 LastLinearVelocity =>
            base.m_lastLinearVelocity;

        public Vector3 LastAngularVelocity =>
            base.m_lastAngularVelocity;

        public class MyWeldInfo
        {
            public MyPhysicsBody Parent;
            public Matrix Transform = Matrix.Identity;
            public readonly HashSet<MyPhysicsBody> Children = new HashSet<MyPhysicsBody>();
            public HkMassElement MassElement;

            internal void SetMassProps(HkMassProperties mp)
            {
                this.MassElement = new HkMassElement();
                this.MassElement.Properties = mp;
                this.MassElement.Tranform = this.Transform;
            }

            internal void UpdateMassProps(HkRigidBody rb)
            {
                HkMassProperties properties = new HkMassProperties {
                    InertiaTensor = rb.InertiaTensor,
                    Mass = rb.Mass,
                    CenterOfMass = rb.CenterOfMassLocal
                };
                this.MassElement = new HkMassElement();
                this.MassElement.Properties = properties;
                this.MassElement.Tranform = this.Transform;
            }
        }

        public delegate void PhysicsContactHandler(ref MyPhysics.MyContactPointEvent e);
    }
}

