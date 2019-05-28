namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_TextPanelDefinition : MyObjectBuilder_CubeBlockDefinition
    {
        [ProtoMember(12)]
        public string ResourceSinkGroup;
        [ProtoMember(15)]
        public float RequiredPowerInput = 0.001f;
        [ProtoMember(0x12)]
        public int TextureResolution = 0x200;
        [ProtoMember(0x15), DefaultValue(1)]
        public int ScreenWidth = 1;
        [ProtoMember(0x18), DefaultValue(1)]
        public int ScreenHeight = 1;
        public float MinFontSize = 0.1f;
        public float MaxFontSize = 10f;
        public float MaxChangingSpeed = 30f;
    }
}

