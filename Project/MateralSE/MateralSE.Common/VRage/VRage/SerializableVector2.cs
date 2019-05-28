namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableVector2
    {
        public float X;
        public float Y;
        public bool ShouldSerializeX() => 
            false;

        public bool ShouldSerializeY() => 
            false;

        public SerializableVector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        [ProtoMember(0x1b), XmlAttribute, NoSerialize]
        public float x
        {
            get => 
                this.X;
            set => 
                (this.X = value);
        }
        [ProtoMember(0x1f), XmlAttribute, NoSerialize]
        public float y
        {
            get => 
                this.Y;
            set => 
                (this.Y = value);
        }
        public static implicit operator Vector2(SerializableVector2 v) => 
            new Vector2(v.X, v.Y);

        public static implicit operator SerializableVector2(Vector2 v) => 
            new SerializableVector2(v.X, v.Y);
    }
}

