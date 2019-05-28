namespace Sandbox.Game.Components
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyComponentType(typeof(MyCraftingComponentBase))]
    public abstract class MyCraftingComponentBase : MyGameLogicComponent, IMyEventProxy, IMyEventOwner
    {
        private List<MyBlueprintToProduce> m_itemsToProduce = new List<MyBlueprintToProduce>();
        protected List<MyBlueprintClassDefinition> m_blueprintClasses = new List<MyBlueprintClassDefinition>();
        [CompilerGenerated]
        private Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint> BlueprintProduced;
        [CompilerGenerated]
        private Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item> MissingRequiredItem;
        [CompilerGenerated]
        private Action<MyCraftingComponentBase> InventoryIsFull;
        [CompilerGenerated]
        private Action<MyCraftingComponentBase, MyBlueprintToProduce> ProductionChanged;
        [CompilerGenerated]
        private Action<MyCraftingComponentBase> OperatingChanged;
        [CompilerGenerated]
        private Action LockAcquired;
        [CompilerGenerated]
        private Action LockReleased;
        protected int m_currentItem = -1;
        protected float m_currentItemStatus;
        protected float m_lastItemStatus;
        protected MyFixedPoint m_currentProductionAmount = 1;
        protected int m_elapsedTimeMs;
        protected float m_craftingSpeedMultiplier = 1f;
        private long m_lockedByEntityId = -1L;

        public event Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint> BlueprintProduced
        {
            [CompilerGenerated] add
            {
                Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint> blueprintProduced = this.BlueprintProduced;
                while (true)
                {
                    Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint> a = blueprintProduced;
                    Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint> action3 = (Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint>) Delegate.Combine(a, value);
                    blueprintProduced = Interlocked.CompareExchange<Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint>>(ref this.BlueprintProduced, action3, a);
                    if (ReferenceEquals(blueprintProduced, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint> blueprintProduced = this.BlueprintProduced;
                while (true)
                {
                    Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint> source = blueprintProduced;
                    Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint> action3 = (Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint>) Delegate.Remove(source, value);
                    blueprintProduced = Interlocked.CompareExchange<Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint>>(ref this.BlueprintProduced, action3, source);
                    if (ReferenceEquals(blueprintProduced, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCraftingComponentBase> InventoryIsFull
        {
            [CompilerGenerated] add
            {
                Action<MyCraftingComponentBase> inventoryIsFull = this.InventoryIsFull;
                while (true)
                {
                    Action<MyCraftingComponentBase> a = inventoryIsFull;
                    Action<MyCraftingComponentBase> action3 = (Action<MyCraftingComponentBase>) Delegate.Combine(a, value);
                    inventoryIsFull = Interlocked.CompareExchange<Action<MyCraftingComponentBase>>(ref this.InventoryIsFull, action3, a);
                    if (ReferenceEquals(inventoryIsFull, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCraftingComponentBase> inventoryIsFull = this.InventoryIsFull;
                while (true)
                {
                    Action<MyCraftingComponentBase> source = inventoryIsFull;
                    Action<MyCraftingComponentBase> action3 = (Action<MyCraftingComponentBase>) Delegate.Remove(source, value);
                    inventoryIsFull = Interlocked.CompareExchange<Action<MyCraftingComponentBase>>(ref this.InventoryIsFull, action3, source);
                    if (ReferenceEquals(inventoryIsFull, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action LockAcquired
        {
            [CompilerGenerated] add
            {
                Action lockAcquired = this.LockAcquired;
                while (true)
                {
                    Action a = lockAcquired;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    lockAcquired = Interlocked.CompareExchange<Action>(ref this.LockAcquired, action3, a);
                    if (ReferenceEquals(lockAcquired, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action lockAcquired = this.LockAcquired;
                while (true)
                {
                    Action source = lockAcquired;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    lockAcquired = Interlocked.CompareExchange<Action>(ref this.LockAcquired, action3, source);
                    if (ReferenceEquals(lockAcquired, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action LockReleased
        {
            [CompilerGenerated] add
            {
                Action lockReleased = this.LockReleased;
                while (true)
                {
                    Action a = lockReleased;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    lockReleased = Interlocked.CompareExchange<Action>(ref this.LockReleased, action3, a);
                    if (ReferenceEquals(lockReleased, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action lockReleased = this.LockReleased;
                while (true)
                {
                    Action source = lockReleased;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    lockReleased = Interlocked.CompareExchange<Action>(ref this.LockReleased, action3, source);
                    if (ReferenceEquals(lockReleased, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item> MissingRequiredItem
        {
            [CompilerGenerated] add
            {
                Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item> missingRequiredItem = this.MissingRequiredItem;
                while (true)
                {
                    Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item> a = missingRequiredItem;
                    Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item> action3 = (Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item>) Delegate.Combine(a, value);
                    missingRequiredItem = Interlocked.CompareExchange<Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item>>(ref this.MissingRequiredItem, action3, a);
                    if (ReferenceEquals(missingRequiredItem, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item> missingRequiredItem = this.MissingRequiredItem;
                while (true)
                {
                    Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item> source = missingRequiredItem;
                    Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item> action3 = (Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item>) Delegate.Remove(source, value);
                    missingRequiredItem = Interlocked.CompareExchange<Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item>>(ref this.MissingRequiredItem, action3, source);
                    if (ReferenceEquals(missingRequiredItem, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCraftingComponentBase> OperatingChanged
        {
            [CompilerGenerated] add
            {
                Action<MyCraftingComponentBase> operatingChanged = this.OperatingChanged;
                while (true)
                {
                    Action<MyCraftingComponentBase> a = operatingChanged;
                    Action<MyCraftingComponentBase> action3 = (Action<MyCraftingComponentBase>) Delegate.Combine(a, value);
                    operatingChanged = Interlocked.CompareExchange<Action<MyCraftingComponentBase>>(ref this.OperatingChanged, action3, a);
                    if (ReferenceEquals(operatingChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCraftingComponentBase> operatingChanged = this.OperatingChanged;
                while (true)
                {
                    Action<MyCraftingComponentBase> source = operatingChanged;
                    Action<MyCraftingComponentBase> action3 = (Action<MyCraftingComponentBase>) Delegate.Remove(source, value);
                    operatingChanged = Interlocked.CompareExchange<Action<MyCraftingComponentBase>>(ref this.OperatingChanged, action3, source);
                    if (ReferenceEquals(operatingChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCraftingComponentBase, MyBlueprintToProduce> ProductionChanged
        {
            [CompilerGenerated] add
            {
                Action<MyCraftingComponentBase, MyBlueprintToProduce> productionChanged = this.ProductionChanged;
                while (true)
                {
                    Action<MyCraftingComponentBase, MyBlueprintToProduce> a = productionChanged;
                    Action<MyCraftingComponentBase, MyBlueprintToProduce> action3 = (Action<MyCraftingComponentBase, MyBlueprintToProduce>) Delegate.Combine(a, value);
                    productionChanged = Interlocked.CompareExchange<Action<MyCraftingComponentBase, MyBlueprintToProduce>>(ref this.ProductionChanged, action3, a);
                    if (ReferenceEquals(productionChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCraftingComponentBase, MyBlueprintToProduce> productionChanged = this.ProductionChanged;
                while (true)
                {
                    Action<MyCraftingComponentBase, MyBlueprintToProduce> source = productionChanged;
                    Action<MyCraftingComponentBase, MyBlueprintToProduce> action3 = (Action<MyCraftingComponentBase, MyBlueprintToProduce>) Delegate.Remove(source, value);
                    productionChanged = Interlocked.CompareExchange<Action<MyCraftingComponentBase, MyBlueprintToProduce>>(ref this.ProductionChanged, action3, source);
                    if (ReferenceEquals(productionChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MyCraftingComponentBase()
        {
        }

        [Event(null, 0x462), Reliable, Server]
        private void AcquireLock_Event(long entityId)
        {
            MyEntity entity = null;
            if (!this.IsLocked && MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase, long>(this, x => new Action<long>(x.AcquireLock_Implementation), entityId, targetEndpoint);
            }
        }

        [Event(null, 0x46c), Reliable, Server, Broadcast]
        private void AcquireLock_Implementation(long entityId)
        {
            MyEntity entity = null;
            if (MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                entity.OnClose += new Action<MyEntity>(this.lockEntity_OnClose);
            }
            this.m_lockedByEntityId = entityId;
            if (this.LockAcquired != null)
            {
                this.LockAcquired();
            }
        }

        public void AcquireLockRequest(long entityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, long>(this, x => new Action<long>(x.AcquireLock_Event), entityId, targetEndpoint);
        }

        public void AddItemToProduce(MyFixedPoint amount, MyBlueprintDefinitionBase blueprint, long senderEntityId)
        {
            SerializableDefinitionId id = (SerializableDefinitionId) blueprint.Id;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyFixedPoint, SerializableDefinitionId, long>(this, x => new Action<MyFixedPoint, SerializableDefinitionId, long>(x.AddItemToProduce_Request), amount, id, senderEntityId, targetEndpoint);
        }

        [Event(null, 0x1a2), Reliable, Server, Broadcast]
        private void AddItemToProduce_Implementation(MyFixedPoint amount, SerializableDefinitionId blueprintId)
        {
            MyBlueprintDefinitionBase blueprint = MyDefinitionManager.Static.GetBlueprintDefinition(blueprintId);
            if (blueprint != null)
            {
                MyBlueprintToProduce item = this.m_itemsToProduce.Find(x => ReferenceEquals(x.Blueprint, blueprint));
                if (item != null)
                {
                    item.Amount += amount;
                }
                else
                {
                    item = new MyBlueprintToProduce(amount, blueprint);
                    this.m_itemsToProduce.Add(item);
                }
                Action<MyCraftingComponentBase, MyBlueprintToProduce> productionChanged = this.ProductionChanged;
                if (productionChanged != null)
                {
                    productionChanged(this, item);
                }
            }
        }

        [Event(null, 0x199), Reliable, Server]
        private void AddItemToProduce_Request(MyFixedPoint amount, SerializableDefinitionId blueprintId, long senderEntityId)
        {
            if (!this.IsLocked || (senderEntityId == this.m_lockedByEntityId))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyFixedPoint, SerializableDefinitionId>(this, x => new Action<MyFixedPoint, SerializableDefinitionId>(x.AddItemToProduce_Implementation), amount, blueprintId, targetEndpoint);
            }
        }

        public void AddItemToRepair(MyFixedPoint amount, MyBlueprintDefinitionBase blueprint, long senderEntityId, uint inventoryItemId, MyObjectBuilderType inventoryItemType, MyStringHash inventoryItemSubtypeId)
        {
            SerializableDefinitionId id = (SerializableDefinitionId) blueprint.Id;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyFixedPoint, SerializableDefinitionId, long, uint, MyObjectBuilderType, MyStringHash>(this, x => new Action<MyFixedPoint, SerializableDefinitionId, long, uint, MyObjectBuilderType, MyStringHash>(x.AddItemToRepair_Request), amount, id, senderEntityId, inventoryItemId, inventoryItemType, inventoryItemSubtypeId, targetEndpoint);
        }

        [Event(null, 0x1d0), Reliable, Server, Broadcast]
        private void AddItemToRepair_Implementation(MyFixedPoint amount, SerializableDefinitionId blueprintId, uint inventoryItemId, MyObjectBuilderType inventoryItemType, MyStringHash inventoryItemSubtypeId)
        {
            MyBlueprintDefinitionBase blueprint = MyDefinitionManager.Static.GetBlueprintDefinition(blueprintId);
            if (blueprint != null)
            {
                Predicate<MyBlueprintToProduce> match = x => (x is MyRepairBlueprintToProduce) && (ReferenceEquals((x as MyRepairBlueprintToProduce).Blueprint, blueprint) && (((x as MyRepairBlueprintToProduce).InventoryItemId == inventoryItemId) && (((x as MyRepairBlueprintToProduce).InventoryItemType == inventoryItemType) && ((x as MyRepairBlueprintToProduce).InventoryItemSubtypeId == inventoryItemSubtypeId))));
                MyRepairBlueprintToProduce item = this.m_itemsToProduce.Find(match) as MyRepairBlueprintToProduce;
                if (item != null)
                {
                    item.Amount += amount;
                }
                else
                {
                    item = new MyRepairBlueprintToProduce(amount, blueprint, inventoryItemId, inventoryItemType, inventoryItemSubtypeId);
                    this.m_itemsToProduce.Add(item);
                }
                Action<MyCraftingComponentBase, MyBlueprintToProduce> productionChanged = this.ProductionChanged;
                if (productionChanged != null)
                {
                    productionChanged(this, item);
                }
            }
        }

        [Event(null, 0x1c7), Reliable, Server]
        private void AddItemToRepair_Request(MyFixedPoint amount, SerializableDefinitionId blueprintId, long senderEntityId, uint inventoryItemId, MyObjectBuilderType inventoryItemType, MyStringHash inventoryItemSubtypeId)
        {
            if (!this.IsLocked || (senderEntityId == this.m_lockedByEntityId))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyFixedPoint, SerializableDefinitionId, uint, MyObjectBuilderType, MyStringHash>(this, x => new Action<MyFixedPoint, SerializableDefinitionId, uint, MyObjectBuilderType, MyStringHash>(x.AddItemToRepair_Implementation), amount, blueprintId, inventoryItemId, inventoryItemType, inventoryItemSubtypeId, targetEndpoint);
            }
        }

        protected virtual void AddProducedItemToInventory(MyBlueprintDefinitionBase definition, MyFixedPoint amountMult)
        {
            if (Sync.IsServer)
            {
                MyInventory inventory = (base.Entity as MyEntity).GetInventory(0);
                if (inventory != null)
                {
                    foreach (MyBlueprintDefinitionBase.Item item in definition.Results)
                    {
                        MyFixedPoint amount = item.Amount * amountMult;
                        IMyInventoryItem item2 = !(definition is MyBlockBlueprintDefinition) ? this.CreateInventoryItem(item.Id, amount) : this.CreateInventoryBlockItem(item.Id, amount);
                        inventory.Add(item2, item2.Amount);
                    }
                }
            }
        }

        private bool CanItemBeProduced(int i)
        {
            if (this.m_itemsToProduce.IsValidIndex<MyBlueprintToProduce>(i))
            {
                if (!this.CanOperate)
                {
                    return false;
                }
                MyBlueprintToProduce itemToProduce = this.GetItemToProduce(i);
                if (itemToProduce != null)
                {
                    MyBlueprintDefinitionBase blueprint = itemToProduce.Blueprint;
                    MyFixedPoint amount = itemToProduce.Amount;
                    if (this.CanUseBlueprint(blueprint))
                    {
                        return (MyFixedPoint.Min(amount, MyFixedPoint.Min(this.MaxProducableAmount(blueprint, true), this.MaxAmountToFitInventory(blueprint))) > 0);
                    }
                }
            }
            return false;
        }

        public bool CanUseBlueprint(MyBlueprintDefinitionBase blueprint)
        {
            using (List<MyBlueprintClassDefinition>.Enumerator enumerator = this.m_blueprintClasses.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.ContainsBlueprint(blueprint))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected void ClearItemsToProduce()
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, long>(this, x => new Action<long>(x.ClearItemsToProduce_Request), this.m_lockedByEntityId, targetEndpoint);
        }

        public void ClearItemsToProduce(long senderEntityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, long>(this, x => new Action<long>(x.ClearItemsToProduce_Request), senderEntityId, targetEndpoint);
        }

        [Event(null, 0x178), Reliable, Server, Broadcast]
        private void ClearItemsToProduce_Event()
        {
            foreach (MyBlueprintToProduce produce in this.m_itemsToProduce)
            {
                produce.Amount = 0;
                Action<MyCraftingComponentBase, MyBlueprintToProduce> productionChanged = this.ProductionChanged;
                if (productionChanged != null)
                {
                    productionChanged(this, produce);
                }
            }
            this.m_itemsToProduce.Clear();
        }

        [Event(null, 0x16f), Reliable, Server]
        private void ClearItemsToProduce_Request(long senderEntityId)
        {
            if (!this.IsLocked || (senderEntityId == this.m_lockedByEntityId))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase>(this, x => new Action(x.ClearItemsToProduce_Event), targetEndpoint);
            }
        }

        public override void Close()
        {
            base.Close();
            MyEntity entity = null;
            if (MyEntities.TryGetEntityById(this.m_lockedByEntityId, out entity, false))
            {
                entity.OnClose -= new Action<MyEntity>(this.lockEntity_OnClose);
            }
            this.m_lockedByEntityId = -1L;
        }

        public virtual bool ContainsOperatingItem(MyPhysicalInventoryItem item) => 
            false;

        protected IMyInventoryItem CreateInventoryBlockItem(MyDefinitionId blockDefinition, MyFixedPoint amount)
        {
            MyObjectBuilder_BlockItem content = new MyObjectBuilder_BlockItem();
            content.BlockDefId = (SerializableDefinitionId) blockDefinition;
            return new MyPhysicalInventoryItem(amount, content, 1f);
        }

        protected IMyInventoryItem CreateInventoryItem(MyDefinitionId itemDefinition, MyFixedPoint amount) => 
            new MyPhysicalInventoryItem(amount, MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) itemDefinition) as MyObjectBuilder_PhysicalObject, 1f);

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            MyObjectBuilder_CraftingComponentBase base2 = builder as MyObjectBuilder_CraftingComponentBase;
            this.m_lockedByEntityId = base2.LockedByEntityId;
        }

        public MyBlueprintToProduce GetCurrentItemInProduction() => 
            (!this.m_itemsToProduce.IsValidIndex<MyBlueprintToProduce>(this.m_currentItem) ? null : this.m_itemsToProduce[this.m_currentItem]);

        public virtual void GetInsertedOperatingItems(List<MyPhysicalInventoryItem> itemsList)
        {
        }

        public MyBlueprintToProduce GetItemToProduce(int index)
        {
            int num = index;
            return (this.m_itemsToProduce.IsValidIndex<MyBlueprintToProduce>(num) ? this.m_itemsToProduce[index] : null);
        }

        public virtual MyFixedPoint GetOperatingItemRemovableAmount(MyPhysicalInventoryItem item) => 
            0;

        public void InsertOperatingItem(MyPhysicalInventoryItem item, long senderEntityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyObjectBuilder_InventoryItem, long>(this, x => new Action<MyObjectBuilder_InventoryItem, long>(x.InsertOperatingItem_Request), item.GetObjectBuilder(), senderEntityId, targetEndpoint);
        }

        [Event(null, 0x306), Reliable, Server, Broadcast]
        private void InsertOperatingItem_Event([DynamicObjectBuilder(false)] MyObjectBuilder_InventoryItem itemBuilder)
        {
            MyPhysicalInventoryItem item = new MyPhysicalInventoryItem(itemBuilder);
            this.InsertOperatingItem_Implementation(item);
        }

        protected virtual void InsertOperatingItem_Implementation(MyPhysicalInventoryItem item)
        {
        }

        [Event(null, 0x2fe), Reliable, Server]
        private void InsertOperatingItem_Request([DynamicObjectBuilder(false)] MyObjectBuilder_InventoryItem itemBuilder, long senderEntityId)
        {
            if (!this.IsLocked || (senderEntityId == this.m_lockedByEntityId))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyObjectBuilder_InventoryItem>(this, x => new Action<MyObjectBuilder_InventoryItem>(x.InsertOperatingItem_Event), itemBuilder, targetEndpoint);
            }
        }

        [Event(null, 260), Reliable, Server, Broadcast]
        private void InventoryIsFull_Implementation()
        {
            Action<MyCraftingComponentBase> inventoryIsFull = this.InventoryIsFull;
            if (inventoryIsFull != null)
            {
                inventoryIsFull(this);
            }
        }

        private bool IsItemInInventory(uint itemId, MyObjectBuilderType objectBuilderType, MyStringHash subtypeId)
        {
            MyInventory inventory = (base.Entity as MyEntity).GetInventory(0);
            if (inventory == null)
            {
                return false;
            }
            MyPhysicalInventoryItem? itemByID = inventory.GetItemByID(itemId);
            return ((itemByID != null) && ((itemByID.Value.Content != null) && ((itemByID.Value.Content.TypeId == objectBuilderType) && (itemByID.Value.Content.SubtypeId == subtypeId))));
        }

        public virtual bool IsOperatingItem(MyPhysicalInventoryItem item) => 
            false;

        private void lockEntity_OnClose(MyEntity obj)
        {
            obj.OnClose -= new Action<MyEntity>(this.lockEntity_OnClose);
            this.m_lockedByEntityId = -1L;
            if (this.LockReleased != null)
            {
                this.LockReleased();
            }
        }

        protected MyFixedPoint MaxAmountToFitInventory(MyBlueprintDefinitionBase definition)
        {
            MyInventory inventory = (base.Entity as MyEntity).GetInventory(0);
            if (inventory == null)
            {
                return 0;
            }
            MyFixedPoint maxValue = MyFixedPoint.MaxValue;
            float volumeRemoved = 0f;
            float massRemoved = 0f;
            if (!MySession.Static.CreativeMode)
            {
                MyBlueprintDefinitionBase.Item[] prerequisites = definition.Prerequisites;
                for (int i = 0; i < prerequisites.Length; i++)
                {
                    float num4;
                    float num5;
                    MyInventory.GetItemVolumeAndMass(prerequisites[i].Id, out num4, out num5);
                    volumeRemoved += num5;
                    massRemoved += num4;
                }
            }
            foreach (MyBlueprintDefinitionBase.Item item in definition.Results)
            {
                maxValue = MyFixedPoint.Min(inventory.ComputeAmountThatFits(item.Id, volumeRemoved, massRemoved), maxValue);
            }
            return maxValue;
        }

        public MyFixedPoint MaxProducableAmount(MyBlueprintDefinitionBase blueprintDefinition, bool raiseMissingRequiredItemEvent = false)
        {
            if (MySession.Static.CreativeMode)
            {
                return MyFixedPoint.MaxValue;
            }
            MyInventory inventory = (base.Entity as MyEntity).GetInventory(0);
            if (inventory == null)
            {
                return 0;
            }
            MyFixedPoint maxValue = MyFixedPoint.MaxValue;
            foreach (MyBlueprintDefinitionBase.Item item in blueprintDefinition.Prerequisites)
            {
                MyFixedPoint b = MyFixedPoint.Floor((MyFixedPoint) (((float) inventory.GetItemAmount(item.Id, MyItemFlags.None, true)) / ((float) item.Amount)));
                maxValue = MyFixedPoint.Min(maxValue, b);
                if (maxValue == 0)
                {
                    if (raiseMissingRequiredItemEvent)
                    {
                        this.RaiseEvent_MissingRequiredItem(blueprintDefinition, item);
                    }
                    return maxValue;
                }
            }
            return maxValue;
        }

        [Event(null, 0xda), Reliable, Server, Broadcast]
        private void MissingRequiredItem_Implementation(SerializableDefinitionId blueprintId, SerializableDefinitionId missingItemId)
        {
            MyBlueprintDefinitionBase.Item item = new MyBlueprintDefinitionBase.Item();
            MyDefinitionId id = missingItemId;
            bool flag = false;
            MyBlueprintDefinitionBase blueprintDefinition = MyDefinitionManager.Static.GetBlueprintDefinition(blueprintId);
            if (blueprintDefinition != null)
            {
                foreach (MyBlueprintDefinitionBase.Item item2 in blueprintDefinition.Prerequisites)
                {
                    if (item2.Id == id)
                    {
                        item = item2;
                        flag = true;
                    }
                }
                bool flag1 = flag;
                Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyBlueprintDefinitionBase.Item> missingRequiredItem = this.MissingRequiredItem;
                if ((missingRequiredItem != null) & flag)
                {
                    missingRequiredItem(this, blueprintDefinition, item);
                }
            }
        }

        [Event(null, 0x382), Reliable, Broadcast]
        private void NewItemSelected_Event(int index)
        {
            this.m_currentItem = !this.m_itemsToProduce.IsValidIndex<MyBlueprintToProduce>(index) ? -1 : index;
            this.m_currentItemStatus = 0f;
            this.RaiseEvent_ProductionChanged();
        }

        protected void OnBlueprintProduced(MyBlueprintDefinitionBase blueprint, MyFixedPoint amount)
        {
            SerializableDefinitionId id = (SerializableDefinitionId) blueprint.Id;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, SerializableDefinitionId, MyFixedPoint>(this, x => new Action<SerializableDefinitionId, MyFixedPoint>(x.OnBlueprintProduced_Implementation), id, amount, targetEndpoint);
        }

        [Event(null, 0x120), Reliable, Server, Broadcast]
        private void OnBlueprintProduced_Implementation(SerializableDefinitionId blueprintId, MyFixedPoint amount)
        {
            MyBlueprintDefinitionBase blueprintDefinition = MyDefinitionManager.Static.GetBlueprintDefinition(blueprintId);
            if (blueprintDefinition != null)
            {
                Action<MyCraftingComponentBase, MyBlueprintDefinitionBase, MyFixedPoint> blueprintProduced = this.BlueprintProduced;
                if (blueprintProduced != null)
                {
                    blueprintProduced(this, blueprintDefinition, amount);
                }
            }
        }

        protected void RaiseEvent_InventoryIsFull()
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase>(this, x => new Action(x.InventoryIsFull_Implementation), targetEndpoint);
        }

        protected void RaiseEvent_MissingRequiredItem(MyBlueprintDefinitionBase blueprint, MyBlueprintDefinitionBase.Item missingItem)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, SerializableDefinitionId, SerializableDefinitionId>(this, x => new Action<SerializableDefinitionId, SerializableDefinitionId>(x.MissingRequiredItem_Implementation), (SerializableDefinitionId) blueprint.Id, (SerializableDefinitionId) missingItem.Id, targetEndpoint);
        }

        private void RaiseEvent_NewItemSelected(int index)
        {
            this.m_currentItem = index;
            this.m_currentItemStatus = 0f;
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase, int>(this, x => new Action<int>(x.NewItemSelected_Event), index, targetEndpoint);
            }
            this.RaiseEvent_ProductionChanged();
        }

        protected void RaiseEvent_OperatingChanged()
        {
            Action<MyCraftingComponentBase> operatingChanged = this.OperatingChanged;
            if (operatingChanged != null)
            {
                operatingChanged(this);
            }
        }

        protected void RaiseEvent_ProductionChanged()
        {
            if (!this.m_itemsToProduce.IsValidIndex<MyBlueprintToProduce>(this.m_currentItem))
            {
                if (this.ProductionChanged != null)
                {
                    this.ProductionChanged(this, null);
                }
            }
            else if (this.ProductionChanged != null)
            {
                this.ProductionChanged(this, this.m_itemsToProduce[this.m_currentItem]);
            }
        }

        [Event(null, 0x44a), Reliable, Server]
        private void ReleaseLock_Event(long entityId)
        {
            if (this.m_lockedByEntityId == entityId)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase, long>(this, x => new Action<long>(x.ReleaseLock_Implementation), entityId, targetEndpoint);
            }
        }

        [Event(null, 0x453), Reliable, Server, Broadcast]
        private void ReleaseLock_Implementation(long entityId)
        {
            MyEntity entity = null;
            if (MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                entity.OnClose -= new Action<MyEntity>(this.lockEntity_OnClose);
            }
            this.m_lockedByEntityId = -1L;
            if (this.LockReleased != null)
            {
                this.LockReleased();
            }
        }

        public void ReleaseLockRequest(long entityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, long>(this, x => new Action<long>(x.ReleaseLock_Event), entityId, targetEndpoint);
        }

        public void RemoveItemToProduce(MyFixedPoint amount, MyBlueprintToProduce blueprintInProduction, long senderEntityId)
        {
            SerializableDefinitionId id = (SerializableDefinitionId) blueprintInProduction.Blueprint.Id;
            int index = this.m_itemsToProduce.IndexOf(blueprintInProduction);
            if (this.m_itemsToProduce.IsValidIndex<MyBlueprintToProduce>(index))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyFixedPoint, SerializableDefinitionId, long, int>(this, x => new Action<MyFixedPoint, SerializableDefinitionId, long, int>(x.RemoveItemToProduce_Request), amount, id, senderEntityId, index, targetEndpoint);
            }
        }

        public void RemoveItemToProduce(MyFixedPoint amount, MyBlueprintDefinitionBase blueprint, long senderEntityId, int itemId = -1)
        {
            SerializableDefinitionId id = (SerializableDefinitionId) blueprint.Id;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyFixedPoint, SerializableDefinitionId, long, int>(this, x => new Action<MyFixedPoint, SerializableDefinitionId, long, int>(x.RemoveItemToProduce_Request), amount, id, senderEntityId, itemId, targetEndpoint);
        }

        [Event(null, 0x210), Reliable, Server, Broadcast]
        private void RemoveItemToProduce_Implementation(MyFixedPoint amount, SerializableDefinitionId blueprintId, int itemId = -1)
        {
            MyBlueprintDefinitionBase blueprint = MyDefinitionManager.Static.GetBlueprintDefinition(blueprintId);
            if (blueprint != null)
            {
                MyBlueprintToProduce currentItemInProduction = this.GetCurrentItemInProduction();
                MyBlueprintToProduce item = !this.m_itemsToProduce.IsValidIndex<MyBlueprintToProduce>(itemId) ? this.m_itemsToProduce.Find(x => ReferenceEquals(x.Blueprint, blueprint)) : this.m_itemsToProduce[itemId];
                if (item != null)
                {
                    item.Amount -= amount;
                    if (item.Amount <= 0)
                    {
                        this.m_itemsToProduce.Remove(item);
                    }
                    if (ReferenceEquals(currentItemInProduction, item) && ((this.m_currentItemStatus >= 1f) || (item.Amount == 0)))
                    {
                        this.SelectItemToProduction();
                    }
                    Action<MyCraftingComponentBase, MyBlueprintToProduce> productionChanged = this.ProductionChanged;
                    if (productionChanged != null)
                    {
                        productionChanged(this, item);
                    }
                }
            }
        }

        [Event(null, 0x207), Reliable, Server]
        private void RemoveItemToProduce_Request(MyFixedPoint amount, SerializableDefinitionId blueprintId, long senderEntityId, int itemId = -1)
        {
            if (!this.IsLocked || (senderEntityId == this.m_lockedByEntityId))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyFixedPoint, SerializableDefinitionId, int>(this, x => new Action<MyFixedPoint, SerializableDefinitionId, int>(x.RemoveItemToProduce_Implementation), amount, blueprintId, itemId, targetEndpoint);
            }
        }

        protected void RemoveOperatingItem(MyPhysicalInventoryItem item, MyFixedPoint amount)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyObjectBuilder_InventoryItem, MyFixedPoint, long>(this, x => new Action<MyObjectBuilder_InventoryItem, MyFixedPoint, long>(x.RemoveOperatingItem_Request), item.GetObjectBuilder(), amount, this.m_lockedByEntityId, targetEndpoint);
        }

        public void RemoveOperatingItem(MyPhysicalInventoryItem item, MyFixedPoint amount, long senderEntityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyObjectBuilder_InventoryItem, MyFixedPoint, long>(this, x => new Action<MyObjectBuilder_InventoryItem, MyFixedPoint, long>(x.RemoveOperatingItem_Request), item.GetObjectBuilder(), amount, senderEntityId, targetEndpoint);
        }

        [Event(null, 0x31f), Reliable, Server, Broadcast]
        private void RemoveOperatingItem_Event([DynamicObjectBuilder(false)] MyObjectBuilder_InventoryItem itemBuilder, MyFixedPoint amount)
        {
            MyPhysicalInventoryItem item = new MyPhysicalInventoryItem(itemBuilder);
            this.RemoveOperatingItem_Implementation(item, amount);
        }

        protected virtual void RemoveOperatingItem_Implementation(MyPhysicalInventoryItem item, MyFixedPoint amount)
        {
        }

        [Event(null, 0x317), Reliable, Server]
        private void RemoveOperatingItem_Request([DynamicObjectBuilder(false)] MyObjectBuilder_InventoryItem itemBuilder, MyFixedPoint amount, long senderEntityId)
        {
            if (!this.IsLocked || (senderEntityId == this.m_lockedByEntityId))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase, MyObjectBuilder_InventoryItem, MyFixedPoint>(this, x => new Action<MyObjectBuilder_InventoryItem, MyFixedPoint>(x.RemoveOperatingItem_Event), itemBuilder, amount, targetEndpoint);
            }
        }

        protected void RemovePrereqItemsFromInventory(MyBlueprintDefinitionBase definition, MyFixedPoint amountMult)
        {
            if (Sync.IsServer && !MySession.Static.CreativeMode)
            {
                MyInventory inventory = (base.Entity as MyEntity).GetInventory(0);
                if (inventory != null)
                {
                    MyBlueprintDefinitionBase.Item[] prerequisites = definition.Prerequisites;
                    int index = 0;
                    while (true)
                    {
                        while (true)
                        {
                            if (index >= prerequisites.Length)
                            {
                                return;
                            }
                            MyBlueprintDefinitionBase.Item item1 = prerequisites[index];
                            MyFixedPoint amount = item1.Amount * amountMult;
                            MyDefinitionId id = item1.Id;
                            MyFixedPoint point2 = 0;
                            if ((MySessionComponentEquivalency.Static != null) && MySessionComponentEquivalency.Static.HasEquivalents(id))
                            {
                                MyFixedPoint point3 = amount;
                                foreach (MyDefinitionId id2 in MySessionComponentEquivalency.Static.GetEquivalents(id))
                                {
                                    MyFixedPoint point4 = inventory.RemoveItemsOfType(point3, id2, MyItemFlags.None, false);
                                    point3 -= point4;
                                    point2 += point4;
                                    if (point3 == 0)
                                    {
                                        break;
                                    }
                                }
                                break;
                            }
                            point2 += inventory.RemoveItemsOfType(amount, id, MyItemFlags.None, false);
                            break;
                        }
                        index++;
                    }
                }
            }
        }

        private void RepairInventoryItem(uint itemId, MyObjectBuilderType objectBuilderType, MyStringHash subtypeId, float amount)
        {
            MyInventory inventory = (base.Entity as MyEntity).GetInventory(0);
            if (inventory != null)
            {
                MyPhysicalInventoryItem? itemByID = inventory.GetItemByID(itemId);
                if (((itemByID != null) && ((itemByID.Value.Content != null) && (itemByID.Value.Content.TypeId == objectBuilderType))) && (itemByID.Value.Content.SubtypeId == subtypeId))
                {
                    float local1;
                    if ((itemByID.Value.Content == null) || (itemByID.Value.Content.DurabilityHP == null))
                    {
                        local1 = 0f;
                    }
                    else
                    {
                        local1 = itemByID.Value.Content.DurabilityHP.Value;
                    }
                    float? nullable2 = null;
                    inventory.UpdateItem(((IMyInventoryItem) itemByID).GetDefinitionId(), new uint?(itemId), nullable2, new float?(MathHelper.Clamp((float) (local1 + amount), (float) 0f, (float) 100f)));
                }
            }
        }

        protected void SelectItemToProduction()
        {
            if (Sync.IsServer)
            {
                if ((this.BlueprintsToProduceCount == 0) || !this.CanOperate)
                {
                    this.RaiseEvent_NewItemSelected(-1);
                }
                else if (this.CanItemBeProduced(this.m_currentItem))
                {
                    this.RaiseEvent_NewItemSelected(this.m_currentItem);
                }
                else
                {
                    float num = 0f;
                    for (int i = 0; i < this.BlueprintsToProduceCount; i++)
                    {
                        MyBlueprintDefinitionBase blueprint = this.GetItemToProduce(i).Blueprint;
                        MyFixedPoint point = this.MaxAmountToFitInventory(blueprint);
                        num += (float) point;
                        if (this.CanItemBeProduced(i))
                        {
                            this.RaiseEvent_NewItemSelected(i);
                            return;
                        }
                    }
                    if ((num == 0f) && (this.BlueprintsToProduceCount > 0))
                    {
                        this.RaiseEvent_InventoryIsFull();
                    }
                    this.RaiseEvent_NewItemSelected(-1);
                }
            }
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_CraftingComponentBase base1 = base.Serialize(false) as MyObjectBuilder_CraftingComponentBase;
            base1.LockedByEntityId = this.m_lockedByEntityId;
            return base1;
        }

        protected void StartProduction()
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, long>(this, x => new Action<long>(x.StartProduction_Request), this.m_lockedByEntityId, targetEndpoint);
        }

        public void StartProduction(long senderEntityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, long>(this, x => new Action<long>(x.StartProduction_Request), senderEntityId, targetEndpoint);
        }

        [Event(null, 0x146), Reliable, Server, Broadcast]
        private void StartProduction_Event()
        {
            this.StartProduction_Implementation();
        }

        protected virtual void StartProduction_Implementation()
        {
        }

        [Event(null, 0x13d), Reliable, Server]
        private void StartProduction_Request(long senderEntityId)
        {
            if (!this.IsLocked || (senderEntityId == this.m_lockedByEntityId))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase>(this, x => new Action(x.StartProduction_Event), targetEndpoint);
            }
        }

        protected void StopOperating()
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase>(this, x => new Action(x.StopOperating_Event), targetEndpoint);
        }

        [Event(null, 0x335), Reliable, Server, Broadcast]
        private void StopOperating_Event()
        {
            this.StopOperating_Implementation();
        }

        protected virtual void StopOperating_Implementation()
        {
        }

        protected void StopProduction()
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, long>(this, x => new Action<long>(x.StopProduction_Request), this.m_lockedByEntityId, targetEndpoint);
        }

        public void StopProduction(long senderEntityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCraftingComponentBase, long>(this, x => new Action<long>(x.StopProduction_Request), senderEntityId, targetEndpoint);
        }

        [Event(null, 0x15f), Reliable, Server, Broadcast]
        private void StopProduction_Event()
        {
            this.StopProduction_Implementation();
        }

        protected virtual void StopProduction_Implementation()
        {
        }

        [Event(null, 0x156), Reliable, Server]
        private void StopProduction_Request(long senderEntityId)
        {
            if (!this.IsLocked || (senderEntityId == this.m_lockedByEntityId))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCraftingComponentBase>(this, x => new Action(x.StopProduction_Event), targetEndpoint);
            }
        }

        public MyBlueprintToProduce TryGetItemToProduce(MyBlueprintDefinitionBase blueprint) => 
            this.m_itemsToProduce.Find(x => ReferenceEquals(x.Blueprint, blueprint));

        public MyRepairBlueprintToProduce TryGetItemToRepair(uint inventoryItemId, MyObjectBuilderType inventoryItemType, MyStringHash inventoryItemSubtypeId) => 
            (this.m_itemsToProduce.Find(x => (x is MyRepairBlueprintToProduce) && (((x as MyRepairBlueprintToProduce).InventoryItemId == inventoryItemId) && (((x as MyRepairBlueprintToProduce).InventoryItemType == inventoryItemType) && ((x as MyRepairBlueprintToProduce).InventoryItemSubtypeId == inventoryItemSubtypeId)))) as MyRepairBlueprintToProduce);

        protected void UpdateCurrentItem()
        {
            MyBlueprintToProduce itemToProduce = this.GetItemToProduce(this.m_currentItem);
            if (itemToProduce != null)
            {
                if (!this.CanItemBeProduced(this.m_currentItem))
                {
                    this.SelectItemToProduction();
                }
                if (itemToProduce is MyRepairBlueprintToProduce)
                {
                    MyRepairBlueprintToProduce produce2 = itemToProduce as MyRepairBlueprintToProduce;
                    if (!this.IsItemInInventory(produce2.InventoryItemId, produce2.InventoryItemType, produce2.InventoryItemSubtypeId) && Sync.IsServer)
                    {
                        this.RemoveItemToProduce(this.m_currentProductionAmount, produce2.Blueprint, this.m_lockedByEntityId, this.m_currentItem);
                    }
                }
                MyBlueprintDefinitionBase blueprint = itemToProduce.Blueprint;
                this.UpdateCurrentItemStatus(0f);
                this.m_lastItemStatus = this.m_currentItemStatus;
                if (Math.Abs((float) (this.m_lastItemStatus - this.m_currentItemStatus)) > 0.01f)
                {
                    this.RaiseEvent_ProductionChanged();
                }
                if (this.m_currentItemStatus >= 1f)
                {
                    MyRepairBlueprintToProduce produce3 = itemToProduce as MyRepairBlueprintToProduce;
                    MyRepairBlueprintDefinition definition = blueprint as MyRepairBlueprintDefinition;
                    if ((produce3 == null) || (definition == null))
                    {
                        if (Sync.IsServer)
                        {
                            this.RemovePrereqItemsFromInventory(blueprint, this.m_currentProductionAmount);
                            this.AddProducedItemToInventory(blueprint, this.m_currentProductionAmount);
                            this.RemoveItemToProduce(this.m_currentProductionAmount, blueprint, this.m_lockedByEntityId, -1);
                            this.OnBlueprintProduced(blueprint, this.m_currentProductionAmount);
                        }
                    }
                    else
                    {
                        this.RepairInventoryItem(produce3.InventoryItemId, produce3.InventoryItemType, produce3.InventoryItemSubtypeId, definition.RepairAmount);
                        if (Sync.IsServer)
                        {
                            this.RemovePrereqItemsFromInventory(blueprint, this.m_currentProductionAmount);
                            this.RemoveItemToProduce(this.m_currentProductionAmount, blueprint, this.m_lockedByEntityId, this.m_currentItem);
                            this.OnBlueprintProduced(blueprint, this.m_currentProductionAmount);
                        }
                    }
                }
            }
        }

        public virtual void UpdateCurrentItemStatus(float statusDelta)
        {
            if (this.IsProducing)
            {
                MyBlueprintToProduce itemToProduce = this.GetItemToProduce(this.m_currentItem);
                if (itemToProduce != null)
                {
                    MyBlueprintDefinitionBase blueprint = itemToProduce.Blueprint;
                    this.m_currentItemStatus = Math.Min((float) 1f, (float) (this.m_currentItemStatus + ((this.m_elapsedTimeMs * this.m_craftingSpeedMultiplier) / (blueprint.BaseProductionTimeInSeconds * 1000f))));
                }
            }
        }

        protected virtual void UpdateOperatingLevel()
        {
        }

        protected abstract void UpdateProduction_Implementation();

        public bool IsProductionDone =>
            (this.m_itemsToProduce.Count == 0);

        public List<MyBlueprintClassDefinition> AvailableBlueprintClasses =>
            this.m_blueprintClasses;

        public int BlueprintsToProduceCount =>
            this.m_itemsToProduce.Count;

        public abstract string DisplayNameText { get; }

        public abstract bool RequiresItemsToOperate { get; }

        public virtual string OperatingItemsDisplayNameText =>
            string.Empty;

        public abstract bool CanOperate { get; }

        public virtual float OperatingItemsLevel =>
            1f;

        public virtual bool AcceptsOperatingItems =>
            false;

        public virtual float AvailableOperatingSpace =>
            0f;

        public bool IsProducing =>
            this.m_itemsToProduce.IsValidIndex<MyBlueprintToProduce>(this.m_currentItem);

        public float CurrentItemStatus =>
            this.m_currentItemStatus;

        public bool IsLocked
        {
            get
            {
                MyEntity entity;
                return MyEntities.TryGetEntityById(this.m_lockedByEntityId, out entity, false);
            }
        }

        public long LockedByEntityId =>
            this.m_lockedByEntityId;

        public List<MyBlueprintToProduce> ItemsInProduction =>
            this.m_itemsToProduce;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCraftingComponentBase.<>c <>9 = new MyCraftingComponentBase.<>c();
            public static Func<MyCraftingComponentBase, Action<SerializableDefinitionId, SerializableDefinitionId>> <>9__73_0;
            public static Func<MyCraftingComponentBase, Action> <>9__75_0;
            public static Func<MyCraftingComponentBase, Action<SerializableDefinitionId, MyFixedPoint>> <>9__78_0;
            public static Func<MyCraftingComponentBase, Action<long>> <>9__80_0;
            public static Func<MyCraftingComponentBase, Action<long>> <>9__81_0;
            public static Func<MyCraftingComponentBase, Action> <>9__82_0;
            public static Func<MyCraftingComponentBase, Action<long>> <>9__84_0;
            public static Func<MyCraftingComponentBase, Action<long>> <>9__85_0;
            public static Func<MyCraftingComponentBase, Action> <>9__86_0;
            public static Func<MyCraftingComponentBase, Action<long>> <>9__88_0;
            public static Func<MyCraftingComponentBase, Action<long>> <>9__89_0;
            public static Func<MyCraftingComponentBase, Action> <>9__90_0;
            public static Func<MyCraftingComponentBase, Action<MyFixedPoint, SerializableDefinitionId, long>> <>9__93_0;
            public static Func<MyCraftingComponentBase, Action<MyFixedPoint, SerializableDefinitionId>> <>9__94_0;
            public static Func<MyCraftingComponentBase, Action<MyFixedPoint, SerializableDefinitionId, long, uint, MyObjectBuilderType, MyStringHash>> <>9__96_0;
            public static Func<MyCraftingComponentBase, Action<MyFixedPoint, SerializableDefinitionId, uint, MyObjectBuilderType, MyStringHash>> <>9__97_0;
            public static Func<MyCraftingComponentBase, Action<MyFixedPoint, SerializableDefinitionId, long, int>> <>9__99_0;
            public static Func<MyCraftingComponentBase, Action<MyFixedPoint, SerializableDefinitionId, long, int>> <>9__100_0;
            public static Func<MyCraftingComponentBase, Action<MyFixedPoint, SerializableDefinitionId, int>> <>9__101_0;
            public static Func<MyCraftingComponentBase, Action<MyObjectBuilder_InventoryItem, long>> <>9__111_0;
            public static Func<MyCraftingComponentBase, Action<MyObjectBuilder_InventoryItem>> <>9__112_0;
            public static Func<MyCraftingComponentBase, Action<MyObjectBuilder_InventoryItem, MyFixedPoint, long>> <>9__114_0;
            public static Func<MyCraftingComponentBase, Action<MyObjectBuilder_InventoryItem, MyFixedPoint, long>> <>9__115_0;
            public static Func<MyCraftingComponentBase, Action<MyObjectBuilder_InventoryItem, MyFixedPoint>> <>9__116_0;
            public static Func<MyCraftingComponentBase, Action> <>9__119_0;
            public static Func<MyCraftingComponentBase, Action<int>> <>9__123_0;
            public static Func<MyCraftingComponentBase, Action<long>> <>9__131_0;
            public static Func<MyCraftingComponentBase, Action<long>> <>9__132_0;
            public static Func<MyCraftingComponentBase, Action<long>> <>9__133_0;
            public static Func<MyCraftingComponentBase, Action<long>> <>9__135_0;

            internal Action<long> <AcquireLock_Event>b__135_0(MyCraftingComponentBase x) => 
                new Action<long>(x.AcquireLock_Implementation);

            internal Action<long> <AcquireLockRequest>b__131_0(MyCraftingComponentBase x) => 
                new Action<long>(x.AcquireLock_Event);

            internal Action<MyFixedPoint, SerializableDefinitionId> <AddItemToProduce_Request>b__94_0(MyCraftingComponentBase x) => 
                new Action<MyFixedPoint, SerializableDefinitionId>(x.AddItemToProduce_Implementation);

            internal Action<MyFixedPoint, SerializableDefinitionId, long> <AddItemToProduce>b__93_0(MyCraftingComponentBase x) => 
                new Action<MyFixedPoint, SerializableDefinitionId, long>(x.AddItemToProduce_Request);

            internal Action<MyFixedPoint, SerializableDefinitionId, uint, MyObjectBuilderType, MyStringHash> <AddItemToRepair_Request>b__97_0(MyCraftingComponentBase x) => 
                new Action<MyFixedPoint, SerializableDefinitionId, uint, MyObjectBuilderType, MyStringHash>(x.AddItemToRepair_Implementation);

            internal Action<MyFixedPoint, SerializableDefinitionId, long, uint, MyObjectBuilderType, MyStringHash> <AddItemToRepair>b__96_0(MyCraftingComponentBase x) => 
                new Action<MyFixedPoint, SerializableDefinitionId, long, uint, MyObjectBuilderType, MyStringHash>(x.AddItemToRepair_Request);

            internal Action <ClearItemsToProduce_Request>b__90_0(MyCraftingComponentBase x) => 
                new Action(x.ClearItemsToProduce_Event);

            internal Action<long> <ClearItemsToProduce>b__88_0(MyCraftingComponentBase x) => 
                new Action<long>(x.ClearItemsToProduce_Request);

            internal Action<long> <ClearItemsToProduce>b__89_0(MyCraftingComponentBase x) => 
                new Action<long>(x.ClearItemsToProduce_Request);

            internal Action<MyObjectBuilder_InventoryItem> <InsertOperatingItem_Request>b__112_0(MyCraftingComponentBase x) => 
                new Action<MyObjectBuilder_InventoryItem>(x.InsertOperatingItem_Event);

            internal Action<MyObjectBuilder_InventoryItem, long> <InsertOperatingItem>b__111_0(MyCraftingComponentBase x) => 
                new Action<MyObjectBuilder_InventoryItem, long>(x.InsertOperatingItem_Request);

            internal Action<SerializableDefinitionId, MyFixedPoint> <OnBlueprintProduced>b__78_0(MyCraftingComponentBase x) => 
                new Action<SerializableDefinitionId, MyFixedPoint>(x.OnBlueprintProduced_Implementation);

            internal Action <RaiseEvent_InventoryIsFull>b__75_0(MyCraftingComponentBase x) => 
                new Action(x.InventoryIsFull_Implementation);

            internal Action<SerializableDefinitionId, SerializableDefinitionId> <RaiseEvent_MissingRequiredItem>b__73_0(MyCraftingComponentBase x) => 
                new Action<SerializableDefinitionId, SerializableDefinitionId>(x.MissingRequiredItem_Implementation);

            internal Action<int> <RaiseEvent_NewItemSelected>b__123_0(MyCraftingComponentBase x) => 
                new Action<int>(x.NewItemSelected_Event);

            internal Action<long> <ReleaseLock_Event>b__133_0(MyCraftingComponentBase x) => 
                new Action<long>(x.ReleaseLock_Implementation);

            internal Action<long> <ReleaseLockRequest>b__132_0(MyCraftingComponentBase x) => 
                new Action<long>(x.ReleaseLock_Event);

            internal Action<MyFixedPoint, SerializableDefinitionId, int> <RemoveItemToProduce_Request>b__101_0(MyCraftingComponentBase x) => 
                new Action<MyFixedPoint, SerializableDefinitionId, int>(x.RemoveItemToProduce_Implementation);

            internal Action<MyFixedPoint, SerializableDefinitionId, long, int> <RemoveItemToProduce>b__100_0(MyCraftingComponentBase x) => 
                new Action<MyFixedPoint, SerializableDefinitionId, long, int>(x.RemoveItemToProduce_Request);

            internal Action<MyFixedPoint, SerializableDefinitionId, long, int> <RemoveItemToProduce>b__99_0(MyCraftingComponentBase x) => 
                new Action<MyFixedPoint, SerializableDefinitionId, long, int>(x.RemoveItemToProduce_Request);

            internal Action<MyObjectBuilder_InventoryItem, MyFixedPoint> <RemoveOperatingItem_Request>b__116_0(MyCraftingComponentBase x) => 
                new Action<MyObjectBuilder_InventoryItem, MyFixedPoint>(x.RemoveOperatingItem_Event);

            internal Action<MyObjectBuilder_InventoryItem, MyFixedPoint, long> <RemoveOperatingItem>b__114_0(MyCraftingComponentBase x) => 
                new Action<MyObjectBuilder_InventoryItem, MyFixedPoint, long>(x.RemoveOperatingItem_Request);

            internal Action<MyObjectBuilder_InventoryItem, MyFixedPoint, long> <RemoveOperatingItem>b__115_0(MyCraftingComponentBase x) => 
                new Action<MyObjectBuilder_InventoryItem, MyFixedPoint, long>(x.RemoveOperatingItem_Request);

            internal Action <StartProduction_Request>b__82_0(MyCraftingComponentBase x) => 
                new Action(x.StartProduction_Event);

            internal Action<long> <StartProduction>b__80_0(MyCraftingComponentBase x) => 
                new Action<long>(x.StartProduction_Request);

            internal Action<long> <StartProduction>b__81_0(MyCraftingComponentBase x) => 
                new Action<long>(x.StartProduction_Request);

            internal Action <StopOperating>b__119_0(MyCraftingComponentBase x) => 
                new Action(x.StopOperating_Event);

            internal Action <StopProduction_Request>b__86_0(MyCraftingComponentBase x) => 
                new Action(x.StopProduction_Event);

            internal Action<long> <StopProduction>b__84_0(MyCraftingComponentBase x) => 
                new Action<long>(x.StopProduction_Request);

            internal Action<long> <StopProduction>b__85_0(MyCraftingComponentBase x) => 
                new Action<long>(x.StopProduction_Request);
        }

        public class MyBlueprintToProduce
        {
            public MyFixedPoint Amount;
            public MyBlueprintDefinitionBase Blueprint;

            public MyBlueprintToProduce(MyFixedPoint amount, MyBlueprintDefinitionBase blueprint)
            {
                this.Amount = amount;
                this.Blueprint = blueprint;
            }
        }

        public class MyRepairBlueprintToProduce : MyCraftingComponentBase.MyBlueprintToProduce
        {
            public uint InventoryItemId;
            public MyObjectBuilderType InventoryItemType;
            public MyStringHash InventoryItemSubtypeId;

            public MyRepairBlueprintToProduce(MyFixedPoint amount, MyBlueprintDefinitionBase blueprint, uint inventoryItemId, MyObjectBuilderType inventoryItemType, MyStringHash inventoryItemSubtypeId) : base(amount, blueprint)
            {
                this.InventoryItemId = inventoryItemId;
                this.InventoryItemType = inventoryItemType;
                this.InventoryItemSubtypeId = inventoryItemSubtypeId;
            }
        }
    }
}

