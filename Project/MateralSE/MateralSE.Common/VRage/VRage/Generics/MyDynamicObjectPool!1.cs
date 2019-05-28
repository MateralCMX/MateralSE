namespace VRage.Generics
{
    using System;
    using System.Collections.Generic;

    public class MyDynamicObjectPool<T> where T: class, new()
    {
        private readonly Stack<T> m_poolStack;

        public MyDynamicObjectPool(int capacity)
        {
            this.m_poolStack = new Stack<T>(capacity);
            this.Preallocate(capacity);
        }

        public T Allocate()
        {
            if (this.m_poolStack.Count == 0)
            {
                this.Preallocate(1);
            }
            return this.m_poolStack.Pop();
        }

        public void Deallocate(T item)
        {
            this.m_poolStack.Push(item);
        }

        private void Preallocate(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T item = Activator.CreateInstance<T>();
                this.m_poolStack.Push(item);
            }
        }

        public void SuppressFinalize()
        {
            using (Stack<T>.Enumerator enumerator = this.m_poolStack.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    GC.SuppressFinalize(enumerator.Current);
                }
            }
        }

        public void TrimToSize(int size)
        {
            while (this.m_poolStack.Count > size)
            {
                this.m_poolStack.Pop();
            }
            this.m_poolStack.TrimExcess();
        }

        public int Count =>
            this.m_poolStack.Count;
    }
}

