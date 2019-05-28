namespace Sandbox.Engine.Multiplayer
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Utils;

    internal sealed class MyMultiplayerClient : MyMultiplayerClientBase
    {
        private string m_worldName;
        private MyGameModeEnum m_gameMode;
        private float m_inventoryMultiplier;
        private float m_blocksInventoryMultiplier;
        private float m_assemblerMultiplier;
        private float m_refineryMultiplier;
        private float m_welderMultiplier;
        private float m_grinderMultiplier;
        private string m_hostName;
        private ulong m_worldSize;
        private int m_appVersion;
        private int m_membersLimit;
        private string m_dataHash;
        private string m_serverPasswordSalt;
        private readonly List<ulong> m_members;
        private readonly Dictionary<ulong, MyMultiplayerBase.MyConnectedClientData> m_memberData;
        public Action OnJoin;
        private List<MyObjectBuilder_Checkpoint.ModItem> m_mods;

        internal MyMultiplayerClient(MyGameServerItem server, MySyncLayer syncLayer) : base(syncLayer)
        {
            this.m_members = new List<ulong>();
            this.m_memberData = new Dictionary<ulong, MyMultiplayerBase.MyConnectedClientData>();
            this.m_mods = new List<MyObjectBuilder_Checkpoint.ModItem>();
            base.SyncLayer.RegisterClientEvents(this);
            base.SyncLayer.TransportLayer.IsBuffering = true;
            this.Server = server;
            base.ServerId = server.SteamID;
            base.ClientLeft += new Action<ulong, MyChatMemberStateChangeEnum>(this.MyMultiplayerClient_ClientLeft);
            syncLayer.TransportLayer.Register(MyMessageId.JOIN_RESULT, 0, new Action<MyPacket>(this.OnJoinResult));
            syncLayer.TransportLayer.Register(MyMessageId.WORLD_DATA, 0, new Action<MyPacket>(this.OnWorldData));
            syncLayer.TransportLayer.Register(MyMessageId.CLIENT_CONNNECTED, 0, new Action<MyPacket>(this.OnClientConnected));
            base.ClientJoined += new Action<ulong>(this.MyMultiplayerClient_ClientJoined);
            base.HostLeft += new Action(this.MyMultiplayerClient_HostLeft);
            MyGameService.Peer2Peer.ConnectionFailed += new Action<ulong, string>(this.Peer2Peer_ConnectionFailed);
            MyGameService.Peer2Peer.SessionRequest += new Action<ulong>(this.Peer2Peer_SessionRequest);
        }

        public override void BanClient(ulong client, bool ban)
        {
            MyControlBanClientMsg message = new MyControlBanClientMsg {
                BannedClient = client,
                Banned = ban
            };
            base.SendControlMessage<MyControlBanClientMsg>(base.ServerId, ref message, true);
        }

        private void CloseClient()
        {
            MyControlDisconnectedMsg message = new MyControlDisconnectedMsg {
                Client = Sync.MyId
            };
            base.SendControlMessage<MyControlDisconnectedMsg>(base.ServerId, ref message, true);
            this.OnJoin = null;
            Thread.Sleep(200);
            this.CloseSession();
        }

        private void CloseSession()
        {
            this.OnJoin = null;
            MyGameService.Peer2Peer.CloseSession(base.ServerId);
            MyGameService.Peer2Peer.ConnectionFailed -= new Action<ulong, string>(this.Peer2Peer_ConnectionFailed);
            MyGameService.Peer2Peer.SessionRequest -= new Action<ulong>(this.Peer2Peer_SessionRequest);
        }

        public override void DisconnectClient(ulong userId)
        {
            this.CloseClient();
        }

        public override void Dispose()
        {
            this.CloseClient();
            base.Dispose();
        }

        public override MyLobbyType GetLobbyType() => 
            MyLobbyType.Public;

        public override ulong GetMemberByIndex(int memberIndex) => 
            this.m_members[memberIndex];

        public override string GetMemberName(ulong steamUserID)
        {
            MyMultiplayerBase.MyConnectedClientData data;
            this.m_memberData.TryGetValue(steamUserID, out data);
            return data.Name;
        }

        public override ulong GetOwner() => 
            base.ServerId;

        public bool IsCorrectVersion() => 
            (this.m_appVersion == MyFinalBuildConstants.APP_VERSION);

        public void LoadMembersFromWorld(List<MyObjectBuilder_Client> clients)
        {
            if (clients != null)
            {
                foreach (MyObjectBuilder_Client client in clients)
                {
                    MyMultiplayerBase.MyConnectedClientData data = new MyMultiplayerBase.MyConnectedClientData {
                        Name = client.Name,
                        IsAdmin = client.IsAdmin
                    };
                    this.m_memberData.Add(client.SteamId, data);
                    base.RaiseClientJoined(client.SteamId);
                }
            }
        }

        private void MyMultiplayerClient_ClientJoined(ulong user)
        {
            if (!this.m_members.Contains(user))
            {
                this.m_members.Add(user);
            }
        }

        private void MyMultiplayerClient_ClientLeft(ulong user, MyChatMemberStateChangeEnum stateChange)
        {
            if (user == base.ServerId)
            {
                base.RaiseHostLeft();
            }
            else
            {
                if (this.m_members.Contains(user))
                {
                    this.m_members.Remove(user);
                    string personaName = MyGameService.GetPersonaName(user);
                    object[] objArray1 = new object[] { "Player disconnected: ", personaName, " (", user, ")" };
                    MySandboxGame.Log.WriteLineAndConsole(string.Concat(objArray1));
                    if (MySandboxGame.IsGameReady && (Sync.MyId != user))
                    {
                        MyHudNotification notification = new MyHudNotification(MyCommonTexts.NotificationClientDisconnected, 0x1388, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
                        object[] arguments = new object[] { personaName };
                        notification.SetTextFormatArguments(arguments);
                        MyHud.Notifications.Add(notification);
                    }
                }
                this.m_memberData.Remove(user);
            }
        }

        private void MyMultiplayerClient_HostLeft()
        {
            this.CloseSession();
            MySessionLoader.UnloadAndExitToMenu();
            MyGuiScreenServerReconnector.ReconnectToLastSession();
        }

        protected override void OnAllMembersData(ref AllMembersDataMsg msg)
        {
            if (msg.Clients != null)
            {
                foreach (MyObjectBuilder_Client client in msg.Clients)
                {
                    if (!this.m_memberData.ContainsKey(client.SteamId))
                    {
                        MyMultiplayerBase.MyConnectedClientData data = new MyMultiplayerBase.MyConnectedClientData {
                            Name = client.Name,
                            IsAdmin = client.IsAdmin
                        };
                        this.m_memberData.Add(client.SteamId, data);
                    }
                    if (!this.m_members.Contains(client.SteamId))
                    {
                        this.m_members.Add(client.SteamId);
                    }
                    if (!Sync.Clients.HasClient(client.SteamId))
                    {
                        Sync.Clients.AddClient(client.SteamId);
                    }
                }
            }
            base.ProcessAllMembersData(ref msg);
        }

        public override void OnChatMessage(ref ChatMsg msg)
        {
            bool flag = false;
            if (this.m_memberData.ContainsKey(msg.Author) && (this.m_memberData[msg.Author].IsAdmin | flag))
            {
                MyClientDebugCommands.Process(msg.Text, msg.Author);
            }
            this.RaiseChatMessageReceived(msg.Author, msg.Text, (ChatChannel) msg.Channel, msg.TargetId, string.IsNullOrEmpty(msg.CustomAuthorName) ? string.Empty : msg.CustomAuthorName);
        }

        protected override void OnClientBan(ref MyControlBanClientMsg data, ulong sender)
        {
            if ((data.BannedClient == Sync.MyId) && data.Banned)
            {
                MySessionLoader.UnloadAndExitToMenu();
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionKicked);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextYouHaveBeenBanned), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
            else
            {
                if (data.Banned)
                {
                    base.AddBannedClient(data.BannedClient);
                }
                else
                {
                    base.RemoveBannedClient(data.BannedClient);
                }
                if (this.m_members.Contains(data.BannedClient) && data.Banned)
                {
                    base.RaiseClientLeft(data.BannedClient, MyChatMemberStateChangeEnum.Banned);
                }
            }
        }

        private void OnClientConnected(MyPacket packet)
        {
            Sync.ClientConnected(packet.Sender.Id.Value);
            ConnectedClientDataMsg msg = base.ReplicationLayer.OnClientConnected(packet);
            this.OnConnectedClient(ref msg);
            packet.Return();
        }

        private void OnConnectedClient(ref ConnectedClientDataMsg msg)
        {
            object[] objArray1 = new object[] { "Client connected: ", msg.Name, " (", msg.SteamID, ")" };
            MySandboxGame.Log.WriteLineAndConsole(string.Concat(objArray1));
            if ((MySandboxGame.IsGameReady && ((msg.SteamID != base.ServerId) && (Sync.MyId != msg.SteamID))) && msg.Join)
            {
                MyHudNotification notification = new MyHudNotification(MyCommonTexts.NotificationClientConnected, 0x1388, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
                object[] arguments = new object[] { msg.Name };
                notification.SetTextFormatArguments(arguments);
                MyHud.Notifications.Add(notification);
            }
            MyMultiplayerBase.MyConnectedClientData data = new MyMultiplayerBase.MyConnectedClientData {
                Name = msg.Name,
                IsAdmin = msg.IsAdmin
            };
            this.m_memberData[msg.SteamID] = data;
            base.RaiseClientJoined(msg.SteamID);
        }

        private void OnJoinResult(MyPacket packet)
        {
            JoinResultMsg msg = base.ReplicationLayer.OnJoinResult(packet);
            this.OnUserJoined(ref msg);
            packet.Return();
        }

        private void OnServerData(ref ServerDataMsg msg)
        {
            this.m_worldName = msg.WorldName;
            this.m_gameMode = msg.GameMode;
            this.m_inventoryMultiplier = msg.InventoryMultiplier;
            this.m_blocksInventoryMultiplier = msg.BlocksInventoryMultiplier;
            this.m_assemblerMultiplier = msg.AssemblerMultiplier;
            this.m_refineryMultiplier = msg.RefineryMultiplier;
            this.m_welderMultiplier = msg.WelderMultiplier;
            this.m_grinderMultiplier = msg.GrinderMultiplier;
            this.m_hostName = msg.HostName;
            this.m_worldSize = msg.WorldSize;
            this.m_appVersion = msg.AppVersion;
            this.m_membersLimit = msg.MembersLimit;
            this.m_dataHash = msg.DataHash;
            this.m_serverPasswordSalt = msg.ServerPasswordSalt;
        }

        private void OnUserJoined(ref JoinResultMsg msg)
        {
            if (msg.JoinResult == JoinResult.OK)
            {
                base.IsServerExperimental = msg.ServerExperimental;
                if (this.OnJoin != null)
                {
                    this.OnJoin();
                    this.OnJoin = null;
                }
            }
            else
            {
                StringBuilder builder;
                MyStringId? nullable;
                Vector2? nullable2;
                if (msg.JoinResult == JoinResult.NotInGroup)
                {
                    MySessionLoader.UnloadAndExitToMenu();
                    this.Dispose();
                    ulong groupId = this.Server.GetGameTagByPrefixUlong("groupId");
                    string clanName = MyGameService.GetClanName(groupId);
                    builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.MultiplayerErrorNotInGroup), clanName)), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
                    screen.ResultCallback = delegate (MyGuiScreenMessageBox.ResultEnum result) {
                        if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                        {
                            MyGameService.OpenOverlayUser(groupId);
                        }
                    };
                    MyGuiSandbox.AddScreen(screen);
                }
                else if (msg.JoinResult == JoinResult.BannedByAdmins)
                {
                    MySessionLoader.UnloadAndExitToMenu();
                    this.Dispose();
                    ulong admin = msg.Admin;
                    if (admin == 0)
                    {
                        builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MultiplayerErrorBannedByAdmins), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    }
                    else
                    {
                        builder = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable = null;
                        nullable2 = null;
                        MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MultiplayerErrorBannedByAdminsWithDialog), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
                        screen.ResultCallback = delegate (MyGuiScreenMessageBox.ResultEnum result) {
                            if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                            {
                                MyGameService.OpenOverlayUser(admin);
                            }
                        };
                        MyGuiSandbox.AddScreen(screen);
                    }
                }
                else
                {
                    StringBuilder messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorConnectionFailed);
                    switch (msg.JoinResult)
                    {
                        case JoinResult.AlreadyJoined:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorAlreadyJoined);
                            break;

                        case JoinResult.TicketInvalid:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorTicketInvalid);
                            break;

                        case JoinResult.SteamServersOffline:
                            messageText = new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MultiplayerErrorSteamServersOffline), MySession.Platform);
                            break;

                        case JoinResult.GroupIdInvalid:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorGroupIdInvalid);
                            break;

                        case JoinResult.ServerFull:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorServerFull);
                            break;

                        case JoinResult.KickedRecently:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorKickedByAdmins);
                            break;

                        case JoinResult.TicketCanceled:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorTicketCanceled);
                            break;

                        case JoinResult.TicketAlreadyUsed:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorTicketAlreadyUsed);
                            break;

                        case JoinResult.LoggedInElseWhere:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorLoggedInElseWhere);
                            break;

                        case JoinResult.NoLicenseOrExpired:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorNoLicenseOrExpired);
                            break;

                        case JoinResult.UserNotConnected:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorUserNotConnected);
                            break;

                        case JoinResult.VACBanned:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorVACBanned);
                            break;

                        case JoinResult.VACCheckTimedOut:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorVACCheckTimedOut);
                            break;

                        case JoinResult.PasswordRequired:
                            MyGuiSandbox.AddScreen(new MyGuiScreenServerPassword(password => this.SendPasswordHash(password)));
                            return;

                        case JoinResult.WrongPassword:
                            messageText = MyTexts.Get(MyCommonTexts.MultiplayerErrorWrongPassword);
                            break;

                        case JoinResult.ExperimentalMode:
                            messageText = !MySandboxGame.Config.ExperimentalMode ? MyTexts.Get(MyCommonTexts.MultiplayerErrorExperimental) : MyTexts.Get(MyCommonTexts.MultiplayerErrorNotExperimental);
                            break;

                        default:
                            break;
                    }
                    this.Dispose();
                    MySessionLoader.UnloadAndExitToMenu();
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
            }
        }

        private void OnWorldData(MyPacket packet)
        {
            ServerDataMsg msg = base.ReplicationLayer.OnWorldData(packet);
            this.OnServerData(ref msg);
            packet.Return();
        }

        private void Peer2Peer_ConnectionFailed(ulong remoteUserId, string error)
        {
            if (remoteUserId == base.ServerId)
            {
                base.RaiseHostLeft();
            }
        }

        private void Peer2Peer_SessionRequest(ulong remoteUserId)
        {
            if (!base.IsClientKickedOrBanned(remoteUserId) && (base.IsServer || (remoteUserId == base.ServerId)))
            {
                MyGameService.Peer2Peer.AcceptSession(remoteUserId);
            }
        }

        public override void SendChatMessage(string text, ChatChannel channel, long targetId = 0L)
        {
            ChatMsg msg = new ChatMsg {
                Text = text,
                Author = Sync.MyId,
                Channel = (byte) channel,
                TargetId = targetId,
                CustomAuthorName = string.Empty
            };
            this.OnChatMessage(ref msg);
            SendChatMessage(ref msg);
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

        public void SendPasswordHash(string password)
        {
            if (string.IsNullOrEmpty(this.m_serverPasswordSalt))
            {
                MyLog.Default.Error("Empty password salt on the server.", Array.Empty<object>());
            }
            else
            {
                byte[] salt = Convert.FromBase64String(this.m_serverPasswordSalt);
                MyControlSendPasswordHashMsg message = new MyControlSendPasswordHashMsg {
                    PasswordHash = new Rfc2898DeriveBytes(password, salt, 0x2710).GetBytes(20)
                };
                base.SendControlMessage<MyControlSendPasswordHashMsg>(base.ServerId, ref message, true);
            }
        }

        public void SendPlayerData(string clientName)
        {
            uint num;
            uint num2;
            ConnectedClientDataMsg msg = new ConnectedClientDataMsg {
                SteamID = Sync.MyId,
                Name = clientName,
                Join = true,
                ExperimentalMode = this.ExperimentalMode
            };
            byte[] buffer = new byte[0x400];
            if (MyGameService.GetAuthSessionTicket(out num2, buffer, out num))
            {
                msg.Token = new byte[num];
                Array.Copy(buffer, msg.Token, (long) num);
                base.ReplicationLayer.SendClientConnected(ref msg);
            }
            else
            {
                MySessionLoader.UnloadAndExitToMenu();
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MultiplayerErrorConnectionFailed), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        public override void SetLobbyType(MyLobbyType myLobbyType)
        {
        }

        public override void SetMemberLimit(int limit)
        {
            this.m_membersLimit = limit;
        }

        public override void SetOwner(ulong owner)
        {
        }

        public override void StartProcessingClientMessagesWithEmptyWorld()
        {
            if (!Sync.Clients.HasClient(base.ServerId))
            {
                Sync.Clients.AddClient(base.ServerId);
            }
            base.StartProcessingClientMessagesWithEmptyWorld();
            if (Sync.Clients.LocalClient == null)
            {
                Sync.Clients.SetLocalSteamId(Sync.MyId, !Sync.Clients.HasClient(Sync.MyId));
            }
        }

        protected override bool IsServerInternal =>
            false;

        public override string WorldName
        {
            get => 
                this.m_worldName;
            set => 
                (this.m_worldName = value);
        }

        public override MyGameModeEnum GameMode
        {
            get => 
                this.m_gameMode;
            set => 
                (this.m_gameMode = value);
        }

        public override float InventoryMultiplier
        {
            get => 
                this.m_inventoryMultiplier;
            set => 
                (this.m_inventoryMultiplier = value);
        }

        public override float BlocksInventoryMultiplier
        {
            get => 
                this.m_blocksInventoryMultiplier;
            set => 
                (this.m_blocksInventoryMultiplier = value);
        }

        public override float AssemblerMultiplier
        {
            get => 
                this.m_assemblerMultiplier;
            set => 
                (this.m_assemblerMultiplier = value);
        }

        public override float RefineryMultiplier
        {
            get => 
                this.m_refineryMultiplier;
            set => 
                (this.m_refineryMultiplier = value);
        }

        public override float WelderMultiplier
        {
            get => 
                this.m_welderMultiplier;
            set => 
                (this.m_welderMultiplier = value);
        }

        public override float GrinderMultiplier
        {
            get => 
                this.m_grinderMultiplier;
            set => 
                (this.m_grinderMultiplier = value);
        }

        public override string HostName
        {
            get => 
                this.m_hostName;
            set => 
                (this.m_hostName = value);
        }

        public override ulong WorldSize
        {
            get => 
                this.m_worldSize;
            set => 
                (this.m_worldSize = value);
        }

        public override int AppVersion
        {
            get => 
                this.m_appVersion;
            set => 
                (this.m_appVersion = value);
        }

        public override string DataHash
        {
            get => 
                this.m_dataHash;
            set => 
                (this.m_dataHash = value);
        }

        public override int MaxPlayers =>
            0x10000;

        public override int ModCount { get; protected set; }

        public override List<MyObjectBuilder_Checkpoint.ModItem> Mods
        {
            get => 
                this.m_mods;
            set
            {
                this.m_mods = value;
                this.ModCount = this.m_mods.Count;
            }
        }

        public override int ViewDistance { get; set; }

        public override bool Scenario { get; set; }

        public override string ScenarioBriefing { get; set; }

        public override DateTime ScenarioStartTime { get; set; }

        public MyGameServerItem Server { get; private set; }

        public override bool ExperimentalMode { get; set; }

        public override IEnumerable<ulong> Members =>
            this.m_members;

        public override int MemberCount =>
            this.m_members.Count;

        public override ulong LobbyId =>
            0UL;

        public override int MemberLimit
        {
            get => 
                this.m_membersLimit;
            set => 
                (this.m_membersLimit = value);
        }
    }
}

