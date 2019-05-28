namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerBool : MySerializer<bool>
    {
        public override void Clone(ref bool value)
        {
        }

        public override bool Equals(ref bool a, ref bool b) => 
            (a == b);

        public override void Read(BitStream stream, out bool value, MySerializeInfo info)
        {
            value = stream.ReadBool();
        }

        public override void Write(BitStream stream, ref bool value, MySerializeInfo info)
        {
            stream.WriteBool(value);
        }
    }
}

