namespace VRage.Generics
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class MyObjectsPoolSimple<T> where T: class, new()
    {
        private T[] m_items;
        private int m_nextAllocateIndex;

        public MyObjectsPoolSimple(int capacity)
        {
            this.m_items = new T[capacity];
        }

        public T Allocate()
        {
            int index = Interlocked.Increment(ref this.m_nextAllocateIndex) - 1;
            if (index >= this.m_items.Length)
            {
                return default(T);
            }
            T local = this.m_items[index];
            if (local == null)
            {
                this.m_items[index] = local = Activator.CreateInstance<T>();
            }
            return local;
        }

        public void ClearAllAllocated()
        {
            if (this.m_nextAllocateIndex > this.m_items.Length)
            {
                Array.Resize<T>(ref this.m_items, Math.Max(this.m_nextAllocateIndex, this.m_items.Length * 2));
            }
            this.m_nextAllocateIndex = 0;
        }

        public int GetAllocatedCount() => 
            Math.Min(this.m_nextAllocateIndex, this.m_items.Length);

        public T GetAllocatedItem(int index) => 
            this.m_items[index];

        public int GetCapacity() => 
            this.m_items.Length;

        public void Sort(IComparer<T> comparer)
        {
            if (this.m_nextAllocateIndex > 1)
            {
                Array.Sort<T>(this.m_items, 0, this.GetAllocatedCount(), comparer);
            }
        }
    }
}

