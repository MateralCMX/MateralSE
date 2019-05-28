namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    public class TupleSerializer<T1, T2, T3, T4, T5, T6> : ISerializer<MyTuple<T1, T2, T3, T4, T5, T6>>
    {
        public readonly ISerializer<T1> m_serializer1;
        public readonly ISerializer<T2> m_serializer2;
        public readonly ISerializer<T3> m_serializer3;
        public readonly ISerializer<T4> m_serializer4;
        public readonly ISerializer<T5> m_serializer5;
        public readonly ISerializer<T6> m_serializer6;

        public TupleSerializer(ISerializer<T1> serializer1, ISerializer<T2> serializer2, ISerializer<T3> serializer3, ISerializer<T4> serializer4, ISerializer<T5> serializer5, ISerializer<T6> serializer6)
        {
            this.m_serializer1 = serializer1;
            this.m_serializer2 = serializer2;
            this.m_serializer3 = serializer3;
            this.m_serializer4 = serializer4;
            this.m_serializer5 = serializer5;
            this.m_serializer6 = serializer6;
        }

        void ISerializer<MyTuple<T1, T2, T3, T4, T5, T6>>.Deserialize(ByteStream source, out MyTuple<T1, T2, T3, T4, T5, T6> data)
        {
            this.m_serializer1.Deserialize(source, out data.Item1);
            this.m_serializer2.Deserialize(source, out data.Item2);
            this.m_serializer3.Deserialize(source, out data.Item3);
            this.m_serializer4.Deserialize(source, out data.Item4);
            this.m_serializer5.Deserialize(source, out data.Item5);
            this.m_serializer6.Deserialize(source, out data.Item6);
        }

        void ISerializer<MyTuple<T1, T2, T3, T4, T5, T6>>.Serialize(ByteStream destination, ref MyTuple<T1, T2, T3, T4, T5, T6> data)
        {
            this.m_serializer1.Serialize(destination, ref data.Item1);
            this.m_serializer2.Serialize(destination, ref data.Item2);
            this.m_serializer3.Serialize(destination, ref data.Item3);
            this.m_serializer4.Serialize(destination, ref data.Item4);
            this.m_serializer5.Serialize(destination, ref data.Item5);
            this.m_serializer6.Serialize(destination, ref data.Item6);
        }
    }
}

