namespace SpaceEngineers.Game.GUI
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using SpaceEngineers.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Input;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    [StaticEventOwner]
    public class MyGuiScreenMedicals : MyGuiScreenBase
    {
        private static readonly TimeSpan m_refreshInterval = TimeSpan.FromSeconds(10.0);
        private MyGuiControlLabel m_labelNoRespawn;
        private StringBuilder m_noRespawnHeader;
        private MyGuiControlTable m_respawnsTable;
        private MyGuiControlButton m_respawnButton;
        private MyGuiControlButton m_refreshButton;
        private MyGuiControlButton m_MotdButton;
        private MyGuiControlMultilineText m_noRespawnText;
        private MyGuiControlButton m_backToFactionsButton;
        private MyGuiControlButton m_showPlayersButton;
        private MyGuiControlTable m_factionsTable;
        private MyGuiControlButton m_selectFactionButton;
        private bool m_showFactions;
        private bool m_showMotD;
        private bool m_hideEmptyMotD;
        private bool m_isMotdOpen;
        private string m_lastMotD;
        private MyGuiControlMultilineText m_motdMultiline;
        private bool m_blackgroundDrawFull;
        private float m_blackgroundFade;
        private bool m_isMultiplayerReady;
        private bool m_paused;
        private bool m_medbaySelect_SuppressNext;
        private MyGuiControlMultilineText m_multilineRespawnWhenShipReady;
        private MyRespawnShipDefinition m_selectedRespawnShip;
        private bool m_haveSelection;
        private object m_selectedRowData;
        private MyGuiControlRotatingWheel m_rotatingWheelControl;
        private MyGuiControlLabel m_rotatingWheelLabel;
        private int m_streamingTimeout;
        private bool m_streamingStarted;
        private bool m_selectedRowIsStreamable;
        private DateTime m_nextRefresh;
        private long m_requestedReplicable;
        private bool m_respawning;
        private MyGuiControlTable.Row m_previouslySelected;
        private MyGuiControlParent m_descriptionControl;
        private List<string> m_preloadedTextures;
        private StringBuilder m_factionTooltip;
        private MyFaction m_applyingToFaction;
        private long m_restrictedRespawn;
        private bool m_waitingForRespawnShip;
        private const int SAFE_FRAME_COUNT = 5;
        private int m_blackgroundCounter;
        private int m_lastTimeSec;
        private readonly List<MyPhysics.HitInfo> m_raycastList;
        private float m_cameraRayLength;
        private long m_lastMedicalRoomId;

        public MyGuiScreenMedicals(bool showFactions, long restrictedRespawn) : base(new Vector2(0.1f, 0.1f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.4f, 0.9f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_noRespawnHeader = new StringBuilder();
            this.m_showMotD = true;
            this.m_hideEmptyMotD = true;
            this.m_lastMotD = string.Empty;
            this.m_blackgroundDrawFull = true;
            this.m_blackgroundFade = 1f;
            this.m_streamingTimeout = 240;
            this.m_preloadedTextures = new List<string>();
            this.m_factionTooltip = new StringBuilder();
            this.m_lastTimeSec = -1;
            this.m_raycastList = new List<MyPhysics.HitInfo>(0x10);
            this.m_cameraRayLength = 20f;
            this.m_showFactions = showFactions;
            this.m_restrictedRespawn = restrictedRespawn;
            this.m_showMotD = MySession.ShowMotD && !Sandbox.Engine.Platform.Game.IsDedicated;
            base.m_position = this.GetPossitionFromRatio();
            Static = this;
            base.EnabledBackgroundFade = true;
            base.CloseButtonEnabled = false;
            base.m_closeOnEsc = false;
            this.m_selectedRespawnShip = null;
            base.CanBeHidden = false;
            this.RecreateControls(true);
            if (!Sync.MultiplayerActive)
            {
                MySandboxGame.PausePush();
                this.m_paused = true;
            }
            MySession.Static.Factions.OnPlayerJoined += new Action<MyFaction, long>(this.OnPlayerJoinedFaction);
            MySession.Static.Factions.OnPlayerLeft += new Action<MyFaction, long>(this.OnPlayerKickedFromFaction);
            MyCampaignManager.AfterCampaignLocalizationsLoaded = (Action) Delegate.Combine(MyCampaignManager.AfterCampaignLocalizationsLoaded, new Action(this.AfterLocalizationLoaded));
        }

        private void AddRespawnInSuit()
        {
            MyGuiControlTable.Row row = new MyGuiControlTable.Row(null);
            Color? textColor = null;
            MyGuiHighlightTexture? icon = null;
            row.AddCell(new MyGuiControlTable.Cell(MyTexts.GetString(MySpaceTexts.SpawnInSpaceSuit), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            textColor = null;
            icon = null;
            row.AddCell(new MyGuiControlTable.Cell(MyTexts.GetString(MySpaceTexts.ScreenMedicals_RespawnShipReady), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            this.m_respawnsTable.Add(row);
        }

        private static bool AddShipRespawnInfo(MyRespawnShipDefinition respawnShip, StringBuilder text)
        {
            MySpaceRespawnComponent @static = MySpaceRespawnComponent.Static;
            int timeInSeconds = (MySession.Static.LocalHumanPlayer == null) ? 0 : @static.GetRespawnCooldownSeconds(MySession.Static.LocalHumanPlayer.Id, respawnShip.Id.SubtypeName);
            bool flag = timeInSeconds == 0;
            if (!@static.IsSynced)
            {
                text.Append(MyTexts.Get(MySpaceTexts.ScreenMedicals_RespawnShipNotReady));
            }
            else if (flag)
            {
                text.Append(MyTexts.Get(MySpaceTexts.ScreenMedicals_RespawnShipReady));
            }
            else
            {
                MyValueFormatter.AppendTimeExact(timeInSeconds, text);
            }
            return flag;
        }

        private void AfterLocalizationLoaded()
        {
            if (this.m_motdMultiline != null)
            {
                this.m_motdMultiline.Text = new StringBuilder(MyTexts.SubstituteTexts(this.m_lastMotD, null));
            }
        }

        private static void BuildOxygenLevelInfo(StringBuilder ownerText, float oxygenLevel)
        {
            if (MySession.Static.Settings.EnableOxygen)
            {
                ownerText.Append(MyTexts.GetString(MySpaceTexts.HudInfoOxygen));
                ownerText.Append(": ");
                ownerText.Append((oxygenLevel * 100f).ToString("F0"));
                ownerText.Append("% ");
            }
        }

        private void CheckPermaDeathAndRespawn(Action respawnAction)
        {
            MyIdentity identity = Sync.Players.TryGetIdentity(MySession.Static.LocalPlayerId);
            if (identity != null)
            {
                if (!MySession.Static.Settings.PermanentDeath.Value || !identity.FirstSpawnDone)
                {
                    respawnAction();
                }
                else
                {
                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.MessageBoxCaptionRespawn), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum retval) {
                        if (retval == MyGuiScreenMessageBox.ResultEnum.YES)
                        {
                            respawnAction();
                        }
                    }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }
        }

        public static void Close()
        {
            if (Static != null)
            {
                Static.CloseScreen();
            }
        }

        public void CreateDetailInfoControl()
        {
            float x = 0.25f;
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.BorderSize = 1;
            parent1.BorderEnabled = true;
            parent1.BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f);
            parent1.Visible = false;
            parent1.Size = new Vector2(x, 0f);
            parent1.Position = new Vector2(-0.33f, -0.224f);
            parent1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            MyGuiControlLabel control = new MyGuiControlLabel();
            control.TextToDraw = new StringBuilder();
            control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            parent1.Controls.Add(control);
            MyGuiControlImage image1 = new MyGuiControlImage();
            image1.BorderSize = 1;
            image1.BorderEnabled = true;
            image1.BorderColor = new VRageMath.Vector4(0.235f, 0.274f, 0.314f, 1f);
            image1.Visible = false;
            image1.Size = (Vector2) (x * new Vector2(1f, 0.7f));
            image1.Padding = new MyGuiBorderThickness(2f, 2f, 2f, 2f);
            image1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
            parent1.Controls.Add(image1);
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? backgroundColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText(position, position, backgroundColor, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, false, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding);
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            text1.Size = new Vector2(x, 0.075f);
            parent1.Controls.Add(text1);
            this.m_descriptionControl = parent1;
        }

        public static bool EqualRespawns(object first, object second)
        {
            if ((first == null) || (second == null))
            {
                return (first == second);
            }
            if (first.GetType() != second.GetType())
            {
                return false;
            }
            if (first is MySpaceRespawnComponent.MyRespawnPointInfo)
            {
                MySpaceRespawnComponent.MyRespawnPointInfo info = second as MySpaceRespawnComponent.MyRespawnPointInfo;
                return ((first as MySpaceRespawnComponent.MyRespawnPointInfo).MedicalRoomId == info.MedicalRoomId);
            }
            if (first is MyRespawnShipDefinition)
            {
                MyRespawnShipDefinition definition = first as MyRespawnShipDefinition;
                MyRespawnShipDefinition definition2 = second as MyRespawnShipDefinition;
                return ((definition.Prefab != null) && ((definition2.Prefab != null) && (definition.Prefab.PrefabPath == definition2.Prefab.PrefabPath)));
            }
            if (!(first is MyPlanetInfo))
            {
                return true;
            }
            MyPlanetInfo info2 = second as MyPlanetInfo;
            return ((first as MyPlanetInfo).PlanetId == info2.PlanetId);
        }

        private string GetFactionTooltip(MyFaction faction)
        {
            this.m_factionTooltip.Clear();
            if (faction != null)
            {
                bool flag = (faction.Description != null) && (faction.Description != string.Empty);
                if (!faction.AcceptHumans)
                {
                    this.m_factionTooltip.Append(MyTexts.Get(MySpaceTexts.ScreenMedicals_DoesNotAcceptPlayers));
                    if (flag)
                    {
                        this.m_factionTooltip.Append("\n");
                    }
                }
                else if (!faction.AutoAcceptMember)
                {
                    this.m_factionTooltip.Append(MyTexts.Get(MySpaceTexts.ScreenMedicals_RequiresAcceptance));
                    if (!faction.IsAnyLeaderOnline)
                    {
                        this.m_factionTooltip.Append("\n");
                        this.m_factionTooltip.Append(MyTexts.Get(MySpaceTexts.ScreenMedicals_LeaderNotOnline));
                    }
                    if (flag)
                    {
                        this.m_factionTooltip.Append("\n");
                    }
                }
                if (flag)
                {
                    this.m_factionTooltip.Append(faction.Description);
                }
                if (faction.Members.Count > 0)
                {
                    this.m_factionTooltip.Append("\n").Append("\n");
                    this.m_factionTooltip.Append(MyTexts.Get(MySpaceTexts.TerminalTab_Factions_Members));
                    foreach (KeyValuePair<long, MyFactionMember> pair in faction.Members)
                    {
                        MyIdentity identity = MySession.Static.Players.TryGetIdentity(pair.Key);
                        if (identity != null)
                        {
                            this.m_factionTooltip.Append("\n");
                            this.m_factionTooltip.Append(identity.DisplayName);
                            if (pair.Value.IsLeader)
                            {
                                this.m_factionTooltip.Append(" (").Append(MyTexts.Get(MyCommonTexts.Leader)).Append(")");
                            }
                        }
                    }
                }
            }
            return this.m_factionTooltip.ToString();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenMyGuiScreenMedicals";

        private static StringBuilder GetOwnerDisplayName(long owner)
        {
            if (owner == 0)
            {
                return MyTexts.Get(MySpaceTexts.BlockOwner_Nobody);
            }
            MyIdentity identity = Sync.Players.TryGetIdentity(owner);
            return ((identity == null) ? MyTexts.Get(MySpaceTexts.BlockOwner_Unknown) : new StringBuilder(identity.DisplayName));
        }

        private Vector2 GetPossitionFromRatio()
        {
            Rectangle fullscreenRectangle = MyGuiManager.GetFullscreenRectangle();
            MyAspectRatioEnum closestAspectRatio = MyVideoSettingsManager.GetClosestAspectRatio(((float) fullscreenRectangle.Width) / ((float) fullscreenRectangle.Height));
            switch (closestAspectRatio)
            {
                case MyAspectRatioEnum.Normal_4_3:
                    return new Vector2(0.79f, 0.52f);

                case MyAspectRatioEnum.Normal_16_9:
                    return new Vector2(0.95f, 0.52f);

                case MyAspectRatioEnum.Normal_16_10:
                    return new Vector2(0.88f, 0.52f);
            }
            return ((closestAspectRatio == MyAspectRatioEnum.Unsupported_5_4) ? new Vector2(0.76f, 0.52f) : new Vector2(0.95f, 0.52f));
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape))
            {
                if (!MyInput.Static.IsAnyShiftKeyPressed())
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                    MyGuiSandbox.AddScreen(new MyGuiScreenMainMenu());
                }
                else
                {
                    Close();
                }
            }
        }

        public override bool HandleInputAfterSimulation()
        {
            if ((!this.m_showFactions && (this.m_respawnsTable.SelectedRow != null)) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.Entity))
            {
                MySpaceRespawnComponent.MyRespawnPointInfo userData = this.m_respawnsTable.SelectedRow.UserData as MySpaceRespawnComponent.MyRespawnPointInfo;
                if (userData != null)
                {
                    MyEntity entity;
                    this.m_respawnButton.Enabled = false;
                    if (((this.m_restrictedRespawn == 0) || (userData.MedicalRoomId == this.m_restrictedRespawn)) && MyEntities.TryGetEntityById(userData.MedicalRoomId, out entity, false))
                    {
                        if ((this.m_lastMedicalRoomId != userData.MedicalRoomId) && ((MySession.Static.LocalCharacter == null) || MySession.Static.LocalCharacter.IsDead))
                        {
                            Vector3D? position = null;
                            MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, entity, position);
                            MyThirdPersonSpectator.Static.ResetInternalTimers();
                            MyThirdPersonSpectator.Static.ResetViewerDistance(new double?((double) this.m_cameraRayLength));
                            MyThirdPersonSpectator.Static.RecalibrateCameraPosition(false);
                            MyThirdPersonSpectator.Static.ResetSpring();
                            this.m_lastMedicalRoomId = userData.MedicalRoomId;
                        }
                        this.m_respawnButton.Enabled = true;
                    }
                }
            }
            return true;
        }

        internal void HideMotdButton()
        {
            this.m_MotdButton.Visible = false;
        }

        private bool IsPlayerInFaction() => 
            (MySession.Static.Factions.GetPlayerFaction(MySession.Static.LocalPlayerId) != null);

        private void OnBackToFactionsClick(MyGuiControlButton sender)
        {
            this.m_showFactions = true;
            this.RecreateControls(true);
        }

        protected override void OnClosed()
        {
            if (this.m_paused)
            {
                this.m_paused = false;
                MySandboxGame.PausePop();
            }
            MyHud.RotatingWheelText = MyHud.Empty;
            this.UnrequestReplicable();
            MySession.Static.Factions.OnPlayerJoined -= new Action<MyFaction, long>(this.OnPlayerJoinedFaction);
            MySession.Static.Factions.OnPlayerLeft -= new Action<MyFaction, long>(this.OnPlayerKickedFromFaction);
            MyCampaignManager.AfterCampaignLocalizationsLoaded = (Action) Delegate.Remove(MyCampaignManager.AfterCampaignLocalizationsLoaded, new Action(this.AfterLocalizationLoaded));
            base.OnClosed();
        }

        private void OnEntityStreamedIn(MyEntity entity)
        {
            if (entity.EntityId == this.m_requestedReplicable)
            {
                this.RequestConfirmation();
                MyEntities.OnEntityAdd -= new Action<MyEntity>(this.OnEntityStreamedIn);
            }
        }

        private void OnFactionSelectClick(MyGuiControlButton sender)
        {
            if (this.m_factionsTable.SelectedRow != null)
            {
                MyFaction userData = this.m_factionsTable.SelectedRow.UserData as MyFaction;
                if (this.m_applyingToFaction != null)
                {
                    MyFactionCollection.CancelJoinRequest(this.m_applyingToFaction.FactionId, MySession.Static.LocalPlayerId);
                }
                this.m_applyingToFaction = userData;
                if (userData != null)
                {
                    if (!userData.AcceptHumans)
                    {
                        return;
                    }
                    else
                    {
                        if (!userData.AutoAcceptMember && !userData.IsAnyLeaderOnline)
                        {
                            return;
                        }
                        MyFactionCollection.SendJoinRequest(userData.FactionId, MySession.Static.LocalPlayerId);
                    }
                }
                this.m_showFactions = false;
                this.RecreateControls(true);
            }
        }

        private void OnFactionsRefreshClick(MyGuiControlButton sender)
        {
            this.RefreshFactions();
        }

        private void OnFactionsTableItemDoubleClick(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            this.OnFactionSelectClick(null);
        }

        private void OnFactionsTableItemMouseOver(MyGuiControlTable.Row row)
        {
            this.m_factionsTable.SetToolTip(this.GetFactionTooltip(row.UserData as MyFaction));
        }

        private void OnFactionsTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            this.RefreshSelectFactionButton();
        }

        private void onMotdClick(MyGuiControlButton sender)
        {
            this.m_hideEmptyMotD = false;
            this.m_showMotD = !this.m_isMotdOpen ? !Sandbox.Engine.Platform.Game.IsDedicated : false;
            this.RecreateControls(false);
        }

        private void OnPendingReplicablesDone()
        {
            this.m_isMultiplayerReady = true;
            MyMultiplayer.Static.PendingReplicablesDone -= new Action(this.OnPendingReplicablesDone);
            if (MySession.Static.VoxelMaps.Instances.Count > 0)
            {
                MySandboxGame.AreClipmapsReady = false;
            }
        }

        private void OnPlayerJoinedFaction(MyFaction faction, long identityId)
        {
            if (identityId == MySession.Static.LocalPlayerId)
            {
                this.m_showFactions = false;
                this.m_applyingToFaction = null;
                this.RecreateControls(true);
                if (!this.m_showFactions)
                {
                    this.m_backToFactionsButton.Enabled = false;
                }
            }
        }

        private void OnPlayerKickedFromFaction(MyFaction faction, long identityId)
        {
            if (identityId == MySession.Static.LocalPlayerId)
            {
                this.m_showFactions = false;
                this.RecreateControls(true);
                if (!this.m_showFactions)
                {
                    this.m_backToFactionsButton.Enabled = true;
                }
            }
        }

        private void OnPlayerRejected(MyFaction faction, long identityId)
        {
        }

        private void OnRefreshClick(MyGuiControlButton sender)
        {
            this.RefreshRespawnPoints();
        }

        private void onRespawnClick(MyGuiControlButton sender)
        {
            if (this.m_respawnsTable.SelectedRow != null)
            {
                object userData = this.m_respawnsTable.SelectedRow.UserData;
                if (userData == null)
                {
                    this.CheckPermaDeathAndRespawn(delegate {
                        long? planetId = null;
                        this.RespawnImmediately(null, planetId);
                        if (!Sync.IsServer)
                        {
                            this.RequestConfirmation();
                        }
                    });
                }
                else if (userData is MyRespawnShipDefinition)
                {
                    MyRespawnShipDefinition respawnShip = userData as MyRespawnShipDefinition;
                    if (MySpaceRespawnComponent.Static.GetRespawnCooldownSeconds(MySession.Static.LocalHumanPlayer.Id, respawnShip.Id.SubtypeName) == 0)
                    {
                        this.CheckPermaDeathAndRespawn(delegate {
                            this.RespawnShip(respawnShip.Id.SubtypeName);
                            if (!Sync.IsServer)
                            {
                                this.RequestConfirmation();
                            }
                        });
                    }
                }
                else if (userData is MyPlanetInfo)
                {
                    this.CheckPermaDeathAndRespawn(delegate {
                        this.RespawnImmediately(null, new long?(((MyPlanetInfo) userData).PlanetId));
                        this.ShowBlackground();
                        if (!Sync.IsServer)
                        {
                            this.RequestConfirmation();
                        }
                        else
                        {
                            MySandboxGame.AreClipmapsReady = false;
                        }
                    });
                }
                else if ((this.m_restrictedRespawn == 0) || (this.m_restrictedRespawn == ((MySpaceRespawnComponent.MyRespawnPointInfo) this.m_respawnsTable.SelectedRow.UserData).MedicalRoomId))
                {
                    this.CheckPermaDeathAndRespawn(() => this.RespawnAtMedicalRoom(((MySpaceRespawnComponent.MyRespawnPointInfo) this.m_respawnsTable.SelectedRow.UserData).MedicalRoomId));
                }
            }
        }

        protected override void OnShow()
        {
            MyHud.Notifications.Clear();
        }

        private void OnShowPlayersClick(MyGuiControlButton sender)
        {
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.PlayersScreen, Array.Empty<object>()));
        }

        private void OnTableItemDoubleClick(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            if (this.m_respawnsTable.SelectedRow != null)
            {
                long? nullable = null;
                object userData = this.m_respawnsTable.SelectedRow.UserData;
                MySpaceRespawnComponent.MyRespawnPointInfo info = userData as MySpaceRespawnComponent.MyRespawnPointInfo;
                if (info != null)
                {
                    nullable = new long?(info.MedicalRoomId);
                }
                else
                {
                    MyPlanetInfo info2 = userData as MyPlanetInfo;
                    if (info2 != null)
                    {
                        nullable = new long?(info2.PlanetId);
                    }
                }
                if (nullable == null)
                {
                    goto TR_0000;
                }
                else
                {
                    MyEntity entity;
                    if (!MyEntities.TryGetEntityById(nullable.Value, out entity, false))
                    {
                        StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionNotReady);
                        MyStringId? okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        Vector2? size = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextNotReady), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, false, size));
                    }
                    else
                    {
                        goto TR_0000;
                    }
                }
            }
            return;
        TR_0000:
            this.onRespawnClick(this.m_respawnButton);
        }

        private unsafe void OnTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            if (this.m_medbaySelect_SuppressNext)
            {
                this.m_medbaySelect_SuppressNext = false;
            }
            else if (!ReferenceEquals(this.m_respawnsTable.SelectedRow, this.m_previouslySelected))
            {
                base.FocusedControl = sender;
                this.m_previouslySelected = this.m_respawnsTable.SelectedRow;
                if (this.m_respawnsTable.SelectedRow == null)
                {
                    this.m_haveSelection = false;
                    this.m_selectedRowData = null;
                    this.m_respawnButton.Enabled = false;
                }
                else
                {
                    this.m_respawnButton.Enabled = true;
                    this.m_haveSelection = true;
                    this.m_selectedRowData = this.m_respawnsTable.SelectedRow.UserData;
                    MySpaceRespawnComponent.MyRespawnPointInfo selectedRowData = this.m_selectedRowData as MySpaceRespawnComponent.MyRespawnPointInfo;
                    if (selectedRowData != null)
                    {
                        this.m_selectedRowIsStreamable = true;
                        Vector3D? position = null;
                        MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, position);
                        this.m_lastMedicalRoomId = 0L;
                        this.m_isMultiplayerReady = false;
                        this.ShowBlackground();
                        MySession.RequestVicinityCache(selectedRowData.MedicalRoomGridId);
                        if (!Sync.IsServer && MyEntities.EntityExists(selectedRowData.MedicalRoomGridId))
                        {
                            this.RequestConfirmation();
                            return;
                        }
                        this.RequestReplicable(selectedRowData.MedicalRoomGridId);
                        MyEntities.OnEntityAdd += new Action<MyEntity>(this.OnEntityStreamedIn);
                        return;
                    }
                    MyPlanetInfo info2 = this.m_selectedRowData as MyPlanetInfo;
                    if (info2 != null)
                    {
                        double num;
                        double num2;
                        this.m_selectedRowIsStreamable = true;
                        Vector3 directionToSunNormalized = MySector.DirectionToSunNormalized;
                        BoundingSphereD ed = BoundingSphereD.CreateFromBoundingBox(info2.WorldAABB);
                        ((BoundingSphereD*) ref ed).IntersectRaySphere(new RayD(ed.Center, directionToSunNormalized), out num, out num2);
                        Vector3D vectord = Vector3D.CalculatePerpendicularVector(directionToSunNormalized);
                        Vector3D vectord2 = ed.Center + (directionToSunNormalized * (num2 * 1.5));
                        MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, new Vector3D?(vectord2));
                        MySpectatorCameraController.Static.SetTarget(ed.Center, new Vector3D?(vectord));
                        this.m_isMultiplayerReady = true;
                        return;
                    }
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, 1000000.0);
                }
                this.ShowBlackground();
                this.UnrequestReplicable();
                this.m_selectedRowIsStreamable = false;
            }
        }

        public override void RecreateControls(bool constructor)
        {
            StringBuilder builder;
            VRageMath.Vector4? nullable;
            Vector2? nullable2;
            MyGuiControlLabel label2;
            Vector2 zero = Vector2.Zero;
            if (this.m_showMotD)
            {
                this.m_isMotdOpen = true;
                base.m_size = new Vector2(0.4f, 0.9f);
                base.m_position = new Vector2(0.8f, 0.5f);
                zero = new Vector2(0f, 0f);
                builder = MyTexts.Get(MyCommonTexts.HideMotD);
            }
            else
            {
                this.m_isMotdOpen = false;
                base.m_size = new Vector2(0.4f, 0.9f);
                base.m_position = new Vector2(0.8f, 0.5f);
                zero = new Vector2(0f, 0f);
                builder = MyTexts.Get(MyCommonTexts.ShowMotD);
            }
            base.RecreateControls(constructor);
            if (this.m_showMotD && !string.IsNullOrEmpty(this.m_lastMotD))
            {
                MyGuiControlImage control = new MyGuiControlImage {
                    OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP
                };
                control.SetTexture(MyGuiConstants.TEXTURE_SCREEN_BACKGROUND.Texture);
                control.Size = new Vector2(0.58f, 0.9f);
                control.Position = new Vector2(-0.23f, -0.45f);
                this.Controls.Add(control);
                MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
                nullable = null;
                list.AddHorizontal(new Vector2(-0.61f, 0f) - new Vector2((base.m_size.Value.X * 0.9f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 1.35f, 0f, nullable);
                this.Controls.Add(list);
                MyGuiControlImage image2 = new MyGuiControlImage {
                    OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                    BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER,
                    Size = new Vector2(0.54f, 0.783f),
                    Position = new Vector2(-0.25f, -0.358f)
                };
                this.Controls.Add(image2);
                MyGuiControlMultilineText text1 = new MyGuiControlMultilineText();
                text1.Position = new Vector2(-0.77f, -0.34f);
                text1.Size = new Vector2(0.515f, 0.74f);
                text1.Font = "Blue";
                text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                text1.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                text1.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_motdMultiline = text1;
                this.m_motdMultiline.Text = new StringBuilder(MyTexts.SubstituteTexts(this.m_lastMotD, null));
                this.Controls.Add(this.m_motdMultiline);
                nullable2 = null;
                MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(new Vector2(0f, (-base.m_size.Value.Y / 2f) + MyGuiConstants.SCREEN_CAPTION_DELTA_Y) + new Vector2(-0.52f, 0.003f)), nullable2, MyTexts.GetString(MyCommonTexts.MotD_Caption), new VRageMath.Vector4?(VRageMath.Vector4.One), 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                    Name = "CaptionLabel",
                    Font = "ScreenCaption"
                };
                this.Controls.Add(label);
            }
            if (!this.m_showFactions)
            {
                this.RecreateControlsRespawn(zero);
            }
            else
            {
                this.RecreateControlsFactions(zero);
            }
            nullable = null;
            int? buttonIndex = null;
            this.m_MotdButton = new MyGuiControlButton(new Vector2?(new Vector2(0.003f, (base.m_size.Value.Y / 2f) - 0.045f) + zero), MyGuiControlButtonStyleEnum.Rectangular, new Vector2(0.36f, 0.033f), nullable, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, builder, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.onMotdClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_MotdButton);
            if (Sandbox.Engine.Platform.Game.IsDedicated || string.IsNullOrEmpty(this.m_lastMotD))
            {
                this.m_MotdButton.Enabled = false;
            }
            nullable2 = null;
            nullable = null;
            MyGuiControlLabel label1 = new MyGuiControlLabel(new Vector2(-0.175f, -0.34f), nullable2, MyTexts.GetString(MyCommonTexts.MotDCaption), nullable, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
            this.m_lastMedicalRoomId = 0L;
            nullable = null;
            nullable2 = null;
            this.m_rotatingWheelControl = new MyGuiControlRotatingWheel(new Vector2?(new Vector2(0.5f, 0.8f) - base.m_position), nullable, 0.36f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @"Textures\GUI\screens\screen_loading_wheel.dds", true, true, nullable2, 1.5f);
            this.Controls.Add(this.m_rotatingWheelControl);
            this.m_rotatingWheelLabel = label2 = new MyGuiControlLabel();
            this.Controls.Add(label2);
            MyHud.RotatingWheelText = MyTexts.Get(MySpaceTexts.LoadingWheel_Streaming);
        }

        private void RecreateControlsFactions(Vector2 offsetting)
        {
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.Factions, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.9f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.9f, 0f, captionTextColor);
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.9f) / 2f, ((-base.m_size.Value.Y / 2f) + 0.18f) + 0.01f), base.m_size.Value.X * 0.9f, 0f, captionTextColor);
            this.Controls.Add(control);
            this.m_factionsTable = new MyGuiControlTable();
            this.m_factionsTable.Position = (new Vector2(0f, (-base.m_size.Value.Y / 2f) + 0.7f) + offsetting) + new Vector2(0f, -0.604f);
            this.m_factionsTable.Size = new Vector2(575f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 1.3f);
            this.m_factionsTable.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            this.m_factionsTable.VisibleRowsCount = 0x10;
            this.Controls.Add(this.m_factionsTable);
            this.m_factionsTable.ColumnsCount = 2;
            this.m_factionsTable.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnFactionsTableItemSelected);
            this.m_factionsTable.ItemDoubleClicked += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnFactionsTableItemDoubleClick);
            this.m_factionsTable.ItemMouseOver += new Action<MyGuiControlTable.Row>(this.OnFactionsTableItemMouseOver);
            float[] p = new float[] { 0.2f, 0.8f };
            this.m_factionsTable.SetCustomColumnWidths(p);
            this.m_factionsTable.SetColumnName(0, MyTexts.Get(MyCommonTexts.Tag));
            this.m_factionsTable.SetColumnName(1, MyTexts.Get(MyCommonTexts.Name));
            captionTextColor = null;
            int? buttonIndex = null;
            this.m_showPlayersButton = new MyGuiControlButton(new Vector2?(new Vector2(0.003f, (base.m_size.Value.Y / 2f) - 0.155f) + offsetting), MyGuiControlButtonStyleEnum.Rectangular, new Vector2(0.36f, 0.033f), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonPlayers), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnShowPlayersClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_showPlayersButton);
            this.m_showPlayersButton.Enabled = MyMultiplayer.Static != null;
            Vector2? size = null;
            captionTextColor = null;
            buttonIndex = null;
            this.m_selectFactionButton = new MyGuiControlButton(new Vector2?(new Vector2(-0.09f, (base.m_size.Value.Y / 2f) - 0.1f) + offsetting), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.TerminalTab_Factions_Join), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnFactionSelectClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_selectFactionButton);
            base.FocusedControl = this.m_selectFactionButton;
            size = null;
            captionTextColor = null;
            buttonIndex = null;
            this.m_refreshButton = new MyGuiControlButton(new Vector2?(new Vector2(0.095f, (base.m_size.Value.Y / 2f) - 0.1f) + offsetting), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Refresh), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnFactionsRefreshClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_refreshButton);
            this.RefreshFactions();
        }

        private void RecreateControlsRespawn(Vector2 offsetting)
        {
            int? nullable3;
            MyGuiBorderThickness? nullable4;
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.Medicals_Title, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.9f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.9f, 0f, captionTextColor);
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.9f) / 2f, ((-base.m_size.Value.Y / 2f) + 0.18f) + 0.01f), base.m_size.Value.X * 0.9f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText();
            text1.Position = new Vector2(0f, (-0.5f * base.Size.Value.Y) + (80f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y));
            Vector2? size = base.Size;
            text1.Size = new Vector2(size.Value.X * 0.85f, 75f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            text1.Font = "Red";
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            text1.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            text1.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            this.m_multilineRespawnWhenShipReady = text1;
            this.Controls.Add(this.m_multilineRespawnWhenShipReady);
            this.UpdateRespawnShipLabel();
            this.m_respawnsTable = new MyGuiControlTable();
            this.m_respawnsTable.Position = (new Vector2(0f, (-base.m_size.Value.Y / 2f) + 0.7f) + offsetting) + new Vector2(0f, -0.03f);
            this.m_respawnsTable.Size = new Vector2(575f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 1.3f);
            this.m_respawnsTable.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
            this.m_respawnsTable.VisibleRowsCount = 0x10;
            this.Controls.Add(this.m_respawnsTable);
            this.m_respawnsTable.ColumnsCount = 2;
            this.m_respawnsTable.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            this.m_respawnsTable.ItemDoubleClicked += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemDoubleClick);
            this.m_respawnsTable.ItemMouseOver += new Action<MyGuiControlTable.Row>(this.respawnsTable_ItemMouseOver);
            float[] p = new float[] { 0.5f, 0.5f };
            this.m_respawnsTable.SetCustomColumnWidths(p);
            this.m_respawnsTable.SetColumnName(0, MyTexts.Get(MyCommonTexts.Name));
            this.m_respawnsTable.SetColumnName(1, MyTexts.Get(MySpaceTexts.ScreenMedicals_OwnerTimeoutColumn));
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(0f, -0.35f) + offsetting;
            label1.ColorMask = (VRageMath.Vector4) Color.Red;
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            this.m_labelNoRespawn = label1;
            this.Controls.Add(this.m_labelNoRespawn);
            if (this.m_applyingToFaction != null)
            {
                captionTextColor = null;
                nullable3 = null;
                nullable4 = null;
                MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2?(new Vector2(-0.02f, (base.m_size.Value.Y / 2f) - 0.21f) + offsetting), new Vector2(0.32f, 0.5f), captionTextColor, "Red", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyTexts.Get(MySpaceTexts.ScreenMedicals_WaitingForAcceptance), true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, nullable3, false, false, null, nullable4);
                this.Controls.Add(text);
            }
            captionTextColor = null;
            nullable3 = null;
            this.m_backToFactionsButton = new MyGuiControlButton(new Vector2?(new Vector2(0.003f, (base.m_size.Value.Y / 2f) - 0.155f) + offsetting), MyGuiControlButtonStyleEnum.Rectangular, new Vector2(0.36f, 0.033f), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.ScreenMedicals_BackToFactionSelection), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnBackToFactionsClick), GuiSounds.MouseClick, 1f, nullable3, false);
            this.Controls.Add(this.m_backToFactionsButton);
            this.m_backToFactionsButton.Enabled = !this.IsPlayerInFaction();
            size = null;
            captionTextColor = null;
            nullable3 = null;
            this.m_respawnButton = new MyGuiControlButton(new Vector2?(new Vector2(-0.09f, (base.m_size.Value.Y / 2f) - 0.1f) + offsetting), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Respawn), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.onRespawnClick), GuiSounds.MouseClick, 1f, nullable3, false);
            this.Controls.Add(this.m_respawnButton);
            base.FocusedControl = this.m_respawnButton;
            size = null;
            captionTextColor = null;
            nullable3 = null;
            this.m_refreshButton = new MyGuiControlButton(new Vector2?(new Vector2(0.095f, (base.m_size.Value.Y / 2f) - 0.1f) + offsetting), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Refresh), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnRefreshClick), GuiSounds.MouseClick, 1f, nullable3, false);
            this.Controls.Add(this.m_refreshButton);
            captionTextColor = null;
            nullable3 = null;
            nullable4 = null;
            this.m_noRespawnText = new MyGuiControlMultilineText(new Vector2?(new Vector2(-0.02f, -0.19f) + offsetting), new Vector2(0.32f, 0.5f), captionTextColor, "Red", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, MyTexts.Get(MySpaceTexts.ScreenMedicals_NoRespawnPossible), true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, nullable3, false, false, null, nullable4);
            this.Controls.Add(this.m_noRespawnText);
            this.CreateDetailInfoControl();
            this.RefreshRespawnPoints();
            this.Controls.Add(this.m_descriptionControl);
        }

        private void RefreshFactions()
        {
            Color? nullable;
            MyGuiHighlightTexture? nullable2;
            this.m_factionsTable.Clear();
            foreach (KeyValuePair<long, MyFaction> pair in MySession.Static.Factions)
            {
                Color? nullable1;
                Color? nullable3;
                bool flag = pair.Value.AcceptHumans && (pair.Value.AutoAcceptMember || pair.Value.IsAnyLeaderOnline);
                MyGuiControlTable.Row row = new MyGuiControlTable.Row(pair.Value);
                if (!flag)
                {
                    nullable1 = new Color?(Color.Red);
                }
                else
                {
                    nullable = null;
                    nullable1 = nullable;
                }
                nullable2 = null;
                null.AddCell(new MyGuiControlTable.Cell(null, pair.Value.Tag, (string) row, nullable1, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                if (!flag)
                {
                    nullable3 = new Color?(Color.Red);
                }
                else
                {
                    nullable = null;
                    nullable3 = nullable;
                }
                nullable2 = null;
                row.AddCell(new MyGuiControlTable.Cell(pair.Value.Name, null, null, nullable3, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                this.m_factionsTable.Add(row);
            }
            if ((MySession.Static.Settings.BlockLimitsEnabled != MyBlockLimitsEnabledEnum.PER_FACTION) || MySession.Static.IsUserAdmin(Sync.MyId))
            {
                MyGuiControlTable.Row row = new MyGuiControlTable.Row(null);
                nullable = null;
                nullable2 = null;
                row.AddCell(new MyGuiControlTable.Cell(null, null, null, nullable, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                nullable = null;
                nullable2 = null;
                row.AddCell(new MyGuiControlTable.Cell(MyTexts.Get(MySpaceTexts.ScreenMedicals_NoFaction), null, null, nullable, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                this.m_factionsTable.Add(row);
            }
            this.RefreshSelectFactionButton();
        }

        private void RefreshMedicalRooms(ListReader<MySpaceRespawnComponent.MyRespawnPointInfo> medicalRooms, MyPlanetInfo[] planetInfos)
        {
            Color? nullable;
            MyGuiHighlightTexture? nullable2;
            this.m_respawnsTable.Clear();
            foreach (MySpaceRespawnComponent.MyRespawnPointInfo info in medicalRooms)
            {
                Color? nullable1;
                Color? nullable3;
                MyGuiControlTable.Row row = new MyGuiControlTable.Row(info);
                bool flag = (this.m_restrictedRespawn == 0) || (info.MedicalRoomId == this.m_restrictedRespawn);
                if (!flag)
                {
                    nullable1 = new Color?(Color.Gray);
                }
                else
                {
                    nullable = null;
                    nullable1 = nullable;
                }
                nullable2 = null;
                null.AddCell(new MyGuiControlTable.Cell(null, info.MedicalRoomName, (string) row, nullable1, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                if (!flag)
                {
                    nullable3 = new Color?(Color.Gray);
                }
                else
                {
                    nullable = null;
                    nullable3 = nullable;
                }
                nullable2 = null;
                row.AddCell(new MyGuiControlTable.Cell(flag ? MyTexts.Get(MySpaceTexts.ScreenMedicals_RespawnShipReady) : MyTexts.Get(MySpaceTexts.ScreenMedicals_RespawnShipNotReady), null, null, nullable3, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                this.m_respawnsTable.Add(row);
            }
            if ((!MySession.Static.CreativeMode && MySession.Static.Settings.EnableRespawnShips) && !MySession.Static.Settings.Scenario)
            {
                MyPlanetInfo[] infoArray = planetInfos;
                int index = 0;
                while (true)
                {
                    if (index >= infoArray.Length)
                    {
                        this.RefreshSpawnShips();
                        break;
                    }
                    MyPlanetInfo userData = infoArray[index];
                    MyGuiControlTable.Row row = new MyGuiControlTable.Row(userData);
                    nullable = null;
                    nullable2 = null;
                    row.AddCell(new MyGuiControlTable.Cell(string.Format(MyTexts.GetString(MySpaceTexts.PlanetRespawnPod), userData.PlanetName), null, null, nullable, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                    nullable = null;
                    nullable2 = null;
                    row.AddCell(new MyGuiControlTable.Cell(MyTexts.Get(MySpaceTexts.ScreenMedicals_RespawnShipReady), null, null, nullable, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                    this.m_respawnsTable.Add(row);
                    index++;
                }
            }
            this.AddRespawnInSuit();
            if (this.m_respawnsTable.RowsCount <= 0)
            {
                this.m_noRespawnText.Visible = true;
            }
            else
            {
                if (!this.m_haveSelection)
                {
                    this.m_respawnsTable.SelectedRowIndex = 0;
                }
                else
                {
                    int num2 = this.m_respawnsTable.FindIndexByUserData(ref this.m_selectedRowData, new MyGuiControlTable.EqualUserData(MyGuiScreenMedicals.EqualRespawns));
                    if (num2 < 0)
                    {
                        this.m_respawnsTable.SelectedRowIndex = 0;
                    }
                    else
                    {
                        this.m_medbaySelect_SuppressNext = true;
                        this.m_respawnsTable.SelectedRowIndex = new int?(num2);
                    }
                }
                MyGuiControlTable.EventArgs eventArgs = new MyGuiControlTable.EventArgs();
                this.OnTableItemSelected(null, eventArgs);
                this.m_noRespawnText.Visible = false;
            }
        }

        private void RefreshRespawnPoints()
        {
            this.m_respawnsTable.Clear();
            this.m_nextRefresh = DateTime.UtcNow + m_refreshInterval;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(s => new Action(MyGuiScreenMedicals.RefreshRespawnPointsRequest), targetEndpoint, position);
        }

        [Event(null, 0x2b8), Reliable, Server]
        private static void RefreshRespawnPointsRequest()
        {
            long num = MySession.Static.Players.TryGetIdentityId(MyEventContext.Current.Sender.Value, 0);
            if (num != 0)
            {
                using (ClearToken<MySpaceRespawnComponent.MyRespawnPointInfo> token = MySpaceRespawnComponent.GetAvailableRespawnPoints(new long?(num), true))
                {
                    MyPlanetInfo[] infoArray = VRage.Library.Collections.EmptyArray<MyPlanetInfo>.Value;
                    if (MySession.Static.Settings.EnableRespawnShips)
                    {
                        infoArray = (from x in MyPlanets.GetPlanets().Select<MyPlanet, MyPlanetInfo>(delegate (MyPlanet x) {
                            MyPlanetInfo info;
                            using (ClearToken<MyRespawnShipDefinition> token = MySpaceRespawnComponent.GetRespawnShips(x))
                            {
                                if (token.List.Count == 0)
                                {
                                    info = null;
                                }
                                else
                                {
                                    string helpTextLocalizationId = null;
                                    string str2 = null;
                                    if (token.List.Count == 1)
                                    {
                                        MyRespawnShipDefinition definition = token.List[0];
                                        if ((definition.Icons != null) && (definition.Icons.Length != 0))
                                        {
                                            str2 = definition.Icons[0];
                                        }
                                        helpTextLocalizationId = definition.HelpTextLocalizationId;
                                    }
                                    float oxygenDensity = 0f;
                                    if (x.HasAtmosphere)
                                    {
                                        MyPlanetAtmosphere atmosphere = x.Generator.Atmosphere;
                                        if (atmosphere.Breathable)
                                        {
                                            oxygenDensity = atmosphere.OxygenDensity;
                                        }
                                    }
                                    MyPlanetInfo info1 = new MyPlanetInfo();
                                    info1.PlanetName = x.Name;
                                    info1.PlanetId = x.EntityId;
                                    info1.OxygenLevel = oxygenDensity;
                                    info1.DropPodThumbnail = str2;
                                    info1.WorldAABB = x.PositionComp.WorldAABB;
                                    info1.Gravity = x.GetInitArguments.SurfaceGravity;
                                    info1.HelpTextLocalizationId = helpTextLocalizationId;
                                    info = info1;
                                }
                            }
                            return info;
                        })
                            where x != null
                            select x).ToArray<MyPlanetInfo>();
                    }
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<List<MySpaceRespawnComponent.MyRespawnPointInfo>, MyPlanetInfo[]>(s => new Action<List<MySpaceRespawnComponent.MyRespawnPointInfo>, MyPlanetInfo[]>(MyGuiScreenMedicals.RequestRespawnPointsResponse), token.List, infoArray, MyEventContext.Current.Sender, position);
                }
            }
        }

        private void RefreshRotatingWheelLabel()
        {
            int num1;
            if (MyHud.RotatingWheelVisible || this.m_respawning)
            {
                num1 = 1;
            }
            else
            {
                num1 = !this.m_blackgroundDrawFull ? 0 : ((int) this.m_selectedRowIsStreamable);
            }
            bool flag = (bool) num1;
            this.m_rotatingWheelLabel.Visible = flag;
            this.m_rotatingWheelControl.Visible = flag;
            if ((MyHud.RotatingWheelVisible || this.m_respawning) && !ReferenceEquals(this.m_rotatingWheelLabel.TextToDraw, MyHud.RotatingWheelText))
            {
                this.m_rotatingWheelLabel.Position = this.m_rotatingWheelControl.Position + new Vector2(0f, 0.05f);
                this.m_rotatingWheelLabel.TextToDraw = MyHud.RotatingWheelText;
                Vector2 textSize = this.m_rotatingWheelLabel.GetTextSize();
                this.m_rotatingWheelLabel.PositionX -= textSize.X / 2f;
            }
        }

        private void RefreshSelectFactionButton()
        {
            if (this.m_factionsTable.SelectedRow == null)
            {
                this.m_selectFactionButton.Enabled = false;
            }
            else
            {
                MyFaction userData = this.m_factionsTable.SelectedRow.UserData as MyFaction;
                this.m_selectFactionButton.Enabled = (userData == null) || (userData.AcceptHumans && (userData.AutoAcceptMember || userData.IsAnyLeaderOnline));
            }
        }

        private void RefreshSpawnShips()
        {
            foreach (MyRespawnShipDefinition definition in MyDefinitionManager.Static.GetRespawnShipDefinitions().Values)
            {
                if (definition.UseForSpace)
                {
                    MyGuiControlTable.Row row = new MyGuiControlTable.Row(definition);
                    Color? textColor = null;
                    MyGuiHighlightTexture? icon = null;
                    row.AddCell(new MyGuiControlTable.Cell(definition.DisplayNameText, null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                    textColor = null;
                    icon = null;
                    MyGuiControlTable.Cell cell = new MyGuiControlTable.Cell(string.Empty, null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                    AddShipRespawnInfo(definition, cell.Text);
                    row.AddCell(cell);
                    this.m_respawnsTable.Add(row);
                }
            }
        }

        private void RequestConfirmation()
        {
            this.m_isMultiplayerReady = false;
            (MyMultiplayer.Static as MyMultiplayerClientBase).RequestBatchConfirmation();
            MyMultiplayer.Static.PendingReplicablesDone += new Action(this.OnPendingReplicablesDone);
        }

        private void RequestReplicable(long replicableId)
        {
            if (this.m_requestedReplicable != replicableId)
            {
                this.UnrequestReplicable();
                this.m_requestedReplicable = replicableId;
                MyReplicationClient replicationLayer = MyMultiplayer.ReplicationLayer as MyReplicationClient;
                if (replicationLayer != null)
                {
                    replicationLayer.RequestReplicable(this.m_requestedReplicable, 0, true);
                }
            }
        }

        [Event(null, 0x2f7), Reliable, Client]
        private static void RequestRespawnPointsResponse(List<MySpaceRespawnComponent.MyRespawnPointInfo> medicalRooms, MyPlanetInfo[] planetInfos)
        {
            Static.RefreshMedicalRooms(medicalRooms, planetInfos);
        }

        private void RespawnAtMedicalRoom(long medicalId)
        {
            string model = null;
            Color red = Color.Red;
            MyLocalCache.GetCharacterInfoFromInventoryConfig(ref model, ref red);
            MyPlayerCollection.RespawnRequest(ReferenceEquals(MySession.Static.LocalCharacter, null), false, medicalId, null, 0, model, red);
            this.m_respawning = true;
            this.m_respawnButton.Visible = false;
            this.m_respawnsTable.Enabled = false;
        }

        private void RespawnImmediately(string shipPrefabId, long? planetId)
        {
            MyIdentity identity = Sync.Players.TryGetIdentity(MySession.Static.LocalPlayerId);
            bool newIdentity = (identity == null) || identity.FirstSpawnDone;
            if (!Sync.IsServer || (string.IsNullOrEmpty(shipPrefabId) && (planetId == null)))
            {
                this.m_waitingForRespawnShip = false;
            }
            else
            {
                this.m_waitingForRespawnShip = true;
                MySpaceRespawnComponent.Static.RespawnDoneEvent += new Action<ulong>(this.RespawnShipDoneEvent);
                MyPlayerCollection.OnRespawnRequestFailureEvent += new Action<ulong>(this.RespawnShipDoneEvent);
            }
            string model = null;
            Color red = Color.Red;
            MyLocalCache.GetCharacterInfoFromInventoryConfig(ref model, ref red);
            long? nullable = planetId;
            MyPlayerCollection.RespawnRequest(ReferenceEquals(MySession.Static.LocalCharacter, null), newIdentity, (nullable != null) ? nullable.GetValueOrDefault() : 0L, shipPrefabId, 0, model, red);
            this.m_respawning = true;
            this.m_respawnButton.Visible = false;
            this.m_respawnsTable.Enabled = false;
        }

        private void RespawnShip(string shipPrefabId)
        {
            MySpaceRespawnComponent @static = MySpaceRespawnComponent.Static;
            int num = (MySession.Static.LocalHumanPlayer == null) ? 0 : @static.GetRespawnCooldownSeconds(MySession.Static.LocalHumanPlayer.Id, shipPrefabId);
            if (@static.IsSynced && (num == 0))
            {
                long? planetId = null;
                this.RespawnImmediately(shipPrefabId, planetId);
            }
            else
            {
                MyRespawnShipDefinition respawnShipDefinition = MyDefinitionManager.Static.GetRespawnShipDefinition(shipPrefabId);
                this.m_selectedRespawnShip = respawnShipDefinition;
                this.UpdateRespawnShipLabel();
            }
        }

        private void RespawnShipDoneEvent(ulong steamId)
        {
            this.m_waitingForRespawnShip = false;
            MySpaceRespawnComponent.Static.RespawnDoneEvent -= new Action<ulong>(this.RespawnShipDoneEvent);
            MyPlayerCollection.OnRespawnRequestFailureEvent -= new Action<ulong>(this.RespawnShipDoneEvent);
        }

        private void respawnsTable_ItemMouseOver(MyGuiControlTable.Row row)
        {
            this.UpdateDetailedInfo(row);
        }

        public void SetMotD(string motd)
        {
            this.m_lastMotD = motd;
            if (this.m_motdMultiline != null)
            {
                this.m_motdMultiline.Text = new StringBuilder(MyTexts.SubstituteTexts(this.m_lastMotD, null));
            }
            if (!string.IsNullOrEmpty(this.m_lastMotD) && !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.m_MotdButton.Enabled = true;
            }
            else
            {
                this.m_showMotD = false;
                this.m_MotdButton.Enabled = false;
            }
            if (this.m_showMotD)
            {
                this.RecreateControls(false);
            }
        }

        public static void SetNoRespawnText(StringBuilder text, int timeSec)
        {
            if (Static != null)
            {
                Static.SetNoRespawnTexts(text, timeSec);
            }
        }

        public void SetNoRespawnTexts(StringBuilder text, int timeSec)
        {
            NoRespawnText = text;
            if (timeSec != this.m_lastTimeSec)
            {
                this.m_lastTimeSec = timeSec;
                int num = timeSec / 60;
                this.m_noRespawnHeader.Clear().AppendFormat(MyTexts.GetString(MySpaceTexts.ScreenMedicals_NoRespawnPlaceHeader), num, timeSec - (num * 60));
                this.m_labelNoRespawn.Text = this.m_noRespawnHeader.ToString();
            }
        }

        public void ShowBlackground()
        {
            this.m_blackgroundCounter = 5;
            this.m_blackgroundDrawFull = true;
            this.m_blackgroundFade = 1f;
            this.m_streamingTimeout = 120;
        }

        internal static void ShowMotDUrl(string url)
        {
            if (MySession.ShowMotD)
            {
                MyGuiSandbox.OpenUrl(url, UrlOpenMode.SteamOrExternalWithConfirm, null);
            }
        }

        private void UnrequestReplicable()
        {
            if (this.m_requestedReplicable != 0)
            {
                MyReplicationClient replicationLayer = MyMultiplayer.ReplicationLayer as MyReplicationClient;
                if (replicationLayer != null)
                {
                    replicationLayer.RequestReplicable(this.m_requestedReplicable, 0, false);
                    this.m_requestedReplicable = 0L;
                }
            }
        }

        public override bool Update(bool hasFocus)
        {
            if (!this.m_showFactions)
            {
                this.UpdateSpawnShipTimes();
            }
            if (MySandboxGame.IsPaused)
            {
                MyHud.Notifications.UpdateBeforeSimulation();
            }
            if (this.IsBlackgroundVisible)
            {
                Rectangle rectangle;
                this.UpdateBlackground();
                MyGuiManager.GetSafeHeightFullScreenPictureSize(MyGuiConstants.LOADING_BACKGROUND_TEXTURE_REAL_SIZE, out rectangle);
                MyGuiManager.DrawSpriteBatch(@"Textures\Gui\Screens\screen_background.dds", rectangle, new Color(new VRageMath.Vector4(0f, 0f, 0f, this.m_blackgroundFade)), true);
            }
            if (!this.m_showFactions)
            {
                if (this.m_selectedRespawnShip != null)
                {
                    MySpaceRespawnComponent @static = MySpaceRespawnComponent.Static;
                    int respawnCooldownSeconds = @static.GetRespawnCooldownSeconds(MySession.Static.LocalHumanPlayer.Id, this.m_selectedRespawnShip.Id.SubtypeName);
                    if (@static.IsSynced && (respawnCooldownSeconds == 0))
                    {
                        long? planetId = null;
                        this.RespawnImmediately(this.m_selectedRespawnShip.Id.SubtypeName, planetId);
                    }
                }
                if (DateTime.UtcNow > this.m_nextRefresh)
                {
                    this.RefreshRespawnPoints();
                }
                this.m_labelNoRespawn.Visible = this.m_labelNoRespawn.Text != null;
            }
            this.m_rotatingWheelControl.Visible = MyHud.RotatingWheelVisible;
            this.RefreshRotatingWheelLabel();
            if ((this.m_respawning && ((MySession.Static.LocalCharacter != null) && !MySession.Static.LocalCharacter.IsDead)) && !this.m_blackgroundDrawFull)
            {
                if (this.m_paused)
                {
                    this.m_paused = false;
                    MySandboxGame.PausePop();
                }
                this.m_blackgroundCounter--;
                if (this.m_blackgroundCounter <= 0)
                {
                    this.CloseScreen();
                }
            }
            return base.Update(hasFocus);
        }

        private void UpdateBlackground()
        {
            if (!this.m_blackgroundDrawFull)
            {
                if (this.m_blackgroundFade > 0f)
                {
                    this.m_blackgroundFade -= 0.1f;
                }
            }
            else if ((this.m_selectedRowIsStreamable || this.m_respawning) && ((MySandboxGame.IsGameReady && (((Sync.IsServer || !MyFakes.ENABLE_WAIT_UNTIL_MULTIPLAYER_READY) || this.m_isMultiplayerReady) && !this.m_waitingForRespawnShip)) && MySandboxGame.AreClipmapsReady))
            {
                this.m_blackgroundDrawFull = false;
            }
        }

        private void UpdateDetailedInfo(MyGuiControlTable.Row row)
        {
            MyGuiControlParent descriptionControl = this.m_descriptionControl;
            MyGuiControlImage image = (MyGuiControlImage) descriptionControl.Controls[1];
            MyGuiControlLabel label = (MyGuiControlLabel) descriptionControl.Controls[0];
            MyGuiControlMultilineText text = descriptionControl.Controls[2] as MyGuiControlMultilineText;
            descriptionControl.Position = new Vector2(-0.33f, -0.3f);
            descriptionControl.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            string dropPodThumbnail = null;
            StringBuilder ownerText = label.TextToDraw.Clear();
            if ((row != null) && (row.UserData != null))
            {
                string helpTextLocalizationId = null;
                MyRespawnShipDefinition userData = row.UserData as MyRespawnShipDefinition;
                if (userData != null)
                {
                    if ((userData.Icons != null) && (userData.Icons.Length != 0))
                    {
                        dropPodThumbnail = userData.Icons[0];
                    }
                    helpTextLocalizationId = userData.HelpTextLocalizationId;
                    ownerText.Append(MyTexts.GetString(MySpaceTexts.Difficulty)).Append(": ");
                    ownerText.Append(MyTexts.GetString(MySpaceTexts.DifficultyHard)).AppendLine();
                }
                else
                {
                    MyPlanetInfo info = row.UserData as MyPlanetInfo;
                    if (info != null)
                    {
                        helpTextLocalizationId = info.HelpTextLocalizationId;
                    }
                    if ((info == null) || (info.DropPodThumbnail == null))
                    {
                        MySpaceRespawnComponent.MyRespawnPointInfo info2 = row.UserData as MySpaceRespawnComponent.MyRespawnPointInfo;
                        if (info2 != null)
                        {
                            ownerText.Append(MyTexts.GetString(MySpaceTexts.ScreenMedicals_Owner));
                            ownerText.Append(": ").Append(GetOwnerDisplayName(info2.OwnerId));
                            ownerText.AppendLine();
                            BuildOxygenLevelInfo(ownerText, info2.OxygenLevel);
                        }
                    }
                    else
                    {
                        dropPodThumbnail = info.DropPodThumbnail;
                        int num2 = 0;
                        if (info.Gravity < 0.5)
                        {
                            num2++;
                        }
                        if (info.OxygenLevel < 0.5)
                        {
                            num2++;
                        }
                        MyStringId id = (num2 < 2) ? ((num2 < 1) ? MySpaceTexts.DifficultyEasy : MySpaceTexts.DifficultyNormal) : MySpaceTexts.DifficultyHard;
                        ownerText.Append(MyTexts.GetString(MySpaceTexts.Difficulty)).Append(": ");
                        ownerText.AppendLine(MyTexts.GetString(id)).AppendLine();
                        BuildOxygenLevelInfo(ownerText, info.OxygenLevel);
                        ownerText.AppendLine();
                        ownerText.Append(MyTexts.GetString(MySpaceTexts.HudInfoGravityNatural));
                        ownerText.Append(' ').Append(info.Gravity.ToString("F2")).AppendLine("g");
                    }
                }
                if (string.IsNullOrEmpty(helpTextLocalizationId))
                {
                    text.Visible = false;
                }
                else
                {
                    text.Text = new StringBuilder(string.Format(MyTexts.GetString(helpTextLocalizationId), MySession.Static.Settings.OptimalSpawnDistance));
                    text.Visible = MySession.Static.Settings.EnableAutorespawn;
                }
            }
            bool flag = ownerText.Length > 0;
            bool flag2 = !string.IsNullOrEmpty(dropPodThumbnail);
            image.Visible = flag2;
            label.Visible = flag;
            if (!flag2 && !flag)
            {
                descriptionControl.Visible = false;
            }
            else
            {
                if (flag2 && (((image.Textures == null) || (image.Textures.Length == 0)) || (image.Textures[0] != dropPodThumbnail)))
                {
                    using (MyUtils.ReuseCollection<string>(ref this.m_preloadedTextures))
                    {
                        this.m_preloadedTextures.Add(dropPodThumbnail);
                        MyRenderProxy.PreloadTextures(this.m_preloadedTextures, VRageRender.Messages.TextureType.GUIWithoutPremultiplyAlpha);
                        image.SetTexture(dropPodThumbnail);
                    }
                }
                descriptionControl.Visible = true;
                label.Size = new Vector2(descriptionControl.Size.X, label.GetTextSize().Y * label.TextScale);
                float y = 0f;
                if (flag2)
                {
                    y += image.Size.Y;
                }
                if (flag)
                {
                    y += label.Size.Y + 0.02f;
                    if (!flag2)
                    {
                        y += 0.01f;
                    }
                }
                if ((text != null) && text.Visible)
                {
                    y += text.Size.Y + 0.02f;
                }
                descriptionControl.Size = new Vector2(descriptionControl.Size.X, y);
                image.PositionY = descriptionControl.Size.Y / 2f;
                label.PositionY = (-descriptionControl.Size.Y / 2f) + 0.01f;
                if (text != null)
                {
                    if (label.Visible)
                    {
                        text.PositionY = (label.PositionY + label.Size.Y) + 0.04f;
                    }
                    else
                    {
                        text.PositionY = (-descriptionControl.Size.Y / 2f) + 0.04f;
                    }
                }
            }
        }

        private void UpdateRespawnShipLabel()
        {
            if (this.m_selectedRespawnShip == null)
            {
                this.m_multilineRespawnWhenShipReady.Visible = false;
            }
            else
            {
                MySpaceRespawnComponent.Static.GetRespawnCooldownSeconds(MySession.Static.LocalHumanPlayer.Id, this.m_selectedRespawnShip.Id.SubtypeName);
                this.m_multilineRespawnWhenShipReady.Text.Clear().AppendFormat(MyTexts.GetString(MySpaceTexts.ScreenMedicals_RespawnWhenShipReady), this.m_selectedRespawnShip.DisplayNameText);
                this.m_multilineRespawnWhenShipReady.RefreshText(false);
                this.m_multilineRespawnWhenShipReady.Visible = true;
            }
        }

        private void UpdateSpawnShipTimes()
        {
            for (int i = 0; i < this.m_respawnsTable.RowsCount; i++)
            {
                MyGuiControlTable.Row row = this.m_respawnsTable.GetRow(i);
                MyRespawnShipDefinition userData = row.UserData as MyRespawnShipDefinition;
                if (userData != null)
                {
                    MyGuiControlTable.Cell cell = row.GetCell(1);
                    bool enabled = AddShipRespawnInfo(userData, cell.Text.Clear());
                    Color color = MyGuiControlBase.ApplyColorMaskModifiers((VRageMath.Vector4) Color.White, enabled, 1f);
                    row.GetCell(0).TextColor = new Color?(color);
                    cell.TextColor = new Color?(color);
                }
            }
        }

        public static MyGuiScreenMedicals Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }

        public static StringBuilder NoRespawnText
        {
            set
            {
                if (Static != null)
                {
                    Static.m_noRespawnText.Text = value;
                }
            }
        }

        public static int ItemsInTable
        {
            get
            {
                if ((Static == null) || (Static.m_respawnsTable == null))
                {
                    return 0;
                }
                return Static.m_respawnsTable.RowsCount;
            }
        }

        public bool IsBlackgroundVisible =>
            (this.m_blackgroundDrawFull || (this.m_blackgroundFade > 0f));

        public bool IsBlackgroundFading =>
            (!this.m_blackgroundDrawFull && (this.m_blackgroundFade > 0f));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenMedicals.<>c <>9 = new MyGuiScreenMedicals.<>c();
            public static Func<IMyEventOwner, Action> <>9__67_0;
            public static Func<MyPlanet, MyGuiScreenMedicals.MyPlanetInfo> <>9__75_0;
            public static Func<MyGuiScreenMedicals.MyPlanetInfo, bool> <>9__75_1;
            public static Func<IMyEventOwner, Action<List<MySpaceRespawnComponent.MyRespawnPointInfo>, MyGuiScreenMedicals.MyPlanetInfo[]>> <>9__75_2;

            internal Action <RefreshRespawnPoints>b__67_0(IMyEventOwner s) => 
                new Action(MyGuiScreenMedicals.RefreshRespawnPointsRequest);

            internal MyGuiScreenMedicals.MyPlanetInfo <RefreshRespawnPointsRequest>b__75_0(MyPlanet x)
            {
                MyGuiScreenMedicals.MyPlanetInfo info;
                using (ClearToken<MyRespawnShipDefinition> token = MySpaceRespawnComponent.GetRespawnShips(x))
                {
                    if (token.List.Count == 0)
                    {
                        info = null;
                    }
                    else
                    {
                        string helpTextLocalizationId = null;
                        string str2 = null;
                        if (token.List.Count == 1)
                        {
                            MyRespawnShipDefinition definition = token.List[0];
                            if ((definition.Icons != null) && (definition.Icons.Length != 0))
                            {
                                str2 = definition.Icons[0];
                            }
                            helpTextLocalizationId = definition.HelpTextLocalizationId;
                        }
                        float oxygenDensity = 0f;
                        if (x.HasAtmosphere)
                        {
                            MyPlanetAtmosphere atmosphere = x.Generator.Atmosphere;
                            if (atmosphere.Breathable)
                            {
                                oxygenDensity = atmosphere.OxygenDensity;
                            }
                        }
                        MyGuiScreenMedicals.MyPlanetInfo info1 = new MyGuiScreenMedicals.MyPlanetInfo();
                        info1.PlanetName = x.Name;
                        info1.PlanetId = x.EntityId;
                        info1.OxygenLevel = oxygenDensity;
                        info1.DropPodThumbnail = str2;
                        info1.WorldAABB = x.PositionComp.WorldAABB;
                        info1.Gravity = x.GetInitArguments.SurfaceGravity;
                        info1.HelpTextLocalizationId = helpTextLocalizationId;
                        info = info1;
                    }
                }
                return info;
            }

            internal bool <RefreshRespawnPointsRequest>b__75_1(MyGuiScreenMedicals.MyPlanetInfo x) => 
                (x != null);

            internal Action<List<MySpaceRespawnComponent.MyRespawnPointInfo>, MyGuiScreenMedicals.MyPlanetInfo[]> <RefreshRespawnPointsRequest>b__75_2(IMyEventOwner s) => 
                new Action<List<MySpaceRespawnComponent.MyRespawnPointInfo>, MyGuiScreenMedicals.MyPlanetInfo[]>(MyGuiScreenMedicals.RequestRespawnPointsResponse);
        }

        private class MyPlanetInfo
        {
            public long PlanetId;
            public string PlanetName;
            public BoundingBoxD WorldAABB;
            public float Gravity;
            public float OxygenLevel;
            [Nullable]
            public string DropPodThumbnail;
            [Nullable]
            public string HelpTextLocalizationId;
        }
    }
}

