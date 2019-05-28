namespace VRage.Collections
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [DebuggerDisplay("Count = {Count}")]
    public class MyDeque<T>
    {
        private T[] m_buffer;
        private int m_front;
        private int m_back;

        public MyDeque(int baseCapacity = 8)
        {
            this.m_buffer = new T[baseCapacity + 1];
        }

        public void Clear()
        {
            Array.Clear(this.m_buffer, 0, this.m_buffer.Length);
            this.m_front = 0;
            this.m_back = 0;
        }

        private void Decrement(ref int index)
        {
            index--;
            if (index < 0)
            {
                index += this.m_buffer.Length;
            }
        }

        public T DequeueBack()
        {
            this.Decrement(ref this.m_back);
            this.m_buffer[this.m_back] = default(T);
            return this.m_buffer[this.m_back];
        }

        public T DequeueFront()
        {
            T local = this.m_buffer[this.m_front];
            this.m_buffer[this.m_front] = default(T);
            this.Increment(ref this.m_front);
            return local;
        }

        public void EnqueueBack(T value)
        {
            this.EnsureCapacityForOne();
            this.m_buffer[this.m_back] = value;
            this.Increment(ref this.m_back);
        }

        public void EnqueueFront(T value)
        {
            this.EnsureCapacityForOne();
            this.Decrement(ref this.m_front);
            this.m_buffer[this.m_front] = value;
        }

        private void EnsureCapacityForOne()
        {
            if (this.Full)
            {
                T[] localArray = new T[((this.m_buffer.Length - 1) * 2) + 1];
                int index = 0;
                int front = this.m_front;
                while (front != this.m_back)
                {
                    index++;
                    localArray[index] = this.m_buffer[front];
                    this.Increment(ref front);
                }
                this.m_buffer = localArray;
                this.m_front = 0;
                this.m_back = index;
            }
        }

        private void Increment(ref int index)
        {
            index = (index + 1) % this.m_buffer.Length;
        }

        public bool Empty =>
            (this.m_front == this.m_back);

        private bool Full =>
            (((this.m_back + 1) % this.m_buffer.Length) == this.m_front);

        public int Count =>
            ((this.m_back - this.m_front) + ((this.m_back < this.m_front) ? this.m_buffer.Length : 0));
    }
}

