namespace VRage.Network
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Serialization;

    [StructLayout(LayoutKind.Sequential)]
    public struct ConnectedClientDataMsg
    {
        public ulong SteamID;
        [Serialize(MyObjectFlags.DefaultZero)]
        public string Name;
        public bool IsAdmin;
        public bool Join;
        [Serialize(MyObjectFlags.DefaultZero)]
        public byte[] Token;
        public bool ExperimentalMode;
    }
}

