namespace VRage.Replication
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRageMath;

    public class MyReplicablesAABB : MyReplicablesBase
    {
        private readonly MyDynamicAABBTreeD m_rootsAABB;
        private readonly HashSet<IMyReplicable> m_roots;
        private readonly CacheList<IMyReplicable> m_tmp;
        private readonly Dictionary<IMyReplicable, int> m_proxies;

        public MyReplicablesAABB(Thread mainThread) : base(mainThread)
        {
            this.m_rootsAABB = new MyDynamicAABBTreeD(Vector3D.One, 1.0);
            this.m_roots = new HashSet<IMyReplicable>();
            this.m_tmp = new CacheList<IMyReplicable>();
            this.m_proxies = new Dictionary<IMyReplicable, int>();
        }

        protected override void AddChild(IMyReplicable replicable, IMyReplicable parent)
        {
            base.AddChild(replicable, parent);
            if (replicable.IsSpatial)
            {
                BoundingBoxD aABB = replicable.GetAABB();
                this.m_proxies.Add(replicable, this.m_rootsAABB.AddProxy(ref aABB, replicable, 0, true));
                replicable.OnAABBChanged = (Action<IMyReplicable>) Delegate.Combine(replicable.OnAABBChanged, new Action<IMyReplicable>(this.OnRootMoved));
            }
        }

        protected override void AddRoot(IMyReplicable replicable)
        {
            this.m_roots.Add(replicable);
            if (replicable.IsSpatial)
            {
                BoundingBoxD aABB = replicable.GetAABB();
                this.m_proxies.Add(replicable, this.m_rootsAABB.AddProxy(ref aABB, replicable, 0, true));
                replicable.OnAABBChanged = (Action<IMyReplicable>) Delegate.Combine(replicable.OnAABBChanged, new Action<IMyReplicable>(this.OnRootMoved));
            }
        }

        public override void GetReplicablesInBox(BoundingBoxD aabb, List<IMyReplicable> list)
        {
            this.m_rootsAABB.OverlapAllBoundingBox<IMyReplicable>(ref aabb, list, 0, true);
        }

        public override void IterateRoots(Action<IMyReplicable> p)
        {
            using (this.m_tmp)
            {
                this.m_rootsAABB.GetAll<IMyReplicable>(this.m_tmp, false, null);
                foreach (IMyReplicable replicable in this.m_tmp)
                {
                    p(replicable);
                }
            }
        }

        private void OnRootMoved(IMyReplicable replicable)
        {
            BoundingBoxD aABB = replicable.GetAABB();
            this.m_rootsAABB.MoveProxy(this.m_proxies[replicable], ref aABB, Vector3D.One);
        }

        protected override void RemoveChild(IMyReplicable replicable, IMyReplicable parent)
        {
            base.RemoveChild(replicable, parent);
            if (this.m_proxies.ContainsKey(replicable))
            {
                replicable.OnAABBChanged = (Action<IMyReplicable>) Delegate.Remove(replicable.OnAABBChanged, new Action<IMyReplicable>(this.OnRootMoved));
                this.m_rootsAABB.RemoveProxy(this.m_proxies[replicable]);
                this.m_proxies.Remove(replicable);
            }
        }

        protected override void RemoveRoot(IMyReplicable replicable)
        {
            if (this.m_roots.Contains(replicable))
            {
                this.m_roots.Remove(replicable);
                if (this.m_proxies.ContainsKey(replicable))
                {
                    replicable.OnAABBChanged = (Action<IMyReplicable>) Delegate.Remove(replicable.OnAABBChanged, new Action<IMyReplicable>(this.OnRootMoved));
                    this.m_rootsAABB.RemoveProxy(this.m_proxies[replicable]);
                    this.m_proxies.Remove(replicable);
                }
            }
        }
    }
}

