namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [XmlRoot("ScenarioDefinitions"), ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ScenarioDefinitions : MyObjectBuilder_Base
    {
        [XmlArrayItem("ScenarioDefinition"), ProtoMember(14)]
        public MyObjectBuilder_ScenarioDefinition[] Scenarios;
    }
}

