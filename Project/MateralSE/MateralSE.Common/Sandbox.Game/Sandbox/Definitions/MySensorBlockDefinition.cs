namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_SensorBlockDefinition), (Type) null)]
    public class MySensorBlockDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float RequiredPowerInput;
        public float MaxRange;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_SensorBlockDefinition definition = builder as MyObjectBuilder_SensorBlockDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.RequiredPowerInput = definition.RequiredPowerInput;
            this.MaxRange = Math.Max(definition.MaxRange, 1f);
        }
    }
}

