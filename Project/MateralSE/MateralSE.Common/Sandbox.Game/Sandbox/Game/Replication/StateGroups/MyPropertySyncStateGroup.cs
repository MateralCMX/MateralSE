namespace Sandbox.Game.Replication.StateGroups
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Collections;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Sync;

    public sealed class MyPropertySyncStateGroup : IMyStateGroup, IMyNetObject, IMyEventOwner
    {
        public Func<MyEventContext, ValidationResult> GlobalValidate = context => ValidationResult.Passed;
        public PriorityAdjustDelegate PriorityAdjust;
        private readonly ClientData m_clientData;
        private readonly ServerData m_serverData;
        private ListReader<SyncBase> m_properties;
        private readonly List<MyTimeSpan> m_propertyTimestamps;
        private readonly MyTimeSpan m_invalidTimestamp;

        public MyPropertySyncStateGroup(IMyReplicable ownerReplicable, SyncType syncType)
        {
            this.PriorityAdjust = (frames, state, priority) => priority;
            this.m_clientData = Sync.IsServer ? null : new ClientData();
            this.m_serverData = Sync.IsServer ? new ServerData() : null;
            this.m_invalidTimestamp = MyTimeSpan.FromTicks(-9223372036854775808L);
            this.Owner = ownerReplicable;
            syncType.PropertyChangedNotify += new Action<SyncBase>(this.Notify);
            syncType.PropertyCountChanged += new Action(this.OnPropertyCountChanged);
            this.m_properties = syncType.Properties;
            this.m_propertyTimestamps = new List<MyTimeSpan>(this.m_properties.Count);
            if (Sync.IsServer)
            {
                for (int i = 0; i < this.m_properties.Count; i++)
                {
                    this.m_propertyTimestamps.Add(MyMultiplayer.Static.ReplicationLayer.GetSimulationUpdateTime());
                }
            }
            else
            {
                for (int i = 0; i < this.m_properties.Count; i++)
                {
                    this.m_propertyTimestamps.Add(this.m_invalidTimestamp);
                }
            }
        }

        public void ClientUpdate(MyTimeSpan clientTimestamp)
        {
            if ((this.m_clientData.DirtyProperties.Bits != 0) && ((MyMultiplayer.Static.FrameCounter - this.m_clientData.LastUpdateFrame) >= 6))
            {
                foreach (SyncBase base2 in this.m_properties)
                {
                    if (this.m_clientData.DirtyProperties[base2.Id])
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyPropertySyncStateGroup, byte, double, BitReaderWriter>(this, x => new Action<byte, double, BitReaderWriter>(x.SyncPropertyChanged_Implementation), (byte) base2.Id, this.m_propertyTimestamps[base2.Id].Milliseconds, (BitReaderWriter) base2, targetEndpoint);
                    }
                }
                this.m_clientData.DirtyProperties.Reset(false);
                this.m_clientData.LastUpdateFrame = MyMultiplayer.Static.FrameCounter;
            }
        }

        public void CreateClientData(MyClientStateBase forClient)
        {
            ServerData.DataPerClient client = new ServerData.DataPerClient();
            this.m_serverData.ServerClientData.Add(forClient.EndpointId, client);
            if (this.m_properties.Count > 0)
            {
                client.DirtyProperties.Reset(true);
            }
        }

        public void Destroy()
        {
            this.Owner = null;
        }

        public void DestroyClientData(MyClientStateBase forClient)
        {
            this.m_serverData.ServerClientData.Remove(forClient.EndpointId);
        }

        public void ForceSend(MyClientStateBase clientData)
        {
        }

        public MyStreamProcessingState IsProcessingForClient(Endpoint forClient) => 
            MyStreamProcessingState.None;

        public bool IsStillDirty(Endpoint forClient) => 
            (this.m_serverData.ServerClientData[forClient].DirtyProperties.Bits != 0L);

        public void MarkDirty()
        {
            if (this.m_properties.Count != 0)
            {
                foreach (KeyValuePair<Endpoint, ServerData.DataPerClient> pair in this.m_serverData.ServerClientData)
                {
                    pair.Value.DirtyProperties.Reset(true);
                }
                MyMultiplayer.GetReplicationServer().AddToDirtyGroups(this);
            }
        }

        private void Notify(SyncBase sync)
        {
            if (((this.Owner != null) && (sync != null)) && this.Owner.IsValid)
            {
                if (!Sync.IsServer)
                {
                    if (this.m_propertyTimestamps[sync.Id] != this.m_invalidTimestamp)
                    {
                        MyTimeSpan simulationUpdateTime = MyMultiplayer.Static.ReplicationLayer.GetSimulationUpdateTime();
                        this.m_propertyTimestamps[sync.Id] = simulationUpdateTime;
                        this.m_clientData.DirtyProperties[sync.Id] = true;
                        MyReplicationClient replicationLayer = MyMultiplayer.ReplicationLayer as MyReplicationClient;
                        if (replicationLayer != null)
                        {
                            replicationLayer.AddToUpdates(this);
                        }
                    }
                }
                else
                {
                    MyTimeSpan simulationUpdateTime = MyMultiplayer.Static.ReplicationLayer.GetSimulationUpdateTime();
                    this.m_propertyTimestamps[sync.Id] = (simulationUpdateTime >= this.m_propertyTimestamps[sync.Id]) ? simulationUpdateTime : this.m_propertyTimestamps[sync.Id];
                    foreach (KeyValuePair<Endpoint, ServerData.DataPerClient> pair in this.m_serverData.ServerClientData)
                    {
                        if (pair.Value != null)
                        {
                            pair.Value.DirtyProperties[sync.Id] = true;
                        }
                    }
                    MyReplicationServer replicationServer = MyMultiplayer.GetReplicationServer();
                    if (replicationServer != null)
                    {
                        replicationServer.AddToDirtyGroups(this);
                    }
                }
            }
        }

        public unsafe void OnAck(MyClientStateBase forClient, byte packetId, bool delivered)
        {
            ServerData.DataPerClient client = this.m_serverData.ServerClientData[forClient.EndpointId];
            if (!delivered)
            {
                ulong* numPtr1 = (ulong*) ref client.DirtyProperties.Bits;
                numPtr1[0] |= client.SentProperties[packetId].Bits;
                MyMultiplayer.GetReplicationServer().AddToDirtyGroups(this);
            }
        }

        private void OnPropertyCountChanged()
        {
            if (Sync.IsServer)
            {
                for (int i = this.m_propertyTimestamps.Count; i < this.m_properties.Count; i++)
                {
                    this.m_propertyTimestamps.Add(MyMultiplayer.Static.ReplicationLayer.GetSimulationUpdateTime());
                }
            }
            else
            {
                for (int i = this.m_propertyTimestamps.Count; i < this.m_properties.Count; i++)
                {
                    this.m_propertyTimestamps.Add(this.m_invalidTimestamp);
                }
            }
        }

        public void Reset(bool reinit, MyTimeSpan clientTimestamp)
        {
        }

        public void Serialize(BitStream stream, Endpoint forClient, MyTimeSpan serverTimestamp, MyTimeSpan lastClientTimestamp, byte packetId, int maxBitPosition, HashSet<string> cachedData)
        {
            SmallBitField dirtyProperties;
            bool enableNetworkServerOutgoingPacketTracking = MyCompilationSymbols.EnableNetworkServerOutgoingPacketTracking;
            if (!stream.Writing)
            {
                dirtyProperties.Bits = stream.ReadUInt64(this.m_properties.Count);
            }
            else
            {
                dirtyProperties = this.m_serverData.ServerClientData[forClient].DirtyProperties;
                stream.WriteUInt64(dirtyProperties.Bits, this.m_properties.Count);
            }
            bool flag2 = MyCompilationSymbols.EnableNetworkServerOutgoingPacketTracking;
            for (int i = 0; i < this.m_properties.Count; i++)
            {
                if (dirtyProperties[i])
                {
                    if (!stream.Reading)
                    {
                        MyMultiplayer.GetReplicationServer();
                        double milliseconds = this.m_propertyTimestamps[i].Milliseconds;
                        bool flag3 = MyCompilationSymbols.EnableNetworkServerOutgoingPacketTracking;
                        stream.WriteDouble(milliseconds);
                        this.m_properties[i].Serialize(stream, false, true);
                    }
                    else
                    {
                        MyTimeSpan span = MyTimeSpan.FromMilliseconds(stream.ReadDouble());
                        if (this.m_properties[i].Serialize(stream, false, span >= this.m_propertyTimestamps[i]))
                        {
                            this.m_propertyTimestamps[i] = span;
                            this.m_clientData.DirtyProperties[i] = false;
                        }
                    }
                }
            }
            if (stream.Writing && (stream.BitPosition <= maxBitPosition))
            {
                ServerData.DataPerClient client = this.m_serverData.ServerClientData[forClient];
                client.SentProperties[packetId].Bits = client.DirtyProperties.Bits;
                client.DirtyProperties.Bits = 0UL;
            }
            bool flag4 = MyCompilationSymbols.EnableNetworkServerOutgoingPacketTracking;
        }

        [Event(null, 0xf8), Reliable, Server]
        private void SyncPropertyChanged_Implementation(byte propertyIndex, double propertyTimestampMs, BitReaderWriter reader)
        {
            ValidationResult result = this.GlobalValidate(MyEventContext.Current);
            if (!MyEventContext.Current.IsLocallyInvoked && (result != ValidationResult.Passed))
            {
                SyncBase base2 = null;
                if (propertyIndex < this.m_properties.Count)
                {
                    base2 = this.m_properties[propertyIndex];
                }
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, result.HasFlag(ValidationResult.Kick), result.ToString() + " " + ((base2 != null) ? this.m_properties[propertyIndex].DebugName : "Incorrect property index"), false);
            }
            else if (propertyIndex < this.m_properties.Count)
            {
                MyMultiplayer.GetReplicationServer();
                Endpoint key = new Endpoint(MyEventContext.Current.Sender, 0);
                MyTimeSpan span = MyTimeSpan.FromMilliseconds(propertyTimestampMs);
                bool acceptAndSetValue = span >= this.m_propertyTimestamps[propertyIndex];
                if (reader.ReadData(this.m_properties[propertyIndex], true, acceptAndSetValue))
                {
                    this.m_propertyTimestamps[propertyIndex] = span;
                }
                else if (acceptAndSetValue)
                {
                    ServerData.DataPerClient client;
                    this.m_serverData.ServerClientData.TryGetValue(key, out client);
                    if (client != null)
                    {
                        client.DirtyProperties[propertyIndex] = true;
                    }
                }
            }
        }

        public bool IsHighPriority =>
            false;

        public IMyReplicable Owner { get; private set; }

        public bool IsStreaming =>
            false;

        public bool IsValid =>
            ((this.Owner != null) && this.Owner.IsValid);

        public int PropertyCount =>
            this.m_properties.Count;

        public bool NeedsUpdate =>
            (this.m_clientData.DirtyProperties.Bits != 0L);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyPropertySyncStateGroup.<>c <>9 = new MyPropertySyncStateGroup.<>c();
            public static Func<MyEventContext, ValidationResult> <>9__24_0;
            public static MyPropertySyncStateGroup.PriorityAdjustDelegate <>9__24_1;
            public static Func<MyPropertySyncStateGroup, Action<byte, double, BitReaderWriter>> <>9__29_0;

            internal ValidationResult <.ctor>b__24_0(MyEventContext context) => 
                ValidationResult.Passed;

            internal float <.ctor>b__24_1(int frames, MyClientStateBase state, float priority) => 
                priority;

            internal Action<byte, double, BitReaderWriter> <ClientUpdate>b__29_0(MyPropertySyncStateGroup x) => 
                new Action<byte, double, BitReaderWriter>(x.SyncPropertyChanged_Implementation);
        }

        private class ClientData
        {
            public SmallBitField DirtyProperties;
            public uint LastUpdateFrame;
        }

        public delegate float PriorityAdjustDelegate(int frameCountWithoutSync, MyClientStateBase clientState, float basePriority);

        private class ServerData
        {
            public readonly Dictionary<Endpoint, DataPerClient> ServerClientData = new Dictionary<Endpoint, DataPerClient>();

            public class DataPerClient
            {
                public SmallBitField DirtyProperties = new SmallBitField(false);
                public readonly SmallBitField[] SentProperties = new SmallBitField[0x100];
            }
        }
    }
}

