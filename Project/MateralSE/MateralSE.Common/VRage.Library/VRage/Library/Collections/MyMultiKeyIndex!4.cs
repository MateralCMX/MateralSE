namespace VRage.Library.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class MyMultiKeyIndex<TKey1, TKey2, TKey3, TValue> : IEnumerable<TValue>, IEnumerable
    {
        private Dictionary<TKey1, TValue> m_index1;
        private Dictionary<TKey2, TValue> m_index2;
        private Dictionary<TKey3, TValue> m_index3;
        public readonly Func<TValue, TKey1> KeySelector1;
        public readonly Func<TValue, TKey2> KeySelector2;
        public readonly Func<TValue, TKey3> KeySelector3;

        public MyMultiKeyIndex(Func<TValue, TKey1> keySelector1, Func<TValue, TKey2> keySelector2, Func<TValue, TKey3> keySelector3, int capacity = 0, EqualityComparer<TKey1> keyComparer1 = null, EqualityComparer<TKey2> keyComparer2 = null, EqualityComparer<TKey3> keyComparer3 = null)
        {
            this.m_index1 = new Dictionary<TKey1, TValue>();
            this.m_index2 = new Dictionary<TKey2, TValue>();
            this.m_index3 = new Dictionary<TKey3, TValue>();
            this.m_index1 = new Dictionary<TKey1, TValue>(capacity, keyComparer1);
            this.m_index2 = new Dictionary<TKey2, TValue>(capacity, keyComparer2);
            this.m_index3 = new Dictionary<TKey3, TValue>(capacity, keyComparer3);
            this.KeySelector1 = keySelector1;
            this.KeySelector2 = keySelector2;
            this.KeySelector3 = keySelector3;
        }

        public void Add(TValue value)
        {
            TKey1 key = this.KeySelector1(value);
            this.m_index1.Add(key, value);
            try
            {
                TKey2 local2 = this.KeySelector2(value);
                this.m_index2.Add(local2, value);
                try
                {
                    this.m_index3.Add(this.KeySelector3(value), value);
                }
                catch
                {
                    this.m_index2.Remove(local2);
                    throw;
                }
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

        public bool ContainsKey(TKey3 key3) => 
            this.m_index3.ContainsKey(key3);

        public Dictionary<TKey1, TValue>.ValueCollection.Enumerator GetEnumerator() => 
            this.m_index1.Values.GetEnumerator();

        public bool Remove(TKey1 key1)
        {
            TValue local;
            return (this.m_index1.TryGetValue(key1, out local) && (this.m_index3.Remove(this.KeySelector3(local)) && (this.m_index2.Remove(this.KeySelector2(local)) && this.m_index1.Remove(key1))));
        }

        public bool Remove(TKey2 key2)
        {
            TValue local;
            return (this.m_index2.TryGetValue(key2, out local) && (this.m_index3.Remove(this.KeySelector3(local)) && (this.m_index1.Remove(this.KeySelector1(local)) && this.m_index2.Remove(key2))));
        }

        public bool Remove(TKey3 key3)
        {
            TValue local;
            return (this.m_index3.TryGetValue(key3, out local) && (this.m_index1.Remove(this.KeySelector1(local)) && (this.m_index2.Remove(this.KeySelector2(local)) && this.m_index3.Remove(key3))));
        }

        public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3) => 
            (this.m_index1.Remove(key1) && (this.m_index2.Remove(key2) && this.m_index3.Remove(key3)));

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryGetValue(TKey1 key1, out TValue result) => 
            this.m_index1.TryGetValue(key1, out result);

        public bool TryGetValue(TKey2 key2, out TValue result) => 
            this.m_index2.TryGetValue(key2, out result);

        public bool TryGetValue(TKey3 key3, out TValue result) => 
            this.m_index3.TryGetValue(key3, out result);

        public bool TryRemove(TKey1 key1, out TValue removedValue) => 
            (this.m_index1.TryGetValue(key1, out removedValue) && (this.m_index3.Remove(this.KeySelector3(removedValue)) && (this.m_index2.Remove(this.KeySelector2(removedValue)) && this.m_index1.Remove(key1))));

        public bool TryRemove(TKey2 key2, out TValue removedValue) => 
            (this.m_index2.TryGetValue(key2, out removedValue) && (this.m_index3.Remove(this.KeySelector3(removedValue)) && (this.m_index1.Remove(this.KeySelector1(removedValue)) && this.m_index2.Remove(key2))));

        public bool TryRemove(TKey3 key3, out TValue removedValue) => 
            (this.m_index3.TryGetValue(key3, out removedValue) && (this.m_index1.Remove(this.KeySelector1(removedValue)) && (this.m_index2.Remove(this.KeySelector2(removedValue)) && this.m_index3.Remove(key3))));

        public TValue this[TKey1 key] =>
            this.m_index1[key];

        public TValue this[TKey2 key] =>
            this.m_index2[key];

        public TValue this[TKey3 key] =>
            this.m_index3[key];

        public int Count =>
            this.m_index1.Count;
    }
}

