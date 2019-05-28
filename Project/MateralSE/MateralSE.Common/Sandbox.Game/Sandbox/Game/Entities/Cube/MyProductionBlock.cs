namespace Sandbox.Game.Entities.Cube
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyProductionBlock), typeof(Sandbox.ModAPI.Ingame.IMyProductionBlock) })]
    public abstract class MyProductionBlock : MyFunctionalBlock, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyProductionBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyProductionBlock, IMyInventoryOwner
    {
        protected MySoundPair m_processSound = new MySoundPair();
        protected List<QueueItem> m_queue;
        private MyInventoryAggregate m_inventoryAggregate;
        private MyInventory m_inputInventory;
        private MyInventory m_outputInventory;
        private int m_lastUpdateTime;
        private bool m_isProducing;
        protected QueueItem? m_currentQueueItem;
        protected static Dictionary<MyDefinitionId, MyFixedPoint> m_tmpInventoryCounts = new Dictionary<MyDefinitionId, MyFixedPoint>();
        [CompilerGenerated]
        private Action StartedProducing;
        [CompilerGenerated]
        private Action StoppedProducing;
        [CompilerGenerated]
        private Action<MyProductionBlock> QueueChanged;
        private string m_string;
        protected readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_useConveyorSystem;
        private IMyConveyorEndpoint m_multilineConveyorEndpoint;
        private uint m_nextItemId;

        public event Action<MyProductionBlock> QueueChanged
        {
            [CompilerGenerated] add
            {
                Action<MyProductionBlock> queueChanged = this.QueueChanged;
                while (true)
                {
                    Action<MyProductionBlock> a = queueChanged;
                    Action<MyProductionBlock> action3 = (Action<MyProductionBlock>) Delegate.Combine(a, value);
                    queueChanged = Interlocked.CompareExchange<Action<MyProductionBlock>>(ref this.QueueChanged, action3, a);
                    if (ReferenceEquals(queueChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyProductionBlock> queueChanged = this.QueueChanged;
                while (true)
                {
                    Action<MyProductionBlock> source = queueChanged;
                    Action<MyProductionBlock> action3 = (Action<MyProductionBlock>) Delegate.Remove(source, value);
                    queueChanged = Interlocked.CompareExchange<Action<MyProductionBlock>>(ref this.QueueChanged, action3, source);
                    if (ReferenceEquals(queueChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action StartedProducing
        {
            [CompilerGenerated] add
            {
                Action startedProducing = this.StartedProducing;
                while (true)
                {
                    Action a = startedProducing;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    startedProducing = Interlocked.CompareExchange<Action>(ref this.StartedProducing, action3, a);
                    if (ReferenceEquals(startedProducing, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action startedProducing = this.StartedProducing;
                while (true)
                {
                    Action source = startedProducing;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    startedProducing = Interlocked.CompareExchange<Action>(ref this.StartedProducing, action3, source);
                    if (ReferenceEquals(startedProducing, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action StoppedProducing
        {
            [CompilerGenerated] add
            {
                Action stoppedProducing = this.StoppedProducing;
                while (true)
                {
                    Action a = stoppedProducing;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    stoppedProducing = Interlocked.CompareExchange<Action>(ref this.StoppedProducing, action3, a);
                    if (ReferenceEquals(stoppedProducing, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action stoppedProducing = this.StoppedProducing;
                while (true)
                {
                    Action source = stoppedProducing;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    stoppedProducing = Interlocked.CompareExchange<Action>(ref this.StoppedProducing, action3, source);
                    if (ReferenceEquals(stoppedProducing, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyProductionBlock()
        {
            this.CreateTerminalControls();
            base.m_soundEmitter = new MyEntity3DSoundEmitter(this, true, 1f);
            this.m_queue = new List<QueueItem>();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            this.IsProducing = false;
            base.Components.ComponentAdded += new Action<Type, MyEntityComponentBase>(this.OnComponentAdded);
        }

        public void AddQueueItemRequest(MyBlueprintDefinitionBase blueprint, MyFixedPoint ammount, int idx = -1)
        {
            SerializableDefinitionId id = (SerializableDefinitionId) blueprint.Id;
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProductionBlock, int, SerializableDefinitionId, MyFixedPoint>(this, x => new Action<int, SerializableDefinitionId, MyFixedPoint>(x.OnAddQueueItemRequest), idx, id, ammount, targetEndpoint);
        }

        public virtual bool AllowSelfPulling() => 
            false;

        public bool CanUseBlueprint(MyBlueprintDefinitionBase blueprint)
        {
            using (List<MyBlueprintClassDefinition>.Enumerator enumerator = this.ProductionBlockDefinition.BlueprintClasses.GetEnumerator())
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

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        public void ClearQueue(bool sendEvent = true)
        {
            if (Sync.IsServer)
            {
                this.ClearQueueRequest();
                if (sendEvent)
                {
                    this.OnQueueChanged();
                }
            }
        }

        [Event(null, 0x1a0), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        protected void ClearQueueRequest()
        {
            for (int i = this.m_queue.Count - 1; i >= 0; i--)
            {
                if (this.RemoveQueueItemTests(i))
                {
                    MyFixedPoint point = -1;
                    EndpointId targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProductionBlock, int, MyFixedPoint, float>(this, x => new Action<int, MyFixedPoint, float>(x.OnRemoveQueueItem), i, point, 0f, targetEndpoint);
                }
            }
        }

        protected override void Closing()
        {
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.StopSound(true, true);
            }
            base.Closing();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        private float ComputeRequiredPower()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            if (!this.IsProducing || this.IsQueueEmpty)
            {
                return this.ProductionBlockDefinition.StandbyPowerConsumption;
            }
            return this.GetOperationalPowerConsumption();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyProductionBlock>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyProductionBlock> switch1 = new MyTerminalControlOnOffSwitch<MyProductionBlock>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MyProductionBlock> switch2 = new MyTerminalControlOnOffSwitch<MyProductionBlock>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                switch2.Getter = x => x.UseConveyorSystem;
                MyTerminalControlOnOffSwitch<MyProductionBlock> local4 = switch2;
                MyTerminalControlOnOffSwitch<MyProductionBlock> local5 = switch2;
                local5.Setter = (x, v) => x.UseConveyorSystem = v;
                MyTerminalControlOnOffSwitch<MyProductionBlock> onOff = local5;
                onOff.EnableToggleAction<MyProductionBlock>();
                MyTerminalControlFactory.AddControl<MyProductionBlock>(onOff);
            }
        }

        private void CubeBlock_IsWorkingChanged(MyCubeBlock block)
        {
            if (base.IsWorking && this.IsProducing)
            {
                this.OnStartProducing();
            }
        }

        private unsafe QueueItem DeserializeQueueItem(MyObjectBuilder_ProductionBlock.QueueItem itemOb)
        {
            QueueItem* itemPtr1;
            QueueItem item = new QueueItem {
                Amount = itemOb.Amount,
                Blueprint = !MyDefinitionManager.Static.HasBlueprint(itemOb.Id) ? MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(itemOb.Id) : MyDefinitionManager.Static.GetBlueprintDefinition(itemOb.Id)
            };
            itemPtr1->ItemId = (itemOb.ItemId != null) ? itemOb.ItemId.Value : this.NextItemId;
            itemPtr1 = (QueueItem*) ref item;
            return item;
        }

        public void FixInputOutputInventories(MyInventoryConstraint inputInventoryConstraint, MyInventoryConstraint outputInventoryConstraint)
        {
            if (this.m_inventoryAggregate.InventoryCount != 2)
            {
                MyInventoryAggregate component = MyInventoryAggregate.FixInputOutputInventories(this.m_inventoryAggregate, inputInventoryConstraint, outputInventoryConstraint);
                base.Components.Remove<MyInventoryBase>();
                this.m_outputInventory = null;
                this.m_inputInventory = null;
                base.Components.Add<MyInventoryBase>(component);
            }
        }

        public override MyInventoryBase GetInventoryBase(int index = 0)
        {
            if (index == 0)
            {
                return this.InputInventory;
            }
            if (index != 1)
            {
                throw new InvalidBranchException();
            }
            return this.OutputInventory;
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_ProductionBlock objectBuilderCubeBlock = (MyObjectBuilder_ProductionBlock) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.InputInventory = this.InputInventory.GetObjectBuilder();
            objectBuilderCubeBlock.OutputInventory = this.OutputInventory.GetObjectBuilder();
            objectBuilderCubeBlock.UseConveyorSystem = (bool) this.m_useConveyorSystem;
            objectBuilderCubeBlock.NextItemId = this.m_nextItemId;
            if (this.m_queue.Count <= 0)
            {
                objectBuilderCubeBlock.Queue = null;
            }
            else
            {
                objectBuilderCubeBlock.Queue = new MyObjectBuilder_ProductionBlock.QueueItem[this.m_queue.Count];
                for (int i = 0; i < this.m_queue.Count; i++)
                {
                    objectBuilderCubeBlock.Queue[i].Id = (SerializableDefinitionId) this.m_queue[i].Blueprint.Id;
                    objectBuilderCubeBlock.Queue[i].Amount = this.m_queue[i].Amount;
                    objectBuilderCubeBlock.Queue[i].ItemId = new uint?(this.m_queue[i].ItemId);
                }
            }
            return objectBuilderCubeBlock;
        }

        protected virtual float GetOperationalPowerConsumption() => 
            this.ProductionBlockDefinition.OperationalPowerConsumption;

        public virtual PullInformation GetPullInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.OwnerID = base.OwnerId;
            information1.Inventory = this.InputInventory;
            information1.Constraint = this.InputInventory.Constraint;
            return information1;
        }

        public virtual PullInformation GetPushInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.OwnerID = base.OwnerId;
            information1.Inventory = this.OutputInventory;
            information1.Constraint = this.OutputInventory.Constraint;
            return information1;
        }

        public QueueItem GetQueueItem(int idx) => 
            this.m_queue[idx];

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.ProductionBlockDefinition.ResourceSinkGroup, this.ProductionBlockDefinition.OperationalPowerConsumption, new Func<float>(this.ComputeRequiredPower));
            component.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink = component;
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_ProductionBlock block = (MyObjectBuilder_ProductionBlock) objectBuilder;
            if (this.InventoryAggregate == null)
            {
                this.InventoryAggregate = new MyInventoryAggregate();
            }
            if (this.InputInventory == null)
            {
                this.InputInventory = new MyInventory(this.ProductionBlockDefinition.InventoryMaxVolume, this.ProductionBlockDefinition.InventorySize, MyInventoryFlags.CanReceive);
                if (block.InputInventory != null)
                {
                    this.InputInventory.Init(block.InputInventory);
                }
            }
            if (this.OutputInventory == null)
            {
                this.OutputInventory = new MyInventory(this.ProductionBlockDefinition.InventoryMaxVolume, this.ProductionBlockDefinition.InventorySize, MyInventoryFlags.CanSend);
                if (block.OutputInventory != null)
                {
                    this.OutputInventory.Init(block.OutputInventory);
                }
            }
            this.m_nextItemId = block.NextItemId;
            uint nextItemId = this.m_nextItemId;
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.CubeBlock_IsWorkingChanged);
            base.ResourceSink.Update();
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            if (block.Queue != null)
            {
                this.m_queue.Clear();
                if (this.m_queue.Capacity < block.Queue.Length)
                {
                    this.m_queue.Capacity = block.Queue.Length;
                }
                int index = 0;
                while (true)
                {
                    if (index >= block.Queue.Length)
                    {
                        this.UpdatePower();
                        break;
                    }
                    MyObjectBuilder_ProductionBlock.QueueItem itemOb = block.Queue[index];
                    QueueItem item = this.DeserializeQueueItem(itemOb);
                    if (item.Blueprint != null)
                    {
                        this.m_queue.Add(item);
                    }
                    else
                    {
                        MySandboxGame.Log.WriteLine($"Could not add item into production block's queue: Blueprint {itemOb.Id} was not found.");
                    }
                    index++;
                }
            }
            this.m_useConveyorSystem.SetLocalValue(block.UseConveyorSystem);
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_multilineConveyorEndpoint = new MyMultilineConveyorEndpoint(this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_multilineConveyorEndpoint));
        }

        protected void InitializeInventoryCounts(bool inputInventory = true)
        {
            m_tmpInventoryCounts.Clear();
            foreach (MyPhysicalInventoryItem item in inputInventory ? this.InputInventory.GetItems() : this.OutputInventory.GetItems())
            {
                MyFixedPoint point = 0;
                MyDefinitionId key = new MyDefinitionId(item.Content.TypeId, item.Content.SubtypeId);
                m_tmpInventoryCounts.TryGetValue(key, out point);
                m_tmpInventoryCounts[key] = point + item.Amount;
            }
        }

        protected virtual unsafe void InsertQueueItem(int idx, MyBlueprintDefinitionBase blueprint, MyFixedPoint amount)
        {
            if (this.CanUseBlueprint(blueprint))
            {
                QueueItem item = new QueueItem {
                    Amount = amount,
                    Blueprint = blueprint
                };
                if (this.m_queue.IsValidIndex<QueueItem>(idx) && ReferenceEquals(this.m_queue[idx].Blueprint, item.Blueprint))
                {
                    MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref item.Amount;
                    pointPtr1[0] += this.m_queue[idx].Amount;
                    item.ItemId = this.m_queue[idx].ItemId;
                    if ((this.m_currentQueueItem != null) && (this.m_queue[idx].ItemId == this.m_currentQueueItem.Value.ItemId))
                    {
                        this.m_currentQueueItem = new QueueItem?(item);
                    }
                    this.m_queue[idx] = item;
                }
                else if (((this.m_queue.Count > 0) && ((idx >= this.m_queue.Count) || (idx == -1))) && ReferenceEquals(this.m_queue[this.m_queue.Count - 1].Blueprint, item.Blueprint))
                {
                    MyFixedPoint* pointPtr2 = (MyFixedPoint*) ref item.Amount;
                    pointPtr2[0] += this.m_queue[this.m_queue.Count - 1].Amount;
                    item.ItemId = this.m_queue[this.m_queue.Count - 1].ItemId;
                    if ((this.m_currentQueueItem != null) && (this.m_queue[this.m_queue.Count - 1].ItemId == this.m_currentQueueItem.Value.ItemId))
                    {
                        this.m_currentQueueItem = new QueueItem?(item);
                    }
                    this.m_queue[this.m_queue.Count - 1] = item;
                }
                else
                {
                    if (idx == -1)
                    {
                        idx = this.m_queue.Count;
                    }
                    if (idx > this.m_queue.Count)
                    {
                        MyLog.Default.WriteLine("Production block.InsertQueueItem: Index out of bounds, desync!");
                        idx = this.m_queue.Count;
                    }
                    if (this.m_queue.Count == 0)
                    {
                        this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                    }
                    item.ItemId = this.NextItemId;
                    this.m_queue.Insert(idx, item);
                }
                this.UpdatePower();
                this.OnQueueChanged();
            }
        }

        public void InsertQueueItemRequest(int idx, MyBlueprintDefinitionBase blueprint)
        {
            this.InsertQueueItemRequest(idx, blueprint, 1);
        }

        public void InsertQueueItemRequest(int idx, MyBlueprintDefinitionBase blueprint, MyFixedPoint amount)
        {
            this.AddQueueItemRequest(blueprint, amount, idx);
        }

        protected virtual unsafe void MoveQueueItem(uint queueItemId, int targetIdx)
        {
            int index = 0;
            while (true)
            {
                if (index < this.m_queue.Count)
                {
                    if (this.m_queue[index].ItemId != queueItemId)
                    {
                        index++;
                        continue;
                    }
                    QueueItem item = this.m_queue[index];
                    int num1 = Math.Min(this.m_queue.Count - 1, targetIdx);
                    targetIdx = num1;
                    if (index == targetIdx)
                    {
                        return;
                    }
                    this.m_queue.RemoveAt(index);
                    int num2 = -1;
                    if (this.m_queue.IsValidIndex<QueueItem>((targetIdx - 1)) && ReferenceEquals(this.m_queue[targetIdx - 1].Blueprint, item.Blueprint))
                    {
                        num2 = targetIdx - 1;
                    }
                    if (this.m_queue.IsValidIndex<QueueItem>(targetIdx) && ReferenceEquals(this.m_queue[targetIdx].Blueprint, item.Blueprint))
                    {
                        num2 = targetIdx;
                    }
                    if (num2 == -1)
                    {
                        this.m_queue.Insert(targetIdx, item);
                    }
                    else
                    {
                        QueueItem item2 = this.m_queue[num2];
                        MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref item2.Amount;
                        pointPtr1[0] += item.Amount;
                        this.m_queue[num2] = item2;
                    }
                }
                this.OnQueueChanged();
                return;
            }
        }

        public void MoveQueueItemRequest(uint srcItemId, int dstIdx)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProductionBlock, uint, int>(this, x => new Action<uint, int>(x.OnMoveQueueItemCallback), srcItemId, dstIdx, targetEndpoint);
        }

        [Event(null, 380), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void OnAddQueueItemRequest(int idx, SerializableDefinitionId defId, MyFixedPoint ammount)
        {
            MyBlueprintDefinitionBase blueprintDefinition = MyDefinitionManager.Static.GetBlueprintDefinition(defId);
            if (blueprintDefinition != null)
            {
                this.InsertQueueItem(idx, blueprintDefinition, ammount);
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProductionBlock, int, SerializableDefinitionId, MyFixedPoint>(this, x => new Action<int, SerializableDefinitionId, MyFixedPoint>(x.OnAddQueueItemSuccess), idx, defId, ammount, targetEndpoint);
            }
        }

        [Event(null, 0x189), Reliable, Broadcast]
        private void OnAddQueueItemSuccess(int idx, SerializableDefinitionId defId, MyFixedPoint ammount)
        {
            this.InsertQueueItem(idx, MyDefinitionManager.Static.GetBlueprintDefinition(defId), ammount);
        }

        protected virtual void OnBeforeInventoryRemovedFromAggregate(MyInventoryAggregate aggregate, MyInventoryBase inventory)
        {
            if (ReferenceEquals(inventory, this.m_inputInventory))
            {
                this.m_inputInventory = null;
            }
            else if (ReferenceEquals(inventory, this.m_outputInventory))
            {
                this.m_outputInventory = null;
            }
        }

        private void OnComponentAdded(Type type, MyEntityComponentBase component)
        {
            MyInventoryAggregate aggregate = component as MyInventoryAggregate;
            if (aggregate != null)
            {
                this.m_inventoryAggregate = aggregate;
                this.m_inventoryAggregate.BeforeRemovedFromContainer += new Action<MyEntityComponentBase>(this.OnInventoryAggregateRemoved);
                this.m_inventoryAggregate.OnAfterComponentAdd += new Action<MyInventoryAggregate, MyInventoryBase>(this.OnInventoryAddedToAggregate);
                this.m_inventoryAggregate.OnBeforeComponentRemove += new Action<MyInventoryAggregate, MyInventoryBase>(this.OnBeforeInventoryRemovedFromAggregate);
                foreach (MyInventory inventory in this.m_inventoryAggregate.ChildList.Reader)
                {
                    this.OnInventoryAddedToAggregate(aggregate, inventory);
                }
            }
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.InputInventory, true);
            base.ReleaseInventory(this.OutputInventory, true);
            base.OnDestroy();
        }

        protected override void OnEnabledChanged()
        {
            this.UpdatePower();
            base.OnEnabledChanged();
            if (base.IsWorking && this.IsProducing)
            {
                this.OnStartProducing();
            }
        }

        protected virtual void OnInventoryAddedToAggregate(MyInventoryAggregate aggregate, MyInventoryBase inventory)
        {
            if (this.m_inputInventory == null)
            {
                this.m_inputInventory = inventory as MyInventory;
            }
            else if (this.m_outputInventory == null)
            {
                this.m_outputInventory = inventory as MyInventory;
            }
        }

        private void OnInventoryAggregateRemoved(MyEntityComponentBase component)
        {
            this.m_inputInventory = null;
            this.m_outputInventory = null;
            this.m_inventoryAggregate.BeforeRemovedFromContainer -= new Action<MyEntityComponentBase>(this.OnInventoryAggregateRemoved);
            this.m_inventoryAggregate.OnAfterComponentAdd -= new Action<MyInventoryAggregate, MyInventoryBase>(this.OnInventoryAddedToAggregate);
            this.m_inventoryAggregate.OnBeforeComponentRemove -= new Action<MyInventoryAggregate, MyInventoryBase>(this.OnBeforeInventoryRemovedFromAggregate);
            this.m_inventoryAggregate = null;
        }

        [Event(null, 410), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnMoveQueueItemCallback(uint srcItemId, int dstIdx)
        {
            this.MoveQueueItem(srcItemId, dstIdx);
        }

        protected virtual void OnQueueChanged()
        {
            if (this.QueueChanged != null)
            {
                this.QueueChanged(this);
            }
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.InputInventory, false);
            base.ReleaseInventory(this.OutputInventory, false);
            base.OnRemovedByCubeBuilder();
        }

        [Event(null, 0x1cf), Reliable, ServerInvoked, Broadcast]
        private void OnRemoveQueueItem(int idx, MyFixedPoint amount, float progress)
        {
            if (amount >= 0)
            {
                this.RemoveFirstQueueItem(idx, amount, progress);
            }
            else
            {
                this.RemoveQueueItem(idx);
            }
        }

        [Event(null, 440), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void OnRemoveQueueItemRequest(int idx, MyFixedPoint amount, float progress)
        {
            if (this.RemoveQueueItemTests(idx))
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProductionBlock, int, MyFixedPoint, float>(this, x => new Action<int, MyFixedPoint, float>(x.OnRemoveQueueItem), idx, amount, progress, targetEndpoint);
            }
        }

        protected void OnStartProducing()
        {
            if (base.m_soundEmitter != null)
            {
                bool? nullable = null;
                base.m_soundEmitter.PlaySound(this.m_processSound, true, false, false, false, false, nullable);
            }
            Action startedProducing = this.StartedProducing;
            if (startedProducing != null)
            {
                startedProducing();
            }
        }

        protected void OnStopProducing()
        {
            if (base.m_soundEmitter != null)
            {
                if (!base.IsWorking)
                {
                    base.m_soundEmitter.StopSound(false, true);
                }
                else
                {
                    base.m_soundEmitter.StopSound(false, true);
                    bool? nullable = null;
                    base.m_soundEmitter.PlaySound(base.m_baseIdleSound, false, true, false, false, false, nullable);
                }
            }
            Action stoppedProducing = this.StoppedProducing;
            if (stoppedProducing != null)
            {
                stoppedProducing();
            }
        }

        private void Receiver_IsPoweredChanged()
        {
            if (!base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                this.IsProducing = false;
            }
            base.UpdateIsWorking();
        }

        protected virtual unsafe void RemoveFirstQueueItem(int index, MyFixedPoint amount, float progress = 0f)
        {
            if (this.m_queue.IsValidIndex<QueueItem>(index))
            {
                QueueItem item = this.m_queue[index];
                MyFixedPoint point1 = MathHelper.Clamp(amount, 0, item.Amount);
                amount = point1;
                MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref item.Amount;
                pointPtr1[0] -= amount;
                this.m_queue[index] = item;
                if (item.Amount <= 0)
                {
                    MyAssembler assembler = this as MyAssembler;
                    if (assembler != null)
                    {
                        assembler.CurrentProgress = 0f;
                    }
                    this.m_queue.RemoveAt(index);
                }
                this.UpdatePower();
                this.OnQueueChanged();
            }
        }

        protected void RemoveFirstQueueItemAnnounce(MyFixedPoint amount, float progress = 0f)
        {
            this.RemoveQueueItemRequest(0, amount, progress);
        }

        protected virtual void RemoveQueueItem(int itemIdx)
        {
            if (itemIdx >= this.m_queue.Count)
            {
                MyLog.Default.WriteLine("Production block.RemoveQueueItem: Index out of bounds!");
            }
            else
            {
                this.m_queue.RemoveAt(itemIdx);
                this.UpdatePower();
                this.OnQueueChanged();
            }
        }

        public void RemoveQueueItemRequest(int idx, MyFixedPoint amount, float progress = 0f)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProductionBlock, int, MyFixedPoint, float>(this, x => new Action<int, MyFixedPoint, float>(x.OnRemoveQueueItemRequest), idx, amount, progress, targetEndpoint);
        }

        private bool RemoveQueueItemTests(int idx)
        {
            if (this.m_queue.IsValidIndex<QueueItem>(idx) || (idx == -1))
            {
                return true;
            }
            MySandboxGame.Log.WriteLine("Invalid queue index in the remove item message!");
            return false;
        }

        void Sandbox.ModAPI.IMyProductionBlock.AddQueueItem(MyDefinitionBase blueprint, MyFixedPoint amount)
        {
            this.AddQueueItemRequest(blueprint as MyBlueprintDefinition, amount, -1);
        }

        bool Sandbox.ModAPI.IMyProductionBlock.CanUseBlueprint(MyDefinitionBase blueprint) => 
            this.CanUseBlueprint(blueprint as MyBlueprintDefinition);

        List<MyProductionQueueItem> Sandbox.ModAPI.IMyProductionBlock.GetQueue()
        {
            List<MyProductionQueueItem> list = new List<MyProductionQueueItem>(this.m_queue.Count);
            foreach (QueueItem item in this.m_queue)
            {
                MyProductionQueueItem item2 = new MyProductionQueueItem {
                    Amount = item.Amount,
                    Blueprint = item.Blueprint,
                    ItemId = item.ItemId
                };
                list.Add(item2);
            }
            return list;
        }

        void Sandbox.ModAPI.IMyProductionBlock.InsertQueueItem(int idx, MyDefinitionBase blueprint, MyFixedPoint amount)
        {
            this.InsertQueueItemRequest(idx, blueprint as MyBlueprintDefinition, amount);
        }

        void Sandbox.ModAPI.Ingame.IMyProductionBlock.AddQueueItem(MyDefinitionId blueprint, decimal amount)
        {
            MyBlueprintDefinitionBase blueprintDefinition = MyDefinitionManager.Static.GetBlueprintDefinition(blueprint);
            this.AddQueueItemRequest(blueprintDefinition, (MyFixedPoint) amount, -1);
        }

        void Sandbox.ModAPI.Ingame.IMyProductionBlock.AddQueueItem(MyDefinitionId blueprint, double amount)
        {
            MyBlueprintDefinitionBase blueprintDefinition = MyDefinitionManager.Static.GetBlueprintDefinition(blueprint);
            this.AddQueueItemRequest(blueprintDefinition, (MyFixedPoint) amount, -1);
        }

        void Sandbox.ModAPI.Ingame.IMyProductionBlock.AddQueueItem(MyDefinitionId blueprint, MyFixedPoint amount)
        {
            MyBlueprintDefinitionBase blueprintDefinition = MyDefinitionManager.Static.GetBlueprintDefinition(blueprint);
            this.AddQueueItemRequest(blueprintDefinition, amount, -1);
        }

        bool Sandbox.ModAPI.Ingame.IMyProductionBlock.CanUseBlueprint(MyDefinitionId blueprint)
        {
            MyBlueprintDefinitionBase blueprintDefinition = MyDefinitionManager.Static.GetBlueprintDefinition(blueprint);
            return this.CanUseBlueprint(blueprintDefinition);
        }

        void Sandbox.ModAPI.Ingame.IMyProductionBlock.ClearQueue()
        {
            this.ClearQueueRequest();
        }

        void Sandbox.ModAPI.Ingame.IMyProductionBlock.GetQueue(List<MyProductionItem> items)
        {
            items.Clear();
            for (int i = 0; i < this.m_queue.Count; i++)
            {
                QueueItem item = this.m_queue[i];
                items.Add(new MyProductionItem(item.ItemId, item.Blueprint.Id, item.Amount));
            }
        }

        void Sandbox.ModAPI.Ingame.IMyProductionBlock.InsertQueueItem(int idx, MyDefinitionId blueprint, decimal amount)
        {
            MyBlueprintDefinitionBase blueprintDefinition = MyDefinitionManager.Static.GetBlueprintDefinition(blueprint);
            this.InsertQueueItemRequest(idx, blueprintDefinition, (MyFixedPoint) amount);
        }

        void Sandbox.ModAPI.Ingame.IMyProductionBlock.InsertQueueItem(int idx, MyDefinitionId blueprint, double amount)
        {
            MyBlueprintDefinitionBase blueprintDefinition = MyDefinitionManager.Static.GetBlueprintDefinition(blueprint);
            this.InsertQueueItemRequest(idx, blueprintDefinition, (MyFixedPoint) amount);
        }

        void Sandbox.ModAPI.Ingame.IMyProductionBlock.InsertQueueItem(int idx, MyDefinitionId blueprint, MyFixedPoint amount)
        {
            MyBlueprintDefinitionBase blueprintDefinition = MyDefinitionManager.Static.GetBlueprintDefinition(blueprint);
            this.InsertQueueItemRequest(idx, blueprintDefinition, amount);
        }

        void Sandbox.ModAPI.Ingame.IMyProductionBlock.RemoveQueueItem(int idx, decimal amount)
        {
            this.RemoveQueueItemRequest(idx, (MyFixedPoint) amount, 0f);
        }

        void Sandbox.ModAPI.Ingame.IMyProductionBlock.RemoveQueueItem(int idx, double amount)
        {
            this.RemoveQueueItemRequest(idx, (MyFixedPoint) amount, 0f);
        }

        void Sandbox.ModAPI.Ingame.IMyProductionBlock.RemoveQueueItem(int idx, MyFixedPoint amount)
        {
            this.RemoveQueueItemRequest(idx, amount, 0f);
        }

        protected void SwapQueue(ref List<QueueItem> otherQueue)
        {
            List<QueueItem> queue = this.m_queue;
            this.m_queue = otherQueue;
            otherQueue = queue;
            this.OnQueueChanged();
        }

        public QueueItem? TryGetFirstQueueItem() => 
            this.TryGetQueueItem(0);

        public QueueItem? TryGetQueueItem(int idx)
        {
            if (this.m_queue.IsValidIndex<QueueItem>(idx))
            {
                return new QueueItem?(this.m_queue[idx]);
            }
            return null;
        }

        public QueueItem? TryGetQueueItemById(uint itemId)
        {
            for (int i = 0; i < this.m_queue.Count; i++)
            {
                if (this.m_queue[i].ItemId == itemId)
                {
                    return new QueueItem?(this.m_queue[i]);
                }
            }
            return null;
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            this.UpdateProduction();
        }

        protected virtual void UpdatePower()
        {
            base.ResourceSink.Update();
        }

        public void UpdateProduction()
        {
            int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            this.UpdateProduction(totalGamePlayTimeInMilliseconds - this.m_lastUpdateTime);
            this.m_lastUpdateTime = totalGamePlayTimeInMilliseconds;
        }

        protected abstract void UpdateProduction(int timeDelta);
        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        protected MyProductionBlockDefinition ProductionBlockDefinition =>
            ((MyProductionBlockDefinition) base.BlockDefinition);

        public MyInventoryAggregate InventoryAggregate
        {
            get => 
                this.m_inventoryAggregate;
            set
            {
                if (value != null)
                {
                    base.Components.Remove<MyInventoryBase>();
                    base.Components.Add<MyInventoryBase>(value);
                }
            }
        }

        public MyInventory InputInventory
        {
            get => 
                this.m_inputInventory;
            protected set
            {
                if (!this.InventoryAggregate.ChildList.Contains(value))
                {
                    if (this.m_inputInventory != null)
                    {
                        this.InventoryAggregate.ChildList.RemoveComponent(this.m_inputInventory);
                    }
                    this.InventoryAggregate.AddComponent(value);
                }
            }
        }

        public MyInventory OutputInventory
        {
            get => 
                this.m_outputInventory;
            protected set
            {
                if (!this.InventoryAggregate.ChildList.Contains(value))
                {
                    if (this.m_outputInventory != null)
                    {
                        this.InventoryAggregate.ChildList.RemoveComponent(this.m_outputInventory);
                    }
                    this.InventoryAggregate.AddComponent(value);
                }
            }
        }

        public bool IsQueueEmpty =>
            (this.m_queue.Count == 0);

        public bool IsProducing
        {
            get => 
                this.m_isProducing;
            protected set
            {
                if (this.m_isProducing != value)
                {
                    this.m_isProducing = value;
                    if (value)
                    {
                        this.OnStartProducing();
                    }
                    else
                    {
                        this.OnStopProducing();
                    }
                    MySandboxGame.Static.Invoke(delegate {
                        if (!base.Closed)
                        {
                            this.UpdatePower();
                        }
                    }, "IsProducing");
                }
            }
        }

        public IEnumerable<QueueItem> Queue =>
            this.m_queue;

        public uint NextItemId
        {
            get
            {
                uint nextItemId = this.m_nextItemId;
                this.m_nextItemId = nextItemId + 1;
                return nextItemId;
            }
        }

        public bool UseConveyorSystem
        {
            get => 
                ((bool) this.m_useConveyorSystem);
            set => 
                (this.m_useConveyorSystem.Value = value);
        }

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_multilineConveyorEndpoint;

        int IMyInventoryOwner.InventoryCount =>
            base.InventoryCount;

        long IMyInventoryOwner.EntityId =>
            base.EntityId;

        bool IMyInventoryOwner.HasInventory =>
            base.HasInventory;

        bool IMyInventoryOwner.UseConveyorSystem
        {
            get => 
                this.UseConveyorSystem;
            set => 
                (this.UseConveyorSystem = value);
        }

        VRage.Game.ModAPI.IMyInventory Sandbox.ModAPI.IMyProductionBlock.InputInventory =>
            this.InputInventory;

        VRage.Game.ModAPI.IMyInventory Sandbox.ModAPI.IMyProductionBlock.OutputInventory =>
            this.OutputInventory;

        VRage.Game.ModAPI.Ingame.IMyInventory Sandbox.ModAPI.Ingame.IMyProductionBlock.InputInventory =>
            this.InputInventory;

        VRage.Game.ModAPI.Ingame.IMyInventory Sandbox.ModAPI.Ingame.IMyProductionBlock.OutputInventory =>
            this.OutputInventory;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyProductionBlock.<>c <>9 = new MyProductionBlock.<>c();
            public static MyTerminalValueControl<MyProductionBlock, bool>.GetterDelegate <>9__46_0;
            public static MyTerminalValueControl<MyProductionBlock, bool>.SetterDelegate <>9__46_1;
            public static Func<MyProductionBlock, Action<int, SerializableDefinitionId, MyFixedPoint>> <>9__52_0;
            public static Func<MyProductionBlock, Action<int, SerializableDefinitionId, MyFixedPoint>> <>9__53_0;
            public static Func<MyProductionBlock, Action<uint, int>> <>9__55_0;
            public static Func<MyProductionBlock, Action<int, MyFixedPoint, float>> <>9__57_0;
            public static Func<MyProductionBlock, Action<int, MyFixedPoint, float>> <>9__58_0;
            public static Func<MyProductionBlock, Action<int, MyFixedPoint, float>> <>9__59_0;

            internal Action<int, SerializableDefinitionId, MyFixedPoint> <AddQueueItemRequest>b__52_0(MyProductionBlock x) => 
                new Action<int, SerializableDefinitionId, MyFixedPoint>(x.OnAddQueueItemRequest);

            internal Action<int, MyFixedPoint, float> <ClearQueueRequest>b__57_0(MyProductionBlock x) => 
                new Action<int, MyFixedPoint, float>(x.OnRemoveQueueItem);

            internal bool <CreateTerminalControls>b__46_0(MyProductionBlock x) => 
                x.UseConveyorSystem;

            internal void <CreateTerminalControls>b__46_1(MyProductionBlock x, bool v)
            {
                x.UseConveyorSystem = v;
            }

            internal Action<uint, int> <MoveQueueItemRequest>b__55_0(MyProductionBlock x) => 
                new Action<uint, int>(x.OnMoveQueueItemCallback);

            internal Action<int, SerializableDefinitionId, MyFixedPoint> <OnAddQueueItemRequest>b__53_0(MyProductionBlock x) => 
                new Action<int, SerializableDefinitionId, MyFixedPoint>(x.OnAddQueueItemSuccess);

            internal Action<int, MyFixedPoint, float> <OnRemoveQueueItemRequest>b__59_0(MyProductionBlock x) => 
                new Action<int, MyFixedPoint, float>(x.OnRemoveQueueItem);

            internal Action<int, MyFixedPoint, float> <RemoveQueueItemRequest>b__58_0(MyProductionBlock x) => 
                new Action<int, MyFixedPoint, float>(x.OnRemoveQueueItemRequest);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct QueueItem
        {
            public MyFixedPoint Amount;
            public MyBlueprintDefinitionBase Blueprint;
            public uint ItemId;
        }
    }
}

