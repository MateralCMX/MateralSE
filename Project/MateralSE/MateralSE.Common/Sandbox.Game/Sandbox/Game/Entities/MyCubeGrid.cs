namespace Sandbox.Game.Entities
{
    using Havok;
    using ParallelTasks;
    using ProtoBuf;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.CoordinateSystem;
    using Sandbox.Game.GameSystems.StructuralIntegrity;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication;
    using Sandbox.Game.Replication.ClientStates;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.SessionComponents.Clipboard;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Compression;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.EntityComponents;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
    using VRage.Groups;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Plugins;
    using VRage.Profiler;
    using VRage.Sync;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageMath.PackedVector;
    using VRageMath.Spatial;
    using VRageRender;
    using VRageRender.Messages;

    [StaticEventOwner, MyEntityType(typeof(MyObjectBuilder_CubeGrid), true)]
    public class MyCubeGrid : VRage.Game.Entity.MyEntity, IMyGridConnectivityTest, IMyEventProxy, IMyEventOwner, VRage.Game.ModAPI.IMyCubeGrid, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.Game.ModAPI.Ingame.IMyCubeGrid, IMySyncedEntity
    {
        private const double GRID_PLACING_AREA_FIX_VALUE = 0.11;
        private const string EXPORT_DIRECTORY = "ExportedModels";
        private const string SOURCE_DIRECTORY = "SourceModels";
        private static readonly List<MyObjectBuilder_CubeGrid[]> m_prefabs = new List<MyObjectBuilder_CubeGrid[]>();
        [ThreadStatic]
        private static List<VRage.Game.Entity.MyEntity> m_tmpResultListPerThread;
        private static readonly List<MyVoxelBase> m_tmpVoxelList = new List<MyVoxelBase>();
        private static int materialID = 0;
        private static Vector2 tumbnailMultiplier = new Vector2();
        private static float m_maxDimensionPreviousRow = 0f;
        private static Vector3D m_newPositionForPlacedObject = new Vector3D(0.0, 0.0, 0.0);
        private const int m_numRowsForPlacedObjects = 4;
        private static List<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>> m_lineOverlapList = new List<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>>();
        [ThreadStatic]
        private static List<HkBodyCollision> m_physicsBoxQueryListPerThread;
        [ThreadStatic]
        private static Dictionary<Vector3I, MySlimBlock> m_tmpCubeSet = new Dictionary<Vector3I, MySlimBlock>(Vector3I.Comparer);
        private static readonly MyDisconnectHelper m_disconnectHelper = new MyDisconnectHelper();
        private static readonly List<NeighborOffsetIndex> m_neighborOffsetIndices = new List<NeighborOffsetIndex>(0x1a);
        private static readonly List<float> m_neighborDistances = new List<float>(0x1a);
        private static readonly List<Vector3I> m_neighborOffsets = new List<Vector3I>(0x1a);
        [ThreadStatic]
        private static MyRandom m_deformationRng;
        [ThreadStatic]
        private static List<Vector3I> m_cacheRayCastCellsPerThread;
        [ThreadStatic]
        private static Dictionary<Vector3I, ConnectivityResult> m_cacheNeighborBlocksPerThread;
        [ThreadStatic]
        private static List<MyCubeBlockDefinition.MountPoint> m_cacheMountPointsAPerThread;
        [ThreadStatic]
        private static List<MyCubeBlockDefinition.MountPoint> m_cacheMountPointsBPerThread;
        private static readonly MyComponentList m_buildComponents = new MyComponentList();
        [ThreadStatic]
        private static List<MyPhysics.HitInfo> m_tmpHitListPerThread;
        private static readonly HashSet<Vector3UByte> m_tmpAreaMountpointPass = new HashSet<Vector3UByte>();
        private static readonly AreaConnectivityTest m_areaOverlapTest = new AreaConnectivityTest();
        [ThreadStatic]
        private static List<Vector3I> m_tmpCubeNeighboursPerThread;
        private static readonly HashSet<Tuple<MySlimBlock, ushort?>> m_tmpBlocksInMultiBlock = new HashSet<Tuple<MySlimBlock, ushort?>>();
        private static readonly List<MySlimBlock> m_tmpSlimBlocks = new List<MySlimBlock>();
        [ThreadStatic]
        private static List<int> m_tmpMultiBlockIndicesPerThread;
        private static readonly System.Type m_gridSystemsType = ChooseGridSystemsType();
        private static readonly List<Tuple<Vector3I, ushort>> m_tmpRazeList = new List<Tuple<Vector3I, ushort>>();
        private static readonly List<Vector3I> m_tmpLocations = new List<Vector3I>();
        [ThreadStatic]
        private static Ref<HkBoxShape> m_lastQueryBoxPerThread;
        [ThreadStatic]
        private static MatrixD m_lastQueryTransform;
        private const double ROTATION_PRECISION = 0.0010000000474974513;
        private static readonly int BLOCK_LIMIT_FOR_LARGE_DESTRUCTION = 3;
        private static readonly int TRASH_HIGHLIGHT = 300;
        private static MyCubeGridHitInfo m_hitInfoTmp;
        private static HashSet<MyBlockLocation> m_tmpBuildList = new HashSet<MyBlockLocation>();
        private static List<Vector3I> m_tmpPositionListReceive = new List<Vector3I>();
        private static List<Vector3I> m_tmpPositionListSend = new List<Vector3I>();
        private List<Vector3I> m_removeBlockQueueWithGenerators;
        private List<Vector3I> m_removeBlockQueueWithoutGenerators;
        private List<Vector3I> m_destroyBlockQueue;
        private List<Vector3I> m_destructionDeformationQueue;
        private List<BlockPositionId> m_destroyBlockWithIdQueueWithGenerators;
        private List<BlockPositionId> m_destroyBlockWithIdQueueWithoutGenerators;
        private List<BlockPositionId> m_removeBlockWithIdQueueWithGenerators;
        private List<BlockPositionId> m_removeBlockWithIdQueueWithoutGenerators;
        [ThreadStatic]
        private static List<byte> m_boneByteList;
        private List<long> m_tmpBlockIdList;
        private List<MyCubeBlock> m_inventories;
        private HashSet<MyCubeBlock> m_unsafeBlocks;
        private HashSet<MyDecoy> m_decoys;
        private bool m_isRazeBatchDelayed;
        private MyDelayedRazeBatch m_delayedRazeBatch;
        public HashSet<MyCockpit> m_occupiedBlocks;
        private Vector3 m_gravity;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_handBrakeSync;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_dampenersEnabled;
        private static List<MyObjectBuilder_CubeGrid> m_recievedGrids = new List<MyObjectBuilder_CubeGrid>();
        public bool IsAccessibleForProgrammableBlock;
        private bool m_largeDestroyInProgress;
        private readonly VRage.Sync.Sync<bool, SyncDirection.FromServer> m_markedAsTrash;
        private int m_trashHighlightCounter;
        private float m_totalBoneDisplacement;
        private static float m_precalculatedCornerBonesDisplacementDistance = 0f;
        internal MyVoxelSegmentation BonesToSend;
        private MyVoxelSegmentation m_bonesToSendSecond;
        private int m_bonesSendCounter;
        private MyDirtyRegion m_dirtyRegion;
        private MyDirtyRegion m_dirtyRegionParallel;
        private MyCubeSize m_gridSizeEnum;
        private Vector3I m_min;
        private Vector3I m_max;
        private readonly ConcurrentDictionary<Vector3I, MyCube> m_cubes;
        private readonly FastResourceLock m_cubeLock;
        private bool m_canHavePhysics;
        private bool m_hasStandAloneBlocks;
        private readonly HashSet<MySlimBlock> m_cubeBlocks;
        private MyConcurrentList<MyCubeBlock> m_fatBlocks;
        private MyLocalityGrouping m_explosions;
        private Dictionary<Vector3, int> m_colorStatistics;
        private int m_PCU;
        private bool m_IsPowered;
        private HashSet<MyCubeBlock> m_processedBlocks;
        private HashSet<MyCubeBlock> m_blocksForDraw;
        private List<MyCubeGrid> m_tmpGrids;
        private MyTestDisconnectsReason m_disconnectsDirty;
        private bool m_blocksForDamageApplicationDirty;
        private bool m_boundsDirty;
        private int m_lastUpdatedDirtyBounds;
        private HashSet<MySlimBlock> m_blocksForDamageApplication;
        private List<MySlimBlock> m_blocksForDamageApplicationCopy;
        private bool m_updatingDirty;
        private int m_resolvingSplits;
        private HashSet<Vector3UByte> m_tmpBuildFailList;
        private List<Vector3UByte> m_tmpBuildOffsets;
        private List<MySlimBlock> m_tmpBuildSuccessBlocks;
        private static List<Vector3I> m_tmpBlockPositions = new List<Vector3I>();
        [ThreadStatic]
        private static List<MySlimBlock> m_tmpBlockListReceive = new List<MySlimBlock>();
        [ThreadStatic]
        private static List<MyCockpit> m_tmpOccupiedCockpitsPerThread;
        [ThreadStatic]
        private static List<MyObjectBuilder_BlockGroup> m_tmpBlockGroupsPerThread;
        public bool HasShipSoundEvents;
        public int NumberOfReactors;
        public float GridGeneralDamageModifier;
        internal MyGridSkeleton Skeleton;
        public readonly BlockTypeCounter BlockCounter;
        public Dictionary<MyObjectBuilderType, int> BlocksCounters;
        private const float m_gizmoMaxDistanceFromCamera = 100f;
        private const float m_gizmoDrawLineScale = 0.002f;
        private bool m_isStatic;
        public Vector3I? XSymmetryPlane;
        public Vector3I? YSymmetryPlane;
        public Vector3I? ZSymmetryPlane;
        public bool XSymmetryOdd;
        public bool YSymmetryOdd;
        public bool ZSymmetryOdd;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_isRespawnGrid;
        public int m_playedTime;
        public bool ControlledFromTurret;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_destructibleBlocks;
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_editable;
        internal readonly List<MyBlockGroup> BlockGroups;
        internal MyCubeGridOwnershipManager m_ownershipManager;
        public MyProjectorBase Projector;
        private bool m_isMarkedForEarlyDeactivation;
        [CompilerGenerated]
        private Action<MySlimBlock> OnBlockAdded;
        [CompilerGenerated]
        private Action<MyCubeBlock> OnFatBlockAdded;
        [CompilerGenerated]
        private Action<MySlimBlock> OnBlockRemoved;
        [CompilerGenerated]
        private Action<MyCubeBlock> OnFatBlockRemoved;
        [CompilerGenerated]
        private Action<MySlimBlock> OnBlockIntegrityChanged;
        [CompilerGenerated]
        private Action<MySlimBlock> OnBlockClosed;
        [CompilerGenerated]
        private Action<MyCubeBlock> OnFatBlockClosed;
        [CompilerGenerated]
        private Action<MyCubeGrid> OnMassPropertiesChanged;
        [CompilerGenerated]
        private static Action<MyCubeGrid> OnSplitGridCreated;
        [CompilerGenerated]
        private Action<MyCubeGrid> OnBlockOwnershipChanged;
        [CompilerGenerated]
        private Action<bool> OnIsStaticChanged;
        [CompilerGenerated]
        private Action<MyCubeGrid, bool> OnStaticChanged;
        [CompilerGenerated]
        private Action<MyCubeGrid, MyCubeGrid> OnGridSplit;
        [CompilerGenerated]
        private Action<MyCubeGrid> OnHierarchyUpdated;
        [CompilerGenerated]
        private Action<MyGridLogicalGroupData> AddedToLogicalGroup;
        [CompilerGenerated]
        private Action RemovedFromLogicalGroup;
        [CompilerGenerated]
        private Action<int> OnHavokSystemIDChanged;
        [CompilerGenerated]
        private Action<MyCubeGrid> OnNameChanged;
        [CompilerGenerated]
        private Action<MyCubeGrid> OnGridChanged;
        public bool CreatePhysics;
        private static readonly HashSet<MyResourceSinkComponent> m_tmpSinks = new HashSet<MyResourceSinkComponent>();
        private static List<LocationIdentity> m_tmpLocationsAndIdsSend = new List<LocationIdentity>();
        private static List<Tuple<Vector3I, ushort>> m_tmpLocationsAndIdsReceive = new List<Tuple<Vector3I, ushort>>();
        private bool m_smallToLargeConnectionsInitialized;
        private bool m_enableSmallToLargeConnections;
        private MyTestDynamicReason m_testDynamic;
        private bool m_worldPositionChanged;
        private bool m_hasAdditionalModelGenerators;
        public MyTerminalBlock MainCockpit;
        public MyTerminalBlock MainRemoteControl;
        private Dictionary<int, MyCubeGridMultiBlockInfo> m_multiBlockInfos;
        private float PREDICTION_SWITCH_TIME;
        private int PREDICTION_SWITCH_MIN_COUNTER;
        private bool m_inventoryMassDirty;
        private static List<MyVoxelBase> m_overlappingVoxelsTmp;
        private static HashSet<MyVoxelBase> m_rootVoxelsToCutTmp;
        private static ConcurrentQueue<MyTuple<int, MyVoxelBase, Vector3I, Vector3I>> m_notificationQueue = new ConcurrentQueue<MyTuple<int, MyVoxelBase, Vector3I, Vector3I>>();
        private List<DeformationPostponedItem> m_deformationPostponed;
        private static MyConcurrentPool<List<DeformationPostponedItem>> m_postponedListsPool = new MyConcurrentPool<List<DeformationPostponedItem>>(0, null, 0x2710, null);
        private Action m_OnUpdateDirtyCompleted;
        private Action m_UpdateDirtyInternal;
        private bool m_bonesSending;
        private WorkData m_workData;
        [ThreadStatic]
        private static HashSet<VRage.Game.Entity.MyEntity> m_tmpQueryCubeBlocks;
        [ThreadStatic]
        private static HashSet<MySlimBlock> m_tmpQuerySlimBlocks;
        private bool m_generatorsEnabled;
        private static readonly Vector3I[] m_tmpBlockSurroundingOffsets;
        private MyHudNotification m_inertiaDampenersNotification;
        private MyGridClientState m_lastNetState;
        private List<long> m_targetingList;
        private bool m_targetingListIsWhitelist;
        private bool m_usesTargetingList;
        private Action m_convertToShipResult;
        private long m_closestParentId;
        public bool ForceDisablePrediction;
        private Action m_pendingGridReleases;
        private Action<MatrixD> m_updateMergingGrids;

        internal event Action<MyGridLogicalGroupData> AddedToLogicalGroup
        {
            [CompilerGenerated] add
            {
                Action<MyGridLogicalGroupData> addedToLogicalGroup = this.AddedToLogicalGroup;
                while (true)
                {
                    Action<MyGridLogicalGroupData> a = addedToLogicalGroup;
                    Action<MyGridLogicalGroupData> action3 = (Action<MyGridLogicalGroupData>) Delegate.Combine(a, value);
                    addedToLogicalGroup = Interlocked.CompareExchange<Action<MyGridLogicalGroupData>>(ref this.AddedToLogicalGroup, action3, a);
                    if (ReferenceEquals(addedToLogicalGroup, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGridLogicalGroupData> addedToLogicalGroup = this.AddedToLogicalGroup;
                while (true)
                {
                    Action<MyGridLogicalGroupData> source = addedToLogicalGroup;
                    Action<MyGridLogicalGroupData> action3 = (Action<MyGridLogicalGroupData>) Delegate.Remove(source, value);
                    addedToLogicalGroup = Interlocked.CompareExchange<Action<MyGridLogicalGroupData>>(ref this.AddedToLogicalGroup, action3, source);
                    if (ReferenceEquals(addedToLogicalGroup, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MySlimBlock> OnBlockAdded
        {
            [CompilerGenerated] add
            {
                Action<MySlimBlock> onBlockAdded = this.OnBlockAdded;
                while (true)
                {
                    Action<MySlimBlock> a = onBlockAdded;
                    Action<MySlimBlock> action3 = (Action<MySlimBlock>) Delegate.Combine(a, value);
                    onBlockAdded = Interlocked.CompareExchange<Action<MySlimBlock>>(ref this.OnBlockAdded, action3, a);
                    if (ReferenceEquals(onBlockAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MySlimBlock> onBlockAdded = this.OnBlockAdded;
                while (true)
                {
                    Action<MySlimBlock> source = onBlockAdded;
                    Action<MySlimBlock> action3 = (Action<MySlimBlock>) Delegate.Remove(source, value);
                    onBlockAdded = Interlocked.CompareExchange<Action<MySlimBlock>>(ref this.OnBlockAdded, action3, source);
                    if (ReferenceEquals(onBlockAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MySlimBlock> OnBlockClosed
        {
            [CompilerGenerated] add
            {
                Action<MySlimBlock> onBlockClosed = this.OnBlockClosed;
                while (true)
                {
                    Action<MySlimBlock> a = onBlockClosed;
                    Action<MySlimBlock> action3 = (Action<MySlimBlock>) Delegate.Combine(a, value);
                    onBlockClosed = Interlocked.CompareExchange<Action<MySlimBlock>>(ref this.OnBlockClosed, action3, a);
                    if (ReferenceEquals(onBlockClosed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MySlimBlock> onBlockClosed = this.OnBlockClosed;
                while (true)
                {
                    Action<MySlimBlock> source = onBlockClosed;
                    Action<MySlimBlock> action3 = (Action<MySlimBlock>) Delegate.Remove(source, value);
                    onBlockClosed = Interlocked.CompareExchange<Action<MySlimBlock>>(ref this.OnBlockClosed, action3, source);
                    if (ReferenceEquals(onBlockClosed, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MySlimBlock> OnBlockIntegrityChanged
        {
            [CompilerGenerated] add
            {
                Action<MySlimBlock> onBlockIntegrityChanged = this.OnBlockIntegrityChanged;
                while (true)
                {
                    Action<MySlimBlock> a = onBlockIntegrityChanged;
                    Action<MySlimBlock> action3 = (Action<MySlimBlock>) Delegate.Combine(a, value);
                    onBlockIntegrityChanged = Interlocked.CompareExchange<Action<MySlimBlock>>(ref this.OnBlockIntegrityChanged, action3, a);
                    if (ReferenceEquals(onBlockIntegrityChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MySlimBlock> onBlockIntegrityChanged = this.OnBlockIntegrityChanged;
                while (true)
                {
                    Action<MySlimBlock> source = onBlockIntegrityChanged;
                    Action<MySlimBlock> action3 = (Action<MySlimBlock>) Delegate.Remove(source, value);
                    onBlockIntegrityChanged = Interlocked.CompareExchange<Action<MySlimBlock>>(ref this.OnBlockIntegrityChanged, action3, source);
                    if (ReferenceEquals(onBlockIntegrityChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeGrid> OnBlockOwnershipChanged
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid> onBlockOwnershipChanged = this.OnBlockOwnershipChanged;
                while (true)
                {
                    Action<MyCubeGrid> a = onBlockOwnershipChanged;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Combine(a, value);
                    onBlockOwnershipChanged = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnBlockOwnershipChanged, action3, a);
                    if (ReferenceEquals(onBlockOwnershipChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid> onBlockOwnershipChanged = this.OnBlockOwnershipChanged;
                while (true)
                {
                    Action<MyCubeGrid> source = onBlockOwnershipChanged;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Remove(source, value);
                    onBlockOwnershipChanged = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnBlockOwnershipChanged, action3, source);
                    if (ReferenceEquals(onBlockOwnershipChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MySlimBlock> OnBlockRemoved
        {
            [CompilerGenerated] add
            {
                Action<MySlimBlock> onBlockRemoved = this.OnBlockRemoved;
                while (true)
                {
                    Action<MySlimBlock> a = onBlockRemoved;
                    Action<MySlimBlock> action3 = (Action<MySlimBlock>) Delegate.Combine(a, value);
                    onBlockRemoved = Interlocked.CompareExchange<Action<MySlimBlock>>(ref this.OnBlockRemoved, action3, a);
                    if (ReferenceEquals(onBlockRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MySlimBlock> onBlockRemoved = this.OnBlockRemoved;
                while (true)
                {
                    Action<MySlimBlock> source = onBlockRemoved;
                    Action<MySlimBlock> action3 = (Action<MySlimBlock>) Delegate.Remove(source, value);
                    onBlockRemoved = Interlocked.CompareExchange<Action<MySlimBlock>>(ref this.OnBlockRemoved, action3, source);
                    if (ReferenceEquals(onBlockRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeBlock> OnFatBlockAdded
        {
            [CompilerGenerated] add
            {
                Action<MyCubeBlock> onFatBlockAdded = this.OnFatBlockAdded;
                while (true)
                {
                    Action<MyCubeBlock> a = onFatBlockAdded;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Combine(a, value);
                    onFatBlockAdded = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.OnFatBlockAdded, action3, a);
                    if (ReferenceEquals(onFatBlockAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeBlock> onFatBlockAdded = this.OnFatBlockAdded;
                while (true)
                {
                    Action<MyCubeBlock> source = onFatBlockAdded;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Remove(source, value);
                    onFatBlockAdded = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.OnFatBlockAdded, action3, source);
                    if (ReferenceEquals(onFatBlockAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeBlock> OnFatBlockClosed
        {
            [CompilerGenerated] add
            {
                Action<MyCubeBlock> onFatBlockClosed = this.OnFatBlockClosed;
                while (true)
                {
                    Action<MyCubeBlock> a = onFatBlockClosed;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Combine(a, value);
                    onFatBlockClosed = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.OnFatBlockClosed, action3, a);
                    if (ReferenceEquals(onFatBlockClosed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeBlock> onFatBlockClosed = this.OnFatBlockClosed;
                while (true)
                {
                    Action<MyCubeBlock> source = onFatBlockClosed;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Remove(source, value);
                    onFatBlockClosed = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.OnFatBlockClosed, action3, source);
                    if (ReferenceEquals(onFatBlockClosed, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeBlock> OnFatBlockRemoved
        {
            [CompilerGenerated] add
            {
                Action<MyCubeBlock> onFatBlockRemoved = this.OnFatBlockRemoved;
                while (true)
                {
                    Action<MyCubeBlock> a = onFatBlockRemoved;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Combine(a, value);
                    onFatBlockRemoved = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.OnFatBlockRemoved, action3, a);
                    if (ReferenceEquals(onFatBlockRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeBlock> onFatBlockRemoved = this.OnFatBlockRemoved;
                while (true)
                {
                    Action<MyCubeBlock> source = onFatBlockRemoved;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Remove(source, value);
                    onFatBlockRemoved = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.OnFatBlockRemoved, action3, source);
                    if (ReferenceEquals(onFatBlockRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeGrid> OnGridChanged
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid> onGridChanged = this.OnGridChanged;
                while (true)
                {
                    Action<MyCubeGrid> a = onGridChanged;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Combine(a, value);
                    onGridChanged = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnGridChanged, action3, a);
                    if (ReferenceEquals(onGridChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid> onGridChanged = this.OnGridChanged;
                while (true)
                {
                    Action<MyCubeGrid> source = onGridChanged;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Remove(source, value);
                    onGridChanged = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnGridChanged, action3, source);
                    if (ReferenceEquals(onGridChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeGrid, MyCubeGrid> OnGridSplit
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid, MyCubeGrid> onGridSplit = this.OnGridSplit;
                while (true)
                {
                    Action<MyCubeGrid, MyCubeGrid> a = onGridSplit;
                    Action<MyCubeGrid, MyCubeGrid> action3 = (Action<MyCubeGrid, MyCubeGrid>) Delegate.Combine(a, value);
                    onGridSplit = Interlocked.CompareExchange<Action<MyCubeGrid, MyCubeGrid>>(ref this.OnGridSplit, action3, a);
                    if (ReferenceEquals(onGridSplit, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid, MyCubeGrid> onGridSplit = this.OnGridSplit;
                while (true)
                {
                    Action<MyCubeGrid, MyCubeGrid> source = onGridSplit;
                    Action<MyCubeGrid, MyCubeGrid> action3 = (Action<MyCubeGrid, MyCubeGrid>) Delegate.Remove(source, value);
                    onGridSplit = Interlocked.CompareExchange<Action<MyCubeGrid, MyCubeGrid>>(ref this.OnGridSplit, action3, source);
                    if (ReferenceEquals(onGridSplit, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<int> OnHavokSystemIDChanged
        {
            [CompilerGenerated] add
            {
                Action<int> onHavokSystemIDChanged = this.OnHavokSystemIDChanged;
                while (true)
                {
                    Action<int> a = onHavokSystemIDChanged;
                    Action<int> action3 = (Action<int>) Delegate.Combine(a, value);
                    onHavokSystemIDChanged = Interlocked.CompareExchange<Action<int>>(ref this.OnHavokSystemIDChanged, action3, a);
                    if (ReferenceEquals(onHavokSystemIDChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<int> onHavokSystemIDChanged = this.OnHavokSystemIDChanged;
                while (true)
                {
                    Action<int> source = onHavokSystemIDChanged;
                    Action<int> action3 = (Action<int>) Delegate.Remove(source, value);
                    onHavokSystemIDChanged = Interlocked.CompareExchange<Action<int>>(ref this.OnHavokSystemIDChanged, action3, source);
                    if (ReferenceEquals(onHavokSystemIDChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeGrid> OnHierarchyUpdated
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid> onHierarchyUpdated = this.OnHierarchyUpdated;
                while (true)
                {
                    Action<MyCubeGrid> a = onHierarchyUpdated;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Combine(a, value);
                    onHierarchyUpdated = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnHierarchyUpdated, action3, a);
                    if (ReferenceEquals(onHierarchyUpdated, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid> onHierarchyUpdated = this.OnHierarchyUpdated;
                while (true)
                {
                    Action<MyCubeGrid> source = onHierarchyUpdated;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Remove(source, value);
                    onHierarchyUpdated = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnHierarchyUpdated, action3, source);
                    if (ReferenceEquals(onHierarchyUpdated, source))
                    {
                        return;
                    }
                }
            }
        }

        [Obsolete("Use OnStaticChanged")]
        public event Action<bool> OnIsStaticChanged
        {
            [CompilerGenerated] add
            {
                Action<bool> onIsStaticChanged = this.OnIsStaticChanged;
                while (true)
                {
                    Action<bool> a = onIsStaticChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Combine(a, value);
                    onIsStaticChanged = Interlocked.CompareExchange<Action<bool>>(ref this.OnIsStaticChanged, action3, a);
                    if (ReferenceEquals(onIsStaticChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool> onIsStaticChanged = this.OnIsStaticChanged;
                while (true)
                {
                    Action<bool> source = onIsStaticChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Remove(source, value);
                    onIsStaticChanged = Interlocked.CompareExchange<Action<bool>>(ref this.OnIsStaticChanged, action3, source);
                    if (ReferenceEquals(onIsStaticChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeGrid> OnMassPropertiesChanged
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid> onMassPropertiesChanged = this.OnMassPropertiesChanged;
                while (true)
                {
                    Action<MyCubeGrid> a = onMassPropertiesChanged;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Combine(a, value);
                    onMassPropertiesChanged = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnMassPropertiesChanged, action3, a);
                    if (ReferenceEquals(onMassPropertiesChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid> onMassPropertiesChanged = this.OnMassPropertiesChanged;
                while (true)
                {
                    Action<MyCubeGrid> source = onMassPropertiesChanged;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Remove(source, value);
                    onMassPropertiesChanged = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnMassPropertiesChanged, action3, source);
                    if (ReferenceEquals(onMassPropertiesChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeGrid> OnNameChanged
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid> onNameChanged = this.OnNameChanged;
                while (true)
                {
                    Action<MyCubeGrid> a = onNameChanged;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Combine(a, value);
                    onNameChanged = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnNameChanged, action3, a);
                    if (ReferenceEquals(onNameChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid> onNameChanged = this.OnNameChanged;
                while (true)
                {
                    Action<MyCubeGrid> source = onNameChanged;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Remove(source, value);
                    onNameChanged = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnNameChanged, action3, source);
                    if (ReferenceEquals(onNameChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyCubeGrid> OnSplitGridCreated
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid> onSplitGridCreated = OnSplitGridCreated;
                while (true)
                {
                    Action<MyCubeGrid> a = onSplitGridCreated;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Combine(a, value);
                    onSplitGridCreated = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref OnSplitGridCreated, action3, a);
                    if (ReferenceEquals(onSplitGridCreated, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid> onSplitGridCreated = OnSplitGridCreated;
                while (true)
                {
                    Action<MyCubeGrid> source = onSplitGridCreated;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Remove(source, value);
                    onSplitGridCreated = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref OnSplitGridCreated, action3, source);
                    if (ReferenceEquals(onSplitGridCreated, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeGrid, bool> OnStaticChanged
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid, bool> onStaticChanged = this.OnStaticChanged;
                while (true)
                {
                    Action<MyCubeGrid, bool> a = onStaticChanged;
                    Action<MyCubeGrid, bool> action3 = (Action<MyCubeGrid, bool>) Delegate.Combine(a, value);
                    onStaticChanged = Interlocked.CompareExchange<Action<MyCubeGrid, bool>>(ref this.OnStaticChanged, action3, a);
                    if (ReferenceEquals(onStaticChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid, bool> onStaticChanged = this.OnStaticChanged;
                while (true)
                {
                    Action<MyCubeGrid, bool> source = onStaticChanged;
                    Action<MyCubeGrid, bool> action3 = (Action<MyCubeGrid, bool>) Delegate.Remove(source, value);
                    onStaticChanged = Interlocked.CompareExchange<Action<MyCubeGrid, bool>>(ref this.OnStaticChanged, action3, source);
                    if (ReferenceEquals(onStaticChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        internal event Action RemovedFromLogicalGroup
        {
            [CompilerGenerated] add
            {
                Action removedFromLogicalGroup = this.RemovedFromLogicalGroup;
                while (true)
                {
                    Action a = removedFromLogicalGroup;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    removedFromLogicalGroup = Interlocked.CompareExchange<Action>(ref this.RemovedFromLogicalGroup, action3, a);
                    if (ReferenceEquals(removedFromLogicalGroup, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action removedFromLogicalGroup = this.RemovedFromLogicalGroup;
                while (true)
                {
                    Action source = removedFromLogicalGroup;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    removedFromLogicalGroup = Interlocked.CompareExchange<Action>(ref this.RemovedFromLogicalGroup, action3, source);
                    if (ReferenceEquals(removedFromLogicalGroup, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<SyncBase> SyncPropertyChanged
        {
            add
            {
                this.SyncType.PropertyChanged += value;
            }
            remove
            {
                this.SyncType.PropertyChanged -= value;
            }
        }

        event Action<VRage.Game.ModAPI.IMySlimBlock> VRage.Game.ModAPI.IMyCubeGrid.OnBlockAdded
        {
            add
            {
                this.OnBlockAdded += this.GetDelegate(value);
            }
            remove
            {
                this.OnBlockAdded -= this.GetDelegate(value);
            }
        }

        event Action<VRage.Game.ModAPI.IMySlimBlock> VRage.Game.ModAPI.IMyCubeGrid.OnBlockIntegrityChanged
        {
            add
            {
                this.OnBlockIntegrityChanged += this.GetDelegate(value);
            }
            remove
            {
                this.OnBlockIntegrityChanged -= this.GetDelegate(value);
            }
        }

        event Action<VRage.Game.ModAPI.IMyCubeGrid> VRage.Game.ModAPI.IMyCubeGrid.OnBlockOwnershipChanged
        {
            add
            {
                this.OnBlockOwnershipChanged += this.GetDelegate(value);
            }
            remove
            {
                this.OnBlockOwnershipChanged -= this.GetDelegate(value);
            }
        }

        event Action<VRage.Game.ModAPI.IMySlimBlock> VRage.Game.ModAPI.IMyCubeGrid.OnBlockRemoved
        {
            add
            {
                this.OnBlockRemoved += this.GetDelegate(value);
            }
            remove
            {
                this.OnBlockRemoved -= this.GetDelegate(value);
            }
        }

        event Action<VRage.Game.ModAPI.IMyCubeGrid> VRage.Game.ModAPI.IMyCubeGrid.OnGridChanged
        {
            add
            {
                this.OnGridChanged += this.GetDelegate(value);
            }
            remove
            {
                this.OnGridChanged -= this.GetDelegate(value);
            }
        }

        event Action<VRage.Game.ModAPI.IMyCubeGrid, VRage.Game.ModAPI.IMyCubeGrid> VRage.Game.ModAPI.IMyCubeGrid.OnGridSplit
        {
            add
            {
                this.OnGridSplit += this.GetDelegate(value);
            }
            remove
            {
                this.OnGridSplit -= this.GetDelegate(value);
            }
        }

        event Action<VRage.Game.ModAPI.IMyCubeGrid, bool> VRage.Game.ModAPI.IMyCubeGrid.OnIsStaticChanged
        {
            add
            {
                this.OnStaticChanged += this.GetDelegate(value);
            }
            remove
            {
                this.OnStaticChanged -= this.GetDelegate(value);
            }
        }

        static MyCubeGrid()
        {
            Vector3I[] vectoriArray1 = new Vector3I[0x1b];
            vectoriArray1[0] = new Vector3I(0, 0, 0);
            vectoriArray1[1] = new Vector3I(1, 0, 0);
            vectoriArray1[2] = new Vector3I(-1, 0, 0);
            vectoriArray1[3] = new Vector3I(0, 0, 1);
            vectoriArray1[4] = new Vector3I(0, 0, -1);
            vectoriArray1[5] = new Vector3I(1, 0, 1);
            vectoriArray1[6] = new Vector3I(-1, 0, 1);
            vectoriArray1[7] = new Vector3I(1, 0, -1);
            vectoriArray1[8] = new Vector3I(-1, 0, -1);
            vectoriArray1[9] = new Vector3I(0, 1, 0);
            vectoriArray1[10] = new Vector3I(1, 1, 0);
            vectoriArray1[11] = new Vector3I(-1, 1, 0);
            vectoriArray1[12] = new Vector3I(0, 1, 1);
            vectoriArray1[13] = new Vector3I(0, 1, -1);
            vectoriArray1[14] = new Vector3I(1, 1, 1);
            vectoriArray1[15] = new Vector3I(-1, 1, 1);
            vectoriArray1[0x10] = new Vector3I(1, 1, -1);
            vectoriArray1[0x11] = new Vector3I(-1, 1, -1);
            vectoriArray1[0x12] = new Vector3I(0, -1, 0);
            vectoriArray1[0x13] = new Vector3I(1, -1, 0);
            vectoriArray1[20] = new Vector3I(-1, -1, 0);
            vectoriArray1[0x15] = new Vector3I(0, -1, 1);
            vectoriArray1[0x16] = new Vector3I(0, -1, -1);
            vectoriArray1[0x17] = new Vector3I(1, -1, 1);
            vectoriArray1[0x18] = new Vector3I(-1, -1, 1);
            vectoriArray1[0x19] = new Vector3I(1, -1, -1);
            vectoriArray1[0x1a] = new Vector3I(-1, -1, -1);
            m_tmpBlockSurroundingOffsets = vectoriArray1;
            for (int i = 0; i < 0x1a; i++)
            {
                m_neighborOffsetIndices.Add((NeighborOffsetIndex) i);
                m_neighborDistances.Add(0f);
                m_neighborOffsets.Add(new Vector3I(0, 0, 0));
            }
            m_neighborOffsets[0] = new Vector3I(1, 0, 0);
            m_neighborOffsets[1] = new Vector3I(-1, 0, 0);
            m_neighborOffsets[2] = new Vector3I(0, 1, 0);
            m_neighborOffsets[3] = new Vector3I(0, -1, 0);
            m_neighborOffsets[4] = new Vector3I(0, 0, 1);
            m_neighborOffsets[5] = new Vector3I(0, 0, -1);
            m_neighborOffsets[6] = new Vector3I(1, 1, 0);
            m_neighborOffsets[7] = new Vector3I(1, -1, 0);
            m_neighborOffsets[8] = new Vector3I(-1, 1, 0);
            m_neighborOffsets[9] = new Vector3I(-1, -1, 0);
            m_neighborOffsets[10] = new Vector3I(0, 1, 1);
            m_neighborOffsets[11] = new Vector3I(0, 1, -1);
            m_neighborOffsets[12] = new Vector3I(0, -1, 1);
            m_neighborOffsets[13] = new Vector3I(0, -1, -1);
            m_neighborOffsets[14] = new Vector3I(1, 0, 1);
            m_neighborOffsets[15] = new Vector3I(1, 0, -1);
            m_neighborOffsets[0x10] = new Vector3I(-1, 0, 1);
            m_neighborOffsets[0x11] = new Vector3I(-1, 0, -1);
            m_neighborOffsets[0x12] = new Vector3I(1, 1, 1);
            m_neighborOffsets[0x13] = new Vector3I(1, 1, -1);
            m_neighborOffsets[20] = new Vector3I(1, -1, 1);
            m_neighborOffsets[0x15] = new Vector3I(1, -1, -1);
            m_neighborOffsets[0x16] = new Vector3I(-1, 1, 1);
            m_neighborOffsets[0x17] = new Vector3I(-1, 1, -1);
            m_neighborOffsets[0x18] = new Vector3I(-1, -1, 1);
            m_neighborOffsets[0x19] = new Vector3I(-1, -1, -1);
            GridCounter = 0;
        }

        public MyCubeGrid() : this(MyCubeSize.Large)
        {
            this.GridScale = 1f;
            this.Render = new MyRenderComponentCubeGrid();
            this.Render.NeedsDraw = true;
            base.PositionComp = new MyCubeGridPosition();
            this.IsUnsupportedStation = false;
            base.Hierarchy.QueryAABBImpl = new Action<BoundingBoxD, List<VRage.Game.Entity.MyEntity>>(this.QueryAABB);
            base.Hierarchy.QuerySphereImpl = new Action<BoundingSphereD, List<VRage.Game.Entity.MyEntity>>(this.QuerySphere);
            base.Hierarchy.QueryLineImpl = new Action<LineD, List<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>>>(this.QueryLine);
            base.Components.Add<MyGridTargeting>(new MyGridTargeting());
            this.SyncType = SyncHelpers.Compose(this, 0);
            this.m_handBrakeSync.ValueChanged += x => this.HandBrakeChanged();
            this.m_dampenersEnabled.ValueChanged += x => this.DampenersEnabledChanged();
            base.m_contactPoint.ValueChanged += x => this.OnContactPointChanged();
            this.m_markedAsTrash.ValueChanged += x => this.MarkedAsTrashChanged();
            this.m_UpdateDirtyInternal = new Action(this.UpdateDirtyInternal);
            this.m_OnUpdateDirtyCompleted = new Action(this.OnUpdateDirtyCompleted);
        }

        private MyCubeGrid(MyCubeSize gridSize)
        {
            this.m_removeBlockQueueWithGenerators = new List<Vector3I>();
            this.m_removeBlockQueueWithoutGenerators = new List<Vector3I>();
            this.m_destroyBlockQueue = new List<Vector3I>();
            this.m_destructionDeformationQueue = new List<Vector3I>();
            this.m_destroyBlockWithIdQueueWithGenerators = new List<BlockPositionId>();
            this.m_destroyBlockWithIdQueueWithoutGenerators = new List<BlockPositionId>();
            this.m_removeBlockWithIdQueueWithGenerators = new List<BlockPositionId>();
            this.m_removeBlockWithIdQueueWithoutGenerators = new List<BlockPositionId>();
            this.m_tmpBlockIdList = new List<long>();
            this.m_inventories = new List<MyCubeBlock>();
            this.m_unsafeBlocks = new HashSet<MyCubeBlock>();
            this.m_occupiedBlocks = new HashSet<MyCockpit>();
            this.m_gravity = Vector3.Zero;
            this.IsAccessibleForProgrammableBlock = true;
            this.BonesToSend = new MyVoxelSegmentation();
            this.m_bonesToSendSecond = new MyVoxelSegmentation();
            this.m_dirtyRegion = new MyDirtyRegion();
            this.m_dirtyRegionParallel = new MyDirtyRegion();
            this.m_min = Vector3I.MaxValue;
            this.m_max = Vector3I.MinValue;
            this.m_cubes = new ConcurrentDictionary<Vector3I, MyCube>();
            this.m_cubeLock = new FastResourceLock();
            this.m_canHavePhysics = true;
            this.m_hasStandAloneBlocks = true;
            this.m_cubeBlocks = new HashSet<MySlimBlock>();
            this.m_fatBlocks = new MyConcurrentList<MyCubeBlock>(100);
            this.m_explosions = new MyLocalityGrouping(MyLocalityGrouping.GroupingMode.Overlaps);
            this.m_colorStatistics = new Dictionary<Vector3, int>();
            this.m_processedBlocks = new HashSet<MyCubeBlock>();
            this.m_blocksForDraw = new HashSet<MyCubeBlock>();
            this.m_tmpGrids = new List<MyCubeGrid>();
            this.m_blocksForDamageApplication = new HashSet<MySlimBlock>();
            this.m_blocksForDamageApplicationCopy = new List<MySlimBlock>();
            this.m_tmpBuildFailList = new HashSet<Vector3UByte>();
            this.m_tmpBuildOffsets = new List<Vector3UByte>();
            this.m_tmpBuildSuccessBlocks = new List<MySlimBlock>();
            this.GridGeneralDamageModifier = 1f;
            this.BlockCounter = new BlockTypeCounter();
            this.BlocksCounters = new Dictionary<MyObjectBuilderType, int>();
            this.BlockGroups = new List<MyBlockGroup>();
            this.m_enableSmallToLargeConnections = true;
            this.PREDICTION_SWITCH_TIME = 5f;
            this.PREDICTION_SWITCH_MIN_COUNTER = 30;
            this.m_deformationPostponed = new List<DeformationPostponedItem>();
            this.m_workData = new WorkData();
            this.m_generatorsEnabled = true;
            this.m_targetingList = new List<long>();
            this.GridScale = 1f;
            this.GridSizeEnum = gridSize;
            this.GridSize = MyDefinitionManager.Static.GetCubeSize(gridSize);
            this.GridSizeHalf = this.GridSize / 2f;
            this.GridSizeHalfVector = new Vector3(this.GridSizeHalf);
            this.GridSizeQuarter = this.GridSize / 4f;
            this.GridSizeQuarterVector = new Vector3(this.GridSizeQuarter);
            this.GridSizeR = 1f / this.GridSize;
            base.NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            this.Skeleton = new MyGridSkeleton();
            GridCounter++;
            base.AddDebugRenderComponent(new MyDebugRenderComponentCubeGrid(this));
            if (MyPerGameSettings.Destruction)
            {
                this.OnPhysicsChanged += entity => MyPhysics.RemoveDestructions(entity);
            }
            if (MyFakes.ASSERT_CHANGES_IN_SIMULATION)
            {
                this.OnPhysicsChanged += delegate (VRage.Game.Entity.MyEntity e) {
                };
                this.OnGridSplit += delegate (MyCubeGrid g1, MyCubeGrid g2) {
                };
            }
        }

        public void ActivatePhysics()
        {
            if ((this.Physics != null) && this.Physics.Enabled)
            {
                this.Physics.RigidBody.Activate();
                if (this.Physics.RigidBody2 != null)
                {
                    this.Physics.RigidBody2.Activate();
                }
            }
        }

        private MySlimBlock AddBlock(MyObjectBuilder_CubeBlock objectBuilder, bool testMerge)
        {
            MySlimBlock block;
            try
            {
                MyCubeBlockDefinition definition;
                if (this.Skeleton == null)
                {
                    this.Skeleton = new MyGridSkeleton();
                }
                MyObjectBuilder_CubeBlock block1 = this.UpgradeCubeBlock(objectBuilder, out definition);
                objectBuilder = block1;
                if (objectBuilder == null)
                {
                    block = null;
                }
                else
                {
                    try
                    {
                        block = this.AddCubeBlock(objectBuilder, testMerge, definition);
                    }
                    catch (DuplicateIdException exception)
                    {
                        string msg = "ERROR while adding cube " + definition.DisplayNameText.ToString() + ": " + exception.ToString();
                        MyLog.Default.WriteLine(msg);
                        block = null;
                    }
                }
            }
            finally
            {
            }
            return block;
        }

        private unsafe void AddBlockEdges(MySlimBlock block)
        {
            MyCubeBlockDefinition blockDefinition = block.BlockDefinition;
            if (((blockDefinition.BlockTopology == MyBlockTopology.Cube) && (blockDefinition.CubeDefinition != null)) && blockDefinition.CubeDefinition.ShowEdges)
            {
                Matrix matrix;
                block.Orientation.GetMatrix(out matrix);
                matrix.Translation = (Vector3) (block.Position * this.GridSize);
                MyCubeGridDefinitions.TableEntry topologyInfo = MyCubeGridDefinitions.GetTopologyInfo(blockDefinition.CubeDefinition.CubeTopology);
                Vector3I vectori = (Vector3I) ((block.Position * 2) + Vector3I.One);
                foreach (MyEdgeDefinition definition2 in topologyInfo.Edges)
                {
                    Vector3 vector2 = Vector3.TransformNormal(definition2.Point0, block.Orientation);
                    Vector3 vector3 = Vector3.TransformNormal(definition2.Point1, block.Orientation);
                    Vector3 vector4 = (vector2 + vector3) * 0.5f;
                    if ((!this.IsDamaged((Vector3I) (vectori + Vector3I.Round(vector2)), 0.04f) && !this.IsDamaged((Vector3I) (vectori + Vector3I.Round(vector4)), 0.04f)) && !this.IsDamaged((Vector3I) (vectori + Vector3I.Round(vector3)), 0.04f))
                    {
                        vector2 = Vector3.Transform(definition2.Point0 * this.GridSizeHalf, ref matrix);
                        vector3 = Vector3.Transform(definition2.Point1 * this.GridSizeHalf, ref matrix);
                        Vector3 vector5 = Vector3.TransformNormal(topologyInfo.Tiles[definition2.Side0].Normal, block.Orientation);
                        Vector3 vector6 = Vector3.TransformNormal(topologyInfo.Tiles[definition2.Side1].Normal, block.Orientation);
                        Vector3 colorMaskHSV = block.ColorMaskHSV;
                        Vector3* vectorPtr1 = (Vector3*) ref colorMaskHSV;
                        vectorPtr1->Y = (colorMaskHSV.Y + 1f) * 0.5f;
                        Vector3* vectorPtr2 = (Vector3*) ref colorMaskHSV;
                        vectorPtr2->Z = (colorMaskHSV.Z + 1f) * 0.5f;
                        this.Render.RenderData.AddEdgeInfo(ref vector2, ref vector3, ref vector5, ref vector6, colorMaskHSV.HSVtoColor(), block);
                    }
                }
            }
        }

        private void AddBlockInSphere(ref BoundingBoxD aabb, HashSet<MySlimBlock> blocks, bool checkTriangles, ref BoundingSphere localSphere, MyCube cube)
        {
            MySlimBlock cubeBlock = cube.CubeBlock;
            BoundingBox box = new BoundingBox((Vector3) ((cubeBlock.Min * this.GridSize) - this.GridSizeHalf), (cubeBlock.Max * this.GridSize) + this.GridSizeHalf);
            if (box.Intersects((BoundingSphere) localSphere))
            {
                if (!checkTriangles)
                {
                    blocks.Add(cubeBlock);
                }
                else if ((cubeBlock.FatBlock == null) || cubeBlock.FatBlock.GetIntersectionWithAABB(ref aabb))
                {
                    blocks.Add(cubeBlock);
                }
            }
        }

        private unsafe void AddBlockInternal(MySlimBlock block)
        {
            Matrix matrix;
            if (block.FatBlock != null)
            {
                block.FatBlock.UpdateWorldMatrix();
            }
            block.CubeGrid = this;
            if (MyFakes.ENABLE_COMPOUND_BLOCKS && (block.FatBlock is MyCompoundCubeBlock))
            {
                MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
                MySlimBlock cubeBlock = this.GetCubeBlock(block.Min);
                MyCompoundCubeBlock block4 = (cubeBlock != null) ? (cubeBlock.FatBlock as MyCompoundCubeBlock) : null;
                if (block4 != null)
                {
                    bool flag2 = false;
                    fatBlock.UpdateWorldMatrix();
                    m_tmpSlimBlocks.Clear();
                    foreach (MySlimBlock block5 in fatBlock.GetBlocks())
                    {
                        ushort num;
                        if (block4.Add(block5, out num))
                        {
                            this.BoundsInclude(block5);
                            this.m_dirtyRegion.AddCube(block5.Min);
                            this.Physics.AddDirtyBlock(cubeBlock);
                            m_tmpSlimBlocks.Add(block5);
                            flag2 = true;
                        }
                    }
                    this.MarkForDraw();
                    foreach (MySlimBlock block6 in m_tmpSlimBlocks)
                    {
                        fatBlock.Remove(block6, true);
                    }
                    if (flag2)
                    {
                        if ((MyCubeGridSmallToLargeConnection.Static != null) && this.m_enableSmallToLargeConnections)
                        {
                            MyCubeGridSmallToLargeConnection.Static.AddBlockSmallToLargeConnection(block);
                        }
                        foreach (MySlimBlock block7 in m_tmpSlimBlocks)
                        {
                            this.NotifyBlockAdded(block7);
                        }
                    }
                    m_tmpSlimBlocks.Clear();
                    return;
                }
            }
            this.m_cubeBlocks.Add(block);
            if (block.FatBlock != null)
            {
                this.m_fatBlocks.Add(block.FatBlock);
            }
            if (!this.m_colorStatistics.ContainsKey(block.ColorMaskHSV))
            {
                this.m_colorStatistics.Add(block.ColorMaskHSV, 0);
            }
            Vector3 colorMaskHSV = block.ColorMaskHSV;
            this.m_colorStatistics[colorMaskHSV] += 1;
            block.AddNeighbours();
            this.BoundsInclude(block);
            if (block.FatBlock != null)
            {
                base.Hierarchy.AddChild(block.FatBlock, false, true);
                this.GridSystems.RegisterInSystems(block.FatBlock);
                if (block.FatBlock.Render.NeedsDrawFromParent)
                {
                    this.m_blocksForDraw.Add(block.FatBlock);
                    block.FatBlock.Render.SetVisibilityUpdates(true);
                }
                MyObjectBuilderType typeId = block.BlockDefinition.Id.TypeId;
                if (typeId != typeof(MyObjectBuilder_CubeBlock))
                {
                    if (!this.BlocksCounters.ContainsKey(typeId))
                    {
                        this.BlocksCounters.Add(typeId, 0);
                    }
                    MyObjectBuilderType type2 = typeId;
                    this.BlocksCounters[type2] += 1;
                }
            }
            block.Orientation.GetMatrix(out matrix);
            bool flag = true;
            Vector3I pos = new Vector3I {
                X = block.Min.X
            };
            while (pos.X <= block.Max.X)
            {
                pos.Y = block.Min.Y;
                while (true)
                {
                    if (pos.Y > block.Max.Y)
                    {
                        int* numPtr3 = (int*) ref pos.X;
                        numPtr3[0]++;
                        break;
                    }
                    pos.Z = block.Min.Z;
                    while (true)
                    {
                        if (pos.Z > block.Max.Z)
                        {
                            int* numPtr2 = (int*) ref pos.Y;
                            numPtr2[0]++;
                            break;
                        }
                        flag &= this.AddCube(block, ref pos, matrix, block.BlockDefinition);
                        int* numPtr1 = (int*) ref pos.Z;
                        numPtr1[0]++;
                    }
                }
            }
            if (this.Physics != null)
            {
                this.Physics.AddBlock(block);
            }
            if (block.FatBlock != null)
            {
                this.ChangeOwner(block.FatBlock, 0L, block.FatBlock.OwnerId);
            }
            if (((MyCubeGridSmallToLargeConnection.Static != null) && this.m_enableSmallToLargeConnections) & flag)
            {
                MyCubeGridSmallToLargeConnection.Static.AddBlockSmallToLargeConnection(block);
            }
            if (MyFakes.ENABLE_MULTIBLOCK_PART_IDS)
            {
                this.AddMultiBlockInfo(block);
            }
            this.NotifyBlockAdded(block);
            block.AddAuthorship();
            this.m_PCU += block.ComponentStack.IsFunctional ? block.BlockDefinition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST;
        }

        private bool AddCube(MySlimBlock block, ref Vector3I pos, Matrix rotation, MyCubeBlockDefinition cubeBlockDefinition)
        {
            MyCube cube1 = new MyCube();
            cube1.Parts = GetCubeParts(block.SkinSubtypeId, cubeBlockDefinition, pos, rotation, this.GridSize, this.GridScale);
            cube1.CubeBlock = block;
            MyCube cube = cube1;
            if (cube != this.m_cubes.GetOrAdd(pos, cube))
            {
                return false;
            }
            this.m_dirtyRegion.AddCube(pos);
            this.MarkForDraw();
            return true;
        }

        private MySlimBlock AddCubeBlock(MyObjectBuilder_CubeBlock objectBuilder, bool testMerge, MyCubeBlockDefinition blockDefinition)
        {
            Vector3I vectori2;
            Matrix matrix;
            Vector3I vectori3;
            Vector3I vectori5;
            Vector3I min = (Vector3I) objectBuilder.Min;
            MySlimBlock.ComputeMax(blockDefinition, (MyBlockOrientation) objectBuilder.BlockOrientation, ref min, out vectori2);
            if (!this.CanAddCubes(min, vectori2))
            {
                return null;
            }
            object obj2 = MyCubeBlockFactory.CreateCubeBlock(objectBuilder);
            MySlimBlock cubeBlock = obj2 as MySlimBlock;
            if (cubeBlock == null)
            {
                cubeBlock = new MySlimBlock();
            }
            if (!cubeBlock.Init(objectBuilder, this, obj2 as MyCubeBlock))
            {
                return null;
            }
            if ((cubeBlock.FatBlock is MyCompoundCubeBlock) && ((cubeBlock.FatBlock as MyCompoundCubeBlock).GetBlocksCount() == 0))
            {
                return null;
            }
            if (cubeBlock.FatBlock != null)
            {
                cubeBlock.FatBlock.Render.FadeIn = this.Render.FadeIn;
                cubeBlock.FatBlock.HookMultiplayer();
            }
            cubeBlock.AddNeighbours();
            this.BoundsInclude(cubeBlock);
            if (cubeBlock.FatBlock != null)
            {
                base.Hierarchy.AddChild(cubeBlock.FatBlock, false, true);
                this.GridSystems.RegisterInSystems(cubeBlock.FatBlock);
                if (cubeBlock.FatBlock.Render.NeedsDrawFromParent)
                {
                    this.m_blocksForDraw.Add(cubeBlock.FatBlock);
                    cubeBlock.FatBlock.Render.SetVisibilityUpdates(true);
                }
                MyObjectBuilderType typeId = cubeBlock.BlockDefinition.Id.TypeId;
                if (typeId != typeof(MyObjectBuilder_CubeBlock))
                {
                    if (!this.BlocksCounters.ContainsKey(typeId))
                    {
                        this.BlocksCounters.Add(typeId, 0);
                    }
                    MyObjectBuilderType type2 = typeId;
                    this.BlocksCounters[type2] += 1;
                }
            }
            this.m_cubeBlocks.Add(cubeBlock);
            if (cubeBlock.FatBlock != null)
            {
                this.m_fatBlocks.Add(cubeBlock.FatBlock);
            }
            if (!this.m_colorStatistics.ContainsKey(cubeBlock.ColorMaskHSV))
            {
                this.m_colorStatistics.Add(cubeBlock.ColorMaskHSV, 0);
            }
            Vector3 colorMaskHSV = cubeBlock.ColorMaskHSV;
            this.m_colorStatistics[colorMaskHSV] += 1;
            objectBuilder.BlockOrientation.GetMatrix(out matrix);
            MyCubeGridDefinitions.GetRotatedBlockSize(blockDefinition, ref matrix, out vectori3);
            Vector3I.TransformNormal(ref blockDefinition.Center, ref matrix, out vectori5);
            bool flag = true;
            Vector3I pos = cubeBlock.Min;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref cubeBlock.Min, ref cubeBlock.Max);
            while (iterator.IsValid())
            {
                flag &= this.AddCube(cubeBlock, ref pos, matrix, blockDefinition);
                iterator.GetNext(out pos);
            }
            if (this.Physics != null)
            {
                this.Physics.AddBlock(cubeBlock);
            }
            this.FixSkeleton(cubeBlock);
            if (MyFakes.ENABLE_MULTIBLOCK_PART_IDS)
            {
                this.AddMultiBlockInfo(cubeBlock);
            }
            if (!testMerge)
            {
                this.NotifyBlockAdded(cubeBlock);
            }
            else
            {
                MyCubeGrid objA = this.DetectMerge(cubeBlock, null, null, false);
                if ((objA == null) || ReferenceEquals(objA, this))
                {
                    this.NotifyBlockAdded(cubeBlock);
                }
                else
                {
                    cubeBlock = objA.GetCubeBlock(cubeBlock.Position);
                    objA.AdditionalModelGenerators.ForEach(delegate (IMyBlockAdditionalModelGenerator g) {
                        g.BlockAddedToMergedGrid(cubeBlock);
                    });
                }
            }
            cubeBlock.AddAuthorship();
            this.m_PCU += cubeBlock.ComponentStack.IsFunctional ? cubeBlock.BlockDefinition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST;
            if (cubeBlock.FatBlock is MyReactor)
            {
                this.NumberOfReactors++;
            }
            this.MarkForUpdate();
            this.MarkForDraw();
            return cubeBlock;
        }

        public void AddDirtyBone(Vector3I gridPosition, Vector3I boneOffset)
        {
            this.Skeleton.Wrap(ref gridPosition, ref boneOffset);
            Vector3I vectori1 = boneOffset - new Vector3I(1, 1, 1);
            Vector3I start = Vector3I.Min(vectori1, new Vector3I(0, 0, 0));
            Vector3I end = Vector3I.Max(vectori1, new Vector3I(0, 0, 0));
            Vector3I next = start;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref end);
            while (iterator.IsValid())
            {
                this.m_dirtyRegion.AddCube((Vector3I) (gridPosition + next));
                iterator.GetNext(out next);
            }
            this.MarkForDraw();
        }

        internal void AddForDamageApplication(MySlimBlock block)
        {
            this.m_blocksForDamageApplication.Add(block);
            this.m_blocksForDamageApplicationDirty = true;
            this.MarkForUpdate();
        }

        internal void AddGroup(MyBlockGroup group)
        {
            foreach (MyBlockGroup group2 in this.BlockGroups)
            {
                if (group2.Name.CompareTo(group.Name) == 0)
                {
                    this.BlockGroups.Remove(group2);
                    group.Blocks.UnionWith(group2.Blocks);
                    break;
                }
            }
            this.BlockGroups.Add(group);
            this.GridSystems.AddGroup(group);
        }

        private void AddGroup(MyObjectBuilder_BlockGroup groupBuilder)
        {
            if (groupBuilder.Blocks.Count != 0)
            {
                MyBlockGroup item = new MyBlockGroup();
                item.Init(this, groupBuilder);
                this.BlockGroups.Add(item);
            }
        }

        public void AddMissingBlocksInMultiBlock(int multiBlockId, long toolOwnerId)
        {
            try
            {
                MatrixI xi;
                MyCubeGridMultiBlockInfo info;
                if (this.GetMissingBlocksMultiBlock(multiBlockId, out info, out xi, m_tmpMultiBlockIndices))
                {
                    this.BuildMultiBlocks(info, ref xi, m_tmpMultiBlockIndices, toolOwnerId);
                }
            }
            finally
            {
                m_tmpMultiBlockIndices.Clear();
            }
        }

        internal void AddMultiBlockInfo(MySlimBlock block)
        {
            MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
            if (fatBlock != null)
            {
                foreach (MySlimBlock block3 in fatBlock.GetBlocks())
                {
                    if (block3.IsMultiBlockPart)
                    {
                        this.AddMultiBlockInfo(block3);
                    }
                }
            }
            else if (block.IsMultiBlockPart)
            {
                MyCubeGridMultiBlockInfo info;
                if (this.m_multiBlockInfos == null)
                {
                    this.m_multiBlockInfos = new Dictionary<int, MyCubeGridMultiBlockInfo>();
                }
                if (!this.m_multiBlockInfos.TryGetValue(block.MultiBlockId, out info))
                {
                    info = new MyCubeGridMultiBlockInfo {
                        MultiBlockId = block.MultiBlockId,
                        MultiBlockDefinition = block.MultiBlockDefinition,
                        MainBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinitionForMultiBlock(block.MultiBlockDefinition.Id.SubtypeName)
                    };
                    this.m_multiBlockInfos.Add(block.MultiBlockId, info);
                }
                info.Blocks.Add(block);
            }
        }

        private void AfterBuildBlocksSuccess(HashSet<MyBlockLocation> builtBlocks, bool instantBuild)
        {
            foreach (MyBlockLocation location in builtBlocks)
            {
                this.AfterBuildBlockSuccess(location, instantBuild);
                MySlimBlock cubeBlock = this.GetCubeBlock(location.CenterPos);
                this.DetectMerge(cubeBlock, null, null, false);
            }
        }

        private void AfterBuildBlockSuccess(MyBlockLocation builtBlock, bool instantBuild)
        {
            MySlimBlock cubeBlock = this.GetCubeBlock(builtBlock.CenterPos);
            if ((cubeBlock != null) && (cubeBlock.FatBlock != null))
            {
                cubeBlock.FatBlock.OnBuildSuccess(builtBlock.Owner, instantBuild);
            }
        }

        private static void AfterPaste(List<MyCubeGrid> grids, Vector3 objectVelocity, bool detectDisconnects)
        {
            foreach (MyCubeGrid grid in grids)
            {
                if (grid.IsStatic)
                {
                    grid.TestDynamic = MyTestDynamicReason.GridCopied;
                }
                Sandbox.Game.Entities.MyEntities.Add(grid, true);
                if (grid.Physics != null)
                {
                    if (!grid.IsStatic)
                    {
                        grid.Physics.LinearVelocity = objectVelocity;
                    }
                    if ((!grid.IsStatic && ((MySession.Static.ControlledEntity != null) && (MySession.Static.ControlledEntity.Entity.Physics != null))) && (MySession.Static.ControlledEntity != null))
                    {
                        grid.Physics.AngularVelocity = MySession.Static.ControlledEntity.Entity.Physics.AngularVelocity;
                    }
                }
                if (detectDisconnects)
                {
                    grid.DetectDisconnectsAfterFrame();
                }
                if (grid.IsStatic)
                {
                    foreach (MySlimBlock block in grid.CubeBlocks)
                    {
                        if (grid.DetectMerge(block, null, null, true) != null)
                        {
                            break;
                        }
                    }
                }
            }
            MatrixD worldMatrix = grids[0].PositionComp.WorldMatrix;
            bool flag = MyCoordinateSystem.Static.IsLocalCoordSysExist(ref worldMatrix, (double) grids[0].GridSize);
            if (grids[0].GridSizeEnum == MyCubeSize.Large)
            {
                if (flag)
                {
                    MyCoordinateSystem.Static.RegisterCubeGrid(grids[0]);
                }
                else
                {
                    MyCoordinateSystem.Static.CreateCoordSys(grids[0], MyClipboardComponent.ClipboardDefinition.PastingSettings.StaticGridAlignToCenter, true);
                }
            }
        }

        public void AnnounceRemoveSplit(List<MySlimBlock> blocks)
        {
            m_tmpPositionListSend.Clear();
            foreach (MySlimBlock block in blocks)
            {
                m_tmpPositionListSend.Add(block.Position);
            }
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, List<Vector3I>>(this, x => new Action<List<Vector3I>>(x.OnRemoveSplit), m_tmpPositionListSend, targetEndpoint);
        }

        private void ApplyDeformationPostponed()
        {
            if (this.m_deformationPostponed.Count > 0)
            {
                List<DeformationPostponedItem> cloned = this.m_deformationPostponed;
                Parallel.Start(delegate {
                    foreach (DeformationPostponedItem item in cloned)
                    {
                        this.ApplyDestructionDeformationInternal(item);
                    }
                    cloned.Clear();
                    m_postponedListsPool.Return(cloned);
                });
                this.m_deformationPostponed = m_postponedListsPool.Get();
                this.m_deformationPostponed.Clear();
            }
        }

        public void ApplyDestructionDeformation(MySlimBlock block, float damage = 1f, MyHitInfo? hitInfo = new MyHitInfo?(), long attackerId = 0L)
        {
            if (MyPerGameSettings.Destruction)
            {
                block.DoDamage(damage, MyDamageType.Deformation, true, hitInfo, attackerId);
            }
            else
            {
                this.EnqueueDestructionDeformationBlock(block.Position);
                this.ApplyDestructionDeformationInternal(block, true, damage, attackerId, false);
            }
        }

        private void ApplyDestructionDeformationInternal(DeformationPostponedItem item)
        {
            if (!base.Closed)
            {
                if (m_deformationRng == null)
                {
                    m_deformationRng = new MyRandom();
                }
                Vector3I maxValue = Vector3I.MaxValue;
                Vector3I minValue = Vector3I.MinValue;
                bool flag = false;
                int x = -1;
                while (x <= 1)
                {
                    int z = -1;
                    while (true)
                    {
                        if (z > 1)
                        {
                            x += 2;
                            break;
                        }
                        flag = ((flag | this.MoveCornerBones(item.Min, new Vector3I(x, 0, z), ref maxValue, ref minValue)) | this.MoveCornerBones(item.Min, new Vector3I(x, z, 0), ref maxValue, ref minValue)) | this.MoveCornerBones(item.Min, new Vector3I(0, x, z), ref maxValue, ref minValue);
                        z += 2;
                    }
                }
                if (flag)
                {
                    this.m_dirtyRegion.AddCubeRegion(maxValue, minValue);
                }
                m_deformationRng.SetSeed(item.Position.GetHashCode());
                float angleDeviation = 0.3926991f;
                float gridSizeQuarter = this.GridSizeQuarter;
                Vector3I min = item.Min;
                for (int i = 0; i < 3; i++)
                {
                    Vector3I dirtyMin = Vector3I.MaxValue;
                    Vector3I dirtyMax = Vector3I.MinValue;
                    if (this.ApplyTable(min, MyCubeGridDeformationTables.ThinUpper[i], ref dirtyMin, ref dirtyMax, m_deformationRng, gridSizeQuarter, angleDeviation) | this.ApplyTable(min, MyCubeGridDeformationTables.ThinLower[i], ref dirtyMin, ref dirtyMax, m_deformationRng, gridSizeQuarter, angleDeviation))
                    {
                        dirtyMin -= Vector3I.One;
                        dirtyMax = (Vector3I) (dirtyMax + Vector3I.One);
                        maxValue = min;
                        minValue = min;
                        this.Skeleton.Wrap(ref maxValue, ref dirtyMin);
                        this.Skeleton.Wrap(ref minValue, ref dirtyMax);
                        this.m_dirtyRegion.AddCubeRegion(maxValue, minValue);
                    }
                }
                MySandboxGame.Static.Invoke(() => this.MarkForDraw(), "ApplyDestructionDeformationInternal::MarkForDraw");
            }
        }

        private float ApplyDestructionDeformationInternal(MySlimBlock block, bool sync, float damage = 1f, long attackerId = 0L, bool postponed = false)
        {
            if (!this.BlocksDestructionEnabled)
            {
                return 0f;
            }
            if (block.UseDamageSystem)
            {
                MyDamageInformation info = new MyDamageInformation(true, 1f, MyDamageType.Deformation, attackerId);
                MyDamageSystem.Static.RaiseBeforeDamageApplied(block, ref info);
                if (info.Amount == 0f)
                {
                    return 0f;
                }
            }
            DeformationPostponedItem item = new DeformationPostponedItem {
                Position = block.Position,
                Min = block.Min,
                Max = block.Max
            };
            this.m_totalBoneDisplacement = 0f;
            if (!postponed)
            {
                this.ApplyDestructionDeformationInternal(item);
            }
            else
            {
                this.m_deformationPostponed.Add(item);
                this.MarkForUpdate();
            }
            if (sync)
            {
                MyDamageInformation info = new MyDamageInformation(false, ((this.m_totalBoneDisplacement * this.GridSize) * 10f) * damage, MyDamageType.Deformation, attackerId);
                if (block.UseDamageSystem)
                {
                    MyDamageSystem.Static.RaiseBeforeDamageApplied(block, ref info);
                }
                if (info.Amount > 0f)
                {
                    MyHitInfo? hitInfo = null;
                    block.DoDamage(info.Amount, MyDamageType.Deformation, true, hitInfo, attackerId);
                }
            }
            return this.m_totalBoneDisplacement;
        }

        public override void ApplyLastControls()
        {
            if (this.m_lastNetState.Valid)
            {
                MyShipController shipController = this.GridSystems.ControlSystem.GetShipController();
                if ((shipController != null) && !shipController.ControllerInfo.IsLocallyControlled())
                {
                    shipController.SetNetState(this.m_lastNetState);
                }
            }
        }

        private bool ApplyTable(Vector3I cubePos, MyCubeGridDeformationTables.DeformationTable table, ref Vector3I dirtyMin, ref Vector3I dirtyMax, MyRandom random, float maxLinearDeviation, float angleDeviation)
        {
            if (this.m_cubes.ContainsKey(cubePos + table.Normal))
            {
                return false;
            }
            float maxValue = this.GridSize / 10f;
            using (MyUtils.ReuseCollection<Vector3I, MySlimBlock>(ref m_tmpCubeSet))
            {
                this.GetExistingCubes(cubePos, table.CubeOffsets, m_tmpCubeSet);
                int num2 = 0;
                if (m_tmpCubeSet.Count > 0)
                {
                    foreach (KeyValuePair<Vector3I, Matrix> pair in table.OffsetTable)
                    {
                        Vector3I key = pair.Key >> 1;
                        Vector3I vectori3 = (pair.Key - Vector3I.One) >> 1;
                        if (!m_tmpCubeSet.ContainsKey(key))
                        {
                            if (!(key != vectori3))
                            {
                                continue;
                            }
                            if (!m_tmpCubeSet.ContainsKey(vectori3))
                            {
                                continue;
                            }
                        }
                        Vector3I boneOffset = pair.Key;
                        Vector3 clamp = new Vector3(this.GridSizeQuarter - random.NextFloat(0f, maxValue));
                        Matrix matrix = pair.Value;
                        Vector3 moveDirection = random.NextDeviatingVector(ref matrix, angleDeviation) * random.NextFloat(1f, maxLinearDeviation);
                        float displacementLength = moveDirection.Max();
                        this.MoveBone(ref cubePos, ref boneOffset, ref moveDirection, ref displacementLength, ref clamp);
                        num2++;
                    }
                }
                m_tmpCubeSet.Clear();
            }
            dirtyMin = Vector3I.Min(dirtyMin, table.MinOffset);
            dirtyMax = Vector3I.Max(dirtyMax, table.MaxOffset);
            return true;
        }

        [Conditional("DEBUG")]
        private void AssertNonPublicBlock(MyObjectBuilder_CubeBlock block)
        {
            MyCubeBlockDefinition definition;
            MyObjectBuilder_CompoundCubeBlock block2 = this.UpgradeCubeBlock(block, out definition) as MyObjectBuilder_CompoundCubeBlock;
            if (block2 == null)
            {
                MyCubeBlockDefinition definition1 = definition;
            }
            else
            {
                MyObjectBuilder_CubeBlock[] blocks = block2.Blocks;
                for (int i = 0; i < blocks.Length; i++)
                {
                    MyObjectBuilder_CubeBlock block1 = blocks[i];
                }
            }
        }

        [Conditional("DEBUG")]
        private void AssertNonPublicBlocks(MyObjectBuilder_CubeGrid builder)
        {
            foreach (MyObjectBuilder_CubeBlock local1 in builder.CubeBlocks)
            {
            }
        }

        protected override void BeforeDelete()
        {
            this.SendRemovedBlocks();
            this.SendRemovedBlocksWithIds();
            this.RemoveAuthorshipAll();
            this.m_cubes.Clear();
            this.m_targetingList.Clear();
            if ((MyFakes.ENABLE_NEW_SOUNDS && MySession.Static.Settings.RealisticSound) && MyFakes.ENABLE_NEW_SOUNDS_QUICK_UPDATE)
            {
                MyEntity3DSoundEmitter.UpdateEntityEmitters(true, false, false);
            }
            Sandbox.Game.Entities.MyEntities.Remove(this);
            this.UnregisterBlocksBeforeClose();
            this.Render.CloseModelGenerators();
            base.BeforeDelete();
            GridCounter--;
        }

        [Event(null, 0x16f3), Reliable, Broadcast]
        private void BlockIntegrityChanged(Vector3I pos, ushort subBlockId, float buildIntegrity, float integrity, MyIntegrityChangeEnum integrityChangeType, long grinderOwner)
        {
            MyCompoundCubeBlock fatBlock = null;
            MySlimBlock cubeBlock = this.GetCubeBlock(pos);
            if (cubeBlock != null)
            {
                fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
            }
            if (fatBlock != null)
            {
                cubeBlock = fatBlock.GetBlock(subBlockId);
            }
            if (cubeBlock != null)
            {
                cubeBlock.SetIntegrity(buildIntegrity, integrity, integrityChangeType, grinderOwner);
            }
        }

        private void BlocksDeformed(List<Vector3I> blockToDestroy)
        {
            foreach (Vector3I vectori in blockToDestroy)
            {
                MySlimBlock cubeBlock = this.GetCubeBlock(vectori);
                if (cubeBlock != null)
                {
                    this.ApplyDestructionDeformationInternal(cubeBlock, false, 1f, 0L, false);
                    this.Physics.AddDirtyBlock(cubeBlock);
                }
            }
        }

        private void BlocksDestroyed(List<Vector3I> blockToDestroy)
        {
            this.m_largeDestroyInProgress = blockToDestroy.Count > BLOCK_LIMIT_FOR_LARGE_DESTRUCTION;
            foreach (Vector3I vectori in blockToDestroy)
            {
                MySlimBlock cubeBlock = this.GetCubeBlock(vectori);
                if (cubeBlock != null)
                {
                    this.RemoveDestroyedBlockInternal(cubeBlock);
                    this.Physics.AddDirtyBlock(cubeBlock);
                }
            }
            this.m_largeDestroyInProgress = false;
        }

        private void BlocksRemoved(List<Vector3I> blocksToRemove)
        {
            foreach (Vector3I vectori in blocksToRemove)
            {
                MySlimBlock cubeBlock = this.GetCubeBlock(vectori);
                if (cubeBlock != null)
                {
                    this.RemoveBlockInternal(cubeBlock, true, true);
                    this.Physics.AddDirtyBlock(cubeBlock);
                }
            }
        }

        private void BlocksRemovedWithGenerator(List<Vector3I> blocksToRemove)
        {
            bool enable = this.EnableGenerators(true, true);
            this.BlocksRemoved(blocksToRemove);
            this.EnableGenerators(enable, true);
        }

        private void BlocksRemovedWithoutGenerator(List<Vector3I> blocksToRemove)
        {
            bool enable = this.EnableGenerators(false, true);
            this.BlocksRemoved(blocksToRemove);
            this.EnableGenerators(enable, true);
        }

        [Event(null, 0x1704), Reliable, Broadcast]
        private void BlockStockpileChanged(Vector3I pos, ushort subBlockId, List<MyStockpileItem> items)
        {
            MySlimBlock cubeBlock = this.GetCubeBlock(pos);
            MyCompoundCubeBlock fatBlock = null;
            if (cubeBlock != null)
            {
                fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
            }
            if (fatBlock != null)
            {
                cubeBlock = fatBlock.GetBlock(subBlockId);
            }
            if (cubeBlock != null)
            {
                cubeBlock.ChangeStockpile(items);
            }
        }

        private void BlocksWithIdDestroyedWithGenerator(List<BlockPositionId> blocksToRemove)
        {
            bool enable = this.EnableGenerators(true, true);
            this.BlocksWithIdRemoved(blocksToRemove);
            this.EnableGenerators(enable, true);
        }

        private void BlocksWithIdDestroyedWithoutGenerator(List<BlockPositionId> blocksToRemove)
        {
            bool enable = this.EnableGenerators(false, true);
            this.BlocksWithIdRemoved(blocksToRemove);
            this.EnableGenerators(enable, true);
        }

        private void BlocksWithIdRemoved(List<BlockPositionId> blocksToRemove)
        {
            foreach (BlockPositionId id in blocksToRemove)
            {
                if (id.CompoundId > 0xffff)
                {
                    MySlimBlock cubeBlock = this.GetCubeBlock(id.Position);
                    if (cubeBlock == null)
                    {
                        continue;
                    }
                    this.RemoveBlockInternal(cubeBlock, true, true);
                    this.Physics.AddDirtyBlock(cubeBlock);
                    continue;
                }
                Vector3I maxValue = Vector3I.MaxValue;
                Vector3I minValue = Vector3I.MinValue;
                this.RemoveBlockInCompound(id.Position, (ushort) id.CompoundId, ref maxValue, ref minValue, null);
                if (maxValue != Vector3I.MaxValue)
                {
                    this.Physics.AddDirtyArea(maxValue, minValue);
                }
            }
        }

        private void BlocksWithIdRemovedWithGenerator(List<BlockPositionId> blocksToRemove)
        {
            bool enable = this.EnableGenerators(true, true);
            this.BlocksWithIdRemoved(blocksToRemove);
            this.EnableGenerators(enable, true);
        }

        private void BlocksWithIdRemovedWithoutGenerator(List<BlockPositionId> blocksToRemove)
        {
            bool enable = this.EnableGenerators(false, true);
            this.BlocksWithIdRemoved(blocksToRemove);
            this.EnableGenerators(enable, true);
        }

        private void BoundsInclude(MySlimBlock block)
        {
            if (block != null)
            {
                this.m_min = Vector3I.Min(this.m_min, block.Min);
                this.m_max = Vector3I.Max(this.m_max, block.Max);
            }
        }

        private void BoundsIncludeUpdateAABB(MySlimBlock block)
        {
            this.BoundsInclude(block);
            this.UpdateGridAABB();
        }

        public static bool BreakGridGroupLink(GridLinkTypeEnum type, long linkId, MyCubeGrid parent, MyCubeGrid child) => 
            MyCubeGridGroups.Static.BreakLink(type, linkId, parent, child);

        private MySlimBlock BuildBlock(MyCubeBlockDefinition blockDefinition, Vector3 colorMaskHsv, Vector3I min, Quaternion orientation, long owner, long entityId, VRage.Game.Entity.MyEntity builderEntity, MyObjectBuilder_CubeBlock blockObjectBuilder = null, bool updateVolume = true, bool testMerge = true, bool buildAsAdmin = false)
        {
            MyBlockOrientation orientation2 = new MyBlockOrientation(ref orientation);
            if (blockObjectBuilder == null)
            {
                blockObjectBuilder = CreateBlockObjectBuilder(blockDefinition, min, orientation2, entityId, owner, ((builderEntity == null) || !MySession.Static.SurvivalMode) | buildAsAdmin);
                blockObjectBuilder.ColorMaskHSV = colorMaskHsv;
            }
            else
            {
                blockObjectBuilder.Min = min;
                blockObjectBuilder.Orientation = orientation;
            }
            MyCubeBuilder.BuildComponent.BeforeCreateBlock(blockDefinition, builderEntity, blockObjectBuilder, buildAsAdmin);
            MySlimBlock block = null;
            Vector3I gridCoords = MySlimBlock.ComputePositionInGrid(new MatrixI(orientation2), blockDefinition, min);
            if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(this.GridIntegerToWorld(gridCoords)))
            {
                return null;
            }
            if (Sync.IsServer)
            {
                MyCubeBuilder.BuildComponent.GetBlockPlacementMaterials(blockDefinition, gridCoords, (MyBlockOrientation) blockObjectBuilder.BlockOrientation, this);
            }
            if (!MyFakes.ENABLE_COMPOUND_BLOCKS || !MyCompoundCubeBlock.IsCompoundEnabled(blockDefinition))
            {
                block = this.AddBlock(blockObjectBuilder, testMerge);
            }
            else
            {
                MySlimBlock cubeBlock = this.GetCubeBlock(min);
                MyCompoundCubeBlock block3 = (cubeBlock != null) ? (cubeBlock.FatBlock as MyCompoundCubeBlock) : null;
                if (block3 == null)
                {
                    MyObjectBuilder_CompoundCubeBlock objectBuilder = MyCompoundCubeBlock.CreateBuilder(blockObjectBuilder);
                    block = this.AddBlock(objectBuilder, testMerge);
                }
                else if (block3.CanAddBlock(blockDefinition, new MyBlockOrientation(ref orientation), 0, false))
                {
                    ushort num;
                    object obj2 = MyCubeBlockFactory.CreateCubeBlock(blockObjectBuilder);
                    block = obj2 as MySlimBlock;
                    if (block == null)
                    {
                        block = new MySlimBlock();
                    }
                    block.Init(blockObjectBuilder, this, obj2 as MyCubeBlock);
                    block.FatBlock.HookMultiplayer();
                    if (block3.Add(block, out num))
                    {
                        this.BoundsInclude(block);
                        this.m_dirtyRegion.AddCube(min);
                        if (this.Physics != null)
                        {
                            this.Physics.AddDirtyBlock(cubeBlock);
                        }
                        this.NotifyBlockAdded(block);
                    }
                }
                this.MarkForDraw();
            }
            if (block != null)
            {
                block.CubeGrid.BoundsInclude(block);
                if (updateVolume)
                {
                    block.CubeGrid.UpdateGridAABB();
                }
                if ((MyCubeGridSmallToLargeConnection.Static != null) && this.m_enableSmallToLargeConnections)
                {
                    MyCubeGridSmallToLargeConnection.Static.AddBlockSmallToLargeConnection(block);
                }
                if (Sync.IsServer)
                {
                    MyCubeBuilder.BuildComponent.AfterSuccessfulBuild(builderEntity, buildAsAdmin);
                }
                MyCubeGrids.NotifyBlockBuilt(this, block);
            }
            return block;
        }

        [Event(null, 0xe91), Reliable, Server]
        public void BuildBlockRequest(uint colorMaskHsv, MyBlockLocation location, [DynamicObjectBuilder(false)] MyObjectBuilder_CubeBlock blockObjectBuilder, long builderEntityId, bool instantBuild, long ownerId)
        {
            VRage.Game.Entity.MyEntity entity = null;
            Sandbox.Game.Entities.MyEntities.TryGetEntityById(builderEntityId, out entity, false);
            bool flag = MyEventContext.Current.IsLocallyInvoked || MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value);
            if ((((entity != null) || flag) || MySession.Static.CreativeMode) && MySessionComponentSafeZones.IsActionAllowed(this, MySafeZoneAction.Building, builderEntityId))
            {
                if (!MySession.Static.GetComponent<MySessionComponentDLC>().HasDefinitionDLC((MyDefinitionId) location.BlockDefinition, MyEventContext.Current.Sender.Value) || ((MySession.Static.ResearchEnabled && !flag) && !MySessionComponentResearch.Static.CanUse(ownerId, (MyDefinitionId) location.BlockDefinition)))
                {
                    (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                }
                else
                {
                    MyCubeBlockDefinition definition;
                    Quaternion quaternion;
                    int? nullable1;
                    MyBlockLocation? resultBlock = null;
                    MyDefinitionManager.Static.TryGetCubeBlockDefinition((MyDefinitionId) location.BlockDefinition, out definition);
                    MyBlockOrientation orientation = location.Orientation;
                    location.Orientation.GetQuaternion(out quaternion);
                    MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = definition.GetBuildProgressModelMountPoints(MyComponentStack.NewBlockIntegrity);
                    if ((blockObjectBuilder != null) && (blockObjectBuilder.MultiBlockId != 0))
                    {
                        nullable1 = new int?(blockObjectBuilder.MultiBlockId);
                    }
                    else
                    {
                        nullable1 = null;
                    }
                    Vector3I centerPos = location.CenterPos;
                    if (this.CanPlaceBlock(location.Min, location.Max, orientation, definition, nullable1, false) && CheckConnectivity(this, definition, buildProgressModelMountPoints, ref quaternion, ref centerPos))
                    {
                        this.BuildBlockSuccess(ColorExtensions.UnpackHSVFromUint(colorMaskHsv), location, blockObjectBuilder, ref resultBlock, entity, flag & instantBuild, ownerId);
                        if (resultBlock != null)
                        {
                            EndpointId targetEndpoint = new EndpointId();
                            MyMultiplayer.RaiseEvent<MyCubeGrid, uint, MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>(this, x => new Action<uint, MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>(x.BuildBlockSucess), colorMaskHsv, location, blockObjectBuilder, builderEntityId, flag & instantBuild, ownerId, targetEndpoint);
                            this.AfterBuildBlockSuccess(resultBlock.Value, instantBuild);
                        }
                    }
                }
            }
        }

        public void BuildBlocks(ref MyBlockBuildArea area, long builderEntityId, long ownerId)
        {
            int blocksToBuild = (area.BuildAreaSize.X * area.BuildAreaSize.Y) * area.BuildAreaSize.Z;
            MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition((MyDefinitionId) area.DefinitionId);
            if (MySession.Static.CheckLimitsAndNotify(ownerId, cubeBlockDefinition.BlockPairName, blocksToBuild * cubeBlockDefinition.PCU, blocksToBuild, this.BlocksCount, null) && MySession.Static.GetComponent<MySessionComponentDLC>().HasDefinitionDLC(cubeBlockDefinition, MySession.Static.Players.TryGetSteamId(ownerId)))
            {
                bool flag = MySession.Static.CreativeToolsEnabled(Sync.MyId);
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, MyBlockBuildArea, long, bool, long>(this, x => new Action<MyBlockBuildArea, long, bool, long>(x.BuildBlocksAreaRequest), area, builderEntityId, flag, ownerId, targetEndpoint);
            }
        }

        public void BuildBlocks(Vector3 colorMaskHsv, HashSet<MyBlockLocation> locations, long builderEntityId, long ownerId)
        {
            MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition((MyDefinitionId) locations.First<MyBlockLocation>().BlockDefinition);
            string blockPairName = cubeBlockDefinition.BlockPairName;
            bool flag = MySession.Static.CreativeToolsEnabled(Sync.MyId);
            if (MySession.Static.CheckLimitsAndNotify(ownerId, blockPairName, (flag || MySession.Static.CreativeMode) ? (locations.Count * cubeBlockDefinition.PCU) : locations.Count, locations.Count, this.BlocksCount, null) && MySession.Static.GetComponent<MySessionComponentDLC>().HasDefinitionDLC(cubeBlockDefinition, MySession.Static.Players.TryGetSteamId(ownerId)))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, uint, HashSet<MyBlockLocation>, long, bool, long>(this, x => new Action<uint, HashSet<MyBlockLocation>, long, bool, long>(x.BuildBlocksRequest), colorMaskHsv.PackHSVToUint(), locations, builderEntityId, flag, ownerId, targetEndpoint);
            }
        }

        private void BuildBlocksArea(ref MyBlockBuildArea area, List<Vector3UByte> validOffsets, long builderEntityId, bool isAdmin, long ownerId, int entityIdSeed)
        {
            MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition((MyDefinitionId) area.DefinitionId);
            if (cubeBlockDefinition != null)
            {
                Quaternion orientation = Base6Directions.GetOrientation(area.OrientationForward, area.OrientationUp);
                Vector3I stepDelta = (Vector3I) area.StepDelta;
                VRage.Game.Entity.MyEntity entity = null;
                Sandbox.Game.Entities.MyEntities.TryGetEntityById(builderEntityId, out entity, false);
                try
                {
                    bool flag = false;
                    validOffsets.Sort(Vector3UByte.Comparer);
                    using (MyRandom.Instance.PushSeed(entityIdSeed))
                    {
                        foreach (Vector3UByte num in validOffsets)
                        {
                            Vector3I vectori2 = (Vector3I) (area.PosInGrid + (num * stepDelta));
                            MySlimBlock block = this.BuildBlock(cubeBlockDefinition, ColorExtensions.UnpackHSVFromUint(area.ColorMaskHSV), (Vector3I) (vectori2 + area.BlockMin), orientation, ownerId, MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM), entity, null, false, false, isAdmin);
                            if (block != null)
                            {
                                ChangeBlockOwner(block, ownerId);
                                flag = true;
                                this.m_tmpBuildSuccessBlocks.Add(block);
                                if (ownerId == MySession.Static.LocalPlayerId)
                                {
                                    MySession @static = MySession.Static;
                                    @static.TotalBlocksCreated++;
                                    if (MySession.Static.ControlledEntity is MyCockpit)
                                    {
                                        MySession session2 = MySession.Static;
                                        session2.TotalBlocksCreatedFromShips++;
                                    }
                                }
                            }
                        }
                    }
                    BoundingBoxD boundingBox = BoundingBoxD.CreateInvalid();
                    foreach (MySlimBlock block2 in this.m_tmpBuildSuccessBlocks)
                    {
                        BoundingBoxD xd2;
                        block2.GetWorldBoundingBox(out xd2, false);
                        boundingBox.Include(xd2);
                        if (block2.FatBlock != null)
                        {
                            block2.FatBlock.OnBuildSuccess(ownerId, isAdmin);
                        }
                    }
                    if (this.m_tmpBuildSuccessBlocks.Count > 0)
                    {
                        if (this.IsStatic && Sync.IsServer)
                        {
                            List<VRage.Game.Entity.MyEntity> entitiesInAABB = Sandbox.Game.Entities.MyEntities.GetEntitiesInAABB(ref boundingBox, false);
                            foreach (MySlimBlock block3 in this.m_tmpBuildSuccessBlocks)
                            {
                                this.DetectMerge(block3, null, entitiesInAABB, false);
                            }
                            entitiesInAABB.Clear();
                        }
                        this.m_tmpBuildSuccessBlocks[0].PlayConstructionSound(MyIntegrityChangeEnum.ConstructionBegin, false);
                        this.UpdateGridAABB();
                    }
                    if (MySession.Static.LocalPlayerId == ownerId)
                    {
                        if (flag)
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.HudPlaceBlock);
                        }
                        else
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                        }
                    }
                }
                finally
                {
                    this.m_tmpBuildSuccessBlocks.Clear();
                }
            }
        }

        [Event(null, 0xf77), Reliable, Broadcast]
        private void BuildBlocksAreaClient(MyBlockBuildArea area, int entityIdSeed, HashSet<Vector3UByte> failList, long builderEntityId, bool isAdmin, long ownerId)
        {
            try
            {
                this.GetAllBuildOffsetsExcept(ref area, failList, this.m_tmpBuildOffsets);
                this.BuildBlocksArea(ref area, this.m_tmpBuildOffsets, builderEntityId, isAdmin, ownerId, entityIdSeed);
            }
            finally
            {
                this.m_tmpBuildOffsets.Clear();
            }
        }

        [Event(null, 0xf42), Reliable, Server]
        private void BuildBlocksAreaRequest(MyBlockBuildArea area, long builderEntityId, bool instantBuild, long ownerId)
        {
            if (!MySession.Static.CreativeMode)
            {
                MyEventContext current = MyEventContext.Current;
                if (!current.IsLocallyInvoked && !MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value))
                {
                    instantBuild = false;
                }
            }
            try
            {
                bool flag = MySession.Static.CreativeToolsEnabled(MyEventContext.Current.Sender.Value) || MySession.Static.CreativeMode;
                bool flag2 = MyEventContext.Current.IsLocallyInvoked || MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value);
                if ((((ownerId != 0) || flag2) || MySession.Static.CreativeMode) && MySessionComponentSafeZones.IsActionAllowed(this, MySafeZoneAction.Building, builderEntityId))
                {
                    MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition((MyDefinitionId) area.DefinitionId);
                    int blocksToBuild = (area.BuildAreaSize.X * area.BuildAreaSize.Y) * area.BuildAreaSize.Z;
                    if (this.IsWithinWorldLimits(ownerId, blocksToBuild, flag ? (blocksToBuild * cubeBlockDefinition.PCU) : blocksToBuild, cubeBlockDefinition.BlockPairName))
                    {
                        MyCubeBuilder.BuildComponent.GetBlockAmountPlacementMaterials(cubeBlockDefinition, (area.BuildAreaSize.X * area.BuildAreaSize.Y) * area.BuildAreaSize.Z);
                        VRage.Game.Entity.MyEntity entity = null;
                        Sandbox.Game.Entities.MyEntities.TryGetEntityById(builderEntityId, out entity, false);
                        if (MyCubeBuilder.BuildComponent.HasBuildingMaterials(entity, true) || flag2)
                        {
                            this.GetValidBuildOffsets(ref area, this.m_tmpBuildOffsets, this.m_tmpBuildFailList);
                            CheckAreaConnectivity(this, ref area, this.m_tmpBuildOffsets, this.m_tmpBuildFailList);
                            int num3 = MyRandom.Instance.CreateRandomSeed();
                            EndpointId targetEndpoint = new EndpointId();
                            MyMultiplayer.RaiseEvent<MyCubeGrid, MyBlockBuildArea, int, HashSet<Vector3UByte>, long, bool, long>(this, x => new Action<MyBlockBuildArea, int, HashSet<Vector3UByte>, long, bool, long>(x.BuildBlocksAreaClient), area, num3, this.m_tmpBuildFailList, builderEntityId, flag2, ownerId, targetEndpoint);
                            this.BuildBlocksArea(ref area, this.m_tmpBuildOffsets, builderEntityId, flag2, ownerId, num3);
                        }
                    }
                }
            }
            finally
            {
                this.m_tmpBuildOffsets.Clear();
                this.m_tmpBuildFailList.Clear();
            }
        }

        [Event(null, 0xf35), Reliable, Broadcast]
        public void BuildBlocksClient(uint colorMaskHsv, HashSet<MyBlockLocation> locations, long builderEntityId, bool instantBuild, long ownerId)
        {
            m_tmpBuildList.Clear();
            VRage.Game.Entity.MyEntity entity = null;
            Sandbox.Game.Entities.MyEntities.TryGetEntityById(builderEntityId, out entity, false);
            this.BuildBlocksSuccess(ColorExtensions.UnpackHSVFromUint(colorMaskHsv), locations, m_tmpBuildList, entity, instantBuild, ownerId);
            if (ownerId == MySession.Static.LocalPlayerId)
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudPlaceBlock);
            }
            this.AfterBuildBlocksSuccess(m_tmpBuildList, instantBuild);
        }

        [Event(null, 0xf2e), Reliable, Client]
        public void BuildBlocksFailedNotify()
        {
            if (MyCubeBuilder.Static != null)
            {
                MyCubeBuilder.Static.NotifyPlacementUnable();
            }
        }

        [Event(null, 0xef2), Reliable, Server]
        private void BuildBlocksRequest(uint colorMaskHsv, HashSet<MyBlockLocation> locations, long builderEntityId, bool instantBuild, long ownerId)
        {
            if ((!MySession.Static.CreativeMode && !MyEventContext.Current.IsLocallyInvoked) && !MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value))
            {
                instantBuild = false;
            }
            m_tmpBuildList.Clear();
            VRage.Game.Entity.MyEntity entity = null;
            Sandbox.Game.Entities.MyEntities.TryGetEntityById(builderEntityId, out entity, false);
            MyCubeBuilder.BuildComponent.GetBlocksPlacementMaterials(locations, this);
            bool flag = MySession.Static.CreativeToolsEnabled(MyEventContext.Current.Sender.Value) || MySession.Static.CreativeMode;
            bool flag2 = MyEventContext.Current.IsLocallyInvoked || MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value);
            if (((((entity != null) || flag2) || MySession.Static.CreativeMode) && MySessionComponentSafeZones.IsActionAllowed(this, MySafeZoneAction.Building, builderEntityId)) && (MyCubeBuilder.BuildComponent.HasBuildingMaterials(entity, false) || flag2))
            {
                MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition((MyDefinitionId) locations.First<MyBlockLocation>().BlockDefinition);
                string blockPairName = cubeBlockDefinition.BlockPairName;
                if (this.IsWithinWorldLimits(ownerId, locations.Count, flag ? (locations.Count * cubeBlockDefinition.PCU) : locations.Count, blockPairName))
                {
                    Vector3 vector = ColorExtensions.UnpackHSVFromUint(colorMaskHsv);
                    this.BuildBlocksSuccess(vector, locations, m_tmpBuildList, entity, flag2 & instantBuild, ownerId);
                    if (m_tmpBuildList.Count <= 0)
                    {
                        MyMultiplayer.RaiseEvent<MyCubeGrid>(this, x => new Action(x.BuildBlocksFailedNotify), new EndpointId(MyEventContext.Current.Sender.Value));
                    }
                    else
                    {
                        MySession @static = MySession.Static;
                        @static.TotalBlocksCreated += (uint) m_tmpBuildList.Count;
                        if (MySession.Static.ControlledEntity is MyCockpit)
                        {
                            MySession session2 = MySession.Static;
                            session2.TotalBlocksCreatedFromShips += (uint) m_tmpBuildList.Count;
                        }
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyCubeGrid, uint, HashSet<MyBlockLocation>, long, bool, long>(this, x => new Action<uint, HashSet<MyBlockLocation>, long, bool, long>(x.BuildBlocksClient), colorMaskHsv, m_tmpBuildList, builderEntityId, flag2 & instantBuild, ownerId, targetEndpoint);
                        if ((Sync.IsServer && !Sandbox.Engine.Platform.Game.IsDedicated) && (MySession.Static.LocalPlayerId == ownerId))
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.HudPlaceBlock);
                        }
                    }
                    this.AfterBuildBlocksSuccess(m_tmpBuildList, instantBuild);
                }
            }
        }

        private void BuildBlocksSuccess(Vector3 colorMaskHsv, HashSet<MyBlockLocation> locations, HashSet<MyBlockLocation> resultBlocks, VRage.Game.Entity.MyEntity builder, bool instantBuilt, long ownerId)
        {
            bool flag = true;
            while (true)
            {
                if ((locations.Count > 0) & flag)
                {
                    flag = false;
                    HashSet<MyBlockLocation>.Enumerator enumerator = locations.GetEnumerator();
                    try
                    {
                        while (true)
                        {
                            if (enumerator.MoveNext())
                            {
                                Quaternion quaternion;
                                MyCubeBlockDefinition definition;
                                MyBlockLocation current = enumerator.Current;
                                current.Orientation.GetQuaternion(out quaternion);
                                Vector3I centerPos = current.CenterPos;
                                MyDefinitionManager.Static.TryGetCubeBlockDefinition((MyDefinitionId) current.BlockDefinition, out definition);
                                if (definition != null)
                                {
                                    MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = definition.GetBuildProgressModelMountPoints(MyComponentStack.NewBlockIntegrity);
                                    if (Sync.IsServer && !this.CanPlaceWithConnectivity(current, ref quaternion, ref centerPos, definition, buildProgressModelMountPoints))
                                    {
                                        continue;
                                    }
                                    MySlimBlock block = this.BuildBlock(definition, colorMaskHsv, current.Min, quaternion, current.Owner, current.EntityId, builder, null, true, false, instantBuilt);
                                    if (block != null)
                                    {
                                        ChangeBlockOwner(block, ownerId);
                                        MyBlockLocation item = current;
                                        resultBlocks.Add(item);
                                    }
                                    flag = true;
                                    locations.Remove(current);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            break;
                        }
                        continue;
                    }
                    finally
                    {
                        enumerator.Dispose();
                        continue;
                    }
                }
                break;
            }
        }

        private void BuildBlockSuccess(Vector3 colorMaskHsv, MyBlockLocation location, MyObjectBuilder_CubeBlock objectBuilder, ref MyBlockLocation? resultBlock, VRage.Game.Entity.MyEntity builder, bool instantBuilt, long ownerId)
        {
            Quaternion quaternion;
            MyCubeBlockDefinition definition;
            location.Orientation.GetQuaternion(out quaternion);
            MyDefinitionManager.Static.TryGetCubeBlockDefinition((MyDefinitionId) location.BlockDefinition, out definition);
            if (definition != null)
            {
                MySlimBlock block = this.BuildBlock(definition, colorMaskHsv, location.Min, quaternion, location.Owner, location.EntityId, instantBuilt ? null : builder, objectBuilder, true, true, false);
                if (block == null)
                {
                    resultBlock = 0;
                }
                else
                {
                    ChangeBlockOwner(block, ownerId);
                    resultBlock = new MyBlockLocation?(location);
                    block.PlayConstructionSound(MyIntegrityChangeEnum.ConstructionBegin, false);
                }
            }
        }

        [Event(null, 0xec1), Reliable, Broadcast]
        public void BuildBlockSucess(uint colorMaskHsv, MyBlockLocation location, [DynamicObjectBuilder(false)] MyObjectBuilder_CubeBlock blockObjectBuilder, long builderEntityId, bool instantBuild, long ownerId)
        {
            VRage.Game.Entity.MyEntity entity = null;
            Sandbox.Game.Entities.MyEntities.TryGetEntityById(builderEntityId, out entity, false);
            MyBlockLocation? resultBlock = null;
            this.BuildBlockSuccess(ColorExtensions.UnpackHSVFromUint(colorMaskHsv), location, blockObjectBuilder, ref resultBlock, entity, instantBuild, ownerId);
            if (resultBlock != null)
            {
                this.AfterBuildBlockSuccess(resultBlock.Value, instantBuild);
            }
        }

        public MySlimBlock BuildGeneratedBlock(MyBlockLocation location, Vector3 colorMaskHsv)
        {
            Quaternion quaternion;
            MyDefinitionId blockDefinition = (MyDefinitionId) location.BlockDefinition;
            MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(blockDefinition);
            location.Orientation.GetQuaternion(out quaternion);
            return this.BuildBlock(cubeBlockDefinition, colorMaskHsv, location.Min, quaternion, location.Owner, location.EntityId, null, null, true, true, false);
        }

        public bool BuildMultiBlocks(MyCubeGridMultiBlockInfo multiBlockInfo, ref MatrixI transform, List<int> multiBlockIndices, long builderEntityId)
        {
            List<MyBlockLocation> collection = new List<MyBlockLocation>();
            List<MyObjectBuilder_CubeBlock> list2 = new List<MyObjectBuilder_CubeBlock>();
            using (List<int>.Enumerator enumerator = multiBlockIndices.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    int current = enumerator.Current;
                    if (current < multiBlockInfo.MultiBlockDefinition.BlockDefinitions.Length)
                    {
                        MyCubeBlockDefinition definition2;
                        bool flag;
                        MyMultiBlockDefinition.MyMultiBlockPartDefinition definition = multiBlockInfo.MultiBlockDefinition.BlockDefinitions[current];
                        if (!MyDefinitionManager.Static.TryGetCubeBlockDefinition(definition.Id, out definition2) || (definition2 == null))
                        {
                            flag = false;
                        }
                        else
                        {
                            MatrixI xi;
                            Vector3I min = Vector3I.Transform(definition.Min, ref transform);
                            MatrixI leftMatrix = new MatrixI(definition.Forward, definition.Up);
                            MatrixI.Multiply(ref leftMatrix, ref transform, out xi);
                            MyBlockOrientation blockOrientation = xi.GetBlockOrientation();
                            if (this.CanPlaceBlock(min, min, blockOrientation, definition2, new int?(multiBlockInfo.MultiBlockId), false))
                            {
                                MyObjectBuilder_CubeBlock item = MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) definition.Id) as MyObjectBuilder_CubeBlock;
                                item.Orientation = Base6Directions.GetOrientation(blockOrientation.Forward, blockOrientation.Up);
                                item.Min = min;
                                item.ColorMaskHSV = MyPlayer.SelectedColor;
                                item.MultiBlockId = multiBlockInfo.MultiBlockId;
                                item.MultiBlockIndex = current;
                                item.MultiBlockDefinition = new SerializableDefinitionId?((SerializableDefinitionId) multiBlockInfo.MultiBlockDefinition.Id);
                                list2.Add(item);
                                MyBlockLocation location = new MyBlockLocation {
                                    Min = min,
                                    Max = min,
                                    CenterPos = min,
                                    Orientation = new MyBlockOrientation(blockOrientation.Forward, blockOrientation.Up),
                                    BlockDefinition = definition.Id,
                                    EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM),
                                    Owner = builderEntityId
                                };
                                collection.Add(location);
                                continue;
                            }
                            flag = false;
                        }
                        return flag;
                    }
                }
            }
            if (MySession.Static.SurvivalMode)
            {
                VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(builderEntityId, false);
                if (entityById == null)
                {
                    return false;
                }
                HashSet<MyBlockLocation> hashSet = new HashSet<MyBlockLocation>(collection);
                MyCubeBuilder.BuildComponent.GetBlocksPlacementMaterials(hashSet, this);
                if (!MyCubeBuilder.BuildComponent.HasBuildingMaterials(entityById, false))
                {
                    return false;
                }
            }
            for (int i = 0; (i < collection.Count) && (i < list2.Count); i++)
            {
                MyBlockLocation location2 = collection[i];
                MyObjectBuilder_CubeBlock block2 = list2[i];
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, uint, MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>(this, x => new Action<uint, MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>(x.BuildBlockRequest), MyPlayer.SelectedColor.PackHSVToUint(), location2, block2, builderEntityId, false, MySession.Static.LocalPlayerId, targetEndpoint);
            }
            return true;
        }

        public MatrixI CalculateMergeTransform(MyCubeGrid gridToMerge, Vector3I gridOffset)
        {
            Vector3 vec = (Vector3) Vector3D.TransformNormal(gridToMerge.WorldMatrix.Forward, base.PositionComp.WorldMatrixNormalizedInv);
            Base6Directions.Direction closestDirection = Base6Directions.GetClosestDirection(vec);
            Base6Directions.Direction up = Base6Directions.GetClosestDirection((Vector3) Vector3D.TransformNormal(gridToMerge.WorldMatrix.Up, base.PositionComp.WorldMatrixNormalizedInv));
            if (up == closestDirection)
            {
                up = Base6Directions.GetPerpendicular(closestDirection);
            }
            return new MatrixI(ref gridOffset, closestDirection, up);
        }

        public bool CanAddCube(Vector3I pos, MyBlockOrientation? orientation, MyCubeBlockDefinition definition, bool ignoreSame = false)
        {
            if (!MyFakes.ENABLE_COMPOUND_BLOCKS || (definition == null))
            {
                return !this.CubeExists(pos);
            }
            if (!this.CubeExists(pos))
            {
                return true;
            }
            MySlimBlock cubeBlock = this.GetCubeBlock(pos);
            if (cubeBlock == null)
            {
                return false;
            }
            MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
            return ((fatBlock != null) && fatBlock.CanAddBlock(definition, orientation, 0, ignoreSame));
        }

        public bool CanAddCubes(Vector3I min, Vector3I max)
        {
            Vector3I key = min;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref min, ref max);
            while (iterator.IsValid())
            {
                if (this.m_cubes.ContainsKey(key))
                {
                    return false;
                }
                iterator.GetNext(out key);
            }
            return true;
        }

        public bool CanAddCubes(Vector3I min, Vector3I max, MyBlockOrientation? orientation, MyCubeBlockDefinition definition)
        {
            if (!MyFakes.ENABLE_COMPOUND_BLOCKS || (definition == null))
            {
                return this.CanAddCubes(min, max);
            }
            Vector3I pos = min;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref min, ref max);
            while (iterator.IsValid())
            {
                if (!this.CanAddCube(pos, orientation, definition, false))
                {
                    return false;
                }
                iterator.GetNext(out pos);
            }
            return true;
        }

        public bool CanAddMissingBlocksInMultiBlock(int multiBlockId)
        {
            bool flag;
            try
            {
                MatrixI xi;
                MyCubeGridMultiBlockInfo info;
                flag = this.GetMissingBlocksMultiBlock(multiBlockId, out info, out xi, m_tmpMultiBlockIndices) ? this.CanAddMultiBlocks(info, ref xi, m_tmpMultiBlockIndices) : false;
            }
            finally
            {
                m_tmpMultiBlockIndices.Clear();
            }
            return flag;
        }

        public bool CanAddMultiBlocks(MyCubeGridMultiBlockInfo multiBlockInfo, ref MatrixI transform, List<int> multiBlockIndices)
        {
            using (List<int>.Enumerator enumerator = multiBlockIndices.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    int current = enumerator.Current;
                    if (current < multiBlockInfo.MultiBlockDefinition.BlockDefinitions.Length)
                    {
                        MyCubeBlockDefinition definition2;
                        bool flag;
                        MyMultiBlockDefinition.MyMultiBlockPartDefinition definition = multiBlockInfo.MultiBlockDefinition.BlockDefinitions[current];
                        if (!MyDefinitionManager.Static.TryGetCubeBlockDefinition(definition.Id, out definition2) || (definition2 == null))
                        {
                            flag = false;
                        }
                        else
                        {
                            MatrixI xi;
                            Vector3I min = Vector3I.Transform(definition.Min, ref transform);
                            MatrixI leftMatrix = new MatrixI(definition.Forward, definition.Up);
                            MatrixI.Multiply(ref leftMatrix, ref transform, out xi);
                            if (this.CanPlaceBlock(min, min, xi.GetBlockOrientation(), definition2, new int?(multiBlockInfo.MultiBlockId), true))
                            {
                                continue;
                            }
                            flag = false;
                        }
                        return flag;
                    }
                }
            }
            return true;
        }

        public bool CanAddOtherBlockInMultiBlock(Vector3I min, Vector3I max, MyBlockOrientation orientation, MyCubeBlockDefinition definition, int? ignoreMultiblockId)
        {
            if (this.m_multiBlockInfos != null)
            {
                using (Dictionary<int, MyCubeGridMultiBlockInfo>.Enumerator enumerator = this.m_multiBlockInfos.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<int, MyCubeGridMultiBlockInfo> current = enumerator.Current;
                        if (((ignoreMultiblockId == null) || (ignoreMultiblockId.Value != current.Key)) && !current.Value.CanAddBlock(ref min, ref max, orientation, definition))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public bool CanBeTeleported(MyGridJumpDriveSystem jumpingSystem, out MyGridJumpDriveSystem.MyJumpFailReason reason)
        {
            reason = MyGridJumpDriveSystem.MyJumpFailReason.None;
            if (MyFixedGrids.IsRooted(this))
            {
                reason = MyGridJumpDriveSystem.MyJumpFailReason.Static;
                return false;
            }
            using (HashSet<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node>.Enumerator enumerator = MyCubeGridGroups.Static.Physical.GetGroup(this).Nodes.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node current = enumerator.Current;
                    if (current.NodeData.Physics != null)
                    {
                        bool flag;
                        if (current.NodeData.IsStatic)
                        {
                            reason = MyGridJumpDriveSystem.MyJumpFailReason.Locked;
                            flag = false;
                        }
                        else
                        {
                            if (!current.NodeData.GridSystems.JumpSystem.IsJumping)
                            {
                                continue;
                            }
                            if (ReferenceEquals(current.NodeData.GridSystems.JumpSystem, jumpingSystem))
                            {
                                continue;
                            }
                            reason = MyGridJumpDriveSystem.MyJumpFailReason.AlreadyJumping;
                            flag = false;
                        }
                        return flag;
                    }
                }
            }
            return true;
        }

        public bool CanHavePhysics()
        {
            if (this.m_canHavePhysics)
            {
                if ((MyPerGameSettings.Game != GameEnum.SE_GAME) && (MyPerGameSettings.Game != GameEnum.VRS_GAME))
                {
                    this.m_canHavePhysics = this.m_cubeBlocks.Count > 0;
                }
                else
                {
                    using (HashSet<MySlimBlock>.Enumerator enumerator = this.m_cubeBlocks.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            if (enumerator.Current.BlockDefinition.HasPhysics)
                            {
                                return true;
                            }
                        }
                    }
                    this.m_canHavePhysics = false;
                }
            }
            return this.m_canHavePhysics;
        }

        public static bool CanHavePhysics(List<MySlimBlock> blocks, int offset, int count)
        {
            if (offset < 0)
            {
                MySandboxGame.Log.WriteLine($"Negative offset in CanHavePhysics - {offset}");
                return false;
            }
            for (int i = offset; (i < (offset + count)) && (i < blocks.Count); i++)
            {
                MySlimBlock block = blocks[i];
                if ((block != null) && block.BlockDefinition.HasPhysics)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CanMergeCubes(MyCubeGrid gridToMerge, Vector3I gridOffset)
        {
            MatrixI transformation = this.CalculateMergeTransform(gridToMerge, gridOffset);
            using (IEnumerator<KeyValuePair<Vector3I, MyCube>> enumerator = gridToMerge.m_cubes.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<Vector3I, MyCube> current = enumerator.Current;
                    Vector3I key = Vector3I.Transform(current.Key, transformation);
                    if (this.m_cubes.ContainsKey(key))
                    {
                        MySlimBlock cubeBlock = this.GetCubeBlock(key);
                        if ((cubeBlock != null) && (cubeBlock.FatBlock is MyCompoundCubeBlock))
                        {
                            MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                            MySlimBlock block3 = gridToMerge.GetCubeBlock(current.Key);
                            if (!(block3.FatBlock is MyCompoundCubeBlock))
                            {
                                MyBlockOrientation orientation2 = MatrixI.Transform(ref block3.Orientation, ref transformation);
                                if (fatBlock.CanAddBlock(block3.BlockDefinition, new MyBlockOrientation?(orientation2), 0, false))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                bool flag = true;
                                foreach (MySlimBlock block4 in (block3.FatBlock as MyCompoundCubeBlock).GetBlocks())
                                {
                                    MyBlockOrientation orientation = MatrixI.Transform(ref block4.Orientation, ref transformation);
                                    if (!fatBlock.CanAddBlock(block4.BlockDefinition, new MyBlockOrientation?(orientation), 0, false))
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                                if (flag)
                                {
                                    continue;
                                }
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CanMoveBlocksFrom(MyCubeGrid grid, Vector3I blockOffset)
        {
            try
            {
                MatrixI transformation = this.CalculateMergeTransform(grid, blockOffset);
                using (IEnumerator<KeyValuePair<Vector3I, MyCube>> enumerator = grid.m_cubes.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<Vector3I, MyCube> current = enumerator.Current;
                        Vector3I key = Vector3I.Transform(current.Key, transformation);
                        if (this.m_cubes.ContainsKey(key))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            finally
            {
            }
        }

        public static bool CanPasteGrid() => 
            MySession.Static.IsCopyPastingEnabled;

        public bool CanPlaceBlock(Vector3I min, Vector3I max, MyBlockOrientation orientation, MyCubeBlockDefinition definition, int? ignoreMultiblockId = new int?(), bool ignoreFracturedPieces = false)
        {
            MyGridPlacementSettings gridPlacementSettings = MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.GetGridPlacementSettings(this.GridSizeEnum, this.IsStatic);
            return this.CanPlaceBlock(min, max, orientation, definition, ref gridPlacementSettings, ignoreMultiblockId, ignoreFracturedPieces);
        }

        public bool CanPlaceBlock(Vector3I min, Vector3I max, MyBlockOrientation orientation, MyCubeBlockDefinition definition, ref MyGridPlacementSettings gridSettings, int? ignoreMultiblockId = new int?(), bool ignoreFracturedPieces = false) => 
            ((this.CanAddCubes(min, max, new MyBlockOrientation?(orientation), definition) && ((!MyFakes.ENABLE_MULTIBLOCKS || !MyFakes.ENABLE_MULTIBLOCK_CONSTRUCTION) || this.CanAddOtherBlockInMultiBlock(min, max, orientation, definition, ignoreMultiblockId))) && TestPlacementAreaCube(this, ref gridSettings, min, max, orientation, definition, this, ignoreFracturedPieces));

        private bool CanPlaceWithConnectivity(MyBlockLocation location, ref Quaternion orientation, ref Vector3I center, MyCubeBlockDefinition blockDefinition, MyCubeBlockDefinition.MountPoint[] mountPoints)
        {
            int? ignoreMultiblockId = null;
            return (this.CanPlaceBlock(location.Min, location.Max, location.Orientation, blockDefinition, ignoreMultiblockId, false) && CheckConnectivity(this, blockDefinition, mountPoints, ref orientation, ref center));
        }

        private static void ChangeBlockOwner(MySlimBlock block, long ownerId)
        {
            if (block.FatBlock != null)
            {
                block.FatBlock.ChangeOwner(ownerId, MyOwnershipShareModeEnum.Faction);
            }
        }

        public bool ChangeColor(MySlimBlock block, Vector3 newHSV)
        {
            bool flag;
            try
            {
                if (block.ColorMaskHSV == newHSV)
                {
                    flag = false;
                }
                else
                {
                    Vector3 colorMaskHSV = block.ColorMaskHSV;
                    this.m_colorStatistics[colorMaskHSV] -= 1;
                    if (this.m_colorStatistics[block.ColorMaskHSV] <= 0)
                    {
                        this.m_colorStatistics.Remove(block.ColorMaskHSV);
                    }
                    block.ColorMaskHSV = newHSV;
                    block.UpdateVisual(false);
                    if (!this.m_colorStatistics.ContainsKey(block.ColorMaskHSV))
                    {
                        this.m_colorStatistics.Add(block.ColorMaskHSV, 0);
                    }
                    colorMaskHSV = block.ColorMaskHSV;
                    this.m_colorStatistics[colorMaskHSV] += 1;
                    flag = true;
                }
            }
            finally
            {
            }
            return flag;
        }

        public void ChangeDisplayNameRequest(string displayName)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, string>(this, x => new Action<string>(x.OnChangeDisplayNameRequest), displayName, targetEndpoint);
        }

        public void ChangeGridOwner(long playerId, MyOwnershipShareModeEnum shareMode)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, long, MyOwnershipShareModeEnum>(this, x => new Action<long, MyOwnershipShareModeEnum>(x.OnChangeGridOwner), playerId, shareMode, targetEndpoint);
            this.OnChangeGridOwner(playerId, shareMode);
        }

        public void ChangeGridOwnership(long playerId, MyOwnershipShareModeEnum shareMode)
        {
            if (Sync.IsServer)
            {
                this.ChangeGridOwner(playerId, shareMode);
            }
        }

        public void ChangeOwner(MyCubeBlock block, long oldOwner, long newOwner)
        {
            if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
            {
                this.m_ownershipManager.ChangeBlockOwnership(block, oldOwner, newOwner);
            }
        }

        public void ChangeOwnerRequest(MyCubeGrid grid, MyCubeBlock block, long playerId, MyOwnershipShareModeEnum shareMode)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, long, long, MyOwnershipShareModeEnum>(this, x => new Action<long, long, MyOwnershipShareModeEnum>(x.OnChangeOwnerRequest), block.EntityId, playerId, shareMode, targetEndpoint);
        }

        private static void ChangeOwnership(long inventoryEntityId, MyCubeGrid grid)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(inventoryEntityId, out entity, false) && (entity != null))
            {
                MyCharacter character = entity as MyCharacter;
                if (character != null)
                {
                    grid.ChangeGridOwner(character.ControllerInfo.Controller.Player.Identity.IdentityId, MyOwnershipShareModeEnum.Faction);
                }
            }
        }

        public static void ChangeOwnersRequest(MyOwnershipShareModeEnum shareMode, List<MySingleOwnershipRequest> requests, long requestingPlayer)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<MyOwnershipShareModeEnum, List<MySingleOwnershipRequest>, long>(s => new Action<MyOwnershipShareModeEnum, List<MySingleOwnershipRequest>, long>(MyCubeGrid.OnChangeOwnersRequest), shareMode, requests, requestingPlayer, targetEndpoint, position);
        }

        public void ChangePowerProducerState(MyMultipleEnabledEnum enabledState, long playerId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, MyMultipleEnabledEnum, long>(this, x => new Action<MyMultipleEnabledEnum, long>(x.OnPowerProducerStateRequest), enabledState, playerId, targetEndpoint);
        }

        public bool ChangeSkin(MySlimBlock block, MyStringHash skinSubtypeId)
        {
            bool flag;
            try
            {
                if (block.SkinSubtypeId == skinSubtypeId)
                {
                    flag = false;
                }
                else
                {
                    block.SkinSubtypeId = skinSubtypeId;
                    block.UpdateVisual(false);
                    flag = true;
                }
            }
            finally
            {
            }
            return flag;
        }

        public static void CheckAreaConnectivity(MyCubeGrid grid, ref MyBlockBuildArea area, List<Vector3UByte> validOffsets, HashSet<Vector3UByte> resultFailList)
        {
            try
            {
                MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition((MyDefinitionId) area.DefinitionId);
                if (cubeBlockDefinition != null)
                {
                    Quaternion orientation = Base6Directions.GetOrientation(area.OrientationForward, area.OrientationUp);
                    Vector3I stepDelta = (Vector3I) area.StepDelta;
                    MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = cubeBlockDefinition.GetBuildProgressModelMountPoints(MyComponentStack.NewBlockIntegrity);
                    int index = validOffsets.Count - 1;
                    while (true)
                    {
                        if (index < 0)
                        {
                            m_areaOverlapTest.Initialize(ref area, cubeBlockDefinition);
                            foreach (Vector3UByte num3 in m_tmpAreaMountpointPass)
                            {
                                m_areaOverlapTest.AddBlock(num3);
                            }
                            int count = 0x7fffffff;
                            while (true)
                            {
                                if ((validOffsets.Count <= 0) || (validOffsets.Count >= count))
                                {
                                    foreach (Vector3UByte num5 in validOffsets)
                                    {
                                        resultFailList.Add(num5);
                                    }
                                    validOffsets.Clear();
                                    validOffsets.AddHashset<Vector3UByte>(m_tmpAreaMountpointPass);
                                    break;
                                }
                                count = validOffsets.Count;
                                for (int i = validOffsets.Count - 1; i >= 0; i--)
                                {
                                    Vector3I vectori3 = (Vector3I) (area.PosInGrid + (validOffsets[i] * stepDelta));
                                    if (CheckConnectivity(m_areaOverlapTest, cubeBlockDefinition, buildProgressModelMountPoints, ref orientation, ref vectori3))
                                    {
                                        m_tmpAreaMountpointPass.Add(validOffsets[i]);
                                        m_areaOverlapTest.AddBlock(validOffsets[i]);
                                        validOffsets.RemoveAtFast<Vector3UByte>(i);
                                    }
                                }
                            }
                            break;
                        }
                        Vector3I position = (Vector3I) (area.PosInGrid + (validOffsets[index] * stepDelta));
                        if (CheckConnectivity(grid, cubeBlockDefinition, buildProgressModelMountPoints, ref orientation, ref position))
                        {
                            m_tmpAreaMountpointPass.Add(validOffsets[index]);
                            validOffsets.RemoveAtFast<Vector3UByte>(index);
                        }
                        index--;
                    }
                }
            }
            finally
            {
                m_tmpAreaMountpointPass.Clear();
            }
        }

        public static bool CheckConnectivity(IMyGridConnectivityTest grid, MyCubeBlockDefinition def, MyCubeBlockDefinition.MountPoint[] mountPoints, ref Quaternion rotation, ref Vector3I position)
        {
            bool flag;
            try
            {
                Vector3I center;
                Vector3I size;
                int num;
                if (mountPoints != null)
                {
                    Vector3I vectori3;
                    Vector3I vectori4;
                    center = def.Center;
                    size = def.Size;
                    Vector3I.Transform(ref center, ref rotation, out vectori4);
                    Vector3I.Transform(ref size, ref rotation, out vectori3);
                    num = 0;
                }
                else
                {
                    return false;
                }
                goto TR_0032;
            TR_0004:
                num++;
            TR_0032:
                while (true)
                {
                    Vector3 vector3;
                    Vector3 vector4;
                    Vector3I vectori7;
                    Vector3I vectori8;
                    if (num >= mountPoints.Length)
                    {
                        flag = false;
                        break;
                    }
                    MyCubeBlockDefinition.MountPoint thisMountPoint = mountPoints[num];
                    Vector3 vector = thisMountPoint.Start - center;
                    Vector3 vector2 = thisMountPoint.End - center;
                    if (MyFakes.ENABLE_TEST_BLOCK_CONNECTIVITY_CHECK)
                    {
                        Vector3 vector8 = Vector3.Min(thisMountPoint.Start, thisMountPoint.End);
                        Vector3 vector9 = Vector3.Max(thisMountPoint.Start, thisMountPoint.End);
                        Vector3I vectori17 = Vector3I.One - Vector3I.Abs(thisMountPoint.Normal);
                        Vector3 vector10 = (((Vector3I.One - vectori17) * vector8) + (Vector3.Clamp(vector8, Vector3.Zero, (Vector3) size) * vectori17)) + (0.001f * vectori17);
                        vector = vector10 - center;
                        vector2 = ((((Vector3I.One - vectori17) * vector9) + (Vector3.Clamp(vector9, Vector3.Zero, (Vector3) size) * vectori17)) - (0.001f * vectori17)) - center;
                    }
                    Vector3I vectori5 = Vector3I.Floor(vector);
                    Vector3I vectori6 = Vector3I.Floor(vector2);
                    Vector3.Transform(ref vector, ref rotation, out vector3);
                    Vector3.Transform(ref vector2, ref rotation, out vector4);
                    Vector3I.Transform(ref vectori5, ref rotation, out vectori7);
                    Vector3I.Transform(ref vectori6, ref rotation, out vectori8);
                    vector3 += vectori7 - Vector3I.Floor(vector3);
                    vector4 += vectori8 - Vector3I.Floor(vector4);
                    Vector3 vector5 = position + vector4;
                    m_cacheNeighborBlocks.Clear();
                    Vector3 vector1 = position + vector3;
                    Vector3 vector6 = Vector3.Min(vector1, vector5);
                    Vector3 vector7 = Vector3.Max(vector1, vector5);
                    Vector3I minI = Vector3I.Floor(vector6);
                    Vector3I maxI = Vector3I.Floor(vector7);
                    grid.GetConnectedBlocks(minI, maxI, m_cacheNeighborBlocks);
                    if (m_cacheNeighborBlocks.Count == 0)
                    {
                        goto TR_0004;
                    }
                    else
                    {
                        Vector3I vectori15;
                        Vector3I.Transform(ref thisMountPoint.Normal, ref rotation, out vectori15);
                        minI -= vectori15;
                        maxI -= vectori15;
                        Vector3I faceNormal = -vectori15;
                        using (Dictionary<Vector3I, ConnectivityResult>.ValueCollection.Enumerator enumerator = m_cacheNeighborBlocks.Values.GetEnumerator())
                        {
                            while (true)
                            {
                                if (enumerator.MoveNext())
                                {
                                    List<MySlimBlock>.Enumerator enumerator2;
                                    ConnectivityResult current = enumerator.Current;
                                    if (current.Position == position)
                                    {
                                        if (!MyFakes.ENABLE_COMPOUND_BLOCKS)
                                        {
                                            continue;
                                        }
                                        if (((current.FatBlock != null) && current.FatBlock.CheckConnectionAllowed) && !current.FatBlock.ConnectionAllowed(ref minI, ref maxI, ref faceNormal, def))
                                        {
                                            continue;
                                        }
                                        if (!(current.FatBlock is MyCompoundCubeBlock))
                                        {
                                            continue;
                                        }
                                        using (enumerator2 = (current.FatBlock as MyCompoundCubeBlock).GetBlocks().GetEnumerator())
                                        {
                                            while (true)
                                            {
                                                if (!enumerator2.MoveNext())
                                                {
                                                    break;
                                                }
                                                MySlimBlock block = enumerator2.Current;
                                                MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = block.BlockDefinition.GetBuildProgressModelMountPoints(block.BuildLevelRatio);
                                                if (CheckNeighborMountPointsForCompound(vector6, vector7, thisMountPoint, ref vectori15, def, current.Position, block.BlockDefinition, buildProgressModelMountPoints, block.Orientation, m_cacheMountPointsA))
                                                {
                                                    return true;
                                                }
                                            }
                                            continue;
                                        }
                                    }
                                    if (((current.FatBlock == null) || !current.FatBlock.CheckConnectionAllowed) || current.FatBlock.ConnectionAllowed(ref minI, ref maxI, ref faceNormal, def))
                                    {
                                        if (current.FatBlock is MyCompoundCubeBlock)
                                        {
                                            using (enumerator2 = (current.FatBlock as MyCompoundCubeBlock).GetBlocks().GetEnumerator())
                                            {
                                                while (true)
                                                {
                                                    if (!enumerator2.MoveNext())
                                                    {
                                                        break;
                                                    }
                                                    MySlimBlock block2 = enumerator2.Current;
                                                    MyCubeBlockDefinition.MountPoint[] neighborMountPoints = block2.BlockDefinition.GetBuildProgressModelMountPoints(block2.BuildLevelRatio);
                                                    if (CheckNeighborMountPoints(vector6, vector7, thisMountPoint, ref vectori15, def, current.Position, block2.BlockDefinition, neighborMountPoints, block2.Orientation, m_cacheMountPointsA))
                                                    {
                                                        return true;
                                                    }
                                                }
                                                continue;
                                            }
                                        }
                                        float currentIntegrityRatio = 1f;
                                        if ((current.FatBlock != null) && (current.FatBlock.SlimBlock != null))
                                        {
                                            currentIntegrityRatio = current.FatBlock.SlimBlock.BuildLevelRatio;
                                        }
                                        MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = current.Definition.GetBuildProgressModelMountPoints(currentIntegrityRatio);
                                        if (CheckNeighborMountPoints(vector6, vector7, thisMountPoint, ref vectori15, def, current.Position, current.Definition, buildProgressModelMountPoints, current.Orientation, m_cacheMountPointsA))
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                    continue;
                                }
                                else
                                {
                                    goto TR_0004;
                                }
                                break;
                            }
                            break;
                        }
                        goto TR_0004;
                    }
                    break;
                }
            }
            finally
            {
                m_cacheNeighborBlocks.Clear();
            }
            return flag;
        }

        public static bool CheckConnectivitySmallBlockToLargeGrid(MyCubeGrid grid, MyCubeBlockDefinition def, ref Quaternion rotation, ref Vector3I addNormal)
        {
            bool flag;
            try
            {
                MyCubeBlockDefinition.MountPoint[] mountPoints = def.MountPoints;
                if (mountPoints == null)
                {
                    flag = false;
                }
                else
                {
                    int index = 0;
                    while (true)
                    {
                        if (index >= mountPoints.Length)
                        {
                            flag = false;
                        }
                        else
                        {
                            Vector3I vectori;
                            MyCubeBlockDefinition.MountPoint point = mountPoints[index];
                            Vector3I.Transform(ref point.Normal, ref rotation, out vectori);
                            if (addNormal != -vectori)
                            {
                                index++;
                                continue;
                            }
                            flag = true;
                        }
                        break;
                    }
                }
            }
            finally
            {
                m_cacheNeighborBlocks.Clear();
            }
            return flag;
        }

        public static bool CheckMergeConnectivity(MyCubeGrid hitGrid, MyCubeGrid gridToMerge, Vector3I gridOffset)
        {
            Quaternion quaternion;
            MatrixI transformation = hitGrid.CalculateMergeTransform(gridToMerge, gridOffset);
            transformation.GetBlockOrientation().GetQuaternion(out quaternion);
            using (HashSet<MySlimBlock>.Enumerator enumerator = gridToMerge.GetBlocks().GetEnumerator())
            {
                while (true)
                {
                    Quaternion quaternion2;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MySlimBlock current = enumerator.Current;
                    Vector3I position = Vector3I.Transform(current.Position, transformation);
                    current.Orientation.GetQuaternion(out quaternion2);
                    quaternion2 = quaternion * quaternion2;
                    MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = current.BlockDefinition.GetBuildProgressModelMountPoints(current.BuildLevelRatio);
                    if (CheckConnectivity(hitGrid, current.BlockDefinition, buildProgressModelMountPoints, ref quaternion2, ref position))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckMountPointsForSide(MyCubeBlockDefinition defA, MyCubeBlockDefinition.MountPoint[] mountPointsA, ref MyBlockOrientation orientationA, ref Vector3I positionA, ref Vector3I normalA, MyCubeBlockDefinition defB, MyCubeBlockDefinition.MountPoint[] mountPointsB, ref MyBlockOrientation orientationB, ref Vector3I positionB)
        {
            TransformMountPoints(m_cacheMountPointsA, defA, mountPointsA, ref orientationA);
            TransformMountPoints(m_cacheMountPointsB, defB, mountPointsB, ref orientationB);
            return CheckMountPointsForSide(m_cacheMountPointsA, ref orientationA, ref positionA, defA.Id, ref normalA, m_cacheMountPointsB, ref orientationB, ref positionB, defB.Id);
        }

        public static bool CheckMountPointsForSide(List<MyCubeBlockDefinition.MountPoint> transormedA, ref MyBlockOrientation orientationA, ref Vector3I positionA, MyDefinitionId idA, ref Vector3I normalA, List<MyCubeBlockDefinition.MountPoint> transormedB, ref MyBlockOrientation orientationB, ref Vector3I positionB, MyDefinitionId idB)
        {
            Vector3I vectori = positionB - positionA;
            Vector3I vectori2 = -normalA;
            for (int i = 0; i < transormedA.Count; i++)
            {
                if (transormedA[i].Enabled)
                {
                    MyCubeBlockDefinition.MountPoint point = transormedA[i];
                    if (point.Normal == normalA)
                    {
                        BoundingBox box = new BoundingBox(Vector3.Min(point.Start, point.End) - vectori, Vector3.Max(point.Start, point.End) - vectori);
                        for (int j = 0; j < transormedB.Count; j++)
                        {
                            if (transormedB[j].Enabled)
                            {
                                MyCubeBlockDefinition.MountPoint point2 = transormedB[j];
                                if ((point2.Normal == vectori2) && ((((point.ExclusionMask & point2.PropertiesMask) == 0) && ((point.PropertiesMask & point2.ExclusionMask) == 0)) || (idA == idB)))
                                {
                                    BoundingBox box2 = new BoundingBox(Vector3.Min(point2.Start, point2.End), Vector3.Max(point2.Start, point2.End));
                                    if (box.Intersects(box2))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool CheckNeighborMountPoints(Vector3 currentMin, Vector3 currentMax, MyCubeBlockDefinition.MountPoint thisMountPoint, ref Vector3I thisMountPointTransformedNormal, MyCubeBlockDefinition thisDefinition, Vector3I neighborPosition, MyCubeBlockDefinition neighborDefinition, MyCubeBlockDefinition.MountPoint[] neighborMountPoints, MyBlockOrientation neighborOrientation, List<MyCubeBlockDefinition.MountPoint> otherMountPoints)
        {
            if (thisMountPoint.Enabled)
            {
                BoundingBox box = new BoundingBox(currentMin - neighborPosition, currentMax - neighborPosition);
                TransformMountPoints(otherMountPoints, neighborDefinition, neighborMountPoints, ref neighborOrientation);
                using (List<MyCubeBlockDefinition.MountPoint>.Enumerator enumerator = otherMountPoints.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyCubeBlockDefinition.MountPoint current = enumerator.Current;
                        if (((((thisMountPoint.ExclusionMask & current.PropertiesMask) == 0) && ((thisMountPoint.PropertiesMask & current.ExclusionMask) == 0)) || (thisDefinition.Id == neighborDefinition.Id)) && (current.Enabled && (!MyFakes.ENABLE_TEST_BLOCK_CONNECTIVITY_CHECK || ((thisMountPointTransformedNormal + current.Normal) == Vector3I.Zero))))
                        {
                            BoundingBox box2 = new BoundingBox(Vector3.Min(current.Start, current.End), Vector3.Max(current.Start, current.End));
                            if (box.Intersects(box2))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool CheckNeighborMountPointsForCompound(Vector3 currentMin, Vector3 currentMax, MyCubeBlockDefinition.MountPoint thisMountPoint, ref Vector3I thisMountPointTransformedNormal, MyCubeBlockDefinition thisDefinition, Vector3I neighborPosition, MyCubeBlockDefinition neighborDefinition, MyCubeBlockDefinition.MountPoint[] neighborMountPoints, MyBlockOrientation neighborOrientation, List<MyCubeBlockDefinition.MountPoint> otherMountPoints)
        {
            if (thisMountPoint.Enabled)
            {
                BoundingBox box = new BoundingBox(currentMin - neighborPosition, currentMax - neighborPosition);
                TransformMountPoints(otherMountPoints, neighborDefinition, neighborMountPoints, ref neighborOrientation);
                using (List<MyCubeBlockDefinition.MountPoint>.Enumerator enumerator = otherMountPoints.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyCubeBlockDefinition.MountPoint current = enumerator.Current;
                        if (((((thisMountPoint.ExclusionMask & current.PropertiesMask) == 0) && ((thisMountPoint.PropertiesMask & current.ExclusionMask) == 0)) || (thisDefinition.Id == neighborDefinition.Id)) && (current.Enabled && (!MyFakes.ENABLE_TEST_BLOCK_CONNECTIVITY_CHECK || ((thisMountPointTransformedNormal - current.Normal) == Vector3I.Zero))))
                        {
                            BoundingBox box2 = new BoundingBox(Vector3.Min(current.Start, current.End), Vector3.Max(current.Start, current.End));
                            if (box.Intersects(box2))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static System.Type ChooseGridSystemsType()
        {
            System.Type gridSystemsType = typeof(MyCubeGridSystems);
            ChooseGridSystemsType(ref gridSystemsType, MyPlugins.GameAssembly);
            ChooseGridSystemsType(ref gridSystemsType, MyPlugins.SandboxAssembly);
            ChooseGridSystemsType(ref gridSystemsType, MyPlugins.UserAssemblies);
            return gridSystemsType;
        }

        private static void ChooseGridSystemsType(ref System.Type gridSystemsType, Assembly[] assemblies)
        {
            if (assemblies != null)
            {
                foreach (Assembly assembly in assemblies)
                {
                    ChooseGridSystemsType(ref gridSystemsType, assembly);
                }
            }
        }

        private static void ChooseGridSystemsType(ref System.Type gridSystemsType, Assembly assembly)
        {
            if (assembly != null)
            {
                foreach (System.Type type in assembly.GetTypes())
                {
                    if (typeof(MyCubeGridSystems).IsAssignableFrom(type))
                    {
                        gridSystemsType = type;
                        return;
                    }
                }
            }
        }

        private void ClearDirty()
        {
            if (!this.m_updatingDirty && (this.m_resolvingSplits == 0))
            {
                MyCube cube;
                MyDirtyRegion dirtyRegion = this.m_dirtyRegion;
                this.m_dirtyRegion = this.m_dirtyRegionParallel;
                this.m_dirtyRegionParallel = dirtyRegion;
                this.m_dirtyRegionParallel.Cubes.Clear();
                while (this.m_dirtyRegionParallel.PartsToRemove.TryDequeue(out cube))
                {
                }
            }
        }

        public void ClearSymmetries()
        {
            this.XSymmetryPlane = null;
            this.YSymmetryPlane = null;
            this.ZSymmetryPlane = null;
        }

        public void CloseStructuralIntegrity()
        {
            if (this.StructuralIntegrity != null)
            {
                this.StructuralIntegrity.Close();
                this.StructuralIntegrity = null;
            }
        }

        [Event(null, 0x1274), Reliable, Server]
        private void ColorBlockRequest(Vector3I min, Vector3I max, Vector3 newHSV, bool playSound, long player)
        {
            if (this.ColorGridOrBlockRequestValidation(player))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, Vector3I, Vector3, bool, long>(this, x => new Action<Vector3I, Vector3I, Vector3, bool, long>(x.OnColorBlock), min, max, newHSV, playSound, player, targetEndpoint);
            }
        }

        public void ColorBlocks(Vector3I min, Vector3I max, Vector3 newHSV, bool playSound, bool validateOwnership)
        {
            long num = validateOwnership ? MySession.Static.LocalPlayerId : 0L;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, Vector3I, Vector3, bool, long>(this, x => new Action<Vector3I, Vector3I, Vector3, bool, long>(x.ColorBlockRequest), min, max, newHSV, playSound, num, targetEndpoint);
        }

        public void ColorGrid(Vector3 newHSV, bool playSound, bool validateOwnership)
        {
            long num = validateOwnership ? MySession.Static.LocalPlayerId : 0L;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3, bool, long>(this, x => new Action<Vector3, bool, long>(x.ColorGridFriendlyRequest), newHSV, playSound, num, targetEndpoint);
        }

        [Event(null, 0x1256), Reliable, Server]
        private void ColorGridFriendlyRequest(Vector3 newHSV, bool playSound, long player)
        {
            if (this.ColorGridOrBlockRequestValidation(player))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3, bool, long>(this, x => new Action<Vector3, bool, long>(x.OnColorGridFriendly), newHSV, playSound, player, targetEndpoint);
            }
        }

        private bool ColorGridOrBlockRequestValidation(long player)
        {
            if (player == 0)
            {
                return true;
            }
            if (!Sync.IsServer)
            {
                return true;
            }
            if (this.BigOwners.Count == 0)
            {
                return true;
            }
            using (List<long>.Enumerator enumerator = this.BigOwners.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (MyPlayer.GetRelationsBetweenPlayers(enumerator.Current, player) == MyRelationsBetweenPlayers.Self)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void ConvertFracturedBlocksToComponents()
        {
            List<MyFracturedBlock> list = new List<MyFracturedBlock>();
            using (HashSet<MySlimBlock>.Enumerator enumerator = this.m_cubeBlocks.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyFracturedBlock fatBlock = enumerator.Current.FatBlock as MyFracturedBlock;
                    if (fatBlock != null)
                    {
                        list.Add(fatBlock);
                    }
                }
            }
            bool enable = this.EnableGenerators(false, false);
            try
            {
                foreach (MyFracturedBlock block2 in list)
                {
                    MyObjectBuilder_CubeBlock objectBuilder = block2.ConvertToOriginalBlocksWithFractureComponent();
                    this.RemoveBlockInternal(block2.SlimBlock, true, false);
                    if (objectBuilder != null)
                    {
                        this.AddBlock(objectBuilder, false);
                    }
                }
            }
            finally
            {
                this.EnableGenerators(enable, false);
            }
        }

        private static void ConvertNextGrid(bool placeOnly)
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.NONE_TIMEOUT, new StringBuilder(MyTexts.GetString(MyCommonTexts.ConvertingObjs)), null, okButtonText, okButtonText, okButtonText, okButtonText, result => ConvertNextPrefab(m_prefabs, placeOnly), 0x3e8, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private static unsafe void ConvertNextPrefab(List<MyObjectBuilder_CubeGrid[]> prefabs, bool placeOnly)
        {
            if (prefabs.Count > 0)
            {
                MyObjectBuilder_CubeGrid[] currentPrefab = prefabs[0];
                int count = prefabs.Count;
                prefabs.RemoveAt(0);
                if (placeOnly)
                {
                    float radius = GetBoundingSphereForGrids(currentPrefab).Radius;
                    m_maxDimensionPreviousRow = MathHelper.Max(radius, m_maxDimensionPreviousRow);
                    if ((prefabs.Count % 4) != 0)
                    {
                        double* numPtr1 = (double*) ref m_newPositionForPlacedObject.X;
                        numPtr1[0] += (2f * radius) + 10f;
                    }
                    else
                    {
                        m_newPositionForPlacedObject.X = -((2f * radius) + 10f);
                        double* numPtr2 = (double*) ref m_newPositionForPlacedObject.Z;
                        numPtr2[0] -= (2f * m_maxDimensionPreviousRow) + 30f;
                        m_maxDimensionPreviousRow = 0f;
                    }
                    PlacePrefabToWorld(currentPrefab, MySector.MainCamera.Position + m_newPositionForPlacedObject, null);
                    ConvertNextPrefab(m_prefabs, placeOnly);
                    return;
                }
                List<MyCubeGrid> baseGrids = new List<MyCubeGrid>();
                MyObjectBuilder_CubeGrid[] gridArray2 = currentPrefab;
                int index = 0;
                while (true)
                {
                    if (index < gridArray2.Length)
                    {
                        MyObjectBuilder_CubeGrid objectBuilder = gridArray2[index];
                        baseGrids.Add(Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilderAndAdd(objectBuilder, false) as MyCubeGrid);
                        index++;
                        continue;
                    }
                    ExportToObjFile(baseGrids, true, false);
                    using (List<MyCubeGrid>.Enumerator enumerator = baseGrids.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.Close();
                        }
                        return;
                    }
                }
            }
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder(MyTexts.GetString(MyCommonTexts.ConvertToObjDone)), null, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        public static void ConvertPrefabsToObjs()
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.NONE_TIMEOUT, new StringBuilder(MyTexts.GetString(MyCommonTexts.ConvertingObjs)), null, okButtonText, okButtonText, okButtonText, okButtonText, result => StartConverting(false), 0x3e8, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private static void ConvertToLocationIdentityList(List<Tuple<Vector3I, ushort>> locationsAndIdsFrom, List<LocationIdentity> locationsAndIdsTo)
        {
            locationsAndIdsTo.Clear();
            locationsAndIdsTo.Capacity = locationsAndIdsFrom.Count;
            foreach (Tuple<Vector3I, ushort> tuple in locationsAndIdsFrom)
            {
                LocationIdentity item = new LocationIdentity {
                    Location = tuple.Item1,
                    Id = tuple.Item2
                };
                locationsAndIdsTo.Add(item);
            }
        }

        [Event(null, 0x1513), Reliable, ServerInvoked, Broadcast]
        public void ConvertToStatic()
        {
            if ((!this.IsStatic && (this.Physics != null)) && ((this.Physics.AngularVelocity.LengthSquared() <= 0.0001) && (this.Physics.LinearVelocity.LengthSquared() <= 0.0001)))
            {
                this.IsStatic = true;
                this.IsUnsupportedStation = true;
                this.Physics.ConvertToStatic();
                base.RaisePhysicsChanged();
                MyFixedGrids.MarkGridRoot(this);
            }
        }

        private static Vector3 ConvertVariantToHsvColor(Color variantColor)
        {
            uint packedValue = variantColor.PackedValue;
            if (packedValue <= 0xff008000)
            {
                if (packedValue == 0xff000000)
                {
                    return MyRenderComponentBase.OldBlackToHSV;
                }
                if (packedValue == 0xff0000ff)
                {
                    return MyRenderComponentBase.OldRedToHSV;
                }
                if (packedValue == 0xff008000)
                {
                    return MyRenderComponentBase.OldGreenToHSV;
                }
            }
            else if (packedValue <= 0xff808080)
            {
                if (packedValue == 0xff00ffff)
                {
                    return MyRenderComponentBase.OldYellowToHSV;
                }
                if (packedValue != 0xff808080)
                {
                }
            }
            else
            {
                if (packedValue == 0xffff0000)
                {
                    return MyRenderComponentBase.OldBlueToHSV;
                }
                if (packedValue == uint.MaxValue)
                {
                    return MyRenderComponentBase.OldWhiteToHSV;
                }
            }
            return MyRenderComponentBase.OldGrayToHSV;
        }

        internal static MyObjectBuilder_CubeBlock CreateBlockObjectBuilder(MyCubeBlockDefinition definition, Vector3I min, MyBlockOrientation orientation, long entityID, long owner, bool fullyBuilt)
        {
            MyObjectBuilder_CubeBlock block = (MyObjectBuilder_CubeBlock) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) definition.Id);
            block.BuildPercent = fullyBuilt ? 1f : 1.525902E-05f;
            block.IntegrityPercent = fullyBuilt ? 1f : 1.525902E-05f;
            block.EntityId = entityID;
            block.Min = min;
            block.BlockOrientation = orientation;
            block.BuiltBy = owner;
            if (definition.ContainsComputer())
            {
                block.Owner = 0L;
                block.ShareMode = MyOwnershipShareModeEnum.All;
            }
            return block;
        }

        private MyCube CreateCube(MySlimBlock block, Vector3I pos, Matrix rotation, MyCubeBlockDefinition cubeBlockDefinition)
        {
            MyCube cube1 = new MyCube();
            cube1.Parts = GetCubeParts(block.SkinSubtypeId, cubeBlockDefinition, pos, rotation, this.GridSize, this.GridScale);
            cube1.CubeBlock = block;
            return cube1;
        }

        private void CreateFractureBlockComponent(MyFractureComponentBase.Info info)
        {
            if (!info.Entity.MarkedForClose)
            {
                MyFractureComponentCubeBlock component = new MyFractureComponentCubeBlock();
                info.Entity.Components.Add<MyFractureComponentBase>(component);
                component.SetShape(info.Shape, info.Compound);
                if (Sync.IsServer)
                {
                    MyCubeBlock entity = info.Entity as MyCubeBlock;
                    if (entity != null)
                    {
                        MyCubeGridSmallToLargeConnection.Static.RemoveBlockSmallToLargeConnection(entity.SlimBlock);
                        MySlimBlock cubeBlock = entity.CubeGrid.GetCubeBlock(entity.Position);
                        MyCompoundCubeBlock block4 = (cubeBlock != null) ? (cubeBlock.FatBlock as MyCompoundCubeBlock) : null;
                        if (block4 == null)
                        {
                            MyObjectBuilder_FractureComponentBase base3 = (MyObjectBuilder_FractureComponentBase) component.Serialize(false);
                            MySyncDestructions.CreateFractureComponent(entity.CubeGrid.EntityId, entity.Position, 0xffff, base3);
                        }
                        else
                        {
                            ushort? blockId = block4.GetBlockId(entity.SlimBlock);
                            if (blockId != null)
                            {
                                MyObjectBuilder_FractureComponentBase base2 = (MyObjectBuilder_FractureComponentBase) component.Serialize(false);
                                MySyncDestructions.CreateFractureComponent(entity.CubeGrid.EntityId, entity.Position, blockId.Value, base2);
                            }
                        }
                        entity.SlimBlock.ApplyDestructionDamage(component.GetIntegrityRatioFromFracturedPieceCounts());
                    }
                }
            }
        }

        private MyFracturedBlock CreateFracturedBlock(MyFracturedBlock.Info info)
        {
            MyCube cube;
            MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_FracturedBlock), "FracturedBlockLarge");
            Vector3I position = info.Position;
            if (this.m_cubes.TryGetValue(position, out cube))
            {
                this.RemoveBlock(cube.CubeBlock, false);
            }
            MyObjectBuilder_CubeBlock objectBuilder = CreateBlockObjectBuilder(MyDefinitionManager.Static.GetCubeBlockDefinition(id), position, new MyBlockOrientation(ref Quaternion.Identity), 0L, 0L, true);
            objectBuilder.ColorMaskHSV = Vector3.Zero;
            (objectBuilder as MyObjectBuilder_FracturedBlock).CreatingFracturedBlock = true;
            MySlimBlock block2 = this.AddBlock(objectBuilder, false);
            if (block2 == null)
            {
                info.Shape.RemoveReference();
                return null;
            }
            MyFracturedBlock fatBlock = block2.FatBlock as MyFracturedBlock;
            fatBlock.OriginalBlocks = info.OriginalBlocks;
            fatBlock.Orientations = info.Orientations;
            fatBlock.MultiBlocks = info.MultiBlocks;
            fatBlock.SetDataFromHavok(info.Shape, info.Compound);
            fatBlock.Render.UpdateRenderObject(true, true);
            this.UpdateBlockNeighbours(fatBlock.SlimBlock);
            if (Sync.IsServer)
            {
                MySyncDestructions.CreateFracturedBlock((MyObjectBuilder_FracturedBlock) fatBlock.GetObjectBuilderCubeBlock(false), base.EntityId, position);
            }
            return fatBlock;
        }

        public MyFracturedBlock CreateFracturedBlock(MyObjectBuilder_FracturedBlock fracturedBlockBuilder, Vector3I position)
        {
            MyCube cube;
            MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_FracturedBlock), "FracturedBlockLarge");
            MyDefinitionManager.Static.GetCubeBlockDefinition(id);
            Vector3I key = position;
            if (this.m_cubes.TryGetValue(key, out cube))
            {
                this.RemoveBlockInternal(cube.CubeBlock, true, true);
            }
            fracturedBlockBuilder.CreatingFracturedBlock = true;
            MySlimBlock block = this.AddBlock(fracturedBlockBuilder, false);
            if (block == null)
            {
                return null;
            }
            MyFracturedBlock fatBlock = block.FatBlock as MyFracturedBlock;
            fatBlock.Render.UpdateRenderObject(true, true);
            this.UpdateBlockNeighbours(fatBlock.SlimBlock);
            return fatBlock;
        }

        private static MyCubeGrid CreateGridForSplit(MyCubeGrid originalGrid, long newEntityId)
        {
            MyObjectBuilder_CubeGrid objectBuilder = MyObjectBuilderSerializer.CreateNewObject(typeof(MyObjectBuilder_CubeGrid)) as MyObjectBuilder_CubeGrid;
            if (objectBuilder == null)
            {
                MyLog.Default.WriteLine("CreateForSplit builder shouldn't be null! Original Grid info: " + originalGrid.ToString());
                return null;
            }
            objectBuilder.EntityId = newEntityId;
            objectBuilder.GridSizeEnum = originalGrid.GridSizeEnum;
            objectBuilder.IsStatic = originalGrid.IsStatic;
            objectBuilder.PersistentFlags = originalGrid.Render.PersistentFlags;
            objectBuilder.PositionAndOrientation = new MyPositionAndOrientation(originalGrid.WorldMatrix);
            objectBuilder.DampenersEnabled = (bool) originalGrid.m_dampenersEnabled;
            objectBuilder.IsPowered = originalGrid.m_IsPowered;
            objectBuilder.IsUnsupportedStation = originalGrid.IsUnsupportedStation;
            MyCubeGrid grid2 = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilderNoinit(objectBuilder) as MyCubeGrid;
            if (grid2 == null)
            {
                return null;
            }
            grid2.InitInternal(objectBuilder, false);
            OnSplitGridCreated.InvokeIfNotNull<MyCubeGrid>(grid2);
            return grid2;
        }

        public static void CreateGridGroupLink(GridLinkTypeEnum type, long linkId, MyCubeGrid parent, MyCubeGrid child)
        {
            MyCubeGridGroups.Static.CreateLink(type, linkId, parent, child);
        }

        private static unsafe void CreateMaterialFile(string folder, string matFilename, List<MyExportModel.Material> materials, List<renderColoredTextureProperties> texturesToRender)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            using (StreamWriter writer = new StreamWriter(matFilename))
            {
                foreach (MyExportModel.Material material in materials)
                {
                    string exportedMaterialName = material.ExportedMaterialName;
                    writer.WriteLine("newmtl {0}", exportedMaterialName);
                    if (MyFakes.ENABLE_EXPORT_MTL_DIAGNOSTICS)
                    {
                        writer.WriteLine("# HSV Mask: {0}", material.ColorMaskHSV.ToString("F2"));
                        writer.WriteLine("# IsGlass: {0}", material.IsGlass);
                        writer.WriteLine("# AddMapsMap: {0}", material.AddMapsTexture ?? "Null");
                        writer.WriteLine("# AlphamaskMap: {0}", material.AlphamaskTexture ?? "Null");
                        writer.WriteLine("# ColorMetalMap: {0}", material.ColorMetalTexture ?? "Null");
                        writer.WriteLine("# NormalGlossMap: {0}", material.NormalGlossTexture ?? "Null");
                    }
                    if (!material.IsGlass)
                    {
                        writer.WriteLine("Ka 1.000 1.000 1.000");
                        writer.WriteLine("Kd 1.000 1.000 1.000");
                        writer.WriteLine("Ks 0.100 0.100 0.100");
                        writer.WriteLine((material.AlphamaskTexture == null) ? "d 1.0" : "d 0.0");
                    }
                    else
                    {
                        writer.WriteLine("Ka 0.000 0.000 0.000");
                        writer.WriteLine("Kd 0.000 0.000 0.000");
                        writer.WriteLine("Ks 0.900 0.900 0.900");
                        writer.WriteLine("d 0.350");
                    }
                    writer.WriteLine("Ns 95.00");
                    writer.WriteLine("illum 2");
                    if (material.ColorMetalTexture != null)
                    {
                        renderColoredTextureProperties* propertiesPtr1;
                        string format = exportedMaterialName + "_{0}.png";
                        string str2 = string.Format(format, "ca");
                        string str3 = string.Format(format, "ng");
                        writer.WriteLine("map_Ka {0}", str2);
                        writer.WriteLine("map_Kd {0}", str2);
                        if (material.AlphamaskTexture != null)
                        {
                            writer.WriteLine("map_d {0}", str2);
                        }
                        bool flag = false;
                        if (material.NormalGlossTexture != null)
                        {
                            string str4;
                            if (dictionary.TryGetValue(material.NormalGlossTexture, out str4))
                            {
                                str3 = str4;
                            }
                            else
                            {
                                flag = true;
                                dictionary.Add(material.NormalGlossTexture, str3);
                            }
                            writer.WriteLine("map_Bump {0}", str3);
                        }
                        renderColoredTextureProperties item = new renderColoredTextureProperties {
                            ColorMaskHSV = material.ColorMaskHSV,
                            TextureAddMaps = material.AddMapsTexture,
                            TextureAplhaMask = material.AlphamaskTexture,
                            TextureColorMetal = material.ColorMetalTexture
                        };
                        propertiesPtr1->TextureNormalGloss = flag ? material.NormalGlossTexture : null;
                        propertiesPtr1 = (renderColoredTextureProperties*) ref item;
                        item.PathToSave_ColorAlpha = Path.Combine(folder, str2);
                        item.PathToSave_NormalGloss = Path.Combine(folder, str3);
                        texturesToRender.Add(item);
                    }
                    writer.WriteLine();
                }
            }
        }

        private static List<MyExportModel.Material> CreateMaterialsForModel(List<MyExportModel.Material> materials, Vector3 colorMaskHSV, MyExportModel renderModel)
        {
            List<MyExportModel.Material> list1 = renderModel.GetMaterials();
            List<MyExportModel.Material> list = new List<MyExportModel.Material>(list1.Count);
            foreach (MyExportModel.Material material in list1)
            {
                MyExportModel.Material? nullable = null;
                foreach (MyExportModel.Material material3 in materials)
                {
                    Vector3 vector = colorMaskHSV - material3.ColorMaskHSV;
                    if ((vector.AbsMax() < 0.01) && material.EqualsMaterialWise(material3))
                    {
                        nullable = new MyExportModel.Material?(material3);
                        break;
                    }
                }
                MyExportModel.Material item = material;
                item.ColorMaskHSV = colorMaskHSV;
                if (nullable != null)
                {
                    item.ExportedMaterialName = nullable.Value.ExportedMaterialName;
                }
                else
                {
                    materialID++;
                    item.ExportedMaterialName = "material_" + materialID;
                    materials.Add(item);
                }
                list.Add(item);
            }
            return list;
        }

        private static void CreateObjFile(string name, string filename, string matFilename, List<Vector3> vertices, List<TriangleWithMaterial> triangles, List<Vector2> uvs, List<MyExportModel.Material> materials, int currVerticesCount)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("mtllib {0}", Path.GetFileName(matFilename));
                writer.WriteLine();
                writer.WriteLine("#");
                writer.WriteLine("# {0}", name);
                writer.WriteLine("#");
                writer.WriteLine();
                writer.WriteLine("# vertices");
                List<int> list = new List<int>(vertices.Count);
                Dictionary<Vector3D, int> dictionary = new Dictionary<Vector3D, int>(vertices.Count / 5);
                int num = 1;
                foreach (Vector3 vector in vertices)
                {
                    int num3;
                    if (!dictionary.TryGetValue(vector, out num3))
                    {
                        num++;
                        num3 = num;
                        dictionary.Add(vector, num3);
                        writer.WriteLine("v {0} {1} {2}", vector.X, vector.Y, vector.Z);
                    }
                    list.Add(num3);
                }
                dictionary = null;
                List<int> list2 = new List<int>(vertices.Count);
                Dictionary<Vector2, int> dictionary2 = new Dictionary<Vector2, int>(vertices.Count / 5);
                writer.WriteLine("# {0} vertices", vertices.Count);
                writer.WriteLine();
                writer.WriteLine("# texture coordinates");
                num = 1;
                foreach (Vector2 vector2 in uvs)
                {
                    int num4;
                    if (!dictionary2.TryGetValue(vector2, out num4))
                    {
                        num++;
                        num4 = num;
                        dictionary2.Add(vector2, num4);
                        writer.WriteLine("vt {0} {1}", vector2.X, vector2.Y);
                    }
                    list2.Add(num4);
                }
                dictionary2 = null;
                writer.WriteLine("# {0} texture coords", uvs.Count);
                writer.WriteLine();
                writer.WriteLine("# faces");
                writer.WriteLine("o {0}", name);
                int num2 = 0;
                using (List<MyExportModel.Material>.Enumerator enumerator3 = materials.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        num2++;
                        string exportedMaterialName = enumerator3.Current.ExportedMaterialName;
                        writer.WriteLine();
                        writer.WriteLine("g {0}_part{1}", name, num2);
                        writer.WriteLine("usemtl {0}", exportedMaterialName);
                        writer.WriteLine("s off");
                        for (int i = 0; i < triangles.Count; i++)
                        {
                            if (exportedMaterialName == triangles[i].material)
                            {
                                TriangleWithMaterial local1 = triangles[i];
                                MyTriangleVertexIndices triangle = local1.triangle;
                                MyTriangleVertexIndices uvIndices = local1.uvIndices;
                                object[] arg = new object[] { list[triangle.I0 - 1], list[triangle.I1 - 1], list[triangle.I2 - 1], list2[uvIndices.I0 - 1], list2[uvIndices.I1 - 1], list2[uvIndices.I2 - 1] };
                                writer.WriteLine("f {0}/{3} {1}/{4} {2}/{5}", arg);
                            }
                        }
                    }
                }
                writer.WriteLine("# {0} faces", triangles.Count);
            }
        }

        private static void CreatePrefabFile(List<MyCubeGrid> baseGrid, string name, string prefabPath)
        {
            Vector2I backBufferResolution = MyRenderProxy.BackBufferResolution;
            tumbnailMultiplier.X = 400f / ((float) backBufferResolution.X);
            tumbnailMultiplier.Y = 400f / ((float) backBufferResolution.Y);
            List<MyObjectBuilder_CubeGrid> copiedPrefab = new List<MyObjectBuilder_CubeGrid>();
            foreach (MyCubeGrid grid in baseGrid)
            {
                copiedPrefab.Add((MyObjectBuilder_CubeGrid) grid.GetObjectBuilder(false));
            }
            MyPrefabManager.SavePrefabToPath(name, prefabPath, copiedPrefab);
        }

        public static MyCubeGrid CreateSplit(MyCubeGrid originalGrid, List<MySlimBlock> blocks, bool sync = true, long newEntityId = 0L)
        {
            MyCubeGrid entity = CreateGridForSplit(originalGrid, newEntityId);
            if (entity == null)
            {
                return null;
            }
            Vector3 centerOfMassWorld = (Vector3) originalGrid.Physics.CenterOfMassWorld;
            Sandbox.Game.Entities.MyEntities.Add(entity, true);
            MoveBlocks(originalGrid, entity, blocks, 0, blocks.Count);
            entity.RebuildGrid(false);
            if (!entity.IsStatic)
            {
                entity.Physics.UpdateMass();
            }
            if (originalGrid.IsStatic)
            {
                entity.TestDynamic = MyTestDynamicReason.GridSplit;
                originalGrid.TestDynamic = MyTestDynamicReason.GridSplit;
            }
            entity.Physics.AngularVelocity = originalGrid.Physics.AngularVelocity;
            entity.Physics.LinearVelocity = originalGrid.Physics.GetVelocityAtPoint(entity.Physics.CenterOfMassWorld);
            originalGrid.UpdatePhysicsShape();
            if (!originalGrid.IsStatic)
            {
                originalGrid.Physics.UpdateMass();
            }
            Vector3 vector2 = Vector3.Cross(originalGrid.Physics.AngularVelocity, ((Vector3) originalGrid.Physics.CenterOfMassWorld) - centerOfMassWorld);
            originalGrid.Physics.LinearVelocity += vector2;
            if (originalGrid.OnGridSplit != null)
            {
                originalGrid.OnGridSplit(originalGrid, entity);
            }
            if (sync)
            {
                if (!Sync.IsServer)
                {
                    return entity;
                }
                m_tmpBlockPositions.Clear();
                foreach (MySlimBlock block in blocks)
                {
                    m_tmpBlockPositions.Add(block.Position);
                }
                MyMultiplayer.RemoveForClientIfIncomplete(originalGrid);
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, List<Vector3I>, long>(originalGrid, x => new Action<List<Vector3I>, long>(x.CreateSplit_Implementation), m_tmpBlockPositions, entity.EntityId, targetEndpoint);
            }
            return entity;
        }

        [Event(null, 0x4fc), Reliable, Broadcast]
        public void CreateSplit_Implementation(List<Vector3I> blocks, long newEntityId)
        {
            m_tmpBlockListReceive.Clear();
            foreach (Vector3I vectori in blocks)
            {
                MySlimBlock cubeBlock = this.GetCubeBlock(vectori);
                if (cubeBlock == null)
                {
                    MySandboxGame.Log.WriteLine("Block was null when trying to create a grid split. Desync?");
                    continue;
                }
                m_tmpBlockListReceive.Add(cubeBlock);
            }
            CreateSplit(this, m_tmpBlockListReceive, false, newEntityId);
            m_tmpBlockListReceive.Clear();
        }

        private static void CreateSplitForGroup(MyCubeGrid originalGrid, List<MySlimBlock> splitBlocks, ref MyDisconnectHelper.Group group)
        {
            int num1;
            if ((!originalGrid.IsStatic && Sync.IsServer) && group.IsValid)
            {
                int num = 0;
                for (int i = group.FirstBlockIndex; i < (group.FirstBlockIndex + group.BlockCount); i++)
                {
                    if (MyDisconnectHelper.IsDestroyedInVoxels(splitBlocks[i]) && ((((float) (num + 1)) / ((float) group.BlockCount)) > 0.4f))
                    {
                        group.IsValid = false;
                        break;
                    }
                }
            }
            if (!group.IsValid || !CanHavePhysics(splitBlocks, group.FirstBlockIndex, group.BlockCount))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) HasStandAloneBlocks(splitBlocks, group.FirstBlockIndex, group.BlockCount);
            }
            group.IsValid = (bool) num1;
            if (((group.BlockCount == 1) && (splitBlocks.Count > group.FirstBlockIndex)) && (splitBlocks[group.FirstBlockIndex] != null))
            {
                MySlimBlock block = splitBlocks[group.FirstBlockIndex];
                if (block.FatBlock is MyFracturedBlock)
                {
                    group.IsValid = false;
                    if (Sync.IsServer)
                    {
                        MyDestructionHelper.CreateFracturePiece(block.FatBlock as MyFracturedBlock, true);
                    }
                }
                else if ((block.FatBlock != null) && block.FatBlock.Components.Has<MyFractureComponentBase>())
                {
                    group.IsValid = false;
                    if (Sync.IsServer)
                    {
                        MyFractureComponentCubeBlock fractureComponent = block.GetFractureComponent();
                        if (fractureComponent != null)
                        {
                            MyDestructionHelper.CreateFracturePiece(fractureComponent, true);
                        }
                    }
                }
                else if (block.FatBlock is MyCompoundCubeBlock)
                {
                    MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
                    bool flag = true;
                    foreach (MySlimBlock block4 in fatBlock.GetBlocks())
                    {
                        flag &= block4.FatBlock.Components.Has<MyFractureComponentBase>();
                        if (!flag)
                        {
                            break;
                        }
                    }
                    if (flag)
                    {
                        group.IsValid = false;
                        if (Sync.IsServer)
                        {
                            using (List<MySlimBlock>.Enumerator enumerator = fatBlock.GetBlocks().GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    MyFractureComponentCubeBlock fractureComponent = enumerator.Current.GetFractureComponent();
                                    if (fractureComponent != null)
                                    {
                                        MyDestructionHelper.CreateFracturePiece(fractureComponent, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (group.IsValid)
            {
                MyCubeGrid item = CreateGridForSplit(originalGrid, group.EntityId);
                if (item == null)
                {
                    group.IsValid = false;
                }
                else
                {
                    originalGrid.m_tmpGrids.Add(item);
                    MoveBlocks(originalGrid, item, splitBlocks, group.FirstBlockIndex, group.BlockCount);
                    item.Render.FadeIn = false;
                    item.RebuildGrid(false);
                    Sandbox.Game.Entities.MyEntities.Add(item, true);
                    group.EntityId = item.EntityId;
                    if (item.IsStatic && Sync.IsServer)
                    {
                        MatrixD worldMatrix = item.WorldMatrix;
                        bool flag2 = MyCoordinateSystem.Static.IsLocalCoordSysExist(ref worldMatrix, (double) item.GridSize);
                        if (item.GridSizeEnum == MyCubeSize.Large)
                        {
                            if (flag2)
                            {
                                MyCoordinateSystem.Static.RegisterCubeGrid(item);
                            }
                            else
                            {
                                MyCoordinateSystem.Static.CreateCoordSys(item, MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.StaticGridAlignToCenter, true);
                            }
                        }
                    }
                }
            }
            if (!group.IsValid)
            {
                RemoveSplit(originalGrid, splitBlocks, group.FirstBlockIndex, group.BlockCount, false);
            }
        }

        public static void CreateSplits(MyCubeGrid originalGrid, List<MySlimBlock> splitBlocks, List<MyDisconnectHelper.Group> groups, MyTestDisconnectsReason reason, bool sync = true)
        {
            if (((originalGrid != null) && ((originalGrid.Physics != null) && (groups != null))) && (splitBlocks != null))
            {
                Vector3D centerOfMassWorld = originalGrid.Physics.CenterOfMassWorld;
                try
                {
                    if (MyCubeGridSmallToLargeConnection.Static != null)
                    {
                        MyCubeGridSmallToLargeConnection.Static.BeforeGridSplit_SmallToLargeGridConnectivity(originalGrid);
                    }
                    MyDisconnectHelper.Group[] internalArray = groups.GetInternalArray<MyDisconnectHelper.Group>();
                    int index = 0;
                    while (true)
                    {
                        if (index >= groups.Count)
                        {
                            originalGrid.UpdatePhysicsShape();
                            foreach (MyCubeGrid grid in originalGrid.m_tmpGrids)
                            {
                                Action <>9__1;
                                grid.RebuildGrid(false);
                                if (originalGrid.IsStatic && !MySession.Static.Settings.StationVoxelSupport)
                                {
                                    grid.TestDynamic = (reason == MyTestDisconnectsReason.SplitBlock) ? MyTestDynamicReason.GridSplitByBlock : MyTestDynamicReason.GridSplit;
                                    originalGrid.TestDynamic = (reason == MyTestDisconnectsReason.SplitBlock) ? MyTestDynamicReason.GridSplitByBlock : MyTestDynamicReason.GridSplit;
                                }
                                grid.Physics.AngularVelocity = originalGrid.Physics.AngularVelocity;
                                grid.Physics.LinearVelocity = originalGrid.Physics.GetVelocityAtPoint(grid.Physics.CenterOfMassWorld);
                                Interlocked.Increment(ref originalGrid.m_resolvingSplits);
                                Action callback = <>9__1;
                                if (<>9__1 == null)
                                {
                                    Action local1 = <>9__1;
                                    callback = <>9__1 = () => Interlocked.Decrement(ref originalGrid.m_resolvingSplits);
                                }
                                grid.UpdateDirty(callback, false);
                                grid.UpdateGravity();
                                grid.MarkForUpdate();
                            }
                            Vector3 vector = Vector3.Cross(originalGrid.Physics.AngularVelocity, (Vector3) (originalGrid.Physics.CenterOfMassWorld - centerOfMassWorld));
                            originalGrid.Physics.LinearVelocity += vector;
                            originalGrid.MarkForUpdate();
                            if (MyCubeGridSmallToLargeConnection.Static != null)
                            {
                                MyCubeGridSmallToLargeConnection.Static.AfterGridSplit_SmallToLargeGridConnectivity(originalGrid, originalGrid.m_tmpGrids);
                            }
                            Action<MyCubeGrid, MyCubeGrid> onGridSplit = originalGrid.OnGridSplit;
                            if (onGridSplit != null)
                            {
                                foreach (MyCubeGrid grid2 in originalGrid.m_tmpGrids)
                                {
                                    onGridSplit(originalGrid, grid2);
                                }
                            }
                            foreach (MyCubeGrid grid3 in originalGrid.m_tmpGrids)
                            {
                                grid3.GridSystems.UpdatePower();
                                if (grid3.GridSystems.ResourceDistributor != null)
                                {
                                    grid3.GridSystems.ResourceDistributor.MarkForUpdate();
                                }
                            }
                            if (sync && Sync.IsServer)
                            {
                                MyMultiplayer.RemoveForClientIfIncomplete(originalGrid);
                                m_tmpBlockPositions.Clear();
                                foreach (MySlimBlock block in splitBlocks)
                                {
                                    m_tmpBlockPositions.Add(block.Position);
                                }
                                foreach (MyCubeGrid local2 in originalGrid.m_tmpGrids)
                                {
                                    local2.IsSplit = true;
                                    MyMultiplayer.ReplicateImmediatelly(MyExternalReplicable.FindByObject(local2), MyExternalReplicable.FindByObject(originalGrid));
                                    local2.IsSplit = false;
                                }
                                EndpointId targetEndpoint = new EndpointId();
                                MyMultiplayer.RaiseEvent<MyCubeGrid, List<Vector3I>, List<MyDisconnectHelper.Group>>(originalGrid, x => new Action<List<Vector3I>, List<MyDisconnectHelper.Group>>(x.CreateSplits_Implementation), m_tmpBlockPositions, groups, targetEndpoint);
                            }
                            break;
                        }
                        CreateSplitForGroup(originalGrid, splitBlocks, ref internalArray[index]);
                        index++;
                    }
                }
                finally
                {
                    originalGrid.m_tmpGrids.Clear();
                }
            }
        }

        [Event(null, 0x58a), Reliable, Broadcast]
        public void CreateSplits_Implementation(List<Vector3I> blocks, List<MyDisconnectHelper.Group> groups)
        {
            if (!base.MarkedForClose)
            {
                m_tmpBlockListReceive.Clear();
                int num = 0;
                while (num < groups.Count)
                {
                    MyDisconnectHelper.Group group = groups[num];
                    int blockCount = group.BlockCount;
                    int firstBlockIndex = group.FirstBlockIndex;
                    while (true)
                    {
                        if (firstBlockIndex >= (group.FirstBlockIndex + group.BlockCount))
                        {
                            groups[num] = group;
                            num++;
                            break;
                        }
                        MySlimBlock cubeBlock = this.GetCubeBlock(blocks[firstBlockIndex]);
                        if (cubeBlock == null)
                        {
                            MySandboxGame.Log.WriteLine("Block was null when trying to create a grid split. Desync?");
                            if ((blockCount - 1) == 0)
                            {
                                group.IsValid = false;
                            }
                        }
                        m_tmpBlockListReceive.Add(cubeBlock);
                        firstBlockIndex++;
                    }
                }
                CreateSplits(this, m_tmpBlockListReceive, groups, MyTestDisconnectsReason.BlockRemoved, false);
                m_tmpBlockListReceive.Clear();
            }
        }

        public void CreateStructuralIntegrity()
        {
            if (this.m_gridSizeEnum == MyCubeSize.Large)
            {
                this.StructuralIntegrity = new MyStructuralIntegrity(this);
            }
        }

        private void CreateSystems()
        {
            object[] args = new object[] { this };
            this.GridSystems = (MyCubeGridSystems) Activator.CreateInstance(m_gridSystemsType, args);
        }

        public bool CubeExists(Vector3I pos) => 
            this.m_cubes.ContainsKey(pos);

        private void DampenersEnabledChanged()
        {
            this.EnableDampingInternal(this.m_dampenersEnabled.Value, false);
        }

        public void DebugDrawPositions(List<Vector3I> positions)
        {
            foreach (Vector3I vectori in positions)
            {
                Vector3 vector1 = vectori + 1;
                MyOrientedBoundingBoxD obb = new MyOrientedBoundingBoxD((Vector3D) (vectori * this.GridSize), this.GridSizeHalfVector, Quaternion.Identity);
                obb.Transform(base.WorldMatrix);
                MyRenderProxy.DebugDrawOBB(obb, Color.White.ToVector3(), 0.5f, true, false, false);
            }
        }

        public void DebugDrawRange(Vector3I min, Vector3I max)
        {
            Vector3I next = min;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref min, ref max);
            while (iterator.IsValid())
            {
                Vector3 vector1 = next + 1;
                MyOrientedBoundingBoxD obb = new MyOrientedBoundingBoxD((Vector3D) (next * this.GridSize), this.GridSizeHalfVector, Quaternion.Identity);
                obb.Transform(base.WorldMatrix);
                MyRenderProxy.DebugDrawOBB(obb, Color.White, 0.5f, true, false, false);
                iterator.GetNext(out next);
            }
        }

        [Event(null, 0x2b8f), Reliable, Server, Broadcast]
        public static void DepressurizeEffect(long gridId, Vector3I from, Vector3I to)
        {
            MySandboxGame.Static.Invoke(() => DepressurizeEffect_Implementation(gridId, from, to), "CubeGrid - DepressurizeEffect");
        }

        public static void DepressurizeEffect_Implementation(long gridId, Vector3I from, Vector3I to)
        {
            MyCubeGrid entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(gridId, false) as MyCubeGrid;
            if (entityById != null)
            {
                MyGridGasSystem.AddDepressurizationEffects(entityById, from, to);
            }
        }

        public override void DeserializeControls(BitStream stream, bool outOfOrder)
        {
            if (!stream.ReadBool())
            {
                this.m_lastNetState.Valid = false;
            }
            else
            {
                MyGridClientState state = new MyGridClientState(stream);
                if (!outOfOrder)
                {
                    this.m_lastNetState = state;
                }
                MyShipController shipController = this.GridSystems.ControlSystem.GetShipController();
                if ((shipController != null) && !shipController.ControllerInfo.IsLocallyControlled())
                {
                    shipController.SetNetState(this.m_lastNetState);
                }
            }
        }

        private void DetectDisconnects()
        {
            if ((MyFakes.DETECT_DISCONNECTS && (this.m_cubes.Count != 0)) && Sync.IsServer)
            {
                MyPerformanceCounter.PerCameraDrawRead.CustomTimers.Remove("Mount points");
                MyPerformanceCounter.PerCameraDrawWrite.CustomTimers.Remove("Mount points");
                MyPerformanceCounter.PerCameraDrawRead.CustomTimers.Remove("Disconnect");
                MyPerformanceCounter.PerCameraDrawWrite.CustomTimers.Remove("Disconnect");
                MyPerformanceCounter.PerCameraDrawRead.StartTimer("Disconnect");
                MyPerformanceCounter.PerCameraDrawWrite.StartTimer("Disconnect");
                m_disconnectHelper.Disconnect(this, this.m_disconnectsDirty, null, false);
                this.m_disconnectsDirty = MyTestDisconnectsReason.NoReason;
                MyPerformanceCounter.PerCameraDrawRead.StopTimer("Disconnect");
                MyPerformanceCounter.PerCameraDrawWrite.StopTimer("Disconnect");
            }
        }

        public void DetectDisconnectsAfterFrame()
        {
            this.m_disconnectsDirty = MyTestDisconnectsReason.BlockRemoved;
            this.MarkForUpdate();
        }

        public MyCubeGrid DetectMerge(MySlimBlock block, MyCubeGrid ignore = null, List<VRage.Game.Entity.MyEntity> nearEntities = null, bool newGrid = false)
        {
            if (!this.IsStatic)
            {
                return null;
            }
            if (!Sync.IsServer)
            {
                return null;
            }
            if (block == null)
            {
                return null;
            }
            MyCubeGrid grid = null;
            BoundingBoxD boundingBox = new BoundingBox((Vector3) ((block.Min * this.GridSize) - this.GridSizeHalf), (block.Max * this.GridSize) + this.GridSizeHalf);
            boundingBox.Inflate((double) this.GridSizeHalf);
            boundingBox = boundingBox.TransformFast(base.WorldMatrix);
            bool flag = false;
            if (nearEntities == null)
            {
                flag = true;
                nearEntities = Sandbox.Game.Entities.MyEntities.GetEntitiesInAABB(ref boundingBox, false);
            }
            for (int i = 0; i < nearEntities.Count; i++)
            {
                Vector3I vectori;
                MyCubeGrid objA = nearEntities[i] as MyCubeGrid;
                MyCubeGrid grid3 = grid ?? this;
                if (((objA != null) && (!ReferenceEquals(objA, this) && (!ReferenceEquals(objA, ignore) && ((objA.Physics != null) && (objA.Physics.Enabled && (objA.IsStatic && (objA.GridSizeEnum == grid3.GridSizeEnum))))))) && grid3.IsMergePossible_Static(block, objA, out vectori))
                {
                    MyCubeGrid grid4 = grid3;
                    MyCubeGrid grid5 = objA;
                    if ((objA.BlocksCount > grid3.BlocksCount) | newGrid)
                    {
                        grid4 = objA;
                        grid5 = grid3;
                    }
                    Vector3I blockOffset = Vector3I.Round(Vector3D.Transform(grid5.PositionComp.GetPosition(), grid4.PositionComp.WorldMatrixNormalizedInv) * this.GridSizeR);
                    if (grid4.CanMoveBlocksFrom(grid5, blockOffset))
                    {
                        if (newGrid)
                        {
                            MyMultiplayer.ReplicateImmediatelly(MyExternalReplicable.FindByObject(this), MyExternalReplicable.FindByObject(grid4));
                        }
                        MyCubeGrid grid6 = grid4.MergeGrid_Static(grid5, blockOffset, block);
                        if (grid6 != null)
                        {
                            grid = grid6;
                        }
                    }
                }
            }
            if (flag)
            {
                nearEntities.Clear();
            }
            return grid;
        }

        public void DismountAllCockpits()
        {
            using (HashSet<MySlimBlock>.Enumerator enumerator = this.GetBlocks().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyCockpit fatBlock = enumerator.Current.FatBlock as MyCockpit;
                    if ((fatBlock != null) && (fatBlock.Pilot != null))
                    {
                        fatBlock.Use();
                    }
                }
            }
        }

        public void DoDamage(float damage, MyHitInfo hitInfo, Vector3? localPos = new Vector3?(), long attackerId = 0L)
        {
            if (Sync.IsServer && MySessionComponentSafeZones.IsActionAllowed(this, MySafeZoneAction.Damage, 0L))
            {
                Vector3I vectori;
                if (localPos != null)
                {
                    this.FixTargetCube(out vectori, localPos.Value * this.GridSizeR);
                }
                else
                {
                    this.FixTargetCube(out vectori, (Vector3) (Vector3D.Transform(hitInfo.Position, base.PositionComp.WorldMatrixInvScaled) * this.GridSizeR));
                }
                MySlimBlock cubeBlock = this.GetCubeBlock(vectori);
                if (cubeBlock != null)
                {
                    if (MyFakes.ENABLE_FRACTURE_COMPONENT)
                    {
                        ushort? contactCompoundId = null;
                        MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                        if (fatBlock != null)
                        {
                            contactCompoundId = this.Physics.GetContactCompoundId(cubeBlock.Position, hitInfo.Position);
                            if (contactCompoundId == null)
                            {
                                return;
                            }
                            MySlimBlock block = fatBlock.GetBlock(contactCompoundId.Value);
                            if (block == null)
                            {
                                return;
                            }
                            cubeBlock = block;
                        }
                    }
                    this.ApplyDestructionDeformation(cubeBlock, damage, new MyHitInfo?(hitInfo), attackerId);
                }
            }
        }

        private void DoLazyUpdates()
        {
            if (((MyCubeGridSmallToLargeConnection.Static != null) && !this.m_smallToLargeConnectionsInitialized) && this.m_enableSmallToLargeConnections)
            {
                this.m_smallToLargeConnectionsInitialized = true;
                MyCubeGridSmallToLargeConnection.Static.AddGridSmallToLargeConnection(this);
            }
            this.m_smallToLargeConnectionsInitialized = true;
            if (!MyPerGameSettings.Destruction && (this.BonesToSend.InputCount > 0))
            {
                int bonesSendCounter = this.m_bonesSendCounter;
                this.m_bonesSendCounter = bonesSendCounter + 1;
                if ((bonesSendCounter > 10) && !this.m_bonesSending)
                {
                    this.m_bonesSendCounter = 0;
                    MyVoxelSegmentation bonesToSend = this.BonesToSend;
                    lock (bonesToSend)
                    {
                        MyVoxelSegmentation segmentation2 = this.BonesToSend;
                        this.BonesToSend = this.m_bonesToSendSecond;
                        this.m_bonesToSendSecond = segmentation2;
                    }
                    int inputCount = this.m_bonesToSendSecond.InputCount;
                    if (Sync.IsServer)
                    {
                        this.m_bonesSending = true;
                        this.m_workData.Priority = WorkPriority.Low;
                        Parallel.Start(new Action<WorkData>(this.SendBonesAsync), null, this.m_workData);
                    }
                }
            }
            if (this.m_blocksForDamageApplicationDirty)
            {
                this.m_blocksForDamageApplicationCopy.AddHashset<MySlimBlock>(this.m_blocksForDamageApplication);
                foreach (MySlimBlock block in this.m_blocksForDamageApplicationCopy)
                {
                    if (block.AccumulatedDamage > 0f)
                    {
                        block.ApplyAccumulatedDamage(true, 0L);
                    }
                }
                this.m_blocksForDamageApplication.Clear();
                this.m_blocksForDamageApplicationCopy.Clear();
                this.m_blocksForDamageApplicationDirty = false;
            }
            if (this.m_disconnectsDirty != MyTestDisconnectsReason.NoReason)
            {
                this.DetectDisconnects();
            }
            if (!MyPerGameSettings.Destruction)
            {
                this.Skeleton.RemoveUnusedBones(this);
            }
            if (this.m_ownershipManager.NeedRecalculateOwners)
            {
                this.m_ownershipManager.RecalculateOwners();
                this.m_ownershipManager.NeedRecalculateOwners = false;
                this.NotifyBlockOwnershipChange(this);
            }
        }

        private static void DrawObjectGizmo(MySlimBlock block)
        {
            IMyGizmoDrawableObject fatBlock = block.FatBlock as IMyGizmoDrawableObject;
            if (fatBlock.CanBeDrawn())
            {
                MyStringId? nullable2;
                Color gizmoColor = fatBlock.GetGizmoColor();
                MatrixD worldMatrix = fatBlock.GetWorldMatrix();
                BoundingBox? boundingBox = fatBlock.GetBoundingBox();
                if (boundingBox != null)
                {
                    BoundingBoxD localbox = boundingBox.Value;
                    nullable2 = null;
                    nullable2 = null;
                    MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref localbox, ref gizmoColor, MySimpleObjectRasterizer.SolidAndWireframe, 1, GetLineWidthForGizmo(fatBlock, boundingBox.Value), nullable2, nullable2, false, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
                }
                else
                {
                    float radius = fatBlock.GetRadius();
                    MySector.MainCamera.GetDistanceFromPoint(worldMatrix.Translation);
                    float lineThickness = 0.002f * Math.Min(100f, Math.Abs((float) (radius - ((float) MySector.MainCamera.GetDistanceFromPoint(worldMatrix.Translation)))));
                    nullable2 = null;
                    nullable2 = null;
                    MySimpleObjectDraw.DrawTransparentSphere(ref worldMatrix, radius, ref gizmoColor, MySimpleObjectRasterizer.SolidAndWireframe, 20, nullable2, nullable2, lineThickness, -1, null, MyBillboard.BlendTypeEnum.Standard, 1f);
                    if (fatBlock.EnableLongDrawDistance() && MyFakes.ENABLE_LONG_DISTANCE_GIZMO_DRAWING)
                    {
                        MyBillboardViewProjection projection;
                        projection.CameraPosition = MySector.MainCamera.Position;
                        projection.ViewAtZero = new Matrix();
                        projection.Viewport = MySector.MainCamera.Viewport;
                        float aspectRatio = projection.Viewport.Width / projection.Viewport.Height;
                        projection.Projection = Matrix.CreatePerspectiveFieldOfView(MySector.MainCamera.FieldOfView, aspectRatio, 1f, 100f);
                        projection.Projection.M33 = -1f;
                        projection.Projection.M34 = -1f;
                        projection.Projection.M43 = 0f;
                        projection.Projection.M44 = 0f;
                        int id = 10;
                        MyRenderProxy.AddBillboardViewProjection(id, projection);
                        nullable2 = null;
                        nullable2 = null;
                        MySimpleObjectDraw.DrawTransparentSphere(ref worldMatrix, radius, ref gizmoColor, MySimpleObjectRasterizer.SolidAndWireframe, 20, nullable2, nullable2, lineThickness, id, null, MyBillboard.BlendTypeEnum.Standard, 1f);
                    }
                }
            }
        }

        internal void EnableDampingInternal(bool enableDampeners, bool updateProxy)
        {
            if ((this.EntityThrustComponent != null) && (this.EntityThrustComponent.DampenersEnabled != enableDampeners))
            {
                this.EntityThrustComponent.DampenersEnabled = enableDampeners;
                this.m_dampenersEnabled.Value = enableDampeners;
                if (((this.Physics != null) && (this.Physics.RigidBody != null)) && !this.Physics.RigidBody.IsActive)
                {
                    this.ActivatePhysics();
                }
                if (MySession.Static.LocalHumanPlayer != null)
                {
                    MyCockpit controlledEntity = MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCockpit;
                    if ((controlledEntity != null) && ReferenceEquals(controlledEntity.CubeGrid, this))
                    {
                        if (this.m_inertiaDampenersNotification == null)
                        {
                            MyStringId text = new MyStringId();
                            this.m_inertiaDampenersNotification = new MyHudNotification(text, 0x9c4, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        }
                        this.m_inertiaDampenersNotification.Text = this.EntityThrustComponent.DampenersEnabled ? MyCommonTexts.NotificationInertiaDampenersOn : MyCommonTexts.NotificationInertiaDampenersOff;
                        MyHud.Notifications.Add(this.m_inertiaDampenersNotification);
                        MyHud.ShipInfo.Reload();
                        MyHud.SinkGroupInfo.Reload();
                    }
                }
            }
        }

        public bool EnableGenerators(bool enable, bool fromServer = false)
        {
            bool generatorsEnabled = this.m_generatorsEnabled;
            if (Sync.IsServer | fromServer)
            {
                if (this.Render == null)
                {
                    this.m_generatorsEnabled = false;
                    return false;
                }
                if (this.m_generatorsEnabled != enable)
                {
                    this.AdditionalModelGenerators.ForEach(delegate (IMyBlockAdditionalModelGenerator g) {
                        g.EnableGenerator(enable);
                    });
                    this.m_generatorsEnabled = enable;
                }
            }
            return generatorsEnabled;
        }

        public void EnqueueDestroyedBlock(Vector3I position)
        {
            if (Sync.IsServer)
            {
                this.m_destroyBlockQueue.Add(position);
                this.MarkForUpdate();
            }
        }

        public unsafe void EnqueueDestroyedBlockWithId(Vector3I position, ushort? compoundId, bool generatorEnabled)
        {
            if (Sync.IsServer)
            {
                BlockPositionId id;
                ushort? nullable;
                if (generatorEnabled)
                {
                    BlockPositionId* idPtr1;
                    id = new BlockPositionId {
                        Position = position
                    };
                    nullable = compoundId;
                    idPtr1->CompoundId = (nullable != null) ? ((uint) nullable.GetValueOrDefault()) : uint.MaxValue;
                    idPtr1 = (BlockPositionId*) ref id;
                    this.m_destroyBlockWithIdQueueWithGenerators.Add(id);
                }
                else
                {
                    BlockPositionId* idPtr2;
                    id = new BlockPositionId {
                        Position = position
                    };
                    nullable = compoundId;
                    idPtr2->CompoundId = (nullable != null) ? ((uint) nullable.GetValueOrDefault()) : uint.MaxValue;
                    idPtr2 = (BlockPositionId*) ref id;
                    this.m_destroyBlockWithIdQueueWithoutGenerators.Add(id);
                }
                this.MarkForUpdate();
            }
        }

        public void EnqueueDestructionDeformationBlock(Vector3I position)
        {
            if (Sync.IsServer)
            {
                this.m_destructionDeformationQueue.Add(position);
                this.MarkForUpdate();
            }
        }

        public void EnqueueRemovedBlock(Vector3I position, bool generatorsEnabled)
        {
            if (Sync.IsServer)
            {
                if (generatorsEnabled)
                {
                    this.m_removeBlockQueueWithGenerators.Add(position);
                }
                else
                {
                    this.m_removeBlockQueueWithoutGenerators.Add(position);
                }
                this.MarkForUpdate();
            }
        }

        public unsafe void EnqueueRemovedBlockWithId(Vector3I position, ushort? compoundId, bool generatorsEnabled)
        {
            if (Sync.IsServer)
            {
                BlockPositionId* idPtr1;
                BlockPositionId id2 = new BlockPositionId {
                    Position = position
                };
                ushort? nullable = compoundId;
                idPtr1->CompoundId = (nullable != null) ? ((uint) nullable.GetValueOrDefault()) : uint.MaxValue;
                idPtr1 = (BlockPositionId*) ref id2;
                BlockPositionId item = id2;
                if (generatorsEnabled)
                {
                    this.m_removeBlockWithIdQueueWithGenerators.Add(item);
                }
                else
                {
                    this.m_removeBlockWithIdQueueWithoutGenerators.Add(item);
                }
                this.MarkForUpdate();
            }
        }

        public static void ExportObject(MyCubeGrid baseGrid, bool convertModelsFromSBC, bool exportObjAndSBC = false)
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.NONE_TIMEOUT, new StringBuilder(MyTexts.GetString(MyCommonTexts.ExportingToObj)), null, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                List<MyCubeGrid> baseGrids = new List<MyCubeGrid>();
                foreach (MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node node in MyCubeGridGroups.Static.Logical.GetGroup(baseGrid).Nodes)
                {
                    baseGrids.Add(node.NodeData);
                }
                ExportToObjFile(baseGrids, convertModelsFromSBC, exportObjAndSBC);
            }, 0x3e8, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private static void ExportToObjFile(List<MyCubeGrid> baseGrids, bool convertModelsFromSBC, bool exportObjAndSBC)
        {
            MyStringId? nullable;
            Vector2? nullable2;
            materialID = 0;
            MyValueFormatter.GetFormatedDateTimeForFilename(DateTime.Now);
            string name = MyUtils.StripInvalidChars(baseGrids[0].DisplayName.Replace(' ', '_'));
            string userDataPath = MyFileSystem.UserDataPath;
            string str2 = "ExportedModels";
            if (!convertModelsFromSBC | exportObjAndSBC)
            {
                userDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                str2 = MyPerGameSettings.GameNameSafe + "_ExportedModels";
            }
            string folder = Path.Combine(userDataPath, str2, name);
            int num = 0;
            while (Directory.Exists(folder))
            {
                num++;
                folder = Path.Combine(userDataPath, str2, $"{name}_{num:000}");
            }
            MyUtils.CreateFolder(folder);
            if (!convertModelsFromSBC | exportObjAndSBC)
            {
                bool flag = false;
                string prefabPath = Path.Combine(folder, name + ".sbc");
                using (List<MyCubeGrid>.Enumerator enumerator = baseGrids.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        using (HashSet<MySlimBlock>.Enumerator enumerator2 = enumerator.Current.CubeBlocks.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                if (!enumerator2.Current.BlockDefinition.Context.IsBaseGame)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (flag)
                {
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.ExportToObjModded), folder)), null, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else
                {
                    CreatePrefabFile(baseGrids, name, prefabPath);
                    MyRenderProxy.TakeScreenshot(tumbnailMultiplier, Path.Combine(folder, name + ".png"), false, true, false);
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.ExportToObjComplete), folder)), null, nullable, nullable, nullable, nullable, result => PackFiles(folder, name), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
            }
            if (exportObjAndSBC | convertModelsFromSBC)
            {
                List<Vector3> vertices = new List<Vector3>();
                List<TriangleWithMaterial> triangles = new List<TriangleWithMaterial>();
                List<Vector2> uvs = new List<Vector2>();
                List<MyExportModel.Material> materials = new List<MyExportModel.Material>();
                int currVerticesCount = 0;
                try
                {
                    GetModelDataFromGrid(baseGrids, vertices, triangles, uvs, materials, currVerticesCount);
                    string filename = Path.Combine(folder, name + ".obj");
                    string matFilename = Path.Combine(folder, name + ".mtl");
                    CreateObjFile(name, filename, matFilename, vertices, triangles, uvs, materials, currVerticesCount);
                    List<renderColoredTextureProperties> texturesToRender = new List<renderColoredTextureProperties>();
                    CreateMaterialFile(folder, matFilename, materials, texturesToRender);
                    if (texturesToRender.Count > 0)
                    {
                        MyRenderProxy.RenderColoredTextures(texturesToRender);
                    }
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.NONE_TIMEOUT, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.ExportToObjComplete), folder)), null, nullable, nullable, nullable, nullable, result => ConvertNextGrid(false), 0x3e8, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                catch (Exception exception)
                {
                    MySandboxGame.Log.WriteLine("Error while exporting to obj file.");
                    MySandboxGame.Log.WriteLine(exception.ToString());
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.ExportToObjFailed), folder)), null, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
            }
        }

        private static void ExtractModelDataForObj(MyModel model, Matrix matrix, List<Vector3> vertices, List<TriangleWithMaterial> triangles, List<Vector2> uvs, ref Vector2 offsetUV, List<MyExportModel.Material> materials, ref int currVerticesCount, Vector3 colorMaskHSV)
        {
            if (!model.HasUV)
            {
                model.LoadUV = true;
                model.UnloadData();
                model.LoadData();
            }
            MyExportModel renderModel = new MyExportModel(model);
            int verticesCount = renderModel.GetVerticesCount();
            List<HalfVector2> uVsForModel = GetUVsForModel(renderModel, verticesCount);
            if (uVsForModel.Count == verticesCount)
            {
                List<MyExportModel.Material> list2 = CreateMaterialsForModel(materials, colorMaskHSV, renderModel);
                for (int i = 0; i < verticesCount; i++)
                {
                    vertices.Add(Vector3.Transform(model.GetVertex(i), matrix));
                    HalfVector2 vector2 = uVsForModel[i];
                    Vector2 vector = (vector2.ToVector2() / model.PatternScale) + offsetUV;
                    uvs.Add(new Vector2(vector.X, -vector.Y));
                }
                int index = 0;
                while (index < renderModel.GetTrianglesCount())
                {
                    int num4 = -1;
                    int num5 = 0;
                    while (true)
                    {
                        if (num5 < list2.Count)
                        {
                            if (index > list2[num5].LastTri)
                            {
                                num5++;
                                continue;
                            }
                            num4 = num5;
                        }
                        MyTriangleVertexIndices triangle = renderModel.GetTriangle(index);
                        string exportedMaterialName = "EmptyMaterial";
                        if (num4 != -1)
                        {
                            exportedMaterialName = list2[num4].ExportedMaterialName;
                        }
                        TriangleWithMaterial item = new TriangleWithMaterial {
                            material = exportedMaterialName,
                            triangle = new MyTriangleVertexIndices((triangle.I0 + 1) + currVerticesCount, (triangle.I1 + 1) + currVerticesCount, (triangle.I2 + 1) + currVerticesCount),
                            uvIndices = new MyTriangleVertexIndices((triangle.I0 + 1) + currVerticesCount, (triangle.I1 + 1) + currVerticesCount, (triangle.I2 + 1) + currVerticesCount)
                        };
                        triangles.Add(item);
                        index++;
                        break;
                    }
                }
                currVerticesCount += verticesCount;
            }
        }

        public HashSet<MySlimBlock> FindBlocksBuiltByID(long identityID) => 
            this.FindBlocksBuiltByID(identityID, new HashSet<MySlimBlock>());

        public HashSet<MySlimBlock> FindBlocksBuiltByID(long identityID, HashSet<MySlimBlock> builtBlocks)
        {
            foreach (MySlimBlock block in this.m_cubeBlocks)
            {
                if (block.BuiltBy == identityID)
                {
                    builtBlocks.Add(block);
                }
            }
            return builtBlocks;
        }

        internal static MyObjectBuilder_CubeBlock FindDefinitionUpgrade(MyObjectBuilder_CubeBlock block, out MyCubeBlockDefinition blockDefinition)
        {
            using (IEnumerator<MyCubeBlockDefinition> enumerator = MyDefinitionManager.Static.GetAllDefinitions().OfType<MyCubeBlockDefinition>().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyCubeBlockDefinition current = enumerator.Current;
                    if ((current.Id.SubtypeId == block.SubtypeId) && !string.IsNullOrEmpty(block.SubtypeId.String))
                    {
                        blockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(current.Id);
                        return MyObjectBuilder_CubeBlock.Upgrade(block, blockDefinition.Id.TypeId, block.SubtypeName);
                    }
                }
            }
            blockDefinition = null;
            return null;
        }

        public void FixSkeleton(MySlimBlock cubeBlock)
        {
            float maxBoneError = MyGridSkeleton.GetMaxBoneError(this.GridSize);
            maxBoneError *= maxBoneError;
            Vector3I end = (Vector3I) ((cubeBlock.Min + Vector3I.One) * 2);
            Vector3I start = cubeBlock.Min * 2;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref end);
            while (iterator.IsValid())
            {
                Vector3 vector = this.Skeleton.GetDefinitionOffsetWithNeighbours(cubeBlock.Min, start, this);
                if (vector.LengthSquared() < maxBoneError)
                {
                    this.Skeleton.Bones.Remove<Vector3I, Vector3>(start);
                }
                else
                {
                    this.Skeleton.Bones[start] = vector;
                }
                iterator.GetNext(out start);
            }
            if (((cubeBlock.BlockDefinition.Skeleton != null) && (cubeBlock.BlockDefinition.Skeleton.Count > 0)) && (this.Physics != null))
            {
                int x = -1;
                while (x <= 1)
                {
                    int y = -1;
                    while (true)
                    {
                        if (y > 1)
                        {
                            x++;
                            break;
                        }
                        int z = -1;
                        while (true)
                        {
                            if (z > 1)
                            {
                                y++;
                                break;
                            }
                            this.SetCubeDirty((Vector3I) (new Vector3I(x, y, z) + cubeBlock.Min));
                            z++;
                        }
                    }
                }
            }
        }

        public void FixTargetCube(out Vector3I cube, Vector3 fractionalGridPosition)
        {
            cube = Vector3I.Round(fractionalGridPosition);
            fractionalGridPosition += new Vector3(0.5f);
            if (!this.m_cubes.ContainsKey(cube))
            {
                Vector3 vector = fractionalGridPosition - cube;
                Vector3 vector2 = new Vector3(1f) - vector;
                m_neighborDistances[1] = vector.X;
                m_neighborDistances[0] = vector2.X;
                m_neighborDistances[3] = vector.Y;
                m_neighborDistances[2] = vector2.Y;
                m_neighborDistances[5] = vector.Z;
                m_neighborDistances[4] = vector2.Z;
                Vector3 vector3 = vector * vector;
                Vector3 vector4 = vector2 * vector2;
                m_neighborDistances[9] = (float) Math.Sqrt((double) (vector3.X + vector3.Y));
                m_neighborDistances[8] = (float) Math.Sqrt((double) (vector3.X + vector4.Y));
                m_neighborDistances[7] = (float) Math.Sqrt((double) (vector4.X + vector3.Y));
                m_neighborDistances[6] = (float) Math.Sqrt((double) (vector4.X + vector4.Y));
                m_neighborDistances[0x11] = (float) Math.Sqrt((double) (vector3.X + vector3.Z));
                m_neighborDistances[0x10] = (float) Math.Sqrt((double) (vector3.X + vector4.Z));
                m_neighborDistances[15] = (float) Math.Sqrt((double) (vector4.X + vector3.Z));
                m_neighborDistances[14] = (float) Math.Sqrt((double) (vector4.X + vector4.Z));
                m_neighborDistances[13] = (float) Math.Sqrt((double) (vector3.Y + vector3.Z));
                m_neighborDistances[12] = (float) Math.Sqrt((double) (vector3.Y + vector4.Z));
                m_neighborDistances[11] = (float) Math.Sqrt((double) (vector4.Y + vector3.Z));
                m_neighborDistances[10] = (float) Math.Sqrt((double) (vector4.Y + vector4.Z));
                Vector3 vector5 = vector3 * vector;
                Vector3 vector6 = vector4 * vector2;
                m_neighborDistances[0x19] = (float) Math.Pow((double) ((vector5.X + vector5.Y) + vector5.Z), 0.33333333333333331);
                m_neighborDistances[0x18] = (float) Math.Pow((double) ((vector5.X + vector5.Y) + vector6.Z), 0.33333333333333331);
                m_neighborDistances[0x17] = (float) Math.Pow((double) ((vector5.X + vector6.Y) + vector5.Z), 0.33333333333333331);
                m_neighborDistances[0x16] = (float) Math.Pow((double) ((vector5.X + vector6.Y) + vector6.Z), 0.33333333333333331);
                m_neighborDistances[0x15] = (float) Math.Pow((double) ((vector6.X + vector5.Y) + vector5.Z), 0.33333333333333331);
                m_neighborDistances[20] = (float) Math.Pow((double) ((vector6.X + vector5.Y) + vector6.Z), 0.33333333333333331);
                m_neighborDistances[0x13] = (float) Math.Pow((double) ((vector6.X + vector6.Y) + vector5.Z), 0.33333333333333331);
                m_neighborDistances[0x12] = (float) Math.Pow((double) ((vector6.X + vector6.Y) + vector6.Z), 0.33333333333333331);
                int num = 0;
                while (num < 0x19)
                {
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= (0x19 - num))
                        {
                            num++;
                            break;
                        }
                        float num3 = m_neighborDistances[m_neighborOffsetIndices[num2 + 1]];
                        if (m_neighborDistances[m_neighborOffsetIndices[num2]] > num3)
                        {
                            NeighborOffsetIndex index = m_neighborOffsetIndices[num2];
                            m_neighborOffsetIndices[num2] = m_neighborOffsetIndices[num2 + 1];
                            m_neighborOffsetIndices[num2 + 1] = index;
                        }
                        num2++;
                    }
                }
                Vector3I vectori = new Vector3I();
                for (int i = 0; i < m_neighborOffsets.Count; i++)
                {
                    vectori = m_neighborOffsets[m_neighborOffsetIndices[i]];
                    if (this.m_cubes.ContainsKey(((Vector3I) cube) + vectori))
                    {
                        cube = (Vector3I) (cube + vectori);
                        return;
                    }
                }
            }
        }

        public void FixTargetCubeLite(out Vector3I cube, Vector3D fractionalGridPosition)
        {
            cube = Vector3I.Round(fractionalGridPosition - 0.5);
        }

        [Event(null, 0x1717), Reliable, Broadcast]
        private void FractureComponentRepaired(Vector3I pos, ushort subBlockId, long toolOwner)
        {
            MyCompoundCubeBlock fatBlock = null;
            MySlimBlock cubeBlock = this.GetCubeBlock(pos);
            if (cubeBlock != null)
            {
                fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
            }
            if (fatBlock != null)
            {
                cubeBlock = fatBlock.GetBlock(subBlockId);
            }
            if ((cubeBlock != null) && (cubeBlock.FatBlock != null))
            {
                cubeBlock.RepairFracturedBlock(toolOwner);
            }
        }

        private unsafe void GetAllBuildOffsetsExcept(ref MyBlockBuildArea area, HashSet<Vector3UByte> exceptList, List<Vector3UByte> resultOffsets)
        {
            Vector3UByte num;
            num.X = 0;
            while (num.X < area.BuildAreaSize.X)
            {
                num.Y = 0;
                while (true)
                {
                    if (num.Y >= area.BuildAreaSize.Y)
                    {
                        byte* numPtr3 = (byte*) ref num.X;
                        numPtr3[0] = (byte) (numPtr3[0] + 1);
                        break;
                    }
                    num.Z = 0;
                    while (true)
                    {
                        if (num.Z >= area.BuildAreaSize.Z)
                        {
                            byte* numPtr2 = (byte*) ref num.Y;
                            numPtr2[0] = (byte) (numPtr2[0] + 1);
                            break;
                        }
                        if (!exceptList.Contains(num))
                        {
                            resultOffsets.Add(num);
                        }
                        byte* numPtr1 = (byte*) ref num.Z;
                        numPtr1[0] = (byte) (numPtr1[0] + 1);
                    }
                }
            }
        }

        public MyCubeGrid GetBiggestGridInGroup()
        {
            MyCubeGrid nodeData = this;
            double num = 0.0;
            foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node node in MyCubeGridGroups.Static.Physical.GetGroup(this).Nodes)
            {
                BoundingBoxD worldAABB = node.NodeData.PositionComp.WorldAABB;
                Vector3D size = worldAABB.Size;
                double volume = size.Volume;
                if (volume > num)
                {
                    num = volume;
                    nodeData = node.NodeData;
                }
            }
            return nodeData;
        }

        private void GetBlockIntersection(MyCube cube, ref LineD line, IntersectionFlags flags, out MyIntersectionResultLineTriangleEx? t, out int cubePartIndex)
        {
            if (cube.CubeBlock.FatBlock == null)
            {
                MyIntersectionResultLineTriangleEx? nullable4 = null;
                float maxValue = float.MaxValue;
                int num4 = -1;
                for (int i = 0; i < cube.Parts.Length; i++)
                {
                    MyCubePart part = cube.Parts[i];
                    MatrixD matrix = part.InstanceData.LocalMatrix * base.WorldMatrix;
                    MatrixD customInvMatrix = MatrixD.Invert(matrix);
                    t = part.Model.GetTrianglePruningStructure().GetIntersectionWithLine(this, ref line, ref customInvMatrix, flags);
                    if (t != 0)
                    {
                        MyIntersectionResultLineTriangleEx triangle = t.Value;
                        float num6 = Vector3.Distance((Vector3) Vector3.Transform(t.Value.IntersectionPointInObjectSpace, matrix), (Vector3) line.From);
                        if (num6 < maxValue)
                        {
                            maxValue = num6;
                            Matrix localMatrix = part.InstanceData.LocalMatrix;
                            MatrixD? cubeWorldMatrix = null;
                            this.TransformCubeToGrid(ref triangle, ref localMatrix, ref cubeWorldMatrix);
                            triangle.IntersectionPointInWorldSpace;
                            nullable4 = new MyIntersectionResultLineTriangleEx?(triangle);
                            num4 = i;
                        }
                    }
                }
                t = nullable4;
                cubePartIndex = num4;
            }
            else
            {
                if (cube.CubeBlock.FatBlock is MyCompoundCubeBlock)
                {
                    MyIntersectionResultLineTriangleEx? nullable = null;
                    double maxValue = double.MaxValue;
                    foreach (MySlimBlock block in (cube.CubeBlock.FatBlock as MyCompoundCubeBlock).GetBlocks())
                    {
                        Matrix matrix;
                        Vector3 vector;
                        block.Orientation.GetMatrix(out matrix);
                        Vector3.TransformNormal(ref block.BlockDefinition.ModelOffset, ref matrix, out vector);
                        matrix.Translation = (block.Position * this.GridSize) + vector;
                        MatrixD customInvMatrix = MatrixD.Invert(block.FatBlock.WorldMatrix);
                        t = block.FatBlock.ModelCollision.GetTrianglePruningStructure().GetIntersectionWithLine(this, ref line, ref customInvMatrix, flags);
                        if ((t == 0) && (block.FatBlock.Subparts != null))
                        {
                            foreach (KeyValuePair<string, MyEntitySubpart> pair in block.FatBlock.Subparts)
                            {
                                customInvMatrix = MatrixD.Invert(pair.Value.WorldMatrix);
                                t = pair.Value.ModelCollision.GetTrianglePruningStructure().GetIntersectionWithLine(this, ref line, ref customInvMatrix, flags);
                                if (t != 0)
                                {
                                    break;
                                }
                            }
                        }
                        if (t != 0)
                        {
                            MyIntersectionResultLineTriangleEx triangle = t.Value;
                            double num2 = Vector3D.Distance(Vector3D.Transform(t.Value.IntersectionPointInObjectSpace, block.FatBlock.WorldMatrix), line.From);
                            if (num2 < maxValue)
                            {
                                maxValue = num2;
                                MatrixD? cubeWorldMatrix = new MatrixD?(block.FatBlock.WorldMatrix);
                                this.TransformCubeToGrid(ref triangle, ref matrix, ref cubeWorldMatrix);
                                nullable = new MyIntersectionResultLineTriangleEx?(triangle);
                            }
                        }
                    }
                    t = nullable;
                }
                else
                {
                    cube.CubeBlock.FatBlock.GetIntersectionWithLine(ref line, out t, IntersectionFlags.ALL_TRIANGLES);
                    if (t != 0)
                    {
                        Matrix matrix2;
                        cube.CubeBlock.Orientation.GetMatrix(out matrix2);
                        MyIntersectionResultLineTriangleEx triangle = t.Value;
                        MatrixD? cubeWorldMatrix = new MatrixD?(cube.CubeBlock.FatBlock.WorldMatrix);
                        this.TransformCubeToGrid(ref triangle, ref matrix2, ref cubeWorldMatrix);
                        t = new MyIntersectionResultLineTriangleEx?(triangle);
                    }
                }
                cubePartIndex = -1;
            }
        }

        public HashSet<MySlimBlock> GetBlocks() => 
            this.m_cubeBlocks;

        public void GetBlocksInMultiBlock(int multiBlockId, HashSet<Tuple<MySlimBlock, ushort?>> outMultiBlocks)
        {
            if (multiBlockId != 0)
            {
                MyCubeGridMultiBlockInfo multiBlockInfo = this.GetMultiBlockInfo(multiBlockId);
                if (multiBlockInfo != null)
                {
                    foreach (MySlimBlock block in multiBlockInfo.Blocks)
                    {
                        MySlimBlock cubeBlock = this.GetCubeBlock(block.Position);
                        MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                        if (fatBlock != null)
                        {
                            outMultiBlocks.Add(new Tuple<MySlimBlock, ushort?>(cubeBlock, fatBlock.GetBlockId(block)));
                            continue;
                        }
                        ushort? nullable2 = null;
                        outMultiBlocks.Add(new Tuple<MySlimBlock, ushort?>(cubeBlock, nullable2));
                    }
                }
            }
        }

        public void GetBlocksInsideSphere(ref BoundingSphereD sphere, HashSet<MySlimBlock> blocks, bool checkTriangles = false)
        {
            blocks.Clear();
            if (base.PositionComp != null)
            {
                Vector3D vectord;
                BoundingBoxD aabb = BoundingBoxD.CreateFromSphere(sphere);
                MatrixD worldMatrixNormalizedInv = base.PositionComp.WorldMatrixNormalizedInv;
                Vector3D.Transform(ref sphere.Center, ref worldMatrixNormalizedInv, out vectord);
                BoundingSphere sphere2 = new BoundingSphere((Vector3) vectord, (float) sphere.Radius);
                BoundingBox box1 = BoundingBox.CreateFromSphere(sphere2);
                Vector3D min = box1.Min;
                Vector3D max = box1.Max;
                Vector3I vectori = new Vector3I((int) Math.Round((double) (max.X * this.GridSizeR)), (int) Math.Round((double) (max.Y * this.GridSizeR)), (int) Math.Round((double) (max.Z * this.GridSizeR)));
                Vector3I vectori1 = new Vector3I((int) Math.Round((double) (min.X * this.GridSizeR)), (int) Math.Round((double) (min.Y * this.GridSizeR)), (int) Math.Round((double) (min.Z * this.GridSizeR)));
                Vector3I start = Vector3I.Min(vectori1, vectori);
                Vector3I end = Vector3I.Max(vectori1, vectori);
                if ((end - start).Volume() < this.m_cubes.Count)
                {
                    Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref end);
                    Vector3I current = iterator.Current;
                    while (iterator.IsValid())
                    {
                        MyCube cube;
                        if (this.m_cubes.TryGetValue(current, out cube))
                        {
                            this.AddBlockInSphere(ref aabb, blocks, checkTriangles, ref sphere2, cube);
                        }
                        iterator.GetNext(out current);
                    }
                }
                else
                {
                    foreach (MyCube cube2 in this.m_cubes.Values)
                    {
                        this.AddBlockInSphere(ref aabb, blocks, checkTriangles, ref sphere2, cube2);
                    }
                }
            }
        }

        public unsafe void GetBlocksInsideSpheres(ref BoundingSphereD sphere1, ref BoundingSphereD sphere2, ref BoundingSphereD sphere3, HashSet<MySlimBlock> blocks1, HashSet<MySlimBlock> blocks2, HashSet<MySlimBlock> blocks3, bool respectDeformationRatio, float detectionBlockHalfSize, ref MatrixD invWorldGrid)
        {
            Vector3D vectord;
            blocks1.Clear();
            blocks2.Clear();
            blocks3.Clear();
            this.m_processedBlocks.Clear();
            Vector3D.Transform(ref sphere3.Center, ref invWorldGrid, out vectord);
            Vector3I vectori = Vector3I.Round((vectord - sphere3.Radius) * this.GridSizeR);
            Vector3I vectori2 = Vector3I.Round((vectord + sphere3.Radius) * this.GridSizeR);
            Vector3 vector = new Vector3(detectionBlockHalfSize);
            BoundingSphereD ed = new BoundingSphereD(vectord, sphere1.Radius);
            BoundingSphereD ed2 = new BoundingSphereD(vectord, sphere2.Radius);
            BoundingSphereD ed3 = new BoundingSphereD(vectord, sphere3.Radius);
            if ((((vectori2.X - vectori.X) * (vectori2.Y - vectori.Y)) * (vectori2.Z - vectori.Z)) >= this.m_cubes.Count)
            {
                using (IEnumerator<MyCube> enumerator = this.m_cubes.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MySlimBlock cubeBlock = enumerator.Current.CubeBlock;
                        if ((cubeBlock.FatBlock == null) || !this.m_processedBlocks.Contains(cubeBlock.FatBlock))
                        {
                            BoundingBox box2;
                            this.m_processedBlocks.Add(cubeBlock.FatBlock);
                            if (respectDeformationRatio)
                            {
                                ed.Radius = sphere1.Radius * cubeBlock.DeformationRatio;
                                ed2.Radius = sphere2.Radius * cubeBlock.DeformationRatio;
                                ed3.Radius = sphere3.Radius * cubeBlock.DeformationRatio;
                            }
                            if (cubeBlock.FatBlock != null)
                            {
                                box2 = new BoundingBox((Vector3) ((cubeBlock.Min * this.GridSize) - this.GridSizeHalf), (cubeBlock.Max * this.GridSize) + this.GridSizeHalf);
                            }
                            else
                            {
                                box2 = new BoundingBox(((Vector3) (cubeBlock.Position * this.GridSize)) - vector, (cubeBlock.Position * this.GridSize) + vector);
                            }
                            if (box2.Intersects((BoundingSphere) ed3))
                            {
                                if (!box2.Intersects((BoundingSphere) ed2))
                                {
                                    blocks3.Add(cubeBlock);
                                }
                                else if (box2.Intersects((BoundingSphere) ed))
                                {
                                    blocks1.Add(cubeBlock);
                                }
                                else
                                {
                                    blocks2.Add(cubeBlock);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Vector3I key = new Vector3I {
                    X = vectori.X
                };
                while (key.X <= vectori2.X)
                {
                    key.Y = vectori.Y;
                    while (true)
                    {
                        if (key.Y > vectori2.Y)
                        {
                            int* numPtr3 = (int*) ref key.X;
                            numPtr3[0]++;
                            break;
                        }
                        key.Z = vectori.Z;
                        while (true)
                        {
                            MyCube cube;
                            if (key.Z > vectori2.Z)
                            {
                                int* numPtr2 = (int*) ref key.Y;
                                numPtr2[0]++;
                                break;
                            }
                            if (this.m_cubes.TryGetValue(key, out cube))
                            {
                                MySlimBlock cubeBlock = cube.CubeBlock;
                                if ((cubeBlock.FatBlock == null) || !this.m_processedBlocks.Contains(cubeBlock.FatBlock))
                                {
                                    BoundingBox box;
                                    this.m_processedBlocks.Add(cubeBlock.FatBlock);
                                    if (respectDeformationRatio)
                                    {
                                        ed.Radius = sphere1.Radius * cubeBlock.DeformationRatio;
                                        ed2.Radius = sphere2.Radius * cubeBlock.DeformationRatio;
                                        ed3.Radius = sphere3.Radius * cubeBlock.DeformationRatio;
                                    }
                                    if (cubeBlock.FatBlock != null)
                                    {
                                        box = new BoundingBox((Vector3) ((cubeBlock.Min * this.GridSize) - this.GridSizeHalf), (cubeBlock.Max * this.GridSize) + this.GridSizeHalf);
                                    }
                                    else
                                    {
                                        box = new BoundingBox(((Vector3) (cubeBlock.Position * this.GridSize)) - vector, (cubeBlock.Position * this.GridSize) + vector);
                                    }
                                    if (box.Intersects((BoundingSphere) ed3))
                                    {
                                        if (!box.Intersects((BoundingSphere) ed2))
                                        {
                                            blocks3.Add(cubeBlock);
                                        }
                                        else if (box.Intersects((BoundingSphere) ed))
                                        {
                                            blocks1.Add(cubeBlock);
                                        }
                                        else
                                        {
                                            blocks2.Add(cubeBlock);
                                        }
                                    }
                                }
                            }
                            int* numPtr1 = (int*) ref key.Z;
                            numPtr1[0]++;
                        }
                    }
                }
            }
            this.m_processedBlocks.Clear();
        }

        public unsafe void GetBlocksIntersectingOBB(BoundingBoxD box, MatrixD boxTransform, List<MySlimBlock> blocks)
        {
            if ((blocks != null) && (base.PositionComp != null))
            {
                MyOrientedBoundingBoxD xd = MyOrientedBoundingBoxD.Create(box, boxTransform);
                BoundingBoxD worldAABB = base.PositionComp.WorldAABB;
                if (xd.Contains(ref worldAABB) == ContainmentType.Contains)
                {
                    foreach (MySlimBlock block in this.GetBlocks())
                    {
                        if ((block.FatBlock == null) || !block.FatBlock.Closed)
                        {
                            blocks.Add(block);
                        }
                    }
                }
                else
                {
                    MatrixD matrix = boxTransform * base.PositionComp.WorldMatrixNormalizedInv;
                    MyOrientedBoundingBoxD xd4 = MyOrientedBoundingBoxD.Create(box, matrix);
                    Vector3D* vectordPtr1 = (Vector3D*) ref xd4.Center;
                    vectordPtr1[0] *= this.GridSizeR;
                    Vector3D* vectordPtr2 = (Vector3D*) ref xd4.HalfExtent;
                    vectordPtr2[0] *= this.GridSizeR;
                    box = box.TransformFast(matrix);
                    Vector3D min = box.Min;
                    Vector3D max = box.Max;
                    Vector3I vectori = new Vector3I((int) Math.Round((double) (max.X * this.GridSizeR)), (int) Math.Round((double) (max.Y * this.GridSizeR)), (int) Math.Round((double) (max.Z * this.GridSizeR)));
                    Vector3I vectori1 = new Vector3I((int) Math.Round((double) (min.X * this.GridSizeR)), (int) Math.Round((double) (min.Y * this.GridSizeR)), (int) Math.Round((double) (min.Z * this.GridSizeR)));
                    Vector3I start = Vector3I.Max(Vector3I.Min(vectori1, vectori), this.Min);
                    Vector3I end = Vector3I.Min(Vector3I.Max(vectori1, vectori), this.Max);
                    if (((start.X <= end.X) && (start.Y <= end.Y)) && (start.Z <= end.Z))
                    {
                        Vector3 vector = new Vector3(0.5f);
                        BoundingBoxD xd5 = new BoundingBoxD();
                        if ((end - start).Size > this.m_cubeBlocks.Count)
                        {
                            foreach (MySlimBlock block2 in this.GetBlocks())
                            {
                                if ((block2.FatBlock == null) || !block2.FatBlock.Closed)
                                {
                                    xd5.Min = (Vector3D) (block2.Min - vector);
                                    xd5.Max = block2.Max + vector;
                                    if (xd4.Intersects(ref xd5))
                                    {
                                        blocks.Add(block2);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (m_tmpQuerySlimBlocks == null)
                            {
                                m_tmpQuerySlimBlocks = new HashSet<MySlimBlock>();
                            }
                            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref end);
                            Vector3I current = iterator.Current;
                            while (iterator.IsValid())
                            {
                                MyCube cube;
                                if (((this.m_cubes != null) && this.m_cubes.TryGetValue(current, out cube)) && (cube.CubeBlock != null))
                                {
                                    MySlimBlock cubeBlock = cube.CubeBlock;
                                    if (!m_tmpQuerySlimBlocks.Contains(cubeBlock))
                                    {
                                        xd5.Min = (Vector3D) (cubeBlock.Min - vector);
                                        xd5.Max = cubeBlock.Max + vector;
                                        if (xd4.Intersects(ref xd5))
                                        {
                                            m_tmpQuerySlimBlocks.Add(cubeBlock);
                                            blocks.Add(cubeBlock);
                                        }
                                    }
                                }
                                iterator.GetNext(out current);
                            }
                            m_tmpQuerySlimBlocks.Clear();
                        }
                    }
                }
            }
        }

        private static BoundingSphere GetBoundingSphereForGrids(MyObjectBuilder_CubeGrid[] currentPrefab)
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, float.MinValue);
            foreach (MyObjectBuilder_CubeGrid grid in currentPrefab)
            {
                MatrixD identity;
                BoundingSphere sphere2 = grid.CalculateBoundingSphere();
                if (grid.PositionAndOrientation == null)
                {
                    identity = MatrixD.Identity;
                }
                else
                {
                    identity = grid.PositionAndOrientation.Value.GetMatrix();
                }
                sphere.Include(sphere2.Transform((Matrix) identity));
            }
            return sphere;
        }

        public Vector3 GetClosestCorner(Vector3I gridPos, Vector3 position) => 
            (((Vector3) (gridPos * this.GridSize)) - (Vector3.SignNonZero(((Vector3) (gridPos * this.GridSize)) - position) * this.GridSizeHalf));

        public MySlimBlock GetCubeBlock(Vector3I pos)
        {
            MyCube cube;
            return (!this.m_cubes.TryGetValue(pos, out cube) ? null : cube.CubeBlock);
        }

        public MySlimBlock GetCubeBlock(Vector3I pos, ushort? compoundId)
        {
            MyCube cube;
            if (compoundId == null)
            {
                return this.GetCubeBlock(pos);
            }
            if (this.m_cubes.TryGetValue(pos, out cube))
            {
                MyCompoundCubeBlock fatBlock = cube.CubeBlock.FatBlock as MyCompoundCubeBlock;
                if (fatBlock != null)
                {
                    return fatBlock.GetBlock(compoundId.Value);
                }
            }
            return null;
        }

        private static MyCubePart[] GetCubeParts(MyStringHash skinSubtypeId, MyCubeBlockDefinition block, Vector3I position, MatrixD rotation, float gridSize, float gridScale)
        {
            List<string> outModels = new List<string>();
            List<MatrixD> outLocalMatrices = new List<MatrixD>();
            List<Vector4UByte> outPatternOffsets = new List<Vector4UByte>();
            GetCubeParts(block, position, (Matrix) rotation, gridSize, outModels, outLocalMatrices, new List<Vector3>(), outPatternOffsets, true);
            MyCubePart[] partArray = new MyCubePart[outModels.Count];
            for (int i = 0; i < partArray.Length; i++)
            {
                MyCubePart part = new MyCubePart();
                MyModel modelOnlyData = MyModels.GetModelOnlyData(outModels[i]);
                modelOnlyData.Rescale(gridScale);
                part.Init(modelOnlyData, skinSubtypeId, outLocalMatrices[i], gridScale);
                part.InstanceData.SetTextureOffset(outPatternOffsets[i]);
                partArray[i] = part;
            }
            return partArray;
        }

        public static unsafe void GetCubeParts(MyCubeBlockDefinition block, Vector3I inputPosition, Matrix rotation, float gridSize, List<string> outModels, List<MatrixD> outLocalMatrices, List<Vector3> outLocalNormals, List<Vector4UByte> outPatternOffsets, bool topologyCheck)
        {
            outModels.Clear();
            outLocalMatrices.Clear();
            outLocalNormals.Clear();
            outPatternOffsets.Clear();
            if (block.CubeDefinition != null)
            {
                if (topologyCheck)
                {
                    Base6Directions.Direction forward = Base6Directions.GetDirection(Vector3I.Round(rotation.Forward));
                    MyCubeGridDefinitions.GetTopologyUniqueOrientation(block.CubeDefinition.CubeTopology, new MyBlockOrientation(forward, Base6Directions.GetDirection(Vector3I.Round(rotation.Up)))).GetMatrix(out rotation);
                }
                MyTileDefinition[] cubeTiles = MyCubeGridDefinitions.GetCubeTiles(block);
                int length = cubeTiles.Length;
                int num2 = 0;
                int num3 = 0x8000;
                float epsilon = 0.01f;
                for (int i = 0; i < length; i++)
                {
                    MyTileDefinition definition1 = cubeTiles[num2 + i];
                    MatrixD item = definition1.LocalMatrix * rotation;
                    Vector3 vector = Vector3.Transform(definition1.Normal, rotation.GetOrientation());
                    Vector3I vectori = inputPosition;
                    if (block.CubeDefinition.CubeTopology == MyCubeTopology.Slope2Base)
                    {
                        Vector3I vectori4 = new Vector3I(Vector3.Sign(vector.MaxAbsComponent()));
                        vectori = (Vector3I) (vectori + vectori4);
                    }
                    string modelAsset = block.CubeDefinition.Model[i];
                    Vector2I vectori2 = block.CubeDefinition.PatternSize[i];
                    Vector2I vectori3 = block.CubeDefinition.ScaleTile[i];
                    int patternScale = (int) MyModels.GetModelOnlyData(modelAsset).PatternScale;
                    Vector2I* vectoriPtr1 = (Vector2I*) ref vectori2;
                    vectoriPtr1 = (Vector2I*) new Vector2I(vectori2.X * patternScale, vectori2.Y * patternScale);
                    int num7 = 0;
                    int num8 = 0;
                    float num9 = Vector3.Dot(Vector3.UnitY, vector);
                    float num10 = Vector3.Dot(Vector3.UnitX, vector);
                    float num11 = Vector3.Dot(Vector3.UnitZ, vector);
                    if (MyUtils.IsZero((float) (Math.Abs(num9) - 1f), epsilon))
                    {
                        int num12 = (vectori.X + num3) / vectori2.Y;
                        int num13 = MyMath.Mod((int) (num12 + ((int) (num12 * Math.Sin((double) (num12 * 10f))))), vectori2.X);
                        num7 = MyMath.Mod((int) (((vectori.Z + vectori.Y) + num13) + num3), vectori2.X);
                        num8 = MyMath.Mod((int) (vectori.X + num3), vectori2.Y);
                        if (Math.Sign(num9) == 1)
                        {
                            num8 = (vectori2.Y - 1) - num8;
                        }
                    }
                    else if (MyUtils.IsZero((float) (Math.Abs(num10) - 1f), epsilon))
                    {
                        int num14 = (vectori.Z + num3) / vectori2.Y;
                        int num15 = MyMath.Mod((int) (num14 + ((int) (num14 * Math.Sin((double) (num14 * 10f))))), vectori2.X);
                        num7 = MyMath.Mod((int) (((vectori.X + vectori.Y) + num15) + num3), vectori2.X);
                        num8 = MyMath.Mod((int) (vectori.Z + num3), vectori2.Y);
                        if (Math.Sign(num10) == 1)
                        {
                            num8 = (vectori2.Y - 1) - num8;
                        }
                    }
                    else if (MyUtils.IsZero((float) (Math.Abs(num11) - 1f), epsilon))
                    {
                        int num16 = (vectori.Y + num3) / vectori2.Y;
                        int num17 = MyMath.Mod((int) (num16 + ((int) (num16 * Math.Sin((double) (num16 * 10f))))), vectori2.X);
                        num7 = MyMath.Mod((int) ((vectori.X + num17) + num3), vectori2.X);
                        num8 = MyMath.Mod((int) (vectori.Y + num3), vectori2.Y);
                        if (Math.Sign(num11) == 1)
                        {
                            num7 = (vectori2.X - 1) - num7;
                        }
                    }
                    else if (MyUtils.IsZero(num10, epsilon))
                    {
                        num7 = MyMath.Mod((int) ((vectori.X * vectori3.X) + num3), vectori2.X);
                        num8 = MyMath.Mod((int) ((vectori.Z * vectori3.Y) + num3), vectori2.Y);
                        if (Math.Sign(num11) == -1)
                        {
                            if (Math.Sign(num9) != 1)
                            {
                                num8 = (vectori2.Y - 1) - num8;
                            }
                        }
                        else if (Math.Sign(num9) == -1)
                        {
                            num8 = (vectori2.Y - 1) - num8;
                        }
                    }
                    else if (MyUtils.IsZero(num11, epsilon))
                    {
                        num7 = MyMath.Mod((int) ((vectori.Z * vectori3.X) + num3), vectori2.X);
                        num8 = MyMath.Mod((int) ((vectori.Y * vectori3.Y) + num3), vectori2.Y);
                        if (Math.Sign(num10) == 1)
                        {
                            if (Math.Sign(num9) != 1)
                            {
                                num7 = (vectori2.X - 1) - num7;
                                num8 = (vectori2.Y - 1) - num8;
                            }
                        }
                        else if (Math.Sign(num9) == 1)
                        {
                            num7 = (vectori2.X - 1) - num7;
                        }
                        else
                        {
                            num8 = (vectori2.Y - 1) - num8;
                        }
                    }
                    else if (MyUtils.IsZero(num9, epsilon))
                    {
                        num7 = MyMath.Mod((int) ((vectori.Y * vectori3.X) + num3), vectori2.X);
                        num8 = MyMath.Mod((int) ((vectori.Z * vectori3.Y) + num3), vectori2.Y);
                        if (Math.Sign(num11) != -1)
                        {
                            if (Math.Sign(num10) == 1)
                            {
                                num7 = (vectori2.X - 1) - num7;
                            }
                        }
                        else if (Math.Sign(num10) == 1)
                        {
                            num8 = (vectori2.Y - 1) - num8;
                        }
                        else
                        {
                            num7 = (vectori2.X - 1) - num7;
                            num8 = (vectori2.Y - 1) - num8;
                        }
                    }
                    item.Translation = (Vector3D) (inputPosition * gridSize);
                    if (definition1.DontOffsetTexture)
                    {
                        num7 = 0;
                        num8 = 0;
                    }
                    outPatternOffsets.Add(new Vector4UByte((byte) num7, (byte) num8, (byte) vectori2.X, (byte) vectori2.Y));
                    outModels.Add(modelAsset);
                    outLocalMatrices.Add(item);
                    outLocalNormals.Add(vector);
                }
            }
        }

        public int GetCurrentMass()
        {
            float num;
            float num2;
            return (int) this.GetCurrentMass(out num, out num2);
        }

        public float GetCurrentMass(out float baseMass, out float physicalMass)
        {
            baseMass = 0f;
            physicalMass = 0f;
            float num = 0f;
            MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = MyCubeGridGroups.Static.Physical.GetGroup(this);
            if (group != null)
            {
                float blocksInventorySizeMultiplier = MySession.Static.Settings.BlocksInventorySizeMultiplier;
                using (HashSet<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node>.Enumerator enumerator = group.Nodes.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyCubeGrid nodeData = enumerator.Current.NodeData;
                        if ((nodeData != null) && ((nodeData.Physics != null) && (nodeData.Physics.Shape != null)))
                        {
                            HkMassProperties? massProperties = nodeData.Physics.Shape.MassProperties;
                            HkMassProperties? baseMassProperties = nodeData.Physics.Shape.BaseMassProperties;
                            if (!this.IsStatic && ((massProperties != null) && (baseMassProperties != null)))
                            {
                                float mass = massProperties.Value.Mass;
                                float num4 = baseMassProperties.Value.Mass;
                                using (HashSet<MyCockpit>.Enumerator enumerator2 = nodeData.OccupiedBlocks.GetEnumerator())
                                {
                                    while (enumerator2.MoveNext())
                                    {
                                        MyCharacter pilot = enumerator2.Current.Pilot;
                                        if (pilot != null)
                                        {
                                            float num6 = pilot.BaseMass;
                                            num4 += num6;
                                            mass += (pilot.CurrentMass - num6) / blocksInventorySizeMultiplier;
                                        }
                                    }
                                }
                                baseMass += num4;
                                num += num4 + ((mass - num4) * blocksInventorySizeMultiplier);
                                if ((nodeData.Physics.WeldInfo.Parent == null) || ReferenceEquals(nodeData.Physics.WeldInfo.Parent, nodeData.Physics))
                                {
                                    physicalMass += nodeData.Physics.Mass;
                                }
                            }
                        }
                    }
                }
            }
            return num;
        }

        private Action<MyCubeGrid> GetDelegate(Action<VRage.Game.ModAPI.IMyCubeGrid> value) => 
            ((Action<MyCubeGrid>) Delegate.CreateDelegate(typeof(Action<MyCubeGrid>), value.Target, value.Method));

        private Action<MySlimBlock> GetDelegate(Action<VRage.Game.ModAPI.IMySlimBlock> value) => 
            ((Action<MySlimBlock>) Delegate.CreateDelegate(typeof(Action<MySlimBlock>), value.Target, value.Method));

        private Action<MyCubeGrid, bool> GetDelegate(Action<VRage.Game.ModAPI.IMyCubeGrid, bool> value) => 
            ((Action<MyCubeGrid, bool>) Delegate.CreateDelegate(typeof(Action<MyCubeGrid, bool>), value.Target, value.Method));

        private Action<MyCubeGrid, MyCubeGrid> GetDelegate(Action<VRage.Game.ModAPI.IMyCubeGrid, VRage.Game.ModAPI.IMyCubeGrid> value) => 
            ((Action<MyCubeGrid, MyCubeGrid>) Delegate.CreateDelegate(typeof(Action<MyCubeGrid, MyCubeGrid>), value.Target, value.Method));

        public MySlimBlock GetExistingCubeForBoneDeformations(ref Vector3I cube, ref MyDamageInformation damageInfo)
        {
            MyCube cube2;
            if (this.m_cubes.TryGetValue(cube, out cube2))
            {
                MySlimBlock cubeBlock = cube2.CubeBlock;
                if (cubeBlock.UsesDeformation)
                {
                    if (cubeBlock.UseDamageSystem)
                    {
                        damageInfo.Amount = 1f;
                        MyDamageSystem.Static.RaiseBeforeDamageApplied(cubeBlock, ref damageInfo);
                        if (damageInfo.Amount == 0f)
                        {
                            return null;
                        }
                    }
                    return cubeBlock;
                }
            }
            return null;
        }

        private void GetExistingCubes(Vector3I cubePos, IEnumerable<Vector3I> offsets, Dictionary<Vector3I, MySlimBlock> resultSet)
        {
            resultSet.Clear();
            foreach (Vector3I vectori2 in offsets)
            {
                MyCube cube;
                Vector3I key = (Vector3I) (cubePos + vectori2);
                if (this.m_cubes.TryGetValue(key, out cube) && (!cube.CubeBlock.IsDestroyed && cube.CubeBlock.UsesDeformation))
                {
                    resultSet[vectori2] = cube.CubeBlock;
                }
            }
        }

        public unsafe void GetExistingCubes(Vector3I boneMin, Vector3I boneMax, Dictionary<Vector3I, MySlimBlock> resultSet, MyDamageInformation? damageInfo = new MyDamageInformation?())
        {
            Vector3I vectori3;
            MyDamageInformation local1;
            resultSet.Clear();
            Vector3I result = Vector3I.Floor((boneMin - Vector3I.One) / 2f);
            Vector3I vectori2 = Vector3I.Ceiling((boneMax - Vector3I.One) / 2f);
            if (damageInfo != null)
            {
                local1 = damageInfo.Value;
            }
            else
            {
                local1 = new MyDamageInformation();
            }
            MyDamageInformation info = local1;
            Vector3I* vectoriPtr1 = (Vector3I*) ref result;
            Vector3I.Max(ref (Vector3I) ref vectoriPtr1, ref this.m_min, out result);
            Vector3I* vectoriPtr2 = (Vector3I*) ref vectori2;
            Vector3I.Min(ref (Vector3I) ref vectoriPtr2, ref this.m_max, out vectori2);
            vectori3.X = result.X;
            goto TR_0012;
        TR_0003:
            int* numPtr1 = (int*) ref vectori3.Z;
            numPtr1[0]++;
        TR_000C:
            while (true)
            {
                MyCube cube;
                if (vectori3.Z > vectori2.Z)
                {
                    int* numPtr2 = (int*) ref vectori3.Y;
                    numPtr2[0]++;
                    break;
                }
                if (this.m_cubes.TryGetValue(vectori3, out cube) && cube.CubeBlock.UsesDeformation)
                {
                    if (cube.CubeBlock.UseDamageSystem && (damageInfo != null))
                    {
                        info.Amount = 1f;
                        MyDamageSystem.Static.RaiseBeforeDamageApplied(cube.CubeBlock, ref info);
                        if (info.Amount == 0f)
                        {
                            goto TR_0003;
                        }
                    }
                    resultSet[vectori3] = cube.CubeBlock;
                }
                goto TR_0003;
            }
        TR_000F:
            while (true)
            {
                if (vectori3.Y > vectori2.Y)
                {
                    int* numPtr3 = (int*) ref vectori3.X;
                    numPtr3[0]++;
                    break;
                }
                vectori3.Z = result.Z;
                goto TR_000C;
            }
        TR_0012:
            while (true)
            {
                if (vectori3.X > vectori2.X)
                {
                    return;
                }
                vectori3.Y = result.Y;
                break;
            }
            goto TR_000F;
        }

        public int GetFatBlockCount<T>() where T: MyCubeBlock
        {
            int num = 0;
            using (List<MyCubeBlock>.Enumerator enumerator = this.GetFatBlocks().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!(enumerator.Current is T))
                    {
                        continue;
                    }
                    num++;
                }
            }
            return num;
        }

        public MyFatBlockReader<T> GetFatBlocks<T>() where T: MyCubeBlock => 
            new MyFatBlockReader<T>(this);

        public ListReader<MyCubeBlock> GetFatBlocks() => 
            this.m_fatBlocks.ListUnsafe;

        public T GetFirstBlockOfType<T>() where T: MyCubeBlock
        {
            using (HashSet<MySlimBlock>.Enumerator enumerator = this.m_cubeBlocks.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MySlimBlock current = enumerator.Current;
                    if ((current.FatBlock != null) && (current.FatBlock is T))
                    {
                        return (current.FatBlock as T);
                    }
                }
            }
            return default(T);
        }

        public void GetGeneratedBlocks(MySlimBlock generatingBlock, List<MySlimBlock> outGeneratedBlocks)
        {
            Vector3I[] tmpBlockSurroundingOffsets;
            int num;
            outGeneratedBlocks.Clear();
            if (generatingBlock == null)
            {
                return;
            }
            else if (!(generatingBlock.FatBlock is MyCompoundCubeBlock))
            {
                if (generatingBlock.BlockDefinition.IsGeneratedBlock)
                {
                    return;
                }
                else if ((generatingBlock.BlockDefinition.GeneratedBlockDefinitions != null) && (generatingBlock.BlockDefinition.GeneratedBlockDefinitions.Length != 0))
                {
                    tmpBlockSurroundingOffsets = m_tmpBlockSurroundingOffsets;
                    num = 0;
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
            while (true)
            {
                while (true)
                {
                    if (num >= tmpBlockSurroundingOffsets.Length)
                    {
                        return;
                    }
                    Vector3I vectori = tmpBlockSurroundingOffsets[num];
                    MySlimBlock cubeBlock = generatingBlock.CubeGrid.GetCubeBlock((Vector3I) (generatingBlock.Position + vectori));
                    if ((cubeBlock != null) && !ReferenceEquals(cubeBlock, generatingBlock))
                    {
                        List<IMyBlockAdditionalModelGenerator>.Enumerator enumerator2;
                        if (cubeBlock.FatBlock is MyCompoundCubeBlock)
                        {
                            foreach (MySlimBlock block2 in (cubeBlock.FatBlock as MyCompoundCubeBlock).GetBlocks())
                            {
                                if (ReferenceEquals(block2, generatingBlock))
                                {
                                    continue;
                                }
                                if (block2.BlockDefinition.IsGeneratedBlock)
                                {
                                    using (enumerator2 = this.AdditionalModelGenerators.GetEnumerator())
                                    {
                                        while (enumerator2.MoveNext())
                                        {
                                            MySlimBlock objB = enumerator2.Current.GetGeneratingBlock(block2);
                                            if (ReferenceEquals(generatingBlock, objB))
                                            {
                                                outGeneratedBlocks.Add(block2);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                        if (cubeBlock.BlockDefinition.IsGeneratedBlock)
                        {
                            using (enumerator2 = this.AdditionalModelGenerators.GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    MySlimBlock objB = enumerator2.Current.GetGeneratingBlock(cubeBlock);
                                    if (ReferenceEquals(generatingBlock, objB))
                                    {
                                        outGeneratedBlocks.Add(cubeBlock);
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
                num++;
            }
        }

        public MySlimBlock GetGeneratingBlock(MySlimBlock generatedBlock)
        {
            if ((generatedBlock != null) && generatedBlock.BlockDefinition.IsGeneratedBlock)
            {
                using (List<IMyBlockAdditionalModelGenerator>.Enumerator enumerator = this.AdditionalModelGenerators.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MySlimBlock generatingBlock = enumerator.Current.GetGeneratingBlock(generatedBlock);
                        if (generatingBlock != null)
                        {
                            return generatingBlock;
                        }
                    }
                }
            }
            return null;
        }

        public bool GetIntersectionWithLine(ref LineD line, ref MyCubeGridHitInfo info, IntersectionFlags flags = 3)
        {
            if (info == null)
            {
                info = new MyCubeGridHitInfo();
            }
            info.Reset();
            if (!base.IsPreview)
            {
                if (this.Projector != null)
                {
                    return false;
                }
                Vector3I? gridSizeInflate = null;
                this.RayCastCells(line.From, line.To, m_cacheRayCastCells, gridSizeInflate, false, true);
                if (m_cacheRayCastCells.Count == 0)
                {
                    return false;
                }
                using (List<Vector3I>.Enumerator enumerator = m_cacheRayCastCells.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        Vector3I current = enumerator.Current;
                        if (this.m_cubes.ContainsKey(current))
                        {
                            MyIntersectionResultLineTriangleEx? nullable2;
                            int num;
                            MyCube cube = this.m_cubes[current];
                            this.GetBlockIntersection(cube, ref line, flags, out nullable2, out num);
                            if (nullable2 != null)
                            {
                                info.Position = cube.CubeBlock.Position;
                                info.Triangle = nullable2.Value;
                                info.CubePartIndex = num;
                                info.Triangle.UserObject = cube;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public override bool GetIntersectionWithLine(ref LineD line, out MyIntersectionResultLineTriangleEx? tri, IntersectionFlags flags = 3)
        {
            bool flag1 = this.GetIntersectionWithLine(ref line, ref m_hitInfoTmp, flags);
            if (flag1)
            {
                tri = new MyIntersectionResultLineTriangleEx?(m_hitInfoTmp.Triangle);
                return flag1;
            }
            tri = 0;
            return flag1;
        }

        internal bool GetIntersectionWithLine(ref LineD line, out MyIntersectionResultLineTriangleEx? t, out MySlimBlock slimBlock, IntersectionFlags flags = 3)
        {
            t = 0;
            slimBlock = null;
            Vector3I? gridSizeInflate = null;
            this.RayCastCells(line.From, line.To, m_cacheRayCastCells, gridSizeInflate, false, true);
            if (m_cacheRayCastCells.Count == 0)
            {
                return false;
            }
            foreach (Vector3I vectori in m_cacheRayCastCells)
            {
                if (this.m_cubes.ContainsKey(vectori))
                {
                    int num;
                    MyCube cube = this.m_cubes[vectori];
                    this.GetBlockIntersection(cube, ref line, flags, out t, out num);
                    if (t != 0)
                    {
                        slimBlock = cube.CubeBlock;
                        break;
                    }
                }
            }
            if ((slimBlock != null) && (slimBlock.FatBlock is MyCompoundCubeBlock))
            {
                ListReader<MySlimBlock> blocks = (slimBlock.FatBlock as MyCompoundCubeBlock).GetBlocks();
                double maxValue = double.MaxValue;
                MySlimBlock block = null;
                int index = 0;
                while (true)
                {
                    MyIntersectionResultLineTriangleEx? nullable2;
                    if (index >= blocks.Count)
                    {
                        slimBlock = block;
                        break;
                    }
                    MySlimBlock block2 = blocks.ItemAt(index);
                    if (block2.FatBlock.GetIntersectionWithLine(ref line, out nullable2, IntersectionFlags.ALL_TRIANGLES) && (nullable2 != null))
                    {
                        double num4 = (nullable2.Value.IntersectionPointInWorldSpace - line.From).LengthSquared();
                        if (num4 < maxValue)
                        {
                            maxValue = num4;
                            block = block2;
                        }
                    }
                    index++;
                }
            }
            return (t != 0);
        }

        public override bool GetIntersectionWithSphere(ref BoundingSphereD sphere)
        {
            bool flag;
            try
            {
                int y;
                int z;
                BoundingBoxD xd = new BoundingBoxD(sphere.Center - new Vector3D(sphere.Radius), sphere.Center + new Vector3D(sphere.Radius));
                MatrixD m = MatrixD.Invert(base.WorldMatrix);
                xd = xd.TransformFast(ref m);
                Vector3 min = (Vector3) xd.Min;
                Vector3 max = (Vector3) xd.Max;
                Vector3I vectori = new Vector3I((int) Math.Round((double) (max.X * this.GridSizeR)), (int) Math.Round((double) (max.Y * this.GridSizeR)), (int) Math.Round((double) (max.Z * this.GridSizeR)));
                Vector3I vectori1 = new Vector3I((int) Math.Round((double) (min.X * this.GridSizeR)), (int) Math.Round((double) (min.Y * this.GridSizeR)), (int) Math.Round((double) (min.Z * this.GridSizeR)));
                Vector3I vectori2 = Vector3I.Min(vectori1, vectori);
                Vector3I vectori3 = Vector3I.Max(vectori1, vectori);
                int x = vectori2.X;
                goto TR_001C;
            TR_0005:
                z++;
            TR_0016:
                while (true)
                {
                    if (z <= vectori3.Z)
                    {
                        if (this.m_cubes.ContainsKey(new Vector3I(x, y, z)))
                        {
                            MyCube cube = this.m_cubes[new Vector3I(x, y, z)];
                            if ((cube.CubeBlock.FatBlock != null) && (cube.CubeBlock.FatBlock.Model != null))
                            {
                                MatrixD matrix = Matrix.Invert((Matrix) cube.CubeBlock.FatBlock.WorldMatrix);
                                new BoundingSphere((Vector3) Vector3D.Transform(sphere.Center, matrix), (float) sphere.Radius);
                                bool intersectionWithSphere = cube.CubeBlock.FatBlock.Model.GetTrianglePruningStructure().GetIntersectionWithSphere(cube.CubeBlock.FatBlock, ref sphere);
                                if (intersectionWithSphere)
                                {
                                    flag = intersectionWithSphere;
                                    break;
                                }
                            }
                            else
                            {
                                if (cube.CubeBlock.BlockDefinition.CubeDefinition.CubeTopology != MyCubeTopology.Box)
                                {
                                    MyCubePart[] parts = cube.Parts;
                                    int index = 0;
                                    while (true)
                                    {
                                        if (index < parts.Length)
                                        {
                                            MyCubePart part1 = parts[index];
                                            MatrixD matrix = Matrix.Invert(part1.InstanceData.LocalMatrix * base.WorldMatrix);
                                            Vector3D vectord = Vector3D.Transform(sphere.Center, matrix);
                                            BoundingSphere localSphere = new BoundingSphere((Vector3) vectord, (float) sphere.Radius);
                                            if (!part1.Model.GetTrianglePruningStructure().GetIntersectionWithSphere(ref localSphere))
                                            {
                                                index++;
                                                continue;
                                            }
                                            flag = true;
                                        }
                                        else
                                        {
                                            goto TR_0005;
                                        }
                                        break;
                                    }
                                }
                                else
                                {
                                    flag = true;
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        y++;
                        goto TR_0019;
                    }
                    goto TR_0005;
                }
                return flag;
            TR_0019:
                while (true)
                {
                    if (y > vectori3.Y)
                    {
                        x++;
                        break;
                    }
                    z = vectori2.Z;
                    goto TR_0016;
                }
            TR_001C:
                while (true)
                {
                    if (x > vectori3.X)
                    {
                        flag = false;
                        break;
                    }
                    y = vectori2.Y;
                    goto TR_0019;
                }
            }
            finally
            {
            }
            return flag;
        }

        public static bool GetLineIntersection(ref LineD line, out MyCubeGrid grid, out Vector3I position, out double distanceSquared)
        {
            grid = null;
            position = new Vector3I();
            distanceSquared = 3.4028234663852886E+38;
            Sandbox.Game.Entities.MyEntities.OverlapAllLineSegment(ref line, m_lineOverlapList);
            using (List<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>>.Enumerator enumerator = m_lineOverlapList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyCubeGrid element = enumerator.Current.Element as MyCubeGrid;
                    if (element != null)
                    {
                        Vector3I? nullable = element.RayCastBlocks(line.From, line.To);
                        if (nullable != null)
                        {
                            Vector3 closestCorner = element.GetClosestCorner(nullable.Value, (Vector3) line.From);
                            float num = (float) Vector3D.DistanceSquared(line.From, Vector3D.Transform(closestCorner, element.WorldMatrix));
                            if (num < distanceSquared)
                            {
                                distanceSquared = num;
                                grid = element;
                                position = nullable.Value;
                            }
                        }
                    }
                }
            }
            m_lineOverlapList.Clear();
            return (grid != null);
        }

        public static bool GetLineIntersectionExact(ref LineD line, out MyCubeGrid grid, out Vector3I position, out double distanceSquared)
        {
            grid = null;
            position = new Vector3I();
            distanceSquared = 3.4028234663852886E+38;
            double maxValue = double.MaxValue;
            Sandbox.Game.Entities.MyEntities.OverlapAllLineSegment(ref line, m_lineOverlapList);
            using (List<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>>.Enumerator enumerator = m_lineOverlapList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MySlimBlock block;
                    double num2;
                    MyCubeGrid element = enumerator.Current.Element as MyCubeGrid;
                    if ((element != null) && ((element.GetLineIntersectionExactAll(ref line, out num2, out block) != null) && (num2 < maxValue)))
                    {
                        grid = element;
                        maxValue = num2;
                    }
                }
            }
            m_lineOverlapList.Clear();
            return (grid != null);
        }

        public Vector3D? GetLineIntersectionExactAll(ref LineD line, out double distance, out MySlimBlock intersectedBlock)
        {
            intersectedBlock = null;
            distance = 3.4028234663852886E+38;
            Vector3I? nullable = null;
            Vector3I zero = Vector3I.Zero;
            double maxValue = double.MaxValue;
            if (this.GetLineIntersectionExactGrid(ref line, ref zero, ref maxValue))
            {
                maxValue = (float) Math.Sqrt(maxValue);
                nullable = new Vector3I?(zero);
            }
            if (nullable != null)
            {
                distance = maxValue;
                intersectedBlock = this.GetCubeBlock(nullable.Value);
                if (intersectedBlock != null)
                {
                    return new Vector3D?((Vector3D) zero);
                }
            }
            return null;
        }

        public bool GetLineIntersectionExactGrid(ref LineD line, ref Vector3I position, ref double distanceSquared)
        {
            MyPhysics.HitInfo? hitInfo = null;
            return this.GetLineIntersectionExactGrid(ref line, ref position, ref distanceSquared, hitInfo);
        }

        public unsafe bool GetLineIntersectionExactGrid(ref LineD line, ref Vector3I position, ref double distanceSquared, MyPhysics.HitInfo? hitInfo = new MyPhysics.HitInfo?())
        {
            Vector3I vectori;
            MyCube cube;
            double maxValue;
            MyIntersectionResultLineTriangleEx? nullable2;
            int num5;
            Vector3I? gridSizeInflate = null;
            this.RayCastCells(line.From, line.To, m_cacheRayCastCells, gridSizeInflate, true, true);
            if (m_cacheRayCastCells.Count == 0)
            {
                return false;
            }
            m_tmpHitList.Clear();
            if (hitInfo != null)
            {
                m_tmpHitList.Add(hitInfo.Value);
            }
            else
            {
                MyPhysics.CastRay(line.From, line.To, m_tmpHitList, 0x18);
            }
            if (m_tmpHitList.Count == 0)
            {
                return false;
            }
            bool flag = false;
            int num = 0;
            goto TR_003E;
        TR_0002:
            m_tmpHitList.Clear();
            return flag;
        TR_0024:
            num++;
            goto TR_003E;
        TR_0027:
            if (maxValue < distanceSquared)
            {
                distanceSquared = maxValue;
                position = vectori;
                flag = true;
            }
            goto TR_0024;
        TR_0037:
            this.GetBlockIntersection(cube, ref line, IntersectionFlags.ALL_TRIANGLES, out nullable2, out num5);
            if (nullable2 != null)
            {
                maxValue = Vector3.DistanceSquared((Vector3) line.From, (Vector3) nullable2.Value.IntersectionPointInWorldSpace);
            }
            goto TR_0027;
        TR_003E:
            while (true)
            {
                if (num < m_cacheRayCastCells.Count)
                {
                    vectori = m_cacheRayCastCells[num];
                    this.m_cubes.TryGetValue(vectori, out cube);
                    maxValue = double.MaxValue;
                    if (cube == null)
                    {
                        break;
                    }
                    if (cube.CubeBlock.FatBlock == null)
                    {
                        goto TR_0037;
                    }
                    else if (cube.CubeBlock.FatBlock.BlockDefinition.UseModelIntersection)
                    {
                        goto TR_0037;
                    }
                    else if (m_tmpHitList.Count > 0)
                    {
                        int num3 = 0;
                        if (MySession.Static.ControlledEntity != null)
                        {
                            while ((num3 < (m_tmpHitList.Count - 1)) && ReferenceEquals(m_tmpHitList[num3].HkHitInfo.GetHitEntity(), MySession.Static.ControlledEntity.Entity))
                            {
                                num3++;
                            }
                        }
                        if ((num3 > 1) && !ReferenceEquals(m_tmpHitList[num3].HkHitInfo.GetHitEntity(), this))
                        {
                            goto TR_0024;
                        }
                        Vector3 gridSizeHalfVector = this.GridSizeHalfVector;
                        Vector3D vectord = Vector3D.Transform(m_tmpHitList[num3].Position, base.PositionComp.WorldMatrixInvScaled);
                        Vector3 vector2 = (Vector3) (vectori * this.GridSize);
                        Vector3D vectord2 = vectord - vector2;
                        double num4 = (vectord2.Max() > Math.Abs(vectord2.Min())) ? vectord2.Max() : vectord2.Min();
                        Vector3D* vectordPtr1 = (Vector3D*) ref vectord2;
                        vectordPtr1->X = (vectord2.X == num4) ? ((num4 > 0.0) ? ((double) 1) : ((double) (-1))) : ((double) 0);
                        Vector3D* vectordPtr2 = (Vector3D*) ref vectord2;
                        vectordPtr2->Y = (vectord2.Y == num4) ? ((num4 > 0.0) ? ((double) 1) : ((double) (-1))) : ((double) 0);
                        Vector3D* vectordPtr3 = (Vector3D*) ref vectord2;
                        vectordPtr3->Z = (vectord2.Z == num4) ? ((num4 > 0.0) ? ((double) 1) : ((double) (-1))) : ((double) 0);
                        vectord -= vectord2 * 0.059999998658895493;
                        if ((Vector3D.Max(vectord, vector2 - gridSizeHalfVector) == vectord) && (Vector3D.Min(vectord, vector2 + gridSizeHalfVector) == vectord))
                        {
                            maxValue = Vector3D.DistanceSquared(line.From, m_tmpHitList[num3].Position);
                            if (maxValue < distanceSquared)
                            {
                                position = vectori;
                                distanceSquared = maxValue;
                                flag = true;
                                goto TR_0024;
                            }
                        }
                    }
                }
                else
                {
                    int num6;
                    if (flag)
                    {
                        goto TR_0002;
                    }
                    else
                    {
                        num6 = 0;
                    }
                    while (true)
                    {
                        while (true)
                        {
                            if (num6 < m_cacheRayCastCells.Count)
                            {
                                MyCube cube2;
                                Vector3I key = m_cacheRayCastCells[num6];
                                this.m_cubes.TryGetValue(key, out cube2);
                                double maxValue = double.MaxValue;
                                if (((cube2 != null) && (cube2.CubeBlock.FatBlock != null)) && cube2.CubeBlock.FatBlock.BlockDefinition.UseModelIntersection)
                                {
                                    MyIntersectionResultLineTriangleEx? nullable3;
                                    int num10;
                                    this.GetBlockIntersection(cube2, ref line, IntersectionFlags.ALL_TRIANGLES, out nullable3, out num10);
                                    if (nullable3 != null)
                                    {
                                        maxValue = Vector3.DistanceSquared((Vector3) line.From, (Vector3) nullable3.Value.IntersectionPointInWorldSpace);
                                    }
                                }
                                else if (m_tmpHitList.Count > 0)
                                {
                                    int num8 = 0;
                                    if (MySession.Static.ControlledEntity != null)
                                    {
                                        while ((num8 < (m_tmpHitList.Count - 1)) && ReferenceEquals(m_tmpHitList[num8].HkHitInfo.GetHitEntity(), MySession.Static.ControlledEntity.Entity))
                                        {
                                            num8++;
                                        }
                                    }
                                    if ((num8 > 1) && !ReferenceEquals(m_tmpHitList[num8].HkHitInfo.GetHitEntity(), this))
                                    {
                                        break;
                                    }
                                    Vector3 gridSizeHalfVector = this.GridSizeHalfVector;
                                    Vector3D vectord3 = Vector3D.Transform(m_tmpHitList[num8].Position, base.PositionComp.WorldMatrixInvScaled);
                                    Vector3 vector4 = (Vector3) (key * this.GridSize);
                                    Vector3D vectord4 = vectord3 - vector4;
                                    double num9 = (vectord4.Max() > Math.Abs(vectord4.Min())) ? vectord4.Max() : vectord4.Min();
                                    Vector3D* vectordPtr4 = (Vector3D*) ref vectord4;
                                    vectordPtr4->X = (vectord4.X == num9) ? ((num9 > 0.0) ? ((double) 1) : ((double) (-1))) : ((double) 0);
                                    Vector3D* vectordPtr5 = (Vector3D*) ref vectord4;
                                    vectordPtr5->Y = (vectord4.Y == num9) ? ((num9 > 0.0) ? ((double) 1) : ((double) (-1))) : ((double) 0);
                                    Vector3D* vectordPtr6 = (Vector3D*) ref vectord4;
                                    vectordPtr6->Z = (vectord4.Z == num9) ? ((num9 > 0.0) ? ((double) 1) : ((double) (-1))) : ((double) 0);
                                    vectord3 -= vectord4 * 0.059999998658895493;
                                    if ((Vector3D.Max(vectord3, vector4 - gridSizeHalfVector) == vectord3) && (Vector3D.Min(vectord3, vector4 + gridSizeHalfVector) == vectord3))
                                    {
                                        if (cube2 == null)
                                        {
                                            Vector3I vectori3;
                                            this.FixTargetCube(out vectori3, (Vector3) (vectord3 * this.GridSizeR));
                                            if (!this.m_cubes.TryGetValue(vectori3, out cube2))
                                            {
                                                break;
                                            }
                                            key = vectori3;
                                        }
                                        maxValue = Vector3D.DistanceSquared(line.From, m_tmpHitList[num8].Position);
                                        if (maxValue < distanceSquared)
                                        {
                                            position = key;
                                            distanceSquared = maxValue;
                                            flag = true;
                                            break;
                                        }
                                    }
                                }
                                if (maxValue < distanceSquared)
                                {
                                    distanceSquared = maxValue;
                                    position = key;
                                    flag = true;
                                }
                            }
                            else
                            {
                                goto TR_0002;
                            }
                            break;
                        }
                        num6++;
                    }
                }
                break;
            }
            goto TR_0027;
        }

        protected static float GetLineWidthForGizmo(IMyGizmoDrawableObject block, BoundingBox box)
        {
            float num = 100f;
            foreach (Vector3 vector2 in box.Corners)
            {
                num = (float) Math.Min((double) num, Math.Abs(MySector.MainCamera.GetDistanceFromPoint(Vector3.Transform(block.GetPositionInGrid() + vector2, block.GetWorldMatrix()))));
            }
            Vector3 vector = box.Max - box.Min;
            float num2 = MathHelper.Max(1f, MathHelper.Min(MathHelper.Min(vector.X, vector.Y), vector.Z));
            return ((num * 0.002f) / num2);
        }

        private bool GetMissingBlocksMultiBlock(int multiblockId, out MyCubeGridMultiBlockInfo multiBlockInfo, out MatrixI transform, List<int> multiBlockIndices)
        {
            transform = new MatrixI();
            multiBlockInfo = this.GetMultiBlockInfo(multiblockId);
            return ((multiBlockInfo != null) ? multiBlockInfo.GetMissingBlocks(out transform, multiBlockIndices) : false);
        }

        private static void GetModelDataFromGrid(List<MyCubeGrid> baseGrid, List<Vector3> vertices, List<TriangleWithMaterial> triangles, List<Vector2> uvs, List<MyExportModel.Material> materials, int currVerticesCount)
        {
            MatrixD xd = MatrixD.Invert(baseGrid[0].WorldMatrix);
            foreach (MyCubeGrid grid in baseGrid)
            {
                MatrixD xd2 = grid.WorldMatrix * xd;
                foreach (KeyValuePair<Vector3I, MyCubeGridRenderCell> pair in grid.RenderData.Cells)
                {
                    IEnumerator<KeyValuePair<MyCubePart, ConcurrentDictionary<uint, bool>>> enumerator = pair.Value.CubeParts.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MyCubePart, ConcurrentDictionary<uint, bool>> current = enumerator.Current;
                            MyCubePart key = current.Key;
                            Vector3 colorMaskHSV = new Vector3(key.InstanceData.ColorMaskHSV.X, key.InstanceData.ColorMaskHSV.Y, key.InstanceData.ColorMaskHSV.Z);
                            Vector2 offsetUV = new Vector2(key.InstanceData.GetTextureOffset(0), key.InstanceData.GetTextureOffset(1));
                            ExtractModelDataForObj(key.Model, key.InstanceData.LocalMatrix * xd2, vertices, triangles, uvs, ref offsetUV, materials, ref currVerticesCount, colorMaskHSV);
                        }
                    }
                    finally
                    {
                        if (enumerator == null)
                        {
                            continue;
                        }
                        enumerator.Dispose();
                    }
                }
                foreach (MySlimBlock block in grid.GetBlocks())
                {
                    if (block.FatBlock != null)
                    {
                        if (block.FatBlock is MyPistonBase)
                        {
                            block.FatBlock.UpdateOnceBeforeFrame();
                        }
                        else if (block.FatBlock is MyCompoundCubeBlock)
                        {
                            foreach (MySlimBlock block2 in (block.FatBlock as MyCompoundCubeBlock).GetBlocks())
                            {
                                ExtractModelDataForObj(block2.FatBlock.Model, (Matrix) (block2.FatBlock.PositionComp.WorldMatrix * xd), vertices, triangles, uvs, ref Vector2.Zero, materials, ref currVerticesCount, block2.ColorMaskHSV);
                                ProcessChildrens(vertices, triangles, uvs, materials, ref currVerticesCount, (Matrix) (block2.FatBlock.PositionComp.WorldMatrix * xd), block2.ColorMaskHSV, block2.FatBlock.Hierarchy.Children);
                            }
                            continue;
                        }
                        ExtractModelDataForObj(block.FatBlock.Model, (Matrix) (block.FatBlock.PositionComp.WorldMatrix * xd), vertices, triangles, uvs, ref Vector2.Zero, materials, ref currVerticesCount, block.ColorMaskHSV);
                        ProcessChildrens(vertices, triangles, uvs, materials, ref currVerticesCount, (Matrix) (block.FatBlock.PositionComp.WorldMatrix * xd), block.ColorMaskHSV, block.FatBlock.Hierarchy.Children);
                    }
                }
            }
        }

        public MyCubeGridMultiBlockInfo GetMultiBlockInfo(int multiBlockId)
        {
            MyCubeGridMultiBlockInfo info;
            if ((this.m_multiBlockInfos == null) || !this.m_multiBlockInfos.TryGetValue(multiBlockId, out info))
            {
                return null;
            }
            return info;
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_CubeGrid objectBuilder = (MyObjectBuilder_CubeGrid) base.GetObjectBuilder(copy);
            this.GetObjectBuilderInternal(objectBuilder, copy);
            return objectBuilder;
        }

        private void GetObjectBuilderInternal(MyObjectBuilder_CubeGrid ob, bool copy)
        {
            SerializableVector3I? nullable2;
            SerializableVector3I? nullable1;
            SerializableVector3I? nullable3;
            SerializableVector3I? nullable4;
            ob.GridSizeEnum = this.GridSizeEnum;
            if (ob.Skeleton == null)
            {
                ob.Skeleton = new List<VRage.Game.BoneInfo>();
            }
            ob.Skeleton.Clear();
            this.Skeleton.Serialize(ob.Skeleton, this.GridSize, this);
            ob.IsStatic = this.IsStatic;
            ob.IsUnsupportedStation = this.IsUnsupportedStation;
            ob.Editable = this.Editable;
            ob.IsPowered = this.m_IsPowered;
            ob.CubeBlocks.Clear();
            foreach (MySlimBlock block in this.m_cubeBlocks)
            {
                MyObjectBuilder_CubeBlock item = null;
                item = !copy ? block.GetObjectBuilder(false) : block.GetCopyObjectBuilder();
                if (item != null)
                {
                    ob.CubeBlocks.Add(item);
                }
            }
            ob.PersistentFlags = this.Render.PersistentFlags;
            if (this.Physics != null)
            {
                ob.LinearVelocity = this.Physics.LinearVelocity;
                ob.AngularVelocity = this.Physics.AngularVelocity;
            }
            Vector3I? xSymmetryPlane = this.XSymmetryPlane;
            if (xSymmetryPlane != null)
            {
                nullable1 = new SerializableVector3I?(xSymmetryPlane.GetValueOrDefault());
            }
            else
            {
                nullable2 = null;
                nullable1 = nullable2;
            }
            ob.XMirroxPlane = nullable1;
            xSymmetryPlane = this.YSymmetryPlane;
            if (xSymmetryPlane != null)
            {
                nullable3 = new SerializableVector3I?(xSymmetryPlane.GetValueOrDefault());
            }
            else
            {
                nullable2 = null;
                nullable3 = nullable2;
            }
            ob.YMirroxPlane = nullable3;
            xSymmetryPlane = this.ZSymmetryPlane;
            if (xSymmetryPlane != null)
            {
                nullable4 = new SerializableVector3I?(xSymmetryPlane.GetValueOrDefault());
            }
            else
            {
                nullable2 = null;
                nullable4 = nullable2;
            }
            ob.ZMirroxPlane = nullable4;
            ob.XMirroxOdd = this.XSymmetryOdd;
            ob.YMirroxOdd = this.YSymmetryOdd;
            ob.ZMirroxOdd = this.ZSymmetryOdd;
            if (copy)
            {
                ob.Name = null;
            }
            ob.BlockGroups.Clear();
            foreach (MyBlockGroup group in this.BlockGroups)
            {
                ob.BlockGroups.Add(group.GetObjectBuilder());
            }
            ob.DisplayName = base.DisplayName;
            ob.DestructibleBlocks = this.DestructibleBlocks;
            ob.IsRespawnGrid = this.IsRespawnGrid;
            ob.playedTime = this.m_playedTime;
            ob.GridGeneralDamageModifier = this.GridGeneralDamageModifier;
            ob.LocalCoordSys = this.LocalCoordSystem;
            ob.TargetingWhitelist = this.m_targetingListIsWhitelist;
            ob.TargetingTargets = this.m_targetingList;
            this.GridSystems.GetObjectBuilder(ob);
        }

        public BoundingBoxD GetPhysicalGroupAABB()
        {
            if (base.MarkedForClose)
            {
                return BoundingBoxD.CreateInvalid();
            }
            BoundingBoxD worldAABB = base.PositionComp.WorldAABB;
            MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = MyCubeGridGroups.Static.Physical.GetGroup(this);
            if (group != null)
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node node in group.Nodes)
                {
                    if (node.NodeData.PositionComp != null)
                    {
                        worldAABB.Include(node.NodeData.PositionComp.WorldAABB);
                    }
                }
            }
            return worldAABB;
        }

        public List<HkShape> GetShapesFromPosition(Vector3I pos) => 
            this.Physics.GetShapesFromPosition(pos);

        private ushort GetSubBlockId(MySlimBlock slimBlock)
        {
            MySlimBlock cubeBlock = slimBlock.CubeGrid.GetCubeBlock(slimBlock.Position);
            MyCompoundCubeBlock fatBlock = null;
            if (cubeBlock != null)
            {
                fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
            }
            if (fatBlock == null)
            {
                return 0;
            }
            ushort? blockId = fatBlock.GetBlockId(slimBlock);
            return ((blockId != null) ? blockId.GetValueOrDefault() : 0);
        }

        public MySlimBlock GetTargetedBlock(Vector3D position)
        {
            Vector3I vectori;
            this.FixTargetCube(out vectori, (Vector3) (Vector3D.Transform(position, base.PositionComp.WorldMatrixNormalizedInv) * this.GridSizeR));
            return this.GetCubeBlock(vectori);
        }

        public MySlimBlock GetTargetedBlockLite(Vector3D position)
        {
            Vector3I vectori;
            this.FixTargetCubeLite(out vectori, Vector3D.Transform(position, base.PositionComp.WorldMatrixNormalizedInv) * this.GridSizeR);
            return this.GetCubeBlock(vectori);
        }

        public static VRage.Game.Entity.MyEntity GetTargetEntity()
        {
            LineD ray = new LineD(MySector.MainCamera.Position, MySector.MainCamera.Position + (MySector.MainCamera.ForwardVector * 10000f));
            m_tmpHitList.Clear();
            MyPhysics.CastRay(ray.From, ray.To, m_tmpHitList, 15);
            m_tmpHitList.RemoveAll(hit => (MySession.Static.ControlledEntity != null) && ReferenceEquals(hit.HkHitInfo.GetHitEntity(), MySession.Static.ControlledEntity.Entity));
            if (m_tmpHitList.Count == 0)
            {
                using (MyUtils.ReuseCollection<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>>(ref m_lineOverlapList))
                {
                    MyGamePruningStructure.GetTopmostEntitiesOverlappingRay(ref ray, m_lineOverlapList, MyEntityQueryType.Both);
                    return ((m_lineOverlapList.Count <= 0) ? null : m_lineOverlapList[0].Element.GetTopMostParent(null));
                }
            }
            return (m_tmpHitList[0].HkHitInfo.GetHitEntity() as VRage.Game.Entity.MyEntity);
        }

        public static MyCubeGrid GetTargetGrid()
        {
            VRage.Game.Entity.MyEntity targetEntity = MyCubeBuilder.Static.FindClosestGrid();
            if (targetEntity == null)
            {
                targetEntity = GetTargetEntity();
            }
            return (targetEntity as MyCubeGrid);
        }

        private static List<HalfVector2> GetUVsForModel(MyExportModel renderModel, int modelVerticesCount) => 
            renderModel.GetTexCoords().ToList<HalfVector2>();

        private unsafe void GetValidBuildOffsets(ref MyBlockBuildArea area, List<Vector3UByte> resultOffsets, HashSet<Vector3UByte> resultFailList)
        {
            Vector3UByte num;
            Vector3I stepDelta = (Vector3I) area.StepDelta;
            MyBlockOrientation orientation = new MyBlockOrientation(area.OrientationForward, area.OrientationUp);
            MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition((MyDefinitionId) area.DefinitionId);
            num.X = 0;
            while (num.X < area.BuildAreaSize.X)
            {
                num.Y = 0;
                while (true)
                {
                    if (num.Y >= area.BuildAreaSize.Y)
                    {
                        byte* numPtr3 = (byte*) ref num.X;
                        numPtr3[0] = (byte) (numPtr3[0] + 1);
                        break;
                    }
                    num.Z = 0;
                    while (true)
                    {
                        if (num.Z >= area.BuildAreaSize.Z)
                        {
                            byte* numPtr2 = (byte*) ref num.Y;
                            numPtr2[0] = (byte) (numPtr2[0] + 1);
                            break;
                        }
                        Vector3I vectori2 = (Vector3I) (area.PosInGrid + (num * stepDelta));
                        int? ignoreMultiblockId = null;
                        if (this.CanPlaceBlock((Vector3I) (vectori2 + area.BlockMin), (Vector3I) (vectori2 + area.BlockMax), orientation, cubeBlockDefinition, ignoreMultiblockId, false))
                        {
                            resultOffsets.Add(num);
                        }
                        else
                        {
                            resultFailList.Add(num);
                        }
                        byte* numPtr1 = (byte*) ref num.Z;
                        numPtr1[0] = (byte) (numPtr1[0] + 1);
                    }
                }
            }
        }

        public Vector3D GridIntegerToWorld(Vector3D gridCoords) => 
            Vector3D.Transform(gridCoords * this.GridSize, base.WorldMatrix);

        public Vector3D GridIntegerToWorld(Vector3I gridCoords) => 
            GridIntegerToWorld(this.GridSize, gridCoords, base.WorldMatrix);

        public static Vector3D GridIntegerToWorld(float gridSize, Vector3I gridCoords, MatrixD worldMatrix) => 
            Vector3D.Transform((Vector3D) (gridCoords * gridSize), worldMatrix);

        private void HandBrakeChanged()
        {
            this.GridSystems.WheelSystem.HandBrake = (bool) this.m_handBrakeSync;
        }

        public bool HasMainCockpit() => 
            (this.MainCockpit != null);

        public bool HasMainRemoteControl() => 
            (this.MainRemoteControl != null);

        public bool HasStandAloneBlocks()
        {
            if (this.m_hasStandAloneBlocks)
            {
                if ((MyPerGameSettings.Game != GameEnum.SE_GAME) && (MyPerGameSettings.Game != GameEnum.VRS_GAME))
                {
                    this.m_hasStandAloneBlocks = this.m_cubeBlocks.Count > 0;
                }
                else
                {
                    using (HashSet<MySlimBlock>.Enumerator enumerator = this.m_cubeBlocks.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            if (enumerator.Current.BlockDefinition.IsStandAlone)
                            {
                                return true;
                            }
                        }
                    }
                    this.m_hasStandAloneBlocks = false;
                }
            }
            return this.m_hasStandAloneBlocks;
        }

        public static bool HasStandAloneBlocks(List<MySlimBlock> blocks, int offset, int count)
        {
            if (offset < 0)
            {
                MySandboxGame.Log.WriteLine($"Negative offset in HasStandAloneBlocks - {offset}");
                return false;
            }
            for (int i = offset; (i < (offset + count)) && (i < blocks.Count); i++)
            {
                MySlimBlock block = blocks[i];
                if ((block != null) && block.BlockDefinition.IsStandAlone)
                {
                    return true;
                }
            }
            return false;
        }

        internal void HavokSystemIDChanged(int id)
        {
            this.OnHavokSystemIDChanged.InvokeIfNotNull<int>(id);
        }

        private void Hierarchy_OnChildRemoved(VRage.ModAPI.IMyEntity obj)
        {
            this.m_fatBlocks.Remove(obj as MyCubeBlock);
        }

        public void HierarchyUpdated(MyCubeGrid root)
        {
            MyGridPhysics physics = this.Physics;
            if (physics != null)
            {
                if (!ReferenceEquals(this, root))
                {
                    physics.SetRelaxedRigidBodyMaxVelocities();
                }
                else
                {
                    physics.SetDefaultRigidBodyMaxVelocities();
                }
            }
            this.OnHierarchyUpdated.InvokeIfNotNull<MyCubeGrid>(this);
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            this.InitInternal(objectBuilder, true);
        }

        private void InitInternal(MyObjectBuilder_EntityBase objectBuilder, bool rebuildGrid)
        {
            int num;
            MySlimBlock block2;
            List<MyDefinitionId> list = new List<MyDefinitionId>();
            base.SyncFlag = true;
            MyObjectBuilder_CubeGrid builder = (MyObjectBuilder_CubeGrid) objectBuilder;
            if (builder != null)
            {
                this.GridSizeEnum = builder.GridSizeEnum;
            }
            this.GridScale = MyDefinitionManager.Static.GetCubeSize(this.GridSizeEnum) / MyDefinitionManager.Static.GetCubeSizeOriginal(this.GridSizeEnum);
            base.Init(objectBuilder);
            float? scale = null;
            this.Init(null, null, null, scale, null);
            this.m_destructibleBlocks.SetLocalValue(builder.DestructibleBlocks);
            bool flag1 = MyFakes.ASSERT_NON_PUBLIC_BLOCKS;
            if (MyFakes.REMOVE_NON_PUBLIC_BLOCKS)
            {
                this.RemoveNonPublicBlocks(builder);
            }
            this.Render.CreateAdditionalModelGenerators((builder != null) ? builder.GridSizeEnum : MyCubeSize.Large);
            this.m_hasAdditionalModelGenerators = this.AdditionalModelGenerators.Count > 0;
            this.CreateSystems();
            if (builder == null)
            {
                goto TR_0011;
            }
            else
            {
                this.IsStatic = builder.IsStatic;
                this.IsUnsupportedStation = builder.IsUnsupportedStation;
                this.CreatePhysics = builder.CreatePhysics;
                this.m_enableSmallToLargeConnections = builder.EnableSmallToLargeConnections;
                this.GridSizeEnum = builder.GridSizeEnum;
                this.Editable = builder.Editable;
                this.m_IsPowered = builder.IsPowered;
                this.GridSystems.BeforeBlockDeserialization(builder);
                this.m_cubes.Clear();
                this.m_cubeBlocks.Clear();
                this.m_fatBlocks.Clear();
                this.m_inventories.Clear();
                base.DisplayName = (builder.DisplayName != null) ? builder.DisplayName : this.MakeCustomName();
                m_tmpOccupiedCockpits.Clear();
                num = 0;
            }
            goto TR_0049;
        TR_0011:
            this.Render.CastShadows = true;
            this.Render.NeedsResolveCastShadow = false;
            if (MyStructuralIntegrity.Enabled)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
            using (List<MyCockpit>.Enumerator enumerator3 = m_tmpOccupiedCockpits.GetEnumerator())
            {
                while (enumerator3.MoveNext())
                {
                    enumerator3.Current.GiveControlToPilot();
                }
            }
            m_tmpOccupiedCockpits.Clear();
            if (MyFakes.ENABLE_MULTIBLOCK_PART_IDS)
            {
                this.PrepareMultiBlockInfos();
            }
            this.m_isRespawnGrid.SetLocalValue(builder.IsRespawnGrid);
            this.m_playedTime = builder.playedTime;
            this.GridGeneralDamageModifier = builder.GridGeneralDamageModifier;
            this.LocalCoordSystem = builder.LocalCoordSys;
            this.m_dampenersEnabled.SetLocalValue(builder.DampenersEnabled);
            if (builder.TargetingTargets != null)
            {
                this.m_targetingList = builder.TargetingTargets;
            }
            this.m_targetingListIsWhitelist = builder.TargetingWhitelist;
            this.m_usesTargetingList = (this.m_targetingList.Count > 0) || this.m_targetingListIsWhitelist;
            if (builder.TargetingTargets != null)
            {
                this.m_targetingList = builder.TargetingTargets;
            }
            this.m_targetingListIsWhitelist = builder.TargetingWhitelist;
            this.m_usesTargetingList = (this.m_targetingList.Count > 0) || this.m_targetingListIsWhitelist;
            return;
        TR_0036:
            num++;
        TR_0049:
            while (true)
            {
                if (num < builder.CubeBlocks.Count)
                {
                    MyObjectBuilder_CubeBlock block = builder.CubeBlocks[num];
                    block2 = this.AddBlock(block, false);
                    if (block2 == null)
                    {
                        goto TR_0036;
                    }
                    else
                    {
                        if (block2.FatBlock is MyCompoundCubeBlock)
                        {
                            foreach (MySlimBlock block3 in (block2.FatBlock as MyCompoundCubeBlock).GetBlocks())
                            {
                                if (!list.Contains(block3.BlockDefinition.Id))
                                {
                                    list.Add(block3.BlockDefinition.Id);
                                }
                            }
                            break;
                        }
                        if (!list.Contains(block2.BlockDefinition.Id))
                        {
                            list.Add(block2.BlockDefinition.Id);
                        }
                    }
                }
                else
                {
                    Vector3I? nullable3;
                    Vector3I? nullable1;
                    Vector3I? nullable4;
                    Vector3I? nullable5;
                    this.GridSystems.AfterBlockDeserialization();
                    if (builder.Skeleton != null)
                    {
                        this.Skeleton.Deserialize(builder.Skeleton, this.GridSize, this.GridSize, false);
                    }
                    this.Render.RenderData.SetBasePositionHint((Vector3) ((this.Min * this.GridSize) - this.GridSize));
                    if (rebuildGrid)
                    {
                        this.RebuildGrid(false);
                    }
                    foreach (MyObjectBuilder_BlockGroup group in builder.BlockGroups)
                    {
                        this.AddGroup(group);
                    }
                    if (this.Physics != null)
                    {
                        Vector3 linearVelocity = (Vector3) builder.LinearVelocity;
                        Vector3 angularVelocity = (Vector3) builder.AngularVelocity;
                        Vector3.ClampToSphere(ref linearVelocity, this.Physics.GetMaxRelaxedLinearVelocity());
                        Vector3.ClampToSphere(ref angularVelocity, this.Physics.GetMaxRelaxedAngularVelocity());
                        this.Physics.LinearVelocity = linearVelocity;
                        this.Physics.AngularVelocity = angularVelocity;
                        if (!this.IsStatic)
                        {
                            this.Physics.Shape.BlocksConnectedToWorld.Clear();
                        }
                        if (MyPerGameSettings.InventoryMass)
                        {
                            this.m_inventoryMassDirty = true;
                        }
                    }
                    SerializableVector3I? xMirroxPlane = builder.XMirroxPlane;
                    if (xMirroxPlane != null)
                    {
                        nullable1 = new Vector3I?(xMirroxPlane.GetValueOrDefault());
                    }
                    else
                    {
                        nullable3 = null;
                        nullable1 = nullable3;
                    }
                    this.XSymmetryPlane = nullable1;
                    xMirroxPlane = builder.YMirroxPlane;
                    if (xMirroxPlane != null)
                    {
                        nullable4 = new Vector3I?(xMirroxPlane.GetValueOrDefault());
                    }
                    else
                    {
                        nullable3 = null;
                        nullable4 = nullable3;
                    }
                    this.YSymmetryPlane = nullable4;
                    xMirroxPlane = builder.ZMirroxPlane;
                    if (xMirroxPlane != null)
                    {
                        nullable5 = new Vector3I?(xMirroxPlane.GetValueOrDefault());
                    }
                    else
                    {
                        nullable3 = null;
                        nullable5 = nullable3;
                    }
                    this.ZSymmetryPlane = nullable5;
                    this.XSymmetryOdd = builder.XMirroxOdd;
                    this.YSymmetryOdd = builder.YMirroxOdd;
                    this.ZSymmetryOdd = builder.ZMirroxOdd;
                    this.GridSystems.Init(builder);
                    if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
                    {
                        this.m_ownershipManager = new MyCubeGridOwnershipManager();
                        this.m_ownershipManager.Init(this);
                    }
                    if (base.Hierarchy != null)
                    {
                        base.Hierarchy.OnChildRemoved += new Action<VRage.ModAPI.IMyEntity>(this.Hierarchy_OnChildRemoved);
                    }
                    goto TR_0011;
                }
                break;
            }
            if (block2.FatBlock is MyCockpit)
            {
                MyCockpit fatBlock = block2.FatBlock as MyCockpit;
                if (fatBlock.Pilot != null)
                {
                    m_tmpOccupiedCockpits.Add(fatBlock);
                }
            }
            goto TR_0036;
        }

        public static bool IsAabbInsideVoxel(MatrixD worldMatrix, BoundingBoxD localAabb, MyGridPlacementSettings settings)
        {
            if (settings.VoxelPlacement != null)
            {
                BoundingBoxD box = localAabb.TransformFast(ref worldMatrix);
                List<MyVoxelBase> result = new List<MyVoxelBase>();
                MyGamePruningStructure.GetAllVoxelMapsInBox(ref box, result);
                using (List<MyVoxelBase>.Enumerator enumerator = result.GetEnumerator())
                {
                    while (true)
                    {
                        bool flag;
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyVoxelBase current = enumerator.Current;
                        if ((settings.VoxelPlacement.Value.PlacementMode != VoxelPlacementMode.Volumetric) && current.IsAnyAabbCornerInside(ref worldMatrix, localAabb))
                        {
                            flag = true;
                        }
                        else
                        {
                            if ((settings.VoxelPlacement.Value.PlacementMode != VoxelPlacementMode.Volumetric) || TestPlacementVoxelMapPenetration(current, settings, ref localAabb, ref worldMatrix, false))
                            {
                                continue;
                            }
                            flag = true;
                        }
                        return flag;
                    }
                }
            }
            return false;
        }

        private bool IsDamaged(Vector3I bonePos, float epsilon = 0.04f)
        {
            Vector3 vector;
            return (this.Skeleton.TryGetBone(ref bonePos, out vector) && !MyUtils.IsZero(ref vector, epsilon * this.GridSize));
        }

        public bool IsDirty() => 
            this.m_dirtyRegion.IsDirty;

        public bool IsGizmoDrawingEnabled() => 
            (ShowSenzorGizmos || (ShowGravityGizmos || ShowAntennaGizmos));

        public static bool IsGridInCompleteState(MyCubeGrid grid)
        {
            using (HashSet<MySlimBlock>.Enumerator enumerator = grid.CubeBlocks.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MySlimBlock current = enumerator.Current;
                    if (!current.IsFullIntegrity || (current.BuildLevelRatio != 1f))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsInSameLogicalGroupAs(VRage.Game.ModAPI.IMyCubeGrid other) => 
            (ReferenceEquals(this, other) || ReferenceEquals(MyCubeGridGroups.Static.Logical.GetGroup(this), MyCubeGridGroups.Static.Logical.GetGroup((MyCubeGrid) other)));

        public static bool IsInVoxels(MySlimBlock block, bool checkForPhysics = true)
        {
            if (!(ReferenceEquals(block.CubeGrid.Physics, null) & checkForPhysics))
            {
                BoundingBoxD xd;
                if (MyPerGameSettings.Destruction && (block.CubeGrid.GridSizeEnum == MyCubeSize.Large))
                {
                    return block.CubeGrid.Physics.Shape.BlocksConnectedToWorld.Contains(block.Position);
                }
                block.GetWorldBoundingBox(out xd, false);
                m_tmpVoxelList.Clear();
                MyGamePruningStructure.GetAllVoxelMapsInBox(ref xd, m_tmpVoxelList);
                float gridSize = block.CubeGrid.GridSize;
                BoundingBoxD aabb = new BoundingBoxD((Vector3D) (gridSize * (block.Min - 0.5)), (Vector3D) (gridSize * (block.Max + 0.5)));
                MatrixD worldMatrix = block.CubeGrid.WorldMatrix;
                using (List<MyVoxelBase>.Enumerator enumerator = m_tmpVoxelList.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (enumerator.Current.IsAnyAabbCornerInside(ref worldMatrix, aabb))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool IsMainCockpit(MyTerminalBlock cockpit) => 
            ReferenceEquals(this.MainCockpit, cockpit);

        public bool IsMainRemoteControl(MyTerminalBlock remoteControl) => 
            ReferenceEquals(this.MainRemoteControl, remoteControl);

        private bool IsMergePossible_Static(MySlimBlock block, MyCubeGrid gridToMerge, out Vector3I gridOffset)
        {
            Vector3I vectori;
            Quaternion quaternion;
            Vector3D vectord = Vector3D.Transform(base.PositionComp.GetPosition(), gridToMerge.PositionComp.WorldMatrixNormalizedInv);
            gridOffset = -Vector3I.Round(vectord * this.GridSizeR);
            if (!IsOrientationsAligned(gridToMerge.WorldMatrix, base.WorldMatrix))
            {
                return false;
            }
            MatrixI matrix = gridToMerge.CalculateMergeTransform(this, -gridOffset);
            Vector3I.Transform(ref block.Position, ref matrix, out vectori);
            MatrixI.Transform(ref block.Orientation, ref matrix).GetQuaternion(out quaternion);
            MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = block.BlockDefinition.GetBuildProgressModelMountPoints(block.BuildLevelRatio);
            return CheckConnectivity(gridToMerge, block.BlockDefinition, buildProgressModelMountPoints, ref quaternion, ref vectori);
        }

        private static bool IsOrientationsAligned(MatrixD transform1, MatrixD transform2)
        {
            double num = Vector3D.Dot(transform1.Forward, transform2.Forward);
            if (((num > 0.0010000000474974513) && (num < 0.99899999995250255)) || ((num < -0.0010000000474974513) && (num > -0.99899999995250255)))
            {
                return false;
            }
            double num2 = Vector3D.Dot(transform1.Up, transform2.Up);
            if (((num2 > 0.0010000000474974513) && (num2 < 0.99899999995250255)) || ((num2 < -0.0010000000474974513) && (num2 > -0.99899999995250255)))
            {
                return false;
            }
            double num3 = Vector3D.Dot(transform1.Right, transform2.Right);
            return (((num3 <= 0.0010000000474974513) || (num3 >= 0.99899999995250255)) && ((num3 >= -0.0010000000474974513) || (num3 <= -0.99899999995250255)));
        }

        public bool IsRoomAtPositionAirtight(Vector3I pos)
        {
            MyOxygenRoom oxygenRoomForCubeGridPosition = this.GridSystems.GasSystem.GetOxygenRoomForCubeGridPosition(ref pos);
            return ((oxygenRoomForCubeGridPosition != null) ? oxygenRoomForCubeGridPosition.IsAirtight : false);
        }

        private static bool IsRooted(MyCubeGrid grid)
        {
            if (grid.IsStatic)
            {
                return true;
            }
            MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = MyCubeGridGroups.Static.Physical.GetGroup(grid);
            if (group != null)
            {
                using (HashSet<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node>.Enumerator enumerator = group.Nodes.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (MyFixedGrids.IsRooted(enumerator.Current.NodeData))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool IsSameConstructAs(VRage.Game.ModAPI.IMyCubeGrid other) => 
            (ReferenceEquals(this, other) || ReferenceEquals(MyCubeGridGroups.Static.Mechanical.GetGroup(this), MyCubeGridGroups.Static.Mechanical.GetGroup((MyCubeGrid) other)));

        public unsafe bool IsTouchingAnyNeighbor(Vector3I min, Vector3I max)
        {
            Vector3I vectori = min;
            int* numPtr1 = (int*) ref vectori.X;
            numPtr1[0]--;
            Vector3I vectori2 = max;
            vectori2.X = vectori.X;
            if (!this.CanAddCubes(vectori, vectori2))
            {
                return true;
            }
            Vector3I vectori3 = min;
            int* numPtr2 = (int*) ref vectori3.Y;
            numPtr2[0]--;
            Vector3I vectori4 = max;
            vectori4.Y = vectori3.Y;
            if (!this.CanAddCubes(vectori3, vectori4))
            {
                return true;
            }
            Vector3I vectori5 = min;
            int* numPtr3 = (int*) ref vectori5.Z;
            numPtr3[0]--;
            Vector3I vectori6 = max;
            vectori6.Z = vectori5.Z;
            if (!this.CanAddCubes(vectori5, vectori6))
            {
                return true;
            }
            Vector3I vectori7 = max;
            int* numPtr4 = (int*) ref vectori7.X;
            numPtr4[0]++;
            Vector3I vectori8 = min;
            vectori8.X = vectori7.X;
            if (!this.CanAddCubes(vectori8, vectori7))
            {
                return true;
            }
            Vector3I vectori9 = max;
            int* numPtr5 = (int*) ref vectori9.Y;
            numPtr5[0]++;
            Vector3I vectori10 = min;
            vectori10.Y = vectori9.Y;
            if (!this.CanAddCubes(vectori10, vectori9))
            {
                return true;
            }
            Vector3I vectori11 = max;
            int* numPtr6 = (int*) ref vectori11.Z;
            numPtr6[0]++;
            Vector3I vectori12 = min;
            vectori12.Z = vectori11.Z;
            return !this.CanAddCubes(vectori12, vectori11);
        }

        private bool IsWithinWorldLimits(long ownerID, int blocksToBuild, int pcu, string name)
        {
            string str;
            return (MySession.Static.IsWithinWorldLimits(out str, ownerID, name, pcu, blocksToBuild, this.BlocksCount, null) == MySession.LimitResult.Passed);
        }

        public static void KillAllCharacters(MyCubeGrid grid)
        {
            if ((grid != null) && Sync.IsServer)
            {
                foreach (MyCockpit cockpit in grid.GetFatBlocks<MyCockpit>())
                {
                    if (cockpit == null)
                    {
                        continue;
                    }
                    if ((cockpit.Pilot != null) && !cockpit.Pilot.IsDead)
                    {
                        cockpit.Pilot.DoDamage(1000f, MyDamageType.Suicide, true, cockpit.Pilot.EntityId);
                        cockpit.RemovePilot();
                    }
                }
            }
        }

        public Vector3I LocalToGridInteger(Vector3 localCoords)
        {
            localCoords *= this.GridSizeR;
            return Vector3I.Round(localCoords);
        }

        public void LogHierarchy()
        {
            this.OnLogHierarchy();
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid>(this, x => new Action(x.OnLogHierarchy), targetEndpoint);
        }

        private string MakeCustomName()
        {
            StringBuilder builder = new StringBuilder();
            int m = 0x2710;
            long num2 = MyMath.Mod(base.EntityId, m);
            string str = null;
            if (this.IsStatic)
            {
                str = MyTexts.GetString(MyCommonTexts.DetailStaticGrid);
            }
            else
            {
                MyCubeSize gridSizeEnum = this.GridSizeEnum;
                if (gridSizeEnum == MyCubeSize.Large)
                {
                    str = MyTexts.GetString(MyCommonTexts.DetailLargeGrid);
                }
                else if (gridSizeEnum == MyCubeSize.Small)
                {
                    str = MyTexts.GetString(MyCommonTexts.DetailSmallGrid);
                }
            }
            builder.Append(str ?? "Grid").Append(" ").Append(num2.ToString());
            return builder.ToString();
        }

        public void MarkAsTrash()
        {
            this.m_markedAsTrash.Value = true;
        }

        private void MarkedAsTrashChanged()
        {
            if (this.MarkedAsTrash)
            {
                this.MarkForDraw();
                this.MarkForUpdate();
                this.m_trashHighlightCounter = TRASH_HIGHLIGHT;
            }
        }

        internal void MarkForDraw()
        {
            MySandboxGame.Static.Invoke(delegate {
                if (!base.Closed)
                {
                    this.Render.NeedsDraw = true;
                }
            }, "MarkForDraw()");
        }

        internal void MarkForUpdate()
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        internal void MarkForUpdateParallel()
        {
            if ((base.NeedsUpdate & MyEntityUpdateEnum.EACH_FRAME) == MyEntityUpdateEnum.NONE)
            {
                MySandboxGame.Static.Invoke(delegate {
                    if (!base.Closed)
                    {
                        this.MarkForUpdate();
                    }
                }, "MarkForUpdate");
            }
        }

        public MyCubeGrid MergeGrid_MergeBlock(MyCubeGrid gridToMerge, Vector3I gridOffset, bool checkMergeOrder = true)
        {
            if (checkMergeOrder && !this.ShouldBeMergedToThis(gridToMerge))
            {
                return null;
            }
            MatrixI transform = this.CalculateMergeTransform(gridToMerge, gridOffset);
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseBlockingEvent<MyCubeGrid, long, SerializableVector3I, Base6Directions.Direction, Base6Directions.Direction, MyCubeGrid>(this, gridToMerge, x => new Action<long, SerializableVector3I, Base6Directions.Direction, Base6Directions.Direction>(x.MergeGrid_MergeBlockClient), gridToMerge.EntityId, transform.Translation, transform.Forward, transform.Up, targetEndpoint);
            return this.MergeGridInternal(gridToMerge, ref transform, true);
        }

        [Event(null, 0x71), Reliable, Broadcast, Blocking]
        private void MergeGrid_MergeBlockClient(long gridId, SerializableVector3I gridOffset, Base6Directions.Direction gridForward, Base6Directions.Direction gridUp)
        {
            MyCubeGrid entity = null;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(gridId, out entity, false))
            {
                MatrixI transform = new MatrixI((Vector3I) gridOffset, gridForward, gridUp);
                this.MergeGridInternal(entity, ref transform, true);
            }
        }

        [Event(null, 0x5e), Reliable, Broadcast, Blocking]
        private void MergeGrid_MergeClient(long gridId, SerializableVector3I gridOffset, Base6Directions.Direction gridForward, Base6Directions.Direction gridUp, Vector3I mergingBlockPos)
        {
            MyCubeGrid entity = null;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(gridId, out entity, false))
            {
                MatrixI transform = new MatrixI((Vector3I) gridOffset, gridForward, gridUp);
                MyCubeGrid grid1 = this.MergeGridInternal(entity, ref transform, true);
                MySlimBlock cubeBlock = grid1.GetCubeBlock(mergingBlockPos);
                using (List<IMyBlockAdditionalModelGenerator>.Enumerator enumerator = grid1.AdditionalModelGenerators.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.BlockAddedToMergedGrid(cubeBlock);
                    }
                }
            }
        }

        private MyCubeGrid MergeGrid_Static(MyCubeGrid gridToMerge, Vector3I gridOffset, MySlimBlock triggeringMergeBlock)
        {
            MatrixI transformation = this.CalculateMergeTransform(gridToMerge, gridOffset);
            Vector3I position = triggeringMergeBlock.Position;
            if (!ReferenceEquals(triggeringMergeBlock.CubeGrid, this))
            {
                position = Vector3I.Transform(position, transformation);
            }
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseBlockingEvent<MyCubeGrid, long, SerializableVector3I, Base6Directions.Direction, Base6Directions.Direction, Vector3I, MyCubeGrid>(this, gridToMerge, x => new Action<long, SerializableVector3I, Base6Directions.Direction, Base6Directions.Direction, Vector3I>(x.MergeGrid_MergeClient), gridToMerge.EntityId, transformation.Translation, transformation.Forward, transformation.Up, position, targetEndpoint);
            MyCubeGrid grid = this.MergeGridInternal(gridToMerge, ref transformation, true);
            using (List<IMyBlockAdditionalModelGenerator>.Enumerator enumerator = grid.AdditionalModelGenerators.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.BlockAddedToMergedGrid(triggeringMergeBlock);
                }
            }
            return grid;
        }

        private unsafe MyCubeGrid MergeGridInternal(MyCubeGrid gridToMerge, ref MatrixI transform, bool disableBlockGenerators = true)
        {
            if (MyCubeGridSmallToLargeConnection.Static != null)
            {
                MyCubeGridSmallToLargeConnection.Static.BeforeGridMerge_SmallToLargeGridConnectivity(this, gridToMerge);
            }
            MyRenderComponentCubeGrid tmpRenderComponent = gridToMerge.Render;
            tmpRenderComponent.DeferRenderRelease = true;
            Matrix transformMatrix = transform.GetFloatMatrix();
            Matrix* matrixPtr1 = (Matrix*) ref transformMatrix;
            matrixPtr1.Translation *= this.GridSize;
            Action<MatrixD> updateMergingComponentWM = delegate (MatrixD matrix) {
                tmpRenderComponent.UpdateRenderObjectMatrices(transformMatrix * matrix);
            };
            Action releaseRenderOldRenderComponent = null;
            releaseRenderOldRenderComponent = delegate {
                tmpRenderComponent.DeferRenderRelease = false;
                this.m_updateMergingGrids = (Action<MatrixD>) Delegate.Remove(this.m_updateMergingGrids, updateMergingComponentWM);
                this.m_pendingGridReleases = (Action) Delegate.Remove(this.m_pendingGridReleases, releaseRenderOldRenderComponent);
            };
            this.m_updateMergingGrids = (Action<MatrixD>) Delegate.Combine(this.m_updateMergingGrids, updateMergingComponentWM);
            this.m_pendingGridReleases = (Action) Delegate.Combine(this.m_pendingGridReleases, releaseRenderOldRenderComponent);
            this.MarkForUpdate();
            MoveBlocksAndClose(gridToMerge, this, transform, disableBlockGenerators);
            this.UpdateGridAABB();
            if (this.Physics != null)
            {
                this.UpdatePhysicsShape();
            }
            if (MyCubeGridSmallToLargeConnection.Static != null)
            {
                MyCubeGridSmallToLargeConnection.Static.AfterGridMerge_SmallToLargeGridConnectivity(this);
            }
            updateMergingComponentWM(base.WorldMatrix);
            return this;
        }

        public void ModifyGroup(MyBlockGroup group)
        {
            this.m_tmpBlockIdList.Clear();
            foreach (MyTerminalBlock block in group.Blocks)
            {
                this.m_tmpBlockIdList.Add(block.EntityId);
            }
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, string, List<long>>(this, x => new Action<string, List<long>>(x.OnModifyGroupSuccess), group.Name.ToString(), this.m_tmpBlockIdList, targetEndpoint);
        }

        private static void MoveBlocks(MyCubeGrid from, MyCubeGrid to, List<MySlimBlock> cubeBlocks, int offset, int count)
        {
            from.EnableGenerators(false, true);
            to.EnableGenerators(false, true);
            to.IsBlockTrasferInProgress = true;
            from.IsBlockTrasferInProgress = true;
            try
            {
                m_tmpBlockGroups.Clear();
                foreach (MyBlockGroup group in from.BlockGroups)
                {
                    m_tmpBlockGroups.Add(group.GetObjectBuilder());
                }
                int num = offset;
                while (true)
                {
                    if (num >= (offset + count))
                    {
                        if (from.Physics != null)
                        {
                            for (int i = offset; i < (offset + count); i++)
                            {
                                MySlimBlock block2 = cubeBlocks[i];
                                if (block2 != null)
                                {
                                    from.Physics.AddDirtyBlock(block2);
                                }
                            }
                        }
                        int num3 = offset;
                        while (true)
                        {
                            if (num3 >= (offset + count))
                            {
                                foreach (MyObjectBuilder_BlockGroup group2 in m_tmpBlockGroups)
                                {
                                    MyBlockGroup group3 = new MyBlockGroup();
                                    group3.Init(to, group2);
                                    if (group3.Blocks.Count > 0)
                                    {
                                        to.AddGroup(group3);
                                    }
                                }
                                m_tmpBlockGroups.Clear();
                                from.RemoveEmptyBlockGroups();
                                break;
                            }
                            MySlimBlock block3 = cubeBlocks[num3];
                            if (block3 != null)
                            {
                                to.AddBlockInternal(block3);
                                from.Skeleton.CopyTo(to.Skeleton, block3.Position);
                            }
                            num3++;
                        }
                        break;
                    }
                    MySlimBlock block = cubeBlocks[num];
                    if (block != null)
                    {
                        if (block.FatBlock != null)
                        {
                            from.Hierarchy.RemoveChild(block.FatBlock, false);
                        }
                        from.RemoveBlockInternal(block, false, false);
                    }
                    num++;
                }
            }
            finally
            {
                from.EnableGenerators(true, true);
                to.EnableGenerators(true, true);
                to.IsBlockTrasferInProgress = false;
                from.IsBlockTrasferInProgress = false;
            }
        }

        private static void MoveBlocksAndClose(MyCubeGrid from, MyCubeGrid to, MatrixI transform, bool disableBlockGenerators = true)
        {
            from.MarkedForClose = true;
            to.IsBlockTrasferInProgress = true;
            from.IsBlockTrasferInProgress = true;
            try
            {
                if (disableBlockGenerators)
                {
                    from.EnableGenerators(false, true);
                    to.EnableGenerators(false, true);
                }
                Sandbox.Game.Entities.MyEntities.Remove(from);
                MyBlockGroup[] groupArray = from.BlockGroups.ToArray();
                int index = 0;
                while (true)
                {
                    if (index >= groupArray.Length)
                    {
                        from.BlockGroups.Clear();
                        from.UnregisterBlocksBeforeClose();
                        foreach (MySlimBlock block in from.m_cubeBlocks)
                        {
                            if (block.FatBlock != null)
                            {
                                from.Hierarchy.RemoveChild(block.FatBlock, false);
                            }
                            block.RemoveNeighbours();
                            block.RemoveAuthorship();
                        }
                        if (from.Physics != null)
                        {
                            from.Physics.Close();
                            from.Physics = null;
                            from.RaisePhysicsChanged();
                        }
                        foreach (MySlimBlock block2 in from.m_cubeBlocks)
                        {
                            block2.Transform(ref transform);
                            to.AddBlockInternal(block2);
                        }
                        from.Skeleton.CopyTo(to.Skeleton, transform, to);
                        if (disableBlockGenerators)
                        {
                            from.EnableGenerators(true, true);
                            to.EnableGenerators(true, true);
                        }
                        from.m_blocksForDraw.Clear();
                        from.m_cubeBlocks.Clear();
                        from.m_fatBlocks.Clear();
                        from.m_cubes.Clear();
                        from.MarkedForClose = false;
                        if (Sync.IsServer)
                        {
                            from.Close();
                        }
                        break;
                    }
                    MyBlockGroup group = groupArray[index];
                    to.AddGroup(group);
                    index++;
                }
            }
            finally
            {
                to.IsBlockTrasferInProgress = false;
                from.IsBlockTrasferInProgress = false;
            }
        }

        private static void MoveBlocksByObjectBuilders(MyCubeGrid from, MyCubeGrid to, List<MySlimBlock> cubeBlocks, int offset, int count)
        {
            from.EnableGenerators(false, true);
            to.EnableGenerators(false, true);
            try
            {
                List<MyObjectBuilder_CubeBlock> list = new List<MyObjectBuilder_CubeBlock>();
                int num = offset;
                while (true)
                {
                    if (num >= (offset + count))
                    {
                        MyEntityIdRemapHelper remapHelper = new MyEntityIdRemapHelper();
                        using (List<MyObjectBuilder_CubeBlock>.Enumerator enumerator = list.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                enumerator.Current.Remap(remapHelper);
                            }
                        }
                        int num2 = offset;
                        while (true)
                        {
                            if (num2 >= (offset + count))
                            {
                                foreach (MyObjectBuilder_CubeBlock block3 in list)
                                {
                                    to.AddBlock(block3, false);
                                }
                                break;
                            }
                            MySlimBlock block2 = cubeBlocks[num2];
                            from.RemoveBlockInternal(block2, true, false);
                            num2++;
                        }
                        break;
                    }
                    MySlimBlock block = cubeBlocks[num];
                    list.Add(block.GetObjectBuilder(true));
                    num++;
                }
            }
            finally
            {
                from.EnableGenerators(true, true);
                to.EnableGenerators(true, true);
            }
        }

        private void MoveBone(ref Vector3I cubePos, ref Vector3I boneOffset, ref Vector3 moveDirection, ref float displacementLength, ref Vector3 clamp)
        {
            this.m_totalBoneDisplacement += displacementLength;
            Vector3I vectori = (Vector3I) ((cubePos * 2) + boneOffset);
            this.Skeleton[vectori] = Vector3.Clamp(this.Skeleton[vectori] + moveDirection, -clamp, clamp);
        }

        private bool MoveCornerBones(Vector3I cubePos, Vector3I offset, ref Vector3I minCube, ref Vector3I maxCube)
        {
            Vector3I vectori = Vector3I.Abs(offset);
            Vector3I vectori2 = Vector3I.Shift(vectori);
            Vector3I vectori3 = offset * vectori2;
            Vector3I vectori4 = offset * Vector3I.Shift(vectori2);
            Vector3 gridSizeQuarterVector = this.GridSizeQuarterVector;
            bool flag1 = (this.m_cubes.ContainsKey(cubePos + offset) & this.m_cubes.ContainsKey(cubePos + vectori3)) & this.m_cubes.ContainsKey(cubePos + vectori4);
            if (flag1)
            {
                Vector3I vectori5 = Vector3I.One - vectori;
                Vector3I boneOffset = (Vector3I) (Vector3I.One + offset);
                Vector3I vectori7 = (Vector3I) (boneOffset + vectori5);
                Vector3I vectori8 = boneOffset - vectori5;
                Vector3 moveDirection = (Vector3) (-offset * 0.25f);
                if (m_precalculatedCornerBonesDisplacementDistance <= 0f)
                {
                    m_precalculatedCornerBonesDisplacementDistance = moveDirection.Length();
                }
                float displacementLength = m_precalculatedCornerBonesDisplacementDistance * this.GridSize;
                moveDirection *= this.GridSize;
                this.MoveBone(ref cubePos, ref boneOffset, ref moveDirection, ref displacementLength, ref gridSizeQuarterVector);
                this.MoveBone(ref cubePos, ref vectori7, ref moveDirection, ref displacementLength, ref gridSizeQuarterVector);
                this.MoveBone(ref cubePos, ref vectori8, ref moveDirection, ref displacementLength, ref gridSizeQuarterVector);
                minCube = Vector3I.Min(Vector3I.Min(cubePos, minCube), ((Vector3I) (cubePos + offset)) - vectori5);
                maxCube = Vector3I.Max(Vector3I.Max(cubePos, maxCube), (Vector3I) ((cubePos + offset) + vectori5));
            }
            return flag1;
        }

        public unsafe void MultiplyBlockSkeleton(MySlimBlock block, float factor, bool updateSync = false)
        {
            if (this.Skeleton == null)
            {
                MyLog.Default.WriteLine("Skeleton null in MultiplyBlockSkeleton!" + this);
            }
            if (this.Physics == null)
            {
                MyLog.Default.WriteLine("Physics null in MultiplyBlockSkeleton!" + this);
            }
            if (((block != null) && (this.Skeleton != null)) && (this.Physics != null))
            {
                Vector3I vectori3;
                Vector3I min = block.Min * 2;
                Vector3I max = (Vector3I) ((block.Max * 2) + 2);
                bool flag = false;
                vectori3.Z = min.Z;
                while (vectori3.Z <= max.Z)
                {
                    vectori3.Y = min.Y;
                    while (true)
                    {
                        if (vectori3.Y > max.Y)
                        {
                            int* numPtr3 = (int*) ref vectori3.Z;
                            numPtr3[0]++;
                            break;
                        }
                        vectori3.X = min.X;
                        while (true)
                        {
                            if (vectori3.X > max.X)
                            {
                                int* numPtr2 = (int*) ref vectori3.Y;
                                numPtr2[0]++;
                                break;
                            }
                            flag |= this.Skeleton.MultiplyBone(ref vectori3, factor, ref block.Min, this, 0.005f);
                            int* numPtr1 = (int*) ref vectori3.X;
                            numPtr1[0]++;
                        }
                    }
                }
                if (flag)
                {
                    if (Sync.IsServer & updateSync)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, float>(this, x => new Action<Vector3I, float>(x.OnBonesMultiplied), block.Position, factor, targetEndpoint);
                    }
                    min = block.Min - Vector3I.One;
                    max = (Vector3I) (block.Max + Vector3I.One);
                    vectori3.Z = min.Z;
                    while (true)
                    {
                        if (vectori3.Z > max.Z)
                        {
                            this.Physics.AddDirtyArea(min, max);
                            this.MarkForDraw();
                            break;
                        }
                        vectori3.Y = min.Y;
                        while (true)
                        {
                            if (vectori3.Y > max.Y)
                            {
                                int* numPtr6 = (int*) ref vectori3.Z;
                                numPtr6[0]++;
                                break;
                            }
                            vectori3.X = min.X;
                            while (true)
                            {
                                if (vectori3.X > max.X)
                                {
                                    int* numPtr5 = (int*) ref vectori3.Y;
                                    numPtr5[0]++;
                                    break;
                                }
                                this.m_dirtyRegion.AddCube(vectori3);
                                int* numPtr4 = (int*) ref vectori3.X;
                                numPtr4[0]++;
                            }
                        }
                    }
                }
            }
        }

        internal void NotifyBlockAdded(MySlimBlock block)
        {
            this.OnBlockAdded.InvokeIfNotNull<MySlimBlock>(block);
            if (block.FatBlock != null)
            {
                this.OnFatBlockAdded.InvokeIfNotNull<MyCubeBlock>(block.FatBlock);
            }
            this.GridSystems.OnBlockAdded(block);
        }

        internal void NotifyBlockClosed(MySlimBlock block)
        {
            this.OnBlockClosed.InvokeIfNotNull<MySlimBlock>(block);
            if (block.FatBlock != null)
            {
                this.OnFatBlockClosed.InvokeIfNotNull<MyCubeBlock>(block.FatBlock);
            }
        }

        internal void NotifyBlockIntegrityChanged(MySlimBlock block, bool handWelded)
        {
            this.OnBlockIntegrityChanged.InvokeIfNotNull<MySlimBlock>(block);
            this.GridSystems.OnBlockIntegrityChanged(block);
            if (block.IsFullIntegrity)
            {
                MyCubeGrids.NotifyBlockFinished(this, block, handWelded);
            }
        }

        internal void NotifyBlockOwnershipChange(MyCubeGrid cubeGrid)
        {
            if (this.OnBlockOwnershipChanged != null)
            {
                this.OnBlockOwnershipChanged(cubeGrid);
            }
            this.GridSystems.OnBlockOwnershipChanged(cubeGrid);
        }

        internal void NotifyBlockRemoved(MySlimBlock block)
        {
            this.OnBlockRemoved.InvokeIfNotNull<MySlimBlock>(block);
            if (block.FatBlock != null)
            {
                this.OnFatBlockRemoved.InvokeIfNotNull<MyCubeBlock>(block.FatBlock);
            }
            if (MyVisualScriptLogicProvider.BlockDestroyed != null)
            {
                MyVisualScriptLogicProvider.BlockDestroyed((block.FatBlock != null) ? block.FatBlock.Name : string.Empty, base.Name, block.BlockDefinition.Id.TypeId.ToString(), block.BlockDefinition.Id.SubtypeName);
            }
            MyCubeGrids.NotifyBlockDestroyed(this, block);
            this.GridSystems.OnBlockRemoved(block);
            this.MarkForUpdate();
        }

        internal void NotifyIsStaticChanged(bool newIsStatic)
        {
            if (this.OnIsStaticChanged != null)
            {
                this.OnIsStaticChanged(newIsStatic);
            }
            if (this.OnStaticChanged != null)
            {
                this.OnStaticChanged(this, newIsStatic);
            }
        }

        internal void NotifyMassPropertiesChanged()
        {
            this.OnMassPropertiesChanged.InvokeIfNotNull<MyCubeGrid>(this);
        }

        internal void OnAddedToGroup(MyGridLogicalGroupData group)
        {
            this.GridSystems.OnAddedToGroup(group);
            if (this.AddedToLogicalGroup != null)
            {
                this.AddedToLogicalGroup(group);
            }
        }

        internal void OnAddedToGroup(MyGridPhysicalGroupData groupData)
        {
            this.GridSystems.OnAddedToGroup(groupData);
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            MyCubeGridGroups.Static.AddNode(GridLinkTypeEnum.Logical, this);
            MyCubeGridGroups.Static.AddNode(GridLinkTypeEnum.Physical, this);
            MyCubeGridGroups.Static.AddNode(GridLinkTypeEnum.Mechanical, this);
            if (!base.IsPreview)
            {
                MyGridPhysicalHierarchy.Static.AddNode(this);
            }
            if (this.IsStatic)
            {
                MyFixedGrids.MarkGridRoot(this);
            }
            this.RecalculateGravity();
            this.UpdateGravity();
            this.MarkForUpdate();
        }

        [Event(null, 0x287e), Reliable, Broadcast]
        private void OnBonesMultiplied(Vector3I blockLocation, float multiplier)
        {
            MySlimBlock cubeBlock = this.GetCubeBlock(blockLocation);
            if (cubeBlock != null)
            {
                this.MultiplyBlockSkeleton(cubeBlock, multiplier, false);
            }
        }

        [Event(null, 0x285c), Reliable, Broadcast]
        private unsafe void OnBonesReceived(int segmentsCount, List<byte> boneByteList)
        {
            byte[] data = boneByteList.ToArray();
            int dataIndex = 0;
            int num2 = 0;
            while (num2 < segmentsCount)
            {
                Vector3I vectori;
                Vector3I vectori2;
                Vector3I vectori5;
                this.Skeleton.DeserializePart(this.GridSize, data, ref dataIndex, out vectori, out vectori2);
                Vector3I zero = Vector3I.Zero;
                Vector3I cube = Vector3I.Zero;
                this.Skeleton.Wrap(ref zero, ref vectori);
                this.Skeleton.Wrap(ref cube, ref vectori2);
                zero -= Vector3I.One;
                cube = (Vector3I) (cube + Vector3I.One);
                vectori5.X = zero.X;
                while (true)
                {
                    if (vectori5.X > cube.X)
                    {
                        num2++;
                        break;
                    }
                    vectori5.Y = zero.Y;
                    while (true)
                    {
                        if (vectori5.Y > cube.Y)
                        {
                            int* numPtr3 = (int*) ref vectori5.X;
                            numPtr3[0]++;
                            break;
                        }
                        vectori5.Z = zero.Z;
                        while (true)
                        {
                            if (vectori5.Z > cube.Z)
                            {
                                int* numPtr2 = (int*) ref vectori5.Y;
                                numPtr2[0]++;
                                break;
                            }
                            this.SetCubeDirty(vectori5);
                            int* numPtr1 = (int*) ref vectori5.Z;
                            numPtr1[0]++;
                        }
                    }
                }
            }
        }

        [Event(null, 0x29b8), Reliable, Server(ValidationType.BigOwner | ValidationType.Access), Broadcast]
        private void OnChangeDisplayNameRequest(string displayName)
        {
            base.DisplayName = displayName;
            if (this.OnNameChanged != null)
            {
                this.OnNameChanged(this);
            }
        }

        [Event(null, 0x2987), Reliable, Broadcast]
        private void OnChangeGridOwner(long playerId, MyOwnershipShareModeEnum shareMode)
        {
            foreach (MySlimBlock block in this.GetBlocks())
            {
                if (block.FatBlock == null)
                {
                    continue;
                }
                if (block.BlockDefinition.RatioEnoughForOwnership(block.BuildLevelRatio))
                {
                    block.FatBlock.ChangeOwner(playerId, shareMode);
                }
            }
        }

        [Event(null, 0x2949), Reliable, Broadcast]
        private void OnChangeOwner(long blockId, long owner, MyOwnershipShareModeEnum shareMode)
        {
            MyCubeBlock entity = null;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeBlock>(blockId, out entity, false))
            {
                entity.ChangeOwner(owner, shareMode);
            }
        }

        [Event(null, 0x292c), Reliable, Server(ValidationType.Access)]
        private void OnChangeOwnerRequest(long blockId, long owner, MyOwnershipShareModeEnum shareMode)
        {
            MyCubeBlock entity = null;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeBlock>(blockId, out entity, false))
            {
                EndpointId id;
                MyEntityOwnershipComponent component = entity.Components.Get<MyEntityOwnershipComponent>();
                if ((Sync.IsServer && (entity.IDModule != null)) && (((entity.IDModule.Owner == 0) || (entity.IDModule.Owner == owner)) || (owner == 0)))
                {
                    this.OnChangeOwner(blockId, owner, shareMode);
                    id = new EndpointId();
                    MyMultiplayer.RaiseEvent<MyCubeGrid, long, long, MyOwnershipShareModeEnum>(this, x => new Action<long, long, MyOwnershipShareModeEnum>(x.OnChangeOwner), blockId, owner, shareMode, id);
                }
                else if ((Sync.IsServer && (component != null)) && (((component.OwnerId == 0) || (component.OwnerId == owner)) || (owner == 0)))
                {
                    this.OnChangeOwner(blockId, owner, shareMode);
                    id = new EndpointId();
                    MyMultiplayer.RaiseEvent<MyCubeGrid, long, long, MyOwnershipShareModeEnum>(this, x => new Action<long, long, MyOwnershipShareModeEnum>(x.OnChangeOwner), blockId, owner, shareMode, id);
                }
                else
                {
                    bool flag = entity.BlockDefinition.ContainsComputer();
                    if (entity.UseObjectsComponent != null)
                    {
                        int num1;
                        if (!flag)
                        {
                            num1 = (int) (entity.UseObjectsComponent.GetDetectors("ownership").Count > 0);
                        }
                        else
                        {
                            num1 = 1;
                        }
                        flag = (bool) num1;
                    }
                    bool flag1 = flag;
                }
            }
        }

        [Event(null, 0x2a14), Reliable, Server(ValidationType.Access)]
        private static void OnChangeOwnersRequest(MyOwnershipShareModeEnum shareMode, List<MySingleOwnershipRequest> requests, long requestingPlayer)
        {
            MyCubeBlock entity = null;
            int index = 0;
            ulong steamId = MySession.Static.Players.TryGetSteamId(requestingPlayer);
            if (MySession.Static.IsUserSpaceMaster(steamId))
            {
                index = requests.Count;
            }
            while (index < requests.Count)
            {
                MySingleOwnershipRequest request = requests[index];
                if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeBlock>(request.BlockId, out entity, false))
                {
                    index++;
                    continue;
                }
                MyEntityOwnershipComponent component = entity.Components.Get<MyEntityOwnershipComponent>();
                if ((Sync.IsServer && (entity.IDModule != null)) && (((entity.IDModule.Owner == 0) || (entity.IDModule.Owner == requestingPlayer)) || (request.Owner == 0)))
                {
                    index++;
                    continue;
                }
                if ((!Sync.IsServer || (component == null)) || (((component.OwnerId != 0) && (component.OwnerId != requestingPlayer)) && (request.Owner != 0)))
                {
                    requests.RemoveAtFast<MySingleOwnershipRequest>(index);
                }
                else
                {
                    index++;
                }
            }
            if (requests.Count > 0)
            {
                OnChangeOwnersSuccess(shareMode, requests);
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<MyOwnershipShareModeEnum, List<MySingleOwnershipRequest>>(s => new Action<MyOwnershipShareModeEnum, List<MySingleOwnershipRequest>>(MyCubeGrid.OnChangeOwnersSuccess), shareMode, requests, targetEndpoint, position);
            }
        }

        [Event(null, 0x2a3f), Reliable, Broadcast]
        private static void OnChangeOwnersSuccess(MyOwnershipShareModeEnum shareMode, List<MySingleOwnershipRequest> requests)
        {
            foreach (MySingleOwnershipRequest request in requests)
            {
                MyCubeBlock entity = null;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeBlock>(request.BlockId, out entity, false))
                {
                    entity.ChangeOwner(request.Owner, shareMode);
                }
            }
        }

        public void OnClosedMessageBox(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (this.m_isRazeBatchDelayed)
            {
                if (base.Closed)
                {
                    this.m_delayedRazeBatch.Occupied.Clear();
                    this.m_delayedRazeBatch = null;
                    this.m_isRazeBatchDelayed = false;
                }
                else
                {
                    if (result == MyGuiScreenMessageBox.ResultEnum.NO)
                    {
                        foreach (MyCockpit cockpit in this.m_delayedRazeBatch.Occupied)
                        {
                            if (cockpit.Pilot == null)
                            {
                                continue;
                            }
                            if (!cockpit.MarkedForClose)
                            {
                                cockpit.RequestRemovePilot();
                            }
                        }
                    }
                    EndpointId targetEndpoint = new EndpointId();
                    MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, Vector3UByte, long>(this, x => new Action<Vector3I, Vector3UByte, long>(x.RazeBlocksAreaRequest), this.m_delayedRazeBatch.Pos, this.m_delayedRazeBatch.Size, MySession.Static.LocalCharacterEntityId, targetEndpoint);
                    this.m_delayedRazeBatch.Occupied.Clear();
                    this.m_delayedRazeBatch = null;
                    this.m_isRazeBatchDelayed = false;
                }
            }
        }

        [Event(null, 0x127d), Reliable, Server, Broadcast]
        private unsafe void OnColorBlock(Vector3I min, Vector3I max, Vector3 newHSV, bool playSound, long player)
        {
            if (this.ColorGridOrBlockRequestValidation(player))
            {
                Vector3I vectori;
                bool flag = false;
                vectori.X = min.X;
                while (vectori.X <= max.X)
                {
                    vectori.Y = min.Y;
                    while (true)
                    {
                        if (vectori.Y > max.Y)
                        {
                            int* numPtr3 = (int*) ref vectori.X;
                            numPtr3[0]++;
                            break;
                        }
                        vectori.Z = min.Z;
                        while (true)
                        {
                            if (vectori.Z > max.Z)
                            {
                                int* numPtr2 = (int*) ref vectori.Y;
                                numPtr2[0]++;
                                break;
                            }
                            MySlimBlock cubeBlock = this.GetCubeBlock(vectori);
                            if (cubeBlock != null)
                            {
                                flag |= this.ChangeColor(cubeBlock, newHSV);
                            }
                            int* numPtr1 = (int*) ref vectori.Z;
                            numPtr1[0]++;
                        }
                    }
                }
                if ((playSound & flag) && (Vector3D.Distance(MySector.MainCamera.Position, Vector3D.Transform((Vector3) (min * this.GridSize), base.WorldMatrix)) < 200.0))
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudColorBlock);
                }
            }
        }

        [Event(null, 0x125f), Reliable, Server, Broadcast]
        private void OnColorGridFriendly(Vector3 newHSV, bool playSound, long player)
        {
            if (this.ColorGridOrBlockRequestValidation(player))
            {
                bool flag = false;
                foreach (MySlimBlock block in this.CubeBlocks)
                {
                    flag |= this.ChangeColor(block, newHSV);
                }
                if (playSound & flag)
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudColorBlock);
                }
            }
        }

        private void OnContactPointChanged()
        {
            if (((this.Physics != null) && (!base.Closed && !base.MarkedForClose)) && !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                VRage.Game.Entity.MyEntity.ContactPointData data = base.m_contactPoint.Value;
                VRage.Game.Entity.MyEntity entity = null;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(data.EntityId, out entity, false) && (entity.Physics != null))
                {
                    Vector3D position = data.LocalPosition + base.PositionComp.WorldMatrix.Translation;
                    if ((data.ContactPointType & VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.AnySound) != VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.None)
                    {
                        MyAudioComponent.PlayContactSoundInternal(this, entity, position, data.Normal, data.SeparatingSpeed);
                    }
                    if ((data.ContactPointType & VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.AnyParticle) != VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.None)
                    {
                        this.Physics.PlayCollisionParticlesInternal(entity, ref position, ref data.Normal, ref data.SeparatingVelocity, data.SeparatingSpeed, data.Impulse, data.ContactPointType);
                    }
                }
            }
        }

        [Event(null, 0x2907), Reliable, Server(ValidationType.BigOwnerSpaceMaster | ValidationType.Access)]
        private void OnConvertedToShipRequest(MyTestDynamicReason reason)
        {
            if ((!this.IsStatic || ((this.Physics == null) || (this.BlocksCount == 0))) || ShouldBeStatic(this, reason))
            {
                MyMultiplayer.RaiseEvent<MyCubeGrid>(this, x => new Action(x.OnConvertToShipFailed), MyEventContext.Current.Sender);
            }
            else
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid>(this, x => new Action(x.OnConvertToDynamic), targetEndpoint);
            }
        }

        [Event(null, 0x291c), Reliable, Server(ValidationType.BigOwnerSpaceMaster | ValidationType.Access)]
        public void OnConvertedToStationRequest()
        {
            if (!this.IsStatic)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid>(this, x => new Action(x.ConvertToStatic), targetEndpoint);
            }
        }

        [Event(null, 0x14f3), Reliable, ServerInvoked, Broadcast]
        public void OnConvertToDynamic()
        {
            if ((MyCubeGridSmallToLargeConnection.Static != null) && this.m_enableSmallToLargeConnections)
            {
                MyCubeGridSmallToLargeConnection.Static.ConvertToDynamic(this);
            }
            this.IsStatic = false;
            this.IsUnsupportedStation = false;
            if (MyCubeGridGroups.Static != null)
            {
                MyCubeGridGroups.Static.UpdateDynamicState(this);
            }
            this.SetInventoryMassDirty();
            this.Physics.ConvertToDynamic(this.GridSizeEnum == MyCubeSize.Large, this.IsClientPredicted);
            base.RaisePhysicsChanged();
            this.Physics.RigidBody.AddGravity();
            this.RecalculateGravity();
            MyFixedGrids.UnmarkGridRoot(this);
        }

        [Event(null, 0x2914), Reliable, Client]
        private void OnConvertToShipFailed()
        {
            if (this.m_convertToShipResult != null)
            {
                this.m_convertToShipResult();
            }
            this.m_convertToShipResult = null;
        }

        [Event(null, 0x29a), Reliable, Server(ValidationType.Access)]
        private void OnGridChangedRPC()
        {
            this.RaiseGridChanged();
        }

        [Event(null, 0x2691), Reliable, Server]
        private static void OnGridClosedRequest(long entityId)
        {
            VRage.Game.Entity.MyEntity entity;
            bool flag;
            MyLog.Default.WriteLineAndConsole("Closing grid request by user: " + MyEventContext.Current.Sender.Value);
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                return;
            }
            Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false);
            if (entity == null)
            {
                return;
            }
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid == null)
            {
                goto TR_0005;
            }
            else
            {
                long playerId = MySession.Static.Players.TryGetIdentityId(MyEventContext.Current.Sender.Value, 0);
                flag = false;
                bool flag2 = false;
                IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(playerId);
                if (faction != null)
                {
                    flag2 = faction.IsLeader(playerId);
                }
                if (!MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value))
                {
                    if (grid.BigOwners.Count != 0)
                    {
                        foreach (long num2 in grid.BigOwners)
                        {
                            if (num2 == playerId)
                            {
                                flag = true;
                            }
                            else
                            {
                                if (MySession.Static.Players.TryGetIdentity(num2) == null)
                                {
                                    continue;
                                }
                                if (!flag2)
                                {
                                    continue;
                                }
                                IMyFaction faction2 = MySession.Static.Factions.TryGetPlayerFaction(num2);
                                if (faction2 == null)
                                {
                                    continue;
                                }
                                if (faction.FactionId != faction2.FactionId)
                                {
                                    continue;
                                }
                                flag = true;
                            }
                            break;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
                else
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                return;
            }
        TR_0005:
            if (!entity.MarkedForClose)
            {
                entity.Close();
            }
        }

        public void OnIntegrityChanged(MySlimBlock block, bool handWelded)
        {
            this.NotifyBlockIntegrityChanged(block, handWelded);
        }

        [Event(null, 0x2b88), Reliable, Server]
        public void OnLogHierarchy()
        {
            MyGridPhysicalHierarchy.Static.Log(MyGridPhysicalHierarchy.Static.GetRoot(this));
        }

        [Event(null, 0x29ca), Reliable, Server(ValidationType.Access), BroadcastExcept]
        private void OnModifyGroupSuccess(string name, List<long> blocks)
        {
            if ((blocks == null) || (blocks.Count == 0))
            {
                foreach (MyBlockGroup group in this.BlockGroups)
                {
                    if (group.Name.ToString().Equals(name))
                    {
                        this.RemoveGroup(group);
                        break;
                    }
                }
            }
            else
            {
                MyBlockGroup group = new MyBlockGroup();
                group.Name.Clear().Append(name);
                using (List<long>.Enumerator enumerator2 = blocks.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        MyTerminalBlock entity = null;
                        if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyTerminalBlock>(enumerator2.Current, out entity, false))
                        {
                            group.Blocks.Add(entity);
                        }
                    }
                }
                this.AddGroup(group);
            }
        }

        private static void OnPasteCompleted(WorkData workData)
        {
            PasteGridData data = workData as PasteGridData;
            if (data == null)
            {
                workData.FlagAsFailed();
            }
            else
            {
                data.Callback();
            }
        }

        [Event(null, 0x28f5), Reliable, Server, Broadcast]
        private void OnPowerProducerStateRequest(MyMultipleEnabledEnum enabledState, long playerId)
        {
            this.GridSystems.SyncObject_PowerProducerStateChanged(enabledState, playerId);
        }

        private void OnRazeBlockInCompoundBlock(List<LocationIdentity> locationsAndIds)
        {
            m_tmpLocationsAndIdsReceive.Clear();
            this.RazeBlockInCompoundBlockSuccess(locationsAndIds, m_tmpLocationsAndIdsReceive);
        }

        [Event(null, 0x29ec), Reliable, Server]
        private void OnRazeBlockInCompoundBlockRequest(List<LocationIdentity> locationsAndIds)
        {
            this.OnRazeBlockInCompoundBlock(locationsAndIds);
            if (m_tmpLocationsAndIdsReceive.Count > 0)
            {
                ConvertToLocationIdentityList(m_tmpLocationsAndIdsReceive, m_tmpLocationsAndIdsSend);
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, List<LocationIdentity>>(this, x => new Action<List<LocationIdentity>>(x.OnRazeBlockInCompoundBlockSuccess), m_tmpLocationsAndIdsSend, targetEndpoint);
            }
        }

        [Event(null, 0x29f9), Reliable, Broadcast]
        private void OnRazeBlockInCompoundBlockSuccess(List<LocationIdentity> locationsAndIds)
        {
            this.OnRazeBlockInCompoundBlock(locationsAndIds);
        }

        internal void OnRemovedFromGroup(MyGridLogicalGroupData group)
        {
            this.GridSystems.OnRemovedFromGroup(group);
            if (this.RemovedFromLogicalGroup != null)
            {
                this.RemovedFromLogicalGroup();
            }
        }

        internal void OnRemovedFromGroup(MyGridPhysicalGroupData group)
        {
            this.GridSystems.OnRemovedFromGroup(group);
        }

        public override void OnRemovedFromScene(object source)
        {
            base.OnRemovedFromScene(source);
            if (!Sandbox.Game.Entities.MyEntities.IsClosingAll)
            {
                MyCubeGridGroups.Static.RemoveNode(GridLinkTypeEnum.Physical, this);
                MyCubeGridGroups.Static.RemoveNode(GridLinkTypeEnum.Logical, this);
                MyCubeGridGroups.Static.RemoveNode(GridLinkTypeEnum.Mechanical, this);
            }
            if (!base.IsPreview)
            {
                MyGridPhysicalHierarchy.Static.RemoveNode(this);
            }
            MyFixedGrids.UnmarkGridRoot(this);
            this.ReleaseMerginGrids();
            if (this.m_unsafeBlocks.Count > 0)
            {
                MyUnsafeGridsSessionComponent.UnregisterGrid(this);
            }
        }

        [Event(null, 0x299d), Reliable, Broadcast]
        private void OnRemoveSplit(List<Vector3I> removedBlocks)
        {
            m_tmpPositionListReceive.Clear();
            foreach (Vector3I vectori in removedBlocks)
            {
                MySlimBlock cubeBlock = this.GetCubeBlock(vectori);
                if (cubeBlock == null)
                {
                    MySandboxGame.Log.WriteLine("Block was null when trying to remove a grid split. Desync?");
                    continue;
                }
                m_tmpBlockListReceive.Add(cubeBlock);
            }
            RemoveSplit(this, m_tmpBlockListReceive, 0, m_tmpBlockListReceive.Count, false);
            m_tmpBlockListReceive.Clear();
        }

        [Event(null, 0x28d6), Reliable, Server(ValidationType.Access)]
        private void OnSetToConstructionRequest(Vector3I blockPosition, long ownerEntityId, byte inventoryIndex, long requestingPlayer)
        {
            MySlimBlock cubeBlock = this.GetCubeBlock(blockPosition);
            if (cubeBlock != null)
            {
                cubeBlock.SetToConstructionSite();
                VRage.Game.Entity.MyEntity entity = null;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(ownerEntityId, out entity, false))
                {
                    VRage.Game.Entity.MyEntity entity1;
                    if ((entity == null) || !entity.HasInventory)
                    {
                        entity1 = null;
                    }
                    else
                    {
                        entity1 = entity;
                    }
                    cubeBlock.MoveItemsToConstructionStockpile(entity1.GetInventory(inventoryIndex));
                    cubeBlock.IncreaseMountLevel(MyWelder.WELDER_AMOUNT_PER_SECOND * 0.01666667f, requestingPlayer, null, 0f, false, MyOwnershipShareModeEnum.Faction, false);
                }
            }
        }

        [Event(null, 0x28ba), Reliable, Server(ValidationType.Access)]
        private void OnStockpileFillRequest(Vector3I blockPosition, long ownerEntityId, byte inventoryIndex)
        {
            MySlimBlock cubeBlock = this.GetCubeBlock(blockPosition);
            if (cubeBlock != null)
            {
                VRage.Game.Entity.MyEntity entity = null;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(ownerEntityId, out entity, false))
                {
                    VRage.Game.Entity.MyEntity entity1;
                    if ((entity == null) || !entity.HasInventory)
                    {
                        entity1 = null;
                    }
                    else
                    {
                        entity1 = entity;
                    }
                    cubeBlock.MoveItemsToConstructionStockpile(entity1.GetInventory(inventoryIndex));
                }
            }
        }

        public void OnTerinalOpened()
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid>(this, x => new Action(x.OnGridChangedRPC), targetEndpoint);
        }

        private void OnUpdateDirtyCompleted()
        {
            if (base.InScene)
            {
                this.UpdateInstanceData();
            }
            this.m_dirtyRegionParallel.Clear();
            this.m_updatingDirty = false;
            this.MarkForDraw();
            this.ReleaseMerginGrids();
        }

        public static void PackFiles(string path, string objectName)
        {
            if (!Directory.Exists(path))
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.ExportToObjFailed), path)), null, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
            else
            {
                using (MyZipArchive archive = MyZipArchive.OpenOnFile(Path.Combine(path, objectName + "_objFiles.zip"), FileMode.Create, FileAccess.ReadWrite, FileShare.None, false))
                {
                    PackFilesToDirectory(path, "*.png", archive);
                    PackFilesToDirectory(path, "*.obj", archive);
                    PackFilesToDirectory(path, "*.mtl", archive);
                }
                using (MyZipArchive archive2 = MyZipArchive.OpenOnFile(Path.Combine(path, objectName + ".zip"), FileMode.Create, FileAccess.ReadWrite, FileShare.None, false))
                {
                    PackFilesToDirectory(path, objectName + ".png", archive2);
                    PackFilesToDirectory(path, "*.sbc", archive2);
                }
                RemoveFilesFromDirectory(path, "*.png");
                RemoveFilesFromDirectory(path, "*.sbc");
                RemoveFilesFromDirectory(path, "*.obj");
                RemoveFilesFromDirectory(path, "*.mtl");
            }
        }

        private static void PackFilesToDirectory(string path, string searchString, MyZipArchive arc)
        {
            int startIndex = path.Length + 1;
            foreach (string str in Directory.GetFiles(path, searchString, SearchOption.AllDirectories))
            {
                using (FileStream stream = File.Open(str, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (Stream stream2 = arc.AddFile(str.Substring(startIndex), CompressionMethodEnum.Deflated, DeflateOptionEnum.Maximum).GetStream(FileMode.Open, FileAccess.Write))
                    {
                        stream.CopyTo(stream2, 0x1000);
                    }
                }
            }
        }

        private void PasteBlocksClient(MyObjectBuilder_CubeGrid gridToMerge, MatrixI mergeTransform)
        {
            MyCubeGrid entity = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilder(gridToMerge, false) as MyCubeGrid;
            if (entity != null)
            {
                Sandbox.Game.Entities.MyEntities.Add(entity, true);
                this.MergeGridInternal(entity, ref mergeTransform, true);
            }
        }

        private MatrixI PasteBlocksServer(List<MyObjectBuilder_CubeGrid> gridsToMerge)
        {
            MyCubeGrid gridToMerge = null;
            using (List<MyObjectBuilder_CubeGrid>.Enumerator enumerator = gridsToMerge.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyCubeGrid entity = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilder(enumerator.Current, false) as MyCubeGrid;
                    if (entity != null)
                    {
                        if (gridToMerge == null)
                        {
                            gridToMerge = entity;
                        }
                        Sandbox.Game.Entities.MyEntities.Add(entity, true);
                    }
                }
            }
            MatrixI transform = this.CalculateMergeTransform(gridToMerge, this.WorldToGridInteger(gridToMerge.PositionComp.GetPosition()));
            this.MergeGridInternal(gridToMerge, ref transform, false);
            return transform;
        }

        public void PasteBlocksToGrid(List<MyObjectBuilder_CubeGrid> gridsToMerge, long inventoryEntityId, bool multiBlock, bool instantBuild)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, List<MyObjectBuilder_CubeGrid>, long, bool, bool>(this, x => new Action<List<MyObjectBuilder_CubeGrid>, long, bool, bool>(x.PasteBlocksToGridServer_Implementation), gridsToMerge, inventoryEntityId, multiBlock, instantBuild, targetEndpoint);
        }

        [Event(null, 0x24a0), Reliable, Broadcast]
        private void PasteBlocksToGridClient_Implementation(MyObjectBuilder_CubeGrid gridToMerge, MatrixI mergeTransform)
        {
            this.PasteBlocksClient(gridToMerge, mergeTransform);
        }

        [Event(null, 0x2474), Reliable, Server]
        private void PasteBlocksToGridServer_Implementation(List<MyObjectBuilder_CubeGrid> gridsToMerge, long inventoryEntityId, bool multiBlock, bool instantBuild)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                VRage.Game.Entity.MyEntity entity;
                Sandbox.Game.Entities.MyEntities.RemapObjectBuilderCollection((IEnumerable<MyObjectBuilder_EntityBase>) gridsToMerge);
                MatrixI xi = this.PasteBlocksServer(gridsToMerge);
                if ((!((MyEventContext.Current.IsLocallyInvoked || MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value)) & instantBuild) && Sandbox.Game.Entities.MyEntities.TryGetEntityById(inventoryEntityId, out entity, false)) && (entity != null))
                {
                    MyInventoryBase builderInventory = MyCubeBuilder.BuildComponent.GetBuilderInventory(entity);
                    if (builderInventory != null)
                    {
                        if (multiBlock)
                        {
                            MyMultiBlockClipboard.TakeMaterialsFromBuilder(gridsToMerge, entity);
                        }
                        else
                        {
                            MyGridClipboard.CalculateItemRequirements(gridsToMerge, m_buildComponents);
                            foreach (KeyValuePair<MyDefinitionId, int> pair in m_buildComponents.TotalMaterials)
                            {
                                builderInventory.RemoveItemsOfType(pair.Value, pair.Key, MyItemFlags.None, false);
                            }
                        }
                    }
                }
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, MyObjectBuilder_CubeGrid, MatrixI>(this, x => new Action<MyObjectBuilder_CubeGrid, MatrixI>(x.PasteBlocksToGridClient_Implementation), gridsToMerge[0], xi, targetEndpoint);
                MyReplicationServer replicationServer = MyMultiplayer.GetReplicationServer();
                if (replicationServer != null)
                {
                    replicationServer.ResendMissingReplicableChildren(this);
                }
            }
        }

        internal void PerformCutouts(List<MyGridPhysics.ExplosionInfo> explosions)
        {
            if (explosions.Count != 0)
            {
                BoundingSphereD sphere = new BoundingSphereD(explosions[0].Position, (double) explosions[0].Radius);
                for (int i = 0; i < explosions.Count; i++)
                {
                    sphere.Include(new BoundingSphereD(explosions[i].Position, (double) explosions[i].Radius));
                }
                using (MyUtils.ReuseCollection<MyVoxelBase>(ref m_rootVoxelsToCutTmp))
                {
                    using (MyUtils.ReuseCollection<MyVoxelBase>(ref m_overlappingVoxelsTmp))
                    {
                        MySession.Static.VoxelMaps.GetAllOverlappingWithSphere(ref sphere, m_overlappingVoxelsTmp);
                        foreach (MyVoxelBase base2 in m_overlappingVoxelsTmp)
                        {
                            m_rootVoxelsToCutTmp.Add(base2.RootVoxel);
                        }
                        int skipCount = 0;
                        Parallel.For(0, explosions.Count, delegate (int i) {
                            MyGridPhysics.ExplosionInfo info = explosions[i];
                            BoundingSphereD ed = new BoundingSphereD(info.Position, (double) info.Radius);
                            for (int j = 0; j < explosions.Count; j++)
                            {
                                if (j != i)
                                {
                                    BoundingSphereD ed2 = new BoundingSphereD(explosions[j].Position, (double) explosions[j].Radius);
                                    if (ed2.Contains(ed) == ContainmentType.Contains)
                                    {
                                        int num2 = skipCount;
                                        skipCount = num2 + 1;
                                        return;
                                    }
                                }
                            }
                            foreach (MyVoxelBase base2 in m_rootVoxelsToCutTmp)
                            {
                                Vector3I vectori;
                                Vector3I vectori2;
                                if (MyVoxelGenerator.CutOutSphereFast(base2, ref info.Position, info.Radius, out vectori, out vectori2, false))
                                {
                                    EndpointId targetEndpoint = new EndpointId();
                                    MyMultiplayer.RaiseEvent<MyVoxelBase, Vector3D, float, bool>(base2, x => new Action<Vector3D, float, bool>(x.PerformCutOutSphereFast), info.Position, info.Radius, true, targetEndpoint);
                                    m_notificationQueue.Enqueue(MyTuple.Create<int, MyVoxelBase, Vector3I, Vector3I>(i, base2, vectori, vectori2));
                                }
                            }
                        }, 1, WorkPriority.VeryHigh, new WorkOptions?(Parallel.DefaultOptions.WithDebugInfo(MyProfiler.TaskType.Voxels, "CutOutVoxel")), true);
                    }
                }
                bool flag = false;
                BoundingBoxD xd = BoundingBoxD.CreateInvalid();
                foreach (MyTuple<int, MyVoxelBase, Vector3I, Vector3I> tuple in m_notificationQueue)
                {
                    flag = true;
                    MyGridPhysics.ExplosionInfo info = explosions[tuple.Item1];
                    xd.Include(new BoundingSphereD(info.Position, (double) info.Radius));
                    Vector3I voxelRangeMin = tuple.Item3;
                    Vector3I voxelRangeMax = tuple.Item4;
                    tuple.Item2.RootVoxel.Storage.NotifyRangeChanged(ref voxelRangeMin, ref voxelRangeMax, MyStorageDataTypeFlags.Content);
                }
                if (flag)
                {
                    MyTuple<int, MyVoxelBase, Vector3I, Vector3I> tuple2;
                    MyShapeBox box = new MyShapeBox {
                        Boundaries = xd
                    };
                    while (m_notificationQueue.TryDequeue(out tuple2))
                    {
                        BoundingBoxD worldBoundaries = box.GetWorldBoundaries();
                        MyVoxelGenerator.NotifyVoxelChanged(MyVoxelBase.OperationType.Cut, tuple2.Item2, ref worldBoundaries);
                    }
                }
            }
        }

        public static void PlacePrefabsToWorld()
        {
            m_newPositionForPlacedObject = MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition();
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.NONE_TIMEOUT, new StringBuilder(MyTexts.GetString(MyCommonTexts.PlacingObjectsToScene)), null, okButtonText, okButtonText, okButtonText, okButtonText, result => StartConverting(true), 0x3e8, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        public static void PlacePrefabToWorld(MyObjectBuilder_CubeGrid[] currentPrefab, Vector3D position, List<MyCubeGrid> createdGrids = null)
        {
            Vector3D zero = Vector3D.Zero;
            Vector3D vectord2 = Vector3D.Zero;
            bool flag = true;
            Sandbox.Game.Entities.MyEntities.RemapObjectBuilderCollection(currentPrefab);
            foreach (MyObjectBuilder_CubeGrid grid in currentPrefab)
            {
                if (grid.PositionAndOrientation != null)
                {
                    if (!flag)
                    {
                        zero = ((Vector3D) grid.PositionAndOrientation.Value.Position) + vectord2;
                    }
                    else
                    {
                        vectord2 = position - grid.PositionAndOrientation.Value.Position;
                        flag = false;
                        zero = position;
                    }
                }
                MyPositionAndOrientation orientation = grid.PositionAndOrientation.Value;
                orientation.Position = zero;
                grid.PositionAndOrientation = new MyPositionAndOrientation?(orientation);
                MyCubeGrid item = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilder(grid, false) as MyCubeGrid;
                if (item != null)
                {
                    item.ClearSymmetries();
                    item.Physics.LinearVelocity = (Vector3) Vector3D.Zero;
                    item.Physics.AngularVelocity = (Vector3) Vector3D.Zero;
                    if (createdGrids != null)
                    {
                        createdGrids.Add(item);
                    }
                    Sandbox.Game.Entities.MyEntities.Add(item, true);
                }
            }
        }

        public static void Preload()
        {
        }

        public override void PrepareForDraw()
        {
            base.PrepareForDraw();
            this.GridSystems.PrepareForDraw();
            if (this.IsGizmoDrawingEnabled())
            {
                foreach (MySlimBlock block in this.m_cubeBlocks)
                {
                    if (block.FatBlock is IMyGizmoDrawableObject)
                    {
                        DrawObjectGizmo(block);
                    }
                }
            }
            if (!this.NeedsPerFrameDraw)
            {
                this.Render.NeedsDraw = false;
            }
        }

        public void PrepareMultiBlockInfos()
        {
            foreach (MySlimBlock block in this.GetBlocks())
            {
                this.AddMultiBlockInfo(block);
            }
        }

        private static void ProcessChildrens(List<Vector3> vertices, List<TriangleWithMaterial> triangles, List<Vector2> uvs, List<MyExportModel.Material> materials, ref int currVerticesCount, Matrix parentMatrix, Vector3 HSV, ListReader<MyHierarchyComponentBase> childrens)
        {
            using (List<MyHierarchyComponentBase>.Enumerator enumerator = childrens.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    VRage.ModAPI.IMyEntity entity = enumerator.Current.Container.Entity;
                    MyModel model = (entity as VRage.Game.Entity.MyEntity).Model;
                    if (model != null)
                    {
                        ExtractModelDataForObj(model, entity.LocalMatrix * parentMatrix, vertices, triangles, uvs, ref Vector2.Zero, materials, ref currVerticesCount, HSV);
                    }
                    ProcessChildrens(vertices, triangles, uvs, materials, ref currVerticesCount, entity.LocalMatrix * parentMatrix, HSV, entity.Hierarchy.Children);
                }
            }
        }

        private unsafe void QueryAABB(BoundingBoxD box, List<VRage.Game.Entity.MyEntity> blocks)
        {
            if ((blocks != null) && (base.PositionComp != null))
            {
                if (box.Contains(base.PositionComp.WorldAABB) == ContainmentType.Contains)
                {
                    foreach (MyCubeBlock block in this.m_fatBlocks)
                    {
                        if (block.Closed)
                        {
                            continue;
                        }
                        blocks.Add(block);
                        if (block.Hierarchy != null)
                        {
                            foreach (MyHierarchyComponentBase base2 in block.Hierarchy.Children)
                            {
                                if (base2.Container != null)
                                {
                                    blocks.Add((VRage.Game.Entity.MyEntity) base2.Container.Entity);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MyOrientedBoundingBoxD xd = MyOrientedBoundingBoxD.Create(box, base.PositionComp.WorldMatrixNormalizedInv);
                    Vector3D* vectordPtr1 = (Vector3D*) ref xd.Center;
                    vectordPtr1[0] *= this.GridSizeR;
                    Vector3D* vectordPtr2 = (Vector3D*) ref xd.HalfExtent;
                    vectordPtr2[0] *= this.GridSizeR;
                    box = box.TransformFast(base.PositionComp.WorldMatrixNormalizedInv);
                    Vector3D min = box.Min;
                    Vector3D max = box.Max;
                    Vector3I vectori = new Vector3I((int) Math.Round((double) (max.X * this.GridSizeR)), (int) Math.Round((double) (max.Y * this.GridSizeR)), (int) Math.Round((double) (max.Z * this.GridSizeR)));
                    Vector3I vectori1 = new Vector3I((int) Math.Round((double) (min.X * this.GridSizeR)), (int) Math.Round((double) (min.Y * this.GridSizeR)), (int) Math.Round((double) (min.Z * this.GridSizeR)));
                    Vector3I start = Vector3I.Max(Vector3I.Min(vectori1, vectori), this.Min);
                    Vector3I end = Vector3I.Min(Vector3I.Max(vectori1, vectori), this.Max);
                    if (((start.X <= end.X) && (start.Y <= end.Y)) && (start.Z <= end.Z))
                    {
                        Vector3 vector = new Vector3(0.5f);
                        BoundingBoxD xd2 = new BoundingBoxD();
                        if ((end - start).Size > this.m_cubeBlocks.Count)
                        {
                            foreach (MyCubeBlock block2 in this.m_fatBlocks)
                            {
                                if (block2.Closed)
                                {
                                    continue;
                                }
                                xd2.Min = (Vector3D) (block2.Min - vector);
                                xd2.Max = block2.Max + vector;
                                if (xd.Intersects(ref xd2))
                                {
                                    blocks.Add(block2);
                                    if (block2.Hierarchy != null)
                                    {
                                        foreach (MyHierarchyComponentBase base3 in block2.Hierarchy.Children)
                                        {
                                            if (base3.Container != null)
                                            {
                                                blocks.Add((VRage.Game.Entity.MyEntity) base3.Container.Entity);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref end);
                            Vector3I current = iterator.Current;
                            if (m_tmpQueryCubeBlocks == null)
                            {
                                m_tmpQueryCubeBlocks = new HashSet<VRage.Game.Entity.MyEntity>();
                            }
                            while (iterator.IsValid())
                            {
                                MyCube cube;
                                if (((this.m_cubes != null) && this.m_cubes.TryGetValue(current, out cube)) && (cube.CubeBlock.FatBlock != null))
                                {
                                    MyCubeBlock fatBlock = cube.CubeBlock.FatBlock;
                                    if (!m_tmpQueryCubeBlocks.Contains(fatBlock))
                                    {
                                        xd2.Min = (Vector3D) (cube.CubeBlock.Min - vector);
                                        xd2.Max = cube.CubeBlock.Max + vector;
                                        if (xd.Intersects(ref xd2))
                                        {
                                            m_tmpQueryCubeBlocks.Add(fatBlock);
                                            blocks.Add(fatBlock);
                                            if (fatBlock.Hierarchy != null)
                                            {
                                                foreach (MyHierarchyComponentBase base4 in fatBlock.Hierarchy.Children)
                                                {
                                                    if (base4.Container != null)
                                                    {
                                                        blocks.Add((VRage.Game.Entity.MyEntity) base4.Container.Entity);
                                                        m_tmpQueryCubeBlocks.Add(fatBlock);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                iterator.GetNext(out current);
                            }
                            m_tmpQueryCubeBlocks.Clear();
                        }
                    }
                }
            }
        }

        private void QueryLine(LineD line, List<MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>> blocks)
        {
            Vector3D vectord;
            Vector3D vectord2;
            MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity> item = new MyLineSegmentOverlapResult<VRage.Game.Entity.MyEntity>();
            BoundingBoxD box = new BoundingBoxD();
            MatrixD worldMatrixNormalizedInv = base.PositionComp.WorldMatrixNormalizedInv;
            Vector3D.Transform(ref line.From, ref worldMatrixNormalizedInv, out vectord);
            Vector3D.Transform(ref line.To, ref worldMatrixNormalizedInv, out vectord2);
            RayD yd = new RayD(vectord, Vector3D.Normalize(vectord2 - vectord));
            Vector3I? gridSizeInflate = null;
            this.RayCastCells(line.From, line.To, m_cacheRayCastCells, gridSizeInflate, false, true);
            foreach (Vector3I vectori in m_cacheRayCastCells)
            {
                MyCube cube;
                if (!this.m_cubes.TryGetValue(vectori, out cube))
                {
                    continue;
                }
                if (cube.CubeBlock.FatBlock != null)
                {
                    MyCubeBlock fatBlock = cube.CubeBlock.FatBlock;
                    item.Element = fatBlock;
                    box.Min = (Vector3D) ((fatBlock.Min * this.GridSize) - this.GridSizeHalfVector);
                    box.Max = (fatBlock.Max * this.GridSize) + this.GridSizeHalfVector;
                    double? nullable2 = yd.Intersects(box);
                    if (nullable2 != null)
                    {
                        item.Distance = nullable2.Value;
                        blocks.Add(item);
                    }
                }
            }
        }

        private void QuerySphere(BoundingSphereD sphere, List<VRage.Game.Entity.MyEntity> blocks)
        {
            if (base.PositionComp != null)
            {
                List<MyHierarchyComponentBase>.Enumerator enumerator2;
                if (base.Closed)
                {
                    MyLog.Default.WriteLine("Grid was Closed in MyCubeGrid.QuerySphere!");
                }
                if (sphere.Contains(base.PositionComp.WorldVolume) == ContainmentType.Contains)
                {
                    foreach (MyCubeBlock block in this.m_fatBlocks)
                    {
                        if (!block.Closed)
                        {
                            blocks.Add(block);
                            using (enumerator2 = block.Hierarchy.Children.GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    VRage.Game.Entity.MyEntity item = (VRage.Game.Entity.MyEntity) enumerator2.Current.Entity;
                                    if (item != null)
                                    {
                                        blocks.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    BoundingBoxD xd = new BoundingBoxD(sphere.Center - new Vector3D(sphere.Radius), sphere.Center + new Vector3D(sphere.Radius));
                    xd = xd.TransformFast(base.PositionComp.WorldMatrixNormalizedInv);
                    Vector3D min = xd.Min;
                    Vector3D max = xd.Max;
                    Vector3I vectori = new Vector3I((int) Math.Round((double) (max.X * this.GridSizeR)), (int) Math.Round((double) (max.Y * this.GridSizeR)), (int) Math.Round((double) (max.Z * this.GridSizeR)));
                    Vector3I vectori1 = new Vector3I((int) Math.Round((double) (min.X * this.GridSizeR)), (int) Math.Round((double) (min.Y * this.GridSizeR)), (int) Math.Round((double) (min.Z * this.GridSizeR)));
                    Vector3I start = Vector3I.Max(Vector3I.Min(vectori1, vectori), this.Min);
                    Vector3I end = Vector3I.Min(Vector3I.Max(vectori1, vectori), this.Max);
                    if (((start.X <= end.X) && (start.Y <= end.Y)) && (start.Z <= end.Z))
                    {
                        Vector3 vector = new Vector3(0.5f);
                        BoundingBox box = new BoundingBox();
                        BoundingSphere sphere2 = new BoundingSphere((Vector3) (xd.Center * this.GridSizeR), ((float) sphere.Radius) * this.GridSizeR);
                        if ((end - start).Size > this.m_cubeBlocks.Count)
                        {
                            foreach (MyCubeBlock block2 in this.m_fatBlocks)
                            {
                                if (block2.Closed)
                                {
                                    continue;
                                }
                                box.Min = ((Vector3) block2.Min) - vector;
                                box.Max = block2.Max + vector;
                                if (sphere2.Intersects(box))
                                {
                                    blocks.Add(block2);
                                    using (enumerator2 = block2.Hierarchy.Children.GetEnumerator())
                                    {
                                        while (enumerator2.MoveNext())
                                        {
                                            VRage.Game.Entity.MyEntity entity = (VRage.Game.Entity.MyEntity) enumerator2.Current.Entity;
                                            if (entity != null)
                                            {
                                                blocks.Add(entity);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (m_tmpQueryCubeBlocks == null)
                            {
                                m_tmpQueryCubeBlocks = new HashSet<VRage.Game.Entity.MyEntity>();
                            }
                            if (this.m_cubes == null)
                            {
                                MyLog.Default.WriteLine("m_cubes null in MyCubeGrid.QuerySphere!");
                            }
                            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref end);
                            Vector3I current = iterator.Current;
                            while (iterator.IsValid())
                            {
                                MyCube cube;
                                if ((this.m_cubes.TryGetValue(current, out cube) && ((cube.CubeBlock.FatBlock != null) && ((cube.CubeBlock.FatBlock != null) && !cube.CubeBlock.FatBlock.Closed))) && !m_tmpQueryCubeBlocks.Contains(cube.CubeBlock.FatBlock))
                                {
                                    box.Min = ((Vector3) cube.CubeBlock.Min) - vector;
                                    box.Max = cube.CubeBlock.Max + vector;
                                    if (sphere2.Intersects(box))
                                    {
                                        blocks.Add(cube.CubeBlock.FatBlock);
                                        m_tmpQueryCubeBlocks.Add(cube.CubeBlock.FatBlock);
                                        using (enumerator2 = cube.CubeBlock.FatBlock.Hierarchy.Children.GetEnumerator())
                                        {
                                            while (enumerator2.MoveNext())
                                            {
                                                VRage.Game.Entity.MyEntity entity = (VRage.Game.Entity.MyEntity) enumerator2.Current.Entity;
                                                if (entity != null)
                                                {
                                                    blocks.Add(entity);
                                                    m_tmpQueryCubeBlocks.Add(entity);
                                                }
                                            }
                                        }
                                    }
                                }
                                iterator.GetNext(out current);
                            }
                            m_tmpQueryCubeBlocks.Clear();
                        }
                    }
                }
            }
        }

        public void RaiseGridChanged()
        {
            this.OnGridChanged.InvokeIfNotNull<MyCubeGrid>(this);
        }

        public Vector3I? RayCastBlocks(Vector3D worldStart, Vector3D worldEnd)
        {
            Vector3I? gridSizeInflate = null;
            this.RayCastCells(worldStart, worldEnd, m_cacheRayCastCells, gridSizeInflate, false, true);
            using (List<Vector3I>.Enumerator enumerator = m_cacheRayCastCells.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    Vector3I current = enumerator.Current;
                    if (this.m_cubes.ContainsKey(current))
                    {
                        return new Vector3I?(current);
                    }
                }
            }
            return null;
        }

        internal HashSet<MyCube> RayCastBlocksAll(Vector3D worldStart, Vector3D worldEnd)
        {
            Vector3I? gridSizeInflate = null;
            this.RayCastCells(worldStart, worldEnd, m_cacheRayCastCells, gridSizeInflate, false, true);
            HashSet<MyCube> set = new HashSet<MyCube>();
            foreach (Vector3I vectori in m_cacheRayCastCells)
            {
                if (this.m_cubes.ContainsKey(vectori))
                {
                    set.Add(this.m_cubes[vectori]);
                }
            }
            return set;
        }

        internal List<MyCube> RayCastBlocksAllOrdered(Vector3D worldStart, Vector3D worldEnd)
        {
            Vector3I? gridSizeInflate = null;
            this.RayCastCells(worldStart, worldEnd, m_cacheRayCastCells, gridSizeInflate, false, true);
            List<MyCube> list = new List<MyCube>();
            foreach (Vector3I vectori in m_cacheRayCastCells)
            {
                if (!this.m_cubes.ContainsKey(vectori))
                {
                    continue;
                }
                if (!list.Contains(this.m_cubes[vectori]))
                {
                    list.Add(this.m_cubes[vectori]);
                }
            }
            return list;
        }

        public void RayCastCells(Vector3D worldStart, Vector3D worldEnd, List<Vector3I> outHitPositions, Vector3I? gridSizeInflate = new Vector3I?(), bool havokWorld = false, bool clearOutHitPositions = true)
        {
            Vector3D vectord;
            Vector3D vectord2;
            MatrixD worldMatrixNormalizedInv = base.PositionComp.WorldMatrixNormalizedInv;
            Vector3D.Transform(ref worldStart, ref worldMatrixNormalizedInv, out vectord);
            Vector3D.Transform(ref worldEnd, ref worldMatrixNormalizedInv, out vectord2);
            Vector3 gridSizeHalfVector = this.GridSizeHalfVector;
            vectord += gridSizeHalfVector;
            vectord2 += gridSizeHalfVector;
            Vector3I min = this.Min - Vector3I.One;
            Vector3I max = (Vector3I) (this.Max + Vector3I.One);
            if (gridSizeInflate != null)
            {
                min -= gridSizeInflate.Value;
                max = (Vector3I) (max + gridSizeInflate.Value);
            }
            if (clearOutHitPositions)
            {
                outHitPositions.Clear();
            }
            MyGridIntersection.Calculate(outHitPositions, this.GridSize, vectord, vectord2, min, max);
        }

        public static void RayCastStaticCells(Vector3D worldStart, Vector3D worldEnd, List<Vector3I> outHitPositions, float gridSize, Vector3I? gridSizeInflate = new Vector3I?(), bool havokWorld = false)
        {
            Vector3D lineStart = worldStart;
            Vector3D vectord3 = new Vector3D((double) (gridSize * 0.5f));
            lineStart += vectord3;
            Vector3D lineEnd = worldEnd + vectord3;
            Vector3I min = -Vector3I.One;
            Vector3I one = Vector3I.One;
            if (gridSizeInflate != null)
            {
                min -= gridSizeInflate.Value;
                one = (Vector3I) (one + gridSizeInflate.Value);
            }
            outHitPositions.Clear();
            if (havokWorld)
            {
                MyGridIntersection.CalculateHavok(outHitPositions, gridSize, lineStart, lineEnd, min, one);
            }
            else
            {
                MyGridIntersection.Calculate(outHitPositions, gridSize, lineStart, lineEnd, min, one);
            }
        }

        public void RazeBlock(Vector3I position)
        {
            m_tmpPositionListSend.Clear();
            m_tmpPositionListSend.Add(position);
            this.RazeBlocks(m_tmpPositionListSend, 0L);
        }

        public void RazeBlockInCompoundBlock(List<Tuple<Vector3I, ushort>> locationsAndIds)
        {
            ConvertToLocationIdentityList(locationsAndIds, m_tmpLocationsAndIdsSend);
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, List<LocationIdentity>>(this, x => new Action<List<LocationIdentity>>(x.OnRazeBlockInCompoundBlockRequest), m_tmpLocationsAndIdsSend, targetEndpoint);
        }

        private void RazeBlockInCompoundBlockSuccess(List<LocationIdentity> locationsAndIds, List<Tuple<Vector3I, ushort>> removedBlocks)
        {
            Vector3I maxValue = Vector3I.MaxValue;
            Vector3I minValue = Vector3I.MinValue;
            foreach (LocationIdentity identity in locationsAndIds)
            {
                this.RemoveBlockInCompound(identity.Location, identity.Id, ref maxValue, ref minValue, removedBlocks);
            }
            this.m_dirtyRegion.AddCubeRegion(maxValue, minValue);
            if (this.Physics != null)
            {
                this.Physics.AddDirtyArea(maxValue, minValue);
            }
            this.MarkForDraw();
        }

        public void RazeBlocks(List<Vector3I> locations, long builderEntityId = 0L)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, List<Vector3I>, long>(this, x => new Action<List<Vector3I>, long>(x.RazeBlocksRequest), locations, builderEntityId, targetEndpoint);
        }

        public void RazeBlocks(ref Vector3I pos, ref Vector3UByte size, long builderEntityId = 0L)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, Vector3UByte, long>(this, x => new Action<Vector3I, Vector3UByte, long>(x.RazeBlocksAreaRequest), pos, size, builderEntityId, targetEndpoint);
        }

        [Event(null, 0x10c3), Reliable, Server]
        private unsafe void RazeBlocksAreaRequest(Vector3I pos, Vector3UByte size, long builderEntityId)
        {
            if ((!MySession.Static.CreativeMode && !MyEventContext.Current.IsLocallyInvoked) && !MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                try
                {
                    Vector3UByte num;
                    num.X = 0;
                    while (true)
                    {
                        if (num.X > size.X)
                        {
                            if (MySessionComponentSafeZones.IsActionAllowed(this, MySafeZoneAction.Building, builderEntityId))
                            {
                                EndpointId targetEndpoint = new EndpointId();
                                MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, Vector3UByte, HashSet<Vector3UByte>>(this, x => new Action<Vector3I, Vector3UByte, HashSet<Vector3UByte>>(x.RazeBlocksAreaSuccess), pos, size, this.m_tmpBuildFailList, targetEndpoint);
                                this.RazeBlocksAreaSuccess(pos, size, this.m_tmpBuildFailList);
                            }
                            break;
                        }
                        num.Y = 0;
                        while (true)
                        {
                            if (num.Y > size.Y)
                            {
                                byte* numPtr3 = (byte*) ref num.X;
                                numPtr3[0] = (byte) (numPtr3[0] + 1);
                                break;
                            }
                            num.Z = 0;
                            while (true)
                            {
                                if (num.Z > size.Z)
                                {
                                    byte* numPtr2 = (byte*) ref num.Y;
                                    numPtr2[0] = (byte) (numPtr2[0] + 1);
                                    break;
                                }
                                Vector3I vectori = (Vector3I) (pos + num);
                                MySlimBlock cubeBlock = this.GetCubeBlock(vectori);
                                if ((cubeBlock == null) || ((cubeBlock.FatBlock != null) && cubeBlock.FatBlock.IsSubBlock))
                                {
                                    this.m_tmpBuildFailList.Add(num);
                                }
                                byte* numPtr1 = (byte*) ref num.Z;
                                numPtr1[0] = (byte) (numPtr1[0] + 1);
                            }
                        }
                    }
                }
                finally
                {
                    this.m_tmpBuildFailList.Clear();
                }
            }
        }

        [Event(null, 0x10e4), Reliable, Broadcast]
        private unsafe void RazeBlocksAreaSuccess(Vector3I pos, Vector3UByte size, HashSet<Vector3UByte> resultFailList)
        {
            Vector3UByte num;
            Vector3I maxValue = Vector3I.MaxValue;
            Vector3I minValue = Vector3I.MinValue;
            if (!MyFakes.ENABLE_MULTIBLOCKS)
            {
                num.X = 0;
                while (num.X <= size.X)
                {
                    num.Y = 0;
                    while (true)
                    {
                        if (num.Y > size.Y)
                        {
                            byte* numPtr6 = (byte*) ref num.X;
                            numPtr6[0] = (byte) (numPtr6[0] + 1);
                            break;
                        }
                        num.Z = 0;
                        while (true)
                        {
                            if (num.Z > size.Z)
                            {
                                byte* numPtr5 = (byte*) ref num.Y;
                                numPtr5[0] = (byte) (numPtr5[0] + 1);
                                break;
                            }
                            if (!resultFailList.Contains(num))
                            {
                                Vector3I vectori4 = (Vector3I) (pos + num);
                                MySlimBlock cubeBlock = this.GetCubeBlock(vectori4);
                                if (cubeBlock != null)
                                {
                                    maxValue = Vector3I.Min(maxValue, cubeBlock.Min);
                                    minValue = Vector3I.Max(minValue, cubeBlock.Max);
                                    this.RemoveBlockByCubeBuilder(cubeBlock);
                                }
                            }
                            byte* numPtr4 = (byte*) ref num.Z;
                            numPtr4[0] = (byte) (numPtr4[0] + 1);
                        }
                    }
                }
                goto TR_0002;
            }
            else
            {
                num.X = 0;
            }
            goto TR_0030;
        TR_0002:
            if (this.Physics != null)
            {
                this.Physics.AddDirtyArea(maxValue, minValue);
            }
            return;
        TR_0030:
            while (true)
            {
                if (num.X <= size.X)
                {
                    num.Y = 0;
                }
                else
                {
                    goto TR_0002;
                }
                break;
            }
            while (true)
            {
                if (num.Y > size.Y)
                {
                    byte* numPtr3 = (byte*) ref num.X;
                    numPtr3[0] = (byte) (numPtr3[0] + 1);
                    break;
                }
                num.Z = 0;
                while (true)
                {
                    if (num.Z > size.Z)
                    {
                        byte* numPtr2 = (byte*) ref num.Y;
                        numPtr2[0] = (byte) (numPtr2[0] + 1);
                        break;
                    }
                    if (!resultFailList.Contains(num))
                    {
                        Vector3I vectori3 = (Vector3I) (pos + num);
                        MySlimBlock cubeBlock = this.GetCubeBlock(vectori3);
                        if (cubeBlock != null)
                        {
                            MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                            if (fatBlock == null)
                            {
                                if (!cubeBlock.IsMultiBlockPart)
                                {
                                    MyFracturedBlock block4 = cubeBlock.FatBlock as MyFracturedBlock;
                                    if (((block4 != null) && (block4.MultiBlocks != null)) && (block4.MultiBlocks.Count > 0))
                                    {
                                        foreach (MyFracturedBlock.MultiBlockPartInfo info in block4.MultiBlocks)
                                        {
                                            if (info != null)
                                            {
                                                m_tmpBlocksInMultiBlock.Clear();
                                                if (MyDefinitionManager.Static.TryGetMultiBlockDefinition(info.MultiBlockDefinition) != null)
                                                {
                                                    this.GetBlocksInMultiBlock(info.MultiBlockId, m_tmpBlocksInMultiBlock);
                                                    this.RemoveMultiBlocks(ref maxValue, ref minValue, m_tmpBlocksInMultiBlock);
                                                }
                                                m_tmpBlocksInMultiBlock.Clear();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        maxValue = Vector3I.Min(maxValue, cubeBlock.Min);
                                        minValue = Vector3I.Max(minValue, cubeBlock.Max);
                                        this.RemoveBlockByCubeBuilder(cubeBlock);
                                    }
                                }
                                else
                                {
                                    m_tmpBlocksInMultiBlock.Clear();
                                    this.GetBlocksInMultiBlock(cubeBlock.MultiBlockId, m_tmpBlocksInMultiBlock);
                                    this.RemoveMultiBlocks(ref maxValue, ref minValue, m_tmpBlocksInMultiBlock);
                                    m_tmpBlocksInMultiBlock.Clear();
                                }
                            }
                            else
                            {
                                m_tmpSlimBlocks.Clear();
                                m_tmpSlimBlocks.AddRange(fatBlock.GetBlocks());
                                foreach (MySlimBlock block3 in m_tmpSlimBlocks)
                                {
                                    if (block3.IsMultiBlockPart)
                                    {
                                        m_tmpBlocksInMultiBlock.Clear();
                                        this.GetBlocksInMultiBlock(block3.MultiBlockId, m_tmpBlocksInMultiBlock);
                                        this.RemoveMultiBlocks(ref maxValue, ref minValue, m_tmpBlocksInMultiBlock);
                                        m_tmpBlocksInMultiBlock.Clear();
                                        continue;
                                    }
                                    ushort? blockId = fatBlock.GetBlockId(block3);
                                    if (blockId != null)
                                    {
                                        this.RemoveBlockInCompound(block3.Position, blockId.Value, ref maxValue, ref minValue, null);
                                    }
                                }
                                m_tmpSlimBlocks.Clear();
                            }
                        }
                    }
                    byte* numPtr1 = (byte*) ref num.Z;
                    numPtr1[0] = (byte) (numPtr1[0] + 1);
                }
            }
            goto TR_0030;
        }

        [Event(null, 0x118d), Reliable, Broadcast]
        public void RazeBlocksClient(List<Vector3I> locations)
        {
            m_tmpPositionListReceive.Clear();
            this.RazeBlocksSuccess(locations, m_tmpPositionListReceive);
        }

        public unsafe void RazeBlocksDelayed(ref Vector3I pos, ref Vector3UByte size, long builderEntityId)
        {
            Vector3UByte num;
            bool flag = false;
            num.X = 0;
            while (num.X <= size.X)
            {
                num.Y = 0;
                while (true)
                {
                    if (num.Y > size.Y)
                    {
                        byte* numPtr3 = (byte*) ref num.X;
                        numPtr3[0] = (byte) (numPtr3[0] + 1);
                        break;
                    }
                    num.Z = 0;
                    while (true)
                    {
                        if (num.Z > size.Z)
                        {
                            byte* numPtr2 = (byte*) ref num.Y;
                            numPtr2[0] = (byte) (numPtr2[0] + 1);
                            break;
                        }
                        Vector3I vectori = (Vector3I) (pos + num);
                        MySlimBlock cubeBlock = this.GetCubeBlock(vectori);
                        if (((cubeBlock != null) && (cubeBlock.FatBlock != null)) && !cubeBlock.FatBlock.IsSubBlock)
                        {
                            MyCockpit fatBlock = cubeBlock.FatBlock as MyCockpit;
                            if ((fatBlock != null) && (fatBlock.Pilot != null))
                            {
                                if (!flag)
                                {
                                    flag = true;
                                    this.m_isRazeBatchDelayed = true;
                                    this.m_delayedRazeBatch = new MyDelayedRazeBatch(pos, size);
                                    this.m_delayedRazeBatch.Occupied = new HashSet<MyCockpit>();
                                }
                                this.m_delayedRazeBatch.Occupied.Add(fatBlock);
                            }
                        }
                        byte* numPtr1 = (byte*) ref num.Z;
                        numPtr1[0] = (byte) (numPtr1[0] + 1);
                    }
                }
            }
            if (!flag)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, Vector3UByte, long>(this, x => new Action<Vector3I, Vector3UByte, long>(x.RazeBlocksAreaRequest), pos, size, builderEntityId, targetEndpoint);
            }
            else if ((MySession.Static.CreativeMode || (MyMultiplayer.Static == null)) || !MySession.Static.IsUserAdmin(Sync.MyId))
            {
                this.OnClosedMessageBox(MyGuiScreenMessageBox.ResultEnum.NO);
            }
            else
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.RemovePilotToo), null, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnClosedMessageBox), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
        }

        [Event(null, 0x1180), Reliable, Server]
        public void RazeBlocksRequest(List<Vector3I> locations, long builderEntityId = 0L)
        {
            m_tmpPositionListReceive.Clear();
            if (MySessionComponentSafeZones.IsActionAllowed(this, MySafeZoneAction.Building, builderEntityId))
            {
                this.RazeBlocksSuccess(locations, m_tmpPositionListReceive);
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, List<Vector3I>>(this, x => new Action<List<Vector3I>>(x.RazeBlocksClient), m_tmpPositionListReceive, targetEndpoint);
            }
        }

        private void RazeBlocksSuccess(List<Vector3I> locations, List<Vector3I> removedBlocks)
        {
            Vector3I maxValue = Vector3I.MaxValue;
            Vector3I minValue = Vector3I.MinValue;
            foreach (Vector3I vectori3 in locations)
            {
                MySlimBlock cubeBlock = this.GetCubeBlock(vectori3);
                if (cubeBlock != null)
                {
                    removedBlocks.Add(vectori3);
                    maxValue = Vector3I.Min(maxValue, cubeBlock.Min);
                    minValue = Vector3I.Max(minValue, cubeBlock.Max);
                    this.RemoveBlockByCubeBuilder(cubeBlock);
                }
            }
            if (this.Physics != null)
            {
                this.Physics.AddDirtyArea(maxValue, minValue);
            }
        }

        public void RazeGeneratedBlocks(List<MySlimBlock> generatedBlocks)
        {
            m_tmpRazeList.Clear();
            m_tmpLocations.Clear();
            foreach (MySlimBlock block in generatedBlocks)
            {
                MySlimBlock cubeBlock = this.GetCubeBlock(block.Position);
                if (cubeBlock != null)
                {
                    if (!(cubeBlock.FatBlock is MyCompoundCubeBlock))
                    {
                        m_tmpLocations.Add(block.Position);
                        continue;
                    }
                    ushort? blockId = (cubeBlock.FatBlock as MyCompoundCubeBlock).GetBlockId(block);
                    if (blockId != null)
                    {
                        m_tmpRazeList.Add(new Tuple<Vector3I, ushort>(block.Position, blockId.Value));
                    }
                }
            }
            if (m_tmpLocations.Count > 0)
            {
                this.RazeGeneratedBlocks(m_tmpLocations);
            }
            if (m_tmpRazeList.Count > 0)
            {
                this.RazeGeneratedBlocksInCompoundBlock(m_tmpRazeList);
            }
            m_tmpRazeList.Clear();
            m_tmpLocations.Clear();
        }

        public void RazeGeneratedBlocks(List<Vector3I> locations)
        {
            Vector3I maxValue = Vector3I.MaxValue;
            Vector3I minValue = Vector3I.MinValue;
            foreach (Vector3I vectori3 in locations)
            {
                MySlimBlock cubeBlock = this.GetCubeBlock(vectori3);
                if (cubeBlock != null)
                {
                    maxValue = Vector3I.Min(maxValue, cubeBlock.Min);
                    minValue = Vector3I.Max(minValue, cubeBlock.Max);
                    this.RemoveBlockByCubeBuilder(cubeBlock);
                }
            }
            if (this.Physics != null)
            {
                this.Physics.AddDirtyArea(maxValue, minValue);
            }
        }

        public void RazeGeneratedBlocksInCompoundBlock(List<Tuple<Vector3I, ushort>> locationsAndIds)
        {
            Vector3I maxValue = Vector3I.MaxValue;
            Vector3I minValue = Vector3I.MinValue;
            foreach (Tuple<Vector3I, ushort> tuple in locationsAndIds)
            {
                MySlimBlock cubeBlock = this.GetCubeBlock(tuple.Item1);
                if ((cubeBlock != null) && (cubeBlock.FatBlock is MyCompoundCubeBlock))
                {
                    this.RemoveBlockInCompoundInternal(tuple.Item1, tuple.Item2, ref maxValue, ref minValue, null, cubeBlock, cubeBlock.FatBlock as MyCompoundCubeBlock);
                }
            }
            this.m_dirtyRegion.AddCubeRegion(maxValue, minValue);
            if (this.Physics != null)
            {
                this.Physics.AddDirtyArea(maxValue, minValue);
            }
            this.MarkForDraw();
        }

        private void RebuildGrid(bool staticPhysics = false)
        {
            if (this.HasStandAloneBlocks() && this.CanHavePhysics())
            {
                this.RecalcBounds();
                this.RemoveRedundantParts();
                if (this.Physics != null)
                {
                    this.Physics.Close();
                    this.Physics = null;
                }
                if (this.CreatePhysics)
                {
                    this.Physics = new MyGridPhysics(this, null, staticPhysics);
                    base.RaisePhysicsChanged();
                    if (!Sync.IsServer && !this.IsClientPredicted)
                    {
                        this.Physics.RigidBody.UpdateMotionType(HkMotionType.Fixed);
                    }
                }
            }
        }

        private void RecalcBounds()
        {
            this.m_min = Vector3I.MaxValue;
            this.m_max = Vector3I.MinValue;
            foreach (KeyValuePair<Vector3I, MyCube> pair in this.m_cubes)
            {
                this.m_min = Vector3I.Min(this.m_min, pair.Key);
                this.m_max = Vector3I.Max(this.m_max, pair.Key);
            }
            if (this.m_cubes.Count == 0)
            {
                this.m_min = -Vector3I.One;
                this.m_max = Vector3I.One;
            }
            this.UpdateGridAABB();
        }

        public void RecalculateGravity()
        {
            Vector3D position;
            if ((this.Physics == null) || (this.Physics.RigidBody == null))
            {
                position = base.PositionComp.GetPosition();
            }
            else
            {
                position = this.Physics.CenterOfMassWorld;
            }
            this.m_gravity = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
            if (MyPerGameSettings.Game == GameEnum.VRS_GAME)
            {
                this.m_gravity += MyGravityProviderSystem.CalculateArtificialGravityInPoint(position, 1f);
            }
        }

        public void RecalculateOwners()
        {
            if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
            {
                this.m_ownershipManager.RecalculateOwners();
            }
        }

        public void RegisterDecoy(MyDecoy block)
        {
            if (this.m_decoys == null)
            {
                this.m_decoys = new HashSet<MyDecoy>();
            }
            this.m_decoys.Add(block);
        }

        public void RegisterInventory(MyCubeBlock block)
        {
            this.m_inventories.Add(block);
        }

        public void RegisterOccupiedBlock(MyCockpit block)
        {
            this.m_occupiedBlocks.Add(block);
        }

        public void RegisterUnsafeBlock(MyCubeBlock block)
        {
            if (this.m_unsafeBlocks.Add(block))
            {
                if (this.m_unsafeBlocks.Count == 1)
                {
                    MyUnsafeGridsSessionComponent.RegisterGrid(this);
                }
                else
                {
                    MyUnsafeGridsSessionComponent.OnGridChanged(this);
                }
            }
        }

        private void ReleaseMerginGrids()
        {
            if (this.m_pendingGridReleases != null)
            {
                this.m_pendingGridReleases();
            }
        }

        [Event(null, 0x288e), Reliable, Server(ValidationType.Controlled | ValidationType.Access), Broadcast]
        private void RelfectorStateRecived(MyMultipleEnabledEnum value)
        {
            this.GridSystems.ReflectorLightSystem.ReflectorStateChanged(value);
        }

        private void RemoveAuthorshipAll()
        {
            foreach (MySlimBlock block in this.GetBlocks())
            {
                block.RemoveAuthorship();
                this.m_PCU -= block.ComponentStack.IsFunctional ? block.BlockDefinition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST;
            }
        }

        public void RemoveBlock(MySlimBlock block, bool updatePhysics = false)
        {
            if (Sync.IsServer && this.m_cubeBlocks.Contains(block))
            {
                this.EnqueueRemovedBlock(block.Min, this.m_generatorsEnabled);
                this.RemoveBlockInternal(block, true, true);
                if (updatePhysics)
                {
                    this.Physics.AddDirtyBlock(block);
                }
            }
        }

        private void RemoveBlockByCubeBuilder(MySlimBlock block)
        {
            this.RemoveBlockInternal(block, true, true);
            if (block.FatBlock != null)
            {
                block.FatBlock.OnRemovedByCubeBuilder();
            }
        }

        private void RemoveBlockEdges(MySlimBlock block)
        {
            using (base.Pin())
            {
                if (!base.MarkedForClose)
                {
                    MyCubeBlockDefinition blockDefinition = block.BlockDefinition;
                    if ((blockDefinition.BlockTopology == MyBlockTopology.Cube) && (blockDefinition.CubeDefinition != null))
                    {
                        Matrix matrix;
                        block.Orientation.GetMatrix(out matrix);
                        matrix.Translation = (Vector3) (block.Position * this.GridSize);
                        MyEdgeDefinition[] edges = MyCubeGridDefinitions.GetTopologyInfo(blockDefinition.CubeDefinition.CubeTopology).Edges;
                        for (int i = 0; i < edges.Length; i++)
                        {
                            MyEdgeDefinition definition1 = edges[i];
                            Vector3 vector2 = Vector3.Transform(definition1.Point0 * this.GridSizeHalf, matrix);
                            Vector3 vector3 = Vector3.Transform(definition1.Point1 * this.GridSizeHalf, matrix);
                            this.Render.RenderData.RemoveEdgeInfo(vector2, vector3, block);
                        }
                    }
                }
            }
        }

        private void RemoveBlockInCompound(Vector3I position, ushort compoundBlockId, ref Vector3I min, ref Vector3I max, List<Tuple<Vector3I, ushort>> removedBlocks = null)
        {
            MySlimBlock cubeBlock = this.GetCubeBlock(position);
            if ((cubeBlock != null) && (cubeBlock.FatBlock is MyCompoundCubeBlock))
            {
                this.RemoveBlockInCompoundInternal(position, compoundBlockId, ref min, ref max, removedBlocks, cubeBlock, cubeBlock.FatBlock as MyCompoundCubeBlock);
            }
        }

        private void RemoveBlockInCompoundInternal(Vector3I position, ushort compoundBlockId, ref Vector3I min, ref Vector3I max, List<Tuple<Vector3I, ushort>> removedBlocks, MySlimBlock block, MyCompoundCubeBlock compoundBlock)
        {
            MySlimBlock block2 = compoundBlock.GetBlock(compoundBlockId);
            if ((block2 != null) && compoundBlock.Remove(block2, false))
            {
                if (removedBlocks != null)
                {
                    removedBlocks.Add(new Tuple<Vector3I, ushort>(position, compoundBlockId));
                }
                min = Vector3I.Min(min, block.Min);
                max = Vector3I.Max(max, block.Max);
                if ((MyCubeGridSmallToLargeConnection.Static != null) && this.m_enableSmallToLargeConnections)
                {
                    MyCubeGridSmallToLargeConnection.Static.RemoveBlockSmallToLargeConnection(block2);
                }
                this.NotifyBlockRemoved(block2);
            }
            if (compoundBlock.GetBlocksCount() == 0)
            {
                this.RemoveBlockByCubeBuilder(block);
            }
        }

        private void RemoveBlockInternal(MySlimBlock block, bool close, bool markDirtyDisconnects = true)
        {
            if (this.m_cubeBlocks.Contains(block))
            {
                if (MyFakes.ENABLE_MULTIBLOCK_PART_IDS)
                {
                    this.RemoveMultiBlockInfo(block);
                }
                this.RenderData.RemoveDecals(block.Position);
                MyTerminalBlock fatBlock = block.FatBlock as MyTerminalBlock;
                if (fatBlock != null)
                {
                    for (int i = 0; i < this.BlockGroups.Count; i++)
                    {
                        MyBlockGroup group = this.BlockGroups[i];
                        if (group.Blocks.Contains(fatBlock) && (group.Blocks.Count == 1))
                        {
                            this.RemoveGroup(group);
                            group.Blocks.Remove(fatBlock);
                            i--;
                        }
                    }
                }
                this.RemoveBlockParts(block);
                Parallel.Start(() => this.RemoveBlockEdges(block));
                if (block.FatBlock != null)
                {
                    if (this.BlocksCounters.ContainsKey(block.BlockDefinition.Id.TypeId))
                    {
                        MyObjectBuilderType typeId = block.BlockDefinition.Id.TypeId;
                        this.BlocksCounters[typeId] -= 1;
                    }
                    block.FatBlock.IsBeingRemoved = true;
                    this.GridSystems.UnregisterFromSystems(block.FatBlock);
                    if (close)
                    {
                        block.FatBlock.Close();
                    }
                    else
                    {
                        base.Hierarchy.RemoveChild(block.FatBlock, false);
                    }
                    if (block.FatBlock.Render.NeedsDrawFromParent)
                    {
                        this.m_blocksForDraw.Remove(block.FatBlock);
                        block.FatBlock.Render.SetVisibilityUpdates(false);
                    }
                }
                block.RemoveNeighbours();
                block.RemoveAuthorship();
                this.m_PCU -= block.ComponentStack.IsFunctional ? block.BlockDefinition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST;
                this.m_cubeBlocks.Remove(block);
                if (block.FatBlock != null)
                {
                    if (block.FatBlock is MyReactor)
                    {
                        this.NumberOfReactors--;
                    }
                    this.m_fatBlocks.Remove(block.FatBlock);
                    block.FatBlock.IsBeingRemoved = false;
                }
                Vector3 colorMaskHSV = block.ColorMaskHSV;
                this.m_colorStatistics[colorMaskHSV] -= 1;
                if (this.m_colorStatistics[block.ColorMaskHSV] <= 0)
                {
                    this.m_colorStatistics.Remove(block.ColorMaskHSV);
                }
                if (markDirtyDisconnects)
                {
                    this.m_disconnectsDirty = MyTestDisconnectsReason.BlockRemoved;
                }
                Vector3I min = block.Min;
                Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref block.Min, ref block.Max);
                while (iterator.IsValid())
                {
                    this.Skeleton.MarkCubeRemoved(ref min);
                    iterator.GetNext(out min);
                }
                if ((block.FatBlock != null) && (block.FatBlock.IDModule != null))
                {
                    this.ChangeOwner(block.FatBlock, block.FatBlock.IDModule.Owner, 0L);
                }
                if ((MyCubeGridSmallToLargeConnection.Static != null) && this.m_enableSmallToLargeConnections)
                {
                    MyCubeGridSmallToLargeConnection.Static.RemoveBlockSmallToLargeConnection(block);
                }
                this.NotifyBlockRemoved(block);
                if (close)
                {
                    this.NotifyBlockClosed(block);
                }
                this.m_boundsDirty = true;
                this.MarkForUpdate();
                this.MarkForDraw();
            }
        }

        private unsafe void RemoveBlockParts(MySlimBlock block)
        {
            Vector3I vectori;
            vectori.X = block.Min.X;
            while (vectori.X <= block.Max.X)
            {
                vectori.Y = block.Min.Y;
                while (true)
                {
                    if (vectori.Y > block.Max.Y)
                    {
                        int* numPtr3 = (int*) ref vectori.X;
                        numPtr3[0]++;
                        break;
                    }
                    vectori.Z = block.Min.Z;
                    while (true)
                    {
                        MyCube cube;
                        if (vectori.Z > block.Max.Z)
                        {
                            int* numPtr2 = (int*) ref vectori.Y;
                            numPtr2[0]++;
                            break;
                        }
                        if (this.m_cubes.TryRemove(vectori, out cube))
                        {
                            this.m_dirtyRegion.PartsToRemove.Enqueue(cube);
                        }
                        int* numPtr1 = (int*) ref vectori.Z;
                        numPtr1[0]++;
                    }
                }
            }
            this.MarkForDraw();
        }

        [Event(null, 0xe4f), Reliable, ServerInvoked, Broadcast]
        public void RemoveBlocksBuiltByID(long identityID)
        {
            foreach (MySlimBlock block in this.FindBlocksBuiltByID(identityID))
            {
                this.RemoveBlock(block, true);
            }
        }

        public void RemoveBlockWithId(MySlimBlock block, bool updatePhysics = false)
        {
            MySlimBlock cubeBlock = this.GetCubeBlock(block.Min);
            if (cubeBlock != null)
            {
                MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                ushort? compoundId = null;
                if (fatBlock != null)
                {
                    compoundId = fatBlock.GetBlockId(block);
                    if (compoundId == null)
                    {
                        return;
                    }
                }
                this.RemoveBlockWithId(block.Min, compoundId, updatePhysics);
            }
        }

        public void RemoveBlockWithId(Vector3I position, ushort? compoundId, bool updatePhysics = false)
        {
            if (Sync.IsServer)
            {
                MySlimBlock cubeBlock = this.GetCubeBlock(position);
                if (cubeBlock != null)
                {
                    this.EnqueueRemovedBlockWithId(cubeBlock.Min, compoundId, this.m_generatorsEnabled);
                    if (compoundId == null)
                    {
                        this.RemoveBlockInternal(cubeBlock, true, true);
                    }
                    else
                    {
                        Vector3I zero = Vector3I.Zero;
                        Vector3I max = Vector3I.Zero;
                        this.RemoveBlockInCompound(cubeBlock.Min, compoundId.Value, ref zero, ref max, null);
                    }
                    if (updatePhysics)
                    {
                        this.Physics.AddDirtyBlock(cubeBlock);
                    }
                }
            }
        }

        [Event(null, 0xdf3), Reliable, Broadcast]
        private void RemovedBlocks(List<Vector3I> locationsWithGenerator, List<Vector3I> destroyLocations, List<Vector3I> DestructionDeformationLocation, List<Vector3I> LocationsWithoutGenerator)
        {
            if (destroyLocations.Count > 0)
            {
                this.BlocksDestroyed(destroyLocations);
            }
            if (locationsWithGenerator.Count > 0)
            {
                this.BlocksRemovedWithGenerator(locationsWithGenerator);
            }
            if (LocationsWithoutGenerator.Count > 0)
            {
                this.BlocksRemovedWithoutGenerator(LocationsWithoutGenerator);
            }
            if (DestructionDeformationLocation.Count > 0)
            {
                this.BlocksDeformed(DestructionDeformationLocation);
            }
        }

        [Event(null, 0xe37), Reliable, Broadcast]
        private void RemovedBlocksWithIds(List<BlockPositionId> removeBlockWithIdQueueWithGenerators, List<BlockPositionId> destroyBlockWithIdQueueWithGenerators, List<BlockPositionId> destroyBlockWithIdQueueWithoutGenerators, List<BlockPositionId> removeBlockWithIdQueueWithoutGenerators)
        {
            if (destroyBlockWithIdQueueWithGenerators.Count > 0)
            {
                this.BlocksWithIdDestroyedWithGenerator(destroyBlockWithIdQueueWithGenerators);
            }
            if (destroyBlockWithIdQueueWithoutGenerators.Count > 0)
            {
                this.BlocksWithIdDestroyedWithoutGenerator(destroyBlockWithIdQueueWithoutGenerators);
            }
            if (removeBlockWithIdQueueWithGenerators.Count > 0)
            {
                this.BlocksWithIdRemovedWithGenerator(removeBlockWithIdQueueWithGenerators);
            }
            if (removeBlockWithIdQueueWithoutGenerators.Count > 0)
            {
                this.BlocksWithIdRemovedWithoutGenerator(removeBlockWithIdQueueWithoutGenerators);
            }
        }

        public void RemoveDestroyedBlock(MySlimBlock block, long attackerId = 0L)
        {
            if (!Sync.IsServer)
            {
                if (!MyFakes.ENABLE_FRACTURE_COMPONENT)
                {
                    block.OnDestroyVisual();
                }
            }
            else if (this.Physics != null)
            {
                if (!MyFakes.ENABLE_FRACTURE_COMPONENT)
                {
                    this.EnqueueDestroyedBlock(block.Position);
                    this.RemoveDestroyedBlockInternal(block);
                    this.Physics.AddDirtyBlock(block);
                }
                else
                {
                    bool enable = attackerId != 0L;
                    bool flag2 = this.EnableGenerators(enable, false);
                    MySlimBlock cubeBlock = this.GetCubeBlock(block.Position);
                    if (cubeBlock != null)
                    {
                        if (ReferenceEquals(cubeBlock, block))
                        {
                            ushort? compoundId = null;
                            this.EnqueueDestroyedBlockWithId(block.Position, compoundId, enable);
                            this.RemoveDestroyedBlockInternal(block);
                            this.Physics.AddDirtyBlock(block);
                        }
                        else
                        {
                            MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                            if (fatBlock != null)
                            {
                                ushort? blockId = fatBlock.GetBlockId(block);
                                if (blockId != null)
                                {
                                    this.EnqueueDestroyedBlockWithId(block.Position, blockId, enable);
                                    this.RemoveDestroyedBlockInternal(block);
                                    this.Physics.AddDirtyBlock(block);
                                }
                            }
                        }
                        this.EnableGenerators(flag2, false);
                        MyFractureComponentCubeBlock fractureComponent = block.GetFractureComponent();
                        if (fractureComponent != null)
                        {
                            MyDestructionHelper.CreateFracturePiece(fractureComponent, true);
                        }
                    }
                }
            }
        }

        private void RemoveDestroyedBlockInternal(MySlimBlock block)
        {
            this.ApplyDestructionDeformationInternal(block, false, 1f, 0L, true);
            ((IMyDestroyableObject) block).OnDestroy();
            MySlimBlock cubeBlock = this.GetCubeBlock(block.Position);
            if (ReferenceEquals(cubeBlock, block))
            {
                this.RemoveBlockInternal(block, true, true);
            }
            else if (cubeBlock != null)
            {
                MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                if (fatBlock != null)
                {
                    ushort? blockId = fatBlock.GetBlockId(block);
                    if (blockId != null)
                    {
                        Vector3I maxValue = Vector3I.MaxValue;
                        Vector3I minValue = Vector3I.MinValue;
                        this.RemoveBlockInCompound(block.Position, blockId.Value, ref maxValue, ref minValue, null);
                    }
                }
            }
        }

        private void RemoveEmptyBlockGroups()
        {
            for (int i = 0; i < this.BlockGroups.Count; i++)
            {
                MyBlockGroup group = this.BlockGroups[i];
                if (group.Blocks.Count == 0)
                {
                    this.RemoveGroup(group);
                    i--;
                }
            }
        }

        private static void RemoveFilesFromDirectory(string path, string fileType)
        {
            string[] files = Directory.GetFiles(path, fileType);
            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i]);
            }
        }

        internal void RemoveFromDamageApplication(MySlimBlock block)
        {
            this.m_blocksForDamageApplication.Remove(block);
            this.m_blocksForDamageApplicationDirty = this.m_blocksForDamageApplication.Count > 0;
            if (this.m_blocksForDamageApplicationDirty)
            {
                this.MarkForUpdate();
            }
        }

        internal void RemoveGroup(MyBlockGroup group)
        {
            this.BlockGroups.Remove(group);
            this.GridSystems.RemoveGroup(group);
        }

        internal void RemoveGroupByName(string name)
        {
            MyBlockGroup item = this.BlockGroups.Find(g => g.Name.CompareTo(name) == 0);
            if (item != null)
            {
                this.BlockGroups.Remove(item);
                this.GridSystems.RemoveGroup(item);
            }
        }

        internal void RemoveMultiBlockInfo(MySlimBlock block)
        {
            if (this.m_multiBlockInfos != null)
            {
                MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
                if (fatBlock == null)
                {
                    MyCubeGridMultiBlockInfo info;
                    if (block.IsMultiBlockPart && ((this.m_multiBlockInfos.TryGetValue(block.MultiBlockId, out info) && (info.Blocks.Remove(block) && ((info.Blocks.Count == 0) && this.m_multiBlockInfos.Remove(block.MultiBlockId)))) && (this.m_multiBlockInfos.Count == 0)))
                    {
                        this.m_multiBlockInfos = null;
                    }
                }
                else
                {
                    foreach (MySlimBlock block3 in fatBlock.GetBlocks())
                    {
                        if (block3.IsMultiBlockPart)
                        {
                            this.RemoveMultiBlockInfo(block3);
                        }
                    }
                }
            }
        }

        private void RemoveMultiBlocks(ref Vector3I min, ref Vector3I max, HashSet<Tuple<MySlimBlock, ushort?>> tmpBlocksInMultiBlock)
        {
            foreach (Tuple<MySlimBlock, ushort?> tuple in tmpBlocksInMultiBlock)
            {
                if (tuple.Item2 != null)
                {
                    this.RemoveBlockInCompound(tuple.Item1.Position, tuple.Item2.Value, ref min, ref max, null);
                    continue;
                }
                min = Vector3I.Min(min, tuple.Item1.Min);
                max = Vector3I.Max(max, tuple.Item1.Max);
                this.RemoveBlockByCubeBuilder(tuple.Item1);
            }
        }

        private bool RemoveNonPublicBlock(MyObjectBuilder_CubeBlock block)
        {
            MyCubeBlockDefinition definition;
            MyObjectBuilder_CompoundCubeBlock block2 = this.UpgradeCubeBlock(block, out definition) as MyObjectBuilder_CompoundCubeBlock;
            if (block2 == null)
            {
                return ((definition != null) && !definition.Public);
            }
            block2.Blocks = block2.Blocks.Where<MyObjectBuilder_CubeBlock>(delegate (MyObjectBuilder_CubeBlock s) {
                MyCubeBlockDefinition def;
                return (!MyDefinitionManager.Static.TryGetCubeBlockDefinition(s.GetId(), out def) || (def.Public || def.IsGeneratedBlock));
            }).ToArray<MyObjectBuilder_CubeBlock>();
            return (block2.Blocks.Length == 0);
        }

        private void RemoveNonPublicBlocks(MyObjectBuilder_CubeGrid builder)
        {
            builder.CubeBlocks.RemoveAll(s => this.RemoveNonPublicBlock(s));
        }

        private void RemoveRedundantParts()
        {
            foreach (KeyValuePair<Vector3I, MyCube> pair in this.m_cubes)
            {
                this.UpdateParts(pair.Key);
            }
        }

        public static void RemoveSplit(MyCubeGrid originalGrid, List<MySlimBlock> blocks, int offset, int count, bool sync = true)
        {
            for (int i = offset; i < (offset + count); i++)
            {
                if (blocks.Count > i)
                {
                    MySlimBlock block = blocks[i];
                    if (block != null)
                    {
                        if (block.FatBlock != null)
                        {
                            originalGrid.Hierarchy.RemoveChild(block.FatBlock, false);
                        }
                        bool enable = originalGrid.EnableGenerators(false, true);
                        originalGrid.RemoveBlockInternal(block, true, false);
                        originalGrid.EnableGenerators(enable, true);
                        originalGrid.Physics.AddDirtyBlock(block);
                    }
                }
            }
            originalGrid.RemoveEmptyBlockGroups();
            if (sync && Sync.IsServer)
            {
                originalGrid.AnnounceRemoveSplit(blocks);
            }
        }

        public void RequestConversionToShip(Action result)
        {
            this.m_convertToShipResult = result;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, MyTestDynamicReason>(this, x => new Action<MyTestDynamicReason>(x.OnConvertedToShipRequest), MyTestDynamicReason.ConvertToShip, targetEndpoint);
        }

        public void RequestConversionToStation()
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid>(this, x => new Action(x.OnConvertedToStationRequest), targetEndpoint);
        }

        public void RequestFillStockpile(Vector3I blockPosition, MyInventory fromInventory)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, long, byte>(this, x => new Action<Vector3I, long, byte>(x.OnStockpileFillRequest), blockPosition, fromInventory.Owner.EntityId, fromInventory.InventoryIdx, targetEndpoint);
        }

        public void RequestSetToConstruction(Vector3I blockPosition, MyInventory fromInventory)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, long, byte, long>(this, x => new Action<Vector3I, long, byte, long>(x.OnSetToConstructionRequest), blockPosition, fromInventory.Owner.EntityId, fromInventory.InventoryIdx, MySession.Static.LocalPlayerId, targetEndpoint);
        }

        public void ResetBlockSkeleton(MySlimBlock block, bool updateSync = false)
        {
            this.MultiplyBlockSkeleton(block, 0f, updateSync);
        }

        public override void ResetControls()
        {
            this.m_lastNetState.Valid = false;
            MyShipController shipController = this.GridSystems.ControlSystem.GetShipController();
            if ((shipController != null) && !shipController.ControllerInfo.IsLocallyControlled())
            {
                shipController.ClearMovementControl();
            }
        }

        public static void ResetInfoGizmos()
        {
            ShowSenzorGizmos = false;
            ShowGravityGizmos = false;
            ShowCenterOfMass = false;
            ShowGridPivot = false;
            ShowAntennaGizmos = false;
            ShowStructuralIntegrity = false;
        }

        private void ResetSkeleton()
        {
            this.Skeleton = new MyGridSkeleton();
        }

        public void ResetStructuralIntegrity()
        {
            if (this.StructuralIntegrity != null)
            {
                this.StructuralIntegrity = null;
            }
        }

        unsafe void IMyGridConnectivityTest.GetConnectedBlocks(Vector3I minI, Vector3I maxI, Dictionary<Vector3I, ConnectivityResult> outOverlappedCubeBlocks)
        {
            Vector3I pos = new Vector3I {
                Z = minI.Z
            };
            while (pos.Z <= maxI.Z)
            {
                pos.Y = minI.Y;
                while (true)
                {
                    if (pos.Y > maxI.Y)
                    {
                        int* numPtr3 = (int*) ref pos.Z;
                        numPtr3[0]++;
                        break;
                    }
                    pos.X = minI.X;
                    while (true)
                    {
                        if (pos.X > maxI.X)
                        {
                            int* numPtr2 = (int*) ref pos.Y;
                            numPtr2[0]++;
                            break;
                        }
                        MySlimBlock cubeBlock = this.GetCubeBlock(pos);
                        if (cubeBlock != null)
                        {
                            ConnectivityResult result = new ConnectivityResult {
                                Definition = cubeBlock.BlockDefinition,
                                FatBlock = cubeBlock.FatBlock,
                                Orientation = cubeBlock.Orientation,
                                Position = cubeBlock.Position
                            };
                            outOverlappedCubeBlocks[cubeBlock.Position] = result;
                        }
                        int* numPtr1 = (int*) ref pos.X;
                        numPtr1[0]++;
                    }
                }
            }
        }

        private long SendBones(MyVoxelSegmentationType segmentationType, out int bytes, out int segmentsCount, out int emptyBones)
        {
            int inputCount = this.m_bonesToSendSecond.InputCount;
            long timestamp = Stopwatch.GetTimestamp();
            List<MyVoxelSegmentation.Segment> list = this.m_bonesToSendSecond.FindSegments(segmentationType, 1);
            if (m_boneByteList == null)
            {
                m_boneByteList = new List<byte>();
            }
            else
            {
                m_boneByteList.Clear();
            }
            emptyBones = 0;
            foreach (MyVoxelSegmentation.Segment segment in list)
            {
                emptyBones += this.Skeleton.SerializePart(segment.Min, segment.Max, this.GridSize, m_boneByteList) ? 0 : 1;
            }
            if (emptyBones != list.Count)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, int, List<byte>>(this, x => new Action<int, List<byte>>(x.OnBonesReceived), list.Count, m_boneByteList, targetEndpoint);
            }
            bytes = m_boneByteList.Count;
            segmentsCount = list.Count;
            return (Stopwatch.GetTimestamp() - timestamp);
        }

        private void SendBonesAsync(WorkData workData)
        {
            int num;
            int num2;
            int num3;
            int inputCount = this.m_bonesToSendSecond.InputCount;
            MyTimeSpan.FromTicks(this.SendBones(MyVoxelSegmentationType.Simple, out num, out num2, out num3));
            this.m_bonesToSendSecond.ClearInput();
            this.m_bonesSending = false;
        }

        public void SendFractureComponentRepaired(MySlimBlock mySlimBlock, long toolOwner)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, ushort, long>(this, x => new Action<Vector3I, ushort, long>(x.FractureComponentRepaired), mySlimBlock.Position, this.GetSubBlockId(mySlimBlock), toolOwner, targetEndpoint);
        }

        public void SendGridCloseRequest()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyCubeGrid.OnGridClosedRequest), base.EntityId, targetEndpoint, position);
        }

        [Event(null, 0x27ef), Reliable, Client]
        public static void SendHudNotificationAfterPaste()
        {
            MyHud.PopRotatingWheelVisible();
        }

        public void SendIntegrityChanged(MySlimBlock mySlimBlock, MyIntegrityChangeEnum integrityChangeType, long toolOwner)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, ushort, float, float, MyIntegrityChangeEnum, long>(this, x => new Action<Vector3I, ushort, float, float, MyIntegrityChangeEnum, long>(x.BlockIntegrityChanged), mySlimBlock.Position, this.GetSubBlockId(mySlimBlock), mySlimBlock.BuildIntegrity, mySlimBlock.Integrity, integrityChangeType, toolOwner, targetEndpoint);
        }

        public void SendReflectorState(MyMultipleEnabledEnum value)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyCubeGrid, MyMultipleEnabledEnum>(this, x => new Action<MyMultipleEnabledEnum>(x.RelfectorStateRecived), value, targetEndpoint);
        }

        public void SendRemovedBlocks()
        {
            if (((this.m_removeBlockQueueWithGenerators.Count > 0) || ((this.m_destroyBlockQueue.Count > 0) || (this.m_destructionDeformationQueue.Count > 0))) || (this.m_removeBlockQueueWithoutGenerators.Count > 0))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, List<Vector3I>, List<Vector3I>, List<Vector3I>, List<Vector3I>>(this, x => new Action<List<Vector3I>, List<Vector3I>, List<Vector3I>, List<Vector3I>>(x.RemovedBlocks), this.m_removeBlockQueueWithGenerators, this.m_destroyBlockQueue, this.m_destructionDeformationQueue, this.m_removeBlockQueueWithoutGenerators, targetEndpoint);
                this.m_removeBlockQueueWithGenerators.Clear();
                this.m_removeBlockQueueWithoutGenerators.Clear();
                this.m_destroyBlockQueue.Clear();
                this.m_destructionDeformationQueue.Clear();
            }
        }

        public void SendRemovedBlocksWithIds()
        {
            if (((this.m_removeBlockWithIdQueueWithGenerators.Count > 0) || ((this.m_removeBlockWithIdQueueWithoutGenerators.Count > 0) || (this.m_destroyBlockWithIdQueueWithGenerators.Count > 0))) || (this.m_destroyBlockWithIdQueueWithoutGenerators.Count > 0))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, List<BlockPositionId>, List<BlockPositionId>, List<BlockPositionId>, List<BlockPositionId>>(this, x => new Action<List<BlockPositionId>, List<BlockPositionId>, List<BlockPositionId>, List<BlockPositionId>>(x.RemovedBlocksWithIds), this.m_removeBlockWithIdQueueWithGenerators, this.m_destroyBlockWithIdQueueWithGenerators, this.m_destroyBlockWithIdQueueWithoutGenerators, this.m_removeBlockWithIdQueueWithoutGenerators, targetEndpoint);
                this.m_removeBlockWithIdQueueWithGenerators.Clear();
                this.m_removeBlockWithIdQueueWithoutGenerators.Clear();
                this.m_destroyBlockWithIdQueueWithGenerators.Clear();
                this.m_destroyBlockWithIdQueueWithoutGenerators.Clear();
            }
        }

        public void SendStockpileChanged(MySlimBlock mySlimBlock, List<MyStockpileItem> list)
        {
            if (list.Count > 0)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyCubeGrid, Vector3I, ushort, List<MyStockpileItem>>(this, x => new Action<Vector3I, ushort, List<MyStockpileItem>>(x.BlockStockpileChanged), mySlimBlock.Position, this.GetSubBlockId(mySlimBlock), list, targetEndpoint);
            }
        }

        public override void SerializeControls(BitStream stream)
        {
            MyShipController shipController = null;
            if (!this.IsStatic && base.InScene)
            {
                shipController = this.GridSystems.ControlSystem.GetShipController();
            }
            if (shipController == null)
            {
                stream.WriteBool(false);
            }
            else
            {
                stream.WriteBool(true);
                shipController.GetNetState().Serialize(stream);
            }
        }

        public void SetBlockDirty(MySlimBlock cubeBlock)
        {
            Vector3I min = cubeBlock.Min;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref cubeBlock.Min, ref cubeBlock.Max);
            while (iterator.IsValid())
            {
                this.m_dirtyRegion.AddCube(min);
                iterator.GetNext(out min);
            }
            this.MarkForUpdate();
            this.MarkForDraw();
        }

        public void SetCubeDirty(Vector3I pos)
        {
            this.m_dirtyRegion.AddCube(pos);
            MySlimBlock cubeBlock = this.GetCubeBlock(pos);
            if (cubeBlock != null)
            {
                this.Physics.AddDirtyBlock(cubeBlock);
            }
            this.MarkForUpdate();
            this.MarkForDraw();
        }

        [Event(null, 0x2958), Reliable, Server]
        public void SetHandbrakeRequest(bool v)
        {
            this.m_handBrakeSync.Value = v;
        }

        internal void SetInventoryMassDirty()
        {
            this.m_inventoryMassDirty = true;
            this.MarkForUpdate();
        }

        public void SetMainCockpit(MyTerminalBlock cockpit)
        {
            this.MainCockpit = cockpit;
        }

        public void SetMainRemoteControl(MyTerminalBlock remoteControl)
        {
            this.MainRemoteControl = remoteControl;
        }

        private bool ShouldBeMergedToThis(MyCubeGrid gridToMerge)
        {
            bool flag = IsRooted(this);
            bool flag2 = IsRooted(gridToMerge);
            return ((flag && !flag2) || ((!flag2 || flag) && (this.BlocksCount > gridToMerge.BlocksCount)));
        }

        private static bool ShouldBeStatic(MyCubeGrid grid, MyTestDynamicReason testReason)
        {
            if (testReason == MyTestDynamicReason.NoReason)
            {
                return true;
            }
            if (grid.IsUnsupportedStation && (testReason != MyTestDynamicReason.ConvertToShip))
            {
                return true;
            }
            if (((grid.GridSizeEnum == MyCubeSize.Small) && (MyCubeGridSmallToLargeConnection.Static != null)) && MyCubeGridSmallToLargeConnection.Static.TestGridSmallToLargeConnection(grid))
            {
                return true;
            }
            if ((testReason != MyTestDynamicReason.GridSplitByBlock) && (testReason != MyTestDynamicReason.ConvertToShip))
            {
                grid.RecalcBounds();
                MyGridPlacementSettings settings = new MyGridPlacementSettings();
                VoxelPlacementSettings settings3 = new VoxelPlacementSettings {
                    PlacementMode = VoxelPlacementMode.Volumetric
                };
                settings.VoxelPlacement = new VoxelPlacementSettings?(settings3);
                if (!IsAabbInsideVoxel(grid.WorldMatrix, grid.PositionComp.LocalAABB, settings))
                {
                    return false;
                }
                if (grid.GetBlocks().Count > 0x400)
                {
                    return grid.IsStatic;
                }
            }
            if (MyGamePruningStructure.AnyVoxelMapInBox(ref grid.PositionComp.WorldAABB))
            {
                using (HashSet<MySlimBlock>.Enumerator enumerator = grid.GetBlocks().GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (IsInVoxels(enumerator.Current, true))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        [Event(null, 0x27e9), Reliable, Client]
        public static void ShowPasteFailedOperation()
        {
            MyHud.Notifications.Add(MyNotificationSingletons.PasteFailed);
        }

        public MyCubeGrid SplitByPlane(PlaneD plane)
        {
            m_tmpSlimBlocks.Clear();
            MyCubeGrid grid = null;
            PlaneD ed = PlaneD.Transform(plane, base.PositionComp.WorldMatrixNormalizedInv);
            foreach (MySlimBlock block in this.GetBlocks())
            {
                BoundingBoxD xd = new BoundingBoxD((Vector3D) (block.Min * this.GridSize), (Vector3D) (block.Max * this.GridSize));
                xd.Inflate((double) (this.GridSize / 2f));
                if (xd.Intersects(ed) == PlaneIntersectionType.Back)
                {
                    m_tmpSlimBlocks.Add(block);
                }
            }
            if (m_tmpSlimBlocks.Count != 0)
            {
                grid = CreateSplit(this, m_tmpSlimBlocks, true, 0L);
                m_tmpSlimBlocks.Clear();
            }
            return grid;
        }

        public static void StartConverting(bool placeOnly)
        {
            string path = Path.Combine(MyFileSystem.UserDataPath, "SourceModels");
            if (Directory.Exists(path))
            {
                m_prefabs.Clear();
                string[] files = Directory.GetFiles(path, "*.zip");
                for (int i = 0; i < files.Length; i++)
                {
                    foreach (string str2 in MyFileSystem.GetFiles(files[i], "*.sbc", MySearchOption.AllDirectories))
                    {
                        if (!MyFileSystem.FileExists(str2))
                        {
                            continue;
                        }
                        MyObjectBuilder_Definitions objectBuilder = null;
                        MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(str2, out objectBuilder);
                        if (objectBuilder.Prefabs[0].CubeGrids != null)
                        {
                            m_prefabs.Add(objectBuilder.Prefabs[0].CubeGrids);
                        }
                    }
                }
                ConvertNextPrefab(m_prefabs, placeOnly);
            }
        }

        public static Vector3D StaticGlobalGrid_UGToWorld(Vector3D ugPos, float gridSize, bool staticGridAlignToCenter) => 
            (!staticGridAlignToCenter ? ((Vector3D) (gridSize * (ugPos - Vector3D.Half))) : ((Vector3D) (gridSize * ugPos)));

        public static Vector3D StaticGlobalGrid_WorldToUG(Vector3D worldPos, float gridSize, bool staticGridAlignToCenter)
        {
            Vector3D vectord = worldPos / ((double) gridSize);
            if (!staticGridAlignToCenter)
            {
                vectord += Vector3D.Half;
            }
            return vectord;
        }

        public static Vector3I StaticGlobalGrid_WorldToUGInt(Vector3D worldPos, float gridSize, bool staticGridAlignToCenter) => 
            Vector3I.Round(StaticGlobalGrid_WorldToUG(worldPos, gridSize, staticGridAlignToCenter));

        private void StepStructuralIntegrity()
        {
            if ((MyStructuralIntegrity.Enabled && (this.Physics != null)) && (this.Physics.HavokWorld != null))
            {
                if (this.StructuralIntegrity == null)
                {
                    this.CreateStructuralIntegrity();
                }
                if (this.StructuralIntegrity != null)
                {
                    this.StructuralIntegrity.Update(0.01666667f);
                }
            }
        }

        public bool SwitchPower()
        {
            this.m_IsPowered = !this.m_IsPowered;
            return this.m_IsPowered;
        }

        public void TargetingAddId(long id)
        {
            if (!this.m_targetingList.Contains(id))
            {
                this.m_targetingList.Add(id);
            }
            this.m_usesTargetingList = (this.m_targetingList.Count > 0) || this.m_targetingListIsWhitelist;
        }

        public bool TargetingCanAttackGrid(long id) => 
            (!this.m_targetingListIsWhitelist ? !this.m_targetingList.Contains(id) : this.m_targetingList.Contains(id));

        public void TargetingRemoveId(long id)
        {
            if (this.m_targetingList.Contains(id))
            {
                this.m_targetingList.Remove(id);
            }
            this.m_usesTargetingList = (this.m_targetingList.Count > 0) || this.m_targetingListIsWhitelist;
        }

        public void TargetingSetWhitelist(bool whitelist)
        {
            this.m_targetingListIsWhitelist = whitelist;
            this.m_usesTargetingList = (this.m_targetingList.Count > 0) || this.m_targetingListIsWhitelist;
        }

        public override unsafe void Teleport(MatrixD worldMatrix, object source = null, bool ignoreAssert = false)
        {
            IEnumerator<VRage.ModAPI.IMyEntity> enumerator;
            Dictionary<MyCubeGrid, HashSet<VRage.ModAPI.IMyEntity>> dictionary = new Dictionary<MyCubeGrid, HashSet<VRage.ModAPI.IMyEntity>>();
            Dictionary<MyCubeGrid, Tuple<Vector3, Vector3>> dictionary2 = new Dictionary<MyCubeGrid, Tuple<Vector3, Vector3>>();
            HashSet<VRage.ModAPI.IMyEntity> set = new HashSet<VRage.ModAPI.IMyEntity>();
            MyHashSetDictionary<MyCubeGrid, VRage.ModAPI.IMyEntity> dictionary3 = new MyHashSetDictionary<MyCubeGrid, VRage.ModAPI.IMyEntity>();
            MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group group = MyCubeGridGroups.Static.Physical.GetGroup(this);
            foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node node in group.Nodes)
            {
                HashSet<VRage.ModAPI.IMyEntity> result = new HashSet<VRage.ModAPI.IMyEntity> {
                    node.NodeData
                };
                node.NodeData.Hierarchy.GetChildrenRecursive(result);
                foreach (VRage.ModAPI.IMyEntity entity in result)
                {
                    if (entity.Physics != null)
                    {
                        foreach (HkConstraint local1 in ((MyPhysicsBody) entity.Physics).Constraints)
                        {
                            VRage.ModAPI.IMyEntity objB = local1.RigidBodyA.GetEntity(0);
                            VRage.ModAPI.IMyEntity entity3 = local1.RigidBodyB.GetEntity(0);
                            VRage.ModAPI.IMyEntity item = ReferenceEquals(entity, objB) ? entity3 : objB;
                            if (!result.Contains(item) && (item != null))
                            {
                                dictionary3.Add(node.NodeData, item);
                            }
                        }
                    }
                }
                dictionary.Add(node.NodeData, result);
            }
            foreach (KeyValuePair<MyCubeGrid, HashSet<VRage.ModAPI.IMyEntity>> pair in dictionary)
            {
                foreach (KeyValuePair<MyCubeGrid, HashSet<VRage.ModAPI.IMyEntity>> pair2 in dictionary)
                {
                    HashSet<VRage.ModAPI.IMyEntity> set3;
                    if (!dictionary3.TryGet(pair.Key, out set3))
                    {
                        continue;
                    }
                    set3.Remove(pair2.Key);
                    if (set3.Count == 0)
                    {
                        dictionary3.Remove(pair.Key);
                    }
                }
            }
            foreach (KeyValuePair<MyCubeGrid, HashSet<VRage.ModAPI.IMyEntity>> pair3 in dictionary.Reverse<KeyValuePair<MyCubeGrid, HashSet<VRage.ModAPI.IMyEntity>>>())
            {
                if (pair3.Key.Physics == null)
                {
                    continue;
                }
                dictionary2[pair3.Key] = new Tuple<Vector3, Vector3>(pair3.Key.Physics.LinearVelocity, pair3.Key.Physics.AngularVelocity);
                enumerator = pair3.Value.Reverse<VRage.ModAPI.IMyEntity>().GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        VRage.ModAPI.IMyEntity current = enumerator.Current;
                        if ((current.Physics != null) && ((current.Physics is MyPhysicsBody) && !((MyPhysicsBody) current.Physics).IsWelded))
                        {
                            if (current.Physics.Enabled)
                            {
                                current.Physics.Enabled = false;
                                continue;
                            }
                            set.Add(current);
                        }
                    }
                }
                finally
                {
                    if (enumerator == null)
                    {
                        continue;
                    }
                    enumerator.Dispose();
                }
            }
            Vector3D vectord = worldMatrix.Translation - base.PositionComp.GetPosition();
            foreach (KeyValuePair<MyCubeGrid, HashSet<VRage.ModAPI.IMyEntity>> pair4 in dictionary)
            {
                HashSet<VRage.ModAPI.IMyEntity> set4;
                MatrixD xd2 = pair4.Key.PositionComp.WorldMatrix;
                MatrixD* xdPtr1 = (MatrixD*) ref xd2;
                xdPtr1.Translation += vectord;
                pair4.Key.PositionComp.SetWorldMatrix(xd2, source, false, true, true, true, false, false);
                if (dictionary3.TryGet(pair4.Key, out set4))
                {
                    foreach (VRage.ModAPI.IMyEntity local2 in set4)
                    {
                        MatrixD xd3 = local2.PositionComp.WorldMatrix;
                        MatrixD* xdPtr2 = (MatrixD*) ref xd3;
                        xdPtr2.Translation += vectord;
                        local2.PositionComp.SetWorldMatrix(xd3, source, false, true, true, true, false, false);
                    }
                }
            }
            BoundingBoxD xd = BoundingBoxD.CreateInvalid();
            foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node node2 in group.Nodes)
            {
                xd.Include(node2.NodeData.PositionComp.WorldAABB);
            }
            MyPhysics.EnsurePhysicsSpace(xd.GetInflated(MyClusterTree.MinimumDistanceFromBorder));
            HkWorld havokWorld = null;
            foreach (KeyValuePair<MyCubeGrid, HashSet<VRage.ModAPI.IMyEntity>> pair5 in dictionary)
            {
                if (pair5.Key.Physics != null)
                {
                    foreach (VRage.ModAPI.IMyEntity entity6 in pair5.Value)
                    {
                        if (entity6.Physics == null)
                        {
                            continue;
                        }
                        if (!((MyPhysicsBody) entity6.Physics).IsWelded && !set.Contains(entity6))
                        {
                            ((MyPhysicsBody) entity6.Physics).LinearVelocity = dictionary2[pair5.Key].Item1;
                            ((MyPhysicsBody) entity6.Physics).AngularVelocity = dictionary2[pair5.Key].Item2;
                            ((MyPhysicsBody) entity6.Physics).EnableBatched();
                            if (havokWorld == null)
                            {
                                havokWorld = ((MyPhysicsBody) entity6.Physics).HavokWorld;
                            }
                        }
                    }
                }
            }
            if (havokWorld != null)
            {
                havokWorld.FinishBatch();
            }
            foreach (KeyValuePair<MyCubeGrid, HashSet<VRage.ModAPI.IMyEntity>> pair6 in dictionary.Reverse<KeyValuePair<MyCubeGrid, HashSet<VRage.ModAPI.IMyEntity>>>())
            {
                if (pair6.Key.Physics == null)
                {
                    continue;
                }
                enumerator = pair6.Value.Reverse<VRage.ModAPI.IMyEntity>().GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        VRage.ModAPI.IMyEntity current = enumerator.Current;
                        if ((current.Physics != null) && ((current.Physics is MyPhysicsBody) && (!((MyPhysicsBody) current.Physics).IsWelded && !set.Contains(current))))
                        {
                            ((MyPhysicsBody) current.Physics).FinishAddBatch();
                        }
                    }
                }
                finally
                {
                    if (enumerator == null)
                    {
                        continue;
                    }
                    enumerator.Dispose();
                }
            }
        }

        public static bool TestBlockPlacementArea(MyCubeBlockDefinition blockDefinition, MyBlockOrientation? blockOrientation, MatrixD worldMatrix, ref MyGridPlacementSettings settings, BoundingBoxD localAabb, bool dynamicBuildMode, VRage.Game.Entity.MyEntity ignoredEntity = null, bool testVoxel = true)
        {
            MyCubeGrid grid;
            if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(worldMatrix.Translation))
            {
                return false;
            }
            Vector3 halfExtents = (Vector3) (localAabb.HalfExtents + settings.SearchHalfExtentsDeltaAbsolute);
            if (MyFakes.ENABLE_BLOCK_PLACING_IN_OCCUPIED_AREA)
            {
                halfExtents -= new Vector3D(0.11);
            }
            Vector3D center = localAabb.TransformFast(ref worldMatrix).Center;
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(worldMatrix);
            rotation.Normalize();
            MyGridPlacementSettings settingsCopy = settings;
            if (dynamicBuildMode && (blockDefinition.CubeSize == MyCubeSize.Large))
            {
                VoxelPlacementSettings settings3 = new VoxelPlacementSettings {
                    PlacementMode = VoxelPlacementMode.Both
                };
                settingsCopy.VoxelPlacement = new VoxelPlacementSettings?(settings3);
            }
            if (testVoxel && !TestVoxelPlacement(blockDefinition, settingsCopy, dynamicBuildMode, worldMatrix, localAabb))
            {
                return false;
            }
            MyPhysics.GetPenetrationsBox(ref halfExtents, ref center, ref rotation, m_physicsBoxQueryList, 7);
            m_lastQueryBox.Value.HalfExtents = halfExtents;
            m_lastQueryTransform = MatrixD.CreateFromQuaternion(rotation);
            m_lastQueryTransform.Translation = center;
            return TestPlacementAreaInternal(null, ref settingsCopy, blockDefinition, blockOrientation, ref localAabb, ignoredEntity, ref worldMatrix, out grid, dynamicBuildMode, false);
        }

        public static bool TestBlockPlacementArea(MyCubeGrid targetGrid, ref MyGridPlacementSettings settings, MyBlockOrientation blockOrientation, MyCubeBlockDefinition blockDefinition, ref Vector3D translation, ref Quaternion rotation, ref Vector3 halfExtents, ref BoundingBoxD localAabb, VRage.Game.Entity.MyEntity ignoredEntity = null)
        {
            MyCubeGrid grid;
            return TestBlockPlacementArea(targetGrid, ref settings, blockOrientation, blockDefinition, ref translation, ref rotation, ref halfExtents, ref localAabb, out grid, ignoredEntity, false, true);
        }

        public static bool TestBlockPlacementArea(MyCubeGrid targetGrid, ref MyGridPlacementSettings settings, MyBlockOrientation blockOrientation, MyCubeBlockDefinition blockDefinition, ref Vector3D translationObsolete, ref Quaternion rotation, ref Vector3 halfExtentsObsolete, ref BoundingBoxD localAabb, out MyCubeGrid touchingGrid, VRage.Game.Entity.MyEntity ignoredEntity = null, bool ignoreFracturedPieces = false, bool testVoxel = true)
        {
            touchingGrid = null;
            MatrixD m = (targetGrid != null) ? targetGrid.WorldMatrix : MatrixD.Identity;
            if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(m.Translation))
            {
                return false;
            }
            Vector3 halfExtents = (Vector3) (localAabb.HalfExtents + settings.SearchHalfExtentsDeltaAbsolute);
            if (MyFakes.ENABLE_BLOCK_PLACING_IN_OCCUPIED_AREA)
            {
                halfExtents -= new Vector3D(0.11);
            }
            Vector3D center = localAabb.TransformFast(ref m).Center;
            Quaternion.CreateFromRotationMatrix(m).Normalize();
            if ((testVoxel && (settings.VoxelPlacement != null)) && (settings.VoxelPlacement.Value.PlacementMode != VoxelPlacementMode.Both))
            {
                bool flag = IsAabbInsideVoxel(m, localAabb, settings);
                if (settings.VoxelPlacement.Value.PlacementMode == VoxelPlacementMode.InVoxel)
                {
                    flag = !flag;
                }
                if (flag)
                {
                    return false;
                }
            }
            if (!MySessionComponentSafeZones.IsActionAllowed(localAabb.TransformFast(ref m), MySafeZoneAction.Building, 0L))
            {
                return false;
            }
            if ((blockDefinition == null) || !blockDefinition.UseModelIntersection)
            {
                MyPhysics.GetPenetrationsBox(ref halfExtents, ref center, ref rotation, m_physicsBoxQueryList, 7);
            }
            else
            {
                MyModel modelOnlyData = MyModels.GetModelOnlyData(blockDefinition.Model);
                if (modelOnlyData != null)
                {
                    bool flag2;
                    modelOnlyData.CheckLoadingErrors(blockDefinition.Context, out flag2);
                    if (flag2)
                    {
                        MyDefinitionErrors.Add(blockDefinition.Context, "There was error during loading of model, please check log file.", TErrorSeverity.Error, true);
                    }
                }
                if ((modelOnlyData == null) || (modelOnlyData.HavokCollisionShapes == null))
                {
                    MyPhysics.GetPenetrationsBox(ref halfExtents, ref center, ref rotation, m_physicsBoxQueryList, 7);
                }
                else
                {
                    Matrix matrix;
                    Vector3 vector2;
                    blockOrientation.GetMatrix(out matrix);
                    Vector3.TransformNormal(ref blockDefinition.ModelOffset, ref matrix, out vector2);
                    center += vector2;
                    int length = modelOnlyData.HavokCollisionShapes.Length;
                    HkShape[] shapes = new HkShape[length];
                    int index = 0;
                    while (true)
                    {
                        if (index >= length)
                        {
                            HkListShape shape = new HkListShape(shapes, length, HkReferencePolicy.None);
                            Quaternion quaternion2 = Quaternion.CreateFromForwardUp(Base6Directions.GetVector(blockOrientation.Forward), Base6Directions.GetVector(blockOrientation.Up));
                            rotation *= quaternion2;
                            MyPhysics.GetPenetrationsShape((HkShape) shape, ref center, ref rotation, m_physicsBoxQueryList, 7);
                            shape.Base.RemoveReference();
                            break;
                        }
                        shapes[index] = modelOnlyData.HavokCollisionShapes[index];
                        index++;
                    }
                }
            }
            m_lastQueryBox.Value.HalfExtents = halfExtents;
            m_lastQueryTransform = MatrixD.CreateFromQuaternion(rotation);
            m_lastQueryTransform.Translation = center;
            return TestPlacementAreaInternal(targetGrid, ref settings, blockDefinition, new MyBlockOrientation?(blockOrientation), ref localAabb, ignoredEntity, ref m, out touchingGrid, false, ignoreFracturedPieces);
        }

        private static void TestGridPlacement(ref MyGridPlacementSettings settings, ref MatrixD worldMatrix, ref MyCubeGrid touchingGrid, float gridSize, bool isStatic, ref BoundingBoxD localAABB, MyCubeBlockDefinition blockDefinition, MyBlockOrientation? blockOrientation, ref bool entityOverlap, ref bool touchingStaticGrid, MyCubeGrid grid)
        {
            BoundingBoxD xd = localAABB.TransformFast(ref worldMatrix);
            MatrixD worldMatrixNormalizedInv = grid.PositionComp.WorldMatrixNormalizedInv;
            xd.TransformFast(ref worldMatrixNormalizedInv);
            Vector3D position = Vector3D.Transform(localAABB.Min, (MatrixD) worldMatrix);
            Vector3D vectord2 = Vector3D.Transform(position, worldMatrixNormalizedInv);
            Vector3D vectord3 = Vector3D.Transform(Vector3D.Transform(localAABB.Max, (MatrixD) worldMatrix), worldMatrixNormalizedInv);
            Vector3D vectord4 = Vector3D.Min(vectord2, vectord3);
            Vector3D vectord5 = (vectord4 + (gridSize / 2f)) / ((double) grid.GridSize);
            Vector3I vectori = Vector3I.Round(vectord5);
            Vector3I vectori2 = Vector3I.Round((Vector3D.Max(vectord2, vectord3) - (gridSize / 2f)) / ((double) grid.GridSize));
            Vector3I min = Vector3I.Min(vectori, vectori2);
            Vector3I max = Vector3I.Max(vectori, vectori2);
            MyBlockOrientation? orientation = null;
            if (((MyFakes.ENABLE_COMPOUND_BLOCKS & isStatic) && grid.IsStatic) && (blockOrientation != null))
            {
                Matrix matrix;
                blockOrientation.Value.GetMatrix(out matrix);
                Matrix rotation = (matrix * worldMatrix) * worldMatrixNormalizedInv;
                rotation.Translation = Vector3.Zero;
                Base6Directions.Direction forward = Base6Directions.GetForward(ref rotation);
                Base6Directions.Direction up = Base6Directions.GetUp(ref rotation);
                if (Base6Directions.IsValidBlockOrientation(forward, up))
                {
                    orientation = new MyBlockOrientation(forward, up);
                }
            }
            if (!grid.CanAddCubes(min, max, orientation, blockDefinition))
            {
                entityOverlap = true;
            }
            else if (settings.CanAnchorToStaticGrid && grid.IsTouchingAnyNeighbor(min, max))
            {
                touchingStaticGrid = true;
                if (touchingGrid == null)
                {
                    touchingGrid = grid;
                }
            }
        }

        public static bool TestPlacementArea(MyCubeGrid targetGrid, ref MyGridPlacementSettings settings, BoundingBoxD localAabb, bool dynamicBuildMode, VRage.Game.Entity.MyEntity ignoredEntity = null)
        {
            MyCubeGrid grid;
            MatrixD worldMatrix = targetGrid.WorldMatrix;
            if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(worldMatrix.Translation))
            {
                return false;
            }
            Vector3 halfExtents = (Vector3) (localAabb.HalfExtents + settings.SearchHalfExtentsDeltaAbsolute);
            if (MyFakes.ENABLE_BLOCK_PLACING_IN_OCCUPIED_AREA)
            {
                halfExtents -= new Vector3D(0.11);
            }
            Vector3D center = localAabb.TransformFast(ref worldMatrix).Center;
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(worldMatrix);
            rotation.Normalize();
            MyPhysics.GetPenetrationsBox(ref halfExtents, ref center, ref rotation, m_physicsBoxQueryList, 0x12);
            m_lastQueryBox.Value.HalfExtents = halfExtents;
            m_lastQueryTransform = MatrixD.CreateFromQuaternion(rotation);
            m_lastQueryTransform.Translation = center;
            MyBlockOrientation? blockOrientation = null;
            return TestPlacementAreaInternal(targetGrid, ref settings, null, blockOrientation, ref localAabb, ignoredEntity, ref worldMatrix, out grid, dynamicBuildMode, false);
        }

        public static bool TestPlacementArea(MyCubeGrid targetGrid, bool targetGridIsStatic, ref MyGridPlacementSettings settings, BoundingBoxD localAabb, bool dynamicBuildMode, VRage.Game.Entity.MyEntity ignoredEntity = null, bool testVoxel = true, bool testPhysics = true)
        {
            MatrixD worldMatrix = targetGrid.WorldMatrix;
            if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(worldMatrix.Translation))
            {
                return false;
            }
            Vector3 halfExtents = (Vector3) (localAabb.HalfExtents + settings.SearchHalfExtentsDeltaAbsolute);
            if (MyFakes.ENABLE_BLOCK_PLACING_IN_OCCUPIED_AREA)
            {
                halfExtents -= new Vector3D(0.11);
            }
            Vector3D center = localAabb.TransformFast(ref worldMatrix).Center;
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(worldMatrix);
            rotation.Normalize();
            if ((testVoxel && (settings.VoxelPlacement != null)) && (settings.VoxelPlacement.Value.PlacementMode != VoxelPlacementMode.Both))
            {
                bool flag2 = IsAabbInsideVoxel(worldMatrix, localAabb, settings);
                if (settings.VoxelPlacement.Value.PlacementMode == VoxelPlacementMode.InVoxel)
                {
                    flag2 = !flag2;
                }
                if (flag2)
                {
                    return false;
                }
            }
            bool flag = true;
            if (testPhysics)
            {
                MyCubeGrid grid;
                MyPhysics.GetPenetrationsBox(ref halfExtents, ref center, ref rotation, m_physicsBoxQueryList, 7);
                m_lastQueryBox.Value.HalfExtents = halfExtents;
                m_lastQueryTransform = MatrixD.CreateFromQuaternion(rotation);
                m_lastQueryTransform.Translation = center;
                MyBlockOrientation? blockOrientation = null;
                flag = TestPlacementAreaInternal(targetGrid, targetGridIsStatic, ref settings, null, blockOrientation, ref localAabb, ignoredEntity, ref worldMatrix, out grid, dynamicBuildMode, false);
            }
            return flag;
        }

        public static bool TestPlacementAreaCube(MyCubeGrid targetGrid, ref MyGridPlacementSettings settings, Vector3I min, Vector3I max, MyBlockOrientation blockOrientation, MyCubeBlockDefinition blockDefinition, VRage.Game.Entity.MyEntity ignoredEntity = null, bool ignoreFracturedPieces = false)
        {
            MyCubeGrid touchingGrid = null;
            return TestPlacementAreaCube(targetGrid, ref settings, min, max, blockOrientation, blockDefinition, out touchingGrid, ignoredEntity, ignoreFracturedPieces);
        }

        public static bool TestPlacementAreaCube(MyCubeGrid targetGrid, ref MyGridPlacementSettings settings, Vector3I min, Vector3I max, MyBlockOrientation blockOrientation, MyCubeBlockDefinition blockDefinition, out MyCubeGrid touchingGrid, VRage.Game.Entity.MyEntity ignoredEntity = null, bool ignoreFracturedPieces = false)
        {
            touchingGrid = null;
            MatrixD xd = (targetGrid != null) ? targetGrid.WorldMatrix : MatrixD.Identity;
            if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(xd.Translation))
            {
                return false;
            }
            float num = (targetGrid != null) ? targetGrid.GridSize : MyDefinitionManager.Static.GetCubeSize(MyCubeSize.Large);
            Vector3 halfExtentsObsolete = (((max - min) * num) + num) / 2f;
            halfExtentsObsolete = !MyFakes.ENABLE_BLOCK_PLACING_IN_OCCUPIED_AREA ? (halfExtentsObsolete - new Vector3(0.03f, 0.03f, 0.03f)) : (halfExtentsObsolete - new Vector3D(0.11));
            MatrixD matrix = MatrixD.CreateTranslation(((max + min) * 0.5f) * num) * xd;
            BoundingBoxD localAabb = BoundingBoxD.CreateInvalid();
            localAabb.Include((Vector3D) ((min * num) - (num / 2f)));
            localAabb.Include((max * num) + (num / 2f));
            Vector3D translation = matrix.Translation;
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(matrix);
            return TestBlockPlacementArea(targetGrid, ref settings, blockOrientation, blockDefinition, ref translation, ref rotation, ref halfExtentsObsolete, ref localAabb, out touchingGrid, ignoredEntity, ignoreFracturedPieces, true);
        }

        public static bool TestPlacementAreaCubeNoAABBInflate(MyCubeGrid targetGrid, ref MyGridPlacementSettings settings, Vector3I min, Vector3I max, MyBlockOrientation blockOrientation, MyCubeBlockDefinition blockDefinition, out MyCubeGrid touchingGrid, VRage.Game.Entity.MyEntity ignoredEntity = null)
        {
            touchingGrid = null;
            MatrixD xd = (targetGrid != null) ? targetGrid.WorldMatrix : MatrixD.Identity;
            if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(xd.Translation))
            {
                return false;
            }
            float num = (targetGrid != null) ? targetGrid.GridSize : MyDefinitionManager.Static.GetCubeSize(MyCubeSize.Large);
            Vector3 halfExtentsObsolete = (((max - min) * num) + num) / 2f;
            MatrixD matrix = MatrixD.CreateTranslation(((max + min) * 0.5f) * num) * xd;
            BoundingBoxD localAabb = BoundingBoxD.CreateInvalid();
            localAabb.Include((Vector3D) ((min * num) - (num / 2f)));
            localAabb.Include((max * num) + (num / 2f));
            Vector3D translation = matrix.Translation;
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(matrix);
            return TestBlockPlacementArea(targetGrid, ref settings, blockOrientation, blockDefinition, ref translation, ref rotation, ref halfExtentsObsolete, ref localAabb, out touchingGrid, ignoredEntity, false, true);
        }

        private static bool TestPlacementAreaInternal(MyCubeGrid targetGrid, ref MyGridPlacementSettings settings, MyCubeBlockDefinition blockDefinition, MyBlockOrientation? blockOrientation, ref BoundingBoxD localAabb, VRage.Game.Entity.MyEntity ignoredEntity, ref MatrixD worldMatrix, out MyCubeGrid touchingGrid, bool dynamicBuildMode = false, bool ignoreFracturedPieces = false) => 
            TestPlacementAreaInternal(targetGrid, (targetGrid != null) ? targetGrid.IsStatic : !dynamicBuildMode, ref settings, blockDefinition, blockOrientation, ref localAabb, ignoredEntity, ref worldMatrix, out touchingGrid, dynamicBuildMode, ignoreFracturedPieces);

        private static bool TestPlacementAreaInternal(MyCubeGrid targetGrid, bool targetGridIsStatic, ref MyGridPlacementSettings settings, MyCubeBlockDefinition blockDefinition, MyBlockOrientation? blockOrientation, ref BoundingBoxD localAabb, VRage.Game.Entity.MyEntity ignoredEntity, ref MatrixD worldMatrix, out MyCubeGrid touchingGrid, bool dynamicBuildMode = false, bool ignoreFracturedPieces = false)
        {
            touchingGrid = null;
            float gridSize = (targetGrid != null) ? targetGrid.GridSize : ((blockDefinition != null) ? MyDefinitionManager.Static.GetCubeSize(blockDefinition.CubeSize) : MyDefinitionManager.Static.GetCubeSize(MyCubeSize.Large));
            bool isStatic = targetGridIsStatic;
            bool entityOverlap = false;
            bool touchingStaticGrid = false;
            using (List<HkBodyCollision>.Enumerator enumerator = m_physicsBoxQueryList.GetEnumerator())
            {
                MyCubeGrid topMostParent;
                goto TR_0035;
            TR_0018:
                if ((isStatic != topMostParent.IsStatic) || (gridSize == topMostParent.GridSize))
                {
                    if (IsOrientationsAligned(topMostParent.WorldMatrix, worldMatrix))
                    {
                        TestGridPlacement(ref settings, ref worldMatrix, ref touchingGrid, gridSize, isStatic, ref localAabb, blockDefinition, blockOrientation, ref entityOverlap, ref touchingStaticGrid, topMostParent);
                        if (entityOverlap)
                        {
                            goto TR_0000;
                        }
                    }
                    else
                    {
                        entityOverlap = true;
                    }
                }
                goto TR_0035;
            TR_0019:
                entityOverlap = true;
                goto TR_0000;
            TR_0035:
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        VRage.Game.Entity.MyEntity objA = enumerator.Current.Body.GetEntity(0) as VRage.Game.Entity.MyEntity;
                        if (objA == null)
                        {
                            continue;
                        }
                        if (objA.GetTopMostParent(null).GetPhysicsBody() == null)
                        {
                            continue;
                        }
                        if (ignoreFracturedPieces && (objA is MyFracturedPiece))
                        {
                            continue;
                        }
                        if (((objA.GetTopMostParent(null).GetPhysicsBody().WeldInfo.Children.Count == 0) && (ignoredEntity != null)) && (ReferenceEquals(objA, ignoredEntity) || ReferenceEquals(objA.GetTopMostParent(null), ignoredEntity)))
                        {
                            continue;
                        }
                        MyPhysicsComponentBase physics = objA.GetTopMostParent(null).Physics;
                        if ((physics != null) && physics.IsPhantom)
                        {
                            continue;
                        }
                        topMostParent = objA.GetTopMostParent(null) as MyCubeGrid;
                        if (objA.GetTopMostParent(null).GetPhysicsBody().WeldInfo.Children.Count <= 0)
                        {
                            if ((topMostParent != null) && (((isStatic && topMostParent.IsStatic) || ((MyFakes.ENABLE_DYNAMIC_SMALL_GRID_MERGING && (!isStatic && (!topMostParent.IsStatic && (blockDefinition != null)))) && (blockDefinition.CubeSize == topMostParent.GridSizeEnum))) || ((isStatic && (topMostParent.IsStatic && (blockDefinition != null))) && (blockDefinition.CubeSize == topMostParent.GridSizeEnum))))
                            {
                                break;
                            }
                            goto TR_0019;
                        }
                        else
                        {
                            if (!ReferenceEquals(objA, ignoredEntity) && TestQueryIntersection(objA.GetPhysicsBody().GetShape(), objA.WorldMatrix))
                            {
                                entityOverlap = true;
                                if (touchingGrid == null)
                                {
                                    touchingGrid = objA as MyCubeGrid;
                                }
                                goto TR_0000;
                            }
                            foreach (MyPhysicsBody body in objA.GetPhysicsBody().WeldInfo.Children)
                            {
                                if (!ReferenceEquals(body.Entity, ignoredEntity) && TestQueryIntersection(body.WeldedRigidBody.GetShape(), body.Entity.WorldMatrix))
                                {
                                    if (touchingGrid == null)
                                    {
                                        touchingGrid = body.Entity as MyCubeGrid;
                                    }
                                    entityOverlap = true;
                                    break;
                                }
                            }
                            if (entityOverlap)
                            {
                                goto TR_0000;
                            }
                        }
                        continue;
                    }
                    else
                    {
                        goto TR_0000;
                    }
                    break;
                }
                goto TR_0018;
            }
        TR_0000:
            m_tmpResultList.Clear();
            m_physicsBoxQueryList.Clear();
            return !entityOverlap;
        }

        private static bool TestPlacementAreaInternalWithEntities(MyCubeGrid targetGrid, bool targetGridIsStatic, ref MyGridPlacementSettings settings, ref BoundingBoxD localAabb, VRage.Game.Entity.MyEntity ignoredEntity, ref MatrixD worldMatrix, bool dynamicBuildMode = false)
        {
            MyCubeGrid touchingGrid = null;
            float gridSize = targetGrid.GridSize;
            bool isStatic = targetGridIsStatic;
            localAabb.TransformFast(ref worldMatrix);
            bool entityOverlap = false;
            bool touchingStaticGrid = false;
            foreach (VRage.Game.Entity.MyEntity entity in m_tmpResultList)
            {
                if (ignoredEntity != null)
                {
                    if (ReferenceEquals(entity, ignoredEntity))
                    {
                        continue;
                    }
                    if (ReferenceEquals(entity.GetTopMostParent(null), ignoredEntity))
                    {
                        continue;
                    }
                }
                if (entity.Physics != null)
                {
                    MyCubeGrid grid = entity as MyCubeGrid;
                    if (grid == null)
                    {
                        MyCharacter character = entity as MyCharacter;
                        if ((character != null) && character.PositionComp.WorldAABB.Intersects(targetGrid.PositionComp.WorldAABB))
                        {
                            entityOverlap = true;
                            break;
                        }
                    }
                    else if ((isStatic != grid.IsStatic) || (gridSize == grid.GridSize))
                    {
                        MyBlockOrientation? blockOrientation = null;
                        TestGridPlacement(ref settings, ref worldMatrix, ref touchingGrid, gridSize, isStatic, ref localAabb, null, blockOrientation, ref entityOverlap, ref touchingStaticGrid, grid);
                        if (entityOverlap)
                        {
                            break;
                        }
                    }
                }
            }
            m_tmpResultList.Clear();
            if (entityOverlap)
            {
                return false;
            }
            bool flag1 = targetGrid.IsStatic;
            return true;
        }

        public static bool TestPlacementVoxelMapOverlap(MyVoxelBase voxelMap, ref MyGridPlacementSettings settings, ref BoundingBoxD localAabb, ref MatrixD worldMatrix, bool touchingStaticGrid = false)
        {
            BoundingBoxD boundingBox = localAabb.TransformFast(ref worldMatrix);
            int num = 2;
            if (voxelMap == null)
            {
                voxelMap = MySession.Static.VoxelMaps.GetVoxelMapWhoseBoundingBoxIntersectsBox(ref boundingBox, null);
            }
            if ((voxelMap != null) && voxelMap.IsAnyAabbCornerInside(ref worldMatrix, localAabb))
            {
                num = 1;
            }
            bool flag = true;
            if (num == 1)
            {
                flag = settings.VoxelPlacement.Value.PlacementMode == VoxelPlacementMode.Both;
            }
            else if (num == 2)
            {
                flag = (settings.VoxelPlacement.Value.PlacementMode == VoxelPlacementMode.OutsideVoxel) || (settings.CanAnchorToStaticGrid & touchingStaticGrid);
            }
            return flag;
        }

        private static bool TestPlacementVoxelMapPenetration(MyVoxelBase voxelMap, MyGridPlacementSettings settings, ref BoundingBoxD localAabb, ref MatrixD worldMatrix, bool touchingStaticGrid = false)
        {
            float num = 0f;
            if (voxelMap != null)
            {
                MyTuple<float, float> tuple = voxelMap.GetVoxelContentInBoundingBox_Fast(localAabb, worldMatrix);
                double volume = localAabb.Volume;
                num = tuple.Item2.IsValid() ? tuple.Item2 : 0f;
            }
            return ((num <= settings.VoxelPlacement.Value.MaxAllowed) && ((num >= settings.VoxelPlacement.Value.MinAllowed) || (settings.CanAnchorToStaticGrid & touchingStaticGrid)));
        }

        private static unsafe bool TestQueryIntersection(HkShape shape, MatrixD transform)
        {
            MatrixD lastQueryTransform = m_lastQueryTransform;
            MatrixD xd2 = transform;
            MatrixD* xdPtr1 = (MatrixD*) ref xd2;
            xdPtr1.Translation = xd2.Translation - lastQueryTransform.Translation;
            lastQueryTransform.Translation = Vector3D.Zero;
            Matrix matrix = (Matrix) lastQueryTransform;
            Matrix matrix2 = (Matrix) xd2;
            return MyPhysics.IsPenetratingShapeShape(m_lastQueryBox.Value, ref matrix, shape, ref matrix2);
        }

        public static bool TestVoxelPlacement(MyCubeBlockDefinition blockDefinition, MyGridPlacementSettings settingsCopy, bool dynamicBuildMode, MatrixD worldMatrix, BoundingBoxD localAabb)
        {
            if (blockDefinition.VoxelPlacement != null)
            {
                settingsCopy.VoxelPlacement = new VoxelPlacementSettings?(dynamicBuildMode ? blockDefinition.VoxelPlacement.Value.DynamicMode : blockDefinition.VoxelPlacement.Value.StaticMode);
            }
            if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(worldMatrix.Translation))
            {
                return false;
            }
            if (settingsCopy.VoxelPlacement.Value.PlacementMode == VoxelPlacementMode.None)
            {
                return false;
            }
            if (settingsCopy.VoxelPlacement.Value.PlacementMode == VoxelPlacementMode.Both)
            {
                return true;
            }
            bool flag = IsAabbInsideVoxel(worldMatrix, localAabb, settingsCopy);
            if (settingsCopy.VoxelPlacement.Value.PlacementMode == VoxelPlacementMode.InVoxel)
            {
                flag = !flag;
            }
            return !flag;
        }

        public override string ToString()
        {
            string str = this.IsStatic ? "S" : "D";
            string str2 = this.GridSizeEnum.ToString();
            object[] objArray1 = new object[9];
            objArray1[0] = "Grid_";
            objArray1[1] = str;
            objArray1[2] = "_";
            objArray1[3] = str2;
            objArray1[4] = "_";
            objArray1[5] = this.m_cubeBlocks.Count;
            objArray1[6] = " {";
            objArray1[7] = base.EntityId.ToString("X8");
            objArray1[8] = "}";
            return string.Concat(objArray1);
        }

        public void TransferBlockLimitsBuiltByID(long author, MyBlockLimits oldLimits, MyBlockLimits newLimits)
        {
            using (HashSet<MySlimBlock>.Enumerator enumerator = this.FindBlocksBuiltByID(author).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.TransferLimits(oldLimits, newLimits);
                }
            }
        }

        [Event(null, 0xe5b), Reliable, ServerInvoked, Broadcast]
        public void TransferBlocksBuiltByID(long oldAuthor, long newAuthor)
        {
            using (HashSet<MySlimBlock>.Enumerator enumerator = this.FindBlocksBuiltByID(oldAuthor).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.TransferAuthorship(newAuthor);
                }
            }
        }

        public void TransferBlocksBuiltByIDClient(long oldAuthor, long newAuthor)
        {
            using (HashSet<MySlimBlock>.Enumerator enumerator = this.FindBlocksBuiltByID(oldAuthor).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.TransferAuthorshipClient(newAuthor);
                }
            }
        }

        private void TransformCubeToGrid(ref MyIntersectionResultLineTriangleEx triangle, ref Matrix cubeLocalMatrix, ref MatrixD? cubeWorldMatrix)
        {
            if (cubeWorldMatrix == 0)
            {
                MatrixD worldMatrix = base.WorldMatrix;
                triangle.IntersectionPointInObjectSpace = Vector3.Transform(triangle.IntersectionPointInObjectSpace, ref cubeLocalMatrix);
                triangle.IntersectionPointInWorldSpace = Vector3D.Transform(triangle.IntersectionPointInObjectSpace, worldMatrix);
                triangle.NormalInObjectSpace = Vector3.TransformNormal(triangle.NormalInObjectSpace, ref cubeLocalMatrix);
                triangle.NormalInWorldSpace = Vector3.TransformNormal(triangle.NormalInObjectSpace, worldMatrix);
            }
            else
            {
                Vector3 intersectionPointInObjectSpace = triangle.IntersectionPointInObjectSpace;
                Vector3 normalInObjectSpace = triangle.NormalInObjectSpace;
                triangle.IntersectionPointInObjectSpace = Vector3.Transform(intersectionPointInObjectSpace, ref cubeLocalMatrix);
                triangle.IntersectionPointInWorldSpace = Vector3D.Transform(intersectionPointInObjectSpace, cubeWorldMatrix.Value);
                triangle.NormalInObjectSpace = Vector3.TransformNormal(normalInObjectSpace, ref cubeLocalMatrix);
                triangle.NormalInWorldSpace = Vector3.TransformNormal(normalInObjectSpace, cubeWorldMatrix.Value);
            }
            triangle.Triangle.InputTriangle.Transform(ref cubeLocalMatrix);
        }

        public static unsafe void TransformMountPoints(List<MyCubeBlockDefinition.MountPoint> outMountPoints, MyCubeBlockDefinition def, MyCubeBlockDefinition.MountPoint[] mountPoints, ref MyBlockOrientation orientation)
        {
            outMountPoints.Clear();
            if (mountPoints != null)
            {
                Matrix matrix;
                orientation.GetMatrix(out matrix);
                Vector3I center = def.Center;
                for (int i = 0; i < mountPoints.Length; i++)
                {
                    MyCubeBlockDefinition.MountPoint point = mountPoints[i];
                    MyCubeBlockDefinition.MountPoint item = new MyCubeBlockDefinition.MountPoint();
                    Vector3 position = point.Start - center;
                    Vector3 vector2 = point.End - center;
                    Vector3I.Transform(ref point.Normal, ref matrix, out item.Normal);
                    Vector3.Transform(ref position, ref matrix, out item.Start);
                    Vector3.Transform(ref vector2, ref matrix, out item.End);
                    item.ExclusionMask = point.ExclusionMask;
                    item.PropertiesMask = point.PropertiesMask;
                    item.Enabled = point.Enabled;
                    Vector3I result = Vector3I.Floor(point.Start) - center;
                    Vector3I vectori3 = Vector3I.Floor(point.End) - center;
                    Vector3I* vectoriPtr1 = (Vector3I*) ref result;
                    Vector3I.Transform(ref (Vector3I) ref vectoriPtr1, ref matrix, out result);
                    Vector3I* vectoriPtr2 = (Vector3I*) ref vectori3;
                    Vector3I.Transform(ref (Vector3I) ref vectoriPtr2, ref matrix, out vectori3);
                    Vector3I vectori4 = Vector3I.Floor(item.Start);
                    Vector3I vectori5 = Vector3I.Floor(item.End);
                    Vector3I vectori6 = result - vectori4;
                    Vector3I vectori7 = vectori3 - vectori5;
                    Vector3* vectorPtr1 = (Vector3*) ref item.Start;
                    vectorPtr1[0] += vectori6;
                    Vector3* vectorPtr2 = (Vector3*) ref item.End;
                    vectorPtr2[0] += vectori7;
                    outMountPoints.Add(item);
                }
            }
        }

        [Event(null, 0x2645), Reliable, Server]
        public static void TryCreateGrid_Implementation(MyCubeSize cubeSize, bool isStatic, MyPositionAndOrientation position, long inventoryEntityId, bool instantBuild)
        {
            string str;
            bool flag = MyEventContext.Current.IsLocallyInvoked || MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value);
            MyDefinitionManager.Static.GetBaseBlockPrefabName(cubeSize, isStatic, MySession.Static.CreativeMode || (instantBuild & flag), out str);
            if (str != null)
            {
                MyObjectBuilder_CubeGrid[] gridPrefab = MyPrefabManager.Static.GetGridPrefab(str);
                if ((gridPrefab != null) && (gridPrefab.Length != 0))
                {
                    MyObjectBuilder_CubeGrid[] gridArray2 = gridPrefab;
                    int index = 0;
                    while (true)
                    {
                        if (index >= gridArray2.Length)
                        {
                            Sandbox.Game.Entities.MyEntities.RemapObjectBuilderCollection(gridPrefab);
                            if (!(instantBuild & flag))
                            {
                                VRage.Game.Entity.MyEntity entity;
                                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(inventoryEntityId, out entity, false) && (entity != null))
                                {
                                    MyInventoryBase builderInventory = MyCubeBuilder.BuildComponent.GetBuilderInventory(entity);
                                    if (builderInventory != null)
                                    {
                                        MyGridClipboard.CalculateItemRequirements(gridPrefab, m_buildComponents);
                                        foreach (KeyValuePair<MyDefinitionId, int> pair in m_buildComponents.TotalMaterials)
                                        {
                                            builderInventory.RemoveItemsOfType(pair.Value, pair.Key, MyItemFlags.None, false);
                                        }
                                    }
                                    break;
                                }
                                if (!flag && !MySession.Static.CreativeMode)
                                {
                                    (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                                    return;
                                }
                            }
                            break;
                        }
                        gridArray2[index].PositionAndOrientation = new MyPositionAndOrientation?(position);
                        index++;
                    }
                    List<MyCubeGrid> grids = new List<MyCubeGrid>();
                    foreach (MyObjectBuilder_CubeGrid grid in gridPrefab)
                    {
                        string[] textArray1 = new string[] { "CreateCompressedMsg: Type: ", grid.GetType().Name.ToString(), "  Name: ", grid.Name, "  EntityID: ", grid.EntityId.ToString("X8") };
                        MySandboxGame.Log.WriteLine(string.Concat(textArray1));
                        MyCubeGrid item = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilder(grid, false) as MyCubeGrid;
                        if (item != null)
                        {
                            grids.Add(item);
                            if (instantBuild & flag)
                            {
                                ChangeOwnership(inventoryEntityId, item);
                            }
                            string[] textArray2 = new string[] { "Status: Exists(", Sandbox.Game.Entities.MyEntities.EntityExists(grid.EntityId).ToString(), ") InScene(", ((grid.PersistentFlags & MyPersistentEntityFlags2.InScene) == MyPersistentEntityFlags2.InScene).ToString(), ")" };
                            MySandboxGame.Log.WriteLine(string.Concat(textArray2));
                        }
                    }
                    AfterPaste(grids, Vector3.Zero, false);
                }
            }
        }

        public bool TryGetCube(Vector3I position, out MyCube cube) => 
            this.m_cubes.TryGetValue(position, out cube);

        [Event(null, 0x27ac), Reliable, Server]
        public static void TryPasteGrid_Implementation(List<MyObjectBuilder_CubeGrid> entities, bool detectDisconnects, Vector3 objectVelocity, bool multiBlock, bool instantBuild, RelativeOffset offset)
        {
            MyLog.Default.WriteLineAndConsole("Pasting grid request by user: " + MyEventContext.Current.Sender.Value);
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsCopyPastingEnabledForUser(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                bool shouldRemoveScripts = !MySession.Static.IsUserScripter(MyEventContext.Current.Sender.Value);
                Vector3D? nullable = null;
                if (offset.Use && offset.RelativeToEntity)
                {
                    VRage.ModAPI.IMyEntity entity;
                    if (!MyEntityIdentifier.TryGetEntity(offset.SpawnerId, out entity, false))
                    {
                        return;
                    }
                    nullable = new Vector3D?((entity as VRage.Game.Entity.MyEntity).WorldMatrix.Translation - offset.OriginalSpawnPoint);
                }
                PasteGridData workData = new PasteGridData(entities, detectDisconnects, objectVelocity, multiBlock, instantBuild, shouldRemoveScripts, MyEventContext.Current.Sender, MyEventContext.Current.IsLocallyInvoked, nullable);
                if (MySandboxGame.Config.SyncRendering)
                {
                    MyEntityIdentifier.PrepareSwapData();
                    MyEntityIdentifier.SwapPerThreadData();
                }
                Parallel.Start(new Action<WorkData>(MyCubeGrid.TryPasteGrid_ImplementationInternal), new Action<WorkData>(MyCubeGrid.OnPasteCompleted), workData);
                if (MySandboxGame.Config.SyncRendering)
                {
                    MyEntityIdentifier.ClearSwapDataAndRestore();
                }
            }
        }

        private static void TryPasteGrid_ImplementationInternal(WorkData workData)
        {
            PasteGridData data = workData as PasteGridData;
            if (data == null)
            {
                workData.FlagAsFailed();
            }
            else
            {
                data.TryPasteGrid();
            }
        }

        public static bool TryRayCastGrid(ref LineD worldRay, out MyCubeGrid hitGrid, out Vector3D worldHitPos)
        {
            bool flag;
            try
            {
                MyPhysics.CastRay(worldRay.From, worldRay.To, m_tmpHitList, 0);
                using (List<MyPhysics.HitInfo>.Enumerator enumerator = m_tmpHitList.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyPhysics.HitInfo current = enumerator.Current;
                        MyCubeGrid hitEntity = current.HkHitInfo.GetHitEntity() as MyCubeGrid;
                        if (hitEntity != null)
                        {
                            worldHitPos = current.Position;
                            MyRenderProxy.DebugDrawAABB(new BoundingBoxD(worldHitPos - 0.01, worldHitPos + 0.01), Color.Wheat.ToVector3(), 1f, 1f, true, false, false);
                            hitGrid = hitEntity;
                            return true;
                        }
                    }
                }
                hitGrid = null;
                worldHitPos = new Vector3D();
                flag = false;
            }
            finally
            {
                m_tmpHitList.Clear();
            }
            return flag;
        }

        private void TryReduceGroupControl()
        {
            MyEntityController entityController = Sync.Players.GetEntityController(this);
            if ((entityController != null) && (entityController.ControlledEntity is MyCockpit))
            {
                MyCockpit controlledEntity = entityController.ControlledEntity as MyCockpit;
                if (ReferenceEquals(controlledEntity.CubeGrid, this))
                {
                    MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(this);
                    if (group != null)
                    {
                        foreach (MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node node in group.Nodes)
                        {
                            if (node.NodeData != this)
                            {
                                if (MySession.Static == null)
                                {
                                    MyLog.Default.WriteLine("MySession.Static was null");
                                }
                                else if (MySession.Static.SyncLayer == null)
                                {
                                    MyLog.Default.WriteLine("MySession.Static.SyncLayer was null");
                                }
                                else if (Sync.Clients == null)
                                {
                                    MyLog.Default.WriteLine("Sync.Clients was null");
                                }
                                Sync.Players.TryReduceControl(controlledEntity, node.NodeData);
                            }
                        }
                    }
                }
            }
        }

        private void UnregisterBlocks(List<MyCubeBlock> cubeBlocks)
        {
            foreach (MyCubeBlock block in cubeBlocks)
            {
                this.GridSystems.UnregisterFromSystems(block);
            }
        }

        private void UnregisterBlocksBeforeClose()
        {
            this.GridSystems.BeforeGridClose();
            this.UnregisterBlocks(this.m_fatBlocks.List);
            this.GridSystems.AfterGridClose();
        }

        public void UnregisterDecoy(MyDecoy block)
        {
            this.m_decoys.Remove(block);
        }

        public void UnregisterInventory(MyCubeBlock block)
        {
            this.m_inventories.Remove(block);
            this.m_inventoryMassDirty = true;
        }

        public void UnregisterOccupiedBlock(MyCockpit block)
        {
            this.m_occupiedBlocks.Remove(block);
        }

        public void UnregisterUnsafeBlock(MyCubeBlock block)
        {
            if (this.m_unsafeBlocks.Remove(block))
            {
                if (this.m_unsafeBlocks.Count == 0)
                {
                    MyUnsafeGridsSessionComponent.UnregisterGrid(this);
                }
                else
                {
                    MyUnsafeGridsSessionComponent.OnGridChanged(this);
                }
            }
        }

        public override void UpdateAfterSimulation()
        {
            MySimpleProfiler.Begin("Grid", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateAfterSimulation");
            base.UpdateAfterSimulation();
            this.ApplyDeformationPostponed();
            if (this.m_hasAdditionalModelGenerators)
            {
                using (List<IMyBlockAdditionalModelGenerator>.Enumerator enumerator = this.AdditionalModelGenerators.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.UpdateAfterSimulation();
                    }
                }
            }
            this.SendRemovedBlocks();
            this.SendRemovedBlocksWithIds();
            if (!this.HasStandAloneBlocks())
            {
                this.m_worldPositionChanged = false;
                if (Sync.IsServer)
                {
                    base.SetFadeOut(false);
                    base.Close();
                }
                MySimpleProfiler.End("UpdateAfterSimulation");
            }
            else
            {
                if (this.MarkedAsTrash)
                {
                    this.m_trashHighlightCounter--;
                    if ((this.TrashHighlightCounter <= 0) && Sync.IsServer)
                    {
                        MySessionComponentTrash.RemoveGrid(this);
                    }
                }
                if (Sync.IsServer)
                {
                    if (!MyFakes.ENABLE_FRACTURE_COMPONENT)
                    {
                        if ((this.Physics != null) && (this.Physics.GetFracturedBlocks().Count > 0))
                        {
                            bool enable = this.EnableGenerators(false, false);
                            foreach (MyFracturedBlock.Info info2 in this.Physics.GetFracturedBlocks())
                            {
                                this.CreateFracturedBlock(info2);
                            }
                            this.EnableGenerators(enable, false);
                        }
                    }
                    else if (this.Physics != null)
                    {
                        if (this.Physics.GetFractureBlockComponents().Count > 0)
                        {
                            try
                            {
                                foreach (MyFractureComponentBase.Info info in this.Physics.GetFractureBlockComponents())
                                {
                                    this.CreateFractureBlockComponent(info);
                                }
                            }
                            finally
                            {
                                this.Physics.ClearFractureBlockComponents();
                            }
                        }
                        this.Physics.CheckLastDestroyedBlockFracturePieces();
                    }
                }
                this.StepStructuralIntegrity();
                if (Sync.IsServer && (this.TestDynamic != MyTestDynamicReason.NoReason))
                {
                    EndpointId targetEndpoint = new EndpointId();
                    MyMultiplayer.RaiseEvent<MyCubeGrid, MyTestDynamicReason>(this, x => new Action<MyTestDynamicReason>(x.OnConvertedToShipRequest), this.TestDynamic, targetEndpoint);
                    this.TestDynamic = MyTestDynamicReason.NoReason;
                }
                this.DoLazyUpdates();
                if ((!Sync.IsServer && ((this.Physics != null) && !this.IsStatic)) && (this.IsClientPredicted == this.Physics.IsStatic))
                {
                    this.Physics.ConvertToDynamic(this.GridSizeEnum == MyCubeSize.Large, this.IsClientPredicted);
                    this.UpdateGravity();
                }
                if (((this.Physics != null) && this.Physics.Enabled) && this.m_inventoryMassDirty)
                {
                    this.m_inventoryMassDirty = false;
                    this.Physics.Shape.UpdateMassFromInventories(this.m_inventories, this.Physics);
                }
                if (this.m_worldPositionChanged)
                {
                    this.UpdateMergingGrids();
                    this.m_worldPositionChanged = false;
                }
                if (this.Physics != null)
                {
                    this.Physics.UpdateAfterSimulation();
                }
                this.GridSystems.UpdateAfterSimulation();
                if (!this.NeedsPerFrameUpdate)
                {
                    base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                }
                if (Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    this.ClearDirty();
                }
                else if (!this.m_updatingDirty && this.m_dirtyRegion.IsDirty)
                {
                    this.UpdateDirty(null, false);
                }
                MySimpleProfiler.End("UpdateAfterSimulation");
            }
        }

        public override void UpdateAfterSimulation100()
        {
            MySimpleProfiler.Begin("Grid", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateAfterSimulation100");
            base.UpdateAfterSimulation100();
            this.UpdateGravity();
            if ((MyFakes.ENABLE_BOUNDINGBOX_SHRINKING && this.m_boundsDirty) && ((MySandboxGame.TotalSimulationTimeInMilliseconds - this.m_lastUpdatedDirtyBounds) > 0x7530))
            {
                Vector3I min = this.m_min;
                Vector3I max = this.m_max;
                this.RecalcBounds();
                this.m_boundsDirty = false;
                this.m_lastUpdatedDirtyBounds = MySandboxGame.TotalSimulationTimeInMilliseconds;
                if ((this.GridSystems.GasSystem != null) && ((min != this.m_min) || (max != this.m_max)))
                {
                    this.GridSystems.GasSystem.OnCubeGridShrinked();
                }
            }
            if (MyFakes.ENABLE_GRID_SYSTEM_UPDATE)
            {
                this.GridSystems.UpdateAfterSimulation100();
            }
            MySimpleProfiler.End("UpdateAfterSimulation100");
        }

        public override void UpdateBeforeSimulation()
        {
            PerFrameData data;
            this.UpdatePredictionFlag();
            if (MyPhysicsConfig.EnableGridSpeedDebugDraw && (this.Physics != null))
            {
                Color color = (this.Physics.RigidBody.MaxLinearVelocity <= 190f) ? Color.Red : Color.Green;
                MyRenderProxy.DebugDrawText3D(base.PositionComp.GetPosition(), this.Physics.LinearVelocity.Length().ToString("F2"), color, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                MyRenderProxy.DebugDrawText3D(base.PositionComp.GetPosition() + (Vector3.One * 3f), this.Physics.AngularVelocity.Length().ToString("F2"), color, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            if (Sync.IsServer && (this.Physics != null))
            {
                bool flag = false;
                if (this.IsMarkedForEarlyDeactivation)
                {
                    if (!this.Physics.IsStatic)
                    {
                        flag = true;
                        this.Physics.ConvertToStatic();
                    }
                }
                else if (!this.IsStatic && this.Physics.IsStatic)
                {
                    flag = true;
                    this.Physics.ConvertToDynamic(this.GridSizeEnum == MyCubeSize.Large, false);
                }
                if (flag)
                {
                    base.RaisePhysicsChanged();
                }
            }
            MySimpleProfiler.Begin("Grid", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateBeforeSimulation");
            if (MyFakes.ENABLE_GRID_SYSTEM_UPDATE)
            {
                this.GridSystems.UpdateBeforeSimulation();
            }
            if (this.m_hasAdditionalModelGenerators)
            {
                using (List<IMyBlockAdditionalModelGenerator>.Enumerator enumerator = this.AdditionalModelGenerators.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.UpdateBeforeSimulation();
                    }
                }
            }
            this.DoLazyUpdates();
            base.UpdateBeforeSimulation();
            if (this.Physics != null)
            {
                this.Physics.UpdateBeforeSimulation();
            }
            if (MySessionComponentReplay.Static.IsEntityBeingReplayed(base.EntityId, out data))
            {
                if (((data.MovementData != null) && !this.IsStatic) && base.InScene)
                {
                    MyShipController shipController = this.GridSystems.ControlSystem.GetShipController();
                    if (shipController != null)
                    {
                        Vector2 rotationIndicator = new Vector2(data.MovementData.Value.RotateVector.X, data.MovementData.Value.RotateVector.Y);
                        shipController.MoveAndRotate((Vector3) data.MovementData.Value.MoveVector, rotationIndicator, data.MovementData.Value.RotateVector.Z);
                    }
                }
                if (data.SwitchWeaponData != null)
                {
                    MyShipController shipController = this.GridSystems.ControlSystem.GetShipController();
                    if (((shipController != null) && (data.SwitchWeaponData.Value.WeaponDefinition != null)) && !data.SwitchWeaponData.Value.WeaponDefinition.Value.TypeId.IsNull)
                    {
                        shipController.SwitchToWeapon(data.SwitchWeaponData.Value.WeaponDefinition.Value);
                    }
                }
                if (data.ShootData != null)
                {
                    MyShipController shipController = this.GridSystems.ControlSystem.GetShipController();
                    if (shipController != null)
                    {
                        if (data.ShootData.Value.Begin)
                        {
                            shipController.BeginShoot((MyShootActionEnum) data.ShootData.Value.ShootAction);
                        }
                        else
                        {
                            shipController.EndShoot((MyShootActionEnum) data.ShootData.Value.ShootAction);
                        }
                    }
                }
                if (data.ControlSwitchesData != null)
                {
                    MyShipController shipController = this.GridSystems.ControlSystem.GetShipController();
                    if (shipController != null)
                    {
                        if (data.ControlSwitchesData.Value.SwitchDamping)
                        {
                            shipController.SwitchDamping();
                        }
                        if (data.ControlSwitchesData.Value.SwitchLandingGears)
                        {
                            shipController.SwitchLandingGears();
                        }
                        if (data.ControlSwitchesData.Value.SwitchLights)
                        {
                            shipController.SwitchLights();
                        }
                        if (data.ControlSwitchesData.Value.SwitchReactors)
                        {
                            shipController.SwitchReactors();
                        }
                        if (data.ControlSwitchesData.Value.SwitchThrusts)
                        {
                            shipController.SwitchThrusts();
                        }
                    }
                }
                if (data.UseData != null)
                {
                    MyShipController shipController = this.GridSystems.ControlSystem.GetShipController();
                    if (shipController != null)
                    {
                        if (data.UseData.Value.Use)
                        {
                            shipController.Use();
                        }
                        else if (data.UseData.Value.UseContinues)
                        {
                            shipController.UseContinues();
                        }
                        else if (data.UseData.Value.UseFinished)
                        {
                            shipController.UseFinished();
                        }
                    }
                }
            }
            MySimpleProfiler.End("UpdateBeforeSimulation");
        }

        public override void UpdateBeforeSimulation10()
        {
            MySimpleProfiler.Begin("Grid", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateBeforeSimulation10");
            base.UpdateBeforeSimulation10();
            if (MyFakes.ENABLE_GRID_SYSTEM_UPDATE)
            {
                this.GridSystems.UpdateBeforeSimulation10();
            }
            MySimpleProfiler.End("UpdateBeforeSimulation10");
        }

        public override void UpdateBeforeSimulation100()
        {
            MySimpleProfiler.Begin("Grid", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateBeforeSimulation100");
            base.UpdateBeforeSimulation100();
            if (MyFakes.ENABLE_GRID_SYSTEM_UPDATE)
            {
                this.GridSystems.UpdateBeforeSimulation100();
            }
            MySimpleProfiler.End("UpdateBeforeSimulation100");
        }

        public void UpdateBlockNeighbours(MySlimBlock block)
        {
            if (this.m_cubeBlocks.Contains(block))
            {
                block.RemoveNeighbours();
                block.AddNeighbours();
                this.m_disconnectsDirty = MyTestDisconnectsReason.SplitBlock;
                this.MarkForUpdate();
            }
        }

        public void UpdateDirty(Action callback = null, bool immediate = false)
        {
            if (!this.m_updatingDirty && (this.m_resolvingSplits == 0))
            {
                this.m_updatingDirty = true;
                MyDirtyRegion dirtyRegion = this.m_dirtyRegion;
                this.m_dirtyRegion = this.m_dirtyRegionParallel;
                this.m_dirtyRegionParallel = dirtyRegion;
                if (immediate)
                {
                    this.UpdateDirtyInternal();
                    if (callback != null)
                    {
                        callback();
                    }
                    this.OnUpdateDirtyCompleted();
                }
                else
                {
                    Parallel.Start(this.m_UpdateDirtyInternal, callback = (Action) Delegate.Combine(callback, this.m_OnUpdateDirtyCompleted));
                }
            }
        }

        public void UpdateDirtyInternal()
        {
            // Invalid method body.
        }

        private void UpdateGravity()
        {
            if ((!this.IsStatic && ((this.Physics != null) && this.Physics.Enabled)) && !this.Physics.IsWelded)
            {
                if (this.Physics.DisableGravity <= 0)
                {
                    this.RecalculateGravity();
                }
                else
                {
                    MyGridPhysics physics = this.Physics;
                    physics.DisableGravity--;
                }
                if (!this.Physics.IsWelded && !this.Physics.RigidBody.Gravity.Equals(this.m_gravity, 0.01f))
                {
                    this.Physics.Gravity = this.m_gravity;
                    this.ActivatePhysics();
                }
            }
        }

        private void UpdateGridAABB()
        {
            base.PositionComp.LocalAABB = new BoundingBox(((Vector3) (this.m_min * this.GridSize)) - this.GridSizeHalfVector, (this.m_max * this.GridSize) + this.GridSizeHalfVector);
        }

        public void UpdateInstanceData()
        {
            this.Render.RebuildDirtyCells();
        }

        private void UpdateMergingGrids()
        {
            if (this.m_updateMergingGrids != null)
            {
                this.m_updateMergingGrids(base.WorldMatrix);
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            this.UpdateGravity();
            if (MyStructuralIntegrity.Enabled)
            {
                this.CreateStructuralIntegrity();
            }
            base.UpdateOnceBeforeFrame();
            if (MyFakes.ENABLE_GRID_SYSTEM_UPDATE || MyFakes.ENABLE_GRID_SYSTEM_ONCE_BEFORE_FRAME_UPDATE)
            {
                this.GridSystems.UpdateOnceBeforeFrame();
            }
            this.ActivatePhysics();
        }

        public void UpdateOwnership(long ownerId, bool isFunctional)
        {
            if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
            {
                this.m_ownershipManager.UpdateOnFunctionalChange(ownerId, isFunctional);
            }
        }

        public void UpdateParticleContactPoint(long entityId, ref Vector3 relativePosition, ref Vector3 normal, ref Vector3 separatingVelocity, float separatingSpeed, float impulse, VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes flags)
        {
            if (flags != VRage.Game.Entity.MyEntity.ContactPointData.ContactPointDataTypes.None)
            {
                VRage.Game.Entity.MyEntity.ContactPointData newValue = new VRage.Game.Entity.MyEntity.ContactPointData {
                    EntityId = entityId,
                    LocalPosition = relativePosition,
                    Normal = normal,
                    ContactPointType = flags,
                    SeparatingVelocity = separatingVelocity,
                    SeparatingSpeed = separatingSpeed,
                    Impulse = impulse
                };
                base.m_contactPoint.SetLocalValue(newValue);
            }
        }

        private void UpdatePartInstanceData(MyCubePart part, Vector3I cubePos)
        {
            MyCube cube;
            if (this.m_cubes.TryGetValue(cubePos, out cube))
            {
                MySlimBlock cubeBlock = cube.CubeBlock;
                if (cubeBlock != null)
                {
                    part.InstanceData.SetColorMaskHSV(new VRageMath.Vector4(cubeBlock.ColorMaskHSV, cubeBlock.Dithering));
                    part.SkinSubtypeId = cube.CubeBlock.SkinSubtypeId;
                }
                if (part.Model.BoneMapping != null)
                {
                    Matrix orientation = part.InstanceData.LocalMatrix.GetOrientation();
                    bool flag = false;
                    part.InstanceData.BoneRange = this.GridSize;
                    int index = 0;
                    while (true)
                    {
                        if (index >= Math.Min(part.Model.BoneMapping.Length, 9))
                        {
                            part.InstanceData.EnableSkinning = flag;
                            break;
                        }
                        Vector3I bonePos = Vector3I.Round(Vector3.Transform((Vector3) (((part.Model.BoneMapping[index] * 1f) - Vector3.One) * 1f), orientation) + Vector3.One);
                        Vector3UByte vec = Vector3UByte.Normalize(this.Skeleton.GetBone(cubePos, bonePos), this.GridSize);
                        if (!Vector3UByte.IsMiddle(vec))
                        {
                            flag = true;
                        }
                        part.InstanceData[index] = vec;
                        index++;
                    }
                }
            }
        }

        private void UpdateParts(Vector3I pos)
        {
            MyCube cube;
            bool flag = this.m_cubes.TryGetValue(pos, out cube);
            if (flag && !cube.CubeBlock.ShowParts)
            {
                this.RemoveBlockEdges(cube.CubeBlock);
            }
            if (!flag || !cube.CubeBlock.ShowParts)
            {
                if (flag)
                {
                    foreach (MyCubePart part in cube.Parts)
                    {
                        this.Render.RenderData.RemoveCubePart(part);
                    }
                }
                foreach (Vector3 vector5 in Base6Directions.Directions)
                {
                    MyCube cube3;
                    Vector3I key = (Vector3I) (pos + Vector3I.Round(vector5));
                    flag = this.m_cubes.TryGetValue(key, out cube3);
                    if (flag && cube3.CubeBlock.ShowParts)
                    {
                        Matrix matrix3;
                        cube3.CubeBlock.Orientation.GetMatrix(out matrix3);
                        MyTileDefinition[] cubeTiles = MyCubeGridDefinitions.GetCubeTiles(cube3.CubeBlock.BlockDefinition);
                        for (int i = 0; i < cube3.Parts.Length; i++)
                        {
                            Vector3 vector6 = Vector3.Normalize(Vector3.TransformNormal(cubeTiles[i].Normal, matrix3));
                            Vector3 vector4 = vector5 + vector6;
                            if (vector4.LengthSquared() < 0.001f)
                            {
                                this.Render.RenderData.AddCubePart(cube3.Parts[i]);
                            }
                        }
                    }
                }
            }
            else
            {
                Matrix matrix;
                MyTileDefinition[] cubeTiles = MyCubeGridDefinitions.GetCubeTiles(cube.CubeBlock.BlockDefinition);
                cube.CubeBlock.Orientation.GetMatrix(out matrix);
                if (this.Skeleton.IsDeformed(pos, 0.004f * this.GridSize, this, false))
                {
                    this.RemoveBlockEdges(cube.CubeBlock);
                }
                for (int i = 0; i < cube.Parts.Length; i++)
                {
                    this.UpdatePartInstanceData(cube.Parts[i], pos);
                    this.Render.RenderData.AddCubePart(cube.Parts[i]);
                    MyTileDefinition definition = cubeTiles[i];
                    if (!definition.IsEmpty)
                    {
                        Vector3 vec = Vector3.TransformNormal(definition.Normal, matrix);
                        Vector3 vector2 = Vector3.TransformNormal(definition.Up, matrix);
                        if (Base6Directions.IsBaseDirection(ref vec))
                        {
                            MyCube cube2;
                            Vector3I key = (Vector3I) (pos + Vector3I.Round(vec));
                            if (this.m_cubes.TryGetValue(key, out cube2) && cube2.CubeBlock.ShowParts)
                            {
                                Matrix matrix2;
                                cube2.CubeBlock.Orientation.GetMatrix(out matrix2);
                                MyTileDefinition[] definitionArray2 = MyCubeGridDefinitions.GetCubeTiles(cube2.CubeBlock.BlockDefinition);
                                for (int j = 0; j < cube2.Parts.Length; j++)
                                {
                                    MyTileDefinition definition2 = definitionArray2[j];
                                    if (!definition2.IsEmpty && ((vec + Vector3.TransformNormal(definition2.Normal, matrix2)).LengthSquared() < 0.001f))
                                    {
                                        if (cube2.CubeBlock.Dithering != cube.CubeBlock.Dithering)
                                        {
                                            this.Render.RenderData.AddCubePart(cube2.Parts[j]);
                                        }
                                        else
                                        {
                                            bool flag2 = false;
                                            if (definition2.FullQuad && !definition.IsRounded)
                                            {
                                                this.Render.RenderData.RemoveCubePart(cube.Parts[i]);
                                                flag2 = true;
                                            }
                                            if (definition.FullQuad && !definition2.IsRounded)
                                            {
                                                this.Render.RenderData.RemoveCubePart(cube2.Parts[j]);
                                                flag2 = true;
                                            }
                                            if ((!flag2 && ((definition2.Up * definition.Up).LengthSquared() > 0.001f)) && ((Vector3.TransformNormal(definition2.Up, matrix2) - vector2).LengthSquared() < 0.001f))
                                            {
                                                if (!definition.IsRounded || definition2.IsRounded)
                                                {
                                                    this.Render.RenderData.RemoveCubePart(cube.Parts[i]);
                                                }
                                                if (definition.IsRounded || !definition2.IsRounded)
                                                {
                                                    this.Render.RenderData.RemoveCubePart(cube2.Parts[j]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdatePhysicsShape()
        {
            this.Physics.UpdateShape();
        }

        public void UpdatePredictionFlag()
        {
            bool isClientPredicted = false;
            this.IsClientPredictedCar = false;
            if ((MyFakes.MULTIPLAYER_CLIENT_SIMULATE_CONTROLLED_GRID && !this.IsStatic) && !this.ForceDisablePrediction)
            {
                MyCubeGrid root = MyGridPhysicalHierarchy.Static.GetRoot(this);
                if (!ReferenceEquals(root, this))
                {
                    if (!ReferenceEquals(root, this))
                    {
                        isClientPredicted = root.IsClientPredicted;
                    }
                }
                else if ((!Sync.IsServer && ReferenceEquals(MySession.Static.TopMostControlledEntity, this)) || (Sync.IsServer && (Sync.Players.GetControllingPlayer(this) != null)))
                {
                    if (MyGridPhysicalHierarchy.Static.HasChildren(this) || MyFixedGrids.IsRooted(this))
                    {
                        if (MyFakes.MULTIPLAYER_CLIENT_SIMULATE_CONTROLLED_CAR)
                        {
                            bool car = true;
                            MyGridPhysicalHierarchy.Static.ApplyOnChildren(this, delegate (MyCubeGrid child) {
                                if (MyGridPhysicalHierarchy.Static.GetEntityConnectingToParent(child) is MyMotorSuspension)
                                {
                                    child.IsClientPredictedWheel = false;
                                    using (List<MyCubeBlock>.Enumerator enumerator = child.GetFatBlocks().GetEnumerator())
                                    {
                                        while (enumerator.MoveNext())
                                        {
                                            if (enumerator.Current is MyWheel)
                                            {
                                                child.IsClientPredictedWheel = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (!child.IsClientPredictedWheel)
                                    {
                                        car = false;
                                    }
                                }
                                else
                                {
                                    car = false;
                                }
                            });
                            isClientPredicted = car;
                            this.IsClientPredictedCar = car;
                        }
                    }
                    else
                    {
                        isClientPredicted = true;
                        if (this.Physics.PredictedContactsCounter > this.PREDICTION_SWITCH_MIN_COUNTER)
                        {
                            if (this.Physics.AnyPredictedContactEntities())
                            {
                                isClientPredicted = false;
                            }
                            else if ((this.Physics.PredictedContactLastTime + MyTimeSpan.FromSeconds((double) this.PREDICTION_SWITCH_TIME)) < MySandboxGame.Static.SimulationTime)
                            {
                                this.Physics.PredictedContactsCounter = 0;
                            }
                        }
                    }
                }
            }
            this.IsClientPredicted = isClientPredicted;
            if (this.IsClientPredicted != isClientPredicted)
            {
                this.Physics.UpdateConstraintsForceDisable();
            }
        }

        private MyObjectBuilder_CubeBlock UpgradeCubeBlock(MyObjectBuilder_CubeBlock block, out MyCubeBlockDefinition blockDefinition)
        {
            MyDefinitionId defId = block.GetId();
            if (MyFakes.ENABLE_COMPOUND_BLOCKS)
            {
                if (block is MyObjectBuilder_CompoundCubeBlock)
                {
                    MyObjectBuilder_CompoundCubeBlock block3 = block as MyObjectBuilder_CompoundCubeBlock;
                    blockDefinition = MyCompoundCubeBlock.GetCompoundCubeBlockDefinition();
                    if (blockDefinition == null)
                    {
                        return null;
                    }
                    if (block3.Blocks.Length == 1)
                    {
                        MyCubeBlockDefinition definition;
                        MyObjectBuilder_CubeBlock self = block3.Blocks[0];
                        if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(self.GetId(), out definition) && !MyCompoundCubeBlock.IsCompoundEnabled(definition))
                        {
                            blockDefinition = definition;
                            return self;
                        }
                    }
                    return block;
                }
                if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out blockDefinition) && MyCompoundCubeBlock.IsCompoundEnabled(blockDefinition))
                {
                    MyObjectBuilder_CompoundCubeBlock block5 = MyCompoundCubeBlock.CreateBuilder(block);
                    MyCubeBlockDefinition compoundCubeBlockDefinition = MyCompoundCubeBlock.GetCompoundCubeBlockDefinition();
                    if (compoundCubeBlockDefinition != null)
                    {
                        blockDefinition = compoundCubeBlockDefinition;
                        return block5;
                    }
                }
            }
            if (block is MyObjectBuilder_Ladder)
            {
                MyObjectBuilder_Passage local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Passage>(block.SubtypeName);
                local1.BlockOrientation = block.BlockOrientation;
                local1.BuildPercent = block.BuildPercent;
                local1.EntityId = block.EntityId;
                local1.IntegrityPercent = block.IntegrityPercent;
                local1.Min = block.Min;
                blockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Passage), block.SubtypeId));
                block = local1;
                return block;
            }
            MyObjectBuilder_CubeBlock block2 = block;
            string[] strArray = new string[] { "Red", "Yellow", "Blue", "Green", "Black", "White", "Gray" };
            Vector3[] vectorArray = new Vector3[] { MyRenderComponentBase.OldRedToHSV, MyRenderComponentBase.OldYellowToHSV, MyRenderComponentBase.OldBlueToHSV, MyRenderComponentBase.OldGreenToHSV, MyRenderComponentBase.OldBlackToHSV, MyRenderComponentBase.OldWhiteToHSV, MyRenderComponentBase.OldGrayToHSV };
            if (!MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out blockDefinition))
            {
                block2 = FindDefinitionUpgrade(block, out blockDefinition);
                if (block2 == null)
                {
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        if (defId.SubtypeName.EndsWith(strArray[i], StringComparison.InvariantCultureIgnoreCase))
                        {
                            string subtypeName = defId.SubtypeName.Substring(0, defId.SubtypeName.Length - strArray[i].Length);
                            MyDefinitionId id2 = new MyDefinitionId(defId.TypeId, subtypeName);
                            if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(id2, out blockDefinition))
                            {
                                block2 = block;
                                block2.ColorMaskHSV = vectorArray[i];
                                block2.SubtypeName = subtypeName;
                                return block2;
                            }
                        }
                    }
                }
                if (block2 == null)
                {
                    return null;
                }
            }
            return block2;
        }

        VRage.Game.ModAPI.IMySlimBlock VRage.Game.ModAPI.IMyCubeGrid.AddBlock(MyObjectBuilder_CubeBlock objectBuilder, bool testMerge) => 
            this.AddBlock(objectBuilder, testMerge);

        void VRage.Game.ModAPI.IMyCubeGrid.ApplyDestructionDeformation(VRage.Game.ModAPI.IMySlimBlock block)
        {
            if (block is MySlimBlock)
            {
                MyHitInfo? hitInfo = null;
                this.ApplyDestructionDeformation(block as MySlimBlock, 1f, hitInfo, 0L);
            }
        }

        MatrixI VRage.Game.ModAPI.IMyCubeGrid.CalculateMergeTransform(VRage.Game.ModAPI.IMyCubeGrid gridToMerge, Vector3I gridOffset)
        {
            if (gridToMerge is MyCubeGrid)
            {
                return this.CalculateMergeTransform(gridToMerge as MyCubeGrid, gridOffset);
            }
            return new MatrixI();
        }

        bool VRage.Game.ModAPI.IMyCubeGrid.CanAddCube(Vector3I pos)
        {
            MyBlockOrientation? orientation = null;
            return this.CanAddCube(pos, orientation, null, false);
        }

        bool VRage.Game.ModAPI.IMyCubeGrid.CanAddCubes(Vector3I min, Vector3I max) => 
            this.CanAddCubes(min, max);

        bool VRage.Game.ModAPI.IMyCubeGrid.CanMergeCubes(VRage.Game.ModAPI.IMyCubeGrid gridToMerge, Vector3I gridOffset) => 
            ((gridToMerge is MyCubeGrid) && this.CanMergeCubes(gridToMerge as MyCubeGrid, gridOffset));

        void VRage.Game.ModAPI.IMyCubeGrid.ChangeGridOwnership(long playerId, MyOwnershipShareModeEnum shareMode)
        {
            this.ChangeGridOwnership(playerId, shareMode);
        }

        void VRage.Game.ModAPI.IMyCubeGrid.ClearSymmetries()
        {
            this.ClearSymmetries();
        }

        void VRage.Game.ModAPI.IMyCubeGrid.ColorBlocks(Vector3I min, Vector3I max, Vector3 newHSV)
        {
            this.ColorBlocks(min, max, newHSV, false, false);
        }

        void VRage.Game.ModAPI.IMyCubeGrid.FixTargetCube(out Vector3I cube, Vector3 fractionalGridPosition)
        {
            this.FixTargetCube(out cube, fractionalGridPosition);
        }

        void VRage.Game.ModAPI.IMyCubeGrid.GetBlocks(List<VRage.Game.ModAPI.IMySlimBlock> blocks, Func<VRage.Game.ModAPI.IMySlimBlock, bool> collect)
        {
            foreach (MySlimBlock block in this.GetBlocks())
            {
                if ((collect == null) || collect(block))
                {
                    blocks.Add(block);
                }
            }
        }

        List<VRage.Game.ModAPI.IMySlimBlock> VRage.Game.ModAPI.IMyCubeGrid.GetBlocksInsideSphere(ref BoundingSphereD sphere)
        {
            HashSet<MySlimBlock> blocks = new HashSet<MySlimBlock>();
            this.GetBlocksInsideSphere(ref sphere, blocks, false);
            List<VRage.Game.ModAPI.IMySlimBlock> list = new List<VRage.Game.ModAPI.IMySlimBlock>(blocks.Count);
            foreach (MySlimBlock block in blocks)
            {
                list.Add(block);
            }
            return list;
        }

        Vector3 VRage.Game.ModAPI.IMyCubeGrid.GetClosestCorner(Vector3I gridPos, Vector3 position) => 
            this.GetClosestCorner(gridPos, position);

        VRage.Game.ModAPI.IMySlimBlock VRage.Game.ModAPI.IMyCubeGrid.GetCubeBlock(Vector3I pos) => 
            this.GetCubeBlock(pos);

        Vector3D? VRage.Game.ModAPI.IMyCubeGrid.GetLineIntersectionExactAll(ref LineD line, out double distance, out VRage.Game.ModAPI.IMySlimBlock intersectedBlock)
        {
            MySlimBlock block;
            Vector3D? nullable1 = this.GetLineIntersectionExactAll(ref line, out distance, out block);
            intersectedBlock = block;
            return nullable1;
        }

        bool VRage.Game.ModAPI.IMyCubeGrid.GetLineIntersectionExactGrid(ref LineD line, ref Vector3I position, ref double distanceSquared) => 
            this.GetLineIntersectionExactGrid(ref line, ref position, ref distanceSquared);

        bool VRage.Game.ModAPI.IMyCubeGrid.IsTouchingAnyNeighbor(Vector3I min, Vector3I max) => 
            this.IsTouchingAnyNeighbor(min, max);

        VRage.Game.ModAPI.IMyCubeGrid VRage.Game.ModAPI.IMyCubeGrid.MergeGrid_MergeBlock(VRage.Game.ModAPI.IMyCubeGrid gridToMerge, Vector3I gridOffset) => 
            (!(gridToMerge is MyCubeGrid) ? null : this.MergeGrid_MergeBlock(gridToMerge as MyCubeGrid, gridOffset, true));

        Vector3I? VRage.Game.ModAPI.IMyCubeGrid.RayCastBlocks(Vector3D worldStart, Vector3D worldEnd) => 
            this.RayCastBlocks(worldStart, worldEnd);

        void VRage.Game.ModAPI.IMyCubeGrid.RayCastCells(Vector3D worldStart, Vector3D worldEnd, List<Vector3I> outHitPositions, Vector3I? gridSizeInflate, bool havokWorld)
        {
            this.RayCastCells(worldStart, worldEnd, outHitPositions, gridSizeInflate, havokWorld, true);
        }

        void VRage.Game.ModAPI.IMyCubeGrid.RazeBlock(Vector3I position)
        {
            this.RazeBlock(position);
        }

        void VRage.Game.ModAPI.IMyCubeGrid.RazeBlocks(List<Vector3I> locations)
        {
            this.RazeBlocks(locations, 0L);
        }

        void VRage.Game.ModAPI.IMyCubeGrid.RazeBlocks(ref Vector3I pos, ref Vector3UByte size)
        {
            this.RazeBlocks(ref pos, ref size, 0L);
        }

        void VRage.Game.ModAPI.IMyCubeGrid.RemoveBlock(VRage.Game.ModAPI.IMySlimBlock block, bool updatePhysics)
        {
            if (block is MySlimBlock)
            {
                this.RemoveBlock(block as MySlimBlock, updatePhysics);
            }
        }

        void VRage.Game.ModAPI.IMyCubeGrid.RemoveDestroyedBlock(VRage.Game.ModAPI.IMySlimBlock block)
        {
            if (block is MySlimBlock)
            {
                this.RemoveDestroyedBlock(block as MySlimBlock, 0L);
            }
        }

        VRage.Game.ModAPI.IMyCubeGrid VRage.Game.ModAPI.IMyCubeGrid.Split(List<VRage.Game.ModAPI.IMySlimBlock> blocks, bool sync) => 
            CreateSplit(this, blocks.ConvertAll<MySlimBlock>(x => (MySlimBlock) x), sync, 0L);

        VRage.Game.ModAPI.IMyCubeGrid VRage.Game.ModAPI.IMyCubeGrid.SplitByPlane(PlaneD plane) => 
            this.SplitByPlane(plane);

        void VRage.Game.ModAPI.IMyCubeGrid.UpdateBlockNeighbours(VRage.Game.ModAPI.IMySlimBlock block)
        {
            if (block is MySlimBlock)
            {
                this.UpdateBlockNeighbours(block as MySlimBlock);
            }
        }

        bool VRage.Game.ModAPI.IMyCubeGrid.WillRemoveBlockSplitGrid(VRage.Game.ModAPI.IMySlimBlock testBlock) => 
            this.WillRemoveBlockSplitGrid((MySlimBlock) testBlock);

        Vector3I VRage.Game.ModAPI.IMyCubeGrid.WorldToGridInteger(Vector3D coords) => 
            this.WorldToGridInteger(coords);

        VRage.Game.ModAPI.Ingame.IMySlimBlock VRage.Game.ModAPI.Ingame.IMyCubeGrid.GetCubeBlock(Vector3I position)
        {
            VRage.Game.ModAPI.Ingame.IMySlimBlock cubeBlock = this.GetCubeBlock(position);
            if (((cubeBlock == null) || ((cubeBlock.FatBlock == null) || !(cubeBlock.FatBlock is MyTerminalBlock))) || !(cubeBlock.FatBlock as MyTerminalBlock).IsAccessibleForProgrammableBlock)
            {
                return null;
            }
            return cubeBlock;
        }

        bool VRage.Game.ModAPI.Ingame.IMyCubeGrid.IsSameConstructAs(VRage.Game.ModAPI.Ingame.IMyCubeGrid other) => 
            this.IsSameConstructAs((MyCubeGrid) other);

        public bool WillRemoveBlockSplitGrid(MySlimBlock testBlock) => 
            m_disconnectHelper.TryDisconnect(testBlock);

        public Vector3I WorldToGridInteger(Vector3D coords) => 
            Vector3I.Round(Vector3D.Transform(coords, base.PositionComp.WorldMatrixNormalizedInv) * this.GridSizeR);

        public Vector3D WorldToGridScaledLocal(Vector3D coords) => 
            (Vector3D.Transform(coords, base.PositionComp.WorldMatrixNormalizedInv) * this.GridSizeR);

        private static List<VRage.Game.Entity.MyEntity> m_tmpResultList =>
            MyUtils.Init<List<VRage.Game.Entity.MyEntity>>(ref m_tmpResultListPerThread);

        public static bool ShowSenzorGizmos
        {
            [CompilerGenerated]
            get => 
                <ShowSenzorGizmos>k__BackingField;
            [CompilerGenerated]
            set => 
                (<ShowSenzorGizmos>k__BackingField = value);
        }

        public static bool ShowGravityGizmos
        {
            [CompilerGenerated]
            get => 
                <ShowGravityGizmos>k__BackingField;
            [CompilerGenerated]
            set => 
                (<ShowGravityGizmos>k__BackingField = value);
        }

        public static bool ShowAntennaGizmos
        {
            [CompilerGenerated]
            get => 
                <ShowAntennaGizmos>k__BackingField;
            [CompilerGenerated]
            set => 
                (<ShowAntennaGizmos>k__BackingField = value);
        }

        public static bool ShowCenterOfMass
        {
            [CompilerGenerated]
            get => 
                <ShowCenterOfMass>k__BackingField;
            [CompilerGenerated]
            set => 
                (<ShowCenterOfMass>k__BackingField = value);
        }

        public static bool ShowGridPivot
        {
            [CompilerGenerated]
            get => 
                <ShowGridPivot>k__BackingField;
            [CompilerGenerated]
            set => 
                (<ShowGridPivot>k__BackingField = value);
        }

        public static bool ShowStructuralIntegrity
        {
            [CompilerGenerated]
            get => 
                <ShowStructuralIntegrity>k__BackingField;
            [CompilerGenerated]
            set => 
                (<ShowStructuralIntegrity>k__BackingField = value);
        }

        private static List<HkBodyCollision> m_physicsBoxQueryList =>
            MyUtils.Init<List<HkBodyCollision>>(ref m_physicsBoxQueryListPerThread);

        private static List<Vector3I> m_cacheRayCastCells =>
            MyUtils.Init<List<Vector3I>>(ref m_cacheRayCastCellsPerThread);

        private static Dictionary<Vector3I, ConnectivityResult> m_cacheNeighborBlocks =>
            MyUtils.Init<Dictionary<Vector3I, ConnectivityResult>>(ref m_cacheNeighborBlocksPerThread);

        private static List<MyCubeBlockDefinition.MountPoint> m_cacheMountPointsA =>
            MyUtils.Init<List<MyCubeBlockDefinition.MountPoint>>(ref m_cacheMountPointsAPerThread);

        private static List<MyCubeBlockDefinition.MountPoint> m_cacheMountPointsB =>
            MyUtils.Init<List<MyCubeBlockDefinition.MountPoint>>(ref m_cacheMountPointsBPerThread);

        private static List<MyPhysics.HitInfo> m_tmpHitList =>
            MyUtils.Init<List<MyPhysics.HitInfo>>(ref m_tmpHitListPerThread);

        private static List<Vector3I> m_tmpCubeNeighbours =>
            MyUtils.Init<List<Vector3I>>(ref m_tmpCubeNeighboursPerThread);

        private static List<int> m_tmpMultiBlockIndices =>
            MyUtils.Init<List<int>>(ref m_tmpMultiBlockIndicesPerThread);

        private static Ref<HkBoxShape> m_lastQueryBox
        {
            get
            {
                if (m_lastQueryBoxPerThread == null)
                {
                    m_lastQueryBoxPerThread = new Ref<HkBoxShape>();
                    m_lastQueryBoxPerThread.Value = new HkBoxShape(Vector3.One);
                }
                return m_lastQueryBoxPerThread;
            }
        }

        string VRage.Game.ModAPI.Ingame.IMyCubeGrid.CustomName
        {
            get => 
                base.DisplayName;
            set
            {
                if (this.IsAccessibleForProgrammableBlock)
                {
                    this.ChangeDisplayNameRequest(value);
                }
            }
        }

        string VRage.Game.ModAPI.IMyCubeGrid.CustomName
        {
            get => 
                base.DisplayName;
            set => 
                this.ChangeDisplayNameRequest(value);
        }

        List<long> VRage.Game.ModAPI.IMyCubeGrid.BigOwners =>
            this.BigOwners;

        List<long> VRage.Game.ModAPI.IMyCubeGrid.SmallOwners =>
            this.SmallOwners;

        bool VRage.Game.ModAPI.IMyCubeGrid.IsRespawnGrid
        {
            get => 
                this.IsRespawnGrid;
            set => 
                (this.IsRespawnGrid = value);
        }

        bool VRage.Game.ModAPI.IMyCubeGrid.IsStatic
        {
            get => 
                this.IsStatic;
            set
            {
                if (value)
                {
                    this.RequestConversionToStation();
                }
                else
                {
                    this.RequestConversionToShip(null);
                }
            }
        }

        Vector3I? VRage.Game.ModAPI.IMyCubeGrid.XSymmetryPlane
        {
            get => 
                this.XSymmetryPlane;
            set => 
                (this.XSymmetryPlane = value);
        }

        Vector3I? VRage.Game.ModAPI.IMyCubeGrid.YSymmetryPlane
        {
            get => 
                this.YSymmetryPlane;
            set => 
                (this.YSymmetryPlane = value);
        }

        Vector3I? VRage.Game.ModAPI.IMyCubeGrid.ZSymmetryPlane
        {
            get => 
                this.ZSymmetryPlane;
            set => 
                (this.ZSymmetryPlane = value);
        }

        bool VRage.Game.ModAPI.IMyCubeGrid.XSymmetryOdd
        {
            get => 
                this.XSymmetryOdd;
            set => 
                (this.XSymmetryOdd = value);
        }

        bool VRage.Game.ModAPI.IMyCubeGrid.YSymmetryOdd
        {
            get => 
                this.YSymmetryOdd;
            set => 
                (this.YSymmetryOdd = value);
        }

        bool VRage.Game.ModAPI.IMyCubeGrid.ZSymmetryOdd
        {
            get => 
                this.ZSymmetryOdd;
            set => 
                (this.ZSymmetryOdd = value);
        }

        public List<MyCubeBlock> Inventories =>
            this.m_inventories;

        public HashSetReader<MyCubeBlock> UnsafeBlocks =>
            this.m_unsafeBlocks;

        public HashSetReader<MyDecoy> Decoys =>
            this.m_decoys;

        public HashSetReader<MyCockpit> OccupiedBlocks =>
            this.m_occupiedBlocks;

        public VRage.Sync.SyncType SyncType { get; set; }

        public bool IsPowered =>
            this.m_IsPowered;

        public int NumberOfGridColors =>
            this.m_colorStatistics.Count;

        public ConcurrentCachingHashSet<Vector3I> DirtyBlocks
        {
            get
            {
                this.m_dirtyRegion.Cubes.ApplyChanges();
                return this.m_dirtyRegion.Cubes;
            }
        }

        public MyCubeGridRenderData RenderData =>
            this.Render.RenderData;

        public HashSet<MyCubeBlock> BlocksForDraw =>
            this.m_blocksForDraw;

        internal MyStructuralIntegrity StructuralIntegrity { get; private set; }

        public bool IsSplit { get; set; }

        private static List<MyCockpit> m_tmpOccupiedCockpits =>
            MyUtils.Init<List<MyCockpit>>(ref m_tmpOccupiedCockpitsPerThread);

        private static List<MyObjectBuilder_BlockGroup> m_tmpBlockGroups =>
            MyUtils.Init<List<MyObjectBuilder_BlockGroup>>(ref m_tmpBlockGroupsPerThread);

        public List<IMyBlockAdditionalModelGenerator> AdditionalModelGenerators =>
            this.Render.AdditionalModelGenerators;

        public MyCubeGridSystems GridSystems { get; private set; }

        public bool IsStatic
        {
            get => 
                this.m_isStatic;
            private set
            {
                if (this.m_isStatic != value)
                {
                    this.m_isStatic = value;
                    this.NotifyIsStaticChanged(this.m_isStatic);
                }
            }
        }

        public bool DampenersEnabled =>
            ((bool) this.m_dampenersEnabled);

        public bool MarkedAsTrash =>
            ((bool) this.m_markedAsTrash);

        public bool IsUnsupportedStation { get; private set; }

        public float GridSize { get; private set; }

        public float GridScale { get; private set; }

        public float GridSizeHalf { get; private set; }

        public Vector3 GridSizeHalfVector { get; private set; }

        public float GridSizeQuarter { get; private set; }

        public Vector3 GridSizeQuarterVector { get; private set; }

        public float GridSizeR { get; private set; }

        public Vector3I Min =>
            this.m_min;

        public Vector3I Max =>
            this.m_max;

        public bool IsRespawnGrid
        {
            get => 
                ((bool) this.m_isRespawnGrid);
            set => 
                (this.m_isRespawnGrid.Value = value);
        }

        public bool DestructibleBlocks
        {
            get => 
                ((bool) this.m_destructibleBlocks);
            set => 
                (this.m_destructibleBlocks.Value = value);
        }

        public bool Editable
        {
            get => 
                ((bool) this.m_editable);
            set => 
                this.m_editable.ValidateAndSet(value);
        }

        public bool BlocksDestructionEnabled =>
            (MySession.Static.Settings.DestructibleBlocks && ((bool) this.m_destructibleBlocks));

        public List<long> SmallOwners =>
            this.m_ownershipManager.SmallOwners;

        public List<long> BigOwners
        {
            get
            {
                List<long> bigOwners = this.m_ownershipManager.BigOwners;
                if (bigOwners.Count == 0)
                {
                    MyCubeGrid parent = MyGridPhysicalHierarchy.Static.GetParent(this);
                    if (parent != null)
                    {
                        bigOwners = parent.BigOwners;
                    }
                }
                return bigOwners;
            }
        }

        public MyCubeSize GridSizeEnum
        {
            get => 
                this.m_gridSizeEnum;
            set
            {
                this.m_gridSizeEnum = value;
                this.GridSize = MyDefinitionManager.Static.GetCubeSize(value);
                this.GridSizeHalf = this.GridSize / 2f;
                this.GridSizeHalfVector = new Vector3(this.GridSizeHalf);
                this.GridSizeQuarter = this.GridSize / 4f;
                this.GridSizeQuarterVector = new Vector3(this.GridSizeQuarter);
                this.GridSizeR = 1f / this.GridSize;
            }
        }

        public MyGridPhysics Physics
        {
            get => 
                ((MyGridPhysics) base.Physics);
            set => 
                (base.Physics = value);
        }

        public int ShapeCount =>
            ((this.Physics != null) ? this.Physics.Shape.ShapeCount : 0);

        public MyEntityThrustComponent EntityThrustComponent =>
            base.Components.Get<MyEntityThrustComponent>();

        public bool IsMarkedForEarlyDeactivation
        {
            get => 
                this.m_isMarkedForEarlyDeactivation;
            set
            {
                if (this.m_isMarkedForEarlyDeactivation != value)
                {
                    this.m_isMarkedForEarlyDeactivation = value;
                    this.MarkForUpdate();
                }
            }
        }

        public bool IsBlockTrasferInProgress { get; private set; }

        public float Mass
        {
            get
            {
                if (this.Physics == null)
                {
                    return 0f;
                }
                if ((Sync.IsServer || (!this.IsStatic || (this.Physics == null))) || (this.Physics.Shape == null))
                {
                    return this.Physics.Mass;
                }
                if (this.Physics.Shape.MassProperties == null)
                {
                    return 0f;
                }
                return this.Physics.Shape.MassProperties.Value.Mass;
            }
        }

        public static int GridCounter
        {
            [CompilerGenerated]
            get => 
                <GridCounter>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<GridCounter>k__BackingField = value);
        }

        public int BlocksCount =>
            this.m_cubeBlocks.Count;

        public int BlocksPCU =>
            this.m_PCU;

        public HashSet<MySlimBlock> CubeBlocks =>
            this.m_cubeBlocks;

        internal bool SmallToLargeConnectionsInitialized =>
            this.m_smallToLargeConnectionsInitialized;

        internal bool EnableSmallToLargeConnections =>
            this.m_enableSmallToLargeConnections;

        internal MyTestDynamicReason TestDynamic
        {
            get => 
                this.m_testDynamic;
            set
            {
                if (this.m_testDynamic != value)
                {
                    this.m_testDynamic = value;
                    this.MarkForUpdate();
                }
            }
        }

        internal MyRenderComponentCubeGrid Render
        {
            get => 
                ((MyRenderComponentCubeGrid) base.Render);
            set => 
                (base.Render = value);
        }

        public long LocalCoordSystem { get; set; }

        internal bool NeedsPerFrameUpdate
        {
            get
            {
                bool needsPerFrameUpdate;
                int num1;
                if (!MyFakes.OPTIMIZE_GRID_UPDATES)
                {
                    return true;
                }
                if ((MyPhysicsConfig.EnableGridSpeedDebugDraw || (this.m_inventoryMassDirty || (this.m_hasAdditionalModelGenerators || (this.m_blocksForDamageApplicationDirty || ((this.m_disconnectsDirty != MyTestDisconnectsReason.NoReason) || ((this.m_resolvingSplits > 0) || (this.Skeleton.NeedsPerFrameUpdate || (this.m_ownershipManager.NeedRecalculateOwners || (((this.Physics != null) ? this.Physics.NeedsPerFrameUpdate : false) || (MySessionComponentReplay.Static.IsEntityBeingReplayed(base.EntityId) || (((this.Physics != null) ? this.Physics.IsDirty() : false) || ((this.m_deformationPostponed.Count > 0) || ((this.m_removeBlockQueueWithGenerators.Count > 0) || ((this.m_destroyBlockQueue.Count > 0) || ((this.m_destructionDeformationQueue.Count > 0) || ((this.m_removeBlockQueueWithoutGenerators.Count > 0) || ((this.m_removeBlockWithIdQueueWithGenerators.Count > 0) || ((this.m_removeBlockWithIdQueueWithoutGenerators.Count > 0) || ((this.m_destroyBlockWithIdQueueWithGenerators.Count > 0) || ((this.m_destroyBlockWithIdQueueWithoutGenerators.Count > 0) || ((this.TestDynamic != MyTestDynamicReason.NoReason) || ((this.BonesToSend.InputCount > 0) || ((this.m_updateMergingGrids != null) || MySessionComponentReplay.Static.HasEntityReplayData(base.EntityId)))))))))))))))))))))))) || this.MarkedAsTrash)
                {
                    needsPerFrameUpdate = true;
                }
                else if (!MyFakes.ENABLE_GRID_SYSTEM_UPDATE || (this.GridSystems == null))
                {
                    needsPerFrameUpdate = false;
                }
                else
                {
                    needsPerFrameUpdate = this.GridSystems.NeedsPerFrameUpdate;
                }
                bool flag = ((bool) num1) | this.IsDirty();
                if (!flag && (Sync.IsServer || this.IsClientPredicted))
                {
                    int isStatic;
                    int num3;
                    if (this.Physics == null)
                    {
                        isStatic = 0;
                    }
                    else if (this.IsMarkedForEarlyDeactivation && !this.Physics.IsStatic)
                    {
                        isStatic = 1;
                    }
                    else if (this.IsMarkedForEarlyDeactivation || this.IsStatic)
                    {
                        isStatic = 0;
                    }
                    else
                    {
                        isStatic = (int) this.Physics.IsStatic;
                    }
                    flag |= num3;
                }
                return flag;
            }
        }

        internal bool NeedsPerFrameDraw
        {
            get
            {
                int showAntennaGizmos;
                if (!MyFakes.OPTIMIZE_GRID_UPDATES)
                {
                    return true;
                }
                if ((ShowCenterOfMass || (ShowGridPivot || ShowSenzorGizmos)) || ShowGravityGizmos)
                {
                    showAntennaGizmos = 1;
                }
                else
                {
                    showAntennaGizmos = (int) ShowAntennaGizmos;
                }
                return ((((this.IsDirty() | showAntennaGizmos) | (MyFakes.ENABLE_GRID_SYSTEM_UPDATE ? this.GridSystems.NeedsPerFrameDraw : false)) | (this.BlocksForDraw.Count > 0)) | this.MarkedAsTrash);
            }
        }

        public bool IsLargeDestroyInProgress =>
            ((this.m_destroyBlockQueue.Count > BLOCK_LIMIT_FOR_LARGE_DESTRUCTION) || this.m_largeDestroyInProgress);

        public bool UsesTargetingList =>
            this.m_usesTargetingList;

        public long ClosestParentId
        {
            get => 
                this.m_closestParentId;
            set
            {
                if (this.m_closestParentId != value)
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

        public bool IsClientPredicted { get; private set; }

        public bool IsClientPredictedWheel { get; private set; }

        public bool IsClientPredictedCar { get; private set; }

        public int TrashHighlightCounter =>
            this.m_trashHighlightCounter;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCubeGrid.<>c <>9 = new MyCubeGrid.<>c();
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__99_0;
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__104_1;
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__110_0;
            public static Predicate<MyPhysics.HitInfo> <>9__113_0;
            public static Converter<VRage.Game.ModAPI.IMySlimBlock, MySlimBlock> <>9__223_0;
            public static Func<MyCubeGrid, Action> <>9__539_0;
            public static Action<VRage.Game.Entity.MyEntity> <>9__575_0;
            public static Action<VRage.Game.Entity.MyEntity> <>9__575_1;
            public static Action<MyCubeGrid, MyCubeGrid> <>9__575_2;
            public static Func<MyCubeGrid, Action<List<Vector3I>, long>> <>9__587_0;
            public static Func<MyCubeGrid, Action<List<Vector3I>, List<MyDisconnectHelper.Group>>> <>9__589_0;
            public static Func<MyCubeGrid, Action<MyCubeGrid.MyTestDynamicReason>> <>9__617_0;
            public static Func<MyCubeGrid, Action<List<Vector3I>, List<Vector3I>, List<Vector3I>, List<Vector3I>>> <>9__671_0;
            public static Func<MyCubeGrid, Action<List<MyCubeGrid.BlockPositionId>, List<MyCubeGrid.BlockPositionId>, List<MyCubeGrid.BlockPositionId>, List<MyCubeGrid.BlockPositionId>>> <>9__677_0;
            public static Func<MyCubeGrid, Action<uint, MyCubeGrid.MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>> <>9__686_0;
            public static Func<MyCubeGrid, Action<MyCubeGrid.MyBlockBuildArea, long, bool, long>> <>9__688_0;
            public static Func<MyCubeGrid, Action<uint, HashSet<MyCubeGrid.MyBlockLocation>, long, bool, long>> <>9__689_0;
            public static Func<MyCubeGrid, Action<uint, HashSet<MyCubeGrid.MyBlockLocation>, long, bool, long>> <>9__690_0;
            public static Func<MyCubeGrid, Action> <>9__690_1;
            public static Func<MyCubeGrid, Action<MyCubeGrid.MyBlockBuildArea, int, HashSet<Vector3UByte>, long, bool, long>> <>9__693_0;
            public static Func<MyCubeGrid, Action<Vector3I, Vector3UByte, long>> <>9__704_0;
            public static Func<MyCubeGrid, Action<Vector3I, Vector3UByte, long>> <>9__705_0;
            public static Func<MyCubeGrid, Action<Vector3I, Vector3UByte, long>> <>9__706_0;
            public static Func<MyCubeGrid, Action<Vector3I, Vector3UByte, HashSet<Vector3UByte>>> <>9__707_0;
            public static Func<MyCubeGrid, Action<List<Vector3I>, long>> <>9__711_0;
            public static Func<MyCubeGrid, Action<List<Vector3I>>> <>9__712_0;
            public static Func<MyCubeGrid, Action<Vector3I, Vector3I, Vector3, bool, long>> <>9__721_0;
            public static Func<MyCubeGrid, Action<Vector3, bool, long>> <>9__722_0;
            public static Func<MyCubeGrid, Action<Vector3, bool, long>> <>9__723_0;
            public static Func<MyCubeGrid, Action<Vector3I, Vector3I, Vector3, bool, long>> <>9__725_0;
            public static Func<MyVoxelBase, Action<Vector3D, float, bool>> <>9__732_1;
            public static Func<MyCubeGrid, Action<Vector3I, float>> <>9__734_0;
            public static Func<MyCubeGrid, Action<int, List<byte>>> <>9__827_0;
            public static Func<MyCubeGrid, Action<List<MyObjectBuilder_CubeGrid>, long, bool, bool>> <>9__876_0;
            public static Func<MyCubeGrid, Action<MyObjectBuilder_CubeGrid, MatrixI>> <>9__877_0;
            public static Func<MyCubeGrid, Action<uint, MyCubeGrid.MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>> <>9__890_0;
            public static Func<IMyEventOwner, Action<long>> <>9__900_0;
            public static Func<MyCubeGrid, Action<MyMultipleEnabledEnum>> <>9__915_0;
            public static Func<MyCubeGrid, Action<Vector3I, ushort, float, float, MyIntegrityChangeEnum, long>> <>9__917_0;
            public static Func<MyCubeGrid, Action<Vector3I, ushort, List<MyStockpileItem>>> <>9__918_0;
            public static Func<MyCubeGrid, Action<Vector3I, ushort, long>> <>9__919_0;
            public static Func<MyCubeGrid, Action<Vector3I, long, byte>> <>9__921_0;
            public static Func<MyCubeGrid, Action<Vector3I, long, byte, long>> <>9__923_0;
            public static Func<MyCubeGrid, Action<MyMultipleEnabledEnum, long>> <>9__925_0;
            public static Func<MyCubeGrid, Action<MyCubeGrid.MyTestDynamicReason>> <>9__927_0;
            public static Func<MyCubeGrid, Action> <>9__928_0;
            public static Func<MyCubeGrid, Action> <>9__929_0;
            public static Func<MyCubeGrid, Action> <>9__929_1;
            public static Func<MyCubeGrid, Action> <>9__931_0;
            public static Func<MyCubeGrid, Action<long, long, MyOwnershipShareModeEnum>> <>9__932_0;
            public static Func<MyCubeGrid, Action<long, long, MyOwnershipShareModeEnum>> <>9__933_0;
            public static Func<MyCubeGrid, Action<long, long, MyOwnershipShareModeEnum>> <>9__933_1;
            public static Func<MyCubeGrid, Action<long, MyOwnershipShareModeEnum>> <>9__940_0;
            public static Func<MyCubeGrid, Action<List<Vector3I>>> <>9__942_0;
            public static Func<MyCubeGrid, Action<string>> <>9__944_0;
            public static Func<MyCubeGrid, Action<string, List<long>>> <>9__946_0;
            public static Func<MyCubeGrid, Action<List<MyCubeGrid.LocationIdentity>>> <>9__948_0;
            public static Func<MyCubeGrid, Action<List<MyCubeGrid.LocationIdentity>>> <>9__949_0;
            public static Func<IMyEventOwner, Action<MyOwnershipShareModeEnum, List<MyCubeGrid.MySingleOwnershipRequest>, long>> <>9__953_0;
            public static Func<IMyEventOwner, Action<MyOwnershipShareModeEnum, List<MyCubeGrid.MySingleOwnershipRequest>>> <>9__954_0;
            public static Func<MyCubeGrid, Action> <>9__1005_0;
            public static Func<MyCubeGrid, Action<long, SerializableVector3I, Base6Directions.Direction, Base6Directions.Direction>> <>9__1011_0;
            public static Func<MyCubeGrid, Action<long, SerializableVector3I, Base6Directions.Direction, Base6Directions.Direction, Vector3I>> <>9__1016_0;

            internal void <.ctor>b__575_0(VRage.Game.Entity.MyEntity entity)
            {
                MyPhysics.RemoveDestructions(entity);
            }

            internal void <.ctor>b__575_1(VRage.Game.Entity.MyEntity e)
            {
            }

            internal void <.ctor>b__575_2(MyCubeGrid g1, MyCubeGrid g2)
            {
            }

            internal Action<List<Vector3I>> <AnnounceRemoveSplit>b__942_0(MyCubeGrid x) => 
                new Action<List<Vector3I>>(x.OnRemoveSplit);

            internal Action<uint, MyCubeGrid.MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long> <BuildBlockRequest>b__686_0(MyCubeGrid x) => 
                new Action<uint, MyCubeGrid.MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>(x.BuildBlockSucess);

            internal Action<MyCubeGrid.MyBlockBuildArea, long, bool, long> <BuildBlocks>b__688_0(MyCubeGrid x) => 
                new Action<MyCubeGrid.MyBlockBuildArea, long, bool, long>(x.BuildBlocksAreaRequest);

            internal Action<uint, HashSet<MyCubeGrid.MyBlockLocation>, long, bool, long> <BuildBlocks>b__689_0(MyCubeGrid x) => 
                new Action<uint, HashSet<MyCubeGrid.MyBlockLocation>, long, bool, long>(x.BuildBlocksRequest);

            internal Action<MyCubeGrid.MyBlockBuildArea, int, HashSet<Vector3UByte>, long, bool, long> <BuildBlocksAreaRequest>b__693_0(MyCubeGrid x) => 
                new Action<MyCubeGrid.MyBlockBuildArea, int, HashSet<Vector3UByte>, long, bool, long>(x.BuildBlocksAreaClient);

            internal Action<uint, HashSet<MyCubeGrid.MyBlockLocation>, long, bool, long> <BuildBlocksRequest>b__690_0(MyCubeGrid x) => 
                new Action<uint, HashSet<MyCubeGrid.MyBlockLocation>, long, bool, long>(x.BuildBlocksClient);

            internal Action <BuildBlocksRequest>b__690_1(MyCubeGrid x) => 
                new Action(x.BuildBlocksFailedNotify);

            internal Action<uint, MyCubeGrid.MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long> <BuildMultiBlocks>b__890_0(MyCubeGrid x) => 
                new Action<uint, MyCubeGrid.MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>(x.BuildBlockRequest);

            internal Action<string> <ChangeDisplayNameRequest>b__944_0(MyCubeGrid x) => 
                new Action<string>(x.OnChangeDisplayNameRequest);

            internal Action<long, MyOwnershipShareModeEnum> <ChangeGridOwner>b__940_0(MyCubeGrid x) => 
                new Action<long, MyOwnershipShareModeEnum>(x.OnChangeGridOwner);

            internal Action<long, long, MyOwnershipShareModeEnum> <ChangeOwnerRequest>b__932_0(MyCubeGrid x) => 
                new Action<long, long, MyOwnershipShareModeEnum>(x.OnChangeOwnerRequest);

            internal Action<MyOwnershipShareModeEnum, List<MyCubeGrid.MySingleOwnershipRequest>, long> <ChangeOwnersRequest>b__953_0(IMyEventOwner s) => 
                new Action<MyOwnershipShareModeEnum, List<MyCubeGrid.MySingleOwnershipRequest>, long>(MyCubeGrid.OnChangeOwnersRequest);

            internal Action<MyMultipleEnabledEnum, long> <ChangePowerProducerState>b__925_0(MyCubeGrid x) => 
                new Action<MyMultipleEnabledEnum, long>(x.OnPowerProducerStateRequest);

            internal Action<Vector3I, Vector3I, Vector3, bool, long> <ColorBlockRequest>b__725_0(MyCubeGrid x) => 
                new Action<Vector3I, Vector3I, Vector3, bool, long>(x.OnColorBlock);

            internal Action<Vector3I, Vector3I, Vector3, bool, long> <ColorBlocks>b__721_0(MyCubeGrid x) => 
                new Action<Vector3I, Vector3I, Vector3, bool, long>(x.ColorBlockRequest);

            internal Action<Vector3, bool, long> <ColorGrid>b__722_0(MyCubeGrid x) => 
                new Action<Vector3, bool, long>(x.ColorGridFriendlyRequest);

            internal Action<Vector3, bool, long> <ColorGridFriendlyRequest>b__723_0(MyCubeGrid x) => 
                new Action<Vector3, bool, long>(x.OnColorGridFriendly);

            internal void <ConvertPrefabsToObjs>b__99_0(MyGuiScreenMessageBox.ResultEnum result)
            {
                MyCubeGrid.StartConverting(false);
            }

            internal Action<List<Vector3I>, long> <CreateSplit>b__587_0(MyCubeGrid x) => 
                new Action<List<Vector3I>, long>(x.CreateSplit_Implementation);

            internal Action<List<Vector3I>, List<MyDisconnectHelper.Group>> <CreateSplits>b__589_0(MyCubeGrid x) => 
                new Action<List<Vector3I>, List<MyDisconnectHelper.Group>>(x.CreateSplits_Implementation);

            internal void <ExportToObjFile>b__104_1(MyGuiScreenMessageBox.ResultEnum result)
            {
                MyCubeGrid.ConvertNextGrid(false);
            }

            internal bool <GetTargetEntity>b__113_0(MyPhysics.HitInfo hit) => 
                ((MySession.Static.ControlledEntity != null) && ReferenceEquals(hit.HkHitInfo.GetHitEntity(), MySession.Static.ControlledEntity.Entity));

            internal Action <LogHierarchy>b__1005_0(MyCubeGrid x) => 
                new Action(x.OnLogHierarchy);

            internal Action<long, SerializableVector3I, Base6Directions.Direction, Base6Directions.Direction> <MergeGrid_MergeBlock>b__1011_0(MyCubeGrid x) => 
                new Action<long, SerializableVector3I, Base6Directions.Direction, Base6Directions.Direction>(x.MergeGrid_MergeBlockClient);

            internal Action<long, SerializableVector3I, Base6Directions.Direction, Base6Directions.Direction, Vector3I> <MergeGrid_Static>b__1016_0(MyCubeGrid x) => 
                new Action<long, SerializableVector3I, Base6Directions.Direction, Base6Directions.Direction, Vector3I>(x.MergeGrid_MergeClient);

            internal Action<string, List<long>> <ModifyGroup>b__946_0(MyCubeGrid x) => 
                new Action<string, List<long>>(x.OnModifyGroupSuccess);

            internal Action<Vector3I, float> <MultiplyBlockSkeleton>b__734_0(MyCubeGrid x) => 
                new Action<Vector3I, float>(x.OnBonesMultiplied);

            internal Action<long, long, MyOwnershipShareModeEnum> <OnChangeOwnerRequest>b__933_0(MyCubeGrid x) => 
                new Action<long, long, MyOwnershipShareModeEnum>(x.OnChangeOwner);

            internal Action<long, long, MyOwnershipShareModeEnum> <OnChangeOwnerRequest>b__933_1(MyCubeGrid x) => 
                new Action<long, long, MyOwnershipShareModeEnum>(x.OnChangeOwner);

            internal Action<MyOwnershipShareModeEnum, List<MyCubeGrid.MySingleOwnershipRequest>> <OnChangeOwnersRequest>b__954_0(IMyEventOwner s) => 
                new Action<MyOwnershipShareModeEnum, List<MyCubeGrid.MySingleOwnershipRequest>>(MyCubeGrid.OnChangeOwnersSuccess);

            internal Action<Vector3I, Vector3UByte, long> <OnClosedMessageBox>b__705_0(MyCubeGrid x) => 
                new Action<Vector3I, Vector3UByte, long>(x.RazeBlocksAreaRequest);

            internal Action <OnConvertedToShipRequest>b__929_0(MyCubeGrid x) => 
                new Action(x.OnConvertToShipFailed);

            internal Action <OnConvertedToShipRequest>b__929_1(MyCubeGrid x) => 
                new Action(x.OnConvertToDynamic);

            internal Action <OnConvertedToStationRequest>b__931_0(MyCubeGrid x) => 
                new Action(x.ConvertToStatic);

            internal Action<List<MyCubeGrid.LocationIdentity>> <OnRazeBlockInCompoundBlockRequest>b__949_0(MyCubeGrid x) => 
                new Action<List<MyCubeGrid.LocationIdentity>>(x.OnRazeBlockInCompoundBlockSuccess);

            internal Action <OnTerinalOpened>b__539_0(MyCubeGrid x) => 
                new Action(x.OnGridChangedRPC);

            internal Action<List<MyObjectBuilder_CubeGrid>, long, bool, bool> <PasteBlocksToGrid>b__876_0(MyCubeGrid x) => 
                new Action<List<MyObjectBuilder_CubeGrid>, long, bool, bool>(x.PasteBlocksToGridServer_Implementation);

            internal Action<MyObjectBuilder_CubeGrid, MatrixI> <PasteBlocksToGridServer_Implementation>b__877_0(MyCubeGrid x) => 
                new Action<MyObjectBuilder_CubeGrid, MatrixI>(x.PasteBlocksToGridClient_Implementation);

            internal Action<Vector3D, float, bool> <PerformCutouts>b__732_1(MyVoxelBase x) => 
                new Action<Vector3D, float, bool>(x.PerformCutOutSphereFast);

            internal void <PlacePrefabsToWorld>b__110_0(MyGuiScreenMessageBox.ResultEnum result)
            {
                MyCubeGrid.StartConverting(true);
            }

            internal Action<List<MyCubeGrid.LocationIdentity>> <RazeBlockInCompoundBlock>b__948_0(MyCubeGrid x) => 
                new Action<List<MyCubeGrid.LocationIdentity>>(x.OnRazeBlockInCompoundBlockRequest);

            internal Action<Vector3I, Vector3UByte, long> <RazeBlocks>b__706_0(MyCubeGrid x) => 
                new Action<Vector3I, Vector3UByte, long>(x.RazeBlocksAreaRequest);

            internal Action<List<Vector3I>, long> <RazeBlocks>b__711_0(MyCubeGrid x) => 
                new Action<List<Vector3I>, long>(x.RazeBlocksRequest);

            internal Action<Vector3I, Vector3UByte, HashSet<Vector3UByte>> <RazeBlocksAreaRequest>b__707_0(MyCubeGrid x) => 
                new Action<Vector3I, Vector3UByte, HashSet<Vector3UByte>>(x.RazeBlocksAreaSuccess);

            internal Action<Vector3I, Vector3UByte, long> <RazeBlocksDelayed>b__704_0(MyCubeGrid x) => 
                new Action<Vector3I, Vector3UByte, long>(x.RazeBlocksAreaRequest);

            internal Action<List<Vector3I>> <RazeBlocksRequest>b__712_0(MyCubeGrid x) => 
                new Action<List<Vector3I>>(x.RazeBlocksClient);

            internal Action<MyCubeGrid.MyTestDynamicReason> <RequestConversionToShip>b__927_0(MyCubeGrid x) => 
                new Action<MyCubeGrid.MyTestDynamicReason>(x.OnConvertedToShipRequest);

            internal Action <RequestConversionToStation>b__928_0(MyCubeGrid x) => 
                new Action(x.OnConvertedToStationRequest);

            internal Action<Vector3I, long, byte> <RequestFillStockpile>b__921_0(MyCubeGrid x) => 
                new Action<Vector3I, long, byte>(x.OnStockpileFillRequest);

            internal Action<Vector3I, long, byte, long> <RequestSetToConstruction>b__923_0(MyCubeGrid x) => 
                new Action<Vector3I, long, byte, long>(x.OnSetToConstructionRequest);

            internal Action<int, List<byte>> <SendBones>b__827_0(MyCubeGrid x) => 
                new Action<int, List<byte>>(x.OnBonesReceived);

            internal Action<Vector3I, ushort, long> <SendFractureComponentRepaired>b__919_0(MyCubeGrid x) => 
                new Action<Vector3I, ushort, long>(x.FractureComponentRepaired);

            internal Action<long> <SendGridCloseRequest>b__900_0(IMyEventOwner s) => 
                new Action<long>(MyCubeGrid.OnGridClosedRequest);

            internal Action<Vector3I, ushort, float, float, MyIntegrityChangeEnum, long> <SendIntegrityChanged>b__917_0(MyCubeGrid x) => 
                new Action<Vector3I, ushort, float, float, MyIntegrityChangeEnum, long>(x.BlockIntegrityChanged);

            internal Action<MyMultipleEnabledEnum> <SendReflectorState>b__915_0(MyCubeGrid x) => 
                new Action<MyMultipleEnabledEnum>(x.RelfectorStateRecived);

            internal Action<List<Vector3I>, List<Vector3I>, List<Vector3I>, List<Vector3I>> <SendRemovedBlocks>b__671_0(MyCubeGrid x) => 
                new Action<List<Vector3I>, List<Vector3I>, List<Vector3I>, List<Vector3I>>(x.RemovedBlocks);

            internal Action<List<MyCubeGrid.BlockPositionId>, List<MyCubeGrid.BlockPositionId>, List<MyCubeGrid.BlockPositionId>, List<MyCubeGrid.BlockPositionId>> <SendRemovedBlocksWithIds>b__677_0(MyCubeGrid x) => 
                new Action<List<MyCubeGrid.BlockPositionId>, List<MyCubeGrid.BlockPositionId>, List<MyCubeGrid.BlockPositionId>, List<MyCubeGrid.BlockPositionId>>(x.RemovedBlocksWithIds);

            internal Action<Vector3I, ushort, List<MyStockpileItem>> <SendStockpileChanged>b__918_0(MyCubeGrid x) => 
                new Action<Vector3I, ushort, List<MyStockpileItem>>(x.BlockStockpileChanged);

            internal Action<MyCubeGrid.MyTestDynamicReason> <UpdateAfterSimulation>b__617_0(MyCubeGrid x) => 
                new Action<MyCubeGrid.MyTestDynamicReason>(x.OnConvertedToShipRequest);

            internal MySlimBlock <VRage.Game.ModAPI.IMyCubeGrid.Split>b__223_0(VRage.Game.ModAPI.IMySlimBlock x) => 
                ((MySlimBlock) x);
        }

        private class AreaConnectivityTest : IMyGridConnectivityTest
        {
            private readonly Dictionary<Vector3I, Vector3I> m_lookup = new Dictionary<Vector3I, Vector3I>();
            private MyBlockOrientation m_orientation;
            private MyCubeBlockDefinition m_definition;
            private Vector3I m_posInGrid;
            private Vector3I m_blockMin;
            private Vector3I m_blockMax;
            private Vector3I m_stepDelta;

            public unsafe void AddBlock(Vector3UByte offset)
            {
                Vector3I vectori2;
                Vector3I vectori = (Vector3I) (this.m_posInGrid + (offset * this.m_stepDelta));
                vectori2.X = this.m_blockMin.X;
                while (vectori2.X <= this.m_blockMax.X)
                {
                    vectori2.Y = this.m_blockMin.Y;
                    while (true)
                    {
                        if (vectori2.Y > this.m_blockMax.Y)
                        {
                            int* numPtr3 = (int*) ref vectori2.X;
                            numPtr3[0]++;
                            break;
                        }
                        vectori2.Z = this.m_blockMin.Z;
                        while (true)
                        {
                            if (vectori2.Z > this.m_blockMax.Z)
                            {
                                int* numPtr2 = (int*) ref vectori2.Y;
                                numPtr2[0]++;
                                break;
                            }
                            this.m_lookup.Add(vectori + vectori2, vectori);
                            int* numPtr1 = (int*) ref vectori2.Z;
                            numPtr1[0]++;
                        }
                    }
                }
            }

            public unsafe void GetConnectedBlocks(Vector3I minI, Vector3I maxI, Dictionary<Vector3I, ConnectivityResult> outOverlappedCubeBlocks)
            {
                Vector3I vectori;
                vectori.X = minI.X;
                while (vectori.X <= maxI.X)
                {
                    vectori.Y = minI.Y;
                    while (true)
                    {
                        if (vectori.Y > maxI.Y)
                        {
                            int* numPtr3 = (int*) ref vectori.X;
                            numPtr3[0]++;
                            break;
                        }
                        vectori.Z = minI.Z;
                        while (true)
                        {
                            Vector3I vectori2;
                            if (vectori.Z > maxI.Z)
                            {
                                int* numPtr2 = (int*) ref vectori.Y;
                                numPtr2[0]++;
                                break;
                            }
                            if (this.m_lookup.TryGetValue(vectori, out vectori2) && !outOverlappedCubeBlocks.ContainsKey(vectori2))
                            {
                                ConnectivityResult result = new ConnectivityResult {
                                    Definition = this.m_definition,
                                    FatBlock = null,
                                    Position = vectori2,
                                    Orientation = this.m_orientation
                                };
                                outOverlappedCubeBlocks.Add(vectori2, result);
                            }
                            int* numPtr1 = (int*) ref vectori.Z;
                            numPtr1[0]++;
                        }
                    }
                }
            }

            public void Initialize(ref MyCubeGrid.MyBlockBuildArea area, MyCubeBlockDefinition definition)
            {
                this.m_definition = definition;
                this.m_orientation = new MyBlockOrientation(area.OrientationForward, area.OrientationUp);
                this.m_posInGrid = area.PosInGrid;
                this.m_blockMin = (Vector3I) area.BlockMin;
                this.m_blockMax = (Vector3I) area.BlockMax;
                this.m_stepDelta = (Vector3I) area.StepDelta;
                this.m_lookup.Clear();
            }
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct BlockPositionId
        {
            [ProtoMember(0x222e)]
            public Vector3I Position;
            [ProtoMember(0x2231)]
            public uint CompoundId;
        }

        public class BlockTypeCounter
        {
            private Dictionary<MyDefinitionId, int> m_countById = new Dictionary<MyDefinitionId, int>(MyDefinitionId.Comparer);

            internal int GetNextNumber(MyDefinitionId blockType)
            {
                int num = 0;
                this.m_countById.TryGetValue(blockType, out num);
                num++;
                this.m_countById[blockType] = num;
                return num;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DeformationPostponedItem
        {
            public Vector3I Position;
            public Vector3I Min;
            public Vector3I Max;
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct LocationIdentity
        {
            [ProtoMember(0x2a59)]
            public Vector3I Location;
            [ProtoMember(0x2a5c)]
            public ushort Id;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public struct MyBlockBuildArea
        {
            public DefinitionIdBlit DefinitionId;
            public uint ColorMaskHSV;
            public Vector3I PosInGrid;
            public Vector3B BlockMin;
            public Vector3B BlockMax;
            public Vector3UByte BuildAreaSize;
            public Vector3B StepDelta;
            public Base6Directions.Direction OrientationForward;
            public Base6Directions.Direction OrientationUp;
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct MyBlockLocation
        {
            [ProtoMember(0x220a)]
            public Vector3I Min;
            [ProtoMember(0x220d)]
            public Vector3I Max;
            [ProtoMember(0x2210)]
            public Vector3I CenterPos;
            [ProtoMember(0x2213)]
            public MyBlockOrientation Orientation;
            [ProtoMember(0x2216)]
            public long EntityId;
            [ProtoMember(0x2219)]
            public DefinitionIdBlit BlockDefinition;
            [ProtoMember(0x221c)]
            public long Owner;
            public MyBlockLocation(MyDefinitionId blockDefinition, Vector3I min, Vector3I max, Vector3I center, Quaternion orientation, long entityId, long owner)
            {
                this.BlockDefinition = blockDefinition;
                this.Min = min;
                this.Max = max;
                this.CenterPos = center;
                this.Orientation = new MyBlockOrientation(ref orientation);
                this.EntityId = entityId;
                this.Owner = owner;
            }
        }

        public class MyCubeGridHitInfo
        {
            public MyIntersectionResultLineTriangleEx Triangle;
            public Vector3I Position;
            public int CubePartIndex = -1;

            public void Reset()
            {
                this.Triangle = new MyIntersectionResultLineTriangleEx();
                this.Position = new Vector3I();
                this.CubePartIndex = -1;
            }
        }

        private class MyCubeGridPosition : MyPositionComponent
        {
            private MyCubeGrid m_grid;

            public override void OnAddedToContainer()
            {
                base.OnAddedToContainer();
                this.m_grid = base.Container.Entity as MyCubeGrid;
            }

            protected override void OnWorldPositionChanged(object source, bool updateChildren, bool forceUpdateAllChildren)
            {
                this.m_grid.m_worldPositionChanged = true;
                base.OnWorldPositionChanged(source, updateChildren, forceUpdateAllChildren);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyNeighbourCachedBlock
        {
            public Vector3I Position;
            public MyCubeBlockDefinition BlockDefinition;
            public MyBlockOrientation Orientation;
            public override int GetHashCode() => 
                this.Position.GetHashCode();
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct MySingleOwnershipRequest
        {
            [ProtoMember(0x2a4f)]
            public long BlockId;
            [ProtoMember(0x2a52)]
            public long Owner;
        }

        private class MySyncGridThrustState
        {
            public Vector3B LastSendState;
            public int SleepFrames;

            public bool ShouldSend(Vector3B newThrust)
            {
                if ((this.SleepFrames <= 4) || (this.LastSendState == newThrust))
                {
                    this.SleepFrames++;
                    return false;
                }
                this.SleepFrames = 0;
                this.LastSendState = newThrust;
                return true;
            }
        }

        public enum MyTestDisconnectsReason
        {
            NoReason,
            BlockRemoved,
            SplitBlock
        }

        internal enum MyTestDynamicReason
        {
            NoReason,
            GridCopied,
            GridSplit,
            GridSplitByBlock,
            ConvertToShip
        }

        private enum NeighborOffsetIndex
        {
            XUP,
            XDOWN,
            YUP,
            YDOWN,
            ZUP,
            ZDOWN,
            XUP_YUP,
            XUP_YDOWN,
            XDOWN_YUP,
            XDOWN_YDOWN,
            YUP_ZUP,
            YUP_ZDOWN,
            YDOWN_ZUP,
            YDOWN_ZDOWN,
            XUP_ZUP,
            XUP_ZDOWN,
            XDOWN_ZUP,
            XDOWN_ZDOWN,
            XUP_YUP_ZUP,
            XUP_YUP_ZDOWN,
            XUP_YDOWN_ZUP,
            XUP_YDOWN_ZDOWN,
            XDOWN_YUP_ZUP,
            XDOWN_YUP_ZDOWN,
            XDOWN_YDOWN_ZUP,
            XDOWN_YDOWN_ZDOWN
        }

        private class PasteGridData : WorkData
        {
            private List<MyObjectBuilder_CubeGrid> m_entities;
            private bool m_detectDisconnects;
            private Vector3 m_objectVelocity;
            private bool m_multiBlock;
            private bool m_instantBuild;
            private List<MyCubeGrid> m_results;
            private bool m_canPlaceGrid;
            private List<VRage.ModAPI.IMyEntity> m_resultIDs;
            private bool m_removeScripts;
            public readonly EndpointId SenderEndpointId;
            public readonly bool IsLocallyInvoked;
            public Vector3D? m_offset;

            public PasteGridData(List<MyObjectBuilder_CubeGrid> entities, bool detectDisconnects, Vector3 objectVelocity, bool multiBlock, bool instantBuild, bool shouldRemoveScripts, EndpointId senderEndpointId, bool isLocallyInvoked, Vector3D? offset)
            {
                this.m_entities = new List<MyObjectBuilder_CubeGrid>(entities);
                this.m_detectDisconnects = detectDisconnects;
                this.m_objectVelocity = objectVelocity;
                this.m_multiBlock = multiBlock;
                this.m_instantBuild = instantBuild;
                this.SenderEndpointId = senderEndpointId;
                this.IsLocallyInvoked = isLocallyInvoked;
                this.m_removeScripts = shouldRemoveScripts;
                this.m_offset = offset;
            }

            public unsafe void Callback()
            {
                Vector3D? nullable;
                if (!this.IsLocallyInvoked)
                {
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent(s => new Action(MyCubeGrid.SendHudNotificationAfterPaste), this.SenderEndpointId, nullable);
                }
                else if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    MyHud.PopRotatingWheelVisible();
                }
                if (this.m_canPlaceGrid)
                {
                    foreach (MyCubeGrid grid in this.m_results)
                    {
                        this.m_canPlaceGrid &= this.TestPastedGridPlacement(grid, true);
                        if (!this.m_canPlaceGrid)
                        {
                            break;
                        }
                    }
                }
                if (this.m_canPlaceGrid && (this.m_results.Count > 0))
                {
                    foreach (VRage.ModAPI.IMyEntity entity in this.m_resultIDs)
                    {
                        VRage.ModAPI.IMyEntity entity2;
                        MyEntityIdentifier.TryGetEntity(entity.EntityId, out entity2, false);
                        if (entity2 == null)
                        {
                            MyEntityIdentifier.AddEntityWithId(entity);
                        }
                    }
                    MyCubeGrid.AfterPaste(this.m_results, this.m_objectVelocity, this.m_detectDisconnects);
                }
                else
                {
                    if (this.m_results != null)
                    {
                        foreach (MyCubeGrid grid2 in this.m_results)
                        {
                            using (HashSet<MySlimBlock>.Enumerator enumerator3 = grid2.GetBlocks().GetEnumerator())
                            {
                                while (enumerator3.MoveNext())
                                {
                                    enumerator3.Current.RemoveAuthorship();
                                }
                            }
                            grid2.Close();
                        }
                    }
                    if (!this.IsLocallyInvoked)
                    {
                        nullable = null;
                        MyMultiplayer.RaiseStaticEvent(s => new Action(MyCubeGrid.ShowPasteFailedOperation), this.SenderEndpointId, nullable);
                    }
                }
                if (this.m_offset != null)
                {
                    foreach (MyCubeGrid local3 in this.m_results)
                    {
                        MatrixD worldMatrix = local3.WorldMatrix;
                        MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                        xdPtr1.Translation = worldMatrix.Translation + this.m_offset.Value;
                        local3.WorldMatrix = worldMatrix;
                    }
                }
            }

            private bool TestPastedGridPlacement(MyCubeGrid grid, bool testPhysics)
            {
                MyGridPlacementSettings gridPlacementSettings = MyClipboardComponent.ClipboardDefinition.PastingSettings.GetGridPlacementSettings(grid.GridSizeEnum, grid.IsStatic);
                return MyCubeGrid.TestPlacementArea(grid, grid.IsStatic, ref gridPlacementSettings, grid.PositionComp.LocalAABB, !grid.IsStatic, null, true, testPhysics);
            }

            public void TryPasteGrid()
            {
                bool flag = MyEventContext.Current.IsLocallyInvoked || MySession.Static.HasPlayerCreativeRights(this.SenderEndpointId.Value);
                bool flag2 = false;
                if ((!MySession.Static.SurvivalMode || flag2) || flag)
                {
                    for (int i = 0; i < this.m_entities.Count; i++)
                    {
                        this.m_entities[i] = (MyObjectBuilder_CubeGrid) this.m_entities[i].Clone();
                    }
                    Sandbox.Game.Entities.MyEntities.RemapObjectBuilderCollection((IEnumerable<MyObjectBuilder_EntityBase>) this.m_entities);
                    MySessionComponentDLC component = MySession.Static.GetComponent<MySessionComponentDLC>();
                    foreach (MyObjectBuilder_CubeGrid grid in this.m_entities)
                    {
                        int index = 0;
                        while (index < grid.CubeBlocks.Count)
                        {
                            MyObjectBuilder_CubeBlock block = grid.CubeBlocks[index];
                            if (this.m_removeScripts)
                            {
                                MyObjectBuilder_MyProgrammableBlock block2 = block as MyObjectBuilder_MyProgrammableBlock;
                                if (block2 != null)
                                {
                                    block2.Program = null;
                                }
                            }
                            if (!component.HasDefinitionDLC(new MyDefinitionId(block.TypeId, block.SubtypeId), this.SenderEndpointId.Value))
                            {
                                grid.CubeBlocks.RemoveAt(index);
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                    bool flag1 = this.m_instantBuild & flag;
                    this.m_results = new List<MyCubeGrid>();
                    MyEntityIdentifier.InEntityCreationBlock = true;
                    MyEntityIdentifier.LazyInitPerThreadStorage(0x800);
                    this.m_canPlaceGrid = true;
                    foreach (MyObjectBuilder_CubeGrid grid2 in this.m_entities)
                    {
                        string[] textArray1 = new string[] { "CreateCompressedMsg: Type: ", grid2.GetType().Name.ToString(), "  Name: ", grid2.Name, "  EntityID: ", grid2.EntityId.ToString("X8") };
                        MySandboxGame.Log.WriteLine(string.Concat(textArray1));
                        MyCubeGrid item = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilder(grid2, false) as MyCubeGrid;
                        if (item != null)
                        {
                            this.m_results.Add(item);
                            this.m_canPlaceGrid &= this.TestPastedGridPlacement(item, false);
                            if (!this.m_canPlaceGrid)
                            {
                                break;
                            }
                            long inventoryEntityId = 0L;
                            if (this.m_instantBuild & flag)
                            {
                                MyCubeGrid.ChangeOwnership(inventoryEntityId, item);
                            }
                            string[] textArray2 = new string[] { "Status: Exists(", Sandbox.Game.Entities.MyEntities.EntityExists(grid2.EntityId).ToString(), ") InScene(", ((grid2.PersistentFlags & MyPersistentEntityFlags2.InScene) == MyPersistentEntityFlags2.InScene).ToString(), ")" };
                            MySandboxGame.Log.WriteLine(string.Concat(textArray2));
                        }
                    }
                    this.m_resultIDs = new List<VRage.ModAPI.IMyEntity>();
                    MyEntityIdentifier.GetPerThreadEntities(this.m_resultIDs);
                    MyEntityIdentifier.ClearPerThreadEntities();
                    MyEntityIdentifier.InEntityCreationBlock = false;
                }
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyCubeGrid.PasteGridData.<>c <>9 = new MyCubeGrid.PasteGridData.<>c();
                public static Func<IMyEventOwner, Action> <>9__15_0;
                public static Func<IMyEventOwner, Action> <>9__15_1;

                internal Action <Callback>b__15_0(IMyEventOwner s) => 
                    new Action(MyCubeGrid.SendHudNotificationAfterPaste);

                internal Action <Callback>b__15_1(IMyEventOwner s) => 
                    new Action(MyCubeGrid.ShowPasteFailedOperation);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RelativeOffset
        {
            public bool Use;
            public bool RelativeToEntity;
            public long SpawnerId;
            public Vector3D OriginalSpawnPoint;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TriangleWithMaterial
        {
            public MyTriangleVertexIndices triangle;
            public MyTriangleVertexIndices uvIndices;
            public string material;
        }
    }
}

