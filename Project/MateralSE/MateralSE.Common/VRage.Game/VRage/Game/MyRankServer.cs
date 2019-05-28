namespace VRage.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    public class MyRankServer
    {
        [XmlAttribute]
        public int Rank { get; set; }

        [XmlAttribute]
        public string Address { get; set; }
    }
}

