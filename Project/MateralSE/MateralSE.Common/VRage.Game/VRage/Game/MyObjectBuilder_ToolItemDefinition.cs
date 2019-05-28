namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Game.Gui;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_ToolItemDefinition : MyObjectBuilder_PhysicalItemDefinition
    {
        [XmlArrayItem("Mining"), ProtoMember(0x6d), DefaultValue((string) null)]
        public MyVoxelMiningDefinition[] VoxelMinings;
        [XmlArrayItem("Action"), ProtoMember(0x71), DefaultValue((string) null)]
        public MyToolActionDefinition[] PrimaryActions;
        [XmlArrayItem("Action"), ProtoMember(0x75), DefaultValue((string) null)]
        public MyToolActionDefinition[] SecondaryActions;
        [ProtoMember(120), DefaultValue(1)]
        public float HitDistance = 1f;

        [ProtoContract]
        public class MyToolActionDefinition
        {
            [ProtoMember(0x42)]
            public string Name;
            [ProtoMember(0x45), DefaultValue(0)]
            public float StartTime;
            [ProtoMember(0x48), DefaultValue(1)]
            public float EndTime = 1f;
            [ProtoMember(0x4b), DefaultValue((float) 1f)]
            public float Efficiency = 1f;
            [ProtoMember(0x4e), DefaultValue((string) null)]
            public string StatsEfficiency;
            [ProtoMember(0x51), DefaultValue((string) null)]
            public string SwingSound;
            [ProtoMember(0x54), DefaultValue((float) 0f)]
            public float SwingSoundStart;
            [ProtoMember(0x57), DefaultValue((float) 0f)]
            public float HitStart;
            [ProtoMember(90), DefaultValue((float) 1f)]
            public float HitDuration = 1f;
            [ProtoMember(0x5d), DefaultValue((string) null)]
            public string HitSound;
            [ProtoMember(0x60), DefaultValue((float) 0f)]
            public float CustomShapeRadius;
            [ProtoMember(0x63)]
            public MyHudTexturesEnum Crosshair = MyHudTexturesEnum.HudOre;
            [XmlArrayItem("HitCondition"), ProtoMember(0x67), DefaultValue((string) null)]
            public MyObjectBuilder_ToolItemDefinition.MyToolActionHitCondition[] HitConditions;
        }

        [ProtoContract]
        public class MyToolActionHitCondition
        {
            [ProtoMember(0x26), DefaultValue((string) null)]
            public string[] EntityType;
            [ProtoMember(0x29)]
            public string Animation;
            [ProtoMember(0x2c)]
            public float AnimationTimeScale = 1f;
            [ProtoMember(0x2f)]
            public string StatsAction;
            [ProtoMember(50)]
            public string StatsActionIfHit;
            [ProtoMember(0x35)]
            public string StatsModifier;
            [ProtoMember(0x38)]
            public string StatsModifierIfHit;
            [ProtoMember(0x3b)]
            public string Component;
        }

        [ProtoContract]
        public class MyVoxelMiningDefinition
        {
            [ProtoMember(0x11), DefaultValue((string) null)]
            public string MinedOre;
            [ProtoMember(0x15), DefaultValue(0)]
            public int HitCount;
            [ProtoMember(0x18), DefaultValue((string) null)]
            public SerializableDefinitionId PhysicalItemId;
            [ProtoMember(0x1c), DefaultValue((float) 0f)]
            public float RemovedRadius;
            [ProtoMember(0x1f), DefaultValue(false)]
            public bool OnlyApplyMaterial;
        }
    }
}

