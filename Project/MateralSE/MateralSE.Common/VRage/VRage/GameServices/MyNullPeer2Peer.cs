namespace VRage.GameServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Utils;

    public class MyNullPeer2Peer : IMyPeer2Peer
    {
        private static readonly StringBuilder m_errorBuilder = new StringBuilder("Attempted to use method from NULL Peer2Peer.");

        public event Action<ulong, string> ConnectionFailed
        {
            add
            {
                ErrorMsg();
            }
            remove
            {
                ErrorMsg();
            }
        }

        public event Action<ulong> SessionRequest
        {
            add
            {
                ErrorMsg();
            }
            remove
            {
                ErrorMsg();
            }
        }

        public bool AcceptSession(ulong remotePeerId)
        {
            ErrorMsg();
            return false;
        }

        public bool CloseSession(ulong remotePeerId)
        {
            ErrorMsg();
            return false;
        }

        private static void ErrorMsg()
        {
            MyLog.Default.Log(MyLogSeverity.Error, m_errorBuilder.ToString(), Array.Empty<object>());
        }

        public bool GetSessionState(ulong remoteUser, ref MyP2PSessionState state)
        {
            state = new MyP2PSessionState();
            return false;
        }

        public bool IsPacketAvailable(out uint msgSize, int channel)
        {
            ErrorMsg();
            msgSize = 0;
            return false;
        }

        public bool ReadPacket(byte[] buffer, ref uint dataSize, out ulong remoteUser, int channel)
        {
            ErrorMsg();
            dataSize = 0;
            remoteUser = 0L;
            return false;
        }

        public bool SendPacket(ulong remoteUser, byte[] data, int byteCount, MyP2PMessageEnum msgType, int channel)
        {
            ErrorMsg();
            return false;
        }

        public unsafe bool SendPacket(ulong remoteUser, byte* data, int byteCount, MyP2PMessageEnum msgType, int channel)
        {
            ErrorMsg();
            return false;
        }

        public void SetServer(bool server)
        {
        }
    }
}

