namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BlueprintClassDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(13), ModdableContentFile("dds")]
        public string HighlightIcon;
        [ProtoMember(0x11), ModdableContentFile("dds")]
        public string InputConstraintIcon;
        [ProtoMember(0x15), ModdableContentFile("dds")]
        public string OutputConstraintIcon;
        [ProtoMember(0x19), DefaultValue((string) null)]
        public string ProgressBarSoundCue;
    }
}

