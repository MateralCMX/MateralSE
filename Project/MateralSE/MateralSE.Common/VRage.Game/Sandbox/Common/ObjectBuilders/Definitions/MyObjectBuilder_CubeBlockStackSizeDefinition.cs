namespace Sandbox.Common.ObjectBuilders.Definitions
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CubeBlockStackSizeDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x22), XmlElement("Block")]
        public BlockStackSizeDef[] Blocks;

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct BlockStackSizeDef
        {
            [ProtoMember(0x15), XmlAttribute("TypeId")]
            public string TypeId;
            [ProtoMember(0x19), XmlAttribute("SubtypeId")]
            public string SubtypeId;
            [ProtoMember(0x1d), XmlAttribute("MaxStackSize"), DefaultValue(1)]
            public int MaxStackSize;
        }
    }
}

