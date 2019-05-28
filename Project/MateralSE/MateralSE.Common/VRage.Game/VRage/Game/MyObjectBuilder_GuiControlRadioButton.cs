namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GuiControlRadioButton : MyObjectBuilder_GuiControlBase
    {
        [ProtoMember(0x2f)]
        public int Key;
        [ProtoMember(50)]
        public MyGuiControlRadioButtonStyleEnum VisualStyle;
        [ProtoMember(0x38)]
        public MyGuiCustomVisualStyle? CustomVisualStyle;
    }
}

