namespace VRage
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyTriangle_Coords
    {
        public Vector2 TexCoord0;
        public Vector2 TexCoord1;
        public Vector2 TexCoord2;
    }
}

