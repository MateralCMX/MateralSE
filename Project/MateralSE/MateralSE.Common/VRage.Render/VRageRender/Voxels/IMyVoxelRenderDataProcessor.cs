namespace VRageRender.Voxels
{
    using System;
    using System.Collections.Generic;

    public interface IMyVoxelRenderDataProcessor
    {
        unsafe void AddPart(List<MyVertexFormatVoxelSingleData> vertices, ushort* indices, int indicesCount, MyVoxelMaterialTriple material);
        void GetDataAndDispose(ref MyVoxelRenderCellData data);
    }
}

