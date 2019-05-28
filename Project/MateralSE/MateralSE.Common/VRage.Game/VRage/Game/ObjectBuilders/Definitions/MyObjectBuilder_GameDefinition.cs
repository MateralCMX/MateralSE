namespace VRage.Game.ObjectBuilders.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), Description("Main definition for a game."), XmlType("VR.GameDefinition"), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_GameDefinition : MyObjectBuilder_DefinitionBase
    {
        [Description("What object builder to inherit from if any."), DefaultValue((string) null)]
        public string InheritFrom;
        [Description("Weather this game definition is the default for new scenarios."), DefaultValue(false)]
        public bool Default;
        [Description("List of session components to load for this Game."), DefaultValue("empty"), XmlArrayItem("Component")]
        public List<Comp> SessionComponents = new List<Comp>();

        [StructLayout(LayoutKind.Sequential)]
        public struct Comp
        {
            [XmlAttribute]
            public string Type;
            [XmlAttribute]
            public string Subtype;
            [XmlText]
            public string ComponentName;
        }
    }
}

