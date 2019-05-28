namespace VRage.Game.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_MainMenuInventorySceneDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x11, IsRequired=false)]
        public string SceneDirectory;
    }
}

