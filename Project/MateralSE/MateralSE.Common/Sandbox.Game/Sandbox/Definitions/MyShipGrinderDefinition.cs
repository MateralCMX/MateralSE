namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ShipGrinderDefinition), (Type) null)]
    public class MyShipGrinderDefinition : MyShipToolDefinition
    {
        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ShipGrinderDefinition definition1 = (MyObjectBuilder_ShipGrinderDefinition) builder;
        }
    }
}

