namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerInt8 : MySerializer<sbyte>
    {
        public override void Clone(ref sbyte value)
        {
        }

        public override bool Equals(ref sbyte a, ref sbyte b) => 
            (a == b);

        public override void Read(BitStream stream, out sbyte value, MySerializeInfo info)
        {
            value = stream.ReadSByte(8);
        }

        public override void Write(BitStream stream, ref sbyte value, MySerializeInfo info)
        {
            stream.WriteSByte(value, 8);
        }
    }
}

