namespace Sandbox.Game.WorldEnvironment
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public static class MyEnvironmentSectorExtensions
    {
        public static void DisableItemsInBox(this MyEnvironmentSector sector, ref BoundingBoxD box)
        {
            if (sector.DataView != null)
            {
                for (int i = 0; i < sector.DataView.LogicalSectors.Count; i++)
                {
                    sector.DataView.LogicalSectors[i].DisableItemsInBox(sector.SectorCenter, ref box);
                }
            }
        }

        public static void GetItemsInAabb(this MyEnvironmentSector sector, ref BoundingBoxD aabb, List<int> itemsInBox)
        {
            if (sector.DataView != null)
            {
                aabb.Translate(-sector.SectorCenter);
                for (int i = 0; i < sector.DataView.LogicalSectors.Count; i++)
                {
                    sector.DataView.LogicalSectors[i].GetItemsInAabb(ref aabb, itemsInBox);
                }
            }
        }

        public static bool HasWorkPending(this MyEnvironmentSector self) => 
            (self.HasSerialWorkPending || (self.HasParallelWorkPending || self.HasParallelWorkInProgress));
    }
}

