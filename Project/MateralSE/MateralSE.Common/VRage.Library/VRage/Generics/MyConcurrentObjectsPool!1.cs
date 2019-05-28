namespace VRage.Generics
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;

    public class MyConcurrentObjectsPool<T> where T: class, new()
    {
        private FastResourceLock m_lock;
        private MyQueue<T> m_unused;
        private HashSet<T> m_active;
        private HashSet<T> m_marked;
        private int m_baseCapacity;

        private MyConcurrentObjectsPool()
        {
            this.m_lock = new FastResourceLock();
        }

        public MyConcurrentObjectsPool(int baseCapacity)
        {
            this.m_lock = new FastResourceLock();
            this.m_baseCapacity = baseCapacity;
            this.m_unused = new MyQueue<T>(this.m_baseCapacity);
            this.m_active = new HashSet<T>();
            this.m_marked = new HashSet<T>();
            for (int i = 0; i < this.m_baseCapacity; i++)
            {
                this.m_unused.Enqueue(Activator.CreateInstance<T>());
            }
        }

        public T Allocate(bool nullAllowed = false)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                T item = default(T);
                if (this.m_unused.Count > 0)
                {
                    item = this.m_unused.Dequeue();
                    this.m_active.Add(item);
                }
                return item;
            }
        }

        public bool AllocateOrCreate(out T item)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                bool flag = this.m_unused.Count == 0;
                item = !flag ? this.m_unused.Dequeue() : Activator.CreateInstance<T>();
                this.m_active.Add(item);
                return flag;
            }
        }

        public void ApplyActionOnAllActives(Action<T> action)
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                foreach (T local in this.m_active)
                {
                    action(local);
                }
            }
        }

        public void Deallocate(T item)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_active.Remove(item);
                this.m_unused.Enqueue(item);
            }
        }

        public void DeallocateAll()
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                foreach (T local in this.m_active)
                {
                    this.m_unused.Enqueue(local);
                }
                this.m_active.Clear();
                this.m_marked.Clear();
            }
        }

        public void DeallocateAllMarked()
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                foreach (T local in this.m_marked)
                {
                    this.m_active.Remove(local);
                    this.m_unused.Enqueue(local);
                }
                this.m_marked.Clear();
            }
        }

        public void MarkForDeallocate(T item)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_marked.Add(item);
            }
        }

        public int ActiveCount
        {
            get
            {
                using (this.m_lock.AcquireSharedUsing())
                {
                    return this.m_active.Count;
                }
            }
        }

        public int BaseCapacity
        {
            get
            {
                using (this.m_lock.AcquireSharedUsing())
                {
                    this.m_lock.AcquireShared();
                    return this.m_baseCapacity;
                }
            }
        }

        public int Capacity
        {
            get
            {
                using (this.m_lock.AcquireSharedUsing())
                {
                    this.m_lock.AcquireShared();
                    return (this.m_unused.Count + this.m_active.Count);
                }
            }
        }
    }
}

