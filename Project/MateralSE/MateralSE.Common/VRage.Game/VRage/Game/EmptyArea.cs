namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct EmptyArea
    {
        [ProtoMember(0x33)]
        public Vector3D Position;
        [ProtoMember(0x35)]
        public float Radius;
    }
}

