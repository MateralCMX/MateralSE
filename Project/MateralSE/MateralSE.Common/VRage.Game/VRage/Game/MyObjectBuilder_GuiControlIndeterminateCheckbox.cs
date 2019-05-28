namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GuiControlIndeterminateCheckbox : MyObjectBuilder_GuiControlBase
    {
        [ProtoMember(0x19)]
        public CheckStateEnum State;
        [ProtoMember(0x1c)]
        public MyGuiControlIndeterminateCheckboxStyleEnum VisualStyle;
    }
}

