namespace VRage.Algorithms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IMyPathVertex<V> : IEnumerable<IMyPathEdge<V>>, IEnumerable
    {
        float EstimateDistanceTo(IMyPathVertex<V> other);
        IMyPathEdge<V> GetEdge(int index);
        IMyPathVertex<V> GetNeighbor(int index);
        int GetNeighborCount();

        MyPathfindingData PathfindingData { get; }
    }
}

