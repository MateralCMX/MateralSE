namespace Sandbox.Game.Components
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;

    [MyComponentBuilder(typeof(MyObjectBuilder_CraftingComponentBlock), true)]
    public class MyCraftingComponentBlock : MyCraftingComponentBase, IMyEventProxy, IMyEventOwner
    {
        private string m_operatingItemsDisplayNameText = "Flammables";
        private bool m_requiresItemsToOperate = true;
        private float m_maxInsertedItems = 20f;
        private int m_operatingItemLastTimeMs = 0x4e20;
        private List<MyDefinitionId> m_acceptedOperatingItems = new List<MyDefinitionId>();
        private List<MyPhysicalInventoryItem> m_insertedItems = new List<MyPhysicalInventoryItem>();
        private float m_operatingItemsLevel;
        private float m_lastOperatingLevel;
        private int m_operatingItemTimerMs;
        private int m_lastUpdateTime;
        private float m_insertedItemUseLevel;
        private float m_currentInsertedItemsCount;
        [CompilerGenerated]
        private Action<MyCraftingComponentBlock, MyCubeBlock> OnBlockTurnedOn;
        [CompilerGenerated]
        private Action<MyCraftingComponentBlock, MyCubeBlock> OnBlockTurnedOff;
        private bool m_updatingOperatingLevel;
        private bool m_blockEnabled;
        private bool m_paused;

        public event Action<MyCraftingComponentBlock, MyCubeBlock> OnBlockTurnedOff
        {
            [CompilerGenerated] add
            {
                Action<MyCraftingComponentBlock, MyCubeBlock> onBlockTurnedOff = this.OnBlockTurnedOff;
                while (true)
                {
                    Action<MyCraftingComponentBlock, MyCubeBlock> a = onBlockTurnedOff;
                    Action<MyCraftingComponentBlock, MyCubeBlock> action3 = (Action<MyCraftingComponentBlock, MyCubeBlock>) Delegate.Combine(a, value);
                    onBlockTurnedOff = Interlocked.CompareExchange<Action<MyCraftingComponentBlock, MyCubeBlock>>(ref this.OnBlockTurnedOff, action3, a);
                    if (ReferenceEquals(onBlockTurnedOff, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCraftingComponentBlock, MyCubeBlock> onBlockTurnedOff = this.OnBlockTurnedOff;
                while (true)
                {
                    Action<MyCraftingComponentBlock, MyCubeBlock> source = onBlockTurnedOff;
                    Action<MyCraftingComponentBlock, MyCubeBlock> action3 = (Action<MyCraftingComponentBlock, MyCubeBlock>) Delegate.Remove(source, value);
                    onBlockTurnedOff = Interlocked.CompareExchange<Action<MyCraftingComponentBlock, MyCubeBlock>>(ref this.OnBlockTurnedOff, action3, source);
                    if (ReferenceEquals(onBlockTurnedOff, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCraftingComponentBlock, MyCubeBlock> OnBlockTurnedOn
        {
            [CompilerGenerated] add
            {
                Action<MyCraftingComponentBlock, MyCubeBlock> onBlockTurnedOn = this.OnBlockTurnedOn;
                while (true)
                {
                    Action<MyCraftingComponentBlock, MyCubeBlock> a = onBlockTurnedOn;
                    Action<MyCraftingComponentBlock, MyCubeBlock> action3 = (Action<MyCraftingComponentBlock, MyCubeBlock>) Delegate.Combine(a, value);
                    onBlockTurnedOn = Interlocked.CompareExchange<Action<MyCraftingComponentBlock, MyCubeBlock>>(ref this.OnBlockTurnedOn, action3, a);
                    if (ReferenceEquals(onBlockTurnedOn, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCraftingComponentBlock, MyCubeBlock> onBlockTurnedOn = this.OnBlockTurnedOn;
                while (true)
                {
                    Action<MyCraftingComponentBlock, MyCubeBlock> source = onBlockTurnedOn;
                    Action<MyCraftingComponentBlock, MyCubeBlock> action3 = (Action<MyCraftingComponentBlock, MyCubeBlock>) Delegate.Remove(source, value);
                    onBlockTurnedOn = Interlocked.CompareExchange<Action<MyCraftingComponentBlock, MyCubeBlock>>(ref this.OnBlockTurnedOn, action3, source);
                    if (ReferenceEquals(onBlockTurnedOn, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyCraftingComponentBlock()
        {
            this.m_operatingItemsDisplayNameText = MyTexts.GetString(MyCommonTexts.DisplayName_Flammables);
        }

        public override bool ContainsOperatingItem(MyPhysicalInventoryItem item) => 
            ((this.m_insertedItems != null) ? this.m_insertedItems.Contains(item) : false);

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            MyObjectBuilder_CraftingComponentBlock block = builder as MyObjectBuilder_CraftingComponentBlock;
            this.m_insertedItemUseLevel = block.InsertedItemUseLevel;
            foreach (MyObjectBuilder_InventoryItem item in block.InsertedItems)
            {
                if (item.Amount <= 0)
                {
                    continue;
                }
                if ((item.PhysicalContent != null) && ((this.m_currentInsertedItemsCount + ((float) item.Amount)) <= this.m_maxInsertedItems))
                {
                    MyPhysicalInventoryItem item2 = new MyPhysicalInventoryItem(item);
                    this.m_currentInsertedItemsCount += (float) item2.Amount;
                    this.m_insertedItems.Add(item2);
                }
            }
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            this.UpdateOperatingLevel();
        }

        public override void GetInsertedOperatingItems(List<MyPhysicalInventoryItem> itemsList)
        {
            itemsList.AddList<MyPhysicalInventoryItem>(this.m_insertedItems);
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) => 
            null;

        public override MyFixedPoint GetOperatingItemRemovableAmount(MyPhysicalInventoryItem item)
        {
            int index = this.m_insertedItems.FindIndex(x => x.Content.GetId() == item.Content.GetId());
            if (!this.m_insertedItems.IsValidIndex<MyPhysicalInventoryItem>(index))
            {
                return 0;
            }
            MyFixedPoint amount = this.m_insertedItems[index].Amount;
            if ((index != 0) || (this.m_insertedItemUseLevel <= 0f))
            {
                return amount;
            }
            return (amount - 1);
        }

        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
            MyCraftingComponentBlockDefinition definition2 = definition as MyCraftingComponentBlockDefinition;
            if (definition2 != null)
            {
                base.m_craftingSpeedMultiplier = definition2.CraftingSpeedMultiplier;
                foreach (string str in definition2.AvailableBlueprintClasses)
                {
                    MyBlueprintClassDefinition blueprintClass = MyDefinitionManager.Static.GetBlueprintClass(str);
                    if (blueprintClass != null)
                    {
                        base.m_blueprintClasses.Add(blueprintClass);
                    }
                }
                foreach (MyDefinitionId id in definition2.AcceptedOperatingItems)
                {
                    this.m_acceptedOperatingItems.Add(id);
                }
            }
        }

        private void InitBlock()
        {
            MyCubeBlock entity = base.Entity as MyCubeBlock;
            MyInventory inventory = null;
            entity.Components.ComponentAdded -= new Action<System.Type, MyEntityComponentBase>(this.OnNewComponentAdded);
            if (entity != null)
            {
                if (entity.InventoryCount == 0)
                {
                    entity.Components.ComponentAdded += new Action<System.Type, MyEntityComponentBase>(this.OnNewComponentAdded);
                    return;
                }
                inventory = entity.GetInventory(0);
                inventory.SetFlags(MyInventoryFlags.CanSend | MyInventoryFlags.CanReceive);
                MyInventoryConstraint constraint = new MyInventoryConstraint("Crafting constraints", null, true);
                using (List<MyBlueprintClassDefinition>.Enumerator enumerator = base.m_blueprintClasses.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        IEnumerator<MyBlueprintDefinitionBase> enumerator2 = enumerator.Current.GetEnumerator();
                        try
                        {
                            while (enumerator2.MoveNext())
                            {
                                MyBlueprintDefinitionBase current = enumerator2.Current;
                                MyBlueprintDefinitionBase.Item[] results = current.Results;
                                int index = 0;
                                while (true)
                                {
                                    if (index >= results.Length)
                                    {
                                        results = current.Prerequisites;
                                        index = 0;
                                        while (index < results.Length)
                                        {
                                            MyBlueprintDefinitionBase.Item item2 = results[index];
                                            constraint.Add(item2.Id);
                                            index++;
                                        }
                                        break;
                                    }
                                    MyBlueprintDefinitionBase.Item item = results[index];
                                    constraint.Add(item.Id);
                                    index++;
                                }
                            }
                        }
                        finally
                        {
                            if (enumerator2 == null)
                            {
                                continue;
                            }
                            enumerator2.Dispose();
                        }
                    }
                }
                inventory.Constraint = constraint;
            }
            this.UpdateBlock();
        }

        protected override void InsertOperatingItem_Implementation(MyPhysicalInventoryItem item)
        {
            if ((this.AcceptsOperatingItems && this.IsOperatingItem(item)) && (((float) item.Amount) <= this.AvailableOperatingSpace))
            {
                base.InsertOperatingItem_Implementation(item);
                int index = this.m_insertedItems.FindIndex(x => x.Content.GetId() == item.Content.GetId());
                if (!this.m_insertedItems.IsValidIndex<MyPhysicalInventoryItem>(index))
                {
                    this.m_insertedItems.Add(item);
                    this.m_currentInsertedItemsCount += (float) item.Amount;
                }
                else
                {
                    this.m_currentInsertedItemsCount += (float) item.Amount;
                    item.Amount += this.m_insertedItems[index].Amount;
                    this.m_insertedItems[index] = item;
                }
                this.UpdateOperatingLevel();
                this.UpdateBlock();
            }
        }

        public override bool IsOperatingItem(MyPhysicalInventoryItem item)
        {
            MyDefinitionId objectId = item.Content.GetObjectId();
            return this.m_acceptedOperatingItems.Contains(objectId);
        }

        public override bool IsSerialized() => 
            true;

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.InitBlock();
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            base.Entity.Components.ComponentAdded -= new Action<System.Type, MyEntityComponentBase>(this.OnNewComponentAdded);
        }

        public void OnBlockEnabledChanged(MyCubeBlock obj)
        {
            if (!this.IsBlockEnabled)
            {
                this.m_paused = true;
            }
            else
            {
                this.m_paused = false;
                this.m_operatingItemTimerMs = this.m_operatingItemLastTimeMs;
                this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                base.m_elapsedTimeMs = 0;
                this.UpdateOperatingLevel();
                this.UpdateBlock();
            }
        }

        private void OnNewComponentAdded(System.Type type, MyEntityComponentBase component)
        {
            if (component is MyInventory)
            {
                this.InitBlock();
            }
        }

        protected override void RemoveOperatingItem_Implementation(MyPhysicalInventoryItem item, MyFixedPoint amount)
        {
            int index = this.m_insertedItems.FindIndex(x => x.Content.GetId() == item.Content.GetId());
            if (this.m_insertedItems.IsValidIndex<MyPhysicalInventoryItem>(index))
            {
                base.RemoveOperatingItem_Implementation(item, amount);
                MyFixedPoint point = MyFixedPoint.Min(amount, this.GetOperatingItemRemovableAmount(item));
                if (point > 0)
                {
                    item.Amount -= point;
                    this.m_currentInsertedItemsCount -= (float) point;
                    if (item.Amount > 0)
                    {
                        this.m_insertedItems[index] = item;
                    }
                    else
                    {
                        this.m_insertedItems.RemoveAt(index);
                    }
                    this.m_insertedItemUseLevel = 0f;
                    this.UpdateOperatingLevel();
                    this.UpdateBlock();
                    base.RaiseEvent_OperatingChanged();
                }
            }
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_CraftingComponentBlock block = base.Serialize(false) as MyObjectBuilder_CraftingComponentBlock;
            block.InsertedItemUseLevel = this.m_insertedItemUseLevel;
            foreach (MyPhysicalInventoryItem item in this.m_insertedItems)
            {
                block.InsertedItems.Add(item.GetObjectBuilder());
            }
            return block;
        }

        protected override void StartProduction_Implementation()
        {
            this.m_paused = false;
            this.TurnBlockOn();
        }

        protected override void StopOperating_Implementation()
        {
            this.m_insertedItems.Clear();
            this.m_currentInsertedItemsCount = 0f;
            this.m_operatingItemsLevel = 0f;
            this.m_insertedItemUseLevel = 0f;
            base.RaiseEvent_OperatingChanged();
        }

        protected override void StopProduction_Implementation()
        {
            base.StopProduction_Implementation();
            this.TurnBlockOff();
        }

        private void TurnBlockOff()
        {
            Action<MyCraftingComponentBlock, MyCubeBlock> onBlockTurnedOff = this.OnBlockTurnedOff;
            if (onBlockTurnedOff != null)
            {
                onBlockTurnedOff(this, this.Block);
            }
        }

        private void TurnBlockOn()
        {
            Action<MyCraftingComponentBlock, MyCubeBlock> onBlockTurnedOn = this.OnBlockTurnedOn;
            if (onBlockTurnedOn != null)
            {
                onBlockTurnedOn(this, this.Block);
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            base.m_elapsedTimeMs = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastUpdateTime;
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (!this.m_paused)
            {
                this.UpdateOperatingLevel();
                this.UpdateBlock();
                if ((!base.IsProductionDone && this.CanOperate) && this.IsBlockEnabled)
                {
                    this.UpdateProduction_Implementation();
                }
            }
        }

        private void UpdateBlock()
        {
            if (this.Block != null)
            {
                if ((this.CanOperate && !this.IsBlockEnabled) && !base.IsProductionDone)
                {
                    this.TurnBlockOn();
                    base.RaiseEvent_OperatingChanged();
                }
                else if (!this.CanOperate && this.IsBlockEnabled)
                {
                    this.TurnBlockOff();
                    base.RaiseEvent_OperatingChanged();
                }
                if (this.IsBlockEnabled)
                {
                    MyCubeBlock block = this.Block;
                    block.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
                }
                else
                {
                    MyCubeBlock block = this.Block;
                    block.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_100TH_FRAME;
                }
            }
        }

        protected override void UpdateOperatingLevel()
        {
            if (!this.m_updatingOperatingLevel)
            {
                this.m_updatingOperatingLevel = true;
                float num = 0f;
                foreach (MyPhysicalInventoryItem item in this.m_insertedItems)
                {
                    num += (float) item.Amount;
                }
                if (this.IsBlockEnabled && (num > 0f))
                {
                    this.m_insertedItemUseLevel += ((float) base.m_elapsedTimeMs) / ((float) this.m_operatingItemLastTimeMs);
                    this.m_insertedItemUseLevel = Math.Min(this.m_insertedItemUseLevel, 1f);
                }
                this.m_operatingItemsLevel = Math.Max((float) 0f, (float) ((num - this.m_insertedItemUseLevel) / this.m_maxInsertedItems));
                if (this.m_insertedItemUseLevel < 1f)
                {
                    if (Math.Abs((float) (this.m_operatingItemsLevel - this.m_lastOperatingLevel)) > 0.01f)
                    {
                        base.RaiseEvent_OperatingChanged();
                        this.m_lastOperatingLevel = this.m_operatingItemsLevel;
                    }
                }
                else
                {
                    this.m_operatingItemsLevel = Math.Max((float) 0f, (float) ((num - 1f) / this.m_maxInsertedItems));
                    this.m_lastOperatingLevel = this.m_operatingItemsLevel;
                    if (Sync.IsServer)
                    {
                        if (num == 1f)
                        {
                            base.StopOperating();
                        }
                        else if (num > 1f)
                        {
                            base.RemoveOperatingItem(this.m_insertedItems.First<MyPhysicalInventoryItem>(), 1);
                        }
                    }
                }
                this.m_updatingOperatingLevel = false;
            }
        }

        protected override void UpdateProduction_Implementation()
        {
            if (this.CanOperate && this.IsBlockEnabled)
            {
                if (base.IsProducing)
                {
                    base.UpdateCurrentItem();
                }
                else if (!base.IsProductionDone)
                {
                    base.SelectItemToProduction();
                    if (base.m_currentItem != -1)
                    {
                        base.UpdateCurrentItem();
                    }
                }
            }
        }

        public override string ComponentTypeDebugString =>
            "Block crafting component";

        public override string DisplayNameText =>
            (!(base.Entity is MyCubeBlock) ? string.Empty : (base.Entity as MyCubeBlock).DisplayNameText);

        public override bool RequiresItemsToOperate =>
            this.m_requiresItemsToOperate;

        public override bool CanOperate =>
            (!MySession.Static.CreativeMode ? (this.m_operatingItemsLevel > 0f) : true);

        public override string OperatingItemsDisplayNameText =>
            this.m_operatingItemsDisplayNameText;

        public override float OperatingItemsLevel =>
            this.m_operatingItemsLevel;

        public MyCubeBlock Block =>
            (base.Entity as MyCubeBlock);

        public bool IsBlockEnabled
        {
            get => 
                this.m_blockEnabled;
            set => 
                (this.m_blockEnabled = value);
        }

        public override bool AcceptsOperatingItems =>
            (this.m_currentInsertedItemsCount < this.m_maxInsertedItems);

        public override float AvailableOperatingSpace =>
            (this.m_maxInsertedItems - this.m_currentInsertedItemsCount);
    }
}

