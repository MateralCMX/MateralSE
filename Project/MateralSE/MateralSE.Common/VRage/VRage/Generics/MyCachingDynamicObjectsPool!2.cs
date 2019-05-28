namespace VRage.Generics
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;

    public class MyCachingDynamicObjectsPool<ObjectKey, ObjectType> where ObjectType: IDisposable, new()
    {
        private static readonly int DEFAULT_POOL_SIZE;
        private static readonly int DEFAULT_CACHE_SIZE;
        private static readonly int DEFAULT_POOL_GROWTH;
        private int m_cacheSize;
        private int m_poolGrowth;
        private Dictionary<ObjectKey, ObjectType> m_cache;
        private MyQueue<ObjectKey> m_entryAge;
        private Stack<ObjectType> m_objectPool;

        static MyCachingDynamicObjectsPool()
        {
            MyCachingDynamicObjectsPool<ObjectKey, ObjectType>.DEFAULT_POOL_SIZE = 0x40;
            MyCachingDynamicObjectsPool<ObjectKey, ObjectType>.DEFAULT_CACHE_SIZE = 8;
            MyCachingDynamicObjectsPool<ObjectKey, ObjectType>.DEFAULT_POOL_GROWTH = 1;
        }

        public MyCachingDynamicObjectsPool() : this(MyCachingDynamicObjectsPool<ObjectKey, ObjectType>.DEFAULT_POOL_SIZE, MyCachingDynamicObjectsPool<ObjectKey, ObjectType>.DEFAULT_CACHE_SIZE, MyCachingDynamicObjectsPool<ObjectKey, ObjectType>.DEFAULT_POOL_GROWTH)
        {
        }

        public MyCachingDynamicObjectsPool(int poolSize) : this(poolSize, MyCachingDynamicObjectsPool<ObjectKey, ObjectType>.DEFAULT_CACHE_SIZE, MyCachingDynamicObjectsPool<ObjectKey, ObjectType>.DEFAULT_POOL_GROWTH)
        {
        }

        public MyCachingDynamicObjectsPool(int poolSize, int cacheSize) : this(poolSize, cacheSize, MyCachingDynamicObjectsPool<ObjectKey, ObjectType>.DEFAULT_POOL_GROWTH)
        {
        }

        public MyCachingDynamicObjectsPool(int poolSize, int cacheSize, int poolGrowth)
        {
            this.m_cacheSize = cacheSize;
            this.m_poolGrowth = poolGrowth;
            this.m_cache = new Dictionary<ObjectKey, ObjectType>(this.m_cacheSize);
            this.m_objectPool = new Stack<ObjectType>(poolSize);
            this.m_entryAge = new MyQueue<ObjectKey>(this.m_cacheSize);
            this.Restock(poolSize);
        }

        public ObjectType Allocate()
        {
            if (this.m_objectPool.Count > 0)
            {
                return this.m_objectPool.Pop();
            }
            if (this.m_entryAge.Count <= 0)
            {
                this.Restock(this.m_poolGrowth);
                return this.m_objectPool.Pop();
            }
            ObjectKey key = this.m_entryAge.Dequeue();
            ObjectType local2 = this.m_cache[key];
            this.m_cache.Remove(key);
            local2.Dispose();
            return local2;
        }

        public void Deallocate(ObjectType obj)
        {
            obj.Dispose();
            this.m_objectPool.Push(obj);
        }

        public void Deallocate(ObjectKey key, ObjectType obj)
        {
            if (this.m_entryAge.Count == this.m_cacheSize)
            {
                ObjectKey local = this.m_entryAge.Dequeue();
                ObjectType local2 = this.m_cache[local];
                this.m_cache.Remove(local);
                this.Deallocate(local2);
            }
            this.m_entryAge.Enqueue(key);
            this.m_cache.Add(key, obj);
        }

        private void Restock(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                this.m_objectPool.Push(Activator.CreateInstance<ObjectType>());
            }
        }

        public bool TryAllocateCached(ObjectKey key, out ObjectType obj)
        {
            if (!this.m_cache.TryGetValue(key, out obj))
            {
                obj = this.Allocate();
                return false;
            }
            this.m_entryAge.Remove(key);
            obj = this.m_cache[key];
            this.m_cache.Remove(key);
            return true;
        }
    }
}

