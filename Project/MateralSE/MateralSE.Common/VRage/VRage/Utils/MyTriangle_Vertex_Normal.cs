namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTriangle_Vertex_Normal
    {
        public MyTriangle_Vertices Vertexes;
        public Vector3 Normal;
    }
}

