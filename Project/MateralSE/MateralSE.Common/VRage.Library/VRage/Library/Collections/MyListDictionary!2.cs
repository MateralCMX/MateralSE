namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class MyListDictionary<TKey, TValue> : MyCollectionDictionary<TKey, List<TValue>, TValue>
    {
        public MyListDictionary()
        {
        }

        public MyListDictionary(IEqualityComparer<TKey> keyComparer = null) : base(keyComparer)
        {
        }
    }
}

