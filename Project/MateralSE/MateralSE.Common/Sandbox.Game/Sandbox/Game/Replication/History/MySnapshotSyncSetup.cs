namespace Sandbox.Game.Replication.History
{
    using System;
    using VRage.Game.Networking;

    public class MySnapshotSyncSetup : MySnapshotFlags
    {
        public string ProfileName;
        public bool ExtrapolationSmoothing;
        public bool IgnoreParentId;
        public bool UserTrend;
    }
}

