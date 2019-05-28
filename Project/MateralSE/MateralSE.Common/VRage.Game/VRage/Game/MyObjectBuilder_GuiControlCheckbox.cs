namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GuiControlCheckbox : MyObjectBuilder_GuiControlBase
    {
        [ProtoMember(0x16)]
        public bool IsChecked;
        [ProtoMember(0x19)]
        public string CheckedTexture;
        [ProtoMember(0x1c)]
        public MyGuiControlCheckboxStyleEnum VisualStyle;
    }
}

