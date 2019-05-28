namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyCubeBlockType(typeof(MyObjectBuilder_Refinery)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyRefinery), typeof(Sandbox.ModAPI.Ingame.IMyRefinery) })]
    public class MyRefinery : MyProductionBlock, Sandbox.ModAPI.IMyRefinery, Sandbox.ModAPI.IMyProductionBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyProductionBlock, Sandbox.ModAPI.Ingame.IMyRefinery
    {
        private VRage.Game.Entity.MyEntity m_currentUser;
        private MyRefineryDefinition m_refineryDef;
        private bool m_queueNeedsRebuild;
        private bool m_processingLock;
        private readonly List<KeyValuePair<int, MyBlueprintDefinitionBase>> m_tmpSortedBlueprints = new List<KeyValuePair<int, MyBlueprintDefinitionBase>>();

        private void ChangeRequirementsToResults(MyBlueprintDefinitionBase queueItem, MyFixedPoint blueprintAmount)
        {
            if (this.m_refineryDef == null)
            {
                MyLog.Default.WriteLine("m_refineryDef shouldn't be null!!!" + this);
            }
            else if (Sync.IsServer && (((MySession.Static != null) && ((queueItem != null) && ((queueItem.Prerequisites != null) && ((base.OutputInventory != null) && ((base.InputInventory != null) && (queueItem.Results != null)))))) && (this.m_refineryDef != null)))
            {
                if (!MySession.Static.CreativeMode)
                {
                    MyFixedPoint point1 = MyFixedPoint.Min(base.OutputInventory.ComputeAmountThatFits(queueItem), blueprintAmount);
                    blueprintAmount = point1;
                }
                if (blueprintAmount != 0)
                {
                    foreach (MyBlueprintDefinitionBase.Item item in queueItem.Prerequisites)
                    {
                        MyObjectBuilder_PhysicalObject objectBuilder = MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) item.Id) as MyObjectBuilder_PhysicalObject;
                        if (objectBuilder == null)
                        {
                            MyLog.Default.WriteLine("obPrerequisite shouldn't be null!!! " + this);
                        }
                        else
                        {
                            float num2 = ((float) blueprintAmount) * ((float) item.Amount);
                            base.InputInventory.RemoveItemsOfType((MyFixedPoint) num2, objectBuilder, false, false);
                            MyFixedPoint amount = base.InputInventory.GetItemAmount(item.Id, MyItemFlags.None, false);
                            if (amount < ((MyFixedPoint) 0.01f))
                            {
                                base.InputInventory.RemoveItemsOfType(amount, item.Id, MyItemFlags.None, false);
                            }
                        }
                    }
                    foreach (MyBlueprintDefinitionBase.Item item2 in queueItem.Results)
                    {
                        MyObjectBuilder_PhysicalObject objectBuilder = MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) item2.Id) as MyObjectBuilder_PhysicalObject;
                        if (objectBuilder == null)
                        {
                            MyLog.Default.WriteLine("obResult shouldn't be null!!! " + this);
                        }
                        else
                        {
                            float num3 = (((float) item2.Amount) * this.m_refineryDef.MaterialEfficiency) * base.UpgradeValues["Effectiveness"];
                            MyFixedPoint amount = (MyFixedPoint) (((float) blueprintAmount) * num3);
                            base.OutputInventory.AddItems(amount, objectBuilder);
                        }
                    }
                    base.RemoveFirstQueueItemAnnounce(blueprintAmount, 0f);
                }
            }
        }

        protected override float GetOperationalPowerConsumption() => 
            ((base.GetOperationalPowerConsumption() * (1f + base.UpgradeValues["Productivity"])) * (1f / base.UpgradeValues["PowerEfficiency"]));

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.UpgradeValues.Add("Productivity", 0f);
            base.UpgradeValues.Add("Effectiveness", 1f);
            base.UpgradeValues.Add("PowerEfficiency", 1f);
            base.Init(objectBuilder, cubeGrid);
            this.m_refineryDef = base.BlockDefinition as MyRefineryDefinition;
            if (base.InventoryAggregate.InventoryCount > 2)
            {
                base.FixInputOutputInventories(this.m_refineryDef.InputInventoryConstraint, this.m_refineryDef.OutputInventoryConstraint);
            }
            base.InputInventory.Constraint = this.m_refineryDef.InputInventoryConstraint;
            base.InputInventory.FilterItemsUsingConstraint();
            base.OutputInventory.Constraint = this.m_refineryDef.OutputInventoryConstraint;
            base.OutputInventory.FilterItemsUsingConstraint();
            this.m_queueNeedsRebuild = true;
            base.m_baseIdleSound = base.BlockDefinition.PrimarySound;
            base.m_processSound = base.BlockDefinition.ActionSound;
            base.ResourceSink.RequiredInputChanged += new MyRequiredResourceChangeDelegate(this.PowerReceiver_RequiredInputChanged);
            base.OnUpgradeValuesChanged += new Action(this.UpdateDetailedInfo);
            this.UpdateDetailedInfo();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        private void inventory_OnContentsChanged(MyInventoryBase inv)
        {
            if (!this.m_processingLock && Sync.IsServer)
            {
                this.m_queueNeedsRebuild = true;
            }
        }

        protected override void OnBeforeInventoryRemovedFromAggregate(MyInventoryAggregate aggregate, MyInventoryBase inventory)
        {
            if (ReferenceEquals(inventory, base.InputInventory))
            {
                base.InputInventory.ContentsChanged += new Action<MyInventoryBase>(this.inventory_OnContentsChanged);
            }
            else if (ReferenceEquals(inventory, base.OutputInventory))
            {
                base.OutputInventory.ContentsChanged += new Action<MyInventoryBase>(this.inventory_OnContentsChanged);
            }
            base.OnBeforeInventoryRemovedFromAggregate(aggregate, inventory);
        }

        protected override void OnInventoryAddedToAggregate(MyInventoryAggregate aggregate, MyInventoryBase inventory)
        {
            base.OnInventoryAddedToAggregate(aggregate, inventory);
            if (ReferenceEquals(inventory, base.InputInventory))
            {
                base.InputInventory.ContentsChanged += new Action<MyInventoryBase>(this.inventory_OnContentsChanged);
            }
            else if (ReferenceEquals(inventory, base.OutputInventory))
            {
                base.OutputInventory.ContentsChanged += new Action<MyInventoryBase>(this.inventory_OnContentsChanged);
            }
        }

        private void PowerReceiver_RequiredInputChanged(MyDefinitionId resourceTypeId, MyResourceSinkComponent receiver, float oldRequirement, float newRequirement)
        {
            this.UpdateDetailedInfo();
        }

        private void ProcessQueueItems(int timeDelta)
        {
            this.m_processingLock = true;
            if (Sync.IsServer)
            {
                goto TR_0010;
            }
        TR_0000:
            base.IsProducing = !base.IsQueueEmpty;
            this.m_processingLock = false;
            return;
        TR_0010:
            while (true)
            {
                if (!base.IsQueueEmpty && (timeDelta > 0))
                {
                    MyProductionBlock.QueueItem item = base.TryGetFirstQueueItem().Value;
                    MyFixedPoint blueprintAmount = (MyFixedPoint) (((timeDelta * (this.m_refineryDef.RefineSpeed + base.UpgradeValues["Productivity"])) * MySession.Static.RefinerySpeedMultiplier) / (item.Blueprint.BaseProductionTimeInSeconds * 1000f));
                    MyBlueprintDefinitionBase.Item[] prerequisites = item.Blueprint.Prerequisites;
                    int index = 0;
                    while (true)
                    {
                        if (index < prerequisites.Length)
                        {
                            MyBlueprintDefinitionBase.Item item2 = prerequisites[index];
                            MyFixedPoint point2 = base.InputInventory.GetItemAmount(item2.Id, MyItemFlags.None, false);
                            MyFixedPoint point3 = blueprintAmount * item2.Amount;
                            if (point2 < point3)
                            {
                                blueprintAmount = point2 * (1f / ((float) item2.Amount));
                            }
                            index++;
                            continue;
                        }
                        if (blueprintAmount != 0)
                        {
                            timeDelta -= Math.Max(1, (int) (((((float) blueprintAmount) * item.Blueprint.BaseProductionTimeInSeconds) / this.m_refineryDef.RefineSpeed) * 1000f));
                            if (timeDelta < 0)
                            {
                                timeDelta = 0;
                            }
                            this.ChangeRequirementsToResults(item.Blueprint, blueprintAmount);
                            continue;
                        }
                        else
                        {
                            this.m_queueNeedsRebuild = true;
                        }
                        break;
                    }
                }
                goto TR_0000;
            }
            goto TR_0010;
        }

        private void RebuildQueue()
        {
            this.m_queueNeedsRebuild = false;
            base.ClearQueue(false);
            this.m_tmpSortedBlueprints.Clear();
            MyPhysicalInventoryItem[] itemArray = base.InputInventory.GetItems().ToArray();
            int index = 0;
            while (true)
            {
                int num2;
                while (true)
                {
                    if (index < itemArray.Length)
                    {
                        num2 = 0;
                        break;
                    }
                    int num4 = 0;
                    while (num4 < this.m_tmpSortedBlueprints.Count)
                    {
                        KeyValuePair<int, MyBlueprintDefinitionBase> pair = this.m_tmpSortedBlueprints[num4];
                        MyBlueprintDefinitionBase blueprint = pair.Value;
                        MyFixedPoint maxValue = MyFixedPoint.MaxValue;
                        MyBlueprintDefinitionBase.Item[] prerequisites = blueprint.Prerequisites;
                        int num5 = 0;
                        while (true)
                        {
                            if (num5 < prerequisites.Length)
                            {
                                MyBlueprintDefinitionBase.Item item = prerequisites[num5];
                                MyFixedPoint amount = itemArray[num4].Amount;
                                if (amount != 0)
                                {
                                    maxValue = MyFixedPoint.Min(amount * (1f / ((float) item.Amount)), maxValue);
                                    num5++;
                                    continue;
                                }
                                maxValue = 0;
                            }
                            if (blueprint.Atomic)
                            {
                                maxValue = MyFixedPoint.Floor(maxValue);
                            }
                            if ((maxValue > 0) && (maxValue != MyFixedPoint.MaxValue))
                            {
                                base.InsertQueueItemRequest(-1, blueprint, maxValue);
                            }
                            num4++;
                            break;
                        }
                    }
                    this.m_tmpSortedBlueprints.Clear();
                    return;
                }
                while (true)
                {
                    if (num2 >= this.m_refineryDef.BlueprintClasses.Count)
                    {
                        index++;
                        break;
                    }
                    using (IEnumerator<MyBlueprintDefinitionBase> enumerator = this.m_refineryDef.BlueprintClasses[num2].GetEnumerator())
                    {
                        while (true)
                        {
                            if (enumerator.MoveNext())
                            {
                                MyBlueprintDefinitionBase current = enumerator.Current;
                                bool flag = false;
                                MyDefinitionId other = new MyDefinitionId(itemArray[index].Content.TypeId, itemArray[index].Content.SubtypeId);
                                int num3 = 0;
                                while (true)
                                {
                                    if (num3 < current.Prerequisites.Length)
                                    {
                                        if (!current.Prerequisites[num3].Id.Equals(other))
                                        {
                                            num3++;
                                            continue;
                                        }
                                        flag = true;
                                    }
                                    if (!flag)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        this.m_tmpSortedBlueprints.Add(new KeyValuePair<int, MyBlueprintDefinitionBase>(index, current));
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    num2++;
                }
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if ((Sync.IsServer && base.IsWorking) && (base.m_useConveyorSystem != null))
            {
                if ((base.InputInventory.VolumeFillFactor < 0.6f) && MyGridConveyorSystem.PullAllRequest(this, base.InputInventory, base.OwnerId, base.InputInventory.Constraint, this.m_refineryDef.OreAmountPerPullRequest, true))
                {
                    this.m_queueNeedsRebuild = true;
                }
                if (base.OutputInventory.VolumeFillFactor > 0.75f)
                {
                    MyGridConveyorSystem.PushAnyRequest(this, base.OutputInventory, base.OwnerId);
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
            base.DetailedInfo.Append(MyTexts.Get(MySpaceTexts.BlockPropertiesText_Effectiveness));
            base.DetailedInfo.Append((base.UpgradeValues["Effectiveness"] * 100f).ToString("F0"));
            base.DetailedInfo.Append("%\n");
            base.DetailedInfo.Append(MyTexts.Get(MySpaceTexts.BlockPropertiesText_Efficiency));
            base.DetailedInfo.Append((base.UpgradeValues["PowerEfficiency"] * 100f).ToString("F0"));
            base.DetailedInfo.Append("%\n\n");
            base.PrintUpgradeModuleInfo();
            base.RaisePropertiesChanged();
        }

        protected override void UpdateProduction(int timeDelta)
        {
            int num1;
            if (this.m_queueNeedsRebuild && Sync.IsServer)
            {
                this.RebuildQueue();
            }
            if ((!base.IsWorking || base.IsQueueEmpty) || base.OutputInventory.IsFull)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId);
            }
            this.IsProducing = (bool) num1;
            if (base.IsProducing)
            {
                this.ProcessQueueItems(timeDelta);
            }
        }
    }
}

