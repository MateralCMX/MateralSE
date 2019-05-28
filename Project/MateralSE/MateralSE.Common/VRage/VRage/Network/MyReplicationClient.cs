namespace VRage.Network
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Profiler;
    using VRage.Replication;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    public class MyReplicationClient : MyReplicationLayer
    {
        private const float NO_RESPONSE_ACTION_SECONDS = 5f;
        public static SerializableVector3I StressSleep = new SerializableVector3I(0, 0, 0);
        private readonly MyClientStateBase m_clientState;
        private bool m_clientReady;
        private bool m_hasTypeTable;
        private readonly IReplicationClientCallback m_callback;
        private readonly CacheList<IMyStateGroup> m_tmpGroups;
        private readonly List<byte> m_acks;
        private byte m_lastStateSyncPacketId;
        private byte m_clientPacketId;
        private MyTimeSpan m_lastServerTimestamp;
        private MyTimeSpan m_lastServerTimeStampReceivedTime;
        private bool m_clientPaused;
        private readonly CachingDictionary<NetworkId, MyPendingReplicable> m_pendingReplicables;
        private readonly HashSet<NetworkId> m_destroyedReplicables;
        private readonly MyEventsBuffer m_eventBuffer;
        private readonly MyEventsBuffer.Handler m_eventHandler;
        private readonly MyEventsBuffer.IsBlockedHandler m_isBlockedHandler;
        private MyTimeSpan m_clientStartTimeStamp;
        private readonly float m_simulationTimeStep;
        private readonly ConcurrentCachingHashSet<IMyStateGroup> m_stateGroupsForUpdate;
        [CompilerGenerated]
        private Action<IMyReplicable> OnReplicableReady;
        private readonly Action<string> m_failureCallback;
        private const int MAX_TIMESTAMP_DIFF_LOW = 80;
        private const int MAX_TIMESTAMP_DIFF_HIGH = 500;
        private const int MAX_TIMESTAMP_DIFF_VERY_HIGH = 0x1388;
        private MyTimeSpan m_lastTime;
        private MyTimeSpan m_ping;
        private MyTimeSpan m_smoothPing;
        private MyTimeSpan m_lastPingTime;
        private MyTimeSpan m_correctionSmooth;
        public static TimingType SynchronizationTimingType = TimingType.None;
        private MyTimeSpan m_lastClientTime;
        private MyTimeSpan m_lastServerTime;
        private MyPacketStatistics m_serverStats;
        private readonly MyPacketTracker m_serverTracker;
        private MyPacketStatistics m_clientStats;
        private readonly CacheList<IMyReplicable> m_tmp;
        private readonly bool m_predictionReset;
        private MyTimeSpan m_lastClientTimestamp;
        private float m_timeDiffSmoothed;

        public event Action<IMyReplicable> OnReplicableReady
        {
            [CompilerGenerated] add
            {
                Action<IMyReplicable> onReplicableReady = this.OnReplicableReady;
                while (true)
                {
                    Action<IMyReplicable> a = onReplicableReady;
                    Action<IMyReplicable> action3 = (Action<IMyReplicable>) Delegate.Combine(a, value);
                    onReplicableReady = Interlocked.CompareExchange<Action<IMyReplicable>>(ref this.OnReplicableReady, action3, a);
                    if (ReferenceEquals(onReplicableReady, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IMyReplicable> onReplicableReady = this.OnReplicableReady;
                while (true)
                {
                    Action<IMyReplicable> source = onReplicableReady;
                    Action<IMyReplicable> action3 = (Action<IMyReplicable>) Delegate.Remove(source, value);
                    onReplicableReady = Interlocked.CompareExchange<Action<IMyReplicable>>(ref this.OnReplicableReady, action3, source);
                    if (ReferenceEquals(onReplicableReady, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyReplicationClient(Endpoint endpointId, IReplicationClientCallback callback, MyClientStateBase clientState, float simulationTimeStep, Action<string> failureCallback, bool predictionReset, Thread mainThread) : base(false, endpointId.Id, mainThread)
        {
            this.m_tmpGroups = new CacheList<IMyStateGroup>(4);
            this.m_acks = new List<byte>();
            this.m_lastServerTimestamp = MyTimeSpan.Zero;
            this.m_lastServerTimeStampReceivedTime = MyTimeSpan.Zero;
            this.m_pendingReplicables = new CachingDictionary<NetworkId, MyPendingReplicable>();
            this.m_destroyedReplicables = new HashSet<NetworkId>();
            this.m_clientStartTimeStamp = MyTimeSpan.Zero;
            this.m_stateGroupsForUpdate = new ConcurrentCachingHashSet<IMyStateGroup>();
            this.m_serverTracker = new MyPacketTracker();
            this.m_tmp = new CacheList<IMyReplicable>();
            this.m_eventBuffer = new MyEventsBuffer(mainThread, 0x20);
            base.m_replicables = new MyReplicablesHierarchy(mainThread);
            this.m_simulationTimeStep = simulationTimeStep;
            this.m_callback = callback;
            this.m_clientState = clientState;
            this.m_clientState.EndpointId = endpointId;
            this.m_eventHandler = new MyEventsBuffer.Handler(this.OnEvent);
            this.m_isBlockedHandler = new MyEventsBuffer.IsBlockedHandler(this.IsBlocked);
            this.m_failureCallback = failureCallback;
            this.m_predictionReset = predictionReset;
        }

        protected override void AddNetworkObject(NetworkId networkId, IMyNetObject obj)
        {
            base.AddNetworkObject(networkId, obj);
            IMyStateGroup item = obj as IMyStateGroup;
            if ((item != null) && item.NeedsUpdate)
            {
                this.m_stateGroupsForUpdate.Add(item);
            }
        }

        public void AddToUpdates(IMyStateGroup group)
        {
            this.m_stateGroupsForUpdate.Add(group);
        }

        public override MyPacketStatistics ClearClientStatistics()
        {
            this.m_clientStats.Reset();
            return this.m_clientStats;
        }

        public override MyPacketStatistics ClearServerStatistics()
        {
            this.m_serverStats.Reset();
            return this.m_serverStats;
        }

        public override void Disconnect()
        {
            this.m_callback.DisconnectFromHost();
        }

        protected override bool DispatchBlockingEvent(IPacketData data, VRage.Network.CallSite site, EndpointId recipient, IMyNetObject eventInstance, Vector3D? position, IMyNetObject blockedNetObj) => 
            this.DispatchEvent(data, site, recipient, eventInstance, position);

        protected override bool DispatchEvent(IPacketData data, VRage.Network.CallSite site, EndpointId target, IMyNetObject instance, Vector3D? position)
        {
            if (site.HasServerFlag)
            {
                this.m_callback.SendEvent(data, site.IsReliable);
            }
            else
            {
                bool hasClientFlag = site.HasClientFlag;
                data.Return();
            }
            return false;
        }

        public override void Dispose()
        {
            this.m_eventBuffer.Dispose();
            base.Dispose();
        }

        protected override MyPacketDataBitStreamBase GetBitStreamPacketData() => 
            this.m_callback.GetBitStreamPacketData();

        public override string GetMultiplayerStat()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(base.GetMultiplayerStat());
            builder.AppendLine("Pending Replicables:");
            foreach (KeyValuePair<NetworkId, MyPendingReplicable> pair in this.m_pendingReplicables)
            {
                string str2 = "   NetworkId: " + pair.Key.ToString() + ", IsStreaming: " + pair.Value.IsStreaming.ToString();
                builder.AppendLine(str2);
            }
            builder.Append(this.m_eventBuffer.GetEventsBufferStat());
            return builder.ToString();
        }

        public override MyTimeSpan GetSimulationUpdateTime()
        {
            MyTimeSpan updateTime = this.m_callback.GetUpdateTime();
            if (this.m_clientStartTimeStamp == MyTimeSpan.Zero)
            {
                this.m_clientStartTimeStamp = updateTime;
            }
            return (updateTime - this.m_clientStartTimeStamp);
        }

        private bool IsBlocked(NetworkId networkId, NetworkId blockedNetId)
        {
            bool flag = this.m_pendingReplicables.ContainsKey(networkId) || this.m_pendingReplicables.ContainsKey(blockedNetId);
            bool flag2 = (base.GetObjectByNetworkId(networkId) == null) || (blockedNetId.IsValid && ReferenceEquals(base.GetObjectByNetworkId(blockedNetId), null));
            return (networkId.IsValid && (flag | flag2));
        }

        public ChatMsg OnChatMessage(MyPacket packet) => 
            MySerializer.CreateAndRead<ChatMsg>(packet.BitStream, null);

        public ConnectedClientDataMsg OnClientConnected(MyPacket packet) => 
            MySerializer.CreateAndRead<ConnectedClientDataMsg>(packet.BitStream, null);

        protected override void OnEvent(MyPacketDataBitStreamBase data, VRage.Network.CallSite site, object obj, IMyNetObject sendAs, Vector3D? position, EndpointId source)
        {
            base.LastMessageFromServer = DateTime.UtcNow;
            base.Invoke(site, data.Stream, obj, source, null, false);
            data.Return();
        }

        protected override void OnEvent(MyPacketDataBitStreamBase data, NetworkId networkId, NetworkId blockedNetId, uint eventId, EndpointId sender, Vector3D? position)
        {
            base.LastMessageFromServer = DateTime.UtcNow;
            bool flag = this.m_eventBuffer.ContainsEvents(networkId) || this.m_eventBuffer.ContainsEvents(blockedNetId);
            if (!(this.IsBlocked(networkId, blockedNetId) | flag))
            {
                base.OnEvent(data, networkId, blockedNetId, eventId, sender, position);
            }
            else
            {
                this.m_eventBuffer.EnqueueEvent(data, networkId, blockedNetId, eventId, sender, position);
                if (blockedNetId.IsValid)
                {
                    this.m_eventBuffer.EnqueueBarrier(blockedNetId, networkId);
                }
            }
        }

        public JoinResultMsg OnJoinResult(MyPacket packet) => 
            MySerializer.CreateAndRead<JoinResultMsg>(packet.BitStream, null);

        public void OnLocalClientReady()
        {
            this.m_clientReady = true;
        }

        public PlayerDataMsg OnPlayerData(MyPacket packet) => 
            MySerializer.CreateAndRead<PlayerDataMsg>(packet.BitStream, null);

        public void OnServerData(MyPacket packet)
        {
            int bitPosition = packet.BitStream.BitPosition;
            try
            {
                base.SerializeTypeTable(packet.BitStream);
                this.m_hasTypeTable = true;
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Server sent bad data!");
                byte[] bytes = new byte[packet.BitStream.ByteLength];
                packet.BitStream.ReadBytes(bytes, 0, packet.BitStream.ByteLength);
                MyLog.Default.WriteLine("Server Data: " + string.Join<byte>(", ", bytes));
                MyLog.Default.WriteLine(exception);
                this.m_failureCallback("Failed to connect to server. See log for details.");
            }
            packet.Return();
        }

        public void OnServerStateSync(MyPacket packet)
        {
            base.LastMessageFromServer = DateTime.UtcNow;
            bool flag = packet.BitStream.ReadBool();
            byte num = packet.BitStream.ReadByte(8);
            if (!flag)
            {
                MyPacketTracker.OrderType type = this.m_serverTracker.Add(num);
                this.m_clientStats.Update(type);
                if (type == MyPacketTracker.OrderType.Duplicate)
                {
                    return;
                }
                this.m_lastStateSyncPacketId = num;
                if (!this.m_acks.Contains(num))
                {
                    this.m_acks.Add(num);
                }
            }
            MyPacketStatistics statistics = new MyPacketStatistics();
            statistics.Read(packet.BitStream);
            this.m_serverStats.Add(statistics);
            MyTimeSpan span = MyTimeSpan.FromMilliseconds(packet.BitStream.ReadDouble());
            if (this.m_lastServerTimestamp < span)
            {
                this.m_lastServerTimestamp = span;
                this.m_lastServerTimeStampReceivedTime = packet.ReceivedTime;
            }
            MyTimeSpan span2 = MyTimeSpan.FromMilliseconds(packet.BitStream.ReadDouble());
            if (span2 > this.m_lastClientTimestamp)
            {
                this.m_lastClientTimestamp = span2;
            }
            double milliseconds = packet.BitStream.ReadDouble();
            if (milliseconds > 0.0)
            {
                MyTimeSpan ping = packet.ReceivedTime - MyTimeSpan.FromMilliseconds(milliseconds);
                if (ping.Milliseconds < 1000.0)
                {
                    this.SetPing(ping);
                }
            }
            MyTimeSpan serverTimestamp = span;
            this.m_callback.ReadCustomState(packet.BitStream);
            while ((packet.BitStream.BytePosition + 2) < packet.BitStream.ByteLength)
            {
                if (!packet.BitStream.CheckTerminator())
                {
                    MyLog.Default.WriteLine("OnServerStateSync: Invalid stream terminator");
                    packet.Return();
                    return;
                }
                NetworkId id = packet.BitStream.ReadNetworkId();
                IMyStateGroup objectByNetworkId = base.GetObjectByNetworkId(id) as IMyStateGroup;
                int num3 = 0;
                num3 = !flag ? packet.BitStream.ReadInt16(0x10) : packet.BitStream.ReadInt32(0x20);
                if (objectByNetworkId == null)
                {
                    packet.BitStream.SetBitPositionRead(packet.BitStream.BitPosition + num3);
                    continue;
                }
                if (flag != objectByNetworkId.IsStreaming)
                {
                    MyLog.Default.WriteLine("received streaming flag but group is not streaming !");
                    packet.BitStream.SetBitPositionRead(packet.BitStream.BitPosition + num3);
                    continue;
                }
                int bytePosition = packet.BitStream.BytePosition;
                int bitPosition = packet.BitStream.BitPosition;
                if (MyCompilationSymbols.EnableNetworkPacketTracking)
                {
                    int num5 = packet.BitStream.BitPosition;
                    objectByNetworkId.Owner.ToString();
                    string fullName = objectByNetworkId.GetType().FullName;
                }
                MyStatsGraph.Begin(objectByNetworkId.GetType().Name, 0x7fffffff, "OnServerStateSync", 0x3e6, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
                objectByNetworkId.Serialize(packet.BitStream, this.m_clientState.EndpointId, serverTimestamp, this.m_lastClientTimestamp, this.m_lastStateSyncPacketId, 0, null);
                MyStatsGraph.End(new float?((float) (packet.BitStream.ByteLength - bytePosition)), 0f, "", "{0} B", null, "OnServerStateSync", 0x3e8, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            }
            if (!packet.BitStream.CheckTerminator())
            {
                MyLog.Default.WriteLine("OnServerStateSync: Invalid stream terminator");
            }
            packet.Return();
            if (this.m_clientPaused)
            {
                this.m_clientPaused = false;
                this.m_callback.PauseClient(false);
            }
        }

        public ServerDataMsg OnWorldData(MyPacket packet) => 
            MySerializer.CreateAndRead<ServerDataMsg>(packet.BitStream, null);

        private void PendingReplicableDestroy(NetworkId networkID, MyPendingReplicable pendingReplicable)
        {
            if (pendingReplicable.DependentReplicables != null)
            {
                foreach (KeyValuePair<NetworkId, MyPendingReplicable> pair in pendingReplicable.DependentReplicables)
                {
                    this.PendingReplicableDestroy(pair.Key, pair.Value);
                }
            }
            foreach (KeyValuePair<NetworkId, MyPendingReplicable> pair2 in this.m_pendingReplicables)
            {
                if (pair2.Value.ParentID.Equals(networkID))
                {
                    this.PendingReplicableDestroy(pair2.Key, pair2.Value);
                }
            }
            using (this.m_tmpGroups)
            {
                pendingReplicable.Replicable.GetStateGroups(this.m_tmpGroups);
                foreach (IMyStateGroup group in this.m_tmpGroups)
                {
                    if (group != null)
                    {
                        if (group.IsStreaming)
                        {
                            base.RemoveNetworkedObject(group);
                            int pendingStreamingRelicablesCount = this.PendingStreamingRelicablesCount;
                            this.PendingStreamingRelicablesCount = pendingStreamingRelicablesCount - 1;
                        }
                        group.Destroy();
                    }
                }
            }
            this.m_eventBuffer.RemoveEvents(networkID);
            this.m_pendingReplicables.Remove(networkID, false);
        }

        public void ProcessReplicationCreate(MyPacket packet)
        {
            TypeId typeId = packet.BitStream.ReadTypeId();
            NetworkId networkID = packet.BitStream.ReadNetworkId();
            NetworkId parentID = packet.BitStream.ReadNetworkId();
            byte num = packet.BitStream.ReadByte(8);
            if (!parentID.IsValid)
            {
                this.m_destroyedReplicables.Remove(networkID);
            }
            else if (!this.m_pendingReplicables.ContainsKey(parentID) && (base.GetObjectByNetworkId(parentID) == null))
            {
                packet.Return();
                return;
            }
            MyPendingReplicable replicable = new MyPendingReplicable {
                ParentID = parentID
            };
            for (int i = 0; i < num; i++)
            {
                NetworkId item = packet.BitStream.ReadNetworkId();
                replicable.StateGroupIds.Add(item);
            }
            IMyReplicable replicable = (IMyReplicable) Activator.CreateInstance(base.GetTypeByTypeId(typeId));
            replicable.Replicable = replicable;
            replicable.IsStreaming = false;
            if (!this.m_pendingReplicables.ContainsKey(networkID))
            {
                this.m_pendingReplicables.Add(networkID, replicable, false);
                this.m_pendingReplicables.ApplyAdditionsAndModifications();
            }
            replicable.OnLoad(packet.BitStream, loaded => this.SetReplicableReady(networkID, parentID, replicable, loaded));
            packet.Return();
        }

        public void ProcessReplicationCreateBegin(MyPacket packet)
        {
            TypeId typeId = packet.BitStream.ReadTypeId();
            NetworkId networkID = packet.BitStream.ReadNetworkId();
            NetworkId parentID = packet.BitStream.ReadNetworkId();
            byte num = packet.BitStream.ReadByte(8);
            MyPendingReplicable replicable = new MyPendingReplicable();
            for (int i = 0; i < num; i++)
            {
                NetworkId item = packet.BitStream.ReadNetworkId();
                replicable.StateGroupIds.Add(item);
            }
            IMyReplicable replicable = (IMyReplicable) Activator.CreateInstance(base.GetTypeByTypeId(typeId));
            replicable.Replicable = replicable;
            replicable.ParentID = parentID;
            if (!this.m_pendingReplicables.ContainsKey(networkID))
            {
                this.m_pendingReplicables.Add(networkID, replicable, false);
                this.m_pendingReplicables.ApplyAdditionsAndModifications();
            }
            List<NetworkId> stateGroupIds = replicable.StateGroupIds;
            IMyStreamableReplicable replicable2 = replicable as IMyStreamableReplicable;
            replicable.IsStreaming = true;
            replicable2.CreateStreamingStateGroup();
            int pendingStreamingRelicablesCount = this.PendingStreamingRelicablesCount;
            this.PendingStreamingRelicablesCount = pendingStreamingRelicablesCount + 1;
            this.AddNetworkObject(stateGroupIds[0], replicable2.GetStreamingStateGroup());
            replicable.StreamingGroupId = stateGroupIds[0];
            replicable2.OnLoadBegin(loaded => this.SetReplicableReady(networkID, parentID, replicable, loaded));
            packet.Return();
        }

        public void ProcessReplicationDestroy(MyPacket packet)
        {
            MyPendingReplicable replicable;
            NetworkId key = packet.BitStream.ReadNetworkId();
            if (!this.m_pendingReplicables.TryGetValue(key, out replicable))
            {
                IMyReplicable objectByNetworkId = base.GetObjectByNetworkId(key) as IMyReplicable;
                if (objectByNetworkId == null)
                {
                    goto TR_0000;
                }
                else
                {
                    using (this.m_tmp)
                    {
                        base.m_replicables.GetAllChildren(objectByNetworkId, this.m_tmp);
                        foreach (IMyReplicable replicable3 in this.m_tmp)
                        {
                            this.ReplicableDestroy(replicable3, true);
                        }
                        this.ReplicableDestroy(objectByNetworkId, true);
                        goto TR_0000;
                    }
                }
            }
            this.PendingReplicableDestroy(key, replicable);
            this.m_pendingReplicables.ApplyRemovals();
        TR_0000:
            this.m_destroyedReplicables.Add(key);
            packet.Return();
        }

        public void ProcessReplicationIslandDone(MyPacket packet)
        {
            byte index = packet.BitStream.ReadByte(8);
            int num2 = packet.BitStream.ReadInt32(0x20);
            Dictionary<long, MatrixD> matrices = new Dictionary<long, MatrixD>();
            for (int i = 0; i < num2; i++)
            {
                long key = packet.BitStream.ReadInt64(0x40);
                Vector3D vectord = packet.BitStream.ReadVector3D();
                Quaternion quaternion = packet.BitStream.ReadQuaternion();
                if (key != 0)
                {
                    MatrixD xd = MatrixD.CreateFromQuaternion(quaternion);
                    xd.Translation = vectord;
                    matrices.Add(key, xd);
                }
            }
            this.m_callback.SetIslandDone(index, matrices);
            packet.Return();
        }

        protected override void RemoveNetworkedObjectInternal(NetworkId networkID, IMyNetObject obj)
        {
            base.RemoveNetworkedObjectInternal(networkID, obj);
            IMyStateGroup item = obj as IMyStateGroup;
            if (item != null)
            {
                this.m_stateGroupsForUpdate.Remove(item, false);
            }
        }

        private void ReplicableDestroy(IMyReplicable replicable, bool removeNetworkObject = true)
        {
            NetworkId id;
            if (base.TryGetNetworkIdByObject(replicable, out id))
            {
                this.m_pendingReplicables.Remove(id, false);
                this.m_pendingReplicables.ApplyRemovals();
                this.m_eventBuffer.RemoveEvents(id);
            }
            using (this.m_tmpGroups)
            {
                replicable.GetStateGroups(this.m_tmpGroups);
                foreach (IMyStateGroup group in this.m_tmpGroups)
                {
                    if (group != null)
                    {
                        if (removeNetworkObject)
                        {
                            base.RemoveNetworkedObject(group);
                        }
                        group.Destroy();
                    }
                }
            }
            if (removeNetworkObject)
            {
                base.RemoveNetworkedObject(replicable);
            }
            replicable.OnDestroyClient();
            base.m_replicables.RemoveHierarchy(replicable);
        }

        public void RequestReplicable(long entityId, byte layer, bool add)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            bitStreamPacketData.Stream.WriteInt64(entityId, 0x40);
            bitStreamPacketData.Stream.WriteBool(add);
            if (add)
            {
                bitStreamPacketData.Stream.WriteByte(layer, 8);
            }
            this.m_callback.SendReplicableRequest(bitStreamPacketData);
        }

        public void SendClientConnected(ref ConnectedClientDataMsg msg)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            MySerializer.Write<ConnectedClientDataMsg>(bitStreamPacketData.Stream, ref msg, null);
            this.m_callback.SendConnectRequest(bitStreamPacketData);
        }

        public void SendClientReady(ref ClientReadyDataMsg msg)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            MySerializer.Write<ClientReadyDataMsg>(bitStreamPacketData.Stream, ref msg, null);
            this.m_callback.SendClientReady(bitStreamPacketData);
            this.OnLocalClientReady();
        }

        public override void SendUpdate()
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            bitStreamPacketData.Stream.WriteByte(this.m_lastStateSyncPacketId, 8);
            byte count = (byte) this.m_acks.Count;
            bitStreamPacketData.Stream.WriteByte(count, 8);
            for (int i = 0; i < count; i++)
            {
                bitStreamPacketData.Stream.WriteByte(this.m_acks[i], 8);
            }
            bitStreamPacketData.Stream.Terminate();
            this.m_acks.Clear();
            this.m_callback.SendClientAcks(bitStreamPacketData);
            bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            this.m_clientPacketId = (byte) (this.m_clientPacketId + 1);
            bitStreamPacketData.Stream.WriteByte(this.m_clientPacketId, 8);
            bitStreamPacketData.Stream.WriteDouble(this.Timestamp.Milliseconds);
            bitStreamPacketData.Stream.WriteDouble(MyTimeSpan.FromTicks(Stopwatch.GetTimestamp()).Milliseconds);
            bool enableNetworkPacketTracking = MyCompilationSymbols.EnableNetworkPacketTracking;
            this.m_clientState.Serialize(bitStreamPacketData.Stream, false);
            bitStreamPacketData.Stream.Terminate();
            this.m_callback.SendClientUpdate(bitStreamPacketData);
        }

        public void SetClientStatePing(short ping)
        {
            this.m_clientState.Ping = ping;
        }

        private void SetPing(MyTimeSpan ping)
        {
            this.m_ping = ping;
            this.UpdatePingSmoothing();
            this.m_callback.SetPing((long) this.m_smoothPing.Milliseconds);
            this.SetClientStatePing((short) this.m_smoothPing.Milliseconds);
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private void SetReplicableReady(NetworkId networkId, NetworkId parentId, IMyReplicable replicable, bool loaded)
        {
            try
            {
                MyPendingReplicable replicable2;
                if (!this.m_pendingReplicables.TryGetValue(networkId, out replicable2) || !ReferenceEquals(replicable2.Replicable, replicable))
                {
                    replicable.OnDestroyClient();
                }
                else if (!loaded)
                {
                    MyPendingReplicable replicable3;
                    if (!this.m_pendingReplicables.TryGetValue(parentId, out replicable3))
                    {
                        this.ReplicableDestroy(replicable, false);
                    }
                    else
                    {
                        if (replicable3.DependentReplicables == null)
                        {
                            replicable3.DependentReplicables = new Dictionary<NetworkId, MyPendingReplicable>();
                        }
                        replicable3.DependentReplicables.Add(networkId, replicable2);
                    }
                }
                else
                {
                    IMyReplicable replicable4;
                    this.m_pendingReplicables.Remove(networkId, false);
                    this.m_pendingReplicables.ApplyRemovals();
                    List<NetworkId> stateGroupIds = replicable2.StateGroupIds;
                    this.AddNetworkObject(networkId, replicable);
                    base.m_replicables.Add(replicable, out replicable4);
                    using (this.m_tmpGroups)
                    {
                        replicable.GetStateGroups(this.m_tmpGroups);
                        for (int i = 0; i < this.m_tmpGroups.Count; i++)
                        {
                            if (!this.m_tmpGroups[i].IsStreaming)
                            {
                                this.AddNetworkObject(stateGroupIds[i], this.m_tmpGroups[i]);
                            }
                            else
                            {
                                int pendingStreamingRelicablesCount = this.PendingStreamingRelicablesCount;
                                this.PendingStreamingRelicablesCount = pendingStreamingRelicablesCount - 1;
                            }
                        }
                    }
                    if (replicable2.DependentReplicables != null)
                    {
                        using (Dictionary<NetworkId, MyPendingReplicable>.Enumerator enumerator = replicable2.DependentReplicables.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                KeyValuePair<NetworkId, MyPendingReplicable> dependentReplicable;
                                dependentReplicable.Value.Replicable.Reload(dependentLoaded => this.SetReplicableReady(dependentReplicable.Key, networkId, dependentReplicable.Value.Replicable, dependentLoaded));
                            }
                        }
                    }
                    this.m_eventBuffer.ProcessEvents(networkId, this.m_eventHandler, this.m_isBlockedHandler, NetworkId.Invalid);
                    MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
                    bitStreamPacketData.Stream.WriteNetworkId(networkId);
                    bitStreamPacketData.Stream.WriteBool(loaded);
                    bitStreamPacketData.Stream.Terminate();
                    this.m_callback.SendReplicableReady(bitStreamPacketData);
                    Action<IMyReplicable> onReplicableReady = this.OnReplicableReady;
                    if (onReplicableReady != null)
                    {
                        onReplicableReady(replicable);
                    }
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine(exception);
                throw;
            }
        }

        public override void Simulate()
        {
            this.m_callback.UpdateSnapshotCache();
        }

        private void TimestampReset()
        {
            this.m_stateGroupsForUpdate.ApplyChanges();
            using (HashSet<IMyStateGroup>.Enumerator enumerator = this.m_stateGroupsForUpdate.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Reset(false, this.Timestamp);
                }
            }
        }

        public override void UpdateAfter()
        {
            if ((this.m_clientReady && this.m_hasTypeTable) && (this.m_clientState != null))
            {
                MyStatsGraph.ProfileAdvanced(true);
                MyStatsGraph.Begin("Replication client update", 0, "UpdateAfter", 0x1db, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
                this.UpdatePingSmoothing();
                MyStatsGraph.CustomTime("Ping", (float) this.m_ping.Milliseconds, "{0} ms", "UpdateAfter", 0x1de, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
                MyStatsGraph.CustomTime("SmoothPing", (float) this.m_smoothPing.Milliseconds, "{0} ms", "UpdateAfter", 0x1df, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
                switch (SynchronizationTimingType)
                {
                    case TimingType.ServerTimestep:
                        this.Timestamp = this.UpdateServerTimestep();
                        break;

                    case TimingType.LastServerTime:
                        this.Timestamp = this.UpdateLastServerTime();
                        break;

                    default:
                        break;
                }
                float? bytesTransfered = null;
                MyStatsGraph.End(bytesTransfered, 0f, "", "{0} B", null, "UpdateAfter", 0x1ed, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
                MyStatsGraph.ProfileAdvanced(false);
                if (StressSleep.X > 0)
                {
                    int num;
                    if (StressSleep.Z == 0)
                    {
                        num = MyRandom.Instance.Next(StressSleep.X, StressSleep.Y);
                    }
                    else
                    {
                        num = ((int) (Math.Sin((this.GetSimulationUpdateTime().Milliseconds * 3.1415926535897931) / ((double) StressSleep.Z)) * StressSleep.Y)) + StressSleep.X;
                    }
                    Thread.Sleep(num);
                }
                if (((MyTimeSpan.FromTicks(Stopwatch.GetTimestamp()) - this.m_lastServerTimeStampReceivedTime).Seconds > 5.0) && !this.m_clientPaused)
                {
                    this.m_clientPaused = true;
                    this.m_callback.PauseClient(true);
                }
            }
        }

        public override void UpdateBefore()
        {
        }

        public override void UpdateClientStateGroups()
        {
            this.m_stateGroupsForUpdate.ApplyChanges();
            foreach (IMyStateGroup group in this.m_stateGroupsForUpdate)
            {
                group.ClientUpdate(this.Timestamp);
                if (!group.NeedsUpdate)
                {
                    this.m_stateGroupsForUpdate.Remove(group, false);
                }
            }
        }

        private MyTimeSpan UpdateLastServerTime()
        {
            MyTimeSpan span7;
            float num4;
            MyTimeSpan span = MyTimeSpan.FromTicks(Stopwatch.GetTimestamp());
            MyTimeSpan span2 = span - this.m_lastClientTime;
            this.m_lastClientTime = span;
            MyTimeSpan span3 = span - this.m_lastServerTimeStampReceivedTime;
            MyTimeSpan span4 = this.m_lastServerTimestamp + span3;
            MyTimeSpan span5 = span4 - this.m_lastServerTime;
            this.m_lastServerTime = span4;
            MyStatsGraph.CustomTime("ClientTimeDelta", (float) span2.Milliseconds, "{0} ms", "UpdateLastServerTime", 0x21b, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            MyStatsGraph.CustomTime("ServerTimeDelta", (float) span5.Milliseconds, "{0} ms", "UpdateLastServerTime", 540, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            MyStatsGraph.CustomTime("TimeDeltaFromPacket", (float) span3.Milliseconds, "{0} ms", "UpdateLastServerTime", 0x21d, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            if (span3.Seconds > 1.0)
            {
                return this.Timestamp;
            }
            float seconds = (float) (this.Timestamp - span4).Seconds;
            MyStatsGraph.CustomTime("ServerClientTimeDiff", -seconds * 1000f, "{0} ms", "UpdateLastServerTime", 0x229, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            float serverSimulationRatio = this.m_callback.GetServerSimulationRatio();
            float num3 = ((float) ((int) ((this.m_callback.GetClientSimulationRatio() / serverSimulationRatio) * 50f))) / 50f;
            if (num3 > 0.8f)
            {
                num3 = 1f;
            }
            seconds += 0.06f * (0.6f - num3);
            if (seconds < -1f)
            {
                span7 = (span3.Seconds < 1.0) ? span4 : this.Timestamp;
                this.m_clientStartTimeStamp -= this.Timestamp - this.GetSimulationUpdateTime();
                return span7;
            }
            if (seconds > 0.2f)
            {
                this.m_clientStartTimeStamp -= this.Timestamp - this.GetSimulationUpdateTime();
                return this.Timestamp;
            }
            this.m_timeDiffSmoothed = MathHelper.Smooth(seconds, this.m_timeDiffSmoothed);
            seconds = this.m_timeDiffSmoothed;
            if (Math.Sign(seconds) > 0)
            {
                num4 = 1f / ((float) Math.Exp((double) (seconds * 6f)));
            }
            else
            {
                num4 = (float) Math.Exp((double) (((float) Math.Max((double) (-seconds - 0.04), (double) 0.0)) * 2f));
            }
            num4 = MathHelper.Clamp(num4, 0.1f, 10f);
            float customTime = Math.Max(Math.Min(num3, 1f), 0.1f);
            float num6 = (this.m_simulationTimeStep / customTime) * num4;
            MyStatsGraph.CustomTime("TimeAdvance", num6, "{0} ms", "UpdateLastServerTime", 0x265, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            MyStatsGraph.CustomTime("ServerClientSimRatio", customTime, "{0}", "UpdateLastServerTime", 0x266, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            span7 = this.Timestamp + MyTimeSpan.FromMilliseconds((double) num6);
            this.m_clientStartTimeStamp -= span7 - this.GetSimulationUpdateTime();
            float delay = (this.m_simulationTimeStep / serverSimulationRatio) - this.m_simulationTimeStep;
            if (delay > 0f)
            {
                this.m_callback.SetNextFrameDelayDelta(delay);
            }
            MyStatsGraph.CustomTime("FrameDelayTime", delay, "{0} ms", "UpdateLastServerTime", 0x275, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            return span7;
        }

        private void UpdatePingSmoothing()
        {
            MyTimeSpan span = MyTimeSpan.FromTicks(Stopwatch.GetTimestamp());
            double num = Math.Min((double) ((span - this.m_lastPingTime).Seconds / ((double) base.PingSmoothFactor)), (double) 1.0);
            this.m_smoothPing = MyTimeSpan.FromMilliseconds((this.m_ping.Milliseconds * num) + (this.m_smoothPing.Milliseconds * (1.0 - num)));
            this.m_lastPingTime = span;
        }

        private MyTimeSpan UpdateServerTimestep()
        {
            MyTimeSpan simulationUpdateTime = this.GetSimulationUpdateTime();
            MyTimeSpan span2 = base.UseSmoothPing ? this.m_smoothPing : this.m_ping;
            double num2 = simulationUpdateTime.Milliseconds - this.m_lastServerTimestamp.Milliseconds;
            int num4 = 0;
            int num5 = 0;
            MyTimeSpan span3 = MyTimeSpan.FromTicks(Stopwatch.GetTimestamp());
            MyTimeSpan span4 = span3 - this.m_lastTime;
            double num3 = (num2 + (-span2.Milliseconds * this.m_callback.GetServerSimulationRatio())) - this.m_simulationTimeStep;
            double num6 = Math.Min((double) (span4.Seconds / ((double) base.SmoothCorrectionAmplitude)), (double) 1.0);
            this.m_correctionSmooth = MyTimeSpan.FromMilliseconds((num3 * num6) + (this.m_correctionSmooth.Milliseconds * (1.0 - num6)));
            int num7 = (int) ((this.m_simulationTimeStep * 2f) / this.m_callback.GetServerSimulationRatio());
            num3 = Math.Min(num3, (double) num7);
            this.m_correctionSmooth = MyTimeSpan.FromMilliseconds(Math.Min(this.m_correctionSmooth.Milliseconds, (double) num7));
            MyStatsGraph.CustomTime("Correction", (float) num3, "{0} ms", "UpdateServerTimestep", 0x28f, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            MyStatsGraph.CustomTime("SmoothCorrection", (float) this.m_correctionSmooth.Milliseconds, "{0} ms", "UpdateServerTimestep", 0x290, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            if (((num2 < -80.0) || ((num2 > (500.0 + span2.Milliseconds)) && !this.m_predictionReset)) || (num2 > 5000.0))
            {
                this.m_clientStartTimeStamp = MyTimeSpan.FromMilliseconds(this.m_clientStartTimeStamp.Milliseconds + num2);
                simulationUpdateTime = this.GetSimulationUpdateTime();
                this.m_correctionSmooth = MyTimeSpan.Zero;
                if (this.m_predictionReset && (num2 > 5000.0))
                {
                    this.TimestampReset();
                }
                if (MyCompilationSymbols.EnableNetworkPositionTracking)
                {
                }
            }
            else
            {
                num4 = (num2 >= 0.0) ? (base.UseSmoothCorrection ? ((int) this.m_correctionSmooth.Milliseconds) : ((int) num3)) : ((int) num3);
                if ((base.LastMessageFromServer - DateTime.UtcNow).Seconds < 1f)
                {
                    if (num2 < 0.0)
                    {
                        num5 = num4;
                        this.m_callback.SetNextFrameDelayDelta((float) num5);
                    }
                    else if (Math.Abs(num4) > base.TimestampCorrectionMinimum)
                    {
                        num5 = (Math.Abs(num4) - base.TimestampCorrectionMinimum) * Math.Sign(num4);
                        this.m_callback.SetNextFrameDelayDelta((float) num5);
                    }
                }
            }
            MyStatsGraph.CustomTime("GameTimeDelta", (float) (simulationUpdateTime - this.Timestamp).Milliseconds, "{0} ms", "UpdateServerTimestep", 0x2b5, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            MyStatsGraph.CustomTime("RealTimeDelta", (float) span4.Milliseconds, "{0} ms", "UpdateServerTimestep", 0x2b6, @"E:\Repo1\Sources\VRage\Replication\MyReplicationClient.cs");
            object[] objArray1 = new object[0x16];
            objArray1[0] = "realtime delta: ";
            objArray1[1] = span4;
            objArray1[2] = ", client: ";
            objArray1[3] = this.Timestamp;
            objArray1[4] = ", server: ";
            objArray1[5] = this.m_lastServerTimestamp;
            objArray1[6] = ", diff: ";
            objArray1[7] = num2.ToString("##.#");
            objArray1[8] = " => ";
            objArray1[9] = (this.Timestamp.Milliseconds - this.m_lastServerTimestamp.Milliseconds).ToString("##.#");
            objArray1[10] = ", Ping: ";
            objArray1[11] = this.m_ping.Milliseconds.ToString("##.#");
            objArray1[12] = " / ";
            objArray1[13] = this.m_smoothPing.Milliseconds.ToString("##.#");
            objArray1[14] = "ms, Correction ";
            objArray1[15] = num3;
            objArray1[0x10] = " / ";
            objArray1[0x11] = this.m_correctionSmooth.Milliseconds;
            objArray1[0x12] = " / ";
            objArray1[0x13] = num5;
            objArray1[20] = ", ratio ";
            objArray1[0x15] = this.m_callback.GetServerSimulationRatio();
            string.Concat(objArray1);
            this.m_lastTime = span3;
            return simulationUpdateTime;
        }

        public override void UpdateStatisticsData(int outgoing, int incoming, int tamperred, float gcMemory, float processMemory)
        {
        }

        public MyTimeSpan Timestamp { get; private set; }

        public int PendingStreamingRelicablesCount { get; private set; }

        public MyTimeSpan Ping =>
            (base.UseSmoothPing ? this.m_smoothPing : this.m_ping);

        public enum TimingType
        {
            None,
            ServerTimestep,
            LastServerTime
        }
    }
}

