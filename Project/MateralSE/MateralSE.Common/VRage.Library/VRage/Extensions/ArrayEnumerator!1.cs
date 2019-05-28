namespace VRage.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ArrayEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
    {
        private T[] m_array;
        private int m_currentIndex;
        public ArrayEnumerator(T[] array)
        {
            this.m_array = array;
            this.m_currentIndex = -1;
        }

        public T Current =>
            this.m_array[this.m_currentIndex];
        public void Dispose()
        {
        }

        object IEnumerator.Current =>
            this.Current;
        public bool MoveNext()
        {
            this.m_currentIndex++;
            return (this.m_currentIndex < this.m_array.Length);
        }

        public void Reset()
        {
            this.m_currentIndex = -1;
        }
    }
}

