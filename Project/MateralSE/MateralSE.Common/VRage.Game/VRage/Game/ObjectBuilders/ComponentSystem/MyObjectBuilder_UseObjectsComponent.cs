namespace VRage.Game.ObjectBuilders.ComponentSystem
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_UseObjectsComponent : MyObjectBuilder_ComponentBase
    {
        [ProtoMember(13)]
        public uint CustomDetectorsCount;
        [ProtoMember(0x10), DefaultValue((string) null)]
        public string[] CustomDetectorsNames;
        [ProtoMember(0x13), DefaultValue((string) null)]
        public Matrix[] CustomDetectorsMatrices;
    }
}

