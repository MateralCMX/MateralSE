namespace VRage.Library.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRangeIterator<T> : IEnumerator<T>, IDisposable, IEnumerator
    {
        private T[] m_array;
        private int m_start;
        private int m_current;
        private int m_end;
        public static Enumerable<T> ForRange(T[] array, int start, int end) => 
            new Enumerable<T>(new MyRangeIterator<T>(array, start, end));

        public static Enumerable<T> ForRange(List<T> list, int start, int end) => 
            new Enumerable<T>(new MyRangeIterator<T>(list, start, end));

        public MyRangeIterator(T[] array, int start, int end)
        {
            this.m_array = array;
            this.m_start = start;
            this.m_current = start - 1;
            this.m_end = end - 1;
        }

        public MyRangeIterator(List<T> list, int start, int end)
        {
            this.m_array = list.GetInternalArray<T>();
            this.m_start = start;
            this.m_current = start - 1;
            this.m_end = end - 1;
        }

        public void Dispose()
        {
            this.m_array = null;
        }

        public bool MoveNext()
        {
            if (this.m_current == this.m_end)
            {
                return false;
            }
            this.m_current++;
            return true;
        }

        public void Reset()
        {
            this.m_current = this.m_start - 1;
        }

        public T Current =>
            this.m_array[this.m_current];
        object IEnumerator.Current =>
            this.Current;
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerable : IEnumerable<T>, IEnumerable
        {
            private MyRangeIterator<T> m_enumerator;
            public Enumerable(MyRangeIterator<T> enume)
            {
                this.m_enumerator = enume;
            }

            public IEnumerator<T> GetEnumerator() => 
                this.m_enumerator;

            IEnumerator IEnumerable.GetEnumerator() => 
                this.GetEnumerator();
        }
    }
}

