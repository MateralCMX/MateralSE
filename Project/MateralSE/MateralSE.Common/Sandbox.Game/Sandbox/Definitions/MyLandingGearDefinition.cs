namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_LandingGearDefinition), (Type) null)]
    public class MyLandingGearDefinition : MyCubeBlockDefinition
    {
        public string LockSound;
        public string UnlockSound;
        public string FailedAttachSound;
        public float MaxLockSeparatingVelocity;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_LandingGearDefinition definition = builder as MyObjectBuilder_LandingGearDefinition;
            this.LockSound = definition.LockSound;
            this.UnlockSound = definition.UnlockSound;
            this.FailedAttachSound = definition.FailedAttachSound;
            this.MaxLockSeparatingVelocity = definition.MaxLockSeparatingVelocity;
        }
    }
}

