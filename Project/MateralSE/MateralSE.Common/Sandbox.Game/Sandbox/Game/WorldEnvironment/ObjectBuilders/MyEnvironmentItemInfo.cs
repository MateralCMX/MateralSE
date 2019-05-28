namespace Sandbox.Game.WorldEnvironment.ObjectBuilders
{
    using System;
    using System.Xml.Serialization;
    using VRage.Utils;

    public class MyEnvironmentItemInfo
    {
        [XmlAttribute]
        public string Type;
        public MyStringHash Subtype;
        [XmlAttribute]
        public float Offset;
        [XmlAttribute]
        public float Density;

        [XmlAttribute("Subtype")]
        public string SubtypeText
        {
            get => 
                this.Subtype.ToString();
            set => 
                (this.Subtype = MyStringHash.GetOrCompute(value));
        }
    }
}

