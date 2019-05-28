namespace VRage.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class MyConcurrentCollectionPool<TCollection, TItem> : IConcurrentPool where TCollection: ICollection<TItem>, new()
    {
        private readonly Stack<TCollection> m_instances;

        public MyConcurrentCollectionPool(int defaultCapacity = 0)
        {
            this.m_instances = new Stack<TCollection>(defaultCapacity);
            if (defaultCapacity > 0)
            {
                for (int i = 0; i < defaultCapacity; i++)
                {
                    this.m_instances.Push(Activator.CreateInstance<TCollection>());
                }
            }
        }

        public TCollection Get()
        {
            Stack<TCollection> instances = this.m_instances;
            lock (instances)
            {
                if (this.m_instances.Count > 0)
                {
                    return this.m_instances.Pop();
                }
            }
            return Activator.CreateInstance<TCollection>();
        }

        public void Return(TCollection instance)
        {
            instance.Clear();
            Stack<TCollection> instances = this.m_instances;
            lock (instances)
            {
                this.m_instances.Push(instance);
            }
        }

        object IConcurrentPool.Get() => 
            this.Get();

        void IConcurrentPool.Return(object obj)
        {
            this.Return((TCollection) obj);
        }

        public int Count
        {
            get
            {
                Stack<TCollection> instances = this.m_instances;
                lock (instances)
                {
                    return this.m_instances.Count;
                }
            }
        }
    }
}

