namespace Sandbox.Game.AI.Pathfinding
{
    using RecastDetour;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    public class MyRDPathfinding : IMyPathfinding
    {
        private const int DEBUG_PATH_MAX_TICKS = 150;
        private const int TILE_SIZE = 0x10;
        private const int TILE_HEIGHT = 70;
        private const int TILE_LINE_COUNT = 0x19;
        private readonly double MIN_NAVMESH_MANAGER_SQUARED_DISTANCE = Math.Pow(160.0, 2.0);
        private Dictionary<MyPlanet, List<MyNavmeshManager>> m_planetManagers = new Dictionary<MyPlanet, List<MyNavmeshManager>>();
        private HashSet<MyCubeGrid> m_grids = new HashSet<MyCubeGrid>();
        private bool m_drawNavmesh;
        private BoundingBoxD? m_debugInvalidateTileAABB;
        private List<RequestedPath> m_debugDrawPaths = new List<RequestedPath>();

        public MyRDPathfinding()
        {
            Sandbox.Game.Entities.MyEntities.OnEntityAdd += new Action<VRage.Game.Entity.MyEntity>(this.MyEntities_OnEntityAdd);
            Sandbox.Game.Entities.MyEntities.OnEntityRemove += new Action<VRage.Game.Entity.MyEntity>(this.MyEntities_OnEntityRemove);
        }

        public bool AddToTrackedGrids(MyCubeGrid cubeGrid)
        {
            if (!this.m_grids.Add(cubeGrid))
            {
                return false;
            }
            cubeGrid.OnBlockAdded += new Action<MySlimBlock>(this.Grid_OnBlockAdded);
            cubeGrid.OnBlockRemoved += new Action<MySlimBlock>(this.Grid_OnBlockRemoved);
            return true;
        }

        private void AreaChanged(MyPlanet planet, BoundingBoxD areaBox)
        {
            List<MyNavmeshManager> list;
            if (this.m_planetManagers.TryGetValue(planet, out list))
            {
                using (List<MyNavmeshManager>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.InvalidateArea(areaBox);
                    }
                }
            }
        }

