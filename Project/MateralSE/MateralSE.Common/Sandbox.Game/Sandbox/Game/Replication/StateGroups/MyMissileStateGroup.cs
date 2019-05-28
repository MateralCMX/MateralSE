namespace Sandbox.Game.Replication.StateGroups
{
    using Sandbox.Game.Replication.History;
    using System;
    using VRage.Game.Entity;
    using VRage.Network;

    public class MyMissileStateGroup : MySimplePhysicsStateGroup
    {
        public MyMissileStateGroup(MyEntity entity, IMyReplicable owner, MyPredictedSnapshotSyncSetup settings) : base(entity, owner, settings)
        {
        }

        public override bool IsStillDirty(Endpoint forClient) => 
            true;
    }
}

