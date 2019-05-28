namespace Sandbox.Game.Replication
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRage.Serialization;
    using VRageMath;

    public abstract class MyComponentReplicableBase<T> : MyExternalReplicableEvent<T> where T: MyEntityComponentBase, IMyEventProxy
    {
        private readonly Action<MyEntity> m_raiseDestroyedHandler;

        protected MyComponentReplicableBase()
        {
            this.m_raiseDestroyedHandler = entity => this.RaiseDestroyed();
        }

        public override BoundingBoxD GetAABB() => 
            BoundingBoxD.CreateInvalid();

        public override void GetStateGroups(List<IMyStateGroup> resultList)
        {
        }

        private void LoadAsync(long entityId, Action<T> loadingDoneHandler)
        {
            Type componentType = MyComponentTypeFactory.GetComponentType(typeof(T));
            MyEntity entity = null;
            MyComponentBase component = null;
            if (MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                entity.Components.TryGet(componentType, out component);
            }
            loadingDoneHandler(component as T);
        }

        private void OnComponentRemovedFromContainer()
        {
            if ((base.Instance != null) && (base.Instance.Entity != null))
            {
                ((MyEntity) base.Instance.Entity).OnClose -= this.m_raiseDestroyedHandler;
                this.RaiseDestroyed();
            }
        }

        public override void OnDestroyClient()
        {
            if ((base.Instance != null) && (base.Instance.Entity != null))
            {
                ((MyEntity) base.Instance.Entity).OnClose -= this.m_raiseDestroyedHandler;
            }
        }

        protected override void OnHook()
        {
            base.OnHook();
            if (base.Instance != null)
            {
                ((MyEntity) base.Instance.Entity).OnClose += this.m_raiseDestroyedHandler;
                base.Instance.BeforeRemovedFromContainer += component => base.OnComponentRemovedFromContainer();
            }
        }

        protected override void OnLoad(BitStream stream, Action<T> loadingDoneHandler)
        {
            long entityId;
            MySerializer.CreateAndRead<long>(stream, out entityId, null);
            MyEntities.CallAsync(() => ((MyComponentReplicableBase<T>) this).LoadAsync(entityId, loadingDoneHandler));
        }

        public override bool OnSave(BitStream stream, Endpoint clientEndpoint)
        {
            long entityId = base.Instance.Entity.EntityId;
            MySerializer.Write<long>(stream, ref entityId, null);
            return true;
        }

        public override bool HasToBeChild =>
            true;
    }
}

