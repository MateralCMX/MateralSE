namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;
    using VRage.Library.Threading;

    public class MyConcurrentQueue<T> : IEnumerable<T>, IEnumerable
    {
        private MyQueue<T> m_queue;
        private SpinLockRef m_lock;

        public MyConcurrentQueue()
        {
            this.m_lock = new SpinLockRef();
            this.m_queue = new MyQueue<T>(8);
        }

        public MyConcurrentQueue(int capacity)
        {
            this.m_lock = new SpinLockRef();
            this.m_queue = new MyQueue<T>(capacity);
        }

        public void Clear()
        {
            using (this.m_lock.Acquire())
            {
                this.m_queue.Clear();
            }
        }

        public T Dequeue()
        {
            using (this.m_lock.Acquire())
            {
                return this.m_queue.Dequeue();
            }
        }

        public void Enqueue(T instance)
        {
            using (this.m_lock.Acquire())
            {
                this.m_queue.Enqueue(instance);
            }
        }

        public ConcurrentEnumerator<SpinLockRef.Token, T, MyQueue<T>.Enumerator> GetEnumerator() => 
            ConcurrentEnumerator.Create<SpinLockRef.Token, T, MyQueue<T>.Enumerator>(this.m_lock.Acquire(), this.m_queue.GetEnumerator());

        public void Remove(T instance)
        {
            using (this.m_lock.Acquire())
            {
                this.m_queue.Remove(instance);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryDequeue(out T instance)
        {
            using (this.m_lock.Acquire())
            {
                if (this.m_queue.Count > 0)
                {
                    instance = this.m_queue.Dequeue();
                    return true;
                }
            }
            instance = default(T);
            return false;
        }

        public bool TryPeek(out T instance)
        {
            using (this.m_lock.Acquire())
            {
                if (this.m_queue.Count > 0)
                {
                    instance = this.m_queue.Peek();
                    return true;
                }
            }
            instance = default(T);
            return false;
        }

        public int Count
        {
            get
            {
                using (this.m_lock.Acquire())
                {
                    return this.m_queue.Count;
                }
            }
        }
    }
}

