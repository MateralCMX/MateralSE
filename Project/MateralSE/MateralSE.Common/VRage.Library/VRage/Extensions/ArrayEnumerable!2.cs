namespace VRage.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ArrayEnumerable<T, TEnumerator> : IEnumerable<T>, IEnumerable where TEnumerator: struct, IEnumerator<T>
    {
        private TEnumerator m_enumerator;
        public ArrayEnumerable(TEnumerator enumerator)
        {
            this.m_enumerator = enumerator;
        }

        public TEnumerator GetEnumerator() => 
            this.m_enumerator;

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();
    }
}

