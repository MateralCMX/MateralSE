namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GuiControlLabel : MyObjectBuilder_GuiControlBase
    {
        [ProtoMember(0x3a)]
        public string TextEnum;
        [ProtoMember(0x3d)]
        public string Text;
        [ProtoMember(0x40)]
        public float TextScale;
        [ProtoMember(0x43)]
        public string Font;
    }
}

