namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using VRage.Game.Entity;
    using VRageMath;

    public interface IMyPathfinding
    {
        void DebugDraw();
        IMyPath FindPathGlobal(Vector3D begin, IMyDestinationShape end, MyEntity relativeEntity);
        IMyPathfindingLog GetPathfindingLog();
        bool ReachableUnderThreshold(Vector3D begin, IMyDestinationShape end, float thresholdDistance);
        void UnloadData();
        void Update();
    }
}

