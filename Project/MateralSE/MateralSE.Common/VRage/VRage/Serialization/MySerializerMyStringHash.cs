namespace VRage.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;
    using VRage.Utils;

    public class MySerializerMyStringHash : MySerializer<MyStringHash>
    {
        public override void Clone(ref MyStringHash value)
        {
        }

        public override bool Equals(ref MyStringHash a, ref MyStringHash b) => 
            a.Equals((MyStringHash) b);

        public override void Read(BitStream stream, out MyStringHash value, MySerializeInfo info)
        {
            value = MyStringHash.TryGet(stream.ReadInt32(0x20));
        }

        public override void Write(BitStream stream, ref MyStringHash value, MySerializeInfo info)
        {
            stream.WriteInt32((int) value, 0x20);
        }
    }
}

