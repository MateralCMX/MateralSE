namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct UniqueListReader<T> : IEnumerable<T>, IEnumerable
    {
        public static UniqueListReader<T> Empty;
        private readonly MyUniqueList<T> m_list;
        public UniqueListReader(MyUniqueList<T> list)
        {
            this.m_list = list;
        }

        public static implicit operator ListReader<T>(UniqueListReader<T> list) => 
            list.m_list.ItemList;

        public static implicit operator UniqueListReader<T>(MyUniqueList<T> list) => 
            new UniqueListReader<T>(list);

        public int Count =>
            this.m_list.Count;
        public T ItemAt(int index) => 
            this.m_list[index];

        public List<T>.Enumerator GetEnumerator() => 
            this.m_list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        static UniqueListReader()
        {
            UniqueListReader<T>.Empty = new UniqueListReader<T>(new MyUniqueList<T>());
        }
    }
}

