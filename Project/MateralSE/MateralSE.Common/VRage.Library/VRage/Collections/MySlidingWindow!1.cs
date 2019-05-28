namespace VRage.Collections
{
    using System;
    using System.Runtime.InteropServices;

    public class MySlidingWindow<T>
    {
        private MyQueue<T> m_items;
        public int Size;
        public T DefaultValue;
        public Func<MyQueue<T>, T> AverageFunc;

        public MySlidingWindow(int size, Func<MyQueue<T>, T> avg, T defaultValue = null)
        {
            this.AverageFunc = avg;
            this.Size = size;
            this.DefaultValue = defaultValue;
            this.m_items = new MyQueue<T>(size + 1);
        }

        public void Add(T item)
        {
            this.m_items.Enqueue(item);
            this.RemoveExcess();
        }

        public void Clear()
        {
            this.m_items.Clear();
        }

        private void RemoveExcess()
        {
            while (this.m_items.Count > this.Size)
            {
                this.m_items.Dequeue();
            }
        }

        public T Average =>
            ((this.m_items.Count != 0) ? this.AverageFunc(this.m_items) : this.DefaultValue);

        public T Last =>
            ((this.m_items.Count > 0) ? this.m_items[this.m_items.Count - 1] : this.DefaultValue);
    }
}

