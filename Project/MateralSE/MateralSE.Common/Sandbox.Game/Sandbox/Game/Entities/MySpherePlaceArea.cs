namespace Sandbox.Game.Entities
{
    using System;
    using VRage.Utils;
    using VRageMath;

    public class MySpherePlaceArea : MyPlaceArea
    {
        private float m_radiusSq;
        private float m_radius;

        public MySpherePlaceArea(float radius, MyStringHash areaType) : base(areaType)
        {
            this.m_radius = radius;
            this.m_radiusSq = radius * radius;
        }

        public override double DistanceSqToPoint(Vector3D point)
        {
            double num = (this.GetPosition() - point).Length() - this.m_radius;
            return ((num < 0.0) ? 0.0 : (num * num));
        }

        public Vector3D GetPosition() => 
            ((base.Container.Entity.PositionComp != null) ? base.Container.Entity.PositionComp.GetPosition() : Vector3D.Zero);

        public override bool TestPoint(Vector3D point) => 
            (Vector3D.DistanceSquared(point, this.GetPosition()) <= this.m_radiusSq);

        public float Radius =>
            this.m_radius;

        public override BoundingBoxD WorldAABB
        {
            get
            {
                Vector3D vectord = new Vector3D((double) this.m_radius);
                return new BoundingBoxD(this.GetPosition() - vectord, this.GetPosition() + vectord);
            }
        }
    }
}

