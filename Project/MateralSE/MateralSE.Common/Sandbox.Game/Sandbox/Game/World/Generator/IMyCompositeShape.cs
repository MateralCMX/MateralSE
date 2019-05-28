namespace Sandbox.Game.World.Generator
{
    using System;
    using VRage.Voxels;
    using VRageMath;

    public interface IMyCompositeShape
    {
        void Close();
        void ComputeContent(MyStorageData storage, int lodIndex, Vector3I lodVoxelRangeMin, Vector3I lodVoxelRangeMax, int lodVoxelSize);
        ContainmentType Contains(ref BoundingBox queryBox, ref BoundingSphere querySphere, int lodVoxelSize);
        void DebugDraw(ref MatrixD worldMatrix, Color color);
        float SignedDistance(ref Vector3 localPos, int lodVoxelSize);
    }
}

