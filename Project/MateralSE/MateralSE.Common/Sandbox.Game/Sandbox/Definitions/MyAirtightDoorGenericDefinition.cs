namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_AirtightDoorGenericDefinition), (Type) null)]
    public class MyAirtightDoorGenericDefinition : MyCubeBlockDefinition
    {
        public string ResourceSinkGroup;
        public float PowerConsumptionIdle;
        public float PowerConsumptionMoving;
        public float OpeningSpeed;
        public string Sound;
        public string OpenSound;
        public string CloseSound;
        public float SubpartMovementDistance = 2.5f;

        protected override void Init(MyObjectBuilder_DefinitionBase builderBase)
        {
            base.Init(builderBase);
            MyObjectBuilder_AirtightDoorGenericDefinition definition = builderBase as MyObjectBuilder_AirtightDoorGenericDefinition;
            this.ResourceSinkGroup = definition.ResourceSinkGroup;
            this.PowerConsumptionIdle = definition.PowerConsumptionIdle;
            this.PowerConsumptionMoving = definition.PowerConsumptionMoving;
            this.OpeningSpeed = definition.OpeningSpeed;
            this.Sound = definition.Sound;
            this.OpenSound = definition.OpenSound;
            this.CloseSound = definition.CloseSound;
            this.SubpartMovementDistance = definition.SubpartMovementDistance;
        }
    }
}

