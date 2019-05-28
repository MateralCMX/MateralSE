namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct QueueReader<T> : IEnumerable<T>, IEnumerable
    {
        private readonly Queue<T> m_collection;
        public int Count =>
            this.m_collection.Count;
        public QueueReader(Queue<T> collection)
        {
            this.m_collection = collection;
        }

        public Queue<T>.Enumerator GetEnumerator() => 
            this.m_collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();
    }
}

