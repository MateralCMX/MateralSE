namespace VRage.Noise
{
    using System;
    using VRageMath;

    public abstract class MyModule : IMyModule
    {
        protected MyModule()
        {
        }

        public abstract double GetValue(double x);
        public abstract double GetValue(double x, double y);
        public abstract double GetValue(double x, double y, double z);
        protected double GradCoherentNoise(double x, int seed, MyNoiseQuality quality)
        {
            int ix = MathHelper.Floor(x);
            double amount = 0.0;
            switch (quality)
            {
                case MyNoiseQuality.Low:
                    amount = x - ix;
                    break;

                case MyNoiseQuality.Standard:
                    amount = MathHelper.SCurve3((double) (x - ix));
                    break;

                case MyNoiseQuality.High:
                    amount = MathHelper.SCurve5((double) (x - ix));
                    break;

                default:
                    break;
            }
            return MathHelper.Lerp(this.GradNoise(x, ix, (long) seed), this.GradNoise(x, ix + 1, (long) seed), amount);
        }

        protected double GradCoherentNoise(double x, double y, int seed, MyNoiseQuality quality)
        {
            int ix = MathHelper.Floor(x);
            int iy = MathHelper.Floor(y);
            int num3 = ix + 1;
            int num4 = iy + 1;
            double amount = 0.0;
            double num6 = 0.0;
            switch (quality)
            {
                case MyNoiseQuality.Low:
                    amount = x - ix;
                    num6 = y - iy;
                    break;

                case MyNoiseQuality.Standard:
                    amount = MathHelper.SCurve3((double) (x - ix));
                    num6 = MathHelper.SCurve3((double) (y - iy));
                    break;

                case MyNoiseQuality.High:
                    amount = MathHelper.SCurve5((double) (x - ix));
                    num6 = MathHelper.SCurve5((double) (y - iy));
                    break;

                default:
                    break;
            }
            return MathHelper.Lerp(MathHelper.Lerp(this.GradNoise(x, y, ix, iy, (long) seed), this.GradNoise(x, y, num3, iy, (long) seed), amount), MathHelper.Lerp(this.GradNoise(x, y, ix, num4, (long) seed), this.GradNoise(x, y, num3, num4, (long) seed), amount), num6);
        }

        protected double GradCoherentNoise(double x, double y, double z, int seed, MyNoiseQuality quality)
        {
            int ix = MathHelper.Floor(x);
            int iy = MathHelper.Floor(y);
            int iz = MathHelper.Floor(z);
            int num4 = ix + 1;
            int num5 = iy + 1;
            int num6 = iz + 1;
            double amount = 0.0;
            double num8 = 0.0;
            double num9 = 0.0;
            switch (quality)
            {
                case MyNoiseQuality.Low:
                    amount = x - ix;
                    num8 = y - iy;
                    num9 = z - iz;
                    break;

                case MyNoiseQuality.Standard:
                    amount = MathHelper.SCurve3((double) (x - ix));
                    num8 = MathHelper.SCurve3((double) (y - iy));
                    num9 = MathHelper.SCurve3((double) (z - iz));
                    break;

                case MyNoiseQuality.High:
                    amount = MathHelper.SCurve5((double) (x - ix));
                    num8 = MathHelper.SCurve5((double) (y - iy));
                    num9 = MathHelper.SCurve5((double) (z - iz));
                    break;

                default:
                    break;
            }
            return MathHelper.Lerp(MathHelper.Lerp(MathHelper.Lerp(this.GradNoise(x, y, z, ix, iy, iz, (long) seed), this.GradNoise(x, y, z, num4, iy, iz, (long) seed), amount), MathHelper.Lerp(this.GradNoise(x, y, z, ix, num5, iz, (long) seed), this.GradNoise(x, y, z, num4, num5, iz, (long) seed), amount), num8), MathHelper.Lerp(MathHelper.Lerp(this.GradNoise(x, y, z, ix, iy, num6, (long) seed), this.GradNoise(x, y, z, num4, iy, num6, (long) seed), amount), MathHelper.Lerp(this.GradNoise(x, y, z, ix, num5, num6, (long) seed), this.GradNoise(x, y, z, num4, num5, num6, (long) seed), amount), num8), num9);
        }

        private double GradNoise(double fx, int ix, long seed)
        {
            long num = (long) (((ulong) ((0x653 * ix) + (0x3f5L * seed))) & 0xffffffffUL);
            num = ((num >> 8) ^ num) & 0xffL;
            return (MyNoiseDefaults.RandomVectors[(int) ((IntPtr) num)] * (fx - ix));
        }

        private double GradNoise(double fx, double fy, int ix, int iy, long seed)
        {
            long num = (long) (((ulong) (((0x653 * ix) + (0x7a69 * iy)) + (0x3f5L * seed))) & 0xffffffffUL);
            num = (((num >> 8) ^ num) & 0xffL) << 1;
            double num2 = MyNoiseDefaults.RandomVectors[(int) ((IntPtr) (num + 1L))];
            return ((MyNoiseDefaults.RandomVectors[(int) ((IntPtr) num)] * (fx - ix)) + (num2 * (fy - iy)));
        }

        private double GradNoise(double fx, double fy, double fz, int ix, int iy, int iz, long seed)
        {
            long num = ((((0x653 * ix) + (0x7a69 * iy)) + (0x1b3b * iz)) + (0x3f5L * seed)) & 0x7fffffffL;
            num = (((num >> 8) ^ num) & 0xffL) * 3L;
            double num2 = MyNoiseDefaults.RandomVectors[(int) ((IntPtr) (num + 1L))];
            double num3 = MyNoiseDefaults.RandomVectors[(int) ((IntPtr) (num + 2L))];
            return (((MyNoiseDefaults.RandomVectors[(int) ((IntPtr) num)] * (fx - ix)) + (num2 * (fy - iy))) + (num3 * (fz - iz)));
        }
    }
}

