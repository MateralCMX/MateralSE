namespace VRage.Library.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class MyMultiKeyIndex<TKey1, TKey2, TValue> : IEnumerable<TValue>, IEnumerable
    {
        private Dictionary<TKey1, TValue> m_index1;
        private Dictionary<TKey2, TValue> m_index2;
        public readonly Func<TValue, TKey1> KeySelector1;
        public readonly Func<TValue, TKey2> KeySelector2;

        public MyMultiKeyIndex(Func<TValue, TKey1> keySelector1, Func<TValue, TKey2> keySelector2, int capacity = 0, EqualityComparer<TKey1> keyComparer1 = null, EqualityComparer<TKey2> keyComparer2 = null)
        {
            this.m_index1 = new Dictionary<TKey1, TValue>();
            this.m_index2 = new Dictionary<TKey2, TValue>();
            this.m_index1 = new Dictionary<TKey1, TValue>(capacity, keyComparer1);
            this.m_index2 = new Dictionary<TKey2, TValue>(capacity, keyComparer2);
            this.KeySelector1 = keySelector1;
            this.KeySelector2 = keySelector2;
        }

        public void Add(TValue value)
        {
            TKey1 key = this.KeySelector1(value);
            this.m_index1.Add(key, value);
            try
            {
                this.m_index2.Add(this.KeySelector2(value), value);
            }
            catch
            {
                this.m_index1.Remove(key);
                throw;
            }
        }

        public bool ContainsKey(TKey1 key1) => 
            this.m_index1.ContainsKey(key1);

        public bool ContainsKey(TKey2 key2) => 
            this.m_index2.ContainsKey(key2);

        public Dictionary<TKey1, TValue>.ValueCollection.Enumerator GetEnumerator() => 
            this.m_index1.Values.GetEnumerator();

        public bool Remove(TKey1 key1)
        {
            TValue local;
            return (this.m_index1.TryGetValue(key1, out local) && (this.m_index2.Remove(this.KeySelector2(local)) && this.m_index1.Remove(key1)));
        }

        public bool Remove(TKey2 key2)
        {
            TValue local;
            return (this.m_index2.TryGetValue(key2, out local) && (this.m_index1.Remove(this.KeySelector1(local)) && this.m_index2.Remove(key2)));
        }

        public bool Remove(TKey1 key1, TKey2 key2) => 
            (this.m_index1.Remove(key1) && this.m_index2.Remove(key2));

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryGetValue(TKey1 key1, out TValue result) => 
            this.m_index1.TryGetValue(key1, out result);

        public bool TryGetValue(TKey2 key2, out TValue result) => 
            this.m_index2.TryGetValue(key2, out result);

        public bool TryRemove(TKey1 key1, out TValue removedValue) => 
            (this.m_index1.TryGetValue(key1, out removedValue) && (this.m_index2.Remove(this.KeySelector2(removedValue)) && this.m_index1.Remove(key1)));

        public bool TryRemove(TKey2 key2, out TValue removedValue) => 
            (this.m_index2.TryGetValue(key2, out removedValue) && (this.m_index1.Remove(this.KeySelector1(removedValue)) && this.m_index2.Remove(key2)));

        public TValue this[TKey1 key] =>
            this.m_index1[key];

        public TValue this[TKey2 key] =>
            this.m_index2[key];

        public int Count =>
            this.m_index1.Count;
    }
}

