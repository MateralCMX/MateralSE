namespace VRage.Noise
{
    using System;
    using VRage.Library.Utils;
    using VRageMath;

    public abstract class MyModuleFast : IMyModule
    {
        private int m_seed;
        private byte[] m_perm = new byte[0x200];
        private float[] m_grad = new float[0x200];

        protected MyModuleFast()
        {
        }

        public abstract double GetValue(double x);
        public abstract double GetValue(double x, double y);
        public abstract double GetValue(double x, double y, double z);
        protected double GradCoherentNoise(double x, MyNoiseQuality quality)
        {
            int num = MathHelper.Floor(x);
            int index = num & 0xff;
            double amount = 0.0;
            switch (quality)
            {
                case MyNoiseQuality.Low:
                    amount = x - num;
                    break;

                case MyNoiseQuality.Standard:
                    amount = MathHelper.SCurve3((double) (x - num));
                    break;

                case MyNoiseQuality.High:
                    amount = MathHelper.SCurve5((double) (x - num));
                    break;

                default:
                    break;
            }
            return MathHelper.Lerp((double) this.m_grad[this.m_perm[index]], (double) this.m_grad[this.m_perm[index + 1]], amount);
        }

        protected double GradCoherentNoise(double x, double y, MyNoiseQuality quality)
        {
            int num = MathHelper.Floor(x);
            int num2 = MathHelper.Floor(y);
            int index = num & 0xff;
            int num4 = num2 & 0xff;
            double amount = 0.0;
            double num6 = 0.0;
            switch (quality)
            {
                case MyNoiseQuality.Low:
                    amount = x - num;
                    num6 = y - num2;
                    break;

                case MyNoiseQuality.Standard:
                    amount = MathHelper.SCurve3((double) (x - num));
                    num6 = MathHelper.SCurve3((double) (y - num2));
                    break;

                case MyNoiseQuality.High:
                    amount = MathHelper.SCurve5((double) (x - num));
                    num6 = MathHelper.SCurve5((double) (y - num2));
                    break;

                default:
                    break;
            }
            int num7 = this.m_perm[index] + num4;
            int num8 = this.m_perm[index + 1] + num4;
            int num9 = this.m_perm[num7];
            int num10 = this.m_perm[num7 + 1];
            int num11 = this.m_perm[num8];
            int num12 = this.m_perm[num8 + 1];
            return MathHelper.Lerp(MathHelper.Lerp((double) this.m_grad[num9], (double) this.m_grad[num11], amount), MathHelper.Lerp((double) this.m_grad[num10], (double) this.m_grad[num12], amount), num6);
        }

        protected double GradCoherentNoise(double x, double y, double z, MyNoiseQuality quality)
        {
            int num = MathHelper.Floor(x);
            int num2 = MathHelper.Floor(y);
            int num3 = MathHelper.Floor(z);
            int index = num & 0xff;
            int num5 = num2 & 0xff;
            int num6 = num3 & 0xff;
            double amount = 0.0;
            double num8 = 0.0;
            double num9 = 0.0;
            switch (quality)
            {
                case MyNoiseQuality.Low:
                    amount = x - num;
                    num8 = y - num2;
                    num9 = z - num3;
                    break;

                case MyNoiseQuality.Standard:
                    amount = MathHelper.SCurve3((double) (x - num));
                    num8 = MathHelper.SCurve3((double) (y - num2));
                    num9 = MathHelper.SCurve3((double) (z - num3));
                    break;

                case MyNoiseQuality.High:
                    amount = MathHelper.SCurve5((double) (x - num));
                    num8 = MathHelper.SCurve5((double) (y - num2));
                    num9 = MathHelper.SCurve5((double) (z - num3));
                    break;

                default:
                    break;
            }
            int num10 = this.m_perm[index] + num5;
            int num11 = this.m_perm[index + 1] + num5;
            int num12 = this.m_perm[num10] + num6;
            int num13 = this.m_perm[num10 + 1] + num6;
            int num14 = this.m_perm[num11] + num6;
            int num15 = this.m_perm[num11 + 1] + num6;
            return MathHelper.Lerp(MathHelper.Lerp(MathHelper.Lerp((double) this.m_grad[num12], (double) this.m_grad[num14], amount), MathHelper.Lerp((double) this.m_grad[num13], (double) this.m_grad[num15], amount), num8), MathHelper.Lerp(MathHelper.Lerp((double) this.m_grad[num12 + 1], (double) this.m_grad[num14 + 1], amount), MathHelper.Lerp((double) this.m_grad[num13 + 1], (double) this.m_grad[num15 + 1], amount), num8), num9);
        }

        public virtual int Seed
        {
            get => 
                this.m_seed;
            set
            {
                this.m_seed = value;
                Random random = new Random(MyRandom.EnableDeterminism ? 1 : this.m_seed);
                for (int i = 0; i < 0x100; i++)
                {
                    byte num2 = (byte) random.Next(0xff);
                    this.m_perm[i] = num2;
                    this.m_perm[0x100 + i] = num2;
                    this.m_grad[i] = -1f + (2f * (((float) this.m_perm[i]) / 255f));
                    this.m_grad[0x100 + i] = this.m_grad[i];
                }
            }
        }
    }
}

