namespace VRage.ModAPI
{
    using System;
    using VRageMath;

    public interface IMyCamera
    {
        double GetDistanceWithFOV(Vector3D position);
        bool IsInFrustum(ref BoundingBoxD boundingBox);
        bool IsInFrustum(ref BoundingSphereD boundingSphere);
        bool IsInFrustum(BoundingBoxD boundingBox);
        LineD WorldLineFromScreen(Vector2 screenCoords);
        Vector3D WorldToScreen(ref Vector3D worldPos);

        Vector3D Position { get; }

        Vector3D PreviousPosition { get; }

        Vector2 ViewportOffset { get; }

        Vector2 ViewportSize { get; }

        MatrixD ViewMatrix { get; }

        MatrixD WorldMatrix { get; }

        MatrixD ProjectionMatrix { get; }

        float NearPlaneDistance { get; }

        float FarPlaneDistance { get; }

        float FieldOfViewAngle { get; }

        float FovWithZoom { get; }

        [Obsolete]
        MatrixD ProjectionMatrixForNearObjects { get; }

        [Obsolete]
        float FieldOfViewAngleForNearObjects { get; }

        [Obsolete]
        float FovWithZoomForNearObjects { get; }
    }
}

