namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    public class TupleSerializer<T1, T2> : ISerializer<MyTuple<T1, T2>>
    {
        public readonly ISerializer<T1> m_serializer1;
        public readonly ISerializer<T2> m_serializer2;

        public TupleSerializer(ISerializer<T1> serializer1, ISerializer<T2> serializer2)
        {
            this.m_serializer1 = serializer1;
            this.m_serializer2 = serializer2;
        }

        void ISerializer<MyTuple<T1, T2>>.Deserialize(ByteStream source, out MyTuple<T1, T2> data)
        {
            this.m_serializer1.Deserialize(source, out data.Item1);
            this.m_serializer2.Deserialize(source, out data.Item2);
        }

        void ISerializer<MyTuple<T1, T2>>.Serialize(ByteStream destination, ref MyTuple<T1, T2> data)
        {
            this.m_serializer1.Serialize(destination, ref data.Item1);
            this.m_serializer2.Serialize(destination, ref data.Item2);
        }
    }
}

