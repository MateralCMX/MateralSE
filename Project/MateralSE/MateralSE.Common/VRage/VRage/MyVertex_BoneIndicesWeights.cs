namespace VRage
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyVertex_BoneIndicesWeights
    {
        public Vector4UByte Indices;
        public Vector4 Weights;
    }
}

