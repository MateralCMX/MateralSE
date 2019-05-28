namespace Sandbox.Engine.Physics
{
    using Havok;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public static class MyPhysicsHelper
    {
        public static void InitBoxPhysics(this IMyEntity entity, MyStringHash materialType, MyModel model, float mass, float angularDamping, ushort collisionLayer, RigidBodyFlag rbFlag)
        {
            Vector3 center = model.BoundingBox.Center;
            entity.InitBoxPhysics(materialType, center, model.BoundingBoxSize, mass, 0f, angularDamping, collisionLayer, rbFlag);
        }

        public static void InitBoxPhysics(this IMyEntity entity, MyStringHash materialType, Vector3 center, Vector3 size, float mass, float linearDamping, float angularDamping, ushort collisionLayer, RigidBodyFlag rbFlag)
        {
            float single1 = mass;
            mass = ((rbFlag & RigidBodyFlag.RBF_STATIC) != RigidBodyFlag.RBF_DEFAULT) ? 0f : single1;
            HkMassProperties properties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(size / 2f, mass);
            MyPhysicsBody body1 = new MyPhysicsBody(entity, rbFlag);
            body1.MaterialType = materialType;
            body1.AngularDamping = angularDamping;
            body1.LinearDamping = linearDamping;
            MyPhysicsBody body = body1;
            HkBoxShape shape = new HkBoxShape(size * 0.5f);
            body.CreateFromCollisionObject((HkShape) shape, center, entity.PositionComp.WorldMatrix, new HkMassProperties?(properties), 15);
            shape.Base.RemoveReference();
            entity.Physics = body;
        }

        internal static void InitBoxPhysics(this IMyEntity entity, Matrix worldMatrix, MyStringHash materialType, Vector3 center, Vector3 size, float mass, float linearDamping, float angularDamping, ushort collisionLayer, RigidBodyFlag rbFlag)
        {
            float single1 = mass;
            mass = ((rbFlag & RigidBodyFlag.RBF_STATIC) != RigidBodyFlag.RBF_DEFAULT) ? 0f : single1;
            HkMassProperties properties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(size / 2f, mass);
            MyPhysicsBody body1 = new MyPhysicsBody(null, rbFlag);
            body1.MaterialType = materialType;
            body1.AngularDamping = angularDamping;
            body1.LinearDamping = linearDamping;
            MyPhysicsBody body = body1;
            HkBoxShape shape = new HkBoxShape(size * 0.5f);
            body.CreateFromCollisionObject((HkShape) shape, center, worldMatrix, new HkMassProperties?(properties), 15);
            shape.Base.RemoveReference();
            entity.Physics = body;
        }

        public static void InitCapsulePhysics(this IMyEntity entity, MyStringHash materialType, Vector3 vertexA, Vector3 vertexB, float radius, float mass, float linearDamping, float angularDamping, ushort collisionLayer, RigidBodyFlag rbFlag)
        {
            float single1 = mass;
            mass = ((rbFlag & RigidBodyFlag.RBF_STATIC) != RigidBodyFlag.RBF_DEFAULT) ? 0f : single1;
            MyPhysicsBody body1 = new MyPhysicsBody(entity, rbFlag);
            body1.MaterialType = materialType;
            body1.AngularDamping = angularDamping;
            body1.LinearDamping = linearDamping;
            MyPhysicsBody body = body1;
            HkMassProperties properties = HkInertiaTensorComputer.ComputeSphereVolumeMassProperties(radius, mass);
            body.ReportAllContacts = true;
            HkCapsuleShape shape = new HkCapsuleShape(vertexA, vertexB, radius);
            body.CreateFromCollisionObject((HkShape) shape, (vertexA + vertexB) / 2f, entity.PositionComp.WorldMatrix, new HkMassProperties?(properties), 15);
            shape.Base.RemoveReference();
            entity.Physics = body;
        }

        public static void InitCharacterPhysics(this IMyEntity entity, MyStringHash materialType, Vector3 center, float characterWidth, float characterHeight, float crouchHeight, float ladderHeight, float headSize, float headHeight, float linearDamping, float angularDamping, ushort collisionLayer, RigidBodyFlag rbFlag, float mass, bool isOnlyVertical, float maxSlope, float maxImpulse, float maxSpeedRelativeToShip, bool networkProxy, float? maxForce)
        {
            MyPhysicsBody body1 = new MyPhysicsBody(entity, rbFlag);
            body1.MaterialType = materialType;
            body1.AngularDamping = angularDamping;
            body1.LinearDamping = linearDamping;
            MyPhysicsBody body = body1;
            body.CreateCharacterCollision(center, characterWidth, characterHeight, crouchHeight, ladderHeight, headSize, headHeight, entity.PositionComp.WorldMatrix, mass, collisionLayer, isOnlyVertical, maxSlope, maxImpulse, maxSpeedRelativeToShip, networkProxy, maxForce);
            entity.Physics = body;
        }

        public static bool InitModelPhysics(this IMyEntity entity, RigidBodyFlag rbFlags = 2, int collisionLayers = 0x11)
        {
            MyEntity entity2 = entity as MyEntity;
            if (entity2.Closed)
            {
                return false;
            }
            if ((entity2.ModelCollision.HavokCollisionShapes == null) || (entity2.ModelCollision.HavokCollisionShapes.Length == 0))
            {
                return false;
            }
            List<HkShape> list = entity2.ModelCollision.HavokCollisionShapes.ToList<HkShape>();
            HkListShape shape = new HkListShape(list.GetInternalArray<HkShape>(), list.Count, HkReferencePolicy.None);
            entity2.Physics = new MyPhysicsBody(entity2, rbFlags);
            entity2.Physics.IsPhantom = false;
            HkMassProperties? massProperties = null;
            (entity2.Physics as MyPhysicsBody).CreateFromCollisionObject((HkShape) shape, (Vector3) Vector3D.Zero, entity2.WorldMatrix, massProperties, collisionLayers);
            entity2.Physics.Enabled = true;
            shape.Base.RemoveReference();
            return true;
        }

        public static void InitSpherePhysics(this IMyEntity entity, MyStringHash materialType, MyModel model, float mass, float linearDamping, float angularDamping, ushort collisionLayer, RigidBodyFlag rbFlag)
        {
            entity.InitSpherePhysics(materialType, model.BoundingSphere.Center, model.BoundingSphere.Radius, mass, linearDamping, angularDamping, collisionLayer, rbFlag);
        }

        public static void InitSpherePhysics(this IMyEntity entity, MyStringHash materialType, Vector3 sphereCenter, float sphereRadius, float mass, float linearDamping, float angularDamping, ushort collisionLayer, RigidBodyFlag rbFlag)
        {
            float single1 = mass;
            mass = ((rbFlag & RigidBodyFlag.RBF_STATIC) != RigidBodyFlag.RBF_DEFAULT) ? 0f : single1;
            MyPhysicsBody body1 = new MyPhysicsBody(entity, rbFlag);
            body1.MaterialType = materialType;
            body1.AngularDamping = angularDamping;
            body1.LinearDamping = linearDamping;
            MyPhysicsBody body = body1;
            HkMassProperties properties = HkInertiaTensorComputer.ComputeSphereVolumeMassProperties(sphereRadius, mass);
            HkSphereShape shape = new HkSphereShape(sphereRadius);
            body.CreateFromCollisionObject((HkShape) shape, sphereCenter, entity.PositionComp.WorldMatrix, new HkMassProperties?(properties), 15);
            shape.Base.RemoveReference();
            entity.Physics = body;
        }
    }
}

