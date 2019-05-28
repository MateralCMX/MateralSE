namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Library.Utils;

    public class MyDiscreteSampler
    {
        private SamplingBin[] m_bins;
        private int m_binCount;
        private bool m_initialized;

        public MyDiscreteSampler()
        {
            this.m_binCount = 0;
            this.m_bins = null;
            this.m_initialized = false;
        }

        public MyDiscreteSampler(int prealloc) : this()
        {
            this.m_bins = new SamplingBin[prealloc];
        }

        private void AllocateBins(int numDensities)
        {
            if ((this.m_bins == null) || (this.m_binCount < numDensities))
            {
                this.m_bins = new SamplingBin[numDensities];
            }
            this.m_binCount = numDensities;
        }

        private void InitializeBins(IEnumerable<float> densities, float normalizationFactor)
        {
            int index = 0;
            foreach (float num2 in densities)
            {
                this.m_bins[index].BinIndex = index;
                this.m_bins[index].Split = num2 * normalizationFactor;
                this.m_bins[index].Donator = 0;
                index++;
            }
            Array.Sort<SamplingBin>(this.m_bins, 0, this.m_binCount, BinComparer.Static);
        }

        public void Prepare(IEnumerable<float> densities)
        {
            float num = 0f;
            int numDensities = 0;
            foreach (float num4 in densities)
            {
                num += num4;
                numDensities++;
            }
            if (numDensities != 0)
            {
                float normalizationFactor = ((float) numDensities) / num;
                this.AllocateBins(numDensities);
                this.InitializeBins(densities, normalizationFactor);
                this.ProcessDonators();
                this.m_initialized = true;
            }
        }

        private unsafe void ProcessDonators()
        {
            int index = 0;
            int num2 = 1;
            int num3 = this.m_binCount - 1;
            while (num2 <= num3)
            {
                this.m_bins[index].Donator = this.m_bins[num3].BinIndex;
                float* singlePtr1 = (float*) ref this.m_bins[num3].Split;
                singlePtr1[0] -= 1f - this.m_bins[index].Split;
                if (this.m_bins[num3].Split < 1f)
                {
                    index = num3;
                    num3--;
                    continue;
                }
                index = num2;
                num2++;
            }
        }

        public SamplingBin[] ReadBins()
        {
            SamplingBin[] destinationArray = new SamplingBin[this.m_binCount];
            Array.Copy(this.m_bins, destinationArray, destinationArray.Length);
            return destinationArray;
        }

        public int Sample()
        {
            int randomInt = MyUtils.GetRandomInt(this.m_binCount);
            SamplingBin bin = this.m_bins[randomInt];
            return ((MyUtils.GetRandomFloat() > bin.Split) ? bin.Donator : bin.BinIndex);
        }

        public int Sample(float rate)
        {
            float single1 = this.m_binCount * rate;
            int index = (int) single1;
            if (index == this.m_binCount)
            {
                index--;
            }
            SamplingBin bin = this.m_bins[index];
            return (((single1 - index) >= bin.Split) ? bin.Donator : bin.BinIndex);
        }

        public int Sample(MyRandom rng)
        {
            int index = rng.Next(this.m_binCount);
            SamplingBin bin = this.m_bins[index];
            return ((rng.NextFloat() > bin.Split) ? bin.Donator : bin.BinIndex);
        }

        public bool Initialized =>
            this.m_initialized;

        private class BinComparer : IComparer<MyDiscreteSampler.SamplingBin>
        {
            public static MyDiscreteSampler.BinComparer Static = new MyDiscreteSampler.BinComparer();

            public int Compare(MyDiscreteSampler.SamplingBin x, MyDiscreteSampler.SamplingBin y)
            {
                float num = x.Split - y.Split;
                return ((num >= 0f) ? ((num <= 0f) ? 0 : 1) : -1);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SamplingBin
        {
            public float Split;
            public int BinIndex;
            public int Donator;
            public override string ToString()
            {
                object[] objArray1 = new object[] { "[", this.BinIndex, "] <- (", this.Donator, ") : ", this.Split };
                return string.Concat(objArray1);
            }
        }
    }
}

