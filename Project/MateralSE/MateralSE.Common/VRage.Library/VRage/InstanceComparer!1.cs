namespace VRage
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class InstanceComparer<T> : IEqualityComparer<T> where T: class
    {
        public static readonly InstanceComparer<T> Default;

        static InstanceComparer()
        {
            InstanceComparer<T>.Default = new InstanceComparer<T>();
        }

        public bool Equals(T x, T y) => 
            (x == y);

        public int GetHashCode(T obj) => 
            RuntimeHelpers.GetHashCode(obj);
    }
}

