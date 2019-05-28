namespace VRage.Library.Collections.__helper_namespace
{
    using System;
    using System.Collections.Generic;
    using VRage;

    internal class TypeIndexTupleComparer : IComparer<MyTuple<Type, int>>
    {
        public static readonly TypeComparer Instance = new TypeComparer();

        public int Compare(MyTuple<Type, int> x, MyTuple<Type, int> y) => 
            string.CompareOrdinal(x.Item1.AssemblyQualifiedName, y.Item1.AssemblyQualifiedName);
    }
}

