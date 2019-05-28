namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRageMath;
    using VRageRender.Utils;

    public class MyGridPathfinding
    {
        private Dictionary<MyCubeGrid, MyGridNavigationMesh> m_navigationMeshes = new Dictionary<MyCubeGrid, MyGridNavigationMesh>();
        private MyNavmeshCoordinator m_coordinator;
        private bool m_highLevelNavigationDirty;

        public MyGridPathfinding(MyNavmeshCoordinator coordinator)
        {
            this.m_coordinator = coordinator;
            this.m_coordinator.SetGridPathfinding(this);
            this.m_highLevelNavigationDirty = false;
        }

        [Conditional("DEBUG")]
        public void DebugDraw()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES != MyWEMDebugDrawMode.NONE))
            {
                foreach (KeyValuePair<MyCubeGrid, MyGridNavigationMesh> pair in this.m_navigationMeshes)
                {
                    Matrix worldMatrix = (Matrix) pair.Key.WorldMatrix;
                    Matrix.Rescale(ref worldMatrix, (float) 2.5f);
                }
            }
        }

        public MyNavigationPrimitive FindClosestPrimitive(Vector3D point, bool highLevel, ref double closestDistSq, MyCubeGrid grid = null)
        {
            if (highLevel)
            {
                return null;
            }
            MyNavigationPrimitive primitive = null;
            if (grid != null)
            {
                MyGridNavigationMesh mesh = null;
                if (this.m_navigationMeshes.TryGetValue(grid, out mesh))
                {
                    primitive = mesh.FindClosestPrimitive(point, highLevel, ref closestDistSq);
                }
            }
            else
            {
                foreach (KeyValuePair<MyCubeGrid, MyGridNavigationMesh> pair in this.m_navigationMeshes)
                {
                    MyNavigationPrimitive primitive2 = pair.Value.FindClosestPrimitive(point, highLevel, ref closestDistSq);
                    if (primitive2 != null)
                    {
                        primitive = primitive2;
                    }
                }
            }
            return primitive;
        }

        public List<Vector4D> FindPathGlobal(MyCubeGrid startGrid, MyCubeGrid endGrid, ref Vector3D start, ref Vector3D end)
        {
            if (!ReferenceEquals(startGrid, endGrid))
            {
                return null;
            }
            Vector3D vectord = Vector3D.Transform(start, startGrid.PositionComp.WorldMatrixInvScaled);
            Vector3D vectord2 = Vector3D.Transform(end, endGrid.PositionComp.WorldMatrixInvScaled);
            MyGridNavigationMesh mesh = null;
            return (!this.m_navigationMeshes.TryGetValue(startGrid, out mesh) ? null : mesh.FindPath((Vector3) vectord, (Vector3) vectord2));
        }

        public void GetCubeTriangles(CubeId cubeId, List<MyNavigationTriangle> trianglesOut)
        {
            MyGridNavigationMesh mesh = null;
            if (mesh != null)
            {
                mesh.GetCubeTriangles(cubeId.Coords, trianglesOut);
            }
        }

        public MyGridNavigationMesh GetNavmesh(MyCubeGrid grid)
        {
            MyGridNavigationMesh mesh = null;
            this.m_navigationMeshes.TryGetValue(grid, out mesh);
            return mesh;
        }

        private void grid_OnClose(MyEntity entity)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            if ((grid != null) && GridCanHaveNavmesh(grid))
            {
                this.m_coordinator.RemoveGridNavmeshLinks(grid);
                this.m_navigationMeshes.Remove(grid);
            }
        }

        public void GridAdded(MyCubeGrid grid)
        {
            if (GridCanHaveNavmesh(grid))
            {
                this.m_navigationMeshes.Add(grid, new MyGridNavigationMesh(grid, this.m_coordinator, 0x20, MyCestmirPathfindingShorts.Pathfinding.NextTimestampFunction));
                this.RegisterGridEvents(grid);
            }
        }

        public static bool GridCanHaveNavmesh(MyCubeGrid grid) => 
            ((MyPerGameSettings.Game == GameEnum.ME_GAME) && (grid.GridSizeEnum == MyCubeSize.Large));

        public void MarkHighLevelDirty()
        {
            this.m_highLevelNavigationDirty = true;
        }

        private void RegisterGridEvents(MyCubeGrid grid)
        {
            grid.OnClose += new Action<MyEntity>(this.grid_OnClose);
        }

        [Conditional("DEBUG")]
        public void RemoveTriangle(int index)
        {
            if (this.m_navigationMeshes.Count != 0)
            {
                using (Dictionary<MyCubeGrid, MyGridNavigationMesh>.ValueCollection.Enumerator enumerator = this.m_navigationMeshes.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.RemoveFace(index);
                    }
                }
            }
        }

        public void Update()
        {
            if (this.m_highLevelNavigationDirty)
            {
                foreach (KeyValuePair<MyCubeGrid, MyGridNavigationMesh> pair in this.m_navigationMeshes)
                {
                    MyGridNavigationMesh mesh = pair.Value;
                    if (mesh.HighLevelDirty)
                    {
                        mesh.UpdateHighLevel();
                    }
                }
                this.m_highLevelNavigationDirty = false;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CubeId
        {
            public MyCubeGrid Grid;
            public Vector3I Coords;
            public override bool Equals(object obj)
            {
                if (!(obj is MyGridPathfinding.CubeId))
                {
                    return false;
                }
                MyGridPathfinding.CubeId id = (MyGridPathfinding.CubeId) obj;
                return (ReferenceEquals(id.Grid, this.Grid) && (id.Coords == this.Coords));
            }

            public override int GetHashCode() => 
                ((this.Grid.GetHashCode() * 0x60000005) + this.Coords.GetHashCode());
        }
    }
}

