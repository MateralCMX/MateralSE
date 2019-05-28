namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;

    public class MyAverageFiltering
    {
        private readonly List<double> m_samples;
        private readonly int m_sampleMaxCount;
        private int m_sampleCursor;
        private double? m_cachedFilteredValue;

        public MyAverageFiltering(int sampleCount)
        {
            this.m_sampleMaxCount = sampleCount;
            this.m_samples = new List<double>(sampleCount);
            this.m_cachedFilteredValue = null;
        }

        public void Add(double value)
        {
            this.m_cachedFilteredValue = null;
            if (this.m_samples.Count < this.m_sampleMaxCount)
            {
                this.m_samples.Add(value);
            }
            else
            {
                int sampleCursor = this.m_sampleCursor;
                this.m_sampleCursor = sampleCursor + 1;
                this.m_samples[sampleCursor] = value;
                if (this.m_sampleCursor >= this.m_sampleMaxCount)
                {
                    this.m_sampleCursor = 0;
                }
            }
        }

        public void Clear()
        {
            this.m_samples.Clear();
            this.m_cachedFilteredValue = null;
        }

        public double Get()
        {
            if (this.m_cachedFilteredValue != null)
            {
                return this.m_cachedFilteredValue.Value;
            }
            double num = 0.0;
            foreach (double num2 in this.m_samples)
            {
                num += num2;
            }
            if (this.m_samples.Count <= 0)
            {
                return 0.0;
            }
            double num3 = num / ((double) this.m_samples.Count);
            this.m_cachedFilteredValue = new double?(num3);
            return num3;
        }
    }
}

