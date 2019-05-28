namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    public class TupleSerializer<T1> : ISerializer<MyTuple<T1>>
    {
        public readonly ISerializer<T1> m_serializer1;

        public TupleSerializer(ISerializer<T1> serializer1)
        {
            this.m_serializer1 = serializer1;
        }

        void ISerializer<MyTuple<T1>>.Deserialize(ByteStream source, out MyTuple<T1> data)
        {
            this.m_serializer1.Deserialize(source, out data.Item1);
        }

        void ISerializer<MyTuple<T1>>.Serialize(ByteStream destination, ref MyTuple<T1> data)
        {
            this.m_serializer1.Serialize(destination, ref data.Item1);
        }
    }
}

