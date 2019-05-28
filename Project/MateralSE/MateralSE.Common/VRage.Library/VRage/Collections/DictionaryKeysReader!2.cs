namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct DictionaryKeysReader<K, V> : IEnumerable<K>, IEnumerable
    {
        private readonly Dictionary<K, V> m_collection;
        public int Count =>
            this.m_collection.Count;
        public DictionaryKeysReader(Dictionary<K, V> collection)
        {
            this.m_collection = collection;
        }

        public Dictionary<K, V>.KeyCollection.Enumerator GetEnumerator() => 
            this.m_collection.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator<K> IEnumerable<K>.GetEnumerator() => 
            this.GetEnumerator();
    }
}

