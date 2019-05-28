namespace VRage.Noise.Modifiers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Noise;
    using VRageMath;

    internal class MyTerrace : IMyModule
    {
        public List<double> ControlPoints;

        public MyTerrace(IMyModule module, bool invert = false)
        {
            this.Module = module;
            this.Invert = invert;
            this.ControlPoints = new List<double>(2);
        }

        public double GetValue(double x) => 
            this.Terrace(this.Module.GetValue(x), this.ControlPoints.Count - 1);

        public double GetValue(double x, double y) => 
            this.Terrace(this.Module.GetValue(x, y), this.ControlPoints.Count - 1);

        public double GetValue(double x, double y, double z) => 
            this.Terrace(this.Module.GetValue(x, y, z), this.ControlPoints.Count - 1);

        private double Terrace(double value, int countMask)
        {
            int num = 0;
            while ((num <= countMask) && (value >= this.ControlPoints[num]))
            {
                num++;
            }
            int num2 = MathHelper.Clamp(num - 1, 0, countMask);
            int num3 = MathHelper.Clamp(num, 0, countMask);
            if (num2 == num3)
            {
                return this.ControlPoints[num3];
            }
            double num4 = this.ControlPoints[num2];
            double num5 = this.ControlPoints[num3];
            double num6 = (value - num4) / (num5 - num4);
            if (this.Invert)
            {
                num6 = 1.0 - num6;
                num4 = num5;
                num5 = num4;
            }
            return MathHelper.Lerp(num4, num5, num6 * num6);
        }

        public IMyModule Module { get; set; }

        public bool Invert { get; set; }
    }
}

