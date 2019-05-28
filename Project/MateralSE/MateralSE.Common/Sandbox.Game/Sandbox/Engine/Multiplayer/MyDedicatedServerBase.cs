namespace Sandbox.Engine.Multiplayer
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Net;
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

    public abstract class MyDedicatedServerBase : MyMultiplayerServerBase
    {
        protected string m_worldName;
        protected MyGameModeEnum m_gameMode;
        protected string m_hostName;
        protected ulong m_worldSize;
        protected int m_appVersion;
        protected int m_membersLimit;
        protected string m_dataHash;
        protected ulong m_groupId;
        private readonly List<ulong> m_members;
        private readonly Dictionary<ulong, MyMultiplayerBase.MyConnectedClientData> m_memberData;
        private bool m_gameServerDataDirty;
        private readonly Dictionary<ulong, MyMultiplayerBase.MyConnectedClientData> m_pendingMembers;
        private readonly HashSet<ulong> m_waitingForGroup;
        private int m_modCount;
        private List<MyObjectBuilder_Checkpoint.ModItem> m_mods;
        private const string STEAM_ID_PREFIX = "STEAM_";
        private const ulong STEAM_ID_MAGIC_CONSTANT = 0x110000100000000UL;

        protected MyDedicatedServerBase(MySyncLayer syncLayer) : base(syncLayer, new EndpointId(Sync.MyId))
        {
            this.m_appVersion = (int) MyFinalBuildConstants.APP_VERSION;
            this.m_members = new List<ulong>();
            this.m_memberData = new Dictionary<ulong, MyMultiplayerBase.MyConnectedClientData>();
            this.m_pendingMembers = new Dictionary<ulong, MyMultiplayerBase.MyConnectedClientData>();
            this.m_waitingForGroup = new HashSet<ulong>();
            this.m_mods = new List<MyObjectBuilder_Checkpoint.ModItem>();
            syncLayer.TransportLayer.Register(MyMessageId.CLIENT_CONNNECTED, 0xff, new Action<MyPacket>(this.ClientConnected));
        }

        public override void BanClient(ulong userId, bool banned)
        {
            MyControlBanClientMsg msg2;
            if (!banned)
            {
                MyLog.Default.WriteLineAndConsole("Player " + userId + " unbanned");
                msg2 = new MyControlBanClientMsg {
                    BannedClient = userId,
                    Banned = false
                };
                MyControlBanClientMsg message = msg2;
                base.SendControlMessageToAll<MyControlBanClientMsg>(ref message, 0UL);
                base.RemoveBannedClient(userId);
                MySandboxGame.ConfigDedicated.Banned.Remove(userId);
            }
            else
            {
                MyLog.Default.WriteLineAndConsole("Player " + this.GetMemberName(userId) + " banned");
                msg2 = new MyControlBanClientMsg {
                    BannedClient = userId,
                    Banned = true
                };
                MyControlBanClientMsg message = msg2;
                base.SendControlMessageToAll<MyControlBanClientMsg>(ref message, 0UL);
                base.AddBannedClient(userId);
                if (this.m_members.Contains(userId))
                {
                    base.RaiseClientLeft(userId, MyChatMemberStateChangeEnum.Banned);
                }
                MySandboxGame.ConfigDedicated.Banned.Add(userId);
            }
            MySandboxGame.ConfigDedicated.Save(null);
        }

        private unsafe void ClientConnected(MyPacket packet)
        {
            Sync.ClientConnected(packet.Sender.Id.Value);
            ConnectedClientDataMsg msg = base.ReplicationLayer.OnClientConnected(packet);
            ConnectedClientDataMsg* msgPtr1 = (ConnectedClientDataMsg*) ref msg;
            this.OnConnectedClient(ref (ConnectedClientDataMsg) ref msgPtr1, msg.SteamID);
            packet.Return();
        }

        private static string ConvertSteamIDFrom64(ulong from)
        {
            from -= (ulong) 0x110000100000000L;
            return new StringBuilder("STEAM_").Append("0:").Append((ulong) (from % ((ulong) 2L))).Append(':').Append((ulong) (from / ((ulong) 2L))).ToString();
        }

        private static ulong ConvertSteamIDTo64(string from)
        {
            char[] separator = new char[] { ':' };
            string[] strArray = from.Replace("STEAM_", "").Split(separator);
            return ((strArray.Length == 3) ? ((((ulong) 0x110000100000000L) + Convert.ToUInt64(strArray[1])) + (Convert.ToUInt64(strArray[2]) * ((ulong) 2L))) : 0UL);
        }

        public override void DisconnectClient(ulong userId)
        {
            MyControlDisconnectedMsg message = new MyControlDisconnectedMsg {
                Client = base.ServerId
            };
            base.SendControlMessage<MyControlDisconnectedMsg>(userId, ref message, true);
            base.RaiseClientLeft(userId, MyChatMemberStateChangeEnum.Disconnected);
        }

        public override void Dispose()
        {
            foreach (ulong num in this.m_members)
            {
                MyControlDisconnectedMsg message = new MyControlDisconnectedMsg {
                    Client = base.ServerId
                };
                if (num != base.ServerId)
                {
                    base.SendControlMessage<MyControlDisconnectedMsg>(num, ref message, true);
                }
            }
            Thread.Sleep(200);
            try
            {
                base.CloseMemberSessions();
                MyGameService.GameServer.EnableHeartbeats(false);
                base.Dispose();
                MyLog.Default.WriteLineAndConsole("Logging off Steam...");
                MyGameService.GameServer.LogOff();
                MyLog.Default.WriteLineAndConsole("Shutting down server...");
                MyGameService.GameServer.Shutdown();
                MyLog.Default.WriteLineAndConsole("Done");
                MyGameService.Peer2Peer.SessionRequest -= new Action<ulong>(this.Peer2Peer_SessionRequest);
                MyGameService.Peer2Peer.ConnectionFailed -= new Action<ulong, string>(this.Peer2Peer_ConnectionFailed);
                base.ClientLeft -= new Action<ulong, MyChatMemberStateChangeEnum>(this.MyDedicatedServer_ClientLeft);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole("catch exception : " + exception);
            }
        }

        public override void DownloadWorld()
        {
        }

        private void GameServer_PolicyResponse(sbyte result)
        {
            MyLog.Default.WriteLineAndConsole("Server PolicyResponse (" + result + ")");
        }

        private void GameServer_ServersConnected()
        {
            MyLog.Default.WriteLineAndConsole("Server connected to Steam");
        }

        private void GameServer_ServersConnectFailure(string result)
        {
            MyLog.Default.WriteLineAndConsole("Server connect failure (" + result + ")");
        }

        private void GameServer_ServersDisconnected(string result)
        {
            MyLog.Default.WriteLineAndConsole("Server disconnected (" + result + ")");
        }

        private void GameServer_UserGroupStatus(ulong userId, ulong groupId, bool member, bool officier)
        {
            if ((groupId == this.m_groupId) && this.m_waitingForGroup.Remove(userId))
            {
                if (member | officier)
                {
                    this.UserAccepted(userId);
                }
                else
                {
                    this.UserRejected(userId, JoinResult.NotInGroup);
                }
            }
        }

        private void GameServer_ValidateAuthTicketResponse(ulong steamID, JoinResult response, ulong steamOwner)
        {
            object[] objArray1 = new object[] { "Server ValidateAuthTicketResponse (", response, "), owner: ", steamOwner };
            MyLog.Default.WriteLineAndConsole(string.Concat(objArray1));
            if (base.IsClientBanned(steamOwner) || MySandboxGame.ConfigDedicated.Banned.Contains(steamOwner))
            {
                this.UserRejected(steamID, JoinResult.BannedByAdmins);
                base.RaiseClientKicked(steamID);
            }
            else if (base.IsClientKicked(steamOwner))
            {
                this.UserRejected(steamID, JoinResult.KickedRecently);
                base.RaiseClientKicked(steamID);
            }
            else if (response != JoinResult.OK)
            {
                this.UserRejected(steamID, response);
            }
            else if ((MySandboxGame.ConfigDedicated.Administrators.Contains(steamID.ToString()) || MySandboxGame.ConfigDedicated.Administrators.Contains(ConvertSteamIDFrom64(steamID))) || MySandboxGame.ConfigDedicated.Reserved.Contains(steamID))
            {
                this.UserAccepted(steamID);
            }
            else if ((this.MemberLimit > 0) && ((this.m_members.Count - 1) >= this.MemberLimit))
            {
                this.UserRejected(steamID, JoinResult.ServerFull);
            }
            else if (this.m_groupId == 0)
            {
                this.UserAccepted(steamID);
            }
            else if (MyGameService.GetServerAccountType(this.m_groupId) != MyGameServiceAccountType.Clan)
            {
                this.UserRejected(steamID, JoinResult.GroupIdInvalid);
            }
            else if (MyGameService.GameServer.RequestGroupStatus(steamID, this.m_groupId))
            {
                this.m_waitingForGroup.Add(steamID);
            }
            else
            {
                this.UserRejected(steamID, JoinResult.SteamServersOffline);
            }
        }

        public override MyLobbyType GetLobbyType() => 
            MyLobbyType.Public;

        public override ulong GetMemberByIndex(int memberIndex) => 
            this.m_members[memberIndex];

        public override string GetMemberName(ulong steamUserID)
        {
            MyMultiplayerBase.MyConnectedClientData data;
            this.MemberDataGet(steamUserID, out data);
            return ((data.Name == null) ? ("ID:" + steamUserID) : data.Name);
        }

        public override ulong GetOwner() => 
            base.ServerId;

        protected void Initialize(IPEndPoint serverEndpoint)
        {
            this.m_groupId = MySandboxGame.ConfigDedicated.GroupID;
            this.ServerStarted = false;
            this.HostName = "Dedicated server";
            this.SetMemberLimit(this.MaxPlayers);
            MyGameService.Peer2Peer.SessionRequest += new Action<ulong>(this.Peer2Peer_SessionRequest);
            MyGameService.Peer2Peer.ConnectionFailed += new Action<ulong, string>(this.Peer2Peer_ConnectionFailed);
            base.ClientLeft += new Action<ulong, MyChatMemberStateChangeEnum>(this.MyDedicatedServer_ClientLeft);
            MyGameService.GameServer.PlatformConnected += new Action(this.GameServer_ServersConnected);
            MyGameService.GameServer.PlatformConnectionFailed += new Action<string>(this.GameServer_ServersConnectFailure);
            MyGameService.GameServer.PlatformDisconnected += new Action<string>(this.GameServer_ServersDisconnected);
            MyGameService.GameServer.PolicyResponse += new Action<sbyte>(this.GameServer_PolicyResponse);
            MyGameService.GameServer.ValidateAuthTicketResponse += new Action<ulong, JoinResult, ulong>(this.GameServer_ValidateAuthTicketResponse);
            MyGameService.GameServer.UserGroupStatusResponse += new Action<ulong, ulong, bool, bool>(this.GameServer_UserGroupStatus);
            string serverName = MySandboxGame.ConfigDedicated.ServerName;
            if (string.IsNullOrWhiteSpace(serverName))
            {
                serverName = "Unnamed server";
            }
            MyGameService.GameServer.SetServerName(serverName);
            MyGameService.Peer2Peer.SetServer(true);
            if (MyGameService.GameServer.Start(serverEndpoint, (ushort) MySandboxGame.ConfigDedicated.SteamPort, MyFinalBuildConstants.APP_VERSION.ToString()))
            {
                MyGameService.GameServer.SetModDir(MyPerGameSettings.SteamGameServerGameDir);
                MyGameService.GameServer.ProductName = MyPerGameSettings.SteamGameServerProductName;
                MyGameService.GameServer.GameDescription = MyPerGameSettings.SteamGameServerDescription;
                MyGameService.GameServer.SetDedicated(true);
                if (!string.IsNullOrEmpty(MySandboxGame.ConfigDedicated.ServerPasswordHash) && !string.IsNullOrEmpty(MySandboxGame.ConfigDedicated.ServerPasswordSalt))
                {
                    MyGameService.GameServer.SetPasswordProtected(true);
                    this.IsPasswordProtected = true;
                }
                MyGameService.GameServer.LogOnAnonymous();
                MyGameService.GameServer.EnableHeartbeats(true);
                if ((this.m_groupId != 0) && (MyGameService.GetServerAccountType(this.m_groupId) != MyGameServiceAccountType.Clan))
                {
                    MyLog.Default.WriteLineAndConsole("Specified group ID is invalid: " + this.m_groupId);
                }
                uint ip = 0;
                ulong serverId = 0UL;
                int num3 = 100;
                while ((ip == 0) && (num3 > 0))
                {
                    MyGameService.GameServer.Update();
                    Thread.Sleep(100);
                    num3--;
                    ip = MyGameService.GameServer.GetPublicIP();
                    serverId = MyGameService.GameServer.ServerId;
                }
                MyGameService.UserId = serverId;
                if (ip == 0)
                {
                    MyLog.Default.WriteLineAndConsole("Error: No IP assigned.");
                }
                else
                {
                    IPAddress address = IPAddressExtensions.FromIPv4NetworkOrder(ip);
                    base.ServerId = MyGameService.GameServer.ServerId;
                    base.ReplicationLayer.SetLocalEndpoint(new EndpointId(MyGameService.GameServer.ServerId));
                    this.m_members.Add(base.ServerId);
                    MyMultiplayerBase.MyConnectedClientData data = new MyMultiplayerBase.MyConnectedClientData {
                        Name = MyTexts.GetString(MySpaceTexts.ChatBotName),
                        IsAdmin = true
                    };
                    this.MemberDataAdd(base.ServerId, data);
                    base.SyncLayer.RegisterClientEvents(this);
                    MyLog.Default.WriteLineAndConsole("Server successfully started");
                    MyLog.Default.WriteLineAndConsole("Product name: " + MyGameService.GameServer.ProductName);
                    MyLog.Default.WriteLineAndConsole("Desc: " + MyGameService.GameServer.GameDescription);
                    MyLog.Default.WriteLineAndConsole("Public IP: " + address.ToString());
                    MyLog.Default.WriteLineAndConsole("Steam ID: " + serverId.ToString());
                    this.ServerStarted = true;
                }
            }
        }

        public virtual bool IsCorrectVersion() => 
            (this.m_appVersion == MyFinalBuildConstants.APP_VERSION);

        private void MemberDataAdd(ulong steamId, MyMultiplayerBase.MyConnectedClientData data)
        {
            this.m_memberData.Add(steamId, data);
            this.m_gameServerDataDirty = true;
        }

        protected bool MemberDataGet(ulong steamId, out MyMultiplayerBase.MyConnectedClientData data) => 
            this.m_memberData.TryGetValue(steamId, out data);

        private void MemberDataRemove(ulong steamId)
        {
            this.m_memberData.Remove(steamId);
            this.m_gameServerDataDirty = true;
        }

        protected void MemberDataSet(ulong steamId, MyMultiplayerBase.MyConnectedClientData data)
        {
            this.m_memberData[steamId] = data;
            this.m_gameServerDataDirty = true;
        }

        private void MyDedicatedServer_ClientLeft(ulong user, MyChatMemberStateChangeEnum arg2)
        {
            MyGameService.Peer2Peer.CloseSession(user);
            MyLog.Default.WriteLineAndConsole("User left " + this.GetMemberName(user));
            if (this.m_members.Contains(user))
            {
                this.m_members.Remove(user);
            }
            if (this.m_pendingMembers.ContainsKey(user))
            {
                this.m_pendingMembers.Remove(user);
            }
            if (this.m_waitingForGroup.Contains(user))
            {
                this.m_waitingForGroup.Remove(user);
            }
            if ((arg2 != MyChatMemberStateChangeEnum.Kicked) && (arg2 != MyChatMemberStateChangeEnum.Banned))
            {
                foreach (ulong num in this.m_members)
                {
                    if (num != base.ServerId)
                    {
                        MyControlDisconnectedMsg message = new MyControlDisconnectedMsg {
                            Client = user
                        };
                        base.SendControlMessage<MyControlDisconnectedMsg>(num, ref message, true);
                    }
                }
            }
            MyGameService.GameServer.SendUserDisconnect(user);
            this.MemberDataRemove(user);
        }

        protected override void OnClientBan(ref MyControlBanClientMsg data, ulong sender)
        {
            if (MySession.Static.IsUserAdmin(sender))
            {
                this.BanClient(data.BannedClient, (bool) data.Banned);
            }
        }

        private unsafe void OnConnectedClient(ref ConnectedClientDataMsg msg, ulong steamId)
        {
            if (!MyGameService.GameServer.BeginAuthSession(steamId, msg.Token))
            {
                MyLog.Default.WriteLineAndConsole("Authentication failed.");
            }
            else if (!msg.ExperimentalMode && this.ExperimentalMode)
            {
                MyLog.Default.WriteLineAndConsole("Server and client Experimental Mode does not match.");
                this.SendJoinResult(steamId, JoinResult.ExperimentalMode, 0UL);
            }
            else
            {
                base.RaiseClientJoined(steamId);
                MyLog.Default.WriteLineAndConsole("OnConnectedClient " + msg.Name + " attempt");
                if (this.m_members.Contains(steamId))
                {
                    MyLog.Default.WriteLineAndConsole("Already joined");
                    this.SendJoinResult(steamId, JoinResult.AlreadyJoined, 0UL);
                }
                else if (!MySandboxGame.ConfigDedicated.Banned.Contains(steamId))
                {
                    MyMultiplayerBase.MyConnectedClientData* dataPtr1;
                    MyMultiplayerBase.MyConnectedClientData data = new MyMultiplayerBase.MyConnectedClientData {
                        Name = msg.Name
                    };
                    dataPtr1->IsAdmin = MySandboxGame.ConfigDedicated.Administrators.Contains(steamId.ToString()) || MySandboxGame.ConfigDedicated.Administrators.Contains(ConvertSteamIDFrom64(steamId));
                    dataPtr1 = (MyMultiplayerBase.MyConnectedClientData*) ref data;
                    this.m_pendingMembers.Add(steamId, data);
                }
                else
                {
                    MyLog.Default.WriteLineAndConsole("User is banned by admins");
                    ulong result = 0UL;
                    foreach (KeyValuePair<ulong, MyMultiplayerBase.MyConnectedClientData> pair in this.m_memberData)
                    {
                        if (pair.Value.IsAdmin && (pair.Key != base.ServerId))
                        {
                            result = pair.Key;
                            break;
                        }
                    }
                    if ((result == 0) && (MySandboxGame.ConfigDedicated.Administrators.Count > 0))
                    {
                        ulong.TryParse(MySandboxGame.ConfigDedicated.Administrators[0], out result);
                    }
                    this.SendJoinResult(steamId, JoinResult.BannedByAdmins, result);
                }
            }
        }

        protected override void OnPasswordHash(ref MyControlSendPasswordHashMsg message, ulong sender)
        {
            base.OnPasswordHash(ref message, sender);
            if (!this.IsPasswordProtected || string.IsNullOrEmpty(MySandboxGame.ConfigDedicated.ServerPasswordHash))
            {
                this.SendJoinResult(sender, JoinResult.OK, 0UL);
            }
            else
            {
                byte[] passwordHash = message.PasswordHash;
                byte[] buffer2 = Convert.FromBase64String(MySandboxGame.ConfigDedicated.ServerPasswordHash);
                if ((passwordHash == null) || (passwordHash.Length != buffer2.Length))
                {
                    this.RejectUserWithWrongPassword(sender);
                }
                else
                {
                    for (int i = 0; i < buffer2.Length; i++)
                    {
                        if (buffer2[i] != passwordHash[i])
                        {
                            this.RejectUserWithWrongPassword(sender);
                            return;
                        }
                    }
                    base.ResetWrongPasswordCounter(sender);
                    this.SendJoinResult(sender, JoinResult.OK, 0UL);
                }
            }
        }

        private void Peer2Peer_ConnectionFailed(ulong remoteUserId, string error)
        {
            object[] objArray1 = new object[] { "Peer2Peer_ConnectionFailed ", remoteUserId, ", ", error };
            MyLog.Default.WriteLineAndConsole(string.Concat(objArray1));
            MySandboxGame.Static.Invoke(() => this.RaiseClientLeft(remoteUserId, MyChatMemberStateChangeEnum.Disconnected), "RaiseClientLeft");
        }

        private void Peer2Peer_SessionRequest(ulong remoteUserId)
        {
            MyLog.Default.WriteLineAndConsole("Peer2Peer_SessionRequest " + remoteUserId);
            MyGameService.Peer2Peer.AcceptSession(remoteUserId);
        }

        private void RejectUserWithWrongPassword(ulong sender)
        {
            base.AddWrongPasswordClient(sender);
            if (base.IsOutOfWrongPasswordTries(sender))
            {
                this.KickClient(sender, true, true);
            }
            else
            {
                this.SendJoinResult(sender, JoinResult.WrongPassword, 0UL);
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
            SendChatMessage(ref msg);
        }

        public override unsafe void SendChatMessageScripted(string text, ChatChannel channel, long targetId = 0L, string customAuthor = null)
        {
            ChatMsg* msgPtr1;
            ChatMsg msg = new ChatMsg {
                Text = text,
                Author = Sync.MyId,
                Channel = (byte) channel,
                TargetId = targetId
            };
            msgPtr1->CustomAuthorName = string.IsNullOrEmpty(customAuthor) ? string.Empty : customAuthor;
            msgPtr1 = (ChatMsg*) ref msg;
            SendChatMessage(ref msg);
        }

        public void SendChatMessageToPlayer(string text, ulong steamId)
        {
            if (MyMultiplayer.Static.IsServer)
            {
                ChatMsg msg = new ChatMsg {
                    Text = text,
                    Author = Sync.MyId,
                    Channel = 3,
                    CustomAuthorName = string.Empty
                };
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<ChatMsg>(s => new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_ToPlayer), msg, new EndpointId(steamId), position);
            }
        }

        private unsafe void SendClientData(ulong steamTo, ulong connectedSteamID, string connectedClientName, bool join)
        {
            ConnectedClientDataMsg* msgPtr1;
            ConnectedClientDataMsg msg2 = new ConnectedClientDataMsg {
                SteamID = connectedSteamID,
                Name = connectedClientName
            };
            msgPtr1->IsAdmin = MySandboxGame.ConfigDedicated.Administrators.Contains(connectedSteamID.ToString()) || MySandboxGame.ConfigDedicated.Administrators.Contains(ConvertSteamIDFrom64(connectedSteamID));
            msgPtr1 = (ConnectedClientDataMsg*) ref msg2;
            msg2.Join = join;
            ConnectedClientDataMsg msg = msg2;
            base.ReplicationLayer.SendClientConnected(ref msg, steamTo);
        }

        internal abstract void SendGameTagsToSteam();
        private void SendJoinResult(ulong sendTo, JoinResult joinResult, ulong adminID = 0UL)
        {
            JoinResultMsg msg = new JoinResultMsg {
                JoinResult = joinResult,
                ServerExperimental = MySession.Static.IsSettingsExperimental(),
                Admin = adminID
            };
            base.ReplicationLayer.SendJoinResult(ref msg, sendTo);
        }

        protected abstract void SendServerData();
        public override void SetLobbyType(MyLobbyType myLobbyType)
        {
        }

        public override void SetMemberLimit(int limit)
        {
            int membersLimit = this.m_membersLimit;
            this.m_membersLimit = (MyDedicatedServerOverrides.MaxPlayers != null) ? MyDedicatedServerOverrides.MaxPlayers.Value : Math.Max(limit, 2);
            this.m_gameServerDataDirty |= membersLimit != this.m_membersLimit;
        }

        public override void SetOwner(ulong owner)
        {
        }

        public override void Tick()
        {
            base.Tick();
            this.UpdateSteamServerData();
        }

        private void UpdateSteamServerData()
        {
            if (this.m_gameServerDataDirty)
            {
                MyGameService.GameServer.SetMapName(this.m_worldName);
                MyGameService.GameServer.SetMaxPlayerCount(this.m_membersLimit);
                foreach (KeyValuePair<ulong, MyMultiplayerBase.MyConnectedClientData> pair in this.m_memberData)
                {
                    MyGameService.GameServer.BrowserUpdateUserData(pair.Key, pair.Value.Name, 0);
                }
                this.m_gameServerDataDirty = false;
            }
        }

        private void UserAccepted(ulong steamID)
        {
            MyMultiplayerBase.MyConnectedClientData data;
            this.m_members.Add(steamID);
            if (this.m_pendingMembers.TryGetValue(steamID, out data))
            {
                this.m_pendingMembers.Remove(steamID);
                this.MemberDataSet(steamID, data);
                foreach (ulong num in this.m_members)
                {
                    if (num != base.ServerId)
                    {
                        this.SendClientData(num, steamID, data.Name, true);
                    }
                }
            }
            this.SendServerData();
            if (this.IsPasswordProtected)
            {
                this.SendJoinResult(steamID, JoinResult.PasswordRequired, 0UL);
            }
            else
            {
                this.SendJoinResult(steamID, JoinResult.OK, 0UL);
            }
        }

        private void UserRejected(ulong steamID, JoinResult reason)
        {
            this.m_pendingMembers.Remove(steamID);
            this.m_waitingForGroup.Remove(steamID);
            if (this.m_members.Contains(steamID))
            {
                base.RaiseClientLeft(steamID, MyChatMemberStateChangeEnum.Disconnected);
            }
            else
            {
                this.SendJoinResult(steamID, reason, 0UL);
            }
        }

        protected override bool IsServerInternal =>
            true;

        public bool ServerStarted { get; private set; }

        public string ServerInitError { get; private set; }

        public override string WorldName
        {
            get => 
                this.m_worldName;
            set
            {
                this.m_worldName = string.IsNullOrEmpty(value) ? "noname" : value;
                this.m_gameServerDataDirty = true;
            }
        }

        public override MyGameModeEnum GameMode
        {
            get => 
                this.m_gameMode;
            set => 
                (this.m_gameMode = value);
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
            0x400;

        public override int ModCount
        {
            get => 
                this.m_modCount;
            protected set
            {
                this.m_modCount = value;
                MyGameService.GameServer.SetKeyValue("mods", this.m_modCount.ToString());
            }
        }

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

        public override int SyncDistance
        {
            get => 
                MyLayers.GetSyncDistance();
            set => 
                MyLayers.SetSyncDistance(value);
        }

        public bool IsPasswordProtected { get; set; }

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
                this.SetMemberLimit(value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyDedicatedServerBase.<>c <>9 = new MyDedicatedServerBase.<>c();
            public static Func<IMyEventOwner, Action<ChatMsg>> <>9__95_0;

            internal Action<ChatMsg> <SendChatMessageToPlayer>b__95_0(IMyEventOwner s) => 
                new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_ToPlayer);
        }
    }
}

