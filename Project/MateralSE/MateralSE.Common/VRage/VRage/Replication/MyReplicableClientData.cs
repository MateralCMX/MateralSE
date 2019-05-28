namespace VRage.Replication
{
    using System;

    public class MyReplicableClientData
    {
        public bool IsPending = true;
        public bool IsStreaming;

        public bool HasActiveStateSync =>
            !this.IsPending;
    }
}

