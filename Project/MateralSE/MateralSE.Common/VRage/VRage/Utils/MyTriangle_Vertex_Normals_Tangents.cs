namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTriangle_Vertex_Normals_Tangents
    {
        public MyTriangle_Vertices Vertices;
        public MyTriangle_Normals Normals;
        public MyTriangle_Normals Tangents;
    }
}

