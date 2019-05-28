namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AnimationDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x25), ModdableContentFile("mwm")]
        public string AnimationModel;
        [ProtoMember(0x29), ModdableContentFile("mwm")]
        public string AnimationModelFPS;
        [ProtoMember(0x2d)]
        public int ClipIndex;
        [ProtoMember(0x30)]
        public string InfluenceArea;
        [ProtoMember(0x33)]
        public bool AllowInCockpit = true;
        [ProtoMember(0x36)]
        public bool AllowWithWeapon;
        [ProtoMember(0x39)]
        public string SupportedSkeletons = "Humanoid";
        [ProtoMember(60)]
        public bool Loop;
        [ProtoMember(0x3f)]
        public SerializableDefinitionId LeftHandItem;
        [ProtoMember(0x42), DefaultValue((string) null), XmlArrayItem("AnimationSet")]
        public AnimationSet[] AnimationSets;
    }
}

