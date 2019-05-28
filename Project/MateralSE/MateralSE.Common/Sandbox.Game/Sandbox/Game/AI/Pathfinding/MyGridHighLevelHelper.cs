namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    public class MyGridHighLevelHelper
    {
        private MyGridNavigationMesh m_mesh;
        private Vector3I m_cellSize;
        private ulong m_packedCoord;
        private int m_currentComponentRel;
        private List<List<int>> m_currentCellConnections;
        private MyVector3ISet m_changedCells;
        private MyVector3ISet m_changedCubes;
        private Dictionary<Vector3I, List<int>> m_triangleRegistry;
        private MyNavmeshComponents m_components;
        private List<MyNavigationTriangle> m_tmpComponentTriangles = new List<MyNavigationTriangle>();
        private List<int> m_tmpNeighbors = new List<int>();
        private static HashSet<int> m_tmpCellTriangles = new HashSet<int>();
        private static MyGridHighLevelHelper m_currentHelper = null;
        private static readonly Vector3I CELL_COORD_SHIFT = new Vector3I(0x80000);
        private Predicate<MyNavigationPrimitive> m_processTrianglePredicate = new Predicate<MyNavigationPrimitive>(MyGridHighLevelHelper.ProcessTriangleForHierarchyStatic);

        public MyGridHighLevelHelper(MyGridNavigationMesh mesh, Dictionary<Vector3I, List<int>> triangleRegistry, Vector3I cellSize)
        {
            this.m_mesh = mesh;
            this.m_cellSize = cellSize;
            this.m_packedCoord = 0UL;
            this.m_currentCellConnections = new List<List<int>>();
            this.m_changedCells = new MyVector3ISet();
            this.m_changedCubes = new MyVector3ISet();
            this.m_triangleRegistry = triangleRegistry;
            this.m_components = new MyNavmeshComponents();
        }

        private Vector3I CellToLowestCube(Vector3I cell) => 
            ((cell - CELL_COORD_SHIFT) * this.m_cellSize);

        private Vector3I CubeToCell(ref Vector3I cube)
        {
            Vector3I vectori;
            Vector3I.Floor(ref (ref Vector3D) ref (cube / this.m_cellSize), out vectori);
            return (Vector3I) (vectori + CELL_COORD_SHIFT);
        }

        public MyHighLevelPrimitive GetHighLevelNavigationPrimitive(MyNavigationTriangle triangle) => 
            ((triangle != null) ? (ReferenceEquals(triangle.Parent, this.m_mesh) ? ((triangle.ComponentIndex == -1) ? null : this.m_mesh.HighLevelGroup.GetPrimitive(triangle.ComponentIndex)) : null) : null);

        public void MarkBlockChanged(MySlimBlock block)
        {
            Vector3I cube = block.Min - Vector3I.One;
            Vector3I vectori2 = (Vector3I) (block.Max + Vector3I.One);
            Vector3I position = cube;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref block.Min, ref block.Max);
            while (iterator.IsValid())
            {
                this.m_changedCubes.Add(position);
                iterator.GetNext(out position);
            }
            Vector3I start = this.CubeToCell(ref cube);
            Vector3I end = this.CubeToCell(ref vectori2);
            position = start;
            Vector3I_RangeIterator iterator2 = new Vector3I_RangeIterator(ref start, ref end);
            while (iterator2.IsValid())
            {
                this.m_changedCells.Add(position);
                iterator2.GetNext(out position);
            }
        }

        public void ProcessChangedCellComponents()
        {
            m_currentHelper = this;
            List<int> list = null;
            foreach (Vector3I vectori4 in this.m_changedCells)
            {
                Vector3I start = this.CellToLowestCube(vectori4);
                Vector3I end = ((Vector3I) (start + this.m_cellSize)) - Vector3I.One;
                Vector3I key = start;
                Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref end);
                while (true)
                {
                    if (!iterator.IsValid())
                    {
                        if (m_tmpCellTriangles.Count != 0)
                        {
                            ulong cellCoord = new MyCellCoord(0, vectori4).PackId64();
                            this.m_components.OpenCell(cellCoord);
                            long num2 = this.m_mesh.GetCurrentTimestamp() + 1L;
                            long currentTimestamp = num2;
                            this.m_currentComponentRel = 0;
                            this.m_tmpComponentTriangles.Clear();
                            foreach (int num6 in m_tmpCellTriangles)
                            {
                                MyNavigationTriangle vertex = this.m_mesh.GetTriangle(num6);
                                if ((this.m_currentComponentRel == 0) || !this.m_mesh.VisitedBetween(vertex, num2, currentTimestamp))
                                {
                                    this.m_components.OpenComponent();
                                    if (this.m_currentComponentRel >= this.m_currentCellConnections.Count)
                                    {
                                        this.m_currentCellConnections.Add(new List<int>());
                                    }
                                    this.m_components.AddComponentTriangle(vertex, vertex.Center);
                                    vertex.ComponentIndex = this.m_currentComponentRel;
                                    this.m_tmpComponentTriangles.Add(vertex);
                                    this.m_mesh.PrepareTraversal(vertex, null, this.m_processTrianglePredicate, null);
                                    this.m_mesh.PerformTraversal();
                                    this.m_tmpComponentTriangles.Add(null);
                                    this.m_components.CloseComponent();
                                    currentTimestamp = this.m_mesh.GetCurrentTimestamp();
                                    if (this.m_currentComponentRel == 0)
                                    {
                                        num2 = currentTimestamp;
                                    }
                                    this.m_currentComponentRel++;
                                }
                            }
                            m_tmpCellTriangles.Clear();
                            MyNavmeshComponents.ClosedCellInfo output = new MyNavmeshComponents.ClosedCellInfo();
                            this.m_components.CloseAndCacheCell(ref output);
                            int startingIndex = output.StartingIndex;
                            foreach (MyNavigationTriangle triangle2 in this.m_tmpComponentTriangles)
                            {
                                if (triangle2 == null)
                                {
                                    startingIndex++;
                                    continue;
                                }
                                triangle2.ComponentIndex = startingIndex;
                            }
                            this.m_tmpComponentTriangles.Clear();
                            if (!output.NewCell && (output.ComponentNum != output.OldComponentNum))
                            {
                                for (int i = 0; i < output.OldComponentNum; i++)
                                {
                                    this.m_mesh.HighLevelGroup.RemovePrimitive(output.OldStartingIndex + i);
                                }
                            }
                            if (output.NewCell || (output.ComponentNum != output.OldComponentNum))
                            {
                                for (int i = 0; i < output.ComponentNum; i++)
                                {
                                    this.m_mesh.HighLevelGroup.AddPrimitive(output.StartingIndex + i, this.m_components.GetComponentCenter(i));
                                }
                            }
                            if (!output.NewCell && (output.ComponentNum == output.OldComponentNum))
                            {
                                for (int i = 0; i < output.ComponentNum; i++)
                                {
                                    this.m_mesh.HighLevelGroup.GetPrimitive(output.StartingIndex + i).UpdatePosition(this.m_components.GetComponentCenter(i));
                                }
                            }
                            int num10 = 0;
                            while (true)
                            {
                                if (num10 >= output.ComponentNum)
                                {
                                    for (int i = 0; i < output.ComponentNum; i++)
                                    {
                                        startingIndex = output.StartingIndex + i;
                                        MyHighLevelPrimitive primitive = this.m_mesh.HighLevelGroup.GetPrimitive(startingIndex);
                                        if (primitive != null)
                                        {
                                            primitive.IsExpanded = true;
                                        }
                                    }
                                    break;
                                }
                                int index = output.StartingIndex + num10;
                                this.m_mesh.HighLevelGroup.GetPrimitive(index).GetNeighbours(this.m_tmpNeighbors);
                                foreach (int num12 in this.m_currentCellConnections[num10])
                                {
                                    if (!this.m_tmpNeighbors.Remove(num12))
                                    {
                                        this.m_mesh.HighLevelGroup.ConnectPrimitives(index, num12);
                                    }
                                }
                                foreach (int num13 in this.m_tmpNeighbors)
                                {
                                    MyHighLevelPrimitive primitive = this.m_mesh.HighLevelGroup.TryGetPrimitive(num13);
                                    if ((primitive != null) && primitive.IsExpanded)
                                    {
                                        this.m_mesh.HighLevelGroup.DisconnectPrimitives(index, num13);
                                    }
                                }
                                this.m_tmpNeighbors.Clear();
                                this.m_currentCellConnections[num10].Clear();
                                num10++;
                            }
                        }
                        break;
                    }
                    if (this.m_triangleRegistry.TryGetValue(key, out list))
                    {
                        foreach (int num5 in list)
                        {
                            m_tmpCellTriangles.Add(num5);
                        }
                    }
                    iterator.GetNext(out key);
                }
            }
            this.m_changedCells.Clear();
            m_currentHelper = null;
        }

        private bool ProcessTriangleForHierarchy(MyNavigationTriangle triangle)
        {
            if (ReferenceEquals(triangle.Parent, this.m_mesh))
            {
                ulong num;
                if (m_tmpCellTriangles.Contains(triangle.Index))
                {
                    this.m_components.AddComponentTriangle(triangle, triangle.Center);
                    this.m_tmpComponentTriangles.Add(triangle);
                    return true;
                }
                if (this.m_components.TryGetComponentCell(triangle.ComponentIndex, out num) && !this.m_currentCellConnections[this.m_currentComponentRel].Contains(triangle.ComponentIndex))
                {
                    this.m_currentCellConnections[this.m_currentComponentRel].Add(triangle.ComponentIndex);
                }
            }
            return false;
        }

        private static bool ProcessTriangleForHierarchyStatic(MyNavigationPrimitive primitive)
        {
            MyNavigationTriangle triangle = primitive as MyNavigationTriangle;
            return m_currentHelper.ProcessTriangleForHierarchy(triangle);
        }

        private void TryClearCell(ulong packedCoord)
        {
            MyNavmeshComponents.CellInfo info;
            if (this.m_components.TryGetCell(packedCoord, out info))
            {
                this.m_components.ClearCell(packedCoord, ref info);
            }
        }

        public bool IsDirty =>
            !this.m_changedCells.Empty;
    }
}

