namespace Sandbox.Game.Replication
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    internal class MyInventoryBaseReplicable : MyExternalReplicableEvent<MyInventoryBase>
    {
        private readonly Action<VRage.Game.Entity.MyEntity> m_destroyEntity;
        private long m_entityId;
        private MyStringHash m_inventoryId;

        public MyInventoryBaseReplicable()
        {
            this.m_destroyEntity = entity => this.RaiseDestroyed();
        }

        public override BoundingBoxD GetAABB() => 
            BoundingBoxD.CreateInvalid();

        public override IMyReplicable GetParent() => 
            (!(this.Inventory.Entity is MyCharacter) ? (!(this.Inventory.Entity is MyCubeBlock) ? null : FindByObject((this.Inventory.Entity as MyCubeBlock).CubeGrid)) : FindByObject(this.Inventory.Entity));

        public override void GetStateGroups(List<IMyStateGroup> resultList)
        {
        }

        private void LoadAsync(Action<MyInventoryBase> loadingDoneHandler)
        {
            VRage.Game.Entity.MyEntity entity;
            MyInventoryBase component = null;
            MyInventoryBase inventory = null;
            if ((Sandbox.Game.Entities.MyEntities.TryGetEntityById(this.m_entityId, out entity, false) && entity.Components.TryGet<MyInventoryBase>(out component)) && (component is MyInventoryAggregate))
            {
                inventory = (component as MyInventoryAggregate).GetInventory(this.m_inventoryId);
            }
            loadingDoneHandler(inventory ?? component);
        }

        private void OnBlockCubeGridChanged(MySlimBlock slimBlock, MyCubeGrid grid)
        {
            if (Sync.IsServer)
            {
                (MyMultiplayer.ReplicationLayer as MyReplicationLayer).RefreshReplicableHierarchy(this);
            }
        }

        public override void OnDestroyClient()
        {
            if ((this.Inventory != null) && (this.Inventory.Entity != null))
            {
                ((VRage.Game.Entity.MyEntity) this.Inventory.Entity).OnClose -= this.m_destroyEntity;
            }
        }

        protected override void OnHook()
        {
            base.OnHook();
            if (this.Inventory != null)
            {
                ((VRage.Game.Entity.MyEntity) this.Inventory.Entity).OnClose += this.m_destroyEntity;
                this.Inventory.BeforeRemovedFromContainer += component => this.OnRemovedFromContainer();
                MyCubeBlock entity = this.Inventory.Entity as MyCubeBlock;
                if (entity != null)
                {
                    entity.SlimBlock.CubeGridChanged += new Action<MySlimBlock, MyCubeGrid>(this.OnBlockCubeGridChanged);
                }
            }
        }

        protected override void OnLoad(BitStream stream, Action<MyInventoryBase> loadingDoneHandler)
        {
            if (stream != null)
            {
                MySerializer.CreateAndRead<long>(stream, out this.m_entityId, null);
                MySerializer.CreateAndRead<MyStringHash>(stream, out this.m_inventoryId, null);
            }
            Sandbox.Game.Entities.MyEntities.CallAsync(() => this.LoadAsync(loadingDoneHandler));
        }

        private void OnRemovedFromContainer()
        {
            if ((this.Inventory != null) && (this.Inventory.Entity != null))
            {
                ((VRage.Game.Entity.MyEntity) this.Inventory.Entity).OnClose -= this.m_destroyEntity;
                MyCubeBlock entity = this.Inventory.Entity as MyCubeBlock;
                if (entity != null)
                {
                    entity.SlimBlock.CubeGridChanged -= new Action<MySlimBlock, MyCubeGrid>(this.OnBlockCubeGridChanged);
                }
                this.RaiseDestroyed();
            }
        }

        public override bool OnSave(BitStream stream, Endpoint clientEndpoint)
        {
            long entityId = this.Inventory.Entity.EntityId;
            MySerializer.Write<long>(stream, ref entityId, null);
            MyStringHash inventoryId = this.Inventory.InventoryId;
            MySerializer.Write<MyStringHash>(stream, ref inventoryId, null);
            return true;
        }

        private MyInventoryBase Inventory =>
            base.Instance;

        public override bool IsValid =>
            ((this.Inventory != null) && ((this.Inventory.Entity != null) && !this.Inventory.Entity.MarkedForClose));

        public override bool HasToBeChild =>
            true;
    }
}

