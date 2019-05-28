namespace Sandbox.Game.SessionComponents.Clipboard
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems.ContextHandling;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Components.Session;
    using VRage.Game.Definitions.SessionComponents;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.Voxels;
    using VRage.Input;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyClipboardComponent : MySessionComponentBase, IMyFocusHolder
    {
        public static MyClipboardComponent Static;
        protected static readonly MyStringId[] m_rotationControls = new MyStringId[] { MyControlsSpace.CUBE_ROTATE_VERTICAL_POSITIVE, MyControlsSpace.CUBE_ROTATE_VERTICAL_NEGATIVE, MyControlsSpace.CUBE_ROTATE_HORISONTAL_POSITIVE, MyControlsSpace.CUBE_ROTATE_HORISONTAL_NEGATIVE, MyControlsSpace.CUBE_ROTATE_ROLL_POSITIVE, MyControlsSpace.CUBE_ROTATE_ROLL_NEGATIVE };
        protected static readonly int[] m_rotationDirections = new int[] { -1, 1, 1, -1, 1, -1 };
        private static MyClipboardDefinition m_definition;
        private static MyGridClipboard m_clipboard;
        private MyFloatingObjectClipboard m_floatingObjectClipboard = new MyFloatingObjectClipboard(true);
        private MyVoxelClipboard m_voxelClipboard = new MyVoxelClipboard();
        private MyHudNotification m_symmetryNotification;
        private MyHudNotification m_pasteNotification;
        private MyHudNotification m_blueprintNotification;
        private float IntersectionDistance = 20f;
        private float BLOCK_ROTATION_SPEED = 0.002f;
        private MyCubeBlockDefinitionWithVariants m_definitionWithVariants;
        protected MyBlockBuilderRotationHints m_rotationHints = new MyBlockBuilderRotationHints();
        private List<Vector3D> m_collisionTestPoints = new List<Vector3D>(12);
        private int m_lastInputHandleTime;
        protected bool m_rotationHintRotating;
        private bool m_activated;
        private MyHudNotification m_stationRotationNotification;
        private MyHudNotification m_stationRotationNotificationOff;

        private void Activate()
        {
            MySession.Static.GameFocusManager.Register(this);
            this.m_activated = true;
        }

        public void ActivateFloatingObjectClipboard(MyObjectBuilder_FloatingObject floatingObject, Vector3D centerDeltaDirection, float dragVectorLength)
        {
            MySessionComponentVoxelHand.Static.Enabled = false;
            this.m_floatingObjectClipboard.SetFloatingObjectFromBuilder(floatingObject, centerDeltaDirection, dragVectorLength);
            this.Activate();
        }

        public void ActivateVoxelClipboard(MyObjectBuilder_EntityBase voxelMap, IMyStorage storage, Vector3 centerDeltaDirection, float dragVectorLength)
        {
            MySessionComponentVoxelHand.Static.Enabled = false;
            this.m_voxelClipboard.SetVoxelMapFromBuilder(voxelMap, storage, centerDeltaDirection, dragVectorLength);
            this.Activate();
        }

        private void Deactivate()
        {
            MySession.Static.GameFocusManager.Unregister(this);
            this.m_activated = false;
            this.m_rotationHints.ReleaseRenderData();
            this.DeactivateCopyPasteVoxel(false);
            this.DeactivateCopyPasteFloatingObject(false);
            this.DeactivateCopyPaste(false);
        }

        public void DeactivateCopyPaste(bool clear = false)
        {
            if (m_clipboard.IsActive)
            {
                m_clipboard.Deactivate(false);
            }
            this.RemovePasteNotification();
            if (clear)
            {
                m_clipboard.ClearClipboard();
            }
        }

        public void DeactivateCopyPasteFloatingObject(bool clear = false)
        {
            if (this.m_floatingObjectClipboard.IsActive)
            {
                this.m_floatingObjectClipboard.Deactivate();
            }
            this.RemovePasteNotification();
            if (clear)
            {
                this.m_floatingObjectClipboard.ClearClipboard();
            }
        }

        public void DeactivateCopyPasteVoxel(bool clear = false)
        {
            if (this.m_voxelClipboard.IsActive)
            {
                this.m_voxelClipboard.Deactivate();
            }
            this.RemovePasteNotification();
            if (clear)
            {
                this.m_voxelClipboard.ClearClipboard();
            }
        }

        private bool HandleBlueprintInput()
        {
            if ((!MyInput.Static.IsNewKeyPressed(MyKeys.B) || !MyInput.Static.IsAnyCtrlKeyPressed()) || MyInput.Static.IsAnyMousePressed())
            {
                return false;
            }
            if (!m_clipboard.IsActive)
            {
                MySessionComponentVoxelHand.Static.Enabled = false;
                MyCubeGrid targetGrid = MyCubeGrid.GetTargetGrid();
                if (targetGrid == null)
                {
                    return true;
                }
                if (!MySessionComponentSafeZones.IsActionAllowed(targetGrid, MySafeZoneAction.Building, MySession.Static.LocalCharacterEntityId))
                {
                    return false;
                }
                if (!MySession.Static.CreativeMode && !MySession.Static.CreativeToolsEnabled(Sync.MyId))
                {
                    List<MyCubeGrid> result = new List<MyCubeGrid>();
                    if (MyInput.Static.IsAnyShiftKeyPressed())
                    {
                        result.Add(targetGrid);
                    }
                    else
                    {
                        MyCubeGridGroups.Static.GetGroups(MyInput.Static.IsAnyAltKeyPressed() ? GridLinkTypeEnum.Physical : GridLinkTypeEnum.Logical).GetGroupNodes(targetGrid, result);
                    }
                    bool flag = true;
                    foreach (MyCubeGrid grid2 in result)
                    {
                        if (grid2.BigOwners.Count == 0)
                        {
                            continue;
                        }
                        if (!grid2.BigOwners.Contains(MySession.Static.LocalPlayerId))
                        {
                            MyFaction playerFaction = MySession.Static.Factions.GetPlayerFaction(MySession.Static.LocalPlayerId);
                            if (playerFaction == null)
                            {
                                flag = false;
                            }
                            else
                            {
                                bool flag2 = false;
                                foreach (long num in grid2.BigOwners)
                                {
                                    if (ReferenceEquals(MySession.Static.Factions.GetPlayerFaction(num), playerFaction))
                                    {
                                        flag2 = true;
                                        break;
                                    }
                                }
                                if (flag2)
                                {
                                    continue;
                                }
                                flag = false;
                            }
                            break;
                        }
                    }
                    if (!flag)
                    {
                        MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                        this.UpdateBlueprintNotification(MyCommonTexts.CubeBuilderNoBlueprintPermission);
                        return true;
                    }
                }
                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                if (MyInput.Static.IsAnyShiftKeyPressed())
                {
                    m_clipboard.CopyGrid(targetGrid);
                }
                else
                {
                    m_clipboard.CopyGroup(targetGrid, MyInput.Static.IsAnyAltKeyPressed() ? GridLinkTypeEnum.Physical : GridLinkTypeEnum.Logical);
                }
                this.UpdatePasteNotification(MyCommonTexts.CubeBuilderPasteNotification);
                if (MyFakes.I_AM_READY_FOR_NEW_BLUEPRINT_SCREEN)
                {
                    MyGuiBlueprintScreen_Reworked screen = MyGuiBlueprintScreen_Reworked.CreateBlueprintScreen(m_clipboard, MySession.Static.CreativeMode || MySession.Static.CreativeToolsEnabled(Sync.MyId), MyBlueprintAccessType.NORMAL);
                    if (targetGrid != null)
                    {
                        screen.CreateBlueprintFromClipboard(true, false);
                    }
                    m_clipboard.Deactivate(false);
                    MyGuiSandbox.AddScreen(screen);
                }
                else
                {
                    MyGuiBlueprintScreen screen = new MyGuiBlueprintScreen(m_clipboard, MySession.Static.CreativeMode || MySession.Static.CreativeToolsEnabled(Sync.MyId), MyBlueprintAccessType.NORMAL);
                    if (targetGrid != null)
                    {
                        screen.CreateFromClipboard(true, false);
                    }
                    m_clipboard.Deactivate(false);
                    MyGuiSandbox.AddScreen(screen);
                }
            }
            return true;
        }

        private bool HandleCopyInput()
        {
            if ((MyInput.Static.IsNewKeyPressed(MyKeys.C) && MyInput.Static.IsAnyCtrlKeyPressed()) && !MyInput.Static.IsAnyMousePressed())
            {
                if (m_clipboard.IsBeingAdded)
                {
                    MyHud.Notifications.Add(MyNotificationSingletons.CopyFailed);
                    return false;
                }
                if ((MySession.Static.CameraController is MyCharacter) || (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Spectator))
                {
                    bool flag = false;
                    MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                    VRage.Game.Entity.MyEntity targetEntity = MyCubeGrid.GetTargetEntity();
                    if (m_clipboard.IsActive || !(targetEntity is MyCubeGrid))
                    {
                        if (!this.m_floatingObjectClipboard.IsActive && (targetEntity is MyFloatingObject))
                        {
                            MySessionComponentVoxelHand.Static.Enabled = false;
                            this.DeactivateCopyPaste(true);
                            this.m_floatingObjectClipboard.CopyfloatingObject(targetEntity as MyFloatingObject);
                            this.UpdatePasteNotification(MyCommonTexts.CubeBuilderPasteNotification);
                            flag = true;
                        }
                    }
                    else
                    {
                        MyCubeGrid gridInGroup = targetEntity as MyCubeGrid;
                        MySessionComponentVoxelHand.Static.Enabled = false;
                        this.DeactivateCopyPasteFloatingObject(true);
                        if (!MyInput.Static.IsAnyShiftKeyPressed())
                        {
                            m_clipboard.CopyGroup(gridInGroup, MyInput.Static.IsAnyAltKeyPressed() ? GridLinkTypeEnum.Physical : GridLinkTypeEnum.Logical);
                            m_clipboard.Activate(null);
                        }
                        else
                        {
                            m_clipboard.CopyGrid(gridInGroup);
                            m_clipboard.Activate(null);
                        }
                        this.UpdatePasteNotification(MyCommonTexts.CubeBuilderPasteNotification);
                        flag = true;
                    }
                    if (flag)
                    {
                        this.Activate();
                        MyHud.Notifications.Add(MyNotificationSingletons.CopySucceeded);
                        return true;
                    }
                    MyHud.Notifications.Add(MyNotificationSingletons.CopyFailed);
                }
            }
            return false;
        }

        private bool HandleCutInput()
        {
            bool handled;
            bool flag;
            VRageMath.Vector4? nullable;
            Vector3? nullable2;
            MyStringId? nullable3;
            Vector2? nullable4;
            if (!MyInput.Static.IsNewKeyPressed(MyKeys.X) || !MyInput.Static.IsAnyCtrlKeyPressed())
            {
                return false;
            }
            VRage.Game.Entity.MyEntity entity = MyCubeGrid.GetTargetEntity();
            if (entity == null)
            {
                return false;
            }
            if (!MySessionComponentSafeZones.IsActionAllowed(entity, MySafeZoneAction.Building, MySession.Static.LocalCharacterEntityId))
            {
                return false;
            }
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid == null)
            {
                goto TR_000E;
            }
            else
            {
                MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
                if (localHumanPlayer == null)
                {
                    return false;
                }
                long identityId = localHumanPlayer.Identity.IdentityId;
                flag = false;
                bool flag2 = false;
                IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(identityId);
                if (faction != null)
                {
                    flag2 = faction.IsLeader(identityId);
                }
                if (!MySession.Static.IsUserAdmin(localHumanPlayer.Id.SteamId))
                {
                    if (grid.BigOwners.Count != 0)
                    {
                        foreach (long num2 in grid.BigOwners)
                        {
                            if (num2 == identityId)
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
                MyHud.Notifications.Add(MyNotificationSingletons.CutPermissionFailed);
                return false;
            }
        TR_000E:
            handled = false;
            if ((grid != null) && !m_clipboard.IsActive)
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                bool cutGroup = !MyInput.Static.IsAnyShiftKeyPressed();
                bool cutOverLg = MyInput.Static.IsAnyAltKeyPressed();
                nullable = null;
                nullable2 = null;
                nullable3 = null;
                Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(entity, true, nullable, 0.01f, nullable2, nullable3);
                nullable3 = null;
                nullable3 = null;
                nullable3 = null;
                nullable3 = null;
                nullable4 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAreYouSureToMoveGridToClipboard), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), nullable3, nullable3, nullable3, nullable3, delegate (MyGuiScreenMessageBox.ResultEnum v) {
                    if (v == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        this.OnCutConfirm(entity as MyCubeGrid, cutGroup, cutOverLg);
                    }
                    VRageMath.Vector4? color = null;
                    Vector3? inflateAmount = null;
                    MyStringId? lineMaterial = null;
                    Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(entity, false, color, 0.01f, inflateAmount, lineMaterial);
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable4));
                handled = true;
            }
            else if (((entity is MyVoxelMap) && !this.m_voxelClipboard.IsActive) && (MyPerGameSettings.GUI.VoxelMapEditingScreen == typeof(MyGuiScreenDebugSpawnMenu)))
            {
                nullable3 = null;
                nullable3 = null;
                nullable3 = null;
                nullable3 = null;
                nullable4 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.MessageBoxTextAreYouSureToRemoveAsteroid), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), nullable3, nullable3, nullable3, nullable3, delegate (MyGuiScreenMessageBox.ResultEnum v) {
                    if (v == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        this.OnCutAsteroidConfirm(entity as MyVoxelMap);
                    }
                    VRageMath.Vector4? color = null;
                    Vector3? inflateAmount = null;
                    MyStringId? lineMaterial = null;
                    Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(entity, false, color, 0.01f, inflateAmount, lineMaterial);
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable4));
                handled = true;
            }
            else if ((entity is MyFloatingObject) && !this.m_floatingObjectClipboard.IsActive)
            {
                nullable = null;
                nullable2 = null;
                nullable3 = null;
                Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(entity, true, nullable, 0.01f, nullable2, nullable3);
                nullable3 = null;
                nullable3 = null;
                nullable3 = null;
                nullable3 = null;
                nullable4 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAreYouSureToMoveGridToClipboard), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), nullable3, nullable3, nullable3, nullable3, delegate (MyGuiScreenMessageBox.ResultEnum v) {
                    if (v == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        this.OnCutFloatingObjectConfirm(entity as MyFloatingObject);
                        handled = true;
                    }
                    VRageMath.Vector4? color = null;
                    Vector3? inflateAmount = null;
                    MyStringId? lineMaterial = null;
                    Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(entity, false, color, 0.01f, inflateAmount, lineMaterial);
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable4));
                handled = true;
            }
            return handled;
        }

        private bool HandleDeleteInput()
        {
            bool flag;
            bool flag2;
            MyStringId? nullable;
            Vector2? nullable2;
            if (!MyInput.Static.IsNewKeyPressed(MyKeys.Delete) || !MyInput.Static.IsAnyCtrlKeyPressed())
            {
                return false;
            }
            VRage.Game.Entity.MyEntity entity = MyCubeGrid.GetTargetEntity();
            if (entity == null)
            {
                return false;
            }
            if (!MySessionComponentSafeZones.IsActionAllowed(entity, MySafeZoneAction.Building, MySession.Static.LocalCharacterEntityId))
            {
                return false;
            }
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid == null)
            {
                goto TR_000A;
            }
            else
            {
                MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
                if (localHumanPlayer == null)
                {
                    return false;
                }
                long identityId = localHumanPlayer.Identity.IdentityId;
                flag2 = false;
                bool flag3 = false;
                IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(identityId);
                if (faction != null)
                {
                    flag3 = faction.IsLeader(identityId);
                }
                if (!MySession.Static.IsUserAdmin(localHumanPlayer.Id.SteamId))
                {
                    if (grid.BigOwners.Count != 0)
                    {
                        foreach (long num2 in grid.BigOwners)
                        {
                            if (num2 == identityId)
                            {
                                flag2 = true;
                            }
                            else
                            {
                                if (MySession.Static.Players.TryGetIdentity(num2) == null)
                                {
                                    continue;
                                }
                                if (!flag3)
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
                                flag2 = true;
                            }
                            break;
                        }
                    }
                    else
                    {
                        flag2 = true;
                    }
                }
                else
                {
                    flag2 = true;
                }
            }
            if (!flag2)
            {
                MyHud.Notifications.Add(MyNotificationSingletons.DeletePermissionFailed);
                return false;
            }
        TR_000A:
            flag = false;
            if (grid != null)
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                bool cutGroup = !MyInput.Static.IsAnyShiftKeyPressed();
                bool cutOverLg = MyInput.Static.IsAnyAltKeyPressed();
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAreYouSureToDeleteGrid), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum v) {
                    if (v == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        this.OnDeleteConfirm(entity as MyCubeGrid, cutGroup, cutOverLg);
                    }
                    VRageMath.Vector4? color = null;
                    Vector3? inflateAmount = null;
                    MyStringId? lineMaterial = null;
                    Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(entity, false, color, 0.01f, inflateAmount, lineMaterial);
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                flag = true;
            }
            else if (entity is MyVoxelMap)
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAreYouSureToDeleteGrid), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum v) {
                    if (v == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        this.OnDeleteAsteroidConfirm(entity as MyVoxelMap);
                    }
                    VRageMath.Vector4? color = null;
                    Vector3? inflateAmount = null;
                    MyStringId? lineMaterial = null;
                    Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(entity, false, color, 0.01f, inflateAmount, lineMaterial);
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                flag = true;
            }
            else if (entity is MyFloatingObject)
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAreYouSureToDeleteGrid), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum v) {
                    if (v == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        this.OnCutFloatingObjectConfirm(entity as MyFloatingObject);
                    }
                    VRageMath.Vector4? color = null;
                    Vector3? inflateAmount = null;
                    MyStringId? lineMaterial = null;
                    Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(entity, false, color, 0.01f, inflateAmount, lineMaterial);
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                flag = true;
            }
            return flag;
        }

        private bool HandleEscape()
        {
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape))
            {
                bool flag = false;
                if (m_clipboard.IsActive)
                {
                    m_clipboard.Deactivate(false);
                    this.UpdatePasteNotification(MyCommonTexts.CubeBuilderPasteNotification);
                    flag = true;
                }
                if (this.m_floatingObjectClipboard.IsActive)
                {
                    this.m_floatingObjectClipboard.Deactivate();
                    this.UpdatePasteNotification(MyCommonTexts.CubeBuilderPasteNotification);
                    flag = true;
                }
                if (this.m_voxelClipboard.IsActive)
                {
                    this.m_voxelClipboard.Deactivate();
                    this.UpdatePasteNotification(MyCommonTexts.CubeBuilderPasteNotification);
                    flag = true;
                }
                if (flag)
                {
                    this.Deactivate();
                    return true;
                }
            }
            return false;
        }

        public bool HandleGameInput()
        {
            MyStringId nullOrEmpty;
            this.m_rotationHintRotating = false;
            if (MyGuiScreenGamePlay.DisableInput)
            {
                return false;
            }
            if (!this.m_activated || !(MySession.Static.ControlledEntity is MyCharacter))
            {
                nullOrEmpty = MyStringId.NullOrEmpty;
            }
            else
            {
                nullOrEmpty = MySession.Static.ControlledEntity.ControlContext;
            }
            MyStringId context = nullOrEmpty;
            bool flag = true;
            if (MySession.Static.ControlledEntity != null)
            {
                flag &= MySessionComponentSafeZones.IsActionAllowed((VRage.Game.Entity.MyEntity) MySession.Static.ControlledEntity, MySafeZoneAction.Building, 0L);
            }
            if (!MySession.Static.IsCopyPastingEnabled && !MySession.Static.CreativeMode)
            {
                if (SpectatorIsBuilding)
                {
                }
            }
            else
            {
                if (MySession.Static.IsCopyPastingEnabled && !(MySession.Static.ControlledEntity is MyShipController))
                {
                    if (this.HandleCopyInput())
                    {
                        return true;
                    }
                    if (flag)
                    {
                        if (this.HandleDeleteInput())
                        {
                            return true;
                        }
                        if (this.HandleCutInput())
                        {
                            return true;
                        }
                        if (this.HandlePasteInput(false))
                        {
                            return true;
                        }
                        if (this.HandleMouseScrollInput(context))
                        {
                            return true;
                        }
                    }
                }
                if (this.HandleEscape())
                {
                    return true;
                }
                if (!flag)
                {
                    return false;
                }
                if (this.HandleLeftMouseButton(context))
                {
                    return true;
                }
            }
            if (!flag)
            {
                return false;
            }
            if (this.HandleBlueprintInput())
            {
                return true;
            }
            if ((!MySession.Static.IsCopyPastingEnabled && (!(MySession.Static.ControlledEntity is MyShipController) && (MyInput.Static.IsNewKeyPressed(MyKeys.V) && MyInput.Static.IsAnyCtrlKeyPressed()))) && !MyInput.Static.IsAnyShiftKeyPressed())
            {
                ShowCannotPasteWarning();
            }
            if (((m_clipboard != null) && m_clipboard.IsActive) && (MyControllerHelper.IsControl(context, MyControlsSpace.FREE_ROTATION, MyControlStateType.NEW_PRESSED, false) || MyControllerHelper.IsControl(context, MyControlsSpace.SWITCH_BUILDING_MODE, MyControlStateType.NEW_PRESSED, false)))
            {
                m_clipboard.EnableStationRotation = !m_clipboard.EnableStationRotation;
                this.m_floatingObjectClipboard.EnableStationRotation = !this.m_floatingObjectClipboard.EnableStationRotation;
            }
            return this.HandleRotationInput(context);
        }

        private bool HandleLeftMouseButton(MyStringId context)
        {
            if (MyInput.Static.IsNewLeftMousePressed() || MyControllerHelper.IsControl(context, MyControlsSpace.COPY_PASTE_ACTION, MyControlStateType.NEW_PRESSED, false))
            {
                bool flag = false;
                if (m_clipboard.IsActive && m_clipboard.PasteGrid(true, true))
                {
                    this.UpdatePasteNotification(MyCommonTexts.CubeBuilderPasteNotification);
                    flag = true;
                }
                if (this.m_floatingObjectClipboard.IsActive && this.m_floatingObjectClipboard.PasteFloatingObject(null))
                {
                    this.UpdatePasteNotification(MyCommonTexts.CubeBuilderPasteNotification);
                    flag = true;
                }
                if (this.m_voxelClipboard.IsActive && this.m_voxelClipboard.PasteVoxelMap(null))
                {
                    this.UpdatePasteNotification(MyCommonTexts.CubeBuilderPasteNotification);
                    flag = true;
                }
                if (flag)
                {
                    this.Deactivate();
                    return true;
                }
            }
            return false;
        }

        private bool HandleMouseScrollInput(MyStringId context)
        {
            bool flag = MyInput.Static.IsAnyCtrlKeyPressed();
            if ((flag && (MyInput.Static.PreviousMouseScrollWheelValue() < MyInput.Static.MouseScrollWheelValue())) || MyControllerHelper.IsControl(context, MyControlsSpace.MOVE_FURTHER, MyControlStateType.PRESSED, false))
            {
                bool flag2 = false;
                if (m_clipboard.IsActive)
                {
                    m_clipboard.MoveEntityFurther();
                    flag2 = true;
                }
                if (this.m_floatingObjectClipboard.IsActive)
                {
                    this.m_floatingObjectClipboard.MoveEntityFurther();
                    flag2 = true;
                }
                if (this.m_voxelClipboard.IsActive)
                {
                    this.m_voxelClipboard.MoveEntityFurther();
                    flag2 = true;
                }
                return flag2;
            }
            if ((!flag || (MyInput.Static.PreviousMouseScrollWheelValue() <= MyInput.Static.MouseScrollWheelValue())) && !MyControllerHelper.IsControl(context, MyControlsSpace.MOVE_CLOSER, MyControlStateType.PRESSED, false))
            {
                return false;
            }
            bool flag3 = false;
            if (m_clipboard.IsActive)
            {
                m_clipboard.MoveEntityCloser();
                flag3 = true;
            }
            if (this.m_floatingObjectClipboard.IsActive)
            {
                this.m_floatingObjectClipboard.MoveEntityCloser();
                flag3 = true;
            }
            if (this.m_voxelClipboard.IsActive)
            {
                this.m_voxelClipboard.MoveEntityCloser();
                flag3 = true;
            }
            return flag3;
        }

        public bool HandlePasteInput(bool forcePaste = false)
        {
            if (forcePaste || ((MyInput.Static.IsNewKeyPressed(MyKeys.V) && MyInput.Static.IsAnyCtrlKeyPressed()) && !MyInput.Static.IsAnyShiftKeyPressed()))
            {
                bool flag = false;
                MySession.Static.GameFocusManager.Clear();
                if (m_clipboard.PasteGrid(true, !this.m_floatingObjectClipboard.HasCopiedFloatingObjects()))
                {
                    MySessionComponentVoxelHand.Static.Enabled = false;
                    this.UpdatePasteNotification(MyCommonTexts.CubeBuilderPasteNotification);
                    flag = true;
                }
                else if (this.m_floatingObjectClipboard.PasteFloatingObject(null))
                {
                    MySessionComponentVoxelHand.Static.Enabled = false;
                    this.UpdatePasteNotification(MyCommonTexts.CubeBuilderPasteNotification);
                    flag = true;
                }
                if (flag)
                {
                    if (this.m_activated)
                    {
                        this.Deactivate();
                    }
                    else
                    {
                        this.Activate();
                    }
                    return true;
                }
            }
            return false;
        }

        private bool HandleRotationInput(MyStringId context)
        {
            int frameDt = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastInputHandleTime;
            this.m_lastInputHandleTime += frameDt;
            if (this.m_activated)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (MyControllerHelper.IsControl(context, m_rotationControls[i], MyControlStateType.PRESSED, false))
                    {
                        bool newlyPressed = MyControllerHelper.IsControl(context, m_rotationControls[i], MyControlStateType.NEW_PRESSED, false);
                        int index = -1;
                        int sign = m_rotationDirections[i];
                        if (MyFakes.ENABLE_STANDARD_AXES_ROTATION)
                        {
                            int[] numArray = new int[] { 1, 1, 0, 0, 2, 2 };
                            if (this.m_rotationHints.RotationUpAxis != numArray[i])
                            {
                                return true;
                            }
                        }
                        if (i < 2)
                        {
                            index = this.m_rotationHints.RotationUpAxis;
                            sign *= this.m_rotationHints.RotationUpDirection;
                        }
                        if ((i >= 2) && (i < 4))
                        {
                            index = this.m_rotationHints.RotationRightAxis;
                            sign *= this.m_rotationHints.RotationRightDirection;
                        }
                        if (i >= 4)
                        {
                            index = this.m_rotationHints.RotationForwardAxis;
                            sign *= this.m_rotationHints.RotationForwardDirection;
                        }
                        if (index != -1)
                        {
                            this.m_rotationHintRotating |= !newlyPressed;
                            this.RotateAxis(index, sign, newlyPressed, frameDt);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
        }

        public override void InitFromDefinition(MySessionComponentDefinition definition)
        {
            base.InitFromDefinition(definition);
            MyClipboardDefinition definition2 = definition as MyClipboardDefinition;
            MyClipboardDefinition definition1 = definition2;
            if (m_clipboard == null)
            {
                m_definition = definition2;
                m_clipboard = new MyGridClipboard(m_definition.PastingSettings, true);
            }
        }

        public override void LoadData()
        {
            base.LoadData();
            Static = this;
        }

        private void OnCutAsteroidConfirm(MyVoxelMap targetVoxelMap)
        {
            if (Sandbox.Game.Entities.MyEntities.EntityExists(targetVoxelMap.EntityId))
            {
                this.DeactivateCopyPaste(true);
                this.DeactivateCopyPasteFloatingObject(true);
                Sandbox.Game.Entities.MyEntities.SendCloseRequest(targetVoxelMap);
            }
        }

        private void OnCutConfirm(MyCubeGrid targetGrid, bool cutGroup, bool cutOverLgs)
        {
            if (Sandbox.Game.Entities.MyEntities.EntityExists(targetGrid.EntityId))
            {
                this.DeactivateCopyPasteVoxel(true);
                this.DeactivateCopyPasteFloatingObject(true);
                if (cutGroup)
                {
                    m_clipboard.CutGroup(targetGrid, cutOverLgs ? GridLinkTypeEnum.Physical : GridLinkTypeEnum.Logical);
                }
                else
                {
                    m_clipboard.CutGrid(targetGrid);
                }
            }
        }

        private void OnCutFloatingObjectConfirm(MyFloatingObject floatingObj)
        {
            if (Sandbox.Game.Entities.MyEntities.Exist(floatingObj))
            {
                this.DeactivateCopyPasteVoxel(true);
                this.DeactivateCopyPaste(true);
                this.m_floatingObjectClipboard.CutFloatingObject(floatingObj);
            }
        }

        private void OnDeleteAsteroidConfirm(MyVoxelMap targetVoxelMap)
        {
            if (Sandbox.Game.Entities.MyEntities.EntityExists(targetVoxelMap.EntityId))
            {
                this.DeactivateCopyPaste(true);
                this.DeactivateCopyPasteFloatingObject(true);
                Sandbox.Game.Entities.MyEntities.SendCloseRequest(targetVoxelMap);
            }
        }

        private void OnDeleteConfirm(MyCubeGrid targetGrid, bool cutGroup, bool cutOverLgs)
        {
            if (Sandbox.Game.Entities.MyEntities.EntityExists(targetGrid.EntityId))
            {
                this.DeactivateCopyPasteVoxel(true);
                this.DeactivateCopyPasteFloatingObject(true);
                if (cutGroup)
                {
                    m_clipboard.DeleteGroup(targetGrid, cutOverLgs ? GridLinkTypeEnum.Physical : GridLinkTypeEnum.Logical);
                }
                else
                {
                    m_clipboard.DeleteGrid(targetGrid);
                }
            }
        }

        private void OnDeleteFloatingObjectConfirm(MyFloatingObject floatingObj)
        {
            if (Sandbox.Game.Entities.MyEntities.Exist(floatingObj))
            {
                this.DeactivateCopyPasteVoxel(true);
                this.DeactivateCopyPaste(true);
                this.m_floatingObjectClipboard.DeleteFloatingObject(floatingObj);
            }
        }

        public void OnLostFocus()
        {
            this.Deactivate();
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

        private void RemovePasteNotification()
        {
            if (this.m_pasteNotification != null)
            {
                MyHud.Notifications.Remove(this.m_pasteNotification);
                this.m_pasteNotification = null;
            }
        }

        private void RotateAxis(int index, int sign, bool newlyPressed, int frameDt)
        {
            float angleDelta = frameDt * this.BLOCK_ROTATION_SPEED;
            if (MyInput.Static.IsAnyCtrlKeyPressed())
            {
                if (!newlyPressed)
                {
                    return;
                }
                angleDelta = 1.570796f;
            }
            if (MyInput.Static.IsAnyAltKeyPressed())
            {
                if (!newlyPressed)
                {
                    return;
                }
                angleDelta = MathHelper.ToRadians((float) 1f);
            }
            if (m_clipboard.IsActive)
            {
                m_clipboard.RotateAroundAxis(index, sign, newlyPressed, angleDelta);
            }
            if (this.m_floatingObjectClipboard.IsActive)
            {
                this.m_floatingObjectClipboard.RotateAroundAxis(index, sign, newlyPressed, angleDelta);
            }
        }

        public static void ShowCannotPasteWarning()
        {
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.Blueprints_NoCreativeRightsMessage), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            if (m_clipboard != null)
            {
                m_clipboard.Deactivate(false);
            }
            if (this.m_floatingObjectClipboard != null)
            {
                this.m_floatingObjectClipboard.Deactivate();
            }
            if (this.m_voxelClipboard != null)
            {
                this.m_voxelClipboard.Deactivate();
            }
            Static = null;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (this.m_activated)
            {
                m_clipboard.Update();
                this.m_floatingObjectClipboard.Update();
                this.m_voxelClipboard.Update();
                if ((m_clipboard.IsActive || this.m_floatingObjectClipboard.IsActive) || this.m_voxelClipboard.IsActive)
                {
                    this.m_collisionTestPoints.Clear();
                    PrepareCharacterCollisionPoints(this.m_collisionTestPoints);
                    if (m_clipboard.IsActive)
                    {
                        m_clipboard.Show();
                    }
                    else
                    {
                        m_clipboard.Hide();
                    }
                    if (!this.m_floatingObjectClipboard.IsActive)
                    {
                        this.m_floatingObjectClipboard.Hide();
                    }
                    else
                    {
                        this.m_floatingObjectClipboard.Show();
                        this.m_floatingObjectClipboard.HideWhenColliding(this.m_collisionTestPoints);
                    }
                    if (this.m_voxelClipboard.IsActive)
                    {
                        this.m_voxelClipboard.Show();
                    }
                    else
                    {
                        this.m_voxelClipboard.Hide();
                    }
                }
                this.UpdateClipboards();
            }
        }

        private void UpdateBlueprintNotification(MyStringId text)
        {
            if (this.m_blueprintNotification != null)
            {
                MyHud.Notifications.Remove(this.m_blueprintNotification);
            }
            this.m_blueprintNotification = new MyHudNotification(text, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            MyHud.Notifications.Add(this.m_blueprintNotification);
        }

        private void UpdateClipboards()
        {
            if (m_clipboard.IsActive)
            {
                m_clipboard.CalculateRotationHints(this.m_rotationHints, this.m_rotationHintRotating);
            }
            else if (this.m_floatingObjectClipboard.IsActive)
            {
                this.m_floatingObjectClipboard.CalculateRotationHints(this.m_rotationHints, this.m_rotationHintRotating);
            }
        }

        private void UpdatePasteNotification(MyStringId myTextsWrapperEnum)
        {
            this.RemovePasteNotification();
            if (m_clipboard.IsActive)
            {
                this.m_pasteNotification = new MyHudNotification(myTextsWrapperEnum, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Control);
                MyHud.Notifications.Add(this.m_pasteNotification);
            }
        }

        public static MyClipboardDefinition ClipboardDefinition =>
            m_definition;

        public MyGridClipboard Clipboard =>
            m_clipboard;

        internal MyFloatingObjectClipboard FloatingObjectClipboard =>
            this.m_floatingObjectClipboard;

        internal MyVoxelClipboard VoxelClipboard =>
            this.m_voxelClipboard;

        public Vector3D FreePlacementTarget =>
            (MyBlockBuilderBase.IntersectionStart + (MyBlockBuilderBase.IntersectionDirection * this.IntersectionDistance));

        public bool IsActive =>
            this.m_activated;

        private static bool DeveloperSpectatorIsBuilding =>
            ((MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Spectator) && (!MySession.Static.SurvivalMode || MyInput.Static.ENABLE_DEVELOPER_KEYS));

        public static bool SpectatorIsBuilding =>
            (DeveloperSpectatorIsBuilding || AdminSpectatorIsBuilding);

        private static bool AdminSpectatorIsBuilding =>
            (MyFakes.ENABLE_ADMIN_SPECTATOR_BUILDING && ((MySession.Static != null) && ((MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Spectator) && ((MyMultiplayer.Static != null) && MySession.Static.IsUserAdmin(Sync.MyId)))));
    }
}

