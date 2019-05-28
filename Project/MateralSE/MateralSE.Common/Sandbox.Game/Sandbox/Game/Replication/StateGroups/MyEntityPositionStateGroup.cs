namespace Sandbox.Game.Replication.StateGroups
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRageMath;

    internal class MyEntityPositionStateGroup : IMyStateGroup, IMyNetObject, IMyEventOwner
    {
        private Vector3D m_position;
        private readonly IMyEntity m_entity;

        public MyEntityPositionStateGroup(IMyReplicable ownerReplicable, IMyEntity entity)
        {
            this.Owner = ownerReplicable;
            this.m_entity = entity;
        }

        public void ClientUpdate(MyTimeSpan clientTimestamp)
        {
            if (!this.m_entity.PositionComp.GetPosition().Equals(this.m_position, 1.0))
            {
                this.m_entity.SetWorldMatrix(MatrixD.CreateTranslation(this.m_position), null);
            }
        }

        public void CreateClientData(MyClientStateBase forClient)
        {
        }

        public void Destroy()
        {
            this.Owner = null;
        }

        public void DestroyClientData(MyClientStateBase forClient)
        {
        }

        public void ForceSend(MyClientStateBase clientData)
        {
        }

        public float GetGroupPriority(int frameCountWithoutSync, MyClientInfo forClient) => 
            1f;

        public MyStreamProcessingState IsProcessingForClient(Endpoint forClient) => 
            MyStreamProcessingState.None;

        public bool IsStillDirty(Endpoint forClient) => 
            true;

        public void OnAck(MyClientStateBase forClient, byte packetId, bool delivered)
        {
        }

        public void Reset(bool reinit, MyTimeSpan clientTimestamp)
        {
        }

        public void Serialize(BitStream stream, Endpoint forClient, MyTimeSpan serverTimestamp, MyTimeSpan lastClientTimestamp, byte packetId, int maxBitPosition, HashSet<string> cachedData)
        {
            if (stream.Writing)
            {
                stream.Write(this.m_entity.PositionComp.GetPosition());
            }
            else
            {
                this.m_position = stream.ReadVector3D();
            }
        }

        public bool IsStreaming =>
            false;

        public bool NeedsUpdate =>
            true;

        public bool IsHighPriority =>
            false;

        public IMyReplicable Owner { get; private set; }

        public bool IsValid =>
            !this.m_entity.MarkedForClose;
    }
}

