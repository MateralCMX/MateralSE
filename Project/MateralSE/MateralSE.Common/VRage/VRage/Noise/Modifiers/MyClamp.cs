namespace VRage.Noise.Modifiers
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Noise;
    using VRageMath;

    public class MyClamp : IMyModule
    {
        public MyClamp(IMyModule module, double lowerBound = -1.0, double upperBound = 1.0)
        {
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
            this.Module = module;
        }

        public double GetValue(double x) => 
            MathHelper.Clamp(this.Module.GetValue(x), this.LowerBound, this.UpperBound);

        public double GetValue(double x, double y) => 
            MathHelper.Clamp(this.Module.GetValue(x, y), this.LowerBound, this.UpperBound);

        public double GetValue(double x, double y, double z) => 
            MathHelper.Clamp(this.Module.GetValue(x, y, z), this.LowerBound, this.UpperBound);

        public double LowerBound { get; set; }

        public double UpperBound { get; set; }

        public IMyModule Module { get; set; }
    }
}

