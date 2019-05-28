namespace VRage.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;
    using VRage.Game.Definitions;
    using VRageRender;

    [MyDefinitionType(typeof(MyObjectBuilder_VisualSettingsDefinition), (Type) null)]
    public class MyVisualSettingsDefinition : MyDefinitionBase
    {
        public MyFogProperties FogProperties = MyFogProperties.Default;
        public MySunProperties SunProperties = MySunProperties.Default;
        public MyPostprocessSettings PostProcessSettings = MyPostprocessSettings.Default;

        public MyVisualSettingsDefinition()
        {
            this.ShadowSettings = new MyShadowsSettings();
        }

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_VisualSettingsDefinition definition1 = new MyObjectBuilder_VisualSettingsDefinition();
            definition1.FogProperties = this.FogProperties;
            definition1.SunProperties = this.SunProperties;
            definition1.PostProcessSettings = this.PostProcessSettings;
            definition1.ShadowSettings.CopyFrom(this.ShadowSettings);
            return definition1;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_VisualSettingsDefinition definition = (MyObjectBuilder_VisualSettingsDefinition) builder;
            this.FogProperties = definition.FogProperties;
            this.SunProperties = definition.SunProperties;
            this.PostProcessSettings = definition.PostProcessSettings;
            this.ShadowSettings.CopyFrom(definition.ShadowSettings);
        }

        [XmlIgnore]
        public MyShadowsSettings ShadowSettings { get; private set; }
    }
}

