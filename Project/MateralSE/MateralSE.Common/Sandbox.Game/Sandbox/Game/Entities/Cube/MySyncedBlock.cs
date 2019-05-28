namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Network;
    using VRage.Sync;

    public class MySyncedBlock : MyCubeBlock, IMyEventProxy, IMyEventOwner, IMySyncedEntity
    {
        public event Action<SyncBase> SyncPropertyChanged
        {
            add
            {
                this.SyncType.PropertyChanged += value;
            }
            remove
            {
                this.SyncType.PropertyChanged -= value;
            }
        }

        public MySyncedBlock()
        {
            this.SyncType = SyncHelpers.Compose(this, 0);
        }

        public VRage.Sync.SyncType SyncType { get; set; }
    }
}

