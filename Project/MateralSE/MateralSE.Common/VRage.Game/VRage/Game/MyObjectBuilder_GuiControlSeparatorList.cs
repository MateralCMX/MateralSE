namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GuiControlSeparatorList : MyObjectBuilder_GuiControlBase
    {
        [ProtoMember(30)]
        public List<Separator> Separators = new List<Separator>();

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct Separator
        {
            [ProtoMember(0x11), DefaultValue((float) 0f), XmlAttribute]
            public float StartX { get; set; }
            [ProtoMember(20), DefaultValue((float) 0f), XmlAttribute]
            public float StartY { get; set; }
            [ProtoMember(0x17), DefaultValue((float) 0f), XmlAttribute]
            public float SizeX { get; set; }
            [ProtoMember(0x1a), DefaultValue((float) 0f), XmlAttribute]
            public float SizeY { get; set; }
        }
    }
}

