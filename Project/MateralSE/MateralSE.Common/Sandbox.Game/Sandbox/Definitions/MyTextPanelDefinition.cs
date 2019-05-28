namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_TextPanelDefinition), (Type) null)]
    public class MyTextPanelDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float RequiredPowerInput;
        public int TextureResolution;
        public int ScreenWidth;
        public int ScreenHeight;
        public float MinFontSize;
        public float MaxFontSize;
        public float MaxChangingSpeed;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_TextPanelDefinition definition = (MyObjectBuilder_TextPanelDefinition) builder;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.RequiredPowerInput = definition.RequiredPowerInput;
            this.TextureResolution = definition.TextureResolution;
            this.ScreenWidth = definition.ScreenWidth;
            this.ScreenHeight = definition.ScreenHeight;
            this.MinFontSize = definition.MinFontSize;
            this.MaxFontSize = definition.MaxFontSize;
            this.MaxChangingSpeed = definition.MaxChangingSpeed;
        }
    }
}

