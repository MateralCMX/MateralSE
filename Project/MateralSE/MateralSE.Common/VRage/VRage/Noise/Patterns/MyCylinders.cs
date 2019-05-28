namespace VRage.Noise.Patterns
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Noise;
    using VRageMath;

    internal class MyCylinders : IMyModule
    {
        public MyCylinders(double frequnecy = 1.0)
        {
            this.Frequency = frequnecy;
        }

        public double GetValue(double x)
        {
            x *= this.Frequency;
            double n = Math.Sqrt((x * x) + (x * x));
            double num2 = n - MathHelper.Floor(n);
            return (1.0 - (Math.Min(num2, 1.0 - num2) * 4.0));
        }

        public double GetValue(double x, double z)
        {
            x *= this.Frequency;
            z *= this.Frequency;
            double n = Math.Sqrt((x * x) + (z * z));
            double num2 = n - MathHelper.Floor(n);
            return (1.0 - (Math.Min(num2, 1.0 - num2) * 4.0));
        }

        public double GetValue(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public double Frequency { get; set; }
    }
}

