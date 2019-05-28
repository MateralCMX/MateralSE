namespace VRage.Collections
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    public class MyConcurrentBucketPool<T> where T: class
    {
        private readonly IMyElementAllocator<T> m_allocator;
        private readonly ConcurrentDictionary<int, ConcurrentStack<T>> m_instances;
        private MyBufferStatistics m_statistics;

        public MyConcurrentBucketPool(string debugName, IMyElementAllocator<T> allocator)
        {
            this.m_allocator = allocator;
            this.m_instances = new ConcurrentDictionary<int, ConcurrentStack<T>>();
            this.m_statistics.Name = debugName;
        }

        public void Clear()
        {
            foreach (KeyValuePair<int, ConcurrentStack<T>> pair in this.m_instances)
            {
                T local;
                while (pair.Value.TryPop(out local))
                {
                    this.m_allocator.Dispose(local);
                }
            }
            this.m_instances.Clear();
            MyBufferStatistics statistics = new MyBufferStatistics {
                Name = this.m_statistics.Name
            };
            this.m_statistics = statistics;
        }

        public T Get(int bucketId)
        {
            ConcurrentStack<T> stack;
            T result = default(T);
            if (this.m_instances.TryGetValue(bucketId, out stack))
            {
                stack.TryPop(out result);
            }
            if (result == null)
            {
                result = this.m_allocator.Allocate(bucketId);
                Interlocked.Increment(ref this.m_statistics.TotalBuffersAllocated);
                Interlocked.Add(ref this.m_statistics.TotalBytesAllocated, this.m_allocator.GetBytes(result));
            }
            int bytes = this.m_allocator.GetBytes(result);
            Interlocked.Add(ref this.m_statistics.ActiveBytes, bytes);
            Interlocked.Increment(ref this.m_statistics.ActiveBuffers);
            this.m_allocator.Init(result);
            return result;
        }

        public MyBufferStatistics GetReport() => 
            this.m_statistics;

        public void Return(T instance)
        {
            ConcurrentStack<T> orAdd;
            int bytes = this.m_allocator.GetBytes(instance);
            int bucketId = this.m_allocator.GetBucketId(instance);
            Interlocked.Add(ref this.m_statistics.ActiveBytes, -bytes);
            Interlocked.Decrement(ref this.m_statistics.ActiveBuffers);
            if (!this.m_instances.TryGetValue(bucketId, out orAdd))
            {
                orAdd = this.m_instances.GetOrAdd(bucketId, new ConcurrentStack<T>());
            }
            orAdd.Push(instance);
        }
    }
}

