namespace VRage.Library.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class MyMultiKeyDictionary<TKey1, TKey2, TKey3, TValue> : IEnumerable<MyMultiKeyDictionary<TKey1, TKey2, TKey3, TValue>.Quadruple>, IEnumerable
    {
        private Dictionary<TKey1, Quadruple<TKey1, TKey2, TKey3, TValue>> m_index1;
        private Dictionary<TKey2, Quadruple<TKey1, TKey2, TKey3, TValue>> m_index2;
        private Dictionary<TKey3, Quadruple<TKey1, TKey2, TKey3, TValue>> m_index3;

        public MyMultiKeyDictionary(int capacity = 0, EqualityComparer<TKey1> keyComparer1 = null, EqualityComparer<TKey2> keyComparer2 = null, EqualityComparer<TKey3> keyComparer3 = null)
        {
            this.m_index1 = new Dictionary<TKey1, Quadruple<TKey1, TKey2, TKey3, TValue>>();
            this.m_index2 = new Dictionary<TKey2, Quadruple<TKey1, TKey2, TKey3, TValue>>();
            this.m_index3 = new Dictionary<TKey3, Quadruple<TKey1, TKey2, TKey3, TValue>>();
            this.m_index1 = new Dictionary<TKey1, Quadruple<TKey1, TKey2, TKey3, TValue>>(capacity, keyComparer1);
            this.m_index2 = new Dictionary<TKey2, Quadruple<TKey1, TKey2, TKey3, TValue>>(capacity, keyComparer2);
            this.m_index3 = new Dictionary<TKey3, Quadruple<TKey1, TKey2, TKey3, TValue>>(capacity, keyComparer3);
        }

        public void Add(TKey1 key1, TKey2 key2, TKey3 key3, TValue value)
        {
            Quadruple<TKey1, TKey2, TKey3, TValue> quadruple = new Quadruple<TKey1, TKey2, TKey3, TValue>(key1, key2, key3, value);
            this.m_index1.Add(key1, quadruple);
            try
            {
                this.m_index2.Add(key2, quadruple);
                try
                {
                    this.m_index3.Add(key3, quadruple);
                }
                catch
                {
                    this.m_index2.Remove(key2);
                    throw;
                }
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

        public bool ContainsKey(TKey3 key3) => 
            this.m_index3.ContainsKey(key3);

        private Dictionary<TKey1, Quadruple<TKey1, TKey2, TKey3, TValue>>.ValueCollection.Enumerator GetEnumerator() => 
            this.m_index1.Values.GetEnumerator();

        public bool Remove(TKey1 key1)
        {
            Quadruple<TKey1, TKey2, TKey3, TValue> quadruple;
            return (this.m_index1.TryGetValue(key1, out quadruple) && (this.m_index3.Remove(quadruple.Key3) && (this.m_index2.Remove(quadruple.Key2) && this.m_index1.Remove(key1))));
        }

        public bool Remove(TKey2 key2)
        {
            Quadruple<TKey1, TKey2, TKey3, TValue> quadruple;
            return (this.m_index2.TryGetValue(key2, out quadruple) && (this.m_index3.Remove(quadruple.Key3) && (this.m_index1.Remove(quadruple.Key1) && this.m_index2.Remove(key2))));
        }

        public bool Remove(TKey3 key3)
        {
            Quadruple<TKey1, TKey2, TKey3, TValue> quadruple;
            return (this.m_index3.TryGetValue(key3, out quadruple) && (this.m_index1.Remove(quadruple.Key1) && (this.m_index2.Remove(quadruple.Key2) && this.m_index3.Remove(key3))));
        }

        public bool Remove(TKey1 key1, TKey2 key2, TKey3 key3) => 
            (this.m_index1.Remove(key1) && (this.m_index2.Remove(key2) && this.m_index3.Remove(key3)));

        IEnumerator<Quadruple<TKey1, TKey2, TKey3, TValue>> IEnumerable<Quadruple<TKey1, TKey2, TKey3, TValue>>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryGetValue(TKey1 key1, out Quadruple<TKey1, TKey2, TKey3, TValue> result) => 
            this.m_index1.TryGetValue(key1, out result);

        public bool TryGetValue(TKey1 key1, out TValue result)
        {
            Quadruple<TKey1, TKey2, TKey3, TValue> quadruple;
            bool flag = this.m_index1.TryGetValue(key1, out quadruple);
            result = quadruple.Value;
            return flag;
        }

        public bool TryGetValue(TKey2 key2, out Quadruple<TKey1, TKey2, TKey3, TValue> result) => 
            this.m_index2.TryGetValue(key2, out result);

        public bool TryGetValue(TKey2 key2, out TValue result)
        {
            Quadruple<TKey1, TKey2, TKey3, TValue> quadruple;
            bool flag = this.m_index2.TryGetValue(key2, out quadruple);
            result = quadruple.Value;
            return flag;
        }

        public bool TryGetValue(TKey3 key3, out Quadruple<TKey1, TKey2, TKey3, TValue> result) => 
            this.m_index3.TryGetValue(key3, out result);

        public bool TryGetValue(TKey3 key3, out TValue result)
        {
            Quadruple<TKey1, TKey2, TKey3, TValue> quadruple;
            bool flag = this.m_index3.TryGetValue(key3, out quadruple);
            result = quadruple.Value;
            return flag;
        }

        public bool TryRemove(TKey1 key1, out TValue removedValue)
        {
            Quadruple<TKey1, TKey2, TKey3, TValue> quadruple;
            int num1;
            if ((!this.m_index1.TryGetValue(key1, out quadruple) || !this.m_index3.Remove(quadruple.Key3)) || !this.m_index2.Remove(quadruple.Key2))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) this.m_index1.Remove(key1);
            }
            bool flag = (bool) num1;
            removedValue = quadruple.Value;
            return flag;
        }

        public bool TryRemove(TKey1 key1, out Quadruple<TKey1, TKey2, TKey3, TValue> removedValue) => 
            (this.m_index1.TryGetValue(key1, out removedValue) && (this.m_index3.Remove(removedValue.Key3) && (this.m_index2.Remove(removedValue.Key2) && this.m_index1.Remove(key1))));

        public bool TryRemove(TKey2 key2, out TValue removedValue)
        {
            Quadruple<TKey1, TKey2, TKey3, TValue> quadruple;
            int num1;
            if ((!this.m_index2.TryGetValue(key2, out quadruple) || !this.m_index3.Remove(quadruple.Key3)) || !this.m_index1.Remove(quadruple.Key1))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) this.m_index2.Remove(key2);
            }
            bool flag = (bool) num1;
            removedValue = quadruple.Value;
            return flag;
        }

        public bool TryRemove(TKey2 key2, out Quadruple<TKey1, TKey2, TKey3, TValue> removedValue) => 
            (this.m_index2.TryGetValue(key2, out removedValue) && (this.m_index3.Remove(removedValue.Key3) && (this.m_index1.Remove(removedValue.Key1) && this.m_index2.Remove(key2))));

        public bool TryRemove(TKey3 key3, out TValue removedValue)
        {
            Quadruple<TKey1, TKey2, TKey3, TValue> quadruple;
            int num1;
            if ((!this.m_index3.TryGetValue(key3, out quadruple) || !this.m_index1.Remove(quadruple.Key1)) || !this.m_index2.Remove(quadruple.Key2))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) this.m_index3.Remove(key3);
            }
            bool flag = (bool) num1;
            removedValue = quadruple.Value;
            return flag;
        }

        public bool TryRemove(TKey3 key3, out Quadruple<TKey1, TKey2, TKey3, TValue> removedValue) => 
            (this.m_index3.TryGetValue(key3, out removedValue) && (this.m_index1.Remove(removedValue.Key1) && (this.m_index2.Remove(removedValue.Key2) && this.m_index3.Remove(key3))));

        public TValue this[TKey1 key] =>
            this.m_index1[key].Value;

        public TValue this[TKey2 key] =>
            this.m_index2[key].Value;

        public TValue this[TKey3 key] =>
            this.m_index3[key].Value;

        public int Count =>
            this.m_index1.Count;

        [StructLayout(LayoutKind.Sequential)]
        public struct Quadruple
        {
            public TKey1 Key1;
            public TKey2 Key2;
            public TKey3 Key3;
            public TValue Value;
            public Quadruple(TKey1 key1, TKey2 key2, TKey3 key3, TValue value)
            {
                this.Key1 = key1;
                this.Key2 = key2;
                this.Key3 = key3;
                this.Value = value;
            }
        }
    }
}

