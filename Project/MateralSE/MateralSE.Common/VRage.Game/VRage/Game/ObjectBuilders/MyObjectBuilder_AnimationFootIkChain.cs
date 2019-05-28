namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlType("AnimationIkChain"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AnimationFootIkChain : MyObjectBuilder_Base
    {
        [ProtoMember(13)]
        public string FootBone;
        [ProtoMember(15)]
        public int ChainLength = 1;
        [ProtoMember(0x11)]
        public bool AlignBoneWithTerrain = true;
    }
}

