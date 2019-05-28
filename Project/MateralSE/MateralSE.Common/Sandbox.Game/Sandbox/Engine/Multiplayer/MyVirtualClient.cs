namespace Sandbox.Engine.Multiplayer
{
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;

    internal class MyVirtualClient
    {
        private readonly MyClientStateBase m_clientState;
        private readonly List<byte> m_acks = new List<byte>();
        private byte m_lastStateSyncPacketId;
        private byte m_clientPacketId;

        public MyVirtualClient(Endpoint endPoint, MyClientStateBase clientState, Sandbox.Game.World.MyPlayer.PlayerId playerId)
        {
            this.m_clientState = clientState;
            this.m_clientState.EndpointId = endPoint;
            this.m_clientState.PlayerSerialId = playerId.SerialId;
            this.PlayerId = playerId;
            TransportLayer.Register(MyMessageId.SERVER_DATA, endPoint.Index, new Action<MyPacket>(this.OnServerData));
            TransportLayer.Register(MyMessageId.REPLICATION_CREATE, endPoint.Index, new Action<MyPacket>(this.OnReplicationCreate));
            TransportLayer.Register(MyMessageId.REPLICATION_DESTROY, endPoint.Index, new Action<MyPacket>(this.OnReplicationDestroy));
            TransportLayer.Register(MyMessageId.SERVER_STATE_SYNC, endPoint.Index, new Action<MyPacket>(this.OnServerStateSync));
            TransportLayer.Register(MyMessageId.RPC, endPoint.Index, new Action<MyPacket>(this.OnEvent));
            TransportLayer.Register(MyMessageId.REPLICATION_STREAM_BEGIN, endPoint.Index, new Action<MyPacket>(this.OnReplicationStreamBegin));
            TransportLayer.Register(MyMessageId.JOIN_RESULT, endPoint.Index, new Action<MyPacket>(this.OnJoinResult));
            TransportLayer.Register(MyMessageId.WORLD_DATA, endPoint.Index, new Action<MyPacket>(this.OnWorldData));
            TransportLayer.Register(MyMessageId.CLIENT_CONNNECTED, endPoint.Index, new Action<MyPacket>(this.OnClientConnected));
            TransportLayer.Register(MyMessageId.REPLICATION_ISLAND_DONE, endPoint.Index, new Action<MyPacket>(this.OnReplicationIslandDone));
        }

        private void OnClientConnected(MyPacket packet)
        {
            throw new NotImplementedException();
        }

        private void OnEvent(MyPacket packet)
        {
            throw new NotImplementedException();
        }

        private void OnJoinResult(MyPacket packet)
        {
            throw new NotImplementedException();
        }

        private void OnReplicationCreate(MyPacket packet)
        {
            packet.BitStream.ReadTypeId();
            NetworkId networkId = packet.BitStream.ReadNetworkId();
            MyPacketDataBitStreamBase bitStreamPacketData = MyNetworkWriter.GetBitStreamPacketData();
            bitStreamPacketData.Stream.WriteNetworkId(networkId);
            bitStreamPacketData.Stream.WriteBool(true);
            bitStreamPacketData.Stream.Terminate();
            this.SendReplicableReady(bitStreamPacketData);
            packet.Return();
        }

        private void OnReplicationDestroy(MyPacket packet)
        {
            packet.Return();
        }

        private void OnReplicationIslandDone(MyPacket packet)
        {
            packet.Return();
        }

        private void OnReplicationStreamBegin(MyPacket packet)
        {
            this.OnReplicationCreate(packet);
            packet.Return();
        }

        private void OnServerData(MyPacket packet)
        {
            packet.Return();
        }

        private void OnServerStateSync(MyPacket packet)
        {
            byte item = packet.BitStream.ReadByte(8);
            if (!packet.BitStream.ReadBool() && !this.m_acks.Contains(item))
            {
                this.m_acks.Add(item);
            }
            this.m_lastStateSyncPacketId = item;
            packet.Return();
        }

        private void OnWorldData(MyPacket packet)
        {
            throw new NotImplementedException();
        }

        private void SendClientAcks(IPacketData data)
        {
            TransportLayer.SendMessage(MyMessageId.CLIENT_ACKS, data, true, new EndpointId(Sync.ServerId), this.m_clientState.EndpointId.Index);
        }

        private void SendClientUpdate(IPacketData data)
        {
            TransportLayer.SendMessage(MyMessageId.CLIENT_UPDATE, data, false, new EndpointId(Sync.ServerId), this.m_clientState.EndpointId.Index);
        }

        private void SendConnectRequest(IPacketData data)
        {
            TransportLayer.SendMessage(MyMessageId.CLIENT_CONNNECTED, data, true, new EndpointId(Sync.ServerId), this.m_clientState.EndpointId.Index);
        }

        private void SendEvent(IPacketData data, bool reliable)
        {
            TransportLayer.SendMessage(MyMessageId.RPC, data, reliable, new EndpointId(Sync.ServerId), this.m_clientState.EndpointId.Index);
        }

        private void SendReplicableReady(IPacketData data)
        {
            TransportLayer.SendMessage(MyMessageId.REPLICATION_READY, data, true, new EndpointId(Sync.ServerId), this.m_clientState.EndpointId.Index);
        }

        private void SendUpdate()
        {
            MyPacketDataBitStreamBase bitStreamPacketData = MyNetworkWriter.GetBitStreamPacketData();
            BitStream stream = bitStreamPacketData.Stream;
            stream.WriteByte(this.m_lastStateSyncPacketId, 8);
            stream.WriteByte((byte) this.m_acks.Count, 8);
            foreach (byte num2 in this.m_acks)
            {
                stream.WriteByte(num2, 8);
            }
            stream.Terminate();
            this.m_acks.Clear();
            this.SendClientAcks(bitStreamPacketData);
            bitStreamPacketData = MyNetworkWriter.GetBitStreamPacketData();
            stream = bitStreamPacketData.Stream;
            this.m_clientPacketId = (byte) (this.m_clientPacketId + 1);
            stream.WriteByte(this.m_clientPacketId, 8);
            stream.WriteDouble(MyTimeSpan.FromTicks(Stopwatch.GetTimestamp()).Milliseconds);
            stream.WriteDouble(0.0);
            this.m_clientState.Serialize(stream, false);
            stream.Terminate();
            this.SendClientUpdate(bitStreamPacketData);
        }

        public void Tick()
        {
            this.SendUpdate();
        }

        public Sandbox.Game.World.MyPlayer.PlayerId PlayerId { get; private set; }

        private static MyTransportLayer TransportLayer =>
            MyMultiplayer.Static.SyncLayer.TransportLayer;
    }
}

