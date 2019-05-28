namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_PrefabDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(14)]
        public bool RespawnShip;
        [ProtoMember(0x13)]
        public MyObjectBuilder_CubeGrid CubeGrid;
        [ProtoMember(0x17), XmlArrayItem("CubeGrid")]
        public MyObjectBuilder_CubeGrid[] CubeGrids;
        [ProtoMember(0x1b), ModdableContentFile("sbc")]
        public string PrefabPath;

        public bool ShouldSerializeCubeGrid() => 
            false;

        public bool ShouldSerializeRespawnShip() => 
            false;
    }
}

