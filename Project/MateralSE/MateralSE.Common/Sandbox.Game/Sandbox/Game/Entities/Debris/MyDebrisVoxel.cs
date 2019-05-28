namespace Sandbox.Game.Entities.Debris
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Game.Components;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    internal class MyDebrisVoxel : MyDebrisBase
    {
        public override void InitComponents()
        {
            base.GameLogic = new MyDebrisVoxelLogic();
            base.Render = new MyRenderComponentDebrisVoxel();
            base.InitComponents();
        }

        internal class MyDebrisVoxelLogic : MyDebrisBase.MyDebrisBaseLogic
        {
            protected override MyPhysicsComponentBase GetPhysics(RigidBodyFlag rigidBodyFlag) => 
                new MyDebrisVoxel.MyDebrisVoxelPhysics(base.Container.Entity, rigidBodyFlag);

            public override void Start(Vector3D position, Vector3D initialVelocity)
            {
                this.Start(position, initialVelocity, MyDefinitionManager.Static.GetDefaultVoxelMaterialDefinition());
            }

            public void Start(Vector3D position, Vector3D initialVelocity, MyVoxelMaterialDefinition mat)
            {
                MyRenderComponentDebrisVoxel render = base.Container.Entity.Render as MyRenderComponentDebrisVoxel;
                render.TexCoordOffset = MyUtils.GetRandomFloat(5f, 15f);
                render.TexCoordScale = MyUtils.GetRandomFloat(8f, 12f);
                render.VoxelMaterialIndex = mat.Index;
                base.Start(position, initialVelocity);
                base.Container.Entity.Render.NeedsResolveCastShadow = true;
                base.Container.Entity.Render.FastCastShadowResolve = true;
            }
        }

        internal class MyDebrisVoxelPhysics : MyDebrisBase.MyDebrisPhysics
        {
            private const float VoxelDensity = 260f;

            public MyDebrisVoxelPhysics(IMyEntity entity, RigidBodyFlag rigidBodyFlag) : base(entity, rigidBodyFlag)
            {
            }

            public override void CreatePhysicsShape(out HkShape shape, out HkMassProperties massProperties, float mass)
            {
                HkSphereShape shape2 = new HkSphereShape((0.5f * ((MyEntity) base.Entity).Render.GetModel().BoundingSphere.Radius) * base.Entity.PositionComp.Scale.Value);
                shape = (HkShape) shape2;
                massProperties = HkInertiaTensorComputer.ComputeSphereVolumeMassProperties(shape2.Radius * 0.5f, mass);
            }

            public override void ScalePhysicsShape(ref HkMassProperties massProperties)
            {
                HkSphereShape shape = (HkSphereShape) this.RigidBody.GetShape();
                shape.Radius = ((MyEntity) base.Entity).Render.GetModel().BoundingSphere.Radius * base.Entity.PositionComp.Scale.Value;
                float mass = this.SphereMass(shape.Radius, 260f);
                massProperties = HkInertiaTensorComputer.ComputeSphereVolumeMassProperties(shape.Radius, mass);
                this.RigidBody.SetShape((HkShape) shape);
                this.RigidBody.SetMassProperties(ref massProperties);
                this.RigidBody.UpdateShape();
            }

            private float SphereMass(float radius, float density) => 
                ((((((radius * radius) * radius) * 3.141593f) * 4f) * 0.333f) * density);
        }
    }
}

