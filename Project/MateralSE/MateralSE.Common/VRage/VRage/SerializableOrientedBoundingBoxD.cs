namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableOrientedBoundingBoxD
    {
        [ProtoMember(13)]
        public SerializableVector3D Center;
        [ProtoMember(0x10)]
        public SerializableVector3D HalfExtent;
        [ProtoMember(0x13)]
        public SerializableQuaternion Orientation;
        public static implicit operator MyOrientedBoundingBoxD(SerializableOrientedBoundingBoxD v) => 
            new MyOrientedBoundingBoxD((Vector3D) v.Center, (Vector3D) v.HalfExtent, (Quaternion) v.Orientation);

        public static implicit operator SerializableOrientedBoundingBoxD(MyOrientedBoundingBoxD v) => 
            new SerializableOrientedBoundingBoxD { 
                Center = v.Center,
                HalfExtent = v.HalfExtent,
                Orientation = v.Orientation
            };
    }
}

