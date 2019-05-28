namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ReflectorBlockDefinition), (Type) null)]
    public class MyReflectorBlockDefinition : MyLightingBlockDefinition
    {
        public string ReflectorTexture;
        public string ReflectorConeMaterial;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ReflectorBlockDefinition definition = (MyObjectBuilder_ReflectorBlockDefinition) builder;
            this.ReflectorTexture = definition.ReflectorTexture;
            this.ReflectorConeMaterial = definition.ReflectorConeMaterial;
        }
    }
}

