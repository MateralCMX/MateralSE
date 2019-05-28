namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_RadioAntennaDefinition), (Type) null)]
    public class MyRadioAntennaDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float MaxBroadcastRadius;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_RadioAntennaDefinition definition = (MyObjectBuilder_RadioAntennaDefinition) builder;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.MaxBroadcastRadius = definition.MaxBroadcastRadius;
        }
    }
}

