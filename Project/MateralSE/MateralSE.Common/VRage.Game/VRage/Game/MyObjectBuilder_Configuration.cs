namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Configuration : MyObjectBuilder_Base
    {
        [ProtoMember(0x34)]
        public CubeSizeSettings CubeSizes;
        [ProtoMember(0x37)]
        public BaseBlockSettings BaseBlockPrefabs;
        [ProtoMember(0x3a)]
        public BaseBlockSettings BaseBlockPrefabsSurvival;
        [ProtoMember(0x3e)]
        public LootBagDefinition LootBag;

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct BaseBlockSettings
        {
            [ProtoMember(0x1c), XmlAttribute]
            public string SmallStatic;
            [ProtoMember(0x1f), XmlAttribute]
            public string LargeStatic;
            [ProtoMember(0x22), XmlAttribute]
            public string SmallDynamic;
            [ProtoMember(0x25), XmlAttribute]
            public string LargeDynamic;
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct CubeSizeSettings
        {
            [ProtoMember(15), XmlAttribute]
            public float Large;
            [ProtoMember(0x12), XmlAttribute]
            public float Small;
            [ProtoMember(0x15), XmlAttribute]
            public float SmallOriginal;
        }

        [ProtoContract]
        public class LootBagDefinition
        {
            [ProtoMember(0x2c)]
            public SerializableDefinitionId ContainerDefinition;
            [ProtoMember(0x30), XmlAttribute]
            public float SearchRadius = 3f;
        }
    }
}

