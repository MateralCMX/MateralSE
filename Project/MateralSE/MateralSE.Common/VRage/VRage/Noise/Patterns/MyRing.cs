namespace VRage.Noise.Patterns
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Noise;

    public class MyRing : IMyModule
    {
        private double m_thickness;
        private double m_thicknessSqr;

        public MyRing(double radius, double thickness)
        {
            this.Radius = radius;
            this.Thickness = thickness;
        }

        private double clampToRing(double squareDstFromRing) => 
            ((squareDstFromRing >= this.m_thicknessSqr) ? 0.0 : (1.0 - (squareDstFromRing / this.m_thicknessSqr)));

        public double GetValue(double x)
        {
            double num = Math.Sqrt(x * x) - this.Radius;
            return this.clampToRing(num * num);
        }

        public double GetValue(double x, double y)
        {
            double num = Math.Sqrt((x * x) + (y * y)) - this.Radius;
            return this.clampToRing(num * num);
        }

        public double GetValue(double x, double y, double z)
        {
            if (Math.Abs(z) >= this.Thickness)
            {
                return 0.0;
            }
            double num = Math.Sqrt((x * x) + (y * y));
            if (Math.Abs((double) (num - this.Radius)) >= this.Thickness)
            {
                return 0.0;
            }
            double num2 = ((y / num) * this.Radius) - y;
            double num1 = ((x / num) * this.Radius) - x;
            double squareDstFromRing = ((num1 * num1) + (num2 * num2)) + (z * z);
            return this.clampToRing(squareDstFromRing);
        }

        public double Radius { get; set; }

        public double Thickness
        {
            get => 
                this.m_thickness;
            set
            {
                this.m_thickness = value;
                this.m_thicknessSqr = value * value;
            }
        }
    }
}

