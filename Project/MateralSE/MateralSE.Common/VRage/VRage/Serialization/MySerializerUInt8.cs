namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerUInt8 : MySerializer<byte>
    {
        public override void Clone(ref byte value)
        {
        }

        public override bool Equals(ref byte a, ref byte b) => 
            (a == b);

        public override void Read(BitStream stream, out byte value, MySerializeInfo info)
        {
            value = stream.ReadByte(8);
        }

        public override void Write(BitStream stream, ref byte value, MySerializeInfo info)
        {
            stream.WriteByte(value, 8);
        }
    }
}

