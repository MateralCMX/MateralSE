namespace VRage.Network
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Library.Utils;
    using VRage.Serialization;

    [StructLayout(LayoutKind.Sequential)]
    public struct ServerDataMsg
    {
        [Serialize(MyObjectFlags.DefaultZero)]
        public string WorldName;
        public MyGameModeEnum GameMode;
        public float InventoryMultiplier;
        public float AssemblerMultiplier;
        public float RefineryMultiplier;
        [Serialize(MyObjectFlags.DefaultZero)]
        public string HostName;
        public ulong WorldSize;
        public int AppVersion;
        public int MembersLimit;
        [Serialize(MyObjectFlags.DefaultZero)]
        public string DataHash;
        public float WelderMultiplier;
        public float GrinderMultiplier;
        public float BlocksInventoryMultiplier;
        [Serialize(MyObjectFlags.DefaultZero)]
        public string ServerPasswordSalt { get; set; }
    }
}

