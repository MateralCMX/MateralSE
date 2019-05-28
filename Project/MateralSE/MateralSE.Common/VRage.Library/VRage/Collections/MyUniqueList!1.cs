namespace VRage.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage.Library.Threading;

    public class MyUniqueList<T>
    {
        private List<T> m_list;
        private HashSet<T> m_hashSet;
        private SpinLockRef m_lock;

        public MyUniqueList()
        {
            this.m_list = new List<T>();
            this.m_hashSet = new HashSet<T>();
            this.m_lock = new SpinLockRef();
        }

        public bool Add(T item)
        {
            bool flag;
            using (this.m_lock.Acquire())
            {
                if (!this.m_hashSet.Add(item))
                {
                    flag = false;
                }
                else
                {
                    this.m_list.Add(item);
                    flag = true;
                }
            }
            return flag;
        }

        public void Clear()
        {
            this.m_list.Clear();
            this.m_hashSet.Clear();
        }

        public bool Contains(T item) => 
            this.m_hashSet.Contains(item);

        public List<T>.Enumerator GetEnumerator() => 
            this.m_list.GetEnumerator();

        public bool Insert(int index, T item)
        {
            bool flag;
            using (this.m_lock.Acquire())
            {
                if (this.m_hashSet.Add(item))
                {
                    this.m_list.Insert(index, item);
                    flag = true;
                }
                else
                {
                    this.m_list.Remove(item);
                    this.m_list.Insert(index, item);
                    flag = false;
                }
            }
            return flag;
        }

        public bool Remove(T item)
        {
            bool flag;
            using (this.m_lock.Acquire())
            {
                if (!this.m_hashSet.Remove(item))
                {
                    flag = false;
                }
                else
                {
                    this.m_list.Remove(item);
                    flag = true;
                }
            }
            return flag;
        }

        public int Count =>
            this.m_list.Count;

        public T this[int index] =>
            this.m_list[index];

        public UniqueListReader<T> Items =>
            new UniqueListReader<T>((MyUniqueList<T>) this);

        public ListReader<T> ItemList =>
            new ListReader<T>(this.m_list);
    }
}

