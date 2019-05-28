namespace VRage.Network
{
    using System;
    using VRage.Library;

    public class MyStateDataEntry : FastPriorityQueue<MyStateDataEntry>.Node
    {
        public readonly NetworkId GroupId;
        public readonly IMyStateGroup Group;
        public readonly IMyReplicable Owner;

        public MyStateDataEntry(IMyReplicable owner, NetworkId groupId, IMyStateGroup group)
        {
            this.Owner = owner;
            base.Priority = 0L;
            this.GroupId = groupId;
            this.Group = group;
        }

        public override string ToString() => 
            $"{base.Priority:0.00}, {this.GroupId}, {this.Group}";
    }
}

