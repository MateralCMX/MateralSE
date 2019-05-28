namespace Sandbox.Game.Replication.StateGroups
{
    using Sandbox.Game.Replication.History;
    using System;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.Network;

    public class MySimplePhysicsStateGroup : MyEntityPhysicsStateGroupBase
    {
        private readonly MyPredictedSnapshotSyncSetup m_settings;

        public MySimplePhysicsStateGroup(MyEntity entity, IMyReplicable owner, MyPredictedSnapshotSyncSetup settings) : base(entity, owner, true)
        {
            this.m_settings = settings;
        }

        public override void ClientUpdate(MyTimeSpan clientTimestamp)
        {
            base.m_snapshotSync.Update(clientTimestamp, this.m_settings);
        }
    }
}

