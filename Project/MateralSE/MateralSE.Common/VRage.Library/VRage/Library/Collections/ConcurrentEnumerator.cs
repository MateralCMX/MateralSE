namespace VRage.Library.Collections
{
    using System;

    public static class ConcurrentEnumerator
    {
        public static ConcurrentEnumerator<TLock, TItem, TEnumerator> Create<TLock, TItem, TEnumerator>(TLock @lock, TEnumerator enumerator) where TLock: struct, IDisposable where TEnumerator: IEnumerator<TItem> => 
            new ConcurrentEnumerator<TLock, TItem, TEnumerator>(@lock, enumerator);
    }
}

