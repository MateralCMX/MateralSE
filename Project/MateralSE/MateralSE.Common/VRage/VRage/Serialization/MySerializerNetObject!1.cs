namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerNetObject<T> : MySerializer<T> where T: class, IMyNetObject
    {
        public override void Clone(ref T value)
        {
            throw new NotSupportedException();
        }

        public override bool Equals(ref T a, ref T b) => 
            (((T) a) == ((T) b));

        public override void Read(BitStream stream, out T value, MySerializeInfo info)
        {
            value = default(T);
            MySerializerNetObject.NetObjectResolver.Resolve<T>(stream, ref value);
        }

        public override void Write(BitStream stream, ref T value, MySerializeInfo info)
        {
            MySerializerNetObject.NetObjectResolver.Resolve<T>(stream, ref value);
        }
    }
}

