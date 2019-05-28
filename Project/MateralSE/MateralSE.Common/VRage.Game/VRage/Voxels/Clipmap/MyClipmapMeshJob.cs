namespace VRage.Voxels.Clipmap
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game.Voxels;
    using VRage.Utils;
    using VRage.Voxels;
    using VRage.Voxels.DualContouring;
    using VRage.Voxels.Sewing;
    using VRageMath;

    public sealed class MyClipmapMeshJob : MyPrecalcJob
    {
        private static readonly MyConcurrentPool<MyClipmapMeshJob> m_instancePool = new MyConcurrentPool<MyClipmapMeshJob>(0x10, null, 0x2710, null);
        private VrSewGuide m_meshAndGuide;
        private MyVoxelContentConstitution m_resultConstitution;
        private volatile bool m_isCancelled;

        public MyClipmapMeshJob() : base(true)
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
                BoundingBoxI cellBounds = this.Clipmap.GetCellBounds(this.Cell, true);
                VrSewGuide meshAndGuide = this.m_meshAndGuide;
                MyMesherResult result = (meshAndGuide == null) ? this.Clipmap.Mesher.CalculateMesh(this.Cell.Lod, cellBounds.Min, cellBounds.Max, MyStorageDataTypeFlags.All, 0, null) : this.Clipmap.Mesher.CalculateMesh(this.Cell.Lod, cellBounds.Min, cellBounds.Max, MyStorageDataTypeFlags.All, 0, meshAndGuide.Mesh);
                this.m_resultConstitution = result.Constitution;
                if (result.Constitution != MyVoxelContentConstitution.Mixed)
                {
                    if (meshAndGuide != null)
                    {
                        MyStorageData storageData = MyDualContouringMesher.Static.StorageData;
                        VrShellDataCache dataCache = VrShellDataCache.FromDataCube(storageData.Size3D, storageData[MyStorageDataTypeEnum.Content], storageData[MyStorageDataTypeEnum.Material]);
                        meshAndGuide.SetMesh(meshAndGuide.Mesh, dataCache);
                    }
                }
                else
                {
                    MyStorageData storageData = MyDualContouringMesher.Static.StorageData;
                    VrShellDataCache dataCache = VrShellDataCache.FromDataCube(storageData.Size3D, storageData[MyStorageDataTypeEnum.Content], storageData[MyStorageDataTypeEnum.Material]);
                    if (meshAndGuide != null)
                    {
                        meshAndGuide.SetMesh(result.Mesh, dataCache);
                    }
                    else
                    {
                        meshAndGuide = new VrSewGuide(result.Mesh, dataCache, this.Clipmap);
                    }
                }
                this.Clipmap.UpdateCellData(this, this.Cell, meshAndGuide, this.m_resultConstitution);
            }
            finally
            {
                this.m_meshAndGuide = null;
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
            if (this.IsReusingGuide)
            {
                this.m_meshAndGuide.RemoveReference(this.Clipmap);
                this.m_meshAndGuide = null;
            }
            m_instancePool.Return(this);
        }

        public static bool Start(MyWorkTracker<MyCellCoord, MyClipmapMeshJob> tracker, MyVoxelClipmap clipmap, MyCellCoord cell, VrSewGuide existingGuide = null)
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
            MyClipmapMeshJob local1 = m_instancePool.Get();
            local1.m_isCancelled = false;
            local1.Clipmap = clipmap;
            local1.Cell = cell;
            local1.WorkTracker = tracker;
            local1.m_meshAndGuide = existingGuide;
            return local1.Enqueue();
        }

        public MyVoxelClipmap Clipmap { get; private set; }

        public MyCellCoord Cell { get; private set; }

        public MyWorkTracker<MyCellCoord, MyClipmapMeshJob> WorkTracker { get; private set; }

        public override bool IsCanceled =>
            this.m_isCancelled;

        public override int Priority =>
            (this.m_isCancelled ? 0x7fffffff : this.Cell.Lod);

        public bool IsReusingGuide =>
            (this.m_meshAndGuide != null);
    }
}

