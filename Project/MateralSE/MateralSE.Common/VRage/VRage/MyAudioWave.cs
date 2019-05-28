namespace VRage
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.Data.Audio;

    [ProtoContract, XmlType("Wave")]
    public sealed class MyAudioWave
    {
        [ProtoMember(0x1a), XmlAttribute]
        public MySoundDimensions Type;
        [ProtoMember(0x1d), DefaultValue(""), ModdableContentFile(new string[] { "xwm", "wav" })]
        public string Start;
        [ProtoMember(0x20), DefaultValue(""), ModdableContentFile(new string[] { "xwm", "wav" })]
        public string Loop;
        [ProtoMember(0x23), DefaultValue(""), ModdableContentFile(new string[] { "xwm", "wav" })]
        public string End;
    }
}

