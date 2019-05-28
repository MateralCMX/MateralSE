namespace VRage.Reflection
{
    using System;
    using System.Collections.Generic;

    public class FullyQualifiedNameComparer : IComparer<Type>
    {
        public static readonly FullyQualifiedNameComparer Default = new FullyQualifiedNameComparer();

        public int Compare(Type x, Type y) => 
            x.FullName.CompareTo(y.FullName);
    }
}

