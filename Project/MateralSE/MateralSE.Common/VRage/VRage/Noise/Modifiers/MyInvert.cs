namespace VRage.Noise.Modifiers
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Noise;

    public class MyInvert : IMyModule
    {
        public MyInvert(IMyModule module)
        {
            this.Module = module;
        }

        public double GetValue(double x) => 
            -this.Module.GetValue(x);

        public double GetValue(double x, double y) => 
            -this.Module.GetValue(x, y);

        public double GetValue(double x, double y, double z) => 
            -this.Module.GetValue(x, y, z);

        public IMyModule Module { get; set; }
    }
}

