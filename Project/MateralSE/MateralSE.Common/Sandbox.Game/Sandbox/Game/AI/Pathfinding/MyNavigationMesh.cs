namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Engine.Utils;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Algorithms;
    using VRage.Generics;
    using VRageMath;
    using VRageRender;
    using VRageRender.Utils;

    public abstract class MyNavigationMesh : MyPathFindingSystem<MyNavigationPrimitive>, IMyNavigationGroup
    {
        private MyDynamicObjectPool<MyNavigationTriangle> m_triPool;
        private MyWingedEdgeMesh m_mesh;
        private MyNavgroupLinks m_externalLinks;
        private Vector3 m_vertex;
        private Vector3 m_left;
        private Vector3 m_right;
        private Vector3 m_normal;
        private List<Vector3> m_vertexList;
        private static List<Vector3> m_debugPointsLeft = new List<Vector3>();
        private static List<Vector3> m_debugPointsRight = new List<Vector3>();
        private static List<Vector3> m_path = new List<Vector3>();
        private static List<Vector3> m_path2;
        private static List<FunnelState> m_debugFunnel = new List<FunnelState>();
        public static int m_debugFunnelIdx = 0;

        public MyNavigationMesh(MyNavgroupLinks externalLinks, int trianglePrealloc = 0x10, Func<long> timestampFunction = null) : base(0x80, timestampFunction)
        {
            this.m_vertexList = new List<Vector3>();
            this.m_triPool = new MyDynamicObjectPool<MyNavigationTriangle>(trianglePrealloc);
            this.m_mesh = new MyWingedEdgeMesh();
            this.m_externalLinks = externalLinks;
        }

        protected MyNavigationTriangle AddTriangle(ref Vector3 A, ref Vector3 B, ref Vector3 C, ref int edgeAB, ref int edgeBC, ref int edgeCA)
        {
            MyNavigationTriangle userData = this.m_triPool.Allocate();
            int num = ((0 + ((edgeAB == -1) ? 1 : 0)) + ((edgeBC == -1) ? 1 : 0)) + ((edgeCA == -1) ? 1 : 0);
            int triangleIndex = -1;
            if (num == 3)
            {
                triangleIndex = this.m_mesh.MakeNewTriangle(userData, ref A, ref B, ref C, out edgeAB, out edgeBC, out edgeCA);
            }
            else if (num == 2)
            {
                triangleIndex = (edgeAB == -1) ? ((edgeBC == -1) ? this.m_mesh.ExtrudeTriangleFromEdge(ref B, edgeCA, userData, out edgeAB, out edgeBC) : this.m_mesh.ExtrudeTriangleFromEdge(ref A, edgeBC, userData, out edgeCA, out edgeAB)) : this.m_mesh.ExtrudeTriangleFromEdge(ref C, edgeAB, userData, out edgeBC, out edgeCA);
            }
            else if (num == 1)
            {
                triangleIndex = (edgeAB != -1) ? ((edgeBC != -1) ? this.GetTriangleOneNewEdge(ref edgeCA, ref edgeAB, ref edgeBC, userData) : this.GetTriangleOneNewEdge(ref edgeBC, ref edgeCA, ref edgeAB, userData)) : this.GetTriangleOneNewEdge(ref edgeAB, ref edgeBC, ref edgeCA, userData);
            }
            else
            {
                MyWingedEdgeMesh.Edge other = this.m_mesh.GetEdge(edgeAB);
                MyWingedEdgeMesh.Edge edge = this.m_mesh.GetEdge(edgeBC);
                MyWingedEdgeMesh.Edge edge3 = this.m_mesh.GetEdge(edgeCA);
                int sharedB = edge3.TryGetSharedVertex(ref other);
                int sharedC = other.TryGetSharedVertex(ref edge);
                int num5 = edge.TryGetSharedVertex(ref edge3);
                int num6 = ((0 + ((sharedB == -1) ? 0 : 1)) + ((sharedC == -1) ? 0 : 1)) + ((num5 == -1) ? 0 : 1);
                if (num6 == 3)
                {
                    triangleIndex = this.m_mesh.MakeFace(userData, edgeAB);
                }
                else if (num6 == 2)
                {
                    triangleIndex = (sharedB != -1) ? ((sharedC != -1) ? this.GetTriangleTwoSharedVertices(edgeCA, edgeAB, ref edgeBC, sharedB, sharedC, userData) : this.GetTriangleTwoSharedVertices(edgeBC, edgeCA, ref edgeAB, num5, sharedB, userData)) : this.GetTriangleTwoSharedVertices(edgeAB, edgeBC, ref edgeCA, sharedC, num5, userData);
                }
                else if (num6 == 1)
                {
                    triangleIndex = (sharedB == -1) ? ((sharedC == -1) ? this.GetTriangleOneSharedVertex(edgeBC, edgeCA, ref edgeAB, num5, userData) : this.GetTriangleOneSharedVertex(edgeAB, edgeBC, ref edgeCA, sharedC, userData)) : this.GetTriangleOneSharedVertex(edgeCA, edgeAB, ref edgeBC, sharedB, userData);
                }
                else
                {
                    int num7;
                    int num8;
                    triangleIndex = this.m_mesh.ExtrudeTriangleFromEdge(ref C, edgeAB, userData, out num7, out num8);
                    this.m_mesh.MergeEdges(num8, edgeCA);
                    this.m_mesh.MergeEdges(num7, edgeBC);
                }
            }
            userData.Init(this, triangleIndex);
            return userData;
        }

        public int ApproximateMemoryFootprint() => 
            (this.m_mesh.ApproximateMemoryFootprint() + (this.m_triPool.Count * (Environment.Is64BitProcess ? 0x58 : 0x38)));

        [Conditional("DEBUG")]
        public void CheckMeshConsistency()
        {
        }

        [Conditional("DEBUG")]
        public virtual void DebugDraw(ref Matrix drawMatrix)
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES != MyWEMDebugDrawMode.NONE)
                {
                    this.m_mesh.DebugDraw(ref drawMatrix, MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES);
                    this.m_mesh.CustomDebugDrawFaces(ref drawMatrix, MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES, obj => (obj as MyNavigationTriangle).Index.ToString());
                }
                if (MyFakes.DEBUG_DRAW_FUNNEL)
                {
                    List<Vector3>.Enumerator enumerator;
                    MyRenderProxy.DebugDrawSphere(Vector3.Transform(this.m_vertex, (Matrix) drawMatrix), 0.05f, Color.Yellow.ToVector3(), 1f, false, false, true, false);
                    MyRenderProxy.DebugDrawSphere(Vector3.Transform(this.m_vertex + this.m_normal, (Matrix) drawMatrix), 0.05f, Color.Orange.ToVector3(), 1f, false, false, true, false);
                    MyRenderProxy.DebugDrawSphere(Vector3.Transform(this.m_left, (Matrix) drawMatrix), 0.05f, Color.Red.ToVector3(), 1f, false, false, true, false);
                    Color green = Color.Green;
                    MyRenderProxy.DebugDrawSphere(Vector3.Transform(this.m_right, (Matrix) drawMatrix), 0.05f, green.ToVector3(), 1f, false, false, true, false);
                    using (enumerator = m_debugPointsLeft.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            green = Color.Red;
                            MyRenderProxy.DebugDrawSphere(Vector3.Transform(enumerator.Current, (Matrix) drawMatrix), 0.03f, green.ToVector3(), 1f, false, false, true, false);
                        }
                    }
                    using (enumerator = m_debugPointsRight.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            green = Color.Green;
                            MyRenderProxy.DebugDrawSphere(Vector3.Transform(enumerator.Current, (Matrix) drawMatrix), 0.04f, green.ToVector3(), 1f, false, false, true, false);
                        }
                    }
                    Vector3? nullable = null;
                    if (m_path != null)
                    {
                        using (enumerator = m_path.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                Vector3 vector = Vector3.Transform(enumerator.Current, (Matrix) drawMatrix);
                                MyRenderProxy.DebugDrawSphere(vector + (Vector3.Up * 0.2f), 0.02f, Color.Orange.ToVector3(), 1f, false, false, true, false);
                                if (nullable != null)
                                {
                                    MyRenderProxy.DebugDrawLine3D(nullable.Value + (Vector3.Up * 0.2f), vector + (Vector3.Up * 0.2f), Color.Orange, Color.Orange, true, false);
                                }
                                nullable = new Vector3?(vector);
                            }
                        }
                    }
                    nullable = null;
                    if (m_path2 != null)
                    {
                        using (enumerator = m_path2.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                Vector3 vector2 = Vector3.Transform(enumerator.Current, (Matrix) drawMatrix);
                                if (nullable != null)
                                {
                                    MyRenderProxy.DebugDrawLine3D(nullable.Value + (Vector3.Up * 0.1f), vector2 + (Vector3.Up * 0.1f), Color.Violet, Color.Violet, true, false);
                                }
                                nullable = new Vector3?(vector2);
                            }
                        }
                    }
                    if (m_debugFunnel.Count > 0)
                    {
                        FunnelState local2 = m_debugFunnel[m_debugFunnelIdx % m_debugFunnel.Count];
                        Vector3 vector3 = Vector3.Transform(local2.Apex, (Matrix) drawMatrix);
                        Vector3 vector4 = vector3 + ((Vector3.Transform(local2.Left, (Matrix) drawMatrix) - vector3) * 10f);
                        Vector3 vector5 = vector3 + ((Vector3.Transform(local2.Right, (Matrix) drawMatrix) - vector3) * 10f);
                        Color cyan = Color.Cyan;
                        MyRenderProxy.DebugDrawLine3D(vector3 + (Vector3.Up * 0.1f), vector4 + (Vector3.Up * 0.1f), cyan, cyan, true, false);
                        MyRenderProxy.DebugDrawLine3D(vector3 + (Vector3.Up * 0.1f), vector5 + (Vector3.Up * 0.1f), cyan, cyan, true, false);
                    }
                }
            }
        }

        public void ErasePools()
        {
            this.m_triPool = null;
        }

        public abstract MyNavigationPrimitive FindClosestPrimitive(Vector3D point, bool highLevel, ref double closestDistanceSq);
        protected List<Vector4D> FindRefinedPath(MyNavigationTriangle start, MyNavigationTriangle end, ref Vector3 startPoint, ref Vector3 endPoint)
        {
            MyPath<MyNavigationPrimitive> inputPath = base.FindPath(start, end, null, null);
            if (inputPath == null)
            {
                return null;
            }
            List<Vector4D> refinedPath = new List<Vector4D> {
                new Vector4D(startPoint, 1.0)
            };
            new Funnel().Calculate(inputPath, refinedPath, ref startPoint, ref endPoint, 0, inputPath.Count - 1);
            m_path.Clear();
            foreach (Vector4D vectord in refinedPath)
            {
                m_path.Add((Vector3) new Vector3D(vectord));
            }
            return refinedPath;
        }

        public abstract IMyHighLevelComponent GetComponent(MyHighLevelPrimitive highLevelPrimitive);
        protected MyNavigationTriangle GetEdgeTriangle(int edgeIndex)
        {
            MyWingedEdgeMesh.Edge edge = this.m_mesh.GetEdge(edgeIndex);
            return ((edge.LeftFace != -1) ? this.GetTriangle(edge.LeftFace) : this.GetTriangle(edge.RightFace));
        }

        public IMyPathEdge<MyNavigationPrimitive> GetExternalEdge(MyNavigationPrimitive primitive, int index) => 
            this.m_externalLinks?.GetEdge(primitive, index);

        public MyNavigationPrimitive GetExternalNeighbor(MyNavigationPrimitive primitive, int index) => 
            this.m_externalLinks?.GetLinkedNeighbor(primitive, index);

        public int GetExternalNeighborCount(MyNavigationPrimitive primitive) => 
            ((this.m_externalLinks == null) ? 0 : this.m_externalLinks.GetLinkCount(primitive));

        public abstract MyHighLevelPrimitive GetHighLevelPrimitive(MyNavigationPrimitive myNavigationTriangle);
        public MyNavigationTriangle GetTriangle(int index) => 
            this.m_mesh.GetFace(index).GetUserData<MyNavigationTriangle>();

        private int GetTriangleOneNewEdge(ref int newEdge, ref int succ, ref int pred, MyNavigationTriangle newTri)
        {
            MyWingedEdgeMesh.Edge edge = this.m_mesh.GetEdge(pred);
            MyWingedEdgeMesh.Edge other = this.m_mesh.GetEdge(succ);
            int vertex = edge.TryGetSharedVertex(ref other);
            if (vertex != -1)
            {
                int num3 = edge.OtherVertex(vertex);
                return this.m_mesh.MakeEdgeFace(num3, other.OtherVertex(vertex), pred, succ, newTri, out newEdge);
            }
            int num2 = succ;
            Vector3 vertexPosition = this.m_mesh.GetVertexPosition(other.GetFacePredVertex(-1));
            this.m_mesh.MergeEdges(num2, succ);
            return this.m_mesh.ExtrudeTriangleFromEdge(ref vertexPosition, pred, newTri, out newEdge, out succ);
        }

        private int GetTriangleOneSharedVertex(int edgeCA, int edgeAB, ref int edgeBC, int sharedA, MyNavigationTriangle newTri)
        {
            int num = this.m_mesh.GetEdge(edgeAB).OtherVertex(sharedA);
            int num3 = edgeBC;
            this.m_mesh.MergeEdges(num3, edgeBC);
            return this.m_mesh.MakeEdgeFace(num, this.m_mesh.GetEdge(edgeCA).OtherVertex(sharedA), edgeAB, edgeCA, newTri, out edgeBC);
        }

        private int GetTriangleTwoSharedVertices(int edgeAB, int edgeBC, ref int edgeCA, int sharedB, int sharedC, MyNavigationTriangle newTri)
        {
            int num = this.m_mesh.GetEdge(edgeAB).OtherVertex(sharedB);
            int leftEdge = edgeCA;
            this.m_mesh.MergeAngle(leftEdge, edgeCA, sharedC);
            return this.m_mesh.MakeEdgeFace(sharedC, num, edgeBC, edgeAB, newTri, out edgeCA);
        }

        public virtual MatrixD GetWorldMatrix() => 
            MatrixD.Identity;

        public abstract Vector3 GlobalToLocal(Vector3D globalPos);
        public abstract Vector3D LocalToGlobal(Vector3 localPos);
        public void RefinePath(MyPath<MyNavigationPrimitive> path, List<Vector4D> output, ref Vector3 startPoint, ref Vector3 endPoint, int begin, int end)
        {
            new Funnel().Calculate(path, output, ref startPoint, ref endPoint, begin, end);
        }

        public void RemoveFace(int index)
        {
            this.m_mesh.RemoveFace(index);
        }

        protected void RemoveTriangle(MyNavigationTriangle tri)
        {
            this.m_mesh.RemoveFace(tri.Index);
            this.m_triPool.Deallocate(tri);
        }

        public MyWingedEdgeMesh Mesh =>
            this.m_mesh;

        public abstract MyHighLevelGroup HighLevelGroup { get; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyNavigationMesh.<>c <>9 = new MyNavigationMesh.<>c();
            public static Func<object, string> <>9__36_0;

            internal string <DebugDraw>b__36_0(object obj) => 
                (obj as MyNavigationTriangle).Index.ToString();
        }

        private class Funnel
        {
            private Vector3 m_end;
            private int m_endIndex;
            private MyPath<MyNavigationPrimitive> m_input;
            private List<Vector4D> m_output;
            private Vector3 m_apex;
            private Vector3 m_apexNormal;
            private Vector3 m_leftPoint;
            private Vector3 m_rightPoint;
            private int m_leftIndex;
            private int m_rightIndex;
            private Vector3 m_leftPlaneNormal;
            private Vector3 m_rightPlaneNormal;
            private float m_leftD;
            private float m_rightD;
            private bool m_funnelConstructed;
            private bool m_segmentDangerous;
            private static float SAFE_DISTANCE = 0.7f;
            private static float SAFE_DISTANCE_SQ = (SAFE_DISTANCE * SAFE_DISTANCE);
            private static float SAFE_DISTANCE2_SQ = ((SAFE_DISTANCE + SAFE_DISTANCE) * (SAFE_DISTANCE + SAFE_DISTANCE));

            private void AddPoint(Vector3D point)
            {
                float num = this.m_segmentDangerous ? 0.5f : 2f;
                this.m_output.Add(new Vector4D(point, (double) num));
                int num2 = this.m_output.Count - 1;
                if (num2 >= 0)
                {
                    Vector4D vectord = this.m_output[num2];
                    if (vectord.W > num)
                    {
                        vectord.W = num;
                        this.m_output[num2] = vectord;
                    }
                }
                this.m_segmentDangerous = false;
            }

            private int AddTriangle(int index)
            {
                if (!this.m_funnelConstructed)
                {
                    this.ConstructFunnel(index);
                }
                else
                {
                    Vector3 vector;
                    Vector3 vector2;
                    MyPath<MyNavigationPrimitive>.PathNode node = this.m_input[index];
                    MyNavigationTriangle vertex = node.Vertex as MyNavigationTriangle;
                    vertex.GetNavigationEdge(node.nextVertex);
                    this.GetEdgeVerticesSafe(vertex, node.nextVertex, out vector, out vector2);
                    PointTestResult result = this.TestPoint(vector);
                    PointTestResult result2 = this.TestPoint(vector2);
                    if (result == PointTestResult.INSIDE)
                    {
                        this.NarrowFunnel(vector, index, true);
                    }
                    if (result2 == PointTestResult.INSIDE)
                    {
                        this.NarrowFunnel(vector2, index, false);
                    }
                    if (result == PointTestResult.RIGHT)
                    {
                        this.m_apex = this.m_rightPoint;
                        this.m_funnelConstructed = false;
                        this.ConstructFunnel(this.m_rightIndex + 1);
                        return (this.m_rightIndex + 1);
                    }
                    if (result2 == PointTestResult.LEFT)
                    {
                        this.m_apex = this.m_leftPoint;
                        this.m_funnelConstructed = false;
                        this.ConstructFunnel(this.m_leftIndex + 1);
                        return (this.m_leftIndex + 1);
                    }
                    if ((result == PointTestResult.INSIDE) || (result2 == PointTestResult.INSIDE))
                    {
                        MyNavigationMesh.FunnelState item = new MyNavigationMesh.FunnelState {
                            Apex = this.m_apex,
                            Left = this.m_leftPoint,
                            Right = this.m_rightPoint
                        };
                        MyNavigationMesh.m_debugFunnel.Add(item);
                    }
                }
                return (index + 1);
            }

            public void Calculate(MyPath<MyNavigationPrimitive> inputPath, List<Vector4D> refinedPath, ref Vector3 start, ref Vector3 end, int startIndex, int endIndex)
            {
                MyNavigationMesh.m_debugFunnel.Clear();
                MyNavigationMesh.m_debugPointsLeft.Clear();
                MyNavigationMesh.m_debugPointsRight.Clear();
                this.m_end = end;
                this.m_endIndex = endIndex;
                this.m_input = inputPath;
                this.m_output = refinedPath;
                this.m_apex = start;
                this.m_funnelConstructed = false;
                this.m_segmentDangerous = false;
                int index = startIndex;
                while (index < endIndex)
                {
                    index = this.AddTriangle(index);
                    if (index == endIndex)
                    {
                        PointTestResult result = this.TestPoint(end);
                        if (result == PointTestResult.LEFT)
                        {
                            this.m_apex = this.m_leftPoint;
                            this.m_funnelConstructed = false;
                            this.ConstructFunnel(this.m_leftIndex);
                            index = this.m_leftIndex + 1;
                        }
                        else if (result == PointTestResult.RIGHT)
                        {
                            this.m_apex = this.m_rightPoint;
                            this.m_funnelConstructed = false;
                            this.ConstructFunnel(this.m_rightIndex);
                            index = this.m_rightIndex + 1;
                        }
                        if ((result == PointTestResult.INSIDE) || (index == endIndex))
                        {
                            this.AddPoint(this.ProjectEndOnTriangle(index));
                        }
                    }
                }
                if (startIndex == endIndex)
                {
                    this.AddPoint(this.ProjectEndOnTriangle(index));
                }
                this.m_input = null;
                this.m_output = null;
            }

            private void ConstructFunnel(int index)
            {
                if (index >= this.m_endIndex)
                {
                    this.AddPoint(this.m_apex);
                }
                else
                {
                    MyPath<MyNavigationPrimitive>.PathNode node = this.m_input[index];
                    MyNavigationTriangle vertex = node.Vertex as MyNavigationTriangle;
                    vertex.GetNavigationEdge(node.nextVertex);
                    this.GetEdgeVerticesSafe(vertex, node.nextVertex, out this.m_leftPoint, out this.m_rightPoint);
                    if (Vector3.IsZero(this.m_leftPoint - this.m_apex))
                    {
                        this.m_apex = vertex.Center;
                    }
                    else if (Vector3.IsZero(this.m_rightPoint - this.m_apex))
                    {
                        this.m_apex = vertex.Center;
                    }
                    else
                    {
                        this.m_apexNormal = vertex.Normal;
                        float num = this.m_leftPoint.Dot(this.m_apexNormal);
                        this.m_apex -= this.m_apexNormal * (this.m_apex.Dot(this.m_apexNormal) - num);
                        this.m_leftIndex = this.m_rightIndex = index;
                        this.RecalculateLeftPlane();
                        this.RecalculateRightPlane();
                        this.m_funnelConstructed = true;
                        this.AddPoint(this.m_apex);
                        MyNavigationMesh.FunnelState item = new MyNavigationMesh.FunnelState {
                            Apex = this.m_apex,
                            Left = this.m_leftPoint,
                            Right = this.m_rightPoint
                        };
                        MyNavigationMesh.m_debugFunnel.Add(item);
                    }
                }
            }

            private void GetEdgeVerticesSafe(MyNavigationTriangle triangle, int edgeIndex, out Vector3 left, out Vector3 right)
            {
                triangle.GetEdgeVertices(edgeIndex, out left, out right);
                float num = (left - right).LengthSquared();
                bool flag = triangle.IsEdgeVertexDangerous(edgeIndex, true);
                bool flag2 = triangle.IsEdgeVertexDangerous(edgeIndex, false);
                this.m_segmentDangerous |= flag | flag2;
                if (!flag)
                {
                    if (flag2)
                    {
                        if (SAFE_DISTANCE_SQ > num)
                        {
                            right = left;
                        }
                        else
                        {
                            float num4 = SAFE_DISTANCE / ((float) Math.Sqrt((double) num));
                            right = (left * num4) + (right * (1f - num4));
                        }
                    }
                }
                else if (!flag2)
                {
                    if (SAFE_DISTANCE_SQ > num)
                    {
                        left = right;
                    }
                    else
                    {
                        float num3 = SAFE_DISTANCE / ((float) Math.Sqrt((double) num));
                        left = (right * num3) + (left * (1f - num3));
                    }
                }
                else if (SAFE_DISTANCE2_SQ > num)
                {
                    left = (left + right) * 0.5f;
                    right = left;
                }
                else
                {
                    float num2 = SAFE_DISTANCE / ((float) Math.Sqrt((double) num));
                    Vector3 vector2 = (right * num2) + (left * (1f - num2));
                    right = (left * num2) + (right * (1f - num2));
                    left = vector2;
                }
                MyNavigationMesh.m_debugPointsLeft.Add(left);
                MyNavigationMesh.m_debugPointsRight.Add(right);
            }

            private void NarrowFunnel(Vector3 point, int index, bool left)
            {
                if (left)
                {
                    this.m_leftPoint = point;
                    this.m_leftIndex = index;
                    this.RecalculateLeftPlane();
                }
                else
                {
                    this.m_rightPoint = point;
                    this.m_rightIndex = index;
                    this.RecalculateRightPlane();
                }
            }

            private Vector3 ProjectEndOnTriangle(int i) => 
                (this.m_input[i].Vertex as MyNavigationTriangle).ProjectLocalPoint(this.m_end);

            private void RecalculateLeftPlane()
            {
                Vector3 vector = this.m_leftPoint - this.m_apex;
                vector.Normalize();
                this.m_leftPlaneNormal = Vector3.Cross(vector, this.m_apexNormal);
                this.m_leftPlaneNormal.Normalize();
                this.m_leftD = -this.m_leftPoint.Dot(this.m_leftPlaneNormal);
            }

            private void RecalculateRightPlane()
            {
                Vector3 vector = this.m_rightPoint - this.m_apex;
                vector.Normalize();
                this.m_rightPlaneNormal = Vector3.Cross(this.m_apexNormal, vector);
                this.m_rightPlaneNormal.Normalize();
                this.m_rightD = -this.m_rightPoint.Dot(this.m_rightPlaneNormal);
            }

            private PointTestResult TestPoint(Vector3 point) => 
                ((point.Dot(this.m_leftPlaneNormal) >= -this.m_leftD) ? ((point.Dot(this.m_rightPlaneNormal) >= -this.m_rightD) ? PointTestResult.INSIDE : PointTestResult.RIGHT) : PointTestResult.LEFT);

            private enum PointTestResult
            {
                LEFT,
                INSIDE,
                RIGHT
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FunnelState
        {
            public Vector3 Apex;
            public Vector3 Left;
            public Vector3 Right;
        }
    }
}

