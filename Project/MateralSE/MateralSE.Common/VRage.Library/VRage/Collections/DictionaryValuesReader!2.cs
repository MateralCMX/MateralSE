namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct DictionaryValuesReader<K, V> : IEnumerable<V>, IEnumerable
    {
        private readonly Dictionary<K, V> m_collection;
        public DictionaryValuesReader(Dictionary<K, V> collection)
        {
            this.m_collection = collection;
        }

        public int Count =>
            this.m_collection.Count;
        public V this[K key] =>
            this.m_collection[key];
        public bool TryGetValue(K key, out V result) => 
            this.m_collection.TryGetValue(key, out result);

        public Dictionary<K, V>.ValueCollection.Enumerator GetEnumerator() => 
            this.m_collection.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator<V> IEnumerable<V>.GetEnumerator() => 
            this.GetEnumerator();

        public static implicit operator DictionaryValuesReader<K, V>(Dictionary<K, V> v) => 
            new DictionaryValuesReader<K, V>(v);
    }
}

