namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    public class MyQueue<T> : IEnumerable<T>, IEnumerable
    {
        protected T[] m_array;
        protected int m_head;
        protected int m_tail;
        protected int m_size;
        private int m_version;

        public MyQueue() : this(0)
        {
        }

        public MyQueue(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentException("Collection cannot be empty", "collection");
            }
            this.m_size = 0;
            this.m_version = 0;
            ICollection<T> is2 = collection as ICollection<T>;
            this.m_array = (is2 == null) ? new T[4] : new T[is2.Count];
            foreach (T local in collection)
            {
                this.Enqueue(local);
            }
        }

        public MyQueue(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentException("Capacity cannot be < 0", "capacity");
            }
            this.m_array = new T[capacity];
            this.m_head = 0;
            this.m_tail = 0;
            this.m_size = 0;
            this.m_version = 0;
        }

        public void Clear()
        {
            if (this.m_head < this.m_tail)
            {
                Array.Clear(this.m_array, this.m_head, this.m_size);
            }
            else
            {
                Array.Clear(this.m_array, this.m_head, this.m_array.Length - this.m_head);
                Array.Clear(this.m_array, 0, this.m_tail);
            }
            this.m_head = 0;
            this.m_tail = 0;
            this.m_size = 0;
            this.m_version++;
        }

        public bool Contains(T item)
        {
            int head = this.m_head;
            int num2 = 0;
            while (num2 < this.m_size)
            {
                if (this.m_array[head % this.m_array.Length].Equals(item))
                {
                    return true;
                }
                num2++;
                head++;
            }
            return false;
        }

        public T Dequeue()
        {
            if (this.m_size == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }
            T local = this.m_array[this.m_head];
            this.m_array[this.m_head] = default(T);
            this.m_head = (this.m_head + 1) % this.m_array.Length;
            this.m_size--;
            this.m_version++;
            return local;
        }

        public void Enqueue(T item)
        {
            if (this.m_size == this.m_array.Length)
            {
                int capacity = (int) ((this.m_array.Length * 200L) / ((long) 100));
                if (capacity < (this.m_array.Length + 4))
                {
                    capacity = this.m_array.Length + 4;
                }
                this.SetCapacity(capacity);
            }
            this.m_array[this.m_tail] = item;
            this.m_tail = (this.m_tail + 1) % this.m_array.Length;
            this.m_size++;
            this.m_version++;
        }

        public Enumerator<T> GetEnumerator() => 
            new Enumerator<T>((MyQueue<T>) this);

        public T Last()
        {
            if (this.m_size == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }
            return this.m_array[((this.m_tail - 1) + this.m_array.Length) % this.m_array.Length];
        }

        public T Peek()
        {
            if (this.m_size == 0)
            {
                throw new InvalidOperationException("Queue is empty");
            }
            return this.m_array[this.m_head];
        }

        public bool Remove(T item)
        {
            int head = this.m_head;
            int num2 = 0;
            while ((num2 < this.m_size) && !this.m_array[head % this.m_array.Length].Equals(item))
            {
                num2++;
                head++;
            }
            if (num2 == this.m_size)
            {
                return false;
            }
            this.Remove(head);
            return true;
        }

        public void Remove(int idx)
        {
            if (idx >= this.m_size)
            {
                object[] objArray1 = new object[] { "Index out of range ", idx, "/", this.m_size };
                throw new InvalidOperationException(string.Concat(objArray1));
            }
            if (idx == 0)
            {
                this.Dequeue();
            }
            else
            {
                int index = idx % this.m_array.Length;
                int num3 = ((this.m_tail + this.m_array.Length) - 1) % this.m_array.Length;
                while (index != num3)
                {
                    int num2 = (index + 1) % this.m_array.Length;
                    this.m_array[index] = this.m_array[num2];
                    index = num2;
                }
                this.m_array[num3] = default(T);
                this.m_tail = num3;
                this.m_size--;
                this.m_version++;
            }
        }

        public bool RemoveWhere(Func<T, bool> predicate, out T item)
        {
            int head = this.m_head;
            int num2 = 0;
            while ((num2 < this.m_size) && !predicate(this.m_array[head % this.m_array.Length]))
            {
                num2++;
                head++;
            }
            if (num2 == this.m_size)
            {
                item = default(T);
                return false;
            }
            item = this.m_array[head];
            this.Remove(head);
            return true;
        }

        protected void SetCapacity(int capacity)
        {
            T[] destinationArray = new T[capacity];
            if (this.m_size > 0)
            {
                if (this.m_head < this.m_tail)
                {
                    Array.Copy(this.m_array, this.m_head, destinationArray, 0, this.m_size);
                }
                else
                {
                    Array.Copy(this.m_array, this.m_head, destinationArray, 0, this.m_array.Length - this.m_head);
                    Array.Copy(this.m_array, 0, destinationArray, this.m_array.Length - this.m_head, this.m_tail);
                }
            }
            this.m_array = destinationArray;
            this.m_head = 0;
            this.m_tail = (this.m_size == capacity) ? 0 : this.m_size;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            if (this.Count > 0)
            {
                builder.Append(this[this.Count - 1]);
                for (int i = this.Count - 2; i >= 0; i--)
                {
                    builder.Append(", ");
                    builder.Append(this[i]);
                }
            }
            builder.Append(']');
            return builder.ToString();
        }

        public void TrimExcess()
        {
            if (this.m_size < ((int) (this.m_array.Length * 0.9)))
            {
                this.SetCapacity(this.m_size);
            }
        }

        public bool TryDequeue(out T item)
        {
            if (this.m_size > 0)
            {
                item = this.Dequeue();
                return true;
            }
            item = default(T);
            return false;
        }

        public T Tail
        {
            get
            {
                if (this.m_size == 0)
                {
                    throw new InvalidOperationException("Queue is empty.");
                }
                return this.m_array[this.m_tail];
            }
        }

        public T[] InternalArray
        {
            get
            {
                T[] localArray = new T[this.Count];
                for (int i = 0; i < this.Count; i++)
                {
                    localArray[i] = this[i];
                }
                return localArray;
            }
        }

        public int Count =>
            this.m_size;

        public T this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.Count))
                {
                    throw new ArgumentException("Index must be larger or equal to 0 and smaller than Count");
                }
                return this.m_array[(this.m_head + index) % this.m_array.Length];
            }
            set
            {
                if ((index < 0) || (index >= this.Count))
                {
                    throw new ArgumentException("Index must be larger or equal to 0 and smaller than Count");
                }
                this.m_array[(this.m_head + index) % this.m_array.Length] = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly int m_version;
            private readonly MyQueue<T> m_queue;
            private int m_index;
            private bool m_first;
            public Enumerator(MyQueue<T> queue)
            {
                this.m_index = 0;
                this.m_first = true;
                this.m_queue = queue;
                this.m_version = this.m_queue.m_version;
                this.Reset();
            }

            public bool MoveNext()
            {
                if (this.m_version != this.m_queue.m_version)
                {
                    throw new InvalidOperationException("Collection modified");
                }
                if (this.m_queue.Count == 0)
                {
                    return false;
                }
                this.m_index++;
                if (this.m_index == this.m_queue.m_array.Length)
                {
                    this.m_index = 0;
                }
                if (!this.m_first)
                {
                    return (this.m_index != this.m_queue.m_tail);
                }
                this.m_first = false;
                return true;
            }

            public void Reset()
            {
                this.m_first = true;
                this.m_index = this.m_queue.m_head - 1;
            }

            public T Current =>
                this.m_queue.m_array[this.m_index];
            object IEnumerator.Current =>
                this.Current;
            public void Dispose()
            {
            }
        }
    }
}

