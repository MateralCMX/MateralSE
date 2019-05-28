namespace Sandbox.Game.World.Generator
{
    using System;
    using VRage.Noise;
    using VRageMath;

    internal class MySphereDensityFunction : IMyAsteroidFieldDensityFunction, IMyModule
    {
        private Vector3D m_center;
        private BoundingSphereD m_sphereMax;
        private double m_innerRadius;
        private double m_outerRadius;
        private double m_middleRadius;
        private double m_halfFalloff;

        public MySphereDensityFunction(Vector3D center, double radius, double additionalFalloff)
        {
            this.m_center = center;
            this.m_sphereMax = new BoundingSphereD(center, radius + additionalFalloff);
            this.m_innerRadius = radius;
            this.m_halfFalloff = additionalFalloff / 2.0;
            this.m_middleRadius = radius + this.m_halfFalloff;
            this.m_outerRadius = radius + additionalFalloff;
        }

        public bool ExistsInCell(ref BoundingBoxD bbox) => 
            (this.m_sphereMax.Contains(bbox) != ContainmentType.Disjoint);

        public double GetValue(double x)
        {
            throw new NotImplementedException();
        }

        public double GetValue(double x, double y)
        {
            throw new NotImplementedException();
        }

        public double GetValue(double x, double y, double z)
        {
            double num = Vector3D.Distance(this.m_center, new Vector3D(x, y, z));
            return ((num <= this.m_outerRadius) ? ((num >= this.m_innerRadius) ? ((num <= this.m_middleRadius) ? ((num - this.m_middleRadius) / this.m_halfFalloff) : ((this.m_middleRadius - num) / -this.m_halfFalloff)) : -1.0) : 1.0);
        }
    }
}

