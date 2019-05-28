namespace VRage.Algorithms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class MyPath<V> : IEnumerable<MyPath<V>.PathNode>, IEnumerable where V: class, IMyPathVertex<V>, IEnumerable<IMyPathEdge<V>>
    {
        private List<PathNode<V>> m_vertices;

        internal MyPath(int size)
        {
            this.m_vertices = new List<PathNode<V>>(size);
        }

        internal void Add(IMyPathVertex<V> vertex, IMyPathVertex<V> nextVertex)
        {
            PathNode<V> item = new PathNode<V> {
                Vertex = vertex
            };
            if (nextVertex == null)
            {
                this.m_vertices.Add(item);
            }
            else
            {
                int neighborCount = vertex.GetNeighborCount();
                for (int i = 0; i < neighborCount; i++)
                {
                    IMyPathVertex<V> neighbor = vertex.GetNeighbor(i);
                    if (ReferenceEquals(neighbor, nextVertex))
                    {
                        item.nextVertex = i;
                        this.m_vertices.Add(item);
                        return;
                    }
                }
            }
        }

        public IEnumerator<PathNode<V>> GetEnumerator() => 
            this.m_vertices.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.m_vertices.GetEnumerator();

        public int Count =>
            this.m_vertices.Count;

        public PathNode<V> this[int position]
        {
            get => 
                this.m_vertices[position];
            set => 
                (this.m_vertices[position] = value);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PathNode
        {
            public IMyPathVertex<V> Vertex;
            public int nextVertex;
        }
    }
}

