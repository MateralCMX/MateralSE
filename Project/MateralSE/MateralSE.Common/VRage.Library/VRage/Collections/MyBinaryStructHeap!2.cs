namespace VRage.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class MyBinaryStructHeap<TKey, TValue> where TValue: struct
    {
        private HeapItem<TKey, TValue>[] m_array;
        private int m_count;
        private int m_capacity;
        private IComparer<TKey> m_comparer;

        public MyBinaryStructHeap(int initialCapacity = 0x80, IComparer<TKey> comparer = null)
        {
            this.m_array = new HeapItem<TKey, TValue>[initialCapacity];
            this.m_count = 0;
            this.m_capacity = initialCapacity;
            this.m_comparer = comparer ?? Comparer<TKey>.Default;
        }

        public void Clear()
        {
            for (int i = 0; i < this.m_count; i++)
            {
                TKey local = default(TKey);
                this.m_array[i].Key = local;
                TValue local2 = default(TValue);
                this.m_array[i].Value = local2;
            }
            this.m_count = 0;
        }

        private void Down(int index)
        {
            if (this.m_count != (index + 1))
            {
                int num = (index * 2) + 1;
                int num2 = num + 1;
                HeapItem<TKey, TValue> fromItem = this.m_array[index];
                while (num2 <= this.m_count)
                {
                    if ((num2 == this.m_count) || (this.m_comparer.Compare(this.m_array[num].Key, this.m_array[num2].Key) < 0))
                    {
                        if (this.m_comparer.Compare(fromItem.Key, this.m_array[num].Key) <= 0)
                        {
                            break;
                        }
                        this.MoveItem(num, index);
                        index = num;
                        num2 = ((index * 2) + 1) + 1;
                        continue;
                    }
                    if (this.m_comparer.Compare(fromItem.Key, this.m_array[num2].Key) <= 0)
                    {
                        break;
                    }
                    this.MoveItem(num2, index);
                    index = num2;
                    num2 = ((index * 2) + 1) + 1;
                }
                this.MoveItem(ref fromItem, index);
            }
        }

        public void Insert(TValue value, TKey key)
        {
            if (this.m_count == this.m_capacity)
            {
                this.Reallocate();
            }
            HeapItem<TKey, TValue> item2 = new HeapItem<TKey, TValue> {
                Key = key,
                Value = value
            };
            this.m_array[this.m_count] = item2;
            this.Up(this.m_count);
            this.m_count++;
        }

        public TValue Min() => 
            this.m_array[0].Value;

        public TKey MinKey() => 
            this.m_array[0].Key;

        private void MoveItem(int fromIndex, int toIndex)
        {
            this.m_array[toIndex] = this.m_array[fromIndex];
        }

        private void MoveItem(ref HeapItem<TKey, TValue> fromItem, int toIndex)
        {
            this.m_array[toIndex] = fromItem;
        }

        private void Reallocate()
        {
            HeapItem<TKey, TValue>[] destinationArray = new HeapItem<TKey, TValue>[this.m_capacity * 2];
            Array.Copy(this.m_array, destinationArray, this.m_capacity);
            this.m_array = destinationArray;
            this.m_capacity *= 2;
        }

        public TValue Remove(TKey key)
        {
            TValue local;
            int index = 0;
            for (int i = 1; i < this.m_count; i++)
            {
                if (this.m_comparer.Compare(key, this.m_array[i].Key) == 0)
                {
                    index = i;
                }
            }
            if (index == this.m_count)
            {
                local = default(TValue);
            }
            else
            {
                local = this.m_array[index].Value;
                this.MoveItem((int) (this.m_count - 1), index);
                this.Up(index);
                this.Down(index);
            }
            this.m_count--;
            return local;
        }

        public TValue Remove(TValue value, IEqualityComparer<TValue> comparer = null)
        {
            TValue local;
            if (this.m_count == 0)
            {
                return default(TValue);
            }
            if (comparer == null)
            {
                comparer = (IEqualityComparer<TValue>) EqualityComparer<TValue>.Default;
            }
            int index = 0;
            for (int i = 0; i < this.m_count; i++)
            {
                if (comparer.Equals(value, this.m_array[i].Value))
                {
                    index = i;
                }
            }
            if (index == this.m_count)
            {
                local = default(TValue);
            }
            else
            {
                local = this.m_array[index].Value;
                this.MoveItem((int) (this.m_count - 1), index);
                this.Up(index);
                this.Down(index);
                this.m_count--;
            }
            return local;
        }

        public TValue RemoveMax()
        {
            int index = 0;
            for (int i = 1; i < this.m_count; i++)
            {
                if (this.m_comparer.Compare(this.m_array[index].Key, this.m_array[i].Key) < 0)
                {
                    index = i;
                }
            }
            TValue local = this.m_array[index].Value;
            if (index != this.m_count)
            {
                this.MoveItem((int) (this.m_count - 1), index);
                this.Up(index);
            }
            this.m_count--;
            return local;
        }

        public TValue RemoveMin()
        {
            TKey local2;
            TValue local3;
            TValue local = this.m_array[0].Value;
            if (this.m_count == 1)
            {
                this.m_count--;
                local2 = default(TKey);
                this.m_array[0].Key = local2;
                local3 = default(TValue);
                this.m_array[0].Value = local3;
            }
            else
            {
                this.MoveItem((int) (this.m_count - 1), 0);
                local2 = default(TKey);
                this.m_array[this.m_count - 1].Key = local2;
                local3 = default(TValue);
                this.m_array[this.m_count - 1].Value = local3;
                this.m_count--;
                this.Down(0);
            }
            return local;
        }

        private void Up(int index)
        {
            if (index != 0)
            {
                int num = (index - 1) / 2;
                if (this.m_comparer.Compare(this.m_array[num].Key, this.m_array[index].Key) > 0)
                {
                    HeapItem<TKey, TValue> fromItem = this.m_array[index];
                    while (true)
                    {
                        this.MoveItem(num, index);
                        index = num;
                        if (index != 0)
                        {
                            num = (index - 1) / 2;
                            if (this.m_comparer.Compare(this.m_array[num].Key, fromItem.Key) > 0)
                            {
                                continue;
                            }
                        }
                        this.MoveItem(ref fromItem, index);
                        return;
                    }
                }
            }
        }

        public int Count =>
            this.m_count;

        public bool Full =>
            (this.m_count == this.m_capacity);

        [StructLayout(LayoutKind.Sequential)]
        public struct HeapItem
        {
            public TKey Key { get; internal set; }
            public TValue Value { get; internal set; }
            public override string ToString() => 
                (this.Key.ToString() + ": " + this.Value.ToString());
        }
    }
}

