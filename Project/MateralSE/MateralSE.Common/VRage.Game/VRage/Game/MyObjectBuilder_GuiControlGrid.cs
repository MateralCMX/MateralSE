namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GuiControlGrid : MyObjectBuilder_GuiControlBase
    {
        [ProtoMember(0x17)]
        public MyGuiControlGridStyleEnum VisualStyle;
        [ProtoMember(0x1a)]
        public int DisplayColumnsCount = 1;
        [ProtoMember(0x1d)]
        public int DisplayRowsCount = 1;
    }
}

