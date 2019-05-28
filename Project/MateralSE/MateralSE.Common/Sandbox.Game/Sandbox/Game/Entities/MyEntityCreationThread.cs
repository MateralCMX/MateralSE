namespace Sandbox.Game.Entities
{
    using Havok;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class MyEntityCreationThread : IDisposable
    {
        private MyConcurrentQueue<Item> m_jobQueue = new MyConcurrentQueue<Item>(0x10);
        private MyConcurrentQueue<Item> m_resultQueue = new MyConcurrentQueue<Item>(0x10);
        private ConcurrentCachingHashSet<Item> m_waitingItems = new ConcurrentCachingHashSet<Item>();
        private AutoResetEvent m_event = new AutoResetEvent(false);
        private Thread m_thread;
        private bool m_exitting;

        public MyEntityCreationThread()
        {
            RuntimeHelpers.RunClassConstructor(typeof(MyEntityIdentifier).TypeHandle);
            this.m_thread = new Thread(new ThreadStart(this.ThreadProc));
            this.m_thread.CurrentCulture = CultureInfo.InvariantCulture;
            this.m_thread.CurrentUICulture = CultureInfo.InvariantCulture;
            this.m_thread.Start();
        }

        public bool ConsumeResult(MyTimeSpan timestamp)
        {
            Item item;
            if (!this.m_resultQueue.TryDequeue(out item))
            {
                return false;
            }
            if (item.Result != null)
            {
                item.Result.DebugAsyncLoading = false;
            }
            bool flag = false;
            if (item.EntityIds != null)
            {
                while (true)
                {
                    if (!MyEntities.HasEntitiesToDelete())
                    {
                        List<IMyEntity>.Enumerator enumerator;
                        using (enumerator = item.EntityIds.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                IMyEntity entity;
                                if (!MyEntityIdentifier.TryGetEntity(enumerator.Current.EntityId, out entity, false))
                                {
                                    continue;
                                }
                                flag = true;
                            }
                        }
                        if (!flag)
                        {
                            using (enumerator = item.EntityIds.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    MyEntityIdentifier.AddEntityWithId(enumerator.Current);
                                }
                            }
                        }
                        item.EntityIds.Clear();
                        break;
                    }
                    MyEntities.DeleteRememberedEntities();
                }
            }
            if (flag)
            {
                if (item.DoneHandler != null)
                {
                    item.DoneHandler(null);
                }
            }
            else
            {
                if (item.AddToScene)
                {
                    MyEntities.Add(item.Result, item.InScene);
                }
                if (item.DoneHandler != null)
                {
                    item.DoneHandler(item.Result);
                }
            }
            return true;
        }

        private bool ConsumeWork(out Item item)
        {
            if (this.m_jobQueue.Count == 0)
            {
                this.m_event.WaitOne();
            }
            return this.m_jobQueue.TryDequeue(out item);
        }

        public void Dispose()
        {
            this.m_exitting = true;
            this.m_event.Set();
            this.m_thread.Join();
        }

        public void ReleaseWaiting(byte index, Dictionary<long, MatrixD> matrices)
        {
            Item item = new Item {
                ReleaseMatrices = matrices,
                WaitGroup = index
            };
            this.SubmitWork(item);
        }

        private void SubmitWork(Item item)
        {
            this.m_jobQueue.Enqueue(item);
            this.m_event.Set();
        }

        public void SubmitWork(MyObjectBuilder_EntityBase objectBuilder, bool addToScene, Action<MyEntity> doneHandler, MyEntity entity = null, byte waitGroup = 0, double serializationTimestamp = 0.0, bool fadeIn = false)
        {
            Item item = new Item {
                ObjectBuilder = objectBuilder,
                AddToScene = addToScene,
                DoneHandler = doneHandler,
                Result = entity,
                WaitGroup = waitGroup,
                SerializationTimestamp = MyTimeSpan.FromMilliseconds(serializationTimestamp),
                FadeIn = fadeIn
            };
            this.SubmitWork(item);
        }

        private unsafe void ThreadProc()
        {
            Thread.CurrentThread.Name = "Entity creation thread";
            HkBaseSystem.InitThread("Entity creation thread");
            MyEntityIdentifier.InEntityCreationBlock = true;
            MyEntityIdentifier.InitPerThreadStorage(0x800);
            while (!this.m_exitting)
            {
                Item item;
                if (!this.ConsumeWork(out item))
                {
                    continue;
                }
                if (item.ReleaseMatrices != null)
                {
                    foreach (Item item2 in this.m_waitingItems)
                    {
                        if (item2.WaitGroup == item.WaitGroup)
                        {
                            MatrixD xd;
                            if (item.ReleaseMatrices.TryGetValue(item2.Result.EntityId, out xd))
                            {
                                item2.Result.PositionComp.WorldMatrix = xd;
                            }
                            this.m_waitingItems.Remove(item2, false);
                            this.m_resultQueue.Enqueue(item2);
                        }
                    }
                    this.m_waitingItems.ApplyRemovals();
                    continue;
                }
                if (item.ObjectBuilder == null)
                {
                    if (item.Result != null)
                    {
                        item.Result.DebugAsyncLoading = true;
                    }
                    if (item.WaitGroup == 0)
                    {
                        this.m_resultQueue.Enqueue(item);
                    }
                    else
                    {
                        this.m_waitingItems.Add(item);
                        this.m_waitingItems.ApplyAdditions();
                    }
                    continue;
                }
                if (item.Result == null)
                {
                    Item* itemPtr1 = (Item*) ref item;
                    itemPtr1->Result = MyEntities.CreateFromObjectBuilderNoinit(item.ObjectBuilder);
                }
                Item* itemPtr2 = (Item*) ref item;
                itemPtr2->InScene = (item.ObjectBuilder.PersistentFlags & MyPersistentEntityFlags2.InScene) == MyPersistentEntityFlags2.InScene;
                item.ObjectBuilder.PersistentFlags &= ~MyPersistentEntityFlags2.InScene;
                item.Result.DebugAsyncLoading = true;
                MyEntities.InitEntity(item.ObjectBuilder, ref item.Result);
                if (item.Result != null)
                {
                    item.Result.Render.FadeIn = item.FadeIn;
                    item.EntityIds = new List<IMyEntity>();
                    MyEntityIdentifier.GetPerThreadEntities(item.EntityIds);
                    MyEntityIdentifier.ClearPerThreadEntities();
                    if (item.WaitGroup == 0)
                    {
                        this.m_resultQueue.Enqueue(item);
                    }
                    else
                    {
                        this.m_waitingItems.Add(item);
                        this.m_waitingItems.ApplyAdditions();
                    }
                }
            }
            MyEntityIdentifier.DestroyPerThreadStorage();
            HkBaseSystem.QuitThread();
        }

        public bool AnyResult =>
            (this.m_resultQueue.Count > 0);

        [StructLayout(LayoutKind.Sequential)]
        private struct Item
        {
            public MyObjectBuilder_EntityBase ObjectBuilder;
            public bool AddToScene;
            public bool InScene;
            public MyEntity Result;
            public Action<MyEntity> DoneHandler;
            public List<IMyEntity> EntityIds;
            public MyTimeSpan SerializationTimestamp;
            public byte WaitGroup;
            public Dictionary<long, MatrixD> ReleaseMatrices;
            public bool FadeIn;
        }
    }
}

