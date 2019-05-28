namespace VRageRender.Voxels
{
    using System;

    public interface IMyVoxelRenderDataProcessorProvider
    {
        IMyVoxelRenderDataProcessor GetRenderDataProcessor(int vertexCount, int indexCount, int partsCount);
    }
}

