namespace VRage.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Collections;

    public class MyPathfindingData : HeapItem<float>
    {
        private object m_lockObject = new object();
        private Dictionary<Thread, long> threadedTimestamp = new Dictionary<Thread, long>();
        internal MyPathfindingData Predecessor;
        internal float PathLength;

        public MyPathfindingData(object parent)
        {
            this.Parent = parent;
        }

        public long GetTimestamp() => 
            this.Timestamp;

        public object Parent { get; private set; }

        internal long Timestamp
        {
            get
            {
                long num = 0L;
                object lockObject = this.m_lockObject;
                lock (lockObject)
                {
                    if (!this.threadedTimestamp.TryGetValue(Thread.CurrentThread, out num))
                    {
                        num = 0L;
                    }
                }
                return num;
            }
            set
            {
                object lockObject = this.m_lockObject;
                lock (lockObject)
                {
                    this.threadedTimestamp[Thread.CurrentThread] = value;
                }
            }
        }
    }
}

