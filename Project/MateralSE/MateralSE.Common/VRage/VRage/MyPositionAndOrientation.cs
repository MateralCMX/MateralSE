namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyPositionAndOrientation
    {
        [ProtoMember(15), XmlElement("Position")]
        public SerializableVector3D Position;
        [ProtoMember(0x13), XmlElement("Forward"), NoSerialize]
        public SerializableVector3 Forward;
        [ProtoMember(0x18), XmlElement("Up"), NoSerialize]
        public SerializableVector3 Up;
        public static readonly MyPositionAndOrientation Default;
        [Serialize(MyPrimitiveFlags.Normalized)]
        public Quaternion Orientation
        {
            get => 
                Quaternion.CreateFromRotationMatrix(this.GetMatrix());
            set
            {
                Matrix matrix = Matrix.CreateFromQuaternion(value);
                this.Forward = matrix.Forward;
                this.Up = matrix.Up;
            }
        }
        public MyPositionAndOrientation(Vector3D position, Vector3 forward, Vector3 up)
        {
            this.Position = position;
            this.Forward = forward;
            this.Up = up;
        }

        public MyPositionAndOrientation(ref MatrixD matrix)
        {
            this.Position = matrix.Translation;
            this.Forward = (SerializableVector3) matrix.Forward;
            this.Up = (SerializableVector3) matrix.Up;
        }

        public MyPositionAndOrientation(MatrixD matrix) : this(matrix.Translation, (Vector3) matrix.Forward, (Vector3) matrix.Up)
        {
        }

        public MatrixD GetMatrix() => 
            MatrixD.CreateWorld((Vector3D) this.Position, (Vector3) this.Forward, (Vector3) this.Up);

        public override string ToString()
        {
            string[] textArray1 = new string[] { this.Position.ToString(), "; ", this.Forward.ToString(), "; ", this.Up.ToString() };
            return string.Concat(textArray1);
        }

        static MyPositionAndOrientation()
        {
            Default = new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
        }
    }
}

