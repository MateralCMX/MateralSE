namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyDecalTopoData
    {
        public Matrix MatrixBinding;
        public Vector3D WorldPosition;
        public Matrix MatrixCurrent;
        public Vector4UByte BoneIndices;
        public Vector4 BoneWeights;
    }
}

