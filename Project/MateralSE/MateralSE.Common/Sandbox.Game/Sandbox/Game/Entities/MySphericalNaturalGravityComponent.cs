namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MySphericalNaturalGravityComponent : MyGravityProviderComponent
    {
        private const double GRAVITY_LIMIT_STRENGTH = 0.05;
        private readonly double m_minRadius;
        private readonly double m_maxRadius;
        private readonly double m_falloff;
        private readonly double m_intensity;
        private float m_gravityLimit;
        private float m_gravityLimitSq;

        public MySphericalNaturalGravityComponent(double minRadius, double maxRadius, double falloff, double intensity)
        {
            this.m_minRadius = minRadius;
            this.m_maxRadius = maxRadius;
            this.m_falloff = falloff;
            this.m_intensity = intensity;
            double num2 = maxRadius;
            double y = 1.0 / falloff;
            this.GravityLimit = (float) (num2 * Math.Pow(intensity / 0.05, y));
        }

        public override float GetGravityMultiplier(Vector3D worldPoint)
        {
            double num = (this.Position - worldPoint).Length();
            if (num > this.m_gravityLimit)
            {
                return 0f;
            }
            float num2 = 1f;
            if (num > this.m_maxRadius)
            {
                num2 = (float) Math.Pow(num / this.m_maxRadius, -this.m_falloff);
            }
            else if (num < this.m_minRadius)
            {
                num2 = (float) (num / this.m_minRadius);
                if (num2 < 0.01f)
                {
                    num2 = 0.01f;
                }
            }
            return (float) (num2 * this.m_intensity);
        }

        public override void GetProxyAABB(out BoundingBoxD aabb)
        {
            BoundingSphereD sphere = new BoundingSphereD(this.Position, (double) this.GravityLimit);
            BoundingBoxD.CreateFromSphere(ref sphere, out aabb);
        }

        public override Vector3 GetWorldGravity(Vector3D worldPoint)
        {
            float gravityMultiplier = this.GetGravityMultiplier(worldPoint);
            return ((this.GetWorldGravityNormalized(ref worldPoint) * 9.81f) * gravityMultiplier);
        }

        public Vector3 GetWorldGravityNormalized(ref Vector3D worldPoint)
        {
            Vector3 vector = (Vector3) (this.Position - worldPoint);
            vector.Normalize();
            return vector;
        }

        public override bool IsPositionInRange(Vector3D worldPoint) => 
            ((this.Position - worldPoint).LengthSquared() <= this.m_gravityLimitSq);

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.Position = base.Entity.PositionComp.GetPosition();
        }

        public Vector3D Position { get; private set; }

        public override bool IsWorking =>
            true;

        public float GravityLimit
        {
            get => 
                this.m_gravityLimit;
            private set
            {
                this.m_gravityLimitSq = value * value;
                this.m_gravityLimit = value;
            }
        }

        public float GravityLimitSq
        {
            get => 
                this.m_gravityLimitSq;
            private set
            {
                this.m_gravityLimitSq = value;
                this.m_gravityLimit = (float) Math.Sqrt((double) value);
            }
        }

        public override string ComponentTypeDebugString =>
            base.GetType().Name;
    }
}

