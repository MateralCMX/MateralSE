namespace VRage.Game.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.ComponentSystem;

    [MyDefinitionType(typeof(MyObjectBuilder_ModStorageComponentDefinition), (Type) null)]
    public class MyModStorageComponentDefinition : MyComponentDefinitionBase
    {
        public Guid[] RegisteredStorageGuids;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_ModStorageComponentDefinition objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_ModStorageComponentDefinition;
            objectBuilder.RegisteredStorageGuids = this.RegisteredStorageGuids;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ModStorageComponentDefinition definition = builder as MyObjectBuilder_ModStorageComponentDefinition;
            this.RegisteredStorageGuids = definition.RegisteredStorageGuids;
        }
    }
}