        private MyNavmeshManager CreateManager(Vector3D center, Vector3D? forwardDirection = new Vector3D?())
        {
            if (forwardDirection == null)
            {
                Vector3D v = -Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(center));
                forwardDirection = new Vector3D?(Vector3D.CalculatePerpendicularVector(v));
            }
            int tileSize = 0x10;
            MyNavmeshManager item = new MyNavmeshManager(this, center, forwardDirection.Value, tileSize, 70, 0x19, this.GetRecastOptions(null)) {
                DrawNavmesh = this.m_drawNavmesh
            };
            this.m_planetManagers[item.Planet].Add(item);
            return item;
        }

        public void DebugDraw()
        {
            foreach (KeyValuePair<MyPlanet, List<MyNavmeshManager>> pair in this.m_planetManagers)
            {
                using (List<MyNavmeshManager>.Enumerator enumerator2 = pair.Value.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.DebugDraw();
                    }
                }
            }
            if (this.m_debugInvalidateTileAABB != null)
            {
                MyRenderProxy.DebugDrawAABB(this.m_debugInvalidateTileAABB.Value, Color.Yellow, 0f, 1f, true, false, false);
            }
            this.DebugDrawPaths();
        }

        private void DebugDrawPaths()
        {
            DateTime now = DateTime.Now;
            for (int i = 0; i < this.m_debugDrawPaths.Count; i++)
            {
                RequestedPath path = this.m_debugDrawPaths[i];
                path.LocalTicks++;
                if (path.LocalTicks <= 150)
                {
                    this.DebugDrawSinglePath(path.Path);
                }
                else
                {
                    this.m_debugDrawPaths.RemoveAt(i);
                    i--;
                }
            }
        }

        private void DebugDrawSinglePath(List<Vector3D> path)
        {
            for (int i = 1; i < path.Count; i++)
            {
                MyRenderProxy.DebugDrawSphere(path[i], 0.5f, Color.Yellow, 0f, false, false, true, false);
                MyRenderProxy.DebugDrawLine3D(path[i - 1], path[i], Color.Yellow, Color.Yellow, false, false);
            }
        }

        public IMyPath FindPathGlobal(Vector3D begin, IMyDestinationShape end, VRage.Game.Entity.MyEntity relativeEntity)
        {
            Vector3D vectord;
            float num;
            IMyEntity entity;
            MyRDPath path = new MyRDPath(this, begin, end);
            if (!path.GetNextTarget(begin, out vectord, out num, out entity))
            {
                path = null;
            }
            return path;
        }

        private List<Vector3D> GetBestPathFromManagers(MyPlanet planet, Vector3D initialPosition, Vector3D targetPosition)
        {
            bool flag;
            List<Vector3D> list;
            Vector3D? nullable;
            List<MyNavmeshManager> list2 = (from m in this.m_planetManagers[planet]
                where m.ContainsPosition(initialPosition)
                select m).ToList<MyNavmeshManager>();
            if (list2.Count <= 0)
            {
                nullable = null;
                this.CreateManager(initialPosition, nullable).TilesToGenerate(initialPosition, targetPosition);
                return new List<Vector3D>();
            }
            using (List<MyNavmeshManager>.Enumerator enumerator = list2.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyNavmeshManager current = enumerator.Current;
                    if (current.ContainsPosition(targetPosition) && (current.GetPathPoints(initialPosition, targetPosition, out list, out flag) || !flag))
                    {
                        return list;
                    }
                }
            }
            MyNavmeshManager manager = null;
            double maxValue = double.MaxValue;
            foreach (MyNavmeshManager manager3 in list2)
            {
                double num2 = (manager3.Center - initialPosition).LengthSquared();
                if (maxValue > num2)
                {
                    maxValue = num2;
                    manager = manager3;
                }
            }
            if (((!manager.GetPathPoints(initialPosition, targetPosition, out list, out flag) & flag) && (list.Count <= 2)) && (maxValue > this.MIN_NAVMESH_MANAGER_SQUARED_DISTANCE))
            {
                double num3 = (initialPosition - targetPosition).LengthSquared();
                if (((manager.Center - targetPosition).LengthSquared() - num3) > this.MIN_NAVMESH_MANAGER_SQUARED_DISTANCE)
                {
                    nullable = null;
                    this.CreateManager(initialPosition, nullable).TilesToGenerate(initialPosition, targetPosition);
                }
            }
            return list;
        }

        public List<Vector3D> GetPath(MyPlanet planet, Vector3D initialPosition, Vector3D targetPosition)
        {
            if (!this.m_planetManagers.ContainsKey(planet))
            {
                this.m_planetManagers[planet] = new List<MyNavmeshManager>();
                planet.RangeChanged += new MyVoxelBase.StorageChanged(this.VoxelChanged);
            }
            List<Vector3D> list = this.GetBestPathFromManagers(planet, initialPosition, targetPosition);
            if (list.Count > 0)
            {
                RequestedPath item = new RequestedPath();
                item.Path = list;
                item.LocalTicks = 0;
                this.m_debugDrawPaths.Add(item);
            }
            return list;
        }

        public IMyPathfindingLog GetPathfindingLog() => 
            null;

        private MyPlanet GetPlanet(Vector3D position)
        {
            int num = 500;
            BoundingBoxD box = new BoundingBoxD(position - (num * 0.5f), position + (num * 0.5f));
            return MyGamePruningStructure.GetClosestPlanet(ref box);
        }

        private MyRecastOptions GetRecastOptions(MyCharacter character)
        {
            MyRecastOptions options1 = new MyRecastOptions();
            options1.cellHeight = 0.2f;
            options1.agentHeight = 1.5f;
            options1.agentRadius = 0.5f;
            options1.agentMaxClimb = 0.6f;
            options1.agentMaxSlope = 60f;
            options1.regionMinSize = 1f;
            options1.regionMergeSize = 10f;
            options1.edgeMaxLen = 50f;
            options1.edgeMaxError = 3f;
            options1.vertsPerPoly = 6f;
            options1.detailSampleDist = 6f;
            options1.detailSampleMaxError = 1f;
            options1.partitionType = 1;
            return options1;
        }

        public static BoundingBoxD GetVoxelAreaAABB(MyVoxelBase storage, Vector3I minVoxelChanged, Vector3I maxVoxelChanged)
        {
            Vector3D vectord;
            Vector3D vectord2;
            MyVoxelCoordSystems.VoxelCoordToWorldPosition(storage.PositionLeftBottomCorner, ref minVoxelChanged, out vectord);
            MyVoxelCoordSystems.VoxelCoordToWorldPosition(storage.PositionLeftBottomCorner, ref maxVoxelChanged, out vectord2);
            return new BoundingBoxD(vectord, vectord2);
        }

        private void Grid_OnBlockAdded(MySlimBlock slimBlock)
        {
            List<MyNavmeshManager> list;
            MyPlanet key = this.GetPlanet(slimBlock.WorldPosition);
            if ((key != null) && this.m_planetManagers.TryGetValue(key, out list))
            {
                BoundingBoxD worldAABB = slimBlock.WorldAABB;
                using (List<MyNavmeshManager>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.InvalidateArea(worldAABB);
                    }
                }
            }
        }

        private void Grid_OnBlockRemoved(MySlimBlock slimBlock)
        {
            List<MyNavmeshManager> list;
            MyPlanet key = this.GetPlanet(slimBlock.WorldPosition);
            if ((key != null) && this.m_planetManagers.TryGetValue(key, out list))
            {
                BoundingBoxD worldAABB = slimBlock.WorldAABB;
                using (List<MyNavmeshManager>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.InvalidateArea(worldAABB);
                    }
                }
            }
        }

        public void InvalidateArea(BoundingBoxD areaBox)
        {
            MyPlanet planet = this.GetPlanet(areaBox.Center);
            this.AreaChanged(planet, areaBox);
        }

        private void MyEntities_OnEntityAdd(VRage.Game.Entity.MyEntity obj)
        {
            MyCubeGrid cubeGrid = obj as MyCubeGrid;
            if (cubeGrid != null)
            {
                List<MyNavmeshManager> list;
                MyPlanet key = this.GetPlanet(cubeGrid.PositionComp.WorldAABB.Center);
                if ((key != null) && this.m_planetManagers.TryGetValue(key, out list))
                {
                    bool flag = false;
                    foreach (MyNavmeshManager manager in list)
                    {
                        flag |= manager.InvalidateArea(cubeGrid.PositionComp.WorldAABB);
                    }
                    if (flag)
                    {
                        this.AddToTrackedGrids(cubeGrid);
                    }
                }
            }
        }

        private void MyEntities_OnEntityRemove(VRage.Game.Entity.MyEntity obj)
        {
            MyCubeGrid item = obj as MyCubeGrid;
            if ((item != null) && this.m_grids.Remove(item))
            {
                List<MyNavmeshManager> list;
                item.OnBlockAdded -= new Action<MySlimBlock>(this.Grid_OnBlockAdded);
                item.OnBlockRemoved -= new Action<MySlimBlock>(this.Grid_OnBlockRemoved);
                MyPlanet key = this.GetPlanet(item.PositionComp.WorldAABB.Center);
                if ((key != null) && this.m_planetManagers.TryGetValue(key, out list))
                {
                    using (List<MyNavmeshManager>.Enumerator enumerator = list.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.InvalidateArea(item.PositionComp.WorldAABB);
                        }
                    }
                }
            }
        }

        public bool ReachableUnderThreshold(Vector3D begin, IMyDestinationShape end, float thresholdDistance) => 
            true;

        public void SetDrawNavmesh(bool drawNavmesh)
        {
            this.m_drawNavmesh = drawNavmesh;
            foreach (KeyValuePair<MyPlanet, List<MyNavmeshManager>> pair in this.m_planetManagers)
            {
                using (List<MyNavmeshManager>.Enumerator enumerator2 = pair.Value.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.DrawNavmesh = this.m_drawNavmesh;
                    }
                }
            }
        }

        public void UnloadData()
        {
            Sandbox.Game.Entities.MyEntities.OnEntityAdd -= new Action<VRage.Game.Entity.MyEntity>(this.MyEntities_OnEntityAdd);
            foreach (MyCubeGrid local1 in this.m_grids)
            {
                local1.OnBlockAdded -= new Action<MySlimBlock>(this.Grid_OnBlockAdded);
                local1.OnBlockRemoved -= new Action<MySlimBlock>(this.Grid_OnBlockRemoved);
            }
            this.m_grids.Clear();
            foreach (KeyValuePair<MyPlanet, List<MyNavmeshManager>> pair in this.m_planetManagers)
            {
                using (List<MyNavmeshManager>.Enumerator enumerator3 = pair.Value.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        enumerator3.Current.UnloadData();
                    }
                }
            }
        }

        public void Update()
        {
            foreach (KeyValuePair<MyPlanet, List<MyNavmeshManager>> pair in this.m_planetManagers)
            {
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    if (!pair.Value[i].Update())
                    {
                        pair.Value.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private void VoxelChanged(MyVoxelBase storage, Vector3I minVoxelChanged, Vector3I maxVoxelChanged, MyStorageDataTypeFlags changedData)
        {
            MyPlanet planet = storage as MyPlanet;
            if (planet != null)
            {
                BoundingBoxD areaBox = GetVoxelAreaAABB(planet, minVoxelChanged, maxVoxelChanged);
                this.AreaChanged(planet, areaBox);
                this.m_debugInvalidateTileAABB = new BoundingBoxD?(areaBox);
            }
        }

        private class RequestedPath
        {
            public List<Vector3D> Path;
            public int LocalTicks;
        }
    }
}

