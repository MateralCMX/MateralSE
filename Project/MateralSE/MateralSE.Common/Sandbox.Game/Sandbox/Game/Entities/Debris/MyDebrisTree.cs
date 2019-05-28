namespace Sandbox.Game.Entities.Debris
{
    using Havok;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRageMath;

    internal class MyDebrisTree : MyDebrisBase
    {
        public MyDebrisTree()
        {
            base.GameLogic = new MyDebrisTreeLogic();
            if (MyDebugDrawSettings.DEBUG_DRAW_TREE_COLLISION_SHAPES)
            {
                base.NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_TREE_COLLISION_SHAPES)
            {
                MyPhysicsComponentBase physics = base.Physics;
                if (physics != null)
                {
                    HkRigidBody rigidBody = physics.RigidBody;
                    if (rigidBody != null)
                    {
                        int shapeIndex = 0;
                        MyPhysicsDebugDraw.DrawCollisionShape(rigidBody.GetShape(), physics.GetWorldMatrix(), 1f, ref shapeIndex, physics.IsActive ? "A" : "I", false);
                    }
                }
            }
        }

        protected class MyDebrisTreeLogic : MyDebrisBase.MyDebrisBaseLogic
        {
            private const float AVERAGE_WOOD_DENSITY = 800f;

            protected override float CalculateMass(float radius)
            {
                float num;
                float num2;
                MyDebrisTree.MyDebrisTreePhysics.ComputeShapeDimensions(base.Entity.Model.BoundingBox, out num, out num2);
                return (((3.141593f * (num2 * num2)) * ((1.333333f * num2) + num)) * 800f);
            }

            protected override MyPhysicsComponentBase GetPhysics(RigidBodyFlag rigidBodyFlag) => 
                new MyDebrisTree.MyDebrisTreePhysics(base.Entity, rigidBodyFlag);

            public override void Init(MyDebrisBaseDescription desc)
            {
                base.Init(desc);
                HkRigidBody rigidBody = base.Entity.Physics.RigidBody;
                rigidBody.EnableDeactivation = false;
                rigidBody.MaxAngularVelocity = 2f;
            }
        }

        protected class MyDebrisTreePhysics : MyDebrisBase.MyDebrisPhysics
        {
            public MyDebrisTreePhysics(IMyEntity entity, RigidBodyFlag rigidBodyFlags) : base(entity, rigidBodyFlags)
            {
            }

            public static void ComputeShapeDimensions(BoundingBox bBox, out float height, out float radius)
            {
                height = bBox.Height;
                radius = height / 20f;
                height -= 2f * radius;
            }

            public override void CreatePhysicsShape(out HkShape shape, out HkMassProperties massProperties, float mass)
            {
                float num;
                float num2;
                shape = HkShape.Empty;
                MyModel model = base.Entity.Render.GetModel();
                BoundingBox boundingBox = model.BoundingBox;
                ComputeShapeDimensions(boundingBox, out num, out num2);
                Vector3 vector = Vector3.Up * num;
                Vector3 vertexA = ((boundingBox.Min + boundingBox.Max) * 0.5f) + (vector * 0.2f);
                Vector3 vertexB = ((boundingBox.Min + boundingBox.Max) * 0.5f) - (vector * 0.45f);
                bool flag = true;
                if (MyFakes.TREE_MESH_FROM_MODEL)
                {
                    HkShape[] havokCollisionShapes = model.HavokCollisionShapes;
                    if ((havokCollisionShapes != null) && (havokCollisionShapes.Length != 0))
                    {
                        if (havokCollisionShapes.Length != 1)
                        {
                            shape = (HkShape) new HkListShape(havokCollisionShapes, HkReferencePolicy.None);
                        }
                        else
                        {
                            shape = havokCollisionShapes[0];
                            shape.AddReference();
                        }
                        flag = false;
                    }
                }
                if (flag)
                {
                    shape = (HkShape) new HkCapsuleShape(vertexA, vertexB, num2);
                }
                massProperties = HkInertiaTensorComputer.ComputeCapsuleVolumeMassProperties(vertexA, vertexB, num2, mass);
            }
        }
    }
}

