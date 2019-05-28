namespace VRage.Render.Scene
{
    using ParallelTasks;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Library.Utils;
    using VRage.Render.Scene.Components;
    using VRage.Utils;

    public class MyActorUpdater
    {
        private HashSet<MyActor> m_pendingUpdate = new HashSet<MyActor>();
        private HashSet<MyActor> m_pendingUpdateProcessed = new HashSet<MyActor>();
        private readonly HashSet<MyActor> m_alwaysUpdateActors = new HashSet<MyActor>();
        private readonly HashSet<MyActorComponent> m_alwaysUpdateComponents = new HashSet<MyActorComponent>();
        private readonly HashSet<MyActorComponent> m_alwaysUpdateComponentsParallel = new HashSet<MyActorComponent>();
        private readonly StateChangeCollector<MyActorComponent> m_alwaysUpdateComponentsParallelCollector;
        private bool m_isUpdatingParallel;
        [ThreadStatic]
        private static HashSet<MyActor> m_pendingUpdateCache;
        private readonly List<HashSet<MyActor>> m_pendingCaches = new List<HashSet<MyActor>>();
        private readonly List<MyDelayedCall> m_delayedCalls = new List<MyDelayedCall>();

        public MyActorUpdater()
        {
            this.m_alwaysUpdateComponentsParallelCollector = new StateChangeCollector<MyActorComponent>(this.m_alwaysUpdateComponentsParallel);
        }

        public void AddForParallelUpdate(MyActorComponent component)
        {
            if (this.m_isUpdatingParallel)
            {
                this.m_alwaysUpdateComponentsParallelCollector.StateChanged(component, true);
            }
            else
            {
                this.m_alwaysUpdateComponentsParallel.Add(component);
            }
        }

        public void AddToAlwaysUpdate(MyActorComponent component)
        {
            this.m_alwaysUpdateComponents.Add(component);
        }

        public void AddToAlwaysUpdate(MyActor actor)
        {
            this.m_alwaysUpdateActors.Add(actor);
        }

        public void AddToNextUpdate(MyActor actor)
        {
            if (!actor.AlwaysUpdate)
            {
                if (!this.m_isUpdatingParallel)
                {
                    this.m_pendingUpdate.Add(actor);
                }
                else
                {
                    HashSet<MyActor> pendingUpdateCache = m_pendingUpdateCache;
                    if (pendingUpdateCache == null)
                    {
                        m_pendingUpdateCache = pendingUpdateCache = new HashSet<MyActor>();
                        List<HashSet<MyActor>> pendingCaches = this.m_pendingCaches;
                        lock (pendingCaches)
                        {
                            this.m_pendingCaches.Add(pendingUpdateCache);
                        }
                    }
                    pendingUpdateCache.Add(actor);
                }
            }
        }

        public void CallIn(Action what, MyTimeSpan delay)
        {
            MyTimeSpan span = new MyTimeSpan(Stopwatch.GetTimestamp()) + delay;
            List<MyDelayedCall> delayedCalls = this.m_delayedCalls;
            lock (delayedCalls)
            {
                MyDelayedCall item = new MyDelayedCall {
                    Call = what,
                    CallTime = span
                };
                this.m_delayedCalls.Add(item);
            }
        }

        public void DestroyIn(MyActor actor, MyTimeSpan delay)
        {
            this.CallIn(delegate {
                if (!actor.IsDestroyed)
                {
                    actor.Destruct();
                }
            }, delay);
        }

        public void DestroyNextFrame(MyActor actor)
        {
            this.DestroyIn(actor, MyTimeSpan.Zero);
        }

        public void ForceDelayedCalls()
        {
            this.UpdateDelayedCalls(MyTimeSpan.MaxValue);
        }

        public void RemoveFromAlwaysUpdate(MyActorComponent component)
        {
            this.m_alwaysUpdateComponents.Remove(component);
        }

        public void RemoveFromAlwaysUpdate(MyActor actor)
        {
            this.m_alwaysUpdateActors.Remove(actor);
        }

        public void RemoveFromParallelUpdate(MyActorComponent component)
        {
            if (this.m_isUpdatingParallel)
            {
                this.m_alwaysUpdateComponentsParallelCollector.StateChanged(component, false);
            }
            else
            {
                this.m_alwaysUpdateComponentsParallel.Remove(component);
            }
        }

        public void RemoveFromUpdates(MyActor actor)
        {
            this.m_alwaysUpdateActors.Remove(actor);
            this.m_pendingUpdate.Remove(actor);
        }

        public void Update()
        {
            MyTimeSpan currentTime = new MyTimeSpan(Stopwatch.GetTimestamp());
            this.UpdateDelayedCalls(currentTime);
            this.m_isUpdatingParallel = true;
            WorkOptions? options = null;
            Parallel.ForEach<MyActorComponent>(this.m_alwaysUpdateComponentsParallel, c => c.OnUpdateBeforeDraw(), WorkPriority.VeryHigh, options, true);
            this.m_isUpdatingParallel = false;
            using (HashSet<MyActorComponent>.Enumerator enumerator = this.m_alwaysUpdateComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.OnUpdateBeforeDraw();
                }
            }
            this.m_alwaysUpdateComponentsParallelCollector.Commit();
            foreach (HashSet<MyActor> set in this.m_pendingCaches)
            {
                foreach (MyActor actor in set)
                {
                    this.AddToNextUpdate(actor);
                }
                set.Clear();
            }
            HashSet<MyActor>.Enumerator enumerator3 = this.m_alwaysUpdateActors.GetEnumerator();
            try
            {
                while (enumerator3.MoveNext())
                {
                    enumerator3.Current.UpdateBeforeDraw();
                }
            }
            finally
            {
                enumerator3.Dispose();
                goto TR_0009;
            }
        TR_0001:
            if (this.m_pendingUpdate.Count <= 0)
            {
                return;
            }
        TR_0009:
            while (true)
            {
                MyUtils.Swap<HashSet<MyActor>>(ref this.m_pendingUpdateProcessed, ref this.m_pendingUpdate);
                using (enumerator3 = this.m_pendingUpdateProcessed.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        enumerator3.Current.UpdateBeforeDraw();
                    }
                }
                this.m_pendingUpdateProcessed.Clear();
                break;
            }
            goto TR_0001;
        }

        private void UpdateDelayedCalls(MyTimeSpan currentTime)
        {
            List<MyDelayedCall> delayedCalls = this.m_delayedCalls;
            lock (delayedCalls)
            {
                int index = 0;
                while (index < this.m_delayedCalls.Count)
                {
                    MyDelayedCall call = this.m_delayedCalls[index];
                    if (currentTime < call.CallTime)
                    {
                        index++;
                        continue;
                    }
                    call.Call();
                    this.m_delayedCalls.RemoveAtFast<MyDelayedCall>(index);
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyActorUpdater.<>c <>9 = new MyActorUpdater.<>c();
            public static Action<MyActorComponent> <>9__13_0;

            internal void <Update>b__13_0(MyActorComponent c)
            {
                c.OnUpdateBeforeDraw();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyDelayedCall
        {
            public MyTimeSpan CallTime;
            public Action Call;
        }

        private class StateChangeCollector<T>
        {
            private bool m_stateChanged;
            private readonly HashSet<T> m_targetSet;
            private readonly ConcurrentDictionary<T, bool> m_changeLog;

            public StateChangeCollector(HashSet<T> mTargetSet)
            {
                this.m_changeLog = new ConcurrentDictionary<T, bool>();
                this.m_targetSet = mTargetSet;
            }

            public void Commit()
            {
                if (this.m_stateChanged)
                {
                    this.m_stateChanged = false;
                    foreach (KeyValuePair<T, bool> pair in this.m_changeLog)
                    {
                        this.m_changeLog.Remove<T, bool>(pair.Key);
                        if (pair.Value)
                        {
                            this.m_targetSet.Add(pair.Key);
                            continue;
                        }
                        this.m_targetSet.Remove(pair.Key);
                    }
                }
            }

            public void StateChanged(T instance, bool add)
            {
                if (this.m_targetSet.Contains(instance) != add)
                {
                    this.m_stateChanged = true;
                    this.m_changeLog[instance] = add;
                }
            }
        }
    }
}

