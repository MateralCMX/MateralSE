namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public abstract class MySerializer
    {
        protected MySerializer()
        {
        }

        public static bool AnyNull(object a, object b) => 
            ((a == null) || (b == null));

        protected internal abstract void Clone(ref object value, MySerializeInfo info);
        public static T CreateAndRead<T>(BitStream stream, MySerializeInfo serializeInfo = null)
        {
            T local;
            CreateAndRead<T>(stream, out local, serializeInfo);
            return local;
        }

        public static void CreateAndRead<T>(BitStream stream, out T value, MySerializeInfo serializeInfo = null)
        {
            MySerializationHelpers.CreateAndRead<T>(stream, out value, MyFactory.GetSerializer<T>(), serializeInfo ?? MySerializeInfo.Default);
        }

        protected internal abstract bool Equals(ref object a, ref object b, MySerializeInfo info);
        protected internal abstract void Read(BitStream stream, out object value, MySerializeInfo info);
        protected internal abstract void Write(BitStream stream, object value, MySerializeInfo info);
        public static void Write<T>(BitStream stream, ref T value, MySerializeInfo serializeInfo = null)
        {
            MySerializationHelpers.Write<T>(stream, ref value, MyFactory.GetSerializer<T>(), serializeInfo ?? MySerializeInfo.Default);
        }
    }
}

