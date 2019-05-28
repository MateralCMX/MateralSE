namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AiCommandBehaviorDefinition : MyObjectBuilder_AiCommandDefinition
    {
        [ProtoMember(11)]
        public string BehaviorTreeName;
        [ProtoMember(14)]
        public MyAiCommandEffect CommandEffect;
    }
}

