namespace VRage.Library.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ConcurrentEnumerable<TLock, TItem, TEnumerable> : IEnumerable<TItem>, IEnumerable where TLock: struct, IDisposable where TEnumerable: IEnumerable<TItem>
    {
        private IEnumerable<TItem> m_enumerable;
        private TLock m_lock;
        public ConcurrentEnumerable(TLock lk, IEnumerable<TItem> enumerable)
        {
            this.m_enumerable = enumerable;
            this.m_lock = lk;
        }

        public ConcurrentEnumerator<TLock, TItem, IEnumerator<TItem>> GetEnumerator() => 
            ConcurrentEnumerator.Create<TLock, TItem, IEnumerator<TItem>>(this.m_lock, this.m_enumerable.GetEnumerator());

        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();
    }
}

