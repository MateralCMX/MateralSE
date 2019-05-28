namespace Sandbox.Game.Replication
{
    using ParallelTasks;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.StateGroups;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Voxels;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    internal class MyVoxelReplicable : MyEntityReplicableBaseEvent<MyVoxelBase>, IMyStreamableReplicable
    {
        private List<MyEntity> m_entities;
        private Action<MyVoxelBase> m_loadingDoneHandler;
        private MyStreamingEntityStateGroup<MyVoxelReplicable> m_streamingGroup;

        public void CreateStreamingStateGroup()
        {
            this.m_streamingGroup = new MyStreamingEntityStateGroup<MyVoxelReplicable>(this, this);
        }

        private void GenerateFromObjectBuilder(MyObjectBuilder_EntityBase builder, out MyVoxelBase voxelMap)
        {
            voxelMap = null;
            try
            {
                MyObjectBuilder_VoxelMap map = builder as MyObjectBuilder_VoxelMap;
                if (map != null)
                {
                    if (!MyEntities.TryGetEntityById<MyVoxelBase>(builder.EntityId, out voxelMap, false))
                    {
                        base.TryRemoveExistingEntity(builder.EntityId);
                        voxelMap = (MyVoxelBase) MyEntities.CreateFromObjectBuilderNoinit(builder);
                        if (voxelMap != null)
                        {
                            voxelMap.Init(builder);
                            MyEntities.Add(voxelMap, true);
                        }
                    }
                    else
                    {
                        MyStorageBase base2 = MyStorageBase.Load(map.StorageName, true);
                        if (voxelMap is MyVoxelMap)
                        {
                            ((MyVoxelMap) voxelMap).Storage = base2;
                        }
                        else if (voxelMap is MyPlanet)
                        {
                            ((MyPlanet) voxelMap).Storage = base2;
                        }
                    }
                }
            }
            catch
            {
                voxelMap = null;
                MyObjectBuilder_VoxelMap map2 = (MyObjectBuilder_VoxelMap) builder;
                if (map2 != null)
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<string>(s => new Action<string>(MyMultiplayerBase.InvalidateVoxelCache), map2.StorageName, targetEndpoint, position);
                }
                MyLog.Default.WriteLine("Failed to load voxel from cache.");
            }
        }

        public override BoundingBoxD GetAABB()
        {
            BoundingBoxD worldAABB = base.Instance.PositionComp.WorldAABB;
            if (!(this.Voxel is MyPlanet))
            {
                worldAABB.Inflate((double) (this.Voxel.SizeInMetres.Length() * 50f));
            }
            return worldAABB;
        }

        public override void GetStateGroups(List<IMyStateGroup> resultList)
        {
            if (this.m_streamingGroup != null)
            {
                resultList.Add(this.m_streamingGroup);
            }
            base.GetStateGroups(resultList);
        }

        public IMyStateGroup GetStreamingStateGroup() => 
            this.m_streamingGroup;

        public void LoadCancel()
        {
            this.m_loadingDoneHandler(null);
        }

        public void LoadDone(BitStream stream)
        {
            this.OnLoad(stream, this.m_loadingDoneHandler);
        }

        public override void OnDestroyClient()
        {
            if (this.Voxel != null)
            {
                if (this.Voxel.Storage != null)
                {
                    byte[] buffer;
                    this.Voxel.Storage.Save(out buffer);
                    MyMultiplayer.Static.VoxelMapData.Write(this.Voxel.StorageName, buffer);
                }
                MyPlanet voxel = this.Voxel as MyPlanet;
                if (this.Voxel.Save && (voxel == null))
                {
                    this.Voxel.Close();
                }
            }
        }

        protected override void OnHook()
        {
            base.OnHook();
            if (Sync.IsServer)
            {
                MyReplicationServer server = MyMultiplayer.GetReplicationServer();
                if (server != null)
                {
                    this.Voxel.RangeChanged += (<p0>, <p1>, <p2>, <p3>) => server.InvalidateClientCache(this, this.Voxel.StorageName);
                }
            }
        }

        protected override void OnLoad(BitStream stream, Action<MyVoxelBase> loadingDoneHandler)
        {
            MyVoxelBase base2;
            bool flag = MySerializer.CreateAndRead<bool>(stream, null);
            bool flag2 = MySerializer.CreateAndRead<bool>(stream, null);
            bool flag3 = MySerializer.CreateAndRead<bool>(stream, null);
            byte[] memoryBuffer = null;
            string asteroid = null;
            if (MySerializer.CreateAndRead<bool>(stream, null))
            {
                memoryBuffer = MySerializer.CreateAndRead<byte[]>(stream, null);
            }
            else if (flag)
            {
                asteroid = MySerializer.CreateAndRead<string>(stream, null);
            }
            string[] textArray1 = new string[] { "MyVoxelReplicable.OnLoad - isUserCreated:", flag.ToString(), " isFromPrefab:", flag2.ToString(), " contentChanged:", flag3.ToString(), " data?: ", (memoryBuffer != null).ToString() };
            MyLog.Default.WriteLine(string.Concat(textArray1));
            if (!flag2)
            {
                MyEntities.TryGetEntityById<MyVoxelBase>(MySerializer.CreateAndRead<long>(stream, null), out base2, false);
            }
            else
            {
                MyObjectBuilder_EntityBase objectBuilder = MySerializer.CreateAndRead<MyObjectBuilder_EntityBase>(stream, MyObjectBuilderSerializer.Dynamic);
                if (memoryBuffer != null)
                {
                    IMyStorage storage = MyStorageBase.Load(memoryBuffer);
                    if (MyEntities.TryGetEntityById<MyVoxelBase>(objectBuilder.EntityId, out base2, false))
                    {
                        if (base2 is MyVoxelMap)
                        {
                            ((MyVoxelMap) base2).Storage = storage;
                        }
                        else if (base2 is MyPlanet)
                        {
                            ((MyPlanet) base2).Storage = storage;
                        }
                    }
                    else
                    {
                        base2 = (MyVoxelBase) MyEntities.CreateFromObjectBuilderNoinit(objectBuilder);
                        if (base2 is MyVoxelMap)
                        {
                            ((MyVoxelMap) base2).Init(objectBuilder, storage);
                        }
                        else if (base2 is MyPlanet)
                        {
                            ((MyPlanet) base2).Init(objectBuilder, storage);
                        }
                        if (base2 != null)
                        {
                            MyEntities.Add(base2, true);
                        }
                    }
                    base2.Save = true;
                }
                else if (flag3)
                {
                    this.GenerateFromObjectBuilder(objectBuilder, out base2);
                }
                else if (objectBuilder is MyObjectBuilder_Planet)
                {
                    if (!MyEntities.TryGetEntityById<MyVoxelBase>(objectBuilder.EntityId, out base2, false))
                    {
                    }
                }
                else if (!flag)
                {
                    base.TryRemoveExistingEntity(objectBuilder.EntityId);
                    this.GenerateFromObjectBuilder(objectBuilder, out base2);
                }
                else
                {
                    base.TryRemoveExistingEntity(objectBuilder.EntityId);
                    IMyStorage storage = MyGuiScreenDebugSpawnMenu.CreateAsteroidStorage(asteroid);
                    base2 = (MyVoxelBase) MyEntities.CreateFromObjectBuilderNoinit(objectBuilder);
                    if (base2 is MyVoxelMap)
                    {
                        ((MyVoxelMap) base2).Init(objectBuilder, storage);
                    }
                    if (base2 != null)
                    {
                        MyEntities.Add(base2, true);
                    }
                }
            }
            loadingDoneHandler(base2);
        }

        public void OnLoadBegin(Action<bool> loadingDoneHandler)
        {
            this.m_loadingDoneHandler = instance => this.OnLoadDone(instance, loadingDoneHandler);
        }

        public override bool OnSave(BitStream stream, Endpoint clientEndpoint) => 
            false;

        protected override void RaiseDestroyed()
        {
            MyPlanet instance = base.Instance as MyPlanet;
            base.RaiseDestroyed();
            if (Sync.IsServer && (instance != null))
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyEntities.ForceCloseEntityOnClients), instance.EntityId, targetEndpoint, position);
            }
        }

        public void Serialize(BitStream stream, HashSet<string> cachedData, Endpoint forClient, Action writeData)
        {
            if (!this.Voxel.Closed)
            {
                bool flag1;
                bool isUserCreated = this.Voxel.CreatedByUser && (this.Voxel.AsteroidName != null);
                bool isFromPrefab = this.Voxel.Save;
                bool contentChanged = this.Voxel.ContentChanged || this.Voxel.BeforeContentChanged;
                if ((cachedData == null) || !cachedData.Contains(this.Voxel.StorageName))
                {
                    flag1 = contentChanged || (isFromPrefab && !isUserCreated);
                }
                else
                {
                    flag1 = false;
                }
                bool sendContent = flag1;
                sendContent |= ReferenceEquals(this.Voxel.AsteroidName, null);
                string asteroidName = this.Voxel.AsteroidName;
                long entityId = this.Voxel.EntityId;
                byte[] data = null;
                MyObjectBuilder_EntityBase builder = null;
                if (sendContent)
                {
                    this.Voxel.Storage.Save(out data);
                }
                if (isFromPrefab)
                {
                    builder = this.Voxel.GetObjectBuilder(false);
                }
                Parallel.Start(delegate {
                    MySerializer.Write<bool>(stream, ref isUserCreated, null);
                    MySerializer.Write<bool>(stream, ref isFromPrefab, null);
                    MySerializer.Write<bool>(stream, ref sendContent, null);
                    MySerializer.Write<bool>(stream, ref contentChanged, null);
                    if (sendContent)
                    {
                        MySerializer.Write<byte[]>(stream, ref data, null);
                    }
                    else if (isUserCreated)
                    {
                        MySerializer.Write<string>(stream, ref asteroidName, null);
                    }
                    if (isFromPrefab)
                    {
                        MySerializer.Write<MyObjectBuilder_EntityBase>(stream, ref builder, MyObjectBuilderSerializer.Dynamic);
                    }
                    else
                    {
                        MySerializer.Write<long>(stream, ref entityId, null);
                    }
                    writeData();
                });
                if (cachedData != null)
                {
                    cachedData.Add(this.Voxel.StorageName);
                }
            }
        }

        public override bool ShouldReplicate(MyClientInfo client) => 
            ((this.Voxel != null) && ((this.Voxel.Storage != null) && (!this.Voxel.Closed && (!(this.Voxel is MyPlanet) ? (this.Voxel.Save || (this.Voxel.ContentChanged || this.Voxel.BeforeContentChanged)) : true))));

        private MyVoxelBase Voxel =>
            base.Instance;

        public override bool IncludeInIslands =>
            false;

        public bool NeedsToBeStreamed =>
            true;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyVoxelReplicable.<>c <>9 = new MyVoxelReplicable.<>c();
            public static Func<IMyEventOwner, Action<string>> <>9__11_0;
            public static Func<IMyEventOwner, Action<long>> <>9__23_0;

            internal Action<string> <GenerateFromObjectBuilder>b__11_0(IMyEventOwner s) => 
                new Action<string>(MyMultiplayerBase.InvalidateVoxelCache);

            internal Action<long> <RaiseDestroyed>b__23_0(IMyEventOwner s) => 
                new Action<long>(MyEntities.ForceCloseEntityOnClients);
        }
    }
}

