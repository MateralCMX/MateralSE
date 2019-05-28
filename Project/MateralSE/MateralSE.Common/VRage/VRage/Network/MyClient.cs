namespace VRage.Network
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Library;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Replication;
    using VRageMath;

    internal class MyClient
    {
        public readonly MyClientStateBase State;
        private readonly IReplicationServerCallback m_callback;
        public float PriorityMultiplier = 1f;
        public bool IsReady;
        private MyTimeSpan m_lastClientRealtime;
        private MyTimeSpan m_lastClientTimestamp;
        private MyTimeSpan m_lastStateSyncTimeStamp;
        private byte m_stateSyncPacketId;
        public readonly Dictionary<IMyReplicable, byte> PermanentReplicables = new Dictionary<IMyReplicable, byte>();
        public readonly HashSet<IMyReplicable> CrucialReplicables = new HashSet<IMyReplicable>();
        public readonly MyConcurrentDictionary<IMyReplicable, MyReplicableClientData> Replicables = new MyConcurrentDictionary<IMyReplicable, MyReplicableClientData>(InstanceComparer<IMyReplicable>.Default);
        public int PendingReplicables;
        public bool WantsBatchCompleteConfirmation = true;
        public readonly MyConcurrentDictionary<IMyReplicable, MyReplicationServer.MyDestroyBlocker> BlockedReplicables = new MyConcurrentDictionary<IMyReplicable, MyReplicationServer.MyDestroyBlocker>(0, null);
        public readonly Dictionary<IMyStateGroup, MyStateDataEntry> StateGroups = new Dictionary<IMyStateGroup, MyStateDataEntry>(InstanceComparer<IMyStateGroup>.Default);
        public readonly FastPriorityQueue<MyStateDataEntry> DirtyQueue = new FastPriorityQueue<MyStateDataEntry>(0x400);
        private readonly HashSet<string> m_clientCachedData = new HashSet<string>();
        public MyPacketStatistics Statistics;
        public UpdateLayer[] UpdateLayers;
        public readonly Dictionary<IMyReplicable, UpdateLayer> ReplicableToLayer = new Dictionary<IMyReplicable, UpdateLayer>();
        private readonly CachingHashSet<IslandData> m_islands = new CachingHashSet<IslandData>();
        private readonly Dictionary<IMyReplicable, IslandData> m_replicableToIsland = new Dictionary<IMyReplicable, IslandData>();
        private byte m_nextIslandIndex;
        private readonly List<MyOrderedPacket> m_incomingBuffer = new List<MyOrderedPacket>();
        private bool m_incomingBuffering = true;
        private byte m_lastProcessedClientPacketId = 0xff;
        private readonly MyPacketTracker m_clientTracker = new MyPacketTracker();
        private bool m_processedPacket;
        private MyTimeSpan m_lastReceivedTimeStamp = MyTimeSpan.Zero;
        private const int MINIMUM_INCOMING_BUFFER = 4;
        private const byte OUT_OF_ORDER_RESET_PROTECTION = 0x40;
        private const byte OUT_OF_ORDER_ACCEPT_THRESHOLD = 6;
        private byte m_lastReceivedAckId;
        private bool m_waitingForReset;
        private readonly List<IMyStateGroup>[] m_pendingStateSyncAcks = (from s in Enumerable.Range(0, 0x100) select new List<IMyStateGroup>(8)).ToArray<List<IMyStateGroup>>();
        private static readonly MyTimeSpan MAXIMUM_PACKET_GAP = MyTimeSpan.FromSeconds(0.40000000596046448);

        public MyClient(MyClientStateBase emptyState, IReplicationServerCallback callback)
        {
            this.m_callback = callback;
            this.State = emptyState;
            this.InitLayers();
        }

        private void AddIncomingPacketSorted(byte packetId, MyPacket packet)
        {
            MyOrderedPacket item = new MyOrderedPacket {
                Id = packetId,
                Packet = packet
            };
            int index = this.m_incomingBuffer.Count - 1;
            while (((index >= 0) && (packetId < this.m_incomingBuffer[index].Id)) && ((packetId >= 0x40) || (this.m_incomingBuffer[index].Id <= 0xc0)))
            {
                index--;
            }
            index++;
            this.m_incomingBuffer.Insert(index, item);
        }

        private void AddPendingAck(byte stateSyncPacketId, IMyStateGroup group)
        {
            this.m_pendingStateSyncAcks[stateSyncPacketId].Add(group);
        }

        private bool CheckStateSyncPacketId()
        {
            int stateSyncPacketId = this.m_stateSyncPacketId;
            byte num2 = 0;
            while (this.m_pendingStateSyncAcks[this.m_stateSyncPacketId].Count != 0)
            {
                this.m_stateSyncPacketId = (byte) (this.m_stateSyncPacketId + 1);
                num2 = (byte) (num2 + 1);
                if (stateSyncPacketId == this.m_stateSyncPacketId)
                {
                    this.Statistics.PendingPackets = num2;
                    return false;
                }
            }
            this.Statistics.PendingPackets = num2;
            return true;
        }

        private void ClearBufferedIncomingPackets(MyTimeSpan serverTimeStamp)
        {
            if (this.m_incomingBuffer.Count > 0)
            {
                this.UpdateIncoming(serverTimeStamp, true);
            }
        }

        public UpdateLayer GetLayerOfReplicable(IMyReplicable rep)
        {
            BoundingBoxD aABB = rep.GetAABB();
            if (this.State.Position != null)
            {
                foreach (UpdateLayer layer in this.UpdateLayers)
                {
                    Vector3D? position = this.State.Position;
                    BoundingBoxD xd2 = new BoundingBoxD(this.State.Position.Value - new Vector3D((double) layer.Descriptor.Radius), position.Value + new Vector3D((double) layer.Descriptor.Radius));
                    if (xd2.Intersects(aABB))
                    {
                        return layer;
                    }
                }
            }
            return null;
        }

        public byte GetReplicableIslandIndex(IMyReplicable replicable)
        {
            IslandData data;
            return (!this.m_replicableToIsland.TryGetValue(replicable, out data) ? 0 : data.Index);
        }

        public bool HasReplicable(IMyReplicable replicable) => 
            this.Replicables.ContainsKey(replicable);

        private void InitLayers()
        {
            this.UpdateLayers = new UpdateLayer[MyLayers.UpdateLayerDescriptors.Count];
            for (int i = 0; i < MyLayers.UpdateLayerDescriptors.Count; i++)
            {
                MyLayers.UpdateLayerDesc desc = MyLayers.UpdateLayerDescriptors[i];
                UpdateLayer layer1 = new UpdateLayer();
                layer1.Descriptor = desc;
                layer1.Replicables = new HashSet<IMyReplicable>();
                layer1.Sender = new MyDistributedUpdater<List<IMyReplicable>, IMyReplicable>(desc.SendInterval);
                layer1.UpdateTimer = i + 1;
                this.UpdateLayers[i] = layer1;
            }
        }

        public bool IsAckAvailable()
        {
            byte num = (byte) (this.m_lastReceivedAckId - 6);
            byte num2 = (byte) (this.m_stateSyncPacketId + 1);
            if (!this.m_waitingForReset && (num2 != num))
            {
                return true;
            }
            this.m_waitingForReset = true;
            return false;
        }

        private static bool IsPreceding(int currentPacketId, int lastPacketId, int threshold)
        {
            if (lastPacketId < currentPacketId)
            {
                lastPacketId += 0x100;
            }
            return ((lastPacketId - currentPacketId) <= threshold);
        }

        public bool IsReplicablePending(IMyReplicable replicable)
        {
            MyReplicableClientData data;
            return (this.Replicables.TryGetValue(replicable, out data) && (data.IsPending || data.IsStreaming));
        }

        public bool IsReplicableReady(IMyReplicable replicable)
        {
            MyReplicableClientData data;
            return (this.Replicables.TryGetValue(replicable, out data) && (!data.IsPending && !data.IsStreaming));
        }

        private void OnAck(byte ackId)
        {
            if (IsPreceding(ackId, this.m_lastReceivedAckId, 6))
            {
                this.RaiseAck(ackId, true);
            }
            else
            {
                this.RaiseAck(ackId, true);
                this.m_lastReceivedAckId = ackId;
            }
        }

        public void OnClientAcks(MyPacket packet)
        {
            byte num3;
            byte num4;
            byte num = packet.BitStream.ReadByte(8);
            byte num2 = packet.BitStream.ReadByte(8);
            for (int i = 0; i < num2; i++)
            {
                this.OnAck(packet.BitStream.ReadByte(8));
            }
            if (!packet.BitStream.CheckTerminator())
            {
                throw new EndOfStreamException("Invalid BitStream terminator");
            }
            if (!this.m_waitingForReset)
            {
                num3 = (byte) (this.m_stateSyncPacketId + 1);
                num4 = (byte) (this.m_lastReceivedAckId - 6);
            }
            else
            {
                this.m_stateSyncPacketId = (byte) (num + 0x40);
                this.CheckStateSyncPacketId();
                num3 = (byte) (this.m_stateSyncPacketId + 1);
                num4 = (byte) (num3 - 0x40);
                this.m_waitingForReset = false;
            }
            for (byte j = num3; j != num4; j = (byte) (j + 1))
            {
                this.RaiseAck(j, false);
            }
        }

        public void OnClientUpdate(MyPacket packet, MyTimeSpan serverTimeStamp)
        {
            if (!this.UsePlayoutDelayBuffer)
            {
                this.ClearBufferedIncomingPackets(serverTimeStamp);
                this.ProcessIncomingPacket(packet, false, serverTimeStamp, false);
                packet.Return();
            }
            else
            {
                int bitPosition = packet.BitStream.BitPosition;
                byte packetId = packet.BitStream.ReadByte(8);
                packet.BitStream.SetBitPositionRead(bitPosition);
                this.AddIncomingPacketSorted(packetId, packet);
            }
        }

        private bool ProcessIncomingPacket(MyPacket packet, bool skipControls, MyTimeSpan serverTimeStamp, bool skip = false)
        {
            byte id = packet.BitStream.ReadByte(8);
            this.m_lastClientTimestamp = MyTimeSpan.FromMilliseconds(packet.BitStream.ReadDouble());
            this.m_lastClientRealtime = MyTimeSpan.FromMilliseconds(packet.BitStream.ReadDouble());
            this.m_lastReceivedTimeStamp = serverTimeStamp;
            this.Statistics.Update(this.m_clientTracker.Add(id));
            bool flag = (id <= this.m_lastProcessedClientPacketId) && ((this.m_lastProcessedClientPacketId <= 0xc0) || (id >= 0x40));
            if (!flag)
            {
                this.m_lastProcessedClientPacketId = id;
            }
            skipControls |= flag;
            this.State.Serialize(packet.BitStream, skipControls);
            if (!packet.BitStream.CheckTerminator())
            {
                throw new EndOfStreamException("Invalid BitStream terminator");
            }
            if (!skip)
            {
                this.m_processedPacket = true;
            }
            return skipControls;
        }

        private void RaiseAck(byte ackId, bool delivered)
        {
            foreach (IMyStateGroup group in this.m_pendingStateSyncAcks[ackId])
            {
                if (this.StateGroups.ContainsKey(group))
                {
                    group.OnAck(this.State, ackId, delivered);
                }
            }
            this.m_pendingStateSyncAcks[ackId].Clear();
        }

        public bool RemoveCache(IMyReplicable replicable, string storageName) => 
            (((replicable == null) || !this.Replicables.ContainsKey(replicable)) && this.m_clientCachedData.Remove(storageName));

        private void RemoveCachedIsland(IslandData island)
        {
            this.m_islands.Remove(island, false);
            foreach (IMyReplicable replicable in island.Replicables)
            {
                this.m_replicableToIsland.Remove(replicable);
            }
        }

        public void RemoveReplicableFromIslands(IMyReplicable replicable)
        {
            IslandData data;
            if (this.m_replicableToIsland.TryGetValue(replicable, out data))
            {
                data.Replicables.Remove(replicable);
                this.m_replicableToIsland.Remove(replicable);
            }
        }

        public void ResetLayerTimers()
        {
            int num = 0;
            UpdateLayer[] updateLayers = this.UpdateLayers;
            for (int i = 0; i < updateLayers.Length; i++)
            {
                num++;
                updateLayers[i].UpdateTimer = num;
            }
        }

        private void SendEmptyStateSync(MyTimeSpan serverTimeStamp)
        {
            MyTimeSpan span;
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            if (!this.WritePacketHeader(bitStreamPacketData.Stream, false, serverTimeStamp, out span))
            {
                bitStreamPacketData.Return();
            }
            else
            {
                bitStreamPacketData.Stream.Terminate();
                this.m_callback.SendStateSync(bitStreamPacketData, this.State.EndpointId, false);
            }
        }

        private void SendReplicationIslandDone(IslandData island, Endpoint clientEndpoint)
        {
            MyPacketDataBitStreamBase bitStreamPacketData = this.m_callback.GetBitStreamPacketData();
            bitStreamPacketData.Stream.WriteByte(island.Index, 8);
            bitStreamPacketData.Stream.WriteInt32(island.Replicables.Count, 0x20);
            foreach (IMyEntityReplicable replicable in island.Replicables)
            {
                if (replicable != null)
                {
                    bitStreamPacketData.Stream.WriteInt64(replicable.EntityId, 0x40);
                    bitStreamPacketData.Stream.Write(replicable.WorldMatrix.Translation);
                    bitStreamPacketData.Stream.WriteQuaternion(Quaternion.CreateFromRotationMatrix(replicable.WorldMatrix));
                }
            }
            this.m_callback.SendReplicationIslandDone(bitStreamPacketData, clientEndpoint);
        }

        public bool SendStateSync(MyStateDataEntry stateGroupEntry, int mtuBytes, ref MyPacketDataBitStreamBase data, MyTimeSpan serverTimeStamp)
        {
            if (data == null)
            {
                MyTimeSpan span;
                data = this.m_callback.GetBitStreamPacketData();
                if (!this.WritePacketHeader(data.Stream, false, serverTimeStamp, out span))
                {
                    data.Return();
                    data = null;
                    return false;
                }
                this.State.ClientTimeStamp = span;
            }
            BitStream stream = data.Stream;
            int num = 8 * (mtuBytes - 2);
            int bitPosition = stream.BitPosition;
            bool enableNetworkServerOutgoingPacketTracking = MyCompilationSymbols.EnableNetworkServerOutgoingPacketTracking;
            stream.Terminate();
            bool flag2 = MyCompilationSymbols.EnableNetworkServerOutgoingPacketTracking;
            stream.WriteNetworkId(stateGroupEntry.GroupId);
            if (MyCompilationSymbols.EnableNetworkServerOutgoingPacketTracking)
            {
                stateGroupEntry.Group.Owner.ToString();
                string fullName = stateGroupEntry.Group.GetType().FullName;
            }
            stream.WriteInt16(0, 0x10);
            this.Serialize(stateGroupEntry.Group, stream, MyTimeSpan.Zero);
            int newBitPosition = stream.BitPosition;
            stream.SetBitPositionWrite(stream.BitPosition);
            stream.WriteInt16((short) (newBitPosition - stream.BitPosition), 0x10);
            stream.SetBitPositionWrite(newBitPosition);
            bool flag3 = MyCompilationSymbols.EnableNetworkServerOutgoingPacketTracking;
            if (((stream.BitPosition - bitPosition) > 0) && (stream.BitPosition <= num))
            {
                this.AddPendingAck(this.m_stateSyncPacketId, stateGroupEntry.Group);
            }
            else
            {
                if (MyCompilationSymbols.EnableNetworkServerOutgoingPacketTracking)
                {
                    stateGroupEntry.Group.Owner.ToString();
                    string fullName = stateGroupEntry.Group.GetType().FullName;
                }
                stateGroupEntry.Group.OnAck(this.State, this.m_stateSyncPacketId, false);
                stream.SetBitPositionWrite(bitPosition);
                data.Stream.Terminate();
                this.m_callback.SendStateSync(data, this.State.EndpointId, false);
                data = null;
            }
            return true;
        }

        public void SendUpdate(MyTimeSpan serverTimeStamp)
        {
            if (serverTimeStamp > (this.m_lastStateSyncTimeStamp + MAXIMUM_PACKET_GAP))
            {
                this.SendEmptyStateSync(serverTimeStamp);
            }
        }

        public void Serialize(IMyStateGroup group, BitStream sendStream, MyTimeSpan timeStamp)
        {
            int maxBitPosition = 0x7fffffff;
            if (timeStamp == MyTimeSpan.Zero)
            {
                timeStamp = this.State.ClientTimeStamp;
            }
            group.Serialize(sendStream, this.State.EndpointId, timeStamp, this.m_lastClientTimestamp, this.m_stateSyncPacketId, maxBitPosition, this.m_clientCachedData);
        }

        public void TryCreateNewCachedIsland(IMyReplicable replicable, HashSet<IMyReplicable> dependencies)
        {
            if (!this.m_replicableToIsland.ContainsKey(replicable))
            {
                if (this.m_nextIslandIndex == 0)
                {
                    this.m_nextIslandIndex = (byte) (this.m_nextIslandIndex + 1);
                }
                IslandData item = new IslandData {
                    Replicables = new HashSet<IMyReplicable>(),
                    Index = this.m_nextIslandIndex
                };
                item.Replicables.Add(replicable);
                this.m_replicableToIsland[replicable] = item;
                if (dependencies != null)
                {
                    foreach (IMyReplicable replicable2 in dependencies)
                    {
                        item.Replicables.Add(replicable2);
                        this.m_replicableToIsland[replicable2] = item;
                    }
                }
                this.m_islands.Add(item);
                this.m_islands.ApplyAdditions();
                this.m_nextIslandIndex = (byte) (this.m_nextIslandIndex + 1);
            }
        }

        public void Update(MyTimeSpan serverTimeStamp)
        {
            if (this.UsePlayoutDelayBuffer)
            {
                this.UpdateIncoming(serverTimeStamp, false);
            }
            else
            {
                if (!this.m_processedPacket)
                {
                    this.State.Update();
                }
                this.m_processedPacket = false;
            }
            if (serverTimeStamp > (this.m_lastReceivedTimeStamp + MAXIMUM_PACKET_GAP))
            {
                this.State.ResetControlledEntityControls();
            }
        }

        private void UpdateIncoming(MyTimeSpan serverTimeStamp, bool skipAll = false)
        {
            if ((this.m_incomingBuffer.Count == 0) || ((this.m_incomingBuffering && (this.m_incomingBuffer.Count < 4)) && !skipAll))
            {
                if (MyCompilationSymbols.EnableNetworkServerIncomingPacketTracking)
                {
                    int count = this.m_incomingBuffer.Count;
                }
                this.m_incomingBuffering = true;
                this.m_lastProcessedClientPacketId = 0xff;
                this.State.Update();
            }
            else
            {
                if (this.m_incomingBuffering)
                {
                    this.m_lastProcessedClientPacketId = (byte) (this.m_incomingBuffer[0].Id - 1);
                }
                this.m_incomingBuffering = false;
                string str = "";
                while (true)
                {
                    bool skipControls = (this.m_incomingBuffer.Count > 4) | skipAll;
                    bool flag = this.ProcessIncomingPacket(this.m_incomingBuffer[0].Packet, skipControls, serverTimeStamp, true);
                    if (MyCompilationSymbols.EnableNetworkServerIncomingPacketTracking)
                    {
                        str = this.m_incomingBuffer[0].Id + ", " + str;
                        if (flag)
                        {
                            str = "-" + str;
                        }
                    }
                    this.m_incomingBuffer[0].Packet.Return();
                    this.m_incomingBuffer.RemoveAt(0);
                    if ((this.m_incomingBuffer.Count <= 4) && (!flag || (this.m_incomingBuffer.Count <= 0)))
                    {
                        bool enableNetworkServerIncomingPacketTracking = MyCompilationSymbols.EnableNetworkServerIncomingPacketTracking;
                        return;
                    }
                }
            }
        }

        public void UpdateIslands()
        {
            foreach (IslandData data in this.m_islands)
            {
                bool flag = true;
                foreach (IMyStreamableReplicable replicable in data.Replicables)
                {
                    MyStateDataEntry entry;
                    if (replicable == null)
                    {
                        continue;
                    }
                    if (this.StateGroups.TryGetValue(replicable.GetStreamingStateGroup(), out entry) && this.DirtyQueue.Contains(entry))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    this.SendReplicationIslandDone(data, this.State.EndpointId);
                    this.RemoveCachedIsland(data);
                }
            }
            this.m_islands.ApplyRemovals();
        }

        public bool WritePacketHeader(BitStream sendStream, bool streaming, MyTimeSpan serverTimeStamp, out MyTimeSpan clientTimestamp)
        {
            this.m_lastStateSyncTimeStamp = serverTimeStamp;
            if (!streaming)
            {
                this.m_stateSyncPacketId = (byte) (this.m_stateSyncPacketId + 1);
                if (!this.CheckStateSyncPacketId())
                {
                    clientTimestamp = MyTimeSpan.Zero;
                    return false;
                }
            }
            sendStream.WriteBool(streaming);
            sendStream.WriteByte(streaming ? ((byte) 0) : this.m_stateSyncPacketId, 8);
            this.Statistics.Write(sendStream, this.m_callback.GetUpdateTime());
            sendStream.WriteDouble(serverTimeStamp.Milliseconds);
            sendStream.WriteDouble(this.m_lastClientTimestamp.Milliseconds);
            this.m_lastClientTimestamp = MyTimeSpan.FromMilliseconds(-1.0);
            sendStream.WriteDouble(this.m_lastClientRealtime.Milliseconds);
            this.m_lastClientRealtime = MyTimeSpan.FromMilliseconds(-1.0);
            this.m_callback.WriteCustomState(sendStream);
            clientTimestamp = serverTimeStamp;
            return true;
        }

        public bool UsePlayoutDelayBufferForCharacter { get; set; }

        public bool UsePlayoutDelayBufferForJetpack { get; set; }

        public bool UsePlayoutDelayBufferForGrids { get; set; }

        private bool UsePlayoutDelayBuffer =>
            (((this.State.IsControllingCharacter && this.UsePlayoutDelayBufferForCharacter) || (this.State.IsControllingJetpack && this.UsePlayoutDelayBufferForJetpack)) || (this.State.IsControllingGrid && this.UsePlayoutDelayBufferForGrids));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyClient.<>c <>9 = new MyClient.<>c();
            public static Func<int, List<IMyStateGroup>> <>9__18_0;

            internal List<IMyStateGroup> <.ctor>b__18_0(int s) => 
                new List<IMyStateGroup>(8);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IslandData
        {
            public HashSet<IMyReplicable> Replicables;
            public byte Index;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyOrderedPacket
        {
            public byte Id;
            public MyPacket Packet;
            public override string ToString() => 
                this.Id.ToString();
        }

        public class UpdateLayer
        {
            public MyLayers.UpdateLayerDesc Descriptor;
            public HashSet<IMyReplicable> Replicables;
            public MyDistributedUpdater<List<IMyReplicable>, IMyReplicable> Sender;
            public int UpdateTimer;
        }
    }
}

