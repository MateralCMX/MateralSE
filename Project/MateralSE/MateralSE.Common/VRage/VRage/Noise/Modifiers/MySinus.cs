namespace VRage.Noise.Modifiers
{
    using System;
    using VRage.Noise;

    public class MySinus : IMyModule
    {
        private IMyModule module;

        public MySinus(IMyModule module)
        {
            this.module = module;
        }

        public double GetValue(double x) => 
            Math.Sin(this.module.GetValue(x) * 3.1415926535897931);

        public double GetValue(double x, double y) => 
            Math.Sin(this.module.GetValue(x, y) * 3.1415926535897931);

        public double GetValue(double x, double y, double z) => 
            Math.Sin(this.module.GetValue(x, y, z) * 3.1415926535897931);
    }
}

