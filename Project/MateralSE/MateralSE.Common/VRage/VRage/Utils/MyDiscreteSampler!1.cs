namespace VRage.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Library.Utils;

    public class MyDiscreteSampler<T> : IEnumerable<T>, IEnumerable
    {
        private T[] m_values;
        private MyDiscreteSampler m_sampler;

        public MyDiscreteSampler(Dictionary<T, float> densities) : this(densities.Keys, densities.Values)
        {
        }

        public MyDiscreteSampler(IEnumerable<T> values, IEnumerable<float> densities)
        {
            int num = values.Count<T>();
            this.m_values = new T[num];
            int index = 0;
            foreach (T local in values)
            {
                this.m_values[index] = local;
                index++;
            }
            this.m_sampler = new MyDiscreteSampler();
            this.m_sampler.Prepare(densities);
        }

        public MyDiscreteSampler(List<T> values, IEnumerable<float> densities)
        {
            this.m_values = new T[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                this.m_values[i] = values[i];
            }
            this.m_sampler = new MyDiscreteSampler();
            this.m_sampler.Prepare(densities);
        }

        public MyDiscreteSampler(T[] values, IEnumerable<float> densities)
        {
            this.m_values = new T[values.Length];
            Array.Copy(values, this.m_values, values.Length);
            this.m_sampler = new MyDiscreteSampler();
            this.m_sampler.Prepare(densities);
        }

        public IEnumerator<T> GetEnumerator() => 
            this.m_values.AsEnumerable<T>().GetEnumerator();

        public T Sample() => 
            this.m_values[this.m_sampler.Sample()];

        public T Sample(float sample) => 
            this.m_values[this.m_sampler.Sample(sample)];

        public T Sample(MyRandom rng) => 
            this.m_values[this.m_sampler.Sample(rng)];

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool Initialized =>
            this.m_sampler.Initialized;

        public int Count =>
            this.m_values.Length;
    }
}

