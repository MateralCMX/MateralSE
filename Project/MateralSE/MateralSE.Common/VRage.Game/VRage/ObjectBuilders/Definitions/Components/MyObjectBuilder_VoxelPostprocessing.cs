namespace VRage.ObjectBuilders.Definitions.Components
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_VoxelPostprocessing : MyObjectBuilder_Base
    {
        [ProtoMember(11), XmlAttribute]
        public bool ForPhysics;
    }
}

