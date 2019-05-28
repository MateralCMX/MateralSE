namespace Sandbox.Engine.Networking
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.GameServices;
    using VRage.Network;

    internal static class MyNetworkWriter
    {
        public const byte PACKET_MAGIC = 0xce;
        public const int SIZE_MTR = 0xf4240;
        private static int m_byteCountSent;
        private static readonly ConcurrentQueue<MyPacketDescriptor> m_packetsToSend = new ConcurrentQueue<MyPacketDescriptor>();
        private static readonly MyConcurrentPool<MyPacketDescriptor> m_descriptorPool;
        private static readonly MyConcurrentPool<MyPacketDataBitStream> m_bitStreamPool;
        private static readonly MyConcurrentPool<MyPacketDataArray> m_arrayPacketPool;
        private static readonly Crc32 m_crc32;
        private static readonly List<IPacketData> m_packetsTmp;
        private static readonly ByteStream m_streamTmp;
        public const int PACKET_HEADER_SIZE = 10;

        static MyNetworkWriter()
        {
            m_descriptorPool = new MyConcurrentPool<MyPacketDescriptor>(0, x => x.Reset(), 0x2710, null);
            m_bitStreamPool = new MyConcurrentPool<MyPacketDataBitStream>(0, null, 0x2710, null);
            m_arrayPacketPool = new MyConcurrentPool<MyPacketDataArray>(0, null, 0x2710, null);
            m_crc32 = new Crc32();
            m_packetsTmp = new List<IPacketData>();
            m_streamTmp = new ByteStream(0xf4640, true);
        }

        public static int GetAndClearStats() => 
            Interlocked.Exchange(ref m_byteCountSent, 0);

        public static MyPacketDataBitStreamBase GetBitStreamPacketData()
        {
            MyPacketDataBitStream local1 = m_bitStreamPool.Get();
            local1.Acquire();
            return local1;
        }

        public static IPacketData GetPacketData(IntPtr data, int offset, int size)
        {
            MyPacketDataArray local1 = m_arrayPacketPool.Get();
            local1.Ptr = data;
            local1.Offset = offset;
            local1.Size = size;
            return local1;
        }

        public static IPacketData GetPacketData(byte[] data, int offset, int size)
        {
            MyPacketDataArray local1 = m_arrayPacketPool.Get();
            local1.Data = data;
            local1.Offset = offset;
            local1.Size = size;
            return local1;
        }

        internal static MyPacketDescriptor GetPacketDescriptor(EndpointId userId, MyP2PMessageEnum msgType, int channel)
        {
            MyPacketDescriptor descriptor = m_descriptorPool.Get();
            descriptor.MsgType = msgType;
            descriptor.Channel = channel;
            if (userId.IsValid)
            {
                descriptor.Recipients.Add(userId);
            }
            return descriptor;
        }

        private static bool IsLastReliable(int channel)
        {
            using (IEnumerator<MyPacketDescriptor> enumerator = m_packetsToSend.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyPacketDescriptor current = enumerator.Current;
                    if ((current.Channel == channel) && ((current.MsgType == MyP2PMessageEnum.Reliable) || (current.MsgType == MyP2PMessageEnum.ReliableWithBuffering)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void SendAll()
        {
            MyPacketDescriptor descriptor;
            int num = 0;
            while (m_packetsToSend.TryDequeue(out descriptor))
            {
                int num1;
                m_packetsTmp.Clear();
                m_packetsTmp.Add(descriptor.Data);
                uint maxValue = uint.MaxValue;
                MyP2PMessageEnum msgType = descriptor.MsgType;
                if (msgType == MyP2PMessageEnum.Reliable)
                {
                    if ((descriptor.Data != null) && (descriptor.Data.Size >= 0xf423e))
                    {
                        Split(descriptor);
                    }
                }
                else if (msgType == MyP2PMessageEnum.ReliableWithBuffering)
                {
                    if (IsLastReliable(descriptor.Channel))
                    {
                        descriptor.MsgType = MyP2PMessageEnum.Reliable;
                    }
                    if ((descriptor.Data != null) && (descriptor.Data.Size >= 0xf423e))
                    {
                        Split(descriptor);
                    }
                }
                byte num3 = 0;
                if ((descriptor.MsgType == MyP2PMessageEnum.Unreliable) || (descriptor.MsgType == MyP2PMessageEnum.UnreliableNoDelay))
                {
                    num1 = 1;
                }
                else
                {
                    num1 = 0;
                }
                byte num4 = (byte) num1;
                foreach (IPacketData data in m_packetsTmp)
                {
                    m_streamTmp.Position = 0L;
                    m_streamTmp.WriteNoAlloc((byte) 0xce);
                    m_streamTmp.WriteNoAlloc(num4);
                    int position = (int) m_streamTmp.Position;
                    m_streamTmp.WriteNoAlloc(maxValue);
                    m_streamTmp.WriteNoAlloc(num3);
                    m_streamTmp.WriteNoAlloc((byte) m_packetsTmp.Count);
                    if (num3 == 0)
                    {
                        m_streamTmp.Write(descriptor.Header.Data, 0, (int) descriptor.Header.Position);
                    }
                    if (data != null)
                    {
                        int count = Math.Min(data.Size, 0xf4240);
                        if ((num3 == 0) && (m_packetsTmp.Count > 1))
                        {
                            count -= (int) descriptor.Header.Position;
                        }
                        if (data.Data != null)
                        {
                            m_streamTmp.Write(data.Data, data.Offset, count);
                        }
                        else
                        {
                            m_streamTmp.Write(data.Ptr, data.Offset, count);
                        }
                    }
                    if (num4 > 0)
                    {
                        int num7 = (int) m_streamTmp.Position;
                        m_crc32.Initialize();
                        m_crc32.ComputeHash(m_streamTmp.Data, position + 4, (num7 - position) - 4);
                        maxValue = m_crc32.CrcValue;
                        m_streamTmp.Position = position;
                        m_streamTmp.WriteNoAlloc(maxValue);
                        m_streamTmp.Position = num7;
                    }
                    foreach (EndpointId id in descriptor.Recipients)
                    {
                        num += (int) m_streamTmp.Position;
                        MyGameService.Peer2Peer.SendPacket(id.Value, m_streamTmp.Data, (int) m_streamTmp.Position, descriptor.MsgType, descriptor.Channel);
                    }
                    num3 = (byte) (num3 + 1);
                }
                foreach (IPacketData data2 in m_packetsTmp)
                {
                    if (data2 != null)
                    {
                        data2.Return();
                    }
                }
                descriptor.Data = null;
                m_descriptorPool.Return(descriptor);
            }
            Interlocked.Add(ref m_byteCountSent, num);
        }

        public static void SendPacket(MyPacketDescriptor packet)
        {
            m_packetsToSend.Enqueue(packet);
        }

        private static void Split(MyPacketDescriptor packet)
        {
            int offset = 0xf4240 - ((int) packet.Header.Position);
            int size = packet.Data.Size;
            while (offset < size)
            {
                int num3 = Math.Min(size - offset, 0xf4240);
                IPacketData item = (packet.Data.Data == null) ? GetPacketData(packet.Data.Ptr, offset, num3) : GetPacketData(packet.Data.Data, offset, num3);
                m_packetsTmp.Add(item);
                offset += num3;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyNetworkWriter.<>c <>9 = new MyNetworkWriter.<>c();

            internal void <.cctor>b__23_0(MyNetworkWriter.MyPacketDescriptor x)
            {
                x.Reset();
            }
        }

        private class MyPacketDataArray : IPacketData
        {
            public void Return()
            {
                this.Data = null;
                this.Ptr = IntPtr.Zero;
                MyNetworkWriter.m_arrayPacketPool.Return(this);
            }

            public byte[] Data { get; set; }

            public IntPtr Ptr { get; set; }

            public int Size { get; set; }

            public int Offset { get; set; }
        }

        private class MyPacketDataBitStream : MyPacketDataBitStreamBase
        {
            public void Acquire()
            {
                base.m_returned = false;
            }

            public override void Return()
            {
                base.Stream.ResetWrite();
                base.m_returned = true;
                MyNetworkWriter.m_bitStreamPool.Return(this);
            }

            public override IntPtr Ptr =>
                base.Stream.DataPointer;

            public override int Size =>
                base.Stream.BytePosition;

            public override byte[] Data =>
                null;

            public override int Offset =>
                0;
        }

        public class MyPacketDescriptor
        {
            public readonly List<EndpointId> Recipients = new List<EndpointId>();
            public MyP2PMessageEnum MsgType;
            public int Channel;
            public readonly ByteStream Header = new ByteStream(0x10, true);
            public IPacketData Data;

            public void Reset()
            {
                if (this.Data != null)
                {
                    this.Data.Return();
                }
                this.Header.Position = 0L;
                this.Data = null;
                this.Recipients.Clear();
            }
        }
    }
}

