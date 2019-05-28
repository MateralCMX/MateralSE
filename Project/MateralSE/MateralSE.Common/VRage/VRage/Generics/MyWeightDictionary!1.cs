namespace VRage.Generics
{
    using System;
    using System.Collections.Generic;

    public class MyWeightDictionary<T>
    {
        private Dictionary<T, float> m_data;
        private float m_sum;

        public MyWeightDictionary(Dictionary<T, float> data)
        {
            this.m_data = data;
            this.m_sum = 0f;
            foreach (KeyValuePair<T, float> pair in data)
            {
                this.m_sum += pair.Value;
            }
        }

        public T GetItemByWeight(float weight)
        {
            float num = 0f;
            T key = default(T);
            using (Dictionary<T, float>.Enumerator enumerator = this.m_data.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<T, float> current = enumerator.Current;
                    key = current.Key;
                    num += current.Value;
                    if (num > weight)
                    {
                        return key;
                    }
                }
            }
            return key;
        }

        public T GetItemByWeightNormalized(float weightNormalized) => 
            this.GetItemByWeight(weightNormalized * this.m_sum);

        public T GetRandomItem(Random rnd)
        {
            float weight = ((float) rnd.NextDouble()) * this.m_sum;
            return this.GetItemByWeight(weight);
        }

        public float GetSum() => 
            this.m_sum;

        public int Count =>
            this.m_data.Count;
    }
}

