namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Voxels;
    using VRageMath;

    public interface IMyPathfindingLog
    {
        void LogStorageWrite(MyVoxelBase map, MyStorageData source, MyStorageDataTypeFlags dataToWrite, Vector3I voxelRangeMin, Vector3I voxelRangeMax);
    }
}

