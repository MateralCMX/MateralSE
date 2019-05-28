namespace VRage.Game.ObjectBuilders.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_SessionComponentResearch : MyObjectBuilder_SessionComponent
    {
        [XmlElement("Research")]
        public List<ResearchData> Researches;

        [StructLayout(LayoutKind.Sequential)]
        public struct ResearchData
        {
            [XmlAttribute("Identity")]
            public long IdentityId;
            [XmlElement("Entry")]
            public List<SerializableDefinitionId> Definitions;
        }
    }
}

