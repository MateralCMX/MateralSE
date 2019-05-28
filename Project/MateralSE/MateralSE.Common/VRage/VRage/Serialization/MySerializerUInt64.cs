namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerUInt64 : MySerializer<ulong>
    {
        public override void Clone(ref ulong value)
        {
        }

        public override bool Equals(ref ulong a, ref ulong b) => 
            (a == b);

        public override void Read(BitStream stream, out ulong value, MySerializeInfo info)
        {
            if (info.IsVariant || info.IsVariantSigned)
            {
                value = stream.ReadUInt64Variant();
            }
            else
            {
                value = stream.ReadUInt64(0x40);
            }
        }

        public override void Write(BitStream stream, ref ulong value, MySerializeInfo info)
        {
            if (info.IsVariant || info.IsVariantSigned)
            {
                stream.WriteVariant(value);
            }
            else
            {
                stream.WriteUInt64(value, 0x40);
            }
        }
    }
}

