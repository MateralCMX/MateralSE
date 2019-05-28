namespace VRage.Noise.Modifiers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Noise;
    using VRageMath;

    public class MyCurve : IMyModule
    {
        public List<MyCurveControlPoint> ControlPoints;

        public MyCurve(IMyModule module)
        {
            this.Module = module;
            this.ControlPoints = new List<MyCurveControlPoint>(4);
        }

        public double GetValue(double x)
        {
            double num = this.Module.GetValue(x);
            int max = this.ControlPoints.Count - 1;
            int num3 = 0;
            while ((num3 <= max) && (num >= this.ControlPoints[num3].Input))
            {
                num3++;
            }
            int num4 = MathHelper.Clamp(num3 - 2, 0, max);
            int num5 = MathHelper.Clamp(num3 - 1, 0, max);
            int num6 = MathHelper.Clamp(num3, 0, max);
            int num7 = MathHelper.Clamp(num3 + 1, 0, max);
            if (num5 == num6)
            {
                return this.ControlPoints[num5].Output;
            }
            double t = (num - this.ControlPoints[num5].Input) / (this.ControlPoints[num6].Input - this.ControlPoints[num5].Input);
            return MathHelper.CubicInterp(this.ControlPoints[num4].Output, this.ControlPoints[num5].Output, this.ControlPoints[num6].Output, this.ControlPoints[num7].Output, t);
        }

        public double GetValue(double x, double y)
        {
            double num = this.Module.GetValue(x, y);
            int max = this.ControlPoints.Count - 1;
            int num3 = 0;
            while ((num3 <= max) && (num >= this.ControlPoints[num3].Input))
            {
                num3++;
            }
            int num4 = MathHelper.Clamp(num3 - 2, 0, max);
            int num5 = MathHelper.Clamp(num3 - 1, 0, max);
            int num6 = MathHelper.Clamp(num3, 0, max);
            int num7 = MathHelper.Clamp(num3 + 1, 0, max);
            if (num5 == num6)
            {
                return this.ControlPoints[num5].Output;
            }
            double t = (num - this.ControlPoints[num5].Input) / (this.ControlPoints[num6].Input - this.ControlPoints[num5].Input);
            return MathHelper.CubicInterp(this.ControlPoints[num4].Output, this.ControlPoints[num5].Output, this.ControlPoints[num6].Output, this.ControlPoints[num7].Output, t);
        }

        public double GetValue(double x, double y, double z)
        {
            double num = this.Module.GetValue(x, y, z);
            int max = this.ControlPoints.Count - 1;
            int num3 = 0;
            while ((num3 <= max) && (num >= this.ControlPoints[num3].Input))
            {
                num3++;
            }
            int num4 = MathHelper.Clamp(num3 - 2, 0, max);
            int num5 = MathHelper.Clamp(num3 - 1, 0, max);
            int num6 = MathHelper.Clamp(num3, 0, max);
            int num7 = MathHelper.Clamp(num3 + 1, 0, max);
            if (num5 == num6)
            {
                return this.ControlPoints[num5].Output;
            }
            double t = (num - this.ControlPoints[num5].Input) / (this.ControlPoints[num6].Input - this.ControlPoints[num5].Input);
            return MathHelper.CubicInterp(this.ControlPoints[num4].Output, this.ControlPoints[num5].Output, this.ControlPoints[num6].Output, this.ControlPoints[num7].Output, t);
        }

        public IMyModule Module { get; set; }
    }
}

