namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Game.Gui;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage;
    using VRageMath;
    using VRageMath.Spatial;
    using VRageRender;
    using VRageRender.Utils;

    public class MyVoxelConnectionHelper
    {
        private Dictionary<InnerEdgeIndex, int> m_innerEdges = new Dictionary<InnerEdgeIndex, int>();
        private MyVector3Grid<OuterEdgePoint> m_outerEdgePoints = new MyVector3Grid<OuterEdgePoint>(1f);
        private Dictionary<int, int> m_innerMultiedges = new Dictionary<int, int>();
        private Dictionary<InnerEdgeIndex, int> m_edgeClassifier = new Dictionary<InnerEdgeIndex, int>();
        private List<OuterEdgePoint> m_tmpOuterEdgePointList = new List<OuterEdgePoint>();
        public static float OUTER_EDGE_EPSILON = 0.05f;
        public static float OUTER_EDGE_EPSILON_SQ = (OUTER_EDGE_EPSILON * OUTER_EDGE_EPSILON);

        public void AddEdgeIndex(ushort iv0, ushort iv1, ref Vector3 posv0, ref Vector3 posv1, int edgeIndex)
        {
            InnerEdgeIndex index = new InnerEdgeIndex(iv0, iv1);
            if (!this.IsInnerEdge(index))
            {
                this.AddOuterEdgeIndex(ref posv0, ref posv1, edgeIndex);
            }
            else
            {
                int num;
                if (!this.m_innerEdges.TryGetValue(index, out num))
                {
                    this.m_innerEdges.Add(index, edgeIndex);
                }
                else
                {
                    this.m_innerMultiedges.Add(edgeIndex, num);
                    this.m_innerEdges[index] = edgeIndex;
                }
            }
        }

        public void AddOuterEdgeIndex(ref Vector3 posv0, ref Vector3 posv1, int edgeIndex)
        {
            this.m_outerEdgePoints.AddPoint(ref posv0, new OuterEdgePoint(edgeIndex, true));
            this.m_outerEdgePoints.AddPoint(ref posv1, new OuterEdgePoint(edgeIndex, false));
        }

        public void ClearCell()
        {
            this.m_innerEdges.Clear();
            this.m_innerMultiedges.Clear();
            this.m_edgeClassifier.Clear();
        }

        [Conditional("DEBUG")]
        public void CollectOuterEdges(List<MyTuple<OuterEdgePoint, Vector3>> output)
        {
            Dictionary<Vector3I, int>.Enumerator enumerator = this.m_outerEdgePoints.EnumerateBins();
            while (enumerator.MoveNext())
            {
                KeyValuePair<Vector3I, int> current = enumerator.Current;
                for (int i = current.Value; i != -1; i = this.m_outerEdgePoints.GetNextBinIndex(i))
                {
                    output.Add(new MyTuple<OuterEdgePoint, Vector3>(this.m_outerEdgePoints.GetData(i), this.m_outerEdgePoints.GetPoint(i)));
                }
            }
        }

        public void DebugDraw(ref Matrix drawMatrix, MyWingedEdgeMesh mesh)
        {
            Dictionary<Vector3I, int>.Enumerator enumerator = this.m_outerEdgePoints.EnumerateBins();
            for (int i = 0; enumerator.MoveNext(); i++)
            {
                int binIndex = MyCestmirDebugInputComponent.BinIndex;
                if ((binIndex == this.m_outerEdgePoints.InvalidIndex) || (i == binIndex))
                {
                    BoundingBoxD xd;
                    BoundingBoxD xd2;
                    Vector3I key = enumerator.Current.Key;
                    int index = enumerator.Current.Value;
                    this.m_outerEdgePoints.GetLocalBinBB(ref key, out xd);
                    xd2.Min = Vector3D.Transform(xd.Min, drawMatrix);
                    xd2.Max = Vector3D.Transform(xd.Max, drawMatrix);
                    while (true)
                    {
                        if (index == this.m_outerEdgePoints.InvalidIndex)
                        {
                            MyRenderProxy.DebugDrawAABB(xd2, Color.PowderBlue, 1f, 1f, false, false, false);
                            break;
                        }
                        Vector3 point = this.m_outerEdgePoints.GetPoint(index);
                        MyWingedEdgeMesh.Edge edge = mesh.GetEdge(this.m_outerEdgePoints.GetData(index).EdgeIndex);
                        Vector3 vertexPosition = mesh.GetVertexPosition(edge.Vertex2);
                        Vector3D pointTo = Vector3D.Transform(point, drawMatrix);
                        MyRenderProxy.DebugDrawArrow3D(Vector3D.Transform((mesh.GetVertexPosition(edge.Vertex1) + vertexPosition) * 0.5f, drawMatrix), pointTo, Color.Yellow, new Color?(Color.Yellow), false, 0.1, null, 0.5f, false);
                        index = this.m_outerEdgePoints.GetNextBinIndex(index);
                    }
                }
            }
        }

        public void FixOuterEdge(int edgeIndex, bool firstPoint, Vector3 currentPosition)
        {
            OuterEdgePoint point1 = new OuterEdgePoint(edgeIndex, firstPoint);
            MyVector3Grid<OuterEdgePoint>.SphereQuery query = this.m_outerEdgePoints.QueryPointsSphere(ref currentPosition, OUTER_EDGE_EPSILON * 3f);
            while (query.MoveNext())
            {
                if (query.Current.EdgeIndex != edgeIndex)
                {
                    continue;
                }
                if (query.Current.FirstPoint == firstPoint)
                {
                    this.m_outerEdgePoints.MovePoint(query.StorageIndex, ref currentPosition);
                }
            }
        }

        private bool IsInnerEdge(InnerEdgeIndex edgeIndex) => 
            (this.m_edgeClassifier[edgeIndex] == 0);

        public bool IsInnerEdge(ushort v0, ushort v1) => 
            this.IsInnerEdge(new InnerEdgeIndex(v0, v1));

        public void PreprocessInnerEdge(ushort a, ushort b)
        {
            int num;
            InnerEdgeIndex key = new InnerEdgeIndex(a, b);
            InnerEdgeIndex index2 = new InnerEdgeIndex(b, a);
            num = this.m_edgeClassifier.TryGetValue(key, out num) ? (num + 1) : 1;
            this.m_edgeClassifier[key] = num;
            num = this.m_edgeClassifier.TryGetValue(index2, out num) ? (num - 1) : -1;
            this.m_edgeClassifier[index2] = num;
        }

        private InnerEdgeIndex RemoveInnerEdge(int formerEdgeIndex, InnerEdgeIndex innerIndex)
        {
            int num;
            if (!this.m_innerMultiedges.TryGetValue(formerEdgeIndex, out num))
            {
                this.m_innerEdges.Remove(innerIndex);
            }
            else
            {
                this.m_innerMultiedges.Remove(formerEdgeIndex);
                this.m_innerEdges[innerIndex] = num;
            }
            return innerIndex;
        }

        public int TryGetAndRemoveEdgeIndex(ushort iv0, ushort iv1, ref Vector3 posv0, ref Vector3 posv1)
        {
            int edgeIndex = -1;
            InnerEdgeIndex key = new InnerEdgeIndex(iv0, iv1);
            if (!this.IsInnerEdge(new InnerEdgeIndex(iv1, iv0)))
            {
                this.TryRemoveOuterEdge(ref posv0, ref posv1, ref edgeIndex);
            }
            else if (!this.m_innerEdges.TryGetValue(key, out edgeIndex))
            {
                edgeIndex = -1;
            }
            else
            {
                this.RemoveInnerEdge(edgeIndex, key);
            }
            return edgeIndex;
        }

        public bool TryRemoveOuterEdge(ref Vector3 posv0, ref Vector3 posv1, ref int edgeIndex)
        {
            if (edgeIndex == -1)
            {
                MyVector3Grid<OuterEdgePoint>.SphereQuery query = this.m_outerEdgePoints.QueryPointsSphere(ref posv0, OUTER_EDGE_EPSILON);
                while (true)
                {
                    if (!query.MoveNext())
                    {
                        edgeIndex = -1;
                        break;
                    }
                    MyVector3Grid<OuterEdgePoint>.SphereQuery query2 = this.m_outerEdgePoints.QueryPointsSphere(ref posv1, OUTER_EDGE_EPSILON);
                    while (query2.MoveNext())
                    {
                        OuterEdgePoint current = query.Current;
                        OuterEdgePoint point2 = query2.Current;
                        if ((current.EdgeIndex == point2.EdgeIndex) && (current.FirstPoint && !point2.FirstPoint))
                        {
                            edgeIndex = current.EdgeIndex;
                            this.m_outerEdgePoints.RemoveTwo(ref query, ref query2);
                            return true;
                        }
                    }
                }
            }
            else
            {
                int num = 0;
                MyVector3Grid<OuterEdgePoint>.SphereQuery query3 = this.m_outerEdgePoints.QueryPointsSphere(ref posv0, OUTER_EDGE_EPSILON);
                while (true)
                {
                    if (query3.MoveNext())
                    {
                        if (query3.Current.EdgeIndex != edgeIndex)
                        {
                            continue;
                        }
                        if (!query3.Current.FirstPoint)
                        {
                            continue;
                        }
                        num++;
                    }
                    MyVector3Grid<OuterEdgePoint>.SphereQuery query4 = this.m_outerEdgePoints.QueryPointsSphere(ref posv1, OUTER_EDGE_EPSILON);
                    while (true)
                    {
                        if (query4.MoveNext())
                        {
                            if (query4.Current.EdgeIndex != edgeIndex)
                            {
                                continue;
                            }
                            if (query4.Current.FirstPoint)
                            {
                                continue;
                            }
                            num++;
                        }
                        if (num != 2)
                        {
                            edgeIndex = -1;
                            break;
                        }
                        this.m_outerEdgePoints.RemoveTwo(ref query3, ref query4);
                        return true;
                    }
                    break;
                }
            }
            return false;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct InnerEdgeIndex : IEquatable<MyVoxelConnectionHelper.InnerEdgeIndex>
        {
            public ushort V0;
            public ushort V1;
            public InnerEdgeIndex(ushort vert0, ushort vert1)
            {
                this.V0 = vert0;
                this.V1 = vert1;
            }

            public override int GetHashCode() => 
                ((this.V0 + this.V1) << 0x10);

            public override bool Equals(object obj) => 
                ((obj is MyVoxelConnectionHelper.InnerEdgeIndex) ? this.Equals((MyVoxelConnectionHelper.InnerEdgeIndex) obj) : false);

            public override string ToString()
            {
                object[] objArray1 = new object[] { "{", this.V0, ", ", this.V1, "}" };
                return string.Concat(objArray1);
            }

            public bool Equals(MyVoxelConnectionHelper.InnerEdgeIndex other) => 
                ((other.V0 == this.V0) && (other.V1 == this.V1));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OuterEdgePoint
        {
            public int EdgeIndex;
            public bool FirstPoint;
            public OuterEdgePoint(int edgeIndex, bool firstPoint)
            {
                this.EdgeIndex = edgeIndex;
                this.FirstPoint = firstPoint;
            }

            public override string ToString()
            {
                object[] objArray1 = new object[4];
                objArray1[0] = "{";
                objArray1[1] = this.EdgeIndex;
                objArray1[2] = this.FirstPoint ? " O--->" : " <---O";
                object[] local1 = objArray1;
                local1[3] = "}";
                return string.Concat(local1);
            }
        }
    }
}

