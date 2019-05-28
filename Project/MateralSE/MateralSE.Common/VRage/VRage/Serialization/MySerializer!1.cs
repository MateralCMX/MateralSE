namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public abstract class MySerializer<T> : MySerializer
    {
        protected MySerializer()
        {
        }

        public abstract void Clone(ref T value);
        protected internal sealed override void Clone(ref object value, MySerializeInfo info)
        {
            T local = (T) value;
            this.Clone(ref local);
            value = local;
        }

        public abstract bool Equals(ref T a, ref T b);
        protected internal sealed override bool Equals(ref object a, ref object b, MySerializeInfo info)
        {
            T local = (T) a;
            T local2 = (T) b;
            return this.Equals(ref local, ref local2);
        }

        public abstract void Read(BitStream stream, out T value, MySerializeInfo info);
        protected internal sealed override void Read(BitStream stream, out object value, MySerializeInfo info)
        {
            T local;
            this.Read(stream, out local, info);
            value = local;
        }

        public abstract void Write(BitStream stream, ref T value, MySerializeInfo info);
        protected internal sealed override void Write(BitStream stream, object value, MySerializeInfo info)
        {
            T local = (T) value;
            this.Write(stream, ref local, info);
        }

        public static bool IsValueType =>
            typeof(T).IsValueType;

        public static bool IsClass =>
            !MySerializer<T>.IsValueType;
    }
}

