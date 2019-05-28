namespace VRage.Network
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct JoinResultMsg
    {
        public VRage.Network.JoinResult JoinResult;
        public bool ServerExperimental;
        public ulong Admin;
    }
}

