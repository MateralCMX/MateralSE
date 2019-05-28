namespace VRage.Utils
{
    using System;

    [AttributeUsage(AttributeTargets.Field, Inherited=false, AllowMultiple=false)]
    public sealed class MyFlagEnumAttribute : Attribute
    {
        private readonly Type m_enumType;

        public MyFlagEnumAttribute(Type enumType)
        {
            this.m_enumType = enumType;
        }

        public Type EnumType =>
            this.m_enumType;
    }
}

