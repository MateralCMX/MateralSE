namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AnimationTreeNodeDynamicTrack : MyObjectBuilder_AnimationTreeNodeTrack
    {
        [ProtoMember(0x71)]
        public string DefaultAnimation;
    }
}

