namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerUInt16 : MySerializer<ushort>
    {
        public override void Clone(ref ushort value)
        {
        }

        public override bool Equals(ref ushort a, ref ushort b) => 
            (a == b);

        public override void Read(BitStream stream, out ushort value, MySerializeInfo info)
        {
            if (info.IsVariant || info.IsVariantSigned)
            {
                value = (ushort) stream.ReadUInt32Variant();
            }
            else
            {
                value = stream.ReadUInt16(0x10);
            }
        }

        public override void Write(BitStream stream, ref ushort value, MySerializeInfo info)
        {
            if (info.IsVariant || info.IsVariantSigned)
            {
                stream.WriteVariant(value);
            }
            else
            {
                stream.WriteUInt16(value, 0x10);
            }
        }
    }
}

