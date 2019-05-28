namespace VRage.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class MyConcurrentPool<T> : IConcurrentPool where T: new()
    {
        private readonly int m_expectedAllocations;
        private readonly Action<T> m_clear;
        private readonly Stack<T> m_instances;
        private readonly Func<T> m_activator;

        public MyConcurrentPool(int defaultCapacity = 0, Action<T> clear = null, int expectedAllocations = 0x2710, Func<T> activator = null)
        {
            this.m_clear = clear;
            this.m_expectedAllocations = expectedAllocations;
            this.m_instances = new Stack<T>(defaultCapacity);
            this.m_activator = activator ?? ExpressionExtension.CreateActivator<T>();
            if (defaultCapacity > 0)
            {
                this.Allocated = defaultCapacity;
                for (int i = 0; i < defaultCapacity; i++)
                {
                    this.m_instances.Push(this.m_activator());
                }
            }
        }

        public void Clean()
        {
            Stack<T> instances = this.m_instances;
            lock (instances)
            {
                this.m_instances.Clear();
            }
        }

        public T Get()
        {
            Stack<T> instances = this.m_instances;
            lock (instances)
            {
                if (this.m_instances.Count > 0)
                {
                    return this.m_instances.Pop();
                }
            }
            return this.m_activator();
        }

        public void Return(T instance)
        {
            if (this.m_clear != null)
            {
                this.m_clear(instance);
            }
            Stack<T> instances = this.m_instances;
            lock (instances)
            {
                this.m_instances.Push(instance);
            }
        }

        object IConcurrentPool.Get() => 
            this.Get();

        void IConcurrentPool.Return(object obj)
        {
            this.Return((T) obj);
        }

        public int Allocated { get; set; }

        public int Count
        {
            get
            {
                Stack<T> instances = this.m_instances;
                lock (instances)
                {
                    return this.m_instances.Count;
                }
            }
        }
    }
}

