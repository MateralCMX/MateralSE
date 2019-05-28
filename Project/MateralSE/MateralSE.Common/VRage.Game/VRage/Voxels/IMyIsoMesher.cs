namespace VRage.Voxels
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Voxels;
    using VRageMath;

    public interface IMyIsoMesher
    {
        MyIsoMesh Precalc(IMyStorage storage, int lod, Vector3I lodVoxelMin, Vector3I lodVoxelMax, MyStorageDataTypeFlags properties = 3, MyVoxelRequestFlags flags = 0);

        int InvalidatedRangeInflate { get; }
    }
}

