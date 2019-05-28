namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_RemoteControlDefinition), (Type) null)]
    public class MyRemoteControlDefinition : MyShipControllerDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float RequiredPowerInput;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_RemoteControlDefinition definition = builder as MyObjectBuilder_RemoteControlDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.RequiredPowerInput = definition.RequiredPowerInput;
        }
    }
}

