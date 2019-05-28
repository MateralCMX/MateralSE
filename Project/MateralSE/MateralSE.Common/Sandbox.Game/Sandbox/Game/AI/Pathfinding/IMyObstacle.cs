namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using VRageMath;

    public interface IMyObstacle
    {
        bool Contains(ref Vector3D point);
        void DebugDraw();
        void Update();
    }
}

