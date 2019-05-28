namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Graphics.GUI;
    using Sandbox.Gui;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Gui;
    using VRage.GameServices;
    using VRage.Input;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;
    using VRage.Steam;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenJoinGame : MyGuiScreenBase
    {
        private MyGuiControlTabControl m_joinGameTabs;
        private MyGuiControlContextMenu m_contextMenu;
        private readonly StringBuilder m_textCache;
        private readonly StringBuilder m_gameTypeText;
        private readonly StringBuilder m_gameTypeToolTip;
        private MyGuiControlTable m_gamesTable;
        private MyGuiControlButton m_joinButton;
        private MyGuiControlButton m_refreshButton;
        private MyGuiControlButton m_detailsButton;
        private MyGuiControlButton m_directConnectButton;
        private MyGuiControlSearchBox m_searchBox;
        private MyGuiControlButton m_advancedSearchButton;
        private MyGuiControlRotatingWheel m_loadingWheel;
        private readonly string m_dataHash;
        private bool m_searchChanged;
        private DateTime m_searchLastChanged;
        private Action m_searchChangedFunc;
        private MyRankedServers m_rankedServers;
        public MyGuiControlTabPage m_selectedPage;
        private int m_remainingTimeUpdateFrame;
        public MyServerFilterOptions FilterOptions;
        public bool EnableAdvancedSearch;
        public bool refresh_favorites;
        [CompilerGenerated]
        private Action<MyGuiControlButton> RefreshRequest;
        private MyGuiControlTabPage m_serversPage;
        private readonly HashSet<MyCachedServerItem> m_dedicatedServers;
        private RefreshStateEnum m_nextState;
        private bool m_refreshPaused;
        private bool m_dedicatedResponding;
        private bool m_lastVersionCheck;
        private readonly List<IMyLobby> m_lobbies;
        private MyGuiControlTabPage m_lobbyPage;
        private MyGuiControlTabPage m_favoritesPage;
        private HashSet<MyCachedServerItem> m_favoriteServers;
        private bool m_favoritesResponding;
        private MyGuiControlTabPage m_historyPage;
        private HashSet<MyCachedServerItem> m_historyServers;
        private bool m_historyResponding;
        private MyGuiControlTabPage m_LANPage;
        private HashSet<MyCachedServerItem> m_lanServers;
        private bool m_lanResponding;
        private MyGuiControlTabPage m_friendsPage;
        private HashSet<ulong> m_friendIds;
        private HashSet<string> m_friendNames;

        private event Action<MyGuiControlButton> RefreshRequest
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlButton> refreshRequest = this.RefreshRequest;
                while (true)
                {
                    Action<MyGuiControlButton> a = refreshRequest;
                    Action<MyGuiControlButton> action3 = (Action<MyGuiControlButton>) Delegate.Combine(a, value);
                    refreshRequest = Interlocked.CompareExchange<Action<MyGuiControlButton>>(ref this.RefreshRequest, action3, a);
                    if (ReferenceEquals(refreshRequest, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlButton> refreshRequest = this.RefreshRequest;
                while (true)
                {
                    Action<MyGuiControlButton> source = refreshRequest;
                    Action<MyGuiControlButton> action3 = (Action<MyGuiControlButton>) Delegate.Remove(source, value);
                    refreshRequest = Interlocked.CompareExchange<Action<MyGuiControlButton>>(ref this.RefreshRequest, action3, source);
                    if (ReferenceEquals(refreshRequest, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenJoinGame() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(1f, 0.9f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_textCache = new StringBuilder();
            this.m_gameTypeText = new StringBuilder();
            this.m_gameTypeToolTip = new StringBuilder();
            this.m_searchLastChanged = DateTime.Now;
            this.m_dedicatedServers = new HashSet<MyCachedServerItem>();
            this.m_lobbies = new List<IMyLobby>();
            this.m_favoriteServers = new HashSet<MyCachedServerItem>();
            this.m_historyServers = new HashSet<MyCachedServerItem>();
            this.m_lanServers = new HashSet<MyCachedServerItem>();
            base.EnabledBackgroundFade = true;
            this.m_dataHash = MyDataIntegrityChecker.GetHashBase64();
            MyObjectBuilder_ServerFilterOptions serverSearchSettings = MySandboxGame.Config.ServerSearchSettings;
            this.FilterOptions = (serverSearchSettings == null) ? new MySpaceServerFilterOptions() : new MySpaceServerFilterOptions(serverSearchSettings);
            this.RecreateControls(true);
            this.m_selectedPage = (MyGuiControlTabPage) this.m_joinGameTabs.Controls.GetControlByName("PageFavoritesPanel");
            this.joinGameTabs_OnPageChanged();
            MyRankedServers.LoadAsync(MyPerGameSettings.RankedServersUrl, new Action<MyRankedServers>(this.OnRankedServersLoaded));
        }

        private void AddHeaders()
        {
            int colIdx = 0;
            colIdx++;
            this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_World));
            colIdx++;
            this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_GameMode));
            colIdx++;
            this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_Username));
            if (MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME)
            {
                colIdx++;
                this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_RemainingTime));
            }
            colIdx++;
            this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_Players));
            colIdx++;
            this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_Mods));
        }

        private void AddLobby(IMyLobby lobby)
        {
            if (!this.FilterOptions.AdvancedFilter || this.FilterOptions.FilterLobby(lobby))
            {
                string lobbyWorldName = MyMultiplayerLobby.GetLobbyWorldName(lobby);
                MyMultiplayerLobby.GetLobbyWorldSize(lobby);
                int lobbyAppVersion = MyMultiplayerLobby.GetLobbyAppVersion(lobby);
                int lobbyModCount = MyMultiplayerLobby.GetLobbyModCount(lobby);
                string str2 = this.m_searchBox.SearchText.Trim();
                if (string.IsNullOrWhiteSpace(str2) || lobbyWorldName.ToLower().Contains(str2.ToLower()))
                {
                    this.m_gameTypeText.Clear();
                    this.m_gameTypeToolTip.Clear();
                    float num3 = MyMultiplayerLobby.GetLobbyFloat("blocksInventoryMultiplier", lobby, 1f);
                    float num4 = MyMultiplayerLobby.GetLobbyFloat("inventoryMultiplier", lobby, 1f);
                    float num5 = MyMultiplayerLobby.GetLobbyFloat("refineryMultiplier", lobby, 1f);
                    float num6 = MyMultiplayerLobby.GetLobbyFloat("assemblerMultiplier", lobby, 1f);
                    MyGameModeEnum lobbyGameMode = MyMultiplayerLobby.GetLobbyGameMode(lobby);
                    if (MyMultiplayerLobby.GetLobbyScenario(lobby))
                    {
                        this.m_gameTypeText.AppendStringBuilder(MyTexts.Get(MySpaceTexts.WorldSettings_GameScenario));
                        DateTime time = MyMultiplayerLobby.GetLobbyDateTime("scenarioStartTime", lobby, DateTime.MinValue);
                        if (time <= DateTime.MinValue)
                        {
                            this.m_gameTypeText.Append(" Lobby");
                        }
                        else
                        {
                            TimeSpan span = (TimeSpan) (DateTime.UtcNow - time);
                            double num10 = Math.Truncate(span.TotalHours);
                            int num11 = (int) ((span.TotalHours - num10) * 60.0);
                            this.m_gameTypeText.Append(" ").Append(num10).Append(":").Append(num11.ToString("D2"));
                        }
                    }
                    else if (lobbyGameMode == MyGameModeEnum.Creative)
                    {
                        if (!this.FilterOptions.CreativeMode)
                        {
                            return;
                        }
                        this.m_gameTypeText.AppendStringBuilder(MyTexts.Get(MyCommonTexts.WorldSettings_GameModeCreative));
                    }
                    else if (lobbyGameMode == MyGameModeEnum.Survival)
                    {
                        if (!this.FilterOptions.SurvivalMode)
                        {
                            return;
                        }
                        this.m_gameTypeText.AppendStringBuilder(MyTexts.Get(MyCommonTexts.WorldSettings_GameModeSurvival));
                        this.m_gameTypeText.Append($" {num4}-{num3}-{num6}-{num5}");
                    }
                    object[] args = new object[] { num4, num3, num6, num5 };
                    this.m_gameTypeToolTip.AppendFormat(MyTexts.Get(MyCommonTexts.JoinGame_GameTypeToolTip_MultipliersFormat).ToString(), args);
                    int lobbyViewDistance = MyMultiplayerLobby.GetLobbyViewDistance(lobby);
                    this.m_gameTypeToolTip.AppendLine();
                    this.m_gameTypeToolTip.AppendFormat(MyTexts.Get(MyCommonTexts.JoinGame_GameTypeToolTip_ViewDistance).ToString(), lobbyViewDistance);
                    if ((!string.IsNullOrEmpty(lobbyWorldName) && (!this.FilterOptions.SameVersion || (lobbyAppVersion == MyFinalBuildConstants.APP_VERSION))) && ((!this.FilterOptions.SameData || !MyFakes.ENABLE_MP_DATA_HASHES) || MyMultiplayerLobby.HasSameData(lobby)))
                    {
                        string lobbyHostName = MyMultiplayerLobby.GetLobbyHostName(lobby);
                        string str4 = lobby.MemberLimit.ToString();
                        string str5 = lobby.MemberCount + "/" + str4;
                        if (((!this.FilterOptions.CheckDistance || this.FilterOptions.ViewDistance.ValueBetween((float) MyMultiplayerLobby.GetLobbyViewDistance(lobby))) && (!this.FilterOptions.CheckPlayer || this.FilterOptions.PlayerCount.ValueBetween((float) lobby.MemberCount))) && (!this.FilterOptions.CheckMod || this.FilterOptions.ModCount.ValueBetween((float) lobbyModCount)))
                        {
                            List<MyObjectBuilder_Checkpoint.ModItem> lobbyMods = MyMultiplayerLobby.GetLobbyMods(lobby);
                            if (((this.FilterOptions.Mods != null) && this.FilterOptions.Mods.Any<ulong>()) && this.FilterOptions.AdvancedFilter)
                            {
                                if (!this.FilterOptions.ModsExclusive)
                                {
                                    if ((lobbyMods == null) || !lobbyMods.Any<MyObjectBuilder_Checkpoint.ModItem>(m => this.FilterOptions.Mods.Contains(m.PublishedFileId)))
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    bool flag = false;
                                    using (HashSet<ulong>.Enumerator enumerator = this.FilterOptions.Mods.GetEnumerator())
                                    {
                                        while (enumerator.MoveNext())
                                        {
                                            if ((lobbyMods == null) || !lobbyMods.Any<MyObjectBuilder_Checkpoint.ModItem>(delegate (MyObjectBuilder_Checkpoint.ModItem m) {
                                                ulong modId;
                                                return (m.PublishedFileId == modId);
                                            }))
                                            {
                                                flag = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (flag)
                                    {
                                        return;
                                    }
                                }
                            }
                            StringBuilder builder = new StringBuilder();
                            int num8 = 15;
                            int num9 = Math.Min(num8, lobbyModCount - 1);
                            foreach (MyObjectBuilder_Checkpoint.ModItem item in lobbyMods)
                            {
                                num8--;
                                if (num8 <= 0)
                                {
                                    builder.Append("...");
                                    break;
                                }
                                num9--;
                                if (num9 <= 0)
                                {
                                    builder.Append(item.FriendlyName);
                                    continue;
                                }
                                builder.AppendLine(item.FriendlyName);
                            }
                            MyGuiControlTable.Row row = new MyGuiControlTable.Row(lobby);
                            string toolTip = MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_Rank).ToString();
                            Color? textColor = null;
                            MyGuiHighlightTexture? icon = null;
                            row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(), null, toolTip, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
                            textColor = null;
                            icon = null;
                            row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
                            textColor = null;
                            icon = null;
                            row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
                            textColor = null;
                            icon = null;
                            row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append(lobbyWorldName), lobby.LobbyId, this.m_textCache.ToString(), textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                            textColor = null;
                            icon = null;
                            row.AddCell(new MyGuiControlTable.Cell(this.m_gameTypeText, null, (this.m_gameTypeToolTip.Length > 0) ? this.m_gameTypeToolTip.ToString() : null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                            textColor = null;
                            icon = null;
                            row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append(lobbyHostName), null, this.m_textCache.ToString(), textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                            textColor = null;
                            icon = null;
                            row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append(str5), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                            textColor = null;
                            icon = null;
                            row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append("---"), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                            textColor = null;
                            icon = null;
                            row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append((lobbyModCount == 0) ? "---" : lobbyModCount.ToString()), null, builder.ToString(), textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                            this.m_gamesTable.Add(row);
                            this.m_friendsPage.Text.Clear().Append(MyTexts.GetString(MyCommonTexts.JoinGame_TabTitle_Friends)).Append(" (").Append(this.m_gamesTable.RowsCount).Append(")");
                        }
                    }
                }
            }
        }

        private void AddServerHeaders()
        {
            int colIdx = 0;
            int columnsCount = this.m_gamesTable.ColumnsCount;
            if (colIdx < columnsCount)
            {
                colIdx++;
                this.m_gamesTable.SetColumnName(colIdx, new StringBuilder());
            }
            if (colIdx < columnsCount)
            {
                colIdx++;
                this.m_gamesTable.SetColumnName(colIdx, new StringBuilder());
            }
            if (colIdx < columnsCount)
            {
                colIdx++;
                this.m_gamesTable.SetColumnName(colIdx, new StringBuilder());
            }
            if (colIdx < columnsCount)
            {
                colIdx++;
                this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_World));
            }
            if (colIdx < columnsCount)
            {
                colIdx++;
                this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_GameMode));
            }
            if (colIdx < columnsCount)
            {
                colIdx++;
                this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_Server));
            }
            if (MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME && (colIdx < columnsCount))
            {
                colIdx++;
                this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_RemainingTime));
            }
            if (colIdx < columnsCount)
            {
                colIdx++;
                this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_Players));
            }
            if (colIdx < columnsCount)
            {
                colIdx++;
                this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_Ping));
            }
            if (colIdx < columnsCount)
            {
                colIdx++;
                this.m_gamesTable.SetColumnName(colIdx, MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_Mods));
            }
        }

        private bool AddServerItem(MyCachedServerItem item)
        {
            MyGameServerItem server = item.Server;
            server.Experimental = item.ExperimentalMode;
            if (!this.FilterOptions.AdvancedFilter || (item.Rules == null))
            {
                if (!this.FilterSimple(item, this.m_searchBox.SearchText))
                {
                    return false;
                }
            }
            else if (!this.FilterAdvanced(item, this.m_searchBox.SearchText))
            {
                return false;
            }
            string map = server.Map;
            StringBuilder gamemodeSB = new StringBuilder();
            StringBuilder gamemodeToolTipSB = new StringBuilder();
            string gameTagByPrefix = server.GetGameTagByPrefix("gamemode");
            if (gameTagByPrefix == "C")
            {
                gamemodeSB.Append(MyTexts.GetString(MyCommonTexts.WorldSettings_GameModeCreative));
                gamemodeToolTipSB.Append(MyTexts.GetString(MyCommonTexts.WorldSettings_GameModeCreative));
            }
            else if (!string.IsNullOrWhiteSpace(gameTagByPrefix))
            {
                string str3 = gameTagByPrefix.Substring(1);
                char[] separator = new char[] { '-' };
                string[] strArray = str3.Split(separator);
                if (strArray.Length != 4)
                {
                    gamemodeSB.Append(MyTexts.GetString(MyCommonTexts.WorldSettings_GameModeSurvival));
                    gamemodeToolTipSB.Append(MyTexts.GetString(MyCommonTexts.WorldSettings_GameModeSurvival));
                }
                else
                {
                    gamemodeSB.Append(MyTexts.GetString(MyCommonTexts.WorldSettings_GameModeSurvival)).Append(" ").Append(str3);
                    object[] args = new object[] { strArray[0], strArray[1], strArray[2], strArray[3] };
                    gamemodeToolTipSB.AppendFormat(MyTexts.GetString(MyCommonTexts.JoinGame_GameTypeToolTip_MultipliersFormat), args);
                }
            }
            if (!this.m_refreshPaused)
            {
                this.AddServerItem(server, map, gamemodeSB, gamemodeToolTipSB, true, item.Settings);
            }
            return true;
        }

        private void AddServerItem(MyGameServerItem server, string sessionName, StringBuilder gamemodeSB, StringBuilder gamemodeToolTipSB, bool sort = true, MyObjectBuilder_SessionSettings settings = null)
        {
            MyGuiHighlightTexture? nullable2;
            ulong gameTagByPrefixUlong = server.GetGameTagByPrefixUlong("mods");
            string str = server.MaxPlayers.ToString();
            StringBuilder text = new StringBuilder(server.Players + "/" + str);
            string gameTagByPrefix = server.GetGameTagByPrefix("view");
            if (!string.IsNullOrEmpty(gameTagByPrefix))
            {
                gamemodeToolTipSB.AppendLine();
                gamemodeToolTipSB.AppendFormat(MyTexts.GetString(MyCommonTexts.JoinGame_GameTypeToolTip_ViewDistance), gameTagByPrefix);
            }
            if (settings != null)
            {
                gamemodeToolTipSB.AppendLine();
                gamemodeToolTipSB.AppendFormat(MyTexts.GetString(MyCommonTexts.JoinGame_GameTypeToolTip_PCU_Max), settings.TotalPCU);
                gamemodeToolTipSB.AppendLine();
                gamemodeToolTipSB.AppendFormat(MyTexts.GetString(MyCommonTexts.JoinGame_GameTypeToolTip_PCU_Settings), settings.BlockLimitsEnabled);
                gamemodeToolTipSB.AppendLine();
                gamemodeToolTipSB.AppendFormat(MyTexts.GetString(MyCommonTexts.JoinGame_GameTypeToolTip_PCU_Initial), MyObjectBuilder_SessionSettings.GetInitialPCU(settings));
                gamemodeToolTipSB.AppendLine();
                gamemodeToolTipSB.AppendFormat(MyTexts.GetString(MyCommonTexts.JoinGame_GameTypeToolTip_Airtightness), settings.EnableOxygenPressurization ? MyTexts.GetString(MyCommonTexts.JoinGame_GameTypeToolTip_ON) : MyTexts.GetString(MyCommonTexts.JoinGame_GameTypeToolTip_OFF));
            }
            Color? textColor = new Color?(Color.White);
            if (server.Experimental && !MySandboxGame.Config.ExperimentalMode)
            {
                textColor = new Color?(Color.DarkGray);
            }
            MyGuiControlTable.Row row = new MyGuiControlTable.Row(server);
            string toolTip = MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_Rank).ToString();
            if (server.IsRanked)
            {
                nullable2 = new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_ICON_STAR);
                row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(), null, toolTip, textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            else
            {
                nullable2 = null;
                row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(), null, toolTip, textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            string str4 = MyTexts.Get(MyCommonTexts.JoinGame_ColumnTitle_Passworded).ToString();
            if (server.Password)
            {
                nullable2 = new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_ICON_LOCK);
                row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(), null, str4, textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            else
            {
                nullable2 = null;
                row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(), null, null, textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            if (server.Experimental)
            {
                nullable2 = new MyGuiHighlightTexture?(MyGuiConstants.TEXTURE_ICON_EXPERIMENTAL);
                row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(), null, MyTexts.GetString(MyCommonTexts.ServerIsExperimental), textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            else
            {
                nullable2 = null;
                row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(), null, null, textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
            }
            this.m_textCache.Clear().Append(sessionName);
            StringBuilder builder2 = new StringBuilder();
            builder2.AppendLine(sessionName);
            if (server.Experimental)
            {
                builder2.Append(MyTexts.GetString(MyCommonTexts.ServerIsExperimental));
            }
            nullable2 = null;
            row.AddCell(new MyGuiControlTable.Cell(this.m_textCache, server.GameID, builder2.ToString(), textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            nullable2 = null;
            row.AddCell(new MyGuiControlTable.Cell(gamemodeSB, null, gamemodeToolTipSB.ToString(), textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            nullable2 = null;
            row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append(server.Name), null, this.m_gameTypeToolTip.Clear().AppendLine(server.Name).Append(server.NetAdr).ToString(), textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            nullable2 = null;
            row.AddCell(new MyGuiControlTable.Cell(text, null, text.ToString(), textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            nullable2 = null;
            row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append(server.Ping), null, this.m_textCache.ToString(), textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            nullable2 = null;
            row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append((gameTagByPrefixUlong == 0) ? "---" : gameTagByPrefixUlong.ToString()), null, null, textColor, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
            if (!server.IsRanked)
            {
                this.m_gamesTable.Add(row);
            }
            else
            {
                row.IsGlobalSortEnabled = false;
                this.m_gamesTable.Insert(0, row);
            }
            if (sort && !server.IsRanked)
            {
                MyGuiControlTable.Row selectedRow = this.m_gamesTable.SelectedRow;
                this.m_gamesTable.Sort(false);
                this.m_gamesTable.SelectedRowIndex = new int?(this.m_gamesTable.FindRow(selectedRow));
            }
        }

        private void AdvancedSearchButtonClicked(MyGuiControlButton myGuiControlButton)
        {
            if ((this.m_detailsButton != null) && (this.m_joinButton != null))
            {
                this.m_detailsButton.Enabled = false;
                this.m_joinButton.Enabled = false;
            }
            MyGuiScreenServerSearchSpace screen = new MyGuiScreenServerSearchSpace(this);
            screen.Closed += b => this.m_searchChangedFunc();
            this.m_loadingWheel.Visible = false;
            MyGuiSandbox.AddScreen(screen);
        }

        private void CloseFavoritesPage()
        {
            this.CloseFavoritesRequest();
            this.m_searchChangedFunc = (Action) Delegate.Remove(this.m_searchChangedFunc, new Action(this.RefreshFavoritesGameList));
            this.m_favoritesResponding = false;
        }

        private void CloseFavoritesRequest()
        {
            MyGameService.OnFavoritesServerListResponded -= new EventHandler<int>(this.OnFavoritesServerListResponded);
            MyGameService.OnFavoritesServersCompleteResponse -= new EventHandler<MyMatchMakingServerResponse>(this.OnFavoritesServersCompleteResponse);
            MyGameService.CancelFavoritesServersRequest();
            this.m_loadingWheel.Visible = false;
        }

        private void CloseFriendsPage()
        {
            this.CloseFriendsRequest();
            this.m_searchChangedFunc = (Action) Delegate.Remove(this.m_searchChangedFunc, new Action(this.RefreshFriendsGameList));
        }

        private void CloseFriendsRequest()
        {
            MyGameService.OnDedicatedServerListResponded -= new EventHandler<int>(this.OnFriendsServerListResponded);
            MyGameService.OnDedicatedServersCompleteResponse -= new EventHandler<MyMatchMakingServerResponse>(this.OnFriendsServersCompleteResponse);
            this.m_loadingWheel.Visible = false;
        }

        private void CloseHistoryPage()
        {
            this.CloseHistoryRequest();
            this.m_historyResponding = false;
            this.m_searchChangedFunc = (Action) Delegate.Remove(this.m_searchChangedFunc, new Action(this.RefreshHistoryGameList));
        }

        private void CloseHistoryRequest()
        {
            MyGameService.OnHistoryServerListResponded -= new EventHandler<int>(this.OnHistoryServerListResponded);
            MyGameService.OnHistoryServersCompleteResponse -= new EventHandler<MyMatchMakingServerResponse>(this.OnHistoryServersCompleteResponse);
            MyGameService.CancelHistoryServersRequest();
            this.m_loadingWheel.Visible = false;
        }

        private void CloseLANPage()
        {
            this.CloseLANRequest();
            this.m_lanResponding = false;
            this.m_searchChangedFunc = (Action) Delegate.Remove(this.m_searchChangedFunc, new Action(this.RefreshLANGameList));
        }

        private void CloseLANRequest()
        {
            MyGameService.OnLANServerListResponded -= new EventHandler<int>(this.OnLANServerListResponded);
            MyGameService.OnLANServersCompleteResponse -= new EventHandler<MyMatchMakingServerResponse>(this.OnLANServersCompleteResponse);
            MyGameService.CancelLANServersRequest();
            this.m_loadingWheel.Visible = false;
        }

        private void CloseLobbyPage()
        {
            this.m_searchChangedFunc = (Action) Delegate.Remove(this.m_searchChangedFunc, new Action(this.LoadPublicLobbies));
        }

        private void CloseRequest()
        {
            this.m_loadingWheel.Visible = false;
            MyGameService.OnDedicatedServerListResponded -= new EventHandler<int>(this.OnDedicatedServerListResponded);
            MyGameService.OnDedicatedServersCompleteResponse -= new EventHandler<MyMatchMakingServerResponse>(this.OnDedicatedServersCompleteResponse);
            MyGameService.CancelInternetServersRequest();
            if (this.m_nextState == RefreshStateEnum.Pause)
            {
                this.m_refreshButton.Text = MyTexts.GetString(MyCommonTexts.ScreenLoadSubscribedWorldRefresh);
                this.m_nextState = RefreshStateEnum.Refresh;
                this.m_refreshPaused = false;
            }
        }

        private void CloseServersPage()
        {
            this.CloseRequest();
            this.m_dedicatedResponding = false;
            this.m_searchChangedFunc = (Action) Delegate.Remove(this.m_searchChangedFunc, () => this.RefreshServerGameList(false));
        }

        private void DedicatedRulesResponse(Dictionary<string, string> rules, MyCachedServerItem server)
        {
            if ((server.Server.NetAdr != null) && !this.m_dedicatedServers.Any<MyCachedServerItem>(x => server.Server.NetAdr.Equals(x.Server.NetAdr)))
            {
                server.Rules = rules;
                if (rules != null)
                {
                    server.DeserializeSettings();
                }
                this.m_dedicatedServers.Add(server);
                if (this.m_dedicatedResponding)
                {
                    server.Server.IsRanked = this.IsRanked(server);
                    this.AddServerItem(server);
                    if (!this.m_refreshPaused)
                    {
                        this.m_serversPage.Text.Clear().Append(MyTexts.GetString(MyCommonTexts.JoinGame_TabTitle_Servers)).Append(" (").Append(this.m_gamesTable.RowsCount).Append(")");
                    }
                }
            }
        }

        private void DirectConnectClick(MyGuiControlButton button)
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenServerConnect());
        }

        private void FavoritesRulesResponse(Dictionary<string, string> rules, MyCachedServerItem server)
        {
            if ((server.Server.NetAdr != null) && !this.m_favoriteServers.Any<MyCachedServerItem>(x => server.Server.NetAdr.Equals(x.Server.NetAdr)))
            {
                server.Rules = rules;
                if (rules != null)
                {
                    server.DeserializeSettings();
                }
                this.m_favoriteServers.Add(server);
                if (this.m_favoritesResponding)
                {
                    server.Server.IsRanked = this.IsRanked(server);
                    this.AddServerItem(server);
                    if (!this.m_refreshPaused)
                    {
                        this.m_favoritesPage.Text.Clear().Append(MyTexts.GetString(MyCommonTexts.JoinGame_TabTitle_Favorites)).Append(" (").Append(this.m_gamesTable.RowsCount).Append(")");
                    }
                }
            }
        }

        private bool FilterAdvanced(MyCachedServerItem item, string searchText = null)
        {
            if (!this.FilterSimple(item, searchText))
            {
                return false;
            }
            if ((item.Rules == null) || !item.Rules.Any<KeyValuePair<string, string>>())
            {
                return false;
            }
            if (!this.FilterOptions.FilterServer(item))
            {
                return false;
            }
            if (((this.FilterOptions.Mods != null) && this.FilterOptions.Mods.Any<ulong>()) && this.FilterOptions.AdvancedFilter)
            {
                if (this.FilterOptions.ModsExclusive)
                {
                    if (!this.FilterOptions.Mods.All<ulong>(modId => item.Mods.Contains(modId)))
                    {
                        return false;
                    }
                }
                else if ((item.Mods == null) || !item.Mods.Any<ulong>(modId => this.FilterOptions.Mods.Contains(modId)))
                {
                    return false;
                }
            }
            this.m_loadingWheel.Visible = false;
            return true;
        }

        private bool FilterSimple(MyCachedServerItem item, string searchText = null)
        {
            float num3;
            MyGameServerItem server = item.Server;
            if (server.AppID != MyGameService.AppId)
            {
                return false;
            }
            int serverVersion = server.ServerVersion;
            if (string.IsNullOrEmpty(server.Map))
            {
                return false;
            }
            if (((searchText != null) && !server.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)) && !server.Map.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }
            if (this.FilterOptions.AllowedGroups && !item.AllowedInGroup)
            {
                return false;
            }
            if (this.FilterOptions.SameVersion && (serverVersion != MyFinalBuildConstants.APP_VERSION))
            {
                return false;
            }
            if ((this.FilterOptions.HasPassword != null) && (this.FilterOptions.HasPassword.Value != server.Password))
            {
                return false;
            }
            if (MyFakes.ENABLE_MP_DATA_HASHES && this.FilterOptions.SameData)
            {
                string str2 = server.GetGameTagByPrefix("datahash");
                if ((str2 != "") && (str2 != this.m_dataHash))
                {
                    return false;
                }
            }
            string gameTagByPrefix = server.GetGameTagByPrefix("gamemode");
            if ((gameTagByPrefix == "C") && !this.FilterOptions.CreativeMode)
            {
                return false;
            }
            if (gameTagByPrefix.StartsWith("S") && !this.FilterOptions.SurvivalMode)
            {
                return false;
            }
            ulong gameTagByPrefixUlong = server.GetGameTagByPrefixUlong("mods");
            if (this.FilterOptions.CheckMod && !this.FilterOptions.ModCount.ValueBetween((float) gameTagByPrefixUlong))
            {
                return false;
            }
            if (this.FilterOptions.CheckPlayer && !this.FilterOptions.PlayerCount.ValueBetween((float) server.Players))
            {
                return false;
            }
            return (((this.FilterOptions.Ping <= -1) || (server.Ping <= this.FilterOptions.Ping)) && (!float.TryParse(server.GetGameTagByPrefix("view"), out num3) || (!this.FilterOptions.CheckDistance || this.FilterOptions.ViewDistance.ValueBetween(num3))));
        }

        private void FriendRulesResponse(Dictionary<string, string> rules, MyCachedServerItem server)
        {
            if ((server.Server.NetAdr != null) && !this.m_dedicatedServers.Any<MyCachedServerItem>(x => server.Server.NetAdr.Equals(x.Server.NetAdr)))
            {
                server.Rules = rules;
                if (rules != null)
                {
                    server.DeserializeSettings();
                }
                this.m_dedicatedServers.Add(server);
                server.Server.IsRanked = this.IsRanked(server);
                MyGuiControlTabPage friendsPage = this.m_friendsPage;
                lock (friendsPage)
                {
                    this.AddServerItem(server);
                    this.m_friendsPage.Text.Clear().Append(MyTexts.GetString(MyCommonTexts.JoinGame_TabTitle_Friends)).Append(" (").Append(this.m_gamesTable.RowsCount).Append(")");
                }
            }
        }

        private void FriendsLobbyResponse(bool success)
        {
            if (!success)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Cannot enumerate worlds"), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
            this.m_lobbies.Clear();
            MyGameService.AddFriendLobbies(this.m_lobbies);
            MyGameService.AddPublicLobbies(this.m_lobbies);
            foreach (IMyLobby lobby in this.m_lobbies)
            {
                if ((this.m_friendIds.Contains(lobby.OwnerId) || (this.m_friendIds.Contains(MyMultiplayerLobby.GetLobbyHostSteamId(lobby)) || this.m_friendIds.Contains(lobby.LobbyId))) || lobby.MemberList.Any<ulong>(m => this.m_friendIds.Contains(m)))
                {
                    MyGuiControlTabPage friendsPage = this.m_friendsPage;
                    lock (friendsPage)
                    {
                        this.AddLobby(lobby);
                    }
                }
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenJoinGame";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if (MyInput.Static.IsNewKeyPressed(MyKeys.F5) && (this.RefreshRequest != null))
            {
                this.RefreshRequest(this.m_refreshButton);
            }
        }

        private void HistoryRulesResponse(Dictionary<string, string> rules, MyCachedServerItem server)
        {
            if ((server.Server.NetAdr != null) && !this.m_historyServers.Any<MyCachedServerItem>(x => server.Server.NetAdr.Equals(x.Server.NetAdr)))
            {
                server.Rules = rules;
                if (rules != null)
                {
                    server.DeserializeSettings();
                }
                this.m_historyServers.Add(server);
                if (this.m_historyResponding)
                {
                    server.Server.IsRanked = this.IsRanked(server);
                    this.AddServerItem(server);
                    if (!this.m_refreshPaused)
                    {
                        this.m_historyPage.Text.Clear().Append(MyTexts.GetString(MyCommonTexts.JoinGame_TabTitle_History)).Append(" (").Append(this.m_gamesTable.RowsCount).Append(")");
                    }
                }
            }
        }

        private void InitFavoritesPage()
        {
            this.InitServersTable();
            this.m_joinButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnJoinServer);
            this.m_refreshButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnRefreshFavoritesServersClick);
            this.RefreshRequest = new Action<MyGuiControlButton>(this.OnRefreshFavoritesServersClick);
            this.m_detailsButton.UserData = this.m_favoriteServers;
            this.m_favoritesResponding = true;
            this.m_searchChangedFunc = (Action) Delegate.Combine(this.m_searchChangedFunc, new Action(this.RefreshFavoritesGameList));
            this.m_favoritesPage = this.m_selectedPage;
            this.m_favoritesPage.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_Favorites));
            this.RefreshFavoritesGameList();
        }

        private void InitFriendsPage()
        {
            this.InitServersTable();
            this.m_joinButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnJoinServer);
            this.m_refreshButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnRefreshFriendsServersClick);
            this.RefreshRequest = new Action<MyGuiControlButton>(this.OnRefreshFriendsServersClick);
            this.m_searchChangedFunc = (Action) Delegate.Combine(this.m_searchChangedFunc, new Action(this.RefreshFriendsGameList));
            this.m_detailsButton.UserData = this.m_dedicatedServers;
            this.m_friendsPage = this.m_selectedPage;
            this.m_friendsPage.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_Friends));
            if (this.m_friendIds == null)
            {
                this.m_friendIds = new HashSet<ulong>();
                this.m_friendNames = new HashSet<string>();
                this.RequestFriendsList();
            }
            this.RefreshFriendsGameList();
        }

        private void InitFriendsTable()
        {
            this.m_gamesTable.ColumnsCount = MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME ? 7 : 6;
            this.m_gamesTable.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            this.m_gamesTable.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnServerTableItemSelected);
            this.m_gamesTable.ItemDoubleClicked += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemDoubleClick);
            if (MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME)
            {
                this.m_gamesTable.SetCustomColumnWidths(new float[] { 0.26f, 0.18f, 0.2f, 0.16f, 0.08f, 0.05f, 0.07f });
            }
            else
            {
                this.m_gamesTable.SetCustomColumnWidths(new float[] { 0.3f, 0.19f, 0.31f, 0.08f, 0.05f, 0.07f });
            }
            int colIdx = 0;
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
            if (MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME)
            {
                this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
                this.m_gamesTable.SetColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                this.m_gamesTable.SetHeaderColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                colIdx++;
            }
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.PlayerCountComparison));
            this.m_gamesTable.SetColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_gamesTable.SetHeaderColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            colIdx++;
            int columnIdx = colIdx;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.PingComparison));
            this.m_gamesTable.SetColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_gamesTable.SetHeaderColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.ModsComparison));
            this.m_gamesTable.SetColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_gamesTable.SetHeaderColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            colIdx++;
            MyGuiControlTable.SortStateEnum? sortState = null;
            this.m_gamesTable.SortByColumn(columnIdx, sortState, true);
        }

        private void InitHistoryPage()
        {
            this.InitServersTable();
            this.m_joinButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnJoinServer);
            this.m_refreshButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnRefreshHistoryServersClick);
            this.RefreshRequest = new Action<MyGuiControlButton>(this.OnRefreshHistoryServersClick);
            this.m_historyResponding = true;
            this.m_searchChangedFunc = (Action) Delegate.Combine(this.m_searchChangedFunc, new Action(this.RefreshHistoryGameList));
            this.m_historyPage = this.m_selectedPage;
            this.m_historyPage.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_History));
            this.m_detailsButton.UserData = this.m_historyServers;
            this.RefreshHistoryGameList();
        }

        private void InitLANPage()
        {
            this.InitServersTable();
            this.m_joinButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnJoinServer);
            this.m_refreshButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnRefreshLANServersClick);
            this.RefreshRequest = new Action<MyGuiControlButton>(this.OnRefreshLANServersClick);
            this.m_detailsButton.UserData = this.m_lanServers;
            this.m_lanResponding = true;
            this.m_searchChangedFunc = (Action) Delegate.Combine(this.m_searchChangedFunc, new Action(this.RefreshLANGameList));
            this.m_LANPage = this.m_selectedPage;
            this.m_LANPage.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_LAN));
            this.RefreshLANGameList();
        }

        private void InitLobbyPage()
        {
            this.InitLobbyTable();
            this.m_detailsButton.Enabled = false;
            this.m_joinButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnJoinServer);
            this.m_refreshButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnRefreshLobbiesClick);
            this.RefreshRequest = new Action<MyGuiControlButton>(this.OnRefreshLobbiesClick);
            this.m_searchChangedFunc = (Action) Delegate.Combine(this.m_searchChangedFunc, new Action(this.RefreshGameList));
            this.m_lobbyPage = this.m_selectedPage;
            this.m_lobbyPage.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_Lobbies));
            this.LoadPublicLobbies();
        }

        private void InitLobbyTable()
        {
            this.m_gamesTable.ColumnsCount = MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME ? 6 : 5;
            this.m_gamesTable.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            this.m_gamesTable.ItemDoubleClicked += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemDoubleClick);
            if (MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME)
            {
                this.m_gamesTable.SetCustomColumnWidths(new float[] { 0.3f, 0.18f, 0.2f, 0.16f, 0.08f, 0.07f });
            }
            else
            {
                this.m_gamesTable.SetCustomColumnWidths(new float[] { 0.29f, 0.19f, 0.37f, 0.08f, 0.07f });
            }
            int colIdx = 0;
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
            if (MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME)
            {
                this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
                this.m_gamesTable.SetColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.m_gamesTable.SetHeaderColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                colIdx++;
            }
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.PlayerCountComparison));
            this.m_gamesTable.SetColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_gamesTable.SetHeaderColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.ModsComparison));
            this.m_gamesTable.SetColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_gamesTable.SetHeaderColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            colIdx++;
        }

        private void InitPageControls(MyGuiControlTabPage page)
        {
            int num;
            MyGuiControlButton button;
            page.Controls.Clear();
            if (this.m_joinButton != null)
            {
                this.Controls.Remove(this.m_joinButton);
            }
            if (this.m_detailsButton != null)
            {
                this.Controls.Remove(this.m_detailsButton);
            }
            if (this.m_directConnectButton != null)
            {
                this.Controls.Remove(this.m_directConnectButton);
            }
            if (this.m_refreshButton != null)
            {
                this.Controls.Remove(this.m_refreshButton);
            }
            Vector2 vector = new Vector2(-0.676f, -0.352f);
            this.m_gamesTable = new MyGuiControlTable();
            this.m_gamesTable.Position = vector + new Vector2(MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.Default).NormalTexture.MinSizeGui.X, 0.067f);
            this.m_gamesTable.Size = new Vector2(1450f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 1f);
            this.m_gamesTable.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_gamesTable.VisibleRowsCount = 0x11;
            page.Controls.Add(this.m_gamesTable);
            Vector2 vector3 = new Vector2(vector.X, 0f) - new Vector2(-0.3137f, (-base.m_size.Value.Y / 2f) + 0.071f);
            Vector2 vector4 = new Vector2(0.1825f, 0f);
            this.m_detailsButton = button = this.MakeButton(vector3 + (vector4 * num), MyCommonTexts.JoinGame_ServerDetails, MySpaceTexts.ToolTipJoinGame_ServerDetails, new Action<MyGuiControlButton>(this.ServerDetailsClick));
            this.Controls.Add(button);
            this.m_directConnectButton = button = this.MakeButton(vector3 + (vector4 * num), MyCommonTexts.JoinGame_DirectConnect, MySpaceTexts.ToolTipJoinGame_DirectConnect, new Action<MyGuiControlButton>(this.DirectConnectClick));
            this.Controls.Add(button);
            this.m_refreshButton = button = this.MakeButton(vector3 + (vector4 * num), MyCommonTexts.ScreenLoadSubscribedWorldRefresh, MySpaceTexts.ToolTipJoinGame_Refresh, null);
            this.Controls.Add(button);
            num = (((0 + 1) + 1) + 1) + 1;
            this.m_joinButton = button = this.MakeButton(vector3 + (vector4 * num), MyCommonTexts.ScreenMenuButtonJoinWorld, MySpaceTexts.ToolTipJoinGame_JoinWorld, null);
            this.Controls.Add(button);
            this.m_joinButton.Enabled = false;
            this.m_detailsButton.Enabled = false;
            Vector2? textureResolution = null;
            this.m_loadingWheel = new MyGuiControlRotatingWheel(new Vector2?(this.m_joinButton.Position + new Vector2(0.2f, -0.026f)), new VRageMath.Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.22f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @"Textures\GUI\screens\screen_loading_wheel.dds", true, true, textureResolution, 1.5f);
            page.Controls.Add(this.m_loadingWheel);
            this.m_loadingWheel.Visible = false;
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.895f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.895f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.895f) / 2f, (base.m_size.Value.Y / 2f) - 0.152f), base.m_size.Value.X * 0.895f, 0f, color);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            color = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.895f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.895f, 0f, color);
            this.Controls.Add(list2);
        }

        private void InitServersPage()
        {
            this.InitServersTable();
            this.m_joinButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnJoinServer);
            this.m_refreshButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnRefreshServersClick);
            this.RefreshRequest = new Action<MyGuiControlButton>(this.OnRefreshServersClick);
            this.m_detailsButton.UserData = this.m_dedicatedServers;
            this.m_dedicatedResponding = true;
            this.m_searchChangedFunc = (Action) Delegate.Combine(this.m_searchChangedFunc, () => this.RefreshServerGameList(false));
            this.m_serversPage = this.m_selectedPage;
            this.m_serversPage.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_Servers));
            this.RefreshServerGameList(true);
        }

        private void InitServersTable()
        {
            this.m_gamesTable.ColumnsCount = MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME ? 10 : 9;
            this.m_gamesTable.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemSelected);
            this.m_gamesTable.ItemSelected += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnServerTableItemSelected);
            this.m_gamesTable.ItemDoubleClicked += new Action<MyGuiControlTable, MyGuiControlTable.EventArgs>(this.OnTableItemDoubleClick);
            if (MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME)
            {
                this.m_gamesTable.SetCustomColumnWidths(new float[] { 0.024f, 0.024f, 0.024f, 0.24f, 0.17f, 0.19f, 0.16f, 0.08f, 0.05f, 0.07f });
            }
            else
            {
                this.m_gamesTable.SetCustomColumnWidths(new float[] { 0.024f, 0.024f, 0.024f, 0.26f, 0.17f, 0.3f, 0.08f, 0.06f, 0.07f });
            }
            int colIdx = 3;
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
            if (MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME)
            {
                this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.TextComparison));
                this.m_gamesTable.SetColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.m_gamesTable.SetHeaderColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                colIdx++;
            }
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.PlayerCountComparison));
            this.m_gamesTable.SetColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_gamesTable.SetHeaderColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            colIdx++;
            int columnIdx = colIdx;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.PingComparison));
            this.m_gamesTable.SetColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_gamesTable.SetHeaderColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            colIdx++;
            this.m_gamesTable.SetColumnComparison(colIdx, new Comparison<MyGuiControlTable.Cell>(this.ModsComparison));
            this.m_gamesTable.SetColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_gamesTable.SetHeaderColumnAlign(colIdx, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            colIdx++;
            MyGuiControlTable.SortStateEnum? sortState = null;
            this.m_gamesTable.SortByColumn(columnIdx, sortState, true);
        }

        private bool IsRanked(MyCachedServerItem server)
        {
            if (this.m_rankedServers == null)
            {
                return false;
            }
            string address = server.Server.NetAdr.ToString();
            return this.m_rankedServers.Servers.Exists(r => r.Address == address);
        }

        private void joinGameTabs_OnPageChanged()
        {
            MyGuiControlTabPage controlByName = (MyGuiControlTabPage) this.m_joinGameTabs.Controls.GetControlByName("PageServersPanel");
            controlByName.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_Servers));
            MyGuiControlTabPage objB = (MyGuiControlTabPage) this.m_joinGameTabs.Controls.GetControlByName("PageLobbiesPanel");
            objB.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_Lobbies));
            MyGuiControlTabPage page3 = (MyGuiControlTabPage) this.m_joinGameTabs.Controls.GetControlByName("PageFavoritesPanel");
            page3.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_Favorites));
            MyGuiControlTabPage page4 = (MyGuiControlTabPage) this.m_joinGameTabs.Controls.GetControlByName("PageHistoryPanel");
            page4.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_History));
            MyGuiControlTabPage page5 = (MyGuiControlTabPage) this.m_joinGameTabs.Controls.GetControlByName("PageLANPanel");
            page5.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_LAN));
            MyGuiControlTabPage page6 = (MyGuiControlTabPage) this.m_joinGameTabs.Controls.GetControlByName("PageFriendsPanel");
            page6.SetToolTip(MyTexts.GetString(MyCommonTexts.JoinGame_TabTooltip_Friends));
            if (ReferenceEquals(this.m_selectedPage, controlByName))
            {
                this.CloseServersPage();
            }
            else if (ReferenceEquals(this.m_selectedPage, objB))
            {
                this.CloseLobbyPage();
            }
            else if (ReferenceEquals(this.m_selectedPage, page3))
            {
                this.CloseFavoritesPage();
            }
            else if (ReferenceEquals(this.m_selectedPage, page5))
            {
                this.CloseLANPage();
            }
            else if (ReferenceEquals(this.m_selectedPage, page4))
            {
                this.CloseHistoryPage();
            }
            else if (ReferenceEquals(this.m_selectedPage, page6))
            {
                this.CloseFriendsPage();
            }
            this.m_selectedPage = this.m_joinGameTabs.GetTabSubControl(this.m_joinGameTabs.SelectedPage);
            this.InitPageControls(this.m_selectedPage);
            if (ReferenceEquals(this.m_selectedPage, controlByName))
            {
                this.InitServersPage();
                this.EnableAdvancedSearch = true;
            }
            else if (ReferenceEquals(this.m_selectedPage, objB))
            {
                this.InitLobbyPage();
                this.EnableAdvancedSearch = false;
            }
            else if (ReferenceEquals(this.m_selectedPage, page3))
            {
                this.InitFavoritesPage();
                this.EnableAdvancedSearch = true;
            }
            else if (ReferenceEquals(this.m_selectedPage, page4))
            {
                this.InitHistoryPage();
                this.EnableAdvancedSearch = true;
            }
            else if (ReferenceEquals(this.m_selectedPage, page5))
            {
                this.InitLANPage();
                this.EnableAdvancedSearch = true;
            }
            else if (ReferenceEquals(this.m_selectedPage, page6))
            {
                this.InitFriendsPage();
                this.EnableAdvancedSearch = false;
            }
            if (this.m_contextMenu != null)
            {
                this.m_contextMenu.Deactivate();
                this.m_contextMenu = null;
            }
            this.m_contextMenu = new MyGuiControlContextMenu();
            this.m_contextMenu.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            this.m_contextMenu.Deactivate();
            this.m_contextMenu.ItemClicked += new Action<MyGuiControlContextMenu, MyGuiControlContextMenu.EventArgs>(this.OnContextMenu_ItemClicked);
            this.Controls.Add(this.m_contextMenu);
        }

        private void JoinSelectedServer(bool checkPing = true)
        {
            MyGuiControlTable.Row selectedRow = this.m_gamesTable.SelectedRow;
            if (selectedRow != null)
            {
                MyGameServerItem userData = selectedRow.UserData as MyGameServerItem;
                if (userData == null)
                {
                    IMyLobby lobby = selectedRow.UserData as IMyLobby;
                    if (lobby != null)
                    {
                        MyJoinGameHelper.JoinGame(lobby, true);
                        MyLocalCache.SaveLastSessionInfo(null, true, true, selectedRow.GetCell(0).Text.ToString(), lobby.LobbyId.ToString(), 0);
                    }
                }
                else
                {
                    StringBuilder builder;
                    MyStringId? nullable;
                    Vector2? nullable2;
                    if (!MySandboxGame.Config.ExperimentalMode && userData.Experimental)
                    {
                        builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionInfo);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MultiplayerErrorExperimental), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                    else if (!checkPing || (userData.Ping <= 150))
                    {
                        MyJoinGameHelper.JoinGame(userData, true);
                        MyLocalCache.SaveLastSessionInfo(null, true, false, userData.Name, userData.NetAdr.Address.ToString(), userData.NetAdr.Port);
                    }
                    else
                    {
                        builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MultiplayerWarningPing), builder, nullable, nullable, nullable, nullable, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                            if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                            {
                                this.JoinSelectedServer(false);
                            }
                        }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                }
            }
        }

        private void LanRulesResponse(Dictionary<string, string> rules, MyCachedServerItem server)
        {
            if ((server.Server.NetAdr != null) && !this.m_lanServers.Any<MyCachedServerItem>(x => server.Server.NetAdr.Equals(x.Server.NetAdr)))
            {
                server.Rules = rules;
                if (rules != null)
                {
                    server.DeserializeSettings();
                }
                this.m_lanServers.Add(server);
                if (this.m_lanResponding)
                {
                    server.Server.IsRanked = this.IsRanked(server);
                    this.AddServerItem(server);
                    if (!this.m_refreshPaused)
                    {
                        this.m_LANPage.Text.Clear().Append(MyTexts.GetString(MyCommonTexts.JoinGame_TabTitle_LAN)).Append(" (").Append(this.m_gamesTable.RowsCount).Append(")");
                    }
                }
            }
        }

        private void LoadPlayersCompleted(Dictionary<string, float> players, MyCachedServerItem serverItem)
        {
            if ((players != null) && players.Keys.Any<string>(n => this.m_friendNames.Contains(n)))
            {
                IPEndPoint netAdr = serverItem.Server.NetAdr;
                MyGameService.GetServerRules(netAdr.Address.ToIPv4NetworkOrder(), (ushort) netAdr.Port, rules => this.FriendRulesResponse(rules, serverItem), () => this.FriendRulesResponse(null, serverItem));
            }
        }

        private void LoadPublicLobbies()
        {
            this.m_loadingWheel.Visible = true;
            MySandboxGame.Log.WriteLine("Requesting lobbies");
            if (this.FilterOptions.SameVersion)
            {
                MyGameService.AddLobbyFilter("appVersion", MyFinalBuildConstants.APP_VERSION.ToString());
            }
            MySandboxGame.Log.WriteLine("Requesting worlds, only compatible: " + this.FilterOptions.SameVersion.ToString());
            MyGameService.RequestLobbyList(new Action<bool>(this.PublicLobbiesCallback));
        }

        private MyGuiControlButton MakeButton(Vector2 position, MyStringId text, MyStringId toolTip, Action<MyGuiControlButton> onClick)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            StringBuilder builder = MyTexts.Get(text);
            return new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Default, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(toolTip), builder, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, null, false);
        }

        private int ModsComparison(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b)
        {
            ulong gameID;
            ulong gameID;
            int result = 0;
            int.TryParse(a.Text.ToString(), out result);
            int num2 = 0;
            int.TryParse(b.Text.ToString(), out num2);
            if (result != num2)
            {
                return result.CompareTo(num2);
            }
            IMyMultiplayerGame userData = a.Row.UserData as IMyMultiplayerGame;
            if (userData != null)
            {
                gameID = userData.GameID;
            }
            else
            {
                MySteamLobby lobby = a.Row.UserData as MySteamLobby;
                if (lobby == null)
                {
                    return 0;
                }
                gameID = lobby.LobbyId;
            }
            IMyMultiplayerGame game2 = b.Row.UserData as IMyMultiplayerGame;
            if (game2 != null)
            {
                gameID = game2.GameID;
            }
            else
            {
                MySteamLobby lobby2 = b.Row.UserData as MySteamLobby;
                if (lobby2 == null)
                {
                    return 0;
                }
                gameID = lobby2.LobbyId;
            }
            return gameID.CompareTo(gameID);
        }

        private void onBannerClick(MyGuiControlImageButton button)
        {
            MyGuiSandbox.OpenUrl((string) button.UserData, UrlOpenMode.ExternalBrowser, null);
        }

        private void OnBlockSearchTextChanged(string text)
        {
            if ((this.m_detailsButton != null) && (this.m_joinButton != null))
            {
                this.m_detailsButton.Enabled = false;
                this.m_joinButton.Enabled = false;
            }
            this.m_searchChanged = true;
            this.m_searchLastChanged = DateTime.Now;
        }

        protected override void OnClosed()
        {
            this.CloseRequest();
            MySandboxGame.Config.ServerSearchSettings = this.FilterOptions.GetObjectBuilder();
            MySandboxGame.Config.Save();
            base.OnClosed();
        }

        private void OnContextMenu_ItemClicked(MyGuiControlContextMenu sender, MyGuiControlContextMenu.EventArgs eventArgs)
        {
            ContextMenuFavoriteActionItem userData = (ContextMenuFavoriteActionItem) eventArgs.UserData;
            MyGameServerItem server = userData.Server;
            if (server != null)
            {
                ContextMenuFavoriteAction action = userData._Action;
                if (action == ContextMenuFavoriteAction.Add)
                {
                    MyGameService.AddFavoriteGame(server.NetAdr.Address.ToIPv4NetworkOrder(), (ushort) server.NetAdr.Port, (ushort) server.NetAdr.Port);
                }
                else
                {
                    if (action != ContextMenuFavoriteAction.Remove)
                    {
                        throw new InvalidBranchException();
                    }
                    MyGameService.RemoveFavoriteGame(server.NetAdr.Address.ToIPv4NetworkOrder(), (ushort) server.NetAdr.Port, (ushort) server.NetAdr.Port);
                    this.m_gamesTable.RemoveSelectedRow();
                    this.m_favoritesPage.Text = new StringBuilder().Append(MyTexts.Get(MyCommonTexts.JoinGame_TabTitle_Favorites)).Append(" (").Append(this.m_gamesTable.RowsCount).Append(")");
                }
            }
        }

        private void OnDedicatedServerListResponded(object sender, int server)
        {
            MyCachedServerItem serverItem = new MyCachedServerItem(MyGameService.GetDedicatedServerDetails(server));
            IPEndPoint netAdr = serverItem.Server.NetAdr;
            MyGameService.GetServerRules(netAdr.Address.ToIPv4NetworkOrder(), (ushort) netAdr.Port, rules => this.DedicatedRulesResponse(rules, serverItem), () => this.DedicatedRulesResponse(null, serverItem));
        }

        private void OnDedicatedServersCompleteResponse(object sender, MyMatchMakingServerResponse response)
        {
            this.CloseRequest();
        }

        private void OnFavoritesCheckboxCheckChanged(MyGuiControlCheckbox checkbox)
        {
            this.RefreshFavoritesGameList();
        }

        private void OnFavoritesServerListResponded(object sender, int server)
        {
            MyCachedServerItem serverItem = new MyCachedServerItem(MyGameService.GetFavoritesServerDetails(server));
            IPEndPoint netAdr = serverItem.Server.NetAdr;
            MyGameService.GetServerRules(netAdr.Address.ToIPv4NetworkOrder(), (ushort) netAdr.Port, rules => this.FavoritesRulesResponse(rules, serverItem), () => this.FavoritesRulesResponse(null, serverItem));
        }

        private void OnFavoritesServersCompleteResponse(object sender, MyMatchMakingServerResponse response)
        {
            this.CloseFavoritesRequest();
        }

        private void OnFriendsServerListResponded(object sender, int server)
        {
            MyCachedServerItem serverItem = new MyCachedServerItem(MyGameService.GetDedicatedServerDetails(server));
            if (serverItem.Server.Players > 0)
            {
                MyGameService.GetPlayerDetails(serverItem.Server.NetAdr.Address.ToIPv4NetworkOrder(), (ushort) serverItem.Server.NetAdr.Port, players => this.LoadPlayersCompleted(players, serverItem), () => this.LoadPlayersCompleted(null, serverItem));
            }
        }

        private void OnFriendsServersCompleteResponse(object sender, MyMatchMakingServerResponse response)
        {
            this.CloseFriendsRequest();
        }

        private void OnHistoryCheckboxCheckChanged(MyGuiControlCheckbox checkbox)
        {
            this.RefreshHistoryGameList();
        }

        private void OnHistoryServerListResponded(object sender, int server)
        {
            MyCachedServerItem serverItem = new MyCachedServerItem(MyGameService.GetHistoryServerDetails(server));
            IPEndPoint netAdr = serverItem.Server.NetAdr;
            MyGameService.GetServerRules(netAdr.Address.ToIPv4NetworkOrder(), (ushort) netAdr.Port, rules => this.HistoryRulesResponse(rules, serverItem), () => this.HistoryRulesResponse(null, serverItem));
        }

        private void OnHistoryServersCompleteResponse(object sender, MyMatchMakingServerResponse response)
        {
            this.CloseHistoryRequest();
        }

        private void OnJoinServer(MyGuiControlButton obj)
        {
            this.JoinSelectedServer(true);
        }

        private void OnLANCheckboxCheckChanged(MyGuiControlCheckbox checkbox)
        {
            this.RefreshLANGameList();
        }

        private void OnLANServerListResponded(object sender, int server)
        {
            MyCachedServerItem serverItem = new MyCachedServerItem(MyGameService.GetLANServerDetails(server));
            IPEndPoint netAdr = serverItem.Server.NetAdr;
            MyGameService.GetServerRules(netAdr.Address.ToIPv4NetworkOrder(), (ushort) netAdr.Port, rules => this.LanRulesResponse(rules, serverItem), () => this.LanRulesResponse(null, serverItem));
        }

        private void OnLANServersCompleteResponse(object sender, MyMatchMakingServerResponse response)
        {
            this.CloseLANRequest();
        }

        private void OnRankedServersLoaded(MyRankedServers rankedServers)
        {
            this.m_rankedServers = rankedServers;
        }

        private void OnRefreshFavoritesServersClick(MyGuiControlButton obj)
        {
            this.RefreshFavoritesGameList();
        }

        private void OnRefreshFriendsServersClick(MyGuiControlButton obj)
        {
            this.RefreshFriendsGameList();
        }

        private void OnRefreshHistoryServersClick(MyGuiControlButton obj)
        {
            this.RefreshHistoryGameList();
        }

        private void OnRefreshLANServersClick(MyGuiControlButton obj)
        {
            this.RefreshLANGameList();
        }

        private void OnRefreshLobbiesClick(MyGuiControlButton obj)
        {
            this.LoadPublicLobbies();
        }

        private void OnRefreshServersClick(MyGuiControlButton obj)
        {
            if ((this.m_detailsButton != null) && (this.m_joinButton != null))
            {
                this.m_detailsButton.Enabled = false;
                this.m_joinButton.Enabled = false;
            }
            switch (this.m_nextState)
            {
                case RefreshStateEnum.Pause:
                    this.m_refreshPaused = true;
                    this.m_refreshButton.Text = MyTexts.GetString(MyCommonTexts.ScreenLoadSubscribedWorldResume);
                    this.m_nextState = RefreshStateEnum.Resume;
                    this.m_loadingWheel.Visible = false;
                    return;

                case RefreshStateEnum.Resume:
                    this.m_refreshPaused = false;
                    if (this.m_loadingWheel.Visible)
                    {
                        this.m_refreshButton.Text = MyTexts.GetString(MyCommonTexts.ScreenLoadSubscribedWorldPause);
                        this.m_nextState = RefreshStateEnum.Pause;
                        this.m_loadingWheel.Visible = true;
                    }
                    else
                    {
                        this.m_refreshButton.Text = MyTexts.GetString(MyCommonTexts.ScreenLoadSubscribedWorldRefresh);
                        this.m_nextState = RefreshStateEnum.Refresh;
                        this.m_loadingWheel.Visible = false;
                    }
                    this.RefreshServerGameList(false);
                    return;

                case RefreshStateEnum.Refresh:
                    this.m_refreshButton.Text = MyTexts.GetString(MyCommonTexts.ScreenLoadSubscribedWorldPause);
                    this.m_nextState = RefreshStateEnum.Pause;
                    this.m_dedicatedServers.Clear();
                    this.RefreshServerGameList(true);
                    this.m_loadingWheel.Visible = true;
                    return;
            }
            throw new ArgumentOutOfRangeException();
        }

        private void OnServerCheckboxCheckChanged(MyGuiControlCheckbox checkbox)
        {
            this.RefreshServerGameList(false);
        }

        private void OnServerTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            if (sender.SelectedRow != null)
            {
                MyGameServerItem userData = sender.SelectedRow.UserData as MyGameServerItem;
                if ((userData != null) && (userData.NetAdr != null))
                {
                    MyGuiControlTable.Cell cell = sender.SelectedRow.GetCell(5);
                    if ((cell != null) && (cell.ToolTip != null))
                    {
                        if (eventArgs.MouseButton != MyMouseButtonsEnum.Right)
                        {
                            this.m_contextMenu.Deactivate();
                        }
                        else
                        {
                            this.m_contextMenu.CreateNewContextMenu();
                            ContextMenuFavoriteAction action = ReferenceEquals(this.m_selectedPage, this.m_favoritesPage) ? ContextMenuFavoriteAction.Remove : ContextMenuFavoriteAction.Add;
                            MyStringId id = MyCommonTexts.JoinGame_Favorites_Remove;
                            if (action == ContextMenuFavoriteAction.Add)
                            {
                                id = MyCommonTexts.JoinGame_Favorites_Add;
                            }
                            ContextMenuFavoriteActionItem item2 = new ContextMenuFavoriteActionItem {
                                Server = userData,
                                _Action = action
                            };
                            this.m_contextMenu.AddItem(MyTexts.Get(id), "", "", item2);
                            this.m_contextMenu.Activate(true);
                        }
                    }
                }
            }
        }

        private void OnShowCompatibleCheckChanged(MyGuiControlCheckbox checkbox)
        {
            this.LoadPublicLobbies();
        }

        private void OnShowOnlyFriendsCheckChanged(MyGuiControlCheckbox checkbox)
        {
            this.LoadPublicLobbies();
        }

        private void OnTableItemDoubleClick(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            this.JoinSelectedServer(true);
        }

        private void OnTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
        {
            sender.CanHaveFocus = true;
            base.FocusedControl = sender;
            if (this.m_gamesTable.SelectedRow == null)
            {
                this.m_joinButton.Enabled = false;
                this.m_detailsButton.Enabled = false;
            }
            else
            {
                this.m_joinButton.Enabled = true;
                if (this.m_gamesTable.SelectedRow.UserData is MyGameServerItem)
                {
                    this.m_detailsButton.Enabled = true;
                }
            }
        }

        private int PingComparison(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b)
        {
            int num;
            int num2;
            ulong gameID;
            ulong gameID;
            if (!int.TryParse(a.Text.ToString(), out num))
            {
                num = -1;
            }
            if (!int.TryParse(b.Text.ToString(), out num2))
            {
                num2 = -1;
            }
            if (num != num2)
            {
                return num.CompareTo(num2);
            }
            IMyMultiplayerGame userData = a.Row.UserData as IMyMultiplayerGame;
            if (userData != null)
            {
                gameID = userData.GameID;
            }
            else
            {
                MySteamLobby lobby = a.Row.UserData as MySteamLobby;
                if (lobby == null)
                {
                    return 0;
                }
                gameID = lobby.LobbyId;
            }
            IMyMultiplayerGame game2 = b.Row.UserData as IMyMultiplayerGame;
            if (game2 != null)
            {
                gameID = game2.GameID;
            }
            else
            {
                MySteamLobby lobby2 = b.Row.UserData as MySteamLobby;
                if (lobby2 == null)
                {
                    return 0;
                }
                gameID = lobby2.LobbyId;
            }
            return gameID.CompareTo(gameID);
        }

        private int PlayerCountComparison(MyGuiControlTable.Cell b, MyGuiControlTable.Cell a)
        {
            ulong gameID;
            ulong gameID;
            List<StringBuilder> list = a.Text.Split('/');
            List<StringBuilder> list2 = b.Text.Split('/');
            int result = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            bool flag = true;
            if ((list.Count < 2) || (list2.Count < 2))
            {
                flag = false;
            }
            else
            {
                flag = (((flag & int.TryParse(list[0].ToString(), out result)) & int.TryParse(list2[0].ToString(), out num2)) & int.TryParse(list[1].ToString(), out num3)) & int.TryParse(list2[1].ToString(), out num4);
            }
            if ((result != num2) && flag)
            {
                return result.CompareTo(num2);
            }
            if ((num3 != num4) && flag)
            {
                return num3.CompareTo(num4);
            }
            IMyMultiplayerGame userData = a.Row.UserData as IMyMultiplayerGame;
            if (userData != null)
            {
                gameID = userData.GameID;
            }
            else
            {
                MySteamLobby lobby = a.Row.UserData as MySteamLobby;
                if (lobby == null)
                {
                    return 0;
                }
                gameID = lobby.LobbyId;
            }
            IMyMultiplayerGame game2 = b.Row.UserData as IMyMultiplayerGame;
            if (game2 != null)
            {
                gameID = game2.GameID;
            }
            else
            {
                MySteamLobby lobby2 = b.Row.UserData as MySteamLobby;
                if (lobby2 == null)
                {
                    return 0;
                }
                gameID = lobby2.LobbyId;
            }
            return gameID.CompareTo(gameID);
        }

        private void PublicLobbiesCallback(bool success)
        {
            if (ReferenceEquals(this.m_selectedPage, this.m_lobbyPage))
            {
                if (!success)
                {
                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Cannot enumerate worlds"), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
                else
                {
                    this.m_lobbies.Clear();
                    MyGameService.AddPublicLobbies(this.m_lobbies);
                    this.RefreshGameList();
                    this.m_loadingWheel.Visible = false;
                }
            }
        }

        private void RebuildFavoritesList()
        {
            this.m_detailsButton.Enabled = false;
            this.m_joinButton.Enabled = false;
            this.m_gamesTable.Clear();
            foreach (MyCachedServerItem item in this.m_favoriteServers)
            {
                this.AddServerItem(item);
            }
            this.m_favoritesPage.Text.Clear().Append(MyTexts.GetString(MyCommonTexts.JoinGame_TabTitle_Favorites)).Append(" (").Append(this.m_gamesTable.RowsCount).Append(")");
        }

        private void RebuildServerList()
        {
            string str;
            if (string.IsNullOrWhiteSpace(this.m_searchBox.SearchText))
            {
                str = null;
            }
            this.m_detailsButton.Enabled = false;
            this.m_joinButton.Enabled = false;
            this.m_gamesTable.Clear();
            foreach (MyCachedServerItem item in this.m_dedicatedServers)
            {
                if (this.FilterOptions.AdvancedFilter)
                {
                    if (!this.FilterAdvanced(item, str))
                    {
                        continue;
                    }
                }
                else if (!this.FilterSimple(item, str))
                {
                    continue;
                }
                MyGameServerItem server = item.Server;
                StringBuilder gamemodeSB = new StringBuilder();
                StringBuilder gamemodeToolTipSB = new StringBuilder();
                string gameTagByPrefix = server.GetGameTagByPrefix("gamemode");
                if (gameTagByPrefix == "C")
                {
                    gamemodeSB.Append(MyTexts.GetString(MyCommonTexts.WorldSettings_GameModeCreative));
                    gamemodeToolTipSB.Append(MyTexts.GetString(MyCommonTexts.WorldSettings_GameModeCreative));
                }
                else if (!string.IsNullOrWhiteSpace(gameTagByPrefix))
                {
                    string str3 = gameTagByPrefix.Substring(1);
                    char[] separator = new char[] { '-' };
                    string[] strArray = str3.Split(separator);
                    if (strArray.Length != 4)
                    {
                        gamemodeSB.Append(MyTexts.GetString(MyCommonTexts.WorldSettings_GameModeSurvival));
                        gamemodeToolTipSB.Append(MyTexts.GetString(MyCommonTexts.WorldSettings_GameModeSurvival));
                    }
                    else
                    {
                        gamemodeSB.Append(MyTexts.GetString(MyCommonTexts.WorldSettings_GameModeSurvival)).Append(" ").Append(str3);
                        object[] args = new object[] { strArray[0], strArray[1], strArray[2], strArray[3] };
                        gamemodeToolTipSB.AppendFormat(MyTexts.GetString(MyCommonTexts.JoinGame_GameTypeToolTip_MultipliersFormat), args);
                    }
                }
                this.AddServerItem(server, server.Map, gamemodeSB, gamemodeToolTipSB, false, item.Settings);
            }
            this.m_gamesTable.Sort(false);
            this.m_serversPage.Text.Clear().Append(MyTexts.GetString(MyCommonTexts.JoinGame_TabTitle_Servers)).Append(" (").Append(this.m_gamesTable.RowsCount).Append(")");
        }

        public override void RecreateControls(bool constructor)
        {
            MyObjectBuilder_GuiScreen screen;
            base.RecreateControls(constructor);
            string str = MakeScreenFilepath("JoinScreen");
            MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_GuiScreen>(Path.Combine(MyFileSystem.ContentPath, str), out screen);
            base.Init(screen);
            this.m_joinGameTabs = this.Controls.GetControlByName("JoinGameTabs") as MyGuiControlTabControl;
            this.m_joinGameTabs.PositionY -= 0.018f;
            this.m_searchBox = new MyGuiControlSearchBox(new Vector2(-0.453f, -0.3f), new Vector2(0.754f, 0.02f), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_searchBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_searchBox.OnTextChanged += new MyGuiControlSearchBox.TextChangedDelegate(this.OnBlockSearchTextChanged);
            this.Controls.Add(this.m_searchBox);
            MyGuiControlButton button1 = new MyGuiControlButton();
            button1.Position = this.m_searchBox.Position + new Vector2(this.m_searchBox.Size.X + 0.155f, 0.006f);
            button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            button1.VisualStyle = MyGuiControlButtonStyleEnum.ComboBoxButton;
            button1.Text = MyTexts.GetString(MyCommonTexts.Advanced);
            this.m_advancedSearchButton = button1;
            this.m_advancedSearchButton.ButtonClicked += new Action<MyGuiControlButton>(this.AdvancedSearchButtonClicked);
            this.m_advancedSearchButton.SetToolTip(MySpaceTexts.ToolTipJoinGame_Advanced);
            this.Controls.Add(this.m_advancedSearchButton);
            this.m_joinGameTabs.TabButtonScale = 0.86f;
            this.m_joinGameTabs.OnPageChanged += new Action(this.joinGameTabs_OnPageChanged);
            this.joinGameTabs_OnPageChanged();
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenMenuButtonJoinGame, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            base.CloseButtonEnabled = true;
            if ((((float) MySandboxGame.ScreenSize.X) / ((float) MySandboxGame.ScreenSize.Y)) == 1.25f)
            {
                base.SetCloseButtonOffset_5_to_4();
            }
            else
            {
                base.SetDefaultCloseButtonOffset();
            }
            string centerTexture = @"Textures\GUI\gtxlogobigger.png";
            string str3 = @"Textures\GUI\gtxlogobiggerHighlight.png";
            MyGuiControlImageButton.StateDefinition definition1 = new MyGuiControlImageButton.StateDefinition();
            definition1.Texture = new MyGuiCompositeTexture(str3);
            MyGuiControlImageButton.StyleDefinition style = new MyGuiControlImageButton.StyleDefinition();
            style.Highlight = definition1;
            MyGuiControlImageButton.StateDefinition definition3 = new MyGuiControlImageButton.StateDefinition();
            definition3.Texture = new MyGuiCompositeTexture(centerTexture);
            style.ActiveHighlight = definition3;
            MyGuiControlImageButton.StateDefinition definition4 = new MyGuiControlImageButton.StateDefinition();
            definition4.Texture = new MyGuiCompositeTexture(centerTexture);
            style.Normal = definition4;
            Vector2 vector = new Vector2(0.2375f, 0.13f) * 0.7f;
            captionTextColor = null;
            int? buttonIndex = null;
            MyGuiControlImageButton control = new MyGuiControlImageButton("Button", new Vector2((base.Size.Value.X / 2f) - 0.04f, (-base.Size.Value.Y / 2f) - 0.01f), new Vector2?(vector), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, new Action<MyGuiControlImageButton>(this.onBannerClick), null, GuiSounds.MouseClick, 1f, buttonIndex, false) {
                BackgroundTexture = new MyGuiCompositeTexture(centerTexture)
            };
            control.ApplyStyle(style);
            control.CanHaveFocus = false;
            control.UserData = "https://www.gtxgaming.co.uk/server-hosting/space-engineers-server-hosting/";
            control.SetToolTip(MyTexts.GetString(MySpaceTexts.JoinScreen_GTXGamingBanner));
            this.Controls.Add(control);
        }

        private void RefreshFavoritesGameList()
        {
            this.CloseFavoritesRequest();
            this.m_gamesTable.Clear();
            this.AddServerHeaders();
            this.m_textCache.Clear();
            this.m_gameTypeText.Clear();
            this.m_gameTypeToolTip.Clear();
            this.m_favoriteServers.Clear();
            this.m_favoritesPage.Text = new StringBuilder().Append(MyTexts.Get(MyCommonTexts.JoinGame_TabTitle_Favorites));
            MySandboxGame.Log.WriteLine("Requesting dedicated servers");
            MyGameService.OnFavoritesServerListResponded += new EventHandler<int>(this.OnFavoritesServerListResponded);
            MyGameService.OnFavoritesServersCompleteResponse += new EventHandler<MyMatchMakingServerResponse>(this.OnFavoritesServersCompleteResponse);
            string filterOps = $"gamedir:{MyPerGameSettings.SteamGameServerGameDir};secure:1";
            MySandboxGame.Log.WriteLine("Requesting favorite servers, filterOps: " + filterOps);
            MyGameService.RequestFavoritesServerList(filterOps);
            this.m_loadingWheel.Visible = true;
            this.m_gamesTable.SelectedRowIndex = null;
        }

        private void RefreshFriendsGameList()
        {
            this.CloseFriendsRequest();
            this.m_gamesTable.Clear();
            this.AddServerHeaders();
            this.m_textCache.Clear();
            this.m_gameTypeText.Clear();
            this.m_gameTypeToolTip.Clear();
            this.m_friendsPage.Text = new StringBuilder().Append(MyTexts.Get(MyCommonTexts.JoinGame_TabTitle_Friends));
            MySandboxGame.Log.WriteLine("Requesting dedicated servers");
            this.CloseFriendsRequest();
            if (this.FilterOptions.SameVersion)
            {
                MyGameService.AddLobbyFilter("appVersion", MyFinalBuildConstants.APP_VERSION.ToString());
            }
            MyGameService.RequestLobbyList(new Action<bool>(this.FriendsLobbyResponse));
            this.m_dedicatedServers.Clear();
            this.m_refreshButton.Text = MyTexts.GetString(MyCommonTexts.Refresh);
            this.m_nextState = RefreshStateEnum.Pause;
            this.m_refreshPaused = false;
            string filterOps = $"gamedir:{MyPerGameSettings.SteamGameServerGameDir};secure:1";
            if (this.FilterOptions.SameVersion)
            {
                filterOps = filterOps + ";gamedataand:" + MyFinalBuildConstants.APP_VERSION;
            }
            MySandboxGame.Log.WriteLine("Requesting dedicated servers, filterOps: " + filterOps);
            MyGameService.OnDedicatedServerListResponded += new EventHandler<int>(this.OnFriendsServerListResponded);
            MyGameService.OnDedicatedServersCompleteResponse += new EventHandler<MyMatchMakingServerResponse>(this.OnFriendsServersCompleteResponse);
            MyGameService.RequestInternetServerList(filterOps);
            this.m_loadingWheel.Visible = true;
            this.m_loadingWheel.Visible = true;
            this.m_gamesTable.SelectedRowIndex = null;
        }

        private void RefreshGameList()
        {
            int num;
            int num2;
            int? nullable4;
            this.m_gamesTable.Clear();
            this.AddHeaders();
            this.m_textCache.Clear();
            this.m_gameTypeText.Clear();
            this.m_gameTypeToolTip.Clear();
            this.m_lobbyPage.Text = MyTexts.Get(MyCommonTexts.JoinGame_TabTitle_Lobbies);
            if (this.m_lobbies == null)
            {
                goto TR_0000;
            }
            else
            {
                num = 0;
                num2 = 0;
            }
            goto TR_0049;
        TR_0000:
            nullable4 = null;
            this.m_gamesTable.SelectedRowIndex = nullable4;
            return;
        TR_0049:
            while (true)
            {
                if (num2 < this.m_lobbies.Count)
                {
                    IMyLobby userData = this.m_lobbies[num2];
                    MyGuiControlTable.Row row = new MyGuiControlTable.Row(userData);
                    if (!this.FilterOptions.AdvancedFilter || this.FilterOptions.FilterLobby(userData))
                    {
                        string lobbyWorldName = MyMultiplayerLobby.GetLobbyWorldName(userData);
                        MyMultiplayerLobby.GetLobbyWorldSize(userData);
                        int lobbyAppVersion = MyMultiplayerLobby.GetLobbyAppVersion(userData);
                        int lobbyModCount = MyMultiplayerLobby.GetLobbyModCount(userData);
                        string str2 = null;
                        float? nullable = null;
                        string str3 = this.m_searchBox.SearchText.Trim();
                        if (string.IsNullOrWhiteSpace(str3) || lobbyWorldName.ToLower().Contains(str3.ToLower()))
                        {
                            this.m_gameTypeText.Clear();
                            this.m_gameTypeToolTip.Clear();
                            float num5 = MyMultiplayerLobby.GetLobbyFloat("inventoryMultiplier", userData, 1f);
                            float num6 = MyMultiplayerLobby.GetLobbyFloat("refineryMultiplier", userData, 1f);
                            float num7 = MyMultiplayerLobby.GetLobbyFloat("assemblerMultiplier", userData, 1f);
                            float num8 = MyMultiplayerLobby.GetLobbyFloat("blocksInventoryMultiplier", userData, 1f);
                            MyGameModeEnum lobbyGameMode = MyMultiplayerLobby.GetLobbyGameMode(userData);
                            if (MyMultiplayerLobby.GetLobbyScenario(userData))
                            {
                                this.m_gameTypeText.AppendStringBuilder(MyTexts.Get(MySpaceTexts.WorldSettings_GameScenario));
                                DateTime time = MyMultiplayerLobby.GetLobbyDateTime("scenarioStartTime", userData, DateTime.MinValue);
                                if (time <= DateTime.MinValue)
                                {
                                    this.m_gameTypeText.Append(" Lobby");
                                }
                                else
                                {
                                    TimeSpan span = (TimeSpan) (DateTime.UtcNow - time);
                                    double num12 = Math.Truncate(span.TotalHours);
                                    int num13 = (int) ((span.TotalHours - num12) * 60.0);
                                    this.m_gameTypeText.Append(" ").Append(num12).Append(":").Append(num13.ToString("D2"));
                                }
                            }
                            else if (lobbyGameMode == MyGameModeEnum.Creative)
                            {
                                if (!this.FilterOptions.CreativeMode)
                                {
                                    break;
                                }
                                this.m_gameTypeText.AppendStringBuilder(MyTexts.Get(MyCommonTexts.WorldSettings_GameModeCreative));
                            }
                            else if (lobbyGameMode == MyGameModeEnum.Survival)
                            {
                                if (!this.FilterOptions.SurvivalMode)
                                {
                                    break;
                                }
                                this.m_gameTypeText.AppendStringBuilder(MyTexts.Get(MyCommonTexts.WorldSettings_GameModeSurvival));
                                this.m_gameTypeText.Append($" {num5}-{num8}-{num7}-{num6}");
                            }
                            object[] args = new object[] { num5, num8, num7, num6 };
                            this.m_gameTypeToolTip.AppendFormat(MyTexts.Get(MyCommonTexts.JoinGame_GameTypeToolTip_MultipliersFormat).ToString(), args);
                            int lobbyViewDistance = MyMultiplayerLobby.GetLobbyViewDistance(userData);
                            this.m_gameTypeToolTip.AppendLine();
                            this.m_gameTypeToolTip.AppendFormat(MyTexts.Get(MyCommonTexts.JoinGame_GameTypeToolTip_ViewDistance).ToString(), lobbyViewDistance);
                            if ((!string.IsNullOrEmpty(lobbyWorldName) && (!this.FilterOptions.SameVersion || (lobbyAppVersion == MyFinalBuildConstants.APP_VERSION))) && ((!this.FilterOptions.SameData || !MyFakes.ENABLE_MP_DATA_HASHES) || MyMultiplayerLobby.HasSameData(userData)))
                            {
                                string lobbyHostName = MyMultiplayerLobby.GetLobbyHostName(userData);
                                string str5 = userData.MemberLimit.ToString();
                                string str6 = userData.MemberCount + "/" + str5;
                                if (((!this.FilterOptions.CheckDistance || this.FilterOptions.ViewDistance.ValueBetween((float) MyMultiplayerLobby.GetLobbyViewDistance(userData))) && (!this.FilterOptions.CheckPlayer || this.FilterOptions.PlayerCount.ValueBetween((float) userData.MemberCount))) && (!this.FilterOptions.CheckMod || this.FilterOptions.ModCount.ValueBetween((float) lobbyModCount)))
                                {
                                    List<MyObjectBuilder_Checkpoint.ModItem> lobbyMods = MyMultiplayerLobby.GetLobbyMods(userData);
                                    if (((this.FilterOptions.Mods != null) && this.FilterOptions.Mods.Any<ulong>()) && this.FilterOptions.AdvancedFilter)
                                    {
                                        if (!this.FilterOptions.ModsExclusive)
                                        {
                                            if ((lobbyMods == null) || !lobbyMods.Any<MyObjectBuilder_Checkpoint.ModItem>(m => this.FilterOptions.Mods.Contains(m.PublishedFileId)))
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            bool flag = false;
                                            using (HashSet<ulong>.Enumerator enumerator = this.FilterOptions.Mods.GetEnumerator())
                                            {
                                                while (enumerator.MoveNext())
                                                {
                                                    if ((lobbyMods == null) || !lobbyMods.Any<MyObjectBuilder_Checkpoint.ModItem>(delegate (MyObjectBuilder_Checkpoint.ModItem m) {
                                                        ulong modId;
                                                        return (m.PublishedFileId == modId);
                                                    }))
                                                    {
                                                        flag = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (flag)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    StringBuilder builder = new StringBuilder();
                                    int num10 = 15;
                                    int num11 = Math.Min(num10, lobbyModCount - 1);
                                    foreach (MyObjectBuilder_Checkpoint.ModItem item in lobbyMods)
                                    {
                                        num10--;
                                        if (num10 <= 0)
                                        {
                                            builder.Append("...");
                                            break;
                                        }
                                        num11--;
                                        if (num11 <= 0)
                                        {
                                            builder.Append(item.FriendlyName);
                                            continue;
                                        }
                                        builder.AppendLine(item.FriendlyName);
                                    }
                                    Color? textColor = null;
                                    MyGuiHighlightTexture? icon = null;
                                    row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append(lobbyWorldName), userData.LobbyId, this.m_textCache.ToString(), textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                                    textColor = null;
                                    icon = null;
                                    row.AddCell(new MyGuiControlTable.Cell(this.m_gameTypeText, null, (this.m_gameTypeToolTip.Length > 0) ? this.m_gameTypeToolTip.ToString() : null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                                    textColor = null;
                                    icon = null;
                                    row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append(lobbyHostName), null, this.m_textCache.ToString(), textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                                    if (MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME)
                                    {
                                        if (str2 != null)
                                        {
                                            textColor = null;
                                            icon = null;
                                            row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append(str2), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                                        }
                                        else if (nullable != null)
                                        {
                                            row.AddCell(new CellRemainingTime(nullable.Value));
                                        }
                                        else
                                        {
                                            textColor = null;
                                            icon = null;
                                            row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear(), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                                        }
                                    }
                                    textColor = null;
                                    icon = null;
                                    row.AddCell(new MyGuiControlTable.Cell(new StringBuilder(str6), null, null, textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                                    textColor = null;
                                    icon = null;
                                    row.AddCell(new MyGuiControlTable.Cell(this.m_textCache.Clear().Append((lobbyModCount == 0) ? "---" : lobbyModCount.ToString()), null, builder.ToString(), textColor, icon, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP));
                                    this.m_gamesTable.Add(row);
                                    num++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.m_lobbyPage.Text = new StringBuilder().Append(MyTexts.Get(MyCommonTexts.JoinGame_TabTitle_Lobbies)).Append(" (").Append(num).Append(")");
                    goto TR_0000;
                }
                break;
            }
            num2++;
            goto TR_0049;
        }

        private void RefreshHistoryGameList()
        {
            this.CloseHistoryRequest();
            this.m_gamesTable.Clear();
            this.AddServerHeaders();
            this.m_textCache.Clear();
            this.m_gameTypeText.Clear();
            this.m_gameTypeToolTip.Clear();
            this.m_historyServers.Clear();
            this.m_historyPage.Text = new StringBuilder().Append(MyTexts.Get(MyCommonTexts.JoinGame_TabTitle_History));
            MySandboxGame.Log.WriteLine("Requesting dedicated servers");
            MyGameService.OnHistoryServerListResponded += new EventHandler<int>(this.OnHistoryServerListResponded);
            MyGameService.OnHistoryServersCompleteResponse += new EventHandler<MyMatchMakingServerResponse>(this.OnHistoryServersCompleteResponse);
            string filterOps = $"gamedir:{MyPerGameSettings.SteamGameServerGameDir};secure:1";
            MySandboxGame.Log.WriteLine("Requesting history servers, filterOps: " + filterOps);
            MyGameService.RequestHistoryServerList(filterOps);
            this.m_loadingWheel.Visible = true;
            this.m_gamesTable.SelectedRowIndex = null;
        }

        private void RefreshLANGameList()
        {
            this.CloseLANRequest();
            this.m_gamesTable.Clear();
            this.AddServerHeaders();
            this.m_textCache.Clear();
            this.m_gameTypeText.Clear();
            this.m_gameTypeToolTip.Clear();
            this.m_lanServers.Clear();
            this.m_LANPage.Text = new StringBuilder().Append(MyTexts.Get(MyCommonTexts.JoinGame_TabTitle_LAN));
            MySandboxGame.Log.WriteLine("Requesting dedicated servers");
            MyGameService.OnLANServerListResponded += new EventHandler<int>(this.OnLANServerListResponded);
            MyGameService.OnLANServersCompleteResponse += new EventHandler<MyMatchMakingServerResponse>(this.OnLANServersCompleteResponse);
            MyGameService.RequestLANServerList();
            this.m_loadingWheel.Visible = true;
            this.m_gamesTable.SelectedRowIndex = null;
        }

        private void RefreshServerGameList(bool resetSteamQuery)
        {
            if ((this.m_lastVersionCheck != this.FilterOptions.SameVersion) || this.FilterOptions.AdvancedFilter)
            {
                resetSteamQuery = true;
            }
            this.m_lastVersionCheck = this.FilterOptions.SameVersion;
            this.m_detailsButton.Enabled = false;
            this.m_joinButton.Enabled = false;
            this.m_gamesTable.Clear();
            this.AddServerHeaders();
            this.m_textCache.Clear();
            this.m_gameTypeText.Clear();
            this.m_gameTypeToolTip.Clear();
            this.m_serversPage.TextEnum = MyCommonTexts.JoinGame_TabTitle_Servers;
            if (resetSteamQuery)
            {
                this.m_dedicatedServers.Clear();
                this.CloseRequest();
                this.m_refreshButton.Text = MyTexts.GetString(MyCommonTexts.ScreenLoadSubscribedWorldPause);
                this.m_nextState = RefreshStateEnum.Pause;
                this.m_refreshPaused = false;
                string filterOps = $"gamedir:{MyPerGameSettings.SteamGameServerGameDir};secure:1";
                if (this.FilterOptions.SameVersion)
                {
                    filterOps = filterOps + ";gamedataand:" + MyFinalBuildConstants.APP_VERSION;
                }
                MySandboxGame.Log.WriteLine("Requesting dedicated servers, filterOps: " + filterOps);
                MyGameService.OnDedicatedServerListResponded += new EventHandler<int>(this.OnDedicatedServerListResponded);
                MyGameService.OnDedicatedServersCompleteResponse += new EventHandler<MyMatchMakingServerResponse>(this.OnDedicatedServersCompleteResponse);
                MyGameService.RequestInternetServerList(filterOps);
                this.m_loadingWheel.Visible = true;
            }
            this.m_gamesTable.SelectedRowIndex = null;
            this.RebuildServerList();
        }

        public override bool RegisterClicks() => 
            true;

        public void RemoveFavoriteServer(MyCachedServerItem server)
        {
            this.m_favoriteServers.Remove(server);
            this.refresh_favorites = true;
        }

        private void RequestFriendsList()
        {
            DateTime now = DateTime.Now;
            int friendsCount = MyGameService.GetFriendsCount();
            MyLog.Default.WriteLine("Got friends: " + friendsCount);
            for (int i = 0; i < friendsCount; i++)
            {
                ulong friendIdByIndex = MyGameService.GetFriendIdByIndex(i);
                string friendNameByIndex = MyGameService.GetFriendNameByIndex(i);
                object[] objArray1 = new object[] { "Got Friend ", i, " : ", friendIdByIndex, " : ", friendNameByIndex };
                MyLog.Default.WriteLine(string.Concat(objArray1));
                this.m_friendIds.Add(friendIdByIndex);
                this.m_friendNames.Add(friendNameByIndex);
            }
            MyLog.Default.WriteLine("Completed friends request: " + (DateTime.Now - now).TotalMilliseconds);
        }

        private void ServerDetailsClick(MyGuiControlButton detailButton)
        {
            if (this.m_gamesTable.SelectedRow != null)
            {
                MyGameServerItem ser = this.m_gamesTable.SelectedRow.UserData as MyGameServerItem;
                if (ser != null)
                {
                    MyCachedServerItem server = (detailButton.UserData as HashSet<MyCachedServerItem>).FirstOrDefault<MyCachedServerItem>(x => x.Server.NetAdr.Equals(ser.NetAdr));
                    if (server != null)
                    {
                        this.m_loadingWheel.Visible = false;
                        MyGuiSandbox.AddScreen(new MyGuiScreenServerDetailsSpace(server));
                    }
                }
            }
        }

        private int TextComparison(MyGuiControlTable.Cell a, MyGuiControlTable.Cell b)
        {
            ulong gameID;
            ulong gameID;
            int num = a.Text.CompareToIgnoreCase(b.Text);
            if (num != 0)
            {
                return num;
            }
            IMyMultiplayerGame userData = a.Row.UserData as IMyMultiplayerGame;
            if (userData != null)
            {
                gameID = userData.GameID;
            }
            else
            {
                MySteamLobby lobby = a.Row.UserData as MySteamLobby;
                if (lobby == null)
                {
                    return 0;
                }
                gameID = lobby.LobbyId;
            }
            IMyMultiplayerGame game2 = b.Row.UserData as IMyMultiplayerGame;
            if (game2 != null)
            {
                gameID = game2.GameID;
            }
            else
            {
                MySteamLobby lobby2 = b.Row.UserData as MySteamLobby;
                if (lobby2 == null)
                {
                    return 0;
                }
                gameID = lobby2.LobbyId;
            }
            return gameID.CompareTo(gameID);
        }

        public override bool Update(bool hasFocus)
        {
            if (this.refresh_favorites & hasFocus)
            {
                this.refresh_favorites = false;
                this.m_joinButton.Enabled = false;
                this.m_detailsButton.Enabled = false;
                this.RebuildFavoritesList();
            }
            if (this.m_searchChanged && (DateTime.Now.Subtract(this.m_searchLastChanged).Milliseconds > 500))
            {
                this.m_searchChanged = false;
                this.m_searchChangedFunc();
            }
            if (MyFakes.ENABLE_JOIN_SCREEN_REMAINING_TIME)
            {
                this.m_remainingTimeUpdateFrame++;
                if ((this.m_remainingTimeUpdateFrame % 50) == 0)
                {
                    int index = 0;
                    while (true)
                    {
                        if (index >= this.m_gamesTable.RowsCount)
                        {
                            this.m_remainingTimeUpdateFrame = 0;
                            break;
                        }
                        this.m_gamesTable.GetRow(index).Update();
                        index++;
                    }
                }
            }
            return base.Update(hasFocus);
        }

        private class CellRemainingTime : MyGuiControlTable.Cell
        {
            private readonly DateTime m_timeEstimatedEnd;

            public CellRemainingTime(float remainingTime) : base("", null, null, nullable, nullable2, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP)
            {
                this.m_timeEstimatedEnd = DateTime.UtcNow + TimeSpan.FromSeconds((double) remainingTime);
                this.FillText();
            }

            private void FillText()
            {
                TimeSpan zero = (TimeSpan) (this.m_timeEstimatedEnd - DateTime.UtcNow);
                if (zero < TimeSpan.Zero)
                {
                    zero = TimeSpan.Zero;
                }
                base.Text.Clear().Append(zero.ToString(@"mm\:ss"));
            }

            public override void Update()
            {
                base.Update();
                this.FillText();
            }
        }

        private enum ContextMenuFavoriteAction
        {
            Add,
            Remove
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ContextMenuFavoriteActionItem
        {
            public MyGameServerItem Server;
            public MyGuiScreenJoinGame.ContextMenuFavoriteAction _Action;
        }

        private enum RefreshStateEnum
        {
            Pause,
            Resume,
            Refresh
        }
    }
}

