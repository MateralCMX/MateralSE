namespace Sandbox.Game.Replication
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.StateGroups;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRage.Serialization;
    using VRageMath;

    internal class MyInventoryReplicable : MyExternalReplicableEvent<MyInventory>
    {
        private MyPropertySyncStateGroup m_propertySync;
        private MyEntityInventoryStateGroup m_stateGroup;
        private long m_entityId;
        private int m_inventoryId;

        public override BoundingBoxD GetAABB() => 
            BoundingBoxD.CreateInvalid();

        public override IMyReplicable GetParent()
        {
            if (base.m_parent == null)
            {
                base.m_parent = FindByObject(base.Instance.Owner);
            }
            return base.m_parent;
        }

        public override void GetStateGroups(List<IMyStateGroup> resultList)
        {
            if (this.m_stateGroup != null)
            {
                resultList.Add(this.m_stateGroup);
            }
            resultList.Add(this.m_propertySync);
        }

        public override ValidationResult HasRights(EndpointId endpointId, ValidationType validationFlags)
        {
            MyExternalReplicable replicable = FindByObject(base.Instance.Owner);
            return ((replicable == null) ? base.HasRights(endpointId, validationFlags) : replicable.HasRights(endpointId, validationFlags));
        }

        private void LoadAsync(Action<MyInventory> loadingDoneHandler)
        {
            VRage.Game.Entity.MyEntity entity;
            VRage.Game.Entity.MyEntity entity1;
            Sandbox.Game.Entities.MyEntities.TryGetEntityById(this.m_entityId, out entity, false);
            MyInventory inventory = null;
            if ((entity == null) || !entity.HasInventory)
            {
                entity1 = null;
            }
            else
            {
                entity1 = entity;
            }
            VRage.Game.Entity.MyEntity thisEntity = entity1;
            if ((thisEntity != null) && !thisEntity.GetTopMostParent(null).MarkedForClose)
            {
                inventory = thisEntity.GetInventory(this.m_inventoryId);
            }
            loadingDoneHandler(inventory);
        }

        private void OnBlockCubeGridChanged(MySlimBlock slimBlock, MyCubeGrid grid)
        {
            base.m_parent = FindByObject((base.Instance.Owner as MyCubeBlock).CubeGrid);
            (MyMultiplayer.ReplicationLayer as MyReplicationLayer).RefreshReplicableHierarchy(this);
        }

        public override void OnDestroyClient()
        {
        }

        protected override void OnHook()
        {
            base.OnHook();
            if (base.Instance != null)
            {
                this.m_stateGroup = new MyEntityInventoryStateGroup(base.Instance, Sync.IsServer, this);
                base.Instance.BeforeRemovedFromContainer += component => this.OnRemovedFromContainer();
                this.m_propertySync = new MyPropertySyncStateGroup(this, base.Instance.SyncType);
                MyCubeBlock owner = base.Instance.Owner as MyCubeBlock;
                if (owner != null)
                {
                    owner.SlimBlock.CubeGridChanged += new Action<MySlimBlock, MyCubeGrid>(this.OnBlockCubeGridChanged);
                    base.m_parent = FindByObject(owner.CubeGrid);
                }
                else
                {
                    base.m_parent = FindByObject(base.Instance.Owner);
                }
            }
        }

        protected override void OnLoad(BitStream stream, Action<MyInventory> loadingDoneHandler)
        {
            if (stream != null)
            {
                MySerializer.CreateAndRead<long>(stream, out this.m_entityId, null);
                MySerializer.CreateAndRead<int>(stream, out this.m_inventoryId, null);
            }
            Sandbox.Game.Entities.MyEntities.CallAsync(() => this.LoadAsync(loadingDoneHandler));
        }

        private void OnRemovedFromContainer()
        {
            if ((base.Instance != null) && (base.Instance.Owner != null))
            {
                MyCubeBlock owner = base.Instance.Owner as MyCubeBlock;
                if (owner != null)
                {
                    owner.SlimBlock.CubeGridChanged -= new Action<MySlimBlock, MyCubeGrid>(this.OnBlockCubeGridChanged);
                }
                this.RaiseDestroyed();
            }
        }

        public override bool OnSave(BitStream stream, Endpoint clientEndpoint)
        {
            long entityId = base.Instance.Owner.EntityId;
            MySerializer.Write<long>(stream, ref entityId, null);
            int num2 = 0;
            int index = 0;
            while (true)
            {
                if (index < base.Instance.Owner.InventoryCount)
                {
                    if (base.Instance != base.Instance.Owner.GetInventory(index))
                    {
                        index++;
                        continue;
                    }
                    num2 = index;
                }
                MySerializer.Write<int>(stream, ref num2, null);
                return true;
            }
        }

        public override string ToString()
        {
            string text1;
            string text2;
            if (base.Instance == null)
            {
                text1 = "<inventory null>";
            }
            else if (base.Instance.Owner == null)
            {
                text1 = "<owner null>";
            }
            else
            {
                text1 = base.Instance.Owner.EntityId.ToString();
            }
            string str = text2;
            return string.Format("MyInventoryReplicable, Owner id: " + str, Array.Empty<object>());
        }

        public override bool IsValid =>
            ((base.Instance != null) && ((base.Instance.Entity != null) && !base.Instance.Entity.MarkedForClose));

        public override bool HasToBeChild =>
            true;
    }
}

