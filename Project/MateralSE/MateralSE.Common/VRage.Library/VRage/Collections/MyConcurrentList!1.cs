namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Library.Collections;

    public class MyConcurrentList<T> : IMyQueue<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private readonly List<T> m_list;
        private readonly FastResourceLock m_lock;

        public MyConcurrentList()
        {
            this.m_lock = new FastResourceLock();
            this.m_list = new List<T>();
        }

        public MyConcurrentList(int reserve)
        {
            this.m_lock = new FastResourceLock();
            this.m_list = new List<T>(reserve);
        }

        public void Add(T value)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_list.Add(value);
            }
        }

        public void AddRange(IEnumerable<T> value)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_list.AddRange(value);
            }
        }

        public void Clear()
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_list.Clear();
            }
        }

        public bool Contains(T item)
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                return this.m_list.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                this.m_list.CopyTo(array, arrayIndex);
            }
        }

        public ConcurrentEnumerator<FastResourceLockExtensions.MySharedLock, T, List<T>.Enumerator> GetEnumerator() => 
            ConcurrentEnumerator.Create<FastResourceLockExtensions.MySharedLock, T, List<T>.Enumerator>(this.m_lock.AcquireSharedUsing(), this.m_list.GetEnumerator());

        public int IndexOf(T item)
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                return this.m_list.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_list.Insert(index, item);
            }
        }

        public T Pop()
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                T local = this.m_list[this.m_list.Count - 1];
                this.m_list.RemoveAt(this.m_list.Count - 1);
                return local;
            }
        }

        public bool Remove(T item)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                return this.m_list.Remove(item);
            }
        }

        public void RemoveAll(Predicate<T> callback)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                int index = 0;
                while (index < this.Count)
                {
                    if (callback(this.m_list[index]))
                    {
                        this.m_list.RemoveAt(index);
                        continue;
                    }
                    index++;
                }
            }
        }

        public void RemoveAt(int index)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_list.RemoveAt(index);
            }
        }

        public void Sort(IComparer<T> comparer)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_list.Sort(comparer);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryDequeueBack(out T value)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                if (this.m_list.Count != 0)
                {
                    int index = this.m_list.Count - 1;
                    value = this.m_list[index];
                    this.m_list.RemoveAt(index);
                }
                else
                {
                    value = default(T);
                    return false;
                }
            }
            return true;
        }

        public bool TryDequeueFront(out T value)
        {
            value = default(T);
            using (this.m_lock.AcquireExclusiveUsing())
            {
                if (this.m_list.Count != 0)
                {
                    value = this.m_list[0];
                    this.m_list.RemoveAt(0);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public ListReader<T> ListUnsafe =>
            new ListReader<T>(this.m_list);

        public List<T> List =>
            this.m_list;

        public int Count
        {
            get
            {
                using (this.m_lock.AcquireSharedUsing())
                {
                    return this.m_list.Count;
                }
            }
        }

        public bool Empty
        {
            get
            {
                using (this.m_lock.AcquireSharedUsing())
                {
                    return (this.m_list.Count == 0);
                }
            }
        }

        public T this[int index]
        {
            get
            {
                using (this.m_lock.AcquireSharedUsing())
                {
                    return this.m_list[index];
                }
            }
            set
            {
                using (this.m_lock.AcquireExclusiveUsing())
                {
                    this.m_list[index] = value;
                }
            }
        }

        public bool IsReadOnly =>
            false;
    }
}

