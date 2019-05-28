namespace VRage.Network
{
    using System;
    using VRage.Sync;

    public interface IMySyncedEntity
    {
        VRage.Sync.SyncType SyncType { get; set; }
    }
}

