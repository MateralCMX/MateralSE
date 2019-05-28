namespace Sandbox.Game.Replication.History
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library.Utils;

    public interface IMySnapshotSync
    {
        void Destroy();
        void Read(ref MySnapshot item, MyTimeSpan timeStamp);
        void Reset(bool reinit = false);
        long Update(MyTimeSpan clientTimestamp, MySnapshotSyncSetup setup);
    }
}

