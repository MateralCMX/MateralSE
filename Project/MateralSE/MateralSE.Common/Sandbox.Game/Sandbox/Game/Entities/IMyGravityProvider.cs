namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    public interface IMyGravityProvider
    {
        float GetGravityMultiplier(Vector3D worldPoint);
        void GetProxyAABB(out BoundingBoxD aabb);
        Vector3 GetWorldGravity(Vector3D worldPoint);
        bool IsPositionInRange(Vector3D worldPoint);

        bool IsWorking { get; }
    }
}

