namespace VRage
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyDecalTriangle_Data
    {
        public MyTriangle_Vertices Vertices;
        public MyTriangle_Normals Normals;
        public MyTriangle_Coords TexCoords;
        public MyTriangle_Colors Colors;
    }
}

