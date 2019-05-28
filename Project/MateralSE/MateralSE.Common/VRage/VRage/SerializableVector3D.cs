namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableVector3D
    {
        public double X;
        public double Y;
        public double Z;
        public bool ShouldSerializeX() => 
            false;

        public bool ShouldSerializeY() => 
            false;

        public bool ShouldSerializeZ() => 
            false;

        public SerializableVector3D(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public SerializableVector3D(Vector3D v)
        {
            this.X = v.X;
            this.Y = v.Y;
            this.Z = v.Z;
        }

        [ProtoMember(0x25), XmlAttribute, NoSerialize]
        public double x
        {
            get => 
                this.X;
            set => 
                (this.X = value);
        }
        [ProtoMember(0x29), XmlAttribute, NoSerialize]
        public double y
        {
            get => 
                this.Y;
            set => 
                (this.Y = value);
        }
        [ProtoMember(0x2d), XmlAttribute, NoSerialize]
        public double z
        {
            get => 
                this.Z;
            set => 
                (this.Z = value);
        }
        public bool IsZero =>
            ((this.X == 0.0) && ((this.Y == 0.0) && (this.Z == 0.0)));
        public static implicit operator Vector3D(SerializableVector3D v) => 
            new Vector3D(v.X, v.Y, v.Z);

        public static implicit operator SerializableVector3D(Vector3D v) => 
            new SerializableVector3D(v.X, v.Y, v.Z);

        public override string ToString()
        {
            object[] objArray1 = new object[] { "X: ", this.X, " Y: ", this.Y, " Z: ", this.Z };
            return string.Concat(objArray1);
        }
    }
}

