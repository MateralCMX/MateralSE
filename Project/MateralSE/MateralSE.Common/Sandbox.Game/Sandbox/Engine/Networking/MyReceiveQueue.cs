namespace Sandbox.Engine.Networking
{
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using VRage;
    using VRage.Collections;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    internal sealed class MyReceiveQueue : IDisposable
    {
        private static readonly MyConcurrentBucketPool<MyPacketDataPooled> m_messagePool = new MyConcurrentBucketPool<MyPacketDataPooled, MessageAllocator>("MessagePool");
        private readonly ConcurrentQueue<MyPacket> m_receiveQueue;
        private readonly Func<MyTimeSpan> m_timestampProvider;
        private readonly int m_channel;
        private bool m_started;
        private readonly Action<ulong> m_disconnectPeerOnError;
        private static readonly Crc32 m_crc32 = new Crc32();
        private byte[] m_largePacket;
        private int m_largePacketCounter;
        private const int PACKET_HEADER_SIZE = 8;

        public MyReceiveQueue(int channel, Action<ulong> disconnectPeerOnError)
        {
            this.m_channel = channel;
            this.m_receiveQueue = new ConcurrentQueue<MyPacket>();
            this.m_disconnectPeerOnError = disconnectPeerOnError;
        }

        private unsafe bool CheckCrc(byte[] data, int crcIndex, int dataIndex, int dataLength)
        {
            byte* numPtr;
            byte[] pinned buffer;
            m_crc32.Initialize();
            m_crc32.ComputeHash(data, dataIndex, dataLength);
            if (((buffer = data) == null) || (buffer.Length == 0))
            {
                numPtr = null;
            }
            else
            {
                numPtr = buffer;
            }
            buffer = null;
            return (*(((uint*) (numPtr + crcIndex))) == m_crc32.CrcValue);
        }

        public void Dispose()
        {
            MyPacket packet;
            while (this.m_receiveQueue.TryDequeue(out packet))
            {
                packet.Return();
            }
        }

        private MyPacketDataPooled GetMessage(int size) => 
            m_messagePool.Get(MathHelper.GetNearestBiggerPowerOfTwo(Math.Max(size, 0x100)));

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        public void Process(NetworkMessageDelegate handler)
        {
            MyPacket packet;
            while (this.m_receiveQueue.TryDequeue(out packet))
            {
                try
                {
                    handler(packet);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                    MyLog.Default.WriteLine("Packet processing error, disconnecting " + packet.Sender.Id);
                    if (!Sync.IsServer)
                    {
                        throw;
                    }
                    this.m_disconnectPeerOnError(packet.Sender.Id.Value);
                }
            }
        }

        public ReceiveStatus ReceiveOne(out uint length)
        {
            ulong num;
            length = 0;
            if ((!MyGameService.IsActive && ((MyGameService.GameServer == null) || !MyGameService.GameServer.Running)) || !MyGameService.Peer2Peer.IsPacketAvailable(out length, this.m_channel))
            {
                return ReceiveStatus.None;
            }
            MyPacketDataPooled message = this.GetMessage((int) length);
            if (!MyGameService.Peer2Peer.ReadPacket(message.Data, ref length, out num, this.m_channel))
            {
                return ReceiveStatus.None;
            }
            MyPacketData item = message;
            bool flag = message.Data[1] > 0;
            if ((message.Data[0] != 0xce) || (flag && !this.CheckCrc(message.Data, 2, 6, ((int) length) - 6)))
            {
                string str = null;
                for (int i = 0; i < Math.Min(10, length); i++)
                {
                    str = str + message.Data[i].ToString("X ");
                }
                MyLog.Default.WriteLine($"ERROR! Invalid packet from channel #{this.m_channel} length {(uint) length} from {num} initial bytes: {str}");
                message.Return();
                return ReceiveStatus.TamperredPacket;
            }
            byte num2 = message.Data[6];
            byte num3 = message.Data[7];
            int byteOffset = 8;
            if (num3 > 1)
            {
                if (num2 == 0)
                {
                    int num6 = num3 * 0xf4240;
                    this.m_largePacket = new byte[num6];
                }
                Array.Copy(message.Data, 8L, this.m_largePacket, (long) (num2 * 0xf4240), (long) (length - 8));
                this.m_largePacketCounter++;
                message.Return();
                if (num2 != (num3 - 1))
                {
                    return this.ReceiveOne(out length);
                }
                MyPacketData data1 = new MyPacketData();
                data1.Data = this.m_largePacket;
                data1.BitStream = new BitStream(0);
                data1.ByteStream = new ByteStream();
                item = data1;
                byteOffset = 0;
                length -= 8;
                length = (uint) (length + ((num3 - 1) * 0xf4240));
                this.m_largePacketCounter = 0;
                this.m_largePacket = null;
            }
            item.BitStream.ResetRead(item.Data, byteOffset, (((int) ((ulong) length)) - byteOffset) * 8, false);
            item.ByteStream.Reset(item.Data, (int) length);
            item.ByteStream.Position = byteOffset;
            item.ReceivedTime = MyTimeSpan.FromTicks(Stopwatch.GetTimestamp());
            item.Sender = new Endpoint(num, 0);
            this.m_receiveQueue.Enqueue(item);
            return ReceiveStatus.Success;
        }

        private class MessageAllocator : IMyElementAllocator<MyReceiveQueue.MyPacketDataPooled>
        {
            public MyReceiveQueue.MyPacketDataPooled Allocate(int size)
            {
                MyReceiveQueue.MyPacketDataPooled pooled1 = new MyReceiveQueue.MyPacketDataPooled();
                pooled1.Data = new byte[size];
                pooled1.BitStream = new BitStream(0);
                pooled1.ByteStream = new ByteStream();
                return pooled1;
            }

            public void Dispose(MyReceiveQueue.MyPacketDataPooled message)
            {
                message.Data = null;
                message.BitStream.Dispose();
                message.BitStream = null;
                message.ByteStream = null;
            }

            public int GetBucketId(MyReceiveQueue.MyPacketDataPooled message) => 
                message.Data.Length;

            public int GetBytes(MyReceiveQueue.MyPacketDataPooled message) => 
                message.Data.Length;

            public void Init(MyReceiveQueue.MyPacketDataPooled message)
            {
                message.Init();
            }
        }

        private class MyPacketData : MyPacket
        {
            public byte[] Data;

            public override void Return()
            {
                this.Data = null;
                base.BitStream.Dispose();
                base.BitStream = null;
                base.ByteStream = null;
            }
        }

        private class MyPacketDataPooled : MyReceiveQueue.MyPacketData
        {
            private bool m_returned;

            public void Init()
            {
                this.m_returned = false;
            }

            public override void Return()
            {
                this.m_returned = true;
                MyReceiveQueue.m_messagePool.Return(this);
            }
        }

        public enum ReceiveStatus
        {
            None,
            TamperredPacket,
            Success
        }
    }
}

