namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;
    using VRageMath;

    public class MySerializerColor : MySerializer<Color>
    {
        public override void Clone(ref Color value)
        {
        }

        public override bool Equals(ref Color a, ref Color b) => 
            (a.PackedValue == b.PackedValue);

        public override void Read(BitStream stream, out Color value, MySerializeInfo info)
        {
            value.PackedValue = stream.ReadUInt32(0x20);
        }

        public override void Write(BitStream stream, ref Color value, MySerializeInfo info)
        {
            stream.WriteUInt32(value.PackedValue, 0x20);
        }
    }
}

