namespace VRage.Noise.Patterns
{
    using System;
    using VRage.Noise;

    public class MySphere : IMyModule
    {
        private double m_outerRadiusBlendingSqrDist;
        private double m_innerRadius;
        private double m_innerRadiusSqr;
        private double m_outerRadius;
        private double m_outerRadiusSqr;

        public MySphere(double innerRadius, double outerRadius)
        {
            this.InnerRadius = innerRadius;
            this.OuterRadius = outerRadius;
        }

        private double ClampDistanceToRadius(double distanceSqr) => 
            ((distanceSqr >= this.m_outerRadiusSqr) ? 0.0 : ((distanceSqr >= this.m_innerRadiusSqr) ? (1.0 - ((distanceSqr - this.m_innerRadiusSqr) / this.m_outerRadiusBlendingSqrDist)) : 1.0));

        public double GetValue(double x)
        {
            double distanceSqr = x * x;
            return this.ClampDistanceToRadius(distanceSqr);
        }

        public double GetValue(double x, double y)
        {
            double distanceSqr = (x * x) + (y * y);
            return this.ClampDistanceToRadius(distanceSqr);
        }

        public double GetValue(double x, double y, double z)
        {
            double distanceSqr = ((x * x) + (y * y)) + (z * z);
            return this.ClampDistanceToRadius(distanceSqr);
        }

        private void UpdateBlendingDistnace()
        {
            this.m_outerRadiusBlendingSqrDist = this.m_outerRadiusSqr - this.m_innerRadiusSqr;
        }

        public double InnerRadius
        {
            get => 
                this.m_innerRadius;
            set
            {
                this.m_innerRadius = value;
                this.m_innerRadiusSqr = value * value;
                this.UpdateBlendingDistnace();
            }
        }

        public double OuterRadius
        {
            get => 
                this.m_outerRadius;
            set
            {
                this.m_outerRadius = value;
                this.m_outerRadiusSqr = value * value;
                this.UpdateBlendingDistnace();
            }
        }
    }
}

