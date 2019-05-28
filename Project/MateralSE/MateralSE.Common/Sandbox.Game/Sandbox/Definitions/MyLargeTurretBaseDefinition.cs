namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_LargeTurretBaseDefinition), (Type) null)]
    public class MyLargeTurretBaseDefinition : MyWeaponBlockDefinition
    {
        public string OverlayTexture;
        public bool AiEnabled;
        public int MinElevationDegrees;
        public int MaxElevationDegrees;
        public int MinAzimuthDegrees;
        public int MaxAzimuthDegrees;
        public bool IdleRotation;
        public float MaxRangeMeters;
        public float RotationSpeed;
        public float ElevationSpeed;
        public float MinFov;
        public float MaxFov;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_LargeTurretBaseDefinition definition = builder as MyObjectBuilder_LargeTurretBaseDefinition;
            this.OverlayTexture = definition.OverlayTexture;
            this.AiEnabled = definition.AiEnabled;
            this.MinElevationDegrees = definition.MinElevationDegrees;
            this.MaxElevationDegrees = definition.MaxElevationDegrees;
            this.MinAzimuthDegrees = definition.MinAzimuthDegrees;
            this.MaxAzimuthDegrees = definition.MaxAzimuthDegrees;
            this.IdleRotation = definition.IdleRotation;
            this.MaxRangeMeters = definition.MaxRangeMeters;
            this.RotationSpeed = definition.RotationSpeed;
            this.ElevationSpeed = definition.ElevationSpeed;
            this.MinFov = definition.MinFov;
            this.MaxFov = definition.MaxFov;
        }
    }
}

