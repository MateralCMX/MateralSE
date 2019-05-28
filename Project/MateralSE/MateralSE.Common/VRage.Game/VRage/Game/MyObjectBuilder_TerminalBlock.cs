namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_TerminalBlock : MyObjectBuilder_CubeBlock
    {
        [ProtoMember(13), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public string CustomName;
        [ProtoMember(0x11)]
        public bool ShowOnHUD;
        [ProtoMember(20)]
        public bool ShowInTerminal = true;
        [ProtoMember(0x17)]
        public bool ShowInToolbarConfig = true;
        [ProtoMember(0x1a)]
        public bool ShowInInventory = true;
    }
}

