namespace VRage.ModAPI
{
    using System;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    public interface IMyVoxelBase : VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity
    {
        bool IsBoxIntersectingBoundingBoxOfThisVoxelMap(ref BoundingBoxD boundingBox);

        IMyStorage Storage { get; }

        Vector3D PositionLeftBottomCorner { get; }

        string StorageName { get; }
    }
}

