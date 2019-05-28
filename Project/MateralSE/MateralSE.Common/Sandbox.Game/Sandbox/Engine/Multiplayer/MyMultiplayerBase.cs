namespace Sandbox.Engine.Multiplayer
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.StateGroups;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game.ModAPI;
    using VRage.GameServices;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Profiler;
    using VRage.Replication;
    using VRage.Utils;

    [StaticEventOwner, PreloadRequired]
    public abstract class MyMultiplayerBase : MyMultiplayerMinimalBase, IDisposable
    {
        private static readonly int NUMBER_OF_WRONG_PASSWORD_TRIES_BEFORE_KICK = 3;
        public readonly MySyncLayer SyncLayer;
        private IMyPeer2Peer m_networkService;
        private ulong m_serverId;
        protected LRUCache<string, byte[]> m_voxelMapData;
        private readonly ConcurrentDictionary<ulong, int> m_kickedClients;
        private readonly Dictionary<ulong, int> m_wrongPasswordClients;
        private readonly MyConcurrentHashSet<ulong> m_bannedClients;
        private static readonly List<ulong> m_tmpClientList = new List<ulong>();
        private int m_lastKickUpdate;
        private readonly Dictionary<int, ITransportCallback> m_controlMessageHandlers = new Dictionary<int, ITransportCallback>();
        private readonly Dictionary<Type, MyControlMessageEnum> m_controlMessageTypes = new Dictionary<Type, MyControlMessageEnum>();
        private TimeSpan m_lastSentTimeTimestamp;
        private readonly BitStream m_sendPhysicsStream = new BitStream(0x600);
        public const int KICK_TIMEOUT_MS = 0x493e0;
        private float m_serverSimulationRatio = 1f;
        [CompilerGenerated]
        private Action<ulong> ClientJoined;
        [CompilerGenerated]
        private Action<ulong, MyChatMemberStateChangeEnum> ClientLeft;
        [CompilerGenerated]
        private Action HostLeft;
        [CompilerGenerated]
        private Action<ulong, string, ChatChannel, long, string> ChatMessageReceived;
        [CompilerGenerated]
        private Action<string, string, string> ScriptedChatMessageReceived;
        [CompilerGenerated]
        private Action<ulong> ClientKicked;
        public Action<string> ProfilerDone;
        [CompilerGenerated]
        private Action PendingReplicablesDone;
        [CompilerGenerated]
        private Action LocalRespawnRequested;

        public event Action<ulong, string, ChatChannel, long, string> ChatMessageReceived
        {
            [CompilerGenerated] add
            {
                Action<ulong, string, ChatChannel, long, string> chatMessageReceived = this.ChatMessageReceived;
                while (true)
                {
                    Action<ulong, string, ChatChannel, long, string> a = chatMessageReceived;
                    Action<ulong, string, ChatChannel, long, string> action3 = (Action<ulong, string, ChatChannel, long, string>) Delegate.Combine(a, value);
                    chatMessageReceived = Interlocked.CompareExchange<Action<ulong, string, ChatChannel, long, string>>(ref this.ChatMessageReceived, action3, a);
                    if (ReferenceEquals(chatMessageReceived, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<ulong, string, ChatChannel, long, string> chatMessageReceived = this.ChatMessageReceived;
                while (true)
                {
                    Action<ulong, string, ChatChannel, long, string> source = chatMessageReceived;
                    Action<ulong, string, ChatChannel, long, string> action3 = (Action<ulong, string, ChatChannel, long, string>) Delegate.Remove(source, value);
                    chatMessageReceived = Interlocked.CompareExchange<Action<ulong, string, ChatChannel, long, string>>(ref this.ChatMessageReceived, action3, source);
                    if (ReferenceEquals(chatMessageReceived, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<ulong> ClientJoined
        {
            [CompilerGenerated] add
            {
                Action<ulong> clientJoined = this.ClientJoined;
                while (true)
                {
                    Action<ulong> a = clientJoined;
                    Action<ulong> action3 = (Action<ulong>) Delegate.Combine(a, value);
                    clientJoined = Interlocked.CompareExchange<Action<ulong>>(ref this.ClientJoined, action3, a);
                    if (ReferenceEquals(clientJoined, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<ulong> clientJoined = this.ClientJoined;
                while (true)
                {
                    Action<ulong> source = clientJoined;
                    Action<ulong> action3 = (Action<ulong>) Delegate.Remove(source, value);
                    clientJoined = Interlocked.CompareExchange<Action<ulong>>(ref this.ClientJoined, action3, source);
                    if (ReferenceEquals(clientJoined, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<ulong> ClientKicked
        {
            [CompilerGenerated] add
            {
                Action<ulong> clientKicked = this.ClientKicked;
                while (true)
                {
                    Action<ulong> a = clientKicked;
                    Action<ulong> action3 = (Action<ulong>) Delegate.Combine(a, value);
                    clientKicked = Interlocked.CompareExchange<Action<ulong>>(ref this.ClientKicked, action3, a);
                    if (ReferenceEquals(clientKicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<ulong> clientKicked = this.ClientKicked;
                while (true)
                {
                    Action<ulong> source = clientKicked;
                    Action<ulong> action3 = (Action<ulong>) Delegate.Remove(source, value);
                    clientKicked = Interlocked.CompareExchange<Action<ulong>>(ref this.ClientKicked, action3, source);
                    if (ReferenceEquals(clientKicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<ulong, MyChatMemberStateChangeEnum> ClientLeft
        {
            [CompilerGenerated] add
            {
                Action<ulong, MyChatMemberStateChangeEnum> clientLeft = this.ClientLeft;
                while (true)
                {
                    Action<ulong, MyChatMemberStateChangeEnum> a = clientLeft;
                    Action<ulong, MyChatMemberStateChangeEnum> action3 = (Action<ulong, MyChatMemberStateChangeEnum>) Delegate.Combine(a, value);
                    clientLeft = Interlocked.CompareExchange<Action<ulong, MyChatMemberStateChangeEnum>>(ref this.ClientLeft, action3, a);
                    if (ReferenceEquals(clientLeft, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<ulong, MyChatMemberStateChangeEnum> clientLeft = this.ClientLeft;
                while (true)
                {
                    Action<ulong, MyChatMemberStateChangeEnum> source = clientLeft;
                    Action<ulong, MyChatMemberStateChangeEnum> action3 = (Action<ulong, MyChatMemberStateChangeEnum>) Delegate.Remove(source, value);
                    clientLeft = Interlocked.CompareExchange<Action<ulong, MyChatMemberStateChangeEnum>>(ref this.ClientLeft, action3, source);
                    if (ReferenceEquals(clientLeft, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action HostLeft
        {
            [CompilerGenerated] add
            {
                Action hostLeft = this.HostLeft;
                while (true)
                {
                    Action a = hostLeft;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    hostLeft = Interlocked.CompareExchange<Action>(ref this.HostLeft, action3, a);
                    if (ReferenceEquals(hostLeft, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action hostLeft = this.HostLeft;
                while (true)
                {
                    Action source = hostLeft;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    hostLeft = Interlocked.CompareExchange<Action>(ref this.HostLeft, action3, source);
                    if (ReferenceEquals(hostLeft, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action LocalRespawnRequested
        {
            [CompilerGenerated] add
            {
                Action localRespawnRequested = this.LocalRespawnRequested;
                while (true)
                {
                    Action a = localRespawnRequested;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    localRespawnRequested = Interlocked.CompareExchange<Action>(ref this.LocalRespawnRequested, action3, a);
                    if (ReferenceEquals(localRespawnRequested, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action localRespawnRequested = this.LocalRespawnRequested;
                while (true)
                {
                    Action source = localRespawnRequested;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    localRespawnRequested = Interlocked.CompareExchange<Action>(ref this.LocalRespawnRequested, action3, source);
                    if (ReferenceEquals(localRespawnRequested, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action PendingReplicablesDone
        {
            [CompilerGenerated] add
            {
                Action pendingReplicablesDone = this.PendingReplicablesDone;
                while (true)
                {
                    Action a = pendingReplicablesDone;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    pendingReplicablesDone = Interlocked.CompareExchange<Action>(ref this.PendingReplicablesDone, action3, a);
                    if (ReferenceEquals(pendingReplicablesDone, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action pendingReplicablesDone = this.PendingReplicablesDone;
                while (true)
                {
                    Action source = pendingReplicablesDone;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    pendingReplicablesDone = Interlocked.CompareExchange<Action>(ref this.PendingReplicablesDone, action3, source);
                    if (ReferenceEquals(pendingReplicablesDone, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<string, string, string> ScriptedChatMessageReceived
        {
            [CompilerGenerated] add
            {
                Action<string, string, string> scriptedChatMessageReceived = this.ScriptedChatMessageReceived;
                while (true)
                {
                    Action<string, string, string> a = scriptedChatMessageReceived;
                    Action<string, string, string> action3 = (Action<string, string, string>) Delegate.Combine(a, value);
                    scriptedChatMessageReceived = Interlocked.CompareExchange<Action<string, string, string>>(ref this.ScriptedChatMessageReceived, action3, a);
                    if (ReferenceEquals(scriptedChatMessageReceived, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<string, string, string> scriptedChatMessageReceived = this.ScriptedChatMessageReceived;
                while (true)
                {
                    Action<string, string, string> source = scriptedChatMessageReceived;
                    Action<string, string, string> action3 = (Action<string, string, string>) Delegate.Remove(source, value);
                    scriptedChatMessageReceived = Interlocked.CompareExchange<Action<string, string, string>>(ref this.ScriptedChatMessageReceived, action3, source);
                    if (ReferenceEquals(scriptedChatMessageReceived, source))
                    {
                        return;
                    }
                }
            }
        }

        internal MyMultiplayerBase(MySyncLayer syncLayer)
        {
            this.SyncLayer = syncLayer;
            this.IsConnectionDirect = true;
            this.IsConnectionAlive = true;
            this.m_kickedClients = new ConcurrentDictionary<ulong, int>();
            this.m_bannedClients = new MyConcurrentHashSet<ulong>();
            this.m_wrongPasswordClients = new Dictionary<ulong, int>();
            this.m_lastKickUpdate = MySandboxGame.TotalTimeInMilliseconds;
            MyNetworkMonitor.Init();
            MyNetworkReader.SetHandler(0, new NetworkMessageDelegate(this.ControlMessageReceived), new Action<ulong>(this.DisconnectClient));
            this.RegisterControlMessage<MyControlKickClientMsg>(MyControlMessageEnum.Kick, new ControlMessageHandler<MyControlKickClientMsg>(this.OnClientKick), MyMessagePermissions.ToServer | MyMessagePermissions.FromServer);
            this.RegisterControlMessage<MyControlDisconnectedMsg>(MyControlMessageEnum.Disconnected, new ControlMessageHandler<MyControlDisconnectedMsg>(this.OnDisconnectedClient), MyMessagePermissions.ToServer | MyMessagePermissions.FromServer);
            this.RegisterControlMessage<MyControlBanClientMsg>(MyControlMessageEnum.Ban, new ControlMessageHandler<MyControlBanClientMsg>(this.OnClientBan), MyMessagePermissions.ToServer | MyMessagePermissions.FromServer);
            this.RegisterControlMessage<MyControlSendPasswordHashMsg>(MyControlMessageEnum.SendPasswordHash, new ControlMessageHandler<MyControlSendPasswordHashMsg>(this.OnPasswordHash), MyMessagePermissions.ToServer);
            syncLayer.TransportLayer.DisconnectPeerOnError = new Action<ulong>(this.DisconnectClient);
            this.ClientKicked += new Action<ulong>(this.KickClient);
        }

        protected void AddBannedClient(ulong userId)
        {
            if (this.m_bannedClients.Contains(userId))
            {
                MySandboxGame.Log.WriteLine("Trying to ban player who was already banned!");
            }
            else
            {
                this.m_bannedClients.Add(userId);
            }
        }

        protected void AddKickedClient(ulong userId)
        {
            if (!this.m_kickedClients.TryAdd(userId, MySandboxGame.TotalTimeInMilliseconds))
            {
                MySandboxGame.Log.WriteLine("Trying to kick player who was already kicked!");
            }
        }

        protected void AddWrongPasswordClient(ulong userId)
        {
            int num;
            if (!this.m_wrongPasswordClients.TryGetValue(userId, out num))
            {
                this.m_wrongPasswordClients[userId] = 1;
            }
            else
            {
                num++;
                this.m_wrongPasswordClients[userId] = num;
            }
        }

        public abstract void BanClient(ulong userId, bool banned);
        protected void CloseMemberSessions()
        {
            for (int i = 0; i < this.MemberCount; i++)
            {
                ulong memberByIndex = this.GetMemberByIndex(i);
                if ((memberByIndex != Sync.MyId) && (memberByIndex == this.ServerId))
                {
                    MyGameService.Peer2Peer.CloseSession(memberByIndex);
                }
            }
        }

        private void ControlMessageReceived(MyPacket p)
        {
            ITransportCallback callback;
            MyControlMessageEnum enum2 = (MyControlMessageEnum) ((byte) p.ByteStream.ReadUShort());
            if (this.m_controlMessageHandlers.TryGetValue((int) enum2, out callback))
            {
                callback.Receive(p.ByteStream, p.Sender.Id.Value);
            }
            p.Return();
        }

        protected MyClientState CreateClientState() => 
            (Activator.CreateInstance(MyPerGameSettings.ClientStateType) as MyClientState);

        public abstract void DisconnectClient(ulong userId);
        public virtual void Dispose()
        {
            MyNetworkMonitor.Dispose();
            this.m_voxelMapData = null;
            MyNetworkReader.ClearHandler(0);
            this.SyncLayer.TransportLayer.Clear();
            MyNetworkReader.Clear();
            this.m_sendPhysicsStream.Dispose();
            this.ReplicationLayer.Dispose();
            this.ClientKicked -= new Action<ulong>(this.KickClient);
            Sandbox.Engine.Multiplayer.MyMultiplayer.Static = null;
        }

        public virtual void DownloadProfiler()
        {
        }

        public virtual void DownloadWorld()
        {
        }

        public abstract MyLobbyType GetLobbyType();
        public abstract ulong GetMemberByIndex(int memberIndex);
        public abstract string GetMemberName(ulong steamUserID);
        public abstract ulong GetOwner();
        private static string GetPlayerName(long identityId)
        {
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(identityId);
            return ((identity != null) ? identity.DisplayName : identityId.ToString());
        }

        private static string GetPlayerName(ulong steamId)
        {
            long identityId = MySession.Static.Players.TryGetIdentityId(steamId, 0);
            return ((identityId != 0) ? GetPlayerName(identityId) : steamId.ToString());
        }

        [Event(null, 0x388), Reliable, Server]
        public static void InvalidateVoxelCache(string storageName)
        {
            Sandbox.Engine.Multiplayer.MyMultiplayer.GetReplicationServer().InvalidateSingleClientCache(storageName, MyEventContext.Current.Sender);
        }

        public void InvokeLocalRespawnRequested()
        {
            this.LocalRespawnRequested.InvokeIfNotNull();
        }

        protected bool IsClientBanned(ulong userId) => 
            this.m_bannedClients.Contains(userId);

        protected bool IsClientKicked(ulong userId) => 
            this.m_kickedClients.ContainsKey(userId);

        protected bool IsClientKickedOrBanned(ulong userId) => 
            (this.m_kickedClients.ContainsKey(userId) || this.m_bannedClients.Contains(userId));

        protected bool IsOutOfWrongPasswordTries(ulong userId)
        {
            int num;
            return (this.m_wrongPasswordClients.TryGetValue(userId, out num) && (num >= NUMBER_OF_WRONG_PASSWORD_TRIES_BEFORE_KICK));
        }

        private void KickClient(ulong client)
        {
            this.KickClient(client, true, false);
        }

        public abstract void KickClient(ulong userId, bool kicked = true, bool add = true);
        protected virtual void OnAllMembersData(ref AllMembersDataMsg msg)
        {
        }

        [Event(null, 770), Reliable, Client]
        private static void OnAllMembersRecieved(AllMembersDataMsg msg)
        {
            Sandbox.Engine.Multiplayer.MyMultiplayer.Static.OnAllMembersData(ref msg);
        }

        [Event(null, 0xfb), Server, Reliable]
        public static void OnCharacterMaxJetpackDisconnectGridAcceleration(float accel)
        {
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxDisconnectParentAcceleration = accel;
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentAcceleration = Math.Min(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentAcceleration, accel);
        }

        [Event(null, 0xf5), Server, Reliable]
        public static void OnCharacterMaxJetpackGridAcceleration(float accel)
        {
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentAcceleration = accel;
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxDisconnectParentAcceleration = Math.Max(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxDisconnectParentAcceleration, accel);
        }

        [Event(null, 0xe3), Server, Reliable]
        public static void OnCharacterMaxJetpackGridDisconnectDistance(float distance)
        {
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDisconnectDistance = distance;
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDistance = Math.Min(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDistance, distance);
        }

        [Event(null, 0xdd), Server, Reliable]
        public static void OnCharacterMaxJetpackGridDistance(float distance)
        {
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDistance = distance;
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDisconnectDistance = Math.Max(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDisconnectDistance, distance);
        }

        [Event(null, 0xef), Server, Reliable]
        public static void OnCharacterMinJetpackDisconnectGridSpeed(float speed)
        {
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectParentSpeed = speed;
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinParentSpeed = Math.Max(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinParentSpeed, speed);
        }

        [Event(null, 0x107), Server, Reliable]
        public static void OnCharacterMinJetpackDisconnectInsideGridSpeed(float accel)
        {
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectInsideParentSpeed = accel;
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinInsideParentSpeed = Math.Max(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinInsideParentSpeed, accel);
        }

        [Event(null, 0xe9), Server, Reliable]
        public static void OnCharacterMinJetpackGridSpeed(float speed)
        {
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinParentSpeed = speed;
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectParentSpeed = Math.Min(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectParentSpeed, speed);
        }

        [Event(null, 0x101), Server, Reliable]
        public static void OnCharacterMinJetpackInsideGridSpeed(float accel)
        {
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinInsideParentSpeed = accel;
            MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectInsideParentSpeed = Math.Min(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectInsideParentSpeed, accel);
        }

        [Event(null, 0xd8), Server, Reliable]
        public static void OnCharacterParentChangeTimeOut(double delay)
        {
            MyCharacterPhysicsStateGroup.ParentChangeTimeOut = MyTimeSpan.FromMilliseconds(delay);
        }

        public virtual void OnChatMessage(ref ChatMsg msg)
        {
        }

        [Event(null, 0x372), Reliable, BroadcastExcept]
        private static void OnChatMessageRecieved_BroadcastExcept(ChatMsg msg)
        {
            Sandbox.Engine.Multiplayer.MyMultiplayer.Static.OnChatMessage(ref msg);
        }

        [Event(null, 0x308), Reliable, Server]
        private static void OnChatMessageRecieved_Server(ChatMsg msg)
        {
            Vector3D? nullable;
            EndpointId sender = MyEventContext.Current.Sender;
            string playerName = sender.ToString();
            string playerName = msg.TargetId.ToString();
            switch (msg.Channel)
            {
                case 0:
                    playerName = GetPlayerName(MyEventContext.Current.Sender.Value);
                    playerName = "everyone";
                    nullable = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ChatMsg>(s => new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_BroadcastExcept), msg, MyEventContext.Current.Sender, nullable);
                    goto TR_0003;

                case 1:
                    if (msg.TargetId <= 0L)
                    {
                        sender = new EndpointId();
                        nullable = null;
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ChatMsg>(s => new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_BroadcastExcept), msg, sender, nullable);
                    }
                    else
                    {
                        ulong num3 = MySession.Static.Players.TryGetSteamId(msg.TargetId);
                        if (num3 != 0)
                        {
                            playerName = GetPlayerName(MyEventContext.Current.Sender.Value);
                            playerName = GetPlayerName(msg.TargetId);
                            nullable = null;
                            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ChatMsg>(s => new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_SingleTarget), msg, new EndpointId(num3), nullable);
                        }
                    }
                    goto TR_0003;

                case 2:
                {
                    IMyFaction faction = MySession.Static.Factions.TryGetFactionById(msg.TargetId);
                    if (faction == null)
                    {
                        goto TR_0003;
                    }
                    else
                    {
                        playerName = GetPlayerName(MyEventContext.Current.Sender.Value);
                        playerName = faction.Tag;
                        foreach (KeyValuePair<long, MyFactionMember> pair in faction.Members)
                        {
                            if (!MySession.Static.Players.IsPlayerOnline(pair.Value.PlayerId))
                            {
                                continue;
                            }
                            ulong num = MySession.Static.Players.TryGetSteamId(pair.Value.PlayerId);
                            if ((num != 0) && (num != MyEventContext.Current.Sender.Value))
                            {
                                nullable = null;
                                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ChatMsg>(s => new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_SingleTarget), msg, new EndpointId(num), nullable);
                            }
                        }
                        goto TR_0003;
                    }
                    break;
                }
                case 3:
                    break;

                case 4:
                    playerName = GetPlayerName(MyEventContext.Current.Sender.Value);
                    playerName = GetPlayerName(msg.TargetId);
                    goto TR_0003;

                default:
                    goto TR_0003;
            }
            ulong num2 = MySession.Static.Players.TryGetSteamId(msg.TargetId);
            if ((num2 != 0) && (num2 != MyEventContext.Current.Sender.Value))
            {
                playerName = GetPlayerName(MyEventContext.Current.Sender.Value);
                playerName = GetPlayerName(msg.TargetId);
                nullable = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ChatMsg>(s => new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_SingleTarget), msg, new EndpointId(num2), nullable);
            }
        TR_0003:
            Sandbox.Engine.Multiplayer.MyMultiplayer.Static.OnChatMessage(ref msg);
            if (Game.IsDedicated && MySandboxGame.ConfigDedicated.SaveChatToLog)
            {
                StringBuilder builder1 = new StringBuilder();
                MyLog.Default.WriteLine($"CHAT - channel: [{msg.Channel.ToString()}], from [{playerName}] to [{playerName}], message: '{msg.Text}'");
            }
        }

        [Event(null, 0x36c), Reliable, Client]
        private static void OnChatMessageRecieved_SingleTarget(ChatMsg msg)
        {
            Sandbox.Engine.Multiplayer.MyMultiplayer.Static.OnChatMessage(ref msg);
        }

        [Event(null, 0x378), Reliable, Client]
        protected static void OnChatMessageRecieved_ToPlayer(ChatMsg msg)
        {
            Sandbox.Engine.Multiplayer.MyMultiplayer.Static.OnChatMessage(ref msg);
        }

        protected abstract void OnClientBan(ref MyControlBanClientMsg data, ulong sender);
        protected abstract void OnClientKick(ref MyControlKickClientMsg data, ulong sender);
        private void OnDisconnectedClient(ref MyControlDisconnectedMsg data, ulong sender)
        {
            this.RaiseClientLeft(data.Client, MyChatMemberStateChangeEnum.Disconnected);
            MyLog.Default.WriteLineAndConsole("Disconnected: " + sender);
        }

        [Event(null, 0x2f2), Broadcast]
        private static void OnElapsedGameTime(long elapsedGameTicks)
        {
            MySession.Static.ElapsedGameTime = new TimeSpan(elapsedGameTicks);
        }

        protected virtual void OnPasswordHash(ref MyControlSendPasswordHashMsg message, ulong sender)
        {
        }

        protected void OnScriptedChatMessage(ref ScriptedChatMsg msg)
        {
            this.RaiseScriptedChatMessageReceived(msg.Author, msg.Text, msg.Font);
        }

        [Event(null, 0x37e), Reliable, Server, Broadcast]
        private static void OnScriptedChatMessageRecieved(ScriptedChatMsg msg)
        {
            if ((MySession.Static != null) && ((msg.Target == 0) || (MySession.Static.LocalPlayerId == msg.Target)))
            {
                Sandbox.Engine.Multiplayer.MyMultiplayer.Static.OnScriptedChatMessage(ref msg);
            }
        }

        public abstract void OnSessionReady();
        [Event(null, 0xd3), Server, Reliable]
        public static void OnSetDebugEntity(long entityId)
        {
            MyFakes.VDB_ENTITY = MyEntities.GetEntityById(entityId, false);
        }

        [Event(null, 0xce), Server, Reliable]
        public static void OnSetPriorityMultiplier(float priority)
        {
            Sandbox.Engine.Multiplayer.MyMultiplayer.Static.ReplicationLayer.SetPriorityMultiplier(MyEventContext.Current.Sender, priority);
        }

        protected void ProcessAllMembersData(ref AllMembersDataMsg msg)
        {
            Sync.Players.ClearIdentities();
            if (msg.Identities != null)
            {
                Sync.Players.LoadIdentities(msg.Identities);
            }
            Sync.Players.ClearPlayers();
            if (msg.Players != null)
            {
                Sync.Players.LoadPlayers(msg.Players);
            }
            MySession.Static.Factions.LoadFactions(msg.Factions, true);
        }

        protected void RaiseChatMessageReceived(ulong steamUserID, string messageText, ChatChannel channel, long targetId, string customAuthorName = null)
        {
            Action<ulong, string, ChatChannel, long, string> chatMessageReceived = this.ChatMessageReceived;
            if (chatMessageReceived != null)
            {
                chatMessageReceived(steamUserID, messageText, channel, targetId, customAuthorName);
            }
            MyAPIUtilities.Static.RecieveMessage(steamUserID, messageText);
        }

        protected void RaiseClientJoined(ulong changedUser)
        {
            Action<ulong> clientJoined = this.ClientJoined;
            if (clientJoined != null)
            {
                clientJoined(changedUser);
            }
        }

        protected void RaiseClientKicked(ulong user)
        {
            Action<ulong> clientKicked = this.ClientKicked;
            if (clientKicked != null)
            {
                clientKicked(user);
            }
        }

        protected void RaiseClientLeft(ulong changedUser, MyChatMemberStateChangeEnum stateChange)
        {
            Action<ulong, MyChatMemberStateChangeEnum> clientLeft = this.ClientLeft;
            if (clientLeft != null)
            {
                clientLeft(changedUser, stateChange);
            }
        }

        protected void RaiseHostLeft()
        {
            Action hostLeft = this.HostLeft;
            if (hostLeft != null)
            {
                hostLeft();
            }
        }

        private void RaiseScriptedChatMessageReceived(string author, string messageText, string font)
        {
            Action<string, string, string> scriptedChatMessageReceived = this.ScriptedChatMessageReceived;
            if (scriptedChatMessageReceived != null)
            {
                scriptedChatMessageReceived(messageText, author, font);
            }
        }

        public void ReceivePendingReplicablesDone()
        {
            this.PendingReplicablesDone.InvokeIfNotNull();
        }

        internal void RegisterControlMessage<T>(MyControlMessageEnum msg, ControlMessageHandler<T> handler, MyMessagePermissions permission) where T: struct
        {
            MyControlMessageCallback<T> callback = new MyControlMessageCallback<T>(handler, MySyncLayer.GetSerializer<T>(), permission);
            this.m_controlMessageHandlers.Add((int) msg, callback);
            this.m_controlMessageTypes.Add(typeof(T), msg);
        }

        protected void RemoveBannedClient(ulong userId)
        {
            this.m_bannedClients.Remove(userId);
        }

        protected void RemoveKickedClient(ulong userId)
        {
            this.m_kickedClients.Remove<ulong, int>(userId);
        }

        public void ReportReplicatedObjects()
        {
            if (VRage.Profiler.MyRenderProfiler.ProfilerVisible)
            {
                this.ReplicationLayer.ReportReplicatedObjects();
            }
        }

        protected void ResetWrongPasswordCounter(ulong userId)
        {
            if (this.m_wrongPasswordClients.ContainsKey(userId))
            {
                this.m_wrongPasswordClients.Remove(userId);
            }
        }

        protected void SendAllMembersDataToClient(ulong clientId)
        {
            AllMembersDataMsg msg = new AllMembersDataMsg();
            if (Sync.Players != null)
            {
                msg.Identities = Sync.Players.SaveIdentities();
                msg.Players = Sync.Players.SavePlayers();
            }
            if (MySession.Static.Factions != null)
            {
                msg.Factions = MySession.Static.Factions.SaveFactions();
            }
            msg.Clients = MySession.Static.SaveMembers(true);
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<AllMembersDataMsg>(s => new Action<AllMembersDataMsg>(MyMultiplayerBase.OnAllMembersRecieved), msg, new EndpointId(clientId), position);
        }

        protected static void SendChatMessage(ref ChatMsg msg)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ChatMsg>(s => new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_Server), msg, targetEndpoint, position);
        }

        public abstract void SendChatMessage(string text, ChatChannel channel, long targetId = 0L);
        public abstract void SendChatMessageScripted(string text, ChatChannel channel, long targetId = 0L, string customAuthor = null);
        protected void SendControlMessage<T>(ulong user, ref T message, bool reliable = true) where T: struct
        {
            ITransportCallback callback;
            MyControlMessageEnum enum2;
            this.m_controlMessageTypes.TryGetValue(typeof(T), out enum2);
            this.m_controlMessageHandlers.TryGetValue((int) enum2, out callback);
            MyControlMessageCallback<T> callback2 = (MyControlMessageCallback<T>) callback;
            if (MySyncLayer.CheckSendPermissions(user, callback2.Permission))
            {
                MyNetworkWriter.MyPacketDescriptor packet = MyNetworkWriter.GetPacketDescriptor(new EndpointId(user), reliable ? MyP2PMessageEnum.ReliableWithBuffering : MyP2PMessageEnum.Unreliable, 0);
                packet.Header.WriteUShort((ushort) enum2);
                callback2.Write(packet.Header, ref message);
                MyNetworkWriter.SendPacket(packet);
            }
        }

        internal void SendControlMessageToAll<T>(ref T message, ulong exceptUserId = 0UL) where T: struct
        {
            for (int i = 0; i < this.MemberCount; i++)
            {
                ulong memberByIndex = this.GetMemberByIndex(i);
                if ((memberByIndex != Sync.MyId) && (memberByIndex != exceptUserId))
                {
                    this.SendControlMessage<T>(memberByIndex, ref message, true);
                }
            }
        }

        private static void SendElapsedGameTime()
        {
            long ticks = MySession.Static.ElapsedGameTime.Ticks;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MyMultiplayerBase.OnElapsedGameTime), ticks, targetEndpoint, position);
        }

        public static void SendScriptedChatMessage(ref ScriptedChatMsg msg)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<ScriptedChatMsg>(s => new Action<ScriptedChatMsg>(MyMultiplayerBase.OnScriptedChatMessageRecieved), msg, targetEndpoint, position);
        }

        public abstract void SetLobbyType(MyLobbyType type);
        public abstract void SetMemberLimit(int limit);
        public abstract void SetOwner(ulong owner);
        protected void SetReplicationLayer(MyReplicationLayer layer)
        {
            if (this.ReplicationLayer != null)
            {
                throw new InvalidOperationException("Replication layer already set");
            }
            this.ReplicationLayer = layer;
            this.ReplicationLayer.RegisterFromGameAssemblies();
        }

        public void StartProcessingClientMessages()
        {
            this.SyncLayer.TransportLayer.IsBuffering = false;
        }

        public virtual void StartProcessingClientMessagesWithEmptyWorld()
        {
            this.StartProcessingClientMessages();
        }

        public virtual void Tick()
        {
            uint frameCounter = this.FrameCounter;
            this.FrameCounter = frameCounter + 1;
            if (base.IsServer && ((MySession.Static.ElapsedGameTime - this.m_lastSentTimeTimestamp).Seconds > 30))
            {
                this.m_lastSentTimeTimestamp = MySession.Static.ElapsedGameTime;
                SendElapsedGameTime();
            }
            int totalTimeInMilliseconds = MySandboxGame.TotalTimeInMilliseconds;
            if ((totalTimeInMilliseconds - this.m_lastKickUpdate) > 0x4e20)
            {
                m_tmpClientList.Clear();
                foreach (ulong num3 in this.m_kickedClients.Keys)
                {
                    m_tmpClientList.Add(num3);
                }
                foreach (ulong num4 in m_tmpClientList)
                {
                    if ((totalTimeInMilliseconds - this.m_kickedClients[num4]) <= 0x493e0)
                    {
                        continue;
                    }
                    this.m_kickedClients.Remove<ulong, int>(num4);
                    if (this.m_wrongPasswordClients.ContainsKey(num4))
                    {
                        this.m_wrongPasswordClients.Remove(num4);
                    }
                }
                m_tmpClientList.Clear();
                this.m_lastKickUpdate = totalTimeInMilliseconds;
            }
            this.ReplicationLayer.SendUpdate();
        }

        public bool IsServerExperimental { get; protected set; }

        public MyReplicationLayer ReplicationLayer { get; private set; }

        public ConcurrentDictionary<ulong, int> KickedClients =>
            this.m_kickedClients;

        public MyConcurrentHashSet<ulong> BannedClients =>
            this.m_bannedClients;

        public ulong ServerId { get; protected set; }

        public float ServerSimulationRatio
        {
            get => 
                ((float) Math.Round((double) this.m_serverSimulationRatio, 2));
            set => 
                (this.m_serverSimulationRatio = value);
        }

        public LRUCache<string, byte[]> VoxelMapData =>
            this.m_voxelMapData;

        public uint FrameCounter { get; private set; }

        public abstract string WorldName { get; set; }

        public abstract MyGameModeEnum GameMode { get; set; }

        public abstract float InventoryMultiplier { get; set; }

        public abstract float BlocksInventoryMultiplier { get; set; }

        public abstract float AssemblerMultiplier { get; set; }

        public abstract float RefineryMultiplier { get; set; }

        public abstract float WelderMultiplier { get; set; }

        public abstract float GrinderMultiplier { get; set; }

        public abstract string HostName { get; set; }

        public abstract ulong WorldSize { get; set; }

        public abstract int AppVersion { get; set; }

        public abstract string DataHash { get; set; }

        public abstract int MaxPlayers { get; }

        public abstract int ModCount { get; protected set; }

        public abstract List<MyObjectBuilder_Checkpoint.ModItem> Mods { get; set; }

        public abstract int ViewDistance { get; set; }

        public abstract bool Scenario { get; set; }

        public abstract string ScenarioBriefing { get; set; }

        public abstract DateTime ScenarioStartTime { get; set; }

        public virtual int SyncDistance { get; set; }

        public abstract bool ExperimentalMode { get; set; }

        public bool IsConnectionDirect { get; protected set; }

        public bool IsConnectionAlive { get; protected set; }

        public DateTime LastMessageReceived =>
            Sandbox.Engine.Multiplayer.MyMultiplayer.ReplicationLayer.LastMessageFromServer;

        public abstract IEnumerable<ulong> Members { get; }

        public abstract int MemberCount { get; }

        public abstract ulong LobbyId { get; }

        public abstract int MemberLimit { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyMultiplayerBase.<>c <>9 = new MyMultiplayerBase.<>c();
            public static Func<IMyEventOwner, Action<AllMembersDataMsg>> <>9__209_0;
            public static Func<IMyEventOwner, Action<long>> <>9__213_0;
            public static Func<IMyEventOwner, Action<ChatMsg>> <>9__215_0;
            public static Func<IMyEventOwner, Action<ScriptedChatMsg>> <>9__216_0;
            public static Func<IMyEventOwner, Action<ChatMsg>> <>9__218_0;
            public static Func<IMyEventOwner, Action<ChatMsg>> <>9__218_2;
            public static Func<IMyEventOwner, Action<ChatMsg>> <>9__218_3;
            public static Func<IMyEventOwner, Action<ChatMsg>> <>9__218_1;
            public static Func<IMyEventOwner, Action<ChatMsg>> <>9__218_4;

            internal Action<ChatMsg> <OnChatMessageRecieved_Server>b__218_0(IMyEventOwner s) => 
                new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_BroadcastExcept);

            internal Action<ChatMsg> <OnChatMessageRecieved_Server>b__218_1(IMyEventOwner s) => 
                new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_BroadcastExcept);

            internal Action<ChatMsg> <OnChatMessageRecieved_Server>b__218_2(IMyEventOwner s) => 
                new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_SingleTarget);

            internal Action<ChatMsg> <OnChatMessageRecieved_Server>b__218_3(IMyEventOwner s) => 
                new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_SingleTarget);

            internal Action<ChatMsg> <OnChatMessageRecieved_Server>b__218_4(IMyEventOwner s) => 
                new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_SingleTarget);

            internal Action<AllMembersDataMsg> <SendAllMembersDataToClient>b__209_0(IMyEventOwner s) => 
                new Action<AllMembersDataMsg>(MyMultiplayerBase.OnAllMembersRecieved);

            internal Action<ChatMsg> <SendChatMessage>b__215_0(IMyEventOwner s) => 
                new Action<ChatMsg>(MyMultiplayerBase.OnChatMessageRecieved_Server);

            internal Action<long> <SendElapsedGameTime>b__213_0(IMyEventOwner s) => 
                new Action<long>(MyMultiplayerBase.OnElapsedGameTime);

            internal Action<ScriptedChatMsg> <SendScriptedChatMessage>b__216_0(IMyEventOwner s) => 
                new Action<ScriptedChatMsg>(MyMultiplayerBase.OnScriptedChatMessageRecieved);
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct MyConnectedClientData
        {
            public string Name;
            public bool IsAdmin;
        }
    }
}

