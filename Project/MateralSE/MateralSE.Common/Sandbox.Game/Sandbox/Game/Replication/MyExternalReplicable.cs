namespace Sandbox.Game.Replication
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Replication;
    using VRageMath;

    public abstract class MyExternalReplicable : IMyReplicable, IMyNetObject, IMyEventOwner
    {
        protected IMyStateGroup m_physicsSync;
        protected IMyReplicable m_parent;
        private static readonly MyConcurrentDictionary<object, MyExternalReplicable> m_objectExternalReplicables = new MyConcurrentDictionary<object, MyExternalReplicable>(0, null);
        [CompilerGenerated]
        private static Action<MyExternalReplicable> Destroyed;

        public static  event Action<MyExternalReplicable> Destroyed
        {
            [CompilerGenerated] add
            {
                Action<MyExternalReplicable> destroyed = Destroyed;
                while (true)
                {
                    Action<MyExternalReplicable> a = destroyed;
                    Action<MyExternalReplicable> action3 = (Action<MyExternalReplicable>) Delegate.Combine(a, value);
                    destroyed = Interlocked.CompareExchange<Action<MyExternalReplicable>>(ref Destroyed, action3, a);
                    if (ReferenceEquals(destroyed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyExternalReplicable> destroyed = Destroyed;
                while (true)
                {
                    Action<MyExternalReplicable> source = destroyed;
                    Action<MyExternalReplicable> action3 = (Action<MyExternalReplicable>) Delegate.Remove(source, value);
                    destroyed = Interlocked.CompareExchange<Action<MyExternalReplicable>>(ref Destroyed, action3, source);
                    if (ReferenceEquals(destroyed, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MyExternalReplicable()
        {
        }

        public abstract bool CheckConsistency();
        public static MyExternalReplicable FindByObject(object obj) => 
            m_objectExternalReplicables.GetValueOrDefault(obj, null);

        public abstract BoundingBoxD GetAABB();
        public virtual HashSet<IMyReplicable> GetDependencies(bool forPlayer) => 
            null;

        protected abstract object GetInstance();
        public abstract IMyReplicable GetParent();
        public virtual HashSet<IMyReplicable> GetPhysicalDependencies(MyTimeSpan timeStamp, MyReplicablesBase replicables) => 
            null;

        public abstract void GetStateGroups(List<IMyStateGroup> resultList);
        public virtual ValidationResult HasRights(EndpointId endpointId, ValidationType validationFlags) => 
            ValidationResult.Passed;

        public virtual void Hook(object obj)
        {
            m_objectExternalReplicables[obj] = this;
        }

        public abstract void OnDestroyClient();
        protected abstract void OnHook();
        public abstract void OnLoad(BitStream stream, Action<bool> loadingDoneHandler);
        public abstract void OnRemovedFromReplication();
        public abstract bool OnSave(BitStream stream, Endpoint clientEndpoint);
        public virtual void OnServerReplicate()
        {
        }

        protected virtual void RaiseDestroyed()
        {
            Dictionary<IMyReplicable, Action> readyForReplicationAction = this.ReadyForReplicationAction;
            if (readyForReplicationAction != null)
            {
                readyForReplicationAction.Remove(this);
            }
            object instance = this.GetInstance();
            if (instance != null)
            {
                m_objectExternalReplicables.Remove(instance);
            }
            Action<MyExternalReplicable> destroyed = Destroyed;
            if (destroyed != null)
            {
                destroyed(this);
            }
        }

        public abstract void Reload(Action<bool> loadingDoneHandler);
        public virtual bool ShouldReplicate(MyClientInfo client) => 
            true;

        public IMyStateGroup PhysicsSync =>
            this.m_physicsSync;

        public virtual string InstanceName =>
            "";

        public virtual bool IsReadyForReplication =>
            (!this.HasToBeChild || ((this.GetParent() != null) && this.GetParent().IsReadyForReplication));

        public virtual Dictionary<IMyReplicable, Action> ReadyForReplicationAction
        {
            get
            {
                IMyReplicable parent = this.GetParent();
                return parent?.ReadyForReplicationAction;
            }
        }

        public virtual bool PriorityUpdate =>
            true;

        public virtual bool IncludeInIslands =>
            true;

        public abstract bool IsValid { get; }

        public abstract bool HasToBeChild { get; }

        public virtual bool IsSpatial =>
            !this.HasToBeChild;

        public Action<IMyReplicable> OnAABBChanged { get; set; }
    }
}

