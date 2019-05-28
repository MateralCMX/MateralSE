namespace ParallelTasks
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using VRage.Collections;
    using VRage.Library;

    public class Pool<T> : Singleton<Pool<T>> where T: class, new()
    {
        private readonly ConcurrentDictionary<Thread, MyConcurrentQueue<T>> m_instances;

        public Pool()
        {
            this.m_instances = new ConcurrentDictionary<Thread, MyConcurrentQueue<T>>(MyEnvironment.ProcessorCount, MyEnvironment.ProcessorCount);
        }

        public void Clean()
        {
            foreach (KeyValuePair<Thread, MyConcurrentQueue<T>> pair in this.m_instances)
            {
                pair.Value.Clear();
            }
        }

        public T Get(Thread thread)
        {
            MyConcurrentQueue<T> queue;
            T local;
            if (!this.m_instances.TryGetValue(thread, out queue))
            {
                queue = new MyConcurrentQueue<T>();
                bool flag = this.m_instances.TryAdd(thread, queue);
            }
            if (!queue.TryDequeue(out local))
            {
                local = Activator.CreateInstance<T>();
            }
            return local;
        }

        public void Return(Thread thread, T instance)
        {
            this.m_instances[thread].Enqueue(instance);
        }
    }
}

