namespace Sandbox.Game
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Utils;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyInventoryTransferEventContent
    {
        public MyFixedPoint Amount;
        public uint ItemId;
        public long SourceOwnerId;
        public MyStringHash SourceInventoryId;
        public long DestinationOwnerId;
        public MyStringHash DestinationInventoryId;
    }
}

