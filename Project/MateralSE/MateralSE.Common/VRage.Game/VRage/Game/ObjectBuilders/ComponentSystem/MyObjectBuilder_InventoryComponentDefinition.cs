namespace VRage.Game.ObjectBuilders.ComponentSystem
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_InventoryComponentDefinition : MyObjectBuilder_ComponentDefinitionBase
    {
        [ProtoMember(0x24)]
        public SerializableVector3? Size;
        [ProtoMember(0x27)]
        public float Volume = float.MaxValue;
        [ProtoMember(0x2a)]
        public float Mass = float.MaxValue;
        [ProtoMember(0x2d)]
        public bool RemoveEntityOnEmpty;
        [ProtoMember(0x30)]
        public bool MultiplierEnabled = true;
        [ProtoMember(0x33)]
        public int MaxItemCount = 0x7fffffff;
        [ProtoMember(0x36), DefaultValue((string) null)]
        public InventoryConstraintDefinition InputConstraint;

        [ProtoContract]
        public class InventoryConstraintDefinition
        {
            [XmlAttribute("Description"), DefaultValue((string) null), ProtoMember(0x13)]
            public string Description;
            [XmlAttribute("Icon"), DefaultValue((string) null), ProtoMember(0x17), ModdableContentFile(new string[] { "dds", "png" })]
            public string Icon;
            [XmlAttribute("Whitelist"), ProtoMember(0x1c)]
            public bool IsWhitelist = true;
            [XmlElement("Entry"), ProtoMember(0x20)]
            public List<SerializableDefinitionId> Entries = new List<SerializableDefinitionId>();
        }
    }
}

