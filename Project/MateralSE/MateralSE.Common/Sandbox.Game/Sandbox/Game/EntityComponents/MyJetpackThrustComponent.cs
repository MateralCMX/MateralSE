namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRageMath;

    internal class MyJetpackThrustComponent : MyEntityThrustComponent
    {
        protected override void AddToGroup(VRage.Game.Entity.MyEntity thrustEntity, MyEntityThrustComponent.MyConveyorConnectedGroup group)
        {
        }

        protected override Vector3 ApplyThrustModifiers(ref MyDefinitionId fuelType, ref Vector3 thrust, ref Vector3 thrustOverride, MyResourceSinkComponentBase resourceSink)
        {
            thrust += thrustOverride;
            if ((((this.Character.ControllerInfo.Controller == null) || !MySession.Static.CreativeToolsEnabled(this.Character.ControllerInfo.Controller.Player.Id.SteamId)) && !MySession.Static.CreativeToolsEnabled(this.Character.ControlSteamId)) || (!ReferenceEquals(MySession.Static.LocalCharacter, this.Character) && !Sync.IsServer))
            {
                thrust *= resourceSink.SuppliedRatioByType(fuelType);
            }
            thrust *= MyFakes.THRUST_FORCE_RATIO;
            return thrust;
        }

        protected override float CalculateConsumptionMultiplier(VRage.Game.Entity.MyEntity thrustEntity, float naturalGravityStrength) => 
            (1f + (this.Jetpack.ConsumptionFactorPerG * (naturalGravityStrength / 9.81f)));

        protected override float CalculateForceMultiplier(VRage.Game.Entity.MyEntity thrustEntity, float planetaryInfluence, bool inAtmosphere)
        {
            float effectivenessAtMinInfluence = 1f;
            if ((this.Jetpack.MaxPlanetaryInfluence != this.Jetpack.MinPlanetaryInfluence) && ((inAtmosphere && this.Jetpack.NeedsAtmosphereForInfluence) || !inAtmosphere))
            {
                return MathHelper.Lerp(this.Jetpack.EffectivenessAtMinInfluence, this.Jetpack.EffectivenessAtMaxInfluence, MathHelper.Clamp((float) ((planetaryInfluence - this.Jetpack.MinPlanetaryInfluence) / (this.Jetpack.MaxPlanetaryInfluence - this.Jetpack.MinPlanetaryInfluence)), (float) 0f, (float) 1f));
            }
            if (this.Jetpack.NeedsAtmosphereForInfluence && !inAtmosphere)
            {
                effectivenessAtMinInfluence = this.Jetpack.EffectivenessAtMinInfluence;
            }
            return effectivenessAtMinInfluence;
        }

        protected override float ForceMagnitude(VRage.Game.Entity.MyEntity thrustEntity, float planetaryInfluence, bool inAtmosphere) => 
            (this.Jetpack.ForceMagnitude * this.CalculateForceMultiplier(thrustEntity, planetaryInfluence, inAtmosphere));

        protected override MyDefinitionId FuelType(VRage.Game.Entity.MyEntity thrustEntity)
        {
            if ((this.Jetpack == null) || (this.Jetpack.FuelDefinition == null))
            {
                return MyResourceDistributorComponent.ElectricityId;
            }
            return this.Jetpack.FuelDefinition.Id;
        }

        protected override bool IsThrustEntityType(VRage.Game.Entity.MyEntity thrustEntity) => 
            (thrustEntity is MyCharacter);

        protected override bool IsUsed(VRage.Game.Entity.MyEntity thrustEntity) => 
            base.Enabled;

        protected override float MaxPowerConsumption(VRage.Game.Entity.MyEntity thrustEntity) => 
            this.Jetpack.MaxPowerConsumption;

        protected override float MinPowerConsumption(VRage.Game.Entity.MyEntity thrustEntity) => 
            this.Jetpack.MinPowerConsumption;

        protected override bool RecomputeOverriddenParameters(VRage.Game.Entity.MyEntity thrustEntity, MyEntityThrustComponent.FuelTypeData fuelData) => 
            false;

        public override void Register(VRage.Game.Entity.MyEntity entity, Vector3I forwardVector, Func<bool> onRegisteredCallback = null)
        {
            MyCharacter character = entity as MyCharacter;
            if (character != null)
            {
                base.Register(entity, forwardVector, onRegisteredCallback);
                MyDefinitionId resourceTypeId = this.FuelType(entity);
                float efficiency = 1f;
                if (MyFakes.ENABLE_HYDROGEN_FUEL)
                {
                    efficiency = this.Jetpack.FuelConverterDefinition.Efficiency;
                }
                base.m_lastFuelTypeData.Efficiency = efficiency;
                base.m_lastFuelTypeData.EnergyDensity = this.Jetpack.FuelDefinition.EnergyDensity;
                base.m_lastSink.SetMaxRequiredInputByType(resourceTypeId, base.m_lastSink.MaxRequiredInputByType(resourceTypeId) + base.PowerAmountToFuel(ref resourceTypeId, this.Jetpack.MaxPowerConsumption, base.m_lastGroup));
                base.SlowdownFactor = Math.Max(character.Definition.Jetpack.ThrustProperties.SlowdownFactor, base.SlowdownFactor);
            }
        }

        protected override void RemoveFromGroup(VRage.Game.Entity.MyEntity thrustEntity, MyEntityThrustComponent.MyConveyorConnectedGroup group)
        {
        }

        protected override void UpdateThrusts(bool enableDampers, Vector3 dampeningVelocity)
        {
            base.UpdateThrusts(enableDampers, dampeningVelocity);
            if (((this.Character != null) && ((this.Character.Physics != null) && (this.Character.Physics.CharacterProxy != null))) && this.Jetpack.TurnedOn)
            {
                if ((base.FinalThrust.LengthSquared() > 0.0001f) && this.Character.Physics.IsInWorld)
                {
                    Vector3D? position = null;
                    Vector3? torque = null;
                    float? maxSpeed = null;
                    this.Character.Physics.AddForce(MyPhysicsForceType.ADD_BODY_FORCE_AND_BODY_TORQUE, new Vector3?(base.FinalThrust), position, torque, maxSpeed, true, false);
                    Vector3 linearVelocityLocal = this.Character.Physics.LinearVelocityLocal;
                    float num = Math.Max(this.Character.Definition.MaxSprintSpeed, Math.Max(this.Character.Definition.MaxRunSpeed, this.Character.Definition.MaxBackrunSpeed));
                    float num2 = MyGridPhysics.ShipMaxLinearVelocity() + num;
                    if (linearVelocityLocal.LengthSquared() > (num2 * num2))
                    {
                        linearVelocityLocal.Normalize();
                        linearVelocityLocal *= num2;
                        this.Character.Physics.LinearVelocity = linearVelocityLocal;
                    }
                }
                if ((this.Character.Physics.Enabled && (this.Character.Physics.LinearVelocity != Vector3.Zero)) && (this.Character.Physics.LinearVelocity.LengthSquared() < 1E-06f))
                {
                    this.Character.Physics.LinearVelocity = Vector3.Zero;
                    base.ControlThrustChanged = true;
                }
            }
        }

        protected override void UpdateThrustStrength(HashSet<VRage.Game.Entity.MyEntity> entities, float thrustForce)
        {
            base.ControlThrust = Vector3.Zero;
        }

        public MyCharacter Entity =>
            (base.Entity as MyCharacter);

        public MyCharacter Character =>
            this.Entity;

        public MyCharacterJetpackComponent Jetpack =>
            this.Character.JetpackComp;
    }
}

