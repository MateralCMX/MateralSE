namespace VRage.Game.ObjectBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Localization : MyObjectBuilder_Base
    {
        public uint Id;
        public string Language = "English";
        public string Context = "VRage";
        public string ResourceName = "Default Name";
        public bool Default;
        public string ResXName;
        [XmlIgnore]
        public List<KeyEntry> Entries = new List<KeyEntry>();
        [XmlIgnore]
        public bool Modified;

        public override string ToString() => 
            (this.ResourceName + " " + this.Id);

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyEntry
        {
            public string Key;
            public string Value;
        }
    }
}

