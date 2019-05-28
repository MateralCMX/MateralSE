namespace VRage.Replication
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using VRage.Network;
    using VRageMath;

    public class MyReplicablesHierarchy : MyReplicablesBase
    {
        public MyReplicablesHierarchy(Thread mainThread) : base(mainThread)
        {
        }

        protected override void AddRoot(IMyReplicable replicable)
        {
        }

        public override void GetReplicablesInBox(BoundingBoxD aabb, List<IMyReplicable> list)
        {
        }

        public override void IterateRoots(Action<IMyReplicable> p)
        {
        }

        protected override void RemoveRoot(IMyReplicable replicable)
        {
        }
    }
}

