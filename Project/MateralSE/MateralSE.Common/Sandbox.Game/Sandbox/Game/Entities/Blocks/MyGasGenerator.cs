namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
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
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;

    [MyCubeBlockType(typeof(MyObjectBuilder_OxygenGenerator)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyGasGenerator), typeof(Sandbox.ModAPI.Ingame.IMyGasGenerator), typeof(Sandbox.ModAPI.IMyOxygenGenerator), typeof(Sandbox.ModAPI.Ingame.IMyOxygenGenerator) })]
    public class MyGasGenerator : MyFunctionalBlock, IMyGasBlock, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyOxygenGenerator, Sandbox.ModAPI.IMyGasGenerator, Sandbox.ModAPI.Ingame.IMyGasGenerator, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyOxygenGenerator, IMyInventoryOwner, IMyEventProxy, IMyEventOwner
    {
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_useConveyorSystem;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_autoRefill;
        private bool m_isProducing;
        private MyInventoryConstraint m_oreConstraint;
        private MyMultilineConveyorEndpoint m_conveyorEndpoint;
        private readonly MyDefinitionId m_oxygenGasId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
        private MyResourceSourceComponent m_sourceComp;
        private float m_productionCapacityMultiplier = 1f;
        private float m_powerConsumptionMultiplier = 1f;

        public MyGasGenerator()
        {
            this.CreateTerminalControls();
            this.SourceComp = new MyResourceSourceComponent(2);
            base.ResourceSink = new MyResourceSinkComponent(1);
        }

        public bool AllowSelfPulling() => 
            false;

        private bool CanRefill()
        {
            if (this.CanProduce && this.HasIce())
            {
                using (List<MyPhysicalInventoryItem>.Enumerator enumerator = this.GetInventory(0).GetItems().GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyObjectBuilder_GasContainerObject content = enumerator.Current.Content as MyObjectBuilder_GasContainerObject;
                        if ((content != null) && (content.GasLevel < 1f))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected override bool CheckIsWorking() => 
            (base.CheckIsWorking() && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId));

        protected override void Closing()
        {
            base.Closing();
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.StopSound(true, true);
            }
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            this.SourceComp.Enabled = this.CanProduce;
            base.ResourceSink.Update();
            if (base.CubeGrid.GridSystems.ResourceDistributor != null)
            {
                base.CubeGrid.GridSystems.ResourceDistributor.ConveyorSystem_OnPoweredChanged();
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        private float ComputeRequiredPower()
        {
            if (((!MySession.Static.Settings.EnableOxygen && this.BlockDefinition.ProducedGases.TrueForAll(info => info.Id == this.m_oxygenGasId)) || !base.Enabled) || !base.IsFunctional)
            {
                return 0f;
            }
            bool flag = false;
            foreach (MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo info in this.BlockDefinition.ProducedGases)
            {
                flag = flag || ((this.SourceComp.CurrentOutputByType(info.Id) > 0f) && (MySession.Static.Settings.EnableOxygen || (info.Id != this.m_oxygenGasId)));
            }
            return ((flag ? this.BlockDefinition.OperationalPowerConsumption : this.BlockDefinition.StandbyPowerConsumption) * this.m_powerConsumptionMultiplier);
        }

        private void ConsumeFuel(ref MyDefinitionId gasTypeId, double iceAmount)
        {
            if (((Sync.IsServer && (base.CubeGrid.GridSystems.ControlSystem != null)) && (iceAmount > 0.0)) && !MySession.Static.CreativeMode)
            {
                List<MyPhysicalInventoryItem> items = this.GetInventory(0).GetItems();
                if ((items.Count > 0) && (iceAmount > 0.0))
                {
                    int num = 0;
                    while (num < items.Count)
                    {
                        MatrixD? nullable;
                        MyPhysicalInventoryItem item = items[num];
                        if (item.Content is MyObjectBuilder_GasContainerObject)
                        {
                            num++;
                            continue;
                        }
                        if (iceAmount < ((float) item.Amount))
                        {
                            MyFixedPoint point = MyFixedPoint.Max((MyFixedPoint) iceAmount, MyFixedPoint.SmallestPossibleValue);
                            nullable = null;
                            this.GetInventory(0).RemoveItems(item.ItemId, new MyFixedPoint?(point), true, false, nullable);
                            return;
                        }
                        iceAmount -= (float) item.Amount;
                        MyFixedPoint? amount = null;
                        nullable = null;
                        this.GetInventory(0).RemoveItems(item.ItemId, amount, true, false, nullable);
                    }
                }
            }
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyGasGenerator>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyGasGenerator> switch1 = new MyTerminalControlOnOffSwitch<MyGasGenerator>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MyGasGenerator> switch2 = new MyTerminalControlOnOffSwitch<MyGasGenerator>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                switch2.Getter = x => x.UseConveyorSystem;
                MyTerminalControlOnOffSwitch<MyGasGenerator> local11 = switch2;
                MyTerminalControlOnOffSwitch<MyGasGenerator> local12 = switch2;
                local12.Setter = (x, v) => x.UseConveyorSystem = v;
                MyTerminalControlOnOffSwitch<MyGasGenerator> onOff = local12;
                onOff.EnableToggleAction<MyGasGenerator>();
                MyTerminalControlFactory.AddControl<MyGasGenerator>(onOff);
                MyTerminalControlButton<MyGasGenerator> button1 = new MyTerminalControlButton<MyGasGenerator>("Refill", MySpaceTexts.BlockPropertyTitle_Refill, MySpaceTexts.BlockPropertyTitle_Refill, new Action<MyGasGenerator>(MyGasGenerator.OnRefillButtonPressed));
                MyTerminalControlButton<MyGasGenerator> button2 = new MyTerminalControlButton<MyGasGenerator>("Refill", MySpaceTexts.BlockPropertyTitle_Refill, MySpaceTexts.BlockPropertyTitle_Refill, new Action<MyGasGenerator>(MyGasGenerator.OnRefillButtonPressed));
                button2.Enabled = x => x.CanRefill();
                on = null;
                MyTerminalControlButton<MyGasGenerator> button = button2;
                button.EnableAction<MyGasGenerator>(null, on, null);
                MyTerminalControlFactory.AddControl<MyGasGenerator>(button);
                on = null;
                on = null;
                MyTerminalControlCheckbox<MyGasGenerator> checkbox1 = new MyTerminalControlCheckbox<MyGasGenerator>("Auto-Refill", MySpaceTexts.BlockPropertyTitle_AutoRefill, MySpaceTexts.BlockPropertyTitle_AutoRefill, on, on);
                MyTerminalControlCheckbox<MyGasGenerator> checkbox2 = new MyTerminalControlCheckbox<MyGasGenerator>("Auto-Refill", MySpaceTexts.BlockPropertyTitle_AutoRefill, MySpaceTexts.BlockPropertyTitle_AutoRefill, on, on);
                checkbox2.Getter = x => x.AutoRefill;
                MyTerminalControlCheckbox<MyGasGenerator> local9 = checkbox2;
                MyTerminalControlCheckbox<MyGasGenerator> local10 = checkbox2;
                local10.Setter = (x, v) => x.AutoRefill = v;
                MyTerminalControlCheckbox<MyGasGenerator> checkbox = local10;
                checkbox.EnableAction<MyGasGenerator>(null);
                MyTerminalControlFactory.AddControl<MyGasGenerator>(checkbox);
            }
        }

        private double GasOutputPerSecond(ref MyDefinitionId gasId) => 
            ((double) (this.SourceComp.CurrentOutputByType(gasId) * ((Sandbox.ModAPI.IMyGasGenerator) this).ProductionCapacityMultiplier));

        private double GasOutputPerUpdate(ref MyDefinitionId gasId) => 
            (this.GasOutputPerSecond(ref gasId) * 0.01666666753590107);

        private double GasToIce(ref MyDefinitionId gasId, double gasAmount) => 
            (gasAmount / this.IceToGasRatio(ref gasId));

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_OxygenGenerator objectBuilderCubeBlock = (MyObjectBuilder_OxygenGenerator) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Inventory = this.GetInventory(0).GetObjectBuilder();
            objectBuilderCubeBlock.UseConveyorSystem = (bool) this.m_useConveyorSystem;
            objectBuilderCubeBlock.AutoRefill = this.AutoRefill;
            return objectBuilderCubeBlock;
        }

        public PullInformation GetPullInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.Inventory = this.GetInventory(0);
            information1.OwnerID = base.OwnerId;
            information1.Constraint = information1.Inventory.Constraint;
            return information1;
        }

        public PullInformation GetPushInformation() => 
            null;

        private bool HasIce()
        {
            if (MySession.Static.CreativeMode)
            {
                return true;
            }
            using (List<MyPhysicalInventoryItem>.Enumerator enumerator = this.GetInventory(0).GetItems().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (!(enumerator.Current.Content is MyObjectBuilder_GasContainerObject))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private float IceAmount()
        {
            if (MySession.Static.CreativeMode)
            {
                return 10000f;
            }
            MyFixedPoint point = 0;
            foreach (MyPhysicalInventoryItem item in this.GetInventory(0).GetItems())
            {
                if (!(item.Content is MyObjectBuilder_GasContainerObject))
                {
                    point += item.Amount;
                }
            }
            return (float) point;
        }

        private double IceToGas(ref MyDefinitionId gasId, double iceAmount) => 
            (iceAmount * this.IceToGasRatio(ref gasId));

        private double IceToGasRatio(ref MyDefinitionId gasId) => 
            ((double) (this.SourceComp.DefinedOutputByType(gasId) / this.BlockDefinition.IceConsumptionPerSecond));

        public override unsafe void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            List<MyResourceSourceInfo> sourceResourceData = new List<MyResourceSourceInfo>();
            foreach (MyOxygenGeneratorDefinition.MyGasGeneratorResourceInfo info in this.BlockDefinition.ProducedGases)
            {
                MyResourceSourceInfo* infoPtr1;
                MyResourceSourceInfo item = new MyResourceSourceInfo {
                    ResourceTypeId = info.Id
                };
                infoPtr1->DefinedOutput = (this.BlockDefinition.IceConsumptionPerSecond * info.IceToGasRatio) * (MySession.Static.CreativeMode ? 10f : 1f);
                infoPtr1 = (MyResourceSourceInfo*) ref item;
                item.ProductionToCapacityMultiplier = 1f;
                sourceResourceData.Add(item);
            }
            this.SourceComp.Init(this.BlockDefinition.ResourceSourceGroup, sourceResourceData);
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_OxygenGenerator generator = objectBuilder as MyObjectBuilder_OxygenGenerator;
            this.InitializeConveyorEndpoint();
            this.m_useConveyorSystem.SetLocalValue(generator.UseConveyorSystem);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            MyInventory component = this.GetInventory(0);
            if (component != null)
            {
                component.Constraint = this.BlockDefinition.InputInventoryConstraint;
            }
            else
            {
                component = new MyInventory(this.BlockDefinition.InventoryMaxVolume, this.BlockDefinition.InventorySize, MyInventoryFlags.CanReceive) {
                    Constraint = this.BlockDefinition.InputInventoryConstraint
                };
                base.Components.Add<MyInventoryBase>(component);
            }
            this.m_oreConstraint = new MyInventoryConstraint(component.Constraint.Description, component.Constraint.Icon, component.Constraint.IsWhitelist);
            foreach (MyDefinitionId id in component.Constraint.ConstrainedIds)
            {
                if (id.TypeId != typeof(MyObjectBuilder_GasContainerObject))
                {
                    this.m_oreConstraint.Add(id);
                }
            }
            if (MyFakes.ENABLE_INVENTORY_FIX)
            {
                base.FixSingleInventory();
            }
            if (component != null)
            {
                component.Init(generator.Inventory);
            }
            this.AutoRefill = generator.AutoRefill;
            this.SourceComp.Enabled = base.Enabled;
            if (Sync.IsServer)
            {
                this.SourceComp.OutputChanged += new MyResourceOutputChangedDelegate(this.Source_OutputChanged);
            }
            float num = this.IceAmount();
            foreach (MyDefinitionId id2 in this.SourceComp.ResourceTypes)
            {
                MyDefinitionId gasId = id2;
                this.m_sourceComp.SetRemainingCapacityByType(id2, (float) this.IceToGas(ref gasId, (double) num));
            }
            MyResourceSinkInfo sinkData = new MyResourceSinkInfo {
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                MaxRequiredInput = this.BlockDefinition.OperationalPowerConsumption,
                RequiredInputFunc = new Func<float>(this.ComputeRequiredPower)
            };
            base.ResourceSink.Init(this.BlockDefinition.ResourceSinkGroup, sinkData);
            base.ResourceSink.IsPoweredChanged += new Action(this.PowerReceiver_IsPoweredChanged);
            base.ResourceSink.Update();
            this.UpdateText();
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_conveyorEndpoint));
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyGasGenerator_IsWorkingChanged);
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
        }

        private void Inventory_ContentsChanged(MyInventoryBase obj)
        {
            float num = this.IceAmount();
            if (base.ResourceSink.CurrentInput < base.ResourceSink.RequiredInput)
            {
                foreach (MyDefinitionId id in this.SourceComp.ResourceTypes)
                {
                    this.m_sourceComp.SetRemainingCapacityByType(id, 0f);
                }
            }
            else
            {
                foreach (MyDefinitionId id2 in this.SourceComp.ResourceTypes)
                {
                    MyDefinitionId gasId = id2;
                    this.m_sourceComp.SetRemainingCapacityByType(id2, (float) this.IceToGas(ref gasId, (double) num));
                }
            }
            base.RaisePropertiesChanged();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        private void MyGasGenerator_IsWorkingChanged(MyCubeBlock obj)
        {
            MySandboxGame.Static.Invoke(delegate {
                if (!base.Closed)
                {
                    this.SourceComp.Enabled = this.CanProduce;
                    this.SetEmissiveStateWorking();
                    base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                }
            }, "MyGasGenerator_IsWorkingChanged");
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), true);
            base.OnDestroy();
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            this.SourceComp.Enabled = this.CanProduce;
            base.ResourceSink.Update();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        protected override void OnInventoryComponentAdded(MyInventoryBase inventory)
        {
            base.OnInventoryComponentAdded(inventory);
            if ((this.GetInventory(0) != null) && MyPerGameSettings.InventoryMass)
            {
                this.GetInventory(0).ContentsChanged += new Action<MyInventoryBase>(this.Inventory_ContentsChanged);
            }
        }

        protected override void OnInventoryComponentRemoved(MyInventoryBase inventory)
        {
            base.OnInventoryComponentRemoved(inventory);
            MyInventory inventory2 = inventory as MyInventory;
            if ((inventory2 != null) && MyPerGameSettings.InventoryMass)
            {
                inventory2.ContentsChanged -= new Action<MyInventoryBase>(this.Inventory_ContentsChanged);
            }
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            this.CheckEmissiveState(true);
        }

        private static void OnRefillButtonPressed(MyGasGenerator generator)
        {
            if (generator.IsWorking)
            {
                generator.SendRefillRequest();
            }
        }

        [Event(null, 0x2ff), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void OnRefillCallback()
        {
            this.RefillBottles();
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        private void PowerReceiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        private void ProduceGas(ref MyDefinitionId gasId, double gasAmount)
        {
            if (gasAmount > 0.0)
            {
                double iceAmount = this.GasToIce(ref gasId, gasAmount);
                this.ConsumeFuel(ref gasId, iceAmount);
            }
        }

        public void RefillBottles()
        {
            List<MyPhysicalInventoryItem> items = this.GetInventory(0).GetItems();
            using (List<MyDefinitionId>.Enumerator enumerator = this.SourceComp.ResourceTypes.GetEnumerator())
            {
                MyDefinitionId id2;
                double num2;
                goto TR_001D;
            TR_0003:
                if (num2 > 0.0)
                {
                    this.ProduceGas(ref id2, num2);
                    this.GetInventory(0).UpdateGasAmount();
                }
            TR_001D:
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyDefinitionId current = enumerator.Current;
                    id2 = current;
                    double num = 0.0;
                    if (MySession.Static.CreativeMode)
                    {
                        num = 3.4028234663852886E+38;
                    }
                    else
                    {
                        foreach (MyPhysicalInventoryItem item in items)
                        {
                            if (!(item.Content is MyObjectBuilder_GasContainerObject))
                            {
                                num += this.IceToGas(ref id2, (double) ((float) item.Amount)) * ((Sandbox.ModAPI.IMyGasGenerator) this).ProductionCapacityMultiplier;
                            }
                        }
                    }
                    num2 = 0.0;
                    using (List<MyPhysicalInventoryItem>.Enumerator enumerator2 = items.GetEnumerator())
                    {
                        while (true)
                        {
                            if (enumerator2.MoveNext())
                            {
                                MyPhysicalInventoryItem item2 = enumerator2.Current;
                                if (num > 0.0)
                                {
                                    MyObjectBuilder_GasContainerObject content = item2.Content as MyObjectBuilder_GasContainerObject;
                                    if (content == null)
                                    {
                                        continue;
                                    }
                                    if (content.GasLevel >= 1f)
                                    {
                                        continue;
                                    }
                                    MyOxygenContainerDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(content) as MyOxygenContainerDefinition;
                                    if (physicalItemDefinition.StoredGasId != current)
                                    {
                                        continue;
                                    }
                                    float num3 = content.GasLevel * physicalItemDefinition.Capacity;
                                    double num4 = Math.Min((double) (physicalItemDefinition.Capacity - num3), num);
                                    content.GasLevel = (float) Math.Min((double) ((num3 + num4) / ((double) physicalItemDefinition.Capacity)), (double) 1.0);
                                    num2 += num4;
                                    num -= num4;
                                    continue;
                                }
                            }
                            else
                            {
                                goto TR_0003;
                            }
                            break;
                        }
                        break;
                    }
                    goto TR_0003;
                }
            }
        }

        bool IMyGasBlock.IsWorking() => 
            this.CanProduce;

        public void SendRefillRequest()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyGasGenerator>(this, x => new Action(x.OnRefillCallback), targetEndpoint);
        }

        public override bool SetEmissiveStateWorking()
        {
            if (!this.CanProduce)
            {
                return false;
            }
            if (this.GetInventory(0) == null)
            {
                return false;
            }
            return ((this.GetInventory(0).FindItem(item => !(item.Content is MyObjectBuilder_GasContainerObject)) != null) ? (!this.m_isProducing ? base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Working, base.Render.RenderObjectIDs[0], null) : base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Alternative, base.Render.RenderObjectIDs[0], null)) : base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Warning, base.Render.RenderObjectIDs[0], null));
        }

        public void SetInventory(MyInventory inventory, int index)
        {
            throw new NotImplementedException("TODO Dusan inventory sync");
        }

        private void Source_OutputChanged(MyDefinitionId changedResourceId, float oldOutput, MyResourceSourceComponent source)
        {
            if (!this.BlockDefinition.ProducedGases.TrueForAll(info => info.Id != changedResourceId))
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            base.ResourceSink.Update();
            if ((MySession.Static != null) && !MySession.Static.Settings.EnableOxygen)
            {
                this.m_sourceComp.SetMaxOutputByType(this.m_oxygenGasId, 0f);
            }
            if (base.ResourceSink.CurrentInput < base.ResourceSink.RequiredInput)
            {
                foreach (MyDefinitionId id in this.SourceComp.ResourceTypes)
                {
                    this.m_sourceComp.SetRemainingCapacityByType(id, 0f);
                }
            }
            else
            {
                foreach (MyDefinitionId id2 in this.SourceComp.ResourceTypes)
                {
                    MyDefinitionId gasId = id2;
                    this.m_sourceComp.SetRemainingCapacityByType(id2, (float) this.IceToGas(ref gasId, (double) this.IceAmount()));
                }
            }
            foreach (MyDefinitionId id4 in this.SourceComp.ResourceTypes)
            {
                double gasAmount = this.GasOutputPerUpdate(ref id4);
                this.ProduceGas(ref id4, gasAmount);
            }
            this.SetEmissiveStateWorking();
            if (MyFakes.ENABLE_OXYGEN_SOUNDS)
            {
                this.UpdateSounds();
            }
            this.m_isProducing = false;
            foreach (MyDefinitionId id5 in this.SourceComp.ResourceTypes)
            {
                this.m_isProducing |= this.SourceComp.CurrentOutputByType(id5) > 0f;
            }
            if (!this.m_isProducing && !base.HasDamageEffect)
            {
                base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        public override void UpdateAfterSimulation100()
        {
            if (Sync.IsServer && base.IsWorking)
            {
                if ((this.m_useConveyorSystem != null) && (this.GetInventory(0).VolumeFillFactor < 0.6f))
                {
                    MyFixedPoint? maxAmount = null;
                    MyGridConveyorSystem.PullAllRequest(this, this.GetInventory(0), base.OwnerId, this.HasIce() ? this.GetInventory(0).Constraint : this.m_oreConstraint, maxAmount, false);
                }
                if (this.AutoRefill && this.CanRefill())
                {
                    this.RefillBottles();
                }
            }
            this.m_isProducing = true;
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        private void UpdateSounds()
        {
            if (base.m_soundEmitter != null)
            {
                if (!base.IsWorking)
                {
                    if (base.m_soundEmitter.IsPlaying)
                    {
                        base.m_soundEmitter.StopSound(false, true);
                    }
                }
                else
                {
                    bool? nullable;
                    if (!this.m_isProducing)
                    {
                        if (((base.m_soundEmitter.SoundId != this.BlockDefinition.IdleSound.Arcade) && ((base.m_soundEmitter.SoundId != this.BlockDefinition.IdleSound.Realistic) && ((base.m_soundEmitter.SoundId == this.BlockDefinition.GenerateSound.Arcade) || (base.m_soundEmitter.SoundId == this.BlockDefinition.GenerateSound.Realistic)))) && base.m_soundEmitter.Loop)
                        {
                            base.m_soundEmitter.StopSound(false, true);
                        }
                    }
                    else if ((base.m_soundEmitter.SoundId != this.BlockDefinition.GenerateSound.Arcade) && (base.m_soundEmitter.SoundId != this.BlockDefinition.GenerateSound.Realistic))
                    {
                        nullable = null;
                        base.m_soundEmitter.PlaySound(this.BlockDefinition.GenerateSound, true, false, false, false, false, nullable);
                    }
                    if (!base.m_soundEmitter.IsPlaying)
                    {
                        nullable = null;
                        base.m_soundEmitter.PlaySound(this.BlockDefinition.IdleSound, true, false, false, false, false, nullable);
                    }
                }
            }
        }

        private void UpdateText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(this.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            if (!MySession.Static.Settings.EnableOxygen)
            {
                base.DetailedInfo.Append("\n");
                base.DetailedInfo.Append("Oxygen disabled in world settings!");
            }
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.SetEmissiveStateWorking();
        }

        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        public bool CanPressurizeRoom =>
            false;

        public bool CanProduce =>
            ((((MySession.Static != null) && MySession.Static.Settings.EnableOxygen) || !this.BlockDefinition.ProducedGases.TrueForAll(info => info.Id == this.m_oxygenGasId)) && (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && (base.IsWorking && (base.Enabled && base.IsFunctional))));

        public bool AutoRefill
        {
            get => 
                ((bool) this.m_autoRefill);
            set => 
                (this.m_autoRefill.Value = value);
        }

        public MyResourceSourceComponent SourceComp
        {
            get => 
                this.m_sourceComp;
            set
            {
                if (base.Components.Contains(typeof(MyResourceSourceComponent)))
                {
                    base.Components.Remove<MyResourceSourceComponent>();
                }
                base.Components.Add<MyResourceSourceComponent>(value);
                this.m_sourceComp = value;
            }
        }

        private MyOxygenGeneratorDefinition BlockDefinition =>
            ((MyOxygenGeneratorDefinition) base.BlockDefinition);

        public bool UseConveyorSystem
        {
            get => 
                ((bool) this.m_useConveyorSystem);
            set => 
                (this.m_useConveyorSystem.Value = value);
        }

        float Sandbox.ModAPI.IMyGasGenerator.ProductionCapacityMultiplier
        {
            get => 
                this.m_productionCapacityMultiplier;
            set
            {
                this.m_productionCapacityMultiplier = value;
                if (this.m_productionCapacityMultiplier < 0.01f)
                {
                    this.m_productionCapacityMultiplier = 0.01f;
                }
            }
        }

        float Sandbox.ModAPI.IMyGasGenerator.PowerConsumptionMultiplier
        {
            get => 
                this.m_powerConsumptionMultiplier;
            set
            {
                this.m_powerConsumptionMultiplier = value;
                if (this.m_powerConsumptionMultiplier < 0.01f)
                {
                    this.m_powerConsumptionMultiplier = 0.01f;
                }
                if (base.ResourceSink != null)
                {
                    base.ResourceSink.SetMaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId, this.BlockDefinition.OperationalPowerConsumption * this.m_powerConsumptionMultiplier);
                    base.ResourceSink.Update();
                }
            }
        }

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

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGasGenerator.<>c <>9 = new MyGasGenerator.<>c();
            public static MyTerminalValueControl<MyGasGenerator, bool>.GetterDelegate <>9__22_0;
            public static MyTerminalValueControl<MyGasGenerator, bool>.SetterDelegate <>9__22_1;
            public static Func<MyGasGenerator, bool> <>9__22_2;
            public static MyTerminalValueControl<MyGasGenerator, bool>.GetterDelegate <>9__22_3;
            public static MyTerminalValueControl<MyGasGenerator, bool>.SetterDelegate <>9__22_4;
            public static Func<MyPhysicalInventoryItem, bool> <>9__42_0;
            public static Func<MyGasGenerator, Action> <>9__71_0;

            internal bool <CreateTerminalControls>b__22_0(MyGasGenerator x) => 
                x.UseConveyorSystem;

            internal void <CreateTerminalControls>b__22_1(MyGasGenerator x, bool v)
            {
                x.UseConveyorSystem = v;
            }

            internal bool <CreateTerminalControls>b__22_2(MyGasGenerator x) => 
                x.CanRefill();

            internal bool <CreateTerminalControls>b__22_3(MyGasGenerator x) => 
                x.AutoRefill;

            internal void <CreateTerminalControls>b__22_4(MyGasGenerator x, bool v)
            {
                x.AutoRefill = v;
            }

            internal Action <SendRefillRequest>b__71_0(MyGasGenerator x) => 
                new Action(x.OnRefillCallback);

            internal bool <SetEmissiveStateWorking>b__42_0(MyPhysicalInventoryItem item) => 
                !(item.Content is MyObjectBuilder_GasContainerObject);
        }
    }
}

