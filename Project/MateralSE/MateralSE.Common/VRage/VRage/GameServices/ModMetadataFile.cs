namespace VRage.GameServices
{
    using System;
    using System.Xml.Serialization;

    [Serializable, XmlRoot("ModMetadata")]
    public class ModMetadataFile
    {
        [XmlElement("ModVersion")]
        public string ModVersion;
        [XmlElement("MinGameVersion")]
        public string MinGameVersion;
        [XmlElement("MaxGameVersion")]
        public string MaxGameVersion;
    }
}

