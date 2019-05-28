namespace Sandbox.Engine.Utils
{
    using Sandbox;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyLocalityGrouping
    {
        public GroupingMode Mode;
        private SortedSet<InstanceInfo> m_instances = new SortedSet<InstanceInfo>(new InstanceInfoComparer());

        public MyLocalityGrouping(GroupingMode mode)
        {
            this.Mode = mode;
        }

        public bool AddInstance(TimeSpan lifeTime, Vector3 position, float radius, bool removeOld = true)
        {
            if (removeOld)
            {
                this.RemoveOld();
            }
            using (SortedSet<InstanceInfo>.Enumerator enumerator = this.m_instances.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    InstanceInfo current = enumerator.Current;
                    float num = (this.Mode == GroupingMode.ContainsCenter) ? Math.Max(radius, current.Radius) : (radius + current.Radius);
                    if (Vector3.DistanceSquared(position, current.Position) < (num * num))
                    {
                        return false;
                    }
                }
            }
            InstanceInfo item = new InstanceInfo {
                EndTimeMs = this.TimeMs + ((int) lifeTime.TotalMilliseconds),
                Position = position,
                Radius = radius
            };
            this.m_instances.Add(item);
            return true;
        }

        public void Clear()
        {
            this.m_instances.Clear();
        }

        public void RemoveOld()
        {
            int timeMs = this.TimeMs;
            while ((this.m_instances.Count > 0) && (this.m_instances.Min.EndTimeMs < timeMs))
            {
                this.m_instances.Remove(this.m_instances.Min);
            }
        }

        private int TimeMs =>
            MySandboxGame.TotalGamePlayTimeInMilliseconds;

        public enum GroupingMode
        {
            ContainsCenter,
            Overlaps
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct InstanceInfo
        {
            public Vector3 Position;
            public float Radius;
            public int EndTimeMs;
        }

        private class InstanceInfoComparer : IComparer<MyLocalityGrouping.InstanceInfo>
        {
            public int Compare(MyLocalityGrouping.InstanceInfo x, MyLocalityGrouping.InstanceInfo y) => 
                (x.EndTimeMs - y.EndTimeMs);
        }
    }
}

