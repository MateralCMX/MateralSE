namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_EdgesDefinition), (Type) null)]
    public class MyEdgesDefinition : MyDefinitionBase
    {
        public MyEdgesModelSet Large;
        public MyEdgesModelSet Small;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_EdgesDefinition definition = builder as MyObjectBuilder_EdgesDefinition;
            this.Large = definition.Large;
            this.Small = definition.Small;
        }
    }
}

