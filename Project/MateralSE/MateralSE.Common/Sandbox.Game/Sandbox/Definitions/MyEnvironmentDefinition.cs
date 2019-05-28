namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    [MyDefinitionType(typeof(MyObjectBuilder_EnvironmentDefinition), typeof(MyEnvironmentDefinition.Postprocessor))]
    public class MyEnvironmentDefinition : MyDefinitionBase
    {
        public MyPlanetProperties PlanetProperties = MyPlanetProperties.Default;
        public MyFogProperties FogProperties = MyFogProperties.Default;
        public MySunProperties SunProperties = MySunProperties.Default;
        public MyPostprocessSettings PostProcessSettings = MyPostprocessSettings.Default;
        public MySSAOSettings SSAOSettings = MySSAOSettings.Default;
        public MyHBAOData HBAOSettings = MyHBAOData.Default;
        public float LargeShipMaxSpeed = 100f;
        public float SmallShipMaxSpeed = 100f;
        public Color ContourHighlightColor = MyObjectBuilder_EnvironmentDefinition.Defaults.ContourHighlightColor;
        public Color ContourHighlightColorAccessDenied = MyObjectBuilder_EnvironmentDefinition.Defaults.ContourHighlightColorAccessDenied;
        public float ContourHighlightThickness = 5f;
        public float HighlightPulseInSeconds;
        public List<MyObjectBuilder_EnvironmentDefinition.EnvironmentalParticleSettings> EnvironmentalParticles = new List<MyObjectBuilder_EnvironmentDefinition.EnvironmentalParticleSettings>();
        private float m_largeShipMaxAngularSpeed = 18000f;
        private float m_smallShipMaxAngularSpeed = 36000f;
        private float m_largeShipMaxAngularSpeedInRadians = MathHelper.ToRadians((float) 18000f);
        private float m_smallShipMaxAngularSpeedInRadians = MathHelper.ToRadians((float) 36000f);
        public string EnvironmentTexture = @"Textures\BackgroundCube\Final\BackgroundCube.dds";
        public MyOrientation EnvironmentOrientation = MyObjectBuilder_EnvironmentDefinition.Defaults.EnvironmentOrientation;

        public MyEnvironmentDefinition()
        {
            this.ShadowSettings = new MyShadowsSettings();
            this.LowLoddingSettings = new MyNewLoddingSettings();
            this.MediumLoddingSettings = new MyNewLoddingSettings();
            this.HighLoddingSettings = new MyNewLoddingSettings();
            this.ExtremeLoddingSettings = new MyNewLoddingSettings();
        }

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_EnvironmentDefinition definition1 = new MyObjectBuilder_EnvironmentDefinition();
            definition1.Id = (SerializableDefinitionId) base.Id;
            definition1.FogProperties = this.FogProperties;
            definition1.SunProperties = this.SunProperties;
            definition1.PostProcessSettings = this.PostProcessSettings;
            definition1.SSAOSettings = this.SSAOSettings;
            definition1.HBAOSettings = this.HBAOSettings;
            definition1.ShadowSettings.CopyFrom(this.ShadowSettings);
            definition1.LowLoddingSettings.CopyFrom(this.LowLoddingSettings);
            definition1.MediumLoddingSettings.CopyFrom(this.MediumLoddingSettings);
            definition1.HighLoddingSettings.CopyFrom(this.HighLoddingSettings);
            definition1.ExtremeLoddingSettings.CopyFrom(this.ExtremeLoddingSettings);
            definition1.SmallShipMaxSpeed = this.SmallShipMaxSpeed;
            definition1.LargeShipMaxSpeed = this.LargeShipMaxSpeed;
            definition1.SmallShipMaxAngularSpeed = this.SmallShipMaxAngularSpeed;
            definition1.LargeShipMaxAngularSpeed = this.LargeShipMaxAngularSpeed;
            definition1.ContourHighlightColor = this.ContourHighlightColor.ToVector4();
            definition1.ContourHighlightThickness = this.ContourHighlightThickness;
            definition1.HighlightPulseInSeconds = this.HighlightPulseInSeconds;
            definition1.EnvironmentTexture = this.EnvironmentTexture;
            definition1.EnvironmentOrientation = this.EnvironmentOrientation;
            definition1.EnvironmentalParticles = this.EnvironmentalParticles;
            return definition1;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_EnvironmentDefinition definition = (MyObjectBuilder_EnvironmentDefinition) builder;
            this.FogProperties = definition.FogProperties;
            this.PlanetProperties = definition.PlanetProperties;
            this.SunProperties = definition.SunProperties;
            this.PostProcessSettings = definition.PostProcessSettings;
            this.SSAOSettings = definition.SSAOSettings;
            this.HBAOSettings = definition.HBAOSettings;
            this.ShadowSettings.CopyFrom(definition.ShadowSettings);
            this.LowLoddingSettings.CopyFrom(definition.LowLoddingSettings);
            this.MediumLoddingSettings.CopyFrom(definition.MediumLoddingSettings);
            this.HighLoddingSettings.CopyFrom(definition.HighLoddingSettings);
            this.ExtremeLoddingSettings.CopyFrom(definition.ExtremeLoddingSettings);
            this.SmallShipMaxSpeed = definition.SmallShipMaxSpeed;
            this.LargeShipMaxSpeed = definition.LargeShipMaxSpeed;
            this.SmallShipMaxAngularSpeed = definition.SmallShipMaxAngularSpeed;
            this.LargeShipMaxAngularSpeed = definition.LargeShipMaxAngularSpeed;
            this.ContourHighlightColor = new Color(definition.ContourHighlightColor);
            this.ContourHighlightThickness = definition.ContourHighlightThickness;
            this.HighlightPulseInSeconds = definition.HighlightPulseInSeconds;
            this.EnvironmentTexture = definition.EnvironmentTexture;
            this.EnvironmentOrientation = definition.EnvironmentOrientation;
            this.EnvironmentalParticles = definition.EnvironmentalParticles;
        }

        public void Merge(MyEnvironmentDefinition src)
        {
            MyEnvironmentDefinition other = new MyEnvironmentDefinition {
                Id = src.Id,
                DisplayNameEnum = src.DisplayNameEnum,
                DescriptionEnum = src.DescriptionEnum,
                DisplayNameString = src.DisplayNameString,
                DescriptionString = src.DescriptionString,
                Icons = src.Icons,
                Enabled = src.Enabled,
                Public = src.Public,
                AvailableInSurvival = src.AvailableInSurvival,
                Context = src.Context
            };
            MyMergeHelper.Merge<MyEnvironmentDefinition>(this, src, other);
        }

        public MyShadowsSettings ShadowSettings { get; private set; }

        public MyNewLoddingSettings LowLoddingSettings { get; private set; }

        public MyNewLoddingSettings MediumLoddingSettings { get; private set; }

        public MyNewLoddingSettings HighLoddingSettings { get; private set; }

        public MyNewLoddingSettings ExtremeLoddingSettings { get; private set; }

        public float LargeShipMaxAngularSpeed
        {
            get => 
                this.m_largeShipMaxAngularSpeed;
            private set
            {
                this.m_largeShipMaxAngularSpeed = value;
                this.m_largeShipMaxAngularSpeedInRadians = MathHelper.ToRadians(this.m_largeShipMaxAngularSpeed);
            }
        }

        public float SmallShipMaxAngularSpeed
        {
            get => 
                this.m_smallShipMaxAngularSpeed;
            private set
            {
                this.m_smallShipMaxAngularSpeed = value;
                this.m_smallShipMaxAngularSpeedInRadians = MathHelper.ToRadians(this.m_smallShipMaxAngularSpeed);
            }
        }

        public float LargeShipMaxAngularSpeedInRadians =>
            this.m_largeShipMaxAngularSpeedInRadians;

        public float SmallShipMaxAngularSpeedInRadians =>
            this.m_smallShipMaxAngularSpeedInRadians;

        private class Postprocessor : MyDefinitionPostprocessor
        {
            public override void AfterLoaded(ref MyDefinitionPostprocessor.Bundle definitions)
            {
            }

            public override void AfterPostprocess(MyDefinitionSet set, Dictionary<MyStringHash, MyDefinitionBase> definitions)
            {
            }

            public override void OverrideBy(ref MyDefinitionPostprocessor.Bundle currentDefinitions, ref MyDefinitionPostprocessor.Bundle overrideBySet)
            {
                foreach (KeyValuePair<MyStringHash, MyDefinitionBase> pair in overrideBySet.Definitions)
                {
                    MyDefinitionBase base2;
                    if (!pair.Value.Enabled)
                    {
                        currentDefinitions.Definitions.Remove(pair.Key);
                        continue;
                    }
                    if (currentDefinitions.Definitions.TryGetValue(pair.Key, out base2))
                    {
                        ((MyEnvironmentDefinition) base2).Merge((MyEnvironmentDefinition) pair.Value);
                        continue;
                    }
                    currentDefinitions.Definitions.Add(pair.Key, pair.Value);
                }
            }
        }
    }
}

