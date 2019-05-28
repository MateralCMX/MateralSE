namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_ThrustDefinition), (Type) null)]
    public class MyThrustDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public MyStringHash ThrusterType;
        public MyFuelConverterInfo FuelConverter;
        public float SlowdownFactor;
        public float ForceMagnitude;
        public float MaxPowerConsumption;
        public float MinPowerConsumption;
        public float FlameDamageLengthScale;
        public float FlameDamage;
        public float FlameLengthScale;
        public Vector4 FlameFullColor;
        public Vector4 FlameIdleColor;
        public string FlamePointMaterial;
        public string FlameLengthMaterial;
        public string FlameFlare;
        public float FlameVisibilityDistance;
        public float FlameGlareQuerySize;
        public float MinPlanetaryInfluence;
        public float MaxPlanetaryInfluence;
        public float InvDiffMinMaxPlanetaryInfluence;
        public float EffectivenessAtMaxInfluence;
        public float EffectivenessAtMinInfluence;
        public bool NeedsAtmosphereForInfluence;
        public float ConsumptionFactorPerG;
        public bool PropellerUse;
        public string PropellerEntity;
        public float PropellerFullSpeed;
        public float PropellerIdleSpeed;
        public float PropellerAcceleration;
        public float PropellerDeceleration;
        public float PropellerMaxDistance;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ThrustDefinition definition = builder as MyObjectBuilder_ThrustDefinition;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.FuelConverter = definition.FuelConverter;
            this.SlowdownFactor = definition.SlowdownFactor;
            this.ForceMagnitude = definition.ForceMagnitude;
            this.ThrusterType = MyStringHash.GetOrCompute(definition.ThrusterType);
            this.MaxPowerConsumption = definition.MaxPowerConsumption;
            this.MinPowerConsumption = definition.MinPowerConsumption;
            this.FlameDamageLengthScale = definition.FlameDamageLengthScale;
            this.FlameDamage = definition.FlameDamage;
            this.FlameLengthScale = definition.FlameLengthScale;
            this.FlameFullColor = definition.FlameFullColor;
            this.FlameIdleColor = definition.FlameIdleColor;
            this.FlamePointMaterial = definition.FlamePointMaterial;
            this.FlameLengthMaterial = definition.FlameLengthMaterial;
            this.FlameFlare = definition.FlameFlare;
            this.FlameVisibilityDistance = definition.FlameVisibilityDistance;
            this.FlameGlareQuerySize = definition.FlameGlareQuerySize;
            this.MinPlanetaryInfluence = definition.MinPlanetaryInfluence;
            this.MaxPlanetaryInfluence = definition.MaxPlanetaryInfluence;
            this.EffectivenessAtMinInfluence = definition.EffectivenessAtMinInfluence;
            this.EffectivenessAtMaxInfluence = definition.EffectivenessAtMaxInfluence;
            this.NeedsAtmosphereForInfluence = definition.NeedsAtmosphereForInfluence;
            this.ConsumptionFactorPerG = definition.ConsumptionFactorPerG;
            this.PropellerUse = definition.PropellerUsesPropellerSystem;
            this.PropellerEntity = definition.PropellerSubpartEntityName;
            this.PropellerFullSpeed = definition.PropellerRoundsPerSecondOnFullSpeed;
            this.PropellerIdleSpeed = definition.PropellerRoundsPerSecondOnIdleSpeed;
            this.PropellerAcceleration = definition.PropellerAccelerationTime;
            this.PropellerDeceleration = definition.PropellerDecelerationTime;
            this.PropellerMaxDistance = definition.PropellerMaxVisibleDistance;
            this.InvDiffMinMaxPlanetaryInfluence = 1f / (this.MaxPlanetaryInfluence - this.MinPlanetaryInfluence);
            if (!this.InvDiffMinMaxPlanetaryInfluence.IsValid())
            {
                this.InvDiffMinMaxPlanetaryInfluence = 0f;
            }
        }
    }
}

