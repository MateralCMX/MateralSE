namespace VRage.Game.Components
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.ModAPI;
    using VRage.Network;

    public abstract class MyEntityComponentBase : MyComponentBase
    {
        [CompilerGenerated]
        private static Action<MyEntityComponentBase> OnAfterAddedToContainer;
        [CompilerGenerated]
        private Action<MyEntityComponentBase> BeforeRemovedFromContainer;

        public event Action<MyEntityComponentBase> BeforeRemovedFromContainer
        {
            [CompilerGenerated] add
            {
                Action<MyEntityComponentBase> beforeRemovedFromContainer = this.BeforeRemovedFromContainer;
                while (true)
                {
                    Action<MyEntityComponentBase> a = beforeRemovedFromContainer;
                    Action<MyEntityComponentBase> action3 = (Action<MyEntityComponentBase>) Delegate.Combine(a, value);
                    beforeRemovedFromContainer = Interlocked.CompareExchange<Action<MyEntityComponentBase>>(ref this.BeforeRemovedFromContainer, action3, a);
                    if (ReferenceEquals(beforeRemovedFromContainer, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntityComponentBase> beforeRemovedFromContainer = this.BeforeRemovedFromContainer;
                while (true)
                {
                    Action<MyEntityComponentBase> source = beforeRemovedFromContainer;
                    Action<MyEntityComponentBase> action3 = (Action<MyEntityComponentBase>) Delegate.Remove(source, value);
                    beforeRemovedFromContainer = Interlocked.CompareExchange<Action<MyEntityComponentBase>>(ref this.BeforeRemovedFromContainer, action3, source);
                    if (ReferenceEquals(beforeRemovedFromContainer, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyEntityComponentBase> OnAfterAddedToContainer
        {
            [CompilerGenerated] add
            {
                Action<MyEntityComponentBase> onAfterAddedToContainer = OnAfterAddedToContainer;
                while (true)
                {
                    Action<MyEntityComponentBase> a = onAfterAddedToContainer;
                    Action<MyEntityComponentBase> action3 = (Action<MyEntityComponentBase>) Delegate.Combine(a, value);
                    onAfterAddedToContainer = Interlocked.CompareExchange<Action<MyEntityComponentBase>>(ref OnAfterAddedToContainer, action3, a);
                    if (ReferenceEquals(onAfterAddedToContainer, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntityComponentBase> onAfterAddedToContainer = OnAfterAddedToContainer;
                while (true)
                {
                    Action<MyEntityComponentBase> source = onAfterAddedToContainer;
                    Action<MyEntityComponentBase> action3 = (Action<MyEntityComponentBase>) Delegate.Remove(source, value);
                    onAfterAddedToContainer = Interlocked.CompareExchange<Action<MyEntityComponentBase>>(ref OnAfterAddedToContainer, action3, source);
                    if (ReferenceEquals(onAfterAddedToContainer, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MyEntityComponentBase()
        {
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            Action<MyEntityComponentBase> onAfterAddedToContainer = OnAfterAddedToContainer;
            if (onAfterAddedToContainer != null)
            {
                onAfterAddedToContainer(this);
            }
            if (this.Entity != null)
            {
                IMySyncedEntity entity = this.Entity as IMySyncedEntity;
                if (((entity != null) && (entity.SyncType != null)) && this.AttachSyncToEntity)
                {
                    entity.SyncType.Append(this);
                }
            }
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            Action<MyEntityComponentBase> beforeRemovedFromContainer = this.BeforeRemovedFromContainer;
            if (beforeRemovedFromContainer != null)
            {
                beforeRemovedFromContainer(this);
            }
        }

        public MyEntityComponentContainer Container =>
            (base.ContainerBase as MyEntityComponentContainer);

        public IMyEntity Entity
        {
            get
            {
                MyEntityComponentContainer containerBase = base.ContainerBase as MyEntityComponentContainer;
                return containerBase?.Entity;
            }
        }

        public abstract string ComponentTypeDebugString { get; }

        public virtual bool AttachSyncToEntity =>
            true;
    }
}

