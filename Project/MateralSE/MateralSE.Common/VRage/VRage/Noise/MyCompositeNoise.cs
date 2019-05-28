namespace VRage.Noise
{
    using System;
    using VRage.Library.Utils;

    public class MyCompositeNoise : MyModule
    {
        private IMyModule[] m_noises;
        private float[] m_amplitudeScales;
        private float m_normalizationFactor = 1f;
        private int m_numNoises;

        public MyCompositeNoise(int numNoises, float startFrequency)
        {
            this.m_numNoises = numNoises;
            this.m_noises = new IMyModule[this.m_numNoises];
            this.m_amplitudeScales = new float[this.m_numNoises];
            this.m_normalizationFactor = 2f - (1f / ((float) Math.Pow(2.0, (double) (this.m_numNoises - 1))));
            float num = startFrequency;
            for (int i = 0; i < this.m_numNoises; i++)
            {
                this.m_amplitudeScales[i] = 1f / ((float) Math.Pow(2.0, (double) i));
                this.m_noises[i] = new MySimplexFast(MyRandom.Instance.Next(), (double) num);
                num *= 2.01f;
            }
        }

        public override double GetValue(double x)
        {
            double num = 0.0;
            for (int i = 0; i < this.m_numNoises; i++)
            {
                num += this.m_amplitudeScales[i] * this.m_noises[i].GetValue(x);
            }
            return this.NormalizeValue(num);
        }

        public override double GetValue(double x, double y)
        {
            double num = 0.0;
            for (int i = 0; i < this.m_numNoises; i++)
            {
                num += this.m_amplitudeScales[i] * this.m_noises[i].GetValue(x, y);
            }
            return this.NormalizeValue(num);
        }

        public override double GetValue(double x, double y, double z)
        {
            double num = 0.0;
            for (int i = 0; i < this.m_numNoises; i++)
            {
                num += this.m_amplitudeScales[i] * this.m_noises[i].GetValue(x, y, z);
            }
            return this.NormalizeValue(num);
        }

        public float GetValue(double x, double y, double z, int numNoises)
        {
            double num = 0.0;
            for (int i = 0; i < numNoises; i++)
            {
                num += this.m_amplitudeScales[i] * this.m_noises[i].GetValue(x, y, z);
            }
            return (float) ((0.5 * num) + 0.5);
        }

        private double NormalizeValue(double value) => 
            (((0.5 * value) / ((double) this.m_normalizationFactor)) + 0.5);
    }
}

