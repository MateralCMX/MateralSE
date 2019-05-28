namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_ResearchGroupDefinition), (Type) null)]
    public class MyResearchGroupDefinition : MyDefinitionBase
    {
        public SerializableDefinitionId[] Members;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ResearchGroupDefinition definition = builder as MyObjectBuilder_ResearchGroupDefinition;
            if (definition.Members != null)
            {
                this.Members = new SerializableDefinitionId[definition.Members.Length];
                for (int i = 0; i < definition.Members.Length; i++)
                {
                    this.Members[i] = definition.Members[i];
                }
            }
        }
    }
}

