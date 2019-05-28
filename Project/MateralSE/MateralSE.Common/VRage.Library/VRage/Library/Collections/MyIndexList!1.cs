namespace VRage.Library.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class MyIndexList<T> : IEnumerable<T>, IEnumerable where T: class
    {
        private List<T> m_list;
        private Queue<int> m_freeList;
        private int m_version;

        public MyIndexList(int capacity = 0)
        {
            this.m_list = new List<T>(capacity);
            this.m_freeList = new Queue<int>(capacity);
        }

        public int Add(T item)
        {
            int num;
            if (item == null)
            {
                throw new ArgumentException("Null cannot be stored in IndexList, it's used as 'empty' indicator");
            }
            if (this.m_freeList.TryDequeue<int>(out num))
            {
                this.m_list[num] = item;
                this.m_version++;
                return num;
            }
            this.m_list.Add(item);
            this.m_version++;
            return (this.m_list.Count - 1);
        }

        private Enumerator<T> GetEnumerator() => 
            new Enumerator<T>((MyIndexList<T>) this);

        public void Remove(int index)
        {
            if (!this.TryRemove(index))
            {
                throw new InvalidOperationException($"Item at index {index} is already empty");
            }
        }

        public void Remove(int index, out T removedItem)
        {
            if (!this.TryRemove(index, out removedItem))
            {
                throw new InvalidOperationException($"Item at index {index} is already empty");
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryRemove(int index)
        {
            T local;
            return this.TryRemove(index, out local);
        }

        public bool TryRemove(int index, out T removedItem)
        {
            removedItem = this.m_list[index];
            if (((T) removedItem) == null)
            {
                return false;
            }
            this.m_version++;
            T local = default(T);
            this.m_list[index] = local;
            this.m_freeList.Enqueue(index);
            return true;
        }

        public int Count =>
            this.m_list.Count;

        public T this[int index] =>
            this.m_list[index];

        public int NextIndex =>
            ((this.m_freeList.Count > 0) ? this.m_freeList.Peek() : this.m_list.Count);

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private MyIndexList<T> m_list;
            private int m_index;
            private int m_version;
            public Enumerator(MyIndexList<T> list)
            {
                this.m_list = list;
                this.m_index = -1;
                this.m_version = list.m_version;
            }

            public T Current
            {
                get
                {
                    if (this.m_version != this.m_list.m_version)
                    {
                        throw new InvalidOperationException("Collection was modified after enumerator was created");
                    }
                    return this.m_list[this.m_index];
                }
            }
            public bool MoveNext()
            {
                while (true)
                {
                    this.m_index++;
                    if (this.m_index >= this.m_list.Count)
                    {
                        return false;
                    }
                    if (this.m_list[this.m_index] != null)
                    {
                        return true;
                    }
                }
            }

            void IDisposable.Dispose()
            {
            }

            object IEnumerator.Current =>
                this.Current;
            void IEnumerator.Reset()
            {
                this.m_index = -1;
                this.m_version = this.m_list.m_version;
            }
        }
    }
}

