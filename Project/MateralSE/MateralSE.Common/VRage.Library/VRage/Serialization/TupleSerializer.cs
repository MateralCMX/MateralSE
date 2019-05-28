namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    public class TupleSerializer : ISerializer<MyTuple>
    {
        void ISerializer<MyTuple>.Deserialize(ByteStream source, out MyTuple data)
        {
        }

        void ISerializer<MyTuple>.Serialize(ByteStream destination, ref MyTuple data)
        {
        }
    }
}

