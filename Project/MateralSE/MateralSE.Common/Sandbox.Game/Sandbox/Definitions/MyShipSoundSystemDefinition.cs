namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ShipSoundSystemDefinition), (Type) null)]
    public class MyShipSoundSystemDefinition : MyDefinitionBase
    {
        public float MaxUpdateRange = 2000f;
        public float MaxUpdateRange_sq = 4000000f;
        public float WheelsCallbackRangeCreate_sq = 250000f;
        public float WheelsCallbackRangeRemove_sq = 562500f;
        public float FullSpeed = 96f;
        public float FullSpeed_sq = 9216f;
        public float SpeedThreshold1 = 32f;
        public float SpeedThreshold2 = 64f;
        public float LargeShipDetectionRadius = 15f;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ShipSoundSystemDefinition definition = builder as MyObjectBuilder_ShipSoundSystemDefinition;
            this.MaxUpdateRange = definition.MaxUpdateRange;
            this.FullSpeed = definition.FullSpeed;
            this.FullSpeed_sq = definition.FullSpeed * definition.FullSpeed;
            this.SpeedThreshold1 = definition.FullSpeed * 0.33f;
            this.SpeedThreshold2 = definition.FullSpeed * 0.66f;
            this.LargeShipDetectionRadius = definition.LargeShipDetectionRadius;
            this.MaxUpdateRange_sq = definition.MaxUpdateRange * definition.MaxUpdateRange;
            this.WheelsCallbackRangeCreate_sq = definition.WheelStartUpdateRange * definition.WheelStartUpdateRange;
            this.WheelsCallbackRangeRemove_sq = definition.WheelStopUpdateRange * definition.WheelStopUpdateRange;
        }
    }
}

