namespace VRage.Library.Collections
{
    using System;
    using System.Collections.Generic;

    public class CacheList<T> : List<T>, IDisposable
    {
        public CacheList()
        {
        }

        public CacheList(int capacity) : base(capacity)
        {
        }

        void IDisposable.Dispose()
        {
            base.Clear();
        }

        public CacheList<T> Empty =>
            ((CacheList<T>) this);
    }
}

