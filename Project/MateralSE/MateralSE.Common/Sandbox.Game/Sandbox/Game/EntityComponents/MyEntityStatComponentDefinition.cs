namespace Sandbox.Game.EntityComponents
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_EntityStatComponentDefinition), (Type) null)]
    public class MyEntityStatComponentDefinition : MyComponentDefinitionBase
    {
        public List<MyDefinitionId> Stats;
        public List<string> Scripts;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_EntityStatComponentDefinition objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_EntityStatComponentDefinition;
            objectBuilder.Stats = new List<SerializableDefinitionId>();
            foreach (MyDefinitionId id in this.Stats)
            {
                objectBuilder.Stats.Add((SerializableDefinitionId) id);
            }
            objectBuilder.Scripts = this.Scripts;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_EntityStatComponentDefinition definition = builder as MyObjectBuilder_EntityStatComponentDefinition;
            this.Stats = new List<MyDefinitionId>();
            foreach (SerializableDefinitionId id in definition.Stats)
            {
                this.Stats.Add(id);
            }
            this.Scripts = definition.Scripts;
        }
    }
}

