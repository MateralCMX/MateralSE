namespace Sandbox.Engine.Voxels
{
    using Havok;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Voxels;
    using VRage.Generics;
    using VRage.Voxels;
    using VRage.Voxels.Mesh;

    internal sealed class MyPrecalcJobPhysicsPrefetch : MyPrecalcJob
    {
        private static readonly MyDynamicObjectPool<MyPrecalcJobPhysicsPrefetch> m_instancePool = new MyDynamicObjectPool<MyPrecalcJobPhysicsPrefetch>(0x10);
        private Args m_args;
        private volatile bool m_isCancelled;
        public int Taken;
        public volatile bool ResultComplete;
        public HkBvCompressedMeshShape Result;

        public MyPrecalcJobPhysicsPrefetch() : base(true)
        {
        }

        public override void Cancel()
        {
            this.m_isCancelled = true;
        }

        public override void DoWork()
        {
            try
            {
                if (!this.m_isCancelled)
                {
                    using (VrVoxelMesh mesh = this.m_args.TargetPhysics.CreateMesh(this.m_args.GeometryCell))
                    {
                        if (!mesh.IsEmpty())
                        {
                            if (!this.m_isCancelled)
                            {
                                this.Result = this.m_args.TargetPhysics.CreateShape(mesh, this.m_args.GeometryCell.Lod);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    this.ResultComplete = true;
                }
            }
            finally
            {
            }
        }

        protected override void OnComplete()
        {
            base.OnComplete();
            if (!this.m_isCancelled && (this.m_args.TargetPhysics.Entity != null))
            {
                this.m_args.TargetPhysics.OnTaskComplete(this.m_args.GeometryCell, this.Result);
            }
            if (!this.m_isCancelled)
            {
                this.m_args.Tracker.Complete(this.m_args.GeometryCell);
            }
            if (!this.Result.Base.IsZero && (this.Taken == 0))
            {
                this.Result.Base.RemoveReference();
            }
            this.Taken = 0;
            this.m_args = new Args();
            this.m_isCancelled = false;
            this.ResultComplete = false;
            this.Result = (HkBvCompressedMeshShape) HkShape.Empty;
            m_instancePool.Deallocate(this);
        }

        public static void Start(Args args)
        {
            MyPrecalcJobPhysicsPrefetch work = m_instancePool.Allocate();
            work.m_args = args;
            args.Tracker.Add(args.GeometryCell, work);
            MyPrecalcComponent.EnqueueBack(work);
        }

        public override bool IsCanceled =>
            this.m_isCancelled;

        [StructLayout(LayoutKind.Sequential)]
        public struct Args
        {
            public MyWorkTracker<MyCellCoord, MyPrecalcJobPhysicsPrefetch> Tracker;
            public IMyStorage Storage;
            public MyCellCoord GeometryCell;
            public MyVoxelPhysicsBody TargetPhysics;
        }
    }
}

