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
    using Sandbox.Game.Entities.Interfaces;
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
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Graphics;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_OxygenTank)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyGasTank), typeof(Sandbox.ModAPI.Ingame.IMyGasTank), typeof(Sandbox.ModAPI.IMyOxygenTank), typeof(Sandbox.ModAPI.Ingame.IMyOxygenTank) })]
    public class MyGasTank : MyFunctionalBlock, IMyGasBlock, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyOxygenTank, Sandbox.ModAPI.IMyGasTank, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyGasTank, Sandbox.ModAPI.Ingame.IMyOxygenTank, IMyInventoryOwner, Sandbox.Game.Entities.Interfaces.IMyGasTank
    {
        private static readonly string[] m_emissiveTextureNames = new string[] { "Emissive0", "Emissive1", "Emissive2", "Emissive3" };
        private Color m_prevColor = Color.White;
        private int m_prevFillCount = -1;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_autoRefill;
        private const float m_maxFillPerSecond = 0.05f;
        private MyMultilineConveyorEndpoint m_conveyorEndpoint;
        private bool m_isStockpiling;
        private MyResourceSourceComponent m_sourceComp;
        private readonly MyDefinitionId m_oxygenGasId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");

        public MyGasTank()
        {
            this.CreateTerminalControls();
            this.SourceComp = new MyResourceSourceComponent(1);
            base.ResourceSink = new MyResourceSinkComponent(2);
            this.m_autoRefill.ValueChanged += x => this.OnAutoRefillChanged();
        }

        public bool AllowSelfPulling() => 
            false;

        private bool CanRefill()
        {
            if ((this.CanStore && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId)) && (this.FilledRatio != 0.0))
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

        internal void ChangeFilledRatio(double newFilledRatio, bool updateSync = false)
        {
            double filledRatio = this.FilledRatio;
            if ((filledRatio != newFilledRatio) || MySession.Static.CreativeMode)
            {
                if (updateSync)
                {
                    this.ChangeFillRatioAmount(newFilledRatio);
                }
                else
                {
                    this.FilledRatio = newFilledRatio;
                    if (!MySession.Static.CreativeMode || (newFilledRatio <= filledRatio))
                    {
                        this.SourceComp.SetRemainingCapacityByType(this.BlockDefinition.StoredGasId, (float) (this.FilledRatio * this.Capacity));
                    }
                    else
                    {
                        this.SourceComp.SetRemainingCapacityByType(this.BlockDefinition.StoredGasId, this.Capacity);
                    }
                    base.ResourceSink.Update();
                    this.UpdateEmissivity();
                    this.UpdateText();
                }
            }
        }

        public void ChangeFillRatioAmount(double newFilledRatio)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyGasTank, double>(this, x => new Action<double>(x.OnFilledRatioCallback), newFilledRatio, targetEndpoint);
        }

        public void ChangeStockpileMode(bool newStockpileMode)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyGasTank, bool>(this, x => new Action<bool>(x.OnStockipleModeCallback), newStockpileMode, targetEndpoint);
            this.UpdateEmissivity();
        }

        protected override bool CheckIsWorking() => 
            (base.CheckIsWorking() && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId));

        private void ComponentStack_IsFunctionalChanged()
        {
            this.SourceComp.Enabled = this.CanStore;
            base.ResourceSink.Update();
            this.FilledRatio = 0.0;
            if (MySession.Static.CreativeMode)
            {
                this.SourceComp.SetRemainingCapacityByType(this.BlockDefinition.StoredGasId, this.Capacity);
            }
            else
            {
                this.SourceComp.SetRemainingCapacityByType(this.BlockDefinition.StoredGasId, (float) (this.FilledRatio * this.Capacity));
            }
            if (((base.CubeGrid != null) && (base.CubeGrid.GridSystems != null)) && (base.CubeGrid.GridSystems.ResourceDistributor != null))
            {
                base.CubeGrid.GridSystems.ResourceDistributor.ConveyorSystem_OnPoweredChanged();
            }
            this.UpdateText();
            this.UpdateEmissivity();
        }

        private float ComputeRequiredGas()
        {
            if (!this.CanStore)
            {
                return 0f;
            }
            float num = this.SourceComp.CurrentOutputByType(this.BlockDefinition.StoredGasId);
            return Math.Min((float) ((((float) (((1.0 - this.FilledRatio) * 60.0) * this.SourceComp.ProductionToCapacityMultiplierByType(this.BlockDefinition.StoredGasId))) * this.Capacity) + num), (float) (0.05f * this.Capacity));
        }

        private float ComputeRequiredPower()
        {
            if (((!MySession.Static.Settings.EnableOxygen && (this.BlockDefinition.StoredGasId == this.m_oxygenGasId)) || !base.Enabled) || !base.IsFunctional)
            {
                return 0f;
            }
            if ((this.SourceComp.CurrentOutputByType(this.BlockDefinition.StoredGasId) > 0f) || (base.ResourceSink.CurrentInputByType(this.BlockDefinition.StoredGasId) > 0f))
            {
                return this.BlockDefinition.OperationalPowerConsumption;
            }
            return this.BlockDefinition.StandbyPowerConsumption;
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyGasTank>())
            {
                base.CreateTerminalControls();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyGasTank> switch1 = new MyTerminalControlOnOffSwitch<MyGasTank>("Stockpile", MySpaceTexts.BlockPropertyTitle_Stockpile, MySpaceTexts.BlockPropertyDescription_Stockpile, on, on);
                MyTerminalControlOnOffSwitch<MyGasTank> switch2 = new MyTerminalControlOnOffSwitch<MyGasTank>("Stockpile", MySpaceTexts.BlockPropertyTitle_Stockpile, MySpaceTexts.BlockPropertyDescription_Stockpile, on, on);
                switch2.Getter = x => x.IsStockpiling;
                MyTerminalControlOnOffSwitch<MyGasTank> local11 = switch2;
                MyTerminalControlOnOffSwitch<MyGasTank> local12 = switch2;
                local12.Setter = (x, v) => x.ChangeStockpileMode(v);
                MyTerminalControlOnOffSwitch<MyGasTank> onOff = local12;
                onOff.EnableToggleAction<MyGasTank>();
                onOff.EnableOnOffActions<MyGasTank>();
                MyTerminalControlFactory.AddControl<MyGasTank>(onOff);
                MyTerminalControlButton<MyGasTank> button1 = new MyTerminalControlButton<MyGasTank>("Refill", MySpaceTexts.BlockPropertyTitle_Refill, MySpaceTexts.BlockPropertyTitle_Refill, new Action<MyGasTank>(MyGasTank.OnRefillButtonPressed));
                MyTerminalControlButton<MyGasTank> button2 = new MyTerminalControlButton<MyGasTank>("Refill", MySpaceTexts.BlockPropertyTitle_Refill, MySpaceTexts.BlockPropertyTitle_Refill, new Action<MyGasTank>(MyGasTank.OnRefillButtonPressed));
                button2.Enabled = x => x.CanRefill();
                on = null;
                MyTerminalControlButton<MyGasTank> button = button2;
                button.EnableAction<MyGasTank>(null, on, null);
                MyTerminalControlFactory.AddControl<MyGasTank>(button);
                on = null;
                on = null;
                MyTerminalControlCheckbox<MyGasTank> checkbox1 = new MyTerminalControlCheckbox<MyGasTank>("Auto-Refill", MySpaceTexts.BlockPropertyTitle_AutoRefill, MySpaceTexts.BlockPropertyTitle_AutoRefill, on, on);
                MyTerminalControlCheckbox<MyGasTank> checkbox2 = new MyTerminalControlCheckbox<MyGasTank>("Auto-Refill", MySpaceTexts.BlockPropertyTitle_AutoRefill, MySpaceTexts.BlockPropertyTitle_AutoRefill, on, on);
                checkbox2.Getter = x => (bool) x.m_autoRefill;
                MyTerminalControlCheckbox<MyGasTank> local9 = checkbox2;
                MyTerminalControlCheckbox<MyGasTank> local10 = checkbox2;
                local10.Setter = (x, v) => x.m_autoRefill.Value = v;
                MyTerminalControlCheckbox<MyGasTank> checkbox = local10;
                checkbox.EnableAction<MyGasTank>(null);
                MyTerminalControlFactory.AddControl<MyGasTank>(checkbox);
            }
        }

        internal void Drain(double amount)
        {
            if (amount != 0.0)
            {
                this.ChangeFilledRatio(Math.Max((double) 0.0, (double) (this.FilledRatio - (amount / ((double) this.Capacity)))), Sync.IsServer);
            }
        }

        private void ExecuteGasTransfer()
        {
            float num = this.GasInputPerUpdate - this.GasOutputPerUpdate;
            if (num != 0f)
            {
                this.Transfer((double) num);
                base.ResourceSink.Update();
                this.SourceComp.OnProductionEnabledChanged(new MyDefinitionId?(this.BlockDefinition.StoredGasId));
            }
            else if (!base.HasDamageEffect)
            {
                base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        internal void Fill(double amount)
        {
            if (amount != 0.0)
            {
                this.ChangeFilledRatio(Math.Min((double) 1.0, (double) (this.FilledRatio + (amount / ((double) this.Capacity)))), Sync.IsServer);
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_OxygenTank objectBuilderCubeBlock = (MyObjectBuilder_OxygenTank) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.IsStockpiling = this.IsStockpiling;
            objectBuilderCubeBlock.FilledRatio = (float) this.FilledRatio;
            objectBuilderCubeBlock.AutoRefill = (bool) this.m_autoRefill;
            objectBuilderCubeBlock.Inventory = this.GetInventory(0).GetObjectBuilder();
            return objectBuilderCubeBlock;
        }

        public double GetOxygenLevel() => 
            this.FilledRatio;

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

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_OxygenTank tank = (MyObjectBuilder_OxygenTank) objectBuilder;
            this.InitializeConveyorEndpoint();
            if (Sync.IsServer)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            }
            if (MyFakes.ENABLE_INVENTORY_FIX)
            {
                base.FixSingleInventory();
                if (this.GetInventory(0) != null)
                {
                    this.GetInventory(0).Constraint = this.BlockDefinition.InputInventoryConstraint;
                }
            }
            MyInventory component = this.GetInventory(0);
            if (component == null)
            {
                component = new MyInventory(this.BlockDefinition.InventoryMaxVolume, this.BlockDefinition.InventorySize, MyInventoryFlags.CanReceive) {
                    Constraint = this.BlockDefinition.InputInventoryConstraint
                };
                base.Components.Add<MyInventoryBase>(component);
                component.Init(tank.Inventory);
            }
            component.ContentsChanged += new Action<MyInventoryBase>(this.MyGasTank_ContentsChanged);
            this.m_autoRefill.SetLocalValue(tank.AutoRefill);
            MyResourceSourceInfo item = new MyResourceSourceInfo {
                ResourceTypeId = this.BlockDefinition.StoredGasId,
                DefinedOutput = 0.05f * this.BlockDefinition.Capacity
            };
            List<MyResourceSourceInfo> list1 = new List<MyResourceSourceInfo>();
            list1.Add(item);
            List<MyResourceSourceInfo> sourceResourceData = list1;
            this.SourceComp.Init(this.BlockDefinition.ResourceSourceGroup, sourceResourceData);
            this.SourceComp.OutputChanged += new MyResourceOutputChangedDelegate(this.Source_OutputChanged);
            this.SourceComp.Enabled = base.Enabled;
            this.IsStockpiling = tank.IsStockpiling;
            MyResourceSinkInfo info2 = new MyResourceSinkInfo {
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                MaxRequiredInput = this.BlockDefinition.OperationalPowerConsumption,
                RequiredInputFunc = new Func<float>(this.ComputeRequiredPower)
            };
            List<MyResourceSinkInfo> list3 = new List<MyResourceSinkInfo>();
            list3.Add(info2);
            info2 = new MyResourceSinkInfo {
                ResourceTypeId = this.BlockDefinition.StoredGasId,
                MaxRequiredInput = this.Capacity,
                RequiredInputFunc = new Func<float>(this.ComputeRequiredGas)
            };
            list3.Add(info2);
            List<MyResourceSinkInfo> sinkData = list3;
            base.ResourceSink.Init(this.BlockDefinition.ResourceSinkGroup, sinkData);
            base.ResourceSink.IsPoweredChanged += new Action(this.PowerReceiver_IsPoweredChanged);
            base.ResourceSink.CurrentInputChanged += new MyCurrentResourceInputChangedDelegate(this.Sink_CurrentInputChanged);
            float filledRatio = tank.FilledRatio;
            if (MySession.Static.CreativeMode && (filledRatio == 0f))
            {
                filledRatio = 0.5f;
            }
            this.ChangeFilledRatio((double) MathHelper.Clamp(filledRatio, 0f, 1f), false);
            base.ResourceSink.Update();
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_conveyorEndpoint));
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyOxygenTank_IsWorkingChanged);
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
        }

        public bool IsResourceStorage(MyDefinitionId resourceDefinition) => 
            this.SourceComp.ResourceTypes.Any<MyDefinitionId>(x => (x == resourceDefinition));

        private void m_inventory_ContentsChanged(MyInventoryBase obj)
        {
            base.RaisePropertiesChanged();
        }

        private void MyGasTank_ContentsChanged(MyInventoryBase obj)
        {
            if ((this.m_autoRefill != null) && this.CanRefill())
            {
                this.RefillBottles();
            }
        }

        private void MyOxygenTank_IsWorkingChanged(MyCubeBlock obj)
        {
            this.SourceComp.Enabled = this.CanStore;
            this.SetStockpilingState(this.m_isStockpiling);
            this.UpdateEmissivity();
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            this.m_prevColor = Color.White;
            this.m_prevFillCount = -1;
            this.UpdateEmissivity();
            this.UpdateText();
        }

        private void OnAutoRefillChanged()
        {
            if ((Sync.IsServer && (this.m_autoRefill != null)) && this.CanRefill())
            {
                this.RefillBottles();
            }
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), true);
            base.OnDestroy();
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            this.SourceComp.Enabled = this.CanStore;
            base.ResourceSink.Update();
            this.UpdateEmissivity();
        }

        [Event(null, 720), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnFilledRatioCallback(double newFilledRatio)
        {
            this.ChangeFilledRatio(newFilledRatio, false);
        }

        protected override void OnInventoryComponentAdded(MyInventoryBase inventory)
        {
            base.OnInventoryComponentAdded(inventory);
            if ((this.GetInventory(0) != null) && MyPerGameSettings.InventoryMass)
            {
                this.GetInventory(0).ContentsChanged += new Action<MyInventoryBase>(this.m_inventory_ContentsChanged);
            }
        }

        protected override void OnInventoryComponentRemoved(MyInventoryBase inventory)
        {
            base.OnInventoryComponentRemoved(inventory);
            MyInventory inventory2 = inventory as MyInventory;
            if ((inventory2 != null) && MyPerGameSettings.InventoryMass)
            {
                inventory2.ContentsChanged -= new Action<MyInventoryBase>(this.m_inventory_ContentsChanged);
            }
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            this.m_prevFillCount = -1;
        }

        private static void OnRefillButtonPressed(MyGasTank tank)
        {
            if (tank.IsWorking)
            {
                tank.SendRefillRequest();
            }
        }

        [Event(null, 0x2db), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void OnRefillCallback()
        {
            this.RefillBottles();
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        [Event(null, 0x2bf), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnStockipleModeCallback(bool newStockpileMode)
        {
            this.IsStockpiling = newStockpileMode;
        }

        private void PowerReceiver_IsPoweredChanged()
        {
            MySandboxGame.Static.Invoke(delegate {
                if (!base.Closed)
                {
                    base.UpdateIsWorking();
                    this.UpdateEmissivity();
                }
            }, "MyGasTank::PowerReceiver_IsPoweredChanged");
        }

        public void RefillBottles()
        {
            bool flag = false;
            double filledRatio = this.FilledRatio;
            foreach (MyPhysicalInventoryItem item in this.GetInventory(0).GetItems())
            {
                if (filledRatio <= 0.0)
                {
                    break;
                }
                MyObjectBuilder_GasContainerObject content = item.Content as MyObjectBuilder_GasContainerObject;
                if ((content != null) && (content.GasLevel < 1f))
                {
                    MyOxygenContainerDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(content) as MyOxygenContainerDefinition;
                    float num2 = content.GasLevel * physicalItemDefinition.Capacity;
                    float num3 = (float) (filledRatio * this.Capacity);
                    float num4 = Math.Min(physicalItemDefinition.Capacity - num2, num3);
                    content.GasLevel = Math.Min((float) ((num2 + num4) / physicalItemDefinition.Capacity), (float) 1f);
                    filledRatio = Math.Max((double) (filledRatio - (num4 / this.Capacity)), (double) 0.0);
                    flag = true;
                }
            }
            if (flag)
            {
                this.ChangeFilledRatio(filledRatio, true);
                this.GetInventory(0).UpdateGasAmount();
            }
        }

        bool IMyGasBlock.IsWorking() => 
            this.CanStore;

        void Sandbox.ModAPI.Ingame.IMyGasTank.RefillBottles()
        {
            if (base.IsWorking)
            {
                this.SendRefillRequest();
            }
        }

        public void SendRefillRequest()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyGasTank>(this, x => new Action(x.OnRefillCallback), targetEndpoint);
        }

        private void SetEmissive(Color color, float fill)
        {
            int num = (int) (fill * m_emissiveTextureNames.Length);
            if ((base.Render.RenderObjectIDs[0] != uint.MaxValue) && ((color != this.m_prevColor) || (num != this.m_prevFillCount)))
            {
                for (int i = 0; i < m_emissiveTextureNames.Length; i++)
                {
                    UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[i], (i < num) ? color : Color.Black, 1f);
                }
                this.m_prevColor = color;
                this.m_prevFillCount = num;
            }
        }

        public override bool SetEmissiveStateDamaged() => 
            false;

        public override bool SetEmissiveStateDisabled() => 
            false;

        public override bool SetEmissiveStateWorking() => 
            false;

        public void SetInventory(MyInventory inventory, int index)
        {
            throw new NotImplementedException("TODO Dusan inventory sync");
        }

        private void SetStockpilingState(bool newState)
        {
            this.m_isStockpiling = newState;
            this.SourceComp.SetProductionEnabledByType(this.BlockDefinition.StoredGasId, !this.m_isStockpiling && this.CanStore);
            base.ResourceSink.Update();
        }

        private void Sink_CurrentInputChanged(MyDefinitionId resourceTypeId, float oldInput, MyResourceSinkComponent sink)
        {
            if ((resourceTypeId == this.BlockDefinition.StoredGasId) && Sync.IsServer)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        private void Source_OutputChanged(MyDefinitionId changedResourceId, float oldOutput, MyResourceSourceComponent source)
        {
            if ((changedResourceId == this.BlockDefinition.StoredGasId) && Sync.IsServer)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        private void Transfer(double transferAmount)
        {
            if (transferAmount > 0.0)
            {
                this.Fill(transferAmount);
            }
            else if (transferAmount < 0.0)
            {
                this.Drain(-transferAmount);
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (Sync.IsServer)
            {
                if ((this.FilledRatio > 0.0) && this.UseConveyorSystem)
                {
                    MyInventory destinationInventory = this.GetInventory(0);
                    if (destinationInventory.VolumeFillFactor < 0.6f)
                    {
                        MyFixedPoint? maxAmount = null;
                        MyGridConveyorSystem.PullAllRequest(this, destinationInventory, base.OwnerId, destinationInventory.Constraint, maxAmount, true);
                    }
                }
                this.ExecuteGasTransfer();
            }
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if (Sync.IsServer && ((this.m_autoRefill != null) && this.CanRefill()))
            {
                this.RefillBottles();
            }
        }

        private void UpdateEmissivity()
        {
            MyEmissiveColorStateResult result;
            Color red = Color.Red;
            bool flag = true;
            if (!this.CanStore)
            {
                if (!base.IsFunctional)
                {
                    if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Damaged, out result))
                    {
                        red = result.EmissiveColor;
                    }
                }
                else
                {
                    flag = false;
                    if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Disabled, out result))
                    {
                        red = result.EmissiveColor;
                    }
                }
            }
            else if (this.IsStockpiling)
            {
                red = Color.Teal;
                flag = false;
                if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Alternative, out result))
                {
                    red = result.EmissiveColor;
                }
            }
            else if (this.FilledRatio <= 9.9999997473787516E-06)
            {
                red = Color.Yellow;
                if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Warning, out result))
                {
                    red = result.EmissiveColor;
                }
            }
            else
            {
                red = Color.Green;
                if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Working, out result))
                {
                    red = result.EmissiveColor;
                }
            }
            this.SetEmissive(red, flag ? ((float) this.FilledRatio) : 1f);
        }

        private void UpdateText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(this.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            if (MySession.Static.Settings.EnableOxygen || (this.BlockDefinition.StoredGasId != this.m_oxygenGasId))
            {
                base.DetailedInfo.Append(string.Format(MyTexts.GetString(MySpaceTexts.Oxygen_Filled), (this.FilledRatio * 100.0).ToString("F1"), (int) (this.FilledRatio * this.Capacity), this.Capacity));
            }
            else
            {
                base.DetailedInfo.Append(MyTexts.Get(MySpaceTexts.Oxygen_Disabled));
            }
            base.RaisePropertiesChanged();
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.UpdateEmissivity();
        }

        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        public bool IsStockpiling
        {
            get => 
                this.m_isStockpiling;
            private set => 
                this.SetStockpilingState(value);
        }

        bool Sandbox.ModAPI.Ingame.IMyGasTank.Stockpile
        {
            get => 
                this.IsStockpiling;
            set => 
                this.ChangeStockpileMode(value);
        }

        bool Sandbox.ModAPI.Ingame.IMyGasTank.AutoRefillBottles
        {
            get => 
                ((bool) this.m_autoRefill);
            set => 
                (this.m_autoRefill.Value = value);
        }

        public bool CanStore =>
            ((((MySession.Static != null) && MySession.Static.Settings.EnableOxygen) || (this.BlockDefinition.StoredGasId != this.m_oxygenGasId)) && (base.IsWorking && (base.Enabled && base.IsFunctional)));

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

        public MyGasTankDefinition BlockDefinition =>
            ((MyGasTankDefinition) base.BlockDefinition);

        private float GasOutputPerSecond =>
            (this.SourceComp.ProductionEnabledByType(this.BlockDefinition.StoredGasId) ? this.SourceComp.CurrentOutputByType(this.BlockDefinition.StoredGasId) : 0f);

        private float GasInputPerSecond =>
            base.ResourceSink.CurrentInputByType(this.BlockDefinition.StoredGasId);

        private float GasOutputPerUpdate =>
            (this.GasOutputPerSecond * 0.01666667f);

        private float GasInputPerUpdate =>
            (this.GasInputPerSecond * 0.01666667f);

        public float Capacity =>
            this.BlockDefinition.Capacity;

        public float GasCapacity =>
            this.Capacity;

        public double FilledRatio { get; private set; }

        public bool CanPressurizeRoom =>
            false;

        public bool UseConveyorSystem { get; set; }

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
            public static readonly MyGasTank.<>c <>9 = new MyGasTank.<>c();
            public static MyTerminalValueControl<MyGasTank, bool>.GetterDelegate <>9__47_0;
            public static MyTerminalValueControl<MyGasTank, bool>.SetterDelegate <>9__47_1;
            public static Func<MyGasTank, bool> <>9__47_2;
            public static MyTerminalValueControl<MyGasTank, bool>.GetterDelegate <>9__47_3;
            public static MyTerminalValueControl<MyGasTank, bool>.SetterDelegate <>9__47_4;
            public static Func<MyGasTank, Action<bool>> <>9__94_0;
            public static Func<MyGasTank, Action<double>> <>9__97_0;
            public static Func<MyGasTank, Action> <>9__99_0;

            internal Action<double> <ChangeFillRatioAmount>b__97_0(MyGasTank x) => 
                new Action<double>(x.OnFilledRatioCallback);

            internal Action<bool> <ChangeStockpileMode>b__94_0(MyGasTank x) => 
                new Action<bool>(x.OnStockipleModeCallback);

            internal bool <CreateTerminalControls>b__47_0(MyGasTank x) => 
                x.IsStockpiling;

            internal void <CreateTerminalControls>b__47_1(MyGasTank x, bool v)
            {
                x.ChangeStockpileMode(v);
            }

            internal bool <CreateTerminalControls>b__47_2(MyGasTank x) => 
                x.CanRefill();

            internal bool <CreateTerminalControls>b__47_3(MyGasTank x) => 
                ((bool) x.m_autoRefill);

            internal void <CreateTerminalControls>b__47_4(MyGasTank x, bool v)
            {
                x.m_autoRefill.Value = v;
            }

            internal Action <SendRefillRequest>b__99_0(MyGasTank x) => 
                new Action(x.OnRefillCallback);
        }
    }
}

