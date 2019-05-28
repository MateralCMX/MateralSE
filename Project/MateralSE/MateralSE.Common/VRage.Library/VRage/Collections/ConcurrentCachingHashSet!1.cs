namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Library.Threading;

    public class ConcurrentCachingHashSet<T> : IEnumerable<T>, IEnumerable
    {
        private readonly HashSet<T> m_hashSet;
        private readonly HashSet<T> m_toAdd;
        private readonly HashSet<T> m_toRemove;
        private readonly SpinLockRef m_setLock;
        private readonly SpinLockRef m_changelistLock;

        public ConcurrentCachingHashSet()
        {
            this.m_hashSet = new HashSet<T>();
            this.m_toAdd = new HashSet<T>();
            this.m_toRemove = new HashSet<T>();
            this.m_setLock = new SpinLockRef();
            this.m_changelistLock = new SpinLockRef();
        }

        public void Add(T item)
        {
            using (this.m_changelistLock.Acquire())
            {
                this.m_toRemove.Remove(item);
                this.m_toAdd.Add(item);
            }
        }

        public void ApplyAdditions()
        {
            using (this.m_setLock.Acquire())
            {
                using (this.m_changelistLock.Acquire())
                {
                    foreach (T local in this.m_toAdd)
                    {
                        this.m_hashSet.Add(local);
                    }
                    this.m_toAdd.Clear();
                }
            }
        }

        public void ApplyChanges()
        {
            this.ApplyAdditions();
            this.ApplyRemovals();
        }

        public void ApplyRemovals()
        {
            using (this.m_setLock.Acquire())
            {
                using (this.m_changelistLock.Acquire())
                {
                    foreach (T local in this.m_toRemove)
                    {
                        this.m_hashSet.Remove(local);
                    }
                    this.m_toRemove.Clear();
                }
            }
        }

        public void Clear()
        {
            using (this.m_setLock.Acquire())
            {
                using (this.m_changelistLock.Acquire())
                {
                    this.m_hashSet.Clear();
                    this.m_toAdd.Clear();
                    this.m_toRemove.Clear();
                }
            }
        }

        public bool Contains(T item)
        {
            using (this.m_setLock.Acquire())
            {
                return this.m_hashSet.Contains(item);
            }
        }

        public HashSet<T>.Enumerator GetEnumerator()
        {
            using (this.m_setLock.Acquire())
            {
                return this.m_hashSet.GetEnumerator();
            }
        }

        public void Remove(T item, bool immediate = false)
        {
            if (immediate)
            {
                using (this.m_setLock.Acquire())
                {
                    using (this.m_changelistLock.Acquire())
                    {
                        this.m_toAdd.Remove(item);
                        this.m_toRemove.Remove(item);
                        this.m_hashSet.Remove(item);
                        return;
                    }
                }
            }
            using (this.m_changelistLock.Acquire())
            {
                this.m_toAdd.Remove(item);
                this.m_toRemove.Add(item);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public override string ToString() => 
            $"Count = {this.m_hashSet.Count}; ToAdd = {this.m_toAdd.Count}; ToRemove = {this.m_toRemove.Count}";

        public int Count
        {
            get
            {
                using (this.m_setLock.Acquire())
                {
                    return this.m_hashSet.Count;
                }
            }
        }
    }
}

