namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    public class MyVoxelPathfinding
    {
        private int m_updateCtr;
        private const int UPDATE_PERIOD = 5;
        private Dictionary<MyVoxelBase, MyVoxelNavigationMesh> m_navigationMeshes;
        private List<Vector3D> m_tmpUpdatePositions;
        private List<MyVoxelBase> m_tmpVoxelMaps;
        private List<MyVoxelNavigationMesh> m_tmpNavmeshes;
        private MyNavmeshCoordinator m_coordinator;
        public MyVoxelPathfindingLog DebugLog;
        private static float MESH_DIST = 40f;

        public MyVoxelPathfinding(MyNavmeshCoordinator coordinator)
        {
            MyEntities.OnEntityAdd += new Action<MyEntity>(this.MyEntities_OnEntityAdd);
            this.m_navigationMeshes = new Dictionary<MyVoxelBase, MyVoxelNavigationMesh>();
            this.m_tmpUpdatePositions = new List<Vector3D>(8);
            this.m_tmpVoxelMaps = new List<MyVoxelBase>();
            this.m_tmpNavmeshes = new List<MyVoxelNavigationMesh>();
            this.m_coordinator = coordinator;
            coordinator.SetVoxelPathfinding(this);
            if (MyFakes.REPLAY_NAVMESH_GENERATION || MyFakes.LOG_NAVMESH_GENERATION)
            {
                this.DebugLog = new MyVoxelPathfindingLog("PathfindingLog.log");
            }
        }

        [Conditional("DEBUG")]
        public void DebugDraw()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                if (this.DebugLog != null)
                {
                    this.DebugLog.DebugDraw();
                }
                foreach (KeyValuePair<MyVoxelBase, MyVoxelNavigationMesh> pair in this.m_navigationMeshes)
                {
                    pair.Key.WorldMatrix;
                }
            }
        }

        public MyNavigationPrimitive FindClosestPrimitive(Vector3D point, bool highLevel, ref double closestDistanceSq, MyVoxelBase voxelMap = null)
        {
            MyNavigationPrimitive primitive = null;
            if (voxelMap != null)
            {
                MyVoxelNavigationMesh mesh = null;
                if (this.m_navigationMeshes.TryGetValue(voxelMap, out mesh))
                {
                    primitive = mesh.FindClosestPrimitive(point, highLevel, ref closestDistanceSq);
                }
            }
            else
            {
                foreach (KeyValuePair<MyVoxelBase, MyVoxelNavigationMesh> pair in this.m_navigationMeshes)
                {
                    MyNavigationPrimitive primitive2 = pair.Value.FindClosestPrimitive(point, highLevel, ref closestDistanceSq);
                    if (primitive2 != null)
                    {
                        primitive = primitive2;
                    }
                }
            }
            return primitive;
        }

        private void GetUpdatePositions()
        {
            this.m_tmpUpdatePositions.Clear();
            using (IEnumerator<MyPlayer> enumerator = Sync.Players.GetOnlinePlayers().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IMyControllableEntity controlledEntity = enumerator.Current.Controller.ControlledEntity;
                    if (controlledEntity != null)
                    {
                        this.m_tmpUpdatePositions.Add(controlledEntity.Entity.PositionComp.GetPosition());
                    }
                }
            }
        }

        public MyVoxelNavigationMesh GetVoxelMapNavmesh(MyVoxelBase map)
        {
            MyVoxelNavigationMesh mesh = null;
            this.m_navigationMeshes.TryGetValue(map, out mesh);
            return mesh;
        }

        public void InvalidateBox(ref BoundingBoxD bbox)
        {
            foreach (KeyValuePair<MyVoxelBase, MyVoxelNavigationMesh> pair in this.m_navigationMeshes)
            {
                Vector3I vectori;
                Vector3I vectori2;
                if (pair.Key.GetContainedVoxelCoords(ref bbox, out vectori, out vectori2))
                {
                    pair.Value.InvalidateRange(vectori, vectori2);
                }
            }
        }

        private void MarkCellsOnPaths()
        {
            foreach (KeyValuePair<MyVoxelBase, MyVoxelNavigationMesh> pair in this.m_navigationMeshes)
            {
                pair.Value.MarkCellsOnPaths();
            }
        }

        private void MyEntities_OnEntityAdd(MyEntity entity)
        {
            MyVoxelBase key = entity as MyVoxelBase;
            if ((key != null) && ((MyPerGameSettings.Game != GameEnum.SE_GAME) || (key is MyPlanet)))
            {
                this.m_navigationMeshes.Add(key, new MyVoxelNavigationMesh(key, this.m_coordinator, MyCestmirPathfindingShorts.Pathfinding.NextTimestampFunction));
                this.RegisterVoxelMapEvents(key);
            }
        }

        private void PerformCellAdditions(List<Vector3D> updatePositions)
        {
            this.MarkCellsOnPaths();
            this.ShuffleMeshes();
            using (List<MyVoxelNavigationMesh>.Enumerator enumerator = this.m_tmpNavmeshes.GetEnumerator())
            {
                while (enumerator.MoveNext() && !enumerator.Current.AddOneMarkedCell(updatePositions))
                {
                }
            }
            this.m_tmpNavmeshes.Clear();
        }

        private void PerformCellMarking(List<Vector3D> updatePositions)
        {
            Vector3D vectord = new Vector3D(1.0);
            foreach (Vector3D vectord2 in updatePositions)
            {
                BoundingBoxD box = new BoundingBoxD(vectord2 - vectord, vectord2 + vectord);
                this.m_tmpVoxelMaps.Clear();
                MyGamePruningStructure.GetAllVoxelMapsInBox(ref box, this.m_tmpVoxelMaps);
                foreach (MyVoxelBase base2 in this.m_tmpVoxelMaps)
                {
                    MyVoxelNavigationMesh mesh = null;
                    this.m_navigationMeshes.TryGetValue(base2, out mesh);
                    if (mesh != null)
                    {
                        mesh.MarkBoxForAddition(box);
                    }
                }
            }
            this.m_tmpVoxelMaps.Clear();
        }

        private void PerformCellRemovals(List<Vector3D> updatePositions)
        {
            this.ShuffleMeshes();
            using (List<MyVoxelNavigationMesh>.Enumerator enumerator = this.m_tmpNavmeshes.GetEnumerator())
            {
                while (enumerator.MoveNext() && !enumerator.Current.RemoveOneUnusedCell(updatePositions))
                {
                }
            }
            this.m_tmpNavmeshes.Clear();
        }

        private void PerformCellUpdates()
        {
            this.ShuffleMeshes();
            using (List<MyVoxelNavigationMesh>.Enumerator enumerator = this.m_tmpNavmeshes.GetEnumerator())
            {
                while (enumerator.MoveNext() && !enumerator.Current.RefreshOneChangedCell())
                {
                }
            }
            this.m_tmpNavmeshes.Clear();
        }

        private void RegisterVoxelMapEvents(MyVoxelBase voxelMap)
        {
            voxelMap.OnClose += new Action<MyEntity>(this.voxelMap_OnClose);
        }

        private void RemoveFarHighLevelGroups(List<Vector3D> updatePositions)
        {
            foreach (KeyValuePair<MyVoxelBase, MyVoxelNavigationMesh> pair in this.m_navigationMeshes)
            {
                pair.Value.RemoveFarHighLevelGroups(updatePositions);
            }
        }

        [Conditional("DEBUG")]
        public void RemoveTriangle(int index)
        {
            if (this.m_navigationMeshes.Count != 0)
            {
                using (Dictionary<MyVoxelBase, MyVoxelNavigationMesh>.ValueCollection.Enumerator enumerator = this.m_navigationMeshes.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.RemoveTriangle(index);
                    }
                }
            }
        }

        private void ShuffleMeshes()
        {
            this.m_tmpNavmeshes.Clear();
            foreach (KeyValuePair<MyVoxelBase, MyVoxelNavigationMesh> pair in this.m_navigationMeshes)
            {
                this.m_tmpNavmeshes.Add(pair.Value);
            }
            int? count = null;
            this.m_tmpNavmeshes.ShuffleList<MyVoxelNavigationMesh>(0, count);
        }

        public void UnloadData()
        {
            if (this.DebugLog != null)
            {
                this.DebugLog.Close();
                this.DebugLog = null;
            }
            MyEntities.OnEntityAdd -= new Action<MyEntity>(this.MyEntities_OnEntityAdd);
        }

        public void Update()
        {
            this.m_updateCtr++;
            int num = this.m_updateCtr % 6;
            if ((((num == 0) || (num == 2)) || (num == 4)) && MyFakes.DEBUG_ONE_VOXEL_PATHFINDING_STEP_SETTING)
            {
                if (!MyFakes.DEBUG_ONE_VOXEL_PATHFINDING_STEP)
                {
                    return;
                }
                MyFakes.DEBUG_ONE_VOXEL_PATHFINDING_STEP = false;
            }
            if (MyFakes.REPLAY_NAVMESH_GENERATION)
            {
                this.DebugLog.PerformOneOperation(MyFakes.REPLAY_NAVMESH_GENERATION_TRIGGER);
                MyFakes.REPLAY_NAVMESH_GENERATION_TRIGGER = false;
            }
            else
            {
                switch (num)
                {
                    case 0:
                        this.GetUpdatePositions();
                        this.PerformCellMarking(this.m_tmpUpdatePositions);
                        this.PerformCellUpdates();
                        this.m_tmpUpdatePositions.Clear();
                        return;

                    case 1:
                    case 3:
                        break;

                    case 2:
                        this.GetUpdatePositions();
                        this.PerformCellMarking(this.m_tmpUpdatePositions);
                        this.PerformCellAdditions(this.m_tmpUpdatePositions);
                        this.m_tmpUpdatePositions.Clear();
                        return;

                    case 4:
                        this.GetUpdatePositions();
                        this.PerformCellRemovals(this.m_tmpUpdatePositions);
                        this.RemoveFarHighLevelGroups(this.m_tmpUpdatePositions);
                        this.m_tmpUpdatePositions.Clear();
                        break;

                    default:
                        return;
                }
            }
        }

        private void voxelMap_OnClose(MyEntity entity)
        {
            MyVoxelBase key = entity as MyVoxelBase;
            if ((key != null) && ((MyPerGameSettings.Game != GameEnum.SE_GAME) || (key is MyPlanet)))
            {
                this.m_navigationMeshes.Remove(key);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CellId : IEquatable<MyVoxelPathfinding.CellId>
        {
            public MyVoxelBase VoxelMap;
            public Vector3I Pos;
            public override bool Equals(object obj) => 
                ((obj != null) ? (!(obj.GetType() != typeof(MyVoxelPathfinding.CellId)) ? this.Equals((MyVoxelPathfinding.CellId) obj) : false) : false);

            public override int GetHashCode() => 
                ((this.VoxelMap.GetHashCode() * 0x60000005) + this.Pos.GetHashCode());

            public bool Equals(MyVoxelPathfinding.CellId other) => 
                (ReferenceEquals(this.VoxelMap, other.VoxelMap) && (this.Pos == other.Pos));
        }
    }
}

