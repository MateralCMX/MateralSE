namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_EquivalencyGroupDefinition), (Type) null)]
    public class MyEquivalencyGroupDefinition : MyDefinitionBase
    {
        public MyDefinitionId MainElement;
        public bool ForceMainElement;
        public List<MyDefinitionId> Equivalents;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_EquivalencyGroupDefinition definition = builder as MyObjectBuilder_EquivalencyGroupDefinition;
            if (definition != null)
            {
                this.MainElement = definition.MainId;
                this.ForceMainElement = definition.ForceMainId;
                this.Equivalents = new List<MyDefinitionId>();
                foreach (SerializableDefinitionId id in definition.EquivalentId)
                {
                    this.Equivalents.Add(id);
                }
            }
        }
    }
}

