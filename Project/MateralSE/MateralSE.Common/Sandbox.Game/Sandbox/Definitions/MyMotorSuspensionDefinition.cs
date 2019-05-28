namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_MotorSuspensionDefinition), (Type) null)]
    public class MyMotorSuspensionDefinition : MyMotorStatorDefinition
    {
        public float MaxSteer;
        public float SteeringSpeed;
        public float PropulsionForce;
        public float MinHeight;
        public float MaxHeight;
        public float AxleFriction;
        public float AirShockMinSpeed;
        public float AirShockMaxSpeed;
        public int AirShockActivationDelay;
        public float RequiredIdlePowerInput;
        public MyDefinitionId? SoundDefinitionId;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            MyDefinitionId id;
            base.Init(builder);
            MyObjectBuilder_MotorSuspensionDefinition definition = (MyObjectBuilder_MotorSuspensionDefinition) builder;
            this.MaxSteer = definition.MaxSteer;
            this.SteeringSpeed = definition.SteeringSpeed;
            this.PropulsionForce = definition.PropulsionForce;
            this.MinHeight = definition.MinHeight;
            this.MaxHeight = definition.MaxHeight;
            this.AxleFriction = definition.AxleFriction;
            this.AirShockMinSpeed = definition.AirShockMinSpeed;
            this.AirShockMaxSpeed = definition.AirShockMaxSpeed;
            this.AirShockActivationDelay = definition.AirShockActivationDelay;
            this.RequiredIdlePowerInput = (definition.RequiredIdlePowerInput != 0f) ? definition.RequiredIdlePowerInput : definition.RequiredPowerInput;
            if ((definition.SoundDefinitionId == null) || !MyDefinitionId.TryParse(definition.SoundDefinitionId.DefinitionTypeName, definition.SoundDefinitionId.DefinitionSubtypeName, out id))
            {
                this.SoundDefinitionId = null;
            }
            else
            {
                this.SoundDefinitionId = new MyDefinitionId?(id);
            }
        }
    }
}

