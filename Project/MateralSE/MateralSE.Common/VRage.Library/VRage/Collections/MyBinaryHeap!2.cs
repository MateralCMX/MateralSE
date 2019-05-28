namespace VRage.Collections
{
    using System;
    using System.Collections.Generic;

    public class MyBinaryHeap<K, V> where V: HeapItem<K>
    {
        private HeapItem<K>[] m_array;
        private int m_count;
        private int m_capacity;
        private IComparer<K> m_comparer;

        public MyBinaryHeap()
        {
            this.m_array = new HeapItem<K>[0x80];
            this.m_count = 0;
            this.m_capacity = 0x80;
            this.m_comparer = Comparer<K>.Default;
        }

        public MyBinaryHeap(int initialCapacity)
        {
            this.m_array = new HeapItem<K>[initialCapacity];
            this.m_count = 0;
            this.m_capacity = initialCapacity;
            this.m_comparer = Comparer<K>.Default;
        }

        public MyBinaryHeap(int initialCapacity, IComparer<K> comparer)
        {
            this.m_array = new HeapItem<K>[initialCapacity];
            this.m_count = 0;
            this.m_capacity = initialCapacity;
            this.m_comparer = comparer;
        }

        public void Clear()
        {
            for (int i = 0; i < this.m_count; i++)
            {
                this.m_array[i] = null;
            }
            this.m_count = 0;
        }

        private void Down(int index)
        {
            if (this.m_count != (index + 1))
            {
                int num = (index * 2) + 1;
                int num2 = num + 1;
                HeapItem<K> fromItem = this.m_array[index];
                while (num2 <= this.m_count)
                {
                    if ((num2 == this.m_count) || (this.m_comparer.Compare(this.m_array[num].HeapKey, this.m_array[num2].HeapKey) < 0))
                    {
                        if (this.m_comparer.Compare(fromItem.HeapKey, this.m_array[num].HeapKey) <= 0)
                        {
                            break;
                        }
                        this.MoveItem(num, index);
                        index = num;
                        num2 = ((index * 2) + 1) + 1;
                        continue;
                    }
                    if (this.m_comparer.Compare(fromItem.HeapKey, this.m_array[num2].HeapKey) <= 0)
                    {
                        break;
                    }
                    this.MoveItem(num2, index);
                    index = num2;
                    num2 = ((index * 2) + 1) + 1;
                }
                this.MoveItem(fromItem, index);
            }
        }

        public V GetItem(int index) => 
            (this.m_array[index] as V);

        public void Insert(V value, K key)
        {
            if (this.m_count == this.m_capacity)
            {
                this.Reallocate();
            }
            value.HeapKey = key;
            this.MoveItem(value, this.m_count);
            this.Up(this.m_count);
            this.m_count++;
        }

        public V Min() => 
            ((V) this.m_array[0]);

        public void Modify(V item, K newKey)
        {
            K heapKey = item.HeapKey;
            item.HeapKey = newKey;
            if (this.m_comparer.Compare(heapKey, newKey) <= 0)
            {
                this.Down(item.HeapIndex);
            }
            else
            {
                this.Up(item.HeapIndex);
            }
        }

        public void ModifyDown(V item, K newKey)
        {
            item.HeapKey = newKey;
            this.Down(item.HeapIndex);
        }

        public void ModifyUp(V item, K newKey)
        {
            item.HeapKey = newKey;
            this.Up(item.HeapIndex);
        }

        private void MoveItem(int fromIndex, int toIndex)
        {
            this.m_array[toIndex] = this.m_array[fromIndex];
            this.m_array[toIndex].HeapIndex = toIndex;
        }

        private void MoveItem(HeapItem<K> fromItem, int toIndex)
        {
            this.m_array[toIndex] = fromItem;
            this.m_array[toIndex].HeapIndex = toIndex;
        }

        public void QueryAll(List<V> list)
        {
            foreach (HeapItem<K> item in this.m_array)
            {
                if (item != null)
                {
                    list.Add((V) item);
                }
            }
        }

        private void Reallocate()
        {
            HeapItem<K>[] destinationArray = new HeapItem<K>[this.m_capacity * 2];
            Array.Copy(this.m_array, destinationArray, this.m_capacity);
            this.m_array = destinationArray;
            this.m_capacity *= 2;
        }

        public void Remove(V item)
        {
            if (this.m_count == 1)
            {
                this.m_count--;
                this.m_array[0] = null;
            }
            else if ((this.m_count - 1) == item.HeapIndex)
            {
                this.m_array[this.m_count - 1] = null;
                this.m_count--;
            }
            else
            {
                this.MoveItem((int) (this.m_count - 1), item.HeapIndex);
                this.m_array[this.m_count - 1] = null;
                this.m_count--;
                if (this.m_comparer.Compare(item.HeapKey, this.m_array[item.HeapIndex].HeapKey) < 0)
                {
                    this.Down(item.HeapIndex);
                }
                else
                {
                    this.Up(item.HeapIndex);
                }
            }
        }

        public V RemoveMax()
        {
            int index = 0;
            for (int i = 1; i < this.m_count; i++)
            {
                if (this.m_comparer.Compare(this.m_array[index].HeapKey, this.m_array[i].HeapKey) < 0)
                {
                    index = i;
                }
            }
            V local = this.m_array[index] as V;
            if (index != this.m_count)
            {
                this.MoveItem((int) (this.m_count - 1), index);
                this.Up(index);
            }
            this.m_count--;
            return local;
        }

        public V RemoveMin()
        {
            V local = (V) this.m_array[0];
            if (this.m_count == 1)
            {
                this.m_count--;
                this.m_array[0] = null;
            }
            else
            {
                this.MoveItem((int) (this.m_count - 1), 0);
                this.m_array[this.m_count - 1] = null;
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
                if (this.m_comparer.Compare(this.m_array[num].HeapKey, this.m_array[index].HeapKey) > 0)
                {
                    HeapItem<K> fromItem = this.m_array[index];
                    while (true)
                    {
                        this.MoveItem(num, index);
                        index = num;
                        if (index != 0)
                        {
                            num = (index - 1) / 2;
                            if (this.m_comparer.Compare(this.m_array[num].HeapKey, fromItem.HeapKey) > 0)
                            {
                                continue;
                            }
                        }
                        this.MoveItem(fromItem, index);
                        return;
                    }
                }
            }
        }

        public int Count =>
            this.m_count;

        public bool Full =>
            (this.m_count == this.m_capacity);
    }
}

