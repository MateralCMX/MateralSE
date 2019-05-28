namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_PreloadFileInfo : MyObjectBuilder_Base
    {
        [ProtoMember(0x17)]
        public string Name;
        [ProtoMember(0x1a), DefaultValue(false)]
        public bool LoadOnDedicated;
    }
}

