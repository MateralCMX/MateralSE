namespace VRage.Noise
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyBillow : MyModule
    {
        public MyBillow(MyNoiseQuality quality = 1, int layerCount = 6, int seed = 0, double frequency = 1.0, double lacunarity = 2.0, double persistence = 0.5)
        {
            this.Quality = quality;
            this.LayerCount = layerCount;
            this.Seed = seed;
            this.Frequency = frequency;
            this.Lacunarity = lacunarity;
            this.Persistence = persistence;
        }

        public override double GetValue(double x)
        {
            double num = 0.0;
            double num2 = 0.0;
            double num3 = 1.0;
            x *= this.Frequency;
            for (int i = 0; i < this.LayerCount; i++)
            {
                long num4 = (this.Seed + i) & 0xffffffffUL;
                num2 = (2.0 * Math.Abs(base.GradCoherentNoise(x, (int) num4, this.Quality))) - 1.0;
                num += num2 * num3;
                x *= this.Lacunarity;
                num3 *= this.Persistence;
            }
            return MathHelper.Clamp((double) (num + 0.5), (double) -1.0, (double) 1.0);
        }

        public override double GetValue(double x, double y)
        {
            double num = 0.0;
            double num2 = 0.0;
            double num3 = 1.0;
            x *= this.Frequency;
            y *= this.Frequency;
            for (int i = 0; i < this.LayerCount; i++)
            {
                long num4 = (this.Seed + i) & 0xffffffffUL;
                num2 = (2.0 * Math.Abs(base.GradCoherentNoise(x, y, (int) num4, this.Quality))) - 1.0;
                num += num2 * num3;
                x *= this.Lacunarity;
                y *= this.Lacunarity;
                num3 *= this.Persistence;
            }
            return MathHelper.Clamp((double) (num + 0.5), (double) -1.0, (double) 1.0);
        }

        public override double GetValue(double x, double y, double z)
        {
            double num = 0.0;
            double num2 = 0.0;
            double num3 = 1.0;
            x *= this.Frequency;
            y *= this.Frequency;
            z *= this.Frequency;
            for (int i = 0; i < this.LayerCount; i++)
            {
                long num4 = (this.Seed + i) & 0xffffffffUL;
                num2 = (2.0 * Math.Abs(base.GradCoherentNoise(x, y, z, (int) num4, this.Quality))) - 1.0;
                num += num2 * num3;
                x *= this.Lacunarity;
                y *= this.Lacunarity;
                z *= this.Lacunarity;
                num3 *= this.Persistence;
            }
            return MathHelper.Clamp((double) (num + 0.5), (double) -1.0, (double) 1.0);
        }

        public MyNoiseQuality Quality { get; set; }

        public int LayerCount { get; set; }

        public int Seed { get; set; }

        public double Frequency { get; set; }

        public double Lacunarity { get; set; }

        public double Persistence { get; set; }
    }
}

