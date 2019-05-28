namespace VRage.Noise
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MySimplexFast : MyModuleFast
    {
        private static Grad[] grad3;
        private int m_seedSimplex;
        private byte[] m_permSimplex = new byte[0x200];
        private byte[] m_gradSimplex = new byte[0x200];

        static MySimplexFast()
        {
            Grad[] gradArray1 = new Grad[12];
            gradArray1[0] = new Grad(1.0, 1.0, 0.0);
            gradArray1[1] = new Grad(-1.0, 1.0, 0.0);
            gradArray1[2] = new Grad(1.0, -1.0, 0.0);
            gradArray1[3] = new Grad(-1.0, -1.0, 0.0);
            gradArray1[4] = new Grad(1.0, 0.0, 1.0);
            gradArray1[5] = new Grad(-1.0, 0.0, 1.0);
            gradArray1[6] = new Grad(1.0, 0.0, -1.0);
            gradArray1[7] = new Grad(-1.0, 0.0, -1.0);
            gradArray1[8] = new Grad(0.0, 1.0, 1.0);
            gradArray1[9] = new Grad(0.0, -1.0, 1.0);
            gradArray1[10] = new Grad(0.0, 1.0, -1.0);
            gradArray1[11] = new Grad(0.0, -1.0, -1.0);
            grad3 = gradArray1;
        }

        public MySimplexFast(int seed = 1, double frequency = 1.0)
        {
            this.Seed = seed;
            this.Frequency = frequency;
        }

        private double Dot(Grad g, double x) => 
            (g.x * x);

        private double Dot(Grad g, double x, double y) => 
            ((g.x * x) + (g.y * y));

        private double Dot(Grad g, double x, double y, double z) => 
            (((g.x * x) + (g.y * y)) + (g.z * z));

        public override double GetValue(double x)
        {
            x *= this.Frequency;
            int num = MathHelper.Floor(x);
            double num2 = x - num;
            double num3 = num2 - 1.0;
            double num6 = 1.0 - (num2 * num2);
            num6 *= num6;
            double num1 = 1.0 - (num3 * num3);
            double num7 = num1 * num1;
            return (0.395 * (((num6 * num6) * this.Dot(grad3[this.m_gradSimplex[num & 0xff]], num2)) + ((num7 * num7) * this.Dot(grad3[this.m_gradSimplex[(num + 1) & 0xff]], num3))));
        }

        public override double GetValue(double x, double y)
        {
            int num7;
            int num8;
            double num18;
            double num19;
            double num20;
            x *= this.Frequency;
            y *= this.Frequency;
            double num = (x + y) * 0.3660254037844386;
            int num2 = MathHelper.Floor((double) (x + num));
            int num3 = MathHelper.Floor((double) (y + num));
            double num4 = (num2 + num3) * 0.21132486540518711;
            double num5 = (x - num2) + num4;
            double num6 = (y - num3) + num4;
            if (num5 > num6)
            {
                num7 = 1;
                num8 = 0;
            }
            else
            {
                num7 = 0;
                num8 = 1;
            }
            double num9 = (num5 - num7) + 0.21132486540518711;
            double num10 = (num6 - num8) + 0.21132486540518711;
            double num11 = ((num5 - 1.0) + 0.21132486540518711) + 0.21132486540518711;
            double num12 = ((num6 - 1.0) + 0.21132486540518711) + 0.21132486540518711;
            int num13 = num2 & 0xff;
            int index = num3 & 0xff;
            double num15 = (0.5 - (num5 * num5)) - (num6 * num6);
            double num16 = (0.5 - (num9 * num9)) - (num10 * num10);
            double num17 = (0.5 - (num11 * num11)) - (num12 * num12);
            if (num15 < 0.0)
            {
                num18 = 0.0;
            }
            else
            {
                num15 *= num15;
                num18 = (num15 * num15) * this.Dot(grad3[this.m_gradSimplex[(num13 + this.m_permSimplex[index]) & 0xff]], num5, num6);
            }
            if (num16 < 0.0)
            {
                num19 = 0.0;
            }
            else
            {
                num16 *= num16;
                num19 = (num16 * num16) * this.Dot(grad3[this.m_gradSimplex[((num13 + num7) + this.m_permSimplex[(index + num8) & 0xff]) & 0xff]], num9, num10);
            }
            if (num17 < 0.0)
            {
                num20 = 0.0;
            }
            else
            {
                num17 *= num17;
                num20 = (num17 * num17) * this.Dot(grad3[this.m_gradSimplex[((num13 + 1) + this.m_permSimplex[(index + 1) & 0xff]) & 0xff]], num11, num12);
            }
            return (70.0 * ((num18 + num19) + num20));
        }

        public override double GetValue(double x, double y, double z)
        {
            int num9;
            int num10;
            int num11;
            int num12;
            int num13;
            int num14;
            double num31;
            double num32;
            double num33;
            double num34;
            x *= this.Frequency;
            y *= this.Frequency;
            z *= this.Frequency;
            double num = ((x + y) + z) * 0.33333333333333331;
            int num2 = MathHelper.Floor((double) (x + num));
            int num3 = MathHelper.Floor((double) (y + num));
            int num4 = MathHelper.Floor((double) (z + num));
            double num5 = ((num2 + num3) + num4) * 0.16666666666666671;
            double num6 = (x - num2) + num5;
            double num7 = (y - num3) + num5;
            double num8 = (z - num4) + num5;
            if (num6 >= num7)
            {
                if (num7 >= num8)
                {
                    num9 = 1;
                    num10 = 0;
                    num11 = 0;
                    num12 = 1;
                    num13 = 1;
                    num14 = 0;
                }
                else if (num6 >= num8)
                {
                    num9 = 1;
                    num10 = 0;
                    num11 = 0;
                    num12 = 1;
                    num13 = 0;
                    num14 = 1;
                }
                else
                {
                    num9 = 0;
                    num10 = 0;
                    num11 = 1;
                    num12 = 1;
                    num13 = 0;
                    num14 = 1;
                }
            }
            else if (num7 < num8)
            {
                num9 = 0;
                num10 = 0;
                num11 = 1;
                num12 = 0;
                num13 = 1;
                num14 = 1;
            }
            else if (num6 < num8)
            {
                num9 = 0;
                num10 = 1;
                num11 = 0;
                num12 = 0;
                num13 = 1;
                num14 = 1;
            }
            else
            {
                num9 = 0;
                num10 = 1;
                num11 = 0;
                num12 = 1;
                num13 = 1;
                num14 = 0;
            }
            double num15 = (num6 - num9) + 0.16666666666666671;
            double num16 = (num7 - num10) + 0.16666666666666671;
            double num17 = (num8 - num11) + 0.16666666666666671;
            double num18 = (num6 - num12) + 0.33333333333333343;
            double num19 = (num7 - num13) + 0.33333333333333343;
            double num20 = (num8 - num14) + 0.33333333333333343;
            double num21 = (num6 - 1.0) + 0.50000000000000011;
            double num22 = (num7 - 1.0) + 0.50000000000000011;
            double num23 = (num8 - 1.0) + 0.50000000000000011;
            int num24 = num2 & 0xff;
            int num25 = num3 & 0xff;
            int index = num4 & 0xff;
            double num27 = ((0.6 - (num6 * num6)) - (num7 * num7)) - (num8 * num8);
            double num28 = ((0.6 - (num15 * num15)) - (num16 * num16)) - (num17 * num17);
            double num29 = ((0.6 - (num18 * num18)) - (num19 * num19)) - (num20 * num20);
            double num30 = ((0.6 - (num21 * num21)) - (num22 * num22)) - (num23 * num23);
            if (num27 < 0.0)
            {
                num31 = 0.0;
            }
            else
            {
                num27 *= num27;
                num31 = (num27 * num27) * this.Dot(grad3[this.m_gradSimplex[num24 + this.m_permSimplex[num25 + this.m_permSimplex[index]]]], num6, num7, num8);
            }
            if (num28 < 0.0)
            {
                num32 = 0.0;
            }
            else
            {
                num28 *= num28;
                num32 = (num28 * num28) * this.Dot(grad3[this.m_gradSimplex[(num24 + num9) + this.m_permSimplex[(num25 + num10) + this.m_permSimplex[index + num11]]]], num15, num16, num17);
            }
            if (num29 < 0.0)
            {
                num33 = 0.0;
            }
            else
            {
                num29 *= num29;
                num33 = (num29 * num29) * this.Dot(grad3[this.m_gradSimplex[(num24 + num12) + this.m_permSimplex[(num25 + num13) + this.m_permSimplex[index + num14]]]], num18, num19, num20);
            }
            if (num30 < 0.0)
            {
                num34 = 0.0;
            }
            else
            {
                num30 *= num30;
                num34 = (num30 * num30) * this.Dot(grad3[this.m_gradSimplex[(num24 + 1) + this.m_permSimplex[(num25 + 1) + this.m_permSimplex[index + 1]]]], num21, num22, num23);
            }
            return (32.0 * (((num31 + num32) + num33) + num34));
        }

        public override int Seed
        {
            get => 
                this.m_seedSimplex;
            set
            {
                this.m_seedSimplex = value;
                MyRNG yrng = new MyRNG(this.m_seedSimplex);
                for (int i = 0; i < 0x100; i++)
                {
                    this.m_permSimplex[i] = (byte) yrng.NextIntRange(0f, 255f);
                    this.m_permSimplex[0x100 + i] = this.m_permSimplex[i];
                    this.m_gradSimplex[i] = (byte) (this.m_permSimplex[i] % 12);
                    this.m_gradSimplex[0x100 + i] = this.m_gradSimplex[i];
                }
            }
        }

        public double Frequency { get; set; }

        private class Grad
        {
            public double x;
            public double y;
            public double z;

            public Grad(double x, double y, double z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }
    }
}

