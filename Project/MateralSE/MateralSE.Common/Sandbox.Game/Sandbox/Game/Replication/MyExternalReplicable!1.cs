namespace Sandbox.Game.Replication
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Collections;

    public abstract class MyExternalReplicable<T> : MyExternalReplicable
    {
        protected MyExternalReplicable()
        {
        }

        public sealed override bool CheckConsistency() => 
            (this.Instance != null);

        protected sealed override object GetInstance() => 
            this.Instance;

        public sealed override void Hook(object obj)
        {
            this.HookInternal(obj);
        }

        private void HookInternal(object obj)
        {
            this.Instance = (T) obj;
            base.Hook(obj);
            this.OnHook();
        }

        protected abstract void OnLoad(BitStream stream, Action<T> loadingDoneHandler);
        public sealed override void OnLoad(BitStream stream, Action<bool> loadingDoneHandler)
        {
            this.OnLoad(stream, (Action<T>) (instance => ((MyExternalReplicable<T>) this).OnLoadDone(instance, loadingDoneHandler)));
        }

        protected void OnLoadDone(T instance, Action<bool> loadingDoneHandler)
        {
            if (instance == null)
            {
                loadingDoneHandler(false);
            }
            else
            {
                this.HookInternal(instance);
                loadingDoneHandler(true);
            }
        }

        public sealed override void OnRemovedFromReplication()
        {
            T local = default(T);
            this.Instance = local;
        }

        protected override void RaiseDestroyed()
        {
            base.RaiseDestroyed();
        }

        public sealed override void Reload(Action<bool> loadingDoneHandler)
        {
            this.OnLoad(null, (Action<T>) (instance => ((MyExternalReplicable<T>) this).OnLoadDone(instance, loadingDoneHandler)));
        }

        public T Instance { get; private set; }

        public override string InstanceName
        {
            get
            {
                if (this.Instance == null)
                {
                    return "";
                }
                return this.Instance.ToString();
            }
        }

        public override bool IsReadyForReplication
        {
            get
            {
                MyEntity instance = this.Instance as MyEntity;
                MyEntityComponentBase base2 = this.Instance as MyEntityComponentBase;
                return ((instance == null) ? ((base2 == null) ? base.IsReadyForReplication : ((MyEntity) base2.Entity).IsReadyForReplication) : instance.IsReadyForReplication);
            }
        }

        public override Dictionary<IMyReplicable, Action> ReadyForReplicationAction
        {
            get
            {
                MyEntity instance = this.Instance as MyEntity;
                MyEntityComponentBase base2 = this.Instance as MyEntityComponentBase;
                return ((instance == null) ? ((base2 == null) ? base.ReadyForReplicationAction : ((((MyEntity) base2.Entity) == null) ? base.ReadyForReplicationAction : ((MyEntity) base2.Entity).ReadyForReplicationAction)) : instance.ReadyForReplicationAction);
            }
        }
    }
}

