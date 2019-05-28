namespace VRage.Collections
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Library.Collections;

    [DebuggerDisplay("Count = {Count}")]
    public class MyConcurrentDeque<T> : IMyQueue<T>
    {
        private readonly MyDeque<T> m_deque;
        private readonly FastResourceLock m_lock;

        public MyConcurrentDeque()
        {
            this.m_deque = new MyDeque<T>(8);
            this.m_lock = new FastResourceLock();
        }

        public void Clear()
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_deque.Clear();
            }
        }

        public void EnqueueBack(T value)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_deque.EnqueueBack(value);
            }
        }

        public void EnqueueFront(T value)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_deque.EnqueueFront(value);
            }
        }

        public bool TryDequeueBack(out T value)
        {
            bool flag;
            using (this.m_lock.AcquireExclusiveUsing())
            {
                if (this.m_deque.Empty)
                {
                    value = default(T);
                    flag = false;
                }
                else
                {
                    value = this.m_deque.DequeueBack();
                    flag = true;
                }
            }
            return flag;
        }

        public bool TryDequeueFront(out T value)
        {
            bool flag;
            using (this.m_lock.AcquireExclusiveUsing())
            {
                if (this.m_deque.Empty)
                {
                    value = default(T);
                    flag = false;
                }
                else
                {
                    value = this.m_deque.DequeueFront();
                    flag = true;
                }
            }
            return flag;
        }

        public bool Empty
        {
            get
            {
                using (this.m_lock.AcquireSharedUsing())
                {
                    return this.m_deque.Empty;
                }
            }
        }

        public int Count
        {
            get
            {
                using (this.m_lock.AcquireSharedUsing())
                {
                    return this.m_deque.Count;
                }
            }
        }
    }
}

