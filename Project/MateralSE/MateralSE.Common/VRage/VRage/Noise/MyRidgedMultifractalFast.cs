namespace VRage.Noise
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyRidgedMultifractalFast : MyModuleFast
    {
        private const int MAX_OCTAVES = 20;
        private double m_lacunarity;
        private double[] m_spectralWeights = new double[20];

        public MyRidgedMultifractalFast(MyNoiseQuality quality = 1, int layerCount = 6, int seed = 0, double frequency = 1.0, double gain = 2.0, double lacunarity = 2.0, double offset = 1.0)
        {
            this.Quality = quality;
            this.LayerCount = layerCount;
            this.Seed = seed;
            this.Frequency = frequency;
            this.Gain = gain;
            this.Lacunarity = lacunarity;
            this.Offset = offset;
        }

        private void CalculateSpectralWeights()
        {
            double num = 1.0;
            double x = 1.0;
            for (int i = 0; i < 20; i++)
            {
                this.m_spectralWeights[i] = Math.Pow(x, -num);
                x *= this.Lacunarity;
            }
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
                num2 = Math.Abs(base.GradCoherentNoise(x, this.Quality));
                num2 = this.Offset - num2;
                num2 = (num2 * num2) * num3;
                num3 = MathHelper.Saturate((double) (num2 * this.Gain));
                num += num2 * this.m_spectralWeights[i];
                x *= this.Lacunarity;
            }
            return (num - 1.0);
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
                num2 = Math.Abs(base.GradCoherentNoise(x, y, this.Quality));
                num2 = this.Offset - num2;
                num2 = (num2 * num2) * num3;
                num3 = MathHelper.Saturate((double) (num2 * this.Gain));
                num += num2 * this.m_spectralWeights[i];
                x *= this.Lacunarity;
                y *= this.Lacunarity;
            }
            return (num - 1.0);
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
                num2 = Math.Abs(base.GradCoherentNoise(x, y, z, this.Quality));
                num2 = this.Offset - num2;
                num2 = (num2 * num2) * num3;
                num3 = MathHelper.Saturate((double) (num2 * this.Gain));
                num += num2 * this.m_spectralWeights[i];
                x *= this.Lacunarity;
                y *= this.Lacunarity;
                z *= this.Lacunarity;
            }
            return (num - 1.0);
        }

        public MyNoiseQuality Quality { get; set; }

        public int LayerCount { get; set; }

        public double Frequency { get; set; }

        public double Gain { get; set; }

        public double Lacunarity
        {
            get => 
                this.m_lacunarity;
            set
            {
                this.m_lacunarity = value;
                this.CalculateSpectralWeights();
            }
        }

        public double Offset { get; set; }
    }
}

