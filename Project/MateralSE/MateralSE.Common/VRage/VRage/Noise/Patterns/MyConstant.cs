namespace VRage.Noise.Patterns
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Noise;

    public class MyConstant : IMyModule
    {
        public MyConstant(double constant)
        {
            this.Constant = constant;
        }

        public double GetValue(double x) => 
            this.Constant;

        public double GetValue(double x, double y) => 
            this.Constant;

        public double GetValue(double x, double y, double z) => 
            this.Constant;

        public double Constant { get; set; }
    }
}

