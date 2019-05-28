namespace VRage.Library.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public abstract class MyCollectionDictionary<TKey, TCollection, TValue> : IEnumerable<KeyValuePair<TKey, TCollection>>, IEnumerable where TCollection: ICollection<TValue>, new()
    {
        private readonly Stack<TCollection> m_collectionCache;
        private readonly Dictionary<TKey, TCollection> m_dictionary;

        public MyCollectionDictionary() : this(null)
        {
        }

        public MyCollectionDictionary(IEqualityComparer<TKey> keyComparer = null)
        {
            this.m_collectionCache = new Stack<TCollection>();
            this.m_dictionary = new Dictionary<TKey, TCollection>(keyComparer);
        }

        public void Add(TKey key, TValue value)
        {
            this.GetOrAdd(key).Add(value);
        }

        public void Add(TKey key, IEnumerable<TValue> values)
        {
            TCollection orAdd = this.GetOrAdd(key);
            foreach (TValue local2 in values)
            {
                orAdd.Add(local2);
            }
        }

        public void Add(TKey key, params TValue[] values)
        {
            TCollection orAdd = this.GetOrAdd(key);
            foreach (TValue local2 in values)
            {
                orAdd.Add(local2);
            }
        }

        public void Add(TKey key, TValue first, TValue second)
        {
            TCollection orAdd = this.GetOrAdd(key);
            orAdd.Add(first);
            orAdd.Add(second);
        }

        public void Add(TKey key, TValue first, TValue second, TValue third)
        {
            TCollection orAdd = this.GetOrAdd(key);
            orAdd.Add(first);
            orAdd.Add(second);
            orAdd.Add(third);
        }

        public void Clear()
        {
            foreach (KeyValuePair<TKey, TCollection> pair in this.m_dictionary)
            {
                this.Return(pair.Value);
            }
            this.m_dictionary.Clear();
        }

        protected virtual TCollection CreateCollection() => 
            Activator.CreateInstance<TCollection>();

        private TCollection Get() => 
            ((this.m_collectionCache.Count <= 0) ? this.CreateCollection() : this.m_collectionCache.Pop());

        public IEnumerator<KeyValuePair<TKey, TCollection>> GetEnumerator() => 
            this.m_dictionary.GetEnumerator();

        public TCollection GetOrAdd(TKey key)
        {
            TCollection local;
            if (!this.m_dictionary.TryGetValue(key, out local))
            {
                local = this.Get();
                this.m_dictionary.Add(key, local);
            }
            return local;
        }

        public TCollection GetOrDefault(TKey key) => 
            this.m_dictionary.GetValueOrDefault<TKey, TCollection>(key);

        public bool Remove(TKey key)
        {
            TCollection local;
            if (!this.m_dictionary.TryGetValue(key, out local))
            {
                return false;
            }
            this.m_dictionary.Remove(key);
            this.Return(local);
            return true;
        }

        public bool Remove(TKey key, TValue value)
        {
            TCollection local;
            return (this.m_dictionary.TryGetValue(key, out local) && local.Remove(value));
        }

        private void Return(TCollection list)
        {
            list.Clear();
            this.m_collectionCache.Push(list);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryGet(TKey key, out TCollection list) => 
            this.m_dictionary.TryGetValue(key, out list);

        public TCollection this[TKey key] =>
            this.m_dictionary[key];

        public Dictionary<TKey, TCollection>.ValueCollection Values =>
            this.m_dictionary.Values;

        public Dictionary<TKey, TCollection>.KeyCollection Keys =>
            this.m_dictionary.Keys;

        public int KeyCount =>
            this.m_dictionary.Count;
    }
}

