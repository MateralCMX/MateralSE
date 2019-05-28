namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_LaserAntennaDefinition), (Type) null)]
    public class MyLaserAntennaDefinition : MyCubeBlockDefinition
    {
        public MyStringHash ResourceSinkGroup;
        public float PowerInputIdle;
        public float PowerInputTurning;
        public float PowerInputLasing;
        public float RotationRate;
        public float MaxRange;
        public bool RequireLineOfSight;
        public int MinElevationDegrees;
        public int MaxElevationDegrees;
        public int MinAzimuthDegrees;
        public int MaxAzimuthDegrees;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_LaserAntennaDefinition definition = (MyObjectBuilder_LaserAntennaDefinition) builder;
            this.ResourceSinkGroup = MyStringHash.GetOrCompute(definition.ResourceSinkGroup);
            this.PowerInputIdle = definition.PowerInputIdle;
            this.PowerInputTurning = definition.PowerInputTurning;
            this.PowerInputLasing = definition.PowerInputLasing;
            this.RotationRate = definition.RotationRate;
            this.MaxRange = definition.MaxRange;
            this.RequireLineOfSight = definition.RequireLineOfSight;
            this.MinElevationDegrees = definition.MinElevationDegrees;
            this.MaxElevationDegrees = definition.MaxElevationDegrees;
            this.MinAzimuthDegrees = definition.MinAzimuthDegrees;
            this.MaxAzimuthDegrees = definition.MaxAzimuthDegrees;
        }
    }
}

