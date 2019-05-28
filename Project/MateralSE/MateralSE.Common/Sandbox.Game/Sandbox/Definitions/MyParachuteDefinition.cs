namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_ParachuteDefinition), (Type) null)]
    public class MyParachuteDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float PowerConsumptionIdle;
        public float PowerConsumptionMoving;
        public MyObjectBuilder_ParachuteDefinition.SubpartDefinition[] Subparts;
        public MyObjectBuilder_ParachuteDefinition.Opening[] OpeningSequence;
        public string ParachuteSubpartName;
        public float DragCoefficient;
        public int MaterialDeployCost;
        public MyDefinitionId MaterialDefinitionId;
        public float ReefAtmosphereLevel;
        public float MinimumAtmosphereLevel;
        public float RadiusMultiplier;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ParachuteDefinition definition = builder as MyObjectBuilder_ParachuteDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.PowerConsumptionIdle = definition.PowerConsumptionIdle;
            this.PowerConsumptionMoving = definition.PowerConsumptionMoving;
            this.Subparts = definition.Subparts;
            this.OpeningSequence = definition.OpeningSequence;
            this.ParachuteSubpartName = definition.ParachuteSubpartName;
            this.DragCoefficient = definition.DragCoefficient;
            this.MaterialDeployCost = definition.MaterialDeployCost;
            this.ReefAtmosphereLevel = definition.ReefAtmosphereLevel;
            this.MinimumAtmosphereLevel = definition.MinimumAtmosphereLevel;
            this.RadiusMultiplier = definition.RadiusMultiplier;
            this.MaterialDefinitionId = new MyDefinitionId(typeof(MyObjectBuilder_Component), definition.MaterialSubtype);
        }
    }
}

