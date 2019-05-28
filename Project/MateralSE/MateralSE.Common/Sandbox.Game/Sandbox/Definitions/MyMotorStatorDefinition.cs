namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_MotorStatorDefinition), (Type) null)]
    public class MyMotorStatorDefinition : MyMechanicalConnectionBlockBaseDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float RequiredPowerInput;
        public float MaxForceMagnitude;
        public float RotorDisplacementMin;
        public float RotorDisplacementMax;
        public float RotorDisplacementMinSmall;
        public float RotorDisplacementMaxSmall;
        public float RotorDisplacementInModel;
        public float UnsafeTorqueThreshold;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_MotorStatorDefinition definition = (MyObjectBuilder_MotorStatorDefinition) builder;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.RequiredPowerInput = definition.RequiredPowerInput;
            this.MaxForceMagnitude = definition.MaxForceMagnitude;
            this.RotorDisplacementMin = definition.RotorDisplacementMin;
            this.RotorDisplacementMax = definition.RotorDisplacementMax;
            this.RotorDisplacementMinSmall = definition.RotorDisplacementMinSmall;
            this.RotorDisplacementMaxSmall = definition.RotorDisplacementMaxSmall;
            this.RotorDisplacementInModel = definition.RotorDisplacementInModel;
            this.UnsafeTorqueThreshold = definition.DangerousTorqueThreshold;
        }
    }
}

