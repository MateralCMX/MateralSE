namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_FontDefinition : MyObjectBuilder_DefinitionBase
    {
        [ModdableContentFile("xml")]
        public string Path;
        private Color? m_colorMask = new Color?(Color.White);
        [ProtoMember(0x36)]
        public bool Default;
        [XmlArrayItem("Resource")]
        public List<MyObjectBuilder_FontData> Resources;
        [XmlArrayItem("LanguageDefinition")]
        public List<LanguageResources> LanguageSpecificDefinitions;

        public Color? ColorMask
        {
            get => 
                this.m_colorMask;
            set
            {
                if (this.m_colorMask != null)
                {
                    this.m_colorMask = new Color(value.Value.R, value.Value.G, value.Value.B, 0xff);
                }
            }
        }

        public Color? ColorMaskAlpha
        {
            get => 
                this.m_colorMask;
            set => 
                (this.m_colorMask = value);
        }

        public class LanguageResources
        {
            [XmlAttribute]
            public string Language;
            [XmlArrayItem("Resource")]
            public List<MyObjectBuilder_FontData> Resources;
        }
    }
}

