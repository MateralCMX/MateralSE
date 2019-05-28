namespace Sandbox.Game.EntityComponents
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.ComponentSystem;

    [MyDefinitionType(typeof(MyObjectBuilder_InventorySpawnComponent_Definition), (Type) null)]
    public class MyEntityInventorySpawnComponent_Definition : MyComponentDefinitionBase
    {
        public MyDefinitionId ContainerDefinition;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_InventorySpawnComponent_Definition definition = builder as MyObjectBuilder_InventorySpawnComponent_Definition;
            this.ContainerDefinition = definition.ContainerDefinition;
        }
    }
}

