namespace VRage.Replication
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;
    using VRage.Library.Utils;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyPacketStatistics
    {
        public int Duplicates;
        public int Drops;
        public int OutOfOrder;
        public int Tamperred;
        public int OutgoingData;
        public int IncomingData;
        public float TimeInterval;
        public byte PendingPackets;
        public float GCMemory;
        public float ProcessMemory;
        private MyTimeSpan m_nextTime;
        private static readonly MyTimeSpan SEND_TIMEOUT;
        public void Reset()
        {
            int num;
            this.Tamperred = num = 0;
            this.IncomingData = num = num;
            this.OutgoingData = num = num;
            this.Drops = num = num;
            this.Duplicates = this.OutOfOrder = num;
            this.TimeInterval = 0f;
        }

        public void UpdateData(int outgoing, int incoming, int incomingTamperred, float gcMemory, float processMemory)
        {
            this.OutgoingData += outgoing;
            this.IncomingData += incoming;
            this.Tamperred += incomingTamperred;
            this.GCMemory = gcMemory;
            this.ProcessMemory = processMemory;
        }

        public void Update(MyPacketTracker.OrderType type)
        {
            switch (type)
            {
                case MyPacketTracker.OrderType.InOrder:
                    break;

                case MyPacketTracker.OrderType.OutOfOrder:
                    this.OutOfOrder++;
                    return;

                case MyPacketTracker.OrderType.Duplicate:
                    this.Duplicates++;
                    return;

                default:
                    this.Drops += (type - 3) + 1;
                    break;
            }
        }

        public void Write(BitStream sendStream, MyTimeSpan currentTime)
        {
            if (currentTime <= this.m_nextTime)
            {
                sendStream.WriteBool(false);
            }
            else
            {
                sendStream.WriteBool(true);
                sendStream.WriteByte((byte) this.Duplicates, 8);
                sendStream.WriteByte((byte) this.OutOfOrder, 8);
                sendStream.WriteByte((byte) this.Drops, 8);
                sendStream.WriteByte((byte) this.Tamperred, 8);
                sendStream.WriteInt32(this.OutgoingData, 0x20);
                sendStream.WriteInt32(this.IncomingData, 0x20);
                sendStream.WriteFloat((float) ((currentTime - this.m_nextTime) + SEND_TIMEOUT).Seconds);
                sendStream.WriteByte(this.PendingPackets, 8);
                sendStream.WriteFloat(this.GCMemory);
                sendStream.WriteFloat(this.ProcessMemory);
                this.Reset();
                this.m_nextTime = currentTime + SEND_TIMEOUT;
            }
        }

        public void Read(BitStream receiveStream)
        {
            if (receiveStream.ReadBool())
            {
                this.Duplicates = receiveStream.ReadByte(8);
                this.OutOfOrder = receiveStream.ReadByte(8);
                this.Drops = receiveStream.ReadByte(8);
                this.Tamperred = receiveStream.ReadByte(8);
                this.OutgoingData = receiveStream.ReadInt32(0x20);
                this.IncomingData = receiveStream.ReadInt32(0x20);
                this.TimeInterval = receiveStream.ReadFloat();
                this.PendingPackets = receiveStream.ReadByte(8);
                this.GCMemory = receiveStream.ReadFloat();
                this.ProcessMemory = receiveStream.ReadFloat();
            }
        }

        public void Add(MyPacketStatistics statistics)
        {
            this.Duplicates += statistics.Duplicates;
            this.OutOfOrder += statistics.OutOfOrder;
            this.Drops += statistics.Drops;
            this.Tamperred += statistics.Tamperred;
            this.OutgoingData += statistics.OutgoingData;
            this.IncomingData += statistics.IncomingData;
            this.TimeInterval += statistics.TimeInterval;
            this.PendingPackets = statistics.PendingPackets;
            this.GCMemory = statistics.GCMemory;
            this.ProcessMemory = statistics.ProcessMemory;
        }

        static MyPacketStatistics()
        {
            SEND_TIMEOUT = MyTimeSpan.FromSeconds(0.10000000149011612);
        }
    }
}

