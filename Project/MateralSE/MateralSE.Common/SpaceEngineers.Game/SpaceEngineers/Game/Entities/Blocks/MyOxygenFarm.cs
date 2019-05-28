namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.EntityComponents.DebugRenders;
    using SpaceEngineers.Game.EntityComponents.GameLogic;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
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
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_OxygenFarm)), MyTerminalInterface(new System.Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyOxygenFarm), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyOxygenFarm) })]
    public class MyOxygenFarm : MyFunctionalBlock, SpaceEngineers.Game.ModAPI.IMyOxygenFarm, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyTerminalBlock, SpaceEngineers.Game.ModAPI.Ingame.IMyOxygenFarm, IMyGasBlock, IMyConveyorEndpointBlock
    {
        private static readonly string[] m_emissiveTextureNames = new string[] { "Emissive0", "Emissive1", "Emissive2", "Emissive3" };
        private float m_maxGasOutputFactor;
        private bool firstUpdate = true;
        private readonly MyDefinitionId m_oxygenGasId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
        private MyResourceSourceComponent m_sourceComp;
        private MyMultilineConveyorEndpoint m_conveyorEndpoint;

        public MyOxygenFarm()
        {
            base.ResourceSink = new MyResourceSinkComponent(1);
            this.SourceComp = new MyResourceSourceComponent(1);
        }

        public bool AllowSelfPulling() => 
            false;

        protected override bool CheckIsWorking() => 
            ((MySession.Static.Settings.EnableOxygen || (this.BlockDefinition.ProducedGas != this.m_oxygenGasId)) && (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking()));

        private void ComponentStack_OnIsFunctionalChanged()
        {
            this.SourceComp.Enabled = base.IsWorking;
            base.ResourceSink.Update();
            this.UpdateEmissivity();
        }

        private float ComputeRequiredPower()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return this.BlockDefinition.OperationalPowerConsumption;
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false) => 
            (base.GetObjectBuilderCubeBlock(copy) as MyObjectBuilder_OxygenFarm);

        public PullInformation GetPullInformation() => 
            null;

        public PullInformation GetPushInformation() => 
            null;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.OnIsWorkingChanged);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            this.InitializeConveyorEndpoint();
            MyResourceSourceInfo sourceResourceData = new MyResourceSourceInfo {
                ResourceTypeId = this.BlockDefinition.ProducedGas,
                DefinedOutput = this.BlockDefinition.MaxGasOutput,
                ProductionToCapacityMultiplier = 1f,
                IsInfiniteCapacity = true
            };
            this.SourceComp.Init(this.BlockDefinition.ResourceSourceGroup, sourceResourceData);
            this.SourceComp.Enabled = base.IsWorking;
            MyResourceSinkInfo sinkData = new MyResourceSinkInfo {
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                MaxRequiredInput = this.BlockDefinition.OperationalPowerConsumption,
                RequiredInputFunc = new Func<float>(this.ComputeRequiredPower)
            };
            base.ResourceSink.Init(this.BlockDefinition.ResourceSinkGroup, sinkData);
            base.ResourceSink.IsPoweredChanged += new Action(this.PowerReceiver_IsPoweredChanged);
            base.ResourceSink.Update();
            base.GameLogic = new MySolarGameLogicComponent();
            this.SolarComponent = base.GameLogic as MySolarGameLogicComponent;
            this.SolarComponent.Initialize(this.BlockDefinition.PanelOrientation, this.BlockDefinition.IsTwoSided, this.BlockDefinition.PanelOffset, this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentSolarPanel(this));
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_OnIsFunctionalChanged);
            this.UpdateVisual();
            this.UpdateDisplay();
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            this.SourceComp.Enabled = base.IsWorking;
            base.ResourceSink.Update();
            this.UpdateEmissivity();
        }

        private void OnIsWorkingChanged(MyCubeBlock obj)
        {
            this.SourceComp.Enabled = base.IsWorking;
            this.UpdateEmissivity();
        }

        private void PowerReceiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        bool IMyGasBlock.IsWorking() => 
            (MySession.Static.Settings.EnableOxygen && (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && (base.IsWorking && base.IsFunctional)));

        float SpaceEngineers.Game.ModAPI.Ingame.IMyOxygenFarm.GetOutput() => 
            (!base.IsWorking ? 0f : this.SolarComponent.MaxOutput);

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (base.CubeGrid.Physics != null)
            {
                float maxOutput;
                base.ResourceSink.Update();
                if (!base.IsWorking || !this.SourceComp.ProductionEnabledByType(this.BlockDefinition.ProducedGas))
                {
                    maxOutput = 0f;
                }
                else
                {
                    maxOutput = this.SolarComponent.MaxOutput;
                }
                float num = maxOutput;
                if ((num != this.m_maxGasOutputFactor) || this.firstUpdate)
                {
                    this.m_maxGasOutputFactor = num;
                    this.SourceComp.SetMaxOutputByType(this.BlockDefinition.ProducedGas, this.SourceComp.DefinedOutputByType(this.BlockDefinition.ProducedGas) * this.m_maxGasOutputFactor);
                    this.UpdateVisual();
                    this.UpdateDisplay();
                    this.firstUpdate = false;
                }
                base.ResourceSink.Update();
            }
        }

        private void UpdateDisplay()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(this.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId), base.DetailedInfo);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_OxygenOutput));
            base.DetailedInfo.Append((this.SourceComp.MaxOutputByType(this.BlockDefinition.ProducedGas) * 60f).ToString("F"));
            base.DetailedInfo.Append(" L/min");
            base.RaisePropertiesChanged();
            this.UpdateEmissivity();
        }

        private void UpdateEmissivity()
        {
            if (base.InScene)
            {
                MyEmissiveColorStateResult result;
                Color red = Color.Red;
                if (!base.IsWorking)
                {
                    if (base.IsFunctional)
                    {
                        if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Disabled, out result))
                        {
                            red = result.EmissiveColor;
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[i], red, 1f);
                        }
                    }
                    else
                    {
                        red = Color.Black;
                        if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Damaged, out result))
                        {
                            red = result.EmissiveColor;
                        }
                        UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[0], red, 0f);
                        for (int i = 1; i < 4; i++)
                        {
                            UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[i], red, 0f);
                        }
                    }
                }
                else if (this.m_maxGasOutputFactor <= 0f)
                {
                    red = Color.Black;
                    if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Damaged, out result))
                    {
                        red = result.EmissiveColor;
                    }
                    UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[0], red, 0f);
                    for (int i = 1; i < 4; i++)
                    {
                        UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[i], red, 0f);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (i < (this.m_maxGasOutputFactor * 4f))
                        {
                            red = Color.Green;
                            if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Working, out result))
                            {
                                red = result.EmissiveColor;
                            }
                            UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[i], red, 1f);
                        }
                        else
                        {
                            red = Color.Black;
                            if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Damaged, out result))
                            {
                                red = result.EmissiveColor;
                            }
                            UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[i], red, 1f);
                        }
                    }
                }
            }
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            base.UpdateIsWorking();
            this.UpdateEmissivity();
        }

        public MyOxygenFarmDefinition BlockDefinition =>
            (base.BlockDefinition as MyOxygenFarmDefinition);

        public MySolarGameLogicComponent SolarComponent { get; private set; }

        public bool CanProduce =>
            ((MySession.Static.Settings.EnableOxygen || (this.BlockDefinition.ProducedGas != this.m_oxygenGasId)) && (base.Enabled && (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && (base.IsWorking && base.IsFunctional))));

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

        public bool CanPressurizeRoom =>
            false;

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        bool SpaceEngineers.Game.ModAPI.Ingame.IMyOxygenFarm.CanProduce =>
            this.CanProduce;
    }
}

