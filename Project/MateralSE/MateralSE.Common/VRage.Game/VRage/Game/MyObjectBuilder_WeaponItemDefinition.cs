namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_WeaponItemDefinition : MyObjectBuilder_PhysicalItemDefinition
    {
        [ProtoMember(0x17)]
        public PhysicalItemWeaponDefinitionId WeaponDefinitionId;
        [ProtoMember(0x1a)]
        public bool ShowAmmoCount;

        [ProtoContract]
        public class PhysicalItemWeaponDefinitionId
        {
            [XmlIgnore]
            public MyObjectBuilderType Type = typeof(MyObjectBuilder_WeaponDefinition);
            [XmlAttribute, ProtoMember(0x13)]
            public string Subtype;
        }
    }
}

