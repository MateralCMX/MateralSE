namespace VRage.Game.News
{
    using System;
    using System.Xml.Serialization;

    public class MyNewsEntry
    {
        [XmlAttribute(AttributeName="title")]
        public string Title;
        [XmlAttribute(AttributeName="date")]
        public string Date;
        [XmlAttribute(AttributeName="version")]
        public string Version;
        [XmlAttribute(AttributeName="public")]
        public bool Public = true;
        [XmlAttribute(AttributeName="dev")]
        public bool Dev;
        [XmlText]
        public string Text;
    }
}

