namespace VRage.Noise.Modifiers
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Noise;

    public class MyExponent : IMyModule
    {
        public MyExponent(IMyModule module, double exponent = 2.0)
        {
            this.Exponent = exponent;
            this.Module = module;
        }

        public double GetValue(double x) => 
            ((Math.Pow((this.Module.GetValue(x) + 1.0) * 0.5, this.Exponent) * 2.0) - 1.0);

        public double GetValue(double x, double y) => 
            ((Math.Pow((this.Module.GetValue(x, y) + 1.0) * 0.5, this.Exponent) * 2.0) - 1.0);

        public double GetValue(double x, double y, double z) => 
            ((Math.Pow((this.Module.GetValue(x, y, z) + 1.0) * 0.5, this.Exponent) * 2.0) - 1.0);

        public double Exponent { get; set; }

        public IMyModule Module { get; set; }
    }
}

