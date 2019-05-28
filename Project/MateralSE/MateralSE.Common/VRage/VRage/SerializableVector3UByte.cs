namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableVector3UByte
    {
        public byte X;
        public byte Y;
        public byte Z;
        public bool ShouldSerializeX() => 
            false;

        public bool ShouldSerializeY() => 
            false;

        public bool ShouldSerializeZ() => 
            false;

        public SerializableVector3UByte(byte x, byte y, byte z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        [ProtoMember(30), XmlAttribute, NoSerialize]
        public byte x
        {
            get => 
                this.X;
            set => 
                (this.X = value);
        }
        [ProtoMember(0x22), XmlAttribute, NoSerialize]
        public byte y
        {
            get => 
                this.Y;
            set => 
                (this.Y = value);
        }
        [ProtoMember(0x26), XmlAttribute, NoSerialize]
        public byte z
        {
            get => 
                this.Z;
            set => 
                (this.Z = value);
        }
        public static implicit operator Vector3UByte(SerializableVector3UByte v) => 
            new Vector3UByte(v.X, v.Y, v.Z);

        public static implicit operator SerializableVector3UByte(Vector3UByte v) => 
            new SerializableVector3UByte(v.X, v.Y, v.Z);
    }
}

