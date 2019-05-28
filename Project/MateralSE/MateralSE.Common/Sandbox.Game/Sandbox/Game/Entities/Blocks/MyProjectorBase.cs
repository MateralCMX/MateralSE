namespace Sandbox.Game.Entities.Blocks
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents.Clipboard;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ObjectBuilders.Definitions.SessionComponents;
    using VRage.Game.Utils;
    using VRage.Generics;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Profiler;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyProjectorBase : MyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider, IMyMultiTextPanelComponentOwner, IMyTextPanelComponentOwner, Sandbox.ModAPI.IMyProjector, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyProjector, Sandbox.ModAPI.IMyTextSurfaceProvider
    {
        private const int PROJECTION_UPDATE_TIME = 0x7d0;
        protected const int OFFSET_LIMIT = 50;
        protected const int ROTATION_LIMIT = 2;
        protected const float SCALE_LIMIT = 0.02f;
        protected const int MAX_SCALED_DRAW_DISTANCE = 50;
        protected const int MAX_SCALED_DRAW_DISTANCE_SQUARED = 0x9c4;
        private int m_lastUpdate;
        private readonly MyProjectorClipboard m_clipboard;
        private readonly MyProjectorClipboard m_spawnClipboard;
        protected Vector3I m_projectionOffset;
        protected Vector3I m_projectionRotation;
        protected float m_projectionScale = 1f;
        private MySlimBlock m_hiddenBlock;
        private bool m_shouldUpdateProjection;
        private bool m_forceUpdateProjection;
        private bool m_shouldUpdateTexts;
        private bool m_shouldResetBuildable;
        private MyObjectBuilder_CubeGrid m_savedProjection;
        protected bool m_showOnlyBuildable;
        private int m_frameCount;
        private bool m_removeRequested;
        private Task m_updateTask;
        private MyObjectBuilder_CubeGrid m_originalGridBuilder;
        protected const int MAX_NUMBER_OF_PROJECTIONS = 0x3e8;
        protected const int MAX_NUMBER_OF_BLOCKS = 0x2710;
        private int m_projectionsRemaining;
        private MyMultiTextPanelComponent m_multiPanel;
        private MyGuiScreenTextPanel m_textBox;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_keepProjection;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_instantBuildingEnabled;
        private readonly VRage.Sync.Sync<int, SyncDirection.BothWays> m_maxNumberOfProjections;
        private readonly VRage.Sync.Sync<int, SyncDirection.BothWays> m_maxNumberOfBlocksPerProjection;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_getOwnershipFromProjector;
        private bool m_isTextPanelOpen;
        private HashSet<MySlimBlock> m_visibleBlocks = new HashSet<MySlimBlock>();
        private HashSet<MySlimBlock> m_buildableBlocks = new HashSet<MySlimBlock>();
        private HashSet<MySlimBlock> m_hiddenBlocks = new HashSet<MySlimBlock>();
        private int m_remainingBlocks;
        private int m_totalBlocks;
        private readonly Dictionary<MyCubeBlockDefinition, int> m_remainingBlocksPerType = new Dictionary<MyCubeBlockDefinition, int>();
        private int m_remainingArmorBlocks;
        private int m_buildableBlocksCount;
        private bool m_statsDirty;

        public MyProjectorBase()
        {
            this.m_clipboard = new MyProjectorClipboard(this, MyClipboardComponent.ClipboardDefinition.PastingSettings);
            this.m_spawnClipboard = new MyProjectorClipboard(this, MyClipboardComponent.ClipboardDefinition.PastingSettings);
            this.m_instantBuildingEnabled.ValueChanged += new Action<SyncBase>(this.m_instantBuildingEnabled_ValueChanged);
            this.m_maxNumberOfProjections.ValueChanged += new Action<SyncBase>(this.m_maxNumberOfProjections_ValueChanged);
            this.m_maxNumberOfBlocksPerProjection.ValueChanged += new Action<SyncBase>(this.m_maxNumberOfBlocksPerProjection_ValueChanged);
            this.m_getOwnershipFromProjector.ValueChanged += new Action<SyncBase>(this.m_getOwnershipFromProjector_ValueChanged);
            this.Render = new MyRenderComponentScreenAreas(this);
        }

        public void Build(MySlimBlock cubeBlock, long owner, long builder, bool requestInstant = true, long builtBy = 0L)
        {
            ulong steamId = MySession.Static.Players.TryGetSteamId(owner);
            if (this.AllowWelding && MySession.Static.GetComponent<MySessionComponentDLC>().HasDefinitionDLC(cubeBlock.BlockDefinition, steamId))
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase, Vector3I, long, long, bool, long>(this, x => new Action<Vector3I, long, long, bool, long>(x.BuildInternal), cubeBlock.Position, owner, builder, requestInstant, builtBy, targetEndpoint);
            }
        }

        [Event(null, 0x5ef), Reliable, Server]
        private void BuildInternal(Vector3I cubeBlockPosition, long owner, long builder, bool requestInstant = true, long builtBy = 0L)
        {
            ulong steamId = MySession.Static.Players.TryGetSteamId(owner);
            MySlimBlock cubeBlock = this.ProjectedGrid.GetCubeBlock(cubeBlockPosition);
            if (((cubeBlock == null) || !this.AllowWelding) || !MySession.Static.GetComponent<MySessionComponentDLC>().HasDefinitionDLC(cubeBlock.BlockDefinition, steamId))
            {
                (Sandbox.Engine.Multiplayer.MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, false, null, true);
            }
            else
            {
                Quaternion identity = Quaternion.Identity;
                MyBlockOrientation orientation = cubeBlock.Orientation;
                Quaternion result = Quaternion.Identity;
                base.Orientation.GetQuaternion(out result);
                orientation.GetQuaternion(out identity);
                identity = Quaternion.Multiply(result, Quaternion.Multiply(this.ProjectionRotationQuaternion, identity));
                MyCubeGrid cubeGrid = base.CubeGrid;
                MyCubeGrid grid2 = cubeBlock.CubeGrid;
                Vector3I gridCoords = (cubeBlock.FatBlock != null) ? cubeBlock.FatBlock.Min : cubeBlock.Position;
                Vector3I vectori3 = cubeGrid.WorldToGridInteger(grid2.GridIntegerToWorld(gridCoords));
                Vector3I vectori4 = cubeGrid.WorldToGridInteger(grid2.GridIntegerToWorld((cubeBlock.FatBlock != null) ? cubeBlock.FatBlock.Max : cubeBlock.Position));
                Vector3I min = new Vector3I(Math.Min(vectori3.X, vectori4.X), Math.Min(vectori3.Y, vectori4.Y), Math.Min(vectori3.Z, vectori4.Z));
                Vector3I max = new Vector3I(Math.Max(vectori3.X, vectori4.X), Math.Max(vectori3.Y, vectori4.Y), Math.Max(vectori3.Z, vectori4.Z));
                MyCubeGrid.MyBlockLocation location = new MyCubeGrid.MyBlockLocation(cubeBlock.BlockDefinition.Id, min, max, cubeGrid.WorldToGridInteger(grid2.GridIntegerToWorld(cubeBlock.Position)), identity, 0L, owner);
                MyObjectBuilder_CubeBlock objectBuilder = null;
                if (this.m_originalGridBuilder != null)
                {
                    foreach (MyObjectBuilder_CubeBlock block3 in this.m_originalGridBuilder.CubeBlocks)
                    {
                        if (block3.Min != gridCoords)
                        {
                            continue;
                        }
                        if (block3.GetId() == cubeBlock.BlockDefinition.Id)
                        {
                            objectBuilder = (MyObjectBuilder_CubeBlock) block3.Clone();
                            objectBuilder.SetupForProjector();
                            if ((MyDefinitionManagerBase.Static != null) && (objectBuilder is MyObjectBuilder_BatteryBlock))
                            {
                                MyBatteryBlockDefinition cubeBlockDefinition = (MyBatteryBlockDefinition) MyDefinitionManager.Static.GetCubeBlockDefinition(objectBuilder);
                                ((MyObjectBuilder_BatteryBlock) objectBuilder).CurrentStoredPower = cubeBlockDefinition.InitialStoredPowerRatio * cubeBlockDefinition.MaxStoredPower;
                            }
                        }
                    }
                }
                if (objectBuilder == null)
                {
                    objectBuilder = cubeBlock.GetObjectBuilder(false);
                    location.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                }
                objectBuilder.ConstructionInventory = null;
                objectBuilder.BuiltBy = builtBy;
                bool flag = requestInstant && MySession.Static.CreativeToolsEnabled(MyEventContext.Current.Sender.Value);
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCubeGrid, uint, MyCubeGrid.MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>(cubeGrid, x => new Action<uint, MyCubeGrid.MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>(x.BuildBlockRequest), cubeBlock.ColorMaskHSV.PackHSVToUint(), location, objectBuilder, builder, flag, owner, targetEndpoint);
                this.HideCube(cubeBlock);
            }
        }

        private float CalculateRequiredPowerInput() => 
            this.BlockDefinition.RequiredPowerInput;

        private bool CanBuild(MySlimBlock cubeBlock) => 
            (this.CanBuild(cubeBlock, false) == BuildCheckResult.OK);

        public BuildCheckResult CanBuild(MySlimBlock projectedBlock, bool checkHavokIntersections)
        {
            Quaternion quaternion;
            if (!this.AllowWelding)
            {
                return BuildCheckResult.NotWeldable;
            }
            MyBlockOrientation blockOrientation = projectedBlock.Orientation;
            blockOrientation.GetQuaternion(out quaternion);
            Quaternion identity = Quaternion.Identity;
            base.Orientation.GetQuaternion(out identity);
            quaternion = Quaternion.Multiply(Quaternion.Multiply(identity, this.ProjectionRotationQuaternion), quaternion);
            Vector3I min = base.CubeGrid.WorldToGridInteger(projectedBlock.CubeGrid.GridIntegerToWorld(projectedBlock.Min));
            Vector3I max = base.CubeGrid.WorldToGridInteger(projectedBlock.CubeGrid.GridIntegerToWorld(projectedBlock.Max));
            Vector3I position = base.CubeGrid.WorldToGridInteger(projectedBlock.CubeGrid.GridIntegerToWorld(projectedBlock.Position));
            min = new Vector3I(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y), Math.Min(min.Z, max.Z));
            max = new Vector3I(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y), Math.Max(min.Z, max.Z));
            if (!base.CubeGrid.CanAddCubes(min, max))
            {
                return BuildCheckResult.IntersectedWithGrid;
            }
            MyGridPlacementSettings settings = new MyGridPlacementSettings {
                SnapMode = SnapMode.OneFreeAxis
            };
            MyCubeBlockDefinition.MountPoint[] buildProgressModelMountPoints = projectedBlock.BlockDefinition.GetBuildProgressModelMountPoints(1f);
            return (!MyCubeGrid.CheckConnectivity(base.CubeGrid, projectedBlock.BlockDefinition, buildProgressModelMountPoints, ref quaternion, ref position) ? BuildCheckResult.NotConnected : ((base.CubeGrid.GetCubeBlock(position) != null) ? BuildCheckResult.AlreadyBuilt : (!checkHavokIntersections ? BuildCheckResult.OK : (!MyCubeGrid.TestPlacementAreaCube(base.CubeGrid, ref settings, min, max, blockOrientation, projectedBlock.BlockDefinition, base.CubeGrid, false) ? BuildCheckResult.IntersectedWithSomethingElse : BuildCheckResult.OK))));
        }

        protected bool CanEditInstantBuildingSettings() => 
            (this.CanEnableInstantBuilding() && ((bool) this.m_instantBuildingEnabled));

        protected bool CanEnableInstantBuilding() => 
            MySession.Static.Settings.ScenarioEditMode;

        protected bool CanProject()
        {
            base.UpdateIsWorking();
            this.UpdateText();
            return base.IsWorking;
        }

        protected bool CanSpawnProjection()
        {
            if (this.m_instantBuildingEnabled != null)
            {
                if (this.ProjectedGrid == null)
                {
                    return false;
                }
                if ((this.m_maxNumberOfBlocksPerProjection >= 0x2710) || (this.m_maxNumberOfBlocksPerProjection >= this.ProjectedGrid.CubeBlocks.Count))
                {
                    return ((this.m_projectionsRemaining != 0) ? this.ScenarioSettingsEnabled() : false);
                }
            }
            return false;
        }

        private void CloseWindow(bool isPublic)
        {
            MyGuiScreenGamePlay.ActiveGameplayScreen = MyGuiScreenGamePlay.TmpGameplayScreenHolder;
            MyGuiScreenGamePlay.TmpGameplayScreenHolder = null;
            foreach (MySlimBlock block in base.CubeGrid.CubeBlocks)
            {
                if ((block.FatBlock != null) && (block.FatBlock.EntityId == base.EntityId))
                {
                    this.SendChangeDescriptionMessage(this.m_textBox.Description.Text, isPublic);
                    this.SendChangeOpenMessage(false, false, 0UL, false);
                    break;
                }
            }
        }

        protected override void Closing()
        {
            base.Closing();
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.SetRender(null);
            }
            base.CubeGrid.OnBlockAdded -= new Action<MySlimBlock>(this.previewGrid_OnBlockAdded);
            base.CubeGrid.OnBlockRemoved -= new Action<MySlimBlock>(this.previewGrid_OnBlockRemoved);
            base.CubeGrid.OnGridSplit -= new Action<MyCubeGrid, MyCubeGrid>(this.CubeGrid_OnGridSplit);
            if (this.m_clipboard.IsActive)
            {
                this.RemoveProjection(false);
            }
            foreach (MyCubeBlock block in base.CubeGrid.GetFatBlocks())
            {
                if (block is Sandbox.ModAPI.IMyTerminalBlock)
                {
                    block.CheckConnectionChanged -= new Action<MyCubeBlock>(this.TerminalBlockOnCheckConnectionChanged);
                }
            }
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        private void CreateTextBox(bool isEditable, StringBuilder description, bool isPublic)
        {
            bool editable = isEditable;
            this.m_textBox = new MyGuiScreenTextPanel(this.DisplayNameText, "", this.PanelComponent.DisplayName, description.ToString(), new Action<VRage.Game.ModAPI.ResultEnum>(this.OnClosedPanelTextBox), null, null, editable, null);
        }

        private void CubeGrid_OnGridSplit(MyCubeGrid grid1, MyCubeGrid grid2)
        {
            if (((this.m_originalGridBuilder != null) && (Sync.IsServer && !base.MarkedForClose)) && !base.Closed)
            {
                this.Remap();
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_ProjectorBase objectBuilderCubeBlock = (MyObjectBuilder_ProjectorBase) base.GetObjectBuilderCubeBlock(copy);
            if (((this.m_clipboard == null) || ((this.m_clipboard.CopiedGrids == null) || (this.m_clipboard.CopiedGrids.Count <= 0))) || (this.m_originalGridBuilder == null))
            {
                if (((objectBuilderCubeBlock.ProjectedGrid != null) || (this.m_savedProjection == null)) || (base.CubeGrid.Projector != null))
                {
                    objectBuilderCubeBlock.ProjectedGrid = null;
                }
                else
                {
                    objectBuilderCubeBlock.ProjectedGrid = this.m_savedProjection;
                    objectBuilderCubeBlock.ProjectionOffset = this.m_projectionOffset;
                    objectBuilderCubeBlock.ProjectionRotation = this.m_projectionRotation;
                    objectBuilderCubeBlock.KeepProjection = (bool) this.m_keepProjection;
                }
            }
            else
            {
                if (!copy)
                {
                    objectBuilderCubeBlock.ProjectedGrid = this.m_originalGridBuilder;
                }
                else
                {
                    MyObjectBuilder_CubeGrid objectBuilder = (MyObjectBuilder_CubeGrid) this.m_originalGridBuilder.Clone();
                    Sandbox.Game.Entities.MyEntities.RemapObjectBuilder(objectBuilder);
                    objectBuilderCubeBlock.ProjectedGrid = objectBuilder;
                }
                objectBuilderCubeBlock.ProjectionOffset = this.m_projectionOffset;
                objectBuilderCubeBlock.ProjectionRotation = this.m_projectionRotation;
                objectBuilderCubeBlock.KeepProjection = (bool) this.m_keepProjection;
                objectBuilderCubeBlock.Scale = this.m_projectionScale;
            }
            objectBuilderCubeBlock.ShowOnlyBuildable = this.m_showOnlyBuildable;
            objectBuilderCubeBlock.InstantBuildingEnabled = (bool) this.m_instantBuildingEnabled;
            objectBuilderCubeBlock.MaxNumberOfProjections = (int) this.m_maxNumberOfProjections;
            objectBuilderCubeBlock.MaxNumberOfBlocks = (int) this.m_maxNumberOfBlocksPerProjection;
            objectBuilderCubeBlock.ProjectionsRemaining = this.m_projectionsRemaining;
            objectBuilderCubeBlock.GetOwnershipFromProjector = (bool) this.m_getOwnershipFromProjector;
            objectBuilderCubeBlock.TextPanels = this.m_multiPanel?.Serialize();
            return objectBuilderCubeBlock;
        }

        public Vector3 GetProjectionTranslationOffset() => 
            ((Vector3) ((this.m_projectionOffset * this.m_clipboard.GridSize) * this.Scale));

        public void HideCube(MySlimBlock cubeBlock)
        {
            this.SetTransparency(cubeBlock, 1f);
        }

        private void HideIntersectedBlock()
        {
            if (this.m_instantBuildingEnabled == null)
            {
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
                if (localCharacter != null)
                {
                    Vector3D translation = localCharacter.GetHeadMatrix(true, true, false, false, false).Translation;
                    if (this.ProjectedGrid != null)
                    {
                        Vector3I pos = this.ProjectedGrid.WorldToGridInteger(translation);
                        MySlimBlock cubeBlock = this.ProjectedGrid.GetCubeBlock(pos);
                        if (cubeBlock == null)
                        {
                            if (this.m_hiddenBlock != null)
                            {
                                this.ShowCube(this.m_hiddenBlock, this.CanBuild(this.m_hiddenBlock));
                                this.m_hiddenBlock = null;
                            }
                        }
                        else if ((Math.Abs(cubeBlock.Dithering) < 1f) && !ReferenceEquals(this.m_hiddenBlock, cubeBlock))
                        {
                            if (this.m_hiddenBlock != null)
                            {
                                this.ShowCube(this.m_hiddenBlock, this.CanBuild(this.m_hiddenBlock));
                            }
                            this.HideCube(cubeBlock);
                            this.m_hiddenBlock = cubeBlock;
                        }
                    }
                }
            }
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(this.BlockDefinition.ResourceSinkGroup, this.BlockDefinition.RequiredPowerInput, new Func<float>(this.CalculateRequiredPowerInput));
            base.ResourceSink = component;
            base.Init(objectBuilder, cubeGrid);
            if (MyFakes.ENABLE_PROJECTOR_BLOCK)
            {
                MyObjectBuilder_ProjectorBase base2 = (MyObjectBuilder_ProjectorBase) objectBuilder;
                if (base2.ProjectedGrid != null)
                {
                    this.m_projectionOffset = Vector3I.Clamp(base2.ProjectionOffset, new Vector3I(-50), new Vector3I(50));
                    this.m_projectionRotation = Vector3I.Clamp(base2.ProjectionRotation, new Vector3I(-2), new Vector3I(2));
                    this.m_projectionScale = base2.Scale;
                    this.m_savedProjection = base2.ProjectedGrid;
                    this.m_keepProjection.SetLocalValue(base2.KeepProjection);
                }
                this.m_showOnlyBuildable = base2.ShowOnlyBuildable;
                this.m_instantBuildingEnabled.SetLocalValue(base2.InstantBuildingEnabled);
                this.m_maxNumberOfProjections.SetLocalValue(MathHelper.Clamp(base2.MaxNumberOfProjections, 0, 0x3e8));
                this.m_maxNumberOfBlocksPerProjection.SetLocalValue(MathHelper.Clamp(base2.MaxNumberOfBlocks, 0, 0x2710));
                this.m_getOwnershipFromProjector.SetLocalValue(base2.GetOwnershipFromProjector);
                this.m_projectionsRemaining = MathHelper.Clamp(base2.ProjectionsRemaining, 0, (int) this.m_maxNumberOfProjections);
                base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyProjector_IsWorkingChanged);
                component.IsPoweredChanged += new Action(this.PowerReceiver_IsPoweredChanged);
                base.ResourceSink.Update();
                this.m_statsDirty = true;
                this.UpdateText();
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
                base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
                base.CubeGrid.OnBlockAdded += new Action<MySlimBlock>(this.previewGrid_OnBlockAdded);
                base.CubeGrid.OnBlockRemoved += new Action<MySlimBlock>(this.previewGrid_OnBlockRemoved);
                base.CubeGrid.OnGridSplit += new Action<MyCubeGrid, MyCubeGrid>(this.CubeGrid_OnGridSplit);
                base.EnabledChanged += new Action<MyTerminalBlock>(this.OnEnabledChanged);
                foreach (MyCubeBlock block in base.CubeGrid.GetFatBlocks())
                {
                    if (block is Sandbox.ModAPI.IMyTerminalBlock)
                    {
                        block.CheckConnectionChanged += new Action<MyCubeBlock>(this.TerminalBlockOnCheckConnectionChanged);
                    }
                }
                if ((this.BlockDefinition.ScreenAreas != null) && (this.BlockDefinition.ScreenAreas.Count > 0))
                {
                    base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
                    this.m_multiPanel = new MyMultiTextPanelComponent(this, this.BlockDefinition.ScreenAreas, base2.TextPanels);
                    this.m_multiPanel.Init(new Action<int, int[]>(this.SendAddImagesToSelectionRequest), new Action<int, int[]>(this.SendRemoveSelectedImageRequest));
                }
            }
        }

        private void InitializeClipboard()
        {
            this.m_clipboard.ResetGridOrientation();
            if (!this.m_clipboard.IsActive && !this.IsActivating)
            {
                int num = 0;
                foreach (MyObjectBuilder_CubeGrid grid in this.m_clipboard.CopiedGrids)
                {
                    num += grid.CubeBlocks.Count;
                }
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(base.BuiltBy);
                if (identity != null)
                {
                    int pcuToBuild = num - this.BlockDefinition.PCU;
                    if (!MySession.Static.CheckLimitsAndNotify(base.BuiltBy, this.BlockDefinition.BlockPairName, pcuToBuild, 0, 0, null))
                    {
                        return;
                    }
                    identity.BlockLimits.IncreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcuToBuild, base.CubeGrid, false);
                }
                this.IsActivating = true;
                this.m_clipboard.Activate(delegate {
                    if (this.m_clipboard.PreviewGrids.Count != 0)
                    {
                        this.ProjectedGrid.Projector = this;
                    }
                    this.m_forceUpdateProjection = true;
                    this.m_shouldUpdateTexts = true;
                    this.m_shouldResetBuildable = true;
                    this.m_clipboard.ActuallyTestPlacement();
                    this.SetRotation(this.m_clipboard, this.m_projectionRotation);
                    base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
                    this.IsActivating = false;
                });
            }
        }

        public bool IsInRange()
        {
            MyCamera mainCamera = MySector.MainCamera;
            return ((mainCamera != null) ? (Vector3D.DistanceSquared(base.PositionComp.WorldVolume.Center, mainCamera.Position) < 2500.0) : false);
        }

        protected bool IsProjecting() => 
            this.m_clipboard.IsActive;

        private bool LoadBlueprint(string path)
        {
            bool flag = false;
            MyObjectBuilder_Definitions prefab = MyBlueprintUtils.LoadPrefab(path);
            if (prefab != null)
            {
                flag = MyGuiBlueprintScreen.CopyBlueprintPrefabToClipboard(prefab, this.m_clipboard, true);
            }
            this.OnBlueprintScreen_Closed(null, null);
            return flag;
        }

        private void m_getOwnershipFromProjector_ValueChanged(SyncBase obj)
        {
            base.RaisePropertiesChanged();
        }

        private void m_instantBuildingEnabled_ValueChanged(SyncBase obj)
        {
            this.m_shouldUpdateProjection = true;
            if (this.m_instantBuildingEnabled != null)
            {
                this.m_projectionsRemaining = (int) this.m_maxNumberOfProjections;
            }
            base.RaisePropertiesChanged();
        }

        private void m_maxNumberOfBlocksPerProjection_ValueChanged(SyncBase obj)
        {
            base.RaisePropertiesChanged();
        }

        private void m_maxNumberOfProjections_ValueChanged(SyncBase obj)
        {
            this.m_projectionsRemaining = (int) this.m_maxNumberOfProjections;
            base.RaisePropertiesChanged();
        }

        private void MyProjector_IsWorkingChanged(MyCubeBlock obj)
        {
            if (!base.IsWorking && this.IsProjecting())
            {
                this.RequestRemoveProjection();
            }
            else
            {
                this.SetEmissiveStateWorking();
                if ((base.IsWorking && !this.IsProjecting()) && this.m_clipboard.HasCopiedGrids())
                {
                    this.InitializeClipboard();
                }
            }
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if ((this.m_multiPanel != null) && (this.m_multiPanel.SurfaceCount > 0))
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                int? renderObjectIndex = null;
                this.m_multiPanel.AddToScene(renderObjectIndex);
            }
        }

        private void OnBlueprintScreen_Closed(MyGuiScreenBase source, VRage.Game.Entity.MyEntity interactedEntity = null)
        {
            base.ResourceSink.Update();
            base.UpdateIsWorking();
            if ((this.m_clipboard.CopiedGrids.Count == 0) || !base.IsWorking)
            {
                this.RemoveProjection(false);
                this.ReopenTerminal(interactedEntity);
            }
            else
            {
                StringBuilder builder;
                MyStringId? nullable;
                Vector2? nullable2;
                if (!this.BlockDefinition.IgnoreSize && (this.m_clipboard.GridSize != base.CubeGrid.GridSize))
                {
                    this.RemoveProjection(false);
                    builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning);
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.NotificationProjectorGridSize), builder, nullable, nullable, nullable, nullable, result => this.ReopenTerminal(interactedEntity), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else
                {
                    if (this.m_clipboard.CopiedGrids.Count > 1)
                    {
                        builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.NotificationProjectorMultipleGrids), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                    int largestGridIndex = -1;
                    int num = -1;
                    for (int i = 0; i < this.m_clipboard.CopiedGrids.Count; i++)
                    {
                        int count = this.m_clipboard.CopiedGrids[i].CubeBlocks.Count;
                        if (count > num)
                        {
                            num = count;
                            largestGridIndex = i;
                        }
                    }
                    MyObjectBuilder_CubeGrid gridBuilder = null;
                    this.m_originalGridBuilder = null;
                    Parallel.Start(delegate {
                        gridBuilder = (MyObjectBuilder_CubeGrid) this.m_clipboard.CopiedGrids[largestGridIndex].Clone();
                        this.m_clipboard.ProcessCubeGrid(this.m_clipboard.CopiedGrids[largestGridIndex]);
                        Sandbox.Game.Entities.MyEntities.RemapObjectBuilder(gridBuilder);
                    }, delegate {
                        if ((gridBuilder != null) && (this.m_originalGridBuilder == null))
                        {
                            this.m_originalGridBuilder = gridBuilder;
                            this.SendNewBlueprint(this.m_originalGridBuilder);
                        }
                    });
                    this.ReopenTerminal(interactedEntity);
                }
            }
        }

        [Event(null, 0x243), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        public void OnChangeDescription(string description, bool isPublic)
        {
            StringBuilder builder = new StringBuilder();
            builder.Clear().Append(description);
            this.PanelComponent.Text = builder.ToString();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        private void OnChangeOpen(bool isOpen, bool editable, ulong user, bool isPublic)
        {
            this.IsTextPanelOpen = isOpen;
            if ((!Sandbox.Engine.Platform.Game.IsDedicated && (user == Sync.MyId)) & isOpen)
            {
                this.OpenWindow(editable, false, isPublic);
            }
        }

        [Event(null, 0x1dd), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void OnChangeOpenRequest(bool isOpen, bool editable, ulong user, bool isPublic)
        {
            if (!((Sync.IsServer && this.IsTextPanelOpen) & isOpen))
            {
                this.OnChangeOpen(isOpen, editable, user, isPublic);
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase, bool, bool, ulong, bool>(this, x => new Action<bool, bool, ulong, bool>(x.OnChangeOpenSuccess), isOpen, editable, user, isPublic, targetEndpoint);
            }
        }

        [Event(null, 0x1e8), Reliable, Broadcast]
        private void OnChangeOpenSuccess(bool isOpen, bool editable, ulong user, bool isPublic)
        {
            this.OnChangeOpen(isOpen, editable, user, isPublic);
        }

        public void OnClosedPanelMessageBox(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (result == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                this.m_textBox.Description.Text.Remove(0x186a0, this.m_textBox.Description.Text.Length - 0x186a0);
                this.CloseWindow(true);
            }
            else
            {
                this.CreateTextBox(true, this.m_textBox.Description.Text, true);
                MyScreenManager.AddScreen(this.m_textBox);
            }
        }

        public void OnClosedPanelTextBox(VRage.Game.ModAPI.ResultEnum result)
        {
            if (this.m_textBox != null)
            {
                if (this.m_textBox.Description.Text.Length <= 0x186a0)
                {
                    this.CloseWindow(true);
                }
                else
                {
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextTooLongText), null, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnClosedPanelMessageBox), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }
        }

        [Event(null, 0x544), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void OnConfirmSpawnProjection()
        {
            if (this.m_maxNumberOfProjections < 0x3e8)
            {
                this.m_projectionsRemaining--;
            }
            if (this.m_keepProjection == null)
            {
                this.RemoveProjection(false);
            }
            this.UpdateText();
            base.RaisePropertiesChanged();
        }

        private void OnEnabledChanged(MyTerminalBlock myTerminalBlock)
        {
            if (!base.Enabled)
            {
                this.RemoveProjection(true);
            }
            if ((this.m_multiPanel != null) && (this.m_multiPanel.SurfaceCount > 0))
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.Reset();
            }
            if (base.ResourceSink != null)
            {
                this.UpdateScreen();
            }
            if (this.CheckIsWorking() && (this.m_multiPanel != null))
            {
                this.Render.UpdateModelProperties();
            }
        }

        [Event(null, 0x686), Reliable, Server(ValidationType.Ownership | ValidationType.Access), BroadcastExcept]
        private void OnNewBlueprintSuccess(MyObjectBuilder_CubeGrid projectedGrid)
        {
            if (!MyEventContext.Current.IsLocallyInvoked)
            {
                if (!MySession.Static.IsUserScripter(MyEventContext.Current.Sender.Value) && this.RemoveScriptsFromProjection(ref projectedGrid))
                {
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase>(this, x => new Action(x.ShowScriptRemoveMessage), MyEventContext.Current.Sender);
                }
                this.SetNewBlueprint(projectedGrid);
            }
        }

        [Event(null, 0x6b2), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnOffsetChangedSuccess(Vector3I positionOffset, Vector3I rotationOffset, float scale, bool showOnlyBuildable)
        {
            this.m_projectionScale = scale;
            this.SetNewOffset(positionOffset, rotationOffset, showOnlyBuildable);
            this.m_shouldUpdateProjection = true;
        }

        protected void OnOffsetsChanged()
        {
            this.m_shouldUpdateProjection = true;
            this.m_shouldUpdateTexts = true;
            this.SendNewOffset(this.m_projectionOffset, this.m_projectionRotation, this.m_projectionScale, this.m_showOnlyBuildable);
            this.Remap();
        }

        public override void OnRegisteredToGridSystems()
        {
            if ((this.m_originalGridBuilder != null) && Sync.IsServer)
            {
                this.Remap();
            }
        }

        [Event(null, 0x6bf), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnRemoveProjectionRequest()
        {
            this.RemoveProjection(false);
        }

        [Event(null, 0x63f), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnRemoveSelectedImageRequest(int panelIndex, int[] selection)
        {
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.RemoveItems(panelIndex, selection);
            }
        }

        [Event(null, 0x64d), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnSelectImageRequest(int panelIndex, int[] selection)
        {
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.SelectItems(panelIndex, selection);
            }
        }

        [Event(null, 0x520), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void OnSpawnProjection()
        {
            if (this.CanSpawnProjection())
            {
                MyObjectBuilder_CubeGrid objectBuilder = (MyObjectBuilder_CubeGrid) this.m_originalGridBuilder.Clone();
                Sandbox.Game.Entities.MyEntities.RemapObjectBuilder(objectBuilder);
                if (this.m_getOwnershipFromProjector != null)
                {
                    foreach (MyObjectBuilder_CubeBlock local1 in objectBuilder.CubeBlocks)
                    {
                        local1.Owner = base.OwnerId;
                        local1.ShareMode = base.IDModule.ShareMode;
                    }
                }
                this.m_spawnClipboard.SetGridFromBuilder(objectBuilder, Vector3.Zero, 0f);
                this.m_spawnClipboard.ResetGridOrientation();
                if (!this.m_spawnClipboard.IsActive)
                {
                    this.m_spawnClipboard.Activate(null);
                }
                this.SetRotation(this.m_spawnClipboard, this.m_projectionRotation);
                this.m_spawnClipboard.Update();
                if (this.m_spawnClipboard.ActuallyTestPlacement() && this.m_spawnClipboard.PasteGrid(true, true))
                {
                    this.OnConfirmSpawnProjection();
                }
                this.m_spawnClipboard.Deactivate(false);
                this.m_spawnClipboard.Clear();
            }
        }

        public void OpenWindow(bool isEditable, bool sync, bool isPublic)
        {
            if (sync)
            {
                this.SendChangeOpenMessage(true, isEditable, Sync.MyId, isPublic);
            }
            else
            {
                this.CreateTextBox(isEditable, new StringBuilder(this.PanelComponent.Text), isPublic);
                MyGuiScreenGamePlay.TmpGameplayScreenHolder = MyGuiScreenGamePlay.ActiveGameplayScreen;
                MyGuiScreenGamePlay.ActiveGameplayScreen = this.m_textBox;
                MyScreenManager.AddScreen(this.m_textBox);
            }
        }

        private void PowerReceiver_IsPoweredChanged()
        {
            if (!base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && this.IsProjecting())
            {
                this.RequestRemoveProjection();
            }
            base.UpdateIsWorking();
            this.SetEmissiveStateWorking();
            this.UpdateScreen();
        }

        private void previewGrid_OnBlockAdded(MySlimBlock obj)
        {
            this.m_shouldUpdateProjection = true;
            this.m_shouldUpdateTexts = true;
            if ((this.m_originalGridBuilder != null) && this.IsProjecting())
            {
                Vector3I vectori = this.ProjectedGrid.WorldToGridInteger(base.CubeGrid.GridIntegerToWorld(obj.Position));
                MyTerminalBlock fatBlock = obj.FatBlock as MyTerminalBlock;
                if (fatBlock != null)
                {
                    foreach (MyObjectBuilder_BlockGroup group in this.m_originalGridBuilder.BlockGroups)
                    {
                        foreach (Vector3I vectori2 in group.Blocks)
                        {
                            if (vectori == vectori2)
                            {
                                MyBlockGroup group1 = new MyBlockGroup();
                                group1.Name = new StringBuilder(group.Name);
                                MyBlockGroup group2 = group1;
                                group2.Blocks.Add(fatBlock);
                                base.CubeGrid.AddGroup(group2);
                            }
                        }
                    }
                    fatBlock.CheckConnectionChanged += new Action<MyCubeBlock>(this.TerminalBlockOnCheckConnectionChanged);
                }
            }
        }

        private void previewGrid_OnBlockRemoved(MySlimBlock obj)
        {
            this.m_shouldUpdateProjection = true;
            this.m_shouldUpdateTexts = true;
            if ((obj != null) && (obj.FatBlock != null))
            {
                obj.FatBlock.CheckConnectionChanged -= new Action<MyCubeBlock>(this.TerminalBlockOnCheckConnectionChanged);
            }
        }

        private void Remap()
        {
            if (this.AllowWelding && ((this.m_originalGridBuilder != null) && Sync.IsServer))
            {
                Sandbox.Game.Entities.MyEntities.RemapObjectBuilder(this.m_originalGridBuilder);
                this.SetNewBlueprint(this.m_originalGridBuilder);
            }
        }

        private void RemoveProjection(bool keepProjection)
        {
            this.m_hiddenBlock = null;
            if (this.ProjectedGrid != null)
            {
                int count = this.ProjectedGrid.CubeBlocks.Count;
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(base.BuiltBy);
                if (identity != null)
                {
                    int pcu = count - this.BlockDefinition.PCU;
                    identity.BlockLimits.DecreaseBlocksBuilt(this.BlockDefinition.BlockPairName, pcu, base.CubeGrid, false);
                }
            }
            this.m_clipboard.Deactivate(false);
            if (!keepProjection)
            {
                this.m_clipboard.Clear();
                this.m_originalGridBuilder = null;
            }
            this.UpdateSounds();
            this.SetEmissiveStateWorking();
            this.m_statsDirty = true;
            this.UpdateText();
            base.RaisePropertiesChanged();
        }

        private bool RemoveScriptsFromProjection(ref MyObjectBuilder_CubeGrid grid)
        {
            bool flag = false;
            foreach (MyObjectBuilder_MyProgrammableBlock block in grid.CubeBlocks)
            {
                if (block == null)
                {
                    continue;
                }
                if (block.Program != null)
                {
                    block.Program = null;
                    flag = true;
                }
            }
            return flag;
        }

        private void ReopenTerminal(VRage.Game.Entity.MyEntity interactedEntity = null)
        {
            if (!MyGuiScreenTerminal.IsOpen)
            {
                MyGuiScreenTerminal.Show(MyTerminalPageEnum.ControlPanel, MySession.Static.LocalCharacter, interactedEntity ?? this);
            }
        }

        private void RequestRemoveProjection()
        {
            this.m_removeRequested = true;
            this.m_frameCount = 0;
        }

        private void ResetRotation()
        {
            this.SetRotation(this.m_clipboard, -this.m_projectionRotation);
        }

        void IMyMultiTextPanelComponentOwner.SelectPanel(List<MyGuiControlListbox.Item> panelItems)
        {
            this.m_multiPanel.SelectPanel((int) panelItems[0].UserData);
            base.RaisePropertiesChanged();
        }

        void Sandbox.ModAPI.IMyProjector.Build(VRage.Game.ModAPI.IMySlimBlock cubeBlock, long owner, long builder, bool requestInstant)
        {
            this.Build((MySlimBlock) cubeBlock, owner, builder, requestInstant, 0L);
        }

        BuildCheckResult Sandbox.ModAPI.IMyProjector.CanBuild(VRage.Game.ModAPI.IMySlimBlock projectedBlock, bool checkHavokIntersections) => 
            this.CanBuild((MySlimBlock) projectedBlock, checkHavokIntersections);

        bool Sandbox.ModAPI.IMyProjector.LoadBlueprint(string path) => 
            this.LoadBlueprint(path);

        bool Sandbox.ModAPI.IMyProjector.LoadRandomBlueprint(string searchPattern)
        {
            bool flag = false;
            string[] files = Directory.GetFiles(Path.Combine(MyFileSystem.ContentPath, "Data", "Blueprints"), searchPattern);
            if (files.Length != 0)
            {
                flag = this.LoadBlueprint(files[MyRandom.Instance.Next() % files.Length]);
            }
            return flag;
        }

        void Sandbox.ModAPI.IMyProjector.SetProjectedGrid(MyObjectBuilder_CubeGrid grid)
        {
            if (grid == null)
            {
                this.SendRemoveProjection();
            }
            else
            {
                MyObjectBuilder_CubeGrid gridBuilder = null;
                this.m_originalGridBuilder = null;
                Parallel.Start(delegate {
                    gridBuilder = (MyObjectBuilder_CubeGrid) grid.Clone();
                    this.m_clipboard.ProcessCubeGrid(grid);
                    Sandbox.Game.Entities.MyEntities.RemapObjectBuilder(gridBuilder);
                }, delegate {
                    if ((gridBuilder != null) && (this.m_originalGridBuilder == null))
                    {
                        this.m_originalGridBuilder = gridBuilder;
                        this.SendNewBlueprint(gridBuilder);
                    }
                });
            }
        }

        void Sandbox.ModAPI.Ingame.IMyProjector.UpdateOffsetAndRotation()
        {
            this.OnOffsetsChanged();
        }

        Sandbox.ModAPI.Ingame.IMyTextSurface Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider.GetSurface(int index) => 
            this.m_multiPanel?.GetSurface(index);

        protected bool ScenarioSettingsEnabled() => 
            (MySession.Static.Settings.ScenarioEditMode || MySession.Static.IsScenario);

        public void SelectBlueprint()
        {
            VRage.Game.Entity.MyEntity interactedEntity = null;
            if (MyGuiScreenTerminal.IsOpen)
            {
                interactedEntity = MyGuiScreenTerminal.InteractedEntity;
                MyGuiScreenTerminal.Hide();
            }
            this.SendRemoveProjection();
            if (MyFakes.I_AM_READY_FOR_NEW_BLUEPRINT_SCREEN)
            {
                MyGuiBlueprintScreen_Reworked reworked1 = MyGuiBlueprintScreen_Reworked.CreateBlueprintScreen(this.m_clipboard, true, MyBlueprintAccessType.PROJECTOR);
                reworked1.Closed += screen => this.OnBlueprintScreen_Closed(screen, interactedEntity);
                MyGuiSandbox.AddScreen(reworked1);
            }
            else
            {
                MyGuiBlueprintScreen screen1 = new MyGuiBlueprintScreen(this.m_clipboard, true, MyBlueprintAccessType.PROJECTOR);
                screen1.Closed += screen => this.OnBlueprintScreen_Closed(screen, interactedEntity);
                MyGuiSandbox.AddScreen(screen1);
            }
        }

        private void SendAddImagesToSelectionRequest(int panelIndex, int[] selection)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase, int, int[]>(this, x => new Action<int, int[]>(x.OnSelectImageRequest), panelIndex, selection, targetEndpoint);
        }

        private void SendChangeDescriptionMessage(StringBuilder description, bool isPublic)
        {
            if (base.CubeGrid.IsPreview || !base.CubeGrid.SyncFlag)
            {
                this.PanelComponent.Text = description.ToString();
            }
            else if (description.CompareTo(this.PanelComponent.Text) != 0)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase, string, bool>(this, x => new Action<string, bool>(x.OnChangeDescription), description.ToString(), isPublic, targetEndpoint);
            }
        }

        private void SendChangeOpenMessage(bool isOpen, bool editable = false, ulong user = 0UL, bool isPublic = false)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase, bool, bool, ulong, bool>(this, x => new Action<bool, bool, ulong, bool>(x.OnChangeOpenRequest), isOpen, editable, user, isPublic, targetEndpoint);
        }

        private void SendConfirmSpawnProjection()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase>(this, x => new Action(x.OnConfirmSpawnProjection), targetEndpoint);
        }

        private void SendNewBlueprint(MyObjectBuilder_CubeGrid projectedGrid)
        {
            this.SetNewBlueprint(projectedGrid);
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase, MyObjectBuilder_CubeGrid>(this, x => new Action<MyObjectBuilder_CubeGrid>(x.OnNewBlueprintSuccess), projectedGrid, targetEndpoint);
        }

        public void SendNewOffset(Vector3I positionOffset, Vector3I rotationOffset, float scale, bool showOnlyBuildable)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase, Vector3I, Vector3I, float, bool>(this, x => new Action<Vector3I, Vector3I, float, bool>(x.OnOffsetChangedSuccess), positionOffset, rotationOffset, scale, showOnlyBuildable, targetEndpoint);
        }

        public void SendRemoveProjection()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase>(this, x => new Action(x.OnRemoveProjectionRequest), targetEndpoint);
        }

        private void SendRemoveSelectedImageRequest(int panelIndex, int[] selection)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase, int, int[]>(this, x => new Action<int, int[]>(x.OnRemoveSelectedImageRequest), panelIndex, selection, targetEndpoint);
        }

        private void SendSpawnProjection()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyProjectorBase>(this, x => new Action(x.OnSpawnProjection), targetEndpoint);
        }

        public override bool SetEmissiveStateWorking() => 
            (base.IsWorking && (!this.IsProjecting() ? base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Working, this.Render.RenderObjectIDs[0], null) : base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Alternative, this.Render.RenderObjectIDs[0], null)));

        internal void SetNewBlueprint(MyObjectBuilder_CubeGrid gridBuilder)
        {
            this.m_originalGridBuilder = gridBuilder;
            MyObjectBuilder_CubeGrid originalGridBuilder = this.m_originalGridBuilder;
            this.m_clipboard.SetGridFromBuilder(originalGridBuilder, Vector3.Zero, 0f);
            if (this.m_instantBuildingEnabled != null)
            {
                this.ResetRotation();
                BoundingBox box = originalGridBuilder.CalculateBoundingBox();
                this.m_projectionOffset.Y = Math.Abs((int) (box.Min.Y / MyDefinitionManager.Static.GetCubeSize(originalGridBuilder.GridSizeEnum))) + 2;
            }
            if (base.Enabled && base.IsWorking)
            {
                if (this.BlockDefinition.AllowScaling)
                {
                    BoundingBox box2 = originalGridBuilder.CalculateBoundingBox();
                    this.m_projectionScale = MathHelper.Clamp((float) (base.CubeGrid.GridSize / box2.Size.Max()), (float) 0.02f, (float) 1f);
                }
                this.InitializeClipboard();
            }
        }

        internal void SetNewOffset(Vector3I positionOffset, Vector3I rotationOffset, bool onlyCanBuildBlock)
        {
            this.m_clipboard.ResetGridOrientation();
            this.m_projectionOffset = positionOffset;
            this.m_projectionRotation = rotationOffset;
            this.m_showOnlyBuildable = onlyCanBuildBlock;
            this.SetRotation(this.m_clipboard, this.m_projectionRotation);
        }

        private void SetRotation(MyGridClipboard clipboard, Vector3I rotation)
        {
            clipboard.RotateAroundAxis(0, Math.Sign(rotation.X), true, Math.Abs((float) (rotation.X * 1.570796f)));
            clipboard.RotateAroundAxis(1, Math.Sign(rotation.Y), true, Math.Abs((float) (rotation.Y * 1.570796f)));
            clipboard.RotateAroundAxis(2, Math.Sign(rotation.Z), true, Math.Abs((float) (rotation.Z * 1.570796f)));
        }

        protected virtual void SetTransparency(MySlimBlock cubeBlock, float transparency)
        {
            transparency = -transparency;
            if ((cubeBlock.Dithering != transparency) || (cubeBlock.CubeGrid.Render.Transparency != transparency))
            {
                cubeBlock.CubeGrid.Render.Transparency = transparency;
                cubeBlock.CubeGrid.Render.CastShadows = false;
                cubeBlock.Dithering = transparency;
                cubeBlock.UpdateVisual(true);
                MyCubeBlock fatBlock = cubeBlock.FatBlock;
                if (fatBlock != null)
                {
                    fatBlock.Render.CastShadows = false;
                    this.SetTransparencyForSubparts(fatBlock, transparency);
                }
                if (((fatBlock != null) && (fatBlock.UseObjectsComponent != null)) && (fatBlock.UseObjectsComponent.DetectorPhysics != null))
                {
                    fatBlock.UseObjectsComponent.DetectorPhysics.Enabled = false;
                }
            }
        }

        private void SetTransparencyForSubparts(VRage.Game.Entity.MyEntity renderEntity, float transparency)
        {
            renderEntity.Render.CastShadows = false;
            if (renderEntity.Subparts != null)
            {
                foreach (KeyValuePair<string, MyEntitySubpart> pair in renderEntity.Subparts)
                {
                    pair.Value.Render.Transparency = transparency;
                    pair.Value.Render.CastShadows = false;
                    pair.Value.Render.RemoveRenderObjects();
                    pair.Value.Render.AddRenderObjects();
                    this.SetTransparencyForSubparts(pair.Value, transparency);
                }
            }
        }

        public void ShowCube(MySlimBlock cubeBlock, bool canBuild)
        {
            if (canBuild)
            {
                this.SetTransparency(cubeBlock, 0.25f);
            }
            else
            {
                this.SetTransparency(cubeBlock, MyGridConstants.PROJECTOR_TRANSPARENCY);
            }
        }

        private void ShowNotification(MyStringId textToDisplay)
        {
            MyHudNotification notification = new MyHudNotification(textToDisplay, 0x1388, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
            MyHud.Notifications.Add(notification);
        }

        [Event(null, 0x6a6), Reliable, Client]
        private void ShowScriptRemoveMessage()
        {
            MyHud.Notifications.Add(new MyHudNotification(MySpaceTexts.Notification_BlueprintScriptRemoved, 0x1388, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
        }

        private void TerminalBlockOnCheckConnectionChanged(MyCubeBlock myCubeBlock)
        {
            this.m_forceUpdateProjection = true;
            this.m_shouldUpdateTexts = true;
        }

        protected void TryChangeMaxNumberOfBlocksPerProjection(float v)
        {
            if (this.CanEditInstantBuildingSettings())
            {
                this.MaxNumberOfBlocksPerProjection = (int) Math.Round((double) v);
            }
        }

        protected void TryChangeNumberOfProjections(float v)
        {
            if (this.CanEditInstantBuildingSettings())
            {
                this.MaxNumberOfProjections = (int) Math.Round((double) v);
            }
        }

        protected void TrySetGetOwnership(bool v)
        {
            if (this.CanEnableInstantBuilding())
            {
                this.GetOwnershipFromProjector = v;
            }
        }

        protected void TrySetInstantBuilding(bool v)
        {
            if (this.CanEnableInstantBuilding())
            {
                this.InstantBuildingEnabled = v;
            }
        }

        protected void TrySpawnProjection()
        {
            if (this.CanSpawnProjection())
            {
                this.SendSpawnProjection();
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            base.ResourceSink.Update();
            if (this.m_removeRequested)
            {
                this.m_frameCount++;
                if (this.m_frameCount > 10)
                {
                    base.UpdateIsWorking();
                    if (!base.IsWorking && this.IsProjecting())
                    {
                        this.RemoveProjection(true);
                    }
                    this.m_frameCount = 0;
                    this.m_removeRequested = false;
                }
            }
            if (this.m_clipboard.IsActive)
            {
                this.m_clipboard.Update();
                if (this.m_shouldResetBuildable)
                {
                    this.m_shouldResetBuildable = false;
                    foreach (MySlimBlock block in this.ProjectedGrid.CubeBlocks)
                    {
                        this.HideCube(block);
                    }
                }
                if (this.m_forceUpdateProjection || (this.m_shouldUpdateProjection && ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastUpdate) > 0x7d0)))
                {
                    this.UpdateProjection();
                    this.m_shouldUpdateProjection = false;
                    this.m_forceUpdateProjection = false;
                    this.m_lastUpdate = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                }
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            if (this.m_clipboard.IsActive && (this.m_instantBuildingEnabled != null))
            {
                this.m_clipboard.ActuallyTestPlacement();
            }
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.UpdateAfterSimulation(true);
            }
            if (this.AllowScaling && (this.ProjectedGrid != null))
            {
                bool flag = this.IsInRange();
                MyCubeGrid projectedGrid = this.ProjectedGrid;
                if (projectedGrid.InScene != flag)
                {
                    if (flag)
                    {
                        Sandbox.Game.Entities.MyEntities.Add(projectedGrid, true);
                    }
                    else
                    {
                        Sandbox.Game.Entities.MyEntities.Remove(projectedGrid);
                    }
                }
            }
        }

        private void UpdateBaseText()
        {
            base.DetailedInfo.Clear();
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MyCommonTexts.BlockPropertiesText_Type));
            base.DetailedInfo.Append(this.BlockDefinition.DisplayNameText);
            base.DetailedInfo.Append("\n");
            base.DetailedInfo.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_MaxRequiredInput));
            MyValueFormatter.AppendWorkInBestUnit(this.BlockDefinition.RequiredPowerInput, base.DetailedInfo);
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if ((base.CubeGrid.Physics != null) && (this.m_savedProjection != null))
            {
                MyObjectBuilder_CubeGrid objectBuilder = (MyObjectBuilder_CubeGrid) this.m_savedProjection.Clone();
                Sandbox.Game.Entities.MyEntities.RemapObjectBuilder(objectBuilder);
                this.m_clipboard.ProcessCubeGrid(objectBuilder);
                this.m_clipboard.SetGridFromBuilder(objectBuilder, Vector3.Zero, 0f);
                this.m_originalGridBuilder = this.m_savedProjection;
                this.m_savedProjection = null;
                if (base.IsWorking)
                {
                    this.InitializeClipboard();
                }
                this.RequestRemoveProjection();
            }
            this.UpdateSounds();
            this.SetEmissiveStateWorking();
            this.UpdateScreen();
        }

        private void UpdateProjection()
        {
            if (this.m_instantBuildingEnabled == null)
            {
                if (this.m_updateTask.IsComplete)
                {
                    this.m_hiddenBlock = null;
                    if (this.m_clipboard.PreviewGrids.Count != 0)
                    {
                        this.ProjectedGrid.Render.Transparency = 0f;
                        this.m_updateTask = MyProjectorUpdateWork.Start(this);
                    }
                }
            }
            else if (this.ProjectedGrid != null)
            {
                foreach (MySlimBlock block in this.ProjectedGrid.CubeBlocks)
                {
                    this.ShowCube(block, true);
                }
                this.m_clipboard.HasPreviewBBox = true;
            }
        }

        public void UpdateScreen()
        {
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.UpdateScreen(this.CheckIsWorking());
            }
        }

        private void UpdateSounds()
        {
            base.UpdateIsWorking();
            if (base.IsWorking)
            {
                bool? nullable;
                if (!this.IsProjecting())
                {
                    if (((base.m_soundEmitter != null) && (base.m_soundEmitter.SoundId != this.BlockDefinition.IdleSound.Arcade)) && (base.m_soundEmitter.SoundId != this.BlockDefinition.IdleSound.Realistic))
                    {
                        base.m_soundEmitter.StopSound(false, true);
                        nullable = null;
                        base.m_soundEmitter.PlaySound(this.BlockDefinition.IdleSound, false, false, false, false, false, nullable);
                    }
                }
                else if (((base.m_soundEmitter != null) && (base.m_soundEmitter.SoundId != this.BlockDefinition.PrimarySound.Arcade)) && (base.m_soundEmitter.SoundId != this.BlockDefinition.PrimarySound.Realistic))
                {
                    base.m_soundEmitter.StopSound(false, true);
                    nullable = null;
                    base.m_soundEmitter.PlaySound(this.BlockDefinition.PrimarySound, false, false, false, false, false, nullable);
                }
            }
        }

        private void UpdateStats()
        {
            this.m_totalBlocks = this.ProjectedGrid.CubeBlocks.Count;
            this.m_remainingArmorBlocks = 0;
            this.m_remainingBlocksPerType.Clear();
            foreach (MySlimBlock block in this.ProjectedGrid.CubeBlocks)
            {
                Vector3 coords = (Vector3) this.ProjectedGrid.GridIntegerToWorld(block.Position);
                Vector3I pos = base.CubeGrid.WorldToGridInteger(coords);
                MySlimBlock cubeBlock = base.CubeGrid.GetCubeBlock(pos);
                if ((cubeBlock == null) || (block.BlockDefinition.Id != cubeBlock.BlockDefinition.Id))
                {
                    if (block.FatBlock == null)
                    {
                        this.m_remainingArmorBlocks++;
                    }
                    else if (!this.m_remainingBlocksPerType.ContainsKey(block.BlockDefinition))
                    {
                        this.m_remainingBlocksPerType.Add(block.BlockDefinition, 1);
                    }
                    else
                    {
                        MyCubeBlockDefinition blockDefinition = block.BlockDefinition;
                        this.m_remainingBlocksPerType[blockDefinition] += 1;
                    }
                }
            }
        }

        private void UpdateText()
        {
            if (this.m_instantBuildingEnabled == null)
            {
                if (!this.m_statsDirty)
                {
                    return;
                }
                if (this.m_clipboard.IsActive)
                {
                    this.UpdateStats();
                }
                this.m_statsDirty = false;
                this.UpdateBaseText();
                if (this.m_clipboard.IsActive && this.AllowWelding)
                {
                    base.DetailedInfo.Append("\n");
                    if (this.m_buildableBlocksCount > 0)
                    {
                        base.DetailedInfo.Append("\n");
                    }
                    else
                    {
                        base.DetailedInfo.Append("WARNING! Projection out of bounds!\n");
                    }
                    object[] objArray3 = new object[] { "Build progress: ", this.m_totalBlocks - this.m_remainingBlocks, "/", this.m_totalBlocks };
                    base.DetailedInfo.Append(string.Concat(objArray3));
                    if ((this.m_remainingArmorBlocks > 0) || (this.m_remainingBlocksPerType.Count != 0))
                    {
                        base.DetailedInfo.Append("\nBlocks remaining:\n");
                        base.DetailedInfo.Append("Armor blocks: " + this.m_remainingArmorBlocks);
                        foreach (KeyValuePair<MyCubeBlockDefinition, int> pair in this.m_remainingBlocksPerType)
                        {
                            base.DetailedInfo.Append("\n");
                            base.DetailedInfo.Append(pair.Key.DisplayNameText + ": " + pair.Value);
                        }
                    }
                    else
                    {
                        base.DetailedInfo.Append("\nComplete!");
                    }
                }
            }
            else
            {
                this.UpdateBaseText();
                if (this.m_clipboard.IsActive && (this.ProjectedGrid != null))
                {
                    if (this.m_maxNumberOfBlocksPerProjection < 0x2710)
                    {
                        base.DetailedInfo.Append("\n");
                        object[] objArray1 = new object[] { "Ship blocks: ", this.ProjectedGrid.BlocksCount, "/", this.m_maxNumberOfBlocksPerProjection };
                        base.DetailedInfo.Append(string.Concat(objArray1));
                    }
                    if (this.m_maxNumberOfProjections < 0x3e8)
                    {
                        base.DetailedInfo.Append("\n");
                        object[] objArray2 = new object[] { "Projections remaining: ", this.m_projectionsRemaining, "/", this.m_maxNumberOfProjections };
                        base.DetailedInfo.Append(string.Concat(objArray2));
                        return;
                    }
                }
                return;
            }
            base.RaisePropertiesChanged();
        }

        public MyProjectorDefinition BlockDefinition =>
            ((MyProjectorDefinition) base.BlockDefinition);

        public MyProjectorClipboard Clipboard =>
            this.m_clipboard;

        internal MyRenderComponentScreenAreas Render
        {
            get => 
                (base.Render as MyRenderComponentScreenAreas);
            set => 
                (base.Render = value);
        }

        public Vector3I ProjectionOffset =>
            this.m_projectionOffset;

        public Vector3I ProjectionRotation =>
            this.m_projectionRotation;

        public Quaternion ProjectionRotationQuaternion
        {
            get
            {
                Vector3 vector = (Vector3) (this.ProjectionRotation * 1.570796f);
                return Quaternion.CreateFromYawPitchRoll(vector.X, vector.Y, vector.Z);
            }
        }

        public MyCubeGrid ProjectedGrid =>
            ((this.m_clipboard.PreviewGrids.Count == 0) ? null : this.m_clipboard.PreviewGrids[0]);

        protected bool InstantBuildingEnabled
        {
            get => 
                ((bool) this.m_instantBuildingEnabled);
            set => 
                (this.m_instantBuildingEnabled.Value = value);
        }

        protected int MaxNumberOfProjections
        {
            get => 
                ((int) this.m_maxNumberOfProjections);
            set => 
                (this.m_maxNumberOfProjections.Value = value);
        }

        protected int MaxNumberOfBlocksPerProjection
        {
            get => 
                ((int) this.m_maxNumberOfBlocksPerProjection);
            set => 
                (this.m_maxNumberOfBlocksPerProjection.Value = value);
        }

        protected bool GetOwnershipFromProjector
        {
            get => 
                ((bool) this.m_getOwnershipFromProjector);
            set => 
                (this.m_getOwnershipFromProjector.Value = value);
        }

        protected bool KeepProjection
        {
            get => 
                ((bool) this.m_keepProjection);
            set => 
                (this.m_keepProjection.Value = value);
        }

        public bool IsActivating { get; private set; }

        public float Scale =>
            (this.BlockDefinition.AllowScaling ? this.m_projectionScale : 1f);

        public bool AllowScaling =>
            this.BlockDefinition.AllowScaling;

        public bool AllowWelding =>
            (this.BlockDefinition.AllowWelding && (!this.BlockDefinition.AllowScaling && !this.BlockDefinition.IgnoreSize));

        MyMultiTextPanelComponent IMyMultiTextPanelComponentOwner.MultiTextPanel =>
            this.m_multiPanel;

        public MyTextPanelComponent PanelComponent =>
            this.m_multiPanel?.PanelComponent;

        public bool IsTextPanelOpen
        {
            get => 
                this.m_isTextPanelOpen;
            set
            {
                if (this.m_isTextPanelOpen != value)
                {
                    this.m_isTextPanelOpen = value;
                    base.RaisePropertiesChanged();
                }
            }
        }

        VRage.Game.ModAPI.IMyCubeGrid Sandbox.ModAPI.IMyProjector.ProjectedGrid =>
            this.ProjectedGrid;

        Vector3I Sandbox.ModAPI.Ingame.IMyProjector.ProjectionOffset
        {
            get => 
                this.m_projectionOffset;
            set => 
                (this.m_projectionOffset = value);
        }

        Vector3I Sandbox.ModAPI.Ingame.IMyProjector.ProjectionRotation
        {
            get => 
                this.m_projectionRotation;
            set => 
                (this.m_projectionRotation = value);
        }

        [Obsolete("Use ProjectionOffset vector instead.")]
        int Sandbox.ModAPI.Ingame.IMyProjector.ProjectionOffsetX =>
            this.m_projectionOffset.X;

        [Obsolete("Use ProjectionOffset vector instead.")]
        int Sandbox.ModAPI.Ingame.IMyProjector.ProjectionOffsetY =>
            this.m_projectionOffset.Y;

        [Obsolete("Use ProjectionOffset vector instead.")]
        int Sandbox.ModAPI.Ingame.IMyProjector.ProjectionOffsetZ =>
            this.m_projectionOffset.Z;

        [Obsolete("Use ProjectionRotation vector instead.")]
        int Sandbox.ModAPI.Ingame.IMyProjector.ProjectionRotX =>
            (this.m_projectionRotation.X * 90);

        [Obsolete("Use ProjectionRotation vector instead.")]
        int Sandbox.ModAPI.Ingame.IMyProjector.ProjectionRotY =>
            (this.m_projectionRotation.Y * 90);

        [Obsolete("Use ProjectionRotation vector instead.")]
        int Sandbox.ModAPI.Ingame.IMyProjector.ProjectionRotZ =>
            (this.m_projectionRotation.Z * 90);

        bool Sandbox.ModAPI.Ingame.IMyProjector.IsProjecting =>
            this.IsProjecting();

        int Sandbox.ModAPI.Ingame.IMyProjector.RemainingBlocks =>
            this.m_remainingBlocks;

        int Sandbox.ModAPI.Ingame.IMyProjector.TotalBlocks =>
            this.m_totalBlocks;

        int Sandbox.ModAPI.Ingame.IMyProjector.RemainingArmorBlocks =>
            this.m_remainingArmorBlocks;

        int Sandbox.ModAPI.Ingame.IMyProjector.BuildableBlocksCount =>
            this.m_buildableBlocksCount;

        Dictionary<MyDefinitionBase, int> Sandbox.ModAPI.Ingame.IMyProjector.RemainingBlocksPerType
        {
            get
            {
                Dictionary<MyDefinitionBase, int> dictionary = new Dictionary<MyDefinitionBase, int>();
                foreach (KeyValuePair<MyCubeBlockDefinition, int> pair in this.m_remainingBlocksPerType)
                {
                    dictionary.Add(pair.Key, pair.Value);
                }
                return dictionary;
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyProjector.ShowOnlyBuildable
        {
            get => 
                this.m_showOnlyBuildable;
            set => 
                (this.m_showOnlyBuildable = value);
        }

        int Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider.SurfaceCount =>
            ((this.m_multiPanel != null) ? this.m_multiPanel.SurfaceCount : 0);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyProjectorBase.<>c <>9 = new MyProjectorBase.<>c();
            public static Func<MyProjectorBase, Action<bool, bool, ulong, bool>> <>9__104_0;
            public static Func<MyProjectorBase, Action<bool, bool, ulong, bool>> <>9__105_0;
            public static Func<MyProjectorBase, Action<string, bool>> <>9__112_0;
            public static Func<MyProjectorBase, Action<Vector3I, long, long, bool, long>> <>9__165_0;
            public static Func<MyCubeGrid, Action<uint, MyCubeGrid.MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>> <>9__166_0;
            public static Func<MyProjectorBase, Action<int, int[]>> <>9__167_0;
            public static Func<MyProjectorBase, Action<int, int[]>> <>9__169_0;
            public static Func<MyProjectorBase, Action<MyObjectBuilder_CubeGrid>> <>9__173_0;
            public static Func<MyProjectorBase, Action> <>9__174_0;
            public static Func<MyProjectorBase, Action<Vector3I, Vector3I, float, bool>> <>9__177_0;
            public static Func<MyProjectorBase, Action> <>9__179_0;
            public static Func<MyProjectorBase, Action> <>9__181_0;
            public static Func<MyProjectorBase, Action> <>9__182_0;

            internal Action<Vector3I, long, long, bool, long> <Build>b__165_0(MyProjectorBase x) => 
                new Action<Vector3I, long, long, bool, long>(x.BuildInternal);

            internal Action<uint, MyCubeGrid.MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long> <BuildInternal>b__166_0(MyCubeGrid x) => 
                new Action<uint, MyCubeGrid.MyBlockLocation, MyObjectBuilder_CubeBlock, long, bool, long>(x.BuildBlockRequest);

            internal Action<bool, bool, ulong, bool> <OnChangeOpenRequest>b__105_0(MyProjectorBase x) => 
                new Action<bool, bool, ulong, bool>(x.OnChangeOpenSuccess);

            internal Action <OnNewBlueprintSuccess>b__174_0(MyProjectorBase x) => 
                new Action(x.ShowScriptRemoveMessage);

            internal Action<int, int[]> <SendAddImagesToSelectionRequest>b__169_0(MyProjectorBase x) => 
                new Action<int, int[]>(x.OnSelectImageRequest);

            internal Action<string, bool> <SendChangeDescriptionMessage>b__112_0(MyProjectorBase x) => 
                new Action<string, bool>(x.OnChangeDescription);

            internal Action<bool, bool, ulong, bool> <SendChangeOpenMessage>b__104_0(MyProjectorBase x) => 
                new Action<bool, bool, ulong, bool>(x.OnChangeOpenRequest);

            internal Action <SendConfirmSpawnProjection>b__182_0(MyProjectorBase x) => 
                new Action(x.OnConfirmSpawnProjection);

            internal Action<MyObjectBuilder_CubeGrid> <SendNewBlueprint>b__173_0(MyProjectorBase x) => 
                new Action<MyObjectBuilder_CubeGrid>(x.OnNewBlueprintSuccess);

            internal Action<Vector3I, Vector3I, float, bool> <SendNewOffset>b__177_0(MyProjectorBase x) => 
                new Action<Vector3I, Vector3I, float, bool>(x.OnOffsetChangedSuccess);

            internal Action <SendRemoveProjection>b__179_0(MyProjectorBase x) => 
                new Action(x.OnRemoveProjectionRequest);

            internal Action<int, int[]> <SendRemoveSelectedImageRequest>b__167_0(MyProjectorBase x) => 
                new Action<int, int[]>(x.OnRemoveSelectedImageRequest);

            internal Action <SendSpawnProjection>b__181_0(MyProjectorBase x) => 
                new Action(x.OnSpawnProjection);
        }

        private class MyProjectorUpdateWork : IWork
        {
            private static readonly MyDynamicObjectPool<MyProjectorBase.MyProjectorUpdateWork> InstancePool = new MyDynamicObjectPool<MyProjectorBase.MyProjectorUpdateWork>(8);
            private MyProjectorBase m_projector;
            private MyCubeGrid m_grid;
            private HashSet<MySlimBlock> m_visibleBlocks = new HashSet<MySlimBlock>();
            private HashSet<MySlimBlock> m_buildableBlocks = new HashSet<MySlimBlock>();
            private HashSet<MySlimBlock> m_hiddenBlocks = new HashSet<MySlimBlock>();
            private int m_remainingBlocks;
            private int m_buildableBlocksCount;

            public void DoWork(WorkData workData = null)
            {
                this.m_remainingBlocks = this.m_grid.BlocksCount;
                this.m_buildableBlocksCount = 0;
                foreach (MySlimBlock block in this.m_grid.CubeBlocks)
                {
                    Vector3D coords = this.m_grid.GridIntegerToWorld(block.Position);
                    Vector3I pos = this.m_projector.CubeGrid.WorldToGridInteger(coords);
                    MySlimBlock cubeBlock = this.m_projector.CubeGrid.GetCubeBlock(pos);
                    if ((cubeBlock != null) && (block.BlockDefinition.Id == cubeBlock.BlockDefinition.Id))
                    {
                        this.m_hiddenBlocks.Add(block);
                        this.m_remainingBlocks--;
                        continue;
                    }
                    if (this.m_projector.CanBuild(block))
                    {
                        this.m_buildableBlocks.Add(block);
                        this.m_buildableBlocksCount++;
                    }
                    else if (!this.m_projector.AllowWelding || !this.m_projector.m_showOnlyBuildable)
                    {
                        this.m_visibleBlocks.Add(block);
                    }
                    else
                    {
                        this.m_hiddenBlocks.Add(block);
                    }
                }
            }

            private void OnComplete()
            {
                if ((!this.m_projector.Closed && !this.m_projector.CubeGrid.Closed) && (this.m_projector.ProjectedGrid != null))
                {
                    foreach (MySlimBlock block in this.m_visibleBlocks)
                    {
                        if (!this.m_projector.m_visibleBlocks.Contains(block))
                        {
                            if (this.m_projector.Enabled)
                            {
                                this.m_projector.ShowCube(block, false);
                                continue;
                            }
                            this.m_projector.HideCube(block);
                        }
                    }
                    MyUtils.Swap<HashSet<MySlimBlock>>(ref this.m_visibleBlocks, ref this.m_projector.m_visibleBlocks);
                    if (this.m_projector.BlockDefinition.AllowWelding)
                    {
                        foreach (MySlimBlock block2 in this.m_buildableBlocks)
                        {
                            if (!this.m_projector.m_buildableBlocks.Contains(block2))
                            {
                                if (this.m_projector.Enabled)
                                {
                                    this.m_projector.ShowCube(block2, true);
                                    continue;
                                }
                                this.m_projector.HideCube(block2);
                            }
                        }
                    }
                    MyUtils.Swap<HashSet<MySlimBlock>>(ref this.m_buildableBlocks, ref this.m_projector.m_buildableBlocks);
                    foreach (MySlimBlock block3 in this.m_hiddenBlocks)
                    {
                        if (!this.m_projector.m_hiddenBlocks.Contains(block3))
                        {
                            this.m_projector.HideCube(block3);
                        }
                    }
                    MyUtils.Swap<HashSet<MySlimBlock>>(ref this.m_hiddenBlocks, ref this.m_projector.m_hiddenBlocks);
                    this.m_projector.m_remainingBlocks = this.m_remainingBlocks;
                    this.m_projector.m_buildableBlocksCount = this.m_buildableBlocksCount;
                    if ((this.m_projector.m_remainingBlocks == 0) && (this.m_projector.m_keepProjection == null))
                    {
                        this.m_projector.RemoveProjection((bool) this.m_projector.m_keepProjection);
                    }
                    else
                    {
                        this.m_projector.UpdateSounds();
                        this.m_projector.SetEmissiveStateWorking();
                    }
                    this.m_projector.m_statsDirty = true;
                    if (this.m_projector.m_shouldUpdateTexts)
                    {
                        this.m_projector.UpdateText();
                        this.m_projector.m_shouldUpdateTexts = false;
                    }
                    this.m_projector.m_clipboard.HasPreviewBBox = false;
                    this.m_projector = null;
                    this.m_visibleBlocks.Clear();
                    this.m_buildableBlocks.Clear();
                    this.m_hiddenBlocks.Clear();
                    InstancePool.Deallocate(this);
                }
            }

            public static Task Start(MyProjectorBase projector)
            {
                MyProjectorBase.MyProjectorUpdateWork work = InstancePool.Allocate();
                work.m_projector = projector;
                work.m_grid = projector.ProjectedGrid;
                return Parallel.Start(work, new Action(work.OnComplete));
            }

            public WorkOptions Options =>
                Parallel.DefaultOptions.WithDebugInfo(MyProfiler.TaskType.Block, "Projector");
        }
    }
}

