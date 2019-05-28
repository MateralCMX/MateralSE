namespace Sandbox.Engine.Multiplayer
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Utils;

    public sealed class MyMultiplayerLobby : MyMultiplayerServerBase
    {
        private IMyLobby m_lobby;
        private bool m_serverDataValid;
        [CompilerGenerated]
        private MyLobbyDataUpdated OnLobbyDataUpdated;

        public event MyLobbyDataUpdated OnLobbyDataUpdated
        {
            [CompilerGenerated] add
            {
                MyLobbyDataUpdated onLobbyDataUpdated = this.OnLobbyDataUpdated;
                while (true)
                {
                    MyLobbyDataUpdated a = onLobbyDataUpdated;
                    MyLobbyDataUpdated updated3 = (MyLobbyDataUpdated) Delegate.Combine(a, value);
                    onLobbyDataUpdated = Interlocked.CompareExchange<MyLobbyDataUpdated>(ref this.OnLobbyDataUpdated, updated3, a);
                    if (ReferenceEquals(onLobbyDataUpdated, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MyLobbyDataUpdated onLobbyDataUpdated = this.OnLobbyDataUpdated;
                while (true)
                {
                    MyLobbyDataUpdated source = onLobbyDataUpdated;
                    MyLobbyDataUpdated updated3 = (MyLobbyDataUpdated) Delegate.Remove(source, value);
                    onLobbyDataUpdated = Interlocked.CompareExchange<MyLobbyDataUpdated>(ref this.OnLobbyDataUpdated, updated3, source);
                    if (ReferenceEquals(onLobbyDataUpdated, source))
                    {
                        return;
                    }
                }
            }
        }

        internal MyMultiplayerLobby(IMyLobby lobby, MySyncLayer syncLayer) : base(syncLayer, new EndpointId(Sync.MyId))
        {
            this.m_lobby = lobby;
            base.ServerId = this.m_lobby.OwnerId;
            base.SyncLayer.RegisterClientEvents(this);
            this.HostName = MyGameService.UserName;
            lobby.OnChatUpdated += new MyLobbyChatUpdated(this.Matchmaking_LobbyChatUpdate);
            lobby.OnChatReceived += new MessageReceivedDelegate(this.Matchmaking_LobbyChatMsg);
            lobby.OnChatScriptedReceived += new MessageScriptedReceivedDelegate(this.Matchmaking_LobbyChatScriptedMsg);
            lobby.OnDataReceived += new MyLobbyDataUpdated(this.lobby_OnDataReceived);
            base.ClientLeft += new Action<ulong, MyChatMemberStateChangeEnum>(this.MyMultiplayerLobby_ClientLeft);
            this.AcceptMemberSessions();
        }

        private void AcceptMemberSessions()
        {
            for (int i = 0; i < this.m_lobby.MemberCount; i++)
            {
                ulong memberByIndex = this.m_lobby.GetMemberByIndex(i);
                if ((memberByIndex != Sync.MyId) && (memberByIndex == base.ServerId))
                {
                    MyGameService.Peer2Peer.AcceptSession(memberByIndex);
                }
            }
        }

        public override void BanClient(ulong userId, bool banned)
        {
        }

        public override void DisconnectClient(ulong userId)
        {
            MyLog.Default.WriteLineAndConsole("User forcibly disconnected " + this.GetMemberName(userId));
            base.RaiseClientLeft(userId, MyChatMemberStateChangeEnum.Disconnected);
        }

        public override void Dispose()
        {
            this.m_lobby.OnChatUpdated -= new MyLobbyChatUpdated(this.Matchmaking_LobbyChatUpdate);
            this.m_lobby.OnChatScriptedReceived -= new MessageScriptedReceivedDelegate(this.Matchmaking_LobbyChatScriptedMsg);
            this.m_lobby.OnChatReceived -= new MessageReceivedDelegate(this.Matchmaking_LobbyChatMsg);
            if (this.m_lobby.IsValid)
            {
                base.CloseMemberSessions();
                this.m_lobby.Leave();
            }
            base.Dispose();
        }

        private void GetChatMessage(int chatMsgID, out string messageText)
        {
            ulong num;
            this.m_lobby.GetChatMessage(chatMsgID, out messageText, out num);
        }

        public static string GetDataHash(IMyLobby lobby) => 
            lobby.GetData("dataHash");

        public static int GetLobbyAppVersion(IMyLobby lobby)
        {
            int num;
            return (int.TryParse(lobby.GetData("appVersion"), out num) ? num : 0);
        }

        public static bool GetLobbyBool(string key, IMyLobby lobby, bool defValue)
        {
            bool flag;
            return (!bool.TryParse(lobby.GetData(key), out flag) ? defValue : flag);
        }

        public static DateTime GetLobbyDateTime(string key, IMyLobby lobby, DateTime defValue)
        {
            DateTime time;
            return (!DateTime.TryParse(lobby.GetData(key), CultureInfo.InvariantCulture, DateTimeStyles.None, out time) ? defValue : time);
        }

        public static float GetLobbyFloat(string key, IMyLobby lobby, float defValue)
        {
            float num;
            return (!float.TryParse(lobby.GetData(key), NumberStyles.Float, CultureInfo.InvariantCulture, out num) ? defValue : num);
        }

        public static MyGameModeEnum GetLobbyGameMode(IMyLobby lobby)
        {
            int num;
            return (!int.TryParse(lobby.GetData("gameMode"), out num) ? MyGameModeEnum.Creative : ((MyGameModeEnum) num));
        }

        public static string GetLobbyHostName(IMyLobby lobby) => 
            lobby.GetData("host");

        public static ulong GetLobbyHostSteamId(IMyLobby lobby) => 
            GetLobbyULong("host_steamId", lobby, 0UL);

        public static int GetLobbyInt(string key, IMyLobby lobby, int defValue)
        {
            int num;
            return (!int.TryParse(lobby.GetData(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out num) ? defValue : num);
        }

        public static long GetLobbyLong(string key, IMyLobby lobby, long defValue)
        {
            long num;
            return (!long.TryParse(lobby.GetData(key), out num) ? defValue : num);
        }

        public static int GetLobbyModCount(IMyLobby lobby) => 
            GetLobbyInt("mods", lobby, 0);

        public static List<MyObjectBuilder_Checkpoint.ModItem> GetLobbyMods(IMyLobby lobby)
        {
            int lobbyModCount = GetLobbyModCount(lobby);
            List<MyObjectBuilder_Checkpoint.ModItem> list = new List<MyObjectBuilder_Checkpoint.ModItem>(lobbyModCount);
            for (int i = 0; i < lobbyModCount; i++)
            {
                string data = lobby.GetData("mod" + i);
                int index = data.IndexOf("_");
                if (index == -1)
                {
                    MySandboxGame.Log.WriteLineAndConsole($"Failed to parse mod details from LobbyData. '{data}'");
                }
                else
                {
                    ulong num4;
                    ulong.TryParse(data.Substring(0, index), out num4);
                    string name = data.Substring(index + 1);
                    list.Add(new MyObjectBuilder_Checkpoint.ModItem(name, num4, name));
                }
            }
            return list;
        }

        public static bool GetLobbyScenario(IMyLobby lobby) => 
            GetLobbyBool("scenario", lobby, false);

        public static string GetLobbyScenarioBriefing(IMyLobby lobby) => 
            lobby.GetData("scenarioBriefing");

        public override MyLobbyType GetLobbyType() => 
            this.m_lobby.LobbyType;

        public static ulong GetLobbyULong(string key, IMyLobby lobby, ulong defValue)
        {
            ulong num;
            return (!ulong.TryParse(lobby.GetData(key), out num) ? defValue : num);
        }

        public static int GetLobbyViewDistance(IMyLobby lobby) => 
            GetLobbyInt("view", lobby, 0x4e20);

        public static string GetLobbyWorldName(IMyLobby lobby) => 
            lobby.GetData("world");

        public static ulong GetLobbyWorldSize(IMyLobby lobby)
        {
            string data = lobby.GetData("worldSize");
            return (string.IsNullOrEmpty(data) ? 0UL : Convert.ToUInt64(data));
        }

        public override ulong GetMemberByIndex(int memberIndex) => 
            this.m_lobby.GetMemberByIndex(memberIndex);

        public override string GetMemberName(ulong steamUserID) => 
            MyGameService.GetPersonaName(steamUserID);

        public override ulong GetOwner() => 
            this.m_lobby.OwnerId;

        public static bool HasSameData(IMyLobby lobby)
        {
            string dataHash = GetDataHash(lobby);
            return ((dataHash != "") ? (dataHash == MyDataIntegrityChecker.GetHashBase64()) : true);
        }

        public bool IsCorrectVersion() => 
            IsLobbyCorrectVersion(this.m_lobby);

        public static bool IsLobbyCorrectVersion(IMyLobby lobby) => 
            (GetLobbyAppVersion(lobby) == MyFinalBuildConstants.APP_VERSION);

        private void lobby_OnDataReceived(bool success, IMyLobby lobby, ulong memberOrLobby)
        {
            MyLobbyDataUpdated onLobbyDataUpdated = this.OnLobbyDataUpdated;
            if (onLobbyDataUpdated != null)
            {
                onLobbyDataUpdated(success, lobby, memberOrLobby);
            }
        }

        private void Matchmaking_LobbyChatMsg(ulong memberId, string message, byte channel, long targetId)
        {
            base.RaiseChatMessageReceived(memberId, message, (ChatChannel) channel, targetId, null);
        }

        private void Matchmaking_LobbyChatScriptedMsg(ulong memberId, string message, byte channel, long targetId, string author)
        {
            base.RaiseChatMessageReceived(memberId, message, (ChatChannel) channel, targetId, author);
        }

        private void Matchmaking_LobbyChatUpdate(IMyLobby lobby, ulong changedUser, ulong makingChangeUser, MyChatMemberStateChangeEnum stateChange)
        {
            if (lobby.LobbyId == this.m_lobby.LobbyId)
            {
                if (stateChange == MyChatMemberStateChangeEnum.Entered)
                {
                    object[] objArray1 = new object[] { "Player entered: ", MyGameService.GetPersonaName(changedUser), " (", changedUser, ")" };
                    MySandboxGame.Log.WriteLineAndConsole(string.Concat(objArray1));
                    MyGameService.Peer2Peer.AcceptSession(changedUser);
                    if ((Sync.Clients == null) || !Sync.Clients.HasClient(changedUser))
                    {
                        base.RaiseClientJoined(changedUser);
                        if (this.Scenario && (changedUser != Sync.MyId))
                        {
                            base.SendAllMembersDataToClient(changedUser);
                        }
                    }
                    if (MySandboxGame.IsGameReady && (changedUser != base.ServerId))
                    {
                        MyHudNotification notification = new MyHudNotification(MyCommonTexts.NotificationClientConnected, 0x1388, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
                        object[] arguments = new object[] { MyGameService.GetPersonaName(changedUser) };
                        notification.SetTextFormatArguments(arguments);
                        MyHud.Notifications.Add(notification);
                    }
                }
                else
                {
                    if ((Sync.Clients == null) || Sync.Clients.HasClient(changedUser))
                    {
                        base.RaiseClientLeft(changedUser, stateChange);
                    }
                    if (changedUser == base.ServerId)
                    {
                        base.RaiseHostLeft();
                        MySessionLoader.UnloadAndExitToMenu();
                        StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                        MyStringId? okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        Vector2? size = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MultiplayerErrorServerHasLeft), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                    }
                    else if (MySandboxGame.IsGameReady)
                    {
                        MyHudNotification notification = new MyHudNotification(MyCommonTexts.NotificationClientDisconnected, 0x1388, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
                        object[] arguments = new object[] { MyGameService.GetPersonaName(changedUser) };
                        notification.SetTextFormatArguments(arguments);
                        MyHud.Notifications.Add(notification);
                    }
                }
            }
        }

        private void MyMultiplayerLobby_ClientLeft(ulong userId, MyChatMemberStateChangeEnum stateChange)
        {
            if (userId == base.ServerId)
            {
                MyGameService.Peer2Peer.CloseSession(userId);
            }
            if ((stateChange == MyChatMemberStateChangeEnum.Kicked) || (stateChange == MyChatMemberStateChangeEnum.Banned))
            {
                MyControlKickClientMsg message = new MyControlKickClientMsg {
                    KickedClient = userId,
                    Kicked = true,
                    Add = false
                };
                MyLog.Default.WriteLineAndConsole("Player " + this.GetMemberName(userId) + " kicked");
                base.SendControlMessageToAll<MyControlKickClientMsg>(ref message, 0UL);
            }
            object[] objArray1 = new object[] { "Player left: ", this.GetMemberName(userId), " (", userId, ")" };
            MySandboxGame.Log.WriteLineAndConsole(string.Concat(objArray1));
        }

        protected override void OnClientBan(ref MyControlBanClientMsg data, ulong sender)
        {
        }

        public override void SendChatMessage(string text, ChatChannel channel, long targetId = 0L)
        {
            this.m_lobby.SendChatMessage(text, (byte) channel, targetId);
        }

        public override void SendChatMessageScripted(string text, ChatChannel channel, long targetId = 0L, string customAuthor = null)
        {
            this.m_lobby.SendChatMessageScripted(text, (byte) channel, targetId, customAuthor);
        }

        public override void SetLobbyType(MyLobbyType type)
        {
            this.m_lobby.LobbyType = type;
        }

        public override void SetMemberLimit(int limit)
        {
            this.m_lobby.MemberLimit = limit;
        }

        public override void SetOwner(ulong owner)
        {
            this.m_lobby.OwnerId = owner;
        }

        public override void Tick()
        {
            base.Tick();
            if (!this.m_serverDataValid)
            {
                if (this.AppVersion == 0)
                {
                    MySession.Static.StartServer(this);
                }
                this.m_serverDataValid = true;
            }
        }

        protected override bool IsServerInternal =>
            true;

        public override string WorldName
        {
            get => 
                GetLobbyWorldName(this.m_lobby);
            set => 
                this.m_lobby.SetData("world", value ?? "Unnamed");
        }

        public override MyGameModeEnum GameMode
        {
            get => 
                GetLobbyGameMode(this.m_lobby);
            set => 
                this.m_lobby.SetData("gameMode", ((int) value).ToString());
        }

        public override float InventoryMultiplier
        {
            get => 
                GetLobbyFloat("inventoryMultiplier", this.m_lobby, 1f);
            set => 
                this.m_lobby.SetData("inventoryMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override float BlocksInventoryMultiplier
        {
            get => 
                GetLobbyFloat("blocksInventoryMultiplier", this.m_lobby, 1f);
            set => 
                this.m_lobby.SetData("blocksInventoryMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override float AssemblerMultiplier
        {
            get => 
                GetLobbyFloat("assemblerMultiplier", this.m_lobby, 1f);
            set => 
                this.m_lobby.SetData("assemblerMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override float RefineryMultiplier
        {
            get => 
                GetLobbyFloat("refineryMultiplier", this.m_lobby, 1f);
            set => 
                this.m_lobby.SetData("refineryMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override float WelderMultiplier
        {
            get => 
                GetLobbyFloat("welderMultiplier", this.m_lobby, 1f);
            set => 
                this.m_lobby.SetData("welderMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override float GrinderMultiplier
        {
            get => 
                GetLobbyFloat("grinderMultiplier", this.m_lobby, 1f);
            set => 
                this.m_lobby.SetData("grinderMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override string HostName
        {
            get => 
                GetLobbyHostName(this.m_lobby);
            set => 
                this.m_lobby.SetData("host", value);
        }

        public override ulong WorldSize
        {
            get => 
                GetLobbyWorldSize(this.m_lobby);
            set => 
                this.m_lobby.SetData("worldSize", value.ToString());
        }

        public override int AppVersion
        {
            get => 
                GetLobbyAppVersion(this.m_lobby);
            set => 
                this.m_lobby.SetData("appVersion", value.ToString());
        }

        public override string DataHash
        {
            get => 
                this.m_lobby.GetData("dataHash");
            set => 
                this.m_lobby.SetData("dataHash", value);
        }

        public static int MAX_PLAYERS =>
            (MySandboxGame.Config.ExperimentalMode ? 0x10 : 8);

        public override int MaxPlayers =>
            MAX_PLAYERS;

        public override int ModCount
        {
            get => 
                GetLobbyModCount(this.m_lobby);
            protected set => 
                this.m_lobby.SetData("mods", value.ToString());
        }

        public override List<MyObjectBuilder_Checkpoint.ModItem> Mods
        {
            get => 
                GetLobbyMods(this.m_lobby);
            set
            {
                this.ModCount = value.Count;
                int num = this.ModCount - 1;
                foreach (MyObjectBuilder_Checkpoint.ModItem item in value)
                {
                    string str = item.PublishedFileId + "_" + item.FriendlyName;
                    num--;
                    this.m_lobby.SetData("mod" + num, str);
                }
            }
        }

        public override int ViewDistance
        {
            get => 
                GetLobbyViewDistance(this.m_lobby);
            set => 
                this.m_lobby.SetData("view", value.ToString());
        }

        public override int SyncDistance
        {
            get => 
                MyLayers.GetSyncDistance();
            set => 
                MyLayers.SetSyncDistance(value);
        }

        public override bool Scenario
        {
            get => 
                GetLobbyBool("scenario", this.m_lobby, false);
            set => 
                this.m_lobby.SetData("scenario", value.ToString());
        }

        public override string ScenarioBriefing
        {
            get => 
                this.m_lobby.GetData("scenarioBriefing");
            set
            {
                string text1;
                if ((value == null) || (value.Length < 1))
                {
                    text1 = " ";
                }
                else
                {
                    text1 = value;
                }
                this.m_lobby.SetData("scenarioBriefing", text1);
            }
        }

        public override DateTime ScenarioStartTime
        {
            get => 
                GetLobbyDateTime("scenarioStartTime", this.m_lobby, DateTime.MinValue);
            set => 
                this.m_lobby.SetData("scenarioStartTime", value.ToString(CultureInfo.InvariantCulture));
        }

        public ulong HostSteamId
        {
            get => 
                GetLobbyULong("host_steamId", this.m_lobby, 0UL);
            set => 
                this.m_lobby.SetData("host_steamId", value.ToString());
        }

        public override bool ExperimentalMode
        {
            get => 
                GetLobbyBool("experimentalMode", this.m_lobby, false);
            set => 
                this.m_lobby.SetData("experimentalMode", value.ToString());
        }

        public override IEnumerable<ulong> Members =>
            this.m_lobby.MemberList;

        public override int MemberCount =>
            this.m_lobby.MemberCount;

        public override ulong LobbyId =>
            this.m_lobby.LobbyId;

        public override int MemberLimit
        {
            get => 
                this.m_lobby.MemberLimit;
            set
            {
            }
        }
    }
}

