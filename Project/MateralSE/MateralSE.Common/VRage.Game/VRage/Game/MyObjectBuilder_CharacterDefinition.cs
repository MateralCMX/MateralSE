namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using VRage.Data;
    using VRage.ObjectBuilders;
    using VRageMath;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_CharacterDefinition : MyObjectBuilder_DefinitionBase
    {
        private static readonly Vector3 DefaultLightOffset = new Vector3(0f, 0f, -0.5f);
        [ProtoMember(0x87)]
        public string Name;
        [ProtoMember(0x8a), ModdableContentFile("mwm")]
        public string Model;
        [ProtoMember(0x8e), ModdableContentFile("dds")]
        public string ReflectorTexture = @"Textures\Lights\reflector.dds";
        [ProtoMember(0x92)]
        public string LeftGlare;
        [ProtoMember(0x95)]
        public string RightGlare;
        [ProtoMember(0x98)]
        public string Skeleton = "Humanoid";
        [ProtoMember(0x9b)]
        public float LightGlareSize = 0.02f;
        [ProtoMember(0x9e)]
        public MyObjectBuilder_JetpackDefinition Jetpack;
        [ProtoMember(0xa1), XmlArrayItem("Resource")]
        public List<SuitResourceDefinition> SuitResourceStorage;
        [ProtoMember(0xa5), XmlArrayItem("BoneSet")]
        public MyBoneSetDefinition[] BoneSets;
        [ProtoMember(0xa8), XmlArrayItem("BoneSet")]
        public MyBoneSetDefinition[] BoneLODs;
        [ProtoMember(0xab)]
        public string LeftLightBone;
        [ProtoMember(0xae)]
        public string RightLightBone;
        [ProtoMember(0xb1)]
        public Vector3 LightOffset = DefaultLightOffset;
        [ProtoMember(180)]
        public string HeadBone;
        [ProtoMember(0xb7)]
        public string LeftHandIKStartBone;
        [ProtoMember(0xba)]
        public string LeftHandIKEndBone;
        [ProtoMember(0xbd)]
        public string RightHandIKStartBone;
        [ProtoMember(0xc0)]
        public string RightHandIKEndBone;
        [ProtoMember(0xc3)]
        public string WeaponBone;
        [ProtoMember(0xc6)]
        public string Camera3rdBone;
        [ProtoMember(0xc9)]
        public string LeftHandItemBone;
        [ProtoMember(0xcc)]
        public string LeftForearmBone;
        [ProtoMember(0xcf)]
        public string LeftUpperarmBone;
        [ProtoMember(210)]
        public string RightForearmBone;
        [ProtoMember(0xd5)]
        public string RightUpperarmBone;
        [ProtoMember(0xd8)]
        public string SpineBone;
        [ProtoMember(0xdb)]
        public float BendMultiplier1st = 1f;
        [ProtoMember(0xde)]
        public float BendMultiplier3rd = 1f;
        [ProtoMember(0xe1), XmlArrayItem("Material")]
        public string[] MaterialsDisabledIn1st;
        [ProtoMember(0xe5), XmlArrayItem("Mapping")]
        public MyMovementAnimationMapping[] AnimationMappings;
        [ProtoMember(0xe8)]
        public float Mass = 100f;
        [ProtoMember(0xeb)]
        public string ModelRootBoneName;
        [ProtoMember(0xee)]
        public string LeftHipBoneName;
        [ProtoMember(0xf1)]
        public string LeftKneeBoneName;
        [ProtoMember(0xf4)]
        public string LeftAnkleBoneName;
        [ProtoMember(0xf7)]
        public string RightHipBoneName;
        [ProtoMember(250)]
        public string RightKneeBoneName;
        [ProtoMember(0xfd)]
        public string RightAnkleBoneName;
        [ProtoMember(0x100)]
        public bool FeetIKEnabled;
        [ProtoMember(0x103), XmlArrayItem("FeetIKSettings")]
        public MyObjectBuilder_MyFeetIKSettings[] IKSettings;
        [ProtoMember(0x106)]
        public string RightHandItemBone;
        [ProtoMember(0x109)]
        public bool UsesAtmosphereDetector;
        [ProtoMember(0x10c)]
        public bool UsesReverbDetector;
        [ProtoMember(0x10f)]
        public bool NeedsOxygen;
        [ProtoMember(0x112)]
        public string RagdollDataFile;
        [ProtoMember(0x115), XmlArrayItem("BoneSet")]
        public MyRagdollBoneSetDefinition[] RagdollBonesMappings;
        [ProtoMember(280), XmlArrayItem("BoneSet")]
        public MyBoneSetDefinition[] RagdollPartialSimulations;
        [ProtoMember(0x11b)]
        public float OxygenConsumptionMultiplier = 1f;
        [ProtoMember(0x11e)]
        public float OxygenConsumption = 10f;
        [ProtoMember(0x121)]
        public float PressureLevelForLowDamage = 0.5f;
        [ProtoMember(0x124)]
        public float DamageAmountAtZeroPressure = 7f;
        [ProtoMember(0x128)]
        public bool VerticalPositionFlyingOnly;
        [ProtoMember(0x12a)]
        public bool UseOnlyWalking = true;
        [ProtoMember(0x12d)]
        public float MaxSlope = 60f;
        [ProtoMember(0x12f)]
        public float MaxSprintSpeed = 11f;
        [ProtoMember(0x132)]
        public float MaxRunSpeed = 11f;
        [ProtoMember(0x134)]
        public float MaxBackrunSpeed = 11f;
        [ProtoMember(310)]
        public float MaxRunStrafingSpeed = 11f;
        [ProtoMember(0x139)]
        public float MaxWalkSpeed = 6f;
        [ProtoMember(0x13b)]
        public float MaxBackwalkSpeed = 6f;
        [ProtoMember(0x13d)]
        public float MaxWalkStrafingSpeed = 6f;
        [ProtoMember(320)]
        public float MaxCrouchWalkSpeed = 4f;
        [ProtoMember(0x142)]
        public float MaxCrouchBackwalkSpeed = 4f;
        [ProtoMember(0x144)]
        public float MaxCrouchStrafingSpeed = 4f;
        [ProtoMember(0x147)]
        public float CharacterHeadSize = 0.55f;
        [ProtoMember(0x149)]
        public float CharacterHeadHeight = 0.25f;
        [ProtoMember(0x14b)]
        public float CharacterCollisionScale = 1f;
        [ProtoMember(0x14d)]
        public float CharacterCollisionWidth = 1f;
        [ProtoMember(0x14f)]
        public float CharacterCollisionHeight = 1.8f;
        [ProtoMember(0x151)]
        public float CharacterCollisionCrouchHeight = 1.25f;
        [ProtoMember(340)]
        public bool CanCrouch = true;
        [ProtoMember(0x156)]
        public bool CanIronsight = true;
        [ProtoMember(0x15d)]
        public string JumpSoundName = "";
        [ProtoMember(0x15f)]
        public float JumpForce = 2.5f;
        [ProtoMember(0x162)]
        public string JetpackIdleSoundName = "";
        [ProtoMember(0x164)]
        public string JetpackRunSoundName = "";
        [ProtoMember(0x167)]
        public string CrouchDownSoundName = "";
        [ProtoMember(0x169)]
        public string CrouchUpSoundName = "";
        [ProtoMember(0x16b)]
        public string MovementSoundName = "";
        [ProtoMember(0x16e)]
        public string PainSoundName = "";
        [ProtoMember(0x170)]
        public string SuffocateSoundName = "";
        [ProtoMember(370)]
        public string DeathSoundName = "";
        [ProtoMember(0x174)]
        public string DeathBySuffocationSoundName = "";
        [ProtoMember(0x177)]
        public string IronsightActSoundName = "";
        [ProtoMember(0x179)]
        public string IronsightDeactSoundName = "";
        [ProtoMember(0x17b)]
        public string FastFlySoundName = "";
        [ProtoMember(0x17e)]
        public string HelmetOxygenNormalSoundName = "";
        [ProtoMember(0x180)]
        public string HelmetOxygenLowSoundName = "";
        [ProtoMember(0x182)]
        public string HelmetOxygenCriticalSoundName = "";
        [ProtoMember(0x184)]
        public string HelmetOxygenNoneSoundName = "";
        [ProtoMember(0x187, IsRequired=false)]
        public string MagnetBootsStartSoundName = "";
        [ProtoMember(0x18a, IsRequired=false)]
        public string MagnetBootsEndSoundName = "";
        [ProtoMember(0x18d, IsRequired=false)]
        public string MagnetBootsStepsSoundName = "";
        [ProtoMember(400, IsRequired=false)]
        public string MagnetBootsProximitySoundName = "";
        [ProtoMember(0x193)]
        public bool LoopingFootsteps;
        [ProtoMember(0x196)]
        public bool VisibleOnHud = true;
        [ProtoMember(0x199)]
        public bool UsableByPlayer = true;
        [ProtoMember(0x19c)]
        public string RagdollRootBody = string.Empty;
        [ProtoMember(0x19f), DefaultValue((string) null)]
        public MyObjectBuilder_InventoryDefinition Inventory;
        [ProtoMember(0x1a2)]
        public string EnabledComponents;
        [ProtoMember(0x1a5)]
        public bool EnableSpawnInventoryAsContainer;
        [ProtoMember(0x1a8), DefaultValue((string) null)]
        public SerializableDefinitionId? InventorySpawnContainerId;
        [ProtoMember(0x1ab)]
        public bool SpawnInventoryOnBodyRemoval;
        [ProtoMember(430)]
        public float LootingTime = 300f;
        [ProtoMember(0x1b1)]
        public string InitialAnimation = "Idle";
        [ProtoMember(0x1b4)]
        public float ImpulseLimit = float.PositiveInfinity;
        [ProtoMember(0x1ba)]
        public string PhysicalMaterial = "Character";
        [ProtoMember(0x1c0)]
        public MyObjectBuilder_DeadBodyShape DeadBodyShape;
        [ProtoMember(0x1c6)]
        public string AnimationController;
        [ProtoMember(0x1c9)]
        public float? MaxForce;
        [ProtoMember(0x1ce)]
        public MyEnumCharacterRotationToSupport RotationToSupport;
        [ProtoMember(0x1d1)]
        public string HUD;
        [ProtoMember(0x1d4), DefaultValue(true)]
        public bool EnableFirstPersonView = true;
        [ProtoMember(0x1d7)]
        public string BreathCalmSoundName = "";
        [ProtoMember(0x1da)]
        public string BreathHeavySoundName = "";
        [ProtoMember(0x1dd)]
        public string OxygenChokeNormalSoundName = "";
        [ProtoMember(480)]
        public string OxygenChokeLowSoundName = "";
        [ProtoMember(0x1e3)]
        public string OxygenChokeCriticalSoundName = "";
        [ProtoMember(0x1e6), DefaultValue(0)]
        public float OxygenSuitRefillTime;
        [ProtoMember(0x1e9), DefaultValue((float) 0.75f)]
        public float MinOxygenLevelForSuitRefill = 0.75f;
        [ProtoMember(0x1ec), DefaultValue(3)]
        public float SuitConsumptionInTemperatureExtreme = 3f;
    }
}

