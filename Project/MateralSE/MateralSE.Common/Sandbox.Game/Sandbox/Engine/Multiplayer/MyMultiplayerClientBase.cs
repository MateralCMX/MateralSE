namespace Sandbox.Engine.Multiplayer
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.History;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Profiler;
    using VRage.Replication;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageRender;

    public abstract class MyMultiplayerClientBase : MyMultiplayerBase, IReplicationClientCallback
    {
        private const int CONNECTION_STATE_TICKS = 12;
        private int m_ticks;
        private bool m_removingVoxelCacheFromServer;

        protected MyMultiplayerClientBase(MySyncLayer syncLayer) : base(syncLayer)
        {
            base.SetReplicationLayer(new MyReplicationClient(new Endpoint(Sync.MyId, 0), this, base.CreateClientState(), 16.66667f, new Action<string>(this.JoinFailCallback), MyFakes.MULTIPLAYER_PREDICTION_RESET_CLIENT_FALLING_BEHIND, Thread.CurrentThread));
            this.ReplicationLayer.UseSmoothPing = MyFakes.MULTIPLAYER_SMOOTH_PING;
            MyReplicationClient.SynchronizationTimingType = MyReplicationClient.TimingType.LastServerTime;
            syncLayer.TransportLayer.Register(MyMessageId.SERVER_DATA, 0, new Action<MyPacket>(this.ReplicationLayer.OnServerData));
            syncLayer.TransportLayer.Register(MyMessageId.REPLICATION_CREATE, 0, new Action<MyPacket>(this.ReplicationLayer.ProcessReplicationCreate));
            syncLayer.TransportLayer.Register(MyMessageId.REPLICATION_DESTROY, 0, new Action<MyPacket>(this.ReplicationLayer.ProcessReplicationDestroy));
            syncLayer.TransportLayer.Register(MyMessageId.SERVER_STATE_SYNC, 0, new Action<MyPacket>(this.ReplicationLayer.OnServerStateSync));
            syncLayer.TransportLayer.Register(MyMessageId.RPC, 0, new Action<MyPacket>(this.ReplicationLayer.OnEvent));
            syncLayer.TransportLayer.Register(MyMessageId.REPLICATION_STREAM_BEGIN, 0, new Action<MyPacket>(this.ReplicationLayer.ProcessReplicationCreateBegin));
            syncLayer.TransportLayer.Register(MyMessageId.REPLICATION_ISLAND_DONE, 0, new Action<MyPacket>(this.ReplicationLayer.ProcessReplicationIslandDone));
            syncLayer.TransportLayer.Register(MyMessageId.WORLD, 0, new Action<MyPacket>(this.ReceiveWorld));
            syncLayer.TransportLayer.Register(MyMessageId.PLAYER_DATA, 0, new Action<MyPacket>(this.ReceivePlayerData));
            MyNetworkMonitor.OnTick += new Action(this.OnTick);
            base.m_voxelMapData = new LRUCache<string, byte[]>(100, null);
            base.m_voxelMapData.OnItemDiscarded = (Action<string, byte[]>) Delegate.Combine(base.m_voxelMapData.OnItemDiscarded, delegate (string name, byte[] _) {
                if (!this.m_removingVoxelCacheFromServer)
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<string>(s => new Action<string>(MyMultiplayerBase.InvalidateVoxelCache), name, targetEndpoint, position);
                }
            });
        }

        public void DisconnectFromHost()
        {
            this.DisconnectClient(0UL);
        }

        public override void Dispose()
        {
            base.Dispose();
            MyNetworkMonitor.OnTick -= new Action(this.OnTick);
        }

        public override void DownloadProfiler()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(s => new Action(MyMultiplayerServerBase.ProfilerRequest), targetEndpoint, position);
        }

        public override void DownloadWorld()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(s => new Action(MyMultiplayerServerBase.WorldRequest), targetEndpoint, position);
        }

        public MyPacketDataBitStreamBase GetBitStreamPacketData() => 
            MyNetworkWriter.GetBitStreamPacketData();

        public float GetClientSimulationRatio() => 
            Math.Min((float) (100f / MySandboxGame.Static.CPULoadSmooth), (float) 1f);

        public float GetServerSimulationRatio() => 
            Sync.ServerSimulationRatio;

        public MyTimeSpan GetUpdateTime() => 
            MySandboxGame.Static.SimulationTimeWithSpeed;

        [Event(null, 0x132), Reliable, Client]
        public static void InvalidateVoxelCacheClient(string storageName)
        {
            MyMultiplayerClientBase @static = (MyMultiplayerClientBase) MyMultiplayer.Static;
            @static.m_removingVoxelCacheFromServer = true;
            @static.m_voxelMapData.Remove(storageName);
            @static.m_removingVoxelCacheFromServer = false;
        }

        private void JoinFailCallback(string message)
        {
            MyStringId caption = new MyStringId();
            MyGuiSandbox.Show(new StringBuilder(message), caption, MyMessageBoxStyleEnum.Error);
        }

        public override void KickClient(ulong client, bool kicked = true, bool add = true)
        {
            MyControlKickClientMsg message = new MyControlKickClientMsg {
                KickedClient = client,
                Kicked = kicked
            };
            base.SendControlMessage<MyControlKickClientMsg>(base.ServerId, ref message, true);
        }

        protected override void OnClientKick(ref MyControlKickClientMsg data, ulong sender)
        {
            if (!data.Kicked)
            {
                base.RemoveKickedClient(data.KickedClient);
            }
            else if (data.KickedClient != Sync.MyId)
            {
                if (data.Add)
                {
                    base.AddKickedClient(data.KickedClient);
                }
                base.RaiseClientLeft(data.KickedClient, MyChatMemberStateChangeEnum.Kicked);
            }
            else
            {
                MySessionLoader.UnloadAndExitToMenu();
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionKicked);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextYouHaveBeenKicked), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        public override void OnSessionReady()
        {
            ClientReadyDataMsg msg = new ClientReadyDataMsg {
                UsePlayoutDelayBufferForCharacter = true,
                UsePlayoutDelayBufferForJetpack = true,
                UsePlayoutDelayBufferForGrids = true
            };
            this.ReplicationLayer.SendClientReady(ref msg);
        }

        private void OnTick()
        {
            this.m_ticks++;
            if (this.m_ticks > 12)
            {
                MyP2PSessionState state = new MyP2PSessionState();
                MyGameService.Peer2Peer.GetSessionState(base.ServerId, ref state);
                base.IsConnectionDirect = !state.UsingRelay;
                base.IsConnectionAlive = state.ConnectionActive;
                this.m_ticks = 0;
            }
        }

        public void PauseClient(bool pause)
        {
            if (pause)
            {
                MySandboxGame.PausePush();
                MyHud.Notifications.Add(MyNotificationSingletons.ConnectionProblem);
            }
            else
            {
                MySandboxGame.PausePop();
                MyHud.Notifications.Remove(MyNotificationSingletons.ConnectionProblem);
            }
        }

        [Event(null, 270), Reliable, Client]
        public static void ReceivePendingReplicablesDone()
        {
            MyMultiplayer.Static.ReceivePendingReplicablesDone();
        }

        public void ReceivePlayerData(MyPacket packet)
        {
            PlayerDataMsg msg = this.ReplicationLayer.OnPlayerData(packet);
            if ((MySession.Static != null) || (msg.ClientSteamId == Sync.MyId))
            {
                MySession.Static.Players.OnInitialPlayerCreated(msg.ClientSteamId, msg.PlayerSerialId, msg.IdentityId, msg.DisplayName, msg.BuildColors, msg.RealPlayer, msg.NewIdentity);
            }
            else
            {
                packet.BitStream.SetBitPositionRead(0);
                base.SyncLayer.TransportLayer.AddMessageToBuffer(packet);
            }
        }

        [Event(null, 0xfc), Reliable, Client]
        public static void ReceiveProfiler(byte[] profilerData)
        {
            MemoryStream reader = new MemoryStream(profilerData);
            try
            {
                MyObjectBuilder_ProfilerSnapshot snapshot;
                MyObjectBuilderSerializer.DeserializeGZippedXML<MyObjectBuilder_ProfilerSnapshot>(reader, out snapshot);
                snapshot.Init(MyRenderProxy.GetRenderProfiler(), VRage.Profiler.SnapshotType.Server, false);
                MyMultiplayer.Static.ProfilerDone.InvokeIfNotNull<string>("ProfilerDownload: Done.");
            }
            catch
            {
                MyMultiplayer.Static.ProfilerDone.InvokeIfNotNull<string>("ProfilerDownload: Could not parse data.");
            }
        }

        public void ReceiveWorld(MyPacket packet)
        {
            MyObjectBuilder_World world;
            MyObjectBuilderSerializer.DeserializeGZippedXML<MyObjectBuilder_World>(new MemoryStream(MySerializer.CreateAndRead<byte[]>(packet.BitStream, null)), out world);
            MyJoinGameHelper.WorldReceived(world, MyMultiplayer.Static);
        }

        public void RequestBatchConfirmation()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(s => new Action(MyMultiplayerServerBase.RequestBatchConfirmation), targetEndpoint, position);
        }

        public void SetIslandDone(byte index, Dictionary<long, MatrixD> matrices)
        {
            MyEntities.ReleaseWaitingAsync(index, matrices);
        }

        public void SetNextFrameDelayDelta(float delay)
        {
            MySandboxGame.Static.SetNextFrameDelayDelta(delay);
        }

        public void SetPing(long duration)
        {
            MyGeneralStats.Static.Ping = duration;
        }

        public override void Tick()
        {
            base.Tick();
            MySession.Static.VirtualClients.Tick();
        }

        public void UpdateSnapshotCache()
        {
            MySnapshotCache.Apply();
        }

        void IReplicationClientCallback.ReadCustomState(BitStream stream)
        {
            Sync.ServerSimulationRatio = stream.ReadFloat();
            float single1 = stream.ReadFloat();
            Sync.ServerCPULoad = single1;
            Sync.ServerCPULoadSmooth = single1;
            float single2 = stream.ReadFloat();
            Sync.ServerThreadLoad = single2;
            Sync.ServerThreadLoadSmooth = single2;
        }

        void IReplicationClientCallback.SendClientAcks(IPacketData data)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.CLIENT_ACKS, data, true, new EndpointId(base.ServerId), 0);
        }

        void IReplicationClientCallback.SendClientReady(MyPacketDataBitStreamBase data)
        {
            Sync.Layer.TransportLayer.SendMessage(MyMessageId.CLIENT_READY, data, true, new EndpointId(Sync.ServerId), 0);
        }

        void IReplicationClientCallback.SendClientUpdate(IPacketData data)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.CLIENT_UPDATE, data, false, new EndpointId(base.ServerId), 0);
        }

        void IReplicationClientCallback.SendConnectRequest(IPacketData data)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.CLIENT_CONNNECTED, data, true, new EndpointId(base.ServerId), 0);
        }

        void IReplicationClientCallback.SendEvent(IPacketData data, bool reliable)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.RPC, data, reliable, new EndpointId(base.ServerId), 0);
        }

        void IReplicationClientCallback.SendReplicableReady(IPacketData data)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.REPLICATION_READY, data, true, new EndpointId(base.ServerId), 0);
        }

        void IReplicationClientCallback.SendReplicableRequest(IPacketData data)
        {
            base.SyncLayer.TransportLayer.SendMessage(MyMessageId.REPLICATION_REQUEST, data, true, new EndpointId(base.ServerId), 0);
        }

        protected MyReplicationClient ReplicationLayer =>
            ((MyReplicationClient) base.ReplicationLayer);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyMultiplayerClientBase.<>c <>9 = new MyMultiplayerClientBase.<>c();
            public static Func<IMyEventOwner, Action<string>> <>9__20_1;
            public static Func<IMyEventOwner, Action> <>9__29_0;
            public static Func<IMyEventOwner, Action> <>9__30_0;
            public static Func<IMyEventOwner, Action> <>9__31_0;

            internal Action<string> <.ctor>b__20_1(IMyEventOwner s) => 
                new Action<string>(MyMultiplayerBase.InvalidateVoxelCache);

            internal Action <DownloadProfiler>b__30_0(IMyEventOwner s) => 
                new Action(MyMultiplayerServerBase.ProfilerRequest);

            internal Action <DownloadWorld>b__29_0(IMyEventOwner s) => 
                new Action(MyMultiplayerServerBase.WorldRequest);

            internal Action <RequestBatchConfirmation>b__31_0(IMyEventOwner s) => 
                new Action(MyMultiplayerServerBase.RequestBatchConfirmation);
        }
    }
}

