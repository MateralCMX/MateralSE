namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_BatteryBlockDefinition), (Type) null)]
    public class MyBatteryBlockDefinition : MyPowerProducerDefinition
    {
        public float MaxStoredPower;
        public float InitialStoredPowerRatio;
        public MyStringHash ResourceSinkGroup;
        public float RequiredPowerInput;
        public bool AdaptibleInput;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_BatteryBlockDefinition definition = builder as MyObjectBuilder_BatteryBlockDefinition;
            if (definition != null)
            {
                this.MaxStoredPower = definition.MaxStoredPower;
                this.InitialStoredPowerRatio = definition.InitialStoredPowerRatio;
                this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
                this.RequiredPowerInput = definition.RequiredPowerInput;
                this.AdaptibleInput = definition.AdaptibleInput;
            }
        }
    }
}

