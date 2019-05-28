namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GhostCharacterDefinition : MyObjectBuilder_DefinitionBase
    {
        [XmlArrayItem("WeaponId"), ProtoMember(0x12)]
        public SerializableDefinitionId[] LeftHandWeapons;
        [XmlArrayItem("WeaponId"), ProtoMember(0x16)]
        public SerializableDefinitionId[] RightHandWeapons;
    }
}

