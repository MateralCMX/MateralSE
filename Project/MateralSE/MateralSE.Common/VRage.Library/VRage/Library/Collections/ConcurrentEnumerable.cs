namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Generic;

    public static class ConcurrentEnumerable
    {
        public static ConcurrentEnumerable<TLock, TItem, TEnumerable> Create<TLock, TItem, TEnumerable>(TLock @lock, IEnumerable<TItem> enumerator) where TLock: struct, IDisposable where TEnumerable: IEnumerable<TItem> => 
            new ConcurrentEnumerable<TLock, TItem, TEnumerable>(@lock, enumerator);
    }
}

