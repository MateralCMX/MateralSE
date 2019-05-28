namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyTimedItemCache
    {
        private readonly HashSet<long> m_eventHappenedHere = new HashSet<long>();
        private readonly Queue<KeyValuePair<long, int>> m_eventQueue = new Queue<KeyValuePair<long, int>>();

        public MyTimedItemCache(int eventTimeoutMs)
        {
            this.EventTimeoutMs = eventTimeoutMs;
        }

        public bool IsItemPresent(long itemHashCode, int currentTimeMs, bool autoinsert = true)
        {
            while (true)
            {
                if (this.m_eventQueue.Count > 0)
                {
                    KeyValuePair<long, int> pair = this.m_eventQueue.Peek();
                    if (currentTimeMs > pair.Value)
                    {
                        this.m_eventHappenedHere.Remove(this.m_eventQueue.Dequeue().Key);
                        continue;
                    }
                }
                if (this.m_eventHappenedHere.Contains(itemHashCode))
                {
                    return true;
                }
                if (autoinsert)
                {
                    this.m_eventHappenedHere.Add(itemHashCode);
                    this.m_eventQueue.Enqueue(new KeyValuePair<long, int>(itemHashCode, currentTimeMs + this.EventTimeoutMs));
                }
                return false;
            }
        }

        public unsafe bool IsPlaceUsed(Vector3D position, double eventSpaceMapping, int currentTimeMs, bool autoinsert = true)
        {
            Vector3D vectord = position * eventSpaceMapping;
            Vector3D* vectordPtr1 = (Vector3D*) ref vectord;
            vectordPtr1->X = Math.Floor(vectord.X);
            Vector3D* vectordPtr2 = (Vector3D*) ref vectord;
            vectordPtr2->Y = Math.Floor(vectord.Y);
            Vector3D* vectordPtr3 = (Vector3D*) ref vectord;
            vectordPtr3->Z = Math.Floor(vectord.Z);
            long hash = vectord.GetHash();
            return this.IsItemPresent(hash, currentTimeMs, autoinsert);
        }

        public int EventTimeoutMs { get; set; }
    }
}

