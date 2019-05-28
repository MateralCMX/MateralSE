namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;

    public static class AttributeExtensions
    {
        public static bool HasAttribute<T>(this MemberInfo element) where T: Attribute => 
            Attribute.IsDefined(element, typeof(T));
    }
}

