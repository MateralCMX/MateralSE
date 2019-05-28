namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Lights;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Lights;

    [MyCubeBlockType(typeof(MyObjectBuilder_ReflectorLight)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyReflectorLight), typeof(Sandbox.ModAPI.Ingame.IMyReflectorLight) })]
    public class MyReflectorLight : MyLightingBlock, Sandbox.ModAPI.IMyReflectorLight, Sandbox.ModAPI.IMyLightingBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyLightingBlock, Sandbox.ModAPI.Ingame.IMyReflectorLight
    {
        private MyFlareDefinition m_flare;
        private static readonly Color COLOR_OFF = new Color(30, 30, 30);
        private bool m_wasWorking = true;

        public MyReflectorLight()
        {
            base.Render = new MyRenderComponentReflectorLight();
        }

        protected override void InitLight(MyLight light, Vector4 color, float radius, float falloff)
        {
            light.Start(color, base.CubeGrid.GridScale * radius, this.DisplayNameText);
            base.m_light.ReflectorOn = true;
            base.m_light.LightType = MyLightType.SPOTLIGHT;
            base.m_light.LightType = MyLightType.SPOTLIGHT;
            light.ReflectorTexture = this.BlockDefinition.ReflectorTexture;
            light.Falloff = 0.3f;
            light.GlossFactor = 0f;
            light.ReflectorGlossFactor = 1f;
            light.ReflectorFalloff = 0.5f;
            light.GlareOn = light.LightOn;
            light.GlareQuerySize = this.GlareQuerySizeDef;
            light.GlareType = MyGlareTypeEnum.Directional;
            MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_FlareDefinition), this.BlockDefinition.Flare);
            MyFlareDefinition definition = MyDefinitionManager.Static.GetDefinition(id) as MyFlareDefinition;
            this.m_flare = definition ?? new MyFlareDefinition();
            light.GlareSize = this.m_flare.Size;
            light.SubGlares = this.m_flare.SubGlares;
            this.UpdateIntensity();
            base.Render.NeedsDrawFromParent = true;
        }

        protected override void UpdateEmissivity(bool force = false)
        {
            if ((base.m_light != null) && ((this.m_wasWorking != (base.IsWorking && base.m_light.ReflectorOn)) || force))
            {
                this.m_wasWorking = base.IsWorking && base.m_light.ReflectorOn;
                if (this.m_wasWorking)
                {
                    UpdateEmissiveParts(base.Render.RenderObjectIDs[0], 1f, base.Color, Color.White);
                }
                else
                {
                    UpdateEmissiveParts(base.Render.RenderObjectIDs[0], 0f, COLOR_OFF, Color.White);
                }
            }
        }

        protected override void UpdateEnabled(bool state)
        {
            if (base.m_light != null)
            {
                bool flag = state && ReferenceEquals(base.CubeGrid.Projector, null);
                base.m_light.ReflectorOn = flag;
                base.m_light.LightOn = flag;
                base.m_light.GlareOn = flag;
            }
        }

        protected override void UpdateIntensity()
        {
            if (base.m_light != null)
            {
                float num = base.CurrentLightPower * base.Intensity;
                base.m_light.ReflectorIntensity = num * 8f;
                base.m_light.Intensity = num * 0.3f;
                float intensity = this.m_flare.Intensity * num;
                if (intensity < this.m_flare.Intensity)
                {
                    intensity = this.m_flare.Intensity;
                }
                base.m_light.GlareIntensity = intensity;
                float num3 = ((num / base.IntensityBounds.Max) / 2f) + 0.5f;
                base.m_light.GlareSize = this.m_flare.Size * num3;
                base.BulbColor = base.ComputeBulbColor();
            }
        }

        protected override void UpdateRadius(float value)
        {
            base.UpdateRadius(value);
            base.Radius = 10f * (base.ReflectorRadius / base.ReflectorRadiusBounds.Max);
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.UpdateEmissivity(true);
        }

        private float GlareQuerySizeDef =>
            (base.CubeGrid.GridScale * (base.IsLargeLight ? 0.5f : 0.1f));

        public override bool IsReflector =>
            true;

        public bool IsReflectorEnabled =>
            base.m_light.ReflectorOn;

        protected override bool SupportsFalloff =>
            false;

        public string ReflectorConeMaterial =>
            this.BlockDefinition.ReflectorConeMaterial;

        public MyReflectorBlockDefinition BlockDefinition
        {
            get
            {
                if (!(base.BlockDefinition is MyReflectorBlockDefinition))
                {
                    base.SlimBlock.BlockDefinition = new MyReflectorBlockDefinition();
                }
                return (MyReflectorBlockDefinition) base.BlockDefinition;
            }
        }
    }
}

