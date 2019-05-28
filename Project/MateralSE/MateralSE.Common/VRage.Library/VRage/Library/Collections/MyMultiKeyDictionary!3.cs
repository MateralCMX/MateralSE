namespace VRage.Library.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class MyMultiKeyDictionary<TKey1, TKey2, TValue> : IEnumerable<MyMultiKeyDictionary<TKey1, TKey2, TValue>.Triple>, IEnumerable
    {
        private Dictionary<TKey1, Triple<TKey1, TKey2, TValue>> m_index1;
        private Dictionary<TKey2, Triple<TKey1, TKey2, TValue>> m_index2;

        public MyMultiKeyDictionary(int capacity = 0, EqualityComparer<TKey1> keyComparer1 = null, EqualityComparer<TKey2> keyComparer2 = null)
        {
            this.m_index1 = new Dictionary<TKey1, Triple<TKey1, TKey2, TValue>>();
            this.m_index2 = new Dictionary<TKey2, Triple<TKey1, TKey2, TValue>>();
            this.m_index1 = new Dictionary<TKey1, Triple<TKey1, TKey2, TValue>>(capacity, keyComparer1);
            this.m_index2 = new Dictionary<TKey2, Triple<TKey1, TKey2, TValue>>(capacity, keyComparer2);
        }

        public void Add(TKey1 key1, TKey2 key2, TValue value)
        {
            Triple<TKey1, TKey2, TValue> triple = new Triple<TKey1, TKey2, TValue>(key1, key2, value);
            this.m_index1.Add(key1, triple);
            try
            {
                this.m_index2.Add(key2, triple);
            }
            catch
            {
                this.m_index1.Remove(key1);
                throw;
            }
        }

        public bool ContainsKey(TKey1 key1) => 
            this.m_index1.ContainsKey(key1);

        public bool ContainsKey(TKey2 key2) => 
            this.m_index2.ContainsKey(key2);

        private Dictionary<TKey1, Triple<TKey1, TKey2, TValue>>.ValueCollection.Enumerator GetEnumerator() => 
            this.m_index1.Values.GetEnumerator();

        public bool Remove(TKey1 key1)
        {
            Triple<TKey1, TKey2, TValue> triple;
            return (this.m_index1.TryGetValue(key1, out triple) && (this.m_index2.Remove(triple.Key2) && this.m_index1.Remove(key1)));
        }

        public bool Remove(TKey2 key2)
        {
            Triple<TKey1, TKey2, TValue> triple;
            return (this.m_index2.TryGetValue(key2, out triple) && (this.m_index1.Remove(triple.Key1) && this.m_index2.Remove(key2)));
        }

        public bool Remove(TKey1 key1, TKey2 key2) => 
            (this.m_index1.Remove(key1) && this.m_index2.Remove(key2));

        IEnumerator<Triple<TKey1, TKey2, TValue>> IEnumerable<Triple<TKey1, TKey2, TValue>>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryGetValue(TKey1 key1, out Triple<TKey1, TKey2, TValue> result) => 
            this.m_index1.TryGetValue(key1, out result);

        public bool TryGetValue(TKey1 key1, out TValue result)
        {
            Triple<TKey1, TKey2, TValue> triple;
            bool flag = this.m_index1.TryGetValue(key1, out triple);
            result = triple.Value;
            return flag;
        }

        public bool TryGetValue(TKey2 key2, out Triple<TKey1, TKey2, TValue> result) => 
            this.m_index2.TryGetValue(key2, out result);

        public bool TryGetValue(TKey2 key2, out TValue result)
        {
            Triple<TKey1, TKey2, TValue> triple;
            bool flag = this.m_index2.TryGetValue(key2, out triple);
            result = triple.Value;
            return flag;
        }

        public bool TryRemove(TKey1 key1, out Triple<TKey1, TKey2, TValue> removedValue) => 
            (this.m_index1.TryGetValue(key1, out removedValue) && (this.m_index2.Remove(removedValue.Key2) && this.m_index1.Remove(key1)));

        public bool TryRemove(TKey1 key1, out TValue removedValue)
        {
            Triple<TKey1, TKey2, TValue> triple;
            int num1;
            if (!this.m_index1.TryGetValue(key1, out triple) || !this.m_index2.Remove(triple.Key2))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) this.m_index1.Remove(key1);
            }
            bool flag = (bool) num1;
            removedValue = triple.Value;
            return flag;
        }

        public bool TryRemove(TKey2 key2, out Triple<TKey1, TKey2, TValue> removedValue) => 
            (this.m_index2.TryGetValue(key2, out removedValue) && (this.m_index1.Remove(removedValue.Key1) && this.m_index2.Remove(key2)));

        public bool TryRemove(TKey2 key2, out TValue removedValue)
        {
            Triple<TKey1, TKey2, TValue> triple;
            int num1;
            if (!this.m_index2.TryGetValue(key2, out triple) || !this.m_index1.Remove(triple.Key1))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) this.m_index2.Remove(key2);
            }
            bool flag = (bool) num1;
            removedValue = triple.Value;
            return flag;
        }

        public TValue this[TKey1 key] =>
            this.m_index1[key].Value;

        public TValue this[TKey2 key] =>
            this.m_index2[key].Value;

        public int Count =>
            this.m_index1.Count;

        [StructLayout(LayoutKind.Sequential)]
        public struct Triple
        {
            public TKey1 Key1;
            public TKey2 Key2;
            public TValue Value;
            public Triple(TKey1 key1, TKey2 key2, TValue value)
            {
                this.Key1 = key1;
                this.Key2 = key2;
                this.Value = value;
            }
        }
    }
}

