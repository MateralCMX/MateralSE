namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class MyHashSetDictionary<TKey, TValue> : MyCollectionDictionary<TKey, HashSet<TValue>, TValue>
    {
        private readonly IEqualityComparer<TValue> m_valueComparer;

        public MyHashSetDictionary()
        {
        }

        public MyHashSetDictionary(IEqualityComparer<TKey> keyComparer = null, IEqualityComparer<TValue> valueComparer = null) : base(keyComparer)
        {
            this.m_valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }

        protected override HashSet<TValue> CreateCollection() => 
            new HashSet<TValue>(this.m_valueComparer);
    }
}

