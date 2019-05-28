namespace Sandbox.Game.Replication.History
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRageMath;
    using VRageRender;

    public class MySnapshotHistory
    {
        public static readonly MyTimeSpan DELAY = MyTimeSpan.FromMilliseconds(100.0);
        private static readonly MyTimeSpan MAX_EXTRAPOLATION_DELAY = MyTimeSpan.FromMilliseconds(5000.0);
        private readonly List<MyItem> m_history = new List<MyItem>();

        public void Add(ref MySnapshot snapshot, MyTimeSpan timestamp)
        {
            if (this.FindExact(timestamp) == -1)
            {
                MyItem item = new MyItem {
                    Valid = true,
                    Type = SnapshotType.Exact,
                    Timestamp = timestamp,
                    Snapshot = snapshot
                };
                int index = this.FindIndex(timestamp);
                this.m_history.Insert(index, item);
            }
        }

        public void ApplyDelta(MyTimeSpan timestamp, ref MySnapshot delta)
        {
            for (int i = 0; i < this.m_history.Count; i++)
            {
                if (timestamp <= this.m_history[i].Timestamp)
                {
                    MyItem item = this.m_history[i];
                    item.Snapshot.Add(ref delta);
                    this.m_history[i] = item;
                }
            }
        }

        public unsafe void ApplyDeltaAngularVelocity(MyTimeSpan timestamp, Vector3 angularVelocityDelta)
        {
            for (int i = 0; i < this.m_history.Count; i++)
            {
                if (timestamp <= this.m_history[i].Timestamp)
                {
                    MyItem item = this.m_history[i];
                    Vector3* vectorPtr1 = (Vector3*) ref item.Snapshot.AngularVelocity;
                    vectorPtr1[0] += angularVelocityDelta;
                    this.m_history[i] = item;
                }
            }
        }

        public unsafe void ApplyDeltaLinearVelocity(MyTimeSpan timestamp, Vector3 linearVelocityDelta)
        {
            for (int i = 0; i < this.m_history.Count; i++)
            {
                if (timestamp <= this.m_history[i].Timestamp)
                {
                    MyItem item = this.m_history[i];
                    Vector3* vectorPtr1 = (Vector3*) ref item.Snapshot.LinearVelocity;
                    vectorPtr1[0] += linearVelocityDelta;
                    this.m_history[i] = item;
                }
            }
        }

        public unsafe void ApplyDeltaPosition(MyTimeSpan timestamp, Vector3D positionDelta)
        {
            for (int i = 0; i < this.m_history.Count; i++)
            {
                if (timestamp <= this.m_history[i].Timestamp)
                {
                    MyItem item = this.m_history[i];
                    Vector3D* vectordPtr1 = (Vector3D*) ref item.Snapshot.Position;
                    vectordPtr1[0] += positionDelta;
                    this.m_history[i] = item;
                }
            }
        }

        public void ApplyDeltaRotation(MyTimeSpan timestamp, Quaternion rotationDelta)
        {
            for (int i = 0; i < this.m_history.Count; i++)
            {
                if (timestamp <= this.m_history[i].Timestamp)
                {
                    MyItem item = this.m_history[i];
                    item.Snapshot.Rotation *= Quaternion.Inverse(rotationDelta);
                    item.Snapshot.Rotation.Normalize();
                    this.m_history[i] = item;
                }
            }
        }

        public void DebugDrawPos(MyEntity entity, MyTimeSpan timestamp, Color color)
        {
            int num = 0;
            MatrixD? nullable = null;
            while (num < this.m_history.Count)
            {
                if (timestamp <= this.m_history[num].Timestamp)
                {
                    MatrixD xd;
                    this.m_history[num].Snapshot.GetMatrix(entity, out xd, true, true);
                    MyRenderProxy.DebugDrawAxis(xd, 0.2f, false, false, false);
                    if (nullable != null)
                    {
                        Color? colorTo = null;
                        MyRenderProxy.DebugDrawArrow3D(nullable.Value.Translation, xd.Translation, color, colorTo, false, 0.1, null, 0.5f, false);
                    }
                    nullable = new MatrixD?(xd);
                }
                num++;
            }
        }

        public bool Empty() => 
            (this.m_history.Count == 0);

        private static float Factor(MyTimeSpan timestamp, ref MyItem item1, ref MyItem item2) => 
            (((float) (timestamp - item1.Timestamp).Ticks) / ((float) (item2.Timestamp - item1.Timestamp).Ticks));

        private int FindExact(MyTimeSpan timestamp)
        {
            int num = 0;
            while ((num < this.m_history.Count) && (timestamp != this.m_history[num].Timestamp))
            {
                num++;
            }
            return ((num >= this.m_history.Count) ? -1 : num);
        }

        private int FindIndex(MyTimeSpan timestamp)
        {
            int num = 0;
            while ((num < this.m_history.Count) && (timestamp > this.m_history[num].Timestamp))
            {
                num++;
            }
            return num;
        }

        public void Get(MyTimeSpan clientTimestamp, out MyItem item)
        {
            if (this.m_history.Count == 0)
            {
                item = new MyItem();
            }
            else
            {
                MyTimeSpan timestamp = clientTimestamp;
                int num = this.FindIndex(timestamp);
                if ((num < this.m_history.Count) && (timestamp == this.m_history[num].Timestamp))
                {
                    item = this.m_history[num];
                    item.Type = SnapshotType.Exact;
                }
                else if (num == 0)
                {
                    item = this.m_history[0];
                    if (timestamp == this.m_history[0].Timestamp)
                    {
                        item.Type = SnapshotType.Exact;
                    }
                    else if (timestamp < this.m_history[0].Timestamp)
                    {
                        item.Type = SnapshotType.TooNew;
                    }
                    else
                    {
                        item.Type = SnapshotType.TooOld;
                    }
                }
                else if ((num < this.m_history.Count) && (this.m_history.Count > 1))
                {
                    this.Lerp(timestamp, num - 1, out item);
                    item.Type = SnapshotType.Interpolation;
                }
                else if ((this.m_history.Count <= 1) || ((timestamp - this.m_history[this.m_history.Count - 1].Timestamp) >= MAX_EXTRAPOLATION_DELAY))
                {
                    item = this.m_history[this.m_history.Count - 1];
                    item.Type = SnapshotType.TooOld;
                }
                else if (!this.m_history[this.m_history.Count - 1].Snapshot.Active)
                {
                    item = this.m_history[this.m_history.Count - 1];
                    item.Timestamp = timestamp;
                }
                else
                {
                    this.Lerp(timestamp, this.m_history.Count - 2, out item);
                    item.Type = SnapshotType.Extrapolation;
                }
            }
        }

        public void GetFirst(out MyItem item)
        {
            MyItem local1;
            if (this.m_history.Count > 0)
            {
                local1 = this.m_history[0];
            }
            else
            {
                local1 = new MyItem();
            }
            item = local1;
        }

        public void GetItem(MyTimeSpan clientTimestamp, out MyItem item)
        {
            if (this.m_history.Count > 0)
            {
                int num = this.FindIndex(clientTimestamp) - 1;
                if ((num >= 0) && (num < this.m_history.Count))
                {
                    item = this.m_history[num];
                    return;
                }
            }
            item = new MyItem();
        }

        public bool GetItems(int index, out MyItem item1, out MyItem item2)
        {
            item1 = this.m_history[index];
            item2 = this.m_history[index + 1];
            if ((item1.Snapshot.ParentId != item2.Snapshot.ParentId) || (item1.Snapshot.InheritRotation != item2.Snapshot.InheritRotation))
            {
                if (this.m_history.Count < (index + 2))
                {
                    index++;
                    item1 = item2;
                    item2 = this.m_history[index + 1];
                }
                else if (index > 0)
                {
                    index--;
                    item2 = item1;
                    item1 = this.m_history[index];
                }
            }
            return ((item1.Snapshot.ParentId == item2.Snapshot.ParentId) && (item1.Snapshot.InheritRotation == item2.Snapshot.InheritRotation));
        }

        public void GetLast(out MyItem item, int index = 0)
        {
            MyItem item1;
            if (this.m_history.Count >= (index + 1))
            {
                item1 = this.m_history[(this.m_history.Count - index) - 1];
            }
            else
            {
                item1 = new MyItem();
            }
            item = item1;
        }

        public long GetLastParentId() => 
            this.m_history[this.m_history.Count - 1].Snapshot.ParentId;

        public MyTimeSpan GetLastTimestamp() => 
            this.m_history[this.m_history.Count - 1].Timestamp;

        public bool IsLastActive() => 
            ((this.m_history.Count >= 1) && this.m_history[this.m_history.Count - 1].Snapshot.Active);

        private void Lerp(MyTimeSpan timestamp, int index, out MyItem item)
        {
            MyItem item2;
            MyItem item3;
            if (this.GetItems(index, out item2, out item3))
            {
                Lerp(timestamp, ref item2, ref item3, out item);
            }
            else
            {
                MyItem item4 = new MyItem {
                    Valid = false
                };
                item = item4;
            }
        }

        public static void Lerp(MyTimeSpan timestamp, ref MyItem item1, ref MyItem item2, out MyItem item)
        {
            float factor = Factor(timestamp, ref item1, ref item2);
            MyItem item3 = new MyItem {
                Valid = true,
                Timestamp = timestamp
            };
            item = item3;
            item1.Snapshot.Lerp(ref item2.Snapshot, factor, out item.Snapshot);
        }

        public void Prune(MyTimeSpan clientTimestamp, MyTimeSpan delay, int leaveCount = 2)
        {
            MyTimeSpan timestamp = clientTimestamp - delay;
            int num = this.FindIndex(timestamp);
            this.m_history.RemoveRange(0, Math.Max(0, num - leaveCount));
        }

        public void PruneTooOld(MyTimeSpan clientTimestamp)
        {
            this.Prune(clientTimestamp, MAX_EXTRAPOLATION_DELAY, 2);
        }

        public void Reset()
        {
            this.m_history.Clear();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.m_history.Count; i++)
            {
                object[] objArray1 = new object[4];
                objArray1[0] = this.m_history[i].Timestamp;
                objArray1[1] = " (";
                MyItem item = this.m_history[i];
                objArray1[2] = item.Snapshot.Position.ToString("N3");
                objArray1[3] = ") ";
                builder.Append(string.Concat(objArray1));
            }
            return builder.ToString();
        }

        public string ToStringRotation()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.m_history.Count; i++)
            {
                object[] objArray1 = new object[4];
                objArray1[0] = this.m_history[i].Timestamp;
                objArray1[1] = " (";
                MyItem item = this.m_history[i];
                objArray1[2] = item.Snapshot.Rotation.ToStringAxisAngle("N3");
                objArray1[3] = ") ";
                builder.Append(string.Concat(objArray1));
            }
            return builder.ToString();
        }

        public string ToStringTimestamps()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.m_history.Count; i++)
            {
                builder.Append(this.m_history[i].Timestamp + " ");
            }
            return builder.ToString();
        }

        public int Count =>
            this.m_history.Count;

        [StructLayout(LayoutKind.Sequential)]
        public struct MyItem
        {
            public bool Valid;
            public MySnapshotHistory.SnapshotType Type;
            public MyTimeSpan Timestamp;
            public MySnapshot Snapshot;
            public override string ToString() => 
                ("Item timestamp: " + this.Timestamp);
        }

        public enum SnapshotType
        {
            Exact,
            TooNew,
            Interpolation,
            Extrapolation,
            TooOld,
            Reset
        }
    }
}

