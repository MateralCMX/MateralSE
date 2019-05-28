namespace Sandbox.Game.Entities.Cube
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_Assembler)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyAssembler), typeof(Sandbox.ModAPI.Ingame.IMyAssembler) })]
    public class MyAssembler : MyProductionBlock, Sandbox.ModAPI.IMyAssembler, Sandbox.ModAPI.IMyProductionBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyProductionBlock, Sandbox.ModAPI.Ingame.IMyAssembler, IMyEventProxy, IMyEventOwner
    {
        private VRage.Game.Entity.MyEntity m_currentUser;
        private MyAssemblerDefinition m_assemblerDef;
        private float m_currentProgress;
        private StateEnum m_currentState;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_slave;
        private bool m_repeatDisassembleEnabled;
        private bool m_repeatAssembleEnabled;
        private bool m_disassembleEnabled;
        private readonly List<VRage.Game.Entity.MyEntity> m_inventoryOwners = new List<VRage.Game.Entity.MyEntity>();
        private List<MyTuple<MyFixedPoint, MyBlueprintDefinitionBase.Item>> m_requiredComponents;
        private const float TIME_IN_ADVANCE = 5f;
        private bool m_isProcessing;
        private bool m_soundStartedFromInventory;
        private List<MyProductionBlock.QueueItem> m_otherQueue;
        private List<MyAssembler> m_assemblers = new List<MyAssembler>();
        private int m_assemblerKeyCounter;
        private MyCubeGrid m_cubeGrid;
        private bool m_inventoryOwnersDirty = true;
        [CompilerGenerated]
        private Action<MyAssembler> CurrentProgressChanged;
        [CompilerGenerated]
        private Action<MyAssembler> CurrentStateChanged;
        [CompilerGenerated]
        private Action<MyAssembler> CurrentModeChanged;
        private static readonly List<IMyConveyorEndpoint> m_conveyorEndpoints = new List<IMyConveyorEndpoint>();
        private static MyAssembler m_assemblerForPathfinding;
        private static readonly Predicate<IMyConveyorEndpoint> m_vertexPredicate = new Predicate<IMyConveyorEndpoint>(MyAssembler.VertexRules);
        private static readonly Predicate<IMyConveyorEndpoint> m_edgePredicate = new Predicate<IMyConveyorEndpoint>(MyAssembler.EdgeRules);

        public event Action<MyAssembler> CurrentModeChanged
        {
            [CompilerGenerated] add
            {
                Action<MyAssembler> currentModeChanged = this.CurrentModeChanged;
                while (true)
                {
                    Action<MyAssembler> a = currentModeChanged;
                    Action<MyAssembler> action3 = (Action<MyAssembler>) Delegate.Combine(a, value);
                    currentModeChanged = Interlocked.CompareExchange<Action<MyAssembler>>(ref this.CurrentModeChanged, action3, a);
                    if (ReferenceEquals(currentModeChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyAssembler> currentModeChanged = this.CurrentModeChanged;
                while (true)
                {
                    Action<MyAssembler> source = currentModeChanged;
                    Action<MyAssembler> action3 = (Action<MyAssembler>) Delegate.Remove(source, value);
                    currentModeChanged = Interlocked.CompareExchange<Action<MyAssembler>>(ref this.CurrentModeChanged, action3, source);
                    if (ReferenceEquals(currentModeChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyAssembler> CurrentProgressChanged
        {
            [CompilerGenerated] add
            {
                Action<MyAssembler> currentProgressChanged = this.CurrentProgressChanged;
                while (true)
                {
                    Action<MyAssembler> a = currentProgressChanged;
                    Action<MyAssembler> action3 = (Action<MyAssembler>) Delegate.Combine(a, value);
                    currentProgressChanged = Interlocked.CompareExchange<Action<MyAssembler>>(ref this.CurrentProgressChanged, action3, a);
                    if (ReferenceEquals(currentProgressChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyAssembler> currentProgressChanged = this.CurrentProgressChanged;
                while (true)
                {
                    Action<MyAssembler> source = currentProgressChanged;
                    Action<MyAssembler> action3 = (Action<MyAssembler>) Delegate.Remove(source, value);
                    currentProgressChanged = Interlocked.CompareExchange<Action<MyAssembler>>(ref this.CurrentProgressChanged, action3, source);
                    if (ReferenceEquals(currentProgressChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyAssembler> CurrentStateChanged
        {
            [CompilerGenerated] add
            {
                Action<MyAssembler> currentStateChanged = this.CurrentStateChanged;
                while (true)
                {
                    Action<MyAssembler> a = currentStateChanged;
                    Action<MyAssembler> action3 = (Action<MyAssembler>) Delegate.Combine(a, value);
                    currentStateChanged = Interlocked.CompareExchange<Action<MyAssembler>>(ref this.CurrentStateChanged, action3, a);
                    if (ReferenceEquals(currentStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyAssembler> currentStateChanged = this.CurrentStateChanged;
                while (true)
                {
                    Action<MyAssembler> source = currentStateChanged;
                    Action<MyAssembler> action3 = (Action<MyAssembler>) Delegate.Remove(source, value);
                    currentStateChanged = Interlocked.CompareExchange<Action<MyAssembler>>(ref this.CurrentStateChanged, action3, source);
                    if (ReferenceEquals(currentStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<Sandbox.ModAPI.IMyAssembler> Sandbox.ModAPI.IMyAssembler.CurrentModeChanged
        {
            add
            {
                this.CurrentModeChanged += this.GetDelegate(value);
            }
            remove
            {
                this.CurrentModeChanged -= this.GetDelegate(value);
            }
        }

        event Action<Sandbox.ModAPI.IMyAssembler> Sandbox.ModAPI.IMyAssembler.CurrentProgressChanged
        {
            add
            {
                this.CurrentProgressChanged += this.GetDelegate(value);
            }
            remove
            {
                this.CurrentProgressChanged -= this.GetDelegate(value);
            }
        }

        event Action<Sandbox.ModAPI.IMyAssembler> Sandbox.ModAPI.IMyAssembler.CurrentStateChanged
        {
            add
            {
                this.CurrentStateChanged += this.GetDelegate(value);
            }
            remove
            {
                this.CurrentStateChanged -= this.GetDelegate(value);
            }
        }

        public MyAssembler()
        {
            this.CreateTerminalControls();
            this.m_otherQueue = new List<MyProductionBlock.QueueItem>();
            this.m_slave.ValueChanged += x => this.OnSlaveChanged();
        }

        private float calculateBlueprintProductionTime(MyBlueprintDefinitionBase currentBlueprint) => 
            ((currentBlueprint.BaseProductionTimeInSeconds * 1000f) / ((MySession.Static.AssemblerSpeedMultiplier * ((MyAssemblerDefinition) base.BlockDefinition).AssemblySpeed) + base.UpgradeValues["Productivity"]));

        public bool CheckConveyorResources(MyFixedPoint? amount, MyDefinitionId contentId)
        {
            using (List<VRage.Game.Entity.MyEntity>.Enumerator enumerator = this.m_inventoryOwners.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    VRage.Game.Entity.MyEntity current = enumerator.Current;
                    if (current != null)
                    {
                        VRage.Game.Entity.MyEntity thisEntity = current;
                        if ((thisEntity != null) && thisEntity.HasInventory)
                        {
                            MyInventoryFlags flags = thisEntity.GetInventory(0).GetFlags();
                            MyInventoryFlags flags2 = MyInventoryFlags.CanSend | MyInventoryFlags.CanReceive;
                            List<MyInventory> list = new List<MyInventory>();
                            int index = 0;
                            while (true)
                            {
                                if (index < thisEntity.InventoryCount)
                                {
                                    list.Add(thisEntity.GetInventory(index));
                                    index++;
                                    continue;
                                }
                                using (List<MyInventory>.Enumerator enumerator2 = list.GetEnumerator())
                                {
                                    while (true)
                                    {
                                        if (!enumerator2.MoveNext())
                                        {
                                            break;
                                        }
                                        if (enumerator2.Current.ContainItems(amount, contentId, MyItemFlags.None) && (((flags == flags2) || (flags == MyInventoryFlags.CanSend)) || ReferenceEquals(thisEntity, this)))
                                        {
                                            return true;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private StateEnum CheckInventory(MyBlueprintDefinitionBase blueprint)
        {
            MyFixedPoint amountMultiplier = (MyFixedPoint) (1f / this.GetEfficiencyMultiplierForBlueprint(blueprint));
            if (this.DisassembleEnabled)
            {
                if (!this.CheckInventoryCapacity(base.InputInventory, blueprint.Prerequisites, amountMultiplier))
                {
                    return StateEnum.InventoryFull;
                }
                if (!this.CheckInventoryContents(base.OutputInventory, blueprint.Results, 1))
                {
                    return StateEnum.MissingItems;
                }
            }
            else
            {
                if (!this.CheckInventoryCapacity(base.OutputInventory, blueprint.Results, 1))
                {
                    return StateEnum.InventoryFull;
                }
                if (!this.CheckInventoryContents(base.InputInventory, blueprint.Prerequisites, amountMultiplier))
                {
                    return StateEnum.MissingItems;
                }
            }
            return StateEnum.Ok;
        }

        private bool CheckInventoryCapacity(MyInventory inventory, MyBlueprintDefinitionBase.Item item, MyFixedPoint amountMultiplier) => 
            inventory.CanItemsBeAdded(item.Amount * amountMultiplier, item.Id);

        private bool CheckInventoryCapacity(MyInventory inventory, MyBlueprintDefinitionBase.Item[] items, MyFixedPoint amountMultiplier)
        {
            if (MySession.Static.CreativeMode)
            {
                return true;
            }
            MyFixedPoint point = 0;
            foreach (MyBlueprintDefinitionBase.Item item in items)
            {
                MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(item.Id);
                if (physicalItemDefinition != null)
                {
                    point += (((MyFixedPoint) physicalItemDefinition.Volume) * item.Amount) * amountMultiplier;
                }
            }
            return ((inventory.CurrentVolume + point) <= inventory.MaxVolume);
        }

        private bool CheckInventoryContents(MyInventory inventory, MyBlueprintDefinitionBase.Item item, MyFixedPoint amountMultiplier) => 
            inventory.ContainItems(new MyFixedPoint?(item.Amount * amountMultiplier), item.Id, MyItemFlags.None);

        private bool CheckInventoryContents(MyInventory inventory, MyBlueprintDefinitionBase.Item[] item, MyFixedPoint amountMultiplier)
        {
            for (int i = 0; i < item.Length; i++)
            {
                if (!inventory.ContainItems(new MyFixedPoint?(item[i].Amount * amountMultiplier), item[i].Id, MyItemFlags.None))
                {
                    return false;
                }
            }
            return true;
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyAssembler>())
            {
                base.CreateTerminalControls();
                MyStringId? on = null;
                on = null;
                MyTerminalControlCheckbox<MyAssembler> checkbox1 = new MyTerminalControlCheckbox<MyAssembler>("slaveMode", MySpaceTexts.Assembler_SlaveMode, MySpaceTexts.Assembler_SlaveMode, on, on);
                MyTerminalControlCheckbox<MyAssembler> checkbox2 = new MyTerminalControlCheckbox<MyAssembler>("slaveMode", MySpaceTexts.Assembler_SlaveMode, MySpaceTexts.Assembler_SlaveMode, on, on);
                checkbox2.Enabled = x => x.SupportsAdvancedFunctions;
                MyTerminalControlCheckbox<MyAssembler> local2 = checkbox2;
                local2.Visible = local2.Enabled;
                local2.Getter = x => x.IsSlave;
                MyTerminalControlCheckbox<MyAssembler> local6 = local2;
                MyTerminalControlCheckbox<MyAssembler> local7 = local2;
                local7.Setter = delegate (MyAssembler x, bool v) {
                    if (x.RepeatEnabled)
                    {
                        x.RequestRepeatEnabled(false);
                    }
                    x.IsSlave = v;
                };
                MyTerminalControlCheckbox<MyAssembler> checkbox = local7;
                checkbox.EnableAction<MyAssembler>(null);
                MyTerminalControlFactory.AddControl<MyAssembler>(checkbox);
            }
        }

        [Event(null, 860), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void DisassembleAllCallback()
        {
            this.DisassembleAllInOutput();
        }

        private void DisassembleAllInOutput()
        {
            base.ClearQueue(false);
            List<Tuple<MyBlueprintDefinitionBase, MyFixedPoint>> list = new List<Tuple<MyBlueprintDefinitionBase, MyFixedPoint>>();
            bool flag = true;
            foreach (MyPhysicalInventoryItem item in base.OutputInventory.GetItems())
            {
                MyBlueprintDefinitionBase base2 = MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(item.Content.GetId());
                if (base2 == null)
                {
                    flag = false;
                    list.Clear();
                    break;
                }
                list.Add(Tuple.Create<MyBlueprintDefinitionBase, MyFixedPoint>(base2, item.Amount));
            }
            if (flag)
            {
                foreach (Tuple<MyBlueprintDefinitionBase, MyFixedPoint> tuple2 in list)
                {
                    base.InsertQueueItemRequest(-1, tuple2.Item1, tuple2.Item2);
                }
            }
            else
            {
                base.InitializeInventoryCounts(false);
                for (int i = 0; i < this.m_assemblerDef.BlueprintClasses.Count; i++)
                {
                    foreach (MyBlueprintDefinitionBase base3 in this.m_assemblerDef.BlueprintClasses[i])
                    {
                        MyFixedPoint maxValue = MyFixedPoint.MaxValue;
                        MyBlueprintDefinitionBase.Item[] results = base3.Results;
                        int index = 0;
                        while (true)
                        {
                            MyFixedPoint point2;
                            if (index < results.Length)
                            {
                                MyBlueprintDefinitionBase.Item item2 = results[index];
                                point2 = 0;
                                MyProductionBlock.m_tmpInventoryCounts.TryGetValue(item2.Id, out point2);
                                if (point2 != 0)
                                {
                                    maxValue = MyFixedPoint.Min((MyFixedPoint) (((double) point2) / ((double) item2.Amount)), maxValue);
                                    index++;
                                    continue;
                                }
                                maxValue = 0;
                            }
                            if (base3.Atomic)
                            {
                                maxValue = MyFixedPoint.Floor(maxValue);
                            }
                            if (maxValue > 0)
                            {
                                base.InsertQueueItemRequest(-1, base3, maxValue);
                                foreach (MyBlueprintDefinitionBase.Item item3 in base3.Results)
                                {
                                    MyProductionBlock.m_tmpInventoryCounts.TryGetValue(item3.Id, out point2);
                                    point2 -= item3.Amount * maxValue;
                                    if (point2 == 0)
                                    {
                                        MyProductionBlock.m_tmpInventoryCounts.Remove(item3.Id);
                                    }
                                    else
                                    {
                                        MyProductionBlock.m_tmpInventoryCounts[item3.Id] = point2;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                MyProductionBlock.m_tmpInventoryCounts.Clear();
            }
        }

        private static bool EdgeRules(IMyConveyorEndpoint edge) => 
            ((edge.CubeBlock.OwnerId != 0) ? m_assemblerForPathfinding.FriendlyWithBlock(edge.CubeBlock) : true);

        private void FinishAssembling(MyBlueprintDefinitionBase blueprint)
        {
            MyFixedPoint point = (MyFixedPoint) (1f / this.GetEfficiencyMultiplierForBlueprint(blueprint));
            for (int i = 0; i < blueprint.Prerequisites.Length; i++)
            {
                MyBlueprintDefinitionBase.Item item = blueprint.Prerequisites[i];
                base.InputInventory.RemoveItemsOfType(item.Amount * point, item.Id, MyItemFlags.None, false);
            }
            foreach (MyBlueprintDefinitionBase.Item item2 in blueprint.Results)
            {
                MyDefinitionId id = item2.Id;
                MyObjectBuilder_PhysicalObject objectBuilder = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject(item2.Id.TypeId, id.SubtypeName);
                base.OutputInventory.AddItems(item2.Amount, objectBuilder);
                if (MyVisualScriptLogicProvider.NewItemBuilt != null)
                {
                    MyVisualScriptLogicProvider.NewItemBuilt(base.EntityId, base.CubeGrid.EntityId, base.Name, base.CubeGrid.Name, objectBuilder.TypeId.ToString(), objectBuilder.SubtypeName, item2.Amount.ToIntSafe());
                }
            }
        }

        private void FinishDisassembling(MyBlueprintDefinitionBase blueprint)
        {
            if (this.RepeatEnabled && Sync.IsServer)
            {
                base.OutputInventory.ContentsChanged -= new Action<MyInventoryBase>(this.OutputInventory_ContentsChanged);
            }
            foreach (MyBlueprintDefinitionBase.Item item in blueprint.Results)
            {
                base.OutputInventory.RemoveItemsOfType(item.Amount, item.Id, MyItemFlags.None, false);
            }
            if (this.RepeatEnabled && Sync.IsServer)
            {
                base.OutputInventory.ContentsChanged += new Action<MyInventoryBase>(this.OutputInventory_ContentsChanged);
            }
            MyFixedPoint point = (MyFixedPoint) (1f / this.GetEfficiencyMultiplierForBlueprint(blueprint));
            for (int i = 0; i < blueprint.Prerequisites.Length; i++)
            {
                MyBlueprintDefinitionBase.Item item2 = blueprint.Prerequisites[i];
                MyObjectBuilder_PhysicalObject objectBuilder = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject(item2.Id.TypeId, item2.Id.SubtypeName);
                base.InputInventory.AddItems(item2.Amount * point, objectBuilder);
            }
        }

        public void GetCoveyorInventoryOwners()
        {
            this.m_inventoryOwners.Clear();
            List<IMyConveyorEndpoint> reachableVertices = new List<IMyConveyorEndpoint>();
            MyGridConveyorSystem.FindReachable(base.ConveyorEndpoint, reachableVertices, vertex => (vertex.CubeBlock != null) && (base.FriendlyWithBlock(vertex.CubeBlock) && vertex.CubeBlock.HasInventory), null, null);
            foreach (IMyConveyorEndpoint endpoint in reachableVertices)
            {
                this.m_inventoryOwners.Add(endpoint.CubeBlock);
            }
            this.m_inventoryOwnersDirty = false;
        }

        private Action<MyAssembler> GetDelegate(Action<Sandbox.ModAPI.IMyAssembler> value) => 
            ((Action<MyAssembler>) Delegate.CreateDelegate(typeof(Action<MyAssembler>), value.Target, value.Method));

        public virtual float GetEfficiencyMultiplierForBlueprint(MyBlueprintDefinitionBase blueprint) => 
            MySession.Static.AssemblerEfficiencyMultiplier;

        private void GetItemFromOtherAssemblers(float remainingTime)
        {
            float num = MySession.Static.AssemblerSpeedMultiplier * (((MyAssemblerDefinition) base.BlockDefinition).AssemblySpeed + base.UpgradeValues["Productivity"]);
            MyAssembler masterAssembler = this.GetMasterAssembler();
            if (masterAssembler != null)
            {
                if (!masterAssembler.m_repeatAssembleEnabled)
                {
                    if (masterAssembler.m_queue.Count > 0)
                    {
                        MyProductionBlock.QueueItem? nullable = masterAssembler.TryGetQueueItem(0);
                        if ((nullable != null) && (nullable.Value.Amount > 1))
                        {
                            int amount = Math.Min(((int) nullable.Value.Amount) - 1, Convert.ToInt32(Math.Ceiling((double) (remainingTime / (nullable.Value.Blueprint.BaseProductionTimeInSeconds / num)))));
                            if (amount > 0)
                            {
                                masterAssembler.RemoveFirstQueueItemAnnounce(amount, masterAssembler.CurrentProgress);
                                base.InsertQueueItemRequest(base.m_queue.Count, nullable.Value.Blueprint, amount);
                            }
                        }
                    }
                }
                else if (base.m_queue.Count == 0)
                {
                    while (remainingTime > 0f)
                    {
                        foreach (MyProductionBlock.QueueItem item in masterAssembler.m_queue)
                        {
                            remainingTime -= (item.Blueprint.BaseProductionTimeInSeconds / num) * item.Amount;
                            base.InsertQueueItemRequest(base.m_queue.Count, item.Blueprint, item.Amount);
                        }
                    }
                }
            }
        }

        private MyAssembler GetMasterAssembler()
        {
            m_conveyorEndpoints.Clear();
            m_assemblerForPathfinding = this;
            MyGridConveyorSystem.FindReachable(base.ConveyorEndpoint, m_conveyorEndpoints, m_vertexPredicate, m_edgePredicate, null);
            int? count = null;
            m_conveyorEndpoints.ShuffleList<IMyConveyorEndpoint>(0, count);
            using (List<IMyConveyorEndpoint>.Enumerator enumerator = m_conveyorEndpoints.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyAssembler cubeBlock = enumerator.Current.CubeBlock as MyAssembler;
                    if ((cubeBlock != null) && (!cubeBlock.DisassembleEnabled && (!cubeBlock.IsSlave && (cubeBlock.m_queue.Count > 0))))
                    {
                        return cubeBlock;
                    }
                }
            }
            return null;
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_Assembler objectBuilderCubeBlock = (MyObjectBuilder_Assembler) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.CurrentProgress = this.CurrentProgress;
            objectBuilderCubeBlock.DisassembleEnabled = this.m_disassembleEnabled;
            objectBuilderCubeBlock.RepeatAssembleEnabled = this.m_repeatAssembleEnabled;
            objectBuilderCubeBlock.RepeatDisassembleEnabled = this.m_repeatDisassembleEnabled;
            objectBuilderCubeBlock.SlaveEnabled = (bool) this.m_slave;
            if (this.m_otherQueue.Count <= 0)
            {
                objectBuilderCubeBlock.OtherQueue = null;
            }
            else
            {
                objectBuilderCubeBlock.OtherQueue = new MyObjectBuilder_ProductionBlock.QueueItem[this.m_otherQueue.Count];
                for (int i = 0; i < this.m_otherQueue.Count; i++)
                {
                    objectBuilderCubeBlock.OtherQueue[i] = new MyObjectBuilder_ProductionBlock.QueueItem { 
                        Amount = this.m_otherQueue[i].Amount,
                        Id = (SerializableDefinitionId) this.m_otherQueue[i].Blueprint.Id
                    };
                }
            }
            return objectBuilderCubeBlock;
        }

        protected override float GetOperationalPowerConsumption() => 
            ((base.GetOperationalPowerConsumption() * (1f + base.UpgradeValues["Productivity"])) * (1f / base.UpgradeValues["PowerEfficiency"]));

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.UpgradeValues.Add("Productivity", 0f);
            base.UpgradeValues.Add("PowerEfficiency", 1f);
            base.Init(objectBuilder, cubeGrid);
            this.m_cubeGrid = cubeGrid;
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            this.m_assemblerDef = base.BlockDefinition as MyAssemblerDefinition;
            if (base.InventoryAggregate.InventoryCount > 2)
            {
                base.FixInputOutputInventories(this.m_assemblerDef.InputInventoryConstraint, this.m_assemblerDef.OutputInventoryConstraint);
            }
            base.InputInventory.Constraint = this.m_assemblerDef.InputInventoryConstraint;
            base.OutputInventory.Constraint = this.m_assemblerDef.OutputInventoryConstraint;
            base.InputInventory.FilterItemsUsingConstraint();
            MyObjectBuilder_Assembler assembler = (MyObjectBuilder_Assembler) objectBuilder;
            if (assembler.OtherQueue != null)
            {
                this.m_otherQueue.Clear();
                if (this.m_otherQueue.Capacity < assembler.OtherQueue.Length)
                {
                    this.m_otherQueue.Capacity = assembler.OtherQueue.Length;
                }
                for (int i = 0; i < assembler.OtherQueue.Length; i++)
                {
                    MyObjectBuilder_ProductionBlock.QueueItem item = assembler.OtherQueue[i];
                    MyBlueprintDefinitionBase base2 = MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(item.Id);
                    if (base2 == null)
                    {
                        MySandboxGame.Log.WriteLine($"No blueprint that produces a single result with Id '{item.Id}'");
                    }
                    else
                    {
                        MyProductionBlock.QueueItem item2 = new MyProductionBlock.QueueItem {
                            Blueprint = base2,
                            Amount = item.Amount
                        };
                        this.m_otherQueue.Add(item2);
                    }
                }
            }
            this.CurrentProgress = assembler.CurrentProgress;
            this.m_disassembleEnabled = this.DisassembleEnabled && assembler.DisassembleEnabled;
            this.m_repeatAssembleEnabled = assembler.RepeatAssembleEnabled;
            this.m_repeatDisassembleEnabled = assembler.RepeatDisassembleEnabled;
            this.m_slave.SetLocalValue(assembler.SlaveEnabled);
            this.UpdateInventoryFlags();
            base.m_baseIdleSound = base.BlockDefinition.PrimarySound;
            base.m_processSound = base.BlockDefinition.ActionSound;
            base.OnUpgradeValuesChanged += new Action(this.UpdateDetailedInfo);
            base.ResourceSink.RequiredInputChanged += new MyRequiredResourceChangeDelegate(this.PowerReceiver_RequiredInputChanged);
            this.UpdateDetailedInfo();
        }

        protected override void InsertQueueItem(int idx, MyBlueprintDefinitionBase blueprint, MyFixedPoint amount)
        {
            if (idx == 0)
            {
                MyProductionBlock.QueueItem? nullable = base.TryGetFirstQueueItem();
                if ((nullable != null) && !ReferenceEquals(nullable.Value.Blueprint, blueprint))
                {
                    this.CurrentProgress = 0f;
                }
            }
            base.InsertQueueItem(idx, blueprint, amount);
        }

        [Event(null, 0x33d), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void ModeSwitchCallback(bool disassembleEnabled)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyAssembler, bool>(this, x => new Action<bool>(x.ModeSwitchClient), disassembleEnabled, targetEndpoint);
            this.DisassembleEnabled = disassembleEnabled;
        }

        [Event(null, 0x344), Reliable, Broadcast]
        private void ModeSwitchClient(bool disassembleEnabled)
        {
            this.DisassembleEnabled = disassembleEnabled;
        }

        protected override void OnBeforeInventoryRemovedFromAggregate(MyInventoryAggregate aggregate, MyInventoryBase inventory)
        {
            base.OnBeforeInventoryRemovedFromAggregate(aggregate, inventory);
            if (ReferenceEquals(inventory, base.OutputInventory) && Sync.IsServer)
            {
                base.OutputInventory.ContentsChanged -= new Action<MyInventoryBase>(this.OutputInventory_ContentsChanged);
            }
        }

        protected override void OnInventoryAddedToAggregate(MyInventoryAggregate aggregate, MyInventoryBase inventory)
        {
            base.OnInventoryAddedToAggregate(aggregate, inventory);
            if (ReferenceEquals(inventory, base.OutputInventory) && Sync.IsServer)
            {
                base.OutputInventory.ContentsChanged += new Action<MyInventoryBase>(this.OutputInventory_ContentsChanged);
            }
        }

        protected override void OnQueueChanged()
        {
            if ((this.CurrentState == StateEnum.MissingItems) && base.IsQueueEmpty)
            {
                this.CurrentState = !base.Enabled ? StateEnum.Disabled : (!base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) ? StateEnum.NotEnoughPower : (!base.IsFunctional ? StateEnum.NotWorking : StateEnum.Ok));
            }
            this.IsProducing = base.IsWorking && !base.IsQueueEmpty;
            base.OnQueueChanged();
        }

        private void OnSlaveChanged()
        {
            if (this.CurrentModeChanged != null)
            {
                this.CurrentModeChanged(this);
            }
        }

        private void OutputInventory_ContentsChanged(MyInventoryBase inventory)
        {
            if ((this.DisassembleEnabled && this.RepeatEnabled) && Sync.IsServer)
            {
                this.RebuildQueueInRepeatDisassembling();
            }
        }

        private void PowerReceiver_RequiredInputChanged(MyDefinitionId resourceTypeId, MyResourceSinkComponent receiver, float oldRequirement, float newRequirement)
        {
            this.UpdateDetailedInfo();
        }

        private void RebuildQueueInRepeatDisassembling()
        {
            if (this.DisassembleEnabled && this.RepeatEnabled)
            {
                this.RequestDisassembleAll();
            }
        }

        protected override void RemoveFirstQueueItem(int index, MyFixedPoint amount, float progress = 0f)
        {
            this.CurrentProgress = progress;
            if (this.CurrentItemIndex == index)
            {
                base.m_currentQueueItem = null;
            }
            base.RemoveFirstQueueItem(index, amount, 0f);
        }

        protected override void RemoveQueueItem(int itemIdx)
        {
            if (itemIdx == 0)
            {
                this.CurrentProgress = 0f;
            }
            base.RemoveQueueItem(itemIdx);
        }

        [Event(null, 0x350), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void RepeatEnabledCallback(bool disassembleEnabled, bool repeatEnable)
        {
            this.RepeatEnabledSuccess(disassembleEnabled, repeatEnable);
        }

        private void RepeatEnabledSuccess(bool disassembleMode, bool repeatEnabled)
        {
            if (disassembleMode)
            {
                this.SetRepeat(ref this.m_repeatDisassembleEnabled, repeatEnabled);
            }
            else
            {
                this.SetRepeat(ref this.m_repeatAssembleEnabled, repeatEnabled);
            }
        }

        public void RequestDisassembleAll()
        {
            if (this.DisassembleEnabled && !this.RepeatEnabled)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyAssembler>(this, x => new Action(x.DisassembleAllCallback), targetEndpoint);
            }
        }

        public void RequestDisassembleEnabled(bool newDisassembleEnabled)
        {
            if (newDisassembleEnabled != this.DisassembleEnabled)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyAssembler, bool>(this, x => new Action<bool>(x.ModeSwitchCallback), newDisassembleEnabled, targetEndpoint);
            }
        }

        public void RequestRepeatEnabled(bool newRepeatEnable)
        {
            if (newRepeatEnable != this.RepeatEnabled)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyAssembler, bool, bool>(this, x => new Action<bool, bool>(x.RepeatEnabledCallback), this.DisassembleEnabled, newRepeatEnable, targetEndpoint);
            }
        }

        private void SetRepeat(ref bool currentValue, bool newValue)
        {
            if (currentValue != newValue)
            {
                currentValue = newValue;
                this.RebuildQueueInRepeatDisassembling();
                if (this.CurrentModeChanged != null)
                {
                    this.CurrentModeChanged(this);
                }
            }
        }

        public override unsafe void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (this.m_inventoryOwnersDirty)
            {
                this.GetCoveyorInventoryOwners();
            }
            if ((Sync.IsServer && base.IsWorking) && (base.m_useConveyorSystem != null))
            {
                if (this.DisassembleEnabled)
                {
                    if (base.OutputInventory.VolumeFillFactor < 0.99f)
                    {
                        MyProductionBlock.QueueItem? nullable = base.TryGetFirstQueueItem();
                        if ((nullable != null) && !base.OutputInventory.ContainItems(new MyFixedPoint?(nullable.Value.Amount), nullable.Value.Blueprint.Results[0].Id, MyItemFlags.None))
                        {
                            MyGridConveyorSystem.ItemPullRequest(this, base.OutputInventory, base.OwnerId, nullable.Value.Blueprint.Results[0].Id, new MyFixedPoint?(nullable.Value.Amount), false);
                        }
                    }
                    if (base.InputInventory.VolumeFillFactor > 0.75f)
                    {
                        MyGridConveyorSystem.PushAnyRequest(this, base.InputInventory, base.OwnerId);
                    }
                }
                else
                {
                    if (base.InputInventory.VolumeFillFactor < 0.99f)
                    {
                        bool flag = false;
                        int idx = 0;
                        float num2 = 0f;
                        while (true)
                        {
                            using (MyUtils.ReuseCollection<MyTuple<MyFixedPoint, MyBlueprintDefinitionBase.Item>>(ref this.m_requiredComponents))
                            {
                                float num3 = num2;
                                MyProductionBlock.QueueItem? nullable2 = base.TryGetQueueItem(idx);
                                float num4 = 5f - num2;
                                if (nullable2 != null)
                                {
                                    float num5 = ((MyAssemblerDefinition) base.BlockDefinition).AssemblySpeed + base.UpgradeValues["Productivity"];
                                    float num6 = MySession.Static.AssemblerSpeedMultiplier * num5;
                                    int num7 = 1;
                                    if ((nullable2.Value.Blueprint.BaseProductionTimeInSeconds / num6) < num4)
                                    {
                                        num7 = Math.Min((int) nullable2.Value.Amount, Convert.ToInt32(Math.Ceiling((double) (num4 / (nullable2.Value.Blueprint.BaseProductionTimeInSeconds / num6)))));
                                    }
                                    num2 += (num7 * nullable2.Value.Blueprint.BaseProductionTimeInSeconds) / num6;
                                    if (num2 < 5f)
                                    {
                                        flag = true;
                                    }
                                    MyFixedPoint point = (MyFixedPoint) (1f / this.GetEfficiencyMultiplierForBlueprint(nullable2.Value.Blueprint));
                                    MyBlueprintDefinitionBase.Item[] prerequisites = nullable2.Value.Blueprint.Prerequisites;
                                    int index = 0;
                                    while (index < prerequisites.Length)
                                    {
                                        MyBlueprintDefinitionBase.Item item = prerequisites[index];
                                        MyFixedPoint point2 = item.Amount * point;
                                        MyFixedPoint point3 = point2 * num7;
                                        bool flag2 = false;
                                        int num9 = 0;
                                        while (true)
                                        {
                                            if (num9 < this.m_requiredComponents.Count)
                                            {
                                                MyBlueprintDefinitionBase.Item item2 = this.m_requiredComponents[num9].Item2;
                                                if (!(item2.Id == item.Id))
                                                {
                                                    num9++;
                                                    continue;
                                                }
                                                MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref item2.Amount;
                                                pointPtr1[0] += point3;
                                                this.m_requiredComponents[num9] = MyTuple.Create<MyFixedPoint, MyBlueprintDefinitionBase.Item>(this.m_requiredComponents[num9].Item1 + point2, item2);
                                                flag2 = true;
                                            }
                                            if (!flag2)
                                            {
                                                MyBlueprintDefinitionBase.Item item3 = new MyBlueprintDefinitionBase.Item {
                                                    Amount = point3,
                                                    Id = item.Id
                                                };
                                                this.m_requiredComponents.Add(MyTuple.Create<MyFixedPoint, MyBlueprintDefinitionBase.Item>(point2, item3));
                                            }
                                            index++;
                                            break;
                                        }
                                    }
                                }
                                foreach (MyTuple<MyFixedPoint, MyBlueprintDefinitionBase.Item> tuple in this.m_requiredComponents)
                                {
                                    MyBlueprintDefinitionBase.Item item4 = tuple.Item2;
                                    MyFixedPoint point5 = base.InputInventory.GetItemAmount(item4.Id, MyItemFlags.None, false);
                                    MyFixedPoint point6 = item4.Amount - point5;
                                    if ((point6 > 0) && ((MyGridConveyorSystem.ItemPullRequest(this, base.InputInventory, base.OwnerId, item4.Id, new MyFixedPoint?(point6), false) == 0) && (tuple.Item1 > point5)))
                                    {
                                        flag = true;
                                        num2 = num3;
                                    }
                                }
                                idx++;
                                if (idx >= base.m_queue.Count)
                                {
                                    flag = false;
                                }
                            }
                            if (!flag)
                            {
                                if (this.IsSlave && !this.RepeatEnabled)
                                {
                                    float remainingTime = 5f - num2;
                                    if (remainingTime > 0f)
                                    {
                                        this.GetItemFromOtherAssemblers(remainingTime);
                                    }
                                }
                                break;
                            }
                        }
                    }
                    if (base.OutputInventory.VolumeFillFactor > 0.75f)
                    {
                        MyGridConveyorSystem.PushAnyRequest(this, base.OutputInventory, base.OwnerId);
                    }
                }
            }
        }

        private void UpdateDetailedInfo()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(base.BlockDefinition.DisplayNameText);
            base.DetailedInfo.AppendFormat("\n", Array.Empty<object>());
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(this.GetOperationalPowerConsumption(), base.DetailedInfo);
            base.DetailedInfo.AppendFormat("\n", Array.Empty<object>());
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_RequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.RequiredInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            base.DetailedInfo.AppendFormat("\n\n", Array.Empty<object>());
            base.DetailedInfo.Append(MyTexts.Get(MySpaceTexts.BlockPropertiesText_Productivity));
            base.DetailedInfo.Append(((base.UpgradeValues["Productivity"] + 1f) * 100f).ToString("F0"));
            base.DetailedInfo.Append("%\n");
            base.DetailedInfo.Append(MyTexts.Get(MySpaceTexts.BlockPropertiesText_Efficiency));
            base.DetailedInfo.Append((base.UpgradeValues["PowerEfficiency"] * 100f).ToString("F0"));
            base.DetailedInfo.Append("%\n\n");
            base.PrintUpgradeModuleInfo();
            base.RaisePropertiesChanged();
        }

        private void UpdateInventoryFlags()
        {
            base.OutputInventory.SetFlags(this.DisassembleEnabled ? MyInventoryFlags.CanReceive : MyInventoryFlags.CanSend);
            base.InputInventory.SetFlags(this.DisassembleEnabled ? MyInventoryFlags.CanSend : MyInventoryFlags.CanReceive);
        }

        protected override void UpdateProduction(int timeDelta)
        {
            if (!base.Enabled)
            {
                this.CurrentState = StateEnum.Disabled;
                base.IsProducing = false;
            }
            else if ((!base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) || (base.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId) < this.GetOperationalPowerConsumption())) && !base.ResourceSink.IsPowerAvailable(MyResourceDistributorComponent.ElectricityId, this.GetOperationalPowerConsumption()))
            {
                this.CurrentState = StateEnum.NotEnoughPower;
                base.IsProducing = false;
            }
            else if (!base.IsWorking)
            {
                this.CurrentState = StateEnum.NotWorking;
                base.IsProducing = false;
            }
            else if (base.IsQueueEmpty)
            {
                this.CurrentState = StateEnum.Ok;
                base.IsProducing = false;
            }
            else
            {
                int num1;
                int idx = 0;
                while ((timeDelta > 0) && (idx < base.m_queue.Count))
                {
                    if (base.IsQueueEmpty)
                    {
                        this.CurrentProgress = 0f;
                        base.IsProducing = false;
                        return;
                    }
                    if (base.m_currentQueueItem == null)
                    {
                        base.m_currentQueueItem = base.TryGetQueueItem(idx);
                    }
                    MyBlueprintDefinitionBase blueprint = base.m_currentQueueItem.Value.Blueprint;
                    this.CurrentState = this.CheckInventory(blueprint);
                    if (this.CurrentState != StateEnum.Ok)
                    {
                        idx++;
                        base.m_currentQueueItem = null;
                    }
                    else
                    {
                        float num2 = this.calculateBlueprintProductionTime(blueprint) - (this.CurrentProgress * this.calculateBlueprintProductionTime(blueprint));
                        if (timeDelta < num2)
                        {
                            this.CurrentProgress += ((float) timeDelta) / this.calculateBlueprintProductionTime(blueprint);
                            timeDelta = 0;
                        }
                        else
                        {
                            if (Sync.IsServer)
                            {
                                if (this.DisassembleEnabled)
                                {
                                    this.FinishDisassembling(blueprint);
                                }
                                else
                                {
                                    if (this.RepeatEnabled)
                                    {
                                        base.InsertQueueItemRequest(-1, blueprint);
                                    }
                                    this.FinishAssembling(blueprint);
                                }
                                base.RemoveQueueItemRequest(base.m_queue.IndexOf(base.m_currentQueueItem.Value), 1, 0f);
                                base.m_currentQueueItem = null;
                            }
                            timeDelta -= (int) Math.Ceiling((double) num2);
                            this.CurrentProgress = 0f;
                            base.m_currentQueueItem = null;
                        }
                    }
                }
                if (!base.IsWorking || base.IsQueueEmpty)
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) (this.CurrentState == StateEnum.Ok);
                }
                this.IsProducing = (bool) num1;
            }
        }

        private static bool VertexRules(IMyConveyorEndpoint vertex) => 
            ((vertex.CubeBlock is MyAssembler) && !ReferenceEquals(vertex.CubeBlock, m_assemblerForPathfinding));

        public bool InventoryOwnersDirty
        {
            get => 
                this.m_inventoryOwnersDirty;
            set => 
                (this.m_inventoryOwnersDirty = value);
        }

        public bool IsSlave
        {
            get => 
                ((bool) this.m_slave);
            set
            {
                if (!(!this.SupportsAdvancedFunctions & value))
                {
                    this.m_slave.Value = value;
                }
            }
        }

        public float CurrentProgress
        {
            get => 
                this.m_currentProgress;
            set
            {
                if (this.m_currentProgress != value)
                {
                    this.m_currentProgress = value;
                    if (this.CurrentProgressChanged != null)
                    {
                        this.CurrentProgressChanged(this);
                    }
                }
            }
        }

        public StateEnum CurrentState
        {
            get => 
                this.m_currentState;
            private set
            {
                if (this.m_currentState != value)
                {
                    this.m_currentState = value;
                    if (this.CurrentStateChanged != null)
                    {
                        this.CurrentStateChanged(this);
                    }
                }
            }
        }

        public int CurrentItemIndex =>
            ((base.m_currentQueueItem != null) ? base.m_queue.FindIndex(x => x.ItemId == base.m_currentQueueItem.Value.ItemId) : -1);

        public bool RepeatEnabled
        {
            get => 
                (this.m_disassembleEnabled ? this.m_repeatDisassembleEnabled : this.m_repeatAssembleEnabled);
            private set
            {
                if (!(!this.SupportsAdvancedFunctions & value))
                {
                    if (this.m_disassembleEnabled)
                    {
                        this.SetRepeat(ref this.m_repeatDisassembleEnabled, value);
                    }
                    else
                    {
                        this.SetRepeat(ref this.m_repeatAssembleEnabled, value);
                    }
                }
            }
        }

        public virtual bool SupportsAdvancedFunctions =>
            true;

        public bool DisassembleEnabled
        {
            get => 
                this.m_disassembleEnabled;
            private set
            {
                if ((this.m_disassembleEnabled != value) && !(!this.SupportsAdvancedFunctions & value))
                {
                    this.CurrentProgress = 0f;
                    this.m_disassembleEnabled = value;
                    base.SwapQueue(ref this.m_otherQueue);
                    this.RebuildQueueInRepeatDisassembling();
                    this.UpdateInventoryFlags();
                    this.m_currentState = StateEnum.Ok;
                    if (this.CurrentModeChanged != null)
                    {
                        this.CurrentModeChanged(this);
                    }
                    if (this.CurrentStateChanged != null)
                    {
                        this.CurrentStateChanged(this);
                    }
                }
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyAssembler.DisassembleEnabled =>
            this.DisassembleEnabled;

        MyAssemblerMode Sandbox.ModAPI.Ingame.IMyAssembler.Mode
        {
            get => 
                (this.DisassembleEnabled ? MyAssemblerMode.Disassembly : MyAssemblerMode.Assembly);
            set => 
                this.RequestDisassembleEnabled(value == MyAssemblerMode.Disassembly);
        }

        bool Sandbox.ModAPI.Ingame.IMyAssembler.CooperativeMode
        {
            get => 
                this.IsSlave;
            set => 
                (this.IsSlave = value);
        }

        bool Sandbox.ModAPI.Ingame.IMyAssembler.Repeating
        {
            get => 
                this.RepeatEnabled;
            set => 
                this.RequestRepeatEnabled(value);
        }

        public virtual int GUIPriority =>
            ((int) MathHelper.Lerp((float) 200f, (float) 500f, (float) (1f - this.m_assemblerDef.AssemblySpeed)));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAssembler.<>c <>9 = new MyAssembler.<>c();
            public static Func<MyAssembler, bool> <>9__43_0;
            public static MyTerminalValueControl<MyAssembler, bool>.GetterDelegate <>9__43_1;
            public static MyTerminalValueControl<MyAssembler, bool>.SetterDelegate <>9__43_2;
            public static Func<MyAssembler, Action<bool>> <>9__81_0;
            public static Func<MyAssembler, Action<bool>> <>9__82_0;
            public static Func<MyAssembler, Action<bool, bool>> <>9__84_0;
            public static Func<MyAssembler, Action> <>9__86_0;

            internal bool <CreateTerminalControls>b__43_0(MyAssembler x) => 
                x.SupportsAdvancedFunctions;

            internal bool <CreateTerminalControls>b__43_1(MyAssembler x) => 
                x.IsSlave;

            internal void <CreateTerminalControls>b__43_2(MyAssembler x, bool v)
            {
                if (x.RepeatEnabled)
                {
                    x.RequestRepeatEnabled(false);
                }
                x.IsSlave = v;
            }

            internal Action<bool> <ModeSwitchCallback>b__82_0(MyAssembler x) => 
                new Action<bool>(x.ModeSwitchClient);

            internal Action <RequestDisassembleAll>b__86_0(MyAssembler x) => 
                new Action(x.DisassembleAllCallback);

            internal Action<bool> <RequestDisassembleEnabled>b__81_0(MyAssembler x) => 
                new Action<bool>(x.ModeSwitchCallback);

            internal Action<bool, bool> <RequestRepeatEnabled>b__84_0(MyAssembler x) => 
                new Action<bool, bool>(x.RepeatEnabledCallback);
        }

        public enum StateEnum
        {
            Ok,
            Disabled,
            NotWorking,
            NotEnoughPower,
            MissingItems,
            InventoryFull
        }
    }
}

