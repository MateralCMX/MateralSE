namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.SessionComponents;
    using VRage.Input;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    [PreloadRequired, StaticEventOwner]
    public class MyGuiScreenAdminMenu : MyGuiScreenDebugBase
    {
        protected MyGuiControlLabel m_enabledCheckboxGlobalLabel;
        protected MyGuiControlLabel m_damageCheckboxGlobalLabel;
        protected MyGuiControlLabel m_shootingCheckboxGlobalLabel;
        protected MyGuiControlLabel m_drillingCheckboxGlobalLabel;
        protected MyGuiControlLabel m_weldingCheckboxGlobalLabel;
        protected MyGuiControlLabel m_grindingCheckboxGlobalLabel;
        protected MyGuiControlLabel m_voxelHandCheckboxGlobalLabel;
        protected MyGuiControlLabel m_buildingCheckboxGlobalLabel;
        protected MyGuiControlCheckbox m_enabledGlobalCheckbox;
        protected MyGuiControlCheckbox m_damageGlobalCheckbox;
        protected MyGuiControlCheckbox m_shootingGlobalCheckbox;
        protected MyGuiControlCheckbox m_drillingGlobalCheckbox;
        protected MyGuiControlCheckbox m_weldingGlobalCheckbox;
        protected MyGuiControlCheckbox m_grindingGlobalCheckbox;
        protected MyGuiControlCheckbox m_voxelHandGlobalCheckbox;
        protected MyGuiControlCheckbox m_buildingGlobalCheckbox;
        protected MyGuiControlLabel m_selectSafeZoneLabel;
        protected MyGuiControlLabel m_selectZoneShapeLabel;
        protected MyGuiControlLabel m_selectAxisLabel;
        protected MyGuiControlLabel m_zoneRadiusLabel;
        protected MyGuiControlLabel m_zoneSizeLabel;
        protected MyGuiControlLabel m_zoneRadiusValueLabel;
        protected MyGuiControlCombobox m_safeZonesCombo;
        protected MyGuiControlCombobox m_safeZonesTypeCombo;
        protected MyGuiControlCombobox m_safeZonesAxisCombo;
        protected MyGuiControlSlider m_sizeSlider;
        protected MyGuiControlSlider m_radiusSlider;
        protected MyGuiControlButton m_addSafeZoneButton;
        protected MyGuiControlButton m_repositionSafeZoneButton;
        protected MyGuiControlButton m_moveToSafeZoneButton;
        protected MyGuiControlButton m_removeSafeZoneButton;
        protected MyGuiControlButton m_renameSafeZoneButton;
        protected MyGuiControlButton m_configureFilterButton;
        protected MyGuiControlLabel m_enabledCheckboxLabel;
        protected MyGuiControlLabel m_damageCheckboxLabel;
        protected MyGuiControlLabel m_shootingCheckboxLabel;
        protected MyGuiControlLabel m_drillingCheckboxLabel;
        protected MyGuiControlLabel m_weldingCheckboxLabel;
        protected MyGuiControlLabel m_grindingCheckboxLabel;
        protected MyGuiControlLabel m_voxelHandCheckboxLabel;
        protected MyGuiControlLabel m_buildingCheckboxLabel;
        protected MyGuiControlCheckbox m_enabledCheckbox;
        protected MyGuiControlCheckbox m_damageCheckbox;
        protected MyGuiControlCheckbox m_shootingCheckbox;
        protected MyGuiControlCheckbox m_drillingCheckbox;
        protected MyGuiControlCheckbox m_weldingCheckbox;
        protected MyGuiControlCheckbox m_grindingCheckbox;
        protected MyGuiControlCheckbox m_voxelHandCheckbox;
        protected MyGuiControlCheckbox m_buildingCheckbox;
        private MySafeZone m_selectedSafeZone;
        private bool m_recreateInProgress;
        private static readonly float TEXT_ALIGN_CONST = 0.05f;
        private static readonly Vector2 CB_OFFSET = new Vector2(-0.05f, 0f);
        private static MyGuiScreenAdminMenu m_static;
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.4f, 1.2f);
        private static readonly float HIDDEN_PART_RIGHT = 0.04f;
        private readonly Vector2 m_controlPadding;
        private readonly float m_textScale;
        protected static MyEntityCyclingOrder m_order;
        private static float m_metricValue = 0f;
        private static long m_entityId;
        private static bool m_showMedbayNotification = true;
        private long m_attachCamera;
        private MyGuiControlLabel m_errorLabel;
        private MyGuiControlLabel m_labelCurrentIndex;
        private MyGuiControlLabel m_labelEntityName;
        private MyGuiControlLabel m_labelNumVisible;
        protected MyGuiControlButton m_removeItemButton;
        private MyGuiControlButton m_depowerItemButton;
        protected MyGuiControlButton m_stopItemButton;
        protected MyGuiControlCheckbox m_onlySmallGridsCheckbox;
        private MyGuiControlCheckbox m_onlyLargeGridsCheckbox;
        private static CyclingOptions m_cyclingOptions = new CyclingOptions();
        protected VRageMath.Vector4 m_labelColor;
        protected MyGuiControlCheckbox m_creativeCheckbox;
        private readonly List<IMyGps> m_gpsList;
        protected MyGuiControlCombobox m_modeCombo;
        protected MyGuiControlCheckbox m_invulnerableCheckbox;
        protected MyGuiControlCheckbox m_untargetableCheckbox;
        protected MyGuiControlCheckbox m_showPlayersCheckbox;
        protected MyGuiControlCheckbox m_keepOriginalOwnershipOnPasteCheckBox;
        protected MyGuiControlCheckbox m_canUseTerminals;
        protected MyGuiControlSlider m_timeDelta;
        protected MyGuiControlLabel m_timeDeltaValue;
        protected MyGuiControlListbox m_entityListbox;
        protected MyGuiControlCombobox m_entityTypeCombo;
        protected MyGuiControlCombobox m_entitySortCombo;
        private MyEntityList.MyEntityTypeEnum m_selectedType;
        private MyEntityList.MyEntitySortOrder m_selectedSort;
        private static bool m_invertOrder;
        private static bool m_damageHandler;
        private static HashSet<long> m_protectedCharacters = new HashSet<long>();
        private static MyPageEnum m_currentPage;
        private int m_currentGpsIndex;
        private bool m_unsavedTrashSettings;
        private AdminSettings m_newSettings;
        private bool m_unsavedTrashExitBoxIsOpened;
        private MyGuiControlTabControl m_trashTabControls;
        private MyGuiControlTabPage m_trashTab_General;
        private MyGuiControlTabPage m_trashTab_Voxel;
        private MyGuiControlTextbox m_textboxBlockCount;
        private MyGuiControlTextbox m_textboxDistanceTrash;
        private MyGuiControlTextbox m_textboxLogoutAgeTrash;
        private MyGuiControlTextbox m_textboxCharacterRemovalTrash;
        private MyGuiControlTextbox m_textboxOptimalGridCount;
        private MyGuiControlTextbox m_textboxVoxelPlayerDistanceTrash;
        private MyGuiControlTextbox m_textboxVoxelGridDistanceTrash;
        private MyGuiControlTextbox m_textboxVoxelAgeTrash;
        private TrashTab m_trashTabSelected;

        public MyGuiScreenAdminMenu() : base(new Vector2((MyGuiManager.GetMaxMouseCoord().X - (SCREEN_SIZE.X * 0.5f)) + HIDDEN_PART_RIGHT, 0.5f), new Vector2?(SCREEN_SIZE), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), false)
        {
            this.m_controlPadding = new Vector2(0.02f, 0.02f);
            this.m_textScale = 0.8f;
            this.m_labelColor = Color.White.ToVector4();
            this.m_gpsList = new List<IMyGps>();
            base.m_backgroundTransition = MySandboxGame.Config.UIBkOpacity;
            base.m_guiTransition = MySandboxGame.Config.UIOpacity;
            if (Sync.IsServer)
            {
                this.CreateScreen();
            }
            else
            {
                m_static = this;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent(x => new Action(MyGuiScreenAdminMenu.RequestSettingFromServer_Implementation), targetEndpoint, position);
            }
            MySessionComponentSafeZones.OnAddSafeZone += new EventHandler(this.MySafeZones_OnAddSafeZone);
            MySessionComponentSafeZones.OnRemoveSafeZone += new EventHandler(this.MySafeZones_OnRemoveSafeZone);
        }

        private void AddCharacter(MyGuiControlButton obj)
        {
            MyCharacterInputComponent.SpawnCharacter(null);
        }

        private void AddSeparator()
        {
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList {
                Size = new Vector2(1f, 0.01f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP
            };
            VRageMath.Vector4? color = null;
            control.AddHorizontal(Vector2.Zero, 1f, 0f, color);
            this.Controls.Add(control);
        }

        [Event(null, 0x79f), Reliable, Server]
        private static void AdminSettingsChanged(AdminSettingsEnum settings, ulong steamId)
        {
            if ((MySession.Static.OnlineMode == MyOnlineModeEnum.OFFLINE) || ((((settings & AdminSettingsEnum.AdminOnly) <= AdminSettingsEnum.None) || MySession.Static.IsUserAdmin(steamId)) && MySession.Static.IsUserModerator(steamId)))
            {
                MySession.Static.RemoteAdminSettings[steamId] = settings;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<AdminSettingsEnum, ulong>(x => new Action<AdminSettingsEnum, ulong>(MyGuiScreenAdminMenu.AdminSettingsChangedClient), settings, steamId, targetEndpoint, position);
            }
            else
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
        }

        [Event(null, 0x7af), Reliable, BroadcastExcept]
        private static void AdminSettingsChangedClient(AdminSettingsEnum settings, ulong steamId)
        {
            MySession.Static.RemoteAdminSettings[steamId] = settings;
        }

        private void BuildingCheckChanged(MyGuiControlCheckbox checkBox)
        {
            if ((this.m_selectedSafeZone != null) && !this.m_recreateInProgress)
            {
                this.m_selectedSafeZone.AllowedActions = !checkBox.IsChecked ? (this.m_selectedSafeZone.AllowedActions & ~MySafeZoneAction.Building) : (this.m_selectedSafeZone.AllowedActions | MySafeZoneAction.Building);
                this.RequestUpdateSafeZone();
            }
        }

        private void BuildingCheckGlobalChanged(MyGuiControlCheckbox checkBox)
        {
            MySessionComponentSafeZones.AllowedActions = !checkBox.IsChecked ? (MySessionComponentSafeZones.AllowedActions & ~MySafeZoneAction.Building) : (MySessionComponentSafeZones.AllowedActions | MySafeZoneAction.Building);
            MySessionComponentSafeZones.RequestUpdateGlobalSafeZone();
        }

        private void ChangeSkin(MyGuiControlButton obj)
        {
            MyGuiScreenAssetModifier screen = new MyGuiScreenAssetModifier(MySession.Static.LocalCharacter);
            MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
            MyGuiSandbox.AddScreen(screen);
        }

        private bool CheckAndStoreTrashTextboxChanges()
        {
            int num;
            float num2;
            float num3;
            int num4;
            int num5;
            float num6;
            float num7;
            int num8;
            if (((this.m_textboxBlockCount == null) || ((this.m_textboxDistanceTrash == null) || ((this.m_textboxLogoutAgeTrash == null) || ((this.m_textboxCharacterRemovalTrash == null) || ((this.m_textboxOptimalGridCount == null) || ((this.m_textboxVoxelPlayerDistanceTrash == null) || (this.m_textboxVoxelGridDistanceTrash == null))))))) || (this.m_textboxVoxelAgeTrash == null))
            {
                return false;
            }
            int.TryParse(this.m_textboxBlockCount.Text, out num);
            float.TryParse(this.m_textboxDistanceTrash.Text, out num2);
            float.TryParse(this.m_textboxLogoutAgeTrash.Text, out num3);
            int.TryParse(this.m_textboxCharacterRemovalTrash.Text, out num4);
            int.TryParse(this.m_textboxOptimalGridCount.Text, out num5);
            float.TryParse(this.m_textboxVoxelPlayerDistanceTrash.Text, out num6);
            float.TryParse(this.m_textboxVoxelGridDistanceTrash.Text, out num7);
            int.TryParse(this.m_textboxVoxelAgeTrash.Text, out num8);
            bool flag = (((((((MySession.Static.Settings.BlockCountThreshold != num) | !(MySession.Static.Settings.PlayerDistanceThreshold == num2)) | !(MySession.Static.Settings.PlayerInactivityThreshold == num3)) | (MySession.Static.Settings.PlayerCharacterRemovalThreshold != num4)) | (MySession.Static.Settings.OptimalGridCount != num5)) | !(MySession.Static.Settings.VoxelPlayerDistanceThreshold == num6)) | !(MySession.Static.Settings.VoxelGridDistanceThreshold == num7)) | (MySession.Static.Settings.VoxelAgeThreshold != num8);
            this.m_newSettings.blockCount = num;
            this.m_newSettings.playerDistance = num2;
            this.m_newSettings.playerInactivity = num3;
            this.m_newSettings.characterRemovalThreshold = num4;
            this.m_newSettings.gridCount = num5;
            this.m_newSettings.voxelDistanceFromPlayer = num6;
            this.m_newSettings.voxelDistanceFromGrid = num7;
            this.m_newSettings.voxelAge = num8;
            this.m_unsavedTrashSettings |= flag;
            return flag;
        }

        private void CircleGps(bool reset, bool forward)
        {
            this.m_onlyLargeGridsCheckbox.Enabled = false;
            this.m_onlySmallGridsCheckbox.Enabled = false;
            this.m_depowerItemButton.Enabled = false;
            this.m_removeItemButton.Enabled = false;
            this.m_stopItemButton.Enabled = false;
            if (((MySession.Static != null) && (MySession.Static.Gpss != null)) && (MySession.Static.LocalHumanPlayer != null))
            {
                this.m_currentGpsIndex = !forward ? (this.m_currentGpsIndex + 1) : (this.m_currentGpsIndex - 1);
                this.m_gpsList.Clear();
                MySession.Static.Gpss.GetGpsList(MySession.Static.LocalPlayerId, this.m_gpsList);
                if (this.m_gpsList.Count == 0)
                {
                    this.m_currentGpsIndex = 0;
                }
                else
                {
                    if (this.m_currentGpsIndex < 0)
                    {
                        this.m_currentGpsIndex = this.m_gpsList.Count - 1;
                    }
                    if ((this.m_gpsList.Count <= this.m_currentGpsIndex) | reset)
                    {
                        this.m_currentGpsIndex = 0;
                    }
                    IMyGps gps = this.m_gpsList[this.m_currentGpsIndex];
                    Vector3D coords = gps.Coords;
                    this.m_labelEntityName.TextToDraw.Clear();
                    this.m_labelEntityName.TextToDraw.Append(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_EntityName));
                    this.m_labelEntityName.TextToDraw.Append(string.IsNullOrEmpty(gps.Name) ? "-" : gps.Name);
                    this.m_labelCurrentIndex.TextToDraw.Clear().AppendFormat(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_CurrentValue), this.m_currentGpsIndex);
                    Vector3D? position = null;
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, position);
                    Vector3D? nullable = MyEntities.FindFreePlace(coords + Vector3D.One, 2f, 30, 5, 1f);
                    MySpectatorCameraController.Static.Position = (nullable != null) ? nullable.Value : (coords + Vector3D.One);
                    MySpectatorCameraController.Static.Target = coords;
                }
            }
        }

        public override bool CloseScreen()
        {
            m_static = null;
            MySessionComponentSafeZones.OnAddSafeZone -= new EventHandler(this.MySafeZones_OnAddSafeZone);
            MySessionComponentSafeZones.OnRemoveSafeZone -= new EventHandler(this.MySafeZones_OnRemoveSafeZone);
            return base.CloseScreen();
        }

        private unsafe void ConstructTrashTab_General(MyGuiControlTabPage page)
        {
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            this.CreateTrashCheckBoxes(page);
            VRageMath.Vector4? color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.192f), base.m_size.Value.X * 0.73f, 0f, color);
            Vector2? size = base.GetSize();
            float y = base.m_currentPosition.Y;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(-0.161f, base.m_currentPosition.Y + TEXT_ALIGN_CONST);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_WithBlockCount);
            MyGuiControlLabel label = label1;
            label.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_WithBlockCount_Tooltip));
            page.Controls.Add(label);
            base.m_currentPosition.Y = y;
            this.m_textboxBlockCount = base.AddTextbox(MySession.Static.Settings.BlockCountThreshold.ToString(), new Action<MyGuiControlTextbox>(this.OnBlockCountChanged), new VRageMath.Vector4?(this.m_labelColor), 0.9f, MyGuiControlTextboxType.DigitsOnly, null, "Debug", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false);
            page.Controls.Add(this.m_textboxBlockCount);
            this.m_textboxBlockCount.Size = new Vector2(0.07f, this.m_textboxBlockCount.Size.Y);
            this.m_textboxBlockCount.PositionX = ((base.m_currentPosition.X + size.Value.X) - this.m_textboxBlockCount.Size.X) - 0.045f;
            this.m_textboxBlockCount.PositionY = base.m_currentPosition.Y;
            this.m_textboxBlockCount.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_WithBlockCount_Tooltip));
            MyGuiControlLabel label6 = new MyGuiControlLabel();
            label6.Position = new Vector2(-0.161f, base.m_currentPosition.Y + TEXT_ALIGN_CONST);
            label6.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label6.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_DistanceFromPlayer);
            MyGuiControlLabel label2 = label6;
            label2.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_DistanceFromPlayer_Tooltip));
            page.Controls.Add(label2);
            this.m_textboxDistanceTrash = base.AddTextbox(MySession.Static.Settings.PlayerDistanceThreshold.ToString(), new Action<MyGuiControlTextbox>(this.OnDistanceChanged), new VRageMath.Vector4?(this.m_labelColor), 0.9f, MyGuiControlTextboxType.DigitsOnly, null, "Debug", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false);
            page.Controls.Add(this.m_textboxDistanceTrash);
            this.m_textboxDistanceTrash.Size = new Vector2(0.07f, this.m_textboxDistanceTrash.Size.Y);
            this.m_textboxDistanceTrash.PositionX = ((base.m_currentPosition.X + size.Value.X) - this.m_textboxDistanceTrash.Size.X) - 0.045f;
            this.m_textboxDistanceTrash.PositionY = base.m_currentPosition.Y;
            this.m_textboxDistanceTrash.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_DistanceFromPlayer_Tooltip));
            MyGuiControlLabel label7 = new MyGuiControlLabel();
            label7.Position = new Vector2(-0.161f, base.m_currentPosition.Y + TEXT_ALIGN_CONST);
            label7.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label7.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_PlayerLogoutAge);
            MyGuiControlLabel label3 = label7;
            label3.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_PlayerLogoutAge_Tooltip));
            page.Controls.Add(label3);
            this.m_textboxLogoutAgeTrash = base.AddTextbox(MySession.Static.Settings.PlayerInactivityThreshold.ToString(), new Action<MyGuiControlTextbox>(this.OnLoginAgeChanged), new VRageMath.Vector4?(this.m_labelColor), 0.9f, MyGuiControlTextboxType.DigitsOnly, null, "Debug", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false);
            page.Controls.Add(this.m_textboxLogoutAgeTrash);
            this.m_textboxLogoutAgeTrash.Size = new Vector2(0.07f, this.m_textboxLogoutAgeTrash.Size.Y);
            this.m_textboxLogoutAgeTrash.PositionX = ((base.m_currentPosition.X + size.Value.X) - this.m_textboxLogoutAgeTrash.Size.X) - 0.045f;
            this.m_textboxLogoutAgeTrash.PositionY = base.m_currentPosition.Y;
            this.m_textboxLogoutAgeTrash.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_PlayerLogoutAge_Tooltip));
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.025f;
            MyGuiControlLabel label8 = new MyGuiControlLabel();
            label8.Position = new Vector2(-0.161f, base.m_currentPosition.Y + TEXT_ALIGN_CONST);
            label8.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label8.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_OptimalGridCount);
            MyGuiControlLabel label4 = label8;
            label4.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_OptimalGridCount_Tooltip));
            page.Controls.Add(label4);
            this.m_textboxOptimalGridCount = base.AddTextbox(MySession.Static.Settings.OptimalGridCount.ToString(), new Action<MyGuiControlTextbox>(this.OnOptimalGridCountChanged), new VRageMath.Vector4?(this.m_labelColor), 0.9f, MyGuiControlTextboxType.DigitsOnly, null, "Debug", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false);
            page.Controls.Add(this.m_textboxOptimalGridCount);
            this.m_textboxOptimalGridCount.Size = new Vector2(0.07f, this.m_textboxOptimalGridCount.Size.Y);
            this.m_textboxOptimalGridCount.PositionY = base.m_currentPosition.Y;
            this.m_textboxOptimalGridCount.PositionX = ((base.m_currentPosition.X + size.Value.X) - this.m_textboxOptimalGridCount.Size.X) - 0.045f;
            this.m_textboxOptimalGridCount.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_OptimalGridCount_Tooltip));
            MyGuiControlLabel label9 = new MyGuiControlLabel();
            label9.Position = new Vector2(-0.161f, base.m_currentPosition.Y + TEXT_ALIGN_CONST);
            label9.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label9.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_PlayerCharacterRemoval);
            MyGuiControlLabel label5 = label9;
            label5.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_PlayerCharacterRemoval_Tooltip));
            page.Controls.Add(label5);
            this.m_textboxCharacterRemovalTrash = base.AddTextbox(MySession.Static.Settings.PlayerCharacterRemovalThreshold.ToString(), new Action<MyGuiControlTextbox>(this.OnCharacterRemovalChanged), new VRageMath.Vector4?(this.m_labelColor), 0.9f, MyGuiControlTextboxType.DigitsOnly, null, "Debug", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false);
            page.Controls.Add(this.m_textboxCharacterRemovalTrash);
            this.m_textboxCharacterRemovalTrash.Size = new Vector2(0.07f, this.m_textboxCharacterRemovalTrash.Size.Y);
            this.m_textboxCharacterRemovalTrash.PositionX = ((base.m_currentPosition.X + size.Value.X) - this.m_textboxCharacterRemovalTrash.Size.X) - 0.045f;
            this.m_textboxCharacterRemovalTrash.PositionY = base.m_currentPosition.Y;
            this.m_textboxCharacterRemovalTrash.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_PlayerCharacterRemoval_Tooltip));
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.06f;
            MyGuiControlButton button2 = this.CreateDebugButton(0.14f, MyCommonTexts.ScreenDebugAdminMenu_SubmitChangesButton, new Action<MyGuiControlButton>(this.OnSubmitButtonClicked), true, new MyStringId?(MyCommonTexts.ScreenDebugAdminMenu_SubmitChangesButtonTooltip), true, false);
            button2.PositionX = -0.088f;
            page.Controls.Add(button2);
            MyStringId? tooltip = null;
            MyGuiControlButton button3 = this.CreateDebugButton(0.14f, MyCommonTexts.ScreenDebugAdminMenu_CancleChangesButton, new Action<MyGuiControlButton>(this.OnCancelButtonClicked), true, tooltip, true, false);
            button3.PositionX = 0.055f;
            button3.PositionY -= 0.0435f;
            page.Controls.Add(button3);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] -= 0.02175f;
            Vector2 currentPosition = base.m_currentPosition;
            tooltip = null;
            MyGuiControlButton button = this.CreateDebugButton(0.284f, MySpaceTexts.ScreenDebugAdminMenu_RemoveFloating, new Action<MyGuiControlButton>(this.OnRemoveFloating), true, tooltip, true, false);
            button.PositionX += 0.003f;
            page.Controls.Add(button);
            this.UpdateCyclingAndDepower();
            MyGuiControlButton button4 = this.CreateDebugButton(0.14f, !MySession.Static.Settings.TrashRemovalEnabled ? MyCommonTexts.ScreenDebugAdminMenu_ResumeTrashButton : MyCommonTexts.ScreenDebugAdminMenu_PauseTrashButton, new Action<MyGuiControlButton>(this.OnTrashButtonClicked), true, new MyStringId?(MyCommonTexts.ScreenDebugAdminMenu_PauseTrashButtonTooltip), true, false);
            button4.PositionX = -0.088f;
            page.Controls.Add(button4);
            tooltip = null;
            MyGuiControlButton button5 = this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_StopAll, new Action<MyGuiControlButton>(this.OnStopEntities), true, tooltip, true, false);
            button5.PositionX = 0.055f;
            button5.PositionY -= 0.0435f;
            page.Controls.Add(button5);
            page.Controls.Add(control);
        }

        private unsafe void ConstructTrashTab_Voxel(MyGuiControlTabPage page)
        {
            Vector2? size = base.GetSize();
            this.CreateVoxelTrashCheckBoxes(page);
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(-0.161f, base.m_currentPosition.Y + TEXT_ALIGN_CONST);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_VoxelDistanceFromPlayer);
            MyGuiControlLabel control = label1;
            control.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_VoxelDistanceFromPlayer_Tooltip));
            page.Controls.Add(control);
            this.m_textboxVoxelPlayerDistanceTrash = base.AddTextbox(MySession.Static.Settings.VoxelPlayerDistanceThreshold.ToString(), new Action<MyGuiControlTextbox>(this.OnDistanceChanged), new VRageMath.Vector4?(this.m_labelColor), 0.9f, MyGuiControlTextboxType.DigitsOnly, null, "Debug", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false);
            page.Controls.Add(this.m_textboxVoxelPlayerDistanceTrash);
            this.m_textboxVoxelPlayerDistanceTrash.Size = new Vector2(0.07f, this.m_textboxVoxelPlayerDistanceTrash.Size.Y);
            this.m_textboxVoxelPlayerDistanceTrash.PositionX = ((base.m_currentPosition.X + size.Value.X) - this.m_textboxVoxelPlayerDistanceTrash.Size.X) - 0.045f;
            this.m_textboxVoxelPlayerDistanceTrash.PositionY = base.m_currentPosition.Y;
            this.m_textboxVoxelPlayerDistanceTrash.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_VoxelDistanceFromPlayer_Tooltip));
            MyGuiControlLabel label4 = new MyGuiControlLabel();
            label4.Position = new Vector2(-0.161f, base.m_currentPosition.Y + TEXT_ALIGN_CONST);
            label4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label4.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_VoxelDistanceFromGrid);
            MyGuiControlLabel label2 = label4;
            label2.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_VoxelDistanceFromGrid_Tooltip));
            page.Controls.Add(label2);
            this.m_textboxVoxelGridDistanceTrash = base.AddTextbox(MySession.Static.Settings.VoxelGridDistanceThreshold.ToString(), new Action<MyGuiControlTextbox>(this.OnDistanceChanged), new VRageMath.Vector4?(this.m_labelColor), 0.9f, MyGuiControlTextboxType.DigitsOnly, null, "Debug", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false);
            page.Controls.Add(this.m_textboxVoxelGridDistanceTrash);
            this.m_textboxVoxelGridDistanceTrash.Size = new Vector2(0.07f, this.m_textboxVoxelGridDistanceTrash.Size.Y);
            this.m_textboxVoxelGridDistanceTrash.PositionX = ((base.m_currentPosition.X + size.Value.X) - this.m_textboxVoxelGridDistanceTrash.Size.X) - 0.045f;
            this.m_textboxVoxelGridDistanceTrash.PositionY = base.m_currentPosition.Y;
            this.m_textboxVoxelGridDistanceTrash.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_VoxelDistanceFromGrid_Tooltip));
            MyGuiControlLabel label5 = new MyGuiControlLabel();
            label5.Position = new Vector2(-0.161f, base.m_currentPosition.Y + TEXT_ALIGN_CONST);
            label5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label5.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_VoxelAge);
            MyGuiControlLabel label3 = label5;
            label3.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_VoxelAge_Tooltip));
            page.Controls.Add(label3);
            this.m_textboxVoxelAgeTrash = base.AddTextbox(MySession.Static.Settings.VoxelAgeThreshold.ToString(), new Action<MyGuiControlTextbox>(this.OnDistanceChanged), new VRageMath.Vector4?(this.m_labelColor), 0.9f, MyGuiControlTextboxType.DigitsOnly, null, "Debug", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, false);
            page.Controls.Add(this.m_textboxVoxelAgeTrash);
            this.m_textboxVoxelAgeTrash.Size = new Vector2(0.07f, this.m_textboxVoxelAgeTrash.Size.Y);
            this.m_textboxVoxelAgeTrash.PositionX = ((base.m_currentPosition.X + size.Value.X) - this.m_textboxVoxelAgeTrash.Size.X) - 0.045f;
            this.m_textboxVoxelAgeTrash.PositionY = base.m_currentPosition.Y;
            this.m_textboxVoxelAgeTrash.SetTooltip(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_VoxelAge_Tooltip));
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.06f;
            MyGuiControlButton button = this.CreateDebugButton(0.14f, MyCommonTexts.ScreenDebugAdminMenu_SubmitChangesButton, new Action<MyGuiControlButton>(this.OnSubmitButtonClicked), true, new MyStringId?(MyCommonTexts.ScreenDebugAdminMenu_SubmitChangesButtonTooltip), true, false);
            button.PositionX = -0.088f;
            page.Controls.Add(button);
            MyStringId? tooltip = null;
            MyGuiControlButton button2 = this.CreateDebugButton(0.14f, MyCommonTexts.ScreenDebugAdminMenu_CancleChangesButton, new Action<MyGuiControlButton>(this.OnCancelButtonClicked), true, tooltip, true, false);
            button2.PositionX = 0.055f;
            button2.PositionY -= 0.0435f;
            page.Controls.Add(button2);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] -= 0.0435f;
            MyGuiControlButton button3 = this.CreateDebugButton(0.14f, !MySession.Static.Settings.VoxelTrashRemovalEnabled ? MyCommonTexts.ScreenDebugAdminMenu_ResumeTrashButton : MyCommonTexts.ScreenDebugAdminMenu_PauseTrashButton, new Action<MyGuiControlButton>(this.OnTrashVoxelButtonClicked), true, new MyStringId?(MyCommonTexts.ScreenDebugAdminMenu_PauseTrashVoxelButtonTooltip), true, false);
            button3.PositionX = -0.088f;
            page.Controls.Add(button3);
        }

        private MyGuiControlButton CreateDebugButton(float usableWidth, MyStringId text, Action<MyGuiControlButton> onClick, bool enabled = true, MyStringId? tooltip = new MyStringId?(), bool increaseSpacing = true, bool addToControls = true)
        {
            VRageMath.Vector4? textColor = null;
            Vector2? size = null;
            MyGuiControlButton button = base.AddButton(MyTexts.Get(text), onClick, null, textColor, size, increaseSpacing, addToControls);
            button.VisualStyle = MyGuiControlButtonStyleEnum.Rectangular;
            button.TextScale = base.m_scale;
            button.Size = new Vector2(usableWidth, button.Size.Y);
            button.Position += new Vector2(-HIDDEN_PART_RIGHT / 2f, 0f);
            button.Enabled = enabled;
            if (tooltip != null)
            {
                button.SetToolTip(tooltip.Value);
            }
            return button;
        }

        private void CreateScreen()
        {
            base.m_closeOnEsc = false;
            base.CanBeHidden = true;
            base.CanHideOthers = true;
            base.m_canCloseInCloseAllScreenCalls = true;
            base.m_canShareInput = true;
            base.m_isTopScreen = false;
            base.m_isTopMostScreen = false;
            this.StoreTrashSettings_RealToTmp();
            this.RecreateControls(true);
        }

        protected virtual void CreateSelectionCombo()
        {
            base.AddCombo<MyEntityCyclingOrder>(m_order, new Action<MyEntityCyclingOrder>(this.OnOrderChanged), true, 10, null, new VRageMath.Vector4?(this.m_labelColor));
        }

        private void CreateSlider(MyGuiControlList list, float usableWidth, float min, float max, ref MyGuiControlSlider slider)
        {
            float width = 400f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            float? defaultValue = null;
            VRageMath.Vector4? color = null;
            slider = new MyGuiControlSlider(new Vector2?(base.m_currentPosition), min, max, width, defaultValue, color, string.Empty, 4, 0.75f * base.m_scale, 0f, "Debug", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
            slider.DebugScale = base.m_sliderDebugScale;
            slider.ColorMask = Color.White.ToVector4();
            list.Controls.Add(slider);
        }

        private MyGuiControlLabel CreateSliderWithDescription(MyGuiControlList list, float usableWidth, float min, float max, string description, ref MyGuiControlSlider slider)
        {
            MyGuiControlLabel control = base.AddLabel(description, VRageMath.Vector4.One, base.m_scale, null, "Debug");
            this.Controls.Remove(control);
            list.Controls.Add(control);
            this.CreateSlider(list, usableWidth, min, max, ref slider);
            MyGuiControlLabel label2 = base.AddLabel("", VRageMath.Vector4.One, base.m_scale, null, "Debug");
            this.Controls.Remove(label2);
            list.Controls.Add(label2);
            return label2;
        }

        protected virtual unsafe void CreateTrashCheckBoxes(MyGuiControlTabPage page)
        {
            MyTrashRemovalFlags flagFixed = MyTrashRemovalFlags.Fixed;
            string str = string.Format(MySessionComponentTrash.GetName(flagFixed), string.Empty);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.025f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = str;
            MyGuiControlLabel control = label1;
            VRageMath.Vector4? color = null;
            MyGuiControlCheckbox checkbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                IsChecked = (MySession.Static.Settings.TrashFlags & flagFixed) == flagFixed,
                IsCheckedChanged = c => this.OnTrashFlagChanged(flagFixed, c.IsChecked)
            };
            page.Controls.Add(checkbox);
            page.Controls.Add(control);
            MyTrashRemovalFlags flagStationary = MyTrashRemovalFlags.Stationary;
            str = string.Format(MySessionComponentTrash.GetName(flagStationary), string.Empty);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.045f;
            MyGuiControlLabel label2 = new MyGuiControlLabel();
            label2.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label2.Text = str;
            control = label2;
            color = null;
            checkbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                IsChecked = (MySession.Static.Settings.TrashFlags & flagStationary) == flagStationary,
                IsCheckedChanged = c => this.OnTrashFlagChanged(flagStationary, c.IsChecked)
            };
            page.Controls.Add(checkbox);
            page.Controls.Add(control);
            MyTrashRemovalFlags flagLinear = MyTrashRemovalFlags.Linear;
            str = string.Format(MySessionComponentTrash.GetName(flagLinear), string.Empty);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.045f;
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label3.Text = str;
            control = label3;
            color = null;
            checkbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                IsChecked = (MySession.Static.Settings.TrashFlags & flagLinear) == flagLinear,
                IsCheckedChanged = c => this.OnTrashFlagChanged(flagLinear, c.IsChecked)
            };
            page.Controls.Add(checkbox);
            page.Controls.Add(control);
            MyTrashRemovalFlags flagAccelerating = MyTrashRemovalFlags.Accelerating;
            str = string.Format(MySessionComponentTrash.GetName(flagAccelerating), string.Empty);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.045f;
            MyGuiControlLabel label4 = new MyGuiControlLabel();
            label4.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label4.Text = str;
            control = label4;
            color = null;
            checkbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                IsChecked = (MySession.Static.Settings.TrashFlags & flagAccelerating) == flagAccelerating,
                IsCheckedChanged = c => this.OnTrashFlagChanged(flagAccelerating, c.IsChecked)
            };
            page.Controls.Add(checkbox);
            page.Controls.Add(control);
            MyTrashRemovalFlags flagPowered = MyTrashRemovalFlags.Powered;
            str = string.Format(MySessionComponentTrash.GetName(flagPowered), string.Empty);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += 0.045f;
            MyGuiControlLabel label5 = new MyGuiControlLabel();
            label5.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label5.Text = str;
            control = label5;
            color = null;
            checkbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                IsChecked = (MySession.Static.Settings.TrashFlags & flagPowered) == flagPowered,
                IsCheckedChanged = c => this.OnTrashFlagChanged(flagPowered, c.IsChecked)
            };
            page.Controls.Add(checkbox);
            page.Controls.Add(control);
            MyTrashRemovalFlags flagControlled = MyTrashRemovalFlags.Controlled;
            str = string.Format(MySessionComponentTrash.GetName(flagControlled), string.Empty);
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += 0.045f;
            MyGuiControlLabel label6 = new MyGuiControlLabel();
            label6.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label6.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label6.Text = str;
            control = label6;
            color = null;
            checkbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                IsChecked = (MySession.Static.Settings.TrashFlags & flagControlled) == flagControlled,
                IsCheckedChanged = c => this.OnTrashFlagChanged(flagControlled, c.IsChecked)
            };
            page.Controls.Add(checkbox);
            page.Controls.Add(control);
            MyTrashRemovalFlags flagWithProduction = MyTrashRemovalFlags.WithProduction;
            str = string.Format(MySessionComponentTrash.GetName(flagWithProduction), string.Empty);
            float* singlePtr7 = (float*) ref base.m_currentPosition.Y;
            singlePtr7[0] += 0.045f;
            MyGuiControlLabel label7 = new MyGuiControlLabel();
            label7.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label7.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label7.Text = str;
            control = label7;
            color = null;
            checkbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                IsChecked = (MySession.Static.Settings.TrashFlags & flagWithProduction) == flagWithProduction,
                IsCheckedChanged = c => this.OnTrashFlagChanged(flagWithProduction, c.IsChecked)
            };
            page.Controls.Add(checkbox);
            page.Controls.Add(control);
            MyTrashRemovalFlags flagWithMedBay = MyTrashRemovalFlags.WithMedBay;
            str = string.Format(MySessionComponentTrash.GetName(flagWithMedBay), string.Empty);
            float* singlePtr8 = (float*) ref base.m_currentPosition.Y;
            singlePtr8[0] += 0.045f;
            MyGuiControlLabel label8 = new MyGuiControlLabel();
            label8.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label8.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label8.Text = str;
            control = label8;
            color = null;
            checkbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                IsChecked = (MySession.Static.Settings.TrashFlags & flagWithMedBay) == flagWithMedBay,
                IsCheckedChanged = c => this.OnTrashFlagChanged(flagWithMedBay, c.IsChecked)
            };
            page.Controls.Add(checkbox);
            page.Controls.Add(control);
        }

        protected virtual unsafe void CreateVoxelTrashCheckBoxes(MyGuiControlTabPage page)
        {
            MyTrashRemovalFlags flagMat = MyTrashRemovalFlags.RevertMaterials;
            string str = string.Format(MySessionComponentTrash.GetName(flagMat), string.Empty);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.025f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = str;
            MyGuiControlLabel control = label1;
            VRageMath.Vector4? color = null;
            MyGuiControlCheckbox checkbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                IsChecked = (MySession.Static.Settings.TrashFlags & flagMat) == flagMat,
                IsCheckedChanged = c => this.OnTrashFlagChanged(flagMat, c.IsChecked)
            };
            page.Controls.Add(checkbox);
            page.Controls.Add(control);
            MyTrashRemovalFlags flagAst = MyTrashRemovalFlags.RevertAsteroids;
            str = string.Format(MySessionComponentTrash.GetName(flagAst), string.Empty);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.045f;
            MyGuiControlLabel label2 = new MyGuiControlLabel();
            label2.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label2.Text = str;
            control = label2;
            color = null;
            checkbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                IsChecked = (MySession.Static.Settings.TrashFlags & flagAst) == flagAst,
                IsCheckedChanged = c => this.OnTrashFlagChanged(flagAst, c.IsChecked)
            };
            page.Controls.Add(checkbox);
            page.Controls.Add(control);
            MyTrashRemovalFlags flagFloat = MyTrashRemovalFlags.RevertWithFloatingsPresent;
            str = string.Format(MySessionComponentTrash.GetName(flagFloat), string.Empty);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.045f;
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label3.Text = str;
            control = label3;
            color = null;
            checkbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP) {
                IsChecked = (MySession.Static.Settings.TrashFlags & flagFloat) == flagFloat,
                IsCheckedChanged = c => this.OnTrashFlagChanged(flagFloat, c.IsChecked)
            };
            page.Controls.Add(checkbox);
            page.Controls.Add(control);
        }

        [Event(null, 0x7e3), Reliable, Client]
        private static void Cycle_Implementation(float newMetricValue, long newEntityId, Vector3D position)
        {
            m_metricValue = newMetricValue;
            m_entityId = newEntityId;
            if ((m_entityId != 0) && !TryAttachCamera(m_entityId))
            {
                MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, new Vector3D?(position + (Vector3.One * 50f)));
            }
            MyGuiScreenAdminMenu firstScreenOfType = MyScreenManager.GetFirstScreenOfType<MyGuiScreenAdminMenu>();
            if (firstScreenOfType != null)
            {
                UpdateRemoveAndDepowerButton(firstScreenOfType, m_entityId);
                firstScreenOfType.m_attachCamera = m_entityId;
                firstScreenOfType.m_labelCurrentIndex.TextToDraw.Clear().AppendFormat(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_CurrentValue), (m_entityId == 0) ? "-" : m_metricValue.ToString());
            }
        }

        [Event(null, 0x70c), Reliable, Server]
        private static void CycleRequest_Implementation(MyEntityCyclingOrder order, bool reset, bool findLarger, float metricValue, long currentEntityId, CyclingOptions options)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                Vector3D zero;
                if (reset)
                {
                    metricValue = float.MinValue;
                    currentEntityId = 0L;
                    findLarger = false;
                }
                MyEntityCycling.FindNext(order, ref metricValue, ref currentEntityId, findLarger, options);
                MyEntity entity = MyEntities.GetEntityByIdOrDefault(currentEntityId, null, false);
                if (entity == null)
                {
                    zero = Vector3D.Zero;
                }
                else
                {
                    zero = entity.WorldMatrix.Translation;
                }
                Vector3D position = zero;
                if (MyEventContext.Current.IsLocallyInvoked)
                {
                    Cycle_Implementation(metricValue, currentEntityId, position);
                }
                else
                {
                    Vector3D? nullable = null;
                    MyMultiplayer.RaiseStaticEvent<float, long, Vector3D>(x => new Action<float, long, Vector3D>(MyGuiScreenAdminMenu.Cycle_Implementation), metricValue, currentEntityId, position, MyEventContext.Current.Sender, nullable);
                }
            }
        }

        private void DamageCheckChanged(MyGuiControlCheckbox checkBox)
        {
            if ((this.m_selectedSafeZone != null) && !this.m_recreateInProgress)
            {
                this.m_selectedSafeZone.AllowedActions = !checkBox.IsChecked ? (this.m_selectedSafeZone.AllowedActions & ~MySafeZoneAction.Damage) : (this.m_selectedSafeZone.AllowedActions | MySafeZoneAction.Damage);
                this.RequestUpdateSafeZone();
            }
        }

        private void DamageCheckGlobalChanged(MyGuiControlCheckbox checkBox)
        {
            MySessionComponentSafeZones.AllowedActions = !checkBox.IsChecked ? (MySessionComponentSafeZones.AllowedActions & ~MySafeZoneAction.Damage) : (MySessionComponentSafeZones.AllowedActions | MySafeZoneAction.Damage);
            MySessionComponentSafeZones.RequestUpdateGlobalSafeZone();
        }

        private void DeleteRecordings(MyGuiControlButton obj)
        {
            MySessionComponentReplay.Static.DeleteRecordings();
        }

        [Event(null, 0x805), Reliable, Client]
        private static void DownloadSettingFromServer(AdminSettings settings)
        {
            MySession.Static.Settings.TrashFlags = settings.flags;
            MySession.Static.Settings.TrashRemovalEnabled = settings.enable;
            MySession.Static.Settings.BlockCountThreshold = settings.blockCount;
            MySession.Static.Settings.PlayerDistanceThreshold = settings.playerDistance;
            MySession.Static.Settings.OptimalGridCount = settings.gridCount;
            MySession.Static.Settings.PlayerInactivityThreshold = settings.playerInactivity;
            MySession.Static.Settings.PlayerCharacterRemovalThreshold = settings.characterRemovalThreshold;
            MySession.Static.Settings.VoxelPlayerDistanceThreshold = settings.voxelDistanceFromPlayer;
            MySession.Static.Settings.VoxelGridDistanceThreshold = settings.voxelDistanceFromGrid;
            MySession.Static.Settings.VoxelAgeThreshold = settings.voxelAge;
            MySession.Static.Settings.VoxelTrashRemovalEnabled = settings.voxelEnable;
            MySession.Static.AdminSettings = settings.AdminSettingsFlags;
            if (m_static != null)
            {
                m_static.CreateScreen();
            }
        }

        public override bool Draw() => 
            base.Draw();

        private void DrillingCheckChanged(MyGuiControlCheckbox checkBox)
        {
            if ((this.m_selectedSafeZone != null) && !this.m_recreateInProgress)
            {
                this.m_selectedSafeZone.AllowedActions = !checkBox.IsChecked ? (this.m_selectedSafeZone.AllowedActions & ~MySafeZoneAction.Drilling) : (this.m_selectedSafeZone.AllowedActions | MySafeZoneAction.Drilling);
                this.RequestUpdateSafeZone();
            }
        }

        private void DrillingCheckGlobalChanged(MyGuiControlCheckbox checkBox)
        {
            MySessionComponentSafeZones.AllowedActions = !checkBox.IsChecked ? (MySessionComponentSafeZones.AllowedActions & ~MySafeZoneAction.Drilling) : (MySessionComponentSafeZones.AllowedActions | MySafeZoneAction.Drilling);
            MySessionComponentSafeZones.RequestUpdateGlobalSafeZone();
        }

        private void EnabledCheckedChanged(MyGuiControlCheckbox checkBox)
        {
            if ((this.m_selectedSafeZone != null) && !this.m_recreateInProgress)
            {
                if (this.m_selectedSafeZone.Enabled != checkBox.IsChecked)
                {
                    this.m_selectedSafeZone.Enabled = checkBox.IsChecked;
                    this.m_selectedSafeZone.RecreateBillboards();
                }
                this.RequestUpdateSafeZone();
            }
        }

        private void EntityListItemClicked(MyGuiControlListbox myGuiControlListbox)
        {
            if (myGuiControlListbox.SelectedItems.Count > 0)
            {
                MyEntityList.MyEntityListInfoItem userData = (MyEntityList.MyEntityListInfoItem) myGuiControlListbox.SelectedItems[myGuiControlListbox.SelectedItems.Count - 1].UserData;
                this.m_attachCamera = userData.EntityId;
                if (!TryAttachCamera(userData.EntityId))
                {
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, new Vector3D?(userData.Position + (Vector3.One * 50f)));
                }
            }
        }

        [Event(null, 0x6ec), Reliable, Server]
        private static void EntityListRequest(MyEntityList.MyEntityTypeEnum selectedType)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                List<MyEntityList.MyEntityListInfoItem> entityList = MyEntityList.GetEntityList(selectedType);
                if (MyEventContext.Current.IsLocallyInvoked)
                {
                    EntityListResponse(entityList);
                }
                else
                {
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<List<MyEntityList.MyEntityListInfoItem>>(x => new Action<List<MyEntityList.MyEntityListInfoItem>>(MyGuiScreenAdminMenu.EntityListResponse), entityList, MyEventContext.Current.Sender, position);
                }
            }
        }

        [Event(null, 0x7b9), Reliable, Client]
        private static void EntityListResponse(List<MyEntityList.MyEntityListInfoItem> entities)
        {
            MyGuiScreenSafeZoneFilter firstScreenOfType = MyScreenManager.GetFirstScreenOfType<MyGuiScreenSafeZoneFilter>();
            if (firstScreenOfType != null)
            {
                MyGuiControlListbox entityListbox = firstScreenOfType.m_entityListbox;
                entityListbox.Items.Clear();
                MyEntityList.SortEntityList(MyEntityList.MyEntitySortOrder.DisplayName, ref entities, m_invertOrder);
                foreach (MyEntityList.MyEntityListInfoItem item in entities)
                {
                    if (!firstScreenOfType.m_selectedSafeZone.Entities.Contains(item.EntityId))
                    {
                        StringBuilder text = MyEntityList.GetFormattedDisplayName(MyEntityList.MyEntitySortOrder.DisplayName, item, true);
                        entityListbox.Items.Add(new MyGuiControlListbox.Item(text, null, null, item.EntityId, null));
                    }
                }
            }
            else
            {
                MyGuiScreenAdminMenu @static = m_static;
                if (@static != null)
                {
                    int num1;
                    MyGuiControlListbox entityListbox = @static.m_entityListbox;
                    entityListbox.Items.Clear();
                    MyEntityList.SortEntityList(@static.m_selectedSort, ref entities, m_invertOrder);
                    if ((@static.m_selectedType == MyEntityList.MyEntityTypeEnum.Grids) || (@static.m_selectedType == MyEntityList.MyEntityTypeEnum.LargeGrids))
                    {
                        num1 = 1;
                    }
                    else
                    {
                        num1 = (int) (@static.m_selectedType == MyEntityList.MyEntityTypeEnum.SmallGrids);
                    }
                    bool isGrid = (bool) num1;
                    foreach (MyEntityList.MyEntityListInfoItem item2 in entities)
                    {
                        StringBuilder text = MyEntityList.GetFormattedDisplayName(@static.m_selectedSort, item2, isGrid);
                        entityListbox.Items.Add(new MyGuiControlListbox.Item(text, MyEntityList.GetDescriptionText(item2, isGrid), null, item2, null));
                    }
                }
            }
        }

        public void ExitButtonPressed()
        {
            if (m_currentPage != MyPageEnum.TrashRemoval)
            {
                this.CloseScreen();
            }
            else
            {
                this.CheckAndStoreTrashTextboxChanges();
                if (!this.m_unsavedTrashSettings)
                {
                    this.CloseScreen();
                }
                else if (!this.m_unsavedTrashExitBoxIsOpened)
                {
                    this.m_unsavedTrashExitBoxIsOpened = true;
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.ScreenDebugAdminMenu_UnsavedTrash), null, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.FinishTrashUnsavedExiting), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }
        }

        private void FinishTrashSetting(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (result == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                this.StoreTrashSettings_TmpToReal();
                this.m_unsavedTrashSettings = false;
                this.RecalcTrash();
                this.RecreateControls(false);
            }
        }

        private void FinishTrashUnsavedExiting(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (result == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                this.StoreTrashSettings_RealToTmp();
                this.CloseScreen();
            }
            this.m_unsavedTrashExitBoxIsOpened = false;
        }

        private void FinishTrashUnsavedTabChange(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (result != MyGuiScreenMessageBox.ResultEnum.YES)
            {
                this.m_modeCombo.SelectItemByKey((long) m_currentPage, true);
            }
            else
            {
                this.StoreTrashSettings_RealToTmp();
                this.NewTabSelected();
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenAdminMenu";

        private void GrindingCheckChanged(MyGuiControlCheckbox checkBox)
        {
            if ((this.m_selectedSafeZone != null) && !this.m_recreateInProgress)
            {
                this.m_selectedSafeZone.AllowedActions = !checkBox.IsChecked ? (this.m_selectedSafeZone.AllowedActions & ~MySafeZoneAction.Grinding) : (this.m_selectedSafeZone.AllowedActions | MySafeZoneAction.Grinding);
                this.RequestUpdateSafeZone();
            }
        }

        private void GrindingCheckGlobalChanged(MyGuiControlCheckbox checkBox)
        {
            MySessionComponentSafeZones.AllowedActions = !checkBox.IsChecked ? (MySessionComponentSafeZones.AllowedActions & ~MySafeZoneAction.Grinding) : (MySessionComponentSafeZones.AllowedActions | MySafeZoneAction.Grinding);
            MySessionComponentSafeZones.RequestUpdateGlobalSafeZone();
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if ((MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.A)) && ReferenceEquals(base.FocusedControl, this.m_entityListbox))
            {
                this.m_entityListbox.SelectedItems.Clear();
                this.m_entityListbox.SelectedItems.AddRange(this.m_entityListbox.Items);
            }
            if ((MyInput.Static.IsNewKeyPressed(MyKeys.Escape) || (MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.MAIN_MENU, MyControlStateType.NEW_PRESSED, false) || ((base.m_defaultJoystickCancelUse && MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.CANCEL, MyControlStateType.NEW_PRESSED, false)) || (MyInput.Static.IsNewKeyPressed(MyKeys.F12) || MyInput.Static.IsNewKeyPressed(MyKeys.F11))))) || MyInput.Static.IsNewKeyPressed(MyKeys.F10))
            {
                this.ExitButtonPressed();
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.SPECTATOR_NONE))
            {
                this.SelectNextCharacter();
            }
        }

        private void m_safeZonesAxisCombo_ItemSelected()
        {
            if (this.m_selectedSafeZone != null)
            {
                if (this.m_safeZonesAxisCombo.GetSelectedIndex() == 0)
                {
                    this.m_zoneRadiusValueLabel.Text = this.m_selectedSafeZone.Size.X.ToString();
                }
                else if (this.m_safeZonesAxisCombo.GetSelectedIndex() == 1)
                {
                    this.m_zoneRadiusValueLabel.Text = this.m_selectedSafeZone.Size.Y.ToString();
                }
                else if (this.m_safeZonesAxisCombo.GetSelectedIndex() == 2)
                {
                    this.m_zoneRadiusValueLabel.Text = this.m_selectedSafeZone.Size.Z.ToString();
                }
            }
        }

        private void m_safeZonesCombo_ItemSelected()
        {
            this.m_selectedSafeZone = (MySafeZone) MyEntities.GetEntityById(this.m_safeZonesCombo.GetItemByIndex(this.m_safeZonesCombo.GetSelectedIndex()).Key, false);
            this.UpdateZoneType();
            this.UpdateSelectedData();
        }

        private void m_safeZonesTypeCombo_ItemSelected()
        {
            if (this.m_selectedSafeZone.Shape != ((int) this.m_safeZonesTypeCombo.GetSelectedKey()))
            {
                this.m_selectedSafeZone.Shape = (MySafeZoneShape) ((int) this.m_safeZonesTypeCombo.GetSelectedKey());
                this.m_selectedSafeZone.RecreatePhysics(true);
                this.UpdateZoneType();
                this.RequestUpdateSafeZone();
            }
        }

        private void m_trashTabControls_OnPageChanged()
        {
            this.m_trashTabSelected = (TrashTab) this.m_trashTabControls.SelectedPage;
        }

        private void MySafeZones_OnAddSafeZone(object sender, System.EventArgs e)
        {
            this.m_selectedSafeZone = (MySafeZone) sender;
            if (m_currentPage == MyPageEnum.SafeZones)
            {
                this.m_recreateInProgress = true;
                this.RefreshSafeZones();
                this.UpdateSelectedData();
                this.m_recreateInProgress = false;
            }
        }

        private void MySafeZones_OnRemoveSafeZone(object sender, System.EventArgs e)
        {
            if (this.m_safeZonesCombo != null)
            {
                if (ReferenceEquals(this.m_selectedSafeZone, sender))
                {
                    this.m_selectedSafeZone = null;
                    this.RefreshSafeZones();
                    this.m_selectedSafeZone = (this.m_safeZonesCombo.GetItemsCount() > 0) ? ((MySafeZone) MyEntities.GetEntityById(this.m_safeZonesCombo.GetItemByIndex(this.m_safeZonesCombo.GetItemsCount() - 1).Key, false)) : null;
                    this.m_recreateInProgress = true;
                    this.UpdateSelectedData();
                    this.m_recreateInProgress = false;
                }
                else
                {
                    this.m_safeZonesCombo.RemoveItem(((MySafeZone) sender).EntityId);
                }
            }
        }

        private void NewTabSelected()
        {
            m_currentPage = (MyPageEnum) ((int) this.m_modeCombo.GetSelectedKey());
            this.RecreateControls(false);
        }

        private void OnAddSafeZone()
        {
            MySessionComponentSafeZones.RequestCreateSafeZone(MySector.MainCamera.Position + (2f * MySector.MainCamera.ForwardVector));
        }

        private void OnBlockCountChanged(MyGuiControlTextbox textbox)
        {
            int num;
            if (int.TryParse(textbox.Text, out num))
            {
                this.m_newSettings.blockCount = num;
                if (this.m_newSettings.blockCount != MySession.Static.Settings.BlockCountThreshold)
                {
                    this.m_unsavedTrashSettings = true;
                }
            }
        }

        private void OnCancelButtonClicked(MyGuiControlButton obj)
        {
            this.StoreTrashSettings_RealToTmp();
            this.RecreateControls(false);
            this.m_unsavedTrashSettings = false;
        }

        private void OnCharacterRemovalChanged(MyGuiControlTextbox textbox)
        {
            int num;
            if (int.TryParse(textbox.Text, out num))
            {
                this.m_newSettings.characterRemovalThreshold = num;
                if (this.m_newSettings.characterRemovalThreshold != MySession.Static.Settings.PlayerCharacterRemovalThreshold)
                {
                    this.m_unsavedTrashSettings = true;
                }
            }
        }

        private void OnConfigureFilter()
        {
            if (this.m_selectedSafeZone != null)
            {
                MySafeZone selectedSafeZone = this.m_selectedSafeZone;
                MyScreenManager.AddScreen(new MyGuiScreenSafeZoneFilter(new Vector2(0.5f, 0.5f), selectedSafeZone));
            }
        }

        private void OnCycleClicked(bool reset, bool forward)
        {
            if (m_order == MyEntityCyclingOrder.Gps)
            {
                this.CircleGps(reset, forward);
            }
            else
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<MyEntityCyclingOrder, bool, bool, float, long, CyclingOptions>(x => new Action<MyEntityCyclingOrder, bool, bool, float, long, CyclingOptions>(MyGuiScreenAdminMenu.CycleRequest_Implementation), m_order, reset, forward, m_metricValue, m_entityId, m_cyclingOptions, targetEndpoint, position);
            }
        }

        private void OnDistanceChanged(MyGuiControlTextbox textbox)
        {
            float num;
            if (float.TryParse(textbox.Text, out num))
            {
                this.m_newSettings.playerDistance = num;
                if (this.m_newSettings.playerDistance != MySession.Static.Settings.PlayerDistanceThreshold)
                {
                    this.m_unsavedTrashSettings = true;
                }
            }
        }

        private void OnEnableAdminModeChanged(MyGuiControlCheckbox checkbox)
        {
            MySession.Static.EnableCreativeTools(Sync.MyId, checkbox.IsChecked);
        }

        private void OnEntityListActionClicked(MyEntityList.EntityListAction action)
        {
            List<long> list = new List<long>();
            List<MyGuiControlListbox.Item> list2 = new List<MyGuiControlListbox.Item>();
            using (List<MyGuiControlListbox.Item>.Enumerator enumerator = this.m_entityListbox.SelectedItems.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyGuiControlListbox.Item current = enumerator.Current;
                    if (this.ValidCharacter(((MyEntityList.MyEntityListInfoItem) current.UserData).EntityId))
                    {
                        list.Add(((MyEntityList.MyEntityListInfoItem) current.UserData).EntityId);
                        list2.Add(current);
                        continue;
                    }
                    return;
                }
            }
            if (action == MyEntityList.EntityListAction.Remove)
            {
                this.m_entityListbox.SelectedItems.Clear();
                foreach (MyGuiControlListbox.Item item2 in list2)
                {
                    this.m_entityListbox.Items.Remove(item2);
                }
                this.m_entityListbox.ScrollToolbarToTop();
                using (List<long>.Enumerator enumerator2 = list.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        MyEntity entity;
                        if (!MyEntities.TryGetEntityById(enumerator2.Current, out entity, false))
                        {
                            continue;
                        }
                        MyVoxelBase base2 = entity as MyVoxelBase;
                        if ((base2 != null) && !base2.SyncFlag)
                        {
                            base2.Close();
                        }
                    }
                }
            }
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<List<long>, MyEntityList.EntityListAction>(x => new Action<List<long>, MyEntityList.EntityListAction>(MyGuiScreenAdminMenu.ProceedEntitiesAction_Implementation), list, action, targetEndpoint, position);
        }

        private void OnEntityOperationClicked(MyEntityList.EntityListAction action)
        {
            if ((this.m_attachCamera != 0) && this.ValidCharacter(this.m_attachCamera))
            {
                MyEntity entity;
                if (MyEntities.TryGetEntityById(this.m_attachCamera, out entity, false))
                {
                    MyVoxelBase base2 = entity as MyVoxelBase;
                    if (base2 != null)
                    {
                        MyEntities.SendCloseRequest(base2);
                        return;
                    }
                }
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, MyEntityList.EntityListAction>(x => new Action<long, MyEntityList.EntityListAction>(MyGuiScreenAdminMenu.ProceedEntity_Implementation), this.m_attachCamera, action, targetEndpoint, position);
            }
        }

        private void OnInvulnerableChanged(MyGuiControlCheckbox checkbox)
        {
            if (checkbox.IsChecked)
            {
                MySession @static = MySession.Static;
                @static.AdminSettings |= AdminSettingsEnum.Invulnerable;
            }
            else
            {
                MySession @static = MySession.Static;
                @static.AdminSettings &= ~AdminSettingsEnum.Invulnerable;
            }
            this.RaiseAdminSettingsChanged();
        }

        private void OnKeepOwnershipChanged(MyGuiControlCheckbox checkbox)
        {
            if (checkbox.IsChecked)
            {
                MySession @static = MySession.Static;
                @static.AdminSettings |= AdminSettingsEnum.KeepOriginalOwnershipOnPaste;
            }
            else
            {
                MySession @static = MySession.Static;
                @static.AdminSettings &= ~AdminSettingsEnum.KeepOriginalOwnershipOnPaste;
            }
            this.RaiseAdminSettingsChanged();
        }

        private void OnLargeGridChanged(MyGuiControlCheckbox checkbox)
        {
            m_cyclingOptions.OnlyLargeGrids = checkbox.IsChecked;
            if (m_cyclingOptions.OnlyLargeGrids)
            {
                this.m_onlySmallGridsCheckbox.IsChecked = false;
            }
        }

        private void OnLoginAgeChanged(MyGuiControlTextbox textbox)
        {
            float num;
            if (float.TryParse(textbox.Text, out num))
            {
                this.m_newSettings.playerInactivity = num;
                if (this.m_newSettings.playerInactivity != MySession.Static.Settings.PlayerInactivityThreshold)
                {
                    this.m_unsavedTrashSettings = true;
                }
            }
        }

        private void OnModeComboSelect()
        {
            if ((m_currentPage != MyPageEnum.TrashRemoval) || !this.m_unsavedTrashSettings)
            {
                this.NewTabSelected();
            }
            else if (m_currentPage != ((int) this.m_modeCombo.GetSelectedKey()))
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.ScreenDebugAdminMenu_UnsavedTrash), null, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.FinishTrashUnsavedTabChange), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OnMoveToSafeZone()
        {
            if ((this.m_selectedSafeZone != null) && (MySession.Static.ControlledEntity != null))
            {
                MyMultiplayer.TeleportControlledEntity(this.m_selectedSafeZone.PositionComp.WorldMatrix.Translation);
            }
        }

        private void OnOptimalGridCountChanged(MyGuiControlTextbox textbox)
        {
            int num;
            if (int.TryParse(textbox.Text, out num))
            {
                this.m_newSettings.gridCount = num;
                if (this.m_newSettings.gridCount != MySession.Static.Settings.OptimalGridCount)
                {
                    this.m_unsavedTrashSettings = true;
                }
            }
        }

        protected void OnOrderChanged(MyEntityCyclingOrder obj)
        {
            m_order = obj;
            this.UpdateSmallLargeGridSelection();
            this.UpdateCyclingAndDepower();
            this.OnCycleClicked(true, true);
        }

        private void OnPlayerControl(MyGuiControlButton obj)
        {
            this.m_attachCamera = 0L;
            MySessionComponentAnimationSystem.Static.EntitySelectedForDebug = null;
            MyGuiScreenGamePlay.SetCameraController();
        }

        private void OnRadiusChange(MyGuiControlSlider slider)
        {
            if ((this.m_selectedSafeZone != null) && !this.m_recreateInProgress)
            {
                this.m_zoneRadiusValueLabel.Text = slider.Value.ToString();
                this.m_selectedSafeZone.Radius = slider.Value;
                this.m_selectedSafeZone.RecreatePhysics(true);
                this.RequestUpdateSafeZone();
            }
        }

        private void OnRecordButtonPressed(MyGuiControlButton obj)
        {
            if (MySessionComponentReplay.Static != null)
            {
                if (!MySessionComponentReplay.Static.IsRecording)
                {
                    MySessionComponentReplay.Static.StartRecording();
                    MySessionComponentReplay.Static.StartReplay();
                    this.CloseScreen();
                }
                else
                {
                    MySessionComponentReplay.Static.StopRecording();
                    MySessionComponentReplay.Static.StopReplay();
                    this.RecreateControls(false);
                }
            }
        }

        private void OnRefreshButton(MyGuiControlButton obj)
        {
            this.RecreateControls(true);
        }

        private void OnRemoveFloating(MyGuiControlButton obj)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(x => new Action(MyGuiScreenAdminMenu.RemoveFloating_Implementation), targetEndpoint, position);
        }

        private void OnRemoveOwnerButton(MyGuiControlButton obj)
        {
            HashSet<long> source = new HashSet<long>();
            List<long> list = new List<long>();
            using (List<MyGuiControlListbox.Item>.Enumerator enumerator = this.m_entityListbox.SelectedItems.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyPlayer.PlayerId id;
                    MyStringId? nullable;
                    Vector2? nullable2;
                    MyPlayer player;
                    long owner = ((MyEntityList.MyEntityListInfoItem) enumerator.Current.UserData).Owner;
                    if (owner == 0)
                    {
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("No owner!"), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        continue;
                    }
                    if (((MySession.Static != null) && (MySession.Static.ControlledEntity != null)) && (owner == MySession.Static.ControlledEntity.ControllerInfo.Controller.Player.Identity.IdentityId))
                    {
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Cannot remove yourself!"), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        continue;
                    }
                    if ((!MySession.Static.Players.TryGetPlayerId(owner, out id) || !MySession.Static.Players.TryGetPlayerById(id, out player)) || !MySession.Static.Players.GetOnlinePlayers().Contains(player))
                    {
                        source.Add(owner);
                    }
                    else
                    {
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Cannot remove online player " + player.DisplayName + ", kick him first!"), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                }
            }
            List<MyGuiControlListbox.Item> list2 = new List<MyGuiControlListbox.Item>();
            foreach (MyGuiControlListbox.Item item in this.m_entityListbox.Items)
            {
                if (source.Contains(((MyEntityList.MyEntityListInfoItem) item.UserData).Owner))
                {
                    list2.Add(item);
                    list.Add(((MyEntityList.MyEntityListInfoItem) item.UserData).EntityId);
                }
            }
            this.m_entityListbox.SelectedItems.Clear();
            foreach (MyGuiControlListbox.Item item2 in list2)
            {
                this.m_entityListbox.Items.Remove(item2);
            }
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<List<long>, List<long>>(x => new Action<List<long>, List<long>>(MyGuiScreenAdminMenu.RemoveOwner_Implementation), source.ToList<long>(), list, targetEndpoint, position);
        }

        private void OnRemoveSafeZone()
        {
            if (this.m_selectedSafeZone != null)
            {
                MySessionComponentSafeZones.RequestDeleteSafeZone(this.m_selectedSafeZone.EntityId);
                this.RequestUpdateSafeZone();
            }
        }

        private void OnRenameSafeZone()
        {
            if (this.m_selectedSafeZone != null)
            {
                MyScreenManager.AddScreen(new MyGuiBlueprintTextDialog(new Vector2(0.5f, 0.5f), delegate (string result) {
                    if (result != null)
                    {
                        this.m_selectedSafeZone.DisplayName = result;
                        this.RequestUpdateSafeZone();
                        this.RefreshSafeZones();
                    }
                }, "New Name", MyTexts.GetString(MySpaceTexts.DetailScreen_Button_Rename), 50, 0.3f));
            }
        }

        private void OnReplayButtonPressed(MyGuiControlButton obj)
        {
            if (MySessionComponentReplay.Static != null)
            {
                if (MySessionComponentReplay.Static.IsReplaying)
                {
                    MySessionComponentReplay.Static.StopReplay();
                    this.RecreateControls(false);
                }
                else if (MySessionComponentReplay.Static.HasRecordedData)
                {
                    MySessionComponentReplay.Static.StartReplay();
                    this.CloseScreen();
                }
            }
        }

        private void OnReplicateEverything(MyGuiControlButton button)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(x => new Action(MyGuiScreenAdminMenu.ReplicateEverything_Implementation), targetEndpoint, position);
        }

        private void OnRepositionSafeZone()
        {
            if (this.m_selectedSafeZone != null)
            {
                this.m_selectedSafeZone.PositionComp.WorldMatrix = MySector.MainCamera.WorldMatrix;
                this.m_selectedSafeZone.RecreatePhysics(true);
                this.RequestUpdateSafeZone();
            }
        }

        private void OnShowPlayersChanged(MyGuiControlCheckbox checkbox)
        {
            if (checkbox.IsChecked)
            {
                MySession @static = MySession.Static;
                @static.AdminSettings |= AdminSettingsEnum.ShowPlayers;
            }
            else
            {
                MySession @static = MySession.Static;
                @static.AdminSettings &= ~AdminSettingsEnum.ShowPlayers;
            }
            this.RaiseAdminSettingsChanged();
        }

        private void OnSizeChange(MyGuiControlSlider slider)
        {
            if ((this.m_selectedSafeZone != null) && !this.m_recreateInProgress)
            {
                this.m_zoneRadiusValueLabel.Text = slider.Value.ToString();
                if (this.m_safeZonesAxisCombo.GetSelectedIndex() == 0)
                {
                    this.m_selectedSafeZone.Size = new Vector3(slider.Value, this.m_selectedSafeZone.Size.Y, this.m_selectedSafeZone.Size.Z);
                }
                else if (this.m_safeZonesAxisCombo.GetSelectedIndex() == 1)
                {
                    this.m_selectedSafeZone.Size = new Vector3(this.m_selectedSafeZone.Size.X, slider.Value, this.m_selectedSafeZone.Size.Z);
                }
                else if (this.m_safeZonesAxisCombo.GetSelectedIndex() == 2)
                {
                    this.m_selectedSafeZone.Size = new Vector3(this.m_selectedSafeZone.Size.X, this.m_selectedSafeZone.Size.Y, slider.Value);
                }
                this.m_selectedSafeZone.RecreatePhysics(true);
                this.RequestUpdateSafeZone();
            }
        }

        private void OnSmallGridChanged(MyGuiControlCheckbox checkbox)
        {
            m_cyclingOptions.OnlySmallGrids = checkbox.IsChecked;
            if (m_cyclingOptions.OnlySmallGrids && (this.m_onlyLargeGridsCheckbox != null))
            {
                this.m_onlyLargeGridsCheckbox.IsChecked = false;
            }
        }

        private void OnStopEntities(MyGuiControlButton myGuiControlButton)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(x => new Action(MyGuiScreenAdminMenu.StopEntities_Implementation), targetEndpoint, position);
        }

        private void OnSubmitButtonClicked(MyGuiControlButton obj)
        {
            this.CheckAndStoreTrashTextboxChanges();
            if ((MySession.Static.Settings.OptimalGridCount != 0) || (MySession.Static.Settings.OptimalGridCount == this.m_newSettings.gridCount))
            {
                this.FinishTrashSetting(MyGuiScreenMessageBox.ResultEnum.YES);
            }
            else
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.ScreenDebugAdminMenu_GridCountWarning), null, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.FinishTrashSetting), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OnTeleportButton(MyGuiControlButton obj)
        {
            if (!ReferenceEquals(MySession.Static.CameraController, MySession.Static.LocalCharacter))
            {
                MyMultiplayer.TeleportControlledEntity(MySpectatorCameraController.Static.Position);
            }
        }

        private void OnTrashButtonClicked(MyGuiControlButton obj)
        {
            MySession.Static.Settings.TrashRemovalEnabled = !MySession.Static.Settings.TrashRemovalEnabled;
            obj.Text = MySession.Static.Settings.TrashRemovalEnabled ? MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_PauseTrashButton) : MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_ResumeTrashButton);
            this.RecalcTrash();
        }

        private unsafe void OnTrashFlagChanged(MyTrashRemovalFlags flag, bool value)
        {
            if (((flag == MyTrashRemovalFlags.WithMedBay) & value) && m_showMedbayNotification)
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyScreenManager.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.ScreenDebugAdminMenu_MedbayNotification), null, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                m_showMedbayNotification = false;
            }
            if (value)
            {
                MyTrashRemovalFlags* flagsPtr1 = (MyTrashRemovalFlags*) ref this.m_newSettings.flags;
                *((int*) flagsPtr1) |= flag;
            }
            else
            {
                MyTrashRemovalFlags* flagsPtr2 = (MyTrashRemovalFlags*) ref this.m_newSettings.flags;
                *((int*) flagsPtr2) &= ~flag;
            }
            this.m_unsavedTrashSettings = true;
        }

        private void OnTrashVoxelButtonClicked(MyGuiControlButton obj)
        {
            MySession.Static.Settings.VoxelTrashRemovalEnabled = !MySession.Static.Settings.VoxelTrashRemovalEnabled;
            obj.Text = MySession.Static.Settings.VoxelTrashRemovalEnabled ? MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_PauseTrashButton) : MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_ResumeTrashButton);
            this.RecalcTrash();
        }

        private void OnUntargetableChanged(MyGuiControlCheckbox checkbox)
        {
            if (checkbox.IsChecked)
            {
                MySession @static = MySession.Static;
                @static.AdminSettings |= AdminSettingsEnum.Untargetable;
            }
            else
            {
                MySession @static = MySession.Static;
                @static.AdminSettings &= ~AdminSettingsEnum.Untargetable;
            }
            this.RaiseAdminSettingsChanged();
        }

        private void OnUseTerminalsChanged(MyGuiControlCheckbox checkbox)
        {
            if (checkbox.IsChecked)
            {
                MySession @static = MySession.Static;
                @static.AdminSettings |= AdminSettingsEnum.UseTerminals;
            }
            else
            {
                MySession @static = MySession.Static;
                @static.AdminSettings &= ~AdminSettingsEnum.UseTerminals;
            }
            this.RaiseAdminSettingsChanged();
        }

        [Event(null, 0x756), Server, Reliable]
        private static void ProceedEntitiesAction_Implementation(List<long> entityIds, MyEntityList.EntityListAction action)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                using (List<long>.Enumerator enumerator = entityIds.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyEntity entity;
                        if (!MyEntities.TryGetEntityById(enumerator.Current, out entity, false))
                        {
                            continue;
                        }
                        MyEntityList.ProceedEntityAction(entity, action);
                    }
                }
            }
        }

        [Event(null, 0x781), Reliable, Server]
        private static void ProceedEntity_Implementation(long entityId, MyEntityList.EntityListAction action)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyEntity entity;
                if (MyEntities.TryGetEntityById(entityId, out entity, false))
                {
                    MyEntityList.ProceedEntityAction(entity, action);
                }
            }
        }

        private void RaiseAdminSettingsChanged()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<AdminSettingsEnum, ulong>(x => new Action<AdminSettingsEnum, ulong>(MyGuiScreenAdminMenu.AdminSettingsChanged), MySession.Static.AdminSettings, Sync.MyId, targetEndpoint, position);
        }

        private void RecalcTrash()
        {
            if (!Sync.IsServer)
            {
                AdminSettings settings = new AdminSettings {
                    flags = MySession.Static.Settings.TrashFlags,
                    enable = MySession.Static.Settings.TrashRemovalEnabled,
                    blockCount = MySession.Static.Settings.BlockCountThreshold,
                    playerDistance = MySession.Static.Settings.PlayerDistanceThreshold,
                    gridCount = MySession.Static.Settings.OptimalGridCount,
                    playerInactivity = MySession.Static.Settings.PlayerInactivityThreshold,
                    characterRemovalThreshold = MySession.Static.Settings.PlayerCharacterRemovalThreshold,
                    voxelDistanceFromPlayer = MySession.Static.Settings.VoxelPlayerDistanceThreshold,
                    voxelDistanceFromGrid = MySession.Static.Settings.VoxelGridDistanceThreshold,
                    voxelAge = MySession.Static.Settings.VoxelAgeThreshold,
                    voxelEnable = MySession.Static.Settings.VoxelTrashRemovalEnabled
                };
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<AdminSettings>(x => new Action<AdminSettings>(MyGuiScreenAdminMenu.UploadSettingsToServer), settings, targetEndpoint, position);
            }
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            float y;
            int? nullable3;
            MyStringId? nullable4;
            base.RecreateControls(constructor);
            Vector2 controlPadding = new Vector2(0.02f, 0.02f);
            float num = 0.8f;
            float separatorSize = 0.01f;
            float x = (SCREEN_SIZE.X - HIDDEN_PART_RIGHT) - (controlPadding.X * 2f);
            float num4 = (SCREEN_SIZE.Y - 1f) / 2f;
            m_static = this;
            base.m_currentPosition = -base.m_size.Value / 2f;
            base.m_currentPosition += controlPadding;
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += num4;
            base.m_scale = num;
            base.AddCaption(MyTexts.Get(MySpaceTexts.ScreenDebugAdminMenu_ModeSelect).ToString(), new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(this.m_controlPadding + new Vector2(-HIDDEN_PART_RIGHT, num4 - 0.03f)), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.44f), base.m_size.Value.X * 0.73f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.365f), base.m_size.Value.X * 0.73f, 0f, color);
            this.Controls.Add(control);
            float* singlePtr2 = (float*) ref base.m_currentPosition.X;
            singlePtr2[0] += 0.018f;
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += (MyGuiConstants.SCREEN_CAPTION_DELTA_Y + controlPadding.Y) - 0.012f;
            color = null;
            Vector2? size = null;
            this.m_modeCombo = base.AddCombo(null, color, size, 10);
            if (!MySession.Static.IsUserSpaceMaster(Sync.MyId))
            {
                nullable3 = null;
                nullable4 = null;
                this.m_modeCombo.AddItem(0L, MySpaceTexts.ScreenDebugAdminMenu_AdminTools, nullable3, nullable4);
                m_currentPage = MyPageEnum.AdminTools;
                this.m_modeCombo.SelectItemByKey((long) m_currentPage, true);
            }
            else
            {
                nullable3 = null;
                nullable4 = null;
                this.m_modeCombo.AddItem(0L, MySpaceTexts.ScreenDebugAdminMenu_AdminTools, nullable3, nullable4);
                nullable3 = null;
                nullable4 = null;
                this.m_modeCombo.AddItem(2L, MyCommonTexts.ScreenDebugAdminMenu_CycleObjects, nullable3, nullable4);
                nullable3 = null;
                nullable4 = null;
                this.m_modeCombo.AddItem(1L, MySpaceTexts.ScreenDebugAdminMenu_Cleanup, nullable3, nullable4);
                nullable3 = null;
                nullable4 = null;
                this.m_modeCombo.AddItem(3L, MySpaceTexts.ScreenDebugAdminMenu_EntityList, nullable3, nullable4);
                if (!MySession.Static.IsUserAdmin(Sync.MyId))
                {
                    if (((m_currentPage == MyPageEnum.GlobalSafeZone) || (m_currentPage == MyPageEnum.SafeZones)) || (m_currentPage == MyPageEnum.ReplayTool))
                    {
                        m_currentPage = MyPageEnum.CycleObjects;
                    }
                }
                else
                {
                    nullable3 = null;
                    nullable4 = null;
                    this.m_modeCombo.AddItem(4L, MySpaceTexts.ScreenDebugAdminMenu_SafeZones, nullable3, nullable4);
                    nullable3 = null;
                    nullable4 = null;
                    this.m_modeCombo.AddItem(5L, MySpaceTexts.ScreenDebugAdminMenu_GlobalSafeZone, nullable3, nullable4);
                    nullable3 = null;
                    nullable4 = null;
                    this.m_modeCombo.AddItem(6L, MySpaceTexts.ScreenDebugAdminMenu_ReplayTool, nullable3, nullable4);
                }
                this.m_modeCombo.SelectItemByKey((long) m_currentPage, true);
            }
            this.m_modeCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnModeComboSelect);
            switch (m_currentPage)
            {
                case MyPageEnum.AdminTools:
                {
                    float* singlePtr12 = (float*) ref base.m_currentPosition.Y;
                    singlePtr12[0] += 0.03f;
                    MyGuiControlLabel label18 = new MyGuiControlLabel();
                    label18.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
                    label18.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label18.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_EnableAdminMode);
                    MyGuiControlLabel label4 = label18;
                    color = null;
                    this.m_creativeCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    this.m_creativeCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.OnEnableAdminModeChanged);
                    this.m_creativeCheckbox.SetToolTip(MyCommonTexts.ScreenDebugAdminMenu_EnableAdminMode_Tooltip);
                    this.m_creativeCheckbox.IsChecked = MySession.Static.CreativeToolsEnabled(Sync.MyId);
                    this.m_creativeCheckbox.Enabled = MySession.Static.HasCreativeRights;
                    this.Controls.Add(this.m_creativeCheckbox);
                    this.Controls.Add(label4);
                    float* singlePtr13 = (float*) ref base.m_currentPosition.Y;
                    singlePtr13[0] += 0.045f;
                    MyGuiControlLabel label19 = new MyGuiControlLabel();
                    label19.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
                    label19.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label19.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_Invulnerable);
                    MyGuiControlLabel label5 = label19;
                    color = null;
                    this.m_invulnerableCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    this.m_invulnerableCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.OnInvulnerableChanged);
                    this.m_invulnerableCheckbox.SetToolTip(MySpaceTexts.ScreenDebugAdminMenu_InvulnerableToolTip);
                    this.m_invulnerableCheckbox.IsChecked = MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.Invulnerable);
                    this.m_invulnerableCheckbox.Enabled = MySession.Static.IsUserAdmin(Sync.MyId);
                    this.Controls.Add(this.m_invulnerableCheckbox);
                    this.Controls.Add(label5);
                    float* singlePtr14 = (float*) ref base.m_currentPosition.Y;
                    singlePtr14[0] += 0.045f;
                    MyGuiControlLabel label20 = new MyGuiControlLabel();
                    label20.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
                    label20.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label20.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_Untargetable);
                    MyGuiControlLabel label6 = label20;
                    color = null;
                    this.m_untargetableCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    this.m_untargetableCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.OnUntargetableChanged);
                    this.m_untargetableCheckbox.SetToolTip(MySpaceTexts.ScreenDebugAdminMenu_UntargetableToolTip);
                    this.m_untargetableCheckbox.IsChecked = MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.Untargetable);
                    this.m_untargetableCheckbox.Enabled = MySession.Static.IsUserAdmin(Sync.MyId);
                    this.Controls.Add(this.m_untargetableCheckbox);
                    this.Controls.Add(label6);
                    float* singlePtr15 = (float*) ref base.m_currentPosition.Y;
                    singlePtr15[0] += 0.045f;
                    MyGuiControlLabel label21 = new MyGuiControlLabel();
                    label21.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
                    label21.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label21.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_ShowPlayers);
                    MyGuiControlLabel label7 = label21;
                    color = null;
                    this.m_showPlayersCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    this.m_showPlayersCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.OnShowPlayersChanged);
                    this.m_showPlayersCheckbox.SetToolTip(MySpaceTexts.ScreenDebugAdminMenu_ShowPlayersToolTip);
                    this.m_showPlayersCheckbox.IsChecked = MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.ShowPlayers);
                    this.m_showPlayersCheckbox.Enabled = MySession.Static.IsUserModerator(Sync.MyId);
                    this.Controls.Add(this.m_showPlayersCheckbox);
                    this.Controls.Add(label7);
                    float* singlePtr16 = (float*) ref base.m_currentPosition.Y;
                    singlePtr16[0] += 0.045f;
                    MyGuiControlLabel label22 = new MyGuiControlLabel();
                    label22.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
                    label22.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label22.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_UseTerminals);
                    MyGuiControlLabel label8 = label22;
                    color = null;
                    this.m_canUseTerminals = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    this.m_canUseTerminals.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.OnUseTerminalsChanged);
                    this.m_canUseTerminals.SetToolTip(MySpaceTexts.ScreenDebugAdminMenu_UseTerminalsToolTip);
                    this.m_canUseTerminals.IsChecked = MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.UseTerminals);
                    this.m_canUseTerminals.Enabled = MySession.Static.IsUserAdmin(Sync.MyId);
                    this.Controls.Add(this.m_canUseTerminals);
                    this.Controls.Add(label8);
                    float* singlePtr17 = (float*) ref base.m_currentPosition.Y;
                    singlePtr17[0] += 0.045f;
                    MyGuiControlLabel label23 = new MyGuiControlLabel();
                    label23.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
                    label23.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label23.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_KeepOriginalOwnershipOnPaste);
                    MyGuiControlLabel label9 = label23;
                    color = null;
                    this.m_keepOriginalOwnershipOnPasteCheckBox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    this.m_keepOriginalOwnershipOnPasteCheckBox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.OnKeepOwnershipChanged);
                    this.m_keepOriginalOwnershipOnPasteCheckBox.SetToolTip(MySpaceTexts.ScreenDebugAdminMenu_KeepOriginalOwnershipOnPasteTip);
                    this.m_keepOriginalOwnershipOnPasteCheckBox.IsChecked = MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.KeepOriginalOwnershipOnPaste);
                    this.m_keepOriginalOwnershipOnPasteCheckBox.Enabled = MySession.Static.IsUserSpaceMaster(Sync.MyId);
                    this.Controls.Add(this.m_keepOriginalOwnershipOnPasteCheckBox);
                    this.Controls.Add(label9);
                    if (MySession.Static.IsUserAdmin(Sync.MyId))
                    {
                        float* singlePtr18 = (float*) ref base.m_currentPosition.Y;
                        singlePtr18[0] += 0.045f;
                        MyGuiControlLabel label24 = new MyGuiControlLabel();
                        label24.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
                        label24.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                        label24.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_TimeOfDay);
                        MyGuiControlLabel label13 = label24;
                        this.Controls.Add(label13);
                        MyGuiControlLabel label25 = new MyGuiControlLabel();
                        label25.Position = base.m_currentPosition + new Vector2(0.285f, 0f);
                        label25.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
                        label25.Text = "0.00";
                        this.m_timeDeltaValue = label25;
                        this.Controls.Add(this.m_timeDeltaValue);
                        float* singlePtr19 = (float*) ref base.m_currentPosition.Y;
                        singlePtr19[0] += 0.035f;
                        float? defaultValue = null;
                        color = null;
                        this.m_timeDelta = new MyGuiControlSlider(new Vector2?(base.m_currentPosition + new Vector2(0.001f, 0f)), 0f, (MySession.Static == null) ? 1f : MySession.Static.Settings.SunRotationIntervalMinutes, 0.29f, defaultValue, color, null, 1, 0.8f, 0f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
                        this.m_timeDelta.Size = new Vector2(0.285f, 1f);
                        this.m_timeDelta.Value = MyTimeOfDayHelper.TimeOfDay;
                        this.m_timeDelta.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                        this.m_timeDelta.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_timeDelta.ValueChanged, new Action<MyGuiControlSlider>(this.TimeDeltaChanged));
                        this.m_timeDeltaValue.Text = $"{this.m_timeDelta.Value:0.00}";
                        this.Controls.Add(this.m_timeDelta);
                    }
                    return;
                }
                case MyPageEnum.TrashRemoval:
                {
                    float* singlePtr11 = (float*) ref base.m_currentPosition.Y;
                    singlePtr11[0] += 0.016f;
                    Vector2 vector2 = new Vector2(0.34f, -0.35f) + new Vector2(0f, 0f);
                    Vector2 vector3 = new Vector2(1f, 1f) + new Vector2(0f, 0f);
                    MyGuiDrawAlignEnum enum3 = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
                    MyGuiControlTabControl control1 = new MyGuiControlTabControl();
                    control1.Position = vector2;
                    control1.Size = vector3;
                    control1.Name = "TrashTabs";
                    control1.OriginAlign = enum3;
                    this.m_trashTabControls = control1;
                    this.m_trashTabControls.TabButtonScale = 0.8f;
                    Vector2 currentPosition = base.m_currentPosition;
                    this.m_trashTab_General = this.m_trashTabControls.GetTabSubControl(0);
                    this.m_trashTab_Voxel = this.m_trashTabControls.GetTabSubControl(1);
                    this.ConstructTrashTab_General(this.m_trashTab_General);
                    base.m_currentPosition = currentPosition;
                    this.ConstructTrashTab_Voxel(this.m_trashTab_Voxel);
                    base.m_currentPosition = currentPosition;
                    TrashTab trashTabSelected = this.m_trashTabSelected;
                    if (trashTabSelected == TrashTab.General)
                    {
                        this.m_trashTabControls.SetPage(0);
                    }
                    else if (trashTabSelected == TrashTab.Voxels)
                    {
                        this.m_trashTabControls.SetPage(1);
                    }
                    this.m_trashTabControls.OnPageChanged += new Action(this.m_trashTabControls_OnPageChanged);
                    Vector2 vector5 = new Vector2(0f, 0.48f);
                    this.m_trashTab_General.Position = (this.m_trashTab_General.Position - vector2) - vector5;
                    this.m_trashTab_Voxel.Position = (this.m_trashTab_Voxel.Position - vector2) - vector5;
                    this.m_trashTab_General.TextEnum = MyCommonTexts.ScreenDebugAdminMenu_GeneralTabButton;
                    this.m_trashTab_Voxel.TextEnum = MyCommonTexts.ScreenDebugAdminMenu_VoxelTabButton;
                    this.m_trashTab_General.TextScale = 0.75f;
                    this.m_trashTab_Voxel.TextScale = 0.75f;
                    trashTabSelected = this.m_trashTabSelected;
                    if (trashTabSelected != TrashTab.General)
                    {
                        TrashTab tab1 = trashTabSelected;
                    }
                    this.Controls.Add(this.m_trashTabControls);
                    return;
                }
                case MyPageEnum.CycleObjects:
                {
                    int num6;
                    float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
                    singlePtr4[0] += 0.03f;
                    MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
                    color = null;
                    list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.19f), base.m_size.Value.X * 0.73f, 0f, color);
                    color = null;
                    list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.138f), base.m_size.Value.X * 0.73f, 0f, color);
                    color = null;
                    list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.305f), base.m_size.Value.X * 0.73f, 0f, color);
                    this.Controls.Add(list2);
                    MyGuiControlLabel label1 = new MyGuiControlLabel();
                    label1.Position = new Vector2(-0.16f, -0.335f);
                    label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label1.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SortBy) + ":";
                    MyGuiControlLabel label = label1;
                    this.Controls.Add(label);
                    MyGuiControlCombobox combobox1 = base.AddCombo<MyEntityCyclingOrder>(m_order, new Action<MyEntityCyclingOrder>(this.OnOrderChanged), true, 10, null, new VRageMath.Vector4?(this.m_labelColor));
                    combobox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
                    combobox1.PositionX = 0.122f;
                    combobox1.Size = new Vector2(0.22f, 1f);
                    float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
                    singlePtr5[0] += 0.005f;
                    MyGuiControlLabel label14 = new MyGuiControlLabel();
                    label14.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
                    label14.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label14.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_SmallGrids);
                    MyGuiControlLabel label2 = label14;
                    color = null;
                    this.m_onlySmallGridsCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    this.m_onlySmallGridsCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.OnSmallGridChanged);
                    this.m_onlySmallGridsCheckbox.IsChecked = m_cyclingOptions.OnlySmallGrids;
                    this.Controls.Add(this.m_onlySmallGridsCheckbox);
                    this.Controls.Add(label2);
                    float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
                    singlePtr6[0] += 0.045f;
                    MyGuiControlLabel label15 = new MyGuiControlLabel();
                    label15.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
                    label15.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label15.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_LargeGrids);
                    MyGuiControlLabel label3 = label15;
                    color = null;
                    this.m_onlyLargeGridsCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    this.m_onlyLargeGridsCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.OnLargeGridChanged);
                    this.m_onlyLargeGridsCheckbox.IsChecked = m_cyclingOptions.OnlyLargeGrids;
                    this.Controls.Add(this.m_onlyLargeGridsCheckbox);
                    this.Controls.Add(label3);
                    float* singlePtr7 = (float*) ref base.m_currentPosition.Y;
                    singlePtr7[0] += 0.12f;
                    y = base.m_currentPosition.Y;
                    nullable4 = null;
                    MyGuiControlButton button1 = this.CreateDebugButton(0.284f, MyCommonTexts.ScreenDebugAdminMenu_First, c => this.OnCycleClicked(true, true), true, nullable4, true, true);
                    button1.PositionX += 0.003f;
                    button1.PositionY -= 0.0435f;
                    base.m_currentPosition.Y = y;
                    nullable4 = null;
                    this.CreateDebugButton(0.14f, MyCommonTexts.ScreenDebugAdminMenu_Next, c => this.OnCycleClicked(false, false), true, nullable4, true, true).PositionX = -0.088f;
                    base.m_currentPosition.Y = y;
                    nullable4 = null;
                    this.CreateDebugButton(0.14f, MyCommonTexts.ScreenDebugAdminMenu_Previous, c => this.OnCycleClicked(false, true), true, nullable4, true, true).PositionX = 0.055f;
                    MyGuiControlLabel label16 = new MyGuiControlLabel();
                    label16.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
                    label16.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label16.Text = MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_EntityName) + " -";
                    this.m_labelEntityName = label16;
                    this.Controls.Add(this.m_labelEntityName);
                    float* singlePtr8 = (float*) ref base.m_currentPosition.Y;
                    singlePtr8[0] += 0.035f;
                    MyGuiControlLabel label17 = new MyGuiControlLabel();
                    label17.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
                    label17.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label17.Text = new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_CurrentValue), (m_entityId == 0) ? "-" : m_metricValue.ToString()).ToString();
                    this.m_labelCurrentIndex = label17;
                    this.Controls.Add(this.m_labelCurrentIndex);
                    float* singlePtr9 = (float*) ref base.m_currentPosition.Y;
                    singlePtr9[0] += 0.208f;
                    y = base.m_currentPosition.Y;
                    nullable4 = null;
                    this.m_removeItemButton = this.CreateDebugButton(0.284f, MyCommonTexts.ScreenDebugAdminMenu_Remove, c => this.OnEntityOperationClicked(MyEntityList.EntityListAction.Remove), true, nullable4, true, true);
                    this.m_removeItemButton.PositionX += 0.003f;
                    base.m_currentPosition.Y = y;
                    nullable4 = null;
                    this.m_stopItemButton = this.CreateDebugButton(0.284f, MyCommonTexts.ScreenDebugAdminMenu_Stop, c => this.OnEntityOperationClicked(MyEntityList.EntityListAction.Stop), true, nullable4, true, true);
                    this.m_stopItemButton.PositionX += 0.003f;
                    this.m_stopItemButton.PositionY += 0.0435f;
                    base.m_currentPosition.Y = y;
                    nullable4 = null;
                    this.m_depowerItemButton = this.CreateDebugButton(0.284f, MySpaceTexts.ScreenDebugAdminMenu_Depower, c => this.OnEntityOperationClicked(MyEntityList.EntityListAction.Depower), true, nullable4, true, true);
                    this.m_depowerItemButton.PositionX += 0.003f;
                    this.m_depowerItemButton.PositionY += 0.087f;
                    float* singlePtr10 = (float*) ref base.m_currentPosition.Y;
                    singlePtr10[0] += 0.125f;
                    y = base.m_currentPosition.Y;
                    base.m_currentPosition.Y = y;
                    MyGuiControlButton button2 = this.CreateDebugButton(0.284f, MyCommonTexts.SpectatorControls_None, new Action<MyGuiControlButton>(this.OnPlayerControl), true, new MyStringId?(MySpaceTexts.SpectatorControls_None_Desc), true, true);
                    button2.PositionX += 0.003f;
                    base.m_currentPosition.Y = y;
                    if ((MySession.Static.LocalCharacter == null) || (MySession.Static.LocalCharacter.Parent != null))
                    {
                        num6 = 0;
                    }
                    else
                    {
                        num6 = (int) MySession.Static.IsUserSpaceMaster(Sync.MyId);
                    }
                    MyGuiControlButton button3 = new Action<MyGuiControlButton>(this.OnTeleportButton).CreateDebugButton((float) MySpaceTexts.ScreenDebugAdminMenu_TeleportHere, 0.284f, (Action<MyGuiControlButton>) this, (bool) num6, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_TeleportHereToolTip), true, true);
                    button3.PositionX += 0.003f;
                    button3.PositionY += 0.0435f;
                    bool enabled = !Sync.IsServer;
                    MyGuiControlButton button4 = this.CreateDebugButton(0.284f, MyCommonTexts.ScreenDebugAdminMenu_ReplicateEverything, new Action<MyGuiControlButton>(this.OnReplicateEverything), enabled, new MyStringId?(enabled ? MyCommonTexts.ScreenDebugAdminMenu_ReplicateEverything_Tooltip : MySpaceTexts.ScreenDebugAdminMenu_ReplicateEverythingServer_Tooltip), true, true);
                    button4.PositionX += 0.003f;
                    MyGuiControlButton local1 = button3;
                    local1.PositionY += 0.0435f;
                    this.OnOrderChanged(m_order);
                    return;
                }
                case MyPageEnum.EntityList:
                {
                    float* singlePtr20 = (float*) ref base.m_currentPosition.Y;
                    singlePtr20[0] += 0.095f;
                    MyGuiControlLabel label26 = new MyGuiControlLabel();
                    label26.Position = new Vector2(-0.16f, -0.334f);
                    label26.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label26.Text = MyTexts.GetString(MyCommonTexts.Select) + ":";
                    MyGuiControlLabel label10 = label26;
                    this.Controls.Add(label10);
                    float* singlePtr21 = (float*) ref base.m_currentPosition.Y;
                    singlePtr21[0] -= 0.065f;
                    this.m_entityTypeCombo = base.AddCombo<MyEntityList.MyEntityTypeEnum>(this.m_selectedType, new Action<MyEntityList.MyEntityTypeEnum>(this.ValueChanged), true, 10, null, new VRageMath.Vector4?(this.m_labelColor));
                    this.m_entityTypeCombo.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
                    this.m_entityTypeCombo.PositionX = 0.122f;
                    this.m_entityTypeCombo.Size = new Vector2(0.22f, 1f);
                    MyGuiControlLabel label27 = new MyGuiControlLabel();
                    label27.Position = new Vector2(-0.16f, -0.284f);
                    label27.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label27.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SortBy) + ":";
                    MyGuiControlLabel label11 = label27;
                    this.Controls.Add(label11);
                    this.m_entitySortCombo = base.AddCombo<MyEntityList.MyEntitySortOrder>(this.m_selectedSort, new Action<MyEntityList.MyEntitySortOrder>(this.ValueChanged), true, 10, null, new VRageMath.Vector4?(this.m_labelColor));
                    this.m_entitySortCombo.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
                    this.m_entitySortCombo.PositionX = 0.122f;
                    this.m_entitySortCombo.Size = new Vector2(0.22f, 1f);
                    MyGuiControlSeparatorList list3 = new MyGuiControlSeparatorList();
                    color = null;
                    list3.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.231f), base.m_size.Value.X * 0.73f, 0f, color);
                    this.Controls.Add(list3);
                    MyGuiControlLabel label28 = new MyGuiControlLabel();
                    label28.Position = new Vector2(-0.153f, -0.205f);
                    label28.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    label28.Text = MyTexts.GetString(MySpaceTexts.SafeZone_ListOfEntities);
                    MyGuiControlLabel label12 = label28;
                    color = null;
                    MyGuiControlPanel panel1 = new MyGuiControlPanel(new Vector2(label12.PositionX - 0.0085f, label12.Position.Y - 0.005f), new Vector2(0.2865f, 0.035f), color, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
                    panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
                    MyGuiControlPanel panel = panel1;
                    this.Controls.Add(panel);
                    this.Controls.Add(label12);
                    float* singlePtr22 = (float*) ref base.m_currentPosition.Y;
                    singlePtr22[0] += 0.065f;
                    this.m_entityListbox = new MyGuiControlListbox(new Vector2?(Vector2.Zero), MyGuiControlListboxStyleEnum.Blueprints);
                    this.m_entityListbox.Size = new Vector2(x, 0f);
                    this.m_entityListbox.Enabled = true;
                    this.m_entityListbox.VisibleRowsCount = 12;
                    this.m_entityListbox.Position = (this.m_entityListbox.Size / 2f) + base.m_currentPosition;
                    this.m_entityListbox.ItemClicked += new Action<MyGuiControlListbox>(this.EntityListItemClicked);
                    this.m_entityListbox.MultiSelect = true;
                    MyGuiControlSeparatorList list4 = new MyGuiControlSeparatorList();
                    color = null;
                    list4.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.271f), base.m_size.Value.X * 0.73f, 0f, color);
                    this.Controls.Add(list4);
                    base.m_currentPosition = this.m_entityListbox.GetPositionAbsoluteBottomLeft();
                    float* singlePtr23 = (float*) ref base.m_currentPosition.Y;
                    singlePtr23[0] += 0.045f;
                    MyGuiControlButton button = this.CreateDebugButton(0.14f, MyCommonTexts.SpectatorControls_None, new Action<MyGuiControlButton>(this.OnPlayerControl), true, new MyStringId?(MySpaceTexts.SpectatorControls_None_Desc), true, true);
                    button.PositionX = -0.088f;
                    MyGuiControlButton button5 = this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_TeleportHere, new Action<MyGuiControlButton>(this.OnTeleportButton), (MySession.Static.LocalCharacter != null) && ReferenceEquals(MySession.Static.LocalCharacter.Parent, null), new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_TeleportHereToolTip), true, true);
                    button5.PositionX = 0.055f;
                    button5.PositionY = button.PositionY;
                    y = base.m_currentPosition.Y;
                    base.m_currentPosition.Y = y;
                    nullable4 = null;
                    this.m_stopItemButton = this.CreateDebugButton(0.14f, MyCommonTexts.ScreenDebugAdminMenu_Stop, c => this.OnEntityListActionClicked(MyEntityList.EntityListAction.Stop), true, nullable4, true, true);
                    this.m_stopItemButton.PositionX = -0.088f;
                    this.m_stopItemButton.PositionY -= 0.0435f;
                    base.m_currentPosition.Y = y;
                    nullable4 = null;
                    this.m_depowerItemButton = this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_Depower, c => this.OnEntityListActionClicked(MyEntityList.EntityListAction.Depower), true, nullable4, true, true);
                    this.m_depowerItemButton.PositionX = 0.055f;
                    this.m_depowerItemButton.PositionY = this.m_stopItemButton.PositionY;
                    nullable4 = null;
                    this.m_removeItemButton = this.CreateDebugButton(0.14f, MyCommonTexts.ScreenDebugAdminMenu_Remove, c => this.OnEntityListActionClicked(MyEntityList.EntityListAction.Remove), true, nullable4, true, true);
                    this.m_removeItemButton.PositionX -= 0.068f;
                    this.m_removeItemButton.PositionY -= 0.0435f;
                    MyGuiControlButton button6 = this.CreateDebugButton(0.14f, MySpaceTexts.buttonRefresh, new Action<MyGuiControlButton>(this.OnRefreshButton), true, new MyStringId?(MySpaceTexts.ProgrammableBlock_ButtonRefreshScripts), true, true);
                    button6.PositionX += 0.075f;
                    button6.PositionY = this.m_removeItemButton.PositionY;
                    MyGuiControlButton button7 = this.CreateDebugButton(0.284f, MySpaceTexts.ScreenDebugAdminMenu_RemoveOwner, new Action<MyGuiControlButton>(this.OnRemoveOwnerButton), true, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_RemoveOwnerToolTip), true, true);
                    button7.PositionX += 0.003f;
                    button7.PositionY -= 0.087f;
                    this.Controls.Add(this.m_entityListbox);
                    this.ValueChanged((MyEntityList.MyEntityTypeEnum) ((int) this.m_entityTypeCombo.GetSelectedKey()));
                    return;
                }
                case MyPageEnum.SafeZones:
                    this.RecreateSafeZonesControls(ref controlPadding, separatorSize, x);
                    return;

                case MyPageEnum.GlobalSafeZone:
                    this.RecreateGlobalSafeZoneControls(ref controlPadding, separatorSize, x);
                    return;

                case MyPageEnum.ReplayTool:
                    this.RecreateReplayToolControls(ref controlPadding, separatorSize, x);
                    return;
            }
            throw new ArgumentOutOfRangeException();
        }

        private unsafe void RecreateGlobalSafeZoneControls(ref Vector2 controlPadding, float separatorSize, float usableWidth)
        {
            this.m_recreateInProgress = true;
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.03f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowDamage);
            this.m_damageCheckboxGlobalLabel = label1;
            VRageMath.Vector4? color = null;
            this.m_damageGlobalCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.Controls.Add(this.m_damageCheckboxGlobalLabel);
            this.Controls.Add(this.m_damageGlobalCheckbox);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.045f;
            MyGuiControlLabel label2 = new MyGuiControlLabel();
            label2.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label2.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowShooting);
            this.m_shootingCheckboxGlobalLabel = label2;
            color = null;
            this.m_shootingGlobalCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.Controls.Add(this.m_shootingCheckboxGlobalLabel);
            this.Controls.Add(this.m_shootingGlobalCheckbox);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.045f;
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label3.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowDrilling);
            this.m_drillingCheckboxGlobalLabel = label3;
            color = null;
            this.m_drillingGlobalCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.Controls.Add(this.m_drillingCheckboxGlobalLabel);
            this.Controls.Add(this.m_drillingGlobalCheckbox);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.045f;
            MyGuiControlLabel label4 = new MyGuiControlLabel();
            label4.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label4.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowWelding);
            this.m_weldingCheckboxGlobalLabel = label4;
            color = null;
            this.m_weldingGlobalCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.Controls.Add(this.m_weldingCheckboxGlobalLabel);
            this.Controls.Add(this.m_weldingGlobalCheckbox);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += 0.045f;
            MyGuiControlLabel label5 = new MyGuiControlLabel();
            label5.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label5.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowGrinding);
            this.m_grindingCheckboxGlobalLabel = label5;
            color = null;
            this.m_grindingGlobalCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.Controls.Add(this.m_grindingCheckboxGlobalLabel);
            this.Controls.Add(this.m_grindingGlobalCheckbox);
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += 0.045f;
            MyGuiControlLabel label6 = new MyGuiControlLabel();
            label6.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label6.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label6.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowBuilding);
            this.m_buildingCheckboxGlobalLabel = label6;
            color = null;
            this.m_buildingGlobalCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.Controls.Add(this.m_buildingCheckboxGlobalLabel);
            this.Controls.Add(this.m_buildingGlobalCheckbox);
            float* singlePtr7 = (float*) ref base.m_currentPosition.Y;
            singlePtr7[0] += 0.045f;
            MyGuiControlLabel label7 = new MyGuiControlLabel();
            label7.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label7.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label7.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowVoxelHands);
            this.m_voxelHandCheckboxGlobalLabel = label7;
            color = null;
            this.m_voxelHandGlobalCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.Controls.Add(this.m_voxelHandCheckboxGlobalLabel);
            this.Controls.Add(this.m_voxelHandGlobalCheckbox);
            this.UpdateSelectedGlobalData();
            this.m_voxelHandGlobalCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.VoxelHandCheckGlobalChanged);
            this.m_buildingGlobalCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.BuildingCheckGlobalChanged);
            this.m_grindingGlobalCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.GrindingCheckGlobalChanged);
            this.m_weldingGlobalCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.WeldingCheckGlobalChanged);
            this.m_drillingGlobalCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.DrillingCheckGlobalChanged);
            this.m_shootingGlobalCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.ShootingCheckGlobalChanged);
            this.m_damageGlobalCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.DamageCheckGlobalChanged);
        }

        private unsafe void RecreateReplayToolControls(ref Vector2 controlPadding, float separatorSize, float usableWidth)
        {
            this.m_recreateInProgress = true;
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.03f;
            if (MySession.Static.IsServer)
            {
                this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_ReloadWorld, new Action<MyGuiControlButton>(this.ReloadWorld), true, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_ReloadWorld_Tooltip), true, true);
            }
            else
            {
                MyGuiControlButton button1 = this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_ReloadWorld, new Action<MyGuiControlButton>(this.ReloadWorld), true, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_ReloadWorldClient_Tooltip), true, true);
                button1.Enabled = false;
                button1.ShowTooltipWhenDisabled = true;
            }
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_ManageCharacters);
            MyGuiControlLabel control = label1;
            this.Controls.Add(control);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.03f;
            Vector2 currentPosition = base.m_currentPosition;
            base.m_buttonXOffset -= 0.075f;
            this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_AddCharacter, new Action<MyGuiControlButton>(this.AddCharacter), true, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_AddCharacter_Tooltip), true, true);
            base.m_currentPosition.Y = currentPosition.Y;
            base.m_buttonXOffset += 0.15f;
            this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_RemoveCharacter, new Action<MyGuiControlButton>(this.TryRemoveCurrentCharacter), true, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_RemoveCharacter_Tooltip), true, true);
            base.m_buttonXOffset = 0f;
            this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_ChangeAsset, new Action<MyGuiControlButton>(this.ChangeSkin), true, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_ChangeAsset_Tooltip), true, true);
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label3.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_ManageRecordings);
            MyGuiControlLabel label2 = label3;
            this.Controls.Add(label2);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.03f;
            if (MySessionComponentReplay.Static.IsReplaying)
            {
                this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_StopReplay, new Action<MyGuiControlButton>(this.OnReplayButtonPressed), true, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_StopReplay_Tooltip), true, true);
            }
            else
            {
                this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_Replay, new Action<MyGuiControlButton>(this.OnReplayButtonPressed), true, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_Replay_Tooltip), true, true);
            }
            if (MySessionComponentReplay.Static.IsRecording)
            {
                this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_StopRecording, new Action<MyGuiControlButton>(this.OnRecordButtonPressed), true, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_StopRecording_Tooltip), true, true);
            }
            else
            {
                this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_RecordAndReplay, new Action<MyGuiControlButton>(this.OnRecordButtonPressed), true, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_RecordAndReplay_Tooltip), true, true);
            }
            this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_DeleteRecordings, new Action<MyGuiControlButton>(this.DeleteRecordings), true, new MyStringId?(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_DeleteRecordings_Tooltip), true, true);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.02f;
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText();
            text1.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            text1.Size = new Vector2(0.7f, 0.6f);
            text1.Font = "Blue";
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiControlMultilineText text = text1;
            text.Text = MyTexts.Get(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_Tutorial);
            this.Controls.Add(text);
        }

        private unsafe void RecreateSafeZonesControls(ref Vector2 controlPadding, float separatorSize, float usableWidth)
        {
            this.m_recreateInProgress = true;
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.025f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_SelectSafeZone);
            this.m_selectSafeZoneLabel = label1;
            this.Controls.Add(this.m_selectSafeZoneLabel);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.03f;
            VRageMath.Vector4? textColor = null;
            Vector2? size = null;
            this.m_safeZonesCombo = base.AddCombo(null, textColor, size, 10);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.005f;
            MyGuiControlLabel label2 = new MyGuiControlLabel();
            label2.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label2.Text = MyTexts.GetString(MySpaceTexts.SafeZone_SelectZoneShape);
            this.m_selectZoneShapeLabel = label2;
            this.Controls.Add(this.m_selectZoneShapeLabel);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.03f;
            textColor = null;
            size = null;
            this.m_safeZonesTypeCombo = base.AddCombo(null, textColor, size, 10);
            int? sortOrder = null;
            this.m_safeZonesTypeCombo.AddItem(0L, MyTexts.GetString(MySpaceTexts.SafeZone_Spherical), sortOrder, null);
            sortOrder = null;
            this.m_safeZonesTypeCombo.AddItem(1L, MyTexts.GetString(MySpaceTexts.SafeZone_Cubical), sortOrder, null);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += 0.005f;
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label3.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_ZoneRadius);
            this.m_zoneRadiusLabel = label3;
            this.Controls.Add(this.m_zoneRadiusLabel);
            this.m_zoneRadiusLabel.Visible = false;
            MyGuiControlLabel label4 = new MyGuiControlLabel();
            label4.Position = new Vector2(base.m_currentPosition.X + 0.285f, base.m_currentPosition.Y);
            label4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            label4.Text = "1";
            this.m_zoneRadiusValueLabel = label4;
            this.Controls.Add(this.m_zoneRadiusValueLabel);
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += 0.03f;
            textColor = null;
            this.m_radiusSlider = new MyGuiControlSlider(new Vector2?(base.m_currentPosition), 1f, 500f, 0.285f, 1f, textColor, null, 1, 0.8f, 0f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
            this.m_radiusSlider.Visible = false;
            this.m_radiusSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_radiusSlider.ValueChanged, new Action<MyGuiControlSlider>(this.OnRadiusChange));
            this.Controls.Add(this.m_radiusSlider);
            float* singlePtr7 = (float*) ref base.m_currentPosition.Y;
            singlePtr7[0] -= 0.03f;
            MyGuiControlLabel label5 = new MyGuiControlLabel();
            label5.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label5.Text = MyTexts.GetString(MySpaceTexts.SafeZone_CubeAxis);
            this.m_selectAxisLabel = label5;
            this.Controls.Add(this.m_selectAxisLabel);
            MyGuiControlLabel label6 = new MyGuiControlLabel();
            label6.Position = new Vector2(base.m_currentPosition.X + 0.09f, base.m_currentPosition.Y);
            label6.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label6.Text = MyTexts.GetString(MyCommonTexts.Size);
            this.m_zoneSizeLabel = label6;
            this.Controls.Add(this.m_zoneSizeLabel);
            float* singlePtr8 = (float*) ref base.m_currentPosition.Y;
            singlePtr8[0] += 0.03f;
            textColor = null;
            size = null;
            this.m_safeZonesAxisCombo = base.AddCombo(null, textColor, size, 10);
            this.m_safeZonesAxisCombo.Size = new Vector2(0.08f, 1f);
            this.m_safeZonesAxisCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_safeZonesAxisCombo_ItemSelected);
            sortOrder = null;
            this.m_safeZonesAxisCombo.AddItem(0L, 0.ToString(), sortOrder, null);
            sortOrder = null;
            this.m_safeZonesAxisCombo.AddItem(1L, 1.ToString(), sortOrder, null);
            sortOrder = null;
            this.m_safeZonesAxisCombo.AddItem(2L, 2.ToString(), sortOrder, null);
            this.m_safeZonesAxisCombo.SelectItemByIndex(0);
            textColor = null;
            this.m_sizeSlider = new MyGuiControlSlider(new Vector2?(base.m_currentPosition + new Vector2(0.09f, -0.05f)), 1f, 500f, 0.195f, 1f, textColor, null, 1, 0.8f, 0f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
            this.m_sizeSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sizeSlider.ValueChanged, new Action<MyGuiControlSlider>(this.OnSizeChange));
            this.Controls.Add(this.m_sizeSlider);
            float* singlePtr9 = (float*) ref base.m_currentPosition.Y;
            singlePtr9[0] += 0.018f;
            MyGuiControlLabel label7 = new MyGuiControlLabel();
            label7.Position = base.m_currentPosition + new Vector2(0.001f, 0f);
            label7.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label7.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_ZoneEnabled);
            this.m_enabledCheckboxLabel = label7;
            textColor = null;
            this.m_enabledCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), textColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.m_enabledCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.EnabledCheckedChanged);
            this.Controls.Add(this.m_enabledCheckboxLabel);
            this.Controls.Add(this.m_enabledCheckbox);
            float* singlePtr10 = (float*) ref base.m_currentPosition.Y;
            singlePtr10[0] += 0.045f;
            MyGuiControlLabel label8 = new MyGuiControlLabel();
            label8.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label8.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label8.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowDamage);
            this.m_damageCheckboxLabel = label8;
            textColor = null;
            this.m_damageCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), textColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.m_damageCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.DamageCheckChanged);
            this.Controls.Add(this.m_damageCheckboxLabel);
            this.Controls.Add(this.m_damageCheckbox);
            float* singlePtr11 = (float*) ref base.m_currentPosition.Y;
            singlePtr11[0] += 0.045f;
            MyGuiControlLabel label9 = new MyGuiControlLabel();
            label9.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label9.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label9.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowShooting);
            this.m_shootingCheckboxLabel = label9;
            textColor = null;
            this.m_shootingCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), textColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.m_shootingCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.ShootingCheckChanged);
            this.Controls.Add(this.m_shootingCheckboxLabel);
            this.Controls.Add(this.m_shootingCheckbox);
            float* singlePtr12 = (float*) ref base.m_currentPosition.Y;
            singlePtr12[0] += 0.045f;
            MyGuiControlLabel label10 = new MyGuiControlLabel();
            label10.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label10.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label10.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowDrilling);
            this.m_drillingCheckboxLabel = label10;
            textColor = null;
            this.m_drillingCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), textColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.m_drillingCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.DrillingCheckChanged);
            this.Controls.Add(this.m_drillingCheckboxLabel);
            this.Controls.Add(this.m_drillingCheckbox);
            float* singlePtr13 = (float*) ref base.m_currentPosition.Y;
            singlePtr13[0] += 0.045f;
            MyGuiControlLabel label11 = new MyGuiControlLabel();
            label11.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label11.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label11.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowWelding);
            this.m_weldingCheckboxLabel = label11;
            textColor = null;
            this.m_weldingCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), textColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.m_weldingCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.WeldingCheckChanged);
            this.Controls.Add(this.m_weldingCheckboxLabel);
            this.Controls.Add(this.m_weldingCheckbox);
            float* singlePtr14 = (float*) ref base.m_currentPosition.Y;
            singlePtr14[0] += 0.045f;
            MyGuiControlLabel label12 = new MyGuiControlLabel();
            label12.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label12.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label12.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowGrinding);
            this.m_grindingCheckboxLabel = label12;
            textColor = null;
            this.m_grindingCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), textColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.m_grindingCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.GrindingCheckChanged);
            this.Controls.Add(this.m_grindingCheckboxLabel);
            this.Controls.Add(this.m_grindingCheckbox);
            float* singlePtr15 = (float*) ref base.m_currentPosition.Y;
            singlePtr15[0] += 0.045f;
            MyGuiControlLabel label13 = new MyGuiControlLabel();
            label13.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label13.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label13.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowBuilding);
            this.m_buildingCheckboxLabel = label13;
            textColor = null;
            this.m_buildingCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), textColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.m_buildingCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.BuildingCheckChanged);
            this.Controls.Add(this.m_buildingCheckboxLabel);
            this.Controls.Add(this.m_buildingCheckbox);
            float* singlePtr16 = (float*) ref base.m_currentPosition.Y;
            singlePtr16[0] += 0.045f;
            MyGuiControlLabel label14 = new MyGuiControlLabel();
            label14.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label14.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label14.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_AllowVoxelHands);
            this.m_voxelHandCheckboxLabel = label14;
            textColor = null;
            this.m_voxelHandCheckbox = new MyGuiControlCheckbox(new Vector2(base.m_currentPosition.X + 0.293f, base.m_currentPosition.Y - 0.01f), textColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            this.m_voxelHandCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.VoxelHandCheckChanged);
            this.Controls.Add(this.m_voxelHandCheckboxLabel);
            this.Controls.Add(this.m_voxelHandCheckbox);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            textColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, -0.304f), base.m_size.Value.X * 0.73f, 0f, textColor);
            this.Controls.Add(control);
            float* singlePtr17 = (float*) ref base.m_currentPosition.Y;
            singlePtr17[0] += 0.097f;
            float y = base.m_currentPosition.Y;
            MyStringId? tooltip = null;
            this.m_addSafeZoneButton = this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_SafeZones_NewSafeZone, c => this.OnAddSafeZone(), true, tooltip, true, true);
            this.m_addSafeZoneButton.PositionX = -0.088f;
            base.m_currentPosition.Y = y;
            tooltip = null;
            this.m_moveToSafeZoneButton = this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_SafeZones_MoveToSafeZone, c => this.OnMoveToSafeZone(), true, tooltip, true, true);
            this.m_moveToSafeZoneButton.PositionX = 0.055f;
            y = base.m_currentPosition.Y;
            tooltip = null;
            this.m_repositionSafeZoneButton = this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_SafeZones_ChangePosition, c => this.OnRepositionSafeZone(), true, tooltip, true, true);
            this.m_repositionSafeZoneButton.PositionX = -0.088f;
            base.m_currentPosition.Y = y;
            tooltip = null;
            this.m_configureFilterButton = this.CreateDebugButton(0.14f, MySpaceTexts.ScreenDebugAdminMenu_SafeZones_ConfigureFilter, c => this.OnConfigureFilter(), true, tooltip, true, true);
            this.m_configureFilterButton.PositionX = 0.055f;
            y = base.m_currentPosition.Y;
            tooltip = null;
            this.m_removeSafeZoneButton = this.CreateDebugButton(0.14f, MyCommonTexts.ScreenDebugAdminMenu_Remove, c => this.OnRemoveSafeZone(), true, tooltip, true, true);
            this.m_removeSafeZoneButton.PositionX = -0.088f;
            base.m_currentPosition.Y = y;
            tooltip = null;
            this.m_renameSafeZoneButton = this.CreateDebugButton(0.14f, MySpaceTexts.DetailScreen_Button_Rename, c => this.OnRenameSafeZone(), true, tooltip, true, true);
            this.m_renameSafeZoneButton.PositionX = 0.055f;
            this.RefreshSafeZones();
            this.UpdateZoneType();
            this.UpdateSelectedData();
            this.m_safeZonesCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_safeZonesCombo_ItemSelected);
            this.m_safeZonesTypeCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_safeZonesTypeCombo_ItemSelected);
            this.m_recreateInProgress = false;
        }

        private void RefreshSafeZones()
        {
            this.m_safeZonesCombo.ClearItems();
            foreach (MySafeZone zone in MySessionComponentSafeZones.SafeZones)
            {
                this.m_safeZonesCombo.AddItem(zone.EntityId, (zone.DisplayName != null) ? zone.DisplayName : zone.ToString(), 1, null);
            }
            if (this.m_selectedSafeZone == null)
            {
                this.m_selectedSafeZone = (this.m_safeZonesCombo.GetItemsCount() > 0) ? ((MySafeZone) MyEntities.GetEntityById(this.m_safeZonesCombo.GetItemByIndex(this.m_safeZonesCombo.GetItemsCount() - 1).Key, false)) : null;
            }
            if (this.m_selectedSafeZone != null)
            {
                this.m_safeZonesCombo.SelectItemByKey(this.m_selectedSafeZone.EntityId, true);
            }
        }

        public override bool RegisterClicks() => 
            true;

        private void ReloadWorld(MyGuiControlButton obj)
        {
            MyGuiScreenGamePlay.Static.ShowLoadMessageBox(MySession.Static.CurrentPath);
        }

        [Event(null, 0x6fd), Reliable, Server]
        private static void RemoveFloating_Implementation()
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                foreach (MyEntity entity in MyEntities.GetEntities())
                {
                    if ((entity is MyFloatingObject) || (entity is MyInventoryBagEntity))
                    {
                        entity.Close();
                    }
                }
            }
        }

        [Event(null, 0x72b), Server, Reliable]
        private static void RemoveOwner_Implementation(List<long> owners, List<long> entityIds)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                using (List<long>.Enumerator enumerator = entityIds.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyEntity entity;
                        if (!MyEntities.TryGetEntityById(enumerator.Current, out entity, false))
                        {
                            continue;
                        }
                        MyEntityList.ProceedEntityAction(entity, MyEntityList.EntityListAction.Remove);
                    }
                }
                foreach (long num in owners)
                {
                    MyIdentity identity = MySession.Static.Players.TryGetIdentity(num);
                    if (identity.Character != null)
                    {
                        identity.Character.Close();
                    }
                    using (HashSet<long>.Enumerator enumerator2 = identity.SavedCharacters.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            MyCharacter character;
                            if (!MyEntities.TryGetEntityById<MyCharacter>(enumerator2.Current, out character, true))
                            {
                                continue;
                            }
                            if (!character.Closed || character.MarkedForClose)
                            {
                                character.Close();
                            }
                        }
                    }
                    if ((identity != null) && (identity.BlockLimits.BlocksBuilt == 0))
                    {
                        MyPlayer.PlayerId playerId = new MyPlayer.PlayerId();
                        MySession.Static.Players.RemoveIdentity(num, playerId);
                    }
                }
            }
        }

        [Event(null, 0x78f), Reliable, Server]
        private static void ReplicateEverything_Implementation()
        {
            if (!MyEventContext.Current.IsLocallyInvoked)
            {
                if (!MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
                {
                    (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                }
                else
                {
                    ((MyReplicationServer) MyMultiplayer.Static.ReplicationLayer).ForceEverything(new Endpoint(MyEventContext.Current.Sender, 0));
                }
            }
        }

        [Event(null, 0x45f), Reliable, Server]
        private static void RequestSettingFromServer_Implementation()
        {
            AdminSettings settings = new AdminSettings {
                flags = MySession.Static.Settings.TrashFlags,
                enable = MySession.Static.Settings.TrashRemovalEnabled,
                blockCount = MySession.Static.Settings.BlockCountThreshold,
                playerDistance = MySession.Static.Settings.PlayerDistanceThreshold,
                gridCount = MySession.Static.Settings.OptimalGridCount,
                playerInactivity = MySession.Static.Settings.PlayerInactivityThreshold,
                characterRemovalThreshold = MySession.Static.Settings.PlayerCharacterRemovalThreshold,
                AdminSettingsFlags = MySession.Static.RemoteAdminSettings.GetValueOrDefault<ulong, AdminSettingsEnum>(MyEventContext.Current.Sender.Value, AdminSettingsEnum.None),
                voxelDistanceFromPlayer = MySession.Static.Settings.VoxelPlayerDistanceThreshold,
                voxelDistanceFromGrid = MySession.Static.Settings.VoxelGridDistanceThreshold,
                voxelAge = MySession.Static.Settings.VoxelAgeThreshold,
                voxelEnable = MySession.Static.Settings.VoxelTrashRemovalEnabled
            };
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<AdminSettings>(x => new Action<AdminSettings>(MyGuiScreenAdminMenu.DownloadSettingFromServer), settings, MyEventContext.Current.Sender, position);
        }

        private void RequestUpdateSafeZone()
        {
            if (this.m_selectedSafeZone != null)
            {
                MySessionComponentSafeZones.RequestUpdateSafeZone((MyObjectBuilder_SafeZone) this.m_selectedSafeZone.GetObjectBuilder(false));
            }
        }

        private void SelectNextCharacter()
        {
            MyCameraControllerEnum cameraControllerEnum = MySession.Static.GetCameraControllerEnum();
            if ((cameraControllerEnum == MyCameraControllerEnum.Entity) || (cameraControllerEnum == MyCameraControllerEnum.ThirdPersonSpectator))
            {
                if (MySession.Static.VirtualClients.Any() && (Sync.Clients.LocalClient != null))
                {
                    MyPlayer nextControlledPlayer = MySession.Static.VirtualClients.GetNextControlledPlayer(MySession.Static.LocalHumanPlayer);
                    MyPlayer player = nextControlledPlayer ?? Sync.Clients.LocalClient.GetPlayer(0);
                    if (player != null)
                    {
                        Sync.Clients.LocalClient.ControlledPlayerSerialId = player.Id.SerialId;
                    }
                }
                else
                {
                    long identityId = MySession.Static.LocalHumanPlayer.Identity.IdentityId;
                    List<MyEntity> list = new List<MyEntity>();
                    foreach (MyEntity entity2 in MyEntities.GetEntities())
                    {
                        MyCharacter character = entity2 as MyCharacter;
                        if (((character != null) && (!character.IsDead && (character.GetIdentity() != null))) && (character.GetIdentity().IdentityId == identityId))
                        {
                            list.Add(entity2);
                        }
                        MyCubeGrid grid = entity2 as MyCubeGrid;
                        if (grid != null)
                        {
                            using (HashSet<MySlimBlock>.Enumerator enumerator2 = grid.GetBlocks().GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    MyCockpit fatBlock = enumerator2.Current.FatBlock as MyCockpit;
                                    if ((fatBlock != null) && ((fatBlock.Pilot != null) && ((fatBlock.Pilot.GetIdentity() != null) && (fatBlock.Pilot.GetIdentity().IdentityId == identityId))))
                                    {
                                        list.Add(fatBlock);
                                    }
                                }
                            }
                        }
                    }
                    int index = list.IndexOf(MySession.Static.ControlledEntity.Entity);
                    List<MyEntity> list2 = new List<MyEntity>();
                    if ((index + 1) < list.Count)
                    {
                        list2.AddRange(list.GetRange(index + 1, (list.Count - index) - 1));
                    }
                    if (index != -1)
                    {
                        list2.AddRange(list.GetRange(0, index + 1));
                    }
                    IMyControllableEntity entity = null;
                    int num3 = 0;
                    while (true)
                    {
                        if (num3 < list2.Count)
                        {
                            if (!(list2[num3] is IMyControllableEntity))
                            {
                                num3++;
                                continue;
                            }
                            entity = list2[num3] as IMyControllableEntity;
                        }
                        if ((MySession.Static.LocalHumanPlayer != null) && (entity != null))
                        {
                            MySession.Static.LocalHumanPlayer.Controller.TakeControl(entity);
                            MyCharacter controlledEntity = MySession.Static.ControlledEntity as MyCharacter;
                            if ((controlledEntity == null) && (MySession.Static.ControlledEntity is MyCockpit))
                            {
                                controlledEntity = (MySession.Static.ControlledEntity as MyCockpit).Pilot;
                            }
                            if (controlledEntity != null)
                            {
                                MySession.Static.LocalHumanPlayer.Identity.ChangeCharacter(controlledEntity);
                            }
                        }
                        break;
                    }
                }
            }
            if (!(MySession.Static.ControlledEntity is MyCharacter))
            {
                MySession.Static.GameFocusManager.Clear();
            }
        }

        private void ShootingCheckChanged(MyGuiControlCheckbox checkBox)
        {
            if ((this.m_selectedSafeZone != null) && !this.m_recreateInProgress)
            {
                this.m_selectedSafeZone.AllowedActions = !checkBox.IsChecked ? (this.m_selectedSafeZone.AllowedActions & ~MySafeZoneAction.Shooting) : (this.m_selectedSafeZone.AllowedActions | MySafeZoneAction.Shooting);
                this.RequestUpdateSafeZone();
            }
        }

        private void ShootingCheckGlobalChanged(MyGuiControlCheckbox checkBox)
        {
            MySessionComponentSafeZones.AllowedActions = !checkBox.IsChecked ? (MySessionComponentSafeZones.AllowedActions & ~MySafeZoneAction.Shooting) : (MySessionComponentSafeZones.AllowedActions | MySafeZoneAction.Shooting);
            MySessionComponentSafeZones.RequestUpdateGlobalSafeZone();
        }

        [Event(null, 0x6d0), Server, Reliable]
        private static void StopEntities_Implementation()
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                foreach (MyEntity entity in MyEntities.GetEntities())
                {
                    if (entity.Physics == null)
                    {
                        continue;
                    }
                    if (!entity.Closed && (!(entity is MyCharacter) && (MySession.Static.Players.GetEntityController(entity) == null)))
                    {
                        entity.Physics.ClearSpeed();
                    }
                }
            }
        }

        private void StoreTrashSettings_RealToTmp()
        {
            this.m_newSettings.flags = MySession.Static.Settings.TrashFlags;
            this.m_newSettings.enable = MySession.Static.Settings.TrashRemovalEnabled;
            this.m_newSettings.blockCount = MySession.Static.Settings.BlockCountThreshold;
            this.m_newSettings.playerDistance = MySession.Static.Settings.PlayerDistanceThreshold;
            this.m_newSettings.gridCount = MySession.Static.Settings.OptimalGridCount;
            this.m_newSettings.playerInactivity = MySession.Static.Settings.PlayerInactivityThreshold;
            this.m_newSettings.characterRemovalThreshold = MySession.Static.Settings.PlayerCharacterRemovalThreshold;
            this.m_newSettings.voxelDistanceFromPlayer = MySession.Static.Settings.VoxelPlayerDistanceThreshold;
            this.m_newSettings.voxelDistanceFromGrid = MySession.Static.Settings.VoxelGridDistanceThreshold;
            this.m_newSettings.voxelAge = MySession.Static.Settings.VoxelAgeThreshold;
            this.m_newSettings.voxelEnable = MySession.Static.Settings.VoxelTrashRemovalEnabled;
            this.m_unsavedTrashSettings = false;
        }

        private void StoreTrashSettings_TmpToReal()
        {
            MySession.Static.Settings.TrashFlags = this.m_newSettings.flags;
            MySession.Static.Settings.TrashRemovalEnabled = this.m_newSettings.enable;
            MySession.Static.Settings.BlockCountThreshold = this.m_newSettings.blockCount;
            MySession.Static.Settings.PlayerDistanceThreshold = this.m_newSettings.playerDistance;
            MySession.Static.Settings.OptimalGridCount = this.m_newSettings.gridCount;
            MySession.Static.Settings.PlayerInactivityThreshold = this.m_newSettings.playerInactivity;
            MySession.Static.Settings.PlayerCharacterRemovalThreshold = this.m_newSettings.characterRemovalThreshold;
            MySession.Static.Settings.VoxelPlayerDistanceThreshold = this.m_newSettings.voxelDistanceFromPlayer;
            MySession.Static.Settings.VoxelGridDistanceThreshold = this.m_newSettings.voxelDistanceFromGrid;
            MySession.Static.Settings.VoxelAgeThreshold = this.m_newSettings.voxelAge;
            MySession.Static.Settings.VoxelTrashRemovalEnabled = this.m_newSettings.voxelEnable;
        }

        private void TimeDeltaChanged(MyGuiControlSlider slider)
        {
            MyTimeOfDayHelper.UpdateTimeOfDay(slider.Value);
            this.m_timeDeltaValue.Text = $"{slider.Value:0.00}";
        }

        private static bool TryAttachCamera(long entityId)
        {
            MyEntity entity;
            if (!MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                return false;
            }
            BoundingSphereD worldVolume = entity.PositionComp.WorldVolume;
            Vector3D? position = null;
            MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, position);
            MySpectatorCameraController.Static.Position = worldVolume.Center + (Math.Max((float) worldVolume.Radius, 1f) * Vector3.One);
            MySpectatorCameraController.Static.Target = worldVolume.Center;
            MySessionComponentAnimationSystem.Static.EntitySelectedForDebug = entity;
            return true;
        }

        private void TryRemoveCurrentCharacter(MyGuiControlButton obj)
        {
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity != null)
            {
                this.SelectNextCharacter();
                if (!ReferenceEquals(MySession.Static.ControlledEntity, controlledEntity))
                {
                    controlledEntity.Entity.Close();
                }
            }
        }

        public override bool Update(bool hasFocus)
        {
            if (this.m_attachCamera != 0)
            {
                TryAttachCamera(this.m_attachCamera);
                UpdateRemoveAndDepowerButton(this, this.m_attachCamera);
            }
            return base.Update(hasFocus);
        }

        private void UpdateCyclingAndDepower()
        {
            int num1;
            if ((m_order == MyEntityCyclingOrder.Characters) || (m_order == MyEntityCyclingOrder.FloatingObjects))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) (m_order != MyEntityCyclingOrder.Gps);
            }
            bool flag = (bool) num1;
            m_cyclingOptions.Enabled = flag;
            if (this.m_depowerItemButton != null)
            {
                this.m_depowerItemButton.Enabled = flag;
            }
        }

        private static void UpdateRemoveAndDepowerButton(MyGuiScreenAdminMenu menu, long entityId)
        {
            MyEntity entity;
            MyEntities.TryGetEntityById(entityId, out entity, false);
            bool flag = (m_currentPage != MyPageEnum.CycleObjects) || (m_order != MyEntityCyclingOrder.Gps);
            menu.m_removeItemButton.Enabled = flag;
            if (menu.m_depowerItemButton != null)
            {
                menu.m_depowerItemButton.Enabled = (entity is MyCubeGrid) & flag;
            }
            if (menu.m_stopItemButton != null)
            {
                menu.m_stopItemButton.Enabled = ((entity != null) && !(entity is MyVoxelBase)) & flag;
            }
            if (m_currentPage == MyPageEnum.CycleObjects)
            {
                if (!(entity is MyVoxelBase))
                {
                    menu.m_labelEntityName.TextToDraw = new StringBuilder(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_EntityName) + ((entity == null) ? "-" : entity.DisplayName));
                }
                else
                {
                    menu.m_labelEntityName.TextToDraw = new StringBuilder(MyTexts.GetString(MyCommonTexts.ScreenDebugAdminMenu_EntityName) + ((MyVoxelBase) entity).StorageName);
                }
            }
        }

        private void UpdateSelectedData()
        {
            this.m_recreateInProgress = true;
            bool flag = this.m_selectedSafeZone != null;
            this.m_enabledCheckbox.Enabled = flag;
            this.m_damageCheckbox.Enabled = flag;
            this.m_shootingCheckbox.Enabled = flag;
            this.m_drillingCheckbox.Enabled = flag;
            this.m_weldingCheckbox.Enabled = flag;
            this.m_grindingCheckbox.Enabled = flag;
            this.m_voxelHandCheckbox.Enabled = flag;
            this.m_buildingCheckbox.Enabled = flag;
            this.m_radiusSlider.Enabled = flag;
            this.m_renameSafeZoneButton.Enabled = flag;
            this.m_removeSafeZoneButton.Enabled = flag;
            this.m_repositionSafeZoneButton.Enabled = flag;
            this.m_moveToSafeZoneButton.Enabled = flag;
            this.m_configureFilterButton.Enabled = flag;
            this.m_safeZonesCombo.Enabled = flag;
            this.m_safeZonesTypeCombo.Enabled = flag;
            this.m_safeZonesAxisCombo.Enabled = flag;
            this.m_sizeSlider.Enabled = flag;
            if (this.m_selectedSafeZone != null)
            {
                this.m_enabledCheckbox.IsChecked = this.m_selectedSafeZone.Enabled;
                if (this.m_selectedSafeZone.Shape == MySafeZoneShape.Sphere)
                {
                    this.m_radiusSlider.Value = this.m_selectedSafeZone.Radius;
                    this.m_zoneRadiusValueLabel.Text = this.m_selectedSafeZone.Radius.ToString();
                }
                else if (this.m_safeZonesAxisCombo.GetSelectedIndex() == 0)
                {
                    this.m_sizeSlider.Value = this.m_selectedSafeZone.Size.X;
                    this.m_zoneRadiusValueLabel.Text = this.m_selectedSafeZone.Size.X.ToString();
                }
                else if (this.m_safeZonesAxisCombo.GetSelectedIndex() == 1)
                {
                    this.m_sizeSlider.Value = this.m_selectedSafeZone.Size.Y;
                    this.m_zoneRadiusValueLabel.Text = this.m_selectedSafeZone.Size.Y.ToString();
                }
                else if (this.m_safeZonesAxisCombo.GetSelectedIndex() == 2)
                {
                    this.m_sizeSlider.Value = this.m_selectedSafeZone.Size.Z;
                    this.m_zoneRadiusValueLabel.Text = this.m_selectedSafeZone.Size.Z.ToString();
                }
                this.m_safeZonesTypeCombo.SelectItemByKey((long) this.m_selectedSafeZone.Shape, true);
                this.m_damageCheckbox.IsChecked = (this.m_selectedSafeZone.AllowedActions & MySafeZoneAction.Damage) > 0;
                this.m_shootingCheckbox.IsChecked = (this.m_selectedSafeZone.AllowedActions & MySafeZoneAction.Shooting) > 0;
                this.m_drillingCheckbox.IsChecked = (this.m_selectedSafeZone.AllowedActions & MySafeZoneAction.Drilling) > 0;
                this.m_weldingCheckbox.IsChecked = (this.m_selectedSafeZone.AllowedActions & MySafeZoneAction.Welding) > 0;
                this.m_grindingCheckbox.IsChecked = (this.m_selectedSafeZone.AllowedActions & MySafeZoneAction.Grinding) > 0;
                this.m_voxelHandCheckbox.IsChecked = (this.m_selectedSafeZone.AllowedActions & MySafeZoneAction.VoxelHand) > 0;
                this.m_buildingCheckbox.IsChecked = (this.m_selectedSafeZone.AllowedActions & MySafeZoneAction.Building) > 0;
            }
            this.m_recreateInProgress = false;
        }

        private void UpdateSelectedGlobalData()
        {
            this.m_damageGlobalCheckbox.IsChecked = (MySessionComponentSafeZones.AllowedActions & MySafeZoneAction.Damage) > 0;
            this.m_shootingGlobalCheckbox.IsChecked = (MySessionComponentSafeZones.AllowedActions & MySafeZoneAction.Shooting) > 0;
            this.m_drillingGlobalCheckbox.IsChecked = (MySessionComponentSafeZones.AllowedActions & MySafeZoneAction.Drilling) > 0;
            this.m_weldingGlobalCheckbox.IsChecked = (MySessionComponentSafeZones.AllowedActions & MySafeZoneAction.Welding) > 0;
            this.m_grindingGlobalCheckbox.IsChecked = (MySessionComponentSafeZones.AllowedActions & MySafeZoneAction.Grinding) > 0;
            this.m_voxelHandGlobalCheckbox.IsChecked = (MySessionComponentSafeZones.AllowedActions & MySafeZoneAction.VoxelHand) > 0;
            this.m_buildingGlobalCheckbox.IsChecked = (MySessionComponentSafeZones.AllowedActions & MySafeZoneAction.Building) > 0;
        }

        private void UpdateSmallLargeGridSelection()
        {
            if (m_currentPage == MyPageEnum.CycleObjects)
            {
                int num1;
                if ((m_order == MyEntityCyclingOrder.Characters) || (m_order == MyEntityCyclingOrder.FloatingObjects))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) (m_order != MyEntityCyclingOrder.Gps);
                }
                bool flag = (bool) num1;
                this.m_removeItemButton.Enabled = true;
                this.m_onlySmallGridsCheckbox.Enabled = flag;
                this.m_onlyLargeGridsCheckbox.Enabled = flag;
            }
        }

        private void UpdateZoneType()
        {
            this.m_zoneRadiusLabel.Visible = false;
            this.m_radiusSlider.Visible = false;
            this.m_selectAxisLabel.Visible = false;
            this.m_zoneSizeLabel.Visible = false;
            this.m_safeZonesAxisCombo.Visible = false;
            this.m_sizeSlider.Visible = false;
            if ((this.m_selectedSafeZone == null) || (this.m_selectedSafeZone.Shape == MySafeZoneShape.Box))
            {
                this.m_selectAxisLabel.Visible = true;
                this.m_zoneSizeLabel.Visible = true;
                this.m_safeZonesAxisCombo.Visible = true;
                this.m_sizeSlider.Visible = true;
            }
            else if (this.m_selectedSafeZone.Shape == MySafeZoneShape.Sphere)
            {
                this.m_zoneRadiusLabel.Visible = true;
                this.m_radiusSlider.Visible = true;
            }
        }

        [Event(null, 0x76a), Reliable, Server]
        private static void UploadSettingsToServer(AdminSettings settings)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MySession.Static.Settings.TrashFlags = settings.flags;
                MySession.Static.Settings.TrashRemovalEnabled = settings.enable;
                MySession.Static.Settings.BlockCountThreshold = settings.blockCount;
                MySession.Static.Settings.PlayerDistanceThreshold = settings.playerDistance;
                MySession.Static.Settings.OptimalGridCount = settings.gridCount;
                MySession.Static.Settings.PlayerInactivityThreshold = settings.playerInactivity;
                MySession.Static.Settings.PlayerCharacterRemovalThreshold = settings.characterRemovalThreshold;
                MySession.Static.Settings.VoxelPlayerDistanceThreshold = settings.voxelDistanceFromPlayer;
                MySession.Static.Settings.VoxelGridDistanceThreshold = settings.voxelDistanceFromGrid;
                MySession.Static.Settings.VoxelAgeThreshold = settings.voxelAge;
                MySession.Static.Settings.VoxelTrashRemovalEnabled = settings.voxelEnable;
            }
        }

        private bool ValidCharacter(long entityId)
        {
            MyEntity entity;
            if (MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                MyPlayer.PlayerId id;
                MyCharacter character = entity as MyCharacter;
                if (((character != null) && Sync.Players.TryGetPlayerId(character.ControllerInfo.ControllingIdentityId, out id)) && (Sync.Players.GetPlayerById(id) != null))
                {
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyScreenManager.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.ScreenDebugAdminMenu_RemoveCharacterNotification), null, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                    return false;
                }
            }
            return true;
        }

        private void ValueChanged(MyEntityList.MyEntitySortOrder selectedOrder)
        {
            int num1;
            m_invertOrder = (this.m_selectedSort == selectedOrder) && !m_invertOrder;
            this.m_selectedSort = selectedOrder;
            List<MyEntityList.MyEntityListInfoItem> items = new List<MyEntityList.MyEntityListInfoItem>(this.m_entityListbox.Items.Count);
            foreach (MyGuiControlListbox.Item item in this.m_entityListbox.Items)
            {
                items.Add((MyEntityList.MyEntityListInfoItem) item.UserData);
            }
            MyEntityList.SortEntityList(selectedOrder, ref items, m_invertOrder);
            this.m_entityListbox.Items.Clear();
            MyEntityList.MyEntityTypeEnum selectedKey = (MyEntityList.MyEntityTypeEnum) ((int) this.m_entityTypeCombo.GetSelectedKey());
            if ((selectedKey == MyEntityList.MyEntityTypeEnum.Grids) || (selectedKey == MyEntityList.MyEntityTypeEnum.LargeGrids))
            {
                num1 = 1;
            }
            else
            {
                num1 = (int) (selectedKey == MyEntityList.MyEntityTypeEnum.SmallGrids);
            }
            bool isGrid = (bool) num1;
            foreach (MyEntityList.MyEntityListInfoItem item2 in items)
            {
                StringBuilder text = MyEntityList.GetFormattedDisplayName(selectedOrder, item2, isGrid);
                int? position = null;
                this.m_entityListbox.Add(new MyGuiControlListbox.Item(text, MyEntityList.GetDescriptionText(item2, isGrid), null, item2, null), position);
            }
        }

        public void ValueChanged(MyEntityList.MyEntityTypeEnum myEntityTypeEnum)
        {
            this.m_selectedType = myEntityTypeEnum;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<MyEntityList.MyEntityTypeEnum>(x => new Action<MyEntityList.MyEntityTypeEnum>(MyGuiScreenAdminMenu.EntityListRequest), myEntityTypeEnum, targetEndpoint, position);
        }

        private void VoxelHandCheckChanged(MyGuiControlCheckbox checkBox)
        {
            if ((this.m_selectedSafeZone != null) && !this.m_recreateInProgress)
            {
                this.m_selectedSafeZone.AllowedActions = !checkBox.IsChecked ? (this.m_selectedSafeZone.AllowedActions & ~MySafeZoneAction.VoxelHand) : (this.m_selectedSafeZone.AllowedActions | MySafeZoneAction.VoxelHand);
                this.RequestUpdateSafeZone();
            }
        }

        private void VoxelHandCheckGlobalChanged(MyGuiControlCheckbox checkBox)
        {
            MySessionComponentSafeZones.AllowedActions = !checkBox.IsChecked ? (MySessionComponentSafeZones.AllowedActions & ~MySafeZoneAction.VoxelHand) : (MySessionComponentSafeZones.AllowedActions | MySafeZoneAction.VoxelHand);
            MySessionComponentSafeZones.RequestUpdateGlobalSafeZone();
        }

        private void WeldingCheckChanged(MyGuiControlCheckbox checkBox)
        {
            if ((this.m_selectedSafeZone != null) && !this.m_recreateInProgress)
            {
                this.m_selectedSafeZone.AllowedActions = !checkBox.IsChecked ? (this.m_selectedSafeZone.AllowedActions & ~MySafeZoneAction.Welding) : (this.m_selectedSafeZone.AllowedActions | MySafeZoneAction.Welding);
                this.RequestUpdateSafeZone();
            }
        }

        private void WeldingCheckGlobalChanged(MyGuiControlCheckbox checkBox)
        {
            MySessionComponentSafeZones.AllowedActions = !checkBox.IsChecked ? (MySessionComponentSafeZones.AllowedActions & ~MySafeZoneAction.Welding) : (MySessionComponentSafeZones.AllowedActions | MySafeZoneAction.Welding);
            MySessionComponentSafeZones.RequestUpdateGlobalSafeZone();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenAdminMenu.<>c <>9 = new MyGuiScreenAdminMenu.<>c();
            public static Func<IMyEventOwner, Action> <>9__157_0;
            public static Func<IMyEventOwner, Action<MyGuiScreenAdminMenu.AdminSettings>> <>9__165_0;
            public static Func<IMyEventOwner, Action<MyGuiScreenAdminMenu.AdminSettings>> <>9__170_0;
            public static Func<IMyEventOwner, Action> <>9__171_0;
            public static Func<IMyEventOwner, Action<MyEntityList.MyEntityTypeEnum>> <>9__175_0;
            public static Func<IMyEventOwner, Action> <>9__185_0;
            public static Func<IMyEventOwner, Action<MyEntityCyclingOrder, bool, bool, float, long, CyclingOptions>> <>9__186_0;
            public static Func<IMyEventOwner, Action<List<long>, List<long>>> <>9__190_0;
            public static Func<IMyEventOwner, Action<List<long>, MyEntityList.EntityListAction>> <>9__193_0;
            public static Func<IMyEventOwner, Action<long, MyEntityList.EntityListAction>> <>9__194_0;
            public static Func<IMyEventOwner, Action<AdminSettingsEnum, ulong>> <>9__195_0;
            public static Func<IMyEventOwner, Action> <>9__197_0;
            public static Func<IMyEventOwner, Action<List<MyEntityList.MyEntityListInfoItem>>> <>9__214_0;
            public static Func<IMyEventOwner, Action<float, long, Vector3D>> <>9__216_0;
            public static Func<IMyEventOwner, Action<AdminSettingsEnum, ulong>> <>9__222_0;

            internal Action <.ctor>b__157_0(IMyEventOwner x) => 
                new Action(MyGuiScreenAdminMenu.RequestSettingFromServer_Implementation);

            internal Action<AdminSettingsEnum, ulong> <AdminSettingsChanged>b__222_0(IMyEventOwner x) => 
                new Action<AdminSettingsEnum, ulong>(MyGuiScreenAdminMenu.AdminSettingsChangedClient);

            internal Action<float, long, Vector3D> <CycleRequest_Implementation>b__216_0(IMyEventOwner x) => 
                new Action<float, long, Vector3D>(MyGuiScreenAdminMenu.Cycle_Implementation);

            internal Action<List<MyEntityList.MyEntityListInfoItem>> <EntityListRequest>b__214_0(IMyEventOwner x) => 
                new Action<List<MyEntityList.MyEntityListInfoItem>>(MyGuiScreenAdminMenu.EntityListResponse);

            internal Action<MyEntityCyclingOrder, bool, bool, float, long, CyclingOptions> <OnCycleClicked>b__186_0(IMyEventOwner x) => 
                new Action<MyEntityCyclingOrder, bool, bool, float, long, CyclingOptions>(MyGuiScreenAdminMenu.CycleRequest_Implementation);

            internal Action<List<long>, MyEntityList.EntityListAction> <OnEntityListActionClicked>b__193_0(IMyEventOwner x) => 
                new Action<List<long>, MyEntityList.EntityListAction>(MyGuiScreenAdminMenu.ProceedEntitiesAction_Implementation);

            internal Action<long, MyEntityList.EntityListAction> <OnEntityOperationClicked>b__194_0(IMyEventOwner x) => 
                new Action<long, MyEntityList.EntityListAction>(MyGuiScreenAdminMenu.ProceedEntity_Implementation);

            internal Action <OnRemoveFloating>b__185_0(IMyEventOwner x) => 
                new Action(MyGuiScreenAdminMenu.RemoveFloating_Implementation);

            internal Action<List<long>, List<long>> <OnRemoveOwnerButton>b__190_0(IMyEventOwner x) => 
                new Action<List<long>, List<long>>(MyGuiScreenAdminMenu.RemoveOwner_Implementation);

            internal Action <OnReplicateEverything>b__197_0(IMyEventOwner x) => 
                new Action(MyGuiScreenAdminMenu.ReplicateEverything_Implementation);

            internal Action <OnStopEntities>b__171_0(IMyEventOwner x) => 
                new Action(MyGuiScreenAdminMenu.StopEntities_Implementation);

            internal Action<AdminSettingsEnum, ulong> <RaiseAdminSettingsChanged>b__195_0(IMyEventOwner x) => 
                new Action<AdminSettingsEnum, ulong>(MyGuiScreenAdminMenu.AdminSettingsChanged);

            internal Action<MyGuiScreenAdminMenu.AdminSettings> <RecalcTrash>b__165_0(IMyEventOwner x) => 
                new Action<MyGuiScreenAdminMenu.AdminSettings>(MyGuiScreenAdminMenu.UploadSettingsToServer);

            internal Action<MyGuiScreenAdminMenu.AdminSettings> <RequestSettingFromServer_Implementation>b__170_0(IMyEventOwner x) => 
                new Action<MyGuiScreenAdminMenu.AdminSettings>(MyGuiScreenAdminMenu.DownloadSettingFromServer);

            internal Action<MyEntityList.MyEntityTypeEnum> <ValueChanged>b__175_0(IMyEventOwner x) => 
                new Action<MyEntityList.MyEntityTypeEnum>(MyGuiScreenAdminMenu.EntityListRequest);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AdminSettings
        {
            public MyTrashRemovalFlags flags;
            public bool enable;
            public int blockCount;
            public float playerDistance;
            public int gridCount;
            public float playerInactivity;
            public int characterRemovalThreshold;
            public bool voxelEnable;
            public float voxelDistanceFromPlayer;
            public float voxelDistanceFromGrid;
            public int voxelAge;
            public AdminSettingsEnum AdminSettingsFlags;
        }

        public enum MyPageEnum
        {
            AdminTools,
            TrashRemoval,
            CycleObjects,
            EntityList,
            SafeZones,
            GlobalSafeZone,
            ReplayTool
        }

        public enum MyRestrictedTypeEnum
        {
            Player,
            Faction,
            Grid,
            FloatingObjects
        }

        public enum MyZoneAxisTypeEnum
        {
            X,
            Y,
            Z
        }

        private enum TrashTab
        {
            General,
            Voxels
        }
    }
}

