namespace Sandbox.Engine.Voxels.Storage
{
    using ParallelTasks;
    using Sandbox.Engine.Voxels;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game.Components;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyVoxelOperationsSessionComponent : MySessionComponentBase
    {
        private const int WaitForLazy = 300;
        public static MyVoxelOperationsSessionComponent Static;
        public bool ShouldWrite = true;
        public bool ShouldFlush = true;
        public static bool EnableCache;
        private volatile int m_scheduledCount;
        private int m_waitForFlush;
        private int m_waitForWrite;
        private readonly MyConcurrentHashSet<StorageData> m_storagesWithCache = new MyConcurrentHashSet<StorageData>();
        private Action<WorkData> m_writePendingCallback;
        private Action<WorkData> m_flushCachesCallback;

        public MyVoxelOperationsSessionComponent()
        {
            this.m_flushCachesCallback = new Action<WorkData>(this.FlushCaches);
            this.m_writePendingCallback = new Action<WorkData>(this.WritePending);
        }

        public void Add(MyStorageBase storage)
        {
            StorageData instance = new StorageData(storage);
            this.m_storagesWithCache.Add(instance);
        }

        public override void BeforeStart()
        {
            Static = this;
        }

        public void FlushCaches(WorkData data)
        {
            StorageData data1 = (StorageData) data;
            data1.Storage.CleanCachedChunks();
            data1.Scheduled = false;
            Interlocked.Decrement(ref this.m_scheduledCount);
        }

        public void Remove(MyStorageBase storage)
        {
            this.m_storagesWithCache.Remove(new StorageData(storage));
        }

        public override void UpdateAfterSimulation()
        {
            if (this.m_storagesWithCache.Count != this.m_scheduledCount)
            {
                this.m_waitForWrite++;
                if (this.m_waitForWrite > 10)
                {
                    this.m_waitForWrite = 0;
                }
                this.m_waitForFlush++;
                if ((this.m_waitForFlush >= 300) && this.ShouldFlush)
                {
                    this.m_waitForFlush = 0;
                    foreach (StorageData data in this.m_storagesWithCache)
                    {
                        if (data.Scheduled)
                        {
                            continue;
                        }
                        if (data.Storage.HasCachedChunks)
                        {
                            Interlocked.Increment(ref this.m_scheduledCount);
                            data.Scheduled = true;
                            Parallel.Start(this.m_flushCachesCallback, null, data);
                        }
                    }
                }
                else if ((this.m_waitForWrite == 0) && this.ShouldWrite)
                {
                    foreach (StorageData data2 in this.m_storagesWithCache)
                    {
                        if (data2.Scheduled)
                        {
                            continue;
                        }
                        if (data2.Storage.HasPendingWrites)
                        {
                            Interlocked.Increment(ref this.m_scheduledCount);
                            data2.Scheduled = true;
                            Parallel.Start(this.m_writePendingCallback, null, data2);
                        }
                    }
                }
            }
        }

        public void WritePending(WorkData data)
        {
            StorageData data1 = (StorageData) data;
            data1.Storage.WritePending(false);
            data1.Scheduled = false;
            Interlocked.Decrement(ref this.m_scheduledCount);
        }

        public IEnumerable<MyStorageBase> QueuedStorages =>
            (from x in this.m_storagesWithCache select x.Storage);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyVoxelOperationsSessionComponent.<>c <>9 = new MyVoxelOperationsSessionComponent.<>c();
            public static Func<MyVoxelOperationsSessionComponent.StorageData, MyStorageBase> <>9__19_0;

            internal MyStorageBase <get_QueuedStorages>b__19_0(MyVoxelOperationsSessionComponent.StorageData x) => 
                x.Storage;
        }

        private class StorageData : WorkData, IEquatable<MyVoxelOperationsSessionComponent.StorageData>
        {
            public readonly MyStorageBase Storage;
            public bool Scheduled;

            public StorageData(MyStorageBase storage)
            {
                this.Storage = storage;
            }

            public bool Equals(MyVoxelOperationsSessionComponent.StorageData other) => 
                ReferenceEquals(this.Storage, other.Storage);

            public override int GetHashCode() => 
                this.Storage.GetHashCode();
        }
    }
}

