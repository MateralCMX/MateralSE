namespace VRageRender.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyPolygon
    {
        private List<Vertex> m_vertices = new List<Vertex>();
        private List<int> m_loops = new List<int>();
        private Plane m_plane;

        public MyPolygon(Plane polygonPlane)
        {
            this.m_plane = polygonPlane;
        }

        public void AddLoop(List<Vector3> loop)
        {
            if (loop.Count >= 3)
            {
                Vertex vertex;
                for (int i = 0; i < loop.Count; i++)
                {
                }
                int count = this.m_vertices.Count;
                int num2 = (this.m_vertices.Count + loop.Count) - 1;
                this.m_loops.Add(count);
                for (int j = 0; j < (loop.Count - 1); j++)
                {
                    vertex = new Vertex {
                        Coord = loop[j],
                        Next = (count + j) + 1,
                        Prev = num2
                    };
                    this.m_vertices.Add(vertex);
                    num2 = count + j;
                }
                vertex = new Vertex {
                    Coord = loop[loop.Count - 1],
                    Next = count,
                    Prev = num2
                };
                this.m_vertices.Add(vertex);
            }
        }

        public void Clear()
        {
            this.m_vertices.Clear();
            this.m_loops.Clear();
        }

        public void DebugDraw(ref MatrixD drawMatrix)
        {
            for (int i = 0; i < this.m_vertices.Count; i++)
            {
                MyRenderProxy.DebugDrawLine3D(this.m_vertices[i].Coord, this.m_vertices[this.m_vertices[i].Next].Coord, Color.DarkRed, Color.DarkRed, false, false);
                MyRenderProxy.DebugDrawPoint(this.m_vertices[i].Coord, Color.Red, false, false);
                MyRenderProxy.DebugDrawText3D(this.m_vertices[i].Coord + (Vector3.Right * 0.05f), i.ToString() + "/" + this.m_vertices.Count.ToString(), Color.Red, 0.45f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
        }

        public LoopIterator GetLoopIterator(int loopIndex) => 
            new LoopIterator(this, this.m_loops[loopIndex]);

        public int GetLoopStart(int loopIndex) => 
            this.m_loops[loopIndex];

        public void GetVertex(int vertexIndex, out Vertex v)
        {
            v = this.m_vertices[vertexIndex];
        }

        public void GetXExtents(out float minX, out float maxX)
        {
            minX = float.PositiveInfinity;
            maxX = float.NegativeInfinity;
            for (int i = 0; i < this.m_vertices.Count; i++)
            {
                float x = this.m_vertices[i].Coord.X;
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
            }
        }

        public void Transform(ref Matrix transformationMatrix)
        {
            for (int i = 0; i < this.m_vertices.Count; i++)
            {
                Vertex vertex = this.m_vertices[i];
                Vector3.Transform(ref vertex.Coord, ref transformationMatrix, out vertex.Coord);
                this.m_vertices[i] = vertex;
            }
        }

        public int VertexCount =>
            this.m_vertices.Count;

        public int LoopCount =>
            this.m_loops.Count;

        public Plane PolygonPlane =>
            this.m_plane;

        [StructLayout(LayoutKind.Sequential)]
        public struct LoopIterator
        {
            private List<MyPolygon.Vertex> m_data;
            private int m_begin;
            private int m_currentIndex;
            private MyPolygon.Vertex m_current;
            public LoopIterator(MyPolygon poly, int loopBegin)
            {
                this.m_data = poly.m_vertices;
                this.m_begin = loopBegin;
                this.m_currentIndex = -1;
                this.m_current = new MyPolygon.Vertex();
                this.m_current.Next = this.m_begin;
            }

            public Vector3 Current =>
                this.m_current.Coord;
            public int CurrentIndex =>
                this.m_currentIndex;
            public bool MoveNext()
            {
                if ((this.m_currentIndex != -1) && (this.m_current.Next == this.m_begin))
                {
                    return false;
                }
                this.m_currentIndex = this.m_current.Next;
                this.m_current = this.m_data[this.m_currentIndex];
                return true;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public Vector3 Coord;
            public int Prev;
            public int Next;
        }
    }
}

