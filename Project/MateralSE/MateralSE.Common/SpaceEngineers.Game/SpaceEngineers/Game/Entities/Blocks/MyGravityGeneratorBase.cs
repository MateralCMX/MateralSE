namespace SpaceEngineers.Game.Entities.Blocks
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRageMath;
    using VRageRender;

    [MyTerminalInterface(new System.Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyGravityGeneratorBase), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGeneratorBase) })]
    public abstract class MyGravityGeneratorBase : MyFunctionalBlock, IMyGizmoDrawableObject, SpaceEngineers.Game.ModAPI.IMyGravityGeneratorBase, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGeneratorBase, IMyGravityProvider
    {
        protected Color m_gizmoColor = new Vector4(0f, 1f, 0f, 0.196f);
        protected const float m_maxGizmoDrawDistance = 1000f;
        protected bool m_oldEmissiveState;
        protected readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_gravityAcceleration;
        protected MyConcurrentHashSet<VRage.ModAPI.IMyEntity> m_containedEntities = new MyConcurrentHashSet<VRage.ModAPI.IMyEntity>();

        protected MyGravityGeneratorBase()
        {
            this.m_gravityAcceleration.ValueChanged += x => this.AccelerationChanged();
            base.NeedsWorldMatrix = true;
        }

        private void AccelerationChanged()
        {
            base.ResourceSink.Update();
        }

        protected abstract float CalculateRequiredPowerInput();
        public bool CanBeDrawn() => 
            (MyCubeGrid.ShowGravityGizmos && (base.ShowOnHUD && (base.IsWorking && (base.HasLocalPlayerAccess() && (base.GetDistanceBetweenCameraAndBoundingSphere() <= 1000.0)))));

        protected override bool CheckIsWorking() => 
            (((base.ResourceSink != null) ? base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) : true) && base.CheckIsWorking());

        protected override void Closing()
        {
            base.Closing();
            if (base.CubeGrid.CreatePhysics && (base.ResourceSink != null))
            {
                base.ResourceSink.IsPoweredChanged -= new Action(this.Receiver_IsPoweredChanged);
                base.ResourceSink.RequiredInputChanged -= new MyRequiredResourceChangeDelegate(this.Receiver_RequiredInputChanged);
            }
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        private HkBvShape CreateFieldShape()
        {
            HkPhantomCallbackShape shape = new HkPhantomCallbackShape(new HkPhantomHandler(this.phantom_Enter), new HkPhantomHandler(this.phantom_Leave));
            return new HkBvShape(this.GetHkShape(), (HkShape) shape, HkReferencePolicy.TakeOwnership);
        }

        public bool EnableLongDrawDistance() => 
            false;

        public virtual BoundingBox? GetBoundingBox() => 
            null;

        public Color GetGizmoColor() => 
            this.m_gizmoColor;

        public float GetGravityMultiplier(Vector3D worldPoint) => 
            (this.IsPositionInRange(worldPoint) ? 1f : 0f);

        protected abstract HkShape GetHkShape();
        public Vector3 GetPositionInGrid() => 
            ((Vector3) base.Position);

        public abstract void GetProxyAABB(out BoundingBoxD aabb);
        public virtual float GetRadius() => 
            -1f;

        public abstract Vector3 GetWorldGravity(Vector3D worldPoint);
        public MatrixD GetWorldMatrix() => 
            base.WorldMatrix;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            this.InitializeSinkComponent();
            base.Init(objectBuilder, cubeGrid);
            if (base.CubeGrid.CreatePhysics)
            {
                if (MyFakes.ENABLE_GRAVITY_PHANTOM)
                {
                    HkBvShape shape = this.CreateFieldShape();
                    base.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_KINEMATIC);
                    base.Physics.IsPhantom = true;
                    HkMassProperties? massProperties = null;
                    base.Physics.CreateFromCollisionObject((HkShape) shape, base.PositionComp.LocalVolume.Center, base.WorldMatrix, massProperties, 0x15);
                    shape.Base.RemoveReference();
                    base.Physics.Enabled = base.IsWorking;
                }
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
                base.ResourceSink.Update();
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            base.m_soundEmitter = new MyEntity3DSoundEmitter(this, true, 1f);
            base.m_baseIdleSound.Init("BlockGravityGen", true);
        }

        protected abstract void InitializeSinkComponent();
        public abstract bool IsPositionInRange(Vector3D worldPoint);
        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            MyGravityProviderSystem.AddGravityGenerator(this);
            if (base.ResourceSink != null)
            {
                base.ResourceSink.Update();
            }
        }

        public override void OnBuildSuccess(long builtBy, bool instantBuild)
        {
            base.ResourceSink.Update();
            base.OnBuildSuccess(builtBy, instantBuild);
        }

        protected override void OnEnabledChanged()
        {
            base.ResourceSink.Update();
            base.OnEnabledChanged();
        }

        public override void OnRemovedFromScene(object source)
        {
            MyGravityProviderSystem.RemoveGravityGenerator(this);
            base.OnRemovedFromScene(source);
        }

        private void phantom_Enter(HkPhantomCallbackShape sender, HkRigidBody body)
        {
            VRage.ModAPI.IMyEntity entity = body.GetEntity(0);
            if (((entity != null) && !(entity is MyCubeGrid)) && this.m_containedEntities.Add(entity))
            {
                MySandboxGame.Static.Invoke(delegate {
                    this.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                    MyPhysicsComponentBase physics = entity.Physics;
                    if ((physics != null) && physics.HasRigidBody)
                    {
                        ((MyPhysicsBody) physics).RigidBody.Activate();
                    }
                }, "MyGravityGeneratorBase/Activate physics");
            }
        }

        private void phantom_Leave(HkPhantomCallbackShape sender, HkRigidBody body)
        {
            VRage.ModAPI.IMyEntity entity = body.GetEntity(0);
            if (entity != null)
            {
                this.m_containedEntities.Remove(entity);
            }
        }

        protected void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
            if (base.Physics != null)
            {
                base.Physics.Enabled = base.IsWorking;
            }
            this.UpdateText();
        }

        protected void Receiver_RequiredInputChanged(MyDefinitionId resourceTypeId, MyResourceSinkComponent receiver, float oldRequirement, float newRequirement)
        {
            this.UpdateText();
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (base.IsWorking)
            {
                foreach (VRage.Game.Entity.MyEntity entity in this.m_containedEntities)
                {
                    Vector3? nullable;
                    float? nullable2;
                    MyCharacter character = entity as MyCharacter;
                    SpaceEngineers.Game.ModAPI.IMyVirtualMass mass = entity as SpaceEngineers.Game.ModAPI.IMyVirtualMass;
                    float naturalGravityMultiplier = MyGravityProviderSystem.CalculateHighestNaturalGravityMultiplierInPoint(entity.WorldMatrix.Translation);
                    MatrixD worldMatrix = entity.WorldMatrix;
                    Vector3 vector = this.GetWorldGravity(worldMatrix.Translation) * MyGravityProviderSystem.CalculateArtificialGravityStrengthMultiplier(naturalGravityMultiplier);
                    if ((mass != null) && entity.Physics.RigidBody.IsActive)
                    {
                        if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_MISCELLANEOUS)
                        {
                            MyRenderProxy.DebugDrawSphere(entity.WorldMatrix.Translation, 0.2f, mass.IsWorking ? Color.Blue : Color.Red, 1f, false, false, true, false);
                        }
                        if ((mass.IsWorking && !mass.CubeGrid.IsStatic) && !mass.CubeGrid.Physics.IsStatic)
                        {
                            nullable = null;
                            nullable2 = null;
                            mass.CubeGrid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, new Vector3?(vector * mass.VirtualMass), new Vector3D?(entity.WorldMatrix.Translation), nullable, nullable2, false, false);
                        }
                        continue;
                    }
                    if ((!entity.Physics.IsKinematic && (!entity.Physics.IsStatic && ((entity.Physics.RigidBody2 == null) && (character == null)))) && (entity.Physics.RigidBody != null))
                    {
                        Vector3D? nullable3;
                        if (!(entity is MyFloatingObject))
                        {
                            nullable3 = null;
                            nullable = null;
                            nullable2 = null;
                            entity.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, new Vector3?(vector * entity.Physics.RigidBody.Mass), nullable3, nullable, nullable2, true, false);
                        }
                        else
                        {
                            float num2 = (entity as MyFloatingObject).HasConstraints() ? 2f : 1f;
                            nullable3 = null;
                            nullable = null;
                            nullable2 = null;
                            entity.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, new Vector3?((Vector3) ((num2 * vector) * entity.Physics.RigidBody.Mass)), nullable3, nullable, nullable2, true, false);
                        }
                    }
                }
            }
            if (this.m_containedEntities.Count == 0)
            {
                this.NeedsUpdate = base.HasDamageEffect ? MyEntityUpdateEnum.EACH_FRAME : MyEntityUpdateEnum.EACH_100TH_FRAME;
            }
        }

        protected void UpdateFieldShape()
        {
            if (MyFakes.ENABLE_GRAVITY_PHANTOM && (base.Physics != null))
            {
                HkBvShape shape = this.CreateFieldShape();
                base.Physics.RigidBody.SetShape((HkShape) shape);
                shape.Base.RemoveReference();
                this.UpdateGeneratorProxy();
            }
            base.ResourceSink.Update();
        }

        private void UpdateGeneratorProxy()
        {
            MyGridPhysics physics = base.CubeGrid.Physics;
            if ((physics != null) && base.InScene)
            {
                Vector3 linearVelocity = physics.LinearVelocity;
                MyGravityProviderSystem.OnGravityGeneratorMoved(this, ref linearVelocity);
            }
        }

        protected abstract void UpdateText();
        protected override void WorldPositionChanged(object source)
        {
            base.WorldPositionChanged(source);
            this.UpdateGeneratorProxy();
        }

        private MyGravityGeneratorBaseDefinition BlockDefinition =>
            ((MyGravityGeneratorBaseDefinition) base.BlockDefinition);

        public float GravityAcceleration
        {
            get => 
                ((float) this.m_gravityAcceleration);
            set
            {
                if (this.m_gravityAcceleration != value)
                {
                    this.m_gravityAcceleration.Value = value;
                }
            }
        }

        float SpaceEngineers.Game.ModAPI.IMyGravityGeneratorBase.GravityAcceleration
        {
            get => 
                this.GravityAcceleration;
            set => 
                (this.GravityAcceleration = MathHelper.Clamp(value, this.BlockDefinition.MinGravityAcceleration, this.BlockDefinition.MaxGravityAcceleration));
        }

        float SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGeneratorBase.Gravity =>
            (this.GravityAcceleration / 9.81f);

        float SpaceEngineers.Game.ModAPI.Ingame.IMyGravityGeneratorBase.GravityAcceleration
        {
            get => 
                this.GravityAcceleration;
            set => 
                (this.GravityAcceleration = MathHelper.Clamp(value, this.BlockDefinition.MinGravityAcceleration, this.BlockDefinition.MaxGravityAcceleration));
        }
    }
}

