namespace Sandbox.Game.Replication
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.StateGroups;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Replication;
    using VRage.Serialization;
    using VRageMath;

    public abstract class MyEntityReplicableBase<T> : MyExternalReplicable<T>, IMyEntityReplicable where T: MyEntity
    {
        private Action<MyEntity> m_onCloseAction;
        private readonly List<IMyReplicable> m_tmpReplicables;
        private readonly HashSet<IMyReplicable> m_physicalDependencies;
        private MyTimeSpan m_lastPhysicalDependencyUpdate;
        private bool m_destroyed;
        protected const double MIN_DITHER_DISTANCE_SQR = 1000000.0;

        protected MyEntityReplicableBase()
        {
            this.m_tmpReplicables = new List<IMyReplicable>();
            this.m_physicalDependencies = new HashSet<IMyReplicable>();
        }

        protected virtual IMyStateGroup CreatePhysicsGroup() => 
            new MyEntityPhysicsStateGroup(base.Instance, this);

        public override BoundingBoxD GetAABB() => 
            ((base.Instance != null) ? base.Instance.PositionComp.WorldAABB : BoundingBoxD.CreateInvalid());

        public override IMyReplicable GetParent() => 
            null;

        public override HashSet<IMyReplicable> GetPhysicalDependencies(MyTimeSpan timeStamp, MyReplicablesBase replicables)
        {
            if (((this.m_lastPhysicalDependencyUpdate != timeStamp) && this.IncludeInIslands) && this.CheckConsistency())
            {
                this.m_lastPhysicalDependencyUpdate = timeStamp;
                this.m_physicalDependencies.Clear();
                bool flag = true;
                BoundingBoxD aABB = this.GetAABB();
                while (flag)
                {
                    flag = false;
                    this.m_tmpReplicables.Clear();
                    this.m_physicalDependencies.Add(this);
                    replicables.GetReplicablesInBox(aABB.GetInflated((double) 2.5), this.m_tmpReplicables);
                    foreach (IMyReplicable replicable in this.m_tmpReplicables)
                    {
                        if (!replicable.CheckConsistency())
                        {
                            continue;
                        }
                        if (!this.m_physicalDependencies.Contains(replicable) && replicable.IncludeInIslands)
                        {
                            this.m_physicalDependencies.Add(replicable);
                            aABB.Include(replicable.GetAABB());
                            flag = true;
                        }
                    }
                }
            }
            return this.m_physicalDependencies;
        }

        public override void GetStateGroups(List<IMyStateGroup> resultList)
        {
            if (base.m_physicsSync != null)
            {
                resultList.Add(base.m_physicsSync);
            }
        }

        private void OnClose(MyEntity ent)
        {
            this.RaiseDestroyed();
        }

        public override void OnDestroyClient()
        {
            if (base.Instance != null)
            {
                if ((base.Instance.PositionComp.GetPosition() - MySector.MainCamera.Position).LengthSquared() > 1000000.0)
                {
                    base.Instance.Render.FadeOut = true;
                }
                base.Instance.Close();
            }
            base.m_physicsSync = null;
            this.m_destroyed = true;
        }

        protected override void OnHook()
        {
            base.m_physicsSync = this.CreatePhysicsGroup();
            this.m_onCloseAction = new Action<MyEntity>(this.OnClose);
            base.Instance.OnClose += this.m_onCloseAction;
            if (Sync.IsServer)
            {
                base.Instance.PositionComp.OnPositionChanged += new Action<MyPositionComponentBase>(this.PositionComp_OnPositionChanged);
            }
        }

        protected override void OnLoad(BitStream stream, Action<T> loadingDoneHandler)
        {
            MyObjectBuilder_EntityBase objectBuilder = MySerializer.CreateAndRead<MyObjectBuilder_EntityBase>(stream, MyObjectBuilderSerializer.Dynamic);
            this.TryRemoveExistingEntity(objectBuilder.EntityId);
            T entity = (T) MyEntities.CreateFromObjectBuilder(objectBuilder, false);
            if (entity != null)
            {
                MyEntities.Add(entity, true);
            }
            loadingDoneHandler(entity);
        }

        public override bool OnSave(BitStream stream, Endpoint clientEndpoint)
        {
            MyObjectBuilder_EntityBase objectBuilder = base.Instance.GetObjectBuilder(false);
            MySerializer.Write<MyObjectBuilder_EntityBase>(stream, ref objectBuilder, MyObjectBuilderSerializer.Dynamic);
            return true;
        }

        private void PositionComp_OnPositionChanged(MyPositionComponentBase obj)
        {
            if (base.OnAABBChanged != null)
            {
                base.OnAABBChanged(this);
            }
        }

        protected void TryRemoveExistingEntity(long entityId)
        {
            MyEntity entity;
            if (MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                entity.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                if (!entity.MarkedForClose)
                {
                    entity.Close();
                }
            }
        }

        public override bool IsValid =>
            ((base.Instance != null) && !base.Instance.MarkedForClose);

        public MatrixD WorldMatrix =>
            ((base.Instance == null) ? MatrixD.Identity : base.Instance.WorldMatrix);

        public long EntityId =>
            ((base.Instance == null) ? 0L : base.Instance.EntityId);

        public override bool HasToBeChild =>
            false;
    }
}

