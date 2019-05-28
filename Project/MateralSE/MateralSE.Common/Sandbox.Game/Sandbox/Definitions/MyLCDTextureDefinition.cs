namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_LCDTextureDefinition), (Type) null)]
    public class MyLCDTextureDefinition : MyDefinitionBase
    {
        public string TexturePath;
        public string SpritePath;
        public string LocalizationId;
        public bool Selectable;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_LCDTextureDefinition definition = builder as MyObjectBuilder_LCDTextureDefinition;
            if (definition != null)
            {
                this.TexturePath = definition.TexturePath;
                this.SpritePath = definition.SpritePath;
                this.LocalizationId = definition.LocalizationId;
                this.Selectable = definition.Selectable;
            }
        }
    }
}

