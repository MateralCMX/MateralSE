namespace VRage.Library.Collections
{
    using System;

    public static class EmptyArray<T>
    {
        public static readonly T[] Value;

        static EmptyArray()
        {
            EmptyArray<T>.Value = new T[0];
        }
    }
}

