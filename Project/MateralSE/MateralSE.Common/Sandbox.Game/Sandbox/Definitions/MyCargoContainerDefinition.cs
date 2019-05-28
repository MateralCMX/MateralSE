namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_CargoContainerDefinition), (Type) null)]
    public class MyCargoContainerDefinition : MyCubeBlockDefinition
    {
        public Vector3 InventorySize;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CargoContainerDefinition definition = builder as MyObjectBuilder_CargoContainerDefinition;
            this.InventorySize = definition.InventorySize;
        }
    }
}

