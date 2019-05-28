namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GuiControlList : MyObjectBuilder_GuiControlParent
    {
        [ProtoMember(0x13)]
        public MyGuiControlListStyleEnum VisualStyle;
    }
}

