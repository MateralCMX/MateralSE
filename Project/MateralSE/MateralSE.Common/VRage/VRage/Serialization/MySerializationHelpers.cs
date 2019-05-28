namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public static class MySerializationHelpers
    {
        public static bool CreateAndRead<TMember>(BitStream stream, out TMember result, MySerializer<TMember> serializer, MySerializeInfo info)
        {
            if (!ReadNullable(stream, info.IsNullable))
            {
                result = default(TMember);
                return false;
            }
            if (!MySerializer<TMember>.IsClass || !info.IsDynamic)
            {
                serializer.Read(stream, out result, info);
            }
            else
            {
                object obj2;
                Type t = typeof(TMember);
                bool flag = true;
                if (info.IsDynamicDefault)
                {
                    flag = stream.ReadBool();
                }
                if (flag)
                {
                    t = stream.ReadDynamicType(typeof(TMember), info.DynamicSerializer);
                }
                MyFactory.GetSerializer(t).Read(stream, out obj2, info);
                result = (TMember) obj2;
            }
            return true;
        }

        private static bool ReadNullable(BitStream stream, bool isNullable) => 
            (!isNullable || stream.ReadBool());

        public static void Write<TMember>(BitStream stream, ref TMember value, MySerializer<TMember> serializer, MySerializeInfo info)
        {
            if (WriteNullable<TMember>(stream, ref value, info.IsNullable, serializer))
            {
                if (!MySerializer<TMember>.IsClass || !info.IsDynamic)
                {
                    if (!MySerializer<TMember>.IsValueType && !(value.GetType() == typeof(TMember)))
                    {
                        throw new MySerializeException(MySerializeErrorEnum.DynamicNotAllowed);
                    }
                    serializer.Write(stream, ref value, info);
                }
                else
                {
                    Type baseType = typeof(TMember);
                    Type type = value.GetType();
                    bool flag = true;
                    if (info.IsDynamicDefault)
                    {
                        flag = baseType != type;
                        stream.WriteBool(flag);
                    }
                    if (flag)
                    {
                        stream.WriteDynamicType(baseType, type, info.DynamicSerializer);
                    }
                    MyFactory.GetSerializer(value.GetType()).Write(stream, (TMember) value, info);
                }
            }
        }

        private static bool WriteNullable<T>(BitStream stream, ref T value, bool isNullable, MySerializer<T> serializer)
        {
            if (isNullable)
            {
                T b = default(T);
                bool flag = !serializer.Equals(ref value, ref b);
                stream.WriteBool(flag);
                return flag;
            }
            if (!typeof(T).IsValueType && (((T) value) == null))
            {
                throw new MySerializeException(MySerializeErrorEnum.NullNotAllowed);
            }
            return true;
        }
    }
}

