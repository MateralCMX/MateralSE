namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;

    public class MyLootBagDefinition
    {
        public MyDefinitionId ContainerDefinition;
        public float SearchRadius;

        public void Init(MyObjectBuilder_Configuration.LootBagDefinition objectBuilder)
        {
            this.ContainerDefinition = objectBuilder.ContainerDefinition;
            this.SearchRadius = objectBuilder.SearchRadius;
        }
    }
}

