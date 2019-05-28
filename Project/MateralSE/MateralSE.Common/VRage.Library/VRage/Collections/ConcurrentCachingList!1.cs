namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Library.Collections;
    using VRage.Library.Threading;

    public class ConcurrentCachingList<T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        private readonly List<T> m_list;
        private readonly List<T> m_toAdd;
        private readonly List<T> m_toRemove;
        private FastResourceLock m_listLock;
        private SpinLockRef m_cacheLock;
        private bool m_dirty;

        public ConcurrentCachingList()
        {
            this.m_list = new List<T>();
            this.m_toAdd = new List<T>();
            this.m_toRemove = new List<T>();
            this.m_listLock = new FastResourceLock();
            this.m_cacheLock = new SpinLockRef();
        }

        public ConcurrentCachingList(int capacity)
        {
            this.m_list = new List<T>();
            this.m_toAdd = new List<T>();
            this.m_toRemove = new List<T>();
            this.m_listLock = new FastResourceLock();
            this.m_cacheLock = new SpinLockRef();
            this.m_list = new List<T>(capacity);
        }

        public void Add(T entity)
        {
            using (this.m_cacheLock.Acquire())
            {
                if (this.m_toRemove.Contains(entity))
                {
                    this.m_toRemove.Remove(entity);
                }
                else
                {
                    this.m_toAdd.Add(entity);
                    this.m_dirty = true;
                }
            }
        }

        public void ApplyAdditions()
        {
            using (this.m_listLock.AcquireExclusiveUsing())
            {
                using (this.m_cacheLock.Acquire())
                {
                    this.m_list.AddList<T>(this.m_toAdd);
                    this.m_toAdd.Clear();
                }
            }
        }

        public void ApplyChanges()
        {
            if (this.m_dirty)
            {
                this.m_dirty = false;
                this.ApplyAdditions();
                this.ApplyRemovals();
            }
        }

        public void ApplyRemovals()
        {
            using (this.m_listLock.AcquireExclusiveUsing())
            {
                using (this.m_cacheLock.Acquire())
                {
                    foreach (T local in this.m_toRemove)
                    {
                        this.m_list.Remove(local);
                    }
                    this.m_toRemove.Clear();
                }
            }
        }

        public void ClearImmediate()
        {
            using (this.m_listLock.AcquireExclusiveUsing())
            {
                using (this.m_cacheLock.Acquire())
                {
                    this.m_toAdd.Clear();
                    this.m_toRemove.Clear();
                    this.m_list.Clear();
                    this.m_dirty = false;
                }
            }
        }

        public void ClearList()
        {
            using (this.m_listLock.AcquireExclusiveUsing())
            {
                this.m_list.Clear();
            }
        }

        [Conditional("DEBUG")]
        public void DebugCheckEmpty()
        {
        }

        public ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, T, List<T>.Enumerator> GetEnumerator() => 
            ConcurrentEnumerator.Create<FastResourceLockExtensions.MySharedLock, T, List<T>.Enumerator>(this.m_listLock.AcquireSharedUsing(), this.m_list.GetEnumerator());

        public void Remove(T entity, bool immediate = false)
        {
            using (this.m_cacheLock.Acquire())
            {
                if (!this.m_toAdd.Remove(entity))
                {
                    this.m_toRemove.Add(entity);
                }
            }
            if (immediate)
            {
                using (this.m_listLock.AcquireExclusiveUsing())
                {
                    using (this.m_cacheLock.Acquire())
                    {
                        this.m_list.Remove(entity);
                        this.m_toRemove.Remove(entity);
                        return;
                    }
                }
            }
            this.m_dirty = true;
        }

        public void RemoveAtImmediately(int index)
        {
            using (this.m_listLock.AcquireExclusiveUsing())
            {
                if ((index >= 0) && (index < this.m_list.Count))
                {
                    this.m_list.RemoveAt(index);
                }
            }
        }

        public void Sort(IComparer<T> comparer)
        {
            using (this.m_listLock.AcquireExclusiveUsing())
            {
                this.m_list.Sort(comparer);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public override string ToString() => 
            $"Count = {this.m_list.Count}; ToAdd = {this.m_toAdd.Count}; ToRemove = {this.m_toRemove.Count}";

        public int Count
        {
            get
            {
                using (this.m_listLock.AcquireSharedUsing())
                {
                    return this.m_list.Count;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                using (this.m_listLock.AcquireSharedUsing())
                {
                    return this.m_list[index];
                }
            }
        }

        public bool IsEmpty =>
            ((this.m_list.Count == 0) && ((this.m_toAdd.Count == 0) && (this.m_toRemove.Count == 0)));
    }
}

