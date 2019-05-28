namespace VRageRender.Voxels
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyVoxelRenderCellData : IDisposable
    {
        public NativeArray Vertices;
        public NativeArray Normals;
        public NativeArray Indices;
        public bool ShortIndices;
        public MyVoxelMeshPartIndex[] Parts;
        public BoundingBox CellBounds;
        public int VertexCount;
        public int IndexCount;
        public void Dispose()
        {
            if (this.Vertices != null)
            {
                this.Vertices.Dispose();
            }
            if (this.Normals != null)
            {
                this.Normals.Dispose();
            }
            if (this.Indices != null)
            {
                this.Indices.Dispose();
            }
        }
    }
}

