namespace VRage
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTriangle_BoneIndicesWeigths
    {
        public MyVertex_BoneIndicesWeights Vertex0;
        public MyVertex_BoneIndicesWeights Vertex1;
        public MyVertex_BoneIndicesWeights Vertex2;
    }
}

