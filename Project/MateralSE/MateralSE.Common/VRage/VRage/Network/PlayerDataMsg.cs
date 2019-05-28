namespace VRage.Network
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Serialization;

    [StructLayout(LayoutKind.Sequential)]
    public struct PlayerDataMsg
    {
        public ulong ClientSteamId;
        public int PlayerSerialId;
        public long IdentityId;
        [Serialize(MyObjectFlags.DefaultZero)]
        public string DisplayName;
        [Serialize(MyObjectFlags.DefaultZero)]
        public List<Vector3> BuildColors;
        public bool RealPlayer;
        public bool NewIdentity;
    }
}

