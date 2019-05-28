namespace BulletXNA.LinearMath
{
    using System;
    using System.Reflection;

    public class ObjectArray<T> where T: new()
    {
        private const int _defaultCapacity = 4;
        private static T[] _emptyArray;
        private T[] _items;
        private int _size;
        private int _version;

        static ObjectArray()
        {
            ObjectArray<T>._emptyArray = new T[0];
        }

        public ObjectArray()
        {
            this._items = ObjectArray<T>._emptyArray;
        }

        public ObjectArray(int capacity)
        {
            if (capacity < 0)
            {
                throw new Exception("ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_SmallCapacity");
            }
            this._items = new T[capacity];
        }

        public void Add(T item)
        {
            if (this._size == this._items.Length)
            {
                this.EnsureCapacity(this._size + 1);
            }
            int index = this._size;
            this._size = index + 1;
            this._items[index] = item;
            this._version++;
        }

        public void Clear()
        {
            if (this._size > 0)
            {
                Array.Clear(this._items, 0, this._size);
                this._size = 0;
            }
            this._version++;
        }

        private void EnsureCapacity(int min)
        {
            if (this._items.Length < min)
            {
                int num = (this._items.Length == 0) ? 4 : (this._items.Length * 2);
                if (num < min)
                {
                    num = min;
                }
                this.Capacity = num;
            }
        }

        public T[] GetRawArray() => 
            this._items;

        public void Resize(int newsize)
        {
            this.Resize(newsize, true);
        }

        public void Resize(int newsize, bool allocate)
        {
            int count = this.Count;
            if (newsize >= count)
            {
                if (newsize > this.Count)
                {
                    this.Capacity = newsize;
                }
                if (allocate)
                {
                    for (int i = count; i < newsize; i++)
                    {
                        this._items[i] = Activator.CreateInstance<T>();
                    }
                }
            }
            else if (allocate)
            {
                for (int i = newsize; i < count; i++)
                {
                    this._items[i] = Activator.CreateInstance<T>();
                }
            }
            else
            {
                for (int i = newsize; i < count; i++)
                {
                    this._items[i] = default(T);
                }
            }
            this._size = newsize;
        }

        public void Swap(int index0, int index1)
        {
            T local = this._items[index0];
            this._items[index0] = this._items[index1];
            this._items[index1] = local;
        }

        public int Capacity
        {
            get => 
                this._items.Length;
            set
            {
                if (value != this._items.Length)
                {
                    if (value < this._size)
                    {
                        throw new Exception("ExceptionResource ArgumentOutOfRange_SmallCapacity");
                    }
                    if (value > 0)
                    {
                        T[] destinationArray = new T[value];
                        if (this._size > 0)
                        {
                            Array.Copy(this._items, 0, destinationArray, 0, this._size);
                        }
                        this._items = destinationArray;
                    }
                    else
                    {
                        this._items = ObjectArray<T>._emptyArray;
                    }
                }
            }
        }

        public int Count =>
            this._size;

        public T this[int index]
        {
            get
            {
                int num = (index + 1) - this._size;
                for (int i = 0; i < num; i++)
                {
                    this.Add(Activator.CreateInstance<T>());
                }
                if (index >= this._size)
                {
                    throw new Exception("ThrowHelper.ThrowArgumentOutOfRangeException()");
                }
                return this._items[index];
            }
            set
            {
                int num = (index + 1) - this._size;
                for (int i = 0; i < num; i++)
                {
                    this.Add(Activator.CreateInstance<T>());
                }
                if (index >= this._size)
                {
                    throw new Exception("ThrowHelper.ThrowArgumentOutOfRangeException()");
                }
                this._items[index] = value;
                this._version++;
            }
        }
    }
}

