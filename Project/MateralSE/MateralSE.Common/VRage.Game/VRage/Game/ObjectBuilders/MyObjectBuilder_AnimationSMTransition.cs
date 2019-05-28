namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;
    using VRageRender.Animations;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AnimationSMTransition : MyObjectBuilder_Base
    {
        [ProtoMember(14), XmlAttribute]
        public string Name;
        [ProtoMember(0x13), XmlAttribute]
        public string From;
        [ProtoMember(0x18), XmlAttribute]
        public string To;
        [ProtoMember(0x1c), XmlAttribute]
        public double TimeInSec;
        [ProtoMember(0x20), XmlAttribute]
        public MyAnimationTransitionSyncType Sync;
        [ProtoMember(0x25), XmlArrayItem("Conjunction")]
        public MyObjectBuilder_AnimationSMConditionsConjunction[] Conditions;
        [ProtoMember(0x2b)]
        public int? Priority;
        [ProtoMember(0x2e), XmlAttribute]
        public MyAnimationTransitionCurve Curve;
    }
}

