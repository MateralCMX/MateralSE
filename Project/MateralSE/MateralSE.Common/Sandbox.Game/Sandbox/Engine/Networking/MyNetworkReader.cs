namespace Sandbox.Engine.Networking
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Utils;

    internal static class MyNetworkReader
    {
        private static int m_byteCountReceived;
        private static int m_tamperred;
        private static readonly ConcurrentDictionary<int, ChannelInfo> m_channels = new ConcurrentDictionary<int, ChannelInfo>();

        public static void Clear()
        {
            foreach (KeyValuePair<int, ChannelInfo> pair in m_channels)
            {
                pair.Value.Queue.Dispose();
            }
            m_channels.Clear();
            MyLog.Default.WriteLine("Network readers disposed");
        }

        public static void ClearHandler(int channel)
        {
            ChannelInfo info;
            if (m_channels.TryGetValue(channel, out info))
            {
                info.Queue.Dispose();
            }
            m_channels.Remove<int, ChannelInfo>(channel);
        }

        public static void GetAndClearStats(out int received, out int tamperred)
        {
            received = Interlocked.Exchange(ref m_byteCountReceived, 0);
            tamperred = Interlocked.Exchange(ref m_tamperred, 0);
        }

        public static void Process()
        {
            foreach (KeyValuePair<int, ChannelInfo> pair in m_channels)
            {
                pair.Value.Queue.Process(pair.Value.Handler);
            }
        }

        public static void ReceiveAll()
        {
            int num = 0;
            int num2 = 0;
            foreach (KeyValuePair<int, ChannelInfo> pair in m_channels)
            {
                while (true)
                {
                    uint num3;
                    MyReceiveQueue.ReceiveStatus status = pair.Value.Queue.ReceiveOne(out num3);
                    if (status == MyReceiveQueue.ReceiveStatus.None)
                    {
                        break;
                    }
                    num += (int) num3;
                    if (status == MyReceiveQueue.ReceiveStatus.TamperredPacket)
                    {
                        num2++;
                    }
                }
            }
            Interlocked.Add(ref m_byteCountReceived, num);
            Interlocked.Add(ref m_tamperred, num2);
        }

        public static void SetHandler(int channel, NetworkMessageDelegate handler, Action<ulong> disconnectPeerOnError)
        {
            ChannelInfo info;
            if (m_channels.TryGetValue(channel, out info))
            {
                info.Queue.Dispose();
            }
            ChannelInfo info1 = new ChannelInfo();
            info1.Handler = handler;
            info1.Queue = new MyReceiveQueue(channel, disconnectPeerOnError);
            info = info1;
            m_channels[channel] = info;
        }

        private class ChannelInfo
        {
            public MyReceiveQueue Queue;
            public NetworkMessageDelegate Handler;
        }
    }
}

