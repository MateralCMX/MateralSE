namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_BeaconDefinition), (Type) null)]
    public class MyBeaconDefinition : MyCubeBlockDefinition
    {
        public string ResourceSinkGroup;
        public string Flare;
        public float MaxBroadcastRadius;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_BeaconDefinition definition = (MyObjectBuilder_BeaconDefinition) builder;
            this.ResourceSinkGroup = definition.ResourceSinkGroup;
            this.MaxBroadcastRadius = definition.MaxBroadcastRadius;
            this.Flare = definition.Flare;
        }
    }
}

