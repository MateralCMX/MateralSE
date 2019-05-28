namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableVector2I
    {
        public int X;
        public int Y;
        public bool ShouldSerializeX() => 
            false;

        public bool ShouldSerializeY() => 
            false;

        public SerializableVector2I(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        [ProtoMember(0x1b), XmlAttribute, NoSerialize]
        public int x
        {
            get => 
                this.X;
            set => 
                (this.X = value);
        }
        [ProtoMember(0x1f), XmlAttribute, NoSerialize]
        public int y
        {
            get => 
                this.Y;
            set => 
                (this.Y = value);
        }
        public static implicit operator Vector2I(SerializableVector2I v) => 
            new Vector2I(v.X, v.Y);

        public static implicit operator SerializableVector2I(Vector2I v) => 
            new SerializableVector2I(v.X, v.Y);
    }
}

