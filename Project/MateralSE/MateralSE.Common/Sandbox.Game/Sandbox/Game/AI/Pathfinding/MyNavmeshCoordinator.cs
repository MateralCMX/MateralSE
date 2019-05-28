namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    public class MyNavmeshCoordinator
    {
        private static List<MyEntity> m_tmpEntityList = new List<MyEntity>();
        private static List<MyGridPathfinding.CubeId> m_tmpLinkCandidates = new List<MyGridPathfinding.CubeId>();
        private static List<MyNavigationTriangle> m_tmpNavTris = new List<MyNavigationTriangle>();
        private static List<MyNavigationPrimitive> m_tmpNavPrims = new List<MyNavigationPrimitive>(4);
        private MyGridPathfinding m_gridPathfinding;
        private MyVoxelPathfinding m_voxelPathfinding;
        private MyDynamicObstacles m_obstacles;
        private Dictionary<MyVoxelPathfinding.CellId, List<MyNavigationPrimitive>> m_voxelLinkDictionary = new Dictionary<MyVoxelPathfinding.CellId, List<MyNavigationPrimitive>>();
        private Dictionary<MyGridPathfinding.CubeId, int> m_gridLinkCounter = new Dictionary<MyGridPathfinding.CubeId, int>();
        private MyNavgroupLinks m_links = new MyNavgroupLinks();
        private MyNavgroupLinks m_highLevelLinks = new MyNavgroupLinks();

        public MyNavmeshCoordinator(MyDynamicObstacles obstacles)
        {
            this.m_obstacles = obstacles;
        }

        private void CollectClosePrimitives(MyNavigationPrimitive addedPrimitive, List<MyNavigationPrimitive> output, int depth)
        {
            if (depth >= 0)
            {
                int count = output.Count;
                output.Add(addedPrimitive);
                int num2 = output.Count;
                for (int i = 0; i < addedPrimitive.GetOwnNeighborCount(); i++)
                {
                    MyNavigationPrimitive ownNeighbor = addedPrimitive.GetOwnNeighbor(i) as MyNavigationPrimitive;
                    if (ownNeighbor != null)
                    {
                        output.Add(ownNeighbor);
                    }
                }
                int num3 = output.Count;
                depth--;
                while (depth > 0)
                {
                    int num5 = num2;
                    while (true)
                    {
                        if (num5 >= num3)
                        {
                            count = num2;
                            num2 = num3;
                            num3 = output.Count;
                            depth--;
                            break;
                        }
                        MyNavigationPrimitive primitive2 = output[num5];
                        int index = 0;
                        while (true)
                        {
                            if (index >= primitive2.GetOwnNeighborCount())
                            {
                                num5++;
                                break;
                            }
                            MyNavigationPrimitive ownNeighbor = primitive2.GetOwnNeighbor(index) as MyNavigationPrimitive;
                            bool flag = false;
                            int num7 = count;
                            while (true)
                            {
                                if (num7 < num3)
                                {
                                    if (output[num7] != ownNeighbor)
                                    {
                                        num7++;
                                        continue;
                                    }
                                    flag = true;
                                }
                                if (!flag && (ownNeighbor != null))
                                {
                                    output.Add(ownNeighbor);
                                }
                                index++;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void DebugDraw()
        {
            if (MyFakes.DEBUG_DRAW_NAVMESH_LINKS && MyFakes.DEBUG_DRAW_NAVMESH_HIERARCHY)
            {
                foreach (KeyValuePair<MyVoxelPathfinding.CellId, List<MyNavigationPrimitive>> pair in this.m_voxelLinkDictionary)
                {
                    Vector3I pos = pair.Key.Pos;
                    BoundingBoxD worldAABB = new BoundingBoxD();
                    MyVoxelCoordSystems.GeometryCellCoordToWorldAABB(pair.Key.VoxelMap.PositionLeftBottomCorner, ref pos, out worldAABB);
                    MyRenderProxy.DebugDrawText3D(worldAABB.Center, "LinkNum: " + pair.Value.Count, Color.Red, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                }
            }
        }

        private void DecreaseGridLinkCounter(MyGridPathfinding.CubeId candidate)
        {
            int num = 0;
            if (this.m_gridLinkCounter.TryGetValue(candidate, out num))
            {
                num--;
                if (num == 0)
                {
                    this.m_gridLinkCounter.Remove(candidate);
                }
                else
                {
                    this.m_gridLinkCounter[candidate] = num;
                }
            }
        }

        private void IncreaseGridLinkCounter(MyGridPathfinding.CubeId candidate)
        {
            int num = 0;
            num = this.m_gridLinkCounter.TryGetValue(candidate, out num) ? (num + 1) : 1;
            this.m_gridLinkCounter[candidate] = num;
        }

        public void InvalidateVoxelsBBox(ref BoundingBoxD bbox)
        {
            this.m_voxelPathfinding.InvalidateBox(ref bbox);
        }

        public unsafe void PrepareVoxelTriangleTests(BoundingBoxD cellBoundingBox, List<MyCubeGrid> gridsToTestOutput)
        {
            m_tmpEntityList.Clear();
            float cubeSize = MyDefinitionManager.Static.GetCubeSize(MyCubeSize.Large);
            cellBoundingBox.Inflate((double) cubeSize);
            if (MyPerGameSettings.NavmeshPresumesDownwardGravity)
            {
                Vector3D min = cellBoundingBox.Min;
                double* numPtr1 = (double*) ref min.Y;
                numPtr1[0] -= cubeSize;
                cellBoundingBox.Min = min;
            }
            MyGamePruningStructure.GetAllEntitiesInBox(ref cellBoundingBox, m_tmpEntityList, MyEntityQueryType.Both);
            foreach (MyCubeGrid grid in m_tmpEntityList)
            {
                if (grid == null)
                {
                    continue;
                }
                if (MyGridPathfinding.GridCanHaveNavmesh(grid))
                {
                    gridsToTestOutput.Add(grid);
                }
            }
            m_tmpEntityList.Clear();
        }

        public void RemoveGridNavmeshLinks(MyCubeGrid grid)
        {
            MyGridNavigationMesh navmesh = this.m_gridPathfinding.GetNavmesh(grid);
            if (navmesh != null)
            {
                m_tmpNavPrims.Clear();
                MyVector3ISet.Enumerator cubes = navmesh.GetCubes();
                while (cubes.MoveNext())
                {
                    int num;
                    MyGridPathfinding.CubeId key = new MyGridPathfinding.CubeId {
                        Grid = grid,
                        Coords = cubes.Current
                    };
                    if (this.m_gridLinkCounter.TryGetValue(key, out num))
                    {
                        m_tmpNavTris.Clear();
                        navmesh.GetCubeTriangles(cubes.Current, m_tmpNavTris);
                        foreach (MyNavigationTriangle triangle in m_tmpNavTris)
                        {
                            this.m_links.RemoveAllLinks(triangle);
                            MyHighLevelPrimitive highLevelPrimitive = triangle.GetHighLevelPrimitive();
                            if (!m_tmpNavPrims.Contains(highLevelPrimitive))
                            {
                                m_tmpNavPrims.Add(highLevelPrimitive);
                            }
                        }
                        m_tmpNavTris.Clear();
                        this.m_gridLinkCounter.Remove(key);
                    }
                }
                cubes.Dispose();
                foreach (MyNavigationPrimitive primitive2 in m_tmpNavPrims)
                {
                    this.m_highLevelLinks.RemoveAllLinks(primitive2);
                }
                m_tmpNavPrims.Clear();
            }
        }

        private void RemoveVoxelLinkFromDictionary(MyVoxelPathfinding.CellId cellId, MyNavigationPrimitive linkedPrimitive)
        {
            List<MyNavigationPrimitive> list = null;
            if (this.m_voxelLinkDictionary.TryGetValue(cellId, out list))
            {
                list.Remove(linkedPrimitive);
                if (list.Count == 0)
                {
                    this.m_voxelLinkDictionary.Remove(cellId);
                }
            }
        }

        public void RemoveVoxelNavmeshLinks(MyVoxelPathfinding.CellId cellId)
        {
            List<MyNavigationPrimitive> list = null;
            if (this.m_voxelLinkDictionary.TryGetValue(cellId, out list))
            {
                foreach (MyNavigationPrimitive primitive in list)
                {
                    this.m_links.RemoveAllLinks(primitive);
                }
                this.m_voxelLinkDictionary.Remove(cellId);
            }
        }

        private void SaveVoxelLinkToDictionary(MyVoxelPathfinding.CellId cellId, MyNavigationPrimitive linkedPrimitive)
        {
            List<MyNavigationPrimitive> list = null;
            if (!this.m_voxelLinkDictionary.TryGetValue(cellId, out list))
            {
                list = new List<MyNavigationPrimitive>();
            }
            else if (list.Contains(linkedPrimitive))
            {
                return;
            }
            list.Add(linkedPrimitive);
            this.m_voxelLinkDictionary[cellId] = list;
        }

        public void SetGridPathfinding(MyGridPathfinding gridPathfinding)
        {
            this.m_gridPathfinding = gridPathfinding;
        }

        public void SetVoxelPathfinding(MyVoxelPathfinding myVoxelPathfinding)
        {
            this.m_voxelPathfinding = myVoxelPathfinding;
        }

        public void TestVoxelNavmeshTriangle(ref Vector3D a, ref Vector3D b, ref Vector3D c, List<MyCubeGrid> gridsToTest, List<MyGridPathfinding.CubeId> linkCandidatesOutput, out bool intersecting)
        {
            Vector3D point = ((a + b) + c) / 3.0;
            if (this.m_obstacles.IsInObstacle(point))
            {
                intersecting = true;
                return;
            }
            Vector3D zero = Vector3D.Zero;
            if (MyPerGameSettings.NavmeshPresumesDownwardGravity)
            {
                zero = Vector3.Down * 2f;
            }
            m_tmpLinkCandidates.Clear();
            intersecting = false;
            using (List<MyCubeGrid>.Enumerator enumerator = gridsToTest.GetEnumerator())
            {
                goto TR_0016;
            TR_0008:
                if (intersecting)
                {
                    goto TR_0006;
                }
            TR_0016:
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        Vector3D vectord2;
                        Vector3D vectord3;
                        Vector3D vectord4;
                        Vector3D vectord5;
                        MyCubeGrid current = enumerator.Current;
                        MatrixD worldMatrixNormalizedInv = current.PositionComp.WorldMatrixNormalizedInv;
                        Vector3D.Transform(ref a, ref worldMatrixNormalizedInv, out vectord2);
                        Vector3D.Transform(ref b, ref worldMatrixNormalizedInv, out vectord3);
                        Vector3D.Transform(ref c, ref worldMatrixNormalizedInv, out vectord4);
                        Vector3D.TransformNormal(ref zero, ref worldMatrixNormalizedInv, out vectord5);
                        BoundingBoxD xd = new BoundingBoxD(Vector3D.MaxValue, Vector3D.MinValue);
                        xd.Include(ref vectord2, ref vectord3, ref vectord4);
                        Vector3I vectori = current.LocalToGridInteger((Vector3) xd.Min);
                        Vector3I vectori2 = current.LocalToGridInteger((Vector3) xd.Max);
                        Vector3I start = vectori - Vector3I.One;
                        Vector3I end = (Vector3I) (vectori2 + Vector3I.One);
                        Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref end);
                        while (iterator.IsValid())
                        {
                            if (current.GetCubeBlock(start) != null)
                            {
                                Vector3 min = (Vector3) ((start - Vector3.One) * current.GridSize);
                                Vector3 max = (start + Vector3.One) * current.GridSize;
                                Vector3 vector3 = (Vector3) ((start - Vector3.Half) * current.GridSize);
                                Vector3 vector4 = (start + Vector3.Half) * current.GridSize;
                                BoundingBoxD xd3 = new BoundingBoxD(min, max);
                                BoundingBoxD xd4 = new BoundingBoxD(vector3, vector4);
                                xd3.Include(min + vectord5);
                                xd3.Include(max + vectord5);
                                xd4.Include(vector3 + vectord5);
                                xd4.Include(vector4 + vectord5);
                                if (xd3.IntersectsTriangle(ref vectord2, ref vectord3, ref vectord4))
                                {
                                    if (xd4.IntersectsTriangle(ref vectord2, ref vectord3, ref vectord4))
                                    {
                                        intersecting = true;
                                        break;
                                    }
                                    int num3 = Math.Min(Math.Abs((int) (vectori.Z - start.Z)), Math.Abs((int) (vectori2.Z - start.Z)));
                                    if (((Math.Min(Math.Abs((int) (vectori.X - start.X)), Math.Abs((int) (vectori2.X - start.X))) + Math.Min(Math.Abs((int) (vectori.Y - start.Y)), Math.Abs((int) (vectori2.Y - start.Y)))) + num3) < 3)
                                    {
                                        MyGridPathfinding.CubeId item = new MyGridPathfinding.CubeId {
                                            Grid = current,
                                            Coords = start
                                        };
                                        m_tmpLinkCandidates.Add(item);
                                    }
                                }
                            }
                            iterator.GetNext(out start);
                        }
                    }
                    else
                    {
                        goto TR_0006;
                    }
                    break;
                }
                goto TR_0008;
            }
        TR_0006:
            if (!intersecting)
            {
                for (int i = 0; i < m_tmpLinkCandidates.Count; i++)
                {
                    linkCandidatesOutput.Add(m_tmpLinkCandidates[i]);
                }
            }
            m_tmpLinkCandidates.Clear();
        }

        public void TryAddVoxelNavmeshLinks(MyNavigationTriangle addedPrimitive, MyVoxelPathfinding.CellId cellId, List<MyGridPathfinding.CubeId> linkCandidates)
        {
            m_tmpNavTris.Clear();
            using (List<MyGridPathfinding.CubeId>.Enumerator enumerator = linkCandidates.GetEnumerator())
            {
                MyGridPathfinding.CubeId current;
                MyNavigationTriangle triangle;
                bool flag;
                goto TR_0021;
            TR_0002:
                m_tmpNavTris.Clear();
                goto TR_0021;
            TR_0004:
                if (flag)
                {
                    this.m_links.AddLink(addedPrimitive, triangle, false);
                    this.SaveVoxelLinkToDictionary(cellId, addedPrimitive);
                    this.IncreaseGridLinkCounter(current);
                }
                goto TR_0002;
            TR_0005:
                m_tmpNavPrims.Clear();
                goto TR_0004;
            TR_0021:
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        current = enumerator.Current;
                        this.m_gridPathfinding.GetCubeTriangles(current, m_tmpNavTris);
                        double maxValue = double.MaxValue;
                        triangle = null;
                        foreach (MyNavigationTriangle triangle2 in m_tmpNavTris)
                        {
                            Vector3D vectord = addedPrimitive.WorldPosition - triangle2.WorldPosition;
                            if (MyPerGameSettings.NavmeshPresumesDownwardGravity && ((Math.Abs(vectord.Y) < 0.3) && (vectord.LengthSquared() < maxValue)))
                            {
                                maxValue = vectord.LengthSquared();
                                triangle = triangle2;
                            }
                        }
                        if (triangle == null)
                        {
                            goto TR_0002;
                        }
                        else
                        {
                            flag = true;
                            List<MyNavigationPrimitive> links = this.m_links.GetLinks(triangle);
                            List<MyNavigationPrimitive> list2 = null;
                            this.m_voxelLinkDictionary.TryGetValue(cellId, out list2);
                            if (links == null)
                            {
                                goto TR_0004;
                            }
                            else
                            {
                                m_tmpNavPrims.Clear();
                                this.CollectClosePrimitives(addedPrimitive, m_tmpNavPrims, 2);
                                for (int i = 0; i < m_tmpNavPrims.Count; i++)
                                {
                                    if ((links.Contains(m_tmpNavPrims[i]) && (list2 != null)) && list2.Contains(m_tmpNavPrims[i]))
                                    {
                                        if ((m_tmpNavPrims[i].WorldPosition - triangle.WorldPosition).LengthSquared() < maxValue)
                                        {
                                            flag = false;
                                            break;
                                        }
                                        this.m_links.RemoveLink(triangle, m_tmpNavPrims[i]);
                                        if (this.m_links.GetLinkCount(m_tmpNavPrims[i]) == 0)
                                        {
                                            this.RemoveVoxelLinkFromDictionary(cellId, m_tmpNavPrims[i]);
                                        }
                                        this.DecreaseGridLinkCounter(current);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                    break;
                }
                goto TR_0005;
            }
        }

        public void TryAddVoxelNavmeshLinks2(MyVoxelPathfinding.CellId cellId, Dictionary<MyGridPathfinding.CubeId, List<MyNavigationPrimitive>> linkCandidates)
        {
            foreach (KeyValuePair<MyGridPathfinding.CubeId, List<MyNavigationPrimitive>> pair in linkCandidates)
            {
                double maxValue = double.MaxValue;
                MyNavigationTriangle triangle = null;
                MyNavigationPrimitive primitive = null;
                m_tmpNavTris.Clear();
                this.m_gridPathfinding.GetCubeTriangles(pair.Key, m_tmpNavTris);
                foreach (MyNavigationTriangle triangle2 in m_tmpNavTris)
                {
                    Vector3 vector;
                    Vector3 vector2;
                    Vector3 vector3;
                    triangle2.GetVertices(out vector, out vector2, out vector3);
                    vector = (Vector3) triangle2.Parent.LocalToGlobal(vector);
                    vector2 = (Vector3) triangle2.Parent.LocalToGlobal(vector2);
                    vector3 = (Vector3) triangle2.Parent.LocalToGlobal(vector3);
                    Vector3D vectord = (vector3 - vector).Cross(vector2 - vector);
                    Vector3D vectord2 = ((vector + vector2) + vector3) / 3f;
                    double num2 = Math.Min(vector.Y, Math.Min(vector2.Y, vector3.Y)) - 0.25;
                    double num3 = Math.Max(vector.Y, Math.Max(vector2.Y, vector3.Y)) + 0.25;
                    foreach (MyNavigationPrimitive primitive2 in pair.Value)
                    {
                        double num5;
                        Vector3D worldPosition = primitive2.WorldPosition;
                        Vector3D vectord4 = worldPosition - vectord2;
                        double num4 = vectord4.Length();
                        Vector3D.Dot(ref vectord4 / num4, ref vectord, out num5);
                        if ((num5 > -0.20000000298023224) && ((worldPosition.Y < num3) && (worldPosition.Y > num2)))
                        {
                            double num6 = num4 / (num5 + 0.30000001192092896);
                            if (num6 < maxValue)
                            {
                                maxValue = num6;
                                triangle = triangle2;
                                primitive = primitive2;
                            }
                        }
                    }
                }
                m_tmpNavTris.Clear();
                if (triangle != null)
                {
                    this.m_links.AddLink(primitive, triangle, false);
                    this.SaveVoxelLinkToDictionary(cellId, primitive);
                    this.IncreaseGridLinkCounter(pair.Key);
                }
            }
        }

        public void UpdateVoxelNavmeshCellHighLevelLinks(MyVoxelPathfinding.CellId cellId)
        {
            List<MyNavigationPrimitive> list = null;
            if (this.m_voxelLinkDictionary.TryGetValue(cellId, out list))
            {
                MyNavigationPrimitive highLevelPrimitive = null;
                MyNavigationPrimitive highLevelPrimitive = null;
                foreach (MyNavigationPrimitive primitive3 in list)
                {
                    highLevelPrimitive = primitive3.GetHighLevelPrimitive();
                    List<MyNavigationPrimitive> links = null;
                    links = this.m_links.GetLinks(primitive3);
                    if (links != null)
                    {
                        using (List<MyNavigationPrimitive>.Enumerator enumerator2 = links.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                highLevelPrimitive = enumerator2.Current.GetHighLevelPrimitive();
                                this.m_highLevelLinks.AddLink(highLevelPrimitive, highLevelPrimitive, true);
                            }
                        }
                    }
                }
            }
        }

        public MyNavgroupLinks Links =>
            this.m_links;

        public MyNavgroupLinks HighLevelLinks =>
            this.m_highLevelLinks;
    }
}

