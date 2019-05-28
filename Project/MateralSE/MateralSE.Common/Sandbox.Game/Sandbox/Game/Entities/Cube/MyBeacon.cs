namespace Sandbox.Game.Entities.Cube
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Lights;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Graphics;
    using VRage.Game.Gui;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Lights;

    [MyCubeBlockType(typeof(MyObjectBuilder_Beacon)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyBeacon), typeof(Sandbox.ModAPI.Ingame.IMyBeacon) })]
    public class MyBeacon : MyFunctionalBlock, Sandbox.ModAPI.IMyBeacon, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyBeacon
    {
        private static readonly Color COLOR_ON = new Color(0xff, 0xff, 0x80);
        private static readonly Color COLOR_OFF = new Color(30, 30, 30);
        private static readonly float POINT_LIGHT_RANGE_SMALL = 2f;
        private static readonly float POINT_LIGHT_RANGE_LARGE = 7.5f;
        private static readonly float POINT_LIGHT_INTENSITY_SMALL = 1f;
        private static readonly float POINT_LIGHT_INTENSITY_LARGE = 1f;
        private static readonly float GLARE_MAX_DISTANCE = 10000f;
        private const float LIGHT_TURNING_ON_TIME_IN_SECONDS = 0.5f;
        private bool m_largeLight;
        private MyLight m_light;
        private Vector3 m_lightPositionOffset;
        private float m_currentLightPower;
        private int m_lastAnimationUpdateTime;
        private bool m_restartTimeMeasure;
        private MyFlareDefinition m_flare;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_radius;
        private bool m_animationRunning;

        public MyBeacon()
        {
            this.CreateTerminalControls();
            this.HudText = new StringBuilder();
            this.m_radius.ValueChanged += obj => this.ChangeRadius();
            base.NeedsWorldMatrix = true;
        }

        private void ChangeRadius()
        {
            this.RadioBroadcaster.BroadcastRadius = (float) this.m_radius;
            this.RadioBroadcaster.RaiseBroadcastRadiusChanged();
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        protected override void Closing()
        {
            MyLights.RemoveLight(this.m_light);
            MyRadioBroadcaster radioBroadcaster = this.RadioBroadcaster;
            radioBroadcaster.OnBroadcastRadiusChanged = (Action) Delegate.Remove(radioBroadcaster.OnBroadcastRadiusChanged, new Action(this.OnBroadcastRadiusChanged));
            base.Closing();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
            this.UpdatePower();
            this.UpdateLightProperties();
            this.UpdateText();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyBeacon>())
            {
                base.CreateTerminalControls();
                MyTerminalControlFactory.GetList(typeof(MyBeacon)).Controls.Remove(MyTerminalControlFactory.GetList(typeof(MyBeacon)).Controls[5]);
                MyTerminalControlFactory.GetList(typeof(MyBeacon)).Controls.Remove(MyTerminalControlFactory.GetList(typeof(MyBeacon)).Controls[5]);
                MyTerminalControlTextbox<MyBeacon> textbox3 = new MyTerminalControlTextbox<MyBeacon>("CustomName", MyCommonTexts.Name, MySpaceTexts.Blank);
                MyTerminalControlTextbox<MyBeacon> textbox4 = new MyTerminalControlTextbox<MyBeacon>("CustomName", MyCommonTexts.Name, MySpaceTexts.Blank);
                textbox4.Getter = x => x.CustomName;
                MyTerminalControlTextbox<MyBeacon> local24 = textbox4;
                MyTerminalControlTextbox<MyBeacon> local25 = textbox4;
                local25.Setter = (x, v) => x.SetCustomName(v);
                MyTerminalControlTextbox<MyBeacon> control = local25;
                control.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyBeacon>(control);
                MyTerminalControlFactory.AddControl<MyBeacon>(new MyTerminalControlSeparator<MyBeacon>());
                MyTerminalControlTextbox<MyBeacon> textbox1 = new MyTerminalControlTextbox<MyBeacon>("HudText", MySpaceTexts.BlockPropertiesTitle_HudText, MySpaceTexts.BlockPropertiesTitle_HudText_Tooltip);
                MyTerminalControlTextbox<MyBeacon> textbox2 = new MyTerminalControlTextbox<MyBeacon>("HudText", MySpaceTexts.BlockPropertiesTitle_HudText, MySpaceTexts.BlockPropertiesTitle_HudText_Tooltip);
                textbox2.Getter = x => x.HudText;
                MyTerminalControlTextbox<MyBeacon> local22 = textbox2;
                MyTerminalControlTextbox<MyBeacon> local23 = textbox2;
                local23.Setter = (x, v) => x.SetHudText(v);
                MyTerminalControlTextbox<MyBeacon> local6 = local23;
                local6.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyBeacon>(local6);
                MyTerminalControlSlider<MyBeacon> slider1 = new MyTerminalControlSlider<MyBeacon>("Radius", MySpaceTexts.BlockPropertyTitle_BroadcastRadius, MySpaceTexts.BlockPropertyDescription_BroadcastRadius);
                MyTerminalControlSlider<MyBeacon> slider2 = new MyTerminalControlSlider<MyBeacon>("Radius", MySpaceTexts.BlockPropertyTitle_BroadcastRadius, MySpaceTexts.BlockPropertyDescription_BroadcastRadius);
                slider2.SetLogLimits(x => 1f, x => (x.BlockDefinition as MyBeaconDefinition).MaxBroadcastRadius);
                MyTerminalValueControl<MyBeacon, float>.GetterDelegate local20 = (MyTerminalValueControl<MyBeacon, float>.GetterDelegate) slider2;
                MyTerminalValueControl<MyBeacon, float>.GetterDelegate local21 = (MyTerminalValueControl<MyBeacon, float>.GetterDelegate) slider2;
                local21.DefaultValueGetter = x => (x.BlockDefinition as MyBeaconDefinition).MaxBroadcastRadius / 10f;
                MyTerminalValueControl<MyBeacon, float>.GetterDelegate local18 = local21;
                MyTerminalValueControl<MyBeacon, float>.GetterDelegate local19 = local21;
                local19.Getter = x => x.RadioBroadcaster.BroadcastRadius;
                MyTerminalValueControl<MyBeacon, float>.GetterDelegate local16 = local19;
                MyTerminalValueControl<MyBeacon, float>.GetterDelegate local17 = local19;
                local17.Setter = (x, v) => x.m_radius.Value = v;
                MyTerminalValueControl<MyBeacon, float>.GetterDelegate local14 = local17;
                MyTerminalValueControl<MyBeacon, float>.GetterDelegate local15 = local17;
                local15.Writer = (x, result) => result.AppendDecimal(x.RadioBroadcaster.BroadcastRadius, 0).Append(" m");
                MyTerminalValueControl<MyBeacon, float>.GetterDelegate local13 = local15;
                ((MyTerminalControlSlider<MyBeacon>) local13).EnableActions<MyBeacon>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyBeacon>((MyTerminalControl<MyBeacon>) local13);
            }
        }

        public override List<MyHudEntityParams> GetHudParams(bool allowBlink)
        {
            base.m_hudParams.Clear();
            if ((((base.CubeGrid != null) && !base.CubeGrid.MarkedForClose) && !base.CubeGrid.Closed) && base.IsWorking)
            {
                List<MyHudEntityParams> hudParams = base.GetHudParams(allowBlink);
                StringBuilder hudText = this.HudText;
                if (hudText.Length > 0)
                {
                    StringBuilder text = hudParams[0].Text;
                    text.Clear();
                    if (!string.IsNullOrEmpty(base.GetOwnerFactionTag()))
                    {
                        text.Append(base.GetOwnerFactionTag());
                        text.Append(".");
                    }
                    text.Append(hudText);
                }
                base.m_hudParams.AddList<MyHudEntityParams>(hudParams);
                if (base.HasLocalPlayerAccess() && (base.SlimBlock.CubeGrid.GridSystems.TerminalSystem != null))
                {
                    base.SlimBlock.CubeGrid.GridSystems.TerminalSystem.NeedsHudUpdate = true;
                    foreach (MyTerminalBlock block in base.SlimBlock.CubeGrid.GridSystems.TerminalSystem.HudBlocks)
                    {
                        if (!ReferenceEquals(block, this))
                        {
                            base.m_hudParams.AddList<MyHudEntityParams>(block.GetHudParams(true));
                        }
                    }
                }
            }
            return base.m_hudParams;
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_Beacon objectBuilderCubeBlock = (MyObjectBuilder_Beacon) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.HudText = this.HudText.ToString();
            objectBuilderCubeBlock.BroadcastRadius = this.RadioBroadcaster.BroadcastRadius;
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyBeaconDefinition blockDefinition = base.BlockDefinition as MyBeaconDefinition;
            if (blockDefinition.EmissiveColorPreset == MyStringHash.NullOrEmpty)
            {
                blockDefinition.EmissiveColorPreset = MyStringHash.GetOrCompute("Beacon");
            }
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(MyStringHash.GetOrCompute(blockDefinition.ResourceSinkGroup), 0.02f, new Func<float>(this.UpdatePowerInput));
            base.ResourceSink = component;
            this.RadioBroadcaster = new MyRadioBroadcaster(blockDefinition.MaxBroadcastRadius / 10f);
            MyObjectBuilder_Beacon beacon = (MyObjectBuilder_Beacon) objectBuilder;
            if (beacon.BroadcastRadius > 0f)
            {
                this.RadioBroadcaster.BroadcastRadius = beacon.BroadcastRadius;
            }
            this.RadioBroadcaster.BroadcastRadius = MathHelper.Clamp(this.RadioBroadcaster.BroadcastRadius, 1f, blockDefinition.MaxBroadcastRadius);
            this.HudText.Clear();
            if (beacon.HudText != null)
            {
                this.HudText.Append(beacon.HudText);
            }
            base.Init(objectBuilder, cubeGrid);
            component.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            component.Update();
            MyRadioBroadcaster radioBroadcaster = this.RadioBroadcaster;
            radioBroadcaster.OnBroadcastRadiusChanged = (Action) Delegate.Combine(radioBroadcaster.OnBroadcastRadiusChanged, new Action(this.OnBroadcastRadiusChanged));
            this.m_largeLight = cubeGrid.GridSizeEnum == MyCubeSize.Large;
            this.m_light = MyLights.AddLight();
            if (this.m_light != null)
            {
                this.m_light.Start(this.DisplayNameText);
                this.m_light.Range = this.m_largeLight ? 2f : 0.3f;
                this.m_light.GlareOn = false;
                this.m_light.GlareQuerySize = this.m_largeLight ? 1.5f : 0.3f;
                this.m_light.GlareQueryShift = this.m_largeLight ? 1f : 0.2f;
                this.m_light.GlareType = MyGlareTypeEnum.Normal;
                this.m_light.GlareMaxDistance = GLARE_MAX_DISTANCE;
                MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_FlareDefinition), blockDefinition.Flare);
                MyFlareDefinition definition = MyDefinitionManager.Static.GetDefinition(id) as MyFlareDefinition;
                this.m_flare = definition ?? new MyFlareDefinition();
                this.m_light.GlareIntensity = this.m_flare.Intensity;
                this.m_light.GlareSize = this.m_flare.Size;
                this.m_light.SubGlares = this.m_flare.SubGlares;
            }
            this.m_lightPositionOffset = this.m_largeLight ? new Vector3(0f, base.CubeGrid.GridSize * 0.3f, 0f) : Vector3.Zero;
            this.UpdateLightPosition();
            this.m_restartTimeMeasure = false;
            this.AnimationRunning = true;
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyBeacon_IsWorkingChanged);
            base.ShowOnHUD = false;
            this.UpdateText();
        }

        private unsafe void MyBeacon_IsWorkingChanged(MyCubeBlock obj)
        {
            if (this.RadioBroadcaster != null)
            {
                this.RadioBroadcaster.Enabled = base.IsWorking;
            }
            if (!MyFakes.ENABLE_RADIO_HUD)
            {
                if (base.IsWorking)
                {
                    MyHudEntityParams* paramsPtr1;
                    MyHudEntityParams hudParams = new MyHudEntityParams {
                        FlagsEnum = ~MyHudIndicatorFlagsEnum.NONE
                    };
                    paramsPtr1.Text = (this.HudText.Length > 0) ? this.HudText : base.CustomName;
                    paramsPtr1 = (MyHudEntityParams*) ref hudParams;
                    MyHud.LocationMarkers.RegisterMarker(this, hudParams);
                }
                else
                {
                    MyHud.LocationMarkers.UnregisterMarker(this);
                }
            }
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            this.UpdateLightParent();
            this.UpdateLightPosition();
            this.UpdateLightProperties();
            this.UpdateEmissivity();
        }

        private void OnBroadcastRadiusChanged()
        {
            base.ResourceSink.Update();
            this.UpdateText();
        }

        protected override void OnEnabledChanged()
        {
            base.ResourceSink.Update();
            this.UpdatePower();
            base.OnEnabledChanged();
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            this.UpdateLightProperties();
            this.UpdateEmissivity();
        }

        protected override void OnOwnershipChanged()
        {
            base.OnOwnershipChanged();
            this.RadioBroadcaster.RaiseOwnerChanged();
        }

        protected override void OnStartWorking()
        {
            base.OnStartWorking();
            if ((base.NeedsUpdate & MyEntityUpdateEnum.EACH_FRAME) == MyEntityUpdateEnum.NONE)
            {
                this.m_restartTimeMeasure = true;
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        protected override void OnStopWorking()
        {
            base.OnStopWorking();
            if ((base.NeedsUpdate & MyEntityUpdateEnum.EACH_FRAME) == MyEntityUpdateEnum.NONE)
            {
                this.m_restartTimeMeasure = true;
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        private void Receiver_IsPoweredChanged()
        {
            if (this.RadioBroadcaster != null)
            {
                this.RadioBroadcaster.Enabled = base.IsWorking;
            }
            this.UpdatePower();
            this.UpdateLightProperties();
            base.UpdateIsWorking();
            this.UpdateText();
        }

        public override bool SetEmissiveStateDamaged() => 
            false;

        public override bool SetEmissiveStateDisabled() => 
            false;

        public override bool SetEmissiveStateWorking() => 
            false;

        private void SetHudText(string text)
        {
            if (this.HudText.CompareUpdate(text))
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyBeacon, string>(this, x => new Action<string>(x.SetHudTextEvent), text, targetEndpoint);
            }
        }

        private void SetHudText(StringBuilder text)
        {
            this.SetHudText(text.ToString());
        }

        [Event(null, 0x7d), Reliable, Server(ValidationType.Ownership | ValidationType.Access), BroadcastExcept]
        protected void SetHudTextEvent(string text)
        {
            this.HudText.CompareUpdate(text);
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            float currentLightPower = this.m_currentLightPower;
            float num2 = 0f;
            if (!this.m_restartTimeMeasure)
            {
                num2 = ((float) (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastAnimationUpdateTime)) / 1000f;
            }
            else
            {
                this.m_restartTimeMeasure = false;
            }
            float num3 = base.IsWorking ? 1f : -1f;
            this.m_currentLightPower = MathHelper.Clamp((float) (this.m_currentLightPower + ((num3 * num2) / 0.5f)), (float) 0f, (float) 1f);
            this.m_lastAnimationUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (this.m_light != null)
            {
                if (this.m_currentLightPower <= 0f)
                {
                    this.m_light.LightOn = false;
                    this.m_light.GlareOn = false;
                }
                else
                {
                    this.m_light.LightOn = true;
                    this.m_light.GlareOn = true;
                }
                if (currentLightPower != this.m_currentLightPower)
                {
                    this.UpdateLightPosition();
                    this.UpdateLightParent();
                    this.m_light.UpdateLight();
                    this.UpdateEmissivity();
                    this.UpdateLightProperties();
                }
            }
            if (this.m_currentLightPower == ((num3 * 0.5f) + 0.5f))
            {
                this.AnimationRunning = false;
            }
        }

        private void UpdateEmissivity()
        {
            Color emissiveColor = COLOR_OFF;
            Color emissiveColor = COLOR_ON;
            if (base.UsesEmissivePreset)
            {
                MyEmissiveColorStateResult result;
                if (MyEmissiveColorPresets.LoadPresetState(base.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Working, out result))
                {
                    emissiveColor = result.EmissiveColor;
                }
                if (MyEmissiveColorPresets.LoadPresetState(base.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Disabled, out result))
                {
                    emissiveColor = result.EmissiveColor;
                }
            }
            UpdateEmissiveParts(base.Render.RenderObjectIDs[0], this.m_currentLightPower, Color.Lerp(emissiveColor, emissiveColor, this.m_currentLightPower), Color.White);
        }

        private void UpdateLightParent()
        {
            if (this.m_light != null)
            {
                uint parentCullObject = base.CubeGrid.Render.RenderData.GetOrAddCell((Vector3) (base.Position * base.CubeGrid.GridSize), true).ParentCullObject;
                this.m_light.ParentID = parentCullObject;
            }
        }

        private void UpdateLightPosition()
        {
            if (this.m_light != null)
            {
                MatrixD localMatrix = base.PositionComp.LocalMatrix;
                this.m_light.Position = Vector3D.Transform(this.m_lightPositionOffset, localMatrix);
                if (!this.AnimationRunning)
                {
                    this.m_light.UpdateLight();
                }
            }
        }

        private void UpdateLightProperties()
        {
            if (this.m_light != null)
            {
                Color color = Color.Lerp(COLOR_OFF, COLOR_ON, this.m_currentLightPower);
                float num = this.m_largeLight ? POINT_LIGHT_RANGE_LARGE : POINT_LIGHT_RANGE_SMALL;
                float num2 = this.m_currentLightPower * (this.m_largeLight ? POINT_LIGHT_INTENSITY_LARGE : POINT_LIGHT_INTENSITY_SMALL);
                this.m_light.Color = color;
                this.m_light.Range = num;
                this.m_light.Intensity = num2;
                this.m_light.GlareIntensity = this.m_currentLightPower * this.m_flare.Intensity;
                this.m_light.UpdateLight();
            }
        }

        private void UpdatePower()
        {
            this.AnimationRunning = true;
        }

        private float UpdatePowerInput()
        {
            float num = this.RadioBroadcaster.BroadcastRadius / 100000f;
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return (num * 0.02f);
        }

        private void UpdateText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(base.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyProperties_CurrentInput));
            MyValueFormatter.AppendWorkInBestUnit(base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) ? base.ResourceSink.RequiredInputByType(MyResourceDistributorComponent.ElectricityId) : 0f, base.DetailedInfo);
            base.RaisePropertiesChanged();
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.UpdateEmissivity();
        }

        protected override void WorldPositionChanged(object source)
        {
            base.WorldPositionChanged(source);
            if (this.RadioBroadcaster != null)
            {
                this.RadioBroadcaster.MoveBroadcaster();
            }
        }

        internal MyRadioBroadcaster RadioBroadcaster
        {
            get => 
                ((MyRadioBroadcaster) base.Components.Get<MyDataBroadcaster>());
            private set => 
                base.Components.Add<MyDataBroadcaster>(value);
        }

        public StringBuilder HudText { get; private set; }

        internal bool AnimationRunning
        {
            get => 
                this.m_animationRunning;
            private set
            {
                if (this.m_animationRunning != value)
                {
                    this.m_animationRunning = value;
                    if (value)
                    {
                        base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                        this.m_lastAnimationUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                    }
                    else if (base.IsFunctional && !base.HasDamageEffect)
                    {
                        base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                    }
                }
            }
        }

        float Sandbox.ModAPI.Ingame.IMyBeacon.Radius
        {
            get => 
                this.RadioBroadcaster.BroadcastRadius;
            set => 
                (this.RadioBroadcaster.BroadcastRadius = MathHelper.Clamp(value, 0f, ((MyBeaconDefinition) base.BlockDefinition).MaxBroadcastRadius));
        }

        string Sandbox.ModAPI.Ingame.IMyBeacon.HudText
        {
            get => 
                this.HudText.ToString();
            set => 
                this.SetHudText(value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyBeacon.<>c <>9 = new MyBeacon.<>c();
            public static MyTerminalControlTextbox<MyBeacon>.GetterDelegate <>9__21_0;
            public static MyTerminalControlTextbox<MyBeacon>.SetterDelegate <>9__21_1;
            public static MyTerminalControlTextbox<MyBeacon>.GetterDelegate <>9__21_2;
            public static MyTerminalControlTextbox<MyBeacon>.SetterDelegate <>9__21_3;
            public static MyTerminalValueControl<MyBeacon, float>.GetterDelegate <>9__21_4;
            public static MyTerminalValueControl<MyBeacon, float>.GetterDelegate <>9__21_5;
            public static MyTerminalValueControl<MyBeacon, float>.GetterDelegate <>9__21_6;
            public static MyTerminalValueControl<MyBeacon, float>.GetterDelegate <>9__21_7;
            public static MyTerminalValueControl<MyBeacon, float>.SetterDelegate <>9__21_8;
            public static MyTerminalControl<MyBeacon>.WriterDelegate <>9__21_9;
            public static Func<MyBeacon, Action<string>> <>9__27_0;

            internal StringBuilder <CreateTerminalControls>b__21_0(MyBeacon x) => 
                x.CustomName;

            internal void <CreateTerminalControls>b__21_1(MyBeacon x, StringBuilder v)
            {
                x.SetCustomName(v);
            }

            internal StringBuilder <CreateTerminalControls>b__21_2(MyBeacon x) => 
                x.HudText;

            internal void <CreateTerminalControls>b__21_3(MyBeacon x, StringBuilder v)
            {
                x.SetHudText(v);
            }

            internal float <CreateTerminalControls>b__21_4(MyBeacon x) => 
                1f;

            internal float <CreateTerminalControls>b__21_5(MyBeacon x) => 
                (x.BlockDefinition as MyBeaconDefinition).MaxBroadcastRadius;

            internal float <CreateTerminalControls>b__21_6(MyBeacon x) => 
                ((x.BlockDefinition as MyBeaconDefinition).MaxBroadcastRadius / 10f);

            internal float <CreateTerminalControls>b__21_7(MyBeacon x) => 
                x.RadioBroadcaster.BroadcastRadius;

            internal void <CreateTerminalControls>b__21_8(MyBeacon x, float v)
            {
                x.m_radius.Value = v;
            }

            internal void <CreateTerminalControls>b__21_9(MyBeacon x, StringBuilder result)
            {
                result.AppendDecimal(x.RadioBroadcaster.BroadcastRadius, 0).Append(" m");
            }

            internal Action<string> <SetHudText>b__27_0(MyBeacon x) => 
                new Action<string>(x.SetHudTextEvent);
        }
    }
}

