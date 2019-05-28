namespace VRage
{
    using System;
    using System.Reflection;

    public static class MyStructDefault
    {
        public static FieldInfo GetDefaultFieldInfo(Type type)
        {
            foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (info.IsInitOnly && (info.GetCustomAttribute(typeof(StructDefaultAttribute)) != null))
                {
                    return info;
                }
            }
            return null;
        }

        public static T GetDefaultValue<T>(Type type) where T: struct
        {
            FieldInfo defaultFieldInfo = GetDefaultFieldInfo(typeof(T));
            return ((defaultFieldInfo != null) ? ((T) defaultFieldInfo.GetValue(null)) : Activator.CreateInstance<T>());
        }
    }
}

