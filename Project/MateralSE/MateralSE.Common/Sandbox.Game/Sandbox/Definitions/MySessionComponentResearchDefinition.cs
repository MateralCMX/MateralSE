namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Components.Session;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_SessionComponentResearchDefinition), (Type) null)]
    public class MySessionComponentResearchDefinition : MySessionComponentDefinition
    {
        public bool WhitelistMode;
        public List<MyDefinitionId> Researches = new List<MyDefinitionId>();

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_SessionComponentResearchDefinition definition = builder as MyObjectBuilder_SessionComponentResearchDefinition;
            this.WhitelistMode = definition.WhitelistMode;
            if (definition.Researches != null)
            {
                foreach (SerializableDefinitionId id in definition.Researches)
                {
                    this.Researches.Add(id);
                }
            }
        }
    }
}

