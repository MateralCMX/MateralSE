namespace VRage.GameServices
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyP2PSessionState
    {
        public bool ConnectionActive;
        public bool Connecting;
        public MyP2PSessionErrorEnum LastSessionError;
        public bool UsingRelay;
        public int BytesQueuedForSend;
        public int PacketsQueuedForSend;
        public uint RemoteIP;
        public ushort RemotePort;
    }
}

