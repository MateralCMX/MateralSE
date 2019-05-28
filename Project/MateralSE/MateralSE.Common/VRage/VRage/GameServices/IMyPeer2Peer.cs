namespace VRage.GameServices
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public interface IMyPeer2Peer
    {
        event Action<ulong, string> ConnectionFailed;

        event Action<ulong> SessionRequest;

        bool AcceptSession(ulong remotePeerId);
        bool CloseSession(ulong remotePeerId);
        bool GetSessionState(ulong remoteUser, ref MyP2PSessionState state);
        bool IsPacketAvailable(out uint msgSize, int channel);
        bool ReadPacket(byte[] buffer, ref uint dataSize, out ulong remoteUser, int channel);
        bool SendPacket(ulong remoteUser, byte[] data, int byteCount, MyP2PMessageEnum msgType, int channel);
        unsafe bool SendPacket(ulong remoteUser, byte* data, int byteCount, MyP2PMessageEnum msgType, int channel);
        void SetServer(bool server);
    }
}

