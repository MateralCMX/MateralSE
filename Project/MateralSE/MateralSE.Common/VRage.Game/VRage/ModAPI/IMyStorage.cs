namespace VRage.ModAPI
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Voxels;
    using VRageMath;

    public interface IMyStorage
    {
        void DeleteRange(MyStorageDataTypeFlags dataToWrite, Vector3I voxelRangeMin, Vector3I voxelRangeMax, bool notify);
        void ExecuteOperationFast<TVoxelOperator>(ref TVoxelOperator voxelOperator, MyStorageDataTypeFlags dataToWrite, ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax, bool notifyRangeChanged) where TVoxelOperator: struct, IVoxelOperator;
        bool Intersect(ref LineD line);
        ContainmentType Intersect(ref BoundingBox box, bool lazy);
        void NotifyRangeChanged(ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax, MyStorageDataTypeFlags dataChanged);
        [Obsolete]
        void OverwriteAllMaterials(byte materialIndex);
        void PinAndExecute(Action action);
        void PinAndExecute(Action<IMyStorage> action);
        void ReadRange(MyStorageData target, MyStorageDataTypeFlags dataToRead, int lodIndex, Vector3I lodVoxelRangeMin, Vector3I lodVoxelRangeMax);
        void ReadRange(MyStorageData target, MyStorageDataTypeFlags dataToRead, int lodIndex, Vector3I lodVoxelRangeMin, Vector3I lodVoxelRangeMax, ref MyVoxelRequestFlags requestFlags);
        void Reset(MyStorageDataTypeFlags dataToReset);
        void Save(out byte[] outCompressedData);
        void WriteRange(MyStorageData source, MyStorageDataTypeFlags dataToWrite, Vector3I voxelRangeMin, Vector3I voxelRangeMax, bool notify = true, bool skipCache = false);

        bool Closed { get; }

        bool MarkedForClose { get; }

        Vector3I Size { get; }

        bool DeleteSupported { get; }
    }
}

