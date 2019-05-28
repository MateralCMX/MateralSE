namespace VRageRender
{
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public static class DrawTrianglesMessageExtensions
    {
        public static void AddTriangle(this IDrawTrianglesMessage msg, Vector3D v0, Vector3D v1, Vector3D v2)
        {
            msg.AddTriangle(ref v0, ref v1, ref v2);
        }
    }
}

