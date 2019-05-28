namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
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
    using VRage.Game.Entity;
    using VRage.Game.Graphics;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_BatteryBlock)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyBatteryBlock), typeof(Sandbox.ModAPI.Ingame.IMyBatteryBlock) })]
    public class MyBatteryBlock : MyFunctionalBlock, Sandbox.ModAPI.IMyBatteryBlock, Sandbox.ModAPI.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.Ingame.IMyBatteryBlock
    {
        private static readonly string[] m_emissiveTextureNames = new string[] { "Emissive0", "Emissive1", "Emissive2", "Emissive3" };
        private bool m_hasRemainingCapacity;
        private float m_maxOutput;
        private float m_currentOutput;
        private float m_currentStoredPower;
        private float m_maxStoredPower;
        private int m_lastUpdateTime;
        private float m_timeRemaining;
        private bool m_sourceDirty;
        private const int m_productionUpdateInterval = 100;
        private readonly VRage.Sync.Sync<Sandbox.ModAPI.Ingame.ChargeMode, SyncDirection.BothWays> m_chargeMode;
        private readonly VRage.Sync.Sync<bool, SyncDirection.FromServer> m_isFull;
        private readonly VRage.Sync.Sync<float, SyncDirection.FromServer> m_storedPower;
        private Color m_prevEmissiveColor = Color.Black;
        private int m_prevFillCount = -1;
        private MyResourceSourceComponent m_sourceComp;

        public MyBatteryBlock()
        {
            this.CreateTerminalControls();
            this.SourceComp = new MyResourceSourceComponent(1);
            base.ResourceSink = new MyResourceSinkComponent(1);
            this.SourceComp.OutputChanged += (x, y, z) => this.UpdateText();
            this.m_chargeMode.ValueChanged += delegate (SyncBase x) {
                this.SourceComp.SetProductionEnabledByType(MyResourceDistributorComponent.ElectricityId, this.m_chargeMode != 1);
                this.UpdateMaxOutputAndEmissivity();
                this.m_sourceDirty = true;
            };
            this.m_storedPower.ValueChanged += x => this.CapacityChanged();
        }

        private void CalculateInputTimeRemaining()
        {
            if (base.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId) != 0f)
            {
                this.TimeRemaining = (this.MaxStoredPower - this.CurrentStoredPower) / ((base.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId) - this.SourceComp.CurrentOutput) / this.SourceComp.ProductionToCapacityMultiplierByType(MyResourceDistributorComponent.ElectricityId));
            }
            else
            {
                this.TimeRemaining = 0f;
            }
        }

        private void CalculateOutputTimeRemaining()
        {
            if ((this.CurrentStoredPower == 0f) || (this.SourceComp.CurrentOutput == 0f))
            {
                this.TimeRemaining = 0f;
            }
            else
            {
                this.TimeRemaining = this.CurrentStoredPower / ((this.SourceComp.CurrentOutput - base.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId)) / this.SourceComp.ProductionToCapacityMultiplier);
            }
        }

        private void CapacityChanged()
        {
            this.CurrentStoredPower = this.m_storedPower.Value;
        }

        protected override bool CheckIsWorking() => 
            (base.Enabled && (this.SourceComp.HasCapacityRemainingByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking()));

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
            this.UpdateMaxOutputAndEmissivity();
        }

        private float ComputeMaxPowerOutput()
        {
            if (!this.CheckIsWorking() || !this.SourceComp.ProductionEnabledByType(MyResourceDistributorComponent.ElectricityId))
            {
                return 0f;
            }
            return this.BlockDefinition.MaxPowerOutput;
        }

        private void ConsumePower(float timeDeltaMs, float output)
        {
            if (this.SourceComp.HasCapacityRemainingByType(MyResourceDistributorComponent.ElectricityId))
            {
                float num = output / (this.SourceComp.ProductionToCapacityMultiplier * 1000f);
                float num2 = timeDeltaMs * num;
                if (num2 != 0f)
                {
                    if ((this.CurrentStoredPower - num2) <= 0f)
                    {
                        this.SourceComp.SetOutput(0f);
                        this.CurrentStoredPower = 0f;
                        this.TimeRemaining = 0f;
                    }
                    else
                    {
                        this.CurrentStoredPower -= num2;
                        if (Sync.IsServer && (this.m_isFull != null))
                        {
                            this.m_isFull.Value = false;
                        }
                    }
                    if (Sync.IsServer)
                    {
                        this.m_storedPower.Value = this.CurrentStoredPower;
                    }
                }
            }
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyBatteryBlock>())
            {
                base.CreateTerminalControls();
                MyTerminalControlCombobox<MyBatteryBlock> combobox1 = new MyTerminalControlCombobox<MyBatteryBlock>("ChargeMode", MySpaceTexts.BlockPropertyTitle_ChargeMode, MySpaceTexts.Blank);
                combobox1.ComboBoxContent = new Action<List<MyTerminalControlComboBoxItem>>(MyBatteryBlock.FillChargeModeCombo);
                combobox1.Getter = x => (long) x.ChargeMode;
                MyTerminalControlCombobox<MyBatteryBlock> local4 = combobox1;
                MyTerminalControlCombobox<MyBatteryBlock> local5 = combobox1;
                local5.Setter = (x, v) => x.ChargeMode = (Sandbox.ModAPI.Ingame.ChargeMode) ((int) v);
                MyTerminalControlCombobox<MyBatteryBlock> control = local5;
                control.SetSerializerRange(MyEnum<Sandbox.ModAPI.Ingame.ChargeMode>.Range.Min, MyEnum<Sandbox.ModAPI.Ingame.ChargeMode>.Range.Max);
                MyTerminalControlFactory.AddControl<MyBatteryBlock>(control);
                MyTerminalControlFactory.AddAction<MyBatteryBlock>(new MyTerminalAction<MyBatteryBlock>("Recharge", MyTexts.Get(MySpaceTexts.BlockActionTitle_RechargeToggle), new Action<MyBatteryBlock>(MyBatteryBlock.OnRechargeToggle), new MyTerminalControl<MyBatteryBlock>.WriterDelegate(MyBatteryBlock.WriteChargeModeValue), MyTerminalActionIcons.TOGGLE));
                MyTerminalControlFactory.AddAction<MyBatteryBlock>(new MyTerminalAction<MyBatteryBlock>("Discharge", MyTexts.Get(MySpaceTexts.BlockActionTitle_DischargeToggle), new Action<MyBatteryBlock>(MyBatteryBlock.OnDischargeToggle), new MyTerminalControl<MyBatteryBlock>.WriterDelegate(MyBatteryBlock.WriteChargeModeValue), MyTerminalActionIcons.TOGGLE));
                MyTerminalControlFactory.AddAction<MyBatteryBlock>(new MyTerminalAction<MyBatteryBlock>("Auto", MyTexts.Get(MySpaceTexts.BlockActionTitle_AutoEnable), new Action<MyBatteryBlock>(MyBatteryBlock.OnAutoEnabled), new MyTerminalControl<MyBatteryBlock>.WriterDelegate(MyBatteryBlock.WriteChargeModeValue), MyTerminalActionIcons.TOGGLE));
            }
        }

        private static void FillChargeModeCombo(List<MyTerminalControlComboBoxItem> list)
        {
            MyTerminalControlComboBoxItem item = new MyTerminalControlComboBoxItem {
                Key = 0L,
                Value = MySpaceTexts.BlockPropertyTitle_Auto
            };
            list.Add(item);
            item = new MyTerminalControlComboBoxItem {
                Key = 1L,
                Value = MySpaceTexts.BlockPropertyTitle_Recharge
            };
            list.Add(item);
            item = new MyTerminalControlComboBoxItem {
                Key = 2L,
                Value = MySpaceTexts.BlockPropertyTitle_Discharge
            };
            list.Add(item);
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_BatteryBlock objectBuilderCubeBlock = (MyObjectBuilder_BatteryBlock) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.CurrentStoredPower = this.CurrentStoredPower;
            objectBuilderCubeBlock.ProducerEnabled = this.SourceComp.ProductionEnabled;
            objectBuilderCubeBlock.SemiautoEnabled = false;
            objectBuilderCubeBlock.OnlyDischargeEnabled = false;
            objectBuilderCubeBlock.ChargeMode = (int) this.ChargeMode;
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyResourceSourceInfo item = new MyResourceSourceInfo {
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                DefinedOutput = this.BlockDefinition.MaxPowerOutput,
                ProductionToCapacityMultiplier = 3600f
            };
            List<MyResourceSourceInfo> list1 = new List<MyResourceSourceInfo>();
            list1.Add(item);
            List<MyResourceSourceInfo> sourceResourceData = list1;
            this.SourceComp.Init(this.BlockDefinition.ResourceSourceGroup, sourceResourceData);
            this.SourceComp.HasCapacityRemainingChanged += (id, source) => base.UpdateIsWorking();
            this.SourceComp.ProductionEnabledChanged += new MyResourceCapacityRemainingChangedDelegate(this.Source_ProductionEnabledChanged);
            MyObjectBuilder_BatteryBlock block = (MyObjectBuilder_BatteryBlock) objectBuilder;
            this.SourceComp.SetProductionEnabledByType(MyResourceDistributorComponent.ElectricityId, block.ProducerEnabled);
            this.MaxStoredPower = this.BlockDefinition.MaxStoredPower;
            base.ResourceSink.Init(this.BlockDefinition.ResourceSinkGroup, this.BlockDefinition.RequiredPowerInput, new Func<float>(this.Sink_ComputeRequiredPower));
            base.Init(objectBuilder, cubeGrid);
            this.CurrentStoredPower = (block.CurrentStoredPower < 0f) ? (this.BlockDefinition.InitialStoredPowerRatio * this.BlockDefinition.MaxStoredPower) : block.CurrentStoredPower;
            if (Sync.IsServer)
            {
                this.m_storedPower.Value = this.CurrentStoredPower;
            }
            if (block.OnlyDischargeEnabled)
            {
                this.m_chargeMode.SetLocalValue(Sandbox.ModAPI.Ingame.ChargeMode.Discharge);
            }
            else
            {
                this.m_chargeMode.SetLocalValue((Sandbox.ModAPI.Ingame.ChargeMode) block.ChargeMode);
            }
            this.UpdateMaxOutputAndEmissivity();
            this.UpdateText();
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyBatteryBlock_IsWorkingChanged);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            this.m_lastUpdateTime = MySession.Static.GameplayFrameCounter;
            if (base.IsWorking)
            {
                this.OnStartWorking();
            }
            base.ResourceSink.Update();
        }

        private void MyBatteryBlock_IsWorkingChanged(MyCubeBlock obj)
        {
            this.UpdateMaxOutputAndEmissivity();
            base.ResourceSink.Update();
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            this.m_prevEmissiveColor = Color.White;
            this.UpdateEmissivity();
        }

        private static void OnAutoEnabled(MyBatteryBlock block)
        {
            block.ChargeMode = Sandbox.ModAPI.Ingame.ChargeMode.Auto;
        }

        private static void OnDischargeToggle(MyBatteryBlock block)
        {
            block.OnlyDischarge = !block.OnlyDischarge;
        }

        protected override void OnEnabledChanged()
        {
            this.SourceComp.Enabled = base.Enabled;
            this.UpdateMaxOutputAndEmissivity();
            base.ResourceSink.Update();
            base.OnEnabledChanged();
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            this.m_prevFillCount = -1;
        }

        private static void OnRechargeToggle(MyBatteryBlock block)
        {
            block.OnlyRecharge = !block.OnlyRecharge;
        }

        private void ProducerEnadChanged()
        {
            this.SourceComp.SetProductionEnabledByType(MyResourceDistributorComponent.ElectricityId, this.ProducerEnabled);
        }

        private void SetEmissive(Color color, float fill)
        {
            int num = (int) (fill * m_emissiveTextureNames.Length);
            if ((base.Render.RenderObjectIDs[0] != uint.MaxValue) && ((color != this.m_prevEmissiveColor) || (num != this.m_prevFillCount)))
            {
                int index = 0;
                while (true)
                {
                    if (index >= m_emissiveTextureNames.Length)
                    {
                        this.m_prevEmissiveColor = color;
                        this.m_prevFillCount = num;
                        break;
                    }
                    if (index < num)
                    {
                        UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[index], color, 1f);
                    }
                    else
                    {
                        UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[index], Color.Black, 0f);
                    }
                    index++;
                }
            }
        }

        public override bool SetEmissiveStateDamaged() => 
            false;

        public override bool SetEmissiveStateDisabled() => 
            false;

        public override bool SetEmissiveStateWorking() => 
            false;

        private float Sink_ComputeRequiredPower()
        {
            int num1;
            if (!base.Enabled || !base.IsFunctional)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) (this.m_isFull == 0);
            }
            float num = (((this.MaxStoredPower - this.CurrentStoredPower) * 60f) / 100f) * this.SourceComp.ProductionToCapacityMultiplierByType(MyResourceDistributorComponent.ElectricityId);
            float num2 = this.SourceComp.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId);
            float num3 = 0f;
            if ((num1 & (this.ChargeMode != Sandbox.ModAPI.Ingame.ChargeMode.Discharge)) != 0)
            {
                float num4 = base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
                num3 = Math.Min(num + num2, num4);
            }
            return num3;
        }

        private void Source_ProductionEnabledChanged(MyDefinitionId changedResourceId, MyResourceSourceComponent source)
        {
            base.UpdateIsWorking();
        }

        private void StorePower(float timeDeltaMs, float input)
        {
            float num = input / (this.SourceComp.ProductionToCapacityMultiplierByType(MyResourceDistributorComponent.ElectricityId) * 1000f);
            float num2 = (timeDeltaMs * num) * 0.8f;
            if (num2 > 0f)
            {
                if ((this.CurrentStoredPower + num2) < this.MaxStoredPower)
                {
                    this.CurrentStoredPower += num2;
                }
                else
                {
                    this.CurrentStoredPower = this.MaxStoredPower;
                    this.TimeRemaining = 0f;
                    if (Sync.IsServer && (this.m_isFull == null))
                    {
                        this.m_isFull.Value = true;
                    }
                }
            }
            if (Sync.IsServer)
            {
                this.m_storedPower.Value = this.CurrentStoredPower;
            }
        }

        private void TransferPower(float timeDeltaMs, float input, float output)
        {
            float num = input - output;
            if (num < 0f)
            {
                this.ConsumePower(timeDeltaMs, -num);
            }
            else if (num > 0f)
            {
                this.StorePower(timeDeltaMs, num);
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            float lastUpdateTime = this.m_lastUpdateTime;
            this.m_lastUpdateTime = MySession.Static.GameplayFrameCounter;
            if (base.IsFunctional)
            {
                this.UpdateMaxOutputAndEmissivity();
                float timeDeltaMs = ((MySession.Static.GameplayFrameCounter - lastUpdateTime) * 0.01666667f) * 1000f;
                if (MySession.Static.CreativeMode)
                {
                    if (Sync.IsServer && base.IsFunctional)
                    {
                        if (this.ChargeMode == Sandbox.ModAPI.Ingame.ChargeMode.Discharge)
                        {
                            base.UpdateIsWorking();
                            if (!this.SourceComp.HasCapacityRemainingByType(MyResourceDistributorComponent.ElectricityId))
                            {
                                return;
                            }
                            this.CalculateOutputTimeRemaining();
                        }
                        else
                        {
                            float single1;
                            if (!base.Enabled || !base.IsFunctional)
                            {
                                single1 = 0f;
                            }
                            else
                            {
                                single1 = 1f;
                            }
                            this.StorePower(timeDeltaMs, ((this.SourceComp.ProductionToCapacityMultiplierByType(MyResourceDistributorComponent.ElectricityId) * this.MaxStoredPower) / 8f) * single1);
                        }
                    }
                }
                else if (Sync.IsServer)
                {
                    switch (this.ChargeMode)
                    {
                        case Sandbox.ModAPI.Ingame.ChargeMode.Auto:
                            this.TransferPower(timeDeltaMs, base.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId), this.SourceComp.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId));
                            break;

                        case Sandbox.ModAPI.Ingame.ChargeMode.Recharge:
                            this.StorePower(timeDeltaMs, base.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId));
                            break;

                        case Sandbox.ModAPI.Ingame.ChargeMode.Discharge:
                            this.ConsumePower(timeDeltaMs, this.SourceComp.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                base.ResourceSink.Update();
                if (this.m_sourceDirty)
                {
                    this.SourceComp.OnProductionEnabledChanged(new MyDefinitionId?(MyResourceDistributorComponent.ElectricityId));
                }
                this.m_sourceDirty = false;
                switch (this.ChargeMode)
                {
                    case Sandbox.ModAPI.Ingame.ChargeMode.Auto:
                        if (base.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId) > this.SourceComp.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId))
                        {
                            this.CalculateInputTimeRemaining();
                            return;
                        }
                        this.CalculateOutputTimeRemaining();
                        return;

                    case Sandbox.ModAPI.Ingame.ChargeMode.Recharge:
                        this.CalculateInputTimeRemaining();
                        return;

                    case Sandbox.ModAPI.Ingame.ChargeMode.Discharge:
                        this.CalculateOutputTimeRemaining();
                        return;
                }
                throw new ArgumentOutOfRangeException();
            }
        }

        internal void UpdateEmissivity()
        {
            if (base.InScene)
            {
                MyEmissiveColorStateResult result;
                float fill = 1f;
                Color red = Color.Red;
                if (!base.IsFunctional || !base.Enabled)
                {
                    if (base.IsFunctional)
                    {
                        if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Disabled, out result))
                        {
                            red = result.EmissiveColor;
                        }
                    }
                    else if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Damaged, out result))
                    {
                        red = result.EmissiveColor;
                    }
                }
                else if (!base.IsWorking)
                {
                    fill = 0.25f;
                    if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Disabled, out result))
                    {
                        red = result.EmissiveColor;
                    }
                }
                else
                {
                    fill = this.CurrentStoredPower / this.MaxStoredPower;
                    if (this.ChargeMode == Sandbox.ModAPI.Ingame.ChargeMode.Auto)
                    {
                        red = Color.Green;
                        if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Working, out result))
                        {
                            red = result.EmissiveColor;
                        }
                    }
                    else if (this.ChargeMode == Sandbox.ModAPI.Ingame.ChargeMode.Discharge)
                    {
                        red = Color.SteelBlue;
                        if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Alternative, out result))
                        {
                            red = result.EmissiveColor;
                        }
                    }
                    else
                    {
                        red = Color.Yellow;
                        if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Warning, out result))
                        {
                            red = result.EmissiveColor;
                        }
                    }
                }
                if (this.BlockDefinition.Id.SubtypeName == "SmallBlockSmallBatteryBlock")
                {
                    UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[0], red, 1f);
                }
                else
                {
                    this.SetEmissive(red, fill);
                }
            }
        }

        private void UpdateMaxOutputAndEmissivity()
        {
            base.ResourceSink.Update();
            this.SourceComp.SetMaxOutputByType(MyResourceDistributorComponent.ElectricityId, this.ComputeMaxPowerOutput());
            this.UpdateEmissivity();
        }

        private void UpdateText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BatteryBlock));
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxOutput));
            MyValueFormatter.AppendWorkInBestUnit(this.BlockDefinition.MaxPowerOutput, base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(this.BlockDefinition.RequiredPowerInput, base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxStoredPower));
            MyValueFormatter.AppendWorkHoursInBestUnit(this.MaxStoredPower, base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentOutput));
            MyValueFormatter.AppendWorkInBestUnit(this.SourceComp.CurrentOutput, base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_StoredPower));
            MyValueFormatter.AppendWorkHoursInBestUnit(this.CurrentStoredPower, base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            float num = base.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId);
            float num2 = this.SourceComp.CurrentOutputByType(MyResourceDistributorComponent.ElectricityId);
            if (num > num2)
            {
                base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_RechargedIn));
                MyValueFormatter.AppendTimeInBestUnit(this.m_timeRemaining, base.DetailedInfo);
            }
            else if (num == num2)
            {
                base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_DepletedIn));
                MyValueFormatter.AppendTimeInBestUnit(float.PositiveInfinity, base.DetailedInfo);
            }
            else
            {
                base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_DepletedIn));
                MyValueFormatter.AppendTimeInBestUnit(this.m_timeRemaining, base.DetailedInfo);
            }
            base.RaisePropertiesChanged();
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.UpdateEmissivity();
        }

        private static void WriteChargeModeValue(MyBatteryBlock block, StringBuilder writeTo)
        {
            switch (block.ChargeMode)
            {
                case Sandbox.ModAPI.Ingame.ChargeMode.Auto:
                    writeTo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyTitle_Auto));
                    return;

                case Sandbox.ModAPI.Ingame.ChargeMode.Recharge:
                    writeTo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyTitle_Recharge));
                    return;

                case Sandbox.ModAPI.Ingame.ChargeMode.Discharge:
                    writeTo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyTitle_Discharge));
                    return;
            }
            throw new ArgumentOutOfRangeException();
        }

        public MyBatteryBlockDefinition BlockDefinition =>
            (base.BlockDefinition as MyBatteryBlockDefinition);

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

        public float TimeRemaining
        {
            get => 
                this.m_timeRemaining;
            set
            {
                this.m_timeRemaining = value;
                this.UpdateText();
            }
        }

        public bool HasCapacityRemaining =>
            this.SourceComp.HasCapacityRemainingByType(MyResourceDistributorComponent.ElectricityId);

        public float MaxStoredPower
        {
            get => 
                this.m_maxStoredPower;
            private set
            {
                if (this.m_maxStoredPower != value)
                {
                    this.m_maxStoredPower = value;
                }
            }
        }

        private bool ProducerEnabled =>
            (this.m_chargeMode != 1);

        public float CurrentStoredPower
        {
            get => 
                this.SourceComp.RemainingCapacityByType(MyResourceDistributorComponent.ElectricityId);
            set
            {
                this.SourceComp.SetRemainingCapacityByType(MyResourceDistributorComponent.ElectricityId, MathHelper.Clamp(value, 0f, this.MaxStoredPower));
                this.UpdateMaxOutputAndEmissivity();
            }
        }

        public float CurrentOutput =>
            ((this.SourceComp == null) ? 0f : this.SourceComp.CurrentOutput);

        public float MaxOutput =>
            ((this.SourceComp == null) ? 0f : this.SourceComp.MaxOutput);

        public float CurrentInput =>
            ((base.ResourceSink == null) ? 0f : base.ResourceSink.CurrentInputByType(MyResourceDistributorComponent.ElectricityId));

        public float MaxInput =>
            ((base.ResourceSink == null) ? 0f : base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId));

        public bool IsCharging =>
            ((this.CurrentInput > this.CurrentOutput) && (this.CurrentInput > 0f));

        public bool SemiautoEnabled
        {
            get => 
                (this.m_chargeMode == 0);
            set
            {
                if (value)
                {
                    this.m_chargeMode.Value = Sandbox.ModAPI.Ingame.ChargeMode.Auto;
                }
            }
        }

        public bool OnlyRecharge
        {
            get => 
                (this.m_chargeMode == 1);
            set => 
                (this.m_chargeMode.Value = value ? Sandbox.ModAPI.Ingame.ChargeMode.Recharge : Sandbox.ModAPI.Ingame.ChargeMode.Auto);
        }

        public bool OnlyDischarge
        {
            get => 
                (this.m_chargeMode == 2);
            set => 
                (this.m_chargeMode.Value = value ? Sandbox.ModAPI.Ingame.ChargeMode.Discharge : Sandbox.ModAPI.Ingame.ChargeMode.Auto);
        }

        public Sandbox.ModAPI.Ingame.ChargeMode ChargeMode
        {
            get => 
                this.m_chargeMode.Value;
            set
            {
                if (((Sandbox.ModAPI.Ingame.ChargeMode) this.m_chargeMode.Value) != value)
                {
                    this.m_chargeMode.Value = value;
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyBatteryBlock.<>c <>9 = new MyBatteryBlock.<>c();
            public static MyTerminalValueControl<MyBatteryBlock, long>.GetterDelegate <>9__58_0;
            public static MyTerminalValueControl<MyBatteryBlock, long>.SetterDelegate <>9__58_1;

            internal long <CreateTerminalControls>b__58_0(MyBatteryBlock x) => 
                ((long) x.ChargeMode);

            internal void <CreateTerminalControls>b__58_1(MyBatteryBlock x, long v)
            {
                x.ChargeMode = (ChargeMode) ((int) v);
            }
        }
    }
}

