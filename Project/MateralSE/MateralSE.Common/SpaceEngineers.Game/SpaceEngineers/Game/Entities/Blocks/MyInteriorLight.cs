namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Lights;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Lights;

    [MyCubeBlockType(typeof(MyObjectBuilder_InteriorLight)), MyTerminalInterface(new Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyInteriorLight), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyInteriorLight) })]
    public class MyInteriorLight : MyLightingBlock, SpaceEngineers.Game.ModAPI.IMyInteriorLight, Sandbox.ModAPI.IMyLightingBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyLightingBlock, SpaceEngineers.Game.ModAPI.Ingame.IMyInteriorLight
    {
        private MyFlareDefinition m_flare;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
        }

        protected override void InitLight(MyLight light, Vector4 color, float radius, float falloff)
        {
            light.Start(color, radius, this.DisplayNameText);
            light.Falloff = falloff;
            light.GlareOn = light.LightOn;
            light.GlareIntensity = 0.4f;
            light.GlareQuerySize = 0.2f;
            light.GlareType = MyGlareTypeEnum.Normal;
            MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_FlareDefinition), base.BlockDefinition.Flare);
            MyFlareDefinition definition = MyDefinitionManager.Static.GetDefinition(id) as MyFlareDefinition;
            this.m_flare = definition ?? new MyFlareDefinition();
            light.GlareSize = this.m_flare.Size;
            light.SubGlares = this.m_flare.SubGlares;
        }

        protected override void UpdateEmissivity(bool force = false)
        {
            if (base.m_light != null)
            {
                base.UpdateEmissivity(force);
                UpdateEmissiveParts(base.Render.RenderObjectIDs[0], base.m_light.LightOn ? base.m_light.Intensity : 0f, Color.Lerp(base.Color, base.Color.ToGray(), 0.5f), Color.Black);
            }
        }

        protected override void UpdateEnabled(bool state)
        {
            if (base.m_light != null)
            {
                base.m_light.LightOn = state;
                base.m_light.GlareOn = state;
            }
        }

        protected override void UpdateIntensity()
        {
            if (base.m_light != null)
            {
                float num = base.CurrentLightPower * base.Intensity;
                base.m_light.Intensity = num * 2f;
                float intensity = this.m_flare.Intensity * num;
                if (intensity < this.m_flare.Intensity)
                {
                    intensity = this.m_flare.Intensity;
                }
                base.m_light.GlareIntensity = intensity;
                base.BulbColor = base.ComputeBulbColor();
            }
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.UpdateEmissivity(true);
        }

        public override bool IsReflector =>
            false;

        protected override bool SupportsFalloff =>
            true;
    }
}

