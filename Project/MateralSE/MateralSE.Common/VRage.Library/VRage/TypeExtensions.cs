namespace VRage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Collections;

    public static class TypeExtensions
    {
        public static readonly HashSetReader<Type> CoreTypes;

        static TypeExtensions()
        {
            Type[] collection = new Type[0x16];
            collection[0] = typeof(object);
            collection[1] = typeof(string);
            collection[2] = typeof(int);
            collection[3] = typeof(short);
            collection[4] = typeof(long);
            collection[5] = typeof(uint);
            collection[6] = typeof(ushort);
            collection[7] = typeof(ulong);
            collection[8] = typeof(double);
            collection[9] = typeof(float);
            collection[10] = typeof(bool);
            collection[11] = typeof(char);
            collection[12] = typeof(byte);
            collection[13] = typeof(sbyte);
            collection[14] = typeof(decimal);
            collection[15] = typeof(Enum);
            collection[0x10] = typeof(ValueType);
            collection[0x11] = typeof(Delegate);
            collection[0x12] = typeof(MulticastDelegate);
            collection[0x13] = typeof(Type);
            collection[20] = typeof(Attribute);
            collection[0x15] = typeof(Exception);
            CoreTypes = new HashSet<Type>(collection);
        }

        private static bool CheckProperty(MemberInfo info, bool read, bool write)
        {
            PropertyInfo info2 = info as PropertyInfo;
            if ((info2 == null) || (read && !info2.CanRead))
            {
                return false;
            }
            return (!write || info2.CanWrite);
        }

        public static Type FindGenericBaseTypeArgument(this Type type, Type genericTypeDefinition)
        {
            Type[] typeArray = type.FindGenericBaseTypeArguments(genericTypeDefinition);
            return ((typeArray.Length != 0) ? typeArray[0] : null);
        }

        public static Type[] FindGenericBaseTypeArguments(this Type type, Type genericTypeDefinition)
        {
            if (!type.IsValueType && !type.IsInterface)
            {
                while (type != typeof(object))
                {
                    if (type.IsGenericType && (type.GetGenericTypeDefinition() == genericTypeDefinition))
                    {
                        return type.GetGenericArguments();
                    }
                    type = type.BaseType;
                }
            }
            return Type.EmptyTypes;
        }

        public static IEnumerable<MemberInfo> GetDataMembers(this Type t, bool fields, bool properties, bool nonPublic, bool inherited, bool _static, bool instance, bool read, bool write)
        {
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.DeclaredOnly;
            if (nonPublic)
            {
                bindingAttr |= BindingFlags.NonPublic;
            }
            if (_static)
            {
                bindingAttr |= BindingFlags.Static;
            }
            if (instance)
            {
                bindingAttr |= BindingFlags.Instance;
            }
            IEnumerable<MemberInfo> members = t.GetMembers(bindingAttr);
            if ((inherited && t.IsClass) && (t != typeof(object)))
            {
                for (Type type = t.BaseType; (type != typeof(object)) && (type != null); type = type.BaseType)
                {
                    members = members.Concat<MemberInfo>(type.GetMembers(bindingAttr));
                }
            }
            SortedDictionary<string, MemberInfo> dictionary = new SortedDictionary<string, MemberInfo>();
            foreach (MemberInfo info in members)
            {
                if ((fields ? (info.MemberType == MemberTypes.Field) : false) || (properties ? CheckProperty(info, read, write) : false))
                {
                    dictionary.Add(info.DeclaringType.Name + info.Name, info);
                }
            }
            return dictionary.Values;
        }

        public static bool HasDefaultConstructor(this Type type) => 
            (!type.IsAbstract && (type.GetConstructor(Type.EmptyTypes) != null));

        public static bool ImplementsGenericInterface(this Type subtype, Type genericInterface) => 
            subtype.GetInterfaces().Any<Type>(x => (x.IsGenericType && (x.GetGenericTypeDefinition() == genericInterface)));

        public static bool IsAccessible(this Type type)
        {
            while (!type.IsPublic)
            {
                if ((type.Attributes & TypeAttributes.NestedPublic) == TypeAttributes.AnsiClass)
                {
                    return false;
                }
                type = type.DeclaringType;
                if (type == null)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsInstanceOfGenericType(this Type subtype, Type genericType)
        {
            for (Type type = subtype; type != null; type = type.BaseType)
            {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == genericType))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsStruct(this Type type) => 
            (type.IsValueType && (!type.IsPrimitive && (!type.IsEnum && (type != typeof(decimal)))));

        public static string PrettyName(this Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return (underlyingType.PrettyName() + "?");
            }
            if (type.IsByRef)
            {
                return $"{type.GetElementType().PrettyName()}&";
            }
            if (type.IsArray)
            {
                return $"{type.GetElementType().PrettyName()}[{new string(',', type.GetArrayRank() - 1)}]";
            }
            if (type.IsGenericType)
            {
                StringBuilder builder = new StringBuilder();
                int index = type.Name.IndexOf('`');
                if (index >= 0)
                {
                    builder.Append(type.Name.Substring(0, index));
                }
                else
                {
                    builder.Append(type.Name);
                }
                builder.Append('<');
                bool flag = true;
                foreach (Type type3 in type.GetGenericArguments())
                {
                    if (!flag)
                    {
                        builder.Append(',');
                    }
                    builder.Append(type3.PrettyName());
                    flag = false;
                }
                builder.Append('>');
                return builder.ToString();
            }
            string name = type.Name;
            uint num2 = <PrivateImplementationDetails>.ComputeStringHash(name);
            if (num2 <= 0x604f4858)
            {
                if (num2 <= 0x2d9fadf1)
                {
                    if (num2 == 0x19402a08)
                    {
                        if (name == "SByte")
                        {
                            return "sbyte";
                        }
                    }
                    else if (num2 != 0x298e5e84)
                    {
                        if ((num2 == 0x2d9fadf1) && (name == "Int16"))
                        {
                            return "short";
                        }
                    }
                    else if (name == "Int64")
                    {
                        return "long";
                    }
                }
                else if (num2 == 0x4ee6c772)
                {
                    if (name == "UInt16")
                    {
                        return "ushort";
                    }
                }
                else if (num2 != 0x4ef81093)
                {
                    if ((num2 == 0x604f4858) && (name == "String"))
                    {
                        return "string";
                    }
                }
                else if (name == "UInt64")
                {
                    return "ulong";
                }
            }
            else if (num2 <= 0xc8e3517f)
            {
                if (num2 == 0xa19a545f)
                {
                    if (name == "Int32")
                    {
                        return "int";
                    }
                }
                else if (num2 != 0xa5aaf4ec)
                {
                    if ((num2 == 0xc8e3517f) && (name == "Void"))
                    {
                        return "void";
                    }
                }
                else if (name == "Decimal")
                {
                    return "decimal";
                }
            }
            else if (num2 == 0xcb39993f)
            {
                if (name == "Byte")
                {
                    return "byte";
                }
            }
            else if (num2 != 0xd2ec146c)
            {
                if ((num2 == 0xe58e64da) && (name == "Object"))
                {
                    return "object";
                }
            }
            else if (name == "UInt32")
            {
                return "uint";
            }
            return (string.IsNullOrWhiteSpace(type.FullName) ? type.Name : type.FullName);
        }

        public static int SizeOf<T>() => 
            SizeOfHelper<T>.Size;

        private static class SizeOfHelper<T>
        {
            public static readonly int Size;

            static SizeOfHelper()
            {
                DynamicMethod method = new DynamicMethod("SizeFunction", typeof(int), Type.EmptyTypes);
                ILGenerator iLGenerator = method.GetILGenerator();
                iLGenerator.Emit(OpCodes.Sizeof, typeof(T));
                iLGenerator.Emit(OpCodes.Ret);
                TypeExtensions.SizeOfHelper<T>.Size = (int) method.Invoke(null, null);
            }
        }
    }
}

