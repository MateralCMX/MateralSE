namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Voxels;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;
    using VRage.Game.Models;
    using VRage.Voxels;
    using VRageMath;

    internal static class MyVoxelMapExtensions
    {
        public static Vector3D GetPositionOnVoxel(this MyVoxelMap map, Vector3D position, float maxVertDistance)
        {
            Vector3I vectori;
            BoundingBox box;
            MyIntersectionResultLineTriangle triangle;
            Vector3D worldPosition = position;
            MyVoxelCoordSystems.WorldPositionToGeometryCellCoord(map.PositionLeftBottomCorner, ref position, out vectori);
            MyVoxelCoordSystems.GeometryCellCoordToLocalAABB(ref vectori, out box);
            Vector3 center = box.Center;
            Line localLine = new Line(center + (Vector3D.Up * maxVertDistance), center + (Vector3D.Down * maxVertDistance), true);
            if (map.Storage.GetGeometry().Intersect(ref localLine, out triangle, IntersectionFlags.ALL_TRIANGLES))
            {
                Vector3 localPosition = triangle.InputTriangle.Vertex0;
                MyVoxelCoordSystems.LocalPositionToWorldPosition(map.PositionLeftBottomCorner - map.StorageMin, ref localPosition, out worldPosition);
            }
            return worldPosition;
        }
    }
}

