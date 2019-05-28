namespace VRage.Noise.Modifiers
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Noise;

    public class MyRemapTo01 : IMyModule
    {
        public MyRemapTo01(IMyModule module)
        {
            this.Module = module;
        }

        public double GetValue(double x) => 
            ((this.Module.GetValue(x) + 1.0) * 0.5);

        public double GetValue(double x, double y) => 
            ((this.Module.GetValue(x, y) + 1.0) * 0.5);

        public double GetValue(double x, double y, double z) => 
            ((this.Module.GetValue(x, y, z) + 1.0) * 0.5);

        public IMyModule Module { get; set; }
    }
}

