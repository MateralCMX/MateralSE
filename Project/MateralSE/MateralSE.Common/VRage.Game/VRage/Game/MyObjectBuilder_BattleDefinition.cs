namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BattleDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(13)]
        public MyObjectBuilder_Toolbar DefaultToolbar;
        [XmlArrayItem("Block"), ProtoMember(0x11), DefaultValue((string) null)]
        public SerializableDefinitionId[] SpawnBlocks;
        [ProtoMember(0x15), DefaultValue((float) 0.067f)]
        public float DefenderEntityDamage = 0.067f;
        [XmlArrayItem("Blueprint"), ProtoMember(0x19), DefaultValue((string) null)]
        public string[] DefaultBlueprints;
    }
}

