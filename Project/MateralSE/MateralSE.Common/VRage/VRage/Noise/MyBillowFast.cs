namespace VRage.Noise
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class MyBillowFast : MyModuleFast
    {
        public MyBillowFast(MyNoiseQuality quality = 1, int layerCount = 6, int seed = 0, double frequency = 1.0, double lacunarity = 2.0, double persistence = 0.5)
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
                int seed = this.Seed;
                num2 = (2.0 * Math.Abs(base.GradCoherentNoise(x, this.Quality))) - 1.0;
                num += num2 * num3;
                x *= this.Lacunarity;
                num3 *= this.Persistence;
            }
            return (num + 0.5);
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
                int seed = this.Seed;
                num2 = (2.0 * Math.Abs(base.GradCoherentNoise(x, y, this.Quality))) - 1.0;
                num += num2 * num3;
                x *= this.Lacunarity;
                y *= this.Lacunarity;
                num3 *= this.Persistence;
            }
            return (num + 0.5);
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
                int seed = this.Seed;
                num2 = (2.0 * Math.Abs(base.GradCoherentNoise(x, y, z, this.Quality))) - 1.0;
                num += num2 * num3;
                x *= this.Lacunarity;
                y *= this.Lacunarity;
                z *= this.Lacunarity;
                num3 *= this.Persistence;
            }
            return (num + 0.5);
        }

        public MyNoiseQuality Quality { get; set; }

        public int LayerCount { get; set; }

        public double Frequency { get; set; }

        public double Lacunarity { get; set; }

        public double Persistence { get; set; }
    }
}

