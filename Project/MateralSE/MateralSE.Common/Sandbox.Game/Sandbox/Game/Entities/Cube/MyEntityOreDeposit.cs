namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Voxels;
    using VRageMath;

    public class MyEntityOreDeposit
    {
        public long DetectorId;
        public MyVoxelBase VoxelMap;
        public Vector3I CellCoord;
        public readonly List<Data> Materials = new List<Data>();
        public static readonly TypeComparer Comparer = new TypeComparer();

        public MyEntityOreDeposit(MyVoxelBase voxelMap, Vector3I cellCoord, long detectorId)
        {
            this.VoxelMap = voxelMap;
            this.CellCoord = cellCoord;
            this.DetectorId = detectorId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Data
        {
            public MyVoxelMaterialDefinition Material;
            public Vector3 AverageLocalPosition;
            internal void ComputeWorldPosition(MyVoxelBase voxelMap, out Vector3D oreWorldPosition)
            {
                MyVoxelCoordSystems.LocalPositionToWorldPosition(voxelMap.PositionComp.GetPosition() - voxelMap.StorageMin, ref this.AverageLocalPosition, out oreWorldPosition);
            }
        }

        public class TypeComparer : IEqualityComparer<MyEntityOreDeposit>
        {
            bool IEqualityComparer<MyEntityOreDeposit>.Equals(MyEntityOreDeposit x, MyEntityOreDeposit y) => 
                ((x.VoxelMap.EntityId == y.VoxelMap.EntityId) && ((x.CellCoord == y.CellCoord) && (x.DetectorId == y.DetectorId)));

            int IEqualityComparer<MyEntityOreDeposit>.GetHashCode(MyEntityOreDeposit obj) => 
                ((int) (obj.VoxelMap.EntityId ^ (obj.CellCoord.GetHashCode() * obj.DetectorId)));
        }
    }
}

