namespace Sandbox.Engine.Multiplayer
{
    using Sandbox.Engine.Networking;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.GameServices;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Profiler;

    internal class MyTransportLayer
    {
        private static readonly int m_messageTypeCount = (((MyMessageId) MyEnum<MyMessageId>.Range.Max) + ((MyMessageId) 1));
        private readonly Queue<int>[] m_slidingWindows = (from s in Enumerable.Range(0, m_messageTypeCount) select new Queue<int>(120)).ToArray<Queue<int>>();
        private readonly int[] m_thisFrameTraffic = new int[m_messageTypeCount];
        private bool m_isBuffering;
        private readonly int m_channel;
        private List<MyPacket> m_buffer;
        private byte m_largeMessageParts;
        private int m_largeMessageSize;
        private readonly Dictionary<HandlerId, Action<MyPacket>> m_handlers = new Dictionary<HandlerId, Action<MyPacket>>();

        public MyTransportLayer(int channel)
        {
            this.m_channel = channel;
            this.DisconnectPeerOnError = null;
            MyNetworkReader.SetHandler(channel, new NetworkMessageDelegate(this.HandleMessage), x => this.DisconnectPeerOnError(x));
        }

        public void AddMessageToBuffer(MyPacket packet)
        {
            this.m_buffer.Add(packet);
        }

        public void Clear()
        {
            MyNetworkReader.ClearHandler(2);
            this.ClearBuffer();
        }

        private void ClearBuffer()
        {
            if (this.m_buffer != null)
            {
                using (List<MyPacket>.Enumerator enumerator = this.m_buffer.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Return();
                    }
                }
                this.m_buffer.Clear();
            }
        }

