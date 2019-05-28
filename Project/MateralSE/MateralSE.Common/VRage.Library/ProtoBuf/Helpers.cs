namespace ProtoBuf
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;

    internal class Helpers
    {
        public static readonly Type[] EmptyTypes = new Type[0];

        private Helpers()
        {
        }

        public static StringBuilder AppendLine(StringBuilder builder) => 
            builder.AppendLine();

        public static void BlockCopy(byte[] from, int fromIndex, byte[] to, int toIndex, int count)
        {
            Buffer.BlockCopy(from, fromIndex, to, toIndex, count);
        }

        [Conditional("DEBUG")]
        public static void DebugAssert(bool condition)
        {
            if (!condition && Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        [Conditional("DEBUG")]
        public static void DebugAssert(bool condition, string message)
        {
            bool flag1 = condition;
        }

        [Conditional("DEBUG")]
        public static void DebugAssert(bool condition, string message, params object[] args)
        {
            bool flag1 = condition;
        }

        [Conditional("DEBUG")]
        public static void DebugWriteLine(string message)
        {
        }

        [Conditional("DEBUG")]
        public static void DebugWriteLine(string message, object obj)
        {
            string str;
            try
            {
                str = (obj == null) ? "(null)" : obj.ToString();
            }
            catch
            {
                str = "(exception)";
            }
        }

        internal static ConstructorInfo GetConstructor(Type type, Type[] parameterTypes, bool nonPublic) => 
            type.GetConstructor(nonPublic ? (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) : (BindingFlags.Public | BindingFlags.Instance), null, parameterTypes, null);

        internal static ConstructorInfo[] GetConstructors(Type type, bool nonPublic) => 
            type.GetConstructors(nonPublic ? (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) : (BindingFlags.Public | BindingFlags.Instance));

        internal static MethodInfo GetGetMethod(PropertyInfo property, bool nonPublic, bool allowInternal)
        {
            if (property == null)
            {
                return null;
            }
            MethodInfo getMethod = property.GetGetMethod(nonPublic);
            if (((getMethod == null) && !nonPublic) & allowInternal)
            {
                getMethod = property.GetGetMethod(true);
                if (((getMethod == null) && !getMethod.IsAssembly) && !getMethod.IsFamilyOrAssembly)
                {
                    getMethod = null;
                }
            }
            return getMethod;
        }

        internal static MemberInfo[] GetInstanceFieldsAndProperties(Type type, bool publicOnly)
        {
            BindingFlags bindingAttr = publicOnly ? (BindingFlags.Public | BindingFlags.Instance) : (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] properties = type.GetProperties(bindingAttr);
            FieldInfo[] fields = type.GetFields(bindingAttr);
            MemberInfo[] array = new MemberInfo[fields.Length + properties.Length];
            properties.CopyTo(array, 0);
            fields.CopyTo(array, properties.Length);
            return array;
        }

        internal static MethodInfo GetInstanceMethod(Type declaringType, string name) => 
            declaringType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        internal static MethodInfo GetInstanceMethod(Type declaringType, string name, Type[] types)
        {
            if (types == null)
            {
                types = EmptyTypes;
            }
            return declaringType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, types, null);
        }

        internal static Type GetMemberType(MemberInfo member)
        {
            MemberTypes memberType = member.MemberType;
            return ((memberType == MemberTypes.Field) ? ((FieldInfo) member).FieldType : ((memberType == MemberTypes.Property) ? ((PropertyInfo) member).PropertyType : null));
        }

        internal static PropertyInfo GetProperty(Type type, string name, bool nonPublic) => 
            type.GetProperty(name, nonPublic ? (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) : (BindingFlags.Public | BindingFlags.Instance));

        internal static MethodInfo GetSetMethod(PropertyInfo property, bool nonPublic, bool allowInternal)
        {
            if (property == null)
            {
                return null;
            }
            MethodInfo setMethod = property.GetSetMethod(nonPublic);
            if (((setMethod == null) && !nonPublic) & allowInternal)
            {
                setMethod = property.GetGetMethod(true);
                if (((setMethod == null) && !setMethod.IsAssembly) && !setMethod.IsFamilyOrAssembly)
                {
                    setMethod = null;
                }
            }
            return setMethod;
        }

        internal static MethodInfo GetStaticMethod(Type declaringType, string name) => 
            declaringType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        public static ProtoTypeCode GetTypeCode(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return (ProtoTypeCode) typeCode;
            }
            return (!(type == typeof(TimeSpan)) ? (!(type == typeof(Guid)) ? (!(type == typeof(Uri)) ? (!(type == typeof(byte[])) ? (!(type == typeof(Type)) ? ProtoTypeCode.Unknown : ProtoTypeCode.Type) : ProtoTypeCode.ByteArray) : ProtoTypeCode.Uri) : ProtoTypeCode.Guid) : ProtoTypeCode.TimeSpan);
        }

        internal static Type GetUnderlyingType(Type type) => 
            Nullable.GetUnderlyingType(type);

        internal static bool IsAssignableFrom(Type target, Type type) => 
            target.IsAssignableFrom(type);

        internal static bool IsEnum(Type type) => 
            type.IsEnum;

        public static bool IsInfinity(double value) => 
            double.IsInfinity(value);

        public static bool IsInfinity(float value) => 
            float.IsInfinity(value);

        public static bool IsNullOrEmpty(string value) => 
            ((value == null) || (value.Length == 0));

        internal static bool IsSubclassOf(Type type, Type baseClass) => 
            type.IsSubclassOf(baseClass);

        internal static bool IsValueType(Type type) => 
            type.IsValueType;

        internal static object ParseEnum(Type type, string value) => 
            Enum.Parse(type, value, true);

        public static void Sort(int[] keys, object[] values)
        {
            while (true)
            {
                bool flag = false;
                int index = 1;
                while (true)
                {
                    if (index >= keys.Length)
                    {
                        if (flag)
                        {
                            break;
                        }
                        return;
                    }
                    if (keys[index - 1] > keys[index])
                    {
                        int num2 = keys[index];
                        keys[index] = keys[index - 1];
                        keys[index - 1] = num2;
                        object obj2 = values[index];
                        values[index] = values[index - 1];
                        values[index - 1] = obj2;
                        flag = true;
                    }
                    index++;
                }
            }
        }

        [Conditional("TRACE")]
        public static void TraceWriteLine(string message)
        {
            Trace.WriteLine(message);
        }
    }
}

