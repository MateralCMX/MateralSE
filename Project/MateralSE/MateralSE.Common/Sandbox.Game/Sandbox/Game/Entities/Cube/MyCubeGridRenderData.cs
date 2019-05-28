namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Library.Collections;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyCubeGridRenderData
    {
        public const int SplitCellCubeCount = 30;
        private const int MAX_DECALS_PER_CUBE = 30;
        private ConcurrentDictionary<Vector3I, ConcurrentQueue<MyDecalPartIdentity>> m_cubeDecals = new ConcurrentDictionary<Vector3I, ConcurrentQueue<MyDecalPartIdentity>>();
        private Vector3 m_basePos;
        private readonly ConcurrentDictionary<Vector3I, MyCubeGridRenderCell> m_cells = new ConcurrentDictionary<Vector3I, MyCubeGridRenderCell>();
        private readonly object m_cellLock = new object();
        private readonly object m_cellsUpdateLock = new object();
        private MyConcurrentHashSet<MyCubeGridRenderCell> m_dirtyCells = new MyConcurrentHashSet<MyCubeGridRenderCell>();
        private MyRenderComponentCubeGrid m_gridRender;
        [ThreadStatic]
        private static List<MyCubeGridRenderCell> m_dirtyCellsBuffer;

        public MyCubeGridRenderData(MyRenderComponentCubeGrid grid)
        {
            this.m_gridRender = grid;
        }

        public void AddCubePart(MyCubePart part)
        {
            Vector3 translation = part.InstanceData.Translation;
            MyCubeGridRenderCell orAddCell = this.GetOrAddCell(translation, true);
            orAddCell.AddCubePart(part);
            this.m_dirtyCells.Add(orAddCell);
        }

        public void AddDecal(Vector3I position, MyCubeGrid.MyCubeGridHitInfo gridHitInfo, uint decalId)
        {
            MyCube cube;
            if (this.m_gridRender.CubeGrid.TryGetCube(position, out cube))
            {
                if (gridHitInfo.CubePartIndex != -1)
                {
                    MyCubePart part = cube.Parts[gridHitInfo.CubePartIndex];
                    this.GetOrAddCell(part.InstanceData.Translation, true).AddCubePartDecal(part, decalId);
                }
                ConcurrentQueue<MyDecalPartIdentity> orAdd = this.m_cubeDecals.GetOrAdd(position, x => new ConcurrentQueue<MyDecalPartIdentity>());
                if (orAdd.Count > 30)
                {
                    this.RemoveDecal(position, orAdd, cube);
                }
                MyDecalPartIdentity item = new MyDecalPartIdentity {
                    DecalId = decalId,
                    CubePartIndex = gridHitInfo.CubePartIndex
                };
                orAdd.Enqueue(item);
            }
        }

        public void AddEdgeInfo(ref Vector3 point0, ref Vector3 point1, ref Vector3 normal0, ref Vector3 normal1, Color color, MySlimBlock owner)
        {
            Vector3 pos = (point0 + point1) * 0.5f;
            Vector3I edgeDirection = Vector3I.Round((point0 - point1) / this.m_gridRender.GridSize);
            MyCubeGridRenderCell orAddCell = this.GetOrAddCell(pos, true);
            if (orAddCell.AddEdgeInfo(this.CalculateEdgeHash(point0, point1), new MyEdgeInfo(ref pos, ref edgeDirection, ref normal0, ref normal1, ref color, MyStringHash.GetOrCompute(owner.BlockDefinition.EdgeType)), owner))
            {
                this.m_dirtyCells.Add(orAddCell);
            }
        }

        private long CalculateEdgeHash(Vector3 point0, Vector3 point1) => 
            (point0.GetHash() * point1.GetHash());

        internal void DebugDraw()
        {
            foreach (KeyValuePair<Vector3I, MyCubeGridRenderCell> pair in this.m_cells)
            {
                pair.Value.DebugDraw();
            }
        }

        internal MyCubeGridRenderCell GetOrAddCell(Vector3 pos, bool create = true) => 
            this.GetOrAddCell(ref pos, create);

        internal MyCubeGridRenderCell GetOrAddCell(ref Vector3 pos, bool create = true)
        {
            Vector3I vectori;
            MyCubeGridRenderCell cell;
            Vector3I.Round(ref (pos - this.m_basePos) / (30f * this.m_gridRender.GridSize), out vectori);
            if (!this.m_cells.TryGetValue(vectori, out cell) && create)
            {
                object cellLock = this.m_cellLock;
                lock (cellLock)
                {
                    MyCubeGridRenderCell cell1 = new MyCubeGridRenderCell(this.m_gridRender);
                    cell1.DebugName = vectori.ToString();
                    cell = cell1;
                    this.m_cells.TryAdd(vectori, cell);
                }
            }
            return cell;
        }

        public void OnRemovedFromRender()
        {
            bool flag1;
            IMyEntity entity = this.m_gridRender.Entity;
            if (entity != null)
            {
                flag1 = !entity.MarkedForClose;
            }
            else
            {
                IMyEntity local1 = entity;
                flag1 = false;
            }
            bool flag = flag1;
            object cellsUpdateLock = this.m_cellsUpdateLock;
            lock (cellsUpdateLock)
            {
                foreach (KeyValuePair<Vector3I, MyCubeGridRenderCell> pair in this.m_cells)
                {
                    MyCubeGridRenderCell instance = pair.Value;
                    instance.OnRemovedFromRender();
                    if (flag)
                    {
                        this.m_dirtyCells.Add(instance);
                    }
                }
            }
        }

        public void RebuildDirtyCells(RenderFlags renderFlags)
        {
            using (MyUtils.ReuseCollection<MyCubeGridRenderCell>(ref m_dirtyCellsBuffer))
            {
                using (ConcurrentEnumerator<SpinLockRef.Token, MyCubeGridRenderCell, HashSet<MyCubeGridRenderCell>.Enumerator> enumerator = this.m_dirtyCells.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            this.m_dirtyCells.Clear();
                            break;
                        }
                        m_dirtyCellsBuffer.Add(enumerator.Current);
                    }
                }
                object cellsUpdateLock = this.m_cellsUpdateLock;
                lock (cellsUpdateLock)
                {
                    using (List<MyCubeGridRenderCell>.Enumerator enumerator2 = m_dirtyCellsBuffer.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            enumerator2.Current.RebuildInstanceParts(renderFlags);
                        }
                    }
                }
            }
        }

        public void RemoveCubePart(MyCubePart part)
        {
            Vector3 translation = part.InstanceData.Translation;
            MyCubeGridRenderCell orAddCell = this.GetOrAddCell(ref translation, false);
            if ((orAddCell != null) && orAddCell.RemoveCubePart(part))
            {
                this.m_dirtyCells.Add(orAddCell);
            }
        }

        private void RemoveDecal(Vector3I position, ConcurrentQueue<MyDecalPartIdentity> decals, MyCube cube)
        {
            MyDecalPartIdentity identity;
            decals.TryDequeue(out identity);
            MyDecals.RemoveDecal(identity.DecalId);
            if (identity.CubePartIndex != -1)
            {
                MyCubePart part = cube.Parts[identity.CubePartIndex];
                this.GetOrAddCell((Vector3) position, true).RemoveCubePartDecal(part, identity.DecalId);
            }
        }

        public void RemoveDecals(Vector3I position)
        {
            ConcurrentQueue<MyDecalPartIdentity> queue;
            if (this.m_cubeDecals.TryGetValue(position, out queue))
            {
                MyCube cube;
                this.m_gridRender.CubeGrid.TryGetCube(position, out cube);
                while (!queue.IsEmpty)
                {
                    this.RemoveDecal(position, queue, cube);
                }
            }
        }

        public void RemoveEdgeInfo(Vector3 point0, Vector3 point1, MySlimBlock owner)
        {
            Vector3 pos = (point0 + point1) * 0.5f;
            MyCubeGridRenderCell orAddCell = this.GetOrAddCell(pos, true);
            if (orAddCell.RemoveEdgeInfo(this.CalculateEdgeHash(point0, point1), owner))
            {
                this.m_dirtyCells.Add(orAddCell);
            }
        }

        public void SetBasePositionHint(Vector3 basePos)
        {
            if (this.m_cells.Count == 0)
            {
                this.m_basePos = basePos;
            }
        }

        public ConcurrentDictionary<Vector3I, MyCubeGridRenderCell> Cells =>
            this.m_cells;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCubeGridRenderData.<>c <>9 = new MyCubeGridRenderData.<>c();
            public static Func<Vector3I, ConcurrentQueue<MyCubeGridRenderData.MyDecalPartIdentity>> <>9__23_0;

            internal ConcurrentQueue<MyCubeGridRenderData.MyDecalPartIdentity> <AddDecal>b__23_0(Vector3I x) => 
                new ConcurrentQueue<MyCubeGridRenderData.MyDecalPartIdentity>();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyDecalPartIdentity
        {
            public uint DecalId;
            public int CubePartIndex;
        }
    }
}

