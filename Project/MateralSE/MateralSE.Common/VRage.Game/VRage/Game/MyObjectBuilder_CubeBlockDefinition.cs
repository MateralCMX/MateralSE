namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Data;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CubeBlockDefinition : MyObjectBuilder_PhysicalModelDefinition
    {
        public VoxelPlacementOverride? VoxelPlacement;
        [ProtoMember(0x1a9), DefaultValue(false)]
        public bool SilenceableByShipSoundSystem;
        [ProtoMember(0x1ac)]
        public MyCubeSize CubeSize;
        [ProtoMember(0x1af)]
        public MyBlockTopology BlockTopology;
        [ProtoMember(0x1b2)]
        public SerializableVector3I Size;
        [ProtoMember(0x1b5)]
        public SerializableVector3 ModelOffset;
        [ProtoMember(0x1bc)]
        public PatternDefinition CubeDefinition;
        [XmlArrayItem("Component"), ProtoMember(0x1c0)]
        public CubeBlockComponent[] Components;
        [XmlArrayItem("Effect"), ProtoMember(0x1c4)]
        public CubeBlockEffectBase[] Effects;
        [ProtoMember(0x1c7)]
        public CriticalPart CriticalComponent;
        [ProtoMember(0x1ca)]
        public MountPoint[] MountPoints;
        [ProtoMember(0x1cd)]
        public Variant[] Variants;
        [XmlArrayItem("Component"), ProtoMember(0x1d1)]
        public EntityComponentDefinition[] EntityComponents;
        [ProtoMember(0x1d4), DefaultValue(1)]
        public MyPhysicsOption PhysicsOption = MyPhysicsOption.Box;
        [XmlArrayItem("Model"), ProtoMember(0x1d8), DefaultValue((string) null)]
        public List<BuildProgressModel> BuildProgressModels;
        [ProtoMember(0x1db)]
        public string BlockPairName;
        [ProtoMember(0x1de)]
        public SerializableVector3I? Center;
        [ProtoMember(0x1e2), DefaultValue(0)]
        public MySymmetryAxisEnum MirroringX;
        [ProtoMember(0x1e5), DefaultValue(0)]
        public MySymmetryAxisEnum MirroringY;
        [ProtoMember(0x1e8), DefaultValue(0)]
        public MySymmetryAxisEnum MirroringZ;
        [ProtoMember(0x1eb), DefaultValue((float) 1f)]
        public float DeformationRatio = 1f;
        [ProtoMember(0x1ee)]
        public string EdgeType;
        [ProtoMember(0x1f1), DefaultValue((float) 10f)]
        public float BuildTimeSeconds = 10f;
        [ProtoMember(500), DefaultValue((float) 1f)]
        public float DisassembleRatio = 1f;
        [ProtoMember(0x1f7)]
        public MyAutorotateMode AutorotateMode;
        [ProtoMember(0x1fa)]
        public string MirroringBlock;
        [ProtoMember(0x1fd)]
        public bool UseModelIntersection;
        [ProtoMember(0x203)]
        public string PrimarySound;
        [ProtoMember(0x206)]
        public string ActionSound;
        [ProtoMember(0x209), DefaultValue((string) null)]
        public string BuildType;
        [ProtoMember(0x20c), DefaultValue((string) null)]
        public string BuildMaterial;
        [XmlArrayItem("Template"), ProtoMember(0x210), DefaultValue((string) null)]
        public string[] CompoundTemplates;
        [ProtoMember(0x213), DefaultValue(true)]
        public bool CompoundEnabled = true;
        [XmlArrayItem("Definition"), ProtoMember(0x217), DefaultValue((string) null)]
        public MySubBlockDefinition[] SubBlockDefinitions;
        [ProtoMember(0x21a), DefaultValue((string) null)]
        public string MultiBlock;
        [ProtoMember(0x21d), DefaultValue((string) null)]
        public string NavigationDefinition;
        [ProtoMember(0x220), DefaultValue(true)]
        public bool GuiVisible = true;
        [XmlArrayItem("BlockVariant"), ProtoMember(0x224), DefaultValue((string) null)]
        public SerializableDefinitionId[] BlockVariants;
        [ProtoMember(0x228), DefaultValue(3)]
        public MyBlockDirection Direction = MyBlockDirection.Both;
        [ProtoMember(0x22c), DefaultValue(3)]
        public MyBlockRotation Rotation = MyBlockRotation.Both;
        [XmlArrayItem("GeneratedBlock"), ProtoMember(560), DefaultValue((string) null)]
        public SerializableDefinitionId[] GeneratedBlocks;
        [ProtoMember(0x233), DefaultValue((string) null)]
        public string GeneratedBlockType;
        [ProtoMember(0x237), DefaultValue(false)]
        public bool Mirrored;
        [ProtoMember(570, IsRequired=false), DefaultValue(0)]
        public int DamageEffectId;
        [ProtoMember(0x23d), DefaultValue("")]
        public string DestroyEffect = "";
        [ProtoMember(0x240), DefaultValue("PoofExplosionCat1")]
        public string DestroySound = "PoofExplosionCat1";
        [ProtoMember(580), DefaultValue((string) null)]
        public List<BoneInfo> Skeleton;
        [ProtoMember(0x248), DefaultValue(false)]
        public bool RandomRotation;
        [ProtoMember(0x24c), DefaultValue((string) null)]
        public bool? IsAirTight;
        [ProtoMember(0x251), DefaultValue(true)]
        public bool IsStandAlone = true;
        [ProtoMember(0x256), DefaultValue(true)]
        public bool HasPhysics = true;
        public bool UseNeighbourOxygenRooms;
        [ProtoMember(0x25b), DefaultValue(1), Obsolete]
        public int Points;
        [ProtoMember(0x25e), DefaultValue(0)]
        public int MaxIntegrity;
        [ProtoMember(0x261), DefaultValue(1)]
        public float BuildProgressToPlaceGeneratedBlocks = 1f;
        [ProtoMember(0x264), DefaultValue((string) null)]
        public string DamagedSound;
        [ProtoMember(0x267), DefaultValue(true)]
        public bool CreateFracturedPieces = true;
        [ProtoMember(0x26a), DefaultValue((string) null)]
        public string EmissiveColorPreset;
        [ProtoMember(0x26d), DefaultValue((float) 1f)]
        public float GeneralDamageMultiplier = 1f;
        [ProtoMember(0x270, IsRequired=false), DefaultValue("")]
        public string DamageEffectName = "";
        [ProtoMember(0x273, IsRequired=false), DefaultValue(true)]
        public bool UsesDeformation = true;
        [ProtoMember(630, IsRequired=false), DefaultValue((string) null)]
        public Vector3? DestroyEffectOffset;
        [ProtoMember(0x279), DefaultValue(1)]
        public int PCU = 1;
        [ProtoMember(0x27c, IsRequired=false), DefaultValue(true)]
        public bool PlaceDecals = true;

        public bool ShouldSerializeCenter() => 
            (this.Center != null);

        [ProtoContract]
        public class BuildProgressModel
        {
            [XmlAttribute, ProtoMember(0x144)]
            public float BuildPercentUpperBound;
            [XmlAttribute, ProtoMember(0x148), ModdableContentFile("mwm")]
            public string File;
            [XmlAttribute, ProtoMember(0x14d), DefaultValue(false)]
            public bool RandomOrientation;
            [ProtoMember(0x150), XmlArray("MountPointOverrides"), XmlArrayItem("MountPoint"), DefaultValue((string) null)]
            public MyObjectBuilder_CubeBlockDefinition.MountPoint[] MountPoints;
            [XmlAttribute, ProtoMember(0x156), DefaultValue(true)]
            public bool Visible = true;
        }

        [ProtoContract]
        public class CriticalPart
        {
            [XmlIgnore]
            public MyObjectBuilderType Type = typeof(MyObjectBuilder_Component);
            [XmlAttribute, ProtoMember(0xfe)]
            public string Subtype;
            [XmlAttribute, ProtoMember(0x101)]
            public int Index;
        }

        [ProtoContract]
        public class CubeBlockComponent
        {
            [XmlIgnore]
            public MyObjectBuilderType Type = typeof(MyObjectBuilder_Component);
            [XmlAttribute, ProtoMember(0xed)]
            public string Subtype;
            [XmlAttribute, ProtoMember(240)]
            public ushort Count;
            [ProtoMember(0xf3)]
            public SerializableDefinitionId DeconstructId;
        }

        [ProtoContract]
        public class CubeBlockEffect
        {
            [XmlAttribute, ProtoMember(0x189)]
            public string Name = "";
            [XmlAttribute, ProtoMember(0x18d)]
            public string Origin = "";
            [XmlAttribute, ProtoMember(0x191)]
            public float Delay;
            [XmlAttribute, ProtoMember(0x195)]
            public float Duration;
            [XmlAttribute, ProtoMember(0x199)]
            public bool Loop;
            [XmlAttribute, ProtoMember(0x19d)]
            public float SpawnTimeMin;
            [XmlAttribute, ProtoMember(0x1a1)]
            public float SpawnTimeMax;
        }

        [ProtoContract]
        public class CubeBlockEffectBase
        {
            [XmlAttribute, ProtoMember(0x175)]
            public string Name = "";
            [XmlAttribute, ProtoMember(0x179)]
            public float ParameterMin = float.MinValue;
            [XmlAttribute, ProtoMember(0x17d)]
            public float ParameterMax = float.MaxValue;
            [XmlArrayItem("ParticleEffect"), ProtoMember(0x181)]
            public MyObjectBuilder_CubeBlockDefinition.CubeBlockEffect[] ParticleEffects;
        }

        [ProtoContract]
        public class EntityComponentDefinition
        {
            [XmlAttribute, ProtoMember(0x169)]
            public string ComponentType;
            [XmlAttribute, ProtoMember(0x16d)]
            public string BuilderType;
        }

        [ProtoContract]
        public class MountPoint
        {
            [XmlAttribute, ProtoMember(0xb5)]
            public BlockSideEnum Side;
            [XmlIgnore, ProtoMember(0xb8)]
            public SerializableVector2 Start;
            [XmlIgnore, ProtoMember(0xbb)]
            public SerializableVector2 End;
            [XmlAttribute, ProtoMember(0xd8), DefaultValue(0)]
            public byte ExclusionMask;
            [XmlAttribute, ProtoMember(0xdb), DefaultValue(0)]
            public byte PropertiesMask;
            [XmlAttribute, ProtoMember(0xde), DefaultValue(true)]
            public bool Enabled = true;
            [XmlAttribute, ProtoMember(0xe1), DefaultValue(false)]
            public bool Default;

            [XmlAttribute]
            public float StartX
            {
                get => 
                    this.Start.X;
                set => 
                    (this.Start.X = value);
            }

            [XmlAttribute]
            public float StartY
            {
                get => 
                    this.Start.Y;
                set => 
                    (this.Start.Y = value);
            }

            [XmlAttribute]
            public float EndX
            {
                get => 
                    this.End.X;
                set => 
                    (this.End.X = value);
            }

            [XmlAttribute]
            public float EndY
            {
                get => 
                    this.End.Y;
                set => 
                    (this.End.Y = value);
            }
        }

        [ProtoContract]
        public class MySubBlockDefinition
        {
            [XmlAttribute, ProtoMember(350)]
            public string SubBlock;
            [ProtoMember(0x161)]
            public SerializableDefinitionId Id;
        }

        [ProtoContract]
        public class PatternDefinition
        {
            [ProtoMember(0x117)]
            public MyCubeTopology CubeTopology;
            [ProtoMember(0x119)]
            public MyObjectBuilder_CubeBlockDefinition.Side[] Sides;
            [ProtoMember(0x11b)]
            public bool ShowEdges;
        }

        [ProtoContract]
        public class Side
        {
            [XmlAttribute, ProtoMember(0x123), ModdableContentFile("mwm")]
            public string Model;
            [XmlIgnore, ProtoMember(0x128)]
            public SerializableVector2I PatternSize;
            [XmlAttribute]
            public int ScaleTileU = 1;
            [XmlAttribute]
            public int ScaleTileV = 1;

            [XmlAttribute]
            public int PatternWidth
            {
                get => 
                    this.PatternSize.X;
                set => 
                    (this.PatternSize.X = value);
            }

            [XmlAttribute]
            public int PatternHeight
            {
                get => 
                    this.PatternSize.Y;
                set => 
                    (this.PatternSize.Y = value);
            }
        }

        [ProtoContract]
        public class Variant
        {
            [XmlAttribute, ProtoMember(0x10d)]
            public string Color;
            [XmlAttribute, ProtoMember(0x110)]
            public string Suffix;
        }
    }
}

