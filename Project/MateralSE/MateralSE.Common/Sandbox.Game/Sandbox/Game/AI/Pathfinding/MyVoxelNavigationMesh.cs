namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage.Algorithms;
    using VRage.Collections;
    using VRage.Generics;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;
    using VRageRender.Utils;

    public class MyVoxelNavigationMesh : MyNavigationMesh
    {
        private static bool DO_CONSISTENCY_CHECKS = false;
        private MyVoxelBase m_voxelMap;
        private static MyVoxelBase m_staticVoxelMap;
        private Vector3 m_cellSize;
        private MyVector3ISet m_processedCells;
        private HashSet<ulong> m_cellsOnWayCoords;
        private List<Vector3I> m_cellsOnWay;
        private List<MyHighLevelPrimitive> m_primitivesOnPath;
        private MyBinaryHeap<float, CellToAddHeapItem> m_toAdd;
        private List<CellToAddHeapItem> m_heapItemList;
        private MyVector3ISet m_markedForAddition;
        private static MyDynamicObjectPool<CellToAddHeapItem> m_heapItemAllocator = new MyDynamicObjectPool<CellToAddHeapItem>(0x80);
        private static MyVector3ISet m_tmpCellSet = new MyVector3ISet();
        private static List<MyCubeGrid> m_tmpGridList = new List<MyCubeGrid>();
        private static List<MyGridPathfinding.CubeId> m_tmpLinkCandidates = new List<MyGridPathfinding.CubeId>();
        private static Dictionary<MyGridPathfinding.CubeId, List<MyNavigationPrimitive>> m_tmpCubeLinkCandidates = new Dictionary<MyGridPathfinding.CubeId, List<MyNavigationPrimitive>>();
        private static MyDynamicObjectPool<List<MyNavigationPrimitive>> m_primitiveListPool = new MyDynamicObjectPool<List<MyNavigationPrimitive>>(8);
        private LinkedList<Vector3I> m_cellsToChange;
        private MyVector3ISet m_cellsToChangeSet;
        private static MyUnionFind m_vertexMapping = new MyUnionFind();
        private static List<int> m_tmpIntList = new List<int>();
        private MyVoxelConnectionHelper m_connectionHelper;
        private MyNavmeshCoordinator m_navmeshCoordinator;
        private MyHighLevelGroup m_higherLevel;
        private MyVoxelHighLevelHelper m_higherLevelHelper;
        private static HashSet<Vector3I> m_adjacentCells = new HashSet<Vector3I>();
        private static Dictionary<Vector3I, BoundingBoxD> m_adjacentBBoxes = new Dictionary<Vector3I, BoundingBoxD>();
        private static Vector3D m_halfMeterOffset = new Vector3D(0.5);
        private static BoundingBoxD m_cellBB = new BoundingBoxD();
        private static Vector3D m_bbMinOffset = new Vector3D(-0.125);
        private Vector3I m_maxCellCoord;
        private const float ExploredRemovingDistance = 200f;
        private const float ProcessedRemovingDistance = 50f;
        private const float AddRemoveKoef = 0.5f;
        private const float MaxAddToProcessingDistance = 25f;
        private float LimitAddingWeight;
        private const float CellsOnWayAdvance = 8f;
        public static float PresentEntityWeight = 100f;
        public static float RecountCellWeight = 10f;
        public static float JustAddedAdjacentCellWeight = 0.02f;
        public static float TooFarWeight = -100f;
        private Vector3 m_debugPos1;
        private Vector3 m_debugPos2;
        private Vector3 m_debugPos3;
        private Vector3 m_debugPos4;
        private Dictionary<ulong, List<DebugDrawEdge>> m_debugCellEdges;
        public const int NAVMESH_LOD = 0;
        private static readonly Vector3I[] m_cornerOffsets = new Vector3I[] { new Vector3I(-1, -1, -1), new Vector3I(0, -1, -1), new Vector3I(-1, 0, -1), new Vector3I(0, 0, -1), new Vector3I(-1, -1, 0), new Vector3I(0, -1, 0), new Vector3I(-1, 0, 0), new Vector3I(0, 0, 0) };

        public MyVoxelNavigationMesh(MyVoxelBase voxelMap, MyNavmeshCoordinator coordinator, Func<long> timestampFunction) : base(coordinator.Links, 0x10, timestampFunction)
        {
            this.LimitAddingWeight = GetWeight(25f);
            this.m_voxelMap = voxelMap;
            m_staticVoxelMap = this.m_voxelMap;
            this.m_processedCells = new MyVector3ISet();
            this.m_cellsOnWayCoords = new HashSet<ulong>();
            this.m_cellsOnWay = new List<Vector3I>();
            this.m_primitivesOnPath = new List<MyHighLevelPrimitive>(0x80);
            this.m_toAdd = new MyBinaryHeap<float, CellToAddHeapItem>(0x80);
            this.m_heapItemList = new List<CellToAddHeapItem>();
            this.m_markedForAddition = new MyVector3ISet();
            this.m_cellsToChange = new LinkedList<Vector3I>();
            this.m_cellsToChangeSet = new MyVector3ISet();
            this.m_connectionHelper = new MyVoxelConnectionHelper();
            this.m_navmeshCoordinator = coordinator;
            this.m_higherLevel = new MyHighLevelGroup(this, coordinator.HighLevelLinks, timestampFunction);
            this.m_higherLevelHelper = new MyVoxelHighLevelHelper(this);
            this.m_debugCellEdges = new Dictionary<ulong, List<DebugDrawEdge>>();
            voxelMap.Storage.RangeChanged += new Action<Vector3I, Vector3I, MyStorageDataTypeFlags>(this.OnStorageChanged);
            this.m_maxCellCoord = ((Vector3I) (this.m_voxelMap.Size / 8)) - Vector3I.One;
        }

        private bool AddCell(Vector3I cellPos, ref HashSet<Vector3I> adjacentCellPos)
        {
            if (MyFakes.LOG_NAVMESH_GENERATION)
            {
                MyCestmirPathfindingShorts.Pathfinding.VoxelPathfinding.DebugLog.LogCellAddition(this, cellPos);
            }
            MyCellCoord coord1 = new MyCellCoord(0, cellPos);
            Vector3 vector1 = ((cellPos * 8) + 8) + 1;
            return true;
        }

        [Conditional("DEBUG")]
        public void AddCellDebug(Vector3I cellPos)
        {
            HashSet<Vector3I> adjacentCellPos = new HashSet<Vector3I>();
            this.AddCell(cellPos, ref adjacentCellPos);
        }

        [Conditional("DEBUG")]
        private void AddDebugOuterEdge(ushort a, ushort b, List<DebugDrawEdge> debugEdgesList, Vector3D aTformed, Vector3D bTformed)
        {
            if (!this.m_connectionHelper.IsInnerEdge(a, b))
            {
                debugEdgesList.Add(new DebugDrawEdge((Vector3) aTformed, (Vector3) bTformed));
            }
        }

        public bool AddOneMarkedCell(List<Vector3D> importantPositions)
        {
            bool flag = false;
            List<Vector3I>.Enumerator enumerator = this.m_cellsOnWay.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Vector3I current = enumerator.Current;
                    if (!this.m_processedCells.Contains(ref current) && !this.m_markedForAddition.Contains(ref current))
                    {
                        this.MarkCellForAddition(current, this.CalculateCellWeight(importantPositions, current));
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
                goto TR_001A;
            }
        TR_000A:
            if (flag)
            {
                return flag;
            }
        TR_001A:
            while (true)
            {
                if (this.m_toAdd.Count == 0)
                {
                    return flag;
                }
                this.m_toAdd.QueryAll(this.m_heapItemList);
                float negativeInfinity = float.NegativeInfinity;
                CellToAddHeapItem item = null;
                foreach (CellToAddHeapItem item2 in this.m_heapItemList)
                {
                    float newKey = this.CalculateCellWeight(importantPositions, item2.Position);
                    if (newKey > negativeInfinity)
                    {
                        negativeInfinity = newKey;
                        item = item2;
                    }
                    this.m_toAdd.Modify(item2, newKey);
                }
                this.m_heapItemList.Clear();
                if (item == null)
                {
                    return flag;
                }
                else if (negativeInfinity >= this.LimitAddingWeight)
                {
                    this.m_toAdd.Remove(item);
                    Vector3I position = item.Position;
                    m_heapItemAllocator.Deallocate(item);
                    this.m_markedForAddition.Remove(position);
                    m_adjacentCells.Clear();
                    if (this.AddCell(position, ref m_adjacentCells))
                    {
                        foreach (Vector3I vectori3 in m_adjacentCells)
                        {
                            float weight = this.CalculateCellWeight(importantPositions, vectori3);
                            this.MarkCellForAddition(vectori3, weight);
                        }
                        return true;
                    }
                }
                else
                {
                    return flag;
                }
                break;
            }
            goto TR_000A;
        }

        private float CalculateCellWeight(List<Vector3D> importantPositions, Vector3I cellPos)
        {
            Vector3D vectord;
            Vector3I geometryCellCoord = cellPos;
            MyVoxelCoordSystems.GeometryCellCenterCoordToWorldPos(this.m_voxelMap.PositionLeftBottomCorner, ref geometryCellCoord, out vectord);
            float positiveInfinity = float.PositiveInfinity;
            using (List<Vector3D>.Enumerator enumerator = importantPositions.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    float num2 = Vector3.RectangularDistance(enumerator.Current, (Vector3) vectord);
                    if (num2 < positiveInfinity)
                    {
                        positiveInfinity = num2;
                    }
                }
            }
            if (this.m_cellsOnWayCoords.Contains(MyCellCoord.PackId64Static(0, cellPos)))
            {
                positiveInfinity -= 8f;
            }
            return GetWeight(positiveInfinity);
        }

        [Conditional("DEBUG")]
        private void CheckOuterEdgeConsistency()
        {
            if (DO_CONSISTENCY_CHECKS)
            {
                foreach (MyTuple<MyVoxelConnectionHelper.OuterEdgePoint, Vector3> local1 in new List<MyTuple<MyVoxelConnectionHelper.OuterEdgePoint, Vector3>>())
                {
                    int edgeIndex = local1.Item1.EdgeIndex;
                    MyWingedEdgeMesh.Edge edge = base.Mesh.GetEdge(edgeIndex);
                    if (local1.Item1.FirstPoint)
                    {
                        edge.GetFaceSuccVertex(-1);
                        continue;
                    }
                    edge.GetFacePredVertex(-1);
                }
            }
        }

        public override unsafe void DebugDraw(ref Matrix drawMatrix)
        {
            if (MyFakes.DEBUG_DRAW_NAVMESH_PROCESSED_VOXEL_CELLS)
            {
                Vector3 vector = Vector3.TransformNormal(this.m_cellSize, (Matrix) drawMatrix);
                Vector3 vector2 = Vector3.Transform((Vector3) (this.m_voxelMap.PositionLeftBottomCorner - this.m_voxelMap.PositionComp.GetPosition()), (Matrix) drawMatrix);
                foreach (Vector3I vectori in this.m_processedCells)
                {
                    BoundingBoxD xd;
                    xd.Min = vector2 + (vector * (new Vector3(0.0625f) + vectori));
                    BoundingBoxD* xdPtr1 = (BoundingBoxD*) ref xd;
                    xdPtr1->Max = xd.Min + vector;
                    xd.Inflate((double) -0.20000000298023224);
                    MyRenderProxy.DebugDrawAABB(xd, Color.Orange, 1f, 1f, false, false, false);
                    MyRenderProxy.DebugDrawText3D(xd.Center, vectori.ToString(), Color.Orange, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
            }
            if (MyFakes.DEBUG_DRAW_NAVMESH_CELLS_ON_PATHS)
            {
                Vector3 vector3 = Vector3.TransformNormal(this.m_cellSize, (Matrix) drawMatrix);
                Vector3 vector4 = Vector3.Transform((Vector3) (this.m_voxelMap.PositionLeftBottomCorner - this.m_voxelMap.PositionComp.GetPosition()), (Matrix) drawMatrix);
                MyCellCoord coord = new MyCellCoord();
                foreach (ulong num in this.m_cellsOnWayCoords)
                {
                    BoundingBoxD xd2;
                    coord.SetUnpack(num);
                    Vector3I coordInLod = coord.CoordInLod;
                    xd2.Min = vector4 + (vector3 * (new Vector3(0.0625f) + coordInLod));
                    BoundingBoxD* xdPtr2 = (BoundingBoxD*) ref xd2;
                    xdPtr2->Max = xd2.Min + vector3;
                    xd2.Inflate((double) -0.30000001192092896);
                    MyRenderProxy.DebugDrawAABB(xd2, Color.Green, 1f, 1f, false, false, false);
                }
            }
            if (MyFakes.DEBUG_DRAW_NAVMESH_PREPARED_VOXEL_CELLS)
            {
                Vector3 vector5 = Vector3.TransformNormal(this.m_cellSize, (Matrix) drawMatrix);
                Vector3 vector6 = Vector3.Transform((Vector3) (this.m_voxelMap.PositionLeftBottomCorner - this.m_voxelMap.PositionComp.GetPosition()), (Matrix) drawMatrix);
                float negativeInfinity = float.NegativeInfinity;
                Vector3I zero = Vector3I.Zero;
                int index = 0;
                while (true)
                {
                    if (index >= this.m_toAdd.Count)
                    {
                        for (int i = 0; i < this.m_toAdd.Count; i++)
                        {
                            BoundingBoxD xd3;
                            CellToAddHeapItem local1 = this.m_toAdd.GetItem(i);
                            float num6 = local1.HeapKey;
                            Vector3I position = local1.Position;
                            xd3.Min = vector6 + (vector5 * (new Vector3(0.0625f) + position));
                            BoundingBoxD* xdPtr3 = (BoundingBoxD*) ref xd3;
                            xdPtr3->Max = xd3.Min + vector5;
                            xd3.Inflate((double) -0.10000000149011612);
                            Color aqua = Color.Aqua;
                            if (position.Equals(zero))
                            {
                                aqua = Color.Red;
                            }
                            MyRenderProxy.DebugDrawAABB(xd3, aqua, 1f, 1f, false, false, false);
                            string text = $"{num6.ToString("n2")}";
                            MyRenderProxy.DebugDrawText3D(xd3.Center, text, aqua, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                        }
                        break;
                    }
                    CellToAddHeapItem item = this.m_toAdd.GetItem(index);
                    float heapKey = item.HeapKey;
                    if (heapKey > negativeInfinity)
                    {
                        negativeInfinity = heapKey;
                        zero = item.Position;
                    }
                    index++;
                }
            }
            MyRenderProxy.DebugDrawSphere(this.m_debugPos1, 0.2f, Color.Red, 1f, false, false, true, false);
            MyRenderProxy.DebugDrawSphere(this.m_debugPos2, 0.2f, Color.Green, 1f, false, false, true, false);
            MyRenderProxy.DebugDrawSphere(this.m_debugPos3, 0.1f, Color.Red, 1f, false, false, true, false);
            MyRenderProxy.DebugDrawSphere(this.m_debugPos4, 0.1f, Color.Green, 1f, false, false, true, false);
            if (MyFakes.DEBUG_DRAW_VOXEL_CONNECTION_HELPER)
            {
                this.m_connectionHelper.DebugDraw(ref drawMatrix, base.Mesh);
            }
            if (MyFakes.DEBUG_DRAW_NAVMESH_CELL_BORDERS)
            {
                foreach (KeyValuePair<ulong, List<DebugDrawEdge>> pair in this.m_debugCellEdges)
                {
                    foreach (DebugDrawEdge edge in pair.Value)
                    {
                        MyRenderProxy.DebugDrawLine3D(edge.V1, edge.V2, Color.Orange, Color.Orange, false, false);
                    }
                }
            }
            else
            {
                this.m_debugCellEdges.Clear();
            }
            if (MyFakes.DEBUG_DRAW_NAVMESH_HIERARCHY)
            {
                if (MyFakes.DEBUG_DRAW_NAVMESH_HIERARCHY_LITE)
                {
                    this.m_higherLevel.DebugDraw(true);
                }
                else
                {
                    this.m_higherLevel.DebugDraw(false);
                    this.m_higherLevelHelper.DebugDraw();
                }
            }
            if ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES == MyWEMDebugDrawMode.LINES) && !(this.m_voxelMap is MyVoxelPhysics))
            {
                int num7 = 0;
                MyWingedEdgeMesh.EdgeEnumerator edges = base.Mesh.GetEdges(null);
                Vector3D position = this.m_voxelMap.PositionComp.GetPosition();
                while (edges.MoveNext())
                {
                    MyWingedEdgeMesh.Edge current = edges.Current;
                    Vector3D vectord2 = base.Mesh.GetVertexPosition(current.Vertex2) + position;
                    Vector3D point = ((base.Mesh.GetVertexPosition(edges.Current.Vertex1) + position) + vectord2) * 0.5;
                    if (MyCestmirPathfindingShorts.Pathfinding.Obstacles.IsInObstacle(point))
                    {
                        MyRenderProxy.DebugDrawSphere(point, 0.05f, Color.Red, 1f, false, false, true, false);
                    }
                    num7++;
                }
            }
        }

        public override MyNavigationPrimitive FindClosestPrimitive(Vector3D point, bool highLevel, ref double closestDistanceSq)
        {
            MatrixD worldMatrixNormalizedInv = this.m_voxelMap.PositionComp.WorldMatrixNormalizedInv;
            Vector3 vector = (Vector3) Vector3D.Transform(point, worldMatrixNormalizedInv);
            float num = (float) closestDistanceSq;
            MyNavigationPrimitive primitive = null;
            primitive = !highLevel ? ((MyNavigationPrimitive) this.GetClosestNavigationTriangle(ref vector, ref num)) : ((MyNavigationPrimitive) this.GetClosestHighLevelPrimitive(ref vector, ref num));
            if (primitive != null)
            {
                closestDistanceSq = num;
            }
            return primitive;
        }

        public List<Vector4D> FindPath(Vector3 start, Vector3 end)
        {
            float positiveInfinity = float.PositiveInfinity;
            MyNavigationTriangle closestNavigationTriangle = this.GetClosestNavigationTriangle(ref start, ref positiveInfinity);
            if (closestNavigationTriangle == null)
            {
                return null;
            }
            positiveInfinity = float.PositiveInfinity;
            MyNavigationTriangle triangle2 = this.GetClosestNavigationTriangle(ref end, ref positiveInfinity);
            if (triangle2 == null)
            {
                return null;
            }
            this.m_debugPos1 = (Vector3) Vector3.Transform(closestNavigationTriangle.Position, this.m_voxelMap.PositionComp.WorldMatrix);
            this.m_debugPos2 = (Vector3) Vector3.Transform(triangle2.Position, this.m_voxelMap.PositionComp.WorldMatrix);
            this.m_debugPos3 = (Vector3) Vector3.Transform(start, this.m_voxelMap.PositionComp.WorldMatrix);
            this.m_debugPos4 = (Vector3) Vector3.Transform(end, this.m_voxelMap.PositionComp.WorldMatrix);
            return base.FindRefinedPath(closestNavigationTriangle, triangle2, ref start, ref end);
        }

        public List<Vector4D> FindPathGlobal(Vector3D start, Vector3D end)
        {
            Vector3D vectord1 = Vector3D.Transform(start, this.m_voxelMap.PositionComp.WorldMatrixNormalizedInv);
            start = vectord1;
            Vector3D vectord2 = Vector3D.Transform(end, this.m_voxelMap.PositionComp.WorldMatrixNormalizedInv);
            end = vectord2;
            return this.FindPath((Vector3) start, (Vector3) end);
        }

        private MyHighLevelPrimitive GetClosestHighLevelPrimitive(ref Vector3 point, ref float closestDistanceSq)
        {
            MyHighLevelPrimitive primitive = null;
            m_tmpIntList.Clear();
            Vector3I vectori = Vector3I.Round((point + (this.m_voxelMap.PositionComp.GetPosition() - this.m_voxelMap.PositionLeftBottomCorner)) / this.m_cellSize);
            for (int i = 0; i < 8; i++)
            {
                Vector3I coordInLod = (Vector3I) (vectori + m_cornerOffsets[i]);
                ulong packedCoord = new MyCellCoord(0, coordInLod).PackId64();
                this.m_higherLevelHelper.CollectComponents(packedCoord, m_tmpIntList);
            }
            foreach (int num3 in m_tmpIntList)
            {
                MyHighLevelPrimitive primitive2 = this.m_higherLevel.GetPrimitive(num3);
                if (primitive2 != null)
                {
                    float num4 = Vector3.DistanceSquared(primitive2.Position, point);
                    if (num4 < closestDistanceSq)
                    {
                        closestDistanceSq = num4;
                        primitive = primitive2;
                    }
                }
            }
            m_tmpIntList.Clear();
            return primitive;
        }

        private MyNavigationTriangle GetClosestNavigationTriangle(ref Vector3 point, ref float closestDistanceSq)
        {
            MyNavigationTriangle triangle = null;
            Vector3I vectori = Vector3I.Round((point + (this.m_voxelMap.PositionComp.GetPosition() - this.m_voxelMap.PositionLeftBottomCorner)) / this.m_cellSize);
            for (int i = 0; i < 8; i++)
            {
                Vector3I position = (Vector3I) (vectori + m_cornerOffsets[i]);
                if (this.m_processedCells.Contains(position))
                {
                    ulong packedCellCoord = new MyCellCoord(0, position).PackId64();
                    MyIntervalList list = this.m_higherLevelHelper.TryGetTriangleList(packedCellCoord);
                    if (list != null)
                    {
                        MyIntervalList.Enumerator enumerator = list.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            int current = enumerator.Current;
                            MyNavigationTriangle triangle2 = base.GetTriangle(current);
                            float num4 = Vector3.DistanceSquared(triangle2.Center, point);
                            if (num4 < closestDistanceSq)
                            {
                                closestDistanceSq = num4;
                                triangle = triangle2;
                            }
                        }
                    }
                }
            }
            return triangle;
        }

        public override IMyHighLevelComponent GetComponent(MyHighLevelPrimitive highLevelPrimitive) => 
            this.m_higherLevelHelper.GetComponent(highLevelPrimitive);

        public override MyHighLevelPrimitive GetHighLevelPrimitive(MyNavigationPrimitive myNavigationTriangle) => 
            this.m_higherLevelHelper.GetHighLevelNavigationPrimitive(myNavigationTriangle as MyNavigationTriangle);

        private static float GetWeight(float rectDistance) => 
            ((rectDistance >= 0f) ? (1f / (1f + rectDistance)) : 1f);

        public override MatrixD GetWorldMatrix() => 
            this.m_voxelMap.WorldMatrix;

        public override Vector3 GlobalToLocal(Vector3D globalPos) => 
            ((Vector3) Vector3D.Transform(globalPos, this.m_voxelMap.PositionComp.WorldMatrixNormalizedInv));

        public void InvalidateRange(Vector3I minVoxelChanged, Vector3I maxVoxelChanged)
        {
            Vector3I vectori;
            Vector3I vectori2;
            minVoxelChanged -= MyPrecalcComponent.InvalidatedRangeInflate;
            maxVoxelChanged = (Vector3I) (maxVoxelChanged + MyPrecalcComponent.InvalidatedRangeInflate);
            this.m_voxelMap.Storage.ClampVoxelCoord(ref minVoxelChanged, 1);
            this.m_voxelMap.Storage.ClampVoxelCoord(ref maxVoxelChanged, 1);
            MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref minVoxelChanged, out vectori);
            MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref maxVoxelChanged, out vectori2);
            Vector3I position = vectori;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref vectori, ref vectori2);
            while (iterator.IsValid())
            {
                if (!this.m_processedCells.Contains(ref position))
                {
                    this.m_higherLevelHelper.TryClearCell(new MyCellCoord(0, position).PackId64());
                }
                else if (!this.m_cellsToChangeSet.Contains(ref position))
                {
                    this.m_cellsToChange.AddLast(position);
                    this.m_cellsToChangeSet.Add(position);
                }
                iterator.GetNext(out position);
            }
        }

        private bool IsCellPosValid(ref Vector3I cellPos)
        {
            if (((cellPos.X > this.m_maxCellCoord.X) || (cellPos.Y > this.m_maxCellCoord.Y)) || (cellPos.Z > this.m_maxCellCoord.Z))
            {
                return false;
            }
            MyCellCoord coord = new MyCellCoord(0, (Vector3I) cellPos);
            return coord.IsCoord64Valid();
        }

        public override Vector3D LocalToGlobal(Vector3 localPos) => 
            Vector3D.Transform(localPos, this.m_voxelMap.WorldMatrix);

        public unsafe void MarkBoxForAddition(BoundingBoxD box)
        {
            Vector3I vectori;
            Vector3I vectori2;
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.m_voxelMap.PositionLeftBottomCorner, ref box.Min, out vectori);
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.m_voxelMap.PositionLeftBottomCorner, ref box.Max, out vectori2);
            this.m_voxelMap.Storage.ClampVoxelCoord(ref vectori, 1);
            this.m_voxelMap.Storage.ClampVoxelCoord(ref vectori2, 1);
            Vector3I* vectoriPtr1 = (Vector3I*) ref vectori;
            MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref (Vector3I) ref vectoriPtr1, out vectori);
            Vector3I* vectoriPtr2 = (Vector3I*) ref vectori2;
            MyVoxelCoordSystems.VoxelCoordToGeometryCellCoord(ref (Vector3I) ref vectoriPtr2, out vectori2);
            Vector3 vector = (vectori + vectori2) * 0.5f;
            vectori = (Vector3I) (vectori / 1);
            vectori2 = (Vector3I) (vectori2 / 1);
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref vectori, ref vectori2);
            while (iterator.IsValid())
            {
                if (Vector3.RectangularDistance((Vector3) vectori, vector) <= 1f)
                {
                    this.MarkCellForAddition(vectori, PresentEntityWeight);
                }
                iterator.GetNext(out vectori);
            }
        }

        private void MarkCellForAddition(Vector3I cellPos, float weight)
        {
            if ((!this.m_processedCells.Contains(ref cellPos) && !this.m_markedForAddition.Contains(ref cellPos)) && this.IsCellPosValid(ref cellPos))
            {
                if (!this.m_toAdd.Full)
                {
                    this.MarkCellForAdditionInternal(ref cellPos, weight);
                }
                else
                {
                    float heapKey = this.m_toAdd.Min().HeapKey;
                    if (weight > heapKey)
                    {
                        this.RemoveMinMarkedForAddition();
                        this.MarkCellForAdditionInternal(ref cellPos, weight);
                    }
                }
            }
        }

        private void MarkCellForAdditionInternal(ref Vector3I cellPos, float weight)
        {
            CellToAddHeapItem item = m_heapItemAllocator.Allocate();
            item.Position = cellPos;
            this.m_toAdd.Insert(item, weight);
            this.m_markedForAddition.Add((Vector3I) cellPos);
        }

        public void MarkCellsOnPaths()
        {
            this.m_primitivesOnPath.Clear();
            this.m_higherLevel.GetPrimitivesOnPath(ref this.m_primitivesOnPath);
            this.m_cellsOnWayCoords.Clear();
            this.m_higherLevelHelper.GetCellsOfPrimitives(ref this.m_cellsOnWayCoords, ref this.m_primitivesOnPath);
            this.m_cellsOnWay.Clear();
            foreach (ulong num in this.m_cellsOnWayCoords)
            {
                MyCellCoord coord = new MyCellCoord();
                coord.SetUnpack(num);
                Vector3I coordInLod = coord.CoordInLod;
                this.m_cellsOnWay.Add(coordInLod);
            }
        }

        private void OnStorageChanged(Vector3I minVoxelChanged, Vector3I maxVoxelChanged, MyStorageDataTypeFlags changedData)
        {
            if (changedData.HasFlag(MyStorageDataTypeFlags.Content))
            {
                this.InvalidateRange(minVoxelChanged, maxVoxelChanged);
            }
        }

        private void PreprocessTriangles(MyIsoMesh generatedMesh, Vector3 centerDisplacement)
        {
            for (int i = 0; i < generatedMesh.TrianglesCount; i++)
            {
                Vector3 vector3;
                ushort idx = generatedMesh.Triangles[i].V0;
                ushort num3 = generatedMesh.Triangles[i].V1;
                ushort num4 = generatedMesh.Triangles[i].V2;
                generatedMesh.GetUnpackedPosition(idx, out vector3);
                Vector3 vector = vector3 - centerDisplacement;
                generatedMesh.GetUnpackedPosition(num3, out vector3);
                Vector3 vector2 = vector3 - centerDisplacement;
                generatedMesh.GetUnpackedPosition(num4, out vector3);
                bool flag = false;
                if ((vector2 - vector).LengthSquared() <= MyVoxelConnectionHelper.OUTER_EDGE_EPSILON_SQ)
                {
                    m_vertexMapping.Union(idx, num3);
                    flag = true;
                }
                Vector3 vector1 = vector3 - centerDisplacement;
                if (((vector3 - centerDisplacement) - vector).LengthSquared() <= MyVoxelConnectionHelper.OUTER_EDGE_EPSILON_SQ)
                {
                    m_vertexMapping.Union(idx, num4);
                    flag = true;
                }
                Vector3 vector4 = (vector3 - centerDisplacement) - vector2;
                if (vector4.LengthSquared() <= MyVoxelConnectionHelper.OUTER_EDGE_EPSILON_SQ)
                {
                    m_vertexMapping.Union(num3, num4);
                    flag = true;
                }
                if (!flag)
                {
                    this.m_connectionHelper.PreprocessInnerEdge(idx, num3);
                    this.m_connectionHelper.PreprocessInnerEdge(num3, num4);
                    this.m_connectionHelper.PreprocessInnerEdge(num4, idx);
                }
            }
        }

        public bool RefreshOneChangedCell()
        {
            bool flag = false;
            while (!flag)
            {
                if (this.m_cellsToChange.Count == 0)
                {
                    return flag;
                }
                Vector3I position = this.m_cellsToChange.First.Value;
                this.m_cellsToChange.RemoveFirst();
                this.m_cellsToChangeSet.Remove(ref position);
                if (!this.m_processedCells.Contains(ref position))
                {
                    this.m_higherLevelHelper.TryClearCell(new MyCellCoord(0, position).PackId64());
                    continue;
                }
                this.RemoveCell(position);
                this.MarkCellForAddition(position, RecountCellWeight);
                flag = true;
            }
            return flag;
        }

        private bool RemoveCell(Vector3I cell)
        {
            if (!MyFakes.REMOVE_VOXEL_NAVMESH_CELLS)
            {
                return true;
            }
            if (!this.m_processedCells.Contains(cell))
            {
                return false;
            }
            if (MyFakes.LOG_NAVMESH_GENERATION)
            {
                MyCestmirPathfindingShorts.Pathfinding.VoxelPathfinding.DebugLog.LogCellRemoval(this, cell);
            }
            MyVoxelPathfinding.CellId cellId = new MyVoxelPathfinding.CellId {
                VoxelMap = this.m_voxelMap,
                Pos = cell
            };
            this.m_navmeshCoordinator.RemoveVoxelNavmeshLinks(cellId);
            ulong packedCellCoord = new MyCellCoord(0, cell).PackId64();
            MyIntervalList list = this.m_higherLevelHelper.TryGetTriangleList(packedCellCoord);
            if (list != null)
            {
                MyIntervalList.Enumerator enumerator = list.GetEnumerator();
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        this.m_higherLevelHelper.ClearCachedCell(packedCellCoord);
                        break;
                    }
                    int current = enumerator.Current;
                    this.RemoveTerrainTriangle(base.GetTriangle(current));
                }
            }
            this.m_processedCells.Remove(ref cell);
            return (list != null);
        }

        [Conditional("DEBUG")]
        public void RemoveCellDebug(Vector3I cellPos)
        {
            this.RemoveCell(cellPos);
        }

        public void RemoveFarHighLevelGroups(List<Vector3D> updatePositions)
        {
            this.m_higherLevelHelper.RemoveTooFarCells(updatePositions, 200f, this.m_processedCells);
        }

        private void RemoveMinMarkedForAddition()
        {
            CellToAddHeapItem item = this.m_toAdd.RemoveMin();
            m_heapItemAllocator.Deallocate(item);
            this.m_markedForAddition.Remove(item.Position);
        }

        public bool RemoveOneUnusedCell(List<Vector3D> importantPositions)
        {
            m_tmpCellSet.Clear();
            m_tmpCellSet.Union(this.m_processedCells);
            bool flag = false;
            foreach (Vector3I vectori in m_tmpCellSet)
            {
                Vector3 vector;
                Vector3D vectord;
                Vector3I geometryCellCoord = vectori * 1;
                MyVoxelCoordSystems.GeometryCellCoordToLocalPosition(ref geometryCellCoord, out vector);
                vector += new Vector3D(0.5);
                MyVoxelCoordSystems.LocalPositionToWorldPosition(this.m_voxelMap.PositionLeftBottomCorner, ref vector, out vectord);
                bool flag2 = true;
                foreach (Vector3D vectord2 in importantPositions)
                {
                    if (Vector3D.RectangularDistance(vectord, vectord2) < 50.0)
                    {
                        flag2 = false;
                        break;
                    }
                }
                if ((flag2 && !this.m_markedForAddition.Contains(vectori)) && this.RemoveCell(vectori))
                {
                    Vector3I cellPos = vectori;
                    this.MarkCellForAddition(cellPos, this.CalculateCellWeight(importantPositions, cellPos));
                    flag = true;
                    break;
                }
            }
            m_tmpCellSet.Clear();
            return flag;
        }

        private void RemoveTerrainTriangle(MyNavigationTriangle tri)
        {
            MyWingedEdgeMesh.FaceVertexEnumerator vertexEnumerator = tri.GetVertexEnumerator();
            vertexEnumerator.MoveNext();
            Vector3 current = vertexEnumerator.Current;
            vertexEnumerator.MoveNext();
            Vector3 vector2 = vertexEnumerator.Current;
            vertexEnumerator.MoveNext();
            Vector3 vector3 = vertexEnumerator.Current;
            int edgeIndex = tri.GetEdgeIndex(0);
            int num2 = tri.GetEdgeIndex(1);
            int num3 = tri.GetEdgeIndex(2);
            int num4 = edgeIndex;
            if (!this.m_connectionHelper.TryRemoveOuterEdge(ref current, ref vector2, ref num4) && (base.Mesh.GetEdge(edgeIndex).OtherFace(tri.Index) != -1))
            {
                this.m_connectionHelper.AddOuterEdgeIndex(ref vector2, ref current, edgeIndex);
            }
            num4 = num2;
            if (!this.m_connectionHelper.TryRemoveOuterEdge(ref vector2, ref vector3, ref num4) && (base.Mesh.GetEdge(num2).OtherFace(tri.Index) != -1))
            {
                this.m_connectionHelper.AddOuterEdgeIndex(ref vector3, ref vector2, num2);
            }
            num4 = num3;
            if (!this.m_connectionHelper.TryRemoveOuterEdge(ref vector3, ref current, ref num4) && (base.Mesh.GetEdge(num3).OtherFace(tri.Index) != -1))
            {
                this.m_connectionHelper.AddOuterEdgeIndex(ref current, ref vector3, num3);
            }
            base.RemoveTriangle(tri);
        }

        public void RemoveTriangle(int index)
        {
            MyNavigationTriangle tri = base.GetTriangle(index);
            this.RemoveTerrainTriangle(tri);
        }

        public override string ToString() => 
            ("Voxel NavMesh: " + this.m_voxelMap.StorageName);

        public static MyVoxelBase VoxelMap =>
            m_staticVoxelMap;

        public Vector3D VoxelMapReferencePosition =>
            this.m_voxelMap.PositionLeftBottomCorner;

        public Vector3D VoxelMapWorldPosition =>
            this.m_voxelMap.PositionComp.GetPosition();

        public override MyHighLevelGroup HighLevelGroup =>
            this.m_higherLevel;

        private class CellToAddHeapItem : HeapItem<float>
        {
            public Vector3I Position;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DebugDrawEdge
        {
            public Vector3 V1;
            public Vector3 V2;
            public DebugDrawEdge(Vector3 v1, Vector3 v2)
            {
                this.V1 = v1;
                this.V2 = v2;
            }
        }
    }
}

