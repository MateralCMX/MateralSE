namespace VRage.Algorithms
{
    using System;

    public interface IMyPathEdge<V>
    {
        V GetOtherVertex(V vertex1);
        float GetWeight();
    }
}

