namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Game;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_InventoryComponentDefinition), (Type) null)]
    public class MyInventoryComponentDefinition : MyComponentDefinitionBase
    {
        public float Volume;
        public float Mass;
        public bool RemoveEntityOnEmpty;
        public bool MultiplierEnabled;
        public int MaxItemCount;
        public MyInventoryConstraint InputConstraint;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_InventoryComponentDefinition objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_InventoryComponentDefinition;
            objectBuilder.Volume = this.Volume;
            objectBuilder.Mass = this.Mass;
            objectBuilder.RemoveEntityOnEmpty = this.RemoveEntityOnEmpty;
            objectBuilder.MultiplierEnabled = this.MultiplierEnabled;
            objectBuilder.MaxItemCount = this.MaxItemCount;
            if (this.InputConstraint != null)
            {
                MyObjectBuilder_InventoryComponentDefinition.InventoryConstraintDefinition definition1 = new MyObjectBuilder_InventoryComponentDefinition.InventoryConstraintDefinition();
                definition1.IsWhitelist = this.InputConstraint.IsWhitelist;
                definition1.Icon = this.InputConstraint.Icon;
                definition1.Description = this.InputConstraint.Description;
                objectBuilder.InputConstraint = definition1;
                foreach (MyObjectBuilderType type in this.InputConstraint.ConstrainedTypes)
                {
                    objectBuilder.InputConstraint.Entries.Add((SerializableDefinitionId) new MyDefinitionId(type));
                }
                foreach (MyDefinitionId id in this.InputConstraint.ConstrainedIds)
                {
                    objectBuilder.InputConstraint.Entries.Add((SerializableDefinitionId) id);
                }
            }
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_InventoryComponentDefinition definition = builder as MyObjectBuilder_InventoryComponentDefinition;
            this.Volume = definition.Volume;
            if (definition.Size != null)
            {
                Vector3 vector = definition.Size.Value;
                this.Volume = vector.Volume;
            }
            this.Mass = definition.Mass;
            this.RemoveEntityOnEmpty = definition.RemoveEntityOnEmpty;
            this.MultiplierEnabled = definition.MultiplierEnabled;
            this.MaxItemCount = definition.MaxItemCount;
            if (definition.InputConstraint != null)
            {
                this.InputConstraint = new MyInventoryConstraint(MyStringId.GetOrCompute(definition.InputConstraint.Description), definition.InputConstraint.Icon, definition.InputConstraint.IsWhitelist);
                foreach (SerializableDefinitionId id in definition.InputConstraint.Entries)
                {
                    if (string.IsNullOrEmpty(id.SubtypeName))
                    {
                        this.InputConstraint.AddObjectBuilderType(id.TypeId);
                        continue;
                    }
                    this.InputConstraint.Add(id);
                }
            }
        }
    }
}

