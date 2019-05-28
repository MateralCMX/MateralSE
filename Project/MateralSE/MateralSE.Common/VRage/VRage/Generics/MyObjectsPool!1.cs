namespace VRage.Generics
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Library.Threading;

    public class MyObjectsPool<T> where T: class, new()
    {
        private MyConcurrentQueue<T> m_unused;
        private HashSet<T> m_active;
        private HashSet<T> m_marked;
        private SpinLockRef m_activeLock;
        private Func<T> m_activator;
        private int m_baseCapacity;

        public MyObjectsPool(int baseCapacity, Func<T> activator = null)
        {
            this.m_activeLock = new SpinLockRef();
            this.m_activator = activator ?? ExpressionExtension.CreateActivator<T>();
            this.m_baseCapacity = baseCapacity;
            this.m_unused = new MyConcurrentQueue<T>(this.m_baseCapacity);
            this.m_active = new HashSet<T>();
            this.m_marked = new HashSet<T>();
            for (int i = 0; i < this.m_baseCapacity; i++)
            {
                this.m_unused.Enqueue(this.m_activator());
            }
        }

        public T Allocate(bool nullAllowed = false)
        {
            T item = default(T);
            using (this.m_activeLock.Acquire())
            {
                if (this.m_unused.Count > 0)
                {
                    item = this.m_unused.Dequeue();
                    this.m_active.Add(item);
                }
            }
            return item;
        }

        public bool AllocateOrCreate(out T item)
        {
            bool flag = false;
            using (this.m_activeLock.Acquire())
            {
                flag = this.m_unused.Count == 0;
                item = !flag ? this.m_unused.Dequeue() : this.m_activator();
                this.m_active.Add(item);
            }
            return flag;
        }

        public void Deallocate(T item)
        {
            using (this.m_activeLock.Acquire())
            {
                this.m_active.Remove(item);
                this.m_unused.Enqueue(item);
            }
        }

        public void DeallocateAll()
        {
            using (this.m_activeLock.Acquire())
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
            using (this.m_activeLock.Acquire())
            {
                foreach (T local in this.m_marked)
                {
                    this.m_active.Remove(local);
                    this.m_unused.Enqueue(local);
                }
                this.m_marked.Clear();
            }
        }

        public void MarkAllActiveForDeallocate()
        {
            using (this.m_activeLock.Acquire())
            {
                this.m_marked.UnionWith(this.m_active);
            }
        }

        public void MarkForDeallocate(T item)
        {
            using (this.m_activeLock.Acquire())
            {
                this.m_marked.Add(item);
            }
        }

        public SpinLockRef ActiveLock =>
            this.m_activeLock;

        public HashSetReader<T> ActiveWithoutLock =>
            new HashSetReader<T>(this.m_active);

        public HashSetReader<T> Active
        {
            get
            {
                using (this.m_activeLock.Acquire())
                {
                    return new HashSetReader<T>(this.m_active);
                }
            }
        }

        public int ActiveCount
        {
            get
            {
                using (this.m_activeLock.Acquire())
                {
                    return this.m_active.Count;
                }
            }
        }

        public int BaseCapacity =>
            this.m_baseCapacity;

        public int Capacity
        {
            get
            {
                using (this.m_activeLock.Acquire())
                {
                    return (this.m_unused.Count + this.m_active.Count);
                }
            }
        }
    }
}

