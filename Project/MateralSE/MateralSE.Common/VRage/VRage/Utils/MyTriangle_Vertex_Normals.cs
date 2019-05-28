namespace VRage.Utils
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTriangle_Vertex_Normals
    {
        public MyTriangle_Vertices Vertices;
        public MyTriangle_Normals Normals;
    }
}

