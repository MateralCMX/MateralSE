namespace VRage.Network
{
    using System;

    public enum CallSiteFlags
    {
        None = 0,
        Client = 1,
        Server = 2,
        Broadcast = 4,
        Reliable = 8,
        RefreshReplicable = 0x10,
        BroadcastExcept = 0x20,
        Blocking = 0x40,
        ServerInvoked = 0x80
    }
}

