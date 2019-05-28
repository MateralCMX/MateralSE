namespace VRage.Game.Voxels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.ModAPI;
    using VRage.Voxels;
    using VRageMath;

    public interface IMyStorage : VRage.ModAPI.IMyStorage
    {
        event Action<Vector3I, Vector3I, MyStorageDataTypeFlags> RangeChanged;

        void Close();
        VRage.Game.Voxels.IMyStorage Copy();
        void DebugDraw(ref MatrixD worldMatrix, MyVoxelDebugDrawMode mode);
        byte[] GetVoxelData();
        bool Intersect(ref LineD line);
        ContainmentType Intersect(ref BoundingBoxI box, int lod, bool exhaustiveContainmentCheck = true);
        void NotifyChanged(Vector3I voxelRangeMin, Vector3I voxelRangeMax, MyStorageDataTypeFlags changedData);
        StoragePin Pin();
        void SetCompressedDataCache(byte[] data);
        void Unpin();

        uint StorageId { get; }

        bool Shared { get; }

        IMyStorageDataProvider DataProvider { get; }

        bool AreCompressedDataCached { get; }
    }
}

