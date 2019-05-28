namespace VRage.Noise.Combiners
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Noise;
    using VRageMath;

    public class MyBlend : IMyModule
    {
        public MyBlend(IMyModule sourceModule1, IMyModule sourceModule2, IMyModule weight)
        {
            this.Source1 = sourceModule1;
            this.Source2 = sourceModule2;
            this.Weight = weight;
        }

        public double GetValue(double x) => 
            MathHelper.Lerp(this.Source1.GetValue(x), this.Source2.GetValue(x), MathHelper.Saturate(this.Weight.GetValue(x)));

        public double GetValue(double x, double y) => 
            MathHelper.Lerp(this.Source1.GetValue(x, y), this.Source2.GetValue(x, y), MathHelper.Saturate(this.Weight.GetValue(x, y)));

        public double GetValue(double x, double y, double z) => 
            MathHelper.Lerp(this.Source1.GetValue(x, y, z), this.Source2.GetValue(x, y, z), MathHelper.Saturate(this.Weight.GetValue(x, y, z)));

        public IMyModule Source1 { get; set; }

        public IMyModule Source2 { get; set; }

        public IMyModule Weight { get; set; }
    }
}

