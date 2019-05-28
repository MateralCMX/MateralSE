namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Algorithms;
    using VRage.Game.Entity;
    using VRageMath;
    using VRageRender.Utils;

    public class MyPathfinding : MyPathFindingSystem<MyNavigationPrimitive>, IMyPathfinding
    {
        private MyVoxelPathfinding m_voxelPathfinding;
        private MyGridPathfinding m_gridPathfinding;
        private MyNavmeshCoordinator m_navmeshCoordinator;
        private MyDynamicObstacles m_obstacles;
        public readonly Func<long> NextTimestampFunction;
        private MyNavigationPrimitive m_reachEndPrimitive;
        private float m_reachPredicateDistance;

        public MyPathfinding() : base(0x80, null)
        {
            this.NextTimestampFunction = new Func<long>(this.GenerateNextTimestamp);
            this.m_obstacles = new MyDynamicObstacles();
            this.m_navmeshCoordinator = new MyNavmeshCoordinator(this.m_obstacles);
            this.m_gridPathfinding = new MyGridPathfinding(this.m_navmeshCoordinator);
            this.m_voxelPathfinding = new MyVoxelPathfinding(this.m_navmeshCoordinator);
            MyEntities.OnEntityAdd += new Action<MyEntity>(this.MyEntities_OnEntityAdd);
        }

        public void DebugDraw()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES != MyWEMDebugDrawMode.NONE)
                {
                    this.m_navmeshCoordinator.Links.DebugDraw(Color.Khaki);
                }
                if (MyFakes.DEBUG_DRAW_NAVMESH_HIERARCHY)
                {
                    this.m_navmeshCoordinator.HighLevelLinks.DebugDraw(Color.LightGreen);
                }
                this.m_navmeshCoordinator.DebugDraw();
                this.m_obstacles.DebugDraw();
            }
        }

        public MyNavigationPrimitive FindClosestPrimitive(Vector3D point, bool highLevel, MyEntity entity = null)
        {
            double positiveInfinity = double.PositiveInfinity;
            MyNavigationPrimitive primitive = null;
            MyNavigationPrimitive primitive2 = null;
            MyVoxelMap voxelMap = entity as MyVoxelMap;
            MyCubeGrid grid = entity as MyCubeGrid;
            if (voxelMap != null)
            {
                primitive = this.VoxelPathfinding.FindClosestPrimitive(point, highLevel, ref positiveInfinity, voxelMap);
            }
            else if (grid != null)
            {
                primitive = this.GridPathfinding.FindClosestPrimitive(point, highLevel, ref positiveInfinity, grid);
            }
            else
            {
                primitive2 = this.VoxelPathfinding.FindClosestPrimitive(point, highLevel, ref positiveInfinity, null);
                if (primitive2 != null)
                {
                    primitive = primitive2;
                }
                primitive2 = this.GridPathfinding.FindClosestPrimitive(point, highLevel, ref positiveInfinity, null);
                if (primitive2 != null)
                {
                    primitive = primitive2;
                }
            }
            return primitive;
        }

        public IMyPath FindPathGlobal(Vector3D begin, IMyDestinationShape end, MyEntity entity = null)
        {
            if (!MyPerGameSettings.EnablePathfinding)
            {
                return null;
            }
            MySmartGoal goal = new MySmartGoal(end, entity);
            MySmartPath path1 = new MySmartPath(this);
            path1.Init(begin, goal);
            return path1;
        }

        public MyPath<MyNavigationPrimitive> FindPathLowlevel(Vector3D begin, Vector3D end)
        {
            MyPath<MyNavigationPrimitive> path = null;
            if (MyPerGameSettings.EnablePathfinding)
            {
                MyNavigationPrimitive start = this.FindClosestPrimitive(begin, false, null);
                MyNavigationPrimitive primitive2 = this.FindClosestPrimitive(end, false, null);
                if ((start != null) && (primitive2 != null))
                {
                    path = base.FindPath(start, primitive2, null, null);
                }
            }
            return path;
        }

        private long GenerateNextTimestamp()
        {
            base.CalculateNextTimestamp();
            return base.GetCurrentTimestamp();
        }

        public IMyPathfindingLog GetPathfindingLog() => 
            this.m_voxelPathfinding.DebugLog;

        private void MyEntities_OnEntityAdd(MyEntity newEntity)
        {
            this.m_obstacles.TryCreateObstacle(newEntity);
            MyCubeGrid grid = newEntity as MyCubeGrid;
            if (grid != null)
            {
                this.m_gridPathfinding.GridAdded(grid);
            }
        }

        private bool ReachablePredicate(MyNavigationPrimitive primitive) => 
            ((this.m_reachEndPrimitive.WorldPosition - primitive.WorldPosition).LengthSquared() <= (this.m_reachPredicateDistance * this.m_reachPredicateDistance));

        public bool ReachableUnderThreshold(Vector3D begin, IMyDestinationShape end, float thresholdDistance)
        {
            this.m_reachPredicateDistance = thresholdDistance;
            MyNavigationPrimitive startingVertex = this.FindClosestPrimitive(begin, false, null);
            MyNavigationPrimitive primitive2 = this.FindClosestPrimitive(end.GetDestination(), false, null);
            if ((startingVertex != null) && (primitive2 != null))
            {
                MyHighLevelPrimitive highLevelPrimitive = startingVertex.GetHighLevelPrimitive();
                primitive2.GetHighLevelPrimitive();
                if (new MySmartGoal(end, null).FindHighLevelPath(this, highLevelPrimitive) == null)
                {
                    return false;
                }
                this.m_reachEndPrimitive = primitive2;
                base.PrepareTraversal(startingVertex, null, new Predicate<MyNavigationPrimitive>(this.ReachablePredicate), null);
                try
                {
                    using (MyPathFindingSystem<MyNavigationPrimitive>.Enumerator enumerator = base.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            if (enumerator.Current.Equals(this.m_reachEndPrimitive))
                            {
                                return true;
                            }
                        }
                    }
                }
                finally
                {
                }
            }
            return false;
        }

        public void UnloadData()
        {
            MyEntities.OnEntityAdd -= new Action<MyEntity>(this.MyEntities_OnEntityAdd);
            this.m_voxelPathfinding.UnloadData();
            this.m_gridPathfinding = null;
            this.m_voxelPathfinding = null;
            this.m_navmeshCoordinator = null;
            this.m_obstacles.Clear();
            this.m_obstacles = null;
        }

        public void Update()
        {
            if (MyPerGameSettings.EnablePathfinding)
            {
                this.m_obstacles.Update();
                this.m_gridPathfinding.Update();
                this.m_voxelPathfinding.Update();
            }
        }

        public MyGridPathfinding GridPathfinding =>
            this.m_gridPathfinding;

        public MyVoxelPathfinding VoxelPathfinding =>
            this.m_voxelPathfinding;

        public MyNavmeshCoordinator Coordinator =>
            this.m_navmeshCoordinator;

        public MyDynamicObstacles Obstacles =>
            this.m_obstacles;

        public long LastHighLevelTimestamp { get; set; }
    }
}

