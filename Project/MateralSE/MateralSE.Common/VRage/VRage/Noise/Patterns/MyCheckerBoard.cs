namespace VRage.Noise.Patterns
{
    using System;
    using VRage.Noise;
    using VRageMath;

    public class MyCheckerBoard : IMyModule
    {
        public double GetValue(double x) => 
            (((MathHelper.Floor(x) & 1) == 1) ? -1.0 : 1.0);

        public double GetValue(double x, double y) => 
            ((((MathHelper.Floor(x) & 1) ^ (MathHelper.Floor(y) & 1)) == 1) ? -1.0 : 1.0);

        public double GetValue(double x, double y, double z)
        {
            int num2 = MathHelper.Floor(z) & 1;
            return (((((MathHelper.Floor(x) & 1) ^ (MathHelper.Floor(y) & 1)) ^ num2) == 1) ? -1.0 : 1.0);
        }
    }
}

