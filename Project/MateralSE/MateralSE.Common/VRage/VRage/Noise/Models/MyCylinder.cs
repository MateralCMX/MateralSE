namespace VRage.Noise.Models
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Noise;

    internal class MyCylinder : IMyModule
    {
        public MyCylinder(IMyModule module)
        {
            this.Module = module;
        }

        public double GetValue(double x)
        {
            throw new NotImplementedException();
        }

        public double GetValue(double angle, double height)
        {
            double x = Math.Cos(angle);
            return this.Module.GetValue(x, height, Math.Sin(angle));
        }

        public double GetValue(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public IMyModule Module { get; set; }
    }
}

