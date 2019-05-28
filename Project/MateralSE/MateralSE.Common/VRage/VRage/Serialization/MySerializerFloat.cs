namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerFloat : MySerializer<float>
    {
        public override void Clone(ref float value)
        {
        }

        public override bool Equals(ref float a, ref float b) => 
            (a == b);

        public override void Read(BitStream stream, out float value, MySerializeInfo info)
        {
            if (info.IsNormalized && info.IsFixed8)
            {
                value = ((float) stream.ReadByte(8)) / 255f;
            }
            else if (!info.IsNormalized || !info.IsFixed16)
            {
                value = stream.ReadFloat();
            }
            else
            {
                value = ((float) stream.ReadUInt16(0x10)) / 65535f;
            }
        }

        public override void Write(BitStream stream, ref float value, MySerializeInfo info)
        {
            if (info.IsNormalized && info.IsFixed8)
            {
                stream.WriteByte((byte) (value * 255f), 8);
            }
            else if (!info.IsNormalized || !info.IsFixed16)
            {
                stream.WriteFloat(value);
            }
            else
            {
                stream.WriteUInt16((ushort) (value * 65535f), 0x10);
            }
        }
    }
}

