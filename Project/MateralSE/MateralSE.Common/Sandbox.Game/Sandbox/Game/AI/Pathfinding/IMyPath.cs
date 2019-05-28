namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.ModAPI;
    using VRageMath;

    public interface IMyPath
    {
        void DebugDraw();
        bool GetNextTarget(Vector3D position, out Vector3D target, out float targetRadius, out IMyEntity relativeEntity);
        void Invalidate();
        void Reinit(Vector3D position);

        IMyDestinationShape Destination { get; }

        IMyEntity EndEntity { get; }

        bool IsValid { get; }

        bool PathCompleted { get; }
    }
}

