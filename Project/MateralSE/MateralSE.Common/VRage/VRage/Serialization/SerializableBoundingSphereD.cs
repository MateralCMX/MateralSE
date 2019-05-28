namespace VRage.Serialization
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct SerializableBoundingSphereD
    {
        [ProtoMember(13)]
        public SerializableVector3D Center;
        [ProtoMember(0x10)]
        public double Radius;
        public static implicit operator BoundingSphereD(SerializableBoundingSphereD v) => 
            new BoundingSphereD((Vector3D) v.Center, v.Radius);

        public static implicit operator SerializableBoundingSphereD(BoundingSphereD v) => 
            new SerializableBoundingSphereD { 
                Center = v.Center,
                Radius = v.Radius
            };
    }
}

