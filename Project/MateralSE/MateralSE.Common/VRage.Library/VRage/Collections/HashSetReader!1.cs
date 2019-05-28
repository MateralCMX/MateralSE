namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct HashSetReader<T> : IEnumerable<T>, IEnumerable
    {
        private readonly HashSet<T> m_hashset;
        public HashSetReader(HashSet<T> set)
        {
            this.m_hashset = set;
        }

        public static implicit operator HashSetReader<T>(HashSet<T> v) => 
            new HashSetReader<T>(v);

        public bool IsValid =>
            (this.m_hashset != null);
        public int Count =>
            this.m_hashset.Count;
        public bool Contains(T item) => 
            this.m_hashset.Contains(item);

        public T First()
        {
            using (HashSet<T>.Enumerator enumerator = this.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException("No elements in collection!");
                }
                return enumerator.Current;
            }
        }

        public T[] ToArray()
        {
            T[] array = new T[this.m_hashset.Count];
            this.m_hashset.CopyTo(array);
            return array;
        }

        public HashSet<T>.Enumerator GetEnumerator() => 
            this.m_hashset.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();
    }
}

