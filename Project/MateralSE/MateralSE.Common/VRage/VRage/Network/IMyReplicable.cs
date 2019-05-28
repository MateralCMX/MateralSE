namespace VRage.Network
{
    using System;
    using System.Collections.Generic;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Replication;
    using VRageMath;

    public interface IMyReplicable : IMyNetObject, IMyEventOwner
    {
        bool CheckConsistency();
        BoundingBoxD GetAABB();
        HashSet<IMyReplicable> GetDependencies(bool forPlayer);
        IMyReplicable GetParent();
        HashSet<IMyReplicable> GetPhysicalDependencies(MyTimeSpan timeStamp, MyReplicablesBase replicables);
        void GetStateGroups(List<IMyStateGroup> resultList);
        ValidationResult HasRights(EndpointId client, ValidationType validationFlags);
        void OnDestroyClient();
        void OnLoad(BitStream stream, Action<bool> loadingDoneHandler);
        void OnRemovedFromReplication();
        bool OnSave(BitStream stream, Endpoint clientEndpoint);
        void Reload(Action<bool> loadingDoneHandler);
        bool ShouldReplicate(MyClientInfo client);

        bool HasToBeChild { get; }

        bool IsSpatial { get; }

        bool PriorityUpdate { get; }

        bool IncludeInIslands { get; }

        string InstanceName { get; }

        bool IsReadyForReplication { get; }

        Dictionary<IMyReplicable, Action> ReadyForReplicationAction { get; }

        Action<IMyReplicable> OnAABBChanged { get; set; }
    }
}

