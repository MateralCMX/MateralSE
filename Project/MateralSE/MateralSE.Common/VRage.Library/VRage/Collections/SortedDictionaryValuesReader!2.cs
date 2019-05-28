namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SortedDictionaryValuesReader<K, V> : IEnumerable<V>, IEnumerable
    {
        private readonly SortedDictionary<K, V> m_collection;
        public SortedDictionaryValuesReader(SortedDictionary<K, V> collection)
        {
            this.m_collection = collection;
        }

        public int Count =>
            this.m_collection.Count;
        public V this[K key] =>
            this.m_collection[key];
        public bool TryGetValue(K key, out V result) => 
            this.m_collection.TryGetValue(key, out result);

        public SortedDictionary<K, V>.ValueCollection.Enumerator GetEnumerator() => 
            this.m_collection.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator<V> IEnumerable<V>.GetEnumerator() => 
            this.GetEnumerator();

        public static implicit operator SortedDictionaryValuesReader<K, V>(SortedDictionary<K, V> v) => 
            new SortedDictionaryValuesReader<K, V>(v);
    }
}

