namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_DefinitionsToPreload : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(13), XmlArrayItem("File")]
        public MyObjectBuilder_PreloadFileInfo[] DefinitionFiles;
    }
}

