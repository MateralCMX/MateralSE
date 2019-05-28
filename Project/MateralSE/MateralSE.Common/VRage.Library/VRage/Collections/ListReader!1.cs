namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ListReader<T> : IEnumerable<T>, IEnumerable
    {
        public static ListReader<T> Empty;
        private readonly List<T> m_list;
        public ListReader(List<T> list)
        {
            this.m_list = list ?? ListReader<T>.Empty.m_list;
        }

        public static implicit operator ListReader<T>(List<T> list) => 
            new ListReader<T>(list);

        public int Count =>
            this.m_list.Count;
        public T this[int index] =>
            this.m_list[index];
        public T ItemAt(int index) => 
            this.m_list[index];

        public int IndexOf(T item) => 
            this.m_list.IndexOf(item);

        public List<T>.Enumerator GetEnumerator() => 
            this.m_list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        static ListReader()
        {
            ListReader<T>.Empty = new ListReader<T>(new List<T>(0));
        }
    }
}

