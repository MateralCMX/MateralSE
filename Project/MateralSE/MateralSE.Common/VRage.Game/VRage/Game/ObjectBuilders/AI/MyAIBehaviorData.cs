namespace VRage.Game.ObjectBuilders.AI
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), ProtoContract, XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyAIBehaviorData : MyObjectBuilder_Base
    {
        [XmlArrayItem("AICategory"), ProtoMember(0x33)]
        public CategorizedData[] Entries;

        [ProtoContract]
        public class ActionData
        {
            [ProtoMember(0x27), XmlAttribute]
            public string ActionName;
            [ProtoMember(0x2a), XmlAttribute]
            public bool ReturnsRunning;
            [XmlArrayItem("Param"), ProtoMember(0x2e)]
            public MyAIBehaviorData.ParameterData[] Parameters;
        }

        [ProtoContract]
        public class CategorizedData
        {
            [ProtoMember(15)]
            public string Category;
            [XmlArrayItem("Action"), ProtoMember(0x13)]
            public MyAIBehaviorData.ActionData[] Descriptors;
        }

        [ProtoContract]
        public class ParameterData
        {
            [ProtoMember(0x1a), XmlAttribute]
            public string Name;
            [ProtoMember(0x1d), XmlAttribute]
            public string TypeFullName;
            [ProtoMember(0x20), XmlAttribute]
            public MyMemoryParameterType MemType;
        }
    }
}

