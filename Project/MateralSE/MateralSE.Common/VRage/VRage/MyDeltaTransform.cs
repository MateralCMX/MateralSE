namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Serialization;
    using VRageMath;

    [Serializable, StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyDeltaTransform
    {
        [NoSerialize]
        public Quaternion OrientationOffset;
        [Serialize, ProtoMember(30)]
        public Vector3 PositionOffset;
        [Serialize, ProtoMember(0x16)]
        public Vector4 OrientationAsVector
        {
            get => 
                this.OrientationOffset.ToVector4();
            set => 
                (this.OrientationOffset = Quaternion.FromVector4(value));
        }
        public bool IsZero =>
            ((this.PositionOffset == Vector3.Zero) && (this.OrientationOffset == Quaternion.Zero));
        public static implicit operator Matrix(MyDeltaTransform transform)
        {
            Matrix matrix;
            Matrix.CreateFromQuaternion(ref transform.OrientationOffset, out matrix);
            matrix.Translation = transform.PositionOffset;
            return matrix;
        }

        public static implicit operator MyDeltaTransform(Matrix matrix)
        {
            MyDeltaTransform transform;
            transform.PositionOffset = matrix.Translation;
            Quaternion.CreateFromRotationMatrix(ref matrix, out transform.OrientationOffset);
            return transform;
        }

        public static implicit operator MatrixD(MyDeltaTransform transform)
        {
            MatrixD xd;
            MatrixD.CreateFromQuaternion(ref transform.OrientationOffset, out xd);
            xd.Translation = transform.PositionOffset;
            return xd;
        }

        public static implicit operator MyDeltaTransform(MatrixD matrix)
        {
            MyDeltaTransform transform;
            transform.PositionOffset = (Vector3) matrix.Translation;
            Quaternion.CreateFromRotationMatrix(ref matrix, out transform.OrientationOffset);
            return transform;
        }

        public static implicit operator MyPositionAndOrientation(MyDeltaTransform deltaTransform) => 
            new MyPositionAndOrientation(deltaTransform.PositionOffset, deltaTransform.OrientationOffset.Forward, deltaTransform.OrientationOffset.Up);

        public static implicit operator MyDeltaTransform(MyPositionAndOrientation value) => 
            new MyDeltaTransform { 
                PositionOffset = (Vector3) value.Position,
                OrientationOffset = Quaternion.CreateFromForwardUp((Vector3) value.Forward, (Vector3) value.Up)
            };
    }
}

