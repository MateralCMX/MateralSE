namespace Sandbox.Definitions
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTileDefinition
    {
        public Matrix LocalMatrix;
        public Vector3 Normal;
        public bool FullQuad;
        public bool IsEmpty;
        public bool IsRounded;
        public bool DontOffsetTexture;
        public Vector3 Up;
    }
}

