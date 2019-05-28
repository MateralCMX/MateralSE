namespace Sandbox.Engine.Multiplayer
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Utils;

    public sealed class MyMultiplayerLobbyClient : MyMultiplayerClientBase
    {
        public readonly IMyLobby Lobby;
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

        internal MyMultiplayerLobbyClient(IMyLobby lobby, MySyncLayer syncLayer) : base(syncLayer)
        {
            this.Lobby = lobby;
            base.ServerId = this.Lobby.OwnerId;
            base.SyncLayer.RegisterClientEvents(this);
            base.SyncLayer.TransportLayer.IsBuffering = true;
            if (!base.SyncLayer.Clients.HasClient(base.ServerId))
            {
                base.SyncLayer.Clients.AddClient(base.ServerId);
            }
            base.ClientLeft += new Action<ulong, MyChatMemberStateChangeEnum>(this.MyMultiplayerLobby_ClientLeft);
            lobby.OnChatUpdated += new MyLobbyChatUpdated(this.Matchmaking_LobbyChatUpdate);
            lobby.OnChatReceived += new MessageReceivedDelegate(this.Matchmaking_LobbyChatMsg);
            lobby.OnChatScriptedReceived += new MessageScriptedReceivedDelegate(this.Matchmaking_LobbyChatScriptedMsg);
            lobby.OnDataReceived += new MyLobbyDataUpdated(this.lobby_OnDataReceived);
            this.AcceptMemberSessions();
        }

        private void AcceptMemberSessions()
        {
            for (int i = 0; i < this.Lobby.MemberCount; i++)
            {
                ulong memberByIndex = this.Lobby.GetMemberByIndex(i);
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
            base.RaiseClientLeft(userId, MyChatMemberStateChangeEnum.Disconnected);
        }

        public override void Dispose()
        {
            this.Lobby.OnChatUpdated -= new MyLobbyChatUpdated(this.Matchmaking_LobbyChatUpdate);
            this.Lobby.OnChatScriptedReceived += new MessageScriptedReceivedDelegate(this.Matchmaking_LobbyChatScriptedMsg);
            this.Lobby.OnChatReceived -= new MessageReceivedDelegate(this.Matchmaking_LobbyChatMsg);
            if (this.Lobby.IsValid)
            {
                base.CloseMemberSessions();
                this.Lobby.Leave();
            }
            base.Dispose();
        }

        private void GetChatMessage(int chatMsgID, out string messageText)
        {
            ulong num;
            this.Lobby.GetChatMessage(chatMsgID, out messageText, out num);
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
            this.Lobby.LobbyType;

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
            this.Lobby.GetMemberByIndex(memberIndex);

        public override string GetMemberName(ulong steamUserID) => 
            MyGameService.GetPersonaName(steamUserID);

        public override ulong GetOwner() => 
            this.Lobby.OwnerId;

        public static bool HasSameData(IMyLobby lobby)
        {
            string dataHash = GetDataHash(lobby);
            return ((dataHash != "") ? (dataHash == MyDataIntegrityChecker.GetHashBase64()) : true);
        }

        public bool IsCorrectVersion() => 
            IsLobbyCorrectVersion(this.Lobby);

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
            if (lobby.LobbyId == this.Lobby.LobbyId)
            {
                if (stateChange != MyChatMemberStateChangeEnum.Entered)
                {
                    if ((Sync.Clients == null) || Sync.Clients.HasClient(changedUser))
                    {
                        base.RaiseClientLeft(changedUser, stateChange);
                    }
                    if (changedUser == base.ServerId)
                    {
                        base.RaiseHostLeft();
                        MySessionLoader.UnloadAndExitToMenu();
                        MyGuiScreenServerReconnector.ReconnectToLastSession();
                    }
                    else if (MySandboxGame.IsGameReady)
                    {
                        MyHudNotification notification = new MyHudNotification(MyCommonTexts.NotificationClientDisconnected, 0x1388, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
                        object[] arguments = new object[] { MyGameService.GetPersonaName(changedUser) };
                        notification.SetTextFormatArguments(arguments);
                        MyHud.Notifications.Add(notification);
                    }
                }
                else
                {
                    object[] objArray1 = new object[] { "Player entered: ", MyGameService.GetPersonaName(changedUser), " (", changedUser, ")" };
                    MySandboxGame.Log.WriteLineAndConsole(string.Concat(objArray1));
                    MyGameService.Peer2Peer.AcceptSession(changedUser);
                    if ((Sync.Clients == null) || !Sync.Clients.HasClient(changedUser))
                    {
                        base.RaiseClientJoined(changedUser);
                    }
                    if (MySandboxGame.IsGameReady && (changedUser != base.ServerId))
                    {
                        MyHudNotification notification = new MyHudNotification(MyCommonTexts.NotificationClientConnected, 0x1388, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
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
            object[] objArray1 = new object[] { "Player left: ", this.GetMemberName(userId), " (", userId, ")" };
            MySandboxGame.Log.WriteLineAndConsole(string.Concat(objArray1));
        }

        protected override void OnAllMembersData(ref AllMembersDataMsg msg)
        {
            bool isServer = Sync.IsServer;
            base.ProcessAllMembersData(ref msg);
        }

        protected override void OnClientBan(ref MyControlBanClientMsg data, ulong sender)
        {
        }

        public override void SendChatMessage(string text, ChatChannel channel, long targetId = 0L)
        {
            this.Lobby.SendChatMessage(text, (byte) channel, targetId);
        }

        public override unsafe void SendChatMessageScripted(string text, ChatChannel channel, long targetId = 0L, string customAuthor = null)
        {
            ChatMsg* msgPtr1;
            ChatMsg msg2 = new ChatMsg {
                Text = "This is god speaking! You are not allowed to send scripted messages from client so your message will be suppressed.",
                Author = Sync.MyId,
                Channel = 1,
                TargetId = MySession.Static.LocalPlayerId
            };
            msgPtr1->CustomAuthorName = string.IsNullOrEmpty(customAuthor) ? MyTexts.GetString(MySpaceTexts.ChatBotName) : customAuthor;
            msgPtr1 = (ChatMsg*) ref msg2;
            ChatMsg msg = msg2;
            this.OnChatMessage(ref msg);
        }

        public override void SetLobbyType(MyLobbyType type)
        {
            this.Lobby.LobbyType = type;
        }

        public override void SetMemberLimit(int limit)
        {
            this.Lobby.MemberLimit = limit;
        }

        public override void SetOwner(ulong owner)
        {
            this.Lobby.OwnerId = owner;
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
            false;

        public override string WorldName
        {
            get => 
                GetLobbyWorldName(this.Lobby);
            set => 
                this.Lobby.SetData("world", value);
        }

        public override MyGameModeEnum GameMode
        {
            get => 
                GetLobbyGameMode(this.Lobby);
            set => 
                this.Lobby.SetData("gameMode", ((int) value).ToString());
        }

        public override float InventoryMultiplier
        {
            get => 
                GetLobbyFloat("inventoryMultiplier", this.Lobby, 1f);
            set => 
                this.Lobby.SetData("inventoryMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override float BlocksInventoryMultiplier
        {
            get => 
                GetLobbyFloat("blocksInventoryMultiplier", this.Lobby, 1f);
            set => 
                this.Lobby.SetData("blocksInventoryMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override float AssemblerMultiplier
        {
            get => 
                GetLobbyFloat("assemblerMultiplier", this.Lobby, 1f);
            set => 
                this.Lobby.SetData("assemblerMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override float RefineryMultiplier
        {
            get => 
                GetLobbyFloat("refineryMultiplier", this.Lobby, 1f);
            set => 
                this.Lobby.SetData("refineryMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override float WelderMultiplier
        {
            get => 
                GetLobbyFloat("welderMultiplier", this.Lobby, 1f);
            set => 
                this.Lobby.SetData("welderMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override float GrinderMultiplier
        {
            get => 
                GetLobbyFloat("grinderMultiplier", this.Lobby, 1f);
            set => 
                this.Lobby.SetData("grinderMultiplier", value.ToString(CultureInfo.InvariantCulture));
        }

        public override string HostName
        {
            get => 
                GetLobbyHostName(this.Lobby);
            set => 
                this.Lobby.SetData("host", value);
        }

        public override ulong WorldSize
        {
            get => 
                GetLobbyWorldSize(this.Lobby);
            set => 
                this.Lobby.SetData("worldSize", value.ToString());
        }

        public override int AppVersion
        {
            get => 
                GetLobbyAppVersion(this.Lobby);
            set => 
                this.Lobby.SetData("appVersion", value.ToString());
        }

        public override string DataHash
        {
            get => 
                this.Lobby.GetData("dataHash");
            set => 
                this.Lobby.SetData("dataHash", value);
        }

        public override int MaxPlayers =>
            MyMultiplayerLobby.MAX_PLAYERS;

        public override int ModCount
        {
            get => 
                GetLobbyModCount(this.Lobby);
            protected set => 
                this.Lobby.SetData("mods", value.ToString());
        }

        public override List<MyObjectBuilder_Checkpoint.ModItem> Mods
        {
            get => 
                GetLobbyMods(this.Lobby);
            set
            {
                this.ModCount = value.Count;
                int num = this.ModCount - 1;
                foreach (MyObjectBuilder_Checkpoint.ModItem item in value)
                {
                    string str = item.PublishedFileId + "_" + item.FriendlyName;
                    num--;
                    this.Lobby.SetData("mod" + num, str);
                }
            }
        }

        public override int ViewDistance
        {
            get => 
                GetLobbyViewDistance(this.Lobby);
            set => 
                this.Lobby.SetData("view", value.ToString());
        }

        public override bool Scenario
        {
            get => 
                GetLobbyBool("scenario", this.Lobby, false);
            set => 
                this.Lobby.SetData("scenario", value.ToString());
        }

        public override string ScenarioBriefing
        {
            get => 
                this.Lobby.GetData("scenarioBriefing");
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
                this.Lobby.SetData("scenarioBriefing", text1);
            }
        }

        public override DateTime ScenarioStartTime
        {
            get => 
                GetLobbyDateTime("scenarioStartTime", this.Lobby, DateTime.MinValue);
            set => 
                this.Lobby.SetData("scenarioStartTime", value.ToString(CultureInfo.InvariantCulture));
        }

        public override bool ExperimentalMode
        {
            get => 
                GetLobbyBool("experimentalMode", this.Lobby, false);
            set => 
                this.Lobby.SetData("experimentalMode", value.ToString());
        }

        public override IEnumerable<ulong> Members =>
            this.Lobby.MemberList;

        public override int MemberCount =>
            this.Lobby.MemberCount;

        public override ulong LobbyId =>
            this.Lobby.LobbyId;

        public override int MemberLimit
        {
            get => 
                this.Lobby.MemberLimit;
            set
            {
            }
        }
    }
}

