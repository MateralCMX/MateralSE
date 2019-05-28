namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableQuaternion
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
        public bool ShouldSerializeX() => 
            false;

        public bool ShouldSerializeY() => 
            false;

        public bool ShouldSerializeZ() => 
            false;

        public bool ShouldSerializeW() => 
            false;

        public SerializableQuaternion(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        [ProtoMember(0x21), XmlAttribute, NoSerialize]
        public float x
        {
            get => 
                this.X;
            set => 
                (this.X = value);
        }
        [ProtoMember(0x25), XmlAttribute, NoSerialize]
        public float y
        {
            get => 
                this.Y;
            set => 
                (this.Y = value);
        }
        [ProtoMember(0x29), XmlAttribute, NoSerialize]
        public float z
        {
            get => 
                this.Z;
            set => 
                (this.Z = value);
        }
        [ProtoMember(0x2d), XmlAttribute, NoSerialize]
        public float w
        {
            get => 
                this.W;
            set => 
                (this.W = value);
        }
        public static implicit operator Quaternion(SerializableQuaternion q) => 
            new Quaternion(q.X, q.Y, q.Z, q.W);

        public static implicit operator SerializableQuaternion(Quaternion q) => 
            new SerializableQuaternion(q.X, q.Y, q.Z, q.W);
    }
}

