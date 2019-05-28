namespace VRageRender
{
    using System;
    using VRageMath;

    public interface IDrawTrianglesMessage
    {
        void AddIndex(int index);
        void AddTriangle(ref Vector3D v0, ref Vector3D v1, ref Vector3D v2);
        void AddVertex(Vector3D position);

        int VertexCount { get; }
    }
}

