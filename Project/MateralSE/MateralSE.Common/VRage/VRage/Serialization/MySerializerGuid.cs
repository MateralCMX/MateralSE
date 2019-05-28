namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerGuid : MySerializer<Guid>
    {
        public override void Clone(ref Guid value)
        {
        }

        public override bool Equals(ref Guid a, ref Guid b) => 
            (a == b);

        public override void Read(BitStream stream, out Guid value, MySerializeInfo info)
        {
            string g = stream.ReadPrefixLengthString(info.Encoding);
            value = new Guid(g);
        }

        public override void Write(BitStream stream, ref Guid value, MySerializeInfo info)
        {
            string str = value.ToString();
            stream.WritePrefixLengthString(str, 0, str.Length, info.Encoding);
        }
    }
}

