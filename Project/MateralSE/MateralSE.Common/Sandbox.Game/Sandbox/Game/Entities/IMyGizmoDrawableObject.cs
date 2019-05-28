namespace Sandbox.Game.Entities
{
    using System;
    using VRageMath;

    public interface IMyGizmoDrawableObject
    {
        bool CanBeDrawn();
        bool EnableLongDrawDistance();
        BoundingBox? GetBoundingBox();
        Color GetGizmoColor();
        Vector3 GetPositionInGrid();
        float GetRadius();
        MatrixD GetWorldMatrix();
    }
}

