namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_ResearchDefinition), (Type) null)]
    public class MyResearchDefinition : MyDefinitionBase
    {
        public List<MyDefinitionId> Entries;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            this.Entries = new List<MyDefinitionId>();
            foreach (SerializableDefinitionId id in (builder as MyObjectBuilder_ResearchDefinition).Entries)
            {
                this.Entries.Add(id);
            }
        }
    }
}

