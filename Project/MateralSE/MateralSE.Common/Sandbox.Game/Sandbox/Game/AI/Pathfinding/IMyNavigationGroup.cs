namespace Sandbox.Game.AI.Pathfinding
{
    using System;
    using System.Collections.Generic;
    using VRage.Algorithms;
    using VRageMath;

    public interface IMyNavigationGroup
    {
        MyNavigationPrimitive FindClosestPrimitive(Vector3D point, bool highLevel, ref double closestDistanceSq);
        IMyHighLevelComponent GetComponent(MyHighLevelPrimitive highLevelPrimitive);
        IMyPathEdge<MyNavigationPrimitive> GetExternalEdge(MyNavigationPrimitive primitive, int index);
        MyNavigationPrimitive GetExternalNeighbor(MyNavigationPrimitive primitive, int index);
        int GetExternalNeighborCount(MyNavigationPrimitive primitive);
        MyHighLevelPrimitive GetHighLevelPrimitive(MyNavigationPrimitive myNavigationTriangle);
        Vector3 GlobalToLocal(Vector3D globalPos);
        Vector3D LocalToGlobal(Vector3 localPos);
        void RefinePath(MyPath<MyNavigationPrimitive> path, List<Vector4D> output, ref Vector3 startPoint, ref Vector3 endPoint, int begin, int end);

        MyHighLevelGroup HighLevelGroup { get; }
    }
}

