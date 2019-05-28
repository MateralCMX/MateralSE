namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableVector3
    {
        public float X;
        public float Y;
        public float Z;
        public bool ShouldSerializeX() => 
            false;

        public bool ShouldSerializeY() => 
            false;

        public bool ShouldSerializeZ() => 
            false;

        public SerializableVector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        [ProtoMember(30), XmlAttribute, NoSerialize]
        public float x
        {
            get => 
                this.X;
            set => 
                (this.X = value);
        }
        [ProtoMember(0x22), XmlAttribute, NoSerialize]
        public float y
        {
            get => 
                this.Y;
            set => 
                (this.Y = value);
        }
        [ProtoMember(0x26), XmlAttribute, NoSerialize]
        public float z
        {
            get => 
                this.Z;
            set => 
                (this.Z = value);
        }
        public bool IsZero =>
            ((this.X == 0f) && ((this.Y == 0f) && (this.Z == 0f)));
        public static implicit operator Vector3(SerializableVector3 v) => 
            new Vector3(v.X, v.Y, v.Z);

        public static implicit operator SerializableVector3(Vector3 v) => 
            new SerializableVector3(v.X, v.Y, v.Z);

        public static bool operator ==(SerializableVector3 a, SerializableVector3 b) => 
            ((a.X == b.X) && ((a.Y == b.Y) && (a.Z == b.Z)));

        public static bool operator !=(SerializableVector3 a, SerializableVector3 b) => 
            ((a.X != b.X) || ((a.Y != b.Y) || !(a.Z == b.Z)));

        public override bool Equals(object obj) => 
            ((obj is SerializableVector3) && (((SerializableVector3) obj) == this));

        public override int GetHashCode() => 
            (((this.X.GetHashCode() * 0x60000005) ^ (this.Y.GetHashCode() * 0x6011)) ^ this.Z.GetHashCode());
    }
}

