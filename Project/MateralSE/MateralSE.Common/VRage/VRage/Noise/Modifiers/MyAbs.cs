namespace VRage.Noise.Modifiers
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Noise;

    public class MyAbs : IMyModule
    {
        public MyAbs(IMyModule module)
        {
            this.Module = module;
        }

        public double GetValue(double x) => 
            Math.Abs(this.Module.GetValue(x));

        public double GetValue(double x, double y) => 
            Math.Abs(this.Module.GetValue(x, y));

        public double GetValue(double x, double y, double z) => 
            Math.Abs(this.Module.GetValue(x, y, z));

        public IMyModule Module { get; set; }
    }
}

