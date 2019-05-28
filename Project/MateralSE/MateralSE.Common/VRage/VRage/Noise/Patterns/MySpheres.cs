namespace VRage.Noise.Patterns
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Noise;
    using VRageMath;

    internal class MySpheres : IMyModule
    {
        public MySpheres(double frequnecy = 1.0)
        {
            this.Frequency = frequnecy;
        }

        public double GetValue(double x)
        {
            x *= this.Frequency;
            int num = MathHelper.Floor(x);
            double num2 = Math.Sqrt((x * x) + (x * x)) - num;
            return (1.0 - (Math.Min(num2, 1.0 - num2) * 4.0));
        }

        public double GetValue(double x, double y)
        {
            x *= this.Frequency;
            y *= this.Frequency;
            int num = MathHelper.Floor(x);
            double num2 = Math.Sqrt((x * x) + (y * y)) - num;
            return (1.0 - (Math.Min(num2, 1.0 - num2) * 4.0));
        }

        public double GetValue(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public double Frequency { get; set; }
    }
}

