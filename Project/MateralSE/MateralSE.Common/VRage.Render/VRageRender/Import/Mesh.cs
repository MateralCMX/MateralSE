namespace VRageRender.Import
{
    using System;
    using VRageMath;

    public class Mesh
    {
        public Matrix AbsoluteMatrix = Matrix.Identity;
        public int MeshIndex;
        public int VertexOffset = -1;
        public int VertexCount = -1;
        public int StartIndex = -1;
        public int IndexCount = -1;
    }
}

