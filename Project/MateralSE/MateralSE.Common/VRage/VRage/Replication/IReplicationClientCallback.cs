namespace VRage.Replication
{
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Library.Collections;
    using VRage.Library.Utils;

    public interface IReplicationClientCallback
    {
        void DisconnectFromHost();
        MyPacketDataBitStreamBase GetBitStreamPacketData();
        float GetClientSimulationRatio();
        float GetServerSimulationRatio();
        MyTimeSpan GetUpdateTime();
        void PauseClient(bool pause);
        void ReadCustomState(BitStream stream);
        void SendClientAcks(IPacketData data);
        void SendClientReady(MyPacketDataBitStreamBase data);
        void SendClientUpdate(IPacketData data);
        void SendConnectRequest(IPacketData data);
        void SendEvent(IPacketData data, bool reliable);
        void SendReplicableReady(IPacketData data);
        void SendReplicableRequest(IPacketData data);
        void SetIslandDone(byte index, Dictionary<long, MatrixD> matrices);
        void SetNextFrameDelayDelta(float delay);
        void SetPing(long duration);
        void UpdateSnapshotCache();
    }
}

