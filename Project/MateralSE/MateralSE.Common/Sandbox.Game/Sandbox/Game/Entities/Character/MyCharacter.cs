namespace Sandbox.Game.Entities.Character
{
    using Havok;
    using Sandbox;
    using Sandbox.Common;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Electricity;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.ClientStates;
    using Sandbox.Game.Screens;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Definitions.Animation;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.Gui;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Game.Utils;
    using VRage.GameServices;
    using VRage.Input;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Animations;
    using VRageRender.Import;

    [MyEntityType(typeof(MyObjectBuilder_Character), true), StaticEventOwner]
    public class MyCharacter : MySkinnedEntity, IMyCameraController, Sandbox.Game.Entities.IMyControllableEntity, VRage.Game.ModAPI.Interfaces.IMyControllableEntity, IMyInventoryOwner, IMyUseObject, IMyDestroyableObject, IMyDecalProxy, IMyCharacter, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyEventProxy, IMyEventOwner, IMyComponentOwner<MyIDModule>, IMySyncedEntity
    {
        private const float LadderSpeed = 2f;
        private const float MinHeadLadderLocalYAngle = -90f;
        private const float MaxHeadLadderLocalYAngle = 90f;
        private float m_stepIncrement;
        private int m_stepsPerAnimation;
        private Vector3 m_ladderIncrementToBase;
        private MatrixD m_baseMatrix;
        private int m_currentLadderStep;
        private MyLadder m_ladder;
        private MyHudNotification m_ladderOffNotification;
        private MyHudNotification m_ladderUpDownNotification;
        private MyHudNotification m_ladderJumpOffNotification;
        private MyHudNotification m_ladderBlockedNotification;
        private long? m_ladderIdInit;
        private HkConstraint m_constraintInstance;
        private HkFixedConstraintData m_constraintData;
        private HkBreakableConstraintData m_constraintBreakableData;
        private bool m_needReconnectLadder;
        private MyCubeGrid m_oldLadderGrid;
        private float m_verticalFootError;
        private float m_cummulativeVerticalFootError;
        private static string TopBody = "LeftHand RightHand LeftFingers RightFingers Head Spine";
        private bool m_resetWeaponAnimationState;
        private Quaternion m_lastRotation;
        private static Dictionary<Vector3D, MyParticleEffect> m_burrowEffectTable = new Dictionary<Vector3D, MyParticleEffect>();
        private readonly Vector3[] m_animationSpeedFilter = new Vector3[4];
        private int m_animationSpeedFilterCursor;
        private int m_wasOnLadder;
        private static List<VRage.Game.Entity.MyEntity> m_supportingEntities;
        private static List<VertexArealBoneIndexWeight> m_boneIndexWeightTmp;
        [ThreadStatic]
        private static MyCharacterHitInfo m_hitInfoTmp;
        public const float MAGIC_COS = 0.996795f;
        public const float CAMERA_NEAR_DISTANCE = 60f;
        internal const float CHARACTER_X_ROTATION_SPEED = 0.13f;
        private const float CHARACTER_Y_ROTATION_FACTOR = 0.02f;
        public const float MINIMAL_SPEED = 0.001f;
        private const float JUMP_DURATION = 0.55f;
        private const float JUMP_TIME = 1f;
        private const float SHOT_TIME = 0.1f;
        private const float FALL_TIME = 0.3f;
        private const float RESPAWN_TIME = 5f;
        internal const float MIN_HEAD_LOCAL_X_ANGLE = -89.9f;
        internal const float MAX_HEAD_LOCAL_X_ANGLE = 89f;
        internal const float MIN_HEAD_LOCAL_Y_ANGLE_ON_LADDER = -89.9f;
        internal const float MAX_HEAD_LOCAL_Y_ANGLE_ON_LADDER = 89f;
        public const int HK_CHARACTER_FLYING = 5;
        private const float AERIAL_CONTROL_FORCE_MULTIPLIER = 0.062f;
        public static float MAX_SHAKE_DAMAGE = 90f;
        [CompilerGenerated]
        private static Action<MyCharacter> OnCharacterDied;
        private float m_currentShotTime;
        private float m_currentShootPositionTime;
        private float m_cameraDistance;
        private float m_currentSpeed;
        private Vector3 m_currentMovementDirection = Vector3.Zero;
        private float m_currentDecceleration;
        private float m_currentJumpTime;
        private float m_frictionBeforeJump = 1.3f;
        private bool m_assetModifiersLoaded;
        private bool m_canJump = true;
        public bool UpdateRotationsOverride;
        private float m_currentWalkDelay;
        private float m_canPlayImpact;
        private static MyStringId m_stringIdHit = MyStringId.GetOrCompute("Hit");
        private MyStringHash m_physicalMaterialHash;
        private long m_deadPlayerIdentityId = -1L;
        private Vector3 m_gravity = Vector3.Zero;
        private bool m_resolveHighlightOverlap;
        public static MyHudNotification OutOfAmmoNotification;
        private int m_weaponBone = -1;
        public float CharacterGeneralDamageModifier = 1f;
        [CompilerGenerated]
        private Action<IMyHandheldGunObject<MyDeviceBase>> WeaponEquiped;
        private bool m_usingByPrimary;
        private float m_headLocalXAngle;
        private float m_headLocalYAngle;
        private float m_previousHeadLocalXAngle;
        private float m_previousHeadLocalYAngle;
        private bool m_headRenderingEnabled = true;
        private readonly VRage.Sync.Sync<MyBootsState, SyncDirection.FromServer> m_bootsState;
        public float RotationSpeed = 0.13f;
        private const double MIN_FORCE_PREDICTION_DURATION = 10.0;
        private bool m_forceDisablePrediction;
        private double m_forceDisablePredictionTime;
        private int m_headBoneIndex = -1;
        private int m_camera3rdBoneIndex = -1;
        private int m_leftHandIKStartBone = -1;
        private int m_leftHandIKEndBone = -1;
        private int m_rightHandIKStartBone = -1;
        private int m_rightHandIKEndBone = -1;
        private int m_leftUpperarmBone = -1;
        private int m_leftForearmBone = -1;
        private int m_rightUpperarmBone = -1;
        private int m_rightForearmBone = -1;
        private int m_leftHandItemBone = -1;
        private int m_rightHandItemBone = -1;
        private int m_spineBone = -1;
        protected bool m_characterBoneCapsulesReady;
        private bool m_animationCommandsEnabled = true;
        private float m_currentAnimationChangeDelay;
        private float SAFE_DELAY_FOR_ANIMATION_BLEND = 0.1f;
        private MyCharacterMovementEnum m_currentMovementState;
        private MyCharacterMovementEnum m_previousMovementState;
        private MyCharacterMovementEnum m_previousNetworkMovementState;
        [CompilerGenerated]
        private CharacterMovementStateDelegate OnMovementStateChanged;
        [CompilerGenerated]
        private CharacterMovementStateChangedDelegate MovementStateChanged;
        private VRage.Game.Entity.MyEntity m_leftHandItem;
        private MyHandItemDefinition m_handItemDefinition;
        private MyZoomModeEnum m_zoomMode;
        private float m_currentHandItemWalkingBlend;
        private float m_currentHandItemShootBlend;
        private CapsuleD[] m_bodyCapsules = new CapsuleD[1];
        private MatrixD m_headMatrix = MatrixD.CreateTranslation(0.0, 1.65, 0.0);
        private MyHudNotification m_pickupObjectNotification;
        private HkCharacterStateType m_currentCharacterState;
        private bool m_isFalling;
        private bool m_isFallingAnimationPlayed;
        private float m_currentFallingTime;
        private bool m_crouchAfterFall;
        private MyCharacterMovementFlags m_movementFlags;
        private MyCharacterMovementFlags m_netMovementFlags;
        private MyCharacterMovementFlags m_previousMovementFlags;
        private bool m_movementsFlagsChanged;
        private string m_characterModel;
        private MyBattery m_suitBattery;
        private MyResourceDistributorComponent m_suitResourceDistributor;
        private float m_outsideTemperature;
        private MyResourceSinkComponent m_sinkComp;
        private VRage.Game.Entity.MyEntity m_topGrid;
        private VRage.Game.Entity.MyEntity m_usingEntity;
        private bool m_enableBag = true;
        public const float REFLECTOR_RANGE = 35f;
        public static float REFLECTOR_DIRECTION = -3.5f;
        public const float REFLECTOR_CONE_ANGLE = 0.373f;
        public const float REFLECTOR_BILLBOARD_LENGTH = 40f;
        public const float REFLECTOR_BILLBOARD_THICKNESS = 6f;
        public static VRageMath.Vector4 REFLECTOR_COLOR = VRageMath.Vector4.One;
        public static float REFLECTOR_FALLOFF = 1f;
        public static float REFLECTOR_GLOSS_FACTOR = 1f;
        public static float REFLECTOR_DIFFUSE_FACTOR = 3.14f;
        public static float REFLECTOR_INTENSITY = 25f;
        public static VRageMath.Vector4 POINT_COLOR = VRageMath.Vector4.One;
        public static float POINT_FALLOFF = 0.3f;
        public static float POINT_GLOSS_FACTOR = 1f;
        public static float POINT_DIFFUSE_FACTOR = 3.14f;
        public static float POINT_LIGHT_INTENSITY = 0.5f;
        public static float POINT_LIGHT_RANGE = 1.08f;
        public static bool LIGHT_PARAMETERS_CHANGED = false;
        public const float LIGHT_GLARE_MAX_DISTANCE_SQR = 1600f;
        private float m_currentLightPower;
        private float m_lightPowerFromProducer;
        private float m_lightTurningOnSpeed = 0.05f;
        private float m_lightTurningOffSpeed = 0.05f;
        private bool m_lightEnabled = true;
        private float m_currentHeadAnimationCounter;
        private float m_currentLocalHeadAnimation = -1f;
        private float m_localHeadAnimationLength = -1f;
        private Vector2? m_localHeadAnimationX;
        private Vector2? m_localHeadAnimationY;
        private List<MyBoneCapsuleInfo> m_bodyCapsuleInfo = new List<MyBoneCapsuleInfo>();
        private HashSet<uint> m_shapeContactPoints = new HashSet<uint>();
        private float m_currentRespawnCounter;
        private MyHudNotification m_respawnNotification;
        private MyHudNotification m_notEnoughStatNotification;
        private MyStringHash manipulationToolId = MyStringHash.GetOrCompute("ManipulationTool");
        private Queue<Vector3> m_bobQueue = new Queue<Vector3>();
        private bool m_dieAfterSimulation;
        private Vector3? m_deathLinearVelocityFromSever;
        private float m_currentLootingCounter;
        private MyEntityCameraSettings m_cameraSettingsWhenAlive;
        private bool m_useAnimationForWeapon = true;
        private long m_relativeDampeningEntityInit;
        private MyCharacterDefinition m_characterDefinition;
        private bool m_isInFirstPersonView = true;
        private bool m_targetFromCamera;
        private bool m_forceFirstPersonCamera;
        [CompilerGenerated]
        private EventHandler OnWeaponChanged;
        [CompilerGenerated]
        private Action<MyCharacter> CharacterDied;
        private bool m_moveAndRotateStopped;
        private bool m_moveAndRotateCalled;
        private readonly VRage.Sync.Sync<int, SyncDirection.FromServer> m_currentAmmoCount;
        private readonly VRage.Sync.Sync<int, SyncDirection.FromServer> m_currentMagazineAmmoCount;
        private readonly VRage.Sync.Sync<MyPlayer.PlayerId, SyncDirection.FromServer> m_controlInfo;
        private MyPlayer.PlayerId? m_savedPlayer;
        private readonly VRage.Sync.Sync<Vector3, SyncDirection.BothWays> m_localHeadPosition;
        private VRage.Sync.Sync<float, SyncDirection.BothWays> m_animLeaning;
        private List<IMyNetworkCommand> m_cachedCommands;
        private Vector3 m_previousLinearVelocity;
        private Vector3D m_previousPosition;
        private bool[] m_isShooting;
        public Vector3 ShootDirection = Vector3.One;
        private long m_lastShootDirectionUpdate;
        private long m_closestParentId;
        private MyIDModule m_idModule = new MyIDModule(0L, MyOwnershipShareModeEnum.Faction);
        internal readonly VRage.Sync.Sync<float, SyncDirection.FromServer> EnvironmentOxygenLevelSync;
        internal readonly VRage.Sync.Sync<float, SyncDirection.FromServer> OxygenLevelAtCharacterLocation;
        internal readonly VRage.Sync.Sync<long, SyncDirection.FromServer> OxygenSourceGridEntityId;
        private static readonly Vector3[] m_defaultColors = new Vector3[] { new Vector3(0f, -1f, 0f), new Vector3(0f, -0.96f, -0.5f), new Vector3(0.575f, 0.15f, 0.2f), new Vector3(0.333f, -0.33f, -0.05f), new Vector3(0f, 0f, 0.05f), new Vector3(0f, -0.8f, 0.6f), new Vector3(0.122f, 0.05f, 0.46f) };
        public static readonly string DefaultModel = "Default_Astronaut";
        private float? m_savedHealth;
        private bool m_wasInFirstPerson;
        private bool m_isInFirstPerson;
        private bool m_wasInThirdPersonBeforeIronSight;
        private List<HkBodyCollision> m_physicsCollisionResults;
        private List<VRage.Game.Entity.MyEntity> m_supportedEntitiesTmp = new List<VRage.Game.Entity.MyEntity>();
        private Vector3D m_crosshairPoint;
        private Vector3D m_aimedPoint;
        private List<HkBodyCollision> m_penetrationList = new List<HkBodyCollision>();
        private List<MyPhysics.HitInfo> m_raycastList;
        private float m_headMovementXOffset;
        private float m_headMovementYOffset;
        private float m_maxHeadMovementOffset = 3f;
        private float m_headMovementStep = 0.1f;
        private bool m_lastGetViewWasDead;
        private Matrix m_getViewAliveWorldMatrix = Matrix.Identity;
        private Vector3D m_lastProceduralGeneratorPosition = Vector3D.PositiveInfinity;
        private static readonly List<uint> m_tmpIds = new List<uint>();
        private MyControllerInfo m_info = new MyControllerInfo();
        private MyDefinitionId? m_endShootAutoswitch;
        private MyDefinitionId? m_autoswitch;
        private MatrixD m_lastCorrectSpectatorCamera;
        private float m_squeezeDamageTimer;
        private const float m_weaponMinAmp = 1.123778f;
        private const float m_weaponMaxAmp = 1.217867f;
        private const float m_weaponMedAmp = 1.170823f;
        private const float m_weaponRunMedAmp = 1.128767f;
        private Quaternion m_weaponMatrixOrientationBackup;
        private MyCharacterBreath m_breath;
        public VRage.Game.Entity.MyEntity ManipulatedEntity;
        private MyGuiScreenBase m_InventoryScreen;
        private MyCharacterClientState m_lastClientState;
        private VRage.Game.Entity.MyEntity m_relativeDampeningEntity;
        private List<MyPhysics.HitInfo> m_hits = new List<MyPhysics.HitInfo>();
        private List<MyPhysics.HitInfo> m_hits2 = new List<MyPhysics.HitInfo>(4);
        private MyCubeGrid m_standingOnGrid;
        private MyVoxelBase m_standingOnVoxel;

        public event Action<MyCharacter> CharacterDied
        {
            [CompilerGenerated] add
            {
                Action<MyCharacter> characterDied = this.CharacterDied;
                while (true)
                {
                    Action<MyCharacter> a = characterDied;
                    Action<MyCharacter> action3 = (Action<MyCharacter>) Delegate.Combine(a, value);
                    characterDied = Interlocked.CompareExchange<Action<MyCharacter>>(ref this.CharacterDied, action3, a);
                    if (ReferenceEquals(characterDied, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCharacter> characterDied = this.CharacterDied;
                while (true)
                {
                    Action<MyCharacter> source = characterDied;
                    Action<MyCharacter> action3 = (Action<MyCharacter>) Delegate.Remove(source, value);
                    characterDied = Interlocked.CompareExchange<Action<MyCharacter>>(ref this.CharacterDied, action3, source);
                    if (ReferenceEquals(characterDied, source))
                    {
                        return;
                    }
                }
            }
        }

        public event CharacterMovementStateChangedDelegate MovementStateChanged
        {
            [CompilerGenerated] add
            {
                CharacterMovementStateChangedDelegate movementStateChanged = this.MovementStateChanged;
                while (true)
                {
                    CharacterMovementStateChangedDelegate a = movementStateChanged;
                    CharacterMovementStateChangedDelegate delegate4 = (CharacterMovementStateChangedDelegate) Delegate.Combine(a, value);
                    movementStateChanged = Interlocked.CompareExchange<CharacterMovementStateChangedDelegate>(ref this.MovementStateChanged, delegate4, a);
                    if (ReferenceEquals(movementStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                CharacterMovementStateChangedDelegate movementStateChanged = this.MovementStateChanged;
                while (true)
                {
                    CharacterMovementStateChangedDelegate source = movementStateChanged;
                    CharacterMovementStateChangedDelegate delegate4 = (CharacterMovementStateChangedDelegate) Delegate.Remove(source, value);
                    movementStateChanged = Interlocked.CompareExchange<CharacterMovementStateChangedDelegate>(ref this.MovementStateChanged, delegate4, source);
                    if (ReferenceEquals(movementStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyCharacter> OnCharacterDied
        {
            [CompilerGenerated] add
            {
                Action<MyCharacter> onCharacterDied = OnCharacterDied;
                while (true)
                {
                    Action<MyCharacter> a = onCharacterDied;
                    Action<MyCharacter> action3 = (Action<MyCharacter>) Delegate.Combine(a, value);
                    onCharacterDied = Interlocked.CompareExchange<Action<MyCharacter>>(ref OnCharacterDied, action3, a);
                    if (ReferenceEquals(onCharacterDied, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCharacter> onCharacterDied = OnCharacterDied;
                while (true)
                {
                    Action<MyCharacter> source = onCharacterDied;
                    Action<MyCharacter> action3 = (Action<MyCharacter>) Delegate.Remove(source, value);
                    onCharacterDied = Interlocked.CompareExchange<Action<MyCharacter>>(ref OnCharacterDied, action3, source);
                    if (ReferenceEquals(onCharacterDied, source))
                    {
                        return;
                    }
                }
            }
        }

        [Obsolete("OnMovementStateChanged is deprecated, use MovementStateChanged")]
        public event CharacterMovementStateDelegate OnMovementStateChanged
        {
            [CompilerGenerated] add
            {
                CharacterMovementStateDelegate onMovementStateChanged = this.OnMovementStateChanged;
                while (true)
                {
                    CharacterMovementStateDelegate a = onMovementStateChanged;
                    CharacterMovementStateDelegate delegate4 = (CharacterMovementStateDelegate) Delegate.Combine(a, value);
                    onMovementStateChanged = Interlocked.CompareExchange<CharacterMovementStateDelegate>(ref this.OnMovementStateChanged, delegate4, a);
                    if (ReferenceEquals(onMovementStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                CharacterMovementStateDelegate onMovementStateChanged = this.OnMovementStateChanged;
                while (true)
                {
                    CharacterMovementStateDelegate source = onMovementStateChanged;
                    CharacterMovementStateDelegate delegate4 = (CharacterMovementStateDelegate) Delegate.Remove(source, value);
                    onMovementStateChanged = Interlocked.CompareExchange<CharacterMovementStateDelegate>(ref this.OnMovementStateChanged, delegate4, source);
                    if (ReferenceEquals(onMovementStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler OnWeaponChanged
        {
            [CompilerGenerated] add
            {
                EventHandler onWeaponChanged = this.OnWeaponChanged;
                while (true)
                {
                    EventHandler a = onWeaponChanged;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    onWeaponChanged = Interlocked.CompareExchange<EventHandler>(ref this.OnWeaponChanged, handler3, a);
                    if (ReferenceEquals(onWeaponChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler onWeaponChanged = this.OnWeaponChanged;
                while (true)
                {
                    EventHandler source = onWeaponChanged;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    onWeaponChanged = Interlocked.CompareExchange<EventHandler>(ref this.OnWeaponChanged, handler3, source);
                    if (ReferenceEquals(onWeaponChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<IMyCharacter> IMyCharacter.CharacterDied
        {
            add
            {
                this.CharacterDied += this.GetDelegate(value);
            }
            remove
            {
                this.CharacterDied -= this.GetDelegate(value);
            }
        }

        public event Action<IMyHandheldGunObject<MyDeviceBase>> WeaponEquiped
        {
            [CompilerGenerated] add
            {
                Action<IMyHandheldGunObject<MyDeviceBase>> weaponEquiped = this.WeaponEquiped;
                while (true)
                {
                    Action<IMyHandheldGunObject<MyDeviceBase>> a = weaponEquiped;
                    Action<IMyHandheldGunObject<MyDeviceBase>> action3 = (Action<IMyHandheldGunObject<MyDeviceBase>>) Delegate.Combine(a, value);
                    weaponEquiped = Interlocked.CompareExchange<Action<IMyHandheldGunObject<MyDeviceBase>>>(ref this.WeaponEquiped, action3, a);
                    if (ReferenceEquals(weaponEquiped, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IMyHandheldGunObject<MyDeviceBase>> weaponEquiped = this.WeaponEquiped;
                while (true)
                {
                    Action<IMyHandheldGunObject<MyDeviceBase>> source = weaponEquiped;
                    Action<IMyHandheldGunObject<MyDeviceBase>> action3 = (Action<IMyHandheldGunObject<MyDeviceBase>>) Delegate.Remove(source, value);
                    weaponEquiped = Interlocked.CompareExchange<Action<IMyHandheldGunObject<MyDeviceBase>>>(ref this.WeaponEquiped, action3, source);
                    if (ReferenceEquals(weaponEquiped, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyCharacter()
        {
            this.ControllerInfo.ControlAcquired += new Action<MyEntityController>(this.OnControlAcquired);
            this.ControllerInfo.ControlReleased += new Action<MyEntityController>(this.OnControlReleased);
            this.CustomNameWithFaction = new StringBuilder();
            base.PositionComp = new MyCharacterPosition();
            (base.PositionComp as MyPositionComponent).WorldPositionChanged = new Action<object>(this.WorldPositionChanged);
            this.Render = new MyRenderComponentCharacter();
            this.Render.EnableColorMaskHsv = true;
            this.Render.NeedsDraw = true;
            this.Render.CastShadows = true;
            this.Render.NeedsResolveCastShadow = false;
            this.Render.SkipIfTooSmall = false;
            this.Render.DrawInAllCascades = true;
            this.Render.MetalnessColorable = true;
            this.SinkComp = new MyResourceSinkComponent(1);
            this.SyncType = SyncHelpers.Compose(this, 0);
            base.AddDebugRenderComponent(new MyDebugRenderComponentCharacter(this));
            if (MyPerGameSettings.CharacterDetectionComponent != null)
            {
                base.Components.Add<MyCharacterDetectorComponent>((MyCharacterDetectorComponent) Activator.CreateInstance(MyPerGameSettings.CharacterDetectionComponent));
            }
            else
            {
                base.Components.Add<MyCharacterDetectorComponent>(new MyCharacterRaycastDetectorComponent());
            }
            this.m_currentAmmoCount.AlwaysReject<int, SyncDirection.FromServer>();
            this.m_currentMagazineAmmoCount.AlwaysReject<int, SyncDirection.FromServer>();
            this.m_controlInfo.ValueChanged += x => this.ControlChanged();
            this.m_controlInfo.AlwaysReject<MyPlayer.PlayerId, SyncDirection.FromServer>();
            this.m_isShooting = new bool[((MyShootActionEnum) MyEnum<MyShootActionEnum>.Range.Max) + MyShootActionEnum.SecondaryAction];
            base.OnClosing += new Action<VRage.Game.Entity.MyEntity>(this.MyEntity_OnClosing);
        }

        private void AccelerateX(float sign)
        {
            this.m_headMovementXOffset += sign * this.m_headMovementStep;
            if (sign <= 0f)
            {
                if (this.m_headMovementXOffset < -this.m_maxHeadMovementOffset)
                {
                    this.m_headMovementXOffset = -this.m_maxHeadMovementOffset;
                }
            }
            else if (this.m_headMovementXOffset > this.m_maxHeadMovementOffset)
            {
                this.m_headMovementXOffset = this.m_maxHeadMovementOffset;
            }
        }

        private void AccelerateY(float sign)
        {
            this.m_headMovementYOffset += sign * this.m_headMovementStep;
            if (sign <= 0f)
            {
                if (this.m_headMovementYOffset < -this.m_maxHeadMovementOffset)
                {
                    this.m_headMovementYOffset = -this.m_maxHeadMovementOffset;
                }
            }
            else if (this.m_headMovementYOffset > this.m_maxHeadMovementOffset)
            {
                this.m_headMovementYOffset = this.m_maxHeadMovementOffset;
            }
        }

        public override void AddCommand(MyAnimationCommand command, bool sync = false)
        {
            if (!base.UseNewAnimationSystem)
            {
                base.AddCommand(command, sync);
                if (sync)
                {
                    this.SendAnimationCommand(ref command);
                }
            }
        }

        private void AddLadderConstraint(MyCubeGrid ladderGrid)
        {
            MyCharacterProxy characterProxy = this.Physics.CharacterProxy;
            if (characterProxy != null)
            {
                characterProxy.GetHitRigidBody().UpdateMotionType(HkMotionType.Dynamic);
                this.m_constraintData = new HkFixedConstraintData();
                if (Sync.IsServer)
                {
                    this.m_constraintBreakableData = new HkBreakableConstraintData(this.m_constraintData);
                    this.m_constraintBreakableData.ReapplyVelocityOnBreak = false;
                    this.m_constraintBreakableData.RemoveFromWorldOnBrake = false;
                    this.m_constraintBreakableData.Threshold = 200f;
                }
                else if (this.m_constraintBreakableData != null)
                {
                    this.m_constraintBreakableData = null;
                }
                this.m_constraintInstance = new HkConstraint(ladderGrid.Physics.RigidBody, characterProxy.GetHitRigidBody(), this.m_constraintBreakableData ?? this.m_constraintData);
                ladderGrid.Physics.AddConstraint(this.m_constraintInstance);
                this.m_constraintInstance.SetVirtualMassInverse(VRageMath.Vector4.Zero, VRageMath.Vector4.One);
            }
        }

        private float AdjustSafeAnimationBlend(float idealBlend)
        {
            float num = 0f;
            if (this.m_currentAnimationChangeDelay > this.SAFE_DELAY_FOR_ANIMATION_BLEND)
            {
                num = idealBlend;
            }
            this.m_currentAnimationChangeDelay = 0f;
            return num;
        }

        private MyBlendOption AdjustSafeAnimationEnd(MyBlendOption idealEnd)
        {
            MyBlendOption immediate = MyBlendOption.Immediate;
            if (this.m_currentAnimationChangeDelay > this.SAFE_DELAY_FOR_ANIMATION_BLEND)
            {
                immediate = idealEnd;
            }
            return immediate;
        }

        private void ApplyDamage(DamageImpactEnum damageImpact, MyStringHash myDamageType)
        {
            if (Sync.IsServer)
            {
                if ((MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_SHOW_DAMAGE) && (damageImpact != DamageImpactEnum.NoDamage))
                {
                    MyRenderProxy.DebugDrawText2D(new Vector2(100f, 100f), "DAMAGE! TYPE: " + myDamageType.ToString() + " IMPACT: " + damageImpact.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
                switch (damageImpact)
                {
                    case DamageImpactEnum.NoDamage:
                        break;

                    case DamageImpactEnum.SmallDamage:
                        this.DoDamage(MyPerGameSettings.CharacterSmallDamage, myDamageType, true, 0L);
                        return;

                    case DamageImpactEnum.MediumDamage:
                        this.DoDamage(MyPerGameSettings.CharacterMediumDamage, myDamageType, true, 0L);
                        return;

                    case DamageImpactEnum.CriticalDamage:
                        this.DoDamage(MyPerGameSettings.CharacterCriticalDamage, myDamageType, true, 0L);
                        return;

                    case DamageImpactEnum.DeadlyDamage:
                        this.DoDamage(MyPerGameSettings.CharacterDeadlyDamage, myDamageType, true, 0L);
                        break;

                    default:
                        return;
                }
            }
        }

        public override void ApplyLastControls()
        {
            if (this.m_lastClientState.Valid && (Sync.IsServer || !this.ControllerInfo.IsLocallyControlled()))
            {
                this.CacheMove(ref this.m_lastClientState.MoveIndicator, ref this.m_lastClientState.Rotation);
            }
        }

        public unsafe void ApplyRotation(Quaternion rot)
        {
            if (!this.IsOnLadder)
            {
                MatrixD other = MatrixD.CreateFromQuaternion(rot);
                if (!this.JetpackRunning || (this.Physics.CharacterProxy == null))
                {
                    if (this.Physics.CharacterProxy != null)
                    {
                        this.Physics.CharacterProxy.SetForwardAndUp((Vector3) other.Forward, (Vector3) other.Up);
                    }
                }
                else
                {
                    float y = base.ModelCollision.BoundingBoxSizeHalf.Y;
                    MatrixD* xdPtr1 = (MatrixD*) ref other;
                    xdPtr1.Translation = (this.Physics.GetWorldMatrix().Translation + (base.WorldMatrix.Up * y)) - (other.Up * y);
                    this.IsRotating = !base.WorldMatrix.EqualsFast(ref other, 0.0001);
                    base.WorldMatrix = other;
                    this.ClearShapeContactPoints();
                }
            }
        }

        public void BeginShoot(MyShootActionEnum action)
        {
            if (this.m_currentMovementState != MyCharacterMovementEnum.Died)
            {
                PerFrameData data2;
                if (this.m_currentWeapon == null)
                {
                    if (action == MyShootActionEnum.SecondaryAction)
                    {
                        this.UseTerminal();
                    }
                    else
                    {
                        this.Use();
                        this.m_usingByPrimary = true;
                        if (MySessionComponentReplay.Static.IsEntityBeingRecorded(base.EntityId))
                        {
                            data2 = new PerFrameData();
                            UseData data3 = new UseData {
                                Use = true
                            };
                            data2.UseData = new UseData?(data3);
                            PerFrameData data = data2;
                            MySessionComponentReplay.Static.ProvideEntityRecordData(base.EntityId, data);
                        }
                    }
                }
                else
                {
                    MyShootActionEnum? shootingAction = this.GetShootingAction();
                    if ((shootingAction != null) && (action != ((MyShootActionEnum) shootingAction.Value)))
                    {
                        this.EndShoot(shootingAction.Value);
                    }
                    if (!this.m_currentWeapon.EnabledInWorldRules)
                    {
                        MyHud.Notifications.Add(MyNotificationSingletons.WeaponDisabledInWorldSettings);
                    }
                    else
                    {
                        if (MySessionComponentReplay.Static.IsEntityBeingRecorded(base.EntityId))
                        {
                            data2 = new PerFrameData();
                            ShootData data5 = new ShootData {
                                Begin = true,
                                ShootAction = (byte) action
                            };
                            data2.ShootData = new ShootData?(data5);
                            PerFrameData data = data2;
                            MySessionComponentReplay.Static.ProvideEntityRecordData(base.EntityId, data);
                        }
                        this.UpdateShootDirection(this.m_currentWeapon.DirectionToTarget(this.m_aimedPoint), this.m_currentWeapon.ShootDirectionUpdateTime);
                        this.BeginShootSync(this.ShootDirection, action);
                    }
                }
            }
        }

        public void BeginShootSync(Vector3 direction, MyShootActionEnum action = 0)
        {
            this.StartShooting(direction, action);
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, Vector3, MyShootActionEnum>(this, x => new Action<Vector3, MyShootActionEnum>(x.ShootBeginCallback), direction, action, targetEndpoint);
            if (MyFakes.SIMULATE_QUICK_TRIGGER)
            {
                this.EndShootInternal(action);
            }
        }

        public void CacheMove(ref Vector3 moveIndicator, ref Quaternion rotate)
        {
            if (this.m_cachedCommands == null)
            {
                this.m_cachedCommands = new List<IMyNetworkCommand>();
            }
            this.m_cachedCommands.Add(new MyMoveNetCommand(this, ref moveIndicator, ref rotate));
        }

        public void CacheMoveDelta(ref Vector3D moveDeltaIndicator)
        {
            if (this.m_cachedCommands == null)
            {
                this.m_cachedCommands = new List<IMyNetworkCommand>();
            }
            this.m_cachedCommands.Add(new MyDeltaNetCommand(this, ref moveDeltaIndicator));
        }

        private void CalculateHandIK(int startBoneIndex, int endBoneIndex, ref MatrixD targetTransform)
        {
            MyCharacterBone finalBone = base.AnimationController.CharacterBones[endBoneIndex];
            MyCharacterBone bone1 = base.AnimationController.CharacterBones[startBoneIndex];
            List<MyCharacterBone> bones = new List<MyCharacterBone>();
            for (int i = startBoneIndex; i <= endBoneIndex; i++)
            {
                bones.Add(base.AnimationController.CharacterBones[i]);
            }
            MatrixD worldMatrixNormalizedInv = base.PositionComp.WorldMatrixNormalizedInv;
            Matrix finalTransform = (Matrix) (targetTransform * worldMatrixNormalizedInv);
            Vector3 translation = finalTransform.Translation;
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_IKSOLVERS)
            {
                MyRenderProxy.DebugDrawText3D(targetTransform.Translation, "Hand target transform", Color.Purple, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                MyRenderProxy.DebugDrawSphere(targetTransform.Translation, 0.03f, Color.Purple, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawAxis(targetTransform, 0.03f, false, false, false);
            }
            targetTransform.Translation;
            MyInverseKinematics.SolveCCDIk(ref translation, bones, 0.0005f, 5, 0.5f, ref finalTransform, finalBone, true);
        }

        private void CalculateHandIK(int upperarmIndex, int forearmIndex, int palmIndex, ref MatrixD targetTransform)
        {
            MyCharacterBone[] characterBones = base.AnimationController.CharacterBones;
            MatrixD worldMatrixNormalizedInv = base.PositionComp.WorldMatrixNormalizedInv;
            Matrix finalTransform = (Matrix) (targetTransform * worldMatrixNormalizedInv);
            Vector3 translation = finalTransform.Translation;
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_IKSOLVERS)
            {
                MyRenderProxy.DebugDrawText3D(targetTransform.Translation, "Hand target transform", Color.Purple, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                MyRenderProxy.DebugDrawSphere(targetTransform.Translation, 0.03f, Color.Purple, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawAxis(targetTransform, 0.03f, false, false, false);
            }
            if ((characterBones.IsValidIndex<MyCharacterBone>(upperarmIndex) && characterBones.IsValidIndex<MyCharacterBone>(forearmIndex)) && characterBones.IsValidIndex<MyCharacterBone>(palmIndex))
            {
                MatrixD worldMatrix = base.PositionComp.WorldMatrix;
                MyInverseKinematics.SolveTwoJointsIkCCD(characterBones, upperarmIndex, forearmIndex, palmIndex, ref finalTransform, ref worldMatrix, characterBones[palmIndex], true);
            }
        }

        protected override unsafe void CalculateTransforms(float distance)
        {
            bool flag = this.IsInFirstPersonView && ReferenceEquals(MySession.Static.CameraController, this);
            bool flag2 = flag || this.ForceFirstPersonCamera;
            base.CalculateTransforms(distance);
            Vector3 zero = Vector3.Zero;
            if (((((this.m_headBoneIndex >= 0) && (base.AnimationController.CharacterBones != null)) & flag) && (ReferenceEquals(MySession.Static.CameraController, this) && !this.IsBot)) && !this.IsOnLadder)
            {
                zero = base.AnimationController.CharacterBones[this.m_headBoneIndex].AbsoluteTransform.Translation;
                zero.Y = 0f;
                MyCharacterBone.TranslateAllBones(base.AnimationController.CharacterBones, -zero);
            }
            if (this.IsOnLadder)
            {
                this.m_wasOnLadder = 100;
            }
            else if (this.m_wasOnLadder > 0)
            {
                this.m_wasOnLadder--;
            }
            if (this.Entity.InScene && (this.m_wasOnLadder == 0))
            {
                base.AnimationController.UpdateInverseKinematics();
            }
            if (this.m_leftHandItem != null)
            {
                this.UpdateLeftHandItemPosition();
            }
            if (((this.m_currentWeapon != null) && (this.WeaponPosition != null)) && (this.m_handItemDefinition != null))
            {
                this.WeaponPosition.Update(true);
                if (((flag2 ? this.m_handItemDefinition.SimulateLeftHandFps : this.m_handItemDefinition.SimulateLeftHand) && (this.m_leftHandIKStartBone != -1)) && (this.m_leftHandIKEndBone != -1))
                {
                    MatrixD targetTransform = this.m_handItemDefinition.LeftHand * ((VRage.Game.Entity.MyEntity) this.m_currentWeapon).WorldMatrix;
                    this.CalculateHandIK(this.m_leftHandIKStartBone, this.m_leftForearmBone, this.m_leftHandIKEndBone, ref targetTransform);
                }
                bool flag3 = flag2 ? this.m_handItemDefinition.SimulateRightHandFps : this.m_handItemDefinition.SimulateRightHand;
                if (((this.m_rightHandIKStartBone != -1) && (this.m_rightHandIKEndBone != -1)) && !this.IsSitting)
                {
                    if (flag3)
                    {
                        MatrixD targetTransform = this.m_handItemDefinition.RightHand * ((VRage.Game.Entity.MyEntity) this.m_currentWeapon).WorldMatrix;
                        this.CalculateHandIK(this.m_rightHandIKStartBone, this.m_rightForearmBone, this.m_rightHandIKEndBone, ref targetTransform);
                    }
                    else if ((this.m_handItemDefinition.SimulateRightHand && !this.m_handItemDefinition.SimulateRightHandFps) & flag2)
                    {
                        Matrix absoluteRigTransform = base.AnimationController.CharacterBones[this.SpineBoneIndex].GetAbsoluteRigTransform();
                        Matrix* matrixPtr1 = (Matrix*) ref absoluteRigTransform;
                        matrixPtr1.Translation -= 2f * zero;
                        base.AnimationController.CharacterBones[this.m_rightHandIKEndBone].SetCompleteBindTransform();
                        base.AnimationController.CharacterBones[this.m_rightForearmBone].SetCompleteBindTransform();
                        base.AnimationController.CharacterBones[this.m_rightHandIKStartBone].SetCompleteTransformFromAbsoluteMatrix(ref absoluteRigTransform, false);
                    }
                }
            }
            base.AnimationController.UpdateTransformations();
        }

        public bool CanPlaceCharacter(ref MatrixD worldMatrix, bool useCharacterCenter = false, bool checkCharacters = false, VRage.Game.Entity.MyEntity ignoreEntity = null)
        {
            Vector3D translation = worldMatrix.Translation;
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(worldMatrix);
            if ((this.Physics == null) || ((this.Physics.CharacterProxy == null) && (this.Physics.RigidBody == null)))
            {
                return true;
            }
            this.m_penetrationList.Clear();
            if (!useCharacterCenter)
            {
                translation += Vector3D.TransformNormal(this.Physics.Center, worldMatrix);
            }
            this.m_penetrationList.Clear();
            MyPhysics.GetPenetrationsShape((this.Physics.CharacterProxy != null) ? this.Physics.CharacterProxy.GetCollisionShape() : this.Physics.RigidBody.GetShape(), ref translation, ref rotation, this.m_penetrationList, 0x12);
            bool flag = false;
            using (List<HkBodyCollision>.Enumerator enumerator = this.m_penetrationList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    VRage.ModAPI.IMyEntity collisionEntity = enumerator.Current.GetCollisionEntity();
                    if (!ReferenceEquals(ignoreEntity, collisionEntity))
                    {
                        if (collisionEntity == null)
                        {
                            if (!checkCharacters)
                            {
                                continue;
                            }
                            flag = true;
                        }
                        else
                        {
                            if (collisionEntity.Physics == null)
                            {
                                MyLog.Default.WriteLine("CanPlaceCharacter found Entity with no physics: " + collisionEntity);
                                continue;
                            }
                            if (collisionEntity.Physics.IsPhantom)
                            {
                                continue;
                            }
                            flag = true;
                        }
                        break;
                    }
                }
            }
            if (MySession.Static.VoxelMaps == null)
            {
                return true;
            }
            if (!flag)
            {
                BoundingSphereD sphere = new BoundingSphereD(worldMatrix.Translation, 0.75);
                flag = MySession.Static.VoxelMaps.GetOverlappingWithSphere(ref sphere) != null;
            }
            return !flag;
        }

        public bool CanStartConstruction(MyCubeBlockDefinition blockDefinition)
        {
            if (blockDefinition == null)
            {
                return false;
            }
            MyInventoryBase builderInventory = MyCubeBuilder.BuildComponent.GetBuilderInventory(this);
            return ((builderInventory != null) ? (builderInventory.GetItemAmount(blockDefinition.Components[0].Definition.Id, MyItemFlags.None, false) >= 1) : false);
        }

        public bool CanStartConstruction(Dictionary<MyDefinitionId, int> constructionCost)
        {
            MyInventoryBase builderInventory = MyCubeBuilder.BuildComponent.GetBuilderInventory(this);
            using (Dictionary<MyDefinitionId, int>.Enumerator enumerator = constructionCost.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<MyDefinitionId, int> current = enumerator.Current;
                    if (builderInventory.GetItemAmount(current.Key, MyItemFlags.None, false) < current.Value)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool CanSwitchAmmoMagazine() => 
            ((this.m_currentWeapon != null) && ((this.m_currentWeapon.GunBase != null) && this.m_currentWeapon.GunBase.CanSwitchAmmoMagazine()));

        public bool CanSwitchToWeapon(MyDefinitionId? weaponDefinition)
        {
            if (((weaponDefinition != null) && (weaponDefinition.Value.TypeId == typeof(MyObjectBuilder_CubePlacer))) && !MySessionComponentSafeZones.IsActionAllowed(this, MySafeZoneAction.Building, 0L))
            {
                return false;
            }
            if (this.IsOnLadder)
            {
                return false;
            }
            return (!this.WeaponTakesBuilderFromInventory(weaponDefinition) || (this.FindWeaponItemByDefinition(weaponDefinition.Value) != null));
        }

        public void ChangeLadder(MyLadder newLadder, bool resetPosition = false)
        {
            if (!ReferenceEquals(newLadder, this.m_ladder))
            {
                MyLadder ladder = this.m_ladder;
                bool flag = true;
                if (ladder != null)
                {
                    if (newLadder != null)
                    {
                        flag = !ReferenceEquals(ladder.CubeGrid, newLadder.CubeGrid);
                    }
                    ladder.IsWorkingChanged -= new Action<MyCubeBlock>(this.MyLadder_IsWorkingChanged);
                    ladder.CubeGridChanged -= new Action<MyCubeGrid>(this.Ladder_OnCubeGridChanged);
                    ladder.OnClose -= new Action<VRage.Game.Entity.MyEntity>(this.m_ladder_OnClose);
                }
                if ((ladder != null) && (newLadder != null))
                {
                    this.m_baseMatrix = (this.m_baseMatrix * ladder.PositionComp.WorldMatrix) * newLadder.PositionComp.WorldMatrixNormalizedInv;
                }
                this.m_ladder = newLadder;
                if (newLadder != null)
                {
                    newLadder.IsWorkingChanged += new Action<MyCubeBlock>(this.MyLadder_IsWorkingChanged);
                    newLadder.CubeGridChanged += new Action<MyCubeGrid>(this.Ladder_OnCubeGridChanged);
                    newLadder.OnClose += new Action<VRage.Game.Entity.MyEntity>(this.m_ladder_OnClose);
                }
                if (flag && (this.Physics != null))
                {
                    this.ReconnectConstraint(ladder?.CubeGrid, newLadder?.CubeGrid);
                    if (newLadder != null)
                    {
                        this.PutCharacterOnLadder(newLadder, resetPosition);
                    }
                }
            }
        }

        [Event(null, 0x228e), Reliable, Server(ValidationType.Controlled), Broadcast]
        private void ChangeModel_Implementation(string model, Vector3 colorMaskHSV, bool resetToDefault, long caller)
        {
            this.ChangeModelAndColorInternal(model, colorMaskHSV);
            if (MySession.Static.LocalPlayerId == caller)
            {
                MyGuiScreenLoadInventory.ResetOnFinish(model, resetToDefault);
            }
        }

        public void ChangeModelAndColor(string model, Vector3 colorMaskHSV, bool resetToDefault = false, long caller = 0L)
        {
            if (this.ResponsibleForUpdate(Sync.Clients.LocalClient))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCharacter, string, Vector3, bool, long>(this, x => new Action<string, Vector3, bool, long>(x.ChangeModel_Implementation), model, colorMaskHSV, resetToDefault, caller, targetEndpoint);
            }
        }

        internal void ChangeModelAndColorInternal(string model, Vector3 colorMaskHSV)
        {
            if (!base.Closed)
            {
                MyCharacterDefinition definition;
                if (((model != this.m_characterModel) && MyDefinitionManager.Static.Characters.TryGetValue(model, out definition)) && !string.IsNullOrEmpty(definition.Model))
                {
                    MyObjectBuilder_Character objectBuilder = (MyObjectBuilder_Character) this.GetObjectBuilder(false);
                    base.Components.Remove<MyInventoryBase>();
                    base.Components.Remove<MyCharacterJetpackComponent>();
                    base.Components.Remove<MyCharacterRagdollComponent>();
                    base.AnimationController.Clear();
                    MyModel modelOnlyData = MyModels.GetModelOnlyData(definition.Model);
                    if (modelOnlyData == null)
                    {
                        return;
                    }
                    this.Render.CleanLights();
                    this.CloseInternal();
                    if (!Sandbox.Game.Entities.MyEntities.Remove(this))
                    {
                        Sandbox.Game.Entities.MyEntities.UnregisterForUpdate(this, false);
                        this.Render.RemoveRenderObjects();
                    }
                    if (this.Physics != null)
                    {
                        this.Physics.Close();
                        this.Physics = null;
                    }
                    this.m_characterModel = model;
                    this.Render.ModelStorage = modelOnlyData;
                    objectBuilder.CharacterModel = model;
                    objectBuilder.EntityId = 0L;
                    if (objectBuilder.HandWeapon != null)
                    {
                        objectBuilder.HandWeapon.EntityId = 0L;
                    }
                    if (this.m_breath != null)
                    {
                        this.m_breath.Close();
                        this.m_breath = null;
                    }
                    float num = (this.StatComp != null) ? this.StatComp.HealthRatio : 1f;
                    float headLocalXAngle = this.m_headLocalXAngle;
                    float headLocalYAngle = this.m_headLocalYAngle;
                    MatrixD worldMatrix = base.PositionComp.WorldMatrix;
                    objectBuilder.PositionAndOrientation = null;
                    this.Init(objectBuilder);
                    base.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                    this.GetInventory(0).ResetVolume();
                    this.InitInventory(objectBuilder);
                    this.m_headLocalXAngle = headLocalXAngle;
                    this.m_headLocalYAngle = headLocalYAngle;
                    if ((this.StatComp != null) && (this.StatComp.Health != null))
                    {
                        this.StatComp.Health.Value = this.StatComp.Health.MaxValue - (this.StatComp.Health.MaxValue * (1f - num));
                    }
                    this.SwitchAnimation(objectBuilder.MovementState, false);
                    if (this.m_currentWeapon != null)
                    {
                        this.m_currentWeapon.OnControlAcquired(this);
                    }
                    if (base.Parent == null)
                    {
                        Sandbox.Game.Entities.MyEntities.Add(this, true);
                    }
                    else if (!base.InScene)
                    {
                        this.OnAddedToScene(this);
                    }
                    MyPlayer player = this.TryGetPlayer();
                    if ((player != null) && (player.Identity != null))
                    {
                        player.Identity.ChangeCharacter(this);
                    }
                    this.SuitRechargeDistributor.UpdateBeforeSimulation();
                }
                this.Render.ColorMaskHsv = colorMaskHSV;
                if (MySession.Static.LocalHumanPlayer != null)
                {
                    MySession.Static.LocalHumanPlayer.Identity.SetColorMask(colorMaskHSV);
                }
            }
        }

        private MyLadder CheckBottomLadder(Vector3D position, ref Vector3 movementDelta, out bool isHit)
        {
            Vector3D from = ((position + (base.WorldMatrix.Up * 0.20000000298023224)) + movementDelta) - (base.WorldMatrix.Forward * 0.20000000298023224);
            Vector3D to = (from + (base.WorldMatrix.Down * 0.40000000596046448)) + (base.WorldMatrix.Forward * 1.5);
            return this.FindLadder(ref from, ref to, out isHit);
        }

        private void CheckExistingStatComponent()
        {
            if (this.StatComp == null)
            {
                bool flag = false;
                MyContainerDefinition definition = null;
                MyComponentContainerExtension.TryGetContainerDefinition(this.m_characterDefinition.Id.TypeId, this.m_characterDefinition.Id.SubtypeId, out definition);
                if (definition != null)
                {
                    using (List<MyContainerDefinition.DefaultComponent>.Enumerator enumerator = definition.DefaultComponents.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.BuilderType == typeof(MyObjectBuilder_CharacterStatComponent))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }
                object[] objArray1 = new object[] { "Stat component has not been created for character: ", this.m_characterDefinition.Id, ", container defined: ", (definition != null).ToString(), ", stat component defined: ", flag.ToString() };
                string msg = string.Concat(objArray1);
                MyLog.Default.WriteLine(msg);
            }
        }

        private MyLadder CheckMiddleLadder(Vector3D position, ref Vector3 movementDelta)
        {
            Vector3D from = ((position + movementDelta) + (base.WorldMatrix.Up * 0.800000011920929)) - (base.WorldMatrix.Forward * 0.20000000298023224);
            Vector3D to = from + (base.WorldMatrix.Forward * 1.5);
            return this.FindLadder(ref from, ref to);
        }

        private MyLadder CheckTopLadder(Vector3D position, ref Vector3 movementDelta, out bool isHit)
        {
            Vector3D from = ((position + movementDelta) + (base.WorldMatrix.Up * 1.75)) - (base.WorldMatrix.Forward * 0.20000000298023224);
            Vector3D to = (from + (base.WorldMatrix.Up * 0.40000000596046448)) + (base.WorldMatrix.Forward * 1.5);
            return this.FindLadder(ref from, ref to, out isHit);
        }

        internal void ClearShapeContactPoints()
        {
            this.m_shapeContactPoints.Clear();
        }

        private void CloseInternal()
        {
            if (this.m_currentWeapon != null)
            {
                ((VRage.Game.Entity.MyEntity) this.m_currentWeapon).Close();
                this.m_currentWeapon = null;
            }
            if (this.m_leftHandItem != null)
            {
                this.m_leftHandItem.Close();
                this.m_leftHandItem = null;
            }
            this.RemoveNotifications();
            if (this.IsOnLadder)
            {
                this.CloseLadderConstraint(this.m_ladder.CubeGrid);
                this.m_ladder.IsWorkingChanged -= new Action<MyCubeBlock>(this.MyLadder_IsWorkingChanged);
            }
            this.RadioBroadcaster.Enabled = false;
            if (this.Render != null)
            {
                this.Render.CleanLights();
            }
            if (MyToolbarComponent.CharacterToolbar != null)
            {
                MyToolbarComponent.CharacterToolbar.ItemChanged -= new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
            }
        }

        private void CloseLadderConstraint(MyCubeGrid ladderGrid)
        {
            if (this.Physics.CharacterProxy != null)
            {
                if ((this.m_constraintInstance != null) && (ladderGrid != null))
                {
                    MyGridPhysics physics = ladderGrid.Physics;
                    if (physics != null)
                    {
                        physics.RemoveConstraint(this.m_constraintInstance);
                    }
                    this.m_constraintInstance.Dispose();
                    this.m_constraintInstance = null;
                }
                if (this.m_constraintBreakableData != null)
                {
                    this.m_constraintData.Dispose();
                    this.m_constraintBreakableData = null;
                }
                this.m_constraintData = null;
            }
        }

        protected override void Closing()
        {
            this.CloseInternal();
            if (this.m_breath != null)
            {
                this.m_breath.Close();
            }
            base.Closing();
        }

        private float ComputeRequiredPower()
        {
            float num = 1E-05f;
            if ((this.OxygenComponent != null) && this.OxygenComponent.NeedsOxygenFromSuit)
            {
                num = 1E-06f;
            }
            if (this.m_lightEnabled)
            {
                num += 2E-06f;
            }
            return (num + Math.Abs((float) (((this.GetOutsideTemperature() * 2f) - 1f) * (this.Definition.SuitConsumptionInTemperatureExtreme / 100000f))));
        }

        private void ControlChanged()
        {
            if (!Sync.IsServer && !this.IsDead)
            {
                if ((this.m_controlInfo.Value.SteamId != 0) && ((this.ControllerInfo.Controller == null) || (this.ControllerInfo.Controller.Player.Id != this.m_controlInfo.Value)))
                {
                    MyPlayer playerById = Sync.Players.GetPlayerById(this.m_controlInfo.Value);
                    if (playerById != null)
                    {
                        MyPlayerCollection.ChangePlayerCharacter(playerById, this, this);
                        if ((playerById.Controller != null) && (playerById.Controller.ControlledEntity != null))
                        {
                            this.IsUsing = playerById.Controller.ControlledEntity as VRage.Game.Entity.MyEntity;
                        }
                        if (((this.m_usingEntity != null) && (playerById != null)) && !ReferenceEquals(Sync.Players.GetControllingPlayer(this.m_usingEntity), playerById))
                        {
                            Sync.Players.SetControlledEntityLocally(playerById.Id, this.m_usingEntity);
                        }
                    }
                }
                if (!this.IsDead && ReferenceEquals(this, MySession.Static.LocalCharacter))
                {
                    MySpectatorCameraController.Static.Position = base.PositionComp.GetPosition();
                }
            }
        }

        private void CreateBodyCapsulesForHits(Dictionary<string, MyCharacterDefinition.RagdollBoneSet> bonesMappings)
        {
            this.m_bodyCapsuleInfo.Clear();
            this.m_bodyCapsules = new CapsuleD[bonesMappings.Count];
            foreach (KeyValuePair<string, MyCharacterDefinition.RagdollBoneSet> pair in bonesMappings)
            {
                try
                {
                    int num;
                    int num2;
                    string[] bones = pair.Value.Bones;
                    MyCharacterBone bone = base.AnimationController.FindBone(bones.First<string>(), out num);
                    MyCharacterBone bone2 = base.AnimationController.FindBone(bones.Last<string>(), out num2);
                    if (bone == null)
                    {
                        continue;
                    }
                    if (bone2 == null)
                    {
                        continue;
                    }
                    if (bone.Depth > bone2.Depth)
                    {
                        num = num2;
                        num2 = num;
                    }
                    MyBoneCapsuleInfo item = new MyBoneCapsuleInfo();
                    item.Bone1 = bone.Index;
                    item.Bone2 = bone2.Index;
                    item.AscendantBone = num;
                    item.DescendantBone = num2;
                    item.Radius = pair.Value.CollisionRadius;
                    this.m_bodyCapsuleInfo.Add(item);
                }
                catch (Exception)
                {
                }
            }
            for (int i = 0; i < this.m_bodyCapsuleInfo.Count; i++)
            {
                if (this.m_bodyCapsuleInfo[i].Bone1 == this.m_headBoneIndex)
                {
                    this.m_bodyCapsuleInfo.Move<MyBoneCapsuleInfo>(i, 0);
                    return;
                }
            }
        }

        [Event(null, 640), Reliable, Broadcast]
        public void CreateBurrowingParticleFX_Client(Vector3D position)
        {
            MyParticleEffect effect;
            if (MyParticlesManager.TryCreateParticleEffect("Burrowing", MatrixD.CreateTranslation(position), out effect))
            {
                effect.WorldMatrix = MatrixD.CreateTranslation(position);
                effect.UserAxisScale = (Vector3) new Vector3D(2.0, 2.0, 2.0);
                m_burrowEffectTable[position] = effect;
            }
        }

        public static MyCharacter CreateCharacter(MatrixD worldMatrix, Vector3 velocity, string characterName, string model, Vector3? colorMask, MyBotDefinition botDefinition, bool findNearPos = true, bool AIMode = false, MyCockpit cockpit = null, bool useInventory = true, long identityId = 0L, bool addDefaultItems = true)
        {
            Vector3D? nullable = null;
            if (findNearPos)
            {
                nullable = Sandbox.Game.Entities.MyEntities.FindFreePlace(worldMatrix.Translation, 2f, 200, 5, 0.5f);
                if (nullable == null)
                {
                    nullable = Sandbox.Game.Entities.MyEntities.FindFreePlace(worldMatrix.Translation, 2f, 200, 5, 5f);
                }
            }
            if (nullable != null)
            {
                worldMatrix.Translation = nullable.Value;
            }
            return CreateCharacterBase(worldMatrix, ref velocity, characterName, model, colorMask, AIMode, useInventory, botDefinition, identityId, addDefaultItems);
        }

        private static MyCharacter CreateCharacterBase(MatrixD worldMatrix, ref Vector3 velocity, string characterName, string model, Vector3? colorMask, bool AIMode, bool useInventory = true, MyBotDefinition botDefinition = null, long identityId = 0L, bool addDefaultItems = true)
        {
            MyCharacter entity = new MyCharacter();
            MyObjectBuilder_Character objectBuilder = Random();
            objectBuilder.CharacterModel = model ?? objectBuilder.CharacterModel;
            if (colorMask != null)
            {
                objectBuilder.ColorMaskHSV = colorMask.Value;
            }
            objectBuilder.JetpackEnabled = MySession.Static.CreativeMode;
            MyObjectBuilder_Battery battery1 = new MyObjectBuilder_Battery();
            battery1.CurrentCapacity = 1f;
            objectBuilder.Battery = battery1;
            objectBuilder.AIMode = AIMode;
            objectBuilder.DisplayName = characterName;
            objectBuilder.LinearVelocity = velocity;
            objectBuilder.PositionAndOrientation = new MyPositionAndOrientation(worldMatrix);
            objectBuilder.CharacterGeneralDamageModifier = 1f;
            objectBuilder.OwningPlayerIdentityId = new long?(identityId);
            entity.Init(objectBuilder);
            Sandbox.Game.Entities.MyEntities.RaiseEntityCreated(entity);
            Sandbox.Game.Entities.MyEntities.Add(entity, true);
            MyInventory inventory = entity.GetInventory(0);
            if (useInventory)
            {
                if ((inventory != null) & addDefaultItems)
                {
                    MyWorldGenerator.InitInventoryWithDefaults(inventory);
                }
            }
            else if (botDefinition != null)
            {
                botDefinition.AddItems(entity);
            }
            if (velocity.Length() > 0f)
            {
                MyCharacterJetpackComponent jetpackComp = entity.JetpackComp;
                if (jetpackComp != null)
                {
                    jetpackComp.EnableDampeners(false);
                }
            }
            return entity;
        }

        public static IMyHandheldGunObject<MyDeviceBase> CreateGun(MyObjectBuilder_EntityBase gunEntity, uint? inventoryItemId = new uint?())
        {
            if (gunEntity == null)
            {
                return null;
            }
            VRage.Game.Entity.MyEntity entity = MyEntityFactory.CreateEntity(gunEntity);
            try
            {
                entity.Init(gunEntity);
            }
            catch (Exception)
            {
                return null;
            }
            IMyHandheldGunObject<MyDeviceBase> obj2 = (IMyHandheldGunObject<MyDeviceBase>) entity;
            if (((obj2 != null) && ((obj2.GunBase != null) && (obj2.GunBase.InventoryItemId == null))) && (inventoryItemId != null))
            {
                obj2.GunBase.InventoryItemId = new uint?(inventoryItemId.Value);
            }
            return obj2;
        }

        public void Crouch()
        {
            if ((!this.IsDead && this.Definition.CanCrouch) && ((!this.JetpackRunning && !this.m_isFalling) && this.HasEnoughSpaceToStandUp()))
            {
                this.WantsCrouch = !this.WantsCrouch;
            }
        }

        internal void DeactivateRespawn()
        {
            this.m_currentRespawnCounter = -1f;
        }

        [Event(null, 0x28d), Reliable, Broadcast]
        public void DeleteBurrowingParticleFX_Client(Vector3D position)
        {
            MyParticleEffect effect;
            if (m_burrowEffectTable.TryGetValue(position, out effect))
            {
                effect.StopEmitting(0f);
                m_burrowEffectTable.Remove(position);
            }
        }

        public override void DeserializeControls(BitStream stream, bool outOfOrder)
        {
            if (!stream.ReadBool())
            {
                this.m_lastClientState.Valid = false;
            }
            else
            {
                MyCharacterClientState state = new MyCharacterClientState(stream);
                if (!outOfOrder)
                {
                    this.m_lastClientState = state;
                    this.SetNetState(ref this.m_lastClientState);
                }
            }
        }

        public void Die()
        {
            if ((this.CharacterCanDie || MyPerGameSettings.CharacterSuicideEnabled) && !this.IsDead)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextSuicide), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum retval) {
                    if (retval == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyCharacter>(this, x => new Action(x.OnSuicideRequest), targetEndpoint);
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.NO, true, size));
            }
        }

        private void DieInternal()
        {
            if (this.CharacterCanDie || MyPerGameSettings.CharacterSuicideEnabled)
            {
                MyPlayer player = this.TryGetPlayer();
                if ((player == null) || !player.IsImmortal)
                {
                    bool flag = ReferenceEquals(MySession.Static.LocalCharacter, this);
                    this.SoundComp.PlayDeathSound((this.StatComp != null) ? this.StatComp.LastDamage.Type : MyStringHash.NullOrEmpty, false);
                    if (base.UseNewAnimationSystem)
                    {
                        base.AnimationController.Variables.SetValue(MyAnimationVariableStorageHints.StrIdDead, 1f);
                    }
                    if (this.m_InventoryScreen != null)
                    {
                        this.m_InventoryScreen.CloseScreen();
                    }
                    if ((this.StatComp != null) && (this.StatComp.Health != null))
                    {
                        this.StatComp.Health.OnStatChanged -= new MyEntityStat.StatChangedDelegate(this.StatComp.OnHealthChanged);
                    }
                    if (this.m_breath != null)
                    {
                        this.m_breath.CurrentState = MyCharacterBreath.State.NoBreath;
                    }
                    if (this.IsOnLadder)
                    {
                        this.GetOffLadder();
                    }
                    if (this.CurrentRemoteControl != null)
                    {
                        MyRemoteControl currentRemoteControl = this.CurrentRemoteControl as MyRemoteControl;
                        if (currentRemoteControl != null)
                        {
                            currentRemoteControl.ForceReleaseControl();
                        }
                        else
                        {
                            MyLargeTurretBase base2 = this.CurrentRemoteControl as MyLargeTurretBase;
                            if (base2 != null)
                            {
                                base2.ForceReleaseControl();
                            }
                        }
                    }
                    if ((this.ControllerInfo != null) && this.ControllerInfo.IsLocallyHumanControlled())
                    {
                        if (MyGuiScreenTerminal.IsOpen)
                        {
                            MyGuiScreenTerminal.Hide();
                        }
                        if (MyGuiScreenGamePlay.ActiveGameplayScreen != null)
                        {
                            MyGuiScreenGamePlay.ActiveGameplayScreen.CloseScreen();
                            MyGuiScreenGamePlay.ActiveGameplayScreen = null;
                        }
                        if (MyGuiScreenGamePlay.TmpGameplayScreenHolder != null)
                        {
                            MyGuiScreenGamePlay.TmpGameplayScreenHolder.CloseScreen();
                            MyGuiScreenGamePlay.TmpGameplayScreenHolder = null;
                        }
                        if (this.ControllerInfo.Controller != null)
                        {
                            this.ControllerInfo.Controller.SaveCamera();
                        }
                    }
                    if (base.Parent is MyCockpit)
                    {
                        MyCockpit parent = base.Parent as MyCockpit;
                        if (ReferenceEquals(parent.Pilot, this))
                        {
                            parent.RemovePilot();
                        }
                    }
                    if (MySession.Static.ControlledEntity is MyRemoteControl)
                    {
                        MyRemoteControl controlledEntity = MySession.Static.ControlledEntity as MyRemoteControl;
                        if (ReferenceEquals(controlledEntity.PreviousControlledEntity, this))
                        {
                            controlledEntity.ForceReleaseControl();
                        }
                    }
                    if ((MySession.Static.ControlledEntity is MyLargeTurretBase) && ReferenceEquals(MySession.Static.LocalCharacter, this))
                    {
                        (MySession.Static.ControlledEntity as MyLargeTurretBase).ForceReleaseControl();
                    }
                    if (this.m_currentMovementState == MyCharacterMovementEnum.Died)
                    {
                        this.StartRespawn(0.1f);
                    }
                    else
                    {
                        ulong playerSteamId = 0UL;
                        if ((this.ControllerInfo.Controller != null) && (this.ControllerInfo.Controller.Player != null))
                        {
                            playerSteamId = this.ControllerInfo.Controller.Player.Id.SteamId;
                            if (!MySession.Static.Cameras.TryGetCameraSettings(this.ControllerInfo.Controller.Player.Id, base.EntityId, (this.ControllerInfo.Controller.ControlledEntity is MyCharacter) && ReferenceEquals(MySession.Static.LocalCharacter, this.ControllerInfo.Controller.ControlledEntity), out this.m_cameraSettingsWhenAlive) && this.ControllerInfo.IsLocallyHumanControlled())
                            {
                                MyEntityCameraSettings settings1 = new MyEntityCameraSettings();
                                settings1.Distance = MyThirdPersonSpectator.Static.GetViewerDistance();
                                settings1.IsFirstPerson = this.IsInFirstPersonView;
                                settings1.HeadAngle = new Vector2(this.HeadLocalXAngle, this.HeadLocalYAngle);
                                this.m_cameraSettingsWhenAlive = settings1;
                            }
                        }
                        MyAnalyticsHelper.ReportPlayerDeath(this.ControllerInfo.IsLocallyHumanControlled(), playerSteamId);
                        MySandboxGame.Log.WriteLine("Player character died. Id : " + playerSteamId);
                        this.m_deadPlayerIdentityId = this.GetPlayerIdentityId();
                        this.IsUsing = null;
                        base.Save = false;
                        this.m_isFalling = false;
                        this.SetCurrentMovementState(MyCharacterMovementEnum.Died);
                        if (Sync.IsServer)
                        {
                            EndpointId targetEndpoint = new EndpointId();
                            MyMultiplayer.RaiseEvent<MyCharacter>(this, x => new Action(x.UnequipWeapon), targetEndpoint);
                        }
                        this.StopUpperAnimation(0.5f);
                        this.m_animationCommandsEnabled = true;
                        if (this.m_isInFirstPerson)
                        {
                            this.PlayCharacterAnimation("DiedFps", MyBlendOption.Immediate, MyFrameOption.PlayOnce, 0.5f, 1f, false, null, false);
                        }
                        else
                        {
                            this.PlayCharacterAnimation("Died", MyBlendOption.Immediate, MyFrameOption.PlayOnce, 0.5f, 1f, false, null, false);
                        }
                        this.InitDeadBodyPhysics();
                        this.StartRespawn(5f);
                        this.m_currentLootingCounter = this.m_characterDefinition.LootingTime;
                        if (flag)
                        {
                            this.EnableLights(false);
                        }
                        if (this.CharacterDied != null)
                        {
                            this.CharacterDied(this);
                        }
                        foreach (MyCharacterComponent component in base.Components)
                        {
                            if (component != null)
                            {
                                component.OnCharacterDead();
                            }
                        }
                        this.SoundComp.CharacterDied();
                        this.JetpackComp = null;
                        if (!base.Components.Has<MyCharacterRagdollComponent>())
                        {
                            base.SyncFlag = true;
                        }
                        Action<MyCharacter> onCharacterDied = OnCharacterDied;
                        if (onCharacterDied != null)
                        {
                            onCharacterDied(this);
                        }
                    }
                }
            }
        }

        public void DisableAnimationCommands()
        {
            this.m_animationCommandsEnabled = false;
        }

        private void DisposeWeapon()
        {
            VRage.Game.Entity.MyEntity currentWeapon = this.m_currentWeapon as VRage.Game.Entity.MyEntity;
            if (currentWeapon != null)
            {
                currentWeapon.EntityId = 0L;
                currentWeapon.Close();
                this.m_currentWeapon = null;
            }
        }

        public bool DoDamage(float damage, MyStringHash damageType, bool updateSync, long attackerId = 0L)
        {
            VRage.Game.Entity.MyEntity entity;
            AdminSettingsEnum enum2;
            damage *= this.CharacterGeneralDamageModifier;
            if (damage < 0f)
            {
                return false;
            }
            if ((damageType != MyDamageType.Suicide) && !MySessionComponentSafeZones.IsActionAllowed(this, MySafeZoneAction.Damage, 0L))
            {
                return false;
            }
            MyPlayer.PlayerId clientIdentity = this.GetClientIdentity();
            if (((clientIdentity.SerialId == 0) && (MySession.Static.RemoteAdminSettings.TryGetValue(clientIdentity.SteamId, out enum2) && enum2.HasFlag(AdminSettingsEnum.Invulnerable))) && (damageType != MyDamageType.Suicide))
            {
                return false;
            }
            if (((damageType != MyDamageType.Suicide) && (this.ControllerInfo.IsLocallyControlled() && ReferenceEquals(MySession.Static.CameraController, this))) && (MAX_SHAKE_DAMAGE > 0f))
            {
                float shakePower = (MySector.MainCamera.CameraShake.MaxShake * MathHelper.Clamp(damage, 0f, MAX_SHAKE_DAMAGE)) / MAX_SHAKE_DAMAGE;
                MySector.MainCamera.CameraShake.AddShake(shakePower);
            }
            if (updateSync)
            {
                this.TriggerCharacterAnimationEvent("hurt", true);
            }
            else
            {
                base.AnimationController.TriggerAction(MyAnimationVariableStorageHints.StrIdActionHurt);
            }
            if ((!this.CharacterCanDie && ((damageType != MyDamageType.Suicide) || !MyPerGameSettings.CharacterSuicideEnabled)) || (this.StatComp == null))
            {
                return false;
            }
            if ((damageType != MyDamageType.Suicide) && Sandbox.Game.Entities.MyEntities.TryGetEntityById(attackerId, out entity, false))
            {
                MyFaction faction;
                MyPlayer playerFromCharacter = MyPlayer.GetPlayerFromCharacter(this);
                MyPlayer playerFromWeapon = null;
                if (ReferenceEquals(entity, this))
                {
                    return false;
                }
                switch (entity)
                {
                    case (MyCharacter _):
                        playerFromWeapon = MyPlayer.GetPlayerFromCharacter(entity as MyCharacter);
                        break;

                    case (IMyGunBaseUser _):
                        playerFromWeapon = MyPlayer.GetPlayerFromWeapon(entity as IMyGunBaseUser);
                        break;

                    case (MyHandDrill _):
                        playerFromWeapon = MyPlayer.GetPlayerFromCharacter((entity as MyHandDrill).Owner);
                        break;

                    case ((null) || (null)):
                        break;

                    default:
                        faction = MySession.Static.Factions.TryGetPlayerFaction(playerFromCharacter.Identity.IdentityId) as MyFaction;
                        if (((faction != null) && !faction.EnableFriendlyFire) && faction.IsMember(playerFromWeapon.Identity.IdentityId))
                        {
                            return false;
                        }
                        break;
                }
                if ((playerFromCharacter != null) && (playerFromWeapon != null))
                {
                    faction = MySession.Static.Factions.TryGetPlayerFaction(playerFromCharacter.Identity.IdentityId) as MyFaction;
                    if (((faction != null) && !faction.EnableFriendlyFire) && faction.IsMember(playerFromWeapon.Identity.IdentityId))
                    {
                        return false;
                    }
                    break;
                }
                else
                {
                    break;
                }
                if (((damage >= 0f) && (MySession.Static != null)) && (MyMusicController.Static != null))
                {
                    if ((ReferenceEquals(this, MySession.Static.LocalCharacter) && !(entity is MyVoxelPhysics)) && !(entity is MyCubeGrid))
                    {
                        MyMusicController.Static.Fighting(false, ((int) damage) * 3);
                    }
                    else if (ReferenceEquals(entity, MySession.Static.LocalCharacter))
                    {
                        MyMusicController.Static.Fighting(false, ((int) damage) * 2);
                    }
                    else if ((entity is IMyGunBaseUser) && ReferenceEquals((entity as IMyGunBaseUser).Owner as MyCharacter, MySession.Static.LocalCharacter))
                    {
                        MyMusicController.Static.Fighting(false, ((int) damage) * 2);
                    }
                    else if (ReferenceEquals(MySession.Static.ControlledEntity, entity))
                    {
                        MyMusicController.Static.Fighting(false, (int) damage);
                    }
                }
            }
            MyDamageInformation info = new MyDamageInformation(false, damage, damageType, attackerId);
            if ((this.UseDamageSystem && !this.m_dieAfterSimulation) && !this.IsDead)
            {
                MyDamageSystem.Static.RaiseBeforeDamageApplied(this, ref info);
            }
            if (info.Amount <= 0f)
            {
                return false;
            }
            this.StatComp.DoDamage(damage, info);
            MyAnalyticsHelper.SetLastDamageInformation(info);
            if (this.UseDamageSystem)
            {
                MyDamageSystem.Static.RaiseAfterDamageApplied(this, info);
            }
            return true;
        }

        public void Down()
        {
            if (!this.WantsFlyUp)
            {
                this.WantsFlyDown = true;
            }
            else
            {
                this.WantsFlyDown = false;
                this.WantsFlyUp = false;
            }
        }

        public void DrawHud(IMyCameraController camera, long playerId)
        {
            MyHud.Crosshair.Recenter();
            if (this.m_currentWeapon != null)
            {
                this.m_currentWeapon.DrawHud(camera, playerId);
            }
        }

        public void EnableAnimationCommands()
        {
            this.m_animationCommandsEnabled = true;
        }

        public void EnableBag(bool enabled)
        {
            this.m_enableBag = enabled;
            if (base.InScene && (this.Render.RenderObjectIDs[0] != uint.MaxValue))
            {
                Color? diffuseColor = null;
                float? emissivity = null;
                MyRenderProxy.UpdateModelProperties(this.Render.RenderObjectIDs[0], "Backpack", enabled ? RenderFlags.Visible : RenderFlags.SkipInDepth, enabled ? RenderFlags.SkipInDepth : RenderFlags.Visible, diffuseColor, emissivity);
            }
        }

        public void EnableBroadcasting(bool enable)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, bool>(this, x => new Action<bool>(x.EnableBroadcastingCallback), enable, targetEndpoint);
            if (!Sync.IsServer)
            {
                this.EnableBroadcastingCallback(enable);
            }
        }

        [Event(null, 0x1caa), Reliable, Server(ValidationType.Controlled), BroadcastExcept]
        public void EnableBroadcastingCallback(bool enable)
        {
            if ((this.RadioBroadcaster != null) && (this.RadioBroadcaster.WantsToBeEnabled != enable))
            {
                this.RadioBroadcaster.WantsToBeEnabled = enable;
                this.RadioBroadcaster.Enabled = enable;
            }
        }

        public void EnableHead(bool enabled)
        {
            if (base.InScene && (this.m_headRenderingEnabled != enabled))
            {
                this.UpdateHeadModelProperties(enabled);
            }
        }

        public void EnableIronsight(bool enable, bool newKeyPress, bool changeCamera, bool hideCrosshairWhenAiming = true)
        {
            if (Sync.IsServer || ReferenceEquals(MySession.Static.LocalCharacter, this))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCharacter, bool, bool, bool, bool, bool>(this, x => new Action<bool, bool, bool, bool, bool>(x.EnableIronsightCallback), enable, newKeyPress, changeCamera, hideCrosshairWhenAiming, false, targetEndpoint);
            }
            if (!Sync.IsServer)
            {
                this.EnableIronsightCallback(enable, newKeyPress, changeCamera, hideCrosshairWhenAiming, true);
            }
        }

        [Event(null, 0x1647), Reliable, Server(ValidationType.Controlled), BroadcastExcept]
        public void EnableIronsightCallback(bool enable, bool newKeyPress, bool changeCamera, bool hideCrosshairWhenAiming = true, bool forceChangeCamera = false)
        {
            if (!enable)
            {
                this.m_zoomMode = MyZoomModeEnum.Classic;
                this.ForceFirstPersonCamera = false;
                if (changeCamera && (MyEventContext.Current.IsLocallyInvoked | forceChangeCamera))
                {
                    MyHud.Crosshair.ResetToDefault(true);
                    MySector.MainCamera.Zoom.SetZoom(MyCameraZoomOperationType.ZoomingOut);
                    float headLocalXAngle = this.m_headLocalXAngle;
                    float headLocalYAngle = this.m_headLocalYAngle;
                    this.m_headLocalXAngle = headLocalXAngle;
                    this.m_headLocalYAngle = headLocalYAngle;
                }
            }
            else if ((this.m_currentWeapon != null) && (this.m_zoomMode != MyZoomModeEnum.IronSight))
            {
                this.m_zoomMode = MyZoomModeEnum.IronSight;
                if (changeCamera && (MyEventContext.Current.IsLocallyInvoked | forceChangeCamera))
                {
                    float headLocalXAngle = this.m_headLocalXAngle;
                    float headLocalYAngle = this.m_headLocalYAngle;
                    Vector3D? position = null;
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, this, position);
                    this.m_headLocalXAngle = headLocalXAngle;
                    this.m_headLocalYAngle = headLocalYAngle;
                    if (hideCrosshairWhenAiming)
                    {
                        MyHud.Crosshair.HideDefaultSprite();
                    }
                    MySector.MainCamera.Zoom.SetZoom(MyCameraZoomOperationType.ZoomingIn);
                }
            }
        }

        public void EnableLights(bool enable)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, bool>(this, x => new Action<bool>(x.EnableLightsCallback), enable, targetEndpoint);
            if (!Sync.IsServer)
            {
                this.EnableLightsCallback(enable);
            }
        }

        [Event(null, 0x1c94), Reliable, Server(ValidationType.Controlled), BroadcastExcept]
        private void EnableLightsCallback(bool enable)
        {
            if (this.m_lightEnabled != enable)
            {
                this.m_lightEnabled = enable;
                this.RecalculatePowerRequirement(false);
                if (this.Render != null)
                {
                    this.Render.UpdateLightPosition();
                }
            }
        }

        [Event(null, 0x2342), Reliable, Broadcast]
        private void EnablePhysics(bool enabled)
        {
            this.Physics.Enabled = enabled;
        }

        public void EndShoot(MyShootActionEnum action)
        {
            if (MySessionComponentReplay.Static.IsEntityBeingRecorded(base.EntityId))
            {
                PerFrameData data2 = new PerFrameData();
                ShootData data3 = new ShootData {
                    Begin = false,
                    ShootAction = (byte) action
                };
                data2.ShootData = new ShootData?(data3);
                PerFrameData data = data2;
                MySessionComponentReplay.Static.ProvideEntityRecordData(base.EntityId, data);
            }
            if ((ReferenceEquals(MySession.Static.LocalCharacter, this) && (this.m_currentMovementState != MyCharacterMovementEnum.Died)) && (this.m_currentWeapon != null))
            {
                if (((MyGuiScreenGamePlay.DoubleClickDetected == null) || !MyGuiScreenGamePlay.DoubleClickDetected[(int) action]) || !this.m_currentWeapon.CanDoubleClickToStick(action))
                {
                    this.EndShootSync(action);
                }
                else
                {
                    this.GunDoubleClickedSync(action);
                }
            }
            if (this.m_usingByPrimary)
            {
                this.m_usingByPrimary = false;
                this.UseFinished();
            }
        }

        private void EndShootAll()
        {
            foreach (MyShootActionEnum enum2 in MyEnum<MyShootActionEnum>.Values)
            {
                if (this.IsShooting(enum2))
                {
                    this.EndShoot(enum2);
                }
            }
        }

        private void EndShootInternal(MyShootActionEnum action = 0)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, MyShootActionEnum>(this, x => new Action<MyShootActionEnum>(x.ShootEndCallback), action, targetEndpoint);
            this.StopShooting(action);
        }

        public void EndShootSync(MyShootActionEnum action = 0)
        {
            if (!MyFakes.SIMULATE_QUICK_TRIGGER)
            {
                this.EndShootInternal(action);
            }
        }

        private void EquipWeapon(IMyHandheldGunObject<MyDeviceBase> newWeapon, bool showNotification = false)
        {
            if (newWeapon != null)
            {
                VRage.Game.Entity.MyEntity entity = (VRage.Game.Entity.MyEntity) newWeapon;
                entity.Render.CastShadows = true;
                entity.Render.NeedsResolveCastShadow = false;
                entity.Save = false;
                entity.OnClose += new Action<VRage.Game.Entity.MyEntity>(this.gunEntity_OnClose);
                MyAssetModifierComponent component = new MyAssetModifierComponent();
                entity.Components.Add<MyAssetModifierComponent>(component);
                Sandbox.Game.Entities.MyEntities.Add(entity, true);
                if (ReferenceEquals(MySession.Static.LocalCharacter, this))
                {
                    MyLocalCache.LoadInventoryConfig(entity, component);
                }
                this.m_handItemDefinition = null;
                this.m_currentWeapon = newWeapon;
                this.m_currentWeapon.OnControlAcquired(this);
                ((VRage.Game.Entity.MyEntity) this.m_currentWeapon).Render.DrawInAllCascades = true;
                if (this.ControllerInfo.IsLocallyControlled())
                {
                    MyHighlightSystem highlightSystem = MySession.Static.GetComponent<MyHighlightSystem>();
                    if ((this.m_currentWeapon != null) && (highlightSystem != null))
                    {
                        VRage.Game.Entity.MyEntity currentWeapon = (VRage.Game.Entity.MyEntity) this.m_currentWeapon;
                        currentWeapon.Render.RenderObjectIDs.ForEach<uint>(delegate (uint id) {
                            if (id != uint.MaxValue)
                            {
                                highlightSystem.AddHighlightOverlappingModel(id);
                            }
                        });
                        if (currentWeapon.Subparts != null)
                        {
                            foreach (KeyValuePair<string, MyEntitySubpart> pair in currentWeapon.Subparts)
                            {
                                Action<uint> <>9__1;
                                Action<uint> action = <>9__1;
                                if (<>9__1 == null)
                                {
                                    Action<uint> local1 = <>9__1;
                                    action = <>9__1 = delegate (uint id) {
                                        if (id != uint.MaxValue)
                                        {
                                            highlightSystem.AddHighlightOverlappingModel(id);
                                        }
                                    };
                                }
                                pair.Value.Render.RenderObjectIDs.ForEach<uint>(action);
                            }
                        }
                    }
                }
                if (this.WeaponEquiped != null)
                {
                    this.WeaponEquiped(this.m_currentWeapon);
                }
                if ((MyVisualScriptLogicProvider.ToolEquipped != null) && (this.ControllerInfo != null))
                {
                    long controllingIdentityId = this.ControllerInfo.ControllingIdentityId;
                    MyVisualScriptLogicProvider.ToolEquipped(controllingIdentityId, newWeapon.DefinitionId.TypeId.ToString(), newWeapon.DefinitionId.SubtypeName);
                }
                MyAnalyticsHelper.ReportActivityStart(this, "item_equip", "character", "toolbar_item_usage", this.m_currentWeapon.GetType().Name, true);
                if (this.m_currentWeapon.PhysicalObject != null)
                {
                    MyDefinitionId id = this.m_currentWeapon.PhysicalObject.GetId();
                    this.m_handItemDefinition = MyDefinitionManager.Static.TryGetHandItemForPhysicalItem(id);
                }
                else if (this.m_currentWeapon.DefinitionId.TypeId == typeof(MyObjectBuilder_CubePlacer))
                {
                    MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_CubePlacer));
                    this.m_handItemDefinition = MyDefinitionManager.Static.TryGetHandItemDefinition(ref id);
                }
                if ((this.m_handItemDefinition == null) || string.IsNullOrEmpty(this.m_handItemDefinition.FingersAnimation))
                {
                    if (this.m_handItemDefinition == null)
                    {
                        this.StopFingersAnimation(0f);
                    }
                    else if (base.UseNewAnimationSystem)
                    {
                        bool sync = ReferenceEquals(this, MySession.Static.LocalCharacter);
                        this.TriggerCharacterAnimationEvent("equip_left_tool", sync);
                        this.TriggerCharacterAnimationEvent("equip_right_tool", sync);
                        this.TriggerCharacterAnimationEvent(this.m_handItemDefinition.Id.SubtypeName.ToLower(), sync);
                        if (!string.IsNullOrEmpty(this.m_handItemDefinition.Id.SubtypeName))
                        {
                            base.AnimationController.Variables.SetValue(MyStringId.GetOrCompute(this.m_handItemDefinition.Id.TypeId.ToString().ToLower()), 1f);
                        }
                    }
                }
                else
                {
                    string fingersAnimation;
                    if (!this.m_characterDefinition.AnimationNameToSubtypeName.TryGetValue(this.m_handItemDefinition.FingersAnimation, out fingersAnimation))
                    {
                        fingersAnimation = this.m_handItemDefinition.FingersAnimation;
                    }
                    MyAnimationDefinition definition = MyDefinitionManager.Static.TryGetAnimationDefinition(fingersAnimation);
                    if (!definition.LeftHandItem.TypeId.IsNull)
                    {
                        this.m_currentWeapon.OnControlReleased();
                        (this.m_currentWeapon as VRage.Game.Entity.MyEntity).Close();
                        this.m_currentWeapon = null;
                    }
                    this.PlayCharacterAnimation(this.m_handItemDefinition.FingersAnimation, MyBlendOption.Immediate, definition.Loop ? MyFrameOption.Loop : MyFrameOption.PlayOnce, 1f, 1f, false, null, false);
                    if (base.UseNewAnimationSystem)
                    {
                        bool sync = ReferenceEquals(this, MySession.Static.LocalCharacter);
                        this.TriggerCharacterAnimationEvent("equip_left_tool", sync);
                        this.TriggerCharacterAnimationEvent("equip_right_tool", sync);
                        this.TriggerCharacterAnimationEvent(this.m_handItemDefinition.Id.SubtypeName.ToLower(), sync);
                        this.TriggerCharacterAnimationEvent(this.m_handItemDefinition.FingersAnimation.ToLower(), sync);
                        if (!string.IsNullOrEmpty(this.m_handItemDefinition.Id.SubtypeName))
                        {
                            base.AnimationController.Variables.SetValue(MyStringId.GetOrCompute(this.m_handItemDefinition.Id.TypeId.ToString().ToLower()), 1f);
                        }
                    }
                    if (!definition.LeftHandItem.TypeId.IsNull)
                    {
                        if (this.m_leftHandItem != null)
                        {
                            (this.m_leftHandItem as IMyHandheldGunObject<MyDeviceBase>).OnControlReleased();
                            this.m_leftHandItem.Close();
                        }
                        uint? inventoryItemId = null;
                        MyObjectBuilder_EntityBase gunEntity = this.GetObjectBuilderForWeapon(new MyDefinitionId?(definition.LeftHandItem), ref inventoryItemId, MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM));
                        IMyHandheldGunObject<MyDeviceBase> obj2 = CreateGun(gunEntity, inventoryItemId);
                        if (obj2 != null)
                        {
                            this.m_leftHandItem = obj2 as VRage.Game.Entity.MyEntity;
                            this.m_leftHandItem.Render.DrawInAllCascades = true;
                            obj2.OnControlAcquired(this);
                            this.UpdateLeftHandItemPosition();
                            Sandbox.Game.Entities.MyEntities.Add(this.m_leftHandItem, true);
                        }
                    }
                }
                MyResourceSinkComponent sink = entity.Components.Get<MyResourceSinkComponent>();
                if ((sink != null) && (this.SuitRechargeDistributor != null))
                {
                    this.SuitRechargeDistributor.AddSink(sink);
                }
                if (showNotification)
                {
                    MyHudNotification notification = new MyHudNotification(MySpaceTexts.NotificationUsingWeaponType, 0x7d0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                    object[] arguments = new object[] { MyDeviceBase.GetGunNotificationName(newWeapon.DefinitionId) };
                    notification.SetTextFormatArguments(arguments);
                    MyHud.Notifications.Add(notification);
                }
                this.Static_CameraAttachedToChanged(null, null);
                if (!(this.IsUsing is MyCockpit))
                {
                    MyHud.Crosshair.ResetToDefault(false);
                }
            }
        }

        private Vector3 FilterLocalSpeed(Vector3 localSpeedWorldRotUnfiltered) => 
            localSpeedWorldRotUnfiltered;

        private int FindBestBone(int capsuleIndex, ref Vector3D hitPosition, ref MatrixD worldMatrix)
        {
            MyBoneCapsuleInfo info = this.m_bodyCapsuleInfo[capsuleIndex];
            CapsuleD ed = this.m_bodyCapsules[capsuleIndex];
            MyCharacterBone bone = base.AnimationController.CharacterBones[info.AscendantBone];
            Vector3D vectord = Vector3.Normalize(ed.P0 - ed.P1);
            double num = vectord.Length();
            double num2 = Vector3D.Dot(hitPosition - ed.P1, vectord) / num;
            MyCharacterBone bone1 = base.AnimationController.CharacterBones[info.DescendantBone];
            int index = bone1.Index;
            double num4 = 0.0;
            MyCharacterBone parent = bone1.Parent;
            while (true)
            {
                if ((num2 >= num4) && (index != bone.Index))
                {
                    num4 = Vector3D.Dot(Vector3D.Transform(parent.AbsoluteTransform.Translation, ref worldMatrix) - ed.P1, vectord) / num;
                    index = parent.Index;
                    if (parent.Parent != null)
                    {
                        continue;
                    }
                }
                return index;
            }
        }

        private MyLadder FindLadder(ref Vector3D from, ref Vector3D to)
        {
            bool flag;
            return this.FindLadder(ref from, ref to, out flag);
        }

        private MyLadder FindLadder(ref Vector3D from, ref Vector3D to, out bool isHit)
        {
            isHit = false;
            LineD line = new LineD(from, to);
            MyIntersectionResultLineTriangleEx? nullable = Sandbox.Game.Entities.MyEntities.GetIntersectionWithLine(ref line, this, null, false, false, true, IntersectionFlags.ALL_TRIANGLES, 0f, true);
            MyLadder objA = null;
            if (nullable != null)
            {
                isHit = true;
                if (!(nullable.Value.Entity is MyCubeGrid))
                {
                    MyLadder entity = nullable.Value.Entity as MyLadder;
                    if (entity != null)
                    {
                        objA = entity;
                    }
                }
                else if (nullable.Value.UserObject != null)
                {
                    MySlimBlock cubeBlock = (nullable.Value.UserObject as MyCube).CubeBlock;
                    if ((cubeBlock != null) && (cubeBlock.FatBlock != null))
                    {
                        MyLadder fatBlock = cubeBlock.FatBlock as MyLadder;
                        if (fatBlock != null)
                        {
                            objA = fatBlock;
                        }
                    }
                }
            }
            if (objA == null)
            {
                return null;
            }
            if (this.Ladder != null)
            {
                if (ReferenceEquals(objA, this.Ladder))
                {
                    return objA;
                }
                if (!ReferenceEquals(objA.GetTopMostParent(null), this.Ladder.GetTopMostParent(null)))
                {
                    return objA;
                }
                if (objA.Orientation.Forward != this.Ladder.Orientation.Forward)
                {
                    return null;
                }
            }
            return objA;
        }

        public MyPhysicalInventoryItem? FindWeaponItemByDefinition(MyDefinitionId weaponDefinition)
        {
            MyPhysicalInventoryItem? nullable = null;
            MyDefinitionId? nullable2 = MyDefinitionManager.Static.ItemIdFromWeaponId(weaponDefinition);
            if ((nullable2 != null) && (this.GetInventory(0) != null))
            {
                nullable = this.GetInventory(0).FindUsableItem(nullable2.Value);
            }
            return nullable;
        }

        public void ForceUpdateBreath()
        {
            if (this.m_breath != null)
            {
                this.m_breath.ForceUpdate();
            }
        }

        public MatrixD Get3rdBoneMatrix(bool includeY, bool includeX = true) => 
            this.GetHeadMatrixInternal(this.m_camera3rdBoneIndex, includeY, includeX, false, false);

        public MatrixD Get3rdCameraMatrix(bool includeY, bool includeX = true) => 
            Matrix.Invert((Matrix) this.Get3rdBoneMatrix(includeY, includeX));

        private Vector3D GetAimedPointFromCamera()
        {
            MatrixD xd2;
            if (!ReferenceEquals(MySession.Static.ControlledEntity, this))
            {
                return this.m_aimedPoint;
            }
            MatrixD.Invert(ref this.GetViewMatrix(), out xd2);
            Vector3D forward = xd2.Forward;
            forward.Normalize();
            Vector3D translation = xd2.Translation;
            translation += forward * (this.GetHeadMatrix(false, false, false, false, false).Translation - translation).Dot(forward);
            Vector3D position = (this.WeaponPosition != null) ? (this.WeaponPosition.LogicalPositionWorld + (forward * 25000.0)) : (translation + (forward * 25000.0));
            if (ReferenceEquals(MySession.Static.ControlledEntity, this))
            {
                if (this.m_raycastList == null)
                {
                    this.m_raycastList = new List<MyPhysics.HitInfo>();
                }
                this.m_raycastList.Clear();
                MyPhysics.CastRay(translation, translation + (forward * 100.0), this.m_raycastList, 0);
                foreach (MyPhysics.HitInfo info in this.m_raycastList)
                {
                    VRage.ModAPI.IMyEntity hitEntity = info.HkHitInfo.GetHitEntity();
                    if (!ReferenceEquals(hitEntity, this) && !ReferenceEquals(hitEntity, this.CurrentWeapon))
                    {
                        position = info.Position;
                        break;
                    }
                }
            }
            return position;
        }

        private Vector3D GetAimedPointFromHead()
        {
            MatrixD xd = this.GetHeadMatrix(false, true, false, false, false);
            return (xd.Translation + (xd.Forward * 25000.0));
        }

        public MyEntityCameraSettings GetCameraEntitySettings() => 
            this.m_cameraSettingsWhenAlive;

        public MyPlayer.PlayerId GetClientIdentity()
        {
            MyPlayer.PlayerId id;
            MyPlayer player = this.TryGetPlayer();
            if (player != null)
            {
                return player.Id;
            }
            MySession.Static.Players.TryGetPlayerId(this.GetPlayerIdentityId(), out id);
            return id;
        }

        public MyCharacterMovementEnum GetCurrentMovementState() => 
            this.m_currentMovementState;

        private DamageImpactEnum GetDamageFromFall(HkRigidBody collidingBody, VRage.Game.Entity.MyEntity collidingEntity, ref HkContactPointEvent value)
        {
            float num = Vector3.Dot(value.ContactPoint.Normal, Vector3.Normalize(this.Physics.HavokWorld.Gravity));
            return (!(num != 0f) ? ((Math.Abs((float) (value.SeparatingVelocity * num)) >= MyPerGameSettings.CharacterDamageMinVelocity) ? ((Math.Abs((float) (value.SeparatingVelocity * num)) <= MyPerGameSettings.CharacterDamageDeadlyDamageVelocity) ? ((Math.Abs((float) (value.SeparatingVelocity * num)) <= MyPerGameSettings.CharacterDamageMediumDamageVelocity) ? DamageImpactEnum.SmallDamage : DamageImpactEnum.MediumDamage) : DamageImpactEnum.DeadlyDamage) : DamageImpactEnum.NoDamage) : DamageImpactEnum.NoDamage);
        }

        private DamageImpactEnum GetDamageFromHit(HkRigidBody collidingBody, VRage.Game.Entity.MyEntity collidingEntity, ref HkContactPointEvent value)
        {
            if (collidingBody.LinearVelocity.Length() < MyPerGameSettings.CharacterDamageHitObjectMinVelocity)
            {
                return DamageImpactEnum.NoDamage;
            }
            if (ReferenceEquals(collidingEntity, this.ManipulatedEntity))
            {
                return DamageImpactEnum.NoDamage;
            }
            if (collidingBody.HasProperty(HkCharacterRigidBody.MANIPULATED_OBJECT))
            {
                return DamageImpactEnum.NoDamage;
            }
            float num = MyPerGameSettings.Destruction ? MyDestructionHelper.MassFromHavok(collidingBody.Mass) : collidingBody.Mass;
            if (num < MyPerGameSettings.CharacterDamageHitObjectMinMass)
            {
                return DamageImpactEnum.NoDamage;
            }
            float num2 = Math.Abs(value.SeparatingVelocity) * num;
            return ((num2 <= MyPerGameSettings.CharacterDamageHitObjectDeadlyEnergy) ? ((num2 <= MyPerGameSettings.CharacterDamageHitObjectCriticalEnergy) ? ((num2 <= MyPerGameSettings.CharacterDamageHitObjectMediumEnergy) ? ((num2 <= MyPerGameSettings.CharacterDamageHitObjectSmallEnergy) ? DamageImpactEnum.NoDamage : DamageImpactEnum.SmallDamage) : DamageImpactEnum.MediumDamage) : DamageImpactEnum.CriticalDamage) : DamageImpactEnum.DeadlyDamage);
        }

        private DamageImpactEnum GetDamageFromSqueeze(HkRigidBody collidingBody, VRage.Game.Entity.MyEntity collidingEntity, ref HkContactPointEvent value)
        {
            if (collidingBody.IsFixed || (collidingBody.Mass < MyPerGameSettings.CharacterSqueezeMinMass))
            {
                return DamageImpactEnum.NoDamage;
            }
            if (value.ContactProperties.IsNew)
            {
                return DamageImpactEnum.NoDamage;
            }
            Vector3 vector = this.Physics.CharacterProxy.Position - collidingBody.Position;
            vector.Normalize();
            Vector3 gravity = this.m_gravity;
            gravity.Normalize();
            if (Vector3.Dot(vector, gravity) < 0.5f)
            {
                return DamageImpactEnum.NoDamage;
            }
            if (this.m_squeezeDamageTimer > 0f)
            {
                this.m_squeezeDamageTimer -= 0.01666667f;
                return DamageImpactEnum.NoDamage;
            }
            this.m_squeezeDamageTimer = MyPerGameSettings.CharacterSqueezeDamageDelay;
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_SHOW_DAMAGE)
            {
                MatrixD worldMatrix = collidingEntity.Physics.GetWorldMatrix();
                int shapeIndex = 2;
                MyPhysicsDebugDraw.DrawCollisionShape(collidingBody.GetShape(), worldMatrix, 1f, ref shapeIndex, null, false);
                MyRenderProxy.DebugDrawText3D(worldMatrix.Translation, "SQUEEZE, MASS:" + collidingBody.Mass, Color.Yellow, 2f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            return ((collidingBody.Mass <= MyPerGameSettings.CharacterSqueezeDeadlyDamageMass) ? ((collidingBody.Mass <= MyPerGameSettings.CharacterSqueezeCriticalDamageMass) ? ((collidingBody.Mass <= MyPerGameSettings.CharacterSqueezeMediumDamageMass) ? DamageImpactEnum.SmallDamage : DamageImpactEnum.MediumDamage) : DamageImpactEnum.CriticalDamage) : DamageImpactEnum.DeadlyDamage);
        }

        private Action<MyCharacter> GetDelegate(Action<IMyCharacter> value) => 
            ((Action<MyCharacter>) Delegate.CreateDelegate(typeof(Action<MyCharacter>), value.Target, value.Method));

        public MatrixD GetHeadMatrix(bool includeY, bool includeX = true, bool forceHeadAnim = false, bool forceHeadBone = false, bool preferLocalOverSync = false)
        {
            int headBone = (this.IsInFirstPersonView | forceHeadBone) ? this.m_headBoneIndex : this.m_camera3rdBoneIndex;
            return this.GetHeadMatrixInternal(headBone, includeY, includeX, forceHeadAnim, forceHeadBone);
        }

        private unsafe MatrixD GetHeadMatrixInternal(int headBone, bool includeY, bool includeX = true, bool forceHeadAnim = false, bool forceHeadBone = false)
        {
            if (base.PositionComp == null)
            {
                return MatrixD.Identity;
            }
            MatrixD identity = MatrixD.Identity;
            bool flag = (this.ShouldUseAnimatedHeadRotation() && (!this.JetpackRunning || this.IsLocalHeadAnimationInProgress())) | forceHeadAnim;
            if (includeX && !flag)
            {
                identity = MatrixD.CreateFromAxisAngle(Vector3D.Right, (double) MathHelper.ToRadians(this.m_headLocalXAngle));
            }
            if (includeY)
            {
                identity *= Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(this.m_headLocalYAngle));
            }
            Vector3 zero = Vector3.Zero;
            if (headBone != -1)
            {
                zero = base.BoneAbsoluteTransforms[headBone].Translation;
                float num = 1f - ((float) Math.Cos((double) MathHelper.ToRadians(this.m_headLocalXAngle)));
                float* singlePtr1 = (float*) ref zero.Y;
                singlePtr1[0] += num * base.AnimationController.InverseKinematics.RootBoneVerticalOffset;
            }
            if ((!flag || ((headBone == -1) || ((base.BoneAbsoluteTransforms[headBone].Right.LengthSquared() <= float.Epsilon) || (base.BoneAbsoluteTransforms[headBone].Up.LengthSquared() <= float.Epsilon)))) || (base.BoneAbsoluteTransforms[headBone].Forward.LengthSquared() <= float.Epsilon))
            {
                this.m_headMatrix = MatrixD.CreateTranslation(0.0, (double) zero.Y, (double) zero.Z);
            }
            else
            {
                Matrix matrix = Matrix.Identity;
                matrix.Translation = zero;
                this.m_headMatrix = MatrixD.CreateRotationX(-1.5707963267948966) * matrix;
            }
            if (this.IsInFirstPersonView && !MyFakes.MULTIPLAYER_CLIENT_SIMULATE_CONTROLLED_CHARACTER)
            {
                float num2 = Math.Abs(this.m_headMovementXOffset);
                float num3 = 0.03f;
                if ((num2 > 0f) && (num2 < (this.m_maxHeadMovementOffset / 2f)))
                {
                    MatrixD* xdPtr1 = (MatrixD*) ref this.m_headMatrix;
                    xdPtr1.Translation += ((num3 * this.m_headMatrix.Up) * num2) * (Math.Sin(10.0 * MySandboxGame.Static.SimulationTime.Seconds) + 3.0);
                }
                else if (num2 > 0f)
                {
                    MatrixD* xdPtr2 = (MatrixD*) ref this.m_headMatrix;
                    xdPtr2.Translation += ((num3 * this.m_headMatrix.Up) * (this.m_maxHeadMovementOffset - num2)) * (Math.Sin(10.0 * MySandboxGame.Static.SimulationTime.Seconds) + 3.0);
                }
                float num4 = Math.Abs(this.m_headMovementYOffset);
                if ((num4 > 0f) && (num4 < (this.m_maxHeadMovementOffset / 2f)))
                {
                    MatrixD* xdPtr3 = (MatrixD*) ref this.m_headMatrix;
                    xdPtr3.Translation += ((num3 * this.m_headMatrix.Up) * num4) * (Math.Sin(10.0 * MySandboxGame.Static.SimulationTime.Seconds) + 3.0);
                }
                else if (num4 > 0f)
                {
                    MatrixD* xdPtr4 = (MatrixD*) ref this.m_headMatrix;
                    xdPtr4.Translation += ((num3 * this.m_headMatrix.Up) * (this.m_maxHeadMovementOffset - num4)) * (Math.Sin(10.0 * MySandboxGame.Static.SimulationTime.Seconds) + 3.0);
                }
            }
            MatrixD xd2 = (identity * this.m_headMatrix) * base.WorldMatrix;
            MatrixD xd3 = MatrixD.CreateFromDir(base.WorldMatrix.Forward, base.WorldMatrix.Up);
            MatrixD xd4 = (this.m_headMatrix * identity) * xd3;
            xd4.Translation = xd2.Translation;
            return xd4;
        }

        public override List<MyHudEntityParams> GetHudParams(bool allowBlink)
        {
            this.UpdateCustomNameWithFaction();
            base.m_hudParams.Clear();
            if (MySession.Static.LocalHumanPlayer != null)
            {
                MyHudEntityParams item = new MyHudEntityParams {
                    FlagsEnum = MyHudIndicatorFlagsEnum.SHOW_TEXT,
                    Text = this.CustomNameWithFaction,
                    ShouldDraw = new Func<bool>(MyHud.CheckShowPlayerNamesOnHud),
                    Owner = this.GetPlayerIdentityId(),
                    Share = MyOwnershipShareModeEnum.Faction,
                    Entity = this
                };
                base.m_hudParams.Add(item);
            }
            return base.m_hudParams;
        }

        public MyIdentity GetIdentity()
        {
            MyPlayer player = this.TryGetPlayer();
            return ((player == null) ? MySession.Static.Players.TryGetIdentity(this.GetPlayerIdentityId()) : player.Identity);
        }

        private MyCharacterMovementEnum GetIdleState() => 
            (this.WantsCrouch ? MyCharacterMovementEnum.Crouching : MyCharacterMovementEnum.Standing);

        public bool GetIntersectionWithLine(ref LineD line, ref MyCharacterHitInfo info, IntersectionFlags flags = 3)
        {
            if (info == null)
            {
                info = new MyCharacterHitInfo();
            }
            info.Reset();
            if (this.UpdateCapsuleBones())
            {
                double maxValue = double.MaxValue;
                Vector3D zero = Vector3D.Zero;
                Vector3D vectord2 = Vector3D.Zero;
                Vector3 vector = Vector3.Zero;
                Vector3 vector2 = Vector3.Zero;
                int capsuleIndex = -1;
                for (int i = 0; i < this.m_bodyCapsules.Length; i++)
                {
                    CapsuleD ed = this.m_bodyCapsules[i];
                    if (ed.Intersect(line, ref zero, ref vectord2, ref vector, ref vector2))
                    {
                        double num4 = Vector3.Distance((Vector3) zero, (Vector3) line.From);
                        if (num4 < maxValue)
                        {
                            maxValue = num4;
                            capsuleIndex = i;
                        }
                    }
                }
                if (capsuleIndex != -1)
                {
                    MyIntersectionResultLineTriangleEx? nullable;
                    MatrixD worldMatrix = base.PositionComp.WorldMatrix;
                    int index = this.FindBestBone(capsuleIndex, ref zero, ref worldMatrix);
                    MatrixD worldMatrixNormalizedInv = base.PositionComp.WorldMatrixNormalizedInv;
                    Vector3D vectord3 = Vector3D.Transform(line.From, ref worldMatrixNormalizedInv);
                    Vector3D position = Vector3D.Transform(line.To, ref worldMatrixNormalizedInv);
                    Line line2 = new Line((Vector3) vectord3, (Vector3) position, true);
                    MyCharacterBone bone1 = base.AnimationController.CharacterBones[index];
                    bone1.ComputeAbsoluteTransform(true);
                    Matrix absoluteTransform = bone1.AbsoluteTransform;
                    Matrix matrix = bone1.SkinTransform * absoluteTransform;
                    Matrix matrix3 = Matrix.Invert(matrix);
                    position = Vector3.Transform((Vector3) position, ref matrix3);
                    Vector3D from = Vector3D.Transform(Vector3.Transform((Vector3) vectord3, ref matrix3), ref worldMatrix);
                    LineD ed2 = new LineD(from, Vector3D.Transform(position, ref worldMatrix));
                    if (base.GetIntersectionWithLine(ref ed2, out nullable, flags))
                    {
                        MyIntersectionResultLineTriangleEx ex = nullable.Value;
                        info.CapsuleIndex = capsuleIndex;
                        info.BoneIndex = index;
                        info.Capsule = this.m_bodyCapsules[info.CapsuleIndex];
                        info.HitHead = (info.CapsuleIndex == 0) && (this.m_bodyCapsules.Length > 1);
                        info.HitPositionBindingPose = ex.IntersectionPointInObjectSpace;
                        info.HitNormalBindingPose = ex.NormalInObjectSpace;
                        info.BindingTransformation = matrix;
                        MyTriangle_Vertices triangle = new MyTriangle_Vertices {
                            Vertex0 = Vector3.Transform(ex.Triangle.InputTriangle.Vertex0, ref matrix),
                            Vertex1 = Vector3.Transform(ex.Triangle.InputTriangle.Vertex1, ref matrix),
                            Vertex2 = Vector3.Transform(ex.Triangle.InputTriangle.Vertex2, ref matrix)
                        };
                        Vector3 triangleNormal = Vector3.TransformNormal(ex.Triangle.InputTriangleNormal, matrix);
                        Vector3 vector4 = Vector3.Transform(ex.IntersectionPointInObjectSpace, ref matrix);
                        Vector3 normal = Vector3.TransformNormal(ex.NormalInObjectSpace, matrix);
                        ex = new MyIntersectionResultLineTriangleEx();
                        ex.Triangle = new MyIntersectionResultLineTriangle(ex.Triangle.TriangleIndex, ref triangle, ref ex.Triangle.BoneWeights, ref triangleNormal, ex.Triangle.Distance);
                        ex.IntersectionPointInObjectSpace = vector4;
                        ex.NormalInObjectSpace = normal;
                        ex.IntersectionPointInWorldSpace = Vector3D.Transform(vector4, ref worldMatrix);
                        ex.NormalInWorldSpace = Vector3.TransformNormal(normal, worldMatrix);
                        ex.InputLineInObjectSpace = line2;
                        ex.Entity = nullable.Value.Entity;
                        info.Triangle = ex;
                        if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_MISC)
                        {
                            MyRenderProxy.DebugClearPersistentMessages();
                            MyRenderProxy.DebugDrawCapsule(info.Capsule.P0, info.Capsule.P1, info.Capsule.Radius, Color.Aqua, false, false, true);
                            Vector3 vector6 = (Vector3) Vector3D.Transform(info.Capsule.P0, ref worldMatrixNormalizedInv);
                            Vector3 vector8 = Vector3.Transform((Vector3) Vector3D.Transform(info.Capsule.P1, ref worldMatrixNormalizedInv), ref matrix3);
                            Vector3D vectord5 = Vector3D.Transform(Vector3.Transform(vector6, ref matrix3), ref worldMatrix);
                            MyRenderProxy.DebugDrawCapsule(vectord5, Vector3D.Transform(vector8, ref worldMatrix), info.Capsule.Radius, Color.Brown, false, false, true);
                            MyRenderProxy.DebugDrawLine3D(line.From, line.To, Color.Blue, Color.Red, false, true);
                            MyRenderProxy.DebugDrawLine3D(ed2.From, ed2.To, Color.Green, Color.Yellow, false, true);
                            MyRenderProxy.DebugDrawSphere(ex.IntersectionPointInWorldSpace, 0.02f, Color.Red, 1f, false, false, true, true);
                            MyRenderProxy.DebugDrawAxis(matrix * base.WorldMatrix, 0.1f, false, true, true);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool GetIntersectionWithLine(ref LineD line, out MyIntersectionResultLineTriangleEx? tri, IntersectionFlags flags = 3)
        {
            tri = new MyIntersectionResultLineTriangleEx?(m_hitInfoTmp.Triangle);
            return this.GetIntersectionWithLine(ref line, ref m_hitInfoTmp, flags);
        }

        public Vector3 GetLocalWeaponPosition() => 
            ((Vector3) this.WeaponPosition.LogicalPositionLocalSpace);

        public static void GetModelAndDefinition(MyObjectBuilder_Character characterOb, out string characterModel, out MyCharacterDefinition characterDefinition, ref Vector3 colorMask)
        {
            characterModel = GetRealModel(characterOb.CharacterModel, ref colorMask);
            characterDefinition = null;
            if (string.IsNullOrEmpty(characterModel) || !MyDefinitionManager.Static.Characters.TryGetValue(characterModel, out characterDefinition))
            {
                characterDefinition = MyDefinitionManager.Static.Characters.First<MyCharacterDefinition>();
                characterModel = characterDefinition.Model;
            }
        }

        private float GetMovementAcceleration(MyCharacterMovementEnum movement)
        {
            if (movement > MyCharacterMovementEnum.WalkStrafingRight)
            {
                if (movement > MyCharacterMovementEnum.Backrunning)
                {
                    if (movement > MyCharacterMovementEnum.RunningLeftBack)
                    {
                        if (movement > MyCharacterMovementEnum.RunningRightFront)
                        {
                            if (movement != MyCharacterMovementEnum.RunningRightBack)
                            {
                                if (movement == MyCharacterMovementEnum.Sprinting)
                                {
                                    return MyPerGameSettings.CharacterMovement.SprintAcceleration;
                                }
                                goto TR_0000;
                            }
                        }
                        else if (movement == MyCharacterMovementEnum.RunStrafingRight)
                        {
                            goto TR_0013;
                        }
                        else if (movement != MyCharacterMovementEnum.RunningRightFront)
                        {
                            goto TR_0000;
                        }
                    }
                    else if (movement == MyCharacterMovementEnum.RunStrafingLeft)
                    {
                        goto TR_000E;
                    }
                    else if ((movement != MyCharacterMovementEnum.RunningLeftFront) && (movement != MyCharacterMovementEnum.RunningLeftBack))
                    {
                        goto TR_0000;
                    }
                    goto TR_0006;
                }
                else if (movement > MyCharacterMovementEnum.CrouchWalkingRightFront)
                {
                    if (movement > MyCharacterMovementEnum.CrouchWalkingRightBack)
                    {
                        if ((movement != MyCharacterMovementEnum.Running) && (movement != MyCharacterMovementEnum.Backrunning))
                        {
                            goto TR_0000;
                        }
                    }
                    else if ((movement != MyCharacterMovementEnum.WalkingRightBack) && (movement != MyCharacterMovementEnum.CrouchWalkingRightBack))
                    {
                        goto TR_0000;
                    }
                    goto TR_0006;
                }
                else if (movement == MyCharacterMovementEnum.CrouchStrafingRight)
                {
                    goto TR_0013;
                }
                else if ((movement == MyCharacterMovementEnum.WalkingRightFront) || (movement == MyCharacterMovementEnum.CrouchWalkingRightFront))
                {
                    goto TR_0006;
                }
                goto TR_0000;
            }
            else
            {
                if (movement > MyCharacterMovementEnum.CrouchBackWalking)
                {
                    if (movement > MyCharacterMovementEnum.WalkingLeftFront)
                    {
                        if (movement > MyCharacterMovementEnum.WalkingLeftBack)
                        {
                            if (movement == MyCharacterMovementEnum.CrouchWalkingLeftBack)
                            {
                                goto TR_0006;
                            }
                            else if (movement == MyCharacterMovementEnum.WalkStrafingRight)
                            {
                                goto TR_0013;
                            }
                        }
                        else if ((movement == MyCharacterMovementEnum.CrouchWalkingLeftFront) || (movement == MyCharacterMovementEnum.WalkingLeftBack))
                        {
                            goto TR_0006;
                        }
                    }
                    else if (movement == MyCharacterMovementEnum.WalkStrafingLeft)
                    {
                        goto TR_000E;
                    }
                    else if (movement == MyCharacterMovementEnum.CrouchStrafingLeft)
                    {
                        goto TR_000E;
                    }
                    else if (movement == MyCharacterMovementEnum.WalkingLeftFront)
                    {
                        goto TR_0006;
                    }
                }
                else if (movement > MyCharacterMovementEnum.Jump)
                {
                    if (movement > MyCharacterMovementEnum.CrouchWalking)
                    {
                        if ((movement == MyCharacterMovementEnum.BackWalking) || (movement == MyCharacterMovementEnum.CrouchBackWalking))
                        {
                            goto TR_0006;
                        }
                    }
                    else if ((movement == MyCharacterMovementEnum.Walking) || (movement == MyCharacterMovementEnum.CrouchWalking))
                    {
                        goto TR_0006;
                    }
                }
                else if (movement == MyCharacterMovementEnum.Standing)
                {
                    goto TR_0003;
                }
                else if (movement == MyCharacterMovementEnum.Crouching)
                {
                    goto TR_0003;
                }
                else if (movement == MyCharacterMovementEnum.Jump)
                {
                    return 0f;
                }
                goto TR_0000;
            }
            goto TR_0006;
        TR_0000:
            return 0f;
        TR_0003:
            return MyPerGameSettings.CharacterMovement.WalkAcceleration;
        TR_0006:
            return MyPerGameSettings.CharacterMovement.WalkAcceleration;
        TR_000E:
            return MyPerGameSettings.CharacterMovement.WalkAcceleration;
        TR_0013:
            return MyPerGameSettings.CharacterMovement.WalkAcceleration;
        }

        public void GetNetState(out MyCharacterClientState state)
        {
            Vector3 zero;
            state.HeadX = this.HeadLocalXAngle;
            state.HeadY = this.HeadLocalYAngle;
            state.MovementState = this.GetCurrentMovementState();
            state.MovementFlags = this.MovementFlags;
            bool flag = this.JetpackComp != null;
            state.Jetpack = flag && this.JetpackComp.TurnedOn;
            state.Dampeners = flag && this.JetpackComp.DampenersTurnedOn;
            state.TargetFromCamera = this.TargetFromCamera;
            state.MoveIndicator = this.MoveIndicator;
            Quaternion quaternion = Quaternion.CreateFromRotationMatrix(this.Entity.WorldMatrix);
            state.Rotation = quaternion;
            state.CharacterState = this.m_currentCharacterState;
            if ((this.Physics == null) || (this.Physics.CharacterProxy == null))
            {
                zero = Vector3.Zero;
            }
            else
            {
                zero = this.Physics.CharacterProxy.SupportNormal;
            }
            state.SupportNormal = zero;
            state.MovementSpeed = this.m_currentSpeed;
            state.MovementDirection = this.m_currentMovementDirection;
            state.IsOnLadder = this.IsOnLadder;
            state.Valid = true;
        }

        public MyCharacterMovementEnum GetNetworkMovementState() => 
            this.m_previousNetworkMovementState;

        private MyCharacterMovementEnum GetNewMovementState(ref Vector3 moveIndicator, ref Vector2 rotationIndicator, ref float acceleration, bool sprint, bool walk, bool canMove, bool movementFlagsChanged)
        {
            if (this.m_currentMovementState == MyCharacterMovementEnum.Died)
            {
                return MyCharacterMovementEnum.Died;
            }
            MyCharacterMovementEnum currentMovementState = this.m_currentMovementState;
            if (this.Definition.UseOnlyWalking)
            {
                walk = true;
            }
            if (this.m_currentJumpTime > 0f)
            {
                return MyCharacterMovementEnum.Jump;
            }
            if (this.JetpackRunning)
            {
                return MyCharacterMovementEnum.Flying;
            }
            bool flag = true;
            bool flag2 = true;
            bool flag3 = true;
            bool flag4 = true;
            bool continuous = false;
            bool flag6 = false;
            bool flag7 = false;
            MyCharacterMovementEnum enum3 = this.m_currentMovementState;
            if (enum3 > MyCharacterMovementEnum.LadderUp)
            {
                if (enum3 > MyCharacterMovementEnum.RunningLeftBack)
                {
                    if (enum3 > MyCharacterMovementEnum.RunningRightFront)
                    {
                        if (enum3 != MyCharacterMovementEnum.RunningRightBack)
                        {
                            if (enum3 == MyCharacterMovementEnum.Sprinting)
                            {
                                flag7 = true;
                            }
                            else if (enum3 == MyCharacterMovementEnum.LadderOut)
                            {
                                return currentMovementState;
                            }
                            goto TR_0048;
                        }
                    }
                    else if ((enum3 != MyCharacterMovementEnum.RunStrafingRight) && (enum3 != MyCharacterMovementEnum.RunningRightFront))
                    {
                        goto TR_0048;
                    }
                    goto TR_0057;
                }
                else
                {
                    if (enum3 > MyCharacterMovementEnum.Running)
                    {
                        if (((enum3 != MyCharacterMovementEnum.RunStrafingLeft) && (enum3 != MyCharacterMovementEnum.RunningLeftFront)) && (enum3 != MyCharacterMovementEnum.RunningLeftBack))
                        {
                            goto TR_0048;
                        }
                        goto TR_0057;
                    }
                    else
                    {
                        if (enum3 == MyCharacterMovementEnum.LadderDown)
                        {
                            return currentMovementState;
                        }
                        else if (enum3 != MyCharacterMovementEnum.Running)
                        {
                            goto TR_0048;
                        }
                        goto TR_0057;
                    }
                    return currentMovementState;
                }
            }
            else
            {
                if (enum3 > MyCharacterMovementEnum.WalkingLeftFront)
                {
                    if (enum3 > MyCharacterMovementEnum.WalkStrafingRight)
                    {
                        if ((enum3 != MyCharacterMovementEnum.WalkingRightFront) && (enum3 != MyCharacterMovementEnum.WalkingRightBack))
                        {
                            if (enum3 == MyCharacterMovementEnum.LadderUp)
                            {
                                return currentMovementState;
                            }
                            goto TR_0048;
                        }
                    }
                    else if ((enum3 != MyCharacterMovementEnum.WalkingLeftBack) && (enum3 != MyCharacterMovementEnum.WalkStrafingRight))
                    {
                        goto TR_0048;
                    }
                }
                else
                {
                    if (enum3 > MyCharacterMovementEnum.Walking)
                    {
                        if ((enum3 != MyCharacterMovementEnum.WalkStrafingLeft) && (enum3 != MyCharacterMovementEnum.WalkingLeftFront))
                        {
                            goto TR_0048;
                        }
                        goto TR_0049;
                    }
                    else
                    {
                        if (enum3 == MyCharacterMovementEnum.Ladder)
                        {
                            return currentMovementState;
                        }
                        else if (enum3 != MyCharacterMovementEnum.Walking)
                        {
                            goto TR_0048;
                        }
                        goto TR_0049;
                    }
                    return currentMovementState;
                }
                goto TR_0049;
            }
        TR_0048:
            if (this.StatComp != null)
            {
                MyTuple<ushort, MyStringHash> tuple;
                flag = this.StatComp.CanDoAction("Walk", out tuple, continuous);
                flag2 = this.StatComp.CanDoAction("Run", out tuple, flag6);
                flag3 = this.StatComp.CanDoAction("Sprint", out tuple, flag7);
                if (((MySession.Static != null) && (ReferenceEquals(MySession.Static.LocalCharacter, this) && (tuple.Item1 == 4))) && (tuple.Item2.String.CompareTo("Stamina") == 0))
                {
                    object[] arguments = new object[] { tuple.Item2 };
                    this.m_notEnoughStatNotification.SetTextFormatArguments(arguments);
                    MyHud.Notifications.Add(this.m_notEnoughStatNotification);
                }
                flag4 = (flag | flag2) | flag3;
            }
            bool flag8 = (((moveIndicator.X != 0f) || !(moveIndicator.Z == 0f)) & canMove) & flag4;
            bool flag9 = (rotationIndicator.X != 0f) || !(rotationIndicator.Y == 0f);
            if (!(flag8 | movementFlagsChanged))
            {
                if (!flag9)
                {
                    MyCharacterMovementEnum enum4 = this.m_currentMovementState;
                    if (enum4 > MyCharacterMovementEnum.CrouchWalkingRightFront)
                    {
                        if (enum4 > MyCharacterMovementEnum.RunningLeftBack)
                        {
                            if (enum4 > MyCharacterMovementEnum.Sprinting)
                            {
                                if (enum4 > MyCharacterMovementEnum.CrouchRotatingLeft)
                                {
                                    if ((enum4 != MyCharacterMovementEnum.RotatingRight) && (enum4 != MyCharacterMovementEnum.CrouchRotatingRight))
                                    {
                                        return currentMovementState;
                                    }
                                }
                                else if ((enum4 != MyCharacterMovementEnum.RotatingLeft) && (enum4 != MyCharacterMovementEnum.CrouchRotatingLeft))
                                {
                                    return currentMovementState;
                                }
                                currentMovementState = this.GetIdleState();
                                this.m_currentDecceleration = MyPerGameSettings.CharacterMovement.WalkDecceleration;
                                return currentMovementState;
                            }
                            else if (enum4 > MyCharacterMovementEnum.RunningRightFront)
                            {
                                if (enum4 != MyCharacterMovementEnum.RunningRightBack)
                                {
                                    if (enum4 == MyCharacterMovementEnum.Sprinting)
                                    {
                                        currentMovementState = this.GetIdleState();
                                        this.m_currentDecceleration = MyPerGameSettings.CharacterMovement.SprintDecceleration;
                                    }
                                    return currentMovementState;
                                }
                            }
                            else if ((enum4 != MyCharacterMovementEnum.RunStrafingRight) && (enum4 != MyCharacterMovementEnum.RunningRightFront))
                            {
                                return currentMovementState;
                            }
                        }
                        else if (enum4 > MyCharacterMovementEnum.Running)
                        {
                            if (enum4 > MyCharacterMovementEnum.RunStrafingLeft)
                            {
                                if ((enum4 != MyCharacterMovementEnum.RunningLeftFront) && (enum4 != MyCharacterMovementEnum.RunningLeftBack))
                                {
                                    return currentMovementState;
                                }
                            }
                            else if ((enum4 != MyCharacterMovementEnum.Backrunning) && (enum4 != MyCharacterMovementEnum.RunStrafingLeft))
                            {
                                return currentMovementState;
                            }
                        }
                        else if (((enum4 != MyCharacterMovementEnum.WalkingRightBack) && (enum4 != MyCharacterMovementEnum.CrouchWalkingRightBack)) && (enum4 != MyCharacterMovementEnum.Running))
                        {
                            return currentMovementState;
                        }
                    }
                    else if (enum4 > MyCharacterMovementEnum.CrouchStrafingLeft)
                    {
                        if (enum4 > MyCharacterMovementEnum.CrouchWalkingLeftBack)
                        {
                            if (enum4 > MyCharacterMovementEnum.CrouchStrafingRight)
                            {
                                if ((enum4 != MyCharacterMovementEnum.WalkingRightFront) && (enum4 != MyCharacterMovementEnum.CrouchWalkingRightFront))
                                {
                                    return currentMovementState;
                                }
                            }
                            else if ((enum4 != MyCharacterMovementEnum.WalkStrafingRight) && (enum4 != MyCharacterMovementEnum.CrouchStrafingRight))
                            {
                                return currentMovementState;
                            }
                        }
                        else if (enum4 > MyCharacterMovementEnum.CrouchWalkingLeftFront)
                        {
                            if ((enum4 != MyCharacterMovementEnum.WalkingLeftBack) && (enum4 != MyCharacterMovementEnum.CrouchWalkingLeftBack))
                            {
                                return currentMovementState;
                            }
                        }
                        else if ((enum4 != MyCharacterMovementEnum.WalkingLeftFront) && (enum4 != MyCharacterMovementEnum.CrouchWalkingLeftFront))
                        {
                            return currentMovementState;
                        }
                    }
                    else if (enum4 > MyCharacterMovementEnum.CrouchWalking)
                    {
                        if (enum4 > MyCharacterMovementEnum.CrouchBackWalking)
                        {
                            if ((enum4 != MyCharacterMovementEnum.WalkStrafingLeft) && (enum4 != MyCharacterMovementEnum.CrouchStrafingLeft))
                            {
                                return currentMovementState;
                            }
                        }
                        else if ((enum4 != MyCharacterMovementEnum.BackWalking) && (enum4 != MyCharacterMovementEnum.CrouchBackWalking))
                        {
                            return currentMovementState;
                        }
                    }
                    else
                    {
                        switch (enum4)
                        {
                            case MyCharacterMovementEnum.Standing:
                                if (this.WantsCrouch)
                                {
                                    currentMovementState = this.GetIdleState();
                                }
                                this.m_currentDecceleration = MyPerGameSettings.CharacterMovement.WalkDecceleration;
                                return currentMovementState;

                            case MyCharacterMovementEnum.Sitting:
                            case MyCharacterMovementEnum.Flying:
                            case MyCharacterMovementEnum.Falling:
                            case MyCharacterMovementEnum.Jump:
                                return currentMovementState;

                            case MyCharacterMovementEnum.Crouching:
                                if (!this.WantsCrouch)
                                {
                                    currentMovementState = this.GetIdleState();
                                }
                                this.m_currentDecceleration = MyPerGameSettings.CharacterMovement.WalkDecceleration;
                                return currentMovementState;

                            default:
                                if ((enum4 == MyCharacterMovementEnum.Walking) || (enum4 == MyCharacterMovementEnum.CrouchWalking))
                                {
                                    break;
                                }
                                return currentMovementState;
                        }
                    }
                    currentMovementState = this.GetIdleState();
                    this.m_currentDecceleration = MyPerGameSettings.CharacterMovement.WalkDecceleration;
                }
                else if ((Math.Abs(rotationIndicator.Y) > 20f) && ((this.m_currentMovementState == MyCharacterMovementEnum.Standing) || (this.m_currentMovementState == MyCharacterMovementEnum.Crouching)))
                {
                    currentMovementState = !this.WantsCrouch ? ((rotationIndicator.Y <= 0f) ? MyCharacterMovementEnum.RotatingLeft : MyCharacterMovementEnum.RotatingRight) : ((rotationIndicator.Y <= 0f) ? MyCharacterMovementEnum.CrouchRotatingLeft : MyCharacterMovementEnum.CrouchRotatingRight);
                }
            }
            else
            {
                currentMovementState = !(sprint & flag3) ? (!flag8 ? this.GetIdleState() : (!(walk & flag) ? (!flag2 ? this.GetWalkingState(ref moveIndicator) : this.GetRunningState(ref moveIndicator)) : this.GetWalkingState(ref moveIndicator))) : this.GetSprintState(ref moveIndicator);
                acceleration = this.GetMovementAcceleration(currentMovementState);
                this.m_currentDecceleration = 0f;
            }
            return currentMovementState;
        TR_0049:
            continuous = true;
            goto TR_0048;
        TR_0057:
            flag6 = true;
            goto TR_0048;
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            long? nullable1;
            MyObjectBuilder_Character objectBuilder = (MyObjectBuilder_Character) base.GetObjectBuilder(copy);
            objectBuilder.CharacterModel = this.m_characterModel;
            objectBuilder.ColorMaskHSV = this.ColorMask;
            if ((this.GetInventory(0) == null) || MyFakes.ENABLE_MEDIEVAL_INVENTORY)
            {
                objectBuilder.Inventory = null;
            }
            else
            {
                objectBuilder.Inventory = this.GetInventory(0).GetObjectBuilder();
            }
            if (this.m_currentWeapon != null)
            {
                objectBuilder.HandWeapon = ((VRage.Game.Entity.MyEntity) this.m_currentWeapon).GetObjectBuilder(false);
            }
            objectBuilder.Battery = this.m_suitBattery.GetObjectBuilder();
            objectBuilder.LightEnabled = this.m_lightEnabled;
            if (this.IsOnLadder)
            {
                nullable1 = new long?(this.m_ladder.EntityId);
            }
            else
            {
                nullable1 = null;
            }
            objectBuilder.UsingLadder = nullable1;
            objectBuilder.HeadAngle = new Vector2(this.m_headLocalXAngle, this.m_headLocalYAngle);
            objectBuilder.LinearVelocity = (this.Physics != null) ? this.Physics.LinearVelocity : Vector3.Zero;
            objectBuilder.Health = null;
            objectBuilder.LootingCounter = this.m_currentLootingCounter;
            objectBuilder.DisplayName = base.DisplayName;
            objectBuilder.CharacterGeneralDamageModifier = this.CharacterGeneralDamageModifier;
            objectBuilder.IsInFirstPersonView = !Sandbox.Engine.Platform.Game.IsDedicated ? this.m_isInFirstPersonView : true;
            objectBuilder.EnableBroadcasting = this.RadioBroadcaster.WantsToBeEnabled;
            objectBuilder.MovementState = this.m_currentMovementState;
            if (base.Components != null)
            {
                if (objectBuilder.EnabledComponents == null)
                {
                    objectBuilder.EnabledComponents = new List<string>();
                }
                foreach (MyComponentBase base2 in base.Components)
                {
                    foreach (KeyValuePair<MyStringId, Tuple<System.Type, System.Type>> pair in MyCharacterComponentTypes.CharacterComponents)
                    {
                        if (pair.Value.Item2 != base2.GetType())
                        {
                            continue;
                        }
                        if (!objectBuilder.EnabledComponents.Contains(pair.Key.ToString()))
                        {
                            objectBuilder.EnabledComponents.Add(pair.Key.ToString());
                        }
                    }
                }
                if (this.JetpackComp != null)
                {
                    this.JetpackComp.GetObjectBuilder(objectBuilder);
                }
                if (this.OxygenComponent != null)
                {
                    this.OxygenComponent.GetObjectBuilder(objectBuilder);
                }
            }
            objectBuilder.PlayerSerialId = this.m_controlInfo.Value.SerialId;
            objectBuilder.PlayerSteamId = this.m_controlInfo.Value.SteamId;
            objectBuilder.OwningPlayerIdentityId = new long?(this.m_idModule.Owner);
            objectBuilder.IsPersistenceCharacter = this.IsPersistenceCharacter;
            objectBuilder.RelativeDampeningEntity = (this.RelativeDampeningEntity != null) ? this.RelativeDampeningEntity.EntityId : 0L;
            return objectBuilder;
        }

        private MyObjectBuilder_EntityBase GetObjectBuilderForWeapon(MyDefinitionId? weaponDefinition, ref uint? inventoryItemId, long weaponEntityId)
        {
            MyObjectBuilder_EntityBase gunEntity = null;
            if ((inventoryItemId != 0) && (Sync.IsServer || this.ControllerInfo.IsLocallyControlled()))
            {
                MyPhysicalInventoryItem? itemByID = this.GetInventory(0).GetItemByID(inventoryItemId.Value);
                if (itemByID != null)
                {
                    MyObjectBuilder_PhysicalGunObject content = itemByID.Value.Content as MyObjectBuilder_PhysicalGunObject;
                    if (content != null)
                    {
                        gunEntity = content.GunEntity;
                    }
                    if (gunEntity != null)
                    {
                        gunEntity.EntityId = weaponEntityId;
                    }
                    else
                    {
                        MyHandItemDefinition definition = MyDefinitionManager.Static.TryGetHandItemForPhysicalItem(weaponDefinition.Value);
                        if (definition != null)
                        {
                            gunEntity = (MyObjectBuilder_EntityBase) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) definition.Id);
                            gunEntity.EntityId = weaponEntityId;
                        }
                    }
                    if (content != null)
                    {
                        content.GunEntity = gunEntity;
                    }
                }
            }
            else
            {
                int num1;
                if (Sync.IsServer || !this.ControllerInfo.IsRemotelyControlled())
                {
                    num1 = (int) !this.WeaponTakesBuilderFromInventory(weaponDefinition);
                }
                else
                {
                    num1 = 1;
                }
                bool flag = (bool) num1;
                if (weaponDefinition == null)
                {
                    this.EquipWeapon(null, false);
                }
                else if (flag && (weaponDefinition.Value.TypeId == typeof(MyObjectBuilder_PhysicalGunObject)))
                {
                    MyHandItemDefinition definition2 = MyDefinitionManager.Static.TryGetHandItemForPhysicalItem(weaponDefinition.Value);
                    if (definition2 != null)
                    {
                        gunEntity = (MyObjectBuilder_EntityBase) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) definition2.Id);
                        gunEntity.EntityId = weaponEntityId;
                    }
                }
                else
                {
                    gunEntity = MyObjectBuilderSerializer.CreateNewObject(weaponDefinition.Value.TypeId, weaponDefinition.Value.SubtypeName) as MyObjectBuilder_EntityBase;
                    if (gunEntity != null)
                    {
                        gunEntity.EntityId = weaponEntityId;
                        if (this.WeaponTakesBuilderFromInventory(weaponDefinition))
                        {
                            MyPhysicalInventoryItem? nullable2 = this.FindWeaponItemByDefinition(weaponDefinition.Value);
                            if (nullable2 != null)
                            {
                                MyObjectBuilder_PhysicalGunObject content = nullable2.Value.Content as MyObjectBuilder_PhysicalGunObject;
                                if (content != null)
                                {
                                    content.GunEntity = gunEntity;
                                }
                                inventoryItemId = new uint?(nullable2.Value.ItemId);
                            }
                        }
                    }
                }
            }
            if (gunEntity != null)
            {
                IMyObjectBuilder_GunObject<MyObjectBuilder_DeviceBase> obj4 = gunEntity as IMyObjectBuilder_GunObject<MyObjectBuilder_DeviceBase>;
                if ((obj4 != null) && (obj4.DeviceBase != null))
                {
                    obj4.DeviceBase.InventoryItemId = inventoryItemId;
                }
            }
            return gunEntity;
        }

        public void GetOffLadder()
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter>(this, x => new Action(x.GetOffLadder_Implementation), targetEndpoint);
        }

        [Event(null, 0xb0), Reliable, Server(ValidationType.Controlled), Broadcast]
        private void GetOffLadder_Implementation()
        {
            if ((this.IsOnLadder && (this.Physics != null)) && !this.IsDead)
            {
                MyLadder ladder = this.m_ladder;
                this.ChangeLadder(null, false);
                this.UpdateLadderNotifications();
                if (this.Physics.CharacterProxy != null)
                {
                    this.Physics.CharacterProxy.AtLadder = false;
                    this.Physics.CharacterProxy.EnableLadderState(false);
                }
                this.m_currentLadderStep = 0;
                this.TriggerCharacterAnimationEvent("GetOffLadder", false);
                if (this.m_currentMovementState != MyCharacterMovementEnum.LadderOut)
                {
                    this.StartFalling();
                }
                else
                {
                    this.m_currentJumpTime = 0.2f;
                    this.Stand();
                }
                Vector3 linearVelocity = ladder.Parent.Physics.LinearVelocity;
                this.Physics.LinearVelocity = linearVelocity;
                if (!Vector3.IsZero(linearVelocity))
                {
                    this.SetRelativeDampening(ladder.Parent);
                }
            }
        }

        private void GetOffLadderFromMovement()
        {
            Vector3D position = base.PositionComp.GetPosition();
            if (this.IsOnLadder)
            {
                position = (this.m_ladder.PositionComp.GetPosition() + ((base.WorldMatrix.Up * MyDefinitionManager.Static.GetCubeSize(MyCubeSize.Large)) * 0.60000002384185791)) + (base.WorldMatrix.Forward * 0.89999997615814209);
            }
            this.GetOffLadder();
            base.PositionComp.SetPosition(position, null, false, true);
        }

        public void GetOnLadder(MyLadder ladder)
        {
            if (this.ResponsibleForUpdate(Sync.Clients.LocalClient))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCharacter, long>(this, x => new Action<long>(x.GetOnLadder_Request), ladder.EntityId, targetEndpoint);
            }
        }

        [Event(null, 0x62), Reliable, Client]
        private void GetOnLadder_Failed()
        {
            if ((this.m_ladderBlockedNotification == null) && ReferenceEquals(this, MySession.Static.LocalCharacter))
            {
                this.m_ladderBlockedNotification = new MyHudNotification(MySpaceTexts.NotificationHintLadderBlocked, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            }
            MyHud.Notifications.Add(this.m_ladderBlockedNotification);
        }

        [Event(null, 0x6c), Reliable, Server(ValidationType.Controlled), Broadcast]
        private void GetOnLadder_Implementation(long ladderId, bool resetPosition = true)
        {
            MyLadder ladder;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyLadder>(ladderId, out ladder, false) && !this.IsOnLadder)
            {
                if (!this.IsClientPredicted)
                {
                    this.ForceDisablePrediction = false;
                    this.UpdatePredictionFlag();
                }
                this.MoveIndicator = Vector3.Zero;
                this.ChangeLadder(ladder, resetPosition);
                this.StopFalling();
                uint? inventoryItemId = null;
                this.SwitchToWeapon(null, inventoryItemId, false);
                if (this.JetpackComp != null)
                {
                    this.JetpackComp.TurnOnJetpack(false, false, false);
                }
                this.SetCurrentMovementState(MyCharacterMovementEnum.Ladder);
                if (this.Physics.CharacterProxy != null)
                {
                    this.Physics.CharacterProxy.EnableLadderState(true);
                }
                this.UpdateNearFlag();
                this.Physics.ClearSpeed();
                this.Physics.LinearVelocity = ladder.CubeGrid.Physics.GetVelocityAtPoint(base.WorldMatrix.Translation);
                this.m_currentLadderStep = 0;
                this.m_stepsPerAnimation = 0x3b;
                this.m_stepIncrement = (2f * ladder.DistanceBetweenPoles) / ((float) this.m_stepsPerAnimation);
                this.StopUpperAnimation(0f);
                this.TriggerCharacterAnimationEvent("GetOnLadder", false);
                if (this.Physics.CharacterProxy != null)
                {
                    this.Physics.CharacterProxy.AtLadder = true;
                }
                this.UpdateLadderNotifications();
            }
        }

        [Event(null, 0x4c), Reliable, Server]
        private void GetOnLadder_Request(long ladderId)
        {
            MyLadder ladder;
            if (Sync.IsServer && Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyLadder>(ladderId, out ladder, false))
            {
                MatrixD worldMatrix = ladder.PositionComp.WorldMatrix;
                if (!this.CanPlaceCharacter(ref worldMatrix, true, true, this))
                {
                    ulong num = MyEventContext.Current.Sender.Value;
                    MyMultiplayer.RaiseEvent<MyCharacter>(this, x => new Action(x.GetOnLadder_Failed), new EndpointId(num));
                }
                else
                {
                    EndpointId targetEndpoint = new EndpointId();
                    MyMultiplayer.RaiseEvent<MyCharacter, long, bool>(this, x => new Action<long, bool>(x.GetOnLadder_Implementation), ladder.EntityId, true, targetEndpoint);
                }
            }
        }

        public float GetOutsideTemperature() => 
            this.m_outsideTemperature;

        public long GetPlayerIdentityId() => 
            this.m_idModule.Owner;

        public MyCharacterMovementEnum GetPreviousMovementState() => 
            this.m_previousMovementState;

        private static string GetRealModel(string asset, ref Vector3 colorMask)
        {
            if (!string.IsNullOrEmpty(asset) && MyObjectBuilder_Character.CharacterModels.ContainsKey(asset))
            {
                SerializableVector3 vector = MyObjectBuilder_Character.CharacterModels[asset];
                if (((vector.X > -1f) || (vector.Y > -1f)) || (vector.Z > -1f))
                {
                    colorMask = (Vector3) vector;
                }
                asset = DefaultModel;
            }
            return asset;
        }

        public MyRelationsBetweenPlayerAndBlock GetRelationTo(long playerId) => 
            MyPlayer.GetRelationBetweenPlayers(this.GetPlayerIdentityId(), playerId);

        public Quaternion GetRotation()
        {
            Quaternion quaternion;
            if (this.JetpackRunning)
            {
                Quaternion.CreateFromRotationMatrix(ref base.WorldMatrix, out quaternion);
            }
            else if (this.Physics.CharacterProxy != null)
            {
                quaternion = Quaternion.CreateFromForwardUp(this.Physics.CharacterProxy.Forward, this.Physics.CharacterProxy.Up);
            }
            else
            {
                quaternion = Quaternion.CreateFromForwardUp((Vector3) base.WorldMatrix.Forward, (Vector3) base.WorldMatrix.Up);
            }
            return quaternion;
        }

        private MyCharacterMovementEnum GetRunningState(ref Vector3 moveIndicator)
        {
            double num = Math.Tan((double) MathHelper.ToRadians((float) 23f));
            return ((Math.Abs(moveIndicator.X) >= (num * Math.Abs(moveIndicator.Z))) ? (((Math.Abs(moveIndicator.X) * num) <= Math.Abs(moveIndicator.Z)) ? ((moveIndicator.X <= 0f) ? ((moveIndicator.Z >= 0f) ? (this.WantsCrouch ? MyCharacterMovementEnum.CrouchWalkingLeftBack : MyCharacterMovementEnum.RunningLeftBack) : (this.WantsCrouch ? MyCharacterMovementEnum.CrouchWalkingLeftFront : MyCharacterMovementEnum.RunningLeftFront)) : ((moveIndicator.Z >= 0f) ? (this.WantsCrouch ? MyCharacterMovementEnum.CrouchWalkingRightBack : MyCharacterMovementEnum.RunningRightBack) : (this.WantsCrouch ? MyCharacterMovementEnum.CrouchWalkingRightFront : MyCharacterMovementEnum.RunningRightFront))) : ((moveIndicator.X <= 0f) ? (this.WantsCrouch ? MyCharacterMovementEnum.CrouchStrafingLeft : MyCharacterMovementEnum.RunStrafingLeft) : (this.WantsCrouch ? MyCharacterMovementEnum.CrouchStrafingRight : MyCharacterMovementEnum.RunStrafingRight))) : ((moveIndicator.Z >= 0f) ? (this.WantsCrouch ? MyCharacterMovementEnum.CrouchBackWalking : MyCharacterMovementEnum.Backrunning) : (this.WantsCrouch ? MyCharacterMovementEnum.CrouchWalking : MyCharacterMovementEnum.Running)));
        }

        public MyShootActionEnum? GetShootingAction()
        {
            foreach (MyShootActionEnum enum2 in MyEnum<MyShootActionEnum>.Values)
            {
                if (this.m_isShooting[(int) enum2])
                {
                    return new MyShootActionEnum?(enum2);
                }
            }
            return null;
        }

        private MyCharacterMovementEnum GetSprintState(ref Vector3 moveIndicator)
        {
            if ((moveIndicator.X != 0f) || (moveIndicator.Z >= 0f))
            {
                return this.GetRunningState(ref moveIndicator);
            }
            return MyCharacterMovementEnum.Sprinting;
        }

        public float GetSuitGasFillLevel(MyDefinitionId gasDefinitionId) => 
            this.OxygenComponent.GetGasFillLevel(gasDefinitionId);

        public override unsafe MatrixD GetViewMatrix()
        {
            if (this.IsDead && MyPerGameSettings.SwitchToSpectatorCameraAfterDeath)
            {
                this.m_isInFirstPersonView = false;
                if (this.m_lastCorrectSpectatorCamera == MatrixD.Zero)
                {
                    this.m_lastCorrectSpectatorCamera = MatrixD.CreateLookAt((base.WorldMatrix.Translation + (2f * Vector3.Up)) - (2f * Vector3.Forward), base.WorldMatrix.Translation, Vector3.Up);
                }
                Vector3 translation = (Vector3) base.WorldMatrix.Translation;
                if (this.m_headBoneIndex != -1)
                {
                    translation = (Vector3) Vector3.Transform(base.AnimationController.CharacterBones[this.m_headBoneIndex].AbsoluteTransform.Translation, base.WorldMatrix);
                }
                MatrixD xd2 = MatrixD.CreateLookAt(MatrixD.Invert(this.m_lastCorrectSpectatorCamera).Translation, translation, Vector3.Up);
                if (!xd2.IsValid() || (xd2 == MatrixD.Zero))
                {
                    return this.m_lastCorrectSpectatorCamera;
                }
                return xd2;
            }
            if (this.IsDead)
            {
                MySpectator.Static.SetTarget(base.PositionComp.GetPosition() + base.WorldMatrix.Up, new Vector3D?(base.WorldMatrix.Up));
            }
            if ((!this.ForceFirstPersonCamera || !this.IsDead) && !this.m_isInFirstPersonView)
            {
                bool forceFirstPersonCamera = this.ForceFirstPersonCamera;
                bool flag = !MyThirdPersonSpectator.Static.IsCameraForced();
                if (!this.ForceFirstPersonCamera & flag)
                {
                    return MyThirdPersonSpectator.Static.GetViewMatrix();
                }
            }
            MatrixD matrix = this.GetHeadMatrix(this.IsOnLadder, true, false, this.ForceFirstPersonCamera, true);
            if (this.IsDead)
            {
                Vector3D translation = matrix.Translation;
                Vector3D vectord2 = -MyGravityProviderSystem.CalculateTotalGravityInPoint(translation);
                if (!Vector3D.IsZero(vectord2))
                {
                    Vector3 halfExtents = new Vector3(this.Definition.CharacterHeadSize * 0.5f);
                    this.m_penetrationList.Clear();
                    MyPhysics.GetPenetrationsBox(ref halfExtents, ref translation, ref Quaternion.Identity, this.m_penetrationList, 0);
                    using (List<HkBodyCollision>.Enumerator enumerator = this.m_penetrationList.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            VRage.ModAPI.IMyEntity collisionEntity = enumerator.Current.GetCollisionEntity();
                            if ((collisionEntity is MyVoxelBase) || (collisionEntity is MyCubeGrid))
                            {
                                vectord2.Normalize();
                                MatrixD* xdPtr1 = (MatrixD*) ref matrix;
                                xdPtr1.Translation += vectord2;
                                this.m_forceFirstPersonCamera = false;
                                this.m_isInFirstPersonView = false;
                                this.m_isInFirstPerson = false;
                                break;
                            }
                        }
                    }
                }
            }
            this.m_lastCorrectSpectatorCamera = MatrixD.Zero;
            if (this.IsDead && this.m_lastGetViewWasDead)
            {
                MatrixD getViewAliveWorldMatrix = this.m_getViewAliveWorldMatrix;
                getViewAliveWorldMatrix.Translation = matrix.Translation;
                return MatrixD.Invert(getViewAliveWorldMatrix);
            }
            this.m_getViewAliveWorldMatrix = (Matrix) matrix;
            this.m_getViewAliveWorldMatrix.Translation = Vector3.Zero;
            this.m_lastGetViewWasDead = this.IsDead;
            return MatrixD.Invert(matrix);
        }

        private MyCharacterMovementEnum GetWalkingState(ref Vector3 moveIndicator)
        {
            double num = Math.Tan((double) MathHelper.ToRadians((float) 23f));
            return ((Math.Abs(moveIndicator.X) >= (num * Math.Abs(moveIndicator.Z))) ? (((Math.Abs(moveIndicator.X) * num) <= Math.Abs(moveIndicator.Z)) ? ((moveIndicator.X <= 0f) ? ((moveIndicator.Z >= 0f) ? (this.WantsCrouch ? MyCharacterMovementEnum.CrouchWalkingLeftBack : MyCharacterMovementEnum.WalkingLeftBack) : (this.WantsCrouch ? MyCharacterMovementEnum.CrouchWalkingLeftFront : MyCharacterMovementEnum.WalkingLeftFront)) : ((moveIndicator.Z >= 0f) ? (this.WantsCrouch ? MyCharacterMovementEnum.CrouchWalkingRightBack : MyCharacterMovementEnum.WalkingRightBack) : (this.WantsCrouch ? MyCharacterMovementEnum.CrouchWalkingRightFront : MyCharacterMovementEnum.WalkingRightFront))) : ((moveIndicator.X <= 0f) ? (this.WantsCrouch ? MyCharacterMovementEnum.CrouchStrafingLeft : MyCharacterMovementEnum.WalkStrafingLeft) : (this.WantsCrouch ? MyCharacterMovementEnum.CrouchStrafingRight : MyCharacterMovementEnum.WalkStrafingRight))) : ((moveIndicator.Z >= 0f) ? (this.WantsCrouch ? MyCharacterMovementEnum.CrouchBackWalking : MyCharacterMovementEnum.BackWalking) : (this.WantsCrouch ? MyCharacterMovementEnum.CrouchWalking : MyCharacterMovementEnum.Walking)));
        }

        private void GunDoubleClicked(MyShootActionEnum action)
        {
            this.OnGunDoubleClicked(action);
        }

        [Event(null, 0x2727), Reliable, Server, BroadcastExcept]
        private void GunDoubleClickedCallback(MyShootActionEnum action)
        {
            int isLocallyInvoked;
            if (Sync.IsServer)
            {
                isLocallyInvoked = (int) MyEventContext.Current.IsLocallyInvoked;
            }
            else
            {
                isLocallyInvoked = 0;
            }
            if (isLocallyInvoked == 0)
            {
                this.GunDoubleClicked(action);
            }
        }

        private void GunDoubleClickedInternal(MyShootActionEnum action = 0)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, MyShootActionEnum>(this, x => new Action<MyShootActionEnum>(x.GunDoubleClickedCallback), action, targetEndpoint);
            this.GunDoubleClicked(action);
        }

        public void GunDoubleClickedSync(MyShootActionEnum action = 0)
        {
            this.GunDoubleClickedInternal(action);
        }

        private void gunEntity_OnClose(VRage.Game.Entity.MyEntity obj)
        {
            if (ReferenceEquals(this.m_currentWeapon, obj))
            {
                this.m_currentWeapon = null;
            }
        }

        public bool HasAccessToLogicalGroup(MyGridLogicalGroupData group) => 
            this.RadioReceiver.HasAccessToLogicalGroup(group);

        public bool HasAnimation(string animationName) => 
            this.Definition.AnimationNameToSubtypeName.ContainsKey(animationName);

        private bool HasEnoughSpaceToStandUp()
        {
            if (!this.IsCrouching)
            {
                return true;
            }
            float num = this.Definition.CharacterCollisionHeight - this.Definition.CharacterCollisionCrouchHeight;
            Vector3D from = base.WorldMatrix.Translation + (this.Definition.CharacterCollisionCrouchHeight * base.WorldMatrix.Up);
            return (MyPhysics.CastRay(from, (base.WorldMatrix.Translation + (this.Definition.CharacterCollisionCrouchHeight * base.WorldMatrix.Up)) + (num * base.WorldMatrix.Up), 0x12) == null);
        }

        public override unsafe void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            this.RadioReceiver = new MyRadioReceiver();
            base.Components.Add<MyDataBroadcaster>(new MyRadioBroadcaster(100f));
            this.RadioBroadcaster.BroadcastRadius = 200f;
            base.SyncFlag = true;
            MyObjectBuilder_Character characterOb = (MyObjectBuilder_Character) objectBuilder;
            this.m_idModule.Owner = (characterOb.OwningPlayerIdentityId == null) ? MySession.Static.Players.TryGetIdentityId(characterOb.PlayerSteamId, characterOb.PlayerSerialId) : characterOb.OwningPlayerIdentityId.Value;
            this.Render.ColorMaskHsv = (Vector3) characterOb.ColorMaskHSV;
            Vector3 colorMaskHsv = this.Render.ColorMaskHsv;
            GetModelAndDefinition(characterOb, out this.m_characterModel, out this.m_characterDefinition, ref colorMaskHsv);
            this.m_physicalMaterialHash = MyStringHash.GetOrCompute(this.m_characterDefinition.PhysicalMaterial);
            base.UseNewAnimationSystem = this.m_characterDefinition.UseNewAnimationSystem;
            if (base.UseNewAnimationSystem && (!Sandbox.Engine.Platform.Game.IsDedicated || !MyPerGameSettings.DisableAnimationsOnDS))
            {
                base.AnimationController.Clear();
                MyStringHash orCompute = MyStringHash.GetOrCompute(this.m_characterDefinition.AnimationController);
                MyAnimationControllerDefinition animControllerDefinition = MyDefinitionManager.Static.GetDefinition<MyAnimationControllerDefinition>(orCompute);
                if (animControllerDefinition != null)
                {
                    base.AnimationController.InitFromDefinition(animControllerDefinition, false);
                }
            }
            if (this.Render.ColorMaskHsv != colorMaskHsv)
            {
                this.Render.ColorMaskHsv = colorMaskHsv;
            }
            characterOb.SubtypeName = this.m_characterDefinition.Id.SubtypeName;
            base.Init(objectBuilder);
            this.m_currentAnimationChangeDelay = 0f;
            this.SoundComp = new MyCharacterSoundComponent();
            this.RadioBroadcaster.WantsToBeEnabled = characterOb.EnableBroadcasting && this.Definition.VisibleOnHud;
            float? scale = null;
            this.Init(new StringBuilder(characterOb.DisplayName), this.m_characterDefinition.Model, null, scale, null);
            base.NeedsUpdate = MyEntityUpdateEnum.SIMULATE | MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            this.SetStandingLocalAABB();
            this.m_currentLootingCounter = characterOb.LootingCounter;
            if (this.m_currentLootingCounter <= 0f)
            {
                this.UpdateCharacterPhysics(false);
            }
            this.m_currentMovementState = characterOb.MovementState;
            if (this.Physics == null)
            {
                goto TR_0035;
            }
            else if (this.Physics.CharacterProxy == null)
            {
                goto TR_0035;
            }
            else
            {
                MyCharacterMovementEnum currentMovementState = this.m_currentMovementState;
                if (currentMovementState > MyCharacterMovementEnum.LadderUp)
                {
                    if ((currentMovementState == MyCharacterMovementEnum.LadderDown) || (currentMovementState == MyCharacterMovementEnum.LadderOut))
                    {
                        goto TR_0038;
                    }
                }
                else
                {
                    switch (currentMovementState)
                    {
                        case MyCharacterMovementEnum.Flying:
                        case MyCharacterMovementEnum.Falling:
                            this.Physics.CharacterProxy.SetState(HkCharacterStateType.HK_CHARACTER_IN_AIR);
                            goto TR_0035;

                        case MyCharacterMovementEnum.Jump:
                            this.Physics.CharacterProxy.SetState(HkCharacterStateType.HK_CHARACTER_JUMPING);
                            goto TR_0035;

                        case MyCharacterMovementEnum.Died:
                            goto TR_0037;

                        case MyCharacterMovementEnum.Ladder:
                            break;

                        default:
                            if (currentMovementState == MyCharacterMovementEnum.LadderUp)
                            {
                                break;
                            }
                            goto TR_0037;
                    }
                    goto TR_0038;
                }
            }
            goto TR_0037;
        TR_0035:
            this.InitAnimations();
            this.ValidateBonesProperties();
            this.CalculateTransforms(0f);
            this.InitAnimationCorrection();
            if (this.m_currentLootingCounter > 0f)
            {
                this.InitDeadBodyPhysics();
                if (this.m_currentMovementState != MyCharacterMovementEnum.Died)
                {
                    this.SetCurrentMovementState(MyCharacterMovementEnum.Died);
                }
                this.SwitchAnimation(MyCharacterMovementEnum.Died, false);
            }
            this.InitInventory(characterOb);
            this.Physics.Enabled = true;
            this.SetHeadLocalXAngle(characterOb.HeadAngle.X);
            this.SetHeadLocalYAngle(characterOb.HeadAngle.Y);
            this.Render.InitLight(this.m_characterDefinition);
            this.Render.InitJetpackThrusts(this.m_characterDefinition);
            this.m_lightEnabled = characterOb.LightEnabled;
            this.Physics.LinearVelocity = (Vector3) characterOb.LinearVelocity;
            if (this.Physics.CharacterProxy != null)
            {
                this.Physics.CharacterProxy.ContactPointCallbackEnabled = true;
                this.Physics.CharacterProxy.ContactPointCallback += new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
            }
            this.Render.UpdateLightProperties(this.m_currentLightPower);
            this.IsInFirstPersonView = !MySession.Static.Settings.Enable3rdPersonView || characterOb.IsInFirstPersonView;
            this.m_breath = new MyCharacterBreath(this);
            this.m_notEnoughStatNotification = new MyHudNotification(MyCommonTexts.NotificationStatNotEnough, 0x3e8, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
            if (this.InventoryAggregate != null)
            {
                this.InventoryAggregate.Init();
            }
            this.UseDamageSystem = true;
            if (characterOb.EnabledComponents == null)
            {
                characterOb.EnabledComponents = new List<string>();
            }
            using (List<string>.Enumerator enumerator = this.m_characterDefinition.EnabledComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string componentName;
                    if (characterOb.EnabledComponents.All<string>(x => x != componentName))
                    {
                        characterOb.EnabledComponents.Add(componentName);
                    }
                }
            }
            foreach (string str in characterOb.EnabledComponents)
            {
                Tuple<System.Type, System.Type> tuple;
                if (MyCharacterComponentTypes.CharacterComponents.TryGetValue(MyStringId.GetOrCompute(str), out tuple))
                {
                    MyEntityComponentBase component = Activator.CreateInstance(tuple.Item1) as MyEntityComponentBase;
                    base.Components.Add(tuple.Item2, component);
                }
            }
            if (this.m_characterDefinition.UsesAtmosphereDetector)
            {
                this.AtmosphereDetectorComp = new MyAtmosphereDetectorComponent();
                this.AtmosphereDetectorComp.InitComponent(true, this);
            }
            if (this.m_characterDefinition.UsesReverbDetector)
            {
                this.ReverbDetectorComp = new MyEntityReverbDetectorComponent();
                this.ReverbDetectorComp.InitComponent(this, true);
            }
            List<MyResourceSinkInfo> sinkData = new List<MyResourceSinkInfo>();
            List<MyResourceSourceInfo> sourceData = new List<MyResourceSourceInfo>();
            bool flag1 = this.Definition.SuitResourceStorage.Count > 0;
            if (flag1)
            {
                this.OxygenComponent = new MyCharacterOxygenComponent();
                base.Components.Add<MyCharacterOxygenComponent>(this.OxygenComponent);
                this.OxygenComponent.Init(characterOb);
                this.OxygenComponent.AppendSinkData(sinkData);
                this.OxygenComponent.AppendSourceData(sourceData);
            }
            this.m_suitBattery = new MyBattery(this);
            this.m_suitBattery.Init(characterOb.Battery, sinkData, sourceData);
            bool local1 = flag1;
            if (local1)
            {
                this.OxygenComponent.CharacterGasSink = this.m_suitBattery.ResourceSink;
                this.OxygenComponent.CharacterGasSource = this.m_suitBattery.ResourceSource;
            }
            sinkData.Clear();
            MyResourceSinkInfo item = new MyResourceSinkInfo {
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                MaxRequiredInput = 1.2E-05f,
                RequiredInputFunc = new Func<float>(this.ComputeRequiredPower)
            };
            sinkData.Add(item);
            if (local1)
            {
                MyResourceSinkInfo* infoPtr1;
                item = new MyResourceSinkInfo {
                    ResourceTypeId = MyCharacterOxygenComponent.OxygenId
                };
                infoPtr1->MaxRequiredInput = (((this.OxygenComponent.OxygenCapacity + (!this.OxygenComponent.NeedsOxygenFromSuit ? this.Definition.OxygenConsumption : 0f)) * this.Definition.OxygenConsumptionMultiplier) * 60f) / 100f;
                infoPtr1 = (MyResourceSinkInfo*) ref item;
                item.RequiredInputFunc = () => (((this.OxygenComponent.HelmetEnabled ? this.Definition.OxygenConsumption : 0f) * this.Definition.OxygenConsumptionMultiplier) * 60f) / 100f;
                sinkData.Add(item);
            }
            this.SinkComp.Init(MyStringHash.GetOrCompute("Utility"), sinkData);
            this.SinkComp.CurrentInputChanged += (<p0>, <p1>, <p2>) => this.SetPowerInput(this.SinkComp.CurrentInputByType(MyResourceDistributorComponent.ElectricityId));
            this.SinkComp.TemporaryConnectedEntity = this;
            this.SuitRechargeDistributor = new MyResourceDistributorComponent(this.ToString());
            this.SuitRechargeDistributor.AddSource(this.m_suitBattery.ResourceSource);
            this.SuitRechargeDistributor.AddSink(this.SinkComp);
            this.SinkComp.Update();
            if (this.m_characterDefinition.Jetpack != null)
            {
                this.JetpackComp = new MyCharacterJetpackComponent();
                this.JetpackComp.Init(characterOb);
            }
            this.WeaponPosition = new MyCharacterWeaponPositionComponent();
            base.Components.Add<MyCharacterWeaponPositionComponent>(this.WeaponPosition);
            this.WeaponPosition.Init(characterOb);
            this.InitWeapon(characterOb.HandWeapon);
            if (this.Definition.RagdollBonesMappings.Count > 0)
            {
                this.CreateBodyCapsulesForHits(this.Definition.RagdollBonesMappings);
            }
            else
            {
                this.m_bodyCapsuleInfo.Clear();
            }
            this.PlayCharacterAnimation(this.Definition.InitialAnimation, MyBlendOption.Immediate, MyFrameOption.JustFirstFrame, 0f, 1f, false, null, false);
            this.m_savedHealth = characterOb.Health;
            this.m_savedPlayer = new MyPlayer.PlayerId(characterOb.PlayerSteamId, characterOb.PlayerSerialId);
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            this.m_previousLinearVelocity = (Vector3) characterOb.LinearVelocity;
            this.ControllerInfo.IsLocallyControlled();
            this.CheckExistingStatComponent();
            this.CharacterGeneralDamageModifier = characterOb.CharacterGeneralDamageModifier;
            this.m_resolveHighlightOverlap = true;
            this.IsPersistenceCharacter = characterOb.IsPersistenceCharacter;
            this.m_bootsState.ValueChanged += new Action<SyncBase>(this.OnBootsStateChanged);
            if (Sync.IsServer)
            {
                this.m_bootsState.Value = MyBootsState.Init;
            }
            this.m_relativeDampeningEntityInit = characterOb.RelativeDampeningEntity;
            this.m_ladderIdInit = characterOb.UsingLadder;
            return;
        TR_0037:
            this.Physics.CharacterProxy.SetState(HkCharacterStateType.HK_CHARACTER_ON_GROUND);
            goto TR_0035;
        TR_0038:
            this.Physics.CharacterProxy.SetState(HkCharacterStateType.HK_CHARACTER_CLIMBING);
            goto TR_0035;
        }

        private void InitAnimationCorrection()
        {
            if (this.IsDead && base.UseNewAnimationSystem)
            {
                base.AnimationController.Variables.SetValue(MyAnimationVariableStorageHints.StrIdDead, 1f);
            }
        }

        private void InitAnimations()
        {
            this.m_animationSpeedFilterCursor = 0;
            for (int i = 0; i < this.m_animationSpeedFilter.Length; i++)
            {
                this.m_animationSpeedFilter[i] = Vector3.Zero;
            }
            foreach (KeyValuePair<string, string[]> pair in this.m_characterDefinition.BoneSets)
            {
                base.AddAnimationPlayer(pair.Key, pair.Value);
            }
            base.SetBoneLODs(this.m_characterDefinition.BoneLODs);
            base.AnimationController.FindBone(this.m_characterDefinition.HeadBone, out this.m_headBoneIndex);
            base.AnimationController.FindBone(this.m_characterDefinition.Camera3rdBone, out this.m_camera3rdBoneIndex);
            if (this.m_camera3rdBoneIndex == -1)
            {
                this.m_camera3rdBoneIndex = this.m_headBoneIndex;
            }
            base.AnimationController.FindBone(this.m_characterDefinition.LeftHandIKStartBone, out this.m_leftHandIKStartBone);
            base.AnimationController.FindBone(this.m_characterDefinition.LeftHandIKEndBone, out this.m_leftHandIKEndBone);
            base.AnimationController.FindBone(this.m_characterDefinition.RightHandIKStartBone, out this.m_rightHandIKStartBone);
            base.AnimationController.FindBone(this.m_characterDefinition.RightHandIKEndBone, out this.m_rightHandIKEndBone);
            base.AnimationController.FindBone(this.m_characterDefinition.LeftUpperarmBone, out this.m_leftUpperarmBone);
            base.AnimationController.FindBone(this.m_characterDefinition.LeftForearmBone, out this.m_leftForearmBone);
            base.AnimationController.FindBone(this.m_characterDefinition.RightUpperarmBone, out this.m_rightUpperarmBone);
            base.AnimationController.FindBone(this.m_characterDefinition.RightForearmBone, out this.m_rightForearmBone);
            base.AnimationController.FindBone(this.m_characterDefinition.WeaponBone, out this.m_weaponBone);
            base.AnimationController.FindBone(this.m_characterDefinition.LeftHandItemBone, out this.m_leftHandItemBone);
            base.AnimationController.FindBone(this.m_characterDefinition.RighHandItemBone, out this.m_rightHandItemBone);
            base.AnimationController.FindBone(this.m_characterDefinition.SpineBone, out this.m_spineBone);
            this.UpdateAnimation(0f);
        }

        private unsafe void InitDeadBodyPhysics()
        {
            int num;
            HkShape shape;
            Vector3 zero = Vector3.Zero;
            this.EnableBag(false);
            this.RadioBroadcaster.BroadcastRadius = 5f;
            if (this.Physics != null)
            {
                zero = this.Physics.LinearVelocity;
                this.Physics.Enabled = false;
                this.Physics.Close();
                this.Physics = null;
            }
            if (this.m_deathLinearVelocityFromSever != null)
            {
                zero = this.m_deathLinearVelocityFromSever.Value;
            }
            HkMassProperties properties = new HkMassProperties {
                Mass = 500f
            };
            if ((!Sync.IsDedicated || !MyFakes.ENABLE_RAGDOLL) || MyFakes.ENABLE_RAGDOLL_CLIENT_SYNC)
            {
                num = 0x17;
            }
            else
            {
                num = 0x13;
            }
            if (this.Definition.DeadBodyShape != null)
            {
                HkBoxShape shape2 = new HkBoxShape(base.PositionComp.LocalAABB.HalfExtents * this.Definition.DeadBodyShape.BoxShapeScale);
                properties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(shape2.HalfExtents, properties.Mass);
                properties.CenterOfMass = shape2.HalfExtents * this.Definition.DeadBodyShape.RelativeCenterOfMass;
                shape = (HkShape) shape2;
                this.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_DEFAULT);
                Vector3D position = base.PositionComp.LocalAABB.HalfExtents * this.Definition.DeadBodyShape.RelativeShapeTranslation;
                MatrixD worldTransform = MatrixD.CreateTranslation(position);
                this.Physics.CreateFromCollisionObject(shape, base.PositionComp.LocalVolume.Center + position, worldTransform, new HkMassProperties?(properties), num);
                this.Physics.Friction = this.Definition.DeadBodyShape.Friction;
                this.Physics.RigidBody.MaxAngularVelocity = 1.570796f;
                this.Physics.LinearVelocity = zero;
                shape.RemoveReference();
                this.Physics.Enabled = true;
            }
            else
            {
                Vector3 halfExtents = base.PositionComp.LocalAABB.HalfExtents;
                float* singlePtr1 = (float*) ref halfExtents.X;
                singlePtr1[0] *= 0.7f;
                float* singlePtr2 = (float*) ref halfExtents.Z;
                singlePtr2[0] *= 0.7f;
                HkBoxShape shape3 = new HkBoxShape(halfExtents);
                properties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(shape3.HalfExtents, properties.Mass);
                properties.CenterOfMass = new Vector3(halfExtents.X, 0f, 0f);
                shape = (HkShape) shape3;
                this.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_DEFAULT);
                this.Physics.CreateFromCollisionObject(shape, base.PositionComp.LocalAABB.Center, MatrixD.Identity, new HkMassProperties?(properties), num);
                this.Physics.Friction = 0.5f;
                this.Physics.RigidBody.MaxAngularVelocity = 1.570796f;
                this.Physics.LinearVelocity = zero;
                shape.RemoveReference();
                this.Physics.Enabled = true;
            }
            HkMassChangerUtil.Create(this.Physics.RigidBody, 0x10000, 1f, 0f);
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        private void InitInventory(MyObjectBuilder_Character characterOb)
        {
            if (this.GetInventory(0) != null)
            {
                if (MyPerGameSettings.ConstrainInventory())
                {
                    MyInventory inventory = this.GetInventory(0);
                    if (inventory.IsConstrained)
                    {
                        inventory.FixInventoryVolume(this.m_characterDefinition.InventoryDefinition.InventoryVolume);
                    }
                }
            }
            else
            {
                if (this.m_characterDefinition.InventoryDefinition == null)
                {
                    this.m_characterDefinition.InventoryDefinition = new MyObjectBuilder_InventoryDefinition();
                }
                MyInventory component = new MyInventory(this.m_characterDefinition.InventoryDefinition, 0);
                component.Init((MyObjectBuilder_Inventory) null);
                if (this.InventoryAggregate != null)
                {
                    this.InventoryAggregate.AddComponent(component);
                }
                else
                {
                    base.Components.Add<MyInventoryBase>(component);
                }
                component.Init(characterOb.Inventory);
                MyCubeBuilder.BuildComponent.AfterCharacterCreate(this);
                if (MyFakes.ENABLE_MEDIEVAL_INVENTORY && (this.InventoryAggregate != null))
                {
                    MyInventoryAggregate inventory = this.InventoryAggregate.GetInventory(MyStringHash.GetOrCompute("Internal")) as MyInventoryAggregate;
                    if (inventory != null)
                    {
                        inventory.AddComponent(component);
                    }
                    else
                    {
                        this.InventoryAggregate.AddComponent(component);
                    }
                }
            }
            this.GetInventory(0).ContentsChanged -= new Action<MyInventoryBase>(this.inventory_OnContentsChanged);
            this.GetInventory(0).BeforeContentsChanged -= new Action<MyInventoryBase>(this.inventory_OnBeforeContentsChanged);
            this.GetInventory(0).BeforeRemovedFromContainer -= new Action<MyEntityComponentBase>(this.inventory_OnRemovedFromContainer);
            this.GetInventory(0).ContentsChanged += new Action<MyInventoryBase>(this.inventory_OnContentsChanged);
            this.GetInventory(0).BeforeContentsChanged += new Action<MyInventoryBase>(this.inventory_OnBeforeContentsChanged);
            this.GetInventory(0).BeforeRemovedFromContainer += new Action<MyEntityComponentBase>(this.inventory_OnRemovedFromContainer);
        }

        private void InitWeapon(MyObjectBuilder_EntityBase weapon)
        {
            if (weapon != null)
            {
                if (((this.m_rightHandItemBone == -1) || (weapon != null)) && (this.m_currentWeapon != null))
                {
                    this.DisposeWeapon();
                }
                MyPhysicalItemDefinition physicalItemForHandItem = MyDefinitionManager.Static.GetPhysicalItemForHandItem(weapon.GetId());
                bool flag = (physicalItemForHandItem != null) && (!MySession.Static.SurvivalMode || (this.GetInventory(0).GetItemAmount(physicalItemForHandItem.Id, MyItemFlags.None, false) > 0));
                if ((this.m_rightHandItemBone != -1) & flag)
                {
                    uint? inventoryItemId = null;
                    this.m_currentWeapon = CreateGun(weapon, inventoryItemId);
                    ((VRage.Game.Entity.MyEntity) this.m_currentWeapon).Render.DrawInAllCascades = true;
                }
            }
        }

        private void inventory_OnBeforeContentsChanged(MyInventoryBase inventory)
        {
            if (ReferenceEquals(this, MySession.Static.LocalCharacter) && (((this.m_currentWeapon != null) && (this.WeaponTakesBuilderFromInventory(new MyDefinitionId?(this.m_currentWeapon.DefinitionId)) && ((inventory != null) && (inventory is MyInventory)))) && (inventory as MyInventory).ContainItems(1, this.m_currentWeapon.PhysicalObject)))
            {
                this.SaveAmmoToWeapon();
            }
        }

        private void inventory_OnContentsChanged(MyInventoryBase inventory)
        {
            if (ReferenceEquals(this, MySession.Static.LocalCharacter))
            {
                if (((this.m_currentWeapon != null) && (this.WeaponTakesBuilderFromInventory(new MyDefinitionId?(this.m_currentWeapon.DefinitionId)) && ((inventory != null) && (inventory is MyInventory)))) && !(inventory as MyInventory).ContainItems(1, this.m_currentWeapon.PhysicalObject))
                {
                    this.SwitchToWeapon((MyToolbarItemWeapon) null);
                }
                if ((this.LeftHandItem != null) && !this.CanSwitchToWeapon(new MyDefinitionId?(this.LeftHandItem.DefinitionId)))
                {
                    this.LeftHandItem.OnControlReleased();
                    this.m_leftHandItem.Close();
                    this.m_leftHandItem = null;
                }
            }
        }

        private void inventory_OnRemovedFromContainer(MyEntityComponentBase component)
        {
            this.GetInventory(0).BeforeRemovedFromContainer -= new Action<MyEntityComponentBase>(this.inventory_OnRemovedFromContainer);
            this.GetInventory(0).ContentsChanged -= new Action<MyInventoryBase>(this.inventory_OnContentsChanged);
            this.GetInventory(0).BeforeContentsChanged -= new Action<MyInventoryBase>(this.inventory_OnBeforeContentsChanged);
        }

        public bool IsLocalHeadAnimationInProgress() => 
            (this.m_currentLocalHeadAnimation >= 0f);

        public static bool IsRunningState(MyCharacterMovementEnum state) => 
            ((state > MyCharacterMovementEnum.RunningLeftFront) ? ((state > MyCharacterMovementEnum.RunStrafingRight) ? ((state == MyCharacterMovementEnum.RunningRightFront) || ((state == MyCharacterMovementEnum.RunningRightBack) || (state == MyCharacterMovementEnum.Sprinting))) : ((state == MyCharacterMovementEnum.RunningLeftBack) || (state == MyCharacterMovementEnum.RunStrafingRight))) : ((state > MyCharacterMovementEnum.Backrunning) ? ((state == MyCharacterMovementEnum.RunStrafingLeft) || (state == MyCharacterMovementEnum.RunningLeftFront)) : ((state == MyCharacterMovementEnum.Running) || (state == MyCharacterMovementEnum.Backrunning))));

        public bool IsShooting(MyShootActionEnum action) => 
            this.m_isShooting[(int) action];

        public static bool IsWalkingState(MyCharacterMovementEnum state) => 
            ((state > MyCharacterMovementEnum.CrouchStrafingRight) ? ((state > MyCharacterMovementEnum.Backrunning) ? ((state > MyCharacterMovementEnum.RunningLeftBack) ? ((state > MyCharacterMovementEnum.RunningRightFront) ? ((state == MyCharacterMovementEnum.RunningRightBack) || (state == MyCharacterMovementEnum.Sprinting)) : ((state == MyCharacterMovementEnum.RunStrafingRight) || (state == MyCharacterMovementEnum.RunningRightFront))) : ((state == MyCharacterMovementEnum.RunStrafingLeft) || ((state == MyCharacterMovementEnum.RunningLeftFront) || (state == MyCharacterMovementEnum.RunningLeftBack)))) : ((state > MyCharacterMovementEnum.WalkingRightBack) ? ((state == MyCharacterMovementEnum.CrouchWalkingRightBack) || ((state == MyCharacterMovementEnum.Running) || (state == MyCharacterMovementEnum.Backrunning))) : ((state == MyCharacterMovementEnum.WalkingRightFront) || ((state == MyCharacterMovementEnum.CrouchWalkingRightFront) || (state == MyCharacterMovementEnum.WalkingRightBack))))) : ((state > MyCharacterMovementEnum.CrouchStrafingLeft) ? ((state > MyCharacterMovementEnum.WalkingLeftBack) ? ((state == MyCharacterMovementEnum.CrouchWalkingLeftBack) || ((state == MyCharacterMovementEnum.WalkStrafingRight) || (state == MyCharacterMovementEnum.CrouchStrafingRight))) : ((state == MyCharacterMovementEnum.WalkingLeftFront) || ((state == MyCharacterMovementEnum.CrouchWalkingLeftFront) || (state == MyCharacterMovementEnum.WalkingLeftBack)))) : ((state > MyCharacterMovementEnum.BackWalking) ? ((state == MyCharacterMovementEnum.CrouchBackWalking) || ((state == MyCharacterMovementEnum.WalkStrafingLeft) || (state == MyCharacterMovementEnum.CrouchStrafingLeft))) : ((state == MyCharacterMovementEnum.Walking) || ((state == MyCharacterMovementEnum.CrouchWalking) || (state == MyCharacterMovementEnum.BackWalking))))));

        [Event(null, 0x18cd), Reliable, Server(ValidationType.Controlled)]
        public void Jump(Vector3 moveIndicator)
        {
            if ((this.m_currentMovementState != MyCharacterMovementEnum.Died) && this.HasEnoughSpaceToStandUp())
            {
                MyTuple<ushort, MyStringHash> tuple;
                if ((this.StatComp != null) && !this.StatComp.CanDoAction("Jump", out tuple, this.m_currentMovementState == MyCharacterMovementEnum.Jump))
                {
                    if (((MySession.Static != null) && (ReferenceEquals(MySession.Static.LocalCharacter, this) && ((tuple.Item1 == 4) && (tuple.Item2.String.CompareTo("Stamina") == 0)))) && (this.m_notEnoughStatNotification != null))
                    {
                        object[] arguments = new object[] { tuple.Item2 };
                        this.m_notEnoughStatNotification.SetTextFormatArguments(arguments);
                        MyHud.Notifications.Add(this.m_notEnoughStatNotification);
                    }
                }
                else if (this.IsMagneticBootsActive)
                {
                    if (Sync.IsServer || this.IsClientPredicted)
                    {
                        Vector3D? position = null;
                        Vector3? torque = null;
                        float? maxSpeed = null;
                        this.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, new Vector3?((Vector3) (1000f * this.Physics.SupportNormal)), position, torque, maxSpeed, true, false);
                    }
                    this.StartFalling();
                }
                else if (!this.IsOnLadder)
                {
                    this.WantsJump = true;
                }
                else
                {
                    if (Sync.IsServer)
                    {
                        this.GetOffLadder();
                    }
                    else
                    {
                        this.GetOffLadder_Implementation();
                    }
                    if (Sync.IsServer || this.IsClientPredicted)
                    {
                        Vector3 jumpDirection = (Vector3) base.WorldMatrix.Backward;
                        if (this.MoveIndicator.X > 0f)
                        {
                            jumpDirection = (Vector3) base.WorldMatrix.Right;
                        }
                        else if (this.MoveIndicator.X < 0f)
                        {
                            jumpDirection = (Vector3) base.WorldMatrix.Left;
                        }
                        MySandboxGame.Static.Invoke(delegate {
                            Vector3D? position = null;
                            Vector3? torque = null;
                            float? maxSpeed = null;
                            this.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, new Vector3?((Vector3) (1000f * jumpDirection)), position, torque, maxSpeed, true, false);
                        }, "Ladder jump");
                    }
                }
            }
        }

        public void Kill(bool sync, MyDamageInformation damageInfo)
        {
            if ((!this.m_dieAfterSimulation && !this.IsDead) && (!MyFakes.DEVELOPMENT_PRESET || (damageInfo.Type == MyDamageType.Suicide)))
            {
                if (sync)
                {
                    this.KillCharacter(damageInfo);
                }
                else
                {
                    if (this.UseDamageSystem)
                    {
                        MyDamageSystem.Static.RaiseDestroyed(this, damageInfo);
                    }
                    MyAnalyticsHelper.SetLastDamageInformation(damageInfo);
                    this.StatComp.LastDamage = damageInfo;
                    this.m_dieAfterSimulation = true;
                }
            }
        }

        private void KillCharacter(MyDamageInformation damageInfo)
        {
            this.Kill(false, damageInfo);
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, MyDamageInformation, Vector3>(this, x => new Action<MyDamageInformation, Vector3>(x.OnKillCharacter), damageInfo, this.Physics.LinearVelocity, targetEndpoint);
        }

        private void Ladder_OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            this.m_needReconnectLadder = true;
            this.m_oldLadderGrid = oldGrid;
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        internal float LimitMaxSpeed(float currentSpeed, MyCharacterMovementEnum movementState, float serverRatio)
        {
            float num = currentSpeed;
            if (movementState > MyCharacterMovementEnum.WalkingRightBack)
            {
                if (movementState > MyCharacterMovementEnum.RunningLeftBack)
                {
                    if (movementState > MyCharacterMovementEnum.Sprinting)
                    {
                        if (movementState <= MyCharacterMovementEnum.CrouchRotatingLeft)
                        {
                            if ((movementState != MyCharacterMovementEnum.RotatingLeft) && (movementState != MyCharacterMovementEnum.CrouchRotatingLeft))
                            {
                            }
                        }
                        else if (((movementState != MyCharacterMovementEnum.RotatingRight) && (movementState != MyCharacterMovementEnum.CrouchRotatingRight)) && (movementState != MyCharacterMovementEnum.LadderOut))
                        {
                        }
                        return num;
                    }
                    else if (movementState > MyCharacterMovementEnum.RunningRightFront)
                    {
                        if (movementState != MyCharacterMovementEnum.RunningRightBack)
                        {
                            if (movementState == MyCharacterMovementEnum.Sprinting)
                            {
                                num = MathHelper.Clamp(currentSpeed, -this.Definition.MaxSprintSpeed * serverRatio, this.Definition.MaxSprintSpeed * serverRatio);
                            }
                            return num;
                        }
                    }
                    else
                    {
                        if ((movementState != MyCharacterMovementEnum.RunStrafingRight) && (movementState != MyCharacterMovementEnum.RunningRightFront))
                        {
                            return num;
                        }
                        goto TR_0024;
                    }
                    goto TR_0026;
                }
                else if (movementState > MyCharacterMovementEnum.Running)
                {
                    if (movementState > MyCharacterMovementEnum.RunStrafingLeft)
                    {
                        if (movementState == MyCharacterMovementEnum.RunningLeftFront)
                        {
                            goto TR_0024;
                        }
                        else if (movementState != MyCharacterMovementEnum.RunningLeftBack)
                        {
                            return num;
                        }
                    }
                    else if (movementState != MyCharacterMovementEnum.Backrunning)
                    {
                        if (movementState != MyCharacterMovementEnum.RunStrafingLeft)
                        {
                            return num;
                        }
                        goto TR_0024;
                    }
                    goto TR_0026;
                }
                else
                {
                    if (movementState > MyCharacterMovementEnum.LadderUp)
                    {
                        if ((movementState == MyCharacterMovementEnum.LadderDown) || (movementState != MyCharacterMovementEnum.Running))
                        {
                            return num;
                        }
                        goto TR_0003;
                    }
                    else if (movementState != MyCharacterMovementEnum.CrouchWalkingRightBack)
                    {
                        if (movementState != MyCharacterMovementEnum.LadderUp)
                        {
                        }
                        return num;
                    }
                    goto TR_000C;
                }
            }
            else
            {
                if (movementState > MyCharacterMovementEnum.WalkingLeftFront)
                {
                    if (movementState > MyCharacterMovementEnum.WalkStrafingRight)
                    {
                        if (movementState > MyCharacterMovementEnum.WalkingRightFront)
                        {
                            if (movementState != MyCharacterMovementEnum.CrouchWalkingRightFront)
                            {
                                if (movementState != MyCharacterMovementEnum.WalkingRightBack)
                                {
                                    return num;
                                }
                                goto TR_0005;
                            }
                        }
                        else if (movementState != MyCharacterMovementEnum.CrouchStrafingRight)
                        {
                            if (movementState != MyCharacterMovementEnum.WalkingRightFront)
                            {
                                return num;
                            }
                            goto TR_000A;
                        }
                        goto TR_000F;
                    }
                    else if (movementState > MyCharacterMovementEnum.WalkingLeftBack)
                    {
                        if (movementState == MyCharacterMovementEnum.CrouchWalkingLeftBack)
                        {
                            goto TR_000C;
                        }
                        else if (movementState != MyCharacterMovementEnum.WalkStrafingRight)
                        {
                            return num;
                        }
                    }
                    else
                    {
                        if (movementState != MyCharacterMovementEnum.CrouchWalkingLeftFront)
                        {
                            if (movementState != MyCharacterMovementEnum.WalkingLeftBack)
                            {
                                return num;
                            }
                            goto TR_0005;
                        }
                        goto TR_000F;
                    }
                    goto TR_000A;
                }
                else if (movementState > MyCharacterMovementEnum.BackWalking)
                {
                    if (movementState > MyCharacterMovementEnum.WalkStrafingLeft)
                    {
                        if (movementState == MyCharacterMovementEnum.CrouchStrafingLeft)
                        {
                            goto TR_000F;
                        }
                        else if (movementState != MyCharacterMovementEnum.WalkingLeftFront)
                        {
                            return num;
                        }
                    }
                    else if (movementState == MyCharacterMovementEnum.CrouchBackWalking)
                    {
                        goto TR_000C;
                    }
                    else if (movementState != MyCharacterMovementEnum.WalkStrafingLeft)
                    {
                        return num;
                    }
                    goto TR_000A;
                }
                else
                {
                    if (movementState > MyCharacterMovementEnum.Walking)
                    {
                        if (movementState == MyCharacterMovementEnum.CrouchWalking)
                        {
                            return MathHelper.Clamp(currentSpeed, -this.Definition.MaxCrouchWalkSpeed * serverRatio, this.Definition.MaxCrouchWalkSpeed * serverRatio);
                        }
                        else if (movementState != MyCharacterMovementEnum.BackWalking)
                        {
                            return num;
                        }
                    }
                    else
                    {
                        switch (movementState)
                        {
                            case MyCharacterMovementEnum.Standing:
                            case MyCharacterMovementEnum.Sitting:
                            case MyCharacterMovementEnum.Crouching:
                            case MyCharacterMovementEnum.Falling:
                            case MyCharacterMovementEnum.Jump:
                            case MyCharacterMovementEnum.Died:
                            case MyCharacterMovementEnum.Ladder:
                                return num;

                            case MyCharacterMovementEnum.Flying:
                                break;

                            default:
                                if (movementState == MyCharacterMovementEnum.Walking)
                                {
                                    num = MathHelper.Clamp(currentSpeed, -this.Definition.MaxWalkSpeed * serverRatio, this.Definition.MaxWalkSpeed * serverRatio);
                                }
                                return num;
                        }
                        goto TR_0003;
                    }
                    goto TR_0005;
                }
                goto TR_000F;
            }
            goto TR_0024;
        TR_0003:
            return MathHelper.Clamp(currentSpeed, -this.Definition.MaxRunSpeed * serverRatio, this.Definition.MaxRunSpeed * serverRatio);
        TR_0005:
            return MathHelper.Clamp(currentSpeed, -this.Definition.MaxBackwalkSpeed * serverRatio, this.Definition.MaxBackwalkSpeed * serverRatio);
        TR_000A:
            return MathHelper.Clamp(currentSpeed, -this.Definition.MaxWalkStrafingSpeed * serverRatio, this.Definition.MaxWalkStrafingSpeed * serverRatio);
        TR_000C:
            return MathHelper.Clamp(currentSpeed, -this.Definition.MaxCrouchBackwalkSpeed * serverRatio, this.Definition.MaxCrouchBackwalkSpeed * serverRatio);
        TR_000F:
            return MathHelper.Clamp(currentSpeed, -this.Definition.MaxCrouchStrafingSpeed * serverRatio, this.Definition.MaxCrouchStrafingSpeed * serverRatio);
        TR_0024:
            return MathHelper.Clamp(currentSpeed, -this.Definition.MaxRunStrafingSpeed * serverRatio, this.Definition.MaxRunStrafingSpeed * serverRatio);
        TR_0026:
            return MathHelper.Clamp(currentSpeed, -this.Definition.MaxBackrunSpeed * serverRatio, this.Definition.MaxBackrunSpeed * serverRatio);
        }

        private void m_ladder_OnClose(VRage.Game.Entity.MyEntity obj)
        {
            if (ReferenceEquals(obj, this.m_ladder))
            {
                this.GetOffLadder_Implementation();
            }
        }

        public void MoveAndRotate(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
            if (((moveIndicator == Vector3.Zero) && (rotationIndicator == Vector2.Zero)) && (rollIndicator == 0f))
            {
                if (((this.MoveIndicator != moveIndicator) || (rotationIndicator != this.RotationIndicator)) || (this.RollIndicator != rollIndicator))
                {
                    this.MoveIndicator = Vector3.Zero;
                    this.RotationIndicator = Vector2.Zero;
                    this.RollIndicator = 0f;
                    this.m_moveAndRotateStopped = true;
                }
            }
            else
            {
                this.MoveIndicator = moveIndicator;
                this.RotationIndicator = rotationIndicator;
                this.RollIndicator = rollIndicator;
                this.m_moveAndRotateCalled = true;
                if ((ReferenceEquals(this, MySession.Static.LocalCharacter) && MyInput.Static.IsAnyCtrlKeyPressed()) && MyInput.Static.IsAnyAltKeyPressed())
                {
                    if (MyInput.Static.PreviousMouseScrollWheelValue() < MyInput.Static.MouseScrollWheelValue())
                    {
                        this.RotationSpeed = Math.Min((float) (this.RotationSpeed * 1.5f), (float) 0.13f);
                    }
                    else if (MyInput.Static.PreviousMouseScrollWheelValue() > MyInput.Static.MouseScrollWheelValue())
                    {
                        this.RotationSpeed = Math.Max((float) (this.RotationSpeed / 1.5f), (float) 0.01f);
                    }
                }
            }
        }

        internal void MoveAndRotateInternal(Vector3 moveIndicator, Vector2 rotationIndicator, float roll, Vector3 rotationCenter)
        {
            if (this.Physics != null)
            {
                PerFrameData data;
                int num1;
                int num5;
                int num6;
                if (((this.Physics.CharacterProxy == null) && this.IsDead) && !this.JetpackRunning)
                {
                    moveIndicator = Vector3.Zero;
                    rotationIndicator = Vector2.Zero;
                    roll = 0f;
                }
                if (MySessionComponentReplay.Static.IsEntityBeingReplayed(base.EntityId, out data))
                {
                    if (data.MovementData != null)
                    {
                        moveIndicator = (Vector3) data.MovementData.Value.MoveVector;
                        rotationIndicator = new Vector2(data.MovementData.Value.RotateVector.X, data.MovementData.Value.RotateVector.Y);
                        roll = data.MovementData.Value.RotateVector.Z;
                        this.MovementFlags = (MyCharacterMovementFlags) data.MovementData.Value.MovementFlags;
                    }
                }
                else if (MySessionComponentReplay.Static.IsEntityBeingRecorded(base.EntityId))
                {
                    PerFrameData data2 = new PerFrameData();
                    MovementData data3 = new MovementData {
                        MoveVector = moveIndicator,
                        RotateVector = new SerializableVector3(rotationIndicator.X, rotationIndicator.Y, roll),
                        MovementFlags = (byte) this.MovementFlags
                    };
                    data2.MovementData = new MovementData?(data3);
                    data = data2;
                    MySessionComponentReplay.Static.ProvideEntityRecordData(base.EntityId, data);
                }
                bool sprint = (moveIndicator.Z != 0f) && this.WantsSprint;
                bool wantsWalk = this.WantsWalk;
                bool wantsJump = this.WantsJump;
                if ((this.JetpackRunning || (((this.m_currentCharacterState == HkCharacterStateType.HK_CHARACTER_IN_AIR) || (this.m_currentCharacterState == ((HkCharacterStateType) 5))) && (this.m_currentJumpTime <= 0f))) || (this.m_currentMovementState == MyCharacterMovementEnum.Died))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) !this.IsFalling;
                }
                bool canMove = (bool) num1;
                if ((this.JetpackRunning || ((this.m_currentCharacterState != HkCharacterStateType.HK_CHARACTER_IN_AIR) && (this.m_currentCharacterState != ((HkCharacterStateType) 5)))) || (this.m_currentJumpTime > 0f))
                {
                    num5 = (int) (this.m_currentMovementState != MyCharacterMovementEnum.Died);
                }
                else
                {
                    num5 = 0;
                }
                bool canRotate = (bool) num5;
                if ((this.m_isFalling || (this.m_currentJumpTime > 0f)) || (this.Physics.CharacterProxy == null))
                {
                    num6 = 0;
                }
                else
                {
                    num6 = (int) (this.Physics.CharacterProxy.GetState() == HkCharacterStateType.HK_CHARACTER_IN_AIR);
                }
                bool flag6 = (bool) num6;
                if (this.IsOnLadder)
                {
                    Vector3 vector1 = this.ProceedLadderMovement(moveIndicator);
                    moveIndicator = vector1;
                }
                float acceleration = 0f;
                float currentSpeed = this.m_currentSpeed;
                if (this.JetpackRunning)
                {
                    this.JetpackComp.MoveAndRotate(ref moveIndicator, ref rotationIndicator, roll, canRotate);
                }
                else if (!((canMove || this.m_movementsFlagsChanged) | flag6))
                {
                    if (this.Physics.CharacterProxy != null)
                    {
                        this.Physics.CharacterProxy.Elevate = 0f;
                    }
                }
                else
                {
                    if (moveIndicator.LengthSquared() > 0f)
                    {
                        Vector3 vector2 = Vector3.Normalize(moveIndicator);
                        moveIndicator = vector2;
                    }
                    MyCharacterMovementEnum movementState = this.GetNewMovementState(ref moveIndicator, ref rotationIndicator, ref acceleration, sprint, wantsWalk, canMove, this.m_movementsFlagsChanged);
                    this.SwitchAnimation(movementState, true);
                    this.m_movementsFlagsChanged = false;
                    this.SetCurrentMovementState(movementState);
                    if ((movementState == MyCharacterMovementEnum.Sprinting) && (this.StatComp != null))
                    {
                        this.StatComp.ApplyModifier("Sprint");
                    }
                    if (!this.IsIdle)
                    {
                        this.m_currentWalkDelay = MathHelper.Clamp(this.m_currentWalkDelay - 0.01666667f, 0f, this.m_currentWalkDelay);
                    }
                    if (canMove)
                    {
                        float serverRatio = 1f;
                        this.m_currentSpeed = this.LimitMaxSpeed(this.m_currentSpeed + ((this.m_currentWalkDelay <= 0f) ? (acceleration * 0.01666667f) : 0f), this.m_currentMovementState, serverRatio);
                    }
                    if (this.Physics.CharacterProxy != null)
                    {
                        this.Physics.CharacterProxy.PosX = (this.m_currentMovementState != MyCharacterMovementEnum.Sprinting) ? -moveIndicator.X : 0f;
                        this.Physics.CharacterProxy.PosY = moveIndicator.Z;
                        this.Physics.CharacterProxy.Elevate = 0f;
                    }
                    if (canMove && (this.m_currentMovementState != MyCharacterMovementEnum.Jump))
                    {
                        int num3 = Math.Sign(this.m_currentSpeed);
                        this.m_currentSpeed += (-num3 * this.m_currentDecceleration) * 0.01666667f;
                        if (Math.Sign(num3) != Math.Sign(this.m_currentSpeed))
                        {
                            this.m_currentSpeed = 0f;
                        }
                    }
                    if (this.Physics.CharacterProxy != null)
                    {
                        this.Physics.CharacterProxy.Speed = (this.m_currentMovementState != MyCharacterMovementEnum.Died) ? this.m_currentSpeed : 0f;
                    }
                    this.m_currentMovementDirection = moveIndicator;
                    if ((this.Physics.CharacterProxy != null) && (this.Physics.CharacterProxy.GetHitRigidBody() != null))
                    {
                        if (wantsJump && (this.m_currentMovementState != MyCharacterMovementEnum.Jump))
                        {
                            this.PlayCharacterAnimation("Jump", MyBlendOption.Immediate, MyFrameOption.StayOnLastFrame, 0f, 1.3f, false, null, false);
                            if (base.UseNewAnimationSystem)
                            {
                                this.TriggerCharacterAnimationEvent("jump", true);
                            }
                            if (this.StatComp != null)
                            {
                                this.StatComp.DoAction("Jump");
                                this.StatComp.ApplyModifier("Jump");
                            }
                            this.m_currentJumpTime = 0.55f;
                            this.SetCurrentMovementState(MyCharacterMovementEnum.Jump);
                            this.m_canJump = false;
                            this.m_frictionBeforeJump = this.Physics.CharacterProxy.GetHitRigidBody().Friction;
                            this.Physics.CharacterProxy.Jump = true;
                        }
                        if (this.m_currentJumpTime > 0f)
                        {
                            this.m_currentJumpTime -= 0.01666667f;
                            this.Physics.CharacterProxy.GetHitRigidBody().Friction = 0f;
                        }
                        if ((this.m_currentJumpTime <= 0f) && (this.m_currentMovementState == MyCharacterMovementEnum.Jump))
                        {
                            this.Physics.CharacterProxy.GetHitRigidBody().Friction = this.m_frictionBeforeJump;
                            if (this.m_currentCharacterState != HkCharacterStateType.HK_CHARACTER_ON_GROUND)
                            {
                                this.StartFalling();
                            }
                            else
                            {
                                MyCharacterMovementEnum standing = MyCharacterMovementEnum.Standing;
                                if ((this.Physics.CharacterProxy != null) && ((this.Physics.CharacterProxy.GetState() == HkCharacterStateType.HK_CHARACTER_IN_AIR) || (this.Physics.CharacterProxy.GetState() == ((HkCharacterStateType) 5))))
                                {
                                    this.StartFalling();
                                }
                                else if (!this.IsFalling)
                                {
                                    if ((moveIndicator.X == 0f) && (moveIndicator.Z == 0f))
                                    {
                                        standing = MyCharacterMovementEnum.Standing;
                                        this.PlayCharacterAnimation("Idle", MyBlendOption.WaitForPreviousEnd, MyFrameOption.Loop, 0.2f, 1f, false, null, false);
                                    }
                                    else if (this.WantsCrouch)
                                    {
                                        if (moveIndicator.Z < 0f)
                                        {
                                            standing = MyCharacterMovementEnum.CrouchWalking;
                                            this.PlayCharacterAnimation("CrouchWalk", MyBlendOption.WaitForPreviousEnd, MyFrameOption.Loop, 0.2f, 1f, false, null, false);
                                        }
                                        else
                                        {
                                            standing = MyCharacterMovementEnum.CrouchBackWalking;
                                            this.PlayCharacterAnimation("CrouchWalkBack", MyBlendOption.WaitForPreviousEnd, MyFrameOption.Loop, 0.2f, 1f, false, null, false);
                                        }
                                    }
                                    else if (moveIndicator.Z >= 0f)
                                    {
                                        standing = MyCharacterMovementEnum.BackWalking;
                                        this.PlayCharacterAnimation("WalkBack", MyBlendOption.WaitForPreviousEnd, MyFrameOption.Loop, 0.5f, 1f, false, null, false);
                                    }
                                    else if (sprint)
                                    {
                                        standing = MyCharacterMovementEnum.Sprinting;
                                        this.PlayCharacterAnimation("Sprint", MyBlendOption.WaitForPreviousEnd, MyFrameOption.Loop, 0.2f, 1f, false, null, false);
                                    }
                                    else
                                    {
                                        standing = MyCharacterMovementEnum.Walking;
                                        this.PlayCharacterAnimation("Walk", MyBlendOption.WaitForPreviousEnd, MyFrameOption.Loop, 0.5f, 1f, false, null, false);
                                    }
                                    if (!this.m_canJump)
                                    {
                                        this.SoundComp.PlayFallSound();
                                    }
                                    this.m_canJump = true;
                                    this.SetCurrentMovementState(standing);
                                }
                            }
                            this.m_currentJumpTime = 0f;
                        }
                    }
                }
                this.UpdateHeadOffset();
                if (!this.JetpackRunning && !this.IsOnLadder)
                {
                    if ((rotationIndicator.Y != 0f) && ((canRotate || this.m_isFalling) || (this.m_currentJumpTime > 0f)))
                    {
                        if (this.Physics.CharacterProxy != null)
                        {
                            MatrixD xd = MatrixD.CreateRotationY((double) ((-rotationIndicator.Y * this.RotationSpeed) * 0.02f)) * MatrixD.CreateWorld(this.Physics.CharacterProxy.Position, this.Physics.CharacterProxy.Forward, this.Physics.CharacterProxy.Up);
                            this.Physics.CharacterProxy.SetForwardAndUp((Vector3) xd.Forward, (Vector3) xd.Up);
                        }
                        else
                        {
                            MatrixD worldMatrix = MatrixD.CreateRotationY((double) ((-rotationIndicator.Y * this.RotationSpeed) * 0.02f)) * base.WorldMatrix;
                            worldMatrix.Translation = base.WorldMatrix.Translation;
                            base.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                        }
                    }
                    if ((rotationIndicator.X != 0f) && (((this.m_currentMovementState == MyCharacterMovementEnum.Died) && !this.m_isInFirstPerson) || (this.m_currentMovementState != MyCharacterMovementEnum.Died)))
                    {
                        this.SetHeadLocalXAngle(this.m_headLocalXAngle - (rotationIndicator.X * this.RotationSpeed));
                        int index = this.IsInFirstPersonView ? this.m_headBoneIndex : this.m_camera3rdBoneIndex;
                        if (index != -1)
                        {
                            this.m_bobQueue.Clear();
                            this.m_bobQueue.Enqueue(base.BoneAbsoluteTransforms[index].Translation);
                        }
                    }
                }
                if ((this.Physics.CharacterProxy != null) && (this.Physics.CharacterProxy.LinearVelocity.LengthSquared() > 0.1f))
                {
                    this.m_shapeContactPoints.Clear();
                }
                this.WantsJump = false;
                this.WantsFlyUp = false;
                this.WantsFlyDown = false;
            }
        }

        public void MoveAndRotateStopped()
        {
        }

        private void MyEntity_OnClosing(VRage.Game.Entity.MyEntity entity)
        {
            if ((entity as MyCharacter).DeadPlayerIdentityId == MySession.Static.LocalPlayerId)
            {
                this.RadioReceiver.Clear();
            }
        }

        private void MyLadder_IsWorkingChanged(MyCubeBlock obj)
        {
            if (Sync.IsServer && !obj.IsWorking)
            {
                this.GetOffLadder();
            }
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            bool local1 = (this.IsUsing is MyCockpit) && !(this.IsUsing as MyCockpit).BlockDefinition.EnableFirstPerson;
            if (this.m_currentMovementState == MyCharacterMovementEnum.Sitting)
            {
                this.EnableBag(this.m_enableBag);
            }
            if (this.m_currentWeapon != null)
            {
                Sandbox.Game.Entities.MyEntities.Remove((VRage.Game.Entity.MyEntity) this.m_currentWeapon);
                Sandbox.Game.Entities.MyEntities.Add((VRage.Game.Entity.MyEntity) this.m_currentWeapon, true);
            }
            this.UpdateShadowIgnoredObjects();
            MyPlayerCollection.UpdateControl(this);
        }

        [Event(null, 0x2799), Reliable, Server(ValidationType.Controlled), Broadcast]
        private void OnAnimationCommand(MyAnimationCommand command)
        {
            this.AddCommand(command, false);
        }

        [Event(null, 0x27a4), Reliable, Server(ValidationType.Controlled), Broadcast]
        private void OnAnimationEvent(string eventName)
        {
            if (base.UseNewAnimationSystem)
            {
                base.AnimationController.TriggerAction(MyStringId.GetOrCompute(eventName));
            }
        }

        protected override void OnAnimationPlay(MyAnimationDefinition animDefinition, MyAnimationCommand command, ref string bonesArea, ref MyFrameOption frameOption, ref bool useFirstPersonVersion)
        {
            MyCharacterMovementEnum currentMovementState = this.GetCurrentMovementState();
            if (((currentMovementState != MyCharacterMovementEnum.Standing) && ((currentMovementState != MyCharacterMovementEnum.RotatingLeft) && (currentMovementState != MyCharacterMovementEnum.RotatingRight))) && command.ExcludeLegsWhenMoving)
            {
                bonesArea = TopBody;
                frameOption = (frameOption != MyFrameOption.JustFirstFrame) ? MyFrameOption.PlayOnce : frameOption;
            }
            useFirstPersonVersion = this.IsInFirstPersonView;
            if (animDefinition.AllowWithWeapon)
            {
                this.m_resetWeaponAnimationState = true;
            }
        }

        public void OnAssumeControl(IMyCameraController previousCameraController)
        {
        }

        public void OnBeginShoot(MyShootActionEnum action)
        {
            if ((this.ControllerInfo != null) && (this.m_currentWeapon != null))
            {
                MyGunStatusEnum oK = MyGunStatusEnum.OK;
                this.m_currentWeapon.CanShoot(action, this.ControllerInfo.ControllingIdentityId, out oK);
                MyGunStatusEnum enum1 = oK;
                if ((oK != MyGunStatusEnum.OK) && (oK != MyGunStatusEnum.Cooldown))
                {
                    this.ShootBeginFailed(action, oK);
                }
            }
        }

        private void OnBootsStateChanged(SyncBase obj)
        {
            if ((!Sync.IsDedicated && (this.SoundComp != null)) && (this.Render != null))
            {
                switch (this.m_bootsState.Value)
                {
                    case MyBootsState.Disabled:
                        MyRenderProxy.UpdateColorEmissivity(this.Render.RenderObjectIDs[0], 0, "Emissive", Color.White, 0f);
                        this.SoundComp.PlayMagneticBootsEnd();
                        break;

                    case MyBootsState.Proximity:
                        MyRenderProxy.UpdateColorEmissivity(this.Render.RenderObjectIDs[0], 0, "Emissive", Color.Yellow, 1f);
                        this.SoundComp.PlayMagneticBootsProximity();
                        break;

                    case MyBootsState.Enabled:
                        MyRenderProxy.UpdateColorEmissivity(this.Render.RenderObjectIDs[0], 0, "Emissive", Color.ForestGreen, 1f);
                        this.SoundComp.PlayMagneticBootsStart();
                        break;

                    default:
                        break;
                }
            }
            this.m_movementsFlagsChanged = true;
        }

        private void OnCharacterStateChanged(HkCharacterStateType newState)
        {
            if (this.m_currentCharacterState != newState)
            {
                if (this.m_currentMovementState != MyCharacterMovementEnum.Died)
                {
                    if (!this.JetpackRunning && !this.IsOnLadder)
                    {
                        if (((this.m_currentJumpTime <= 0f) && (newState == HkCharacterStateType.HK_CHARACTER_IN_AIR)) || (newState == ((HkCharacterStateType) 5)))
                        {
                            this.StartFalling();
                        }
                        else if (this.m_isFalling)
                        {
                            this.StopFalling();
                        }
                    }
                    if (this.JetpackRunning)
                    {
                        this.m_currentJumpTime = 0f;
                    }
                }
                this.m_currentCharacterState = newState;
            }
        }

        private void OnControlAcquired(MyEntityController controller)
        {
            MyPlayer objA = controller.Player;
            this.m_idModule.Owner = objA.Identity.IdentityId;
            this.SetPlayer(controller.Player, true);
            if ((MyMultiplayer.Static != null) && Sync.IsServer)
            {
                this.IsPersistenceCharacter = true;
            }
            if (!objA.IsLocalPlayer)
            {
                base.DisplayName = objA.Identity.DisplayName;
                this.UpdateHudMarker();
            }
            else
            {
                if (ReferenceEquals(objA, MySession.Static.LocalHumanPlayer))
                {
                    MyHighlightSystem highlightSystem = MySession.Static.GetComponent<MyHighlightSystem>();
                    if (highlightSystem != null)
                    {
                        this.Render.RenderObjectIDs.ForEach<uint>(id => highlightSystem.AddHighlightOverlappingModel(id));
                        this.m_resolveHighlightOverlap = false;
                    }
                    MyHud.SetHudDefinition(this.Definition.HUD);
                    MyHud.HideAll();
                    MyHud.Crosshair.ResetToDefault(true);
                    MyHud.Crosshair.Recenter();
                    if (MyGuiScreenGamePlay.Static != null)
                    {
                        MySession.Static.CameraAttachedToChanged += new Action<IMyCameraController, IMyCameraController>(this.Static_CameraAttachedToChanged);
                    }
                    if ((MySession.Static.CameraController is VRage.Game.Entity.MyEntity) && !MySession.Static.GetComponent<MySessionComponentCutscenes>().IsCutsceneRunning)
                    {
                        Vector3D? position = null;
                        MySession.Static.SetCameraController(this.IsInFirstPersonView ? MyCameraControllerEnum.Entity : MyCameraControllerEnum.ThirdPersonSpectator, this, position);
                    }
                    MyHud.GravityIndicator.Entity = this;
                    MyHud.GravityIndicator.Show(null);
                    MyHud.OreMarkers.Visible = true;
                    MyHud.LargeTurretTargets.Visible = true;
                    if (MySession.Static.IsScenario)
                    {
                        MyHud.ScenarioInfo.Show(null);
                    }
                }
                MyCharacterJetpackComponent jetpackComp = this.JetpackComp;
                if (jetpackComp != null)
                {
                    jetpackComp.TurnOnJetpack(jetpackComp.TurnedOn, false, false);
                }
                this.m_suitBattery.OwnedByLocalPlayer = true;
                base.DisplayName = objA.Identity.DisplayName;
            }
            if (((this.StatComp != null) && (this.StatComp.Health != null)) && (this.StatComp.Health.Value <= 0f))
            {
                this.m_dieAfterSimulation = true;
            }
            else
            {
                if (this.m_currentWeapon != null)
                {
                    this.m_currentWeapon.OnControlAcquired(this);
                }
                this.UpdateCharacterPhysics(false);
                if (ReferenceEquals(this, MySession.Static.ControlledEntity) && (MyToolbarComponent.CharacterToolbar != null))
                {
                    MyToolbarComponent.CharacterToolbar.ItemChanged -= new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
                    MyToolbarComponent.CharacterToolbar.ItemChanged += new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
                }
            }
        }

        private void OnControlReleased(MyEntityController controller)
        {
            this.Static_CameraAttachedToChanged(null, null);
            if (!ReferenceEquals(MySession.Static.LocalHumanPlayer, controller.Player))
            {
                if (!MyFakes.ENABLE_RADIO_HUD)
                {
                    MyHud.LocationMarkers.UnregisterMarker(this);
                }
            }
            else
            {
                MyHud.SelectedObjectHighlight.RemoveHighlight();
                this.RemoveNotifications();
                if (MyGuiScreenGamePlay.Static != null)
                {
                    MySession.Static.CameraAttachedToChanged -= new Action<IMyCameraController, IMyCameraController>(this.Static_CameraAttachedToChanged);
                }
                MyHud.GravityIndicator.Hide();
                this.m_suitBattery.OwnedByLocalPlayer = false;
                MyHud.LargeTurretTargets.Visible = false;
                MyHud.OreMarkers.Visible = false;
                this.RadioReceiver.Clear();
                if (MyGuiScreenGamePlay.ActiveGameplayScreen != null)
                {
                    MyGuiScreenGamePlay.ActiveGameplayScreen.CloseScreen();
                }
                this.ResetMovement();
                MyCubeBuilder.Static.Deactivate();
            }
            this.SoundComp.StopStateSound(true);
            MyToolbarComponent.CharacterToolbar.ItemChanged -= new Action<MyToolbar, MyToolbar.IndexArgs>(this.Toolbar_ItemChanged);
        }

        public void OnDestroy()
        {
            this.Die();
        }

        public void OnEndShoot(MyShootActionEnum action)
        {
            if ((this.m_currentMovementState != MyCharacterMovementEnum.Died) && (this.m_currentWeapon != null))
            {
                this.m_currentWeapon.EndShoot(action);
                if (this.m_endShootAutoswitch != null)
                {
                    this.SwitchToWeapon(this.m_endShootAutoswitch, true);
                    this.m_endShootAutoswitch = null;
                }
            }
        }

        public void OnGunDoubleClicked(MyShootActionEnum action)
        {
            if ((this.m_currentMovementState != MyCharacterMovementEnum.Died) && (this.m_currentWeapon != null))
            {
                this.m_currentWeapon.DoubleClicked(action);
            }
        }

        public void OnInventoryBreak()
        {
        }

        [Event(null, 0x2671), Reliable, Broadcast]
        private void OnKillCharacter(MyDamageInformation damageInfo, Vector3 lastLinearVelocity)
        {
            this.m_deathLinearVelocityFromSever = new Vector3?(lastLinearVelocity);
            this.Kill(false, damageInfo);
        }

        [Event(null, 0x27bf), Reliable, Broadcast]
        private void OnRagdollTransformsUpdate(int transformsCount, Vector3[] transformsPositions, Quaternion[] transformsOrientations, Quaternion worldOrientation, Vector3 worldPosition)
        {
            MyCharacterRagdollComponent component = base.Components.Get<MyCharacterRagdollComponent>();
            if ((((((component != null) && (this.Physics != null)) && (this.Physics.Ragdoll != null)) && (component.RagdollMapper != null)) && this.Physics.Ragdoll.InWorld) && component.RagdollMapper.IsActive)
            {
                Matrix worldMatrix = Matrix.CreateFromQuaternion(worldOrientation);
                worldMatrix.Translation = worldPosition;
                Matrix[] transforms = new Matrix[transformsCount];
                for (int i = 0; i < transformsCount; i++)
                {
                    transforms[i] = Matrix.CreateFromQuaternion(transformsOrientations[i]);
                    transforms[i].Translation = transformsPositions[i];
                }
                component.RagdollMapper.UpdateRigidBodiesTransformsSynced(transformsCount, worldMatrix, transforms);
            }
        }

        [Event(null, 0x22bb), Reliable, Broadcast]
        private void OnRefillFromBottle(SerializableDefinitionId gasId)
        {
            if (ReferenceEquals(this, MySession.Static.LocalCharacter))
            {
                MyCharacterOxygenComponent oxygenComponent = this.OxygenComponent;
            }
        }

        public void OnReleaseControl(IMyCameraController newCameraController)
        {
        }

        public override void OnRemovedFromScene(object source)
        {
            MyHighlightSystem highlightSystem = MySession.Static.GetComponent<MyHighlightSystem>();
            if (highlightSystem != null)
            {
                this.Render.RenderObjectIDs.ForEach<uint>(id => highlightSystem.RemoveHighlightOverlappingModel(id));
            }
            base.OnRemovedFromScene(source);
            if (this.m_currentWeapon != null)
            {
                if (highlightSystem != null)
                {
                    ((VRage.Game.Entity.MyEntity) this.m_currentWeapon).Render.RenderObjectIDs.ForEach<uint>(id => highlightSystem.RemoveHighlightOverlappingModel(id));
                }
                ((VRage.Game.Entity.MyEntity) this.m_currentWeapon).OnRemovedFromScene(source);
            }
            if (this.m_leftHandItem != null)
            {
                this.m_leftHandItem.OnRemovedFromScene(source);
            }
            this.m_resolveHighlightOverlap = true;
        }

        [Event(null, 0x22c9), Reliable, Server(ValidationType.Controlled), BroadcastExcept]
        private void OnSecondarySoundPlay(MyCueId soundId)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.SoundComp.StartSecondarySound(soundId, false);
            }
        }

        [Event(null, 0x1e63), Reliable, Server(ValidationType.Controlled)]
        private void OnSuicideRequest()
        {
            this.DoDamage(1000f, MyDamageType.Suicide, true, base.EntityId);
        }

        [Event(null, 0x274e), Reliable, Server(ValidationType.Controlled)]
        private void OnSwitchAmmoMagazineRequest()
        {
            if (this.CanSwitchAmmoMagazine())
            {
                this.SwitchAmmoMagazineSuccess();
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCharacter>(this, x => new Action(x.OnSwitchAmmoMagazineSuccess), targetEndpoint);
            }
        }

        [Event(null, 0x275a), Reliable, Broadcast]
        private void OnSwitchAmmoMagazineSuccess()
        {
            this.SwitchAmmoMagazineSuccess();
        }

        [Event(null, 0x27dc), Reliable, Server(ValidationType.Controlled), Broadcast]
        private void OnSwitchHelmet()
        {
            if (this.OxygenComponent != null)
            {
                this.OxygenComponent.SwitchHelmet();
                if (this.m_currentWeapon != null)
                {
                    this.m_currentWeapon.UpdateSoundEmitter();
                }
            }
        }

        [Event(null, 0x278c), Reliable, Broadcast]
        private void OnSwitchToWeaponSuccess(SerializableDefinitionId? weapon, uint? inventoryItemId, long weaponEntityId)
        {
            MyDefinitionId? nullable1;
            SerializableDefinitionId? nullable = weapon;
            if (nullable != null)
            {
                nullable1 = new MyDefinitionId?(nullable.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            this.SwitchToWeaponSuccess(nullable1, inventoryItemId, weaponEntityId);
        }

        [Event(null, 0x22ad), Reliable, Broadcast]
        private void OnUpdateOxygen(float oxygenAmount)
        {
            if (this.OxygenComponent != null)
            {
                this.OxygenComponent.SuitOxygenAmount = oxygenAmount;
            }
        }

        public void PickUp()
        {
            if (!this.IsDead)
            {
                MyCharacterPickupComponent component = base.Components.Get<MyCharacterPickupComponent>();
                if (component != null)
                {
                    component.PickUp();
                }
            }
        }

        public void PickUpContinues()
        {
            if (!this.IsDead)
            {
                MyCharacterPickupComponent component = base.Components.Get<MyCharacterPickupComponent>();
                if (component != null)
                {
                    component.PickUpContinues();
                }
            }
        }

        public void PickUpFinished()
        {
            if (!this.IsDead)
            {
                MyCharacterPickupComponent component = base.Components.Get<MyCharacterPickupComponent>();
                if (component != null)
                {
                    component.PickUpFinished();
                }
            }
        }

        public void PlayCharacterAnimation(string animationName, MyBlendOption blendOption, MyFrameOption frameOption, float blendTime, float timeScale = 1f, bool sync = false, string influenceArea = null, bool excludeLegsWhenMoving = false)
        {
            if (!base.UseNewAnimationSystem)
            {
                bool flag = Sandbox.Engine.Platform.Game.IsDedicated && MyPerGameSettings.DisableAnimationsOnDS;
                if (((!flag || sync) && this.m_animationCommandsEnabled) && (animationName != null))
                {
                    string str = null;
                    if (!this.m_characterDefinition.AnimationNameToSubtypeName.TryGetValue(animationName, out str))
                    {
                        str = animationName;
                    }
                    MyAnimationCommand command = new MyAnimationCommand {
                        AnimationSubtypeName = str,
                        PlaybackCommand = MyPlaybackCommand.Play,
                        BlendOption = blendOption,
                        FrameOption = frameOption,
                        BlendTime = blendTime,
                        TimeScale = timeScale,
                        Area = influenceArea,
                        ExcludeLegsWhenMoving = excludeLegsWhenMoving
                    };
                    if (sync)
                    {
                        this.SendAnimationCommand(ref command);
                    }
                    else if (!flag)
                    {
                        this.AddCommand(command, sync);
                    }
                }
            }
        }

        public void PlaySecondarySound(MyCueId soundId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, MyCueId>(this, x => new Action<MyCueId>(x.OnSecondarySoundPlay), soundId, targetEndpoint);
        }

        public static void Preload()
        {
            using (List<MyAnimationDefinition>.Enumerator enumerator = MyDefinitionManager.Static.GetAnimationDefinitions().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string animationModel = enumerator.Current.AnimationModel;
                    if (!string.IsNullOrEmpty(animationModel))
                    {
                        MyModels.GetModelOnlyAnimationData(animationModel, false);
                    }
                }
            }
            if (MyModelImporter.LINEAR_KEYFRAME_REDUCTION_STATS)
            {
                List<float> list = new List<float>();
                foreach (KeyValuePair<string, List<MyModelImporter.ReductionInfo>> pair in MyModelImporter.ReductionStats)
                {
                    foreach (MyModelImporter.ReductionInfo info in pair.Value)
                    {
                        list.Add(((float) info.OptimizedKeys) / ((float) info.OriginalKeys));
                    }
                }
                ((IEnumerable<float>) list).Average();
            }
        }

        private Vector3 ProceedLadderMovement(Vector3 moveIndicator)
        {
            Vector3D position = base.PositionComp.GetPosition();
            Vector3 zero = Vector3.Zero;
            if ((moveIndicator.Z != 0f) && (this.m_currentLadderStep == 0))
            {
                bool flag;
                bool flag2;
                if (moveIndicator.Z < 0f)
                {
                    zero = (Vector3) ((base.WorldMatrix.Up * this.m_stepIncrement) * this.m_stepsPerAnimation);
                }
                if (moveIndicator.Z > 0f)
                {
                    zero = (Vector3) ((base.WorldMatrix.Down * this.m_stepIncrement) * this.m_stepsPerAnimation);
                }
                MyLadder ladder = this.CheckTopLadder(position, ref zero, out flag);
                MyLadder ladder2 = this.CheckBottomLadder(position, ref zero, out flag2);
                bool flag3 = false;
                bool forceStartAnimation = false;
                MyCharacterMovementEnum currentMovementState = this.GetCurrentMovementState();
                if (moveIndicator.Z < 0f)
                {
                    flag3 = (ladder != null) && ladder.IsFunctional;
                    if (flag3 && (this.GetCurrentMovementState() == MyCharacterMovementEnum.LadderDown))
                    {
                        this.m_currentLadderStep = this.m_stepsPerAnimation - this.m_currentLadderStep;
                        forceStartAnimation = this.m_currentLadderStep > (this.m_stepsPerAnimation / 2);
                    }
                    currentMovementState = MyCharacterMovementEnum.LadderUp;
                }
                if (moveIndicator.Z > 0f)
                {
                    flag3 = (ladder2 != null) && ladder2.IsFunctional;
                    if (flag3 && (this.GetCurrentMovementState() == MyCharacterMovementEnum.LadderUp))
                    {
                        this.m_currentLadderStep = this.m_stepsPerAnimation - this.m_currentLadderStep;
                        forceStartAnimation = this.m_currentLadderStep > (this.m_stepsPerAnimation / 2);
                    }
                    currentMovementState = MyCharacterMovementEnum.LadderDown;
                }
                if (flag3)
                {
                    this.SetCurrentMovementState(currentMovementState);
                    this.StartStep(forceStartAnimation);
                }
                else if (this.Physics.CharacterProxy == null)
                {
                    if (this.m_currentLadderStep == 0)
                    {
                        this.m_currentLadderStep = (2 * this.m_stepsPerAnimation) + 50;
                    }
                }
                else if ((moveIndicator.Z >= 0f) || flag)
                {
                    if (((moveIndicator.Z > 0f) && !flag2) && Sync.IsServer)
                    {
                        this.GetOffLadder();
                    }
                }
                else if (this.GetCurrentMovementState() != MyCharacterMovementEnum.LadderOut)
                {
                    this.m_currentLadderStep = (2 * this.m_stepsPerAnimation) + 50;
                    this.SetCurrentMovementState(MyCharacterMovementEnum.LadderOut);
                    this.TriggerCharacterAnimationEvent("LadderOut", false);
                }
            }
            return moveIndicator;
        }

        private void PromotedChanged()
        {
        }

        private unsafe void PutCharacterOnLadder(MyLadder ladder, bool resetPosition)
        {
            MatrixD* xdPtr1;
            Vector3 translation = (Vector3) (base.WorldMatrix * ladder.PositionComp.WorldMatrixInvScaled).Translation;
            MatrixD xd = MatrixD.Normalize(ladder.StartMatrix) * MatrixD.CreateRotationY(3.1415929794311523);
            float num = Vector3.Dot((Vector3) base.WorldMatrix.Up, (Vector3) ladder.PositionComp.WorldMatrix.Up);
            float y = ladder.StartMatrix.Translation.Y;
            if (num < 0f)
            {
                xd *= MatrixD.CreateRotationZ(3.1415929794311523);
                y = -y;
            }
            xdPtr1.Translation = new Vector3D((double) ladder.StartMatrix.Translation.X, resetPosition ? ((double) y) : ((double) translation.Y), (double) ladder.StartMatrix.Translation.Z);
            xdPtr1 = (MatrixD*) ref xd;
            float num3 = this.m_stepIncrement * this.m_currentLadderStep;
            if (num < 0f)
            {
                num3 *= -1f;
            }
            float num4 = y + ((this.GetCurrentMovementState() == MyCharacterMovementEnum.LadderUp) ? -num3 : num3);
            float num5 = (((int) ((((float) xd.Translation.Y) - num4) / ladder.DistanceBetweenPoles)) * ladder.DistanceBetweenPoles) + num4;
            MatrixD* xdPtr2 = (MatrixD*) ref xd;
            xdPtr2.Translation = new Vector3(xd.Translation.X, (double) num5, xd.Translation.Z);
            if (num < 0f)
            {
                num5 *= -1f;
            }
            this.m_ladderIncrementToBase = Vector3.Zero;
            this.m_ladderIncrementToBase.Y = num5;
            MatrixD worldMatrix = xd * ladder.WorldMatrix;
            if (this.Physics.CharacterProxy != null)
            {
                this.Physics.CharacterProxy.ImmediateSetWorldTransform = true;
            }
            MatrixD* xdPtr3 = (MatrixD*) ref xd;
            xdPtr3.Translation = new Vector3D(xd.Translation.X, 0.0, xd.Translation.Z);
            this.m_baseMatrix = xd;
            base.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
            if (this.Physics.CharacterProxy != null)
            {
                this.SetCharacterLadderConstraint(worldMatrix);
                this.Physics.CharacterProxy.ImmediateSetWorldTransform = false;
            }
        }

        public static MyObjectBuilder_Character Random()
        {
            MyObjectBuilder_Character character1 = new MyObjectBuilder_Character();
            character1.CharacterModel = DefaultModel;
            character1.SubtypeName = DefaultModel;
            character1.ColorMaskHSV = m_defaultColors[MyUtils.GetRandomInt(0, m_defaultColors.Length)];
            return character1;
        }

        private bool RayCastGround()
        {
            float num = MyConstants.DEFAULT_GROUND_SEARCH_DISTANCE;
            Vector3D from = base.PositionComp.GetPosition() + (base.PositionComp.WorldMatrix.Up * 0.5);
            Vector3D to = from + (base.PositionComp.WorldMatrix.Down * num);
            Vector3D vectord3 = base.PositionComp.WorldMatrix.Forward * 0.20000000298023224;
            Vector3D vectord4 = -base.PositionComp.WorldMatrix.Forward * 0.20000000298023224;
            this.m_hits.Clear();
            this.m_hits2.Clear();
            MyPhysics.CastRay(from, to, this.m_hits2, 0x12);
            this.m_hits.AddList<MyPhysics.HitInfo>(this.m_hits2);
            MyPhysics.CastRay(from + vectord3, to + vectord3, this.m_hits2, 0x12);
            this.m_hits.AddList<MyPhysics.HitInfo>(this.m_hits2);
            MyPhysics.CastRay(from + vectord4, to + vectord4, this.m_hits2, 0x12);
            this.m_hits.AddList<MyPhysics.HitInfo>(this.m_hits2);
            int num2 = 0;
            while ((num2 < this.m_hits.Count) && ((this.m_hits[num2].HkHitInfo.Body == null) || ReferenceEquals(this.m_hits[num2].HkHitInfo.GetHitEntity(), this.Entity.Components)))
            {
                num2++;
            }
            if (this.m_hits.Count == 0)
            {
                this.m_standingOnGrid = null;
                this.m_standingOnVoxel = null;
            }
            if (num2 < this.m_hits.Count)
            {
                MyPhysics.HitInfo local1 = this.m_hits[num2];
                VRage.ModAPI.IMyEntity hitEntity = local1.HkHitInfo.GetHitEntity();
                if (Vector3D.DistanceSquared(local1.Position, from) < (num * num))
                {
                    MyCubeGrid grid = hitEntity as MyCubeGrid;
                    MyVoxelBase base2 = hitEntity as MyVoxelBase;
                    this.m_standingOnGrid = grid;
                    this.m_standingOnVoxel = base2;
                }
            }
            this.m_hits.Clear();
            return ((this.m_standingOnGrid != null) || (this.m_standingOnVoxel != null));
        }

        internal void RecalculatePowerRequirement(bool chargeImmediatelly = false)
        {
            this.SinkComp.Update();
            this.UpdateLightPower(chargeImmediatelly);
        }

        private void ReconnectConstraint(MyCubeGrid oldLadderGrid, MyCubeGrid newLadderGrid)
        {
            this.CloseLadderConstraint(oldLadderGrid);
            if (newLadderGrid != null)
            {
                this.AddLadderConstraint(newLadderGrid);
            }
        }

        [Event(null, 0x201d), Reliable, Server, Broadcast]
        private static void RefreshAssetModifiers(long playerId, long entityId)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && (((MySession.Static.LocalHumanPlayer != null) && (MySession.Static.LocalHumanPlayer.Identity.IdentityId == playerId)) && (MyGameService.InventoryItems != null)))
            {
                List<MyGameInventoryItem> items = new List<MyGameInventoryItem>();
                foreach (MyGameInventoryItem item in MyGameService.InventoryItems)
                {
                    if (item.IsInUse)
                    {
                        items.Add(item);
                    }
                }
                MyGameService.GetItemsCheckData(items, delegate (byte[] checkDataResult) {
                    if (checkDataResult != null)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<long, byte[]>(x => new Action<long, byte[]>(MyCharacter.SendSkinData), entityId, checkDataResult, targetEndpoint, position);
                    }
                });
            }
        }

        private void relativeDampeningEntityClosed(VRage.Game.Entity.MyEntity entity)
        {
            this.m_relativeDampeningEntity = null;
        }

        public void RemoveNotification(ref MyHudNotification notification)
        {
            if (notification != null)
            {
                MyHud.Notifications.Remove(notification);
                notification = null;
            }
        }

        private void RemoveNotifications()
        {
            this.RemoveNotification(ref this.m_pickupObjectNotification);
            this.RemoveNotification(ref this.m_respawnNotification);
        }

        private void RequestSwitchToWeapon(MyDefinitionId? weapon, uint? inventoryItemId)
        {
            SerializableDefinitionId? nullable1;
            MyDefinitionId? nullable2 = weapon;
            if (nullable2 != null)
            {
                nullable1 = new SerializableDefinitionId?(nullable2.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            SerializableDefinitionId? nullable = nullable1;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, SerializableDefinitionId?, uint?>(this, x => new Action<SerializableDefinitionId?, uint?>(x.SwitchToWeaponMessage), nullable, inventoryItemId, targetEndpoint);
        }

        public override void ResetControls()
        {
            this.ResetMovement();
            this.m_lastClientState.Valid = false;
        }

        public void ResetHeadRotation()
        {
            if (base.m_actualUpdateFrame > 0L)
            {
                this.m_headLocalYAngle = 0f;
                this.m_headLocalXAngle = 0f;
            }
        }

        private void ResetMovement()
        {
            this.MoveIndicator = Vector3.Zero;
            this.RotationIndicator = Vector2.Zero;
            this.RollIndicator = 0f;
        }

        public bool ResponsibleForUpdate(MyNetworkClient player)
        {
            if (Sync.Players == null)
            {
                return false;
            }
            MyPlayer controllingPlayer = Sync.Players.GetControllingPlayer(this);
            if ((controllingPlayer == null) && (this.CurrentRemoteControl != null))
            {
                controllingPlayer = Sync.Players.GetControllingPlayer(this.CurrentRemoteControl as VRage.Game.Entity.MyEntity);
            }
            return ((controllingPlayer != null) ? ReferenceEquals(controllingPlayer.Client, player) : player.IsGameServer());
        }

        private void RigidBody_ContactPointCallback(ref HkContactPointEvent value)
        {
            if ((((((!this.IsDead && (this.Physics != null)) && (this.Physics.CharacterProxy != null)) && (MySession.Static != null)) && ((value.Base.BodyA != null) && (value.Base.BodyB != null))) && ((value.Base.BodyA.UserObject != null) && (value.Base.BodyB.UserObject != null))) && (!value.Base.BodyA.HasProperty(HkCharacterRigidBody.MANIPULATED_OBJECT) && !value.Base.BodyB.HasProperty(HkCharacterRigidBody.MANIPULATED_OBJECT)))
            {
                if (this.Render != null)
                {
                    this.Render.TrySpawnWalkingParticles(ref value);
                }
                int index = 0;
                Vector3 normal = value.ContactPoint.Normal;
                VRage.Game.Entity.MyEntity other = value.GetPhysicsBody(index).Entity as VRage.Game.Entity.MyEntity;
                HkRigidBody bodyA = value.Base.BodyA;
                if (ReferenceEquals(other, this))
                {
                    index = 1;
                    other = value.GetPhysicsBody(index).Entity as VRage.Game.Entity.MyEntity;
                    bodyA = value.Base.BodyB;
                    normal = -normal;
                }
                MyCharacter character = other as MyCharacter;
                if ((character != null) && (character.Physics != null))
                {
                    if (character.IsDead)
                    {
                        if ((character.Physics.Ragdoll != null) && character.Physics.Ragdoll.GetRootRigidBody().HasProperty(HkCharacterRigidBody.MANIPULATED_OBJECT))
                        {
                            return;
                        }
                    }
                    else if ((character.Physics.CharacterProxy == null) || (this.Physics.CharacterProxy.Supported && character.Physics.CharacterProxy.Supported))
                    {
                        return;
                    }
                }
                MyCubeGrid cubeGrid = other as MyCubeGrid;
                if (cubeGrid != null)
                {
                    if (this.IsOnLadder)
                    {
                        uint shapeKey = value.GetShapeKey(index);
                        bool flag = shapeKey == uint.MaxValue;
                        if (!flag)
                        {
                            MySlimBlock blockFromShapeKey = cubeGrid.Physics.Shape.GetBlockFromShapeKey(shapeKey);
                            if (blockFromShapeKey != null)
                            {
                                MyLadder fatBlock = blockFromShapeKey.FatBlock as MyLadder;
                                flag = (fatBlock != null) && !this.ShouldCollideWith(fatBlock);
                            }
                        }
                        if (flag)
                        {
                            value.ContactProperties.IsDisabled = true;
                        }
                    }
                    if (MyFakes.ENABLE_REALISTIC_ON_TOUCH && (this.SoundComp != null))
                    {
                        this.SoundComp.UpdateEntityEmitters(cubeGrid);
                    }
                }
                if (Math.Abs(value.SeparatingVelocity) >= 3f)
                {
                    Vector3 linearVelocity = this.Physics.LinearVelocity;
                    if ((linearVelocity - this.m_previousLinearVelocity).Length() <= 10f)
                    {
                        Vector3 velocityAtPoint = bodyA.GetVelocityAtPoint(value.ContactPoint.Position);
                        float num2 = linearVelocity.Length();
                        float num3 = velocityAtPoint.Length();
                        Vector3 vector5 = (num2 > 0f) ? Vector3.Normalize(linearVelocity) : Vector3.Zero;
                        Vector3 vector6 = (num3 > 0f) ? Vector3.Normalize(velocityAtPoint) : Vector3.Zero;
                        float num4 = (num2 > 0f) ? Vector3.Dot(vector5, normal) : 0f;
                        num3 *= (num3 > 0f) ? -Vector3.Dot(vector6, normal) : 0f;
                        float num6 = Math.Min((float) ((num2 * num4) + num3), (float) (Math.Abs(value.SeparatingVelocity) - 17f));
                        if ((num6 >= -8f) && (this.m_canPlayImpact <= 0f))
                        {
                            this.m_canPlayImpact = 0.3f;
                            HkContactPointEvent hkContactPointEvent = value;
                            Func<bool> canHear = delegate {
                                if (MySession.Static.ControlledEntity == null)
                                {
                                    return false;
                                }
                                VRage.Game.Entity.MyEntity topMostParent = MySession.Static.ControlledEntity.Entity.GetTopMostParent(null);
                                return ReferenceEquals(topMostParent, hkContactPointEvent.GetPhysicsBody(0).Entity) || ReferenceEquals(topMostParent, hkContactPointEvent.GetPhysicsBody(1).Entity);
                            };
                            Vector3D position = this.Physics.ClusterToWorld(value.ContactPoint.Position);
                            MyStringHash materialAt = value.Base.BodyB.GetBody().GetMaterialAt(position - (value.ContactPoint.Normal * 0.1f));
                            MyAudioComponent.PlayContactSound(this.Entity.EntityId, m_stringIdHit, position, this.m_physicalMaterialHash, materialAt, (Math.Abs(value.SeparatingVelocity) < 15f) ? (0.5f + (Math.Abs(value.SeparatingVelocity) / 30f)) : 1f, canHear, null, 0f);
                        }
                        if (Sync.IsServer && (num6 >= 0f))
                        {
                            float num9;
                            float num8 = MyDestructionHelper.MassFromHavok(bodyA.Mass);
                            if ((MyDestructionHelper.MassFromHavok(this.Physics.Mass) > num8) && !bodyA.IsFixedOrKeyframed)
                            {
                                num9 = num8;
                            }
                            else
                            {
                                num9 = MyDestructionHelper.MassToHavok(70f);
                                if (this.Physics.CharacterProxy.Supported && !bodyA.IsFixedOrKeyframed)
                                {
                                    num9 += (Math.Abs(Vector3.Dot(Vector3.Normalize(velocityAtPoint), this.Physics.CharacterProxy.SupportNormal)) * num8) / 10f;
                                }
                            }
                            float impact = ((MyDestructionHelper.MassFromHavok(num9) * num6) * num6) / 2f;
                            if (num3 > 2f)
                            {
                                impact -= 400f;
                            }
                            else if ((num3 == 0f) && (impact > 100f))
                            {
                                impact /= 80f;
                            }
                            impact /= 10f;
                            if ((impact >= 1f) && Sync.IsServer)
                            {
                                if (ReferenceEquals(value.GetPhysicsBody(0).Entity, this))
                                {
                                    VRage.ModAPI.IMyEntity entity = value.GetPhysicsBody(1).Entity;
                                }
                                MySandboxGame.Static.Invoke(() => this.DoDamage(impact, MyDamageType.Environment, true, (other != null) ? other.EntityId : 0L), "MyCharacter.DoDamage");
                            }
                        }
                    }
                }
            }
        }

        public void Rotate(Vector2 rotationIndicator, float roll)
        {
            if (this.IsInFirstPersonView)
            {
                this.RotateHead(rotationIndicator);
            }
            else
            {
                this.RotateHead(rotationIndicator);
                MyThirdPersonSpectator.Static.Rotate(rotationIndicator, roll);
            }
        }

        private void RotateHead(Vector2 rotationIndicator)
        {
            if (rotationIndicator.X != 0f)
            {
                this.SetHeadLocalXAngle(this.m_headLocalXAngle - (rotationIndicator.X * 0.5f));
            }
            if (rotationIndicator.Y != 0f)
            {
                float num = -rotationIndicator.Y * 0.5f;
                this.SetHeadLocalYAngle(this.m_headLocalYAngle + num);
            }
        }

        public void RotateStopped()
        {
        }

        void Sandbox.Game.Entities.IMyControllableEntity.PickUpFinished()
        {
            this.PickUpFinished();
        }

        void Sandbox.Game.Entities.IMyControllableEntity.Sprint(bool enabled)
        {
            this.Sprint(enabled);
        }

        void Sandbox.Game.Entities.IMyControllableEntity.UseFinished()
        {
            this.UseFinished();
        }

        private void SaveAmmoToWeapon()
        {
        }

        public void SendAnimationCommand(ref MyAnimationCommand command)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, MyAnimationCommand>(this, x => new Action<MyAnimationCommand>(x.OnAnimationCommand), command, targetEndpoint);
        }

        public void SendAnimationEvent(string eventName)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, string>(this, x => new Action<string>(x.OnAnimationEvent), eventName, targetEndpoint);
        }

        public void SendRagdollTransforms(Matrix world, Matrix[] localBodiesTransforms)
        {
            if (this.ResponsibleForUpdate(Sync.Clients.LocalClient))
            {
                Vector3 translation = world.Translation;
                int length = localBodiesTransforms.Length;
                Quaternion quaternion = Quaternion.CreateFromRotationMatrix(world.GetOrientation());
                Vector3[] vectorArray = new Vector3[length];
                Quaternion[] quaternionArray = new Quaternion[length];
                int index = 0;
                while (true)
                {
                    if (index >= localBodiesTransforms.Length)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyCharacter, int, Vector3[], Quaternion[], Quaternion, Vector3>(this, x => new Action<int, Vector3[], Quaternion[], Quaternion, Vector3>(x.OnRagdollTransformsUpdate), length, vectorArray, quaternionArray, quaternion, translation, targetEndpoint);
                        break;
                    }
                    vectorArray[index] = localBodiesTransforms[index].Translation;
                    quaternionArray[index] = Quaternion.CreateFromRotationMatrix(localBodiesTransforms[index].GetOrientation());
                    index++;
                }
            }
        }

        public void SendRefillFromBottle(MyDefinitionId gasId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, SerializableDefinitionId>(this, x => new Action<SerializableDefinitionId>(x.OnRefillFromBottle), (SerializableDefinitionId) gasId, targetEndpoint);
        }

        [Event(null, 0x2043), Reliable, Server, Broadcast]
        private static void SendSkinData(long entityId, byte[] checkDataResult)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyCharacter entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(entityId, false) as MyCharacter;
                if (entityById != null)
                {
                    MyAssetModifierComponent component;
                    if (entityById.Components.TryGet<MyAssetModifierComponent>(out component))
                    {
                        MyAssetModifierComponent.ApplyAssetModifierSync(entityId, checkDataResult, true);
                    }
                    VRage.Game.Entity.MyEntity currentWeapon = entityById.CurrentWeapon as VRage.Game.Entity.MyEntity;
                    if (((currentWeapon != null) && (entityById.CurrentWeapon != null)) && entityById.CurrentWeapon.IsSkinnable)
                    {
                        MyAssetModifierComponent.ApplyAssetModifierSync(currentWeapon.EntityId, checkDataResult, true);
                    }
                }
            }
        }

        public override void SerializeControls(BitStream stream)
        {
            if (this.IsDead)
            {
                stream.WriteBool(false);
            }
            else
            {
                MyCharacterClientState state;
                stream.WriteBool(true);
                this.GetNetState(out state);
                state.Serialize(stream);
                if (MyCompilationSymbols.EnableNetworkPositionTracking)
                {
                    this.MoveIndicator.Equals(Vector3.Zero, 0.001f);
                }
            }
        }

        private void SetCharacterLadderConstraint(MatrixD characterWM)
        {
            characterWM.Translation = this.Physics.WorldToCluster(characterWM.Translation) + Vector3D.TransformNormal(this.Physics.Center, characterWM);
            Matrix pivotA = Matrix.Invert(this.m_ladder.Parent.Physics.RigidBody.GetRigidBodyMatrix());
            pivotA = Matrix.CreateWorld((Vector3) characterWM.Translation) * pivotA;
            Matrix pivotB = Matrix.CreateWorld((Vector3) characterWM.Translation) * Matrix.Invert((Matrix) characterWM);
            this.m_constraintData.SetInBodySpaceInternal(ref pivotA, ref pivotB);
        }

        private void SetCrouchingLocalAABB()
        {
            float x = this.Definition.CharacterCollisionWidth / 2f;
            base.PositionComp.LocalAABB = new BoundingBox(-new Vector3(x, 0f, x), new Vector3(x, this.Definition.CharacterCollisionHeight / 2f, x));
        }

        internal void SetCurrentMovementState(MyCharacterMovementEnum state)
        {
            if (this.m_currentMovementState != state)
            {
                this.m_previousMovementState = this.m_currentMovementState;
                this.m_currentMovementState = state;
                this.UpdateCrouchState();
                if (this.OnMovementStateChanged != null)
                {
                    this.OnMovementStateChanged(this.m_previousMovementState, this.m_currentMovementState);
                }
                if (this.MovementStateChanged != null)
                {
                    this.MovementStateChanged(this, this.m_previousMovementState, this.m_currentMovementState);
                }
            }
        }

        public void SetHandAdditionalRotation(Quaternion rotation, bool updateSync = true)
        {
            if (!string.IsNullOrEmpty(this.Definition.LeftForearmBone) && (base.GetAdditionalRotation(this.Definition.LeftForearmBone) != rotation))
            {
                base.m_additionalRotations[this.Definition.LeftForearmBone] = rotation;
                base.m_additionalRotations[this.Definition.RightForearmBone] = Quaternion.Inverse(rotation);
            }
        }

        public void SetHeadAdditionalRotation(Quaternion rotation, bool updateSync = true)
        {
            if (!string.IsNullOrEmpty(this.Definition.HeadBone) && (base.GetAdditionalRotation(this.Definition.HeadBone) != rotation))
            {
                base.m_additionalRotations[this.Definition.HeadBone] = rotation;
            }
        }

        internal void SetHeadLocalXAngle(float angle)
        {
            this.HeadLocalXAngle = angle;
        }

        private void SetHeadLocalYAngle(float angle)
        {
            this.HeadLocalYAngle = angle;
        }

        public void SetLocalHeadAnimation(float? targetX, float? targetY, float length)
        {
            if (length > 0f)
            {
                if (this.m_headLocalYAngle >= 0f)
                {
                    this.m_headLocalYAngle = ((this.m_headLocalYAngle + 180f) % 360f) - 180f;
                }
                else
                {
                    this.m_headLocalYAngle = -this.m_headLocalYAngle;
                    this.m_headLocalYAngle = ((this.m_headLocalYAngle + 180f) % 360f) - 180f;
                    this.m_headLocalYAngle = -this.m_headLocalYAngle;
                }
            }
            this.m_currentLocalHeadAnimation = 0f;
            this.m_localHeadAnimationLength = length;
            this.m_localHeadAnimationX = (targetX == null) ? null : ((Vector2?) new Vector2(this.m_headLocalXAngle, targetX.Value));
            if (targetY != null)
            {
                this.m_localHeadAnimationY = new Vector2(this.m_headLocalYAngle, targetY.Value);
            }
            else
            {
                this.m_localHeadAnimationY = null;
            }
        }

        public void SetNetState(ref MyCharacterClientState state)
        {
            if ((!this.IsDead && ((this.IsUsing == null) || this.IsOnLadder)) && !base.Closed)
            {
                bool flag = this.ControllerInfo.IsLocallyControlled();
                if (Sync.IsServer || !flag)
                {
                    if ((((state.MovementState == MyCharacterMovementEnum.LadderUp) || (state.MovementState == MyCharacterMovementEnum.LadderOut)) && (this.GetCurrentMovementState() == MyCharacterMovementEnum.Ladder)) && state.IsOnLadder)
                    {
                        state.MoveIndicator.Z = -1f;
                    }
                    if (((state.MovementState == MyCharacterMovementEnum.LadderDown) && (this.GetCurrentMovementState() == MyCharacterMovementEnum.Ladder)) && state.IsOnLadder)
                    {
                        state.MoveIndicator.Z = 1f;
                    }
                    this.SetHeadLocalXAngle(state.HeadX);
                    this.SetHeadLocalYAngle(state.HeadY);
                    MyCharacterJetpackComponent jetpackComp = this.JetpackComp;
                    if ((jetpackComp != null) && !this.IsOnLadder)
                    {
                        if (state.Jetpack != this.JetpackComp.TurnedOn)
                        {
                            jetpackComp.TurnOnJetpack(state.Jetpack, true, false);
                        }
                        if (state.Dampeners != this.JetpackComp.DampenersTurnedOn)
                        {
                            jetpackComp.EnableDampeners(state.Dampeners);
                        }
                    }
                    if ((this.GetCurrentMovementState() != state.MovementState) && (state.MovementState == MyCharacterMovementEnum.LadderOut))
                    {
                        this.TriggerCharacterAnimationEvent("LadderOut", false);
                    }
                    if ((this.IsOnLadder && state.IsOnLadder) || (!this.IsOnLadder && !state.IsOnLadder))
                    {
                        this.CacheMove(ref state.MoveIndicator, ref state.Rotation);
                    }
                    this.MovementFlags = state.MovementFlags | (this.MovementFlags & MyCharacterMovementFlags.Jump);
                    if (!this.IsOnLadder)
                    {
                        this.SetCurrentMovementState(state.MovementState);
                    }
                }
                if (Sync.IsServer)
                {
                    this.TargetFromCamera = state.TargetFromCamera;
                }
                if (!Sync.IsServer && (!this.IsClientPredicted || !flag))
                {
                    if ((this.m_previousMovementState == MyCharacterMovementEnum.Jump) && (state.CharacterState == HkCharacterStateType.HK_CHARACTER_ON_GROUND))
                    {
                        this.StopFalling();
                    }
                    this.m_currentSpeed = state.MovementSpeed;
                    this.m_currentMovementDirection = state.MovementDirection;
                    this.OnCharacterStateChanged(state.CharacterState);
                    this.Physics.SupportNormal = state.SupportNormal;
                }
            }
        }

        public void SetPhysicsEnabled(bool enabled)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, bool>(this, x => new Action<bool>(x.EnablePhysics), enabled, targetEndpoint);
        }

        public void SetPlayer(MyPlayer player, bool update = true)
        {
            if (Sync.IsServer)
            {
                this.m_controlInfo.Value = player.Id;
                if (update)
                {
                    MyPlayerCollection.ChangePlayerCharacter(player, this, this);
                }
                this.m_savedPlayer = new MyPlayer.PlayerId?(player.Id);
            }
        }

        private void SetPowerInput(float input)
        {
            if (!this.LightEnabled || (input < 2E-06f))
            {
                this.m_lightPowerFromProducer = 0f;
            }
            else
            {
                this.m_lightPowerFromProducer = 2E-06f;
                input -= 2E-06f;
            }
        }

        public void SetPreviousMovementState(MyCharacterMovementEnum previousMovementState)
        {
            this.m_previousMovementState = previousMovementState;
        }

        private void SetRelativeDampening(VRage.Game.Entity.MyEntity entity)
        {
            this.RelativeDampeningEntity = entity;
            this.JetpackComp.EnableDampeners(true);
            this.JetpackComp.TurnOnJetpack(true, false, false);
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, long>(s => new Action<long, long>(MyPlayerCollection.SetDampeningEntityClient), base.EntityId, this.RelativeDampeningEntity.EntityId, targetEndpoint, position);
            }
        }

        public void SetSpineAdditionalRotation(Quaternion rotation, Quaternion rotationForClients, bool updateSync = true)
        {
            if (!string.IsNullOrEmpty(this.Definition.SpineBone) && (base.GetAdditionalRotation(this.Definition.SpineBone) != rotation))
            {
                base.m_additionalRotations[this.Definition.SpineBone] = rotation;
            }
        }

        private void SetStandingLocalAABB()
        {
            float x = this.Definition.CharacterCollisionWidth / 2f;
            base.PositionComp.LocalAABB = new BoundingBox(-new Vector3(x, 0f, x), new Vector3(x, this.Definition.CharacterCollisionHeight, x));
        }

        public void SetupAutoswitch(MyDefinitionId? switchToNow, MyDefinitionId? switchOnEndShoot)
        {
            this.m_autoswitch = switchToNow;
            this.m_endShootAutoswitch = switchOnEndShoot;
        }

        public void SetUpperHandAdditionalRotation(Quaternion rotation, bool updateSync = true)
        {
            if (!string.IsNullOrEmpty(this.Definition.LeftUpperarmBone) && (base.GetAdditionalRotation(this.Definition.LeftUpperarmBone) != rotation))
            {
                base.m_additionalRotations[this.Definition.LeftUpperarmBone] = rotation;
                base.m_additionalRotations[this.Definition.RightUpperarmBone] = Quaternion.Inverse(rotation);
            }
        }

        [Event(null, 0x26f0), Reliable, Server(ValidationType.Controlled), BroadcastExcept]
        private void ShootBeginCallback(Vector3 direction, MyShootActionEnum action)
        {
            int isLocallyInvoked;
            if (Sync.IsServer)
            {
                isLocallyInvoked = (int) MyEventContext.Current.IsLocallyInvoked;
            }
            else
            {
                isLocallyInvoked = 0;
            }
            if (isLocallyInvoked == 0)
            {
                this.StartShooting(direction, action);
            }
        }

        private void ShootBeginFailed(MyShootActionEnum action, MyGunStatusEnum status)
        {
            this.m_currentWeapon.BeginFailReaction(action, status);
            this.m_isShooting[(int) action] = false;
            if (ReferenceEquals(MySession.Static.ControlledEntity, this))
            {
                this.m_currentWeapon.BeginFailReactionLocal(action, status);
            }
        }

        [Event(null, 0x2745), Reliable, Server(ValidationType.Controlled), BroadcastExcept]
        private void ShootDirectionChangeCallback(Vector3 direction)
        {
            if ((this.ControllerInfo == null) || !this.ControllerInfo.IsLocallyControlled())
            {
                this.ShootDirection = direction;
            }
        }

        [Event(null, 0x271d), Reliable, Server(ValidationType.Controlled), BroadcastExcept]
        private void ShootEndCallback(MyShootActionEnum action)
        {
            int isLocallyInvoked;
            if (Sync.IsServer)
            {
                isLocallyInvoked = (int) MyEventContext.Current.IsLocallyInvoked;
            }
            else
            {
                isLocallyInvoked = 0;
            }
            if (isLocallyInvoked == 0)
            {
                this.StopShooting(action);
            }
        }

        private void ShootFailedLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
            if (status == MyGunStatusEnum.OutOfAmmo)
            {
                this.ShowOutOfAmmoNotification();
            }
            this.m_currentWeapon.ShootFailReactionLocal(action, status);
        }

        private void ShootInternal()
        {
            MyGunStatusEnum oK = MyGunStatusEnum.OK;
            MyShootActionEnum? shootingAction = this.GetShootingAction();
            if (this.ControllerInfo != null)
            {
                if (Sync.IsServer)
                {
                    this.m_currentAmmoCount.Value = this.m_currentWeapon.CurrentAmmunition;
                    this.m_currentMagazineAmmoCount.Value = this.m_currentWeapon.CurrentMagazineAmmunition;
                }
                else
                {
                    this.m_currentWeapon.CurrentMagazineAmmunition = (int) this.m_currentMagazineAmmoCount;
                    this.m_currentWeapon.CurrentAmmunition = (int) this.m_currentAmmoCount;
                }
                this.m_aimedPoint = this.GetAimedPointFromCamera();
                this.UpdateShootDirection(this.m_currentWeapon.DirectionToTarget(this.m_aimedPoint), this.m_currentWeapon.ShootDirectionUpdateTime);
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_TOOLS)
                {
                    float num = 20f;
                    MatrixD xd = MatrixD.Invert(ref this.GetViewMatrix());
                    MyRenderProxy.DebugDrawLine3D(xd.Translation, xd.Translation + (xd.Forward * num), Color.LightGreen, Color.LightGreen, false, false);
                    MatrixD? cameraViewMatrix = null;
                    MyDebugDrawHelper.DrawNamedPoint(xd.Translation + (xd.Forward * 5.0), "crosshair", new Color?(Color.LightGreen), cameraViewMatrix);
                    MyRenderProxy.DebugDrawLine3D(this.WeaponPosition.LogicalPositionWorld, this.WeaponPosition.LogicalPositionWorld + (this.ShootDirection * num), Color.Red, Color.Red, false, false);
                    cameraViewMatrix = null;
                    MyDebugDrawHelper.DrawNamedPoint(this.WeaponPosition.LogicalPositionWorld + (this.ShootDirection * 5f), "shootdir", new Color?(Color.Red), cameraViewMatrix);
                    cameraViewMatrix = null;
                    MyDebugDrawHelper.DrawNamedPoint(this.m_aimedPoint, "aimed", new Color?(Color.White), cameraViewMatrix);
                }
                if ((shootingAction != null) && this.m_currentWeapon.CanShoot(shootingAction.Value, this.ControllerInfo.ControllingIdentityId, out oK))
                {
                    if (Sandbox.Engine.Platform.Game.IsDedicated)
                    {
                        this.m_currentWeapon.Shoot(shootingAction.Value, this.ShootDirection, new Vector3D?(this.WeaponPosition.LogicalPositionWorld), null);
                    }
                    else
                    {
                        Vector3D? overrideWeaponPos = null;
                        this.m_currentWeapon.Shoot(shootingAction.Value, this.ShootDirection, overrideWeaponPos, null);
                    }
                }
                if (this.m_currentWeapon != null)
                {
                    if (ReferenceEquals(MySession.Static.ControlledEntity, this))
                    {
                        if ((oK != MyGunStatusEnum.OK) && (oK != MyGunStatusEnum.Cooldown))
                        {
                            this.ShootFailedLocal(shootingAction.Value, oK);
                        }
                        else if (((shootingAction != null) && this.m_currentWeapon.IsShooting) && (oK == MyGunStatusEnum.OK))
                        {
                            this.ShootSuccessfulLocal(shootingAction.Value);
                        }
                    }
                    if ((oK != MyGunStatusEnum.OK) && (oK != MyGunStatusEnum.Cooldown))
                    {
                        this.m_isShooting[shootingAction.Value] = false;
                    }
                }
                if (this.m_autoswitch != null)
                {
                    this.SwitchToWeapon(this.m_autoswitch, true);
                    this.m_autoswitch = null;
                }
            }
        }

        private void ShootSuccessfulLocal(MyShootActionEnum action)
        {
            this.m_currentShotTime = 0.1f;
            this.WeaponPosition.AddBackkick(this.m_currentWeapon.BackkickForcePerSecond * 0.01666667f);
            MyCharacterJetpackComponent jetpackComp = this.JetpackComp;
            if ((this.m_currentWeapon.BackkickForcePerSecond > 0f) && (this.JetpackRunning || this.m_isFalling))
            {
                Vector3? torque = null;
                float? maxSpeed = null;
                this.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, new Vector3?((Vector3) (-this.m_currentWeapon.BackkickForcePerSecond * (this.m_currentWeapon as VRage.Game.Entity.MyEntity).WorldMatrix.Forward)), new Vector3D?(base.PositionComp.GetPosition()), torque, maxSpeed, true, false);
            }
        }

        private bool ShouldCollideWith(MyLadder ladder) => 
            false;

        public bool ShouldEndShootingOnPause(MyShootActionEnum action) => 
            ((this.m_currentMovementState == MyCharacterMovementEnum.Died) || ((this.m_currentWeapon == null) || this.m_currentWeapon.ShouldEndShootOnPause(action)));

        private bool ShouldUseAnimatedHeadRotation() => 
            false;

        public MyGuiScreenBase ShowAggregateInventoryScreen(MyInventoryBase rightSelectedInventory = null)
        {
            if ((MyPerGameSettings.GUI.InventoryScreen != null) && (this.InventoryAggregate != null))
            {
                this.InventoryAggregate.Init();
                object[] args = new object[] { this.InventoryAggregate, rightSelectedInventory };
                this.m_InventoryScreen = MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.InventoryScreen, args);
                MyGuiSandbox.AddScreen(this.m_InventoryScreen);
                this.m_InventoryScreen.Closed += delegate (MyGuiScreenBase scr) {
                    if (this.InventoryAggregate != null)
                    {
                        this.InventoryAggregate.DetachCallbacks();
                    }
                    this.m_InventoryScreen = null;
                };
            }
            return this.m_InventoryScreen;
        }

        public void ShowInventory()
        {
            if ((this.m_currentMovementState != MyCharacterMovementEnum.Died) && (this.m_currentMovementState != MyCharacterMovementEnum.Died))
            {
                MyCharacterDetectorComponent component = base.Components.Get<MyCharacterDetectorComponent>();
                if ((component.UseObject != null) && component.UseObject.IsActionSupported(UseActionEnum.OpenInventory))
                {
                    component.UseObject.Use(UseActionEnum.OpenInventory, this);
                }
                else if (MyPerGameSettings.TerminalEnabled)
                {
                    MyGuiScreenTerminal.Show(MyTerminalPageEnum.Inventory, this, null);
                }
                else if (base.HasInventory && (this.GetInventory(0) != null))
                {
                    this.ShowAggregateInventoryScreen(this.GetInventory(0));
                }
            }
        }

        public void ShowOutOfAmmoNotification()
        {
            if (OutOfAmmoNotification == null)
            {
                OutOfAmmoNotification = new MyHudNotification(MyCommonTexts.OutOfAmmo, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            }
            if (this.m_currentWeapon is VRage.Game.Entity.MyEntity)
            {
                object[] arguments = new object[] { (this.m_currentWeapon as VRage.Game.Entity.MyEntity).DisplayName };
                OutOfAmmoNotification.SetTextFormatArguments(arguments);
            }
            MyHud.Notifications.Add(OutOfAmmoNotification);
        }

        public void ShowTerminal()
        {
            if (this.m_currentMovementState != MyCharacterMovementEnum.Died)
            {
                MyCharacterDetectorComponent component = base.Components.Get<MyCharacterDetectorComponent>();
                if ((MyToolbarComponent.CharacterToolbar == null) || !(MyToolbarComponent.CharacterToolbar.SelectedItem is MyToolbarItemVoxelHand))
                {
                    if ((component.UseObject != null) && component.UseObject.IsActionSupported(UseActionEnum.OpenTerminal))
                    {
                        component.UseObject.Use(UseActionEnum.OpenTerminal, this);
                    }
                    else if (MyPerGameSettings.TerminalEnabled)
                    {
                        MyGuiScreenTerminal.Show(MyTerminalPageEnum.Inventory, this, null);
                    }
                    else if (MyFakes.ENABLE_QUICK_WARDROBE)
                    {
                        MyGuiScreenWardrobe screen = new MyGuiScreenWardrobe(this, null);
                        MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
                        MyGuiSandbox.AddScreen(screen);
                    }
                    else if ((MyPerGameSettings.GUI.GameplayOptionsScreen != null) && !MySession.Static.SurvivalMode)
                    {
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.GameplayOptionsScreen, Array.Empty<object>()));
                    }
                }
            }
        }

        public override void Simulate()
        {
            base.Simulate();
            if (this.m_cachedCommands != null)
            {
                if ((this.IsUsing != null) && !this.IsOnLadder)
                {
                    this.m_cachedCommands.Clear();
                }
                foreach (IMyNetworkCommand command in this.m_cachedCommands)
                {
                    if (command.ExecuteBeforeMoveAndRotate)
                    {
                        command.Apply();
                    }
                }
            }
            if ((this.ControllerInfo.IsLocallyControlled() || ((((this.IsUsing == null) || this.IsOnLadder) && (this.m_cachedCommands != null)) && (this.m_cachedCommands.Count == 0))) || ((base.Parent == null) && MySessionComponentReplay.Static.IsEntityBeingReplayed(base.EntityId)))
            {
                this.MoveAndRotateInternal(this.MoveIndicator, this.RotationIndicator, this.RollIndicator, this.RotationCenterIndicator);
            }
            if (this.m_cachedCommands != null)
            {
                if (((this.IsUsing != null) && !this.IsOnLadder) || this.IsDead)
                {
                    this.m_cachedCommands.Clear();
                }
                foreach (IMyNetworkCommand command2 in this.m_cachedCommands)
                {
                    if (!command2.ExecuteBeforeMoveAndRotate)
                    {
                        command2.Apply();
                    }
                }
                this.m_cachedCommands.Clear();
            }
            foreach (MyCharacterComponent component in base.Components)
            {
                if (component == null)
                {
                    continue;
                }
                if (component.NeedsUpdateSimulation)
                {
                    component.Simulate();
                }
            }
            if ((!this.IsDead && ((this.m_currentMovementState != MyCharacterMovementEnum.Sitting) && !MySandboxGame.IsPaused)) && (this.Physics.CharacterProxy != null))
            {
                Vector3 linearVelocity = this.Physics.LinearVelocity;
                Vector3 angularVelocity = this.Physics.AngularVelocity;
                if (this.JetpackRunning)
                {
                    this.Physics.CharacterProxy.UpdateSupport(0.01666667f);
                    this.Physics.CharacterProxy.ApplyGravity(this.Physics.Gravity);
                    this.Physics.CharacterProxy.AngularVelocity = Vector3.Zero;
                }
                else
                {
                    bool supported = this.Physics.CharacterProxy.Supported;
                    this.Physics.CharacterProxy.GetSupportingEntities(this.m_supportedEntitiesTmp);
                    this.Physics.CharacterProxy.StepSimulation(0.01666667f);
                    bool flag2 = this.Physics.CharacterProxy.Supported;
                    if ((((!Sync.IsServer && !flag2) & supported) && (this.m_supportedEntitiesTmp.Count > 0)) && (this.m_supportedEntitiesTmp[0].Physics.RigidBody != null))
                    {
                        Vector3 vector3;
                        Vector3D translation = base.WorldMatrix.Translation;
                        this.m_supportedEntitiesTmp[0].Physics.GetVelocityAtPointLocal(ref translation, out vector3);
                        Vector3 vector4 = this.Physics.LinearVelocity - this.Physics.LinearVelocityLocal;
                        this.Physics.LinearVelocity = (this.Physics.LinearVelocityLocal + vector3) - vector4;
                    }
                    this.m_supportedEntitiesTmp.Clear();
                }
                if (!Sync.IsServer && !this.IsClientPredicted)
                {
                    this.Physics.LinearVelocity = linearVelocity;
                    this.Physics.AngularVelocity = angularVelocity;
                }
            }
        }

        public void Sit(bool enableFirstPerson, bool playerIsPilot, bool enableBag, string animation)
        {
            this.EndShootAll();
            MyDefinitionId? weaponDefinition = null;
            uint? inventoryItemId = null;
            this.SwitchToWeaponInternal(weaponDefinition, false, inventoryItemId, 0L);
            this.Render.NearFlag = false;
            this.m_isFalling = false;
            this.PlayCharacterAnimation(animation, MyBlendOption.Immediate, MyFrameOption.Loop, 0f, 1f, false, null, false);
            this.StopUpperCharacterAnimation(0f);
            this.StopFingersAnimation(0f);
            this.SetHandAdditionalRotation(Quaternion.CreateFromAxisAngle(Vector3.Forward, MathHelper.ToRadians((float) 0f)), true);
            this.SetUpperHandAdditionalRotation(Quaternion.CreateFromAxisAngle(Vector3.Forward, MathHelper.ToRadians((float) 0f)), true);
            if (base.UseNewAnimationSystem)
            {
                base.AnimationController.Variables.SetValue(MyAnimationVariableStorageHints.StrIdLean, 0f);
            }
            this.SetSpineAdditionalRotation(Quaternion.CreateFromAxisAngle(Vector3.Forward, 0f), Quaternion.CreateFromAxisAngle(Vector3.Forward, 0f), true);
            this.SetHeadAdditionalRotation(Quaternion.Identity, false);
            base.FlushAnimationQueue();
            this.SinkComp.Update();
            this.UpdateLightPower(true);
            this.EnableBag(enableBag);
            if (Sync.IsServer)
            {
                this.m_bootsState.Value = MyBootsState.Init;
            }
            this.SetCurrentMovementState(MyCharacterMovementEnum.Sitting);
            if (!Sandbox.Engine.Platform.Game.IsDedicated && base.UseNewAnimationSystem)
            {
                this.TriggerCharacterAnimationEvent("sit", false);
                if (!string.IsNullOrEmpty(animation))
                {
                    string str = string.Empty;
                    if (!this.Definition.AnimationNameToSubtypeName.TryGetValue(animation, out str))
                    {
                        str = animation;
                    }
                    this.TriggerCharacterAnimationEvent(str, false);
                }
            }
            this.UpdateAnimation(0f);
        }

        private void SlowDownX()
        {
            if (Math.Abs(this.m_headMovementXOffset) > 0f)
            {
                this.m_headMovementXOffset += Math.Sign(-this.m_headMovementXOffset) * this.m_headMovementStep;
                if (Math.Abs(this.m_headMovementXOffset) < this.m_headMovementStep)
                {
                    this.m_headMovementXOffset = 0f;
                }
            }
        }

        private void SlowDownY()
        {
            if (Math.Abs(this.m_headMovementYOffset) > 0f)
            {
                this.m_headMovementYOffset += Math.Sign(-this.m_headMovementYOffset) * this.m_headMovementStep;
                if (Math.Abs(this.m_headMovementYOffset) < this.m_headMovementStep)
                {
                    this.m_headMovementYOffset = 0f;
                }
            }
        }

        [Event(null, 0x2678), Reliable, Broadcast]
        public void SpawnCharacterRelative(long RelatedEntity, Vector3 DeltaPosition)
        {
            VRage.Game.Entity.MyEntity entity;
            if ((RelatedEntity != 0) && Sandbox.Game.Entities.MyEntities.TryGetEntityById(RelatedEntity, out entity, false))
            {
                this.Physics.LinearVelocity = entity.Physics.LinearVelocity;
                this.Physics.AngularVelocity = entity.Physics.AngularVelocity;
                MatrixD xd = Matrix.CreateTranslation(DeltaPosition) * entity.WorldMatrix;
                base.PositionComp.SetPosition(xd.Translation, null, false, true);
            }
        }

        public void Sprint(bool enabled)
        {
            if (this.WantsSprint != enabled)
            {
                this.WantsSprint = enabled;
            }
            if (this.WantsSprint && (this.m_zoomMode == MyZoomModeEnum.IronSight))
            {
                this.EnableIronsight(false, false, true, true);
                if (this.m_wasInThirdPersonBeforeIronSight)
                {
                    MyGuiScreenGamePlay.Static.SwitchCamera();
                }
            }
        }

        public void Stand()
        {
            this.PlayCharacterAnimation("Idle", MyBlendOption.Immediate, MyFrameOption.Loop, 0f, 1f, false, null, false);
            this.Render.NearFlag = false;
            this.StopUpperCharacterAnimation(0f);
            this.RecalculatePowerRequirement(false);
            this.EnableBag(true);
            this.UpdateHeadModelProperties(this.m_headRenderingEnabled);
            this.SetCurrentMovementState(MyCharacterMovementEnum.Standing);
            this.m_wasInFirstPerson = false;
            this.IsUsing = null;
            if (Sync.IsServer)
            {
                this.m_bootsState.Value = MyBootsState.Init;
            }
            if (this.Physics.CharacterProxy != null)
            {
                this.Physics.CharacterProxy.Stand();
            }
        }

        internal void StartFalling()
        {
            if ((!this.JetpackRunning && (this.m_currentMovementState != MyCharacterMovementEnum.Died)) && (this.m_currentMovementState != MyCharacterMovementEnum.Sitting))
            {
                this.m_currentFallingTime = (this.m_currentCharacterState != HkCharacterStateType.HK_CHARACTER_JUMPING) ? 0f : -1f;
                this.m_isFalling = true;
                this.m_crouchAfterFall = this.WantsCrouch;
                this.WantsCrouch = false;
                this.SetCurrentMovementState(MyCharacterMovementEnum.Falling);
            }
        }

        private void StartRespawn(float respawnTime)
        {
            MyPlayer player = this.TryGetPlayer();
            if (player != null)
            {
                MySessionComponentMissionTriggers.PlayerDied(player);
                if ((MyVisualScriptLogicProvider.PlayerDied != null) && !this.IsBot)
                {
                    MyVisualScriptLogicProvider.PlayerDied(player.Identity.IdentityId);
                }
                if ((MyVisualScriptLogicProvider.NPCDied != null) && this.IsBot)
                {
                    string subtypeName;
                    if (base.DefinitionId == null)
                    {
                        subtypeName = "";
                    }
                    else
                    {
                        subtypeName = base.DefinitionId.Value.SubtypeName;
                    }
                    MyVisualScriptLogicProvider.NPCDied(subtypeName);
                }
                if (!MySessionComponentMissionTriggers.CanRespawn(player.Id))
                {
                    this.m_currentRespawnCounter = -1f;
                }
            }
            if (this.m_currentRespawnCounter != -1f)
            {
                if ((MySession.Static != null) && ReferenceEquals(this, MySession.Static.ControlledEntity))
                {
                    MyGuiScreenTerminal.Hide();
                    this.m_respawnNotification = new MyHudNotification(MyCommonTexts.NotificationRespawn, 0x1388, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 5, MyNotificationLevel.Normal);
                    this.m_respawnNotification.Level = MyNotificationLevel.Important;
                    object[] arguments = new object[] { (int) this.m_currentRespawnCounter };
                    this.m_respawnNotification.SetTextFormatArguments(arguments);
                    MyHud.Notifications.Add(this.m_respawnNotification);
                }
                this.m_currentRespawnCounter = respawnTime;
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        private void StartShooting(Vector3 direction, MyShootActionEnum action)
        {
            this.ShootDirection = direction;
            this.m_isShooting[(int) action] = true;
            this.OnBeginShoot(action);
        }

        private void StartStep(bool forceStartAnimation)
        {
            if ((this.m_currentLadderStep == 0) | forceStartAnimation)
            {
                this.TriggerCharacterAnimationEvent((this.m_currentMovementState == MyCharacterMovementEnum.LadderUp) ? "LadderUp" : "LadderDown", false);
            }
            if (this.m_currentLadderStep == 0)
            {
                this.m_currentLadderStep = this.m_stepsPerAnimation;
            }
        }

        private void Static_CameraAttachedToChanged(IMyCameraController oldController, IMyCameraController newController)
        {
            if (!base.Closed)
            {
                if ((!ReferenceEquals(oldController, newController) && ReferenceEquals(MySession.Static.ControlledEntity, this)) && !ReferenceEquals(newController, this))
                {
                    this.ResetMovement();
                    this.EndShootAll();
                }
                this.UpdateNearFlag();
                if ((this.Render.NearFlag || (MySector.MainCamera == null)) && !ReferenceEquals(oldController, newController))
                {
                    this.ResetHeadRotation();
                }
            }
        }

        private void StopCurrentWeaponShooting()
        {
            if (this.m_currentWeapon != null)
            {
                foreach (MyShootActionEnum enum2 in MyEnum<MyShootActionEnum>.Values)
                {
                    if (this.IsShooting(enum2))
                    {
                        this.m_currentWeapon.EndShoot(enum2);
                    }
                }
            }
        }

        internal void StopFalling()
        {
            if (this.m_currentMovementState != MyCharacterMovementEnum.Died)
            {
                MyCharacterJetpackComponent jetpackComp = this.JetpackComp;
                if (this.m_isFalling && (((jetpackComp == null) || !jetpackComp.TurnedOn) || !jetpackComp.IsPowered))
                {
                    this.SoundComp.PlayFallSound();
                }
                if (this.m_isFalling)
                {
                    this.m_movementsFlagsChanged = true;
                }
                this.m_isFalling = false;
                this.m_isFallingAnimationPlayed = false;
                this.m_currentFallingTime = 0f;
                this.m_currentJumpTime = 0f;
                this.m_canJump = true;
                this.WantsCrouch = this.m_crouchAfterFall;
                this.m_crouchAfterFall = false;
            }
        }

        private void StopFingersAnimation(float blendTime)
        {
            base.PlayerStop("LeftFingers", blendTime);
            base.PlayerStop("RightFingers", blendTime);
        }

        public void StopLowerCharacterAnimation(float blendTime)
        {
            if (!base.UseNewAnimationSystem)
            {
                MyAnimationCommand command = new MyAnimationCommand {
                    AnimationSubtypeName = null,
                    PlaybackCommand = MyPlaybackCommand.Stop,
                    Area = "LowerBody",
                    BlendTime = blendTime,
                    TimeScale = 1f
                };
                this.AddCommand(command, false);
            }
        }

        private void StopShooting(MyShootActionEnum action)
        {
            this.m_isShooting[(int) action] = false;
            this.OnEndShoot(action);
        }

        private void StopUpperAnimation(float blendTime)
        {
            base.PlayerStop("Head", blendTime);
            base.PlayerStop("Spine", blendTime);
            base.PlayerStop("LeftHand", blendTime);
            base.PlayerStop("RightHand", blendTime);
        }

        public void StopUpperCharacterAnimation(float blendTime)
        {
            if (!base.UseNewAnimationSystem)
            {
                MyAnimationCommand command = new MyAnimationCommand {
                    AnimationSubtypeName = null,
                    PlaybackCommand = MyPlaybackCommand.Stop,
                    Area = TopBody,
                    BlendTime = blendTime,
                    TimeScale = 1f
                };
                this.AddCommand(command, false);
            }
        }

        public void SwitchAmmoMagazine()
        {
            this.SwitchAmmoMagazineInternal(true);
        }

        private void SwitchAmmoMagazineInternal(bool sync)
        {
            if (sync)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCharacter>(this, x => new Action(x.OnSwitchAmmoMagazineRequest), targetEndpoint);
            }
            else if (!this.IsDead && (this.CurrentWeapon != null))
            {
                this.CurrentWeapon.GunBase.SwitchAmmoMagazineToNextAvailable();
            }
        }

        private void SwitchAmmoMagazineSuccess()
        {
            this.SwitchAmmoMagazineInternal(false);
        }

        internal void SwitchAnimation(MyCharacterMovementEnum movementState, bool checkState = true)
        {
            if ((!Sandbox.Engine.Platform.Game.IsDedicated || !MyPerGameSettings.DisableAnimationsOnDS) && (!checkState || (this.m_currentMovementState != movementState)))
            {
                if (IsWalkingState(this.m_currentMovementState) != IsWalkingState(movementState))
                {
                    this.m_currentHandItemWalkingBlend = 0f;
                }
                if (movementState > MyCharacterMovementEnum.CrouchWalkingRightFront)
                {
                    if (movementState <= MyCharacterMovementEnum.RunningLeftBack)
                    {
                        if (movementState <= MyCharacterMovementEnum.Running)
                        {
                            if (movementState == MyCharacterMovementEnum.WalkingRightBack)
                            {
                                this.PlayCharacterAnimation("WalkRightBack", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            }
                            else if (movementState == MyCharacterMovementEnum.CrouchWalkingRightBack)
                            {
                                this.PlayCharacterAnimation("CrouchWalkRightBack", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            }
                            else if (movementState == MyCharacterMovementEnum.Running)
                            {
                                this.PlayCharacterAnimation("Run", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            }
                        }
                        else if (movementState <= MyCharacterMovementEnum.RunStrafingLeft)
                        {
                            if (movementState == MyCharacterMovementEnum.Backrunning)
                            {
                                this.PlayCharacterAnimation("RunBack", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            }
                            else if (movementState == MyCharacterMovementEnum.RunStrafingLeft)
                            {
                                this.PlayCharacterAnimation("RunLeft", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            }
                        }
                        else if (movementState == MyCharacterMovementEnum.RunningLeftFront)
                        {
                            this.PlayCharacterAnimation("RunLeftFront", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                        }
                        else if (movementState == MyCharacterMovementEnum.RunningLeftBack)
                        {
                            this.PlayCharacterAnimation("RunLeftBack", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                        }
                    }
                    else if (movementState <= MyCharacterMovementEnum.Sprinting)
                    {
                        if (movementState <= MyCharacterMovementEnum.RunningRightFront)
                        {
                            if (movementState == MyCharacterMovementEnum.RunStrafingRight)
                            {
                                this.PlayCharacterAnimation("RunRight", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            }
                            else if (movementState == MyCharacterMovementEnum.RunningRightFront)
                            {
                                this.PlayCharacterAnimation("RunRightFront", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            }
                        }
                        else if (movementState == MyCharacterMovementEnum.RunningRightBack)
                        {
                            this.PlayCharacterAnimation("RunRightBack", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                        }
                        else if (movementState == MyCharacterMovementEnum.Sprinting)
                        {
                            this.PlayCharacterAnimation("Sprint", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.1f), 1f, false, null, false);
                        }
                    }
                    else if (movementState <= MyCharacterMovementEnum.CrouchRotatingLeft)
                    {
                        if (movementState == MyCharacterMovementEnum.RotatingLeft)
                        {
                            this.PlayCharacterAnimation("StandLeftTurn", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                        }
                        else if (movementState == MyCharacterMovementEnum.CrouchRotatingLeft)
                        {
                            this.PlayCharacterAnimation("CrouchLeftTurn", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                        }
                    }
                    else if (movementState == MyCharacterMovementEnum.RotatingRight)
                    {
                        this.PlayCharacterAnimation("StandRightTurn", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                    }
                    else if (movementState == MyCharacterMovementEnum.CrouchRotatingRight)
                    {
                        this.PlayCharacterAnimation("CrouchRightTurn", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                    }
                }
                else if (movementState > MyCharacterMovementEnum.CrouchStrafingLeft)
                {
                    if (movementState <= MyCharacterMovementEnum.CrouchWalkingLeftBack)
                    {
                        if (movementState <= MyCharacterMovementEnum.CrouchWalkingLeftFront)
                        {
                            if (movementState == MyCharacterMovementEnum.WalkingLeftFront)
                            {
                                this.PlayCharacterAnimation("WalkLeftFront", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            }
                            else if (movementState == MyCharacterMovementEnum.CrouchWalkingLeftFront)
                            {
                                this.PlayCharacterAnimation("CrouchWalkLeftFront", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            }
                        }
                        else if (movementState == MyCharacterMovementEnum.WalkingLeftBack)
                        {
                            this.PlayCharacterAnimation("WalkLeftBack", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                        }
                        else if (movementState == MyCharacterMovementEnum.CrouchWalkingLeftBack)
                        {
                            this.PlayCharacterAnimation("CrouchWalkLeftBack", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                        }
                    }
                    else if (movementState <= MyCharacterMovementEnum.CrouchStrafingRight)
                    {
                        if (movementState == MyCharacterMovementEnum.WalkStrafingRight)
                        {
                            this.PlayCharacterAnimation("StrafeRight", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                        }
                        else if (movementState == MyCharacterMovementEnum.CrouchStrafingRight)
                        {
                            this.PlayCharacterAnimation("CrouchStrafeRight", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                        }
                    }
                    else if (movementState == MyCharacterMovementEnum.WalkingRightFront)
                    {
                        this.PlayCharacterAnimation("WalkRightFront", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                    }
                    else if (movementState == MyCharacterMovementEnum.CrouchWalkingRightFront)
                    {
                        this.PlayCharacterAnimation("CrouchWalkRightFront", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                    }
                }
                else if (movementState > MyCharacterMovementEnum.CrouchWalking)
                {
                    if (movementState <= MyCharacterMovementEnum.CrouchBackWalking)
                    {
                        if (movementState == MyCharacterMovementEnum.BackWalking)
                        {
                            this.PlayCharacterAnimation("WalkBack", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                        }
                        else if (movementState == MyCharacterMovementEnum.CrouchBackWalking)
                        {
                            this.PlayCharacterAnimation("CrouchWalkBack", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                        }
                    }
                    else if (movementState == MyCharacterMovementEnum.WalkStrafingLeft)
                    {
                        this.PlayCharacterAnimation("StrafeLeft", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                    }
                    else if (movementState == MyCharacterMovementEnum.CrouchStrafingLeft)
                    {
                        this.PlayCharacterAnimation("CrouchStrafeLeft", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                    }
                }
                else
                {
                    switch (movementState)
                    {
                        case MyCharacterMovementEnum.Standing:
                            this.PlayCharacterAnimation("Idle", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            return;

                        case MyCharacterMovementEnum.Sitting:
                            break;

                        case MyCharacterMovementEnum.Crouching:
                            this.PlayCharacterAnimation("CrouchIdle", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.1f), 1f, false, null, false);
                            return;

                        case MyCharacterMovementEnum.Flying:
                            this.PlayCharacterAnimation("Jetpack", this.AdjustSafeAnimationEnd(MyBlendOption.Immediate), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0f), 1f, false, null, false);
                            return;

                        case MyCharacterMovementEnum.Falling:
                            this.PlayCharacterAnimation("FreeFall", this.AdjustSafeAnimationEnd(MyBlendOption.Immediate), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            return;

                        case MyCharacterMovementEnum.Jump:
                            this.PlayCharacterAnimation("Jump", this.AdjustSafeAnimationEnd(MyBlendOption.Immediate), MyFrameOption.Default, this.AdjustSafeAnimationBlend(0f), 1.3f, false, null, false);
                            return;

                        case MyCharacterMovementEnum.Died:
                            this.PlayCharacterAnimation("Died", this.AdjustSafeAnimationEnd(MyBlendOption.Immediate), MyFrameOption.Default, this.AdjustSafeAnimationBlend(0.5f), 1f, false, null, false);
                            break;

                        default:
                            if (movementState == MyCharacterMovementEnum.Walking)
                            {
                                this.PlayCharacterAnimation("Walk", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.1f), 1f, false, null, false);
                                return;
                            }
                            if (movementState == MyCharacterMovementEnum.CrouchWalking)
                            {
                                this.PlayCharacterAnimation("CrouchWalk", this.AdjustSafeAnimationEnd(MyBlendOption.WaitForPreviousEnd), MyFrameOption.Loop, this.AdjustSafeAnimationBlend(0.2f), 1f, false, null, false);
                            }
                            return;
                    }
                }
            }
        }

        public void SwitchBroadcasting()
        {
            if (this.m_currentMovementState != MyCharacterMovementEnum.Died)
            {
                this.EnableBroadcasting(!this.RadioBroadcaster.WantsToBeEnabled);
            }
        }

        private void SwitchCameraIronSightChanges()
        {
            this.m_wasInThirdPersonBeforeIronSight = false;
            if (this.m_zoomMode == MyZoomModeEnum.IronSight)
            {
                if (this.m_isInFirstPersonView)
                {
                    MyHud.Crosshair.HideDefaultSprite();
                }
                else
                {
                    MyHud.Crosshair.ResetToDefault(true);
                }
            }
        }

        [Event(null, 0x2802), Reliable, Client]
        public void SwitchJetpack()
        {
            if (this.JetpackComp != null)
            {
                this.JetpackComp.SwitchThrusts();
            }
        }

        public void SwitchLandingGears()
        {
        }

        public void SwitchLights()
        {
            if (this.m_currentMovementState != MyCharacterMovementEnum.Died)
            {
                this.EnableLights(!this.LightEnabled);
                this.RecalculatePowerRequirement(false);
            }
        }

        public void SwitchReactors()
        {
        }

        public void SwitchToWeapon(MyToolbarItemWeapon weapon)
        {
            uint? inventoryItemId = null;
            this.SwitchToWeapon(weapon, inventoryItemId, true);
        }

        public void SwitchToWeapon(MyDefinitionId weaponDefinition)
        {
            this.SwitchToWeapon(new MyDefinitionId?(weaponDefinition), true);
        }

        public void SwitchToWeapon(MyDefinitionId? weaponDefinition, bool sync = true)
        {
            if ((weaponDefinition == null) || (this.m_rightHandItemBone != -1))
            {
                if (!this.WeaponTakesBuilderFromInventory(weaponDefinition))
                {
                    uint? inventoryItemId = null;
                    this.SwitchToWeaponInternal(weaponDefinition, sync, inventoryItemId, 0L);
                }
                else
                {
                    MyPhysicalInventoryItem? nullable = this.FindWeaponItemByDefinition(weaponDefinition.Value);
                    if (nullable != null)
                    {
                        if (nullable.Value.Content != null)
                        {
                            this.SwitchToWeaponInternal(weaponDefinition, sync, new uint?(nullable.Value.ItemId), 0L);
                        }
                        else
                        {
                            MySandboxGame.Log.WriteLine("item.Value.Content was null in MyCharacter.SwitchToWeapon");
                            MySandboxGame.Log.WriteLine("item.Value = " + nullable.Value);
                            MySandboxGame.Log.WriteLine("weaponDefinition.Value = " + weaponDefinition);
                        }
                    }
                }
            }
        }

        public void SwitchToWeapon(MyToolbarItemWeapon weapon, uint? inventoryItemId, bool sync = true)
        {
            MyDefinitionId? weaponDefinition = null;
            if (weapon != null)
            {
                weaponDefinition = new MyDefinitionId?(weapon.Definition.Id);
            }
            if ((weaponDefinition == null) || (this.m_rightHandItemBone != -1))
            {
                if (!this.WeaponTakesBuilderFromInventory(weaponDefinition))
                {
                    uint? nullable3 = null;
                    this.SwitchToWeaponInternal(weaponDefinition, sync, nullable3, 0L);
                }
                else
                {
                    MyPhysicalInventoryItem? nullable2 = null;
                    nullable2 = (inventoryItemId == null) ? this.FindWeaponItemByDefinition(weaponDefinition.Value) : this.GetInventory(0).GetItemByID(inventoryItemId.Value);
                    if (nullable2 != null)
                    {
                        if (nullable2.Value.Content != null)
                        {
                            this.SwitchToWeaponInternal(weaponDefinition, sync, new uint?(nullable2.Value.ItemId), 0L);
                        }
                        else
                        {
                            MySandboxGame.Log.WriteLine("item.Value.Content was null in MyCharacter.SwitchToWeapon");
                            MySandboxGame.Log.WriteLine("item.Value = " + nullable2.Value);
                            MySandboxGame.Log.WriteLine("weaponDefinition.Value = " + weaponDefinition);
                        }
                    }
                }
            }
        }

        private void SwitchToWeaponInternal(MyDefinitionId? weaponDefinition, bool updateSync, uint? inventoryItemId, long weaponEntityId)
        {
            if (MySessionComponentReplay.Static.IsEntityBeingRecorded(base.EntityId))
            {
                PerFrameData data2 = new PerFrameData();
                SwitchWeaponData data3 = new SwitchWeaponData {
                    WeaponDefinition = weaponDefinition,
                    InventoryItemId = inventoryItemId,
                    WeaponEntityId = weaponEntityId
                };
                data2.SwitchWeaponData = new SwitchWeaponData?(data3);
                PerFrameData data = data2;
                MySessionComponentReplay.Static.ProvideEntityRecordData(base.EntityId, data);
            }
            if (updateSync)
            {
                this.UnequipWeapon();
                this.RequestSwitchToWeapon(weaponDefinition, inventoryItemId);
            }
            else
            {
                this.UnequipWeapon();
                this.StopCurrentWeaponShooting();
                if ((weaponDefinition != null) && (weaponDefinition.Value.TypeId != MyObjectBuilderType.Invalid))
                {
                    IMyHandheldGunObject<MyDeviceBase> newWeapon = CreateGun(this.GetObjectBuilderForWeapon(weaponDefinition, ref inventoryItemId, weaponEntityId), inventoryItemId);
                    this.EquipWeapon(newWeapon, false);
                    this.UpdateShadowIgnoredObjects();
                }
            }
        }

        [Event(null, 0x2766), Reliable, Server(ValidationType.Controlled)]
        private void SwitchToWeaponMessage(SerializableDefinitionId? weapon, uint? inventoryItemId)
        {
            MyDefinitionId? nullable2;
            MyDefinitionId? nullable1;
            SerializableDefinitionId? nullable = weapon;
            if (nullable != null)
            {
                nullable1 = new MyDefinitionId?(nullable.GetValueOrDefault());
            }
            else
            {
                nullable2 = null;
                nullable1 = nullable2;
            }
            if (this.CanSwitchToWeapon(nullable1))
            {
                EndpointId id;
                if (inventoryItemId == null)
                {
                    if (weapon != null)
                    {
                        MyDefinitionId? nullable6;
                        long weaponEntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                        nullable = weapon;
                        if (nullable != null)
                        {
                            nullable6 = new MyDefinitionId?(nullable.GetValueOrDefault());
                        }
                        else
                        {
                            nullable2 = null;
                            nullable6 = nullable2;
                        }
                        this.SwitchToWeaponSuccess(nullable6, inventoryItemId, weaponEntityId);
                        id = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyCharacter, SerializableDefinitionId?, uint?, long>(this, x => new Action<SerializableDefinitionId?, uint?, long>(x.OnSwitchToWeaponSuccess), weapon, inventoryItemId, weaponEntityId, id);
                    }
                    else
                    {
                        id = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyCharacter>(this, x => new Action(x.UnequipWeapon), id);
                    }
                }
                else
                {
                    MyInventory inventory = this.GetInventory(0);
                    if (inventory != null)
                    {
                        MyPhysicalInventoryItem? itemByID = inventory.GetItemByID(inventoryItemId.Value);
                        if (itemByID != null)
                        {
                            MyDefinitionId? nullable4 = MyDefinitionManager.Static.ItemIdFromWeaponId(weapon.Value);
                            if ((nullable4 != null) && (itemByID.Value.Content.GetObjectId() == nullable4.Value))
                            {
                                MyDefinitionId? nullable5;
                                long weaponEntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                                nullable = weapon;
                                if (nullable != null)
                                {
                                    nullable5 = new MyDefinitionId?(nullable.GetValueOrDefault());
                                }
                                else
                                {
                                    nullable2 = null;
                                    nullable5 = nullable2;
                                }
                                this.SwitchToWeaponSuccess(nullable5, inventoryItemId, weaponEntityId);
                                id = new EndpointId();
                                MyMultiplayer.RaiseEvent<MyCharacter, SerializableDefinitionId?, uint?, long>(this, x => new Action<SerializableDefinitionId?, uint?, long>(x.OnSwitchToWeaponSuccess), weapon, inventoryItemId, weaponEntityId, id);
                            }
                        }
                    }
                }
            }
        }

        private void SwitchToWeaponSuccess(MyDefinitionId? weapon, uint? inventoryItemId, long weaponEntityId)
        {
            if (!base.Closed)
            {
                if (!this.IsDead)
                {
                    this.SwitchToWeaponInternal(weapon, false, inventoryItemId, weaponEntityId);
                }
                if (this.OnWeaponChanged != null)
                {
                    this.OnWeaponChanged(this, null);
                }
            }
        }

        public void SwitchWalk()
        {
            this.WantsWalk = !this.WantsWalk;
        }

        public unsafe void SyncHeadToolTransform(ref MatrixD headMatrix)
        {
            if (this.ControllerInfo.IsLocallyControlled())
            {
                MyTransform transform = new MyTransform((Matrix) (headMatrix * base.PositionComp.WorldMatrixInvScaled));
                MyTransform* transformPtr1 = (MyTransform*) ref transform;
                transformPtr1->Rotation = Quaternion.Normalize(transform.Rotation);
            }
        }

        private void Toolbar_ItemChanged(MyToolbar toolbar, MyToolbar.IndexArgs index)
        {
            MyToolbarItem itemAtIndex = toolbar.GetItemAtIndex(index.ItemIndex);
            if (itemAtIndex == null)
            {
                if (MySandboxGame.IsGameReady)
                {
                    MyToolBarCollection.RequestClearSlot(MySession.Static.LocalHumanPlayer.Id, index.ItemIndex);
                }
            }
            else
            {
                MyToolbarItemDefinition definition = itemAtIndex as MyToolbarItemDefinition;
                if (definition != null)
                {
                    MyDefinitionId defId = definition.Definition.Id;
                    if (defId.TypeId != typeof(MyObjectBuilder_PhysicalGunObject))
                    {
                        MyToolBarCollection.RequestChangeSlotItem(MySession.Static.LocalHumanPlayer.Id, index.ItemIndex, defId);
                    }
                    else
                    {
                        MyToolBarCollection.RequestChangeSlotItem(MySession.Static.LocalHumanPlayer.Id, index.ItemIndex, itemAtIndex.GetObjectBuilder());
                    }
                }
            }
        }

        private void ToolHeadTransformChanged()
        {
            MyEngineerToolBase currentWeapon = this.m_currentWeapon as MyEngineerToolBase;
            if ((currentWeapon != null) && !this.ControllerInfo.IsLocallyControlled())
            {
                currentWeapon.UpdateSensorPosition();
            }
        }

        public override string ToString() => 
            this.m_characterModel;

        [Event(null, 0x298), Broadcast, Reliable]
        public void TriggerAnimationEvent(string eventName)
        {
            base.AnimationController.TriggerAction(MyStringId.GetOrCompute(eventName));
        }

        public void TriggerCharacterAnimationEvent(string eventName, bool sync)
        {
            if (base.UseNewAnimationSystem && !string.IsNullOrEmpty(eventName))
            {
                if (MySessionComponentReplay.Static.IsEntityBeingRecorded(base.EntityId))
                {
                    PerFrameData data2 = new PerFrameData();
                    AnimationData data3 = new AnimationData {
                        Animation = eventName
                    };
                    data2.AnimationData = new AnimationData?(data3);
                    PerFrameData data = data2;
                    MySessionComponentReplay.Static.ProvideEntityRecordData(base.EntityId, data);
                }
                if (sync)
                {
                    this.SendAnimationEvent(eventName);
                }
                else
                {
                    base.AnimationController.TriggerAction(MyStringId.GetOrCompute(eventName));
                }
            }
        }

        private MyPlayer TryGetPlayer()
        {
            MyEntityController controller = this.ControllerInfo.Controller;
            return controller?.Player;
        }

        [Event(null, 0x1b21), Reliable, Server(ValidationType.Controlled), BroadcastExcept]
        public void UnequipWeapon()
        {
            if ((this.m_leftHandItem != null) && (this.m_leftHandItem is IMyHandheldGunObject<MyDeviceBase>))
            {
                (this.m_leftHandItem as IMyHandheldGunObject<MyDeviceBase>).OnControlReleased();
                this.m_leftHandItem.Close();
                this.m_leftHandItem = null;
                bool sync = ReferenceEquals(this, MySession.Static.LocalCharacter);
                this.TriggerCharacterAnimationEvent("unequip_left_tool", sync);
            }
            if (this.m_currentWeapon != null)
            {
                if (this.ControllerInfo.IsLocallyControlled())
                {
                    MyHighlightSystem highlightSystem = MySession.Static.GetComponent<MyHighlightSystem>();
                    if (highlightSystem != null)
                    {
                        VRage.Game.Entity.MyEntity entity = (VRage.Game.Entity.MyEntity) this.m_currentWeapon;
                        entity.Render.RenderObjectIDs.ForEach<uint>(delegate (uint id) {
                            if (id != uint.MaxValue)
                            {
                                highlightSystem.RemoveHighlightOverlappingModel(id);
                            }
                        });
                        if (entity.Subparts != null)
                        {
                            foreach (KeyValuePair<string, MyEntitySubpart> pair in entity.Subparts)
                            {
                                Action<uint> <>9__1;
                                Action<uint> action = <>9__1;
                                if (<>9__1 == null)
                                {
                                    Action<uint> local1 = <>9__1;
                                    action = <>9__1 = delegate (uint id) {
                                        if (id != uint.MaxValue)
                                        {
                                            highlightSystem.RemoveHighlightOverlappingModel(id);
                                        }
                                    };
                                }
                                pair.Value.Render.RenderObjectIDs.ForEach<uint>(action);
                            }
                        }
                    }
                }
                if ((ReferenceEquals(MySession.Static.LocalCharacter, this) && !MyInput.Static.IsGameControlPressed(MyControlsSpace.PRIMARY_TOOL_ACTION)) && !MyInput.Static.IsGameControlPressed(MyControlsSpace.SECONDARY_TOOL_ACTION))
                {
                    this.EndShootAll();
                }
                else if (Sync.IsServer)
                {
                    foreach (MyShootActionEnum enum2 in MyEnum<MyShootActionEnum>.Values)
                    {
                        if (this.IsShooting(enum2))
                        {
                            this.m_currentWeapon.EndShoot(enum2);
                        }
                    }
                }
                if ((base.UseNewAnimationSystem && (this.m_handItemDefinition != null)) && !string.IsNullOrEmpty(this.m_handItemDefinition.Id.SubtypeName))
                {
                    base.AnimationController.Variables.SetValue(MyStringId.GetOrCompute(this.m_handItemDefinition.Id.TypeId.ToString().ToLower()), 0f);
                }
                this.SaveAmmoToWeapon();
                this.m_currentWeapon.OnControlReleased();
                if (this.m_zoomMode == MyZoomModeEnum.IronSight)
                {
                    bool isInFirstPersonView = this.IsInFirstPersonView;
                    this.EnableIronsight(false, true, ReferenceEquals(MySession.Static.CameraController, this), true);
                    this.IsInFirstPersonView = isInFirstPersonView;
                }
                VRage.Game.Entity.MyEntity currentWeapon = this.m_currentWeapon as VRage.Game.Entity.MyEntity;
                MyResourceSinkComponent sink = currentWeapon.Components.Get<MyResourceSinkComponent>();
                if (sink != null)
                {
                    this.SuitRechargeDistributor.RemoveSink(sink, true, false);
                }
                VRage.Game.Entity.MyEntity local3 = currentWeapon;
                local3.SetFadeOut(false);
                local3.OnClose -= new Action<VRage.Game.Entity.MyEntity>(this.gunEntity_OnClose);
                Sandbox.Game.Entities.MyEntities.Remove(local3);
                local3.Close();
                this.m_currentWeapon = null;
                if (this.ControllerInfo.IsLocallyHumanControlled() && (MySector.MainCamera != null))
                {
                    MySector.MainCamera.Zoom.ResetZoom();
                }
                if (!base.UseNewAnimationSystem)
                {
                    this.StopUpperAnimation(0.2f);
                    this.SwitchAnimation(this.m_currentMovementState, false);
                }
                else
                {
                    bool sync = ReferenceEquals(this, MySession.Static.LocalCharacter);
                    this.TriggerCharacterAnimationEvent("unequip_left_tool", sync);
                    this.TriggerCharacterAnimationEvent("unequip_right_tool", sync);
                }
                base.AnimationController.Variables.SetValue(MyAnimationVariableStorageHints.StrIdShooting, 0f);
                base.AnimationController.Update();
                MyAnalyticsHelper.ReportActivityEnd(this, "item_equip");
            }
            if (this.m_currentShotTime <= 0f)
            {
                this.StopUpperAnimation(0f);
                this.StopFingersAnimation(0f);
            }
            this.m_currentWeapon = null;
            this.StopFingersAnimation(0f);
        }

        public void Up()
        {
            if (!this.WantsFlyDown)
            {
                this.WantsFlyUp = true;
            }
            else
            {
                this.WantsFlyUp = false;
                this.WantsFlyDown = false;
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            LIGHT_PARAMETERS_CHANGED = false;
            if (((MyPetaInputComponent.MovementDistanceCounter == 0) && (MyPetaInputComponent.MovementDistance == 0f)) && (this.Physics.LinearVelocity.Length() < 0.001f))
            {
                MyPetaInputComponent.MovementDistance = (float) Vector3D.Distance(MyPetaInputComponent.MovementDistanceStart, base.PositionComp.GetPosition());
                MyPetaInputComponent.MovementDistanceCounter = -1;
            }
            bool flag = true;
            if (((this.ControllerInfo.Controller != null) && (this.CharacterCanDie && (this.ControllerInfo.Controller.Player.DisplayName != null))) && (this.ControllerInfo.Controller.Player.DisplayName == "mikrogen"))
            {
                AdminSettingsEnum enum2;
                MyPlayer.PlayerId clientIdentity = this.GetClientIdentity();
                if (((clientIdentity.SerialId == 0) && MySession.Static.RemoteAdminSettings.TryGetValue(clientIdentity.SteamId, out enum2)) && enum2.HasFlag(AdminSettingsEnum.Invulnerable))
                {
                    flag = false;
                }
            }
            if ((this.m_currentMovementState == MyCharacterMovementEnum.Sitting) || (this.m_currentMovementState == MyCharacterMovementEnum.Died))
            {
                flag = false;
            }
            if ((flag && (this.Physics != null)) && (this.Physics.CharacterProxy != null))
            {
                this.RayCastGround();
                if (((this.m_standingOnGrid != null) || (this.m_standingOnVoxel != null)) || this.IsFalling)
                {
                    this.Physics.CharacterProxy.SetSupportDistance(0.1f);
                    this.Physics.CharacterProxy.MaxSlope = MathHelper.ToRadians(this.Definition.MaxSlope);
                }
                else
                {
                    this.Physics.CharacterProxy.SetSupportDistance(0.001f);
                    this.Physics.CharacterProxy.MaxSlope = MathHelper.ToRadians((float) 20f);
                }
            }
            this.UpdateLadder();
            if (!this.IsDead && (this.StatComp != null))
            {
                this.StatComp.Update();
            }
            this.UpdateDying();
            if ((!Sandbox.Engine.Platform.Game.IsDedicated || !MyPerGameSettings.DisableAnimationsOnDS) && !this.IsDead)
            {
                this.UpdateShake();
            }
            if (this.IsDead || ((!Sync.IsServer && !this.IsClientPredicted) && ReferenceEquals(MySession.Static.TopMostControlledEntity, this)))
            {
                this.UpdatePhysicalMovement();
            }
            if (!this.IsDead)
            {
                this.UpdateFallAndSpine();
            }
            if (this.JetpackRunning)
            {
                this.JetpackComp.ClearMovement();
            }
            if (Sandbox.Engine.Platform.Game.IsDedicated && MyPerGameSettings.DisableAnimationsOnDS)
            {
                if ((this.m_currentWeapon != null) && (this.WeaponPosition != null))
                {
                    this.WeaponPosition.Update(true);
                }
            }
            else
            {
                MyCharacterRagdollComponent component = base.Components.Get<MyCharacterRagdollComponent>();
                if (component != null)
                {
                    component.Distance = this.m_cameraDistance;
                }
                this.Render.UpdateLightPosition();
                this.UpdateBobQueue();
            }
            this.UpdateCharacterStateChange();
            this.UpdateRespawnAndLooting();
            this.UpdateShooting();
            foreach (MyCharacterComponent component2 in base.Components)
            {
                if (component2 == null)
                {
                    continue;
                }
                if (component2.NeedsUpdateAfterSimulation)
                {
                    component2.UpdateAfterSimulation();
                }
            }
            this.m_characterBoneCapsulesReady = false;
            if (this.Physics != null)
            {
                this.m_previousLinearVelocity = this.Physics.LinearVelocity;
            }
            this.m_previousPosition = base.WorldMatrix.Translation;
            if ((this.Physics != null) && (this.Physics.CharacterProxy == null))
            {
                this.Render.UpdateWalkParticles();
            }
            this.SoundComp.FindAndPlayStateSound();
            this.SoundComp.UpdateWindSounds();
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            foreach (MyCharacterComponent component in base.Components)
            {
                if (component == null)
                {
                    continue;
                }
                if (component.NeedsUpdateAfterSimulation10)
                {
                    component.UpdateAfterSimulation10();
                }
            }
            this.UpdateCameraDistance();
            if (Sync.IsServer)
            {
                this.UpdateBootsStateAndEmmisivity();
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            this.UpdateAssetModifiers();
            this.SoundComp.UpdateAfterSimulation100();
            this.UpdateOutsideTemperature();
        }

        public override void UpdateAnimation(float distance)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated || !MyPerGameSettings.DisableAnimationsOnDS)
            {
                if (distance >= MyFakes.ANIMATION_UPDATE_DISTANCE)
                {
                    this.WeaponPosition.Update(true);
                }
                else
                {
                    MyAnimationPlayerBlendPair pair;
                    if (base.UseNewAnimationSystem)
                    {
                        this.UpdateAnimationNewSystem();
                    }
                    base.UpdateAnimation(distance);
                    if ((base.TryGetAnimationPlayer("LeftHand", out pair) && ((pair.GetState() == MyAnimationPlayerBlendPair.AnimationBlendState.Stopped) && (this.m_leftHandItem != null))) && !base.UseNewAnimationSystem)
                    {
                        this.m_leftHandItem.Close();
                        this.m_leftHandItem = null;
                    }
                    this.Render.UpdateThrustMatrices(base.BoneAbsoluteTransforms);
                    if (this.m_resetWeaponAnimationState)
                    {
                        this.m_resetWeaponAnimationState = false;
                    }
                }
            }
        }

        private void UpdateAnimationNewSystem()
        {
            float single1;
            float single2;
            MyAnimationVariableStorage variables = base.AnimationController.Variables;
            if (this.Physics != null)
            {
                Vector3 v = this.Physics.LinearVelocity * Vector3.TransformNormal(this.m_currentMovementDirection, base.WorldMatrix);
                MyCharacterProxy characterProxy = this.Physics.CharacterProxy;
                if (Sync.IsServer || MyFakes.MULTIPLAYER_CLIENT_SIMULATE_CONTROLLED_CHARACTER)
                {
                    if (characterProxy == null)
                    {
                        v = this.Physics.LinearVelocityLocal;
                    }
                    else
                    {
                        Vector3 linearVelocityLocal = this.Physics.LinearVelocityLocal;
                        Vector3 groundVelocity = characterProxy.GroundVelocity;
                        Vector3 interpolatedVelocity = characterProxy.CharacterRigidBody.InterpolatedVelocity;
                        Vector3 vector2 = ((interpolatedVelocity - groundVelocity).LengthSquared() >= (linearVelocityLocal - groundVelocity).LengthSquared()) ? linearVelocityLocal : interpolatedVelocity;
                        v = vector2 - groundVelocity;
                        if (this.GetCurrentMovementState() == MyCharacterMovementEnum.Standing)
                        {
                            float num7 = characterProxy.Up.Dot(v);
                            if (num7 < 0f)
                            {
                                v -= characterProxy.Up * num7;
                            }
                        }
                    }
                }
                Vector3 vector4 = this.FilterLocalSpeed(v);
                MatrixD worldMatrix = base.PositionComp.WorldMatrix;
                float newValue = vector4.Dot((Vector3) worldMatrix.Right);
                float num2 = vector4.Dot((Vector3) worldMatrix.Up);
                float num3 = vector4.Dot((Vector3) worldMatrix.Forward);
                float num4 = (float) Math.Sqrt((double) ((newValue * newValue) + (num3 * num3)));
                variables.SetValue(MyAnimationVariableStorageHints.StrIdSpeed, num4);
                variables.SetValue(MyAnimationVariableStorageHints.StrIdSpeedX, newValue);
                variables.SetValue(MyAnimationVariableStorageHints.StrIdSpeedY, num2);
                variables.SetValue(MyAnimationVariableStorageHints.StrIdSpeedZ, num3);
                float num5 = (vector4.LengthSquared() > 0.0025f) ? (((float) ((-Math.Atan2((double) num3, (double) newValue) * 180.0) / 3.1415926535897931)) + 90f) : 0f;
                while (true)
                {
                    if (num5 >= 0f)
                    {
                        variables.SetValue(MyAnimationVariableStorageHints.StrIdSpeedAngle, num5);
                        if (characterProxy != null)
                        {
                            Vector3 zero = Vector3.Zero;
                            if (Sync.IsServer)
                            {
                                zero = characterProxy.GroundAngularVelocity;
                            }
                            else
                            {
                                using (MyUtils.ReuseCollection<VRage.Game.Entity.MyEntity>(ref m_supportingEntities))
                                {
                                    int num8 = 0;
                                    characterProxy.GetSupportingEntities(m_supportingEntities);
                                    using (List<VRage.Game.Entity.MyEntity>.Enumerator enumerator = m_supportingEntities.GetEnumerator())
                                    {
                                        while (enumerator.MoveNext())
                                        {
                                            MyPhysicsComponentBase physics = enumerator.Current.Physics;
                                            if (physics != null)
                                            {
                                                num8++;
                                                zero += physics.AngularVelocityLocal;
                                            }
                                        }
                                    }
                                    if (num8 != 0)
                                    {
                                        zero /= (float) num8;
                                    }
                                }
                            }
                            this.m_lastRotation *= Quaternion.CreateFromAxisAngle(zero, 0.01666667f);
                        }
                        Quaternion rotation = this.GetRotation();
                        float num6 = (((Quaternion.Inverse(rotation) * this.m_lastRotation).Y / 0.0013f) * 180f) / 3.141593f;
                        variables.SetValue(MyAnimationVariableStorageHints.StrIdTurningSpeed, num6);
                        this.m_lastRotation = rotation;
                        if (this.OxygenComponent != null)
                        {
                            variables.SetValue(MyAnimationVariableStorageHints.StrIdHelmetOpen, this.OxygenComponent.HelmetEnabled ? 0f : 1f);
                        }
                        if ((base.Parent is MyCockpit) || this.IsOnLadder)
                        {
                            variables.SetValue(MyAnimationVariableStorageHints.StrIdLean, 0f);
                        }
                        else
                        {
                            variables.SetValue(MyAnimationVariableStorageHints.StrIdLean, (float) this.m_animLeaning);
                        }
                        break;
                    }
                    num5 += 360f;
                }
            }
            bool flag = ReferenceEquals(MySession.Static.CameraController, this);
            bool flag2 = (this.m_isInFirstPerson || this.ForceFirstPersonCamera) & flag;
            if (this.JetpackComp != null)
            {
                base.AnimationController.Variables.SetValue(MyAnimationVariableStorageHints.StrIdFlying, this.JetpackComp.Running ? 1f : 0f);
            }
            MyCharacterMovementEnum currentMovementState = this.GetCurrentMovementState();
            variables.SetValue(MyAnimationVariableStorageHints.StrIdFlying, (currentMovementState == MyCharacterMovementEnum.Flying) ? 1f : 0f);
            if (this.IsFalling || (currentMovementState == MyCharacterMovementEnum.Falling))
            {
                single1 = 1f;
            }
            else
            {
                single1 = 0f;
            }
            MyAnimationVariableStorageHints.StrIdFalling.SetValue((MyStringId) variables, single1);
            if (!this.WantsCrouch || this.WantsSprint)
            {
                single2 = 0f;
            }
            else
            {
                single2 = 1f;
            }
            variables.SetValue(MyAnimationVariableStorageHints.StrIdCrouch, single2);
            variables.SetValue(MyAnimationVariableStorageHints.StrIdSitting, (currentMovementState == MyCharacterMovementEnum.Sitting) ? 1f : 0f);
            variables.SetValue(MyAnimationVariableStorageHints.StrIdJumping, (currentMovementState == MyCharacterMovementEnum.Jump) ? 1f : 0f);
            variables.SetValue(MyAnimationVariableStorageHints.StrIdFirstPerson, flag2 ? 1f : 0f);
            variables.SetValue(MyAnimationVariableStorageHints.StrIdForcedFirstPerson, this.ForceFirstPersonCamera ? 1f : 0f);
            variables.SetValue(MyAnimationVariableStorageHints.StrIdHoldingTool, (this.m_currentWeapon != null) ? 1f : 0f);
            if (this.WeaponPosition == null)
            {
                variables.SetValue(MyAnimationVariableStorageHints.StrIdShooting, 0f);
                variables.SetValue(MyAnimationVariableStorageHints.StrIdIronsight, 0f);
            }
            else
            {
                float single3;
                if (((this.m_currentWeapon == null) || !this.WeaponPosition.IsShooting) || this.WeaponPosition.ShouldSupressShootAnimation)
                {
                    single3 = 0f;
                }
                else
                {
                    single3 = 1f;
                }
                variables.SetValue(MyAnimationVariableStorageHints.StrIdShooting, single3);
                variables.SetValue(MyAnimationVariableStorageHints.StrIdIronsight, this.WeaponPosition.IsInIronSight ? 1f : 0f);
            }
            variables.SetValue(MyAnimationVariableStorageHints.StrIdLadder, this.IsOnLadder ? 1f : 0f);
        }

        private void UpdateAssetModifiers()
        {
            if ((!this.m_assetModifiersLoaded && !Sandbox.Engine.Platform.Game.IsDedicated) && (MySession.Static.LocalHumanPlayer != null))
            {
                long playerIdentityId = this.GetPlayerIdentityId();
                if (playerIdentityId == MySession.Static.LocalHumanPlayer.Identity.IdentityId)
                {
                    if (!this.IsDead && !this.IsBot)
                    {
                        MyLocalCache.LoadInventoryConfig(this, false);
                        this.m_assetModifiersLoaded = true;
                    }
                }
                else if (playerIdentityId != -1L)
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long, long>(x => new Action<long, long>(MyCharacter.RefreshAssetModifiers), playerIdentityId, base.EntityId, targetEndpoint, position);
                    this.m_assetModifiersLoaded = true;
                }
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (MySession.Static != null)
            {
                PerFrameData data;
                base.AnimationController.UpdateTransformations();
                this.UpdatePredictionFlag();
                this.m_previousMovementFlags = this.m_movementFlags;
                this.m_previousNetworkMovementState = this.GetCurrentMovementState();
                this.UpdateZeroMovement();
                this.m_moveAndRotateCalled = false;
                base.m_actualUpdateFrame += (ulong) 1L;
                this.m_isInFirstPerson = ReferenceEquals(MySession.Static.CameraController, this) && this.IsInFirstPersonView;
                bool flag = this.ControllerInfo.IsLocallyControlled() && ReferenceEquals(MySession.Static.CameraController, this);
                bool flag2 = (this.m_isInFirstPerson || this.ForceFirstPersonCamera) & flag;
                if ((this.m_wasInFirstPerson != flag2) && (this.m_currentMovementState != MyCharacterMovementEnum.Sitting))
                {
                    MySector.MainCamera.Zoom.ApplyToFov = flag2;
                    this.UpdateNearFlag();
                }
                this.m_wasInFirstPerson = flag2;
                this.UpdateLightPower(false);
                this.m_currentAnimationChangeDelay += 0.01666667f;
                if ((Sync.IsServer && (!this.IsDead && !Sandbox.Game.Entities.MyEntities.IsInsideWorld(base.PositionComp.GetPosition()))) && MySession.Static.SurvivalMode)
                {
                    this.DoDamage(1000f, MyDamageType.Suicide, true, base.EntityId);
                }
                foreach (MyCharacterComponent component in base.Components)
                {
                    if (component == null)
                    {
                        continue;
                    }
                    if (component.NeedsUpdateBeforeSimulation)
                    {
                        component.UpdateBeforeSimulation();
                    }
                }
                if (this.m_canPlayImpact > 0f)
                {
                    this.m_canPlayImpact -= 0.01666667f;
                }
                if ((this.ReverbDetectorComp != null) && ReferenceEquals(this, MySession.Static.LocalCharacter))
                {
                    this.ReverbDetectorComp.Update();
                }
                if (this.m_resolveHighlightOverlap)
                {
                    if (this.ControllerInfo.IsLocallyControlled() && !(base.Parent is MyCockpit))
                    {
                        MyHighlightSystem highlightSystem = MySession.Static.GetComponent<MyHighlightSystem>();
                        if (highlightSystem != null)
                        {
                            this.Render.RenderObjectIDs.ForEach<uint>(id => highlightSystem.AddHighlightOverlappingModel(id));
                        }
                    }
                    this.m_resolveHighlightOverlap = false;
                }
                if (MySessionComponentReplay.Static.IsEntityBeingReplayed(base.EntityId, out data))
                {
                    if (data.SwitchWeaponData != null)
                    {
                        this.SwitchToWeaponInternal(data.SwitchWeaponData.Value.WeaponDefinition, false, data.SwitchWeaponData.Value.InventoryItemId, data.SwitchWeaponData.Value.WeaponEntityId);
                    }
                    if (data.ShootData != null)
                    {
                        if (data.ShootData.Value.Begin)
                        {
                            this.BeginShoot((MyShootActionEnum) data.ShootData.Value.ShootAction);
                        }
                        else
                        {
                            this.EndShoot((MyShootActionEnum) data.ShootData.Value.ShootAction);
                        }
                    }
                    if (data.AnimationData != null)
                    {
                        this.TriggerCharacterAnimationEvent(data.AnimationData.Value.Animation, false);
                    }
                    if (data.ControlSwitchesData != null)
                    {
                        if (data.ControlSwitchesData.Value.SwitchDamping)
                        {
                            ((VRage.Game.ModAPI.Interfaces.IMyControllableEntity) this).SwitchDamping();
                        }
                        if (data.ControlSwitchesData.Value.SwitchHelmet)
                        {
                            ((VRage.Game.ModAPI.Interfaces.IMyControllableEntity) this).SwitchHelmet();
                        }
                        if (data.ControlSwitchesData.Value.SwitchLandingGears)
                        {
                            ((VRage.Game.ModAPI.Interfaces.IMyControllableEntity) this).SwitchLandingGears();
                        }
                        if (data.ControlSwitchesData.Value.SwitchLights)
                        {
                            ((VRage.Game.ModAPI.Interfaces.IMyControllableEntity) this).SwitchLights();
                        }
                        if (data.ControlSwitchesData.Value.SwitchReactors)
                        {
                            ((VRage.Game.ModAPI.Interfaces.IMyControllableEntity) this).SwitchReactors();
                        }
                        if (data.ControlSwitchesData.Value.SwitchThrusts)
                        {
                            ((VRage.Game.ModAPI.Interfaces.IMyControllableEntity) this).SwitchThrusts();
                        }
                    }
                    if (data.UseData != null)
                    {
                        if (data.UseData.Value.Use)
                        {
                            this.Use();
                        }
                        else if (data.UseData.Value.UseContinues)
                        {
                            this.UseContinues();
                        }
                        else if (data.UseData.Value.UseFinished)
                        {
                            this.UseFinished();
                        }
                    }
                }
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            this.SuitRechargeDistributor.UpdateBeforeSimulation();
            this.RadioReceiver.UpdateBroadcastersInRange();
            if (ReferenceEquals(this, MySession.Static.LocalCharacter))
            {
                this.RadioReceiver.UpdateHud(false);
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            this.m_suitBattery.UpdateOnServer100();
            if (Sync.IsServer && !this.m_suitBattery.ResourceSource.HasCapacityRemaining)
            {
                float damage = 0f;
                switch (MySectorWeatherComponent.TemperatureToLevel(this.GetOutsideTemperature()))
                {
                    case MyTemperatureLevel.ExtremeFreeze:
                    case MyTemperatureLevel.ExtremeHot:
                        damage = 5f;
                        break;

                    case MyTemperatureLevel.Freeze:
                    case MyTemperatureLevel.Hot:
                        damage = 2f;
                        break;

                    default:
                        break;
                }
                if (damage > 0f)
                {
                    this.DoDamage(damage, MyDamageType.Environment, true, 0L);
                }
            }
            foreach (MyCharacterComponent component in base.Components)
            {
                if (component == null)
                {
                    continue;
                }
                if (component.NeedsUpdateBeforeSimulation100)
                {
                    component.UpdateBeforeSimulation100();
                }
            }
            if (this.AtmosphereDetectorComp != null)
            {
                this.AtmosphereDetectorComp.UpdateAtmosphereStatus();
            }
            if (((this.m_relativeDampeningEntityInit != 0) && (this.JetpackComp != null)) && !this.JetpackComp.DampenersTurnedOn)
            {
                this.m_relativeDampeningEntityInit = 0L;
            }
            if ((this.RelativeDampeningEntity == null) && (this.m_relativeDampeningEntityInit != 0))
            {
                this.RelativeDampeningEntity = Sandbox.Game.Entities.MyEntities.GetEntityByIdOrDefault(this.m_relativeDampeningEntityInit, null, false);
                if (this.RelativeDampeningEntity != null)
                {
                    this.m_relativeDampeningEntityInit = 0L;
                }
            }
            if (this.RelativeDampeningEntity != null)
            {
                MyEntityThrustComponent.UpdateRelativeDampeningEntity(this, this.RelativeDampeningEntity);
            }
        }

        private void UpdateBobQueue()
        {
            int index = this.IsInFirstPersonView ? this.m_headBoneIndex : this.m_camera3rdBoneIndex;
            if (index != -1)
            {
                int num1;
                this.m_bobQueue.Enqueue(base.BoneAbsoluteTransforms[index].Translation);
                if (((this.m_currentMovementState == MyCharacterMovementEnum.Standing) || ((this.m_currentMovementState == MyCharacterMovementEnum.Sitting) || ((this.m_currentMovementState == MyCharacterMovementEnum.Crouching) || ((this.m_currentMovementState == MyCharacterMovementEnum.RotatingLeft) || (this.m_currentMovementState == MyCharacterMovementEnum.RotatingRight))))) || (this.m_currentMovementState == MyCharacterMovementEnum.Died))
                {
                    num1 = 5;
                }
                else
                {
                    num1 = 20;
                }
                int num2 = num1;
                if (this.WantsCrouch)
                {
                    num2 = 3;
                }
                while (this.m_bobQueue.Count > num2)
                {
                    this.m_bobQueue.Dequeue();
                }
            }
        }

        private void UpdateBootsStateAndEmmisivity()
        {
            if ((this.IsMagneticBootsEnabled && (!this.IsDead && !this.IsSitting)) && this.Physics.CharacterProxy.Supported)
            {
                this.m_bootsState.Value = MyBootsState.Enabled;
            }
            else if ((((!this.JetpackRunning && !this.IsFalling) && !this.IsJumping) || ((this.Physics.CharacterProxy == null) || !this.Physics.CharacterProxy.Supported)) || (this.m_gravity.LengthSquared() >= 0.001f))
            {
                this.m_bootsState.Value = MyBootsState.Disabled;
            }
            else
            {
                this.m_bootsState.Value = MyBootsState.Proximity;
            }
        }

        public bool UpdateCalled()
        {
            base.m_actualDrawFrame = base.m_actualUpdateFrame;
            return (base.m_actualUpdateFrame != base.m_actualDrawFrame);
        }

        private void UpdateCameraDistance()
        {
            this.m_cameraDistance = (float) Vector3D.Distance(MySector.MainCamera.Position, base.WorldMatrix.Translation);
        }

        private bool UpdateCapsuleBones()
        {
            if (!this.m_characterBoneCapsulesReady)
            {
                if ((this.m_bodyCapsuleInfo == null) || (this.m_bodyCapsuleInfo.Count == 0))
                {
                    return false;
                }
                MyRenderDebugInputComponent.Clear();
                MyCharacterBone[] characterBones = base.AnimationController.CharacterBones;
                if ((this.Physics.Ragdoll == null) || !base.Components.Has<MyCharacterRagdollComponent>())
                {
                    for (int i = 0; i < this.m_bodyCapsuleInfo.Count; i++)
                    {
                        MyBoneCapsuleInfo info2 = this.m_bodyCapsuleInfo[i];
                        if (((characterBones != null) && (info2.Bone1 < characterBones.Length)) && (info2.Bone2 < characterBones.Length))
                        {
                            this.m_bodyCapsules[i].P0 = (characterBones[info2.Bone1].AbsoluteTransform * base.WorldMatrix).Translation;
                            this.m_bodyCapsules[i].P1 = (characterBones[info2.Bone2].AbsoluteTransform * base.WorldMatrix).Translation;
                            Vector3 vector4 = (Vector3) (this.m_bodyCapsules[i].P0 - this.m_bodyCapsules[i].P1);
                            if (info2.Radius != 0f)
                            {
                                this.m_bodyCapsules[i].Radius = info2.Radius;
                            }
                            else if (vector4.LengthSquared() >= 0.05f)
                            {
                                this.m_bodyCapsules[i].Radius = vector4.Length() * 0.3f;
                            }
                            else
                            {
                                this.m_bodyCapsules[i].P1 = this.m_bodyCapsules[i].P0 + ((characterBones[info2.Bone1].AbsoluteTransform * base.WorldMatrix).Left * 0.10000000149011612);
                                this.m_bodyCapsules[i].Radius = 0.1f;
                            }
                            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_SHOW_DAMAGE)
                            {
                                MyRenderDebugInputComponent.AddCapsule(this.m_bodyCapsules[i], Color.Green);
                            }
                        }
                    }
                }
                else
                {
                    MyCharacterRagdollComponent component = base.Components.Get<MyCharacterRagdollComponent>();
                    for (int i = 0; i < this.m_bodyCapsuleInfo.Count; i++)
                    {
                        MyBoneCapsuleInfo info = this.m_bodyCapsuleInfo[i];
                        if (((characterBones != null) && (info.Bone1 < characterBones.Length)) && (info.Bone2 < characterBones.Length))
                        {
                            MatrixD matrix = characterBones[info.Bone1].AbsoluteTransform * base.WorldMatrix;
                            HkShape shape = component.RagdollMapper.GetBodyBindedToBone(characterBones[info.Bone1]).GetShape();
                            this.m_bodyCapsules[i].P0 = matrix.Translation;
                            this.m_bodyCapsules[i].P1 = (characterBones[info.Bone2].AbsoluteTransform * base.WorldMatrix).Translation;
                            Vector3 vector = (Vector3) (this.m_bodyCapsules[i].P0 - this.m_bodyCapsules[i].P1);
                            if (vector.LengthSquared() >= 0.05f)
                            {
                                if (info.Radius != 0f)
                                {
                                    this.m_bodyCapsules[i].Radius = info.Radius;
                                }
                                else if (shape.ShapeType != HkShapeType.Capsule)
                                {
                                    this.m_bodyCapsules[i].Radius = vector.Length() * 0.28f;
                                }
                                else
                                {
                                    HkCapsuleShape shape3 = (HkCapsuleShape) shape;
                                    this.m_bodyCapsules[i].Radius = shape3.Radius;
                                }
                                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_SHOW_DAMAGE)
                                {
                                    MyRenderDebugInputComponent.AddCapsule(this.m_bodyCapsules[i], Color.Blue);
                                    MyRenderProxy.DebugDrawCapsule(this.m_bodyCapsules[i].P0, this.m_bodyCapsules[i].P1, this.m_bodyCapsules[i].Radius, Color.Yellow, false, false, false);
                                }
                            }
                            else if (shape.ShapeType == HkShapeType.Capsule)
                            {
                                HkCapsuleShape shape2 = (HkCapsuleShape) shape;
                                this.m_bodyCapsules[i].P0 = Vector3.Transform(shape2.VertexA, matrix);
                                this.m_bodyCapsules[i].P1 = Vector3.Transform(shape2.VertexB, matrix);
                                this.m_bodyCapsules[i].Radius = shape2.Radius * 0.8f;
                                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_SHOW_DAMAGE)
                                {
                                    MyRenderDebugInputComponent.AddCapsule(this.m_bodyCapsules[i], Color.Green);
                                }
                            }
                            else
                            {
                                VRageMath.Vector4 vector2;
                                VRageMath.Vector4 vector3;
                                shape.GetLocalAABB(0.0001f, out vector2, out vector3);
                                float num2 = Math.Max(Math.Max((float) (vector3.X - vector2.X), (float) (vector3.Y - vector2.Y)), vector3.Z - vector2.Z) * 0.5f;
                                this.m_bodyCapsules[i].P0 = matrix.Translation + ((matrix.Left * num2) * 0.25);
                                this.m_bodyCapsules[i].P1 = matrix.Translation + ((matrix.Left * num2) * 0.5);
                                this.m_bodyCapsules[i].Radius = num2 * 0.25f;
                                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_SHOW_DAMAGE)
                                {
                                    MyRenderDebugInputComponent.AddCapsule(this.m_bodyCapsules[i], Color.Blue);
                                }
                            }
                        }
                    }
                }
                this.m_characterBoneCapsulesReady = true;
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_SHOW_DAMAGE)
                {
                    foreach (Tuple<CapsuleD, Color> tuple in MyRenderDebugInputComponent.CapsulesToDraw)
                    {
                        MyRenderProxy.DebugDrawCapsule(tuple.Item1.P0, tuple.Item1.P1, tuple.Item1.Radius, tuple.Item2, false, false, false);
                    }
                }
            }
            return true;
        }

        public void UpdateCharacterPhysics(bool forceUpdate = false)
        {
            if ((this.Physics == null) || this.Physics.Enabled)
            {
                float num = (2f * MyPerGameSettings.PhysicsConvexRadius) + 0.03f;
                float maxSpeedRelativeToShip = Math.Max(this.Definition.MaxSprintSpeed, Math.Max(this.Definition.MaxRunSpeed, this.Definition.MaxBackrunSpeed));
                if (!Sync.IsServer && (!this.IsClientPredicted || this.ForceDisablePrediction))
                {
                    if (((this.Physics == null) || !this.Physics.IsStatic) | forceUpdate)
                    {
                        if (this.Physics != null)
                        {
                            this.Physics.Close();
                        }
                        float num3 = 1f;
                        int num4 = 0x16;
                        Vector3 center = new Vector3(0f, this.Definition.CharacterCollisionHeight / 2f, 0f);
                        this.InitCharacterPhysics(VRage.Game.MyMaterialType.CHARACTER, center, (this.Definition.CharacterCollisionWidth * this.Definition.CharacterCollisionScale) * num3, (this.Definition.CharacterCollisionHeight - ((this.Definition.CharacterCollisionWidth * this.Definition.CharacterCollisionScale) * num3)) - num, this.Definition.CharacterCollisionCrouchHeight - this.Definition.CharacterCollisionWidth, this.Definition.CharacterCollisionWidth - num, (this.Definition.CharacterHeadSize * this.Definition.CharacterCollisionScale) * num3, this.Definition.CharacterHeadHeight, 0.7f, 0.7f, (ushort) num4, RigidBodyFlag.RBF_STATIC, 0f, this.Definition.VerticalPositionFlyingOnly, this.Definition.MaxSlope, this.Definition.ImpulseLimit, maxSpeedRelativeToShip, true, this.Definition.MaxForce);
                        this.Physics.Enabled = true;
                    }
                }
                else if (((this.Physics == null) || this.Physics.IsStatic) | forceUpdate)
                {
                    Vector3 zero = Vector3.Zero;
                    if (this.Physics != null)
                    {
                        zero = this.Physics.LinearVelocityLocal;
                        this.Physics.Close();
                    }
                    Vector3 center = new Vector3(0f, this.Definition.CharacterCollisionHeight / 2f, 0f);
                    this.InitCharacterPhysics(VRage.Game.MyMaterialType.CHARACTER, center, this.Definition.CharacterCollisionWidth * this.Definition.CharacterCollisionScale, (this.Definition.CharacterCollisionHeight - (this.Definition.CharacterCollisionWidth * this.Definition.CharacterCollisionScale)) - num, this.Definition.CharacterCollisionCrouchHeight - this.Definition.CharacterCollisionWidth, this.Definition.CharacterCollisionWidth - num, this.Definition.CharacterHeadSize * this.Definition.CharacterCollisionScale, this.Definition.CharacterHeadHeight, 0.7f, 0.7f, 0x12, RigidBodyFlag.RBF_DEFAULT, MyPerGameSettings.Destruction ? MyDestructionHelper.MassToHavok(this.Definition.Mass) : this.Definition.Mass, this.Definition.VerticalPositionFlyingOnly, this.Definition.MaxSlope, this.Definition.ImpulseLimit, maxSpeedRelativeToShip, false, this.Definition.MaxForce);
                    if (this.Physics.CharacterProxy != null)
                    {
                        this.Physics.CharacterProxy.ContactPointCallback -= new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
                        this.Physics.CharacterProxy.ContactPointCallbackEnabled = true;
                        this.Physics.CharacterProxy.ContactPointCallback += new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
                    }
                    this.Physics.Enabled = true;
                    this.Physics.LinearVelocity = zero;
                    this.UpdateCrouchState();
                }
            }
        }

        private void UpdateCharacterStateChange()
        {
            if (!this.IsDead && (this.Physics.CharacterProxy != null))
            {
                this.OnCharacterStateChanged(this.Physics.CharacterProxy.GetState());
            }
        }

        private void UpdateCrouchState()
        {
            bool isCrouching = this.IsCrouching;
            bool flag2 = this.m_previousMovementState.GetMode() == 2;
            MyCharacterProxy characterProxy = this.Physics.CharacterProxy;
            if ((characterProxy != null) && (characterProxy.IsCrouching != isCrouching))
            {
                characterProxy.SetShapeForCrouch(this.Physics.HavokWorld, isCrouching);
            }
            if (isCrouching != flag2)
            {
                if (isCrouching)
                {
                    this.SetCrouchingLocalAABB();
                }
                else
                {
                    this.SetStandingLocalAABB();
                }
                if (characterProxy == null)
                {
                    this.UpdateCharacterPhysics(true);
                }
            }
        }

        public StringBuilder UpdateCustomNameWithFaction()
        {
            this.CustomNameWithFaction.Clear();
            MyIdentity identity = this.GetIdentity();
            if (identity == null)
            {
                this.CustomNameWithFaction.Append(base.DisplayName);
            }
            else
            {
                IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(identity.IdentityId);
                if (faction != null)
                {
                    this.CustomNameWithFaction.Append(faction.Tag);
                    this.CustomNameWithFaction.Append('.');
                }
                this.CustomNameWithFaction.Append(identity.DisplayName);
            }
            return this.CustomNameWithFaction;
        }

        private void UpdateDying()
        {
            if (this.m_dieAfterSimulation)
            {
                this.m_bootsState.ValueChanged -= new Action<SyncBase>(this.OnBootsStateChanged);
                this.DieInternal();
                this.m_dieAfterSimulation = false;
            }
        }

        private void UpdateFallAndSpine()
        {
            MyCharacterJetpackComponent jetpackComp = this.JetpackComp;
            if (jetpackComp != null)
            {
                jetpackComp.UpdateFall();
            }
            if (this.m_isFalling && !this.JetpackRunning)
            {
                this.m_currentFallingTime += 0.01666667f;
                if ((this.m_currentFallingTime > 0.3f) && !this.m_isFallingAnimationPlayed)
                {
                    this.SwitchAnimation(MyCharacterMovementEnum.Falling, false);
                    this.m_isFallingAnimationPlayed = true;
                }
            }
            if (((this.JetpackRunning && (!jetpackComp.Running || (!this.IsLocalHeadAnimationInProgress() && !this.Definition.VerticalPositionFlyingOnly))) || (this.IsDead || this.IsSitting)) || this.IsOnLadder)
            {
                if (base.UseNewAnimationSystem)
                {
                    base.AnimationController.Variables.SetValue(MyAnimationVariableStorageHints.StrIdLean, 0f);
                }
                else
                {
                    this.SetSpineAdditionalRotation(Quaternion.CreateFromAxisAngle(Vector3.Backward, 0f), Quaternion.CreateFromAxisAngle(Vector3.Backward, 0f), true);
                }
            }
            else
            {
                float num = this.IsInFirstPersonView ? this.m_characterDefinition.BendMultiplier1st : this.m_characterDefinition.BendMultiplier3rd;
                if (!base.UseNewAnimationSystem)
                {
                    float num4 = MathHelper.Clamp(-this.m_headLocalXAngle, -45f, 89f);
                    Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.Backward, MathHelper.ToRadians((float) (num * num4)));
                    this.SetSpineAdditionalRotation(rotation, Quaternion.CreateFromAxisAngle(Vector3.Backward, MathHelper.ToRadians((float) (this.m_characterDefinition.BendMultiplier3rd * num4))), true);
                }
                else
                {
                    float num2 = MathHelper.Clamp(-this.m_headLocalXAngle, -89.9f, 89f);
                    float num3 = this.m_characterDefinition.BendMultiplier3rd * num2;
                    if (ReferenceEquals(MySession.Static.LocalCharacter, this) && ((!MyInput.Static.IsGameControlPressed(MyControlsSpace.LOOKAROUND) || (this.IsInFirstPersonView || this.ForceFirstPersonCamera)) || (this.CurrentWeapon != null)))
                    {
                        this.m_animLeaning.Value = num3;
                    }
                }
            }
            if (((this.m_currentWeapon != null) || (this.IsDead || this.JetpackRunning)) || this.IsSitting)
            {
                this.SetHandAdditionalRotation(Quaternion.CreateFromAxisAngle(Vector3.Forward, MathHelper.ToRadians((float) 0f)), true);
                this.SetUpperHandAdditionalRotation(Quaternion.CreateFromAxisAngle(Vector3.Forward, MathHelper.ToRadians((float) 0f)), true);
            }
            else
            {
                float headLocalXAngle = this.m_headLocalXAngle;
                float single4 = this.m_headLocalXAngle;
            }
        }

        private void UpdateHeadModelProperties(bool enabled)
        {
            if (this.m_characterDefinition.MaterialsDisabledIn1st != null)
            {
                this.m_headRenderingEnabled = enabled;
                if (this.Render.RenderObjectIDs[0] != uint.MaxValue)
                {
                    foreach (string str in this.m_characterDefinition.MaterialsDisabledIn1st)
                    {
                        Color? diffuseColor = null;
                        float? emissivity = null;
                        MyRenderProxy.UpdateModelProperties(this.Render.RenderObjectIDs[0], str, enabled ? RenderFlags.Visible : ((RenderFlags) 0), enabled ? ((RenderFlags) 0) : RenderFlags.Visible, diffuseColor, emissivity);
                    }
                }
            }
        }

        private void UpdateHeadOffset()
        {
            MyCharacterMovementEnum currentMovementState = this.m_currentMovementState;
            if (currentMovementState > MyCharacterMovementEnum.CrouchWalkingRightFront)
            {
                if (currentMovementState > MyCharacterMovementEnum.RunningLeftBack)
                {
                    if (currentMovementState > MyCharacterMovementEnum.Sprinting)
                    {
                        if (currentMovementState > MyCharacterMovementEnum.CrouchRotatingLeft)
                        {
                            if ((currentMovementState == MyCharacterMovementEnum.RotatingRight) || (currentMovementState == MyCharacterMovementEnum.CrouchRotatingRight))
                            {
                                goto TR_0004;
                            }
                        }
                        else
                        {
                            if ((currentMovementState != MyCharacterMovementEnum.RotatingLeft) && (currentMovementState != MyCharacterMovementEnum.CrouchRotatingLeft))
                            {
                                return;
                            }
                            goto TR_0004;
                        }
                        return;
                    }
                    else if (currentMovementState > MyCharacterMovementEnum.RunningRightFront)
                    {
                        if (currentMovementState != MyCharacterMovementEnum.RunningRightBack)
                        {
                            if (currentMovementState != MyCharacterMovementEnum.Sprinting)
                            {
                                return;
                            }
                            goto TR_0001;
                        }
                        return;
                    }
                    else if (currentMovementState != MyCharacterMovementEnum.RunStrafingRight)
                    {
                        MyCharacterMovementEnum enum6 = currentMovementState;
                        return;
                    }
                    goto TR_0017;
                }
                else if (currentMovementState > MyCharacterMovementEnum.Running)
                {
                    if (currentMovementState > MyCharacterMovementEnum.RunStrafingLeft)
                    {
                        if (currentMovementState != MyCharacterMovementEnum.RunningLeftFront)
                        {
                            MyCharacterMovementEnum enum5 = currentMovementState;
                            return;
                        }
                    }
                    else
                    {
                        if (currentMovementState == MyCharacterMovementEnum.Backrunning)
                        {
                            goto TR_0008;
                        }
                        else if (currentMovementState != MyCharacterMovementEnum.RunStrafingLeft)
                        {
                            return;
                        }
                        goto TR_000C;
                    }
                }
                else if ((currentMovementState != MyCharacterMovementEnum.WalkingRightBack) && (currentMovementState != MyCharacterMovementEnum.CrouchWalkingRightBack))
                {
                    if (currentMovementState != MyCharacterMovementEnum.Running)
                    {
                        return;
                    }
                    goto TR_0001;
                }
            }
            else
            {
                if (currentMovementState > MyCharacterMovementEnum.CrouchStrafingLeft)
                {
                    if (currentMovementState > MyCharacterMovementEnum.CrouchWalkingLeftBack)
                    {
                        if (currentMovementState > MyCharacterMovementEnum.CrouchStrafingRight)
                        {
                            if (currentMovementState != MyCharacterMovementEnum.WalkingRightFront)
                            {
                                MyCharacterMovementEnum enum4 = currentMovementState;
                                return;
                            }
                            return;
                        }
                        else if ((currentMovementState != MyCharacterMovementEnum.WalkStrafingRight) && (currentMovementState != MyCharacterMovementEnum.CrouchStrafingRight))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (currentMovementState <= MyCharacterMovementEnum.CrouchWalkingLeftFront)
                        {
                            if (currentMovementState != MyCharacterMovementEnum.WalkingLeftFront)
                            {
                                MyCharacterMovementEnum enum1 = currentMovementState;
                                return;
                            }
                        }
                        else if (currentMovementState != MyCharacterMovementEnum.WalkingLeftBack)
                        {
                            MyCharacterMovementEnum enum3 = currentMovementState;
                            return;
                        }
                        return;
                    }
                    goto TR_0017;
                }
                else
                {
                    if (currentMovementState > MyCharacterMovementEnum.CrouchWalking)
                    {
                        if (currentMovementState > MyCharacterMovementEnum.CrouchBackWalking)
                        {
                            if ((currentMovementState != MyCharacterMovementEnum.WalkStrafingLeft) && (currentMovementState != MyCharacterMovementEnum.CrouchStrafingLeft))
                            {
                                return;
                            }
                        }
                        else
                        {
                            if ((currentMovementState != MyCharacterMovementEnum.BackWalking) && (currentMovementState != MyCharacterMovementEnum.CrouchBackWalking))
                            {
                                return;
                            }
                            goto TR_0008;
                        }
                        goto TR_000C;
                    }
                    else
                    {
                        switch (currentMovementState)
                        {
                            case MyCharacterMovementEnum.Standing:
                            case MyCharacterMovementEnum.Crouching:
                            case MyCharacterMovementEnum.Falling:
                            case MyCharacterMovementEnum.Jump:
                                goto TR_0004;

                            case MyCharacterMovementEnum.Sitting:
                            case MyCharacterMovementEnum.Flying:
                                break;

                            default:
                                if ((currentMovementState != MyCharacterMovementEnum.Walking) && (currentMovementState != MyCharacterMovementEnum.CrouchWalking))
                                {
                                    return;
                                }
                                goto TR_0001;
                        }
                    }
                    return;
                }
                goto TR_0004;
            }
            return;
        TR_0001:
            this.AccelerateX(-1f);
            this.SlowDownY();
            return;
        TR_0004:
            this.SlowDownX();
            this.SlowDownY();
            return;
        TR_0008:
            this.AccelerateX(1f);
            this.SlowDownY();
            return;
        TR_000C:
            this.SlowDownX();
            this.AccelerateY(1f);
            return;
        TR_0017:
            this.SlowDownX();
            this.AccelerateY(-1f);
        }

        private void UpdateHudMarker()
        {
            if (!MyFakes.ENABLE_RADIO_HUD)
            {
                MyHudEntityParams hudParams = new MyHudEntityParams {
                    FlagsEnum = MyHudIndicatorFlagsEnum.SHOW_TEXT,
                    Text = new StringBuilder(this.GetIdentity().DisplayName),
                    ShouldDraw = new Func<bool>(MyHud.CheckShowPlayerNamesOnHud)
                };
                MyHud.LocationMarkers.RegisterMarker(this, hudParams);
            }
        }

        private unsafe void UpdateLadder()
        {
            if (this.IsOnLadder && (!this.m_ladder.MarkedForClose && !this.m_needReconnectLadder))
            {
                if ((Sync.IsServer && ((this.m_constraintInstance != null) && (this.m_constraintBreakableData != null))) && this.m_constraintBreakableData.getIsBroken(this.m_constraintInstance))
                {
                    this.GetOffLadder();
                }
                if (this.m_currentLadderStep > 0)
                {
                    bool flag;
                    Vector3D translation = base.PositionComp.WorldMatrix.Translation;
                    float stepIncrement = this.m_stepIncrement;
                    if (this.GetCurrentMovementState() == MyCharacterMovementEnum.LadderDown)
                    {
                        stepIncrement = -stepIncrement;
                    }
                    float* singlePtr1 = (float*) ref this.m_ladderIncrementToBase.Y;
                    singlePtr1[0] += stepIncrement;
                    Vector3 movementDelta = (Vector3) (base.WorldMatrix.Up * stepIncrement);
                    MyLadder ladder = this.CheckBottomLadder(translation, ref movementDelta, out flag);
                    MyLadder ladder2 = this.CheckTopLadder(translation, ref movementDelta, out flag);
                    if ((((this.m_currentMovementState == MyCharacterMovementEnum.LadderUp) && (ladder2 != null)) || (((this.m_currentMovementState == MyCharacterMovementEnum.LadderDown) && (ladder != null)) || (this.Physics.CharacterProxy == null))) || (this.m_currentMovementState == MyCharacterMovementEnum.LadderOut))
                    {
                        MyLadder objA = this.CheckMiddleLadder(translation + (base.WorldMatrix.Up * 0.10000000149011612), ref movementDelta);
                        MyLadder objB = this.CheckMiddleLadder(translation - (base.WorldMatrix.Up * 0.10000000149011612), ref movementDelta);
                        if ((ReferenceEquals(objA, objB) && !ReferenceEquals(objB, this.m_ladder)) && (objB != null))
                        {
                            this.ChangeLadder(objB, false);
                        }
                        if ((this.m_currentLadderStep < 20) && (this.m_currentMovementState == MyCharacterMovementEnum.LadderOut))
                        {
                            Vector3 vector2 = new Vector3(0f, 0.001f, 0.025f);
                            float* singlePtr2 = (float*) ref this.m_ladderIncrementToBase.Y;
                            singlePtr2[0] += vector2.Y - stepIncrement;
                            float* singlePtr3 = (float*) ref this.m_ladderIncrementToBase.Z;
                            singlePtr3[0] += vector2.Z;
                        }
                        MatrixD characterWM = this.m_baseMatrix * this.m_ladder.WorldMatrix;
                        MatrixD* xdPtr1 = (MatrixD*) ref characterWM;
                        xdPtr1.Translation += base.WorldMatrix.Up * this.m_ladderIncrementToBase.Y;
                        MatrixD* xdPtr2 = (MatrixD*) ref characterWM;
                        xdPtr2.Translation += base.WorldMatrix.Forward * this.m_ladderIncrementToBase.Z;
                        if ((this.Physics.CharacterProxy != null) && (this.m_constraintInstance != null))
                        {
                            this.SetCharacterLadderConstraint(characterWM);
                        }
                    }
                    this.m_currentLadderStep--;
                    if (this.m_currentLadderStep == 0)
                    {
                        if ((this.GetCurrentMovementState() == MyCharacterMovementEnum.LadderUp) || (this.GetCurrentMovementState() == MyCharacterMovementEnum.LadderDown))
                        {
                            this.SetCurrentMovementState(MyCharacterMovementEnum.Ladder);
                        }
                        else if ((this.GetCurrentMovementState() == MyCharacterMovementEnum.LadderOut) && Sync.IsServer)
                        {
                            Vector3 linearVelocity = this.m_ladder.Parent.Physics.LinearVelocity;
                            Vector3 position = this.m_ladder.StopMatrix.Translation;
                            if (Vector3.Dot((Vector3) base.WorldMatrix.Up, (Vector3) this.m_ladder.PositionComp.WorldMatrix.Up) < 0f)
                            {
                                Vector3* vectorPtr1 = (Vector3*) ref position;
                                vectorPtr1 = (Vector3*) new Vector3(position.X, -position.Y, position.Z);
                            }
                            Vector3D pos = Vector3D.Transform(position, this.m_ladder.WorldMatrix) - (base.WorldMatrix.Up * 0.20000000298023224);
                            this.GetOffLadder();
                            base.PositionComp.SetPosition(pos, null, false, true);
                            if (Vector3.IsZero(this.Gravity))
                            {
                                this.Physics.LinearVelocity = linearVelocity + (base.WorldMatrix.Down * 0.5);
                            }
                        }
                    }
                }
                if ((this.Physics.CharacterProxy == null) && (this.m_constraintInstance == null))
                {
                    MatrixD worldMatrix = this.m_baseMatrix * this.m_ladder.WorldMatrix;
                    MatrixD* xdPtr3 = (MatrixD*) ref worldMatrix;
                    xdPtr3.Translation += base.WorldMatrix.Up * this.m_ladderIncrementToBase.Y;
                    MatrixD* xdPtr4 = (MatrixD*) ref worldMatrix;
                    xdPtr4.Translation += base.WorldMatrix.Forward * this.m_ladderIncrementToBase.Z;
                    base.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                }
            }
        }

        private void UpdateLadderNotifications()
        {
            if (ReferenceEquals(this, MySession.Static.LocalCharacter))
            {
                if (!this.IsOnLadder)
                {
                    if (this.m_ladderOffNotification != null)
                    {
                        MyHud.Notifications.Remove(this.m_ladderOffNotification);
                        this.m_ladderOffNotification = null;
                    }
                    if (this.m_ladderUpDownNotification != null)
                    {
                        MyHud.Notifications.Remove(this.m_ladderUpDownNotification);
                        this.m_ladderUpDownNotification = null;
                    }
                    if (this.m_ladderJumpOffNotification != null)
                    {
                        MyHud.Notifications.Remove(this.m_ladderJumpOffNotification);
                        this.m_ladderJumpOffNotification = null;
                    }
                }
                else
                {
                    if (this.m_ladderOffNotification == null)
                    {
                        this.m_ladderOffNotification = new MyHudNotification(MySpaceTexts.NotificationHintPressToGetDownFromLadder, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Control);
                        if (!MyInput.Static.IsJoystickConnected() || !MyInput.Static.IsJoystickLastUsed)
                        {
                            object[] arguments = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.USE).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard) + "]" };
                            this.m_ladderOffNotification.SetTextFormatArguments(arguments);
                        }
                        else
                        {
                            object[] arguments = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.USE).ToString() + "]" };
                            this.m_ladderOffNotification.SetTextFormatArguments(arguments);
                        }
                        MyHud.Notifications.Add(this.m_ladderOffNotification);
                    }
                    if (this.m_ladderUpDownNotification == null)
                    {
                        this.m_ladderUpDownNotification = new MyHudNotification(MySpaceTexts.NotificationHintPressToClimbUpDown, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Control);
                        if (!MyInput.Static.IsJoystickConnected() || !MyInput.Static.IsJoystickLastUsed)
                        {
                            object[] arguments = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.FORWARD).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard) + "]", "[" + MyInput.Static.GetGameControl(MyControlsSpace.BACKWARD).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard) + "]" };
                            this.m_ladderUpDownNotification.SetTextFormatArguments(arguments);
                        }
                        else
                        {
                            object[] arguments = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.FORWARD).ToString() + "]", "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.BACKWARD).ToString() + "]" };
                            this.m_ladderUpDownNotification.SetTextFormatArguments(arguments);
                        }
                        MyHud.Notifications.Add(this.m_ladderUpDownNotification);
                    }
                    if (this.m_ladderJumpOffNotification == null)
                    {
                        this.m_ladderJumpOffNotification = new MyHudNotification(MySpaceTexts.NotificationHintPressToJumpOffLadder, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Control);
                        if (!MyInput.Static.IsJoystickConnected() || !MyInput.Static.IsJoystickLastUsed)
                        {
                            object[] arguments = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.JUMP).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard) + "]" };
                            this.m_ladderJumpOffNotification.SetTextFormatArguments(arguments);
                        }
                        else
                        {
                            object[] arguments = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.JUMP).ToString() + "]" };
                            this.m_ladderJumpOffNotification.SetTextFormatArguments(arguments);
                        }
                        MyHud.Notifications.Add(this.m_ladderJumpOffNotification);
                    }
                }
            }
        }

        private unsafe void UpdateLeftHandItemPosition()
        {
            MatrixD xd = base.AnimationController.CharacterBones[this.m_leftHandItemBone].AbsoluteTransform * base.WorldMatrix;
            MatrixD* xdPtr1 = (MatrixD*) ref xd;
            xdPtr1.Up = xd.Forward;
            xd.Forward = xd.Up;
            MatrixD* xdPtr2 = (MatrixD*) ref xd;
            xdPtr2.Right = Vector3D.Cross(xd.Forward, xd.Up);
            this.m_leftHandItem.WorldMatrix = xd;
        }

        public void UpdateLightPower(bool chargeImmediately = false)
        {
            float currentLightPower = this.m_currentLightPower;
            if ((this.m_lightPowerFromProducer <= 0f) || !this.m_lightEnabled)
            {
                this.m_currentLightPower = chargeImmediately ? 0f : MathHelper.Clamp((float) (this.m_currentLightPower - this.m_lightTurningOffSpeed), (float) 0f, (float) 1f);
            }
            else
            {
                this.m_currentLightPower = !chargeImmediately ? MathHelper.Clamp((float) (this.m_currentLightPower + this.m_lightTurningOnSpeed), (float) 0f, (float) 1f) : 1f;
            }
            if (this.Render != null)
            {
                this.Render.UpdateLight(this.m_currentLightPower, !(currentLightPower == this.m_currentLightPower), LIGHT_PARAMETERS_CHANGED);
            }
            if (this.RadioBroadcaster != null)
            {
                if (this.RadioBroadcaster.WantsToBeEnabled && (this.m_suitBattery != null))
                {
                    this.RadioBroadcaster.Enabled = this.m_suitBattery.ResourceSource.CurrentOutput > 0f;
                }
                else
                {
                    this.RadioBroadcaster.Enabled = false;
                }
            }
        }

        private bool UpdateLooting(float amount)
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_MISC)
            {
                MyRenderProxy.DebugDrawText3D(base.WorldMatrix.Translation, this.m_currentLootingCounter.ToString("n1"), Color.Green, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            if (this.m_currentLootingCounter > 0f)
            {
                this.m_currentLootingCounter -= amount;
                if ((this.m_currentLootingCounter <= 0f) && Sync.IsServer)
                {
                    base.Close();
                    base.Save = false;
                    return true;
                }
            }
            return false;
        }

        public void UpdateMovementAndFlags(MyCharacterMovementEnum movementState, MyCharacterMovementFlags flags)
        {
            if ((this.m_currentMovementState != movementState) && (this.Physics != null))
            {
                this.m_movementFlags = flags;
                this.SwitchAnimation(movementState, true);
                this.SetCurrentMovementState(movementState);
            }
        }

        private void UpdateNearFlag()
        {
            int num1;
            if ((!this.ControllerInfo.IsLocallyControlled() || !ReferenceEquals(MySession.Static.CameraController, this)) || (!this.m_isInFirstPerson && !this.ForceFirstPersonCamera))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) !this.IsOnLadder;
            }
            bool flag = ((bool) num1) & (this.CurrentMovementState != MyCharacterMovementEnum.Sitting);
            if (this.m_currentWeapon != null)
            {
                ((VRage.Game.Entity.MyEntity) this.m_currentWeapon).Render.NearFlag = flag;
            }
            if (this.m_leftHandItem != null)
            {
                this.m_leftHandItem.Render.NearFlag = flag;
            }
            this.Render.NearFlag = flag;
            this.m_bobQueue.Clear();
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (this.m_needReconnectLadder)
            {
                if (this.m_ladder != null)
                {
                    this.ReconnectConstraint(this.m_oldLadderGrid, this.m_ladder.CubeGrid);
                    if (this.m_constraintInstance != null)
                    {
                        this.SetCharacterLadderConstraint(base.WorldMatrix);
                    }
                }
                this.m_needReconnectLadder = false;
                this.m_oldLadderGrid = null;
            }
            this.RecalculatePowerRequirement(true);
            MyEntityStat health = this.StatComp?.Health;
            if (health != null)
            {
                if (this.m_savedHealth != null)
                {
                    health.Value = this.m_savedHealth.Value;
                }
                health.OnStatChanged += new MyEntityStat.StatChangedDelegate(this.StatComp.OnHealthChanged);
            }
            if (this.m_breath != null)
            {
                this.m_breath.ForceUpdate();
            }
            if (this.m_currentMovementState == MyCharacterMovementEnum.Died)
            {
                this.Physics.ForceActivate();
            }
            base.UpdateOnceBeforeFrame();
            if (this.m_currentWeapon != null)
            {
                Sandbox.Game.Entities.MyEntities.Remove((VRage.Game.Entity.MyEntity) this.m_currentWeapon);
                this.EquipWeapon(this.m_currentWeapon, false);
            }
            if (((this.ControllerInfo.Controller == null) && ((this.m_savedPlayer != null) && (this.m_savedPlayer.Value.SteamId != 0))) && Sync.IsServer)
            {
                this.m_controlInfo.Value = this.m_savedPlayer.Value;
            }
            if (this.m_relativeDampeningEntityInit != 0)
            {
                this.RelativeDampeningEntity = Sandbox.Game.Entities.MyEntities.GetEntityByIdOrDefault(this.m_relativeDampeningEntityInit, null, false);
            }
            if (this.m_ladderIdInit != null)
            {
                if (!(Sandbox.Game.Entities.MyEntities.GetEntityById(this.m_ladderIdInit.Value, false) is MyLadder))
                {
                    base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                }
                else
                {
                    this.GetOnLadder_Implementation(this.m_ladderIdInit.Value, false);
                    this.m_ladderIdInit = null;
                }
            }
            this.UpdateAssetModifiers();
        }

        private void UpdateOutsideTemperature()
        {
            MyCockpit parent = base.Parent as MyCockpit;
            if (((parent != null) && parent.BlockDefinition.IsPressurized) && parent.IsWorking)
            {
                this.m_outsideTemperature = 0.5f;
            }
            else
            {
                float temperatureInPoint = MySectorWeatherComponent.GetTemperatureInPoint(base.PositionComp.GetPosition());
                float num2 = temperatureInPoint;
                if (this.OxygenSourceGridEntityId != null)
                {
                    num2 = MathHelper.Lerp(temperatureInPoint, 0.5f, (float) this.OxygenLevelAtCharacterLocation);
                }
                float outsideTemperature = this.m_outsideTemperature;
                this.m_outsideTemperature = num2;
                if (MySectorWeatherComponent.TemperatureToLevel(this.m_outsideTemperature) != MySectorWeatherComponent.TemperatureToLevel(outsideTemperature))
                {
                    this.RecalculatePowerRequirement(false);
                }
            }
        }

        public void UpdateOxygen(float oxygenAmount)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, float>(this, x => new Action<float>(x.OnUpdateOxygen), oxygenAmount, targetEndpoint);
        }

        internal unsafe void UpdatePhysicalMovement()
        {
            if ((MySandboxGame.IsGameReady && ((this.Physics != null) && (this.Physics.Enabled && MySession.Static.Ready))) && (this.Physics.HavokWorld != null))
            {
                MyCharacterJetpackComponent jetpackComp = this.JetpackComp;
                bool flag = (jetpackComp != null) && jetpackComp.UpdatePhysicalMovement();
                bool flag2 = MyGravityProviderSystem.IsGravityReady();
                this.m_gravity = MyGravityProviderSystem.CalculateTotalGravityInPoint(base.PositionComp.WorldAABB.Center) + this.Physics.HavokWorld.Gravity;
                if (this.m_gravity.Length() > 100f)
                {
                    this.m_gravity.Normalize();
                    this.m_gravity *= 100f;
                }
                MatrixD worldMatrix = base.WorldMatrix;
                bool flag3 = false;
                bool flag4 = true;
                if ((((flag && !this.Definition.VerticalPositionFlyingOnly) && !this.IsMagneticBootsEnabled) || this.IsDead) || this.IsOnLadder)
                {
                    if (!this.IsDead)
                    {
                        if (this.IsOnLadder && (this.Physics.CharacterProxy != null))
                        {
                            this.Physics.CharacterProxy.Gravity = Vector3.Zero;
                            MatrixD xd4 = this.m_baseMatrix * this.m_ladder.WorldMatrix;
                            this.Physics.CharacterProxy.SetForwardAndUp((Vector3) xd4.Forward, (Vector3) xd4.Up);
                        }
                    }
                    else if (this.Physics.HasRigidBody && this.Physics.RigidBody.IsActive)
                    {
                        Vector3 gravity = this.m_gravity;
                        if ((Sync.IsDedicated && MyFakes.ENABLE_RAGDOLL) && !MyFakes.ENABLE_RAGDOLL_CLIENT_SYNC)
                        {
                            gravity = Vector3.Zero;
                        }
                        this.Physics.RigidBody.Gravity = gravity;
                    }
                }
                else
                {
                    Vector3 up = (Vector3) worldMatrix.Up;
                    Vector3 forward = (Vector3) worldMatrix.Forward;
                    if (this.Physics.CharacterProxy != null)
                    {
                        if (!this.Physics.CharacterProxy.Up.IsValid() || !this.Physics.CharacterProxy.Forward.IsValid())
                        {
                            this.Physics.CharacterProxy.SetForwardAndUp((Vector3) worldMatrix.Forward, (Vector3) worldMatrix.Up);
                        }
                        up = this.Physics.CharacterProxy.Up;
                        forward = this.Physics.CharacterProxy.Forward;
                        this.Physics.CharacterProxy.Gravity = flag ? Vector3.Zero : (this.m_gravity * MyPerGameSettings.CharacterGravityMultiplier);
                    }
                    if (((this.m_gravity.LengthSquared() > 0.1f) && (up != Vector3.Zero)) && this.m_gravity.IsValid())
                    {
                        this.UpdateStandup(ref this.m_gravity, ref up, ref forward);
                        if (jetpackComp != null)
                        {
                            jetpackComp.CurrentAutoEnableDelay = 0f;
                        }
                    }
                    else
                    {
                        int num1;
                        if (this.IsMagneticBootsEnabled)
                        {
                            Vector3 gravity = -this.Physics.CharacterProxy.SupportNormal;
                            this.UpdateStandup(ref gravity, ref up, ref forward);
                            if (!this.IsMagneticBootsActive && Sync.IsServer)
                            {
                                this.UpdateBootsStateAndEmmisivity();
                            }
                        }
                        else if ((!this.IsJumping && (!this.IsFalling && !this.JetpackRunning)) && (this.Physics.CharacterProxy == null))
                        {
                            MatrixD xd3 = this.Physics.GetWorldMatrix();
                            MyPhysics.HitInfo? nullable = MyPhysics.CastRay(xd3.Translation + xd3.Up, xd3.Translation + (xd3.Down * 0.5), 30);
                            if (nullable != null)
                            {
                                Vector3 gravity = -nullable.Value.HkHitInfo.Normal;
                                this.UpdateStandup(ref gravity, ref up, ref forward);
                            }
                        }
                        if ((jetpackComp == null) || (jetpackComp.CurrentAutoEnableDelay == -1f))
                        {
                            num1 = 0;
                        }
                        else
                        {
                            num1 = (int) !this.IsMagneticBootsActive;
                        }
                        if ((num1 & flag2) != 0)
                        {
                            jetpackComp.CurrentAutoEnableDelay += 0.01666667f;
                        }
                    }
                    if (this.Physics.CharacterProxy != null)
                    {
                        this.Physics.CharacterProxy.SetForwardAndUp(forward, up);
                    }
                    else
                    {
                        flag4 = false;
                        worldMatrix = MatrixD.CreateWorld(worldMatrix.Translation, forward, up);
                    }
                }
                if (flag4)
                {
                    worldMatrix = this.Physics.GetWorldMatrix();
                }
                if (this.m_currentMovementState != MyCharacterMovementEnum.Standing)
                {
                    this.m_cummulativeVerticalFootError = 0f;
                }
                else
                {
                    this.m_cummulativeVerticalFootError += this.m_verticalFootError * 0.2f;
                    this.m_cummulativeVerticalFootError = MathHelper.Clamp(this.m_cummulativeVerticalFootError, -0.75f, 0.75f);
                }
                MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                xdPtr1.Translation = worldMatrix.Translation + (worldMatrix.Up * this.m_cummulativeVerticalFootError);
                Vector3D zero = worldMatrix.Translation - base.WorldMatrix.Translation;
                if (((zero.LengthSquared() <= 9.9999997473787516E-06) && (Vector3D.DistanceSquared(base.WorldMatrix.Forward, worldMatrix.Forward) <= 9.9999997473787516E-06)) && (Vector3D.DistanceSquared(base.WorldMatrix.Up, worldMatrix.Up) <= 9.9999997473787516E-06))
                {
                    zero = Vector3D.Zero;
                }
                else
                {
                    object physics;
                    if (flag3 || !flag4)
                    {
                        physics = null;
                    }
                    else
                    {
                        physics = this.Physics;
                    }
                    base.PositionComp.SetWorldMatrix(worldMatrix, physics, false, true, true, false, false, false);
                }
                MyCharacterProxy characterProxy = this.Physics.CharacterProxy;
                if (characterProxy != null)
                {
                    HkCharacterRigidBody characterRigidBody = characterProxy.CharacterRigidBody;
                    if (characterRigidBody != null)
                    {
                        characterRigidBody.InterpolatedVelocity = (Vector3) (zero / 0.01666666753590107);
                    }
                }
                if (this.IsClientPredicted || Sync.IsServer)
                {
                    this.Physics.UpdateAccelerations();
                }
            }
        }

        public unsafe void UpdatePredictionFlag()
        {
            if (Sync.IsServer || this.IsDead)
            {
                this.IsClientPredicted = true;
            }
            else
            {
                int num1;
                if (this.ForceDisablePrediction && (MySandboxGame.Static.SimulationTime.Seconds > (this.m_forceDisablePredictionTime + 10.0)))
                {
                    this.ForceDisablePrediction = false;
                }
                bool flag = ReferenceEquals(MySession.Static.TopMostControlledEntity, this);
                if ((!(MyFakes.MULTIPLAYER_CLIENT_SIMULATE_CONTROLLED_CHARACTER & flag) || (this.IsDead || (this.JetpackRunning && !MyFakes.MULTIPLAYER_CLIENT_SIMULATE_CONTROLLED_CHARACTER_IN_JETPACK))) || this.ForceDisablePrediction)
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) !this.AlwaysDisablePrediction;
                }
                bool flag2 = (bool) num1;
                if (this.ControllerInfo.IsLocallyControlled())
                {
                    HkShape collisionShape;
                    MyCharacterProxy characterProxy = this.Physics.CharacterProxy;
                    if (characterProxy != null)
                    {
                        collisionShape = characterProxy.GetCollisionShape();
                    }
                    if (!collisionShape.IsZero)
                    {
                        using (MyUtils.ReuseCollection<HkBodyCollision>(ref this.m_physicsCollisionResults))
                        {
                            MatrixD worldMatrix = this.Physics.GetWorldMatrix();
                            MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                            xdPtr1.Translation += Vector3D.TransformNormal(this.Physics.Center, ref worldMatrix);
                            Vector3D translation = worldMatrix.Translation;
                            Quaternion rotation = Quaternion.CreateFromRotationMatrix(worldMatrix);
                            MyPhysics.GetPenetrationsShape(collisionShape, ref translation, ref rotation, this.m_physicsCollisionResults, 30);
                            foreach (HkBodyCollision collision in this.m_physicsCollisionResults)
                            {
                                MyGridPhysics userObject = collision.Body.UserObject as MyGridPhysics;
                                if (userObject != null)
                                {
                                    if (this.IsOnLadder)
                                    {
                                        MySlimBlock blockFromShapeKey = userObject.Shape.GetBlockFromShapeKey(collision.ShapeKey);
                                        if (((blockFromShapeKey != null) && (blockFromShapeKey.FatBlock is MyLadder)) && !this.ShouldCollideWith(blockFromShapeKey.FatBlock as MyLadder))
                                        {
                                            this.ForceDisablePrediction = false;
                                            flag2 = true;
                                            continue;
                                        }
                                    }
                                    this.ForceDisablePrediction = true;
                                    flag2 = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (this.IsClientPredicted != flag2)
                {
                    this.IsClientPredicted = flag2;
                    this.UpdateCharacterPhysics(false);
                }
            }
        }

        private void UpdateRespawnAndLooting()
        {
            if (this.m_currentRespawnCounter > 0f)
            {
                MyPlayer player = this.TryGetPlayer();
                if ((player != null) && !MySessionComponentMissionTriggers.CanRespawn(player.Id))
                {
                    if (this.m_respawnNotification != null)
                    {
                        this.m_respawnNotification.m_lifespanMs = 0;
                    }
                    this.m_currentRespawnCounter = -1f;
                }
                this.m_currentRespawnCounter -= 0.01666667f;
                if (this.m_respawnNotification != null)
                {
                    object[] arguments = new object[] { (int) this.m_currentRespawnCounter };
                    this.m_respawnNotification.SetTextFormatArguments(arguments);
                }
                if (((this.m_currentRespawnCounter <= 0f) && Sync.IsServer) && (player != null))
                {
                    Sync.Players.KillPlayer(player);
                }
            }
            this.UpdateLooting(0.01666667f);
        }

        private void UpdateShadowIgnoredObjects()
        {
            if (this.Render != null)
            {
                this.Render.UpdateShadowIgnoredObjects();
                if (this.m_currentWeapon != null)
                {
                    this.UpdateShadowIgnoredObjects((VRage.Game.Entity.MyEntity) this.m_currentWeapon);
                }
                if (this.m_leftHandItem != null)
                {
                    this.UpdateShadowIgnoredObjects(this.m_leftHandItem);
                }
            }
        }

        private void UpdateShadowIgnoredObjects(VRage.ModAPI.IMyEntity parent)
        {
            this.Render.UpdateShadowIgnoredObjects(parent);
            foreach (MyHierarchyComponentBase base2 in parent.Hierarchy.Children)
            {
                this.UpdateShadowIgnoredObjects(base2.Container.Entity);
            }
        }

        private void UpdateShake()
        {
            if ((MySession.Static.LocalHumanPlayer != null) && ReferenceEquals(this, MySession.Static.LocalHumanPlayer.Identity.Character))
            {
                if (((this.m_currentMovementState == MyCharacterMovementEnum.Standing) || (this.m_currentMovementState == MyCharacterMovementEnum.Crouching)) || (this.m_currentMovementState == MyCharacterMovementEnum.Flying))
                {
                    this.m_currentHeadAnimationCounter += 0.01666667f;
                }
                else
                {
                    this.m_currentHeadAnimationCounter = 0f;
                }
                if (this.m_currentLocalHeadAnimation >= 0f)
                {
                    this.m_currentLocalHeadAnimation += 0.01666667f;
                    float amount = this.m_currentLocalHeadAnimation / this.m_localHeadAnimationLength;
                    if (this.m_currentLocalHeadAnimation > this.m_localHeadAnimationLength)
                    {
                        this.m_currentLocalHeadAnimation = -1f;
                        amount = 1f;
                    }
                    if (this.m_localHeadAnimationX != null)
                    {
                        this.SetHeadLocalXAngle(MathHelper.Lerp(this.m_localHeadAnimationX.Value.X, this.m_localHeadAnimationX.Value.Y, amount));
                    }
                    if (this.m_localHeadAnimationY != null)
                    {
                        this.SetHeadLocalYAngle(MathHelper.Lerp(this.m_localHeadAnimationY.Value.X, this.m_localHeadAnimationY.Value.Y, amount));
                    }
                }
            }
        }

        public void UpdateShootDirection(Vector3 direction, int multiplayerUpdateInterval)
        {
            if ((this.ControllerInfo != null) && this.ControllerInfo.IsLocallyControlled())
            {
                MatrixD xd = this.GetHeadMatrix(false, !this.JetpackRunning, false, false, false);
                if (direction.Dot((Vector3) xd.Forward) < 0.996795f)
                {
                    direction = (Vector3) xd.Forward;
                }
                if ((multiplayerUpdateInterval != 0) && ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastShootDirectionUpdate) > multiplayerUpdateInterval))
                {
                    EndpointId targetEndpoint = new EndpointId();
                    MyMultiplayer.RaiseEvent<MyCharacter, Vector3>(this, x => new Action<Vector3>(x.ShootDirectionChangeCallback), direction, targetEndpoint);
                    this.m_lastShootDirectionUpdate = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                }
                this.ShootDirection = direction;
            }
        }

        private void UpdateShooting()
        {
            if (this.m_currentWeapon != null)
            {
                if ((ReferenceEquals(MySession.Static.LocalCharacter, this) && (!(MyScreenManager.GetScreenWithFocus() is MyGuiScreenGamePlay) && MyScreenManager.IsAnyScreenOpening())) && (MyInput.Static.IsGameControlPressed(MyControlsSpace.PRIMARY_TOOL_ACTION) || MyInput.Static.IsGameControlPressed(MyControlsSpace.SECONDARY_TOOL_ACTION)))
                {
                    this.EndShootAll();
                }
                if (this.m_currentWeapon.IsShooting)
                {
                    this.m_currentShootPositionTime = 0.1f;
                }
                this.ShootInternal();
            }
            else if (this.m_usingByPrimary)
            {
                Sandbox.Game.Entities.IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
                if (!MyControllerHelper.IsControl((controlledEntity != null) ? controlledEntity.ControlContext : MySpaceBindingCreator.CX_BASE, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.PRESSED, false))
                {
                    this.m_usingByPrimary = false;
                }
                this.UseContinues();
            }
            if (this.m_currentShotTime > 0f)
            {
                this.m_currentShotTime -= 0.01666667f;
                if (this.m_currentShotTime <= 0f)
                {
                    this.m_currentShotTime = 0f;
                }
            }
            if (this.m_currentShootPositionTime > 0f)
            {
                this.m_currentShootPositionTime -= 0.01666667f;
                if (this.m_currentShootPositionTime <= 0f)
                {
                    this.m_currentShootPositionTime = 0f;
                }
            }
        }

        private void UpdateStandup(ref Vector3 gravity, ref Vector3 chUp, ref Vector3 chForward)
        {
            Vector3 vector = -Vector3.Normalize(gravity);
            Vector3 vector2 = vector;
            if (this.Physics != null)
            {
                Vector3 supportNormal = this.Physics.SupportNormal;
                if (this.Definition.RotationToSupport != MyEnumCharacterRotationToSupport.OneAxis)
                {
                    if (this.Definition.RotationToSupport == MyEnumCharacterRotationToSupport.Full)
                    {
                        vector2 = supportNormal;
                    }
                }
                else
                {
                    float num3 = vector.Dot(ref supportNormal);
                    if (!MyUtils.IsZero((float) (num3 - 1f), 1E-05f) && !MyUtils.IsZero((float) (num3 + 1f), 1E-05f))
                    {
                        Vector3 vector4 = vector.Cross(supportNormal);
                        vector4.Normalize();
                        vector2 = Vector3.Lerp(supportNormal, vector, Math.Abs(vector4.Dot((Vector3) base.WorldMatrix.Forward)));
                    }
                }
            }
            float f = Vector3.Dot(chUp, vector2) / (chUp.Length() * vector2.Length());
            if ((float.IsNaN(f) || float.IsNegativeInfinity(f)) || float.IsPositiveInfinity(f))
            {
                f = 1f;
            }
            f = MathHelper.Clamp(f, -1f, 1f);
            if (!MyUtils.IsZero((float) (f - 1f), 1E-08f))
            {
                float num4 = 0f;
                num4 = !MyUtils.IsZero((float) (f + 1f), 1E-08f) ? ((float) Math.Acos((double) f)) : 0.1f;
                num4 = Math.Min(Math.Abs(num4), 0.04f) * Math.Sign(num4);
                Vector3 vector5 = Vector3.Cross(chUp, vector2);
                if (vector5.LengthSquared() > 0f)
                {
                    vector5 = Vector3.Normalize(vector5);
                    chUp = Vector3.TransformNormal(chUp, Matrix.CreateFromAxisAngle(vector5, num4));
                    chForward = Vector3.TransformNormal(chForward, Matrix.CreateFromAxisAngle(vector5, num4));
                }
            }
        }

        public void UpdateStoredGas(MyDefinitionId gasId, float fillLevel)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCharacter, SerializableDefinitionId, float>(this, x => new Action<SerializableDefinitionId, float>(x.UpdateStoredGas_Implementation), (SerializableDefinitionId) gasId, fillLevel, targetEndpoint);
        }

        [Event(null, 0x229e), Reliable, Broadcast]
        private void UpdateStoredGas_Implementation(SerializableDefinitionId gasId, float fillLevel)
        {
            if (this.OxygenComponent != null)
            {
                MyDefinitionId id = gasId;
                this.OxygenComponent.UpdateStoredGasLevel(ref id, fillLevel);
            }
        }

        public void UpdateZeroMovement()
        {
            if (this.ControllerInfo.IsLocallyControlled() && !this.m_moveAndRotateCalled)
            {
                this.MoveAndRotate(Vector3.Zero, Vector2.Zero, 0f);
            }
        }

        public void Use()
        {
            if (this.IsOnLadder)
            {
                if (this.GetCurrentMovementState() != MyCharacterMovementEnum.LadderOut)
                {
                    Vector3D pos = base.PositionComp.GetPosition() + (this.m_ladder.WorldMatrix.Forward * 1.2000000476837158);
                    this.GetOffLadder();
                    base.PositionComp.SetPosition(pos, null, false, true);
                }
            }
            else if (!this.IsDead)
            {
                MyCharacterDetectorComponent component = base.Components.Get<MyCharacterDetectorComponent>();
                if ((component == null) || (component.UseObject == null))
                {
                    VRage.Game.Entity.MyEntity detectedEntity = component.DetectedEntity as VRage.Game.Entity.MyEntity;
                    if ((detectedEntity != null) && (!(detectedEntity is MyCharacter) || (detectedEntity as MyCharacter).IsDead))
                    {
                        MyInventoryBase inventoryBase = null;
                        if (detectedEntity.TryGetInventory(out inventoryBase))
                        {
                            this.ShowAggregateInventoryScreen(inventoryBase);
                        }
                    }
                }
                else if (component.UseObject.PrimaryAction != UseActionEnum.None)
                {
                    if (component.UseObject.PlayIndicatorSound)
                    {
                        MyGuiAudio.PlaySound(MyGuiSounds.HudUse);
                        this.SoundComp.StopStateSound(true);
                    }
                    component.RaiseObjectUsed();
                    component.UseObject.Use(component.UseObject.PrimaryAction, this);
                }
                else if (component.UseObject.SecondaryAction != UseActionEnum.None)
                {
                    if (component.UseObject.PlayIndicatorSound)
                    {
                        MyGuiAudio.PlaySound(MyGuiSounds.HudUse);
                        this.SoundComp.StopStateSound(true);
                    }
                    component.RaiseObjectUsed();
                    component.UseObject.Use(component.UseObject.SecondaryAction, this);
                }
            }
        }

        public void UseContinues()
        {
            if (!this.IsDead)
            {
                MyCharacterDetectorComponent component = base.Components.Get<MyCharacterDetectorComponent>();
                if (((component != null) && ((component.UseObject != null) && component.UseObject.IsActionSupported(UseActionEnum.Manipulate))) && component.UseObject.ContinuousUsage)
                {
                    component.UseObject.Use(UseActionEnum.Manipulate, this);
                }
            }
        }

        public void UseFinished()
        {
            if (!this.IsDead)
            {
                MyCharacterDetectorComponent component = base.Components.Get<MyCharacterDetectorComponent>();
                if ((component.UseObject != null) && component.UseObject.IsActionSupported(UseActionEnum.UseFinished))
                {
                    component.UseObject.Use(UseActionEnum.UseFinished, this);
                }
            }
        }

        public void UseTerminal()
        {
            if (!this.IsDead)
            {
                MyCharacterDetectorComponent component = base.Components.Get<MyCharacterDetectorComponent>();
                if ((component.UseObject != null) && component.UseObject.IsActionSupported(UseActionEnum.OpenTerminal))
                {
                    if (component.UseObject.PlayIndicatorSound)
                    {
                        MyGuiAudio.PlaySound(MyGuiSounds.HudUse);
                        this.SoundComp.StopStateSound(true);
                    }
                    component.UseObject.Use(UseActionEnum.OpenTerminal, this);
                    component.UseContinues();
                }
            }
        }

        private void ValidateBonesProperties()
        {
            if ((this.m_rightHandItemBone == -1) && (this.m_currentWeapon != null))
            {
                this.DisposeWeapon();
            }
        }

        bool IMyComponentOwner<MyIDModule>.GetComponent(out MyIDModule module)
        {
            module = this.m_idModule;
            return true;
        }

        MyActionDescription IMyUseObject.GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description = new MyActionDescription {
                Text = MySpaceTexts.NotificationHintPressToOpenInventory
            };
            description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.INVENTORY) + "]", base.DisplayName };
            description.IsTextControlHint = true;
            description.JoystickText = new MyStringId?(MyCommonTexts.NotificationHintJoystickPressToOpenInventory);
            description.JoystickFormatParams = new object[] { base.DisplayName };
            return description;
        }

        bool IMyUseObject.HandleInput()
        {
            MyCharacterDetectorComponent component = base.Components.Get<MyCharacterDetectorComponent>();
            return ((component != null) && ((component.UseObject != null) && component.UseObject.HandleInput()));
        }

        void IMyUseObject.OnSelectionLost()
        {
        }

        void IMyUseObject.SetInstanceID(int id)
        {
        }

        void IMyUseObject.SetRenderID(uint id)
        {
        }

        void IMyUseObject.Use(UseActionEnum actionEnum, VRage.ModAPI.IMyEntity entity)
        {
            MyCharacter user = entity as MyCharacter;
            if (MyPerGameSettings.TerminalEnabled)
            {
                MyGuiScreenTerminal.Show(MyTerminalPageEnum.Inventory, user, this);
            }
            if ((MyPerGameSettings.GUI.InventoryScreen != null) && this.IsDead)
            {
                MyInventoryAggregate rightSelectedInventory = base.Components.Get<MyInventoryAggregate>();
                if (rightSelectedInventory != null)
                {
                    user.ShowAggregateInventoryScreen(rightSelectedInventory);
                }
            }
        }

        void IMyCharacter.Kill(object statChangeData)
        {
            MyDamageInformation damageInfo = new MyDamageInformation();
            if (statChangeData != null)
            {
                damageInfo = (MyDamageInformation) statChangeData;
            }
            this.Kill(true, damageInfo);
        }

        void IMyCharacter.TriggerCharacterAnimationEvent(string eventName, bool sync)
        {
            this.TriggerCharacterAnimationEvent(eventName, sync);
        }

        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        void IMyCameraController.ControlCamera(MyCamera currentCamera)
        {
            MatrixD viewMatrix = this.GetViewMatrix();
            currentCamera.SetViewMatrix(viewMatrix);
            currentCamera.CameraSpring.Enabled = !this.IsInFirstPersonView && !this.ForceFirstPersonCamera;
            this.EnableHead(!this.ControllerInfo.IsLocallyControlled() || (!this.IsInFirstPersonView && !this.ForceFirstPersonCamera));
        }

        bool IMyCameraController.HandlePickUp() => 
            false;

        bool IMyCameraController.HandleUse() => 
            false;

        void IMyCameraController.OnAssumeControl(IMyCameraController previousCameraController)
        {
            this.OnAssumeControl(previousCameraController);
        }

        void IMyCameraController.OnReleaseControl(IMyCameraController newCameraController)
        {
            this.OnReleaseControl(newCameraController);
            if (base.InScene)
            {
                this.EnableHead(true);
            }
        }

        void IMyCameraController.Rotate(Vector2 rotationIndicator, float rollIndicator)
        {
            this.Rotate(rotationIndicator, rollIndicator);
        }

        void IMyCameraController.RotateStopped()
        {
            this.RotateStopped();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Crouch()
        {
            this.Crouch();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Die()
        {
            this.Die();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Down()
        {
            this.Down();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.DrawHud(IMyCameraController camera, long playerId)
        {
            if (camera != null)
            {
                this.DrawHud(camera, playerId);
            }
        }

        MatrixD VRage.Game.ModAPI.Interfaces.IMyControllableEntity.GetHeadMatrix(bool includeY, bool includeX, bool forceHeadAnim, bool forceHeadBone) => 
            this.GetHeadMatrix(includeY, includeX, forceHeadAnim, false, false);

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Jump(Vector3 moveIndicator)
        {
            this.Jump(moveIndicator);
            if (!Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCharacter, Vector3>(this, x => new Action<Vector3>(x.Jump), moveIndicator, targetEndpoint);
            }
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.MoveAndRotate(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
            this.MoveAndRotate(moveIndicator, rotationIndicator, rollIndicator);
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.MoveAndRotateStopped()
        {
            this.MoveAndRotateStopped();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.PickUp()
        {
            this.PickUp();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.PickUpContinues()
        {
            this.PickUpContinues();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ShowInventory()
        {
            this.ShowInventory();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ShowTerminal()
        {
            this.ShowTerminal();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchDamping()
        {
            MyCharacterJetpackComponent jetpackComp = this.JetpackComp;
            if (jetpackComp != null)
            {
                jetpackComp.SwitchDamping();
                if (!jetpackComp.DampenersEnabled)
                {
                    this.RelativeDampeningEntity = null;
                }
            }
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchHelmet()
        {
            if (Sync.IsServer || ReferenceEquals(MySession.Static.LocalCharacter, this))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCharacter>(this, x => new Action(x.OnSwitchHelmet), targetEndpoint);
            }
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchLandingGears()
        {
            this.SwitchLandingGears();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchLights()
        {
            this.SwitchLights();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchReactors()
        {
            this.SwitchReactors();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchThrusts()
        {
            MyCharacterJetpackComponent jetpackComp = this.JetpackComp;
            if ((jetpackComp != null) && this.HasEnoughSpaceToStandUp())
            {
                jetpackComp.SwitchThrusts();
            }
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Up()
        {
            this.Up();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Use()
        {
            this.Use();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.UseContinues()
        {
            this.UseContinues();
        }

        void IMyDecalProxy.AddDecals(ref MyHitInfo hitInfo, MyStringHash source, object customdata, IMyDecalHandler decalHandler, MyStringHash material)
        {
            MyCharacterHitInfo info = customdata as MyCharacterHitInfo;
            if ((info != null) && (info.BoneIndex != -1))
            {
                MyDecalRenderInfo renderInfo = new MyDecalRenderInfo {
                    Position = info.Triangle.IntersectionPointInObjectSpace,
                    Normal = info.Triangle.NormalInObjectSpace,
                    RenderObjectIds = this.Render.RenderObjectIDs,
                    Source = source,
                    Material = (material.GetHashCode() != 0) ? material : MyStringHash.GetOrCompute(this.m_characterDefinition.PhysicalMaterial)
                };
                VertexBoneIndicesWeights? affectingBoneIndicesWeights = info.Triangle.GetAffectingBoneIndicesWeights(ref m_boneIndexWeightTmp);
                renderInfo.BoneIndices = affectingBoneIndicesWeights.Value.Indices;
                renderInfo.BoneWeights = affectingBoneIndicesWeights.Value.Weights;
                MyDecalBindingInfo info3 = new MyDecalBindingInfo {
                    Position = info.HitPositionBindingPose,
                    Normal = info.HitNormalBindingPose,
                    Transformation = info.BindingTransformation
                };
                renderInfo.Binding = new MyDecalBindingInfo?(info3);
                m_tmpIds.Clear();
                decalHandler.AddDecal(ref renderInfo, m_tmpIds);
                foreach (uint num in m_tmpIds)
                {
                    base.AddBoneDecal(num, info.BoneIndex);
                }
            }
        }

        bool IMyDestroyableObject.DoDamage(float damage, MyStringHash damageType, bool sync, MyHitInfo? hitInfo, long attackerId) => 
            this.DoDamage(damage, damageType, sync, attackerId);

        void IMyDestroyableObject.OnDestroy()
        {
            this.OnDestroy();
        }

        public bool WeaponTakesBuilderFromInventory(MyDefinitionId? weaponDefinition)
        {
            if (weaponDefinition == null)
            {
                return false;
            }
            if ((weaponDefinition.Value.TypeId == typeof(MyObjectBuilder_CubePlacer)) || ((weaponDefinition.Value.TypeId == typeof(MyObjectBuilder_PhysicalGunObject)) && (weaponDefinition.Value.SubtypeId == this.manipulationToolId)))
            {
                return false;
            }
            return (!MySession.Static.CreativeMode && !MyFakes.ENABLE_SURVIVAL_SWITCHING);
        }

        private void WorldPositionChanged(object source)
        {
            if (this.RadioBroadcaster != null)
            {
                this.RadioBroadcaster.MoveBroadcaster();
            }
            this.Render.UpdateLightPosition();
        }

        public void Zoom(bool newKeyPress, bool hideCrosshairWhenAiming = true)
        {
            bool? nullable;
            MyZoomModeEnum zoomMode = this.m_zoomMode;
            if (zoomMode == MyZoomModeEnum.Classic)
            {
                if ((this.Definition.CanIronsight && (this.m_currentWeapon != null)) && (ReferenceEquals(MySession.Static.CameraController, this) || !this.ControllerInfo.IsLocallyControlled()))
                {
                    if (!this.IsInFirstPersonView)
                    {
                        MyGuiScreenGamePlay.Static.SwitchCamera();
                        this.m_wasInThirdPersonBeforeIronSight = true;
                    }
                    nullable = null;
                    this.SoundComp.PlaySecondarySound(CharacterSoundsEnum.IRONSIGHT_ACT_SOUND, true, false, nullable);
                    this.EnableIronsight(true, newKeyPress, true, hideCrosshairWhenAiming);
                }
            }
            else if ((zoomMode == MyZoomModeEnum.IronSight) && (ReferenceEquals(MySession.Static.CameraController, this) || !this.ControllerInfo.IsLocallyControlled()))
            {
                nullable = null;
                this.SoundComp.PlaySecondarySound(CharacterSoundsEnum.IRONSIGHT_DEACT_SOUND, true, false, nullable);
                this.EnableIronsight(false, newKeyPress, true, true);
                if (this.m_wasInThirdPersonBeforeIronSight)
                {
                    MyGuiScreenGamePlay.Static.SwitchCamera();
                }
            }
        }

        public bool IsOnLadder =>
            (this.m_ladder != null);

        public MyLadder Ladder =>
            this.m_ladder;

        VRage.ModAPI.IMyEntity VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Entity =>
            this.Entity;

        IMyControllerInfo VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ControllerInfo =>
            this.ControllerInfo;

        int IMyInventoryOwner.InventoryCount =>
            base.InventoryCount;

        long IMyInventoryOwner.EntityId =>
            base.EntityId;

        bool IMyInventoryOwner.HasInventory =>
            base.HasInventory;

        bool IMyInventoryOwner.UseConveyorSystem
        {
            get => 
                false;
            set
            {
                throw new NotImplementedException();
            }
        }

        internal bool CanJump
        {
            get => 
                this.m_canJump;
            set => 
                (this.m_canJump = value);
        }

        internal float CurrentWalkDelay
        {
            get => 
                this.m_currentWalkDelay;
            set => 
                (this.m_currentWalkDelay = value);
        }

        public Vector3 Gravity =>
            this.m_gravity;

        public int WeaponBone =>
            this.m_weaponBone;

        private IMyHandheldGunObject<MyDeviceBase> m_currentWeapon { get; set; }

        public bool IsClientPredicted { get; private set; }

        public bool ForceDisablePrediction
        {
            get => 
                this.m_forceDisablePrediction;
            set
            {
                this.m_forceDisablePrediction = value;
                this.m_forceDisablePredictionTime = MySandboxGame.Static.SimulationTime.Seconds;
            }
        }

        public bool AlwaysDisablePrediction { get; set; }

        public bool HeadRenderingEnabled =>
            this.m_headRenderingEnabled;

        public float HeadLocalXAngle
        {
            get => 
                (this.m_headLocalXAngle.IsValid() ? this.m_headLocalXAngle : 0f);
            set
            {
                this.m_previousHeadLocalXAngle = this.m_headLocalXAngle;
                this.m_headLocalXAngle = value.IsValid() ? MathHelper.Clamp(value, -89.9f, 89f) : 0f;
            }
        }

        public float HeadLocalYAngle
        {
            get => 
                this.m_headLocalYAngle;
            set
            {
                this.m_previousHeadLocalYAngle = this.m_headLocalYAngle;
                if (!this.IsOnLadder || !this.IsInFirstPersonView)
                {
                    this.m_headLocalYAngle = value;
                }
                else
                {
                    this.m_headLocalYAngle = MathHelper.Clamp(value, -89.9f, 89f);
                }
            }
        }

        public bool HeadMoved =>
            ((this.m_previousHeadLocalXAngle != this.m_headLocalXAngle) || !(this.m_previousHeadLocalYAngle == this.m_headLocalYAngle));

        public MyCharacterMovementEnum CurrentMovementState
        {
            get => 
                this.m_currentMovementState;
            set => 
                this.SetCurrentMovementState(value);
        }

        public MyCharacterMovementEnum PreviousMovementState =>
            this.m_previousMovementState;

        public MyHandItemDefinition HandItemDefinition =>
            this.m_handItemDefinition;

        public MyZoomModeEnum ZoomMode =>
            this.m_zoomMode;

        public bool ShouldSupressShootAnimation =>
            ((this.m_currentWeapon != null) ? this.m_currentWeapon.SupressShootAnimation() : false);

        public HkCharacterStateType CharacterGroundState =>
            this.m_currentCharacterState;

        public bool JetpackRunning =>
            ((this.JetpackComp != null) && this.JetpackComp.Running);

        internal MyResourceDistributorComponent SuitRechargeDistributor
        {
            get => 
                this.m_suitResourceDistributor;
            set
            {
                if (base.Components.Contains(typeof(MyResourceDistributorComponent)))
                {
                    base.Components.Remove<MyResourceDistributorComponent>();
                }
                base.Components.Add<MyResourceDistributorComponent>(value);
                this.m_suitResourceDistributor = value;
            }
        }

        public MyResourceSinkComponent SinkComp
        {
            get => 
                this.m_sinkComp;
            set => 
                (this.m_sinkComp = value);
        }

        public bool EnabledBag =>
            this.m_enableBag;

        public VRage.Sync.SyncType SyncType { get; set; }

        public float CurrentLightPower =>
            this.m_currentLightPower;

        public float CurrentRespawnCounter =>
            this.m_currentRespawnCounter;

        internal MyRadioReceiver RadioReceiver
        {
            get => 
                ((MyRadioReceiver) base.Components.Get<MyDataReceiver>());
            private set => 
                base.Components.Add<MyDataReceiver>(value);
        }

        internal MyRadioBroadcaster RadioBroadcaster
        {
            get => 
                ((MyRadioBroadcaster) base.Components.Get<MyDataBroadcaster>());
            private set => 
                base.Components.Add<MyDataBroadcaster>(value);
        }

        public StringBuilder CustomNameWithFaction { get; private set; }

        internal MyRenderComponentCharacter Render
        {
            get => 
                (base.Render as MyRenderComponentCharacter);
            set => 
                (base.Render = value);
        }

        public MyCharacterSoundComponent SoundComp
        {
            get => 
                base.Components.Get<MyCharacterSoundComponent>();
            set
            {
                if (base.Components.Has<MyCharacterSoundComponent>())
                {
                    base.Components.Remove<MyCharacterSoundComponent>();
                }
                base.Components.Add<MyCharacterSoundComponent>(value);
            }
        }

        public MyAtmosphereDetectorComponent AtmosphereDetectorComp
        {
            get => 
                base.Components.Get<MyAtmosphereDetectorComponent>();
            set
            {
                if (base.Components.Has<MyAtmosphereDetectorComponent>())
                {
                    base.Components.Remove<MyAtmosphereDetectorComponent>();
                }
                base.Components.Add<MyAtmosphereDetectorComponent>(value);
            }
        }

        public MyEntityReverbDetectorComponent ReverbDetectorComp
        {
            get => 
                base.Components.Get<MyEntityReverbDetectorComponent>();
            set
            {
                if (base.Components.Has<MyEntityReverbDetectorComponent>())
                {
                    base.Components.Remove<MyEntityReverbDetectorComponent>();
                }
                base.Components.Add<MyEntityReverbDetectorComponent>(value);
            }
        }

        public MyCharacterStatComponent StatComp
        {
            get => 
                (base.Components.Get<MyEntityStatComponent>() as MyCharacterStatComponent);
            set
            {
                if (base.Components.Has<MyEntityStatComponent>())
                {
                    base.Components.Remove<MyEntityStatComponent>();
                }
                base.Components.Add<MyEntityStatComponent>(value);
            }
        }

        public MyCharacterJetpackComponent JetpackComp
        {
            get => 
                base.Components.Get<MyCharacterJetpackComponent>();
            set
            {
                if (base.Components.Has<MyCharacterJetpackComponent>())
                {
                    base.Components.Remove<MyCharacterJetpackComponent>();
                }
                base.Components.Add<MyCharacterJetpackComponent>(value);
            }
        }

        float IMyCharacter.BaseMass =>
            this.BaseMass;

        float IMyCharacter.CurrentMass =>
            this.CurrentMass;

        public float BaseMass =>
            this.Definition.Mass;

        public float CurrentMass
        {
            get
            {
                float mass = 0f;
                if ((this.ManipulatedEntity != null) && (this.ManipulatedEntity.Physics != null))
                {
                    mass = this.ManipulatedEntity.Physics.Mass;
                }
                return ((this.GetInventory(0) == null) ? (this.BaseMass + mass) : ((this.BaseMass + ((float) this.GetInventory(0).CurrentMass)) + mass));
            }
        }

        public MyCharacterDefinition Definition =>
            this.m_characterDefinition;

        MyDefinitionBase IMyCharacter.Definition =>
            this.m_characterDefinition;

        public bool IsInFirstPersonView
        {
            get => 
                this.m_isInFirstPersonView;
            set
            {
                if (!value && !MySession.Static.Settings.Enable3rdPersonView)
                {
                    this.m_isInFirstPersonView = true;
                }
                else if (!this.Definition.EnableFirstPersonView)
                {
                    this.m_isInFirstPersonView = false;
                }
                else
                {
                    this.m_isInFirstPersonView = value;
                    this.ResetHeadRotation();
                    if (!this.m_isInFirstPersonView && (this.m_zoomMode == MyZoomModeEnum.IronSight))
                    {
                        this.EnableIronsight(false, false, true, true);
                    }
                    this.SwitchCameraIronSightChanges();
                }
            }
        }

        public bool EnableFirstPersonView
        {
            get => 
                this.Definition.EnableFirstPersonView;
            set
            {
            }
        }

        public bool TargetFromCamera
        {
            get => 
                (!ReferenceEquals(MySession.Static.ControlledEntity, this) ? (!Sandbox.Engine.Platform.Game.IsDedicated ? this.m_targetFromCamera : false) : (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.ThirdPersonSpectator));
            set => 
                (this.m_targetFromCamera = value);
        }

        public MyToolbar Toolbar =>
            MyToolbarComponent.CharacterToolbar;

        public bool ForceFirstPersonCamera
        {
            get => 
                (!this.IsDead && (this.m_forceFirstPersonCamera || MyThirdPersonSpectator.Static.IsCameraForced()));
            set => 
                (this.m_forceFirstPersonCamera = !this.IsDead & value);
        }

        public bool IsCameraNear =>
            (!MyFakes.ENABLE_PERMANENT_SIMULATIONS_COMPUTATION ? (this.Render.IsVisible() && (this.m_cameraDistance <= 60f)) : true);

        public MyInventoryAggregate InventoryAggregate
        {
            get => 
                (base.Components.Get<MyInventoryBase>() as MyInventoryAggregate);
            set
            {
                if (base.Components.Has<MyInventoryBase>())
                {
                    base.Components.Remove<MyInventoryBase>();
                }
                base.Components.Add<MyInventoryBase>(value);
            }
        }

        public MyCharacterOxygenComponent OxygenComponent { get; private set; }

        public MyCharacterWeaponPositionComponent WeaponPosition { get; private set; }

        public Vector3 MoveIndicator { get; set; }

        public Vector2 RotationIndicator { get; set; }

        public bool IsRotating { get; set; }

        public float RollIndicator { get; set; }

        public Vector3 RotationCenterIndicator { get; set; }

        public ulong ControlSteamId =>
            ((this.m_controlInfo != null) ? this.m_controlInfo.Value.SteamId : 0UL);

        public MyPromoteLevel PromoteLevel
        {
            get
            {
                MyPlayer.PlayerId id = this.m_controlInfo.Value;
                return MySession.Static.GetUserPromoteLevel(id.SteamId);
            }
        }

        public long ClosestParentId
        {
            get => 
                this.m_closestParentId;
            set
            {
                if ((this.m_closestParentId != value) || !MyGridPhysicalHierarchy.Static.NonGridLinkExists(value, this))
                {
                    MyCubeGrid grid;
                    if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(this.m_closestParentId, out grid, true))
                    {
                        MyGridPhysicalHierarchy.Static.RemoveNonGridNode(grid, this);
                    }
                    if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(value, out grid, false))
                    {
                        this.m_closestParentId = 0L;
                    }
                    else
                    {
                        this.m_closestParentId = value;
                        MyGridPhysicalHierarchy.Static.AddNonGridNode(grid, this);
                    }
                }
            }
        }

        public bool IsPersistenceCharacter { get; set; }

        public MyPlayer.PlayerId? SavedPlayer =>
            this.m_savedPlayer;

        public bool InheritRotation =>
            (!this.JetpackRunning && (!this.IsFalling && !this.IsJumping));

        public Vector3D AimedPoint
        {
            get => 
                this.m_aimedPoint;
            set => 
                (this.m_aimedPoint = value);
        }

        public bool IsIdle =>
            ((this.m_currentMovementState == MyCharacterMovementEnum.Standing) || (this.m_currentMovementState == MyCharacterMovementEnum.Crouching));

        internal float HeadMovementXOffset =>
            this.m_headMovementXOffset;

        internal float HeadMovementYOffset =>
            this.m_headMovementYOffset;

        public VRage.Game.Entity.MyEntity IsUsing
        {
            get => 
                this.m_usingEntity;
            set => 
                (this.m_usingEntity = value);
        }

        public float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public bool LightEnabled =>
            this.m_lightEnabled;

        public bool IsCrouching =>
            (this.m_currentMovementState.GetMode() == 2);

        public bool IsSprinting =>
            (this.m_currentMovementState == MyCharacterMovementEnum.Sprinting);

        public bool IsFalling
        {
            get
            {
                MyCharacterMovementEnum currentMovementState = this.GetCurrentMovementState();
                return (this.m_isFalling && (currentMovementState != MyCharacterMovementEnum.Flying));
            }
        }

        public bool IsJumping =>
            (this.m_currentMovementState == MyCharacterMovementEnum.Jump);

        public bool IsMagneticBootsEnabled
        {
            get
            {
                if ((this.IsJumping || (this.IsOnLadder || (this.IsFalling || (this.Physics == null)))) || (this.Physics.CharacterProxy == null))
                {
                    return false;
                }
                return ((this.Physics.CharacterProxy.Gravity.LengthSquared() < 0.001f) && !this.JetpackRunning);
            }
        }

        public bool IsMagneticBootsActive =>
            (this.IsMagneticBootsEnabled && (this.m_bootsState == 3));

        bool IMyCharacter.IsDead =>
            this.IsDead;

        public long DeadPlayerIdentityId =>
            this.m_deadPlayerIdentityId;

        public Vector3 ColorMask
        {
            get => 
                base.Render.ColorMaskHsv;
            set => 
                this.ChangeModelAndColor(this.ModelName, value, false, 0L);
        }

        public string ModelName
        {
            get => 
                this.m_characterModel;
            set => 
                this.ChangeModelAndColor(value, this.ColorMask, false, 0L);
        }

        public IMyGunObject<MyDeviceBase> CurrentWeapon =>
            this.m_currentWeapon;

        public IMyHandheldGunObject<MyDeviceBase> LeftHandItem =>
            (this.m_leftHandItem as IMyHandheldGunObject<MyDeviceBase>);

        internal Sandbox.Game.Entities.IMyControllableEntity CurrentRemoteControl { get; set; }

        public MyBattery SuitBattery =>
            this.m_suitBattery;

        public override string DisplayNameText =>
            base.DisplayName;

        public static bool CharactersCanDie =>
            (!MySession.Static.CreativeMode || MyFakes.CHARACTER_CAN_DIE_EVEN_IN_CREATIVE_MODE);

        public bool CharacterCanDie =>
            (CharactersCanDie || ((this.ControllerInfo.Controller != null) && (this.ControllerInfo.Controller.Player.Id.SerialId != 0)));

        public override Vector3D LocationForHudMarker =>
            (base.LocationForHudMarker + (base.WorldMatrix.Up * 2.1));

        public MyPhysicsBody Physics
        {
            get => 
                (base.Physics as MyPhysicsBody);
            set => 
                (base.Physics = value);
        }

        public VRage.Game.Entity.MyEntity Entity =>
            this;

        public MyControllerInfo ControllerInfo =>
            this.m_info;

        public bool IsDead =>
            (this.m_currentMovementState == MyCharacterMovementEnum.Died);

        public bool IsSitting =>
            (this.m_currentMovementState == MyCharacterMovementEnum.Sitting);

        public float CurrentJump =>
            this.m_currentJumpTime;

        public MyToolbarType ToolbarType =>
            MyToolbarType.Character;

        VRage.ModAPI.IMyEntity IMyUseObject.Owner =>
            this;

        MyModelDummy IMyUseObject.Dummy =>
            null;

        float IMyUseObject.InteractiveDistance =>
            5f;

        MatrixD IMyUseObject.ActivationMatrix
        {
            get
            {
                if (base.PositionComp == null)
                {
                    return MatrixD.Zero;
                }
                if ((!this.IsDead || (this.Physics == null)) || (this.Definition.DeadBodyShape == null))
                {
                    float num2 = 0.75f;
                    Matrix matrix2 = (Matrix) base.WorldMatrix;
                    Matrix* matrixPtr7 = (Matrix*) ref matrix2;
                    matrixPtr7.Forward *= num2;
                    Matrix* matrixPtr8 = (Matrix*) ref matrix2;
                    matrixPtr8.Up *= this.Definition.CharacterCollisionHeight * num2;
                    Matrix* matrixPtr9 = (Matrix*) ref matrix2;
                    matrixPtr9.Right *= num2;
                    matrix2.Translation = (Vector3) base.PositionComp.WorldAABB.Center;
                    return matrix2;
                }
                float num = 0.8f;
                Matrix worldMatrix = (Matrix) base.WorldMatrix;
                Matrix* matrixPtr1 = (Matrix*) ref worldMatrix;
                matrixPtr1.Forward *= num;
                Matrix* matrixPtr2 = (Matrix*) ref worldMatrix;
                matrixPtr2.Up *= this.Definition.CharacterCollisionHeight * num;
                Matrix* matrixPtr3 = (Matrix*) ref worldMatrix;
                matrixPtr3.Right *= num;
                worldMatrix.Translation = (Vector3) base.PositionComp.WorldAABB.Center;
                Matrix* matrixPtr4 = (Matrix*) ref worldMatrix;
                matrixPtr4.Translation += (0.5f * worldMatrix.Right) * this.Definition.DeadBodyShape.RelativeShapeTranslation.X;
                Matrix* matrixPtr5 = (Matrix*) ref worldMatrix;
                matrixPtr5.Translation += (0.5f * worldMatrix.Up) * this.Definition.DeadBodyShape.RelativeShapeTranslation.Y;
                Matrix* matrixPtr6 = (Matrix*) ref worldMatrix;
                matrixPtr6.Translation += (0.5f * worldMatrix.Forward) * this.Definition.DeadBodyShape.RelativeShapeTranslation.Z;
                return worldMatrix;
            }
        }

        MatrixD IMyUseObject.WorldMatrix =>
            base.WorldMatrix;

        uint IMyUseObject.RenderObjectID =>
            base.Render.GetRenderObjectID();

        int IMyUseObject.InstanceID =>
            -1;

        bool IMyUseObject.ShowOverlay =>
            false;

        UseActionEnum IMyUseObject.SupportedActions
        {
            get
            {
                if (!this.IsDead || this.Definition.EnableSpawnInventoryAsContainer)
                {
                    return UseActionEnum.None;
                }
                return (UseActionEnum.OpenInventory | UseActionEnum.OpenTerminal);
            }
        }

        UseActionEnum IMyUseObject.PrimaryAction
        {
            get
            {
                if (!this.IsDead || this.Definition.EnableSpawnInventoryAsContainer)
                {
                    return UseActionEnum.None;
                }
                return UseActionEnum.OpenInventory;
            }
        }

        UseActionEnum IMyUseObject.SecondaryAction
        {
            get
            {
                if (!this.IsDead || this.Definition.EnableSpawnInventoryAsContainer)
                {
                    return UseActionEnum.None;
                }
                return UseActionEnum.OpenTerminal;
            }
        }

        bool IMyUseObject.ContinuousUsage =>
            false;

        bool IMyUseObject.PlayIndicatorSound =>
            true;

        public bool UseDamageSystem { get; private set; }

        public float Integrity
        {
            get
            {
                float num = 100f;
                if ((this.StatComp != null) && (this.StatComp.Health != null))
                {
                    num = this.StatComp.Health.Value;
                }
                return num;
            }
        }

        bool IMyCameraController.IsInFirstPersonView
        {
            get => 
                this.IsInFirstPersonView;
            set => 
                (this.IsInFirstPersonView = value);
        }

        bool IMyCameraController.ForceFirstPersonCamera
        {
            get => 
                this.ForceFirstPersonCamera;
            set => 
                (this.ForceFirstPersonCamera = value);
        }

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ForceFirstPersonCamera
        {
            get => 
                this.ForceFirstPersonCamera;
            set => 
                (this.ForceFirstPersonCamera = value);
        }

        bool IMyCameraController.AllowCubeBuilding =>
            true;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledThrusts =>
            ((this.JetpackComp != null) && this.JetpackComp.TurnedOn);

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledDamping =>
            ((this.JetpackComp != null) && this.JetpackComp.DampenersTurnedOn);

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledLights =>
            this.LightEnabled;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledLeadingGears =>
            false;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledReactors =>
            false;

        bool Sandbox.Game.Entities.IMyControllableEntity.EnabledBroadcasting =>
            this.RadioBroadcaster.Enabled;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledHelmet =>
            this.OxygenComponent.HelmetEnabled;

        float IMyDestroyableObject.Integrity =>
            this.Integrity;

        public bool PrimaryLookaround =>
            (this.IsOnLadder && !this.IsInFirstPersonView);

        public MyCharacterMovementFlags MovementFlags
        {
            get => 
                this.m_movementFlags;
            internal set => 
                (this.m_movementFlags = value);
        }

        public MyCharacterMovementFlags PreviousMovementFlags =>
            this.m_previousMovementFlags;

        public bool WantsJump
        {
            get => 
                ((this.m_movementFlags & MyCharacterMovementFlags.Jump) == MyCharacterMovementFlags.Jump);
            private set
            {
                if (value)
                {
                    this.m_movementFlags |= MyCharacterMovementFlags.Jump;
                }
                else
                {
                    this.m_movementFlags = ((MyCharacterMovementFlags) ((int) this.m_movementFlags)) & ((MyCharacterMovementFlags) 0xfe);
                }
            }
        }

        private bool WantsSprint
        {
            get => 
                ((this.m_movementFlags & MyCharacterMovementFlags.Sprint) == MyCharacterMovementFlags.Sprint);
            set
            {
                if (value)
                {
                    this.m_movementFlags |= MyCharacterMovementFlags.Sprint;
                }
                else
                {
                    this.m_movementFlags = ((MyCharacterMovementFlags) ((int) this.m_movementFlags)) & ((MyCharacterMovementFlags) 0xfd);
                }
            }
        }

        public bool WantsWalk
        {
            get => 
                ((this.m_movementFlags & MyCharacterMovementFlags.Walk) == MyCharacterMovementFlags.Walk);
            private set
            {
                if (value)
                {
                    this.m_movementFlags |= MyCharacterMovementFlags.Walk;
                }
                else
                {
                    this.m_movementFlags = ((MyCharacterMovementFlags) ((int) this.m_movementFlags)) & ((MyCharacterMovementFlags) 0xdf);
                }
            }
        }

        private bool WantsFlyUp
        {
            get => 
                ((this.m_movementFlags & MyCharacterMovementFlags.FlyUp) == MyCharacterMovementFlags.FlyUp);
            set
            {
                if (value)
                {
                    this.m_movementFlags |= MyCharacterMovementFlags.FlyUp;
                }
                else
                {
                    this.m_movementFlags = ((MyCharacterMovementFlags) ((int) this.m_movementFlags)) & ((MyCharacterMovementFlags) 0xfb);
                }
            }
        }

        private bool WantsFlyDown
        {
            get => 
                ((this.m_movementFlags & MyCharacterMovementFlags.FlyDown) == MyCharacterMovementFlags.FlyDown);
            set
            {
                if (value)
                {
                    this.m_movementFlags |= MyCharacterMovementFlags.FlyDown;
                }
                else
                {
                    this.m_movementFlags = ((MyCharacterMovementFlags) ((int) this.m_movementFlags)) & ((MyCharacterMovementFlags) 0xf7);
                }
            }
        }

        private bool WantsCrouch
        {
            get => 
                ((this.m_movementFlags & MyCharacterMovementFlags.Crouch) == MyCharacterMovementFlags.Crouch);
            set
            {
                if (value)
                {
                    this.m_movementFlags |= MyCharacterMovementFlags.Crouch;
                }
                else
                {
                    this.m_movementFlags = ((MyCharacterMovementFlags) ((int) this.m_movementFlags)) & ((MyCharacterMovementFlags) 0xef);
                }
            }
        }

        public MyCharacterBreath Breath =>
            this.m_breath;

        public float CharacterAccumulatedDamage { get; set; }

        public MyStringId ControlContext =>
            (!MyCubeBuilder.Static.IsBuildMode ? (!MySessionComponentVoxelHand.Static.BuildMode ? MySpaceBindingCreator.CX_CHARACTER : MySpaceBindingCreator.CX_VOXEL) : MySpaceBindingCreator.CX_BUILD_MODE);

        public float EnvironmentOxygenLevel =>
            ((float) this.EnvironmentOxygenLevelSync);

        public float OxygenLevel =>
            ((float) this.OxygenLevelAtCharacterLocation);

        public float SuitEnergyLevel =>
            (this.SuitBattery.ResourceSource.RemainingCapacityByType(MyResourceDistributorComponent.ElectricityId) / 1E-05f);

        public bool IsPlayer =>
            !MySession.Static.Players.IdentityIsNpc(this.GetPlayerIdentityId());

        public bool IsBot =>
            !this.IsPlayer;

        public int SpineBoneIndex =>
            this.m_spineBone;

        public int HeadBoneIndex =>
            this.m_headBoneIndex;

        public VRage.ModAPI.IMyEntity EquippedTool =>
            (this.m_currentWeapon as VRage.ModAPI.IMyEntity);

        public VRage.Game.Entity.MyEntity RelativeDampeningEntity
        {
            get => 
                this.m_relativeDampeningEntity;
            set
            {
                if (!ReferenceEquals(this.m_relativeDampeningEntity, value))
                {
                    if (this.m_relativeDampeningEntity != null)
                    {
                        this.m_relativeDampeningEntity.OnClose -= new Action<VRage.Game.Entity.MyEntity>(this.relativeDampeningEntityClosed);
                    }
                    this.m_relativeDampeningEntity = value;
                    if (this.m_relativeDampeningEntity != null)
                    {
                        this.m_relativeDampeningEntity.OnClose += new Action<VRage.Game.Entity.MyEntity>(this.relativeDampeningEntityClosed);
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCharacter.<>c <>9 = new MyCharacter.<>c();
            public static Func<MyCharacter, Action<long>> <>9__19_0;
            public static Func<MyCharacter, Action> <>9__20_1;
            public static Func<MyCharacter, Action<long, bool>> <>9__20_0;
            public static Func<MyCharacter, Action> <>9__23_0;
            public static Func<MyCharacter, Action<bool, bool, bool, bool, bool>> <>9__587_0;
            public static Func<MyCharacter, Action> <>9__598_0;
            public static Func<MyCharacter, Action<bool>> <>9__662_0;
            public static Func<MyCharacter, Action<bool>> <>9__664_0;
            public static Func<MyCharacter, Action> <>9__700_1;
            public static Func<MyCharacter, Action> <>9__702_0;
            public static Func<IMyEventOwner, Action<long, long>> <>9__707_0;
            public static Func<IMyEventOwner, Action<long, byte[]>> <>9__708_1;
            public static Func<MyCharacter, Action<string, Vector3, bool, long>> <>9__774_0;
            public static Func<MyCharacter, Action<SerializableDefinitionId, float>> <>9__776_0;
            public static Func<MyCharacter, Action<float>> <>9__778_0;
            public static Func<MyCharacter, Action<SerializableDefinitionId>> <>9__780_0;
            public static Func<MyCharacter, Action<MyCueId>> <>9__782_0;
            public static Func<MyCharacter, Action<bool>> <>9__785_0;
            public static Func<MyCharacter, Action<Vector3>> <>9__855_0;
            public static Func<MyCharacter, Action> <>9__881_0;
            public static Func<MyCharacter, Action<MyDamageInformation, Vector3>> <>9__945_0;
            public static Func<MyCharacter, Action<Vector3, MyShootActionEnum>> <>9__953_0;
            public static Func<MyCharacter, Action<MyShootActionEnum>> <>9__959_0;
            public static Func<MyCharacter, Action<MyShootActionEnum>> <>9__960_0;
            public static Func<MyCharacter, Action<Vector3>> <>9__963_0;
            public static Func<MyCharacter, Action> <>9__965_0;
            public static Func<MyCharacter, Action<SerializableDefinitionId?, uint?>> <>9__967_0;
            public static Func<MyCharacter, Action<SerializableDefinitionId?, uint?, long>> <>9__968_1;
            public static Func<MyCharacter, Action<SerializableDefinitionId?, uint?, long>> <>9__968_2;
            public static Func<MyCharacter, Action> <>9__968_0;
            public static Func<MyCharacter, Action<MyAnimationCommand>> <>9__970_0;
            public static Func<MyCharacter, Action<string>> <>9__972_0;
            public static Func<MyCharacter, Action<int, Vector3[], Quaternion[], Quaternion, Vector3>> <>9__974_0;
            public static Func<IMyEventOwner, Action<long, long>> <>9__993_0;

            internal Action<Vector3, MyShootActionEnum> <BeginShootSync>b__953_0(MyCharacter x) => 
                new Action<Vector3, MyShootActionEnum>(x.ShootBeginCallback);

            internal Action<string, Vector3, bool, long> <ChangeModelAndColor>b__774_0(MyCharacter x) => 
                new Action<string, Vector3, bool, long>(x.ChangeModel_Implementation);

            internal Action <Die>b__700_1(MyCharacter x) => 
                new Action(x.OnSuicideRequest);

            internal Action <DieInternal>b__702_0(MyCharacter x) => 
                new Action(x.UnequipWeapon);

            internal Action<bool> <EnableBroadcasting>b__664_0(MyCharacter x) => 
                new Action<bool>(x.EnableBroadcastingCallback);

            internal Action<bool, bool, bool, bool, bool> <EnableIronsight>b__587_0(MyCharacter x) => 
                new Action<bool, bool, bool, bool, bool>(x.EnableIronsightCallback);

            internal Action<bool> <EnableLights>b__662_0(MyCharacter x) => 
                new Action<bool>(x.EnableLightsCallback);

            internal Action<MyShootActionEnum> <EndShootInternal>b__959_0(MyCharacter x) => 
                new Action<MyShootActionEnum>(x.ShootEndCallback);

            internal Action <GetOffLadder>b__23_0(MyCharacter x) => 
                new Action(x.GetOffLadder_Implementation);

            internal Action<long, bool> <GetOnLadder_Request>b__20_0(MyCharacter x) => 
                new Action<long, bool>(x.GetOnLadder_Implementation);

            internal Action <GetOnLadder_Request>b__20_1(MyCharacter x) => 
                new Action(x.GetOnLadder_Failed);

            internal Action<long> <GetOnLadder>b__19_0(MyCharacter x) => 
                new Action<long>(x.GetOnLadder_Request);

            internal Action<MyShootActionEnum> <GunDoubleClickedInternal>b__960_0(MyCharacter x) => 
                new Action<MyShootActionEnum>(x.GunDoubleClickedCallback);

            internal Action<MyDamageInformation, Vector3> <KillCharacter>b__945_0(MyCharacter x) => 
                new Action<MyDamageInformation, Vector3>(x.OnKillCharacter);

            internal Action <OnSwitchAmmoMagazineRequest>b__965_0(MyCharacter x) => 
                new Action(x.OnSwitchAmmoMagazineSuccess);

            internal Action<MyCueId> <PlaySecondarySound>b__782_0(MyCharacter x) => 
                new Action<MyCueId>(x.OnSecondarySoundPlay);

            internal Action<long, byte[]> <RefreshAssetModifiers>b__708_1(IMyEventOwner x) => 
                new Action<long, byte[]>(MyCharacter.SendSkinData);

            internal Action<SerializableDefinitionId?, uint?> <RequestSwitchToWeapon>b__967_0(MyCharacter x) => 
                new Action<SerializableDefinitionId?, uint?>(x.SwitchToWeaponMessage);

            internal Action<MyAnimationCommand> <SendAnimationCommand>b__970_0(MyCharacter x) => 
                new Action<MyAnimationCommand>(x.OnAnimationCommand);

            internal Action<string> <SendAnimationEvent>b__972_0(MyCharacter x) => 
                new Action<string>(x.OnAnimationEvent);

            internal Action<int, Vector3[], Quaternion[], Quaternion, Vector3> <SendRagdollTransforms>b__974_0(MyCharacter x) => 
                new Action<int, Vector3[], Quaternion[], Quaternion, Vector3>(x.OnRagdollTransformsUpdate);

            internal Action<SerializableDefinitionId> <SendRefillFromBottle>b__780_0(MyCharacter x) => 
                new Action<SerializableDefinitionId>(x.OnRefillFromBottle);

            internal Action<bool> <SetPhysicsEnabled>b__785_0(MyCharacter x) => 
                new Action<bool>(x.EnablePhysics);

            internal Action<long, long> <SetRelativeDampening>b__993_0(IMyEventOwner s) => 
                new Action<long, long>(MyPlayerCollection.SetDampeningEntityClient);

            internal Action <SwitchAmmoMagazineInternal>b__598_0(MyCharacter x) => 
                new Action(x.OnSwitchAmmoMagazineRequest);

            internal Action <SwitchToWeaponMessage>b__968_0(MyCharacter x) => 
                new Action(x.UnequipWeapon);

            internal Action<SerializableDefinitionId?, uint?, long> <SwitchToWeaponMessage>b__968_1(MyCharacter x) => 
                new Action<SerializableDefinitionId?, uint?, long>(x.OnSwitchToWeaponSuccess);

            internal Action<SerializableDefinitionId?, uint?, long> <SwitchToWeaponMessage>b__968_2(MyCharacter x) => 
                new Action<SerializableDefinitionId?, uint?, long>(x.OnSwitchToWeaponSuccess);

            internal Action<long, long> <UpdateAssetModifiers>b__707_0(IMyEventOwner x) => 
                new Action<long, long>(MyCharacter.RefreshAssetModifiers);

            internal Action<float> <UpdateOxygen>b__778_0(MyCharacter x) => 
                new Action<float>(x.OnUpdateOxygen);

            internal Action<Vector3> <UpdateShootDirection>b__963_0(MyCharacter x) => 
                new Action<Vector3>(x.ShootDirectionChangeCallback);

            internal Action<SerializableDefinitionId, float> <UpdateStoredGas>b__776_0(MyCharacter x) => 
                new Action<SerializableDefinitionId, float>(x.UpdateStoredGas_Implementation);

            internal Action<Vector3> <VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Jump>b__855_0(MyCharacter x) => 
                new Action<Vector3>(x.Jump);

            internal Action <VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchHelmet>b__881_0(MyCharacter x) => 
                new Action(x.OnSwitchHelmet);
        }

        private class MyCharacterPosition : MyPositionComponent
        {
            private const int CHECK_FREQUENCY = 20;
            private int m_checkOutOfWorldCounter;

            private void ClampToWorld()
            {
                if (MySession.Static.WorldBoundaries != null)
                {
                    this.m_checkOutOfWorldCounter++;
                    if (this.m_checkOutOfWorldCounter > 20)
                    {
                        Vector3D position = base.GetPosition();
                        Vector3D min = MySession.Static.WorldBoundaries.Value.Min;
                        Vector3D max = MySession.Static.WorldBoundaries.Value.Max;
                        Vector3D vectord4 = position - (Vector3.One * 10f);
                        Vector3D vectord5 = position + (Vector3.One * 10f);
                        if (((vectord4.X >= min.X) && ((vectord4.Y >= min.Y) && ((vectord4.Z >= min.Z) && ((vectord5.X <= max.X) && (vectord5.Y <= max.Y))))) && (vectord5.Z <= max.Z))
                        {
                            this.m_checkOutOfWorldCounter = 0;
                        }
                        else
                        {
                            Vector3 linearVelocity = base.Container.Entity.Physics.LinearVelocity;
                            bool flag = false;
                            if ((position.X < min.X) || (position.X > max.X))
                            {
                                flag = true;
                                linearVelocity.X = 0f;
                            }
                            if ((position.Y < min.Y) || (position.Y > max.Y))
                            {
                                flag = true;
                                linearVelocity.Y = 0f;
                            }
                            if ((position.Z < min.Z) || (position.Z > max.Z))
                            {
                                flag = true;
                                linearVelocity.Z = 0f;
                            }
                            if (flag)
                            {
                                this.m_checkOutOfWorldCounter = 0;
                                base.SetPosition(Vector3.Clamp((Vector3) position, (Vector3) min, (Vector3) max), null, false, true);
                                base.Container.Entity.Physics.LinearVelocity = linearVelocity;
                            }
                            this.m_checkOutOfWorldCounter = 20;
                        }
                    }
                }
            }

            protected override void OnWorldPositionChanged(object source, bool updateChildren, bool forceUpdateAllChildren)
            {
                this.ClampToWorld();
                base.OnWorldPositionChanged(source, updateChildren, forceUpdateAllChildren);
            }
        }
    }
}

