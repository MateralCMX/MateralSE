namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_FloraElementDefinition : MyObjectBuilder_DefinitionBase
    {
        [ProtoMember(0x36), DefaultValue((string) null), XmlArrayItem("Group")]
        public string[] AppliedGroups;
        [ProtoMember(0x3a)]
        public float SpawnProbability = 1f;
        [ProtoMember(0x3d)]
        public MyAreaTransformType AreaTransformType = MyAreaTransformType.ENRICHING;
        [ProtoMember(0x40), DefaultValue(false)]
        public bool Regrowable;
        [ProtoMember(0x43), DefaultValue(0)]
        public float GrowTime;
        [ProtoMember(70), DefaultValue((string) null), XmlArrayItem("Step")]
        public GrowthStep[] GrowthSteps;
        [ProtoMember(0x4a)]
        public int PostGatherStep;
        [ProtoMember(0x4d)]
        public int GatherableStep = -1;
        [ProtoMember(80), DefaultValue((string) null)]
        public GatheredItemDef GatheredItem;
        [ProtoMember(0x53), DefaultValue(0)]
        public float DecayTime;

        [ProtoContract]
        public class EnvItem
        {
            [ProtoMember(0x17), XmlAttribute]
            public string Group;
            [ProtoMember(0x1b), XmlAttribute]
            public string Subtype;
        }

        [ProtoContract]
        public class GatheredItemDef
        {
            [ProtoMember(0x2f)]
            public SerializableDefinitionId Id;
            [ProtoMember(50)]
            public float Amount;
        }

        [ProtoContract]
        public class GrowthStep
        {
            [ProtoMember(0x23), XmlAttribute]
            public int GroupInsId = -1;
            [ProtoMember(0x27), XmlAttribute]
            public float Percent = 1f;
        }
    }
}

