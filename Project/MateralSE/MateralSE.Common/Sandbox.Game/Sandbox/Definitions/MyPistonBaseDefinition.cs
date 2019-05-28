namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_PistonBaseDefinition), (Type) null)]
    public class MyPistonBaseDefinition : MyMechanicalConnectionBlockBaseDefinition
    {
        public float Minimum;
        public float Maximum;
        public float MaxVelocity;
        public MyStringHash ResourceSinkGroup;
        public float RequiredPowerInput;
        public float MaxImpulse;
        public float DefaultMaxImpulseAxis;
        public float DefaultMaxImpulseNonAxis;
        public float UnsafeImpulseThreshold;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_PistonBaseDefinition definition = (MyObjectBuilder_PistonBaseDefinition) builder;
            this.Minimum = definition.Minimum;
            this.Maximum = definition.Maximum;
            this.MaxVelocity = definition.MaxVelocity;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.RequiredPowerInput = definition.RequiredPowerInput;
            this.MaxImpulse = definition.MaxImpulse;
            this.DefaultMaxImpulseAxis = definition.DefaultMaxImpulseAxis;
            this.DefaultMaxImpulseNonAxis = definition.DefaultMaxImpulseNonAxis;
            this.UnsafeImpulseThreshold = definition.DangerousImpulseThreshold;
        }
    }
}

