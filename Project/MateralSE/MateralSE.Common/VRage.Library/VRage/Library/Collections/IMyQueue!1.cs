namespace VRage.Library.Collections
{
    using System;
    using System.Runtime.InteropServices;

    public interface IMyQueue<T>
    {
        bool TryDequeueFront(out T value);
    }
}

