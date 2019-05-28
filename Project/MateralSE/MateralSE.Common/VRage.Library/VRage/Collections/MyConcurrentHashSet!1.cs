namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using VRage.Library.Collections;
    using VRage.Library.Threading;

    public class MyConcurrentHashSet<T> : IEnumerable<T>, IEnumerable
    {
        private HashSet<T> m_set;
        private SpinLockRef m_lock;

        public MyConcurrentHashSet()
        {
            this.m_lock = new SpinLockRef();
            this.m_set = new HashSet<T>();
        }

        public MyConcurrentHashSet(IEqualityComparer<T> comparer)
        {
            this.m_lock = new SpinLockRef();
            this.m_set = new HashSet<T>(comparer);
        }

        public bool Add(T instance)
        {
            using (this.m_lock.Acquire())
            {
                return this.m_set.Add(instance);
            }
        }

        public void Clear()
        {
            using (this.m_lock.Acquire())
            {
                this.m_set.Clear();
            }
        }

        public bool Contains(T value)
        {
            using (this.m_lock.Acquire())
            {
                return this.m_set.Contains(value);
            }
        }

        public ConcurrentEnumerator<SpinLockRef.Token, T, HashSet<T>.Enumerator> GetEnumerator() => 
            ConcurrentEnumerator.Create<SpinLockRef.Token, T, HashSet<T>.Enumerator>(this.m_lock.Acquire(), this.m_set.GetEnumerator());

        public bool Remove(T value)
        {
            using (this.m_lock.Acquire())
            {
                return this.m_set.Remove(value);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public int Count
        {
            get
            {
                using (this.m_lock.Acquire())
                {
                    return this.m_set.Count;
                }
            }
        }
    }
}

