namespace VRage.Voxels.Clipmap
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Collections;
    using VRage.Game.Voxels;
    using VRage.Utils;
    using VRage.Voxels;
    using VRage.Voxels.DualContouring;
    using VRageMath;
    using VRageRender.Voxels;

    public class MyClipmapFullMeshJob : MyPrecalcJob
    {
        private static readonly MyConcurrentPool<MyClipmapFullMeshJob> m_instancePool = new MyConcurrentPool<MyClipmapFullMeshJob>(0x10, null, 0x2710, null);
        private MyVoxelRenderCellData m_cellData;
        private volatile bool m_isCancelled;

        public MyClipmapFullMeshJob() : base(true)
        {
        }

        public override void Cancel()
        {
            this.m_isCancelled = true;
        }

        public override void DebugDraw(Color c)
        {
        }

        public override void DoWork()
        {
            try
            {
                if (!this.m_isCancelled)
                {
                    BoundingBoxI cellBounds = this.Clipmap.GetCellBounds(this.Cell, true);
                    MyMesherResult result = this.Clipmap.Mesher.CalculateMesh(this.Cell.Lod, cellBounds.Min, cellBounds.Max, MyStorageDataTypeFlags.All, 0, null);
                    if (this.m_isCancelled || !result.MeshProduced)
                    {
                        this.m_cellData = new MyVoxelRenderCellData();
                    }
                    else
                    {
                        MyRenderDataBuilder.Instance.Build(result.Mesh, out this.m_cellData, this.Clipmap.VoxelRenderDataProcessorProvider);
                    }
                    if (result.MeshProduced)
                    {
                        result.Mesh.Dispose();
                    }
                }
            }
            finally
            {
            }
        }

        private bool Enqueue()
        {
            this.WorkTracker.Add(this.Cell, this);
            if (MyPrecalcComponent.EnqueueBack(this))
            {
                return true;
            }
            this.WorkTracker.Complete(this.Cell);
            return false;
        }

        protected override void OnComplete()
        {
            base.OnComplete();
            bool flag = false;
            if (!this.m_isCancelled && (this.Clipmap.Mesher != null))
            {
                this.Clipmap.UpdateCellRender(this.Cell, null, ref this.m_cellData);
                if (!base.IsValid)
                {
                    flag = true;
                }
            }
            if (!this.m_isCancelled)
            {
                this.WorkTracker.Complete(this.Cell);
            }
            this.m_cellData = new MyVoxelRenderCellData();
            if (flag)
            {
                this.Enqueue();
            }
            else
            {
                m_instancePool.Return(this);
            }
        }

        public static bool Start(MyWorkTracker<MyCellCoord, MyClipmapFullMeshJob> tracker, MyVoxelClipmap clipmap, MyCellCoord cell)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException("tracker");
            }
            if (clipmap == null)
            {
                throw new ArgumentNullException("clipmap");
            }
            if (tracker.Exists(cell))
            {
                object[] args = new object[] { cell };
                MyLog.Default.Error("A Stitch job for cell {0} is already scheduled.", args);
                return false;
            }
            MyClipmapFullMeshJob local1 = m_instancePool.Get();
            local1.m_isCancelled = false;
            local1.Clipmap = clipmap;
            local1.Cell = cell;
            local1.WorkTracker = tracker;
            return local1.Enqueue();
        }

        public MyVoxelClipmap Clipmap { get; private set; }

        public MyCellCoord Cell { get; private set; }

        public MyWorkTracker<MyCellCoord, MyClipmapFullMeshJob> WorkTracker { get; private set; }

        public override bool IsCanceled =>
            this.m_isCancelled;

        public override int Priority =>
            (this.m_isCancelled ? 0x7fffffff : this.Cell.Lod);
    }
}

