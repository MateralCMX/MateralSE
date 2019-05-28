namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_World : MyObjectBuilder_Base
    {
        [ProtoMember(14)]
        public MyObjectBuilder_Checkpoint Checkpoint;
        [ProtoMember(0x11)]
        public MyObjectBuilder_Sector Sector;
        [ProtoMember(20)]
        public SerializableDictionary<string, byte[]> VoxelMaps;
        public List<BoundingBoxD> Clusters;
        public List<MyObjectBuilder_Planet> Planets;
    }
}

