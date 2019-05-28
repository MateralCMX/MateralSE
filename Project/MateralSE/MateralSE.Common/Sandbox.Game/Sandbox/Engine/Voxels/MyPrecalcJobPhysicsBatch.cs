namespace Sandbox.Engine.Voxels
{
    using Havok;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Voxels;
    using VRage.Generics;
    using VRage.Utils;
    using VRage.Voxels;
    using VRage.Voxels.Mesh;
    using VRageMath;

    internal sealed class MyPrecalcJobPhysicsBatch : MyPrecalcJob
    {
        private static readonly MyDynamicObjectPool<MyPrecalcJobPhysicsBatch> m_instancePool = new MyDynamicObjectPool<MyPrecalcJobPhysicsBatch>(8);
        private MyVoxelPhysicsBody m_targetPhysics;
        internal HashSet<Vector3I> CellBatch;
        private Dictionary<Vector3I, HkBvCompressedMeshShape> m_newShapes;
        private volatile bool m_isCancelled;
        public int Lod;

        public MyPrecalcJobPhysicsBatch() : base(true)
        {
            this.CellBatch = new HashSet<Vector3I>(Vector3I.Comparer);
            this.m_newShapes = new Dictionary<Vector3I, HkBvCompressedMeshShape>(Vector3I.Comparer);
        }

        public override void Cancel()
        {
            this.m_isCancelled = true;
        }

        public override void DoWork()
        {
            try
            {
                foreach (Vector3I vectori in this.CellBatch)
                {
                    if (!this.m_isCancelled)
                    {
                        VrVoxelMesh self = this.m_targetPhysics.CreateMesh(new MyCellCoord(this.Lod, vectori));
                        if (!this.m_isCancelled)
                        {
                            if (self.IsEmpty())
                            {
                                this.m_newShapes.Add(vectori, (HkBvCompressedMeshShape) HkShape.Empty);
                            }
                            else
                            {
                                this.m_newShapes.Add(vectori, this.m_targetPhysics.CreateShape(self, this.Lod));
                            }
                            if (self != null)
                            {
                                self.Dispose();
                            }
                            continue;
                        }
                    }
                    break;
                }
            }
            finally
            {
            }
        }

        protected override void OnComplete()
        {
            base.OnComplete();
            if (((MySession.Static != null) == MySession.Static.GetComponent<MyPrecalcComponent>().Loaded) && !this.m_isCancelled)
            {
                this.m_targetPhysics.OnBatchTaskComplete(this.m_newShapes, this.Lod);
            }
            foreach (HkBvCompressedMeshShape shape in this.m_newShapes.Values)
            {
                HkShape shape2 = shape.Base;
                if (!shape2.IsZero)
                {
                    shape.Base.RemoveReference();
                }
            }
            if (ReferenceEquals(this.m_targetPhysics.RunningBatchTask[this.Lod], this))
            {
                this.m_targetPhysics.RunningBatchTask[this.Lod] = null;
            }
            this.m_targetPhysics = null;
            this.CellBatch.Clear();
            this.m_newShapes.Clear();
            this.m_isCancelled = false;
            m_instancePool.Deallocate(this);
        }

        public static void Start(MyVoxelPhysicsBody targetPhysics, ref HashSet<Vector3I> cellBatchForSwap, int lod)
        {
            MyPrecalcJobPhysicsBatch job = m_instancePool.Allocate();
            job.Lod = lod;
            job.m_targetPhysics = targetPhysics;
            MyUtils.Swap<HashSet<Vector3I>>(ref job.CellBatch, ref cellBatchForSwap);
            targetPhysics.RunningBatchTask[lod] = job;
            MyPrecalcComponent.EnqueueBack(job);
        }
    }
}

