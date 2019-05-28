namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_BotDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x18)]
        public BotBehavior BotBehaviorTree;
        [ProtoMember(0x1b), DefaultValue("")]
        public string BehaviorType = "";
        [ProtoMember(30), DefaultValue("")]
        public string BehaviorSubtype = "";
        [ProtoMember(0x21)]
        public bool Commandable;

        [ProtoContract]
        public class BotBehavior
        {
            [XmlIgnore]
            public MyObjectBuilderType Type = typeof(MyObjectBuilder_BehaviorTreeDefinition);
            [XmlAttribute, ProtoMember(20)]
            public string Subtype;
        }
    }
}

