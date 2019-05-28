namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableBoundingBoxD
    {
        [ProtoMember(13)]
        public SerializableVector3D Min;
        [ProtoMember(0x10)]
        public SerializableVector3D Max;
        public static implicit operator BoundingBoxD(SerializableBoundingBoxD v) => 
            new BoundingBoxD((Vector3D) v.Min, (Vector3D) v.Max);

        public static implicit operator SerializableBoundingBoxD(BoundingBoxD v) => 
            new SerializableBoundingBoxD { 
                Min = v.Min,
                Max = v.Max
            };
    }
}

