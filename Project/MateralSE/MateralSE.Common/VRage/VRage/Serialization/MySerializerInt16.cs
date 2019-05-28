namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerInt16 : MySerializer<short>
    {
        public override void Clone(ref short value)
        {
        }

        public override bool Equals(ref short a, ref short b) => 
            (a == b);

        public override void Read(BitStream stream, out short value, MySerializeInfo info)
        {
            if (info.IsVariant)
            {
                value = (short) stream.ReadUInt32Variant();
            }
            else if (info.IsVariantSigned)
            {
                value = (short) stream.ReadInt32Variant();
            }
            else
            {
                value = stream.ReadInt16(0x10);
            }
        }

        public override void Write(BitStream stream, ref short value, MySerializeInfo info)
        {
            if (info.IsVariant)
            {
                stream.WriteVariant((uint) ((ushort) value));
            }
            else if (info.IsVariantSigned)
            {
                stream.WriteVariantSigned(value);
            }
            else
            {
                stream.WriteInt16(value, 0x10);
            }
        }
    }
}

