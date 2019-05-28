namespace Sandbox.Game.Replication
{
    using Sandbox.Game.Replication.StateGroups;
    using System;
    using VRage.Network;

    internal class MySafeZoneReplicable : MyEntityReplicableBaseEvent<MySafeZone>
    {
        protected override IMyStateGroup CreatePhysicsGroup() => 
            new MyEntityPositionStateGroup(this, base.Instance);

        public override void OnDestroyClient()
        {
            if ((base.Instance != null) && base.Instance.Save)
            {
                base.Instance.Close();
            }
        }
    }
}

