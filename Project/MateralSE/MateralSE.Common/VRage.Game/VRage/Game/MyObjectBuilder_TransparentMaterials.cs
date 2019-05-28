namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_TransparentMaterials : MyObjectBuilder_Base
    {
        [XmlArrayItem("TransparentMaterial"), ProtoMember(13)]
        public MyObjectBuilder_TransparentMaterial[] Materials;
    }
}

