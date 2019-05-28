namespace VRage.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Library.Threading;

    public class MyCommitQueue<T>
    {
        private Queue<T> m_commited;
        private SpinLock m_commitLock;
        private List<T> m_dirty;
        private SpinLock m_dirtyLock;

        public MyCommitQueue()
        {
            this.m_commited = new Queue<T>();
            this.m_dirty = new List<T>();
        }

        public void Commit()
        {
            this.m_dirtyLock.Enter();
            try
            {
                this.m_commitLock.Enter();
                try
                {
                    foreach (T local in this.m_dirty)
                    {
                        this.m_commited.Enqueue(local);
                    }
                }
                finally
                {
                    this.m_commitLock.Exit();
                }
                this.m_dirty.Clear();
            }
            finally
            {
                this.m_dirtyLock.Exit();
            }
        }

        public void Enqueue(T obj)
        {
            this.m_dirtyLock.Enter();
            try
            {
                this.m_dirty.Add(obj);
            }
            finally
            {
                this.m_dirtyLock.Exit();
            }
        }

        public bool TryDequeue(out T obj)
        {
            this.m_commitLock.Enter();
            try
            {
                if (this.m_commited.Count > 0)
                {
                    obj = this.m_commited.Dequeue();
                    return true;
                }
            }
            finally
            {
                this.m_commitLock.Exit();
            }
            obj = default(T);
            return false;
        }

        public int Count
        {
            get
            {
                int count;
                this.m_commitLock.Enter();
                try
                {
                    count = this.m_commited.Count;
                }
                finally
                {
                    this.m_commitLock.Exit();
                }
                return count;
            }
        }

        public int UncommitedCount
        {
            get
            {
                int count;
                this.m_dirtyLock.Enter();
                try
                {
                    count = this.m_dirty.Count;
                }
                finally
                {
                    this.m_dirtyLock.Exit();
                }
                return count;
            }
        }
    }
}

