namespace VRage.Network
{
    using System;
    using System.Collections.Generic;
    using VRage.Library.Collections;
    using VRage.Library.Utils;

    public interface IMyStateGroup : IMyNetObject, IMyEventOwner
    {
        void ClientUpdate(MyTimeSpan clientTimestamp);
        void CreateClientData(MyClientStateBase forClient);
        void Destroy();
        void DestroyClientData(MyClientStateBase forClient);
        void ForceSend(MyClientStateBase clientData);
        MyStreamProcessingState IsProcessingForClient(Endpoint forClient);
        bool IsStillDirty(Endpoint forClient);
        void OnAck(MyClientStateBase forClient, byte packetId, bool delivered);
        void Reset(bool reinit, MyTimeSpan clientTimestamp);
        void Serialize(BitStream stream, Endpoint forClient, MyTimeSpan serverTimestamp, MyTimeSpan lastClientTimestamp, byte packetId, int maxBitPosition, HashSet<string> cachedData);

        bool IsStreaming { get; }

        bool NeedsUpdate { get; }

        bool IsHighPriority { get; }

        IMyReplicable Owner { get; }
    }
}

