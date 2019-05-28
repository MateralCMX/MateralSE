namespace VRage.Compiler
{
    using System;

    public static class TypeNameHelper<T>
    {
        public static readonly string Name;

        static TypeNameHelper()
        {
            TypeNameHelper<T>.Name = typeof(T).Name;
        }
    }
}

