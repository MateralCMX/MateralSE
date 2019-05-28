namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Voxels;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Voxels;
    using VRageMath;

    public static class VoxelBaseExtensions
    {
        public static MyVoxelMaterialDefinition GetMaterialAt(this MyVoxelBase self, ref Vector3D worldPosition)
        {
            Vector3 vector;
            if (self.Storage == null)
            {
                return null;
            }
            MyVoxelCoordSystems.WorldPositionToLocalPosition(worldPosition, self.PositionComp.WorldMatrix, self.PositionComp.WorldMatrixInvScaled, self.SizeInMetresHalf, out vector);
            Vector3I voxelCoords = (Vector3I) (new Vector3I(vector / 1f) + self.StorageMin);
            return self.Storage.GetMaterialAt(ref voxelCoords);
        }
    }
}

