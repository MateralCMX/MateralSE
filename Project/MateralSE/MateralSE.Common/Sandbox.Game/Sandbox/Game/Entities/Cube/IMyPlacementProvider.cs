namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public interface IMyPlacementProvider
    {
        void RayCastGridCells(MyCubeGrid grid, List<Vector3I> outHitPositions, Vector3I gridSizeInflate, float maxDist);
        void UpdatePlacement();

        Vector3D RayStart { get; }

        Vector3D RayDirection { get; }

        Sandbox.Engine.Physics.MyPhysics.HitInfo? HitInfo { get; }

        MyCubeGrid ClosestGrid { get; }

        MyVoxelBase ClosestVoxelMap { get; }

        bool CanChangePlacementObjectSize { get; }

        float IntersectionDistance { get; set; }
    }
}

