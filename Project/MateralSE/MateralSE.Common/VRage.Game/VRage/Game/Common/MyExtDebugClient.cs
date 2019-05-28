namespace VRage.Game.Common
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Utils;

    public class MyExtDebugClient : IDisposable
    {
        public const int GameDebugPort = 0x32c8;
        private const int MsgSizeLimit = 0x2800;
        private TcpClient m_client;
        private readonly byte[] m_arrayBuffer = new byte[0x2800];
        private IntPtr m_tempBuffer = Marshal.AllocHGlobal(0x2800);
        private Thread m_clientThread;
        private bool m_finished = false;
        private readonly ConcurrentCachingList<ReceivedMsgHandler> m_receivedMsgHandlers = new ConcurrentCachingList<ReceivedMsgHandler>();

        public event ReceivedMsgHandler ReceivedMsg
        {
            add
            {
                if (!this.m_receivedMsgHandlers.Contains<ReceivedMsgHandler>(value))
                {
                    this.m_receivedMsgHandlers.Add(value);
                    this.m_receivedMsgHandlers.ApplyAdditions();
                }
            }
            remove
            {
                if (this.m_receivedMsgHandlers.Contains<ReceivedMsgHandler>(value))
                {
                    this.m_receivedMsgHandlers.Remove(value, false);
                    this.m_receivedMsgHandlers.ApplyRemovals();
                }
            }
        }

        public MyExtDebugClient()
        {
            Thread thread1 = new Thread(new ThreadStart(this.ClientThreadProc));
            thread1.IsBackground = true;
            this.m_clientThread = thread1;
            this.m_clientThread.Start();
        }

        private void ClientThreadProc()
        {
            while (true)
            {
                while (true)
                {
                    if (this.m_finished)
                    {
                        return;
                    }
                    if (((this.m_client == null) || (this.m_client.Client == null)) || !this.m_client.Connected)
                    {
                        if (!MyTryConnectHelper.TryConnect(IPAddress.Loopback.ToString(), 0x32c8))
                        {
                            Thread.Sleep(0x9c4);
                            continue;
                        }
                        try
                        {
                            this.m_client = new TcpClient();
                            this.m_client.Connect(IPAddress.Loopback, 0x32c8);
                        }
                        catch (Exception)
                        {
                        }
                        if (((this.m_client == null) || (this.m_client.Client == null)) || !this.m_client.Connected)
                        {
                            Thread.Sleep(0x9c4);
                            continue;
                        }
                    }
                    break;
                }
                try
                {
                    if (this.m_client.Client != null)
                    {
                        if (this.m_client.Client.Receive(this.m_arrayBuffer, 0, MyExternalDebugStructures.MsgHeaderSize, SocketFlags.None) == 0)
                        {
                            this.m_client.Client.Close();
                            this.m_client.Client = null;
                            this.m_client = null;
                        }
                        else
                        {
                            Marshal.Copy(this.m_arrayBuffer, 0, this.m_tempBuffer, MyExternalDebugStructures.MsgHeaderSize);
                            MyExternalDebugStructures.CommonMsgHeader messageHeader = (MyExternalDebugStructures.CommonMsgHeader) Marshal.PtrToStructure(this.m_tempBuffer, typeof(MyExternalDebugStructures.CommonMsgHeader));
                            if (messageHeader.IsValid)
                            {
                                this.m_client.Client.Receive(this.m_arrayBuffer, messageHeader.MsgSize, SocketFlags.None);
                                if (this.m_receivedMsgHandlers != null)
                                {
                                    Marshal.Copy(this.m_arrayBuffer, 0, this.m_tempBuffer, messageHeader.MsgSize);
                                    foreach (ReceivedMsgHandler handler in this.m_receivedMsgHandlers)
                                    {
                                        if (handler != null)
                                        {
                                            handler(messageHeader, this.m_tempBuffer);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    if (this.m_client.Client != null)
                    {
                        this.m_client.Client.Close();
                        this.m_client.Client = null;
                        this.m_client = null;
                    }
                }
                catch (ObjectDisposedException)
                {
                    if (this.m_client.Client != null)
                    {
                        this.m_client.Client.Close();
                        this.m_client.Client = null;
                        this.m_client = null;
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void Dispose()
        {
            this.m_finished = true;
            if (this.m_client != null)
            {
                this.m_client.Client.Disconnect(false);
                this.m_client.Close();
            }
            Marshal.FreeHGlobal(this.m_tempBuffer);
        }

        public bool SendMessageToGame<TMessage>(TMessage msg) where TMessage: MyExternalDebugStructures.IExternalDebugMsg
        {
            if (((this.m_client == null) || (this.m_client.Client == null)) || !this.m_client.Connected)
            {
                return false;
            }
            int msgSize = Marshal.SizeOf(typeof(TMessage));
            Marshal.StructureToPtr<MyExternalDebugStructures.CommonMsgHeader>(MyExternalDebugStructures.CommonMsgHeader.Create(msg.GetTypeStr(), msgSize), this.m_tempBuffer, true);
            Marshal.Copy(this.m_tempBuffer, this.m_arrayBuffer, 0, MyExternalDebugStructures.MsgHeaderSize);
            Marshal.StructureToPtr<TMessage>(msg, this.m_tempBuffer, true);
            Marshal.Copy(this.m_tempBuffer, this.m_arrayBuffer, MyExternalDebugStructures.MsgHeaderSize, msgSize);
            try
            {
                this.m_client.Client.Send(this.m_arrayBuffer, 0, MyExternalDebugStructures.MsgHeaderSize + msgSize, SocketFlags.None);
            }
            catch (SocketException)
            {
                return false;
            }
            return true;
        }

        public bool ConnectedToGame =>
            ((this.m_client != null) && this.m_client.Connected);

        public delegate void ReceivedMsgHandler(MyExternalDebugStructures.CommonMsgHeader messageHeader, IntPtr messageData);
    }
}

