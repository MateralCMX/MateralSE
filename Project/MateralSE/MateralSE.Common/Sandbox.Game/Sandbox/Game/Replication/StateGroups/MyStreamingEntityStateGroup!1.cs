namespace Sandbox.Game.Replication.StateGroups
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Utils;

    internal class MyStreamingEntityStateGroup<T> : IMyStateGroup, IMyNetObject, IMyEventOwner where T: IMyStreamableReplicable
    {
        private int m_streamSize;
        private const int HEADER_SIZE = 0x61;
        private const int SAFE_VALUE = 0x80;
        private bool m_streamed;
        private Dictionary<Endpoint, StreamClientData<T>> m_clientStreamData;
        private SortedList<StreamPartInfo<T>, byte[]> m_receivedParts;
        private short m_numPartsToReceive;
        private int m_receivedBytes;
        private int m_uncompressedSize;

        public MyStreamingEntityStateGroup(T obj, IMyReplicable owner)
        {
            this.m_streamSize = 0x1f40;
            this.Instance = obj;
            this.Owner = owner;
        }

        public void ClientUpdate(MyTimeSpan clientTimestamp)
        {
        }

        public void CreateClientData(MyClientStateBase forClient)
        {
            StreamClientData<T> data;
            if (this.m_clientStreamData == null)
            {
                this.m_clientStreamData = new Dictionary<Endpoint, StreamClientData<T>>();
            }
            if (!this.m_clientStreamData.TryGetValue(forClient.EndpointId, out data))
            {
                this.m_clientStreamData[forClient.EndpointId] = new StreamClientData<T>();
            }
            this.m_clientStreamData[forClient.EndpointId].Dirty = true;
        }

        private unsafe void CreateReplicable(int uncompressedSize)
        {
            byte* numPtr;
            byte[] pinned buffer2;
            byte[] dst = new byte[this.m_receivedBytes];
            int dstOffset = 0;
            foreach (KeyValuePair<StreamPartInfo<T>, byte[]> pair in this.m_receivedParts)
            {
                Buffer.BlockCopy(pair.Value, 0, dst, dstOffset, pair.Value.Length);
                dstOffset += pair.Value.Length;
            }
            BitStream stream = new BitStream(0x600);
            stream.ResetWrite();
            if (((buffer2 = MemoryCompressor.Decompress(dst)) == null) || (buffer2.Length == 0))
            {
                numPtr = null;
            }
            else
            {
                numPtr = buffer2;
            }
            stream.SerializeMemory((void*) numPtr, uncompressedSize);
            buffer2 = null;
            stream.ResetRead();
            this.Instance.LoadDone(stream);
            if (!stream.CheckTerminator())
            {
                MyLog.Default.WriteLine("Streaming entity: Invalid stream terminator");
            }
            stream.Dispose();
            if (this.m_receivedParts != null)
            {
                this.m_receivedParts.Clear();
            }
            this.m_receivedParts = null;
            this.m_receivedBytes = 0;
        }

        public void Destroy()
        {
            if (this.m_receivedParts != null)
            {
                this.m_receivedParts.Clear();
                this.m_receivedParts = null;
            }
        }

        public void DestroyClientData(MyClientStateBase forClient)
        {
            if (this.m_clientStreamData != null)
            {
                this.m_clientStreamData.Remove(forClient.EndpointId);
            }
        }

        public void ForceSend(MyClientStateBase clientData)
        {
            StreamClientData<T> data = this.m_clientStreamData[clientData.EndpointId];
            data.ForceSend = true;
            this.SaveReplicable(data, null, clientData.EndpointId);
        }

        public MyStreamProcessingState IsProcessingForClient(Endpoint forClient)
        {
            StreamClientData<T> data;
            return (!this.m_clientStreamData.TryGetValue(forClient, out data) ? MyStreamProcessingState.None : (data.CreatingData ? MyStreamProcessingState.Processing : ((data.ObjectData != null) ? MyStreamProcessingState.Finished : MyStreamProcessingState.None)));
        }

        public bool IsStillDirty(Endpoint forClient) => 
            this.m_clientStreamData[forClient].Dirty;

        public void OnAck(MyClientStateBase forClient, byte packetId, bool delivered)
        {
        }

        private void ProcessRead(BitStream stream)
        {
            if ((stream.BitLength != stream.BitPosition) && !this.m_streamed)
            {
                if (!stream.ReadBool())
                {
                    MyLog.Default.WriteLine("received empty state group");
                    if (this.m_receivedParts != null)
                    {
                        this.m_receivedParts.Clear();
                    }
                    this.m_receivedParts = null;
                    this.Instance.LoadCancel();
                }
                else
                {
                    this.m_uncompressedSize = stream.ReadInt32(0x20);
                    if (!this.ReadPart(ref stream))
                    {
                        this.m_receivedParts = null;
                        this.Instance.LoadCancel();
                    }
                    else if (this.m_receivedParts.Count == this.m_numPartsToReceive)
                    {
                        this.m_streamed = true;
                        this.CreateReplicable(this.m_uncompressedSize);
                    }
                }
            }
        }

        private void ProcessWrite(int maxBitPosition, BitStream stream, Endpoint forClient, byte packetId, HashSet<string> cachedData)
        {
            StreamClientData<T> clientData = this.m_clientStreamData[forClient];
            if (clientData.FailedIncompletePackets.Count > 0)
            {
                this.WriteIncompletePacket(clientData, packetId, ref stream);
            }
            else
            {
                int bitsToSend = 0;
                if (clientData.ObjectData == null)
                {
                    this.SaveReplicable(clientData, cachedData, forClient);
                }
                else
                {
                    this.m_streamSize = MyLibraryUtils.GetDivisionCeil(((maxBitPosition - stream.BitPosition) - 0x61) - 0x80, 8) * 8;
                    clientData.NumParts = (short) MyLibraryUtils.GetDivisionCeil(clientData.ObjectData.Length * 8, this.m_streamSize);
                    bitsToSend = clientData.RemainingBits;
                    if (bitsToSend == 0)
                    {
                        clientData.ForceSend = false;
                        clientData.Dirty = false;
                        stream.WriteBool(false);
                    }
                    else
                    {
                        stream.WriteBool(true);
                        stream.WriteInt32(clientData.UncompressedSize, 0x20);
                        if ((bitsToSend <= this.m_streamSize) && !clientData.Incomplete)
                        {
                            this.WriteWhole(bitsToSend, clientData, packetId, ref stream);
                        }
                        else
                        {
                            this.WritePart(ref bitsToSend, clientData, packetId, ref stream);
                            clientData.Incomplete = clientData.RemainingBits > 0;
                        }
                        if (clientData.RemainingBits == 0)
                        {
                            clientData.Dirty = false;
                            clientData.ForceSend = false;
                        }
                    }
                }
            }
        }

        private unsafe bool ReadPart(ref BitStream stream)
        {
            byte* numPtr;
            byte[] pinned buffer2;
            this.m_numPartsToReceive = stream.ReadInt16(0x10);
            short num = stream.ReadInt16(0x10);
            int num2 = stream.ReadInt32(0x20);
            int divisionCeil = MyLibraryUtils.GetDivisionCeil(num2, 8);
            int num4 = stream.BitLength - stream.BitPosition;
            if (num4 < num2)
            {
                string[] textArray1 = new string[10];
                textArray1[0] = "trying to read more than there is in stream. Total num parts : ";
                textArray1[1] = this.m_numPartsToReceive.ToString();
                textArray1[2] = " current part : ";
                textArray1[3] = num.ToString();
                textArray1[4] = " bits to read : ";
                textArray1[5] = num2.ToString();
                textArray1[6] = " bits in stream : ";
                textArray1[7] = num4.ToString();
                textArray1[8] = " replicable : ";
                textArray1[9] = this.Instance.ToString();
                MyLog.Default.WriteLine(string.Concat(textArray1));
                return false;
            }
            if (this.m_receivedParts == null)
            {
                this.m_receivedParts = new SortedList<StreamPartInfo<T>, byte[]>();
            }
            this.m_receivedBytes += divisionCeil;
            byte[] buffer = new byte[divisionCeil];
            if (((buffer2 = buffer) == null) || (buffer2.Length == 0))
            {
                numPtr = null;
            }
            else
            {
                numPtr = buffer2;
            }
            stream.ReadMemory((void*) numPtr, num2);
            buffer2 = null;
            StreamPartInfo<T> info = new StreamPartInfo<T> {
                NumBits = num2,
                StartIndex = num
            };
            this.m_receivedParts[info] = buffer;
            return true;
        }

        public void Reset(bool reinit, MyTimeSpan clientTimestamp)
        {
        }

        private void SaveReplicable(StreamClientData<T> clientData, HashSet<string> cachedData, Endpoint forClient)
        {
            BitStream str = new BitStream(0x600);
            str.ResetWrite();
            clientData.CreatingData = true;
            this.Instance.Serialize(str, cachedData, forClient, () => ((MyStreamingEntityStateGroup<T>) this).WriteClientData(str, clientData));
        }

        public void Serialize(BitStream stream, Endpoint forClient, MyTimeSpan serverTimestamp, MyTimeSpan lastClientTimestamp, byte packetId, int maxBitPosition, HashSet<string> cachedData)
        {
            if ((stream == null) || !stream.Reading)
            {
                this.ProcessWrite(maxBitPosition, stream, forClient, packetId, cachedData);
            }
            else
            {
                this.ProcessRead(stream);
            }
        }

        private unsafe void WriteClientData(BitStream str, StreamClientData<T> clientData)
        {
            byte* numPtr;
            byte[] pinned buffer2;
            str.Terminate();
            str.ResetRead();
            int bitLength = str.BitLength;
            byte[] bytes = new byte[str.ByteLength];
            if (((buffer2 = bytes) == null) || (buffer2.Length == 0))
            {
                numPtr = null;
            }
            else
            {
                numPtr = buffer2;
            }
            str.SerializeMemory((void*) numPtr, bitLength);
            buffer2 = null;
            str.Dispose();
            clientData.CurrentPart = 0;
            clientData.ObjectData = MemoryCompressor.Compress(bytes);
            clientData.UncompressedSize = bitLength;
            clientData.RemainingBits = clientData.ObjectData.Length * 8;
            clientData.CreatingData = false;
        }

        private unsafe void WriteIncompletePacket(StreamClientData<T> clientData, byte packetId, ref BitStream stream)
        {
            if (clientData.ObjectData == null)
            {
                clientData.FailedIncompletePackets.Clear();
            }
            else
            {
                StreamPartInfo<T> item = clientData.FailedIncompletePackets[0];
                clientData.FailedIncompletePackets.Remove(item);
                clientData.SendPackets[packetId] = item;
                stream.WriteBool(true);
                stream.WriteInt32(clientData.UncompressedSize, 0x20);
                stream.WriteInt16(clientData.NumParts, 0x10);
                stream.WriteInt16(item.Position, 0x10);
                stream.WriteInt32(item.NumBits, 0x20);
                byte* numPtr = &(clientData.ObjectData[item.StartIndex]);
                stream.WriteMemory((void*) numPtr, item.NumBits);
                fixed (byte* numRef = null)
                {
                    return;
                }
            }
        }

        private unsafe void WritePart(ref int bitsToSend, StreamClientData<T> clientData, byte packetId, ref BitStream stream)
        {
            bitsToSend = Math.Min(this.m_streamSize, clientData.RemainingBits);
            StreamPartInfo<T> info1 = new StreamPartInfo<T>();
            info1.StartIndex = clientData.LastPosition;
            info1.NumBits = bitsToSend;
            StreamPartInfo<T> info = info1;
            clientData.LastPosition = info.StartIndex + MyLibraryUtils.GetDivisionCeil(this.m_streamSize, 8);
            clientData.SendPackets[packetId] = info;
            clientData.RemainingBits = Math.Max(0, clientData.RemainingBits - this.m_streamSize);
            stream.WriteInt16(clientData.NumParts, 0x10);
            stream.WriteInt16(clientData.CurrentPart, 0x10);
            info.Position = clientData.CurrentPart;
            clientData.CurrentPart = (short) (clientData.CurrentPart + 1);
            stream.WriteInt32(bitsToSend, 0x20);
            byte* numPtr = &(clientData.ObjectData[info.StartIndex]);
            stream.WriteMemory((void*) numPtr, bitsToSend);
            fixed (byte* numRef = null)
            {
                return;
            }
        }

        private unsafe void WriteWhole(int bitsToSend, StreamClientData<T> clientData, byte packetId, ref BitStream stream)
        {
            byte* numPtr;
            byte[] pinned buffer;
            StreamPartInfo<T> info1 = new StreamPartInfo<T>();
            info1.StartIndex = 0;
            info1.NumBits = bitsToSend;
            info1.Position = 0;
            StreamPartInfo<T> info = info1;
            clientData.SendPackets[packetId] = info;
            clientData.RemainingBits = 0;
            clientData.Dirty = false;
            clientData.ForceSend = false;
            stream.WriteInt16(1, 0x10);
            stream.WriteInt16(0, 0x10);
            stream.WriteInt32(bitsToSend, 0x20);
            if (((buffer = clientData.ObjectData) == null) || (buffer.Length == 0))
            {
                numPtr = null;
            }
            else
            {
                numPtr = buffer;
            }
            stream.WriteMemory((void*) numPtr, bitsToSend);
            buffer = null;
        }

        private T Instance { get; set; }

        public IMyReplicable Owner { get; private set; }

        public bool NeedsUpdate =>
            false;

        public bool IsValid =>
            ((this.Owner != null) && this.Owner.IsValid);

        public bool IsHighPriority =>
            false;

        public bool IsStreaming =>
            true;

        private class StreamClientData
        {
            public short CurrentPart;
            public short NumParts;
            public int LastPosition;
            public byte[] ObjectData;
            public bool CreatingData;
            public bool Incomplete;
            public bool Dirty;
            public int RemainingBits;
            public int UncompressedSize;
            public bool ForceSend;
            public readonly Dictionary<byte, MyStreamingEntityStateGroup<T>.StreamPartInfo> SendPackets;
            public readonly List<MyStreamingEntityStateGroup<T>.StreamPartInfo> FailedIncompletePackets;

            public StreamClientData()
            {
                this.SendPackets = new Dictionary<byte, MyStreamingEntityStateGroup<T>.StreamPartInfo>();
                this.FailedIncompletePackets = new List<MyStreamingEntityStateGroup<T>.StreamPartInfo>();
            }
        }

        private class StreamPartInfo : IComparable<MyStreamingEntityStateGroup<T>.StreamPartInfo>
        {
            public int StartIndex;
            public int NumBits;
            public short Position;

            public int CompareTo(MyStreamingEntityStateGroup<T>.StreamPartInfo b) => 
                this.StartIndex.CompareTo(b.StartIndex);
        }
    }
}

