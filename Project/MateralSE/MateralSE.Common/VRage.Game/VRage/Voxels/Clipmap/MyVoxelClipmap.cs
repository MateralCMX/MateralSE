namespace VRage.Voxels.Clipmap
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Entities.Components;
    using VRage.Game.Voxels;
    using VRage.Library.Collections;
    using VRage.Utils;
    using VRage.Voxels;
    using VRage.Voxels.Mesh;
    using VRage.Voxels.Sewing;
    using VRageMath;
    using VRageRender;
    using VRageRender.Voxels;

    public class MyVoxelClipmap : IMyLodController
    {
        internal int CellBits;
        internal int CellSize;
        private int m_updateDistance;
        private MyVoxelClipmapCache m_cache;
        internal readonly int[] Ranges = new int[0x10];
        private readonly Vector3I m_voxelSize;
        private MatrixD m_localToWorld;
        private MatrixD m_worldToLocal;
        internal readonly List<MyVoxelClipmapRing> Rings = new List<MyVoxelClipmapRing>(0x10);
        private Ref<int> m_lockOwner = Ref.Create<int>(-1);
        private FastResourceLock m_lock = new FastResourceLock();
        private float? m_spherizeRadius;
        private Vector3D m_spherizePosition;
        private const int LOADED_WAIT_TIME = 5;
        private int m_loadedCounter;
        private MyVoxelClipmapSettings m_settings;
        private bool m_settingsChanged;
        private static readonly MyConcurrentPool<StitchOperation> m_stitchDependencyPool = new MyConcurrentPool<StitchOperation>(100, null, 0xf4240, null);
        private static readonly MyConcurrentPool<CompoundStitchOperation> m_compoundStitchDependencyPool = new MyConcurrentPool<CompoundStitchOperation>(50, null, 0xf4240, null);
        private readonly MyWorkTracker<MyCellCoord, MyClipmapMeshJob> m_dataWorkTracker = new MyWorkTracker<MyCellCoord, MyClipmapMeshJob>(null);
        private readonly MyWorkTracker<MyCellCoord, MyClipmapFullMeshJob> m_fullWorkTracker = new MyWorkTracker<MyCellCoord, MyClipmapFullMeshJob>(null);
        private List<MyTuple<MyCellCoord, VrSewGuide>> m_cachedCellRequests = new List<MyTuple<MyCellCoord, VrSewGuide>>();
        private readonly MyConcurrentQueue<CellRenderUpdate> m_cellRenderUpdates = new MyConcurrentQueue<CellRenderUpdate>(0x80);
        private readonly MyListDictionary<MyCellCoord, StitchOperation> m_stitchDependencies = new MyListDictionary<MyCellCoord, StitchOperation>(MyCellCoord.Comparer);
        private readonly MyWorkTracker<MyCellCoord, MyClipmapSewJob> m_stitchWorkTracker = new MyWorkTracker<MyCellCoord, MyClipmapSewJob>(MyCellCoord.Comparer);
        private static readonly Vector3I[] m_neighbourOffsets = new Vector3I[] { new Vector3I(0, 0, 0), new Vector3I(1, 0, 0), new Vector3I(0, 1, 0), new Vector3I(1, 1, 0), new Vector3I(0, 0, 1), new Vector3I(1, 0, 1), new Vector3I(0, 1, 1), new Vector3I(1, 1, 1) };
        private static readonly VrSewOperation[] m_compromizes = new VrSewOperation[] { 0, (VrSewOperation.X | VrSewOperation.XY | VrSewOperation.XYZ | VrSewOperation.XZ), (VrSewOperation.XY | VrSewOperation.XYZ | VrSewOperation.Y | VrSewOperation.YZ), (VrSewOperation.XY | VrSewOperation.XYZ), (VrSewOperation.XYZ | VrSewOperation.XZ | VrSewOperation.YZ | VrSewOperation.Z), (VrSewOperation.XYZ | VrSewOperation.XZ), (VrSewOperation.XYZ | VrSewOperation.YZ), 0 };
        private UpdateState m_updateState;
        [CompilerGenerated]
        private Action<IMyLodController> m_loaded;
        private Vector3L m_lastPosition;
        private BoundingBoxI? m_invalidateRange;
        public static bool DebugDrawDependencies = false;
        public static bool UpdateVisibility = true;
        public static StitchMode ActiveStitchMode = StitchMode.Stitch;

        public event Action<IMyLodController> Loaded
        {
            add
            {
                this.m_loaded += value;
                Interlocked.Exchange(ref this.m_loadedCounter, 5);
            }
            remove
            {
                this.m_loaded -= value;
            }
        }

        private event Action<IMyLodController> m_loaded
        {
            [CompilerGenerated] add
            {
                Action<IMyLodController> loaded = this.m_loaded;
                while (true)
                {
                    Action<IMyLodController> a = loaded;
                    Action<IMyLodController> action3 = (Action<IMyLodController>) Delegate.Combine(a, value);
                    loaded = Interlocked.CompareExchange<Action<IMyLodController>>(ref this.m_loaded, action3, a);
                    if (ReferenceEquals(loaded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IMyLodController> loaded = this.m_loaded;
                while (true)
                {
                    Action<IMyLodController> source = loaded;
                    Action<IMyLodController> action3 = (Action<IMyLodController>) Delegate.Remove(source, value);
                    loaded = Interlocked.CompareExchange<Action<IMyLodController>>(ref this.m_loaded, action3, source);
                    if (ReferenceEquals(loaded, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyVoxelClipmap(Vector3I voxelSize, MatrixD worldMatrix, MyVoxelMesherComponent mesher, float? spherizeRadius, Vector3D spherizePosition, string settingsGroup = null)
        {
            this.m_voxelSize = voxelSize;
            this.m_spherizeRadius = spherizeRadius;
            this.m_spherizePosition = spherizePosition;
            for (int i = 0; i < 0x10; i++)
            {
                this.Rings.Add(new MyVoxelClipmapRing(this, i));
            }
            this.Mesher = mesher;
            this.UpdateWorldMatrix(ref worldMatrix);
            this.SettingsGroup = settingsGroup;
            MyVoxelClipmapSettings settings = MyVoxelClipmapSettings.GetSettings(this.SettingsGroup);
            this.UpdateSettings(settings);
            this.ApplySettings(false);
        }

        private void ApplySettings(bool invalidateCells)
        {
            this.CellBits = this.m_settings.CellSizeLg2;
            this.CellSize = 1 << (this.CellBits & 0x1f);
            this.m_updateDistance = (this.CellSize * this.CellSize) / 4;
            Vector3I size = (Vector3I) (((this.m_voxelSize + this.CellSize) - 1) >> this.CellBits);
            for (int i = 0; i < 0x10; i++)
            {
                this.Rings[i].UpdateSize(size);
                size = (Vector3I) ((size + 1) >> 1);
                this.Ranges[i] = this.m_settings.LodRanges[i];
            }
            this.m_settingsChanged = false;
            if (invalidateCells)
            {
                this.m_invalidateRange = new BoundingBoxI?(BoundingBoxI.CreateInvalid());
            }
        }

        internal unsafe MyVoxelContentConstitution ApproximateCellConstitution(Vector3I cell, int lod)
        {
            BoundingBoxI cellBounds = this.GetCellBounds(new MyCellCoord(lod, cell), false);
            Vector3I* vectoriPtr1 = (Vector3I*) ref cellBounds.Min;
            vectoriPtr1[0] -= 1;
            IMyStorage storage = this.Mesher.Storage;
            if (storage == null)
            {
                return MyVoxelContentConstitution.Empty;
            }
            switch (storage.Intersect(ref cellBounds, lod, true))
            {
                case ContainmentType.Disjoint:
                    return MyVoxelContentConstitution.Empty;

                case ContainmentType.Contains:
                    return MyVoxelContentConstitution.Full;

                case ContainmentType.Intersects:
                    return MyVoxelContentConstitution.Mixed;
            }
            throw new ArgumentOutOfRangeException();
        }

        public void BindToActor(IMyVoxelActor actor)
        {
            if (this.Actor != null)
            {
                throw new InvalidOperationException("Lod Controller is already bound to actor.");
            }
            this.Actor = actor;
            this.Actor.TransitionMode = MyVoxelActorTransitionMode.Fade;
            this.Actor.Move += new ActionRef<MatrixD>(this.UpdateWorldMatrix);
            this.BindToCache();
            this.m_updateState = UpdateState.Idle;
        }

        private void BindToCache()
        {
            if ((this.m_cache != null) && (this.Actor != null))
            {
                this.m_cache.Register(this.Actor.Id, this);
            }
        }

        private void CollectChildStitch(CompoundStitchOperation compound, MyVoxelClipmapRing.CellData parentData, MyCellCoord cell, Vector3I neighbourOffset)
        {
            Vector3I vectori = (Vector3I) (cell.CoordInLod + neighbourOffset);
            Vector3I vectori2 = Vector3I.One - neighbourOffset;
            int lod = cell.Lod - 1;
            goto TR_0018;
        TR_0001:
            lod--;
        TR_0018:
            while (true)
            {
                if (lod < 0)
                {
                    break;
                }
                int num2 = cell.Lod - lod;
                Vector3I vectori3 = vectori << num2;
                Vector3I vectori4 = (Vector3I) ((vectori3 + (((1 << (num2 & 0x1f)) - 1) * vectori2)) + 1);
                if (this.Rings[lod] == null)
                {
                    break;
                }
                if (!this.Rings[lod].IsInBounds(vectori3) && !this.Rings[lod].IsInBounds(vectori4))
                {
                    break;
                }
                if (this.Rings[lod].Cells.Count<KeyValuePair<Vector3I, MyVoxelClipmapRing.CellData>>() == 0)
                {
                    break;
                }
                using (IEnumerator<Vector3I> enumerator = Vector3I.EnumerateRange(vectori3, vectori4).GetEnumerator())
                {
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            Vector3I current = enumerator.Current;
                            StitchOperation preallocatedOperation = m_stitchDependencyPool.Get();
                            if (preallocatedOperation != null)
                            {
                                preallocatedOperation.Init(cell);
                                this.PrepareStitch(parentData, new MyCellCoord(lod, current - neighbourOffset), compound, preallocatedOperation);
                                compound.Children.Add(preallocatedOperation);
                                Vector3I min = ((current - vectori3) << this.CellBits) >> num2;
                                Vector3I max = (Vector3I) (min + ((1 << (this.CellBits & 0x1f)) >> (num2 & 0x1f)));
                                if (neighbourOffset.X == 1)
                                {
                                    max.X = this.CellSize;
                                }
                                if (neighbourOffset.Y == 1)
                                {
                                    max.Y = this.CellSize;
                                }
                                if (neighbourOffset.Z == 1)
                                {
                                    max.Z = this.CellSize;
                                }
                                preallocatedOperation.Range = new BoundingBoxI(min, max);
                                continue;
                            }
                        }
                        else
                        {
                            goto TR_0001;
                        }
                        break;
                    }
                    break;
                }
                goto TR_0001;
            }
        }

        private bool CollectMeshes(StitchOperation stitch, bool child = false)
        {
            MyVoxelClipmapRing.CellData data;
            bool flag = false;
            for (int i = 0; i < stitch.Dependencies.Length; i++)
            {
                MyCellCoord cell = stitch.Dependencies[i];
                if (cell.Lod >= 0)
                {
                    stitch.Dependencies[i] = MakeFulfilled(stitch.Dependencies[i]);
                    if (this.Rings[cell.Lod].TryGetCell(cell.CoordInLod, out data))
                    {
                        stitch.Guides[i] = this.CollectMeshForOperation(stitch, cell, data);
                    }
                }
                if ((stitch.Guides[i] != null) && (stitch.Guides[i].Mesh != null))
                {
                    flag = true;
                }
            }
            if (stitch.Guides[0] == null)
            {
                return false;
            }
            if (!flag)
            {
                return false;
            }
            if (!child)
            {
                MyVoxelClipmapRing.Vicinity vicinity = new MyVoxelClipmapRing.Vicinity(stitch.Guides, stitch.Dependencies);
                MyCellCoord coord2 = MakeFulfilled(stitch.Dependencies[0]);
                this.Rings[coord2.Lod].TryGetCell(coord2.CoordInLod, out data);
                if (data.Vicinity == vicinity)
                {
                    return false;
                }
                data.Vicinity = vicinity;
            }
            return true;
        }

        private VrSewGuide CollectMeshForOperation(StitchOperation op, MyCellCoord cell, MyVoxelClipmapRing.CellData cellData)
        {
            if (op.BorderOperation && (cellData.Guide == null))
            {
                BoundingBoxI cellBounds = this.GetCellBounds(cell, true);
                cellData.Guide = new VrSewGuide(cell.Lod, cellBounds.Min, cellBounds.Max, this.GetShellCacheForConstitution(cellData.Constitution), this);
            }
            return cellData.Guide;
        }

        internal void CommitStitchOperation(StitchOperation stitch, bool dereference = true)
        {
            CompoundStitchOperation compound = stitch.GetCompound();
            if (compound == null)
            {
                stitch.Clear(dereference);
                m_stitchDependencyPool.Return(stitch);
            }
            else
            {
                foreach (StitchOperation operation2 in compound.Children)
                {
                    operation2.Clear(dereference);
                    m_stitchDependencyPool.Return(operation2);
                }
                stitch.Clear(dereference);
                m_compoundStitchDependencyPool.Return(compound);
            }
        }

        public void DebugDraw(ref MatrixD cameraMatrix)
        {
            using (this.m_lock.AcquireExclusiveRecursiveUsing(this.m_lockOwner))
            {
                using (List<MyVoxelClipmapRing>.Enumerator enumerator = this.Rings.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.DebugDraw();
                    }
                }
                if (DebugDrawDependencies)
                {
                    this.DebugDrawDependenciesInternal();
                }
            }
        }

        private void DebugDrawDependenciesInternal()
        {
            Vector3L lastPosition = this.m_lastPosition;
            foreach (KeyValuePair<MyCellCoord, List<StitchOperation>> pair in this.m_stitchDependencies)
            {
                Vector3D cellCenter = this.GetCellCenter(pair.Key);
                float a = 1f;
                if (a >= 0f)
                {
                    MyVoxelClipmapRing.CellData data;
                    Color color = new Color(Color.Orange, a);
                    MyCellCoord key = pair.Key;
                    if (this.Rings[key.Lod].TryGetCell(key.CoordInLod, out data))
                    {
                        MyRenderProxy.DebugDrawText3D(cellCenter, $"Status: {data.Status}, Constitution: {data.Constitution}", color, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    }
                    foreach (StitchOperation operation in pair.Value)
                    {
                        color = new Color(Color.Orange, a);
                        Vector3D pointFrom = this.GetCellCenter(operation.Cell);
                        MyRenderProxy.DebugDrawArrow3D(pointFrom, cellCenter, color, new Color?(color), true, 0.1, null, 0.5f, false);
                        MyRenderProxy.DebugDrawText3D(pointFrom, $"Stitch dependency 
pending: {operation.Pending}", color, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1, false);
                    }
                }
            }
        }

        private void DispatchStitch(StitchOperation stitch)
        {
            CompoundStitchOperation compound = stitch.GetCompound();
            if (compound == null)
            {
                if (!this.CollectMeshes(stitch, false))
                {
                    stitch.Start();
                    this.CommitStitchOperation(stitch, true);
                    return;
                }
            }
            else
            {
                this.CollectMeshes(stitch, false);
                for (int i = compound.Children.Count - 1; i >= 0; i--)
                {
                    if (!this.CollectMeshes(compound.Children[i], true))
                    {
                        compound.Children[i].Start();
                        this.CommitStitchOperation(compound.Children[i], true);
                        compound.Children.RemoveAtFast<StitchOperation>(i);
                    }
                }
            }
            stitch.Start();
            CompoundStitchOperation operation2 = stitch.GetCompound();
            if (operation2 != null)
            {
                operation2.Children.ForEach(x => x.Start());
            }
            if (this.m_stitchWorkTracker.Exists(stitch.Cell))
            {
                this.CommitStitchOperation(stitch, true);
            }
            else if (!MyClipmapSewJob.Start(this.m_stitchWorkTracker, this, stitch))
            {
                this.CommitStitchOperation(stitch, true);
            }
        }

        private void FeedMeshResult(MyCellCoord cell, VrSewGuide mesh, MyVoxelContentConstitution constitution)
        {
            MyVoxelClipmapRing ring = this.Rings[cell.Lod];
            if ((mesh == null) && ring.IsForwardEdge(cell.CoordInLod))
            {
                BoundingBoxI cellBounds = this.GetCellBounds(cell, true);
                mesh = new VrSewGuide(cell.Lod, cellBounds.Min, cellBounds.Max, this.GetShellCacheForConstitution(constitution), this);
            }
            ring.UpdateCellData(cell.CoordInLod, mesh, constitution);
            this.ReadyAllStitchDependencies(cell);
            if (mesh != null)
            {
                this.Stitch(cell);
            }
        }

        ~MyVoxelClipmap()
        {
        }

        public VrVoxelMesh GetCachedMesh(Vector3I coord)
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                int num = 0;
                while (true)
                {
                    MyVoxelClipmapRing.CellData data;
                    if (num >= 0x10)
                    {
                        break;
                    }
                    Vector3I cell = coord >> (num + this.CellBits);
                    if (!this.Rings[num].TryGetCell(cell, out data) || (data.Guide == null))
                    {
                        num++;
                        continue;
                    }
                    return data.Guide.Mesh;
                }
            }
            return null;
        }

        public VrVoxelMesh GetCachedMesh(Vector3I coord, int lod)
        {
            using (this.m_lock.AcquireSharedUsing())
            {
                MyVoxelClipmapRing.CellData data;
                coord = coord >> (lod + this.CellBits);
                if (this.Rings[lod].TryGetCell(coord, out data) && (data.Guide != null))
                {
                    return data.Guide.Mesh;
                }
            }
            return null;
        }

        public BoundingBoxI GetCellBounds(MyCellCoord cell, bool inLod = true)
        {
            int cellSize = this.CellSize;
            Vector3I min = cell.CoordInLod * cellSize;
            Vector3I max = (Vector3I) (min + cellSize);
            switch (this.InstanceStitchMode)
            {
                case StitchMode.None:
                case StitchMode.Stitch:
                    break;

                case StitchMode.BlindMeet:
                    max = (Vector3I) (max + 1);
                    break;

                case StitchMode.Overlap:
                    max = (Vector3I) (max + 2);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (!inLod)
            {
                min = min << cell.Lod;
                max = max << cell.Lod;
            }
            return new BoundingBoxI(min, max);
        }

        private Vector3D GetCellCenter(MyCellCoord cell) => 
            Vector3D.Transform((Vector3D) (((cell.CoordInLod * this.CellSize) + (this.CellSize >> 1)) << cell.Lod), this.LocalToWorld);

        private VrShellDataCache GetShellCacheForConstitution(MyVoxelContentConstitution constitution) => 
            ((constitution == MyVoxelContentConstitution.Empty) ? VrShellDataCache.Empty : VrShellDataCache.Full);

        internal void HandleCacheEviction(MyCellCoord coord, VrSewGuide guide)
        {
        }

        public void InvalidateAll()
        {
            using (this.m_lock.AcquireExclusiveRecursiveUsing(this.m_lockOwner))
            {
                if (this.m_updateState != UpdateState.NotReady)
                {
                    this.m_invalidateRange = new BoundingBoxI?(BoundingBoxI.CreateInvalid());
                }
            }
        }

        private void InvalidateInternal(BoundingBoxI bounds)
        {
            List<MyVoxelClipmapRing>.Enumerator enumerator;
            if (bounds == BoundingBoxI.CreateInvalid())
            {
                if (this.m_cache != null)
                {
                    this.m_cache.EvictAll(this.Actor.Id);
                }
                this.Actor.BeginBatch(1);
                using (enumerator = this.Rings.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.InvalidateAll();
                    }
                }
                this.m_lastPosition = (Vector3L) Vector3I.MinValue;
            }
            else
            {
                if (this.m_cache != null)
                {
                    this.m_cache.EvictAll(this.Actor.Id, new BoundingBoxI(bounds.Min >> this.CellBits, (Vector3I) ((bounds.Max + (this.CellSize - 1)) >> this.CellBits)));
                }
                this.Actor.BeginBatch(0);
                using (enumerator = this.Rings.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.InvalidateRange(bounds);
                    }
                }
                this.m_updateState = UpdateState.Calculate;
            }
        }

        public void InvalidateRange(Vector3I min, Vector3I max)
        {
            BoundingBoxI box = new BoundingBoxI(min - 1, (Vector3I) (max + 1));
            using (this.m_lock.AcquireExclusiveRecursiveUsing(this.m_lockOwner))
            {
                if (this.m_updateState != UpdateState.NotReady)
                {
                    if (this.m_invalidateRange != null)
                    {
                        this.m_invalidateRange = new BoundingBoxI?(this.m_invalidateRange.Value.Include(box));
                    }
                    else
                    {
                        this.m_invalidateRange = new BoundingBoxI?(box);
                    }
                }
            }
        }

        internal static MyCellCoord MakeFulfilled(MyCellCoord fullfiled) => 
            new MyCellCoord(~fullfiled.Lod, fullfiled.CoordInLod);

        private bool MoveUpdate(ref MatrixD view, BoundingFrustumD viewFrustum, float farClipping)
        {
            List<MyVoxelClipmapRing>.Enumerator enumerator;
            Vector3D vectord = Vector3D.Transform(view.Translation, this.m_worldToLocal) / 1.0;
            if (Vector3D.DistanceSquared(vectord, (Vector3D) this.m_lastPosition) < this.m_updateDistance)
            {
                return false;
            }
            Vector3L relativePosition = new Vector3L(vectord);
            this.m_lastPosition = relativePosition;
            if (!this.Actor.IsBatching)
            {
                this.Actor.BeginBatch(1);
            }
            using (enumerator = this.Rings.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Update(relativePosition);
                }
            }
            using (enumerator = this.Rings.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ProcessChanges();
                }
            }
            using (enumerator = this.Rings.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.DispatchStitchingRefreshes();
                }
            }
            this.ProcessCacheHits();
            return true;
        }

        private StitchOperation PrepareStitch(MyVoxelClipmapRing.CellData parentData, MyCellCoord cell, CompoundStitchOperation parent = null, StitchOperation preallocatedOperation = null)
        {
            StitchOperation op = preallocatedOperation;
            if (preallocatedOperation == null)
            {
                using (this.m_lock.AcquireExclusiveRecursiveUsing(this.m_lockOwner))
                {
                    op = m_stitchDependencyPool.Get();
                }
                op.Init(cell);
            }
            StitchOperation operation2 = parent ?? op;
            int pending = operation2.Pending;
            if (parentData.Status == MyVoxelClipmapRing.CellStatus.Pending)
            {
                op.Dependencies[0] = op.Cell;
                operation2.Pending = (short) (operation2.Pending + 1);
            }
            else
            {
                op.Dependencies[0] = MakeFulfilled(op.Cell);
                op.Guides[0] = parentData.Guide;
            }
            VrSewOperation all = VrSewOperation.All;
            if (parent != null)
            {
                all = 0;
            }
            for (int i = 1; i < m_neighbourOffsets.Length; i++)
            {
                MyVoxelClipmapRing.CellData data;
                MyCellCoord coord = new MyCellCoord(cell.Lod, (Vector3I) (cell.CoordInLod + m_neighbourOffsets[i]));
                if (coord.Lod != op.Cell.Lod)
                {
                    op.BorderOperation = true;
                }
                if (!this.TryGetCellAt(ref coord, out data))
                {
                    all = all.Without(m_compromizes[i]);
                    op.Dependencies[i] = MakeFulfilled(coord);
                }
                else if (data.Status == MyVoxelClipmapRing.CellStatus.Pending)
                {
                    op.Dependencies[i] = coord;
                    operation2.Pending = (short) (operation2.Pending + 1);
                }
                else
                {
                    op.Guides[i] = this.CollectMeshForOperation(op, coord, data);
                    op.Dependencies[i] = MakeFulfilled(coord);
                }
                if ((parent != null) && (coord.Lod < op.Cell.Lod))
                {
                    all = all.With(m_compromizes[i]);
                }
            }
            op.Operation = all;
            int num2 = 0;
            if (operation2.Pending != pending)
            {
                for (int j = 0; j < op.Dependencies.Length; j++)
                {
                    MyCellCoord key = op.Dependencies[j];
                    if (key.Lod >= 0)
                    {
                        this.m_stitchDependencies.Add(key, operation2);
                        num2++;
                    }
                }
            }
            return op;
        }

        private void ProcessCacheHits()
        {
            int count = this.m_cachedCellRequests.Count;
            for (int i = 0; i < count; i++)
            {
                MyTuple<MyCellCoord, VrSewGuide> tuple = this.m_cachedCellRequests[i];
                tuple.Item2.AddReference(this);
                this.FeedMeshResult(tuple.Item1, tuple.Item2, MyVoxelContentConstitution.Mixed);
                tuple.Item2.RemoveReference(this.m_dataWorkTracker);
            }
            this.m_cachedCellRequests.Clear();
        }

        private void ProcessUpdates()
        {
            CellRenderUpdate update;
            while (this.m_cellRenderUpdates.TryDequeue(out update))
            {
                if (this.IsUnloaded)
                {
                    if (update.Operation != null)
                    {
                        this.CommitStitchOperation(update.Operation, true);
                    }
                    update.Data.Dispose();
                    continue;
                }
                MyVoxelClipmapRing local1 = this.Rings[update.Cell.Lod];
                local1.UpdateCellRender(update.Cell.CoordInLod, ref update.Data);
                if (update.Operation != null)
                {
                    this.CommitStitchOperation(update.Operation, true);
                    if (update.Operation.Recalculate)
                    {
                        this.Stitch(update.Cell);
                    }
                }
                local1.FinishAdd(update.Cell.CoordInLod);
            }
        }

        private void ReadyAllStitchDependencies(MyCellCoord cell)
        {
            List<StitchOperation> list;
            if (this.m_stitchDependencies.TryGet(cell, out list))
            {
                int num = 0;
                while (true)
                {
                    if (num >= list.Count)
                    {
                        this.m_stitchDependencies.Remove(cell);
                        break;
                    }
                    this.ReadyStitchDependency(list[num]);
                    num++;
                }
            }
        }

        private void ReadyStitchDependency(StitchOperation stitch)
        {
            stitch.Pending = (short) (stitch.Pending - 1);
            if (stitch.Pending == 0)
            {
                this.DispatchStitch(stitch);
            }
        }

        internal void RequestCell(Vector3I cell, int lod, VrSewGuide existingGuide = null)
        {
            MyCellCoord id = new MyCellCoord(lod, cell);
            if (this.InstanceStitchMode != StitchMode.Stitch)
            {
                if (this.m_fullWorkTracker.Exists(id))
                {
                    this.m_fullWorkTracker.Invalidate(id);
                }
                else
                {
                    MyClipmapFullMeshJob.Start(this.m_fullWorkTracker, this, id);
                }
            }
            else
            {
                VrSewGuide guide;
                if ((this.m_cache != null) && this.m_cache.TryRead(this.Actor.Id, id, out guide))
                {
                    guide.AddReference(this);
                    this.m_cachedCellRequests.Add(MyTuple.Create<MyCellCoord, VrSewGuide>(id, guide));
                }
                else if (this.m_dataWorkTracker.Exists(id))
                {
                    this.m_dataWorkTracker.Invalidate(id);
                }
                else
                {
                    if (existingGuide != null)
                    {
                        existingGuide.AddReference(this.m_dataWorkTracker);
                    }
                    MyClipmapMeshJob.Start(this.m_dataWorkTracker, this, id, existingGuide);
                }
            }
        }

        internal bool Stitch(MyCellCoord cell)
        {
            MyVoxelClipmapRing.CellData data;
            StitchOperation operation;
            int num;
            if (this.InstanceStitchMode != StitchMode.Stitch)
            {
                return false;
            }
            MyVoxelClipmapRing ring = this.Rings[cell.Lod];
            if (!ring.TryGetCell(cell.CoordInLod, out data))
            {
                return false;
            }
            if (this.m_stitchWorkTracker.Exists(cell))
            {
                this.m_stitchWorkTracker.Invalidate(cell);
                return true;
            }
            if (!ring.IsInnerLodEdge(cell.CoordInLod, out num))
            {
                operation = this.PrepareStitch(data, cell, null, null);
            }
            else
            {
                CompoundStitchOperation operation2;
                using (this.m_lock.AcquireExclusiveRecursiveUsing(this.m_lockOwner))
                {
                    operation2 = m_compoundStitchDependencyPool.Get();
                }
                if (operation2 == null)
                {
                    return false;
                }
                operation2.Init(cell);
                this.PrepareStitch(data, cell, null, operation2);
                if (ring.IsInsideInnerLod((Vector3I) (cell.CoordInLod + m_neighbourOffsets[num])))
                {
                    this.CollectChildStitch(operation2, data, cell, m_neighbourOffsets[num]);
                }
                operation = operation2;
            }
            if ((operation != null) && (operation.Pending == 0))
            {
                this.DispatchStitch(operation);
            }
            return true;
        }

        private unsafe bool TryGetCellAt(ref MyCellCoord cell, out MyVoxelClipmapRing.CellData data)
        {
            while (!this.Rings[cell.Lod].TryGetCell(cell.CoordInLod, out data))
            {
                int* numPtr1 = (int*) ref cell.Lod;
                numPtr1[0]++;
                Vector3I* vectoriPtr1 = (Vector3I*) ref cell.CoordInLod;
                vectoriPtr1[0] = vectoriPtr1[0] >> 1;
                if (cell.Lod >= 0x10)
                {
                    data = null;
                    return false;
                }
            }
            return true;
        }

        public void Unload()
        {
            this.m_dataWorkTracker.CancelAll();
            this.m_fullWorkTracker.CancelAll();
            this.m_stitchWorkTracker.CancelAll();
            using (this.m_lock.AcquireExclusiveRecursiveUsing(this.m_lockOwner))
            {
                this.m_updateState = UpdateState.Unloaded;
                this.Actor.Move -= new ActionRef<MatrixD>(this.UpdateWorldMatrix);
                this.ProcessUpdates();
                foreach (StitchOperation operation in from x in from x in this.m_stitchDependencies.Values select x
                    group x by x into x
                    select x.Key)
                {
                    operation.Pending = 0;
                    this.CommitStitchOperation(operation, false);
                }
                using (List<MyVoxelClipmapRing>.Enumerator enumerator2 = this.Rings.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.InvalidateAll();
                    }
                }
                this.m_stitchDependencies.Clear();
                if (this.m_cache != null)
                {
                    this.m_cache.Unregister(this.Actor.Id);
                }
                if (this.m_loaded != null)
                {
                    MyRenderProxy.EnqueueMainThreadCallback(delegate {
                        Action<IMyLodController> loaded = this.m_loaded;
                        if (loaded != null)
                        {
                            loaded(this);
                        }
                    });
                }
            }
        }

        public void Update(ref MatrixD view, BoundingFrustumD viewFrustum, float farClipping)
        {
            if ((this.Mesher.Storage != null) && (this.m_updateState != UpdateState.NotReady))
            {
                using (this.m_lock.AcquireExclusiveRecursiveUsing(this.m_lockOwner))
                {
                    this.ProcessUpdates();
                    if (!this.WorkPending)
                    {
                        this.InstanceStitchMode = ActiveStitchMode;
                        UpdateState updateState = this.m_updateState;
                        if (updateState != UpdateState.Idle)
                        {
                            if ((updateState == UpdateState.Calculate) && !this.WorkPending)
                            {
                                this.m_updateState = UpdateState.Idle;
                                bool justLoaded = this.m_loaded != null;
                                if (justLoaded)
                                {
                                    MyRenderProxy.EnqueueMainThreadCallback(delegate {
                                        Action<IMyLodController> loaded = this.m_loaded;
                                        if (loaded != null)
                                        {
                                            loaded(this);
                                        }
                                    });
                                }
                                MyVoxelClipmapCache cache = this.m_cache;
                                this.Actor.EndBatch(justLoaded);
                            }
                        }
                        else if (this.m_settingsChanged)
                        {
                            this.ApplySettings(true);
                        }
                        else if ((UpdateVisibility && !MyRenderProxy.Settings.FreezeTerrainQueries) && this.MoveUpdate(ref view, viewFrustum, farClipping))
                        {
                            this.m_updateState = UpdateState.Calculate;
                        }
                        else if (this.m_invalidateRange != null)
                        {
                            this.InvalidateInternal(this.m_invalidateRange.Value);
                            this.m_invalidateRange = null;
                        }
                        else if (!Equals(this.m_loadedCounter, 0) && (this.m_loaded != null))
                        {
                            Interlocked.Decrement(ref this.m_loadedCounter);
                            if (Equals(this.m_loadedCounter, 0))
                            {
                                MyRenderProxy.EnqueueMainThreadCallback(delegate {
                                    Action<IMyLodController> loaded = this.m_loaded;
                                    if (loaded != null)
                                    {
                                        loaded(this);
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }

        internal void UpdateCellData(MyClipmapMeshJob job, MyCellCoord cell, VrSewGuide guide, MyVoxelContentConstitution constitution)
        {
            using (this.m_lock.AcquireExclusiveRecursiveUsing(this.m_lockOwner))
            {
                if (job.IsReusingGuide)
                {
                    guide.RemoveReference(this.m_dataWorkTracker);
                }
                if ((this.IsUnloaded || job.IsCanceled) && !job.IsReusingGuide)
                {
                    if (guide != null)
                    {
                        guide.RemoveReference(this);
                    }
                }
                else
                {
                    this.FeedMeshResult(cell, guide, constitution);
                    if (((this.m_cache != null) && (guide != null)) && (guide.Mesh != null))
                    {
                        this.m_cache.Write(this.Actor.Id, cell, guide);
                    }
                    this.m_dataWorkTracker.Complete(cell);
                }
            }
        }

        internal void UpdateCellRender(MyCellCoord coord, StitchOperation stitch, ref MyVoxelRenderCellData cellData)
        {
            if (!this.IsUnloaded)
            {
                this.m_cellRenderUpdates.Enqueue(new CellRenderUpdate(coord, stitch, ref cellData));
            }
            else
            {
                using (this.m_lock.AcquireExclusiveRecursiveUsing(this.m_lockOwner))
                {
                    if (stitch != null)
                    {
                        this.CommitStitchOperation(stitch, true);
                    }
                }
            }
        }

        public bool UpdateSettings(MyVoxelClipmapSettings settings)
        {
            if (!settings.IsValid || settings.Equals(this.m_settings))
            {
                return false;
            }
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_settings = settings;
                this.m_settingsChanged = true;
            }
            return true;
        }

        private void UpdateWorldMatrix(ref MatrixD matrix)
        {
            this.LocalToWorld = matrix;
            MatrixD.Invert(ref this.m_localToWorld, out this.m_worldToLocal);
        }

        internal MyVoxelMesherComponent Mesher { get; private set; }

        public MyVoxelClipmapCache Cache
        {
            get => 
                this.m_cache;
            set
            {
                if ((this.m_cache != null) && (this.Actor != null))
                {
                    this.m_cache.Unregister(this.Actor.Id);
                }
                this.m_cache = value;
                this.BindToCache();
            }
        }

        public IMyVoxelRenderDataProcessorProvider VoxelRenderDataProcessorProvider { get; set; }

        public string SettingsGroup { get; private set; }

        public bool IsUnloaded =>
            (this.m_updateState == UpdateState.Unloaded);

        public StitchMode InstanceStitchMode { get; private set; }

        public IEnumerable<IMyVoxelActorCell> Cells =>
            (from x in from x in this.Rings select x.Cells.Values
                select x.Cell into x
                where x != null
                select x);

        public IMyVoxelActor Actor { get; private set; }

        public Vector3I Size =>
            this.m_voxelSize;

        public MatrixD LocalToWorld
        {
            get => 
                this.m_localToWorld;
            set => 
                (this.m_localToWorld = value);
        }

        private bool WorkPending =>
            ((this.m_stitchDependencies.KeyCount != 0) || (this.m_stitchWorkTracker.HasAny || (this.m_dataWorkTracker.HasAny || (this.m_fullWorkTracker.HasAny || (this.m_cellRenderUpdates.Count != 0)))));

        public float? SpherizeRadius =>
            this.m_spherizeRadius;

        public Vector3D SpherizePosition =>
            this.m_spherizePosition;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyVoxelClipmap.<>c <>9 = new MyVoxelClipmap.<>c();
            public static Action<MyVoxelClipmap.StitchOperation> <>9__62_0;
            public static Func<MyVoxelClipmapRing, IEnumerable<MyVoxelClipmapRing.CellData>> <>9__79_0;
            public static Func<MyVoxelClipmapRing.CellData, IMyVoxelActorCell> <>9__79_1;
            public static Func<IMyVoxelActorCell, bool> <>9__79_2;
            public static Func<List<MyVoxelClipmap.StitchOperation>, IEnumerable<MyVoxelClipmap.StitchOperation>> <>9__101_1;
            public static Func<MyVoxelClipmap.StitchOperation, MyVoxelClipmap.StitchOperation> <>9__101_2;
            public static Func<IGrouping<MyVoxelClipmap.StitchOperation, MyVoxelClipmap.StitchOperation>, MyVoxelClipmap.StitchOperation> <>9__101_3;

            internal void <DispatchStitch>b__62_0(MyVoxelClipmap.StitchOperation x)
            {
                x.Start();
            }

            internal IEnumerable<MyVoxelClipmapRing.CellData> <get_Cells>b__79_0(MyVoxelClipmapRing x) => 
                x.Cells.Values;

            internal IMyVoxelActorCell <get_Cells>b__79_1(MyVoxelClipmapRing.CellData x) => 
                x.Cell;

            internal bool <get_Cells>b__79_2(IMyVoxelActorCell x) => 
                (x != null);

            internal IEnumerable<MyVoxelClipmap.StitchOperation> <Unload>b__101_1(List<MyVoxelClipmap.StitchOperation> x) => 
                x;

            internal MyVoxelClipmap.StitchOperation <Unload>b__101_2(MyVoxelClipmap.StitchOperation x) => 
                x;

            internal MyVoxelClipmap.StitchOperation <Unload>b__101_3(IGrouping<MyVoxelClipmap.StitchOperation, MyVoxelClipmap.StitchOperation> x) => 
                x.Key;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CellRenderUpdate
        {
            public readonly MyCellCoord Cell;
            public readonly MyVoxelClipmap.StitchOperation Operation;
            public MyVoxelRenderCellData Data;
            public CellRenderUpdate(MyCellCoord cell, MyVoxelClipmap.StitchOperation operation, ref MyVoxelRenderCellData data)
            {
                this.Cell = cell;
                this.Data = data;
                this.Operation = operation;
            }
        }

        internal class CompoundStitchOperation : MyVoxelClipmap.StitchOperation
        {
            public List<MyVoxelClipmap.StitchOperation> Children = new List<MyVoxelClipmap.StitchOperation>();

            public override void Clear(bool dereference = true)
            {
                base.Clear(dereference);
                this.Children.Clear();
            }

            public override MyVoxelClipmap.CompoundStitchOperation GetCompound() => 
                this;

            public override void Init(MyCellCoord coord)
            {
                base.Init(coord);
            }

            public override void SetState(MyVoxelClipmap.StitchOperation.OpState value)
            {
                for (int i = 0; i < this.Children.Count; i++)
                {
                }
            }
        }

        public enum StitchMode
        {
            None,
            BlindMeet,
            Overlap,
            Stitch
        }

        internal class StitchOperation
        {
            public MyCellCoord[] Dependencies = new MyCellCoord[8];
            public VrSewGuide[] Guides = new VrSewGuide[8];
            public VrSewOperation Operation;
            public BoundingBoxI? Range;
            public short Pending;
            public bool Recalculate;
            public bool BorderOperation;
            private OpState m_state;

            public virtual void Clear(bool dereference = true)
            {
                for (int i = 0; i < this.Guides.Length; i++)
                {
                    if ((this.Guides[i] != null) & dereference)
                    {
                        this.Guides[i].RemoveReference(this);
                    }
                    this.Guides[i] = null;
                }
                this.Recalculate = false;
                if (this.Range != null)
                {
                    this.Range = null;
                }
                this.BorderOperation = false;
            }

            public virtual MyVoxelClipmap.CompoundStitchOperation GetCompound() => 
                null;

            public virtual void Init(MyCellCoord coord)
            {
                this.Cell = coord;
            }

            [Conditional("DEBUG")]
            public virtual void SetState(OpState value)
            {
                this.m_state = value;
            }

            public void Start()
            {
                for (int i = 0; i < this.Guides.Length; i++)
                {
                    if (this.Guides[i] != null)
                    {
                        this.Guides[i].AddReference(this, true);
                    }
                }
            }

            public MyCellCoord Cell { get; private set; }

            public OpState State =>
                this.m_state;

            public enum OpState
            {
                Pooled,
                Pending,
                Queued,
                Working,
                Ready,
                Returned
            }
        }

        private enum UpdateState
        {
            NotReady,
            Idle,
            Calculate,
            Unloaded
        }
    }
}

