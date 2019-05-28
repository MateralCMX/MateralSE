namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerString : MySerializer<string>
    {
        public override void Clone(ref string value)
        {
        }

        public override bool Equals(ref string a, ref string b) => 
            (a == b);

        public override void Read(BitStream stream, out string value, MySerializeInfo info)
        {
            value = stream.ReadPrefixLengthString(info.Encoding);
        }

        public override void Write(BitStream stream, ref string value, MySerializeInfo info)
        {
            stream.WritePrefixLengthString(value, 0, value.Length, info.Encoding);
        }
    }
}

