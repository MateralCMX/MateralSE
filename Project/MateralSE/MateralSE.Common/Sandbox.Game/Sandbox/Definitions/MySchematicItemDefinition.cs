namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_SchematicItemDefinition), (Type) null)]
    public class MySchematicItemDefinition : MyUsableItemDefinition
    {
        public MyDefinitionId Research;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_SchematicItemDefinition definition = builder as MyObjectBuilder_SchematicItemDefinition;
            this.Research = definition.Research.Value;
        }
    }
}

