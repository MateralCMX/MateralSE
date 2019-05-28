namespace Sandbox
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Groups;
    using VRage.ModAPI;
    using VRageMath;

    public static class MyCubeGridExtensions
    {
        public static BoundingBox CalculateBoundingBox(this MyObjectBuilder_CubeGrid grid)
        {
            float cubeSize = MyDefinitionManager.Static.GetCubeSize(grid.GridSizeEnum);
            BoundingBox box = new BoundingBox(Vector3.MaxValue, Vector3.MinValue);
            try
            {
                foreach (MyObjectBuilder_CubeBlock block in grid.CubeBlocks)
                {
                    MyCubeBlockDefinition definition;
                    if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(block.GetId(), out definition))
                    {
                        MyBlockOrientation blockOrientation = (MyBlockOrientation) block.BlockOrientation;
                        Vector3 point = (new Vector3((Vector3I) block.Min) * cubeSize) - new Vector3(cubeSize / 2f);
                        box.Include(point);
                        box.Include(point + Vector3.Abs(Vector3.TransformNormal(new Vector3(definition.Size) * cubeSize, blockOrientation)));
                    }
                }
            }
            catch (KeyNotFoundException exception)
            {
                MySandboxGame.Log.WriteLine(exception);
                return new BoundingBox();
            }
            return box;
        }

        public static BoundingSphere CalculateBoundingSphere(this MyObjectBuilder_CubeGrid grid) => 
            BoundingSphere.CreateFromBoundingBox(grid.CalculateBoundingBox());

        internal static bool HasSameGroupAndIsGrid<TGroupData>(this MyGroups<MyCubeGrid, TGroupData> groups, IMyEntity gridA, IMyEntity gridB) where TGroupData: IGroupData<MyCubeGrid>, new()
        {
            MyCubeGrid nodeA = gridA as MyCubeGrid;
            MyCubeGrid nodeB = gridB as MyCubeGrid;
            return ((nodeA != null) && ((nodeB != null) && groups.HasSameGroup(nodeA, nodeB)));
        }

        public static void HookMultiplayer(this MyCubeBlock cubeBlock)
        {
            if (cubeBlock != null)
            {
                MyEntities.RaiseEntityCreated(cubeBlock);
            }
        }
    }
}

