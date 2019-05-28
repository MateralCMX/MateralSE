namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.EntityComponents.DebugRenders;
    using SpaceEngineers.Game.EntityComponents.GameLogic;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Graphics;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_SolarPanel)), MyTerminalInterface(new Type[] { typeof(SpaceEngineers.Game.ModAPI.IMySolarPanel), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMySolarPanel) })]
    public class MySolarPanel : MyEnvironmentalPowerProducer, SpaceEngineers.Game.ModAPI.IMySolarPanel, Sandbox.ModAPI.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyPowerProducer, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, IMyCubeBlock, IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMySolarPanel
    {
        private static readonly string[] m_emissiveTextureNames = new string[] { "Emissive0", "Emissive1", "Emissive2", "Emissive3" };

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            this.SolarPanelDefinition = (MySolarPanelDefinition) base.BlockDefinition;
            base.GameLogic = this.SolarComponent = new MySolarGameLogicComponent();
            this.SolarComponent.OnProductionChanged += new Action(this.OnProductionChanged);
            this.SolarComponent.Initialize(this.SolarPanelDefinition.PanelOrientation, this.SolarPanelDefinition.IsTwoSided, this.SolarPanelDefinition.PanelOffset, this);
            base.Init(objectBuilder, cubeGrid);
            base.AddDebugRenderComponent(new MyDebugRenderComponentSolarPanel(this));
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            this.UpdateEmissivity();
        }

        protected override void OnProductionChanged()
        {
            base.OnProductionChanged();
            this.UpdateEmissivity();
        }

        public override void SetDamageEffect(bool show)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                base.SetDamageEffect(show);
                if ((base.m_soundEmitter != null) && (((base.BlockDefinition.DamagedSound != null) && !show) && ((base.m_soundEmitter.SoundId == base.BlockDefinition.DamagedSound.Arcade) || (base.m_soundEmitter.SoundId != base.BlockDefinition.DamagedSound.Realistic))))
                {
                    base.m_soundEmitter.StopSound(false, true);
                }
            }
        }

        public override bool SetEmissiveStateDamaged() => 
            false;

        public override bool SetEmissiveStateDisabled() => 
            false;

        public override bool SetEmissiveStateWorking() => 
            false;

        protected void UpdateEmissivity()
        {
            if (base.InScene)
            {
                MyEmissiveColorStateResult result;
                Color red = Color.Red;
                if (!base.IsFunctional)
                {
                    if (MyEmissiveColorPresets.LoadPresetState(base.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Damaged, out result))
                    {
                        red = result.EmissiveColor;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[i], red, 0f);
                    }
                }
                else if (!base.IsWorking)
                {
                    if (MyEmissiveColorPresets.LoadPresetState(base.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Disabled, out result))
                    {
                        red = result.EmissiveColor;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[i], red, 1f);
                    }
                }
                else if (base.SourceComp.MaxOutput <= 0f)
                {
                    if (MyEmissiveColorPresets.LoadPresetState(base.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Warning, out result))
                    {
                        red = result.EmissiveColor;
                    }
                    UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[0], red, 1f);
                    for (int i = 1; i < 4; i++)
                    {
                        UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[i], red, 1f);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (i < ((base.SourceComp.MaxOutput / base.BlockDefinition.MaxPowerOutput) * 4f))
                        {
                            red = Color.Green;
                            if (MyEmissiveColorPresets.LoadPresetState(base.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Working, out result))
                            {
                                red = result.EmissiveColor;
                            }
                            UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[i], red, 1f);
                        }
                        else
                        {
                            red = Color.Black;
                            if (MyEmissiveColorPresets.LoadPresetState(base.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Damaged, out result))
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
            this.UpdateEmissivity();
        }

        public MySolarPanelDefinition SolarPanelDefinition { get; private set; }

        public MySolarGameLogicComponent SolarComponent { get; private set; }

        protected override float CurrentProductionRatio =>
            this.SolarComponent.MaxOutput;
    }
}

