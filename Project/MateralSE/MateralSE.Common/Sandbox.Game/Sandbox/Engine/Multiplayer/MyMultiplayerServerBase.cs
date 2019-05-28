namespace Sandbox.Engine.Multiplayer
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.GameServices;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Profiler;
    using VRage.Replication;
    using VRage.Utils;
    using VRageRender;

    public abstract class MyMultiplayerServerBase : MyMultiplayerBase, IReplicationServerCallback
    {
        private readonly MyReplicableFactory m_factory;
        private MemoryStream m_worldSendStream;

        protected MyMultiplayerServerBase(MySyncLayer syncLayer, EndpointId localClientEndpoint) : base(syncLayer)
        {
            this.m_factory = new MyReplicableFactory();
            MyReplicationServer layer = new MyReplicationServer(this, localClientEndpoint, Thread.CurrentThread);
            base.SetReplicationLayer(layer);
            base.ClientLeft += (steamId, e) => MySandboxGame.Static.Invoke(() => this.ReplicationLayer.OnClientLeft(new EndpointId(steamId)), "P2P Client left");
            base.ClientJoined += steamId => this.ReplicationLayer.OnClientJoined(new EndpointId(steamId), base.CreateClientState());
            MyEntities.OnEntityCreate += new Action<MyEntity>(this.CreateReplicableForObject);
            MyEntityComponentBase.OnAfterAddedToContainer += new Action<MyEntityComponentBase>(this.CreateReplicableForObject);
            MyExternalReplicable.Destroyed += new Action<MyExternalReplicable>(this.DestroyReplicable);
            foreach (MyEntity entity in MyEntities.GetEntities())
            {
                this.CreateReplicableForObject(entity);
                MyEntityComponentContainer components = entity.Components;
                if (components != null)
                {
                    foreach (MyComponentBase base2 in components)
                    {
                        this.CreateReplicableForObject(base2);
                    }
                }
            }
            syncLayer.TransportLayer.Register(MyMessageId.RPC, 0xff, new Action<MyPacket>(this.ReplicationLayer.OnEvent));
            syncLayer.TransportLayer.Register(MyMessageId.REPLICATION_READY, 0xff, new Action<MyPacket>(this.ReplicationLayer.ReplicableReady));
            syncLayer.TransportLayer.Register(MyMessageId.REPLICATION_REQUEST, 0xff, new Action<MyPacket>(this.ReplicationLayer.ReplicableRequest));
            syncLayer.TransportLayer.Register(MyMessageId.CLIENT_UPDATE, 0xff, new Action<MyPacket>(this.ReplicationLayer.OnClientUpdate));
            syncLayer.TransportLayer.Register(MyMessageId.CLIENT_ACKS, 0xff, new Action<MyPacket>(this.ReplicationLayer.OnClientAcks));
            syncLayer.TransportLayer.Register(MyMessageId.CLIENT_READY, 0xff, new Action<MyPacket>(this.ClientReady));
        }

        private void ClientReady(MyPacket packet)
        {
            this.ReplicationLayer.OnClientReady(packet.Sender, packet);
            packet.Return();
        }

        private void CreateReplicableForObject(object obj)
        {
            if ((obj != null) && !(obj is MyInventoryAggregate))
            {
                MyEntity entity = obj as MyEntity;
                if ((entity == null) || !entity.IsPreview)
                {
                    Type type = this.m_factory.FindTypeFor(obj);
                    if ((type != null) && this.ReplicationLayer.IsTypeReplicated(type))
                    {
                        MyExternalReplicable replicable = (MyExternalReplicable) Activator.CreateInstance(type);
                        replicable.Hook(obj);
                        this.ReplicationLayer.Replicate(replicable);
                        replicable.OnServerReplicate();
                    }
                }
            }
        }

        private void DestroyReplicable(MyExternalReplicable obj)
        {
            this.ReplicationLayer.Destroy(obj);
        }

        public override void Dispose()
        {
            MyEntities.OnEntityCreate -= new Action<MyEntity>(this.CreateReplicableForObject);
            MyEntityComponentBase.OnAfterAddedToContainer -= new Action<MyEntityComponentBase>(this.CreateReplicableForObject);
            MyExternalReplicable.Destroyed -= new Action<MyExternalReplicable>(this.DestroyReplicable);
            base.Dispose();
        }

        public MyPacketDataBitStreamBase GetBitStreamPacketData() => 
            MyNetworkWriter.GetBitStreamPacketData();

        public MyTimeSpan GetUpdateTime() => 
            MySandboxGame.Static.SimulationTimeWithSpeed;

        public override void KickClient(ulong userId, bool kicked = true, bool add = true)
        {
            MyControlKickClientMsg msg2;
            if (!kicked)
            {
                msg2 = new MyControlKickClientMsg {
                    KickedClient = userId,
                    Kicked = kicked
                };
                MyControlKickClientMsg message = msg2;
                MyLog.Default.WriteLineAndConsole("Player " + userId + " unkicked");
                base.RemoveKickedClient(userId);
                base.SendControlMessageToAll<MyControlKickClientMsg>(ref message, 0UL);
            }
            else
            {
                msg2 = new MyControlKickClientMsg {
                    KickedClient = userId,
                    Kicked = kicked,
                    Add = add
                };
                MyControlKickClientMsg message = msg2;
                MyLog.Default.WriteLineAndConsole("Player " + this.GetMemberName(userId) + " kicked");
                base.SendControlMessageToAll<MyControlKickClientMsg>(ref message, 0UL);
                if (add)
                {
                    base.AddKickedClient(userId);
                }
                base.RaiseClientLeft(userId, MyChatMemberStateChangeEnum.Kicked);
            }
        }

        protected override void OnClientKick(ref MyControlKickClientMsg data, ulong sender)
        {
            if (MySession.Static.IsUserAdmin(sender))
            {
                this.KickClient(data.KickedClient, (bool) data.Kicked, true);
            }
        }

        private void OnProfilerRequest(EndpointId sender)
        {
            if (!base.IsServer)
            {
                MyLog.Default.WriteLine("Profiler request received from " + MySession.Static.Players.TryGetIdentityNameFromSteamId(sender.Value) + ", but ignored");
            }
            else
            {
                MyLog.Default.WriteLine("Profiler request received from " + MySession.Static.Players.TryGetIdentityNameFromSteamId(sender.Value));
                ProfilerData data1 = new ProfilerData();
                data1.Sender = sender;
                data1.Priority = WorkPriority.Low;
                ProfilerData workData = data1;
                VRage.Profiler.MyRenderProfiler.AddPause(true);
                Parallel.Start(new Action<WorkData>(this.ProfilerRequestAsync), new Action<WorkData>(this.OnProfilerRequestComplete), workData);
            }
        }

        private void OnProfilerRequestComplete(WorkData data)
        {
            ProfilerData data2 = data as ProfilerData;
            base.SyncLayer.TransportLayer.SendFlush(data2.Sender.Value);
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<byte[]>(s => new Action<byte[]>(MyMultiplayerClientBase.ReceiveProfiler), data2.Buffer, new EndpointId(data2.Sender.Value), position);
        }

        public override void OnSessionReady()
        {
        }

        protected void OnWorldRequest(EndpointId sender)
        {
            MySandboxGame.Log.WriteLineAndConsole("World request received: " + this.GetMemberName(sender.Value));
            if (base.IsClientKickedOrBanned(sender.Value) || ((MySandboxGame.ConfigDedicated != null) && MySandboxGame.ConfigDedicated.Banned.Contains(sender.Value)))
            {
                MySandboxGame.Log.WriteLineAndConsole("Sending no world, because client has been kicked or banned: " + this.GetMemberName(sender.Value) + " (Client is probably modified.)");
                base.RaiseClientLeft(sender.Value, MyChatMemberStateChangeEnum.Banned);
            }
            else
            {
                this.m_worldSendStream = new MemoryStream();
                if (base.IsServer && (MySession.Static != null))
                {
                    MyObjectBuilder_Gps gps;
                    MySandboxGame.Log.WriteLine("...responding");
                    long key = MySession.Static.Players.TryGetIdentityId(sender.Value, 0);
                    MyObjectBuilder_World objectBuilder = MySession.Static.GetWorld(false);
                    MyObjectBuilder_Checkpoint checkpoint = objectBuilder.Checkpoint;
                    checkpoint.WorkshopId = null;
                    checkpoint.CharacterToolbar = null;
                    checkpoint.Settings.ScenarioEditMode = checkpoint.Settings.ScenarioEditMode && !MySession.Static.LoadedAsMission;
                    checkpoint.Gps.Dictionary.TryGetValue(key, out gps);
                    checkpoint.Gps.Dictionary.Clear();
                    if (gps != null)
                    {
                        checkpoint.Gps.Dictionary.Add(key, gps);
                    }
                    objectBuilder.Clusters = new List<BoundingBoxD>();
                    MyPhysics.SerializeClusters(objectBuilder.Clusters);
                    objectBuilder.Planets = MySession.Static.GetPlanetObjectBuilders();
                    MyObjectBuilderSerializer.SerializeXML(this.m_worldSendStream, objectBuilder, MyObjectBuilderSerializer.XmlCompression.Gzip, null);
                    base.SyncLayer.TransportLayer.SendFlush(sender.Value);
                    byte[] worldData = this.m_worldSendStream.ToArray();
                    this.ReplicationLayer.SendWorld(worldData, MyEventContext.Current.Sender);
                }
            }
        }

        [Event(null, 0xdb), Reliable, Server]
        public static void ProfilerRequest()
        {
            (MyMultiplayer.Static as MyMultiplayerServerBase).OnProfilerRequest(MyEventContext.Current.Sender);
        }

        private void ProfilerRequestAsync(WorkData data)
        {
            ProfilerData data2 = data as ProfilerData;
            try
            {
                VRage.Profiler.MyRenderProfiler.AddPause(false);
                MemoryStream writeTo = new MemoryStream();
                MyObjectBuilderSerializer.SerializeXML(writeTo, MyObjectBuilder_ProfilerSnapshot.GetObjectBuilder(MyRenderProxy.GetRenderProfiler()), MyObjectBuilderSerializer.XmlCompression.Gzip, null);
                data2.Buffer = writeTo.ToArray();
                MyLog.Default.WriteLine("Profiler for " + MySession.Static.Players.TryGetIdentityNameFromSteamId(data2.Sender.Value) + " serialized");
            }
            catch
            {
                MyLog.Default.WriteLine("Profiler serialization for " + MySession.Static.Players.TryGetIdentityNameFromSteamId(data2.Sender.Value) + " crashed");
            }
        }

        public void RaiseReplicableCreated(object obj)
        {
            this.CreateReplicableForObject(obj);
        }

        [Event(null, 0xf4), Reliable, Server]
        public static void RequestBatchConfirmation()
        {
            (MyMultiplayer.Static.ReplicationLayer as MyReplicationServer).SetClientBatchConfrmation(new Endpoint(MyEventContext.Current.Sender, 0), true);
        }

        public void SendPendingReplicablesDone(Endpoint endpoint)
        {
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(s => new Action(MyMultiplayerClientBase.ReceivePendingReplicablesDone), endpoint.Id, position);
        }

        public void ValidationFailed(ulong clientId, bool kick = true, string additionalInfo = null, bool stackTrace = true)
        {
            string msg = MySession.Static.Players.TryGetIdentityNameFromSteamId(clientId) + (kick ? " was trying to cheat!" : "'s action was blocked.");
            MyLog.Default.WriteLine(msg);
            if (additionalInfo != null)
            {
                MyLog.Default.WriteLine(additionalInfo);
            }
            if (stackTrace)
            {
                MyLog.Default.WriteLine(Environment.StackTrace);
            }
            bool flag1 = kick;
        }

        int IReplicationServerCallback.GetMTRSize(Endpoint clientId) => 
            0xfffff;

        int IReplicationServerCallback.GetMTUSize(Endpoint clientId) => 
            0x4a6;

        IMyReplicable IReplicationServerCallback.GetReplicableByEntityId(long entityId)
        {
            MyEntity entity;
            return (!MyEntities.TryGetEntityById(entityId, out entity, false) ? null : MyExternalReplicable.FindByObject(entity));
        }

        void IReplicationServerCallback.SendEvent(IPacketData data, bool reliable, List<EndpointId> endpoints)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.RPC, data, reliable, endpoints, 0);
        }

        void IReplicationServerCallback.SendJoinResult(IPacketData data, EndpointId endpoint)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.JOIN_RESULT, data, true, endpoint, 0);
        }

        void IReplicationServerCallback.SendPlayerData(IPacketData data, List<EndpointId> endpoints)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.PLAYER_DATA, data, true, endpoints, 0);
        }

        void IReplicationServerCallback.SendReplicationCreate(IPacketData data, Endpoint endpoint)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.REPLICATION_CREATE, data, true, endpoint.Id, endpoint.Index);
        }

        void IReplicationServerCallback.SendReplicationCreateStreamed(IPacketData data, Endpoint endpoint)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.REPLICATION_STREAM_BEGIN, data, true, endpoint.Id, endpoint.Index);
        }

        void IReplicationServerCallback.SendReplicationDestroy(IPacketData data, List<EndpointId> endpoints)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.REPLICATION_DESTROY, data, true, endpoints, 0);
        }

        void IReplicationServerCallback.SendReplicationIslandDone(IPacketData data, Endpoint endpoint)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.REPLICATION_ISLAND_DONE, data, true, endpoint.Id, endpoint.Index);
        }

        void IReplicationServerCallback.SendServerData(IPacketData data, Endpoint endpoint)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.SERVER_DATA, data, true, endpoint.Id, endpoint.Index);
        }

        void IReplicationServerCallback.SendStateSync(IPacketData data, Endpoint endpoint, bool reliable)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.SERVER_STATE_SYNC, data, reliable, endpoint.Id, endpoint.Index);
        }

        void IReplicationServerCallback.SendVoxelCacheInvalidated(string storageName, EndpointId endpoint)
        {
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<string>(s => new Action<string>(MyMultiplayerClientBase.InvalidateVoxelCacheClient), storageName, endpoint, position);
        }

        void IReplicationServerCallback.SendWorld(IPacketData data, EndpointId endpoint)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.WORLD, data, true, endpoint, 0);
        }

        void IReplicationServerCallback.SendWorldData(IPacketData data, List<EndpointId> endpoints)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.WORLD_DATA, data, true, endpoints, 0);
        }

        void IReplicationServerCallback.SentClientJoined(IPacketData data, EndpointId endpoint)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.CLIENT_CONNNECTED, data, true, endpoint, 0);
        }

        void IReplicationServerCallback.WriteCustomState(BitStream stream)
        {
            stream.WriteFloat(MyPhysics.SimulationRatio);
            float cPULoad = MySandboxGame.Static.CPULoad;
            stream.WriteFloat(cPULoad);
            float threadLoad = MySandboxGame.Static.ThreadLoad;
            stream.WriteFloat(threadLoad);
        }

        [Event(null, 0x81), Reliable, Server]
        public static void WorldRequest()
        {
            (MyMultiplayer.Static as MyMultiplayerServerBase).OnWorldRequest(MyEventContext.Current.Sender);
        }

        protected MyReplicationServer ReplicationLayer =>
            ((MyReplicationServer) base.ReplicationLayer);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyMultiplayerServerBase.<>c <>9 = new MyMultiplayerServerBase.<>c();
            public static Func<IMyEventOwner, Action<byte[]>> <>9__14_0;
            public static Func<IMyEventOwner, Action> <>9__18_0;
            public static Func<IMyEventOwner, Action<string>> <>9__35_0;

            internal Action<byte[]> <OnProfilerRequestComplete>b__14_0(IMyEventOwner s) => 
                new Action<byte[]>(MyMultiplayerClientBase.ReceiveProfiler);

            internal Action <SendPendingReplicablesDone>b__18_0(IMyEventOwner s) => 
                new Action(MyMultiplayerClientBase.ReceivePendingReplicablesDone);

            internal Action<string> <VRage.Replication.IReplicationServerCallback.SendVoxelCacheInvalidated>b__35_0(IMyEventOwner s) => 
                new Action<string>(MyMultiplayerClientBase.InvalidateVoxelCacheClient);
        }

        private class ProfilerData : WorkData
        {
            public EndpointId Sender;
            public byte[] Buffer;
        }
    }
}

