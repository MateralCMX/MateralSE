namespace VRage.Noise
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class MyPerlinFast : MyModuleFast
    {
        public MyPerlinFast(MyNoiseQuality quality = 1, int octaveCount = 6, int seed = 0, double frequency = 1.0, double lacunarity = 2.0, double persistence = 0.5)
        {
            this.Quality = quality;
            this.OctaveCount = octaveCount;
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
            for (int i = 0; i < this.OctaveCount; i++)
            {
                int seed = this.Seed;
                num2 = base.GradCoherentNoise(x, this.Quality);
                num += num2 * num3;
                x *= this.Lacunarity;
                num3 *= this.Persistence;
            }
            return num;
        }

        public override double GetValue(double x, double y)
        {
            double num = 0.0;
            double num2 = 0.0;
            double num3 = 1.0;
            x *= this.Frequency;
            y *= this.Frequency;
            for (int i = 0; i < this.OctaveCount; i++)
            {
                int seed = this.Seed;
                num2 = base.GradCoherentNoise(x, y, this.Quality);
                num += num2 * num3;
                x *= this.Lacunarity;
                y *= this.Lacunarity;
                num3 *= this.Persistence;
            }
            return num;
        }

        public override double GetValue(double x, double y, double z)
        {
            double num = 0.0;
            double num2 = 0.0;
            double num3 = 1.0;
            x *= this.Frequency;
            y *= this.Frequency;
            z *= this.Frequency;
            for (int i = 0; i < this.OctaveCount; i++)
            {
                int seed = this.Seed;
                num2 = base.GradCoherentNoise(x, y, z, this.Quality);
                num += num2 * num3;
                x *= this.Lacunarity;
                y *= this.Lacunarity;
                z *= this.Lacunarity;
                num3 *= this.Persistence;
            }
            return num;
        }

        public MyNoiseQuality Quality { get; set; }

        public int OctaveCount { get; set; }

        public double Frequency { get; set; }

        public double Lacunarity { get; set; }

        public double Persistence { get; set; }
    }
}

