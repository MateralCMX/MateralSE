namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_DropContainerDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(30, IsRequired=false)]
        public string Prefab;
        [ProtoMember(0x21, IsRequired=false)]
        public MyContainerSpawnRules SpawnRules;
        [ProtoMember(0x24, IsRequired=false)]
        public float Priority = 1f;
    }
}

