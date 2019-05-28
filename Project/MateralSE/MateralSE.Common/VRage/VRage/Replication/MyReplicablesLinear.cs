namespace VRage.Replication
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using VRage.Collections;
    using VRage.Network;
    using VRageMath;

    public class MyReplicablesLinear : MyReplicablesBase
    {
        private const int UPDATE_INTERVAL = 60;
        private readonly HashSet<IMyReplicable> m_roots;
        private readonly MyDistributedUpdater<List<IMyReplicable>, IMyReplicable> m_updateList;

        public MyReplicablesLinear(Thread mainThread) : base(mainThread)
        {
            this.m_roots = new HashSet<IMyReplicable>();
            this.m_updateList = new MyDistributedUpdater<List<IMyReplicable>, IMyReplicable>(60);
        }

        protected override void AddRoot(IMyReplicable replicable)
        {
            this.m_roots.Add(replicable);
            this.m_updateList.List.Add(replicable);
        }

        public override void GetReplicablesInBox(BoundingBoxD aabb, List<IMyReplicable> list)
        {
            throw new NotImplementedException();
        }

        public override void IterateRoots(Action<IMyReplicable> p)
        {
            foreach (IMyReplicable replicable in this.m_roots)
            {
                p(replicable);
            }
        }

        protected override void RemoveRoot(IMyReplicable replicable)
        {
            if (this.m_roots.Contains(replicable))
            {
                this.m_roots.Remove(replicable);
                this.m_updateList.List.Remove(replicable);
            }
        }
    }
}

