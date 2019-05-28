namespace VRage.Library.Collections.__helper_namespace
{
    using System;
    using System.Collections.Generic;

    internal class TypeComparer : IComparer<Type>
    {
        public static readonly TypeComparer Instance = new TypeComparer();

        public int Compare(Type x, Type y) => 
            string.CompareOrdinal(x.AssemblyQualifiedName, y.AssemblyQualifiedName);
    }
}

