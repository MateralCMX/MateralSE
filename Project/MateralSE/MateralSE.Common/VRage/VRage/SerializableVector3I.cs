namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableVector3I
    {
        public int X;
        public int Y;
        public int Z;
        public bool ShouldSerializeX() => 
            false;

        public bool ShouldSerializeY() => 
            false;

        public bool ShouldSerializeZ() => 
            false;

        public SerializableVector3I(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        [ProtoMember(30), XmlAttribute, NoSerialize]
        public int x
        {
            get => 
                this.X;
            set => 
                (this.X = value);
        }
        [ProtoMember(0x22), XmlAttribute, NoSerialize]
        public int y
        {
            get => 
                this.Y;
            set => 
                (this.Y = value);
        }
        [ProtoMember(0x26), XmlAttribute, NoSerialize]
        public int z
        {
            get => 
                this.Z;
            set => 
                (this.Z = value);
        }
        public static implicit operator Vector3I(SerializableVector3I v) => 
            new Vector3I(v.X, v.Y, v.Z);

        public static implicit operator SerializableVector3I(Vector3I v) => 
            new SerializableVector3I(v.X, v.Y, v.Z);

        public static bool operator ==(SerializableVector3I a, SerializableVector3I b) => 
            ((a.X == b.X) && ((a.Y == b.Y) && (a.Z == b.Z)));

        public static bool operator !=(SerializableVector3I a, SerializableVector3I b) => 
            ((a.X != b.X) || ((a.Y != b.Y) || (a.Z != b.Z)));

        public override bool Equals(object obj) => 
            ((obj is SerializableVector3I) && (((SerializableVector3I) obj) == this));

        public override int GetHashCode() => 
            (((this.X.GetHashCode() * 0x60000005) ^ (this.Y.GetHashCode() * 0x6011)) ^ this.Z.GetHashCode());
    }
}

