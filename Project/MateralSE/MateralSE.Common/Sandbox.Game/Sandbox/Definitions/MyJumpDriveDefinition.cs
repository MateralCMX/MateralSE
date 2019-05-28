namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_JumpDriveDefinition), (Type) null)]
    public class MyJumpDriveDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float RequiredPowerInput;
        public float PowerNeededForJump;
        public double MaxJumpDistance;
        public double MaxJumpMass;
        public float JumpDelay;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_JumpDriveDefinition definition = builder as MyObjectBuilder_JumpDriveDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.RequiredPowerInput = definition.RequiredPowerInput;
            this.PowerNeededForJump = definition.PowerNeededForJump;
            this.MaxJumpDistance = definition.MaxJumpDistance;
            this.MaxJumpMass = definition.MaxJumpMass;
            this.JumpDelay = definition.JumpDelay;
        }
    }
}

