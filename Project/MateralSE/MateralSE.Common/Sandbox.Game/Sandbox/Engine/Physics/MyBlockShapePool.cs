namespace Sandbox.Engine.Physics
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Game.Models;
    using VRage.Game.Voxels;
    using VRage.Utils;

    public class MyBlockShapePool
    {
        public const int PREALLOCATE_COUNT = 50;
        private const int MAX_CLONE_PER_FRAME = 3;
        private Dictionary<MyDefinitionId, Dictionary<string, ConcurrentQueue<HkdBreakableShape>>> m_pools = new Dictionary<MyDefinitionId, Dictionary<string, ConcurrentQueue<HkdBreakableShape>>>();
        private MyWorkTracker<MyDefinitionId, MyBreakableShapeCloneJob> m_tracker = new MyWorkTracker<MyDefinitionId, MyBreakableShapeCloneJob>(null);
        private FastResourceLock m_poolLock = new FastResourceLock();
        private int m_missing;
        private bool m_dequeuedThisFrame;

        public void AllocateForDefinition(string model, MyPhysicalModelDefinition definition, int count)
        {
            if (!string.IsNullOrEmpty(model))
            {
                MyModel modelOnlyData = MyModels.GetModelOnlyData(model);
                if (modelOnlyData.HavokBreakableShapes == null)
                {
                    MyDestructionData.Static.LoadModelDestruction(model, definition, modelOnlyData.BoundingBoxSize, true, false);
                }
                if ((modelOnlyData.HavokBreakableShapes != null) && (modelOnlyData.HavokBreakableShapes.Length != 0))
                {
                    ConcurrentQueue<HkdBreakableShape> queue;
                    using (this.m_poolLock.AcquireExclusiveUsing())
                    {
                        if (!this.m_pools.ContainsKey(definition.Id))
                        {
                            this.m_pools[definition.Id] = new Dictionary<string, ConcurrentQueue<HkdBreakableShape>>();
                        }
                        if (!this.m_pools[definition.Id].ContainsKey(model))
                        {
                            this.m_pools[definition.Id][model] = new ConcurrentQueue<HkdBreakableShape>();
                        }
                        queue = this.m_pools[definition.Id][model];
                    }
                    for (int i = 0; i < count; i++)
                    {
                        HkdBreakableShape item = modelOnlyData.HavokBreakableShapes[0].Clone();
                        queue.Enqueue(item);
                        if (i == 0)
                        {
                            HkMassProperties massProperties = new HkMassProperties();
                            item.BuildMassProperties(ref massProperties);
                            if (!massProperties.InertiaTensor.IsValid())
                            {
                                MyLog.Default.WriteLine($"Block with wrong destruction! (q.isOk): {definition.Model}");
                                return;
                            }
                        }
                    }
                }
            }
        }

        public void EnqueShape(string model, MyDefinitionId id, HkdBreakableShape shape)
        {
            using (this.m_poolLock.AcquireExclusiveUsing())
            {
                if (!this.m_pools.ContainsKey(id))
                {
                    this.m_pools[id] = new Dictionary<string, ConcurrentQueue<HkdBreakableShape>>();
                }
                if (!this.m_pools[id].ContainsKey(model))
                {
                    this.m_pools[id][model] = new ConcurrentQueue<HkdBreakableShape>();
                }
            }
            this.m_pools[id][model].Enqueue(shape);
            this.m_missing--;
        }

        public void EnqueShapes(string model, MyDefinitionId id, List<HkdBreakableShape> shapes)
        {
            using (this.m_poolLock.AcquireExclusiveUsing())
            {
                if (!this.m_pools.ContainsKey(id))
                {
                    this.m_pools[id] = new Dictionary<string, ConcurrentQueue<HkdBreakableShape>>();
                }
                if (!this.m_pools[id].ContainsKey(model))
                {
                    this.m_pools[id][model] = new ConcurrentQueue<HkdBreakableShape>();
                }
            }
            foreach (HkdBreakableShape shape in shapes)
            {
                this.m_pools[id][model].Enqueue(shape);
            }
            this.m_missing -= shapes.Count;
        }

        internal void Free()
        {
            HashSet<IntPtr> set = new HashSet<IntPtr>();
            this.m_tracker.CancelAll();
            using (this.m_poolLock.AcquireExclusiveUsing())
            {
                Dictionary<MyDefinitionId, Dictionary<string, ConcurrentQueue<HkdBreakableShape>>>.ValueCollection.Enumerator enumerator;
                Dictionary<string, ConcurrentQueue<HkdBreakableShape>>.ValueCollection.Enumerator enumerator2;
                IEnumerator<HkdBreakableShape> enumerator3;
                using (enumerator = this.m_pools.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        using (enumerator2 = enumerator.Current.Values.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                enumerator3 = enumerator2.Current.GetEnumerator();
                                try
                                {
                                    while (enumerator3.MoveNext())
                                    {
                                        HkdBreakableShape current = enumerator3.Current;
                                        if (set.Contains(current.NativeDebug))
                                        {
                                            string msg = "Shape " + current.Name + " was referenced twice in the pool!";
                                            MyLog.Default.WriteLine(msg);
                                        }
                                        set.Add(current.NativeDebug);
                                    }
                                }
                                finally
                                {
                                    if (enumerator3 == null)
                                    {
                                        continue;
                                    }
                                    enumerator3.Dispose();
                                }
                            }
                        }
                    }
                }
                using (enumerator = this.m_pools.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        using (enumerator2 = enumerator.Current.Values.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                enumerator3 = enumerator2.Current.GetEnumerator();
                                try
                                {
                                    while (enumerator3.MoveNext())
                                    {
                                        HkdBreakableShape current = enumerator3.Current;
                                        IntPtr nativeDebug = current.NativeDebug;
                                        current.RemoveReference();
                                    }
                                }
                                finally
                                {
                                    if (enumerator3 == null)
                                    {
                                        continue;
                                    }
                                    enumerator3.Dispose();
                                }
                            }
                        }
                    }
                }
                this.m_pools.Clear();
            }
        }

        public HkdBreakableShape GetBreakableShape(string model, MyCubeBlockDefinition block)
        {
            HkdBreakableShape shape;
            this.m_dequeuedThisFrame = true;
            if (!block.Public || MyFakes.LAZY_LOAD_DESTRUCTION)
            {
                using (this.m_poolLock.AcquireExclusiveUsing())
                {
                    if (!this.m_pools.ContainsKey(block.Id))
                    {
                        this.m_pools[block.Id] = new Dictionary<string, ConcurrentQueue<HkdBreakableShape>>();
                    }
                    if (!this.m_pools[block.Id].ContainsKey(model))
                    {
                        this.m_pools[block.Id][model] = new ConcurrentQueue<HkdBreakableShape>();
                    }
                }
            }
            ConcurrentQueue<HkdBreakableShape> local1 = this.m_pools[block.Id][model];
            if (local1.Count == 0)
            {
                this.AllocateForDefinition(model, block, 1);
            }
            else
            {
                this.m_missing++;
            }
            local1.TryDequeue(out shape);
            return shape;
        }

        public void Preallocate()
        {
            MySandboxGame.Log.WriteLine("Preallocate shape pool - START");
            foreach (string str in MyDefinitionManager.Static.GetDefinitionPairNames())
            {
                MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(str);
                if ((definitionGroup.Large != null) && definitionGroup.Large.Public)
                {
                    MyCubeBlockDefinition large = definitionGroup.Large;
                    this.AllocateForDefinition(definitionGroup.Large.Model, large, 50);
                    foreach (MyCubeBlockDefinition.BuildProgressModel model in definitionGroup.Large.BuildProgressModels)
                    {
                        this.AllocateForDefinition(model.File, large, 50);
                    }
                }
                if ((definitionGroup.Small != null) && definitionGroup.Small.Public)
                {
                    this.AllocateForDefinition(definitionGroup.Small.Model, definitionGroup.Small, 50);
                    foreach (MyCubeBlockDefinition.BuildProgressModel model2 in definitionGroup.Small.BuildProgressModels)
                    {
                        this.AllocateForDefinition(model2.File, definitionGroup.Small, 50);
                    }
                }
            }
            MySandboxGame.Log.WriteLine("Preallocate shape pool - END");
        }

        public void RefillPools()
        {
            if (this.m_missing != 0)
            {
                if (this.m_dequeuedThisFrame && !MyFakes.CLONE_SHAPES_ON_WORKER)
                {
                    this.m_dequeuedThisFrame = false;
                }
                else
                {
                    int num = 0;
                    if (MyFakes.CLONE_SHAPES_ON_WORKER)
                    {
                        this.StartJobs();
                    }
                    else
                    {
                        using (this.m_poolLock.AcquireSharedUsing())
                        {
                            foreach (KeyValuePair<MyDefinitionId, Dictionary<string, ConcurrentQueue<HkdBreakableShape>>> pair in this.m_pools)
                            {
                                foreach (KeyValuePair<string, ConcurrentQueue<HkdBreakableShape>> pair2 in pair.Value)
                                {
                                    if (pair.Value.Count < 50)
                                    {
                                        MyCubeBlockDefinition definition;
                                        MyDefinitionManager.Static.TryGetDefinition<MyCubeBlockDefinition>(pair.Key, out definition);
                                        int count = Math.Min((int) (50 - pair.Value.Count), (int) (3 - num));
                                        this.AllocateForDefinition(pair2.Key, definition, count);
                                        num += count;
                                    }
                                    if (num >= 3)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    this.m_missing -= num;
                    num = 0;
                }
            }
        }

        private void StartJobs()
        {
            using (this.m_poolLock.AcquireSharedUsing())
            {
                foreach (KeyValuePair<MyDefinitionId, Dictionary<string, ConcurrentQueue<HkdBreakableShape>>> pair in this.m_pools)
                {
                    foreach (KeyValuePair<string, ConcurrentQueue<HkdBreakableShape>> pair2 in pair.Value)
                    {
                        if (pair2.Value.Count >= 50)
                        {
                            continue;
                        }
                        if (!this.m_tracker.Exists(pair.Key))
                        {
                            MyPhysicalModelDefinition definition;
                            MyDefinitionManager.Static.TryGetDefinition<MyPhysicalModelDefinition>(pair.Key, out definition);
                            MyModel modelOnlyData = MyModels.GetModelOnlyData(definition.Model);
                            if (modelOnlyData.HavokBreakableShapes != null)
                            {
                                MyBreakableShapeCloneJob.Args args = new MyBreakableShapeCloneJob.Args {
                                    Model = pair2.Key,
                                    DefId = pair.Key,
                                    ShapeToClone = modelOnlyData.HavokBreakableShapes[0],
                                    Count = 50 - pair.Value.Count,
                                    Tracker = this.m_tracker
                                };
                                MyBreakableShapeCloneJob.Start(args);
                            }
                        }
                    }
                }
            }
        }
    }
}

