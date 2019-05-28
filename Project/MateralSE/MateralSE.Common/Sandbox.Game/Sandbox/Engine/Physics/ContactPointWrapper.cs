namespace Sandbox.Engine.Physics
{
    using Havok;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.ModAPI;
    using VRageMath;

    public class ContactPointWrapper
    {
        public MyPhysicsBody bodyA;
        public MyPhysicsBody bodyB;
        public Vector3 position;
        public Vector3 normal;
        public IMyEntity entityA;
        public IMyEntity entityB;
        public float separatingVelocity;

        public ContactPointWrapper(ref HkContactPointEvent e)
        {
            this.bodyA = e.Base.BodyA.GetBody();
            this.bodyB = e.Base.BodyB.GetBody();
            this.position = e.ContactPoint.Position;
            this.normal = e.ContactPoint.Normal;
            MyPhysicsBody physicsBody = e.GetPhysicsBody(0);
            MyPhysicsBody body2 = e.GetPhysicsBody(1);
            this.entityA = physicsBody.Entity;
            this.entityB = body2.Entity;
            this.separatingVelocity = e.SeparatingVelocity;
        }

        public bool IsValid()
        {
            if ((this.bodyA == null) || (this.bodyB == null))
            {
                return false;
            }
            Vector3 position = this.position;
            Vector3 normal = this.normal;
            return ((this.entityA != null) && (this.entityB != null));
        }

        public Vector3D WorldPosition { get; set; }
    }
}

