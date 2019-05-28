namespace VRage.Serialization
{
    using SharpDX;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerHalf : MySerializer<Half>
    {
        public override void Clone(ref Half value)
        {
        }

        public override bool Equals(ref Half a, ref Half b) => 
            (a == b);

        public override void Read(BitStream stream, out Half value, MySerializeInfo info)
        {
            if (!info.IsNormalized || !info.IsFixed8)
            {
                value = stream.ReadHalf();
            }
            else
            {
                value = ((float) stream.ReadByte(8)) / 255f;
            }
        }

        public override void Write(BitStream stream, ref Half value, MySerializeInfo info)
        {
            if (!info.IsNormalized || !info.IsFixed8)
            {
                stream.WriteHalf((float) value);
            }
            else
            {
                stream.WriteByte((byte) (value * 255f), 8);
            }
        }
    }
}

