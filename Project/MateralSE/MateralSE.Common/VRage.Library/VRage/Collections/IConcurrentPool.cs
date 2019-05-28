namespace VRage.Collections
{
    using System;

    internal interface IConcurrentPool
    {
        object Get();
        void Return(object obj);
    }
}

