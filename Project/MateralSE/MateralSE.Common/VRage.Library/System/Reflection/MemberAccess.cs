namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;

    public static class MemberAccess
    {
        public static bool CheckGetterSignature<T, TMember>(this MemberInfo memberInfo)
        {
            if (!typeof(T).IsAssignableFrom(memberInfo.DeclaringType))
            {
                return false;
            }
            if (memberInfo is PropertyInfo)
            {
                return typeof(TMember).IsAssignableFrom(((PropertyInfo) memberInfo).PropertyType);
            }
            if (!(memberInfo is FieldInfo))
            {
                throw new InvalidOperationException("Member info must be PropertyInfo, FieldInfo");
            }
            return typeof(TMember).IsAssignableFrom(((FieldInfo) memberInfo).FieldType);
        }

        public static bool CheckSetterSignature<T, TMember>(this MemberInfo memberInfo)
        {
            if (!typeof(T).IsAssignableFrom(memberInfo.DeclaringType))
            {
                return false;
            }
            if (memberInfo is PropertyInfo)
            {
                return typeof(TMember).IsAssignableFrom(((PropertyInfo) memberInfo).PropertyType);
            }
            if (!(memberInfo is FieldInfo))
            {
                throw new InvalidOperationException("Member info must be PropertyInfo, FieldInfo");
            }
            return typeof(TMember).IsAssignableFrom(((FieldInfo) memberInfo).FieldType);
        }

        public static Func<T, TMember> CreateGetter<T, TMember>(this MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
            {
                return ((PropertyInfo) memberInfo).CreateGetter<T, TMember>();
            }
            if (!(memberInfo is FieldInfo))
            {
                throw new InvalidOperationException("Member info must be PropertyInfo, FieldInfo");
            }
            return ((FieldInfo) memberInfo).CreateGetter<T, TMember>();
        }

        public static Getter<T, TMember> CreateGetterRef<T, TMember>(this MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
            {
                return ((PropertyInfo) memberInfo).CreateGetterRef<T, TMember>();
            }
            if (!(memberInfo is FieldInfo))
            {
                throw new InvalidOperationException("Member info must be PropertyInfo, FieldInfo");
            }
            return ((FieldInfo) memberInfo).CreateGetterRef<T, TMember>();
        }

        public static Action<T, TMember> CreateSetter<T, TMember>(this MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
            {
                return ((PropertyInfo) memberInfo).CreateSetter<T, TMember>();
            }
            if (!(memberInfo is FieldInfo))
            {
                throw new InvalidOperationException("Member info must be PropertyInfo, FieldInfo");
            }
            return ((FieldInfo) memberInfo).CreateSetter<T, TMember>();
        }

        public static Setter<T, TMember> CreateSetterRef<T, TMember>(this MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
            {
                return ((PropertyInfo) memberInfo).CreateSetterRef<T, TMember>();
            }
            if (!(memberInfo is FieldInfo))
            {
                throw new InvalidOperationException("Member info must be PropertyInfo, FieldInfo");
            }
            return ((FieldInfo) memberInfo).CreateSetterRef<T, TMember>();
        }

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
            {
                return ((PropertyInfo) memberInfo).PropertyType;
            }
            if (memberInfo is FieldInfo)
            {
                return ((FieldInfo) memberInfo).FieldType;
            }
            if (!(memberInfo is MethodInfo))
            {
                throw new InvalidOperationException("Member info must be PropertyInfo, FieldInfo or MethodInfo");
            }
            return ((MethodInfo) memberInfo).ReturnType;
        }

        public static object GetValue(this MemberInfo memberInfo, object forObject)
        {
            MemberTypes memberType = memberInfo.MemberType;
            if (memberType == MemberTypes.Field)
            {
                return ((FieldInfo) memberInfo).GetValue(forObject);
            }
            if (memberType != MemberTypes.Property)
            {
                throw new NotImplementedException();
            }
            return ((PropertyInfo) memberInfo).GetValue(forObject);
        }

        public static bool IsMemberPublic(this MemberInfo memberInfo)
        {
            MemberTypes memberType = memberInfo.MemberType;
            if (memberType == MemberTypes.Field)
            {
                return ((((FieldInfo) memberInfo).Attributes & FieldAttributes.Public) == FieldAttributes.Public);
            }
            if (memberType != MemberTypes.Property)
            {
                throw new NotImplementedException();
            }
            PropertyInfo info = (PropertyInfo) memberInfo;
            MethodInfo getMethod = info.GetGetMethod();
            MethodInfo setMethod = info.GetSetMethod();
            return ((getMethod != null) && ((setMethod != null) && (((getMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public) && ((setMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public))));
        }

        public static void SetValue(this MemberInfo memberInfo, object forObject, object value)
        {
            MemberTypes memberType = memberInfo.MemberType;
            if (memberType == MemberTypes.Field)
            {
                ((FieldInfo) memberInfo).SetValue(forObject, value);
            }
            else
            {
                if (memberType != MemberTypes.Property)
                {
                    throw new NotImplementedException();
                }
                ((PropertyInfo) memberInfo).SetValue(forObject, value);
            }
        }
    }
}

