namespace Sandbox.Game.Replication.History
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Blocks;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.Profiler;
    using VRageMath;
    using VRageRender;

    public class MyAnimatedSnapshotSync : IMySnapshotSync
    {
        private readonly MySnapshotHistory m_history = new MySnapshotHistory();
        private MyTimeSpan m_safeMovementCounter;
        private Vector3 m_lastVelocity;
        private readonly MyEntity m_entity;
        private Vector3D m_lastPos;
        private bool m_deactivated;
        private bool m_wasExtrapolating;
        private MyTimeSpan m_lastTimeDelta;
        private MyTimeSpan m_lastTime;
        public static MyTimeSpan TimeShift = MyTimeSpan.FromMilliseconds(64.0);
        private int m_invalidParentCounter;
        private readonly List<MyBlend> m_blends = new List<MyBlend>();
        private readonly List<MyBlend> m_blendsToRemove = new List<MyBlend>();

        public MyAnimatedSnapshotSync(MyEntity entity)
        {
            this.m_entity = entity;
        }

        private unsafe void BlendExtrapolation(ref MySnapshotHistory.MyItem item)
        {
            this.m_blendsToRemove.Clear();
            MySnapshot ss = new MySnapshot();
            bool flag = true;
            float factor = 1f;
            foreach (MyBlend blend in this.m_blends)
            {
                MySnapshotHistory.MyItem item4;
                MyTimeSpan span = item.Timestamp - blend.TimeStart;
                if ((span < MyTimeSpan.Zero) || (span >= blend.Duration))
                {
                    this.m_blendsToRemove.Add(blend);
                    continue;
                }
                MySnapshotHistory.MyItem item2 = blend.Item1;
                MySnapshotHistory.MyItem item3 = blend.Item2;
                MySnapshotHistory.Lerp(item.Timestamp, ref item2, ref item3, out item4);
                if (flag)
                {
                    ss = item4.Snapshot;
                }
                else
                {
                    MySnapshot* snapshotPtr1 = (MySnapshot*) ref ss;
                    item4.Snapshot.Lerp(ref (MySnapshot) ref snapshotPtr1, factor, out ss);
                }
                if (ss.ParentId == -1L)
                {
                    this.m_blendsToRemove.Add(blend);
                    flag = true;
                    break;
                }
                factor = 1f - ((float) (span.Seconds / blend.Duration.Seconds));
                flag = false;
            }
            if (!flag)
            {
                MySnapshot* snapshotPtr2 = (MySnapshot*) ref ss;
                item.Snapshot.Lerp(ref (MySnapshot) ref snapshotPtr2, factor, out ss);
                if (ss.ParentId != -1L)
                {
                    item.Snapshot = ss;
                }
            }
            foreach (MyBlend blend2 in this.m_blendsToRemove)
            {
                this.m_blends.Remove(blend2);
            }
        }

        public void Destroy()
        {
            this.Reset(false);
        }

        public void Read(ref MySnapshot item, MyTimeSpan timeStamp)
        {
            if (this.m_wasExtrapolating)
            {
                MyTimeSpan lastTimeDelta = this.m_lastTimeDelta;
                if (this.m_blends.Count > 0)
                {
                    lastTimeDelta = this.m_blends[this.m_blends.Count - 1].Duration - (this.m_lastTime - this.m_blends[this.m_blends.Count - 1].TimeStart);
                    if (lastTimeDelta < this.m_lastTimeDelta)
                    {
                        lastTimeDelta = this.m_lastTimeDelta;
                    }
                }
                MyBlend blend = new MyBlend {
                    TimeStart = this.m_lastTime,
                    Duration = lastTimeDelta
                };
                if (this.m_history.GetItems(this.m_history.Count - 2, out blend.Item1, out blend.Item2))
                {
                    this.m_blends.Add(blend);
                }
                this.m_wasExtrapolating = false;
            }
            this.m_history.Add(ref item, timeStamp);
            this.m_history.PruneTooOld(timeStamp - TimeShift);
        }

        public void Reset(bool reinit = false)
        {
            if (reinit)
            {
                this.m_history.Reset();
                this.m_blends.Clear();
            }
            this.m_deactivated = false;
        }

        public long Update(MyTimeSpan clientTimestamp, MySnapshotSyncSetup setup)
        {
            MySnapshotHistory.MyItem item;
            bool flag;
            int num1;
            int num2;
            int num3;
            if ((this.m_deactivated && !this.m_history.IsLastActive()) || MyFakes.MULTIPLAYER_SKIP_ANIMATION)
            {
                return -1L;
            }
            this.m_deactivated = false;
            MyTimeSpan span = clientTimestamp - TimeShift;
            this.m_history.Get(span, out item);
            this.m_history.PruneTooOld(span);
            if ((item.Valid && !item.Snapshot.Active) || this.m_history.Empty())
            {
                this.m_deactivated = true;
            }
            MyEntity parent = MySnapshot.GetParent(this.m_entity, out flag);
            if (setup.IgnoreParentId || !item.Valid)
            {
                num1 = 1;
            }
            else if ((parent != null) || (item.Snapshot.ParentId != 0))
            {
                num1 = (parent == null) ? 0 : ((int) (item.Snapshot.ParentId == parent.EntityId));
            }
            else
            {
                num1 = 1;
            }
            bool flag2 = (bool) num2;
            this.m_invalidParentCounter = flag2 ? 0 : (this.m_invalidParentCounter + 1);
            if (!item.Valid || (item.Type == MySnapshotHistory.SnapshotType.TooNew))
            {
                num3 = 0;
            }
            else
            {
                num3 = (int) (item.Type != MySnapshotHistory.SnapshotType.TooOld);
            }
            bool flag3 = ((bool) num3) & flag2;
            if (flag3)
            {
                if (MyFakes.MULTIPLAYER_EXTRAPOLATION_SMOOTHING && setup.ExtrapolationSmoothing)
                {
                    this.m_wasExtrapolating = item.Type == MySnapshotHistory.SnapshotType.Extrapolation;
                    this.m_lastTimeDelta = span - this.m_history.GetLastTimestamp();
                    this.m_lastTime = item.Timestamp;
                }
                if (item.Snapshot.Active && (this.m_blends.Count > 0))
                {
                    this.BlendExtrapolation(ref item);
                }
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_NETWORK_SYNC)
                {
                    MyRenderProxy.DebugDrawAABB(this.m_entity.PositionComp.WorldAABB, Color.White, 1f, 1f, true, false, false);
                    if (!(this.m_entity is MyWheel))
                    {
                        MatrixD xd;
                        item.Snapshot.GetMatrix(this.m_entity, out xd, true, true);
                        double milliseconds = (this.m_history.GetLastTimestamp() - (clientTimestamp - TimeShift)).Milliseconds;
                        MyRenderProxy.DebugDrawSphere(xd.Translation, (float) Math.Abs((double) (milliseconds / 32.0)), (milliseconds < 0.0) ? Color.Red : Color.Green, 1f, false, false, true, false);
                        MyRenderProxy.DebugDrawAxis(xd, 1f, false, false, false);
                        if (parent != null)
                        {
                            Color? colorTo = null;
                            MyRenderProxy.DebugDrawArrow3D(xd.Translation, parent.WorldMatrix.Translation, Color.Blue, colorTo, false, 0.1, null, 0.5f, false);
                        }
                    }
                }
                MySnapshotCache.Add(this.m_entity, ref item.Snapshot, setup, item.Type == MySnapshotHistory.SnapshotType.Reset);
            }
            if (MySnapshotCache.DEBUG_ENTITY_ID == this.m_entity.EntityId)
            {
                MyStatsGraph.ProfileAdvanced(true);
                MyStatsGraph.Begin("Animation", 0x7fffffff, "Update", 0x5c, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyAnimatedSnapshotSync.cs");
                MyStatsGraph.CustomTime("applySnapshot", flag3 ? 1f : 0.5f, "{0}", "Update", 0x5d, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyAnimatedSnapshotSync.cs");
                MyStatsGraph.CustomTime("extrapolating", (item.Type == MySnapshotHistory.SnapshotType.Extrapolation) ? 1f : 0.5f, "{0}", "Update", 0x5e, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyAnimatedSnapshotSync.cs");
                MyStatsGraph.CustomTime("blendsCount", (float) this.m_blends.Count, "{0}", "Update", 0x5f, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyAnimatedSnapshotSync.cs");
                MyStatsGraph.End(new float?((float) 1), 1f, "{0}", "{0} B", null, "Update", 0x60, @"E:\Repo1\Sources\Sandbox.Game\Game\Replication\History\MyAnimatedSnapshotSync.cs");
                MyStatsGraph.ProfileAdvanced(false);
            }
            return (item.Valid ? item.Snapshot.ParentId : -1L);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyBlend
        {
            public MySnapshotHistory.MyItem Item1;
            public MySnapshotHistory.MyItem Item2;
            public MyTimeSpan Duration;
            public MyTimeSpan TimeStart;
        }
    }
}

