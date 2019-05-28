namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using VRageMath;

    public interface IMyDestinationShape
    {
        void DebugDraw();
        Vector3D GetBestPoint(Vector3D queryPoint);
        Vector3D GetClosestPoint(Vector3D queryPoint);
        Vector3D GetDestination();
        float PointAdmissibility(Vector3D position, float tolerance);
        void SetRelativeTransform(MatrixD invWorldTransform);
        void UpdateWorldTransform(MatrixD worldTransform);
    }
}

