namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.AI.Pathfinding;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Cube.CubeBuilder;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.ContextHandling;
    using Sandbox.Game.GameSystems.CoordinateSystem;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.Graphics;
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
    using VRage.Game.Components.Session;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
    using VRage.Input;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Models;
    using VRageRender.Utils;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation | MyUpdateOrder.BeforeSimulation), StaticEventOwner]
    public class MyCubeBuilder : MyBlockBuilderBase, IMyCubeBuilder, IMyFocusHolder
    {
        private static float SEMI_TRANSPARENT_BOX_MODIFIER = 1.04f;
        private static readonly MyStringId ID_SQUARE = MyStringId.GetOrCompute("Square");
        private static readonly MyStringId ID_GIZMO_DRAW_LINE_RED = MyStringId.GetOrCompute("GizmoDrawLineRed");
        private static readonly MyStringId ID_GIZMO_DRAW_LINE = MyStringId.GetOrCompute("GizmoDrawLine");
        private const float DEBUG_SCALE = 0.5f;
        private static string[] m_mountPointSideNames = new string[] { "Front", "Back", "Left", "Right", "Top", "Bottom" };
        private MyBlockRemovalData m_removalTemporalData;
        [CompilerGenerated]
        private Action OnBlockSizeChanged;
        [CompilerGenerated]
        private Action<MyCubeBlockDefinition> OnBlockAdded;
        public static MyCubeBuilder Static;
        protected static double BLOCK_ROTATION_SPEED = 0.002;
        private static MyColoringArea[] m_currColoringArea = new MyColoringArea[8];
        private static List<Vector3I> m_cacheGridIntersections = new List<Vector3I>();
        private static int m_cycle = 0;
        public static Dictionary<MyPlayer.PlayerId, List<Vector3>> AllPlayersColors = null;
        protected bool canBuild = true;
        private List<Vector3D> m_collisionTestPoints = new List<Vector3D>(12);
        private int m_lastInputHandleTime;
        private bool m_customRotation;
        private float m_animationSpeed = 0.1f;
        private bool m_animationLock;
        private bool m_stationPlacement;
        protected MyBlockBuilderRotationHints m_rotationHints = new MyBlockBuilderRotationHints();
        protected MyBlockBuilderRenderData m_renderData = new MyBlockBuilderRenderData();
        private bool m_blockCreationActivated;
        private bool m_useSymmetry;
        private bool m_useTransparency = true;
        private bool m_alignToDefault = true;
        public Vector3D? MaxGridDistanceFrom;
        private bool AllowFreeSpacePlacement = true;
        private float FreeSpacePlacementDistance = 20f;
        private StringBuilder m_cubeCountStringBuilder = new StringBuilder(10);
        private const int MAX_CUBES_BUILT_AT_ONCE = 0x800;
        private const int MAX_CUBES_BUILT_IN_ONE_AXIS = 0xff;
        private const float CONTINUE_BUILDING_VIEW_ANGLE_CHANGE_THRESHOLD = 0.998f;
        private const float CONTINUE_BUILDING_VIEW_POINT_CHANGE_THRESHOLD = 0.25f;
        protected MyCubeBuilderGizmo m_gizmo = new MyCubeBuilderGizmo();
        private MySymmetrySettingModeEnum m_symmetrySettingMode = MySymmetrySettingModeEnum.NoPlane;
        private Vector3D m_initialIntersectionStart;
        private Vector3D m_initialIntersectionDirection;
        protected MyCubeBuilderState m_cubeBuilderState;
        protected MyCoordinateSystem.CoordSystemData m_lastLocalCoordSysData;
        private MyHudNotification m_blockNotAvailableNotification;
        private MyHudNotification m_symmetryNotification;
        private CubePlacementModeEnum m_cubePlacementMode;
        private bool m_isBuildMode;
        private MyHudNotification m_buildModeHint;
        private MyHudNotification m_cubePlacementModeNotification;
        private MyHudNotification m_cubePlacementModeHint;
        private MyHudNotification m_cubePlacementUnable;
        protected HashSet<MyCubeGrid.MyBlockLocation> m_blocksBuildQueue = new HashSet<MyCubeGrid.MyBlockLocation>();
        protected List<Vector3I> m_tmpBlockPositionList = new List<Vector3I>();
        protected List<Tuple<Vector3I, ushort>> m_tmpCompoundBlockPositionIdList = new List<Tuple<Vector3I, ushort>>();
        protected HashSet<Vector3I> m_tmpBlockPositionsSet = new HashSet<Vector3I>();

        public event Action<MyCubeBlockDefinition> OnBlockAdded
        {
            [CompilerGenerated] add
            {
                Action<MyCubeBlockDefinition> onBlockAdded = this.OnBlockAdded;
                while (true)
                {
                    Action<MyCubeBlockDefinition> a = onBlockAdded;
                    Action<MyCubeBlockDefinition> action3 = (Action<MyCubeBlockDefinition>) Delegate.Combine(a, value);
                    onBlockAdded = Interlocked.CompareExchange<Action<MyCubeBlockDefinition>>(ref this.OnBlockAdded, action3, a);
                    if (ReferenceEquals(onBlockAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeBlockDefinition> onBlockAdded = this.OnBlockAdded;
                while (true)
                {
                    Action<MyCubeBlockDefinition> source = onBlockAdded;
                    Action<MyCubeBlockDefinition> action3 = (Action<MyCubeBlockDefinition>) Delegate.Remove(source, value);
                    onBlockAdded = Interlocked.CompareExchange<Action<MyCubeBlockDefinition>>(ref this.OnBlockAdded, action3, source);
                    if (ReferenceEquals(onBlockAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action OnBlockSizeChanged
        {
            [CompilerGenerated] add
            {
                Action onBlockSizeChanged = this.OnBlockSizeChanged;
                while (true)
                {
                    Action a = onBlockSizeChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onBlockSizeChanged = Interlocked.CompareExchange<Action>(ref this.OnBlockSizeChanged, action3, a);
                    if (ReferenceEquals(onBlockSizeChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onBlockSizeChanged = this.OnBlockSizeChanged;
                while (true)
                {
                    Action source = onBlockSizeChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onBlockSizeChanged = Interlocked.CompareExchange<Action>(ref this.OnBlockSizeChanged, action3, source);
                    if (ReferenceEquals(onBlockSizeChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyCubeBuilder()
        {
            if (Sync.IsServer)
            {
                AllPlayersColors = new Dictionary<MyPlayer.PlayerId, List<Vector3>>();
            }
        }

        public MyCubeBuilder()
        {
            this.InitializeNotifications();
        }

        public override void Activate(MyDefinitionId? blockDefinitionId = new MyDefinitionId?())
        {
            if (MySession.Static.CameraController != null)
            {
                MySession.Static.GameFocusManager.Register(this);
            }
            this.ActivateBlockCreation(blockDefinitionId);
        }

        private void ActivateBlockCreation(MyDefinitionId? blockDefinitionId = new MyDefinitionId?())
        {
            if (MySession.Static.CameraController != null)
            {
                bool allowCubeBuilding = MySession.Static.CameraController.AllowCubeBuilding;
            }
            if ((!(MySession.Static.ControlledEntity is MyShipController) || (MySession.Static.ControlledEntity as MyShipController).BuildingMode) && (!(MySession.Static.ControlledEntity is MyCharacter) || !(MySession.Static.ControlledEntity as MyCharacter).IsDead))
            {
                bool updateNotAvailableNotification = false;
                if ((this.IsCubeSizeModesAvailable && (blockDefinitionId != null)) && (this.CurrentBlockDefinition != null))
                {
                    MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(blockDefinitionId.Value);
                    MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(cubeBlockDefinition.BlockPairName);
                    if (this.OnBlockSizeChanged != null)
                    {
                        this.OnBlockSizeChanged();
                    }
                    if (((this.CurrentBlockDefinition.CubeSize != MyCubeSize.Large) || (definitionGroup.Small == null)) && ((this.CurrentBlockDefinition.CubeSize != MyCubeSize.Small) || (definitionGroup.Large == null)))
                    {
                        updateNotAvailableNotification = true;
                    }
                    else
                    {
                        MyCubeSize newCubeSize = (this.m_cubeBuilderState.CubeSizeMode == MyCubeSize.Large) ? MyCubeSize.Small : MyCubeSize.Large;
                        this.m_cubeBuilderState.SetCubeSize(newCubeSize);
                        this.SetSurvivalIntersectionDist();
                        if ((newCubeSize == MyCubeSize.Small) && (this.CubePlacementMode == CubePlacementModeEnum.LocalCoordinateSystem))
                        {
                            this.CycleCubePlacementMode();
                        }
                        int index = this.m_cubeBuilderState.CurrentBlockDefinitionStages.IndexOf(this.CurrentBlockDefinition);
                        if ((index != -1) && (this.m_cubeBuilderState.CurrentBlockDefinitionStages.Count > 0))
                        {
                            this.UpdateCubeBlockStageDefinition(this.m_cubeBuilderState.CurrentBlockDefinitionStages[index]);
                        }
                    }
                }
                else if ((this.CurrentBlockDefinition == null) && (blockDefinitionId != null))
                {
                    MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(blockDefinitionId.Value);
                    MyCubeSize cubeSizeMode = this.m_cubeBuilderState.CubeSizeMode;
                    if ((cubeBlockDefinition.CubeSize != cubeSizeMode) && (((cubeBlockDefinition.CubeSize == MyCubeSize.Large) ? MyDefinitionManager.Static.GetDefinitionGroup(cubeBlockDefinition.BlockPairName).Small : MyDefinitionManager.Static.GetDefinitionGroup(cubeBlockDefinition.BlockPairName).Large) == null))
                    {
                        cubeSizeMode = cubeBlockDefinition.CubeSize;
                    }
                    this.m_cubeBuilderState.SetCubeSize(cubeSizeMode);
                    this.CubePlacementMode = CubePlacementModeEnum.FreePlacement;
                    this.ShowCubePlacementNotification();
                }
                this.UpdateNotificationBlockNotAvailable(updateNotAvailableNotification);
                this.UpdateCubeBlockDefinition(blockDefinitionId);
                this.SetSurvivalIntersectionDist();
                if (!MySession.Static.CreativeMode)
                {
                    this.AllowFreeSpacePlacement = false;
                    this.ShowRemoveGizmo = true;
                }
                else
                {
                    this.AllowFreeSpacePlacement = false;
                    this.MaxGridDistanceFrom = null;
                    this.ShowRemoveGizmo = MyFakes.SHOW_REMOVE_GIZMO;
                }
                this.ActivateNotifications();
                if (!(MySession.Static.ControlledEntity is MyShipController) || !(MySession.Static.ControlledEntity as MyShipController).BuildingMode)
                {
                    MyHud.Crosshair.ResetToDefault(true);
                }
                this.BlockCreationIsActivated = true;
                this.AlignToGravity(true);
            }
        }

        private void ActivateBuildModeNotifications(bool joystick)
        {
        }

        private void ActivateNotifications()
        {
            if (this.m_cubePlacementModeHint != null)
            {
                this.m_cubePlacementModeHint.Level = MyNotificationLevel.Control;
                MyHud.Notifications.Add(this.m_cubePlacementModeHint);
            }
        }

        public virtual void Add()
        {
            if (this.CurrentBlockDefinition != null)
            {
                this.m_blocksBuildQueue.Clear();
                bool flag = true;
                foreach (MyCubeBuilderGizmo.MyGizmoSpaceProperties properties in this.m_gizmo.Spaces)
                {
                    if ((this.BuildInputValid && (!Sandbox.Game.Entities.MyEntities.MemoryLimitReachedReport && (properties.Enabled && properties.m_buildAllowed))) && Static.canBuild)
                    {
                        flag = false;
                        this.AddBlocksToBuildQueueOrSpawn(properties);
                    }
                }
                if (flag)
                {
                    this.NotifyPlacementUnable();
                }
                if (this.m_blocksBuildQueue.Count > 0)
                {
                    if (MyMusicController.Static != null)
                    {
                        MyMusicController.Static.Building(0x7d0);
                    }
                    this.CurrentGrid.BuildBlocks(MyPlayer.SelectedColor, this.m_blocksBuildQueue, MySession.Static.LocalCharacterEntityId, MySession.Static.LocalPlayerId);
                }
            }
        }

        private bool AddBlocksToBuildQueueOrSpawn(MyCubeBuilderGizmo.MyGizmoSpaceProperties gizmoSpace) => 
            this.AddBlocksToBuildQueueOrSpawn(gizmoSpace.m_blockDefinition, ref gizmoSpace.m_worldMatrixAdd, gizmoSpace.m_min, gizmoSpace.m_max, gizmoSpace.m_centerPos, gizmoSpace.LocalOrientation);

        public unsafe bool AddBlocksToBuildQueueOrSpawn(MyCubeBlockDefinition blockDefinition, ref MatrixD worldMatrixAdd, Vector3I min, Vector3I max, Vector3I center, Quaternion localOrientation)
        {
            MyPlayer.PlayerId id;
            MyPlayer player;
            EndpointId id2;
            Vector3D? nullable;
            bool flag = false;
            if (!MySession.Static.Players.TryGetPlayerId(MySession.Static.LocalPlayerId, out id))
            {
                return false;
            }
            if (!MySession.Static.Players.TryGetPlayerById(id, out player))
            {
                return false;
            }
            bool flag2 = MySession.Static.CreativeToolsEnabled(id.SteamId);
            if (!MySession.Static.CheckLimitsAndNotify(MySession.Static.LocalPlayerId, blockDefinition.BlockPairName, flag2 ? blockDefinition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST, 1, 0, null))
            {
                return false;
            }
            BuildData data = new BuildData();
            if (!this.GridAndBlockValid)
            {
                data.Position = worldMatrixAdd.Translation;
                if (MySession.Static.ControlledEntity == null)
                {
                    data.AbsolutePosition = true;
                }
                else
                {
                    Vector3D* vectordPtr2 = (Vector3D*) ref data.Position;
                    vectordPtr2[0] -= MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition();
                }
                data.Forward = (Vector3) worldMatrixAdd.Forward;
                data.Up = (Vector3) worldMatrixAdd.Up;
                id2 = new EndpointId();
                nullable = null;
                MyMultiplayer.RaiseStaticEvent<Author, DefinitionIdBlit, BuildData, bool, bool, uint>(s => new Action<Author, DefinitionIdBlit, BuildData, bool, bool, uint>(MyCubeBuilder.RequestGridSpawn), new Author(MySession.Static.LocalCharacterEntityId, MySession.Static.LocalPlayerId), blockDefinition.Id, data, MySession.Static.CreativeToolsEnabled(Sync.MyId), false, MyPlayer.SelectedColor.PackHSVToUint(), id2, nullable);
                flag = true;
                MySession @static = MySession.Static;
                @static.TotalBlocksCreated++;
                if (MySession.Static.ControlledEntity is MyCockpit)
                {
                    MySession session2 = MySession.Static;
                    session2.TotalBlocksCreatedFromShips++;
                }
            }
            else
            {
                if (!this.PlacingSmallGridOnLargeStatic)
                {
                    this.m_blocksBuildQueue.Add(new MyCubeGrid.MyBlockLocation(blockDefinition.Id, min, max, center, localOrientation, MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM), MySession.Static.LocalPlayerId));
                }
                else
                {
                    MatrixD xd = worldMatrixAdd;
                    data.Position = xd.Translation;
                    if (MySession.Static.ControlledEntity == null)
                    {
                        data.AbsolutePosition = true;
                    }
                    else
                    {
                        Vector3D* vectordPtr1 = (Vector3D*) ref data.Position;
                        vectordPtr1[0] -= MySession.Static.ControlledEntity.Entity.PositionComp.GetPosition();
                    }
                    data.Forward = (Vector3) xd.Forward;
                    data.Up = (Vector3) xd.Up;
                    id2 = new EndpointId();
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<Author, DefinitionIdBlit, BuildData, bool, bool, uint>(s => new Action<Author, DefinitionIdBlit, BuildData, bool, bool, uint>(MyCubeBuilder.RequestGridSpawn), new Author(MySession.Static.LocalCharacterEntityId, MySession.Static.LocalPlayerId), blockDefinition.Id, data, MySession.Static.CreativeToolsEnabled(Sync.MyId), true, MyPlayer.SelectedColor.PackHSVToUint(), id2, nullable);
                }
                flag = true;
            }
            if (this.OnBlockAdded != null)
            {
                this.OnBlockAdded(blockDefinition);
            }
            return flag;
        }

        public virtual bool AddConstruction(VRage.Game.Entity.MyEntity builder)
        {
            MyPlayer controllingPlayer = Sync.Players.GetControllingPlayer(builder);
            if (!this.canBuild || ((controllingPlayer != null) && !controllingPlayer.IsLocalPlayer))
            {
                return false;
            }
            if ((controllingPlayer == null) || controllingPlayer.IsRemotePlayer)
            {
                VRage.Game.Entity.MyEntity isUsing = (builder as MyCharacter).IsUsing;
                if (isUsing == null)
                {
                    return false;
                }
                controllingPlayer = Sync.Players.GetControllingPlayer(isUsing);
                if ((controllingPlayer == null) || controllingPlayer.IsRemotePlayer)
                {
                    return false;
                }
            }
            MyCubeBuilderGizmo.MyGizmoSpaceProperties spaceDefault = this.m_gizmo.SpaceDefault;
            if ((!spaceDefault.Enabled || (!this.BuildInputValid || (!spaceDefault.m_buildAllowed || !this.canBuild))) || Sandbox.Game.Entities.MyEntities.MemoryLimitReachedReport)
            {
                this.NotifyPlacementUnable();
                return false;
            }
            this.m_blocksBuildQueue.Clear();
            bool flag1 = this.AddBlocksToBuildQueueOrSpawn(spaceDefault);
            if ((flag1 && (this.CurrentGrid != null)) && (this.m_blocksBuildQueue.Count > 0))
            {
                if (((MySession.Static != null) && ReferenceEquals(builder, MySession.Static.LocalCharacter)) && (MyMusicController.Static != null))
                {
                    MyMusicController.Static.Building(0x7d0);
                }
                if (Sync.IsServer)
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudPlaceBlock);
                }
                if (ReferenceEquals(builder, MySession.Static.LocalCharacter))
                {
                    MySession @static = MySession.Static;
                    @static.TotalBlocksCreated++;
                    if (MySession.Static.ControlledEntity is MyCockpit)
                    {
                        MySession session2 = MySession.Static;
                        session2.TotalBlocksCreatedFromShips++;
                    }
                }
                this.CurrentGrid.BuildBlocks(MyPlayer.SelectedColor, this.m_blocksBuildQueue, builder.EntityId, controllingPlayer.Identity.IdentityId);
            }
            return flag1;
        }

        private void AddFastBuildModels(MyCubeBuilderGizmo.MyGizmoSpaceProperties gizmoSpace, MatrixD baseMatrix, List<MatrixD> matrices, List<string> models, MyCubeBlockDefinition definition)
        {
            this.AddFastBuildModels(baseMatrix, ref gizmoSpace.m_localMatrixAdd, matrices, models, definition, gizmoSpace.m_startBuild, gizmoSpace.m_continueBuild);
        }

        public unsafe void AddFastBuildModels(MatrixD baseMatrix, ref Matrix localMatrixAdd, List<MatrixD> matrices, List<string> models, MyCubeBlockDefinition definition, Vector3I? startBuild, Vector3I? continueBuild)
        {
            AddFastBuildModelWithSubparts(ref baseMatrix, matrices, models, definition, this.CurrentBlockScale);
            if (((this.CurrentBlockDefinition != null) && (startBuild != null)) && (continueBuild != null))
            {
                Vector3I vectori;
                Vector3I vectori2;
                Vector3I vectori3;
                int num;
                Vector3I.TransformNormal(ref this.CurrentBlockDefinition.Size, ref localMatrixAdd, out vectori);
                vectori = Vector3I.Abs(vectori);
                ComputeSteps(startBuild.Value, continueBuild.Value, vectori, out vectori2, out vectori3, out num);
                Vector3I zero = Vector3I.Zero;
                int num2 = 0;
                while (num2 < vectori3.X)
                {
                    zero.Y = 0;
                    int num3 = 0;
                    while (true)
                    {
                        if (num3 >= vectori3.Y)
                        {
                            num2++;
                            int* numPtr3 = (int*) ref zero.X;
                            numPtr3[0] += vectori2.X;
                            break;
                        }
                        zero.Z = 0;
                        int num4 = 0;
                        while (true)
                        {
                            Vector3 vector;
                            if (num4 >= vectori3.Z)
                            {
                                num3++;
                                int* numPtr2 = (int*) ref zero.Y;
                                numPtr2[0] += vectori2.Y;
                                break;
                            }
                            Vector3I vectori5 = zero;
                            if (this.CurrentGrid != null)
                            {
                                vector = (Vector3) Vector3.Transform((Vector3) (vectori5 * this.CurrentGrid.GridSize), this.CurrentGrid.WorldMatrix.GetOrientation());
                            }
                            else
                            {
                                vector = (Vector3) (vectori5 * MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize));
                            }
                            MatrixD matrix = baseMatrix;
                            MatrixD* xdPtr1 = (MatrixD*) ref matrix;
                            xdPtr1.Translation += vector;
                            AddFastBuildModelWithSubparts(ref matrix, matrices, models, definition, this.CurrentBlockScale);
                            num4++;
                            int* numPtr1 = (int*) ref zero.Z;
                            numPtr1[0] += vectori2.Z;
                        }
                    }
                }
            }
        }

        protected static void AfterGridBuild(VRage.Game.Entity.MyEntity builder, MyCubeGrid grid, bool instantBuild, ulong senderId)
        {
            if ((grid == null) || grid.Closed)
            {
                SpawnGridReply(false, senderId);
            }
            else
            {
                MySlimBlock cubeBlock = grid.GetCubeBlock(Vector3I.Zero);
                if (cubeBlock != null)
                {
                    if (grid.IsStatic)
                    {
                        MySlimBlock local1;
                        MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                        if ((fatBlock == null) || (fatBlock.GetBlocksCount() <= 0))
                        {
                            local1 = null;
                        }
                        else
                        {
                            local1 = fatBlock.GetBlocks()[0];
                        }
                        MySlimBlock block3 = local1;
                        MyCubeGrid grid2 = grid.DetectMerge(cubeBlock, null, null, true);
                        if (grid2 == null)
                        {
                            grid2 = grid;
                        }
                        MySlimBlock mainBlock = cubeBlock;
                        if (block3 != null)
                        {
                            mainBlock = grid2.GetCubeBlock(block3.Position);
                        }
                        grid2.AdditionalModelGenerators.ForEach(g => g.UpdateAfterGridSpawn(mainBlock));
                        if (((MyCubeGridSmallToLargeConnection.Static != null) && (Sync.IsServer && !MyCubeGridSmallToLargeConnection.Static.AddBlockSmallToLargeConnection(cubeBlock))) && (grid.GridSizeEnum == MyCubeSize.Small))
                        {
                            cubeBlock.CubeGrid.TestDynamic = MyCubeGrid.MyTestDynamicReason.GridCopied;
                        }
                    }
                    if (Sync.IsServer)
                    {
                        BuildComponent.AfterSuccessfulBuild(builder, instantBuild);
                    }
                    if (cubeBlock.FatBlock != null)
                    {
                        cubeBlock.FatBlock.OnBuildSuccess(cubeBlock.BuiltBy, instantBuild);
                    }
                    if (grid.IsStatic && (grid.GridSizeEnum != MyCubeSize.Small))
                    {
                        MatrixD worldMatrix = grid.WorldMatrix;
                        if (MyCoordinateSystem.Static.IsLocalCoordSysExist(ref worldMatrix, (double) grid.GridSize))
                        {
                            MyCoordinateSystem.Static.RegisterCubeGrid(grid);
                        }
                        else
                        {
                            MyCoordinateSystem.Static.CreateCoordSys(grid, MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.StaticGridAlignToCenter, true);
                        }
                    }
                    MyCubeGrids.NotifyBlockBuilt(grid, cubeBlock);
                    SpawnGridReply(true, senderId);
                }
            }
        }

        public void AlignToGravity(bool alignToCamera = true)
        {
            Vector3 direction = MyGravityProviderSystem.CalculateTotalGravityInPoint(MyBlockBuilderBase.IntersectionStart);
            if (direction.LengthSquared() > 0f)
            {
                Matrix worldMatrixAdd = (Matrix) this.m_gizmo.SpaceDefault.m_worldMatrixAdd;
                direction.Normalize();
                Vector3D vectord = !((MySector.MainCamera != null) & alignToCamera) ? Vector3D.Reject(this.m_gizmo.SpaceDefault.m_worldMatrixAdd.Forward, direction) : Vector3D.Reject(MySector.MainCamera.ForwardVector, direction);
                if (!vectord.IsValid() || (vectord.LengthSquared() <= double.Epsilon))
                {
                    vectord = Vector3D.CalculatePerpendicularVector(direction);
                }
                vectord.Normalize();
                this.m_gizmo.SpaceDefault.m_worldMatrixAdd = Matrix.CreateWorld(worldMatrixAdd.Translation, (Vector3) vectord, -direction);
            }
        }

        public static List<MyCubeBlockDefinition.MountPoint> AutogenerateMountpoints(MyModel model, float gridSize)
        {
            HkShape[] havokCollisionShapes = model.HavokCollisionShapes;
            if (havokCollisionShapes == null)
            {
                if (model.HavokBreakableShapes == null)
                {
                    return new List<MyCubeBlockDefinition.MountPoint>();
                }
                havokCollisionShapes = new HkShape[] { model.HavokBreakableShapes[0].GetShape() };
            }
            return AutogenerateMountpoints(havokCollisionShapes, gridSize);
        }

        public static List<MyCubeBlockDefinition.MountPoint> AutogenerateMountpoints(HkShape[] shapes, float gridSize)
        {
            HkShapeCutterUtil cutter = new HkShapeCutterUtil();
            List<BoundingBox>[] listArray1 = new List<BoundingBox>[Base6Directions.EnumDirections.Length];
            List<MyCubeBlockDefinition.MountPoint> mountPoints = new List<MyCubeBlockDefinition.MountPoint>();
            Base6Directions.Direction[] enumDirections = Base6Directions.EnumDirections;
            int index = 0;
            while (index < enumDirections.Length)
            {
                int num2 = (int) enumDirections[index];
                Vector3 direction = Base6Directions.Directions[num2];
                HkShape[] shapeArray = shapes;
                int num3 = 0;
                while (true)
                {
                    if (num3 < shapeArray.Length)
                    {
                        HkShape shape = shapeArray[num3];
                        if (shape.ShapeType != HkShapeType.List)
                        {
                            if (shape.ShapeType != HkShapeType.Mopp)
                            {
                                FindMountPoint(cutter, shape, direction, gridSize, mountPoints);
                                num3++;
                                continue;
                            }
                            HkMoppBvTreeShape shape6 = (HkMoppBvTreeShape) shape;
                            int num4 = 0;
                            while (true)
                            {
                                HkShapeCollection shapeCollection = shape6.ShapeCollection;
                                if (num4 >= shapeCollection.ShapeCount)
                                {
                                    break;
                                }
                                HkShape shape7 = shape6.ShapeCollection.GetShape((uint) num4, null);
                                if (shape7.ShapeType == HkShapeType.ConvexTranslate)
                                {
                                    FindMountPoint(cutter, ((HkConvexTranslateShape) shape7).Base, direction, gridSize, mountPoints);
                                }
                                num4++;
                            }
                        }
                        else
                        {
                            HkShapeContainerIterator iterator = ((HkListShape) shape).GetIterator();
                            while (iterator.IsValid)
                            {
                                HkShape currentValue = iterator.CurrentValue;
                                if (currentValue.ShapeType == HkShapeType.ConvexTransform)
                                {
                                    FindMountPoint(cutter, ((HkConvexTransformShape) currentValue).Base, direction, gridSize, mountPoints);
                                }
                                else if (currentValue.ShapeType == HkShapeType.ConvexTranslate)
                                {
                                    FindMountPoint(cutter, ((HkConvexTranslateShape) currentValue).Base, direction, gridSize, mountPoints);
                                }
                                else
                                {
                                    FindMountPoint(cutter, currentValue, direction, gridSize, mountPoints);
                                }
                                iterator.Next();
                            }
                        }
                    }
                    index++;
                    break;
                }
            }
            return mountPoints;
        }

        private void BeforeCurrentGridChange(MyCubeGrid newCurrentGrid)
        {
            this.TriggerRespawnShipNotification(newCurrentGrid);
        }

        public static unsafe bool CalculateBlockRotation(int index, int sign, ref MatrixD currentMatrix, out MatrixD rotatedMatrix, double angle, MyBlockDirection blockDirection = 3, MyBlockRotation blockRotation = 3)
        {
            MatrixD identity = MatrixD.Identity;
            if (index == 2)
            {
                sign *= -1;
            }
            Vector3D zero = Vector3D.Zero;
            switch (index)
            {
                case 0:
                {
                    double* numPtr1 = (double*) ref zero.X;
                    numPtr1[0] += sign * angle;
                    identity = MatrixD.CreateFromAxisAngle(currentMatrix.Right, sign * angle);
                    break;
                }
                case 1:
                {
                    double* numPtr2 = (double*) ref zero.Y;
                    numPtr2[0] += sign * angle;
                    identity = MatrixD.CreateFromAxisAngle(currentMatrix.Up, sign * angle);
                    break;
                }
                case 2:
                {
                    double* numPtr3 = (double*) ref zero.Z;
                    numPtr3[0] += sign * angle;
                    identity = MatrixD.CreateFromAxisAngle(currentMatrix.Forward, sign * angle);
                    break;
                }
                default:
                    break;
            }
            rotatedMatrix = currentMatrix;
            rotatedMatrix *= identity;
            rotatedMatrix = MatrixD.Orthogonalize(rotatedMatrix);
            bool flag = CheckValidBlockRotation((Matrix) rotatedMatrix, blockDirection, blockRotation);
            if (flag && !Static.DynamicMode)
            {
                if (!Static.m_animationLock)
                {
                    Static.m_animationLock = true;
                }
                else
                {
                    flag = !flag;
                }
            }
            return flag;
        }

        private void CalculateCubePlacement()
        {
            if (this.IsActivated && (this.CurrentBlockDefinition != null))
            {
                this.ChooseHitObject();
                Vector3D worldPos = (base.m_hitInfo != null) ? base.m_hitInfo.Value.Position : (MyBlockBuilderBase.IntersectionStart + (MyBlockBuilderBase.IntersectionDistance * MyBlockBuilderBase.IntersectionDirection));
                if (this.CurrentBlockDefinition != null)
                {
                    float cubeSize = MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize);
                    if (MyCoordinateSystem.Static != null)
                    {
                        long localCoordSystem;
                        double num2 = (worldPos - this.m_lastLocalCoordSysData.Origin.Position).LengthSquared();
                        if ((base.m_currentGrid == null) || (base.m_currentGrid.LocalCoordSystem == this.m_lastLocalCoordSysData.Id))
                        {
                            localCoordSystem = (num2 > MyCoordinateSystem.Static.CoordSystemSizeSquared) ? 0L : this.m_lastLocalCoordSysData.Id;
                        }
                        else
                        {
                            localCoordSystem = base.m_currentGrid.LocalCoordSystem;
                        }
                        long num3 = localCoordSystem;
                        this.m_lastLocalCoordSysData = MyCoordinateSystem.Static.SnapWorldPosToClosestGrid(ref worldPos, (double) cubeSize, MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.StaticGridAlignToCenter, new long?(num3));
                    }
                    switch (this.CubePlacementMode)
                    {
                        case CubePlacementModeEnum.LocalCoordinateSystem:
                            this.CalculateLocalCoordinateSystemMode(worldPos);
                            return;

                        case CubePlacementModeEnum.FreePlacement:
                            this.CalculateFreePlacementMode(worldPos);
                            return;

                        case CubePlacementModeEnum.GravityAligned:
                            this.CalculateGravityAlignedMode(worldPos);
                            return;
                    }
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected void CalculateFreePlacementMode(Vector3D position)
        {
            this.DynamicMode = (base.m_currentGrid == null) || this.IsDynamicOverride();
        }

        protected void CalculateGravityAlignedMode(Vector3D position)
        {
            this.DynamicMode = (base.m_currentGrid == null) || this.IsDynamicOverride();
            if (!this.m_animationLock)
            {
                this.AlignToGravity(false);
            }
        }

        protected void CalculateLocalCoordinateSystemMode(Vector3D position)
        {
            this.DynamicMode = (base.m_currentGrid == null) && ReferenceEquals(base.m_currentVoxelBase, null);
        }

        protected virtual bool CaluclateDynamicModePos(Vector3D defaultPos, bool isDynamicOverride = false)
        {
            bool valid = true;
            if (!this.FreezeGizmo)
            {
                this.m_gizmo.SpaceDefault.m_worldMatrixAdd.Translation = defaultPos;
                if (isDynamicOverride)
                {
                    defaultPos = this.GetFreeSpacePlacementPosition(out valid);
                    this.m_gizmo.SpaceDefault.m_worldMatrixAdd.Translation = defaultPos;
                }
            }
            return valid;
        }

        protected virtual bool CancelBuilding()
        {
            if (this.m_gizmo.SpaceDefault.m_continueBuild == null)
            {
                return false;
            }
            this.m_gizmo.SpaceDefault.m_startBuild = null;
            this.m_gizmo.SpaceDefault.m_startRemove = null;
            this.m_gizmo.SpaceDefault.m_continueBuild = null;
            return true;
        }

        public virtual bool CanStartConstruction(VRage.Game.Entity.MyEntity buildingEntity)
        {
            MatrixD worldMatrixAdd = this.m_gizmo.SpaceDefault.m_worldMatrixAdd;
            BuildComponent.GetGridSpawnMaterials(this.CurrentBlockDefinition, worldMatrixAdd, false);
            return BuildComponent.HasBuildingMaterials(buildingEntity, false);
        }

        private void Change(int expand = 0)
        {
            this.m_tmpBlockPositionList.Clear();
            if (expand == -1)
            {
                this.CurrentGrid.ColorGrid(MyPlayer.SelectedColor, true, true);
            }
            int index = -1;
            bool playSound = false;
            foreach (MyCubeBuilderGizmo.MyGizmoSpaceProperties properties in this.m_gizmo.Spaces)
            {
                index++;
                if (properties.Enabled && (properties.m_removeBlock != null))
                {
                    playSound = false;
                    Vector3I min = properties.m_removeBlock.Position - (Vector3I.One * expand);
                    Vector3I max = (Vector3I) (properties.m_removeBlock.Position + (Vector3I.One * expand));
                    if ((m_currColoringArea[index].Start != min) || (m_currColoringArea[index].End != max))
                    {
                        m_currColoringArea[index].Start = min;
                        m_currColoringArea[index].End = max;
                        playSound = true;
                    }
                    this.CurrentGrid.ColorBlocks(min, max, MyPlayer.SelectedColor, playSound, true);
                }
            }
        }

        protected bool CheckSmallViewChange()
        {
            double num = (this.m_initialIntersectionStart - MyBlockBuilderBase.IntersectionStart).Length();
            return ((Vector3D.Dot(this.m_initialIntersectionDirection, MyBlockBuilderBase.IntersectionDirection) > 0.99800002574920654) && (num < 0.25));
        }

        public static unsafe bool CheckValidBlockRotation(Matrix localMatrix, MyBlockDirection blockDirection, MyBlockRotation blockRotation)
        {
            Vector3I vectori = Vector3I.Round(localMatrix.Forward);
            Vector3I vectori2 = Vector3I.Round(localMatrix.Up);
            Vector3I* vectoriPtr1 = (Vector3I*) ref vectori;
            Vector3I* vectoriPtr2 = (Vector3I*) ref vectori2;
            int num = Vector3I.Dot(ref (Vector3I) ref vectoriPtr2, ref vectori2);
            if ((Vector3I.Dot(ref (Vector3I) ref vectoriPtr1, ref vectori) > 1) || (num > 1))
            {
                return (blockDirection == MyBlockDirection.Both);
            }
            return ((blockDirection != MyBlockDirection.Horizontal) || ((vectori != Vector3I.Up) && (!(vectori == -Vector3I.Up) && ((blockRotation != MyBlockRotation.Vertical) || !(vectori2 != Vector3I.Up)))));
        }

        public static bool CheckValidBlocksRotation(Matrix gridLocalMatrix, MyCubeGrid grid)
        {
            bool flag = true;
            using (HashSet<MySlimBlock>.Enumerator enumerator = grid.GetBlocks().GetEnumerator())
            {
                do
                {
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            Matrix matrix;
                            MySlimBlock current = enumerator.Current;
                            MyCompoundCubeBlock fatBlock = current.FatBlock as MyCompoundCubeBlock;
                            if (fatBlock != null)
                            {
                                foreach (MySlimBlock block3 in fatBlock.GetBlocks())
                                {
                                    block3.Orientation.GetMatrix(out matrix);
                                    matrix *= gridLocalMatrix;
                                    flag = flag && CheckValidBlockRotation(matrix, block3.BlockDefinition.Direction, block3.BlockDefinition.Rotation);
                                    if (!flag)
                                    {
                                        break;
                                    }
                                }
                                break;
                            }
                            current.Orientation.GetMatrix(out matrix);
                            matrix *= gridLocalMatrix;
                            flag = flag && CheckValidBlockRotation(matrix, current.BlockDefinition.Direction, current.BlockDefinition.Rotation);
                        }
                        else
                        {
                            return flag;
                        }
                        break;
                    }
                }
                while (flag);
            }
            return flag;
        }

        protected internal override void ChooseHitObject()
        {
            if (!this.IsBuilding())
            {
                base.ChooseHitObject();
                this.m_gizmo.Clear();
            }
        }

        protected void ClearRenderData()
        {
            this.m_renderData.BeginCollectingInstanceData();
            this.m_renderData.EndCollectingInstanceData((this.CurrentGrid != null) ? this.CurrentGrid.WorldMatrix : MatrixD.Identity, this.UseTransparency);
        }

        public virtual void ContinueBuilding(bool planeBuild)
        {
            MyCubeBuilderGizmo.MyGizmoSpaceProperties spaceDefault = this.m_gizmo.SpaceDefault;
            this.ContinueBuilding(planeBuild, spaceDefault.m_startBuild, spaceDefault.m_startRemove, ref spaceDefault.m_continueBuild, spaceDefault.m_min, spaceDefault.m_max);
        }

        protected unsafe void ContinueBuilding(bool planeBuild, Vector3I? startBuild, Vector3I? startRemove, ref Vector3I? continueBuild, Vector3I blockMinPosision, Vector3I blockMaxPosition)
        {
            if (((startBuild != null) || (startRemove != null)) && (this.GridAndBlockValid || this.VoxelMapAndBlockValid))
            {
                continueBuild = 0;
                if (!this.CheckSmallViewChange())
                {
                    Vector3I vectori3;
                    base.IntersectInflated(m_cacheGridIntersections, this.CurrentGrid);
                    Vector3I vectori = (startBuild != null) ? blockMinPosision : startRemove.Value;
                    Vector3I vectori2 = (startBuild != null) ? blockMaxPosition : startRemove.Value;
                    vectori3.X = vectori.X;
                    while (true)
                    {
                        while (true)
                        {
                            if (vectori3.X > vectori2.X)
                            {
                                return;
                            }
                            vectori3.Y = vectori.Y;
                            break;
                        }
                        while (true)
                        {
                            if (vectori3.Y > vectori2.Y)
                            {
                                int* numPtr3 = (int*) ref vectori3.X;
                                numPtr3[0]++;
                                break;
                            }
                            vectori3.Z = vectori.Z;
                            while (true)
                            {
                                if (vectori3.Z > vectori2.Z)
                                {
                                    int* numPtr2 = (int*) ref vectori3.Y;
                                    numPtr2[0]++;
                                    break;
                                }
                                if (planeBuild)
                                {
                                    foreach (Vector3I vectori4 in m_cacheGridIntersections)
                                    {
                                        if (((vectori4.X == vectori3.X) || (vectori4.Y == vectori3.Y)) || (vectori4.Z == vectori3.Z))
                                        {
                                            if (vectori4.X == vectori3.X)
                                            {
                                                if (this.CurrentGrid != null)
                                                {
                                                    this.CurrentGrid.WorldMatrix.Up;
                                                    this.CurrentGrid.WorldMatrix.Forward;
                                                }
                                            }
                                            else if (vectori4.Y == vectori3.Y)
                                            {
                                                if (this.CurrentGrid != null)
                                                {
                                                    this.CurrentGrid.WorldMatrix.Right;
                                                    this.CurrentGrid.WorldMatrix.Forward;
                                                }
                                            }
                                            else if (vectori4.Z == vectori3.Z)
                                            {
                                                if (this.CurrentGrid != null)
                                                {
                                                    this.CurrentGrid.WorldMatrix.Up;
                                                    this.CurrentGrid.WorldMatrix.Right;
                                                }
                                            }
                                            Vector3I vectori5 = (Vector3I) (Vector3I.Abs(vectori4 - vectori3) + Vector3I.One);
                                            if ((vectori5.Size < 0x800) && (vectori5.AbsMax() <= 0xff))
                                            {
                                                continueBuild = new Vector3I?(vectori4);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (Vector3I vectori6 in m_cacheGridIntersections)
                                    {
                                        if (((((vectori6.X == vectori3.X) && (vectori6.Y == vectori3.Y)) || ((vectori6.Y == vectori3.Y) && (vectori6.Z == vectori3.Z))) || ((vectori6.X == vectori3.X) && (vectori6.Z == vectori3.Z))) && (((vectori6 - vectori3) + Vector3I.One).AbsMax() <= 0xff))
                                        {
                                            continueBuild = new Vector3I?(vectori6);
                                            break;
                                        }
                                    }
                                }
                                int* numPtr1 = (int*) ref vectori3.Z;
                                numPtr1[0]++;
                            }
                        }
                    }
                }
            }
        }

        public static MyObjectBuilder_CubeBlock ConvertDynamicGridBlockToStatic(ref MatrixD worldMatrix, MyObjectBuilder_CubeBlock origBlock)
        {
            MyCubeBlockDefinition definition;
            Quaternion quaternion;
            MyDefinitionId defId = new MyDefinitionId(origBlock.TypeId, origBlock.SubtypeName);
            MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out definition);
            if (definition == null)
            {
                return null;
            }
            MyObjectBuilder_CubeBlock block1 = MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) defId) as MyObjectBuilder_CubeBlock;
            block1.EntityId = origBlock.EntityId;
            origBlock.BlockOrientation.GetQuaternion(out quaternion);
            Matrix matrix = Matrix.CreateFromQuaternion(quaternion);
            matrix * worldMatrix;
            block1.Orientation = Quaternion.CreateFromRotationMatrix(matrix);
            Vector3I vectori = Vector3I.Abs(Vector3I.Round(Vector3.TransformNormal((Vector3) definition.Size, matrix)));
            Vector3I min = (Vector3I) origBlock.Min;
            Vector3I vectori3 = ((Vector3I) (origBlock.Min + vectori)) - Vector3I.One;
            Vector3I.Round(Vector3.TransformNormal((Vector3) min, worldMatrix));
            Vector3I.Round(Vector3.TransformNormal((Vector3) vectori3, worldMatrix));
            block1.Min = Vector3I.Min(min, vectori3);
            block1.MultiBlockId = origBlock.MultiBlockId;
            block1.MultiBlockDefinition = origBlock.MultiBlockDefinition;
            block1.MultiBlockIndex = origBlock.MultiBlockIndex;
            block1.BuildPercent = origBlock.BuildPercent;
            block1.IntegrityPercent = origBlock.BuildPercent;
            return block1;
        }

        public static MyObjectBuilder_CubeGrid ConvertGridBuilderToStatic(MyObjectBuilder_CubeGrid originalGrid, MatrixD worldMatrix)
        {
            MyObjectBuilder_CubeGrid grid = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_CubeGrid>();
            grid.EntityId = originalGrid.EntityId;
            grid.PositionAndOrientation = new MyPositionAndOrientation(worldMatrix.Translation, (Vector3) worldMatrix.Forward, (Vector3) worldMatrix.Up);
            grid.GridSizeEnum = originalGrid.GridSizeEnum;
            grid.IsStatic = true;
            grid.PersistentFlags |= MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
            foreach (MyObjectBuilder_CubeBlock block in originalGrid.CubeBlocks)
            {
                if (!(block is MyObjectBuilder_CompoundCubeBlock))
                {
                    MyObjectBuilder_CubeBlock block6 = ConvertDynamicGridBlockToStatic(ref worldMatrix, block);
                    if (block6 == null)
                    {
                        continue;
                    }
                    grid.CubeBlocks.Add(block6);
                    continue;
                }
                MyObjectBuilder_CompoundCubeBlock block2 = block as MyObjectBuilder_CompoundCubeBlock;
                MyObjectBuilder_CompoundCubeBlock item = ConvertDynamicGridBlockToStatic(ref worldMatrix, block) as MyObjectBuilder_CompoundCubeBlock;
                if (item != null)
                {
                    item.Blocks = new MyObjectBuilder_CubeBlock[block2.Blocks.Length];
                    int index = 0;
                    while (true)
                    {
                        if (index >= block2.Blocks.Length)
                        {
                            grid.CubeBlocks.Add(item);
                            break;
                        }
                        MyObjectBuilder_CubeBlock origBlock = block2.Blocks[index];
                        MyObjectBuilder_CubeBlock block5 = ConvertDynamicGridBlockToStatic(ref worldMatrix, origBlock);
                        if (block5 != null)
                        {
                            item.Blocks[index] = block5;
                        }
                        index++;
                    }
                }
            }
            return grid;
        }

        protected static MyObjectBuilder_CubeGrid CreateMultiBlockGridBuilder(MyMultiBlockDefinition multiCubeBlockDefinition, Matrix rotationMatrix, Vector3D position = new Vector3D())
        {
            int num2;
            MyObjectBuilder_CubeGrid grid = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_CubeGrid>();
            grid.PositionAndOrientation = new MyPositionAndOrientation(position, rotationMatrix.Forward, rotationMatrix.Up);
            grid.IsStatic = false;
            grid.PersistentFlags |= MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
            if (multiCubeBlockDefinition.BlockDefinitions == null)
            {
                return null;
            }
            MyCubeSize? nullable = null;
            Vector3I maxValue = Vector3I.MaxValue;
            Vector3I minValue = Vector3I.MinValue;
            int num = MyRandom.Instance.Next();
            while (true)
            {
                if (num != 0)
                {
                    num2 = 0;
                    break;
                }
                num = MyRandom.Instance.Next();
            }
            while (true)
            {
                while (true)
                {
                    MyCubeBlockDefinition definition2;
                    if (num2 >= multiCubeBlockDefinition.BlockDefinitions.Length)
                    {
                        if (grid.CubeBlocks.Count == 0)
                        {
                            return null;
                        }
                        grid.GridSizeEnum = nullable.Value;
                        return grid;
                    }
                    MyMultiBlockDefinition.MyMultiBlockPartDefinition definition = multiCubeBlockDefinition.BlockDefinitions[num2];
                    MyDefinitionManager.Static.TryGetCubeBlockDefinition(definition.Id, out definition2);
                    if (definition2 != null)
                    {
                        if (nullable == null)
                        {
                            nullable = new MyCubeSize?(definition2.CubeSize);
                        }
                        else if (((MyCubeSize) nullable.Value) != definition2.CubeSize)
                        {
                            break;
                        }
                        MyObjectBuilder_CubeBlock item = MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) definition2.Id) as MyObjectBuilder_CubeBlock;
                        item.Orientation = Base6Directions.GetOrientation(definition.Forward, definition.Up);
                        item.Min = definition.Min;
                        item.ColorMaskHSV = MyPlayer.SelectedColor;
                        item.MultiBlockId = num;
                        item.MultiBlockIndex = num2;
                        item.MultiBlockDefinition = new SerializableDefinitionId?((SerializableDefinitionId) multiCubeBlockDefinition.Id);
                        item.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                        bool flag = false;
                        bool flag2 = true;
                        bool flag3 = MyCompoundCubeBlock.IsCompoundEnabled(definition2);
                        foreach (MyObjectBuilder_CubeBlock block2 in grid.CubeBlocks)
                        {
                            if (block2.Min == item.Min)
                            {
                                if (!MyFakes.ENABLE_COMPOUND_BLOCKS || !(block2 is MyObjectBuilder_CompoundCubeBlock))
                                {
                                    flag2 = false;
                                }
                                else if (!flag3)
                                {
                                    flag2 = false;
                                }
                                else
                                {
                                    MyObjectBuilder_CompoundCubeBlock block3 = block2 as MyObjectBuilder_CompoundCubeBlock;
                                    MyObjectBuilder_CubeBlock[] destinationArray = new MyObjectBuilder_CubeBlock[block3.Blocks.Length + 1];
                                    Array.Copy(block3.Blocks, destinationArray, block3.Blocks.Length);
                                    destinationArray[destinationArray.Length - 1] = item;
                                    block3.Blocks = destinationArray;
                                    flag = true;
                                }
                                break;
                            }
                        }
                        if (flag2)
                        {
                            if (!flag)
                            {
                                if (!MyFakes.ENABLE_COMPOUND_BLOCKS || !MyCompoundCubeBlock.IsCompoundEnabled(definition2))
                                {
                                    grid.CubeBlocks.Add(item);
                                }
                                else
                                {
                                    MyObjectBuilder_CompoundCubeBlock block4 = MyCompoundCubeBlock.CreateBuilder(item);
                                    grid.CubeBlocks.Add(block4);
                                }
                            }
                            maxValue = Vector3I.Min(maxValue, definition.Min);
                            minValue = Vector3I.Max(minValue, definition.Min);
                        }
                    }
                    break;
                }
                num2++;
            }
        }

        protected void CycleCubePlacementMode()
        {
            switch (this.CubePlacementMode)
            {
                case CubePlacementModeEnum.LocalCoordinateSystem:
                    this.CubePlacementMode = CubePlacementModeEnum.FreePlacement;
                    return;

                case CubePlacementModeEnum.FreePlacement:
                    this.CubePlacementMode = CubePlacementModeEnum.GravityAligned;
                    return;

                case CubePlacementModeEnum.GravityAligned:
                    int num1;
                    if ((this.CurrentBlockDefinition == null) || (this.CurrentBlockDefinition.CubeSize != MyCubeSize.Large))
                    {
                        num1 = 1;
                    }
                    else
                    {
                        num1 = 0;
                    }
                    this.CubePlacementMode = (CubePlacementModeEnum) num1;
                    return;
            }
            throw new ArgumentOutOfRangeException();
        }

        public override void Deactivate()
        {
            if (base.Loaded)
            {
                if (this.BlockCreationIsActivated)
                {
                    this.DeactivateBlockCreation();
                }
                if (this.m_cubeBuilderState != null)
                {
                    this.CurrentBlockDefinition = null;
                }
                this.m_stationPlacement = false;
                this.CurrentGrid = null;
                this.CurrentVoxelBase = null;
                this.IsBuildMode = false;
                MyBlockBuilderBase.PlacementProvider = null;
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    this.m_rotationHints.ReleaseRenderData();
                }
                MyCoordinateSystem.Static.Visible = false;
                MyHud.Notifications.Remove(this.m_cubePlacementModeNotification);
                MyHud.Notifications.Remove(this.m_cubePlacementModeHint);
            }
        }

        public void DeactivateBlockCreation()
        {
            if ((this.m_cubeBuilderState != null) && (this.m_cubeBuilderState.CurrentBlockDefinition != null))
            {
                this.m_cubeBuilderState.UpdateCubeBlockDefinition(new MyDefinitionId?(this.m_cubeBuilderState.CurrentBlockDefinition.Id), this.m_gizmo.SpaceDefault.m_localMatrixAdd);
            }
            this.BlockCreationIsActivated = false;
            this.DeactivateNotifications();
        }

        private void DeactivateBuildModeNotifications()
        {
        }

        private void DeactivateNotifications()
        {
        }

        private void DebugDraw()
        {
            if ((MyPerGameSettings.EnableAi && MyDebugDrawSettings.ENABLE_DEBUG_DRAW) && (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES != MyWEMDebugDrawMode.NONE))
            {
                MyCubeBlockDefinition currentBlockDefinition = this.CurrentBlockDefinition;
                if ((currentBlockDefinition != null) && (this.CurrentGrid != null))
                {
                    Matrix worldMatrixAdd = (Matrix) this.m_gizmo.SpaceDefault.m_worldMatrixAdd;
                    worldMatrixAdd.Translation = (Vector3) Vector3.Transform((Vector3) (this.m_gizmo.SpaceDefault.m_addPos * 2.5f), this.CurrentGrid.PositionComp.WorldMatrix);
                    worldMatrixAdd = Matrix.Rescale(worldMatrixAdd, this.CurrentGrid.GridSize);
                    if (currentBlockDefinition.NavigationDefinition != null)
                    {
                        MyGridNavigationMesh mesh = currentBlockDefinition.NavigationDefinition.Mesh;
                    }
                }
            }
            if (MyFakes.ENABLE_DEBUG_DRAW_TEXTURE_NAMES)
            {
                this.DebugDrawModelTextures();
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_MODEL_INFO)
            {
                this.DebugDrawModelInfo();
            }
            if (MyFakes.ENABLE_DEBUG_DRAW_GENERATING_BLOCK)
            {
                this.DebugDrawGeneratingBlock();
            }
        }

        private static void DebugDrawBareBlockInfo(MySlimBlock block, ref float yPos)
        {
            yPos += 20f;
            MyRenderProxy.DebugDrawText2D(new Vector2(20f, yPos), $"Display Name: {block.BlockDefinition.DisplayNameText}", Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            yPos += 10f;
            MyRenderProxy.DebugDrawText2D(new Vector2(20f, yPos), $"Cube type: {block.BlockDefinition.CubeDefinition.CubeTopology}", Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            yPos += 10f;
            using (IEnumerator<string> enumerator = block.BlockDefinition.CubeDefinition.Model.Distinct<string>().OrderBy<string, string>(s => s, StringComparer.InvariantCultureIgnoreCase).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    DebugDrawModelInfo(MyModels.GetModel(enumerator.Current), ref yPos);
                }
            }
        }

        private void DebugDrawGeneratingBlock()
        {
            LineD line = new LineD(MyBlockBuilderBase.IntersectionStart, MyBlockBuilderBase.IntersectionStart + (MyBlockBuilderBase.IntersectionDirection * 200.0));
            MyIntersectionResultLineTriangleEx? nullable = Sandbox.Game.Entities.MyEntities.GetIntersectionWithLine(ref line, MySession.Static.LocalCharacter, null, false, true, true, IntersectionFlags.ALL_TRIANGLES, 0f, true);
            if ((nullable != null) && (nullable.Value.Entity is MyCubeGrid))
            {
                MyIntersectionResultLineTriangleEx? t = null;
                MySlimBlock slimBlock = null;
                if (((nullable.Value.Entity as MyCubeGrid).GetIntersectionWithLine(ref line, out t, out slimBlock, IntersectionFlags.ALL_TRIANGLES) && ((t != null) && (slimBlock != null))) && slimBlock.BlockDefinition.IsGeneratedBlock)
                {
                    this.DebugDrawGeneratingBlock(slimBlock);
                }
            }
        }

        private void DebugDrawGeneratingBlock(MySlimBlock generatedBlock)
        {
            MySlimBlock generatingBlock = generatedBlock.CubeGrid.GetGeneratingBlock(generatedBlock);
            if (generatingBlock != null)
            {
                string[] textArray1 = new string[] { "Generated SubTypeId: ", generatedBlock.BlockDefinition.Id.SubtypeName, " ", generatedBlock.Min.ToString(), " ", generatedBlock.Orientation.ToString() };
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, 0f), string.Concat(textArray1), Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                string[] textArray2 = new string[] { "Generating SubTypeId: ", generatingBlock.BlockDefinition.Id.SubtypeName, " ", generatingBlock.Min.ToString(), " ", generatingBlock.Orientation.ToString() };
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, 14f), string.Concat(textArray2), Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                VRageMath.Vector4 vector = new VRageMath.Vector4(Color.Blue.ToVector3() * 0.8f, 1f);
                MyStringId? lineMaterial = null;
                DrawSemiTransparentBox(generatingBlock.CubeGrid, generatingBlock, Color.Blue, false, lineMaterial, new VRageMath.Vector4?(vector));
            }
        }

        private void DebugDrawModelInfo()
        {
            LineD line = new LineD(MyBlockBuilderBase.IntersectionStart, MyBlockBuilderBase.IntersectionStart + (MyBlockBuilderBase.IntersectionDirection * 1000.0));
            MyIntersectionResultLineTriangleEx? nullable = Sandbox.Game.Entities.MyEntities.GetIntersectionWithLine(ref line, MySession.Static.LocalCharacter, null, false, false, true, IntersectionFlags.ALL_TRIANGLES, 0f, false);
            IMyEntity hitEntity = null;
            Vector3D zero = Vector3D.Zero;
            if (nullable != null)
            {
                hitEntity = nullable.Value.Entity;
                zero = nullable.Value.IntersectionPointInWorldSpace;
            }
            MyPhysics.HitInfo? nullable2 = MyPhysics.CastRay(line.From, line.To, 30);
            if ((nullable2 != null) && ((nullable == null) || ((nullable2.Value.Position - line.From).Length() < (zero - line.From).Length())))
            {
                hitEntity = nullable2.Value.HkHitInfo.GetHitEntity();
                zero = nullable2.Value.Position;
            }
            float y = 20f;
            switch (hitEntity)
            {
                case (null):
                    MyRenderProxy.DebugDrawText2D(new Vector2(20f, 20f), "Nothing detected nearby", Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    break;

                default:
                {
                    double num2 = (zero - line.From).Length();
                    if (hitEntity is MyEnvironmentSector)
                    {
                        MyEnvironmentSector sector = hitEntity as MyEnvironmentSector;
                        MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), "Type: EnvironmentSector " + sector.SectorId, Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        y += 10f;
                        short modelIndex = sector.GetModelIndex(sector.GetItemFromShapeKey(nullable2.Value.HkHitInfo.GetShapeKey(0)));
                        DebugDrawModelInfo(MyModels.GetModelOnlyData(sector.Owner.GetModelForId(modelIndex).Model), ref y);
                    }
                    else if (hitEntity is MyVoxelBase)
                    {
                        MyVoxelBase self = (MyVoxelBase) hitEntity;
                        if (self.RootVoxel != null)
                        {
                            self = self.RootVoxel;
                        }
                        Vector3D worldPosition = zero;
                        MyVoxelMaterialDefinition materialAt = self.GetMaterialAt(ref worldPosition);
                        if (self.RootVoxel is MyPlanet)
                        {
                            MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), "Type: planet/moon", Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            y += 10f;
                            MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), "Terrain: " + materialAt, Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            y += 10f;
                        }
                        else
                        {
                            MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), "Type: asteroid", Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            y += 10f;
                            MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), "Terrain: " + materialAt, Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            y += 10f;
                        }
                        MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), "Object size: " + self.SizeInMetres, Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        y += 10f;
                    }
                    else if (!(hitEntity is MyCubeGrid))
                    {
                        if (!(nullable.Value.Entity is MyCubeBlock))
                        {
                            MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), "Unknown object detected", Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            y += 10f;
                        }
                        else
                        {
                            MyCubeBlock block = (MyCubeBlock) nullable.Value.Entity;
                            MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), "Detected block", Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            y += 10f;
                            MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), $"Block name: {block.DisplayName}", Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            y += 10f;
                            DebugDrawModelTextures(block, ref y);
                        }
                    }
                    else
                    {
                        MyIntersectionResultLineTriangleEx? nullable3;
                        MySlimBlock block;
                        MyCubeGrid grid = (MyCubeGrid) hitEntity;
                        MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), "Detected grid object", Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        y += 10f;
                        MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), $"Grid name: {grid.DisplayName}", Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        y += 10f;
                        if ((grid.GetIntersectionWithLine(ref line, out nullable3, out block, IntersectionFlags.ALL_TRIANGLES) && (nullable3 != null)) && (block != null))
                        {
                            if (block.FatBlock != null)
                            {
                                DebugDrawModelTextures(block.FatBlock, ref y);
                            }
                            else
                            {
                                DebugDrawBareBlockInfo(block, ref y);
                            }
                        }
                    }
                    MyRenderProxy.DebugDrawText2D(new Vector2(20f, y), "Distance " + num2 + "m", Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    break;
                }
            }
        }

        private static void DebugDrawModelInfo(MyModel model, ref float yPos)
        {
            MyRenderProxy.DebugDrawText2D(new Vector2(20f, yPos), "Asset: " + model.AssetName, Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            yPos += 10f;
            int startIndex = model.AssetName.LastIndexOf(@"\") + 1;
            if ((startIndex == -1) || (startIndex >= model.AssetName.Length))
            {
                MyTomasInputComponent.ClipboardText = model.AssetName;
            }
            else
            {
                MyTomasInputComponent.ClipboardText = model.AssetName.Substring(startIndex);
            }
            DebugDrawTexturesInfo(model, ref yPos);
        }

        private void DebugDrawModelTextures()
        {
            LineD line = new LineD(MyBlockBuilderBase.IntersectionStart, MyBlockBuilderBase.IntersectionStart + (MyBlockBuilderBase.IntersectionDirection * 200.0));
            MyIntersectionResultLineTriangleEx? nullable = Sandbox.Game.Entities.MyEntities.GetIntersectionWithLine(ref line, MySession.Static.LocalCharacter, null, false, true, true, IntersectionFlags.ALL_TRIANGLES, 0f, true);
            if (nullable != null)
            {
                float yPos = 0f;
                if (nullable.Value.Entity is MyCubeGrid)
                {
                    MyIntersectionResultLineTriangleEx? t = null;
                    MySlimBlock slimBlock = null;
                    if (((nullable.Value.Entity as MyCubeGrid).GetIntersectionWithLine(ref line, out t, out slimBlock, IntersectionFlags.ALL_TRIANGLES) && (t != null)) && (slimBlock != null))
                    {
                        DebugDrawModelTextures(slimBlock.FatBlock, ref yPos);
                    }
                }
            }
        }

        private static void DebugDrawModelTextures(MyCubeBlock block, ref float yPos)
        {
            MyModel model = null;
            if (block != null)
            {
                model = block.Model;
            }
            if (model != null)
            {
                yPos += 20f;
                MyRenderProxy.DebugDrawText2D(new Vector2(20f, yPos), "SubTypeId: " + block.BlockDefinition.Id.SubtypeName, Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                yPos += 10f;
                MyRenderProxy.DebugDrawText2D(new Vector2(20f, yPos), "Display name: " + block.BlockDefinition.DisplayNameText, Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                yPos += 10f;
                if (block.SlimBlock.IsMultiBlockPart)
                {
                    MyCubeGridMultiBlockInfo multiBlockInfo = block.CubeGrid.GetMultiBlockInfo(block.SlimBlock.MultiBlockId);
                    if (multiBlockInfo != null)
                    {
                        object[] objArray1 = new object[] { "Multiblock: ", multiBlockInfo.MultiBlockDefinition.Id.SubtypeName, " (Id:", block.SlimBlock.MultiBlockId, ")" };
                        MyRenderProxy.DebugDrawText2D(new Vector2(20f, yPos), string.Concat(objArray1), Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        yPos += 10f;
                    }
                }
                if (block.BlockDefinition.IsGeneratedBlock)
                {
                    MyRenderProxy.DebugDrawText2D(new Vector2(20f, yPos), "Generated block: " + block.BlockDefinition.GeneratedBlockType, Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    yPos += 10f;
                }
                MyRenderProxy.DebugDrawText2D(new Vector2(20f, yPos), "BlockID: " + block.EntityId, Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                yPos += 10f;
                if (block.ModelCollision != null)
                {
                    MyRenderProxy.DebugDrawText2D(new Vector2(20f, yPos), "Collision: " + block.ModelCollision.AssetName, Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
                yPos += 10f;
                DebugDrawModelInfo(model, ref yPos);
            }
        }

        private static void DebugDrawTexturesInfo(MyModel model, ref float yPos)
        {
            HashSet<string> source = new HashSet<string>();
            foreach (VRageRender.Models.MyMesh mesh in model.GetMeshList())
            {
                if (mesh.Material.Textures == null)
                {
                    source.Add("<null material>");
                    continue;
                }
                source.Add("Material: " + mesh.Material.Name);
                foreach (string str in mesh.Material.Textures.Values)
                {
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        source.Add(str);
                    }
                }
            }
            foreach (string str2 in source.OrderBy<string, string>(s => s, StringComparer.InvariantCultureIgnoreCase))
            {
                MyRenderProxy.DebugDrawText2D(new Vector2(20f, yPos), str2, Color.White, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                yPos += 10f;
            }
        }

        public override unsafe void Draw()
        {
            Vector3D vectord4;
            base.Draw();
            this.DebugDraw();
            if (this.BlockCreationIsActivated)
            {
                MyHud.Crosshair.Recenter();
            }
            if (!this.IsActivated)
            {
                goto TR_0000;
            }
            else if (this.CurrentBlockDefinition != null)
            {
                int num1;
                if (!this.BuildInputValid)
                {
                    this.ClearRenderData();
                    return;
                }
                this.DrawBuildingStepsCount(this.m_gizmo.SpaceDefault.m_startBuild, this.m_gizmo.SpaceDefault.m_startRemove, this.m_gizmo.SpaceDefault.m_continueBuild, ref this.m_gizmo.SpaceDefault.m_localMatrixAdd);
                bool addPos = this.m_gizmo.SpaceDefault.m_startBuild != null;
                bool removePos = false;
                float gridSize = 0f;
                if (this.CurrentBlockDefinition != null)
                {
                    gridSize = MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize);
                }
                if (this.DynamicMode)
                {
                    PlaneD ed = new PlaneD(MySector.MainCamera.Position, MySector.MainCamera.UpVector);
                    Vector3D intersectionStart = MyBlockBuilderBase.IntersectionStart;
                    Vector3D defaultPos = ed.ProjectPoint(ref intersectionStart) + (MyBlockBuilderBase.IntersectionDistance * MyBlockBuilderBase.IntersectionDirection);
                    if (base.m_hitInfo != null)
                    {
                        defaultPos = base.m_hitInfo.Value.Position;
                    }
                    addPos = this.CaluclateDynamicModePos(defaultPos, this.IsDynamicOverride());
                    MyCoordinateSystem.Static.Visible = false;
                }
                else if ((this.m_gizmo.SpaceDefault.m_startBuild == null) && (this.m_gizmo.SpaceDefault.m_startRemove == null))
                {
                    if (!this.FreezeGizmo)
                    {
                        if (this.CurrentGrid != null)
                        {
                            MyCoordinateSystem.Static.Visible = false;
                            if (this.GetAddAndRemovePositions(gridSize, this.PlacingSmallGridOnLargeStatic, out this.m_gizmo.SpaceDefault.m_addPos, out this.m_gizmo.SpaceDefault.m_addPosSmallOnLarge, out this.m_gizmo.SpaceDefault.m_addDir, out this.m_gizmo.SpaceDefault.m_removePos, out this.m_gizmo.SpaceDefault.m_removeBlock, out this.m_gizmo.SpaceDefault.m_blockIdInCompound, this.m_gizmo.SpaceDefault.m_removeBlocksInMultiBlock) || (this.m_gizmo.SpaceDefault.m_removeBlock != null))
                            {
                                this.m_gizmo.SpaceDefault.m_localMatrixAdd.Translation = !this.PlacingSmallGridOnLargeStatic ? ((Vector3) this.m_gizmo.SpaceDefault.m_addPos) : this.m_gizmo.SpaceDefault.m_addPosSmallOnLarge.Value;
                                this.m_gizmo.SpaceDefault.m_worldMatrixAdd = this.m_gizmo.SpaceDefault.m_localMatrixAdd * this.CurrentGrid.WorldMatrix;
                                Vector3I? singleMountPointNormal = this.GetSingleMountPointNormal();
                                if (((singleMountPointNormal != null) && this.GridAndBlockValid) && (this.m_gizmo.SpaceDefault.m_addDir != Vector3I.Zero))
                                {
                                    this.m_gizmo.SetupLocalAddMatrix(this.m_gizmo.SpaceDefault, singleMountPointNormal.Value);
                                }
                            }
                        }
                        else
                        {
                            MyCoordinateSystem.Static.Visible = true;
                            Vector3D localSnappedPos = this.m_lastLocalCoordSysData.LocalSnappedPos;
                            if (!MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.StaticGridAlignToCenter)
                            {
                                localSnappedPos -= new Vector3D(0.5 * gridSize, 0.5 * gridSize, -0.5 * gridSize);
                            }
                            Vector3I vectori = Vector3I.Round(localSnappedPos / ((double) gridSize));
                            this.m_gizmo.SpaceDefault.m_addPos = vectori;
                            this.m_gizmo.SpaceDefault.m_localMatrixAdd.Translation = (Vector3) this.m_lastLocalCoordSysData.LocalSnappedPos;
                            this.m_gizmo.SpaceDefault.m_worldMatrixAdd = this.m_lastLocalCoordSysData.Origin.TransformMatrix;
                            addPos = true;
                        }
                    }
                    if (this.m_gizmo.SpaceDefault.m_removeBlock != null)
                    {
                        removePos = true;
                    }
                }
                if ((MySession.Static.ControlledEntity == null) || !(MySession.Static.ControlledEntity is MyCockpit))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) !MyBlockBuilderBase.SpectatorIsBuilding;
                }
                if (num1 == 0)
                {
                    if (this.IsInSymmetrySettingMode)
                    {
                        this.m_gizmo.SpaceDefault.m_continueBuild = null;
                        addPos = false;
                        removePos = false;
                        if (this.m_gizmo.SpaceDefault.m_removeBlock != null)
                        {
                            Vector3 center = ((this.m_gizmo.SpaceDefault.m_removeBlock.Min * this.CurrentGrid.GridSize) + (this.m_gizmo.SpaceDefault.m_removeBlock.Max * this.CurrentGrid.GridSize)) * 0.5f;
                            Color color = this.DrawSymmetryPlane(this.m_symmetrySettingMode, this.CurrentGrid, center);
                            VRageMath.Vector4? lineColor = null;
                            DrawSemiTransparentBox(this.CurrentGrid, this.m_gizmo.SpaceDefault.m_removeBlock, color.ToVector4(), false, new MyStringId?(ID_GIZMO_DRAW_LINE_RED), lineColor);
                        }
                    }
                    if ((this.CurrentGrid != null) && (this.UseSymmetry || this.IsInSymmetrySettingMode))
                    {
                        if (this.CurrentGrid.XSymmetryPlane != null)
                        {
                            Vector3 center = this.CurrentGrid.XSymmetryPlane.Value * this.CurrentGrid.GridSize;
                            this.DrawSymmetryPlane(this.CurrentGrid.XSymmetryOdd ? MySymmetrySettingModeEnum.XPlaneOdd : MySymmetrySettingModeEnum.XPlane, this.CurrentGrid, center);
                        }
                        if (this.CurrentGrid.YSymmetryPlane != null)
                        {
                            Vector3 center = this.CurrentGrid.YSymmetryPlane.Value * this.CurrentGrid.GridSize;
                            this.DrawSymmetryPlane(this.CurrentGrid.YSymmetryOdd ? MySymmetrySettingModeEnum.YPlaneOdd : MySymmetrySettingModeEnum.YPlane, this.CurrentGrid, center);
                        }
                        if (this.CurrentGrid.ZSymmetryPlane != null)
                        {
                            Vector3 center = this.CurrentGrid.ZSymmetryPlane.Value * this.CurrentGrid.GridSize;
                            this.DrawSymmetryPlane(this.CurrentGrid.ZSymmetryOdd ? MySymmetrySettingModeEnum.ZPlaneOdd : MySymmetrySettingModeEnum.ZPlane, this.CurrentGrid, center);
                        }
                    }
                }
                this.UpdateGizmos(addPos, removePos, true);
                if ((this.CurrentGrid != null) && (!this.DynamicMode || (this.CurrentGrid == null)))
                {
                    this.m_renderData.EndCollectingInstanceData(this.CurrentGrid.WorldMatrix, this.UseTransparency);
                    goto TR_0002;
                }
            }
            else
            {
                goto TR_0000;
            }
            MatrixD worldMatrixAdd = this.m_gizmo.SpaceDefault.m_worldMatrixAdd;
            Vector3D.TransformNormal(ref this.CurrentBlockDefinition.ModelOffset, ref worldMatrixAdd, out vectord4);
            MatrixD* xdPtr1 = (MatrixD*) ref worldMatrixAdd;
            xdPtr1.Translation = worldMatrixAdd.Translation + vectord4;
            this.m_renderData.EndCollectingInstanceData(worldMatrixAdd, this.UseTransparency);
            goto TR_0002;
        TR_0000:
            this.ClearRenderData();
            return;
        TR_0002:
            UpdateBlockInfoHud();
        }

        protected void DrawBuildingStepsCount(Vector3I? startBuild, Vector3I? startRemove, Vector3I? continueBuild, ref Matrix localMatrixAdd)
        {
            Vector3I? nullable2 = startBuild;
            Vector3I? nullable = (nullable2 != null) ? nullable2 : startRemove;
            if ((nullable != null) && (continueBuild != null))
            {
                Vector3I vectori;
                int num;
                Vector3I vectori2;
                Vector3I vectori3;
                Vector3I.TransformNormal(ref this.CurrentBlockDefinition.Size, ref localMatrixAdd, out vectori);
                vectori = Vector3I.Abs(vectori);
                ComputeSteps(nullable.Value, continueBuild.Value, (startBuild != null) ? vectori : Vector3I.One, out vectori2, out vectori3, out num);
                this.m_cubeCountStringBuilder.Clear();
                StringBuilder builder = MyTexts.Get(MyCommonTexts.Clipboard_TotalBlocks);
                this.m_cubeCountStringBuilder.Append(builder);
                this.m_cubeCountStringBuilder.AppendInt32(num);
                Color? colorMask = null;
                MyGuiManager.DrawString("White", this.m_cubeCountStringBuilder, new Vector2(0.51f, 0.51f), 0.7f, colorMask, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
            }
        }

        public static void DrawMountPoints(float cubeSize, MyCubeBlockDefinition def, ref MatrixD drawMatrix)
        {
            MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = def.GetBuildProgressModelMountPoints(1f);
            if (buildProgressModelMountPoints != null)
            {
                if (!MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AUTOGENERATE)
                {
                    DrawMountPoints(cubeSize, def, drawMatrix, buildProgressModelMountPoints);
                }
                else if (def.Model != null)
                {
                    int shapeIndex = 0;
                    MyModel model = MyModels.GetModel(def.Model);
                    HkShape[] havokCollisionShapes = model.HavokCollisionShapes;
                    int index = 0;
                    while (true)
                    {
                        if (index >= havokCollisionShapes.Length)
                        {
                            DrawMountPoints(cubeSize, def, drawMatrix, AutogenerateMountpoints(model, cubeSize).ToArray());
                            break;
                        }
                        MyPhysicsDebugDraw.DrawCollisionShape(havokCollisionShapes[index], drawMatrix, 0.2f, ref shapeIndex, null, false);
                        index++;
                    }
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS_HELPERS)
                {
                    DrawMountPointsAxisHelpers(def, ref drawMatrix, cubeSize);
                }
            }
        }

        public static void DrawMountPoints(float cubeSize, MyCubeBlockDefinition def, MatrixD drawMatrix, MyCubeBlockDefinition.MountPoint[] mountPoints)
        {
            Color yellow = Color.Yellow;
            Color blue = Color.Blue;
            Vector3I center = def.Center;
            Vector3 vector = (Vector3) (def.Size * 0.5f);
            MatrixD transform = MatrixD.CreateTranslation((Vector3) ((center - vector) * cubeSize)) * drawMatrix;
            for (int i = 0; i < mountPoints.Length; i++)
            {
                if ((((mountPoints[i].Normal != Base6Directions.IntDirections[0]) || MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS0) && (((mountPoints[i].Normal != Base6Directions.IntDirections[1]) || MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS1) && (((mountPoints[i].Normal != Base6Directions.IntDirections[2]) || MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS2) && (((mountPoints[i].Normal != Base6Directions.IntDirections[3]) || MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS3) && ((mountPoints[i].Normal != Base6Directions.IntDirections[4]) || MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS4))))) && ((mountPoints[i].Normal != Base6Directions.IntDirections[5]) || MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS_AXIS5))
                {
                    Vector3 vector2 = mountPoints[i].Start - center;
                    Vector3 vector3 = mountPoints[i].End - center;
                    BoundingBoxD box = new BoundingBoxD(Vector3.Min(vector2, vector3) * cubeSize, Vector3.Max(vector2, vector3) * cubeSize);
                    MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(box, transform), mountPoints[i].Default ? blue : yellow, 0.2f, true, false, false);
                }
            }
        }

        private static unsafe void DrawMountPointsAxisHelpers(MyCubeBlockDefinition def, ref MatrixD drawMatrix, float cubeSize)
        {
            MyStringId? nullable;
            Vector3 vector = (Vector3) (def.Size * 0.5f);
            MatrixD matrix = (MatrixD.CreateTranslation(((Vector3) def.Center) - vector) * MatrixD.CreateScale((double) cubeSize)) * drawMatrix;
            for (int i = 0; i < 6; i++)
            {
                Base6Directions.Direction mountPointDirection = (Base6Directions.Direction) ((byte) i);
                Vector3D zero = Vector3D.Zero;
                zero.Z = -0.20000000298023224;
                Vector3D forward = Vector3.Forward;
                Vector3D right = Vector3.Right;
                Vector3D up = Vector3.Up;
                zero = Vector3D.Transform((Vector3D) def.MountPointLocalToBlockLocal((Vector3) zero, mountPointDirection), matrix);
                forward = Vector3D.TransformNormal((Vector3D) def.MountPointLocalNormalToBlockLocal((Vector3) forward, mountPointDirection), matrix);
                up = Vector3D.TransformNormal((Vector3D) def.MountPointLocalNormalToBlockLocal((Vector3) up, mountPointDirection), matrix);
                right = Vector3D.TransformNormal((Vector3D) def.MountPointLocalNormalToBlockLocal((Vector3) right, mountPointDirection), matrix);
                MatrixD worldMatrix = MatrixD.CreateWorld(zero + (right * 0.25), forward, right);
                MatrixD xd4 = MatrixD.CreateWorld(zero + (up * 0.25), forward, up);
                VRageMath.Vector4 vctColor = Color.Red.ToVector4();
                VRageMath.Vector4 vector3 = Color.Green.ToVector4();
                MyRenderProxy.DebugDrawSphere(zero, 0.03f * cubeSize, Color.Red.ToVector3(), 1f, true, false, true, false);
                nullable = null;
                MySimpleObjectDraw.DrawTransparentCylinder(ref worldMatrix, 0f, 0.03f * cubeSize, 0.5f * cubeSize, ref vctColor, false, 0x10, 0.01f * cubeSize, nullable);
                nullable = null;
                MySimpleObjectDraw.DrawTransparentCylinder(ref xd4, 0f, 0.03f * cubeSize, 0.5f * cubeSize, ref vector3, false, 0x10, 0.01f * cubeSize, nullable);
                MyRenderProxy.DebugDrawLine3D(zero, zero - (forward * 0.20000000298023224), Color.Red, Color.Red, true, false);
                float scale = 0.5f * cubeSize;
                float num3 = 0.5f * cubeSize;
                float num4 = 0.5f * cubeSize;
                if (MySector.MainCamera != null)
                {
                    float num5 = (float) ((zero + (right * 0.550000011920929)) - MySector.MainCamera.Position).Length();
                    float num6 = (float) ((zero + (up * 0.550000011920929)) - MySector.MainCamera.Position).Length();
                    float num7 = (float) ((zero + (forward * 0.10000000149011612)) - MySector.MainCamera.Position).Length();
                    scale = (scale * 6f) / num5;
                    num3 = (num3 * 6f) / num6;
                    num4 = (num4 * 6f) / num7;
                }
                MyRenderProxy.DebugDrawText3D(zero + (right * 0.550000011920929), "X", Color.Red, scale, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                MyRenderProxy.DebugDrawText3D(zero + (up * 0.550000011920929), "Y", Color.Green, num3, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                MyRenderProxy.DebugDrawText3D(zero + (forward * 0.10000000149011612), m_mountPointSideNames[i], Color.White, num4, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
            }
            BoundingBoxD xd2 = new BoundingBoxD((Vector3D) ((-def.Size * cubeSize) * 0.5f), (Vector3D) ((def.Size * cubeSize) * 0.5f));
            Color black = Color.Black;
            if ((((float) (matrix.Translation - MySector.MainCamera.Position).Length()) - (((float) xd2.Size.Max()) * 0.866f)) < (cubeSize * 3f))
            {
                Color* colorPtr1 = (Color*) ref black;
                BoundingBoxD* xdPtr1 = (BoundingBoxD*) ref xd2;
                nullable = null;
                nullable = null;
                MySimpleObjectDraw.DrawTransparentBox(ref drawMatrix, ref (BoundingBoxD) ref xdPtr1, ref (Color) ref colorPtr1, ref black, MySimpleObjectRasterizer.Wireframe, def.Size * 10, (0.005f / ((float) xd2.Size.Max())) * cubeSize, nullable, nullable, true, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
            }
        }

        protected static unsafe void DrawRemovingCubes(Vector3I? startRemove, Vector3I? continueBuild, MySlimBlock removeBlock)
        {
            if (((startRemove != null) && (continueBuild != null)) && (removeBlock != null))
            {
                Vector3I vectori;
                Vector3I vectori2;
                int num;
                Color white = Color.White;
                ComputeSteps(startRemove.Value, continueBuild.Value, Vector3I.One, out vectori, out vectori2, out num);
                MatrixD worldMatrix = removeBlock.CubeGrid.WorldMatrix;
                BoundingBoxD localbox = BoundingBoxD.CreateInvalid();
                localbox.Include(startRemove.Value * removeBlock.CubeGrid.GridSize);
                localbox.Include(continueBuild.Value * removeBlock.CubeGrid.GridSize);
                Vector3D* vectordPtr1 = (Vector3D*) ref localbox.Min;
                vectordPtr1[0] -= new Vector3((removeBlock.CubeGrid.GridSize / 2f) + 0.02f);
                Vector3D* vectordPtr2 = (Vector3D*) ref localbox.Max;
                vectordPtr2[0] += new Vector3((removeBlock.CubeGrid.GridSize / 2f) + 0.02f);
                Color* colorPtr1 = (Color*) ref white;
                MyStringId? faceMaterial = null;
                MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref localbox, ref (Color) ref colorPtr1, ref white, MySimpleObjectRasterizer.Wireframe, vectori2, 0.04f, faceMaterial, new MyStringId?(ID_GIZMO_DRAW_LINE_RED), true, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
                Color color = new Color(Color.Red * 0.2f, 0.3f);
                faceMaterial = null;
                MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref localbox, ref color, MySimpleObjectRasterizer.Solid, 0, 0.04f, new MyStringId?(ID_SQUARE), faceMaterial, true, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
            }
        }

        public static void DrawSemiTransparentBox(MyCubeGrid grid, MySlimBlock block, Color color, bool onlyWireframe = false, MyStringId? lineMaterial = new MyStringId?(), VRageMath.Vector4? lineColor = new VRageMath.Vector4?())
        {
            DrawSemiTransparentBox(block.Min, block.Max, grid, color, onlyWireframe, lineMaterial, lineColor);
        }

        public static void DrawSemiTransparentBox(Vector3I minPosition, Vector3I maxPosition, MyCubeGrid grid, Color color, bool onlyWireframe = false, MyStringId? lineMaterial = new MyStringId?(), VRageMath.Vector4? lineColor = new VRageMath.Vector4?())
        {
            float gridSize = grid.GridSize;
            Vector3 max = (maxPosition * gridSize) + new Vector3((gridSize / 2f) * SEMI_TRANSPARENT_BOX_MODIFIER);
            BoundingBoxD localbox = new BoundingBoxD((Vector3D) ((minPosition * gridSize) - new Vector3((gridSize / 2f) * SEMI_TRANSPARENT_BOX_MODIFIER)), max);
            MatrixD worldMatrix = grid.WorldMatrix;
            Color white = Color.White;
            if (lineColor != null)
            {
                white = lineColor.Value;
            }
            MyStringId? faceMaterial = null;
            MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref localbox, ref white, MySimpleObjectRasterizer.Wireframe, 1, 0.04f, faceMaterial, lineMaterial, false, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
            if (!onlyWireframe)
            {
                Color color3 = new Color(color * 0.2f, 0.3f);
                faceMaterial = null;
                MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref localbox, ref color3, MySimpleObjectRasterizer.Solid, 0, 0.04f, new MyStringId?(ID_SQUARE), faceMaterial, true, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
            }
        }

        private unsafe Color DrawSymmetryPlane(MySymmetrySettingModeEnum plane, MyCubeGrid localGrid, Vector3 center)
        {
            Vector3D vectord;
            Vector3D vectord2;
            Vector3D vectord3;
            Vector3D vectord4;
            MatrixD xd;
            BoundingBox localAABB = localGrid.PositionComp.LocalAABB;
            float num = 1f;
            float gridSize = localGrid.GridSize;
            Vector3 zero = Vector3.Zero;
            Vector3 position = Vector3.Zero;
            Vector3 vector3 = Vector3.Zero;
            Vector3 vector4 = Vector3.Zero;
            float a = 0.1f;
            Color gray = Color.Gray;
            if (plane > MySymmetrySettingModeEnum.YPlane)
            {
                if (plane != MySymmetrySettingModeEnum.YPlaneOdd)
                {
                    if ((plane == MySymmetrySettingModeEnum.ZPlane) || (plane == MySymmetrySettingModeEnum.ZPlaneOdd))
                    {
                        gray = new Color(0f, 0f, 1f, a);
                        center.X = 0f;
                        center.Y = 0f;
                        float* singlePtr3 = (float*) ref center.Z;
                        singlePtr3[0] -= localAABB.Center.Z - ((plane == MySymmetrySettingModeEnum.ZPlaneOdd) ? (localGrid.GridSize * 0.50025f) : 0f);
                        zero = (new Vector3((localAABB.HalfExtents.X * num) + gridSize, (localAABB.HalfExtents.Y * num) + gridSize, 0f) + localAABB.Center) + center;
                        position = (new Vector3((-localAABB.HalfExtents.X * num) - gridSize, (localAABB.HalfExtents.Y * num) + gridSize, 0f) + localAABB.Center) + center;
                        vector3 = (new Vector3((localAABB.HalfExtents.X * num) + gridSize, (-localAABB.HalfExtents.Y * num) - gridSize, 0f) + localAABB.Center) + center;
                        vector4 = (new Vector3((-localAABB.HalfExtents.X * num) - gridSize, (-localAABB.HalfExtents.Y * num) - gridSize, 0f) + localAABB.Center) + center;
                    }
                    goto TR_0000;
                }
            }
            else if ((plane != MySymmetrySettingModeEnum.XPlane) && (plane != MySymmetrySettingModeEnum.XPlaneOdd))
            {
                if (plane != MySymmetrySettingModeEnum.YPlane)
                {
                    goto TR_0000;
                }
            }
            else
            {
                gray = new Color(1f, 0f, 0f, a);
                float* singlePtr1 = (float*) ref center.X;
                singlePtr1[0] -= localAABB.Center.X + ((plane == MySymmetrySettingModeEnum.XPlaneOdd) ? (localGrid.GridSize * 0.50025f) : 0f);
                center.Y = 0f;
                center.Z = 0f;
                zero = (new Vector3(0f, (localAABB.HalfExtents.Y * num) + gridSize, (localAABB.HalfExtents.Z * num) + gridSize) + localAABB.Center) + center;
                position = (new Vector3(0f, (-localAABB.HalfExtents.Y * num) - gridSize, (localAABB.HalfExtents.Z * num) + gridSize) + localAABB.Center) + center;
                vector3 = (new Vector3(0f, (localAABB.HalfExtents.Y * num) + gridSize, (-localAABB.HalfExtents.Z * num) - gridSize) + localAABB.Center) + center;
                vector4 = (new Vector3(0f, (-localAABB.HalfExtents.Y * num) - gridSize, (-localAABB.HalfExtents.Z * num) - gridSize) + localAABB.Center) + center;
                goto TR_0000;
            }
            gray = new Color(0f, 1f, 0f, a);
            center.X = 0f;
            float* singlePtr2 = (float*) ref center.Y;
            singlePtr2[0] -= localAABB.Center.Y + ((plane == MySymmetrySettingModeEnum.YPlaneOdd) ? (localGrid.GridSize * 0.50025f) : 0f);
            center.Z = 0f;
            zero = (new Vector3((localAABB.HalfExtents.X * num) + gridSize, 0f, (localAABB.HalfExtents.Z * num) + gridSize) + localAABB.Center) + center;
            position = (new Vector3((-localAABB.HalfExtents.X * num) - gridSize, 0f, (localAABB.HalfExtents.Z * num) + gridSize) + localAABB.Center) + center;
            vector3 = (new Vector3((localAABB.HalfExtents.X * num) + gridSize, 0f, (-localAABB.HalfExtents.Z * num) - gridSize) + localAABB.Center) + center;
            vector4 = (new Vector3((-localAABB.HalfExtents.X * num) - gridSize, 0f, (-localAABB.HalfExtents.Z * num) - gridSize) + localAABB.Center) + center;
        TR_0000:
            xd = this.CurrentGrid.WorldMatrix;
            Vector3D.Transform(ref zero, ref xd, out vectord);
            Vector3D.Transform(ref position, ref xd, out vectord2);
            Vector3D.Transform(ref vector3, ref xd, out vectord3);
            Vector3D.Transform(ref vector4, ref xd, out vectord4);
            MyRenderProxy.DebugDrawTriangle(vectord, vectord2, vectord3, gray, true, true, false);
            MyRenderProxy.DebugDrawTriangle(vectord3, vectord2, vectord4, gray, true, true, false);
            return gray;
        }

        private static unsafe bool FindMountPoint(HkShapeCutterUtil cutter, HkShape shape, Vector3 direction, float gridSize, List<MyCubeBlockDefinition.MountPoint> mountPoints)
        {
            Vector3 vector;
            Vector3 vector2;
            float d = (gridSize * 0.75f) / 2f;
            Plane plane = new Plane(-direction, d);
            float num2 = 0.2f;
            if (!cutter.Cut(shape, new VRageMath.Vector4(plane.Normal.X, plane.Normal.Y, plane.Normal.Z, plane.D), out vector, out vector2))
            {
                return false;
            }
            BoundingBox box = new BoundingBox(vector, vector2);
            box.InflateToMinimum(new Vector3(num2));
            float num3 = gridSize * 0.5f;
            MyCubeBlockDefinition.MountPoint item = new MyCubeBlockDefinition.MountPoint {
                Normal = new Vector3I(direction),
                Start = (box.Min + new Vector3(num3)) / gridSize,
                End = (box.Max + new Vector3(num3)) / gridSize,
                Enabled = true
            };
            Vector3 vector3 = Vector3.Abs(direction) * item.Start;
            Vector3* vectorPtr1 = (Vector3*) ref item.Start;
            vectorPtr1[0] -= vector3;
            Vector3* vectorPtr2 = (Vector3*) ref item.Start;
            vectorPtr2[0] -= direction * 0.04f;
            Vector3* vectorPtr3 = (Vector3*) ref item.End;
            vectorPtr3[0] -= Vector3.Abs(direction) * item.End;
            Vector3* vectorPtr4 = (Vector3*) ref item.End;
            vectorPtr4[0] += direction * 0.04f;
            if (vector3.AbsMax() > 0.5f)
            {
                Vector3* vectorPtr5 = (Vector3*) ref item.Start;
                vectorPtr5[0] += Vector3.Abs(direction);
                Vector3* vectorPtr6 = (Vector3*) ref item.End;
                vectorPtr6[0] += Vector3.Abs(direction);
            }
            mountPoints.Add(item);
            return true;
        }

        public virtual bool GetAddAndRemovePositions(float gridSize, bool placingSmallGridOnLargeStatic, out Vector3I addPos, out Vector3? addPosSmallOnLarge, out Vector3I addDir, out Vector3I removePos, out MySlimBlock removeBlock, out ushort? compoundBlockId, HashSet<Tuple<MySlimBlock, ushort?>> removeBlocksInMultiBlock)
        {
            MySlimBlock block;
            Vector3D vectord;
            Vector3D vectord2;
            bool flag = false;
            addPosSmallOnLarge = 0;
            removePos = new Vector3I();
            removeBlock = null;
            flag = base.GetBlockAddPosition(gridSize, placingSmallGridOnLargeStatic, out block, out vectord, out vectord2, out addPos, out addDir, out compoundBlockId);
            float num = placingSmallGridOnLargeStatic ? this.CurrentGrid.GridSize : gridSize;
            if ((this.MaxGridDistanceFrom == null) || (Vector3D.DistanceSquared(vectord2 * num, Vector3D.Transform(this.MaxGridDistanceFrom.Value, base.m_invGridWorldMatrix)) < (MyBlockBuilderBase.CubeBuilderDefinition.MaxBlockBuildingDistance * MyBlockBuilderBase.CubeBuilderDefinition.MaxBlockBuildingDistance)))
            {
                removePos = Vector3I.Round(vectord);
                removeBlock = block;
                if (((removeBlock != null) && (removeBlock.FatBlock != null)) && ReferenceEquals(MySession.Static.ControlledEntity as MyShipController, removeBlock.FatBlock))
                {
                    removeBlock = null;
                }
            }
            else if (!this.AllowFreeSpacePlacement || (this.CurrentGrid == null))
            {
                flag = false;
            }
            else
            {
                Vector3D position = MyBlockBuilderBase.IntersectionStart + (MyBlockBuilderBase.IntersectionDirection * Math.Min(this.FreeSpacePlacementDistance, MyBlockBuilderBase.IntersectionDistance));
                addPos = this.MakeCubePosition(position);
                addDir = new Vector3I(0, 0, 1);
                removePos = addPos - addDir;
                removeBlock = this.CurrentGrid.GetCubeBlock(removePos);
                if (((removeBlock != null) && (removeBlock.FatBlock != null)) && ReferenceEquals(MySession.Static.ControlledEntity as MyShipController, removeBlock.FatBlock))
                {
                    removeBlock = null;
                }
                flag = true;
            }
            if (!Static.canBuild)
            {
                return false;
            }
            if (flag & placingSmallGridOnLargeStatic)
            {
                MatrixD identity = Matrix.Identity;
                if (block != null)
                {
                    identity = block.CubeGrid.WorldMatrix.GetOrientation();
                    if (block.FatBlock != null)
                    {
                        if (compoundBlockId == 0)
                        {
                            if (block.FatBlock.Components.Has<MyFractureComponentBase>())
                            {
                                return false;
                            }
                        }
                        else
                        {
                            MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
                            if (fatBlock != null)
                            {
                                MySlimBlock block3 = fatBlock.GetBlock(compoundBlockId.Value);
                                if ((block3 != null) && block3.FatBlock.Components.Has<MyFractureComponentBase>())
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                MatrixD.Invert(identity);
                if (base.m_hitInfo != null)
                {
                    Vector3 vector2 = Vector3.TransformNormal(base.m_hitInfo.Value.HkHitInfo.Normal, base.m_invGridWorldMatrix);
                    addDir = Vector3I.Sign(Vector3.DominantAxisProjection(vector2));
                }
                Vector3 vector = removePos + (0.5f * addDir);
                Vector3D vectord4 = vectord2 - vector;
                Vector3I vectori = Vector3I.Abs(addDir);
                float num2 = gridSize / this.CurrentGrid.GridSize;
                addPosSmallOnLarge = new Vector3?((Vector3) ((num2 * Vector3I.Round((Vector3D) ((((vector + (((Vector3I.One - vectori) * Vector3.Clamp((Vector3) vectord4, new Vector3(-0.495f), new Vector3(0.495f))) + (vectori * vectord4))) + (((MyFakes.ENABLE_VR_BUILDING ? 0.25f : 0.1f) * num2) * addDir)) - (num2 * Vector3.Half)) / ((double) num2)))) + (num2 * Vector3.Half)));
            }
            return flag;
        }

        public void GetAddPosition(out Vector3D position)
        {
            position = this.m_gizmo.SpaceDefault.m_worldMatrixAdd.Translation;
        }

        public static void GetAllBlocksPositions(HashSet<Tuple<MySlimBlock, ushort?>> blockInCompoundIDs, HashSet<Vector3I> outPositions)
        {
            foreach (Tuple<MySlimBlock, ushort?> tuple in blockInCompoundIDs)
            {
                Vector3I min = tuple.Item1.Min;
                Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref tuple.Item1.Min, ref tuple.Item1.Max);
                while (iterator.IsValid())
                {
                    outPositions.Add(min);
                    iterator.GetNext(out min);
                }
            }
        }

        public MyOrientedBoundingBoxD GetBuildBoundingBox(float inflate = 0f)
        {
            if (this.m_gizmo.SpaceDefault.m_blockDefinition == null)
            {
                return new MyOrientedBoundingBoxD();
            }
            float cubeSize = MyDefinitionManager.Static.GetCubeSize(this.m_gizmo.SpaceDefault.m_blockDefinition.CubeSize);
            Vector3 vector = ((this.m_gizmo.SpaceDefault.m_blockDefinition.Size * cubeSize) * 0.5f) + inflate;
            MatrixD worldMatrixAdd = this.m_gizmo.SpaceDefault.m_worldMatrixAdd;
            if ((this.m_gizmo.SpaceDefault.m_removeBlock != null) && (this.m_gizmo.SpaceDefault.m_addPosSmallOnLarge == null))
            {
                MySlimBlock removeBlock = this.m_gizmo.SpaceDefault.m_removeBlock;
                worldMatrixAdd.Translation = Vector3D.Transform((Vector3) (this.m_gizmo.SpaceDefault.m_addPos * cubeSize), removeBlock.CubeGrid.PositionComp.WorldMatrix);
            }
            return new MyOrientedBoundingBoxD(new BoundingBoxD(Vector3D.Zero - vector, Vector3D.Zero + vector), worldMatrixAdd);
        }

        public static double? GetCurrentRayIntersection()
        {
            MyPhysics.HitInfo? nullable = MyPhysics.CastRay(MyBlockBuilderBase.IntersectionStart, MyBlockBuilderBase.IntersectionStart + (2000.0 * MyBlockBuilderBase.IntersectionDirection), 30);
            if (nullable != null)
            {
                return new double?((nullable.Value.Position - MyBlockBuilderBase.IntersectionStart).Length());
            }
            return null;
        }

        private Vector3D GetFreeSpacePlacementPosition(out bool valid)
        {
            valid = false;
            float cubeSize = MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize);
            MatrixD worldMatrixAdd = this.m_gizmo.SpaceDefault.m_worldMatrixAdd;
            Vector3D intersectionStart = MyBlockBuilderBase.IntersectionStart;
            Vector3D freePlacementTarget = base.FreePlacementTarget;
            worldMatrixAdd.Translation = intersectionStart;
            HkShape shape = (HkShape) new HkBoxShape((Vector3) ((this.CurrentBlockDefinition.Size * cubeSize) * 0.5f));
            double maxValue = double.MaxValue;
            try
            {
                float? nullable = MyPhysics.CastShape(freePlacementTarget, shape, ref worldMatrixAdd, 30, 0f);
                if ((nullable != null) && (nullable.Value != 0f))
                {
                    maxValue = ((intersectionStart + (((double) nullable.Value) * (freePlacementTarget - intersectionStart))) - MyBlockBuilderBase.IntersectionStart).Length() * 0.98;
                    valid = true;
                }
            }
            finally
            {
                shape.RemoveReference();
            }
            if (maxValue < this.LowLimitDistanceForDynamicMode())
            {
                maxValue = MyBlockBuilderBase.IntersectionDistance;
                valid = false;
            }
            if (maxValue > MyBlockBuilderBase.IntersectionDistance)
            {
                maxValue = MyBlockBuilderBase.IntersectionDistance;
                valid = false;
            }
            if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(MyBlockBuilderBase.IntersectionStart + (maxValue * MyBlockBuilderBase.IntersectionDirection)))
            {
                valid = false;
            }
            return (MyBlockBuilderBase.IntersectionStart + (maxValue * MyBlockBuilderBase.IntersectionDirection));
        }

        private Vector3I? GetSingleMountPointNormal()
        {
            if (this.CurrentBlockDefinition == null)
            {
                return null;
            }
            MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = this.CurrentBlockDefinition.GetBuildProgressModelMountPoints(1f);
            if ((buildProgressModelMountPoints == null) || (buildProgressModelMountPoints.Length == 0))
            {
                return null;
            }
            Vector3I normal = buildProgressModelMountPoints[0].Normal;
            if (this.AlignToDefault && !this.m_customRotation)
            {
                int index = 0;
                while (true)
                {
                    if (index >= buildProgressModelMountPoints.Length)
                    {
                        for (int i = 0; i < buildProgressModelMountPoints.Length; i++)
                        {
                            if (MyCubeBlockDefinition.NormalToBlockSide(buildProgressModelMountPoints[i].Normal) == BlockSideEnum.Bottom)
                            {
                                return new Vector3I?(buildProgressModelMountPoints[i].Normal);
                            }
                        }
                        break;
                    }
                    if (buildProgressModelMountPoints[index].Default)
                    {
                        return new Vector3I?(buildProgressModelMountPoints[index].Normal);
                    }
                    index++;
                }
            }
            Vector3I vectori2 = -normal;
            switch (this.CurrentBlockDefinition.AutorotateMode)
            {
                case MyAutorotateMode.OneDirection:
                    for (int i = 1; i < buildProgressModelMountPoints.Length; i++)
                    {
                        if (buildProgressModelMountPoints[i].Normal != normal)
                        {
                            return null;
                        }
                    }
                    break;

                case MyAutorotateMode.OppositeDirections:
                    for (int i = 1; i < buildProgressModelMountPoints.Length; i++)
                    {
                        Vector3I vectori3 = buildProgressModelMountPoints[i].Normal;
                        if ((vectori3 != normal) && (vectori3 != vectori2))
                        {
                            return null;
                        }
                    }
                    break;

                case MyAutorotateMode.FirstDirection:
                    break;

                default:
                    return null;
            }
            return new Vector3I?(normal);
        }

        private int GetStandardRotationAxisAndDirection(int index, ref int direction)
        {
            int num = -1;
            MatrixD matrix = MatrixD.Transpose(this.m_gizmo.SpaceDefault.m_localMatrixAdd);
            Vector3I vectori = Vector3I.Round(Vector3D.TransformNormal(Vector3D.Up, matrix));
            if (MyInput.Static.IsAnyShiftKeyPressed())
            {
                direction *= -1;
            }
            if (this.CubePlacementMode == CubePlacementModeEnum.FreePlacement)
            {
                return new int[] { 1, 1, 0, 0, 2, 2 }[index];
            }
            Vector3I? singleMountPointNormal = this.GetSingleMountPointNormal();
            if (singleMountPointNormal != null)
            {
                Vector3I vectori2 = singleMountPointNormal.Value;
                int num2 = Vector3I.Dot(ref vectori2, ref Vector3I.Up);
                int num3 = Vector3I.Dot(ref vectori2, ref Vector3I.Right);
                int num4 = Vector3I.Dot(ref vectori2, ref Vector3I.Forward);
                if ((num2 == 1) || (num2 == -1))
                {
                    num = 1;
                    direction *= num2;
                }
                else if ((num3 == 1) || (num3 == -1))
                {
                    num = 0;
                    direction *= num3;
                }
                else if ((num4 == 1) || (num4 == -1))
                {
                    num = 2;
                    direction *= num4;
                }
            }
            else if (index < 2)
            {
                int num5 = Vector3I.Dot(ref vectori, ref Vector3I.Up);
                int num6 = Vector3I.Dot(ref vectori, ref Vector3I.Right);
                int num7 = Vector3I.Dot(ref vectori, ref Vector3I.Forward);
                if ((num5 == 1) || (num5 == -1))
                {
                    num = 1;
                    direction *= num5;
                }
                else if ((num6 == 1) || (num6 == -1))
                {
                    num = 0;
                    direction *= num6;
                }
                else if ((num7 == 1) || (num7 == -1))
                {
                    num = 2;
                    direction *= num7;
                }
            }
            else if ((index < 2) || (index >= 4))
            {
                if (index >= 4)
                {
                    num = 2;
                }
            }
            else
            {
                Vector3I vectori4 = Vector3I.Round(this.m_gizmo.SpaceDefault.m_localMatrixAdd.Forward);
                if (Vector3I.Dot(ref vectori4, ref Vector3I.Up) != 0)
                {
                    num = 0;
                }
                else
                {
                    Vector3I vectori3;
                    Vector3I.Cross(ref vectori4, ref Vector3I.Up, out vectori3);
                    vectori3 = Vector3I.Round(Vector3D.TransformNormal((Vector3) vectori3, matrix));
                    int num8 = Vector3I.Dot(ref vectori3, ref Vector3I.Up);
                    int num9 = Vector3I.Dot(ref vectori3, ref Vector3I.Right);
                    int num10 = Vector3I.Dot(ref vectori3, ref Vector3I.Forward);
                    if ((num8 == 1) || (num8 == -1))
                    {
                        num = 1;
                        direction *= num8;
                    }
                    else if ((num9 == 1) || (num9 == -1))
                    {
                        num = 0;
                    }
                    else if ((num10 == 1) || (num10 == -1))
                    {
                        num = 2;
                        direction *= num10;
                    }
                }
            }
            return num;
        }

        private bool HandleAdminAndCreativeInput(MyStringId context)
        {
            int creativeMode;
            if (!MySession.Static.CreativeToolsEnabled(Sync.MyId) || !MySession.Static.HasCreativeRights)
            {
                creativeMode = (int) MySession.Static.CreativeMode;
            }
            else
            {
                creativeMode = 1;
            }
            bool flag = (bool) creativeMode;
            if (!flag)
            {
                return (MyBlockBuilderBase.SpectatorIsBuilding && false);
            }
            if (!(MySession.Static.ControlledEntity is MyShipController) && this.HandleBlockCreationMovement(context))
            {
                return true;
            }
            if (this.DynamicMode)
            {
                if (flag && MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.NEW_PRESSED, false))
                {
                    this.Add();
                }
            }
            else if (this.CurrentGrid != null)
            {
                this.HandleCurrentGridInput(context);
            }
            else if (MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.NEW_PRESSED, false))
            {
                this.Add();
            }
            return false;
        }

        private bool HandleBlockCreationMovement(MyStringId context)
        {
            bool flag = MyInput.Static.IsAnyCtrlKeyPressed();
            if ((!flag || (MyInput.Static.PreviousMouseScrollWheelValue() >= MyInput.Static.MouseScrollWheelValue())) && !MyControllerHelper.IsControl(context, MyControlsSpace.MOVE_FURTHER, MyControlStateType.PRESSED, false))
            {
                if ((!flag || (MyInput.Static.PreviousMouseScrollWheelValue() <= MyInput.Static.MouseScrollWheelValue())) && !MyControllerHelper.IsControl(context, MyControlsSpace.MOVE_CLOSER, MyControlStateType.PRESSED, false))
                {
                    return false;
                }
                MyBlockBuilderBase.IntersectionDistance /= 1.1f;
                if (MyBlockBuilderBase.IntersectionDistance < MyBlockBuilderBase.CubeBuilderDefinition.MinBlockBuildingDistance)
                {
                    MyBlockBuilderBase.IntersectionDistance = MyBlockBuilderBase.CubeBuilderDefinition.MinBlockBuildingDistance;
                }
                return true;
            }
            float intersectionDistance = MyBlockBuilderBase.IntersectionDistance;
            MyBlockBuilderBase.IntersectionDistance *= 1.1f;
            if (MyBlockBuilderBase.IntersectionDistance > MyBlockBuilderBase.CubeBuilderDefinition.MaxBlockBuildingDistance)
            {
                MyBlockBuilderBase.IntersectionDistance = MyBlockBuilderBase.CubeBuilderDefinition.MaxBlockBuildingDistance;
            }
            if ((MySession.Static.SurvivalMode && !MyBlockBuilderBase.SpectatorIsBuilding) && (this.CurrentBlockDefinition != null))
            {
                float cubeSize = MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize);
                BoundingBoxD gizmoBox = new BoundingBoxD((Vector3D) ((-this.CurrentBlockDefinition.Size * cubeSize) * 0.5f), (Vector3D) ((this.CurrentBlockDefinition.Size * cubeSize) * 0.5f));
                MatrixD worldMatrixAdd = this.m_gizmo.SpaceDefault.m_worldMatrixAdd;
                worldMatrixAdd.Translation = base.FreePlacementTarget;
                if (!MyCubeBuilderGizmo.DefaultGizmoCloseEnough(ref MatrixD.Invert(worldMatrixAdd), gizmoBox, cubeSize, MyBlockBuilderBase.IntersectionDistance))
                {
                    MyBlockBuilderBase.IntersectionDistance = intersectionDistance;
                }
            }
            return true;
        }

        private bool HandleBlockVariantsInput(MyStringId context)
        {
            int index;
            int num3;
            int num4;
            if (!MyFakes.ENABLE_BLOCK_STAGES)
            {
                goto TR_0000;
            }
            else if (this.CurrentBlockDefinition == null)
            {
                goto TR_0000;
            }
            else if (this.m_cubeBuilderState.CurrentBlockDefinitionStages.Count <= 0)
            {
                goto TR_0000;
            }
            else if (this.FreezeGizmo)
            {
                goto TR_0000;
            }
            else
            {
                bool? nullable = null;
                int num = MyInput.Static.MouseScrollWheelValue();
                if ((!MyInput.Static.IsGameControlPressed(MyControlsSpace.LOOKAROUND) && (!MyInput.Static.IsAnyCtrlKeyPressed() && !MyInput.Static.IsAnyShiftKeyPressed())) && (num != 0))
                {
                    if ((MyInput.Static.PreviousMouseScrollWheelValue() < MyInput.Static.MouseScrollWheelValue()) || MyControllerHelper.IsControl(context, MyControlsSpace.NEXT_BLOCK_STAGE, MyControlStateType.NEW_PRESSED, false))
                    {
                        nullable = true;
                    }
                    else if ((MyInput.Static.PreviousMouseScrollWheelValue() > MyInput.Static.MouseScrollWheelValue()) || MyControllerHelper.IsControl(context, MyControlsSpace.PREV_BLOCK_STAGE, MyControlStateType.NEW_PRESSED, false))
                    {
                        nullable = false;
                    }
                }
                if (nullable == null)
                {
                    goto TR_0000;
                }
                else
                {
                    index = this.m_cubeBuilderState.CurrentBlockDefinitionStages.IndexOf(this.CurrentBlockDefinition);
                    num4 = nullable.Value ? 1 : -1;
                    num3 = index;
                }
            }
            while (true)
            {
                if ((num3 += num4) != index)
                {
                    if (num3 >= this.m_cubeBuilderState.CurrentBlockDefinitionStages.Count)
                    {
                        num3 = 0;
                    }
                    else if (num3 < 0)
                    {
                        num3 = this.m_cubeBuilderState.CurrentBlockDefinitionStages.Count - 1;
                    }
                    if (MySession.Static.SurvivalMode && (!this.m_cubeBuilderState.CurrentBlockDefinitionStages[num3].AvailableInSurvival || (!MyFakes.ENABLE_MULTIBLOCK_CONSTRUCTION && (this.m_cubeBuilderState.CurrentBlockDefinitionStages[num3].MultiBlock != null))))
                    {
                        continue;
                    }
                }
                this.UpdateCubeBlockStageDefinition(this.m_cubeBuilderState.CurrentBlockDefinitionStages[num3]);
                break;
            }
        TR_0000:
            return false;
        }

        private bool HandleCurrentGridInput(MyStringId context)
        {
            MySymmetrySettingModeEnum symmetrySettingMode;
            if (MyControllerHelper.IsControl(context, MyControlsSpace.SYMMETRY_SWITCH, MyControlStateType.NEW_PRESSED, false) && !(MySession.Static.ControlledEntity is MyShipController))
            {
                if (this.BlockCreationIsActivated)
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                }
                symmetrySettingMode = this.m_symmetrySettingMode;
                if (symmetrySettingMode > MySymmetrySettingModeEnum.YPlane)
                {
                    if (symmetrySettingMode == MySymmetrySettingModeEnum.YPlaneOdd)
                    {
                        this.m_symmetrySettingMode = MySymmetrySettingModeEnum.ZPlane;
                        this.UpdateSymmetryNotification(MyCommonTexts.SettingSymmetryZ);
                    }
                    else if (symmetrySettingMode == MySymmetrySettingModeEnum.ZPlane)
                    {
                        this.m_symmetrySettingMode = MySymmetrySettingModeEnum.ZPlaneOdd;
                        this.UpdateSymmetryNotification(MyCommonTexts.SettingSymmetryZOffset);
                    }
                    else if (symmetrySettingMode == MySymmetrySettingModeEnum.ZPlaneOdd)
                    {
                        this.m_symmetrySettingMode = MySymmetrySettingModeEnum.NoPlane;
                        this.RemoveSymmetryNotification();
                    }
                }
                else
                {
                    switch (symmetrySettingMode)
                    {
                        case MySymmetrySettingModeEnum.NoPlane:
                            this.m_symmetrySettingMode = MySymmetrySettingModeEnum.XPlane;
                            this.UpdateSymmetryNotification(MyCommonTexts.SettingSymmetryX);
                            break;

                        case MySymmetrySettingModeEnum.XPlane:
                            this.m_symmetrySettingMode = MySymmetrySettingModeEnum.XPlaneOdd;
                            this.UpdateSymmetryNotification(MyCommonTexts.SettingSymmetryXOffset);
                            break;

                        case (MySymmetrySettingModeEnum.XPlane | MySymmetrySettingModeEnum.NoPlane):
                            break;

                        case MySymmetrySettingModeEnum.XPlaneOdd:
                            this.m_symmetrySettingMode = MySymmetrySettingModeEnum.YPlane;
                            this.UpdateSymmetryNotification(MyCommonTexts.SettingSymmetryY);
                            break;

                        default:
                            if (symmetrySettingMode == MySymmetrySettingModeEnum.YPlane)
                            {
                                this.m_symmetrySettingMode = MySymmetrySettingModeEnum.YPlaneOdd;
                                this.UpdateSymmetryNotification(MyCommonTexts.SettingSymmetryYOffset);
                            }
                            break;
                    }
                }
            }
            if (MyControllerHelper.IsControl(context, MyControlsSpace.USE_SYMMETRY, MyControlStateType.NEW_PRESSED, false) && !(MySession.Static.ControlledEntity is MyShipController))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                if (this.m_symmetrySettingMode != MySymmetrySettingModeEnum.NoPlane)
                {
                    this.UseSymmetry = false;
                    this.m_symmetrySettingMode = MySymmetrySettingModeEnum.NoPlane;
                    this.RemoveSymmetryNotification();
                    return true;
                }
                this.UseSymmetry = !this.UseSymmetry;
            }
            if ((this.CurrentBlockDefinition == null) || !this.BlockCreationIsActivated)
            {
                this.m_symmetrySettingMode = MySymmetrySettingModeEnum.NoPlane;
                this.RemoveSymmetryNotification();
            }
            if (!this.IsInSymmetrySettingMode)
            {
                goto TR_0042;
            }
            else if (MySession.Static.ControlledEntity is MyShipController)
            {
                goto TR_0042;
            }
            else
            {
                if (MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.NEW_PRESSED, false))
                {
                    if (this.m_gizmo.SpaceDefault.m_removeBlock != null)
                    {
                        Vector3I vectori = (Vector3I) ((this.m_gizmo.SpaceDefault.m_removeBlock.Min + this.m_gizmo.SpaceDefault.m_removeBlock.Max) / 2);
                        MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                        symmetrySettingMode = this.m_symmetrySettingMode;
                        if (symmetrySettingMode > MySymmetrySettingModeEnum.YPlane)
                        {
                            if (symmetrySettingMode == MySymmetrySettingModeEnum.YPlaneOdd)
                            {
                                this.CurrentGrid.YSymmetryPlane = new Vector3I?(vectori);
                                this.CurrentGrid.YSymmetryOdd = true;
                            }
                            else if (symmetrySettingMode == MySymmetrySettingModeEnum.ZPlane)
                            {
                                this.CurrentGrid.ZSymmetryPlane = new Vector3I?(vectori);
                                this.CurrentGrid.ZSymmetryOdd = false;
                            }
                            else if (symmetrySettingMode == MySymmetrySettingModeEnum.ZPlaneOdd)
                            {
                                this.CurrentGrid.ZSymmetryPlane = new Vector3I?(vectori);
                                this.CurrentGrid.ZSymmetryOdd = true;
                            }
                        }
                        else
                        {
                            switch (symmetrySettingMode)
                            {
                                case MySymmetrySettingModeEnum.NoPlane:
                                case (MySymmetrySettingModeEnum.XPlane | MySymmetrySettingModeEnum.NoPlane):
                                    break;

                                case MySymmetrySettingModeEnum.XPlane:
                                    this.CurrentGrid.XSymmetryPlane = new Vector3I?(vectori);
                                    this.CurrentGrid.XSymmetryOdd = false;
                                    break;

                                case MySymmetrySettingModeEnum.XPlaneOdd:
                                    this.CurrentGrid.XSymmetryPlane = new Vector3I?(vectori);
                                    this.CurrentGrid.XSymmetryOdd = true;
                                    break;

                                default:
                                    if (symmetrySettingMode == MySymmetrySettingModeEnum.YPlane)
                                    {
                                        this.CurrentGrid.YSymmetryPlane = new Vector3I?(vectori);
                                        this.CurrentGrid.YSymmetryOdd = false;
                                    }
                                    break;
                            }
                        }
                    }
                    return true;
                }
                if (!MyControllerHelper.IsControl(context, MyControlsSpace.SECONDARY_TOOL_ACTION, MyControlStateType.NEW_PRESSED, false))
                {
                    goto TR_0042;
                }
                else
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudDeleteBlock);
                    symmetrySettingMode = this.m_symmetrySettingMode;
                    if (symmetrySettingMode > MySymmetrySettingModeEnum.YPlane)
                    {
                        if (symmetrySettingMode != MySymmetrySettingModeEnum.YPlaneOdd)
                        {
                            if ((symmetrySettingMode == MySymmetrySettingModeEnum.ZPlane) || (symmetrySettingMode == MySymmetrySettingModeEnum.ZPlaneOdd))
                            {
                                this.CurrentGrid.ZSymmetryPlane = null;
                                this.CurrentGrid.ZSymmetryOdd = false;
                            }
                            goto TR_0010;
                        }
                    }
                    else
                    {
                        switch (symmetrySettingMode)
                        {
                            case MySymmetrySettingModeEnum.NoPlane:
                            case (MySymmetrySettingModeEnum.XPlane | MySymmetrySettingModeEnum.NoPlane):
                                goto TR_0010;

                            case MySymmetrySettingModeEnum.XPlane:
                            case MySymmetrySettingModeEnum.XPlaneOdd:
                                this.CurrentGrid.XSymmetryPlane = null;
                                this.CurrentGrid.XSymmetryOdd = false;
                                goto TR_0010;

                            default:
                                if (symmetrySettingMode == MySymmetrySettingModeEnum.YPlane)
                                {
                                    break;
                                }
                                goto TR_0010;
                        }
                    }
                    this.CurrentGrid.YSymmetryPlane = null;
                    this.CurrentGrid.YSymmetryOdd = false;
                }
            }
        TR_0010:
            return false;
        TR_0042:
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape))
            {
                if (this.m_symmetrySettingMode != MySymmetrySettingModeEnum.NoPlane)
                {
                    this.m_symmetrySettingMode = MySymmetrySettingModeEnum.NoPlane;
                    this.RemoveSymmetryNotification();
                    return true;
                }
                if (this.CancelBuilding())
                {
                    return true;
                }
            }
            if (MyInput.Static.IsNewLeftMousePressed() || MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_BUILD_ACTION, MyControlStateType.NEW_PRESSED, false))
            {
                if (this.PlacingSmallGridOnLargeStatic || (!MyInput.Static.IsAnyCtrlKeyPressed() && (BuildingMode == BuildingModeEnum.SingleBlock)))
                {
                    this.Add();
                }
                else
                {
                    this.StartBuilding();
                }
            }
            if (MyInput.Static.IsNewRightMousePressed() || MyControllerHelper.IsControl(context, MyControlsSpace.SECONDARY_BUILD_ACTION, MyControlStateType.NEW_PRESSED, false))
            {
                if (MyInput.Static.IsAnyCtrlKeyPressed() || (BuildingMode != BuildingModeEnum.SingleBlock))
                {
                    this.StartRemoving();
                }
                else
                {
                    if (MyFakes.ENABLE_COMPOUND_BLOCKS && !this.CompoundEnabled)
                    {
                        foreach (MyCubeBuilderGizmo.MyGizmoSpaceProperties properties in this.m_gizmo.Spaces)
                        {
                            if (properties.Enabled)
                            {
                                properties.m_blockIdInCompound = null;
                            }
                        }
                    }
                    this.PrepareBlocksToRemove();
                    this.Remove();
                }
            }
            if ((MyInput.Static.IsLeftMousePressed() || MyInput.Static.IsRightMousePressed()) || MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_BUILD_ACTION, MyControlStateType.PRESSED, false))
            {
                this.ContinueBuilding(MyInput.Static.IsAnyShiftKeyPressed() || (BuildingMode == BuildingModeEnum.Plane));
            }
            if ((MyInput.Static.IsNewLeftMouseReleased() || MyInput.Static.IsNewRightMouseReleased()) || MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_BUILD_ACTION, MyControlStateType.NEW_RELEASED, false))
            {
                this.StopBuilding();
            }
            if ((MyInput.Static.IsNewLeftMouseReleased() || MyInput.Static.IsNewRightMouseReleased()) || MyControllerHelper.IsControl(context, MyControlsSpace.PRIMARY_BUILD_ACTION, MyControlStateType.NEW_RELEASED, false))
            {
                this.StopBuilding();
            }
            return false;
        }

        private bool HandleExportInput()
        {
            if ((!MyInput.Static.IsNewKeyPressed(MyKeys.E) || (!MyInput.Static.IsAnyAltKeyPressed() || (!MyInput.Static.IsAnyCtrlKeyPressed() || (MyInput.Static.IsAnyShiftKeyPressed() || MyInput.Static.IsAnyMousePressed())))) || !MyPerGameSettings.EnableObjectExport)
            {
                return false;
            }
            MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
            MyCubeGrid targetGrid = MyCubeGrid.GetTargetGrid();
            if (targetGrid != null)
            {
                MyCubeGrid.ExportObject(targetGrid, false, true);
            }
            return true;
        }

        public virtual bool HandleGameInput()
        {
            MyStringId nullOrEmpty;
            if (this.HandleExportInput())
            {
                return true;
            }
            if (MyGuiScreenGamePlay.DisableInput)
            {
                return false;
            }
            if ((MyInput.Static.IsNewGameControlPressed(MyControlsSpace.LANDING_GEAR) && (ReferenceEquals(MySession.Static.ControlledEntity, MySession.Static.LocalCharacter) && ((MySession.Static.LocalHumanPlayer != null) && (ReferenceEquals(MySession.Static.LocalHumanPlayer.Identity.Character, MySession.Static.ControlledEntity) && !MyInput.Static.IsAnyShiftKeyPressed())))) && (MyGuiScreenGamePlay.ActiveGameplayScreen == null))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                MyGuiScreenColorPicker screen = new MyGuiScreenColorPicker();
                MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
                MyGuiSandbox.AddScreen(screen);
            }
            if (!this.IsActivated || !(MySession.Static.ControlledEntity is MyCharacter))
            {
                nullOrEmpty = MyStringId.NullOrEmpty;
            }
            else
            {
                nullOrEmpty = MySession.Static.ControlledEntity.ControlContext;
            }
            MyStringId context = nullOrEmpty;
            if (MyControllerHelper.IsControl(context, MyControlsSpace.CUBE_DEFAULT_MOUNTPOINT, MyControlStateType.NEW_PRESSED, false))
            {
                this.AlignToDefault = !this.AlignToDefault;
                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
            }
            if (!this.IsActivated)
            {
                return false;
            }
            int frameDt = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastInputHandleTime;
            this.m_lastInputHandleTime += frameDt;
            bool flag = (MySession.Static.ControlledEntity is MyCockpit) && !MyBlockBuilderBase.SpectatorIsBuilding;
            if ((flag && (MySession.Static.ControlledEntity is MyCockpit)) && (MySession.Static.ControlledEntity as MyCockpit).BuildingMode)
            {
                flag = false;
            }
            if (MySandboxGame.IsPaused | flag)
            {
                return false;
            }
            if ((MyInput.Static.IsNewLeftMousePressed() && ((MySession.Static.ControlledEntity is MyCockpit) && (MySession.Static.ControlledEntity as MyCockpit).BuildingMode)) && MySession.Static.SurvivalMode)
            {
                MySession.Static.LocalCharacter.BeginShoot(MyShootActionEnum.PrimaryAction);
            }
            if (this.IsActivated && MyControllerHelper.IsControl(context, MyControlsSpace.BUILD_MODE, MyControlStateType.NEW_PRESSED, false))
            {
                this.IsBuildMode = !this.IsBuildMode;
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.FREE_ROTATION))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                this.CycleCubePlacementMode();
            }
            if (this.HandleAdminAndCreativeInput(context))
            {
                return true;
            }
            if (((this.CurrentGrid != null) && MyInput.Static.IsNewGameControlPressed(MyControlsSpace.LANDING_GEAR)) && MyInput.Static.IsAnyShiftKeyPressed())
            {
                foreach (MyCubeBuilderGizmo.MyGizmoSpaceProperties properties in this.m_gizmo.Spaces)
                {
                    if ((properties.m_removeBlock != null) && (MySession.Static.LocalHumanPlayer != null))
                    {
                        MySession.Static.LocalHumanPlayer.ChangeOrSwitchToColor(properties.m_removeBlock.ColorMaskHSV);
                    }
                }
            }
            if ((this.CurrentGrid != null) && MyControllerHelper.IsControl(context, MyControlsSpace.CUBE_COLOR_CHANGE, MyControlStateType.PRESSED, false))
            {
                int expand = 0;
                if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsAnyShiftKeyPressed())
                {
                    expand = -1;
                }
                else if (MyInput.Static.IsAnyCtrlKeyPressed())
                {
                    expand = 1;
                }
                else if (MyInput.Static.IsAnyShiftKeyPressed())
                {
                    expand = 3;
                }
                this.Change(expand);
            }
            if (this.HandleRotationInput(context, frameDt))
            {
                return true;
            }
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            if ((MyInput.Static.IsNewGameControlPressed(MyControlsSpace.SWITCH_LEFT) && (this.IsActivated && ((this.CurrentBlockDefinition == null) || MyFakes.ENABLE_BLOCK_COLORING))) && (localHumanPlayer != null))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                localHumanPlayer.SelectedBuildColorSlot = ((localHumanPlayer.SelectedBuildColorSlot - 1) >= 0) ? (localHumanPlayer.SelectedBuildColorSlot - 1) : (localHumanPlayer.BuildColorSlots.Count - 1);
            }
            if ((MyInput.Static.IsNewGameControlPressed(MyControlsSpace.SWITCH_RIGHT) && (this.IsActivated && ((this.CurrentBlockDefinition == null) || MyFakes.ENABLE_BLOCK_COLORING))) && (localHumanPlayer != null))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                localHumanPlayer.SelectedBuildColorSlot = ((localHumanPlayer.SelectedBuildColorSlot + 1) < localHumanPlayer.BuildColorSlots.Count) ? (localHumanPlayer.SelectedBuildColorSlot + 1) : 0;
            }
            return this.HandleBlockVariantsInput(context);
        }

        private bool HandleRotationInput(MyStringId context, int frameDt)
        {
            if (this.IsActivated)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (MyControllerHelper.IsControl(context, MyBlockBuilderBase.m_rotationControls[i], MyControlStateType.PRESSED, false))
                    {
                        if (this.AlignToDefault)
                        {
                            this.m_customRotation = true;
                        }
                        bool newlyPressed = MyControllerHelper.IsControl(context, MyBlockBuilderBase.m_rotationControls[i], MyControlStateType.NEW_PRESSED, false);
                        int index = -1;
                        int direction = MyBlockBuilderBase.m_rotationDirections[i];
                        if (MyFakes.ENABLE_STANDARD_AXES_ROTATION)
                        {
                            index = this.GetStandardRotationAxisAndDirection(i, ref direction);
                        }
                        else
                        {
                            if (i < 2)
                            {
                                index = this.m_rotationHints.RotationUpAxis;
                                direction *= this.m_rotationHints.RotationUpDirection;
                            }
                            if ((i >= 2) && (i < 4))
                            {
                                index = this.m_rotationHints.RotationRightAxis;
                                direction *= this.m_rotationHints.RotationRightDirection;
                            }
                            if (i >= 4)
                            {
                                index = this.m_rotationHints.RotationForwardAxis;
                                direction *= this.m_rotationHints.RotationForwardDirection;
                            }
                        }
                        if (index != -1)
                        {
                            if ((this.CurrentBlockDefinition != null) && (this.CurrentBlockDefinition.Rotation == MyBlockRotation.None))
                            {
                                return false;
                            }
                            double angleDelta = frameDt * BLOCK_ROTATION_SPEED;
                            if (MyInput.Static.IsAnyCtrlKeyPressed() || (this.m_cubePlacementMode == CubePlacementModeEnum.GravityAligned))
                            {
                                if (!newlyPressed)
                                {
                                    return false;
                                }
                                angleDelta = 1.5707963267948966;
                            }
                            if (MyInput.Static.IsAnyAltKeyPressed())
                            {
                                if (!newlyPressed)
                                {
                                    return false;
                                }
                                angleDelta = MathHelperD.ToRadians(1.0);
                            }
                            this.RotateAxis(index, direction, angleDelta, newlyPressed);
                        }
                    }
                }
            }
            return false;
        }

        private void HideNotificationBlockNotAvailable()
        {
            if ((this.m_blockNotAvailableNotification != null) && this.m_blockNotAvailableNotification.Alive)
            {
                MyHud.Notifications.Remove(this.m_blockNotAvailableNotification);
            }
        }

        public override void InitFromDefinition(MySessionComponentDefinition definition)
        {
            base.InitFromDefinition(definition);
        }

        private void InitializeNotifications()
        {
            this.m_cubePlacementModeNotification = new MyHudNotification(MyCommonTexts.NotificationCubePlacementModeChanged, 0x9c4, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            this.UpdatePlacementNotificationState();
            this.m_cubePlacementModeHint = new MyHudNotification(MyCommonTexts.ControlHintCubePlacementMode, MyHudNotificationBase.INFINITE, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            this.m_cubePlacementModeHint.Level = MyNotificationLevel.Control;
            this.m_cubePlacementUnable = new MyHudNotification(MyCommonTexts.NotificationCubePlacementUnable, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
        }

        public void InputLost()
        {
            this.m_gizmo.Clear();
        }

        private bool IntersectsCharacterOrCamera(MyCubeBuilderGizmo.MyGizmoSpaceProperties gizmoSpace, float gridSize, ref MatrixD inverseBlockInGridWorldMatrix)
        {
            if (this.CurrentBlockDefinition == null)
            {
                return false;
            }
            bool flag = false;
            if (MySector.MainCamera != null)
            {
                flag = this.m_gizmo.PointInsideGizmo(MySector.MainCamera.Position, gizmoSpace.SourceSpace, ref inverseBlockInGridWorldMatrix, gridSize, 0.05f, this.CurrentVoxelBase != null, this.DynamicMode);
            }
            if (flag)
            {
                return true;
            }
            if ((MySession.Static.ControlledEntity != null) && (MySession.Static.ControlledEntity is MyCharacter))
            {
                this.m_collisionTestPoints.Clear();
                PrepareCharacterCollisionPoints(this.m_collisionTestPoints);
                flag = this.m_gizmo.PointsAABBIntersectsGizmo(this.m_collisionTestPoints, gizmoSpace.SourceSpace, ref inverseBlockInGridWorldMatrix, gridSize, 0.05f, this.CurrentVoxelBase != null, this.DynamicMode);
            }
            return flag;
        }

        protected virtual bool IsBuilding() => 
            ((this.m_gizmo.SpaceDefault.m_startBuild != null) || (this.m_gizmo.SpaceDefault.m_startRemove != null));

        protected virtual bool IsDynamicOverride() => 
            ((this.m_cubeBuilderState != null) && ((this.m_cubeBuilderState.CurrentBlockDefinition != null) && ((this.CurrentGrid != null) && (this.m_cubeBuilderState.CurrentBlockDefinition.CubeSize != this.CurrentGrid.GridSizeEnum))));

        public override void LoadData()
        {
            base.LoadData();
            this.m_cubeBuilderState = new MyCubeBuilderState();
            Static = this;
            MyCubeGrid.ShowStructuralIntegrity = false;
            this.m_useSymmetry = MySandboxGame.Config.CubeBuilderUseSymmetry;
            this.m_alignToDefault = MySandboxGame.Config.CubeBuilderAlignToDefault;
        }

        private float LowLimitDistanceForDynamicMode() => 
            ((this.CurrentBlockDefinition == null) ? 2.6f : (MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize) + 0.1f));

        private Vector3I MakeCubePosition(Vector3D position)
        {
            Vector3I vectori;
            position -= this.CurrentGrid.WorldMatrix.Translation;
            Vector3D vectord = new Vector3D((double) this.CurrentGrid.GridSize);
            Vector3D vectord2 = position / vectord;
            vectori.X = (int) Math.Round(vectord2.X);
            vectori.Y = (int) Math.Round(vectord2.Y);
            vectori.Z = (int) Math.Round(vectord2.Z);
            return vectori;
        }

        public void NotifyPlacementUnable()
        {
            if (this.CurrentBlockDefinition != null)
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                object[] arguments = new object[] { this.CurrentBlockDefinition.DisplayNameText };
                this.m_cubePlacementUnable.SetTextFormatArguments(arguments);
                MyHud.Notifications.Add(this.m_cubePlacementUnable);
            }
        }

        public void OnClosedMessageBox(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (((this.m_removalTemporalData != null) && ((this.m_removalTemporalData.Block != null) && ((this.CurrentGrid != null) && (((this.m_removalTemporalData.Block.FatBlock == null) || !this.m_removalTemporalData.Block.FatBlock.IsSubBlock) && (this.m_removalTemporalData.Block.CubeGrid != null))))) && !this.m_removalTemporalData.Block.CubeGrid.Closed)
            {
                MyCockpit fatBlock = this.m_removalTemporalData.Block.FatBlock as MyCockpit;
                if (((result == MyGuiScreenMessageBox.ResultEnum.NO) && ((fatBlock != null) && !fatBlock.Closed)) && (fatBlock.Pilot != null))
                {
                    fatBlock.RequestRemovePilot();
                }
                this.RemoveBlockInternal(this.m_removalTemporalData.Block, this.m_removalTemporalData.BlockIdInCompound, this.m_removalTemporalData.CheckExisting);
                this.Remove();
            }
            this.m_removalTemporalData = null;
        }

        public void OnLostFocus()
        {
            this.Deactivate();
        }

        protected virtual void PrepareBlocksToRemove()
        {
            MyCubeBuilderGizmo.MyGizmoSpaceProperties properties;
            this.m_tmpBlockPositionList.Clear();
            this.m_tmpCompoundBlockPositionIdList.Clear();
            MyCubeBuilderGizmo.MyGizmoSpaceProperties[] spaces = this.m_gizmo.Spaces;
            int index = 0;
            goto TR_0013;
        TR_0001:
            index++;
        TR_0013:
            while (true)
            {
                if (index >= spaces.Length)
                {
                    return;
                }
                properties = spaces[index];
                if (!properties.Enabled)
                {
                    goto TR_0001;
                }
                else if (!this.GridAndBlockValid)
                {
                    goto TR_0001;
                }
                else if (properties.m_removeBlock == null)
                {
                    goto TR_0001;
                }
                else
                {
                    if ((properties.m_removeBlock.FatBlock != null) && properties.m_removeBlock.FatBlock.IsSubBlock)
                    {
                        goto TR_0001;
                    }
                    if (!ReferenceEquals(this.CurrentGrid, properties.m_removeBlock.CubeGrid))
                    {
                        goto TR_0001;
                    }
                    else
                    {
                        if (properties.m_removeBlocksInMultiBlock.Count > 0)
                        {
                            foreach (Tuple<MySlimBlock, ushort?> tuple in properties.m_removeBlocksInMultiBlock)
                            {
                                this.RemoveBlock(tuple.Item1, tuple.Item2, false);
                            }
                            break;
                        }
                        this.RemoveBlock(properties.m_removeBlock, properties.m_blockIdInCompound, true);
                    }
                }
                break;
            }
            properties.m_removeBlock = null;
            properties.m_removeBlocksInMultiBlock.Clear();
            goto TR_0001;
        }

        public static void PrepareCharacterCollisionPoints(List<Vector3D> outList)
        {
            MyCharacter controlledEntity = MySession.Static.ControlledEntity as MyCharacter;
            if (controlledEntity != null)
            {
                float characterCollisionCrouchHeight = controlledEntity.Definition.CharacterCollisionHeight * 0.7f;
                float num2 = controlledEntity.Definition.CharacterCollisionWidth * 0.2f;
                if (controlledEntity != null)
                {
                    if (controlledEntity.IsCrouching)
                    {
                        characterCollisionCrouchHeight = controlledEntity.Definition.CharacterCollisionCrouchHeight;
                    }
                    Vector3 vector = controlledEntity.PositionComp.LocalMatrix.Up * characterCollisionCrouchHeight;
                    Vector3 vector2 = controlledEntity.PositionComp.LocalMatrix.Forward * num2;
                    Vector3 vector3 = controlledEntity.PositionComp.LocalMatrix.Right * num2;
                    Vector3D vectord = controlledEntity.Entity.PositionComp.GetPosition() + (controlledEntity.PositionComp.LocalMatrix.Up * 0.2f);
                    float num3 = 0f;
                    for (int i = 0; i < 6; i++)
                    {
                        float num5 = (float) Math.Sin((double) num3);
                        float num6 = (float) Math.Cos((double) num3);
                        Vector3D item = (vectord + (num5 * vector3)) + (num6 * vector2);
                        outList.Add(item);
                        outList.Add(item + vector);
                        num3 += 1.047198f;
                    }
                }
            }
        }

        protected void Remove()
        {
            if ((this.m_tmpBlockPositionList.Count > 0) || (this.m_tmpCompoundBlockPositionIdList.Count > 0))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudDeleteBlock);
                if (this.m_tmpBlockPositionList.Count > 0)
                {
                    this.CurrentGrid.RazeBlocks(this.m_tmpBlockPositionList, 0L);
                    this.m_tmpBlockPositionList.Clear();
                }
                if (this.m_tmpCompoundBlockPositionIdList.Count > 0)
                {
                    this.CurrentGrid.RazeBlockInCompoundBlock(this.m_tmpCompoundBlockPositionIdList);
                }
            }
        }

        protected void RemoveBlock(MySlimBlock block, ushort? blockIdInCompound, bool checkExisting = false)
        {
            if ((block != null) && ((block.FatBlock == null) || !block.FatBlock.IsSubBlock))
            {
                MyCockpit fatBlock = block.FatBlock as MyCockpit;
                if ((fatBlock != null) && (fatBlock.Pilot != null))
                {
                    this.m_removalTemporalData = new MyBlockRemovalData(block, blockIdInCompound, checkExisting);
                    if (MySession.Static.CreativeMode || !MySession.Static.IsUserAdmin(Sync.MyId))
                    {
                        this.OnClosedMessageBox(MyGuiScreenMessageBox.ResultEnum.NO);
                    }
                    else
                    {
                        MyStringId? okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        Vector2? size = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.RemovePilotToo), null, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnClosedMessageBox), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                    }
                }
                else
                {
                    this.RemoveBlockInternal(block, blockIdInCompound, checkExisting);
                }
            }
        }

        protected void RemoveBlockInternal(MySlimBlock block, ushort? blockIdInCompound, bool checkExisting = false)
        {
            if (block.FatBlock is MyCompoundCubeBlock)
            {
                if (blockIdInCompound != null)
                {
                    if (!checkExisting || !this.m_tmpCompoundBlockPositionIdList.Exists(t => (t.Item1 == block.Min) && (t.Item2 == blockIdInCompound.Value)))
                    {
                        this.m_tmpCompoundBlockPositionIdList.Add(new Tuple<Vector3I, ushort>(block.Min, blockIdInCompound.Value));
                    }
                }
                else if (!checkExisting || !this.m_tmpBlockPositionList.Contains(block.Min))
                {
                    this.m_tmpBlockPositionList.Add(block.Min);
                }
            }
            else if (!checkExisting || !this.m_tmpBlockPositionList.Contains(block.Min))
            {
                this.m_tmpBlockPositionList.Add(block.Min);
            }
        }

        private void RemoveSymmetryNotification()
        {
            if (this.m_symmetryNotification != null)
            {
                MyHud.Notifications.Remove(this.m_symmetryNotification);
                this.m_symmetryNotification = null;
            }
        }

        [Event(null, 0x124d), Reliable, Server]
        private static void RequestGridSpawn(Author author, DefinitionIdBlit definition, BuildData position, bool instantBuild, bool forceStatic, uint colorMaskHsv)
        {
            int num1;
            VRage.Game.Entity.MyEntity builder = null;
            if (MyEventContext.Current.IsLocallyInvoked || MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value))
            {
                num1 = 1;
            }
            else
            {
                num1 = (int) MySession.Static.CreativeToolsEnabled(Sync.MyId);
            }
            bool flag = (bool) num1;
            if ((!MySession.Static.GetComponent<MySessionComponentDLC>().HasDefinitionDLC((MyDefinitionId) definition, MyEventContext.Current.Sender.Value) || ((MySession.Static.ResearchEnabled && !flag) && !MySessionComponentResearch.Static.CanUse(author.IdentityId, (MyDefinitionId) definition))) || (instantBuild && !flag))
            {
                MyMultiplayerServerBase @static = MyMultiplayer.Static as MyMultiplayerServerBase;
                if (@static == null)
                {
                    MyMultiplayerServerBase local1 = @static;
                }
                else
                {
                    @static.ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                }
            }
            else
            {
                Sandbox.Game.Entities.MyEntities.TryGetEntityById(author.EntityId, out builder, false);
                MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition((MyDefinitionId) definition);
                Vector3D vectord = position.Position;
                if (!position.AbsolutePosition)
                {
                    vectord += builder.PositionComp.GetPosition();
                }
                MatrixD m = MatrixD.CreateWorld(vectord, position.Forward, position.Up);
                if (Sandbox.Game.Entities.MyEntities.IsInsideWorld(m.Translation))
                {
                    float cubeSize = MyDefinitionManager.Static.GetCubeSize(cubeBlockDefinition.CubeSize);
                    BoundingBoxD localAabb = new BoundingBoxD((Vector3D) ((-cubeBlockDefinition.Size * cubeSize) * 0.5f), (Vector3D) ((cubeBlockDefinition.Size * cubeSize) * 0.5f));
                    if (MySessionComponentSafeZones.IsActionAllowed(localAabb.TransformFast(ref m), MySafeZoneAction.Building, builder.EntityId))
                    {
                        int stationPlacement;
                        MyGridPlacementSettings gridPlacementSettings = MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.GetGridPlacementSettings(cubeBlockDefinition.CubeSize);
                        VoxelPlacementSettings settings3 = new VoxelPlacementSettings {
                            PlacementMode = VoxelPlacementMode.OutsideVoxel
                        };
                        gridPlacementSettings.VoxelPlacement = new VoxelPlacementSettings?(settings3);
                        if (forceStatic || MyCubeGrid.IsAabbInsideVoxel(m, localAabb, gridPlacementSettings))
                        {
                            stationPlacement = 1;
                        }
                        else
                        {
                            stationPlacement = (int) Static.m_stationPlacement;
                        }
                        bool isStatic = (bool) stationPlacement;
                        BuildComponent.GetGridSpawnMaterials(cubeBlockDefinition, m, isStatic);
                        bool canSpawn = (flag & instantBuild) || BuildComponent.HasBuildingMaterials(builder, false);
                        ulong senderId = MyEventContext.Current.Sender.Value;
                        if (!canSpawn)
                        {
                            SpawnGridReply(canSpawn, senderId);
                        }
                        else
                        {
                            SpawnFlags spawnFlags = SpawnFlags.AddToScene | SpawnFlags.CreatePhysics | SpawnFlags.EnableSmallTolargeConnections;
                            if (flag & instantBuild)
                            {
                                spawnFlags |= SpawnFlags.None | SpawnFlags.SpawnAsMaster;
                            }
                            Vector3 color = ColorExtensions.UnpackHSVFromUint(colorMaskHsv);
                            if (isStatic)
                            {
                                SpawnStaticGrid(cubeBlockDefinition, builder, m, color, spawnFlags, author.IdentityId, grid => AfterGridBuild(builder, grid as MyCubeGrid, instantBuild, senderId));
                            }
                            else
                            {
                                SpawnDynamicGrid(cubeBlockDefinition, builder, m, color, 0L, spawnFlags, author.IdentityId, grid => AfterGridBuild(builder, grid as MyCubeGrid, instantBuild, senderId));
                            }
                        }
                    }
                }
            }
        }

        protected virtual unsafe void RotateAxis(int index, int sign, double angleDelta, bool newlyPressed)
        {
            if (this.DynamicMode)
            {
                MatrixD xd2;
                MatrixD worldMatrixAdd = this.m_gizmo.SpaceDefault.m_worldMatrixAdd;
                if (CalculateBlockRotation(index, sign, ref worldMatrixAdd, out xd2, angleDelta, MyBlockDirection.Both, MyBlockRotation.Both))
                {
                    this.m_gizmo.SpaceDefault.m_worldMatrixAdd = xd2;
                }
            }
            else if (newlyPressed)
            {
                MatrixD* xdPtr1;
                angleDelta = 1.5707963267948966;
                MatrixD localMatrixAdd = this.m_gizmo.SpaceDefault.m_localMatrixAdd;
                if (CalculateBlockRotation(index, sign, ref localMatrixAdd, out (MatrixD) ref xdPtr1, angleDelta, (this.CurrentBlockDefinition != null) ? this.CurrentBlockDefinition.Direction : MyBlockDirection.Both, (this.CurrentBlockDefinition != null) ? this.CurrentBlockDefinition.Rotation : MyBlockRotation.Both))
                {
                    MatrixD xd4;
                    MyGuiAudio.PlaySound(MyGuiSounds.HudRotateBlock);
                    xdPtr1 = (MatrixD*) ref xd4;
                    this.m_gizmo.RotateAxis(ref xd4);
                }
            }
        }

        public static void SelectBlockToToolbar(MySlimBlock block, bool selectToNextSlot = true)
        {
            MyDefinitionId id = block.BlockDefinition.Id;
            if (block.FatBlock is MyCompoundCubeBlock)
            {
                MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
                m_cycle = m_cycle % fatBlock.GetBlocksCount();
                id = fatBlock.GetBlocks()[m_cycle].BlockDefinition.Id;
                m_cycle++;
            }
            if (block.FatBlock is MyFracturedBlock)
            {
                MyFracturedBlock fatBlock = block.FatBlock as MyFracturedBlock;
                m_cycle = m_cycle % fatBlock.OriginalBlocks.Count;
                id = fatBlock.OriginalBlocks[m_cycle];
                m_cycle++;
            }
            if (MyToolbarComponent.CurrentToolbar.SelectedSlot != null)
            {
                int slot = MyToolbarComponent.CurrentToolbar.SelectedSlot.Value;
                if (selectToNextSlot)
                {
                    slot++;
                }
                if (!MyToolbarComponent.CurrentToolbar.IsValidSlot(slot))
                {
                    slot = 0;
                }
                MyObjectBuilder_ToolbarItemCubeBlock data = new MyObjectBuilder_ToolbarItemCubeBlock();
                data.DefinitionId = (SerializableDefinitionId) id;
                MyToolbarComponent.CurrentToolbar.SetItemAtSlot(slot, MyToolbarItemFactory.CreateToolbarItem(data));
            }
            else
            {
                int slot = 0;
                while (MyToolbarComponent.CurrentToolbar.GetSlotItem(slot) != null)
                {
                    slot++;
                }
                if (!MyToolbarComponent.CurrentToolbar.IsValidSlot(slot))
                {
                    slot = 0;
                }
                MyObjectBuilder_ToolbarItemCubeBlock data = new MyObjectBuilder_ToolbarItemCubeBlock();
                data.DefinitionId = (SerializableDefinitionId) id;
                MyToolbarComponent.CurrentToolbar.SetItemAtSlot(slot, MyToolbarItemFactory.CreateToolbarItem(data));
            }
        }

        private void SetSurvivalIntersectionDist()
        {
            if (((this.CurrentBlockDefinition != null) && (MySession.Static.SurvivalMode && !MyBlockBuilderBase.SpectatorIsBuilding)) && !MySession.Static.CreativeToolsEnabled(Sync.MyId))
            {
                if (this.CurrentBlockDefinition.CubeSize == MyCubeSize.Large)
                {
                    MyBlockBuilderBase.IntersectionDistance = (float) MyBlockBuilderBase.CubeBuilderDefinition.BuildingDistLargeSurvivalCharacter;
                }
                else
                {
                    MyBlockBuilderBase.IntersectionDistance = (float) MyBlockBuilderBase.CubeBuilderDefinition.BuildingDistSmallSurvivalCharacter;
                }
            }
        }

        private void ShowCubePlacementNotification()
        {
            string str = "[" + MyInput.Static.GetGameControl(MyControlsSpace.FREE_ROTATION) + "]";
            switch (this.CubePlacementMode)
            {
                case CubePlacementModeEnum.LocalCoordinateSystem:
                {
                    object[] arguments = new object[] { MyTexts.GetString(MyCommonTexts.NotificationCubePlacementMode_LocalCoordSystem) };
                    this.m_cubePlacementModeNotification.SetTextFormatArguments(arguments);
                    object[] objArray2 = new object[] { str, MyTexts.GetString(MyCommonTexts.ControlHintCubePlacementMode_LocalCoordSystem) };
                    this.m_cubePlacementModeHint.SetTextFormatArguments(objArray2);
                    break;
                }
                case CubePlacementModeEnum.FreePlacement:
                {
                    object[] arguments = new object[] { MyTexts.GetString(MyCommonTexts.NotificationCubePlacementMode_FreePlacement) };
                    this.m_cubePlacementModeNotification.SetTextFormatArguments(arguments);
                    object[] objArray4 = new object[] { str, MyTexts.GetString(MyCommonTexts.ControlHintCubePlacementMode_FreePlacement) };
                    this.m_cubePlacementModeHint.SetTextFormatArguments(objArray4);
                    break;
                }
                case CubePlacementModeEnum.GravityAligned:
                {
                    object[] arguments = new object[] { MyTexts.GetString(MyCommonTexts.NotificationCubePlacementMode_GravityAligned) };
                    this.m_cubePlacementModeNotification.SetTextFormatArguments(arguments);
                    object[] objArray6 = new object[] { str, MyTexts.GetString(MyCommonTexts.ControlHintCubePlacementMode_GravityAligned) };
                    this.m_cubePlacementModeHint.SetTextFormatArguments(objArray6);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            this.UpdatePlacementNotificationState();
            MyHud.Notifications.Add(this.m_cubePlacementModeNotification);
            MyHud.Notifications.Remove(this.m_cubePlacementModeHint);
            MyHud.Notifications.Add(this.m_cubePlacementModeHint);
        }

        private void ShowNotificationBlockNotAvailable(MyStringId grid1Text, string blockDisplayName, MyStringId grid2Text)
        {
            if (MyFakes.ENABLE_NOTIFICATION_BLOCK_NOT_AVAILABLE)
            {
                if (this.m_blockNotAvailableNotification == null)
                {
                    this.m_blockNotAvailableNotification = new MyHudNotification(MySpaceTexts.NotificationBlockNotAvailableFor, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 1, MyNotificationLevel.Normal);
                }
                object[] arguments = new object[] { MyTexts.Get(grid1Text).ToLower().FirstLetterUpperCase(), blockDisplayName.ToLower(), MyTexts.Get(grid2Text).ToLower() };
                this.m_blockNotAvailableNotification.SetTextFormatArguments(arguments);
                MyHud.Notifications.Add(this.m_blockNotAvailableNotification);
            }
        }

        public static MyCubeGrid SpawnDynamicGrid(MyCubeBlockDefinition blockDefinition, VRage.Game.Entity.MyEntity builder, MatrixD worldMatrix, Vector3 color, long entityId = 0L, SpawnFlags spawnFlags = 7, long builtBy = 0L, Action<VRage.Game.Entity.MyEntity> completionCallback = null)
        {
            MyObjectBuilder_CubeGrid objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_CubeGrid>();
            Vector3 vector = Vector3.TransformNormal(MyCubeBlock.GetBlockGridOffset(blockDefinition), worldMatrix);
            Vector3D? relativeOffset = new Vector3D?((worldMatrix.Translation - vector) - builder.WorldMatrix.Translation);
            objectBuilder.PositionAndOrientation = new MyPositionAndOrientation(worldMatrix.Translation - vector, (Vector3) worldMatrix.Forward, (Vector3) worldMatrix.Up);
            objectBuilder.GridSizeEnum = blockDefinition.CubeSize;
            objectBuilder.IsStatic = false;
            objectBuilder.PersistentFlags |= MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
            MyObjectBuilder_CubeBlock ob = MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) blockDefinition.Id) as MyObjectBuilder_CubeBlock;
            ob.Orientation = Quaternion.CreateFromForwardUp((Vector3) Vector3I.Forward, (Vector3) Vector3I.Up);
            ob.Min = (SerializableVector3I) (((blockDefinition.Size / 2) - blockDefinition.Size) + Vector3I.One);
            ob.ColorMaskHSV = color;
            ob.BuiltBy = builtBy;
            ob.Owner = builtBy;
            BuildComponent.BeforeCreateBlock(blockDefinition, builder, ob, (spawnFlags & (SpawnFlags.None | SpawnFlags.SpawnAsMaster)) != SpawnFlags.None);
            objectBuilder.CubeBlocks.Add(ob);
            MyCubeGrid grid2 = null;
            if (builder != null)
            {
                VRage.Game.Entity.MyEntity entity = (builder.Parent == null) ? builder : ((builder.Parent is MyCubeBlock) ? ((MyCubeBlock) builder.Parent).CubeGrid : builder.Parent);
                if ((entity.Physics != null) && (entity.Physics.LinearVelocity.LengthSquared() >= 225f))
                {
                    objectBuilder.LinearVelocity = entity.Physics.LinearVelocity;
                }
            }
            if (entityId != 0)
            {
                objectBuilder.EntityId = entityId;
                ob.EntityId = entityId + 1L;
                Vector3D? nullable2 = null;
                grid2 = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilderParallel(objectBuilder, true, completionCallback, null, null, nullable2, true, false) as MyCubeGrid;
            }
            else if (Sync.IsServer)
            {
                objectBuilder.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                ob.EntityId = objectBuilder.EntityId + 1L;
                grid2 = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilderParallel(objectBuilder, true, completionCallback, null, builder, relativeOffset, true, false) as MyCubeGrid;
            }
            return grid2;
        }

        [Event(null, 0x12a5), Reliable, Client]
        private static void SpawnGridReply(bool success)
        {
            if (success)
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudPlaceBlock);
            }
            else
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
            }
        }

        private static void SpawnGridReply(bool canSpawn, ulong senderId)
        {
            if (senderId == 0)
            {
                SpawnGridReply(canSpawn);
            }
            else
            {
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<bool>(s => new Action<bool>(MyCubeBuilder.SpawnGridReply), canSpawn, new EndpointId(senderId), position);
            }
        }

        public static MyCubeGrid SpawnStaticGrid(MyCubeBlockDefinition blockDefinition, VRage.Game.Entity.MyEntity builder, MatrixD worldMatrix, Vector3 color, SpawnFlags spawnFlags = 7, long builtBy = 0L, Action<VRage.Game.Entity.MyEntity> completionCallback = null)
        {
            MyCubeGrid grid2;
            Vector3D? nullable;
            MyObjectBuilder_CubeGrid objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_CubeGrid>();
            Vector3 vector = Vector3.TransformNormal(MyCubeBlock.GetBlockGridOffset(blockDefinition), worldMatrix);
            objectBuilder.PositionAndOrientation = new MyPositionAndOrientation(worldMatrix.Translation - vector, (Vector3) worldMatrix.Forward, (Vector3) worldMatrix.Up);
            objectBuilder.GridSizeEnum = blockDefinition.CubeSize;
            objectBuilder.IsStatic = true;
            objectBuilder.CreatePhysics = (spawnFlags & SpawnFlags.CreatePhysics) != SpawnFlags.None;
            objectBuilder.EnableSmallToLargeConnections = (spawnFlags & SpawnFlags.EnableSmallTolargeConnections) != SpawnFlags.None;
            objectBuilder.PersistentFlags |= MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
            if ((spawnFlags & SpawnFlags.AddToScene) != SpawnFlags.None)
            {
                objectBuilder.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            }
            MyObjectBuilder_CubeBlock ob = MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) blockDefinition.Id) as MyObjectBuilder_CubeBlock;
            ob.Orientation = Quaternion.CreateFromForwardUp((Vector3) Vector3I.Forward, (Vector3) Vector3I.Up);
            ob.Min = (SerializableVector3I) (((blockDefinition.Size / 2) - blockDefinition.Size) + Vector3I.One);
            if ((spawnFlags & SpawnFlags.AddToScene) != SpawnFlags.None)
            {
                ob.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            }
            ob.ColorMaskHSV = color;
            ob.BuiltBy = builtBy;
            ob.Owner = builtBy;
            BuildComponent.BeforeCreateBlock(blockDefinition, builder, ob, (spawnFlags & (SpawnFlags.None | SpawnFlags.SpawnAsMaster)) != SpawnFlags.None);
            objectBuilder.CubeBlocks.Add(ob);
            if ((spawnFlags & SpawnFlags.AddToScene) != SpawnFlags.None)
            {
                nullable = null;
                grid2 = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilderParallel(objectBuilder, true, completionCallback, null, null, nullable, true, false) as MyCubeGrid;
            }
            else
            {
                nullable = null;
                grid2 = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilderParallel(objectBuilder, false, completionCallback, null, null, nullable, true, false) as MyCubeGrid;
            }
            return grid2;
        }

        public virtual void StartBuilding()
        {
            this.StartBuilding(ref this.m_gizmo.SpaceDefault.m_startBuild, this.m_gizmo.SpaceDefault.m_startRemove);
        }

        protected void StartBuilding(ref Vector3I? startBuild, Vector3I? startRemove)
        {
            if (((this.GridAndBlockValid || this.VoxelMapAndBlockValid) && !this.PlacingSmallGridOnLargeStatic) && !Sandbox.Game.Entities.MyEntities.MemoryLimitReachedReport)
            {
                Vector3I vectori;
                Vector3? nullable;
                Vector3I vectori2;
                Vector3I vectori3;
                MySlimBlock block;
                ushort? nullable2;
                this.m_initialIntersectionStart = MyBlockBuilderBase.IntersectionStart;
                this.m_initialIntersectionDirection = MyBlockBuilderBase.IntersectionDirection;
                float cubeSize = MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize);
                if ((startRemove != null) || !this.GetAddAndRemovePositions(cubeSize, this.PlacingSmallGridOnLargeStatic, out vectori, out nullable, out vectori3, out vectori2, out block, out nullable2, null))
                {
                    startBuild = 0;
                }
                else
                {
                    startBuild = new Vector3I?(vectori);
                }
            }
        }

        protected virtual void StartRemoving()
        {
            this.StartRemoving(this.m_gizmo.SpaceDefault.m_startBuild, ref this.m_gizmo.SpaceDefault.m_startRemove);
        }

        protected void StartRemoving(Vector3I? startBuild, ref Vector3I? startRemove)
        {
            if (!this.PlacingSmallGridOnLargeStatic)
            {
                this.m_initialIntersectionStart = MyBlockBuilderBase.IntersectionStart;
                this.m_initialIntersectionDirection = MyBlockBuilderBase.IntersectionDirection;
                if ((this.CurrentGrid != null) && (startBuild == null))
                {
                    double num;
                    startRemove = base.IntersectCubes(this.CurrentGrid, out num);
                }
            }
        }

        public void StartStaticGridPlacement(MyCubeSize cubeSize, bool isStatic)
        {
            MyCubeBlockDefinition definition;
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (localCharacter != null)
            {
                localCharacter.SwitchToWeapon((MyToolbarItemWeapon) null);
            }
            MyDefinitionId defId = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), "LargeBlockArmorBlock");
            if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(defId, out definition))
            {
                this.Activate(new MyDefinitionId?(definition.Id));
                this.m_stationPlacement = true;
            }
        }

        public virtual void StopBuilding()
        {
            MyCubeBuilderGizmo.MyGizmoSpaceProperties[] spaces;
            int num2;
            if ((!this.GridAndBlockValid && !this.VoxelMapAndBlockValid) || Sandbox.Game.Entities.MyEntities.MemoryLimitReachedReport)
            {
                spaces = this.m_gizmo.Spaces;
                num2 = 0;
                while (num2 < spaces.Length)
                {
                    MyCubeBuilderGizmo.MyGizmoSpaceProperties properties1 = spaces[num2];
                    properties1.m_startBuild = null;
                    properties1.m_continueBuild = null;
                    properties1.m_startRemove = null;
                    num2++;
                }
            }
            else
            {
                bool smallViewChange = this.CheckSmallViewChange();
                this.m_blocksBuildQueue.Clear();
                this.m_tmpBlockPositionList.Clear();
                this.UpdateGizmos(true, true, false);
                int num = 0;
                spaces = this.m_gizmo.Spaces;
                for (num2 = 0; num2 < spaces.Length; num2++)
                {
                    if (spaces[num2].Enabled)
                    {
                        num++;
                    }
                }
                foreach (MyCubeBuilderGizmo.MyGizmoSpaceProperties properties in this.m_gizmo.Spaces)
                {
                    if (properties.Enabled)
                    {
                        this.StopBuilding(smallViewChange, ref properties.m_startBuild, ref properties.m_startRemove, ref properties.m_continueBuild, properties.m_min, properties.m_max, properties.m_centerPos, ref properties.m_localMatrixAdd, properties.m_blockDefinition);
                    }
                }
                if (this.m_blocksBuildQueue.Count > 0)
                {
                    this.CurrentGrid.BuildBlocks(MyPlayer.SelectedColor, this.m_blocksBuildQueue, MySession.Static.LocalCharacterEntityId, MySession.Static.LocalPlayerId);
                    this.m_blocksBuildQueue.Clear();
                }
                if (this.m_tmpBlockPositionList.Count > 0)
                {
                    this.CurrentGrid.RazeBlocks(this.m_tmpBlockPositionList, MySession.Static.LocalCharacterEntityId);
                    this.m_tmpBlockPositionList.Clear();
                }
            }
        }

        protected unsafe void StopBuilding(bool smallViewChange, ref Vector3I? startBuild, ref Vector3I? startRemove, ref Vector3I? continueBuild, Vector3I blockMinPosition, Vector3I blockMaxPosition, Vector3I blockCenterPosition, ref Matrix localMatrixAdd, MyCubeBlockDefinition blockDefinition)
        {
            if ((startBuild == 0) || !((continueBuild != 0) | smallViewChange))
            {
                if ((startRemove != 0) && ((continueBuild != 0) | smallViewChange))
                {
                    Vector3I vectori13;
                    Vector3I vectori14;
                    int num2;
                    MyGuiAudio.PlaySound(MyGuiSounds.HudDeleteBlock);
                    Vector3I pos = startRemove.Value;
                    Vector3I vectori12 = startRemove.Value;
                    if (smallViewChange)
                    {
                        continueBuild = startRemove;
                    }
                    ComputeSteps(startRemove.Value, continueBuild.Value, Vector3I.One, out vectori13, out vectori14, out num2);
                    pos = Vector3I.Min(startRemove.Value, continueBuild.Value);
                    Vector3UByte size = new Vector3UByte(Vector3I.Max(startRemove.Value, continueBuild.Value) - pos);
                    this.CurrentGrid.RazeBlocksDelayed(ref pos, ref size, MySession.Static.LocalCharacterEntityId);
                }
            }
            else
            {
                Vector3I vectori3;
                Vector3I vectori4;
                Vector3I vectori5;
                int num;
                int num1;
                Vector3I vec = blockMinPosition - blockCenterPosition;
                Vector3I vectori2 = blockMaxPosition - blockCenterPosition;
                Vector3I.TransformNormal(ref this.CurrentBlockDefinition.Size, ref localMatrixAdd, out vectori3);
                vectori3 = Vector3I.Abs(vectori3);
                if (smallViewChange)
                {
                    continueBuild = startBuild;
                }
                ComputeSteps(startBuild.Value, continueBuild.Value, vectori3, out vectori4, out vectori5, out num);
                Vector3I vectori6 = blockCenterPosition;
                Quaternion rot = Quaternion.CreateFromRotationMatrix(localMatrixAdd);
                MyDefinitionId id = blockDefinition.Id;
                if ((!blockDefinition.RandomRotation || (blockDefinition.Size.X != blockDefinition.Size.Y)) || (blockDefinition.Size.X != blockDefinition.Size.Z))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (blockDefinition.Rotation == MyBlockRotation.Both) ? 1 : ((int) (blockDefinition.Rotation == MyBlockRotation.Vertical));
                }
                if (num1 == 0)
                {
                    MyCubeGrid.MyBlockBuildArea area = new MyCubeGrid.MyBlockBuildArea {
                        PosInGrid = vectori6,
                        BlockMin = new Vector3B(vec),
                        BlockMax = new Vector3B(vectori2),
                        BuildAreaSize = new Vector3UByte(vectori5),
                        StepDelta = new Vector3B(vectori4),
                        OrientationForward = Base6Directions.GetForward(ref rot),
                        OrientationUp = Base6Directions.GetUp(ref rot),
                        DefinitionId = id,
                        ColorMaskHSV = MyPlayer.SelectedColor.PackHSVToUint()
                    };
                    this.CurrentGrid.BuildBlocks(ref area, MySession.Static.LocalCharacterEntityId, MySession.Static.LocalPlayerId);
                    if (this.OnBlockAdded != null)
                    {
                        this.OnBlockAdded(blockDefinition);
                    }
                }
                else
                {
                    Vector3I vectori7;
                    this.m_blocksBuildQueue.Clear();
                    vectori7.X = 0;
                    while (true)
                    {
                        if (vectori7.X >= vectori5.X)
                        {
                            if (this.m_blocksBuildQueue.Count > 0)
                            {
                                MyGuiAudio.PlaySound(MyGuiSounds.HudPlaceBlock);
                            }
                            break;
                        }
                        vectori7.Y = 0;
                        while (true)
                        {
                            if (vectori7.Y >= vectori5.Y)
                            {
                                int* numPtr3 = (int*) ref vectori7.X;
                                numPtr3[0]++;
                                break;
                            }
                            vectori7.Z = 0;
                            while (true)
                            {
                                Quaternion quaternion2;
                                if (vectori7.Z >= vectori5.Z)
                                {
                                    int* numPtr2 = (int*) ref vectori7.Y;
                                    numPtr2[0]++;
                                    break;
                                }
                                Vector3I center = (Vector3I) (blockCenterPosition + (vectori7 * vectori4));
                                Vector3I min = (Vector3I) (blockMinPosition + (vectori7 * vectori4));
                                Vector3I max = (Vector3I) (blockMaxPosition + (vectori7 * vectori4));
                                if (blockDefinition.Rotation == MyBlockRotation.Both)
                                {
                                    Base6Directions.Direction dir = (Base6Directions.Direction) ((byte) (Math.Abs(MyRandom.Instance.Next()) % 6));
                                    Base6Directions.Direction direction2 = dir;
                                    while (true)
                                    {
                                        if (Vector3I.Dot(Base6Directions.GetIntVector(dir), Base6Directions.GetIntVector(direction2)) == 0)
                                        {
                                            quaternion2 = Quaternion.CreateFromForwardUp((Vector3) Base6Directions.GetIntVector(dir), (Vector3) Base6Directions.GetIntVector(direction2));
                                            break;
                                        }
                                        direction2 = (Base6Directions.Direction) ((byte) (Math.Abs(MyRandom.Instance.Next()) % 6));
                                    }
                                }
                                else
                                {
                                    Base6Directions.Direction up = Base6Directions.Direction.Up;
                                    Base6Directions.Direction dir = up;
                                    while (true)
                                    {
                                        if (Vector3I.Dot(Base6Directions.GetIntVector(dir), Base6Directions.GetIntVector(up)) == 0)
                                        {
                                            quaternion2 = Quaternion.CreateFromForwardUp((Vector3) Base6Directions.GetIntVector(dir), (Vector3) Base6Directions.GetIntVector(up));
                                            break;
                                        }
                                        dir = (Base6Directions.Direction) ((byte) (Math.Abs(MyRandom.Instance.Next()) % 6));
                                    }
                                }
                                this.m_blocksBuildQueue.Add(new MyCubeGrid.MyBlockLocation(blockDefinition.Id, min, max, center, quaternion2, MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM), MySession.Static.LocalPlayerId));
                                int* numPtr1 = (int*) ref vectori7.Z;
                                numPtr1[0]++;
                            }
                        }
                    }
                }
            }
            startBuild = 0;
            continueBuild = 0;
            startRemove = 0;
        }

        public static Vector3 TransformLargeGridHitCoordToSmallGrid(Vector3D coords, MatrixD worldMatrixNormalizedInv, float gridSize)
        {
            Vector3D vectord = (Vector3D.Transform(coords, worldMatrixNormalizedInv) / ((double) gridSize)) * 10.0;
            Vector3I vectori = Vector3I.Sign((Vector3) vectord);
            return (((vectori * Vector3I.Round(Vector3D.Abs(vectord - (0.5 * vectori)))) + (0.5 * vectori)) / 10.0);
        }

        private void TriggerRespawnShipNotification(MyCubeGrid newCurrentGrid)
        {
            MyNotificationSingletons singleNotification = MySession.Static.Settings.RespawnShipDelete ? MyNotificationSingletons.RespawnShipWarning : MyNotificationSingletons.BuildingOnRespawnShipWarning;
            if ((newCurrentGrid == null) || !newCurrentGrid.IsRespawnGrid)
            {
                MyHud.Notifications.Remove(singleNotification);
            }
            else
            {
                MyHud.Notifications.Add(singleNotification);
            }
        }

        protected override void UnloadData()
        {
            this.Deactivate();
            base.UnloadData();
            this.RemoveSymmetryNotification();
            this.m_gizmo.Clear();
            this.CurrentGrid = null;
            this.UnloadRenderObjects();
            this.m_cubeBuilderState = null;
        }

        private void UnloadRenderObjects()
        {
            this.m_gizmo.RemoveGizmoCubeParts();
            this.m_renderData.UnloadRenderObjects();
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            this.CalculateCubePlacement();
        }

        public override void UpdateBeforeSimulation()
        {
            this.UpdateNotificationBlockLimit();
            MyShipController controlledEntity = MySession.Static.ControlledEntity as MyShipController;
            if (!Static.IsActivated || (controlledEntity == null))
            {
                Static.canBuild = true;
            }
            else if ((!controlledEntity.hasPower || !controlledEntity.BuildingMode) || !Sandbox.Game.Entities.MyEntities.IsInsideWorld(controlledEntity.PositionComp.GetPosition()))
            {
                Static.canBuild = false;
            }
            else
            {
                Static.canBuild = true;
            }
        }

        protected static void UpdateBlockInfoHud()
        {
            MyHud.BlockInfo.Visible = false;
            MyCubeBlockDefinition currentBlockDefinition = Static.CurrentBlockDefinition;
            if (((currentBlockDefinition != null) && Static.IsActivated) && ((MyFakes.ENABLE_SMALL_GRID_BLOCK_INFO || (currentBlockDefinition == null)) || (currentBlockDefinition.CubeSize != MyCubeSize.Small)))
            {
                MySlimBlock.SetBlockComponents(MyHud.BlockInfo, currentBlockDefinition, BuildComponent.GetBuilderInventory(MySession.Static.LocalCharacter));
                MyHud.BlockInfo.Visible = true;
            }
        }

        protected virtual void UpdateCubeBlockDefinition(MyDefinitionId? id)
        {
            this.m_cubeBuilderState.UpdateCubeBlockDefinition(id, this.m_gizmo.SpaceDefault.m_localMatrixAdd);
            if ((this.CurrentBlockDefinition != null) && this.IsCubeSizeModesAvailable)
            {
                this.m_cubeBuilderState.UpdateComplementBlock();
            }
            this.m_cubeBuilderState.UpdateBlockDefinitionStages(id);
            if (this.m_cubeBuilderState.CurrentBlockDefinition != null)
            {
                Quaternion quaternion;
                MyDefinitionId local1;
                this.m_gizmo.RotationOptions = MyCubeGridDefinitions.GetCubeRotationOptions(this.CurrentBlockDefinition);
                if (id != null)
                {
                    local1 = id.Value;
                }
                else
                {
                    local1 = new MyDefinitionId();
                }
                MyDefinitionId key = local1;
                if (this.m_cubeBuilderState.RotationsByDefinitionHash.TryGetValue(key, out quaternion))
                {
                    this.m_gizmo.SpaceDefault.m_localMatrixAdd = Matrix.CreateFromQuaternion(quaternion);
                }
                else
                {
                    this.m_gizmo.SpaceDefault.m_localMatrixAdd = Matrix.Identity;
                }
            }
        }

        protected virtual void UpdateCubeBlockStageDefinition(MyCubeBlockDefinition stageCubeBlockDefinition)
        {
            Quaternion quaternion;
            if ((this.CurrentBlockDefinition != null) && (stageCubeBlockDefinition != null))
            {
                Quaternion quaternion2 = Quaternion.CreateFromRotationMatrix(this.m_gizmo.SpaceDefault.m_localMatrixAdd);
                this.m_cubeBuilderState.RotationsByDefinitionHash[this.CurrentBlockDefinition.Id] = quaternion2;
            }
            this.CurrentBlockDefinition = stageCubeBlockDefinition;
            this.m_gizmo.RotationOptions = MyCubeGridDefinitions.GetCubeRotationOptions(this.CurrentBlockDefinition);
            if (this.m_cubeBuilderState.RotationsByDefinitionHash.TryGetValue(stageCubeBlockDefinition.Id, out quaternion))
            {
                this.m_gizmo.SpaceDefault.m_localMatrixAdd = Matrix.CreateFromQuaternion(quaternion);
            }
            else
            {
                this.m_gizmo.SpaceDefault.m_localMatrixAdd = Matrix.Identity;
            }
        }

        protected virtual void UpdateGizmo(MyCubeBuilderGizmo.MyGizmoSpaceProperties gizmoSpace, bool add, bool remove, bool draw)
        {
            if (gizmoSpace.Enabled)
            {
                if (!Static.canBuild)
                {
                    gizmoSpace.m_showGizmoCube = false;
                    gizmoSpace.m_buildAllowed = false;
                }
                if (this.DynamicMode)
                {
                    this.UpdateGizmo_DynamicMode(gizmoSpace);
                }
                else if (this.CurrentGrid != null)
                {
                    this.UpdateGizmo_Grid(gizmoSpace, add, remove, draw);
                }
                else
                {
                    this.UpdateGizmo_VoxelMap(gizmoSpace, add, remove, draw);
                }
            }
        }

        private void UpdateGizmo_DynamicMode(MyCubeBuilderGizmo.MyGizmoSpaceProperties gizmoSpace)
        {
            int num1;
            gizmoSpace.m_animationProgress = 1f;
            float cubeSize = MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize);
            BoundingBoxD gizmoBox = new BoundingBoxD((Vector3D) ((-this.CurrentBlockDefinition.Size * cubeSize) * 0.5f), (Vector3D) ((this.CurrentBlockDefinition.Size * cubeSize) * 0.5f));
            MyGridPlacementSettings settings = (this.CurrentBlockDefinition.CubeSize == MyCubeSize.Large) ? MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.LargeGrid : MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.SmallGrid;
            MatrixD worldMatrixAdd = gizmoSpace.m_worldMatrixAdd;
            MyCubeGrid.GetCubeParts(this.CurrentBlockDefinition, Vector3I.Zero, Matrix.Identity, cubeSize, gizmoSpace.m_cubeModelsTemp, gizmoSpace.m_cubeMatricesTemp, gizmoSpace.m_cubeNormals, gizmoSpace.m_patternOffsets, true);
            if (gizmoSpace.m_showGizmoCube)
            {
                this.m_gizmo.AddFastBuildParts(gizmoSpace, this.CurrentBlockDefinition, null);
                this.m_gizmo.UpdateGizmoCubeParts(gizmoSpace, this.m_renderData, ref MatrixD.Identity, this.CurrentBlockDefinition);
            }
            BuildComponent.GetGridSpawnMaterials(this.CurrentBlockDefinition, worldMatrixAdd, false);
            if (!MySession.Static.CreativeToolsEnabled(Sync.MyId))
            {
                gizmoSpace.m_buildAllowed &= BuildComponent.HasBuildingMaterials(MySession.Static.LocalCharacter, false);
            }
            MatrixD invGridWorldMatrix = MatrixD.Invert(worldMatrixAdd);
            if ((MySession.Static.SurvivalMode && !MyBlockBuilderBase.SpectatorIsBuilding) && !MySession.Static.CreativeToolsEnabled(Sync.MyId))
            {
                if (!MyCubeBuilderGizmo.DefaultGizmoCloseEnough(ref invGridWorldMatrix, gizmoBox, cubeSize, MyBlockBuilderBase.IntersectionDistance) || (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Spectator))
                {
                    gizmoSpace.m_buildAllowed = false;
                    gizmoSpace.m_removeBlock = null;
                }
                if (MyBlockBuilderBase.CameraControllerSpectator)
                {
                    gizmoSpace.m_showGizmoCube = false;
                    gizmoSpace.m_buildAllowed = false;
                    return;
                }
            }
            if (!gizmoSpace.m_dynamicBuildAllowed)
            {
                MyBlockOrientation? blockOrientation = null;
                bool flag = MyCubeGrid.TestBlockPlacementArea(this.CurrentBlockDefinition, blockOrientation, worldMatrixAdd, ref settings, gizmoBox, this.DynamicMode, null, true);
                gizmoSpace.m_buildAllowed &= flag;
            }
            gizmoSpace.m_showGizmoCube = true;
            gizmoSpace.m_cubeMatricesTemp.Clear();
            gizmoSpace.m_cubeModelsTemp.Clear();
            if (((MyHud.Stats.GetStat(MyStringHash.GetOrCompute("hud_mode")).CurrentValue != 1f) || MyHud.CutsceneHud) || !MySandboxGame.Config.RotationHints)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) MyFakes.ENABLE_ROTATION_HINTS;
            }
            this.m_rotationHints.CalculateRotationHints(worldMatrixAdd, (bool) num1, false, false);
            if (!this.CurrentBlockDefinition.IsStandAlone)
            {
                gizmoSpace.m_buildAllowed = false;
            }
            gizmoSpace.m_buildAllowed &= !this.IntersectsCharacterOrCamera(gizmoSpace, cubeSize, ref invGridWorldMatrix);
            if (!MySessionComponentSafeZones.IsActionAllowed(gizmoBox.TransformFast(ref worldMatrixAdd), MySafeZoneAction.Building, 0L))
            {
                gizmoSpace.m_buildAllowed = false;
                gizmoSpace.m_removeBlock = null;
            }
            if (gizmoSpace.m_showGizmoCube)
            {
                Color white = Color.White;
                MyStringId id = gizmoSpace.m_buildAllowed ? ID_GIZMO_DRAW_LINE : ID_GIZMO_DRAW_LINE_RED;
                if (gizmoSpace.SymmetryPlane == MySymmetrySettingModeEnum.Disabled)
                {
                    MyStringId? faceMaterial = null;
                    MySimpleObjectDraw.DrawTransparentBox(ref worldMatrixAdd, ref gizmoBox, ref white, MySimpleObjectRasterizer.Wireframe, 1, 0.04f, faceMaterial, new MyStringId?(id), false, -1, MyBillboard.BlendTypeEnum.LDR, 1f, null);
                }
                this.AddFastBuildModels(gizmoSpace, MatrixD.Identity, gizmoSpace.m_cubeMatricesTemp, gizmoSpace.m_cubeModelsTemp, gizmoSpace.m_blockDefinition);
                for (int i = 0; i < gizmoSpace.m_cubeMatricesTemp.Count; i++)
                {
                    string str = gizmoSpace.m_cubeModelsTemp[i];
                    if (!string.IsNullOrEmpty(str))
                    {
                        int model = MyModel.GetId(str);
                        this.m_renderData.AddInstance(model, gizmoSpace.m_cubeMatricesTemp[i], ref MatrixD.Identity, MyPlayer.SelectedColor, null, 1f);
                    }
                }
            }
        }

        private unsafe void UpdateGizmo_Grid(MyCubeBuilderGizmo.MyGizmoSpaceProperties gizmoSpace, bool add, bool remove, bool draw)
        {
            Color color = new Color(Color.Green * 0.6f, 1f);
            Color color2 = new Color(Color.Red * 0.8f, 1f);
            Color yellow = Color.Yellow;
            Color black = Color.Black;
            Color gray = Color.Gray;
            Color white = Color.White;
            if (add)
            {
                if (!this.m_animationLock)
                {
                    gizmoSpace.m_animationLastMatrix = gizmoSpace.m_localMatrixAdd;
                }
                MatrixD localMatrixAdd = gizmoSpace.m_localMatrixAdd;
                if (gizmoSpace.m_animationProgress < 1f)
                {
                    localMatrixAdd = MatrixD.Slerp(gizmoSpace.m_animationLastMatrix, gizmoSpace.m_localMatrixAdd, gizmoSpace.m_animationProgress);
                }
                else if (gizmoSpace.m_animationProgress >= 1f)
                {
                    this.m_animationLock = false;
                    gizmoSpace.m_animationLastMatrix = gizmoSpace.m_localMatrixAdd;
                }
                MatrixD worldMatrix = localMatrixAdd * this.CurrentGrid.WorldMatrix;
                if ((gizmoSpace.m_startBuild != null) && (gizmoSpace.m_continueBuild != null))
                {
                    gizmoSpace.m_buildAllowed = true;
                }
                if (this.PlacingSmallGridOnLargeStatic && (gizmoSpace.m_positionsSmallOnLarge.Count == 0))
                {
                    return;
                }
                if (this.CurrentBlockDefinition != null)
                {
                    Vector3 vector;
                    MyBlockOrientation orientation = new MyBlockOrientation(ref gizmoSpace.m_localMatrixAdd.GetOrientation());
                    if (!this.PlacingSmallGridOnLargeStatic)
                    {
                        bool flag2 = CheckValidBlockRotation(gizmoSpace.m_localMatrixAdd, this.CurrentBlockDefinition.Direction, this.CurrentBlockDefinition.Rotation);
                        int? ignoreMultiblockId = null;
                        gizmoSpace.m_buildAllowed &= flag2 & this.CurrentGrid.CanPlaceBlock(gizmoSpace.m_min, gizmoSpace.m_max, orientation, gizmoSpace.m_blockDefinition, ignoreMultiblockId, false);
                    }
                    BuildComponent.GetBlockPlacementMaterials(gizmoSpace.m_blockDefinition, gizmoSpace.m_addPos, orientation, this.CurrentGrid);
                    if (!MySession.Static.CreativeToolsEnabled(Sync.MyId))
                    {
                        gizmoSpace.m_buildAllowed &= BuildComponent.HasBuildingMaterials(MySession.Static.LocalCharacter, false);
                    }
                    if ((!this.PlacingSmallGridOnLargeStatic && (MySession.Static.SurvivalMode && !MySession.Static.CreativeToolsEnabled(Sync.MyId))) && !MyBlockBuilderBase.SpectatorIsBuilding)
                    {
                        Vector3 max = (this.m_gizmo.SpaceDefault.m_max + new Vector3(0.5f)) * this.CurrentGrid.GridSize;
                        BoundingBoxD gizmoBox = new BoundingBoxD((Vector3D) ((this.m_gizmo.SpaceDefault.m_min - new Vector3(0.5f)) * this.CurrentGrid.GridSize), max);
                        if (!MyCubeBuilderGizmo.DefaultGizmoCloseEnough(ref base.m_invGridWorldMatrix, gizmoBox, this.CurrentGrid.GridSize, MyBlockBuilderBase.IntersectionDistance) || MyBlockBuilderBase.CameraControllerSpectator)
                        {
                            gizmoSpace.m_buildAllowed = false;
                            gizmoSpace.m_removeBlock = null;
                            return;
                        }
                    }
                    if (gizmoSpace.m_buildAllowed)
                    {
                        Quaternion.CreateFromRotationMatrix(ref gizmoSpace.m_localMatrixAdd, out gizmoSpace.m_rotation);
                        if ((gizmoSpace.SymmetryPlane == MySymmetrySettingModeEnum.Disabled) && !this.PlacingSmallGridOnLargeStatic)
                        {
                            MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = this.CurrentBlockDefinition.GetBuildProgressModelMountPoints(MyComponentStack.NewBlockIntegrity);
                            gizmoSpace.m_buildAllowed = MyCubeGrid.CheckConnectivity(this.CurrentGrid, this.CurrentBlockDefinition, buildProgressModelMountPoints, ref gizmoSpace.m_rotation, ref gizmoSpace.m_centerPos);
                        }
                    }
                    Color color3 = color;
                    if (!this.PlacingSmallGridOnLargeStatic)
                    {
                        gizmoSpace.m_showGizmoCube = !this.IntersectsCharacterOrCamera(gizmoSpace, this.CurrentGrid.GridSize, ref base.m_invGridWorldMatrix);
                    }
                    else
                    {
                        MatrixD inverseBlockInGridWorldMatrix = MatrixD.Invert(gizmoSpace.m_worldMatrixAdd);
                        gizmoSpace.m_showGizmoCube = !this.IntersectsCharacterOrCamera(gizmoSpace, this.CurrentGrid.GridSize, ref inverseBlockInGridWorldMatrix);
                    }
                    gizmoSpace.m_buildAllowed &= gizmoSpace.m_showGizmoCube;
                    Vector3D zero = Vector3D.Zero;
                    Vector3D translation = gizmoSpace.m_worldMatrixAdd.Translation;
                    MatrixD worldMatrixAdd = gizmoSpace.m_worldMatrixAdd;
                    int num = 0;
                    vector.X = 0f;
                    while (true)
                    {
                        if (vector.X >= this.CurrentBlockDefinition.Size.X)
                        {
                            zero /= (double) this.CurrentBlockDefinition.Size.Size;
                            if (!this.m_animationLock)
                            {
                                gizmoSpace.m_animationProgress = 0f;
                                gizmoSpace.m_animationLastPosition = zero;
                            }
                            else if (gizmoSpace.m_animationProgress < 1f)
                            {
                                zero = Vector3D.Lerp(gizmoSpace.m_animationLastPosition, zero, (double) gizmoSpace.m_animationProgress);
                            }
                            zero = Vector3D.Transform(zero, this.CurrentGrid.WorldMatrix);
                            worldMatrixAdd.Translation = zero;
                            gizmoSpace.m_worldMatrixAdd = worldMatrixAdd;
                            float gridSize = this.PlacingSmallGridOnLargeStatic ? MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize) : this.CurrentGrid.GridSize;
                            BoundingBoxD localAabb = new BoundingBoxD((Vector3D) ((-this.CurrentBlockDefinition.Size * gridSize) * 0.5f), (Vector3D) ((this.CurrentBlockDefinition.Size * gridSize) * 0.5f));
                            MyGridPlacementSettings gridPlacementSettings = MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.GetGridPlacementSettings(this.CurrentBlockDefinition.CubeSize, this.CurrentGrid.IsStatic);
                            MyBlockOrientation orientation2 = new MyBlockOrientation(ref Quaternion.Identity);
                            bool flag = MyCubeGrid.TestVoxelPlacement(this.CurrentBlockDefinition, gridPlacementSettings, false, worldMatrixAdd, localAabb);
                            gizmoSpace.m_buildAllowed &= flag;
                            if (this.PlacingSmallGridOnLargeStatic)
                            {
                                if ((MySession.Static.SurvivalMode && !MyBlockBuilderBase.SpectatorIsBuilding) && !MySession.Static.CreativeToolsEnabled(Sync.MyId))
                                {
                                    MatrixD invGridWorldMatrix = Matrix.Invert((Matrix) worldMatrixAdd);
                                    BuildComponent.GetBlockPlacementMaterials(this.CurrentBlockDefinition, gizmoSpace.m_addPos, orientation, this.CurrentGrid);
                                    gizmoSpace.m_buildAllowed &= BuildComponent.HasBuildingMaterials(MySession.Static.LocalCharacter, false);
                                    if (!MyCubeBuilderGizmo.DefaultGizmoCloseEnough(ref invGridWorldMatrix, localAabb, gridSize, MyBlockBuilderBase.IntersectionDistance) || MyBlockBuilderBase.CameraControllerSpectator)
                                    {
                                        gizmoSpace.m_buildAllowed = false;
                                        gizmoSpace.m_removeBlock = null;
                                        return;
                                    }
                                }
                                MyGridPlacementSettings settings = MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.GetGridPlacementSettings(this.CurrentGrid.GridSizeEnum, this.CurrentGrid.IsStatic);
                                bool flag4 = CheckValidBlockRotation(gizmoSpace.m_localMatrixAdd, this.CurrentBlockDefinition.Direction, this.CurrentBlockDefinition.Rotation) && MyCubeGrid.TestBlockPlacementArea(this.CurrentBlockDefinition, new MyBlockOrientation?(orientation2), worldMatrixAdd, ref settings, localAabb, !this.CurrentGrid.IsStatic, null, false);
                                gizmoSpace.m_buildAllowed &= flag4;
                                if (gizmoSpace.m_buildAllowed && (gizmoSpace.SymmetryPlane == MySymmetrySettingModeEnum.Disabled))
                                {
                                    gizmoSpace.m_buildAllowed &= MyCubeGrid.CheckConnectivitySmallBlockToLargeGrid(this.CurrentGrid, this.CurrentBlockDefinition, ref gizmoSpace.m_rotation, ref gizmoSpace.m_addDir);
                                }
                                gizmoSpace.m_worldMatrixAdd = worldMatrixAdd;
                            }
                            if (!MySessionComponentSafeZones.IsActionAllowed(localAabb.TransformFast(ref worldMatrixAdd), MySafeZoneAction.Building, 0L))
                            {
                                gizmoSpace.m_buildAllowed = false;
                                gizmoSpace.m_removeBlock = null;
                            }
                            color3 = Color.White;
                            MyStringId id = gizmoSpace.m_buildAllowed ? ID_GIZMO_DRAW_LINE : ID_GIZMO_DRAW_LINE_RED;
                            if (gizmoSpace.SymmetryPlane == MySymmetrySettingModeEnum.Disabled)
                            {
                                MyStringId? nullable2;
                                MatrixD* xdPtr2;
                                Color* colorPtr1;
                                if (!MyFakes.ENABLE_VR_BUILDING)
                                {
                                    xdPtr2 = (MatrixD*) ref worldMatrixAdd;
                                    worldMatrix.Translation = worldMatrixAdd.Translation;
                                    colorPtr1 = (Color*) ref color3;
                                    nullable2 = null;
                                    MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref localAabb, ref color3, MySimpleObjectRasterizer.Wireframe, 1, 0.04f, nullable2, new MyStringId?(id), false, -1, MyBillboard.BlendTypeEnum.LDR, 1f, null);
                                }
                                else
                                {
                                    Vector3 vector4 = (Vector3) (-0.5f * gizmoSpace.m_addDir);
                                    if (gizmoSpace.m_addPosSmallOnLarge != null)
                                    {
                                        vector4 = (Vector3) ((-0.5f * (MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize) / this.CurrentGrid.GridSize)) * gizmoSpace.m_addDir);
                                    }
                                    vector4 *= this.CurrentGrid.GridSize;
                                    Vector3I vectori3 = Vector3I.Round(Vector3.Abs(Vector3.TransformNormal((Vector3) this.CurrentBlockDefinition.Size, gizmoSpace.m_localMatrixAdd)));
                                    Vector3 vector5 = (Vector3) (((gridSize * 0.5f) * (vectori3 * (Vector3I.One - Vector3I.Abs(gizmoSpace.m_addDir)))) + (0.02f * Vector3I.Abs(gizmoSpace.m_addDir)));
                                    BoundingBoxD localbox = new BoundingBoxD(-vector5 + vector4, vector5 + vector4);
                                    nullable2 = null;
                                    MySimpleObjectDraw.DrawTransparentBox(ref (MatrixD) ref xdPtr2, ref localbox, ref (Color) ref colorPtr1, MySimpleObjectRasterizer.Wireframe, 1, (gizmoSpace.m_addPosSmallOnLarge != null) ? 0.04f : 0.06f, nullable2, new MyStringId?(id), false, -1, MyBillboard.BlendTypeEnum.LDR, 1f, null);
                                }
                            }
                            gizmoSpace.m_cubeMatricesTemp.Clear();
                            gizmoSpace.m_cubeModelsTemp.Clear();
                            if (gizmoSpace.m_showGizmoCube)
                            {
                                Vector3D vectord4;
                                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS)
                                {
                                    float cubeSize = MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize);
                                    if (!this.PlacingSmallGridOnLargeStatic)
                                    {
                                        cubeSize = this.CurrentGrid.GridSize;
                                    }
                                    DrawMountPoints(cubeSize, this.CurrentBlockDefinition, ref worldMatrixAdd);
                                }
                                Vector3D.TransformNormal(ref this.CurrentBlockDefinition.ModelOffset, ref gizmoSpace.m_worldMatrixAdd, out vectord4);
                                worldMatrix.Translation = zero + (this.CurrentGrid.GridScale * vectord4);
                                this.AddFastBuildModels(gizmoSpace, worldMatrix, gizmoSpace.m_cubeMatricesTemp, gizmoSpace.m_cubeModelsTemp, gizmoSpace.m_blockDefinition);
                                for (int i = 0; i < gizmoSpace.m_cubeMatricesTemp.Count; i++)
                                {
                                    string str = gizmoSpace.m_cubeModelsTemp[i];
                                    if (!string.IsNullOrEmpty(str))
                                    {
                                        this.m_renderData.AddInstance(MyModel.GetId(str), gizmoSpace.m_cubeMatricesTemp[i], ref base.m_invGridWorldMatrix, MyPlayer.SelectedColor, null, 1f);
                                    }
                                }
                            }
                            if (gizmoSpace.SymmetryPlane == MySymmetrySettingModeEnum.Disabled)
                            {
                                int rotationHints;
                                int num9;
                                IMyHudStat stat = MyHud.Stats.GetStat(MyStringHash.GetOrCompute("hud_mode"));
                                if (MyHud.MinimalHud || MyHud.CutsceneHud)
                                {
                                    rotationHints = 0;
                                }
                                else
                                {
                                    rotationHints = (int) MySandboxGame.Config.RotationHints;
                                }
                                if (((rotationHints & draw) == 0) || !MyFakes.ENABLE_ROTATION_HINTS)
                                {
                                    num9 = 0;
                                }
                                else
                                {
                                    num9 = (int) (stat.CurrentValue == 1f);
                                }
                                this.m_rotationHints.CalculateRotationHints(worldMatrix, (bool) num9, false, false);
                            }
                            break;
                        }
                        vector.Y = 0f;
                        while (true)
                        {
                            if (vector.Y >= this.CurrentBlockDefinition.Size.Y)
                            {
                                float* singlePtr3 = (float*) ref vector.X;
                                singlePtr3[0]++;
                                break;
                            }
                            vector.Z = 0f;
                            while (true)
                            {
                                if (vector.Z >= this.CurrentBlockDefinition.Size.Z)
                                {
                                    float* singlePtr2 = (float*) ref vector.Y;
                                    singlePtr2[0]++;
                                    break;
                                }
                                if (!this.PlacingSmallGridOnLargeStatic)
                                {
                                    num++;
                                    Vector3I inputPosition = gizmoSpace.m_positions[num];
                                    Vector3D vectord3 = Vector3D.Transform((Vector3) (inputPosition * this.CurrentGrid.GridSize), this.CurrentGrid.WorldMatrix);
                                    zero += inputPosition * this.CurrentGrid.GridSize;
                                    MyCubeGrid.GetCubeParts(this.CurrentBlockDefinition, inputPosition, (Matrix) localMatrixAdd.GetOrientation(), this.CurrentGrid.GridSize, gizmoSpace.m_cubeModelsTemp, gizmoSpace.m_cubeMatricesTemp, gizmoSpace.m_cubeNormals, gizmoSpace.m_patternOffsets, false);
                                    if (gizmoSpace.m_showGizmoCube)
                                    {
                                        int num5 = 0;
                                        while (true)
                                        {
                                            if (num5 >= gizmoSpace.m_cubeMatricesTemp.Count)
                                            {
                                                this.m_gizmo.AddFastBuildParts(gizmoSpace, this.CurrentBlockDefinition, this.CurrentGrid);
                                                this.m_gizmo.UpdateGizmoCubeParts(gizmoSpace, this.m_renderData, ref base.m_invGridWorldMatrix, this.CurrentBlockDefinition);
                                                break;
                                            }
                                            MatrixD xd8 = gizmoSpace.m_cubeMatricesTemp[num5] * this.CurrentGrid.WorldMatrix;
                                            xd8.Translation = vectord3;
                                            gizmoSpace.m_cubeMatricesTemp[num5] = xd8;
                                            num5++;
                                        }
                                    }
                                }
                                else
                                {
                                    float num3 = MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize) / this.CurrentGrid.GridSize;
                                    num++;
                                    Vector3I inputPosition = Vector3I.Round((Vector3D) (gizmoSpace.m_positionsSmallOnLarge[num] / ((double) num3)));
                                    Vector3D vectord2 = Vector3D.Transform((Vector3D) (gizmoSpace.m_positionsSmallOnLarge[num] * this.CurrentGrid.GridSize), this.CurrentGrid.WorldMatrix);
                                    zero += vectord2;
                                    worldMatrixAdd.Translation = vectord2;
                                    MyCubeGrid.GetCubeParts(this.CurrentBlockDefinition, inputPosition, gizmoSpace.m_localMatrixAdd.GetOrientation(), this.CurrentGrid.GridSize, gizmoSpace.m_cubeModelsTemp, gizmoSpace.m_cubeMatricesTemp, gizmoSpace.m_cubeNormals, gizmoSpace.m_patternOffsets, true);
                                    if (gizmoSpace.m_showGizmoCube)
                                    {
                                        int num4 = 0;
                                        while (true)
                                        {
                                            if (num4 >= gizmoSpace.m_cubeMatricesTemp.Count)
                                            {
                                                this.m_gizmo.AddFastBuildParts(gizmoSpace, this.CurrentBlockDefinition, this.CurrentGrid);
                                                this.m_gizmo.UpdateGizmoCubeParts(gizmoSpace, this.m_renderData, ref base.m_invGridWorldMatrix, null);
                                                break;
                                            }
                                            MatrixD xd7 = gizmoSpace.m_cubeMatricesTemp[num4];
                                            MatrixD* xdPtr1 = (MatrixD*) ref xd7;
                                            xdPtr1.Translation *= num3;
                                            xd7 *= this.CurrentGrid.WorldMatrix;
                                            xd7.Translation = vectord2;
                                            gizmoSpace.m_cubeMatricesTemp[num4] = xd7;
                                            num4++;
                                        }
                                    }
                                }
                                float* singlePtr1 = (float*) ref vector.Z;
                                singlePtr1[0]++;
                            }
                        }
                    }
                }
            }
            if ((gizmoSpace.m_startRemove != null) && (gizmoSpace.m_continueBuild != null))
            {
                gizmoSpace.m_buildAllowed = true;
                DrawRemovingCubes(gizmoSpace.m_startRemove, gizmoSpace.m_continueBuild, gizmoSpace.m_removeBlock);
            }
            else if (!remove || !this.ShowRemoveGizmo)
            {
                if (MySession.Static.SurvivalMode && !MySession.Static.CreativeToolsEnabled(Sync.MyId))
                {
                    bool cameraControllerSpectator = MyBlockBuilderBase.CameraControllerSpectator;
                    Vector3 max = (this.m_gizmo.SpaceDefault.m_max + new Vector3(0.5f)) * this.CurrentGrid.GridSize;
                    BoundingBoxD gizmoBox = new BoundingBoxD((Vector3D) ((this.m_gizmo.SpaceDefault.m_min - new Vector3(0.5f)) * this.CurrentGrid.GridSize), max);
                    if (!MyCubeBuilderGizmo.DefaultGizmoCloseEnough(ref base.m_invGridWorldMatrix, gizmoBox, this.CurrentGrid.GridSize, MyBlockBuilderBase.IntersectionDistance))
                    {
                        gizmoSpace.m_removeBlock = null;
                    }
                }
            }
            else
            {
                VRageMath.Vector4? nullable3;
                if (gizmoSpace.m_removeBlocksInMultiBlock.Count <= 0)
                {
                    if ((gizmoSpace.m_removeBlock != null) && !MyFakes.ENABLE_VR_BUILDING)
                    {
                        nullable3 = null;
                        DrawSemiTransparentBox(this.CurrentGrid, gizmoSpace.m_removeBlock, color2, false, new MyStringId?(ID_GIZMO_DRAW_LINE_RED), nullable3);
                    }
                }
                else
                {
                    this.m_tmpBlockPositionsSet.Clear();
                    GetAllBlocksPositions(gizmoSpace.m_removeBlocksInMultiBlock, this.m_tmpBlockPositionsSet);
                    foreach (Vector3I local1 in this.m_tmpBlockPositionsSet)
                    {
                        nullable3 = null;
                        DrawSemiTransparentBox(local1, local1, this.CurrentGrid, color2, false, new MyStringId?(ID_GIZMO_DRAW_LINE_RED), nullable3);
                    }
                    this.m_tmpBlockPositionsSet.Clear();
                }
                if (((gizmoSpace.m_removeBlock != null) && MyDebugDrawSettings.ENABLE_DEBUG_DRAW) && MyDebugDrawSettings.DEBUG_DRAW_REMOVE_CUBE_COORDS)
                {
                    MySlimBlock removeBlock = gizmoSpace.m_removeBlock;
                    MyCubeGrid cubeGrid = removeBlock.CubeGrid;
                    Matrix worldMatrix = (Matrix) cubeGrid.WorldMatrix;
                    MyRenderProxy.DebugDrawText3D(Vector3.Transform((Vector3) (removeBlock.Position * cubeGrid.GridSize), worldMatrix), removeBlock.Position.ToString(), Color.White, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                }
            }
            gizmoSpace.m_animationProgress += this.m_animationSpeed;
        }

        private unsafe void UpdateGizmo_VoxelMap(MyCubeBuilderGizmo.MyGizmoSpaceProperties gizmoSpace, bool add, bool remove, bool draw)
        {
            Vector3 vector;
            if (!this.m_animationLock)
            {
                gizmoSpace.m_animationLastMatrix = gizmoSpace.m_localMatrixAdd;
            }
            MatrixD localMatrixAdd = gizmoSpace.m_localMatrixAdd;
            if (gizmoSpace.m_animationProgress < 1f)
            {
                localMatrixAdd = MatrixD.Slerp(gizmoSpace.m_animationLastMatrix, gizmoSpace.m_localMatrixAdd, gizmoSpace.m_animationProgress);
            }
            else if (gizmoSpace.m_animationProgress >= 1f)
            {
                this.m_animationLock = false;
                gizmoSpace.m_animationLastMatrix = gizmoSpace.m_localMatrixAdd;
            }
            float cubeSize = MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize);
            Vector3D zero = Vector3D.Zero;
            MatrixD worldMatrixAdd = gizmoSpace.m_worldMatrixAdd;
            MatrixD xd3 = localMatrixAdd.GetOrientation();
            Color white = new Color(Color.Green * 0.6f, 1f);
            gizmoSpace.m_showGizmoCube = !this.IntersectsCharacterOrCamera(gizmoSpace, cubeSize, ref MatrixD.Identity);
            int num2 = 0;
            vector.X = 0f;
            while (vector.X < this.CurrentBlockDefinition.Size.X)
            {
                vector.Y = 0f;
                while (true)
                {
                    if (vector.Y >= this.CurrentBlockDefinition.Size.Y)
                    {
                        float* singlePtr3 = (float*) ref vector.X;
                        singlePtr3[0]++;
                        break;
                    }
                    vector.Z = 0f;
                    while (true)
                    {
                        if (vector.Z >= this.CurrentBlockDefinition.Size.Z)
                        {
                            float* singlePtr2 = (float*) ref vector.Y;
                            singlePtr2[0]++;
                            break;
                        }
                        num2++;
                        Vector3I inputPosition = gizmoSpace.m_positions[num2];
                        Vector3D position = (Vector3D) (inputPosition * cubeSize);
                        if (!MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.StaticGridAlignToCenter)
                        {
                            position += new Vector3D(0.5 * cubeSize, 0.5 * cubeSize, -0.5 * cubeSize);
                        }
                        Vector3D vectord3 = Vector3D.Transform(position, gizmoSpace.m_worldMatrixAdd);
                        zero += position;
                        MyCubeGrid.GetCubeParts(this.CurrentBlockDefinition, inputPosition, (Matrix) xd3, cubeSize, gizmoSpace.m_cubeModelsTemp, gizmoSpace.m_cubeMatricesTemp, gizmoSpace.m_cubeNormals, gizmoSpace.m_patternOffsets, false);
                        if (gizmoSpace.m_showGizmoCube)
                        {
                            int num3 = 0;
                            while (true)
                            {
                                if (num3 >= gizmoSpace.m_cubeMatricesTemp.Count)
                                {
                                    worldMatrixAdd.Translation = vectord3;
                                    MatrixD invGridWorldMatrix = MatrixD.Invert(xd3 * worldMatrixAdd);
                                    this.m_gizmo.AddFastBuildParts(gizmoSpace, this.CurrentBlockDefinition, null);
                                    this.m_gizmo.UpdateGizmoCubeParts(gizmoSpace, this.m_renderData, ref invGridWorldMatrix, this.CurrentBlockDefinition);
                                    break;
                                }
                                MatrixD xd6 = gizmoSpace.m_cubeMatricesTemp[num3] * gizmoSpace.m_worldMatrixAdd;
                                xd6.Translation = vectord3;
                                gizmoSpace.m_cubeMatricesTemp[num3] = xd6;
                                num3++;
                            }
                        }
                        float* singlePtr1 = (float*) ref vector.Z;
                        singlePtr1[0]++;
                    }
                }
            }
            zero /= (double) this.CurrentBlockDefinition.Size.Size;
            if (!this.m_animationLock)
            {
                gizmoSpace.m_animationProgress = 0f;
                gizmoSpace.m_animationLastPosition = zero;
            }
            else if (gizmoSpace.m_animationProgress < 1f)
            {
                zero = Vector3D.Lerp(gizmoSpace.m_animationLastPosition, zero, (double) gizmoSpace.m_animationProgress);
            }
            worldMatrixAdd.Translation = Vector3D.Transform(zero, gizmoSpace.m_worldMatrixAdd);
            worldMatrixAdd = xd3 * worldMatrixAdd;
            BoundingBoxD localAabb = new BoundingBoxD((Vector3D) ((-this.CurrentBlockDefinition.Size * cubeSize) * 0.5f), (Vector3D) ((this.CurrentBlockDefinition.Size * cubeSize) * 0.5f));
            MyGridPlacementSettings settings = (this.CurrentBlockDefinition.CubeSize == MyCubeSize.Large) ? MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.LargeStaticGrid : MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.SmallStaticGrid;
            MyBlockOrientation orientation = new MyBlockOrientation(ref Quaternion.Identity);
            bool flag = CheckValidBlockRotation(gizmoSpace.m_localMatrixAdd, this.CurrentBlockDefinition.Direction, this.CurrentBlockDefinition.Rotation) && MyCubeGrid.TestBlockPlacementArea(this.CurrentBlockDefinition, new MyBlockOrientation?(orientation), worldMatrixAdd, ref settings, localAabb, false, null, true);
            gizmoSpace.m_buildAllowed &= flag;
            gizmoSpace.m_buildAllowed &= gizmoSpace.m_showGizmoCube;
            gizmoSpace.m_worldMatrixAdd = worldMatrixAdd;
            BuildComponent.GetGridSpawnMaterials(this.CurrentBlockDefinition, worldMatrixAdd, true);
            if (!MySession.Static.CreativeToolsEnabled(Sync.MyId))
            {
                gizmoSpace.m_buildAllowed &= BuildComponent.HasBuildingMaterials(MySession.Static.LocalCharacter, false);
            }
            if ((MySession.Static.SurvivalMode && !MyBlockBuilderBase.SpectatorIsBuilding) && !MySession.Static.CreativeToolsEnabled(Sync.MyId))
            {
                BoundingBoxD gizmoBox = localAabb.TransformFast(ref worldMatrixAdd);
                if (!MyCubeBuilderGizmo.DefaultGizmoCloseEnough(ref MatrixD.Identity, gizmoBox, cubeSize, MyBlockBuilderBase.IntersectionDistance) || MyBlockBuilderBase.CameraControllerSpectator)
                {
                    gizmoSpace.m_buildAllowed = false;
                    gizmoSpace.m_showGizmoCube = false;
                    gizmoSpace.m_removeBlock = null;
                    return;
                }
            }
            white = Color.White;
            MyStringId id = gizmoSpace.m_buildAllowed ? ID_GIZMO_DRAW_LINE : ID_GIZMO_DRAW_LINE_RED;
            if (gizmoSpace.SymmetryPlane == MySymmetrySettingModeEnum.Disabled)
            {
                int rotationHints;
                MyStringId? faceMaterial = null;
                MySimpleObjectDraw.DrawTransparentBox(ref worldMatrixAdd, ref localAabb, ref white, MySimpleObjectRasterizer.Wireframe, 1, 0.04f, faceMaterial, new MyStringId?(id), false, -1, MyBillboard.BlendTypeEnum.LDR, 1f, null);
                if (MyHud.MinimalHud || MyHud.CutsceneHud)
                {
                    rotationHints = 0;
                }
                else
                {
                    rotationHints = (int) MySandboxGame.Config.RotationHints;
                }
                this.m_rotationHints.CalculateRotationHints(worldMatrixAdd, ((rotationHints & draw) != 0) && MyFakes.ENABLE_ROTATION_HINTS, false, false);
            }
            gizmoSpace.m_cubeMatricesTemp.Clear();
            gizmoSpace.m_cubeModelsTemp.Clear();
            if (gizmoSpace.m_showGizmoCube)
            {
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_MOUNT_POINTS)
                {
                    DrawMountPoints(cubeSize, this.CurrentBlockDefinition, ref worldMatrixAdd);
                }
                this.AddFastBuildModels(gizmoSpace, MatrixD.Identity, gizmoSpace.m_cubeMatricesTemp, gizmoSpace.m_cubeModelsTemp, gizmoSpace.m_blockDefinition);
                for (int i = 0; i < gizmoSpace.m_cubeMatricesTemp.Count; i++)
                {
                    string str = gizmoSpace.m_cubeModelsTemp[i];
                    if (!string.IsNullOrEmpty(str))
                    {
                        this.m_renderData.AddInstance(MyModel.GetId(str), gizmoSpace.m_cubeMatricesTemp[i], ref MatrixD.Identity, MyPlayer.SelectedColor, null, 1f);
                    }
                }
            }
            gizmoSpace.m_animationProgress += this.m_animationSpeed;
        }

        private void UpdateGizmos(bool addPos, bool removePos, bool draw)
        {
            if ((this.CurrentBlockDefinition != null) && (((this.CurrentGrid == null) || (this.CurrentGrid.Physics == null)) || !this.CurrentGrid.Physics.RigidBody.HasProperty(HkCharacterRigidBody.MANIPULATED_OBJECT)))
            {
                this.m_gizmo.SpaceDefault.m_blockDefinition = this.CurrentBlockDefinition;
                this.m_gizmo.EnableGizmoSpaces(this.CurrentBlockDefinition, this.CurrentGrid, this.UseSymmetry);
                this.m_renderData.BeginCollectingInstanceData();
                this.m_rotationHints.Clear();
                int length = this.m_gizmo.Spaces.Length;
                if (this.CurrentGrid != null)
                {
                    base.m_invGridWorldMatrix = this.CurrentGrid.PositionComp.WorldMatrixInvScaled;
                }
                for (int i = 0; i < length; i++)
                {
                    MyCubeBuilderGizmo.MyGizmoSpaceProperties gizmoSpace = this.m_gizmo.Spaces[i];
                    bool flag = addPos && this.BuildInputValid;
                    if (gizmoSpace.SymmetryPlane != MySymmetrySettingModeEnum.Disabled)
                    {
                        flag &= this.UseSymmetry;
                        removePos &= this.UseSymmetry;
                    }
                    else
                    {
                        Quaternion localOrientation = gizmoSpace.LocalOrientation;
                        if ((!this.PlacingSmallGridOnLargeStatic && (this.CurrentGrid != null)) && !MySession.Static.CreativeToolsEnabled(Sync.MyId))
                        {
                            flag &= this.CurrentGrid.CanAddCube(gizmoSpace.m_addPos, new MyBlockOrientation(ref localOrientation), this.CurrentBlockDefinition, false);
                        }
                    }
                    this.UpdateGizmo(gizmoSpace, flag || this.FreezeGizmo, removePos || this.FreezeGizmo, draw);
                }
            }
        }

        private void UpdateNotificationBlockLimit()
        {
        }

        public void UpdateNotificationBlockNotAvailable(bool updateNotAvailableNotification)
        {
            if (MyFakes.ENABLE_NOTIFICATION_BLOCK_NOT_AVAILABLE)
            {
                if (!updateNotAvailableNotification)
                {
                    this.HideNotificationBlockNotAvailable();
                }
                else
                {
                    int num1;
                    bool flag = (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Spectator) && false;
                    if ((MySession.Static.ControlledEntity == null) || !(MySession.Static.ControlledEntity is MyCockpit))
                    {
                        num1 = 0;
                    }
                    else
                    {
                        num1 = (int) !flag;
                    }
                    bool flag2 = (bool) num1;
                    if (this.BlockCreationIsActivated && (this.CurrentBlockDefinition != null))
                    {
                        if (((this.CurrentGrid != null) && ((this.CurrentBlockDefinition.CubeSize != this.CurrentGrid.GridSizeEnum) && !flag2)) && !this.PlacingSmallGridOnLargeStatic)
                        {
                            MyStringId id = (this.CurrentGrid.GridSizeEnum == MyCubeSize.Small) ? MySpaceTexts.NotificationArgLargeShip : MySpaceTexts.NotificationArgSmallShip;
                            MyStringId id2 = (this.CurrentGrid.GridSizeEnum == MyCubeSize.Small) ? MySpaceTexts.NotificationArgSmallShip : (this.CurrentGrid.IsStatic ? MySpaceTexts.NotificationArgStation : MySpaceTexts.NotificationArgLargeShip);
                            this.ShowNotificationBlockNotAvailable(id, this.CurrentBlockDefinition.DisplayNameText, id2);
                        }
                        else if ((this.BlockCreationIsActivated && (this.CurrentBlockDefinition != null)) && (this.CurrentGrid == null))
                        {
                            MyStringId id3 = (this.CurrentBlockDefinition.CubeSize == MyCubeSize.Small) ? MySpaceTexts.NotificationArgSmallShip : MySpaceTexts.NotificationArgLargeShip;
                            MyStringId id4 = (this.CurrentBlockDefinition.CubeSize == MyCubeSize.Small) ? MySpaceTexts.NotificationArgLargeShip : MySpaceTexts.NotificationArgSmallShip;
                            this.ShowNotificationBlockNotAvailable(id3, this.CurrentBlockDefinition.DisplayNameText, id4);
                        }
                    }
                }
            }
        }

        private void UpdatePlacementNotificationState()
        {
            this.m_cubePlacementModeNotification.m_lifespanMs = MySandboxGame.Config.ControlsHints ? 0 : 0x9c4;
        }

        private void UpdateSymmetryNotification(MyStringId myTextsWrapperEnum)
        {
            this.RemoveSymmetryNotification();
            this.m_symmetryNotification = new MyHudNotification(myTextsWrapperEnum, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Control);
            if (!MyInput.Static.IsJoystickConnected() || !MyInput.Static.IsJoystickLastUsed)
            {
                object[] arguments = new object[] { MyInput.Static.GetGameControl(MyControlsSpace.PRIMARY_TOOL_ACTION), MyInput.Static.GetGameControl(MyControlsSpace.SECONDARY_TOOL_ACTION) };
                this.m_symmetryNotification.SetTextFormatArguments(arguments);
            }
            else
            {
                object[] arguments = new object[] { MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_BUILD_MODE, MyControlsSpace.PRIMARY_TOOL_ACTION), MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_BUILD_MODE, MyControlsSpace.SECONDARY_BUILD_ACTION) };
                this.m_symmetryNotification.SetTextFormatArguments(arguments);
            }
            MyHud.Notifications.Add(this.m_symmetryNotification);
        }

        void IMyCubeBuilder.Activate(MyDefinitionId? blockDefinitionId = new MyDefinitionId?())
        {
            this.Activate(blockDefinitionId);
        }

        bool IMyCubeBuilder.AddConstruction(IMyEntity buildingEntity) => 
            false;

        void IMyCubeBuilder.Deactivate()
        {
            this.Deactivate();
        }

        void IMyCubeBuilder.DeactivateBlockCreation()
        {
            this.DeactivateBlockCreation();
        }

        IMyCubeGrid IMyCubeBuilder.FindClosestGrid() => 
            base.FindClosestGrid();

        void IMyCubeBuilder.StartNewGridPlacement(MyCubeSize cubeSize, bool isStatic)
        {
            this.StartStaticGridPlacement(cubeSize, isStatic);
        }

        bool IMyCubeBuilder.BlockCreationIsActivated =>
            this.BlockCreationIsActivated;

        bool IMyCubeBuilder.FreezeGizmo
        {
            get => 
                this.FreezeGizmo;
            set => 
                (this.FreezeGizmo = value);
        }

        bool IMyCubeBuilder.ShowRemoveGizmo
        {
            get => 
                this.ShowRemoveGizmo;
            set => 
                (this.ShowRemoveGizmo = value);
        }

        bool IMyCubeBuilder.UseSymmetry
        {
            get => 
                this.UseSymmetry;
            set => 
                (this.UseSymmetry = value);
        }

        bool IMyCubeBuilder.UseTransparency
        {
            get => 
                this.UseTransparency;
            set => 
                (this.UseTransparency = value);
        }

        bool IMyCubeBuilder.IsActivated =>
            this.IsActivated;

        public override System.Type[] Dependencies
        {
            get
            {
                System.Type[] typeArray = new System.Type[base.Dependencies.Length + 1];
                for (int i = 0; i < base.Dependencies.Length; i++)
                {
                    typeArray[i] = base.Dependencies[i];
                }
                typeArray[typeArray.Length - 1] = typeof(MyToolbarComponent);
                return typeArray;
            }
        }

        public static MyBuildComponentBase BuildComponent
        {
            [CompilerGenerated]
            get => 
                <BuildComponent>k__BackingField;
            [CompilerGenerated]
            set => 
                (<BuildComponent>k__BackingField = value);
        }

        public bool CompoundEnabled { get; protected set; }

        public bool BlockCreationIsActivated
        {
            get => 
                this.m_blockCreationActivated;
            private set => 
                (this.m_blockCreationActivated = value);
        }

        public override bool IsActivated =>
            this.BlockCreationIsActivated;

        public bool UseSymmetry
        {
            get
            {
                if ((!this.m_useSymmetry || (MySession.Static == null)) || (!MySession.Static.CreativeMode && !MySession.Static.CreativeToolsEnabled(Sync.MyId)))
                {
                    return false;
                }
                return !(MySession.Static.ControlledEntity is MyShipController);
            }
            set
            {
                if (this.m_useSymmetry != value)
                {
                    this.m_useSymmetry = value;
                    MySandboxGame.Config.CubeBuilderUseSymmetry = value;
                    MySandboxGame.Config.Save();
                }
            }
        }

        public bool UseTransparency
        {
            get => 
                this.m_useTransparency;
            set
            {
                if (this.m_useTransparency != value)
                {
                    this.m_useTransparency = value;
                    this.m_renderData.BeginCollectingInstanceData();
                    this.m_rotationHints.Clear();
                    MatrixD gridWorldMatrix = (this.CurrentGrid != null) ? this.CurrentGrid.WorldMatrix : MatrixD.Identity;
                    this.m_renderData.EndCollectingInstanceData(gridWorldMatrix, this.UseTransparency);
                }
            }
        }

        public bool AlignToDefault
        {
            get => 
                this.m_alignToDefault;
            set
            {
                if (this.m_alignToDefault != value)
                {
                    this.m_alignToDefault = value;
                    MySandboxGame.Config.CubeBuilderAlignToDefault = value;
                    MySandboxGame.Config.Save();
                }
            }
        }

        public bool FreezeGizmo { get; set; }

        public bool ShowRemoveGizmo { get; set; }

        public MyCubeBuilderState CubeBuilderState =>
            this.m_cubeBuilderState;

        protected internal override MyCubeGrid CurrentGrid
        {
            get => 
                base.m_currentGrid;
            protected set
            {
                if (!this.FreezeGizmo && !ReferenceEquals(base.m_currentGrid, value))
                {
                    this.BeforeCurrentGridChange(value);
                    base.m_currentGrid = value;
                    this.m_customRotation = false;
                    if ((this.IsCubeSizeModesAvailable && (this.CurrentBlockDefinition != null)) && (base.m_currentGrid != null))
                    {
                        MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(this.CurrentBlockDefinition.BlockPairName);
                        int index = this.m_cubeBuilderState.CurrentBlockDefinitionStages.IndexOf(this.CurrentBlockDefinition);
                        MyCubeSize gridSizeEnum = base.m_currentGrid.GridSizeEnum;
                        if ((gridSizeEnum != this.CurrentBlockDefinition.CubeSize) && (((gridSizeEnum == MyCubeSize.Small) && (definitionGroup.Small != null)) || ((gridSizeEnum == MyCubeSize.Large) && (definitionGroup.Large != null))))
                        {
                            this.m_cubeBuilderState.SetCubeSize(gridSizeEnum);
                            this.SetSurvivalIntersectionDist();
                            if ((gridSizeEnum == MyCubeSize.Small) && (this.CubePlacementMode == CubePlacementModeEnum.LocalCoordinateSystem))
                            {
                                this.CycleCubePlacementMode();
                            }
                            if ((index != -1) && (index < this.m_cubeBuilderState.CurrentBlockDefinitionStages.Count))
                            {
                                this.UpdateCubeBlockStageDefinition(this.m_cubeBuilderState.CurrentBlockDefinitionStages[index]);
                            }
                        }
                    }
                    if (base.m_currentGrid == null)
                    {
                        this.RemoveSymmetryNotification();
                        this.m_gizmo.Clear();
                    }
                }
            }
        }

        protected internal override MyVoxelBase CurrentVoxelBase
        {
            get => 
                base.m_currentVoxelBase;
            protected set
            {
                if (!this.FreezeGizmo && !ReferenceEquals(base.m_currentVoxelBase, value))
                {
                    base.m_currentVoxelBase = value;
                    if (base.m_currentVoxelBase == null)
                    {
                        this.RemoveSymmetryNotification();
                        this.m_gizmo.Clear();
                    }
                }
            }
        }

        protected override MyCubeBlockDefinition CurrentBlockDefinition
        {
            get => 
                this.m_cubeBuilderState?.CurrentBlockDefinition;
            set
            {
                if (this.m_cubeBuilderState != null)
                {
                    this.m_cubeBuilderState.CurrentBlockDefinition = value;
                }
            }
        }

        public MyCubeBlockDefinition ToolbarBlockDefinition
        {
            get
            {
                if (this.m_cubeBuilderState == null)
                {
                    return null;
                }
                if (!MyFakes.ENABLE_BLOCK_STAGES || (this.m_cubeBuilderState.CurrentBlockDefinitionStages.Count <= 0))
                {
                    return this.CurrentBlockDefinition;
                }
                return this.m_cubeBuilderState.CurrentBlockDefinitionStages[0];
            }
        }

        public static BuildingModeEnum BuildingMode
        {
            get
            {
                int cubeBuilderBuildingMode = MySandboxGame.Config.CubeBuilderBuildingMode;
                if (!System.Enum.IsDefined(typeof(BuildingModeEnum), cubeBuilderBuildingMode))
                {
                    cubeBuilderBuildingMode = 0;
                }
                return (BuildingModeEnum) cubeBuilderBuildingMode;
            }
            set => 
                (MySandboxGame.Config.CubeBuilderBuildingMode = (int) value);
        }

        public virtual bool IsCubeSizeModesAvailable =>
            true;

        public bool IsBuildMode
        {
            get => 
                this.m_isBuildMode;
            set
            {
                this.m_isBuildMode = value;
                MyHud.IsBuildMode = value;
                if (!value)
                {
                    this.DeactivateBuildModeNotifications();
                }
                else
                {
                    int num1;
                    if (!MyInput.Static.IsJoystickConnected() || !MyInput.Static.IsJoystickLastUsed)
                    {
                        num1 = 0;
                    }
                    else
                    {
                        num1 = (int) MyFakes.ENABLE_CONTROLLER_HINTS;
                    }
                    this.ActivateBuildModeNotifications((bool) num1);
                }
            }
        }

        public CubePlacementModeEnum CubePlacementMode
        {
            get => 
                this.m_cubePlacementMode;
            set
            {
                if (this.m_cubePlacementMode != value)
                {
                    this.m_cubePlacementMode = value;
                    this.ShowCubePlacementNotification();
                }
            }
        }

        public bool DynamicMode { get; protected set; }

        protected bool GridValid =>
            (this.BlockCreationIsActivated && (this.CurrentGrid != null));

        protected bool GridAndBlockValid =>
            (this.GridValid && ((this.CurrentBlockDefinition != null) && ((this.CurrentBlockDefinition.CubeSize == this.CurrentGrid.GridSizeEnum) || this.PlacingSmallGridOnLargeStatic)));

        protected bool VoxelMapAndBlockValid =>
            (this.BlockCreationIsActivated && ((this.CurrentVoxelBase != null) && (this.CurrentBlockDefinition != null)));

        public bool PlacingSmallGridOnLargeStatic =>
            (MyFakes.ENABLE_STATIC_SMALL_GRID_ON_LARGE && (this.GridValid && ((this.CurrentBlockDefinition != null) && ((this.CurrentBlockDefinition.CubeSize == MyCubeSize.Small) && ((this.CurrentGrid.GridSizeEnum == MyCubeSize.Large) && this.CurrentGrid.IsStatic)))));

        protected bool BuildInputValid =>
            (this.GridAndBlockValid || (this.VoxelMapAndBlockValid || (this.DynamicMode || (this.CurrentBlockDefinition != null))));

        private float CurrentBlockScale =>
            ((this.CurrentBlockDefinition != null) ? (MyDefinitionManager.Static.GetCubeSize(this.CurrentBlockDefinition.CubeSize) / MyDefinitionManager.Static.GetCubeSizeOriginal(this.CurrentBlockDefinition.CubeSize)) : 1f);

        private bool IsInSymmetrySettingMode =>
            (this.m_symmetrySettingMode != MySymmetrySettingModeEnum.NoPlane);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCubeBuilder.<>c <>9 = new MyCubeBuilder.<>c();
            public static Func<string, string> <>9__16_0;
            public static Func<string, string> <>9__17_0;
            public static Func<IMyEventOwner, Action<MyCubeBuilder.Author, DefinitionIdBlit, MyCubeBuilder.BuildData, bool, bool, uint>> <>9__226_1;
            public static Func<IMyEventOwner, Action<MyCubeBuilder.Author, DefinitionIdBlit, MyCubeBuilder.BuildData, bool, bool, uint>> <>9__226_0;
            public static Func<IMyEventOwner, Action<bool>> <>9__287_0;

            internal Action<MyCubeBuilder.Author, DefinitionIdBlit, MyCubeBuilder.BuildData, bool, bool, uint> <AddBlocksToBuildQueueOrSpawn>b__226_0(IMyEventOwner s) => 
                new Action<MyCubeBuilder.Author, DefinitionIdBlit, MyCubeBuilder.BuildData, bool, bool, uint>(MyCubeBuilder.RequestGridSpawn);

            internal Action<MyCubeBuilder.Author, DefinitionIdBlit, MyCubeBuilder.BuildData, bool, bool, uint> <AddBlocksToBuildQueueOrSpawn>b__226_1(IMyEventOwner s) => 
                new Action<MyCubeBuilder.Author, DefinitionIdBlit, MyCubeBuilder.BuildData, bool, bool, uint>(MyCubeBuilder.RequestGridSpawn);

            internal string <DebugDrawBareBlockInfo>b__17_0(string s) => 
                s;

            internal string <DebugDrawTexturesInfo>b__16_0(string s) => 
                s;

            internal Action<bool> <SpawnGridReply>b__287_0(IMyEventOwner s) => 
                new Action<bool>(MyCubeBuilder.SpawnGridReply);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Author
        {
            public long EntityId;
            public long IdentityId;
            public Author(long entityId, long identityId)
            {
                this.EntityId = entityId;
                this.IdentityId = identityId;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BuildData
        {
            public Vector3D Position;
            public Vector3 Forward;
            public Vector3 Up;
            public bool AbsolutePosition;
        }

        public enum BuildingModeEnum
        {
            SingleBlock,
            Line,
            Plane
        }

        public enum CubePlacementModeEnum
        {
            LocalCoordinateSystem,
            FreePlacement,
            GravityAligned
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyColoringArea
        {
            public Vector3I Start;
            public Vector3I End;
        }

        [Flags]
        public enum SpawnFlags : ushort
        {
            None = 0,
            AddToScene = 1,
            CreatePhysics = 2,
            EnableSmallTolargeConnections = 4,
            SpawnAsMaster = 8,
            Default = 7
        }
    }
}

