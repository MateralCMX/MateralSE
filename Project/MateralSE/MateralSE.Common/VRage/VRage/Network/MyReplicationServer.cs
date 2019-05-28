namespace VRage.Network
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Replication;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    public class MyReplicationServer : MyReplicationLayer
    {
        private const int MAX_NUM_STATE_SYNC_PACKETS_PER_CLIENT = 7;
        private readonly IReplicationServerCallback m_callback;
        private readonly CacheList<IMyStateGroup> m_tmpGroups;
        private HashSet<IMyReplicable> m_toRecalculateHash;
        private readonly List<IMyReplicable> m_tmpReplicableList;
        private readonly CacheList<IMyReplicable> m_tmp;
        private readonly HashSet<IMyReplicable> m_lastLayerAdditions;
        private readonly CachingHashSet<IMyReplicable> m_postponedDestructionReplicables;
        private readonly ConcurrentCachingHashSet<IMyReplicable> m_priorityUpdates;
        private MyTimeSpan m_serverTimeStamp;
        private long m_serverFrame;
        private readonly ConcurrentQueue<IMyStateGroup> m_dirtyGroups;
        public static SerializableVector3I StressSleep = new SerializableVector3I(0, 0, 0);
        private readonly Dictionary<IMyReplicable, List<IMyStateGroup>> m_replicableGroups;
        private readonly ConcurrentDictionary<Endpoint, MyClient> m_clientStates;
        private readonly ConcurrentDictionary<Endpoint, MyTimeSpan> m_recentClientsStates;
        private readonly HashSet<Endpoint> m_recentClientStatesToRemove;
        private readonly MyTimeSpan SAVED_CLIENT_DURATION;
        [ThreadStatic]
        private static List<EndpointId> m_recipients;
        private readonly List<EndpointId> m_endpoints;

        public MyReplicationServer(IReplicationServerCallback callback, EndpointId localEndpoint, Thread mainThread) : base(true, localEndpoint, mainThread)
        {
            this.m_tmpGroups = new CacheList<IMyStateGroup>(4);
            this.m_toRecalculateHash = new HashSet<IMyReplicable>();
            this.m_tmpReplicableList = new List<IMyReplicable>();
            this.m_tmp = new CacheList<IMyReplicable>();
            this.m_lastLayerAdditions = new HashSet<IMyReplicable>();
            this.m_postponedDestructionReplicables = new CachingHashSet<IMyReplicable>();
            this.m_priorityUpdates = new ConcurrentCachingHashSet<IMyReplicable>();
            this.m_serverTimeStamp = MyTimeSpan.Zero;
            this.m_dirtyGroups = new ConcurrentQueue<IMyStateGroup>();
            this.m_replicableGroups = new Dictionary<IMyReplicable, List<IMyStateGroup>>();
            this.m_clientStates = new ConcurrentDictionary<Endpoint, MyClient>();
            this.m_recentClientsStates = new ConcurrentDictionary<Endpoint, MyTimeSpan>();
            this.m_recentClientStatesToRemove = new HashSet<Endpoint>();
            this.SAVED_CLIENT_DURATION = MyTimeSpan.FromSeconds(60.0);
            this.m_endpoints = new List<EndpointId>();
            this.m_callback = callback;
            base.m_replicables = new MyReplicablesAABB(mainThread);
        }

        public void AddClient(Endpoint endpoint, MyClientStateBase clientState)
        {
            if (!this.m_clientStates.ContainsKey(endpoint))
            {
                clientState.EndpointId = endpoint;
                this.m_clientStates.TryAdd(endpoint, new MyClient(clientState, this.m_callback));
            }
        }

        private void AddClientReplicable(IMyReplicable replicable, MyClient client, bool force)
        {
            client.Replicables.Add(replicable, new MyReplicableClientData());
            client.PendingReplicables++;
            if (this.m_replicableGroups.ContainsKey(replicable))
            {
                foreach (IMyStateGroup group in this.m_replicableGroups[replicable])
                {
                    NetworkId networkIdByObject = base.GetNetworkIdByObject(group);
                    if (!group.IsStreaming || (replicable as IMyStreamableReplicable).NeedsToBeStreamed)
                    {
                        client.StateGroups.Add(group, new MyStateDataEntry(replicable, networkIdByObject, group));
                        this.ScheduleStateGroupSync(client, client.StateGroups[group], base.SyncFrameCounter);
                        group.CreateClientData(client.State);
                        if (force)
                        {
                            group.ForceSend(client.State);
                        }
                    }
                }
            }
        }

        private void AddCrucialReplicable(MyClient client, IMyReplicable replicable)
        {
            client.CrucialReplicables.Add(replicable);
            HashSet<IMyReplicable> physicalDependencies = replicable.GetPhysicalDependencies(this.m_serverTimeStamp, base.m_replicables);
            if (physicalDependencies != null)
            {
                foreach (IMyReplicable replicable2 in physicalDependencies)
                {
                    client.CrucialReplicables.Add(replicable2);
                }
            }
        }

        private void AddForClient(IMyReplicable replicable, Endpoint clientEndpoint, MyClient client, bool force, bool addDependencies = false)
        {
            if (((replicable.IsReadyForReplication && !client.HasReplicable(replicable)) && replicable.ShouldReplicate(new MyClientInfo(client))) && replicable.IsValid)
            {
                this.AddClientReplicable(replicable, client, force);
                this.SendReplicationCreate(replicable, client, clientEndpoint);
                if (!(replicable is IMyStreamableReplicable))
                {
                    foreach (IMyReplicable replicable2 in base.m_replicables.GetChildren(replicable))
                    {
                        this.AddForClient(replicable2, clientEndpoint, client, force, false);
                    }
                }
                if (this.GetClientReplicableIslandIndex(replicable, clientEndpoint) == 0)
                {
                    HashSet<IMyReplicable> physicalDependencies = replicable.GetPhysicalDependencies(this.m_serverTimeStamp, base.m_replicables);
                    if ((physicalDependencies != null) && (physicalDependencies.Count > 0))
                    {
                        client.TryCreateNewCachedIsland(replicable, physicalDependencies);
                        if (addDependencies)
                        {
                            MyClientInfo info1 = new MyClientInfo(client);
                            foreach (IMyReplicable replicable3 in physicalDependencies)
                            {
                                this.AddForClient(replicable3, clientEndpoint, client, force, false);
                            }
                        }
                    }
                }
            }
        }

        private void AddReplicableToLayer(IMyReplicable rep, MyClient.UpdateLayer layer, MyClient client, bool addDependencies)
        {
            if (!this.IsReplicableInPreviousLayer(rep, layer, client))
            {
                this.AddReplicableToLayerSingle(rep, layer, client, true);
                if (addDependencies)
                {
                    HashSet<IMyReplicable> physicalDependencies = rep.GetPhysicalDependencies(this.m_serverTimeStamp, base.m_replicables);
                    if (physicalDependencies != null)
                    {
                        foreach (IMyReplicable replicable in physicalDependencies)
                        {
                            if (!this.IsReplicableInPreviousLayer(replicable, layer, client))
                            {
                                this.AddReplicableToLayerSingle(replicable, layer, client, true);
                            }
                        }
                    }
                }
            }
        }

        private void AddReplicableToLayerSingle(IMyReplicable rep, MyClient.UpdateLayer layer, MyClient client, bool removeFromDelete = true)
        {
            layer.Replicables.Add(rep);
            layer.Sender.List.Add(rep);
            client.ReplicableToLayer[rep] = layer;
            if (removeFromDelete)
            {
                this.m_toRecalculateHash.Remove(rep);
            }
            HashSet<IMyReplicable> dependencies = rep.GetDependencies(false);
            if (dependencies != null)
            {
                foreach (IMyReplicable replicable in dependencies)
                {
                    this.m_lastLayerAdditions.Add(replicable);
                }
            }
        }

        private void AddStateGroups(IMyReplicable replicable)
        {
            using (this.m_tmpGroups)
            {
                IMyStreamableReplicable replicable2 = replicable as IMyStreamableReplicable;
                if (replicable2 != null)
                {
                    replicable2.CreateStreamingStateGroup();
                }
                replicable.GetStateGroups(this.m_tmpGroups);
                foreach (IMyStateGroup group in this.m_tmpGroups)
                {
                    base.AddNetworkObjectServer(group);
                }
                this.m_replicableGroups.Add(replicable, new List<IMyStateGroup>(this.m_tmpGroups));
            }
        }

        public void AddToDirtyGroups(IMyStateGroup group)
        {
            if (group.Owner.IsReadyForReplication)
            {
                this.m_dirtyGroups.Enqueue(group);
            }
        }

        private void ApplyDirtyGroups()
        {
            IMyStateGroup group;
            while (this.m_dirtyGroups.TryDequeue(out group))
            {
                IEnumerator<KeyValuePair<Endpoint, MyClient>> enumerator = this.m_clientStates.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        MyStateDataEntry entry;
                        KeyValuePair<Endpoint, MyClient> current = enumerator.Current;
                        if (current.Value.StateGroups.TryGetValue(group, out entry))
                        {
                            this.ScheduleStateGroupSync(current.Value, entry, base.SyncFrameCounter);
                        }
                    }
                }
                finally
                {
                    if (enumerator == null)
                    {
                        continue;
                    }
                    enumerator.Dispose();
                }
            }
        }

        public override MyPacketStatistics ClearClientStatistics()
        {
            MyPacketStatistics statistics = new MyPacketStatistics();
            foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
            {
                statistics.Add(pair.Value.Statistics);
            }
            return statistics;
        }

        public override MyPacketStatistics ClearServerStatistics() => 
            new MyPacketStatistics();

        public void Destroy(IMyReplicable obj)
        {
            if ((base.IsTypeReplicated(obj.GetType()) && (obj.IsReadyForReplication && ((obj.ReadyForReplicationAction == null) || (obj.ReadyForReplicationAction.Count <= 0)))) && !base.GetNetworkIdByObject(obj).IsInvalid)
            {
                this.m_priorityUpdates.Remove(obj, false);
                this.m_priorityUpdates.ApplyChanges();
                bool isAnyClientPending = false;
                bool flag = (obj.GetParent() != null) && !obj.GetParent().IsValid;
                this.RemoveForClients(obj, delegate (MyClient client) {
                    if (!client.BlockedReplicables.ContainsKey(obj))
                    {
                        client.PermanentReplicables.Remove(obj);
                        client.CrucialReplicables.Remove(obj);
                        client.RemoveReplicableFromIslands(obj);
                        return true;
                    }
                    client.BlockedReplicables[obj].Remove = true;
                    if (!obj.HasToBeChild && !this.m_priorityUpdates.Contains(obj))
                    {
                        this.m_priorityUpdates.Add(obj);
                    }
                    isAnyClientPending = true;
                    return false;
                }, !flag);
                base.m_replicables.RemoveHierarchy(obj);
                if (isAnyClientPending)
                {
                    this.m_postponedDestructionReplicables.Add(obj);
                }
                else
                {
                    this.RemoveStateGroups(obj);
                    base.RemoveNetworkedObject(obj);
                    this.m_postponedDestructionReplicables.Remove(obj, false);
                    obj.OnRemovedFromReplication();
                }
            }
        }

        public override void Disconnect()
        {
            foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
            {
                this.RemoveClient(pair.Key);
            }
        }

        protected override bool DispatchBlockingEvent(IPacketData data, CallSite site, EndpointId target, IMyNetObject targetReplicable, Vector3D? position, IMyNetObject blockingReplicable)
        {
            Endpoint key = new Endpoint(target, 0);
            IMyReplicable replicable = blockingReplicable as IMyReplicable;
            IMyReplicable replicable2 = targetReplicable as IMyReplicable;
            if (site.HasBroadcastFlag || site.HasBroadcastExceptFlag)
            {
                foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
                {
                    if ((!site.HasBroadcastExceptFlag || (pair.Key.Id != target)) && ((pair.Key.Index == 0) && this.ShouldSendEvent(targetReplicable, position, pair.Value)))
                    {
                        TryAddBlockerForClient(pair.Value, replicable2, replicable);
                    }
                }
            }
            else
            {
                MyClient client;
                if ((site.HasClientFlag && (base.m_localEndpoint != target)) && this.m_clientStates.TryGetValue(key, out client))
                {
                    TryAddBlockerForClient(client, replicable2, replicable);
                }
            }
            return this.DispatchEvent(data, site, target, targetReplicable, position);
        }

        private void DispatchEvent(IPacketData data, List<EndpointId> recipients, bool reliable)
        {
            this.m_callback.SendEvent(data, reliable, recipients);
        }

        private void DispatchEvent(IPacketData data, MyClient client, bool reliable)
        {
            m_recipients.Add(client.State.EndpointId.Id);
            this.DispatchEvent(data, m_recipients, reliable);
            m_recipients.Clear();
        }

        protected override bool DispatchEvent(IPacketData data, CallSite site, EndpointId target, IMyNetObject eventInstance, Vector3D? position)
        {
            if (m_recipients == null)
            {
                m_recipients = new List<EndpointId>();
            }
            Endpoint key = new Endpoint(target, 0);
            bool flag = false;
            if (!site.HasBroadcastFlag && !site.HasBroadcastExceptFlag)
            {
                MyClient client;
                if ((site.HasClientFlag && ((base.m_localEndpoint != target) && this.m_clientStates.TryGetValue(key, out client))) && this.ShouldSendEvent(eventInstance, position, client))
                {
                    this.DispatchEvent(data, client, site.IsReliable);
                    flag = true;
                }
            }
            else
            {
                foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
                {
                    if ((!site.HasBroadcastExceptFlag || (pair.Key.Id != target)) && ((pair.Key.Index == 0) && this.ShouldSendEvent(eventInstance, position, pair.Value)))
                    {
                        m_recipients.Add(pair.Key.Id);
                    }
                }
                if (m_recipients.Count > 0)
                {
                    this.DispatchEvent(data, m_recipients, site.IsReliable);
                    flag = true;
                    m_recipients.Clear();
                }
            }
            if (!flag)
            {
                data.Return();
            }
            return ShouldServerInvokeLocally(site, base.m_localEndpoint, target);
        }

        private void FilterStateSync(MyClient client)
        {
            if (client.IsAckAvailable())
            {
                this.ApplyDirtyGroups();
                int num = 0;
                MyPacketDataBitStreamBase base2 = null;
                List<MyStateDataEntry> list = PoolManager.Get<List<MyStateDataEntry>>();
                int mTUSize = this.m_callback.GetMTUSize(client.State.EndpointId);
                int count = client.DirtyQueue.Count;
                int num4 = 7;
                MyStateDataEntry entry = null;
                while (true)
                {
                    count--;
                    if (((count <= 0) || (num4 <= 0)) || (client.DirtyQueue.First.Priority >= base.SyncFrameCounter))
                    {
                        break;
                    }
                    MyStateDataEntry item = client.DirtyQueue.Dequeue();
                    list.Add(item);
                    if ((item.Owner != null) && !item.Group.IsStreaming)
                    {
                        MyReplicableClientData data;
                        if (!client.Replicables.TryGetValue(item.Owner, out data))
                        {
                            continue;
                        }
                        if (!data.HasActiveStateSync)
                        {
                            continue;
                        }
                    }
                    if (item.Group.IsStreaming)
                    {
                        if ((entry != null) || (item.Group.IsProcessingForClient(client.State.EndpointId) == MyStreamProcessingState.Processing))
                        {
                            this.ScheduleStateGroupSync(client, item, base.SyncFrameCounter);
                        }
                        else
                        {
                            entry = item;
                        }
                    }
                    else
                    {
                        if (!client.SendStateSync(item, mTUSize, ref base2, this.m_serverTimeStamp))
                        {
                            break;
                        }
                        num++;
                        if (base2 == null)
                        {
                            num4--;
                        }
                    }
                }
                if (base2 != null)
                {
                    base2.Stream.Terminate();
                    this.m_callback.SendStateSync(base2, client.State.EndpointId, false);
                }
                if (entry != null)
                {
                    this.SendStreamingEntry(client, entry);
                }
                client.UpdateIslands();
                long syncFrameCounter = base.SyncFrameCounter;
                foreach (MyStateDataEntry entry3 in list)
                {
                    if (entry3.Group.IsStillDirty(client.State.EndpointId))
                    {
                        this.ScheduleStateGroupSync(client, entry3, syncFrameCounter);
                    }
                }
            }
        }

        public void ForceEverything(Endpoint clientEndpoint)
        {
            base.m_replicables.IterateRoots(replicable => this.ForceReplicable(replicable, clientEndpoint));
        }

        private void ForceReplicable(IMyReplicable obj, Endpoint clientEndpoint)
        {
            if ((((base.m_localEndpoint != clientEndpoint.Id) && !clientEndpoint.Id.IsNull) && (obj != null)) && this.m_clientStates.ContainsKey(clientEndpoint))
            {
                MyClient client = this.m_clientStates[clientEndpoint];
                if (!client.Replicables.ContainsKey(obj))
                {
                    this.AddForClient(obj, clientEndpoint, client, true, false);
                }
            }
        }

        public void ForceReplicable(IMyReplicable obj, IMyReplicable parent = null)
        {
            if (obj != null)
            {
                foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
                {
                    if (((parent == null) || pair.Value.Replicables.ContainsKey(parent)) && !pair.Value.Replicables.ContainsKey(obj))
                    {
                        this.RefreshReplicable(obj, pair.Key, pair.Value, true);
                    }
                }
            }
        }

        protected override MyPacketDataBitStreamBase GetBitStreamPacketData() => 
            this.m_callback.GetBitStreamPacketData();

        public override MyClientStateBase GetClientData(Endpoint endpointId)
        {
            MyClient client;
            return (this.m_clientStates.TryGetValue(endpointId, out client) ? client.State : null);
        }

        public void GetClientPings(out SerializableDictionary<ulong, short> pings)
        {
            pings = new SerializableDictionary<ulong, short>();
            foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
            {
                pings[pair.Key.Id.Value] = pair.Value.State.Ping;
            }
        }

        public MyTimeSpan GetClientRelevantServerTimestamp(Endpoint clientEndpoint) => 
            this.m_serverTimeStamp;

        public byte GetClientReplicableIslandIndex(IMyReplicable replicable, Endpoint clientEndpoint) => 
            this.m_clientStates[clientEndpoint].GetReplicableIslandIndex(replicable);

        public override string GetMultiplayerStat()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(base.GetMultiplayerStat());
            builder.AppendLine("Client state info:");
            foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
            {
                object[] objArray1 = new object[] { "    Endpoint: ", pair.Key, ", Blocked Close Msgs Count: ", pair.Value.BlockedReplicables.Count };
                string str2 = string.Concat(objArray1);
                builder.AppendLine(str2);
            }
            return builder.ToString();
        }

        public override MyTimeSpan GetSimulationUpdateTime() => 
            this.m_serverTimeStamp;

        public void InvalidateClientCache(IMyReplicable replicable, string storageName)
        {
            foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
            {
                if (pair.Value.RemoveCache(replicable, storageName))
                {
                    this.m_callback.SendVoxelCacheInvalidated(storageName, pair.Key.Id);
                }
            }
        }

        public void InvalidateSingleClientCache(string storageName, EndpointId clientId)
        {
            foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
            {
                if (pair.Key.Id == clientId)
                {
                    pair.Value.RemoveCache(null, storageName);
                }
            }
        }

        private bool IsReplicableInPreviousLayer(IMyReplicable rep, MyClient.UpdateLayer layer, MyClient client)
        {
            int index = 0;
            while (true)
            {
                MyClient.UpdateLayer objA = client.UpdateLayers[index];
                index++;
                if (objA.Replicables.Contains(rep))
                {
                    this.m_toRecalculateHash.Remove(rep);
                    return true;
                }
                if (ReferenceEquals(objA, layer))
                {
                    return false;
                }
            }
        }

        public void OnClientAcks(MyPacket packet)
        {
            MyClient client;
            if (this.m_clientStates.TryGetValue(packet.Sender, out client))
            {
                client.OnClientAcks(packet);
                packet.Return();
            }
        }

        public ConnectedClientDataMsg OnClientConnected(MyPacket packet) => 
            MySerializer.CreateAndRead<ConnectedClientDataMsg>(packet.BitStream, null);

        private void OnClientConnected(EndpointId endpointId, MyClientStateBase clientState)
        {
            this.AddClient(new Endpoint(endpointId, 0), clientState);
        }

        public void OnClientJoined(EndpointId endpointId, MyClientStateBase clientState)
        {
            this.OnClientConnected(endpointId, clientState);
        }

        public void OnClientLeft(EndpointId endpointId)
        {
            while (true)
            {
                bool flag = false;
                foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
                {
                    if (pair.Key.Id == endpointId)
                    {
                        flag = true;
                        this.RemoveClient(pair.Key);
                        break;
                    }
                }
                if (!flag)
                {
                    return;
                }
            }
        }

        public void OnClientReady(Endpoint endpointId, ref ClientReadyDataMsg msg)
        {
            MyClient client;
            if (this.m_clientStates.TryGetValue(endpointId, out client))
            {
                client.IsReady = true;
                client.UsePlayoutDelayBufferForCharacter = msg.UsePlayoutDelayBufferForCharacter;
                client.UsePlayoutDelayBufferForJetpack = msg.UsePlayoutDelayBufferForJetpack;
                client.UsePlayoutDelayBufferForGrids = msg.UsePlayoutDelayBufferForGrids;
            }
        }

        public void OnClientReady(Endpoint endpointId, MyPacket packet)
        {
            ClientReadyDataMsg msg = MySerializer.CreateAndRead<ClientReadyDataMsg>(packet.BitStream, null);
            this.OnClientReady(endpointId, ref msg);
            this.SendServerData(endpointId);
        }

        public void OnClientUpdate(MyPacket packet)
        {
            MyClient client;
            if (this.m_clientStates.TryGetValue(packet.Sender, out client))
            {
                client.OnClientUpdate(packet, this.m_serverTimeStamp);
            }
        }

        protected override void OnEvent(MyPacketDataBitStreamBase data, CallSite site, object obj, IMyNetObject sendAs, Vector3D? position, EndpointId source)
        {
            MyClientStateBase clientData = this.GetClientData(new Endpoint(source, 0));
            if (clientData == null)
            {
                data.Return();
            }
            else if (site.HasServerInvokedFlag)
            {
                data.Return();
                this.m_callback.ValidationFailed(source.Value, true, "ServerInvoked " + site.ToString(), false);
            }
            else
            {
                IMyReplicable replicable = sendAs as IMyReplicable;
                if (replicable != null)
                {
                    ValidationResult result = replicable.HasRights(source, site.ValidationFlags);
                    if (result != ValidationResult.Passed)
                    {
                        data.Return();
                        this.m_callback.ValidationFailed(source.Value, result.HasFlag(ValidationResult.Kick), result.ToString() + " " + site.ToString(), false);
                        return;
                    }
                }
                if (!base.Invoke(site, data.Stream, obj, source, clientData, true))
                {
                    data.Return();
                }
                else
                {
                    if (!data.Stream.CheckTerminator())
                    {
                        throw new EndOfStreamException("Invalid BitStream terminator");
                    }
                    if ((site.HasClientFlag || site.HasBroadcastFlag) || site.HasBroadcastExceptFlag)
                    {
                        this.DispatchEvent(data, site, source, sendAs, position);
                    }
                    else
                    {
                        data.Return();
                    }
                }
            }
        }

        private bool ProcessBlocker(IMyReplicable replicable, Endpoint endpoint, MyClient client, IMyReplicable parent)
        {
            if (client.BlockedReplicables.ContainsKey(replicable))
            {
                MyDestroyBlocker blocker = client.BlockedReplicables[replicable];
                if (blocker.IsProcessing)
                {
                    return true;
                }
                blocker.IsProcessing = true;
                using (List<IMyReplicable>.Enumerator enumerator = blocker.Blockers.GetEnumerator())
                {
                    while (true)
                    {
                        bool flag2;
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        IMyReplicable current = enumerator.Current;
                        if (!client.IsReplicableReady(replicable) || !client.IsReplicableReady(current))
                        {
                            blocker.IsProcessing = false;
                            flag2 = false;
                        }
                        else
                        {
                            bool flag = true;
                            if (!ReferenceEquals(current, parent))
                            {
                                flag = this.ProcessBlocker(current, endpoint, client, replicable);
                            }
                            if (flag)
                            {
                                continue;
                            }
                            blocker.IsProcessing = false;
                            flag2 = false;
                        }
                        return flag2;
                    }
                }
                client.BlockedReplicables.Remove(replicable);
                if (blocker.Remove)
                {
                    this.RemoveForClient(replicable, client, true);
                }
                blocker.IsProcessing = false;
            }
            return true;
        }

        private void RefreshReplicable(IMyReplicable replicable, Endpoint endPoint, MyClient client, bool force = false)
        {
            if (!replicable.IsSpatial)
            {
                IMyReplicable parent = replicable.GetParent();
                if (parent == null)
                {
                    if (client.Replicables.ContainsKey(replicable) && !client.BlockedReplicables.ContainsKey(replicable))
                    {
                        this.RemoveForClient(replicable, client, true);
                    }
                }
                else if (client.HasReplicable(parent))
                {
                    this.AddForClient(replicable, endPoint, client, force, false);
                }
            }
            else
            {
                bool flag = true;
                if (replicable.HasToBeChild)
                {
                    IMyReplicable parent = replicable.GetParent();
                    if (parent != null)
                    {
                        flag = client.HasReplicable(parent);
                    }
                }
                MyClient.UpdateLayer layerOfReplicable = client.GetLayerOfReplicable(replicable);
                if ((layerOfReplicable != null) & flag)
                {
                    this.AddForClient(replicable, endPoint, client, force, false);
                    this.AddReplicableToLayerSingle(replicable, layerOfReplicable, client, false);
                }
                else
                {
                    int num1;
                    if (ReferenceEquals(replicable, client.State.ControlledReplicable) || ReferenceEquals(replicable, client.State.CharacterReplicable))
                    {
                        num1 = 1;
                    }
                    else
                    {
                        num1 = (int) client.CrucialReplicables.Contains(replicable);
                    }
                    if ((num1 & flag) != 0)
                    {
                        this.AddReplicableToLayerSingle(replicable, client.UpdateLayers[0], client, false);
                        this.AddForClient(replicable, endPoint, client, force, false);
                    }
                    else if (client.Replicables.ContainsKey(replicable) && !client.BlockedReplicables.ContainsKey(replicable))
                    {
                        this.RemoveForClient(replicable, client, true);
                    }
                }
            }
        }

        private void RemoveClient(Endpoint endpoint)
        {
            MyClient client;
            if (this.m_clientStates.TryGetValue(endpoint, out client))
            {
                while (true)
                {
                    if (client.Replicables.Count <= 0)
                    {
                        this.m_clientStates.Remove<Endpoint, MyClient>(endpoint);
                        this.m_recentClientsStates[endpoint] = this.m_callback.GetUpdateTime() + this.SAVED_CLIENT_DURATION;
                        break;
                    }
                    KeyValuePair<IMyReplicable, MyReplicableClientData> pair = client.Replicables.FirstPair();
                    this.RemoveForClient(pair.Key, client, false);
                }
            }
        }

        private void RemoveClientReplicable(IMyReplicable replicable, MyClient client)
        {
            if (this.m_replicableGroups.ContainsKey(replicable))
            {
                using (this.m_tmpGroups)
                {
                    MyReplicableClientData data;
                    replicable.GetStateGroups(this.m_tmpGroups);
                    foreach (IMyStateGroup group in this.m_replicableGroups[replicable])
                    {
                        group.DestroyClientData(client.State);
                        if (client.StateGroups.ContainsKey(group))
                        {
                            if (client.DirtyQueue.Contains(client.StateGroups[group]))
                            {
                                client.DirtyQueue.Remove(client.StateGroups[group]);
                            }
                            client.StateGroups.Remove(group);
                        }
                    }
                    if (client.Replicables.TryGetValue(replicable, out data) && data.IsPending)
                    {
                        client.PendingReplicables--;
                    }
                    client.Replicables.Remove(replicable);
                    this.m_tmpGroups.Clear();
                }
            }
        }

        private void RemoveForClient(IMyReplicable replicable, MyClient client, bool sendDestroyToClient)
        {
            if (m_recipients == null)
            {
                m_recipients = new List<EndpointId>();
            }
            using (this.m_tmp)
            {
                base.m_replicables.GetAllChildren(replicable, this.m_tmp);
                this.m_tmp.Add(replicable);
                this.RemoveForClientInternal(replicable, client);
                if (sendDestroyToClient)
                {
                    m_recipients.Add(client.State.EndpointId.Id);
                    this.SendReplicationDestroy(replicable, m_recipients);
                    m_recipients.Clear();
                }
            }
        }

        public void RemoveForClientIfIncomplete(IMyEventProxy objA)
        {
            if (objA != null)
            {
                IMyReplicable replicableA = base.GetProxyTarget(objA) as IMyReplicable;
                this.RemoveForClients(replicableA, x => x.IsReplicablePending(replicableA), true);
            }
        }

        private void RemoveForClientInternal(IMyReplicable replicable, MyClient client)
        {
            foreach (IMyReplicable replicable2 in this.m_tmp)
            {
                client.BlockedReplicables.Remove(replicable2);
                this.RemoveClientReplicable(replicable2, client);
            }
            MyClient.UpdateLayer[] updateLayers = client.UpdateLayers;
            for (int i = 0; i < updateLayers.Length; i++)
            {
                updateLayers[i].Replicables.Remove(replicable);
            }
        }

        private void RemoveForClients(IMyReplicable replicable, Func<MyClient, bool> validate, bool sendDestroyToClient)
        {
            using (this.m_tmp)
            {
                if (m_recipients == null)
                {
                    m_recipients = new List<EndpointId>();
                }
                bool flag = true;
                foreach (MyClient client in this.m_clientStates.Values)
                {
                    if (validate(client))
                    {
                        if (flag)
                        {
                            base.m_replicables.GetAllChildren(replicable, this.m_tmp);
                            this.m_tmp.Add(replicable);
                            flag = false;
                        }
                        this.RemoveForClientInternal(replicable, client);
                        if (sendDestroyToClient)
                        {
                            m_recipients.Add(client.State.EndpointId.Id);
                        }
                    }
                }
                if (m_recipients.Count > 0)
                {
                    this.SendReplicationDestroy(replicable, m_recipients);
                    m_recipients.Clear();
                }
            }
        }

        private void RemoveStateGroups(IMyReplicable replicable)
        {
            foreach (MyClient client in this.m_clientStates.Values)
            {
                this.RemoveClientReplicable(replicable, client);
            }
            foreach (IMyStateGroup group in this.m_replicableGroups[replicable])
            {
                base.RemoveNetworkedObject(group);
                group.Destroy();
            }
            this.m_replicableGroups.Remove(replicable);
        }

        public void ReplicableReady(MyPacket packet)
        {
            MyClient client;
            NetworkId id = packet.BitStream.ReadNetworkId();
            bool flag = packet.BitStream.ReadBool();
            if (!packet.BitStream.CheckTerminator())
            {
                throw new EndOfStreamException("Invalid BitStream terminator");
            }
            if (this.m_clientStates.TryGetValue(packet.Sender, out client))
            {
                IMyReplicable objectByNetworkId = base.GetObjectByNetworkId(id) as IMyReplicable;
                if (objectByNetworkId != null)
                {
                    MyReplicableClientData data;
                    if (!client.Replicables.TryGetValue(objectByNetworkId, out data))
                    {
                        if (!flag)
                        {
                            this.RemoveForClient(objectByNetworkId, client, false);
                        }
                    }
                    else if (flag)
                    {
                        data.IsPending = false;
                        data.IsStreaming = false;
                        client.PendingReplicables--;
                        if (client.WantsBatchCompleteConfirmation && (client.PendingReplicables == 0))
                        {
                            this.m_callback.SendPendingReplicablesDone(packet.Sender);
                            client.WantsBatchCompleteConfirmation = false;
                        }
                    }
                }
                if (objectByNetworkId != null)
                {
                    this.ProcessBlocker(objectByNetworkId, packet.Sender, client, null);
                }
            }
            packet.Return();
        }

        public void ReplicableRequest(MyPacket packet)
        {
            long entityId = packet.BitStream.ReadInt64(0x40);
            byte num2 = 0;
            bool flag1 = packet.BitStream.ReadBool();
            if (flag1)
            {
                num2 = packet.BitStream.ReadByte(8);
            }
            IMyReplicable replicableByEntityId = this.m_callback.GetReplicableByEntityId(entityId);
            MyClient client = this.m_clientStates[packet.Sender];
            if (flag1)
            {
                if (replicableByEntityId != null)
                {
                    client.PermanentReplicables[replicableByEntityId] = num2;
                }
            }
            else if (replicableByEntityId != null)
            {
                client.PermanentReplicables.Remove(replicableByEntityId);
            }
            packet.Return();
        }

        public void Replicate(IMyReplicable obj)
        {
            if (base.IsTypeReplicated(obj.GetType()))
            {
                if (!obj.IsReadyForReplication)
                {
                    obj.ReadyForReplicationAction.Add(obj, () => this.Replicate(obj));
                }
                else
                {
                    IMyReplicable replicable;
                    base.AddNetworkObjectServer(obj);
                    base.m_replicables.Add(obj, out replicable);
                    this.AddStateGroups(obj);
                    if (obj.PriorityUpdate)
                    {
                        this.m_priorityUpdates.Add(obj);
                    }
                }
            }
        }

        public void ResendMissingReplicableChildren(IMyEventProxy target)
        {
            this.ResendMissingReplicableChildren(base.GetProxyTarget(target) as IMyReplicable);
        }

        private void ResendMissingReplicableChildren(IMyReplicable replicable)
        {
            base.m_replicables.GetAllChildren(replicable, this.m_tmpReplicableList);
            foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
            {
                if (pair.Value.HasReplicable(replicable))
                {
                    foreach (IMyReplicable replicable2 in this.m_tmpReplicableList)
                    {
                        if (!pair.Value.HasReplicable(replicable2))
                        {
                            this.AddForClient(replicable2, pair.Key, pair.Value, false, false);
                        }
                    }
                }
            }
            this.m_tmpReplicableList.Clear();
        }

        public void ResetForClients(IMyReplicable obj)
        {
            this.RemoveForClients(obj, client => client.Replicables.ContainsKey(obj), true);
        }

        private void ScheduleStateGroupSync(MyClient client, MyStateDataEntry groupEntry, long currentTime)
        {
            IMyReplicable key = groupEntry.Owner.GetParent() ?? groupEntry.Owner;
            MyClient.UpdateLayer layerOfReplicable = null;
            if (!client.ReplicableToLayer.TryGetValue(key, out layerOfReplicable))
            {
                layerOfReplicable = client.GetLayerOfReplicable(key);
                if (layerOfReplicable == null)
                {
                    return;
                }
            }
            int num = ReferenceEquals(key, client.State.ControlledReplicable) ? 1 : (groupEntry.Group.IsHighPriority ? Math.Max(layerOfReplicable.Descriptor.SendInterval >> 4, 1) : layerOfReplicable.Descriptor.SendInterval);
            long priority = MyRandom.Instance.Next(1, num * 2) + currentTime;
            if (!client.DirtyQueue.Contains(groupEntry))
            {
                if (groupEntry.Owner.IsValid || this.m_postponedDestructionReplicables.Contains(groupEntry.Owner))
                {
                    client.DirtyQueue.Enqueue(groupEntry, priority);
                }
            }
            else if (!groupEntry.Owner.IsValid && !this.m_postponedDestructionReplicables.Contains(groupEntry.Owner))
            {
                client.DirtyQueue.Remove(groupEntry);
            }
            else
            {
                long num3 = groupEntry.Priority;
                priority = Math.Min(num3, priority);
                if (priority != num3)
                {
                    client.DirtyQueue.UpdatePriority(groupEntry, priority);
                }
            }
        }

        public void SendClientConnected(ref ConnectedClientDataMsg msg, ulong sendTo)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            MySerializer.Write<ConnectedClientDataMsg>(bitStreamPacketData.Stream, ref msg, null);
            this.m_callback.SentClientJoined(bitStreamPacketData, new EndpointId(sendTo));
        }

        public void SendJoinResult(ref JoinResultMsg msg, ulong sendTo)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            MySerializer.Write<JoinResultMsg>(bitStreamPacketData.Stream, ref msg, null);
            this.m_callback.SendJoinResult(bitStreamPacketData, new EndpointId(sendTo));
        }

        public void SendPlayerData(ref PlayerDataMsg msg)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            MySerializer.Write<PlayerDataMsg>(bitStreamPacketData.Stream, ref msg, null);
            this.m_endpoints.Clear();
            foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
            {
                this.m_endpoints.Add(pair.Key.Id);
            }
            this.m_callback.SendPlayerData(bitStreamPacketData, this.m_endpoints);
        }

        private void SendReplicationCreate(IMyReplicable obj, MyClient client, Endpoint clientEndpoint)
        {
            TypeId typeIdByType = base.GetTypeIdByType(obj.GetType());
            NetworkId networkIdByObject = base.GetNetworkIdByObject(obj);
            NetworkId invalid = NetworkId.Invalid;
            IMyReplicable parent = obj.GetParent();
            if (parent != null)
            {
                invalid = base.GetNetworkIdByObject(parent);
            }
            List<IMyStateGroup> list = this.m_replicableGroups[obj];
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            BitStream stream = bitStreamPacketData.Stream;
            stream.WriteTypeId(typeIdByType);
            stream.WriteNetworkId(networkIdByObject);
            stream.WriteNetworkId(invalid);
            IMyStreamableReplicable replicable2 = obj as IMyStreamableReplicable;
            bool flag = (replicable2 != null) && replicable2.NeedsToBeStreamed;
            if ((replicable2 == null) || replicable2.NeedsToBeStreamed)
            {
                stream.WriteByte((byte) list.Count, 8);
            }
            else
            {
                stream.WriteByte((byte) (list.Count - 1), 8);
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (flag || !list[i].IsStreaming)
                {
                    stream.WriteNetworkId(base.GetNetworkIdByObject(list[i]));
                }
            }
            if (flag)
            {
                client.Replicables[obj].IsStreaming = true;
                this.m_callback.SendReplicationCreateStreamed(bitStreamPacketData, clientEndpoint);
            }
            else
            {
                obj.OnSave(stream, clientEndpoint);
                this.m_callback.SendReplicationCreate(bitStreamPacketData, clientEndpoint);
            }
        }

        private void SendReplicationDestroy(IMyReplicable obj, List<EndpointId> recipients)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            bitStreamPacketData.Stream.WriteNetworkId(base.GetNetworkIdByObject(obj));
            this.m_callback.SendReplicationDestroy(bitStreamPacketData, recipients);
        }

        private void SendServerData(Endpoint endpointId)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            base.SerializeTypeTable(bitStreamPacketData.Stream);
            this.m_callback.SendServerData(bitStreamPacketData, endpointId);
        }

        private void SendStreamingEntry(MyClient client, MyStateDataEntry entry)
        {
            Endpoint endpointId = client.State.EndpointId;
            if (entry.Group.IsProcessingForClient(endpointId) != MyStreamProcessingState.Finished)
            {
                client.Serialize(entry.Group, null, MyTimeSpan.Zero);
                this.ScheduleStateGroupSync(client, entry, base.SyncFrameCounter);
            }
            else
            {
                MyTimeSpan span;
                MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
                BitStream sendStream = bitStreamPacketData.Stream;
                client.WritePacketHeader(sendStream, true, this.m_serverTimeStamp, out span);
                sendStream.Terminate();
                sendStream.WriteNetworkId(entry.GroupId);
                sendStream.WriteInt32(0, 0x20);
                client.Serialize(entry.Group, sendStream, span);
                int bitPosition = sendStream.BitPosition;
                sendStream.SetBitPositionWrite(sendStream.BitPosition);
                sendStream.WriteInt32(bitPosition - sendStream.BitPosition, 0x20);
                sendStream.SetBitPositionWrite(bitPosition);
                sendStream.Terminate();
                this.m_callback.SendStateSync(bitStreamPacketData, endpointId, true);
            }
            IMyReplicable owner = entry.Group.Owner;
            if (owner != null)
            {
                using (this.m_tmp)
                {
                    base.m_replicables.GetAllChildren(owner, this.m_tmp);
                    foreach (IMyReplicable replicable2 in this.m_tmp)
                    {
                        if (!client.HasReplicable(replicable2))
                        {
                            this.AddForClient(replicable2, endpointId, client, false, false);
                        }
                    }
                }
            }
        }

        public override void SendUpdate()
        {
            this.m_serverTimeStamp = this.m_callback.GetUpdateTime();
            this.m_serverFrame += 1L;
            this.ApplyDirtyGroups();
            if (this.m_clientStates.Count != 0)
            {
                this.m_priorityUpdates.ApplyChanges();
                foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
                {
                    if (pair.Value.IsReady)
                    {
                        this.m_lastLayerAdditions.Clear();
                        IMyReplicable controlledReplicable = pair.Value.State.ControlledReplicable;
                        IMyReplicable characterReplicable = pair.Value.State.CharacterReplicable;
                        int length = pair.Value.UpdateLayers.Length;
                        MyClient.UpdateLayer layer1 = pair.Value.UpdateLayers[pair.Value.UpdateLayers.Length - 1];
                        MyClient.UpdateLayer[] updateLayers = pair.Value.UpdateLayers;
                        int index = 0;
                        while (true)
                        {
                            if (index >= updateLayers.Length)
                            {
                                foreach (IMyReplicable replicable9 in this.m_priorityUpdates)
                                {
                                    this.RefreshReplicable(replicable9, pair.Key, pair.Value, false);
                                }
                                break;
                            }
                            MyClient.UpdateLayer layer = updateLayers[index];
                            length--;
                            layer.UpdateTimer--;
                            if (layer.UpdateTimer <= 0)
                            {
                                layer.UpdateTimer = layer.Descriptor.UpdateInterval;
                                if (pair.Value.State.Position != null)
                                {
                                    BoundingBoxD aabb = new BoundingBoxD(pair.Value.State.Position.Value - new Vector3D((double) layer.Descriptor.Radius), pair.Value.State.Position.Value + new Vector3D((double) layer.Descriptor.Radius));
                                    base.m_replicables.GetReplicablesInBox(aabb, this.m_tmpReplicableList);
                                }
                                HashSet<IMyReplicable> replicables = layer.Replicables;
                                layer.Replicables = this.m_toRecalculateHash;
                                this.m_toRecalculateHash = replicables;
                                layer.Sender.List.Clear();
                                foreach (IMyReplicable replicable3 in this.m_tmpReplicableList)
                                {
                                    this.AddReplicableToLayer(replicable3, layer, pair.Value, length == 0);
                                }
                                this.m_tmpReplicableList.Clear();
                                foreach (KeyValuePair<IMyReplicable, byte> pair2 in pair.Value.PermanentReplicables)
                                {
                                    if (((pair.Value.UpdateLayers.Length - pair2.Value) - 1) == length)
                                    {
                                        this.AddReplicableToLayer(pair2.Key, layer, pair.Value, true);
                                    }
                                }
                                if (length == 0)
                                {
                                    pair.Value.CrucialReplicables.Clear();
                                    if (controlledReplicable != null)
                                    {
                                        this.AddReplicableToLayer(controlledReplicable, layer, pair.Value, true);
                                        this.AddReplicableToLayer(characterReplicable, layer, pair.Value, true);
                                        this.AddCrucialReplicable(pair.Value, controlledReplicable);
                                        this.AddCrucialReplicable(pair.Value, characterReplicable);
                                        HashSet<IMyReplicable> dependencies = characterReplicable.GetDependencies(true);
                                        if (dependencies != null)
                                        {
                                            foreach (IMyReplicable replicable4 in dependencies)
                                            {
                                                this.AddReplicableToLayer(replicable4, layer, pair.Value, false);
                                            }
                                        }
                                    }
                                    foreach (IMyReplicable replicable5 in this.m_lastLayerAdditions)
                                    {
                                        this.AddReplicableToLayer(replicable5, layer, pair.Value, false);
                                    }
                                }
                                foreach (IMyReplicable replicable6 in layer.Replicables)
                                {
                                    IMyReplicable parent = replicable6.GetParent();
                                    if (!pair.Value.HasReplicable(replicable6) && ((parent == null) || pair.Value.HasReplicable(parent)))
                                    {
                                        this.AddForClient(replicable6, pair.Key, pair.Value, false, true);
                                    }
                                }
                                foreach (IMyReplicable replicable8 in this.m_toRecalculateHash)
                                {
                                    this.RefreshReplicable(replicable8, pair.Key, pair.Value, false);
                                }
                                this.m_toRecalculateHash.Clear();
                                if ((pair.Value.WantsBatchCompleteConfirmation && (length == 0)) && (pair.Value.PendingReplicables == 0))
                                {
                                    this.m_callback.SendPendingReplicablesDone(pair.Key);
                                    pair.Value.WantsBatchCompleteConfirmation = false;
                                }
                            }
                            index++;
                        }
                    }
                }
                foreach (IMyReplicable replicable10 in this.m_priorityUpdates)
                {
                    this.m_priorityUpdates.Remove(replicable10, false);
                }
                this.m_priorityUpdates.ApplyRemovals();
                foreach (KeyValuePair<Endpoint, MyClient> pair3 in this.m_clientStates)
                {
                    this.FilterStateSync(pair3.Value);
                }
                foreach (KeyValuePair<Endpoint, MyClient> pair4 in this.m_clientStates)
                {
                    pair4.Value.SendUpdate(this.m_serverTimeStamp);
                }
                if (StressSleep.X > 0)
                {
                    int millisecondsTimeout = (StressSleep.Z != 0) ? (((int) (Math.Sin((this.m_serverTimeStamp.Milliseconds * 3.1415926535897931) / ((double) StressSleep.Z)) * StressSleep.Y)) + StressSleep.X) : MyRandom.Instance.Next(StressSleep.X, StressSleep.Y);
                    Thread.Sleep(millisecondsTimeout);
                }
            }
        }

        public void SendWorld(byte[] worldData, EndpointId sendTo)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            MySerializer.Write<byte[]>(bitStreamPacketData.Stream, ref worldData, null);
            this.m_callback.SendWorld(bitStreamPacketData, sendTo);
        }

        public void SendWorldData(ref ServerDataMsg msg)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            MySerializer.Write<ServerDataMsg>(bitStreamPacketData.Stream, ref msg, null);
            this.m_endpoints.Clear();
            foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
            {
                this.m_endpoints.Add(pair.Key.Id);
            }
            this.m_callback.SendWorldData(bitStreamPacketData, this.m_endpoints);
        }

        public void SetClientBatchConfrmation(Endpoint clientEndpoint, bool value)
        {
            MyClient client;
            if (this.m_clientStates.TryGetValue(clientEndpoint, out client))
            {
                client.WantsBatchCompleteConfirmation = value;
                if (value)
                {
                    client.ResetLayerTimers();
                }
            }
        }

        public override void SetPriorityMultiplier(EndpointId id, float priority)
        {
            MyClient client;
            if (this.m_clientStates.TryGetValue(new Endpoint(id, 0), out client))
            {
                client.PriorityMultiplier = priority;
            }
        }

        private bool ShouldSendEvent(IMyNetObject eventInstance, Vector3D? position, MyClient client)
        {
            if (position != null)
            {
                if (client.State.Position == null)
                {
                    return false;
                }
                int syncDistance = MyLayers.GetSyncDistance();
                if (Vector3D.DistanceSquared(position.Value, client.State.Position.Value) > (syncDistance * syncDistance))
                {
                    return false;
                }
            }
            return ((eventInstance != null) ? ((eventInstance is IMyReplicable) && client.Replicables.ContainsKey((IMyReplicable) eventInstance)) : true);
        }

        public override void Simulate()
        {
        }

        private static void TryAddBlockerForClient(MyClient client, IMyReplicable targetReplicable, IMyReplicable blockingReplicable)
        {
            if ((!client.IsReplicableReady(targetReplicable) || (!client.IsReplicableReady(blockingReplicable) || client.BlockedReplicables.ContainsKey(targetReplicable))) || client.BlockedReplicables.ContainsKey(blockingReplicable))
            {
                MyDestroyBlocker blocker;
                MyDestroyBlocker blocker2;
                if (!client.BlockedReplicables.TryGetValue(targetReplicable, out blocker))
                {
                    blocker = new MyDestroyBlocker();
                    client.BlockedReplicables.Add(targetReplicable, blocker);
                }
                blocker.Blockers.Add(blockingReplicable);
                if (!client.BlockedReplicables.TryGetValue(blockingReplicable, out blocker2))
                {
                    blocker2 = new MyDestroyBlocker();
                    client.BlockedReplicables.Add(blockingReplicable, blocker2);
                }
                blocker2.Blockers.Add(targetReplicable);
            }
        }

        public override void UpdateAfter()
        {
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        public override void UpdateBefore()
        {
            Endpoint key = new Endpoint();
            foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
            {
                try
                {
                    pair.Value.Update(this.m_serverTimeStamp);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                    key = pair.Key;
                }
            }
            if (key.Id.IsValid)
            {
                this.m_callback.DisconnectClient(key.Id.Value);
            }
            MyTimeSpan updateTime = this.m_callback.GetUpdateTime();
            foreach (KeyValuePair<Endpoint, MyTimeSpan> pair2 in this.m_recentClientsStates)
            {
                if (pair2.Value < updateTime)
                {
                    this.m_recentClientStatesToRemove.Add(pair2.Key);
                }
            }
            foreach (Endpoint endpoint2 in this.m_recentClientStatesToRemove)
            {
                this.m_recentClientsStates.Remove<Endpoint, MyTimeSpan>(endpoint2);
            }
            this.m_recentClientStatesToRemove.Clear();
            this.m_postponedDestructionReplicables.ApplyAdditions();
            foreach (IMyReplicable replicable in this.m_postponedDestructionReplicables)
            {
                this.Destroy(replicable);
            }
            this.m_postponedDestructionReplicables.ApplyRemovals();
        }

        public override void UpdateClientStateGroups()
        {
        }

        public override void UpdateStatisticsData(int outgoing, int incoming, int tamperred, float gcMemory, float processMemory)
        {
            foreach (KeyValuePair<Endpoint, MyClient> pair in this.m_clientStates)
            {
                pair.Value.Statistics.UpdateData(outgoing, incoming, tamperred, gcMemory, processMemory);
            }
        }

        internal class MyDestroyBlocker
        {
            public bool Remove;
            public bool IsProcessing;
            public readonly List<IMyReplicable> Blockers = new List<IMyReplicable>();
        }
    }
}

