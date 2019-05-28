namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BlueprintDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(14), XmlArrayItem("Item")]
        public BlueprintItem[] Prerequisites;
        [ProtoMember(0x15)]
        public BlueprintItem Result;
        [ProtoMember(0x18), XmlArrayItem("Item")]
        public BlueprintItem[] Results;
        [ProtoMember(0x20)]
        public float BaseProductionTimeInSeconds = 1f;
        [ProtoMember(0x23), DefaultValue((string) null)]
        public string ProgressBarSoundCue;
        [ProtoMember(0x26, IsRequired=false), DefaultValue(false)]
        public bool IsPrimary;
    }
}

