namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Library.Collections;

    public class MyConcurrentSortedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        private SortedDictionary<TKey, TValue> m_dictionary;
        private FastResourceLock m_lock;

        public MyConcurrentSortedDictionary(IComparer<TKey> comparer = null)
        {
            this.m_lock = new FastResourceLock();
            this.m_dictionary = new SortedDictionary<TKey, TValue>(comparer);
        }

        public void Add(TKey key, TValue value)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_dictionary.Add(key, value);
            }
        }

        public TValue ChangeKey(TKey oldKey, TKey newKey)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                TValue local = this.m_dictionary[oldKey];
                this.m_dictionary.Remove(oldKey);
                this.m_dictionary[newKey] = local;
                return local;
            }
        }

        public void Clear()
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_dictionary.Clear();
            }
        }

        public bool ContainsKey(TKey key)
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                return this.m_dictionary.ContainsKey(key);
            }
        }

        public bool ContainsValue(TValue value)
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                return this.m_dictionary.ContainsValue(value);
            }
        }

        public ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, KeyValuePair<TKey, TValue>, SortedDictionary<TKey, TValue>.Enumerator> GetEnumerator() => 
            ConcurrentEnumerator.Create<FastResourceLockExtensions.MySharedLock, KeyValuePair<TKey, TValue>, SortedDictionary<TKey, TValue>.Enumerator>(this.m_lock.AcquireSharedUsing(), this.m_dictionary.GetEnumerator());

        public void GetValues(List<TValue> result)
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                foreach (TValue local in this.m_dictionary.Values)
                {
                    result.Add(local);
                }
            }
        }

        public bool Remove(TKey key)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                return this.m_dictionary.Remove(key);
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryAdd(TKey key, TValue value)
        {
            bool flag;
            using (this.m_lock.AcquireExclusiveUsing())
            {
                if (this.m_dictionary.ContainsKey(key))
                {
                    flag = false;
                }
                else
                {
                    this.m_dictionary.Add(key, value);
                    flag = true;
                }
            }
            return flag;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                return this.m_dictionary.TryGetValue(key, out value);
            }
        }

        public int Count
        {
            get
            {
                using (this.m_lock.AcquireSharedUsing())
                {
                    return this.m_dictionary.Count;
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                using (this.m_lock.AcquireSharedUsing())
                {
                    return this.m_dictionary[key];
                }
            }
            set
            {
                using (this.m_lock.AcquireExclusiveUsing())
                {
                    this.m_dictionary[key] = value;
                }
            }
        }

        public ConcurrentEnumerable<FastResourceLockExtensions.MySharedLock, TKey, SortedDictionary<TKey, TValue>.KeyCollection> Keys =>
            ConcurrentEnumerable.Create<FastResourceLockExtensions.MySharedLock, TKey, SortedDictionary<TKey, TValue>.KeyCollection>(this.m_lock.AcquireSharedUsing(), this.m_dictionary.Keys);

        public ConcurrentEnumerable<FastResourceLockExtensions.MySharedLock, TValue, SortedDictionary<TKey, TValue>.ValueCollection> Values =>
            ConcurrentEnumerable.Create<FastResourceLockExtensions.MySharedLock, TValue, SortedDictionary<TKey, TValue>.ValueCollection>(this.m_lock.AcquireSharedUsing(), this.m_dictionary.Values);
    }
}

