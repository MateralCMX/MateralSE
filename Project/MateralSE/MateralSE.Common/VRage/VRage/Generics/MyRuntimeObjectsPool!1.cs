namespace VRage.Generics
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;

    public class MyRuntimeObjectsPool<TPool> where TPool: class
    {
        private readonly Queue<TPool> m_unused;
        private readonly Func<TPool> m_constructor;
        private readonly HashSet<TPool> m_active;
        private readonly HashSet<TPool> m_marked;
        private readonly int m_baseCapacity;

        public MyRuntimeObjectsPool(int baseCapacity, Func<TPool> constructor)
        {
            this.m_constructor = constructor;
            this.m_baseCapacity = baseCapacity;
            this.m_unused = new Queue<TPool>(this.m_baseCapacity);
            this.m_active = new HashSet<TPool>();
            this.m_marked = new HashSet<TPool>();
            for (int i = 0; i < this.m_baseCapacity; i++)
            {
                this.m_unused.Enqueue(this.m_constructor());
            }
        }

        public MyRuntimeObjectsPool(int baseCapacity, Type type) : this(baseCapacity, ExpressionExtension.CreateActivator<TPool>(type))
        {
        }

        public TPool Allocate(bool nullAllowed = false)
        {
            TPool item = default(TPool);
            if (this.m_unused.Count > 0)
            {
                item = this.m_unused.Dequeue();
                this.m_active.Add(item);
            }
            return item;
        }

        public bool AllocateOrCreate(out TPool item)
        {
            bool flag1 = this.m_unused.Count == 0;
            item = !flag1 ? this.m_unused.Dequeue() : this.m_constructor();
            this.m_active.Add(item);
            return flag1;
        }

        public void Deallocate(TPool item)
        {
            this.m_active.Remove(item);
            this.m_unused.Enqueue(item);
        }

        public void DeallocateAll()
        {
            foreach (TPool local in this.m_active)
            {
                this.m_unused.Enqueue(local);
            }
            this.m_active.Clear();
            this.m_marked.Clear();
        }

        public void DeallocateAllMarked()
        {
            foreach (TPool local in this.m_marked)
            {
                this.Deallocate(local);
            }
            this.m_marked.Clear();
        }

        public void MarkAllActiveForDeallocate()
        {
            this.m_marked.UnionWith(this.m_active);
        }

        public void MarkForDeallocate(TPool item)
        {
            this.m_marked.Add(item);
        }

        public void TrimToBaseCapacity()
        {
            while ((this.Capacity > this.BaseCapacity) && (this.m_unused.Count > 0))
            {
                this.m_unused.Dequeue();
            }
            this.m_unused.TrimExcess();
            this.m_active.TrimExcess();
            this.m_marked.TrimExcess();
        }

        public QueueReader<TPool> Unused =>
            new QueueReader<TPool>(this.m_unused);

        public HashSetReader<TPool> Active =>
            new HashSetReader<TPool>(this.m_active);

        public int ActiveCount =>
            this.m_active.Count;

        public int BaseCapacity =>
            this.m_baseCapacity;

        public int Capacity =>
            (this.m_unused.Count + this.m_active.Count);
    }
}

