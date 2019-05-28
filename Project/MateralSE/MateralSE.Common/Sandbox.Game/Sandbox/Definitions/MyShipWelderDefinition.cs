namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ShipWelderDefinition), (Type) null)]
    public class MyShipWelderDefinition : MyShipToolDefinition
    {
        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ShipWelderDefinition definition1 = (MyObjectBuilder_ShipWelderDefinition) builder;
        }
    }
}

