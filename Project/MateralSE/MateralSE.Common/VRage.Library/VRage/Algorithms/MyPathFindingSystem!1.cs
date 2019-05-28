namespace VRage.Algorithms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Collections;

    public class MyPathFindingSystem<V> : IEnumerable<V>, IEnumerable where V: class, IMyPathVertex<V>
    {
        private long m_timestamp;
        private Func<long> m_timestampFunction;
        private Queue<V> m_bfsQueue;
        private List<V> m_reachableList;
        private MyBinaryHeap<float, MyPathfindingData> m_openVertices;
        private Enumerator<V> m_enumerator;
        private bool m_enumerating;

        public MyPathFindingSystem(int queueInitSize = 0x80, Func<long> timestampFunction = null)
        {
            this.m_bfsQueue = new Queue<V>(queueInitSize);
            this.m_reachableList = new List<V>(0x80);
            this.m_openVertices = new MyBinaryHeap<float, MyPathfindingData>(0x80);
            this.m_timestamp = 0L;
            this.m_timestampFunction = timestampFunction;
            this.m_enumerating = false;
            this.m_enumerator = new Enumerator<V>();
        }

        protected void CalculateNextTimestamp()
        {
            if (this.m_timestampFunction != null)
            {
                this.m_timestamp = this.m_timestampFunction();
            }
            else
            {
                this.m_timestamp += 1L;
            }
        }

        public MyPath<V> FindPath(V start, V end, Predicate<V> vertexTraversable = null, Predicate<IMyPathEdge<V>> edgeTraversable = null)
        {
            this.CalculateNextTimestamp();
            MyPathfindingData pathfindingData = start.PathfindingData;
            this.Visit(pathfindingData);
            pathfindingData.Predecessor = null;
            pathfindingData.PathLength = 0f;
            IMyPathVertex<V> vertex = null;
            float positiveInfinity = float.PositiveInfinity;
            this.m_openVertices.Insert(start.PathfindingData, start.EstimateDistanceTo(end));
            while (true)
            {
                if (this.m_openVertices.Count > 0)
                {
                    MyPathfindingData data2 = this.m_openVertices.RemoveMin();
                    V parent = data2.Parent as V;
                    float pathLength = data2.PathLength;
                    if ((vertex == null) || (pathLength < positiveInfinity))
                    {
                        for (int i = 0; i < parent.GetNeighborCount(); i++)
                        {
                            IMyPathEdge<V> edge = parent.GetEdge(i);
                            if ((edge != null) && ((edgeTraversable == null) || edgeTraversable(edge)))
                            {
                                V otherVertex = edge.GetOtherVertex(parent);
                                if ((otherVertex != null) && ((vertexTraversable == null) || vertexTraversable(otherVertex)))
                                {
                                    float num4 = data2.PathLength + edge.GetWeight();
                                    MyPathfindingData vertexData = otherVertex.PathfindingData;
                                    if ((otherVertex == end) && (num4 < positiveInfinity))
                                    {
                                        vertex = otherVertex;
                                        positiveInfinity = num4;
                                    }
                                    if (!this.Visited(vertexData))
                                    {
                                        this.Visit(vertexData);
                                        vertexData.PathLength = num4;
                                        vertexData.Predecessor = data2;
                                        this.m_openVertices.Insert(vertexData, num4 + otherVertex.EstimateDistanceTo(end));
                                    }
                                    else if (num4 < vertexData.PathLength)
                                    {
                                        vertexData.PathLength = num4;
                                        vertexData.Predecessor = data2;
                                        this.m_openVertices.ModifyUp(vertexData, num4 + otherVertex.EstimateDistanceTo(end));
                                    }
                                }
                            }
                        }
                        continue;
                    }
                }
                this.m_openVertices.Clear();
                return ((vertex != null) ? this.ReturnPath(vertex.PathfindingData, null, 0) : null);
            }
        }

        public MyPath<V> FindPath(V start, Func<V, float> heuristic, Func<V, float> terminationCriterion, Predicate<V> vertexTraversable = null, bool returnClosest = true)
        {
            this.CalculateNextTimestamp();
            MyPathfindingData pathfindingData = start.PathfindingData;
            this.Visit(pathfindingData);
            pathfindingData.Predecessor = null;
            pathfindingData.PathLength = 0f;
            IMyPathVertex<V> vertex = null;
            float positiveInfinity = float.PositiveInfinity;
            IMyPathVertex<V> vertex2 = null;
            float num2 = float.PositiveInfinity;
            float num3 = terminationCriterion(start);
            if (num3 != float.PositiveInfinity)
            {
                vertex = start;
                positiveInfinity = heuristic(start) + num3;
            }
            this.m_openVertices.Insert(start.PathfindingData, heuristic(start));
            while (true)
            {
                if (this.m_openVertices.Count > 0)
                {
                    MyPathfindingData data2 = this.m_openVertices.RemoveMin();
                    V parent = data2.Parent as V;
                    float pathLength = data2.PathLength;
                    if ((vertex == null) || ((pathLength + heuristic(parent)) < positiveInfinity))
                    {
                        for (int i = 0; i < parent.GetNeighborCount(); i++)
                        {
                            IMyPathEdge<V> edge = parent.GetEdge(i);
                            if (edge != null)
                            {
                                V otherVertex = edge.GetOtherVertex(parent);
                                if ((otherVertex != null) && ((vertexTraversable == null) || vertexTraversable(otherVertex)))
                                {
                                    float num6 = data2.PathLength + edge.GetWeight();
                                    MyPathfindingData vertexData = otherVertex.PathfindingData;
                                    float key = num6 + heuristic(otherVertex);
                                    if (key < num2)
                                    {
                                        vertex2 = otherVertex;
                                        num2 = key;
                                    }
                                    num3 = terminationCriterion(otherVertex);
                                    if ((key + num3) < positiveInfinity)
                                    {
                                        vertex = otherVertex;
                                        positiveInfinity = key + num3;
                                    }
                                    if (!this.Visited(vertexData))
                                    {
                                        this.Visit(vertexData);
                                        vertexData.PathLength = num6;
                                        vertexData.Predecessor = data2;
                                        this.m_openVertices.Insert(vertexData, key);
                                    }
                                    else if (num6 < vertexData.PathLength)
                                    {
                                        vertexData.PathLength = num6;
                                        vertexData.Predecessor = data2;
                                        this.m_openVertices.ModifyUp(vertexData, key);
                                    }
                                }
                            }
                        }
                        continue;
                    }
                }
                this.m_openVertices.Clear();
                if (vertex != null)
                {
                    return this.ReturnPath(vertex.PathfindingData, null, 0);
                }
                if (!returnClosest || (vertex2 == null))
                {
                    return null;
                }
                return this.ReturnPath(vertex2.PathfindingData, null, 0);
            }
        }

        public void FindReachable(IEnumerable<V> fromSet, List<V> reachableVertices, Predicate<V> vertexFilter = null, Predicate<V> vertexTraversable = null, Predicate<IMyPathEdge<V>> edgeTraversable = null)
        {
            this.CalculateNextTimestamp();
            foreach (V local in fromSet)
            {
                if (!this.Visited(local))
                {
                    this.FindReachableInternal(local, reachableVertices, vertexFilter, vertexTraversable, edgeTraversable);
                }
            }
        }

        public void FindReachable(V from, List<V> reachableVertices, Predicate<V> vertexFilter = null, Predicate<V> vertexTraversable = null, Predicate<IMyPathEdge<V>> edgeTraversable = null)
        {
            this.FindReachableInternal(from, reachableVertices, vertexFilter, vertexTraversable, edgeTraversable);
        }

        private void FindReachableInternal(V from, List<V> reachableVertices, Predicate<V> vertexFilter = null, Predicate<V> vertexTraversable = null, Predicate<IMyPathEdge<V>> edgeTraversable = null)
        {
            this.PrepareTraversal(from, vertexFilter, vertexTraversable, edgeTraversable);
            foreach (V local in this)
            {
                reachableVertices.Add(local);
            }
        }

        public long GetCurrentTimestamp() => 
            this.m_timestamp;

        public Enumerator<V> GetEnumerator() => 
            this.GetEnumeratorInternal();

        private Enumerator<V> GetEnumeratorInternal() => 
            this.m_enumerator;

        public void PerformTraversal()
        {
            while (this.m_enumerator.MoveNext())
            {
            }
            this.m_enumerator.Dispose();
        }

        public void PrepareTraversal(V startingVertex, Predicate<V> vertexFilter = null, Predicate<V> vertexTraversable = null, Predicate<IMyPathEdge<V>> edgeTraversable = null)
        {
            this.m_enumerator.Init((MyPathFindingSystem<V>) this, startingVertex, vertexFilter, vertexTraversable, edgeTraversable);
        }

        public bool Reachable(V from, V to)
        {
            this.PrepareTraversal(from, null, null, null);
            using (Enumerator<V> enumerator = this.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    V current = enumerator.Current;
                    if (current.Equals(to))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private MyPath<V> ReturnPath(MyPathfindingData vertexData, MyPathfindingData successor, int remainingVertices)
        {
            V local;
            V parent;
            if (vertexData.Predecessor == null)
            {
                V parent;
                MyPath<V> path = new MyPath<V>(remainingVertices + 1);
                if (successor != null)
                {
                    parent = successor.Parent as V;
                }
                else
                {
                    local = default(V);
                    parent = local;
                }
                path.Add(vertexData.Parent as V, parent);
                return path;
            }
            MyPath<V> path2 = this.ReturnPath(vertexData.Predecessor, vertexData, remainingVertices + 1);
            if (successor != null)
            {
                parent = successor.Parent as V;
            }
            else
            {
                local = default(V);
                parent = local;
            }
            path2.Add(vertexData.Parent as V, parent);
            return path2;
        }

        IEnumerator<V> IEnumerable<V>.GetEnumerator() => 
            this.GetEnumeratorInternal();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumeratorInternal();

        private void Visit(V vertex)
        {
            vertex.PathfindingData.Timestamp = this.m_timestamp;
        }

        private void Visit(MyPathfindingData vertexData)
        {
            vertexData.Timestamp = this.m_timestamp;
        }

        private bool Visited(V vertex) => 
            (vertex.PathfindingData.Timestamp == this.m_timestamp);

        private bool Visited(MyPathfindingData vertexData) => 
            (vertexData.Timestamp == this.m_timestamp);

        public bool VisitedBetween(V vertex, long start, long end) => 
            ((vertex.PathfindingData.Timestamp >= start) && (vertex.PathfindingData.Timestamp <= end));

        public class Enumerator : IEnumerator<V>, IDisposable, IEnumerator
        {
            private V m_currentVertex;
            private MyPathFindingSystem<V> m_parent;
            private Predicate<V> m_vertexFilter;
            private Predicate<V> m_vertexTraversable;
            private Predicate<IMyPathEdge<V>> m_edgeTraversable;

            public void Dispose()
            {
                this.m_vertexFilter = null;
                this.m_currentVertex = default(V);
                this.m_edgeTraversable = null;
                this.m_vertexTraversable = null;
                this.m_parent.m_enumerating = false;
                this.m_parent.m_bfsQueue.Clear();
                this.m_parent = null;
            }

            public void Init(MyPathFindingSystem<V> parent, V startingVertex, Predicate<V> vertexFilter = null, Predicate<V> vertexTraversable = null, Predicate<IMyPathEdge<V>> edgeTraversable = null)
            {
                this.m_parent = parent;
                this.m_vertexFilter = vertexFilter;
                this.m_vertexTraversable = vertexTraversable;
                this.m_edgeTraversable = edgeTraversable;
                this.m_parent.CalculateNextTimestamp();
                this.m_parent.m_enumerating = true;
                this.m_parent.m_bfsQueue.Enqueue(startingVertex);
                startingVertex.PathfindingData.Timestamp = this.m_parent.m_timestamp;
            }

            public bool MoveNext()
            {
                V otherVertex;
                int num;
            TR_0014:
                while (true)
                {
                    if (this.m_parent.m_bfsQueue.Count<V>() == 0)
                    {
                        return false;
                    }
                    this.m_currentVertex = this.m_parent.m_bfsQueue.Dequeue();
                    otherVertex = default(V);
                    num = 0;
                    break;
                }
                goto TR_0011;
            TR_0004:
                num++;
            TR_0011:
                while (true)
                {
                    if (num >= this.m_currentVertex.GetNeighborCount())
                    {
                        if ((this.m_vertexFilter != null) && !this.m_vertexFilter(this.m_currentVertex))
                        {
                            break;
                        }
                        return true;
                    }
                    if (this.m_edgeTraversable != null)
                    {
                        IMyPathEdge<V> edge = this.m_currentVertex.GetEdge(num);
                        if (!this.m_edgeTraversable(edge))
                        {
                            goto TR_0004;
                        }
                        else
                        {
                            otherVertex = edge.GetOtherVertex(this.m_currentVertex);
                            if (otherVertex == null)
                            {
                                goto TR_0004;
                            }
                        }
                    }
                    else
                    {
                        otherVertex = (V) this.m_currentVertex.GetNeighbor(num);
                        if (otherVertex == null)
                        {
                            goto TR_0004;
                        }
                    }
                    if ((otherVertex.PathfindingData.Timestamp != this.m_parent.m_timestamp) && ((this.m_vertexTraversable == null) || this.m_vertexTraversable(otherVertex)))
                    {
                        this.m_parent.m_bfsQueue.Enqueue(otherVertex);
                        otherVertex.PathfindingData.Timestamp = this.m_parent.m_timestamp;
                    }
                    goto TR_0004;
                }
                goto TR_0014;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public V Current =>
                this.m_currentVertex;

            object IEnumerator.Current =>
                this.m_currentVertex;
        }
    }
}

