namespace VRage.Replication
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;

    public interface IReplicationServerCallback
    {
        void DisconnectClient(ulong clientId);
        MyPacketDataBitStreamBase GetBitStreamPacketData();
        int GetMTRSize(Endpoint clientId);
        int GetMTUSize(Endpoint clientId);
        IMyReplicable GetReplicableByEntityId(long entityId);
        MyTimeSpan GetUpdateTime();
        void SendEvent(IPacketData data, bool reliable, List<EndpointId> endpoints);
        void SendJoinResult(IPacketData data, EndpointId endpoint);
        void SendPendingReplicablesDone(Endpoint endpoint);
        void SendPlayerData(IPacketData data, List<EndpointId> endpoints);
        void SendReplicationCreate(IPacketData data, Endpoint endpoint);
        void SendReplicationCreateStreamed(IPacketData data, Endpoint endpoint);
        void SendReplicationDestroy(IPacketData data, List<EndpointId> endpoints);
        void SendReplicationIslandDone(IPacketData data, Endpoint endpoint);
        void SendServerData(IPacketData data, Endpoint endpoint);
        void SendStateSync(IPacketData data, Endpoint endpoint, bool reliable);
        void SendVoxelCacheInvalidated(string storageName, EndpointId endpoint);
        void SendWorld(IPacketData data, EndpointId endpoint);
        void SendWorldData(IPacketData data, List<EndpointId> endpoints);
        void SentClientJoined(IPacketData data, EndpointId endpoint);
        void ValidationFailed(ulong clientId, bool kick = true, string additionalInfo = null, bool stackTrace = true);
        void WriteCustomState(BitStream stream);
    }
}

