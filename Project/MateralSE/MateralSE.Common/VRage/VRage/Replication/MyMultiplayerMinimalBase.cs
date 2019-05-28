namespace VRage.Replication
{
    using System;

    public abstract class MyMultiplayerMinimalBase
    {
        public static MyMultiplayerMinimalBase Instance;
        public readonly bool IsServer;

        protected MyMultiplayerMinimalBase()
        {
            this.IsServer = this.IsServerInternal;
        }

        protected abstract bool IsServerInternal { get; }
    }
}

