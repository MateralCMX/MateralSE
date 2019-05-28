namespace VRageRender.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;
    using VRageRender;

    public class MyPolygonBoolOps
    {
        private static Operation m_operationIntersection;
        private static Operation m_operationUnion;
        private static Operation m_operationDifference;
        private static MyPolygonBoolOps m_static;
        private MyPolygon m_polyA = new MyPolygon(new Plane(Vector3.Forward, 0f));
        private MyPolygon m_polyB = new MyPolygon(new Plane(Vector3.Forward, 0f));
        private Operation m_operation;
        private List<BoundPair> m_boundsA = new List<BoundPair>();
        private List<BoundPair> m_boundsB = new List<BoundPair>();
        private List<BoundPair> m_usedBoundPairs = new List<BoundPair>();
        private List<float> m_scanBeamList = new List<float>();
        private List<float> m_horizontalScanBeamList = new List<float>();
        private List<BoundPair> m_localMinimaList = new List<BoundPair>();
        private List<Edge> m_activeEdgeList = new List<Edge>();
        private List<Vector3> m_tmpList = new List<Vector3>();
        private List<SortedEdgeEntry> m_sortedEdgeList = new List<SortedEdgeEntry>();
        private List<IntersectionListEntry> m_intersectionList = new List<IntersectionListEntry>();
        private List<int> m_edgePositionInfo = new List<int>();
        private List<PartialPolygon> m_results = new List<PartialPolygon>();
        private Matrix m_projectionTransform;
        private Matrix m_invProjectionTransform;
        private Plane m_projectionPlane;

        static MyPolygonBoolOps()
        {
            InitializeOperations();
        }

        private void AddBoundPairsToActiveEdges(float bottomY)
        {
            int num = this.m_localMinimaList.Count - 1;
            while (true)
            {
                if (num >= 0)
                {
                    BoundPair pair = this.m_localMinimaList[num];
                    if (pair.MinimumCoordinate == bottomY)
                    {
                        MyPolygon.Vertex vertex;
                        MyPolygon.Vertex vertex2;
                        MyPolygon.Vertex vertex3;
                        int boundPairIndex = this.m_usedBoundPairs.Count;
                        this.m_usedBoundPairs.Add(this.m_localMinimaList[num]);
                        this.m_localMinimaList[num].Parent.GetVertex(this.m_localMinimaList[num].Minimum, out vertex);
                        this.m_localMinimaList[num].Parent.GetVertex(vertex.Prev, out vertex2);
                        this.m_localMinimaList[num].Parent.GetVertex(vertex.Next, out vertex3);
                        PolygonType polyType = ReferenceEquals(this.m_localMinimaList[num].Parent, this.m_polyA) ? PolygonType.SUBJECT : PolygonType.CLIP;
                        Edge leftEdge = this.PrepareActiveEdge(boundPairIndex, ref vertex, ref vertex3, polyType, Side.LEFT);
                        Edge rightEdge = this.PrepareActiveEdge(boundPairIndex, ref vertex, ref vertex2, polyType, Side.RIGHT);
                        int index = this.SortInMinimum(ref leftEdge, ref rightEdge, polyType);
                        if (leftEdge.Contributing)
                        {
                            PartialPolygon polygon = new PartialPolygon();
                            polygon.Append(vertex.Coord);
                            leftEdge.AssociatedPolygon = polygon;
                            rightEdge.AssociatedPolygon = polygon;
                        }
                        if (leftEdge.DXdy > rightEdge.DXdy)
                        {
                            this.m_activeEdgeList.Insert(index, rightEdge);
                            this.m_activeEdgeList.Insert(index + 1, leftEdge);
                        }
                        else
                        {
                            this.m_activeEdgeList.Insert(index, leftEdge);
                            this.m_activeEdgeList.Insert(index + 1, rightEdge);
                        }
                        num--;
                        continue;
                    }
                }
                int count = (this.m_localMinimaList.Count - 1) - num;
                this.m_localMinimaList.RemoveRange(num + 1, count);
                return;
            }
        }

        private void AddBoundPairsToLists(List<BoundPair> boundsList)
        {
            foreach (BoundPair pair in boundsList)
            {
                pair.CalculateMinimumCoordinate();
                int num = this.m_localMinimaList.Count - 1;
                while (true)
                {
                    MyPolygon.Vertex vertex;
                    MyPolygon.Vertex vertex2;
                    if (num >= 0)
                    {
                        BoundPair pair2 = this.m_localMinimaList[num];
                        if (pair2.MinimumCoordinate < pair.MinimumCoordinate)
                        {
                            num--;
                            continue;
                        }
                        this.m_localMinimaList.Insert(num + 1, pair);
                    }
                    if (num == -1)
                    {
                        this.m_localMinimaList.Insert(0, pair);
                    }
                    this.InsertScanBeamDivide(pair.MinimumCoordinate);
                    pair.Parent.GetVertex(pair.Minimum, out vertex2);
                    pair.Parent.GetVertex(vertex2.Next, out vertex);
                    this.InsertScanBeamDivide(vertex.Coord.Y);
                    pair.Parent.GetVertex(vertex2.Prev, out vertex);
                    while (true)
                    {
                        if (vertex.Coord.Y != vertex2.Coord.Y)
                        {
                            this.InsertScanBeamDivide(vertex.Coord.Y);
                            break;
                        }
                        pair.Parent.GetVertex(vertex.Prev, out vertex);
                    }
                    break;
                }
            }
        }

        private void AddLocalMaximum(int leftEdgeIndex, int rightEdgeIndex, ref Edge e1, ref Edge e2, Vector3 maximumPosition)
        {
            if (e1.OutputSide == Side.LEFT)
            {
                e1.AssociatedPolygon.Append(maximumPosition);
            }
            else
            {
                e1.AssociatedPolygon.Prepend(maximumPosition);
            }
            if (ReferenceEquals(e1.AssociatedPolygon, e2.AssociatedPolygon))
            {
                this.m_results.Add(e1.AssociatedPolygon);
            }
            else
            {
                int num;
                if (e1.OutputSide == Side.LEFT)
                {
                    Edge edge = this.FindOtherPolygonEdge(e2.AssociatedPolygon, rightEdgeIndex, out num);
                    e1.AssociatedPolygon.Add(e2.AssociatedPolygon);
                    e2.AssociatedPolygon = e1.AssociatedPolygon;
                    edge.AssociatedPolygon = e2.AssociatedPolygon;
                    this.m_activeEdgeList[num] = edge;
                }
                else
                {
                    Edge edge2 = this.FindOtherPolygonEdge(e1.AssociatedPolygon, leftEdgeIndex, out num);
                    e2.AssociatedPolygon.Add(e1.AssociatedPolygon);
                    e1.AssociatedPolygon = e2.AssociatedPolygon;
                    edge2.AssociatedPolygon = e2.AssociatedPolygon;
                    this.m_activeEdgeList[num] = edge2;
                }
            }
            e1.AssociatedPolygon = null;
            e2.AssociatedPolygon = null;
        }

        private void BuildIntersectionList(float bottom, float top)
        {
            float dy = top - bottom;
            float num2 = 1f / dy;
            SortedEdgeEntry entry = new SortedEdgeEntry();
            this.GetSortedEdgeEntry(bottom, top, dy, 0, ref entry);
            this.m_sortedEdgeList.Add(entry);
            int i = 1;
            while (i < this.m_activeEdgeList.Count)
            {
                this.GetSortedEdgeEntry(bottom, top, dy, i, ref entry);
                int num4 = this.m_sortedEdgeList.Count - 1;
                while (true)
                {
                    if (num4 >= 0)
                    {
                        SortedEdgeEntry entry2 = this.m_sortedEdgeList[num4];
                        if (CompareSortedEdgeEntries(ref entry2, ref entry) != -1)
                        {
                            float num7 = 1f / (entry.DX - entry2.DX);
                            float num6 = (entry2.QNumerator - entry.QNumerator) * num7;
                            IntersectionListEntry intersection = new IntersectionListEntry {
                                RIndex = entry.Index,
                                LIndex = entry2.Index,
                                X = ((entry.DX * num2) * num6) + (entry.QNumerator * num2),
                                Y = num6
                            };
                            this.InsertIntersection(ref intersection);
                            num4--;
                            continue;
                        }
                    }
                    this.m_sortedEdgeList.Insert(num4 + 1, entry);
                    i++;
                    break;
                }
            }
            this.m_sortedEdgeList.Clear();
        }

        [Conditional("DEBUG")]
        private static void CheckClassificationIndices()
        {
        }

        private void Clear()
        {
            this.m_polyA.Clear();
            this.m_polyB.Clear();
            this.m_boundsA.Clear();
            this.m_boundsB.Clear();
            this.m_usedBoundPairs.Clear();
            this.m_scanBeamList.Clear();
            this.m_horizontalScanBeamList.Clear();
            this.m_localMinimaList.Clear();
            this.m_activeEdgeList.Clear();
            this.m_results.Clear();
        }

        private static int CompareCoords(Vector3 coord1, Vector3 coord2) => 
            ((coord1.Y <= coord2.Y) ? ((coord1.Y >= coord2.Y) ? ((coord1.X <= coord2.X) ? ((coord1.X >= coord2.X) ? 0 : -1) : 1) : -1) : 1);

        private static int CompareEdges(ref Edge edge1, ref Edge edge2)
        {
            if (edge1.BottomX < edge2.BottomX)
            {
                return -1;
            }
            if (edge1.BottomX == edge2.BottomX)
            {
                if (edge1.Kind == edge2.Kind)
                {
                    return 1;
                }
                if (edge1.Kind == PolygonType.CLIP)
                {
                    return -1;
                }
            }
            return 1;
        }

        private static int CompareSortedEdgeEntries(ref SortedEdgeEntry entry1, ref SortedEdgeEntry entry2)
        {
            if (entry1.XCoord < entry2.XCoord)
            {
                return -1;
            }
            if (entry1.XCoord == entry2.XCoord)
            {
                if (entry1.Kind == entry2.Kind)
                {
                    return 1;
                }
                if (entry1.Kind == PolygonType.CLIP)
                {
                    return -1;
                }
            }
            return 1;
        }

        private static void ConstructBoundPairs(MyPolygon poly, List<BoundPair> boundList)
        {
            int loop = 0;
            while (loop < poly.LoopCount)
            {
                MyPolygon.Vertex vertex;
                MyPolygon.Vertex vertex2;
                int vertexIndex = FindLoopLocalMaximum(poly, loop);
                poly.GetVertex(vertexIndex, out vertex);
                int next = vertexIndex;
                poly.GetVertex(vertex.Prev, out vertex2);
                BoundPair item = new BoundPair(poly, -1, -1, vertexIndex, vertex2.Coord.Y == vertex.Coord.Y);
                bool flag = true;
                int num5 = -1;
                while (true)
                {
                    Vector3 coord = vertex.Coord;
                    int right = next;
                    next = vertex.Next;
                    poly.GetVertex(next, out vertex);
                    int num6 = num5;
                    num5 = CompareCoords(vertex.Coord, coord);
                    if (flag)
                    {
                        if (num5 > 0)
                        {
                            item.Minimum = right;
                            flag = false;
                        }
                    }
                    else if (num5 < 0)
                    {
                        item.Left = right;
                        boundList.Add(item);
                        item = new BoundPair(poly, -1, -1, right, num6 == 0);
                        flag = true;
                    }
                    if (next == vertexIndex)
                    {
                        item.Left = next;
                        boundList.Add(item);
                        loop++;
                        break;
                    }
                }
            }
        }

        public void DebugDraw(MatrixD drawMatrix)
        {
            drawMatrix = (drawMatrix * this.m_invProjectionTransform) * Matrix.CreateTranslation(this.m_invProjectionTransform.Left * 12f);
            DebugDrawBoundList(drawMatrix, this.m_polyA, this.m_boundsA);
            DebugDrawBoundList(drawMatrix, this.m_polyB, this.m_boundsB);
        }

        private static MatrixD DebugDrawBoundList(MatrixD drawMatrix, MyPolygon drawPoly, List<BoundPair> boundList)
        {
            foreach (BoundPair pair in boundList)
            {
                MyPolygon.Vertex vertex;
                Vector3 position = new Vector3();
                int left = pair.Left;
                drawPoly.GetVertex(left, out vertex);
                int prev = vertex.Prev;
                while (true)
                {
                    MyPolygon.Vertex vertex2;
                    if (left == pair.Minimum)
                    {
                        MatrixD matrix = drawMatrix;
                        matrix.Translation = position;
                        MyRenderProxy.DebugDrawAxis(matrix, 0.25f, false, false, false);
                        MyRenderProxy.DebugDrawSphere(position, 0.03f, Color.Yellow, 1f, false, false, true, false);
                        left = pair.Minimum;
                        drawPoly.GetVertex(left, out vertex);
                        prev = vertex.Prev;
                        while (true)
                        {
                            if (left == pair.Right)
                            {
                                if (pair.RightIsPrecededByHorizontal)
                                {
                                    MyRenderProxy.DebugDrawSphere(position, 0.03f, Color.Red, 1f, false, false, true, false);
                                }
                                break;
                            }
                            drawPoly.GetVertex(prev, out vertex2);
                            position = (Vector3) Vector3D.Transform(vertex2.Coord, drawMatrix);
                            MyRenderProxy.DebugDrawLine3D(Vector3D.Transform(vertex.Coord, drawMatrix), position, Color.Green, Color.Green, false, false);
                            left = prev;
                            vertex = vertex2;
                            prev = vertex.Prev;
                        }
                        break;
                    }
                    drawPoly.GetVertex(prev, out vertex2);
                    position = (Vector3) Vector3D.Transform(vertex2.Coord, drawMatrix);
                    MyRenderProxy.DebugDrawLine3D(Vector3D.Transform(vertex.Coord, drawMatrix), position, Color.Red, Color.Red, false, false);
                    left = prev;
                    vertex = vertex2;
                    prev = vertex.Prev;
                }
            }
            return drawMatrix;
        }

        public MyPolygon Difference(MyPolygon polyA, MyPolygon polyB) => 
            this.PerformBooleanOperation(polyA, polyB, m_operationDifference);

        private static int EncodeClassificationIndex(Side side1, PolygonType type1, Side side2, PolygonType type2) => 
            (((((byte) (side1 + type1)) << 2) + ((int) side2)) + ((int) type2));

        private static int FindLoopLocalMaximum(MyPolygon poly, int loop)
        {
            MyPolygon.Vertex vertex;
            MyPolygon.Vertex vertex2;
            int loopStart = poly.GetLoopStart(loop);
            poly.GetVertex(loopStart, out vertex);
            int num2 = loopStart;
            Vector3 coord = vertex.Coord;
            loopStart = vertex.Prev;
            poly.GetVertex(loopStart, out vertex2);
            while ((vertex2.Coord.Y > coord.Y) || ((vertex2.Coord.Y == coord.Y) && (vertex2.Coord.X > coord.X)))
            {
                num2 = loopStart;
                coord = vertex2.Coord;
                loopStart = vertex2.Prev;
                poly.GetVertex(loopStart, out vertex2);
            }
            loopStart = vertex.Next;
            poly.GetVertex(loopStart, out vertex2);
            while ((vertex2.Coord.Y > coord.Y) || ((vertex2.Coord.Y == coord.Y) && (vertex2.Coord.X > coord.X)))
            {
                num2 = loopStart;
                coord = vertex2.Coord;
                loopStart = vertex2.Next;
                poly.GetVertex(loopStart, out vertex2);
            }
            return num2;
        }

        private Edge FindOtherPolygonEdge(PartialPolygon polygon, int thisEdgeIndex, out int otherEdgeIndex)
        {
            for (int i = 0; i < this.m_activeEdgeList.Count; i++)
            {
                if ((i != thisEdgeIndex) && ReferenceEquals(this.m_activeEdgeList[i].AssociatedPolygon, polygon))
                {
                    otherEdgeIndex = i;
                    return this.m_activeEdgeList[i];
                }
            }
            otherEdgeIndex = -1;
            return new Edge();
        }

        private SortedEdgeEntry GetSortedEdgeEntry(float bottom, float top, float dy, int i, ref SortedEdgeEntry entry)
        {
            Edge edge = this.m_activeEdgeList[i];
            entry.Index = i;
            if (edge.TopY != top)
            {
                entry.XCoord = edge.CalculateX(dy);
            }
            else
            {
                MyPolygon.Vertex vertex;
                if (edge.Kind == PolygonType.SUBJECT)
                {
                    this.m_polyA.GetVertex(edge.TopVertexIndex, out vertex);
                }
                else
                {
                    this.m_polyB.GetVertex(edge.TopVertexIndex, out vertex);
                }
                entry.XCoord = vertex.Coord.X;
            }
            entry.DX = entry.XCoord - edge.BottomX;
            entry.Kind = edge.Kind;
            entry.QNumerator = edge.CalculateQNumerator(bottom, top, entry.XCoord);
            return entry;
        }

        private static void InitClassificationTable(IntersectionClassification[] m_classificationTable)
        {
            for (int i = 0; i < 0x10; i++)
            {
                m_classificationTable[i] = IntersectionClassification.INVALID;
            }
        }

        private void InitializeEdgePositions()
        {
            this.m_edgePositionInfo.Capacity = Math.Max(this.m_edgePositionInfo.Capacity, this.m_intersectionList.Count);
            for (int i = 0; i < this.m_edgePositionInfo.Count; i++)
            {
                this.m_edgePositionInfo[i] = i;
            }
            for (int j = this.m_edgePositionInfo.Count; j < this.m_activeEdgeList.Count; j++)
            {
                this.m_edgePositionInfo.Add(j);
            }
        }

        private static void InitializeOperations()
        {
            IntersectionClassification[] classificationArray1 = new IntersectionClassification[0x10];
            IntersectionClassification[] classificationArray2 = new IntersectionClassification[0x10];
            InitClassificationTable(classificationArray2);
            classificationArray2[7] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray2[13] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray2[2] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray2[8] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray2[4] = IntersectionClassification.LEFT_E2_INTERMEDIATE;
            classificationArray2[1] = IntersectionClassification.LEFT_E2_INTERMEDIATE;
            classificationArray2[14] = IntersectionClassification.RIGHT_E1_INTERMEDIATE;
            classificationArray2[11] = IntersectionClassification.RIGHT_E1_INTERMEDIATE;
            classificationArray2[3] = IntersectionClassification.LOCAL_MAXIMUM;
            classificationArray2[6] = IntersectionClassification.LOCAL_MAXIMUM;
            classificationArray2[9] = IntersectionClassification.LOCAL_MINIMUM;
            classificationArray2[12] = IntersectionClassification.LOCAL_MINIMUM;
            m_operationIntersection = new Operation(classificationArray2, true, true, false, false);
            IntersectionClassification[] classificationArray3 = new IntersectionClassification[0x10];
            InitClassificationTable(classificationArray3);
            classificationArray3[7] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray3[13] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray3[2] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray3[8] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray3[4] = IntersectionClassification.LEFT_E1_INTERMEDIATE;
            classificationArray3[1] = IntersectionClassification.LEFT_E1_INTERMEDIATE;
            classificationArray3[14] = IntersectionClassification.RIGHT_E2_INTERMEDIATE;
            classificationArray3[11] = IntersectionClassification.RIGHT_E2_INTERMEDIATE;
            classificationArray3[3] = IntersectionClassification.LOCAL_MINIMUM;
            classificationArray3[6] = IntersectionClassification.LOCAL_MINIMUM;
            classificationArray3[9] = IntersectionClassification.LOCAL_MAXIMUM;
            classificationArray3[12] = IntersectionClassification.LOCAL_MAXIMUM;
            m_operationUnion = new Operation(classificationArray3, false, false, false, false);
            IntersectionClassification[] classificationArray4 = new IntersectionClassification[0x10];
            InitClassificationTable(classificationArray4);
            classificationArray4[7] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray4[13] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray4[2] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray4[8] = IntersectionClassification.LIKE_INTERSECTION;
            classificationArray4[4] = IntersectionClassification.LEFT_E2_INTERMEDIATE;
            classificationArray4[1] = IntersectionClassification.LEFT_E2_INTERMEDIATE;
            classificationArray4[14] = IntersectionClassification.RIGHT_E1_INTERMEDIATE;
            classificationArray4[11] = IntersectionClassification.RIGHT_E1_INTERMEDIATE;
            classificationArray4[3] = IntersectionClassification.LOCAL_MAXIMUM;
            classificationArray4[6] = IntersectionClassification.LOCAL_MAXIMUM;
            classificationArray4[9] = IntersectionClassification.LOCAL_MINIMUM;
            classificationArray4[12] = IntersectionClassification.LOCAL_MINIMUM;
            m_operationDifference = new Operation(classificationArray4, false, true, false, true);
        }

        private void InsertIntersection(ref IntersectionListEntry intersection)
        {
            for (int i = 0; i < this.m_intersectionList.Count; i++)
            {
                if (this.m_intersectionList[i].Y > intersection.Y)
                {
                    this.m_intersectionList.Insert(i, intersection);
                    return;
                }
            }
            this.m_intersectionList.Add(intersection);
        }

        private void InsertScanBeamDivide(float value)
        {
            for (int i = 0; i < this.m_scanBeamList.Count; i++)
            {
                if (this.m_scanBeamList[i] <= value)
                {
                    if (this.m_scanBeamList[i] != value)
                    {
                        this.m_scanBeamList.Insert(i, value);
                    }
                    return;
                }
            }
            this.m_scanBeamList.Add(value);
        }

        public MyPolygon Intersection(MyPolygon polyA, MyPolygon polyB) => 
            this.PerformBooleanOperation(polyA, polyB, m_operationIntersection);

        private static Side OtherSide(Side side) => 
            ((side != Side.LEFT) ? Side.LEFT : Side.RIGHT);

        private MyPolygon PerformBooleanOperation(MyPolygon polyA, MyPolygon polyB, Operation operation)
        {
            this.Clear();
            this.PrepareTransforms(polyA);
            this.ProjectPoly(polyA, this.m_polyA, ref this.m_projectionTransform);
            this.ProjectPoly(polyB, this.m_polyB, ref this.m_projectionTransform);
            this.m_operation = operation;
            this.PerformInPlane();
            this.m_operation = null;
            return this.UnprojectResult();
        }

        private void PerformInPlane()
        {
            ConstructBoundPairs(this.m_polyA, this.m_boundsA);
            ConstructBoundPairs(this.m_polyB, this.m_boundsB);
            this.AddBoundPairsToLists(this.m_boundsA);
            this.AddBoundPairsToLists(this.m_boundsB);
            float bottomY = this.m_scanBeamList[this.m_scanBeamList.Count - 1];
            this.m_scanBeamList.RemoveAt(this.m_scanBeamList.Count - 1);
            do
            {
                this.AddBoundPairsToActiveEdges(bottomY);
                float top = this.m_scanBeamList[this.m_scanBeamList.Count - 1];
                this.m_scanBeamList.RemoveAt(this.m_scanBeamList.Count - 1);
                if (bottomY == top)
                {
                    this.ProcessHorizontalLine(bottomY);
                    if (this.m_scanBeamList.Count == 0)
                    {
                        break;
                    }
                    top = this.m_scanBeamList[this.m_scanBeamList.Count - 1];
                    this.m_scanBeamList.RemoveAt(this.m_scanBeamList.Count - 1);
                }
                this.ProcessIntersections(bottomY, top);
                this.UpdateActiveEdges(bottomY, top);
                bottomY = top;
            }
            while (this.m_scanBeamList.Count > 0);
            this.m_scanBeamList.Clear();
        }

        private void PerformIntersection(int leftEdgeIndex, int rightEdgeIndex, ref Edge e1, ref Edge e2, ref Vector3 intersectionPosition, IntersectionClassification intersectionClassification)
        {
            switch (intersectionClassification)
            {
                case IntersectionClassification.LIKE_INTERSECTION:
                {
                    Side outputSide = e1.OutputSide;
                    e1.OutputSide = e2.OutputSide;
                    e2.OutputSide = outputSide;
                    if (e1.Contributing)
                    {
                        if (e1.OutputSide == Side.RIGHT)
                        {
                            e1.AssociatedPolygon.Append(intersectionPosition);
                            e2.AssociatedPolygon.Prepend(intersectionPosition);
                        }
                        else
                        {
                            e1.AssociatedPolygon.Prepend(intersectionPosition);
                            e2.AssociatedPolygon.Append(intersectionPosition);
                        }
                    }
                    break;
                }
                case IntersectionClassification.LOCAL_MINIMUM:
                {
                    PartialPolygon polygon2 = new PartialPolygon();
                    polygon2.Append(intersectionPosition);
                    e1.AssociatedPolygon = polygon2;
                    e2.AssociatedPolygon = polygon2;
                    break;
                }
                case IntersectionClassification.LOCAL_MAXIMUM:
                    this.AddLocalMaximum(leftEdgeIndex, rightEdgeIndex, ref e1, ref e2, intersectionPosition);
                    break;

                case IntersectionClassification.LEFT_E1_INTERMEDIATE:
                    e1.AssociatedPolygon.Append(intersectionPosition);
                    break;

                case IntersectionClassification.RIGHT_E1_INTERMEDIATE:
                    e1.AssociatedPolygon.Prepend(intersectionPosition);
                    break;

                case IntersectionClassification.LEFT_E2_INTERMEDIATE:
                    e2.AssociatedPolygon.Append(intersectionPosition);
                    break;

                case IntersectionClassification.RIGHT_E2_INTERMEDIATE:
                    e2.AssociatedPolygon.Prepend(intersectionPosition);
                    break;

                default:
                    break;
            }
            PartialPolygon associatedPolygon = e1.AssociatedPolygon;
            e1.AssociatedPolygon = e2.AssociatedPolygon;
            e2.AssociatedPolygon = associatedPolygon;
            if (intersectionClassification != IntersectionClassification.LIKE_INTERSECTION)
            {
                e1.Contributing = !e1.Contributing;
                e2.Contributing = !e2.Contributing;
            }
            this.m_activeEdgeList[leftEdgeIndex] = e2;
            this.m_activeEdgeList[rightEdgeIndex] = e1;
        }

        private unsafe Edge PrepareActiveEdge(int boundPairIndex, ref MyPolygon.Vertex lowerVertex, ref MyPolygon.Vertex upperVertex, PolygonType polyType, Side side)
        {
            Edge* edgePtr1;
            Edge* edgePtr2;
            Edge edge = new Edge {
                BoundPairIndex = boundPairIndex,
                BoundPairSide = side,
                Kind = polyType
            };
            if (polyType == PolygonType.CLIP)
            {
                edgePtr1->OutputSide = this.m_operation.ClipInvert ? OtherSide(side) : side;
            }
            else
            {
                edgePtr1 = (Edge*) ref edge;
                edgePtr2->OutputSide = this.m_operation.SubjectInvert ? OtherSide(side) : side;
            }
            edgePtr2 = (Edge*) ref edge;
            this.RecalculateActiveEdge(ref edge, ref lowerVertex, ref upperVertex, side);
            return edge;
        }

        private void PrepareTransforms(MyPolygon polyA)
        {
            this.m_projectionPlane = polyA.PolygonPlane;
            Vector3 position = -polyA.PolygonPlane.Normal * polyA.PolygonPlane.D;
            Vector3 normal = polyA.PolygonPlane.Normal;
            this.m_invProjectionTransform = Matrix.CreateWorld(position, normal, Vector3.Cross(Vector3.CalculatePerpendicularVector(normal), normal));
            Matrix.Invert(ref this.m_invProjectionTransform, out this.m_projectionTransform);
        }

        private unsafe void ProcessHorizontalLine(float bottomY)
        {
            float positiveInfinity = float.PositiveInfinity;
            PolygonType sUBJECT = PolygonType.SUBJECT;
            int from = 0;
            int to = 0;
            while (to < this.m_activeEdgeList.Count)
            {
                int rightEdgeIndex = to;
                Edge edge = this.m_activeEdgeList[rightEdgeIndex];
                while (true)
                {
                    if ((positiveInfinity >= edge.BottomX) && (((positiveInfinity != edge.BottomX) || (sUBJECT != PolygonType.CLIP)) || (edge.Kind != PolygonType.SUBJECT)))
                    {
                        if (edge.TopY != bottomY)
                        {
                            int leftEdgeIndex = to - 1;
                            while (true)
                            {
                                if (leftEdgeIndex < from)
                                {
                                    from++;
                                    break;
                                }
                                Edge edge3 = this.m_activeEdgeList[leftEdgeIndex];
                                Vector3 intersectionPosition = new Vector3(edge.BottomX, bottomY, 0f);
                                IntersectionClassification intersectionClassification = this.m_operation.ClassifyIntersection(edge3.OutputSide, edge3.Kind, edge.OutputSide, edge.Kind);
                                this.PerformIntersection(leftEdgeIndex, rightEdgeIndex, ref edge3, ref edge, ref intersectionPosition, intersectionClassification);
                                rightEdgeIndex = leftEdgeIndex;
                                leftEdgeIndex--;
                            }
                        }
                        else if (edge.DXdy != 0f)
                        {
                            float num6 = edge.BottomX + edge.DXdy;
                            if ((num6 < positiveInfinity) || (((num6 == positiveInfinity) && (edge.Kind == PolygonType.CLIP)) && (sUBJECT == PolygonType.SUBJECT)))
                            {
                                positiveInfinity = num6;
                                sUBJECT = edge.Kind;
                            }
                        }
                        else
                        {
                            for (int i = to - 1; i >= from; i--)
                            {
                                Edge edge2 = this.m_activeEdgeList[i];
                                if ((edge2.Kind == edge.Kind) && (edge2.TopVertexIndex == edge.TopVertexIndex))
                                {
                                    if (edge2.Contributing)
                                    {
                                        Edge* edgePtr1 = (Edge*) ref edge;
                                        this.AddLocalMaximum(i, rightEdgeIndex, ref edge2, ref (Edge) ref edgePtr1, new Vector3(edge.BottomX, bottomY, 0f));
                                    }
                                    this.m_activeEdgeList.RemoveAt(rightEdgeIndex);
                                    this.m_activeEdgeList.RemoveAt(i);
                                    to -= 2;
                                    break;
                                }
                                Vector3 intersectionPosition = new Vector3(edge.BottomX, bottomY, 0f);
                                this.PerformIntersection(i, rightEdgeIndex, ref edge2, ref edge, ref intersectionPosition, this.m_operation.ClassifyIntersection(edge2.OutputSide, edge2.Kind, edge.OutputSide, edge.Kind));
                                rightEdgeIndex = i;
                            }
                        }
                        to++;
                        break;
                    }
                    this.ProcessOldHorizontalEdges(bottomY, ref positiveInfinity, ref sUBJECT, ref from, to);
                }
            }
            while (from < this.m_activeEdgeList.Count)
            {
                this.ProcessOldHorizontalEdges(bottomY, ref positiveInfinity, ref sUBJECT, ref from, this.m_activeEdgeList.Count);
            }
        }

        private void ProcessIntersectionList()
        {
            this.InitializeEdgePositions();
            for (int i = 0; i < this.m_intersectionList.Count; i++)
            {
                IntersectionListEntry entry = this.m_intersectionList[i];
                int leftEdgeIndex = this.m_edgePositionInfo[entry.LIndex];
                int rightEdgeIndex = this.m_edgePositionInfo[entry.RIndex];
                Edge edge = this.m_activeEdgeList[leftEdgeIndex];
                Edge edge2 = this.m_activeEdgeList[rightEdgeIndex];
                Vector3 intersectionPosition = new Vector3(entry.X, entry.Y, 0f);
                IntersectionClassification intersectionClassification = this.m_operation.ClassifyIntersection(edge.OutputSide, edge.Kind, edge2.OutputSide, edge2.Kind);
                this.PerformIntersection(leftEdgeIndex, rightEdgeIndex, ref edge, ref edge2, ref intersectionPosition, intersectionClassification);
                this.SwapEdgePositions(entry.LIndex, entry.RIndex);
            }
            this.m_intersectionList.Clear();
            this.m_edgePositionInfo.Clear();
        }

        private void ProcessIntersections(float bottom, float top)
        {
            if (this.m_activeEdgeList.Count != 0)
            {
                this.BuildIntersectionList(bottom, top);
                this.ProcessIntersectionList();
            }
        }

        private unsafe void ProcessOldHorizontalEdges(float bottomY, ref float endX, ref PolygonType endType, ref int from, int to)
        {
            float num = endX;
            PolygonType type = endType;
            endX = float.PositiveInfinity;
            endType = PolygonType.SUBJECT;
            for (int i = from; i < to; i++)
            {
                Edge edge = this.m_activeEdgeList[i];
                if (((edge.BottomX + edge.DXdy) != num) || (edge.Kind != type))
                {
                    float num6 = edge.BottomX + edge.DXdy;
                    if ((num6 < endX) || (((num6 == endX) && (edge.Kind == PolygonType.CLIP)) && (endType == PolygonType.SUBJECT)))
                    {
                        endX = num6;
                        endType = edge.Kind;
                    }
                }
                else
                {
                    MyPolygon.Vertex vertex;
                    MyPolygon.Vertex vertex2;
                    BoundPair pair = this.m_usedBoundPairs[edge.BoundPairIndex];
                    pair.Parent.GetVertex(edge.TopVertexIndex, out vertex);
                    Vector3 coord = vertex.Coord;
                    if (edge.Contributing)
                    {
                        if (edge.OutputSide == Side.LEFT)
                        {
                            edge.AssociatedPolygon.Append(coord);
                        }
                        else
                        {
                            edge.AssociatedPolygon.Prepend(coord);
                        }
                    }
                    if (edge.BoundPairSide == Side.LEFT)
                    {
                        pair.Parent.GetVertex(vertex.Next, out vertex2);
                    }
                    else
                    {
                        pair.Parent.GetVertex(vertex.Prev, out vertex2);
                    }
                    Edge* edgePtr1 = (Edge*) ref edge;
                    this.RecalculateActiveEdge(ref (Edge) ref edgePtr1, ref vertex, ref vertex2, edge.BoundPairSide);
                    this.m_activeEdgeList[i] = edge;
                    if ((edge.TopY == bottomY) && (edge.DXdy != 0f))
                    {
                        float num3 = edge.BottomX + edge.DXdy;
                        if ((num3 < endX) || (((num3 == endX) && (edge.Kind == PolygonType.CLIP)) && (endType == PolygonType.SUBJECT)))
                        {
                            endX = num3;
                            endType = edge.Kind;
                        }
                    }
                    else
                    {
                        int rightEdgeIndex = i;
                        int leftEdgeIndex = i - 1;
                        while (true)
                        {
                            if (leftEdgeIndex < from)
                            {
                                from++;
                                break;
                            }
                            Edge edge2 = this.m_activeEdgeList[leftEdgeIndex];
                            Vector3 intersectionPosition = new Vector3(edge.BottomX, bottomY, 0f);
                            IntersectionClassification intersectionClassification = this.m_operation.ClassifyIntersection(edge2.OutputSide, edge2.Kind, edge.OutputSide, edge.Kind);
                            this.PerformIntersection(leftEdgeIndex, rightEdgeIndex, ref edge2, ref edge, ref intersectionPosition, intersectionClassification);
                            rightEdgeIndex = leftEdgeIndex;
                            leftEdgeIndex--;
                        }
                    }
                }
            }
        }

        private void ProjectPoly(MyPolygon input, MyPolygon output, ref Matrix projection)
        {
            int loopIndex = 0;
            while (loopIndex < input.LoopCount)
            {
                this.m_tmpList.Clear();
                MyPolygon.LoopIterator loopIterator = input.GetLoopIterator(loopIndex);
                while (true)
                {
                    if (!loopIterator.MoveNext())
                    {
                        output.AddLoop(this.m_tmpList);
                        loopIndex++;
                        break;
                    }
                    Vector3 item = Vector3.Transform(loopIterator.Current, (Matrix) projection);
                    this.m_tmpList.Add(item);
                }
            }
        }

        private void RecalculateActiveEdge(ref Edge edge, ref MyPolygon.Vertex lowerVertex, ref MyPolygon.Vertex upperVertex, Side boundPairSide)
        {
            float num = upperVertex.Coord.Y - lowerVertex.Coord.Y;
            float num2 = upperVertex.Coord.X - lowerVertex.Coord.X;
            edge.TopVertexIndex = (boundPairSide == Side.LEFT) ? lowerVertex.Next : lowerVertex.Prev;
            edge.BottomX = lowerVertex.Coord.X;
            edge.TopY = upperVertex.Coord.Y;
            edge.DXdy = (num == 0f) ? num2 : (num2 / num);
            this.InsertScanBeamDivide(upperVertex.Coord.Y);
        }

        private int SortInMinimum(ref Edge leftEdge, ref Edge rightEdge, PolygonType type)
        {
            bool parity = false;
            int num = 0;
            while (true)
            {
                if (num < this.m_activeEdgeList.Count)
                {
                    Edge edge = this.m_activeEdgeList[num];
                    if (CompareEdges(ref leftEdge, ref edge) != -1)
                    {
                        if (edge.Kind != type)
                        {
                            parity = !parity;
                        }
                        num++;
                        continue;
                    }
                }
                bool flag2 = this.m_operation.InitializeContributing(parity, type);
                leftEdge.Contributing = flag2;
                rightEdge.Contributing = flag2;
                return num;
            }
        }

        private void SwapEdgePositions(int leftEdge, int rightEdge)
        {
            int num = this.m_edgePositionInfo[leftEdge];
            int num2 = this.m_edgePositionInfo[rightEdge];
            this.m_edgePositionInfo[leftEdge] = num2;
            this.m_edgePositionInfo[rightEdge] = num;
        }

        public MyPolygon Union(MyPolygon polyA, MyPolygon polyB) => 
            this.PerformBooleanOperation(polyA, polyB, m_operationUnion);

        private MyPolygon UnprojectResult()
        {
            MyPolygon input = new MyPolygon(new Plane(Vector3.Forward, 0f));
            MyPolygon output = new MyPolygon(this.m_projectionPlane);
            foreach (PartialPolygon polygon3 in this.m_results)
            {
                polygon3.Postprocess();
                if (polygon3.GetLoop().Count != 0)
                {
                    input.AddLoop(polygon3.GetLoop());
                }
            }
            this.ProjectPoly(input, output, ref this.m_invProjectionTransform);
            return output;
        }

        private unsafe void UpdateActiveEdges(float bottomY, float topY)
        {
            Vector3 coord;
            Edge edge;
            if (this.m_activeEdgeList.Count == 0)
            {
                return;
            }
            float dy = topY - bottomY;
            int leftEdgeIndex = 0;
            goto TR_0023;
        TR_0002:
            leftEdgeIndex++;
        TR_0023:
            while (true)
            {
                if (leftEdgeIndex >= this.m_activeEdgeList.Count)
                {
                    return;
                }
                edge = this.m_activeEdgeList[leftEdgeIndex];
                if (edge.TopY != topY)
                {
                    Edge* edgePtr3 = (Edge*) ref edge;
                    edgePtr3->BottomX = edge.CalculateX(dy);
                    this.m_activeEdgeList[leftEdgeIndex] = edge;
                    goto TR_0002;
                }
                else
                {
                    MyPolygon.Vertex vertex;
                    BoundPair pair = this.m_usedBoundPairs[edge.BoundPairIndex];
                    bool flag = false;
                    pair.Parent.GetVertex(edge.TopVertexIndex, out vertex);
                    coord = vertex.Coord;
                    if (edge.TopVertexIndex == ((edge.BoundPairSide == Side.LEFT) ? pair.Left : pair.Right))
                    {
                        flag = true;
                    }
                    else
                    {
                        MyPolygon.Vertex vertex2;
                        if (edge.Contributing)
                        {
                            if (edge.OutputSide == Side.LEFT)
                            {
                                edge.AssociatedPolygon.Append(coord);
                            }
                            else
                            {
                                edge.AssociatedPolygon.Prepend(coord);
                            }
                        }
                        if (edge.BoundPairSide == Side.LEFT)
                        {
                            pair.Parent.GetVertex(vertex.Next, out vertex2);
                        }
                        else
                        {
                            pair.Parent.GetVertex(vertex.Prev, out vertex2);
                        }
                        Edge* edgePtr1 = (Edge*) ref edge;
                        this.RecalculateActiveEdge(ref (Edge) ref edgePtr1, ref vertex, ref vertex2, edge.BoundPairSide);
                    }
                    if (!flag)
                    {
                        this.m_activeEdgeList[leftEdgeIndex] = edge;
                    }
                    else
                    {
                        if (edge.BoundPairSide != Side.RIGHT)
                        {
                            break;
                        }
                        if (!this.m_usedBoundPairs[edge.BoundPairIndex].RightIsPrecededByHorizontal)
                        {
                            break;
                        }
                        Edge* edgePtr2 = (Edge*) ref edge;
                        edgePtr2->BottomX = edge.CalculateX(dy);
                        edge.TopY = topY;
                        edge.DXdy = 0f;
                        this.m_activeEdgeList[leftEdgeIndex] = edge;
                    }
                    goto TR_0002;
                }
                break;
            }
            int rightEdgeIndex = -1;
            Edge edge2 = new Edge();
            int num4 = leftEdgeIndex + 1;
            while (true)
            {
                if (num4 < this.m_activeEdgeList.Count)
                {
                    edge2 = this.m_activeEdgeList[num4];
                    if ((edge2.Kind != edge.Kind) || (edge2.TopVertexIndex != edge.TopVertexIndex))
                    {
                        num4++;
                        continue;
                    }
                    rightEdgeIndex = num4;
                }
                if (edge.Contributing && edge2.Contributing)
                {
                    this.AddLocalMaximum(leftEdgeIndex, rightEdgeIndex, ref edge, ref edge2, coord);
                }
                this.m_activeEdgeList.RemoveAt(rightEdgeIndex);
                this.m_activeEdgeList.RemoveAt(leftEdgeIndex);
                break;
            }
            goto TR_0023;
        }

        public static MyPolygonBoolOps Static
        {
            get
            {
                if (m_static == null)
                {
                    m_static = new MyPolygonBoolOps();
                }
                return m_static;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BoundPair
        {
            public MyPolygon Parent;
            public int Left;
            public int Minimum;
            public int Right;
            public bool RightIsPrecededByHorizontal;
            private float m_minimumCoordinate;
            public float MinimumCoordinate =>
                this.m_minimumCoordinate;
            public BoundPair(MyPolygon parent, int left, int minimum, int right, bool rightHorizontal)
            {
                this.Parent = parent;
                this.Left = left;
                this.Minimum = minimum;
                this.Right = right;
                this.RightIsPrecededByHorizontal = rightHorizontal;
                this.m_minimumCoordinate = 0f;
            }

            public bool IsValid() => 
                ((this.Parent != null) && ((this.Left != -1) && ((this.Right != -1) && ((this.Minimum != -1) && ((this.Left != this.Minimum) && ((this.Right != this.Minimum) && !float.IsNaN(this.MinimumCoordinate)))))));

            public void CalculateMinimumCoordinate()
            {
                MyPolygon.Vertex vertex;
                this.Parent.GetVertex(this.Minimum, out vertex);
                this.m_minimumCoordinate = vertex.Coord.Y;
            }
        }

        private enum ClassificationIndex : byte
        {
            LEFT_SUBJECT_AND_LEFT_SUBJECT = 0,
            LEFT_SUBJECT_AND_LEFT_CLIP = 1,
            LEFT_SUBJECT_AND_RIGHT_SUBJECT = 2,
            LEFT_SUBJECT_AND_RIGHT_CLIP = 3,
            LEFT_CLIP_AND_LEFT_SUBJECT = 4,
            LEFT_CLIP_AND_LEFT_CLIP = 5,
            LEFT_CLIP_AND_RIGHT_SUBJECT = 6,
            LEFT_CLIP_AND_RIGHT_CLIP = 7,
            RIGHT_SUBJECT_AND_LEFT_SUBJECT = 8,
            RIGHT_SUBJECT_AND_LEFT_CLIP = 9,
            RIGHT_SUBJECT_AND_RIGHT_SUBJECT = 10,
            RIGHT_SUBJECT_AND_RIGHT_CLIP = 11,
            RIGHT_CLIP_AND_LEFT_SUBJECT = 12,
            RIGHT_CLIP_AND_LEFT_CLIP = 13,
            RIGHT_CLIP_AND_RIGHT_SUBJECT = 14,
            RIGHT_CLIP_AND_RIGHT_CLIP = 15
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Edge
        {
            public int BoundPairIndex;
            public MyPolygonBoolOps.Side BoundPairSide;
            public int TopVertexIndex;
            public float BottomX;
            public float TopY;
            public float DXdy;
            public MyPolygonBoolOps.PolygonType Kind;
            public MyPolygonBoolOps.Side OutputSide;
            public bool Contributing;
            public MyPolygonBoolOps.PartialPolygon AssociatedPolygon;
            public float CalculateX(float dy) => 
                (this.BottomX + (this.DXdy * dy));

            public float CalculateQNumerator(float bottom, float top, float topX) => 
                ((this.BottomX * top) - (topX * bottom));
        }

        private enum IntersectionClassification : byte
        {
            INVALID = 0,
            LIKE_INTERSECTION = 1,
            LOCAL_MINIMUM = 2,
            LOCAL_MAXIMUM = 3,
            LEFT_E1_INTERMEDIATE = 4,
            RIGHT_E1_INTERMEDIATE = 5,
            LEFT_E2_INTERMEDIATE = 6,
            RIGHT_E2_INTERMEDIATE = 7
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IntersectionListEntry
        {
            public int LIndex;
            public int RIndex;
            public float X;
            public float Y;
        }

        private class Operation
        {
            private MyPolygonBoolOps.IntersectionClassification[] m_classificationTable;
            private bool m_sParityForContribution;
            private bool m_cParityForContribution;

            public Operation(MyPolygonBoolOps.IntersectionClassification[] classificationTable, bool subjectContributingParity, bool clipContributingParity, bool invertSubjectSides, bool invertClipSides)
            {
                this.m_classificationTable = classificationTable;
                this.m_sParityForContribution = subjectContributingParity;
                this.m_cParityForContribution = clipContributingParity;
                this.SubjectInvert = invertSubjectSides;
                this.ClipInvert = invertClipSides;
            }

            public MyPolygonBoolOps.IntersectionClassification ClassifyIntersection(MyPolygonBoolOps.Side side1, MyPolygonBoolOps.PolygonType type1, MyPolygonBoolOps.Side side2, MyPolygonBoolOps.PolygonType type2) => 
                this.m_classificationTable[MyPolygonBoolOps.EncodeClassificationIndex(side1, type1, side2, type2)];

            public bool InitializeContributing(bool parity, MyPolygonBoolOps.PolygonType type) => 
                ((type != MyPolygonBoolOps.PolygonType.SUBJECT) ? (!parity ? !this.m_cParityForContribution : this.m_cParityForContribution) : (!parity ? !this.m_sParityForContribution : this.m_sParityForContribution));

            public bool SubjectInvert { get; private set; }

            public bool ClipInvert { get; private set; }
        }

        private class PartialPolygon
        {
            private List<Vector3> m_vertices = new List<Vector3>();

            public void Add(MyPolygonBoolOps.PartialPolygon other)
            {
                if (other.m_vertices.Count != 0)
                {
                    this.Append(other.m_vertices[0]);
                    for (int i = 1; i < other.m_vertices.Count; i++)
                    {
                        this.m_vertices.Add(other.m_vertices[i]);
                    }
                }
            }

            public void Append(Vector3 newVertex)
            {
                int count = this.m_vertices.Count;
                if ((count == 0) || (Vector3.DistanceSquared(newVertex, this.m_vertices[count - 1]) > 1E-06f))
                {
                    this.m_vertices.Add(newVertex);
                }
            }

            public void Clear()
            {
                this.m_vertices.Clear();
            }

            public List<Vector3> GetLoop() => 
                this.m_vertices;

            public void Postprocess()
            {
                if ((this.m_vertices.Count >= 3) && (Vector3.DistanceSquared(this.m_vertices[this.m_vertices.Count - 1], this.m_vertices[0]) <= 1E-06f))
                {
                    this.m_vertices.RemoveAt(this.m_vertices.Count - 1);
                }
                if (this.m_vertices.Count < 3)
                {
                    this.m_vertices.Clear();
                }
            }

            public void Prepend(Vector3 newVertex)
            {
                if ((this.m_vertices.Count == 0) || (Vector3.DistanceSquared(newVertex, this.m_vertices[0]) > 1E-06f))
                {
                    this.m_vertices.Insert(0, newVertex);
                }
            }

            public void Reverse()
            {
                this.m_vertices.Reverse();
            }
        }

        private enum PolygonType : byte
        {
            SUBJECT = 0,
            CLIP = 1
        }

        private enum Side : byte
        {
            LEFT = 0,
            RIGHT = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SortedEdgeEntry
        {
            public int Index;
            public float XCoord;
            public float DX;
            public MyPolygonBoolOps.PolygonType Kind;
            public float QNumerator;
        }
    }
}

