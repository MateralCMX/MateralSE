namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_AiCommandDefinition), (Type) null)]
    public class MyAiCommandDefinition : MyDefinitionBase
    {
        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
        }
    }
}

