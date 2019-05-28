namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using System;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    public class MyEnvironmentItemTypeDefinition
    {
        [XmlAttribute]
        public string Name;
        public int LodFrom = -1;
        public int LodTo = -1;
        public SerializableDefinitionId? Provider;
        [XmlElement("Proxy")]
        public SerializableDefinitionId[] Proxies;
    }
}

