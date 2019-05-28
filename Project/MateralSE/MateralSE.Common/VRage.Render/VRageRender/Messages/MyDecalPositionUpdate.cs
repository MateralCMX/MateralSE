namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyDecalPositionUpdate
    {
        public uint ID;
        public Vector3D Position;
        public Matrix Transform;
    }
}

