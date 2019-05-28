namespace VRage.Library.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ConcurrentEnumerator<TLock, TItem, TEnumerator> : IEnumerator<TItem>, IDisposable, IEnumerator where TLock: struct, IDisposable where TEnumerator: IEnumerator<TItem>
    {
        private TEnumerator m_enumerator;
        private TLock m_lock;
        public ConcurrentEnumerator(TLock @lock, TEnumerator enumerator)
        {
            this.m_enumerator = enumerator;
            this.m_lock = @lock;
        }

        public void Dispose()
        {
            this.m_enumerator.Dispose();
            this.m_lock.Dispose();
        }

        public bool MoveNext() => 
            this.m_enumerator.MoveNext();

        public void Reset()
        {
            this.m_enumerator.Reset();
        }

        public TItem Current =>
            this.m_enumerator.Current;
        object IEnumerator.Current =>
            this.Current;
    }
}

