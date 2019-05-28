namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_RespawnShipDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(12)]
        public string Prefab;
        [ProtoMember(15)]
        public int CooldownSeconds;
        [ProtoMember(0x12, IsRequired=false)]
        public bool SpawnWithDefaultItems = true;
        [ProtoMember(0x15, IsRequired=false)]
        public bool UseForSpace;
        [ProtoMember(0x18, IsRequired=false)]
        public float MinimalAirDensity = 0.7f;
        [ProtoMember(0x1b, IsRequired=false)]
        public bool UseForPlanetsWithAtmosphere;
        [ProtoMember(30, IsRequired=false)]
        public bool UseForPlanetsWithoutAtmosphere;
        [ProtoMember(0x21, IsRequired=false)]
        public float? PlanetDeployAltitude;
        [ProtoMember(0x24, IsRequired=false)]
        public SerializableVector3 InitialLinearVelocity = Vector3.Zero;
        [ProtoMember(0x27, IsRequired=false)]
        public SerializableVector3 InitialAngularVelocity = Vector3.Zero;
        [ProtoMember(0x2a)]
        public string HelpTextLocalizationId;
    }
}

