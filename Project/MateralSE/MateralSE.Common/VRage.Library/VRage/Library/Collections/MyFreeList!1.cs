namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class MyFreeList<TItem>
    {
        private TItem[] m_list;
        private int m_size;
        private readonly Queue<int> m_freePositions;
        private readonly TItem m_default;

        public MyFreeList(int capacity = 0x10, TItem defaultValue = null)
        {
            this.m_list = new TItem[0x10];
            this.m_freePositions = new Queue<int>(capacity / 2);
            this.m_default = defaultValue;
        }

        public int Allocate()
        {
            int num;
            if (this.m_freePositions.Count > 0)
            {
                num = this.m_freePositions.Dequeue();
            }
            else
            {
                if (this.m_size == this.m_list.Length)
                {
                    Array.Resize<TItem>(ref this.m_list, this.m_list.Length << 1);
                }
                int size = this.m_size;
                this.m_size = size + 1;
                num = size;
            }
            return num;
        }

        public int Allocate(TItem value)
        {
            int index = this.Allocate();
            this.m_list[index] = value;
            return index;
        }

        public void Clear()
        {
            for (int i = 0; i < this.m_size; i++)
            {
                this.m_list[i] = default(TItem);
            }
            this.m_size = 0;
            this.m_freePositions.Clear();
        }

        public void Free(int position)
        {
            this.m_list[position] = this.m_default;
            if (position == this.m_size)
            {
                this.m_size--;
            }
            else
            {
                this.m_freePositions.Enqueue(position);
            }
        }

        public TItem[] GetInternalArray() => 
            this.m_list;

        public bool KeyValid(int key) => 
            (((ulong) key) < this.m_size);

        public TItem this[int index]
        {
            get => 
                this.m_list[index];
            set => 
                (this.m_list[index] = value);
        }

        public int UsedLength =>
            this.m_size;

        public int Count =>
            (this.m_size - this.m_freePositions.Count);

        public int Capacity =>
            this.m_list.Length;
    }
}

