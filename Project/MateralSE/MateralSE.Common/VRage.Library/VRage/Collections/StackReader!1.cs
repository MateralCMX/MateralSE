namespace VRage.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct StackReader<T> : IEnumerable<T>, IEnumerable
    {
        private readonly Stack<T> m_collection;
        public StackReader(Stack<T> collection)
        {
            this.m_collection = collection;
        }

        public Stack<T>.Enumerator GetEnumerator() => 
            this.m_collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();
    }
}

