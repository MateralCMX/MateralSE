namespace Sandbox.Game.Entities.Debris
{
    using Havok;
    using Sandbox;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyDebrisBase : MyEntity
    {
        private const float STONE_DENSITY = 2600f;

        public override void InitComponents()
        {
            if (base.GameLogic == null)
            {
                base.GameLogic = new MyDebrisBaseLogic();
            }
            base.InitComponents();
        }

        public MyDebrisBaseLogic Debris =>
            ((MyDebrisBaseLogic) base.GameLogic);

        public class MyDebrisBaseLogic : MyEntityGameLogic
        {
            private MyDebrisBase m_debris;
            private Action<MyDebrisBase> m_onCloseCallback;
            private bool m_isStarted;
            private int m_createdTime;
            public int LifespanInMiliseconds;
            protected HkMassProperties m_massProperties;

            protected virtual float CalculateMass(float r) => 
                (((3.141593f * (r * r)) * (1.333333f * r)) * 2600f);

            public override void Close()
            {
                base.Close();
            }

            public virtual void Free()
            {
                if (base.Container.Entity.Physics != null)
                {
                    base.Container.Entity.Physics.Close();
                    base.Container.Entity.Physics = null;
                }
            }

            protected virtual MyPhysicsComponentBase GetPhysics(RigidBodyFlag rigidBodyFlag) => 
                new MyDebrisBase.MyDebrisPhysics(base.Container.Entity, rigidBodyFlag);

            public virtual void Init(MyDebrisBaseDescription desc)
            {
                HkShape shape;
                base.Init(null, desc.Model, null, 1f, null);
                this.LifespanInMiliseconds = MyUtils.GetRandomInt(desc.LifespanMinInMiliseconds, desc.LifespanMaxInMiliseconds);
                MyDebrisBase.MyDebrisPhysics physics = (MyDebrisBase.MyDebrisPhysics) this.GetPhysics(RigidBodyFlag.RBF_DEBRIS);
                base.Container.Entity.Physics = physics;
                float mass = this.CalculateMass(((MyEntity) base.Entity).Render.GetModel().BoundingSphere.Radius);
                physics.CreatePhysicsShape(out shape, out this.m_massProperties, mass);
                physics.CreateFromCollisionObject(shape, Vector3.Zero, MatrixD.Identity, new HkMassProperties?(this.m_massProperties), 20);
                HkMassChangerUtil.Create(physics.RigidBody, -21, 1f, 0f);
                new HkEasePenetrationAction(physics.RigidBody, 3f) { InitialAllowedPenetrationDepthMultiplier = 10f }.RemoveReference();
                base.Container.Entity.Physics.Enabled = false;
                shape.RemoveReference();
                base.m_entity.Save = false;
                base.Container.Entity.Physics.PlayCollisionCueEnabled = true;
                base.NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
                base.Container.Entity.NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
                this.m_onCloseCallback = desc.OnCloseAction;
            }

            public override void MarkForClose()
            {
                if (this.m_onCloseCallback != null)
                {
                    this.m_onCloseCallback(this.m_debris);
                    this.m_onCloseCallback = null;
                }
                base.MarkForClose();
            }

            public override void OnAddedToContainer()
            {
                base.OnAddedToContainer();
                this.m_debris = base.Container.Entity as MyDebrisBase;
            }

            public virtual void Start(Vector3D position, Vector3D initialVelocity)
            {
                this.Start(MatrixD.CreateTranslation(position), initialVelocity, true);
            }

            public virtual void Start(MatrixD position, Vector3D initialVelocity, bool randomRotation = true)
            {
                this.m_createdTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                base.Container.Entity.WorldMatrix = position;
                base.Container.Entity.Physics.Clear();
                base.Container.Entity.Physics.LinearVelocity = (Vector3) initialVelocity;
                if (randomRotation)
                {
                    base.Container.Entity.Physics.AngularVelocity = new Vector3(MyUtils.GetRandomRadian(), MyUtils.GetRandomRadian(), MyUtils.GetRandomRadian());
                }
                MyEntities.Add(base.m_entity, true);
                base.Container.Entity.Physics.Enabled = true;
                Vector3 vector = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position.Translation);
                ((MyPhysicsBody) base.Container.Entity.Physics).RigidBody.Gravity = vector;
                this.m_isStarted = true;
            }

            public override void UpdateAfterSimulation()
            {
                base.UpdateAfterSimulation();
                if (this.m_isStarted)
                {
                    int num = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_createdTime;
                    if (num > this.LifespanInMiliseconds)
                    {
                        goto TR_0000;
                    }
                    else if (!MyDebris.Static.TooManyDebris)
                    {
                        float num2 = ((float) num) / ((float) this.LifespanInMiliseconds);
                        float num3 = 0.75f;
                        if (num2 > num3)
                        {
                            uint renderObjectID = base.Container.Entity.Render.GetRenderObjectID();
                            if (renderObjectID != uint.MaxValue)
                            {
                                Color? diffuseColor = null;
                                Vector3? colorMaskHsv = null;
                                MyRenderProxy.UpdateRenderEntity(renderObjectID, diffuseColor, colorMaskHsv, new float?((num2 - num3) / (1f - num3)), false);
                            }
                        }
                    }
                    else
                    {
                        goto TR_0000;
                    }
                }
                return;
            TR_0000:
                this.MarkForClose();
            }
        }

        public class MyDebrisPhysics : MyPhysicsBody
        {
            public MyDebrisPhysics(IMyEntity entity, RigidBodyFlag rigidBodyFlag) : base(entity, rigidBodyFlag)
            {
            }

            public virtual void CreatePhysicsShape(out HkShape shape, out HkMassProperties massProperties, float mass)
            {
                HkBoxShape shape2 = new HkBoxShape(((((MyEntity) base.Entity).Render.GetModel().BoundingBox.Max - ((MyEntity) base.Entity).Render.GetModel().BoundingBox.Min) / 2f) * base.Entity.PositionComp.Scale.Value);
                Vector3 translation = (((MyEntity) base.Entity).Render.GetModel().BoundingBox.Max + ((MyEntity) base.Entity).Render.GetModel().BoundingBox.Min) / 2f;
                shape = (HkShape) new HkTransformShape((HkShape) shape2, ref translation, ref Quaternion.Identity);
                massProperties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(shape2.HalfExtents, mass);
                massProperties.CenterOfMass = translation;
            }

            public virtual void ScalePhysicsShape(ref HkMassProperties massProperties)
            {
                HkShape shape;
                MyModel model = base.Entity.Render.GetModel();
                if ((model.HavokCollisionShapes != null) && (model.HavokCollisionShapes.Length != 0))
                {
                    Vector4 vector;
                    Vector4 vector2;
                    shape = model.HavokCollisionShapes[0];
                    shape.GetLocalAABB(0.1f, out vector, out vector2);
                    Vector3 halfExtents = new Vector3((vector2 - vector) * 0.5f);
                    massProperties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(halfExtents, halfExtents.Volume * 50f);
                    massProperties.CenterOfMass = new Vector3((vector + vector2) * 0.5f);
                }
                else
                {
                    HkTransformShape shape2 = (HkTransformShape) this.RigidBody.GetShape();
                    HkBoxShape childShape = (HkBoxShape) shape2.ChildShape;
                    childShape.HalfExtents = ((model.BoundingBox.Max - model.BoundingBox.Min) / 2f) * base.Entity.PositionComp.Scale.Value;
                    massProperties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(childShape.HalfExtents, childShape.HalfExtents.Volume * 0.5f);
                    massProperties.CenterOfMass = shape2.Transform.Translation;
                    shape = (HkShape) shape2;
                }
                this.RigidBody.SetShape(shape);
                this.RigidBody.SetMassProperties(ref massProperties);
                this.RigidBody.UpdateShape();
            }
        }
    }
}

