namespace Sandbox.Definitions
{
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_CharacterDefinition), (Type) null)]
    public class MyCharacterDefinition : MyDefinitionBase
    {
        public string Name;
        public string Model;
        public string ReflectorTexture;
        public string LeftGlare;
        public string RightGlare;
        public string LeftLightBone;
        public string RightLightBone;
        public Vector3 LightOffset;
        public float LightGlareSize;
        public string HeadBone;
        public string Camera3rdBone;
        public string LeftHandIKStartBone;
        public string LeftHandIKEndBone;
        public string RightHandIKStartBone;
        public string RightHandIKEndBone;
        public string WeaponBone;
        public string LeftHandItemBone;
        public string Skeleton;
        public string LeftForearmBone;
        public string LeftUpperarmBone;
        public string RightForearmBone;
        public string RightUpperarmBone;
        public string SpineBone;
        public float BendMultiplier1st;
        public float BendMultiplier3rd;
        public bool UsesAtmosphereDetector;
        public bool UsesReverbDetector;
        [Obsolete("Dont ever use again.")]
        public bool NeedsOxygen;
        public float OxygenConsumptionMultiplier;
        public float OxygenConsumption;
        public float OxygenSuitRefillTime;
        public float MinOxygenLevelForSuitRefill;
        public float PressureLevelForLowDamage;
        public float DamageAmountAtZeroPressure;
        public bool LoopingFootsteps;
        public bool VisibleOnHud;
        public bool UsableByPlayer;
        public string PhysicalMaterial;
        public float JumpForce;
        public bool EnableFirstPersonView;
        public string JumpSoundName;
        public string JetpackIdleSoundName;
        public string JetpackRunSoundName;
        public string CrouchDownSoundName;
        public string CrouchUpSoundName;
        public string PainSoundName;
        public string SuffocateSoundName;
        public string DeathSoundName;
        public string DeathBySuffocationSoundName;
        public string IronsightActSoundName;
        public string IronsightDeactSoundName;
        public string FastFlySoundName;
        public string HelmetOxygenNormalSoundName;
        public string HelmetOxygenLowSoundName;
        public string HelmetOxygenCriticalSoundName;
        public string HelmetOxygenNoneSoundName;
        public string MovementSoundName;
        public string MagnetBootsStartSoundName;
        public string MagnetBootsEndSoundName;
        public string MagnetBootsStepsSoundName;
        public string MagnetBootsProximitySoundName;
        public string BreathCalmSoundName;
        public string BreathHeavySoundName;
        public string OxygenChokeNormalSoundName;
        public string OxygenChokeLowSoundName;
        public string OxygenChokeCriticalSoundName;
        public bool FeetIKEnabled;
        public string ModelRootBoneName;
        public string LeftHipBoneName;
        public string LeftKneeBoneName;
        public string LeftAnkleBoneName;
        public string RightHipBoneName;
        public string RightKneeBoneName;
        public string RightAnkleBoneName;
        public string RagdollDataFile;
        public Dictionary<string, RagdollBoneSet> RagdollBonesMappings = new Dictionary<string, RagdollBoneSet>();
        public Dictionary<string, string[]> RagdollPartialSimulations = new Dictionary<string, string[]>();
        public string RagdollRootBody;
        public Dictionary<MyCharacterMovementEnum, MyFeetIKSettings> FeetIKSettings;
        public List<SuitResourceDefinition> SuitResourceStorage;
        public MyObjectBuilder_JetpackDefinition Jetpack;
        public Dictionary<string, string[]> BoneSets = new Dictionary<string, string[]>();
        public Dictionary<float, string[]> BoneLODs = new Dictionary<float, string[]>();
        public Dictionary<string, string> AnimationNameToSubtypeName = new Dictionary<string, string>();
        public string[] MaterialsDisabledIn1st;
        public float Mass;
        public float ImpulseLimit;
        public string RighHandItemBone;
        public bool VerticalPositionFlyingOnly;
        public bool UseOnlyWalking;
        public float MaxSlope;
        public float MaxSprintSpeed;
        public float MaxRunSpeed;
        public float MaxBackrunSpeed;
        public float MaxRunStrafingSpeed;
        public float MaxWalkSpeed;
        public float MaxBackwalkSpeed;
        public float MaxWalkStrafingSpeed;
        public float MaxCrouchWalkSpeed;
        public float MaxCrouchBackwalkSpeed;
        public float MaxCrouchStrafingSpeed;
        public float CharacterHeadSize;
        public float CharacterHeadHeight;
        public float CharacterCollisionScale;
        public float CharacterCollisionHeight;
        public float CharacterCollisionWidth;
        public float CharacterCollisionCrouchHeight;
        public float CharacterWidth;
        public float CharacterHeight;
        public float CharacterLength;
        public bool CanCrouch;
        public bool CanIronsight;
        public MyObjectBuilder_InventoryDefinition InventoryDefinition;
        public bool EnableSpawnInventoryAsContainer;
        public MyDefinitionId? InventorySpawnContainerId;
        public bool SpawnInventoryOnBodyRemoval;
        [Obsolete("Use MyComponentDefinitionBase and MyContainerDefinition to define enabled types of components on entities")]
        public List<string> EnabledComponents = new List<string>();
        public float LootingTime;
        public string InitialAnimation;
        public MyObjectBuilder_DeadBodyShape DeadBodyShape;
        public string AnimationController;
        public float? MaxForce;
        public MyEnumCharacterRotationToSupport RotationToSupport;
        public string HUD;
        public float SuitConsumptionInTemperatureExtreme;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_CharacterDefinition objectBuilder = (MyObjectBuilder_CharacterDefinition) base.GetObjectBuilder();
            objectBuilder.Name = this.Name;
            objectBuilder.Model = this.Model;
            objectBuilder.ReflectorTexture = this.ReflectorTexture;
            objectBuilder.LeftGlare = this.LeftGlare;
            objectBuilder.RightGlare = this.RightGlare;
            objectBuilder.LightGlareSize = this.LightGlareSize;
            objectBuilder.Skeleton = this.Skeleton;
            objectBuilder.LeftForearmBone = this.LeftForearmBone;
            objectBuilder.LeftUpperarmBone = this.LeftUpperarmBone;
            objectBuilder.RightForearmBone = this.RightForearmBone;
            objectBuilder.RightUpperarmBone = this.RightUpperarmBone;
            objectBuilder.SpineBone = this.SpineBone;
            objectBuilder.MaterialsDisabledIn1st = this.MaterialsDisabledIn1st;
            objectBuilder.UsesAtmosphereDetector = this.UsesAtmosphereDetector;
            objectBuilder.UsesReverbDetector = this.UsesReverbDetector;
            objectBuilder.NeedsOxygen = this.NeedsOxygen;
            objectBuilder.OxygenConsumptionMultiplier = this.OxygenConsumptionMultiplier;
            objectBuilder.OxygenConsumption = this.OxygenConsumption;
            objectBuilder.OxygenSuitRefillTime = this.OxygenSuitRefillTime;
            objectBuilder.MinOxygenLevelForSuitRefill = this.MinOxygenLevelForSuitRefill;
            objectBuilder.PressureLevelForLowDamage = this.PressureLevelForLowDamage;
            objectBuilder.DamageAmountAtZeroPressure = this.DamageAmountAtZeroPressure;
            objectBuilder.JumpSoundName = this.JumpSoundName;
            objectBuilder.JetpackIdleSoundName = this.JetpackIdleSoundName;
            objectBuilder.JetpackRunSoundName = this.JetpackRunSoundName;
            objectBuilder.CrouchDownSoundName = this.CrouchDownSoundName;
            objectBuilder.CrouchUpSoundName = this.CrouchUpSoundName;
            objectBuilder.SuffocateSoundName = this.SuffocateSoundName;
            objectBuilder.PainSoundName = this.PainSoundName;
            objectBuilder.DeathSoundName = this.DeathSoundName;
            objectBuilder.DeathBySuffocationSoundName = this.DeathBySuffocationSoundName;
            objectBuilder.IronsightActSoundName = this.IronsightActSoundName;
            objectBuilder.IronsightDeactSoundName = this.IronsightDeactSoundName;
            objectBuilder.LoopingFootsteps = this.LoopingFootsteps;
            objectBuilder.MagnetBootsStartSoundName = this.MagnetBootsStartSoundName;
            objectBuilder.MagnetBootsEndSoundName = this.MagnetBootsEndSoundName;
            objectBuilder.MagnetBootsStepsSoundName = this.MagnetBootsStepsSoundName;
            objectBuilder.MagnetBootsProximitySoundName = this.MagnetBootsProximitySoundName;
            objectBuilder.VisibleOnHud = this.VisibleOnHud;
            objectBuilder.UsableByPlayer = this.UsableByPlayer;
            objectBuilder.SuitResourceStorage = this.SuitResourceStorage;
            objectBuilder.Jetpack = this.Jetpack;
            objectBuilder.VerticalPositionFlyingOnly = this.VerticalPositionFlyingOnly;
            objectBuilder.UseOnlyWalking = this.UseOnlyWalking;
            objectBuilder.MaxSlope = this.MaxSlope;
            objectBuilder.MaxSprintSpeed = this.MaxSprintSpeed;
            objectBuilder.MaxRunSpeed = this.MaxRunSpeed;
            objectBuilder.MaxBackrunSpeed = this.MaxBackrunSpeed;
            objectBuilder.MaxRunStrafingSpeed = this.MaxRunStrafingSpeed;
            objectBuilder.MaxWalkSpeed = this.MaxWalkSpeed;
            objectBuilder.MaxBackwalkSpeed = this.MaxBackwalkSpeed;
            objectBuilder.MaxWalkStrafingSpeed = this.MaxWalkStrafingSpeed;
            objectBuilder.MaxCrouchWalkSpeed = this.MaxCrouchWalkSpeed;
            objectBuilder.MaxCrouchBackwalkSpeed = this.MaxCrouchBackwalkSpeed;
            objectBuilder.MaxCrouchStrafingSpeed = this.MaxCrouchStrafingSpeed;
            objectBuilder.CharacterHeadSize = this.CharacterHeadSize;
            objectBuilder.CharacterHeadHeight = this.CharacterHeadHeight;
            objectBuilder.CharacterCollisionScale = this.CharacterCollisionScale;
            objectBuilder.CharacterCollisionWidth = this.CharacterCollisionWidth;
            objectBuilder.CharacterCollisionHeight = this.CharacterCollisionHeight;
            objectBuilder.CharacterCollisionCrouchHeight = this.CharacterCollisionCrouchHeight;
            objectBuilder.CanCrouch = this.CanCrouch;
            objectBuilder.CanIronsight = this.CanIronsight;
            objectBuilder.Inventory = this.InventoryDefinition;
            objectBuilder.PhysicalMaterial = this.PhysicalMaterial;
            objectBuilder.EnabledComponents = string.Join(" ", this.EnabledComponents);
            objectBuilder.EnableSpawnInventoryAsContainer = this.EnableSpawnInventoryAsContainer;
            if (this.EnableSpawnInventoryAsContainer)
            {
                if (this.InventorySpawnContainerId != null)
                {
                    objectBuilder.InventorySpawnContainerId = new SerializableDefinitionId?(this.InventorySpawnContainerId.Value);
                }
                objectBuilder.SpawnInventoryOnBodyRemoval = this.SpawnInventoryOnBodyRemoval;
            }
            objectBuilder.LootingTime = this.LootingTime;
            objectBuilder.DeadBodyShape = this.DeadBodyShape;
            objectBuilder.AnimationController = this.AnimationController;
            objectBuilder.MaxForce = this.MaxForce;
            objectBuilder.RotationToSupport = this.RotationToSupport;
            objectBuilder.BreathCalmSoundName = this.BreathCalmSoundName;
            objectBuilder.BreathHeavySoundName = this.BreathHeavySoundName;
            objectBuilder.OxygenChokeNormalSoundName = this.OxygenChokeNormalSoundName;
            objectBuilder.OxygenChokeLowSoundName = this.OxygenChokeLowSoundName;
            objectBuilder.OxygenChokeCriticalSoundName = this.OxygenChokeCriticalSoundName;
            objectBuilder.SuitConsumptionInTemperatureExtreme = this.SuitConsumptionInTemperatureExtreme;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_CharacterDefinition definition = (MyObjectBuilder_CharacterDefinition) objectBuilder;
            this.Name = definition.Name;
            this.Model = definition.Model;
            this.ReflectorTexture = definition.ReflectorTexture;
            this.LeftGlare = definition.LeftGlare;
            this.RightGlare = definition.RightGlare;
            this.LeftLightBone = definition.LeftLightBone;
            this.RightLightBone = definition.RightLightBone;
            this.LightOffset = definition.LightOffset;
            this.LightGlareSize = definition.LightGlareSize;
            this.HeadBone = definition.HeadBone;
            this.Camera3rdBone = definition.Camera3rdBone;
            this.LeftHandIKStartBone = definition.LeftHandIKStartBone;
            this.LeftHandIKEndBone = definition.LeftHandIKEndBone;
            this.RightHandIKStartBone = definition.RightHandIKStartBone;
            this.RightHandIKEndBone = definition.RightHandIKEndBone;
            this.WeaponBone = definition.WeaponBone;
            this.LeftHandItemBone = definition.LeftHandItemBone;
            this.RighHandItemBone = definition.RightHandItemBone;
            this.Skeleton = definition.Skeleton;
            this.LeftForearmBone = definition.LeftForearmBone;
            this.LeftUpperarmBone = definition.LeftUpperarmBone;
            this.RightForearmBone = definition.RightForearmBone;
            this.RightUpperarmBone = definition.RightUpperarmBone;
            this.SpineBone = definition.SpineBone;
            this.BendMultiplier1st = definition.BendMultiplier1st;
            this.BendMultiplier3rd = definition.BendMultiplier3rd;
            this.MaterialsDisabledIn1st = definition.MaterialsDisabledIn1st;
            this.FeetIKEnabled = definition.FeetIKEnabled;
            this.ModelRootBoneName = definition.ModelRootBoneName;
            this.LeftHipBoneName = definition.LeftHipBoneName;
            this.LeftKneeBoneName = definition.LeftKneeBoneName;
            this.LeftAnkleBoneName = definition.LeftAnkleBoneName;
            this.RightHipBoneName = definition.RightHipBoneName;
            this.RightKneeBoneName = definition.RightKneeBoneName;
            this.RightAnkleBoneName = definition.RightAnkleBoneName;
            this.UsesAtmosphereDetector = definition.UsesAtmosphereDetector;
            this.UsesReverbDetector = definition.UsesReverbDetector;
            this.NeedsOxygen = definition.NeedsOxygen;
            this.OxygenConsumptionMultiplier = definition.OxygenConsumptionMultiplier;
            this.OxygenConsumption = definition.OxygenConsumption;
            this.OxygenSuitRefillTime = definition.OxygenSuitRefillTime;
            this.MinOxygenLevelForSuitRefill = definition.MinOxygenLevelForSuitRefill;
            this.PressureLevelForLowDamage = definition.PressureLevelForLowDamage;
            this.DamageAmountAtZeroPressure = definition.DamageAmountAtZeroPressure;
            this.RagdollDataFile = definition.RagdollDataFile;
            this.JumpSoundName = definition.JumpSoundName;
            this.JetpackIdleSoundName = definition.JetpackIdleSoundName;
            this.JetpackRunSoundName = definition.JetpackRunSoundName;
            this.CrouchDownSoundName = definition.CrouchDownSoundName;
            this.CrouchUpSoundName = definition.CrouchUpSoundName;
            this.PainSoundName = definition.PainSoundName;
            this.SuffocateSoundName = definition.SuffocateSoundName;
            this.DeathSoundName = definition.DeathSoundName;
            this.DeathBySuffocationSoundName = definition.DeathBySuffocationSoundName;
            this.IronsightActSoundName = definition.IronsightActSoundName;
            this.IronsightDeactSoundName = definition.IronsightDeactSoundName;
            this.FastFlySoundName = definition.FastFlySoundName;
            this.HelmetOxygenNormalSoundName = definition.HelmetOxygenNormalSoundName;
            this.HelmetOxygenLowSoundName = definition.HelmetOxygenLowSoundName;
            this.HelmetOxygenCriticalSoundName = definition.HelmetOxygenCriticalSoundName;
            this.HelmetOxygenNoneSoundName = definition.HelmetOxygenNoneSoundName;
            this.MovementSoundName = definition.MovementSoundName;
            this.MagnetBootsStartSoundName = definition.MagnetBootsStartSoundName;
            this.MagnetBootsStepsSoundName = definition.MagnetBootsStepsSoundName;
            this.MagnetBootsEndSoundName = definition.MagnetBootsEndSoundName;
            this.MagnetBootsProximitySoundName = definition.MagnetBootsProximitySoundName;
            this.LoopingFootsteps = definition.LoopingFootsteps;
            this.VisibleOnHud = definition.VisibleOnHud;
            this.UsableByPlayer = definition.UsableByPlayer;
            this.RagdollRootBody = definition.RagdollRootBody;
            this.InitialAnimation = definition.InitialAnimation;
            this.PhysicalMaterial = definition.PhysicalMaterial;
            this.JumpForce = definition.JumpForce;
            this.RotationToSupport = definition.RotationToSupport;
            this.HUD = definition.HUD;
            this.EnableFirstPersonView = definition.EnableFirstPersonView;
            this.BreathCalmSoundName = definition.BreathCalmSoundName;
            this.BreathHeavySoundName = definition.BreathHeavySoundName;
            this.OxygenChokeNormalSoundName = definition.OxygenChokeNormalSoundName;
            this.OxygenChokeLowSoundName = definition.OxygenChokeLowSoundName;
            this.OxygenChokeCriticalSoundName = definition.OxygenChokeCriticalSoundName;
            this.FeetIKSettings = new Dictionary<MyCharacterMovementEnum, MyFeetIKSettings>();
            if (definition.IKSettings != null)
            {
                MyObjectBuilder_MyFeetIKSettings[] iKSettings = definition.IKSettings;
                int index = 0;
                while (index < iKSettings.Length)
                {
                    MyObjectBuilder_MyFeetIKSettings settings = iKSettings[index];
                    char[] separator = new char[] { ',' };
                    string[] strArray = settings.MovementState.Split(separator);
                    int num2 = 0;
                    while (true)
                    {
                        MyCharacterMovementEnum enum2;
                        if (num2 >= strArray.Length)
                        {
                            index++;
                            break;
                        }
                        string str = strArray[num2].Trim();
                        if ((str != "") && Enum.TryParse<MyCharacterMovementEnum>(str, true, out enum2))
                        {
                            MyFeetIKSettings settings2 = new MyFeetIKSettings {
                                Enabled = settings.Enabled,
                                AboveReachableDistance = settings.AboveReachableDistance,
                                BelowReachableDistance = settings.BelowReachableDistance,
                                VerticalShiftDownGain = settings.VerticalShiftDownGain,
                                VerticalShiftUpGain = settings.VerticalShiftUpGain,
                                FootSize = new Vector3(settings.FootWidth, settings.AnkleHeight, settings.FootLenght)
                            };
                            this.FeetIKSettings.Add(enum2, settings2);
                        }
                        num2++;
                    }
                }
            }
            this.SuitResourceStorage = definition.SuitResourceStorage;
            this.Jetpack = definition.Jetpack;
            if (definition.BoneSets != null)
            {
                this.BoneSets = definition.BoneSets.ToDictionary<MyBoneSetDefinition, string, string[]>(x => x.Name, x => x.Bones.Split(new char[] { ' ' }));
            }
            if (definition.BoneLODs != null)
            {
                this.BoneLODs = definition.BoneLODs.ToDictionary<MyBoneSetDefinition, float, string[]>(x => Convert.ToSingle(x.Name), x => x.Bones.Split(new char[] { ' ' }));
            }
            if (definition.AnimationMappings != null)
            {
                this.AnimationNameToSubtypeName = definition.AnimationMappings.ToDictionary<MyMovementAnimationMapping, string, string>(mapping => mapping.Name, mapping => mapping.AnimationSubtypeName);
            }
            if (definition.RagdollBonesMappings != null)
            {
                this.RagdollBonesMappings = definition.RagdollBonesMappings.ToDictionary<MyRagdollBoneSetDefinition, string, RagdollBoneSet>(x => x.Name, x => new RagdollBoneSet(x.Bones, x.CollisionRadius));
            }
            if (definition.RagdollPartialSimulations != null)
            {
                this.RagdollPartialSimulations = definition.RagdollPartialSimulations.ToDictionary<MyBoneSetDefinition, string, string[]>(x => x.Name, x => x.Bones.Split(new char[] { ' ' }));
            }
            this.Mass = definition.Mass;
            this.ImpulseLimit = definition.ImpulseLimit;
            this.VerticalPositionFlyingOnly = definition.VerticalPositionFlyingOnly;
            this.UseOnlyWalking = definition.UseOnlyWalking;
            this.MaxSlope = definition.MaxSlope;
            this.MaxSprintSpeed = definition.MaxSprintSpeed;
            this.MaxRunSpeed = definition.MaxRunSpeed;
            this.MaxBackrunSpeed = definition.MaxBackrunSpeed;
            this.MaxRunStrafingSpeed = definition.MaxRunStrafingSpeed;
            this.MaxWalkSpeed = definition.MaxWalkSpeed;
            this.MaxBackwalkSpeed = definition.MaxBackwalkSpeed;
            this.MaxWalkStrafingSpeed = definition.MaxWalkStrafingSpeed;
            this.MaxCrouchWalkSpeed = definition.MaxCrouchWalkSpeed;
            this.MaxCrouchBackwalkSpeed = definition.MaxCrouchBackwalkSpeed;
            this.MaxCrouchStrafingSpeed = definition.MaxCrouchStrafingSpeed;
            this.CharacterHeadSize = definition.CharacterHeadSize;
            this.CharacterHeadHeight = definition.CharacterHeadHeight;
            this.CharacterCollisionScale = definition.CharacterCollisionScale;
            this.CharacterCollisionWidth = definition.CharacterCollisionWidth;
            this.CharacterCollisionHeight = definition.CharacterCollisionHeight;
            this.CharacterCollisionCrouchHeight = definition.CharacterCollisionCrouchHeight;
            this.CanCrouch = definition.CanCrouch;
            this.CanIronsight = definition.CanIronsight;
            this.InventoryDefinition = (definition.Inventory != null) ? definition.Inventory : new MyObjectBuilder_InventoryDefinition();
            if (definition.EnabledComponents != null)
            {
                char[] separator = new char[] { ' ' };
                this.EnabledComponents = definition.EnabledComponents.Split(separator).ToList<string>();
            }
            this.EnableSpawnInventoryAsContainer = definition.EnableSpawnInventoryAsContainer;
            if (this.EnableSpawnInventoryAsContainer)
            {
                if (definition.InventorySpawnContainerId != null)
                {
                    this.InventorySpawnContainerId = new MyDefinitionId?(definition.InventorySpawnContainerId.Value);
                }
                this.SpawnInventoryOnBodyRemoval = definition.SpawnInventoryOnBodyRemoval;
            }
            this.LootingTime = definition.LootingTime;
            this.DeadBodyShape = definition.DeadBodyShape;
            this.AnimationController = definition.AnimationController;
            this.MaxForce = definition.MaxForce;
            this.SuitConsumptionInTemperatureExtreme = definition.SuitConsumptionInTemperatureExtreme;
        }

        public bool UseNewAnimationSystem =>
            (this.AnimationController != null);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCharacterDefinition.<>c <>9 = new MyCharacterDefinition.<>c();
            public static Func<MyBoneSetDefinition, string> <>9__128_0;
            public static Func<MyBoneSetDefinition, string[]> <>9__128_1;
            public static Func<MyBoneSetDefinition, float> <>9__128_2;
            public static Func<MyBoneSetDefinition, string[]> <>9__128_3;
            public static Func<MyMovementAnimationMapping, string> <>9__128_4;
            public static Func<MyMovementAnimationMapping, string> <>9__128_5;
            public static Func<MyRagdollBoneSetDefinition, string> <>9__128_6;
            public static Func<MyRagdollBoneSetDefinition, MyCharacterDefinition.RagdollBoneSet> <>9__128_7;
            public static Func<MyBoneSetDefinition, string> <>9__128_8;
            public static Func<MyBoneSetDefinition, string[]> <>9__128_9;

            internal string <Init>b__128_0(MyBoneSetDefinition x) => 
                x.Name;

            internal string[] <Init>b__128_1(MyBoneSetDefinition x)
            {
                char[] separator = new char[] { ' ' };
                return x.Bones.Split(separator);
            }

            internal float <Init>b__128_2(MyBoneSetDefinition x) => 
                Convert.ToSingle(x.Name);

            internal string[] <Init>b__128_3(MyBoneSetDefinition x)
            {
                char[] separator = new char[] { ' ' };
                return x.Bones.Split(separator);
            }

            internal string <Init>b__128_4(MyMovementAnimationMapping mapping) => 
                mapping.Name;

            internal string <Init>b__128_5(MyMovementAnimationMapping mapping) => 
                mapping.AnimationSubtypeName;

            internal string <Init>b__128_6(MyRagdollBoneSetDefinition x) => 
                x.Name;

            internal MyCharacterDefinition.RagdollBoneSet <Init>b__128_7(MyRagdollBoneSetDefinition x) => 
                new MyCharacterDefinition.RagdollBoneSet(x.Bones, x.CollisionRadius);

            internal string <Init>b__128_8(MyBoneSetDefinition x) => 
                x.Name;

            internal string[] <Init>b__128_9(MyBoneSetDefinition x)
            {
                char[] separator = new char[] { ' ' };
                return x.Bones.Split(separator);
            }
        }

        public class RagdollBoneSet
        {
            public string[] Bones;
            public float CollisionRadius;

            public RagdollBoneSet(string bones, float radius)
            {
                char[] separator = new char[] { ' ' };
                this.Bones = bones.Split(separator);
                this.CollisionRadius = radius;
            }
        }
    }
}

