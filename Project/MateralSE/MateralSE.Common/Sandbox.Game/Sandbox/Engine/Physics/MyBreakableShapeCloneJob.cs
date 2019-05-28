namespace Sandbox.Engine.Physics
{
    using Havok;
    using Sandbox;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Voxels;
    using VRage.Generics;
    using VRage.Voxels;

    public class MyBreakableShapeCloneJob : MyPrecalcJob
    {
        private static readonly MyDynamicObjectPool<MyBreakableShapeCloneJob> m_instancePool = new MyDynamicObjectPool<MyBreakableShapeCloneJob>(0x10);
        private Args m_args;
        private List<HkdBreakableShape> m_clonedShapes;
        private bool m_isCanceled;

        public MyBreakableShapeCloneJob() : base(true)
        {
            this.m_clonedShapes = new List<HkdBreakableShape>();
        }

        public override void Cancel()
        {
            this.m_isCanceled = true;
        }

        public override void DoWork()
        {
            for (int i = 0; i < this.m_args.Count; i++)
            {
                if (this.m_isCanceled && (i > 0))
                {
                    return;
                }
                this.m_clonedShapes.Add(this.m_args.ShapeToClone.Clone());
            }
        }

        protected override void OnComplete()
        {
            base.OnComplete();
            if ((MyDestructionData.Static != null) && (MyDestructionData.Static.BlockShapePool != null))
            {
                MyDestructionData.Static.BlockShapePool.EnqueShapes(this.m_args.Model, this.m_args.DefId, this.m_clonedShapes);
            }
            this.m_clonedShapes.Clear();
            this.m_args.Tracker.Complete(this.m_args.DefId);
            this.m_args = new Args();
            this.m_isCanceled = false;
            m_instancePool.Deallocate(this);
        }

        public static void Start(Args args)
        {
            MyBreakableShapeCloneJob work = m_instancePool.Allocate();
            work.m_args = args;
            args.Tracker.Add(args.DefId, work);
            MyPrecalcComponent.EnqueueBack(work);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Args
        {
            public MyWorkTracker<MyDefinitionId, MyBreakableShapeCloneJob> Tracker;
            public string Model;
            public MyDefinitionId DefId;
            public HkdBreakableShape ShapeToClone;
            public int Count;
        }
    }
}

