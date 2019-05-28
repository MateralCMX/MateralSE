namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;

    public class MySerializerDBNull : MySerializer<DBNull>
    {
        public override void Clone(ref DBNull value)
        {
        }

        public override bool Equals(ref DBNull a, ref DBNull b) => 
            true;

        public override void Read(BitStream stream, out DBNull value, MySerializeInfo info)
        {
            value = DBNull.Value;
        }

        public override void Write(BitStream stream, ref DBNull value, MySerializeInfo info)
        {
        }
    }
}

