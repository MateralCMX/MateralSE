namespace VRage.Noise.Combiners
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Noise;

    public class MyPower : IMyModule
    {
        private double powerOffset;

        public MyPower(IMyModule baseModule, IMyModule powerModule, double powerOffset = 0.0)
        {
            this.Base = baseModule;
            this.Power = powerModule;
            this.powerOffset = powerOffset;
        }

        public double GetValue(double x) => 
            Math.Pow(this.Base.GetValue(x), this.powerOffset + this.Power.GetValue(x));

        public double GetValue(double x, double y) => 
            Math.Pow(this.Base.GetValue(x, y), this.powerOffset + this.Power.GetValue(x, y));

        public double GetValue(double x, double y, double z) => 
            Math.Pow(this.Base.GetValue(x, y, z), this.powerOffset + this.Power.GetValue(x, y, z));

        public IMyModule Base { get; set; }

        public IMyModule Power { get; set; }
    }
}

