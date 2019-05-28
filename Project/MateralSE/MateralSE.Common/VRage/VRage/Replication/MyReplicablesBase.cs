namespace VRage.Replication
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Network;
    using VRageMath;

    public abstract class MyReplicablesBase
    {
        private static readonly HashSet<IMyReplicable> m_empty = new HashSet<IMyReplicable>();
        private readonly Stack<HashSet<IMyReplicable>> m_hashSetPool = new Stack<HashSet<IMyReplicable>>();
        private readonly ConcurrentDictionary<IMyReplicable, HashSet<IMyReplicable>> m_parentToChildren = new ConcurrentDictionary<IMyReplicable, HashSet<IMyReplicable>>();
        private readonly ConcurrentDictionary<IMyReplicable, IMyReplicable> m_childToParent = new ConcurrentDictionary<IMyReplicable, IMyReplicable>();
        private readonly Thread m_mainThread;

        protected MyReplicablesBase(Thread mainThread)
        {
            this.m_mainThread = mainThread;
        }

        public void Add(IMyReplicable replicable, out IMyReplicable parent)
        {
            if (replicable.HasToBeChild && TryGetParent(replicable, out parent))
            {
                this.AddChild(replicable, parent);
            }
            else if (replicable.HasToBeChild)
            {
                parent = null;
            }
            else
            {
                parent = null;
                this.AddRoot(replicable);
            }
        }

        protected virtual void AddChild(IMyReplicable replicable, IMyReplicable parent)
        {
            HashSet<IMyReplicable> set;
            if (!this.m_parentToChildren.TryGetValue(parent, out set))
            {
                set = this.Obtain();
                this.m_parentToChildren[parent] = set;
            }
            set.Add(replicable);
            this.m_childToParent[replicable] = parent;
        }

        protected abstract void AddRoot(IMyReplicable replicable);
        [Conditional("DEBUG")]
        protected void CheckThread()
        {
        }

        public void GetAllChildren(IMyReplicable replicable, List<IMyReplicable> resultList)
        {
            foreach (IMyReplicable replicable2 in this.GetChildren(replicable))
            {
                resultList.Add(replicable2);
                this.GetAllChildren(replicable2, resultList);
            }
        }

        public HashSetReader<IMyReplicable> GetChildren(IMyReplicable replicable) => 
            this.m_parentToChildren.GetValueOrDefault<IMyReplicable, HashSet<IMyReplicable>>(replicable, m_empty);

        public abstract void GetReplicablesInBox(BoundingBoxD aabb, List<IMyReplicable> list);
        public abstract void IterateRoots(Action<IMyReplicable> p);
        private HashSet<IMyReplicable> Obtain() => 
            ((this.m_hashSetPool.Count > 0) ? this.m_hashSetPool.Pop() : new HashSet<IMyReplicable>());

        public void Refresh(IMyReplicable replicable)
        {
            IMyReplicable replicable2;
            if (!replicable.HasToBeChild || !TryGetParent(replicable, out replicable2))
            {
                IMyReplicable replicable4;
                if (this.m_childToParent.TryGetValue(replicable, out replicable4))
                {
                    this.RemoveChild(replicable, replicable4);
                    this.AddRoot(replicable);
                }
            }
            else
            {
                IMyReplicable replicable3;
                if (!this.m_childToParent.TryGetValue(replicable, out replicable3))
                {
                    this.RemoveRoot(replicable);
                    this.AddChild(replicable, replicable2);
                }
                else if (!ReferenceEquals(replicable3, replicable2))
                {
                    this.RemoveChild(replicable, replicable3);
                    this.AddChild(replicable, replicable2);
                }
            }
        }

        private void Remove(IMyReplicable replicable)
        {
            IMyReplicable replicable2;
            if (this.m_childToParent.TryGetValue(replicable, out replicable2))
            {
                this.RemoveChild(replicable, replicable2);
            }
            this.RemoveRoot(replicable);
        }

        protected virtual void RemoveChild(IMyReplicable replicable, IMyReplicable parent)
        {
            this.m_childToParent.Remove<IMyReplicable, IMyReplicable>(replicable);
            HashSet<IMyReplicable> item = this.m_parentToChildren[parent];
            item.Remove(replicable);
            if (item.Count == 0)
            {
                this.m_parentToChildren.Remove<IMyReplicable, HashSet<IMyReplicable>>(parent);
                this.m_hashSetPool.Push(item);
            }
        }

        public void RemoveHierarchy(IMyReplicable replicable)
        {
            HashSet<IMyReplicable> set = this.m_parentToChildren.GetValueOrDefault<IMyReplicable, HashSet<IMyReplicable>>(replicable, m_empty);
            while (set.Count > 0)
            {
                HashSet<IMyReplicable>.Enumerator enumerator = set.GetEnumerator();
                enumerator.MoveNext();
                this.RemoveHierarchy(enumerator.Current);
            }
            this.Remove(replicable);
        }

        protected abstract void RemoveRoot(IMyReplicable replicable);
        private static bool TryGetParent(IMyReplicable replicable, out IMyReplicable parent)
        {
            parent = replicable.GetParent();
            return (parent != null);
        }
    }
}

