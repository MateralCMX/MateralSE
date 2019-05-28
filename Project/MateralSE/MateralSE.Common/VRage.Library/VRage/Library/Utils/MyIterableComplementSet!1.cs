namespace VRage.Library.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using VRage.Library.Collections;

    public class MyIterableComplementSet<T> : IEnumerable<T>, IEnumerable
    {
        private Dictionary<T, int> m_index;
        private List<T> m_data;
        private int m_split;

        public MyIterableComplementSet()
        {
            this.m_index = new Dictionary<T, int>();
            this.m_data = new List<T>();
        }

        public void Add(T item)
        {
            this.m_index.Add(item, this.m_data.Count);
            this.m_data.Add(item);
        }

        public void AddToComplement(T item)
        {
            this.m_index.Add(item, this.m_data.Count);
            this.m_data.Add(item);
            this.MoveToComplement(item);
        }

        public void AllToComplement()
        {
            this.m_split = this.m_data.Count;
        }

        public void AllToSet()
        {
            this.m_split = 0;
        }

        public void Clear()
        {
            this.m_split = 0;
            this.m_index.Clear();
            this.m_data.Clear();
        }

        public void ClearComplement()
        {
            for (int i = this.m_split; i < this.m_data.Count; i++)
            {
                this.m_index.Remove(this.m_data[i]);
            }
            this.m_data.RemoveRange(this.m_split, this.m_data.Count - this.m_split);
        }

        public void ClearSet()
        {
            for (int i = this.m_split; i < this.m_data.Count; i++)
            {
                this.m_index.Remove(this.m_data[i]);
            }
            this.m_data.RemoveRange(this.m_split, this.m_data.Count - this.m_split);
        }

        public IEnumerable<T> Complement() => 
            MyRangeIterator<T>.ForRange(this.m_data, 0, this.m_split);

        public bool Contains(T item) => 
            this.m_index.ContainsKey(item);

        public IEnumerator<T> GetEnumerator() => 
            this.m_data.GetEnumerator();

        public bool IsInComplement(T item) => 
            (this.m_index[item] < this.m_split);

        public void MoveToComplement(T item)
        {
            T local = this.m_data[this.m_split];
            int num = this.m_index[item];
            this.m_data[this.m_split] = item;
            this.m_index[item] = this.m_split;
            this.m_data[num] = local;
            this.m_index[local] = num;
            this.m_split++;
        }

        public void MoveToSet(T item)
        {
            this.m_split--;
            T local = this.m_data[this.m_split];
            int num = this.m_index[item];
            this.m_data[this.m_split] = item;
            this.m_index[item] = this.m_split;
            this.m_data[num] = local;
            this.m_index[local] = num;
        }

        public void Remove(T item)
        {
            int split = this.m_index[item];
            if (this.m_split > split)
            {
                this.m_split--;
                T local = this.m_data[this.m_split];
                this.m_index[local] = split;
                this.m_data[split] = local;
                split = this.m_split;
            }
            int index = this.m_data.Count - 1;
            this.m_data[split] = this.m_data[index];
            this.m_index[this.m_data[index]] = split;
            this.m_index.Remove(item);
            this.m_data.RemoveAt(index);
        }

        public IEnumerable<T> Set() => 
            MyRangeIterator<T>.ForRange(this.m_data, this.m_split, this.m_data.Count);

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();
    }
}

