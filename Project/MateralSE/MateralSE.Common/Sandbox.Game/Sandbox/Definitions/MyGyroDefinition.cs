namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_GyroDefinition), (Type) null)]
    public class MyGyroDefinition : MyCubeBlockDefinition
    {
        public string ResourceSinkGroup;
        public float ForceMagnitude;
        public float RequiredPowerInput;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GyroDefinition definition = (MyObjectBuilder_GyroDefinition) builder;
            this.ResourceSinkGroup = definition.ResourceSinkGroup;
            this.ForceMagnitude = definition.ForceMagnitude;
            this.RequiredPowerInput = definition.RequiredPowerInput;
        }
    }
}

