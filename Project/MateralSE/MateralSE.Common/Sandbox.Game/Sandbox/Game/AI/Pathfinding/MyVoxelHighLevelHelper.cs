namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Engine.Utils;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    public class MyVoxelHighLevelHelper
    {
        public static readonly bool DO_CONSISTENCY_CHECKS = true;
        private MyVoxelNavigationMesh m_mesh;
        private bool m_cellOpen;
        private MyIntervalList m_triangleList;
        private int m_currentComponentRel;
        private int m_currentComponentMarker;
        private Vector3I m_currentCell;
        private ulong m_packedCoord;
        private List<List<ConnectionInfo>> m_currentCellConnections;
        private static MyVoxelHighLevelHelper m_currentHelper;
        private Dictionary<ulong, MyIntervalList> m_triangleLists;
        private MyVector3ISet m_exploredCells;
        private MyNavmeshComponents m_navmeshComponents;
        private Predicate<MyNavigationPrimitive> m_processTrianglePredicate = new Predicate<MyNavigationPrimitive>(MyVoxelHighLevelHelper.ProcessTriangleForHierarchyStatic);
        private List<MyNavigationTriangle> m_tmpComponentTriangles = new List<MyNavigationTriangle>();
        private List<int> m_tmpNeighbors = new List<int>();
        private static List<ulong> m_removedHLpackedCoord = new List<ulong>();

        public MyVoxelHighLevelHelper(MyVoxelNavigationMesh mesh)
        {
            this.m_mesh = mesh;
            this.m_triangleList = new MyIntervalList();
            this.m_triangleLists = new Dictionary<ulong, MyIntervalList>();
            this.m_exploredCells = new MyVector3ISet();
            this.m_navmeshComponents = new MyNavmeshComponents();
            this.m_currentCellConnections = new List<List<ConnectionInfo>>();
            for (int i = 0; i < 8; i++)
            {
                this.m_currentCellConnections.Add(new List<ConnectionInfo>());
            }
        }

        public void AddExplored(ref Vector3I cellPos)
        {
            this.m_exploredCells.Add(ref cellPos);
        }

        public void AddTriangle(int triIndex)
        {
            this.m_triangleList.Add(triIndex);
        }

        [Conditional("DEBUG")]
        public void CheckConsistency()
        {
            if (DO_CONSISTENCY_CHECKS)
            {
                MyCellCoord coord = new MyCellCoord();
                foreach (KeyValuePair<ulong, MyIntervalList> pair in this.m_triangleLists)
                {
                    coord.SetUnpack(pair.Key);
                }
            }
        }

        public void ClearCachedCell(ulong packedCoord)
        {
            MyNavmeshComponents.CellInfo info;
            this.m_triangleLists.Remove(packedCoord);
            if (this.m_navmeshComponents.TryGetCell(packedCoord, out info))
            {
                for (int i = 0; i < info.ComponentNum; i++)
                {
                    int index = info.StartingIndex + i;
                    MyHighLevelPrimitive primitive = this.m_mesh.HighLevelGroup.GetPrimitive(index);
                    if (primitive != null)
                    {
                        primitive.IsExpanded = false;
                    }
                }
            }
        }

        public void CloseCell()
        {
            this.m_cellOpen = false;
            this.m_packedCoord = 0UL;
            this.m_triangleList.Clear();
        }

        public void CollectComponents(ulong packedCoord, List<int> output)
        {
            MyNavmeshComponents.CellInfo cellInfo = new MyNavmeshComponents.CellInfo();
            if (this.m_navmeshComponents.TryGetCell(packedCoord, out cellInfo))
            {
                for (int i = 0; i < cellInfo.ComponentNum; i++)
                {
                    output.Add(cellInfo.StartingIndex + i);
                }
            }
        }

        private MyNavmeshComponents.ClosedCellInfo ConstructComponents()
        {
            long start = this.m_mesh.GetCurrentTimestamp() + 1L;
            long end = start;
            this.m_currentComponentRel = 0;
            this.m_navmeshComponents.OpenCell(this.m_packedCoord);
            this.m_tmpComponentTriangles.Clear();
            MyIntervalList.Enumerator enumerator = this.m_triangleList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                int current = enumerator.Current;
                this.m_currentComponentMarker = -2 - this.m_currentComponentRel;
                MyNavigationTriangle vertex = this.m_mesh.GetTriangle(current);
                if (!this.m_mesh.VisitedBetween(vertex, start, end))
                {
                    this.m_navmeshComponents.OpenComponent();
                    if (this.m_currentComponentRel >= this.m_currentCellConnections.Count)
                    {
                        this.m_currentCellConnections.Add(new List<ConnectionInfo>());
                    }
                    m_currentHelper = this;
                    this.m_navmeshComponents.AddComponentTriangle(vertex, vertex.Center);
                    vertex.ComponentIndex = this.m_currentComponentMarker;
                    this.m_tmpComponentTriangles.Add(vertex);
                    this.m_mesh.PrepareTraversal(vertex, null, this.m_processTrianglePredicate, null);
                    this.m_mesh.PerformTraversal();
                    this.m_tmpComponentTriangles.Add(null);
                    this.m_navmeshComponents.CloseComponent();
                    end = this.m_mesh.GetCurrentTimestamp();
                    this.m_currentComponentRel++;
                }
            }
            MyNavmeshComponents.ClosedCellInfo output = new MyNavmeshComponents.ClosedCellInfo();
            this.m_navmeshComponents.CloseAndCacheCell(ref output);
            return output;
        }

        public void DebugDraw()
        {
            if (MyFakes.DEBUG_DRAW_NAVMESH_EXPLORED_HL_CELLS)
            {
                foreach (Vector3I vectori in this.m_exploredCells)
                {
                    BoundingBoxD xd;
                    MyVoxelCoordSystems.GeometryCellCoordToWorldAABB(this.m_mesh.VoxelMapReferencePosition, ref vectori, out xd);
                    MyRenderProxy.DebugDrawAABB(xd, Color.Sienna, 1f, 1f, false, false, false);
                }
            }
            if (MyFakes.DEBUG_DRAW_NAVMESH_FRINGE_HL_CELLS)
            {
                foreach (ulong num in this.m_navmeshComponents.GetPresentCells())
                {
                    MyCellCoord coord = new MyCellCoord();
                    coord.SetUnpack(num);
                    Vector3I coordInLod = coord.CoordInLod;
                    if (this.m_exploredCells.Contains(ref coordInLod))
                    {
                        MyNavmeshComponents.CellInfo cellInfo = new MyNavmeshComponents.CellInfo();
                        if (this.m_navmeshComponents.TryGetCell(num, out cellInfo))
                        {
                            int num2 = 0;
                            while (num2 < cellInfo.ComponentNum)
                            {
                                int index = cellInfo.StartingIndex + num2;
                                MyHighLevelPrimitive primitive = this.m_mesh.HighLevelGroup.GetPrimitive(index);
                                Base6Directions.Direction[] enumDirections = Base6Directions.EnumDirections;
                                int num4 = 0;
                                while (true)
                                {
                                    if (num4 >= enumDirections.Length)
                                    {
                                        num2++;
                                        break;
                                    }
                                    Base6Directions.Direction dir = enumDirections[num4];
                                    Base6Directions.DirectionFlags directionFlag = Base6Directions.GetDirectionFlag(dir);
                                    if (!cellInfo.ExploredDirections.HasFlag(directionFlag) && !this.m_exploredCells.Contains((Vector3I) (coordInLod + Base6Directions.GetIntVector(dir))))
                                    {
                                        Vector3 vector = Base6Directions.GetVector(dir);
                                        MyRenderProxy.DebugDrawLine3D(primitive.WorldPosition, primitive.WorldPosition + (vector * 3f), Color.Red, Color.Red, false, false);
                                    }
                                    num4++;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void GetCellsOfPrimitives(ref HashSet<ulong> cells, ref List<MyHighLevelPrimitive> primitives)
        {
            foreach (MyHighLevelPrimitive primitive in primitives)
            {
                ulong num;
                if (this.m_navmeshComponents.GetComponentCell(primitive.Index, out num))
                {
                    cells.Add(num);
                }
            }
        }

        public IMyHighLevelComponent GetComponent(MyHighLevelPrimitive primitive)
        {
            ulong num;
            Base6Directions.DirectionFlags flags;
            if (!this.m_navmeshComponents.GetComponentCell(primitive.Index, out num))
            {
                return null;
            }
            if (!this.m_navmeshComponents.GetComponentInfo(primitive.Index, num, out flags))
            {
                return null;
            }
            MyCellCoord coord = new MyCellCoord();
            coord.SetUnpack(num);
            foreach (Base6Directions.Direction direction in Base6Directions.EnumDirections)
            {
                Base6Directions.DirectionFlags directionFlag = Base6Directions.GetDirectionFlag(direction);
                if (!flags.HasFlag(directionFlag))
                {
                    Vector3I position = (Vector3I) (coord.CoordInLod + Base6Directions.GetIntVector(direction));
                    if (this.m_exploredCells.Contains(ref position))
                    {
                        flags |= directionFlag;
                    }
                }
            }
            return new Component(primitive.Index, flags);
        }

        public MyHighLevelPrimitive GetHighLevelNavigationPrimitive(MyNavigationTriangle triangle) => 
            (ReferenceEquals(triangle.Parent, this.m_mesh) ? ((triangle.ComponentIndex == -1) ? null : this.m_mesh.HighLevelGroup.GetPrimitive(triangle.ComponentIndex)) : null);

        private unsafe void MarkExploredDirections(ref MyNavmeshComponents.ClosedCellInfo cellInfo)
        {
            foreach (Base6Directions.Direction direction in Base6Directions.EnumDirections)
            {
                Base6Directions.DirectionFlags directionFlag = Base6Directions.GetDirectionFlag(direction);
                if (!cellInfo.ExploredDirections.HasFlag(directionFlag))
                {
                    Vector3I intVector = Base6Directions.GetIntVector(direction);
                    MyCellCoord coord = new MyCellCoord {
                        Lod = 0,
                        CoordInLod = (Vector3I) (this.m_currentCell + intVector)
                    };
                    if (((coord.CoordInLod.X != -1) && (coord.CoordInLod.Y != -1)) && (coord.CoordInLod.Z != -1))
                    {
                        ulong key = coord.PackId64();
                        if (this.m_triangleLists.ContainsKey(key))
                        {
                            this.m_navmeshComponents.MarkExplored(key, Base6Directions.GetFlippedDirection(direction));
                            Base6Directions.DirectionFlags* flagsPtr1 = (Base6Directions.DirectionFlags*) ref cellInfo.ExploredDirections;
                            *((sbyte*) flagsPtr1) = *(((byte*) flagsPtr1)) | Base6Directions.GetDirectionFlag(direction);
                        }
                    }
                }
            }
            this.m_navmeshComponents.SetExplored(this.m_packedCoord, cellInfo.ExploredDirections);
        }

        public void OpenNewCell(MyCellCoord coord)
        {
            this.m_cellOpen = true;
            this.m_currentCell = coord.CoordInLod;
            this.m_packedCoord = coord.PackId64();
            this.m_triangleList.Clear();
        }

        public void ProcessCellComponents()
        {
            this.m_triangleLists.Add(this.m_packedCoord, this.m_triangleList.GetCopy());
            MyNavmeshComponents.ClosedCellInfo cellInfo = this.ConstructComponents();
            this.UpdateHighLevelPrimitives(ref cellInfo);
            this.MarkExploredDirections(ref cellInfo);
            for (int i = 0; i < cellInfo.ComponentNum; i++)
            {
                int index = cellInfo.StartingIndex + i;
                MyHighLevelPrimitive primitive = this.m_mesh.HighLevelGroup.GetPrimitive(index);
                if (primitive != null)
                {
                    primitive.IsExpanded = true;
                }
            }
        }

        private bool ProcessTriangleForHierarchy(MyNavigationTriangle triangle)
        {
            if (ReferenceEquals(triangle.Parent, this.m_mesh))
            {
                ulong num;
                if (triangle.ComponentIndex == -1)
                {
                    this.m_navmeshComponents.AddComponentTriangle(triangle, triangle.Center);
                    this.m_tmpComponentTriangles.Add(triangle);
                    triangle.ComponentIndex = this.m_currentComponentMarker;
                    return true;
                }
                if (triangle.ComponentIndex == this.m_currentComponentMarker)
                {
                    return true;
                }
                if (this.m_navmeshComponents.GetComponentCell(triangle.ComponentIndex, out num))
                {
                    MyCellCoord coord = new MyCellCoord();
                    coord.SetUnpack(num);
                    Vector3I vec = coord.CoordInLod - this.m_currentCell;
                    if (vec.RectangularLength() != 1)
                    {
                        return false;
                    }
                    ConnectionInfo item = new ConnectionInfo {
                        Direction = Base6Directions.GetDirection(vec),
                        ComponentIndex = triangle.ComponentIndex
                    };
                    if (!this.m_currentCellConnections[this.m_currentComponentRel].Contains(item))
                    {
                        this.m_currentCellConnections[this.m_currentComponentRel].Add(item);
                    }
                }
            }
            return false;
        }

        private static bool ProcessTriangleForHierarchyStatic(MyNavigationPrimitive primitive)
        {
            MyNavigationTriangle triangle = primitive as MyNavigationTriangle;
            return m_currentHelper.ProcessTriangleForHierarchy(triangle);
        }

        private void RemoveExplored(ulong packedCoord)
        {
            MyCellCoord coord = new MyCellCoord();
            coord.SetUnpack(packedCoord);
            this.m_exploredCells.Remove(ref coord.CoordInLod);
        }

        public void RemoveTooFarCells(List<Vector3D> importantPositions, float maxDistance, MyVector3ISet processedCells)
        {
            m_removedHLpackedCoord.Clear();
            foreach (Vector3I vectori in this.m_exploredCells)
            {
                Vector3D vectord;
                MyVoxelCoordSystems.GeometryCellCenterCoordToWorldPos(this.m_mesh.VoxelMapReferencePosition, ref vectori, out vectord);
                float positiveInfinity = float.PositiveInfinity;
                using (List<Vector3D>.Enumerator enumerator2 = importantPositions.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        float num2 = Vector3.RectangularDistance(enumerator2.Current, (Vector3) vectord);
                        if (num2 < positiveInfinity)
                        {
                            positiveInfinity = num2;
                        }
                    }
                }
                if ((positiveInfinity > maxDistance) && !processedCells.Contains(vectori))
                {
                    m_removedHLpackedCoord.Add(new MyCellCoord(0, vectori).PackId64());
                }
            }
            foreach (ulong num3 in m_removedHLpackedCoord)
            {
                this.TryClearCell(num3);
            }
        }

        public unsafe void TryClearCell(ulong packedCoord)
        {
            MyNavmeshComponents.CellInfo info;
            if (this.m_triangleLists.ContainsKey(packedCoord))
            {
                this.ClearCachedCell(packedCoord);
            }
            this.RemoveExplored(packedCoord);
            if (this.m_navmeshComponents.TryGetCell(packedCoord, out info))
            {
                for (int i = 0; i < info.ComponentNum; i++)
                {
                    int index = info.StartingIndex + i;
                    this.m_mesh.HighLevelGroup.RemovePrimitive(index);
                }
                foreach (Base6Directions.Direction direction in Base6Directions.EnumDirections)
                {
                    Base6Directions.DirectionFlags directionFlag = Base6Directions.GetDirectionFlag(direction);
                    if (info.ExploredDirections.HasFlag(directionFlag))
                    {
                        MyNavmeshComponents.CellInfo info2;
                        Vector3I intVector = Base6Directions.GetIntVector(direction);
                        MyCellCoord coord = new MyCellCoord();
                        coord.SetUnpack(packedCoord);
                        MyCellCoord* coordPtr1 = (MyCellCoord*) ref coord;
                        coordPtr1->CoordInLod = (Vector3I) (coord.CoordInLod + intVector);
                        if (this.m_navmeshComponents.TryGetCell(coord.PackId64(), out info2))
                        {
                            Base6Directions.DirectionFlags flags2 = Base6Directions.GetDirectionFlag(Base6Directions.GetFlippedDirection(direction));
                            this.m_navmeshComponents.SetExplored(coord.PackId64(), info2.ExploredDirections & ((byte) ~flags2));
                        }
                    }
                }
                this.m_navmeshComponents.ClearCell(packedCoord, ref info);
            }
        }

        public MyIntervalList TryGetTriangleList(ulong packedCellCoord)
        {
            MyIntervalList list = null;
            this.m_triangleLists.TryGetValue(packedCellCoord, out list);
            return list;
        }

        private void UpdateHighLevelPrimitives(ref MyNavmeshComponents.ClosedCellInfo cellInfo)
        {
            int startingIndex = cellInfo.StartingIndex;
            foreach (MyNavigationTriangle triangle in this.m_tmpComponentTriangles)
            {
                if (triangle == null)
                {
                    startingIndex++;
                    continue;
                }
                triangle.ComponentIndex = startingIndex;
            }
            this.m_tmpComponentTriangles.Clear();
            if (!cellInfo.NewCell && (cellInfo.ComponentNum != cellInfo.OldComponentNum))
            {
                for (int j = 0; j < cellInfo.OldComponentNum; j++)
                {
                    this.m_mesh.HighLevelGroup.RemovePrimitive(cellInfo.OldStartingIndex + j);
                }
            }
            if (cellInfo.NewCell || (cellInfo.ComponentNum != cellInfo.OldComponentNum))
            {
                for (int j = 0; j < cellInfo.ComponentNum; j++)
                {
                    this.m_mesh.HighLevelGroup.AddPrimitive(cellInfo.StartingIndex + j, this.m_navmeshComponents.GetComponentCenter(j));
                }
            }
            if (!cellInfo.NewCell && (cellInfo.ComponentNum == cellInfo.OldComponentNum))
            {
                for (int j = 0; j < cellInfo.ComponentNum; j++)
                {
                    this.m_mesh.HighLevelGroup.GetPrimitive(cellInfo.StartingIndex + j).UpdatePosition(this.m_navmeshComponents.GetComponentCenter(j));
                }
            }
            for (int i = 0; i < cellInfo.ComponentNum; i++)
            {
                int index = cellInfo.StartingIndex + i;
                this.m_mesh.HighLevelGroup.GetPrimitive(index).GetNeighbours(this.m_tmpNeighbors);
                foreach (ConnectionInfo info in this.m_currentCellConnections[i])
                {
                    if (!this.m_tmpNeighbors.Remove(info.ComponentIndex))
                    {
                        this.m_mesh.HighLevelGroup.ConnectPrimitives(index, info.ComponentIndex);
                    }
                }
                foreach (int num7 in this.m_tmpNeighbors)
                {
                    MyHighLevelPrimitive primitive = this.m_mesh.HighLevelGroup.TryGetPrimitive(num7);
                    if ((primitive != null) && primitive.IsExpanded)
                    {
                        this.m_mesh.HighLevelGroup.DisconnectPrimitives(index, num7);
                    }
                }
                this.m_tmpNeighbors.Clear();
                this.m_currentCellConnections[i].Clear();
            }
        }

        public class Component : IMyHighLevelComponent
        {
            private int m_componentIndex;
            private Base6Directions.DirectionFlags m_exploredDirections;

            public Component(int index, Base6Directions.DirectionFlags exploredDirections)
            {
                this.m_componentIndex = index;
                this.m_exploredDirections = exploredDirections;
            }

            bool IMyHighLevelComponent.Contains(MyNavigationPrimitive primitive)
            {
                MyNavigationTriangle triangle = primitive as MyNavigationTriangle;
                return ((triangle != null) ? (triangle.ComponentIndex == this.m_componentIndex) : false);
            }

            bool IMyHighLevelComponent.FullyExplored =>
                (this.m_exploredDirections == Base6Directions.DirectionFlags.All);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ConnectionInfo
        {
            public int ComponentIndex;
            public VRageMath.Base6Directions.Direction Direction;
        }
    }
}