        private void HandleMessage(MyPacket p)
        {
            int bitPosition = p.BitStream.BitPosition;
            MyMessageId id = (MyMessageId) p.BitStream.ReadByte(8);
            if (id == MyMessageId.FLUSH)
            {
                this.ClearBuffer();
                p.Return();
            }
            else
            {
                p.BitStream.SetBitPositionRead(bitPosition);
                if ((this.IsBuffering && ((id != MyMessageId.JOIN_RESULT) && ((id != MyMessageId.WORLD_DATA) && (id != MyMessageId.WORLD)))) && (id != MyMessageId.PLAYER_DATA))
                {
                    this.m_buffer.Add(p);
                }
                else
                {
                    this.ProfilePacketStatistics(true);
                    MyStatsGraph.Begin("Live data", 0, "HandleMessage", 0xc5, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
                    this.ProcessMessage(p);
                    float? bytesTransfered = null;
                    MyStatsGraph.End(bytesTransfered, 0f, "", "{0} B", null, "HandleMessage", 0xc7, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
                    this.ProfilePacketStatistics(false);
                }
            }
        }

        private MyNetworkWriter.MyPacketDescriptor InitSendStream(EndpointId endpoint, MyP2PMessageEnum msgType, MyMessageId msgId, byte index = 0)
        {
            MyNetworkWriter.MyPacketDescriptor descriptor1 = MyNetworkWriter.GetPacketDescriptor(endpoint, msgType, this.m_channel);
            descriptor1.Header.WriteByte((byte) msgId);
            descriptor1.Header.WriteByte(index);
            return descriptor1;
        }

        private void ProcessBuffer()
        {
            try
            {
                this.IsProcessingBuffer = true;
                this.ProfilePacketStatistics(true);
                MyStatsGraph.Begin("Live data", 0, "ProcessBuffer", 0xa2, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
                foreach (MyPacket packet in this.m_buffer)
                {
                    this.ProcessMessage(packet);
                }
                float? bytesTransfered = null;
                MyStatsGraph.End(bytesTransfered, 0f, "", "{0} B", null, "ProcessBuffer", 0xa7, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
                this.ProfilePacketStatistics(false);
            }
            finally
            {
                this.IsProcessingBuffer = false;
            }
        }

        private unsafe void ProcessMessage(MyPacket p)
        {
            HandlerId id;
            Action<MyPacket> action;
            id.messageId = (MyMessageId) p.BitStream.ReadByte(8);
            id.receiverIndex = p.BitStream.ReadByte(8);
            if (((ulong) id.messageId) < this.m_thisFrameTraffic.Length)
            {
                int* numPtr1 = (int*) ref this.m_thisFrameTraffic[(int) id.messageId];
                numPtr1[0] += p.BitStream.ByteLength;
            }
            p.Sender = new Endpoint(p.Sender.Id, id.receiverIndex);
            if (!this.m_handlers.TryGetValue(id, out action))
            {
                HandlerId key = new HandlerId {
                    messageId = id.messageId,
                    receiverIndex = 0xff
                };
                this.m_handlers.TryGetValue(key, out action);
            }
            if (action == null)
            {
                p.Return();
            }
            else
            {
                MyStatsGraph.Begin(MyEnum<MyMessageId>.GetName(id.messageId), 0x7fffffff, "ProcessMessage", 0xe2, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
                action(p);
                MyStatsGraph.End(new float?((float) p.BitStream.ByteLength), 0f, "", "{0} B", null, "ProcessMessage", 0xe4, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
            }
        }

        private void ProfilePacketStatistics(bool begin)
        {
            if (begin)
            {
                MyStatsGraph.ProfileAdvanced(true);
                MyStatsGraph.Begin("Packet statistics", 0, "ProfilePacketStatistics", 0x75, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
            }
            else
            {
                float? bytesTransfered = null;
                MyStatsGraph.End(bytesTransfered, 0f, "", "{0} B", null, "ProfilePacketStatistics", 0x79, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
                MyStatsGraph.ProfileAdvanced(false);
            }
        }

        public void Register(MyMessageId messageId, byte receiverIndex, Action<MyPacket> handler)
        {
            HandlerId key = new HandlerId {
                messageId = messageId,
                receiverIndex = receiverIndex
            };
            this.m_handlers.Add(key, handler);
        }

        public void SendFlush(ulong sendTo)
        {
            MyNetworkWriter.SendPacket(this.InitSendStream(new EndpointId(sendTo), MyP2PMessageEnum.ReliableWithBuffering, MyMessageId.FLUSH, 0));
        }

        public void SendMessage(MyMessageId id, IPacketData data, bool reliable, List<EndpointId> endpoints, byte index = 0)
        {
            MyNetworkWriter.MyPacketDescriptor packet = this.InitSendStream(EndpointId.Null, reliable ? MyP2PMessageEnum.ReliableWithBuffering : MyP2PMessageEnum.Unreliable, id, index);
            packet.Recipients.AddRange(endpoints);
            packet.Data = data;
            MyNetworkWriter.SendPacket(packet);
        }

        public void SendMessage(MyMessageId id, IPacketData data, bool reliable, EndpointId endpoint, byte index = 0)
        {
            MyNetworkWriter.MyPacketDescriptor packet = this.InitSendStream(endpoint, reliable ? MyP2PMessageEnum.ReliableWithBuffering : MyP2PMessageEnum.Unreliable, id, index);
            packet.Data = data;
            MyNetworkWriter.SendPacket(packet);
        }

        public void Tick()
        {
            int num = 0;
            this.ProfilePacketStatistics(true);
            MyStatsGraph.Begin("Average data", 0x7fffffff, "Tick", 130, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
            int index = 0;
            while (index < m_messageTypeCount)
            {
                Queue<int> queue = this.m_slidingWindows[index];
                queue.Enqueue(this.m_thisFrameTraffic[index]);
                this.m_thisFrameTraffic[index] = 0;
                while (true)
                {
                    if (queue.Count <= 60)
                    {
                        int num3 = 0;
                        foreach (int num4 in queue)
                        {
                            num3 += num4;
                        }
                        if (num3 > 0)
                        {
                            MyStatsGraph.Begin(MyEnum<MyMessageId>.GetName((MyMessageId) ((byte) index)), 0x7fffffff, "Tick", 0x93, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
                            MyStatsGraph.End(new float?(((float) num3) / 60f), ((float) num3) / 1024f, "{0} KB/s", "{0} B", null, "Tick", 0x94, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
                        }
                        num += num3;
                        index++;
                        break;
                    }
                    queue.Dequeue();
                }
            }
            MyStatsGraph.End(new float?(((float) num) / 60f), ((float) num) / 1024f, "{0} KB/s", "{0} B", null, "Tick", 0x98, @"E:\Repo1\Sources\Sandbox.Game\Engine\Multiplayer\MyTransportLayer.cs");
            this.ProfilePacketStatistics(false);
        }

        [Conditional("DEBUG")]
        private void TraceMessage(string text, string messageText, ulong userId, long messageSize, MyP2PMessageEnum sendType)
        {
            MyNetworkClient client;
            if ((MyMultiplayer.Static == null) || !MyMultiplayer.Static.SyncLayer.Clients.TryGetClient(userId, out client))
            {
                userId.ToString();
            }
            else
            {
                string displayName = client.DisplayName;
            }
            if (sendType != MyP2PMessageEnum.Reliable)
            {
                MyP2PMessageEnum enum1 = sendType;
            }
        }

        public void Unregister(MyMessageId messageId, byte receiverIndex)
        {
            HandlerId key = new HandlerId {
                messageId = messageId,
                receiverIndex = receiverIndex
            };
            this.m_handlers.Remove(key);
        }

        public bool IsProcessingBuffer { get; private set; }

        public bool IsBuffering
        {
            get => 
                this.m_isBuffering;
            set
            {
                this.m_isBuffering = value;
                if (this.m_isBuffering && (this.m_buffer == null))
                {
                    this.m_buffer = new List<MyPacket>();
                }
                else if (!this.m_isBuffering && (this.m_buffer != null))
                {
                    this.ProcessBuffer();
                    this.m_buffer = null;
                }
            }
        }

        public Action<ulong> DisconnectPeerOnError { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTransportLayer.<>c <>9 = new MyTransportLayer.<>c();
            public static Func<int, Queue<int>> <>9__22_1;

            internal Queue<int> <.ctor>b__22_1(int s) => 
                new Queue<int>(120);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HandlerId
        {
            public MyMessageId messageId;
            public byte receiverIndex;
        }
    }
}

