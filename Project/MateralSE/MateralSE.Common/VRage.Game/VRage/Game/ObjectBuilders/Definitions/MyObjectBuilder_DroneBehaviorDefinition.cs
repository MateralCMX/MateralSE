namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_DroneBehaviorDefinition : MyObjectBuilder_DefinitionBase
    {
        public float StrafeWidth = 10f;
        public float StrafeHeight = 10f;
        public float StrafeDepth = 5f;
        public float MinStrafeDistance = 2f;
        public bool AvoidCollisions = true;
        public bool RotateToPlayer = true;
        public bool UseStaticWeaponry = true;
        public bool UseTools = true;
        public bool UseRammingBehavior;
        public bool CanBeDisabled = true;
        public bool UsePlanetHover;
        public string AlternativeBehavior = "";
        public float PlanetHoverMin = 2f;
        public float PlanetHoverMax = 25f;
        public float SpeedLimit = 50f;
        public float PlayerYAxisOffset = 0.9f;
        public float TargetDistance = 200f;
        public float MaxManeuverDistance = 250f;
        public float StaticWeaponryUsage = 300f;
        public float RammingBehaviorDistance = 75f;
        public float ToolsUsage = 8f;
        public int WaypointDelayMsMin = 0x3e8;
        public int WaypointDelayMsMax = 0xbb8;
        public int WaypointMaxTime = 0x3a98;
        public float WaypointThresholdDistance = 0.5f;
        public int LostTimeMs = 0x4e20;
        public bool UsesWeaponBehaviors;
        public float WeaponBehaviorNotFoundDelay = 3f;
        public string SoundLoop = "";
        [XmlArrayItem("WeaponBehavior")]
        public List<MyWeaponBehavior> WeaponBehaviors;
    }
}

