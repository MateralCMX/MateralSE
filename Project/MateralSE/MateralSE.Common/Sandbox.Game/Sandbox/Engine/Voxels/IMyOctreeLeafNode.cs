namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Voxels;
    using VRageMath;

    public interface IMyOctreeLeafNode
    {
        void ExecuteOperation<TOperator>(ref TOperator source, ref Vector3I readOffset, ref Vector3I min, ref Vector3I max) where TOperator: struct, IVoxelOperator;
        byte GetFilteredValue();
        ContainmentType Intersect(ref BoundingBoxI box, int lod, bool exhaustiveContainmentCheck = true);
        bool Intersect(ref LineD box, out double startOffset, out double endOffset);
        void OnDataProviderChanged(IMyStorageDataProvider newProvider);
        void ReadRange(MyStorageData target, MyStorageDataTypeFlags types, ref Vector3I writeOffset, int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod, ref MyVoxelRequestFlags flags);
        void ReplaceValues(Dictionary<byte, byte> oldToNewValueMap);
        bool TryGetUniformValue(out byte uniformValue);

        MyOctreeStorage.ChunkTypeEnum SerializedChunkType { get; }

        int SerializedChunkSize { get; }

        Vector3I VoxelRangeMin { get; }

        bool ReadOnly { get; }
    }
}

