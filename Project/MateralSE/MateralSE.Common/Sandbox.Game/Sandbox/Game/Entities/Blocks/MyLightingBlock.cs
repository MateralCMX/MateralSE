namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Lights;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyLightingBlock), typeof(Sandbox.ModAPI.Ingame.IMyLightingBlock) })]
    public abstract class MyLightingBlock : MyFunctionalBlock, Sandbox.ModAPI.IMyLightingBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyLightingBlock
    {
        private const double MIN_MOVEMENT_SQUARED_FOR_UPDATE = 0.0001;
        private const int NUM_DECIMALS = 1;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_blinkIntervalSeconds;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_blinkLength;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_blinkOffset;
        protected MyLight m_light;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_intensity;
        private readonly VRage.Sync.Sync<VRageMath.Color, SyncDirection.BothWays> m_lightColor;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_lightRadius;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_lightFalloff;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_lightOffset;
        private Vector3 m_lightLocalPosition;
        private readonly float m_lightTurningOnSpeed = 0.05f;
        private bool m_positionDirty = true;
        private MatrixD m_oldWorldMatrix = MatrixD.Zero;
        private const int MaxLightUpdateDistance = 0x1388;
        private bool m_emissiveMaterialDirty;
        private VRageMath.Color m_bulbColor = VRageMath.Color.Black;
        private float m_currentLightPower;
        private bool m_blinkOn = true;
        private float m_radius;
        private float m_reflectorRadius;
        private VRageMath.Color m_color;
        private float m_falloff;

        public MyLightingBlock()
        {
            this.CreateTerminalControls();
            this.m_lightColor.ValueChanged += x => this.LightColorChanged();
            this.m_lightRadius.ValueChanged += x => this.LightRadiusChanged();
            this.m_lightFalloff.ValueChanged += x => this.LightFalloffChanged();
            this.m_lightOffset.ValueChanged += x => this.LightOffsetChanged();
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        protected override void Closing()
        {
            MyLights.RemoveLight(this.m_light);
            base.Closing();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        protected VRageMath.Color ComputeBulbColor()
        {
            float num2 = 0.125f + (this.IntensityBounds.Normalize(this.Intensity) * 0.25f);
            return new VRageMath.Color((this.Color.R * 0.5f) + num2, (this.Color.G * 0.5f) + num2, (this.Color.B * 0.5f) + num2);
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyLightingBlock>())
            {
                base.CreateTerminalControls();
                MyTerminalControlColor<MyLightingBlock> color1 = new MyTerminalControlColor<MyLightingBlock>("Color", MySpaceTexts.BlockPropertyTitle_LightColor);
                MyTerminalControlColor<MyLightingBlock> color2 = new MyTerminalControlColor<MyLightingBlock>("Color", MySpaceTexts.BlockPropertyTitle_LightColor);
                color2.Getter = x => x.Color;
                MyTerminalControlColor<MyLightingBlock> local111 = color2;
                MyTerminalControlColor<MyLightingBlock> control = color2;
                control.Setter = (x, v) => x.m_lightColor.Value = v;
                MyTerminalControlFactory.AddControl<MyLightingBlock>(control);
                MyTerminalControlSlider<MyLightingBlock> slider13 = new MyTerminalControlSlider<MyLightingBlock>("Radius", MySpaceTexts.BlockPropertyTitle_LightRadius, MySpaceTexts.BlockPropertyDescription_LightRadius);
                MyTerminalControlSlider<MyLightingBlock> slider14 = new MyTerminalControlSlider<MyLightingBlock>("Radius", MySpaceTexts.BlockPropertyTitle_LightRadius, MySpaceTexts.BlockPropertyDescription_LightRadius);
                slider14.SetLimits(x => x.IsReflector ? ((MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) x.ReflectorRadiusBounds.Min) : ((MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) x.RadiusBounds.Min), x => x.IsReflector ? ((MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) x.ReflectorRadiusBounds.Max) : ((MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) x.RadiusBounds.Max));
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local109 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider14;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local110 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider14;
                local110.DefaultValueGetter = x => x.IsReflector ? ((MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) x.ReflectorRadiusBounds.Default) : ((MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) x.RadiusBounds.Default);
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local107 = local110;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local108 = local110;
                local108.Getter = x => x.IsReflector ? ((MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) x.ReflectorRadius) : ((MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) x.Radius);
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local105 = local108;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local106 = local108;
                local106.Setter = (x, v) => x.m_lightRadius.Value = v;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local103 = local106;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local104 = local106;
                local104.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.IsReflector ? x.m_reflectorRadius : x.m_radius, 1)).Append(" m");
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local9 = local104;
                ((MyTerminalControlSlider<MyLightingBlock>) local9).EnableActions<MyLightingBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyLightingBlock>((MyTerminalControl<MyLightingBlock>) local9);
                MyTerminalControlSlider<MyLightingBlock> slider11 = new MyTerminalControlSlider<MyLightingBlock>("Falloff", MySpaceTexts.BlockPropertyTitle_LightFalloff, MySpaceTexts.BlockPropertyDescription_LightFalloff);
                MyTerminalControlSlider<MyLightingBlock> slider12 = new MyTerminalControlSlider<MyLightingBlock>("Falloff", MySpaceTexts.BlockPropertyTitle_LightFalloff, MySpaceTexts.BlockPropertyDescription_LightFalloff);
                slider12.SetLimits(x => x.FalloffBounds.Min, x => x.FalloffBounds.Max);
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local101 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider12;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local102 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider12;
                local102.DefaultValueGetter = x => x.FalloffBounds.Default;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local99 = local102;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local100 = local102;
                local100.Getter = x => x.Falloff;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local97 = local100;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local98 = local100;
                local98.Setter = (x, v) => x.m_lightFalloff.Value = v;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local95 = local98;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local96 = local98;
                local96.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.Falloff, 1));
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local93 = local96;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local94 = local96;
                local94.Visible = x => x.SupportsFalloff;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local17 = local94;
                ((MyTerminalControlSlider<MyLightingBlock>) local17).EnableActions<MyLightingBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyLightingBlock>((MyTerminalControl<MyLightingBlock>) local17);
                MyTerminalControlSlider<MyLightingBlock> slider9 = new MyTerminalControlSlider<MyLightingBlock>("Intensity", MySpaceTexts.BlockPropertyTitle_LightIntensity, MySpaceTexts.BlockPropertyDescription_LightIntensity);
                MyTerminalControlSlider<MyLightingBlock> slider10 = new MyTerminalControlSlider<MyLightingBlock>("Intensity", MySpaceTexts.BlockPropertyTitle_LightIntensity, MySpaceTexts.BlockPropertyDescription_LightIntensity);
                slider10.SetLimits(x => x.IntensityBounds.Min, x => x.IntensityBounds.Max);
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local91 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider10;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local92 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider10;
                local92.DefaultValueGetter = x => x.IntensityBounds.Default;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local89 = local92;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local90 = local92;
                local90.Getter = x => x.Intensity;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local87 = local90;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local88 = local90;
                local88.Setter = (x, v) => x.Intensity = v;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local85 = local88;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local86 = local88;
                local86.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.Intensity, 1));
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local24 = local86;
                ((MyTerminalControlSlider<MyLightingBlock>) local24).EnableActions<MyLightingBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyLightingBlock>((MyTerminalControl<MyLightingBlock>) local24);
                MyTerminalControlSlider<MyLightingBlock> slider7 = new MyTerminalControlSlider<MyLightingBlock>("Offset", MySpaceTexts.BlockPropertyTitle_LightOffset, MySpaceTexts.BlockPropertyDescription_LightOffset);
                MyTerminalControlSlider<MyLightingBlock> slider8 = new MyTerminalControlSlider<MyLightingBlock>("Offset", MySpaceTexts.BlockPropertyTitle_LightOffset, MySpaceTexts.BlockPropertyDescription_LightOffset);
                slider8.SetLimits(x => x.OffsetBounds.Min, x => x.OffsetBounds.Max);
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local83 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider8;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local84 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider8;
                local84.DefaultValueGetter = x => x.OffsetBounds.Default;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local81 = local84;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local82 = local84;
                local82.Getter = x => x.Offset;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local79 = local82;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local80 = local82;
                local80.Setter = (x, v) => x.m_lightOffset.Value = v;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local77 = local80;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local78 = local80;
                local78.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.Offset, 1));
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local31 = local78;
                ((MyTerminalControlSlider<MyLightingBlock>) local31).EnableActions<MyLightingBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyLightingBlock>((MyTerminalControl<MyLightingBlock>) local31);
                MyTerminalControlSlider<MyLightingBlock> slider5 = new MyTerminalControlSlider<MyLightingBlock>("Blink Interval", MySpaceTexts.BlockPropertyTitle_LightBlinkInterval, MySpaceTexts.BlockPropertyDescription_LightBlinkInterval);
                MyTerminalControlSlider<MyLightingBlock> slider6 = new MyTerminalControlSlider<MyLightingBlock>("Blink Interval", MySpaceTexts.BlockPropertyTitle_LightBlinkInterval, MySpaceTexts.BlockPropertyDescription_LightBlinkInterval);
                slider6.SetLimits(x => x.BlinkIntervalSecondsBounds.Min, x => x.BlinkIntervalSecondsBounds.Max);
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local75 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider6;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local76 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider6;
                local76.DefaultValueGetter = x => x.BlinkIntervalSecondsBounds.Default;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local73 = local76;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local74 = local76;
                local74.Getter = x => x.BlinkIntervalSeconds;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local71 = local74;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local72 = local74;
                local72.Setter = (x, v) => x.BlinkIntervalSeconds = v;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local69 = local72;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local70 = local72;
                local70.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.BlinkIntervalSeconds, 1)).Append(" s");
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local38 = local70;
                ((MyTerminalControlSlider<MyLightingBlock>) local38).EnableActions<MyLightingBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyLightingBlock>((MyTerminalControl<MyLightingBlock>) local38);
                MyTerminalControlSlider<MyLightingBlock> slider3 = new MyTerminalControlSlider<MyLightingBlock>("Blink Lenght", MySpaceTexts.BlockPropertyTitle_LightBlinkLenght, MySpaceTexts.BlockPropertyDescription_LightBlinkLenght);
                MyTerminalControlSlider<MyLightingBlock> slider4 = new MyTerminalControlSlider<MyLightingBlock>("Blink Lenght", MySpaceTexts.BlockPropertyTitle_LightBlinkLenght, MySpaceTexts.BlockPropertyDescription_LightBlinkLenght);
                slider4.SetLimits(x => x.BlinkLenghtBounds.Min, x => x.BlinkLenghtBounds.Max);
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local67 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider4;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local68 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider4;
                local68.DefaultValueGetter = x => x.BlinkLenghtBounds.Default;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local65 = local68;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local66 = local68;
                local66.Getter = x => x.BlinkLength;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local63 = local66;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local64 = local66;
                local64.Setter = (x, v) => x.BlinkLength = v;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local61 = local64;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local62 = local64;
                local62.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.BlinkLength, 1)).Append(" %");
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local45 = local62;
                ((MyTerminalControlSlider<MyLightingBlock>) local45).EnableActions<MyLightingBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyLightingBlock>((MyTerminalControl<MyLightingBlock>) local45);
                MyTerminalControlSlider<MyLightingBlock> slider1 = new MyTerminalControlSlider<MyLightingBlock>("Blink Offset", MySpaceTexts.BlockPropertyTitle_LightBlinkOffset, MySpaceTexts.BlockPropertyDescription_LightBlinkOffset);
                MyTerminalControlSlider<MyLightingBlock> slider2 = new MyTerminalControlSlider<MyLightingBlock>("Blink Offset", MySpaceTexts.BlockPropertyTitle_LightBlinkOffset, MySpaceTexts.BlockPropertyDescription_LightBlinkOffset);
                slider2.SetLimits(x => x.BlinkOffsetBounds.Min, x => x.BlinkOffsetBounds.Max);
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local59 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider2;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local60 = (MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate) slider2;
                local60.DefaultValueGetter = x => x.BlinkOffsetBounds.Default;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local57 = local60;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local58 = local60;
                local58.Getter = x => x.BlinkOffset;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local55 = local58;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local56 = local58;
                local56.Setter = (x, v) => x.BlinkOffset = v;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local53 = local56;
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local54 = local56;
                local54.Writer = (x, result) => result.Append(MyValueFormatter.GetFormatedFloat(x.BlinkOffset, 1)).Append(" %");
                MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate local52 = local54;
                ((MyTerminalControlSlider<MyLightingBlock>) local52).EnableActions<MyLightingBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MyLightingBlock>((MyTerminalControl<MyLightingBlock>) local52);
            }
        }

        private void CubeBlock_OnWorkingChanged(MyCubeBlock block)
        {
            this.m_positionDirty = true;
        }

        private float GetNewLightPower() => 
            MathHelper.Clamp((float) (this.CurrentLightPower + ((base.IsWorking ? ((float) 1) : ((float) (-1))) * this.m_lightTurningOnSpeed)), (float) 0f, (float) 1f);

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            Vector4 vector = this.m_color.ToVector4();
            MyObjectBuilder_LightingBlock objectBuilderCubeBlock = (MyObjectBuilder_LightingBlock) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.ColorRed = vector.X;
            objectBuilderCubeBlock.ColorGreen = vector.Y;
            objectBuilderCubeBlock.ColorBlue = vector.Z;
            objectBuilderCubeBlock.ColorAlpha = vector.W;
            objectBuilderCubeBlock.Radius = this.m_radius;
            objectBuilderCubeBlock.ReflectorRadius = this.m_reflectorRadius;
            objectBuilderCubeBlock.Falloff = this.Falloff;
            objectBuilderCubeBlock.Intensity = (float) this.m_intensity;
            objectBuilderCubeBlock.BlinkIntervalSeconds = (float) this.m_blinkIntervalSeconds;
            objectBuilderCubeBlock.BlinkLenght = (float) this.m_blinkLength;
            objectBuilderCubeBlock.BlinkOffset = (float) this.m_blinkOffset;
            objectBuilderCubeBlock.Offset = (float) this.m_lightOffset;
            return objectBuilderCubeBlock;
        }

        public override unsafe void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyBounds* boundsPtr1;
            MyBounds* boundsPtr2;
            MyBounds* boundsPtr3;
            MyBounds* boundsPtr4;
            MyBounds* boundsPtr5;
            MyBounds* boundsPtr6;
            MyBounds* boundsPtr7;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.BlockDefinition.ResourceSinkGroup, this.BlockDefinition.RequiredPowerInput, delegate {
                if (!base.Enabled || !base.IsFunctional)
                {
                    return 0f;
                }
                return base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
            });
            component.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink = component;
            base.Init(objectBuilder, cubeGrid);
            this.IsLargeLight = cubeGrid.GridSizeEnum == MyCubeSize.Large;
            MyObjectBuilder_LightingBlock block = (MyObjectBuilder_LightingBlock) objectBuilder;
            foreach (KeyValuePair<string, MyModelDummy> pair in MyModels.GetModelOnlyDummies(this.BlockDefinition.Model).Dummies)
            {
                if (pair.Key.ToLower().Contains("light"))
                {
                    this.m_lightLocalPosition = pair.Value.Matrix.Translation;
                    break;
                }
            }
            this.m_color = (block.ColorAlpha == -1f) ? this.LightColorDef : new Vector4(block.ColorRed, block.ColorGreen, block.ColorBlue, block.ColorAlpha);
            this.m_radius = boundsPtr1.Clamp((block.Radius == -1f) ? this.RadiusBounds.Default : block.Radius);
            boundsPtr1 = &this.RadiusBounds;
            this.m_reflectorRadius = boundsPtr2.Clamp((block.ReflectorRadius == -1f) ? this.ReflectorRadiusBounds.Default : block.ReflectorRadius);
            boundsPtr2 = &this.ReflectorRadiusBounds;
            this.m_falloff = boundsPtr3.Clamp((block.Falloff == -1f) ? this.FalloffBounds.Default : block.Falloff);
            boundsPtr3 = &this.FalloffBounds;
            this.m_blinkIntervalSeconds.SetLocalValue(boundsPtr4.Clamp((block.BlinkIntervalSeconds == -1f) ? this.BlinkIntervalSecondsBounds.Default : block.BlinkIntervalSeconds));
            boundsPtr4 = &this.BlinkIntervalSecondsBounds;
            this.m_blinkLength.SetLocalValue(boundsPtr5.Clamp((block.BlinkLenght == -1f) ? this.BlinkLenghtBounds.Default : block.BlinkLenght));
            boundsPtr5 = &this.BlinkLenghtBounds;
            this.m_blinkOffset.SetLocalValue(boundsPtr6.Clamp((block.BlinkOffset == -1f) ? this.BlinkOffsetBounds.Default : block.BlinkOffset));
            boundsPtr6 = &this.BlinkOffsetBounds;
            this.m_intensity.SetLocalValue(boundsPtr7.Clamp((block.Intensity == -1f) ? this.IntensityBounds.Default : block.Intensity));
            boundsPtr7 = &this.IntensityBounds;
            this.m_lightOffset.SetLocalValue(this.OffsetBounds.Clamp((block.Offset == -1f) ? this.OffsetBounds.Default : block.Offset));
            this.m_positionDirty = true;
            this.m_light = MyLights.AddLight();
            if (this.m_light != null)
            {
                this.InitLight(this.m_light, (Vector4) this.m_color, this.m_radius, this.m_falloff);
                this.m_light.ReflectorColor = this.m_color;
                this.m_light.ReflectorRange = this.m_reflectorRadius;
                this.m_light.Range = this.m_radius;
                this.m_light.ReflectorConeDegrees = this.ReflectorConeDegrees;
                this.UpdateRadius(this.IsReflector ? this.m_reflectorRadius : this.m_radius);
            }
            this.UpdateIntensity();
            this.UpdateLightPosition();
            this.UpdateLightBlink();
            this.UpdateEnabled();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
            base.ResourceSink.Update();
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.CubeBlock_OnWorkingChanged);
        }

        protected abstract void InitLight(MyLight light, Vector4 color, float radius, float falloff);
        private void LightColorChanged()
        {
            this.Color = this.m_lightColor.Value;
        }

        private void LightFalloffChanged()
        {
            this.Falloff = this.m_lightFalloff.Value;
        }

        private void LightOffsetChanged()
        {
            this.UpdateLightProperties();
        }

        private void LightRadiusChanged()
        {
            this.UpdateRadius(this.m_lightRadius.Value);
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if (this.m_light != null)
            {
                uint parentCullObject = base.CubeGrid.Render.RenderData.GetOrAddCell((Vector3) (base.Position * base.CubeGrid.GridSize), true).ParentCullObject;
                this.m_light.ParentID = parentCullObject;
            }
            this.UpdateLightPosition();
            this.UpdateLightProperties();
            this.UpdateEmissivity(true);
        }

        public override void OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            base.OnCubeGridChanged(oldGrid);
            this.m_positionDirty = true;
        }

        protected override void OnEnabledChanged()
        {
            base.ResourceSink.Update();
            base.OnEnabledChanged();
        }

        public override void OnRegisteredToGridSystems()
        {
            base.OnRegisteredToGridSystems();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        protected override void OnStartWorking()
        {
            base.OnStartWorking();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        protected override void OnStopWorking()
        {
            base.OnStopWorking();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if ((MySector.MainCamera.Position - base.PositionComp.GetPosition()).AbsMax() <= 5000.0)
            {
                if (this.m_light != null)
                {
                    uint parentCullObject = base.CubeGrid.Render.RenderData.GetOrAddCell((Vector3) (base.Position * base.CubeGrid.GridSize), true).ParentCullObject;
                    this.m_light.ParentID = parentCullObject;
                }
                float newLightPower = this.GetNewLightPower();
                if (newLightPower != this.CurrentLightPower)
                {
                    this.CurrentLightPower = newLightPower;
                    this.UpdateIntensity();
                }
                this.UpdateLightBlink();
                this.UpdateEnabled();
                this.UpdateLightProperties();
                this.UpdateEmissivity(false);
            }
        }

        public override void UpdateAfterSimulation100()
        {
            if (((MySector.MainCamera.Position - base.PositionComp.GetPosition()).AbsMax() > 5000.0) && !base.HasDamageEffect)
            {
                base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
            }
            else
            {
                base.NeedsUpdate = !((base.HasDamageEffect | (this.m_blinkIntervalSeconds > 0f)) | !(this.GetNewLightPower() == this.CurrentLightPower)) ? (base.NeedsUpdate & ~MyEntityUpdateEnum.EACH_FRAME) : (base.NeedsUpdate | MyEntityUpdateEnum.EACH_FRAME);
                this.UpdateLightProperties();
            }
        }

        private void UpdateEmissiveMaterial()
        {
            if (this.m_emissiveMaterialDirty)
            {
                MyRenderProxy.UpdateModelProperties(base.Render.RenderObjectIDs[0], "Emissive", 0, 0, new VRageMath.Color?(this.BulbColor), new float?(this.CurrentLightPower));
                MyRenderProxy.UpdateModelProperties(base.Render.RenderObjectIDs[0], "EmissiveSpotlight", 0, 0, new VRageMath.Color?(this.BulbColor), new float?(this.CurrentLightPower));
                this.m_emissiveMaterialDirty = false;
            }
        }

        protected virtual void UpdateEmissivity(bool force = false)
        {
        }

        private void UpdateEnabled()
        {
            this.UpdateEnabled(((this.CurrentLightPower * this.Intensity) > 0f) && this.m_blinkOn);
        }

        protected abstract void UpdateEnabled(bool state);
        protected abstract void UpdateIntensity();
        private void UpdateLightBlink()
        {
            if (this.m_blinkIntervalSeconds <= 0.00099f)
            {
                this.m_blinkOn = true;
            }
            else
            {
                ulong num = (ulong) (this.m_blinkIntervalSeconds * 1000f);
                float num2 = (num * this.m_blinkOffset) * 0.01f;
                ulong num4 = (ulong) ((num * this.m_blinkLength) * 0.01f);
                this.m_blinkOn = num4 > (((ulong) (MySession.Static.ElapsedGameTime.TotalMilliseconds - num2)) % num);
            }
        }

        private void UpdateLightPosition()
        {
            if ((this.m_light != null) && this.m_positionDirty)
            {
                this.m_positionDirty = false;
                MatrixD localMatrix = base.PositionComp.LocalMatrix;
                this.m_light.Position = Vector3D.Transform(this.m_lightLocalPosition, localMatrix);
                this.m_light.ReflectorDirection = (Vector3) localMatrix.Forward;
                this.m_light.ReflectorUp = (Vector3) localMatrix.Up;
            }
        }

        private void UpdateLightProperties()
        {
            if (this.m_light != null)
            {
                this.m_light.Range = this.m_radius;
                this.m_light.ReflectorRange = this.m_reflectorRadius;
                this.m_light.Color = this.m_color;
                this.m_light.ReflectorColor = this.m_color;
                this.m_light.Falloff = this.m_falloff;
                this.m_light.PointLightOffset = this.Offset;
                this.m_light.UpdateLight();
            }
        }

        protected virtual void UpdateRadius(float value)
        {
            if (this.IsReflector)
            {
                this.ReflectorRadius = value;
            }
            else
            {
                this.Radius = value;
            }
        }

        public MyLightingBlockDefinition BlockDefinition =>
            ((MyLightingBlockDefinition) base.BlockDefinition);

        public MyBounds BlinkIntervalSecondsBounds =>
            this.BlockDefinition.BlinkIntervalSeconds;

        public MyBounds BlinkLenghtBounds =>
            this.BlockDefinition.BlinkLenght;

        public MyBounds BlinkOffsetBounds =>
            this.BlockDefinition.BlinkOffset;

        public MyBounds FalloffBounds =>
            this.BlockDefinition.LightFalloff;

        public MyBounds OffsetBounds =>
            this.BlockDefinition.LightOffset;

        public MyBounds RadiusBounds =>
            this.BlockDefinition.LightRadius;

        public MyBounds ReflectorRadiusBounds =>
            this.BlockDefinition.LightReflectorRadius;

        public MyBounds IntensityBounds =>
            this.BlockDefinition.LightIntensity;

        public float ReflectorConeDegrees =>
            52f;

        public Vector4 LightColorDef =>
            (this.IsLargeLight ? new VRageMath.Color(0xff, 0xff, 0xde) : new VRageMath.Color(0xce, 0xeb, 0xff)).ToVector4();

        public float ReflectorIntensityDef =>
            (this.IsLargeLight ? 0.5f : 1.137f);

        public bool IsLargeLight { get; private set; }

        public abstract bool IsReflector { get; }

        protected abstract bool SupportsFalloff { get; }

        public VRageMath.Color Color
        {
            get => 
                this.m_color;
            set
            {
                if (this.m_color != value)
                {
                    this.m_color = value;
                    this.BulbColor = this.ComputeBulbColor();
                    this.UpdateEmissivity(true);
                    this.UpdateLightProperties();
                    base.RaisePropertiesChanged();
                }
            }
        }

        public float Radius
        {
            get => 
                this.m_radius;
            set
            {
                if (this.m_radius != value)
                {
                    this.m_radius = value;
                    this.UpdateLightProperties();
                    base.RaisePropertiesChanged();
                }
            }
        }

        public float ReflectorRadius
        {
            get => 
                this.m_reflectorRadius;
            set
            {
                if (this.m_reflectorRadius != value)
                {
                    this.m_reflectorRadius = value;
                    this.UpdateLightProperties();
                    base.RaisePropertiesChanged();
                }
            }
        }

        public float BlinkLength
        {
            get => 
                ((float) this.m_blinkLength);
            set
            {
                if (this.m_blinkLength != value)
                {
                    this.m_blinkLength.Value = (float) Math.Round((double) value, 1);
                    base.RaisePropertiesChanged();
                }
            }
        }

        public float BlinkOffset
        {
            get => 
                ((float) this.m_blinkOffset);
            set
            {
                if (this.m_blinkOffset != value)
                {
                    this.m_blinkOffset.Value = (float) Math.Round((double) value, 1);
                    base.RaisePropertiesChanged();
                }
            }
        }

        public float BlinkIntervalSeconds
        {
            get => 
                ((float) this.m_blinkIntervalSeconds);
            set
            {
                if (this.m_blinkIntervalSeconds != value)
                {
                    this.m_blinkIntervalSeconds.Value = (value <= this.m_blinkIntervalSeconds) ? ((float) Math.Round((double) (value - 0.04999f), 1)) : ((float) Math.Round((double) (value + 0.04999f), 1));
                    if ((this.m_blinkIntervalSeconds == 0f) && base.Enabled)
                    {
                        this.UpdateEnabled();
                    }
                    base.RaisePropertiesChanged();
                }
            }
        }

        public virtual float Falloff
        {
            get => 
                this.m_falloff;
            set
            {
                if (this.m_falloff != value)
                {
                    this.m_falloff = value;
                    this.UpdateIntensity();
                    this.UpdateLightProperties();
                    base.RaisePropertiesChanged();
                }
            }
        }

        public float Intensity
        {
            get => 
                ((float) this.m_intensity);
            set
            {
                if (this.m_intensity != value)
                {
                    this.m_intensity.Value = value;
                    this.UpdateIntensity();
                    this.UpdateLightProperties();
                    base.RaisePropertiesChanged();
                }
            }
        }

        public float Offset
        {
            get => 
                ((float) this.m_lightOffset);
            set
            {
                if (this.m_lightOffset != value)
                {
                    this.m_lightOffset.Value = value;
                    this.UpdateLightProperties();
                    base.RaisePropertiesChanged();
                }
            }
        }

        public float CurrentLightPower
        {
            get => 
                this.m_currentLightPower;
            set
            {
                if (this.m_currentLightPower != value)
                {
                    this.m_currentLightPower = value;
                    this.m_emissiveMaterialDirty = true;
                }
            }
        }

        public VRageMath.Color BulbColor
        {
            get => 
                this.m_bulbColor;
            set
            {
                if (this.m_bulbColor != value)
                {
                    this.m_bulbColor = value;
                    this.m_emissiveMaterialDirty = true;
                }
            }
        }

        float Sandbox.ModAPI.Ingame.IMyLightingBlock.ReflectorRadius =>
            this.ReflectorRadius;

        float Sandbox.ModAPI.Ingame.IMyLightingBlock.BlinkLenght =>
            this.BlinkLength;

        float Sandbox.ModAPI.Ingame.IMyLightingBlock.Radius
        {
            get => 
                (this.IsReflector ? this.ReflectorRadius : this.Radius);
            set
            {
                float single1 = MathHelper.Clamp(value, this.RadiusBounds.Min, this.RadiusBounds.Max);
                float single2 = MathHelper.Clamp(value, this.ReflectorRadiusBounds.Min, this.ReflectorRadiusBounds.Max);
                value = this.IsReflector ? single2 : single1;
                this.m_lightRadius.Value = value;
            }
        }

        float Sandbox.ModAPI.Ingame.IMyLightingBlock.Intensity
        {
            get => 
                ((float) this.m_intensity);
            set
            {
                float single1 = MathHelper.Clamp(value, this.IntensityBounds.Min, this.IntensityBounds.Max);
                value = single1;
                this.m_intensity.Value = value;
            }
        }

        float Sandbox.ModAPI.Ingame.IMyLightingBlock.Falloff
        {
            get => 
                ((float) this.m_lightFalloff);
            set
            {
                float single1 = MathHelper.Clamp(value, this.FalloffBounds.Min, this.FalloffBounds.Max);
                value = single1;
                this.m_lightFalloff.Value = value;
            }
        }

        float Sandbox.ModAPI.Ingame.IMyLightingBlock.BlinkIntervalSeconds
        {
            get => 
                this.BlinkIntervalSeconds;
            set
            {
                float single1 = MathHelper.Clamp(value, this.BlinkIntervalSecondsBounds.Min, this.BlinkIntervalSecondsBounds.Max);
                value = single1;
                this.BlinkIntervalSeconds = value;
            }
        }

        float Sandbox.ModAPI.Ingame.IMyLightingBlock.BlinkLength
        {
            get => 
                this.BlinkLength;
            set
            {
                float single1 = MathHelper.Clamp(value, this.BlinkLenghtBounds.Min, this.BlinkLenghtBounds.Max);
                value = single1;
                this.BlinkLength = value;
            }
        }

        float Sandbox.ModAPI.Ingame.IMyLightingBlock.BlinkOffset
        {
            get => 
                this.BlinkOffset;
            set
            {
                float single1 = MathHelper.Clamp(value, this.BlinkOffsetBounds.Min, this.BlinkOffsetBounds.Max);
                value = single1;
                this.BlinkOffset = value;
            }
        }

        VRageMath.Color Sandbox.ModAPI.Ingame.IMyLightingBlock.Color
        {
            get => 
                this.Color;
            set => 
                (this.m_lightColor.Value = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyLightingBlock.<>c <>9 = new MyLightingBlock.<>c();
            public static MyTerminalValueControl<MyLightingBlock, Color>.GetterDelegate <>9__48_0;
            public static MyTerminalValueControl<MyLightingBlock, Color>.SetterDelegate <>9__48_1;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_2;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_3;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_4;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_5;
            public static MyTerminalValueControl<MyLightingBlock, float>.SetterDelegate <>9__48_6;
            public static MyTerminalControl<MyLightingBlock>.WriterDelegate <>9__48_7;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_8;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_9;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_10;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_11;
            public static MyTerminalValueControl<MyLightingBlock, float>.SetterDelegate <>9__48_12;
            public static MyTerminalControl<MyLightingBlock>.WriterDelegate <>9__48_13;
            public static Func<MyLightingBlock, bool> <>9__48_14;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_15;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_16;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_17;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_18;
            public static MyTerminalValueControl<MyLightingBlock, float>.SetterDelegate <>9__48_19;
            public static MyTerminalControl<MyLightingBlock>.WriterDelegate <>9__48_20;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_21;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_22;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_23;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_24;
            public static MyTerminalValueControl<MyLightingBlock, float>.SetterDelegate <>9__48_25;
            public static MyTerminalControl<MyLightingBlock>.WriterDelegate <>9__48_26;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_27;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_28;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_29;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_30;
            public static MyTerminalValueControl<MyLightingBlock, float>.SetterDelegate <>9__48_31;
            public static MyTerminalControl<MyLightingBlock>.WriterDelegate <>9__48_32;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_33;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_34;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_35;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_36;
            public static MyTerminalValueControl<MyLightingBlock, float>.SetterDelegate <>9__48_37;
            public static MyTerminalControl<MyLightingBlock>.WriterDelegate <>9__48_38;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_39;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_40;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_41;
            public static MyTerminalValueControl<MyLightingBlock, float>.GetterDelegate <>9__48_42;
            public static MyTerminalValueControl<MyLightingBlock, float>.SetterDelegate <>9__48_43;
            public static MyTerminalControl<MyLightingBlock>.WriterDelegate <>9__48_44;

            internal Color <CreateTerminalControls>b__48_0(MyLightingBlock x) => 
                x.Color;

            internal void <CreateTerminalControls>b__48_1(MyLightingBlock x, Color v)
            {
                x.m_lightColor.Value = v;
            }

            internal float <CreateTerminalControls>b__48_10(MyLightingBlock x) => 
                x.FalloffBounds.Default;

            internal float <CreateTerminalControls>b__48_11(MyLightingBlock x) => 
                x.Falloff;

            internal void <CreateTerminalControls>b__48_12(MyLightingBlock x, float v)
            {
                x.m_lightFalloff.Value = v;
            }

            internal void <CreateTerminalControls>b__48_13(MyLightingBlock x, StringBuilder result)
            {
                result.Append(MyValueFormatter.GetFormatedFloat(x.Falloff, 1));
            }

            internal bool <CreateTerminalControls>b__48_14(MyLightingBlock x) => 
                x.SupportsFalloff;

            internal float <CreateTerminalControls>b__48_15(MyLightingBlock x) => 
                x.IntensityBounds.Min;

            internal float <CreateTerminalControls>b__48_16(MyLightingBlock x) => 
                x.IntensityBounds.Max;

            internal float <CreateTerminalControls>b__48_17(MyLightingBlock x) => 
                x.IntensityBounds.Default;

            internal float <CreateTerminalControls>b__48_18(MyLightingBlock x) => 
                x.Intensity;

            internal void <CreateTerminalControls>b__48_19(MyLightingBlock x, float v)
            {
                x.Intensity = v;
            }

            internal float <CreateTerminalControls>b__48_2(MyLightingBlock x) => 
                (x.IsReflector ? x.ReflectorRadiusBounds.Min : x.RadiusBounds.Min);

            internal void <CreateTerminalControls>b__48_20(MyLightingBlock x, StringBuilder result)
            {
                result.Append(MyValueFormatter.GetFormatedFloat(x.Intensity, 1));
            }

            internal float <CreateTerminalControls>b__48_21(MyLightingBlock x) => 
                x.OffsetBounds.Min;

            internal float <CreateTerminalControls>b__48_22(MyLightingBlock x) => 
                x.OffsetBounds.Max;

            internal float <CreateTerminalControls>b__48_23(MyLightingBlock x) => 
                x.OffsetBounds.Default;

            internal float <CreateTerminalControls>b__48_24(MyLightingBlock x) => 
                x.Offset;

            internal void <CreateTerminalControls>b__48_25(MyLightingBlock x, float v)
            {
                x.m_lightOffset.Value = v;
            }

            internal void <CreateTerminalControls>b__48_26(MyLightingBlock x, StringBuilder result)
            {
                result.Append(MyValueFormatter.GetFormatedFloat(x.Offset, 1));
            }

            internal float <CreateTerminalControls>b__48_27(MyLightingBlock x) => 
                x.BlinkIntervalSecondsBounds.Min;

            internal float <CreateTerminalControls>b__48_28(MyLightingBlock x) => 
                x.BlinkIntervalSecondsBounds.Max;

            internal float <CreateTerminalControls>b__48_29(MyLightingBlock x) => 
                x.BlinkIntervalSecondsBounds.Default;

            internal float <CreateTerminalControls>b__48_3(MyLightingBlock x) => 
                (x.IsReflector ? x.ReflectorRadiusBounds.Max : x.RadiusBounds.Max);

            internal float <CreateTerminalControls>b__48_30(MyLightingBlock x) => 
                x.BlinkIntervalSeconds;

            internal void <CreateTerminalControls>b__48_31(MyLightingBlock x, float v)
            {
                x.BlinkIntervalSeconds = v;
            }

            internal void <CreateTerminalControls>b__48_32(MyLightingBlock x, StringBuilder result)
            {
                result.Append(MyValueFormatter.GetFormatedFloat(x.BlinkIntervalSeconds, 1)).Append(" s");
            }

            internal float <CreateTerminalControls>b__48_33(MyLightingBlock x) => 
                x.BlinkLenghtBounds.Min;

            internal float <CreateTerminalControls>b__48_34(MyLightingBlock x) => 
                x.BlinkLenghtBounds.Max;

            internal float <CreateTerminalControls>b__48_35(MyLightingBlock x) => 
                x.BlinkLenghtBounds.Default;

            internal float <CreateTerminalControls>b__48_36(MyLightingBlock x) => 
                x.BlinkLength;

            internal void <CreateTerminalControls>b__48_37(MyLightingBlock x, float v)
            {
                x.BlinkLength = v;
            }

            internal void <CreateTerminalControls>b__48_38(MyLightingBlock x, StringBuilder result)
            {
                result.Append(MyValueFormatter.GetFormatedFloat(x.BlinkLength, 1)).Append(" %");
            }

            internal float <CreateTerminalControls>b__48_39(MyLightingBlock x) => 
                x.BlinkOffsetBounds.Min;

            internal float <CreateTerminalControls>b__48_4(MyLightingBlock x) => 
                (x.IsReflector ? x.ReflectorRadiusBounds.Default : x.RadiusBounds.Default);

            internal float <CreateTerminalControls>b__48_40(MyLightingBlock x) => 
                x.BlinkOffsetBounds.Max;

            internal float <CreateTerminalControls>b__48_41(MyLightingBlock x) => 
                x.BlinkOffsetBounds.Default;

            internal float <CreateTerminalControls>b__48_42(MyLightingBlock x) => 
                x.BlinkOffset;

            internal void <CreateTerminalControls>b__48_43(MyLightingBlock x, float v)
            {
                x.BlinkOffset = v;
            }

            internal void <CreateTerminalControls>b__48_44(MyLightingBlock x, StringBuilder result)
            {
                result.Append(MyValueFormatter.GetFormatedFloat(x.BlinkOffset, 1)).Append(" %");
            }

            internal float <CreateTerminalControls>b__48_5(MyLightingBlock x) => 
                (x.IsReflector ? x.ReflectorRadius : x.Radius);

            internal void <CreateTerminalControls>b__48_6(MyLightingBlock x, float v)
            {
                x.m_lightRadius.Value = v;
            }

            internal void <CreateTerminalControls>b__48_7(MyLightingBlock x, StringBuilder result)
            {
                result.Append(MyValueFormatter.GetFormatedFloat(x.IsReflector ? x.m_reflectorRadius : x.m_radius, 1)).Append(" m");
            }

            internal float <CreateTerminalControls>b__48_8(MyLightingBlock x) => 
                x.FalloffBounds.Min;

            internal float <CreateTerminalControls>b__48_9(MyLightingBlock x) => 
                x.FalloffBounds.Max;
        }
    }
}

