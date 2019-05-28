namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Game;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.VoiceChat;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.GameServices;
    using VRage.Input;
    using VRage.Network;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    [StaticEventOwner]
    public class MyGuiScreenPlayers : MyGuiScreenBase
    {
        protected static readonly string OWNER_MARKER = "*****";
        protected int PlayerNameColumn;
        protected int PlayerFactionNameColumn;
        protected int PlayerFactionTagColumn;
        protected int PlayerMutedColumn;
        protected int GameAdminColumn;
        protected int GamePingColumn;
        private bool m_getPingAndRefresh;
        protected MyGuiControlButton m_inviteButton;
        protected MyGuiControlButton m_promoteButton;
        protected MyGuiControlButton m_demoteButton;
        protected MyGuiControlButton m_kickButton;
        protected MyGuiControlButton m_banButton;
        protected MyGuiControlLabel m_maxPlayersValueLabel;
        protected MyGuiControlTable m_playersTable;
        protected MyGuiControlCombobox m_lobbyTypeCombo;
        protected MyGuiControlSlider m_maxPlayersSlider;
        protected HashSet<ulong> m_mutedPlayers;
        protected Dictionary<ulong, short> pings;
        protected ulong m_lastSelected;
        private MyGuiControlLabel m_caption;

        public MyGuiScreenPlayers() : base(nullable2, new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, MyGuiConstants.TEXTURE_SCREEN_BACKGROUND.Texture, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.PlayerFactionNameColumn = 1;
            this.PlayerFactionTagColumn = 2;
            this.PlayerMutedColumn = 3;
            this.GameAdminColumn = 4;
            this.GamePingColumn = 5;
            this.m_getPingAndRefresh = true;
            this.pings = new Dictionary<ulong, short>();
            Vector2? nullable = new Vector2(0.837f, 0.813f);
            base.EnabledBackgroundFade = true;
            MyMultiplayer.Static.ClientJoined += new Action<ulong>(this.Multiplayer_PlayerJoined);
            MyMultiplayer.Static.ClientLeft += new Action<ulong, MyChatMemberStateChangeEnum>(this.Multiplayer_PlayerLeft);
            MySession.Static.Factions.FactionCreated += new Action<long>(this.OnFactionCreated);
            MySession.Static.Factions.FactionEdited += new Action<long>(this.OnFactionEdited);
            MySession.Static.Factions.FactionStateChanged += new Action<MyFactionStateChange, long, long, long, long>(this.OnFactionStateChanged);
            MySession.Static.OnUserPromoteLevelChanged += new Action<ulong, MyPromoteLevel>(this.OnUserPromoteLevelChanged);
            MyMultiplayerLobby @static = MyMultiplayer.Static as MyMultiplayerLobby;
            if (@static != null)
            {
                @static.OnLobbyDataUpdated += new MyLobbyDataUpdated(this.Matchmaking_LobbyDataUpdate);
            }
            MyMultiplayerLobbyClient client = MyMultiplayer.Static as MyMultiplayerLobbyClient;
            if (client != null)
            {
                client.OnLobbyDataUpdated += new MyLobbyDataUpdated(this.Matchmaking_LobbyDataUpdate);
            }
            if (MyPerGameSettings.EnableMutePlayer)
            {
                this.GameAdminColumn = 4;
            }
            this.m_mutedPlayers = MySandboxGame.Config.MutedPlayers;
            this.RecreateControls(true);
        }

        protected void AddPlayer(ulong userId)
        {
            string memberName = MyMultiplayer.Static.GetMemberName(userId);
            if (!string.IsNullOrEmpty(memberName))
            {
                MyGuiControlTable.Row row = new MyGuiControlTable.Row(userId);
                Color? textColor = null;
                MyGuiHighlightTexture? icon = null;
                row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(memberName), memberName, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                long playerId = Sync.Players.TryGetIdentityId(userId, 0);
                MyFaction playerFaction = MySession.Static.Factions.GetPlayerFaction(playerId);
                StringBuilder text = new StringBuilder();
                if (playerFaction != null)
                {
                    text.Append(MyStatControlText.SubstituteTexts(playerFaction.Name, null));
                    if (playerFaction.IsLeader(playerId))
                    {
                        text.Append(" (").Append(MyTexts.Get(MyCommonTexts.Leader)).Append(")");
                    }
                }
                textColor = null;
                icon = null;
                row.AddCell(new MyGuiControlTable.Cell(text, null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                textColor = null;
                icon = null;
                row.AddCell(new MyGuiControlTable.Cell(new StringBuilder((playerFaction != null) ? playerFaction.Tag : ""), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                textColor = null;
                icon = null;
                MyGuiControlTable.Cell cell = new MyGuiControlTable.Cell(new StringBuilder(""), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
                row.AddCell(cell);
                if (MyPerGameSettings.EnableMutePlayer && (userId != Sync.MyId))
                {
                    Vector2? position = null;
                    VRageMath.Vector4? color = null;
                    MyGuiControlCheckbox control = new MyGuiControlCheckbox(position, color, "", false, MyGuiControlCheckboxStyleEnum.Muted, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER) {
                        IsChecked = MySandboxGame.Config.MutedPlayers.Contains(userId)
                    };
                    control.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(control.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.IsMuteCheckedChanged));
                    control.UserData = userId;
                    cell.Control = control;
                    this.m_playersTable.Controls.Add(control);
                }
                StringBuilder builder2 = new StringBuilder();
                MyPromoteLevel userPromoteLevel = MySession.Static.GetUserPromoteLevel(userId);
                for (int i = 0; i < userPromoteLevel; i++)
                {
                    builder2.Append("*");
                }
                textColor = null;
                icon = null;
                row.AddCell(new MyGuiControlTable.Cell(builder2, null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                if (this.pings.ContainsKey(userId))
                {
                    textColor = null;
                    icon = null;
                    row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(this.pings[userId].ToString()), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                }
                else
                {
                    textColor = null;
                    icon = null;
                    row.AddCell(new MyGuiControlTable.Cell(new StringBuilder("----"), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                }
                this.m_playersTable.Add(row);
                this.UpdateCaption();
            }
        }

        protected void banButton_ButtonClicked(MyGuiControlButton obj)
        {
            MyGuiControlTable.Row selectedRow = this.m_playersTable.SelectedRow;
            if (selectedRow != null)
            {
                MyMultiplayer.Static.BanClient((ulong) selectedRow.UserData, true);
            }
        }

        private static void ClearPromoteNotificaions()
        {
            MyHud.Notifications.Remove(MyNotificationSingletons.PlayerDemotedNone);
            MyHud.Notifications.Remove(MyNotificationSingletons.PlayerDemotedScripter);
            MyHud.Notifications.Remove(MyNotificationSingletons.PlayerDemotedModerator);
            MyHud.Notifications.Remove(MyNotificationSingletons.PlayerDemotedSpaceMaster);
            MyHud.Notifications.Remove(MyNotificationSingletons.PlayerPromotedScripter);
            MyHud.Notifications.Remove(MyNotificationSingletons.PlayerPromotedModerator);
            MyHud.Notifications.Remove(MyNotificationSingletons.PlayerPromotedSpaceMaster);
            MyHud.Notifications.Remove(MyNotificationSingletons.PlayerPromotedAdmin);
        }

        protected void demoteButton_ButtonClicked(MyGuiControlButton obj)
        {
            MyGuiControlTable.Row selectedRow = this.m_playersTable.SelectedRow;
            if ((selectedRow != null) && MySession.Static.CanDemoteUser(Sync.MyId, (ulong) selectedRow.UserData))
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<ulong, bool>(x => new Action<ulong, bool>(MyGuiScreenPlayers.Promote), (ulong) selectedRow.UserData, false, targetEndpoint, position);
            }
        }

        public override bool Draw()
        {
            if (this.m_getPingAndRefresh)
            {
                this.m_getPingAndRefresh = false;
                RefreshPlusPings();
            }
            return base.Draw();
        }

        private int GameAdminCompare(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b)
        {
            ulong userData = (ulong) a.Row.UserData;
            ulong steamId = (ulong) b.Row.UserData;
            int userPromoteLevel = (int) MySession.Static.GetUserPromoteLevel(userData);
            return userPromoteLevel.CompareTo((int) MySession.Static.GetUserPromoteLevel(steamId));
        }

        private int GamePingCompare(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b)
        {
            int num;
            int num2;
            if (!int.TryParse(a.Text.ToString(), out num))
            {
                num = -1;
            }
            if (!int.TryParse(b.Text.ToString(), out num2))
            {
                num2 = -1;
            }
            return num.CompareTo(num2);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenPlayers";

        protected MyOnlineModeEnum GetOnlineMode(MyLobbyType lobbyType)
        {
            switch (lobbyType)
            {
                case MyLobbyType.Private:
                    return MyOnlineModeEnum.PRIVATE;

                case MyLobbyType.FriendsOnly:
                    return MyOnlineModeEnum.FRIENDS;

                case MyLobbyType.Public:
                    return MyOnlineModeEnum.PUBLIC;
            }
            return MyOnlineModeEnum.PUBLIC;
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if (MyInput.Static.IsNewKeyPressed(MyKeys.F3))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                this.CloseScreen();
            }
        }

        protected void inviteButton_ButtonClicked(MyGuiControlButton obj)
        {
            MyGameService.OpenInviteOverlay();
        }

        protected void IsMuteCheckedChanged(MyGuiControlCheckbox obj)
        {
            ulong userData = (ulong) obj.UserData;
            if (obj.IsChecked)
            {
                this.MutePlayer(userData);
            }
            else
            {
                this.UnmutePlayer(userData);
            }
        }

        protected void kickButton_ButtonClicked(MyGuiControlButton obj)
        {
            MyGuiControlTable.Row selectedRow = this.m_playersTable.SelectedRow;
            if (selectedRow != null)
            {
                MyMultiplayer.Static.KickClient((ulong) selectedRow.UserData, true, true);
            }
        }

        protected void lobbyTypeCombo_OnSelect()
        {
            MyLobbyType selectedKey = (MyLobbyType) ((int) this.m_lobbyTypeCombo.GetSelectedKey());
            this.m_lobbyTypeCombo.SelectItemByKey((long) MyMultiplayer.Static.GetLobbyType(), false);
            MyMultiplayer.Static.SetLobbyType(selectedKey);
        }

        protected void makeOwnerButton_ButtonClicked(MyGuiControlButton obj)
        {
            if (this.m_playersTable.SelectedRow != null)
            {
                MyMultiplayer.Static.GetOwner();
                ulong userData = (ulong) this.m_playersTable.SelectedRow.UserData;
                MyMultiplayer.Static.SetOwner(userData);
            }
        }

        protected void Matchmaking_LobbyDataUpdate(bool success, IMyLobby lobby, ulong memberOrLobby)
        {
            if (success)
            {
                ulong newOwnerId = lobby.OwnerId;
                MyGuiControlTable.Row row = this.m_playersTable.Find(row => row.GetCell(this.GameAdminColumn).Text.Length == OWNER_MARKER.Length);
                MyGuiControlTable.Row row2 = this.m_playersTable.Find(row => ((ulong) row.UserData) == newOwnerId);
                if (row != null)
                {
                    row.GetCell(this.GameAdminColumn).Text.Clear();
                }
                if (row2 != null)
                {
                    row2.GetCell(this.GameAdminColumn).Text.Clear().Append(OWNER_MARKER);
                }
                MyLobbyType lobbyType = lobby.LobbyType;
                this.m_lobbyTypeCombo.SelectItemByKey((long) lobbyType, false);
                MySession.Static.Settings.OnlineMode = this.GetOnlineMode(lobbyType);
                this.UpdateButtonsEnabledState();
                if (!Sync.IsServer)
                {
                    this.m_maxPlayersSlider.ValueChanged = null;
                    MySession.Static.Settings.MaxPlayers = (short) MyMultiplayer.Static.MemberLimit;
                    this.m_maxPlayersSlider.Value = MySession.Static.MaxPlayers;
                    this.m_maxPlayersSlider.ValueChanged = new Action<MyGuiControlSlider>(this.MaxPlayersSlider_Changed);
                    this.m_maxPlayersValueLabel.Text = this.m_maxPlayersSlider.Value.ToString();
                    this.UpdateCaption();
                }
            }
        }

        protected void MaxPlayersSlider_Changed(MyGuiControlSlider control)
        {
            MySession.Static.Settings.MaxPlayers = (short) this.m_maxPlayersSlider.Value;
            MyMultiplayer.Static.SetMemberLimit(MySession.Static.MaxPlayers);
            this.m_maxPlayersValueLabel.Text = this.m_maxPlayersSlider.Value.ToString();
            this.UpdateCaption();
        }

        protected void Multiplayer_PlayerJoined(ulong userId)
        {
            this.AddPlayer(userId);
        }

        protected void Multiplayer_PlayerLeft(ulong userId, MyChatMemberStateChangeEnum arg2)
        {
            this.RemovePlayer(userId);
        }

        protected void MutePlayer(ulong mutedUserId)
        {
            this.m_mutedPlayers.Add(mutedUserId);
            MySandboxGame.Config.MutedPlayers = this.m_mutedPlayers;
            MySandboxGame.Config.Save();
            MyVoiceChatSessionComponent.MutePlayerRequest(mutedUserId, true);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (MyMultiplayer.Static != null)
            {
                MyMultiplayer.Static.ClientJoined -= new Action<ulong>(this.Multiplayer_PlayerJoined);
                MyMultiplayer.Static.ClientLeft -= new Action<ulong, MyChatMemberStateChangeEnum>(this.Multiplayer_PlayerLeft);
            }
            if (MySession.Static != null)
            {
                MySession.Static.Factions.FactionCreated -= new Action<long>(this.OnFactionCreated);
                MySession.Static.Factions.FactionEdited -= new Action<long>(this.OnFactionEdited);
                MySession.Static.Factions.FactionStateChanged -= new Action<MyFactionStateChange, long, long, long, long>(this.OnFactionStateChanged);
                MySession.Static.OnUserPromoteLevelChanged -= new Action<ulong, MyPromoteLevel>(this.OnUserPromoteLevelChanged);
            }
            MyMultiplayerLobby @static = MyMultiplayer.Static as MyMultiplayerLobby;
            if (@static != null)
            {
                @static.OnLobbyDataUpdated -= new MyLobbyDataUpdated(this.Matchmaking_LobbyDataUpdate);
            }
            MyMultiplayerLobbyClient client = MyMultiplayer.Static as MyMultiplayerLobbyClient;
            if (client != null)
            {
                client.OnLobbyDataUpdated -= new MyLobbyDataUpdated(this.Matchmaking_LobbyDataUpdate);
            }
        }

        private void OnFactionCreated(long insertedId)
        {
            RefreshPlusPings();
        }

        private void OnFactionEdited(long editedId)
        {
            RefreshPlusPings();
        }

        private void OnFactionStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
        {
            RefreshPlusPings();
        }

        private void OnUserPromoteLevelChanged(ulong steamId, MyPromoteLevel promotionLevel)
        {
            RefreshPlusPings();
        }

        protected void playersTable_ItemSelected(MyGuiControlTable table, MyGuiControlTable.EventArgs args)
        {
            this.UpdateButtonsEnabledState();
            if (this.m_playersTable.SelectedRow != null)
            {
                this.m_lastSelected = (ulong) this.m_playersTable.SelectedRow.UserData;
            }
        }

        [Event(null, 0x263), Reliable, Server]
        public static void Promote(ulong playerId, bool promote)
        {
            if (MyEventContext.Current.IsLocallyInvoked || ((!promote || MySession.Static.CanPromoteUser(MyEventContext.Current.Sender.Value, playerId)) && (promote || MySession.Static.CanDemoteUser(MyEventContext.Current.Sender.Value, playerId))))
            {
                PromoteImplementation(playerId, promote);
            }
            else
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, false, null, true);
            }
        }

        protected void promoteButton_ButtonClicked(MyGuiControlButton obj)
        {
            MyGuiControlTable.Row selectedRow = this.m_playersTable.SelectedRow;
            if ((selectedRow != null) && MySession.Static.CanPromoteUser(Sync.MyId, (ulong) selectedRow.UserData))
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<ulong, bool>(x => new Action<ulong, bool>(MyGuiScreenPlayers.Promote), (ulong) selectedRow.UserData, true, targetEndpoint, position);
            }
        }

        public static void PromoteImplementation(ulong playerId, bool promote)
        {
            MyPromoteLevel userPromoteLevel = MySession.Static.GetUserPromoteLevel(playerId);
            if (promote)
            {
                if (userPromoteLevel >= MyPromoteLevel.Admin)
                {
                    return;
                }
                userPromoteLevel += 1;
                if (!MySession.Static.EnableScripterRole && (userPromoteLevel == MyPromoteLevel.Scripter))
                {
                    userPromoteLevel += 1;
                }
            }
            else
            {
                if (userPromoteLevel == MyPromoteLevel.None)
                {
                    return;
                }
                userPromoteLevel -= 1;
                if (!MySession.Static.EnableScripterRole && (userPromoteLevel == MyPromoteLevel.Scripter))
                {
                    userPromoteLevel -= 1;
                }
            }
            MySession.Static.SetUserPromoteLevel(playerId, userPromoteLevel);
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<MyPromoteLevel, bool>(x => new Action<MyPromoteLevel, bool>(MyGuiScreenPlayers.ShowPromoteMessage), userPromoteLevel, promote, new EndpointId(playerId), position);
            RefreshPlusPings();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.CloseButtonEnabled = true;
            Vector2 vector = base.Size.Value / MyGuiConstants.TEXTURE_SCREEN_BACKGROUND.SizeGui;
            Vector2? size = base.Size;
            float single1 = (-0.5f * size.Value) + ((vector * MyGuiConstants.TEXTURE_SCREEN_BACKGROUND.PaddingSizeGui) * 1.1f);
            VRageMath.Vector4? captionTextColor = null;
            this.m_caption = base.AddCaption(MyCommonTexts.ScreenCaptionPlayers, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(-0.364f, -0.331f), 0.728f, 0f, captionTextColor);
            captionTextColor = null;
            control.AddHorizontal(new Vector2(-0.364f, 0.358f), 0.728f, 0f, captionTextColor);
            captionTextColor = null;
            control.AddHorizontal(new Vector2(-0.36f, -0.006f), 0.17f, 0f, captionTextColor);
            this.Controls.Add(control);
            size = null;
            captionTextColor = null;
            int? buttonIndex = null;
            this.m_inviteButton = new MyGuiControlButton(new Vector2(-0.361f, -0.304f), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, MyTexts.Get(MyCommonTexts.ScreenPlayers_Invite), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_inviteButton.ButtonClicked += new Action<MyGuiControlButton>(this.inviteButton_ButtonClicked);
            this.Controls.Add(this.m_inviteButton);
            size = null;
            captionTextColor = null;
            buttonIndex = null;
            this.m_promoteButton = new MyGuiControlButton(new Vector2(-0.361f, -0.247f), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, MyTexts.Get(MyCommonTexts.ScreenPlayers_Promote), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_promoteButton.ButtonClicked += new Action<MyGuiControlButton>(this.promoteButton_ButtonClicked);
            this.Controls.Add(this.m_promoteButton);
            size = null;
            captionTextColor = null;
            buttonIndex = null;
            this.m_demoteButton = new MyGuiControlButton(new Vector2(-0.361f, -0.191f), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, MyTexts.Get(MyCommonTexts.ScreenPlayers_Demote), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_demoteButton.ButtonClicked += new Action<MyGuiControlButton>(this.demoteButton_ButtonClicked);
            this.Controls.Add(this.m_demoteButton);
            size = null;
            captionTextColor = null;
            buttonIndex = null;
            this.m_kickButton = new MyGuiControlButton(new Vector2(-0.361f, -0.135f), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, MyTexts.Get(MyCommonTexts.ScreenPlayers_Kick), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_kickButton.ButtonClicked += new Action<MyGuiControlButton>(this.kickButton_ButtonClicked);
            this.Controls.Add(this.m_kickButton);
            size = null;
            captionTextColor = null;
            buttonIndex = null;
            this.m_banButton = new MyGuiControlButton(new Vector2(-0.361f, -0.079f), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, MyTexts.Get(MyCommonTexts.ScreenPlayers_Ban), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_banButton.ButtonClicked += new Action<MyGuiControlButton>(this.banButton_ButtonClicked);
            this.Controls.Add(this.m_banButton);
            size = null;
            captionTextColor = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2(-0.362f, 0.013f), size, "Lobby Type:", captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            if ((MyMultiplayer.Static != null) && (MyMultiplayer.Static.LobbyId != 0))
            {
                this.Controls.Add(label);
            }
            size = null;
            captionTextColor = null;
            size = null;
            size = null;
            captionTextColor = null;
            this.m_lobbyTypeCombo = new MyGuiControlCombobox(new Vector2(-0.363f, 0.044f), size, captionTextColor, size, 3, size, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
            this.m_lobbyTypeCombo.Size = new Vector2(0.175f, 0.04f);
            this.m_lobbyTypeCombo.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            buttonIndex = null;
            MyStringId? toolTip = null;
            this.m_lobbyTypeCombo.AddItem(0L, MyCommonTexts.ScreenPlayersLobby_Private, buttonIndex, toolTip);
            buttonIndex = null;
            toolTip = null;
            this.m_lobbyTypeCombo.AddItem(1L, MyCommonTexts.ScreenPlayersLobby_Friends, buttonIndex, toolTip);
            buttonIndex = null;
            toolTip = null;
            this.m_lobbyTypeCombo.AddItem(2L, MyCommonTexts.ScreenPlayersLobby_Public, buttonIndex, toolTip);
            this.m_lobbyTypeCombo.SelectItemByKey((long) MyMultiplayer.Static.GetLobbyType(), true);
            if ((MyMultiplayer.Static != null) && (MyMultiplayer.Static.LobbyId != 0))
            {
                this.Controls.Add(this.m_lobbyTypeCombo);
            }
            size = null;
            captionTextColor = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2(-0.361f, 0.103f), size, MyTexts.GetString(MyCommonTexts.MaxPlayers) + ":", captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            if ((MyMultiplayer.Static != null) && (MyMultiplayer.Static.LobbyId != 0))
            {
                this.Controls.Add(label2);
            }
            size = null;
            captionTextColor = null;
            this.m_maxPlayersValueLabel = new MyGuiControlLabel(new Vector2(-0.192f, 0.103f), size, null, captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            if ((MyMultiplayer.Static != null) && (MyMultiplayer.Static.LobbyId != 0))
            {
                this.Controls.Add(this.m_maxPlayersValueLabel);
            }
            captionTextColor = null;
            this.m_maxPlayersSlider = new MyGuiControlSlider(new Vector2(-0.364f, 0.133f), 2f, (float) MyMultiplayerLobby.MAX_PLAYERS, 0.177f, new float?(Sync.IsServer ? ((float) MySession.Static.MaxPlayers) : ((float) MyMultiplayer.Static.MemberLimit)), captionTextColor, null, 1, 0.8f, 0f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, true, false);
            this.m_maxPlayersValueLabel.Text = this.m_maxPlayersSlider.Value.ToString();
            this.m_maxPlayersSlider.ValueChanged = new Action<MyGuiControlSlider>(this.MaxPlayersSlider_Changed);
            if ((MyMultiplayer.Static != null) && (MyMultiplayer.Static.LobbyId != 0))
            {
                this.Controls.Add(this.m_maxPlayersSlider);
            }
            MyGuiControlTable table1 = new MyGuiControlTable();
            table1.Position = new Vector2(0.364f, -0.307f);
            table1.Size = new Vector2(0.54f, 0.813f);
            table1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            table1.ColumnsCount = 6;
            this.m_playersTable = table1;
            this.m_playersTable.VisibleRowsCount = 0x12;
            float num = 0.3f;
            float num2 = 0.12f;
            float num3 = 0.12f;
            float num4 = 0.12f;
            float num5 = MyPerGameSettings.EnableMutePlayer ? 0.13f : 0f;
            float[] p = new float[] { num, ((((1f - num) - num2) - num3) - num5) - num4, num2, num5, num3, num4 };
            this.m_playersTable.SetCustomColumnWidths(p);
            this.m_playersTable.SetColumnComparison(this.PlayerNameColumn, (a, b) => a.Text.CompareToIgnoreCase(b.Text));
            this.m_playersTable.SetColumnName(this.PlayerNameColumn, MyTexts.Get(MyCommonTexts.ScreenPlayers_PlayerName));
            this.m_playersTable.SetColumnComparison(this.PlayerFactionNameColumn, (a, b) => a.Text.CompareToIgnoreCase(b.Text));
            this.m_playersTable.SetColumnName(this.PlayerFactionNameColumn, MyTexts.Get(MyCommonTexts.ScreenPlayers_FactionName));
            this.m_playersTable.SetColumnComparison(this.PlayerFactionTagColumn, (a, b) => a.Text.CompareToIgnoreCase(b.Text));
            this.m_playersTable.SetColumnName(this.PlayerFactionTagColumn, MyTexts.Get(MyCommonTexts.ScreenPlayers_FactionTag));
            if (MyPerGameSettings.EnableMutePlayer)
            {
                this.m_playersTable.SetColumnName(this.PlayerMutedColumn, new StringBuilder(MyTexts.GetString(MyCommonTexts.ScreenPlayers_Muted)));
            }
            this.m_playersTable.SetColumnComparison(this.GameAdminColumn, new Comparison<MyGuiControlTable.Cell>(this.GameAdminCompare));
            this.m_playersTable.SetColumnName(this.GameAdminColumn, MyTexts.Get(MyCommonTexts.ScreenPlayers_Rank));
            this.m_playersTable.SetColumnComparison(this.GamePingColumn, new Comparison<MyGuiControlTable.Cell>(this.GamePingCompare));
            this.m_playersTable.SetColumnName(this.GamePingColumn, MyTexts.Get(MyCommonTexts.ScreenPlayers_Ping));
            this.m_playersTable.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.playersTable_ItemSelected);
            this.Controls.Add(this.m_playersTable);
            foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
            {
                if (player.Id.SerialId == 0)
                {
                    int index = 0;
                    while (true)
                    {
                        if (index >= this.m_playersTable.RowsCount)
                        {
                            this.AddPlayer(player.Id.SteamId);
                            break;
                        }
                        MyGuiControlTable.Row row = this.m_playersTable.GetRow(index);
                        if (row.UserData is ulong)
                        {
                            ulong steamId = player.Id.SteamId;
                            ulong userData = (ulong) row.UserData;
                        }
                        index++;
                    }
                }
            }
            this.m_lobbyTypeCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.lobbyTypeCombo_OnSelect);
            if (this.m_lastSelected != 0)
            {
                MyGuiControlTable.Row row2 = this.m_playersTable.Find(r => ((ulong) r.UserData) == this.m_lastSelected);
                if (row2 != null)
                {
                    this.m_playersTable.SelectedRow = row2;
                }
            }
            this.UpdateButtonsEnabledState();
            this.UpdateCaption();
        }

        protected static void Refresh()
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyGuiScreenPlayers firstScreenOfType = MyScreenManager.GetFirstScreenOfType<MyGuiScreenPlayers>();
                if (firstScreenOfType != null)
                {
                    firstScreenOfType.RecreateControls(false);
                }
            }
        }

        public static void RefreshPlusPings()
        {
            if (!Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent(s => new Action(MyGuiScreenPlayers.RequestPingsAndRefresh), targetEndpoint, position);
            }
            else if (!Sandbox.Engine.Platform.Game.IsDedicated && (MySession.Static != null))
            {
                MyReplicationServer replicationLayer = MyMultiplayer.Static.ReplicationLayer as MyReplicationServer;
                if (replicationLayer != null)
                {
                    SerializableDictionary<ulong, short> dictionary;
                    replicationLayer.GetClientPings(out dictionary);
                    SendPingsAndRefresh(dictionary);
                }
            }
        }

        protected void RemovePlayer(ulong userId)
        {
            this.m_playersTable.Remove(row => ((ulong) row.UserData) == userId);
            this.UpdateButtonsEnabledState();
            if (MySession.Static != null)
            {
                this.UpdateCaption();
            }
        }

        [Event(null, 0x308), Reliable, Server]
        public static void RequestPingsAndRefresh()
        {
            if (Sync.IsServer && (MySession.Static != null))
            {
                MyReplicationServer replicationLayer = MyMultiplayer.Static.ReplicationLayer as MyReplicationServer;
                if (replicationLayer != null)
                {
                    SerializableDictionary<ulong, short> dictionary;
                    replicationLayer.GetClientPings(out dictionary);
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<SerializableDictionary<ulong, short>>(s => new Action<SerializableDictionary<ulong, short>>(MyGuiScreenPlayers.SendPingsAndRefresh), dictionary, new EndpointId(MyEventContext.Current.Sender.Value), position);
                }
            }
        }

        [Event(null, 0x319), Reliable, Client]
        private static void SendPingsAndRefresh(SerializableDictionary<ulong, short> dictionary)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyGuiScreenPlayers firstScreenOfType = MyScreenManager.GetFirstScreenOfType<MyGuiScreenPlayers>();
                if (firstScreenOfType != null)
                {
                    firstScreenOfType.pings.Clear();
                    foreach (KeyValuePair<ulong, short> pair in dictionary.Dictionary)
                    {
                        firstScreenOfType.pings[pair.Key] = pair.Value;
                    }
                    firstScreenOfType.RecreateControls(false);
                }
            }
        }

        [Event(null, 0x28b), Reliable, Client]
        protected static void ShowPromoteMessage(MyPromoteLevel promoteLevel, bool promote)
        {
            ClearPromoteNotificaions();
            switch (promoteLevel)
            {
                case MyPromoteLevel.None:
                    MyHud.Notifications.Add(MyNotificationSingletons.PlayerDemotedNone);
                    return;

                case MyPromoteLevel.Scripter:
                    MyHud.Notifications.Add(promote ? MyNotificationSingletons.PlayerPromotedScripter : MyNotificationSingletons.PlayerDemotedScripter);
                    return;

                case MyPromoteLevel.Moderator:
                    MyHud.Notifications.Add(promote ? MyNotificationSingletons.PlayerPromotedModerator : MyNotificationSingletons.PlayerDemotedModerator);
                    return;

                case MyPromoteLevel.SpaceMaster:
                    MyHud.Notifications.Add(promote ? MyNotificationSingletons.PlayerPromotedSpaceMaster : MyNotificationSingletons.PlayerDemotedSpaceMaster);
                    return;

                case MyPromoteLevel.Admin:
                    MyHud.Notifications.Add(MyNotificationSingletons.PlayerPromotedAdmin);
                    return;

                case MyPromoteLevel.Owner:
                    return;
            }
            throw new ArgumentOutOfRangeException("promoteLevel", promoteLevel, null);
        }

        protected void UnmutePlayer(ulong mutedUserId)
        {
            this.m_mutedPlayers.Remove(mutedUserId);
            MySandboxGame.Config.MutedPlayers = this.m_mutedPlayers;
            MySandboxGame.Config.Save();
            MyVoiceChatSessionComponent.MutePlayerRequest(mutedUserId, false);
        }

        protected void UpdateButtonsEnabledState()
        {
            if (MyMultiplayer.Static != null)
            {
                ulong userId = MyGameService.UserId;
                ulong owner = MyMultiplayer.Static.GetOwner();
                bool flag1 = this.m_playersTable.SelectedRow != null;
                ulong target = flag1 ? ((ulong) this.m_playersTable.SelectedRow.UserData) : ((ulong) 0L);
                bool flag = userId == target;
                bool flag2 = MySession.Static.IsUserAdmin(userId);
                bool flag3 = userId == owner;
                bool local1 = flag1;
                bool flag4 = local1 && MySession.Static.CanPromoteUser(Sync.MyId, target);
                bool local2 = local1;
                bool flag5 = local2 && MySession.Static.CanDemoteUser(Sync.MyId, target);
                MyLobbyType selectedKey = (MyLobbyType) ((int) this.m_lobbyTypeCombo.GetSelectedKey());
                if (!local2 || flag)
                {
                    this.m_promoteButton.Enabled = false;
                    this.m_demoteButton.Enabled = false;
                    this.m_kickButton.Enabled = false;
                    this.m_banButton.Enabled = false;
                }
                else
                {
                    this.m_promoteButton.Enabled = flag4;
                    this.m_demoteButton.Enabled = flag5;
                    this.m_kickButton.Enabled = flag4 & flag2;
                    this.m_banButton.Enabled = flag4 & flag2;
                }
                this.m_banButton.Enabled &= MyMultiplayer.Static is MyMultiplayerClient;
                this.m_inviteButton.Enabled = !MyMultiplayer.Static.IsServer ? (selectedKey == MyLobbyType.Public) : ((selectedKey == MyLobbyType.Public) || (selectedKey == MyLobbyType.FriendsOnly));
                this.m_lobbyTypeCombo.Enabled = flag3;
                this.m_maxPlayersSlider.Enabled = flag3;
                this.m_lobbyTypeCombo.Enabled = flag3;
                this.m_maxPlayersSlider.Enabled = flag3;
            }
        }

        private void UpdateCaption()
        {
            string name = string.Empty;
            MyMultiplayerClient @static = MyMultiplayer.Static as MyMultiplayerClient;
            if (@static != null)
            {
                if (@static.Server != null)
                {
                    name = @static.Server.Name;
                }
            }
            else
            {
                MyMultiplayerLobbyClient client2 = MyMultiplayer.Static as MyMultiplayerLobbyClient;
                if (client2 != null)
                {
                    name = client2.HostName;
                }
            }
            if (string.IsNullOrEmpty(name))
            {
                object[] objArray1 = new object[] { MyTexts.Get(MyCommonTexts.ScreenCaptionPlayers), " (", this.m_playersTable.RowsCount, " / ", MySession.Static.MaxPlayers, ")" };
                this.m_caption.Text = string.Concat(objArray1);
            }
            else
            {
                object[] objArray2 = new object[9];
                objArray2[0] = MyTexts.Get(MyCommonTexts.ScreenCaptionServerName);
                objArray2[1] = name;
                objArray2[2] = "  -  ";
                objArray2[3] = MyTexts.Get(MyCommonTexts.ScreenCaptionPlayers);
                objArray2[4] = " (";
                objArray2[5] = this.m_playersTable.RowsCount;
                objArray2[6] = " / ";
                objArray2[7] = MySession.Static.MaxPlayers;
                objArray2[8] = ")";
                this.m_caption.Text = string.Concat(objArray2);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenPlayers.<>c <>9 = new MyGuiScreenPlayers.<>c();
            public static Comparison<MyGuiControlTable.Cell> <>9__24_0;
            public static Comparison<MyGuiControlTable.Cell> <>9__24_1;
            public static Comparison<MyGuiControlTable.Cell> <>9__24_2;
            public static Func<IMyEventOwner, Action<ulong, bool>> <>9__42_0;
            public static Func<IMyEventOwner, Action<ulong, bool>> <>9__43_0;
            public static Func<IMyEventOwner, Action<MyPromoteLevel, bool>> <>9__45_0;
            public static Func<IMyEventOwner, Action> <>9__57_0;
            public static Func<IMyEventOwner, Action<SerializableDictionary<ulong, short>>> <>9__58_0;

            internal Action<ulong, bool> <demoteButton_ButtonClicked>b__43_0(IMyEventOwner x) => 
                new Action<ulong, bool>(MyGuiScreenPlayers.Promote);

            internal Action<ulong, bool> <promoteButton_ButtonClicked>b__42_0(IMyEventOwner x) => 
                new Action<ulong, bool>(MyGuiScreenPlayers.Promote);

            internal Action<MyPromoteLevel, bool> <PromoteImplementation>b__45_0(IMyEventOwner x) => 
                new Action<MyPromoteLevel, bool>(MyGuiScreenPlayers.ShowPromoteMessage);

            internal int <RecreateControls>b__24_0(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => 
                a.Text.CompareToIgnoreCase(b.Text);

            internal int <RecreateControls>b__24_1(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => 
                a.Text.CompareToIgnoreCase(b.Text);

            internal int <RecreateControls>b__24_2(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => 
                a.Text.CompareToIgnoreCase(b.Text);

            internal Action <RefreshPlusPings>b__57_0(IMyEventOwner s) => 
                new Action(MyGuiScreenPlayers.RequestPingsAndRefresh);

            internal Action<SerializableDictionary<ulong, short>> <RequestPingsAndRefresh>b__58_0(IMyEventOwner s) => 
                new Action<SerializableDictionary<ulong, short>>(MyGuiScreenPlayers.SendPingsAndRefresh);
        }
    }
}

