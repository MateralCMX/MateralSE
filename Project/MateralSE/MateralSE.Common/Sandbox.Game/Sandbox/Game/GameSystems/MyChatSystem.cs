namespace Sandbox.Game.GameSystems
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems.Chat;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Network;
    using VRageMath;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 900)]
    public class MyChatSystem : MySessionComponentBase
    {
        [CompilerGenerated]
        private Action<long> PlayerMessageReceived;
        [CompilerGenerated]
        private Action<long> FactionMessageReceived;
        public MyChatCommandSystem CommandSystem = new MyChatCommandSystem();
        private ChatChannel m_currentChannel;
        private long m_sourceFactionId;
        private long m_targetPlayerId;
        public MyUnifiedChatHistory ChatHistory = new MyUnifiedChatHistory();
        private int m_frameCount;

        public event Action<long> FactionMessageReceived
        {
            [CompilerGenerated] add
            {
                Action<long> factionMessageReceived = this.FactionMessageReceived;
                while (true)
                {
                    Action<long> a = factionMessageReceived;
                    Action<long> action3 = (Action<long>) Delegate.Combine(a, value);
                    factionMessageReceived = Interlocked.CompareExchange<Action<long>>(ref this.FactionMessageReceived, action3, a);
                    if (ReferenceEquals(factionMessageReceived, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<long> factionMessageReceived = this.FactionMessageReceived;
                while (true)
                {
                    Action<long> source = factionMessageReceived;
                    Action<long> action3 = (Action<long>) Delegate.Remove(source, value);
                    factionMessageReceived = Interlocked.CompareExchange<Action<long>>(ref this.FactionMessageReceived, action3, source);
                    if (ReferenceEquals(factionMessageReceived, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<long> PlayerMessageReceived
        {
            [CompilerGenerated] add
            {
                Action<long> playerMessageReceived = this.PlayerMessageReceived;
                while (true)
                {
                    Action<long> a = playerMessageReceived;
                    Action<long> action3 = (Action<long>) Delegate.Combine(a, value);
                    playerMessageReceived = Interlocked.CompareExchange<Action<long>>(ref this.PlayerMessageReceived, action3, a);
                    if (ReferenceEquals(playerMessageReceived, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<long> playerMessageReceived = this.PlayerMessageReceived;
                while (true)
                {
                    Action<long> source = playerMessageReceived;
                    Action<long> action3 = (Action<long>) Delegate.Remove(source, value);
                    playerMessageReceived = Interlocked.CompareExchange<Action<long>>(ref this.PlayerMessageReceived, action3, source);
                    if (ReferenceEquals(playerMessageReceived, source))
                    {
                        return;
                    }
                }
            }
        }

        public static void AddFactionChatItem(MyUnifiedChatItem chatItem)
        {
            MySession.Static.ChatSystem.ChatHistory.EnqueueMessage(ref chatItem);
        }

        public void ChangeChatChannel_Faction()
        {
            this.m_currentChannel = ChatChannel.Faction;
            this.m_sourceFactionId = 0L;
            this.m_targetPlayerId = 0L;
        }

        public void ChangeChatChannel_Global()
        {
            this.m_currentChannel = ChatChannel.Global;
            this.m_sourceFactionId = 0L;
            this.m_targetPlayerId = 0L;
        }

        public void ChangeChatChannel_Whisper(long playerId)
        {
            this.m_currentChannel = ChatChannel.Private;
            this.m_sourceFactionId = 0L;
            this.m_targetPlayerId = playerId;
        }

        public static Color GetChannelColor(ChatChannel channel)
        {
            switch (channel)
            {
                case ChatChannel.Global:
                case ChatChannel.GlobalScripted:
                case ChatChannel.ChatBot:
                    return Color.White;

                case ChatChannel.Faction:
                    return Color.LimeGreen;

                case ChatChannel.Private:
                    return Color.Violet;
            }
            throw new ArgumentOutOfRangeException();
        }

        public static Color GetRelationColor(long identityId)
        {
            if (MySession.Static.IsUserAdmin(MySession.Static.Players.TryGetSteamId(identityId)))
            {
                return Color.Purple;
            }
            switch (MyPlayer.GetRelationsBetweenPlayers(MySession.Static.LocalPlayerId, identityId))
            {
                case MyRelationsBetweenPlayers.Self:
                    return Color.CornflowerBlue;

                case MyRelationsBetweenPlayers.Allies:
                    return Color.LightGreen;

                case MyRelationsBetweenPlayers.Neutral:
                    return Color.PaleGoldenrod;

                case MyRelationsBetweenPlayers.Enemies:
                    return Color.Crimson;
            }
            throw new ArgumentOutOfRangeException();
        }

        [Event(null, 0x7e), Reliable, Server(ValidationType.Controlled)]
        private static void OnFactionMessageRequest(MyUnifiedChatItem item)
        {
            if ((item.Text.Length != 0) && (item.Text.Length <= 200))
            {
                IMyFaction faction = MySession.Static.Factions.TryGetFactionById(item.TargetId);
                if ((MySession.Static.Players.TryGetIdentity(item.SenderId) != null) && (faction != null))
                {
                    MyPlayer.PlayerId id;
                    Vector3D? nullable;
                    bool flag = false;
                    ulong steamId = 0UL;
                    if (!faction.IsMember(item.SenderId) && MySession.Static.Players.TryGetPlayerId(item.SenderId, out id))
                    {
                        steamId = id.SteamId;
                        flag |= MySession.Static.IsUserAdmin(steamId);
                    }
                    foreach (KeyValuePair<long, MyFactionMember> pair in faction.Members)
                    {
                        MyPlayer.PlayerId id2;
                        MySession.Static.Players.TryGetPlayerId(pair.Value.PlayerId, out id2);
                        ulong num2 = id2.SteamId;
                        if (num2 != 0)
                        {
                            nullable = null;
                            MyMultiplayer.RaiseStaticEvent<MyUnifiedChatItem>(x => new Action<MyUnifiedChatItem>(MyChatSystem.OnFactionMessageSuccess), item, new EndpointId(num2), nullable);
                        }
                    }
                    if (flag)
                    {
                        nullable = null;
                        MyMultiplayer.RaiseStaticEvent<MyUnifiedChatItem>(x => new Action<MyUnifiedChatItem>(MyChatSystem.OnFactionMessageSuccess), item, new EndpointId(steamId), nullable);
                    }
                }
            }
        }

        [Event(null, 0xa9), Reliable, Client]
        private static void OnFactionMessageSuccess(MyUnifiedChatItem item)
        {
            long senderId = item.SenderId;
            if (!Sync.IsServer || (senderId == MySession.Static.LocalPlayerId))
            {
                AddFactionChatItem(item);
                if (MyMultiplayer.Static != null)
                {
                    ChatMsg msg = new ChatMsg {
                        Text = item.Text,
                        Author = MySession.Static.Players.TryGetSteamId(item.SenderId),
                        Channel = (byte) item.Channel,
                        TargetId = item.TargetId,
                        CustomAuthorName = string.Empty
                    };
                    MyMultiplayer.Static.OnChatMessage(ref msg);
                }
            }
            if (senderId != MySession.Static.LocalPlayerId)
            {
                MySession.Static.Gpss.ScanText(item.Text, MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_NewFromFactionComms));
            }
            MySession.Static.ChatSystem.OnNewFactionMessage(ref item);
        }

        public void OnNewFactionMessage(ref MyUnifiedChatItem item)
        {
            IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(MySession.Static.LocalPlayerId);
            if (!Sync.IsDedicated && ((faction != null) || !MySession.Static.IsUserAdmin(Sync.MyId)))
            {
                Action<long> factionMessageReceived = this.FactionMessageReceived;
                if (factionMessageReceived != null)
                {
                    factionMessageReceived(item.TargetId);
                }
            }
        }

        public void SendNewFactionMessage(MyUnifiedChatItem chatItem)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<MyUnifiedChatItem>(x => new Action<MyUnifiedChatItem>(MyChatSystem.OnFactionMessageRequest), chatItem, targetEndpoint, position);
        }

        private void ShowNewMessageHudNotification(MyHudNotification notification)
        {
            MyHud.Notifications.Add(notification);
        }

        public override void UpdateAfterSimulation()
        {
        }

        public ChatChannel CurrentChannel =>
            this.m_currentChannel;

        public long CurrentTarget
        {
            get
            {
                switch (this.m_currentChannel)
                {
                    case ChatChannel.Global:
                        return 0L;

                    case ChatChannel.Faction:
                    {
                        IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(MySession.Static.LocalPlayerId);
                        return ((faction != null) ? faction.FactionId : 0L);
                    }
                    case ChatChannel.Private:
                        return this.m_targetPlayerId;
                }
                return 0L;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyChatSystem.<>c <>9 = new MyChatSystem.<>c();
            public static Func<IMyEventOwner, Action<MyUnifiedChatItem>> <>9__24_0;
            public static Func<IMyEventOwner, Action<MyUnifiedChatItem>> <>9__25_1;
            public static Func<IMyEventOwner, Action<MyUnifiedChatItem>> <>9__25_0;

            internal Action<MyUnifiedChatItem> <OnFactionMessageRequest>b__25_0(IMyEventOwner x) => 
                new Action<MyUnifiedChatItem>(MyChatSystem.OnFactionMessageSuccess);

            internal Action<MyUnifiedChatItem> <OnFactionMessageRequest>b__25_1(IMyEventOwner x) => 
                new Action<MyUnifiedChatItem>(MyChatSystem.OnFactionMessageSuccess);

            internal Action<MyUnifiedChatItem> <SendNewFactionMessage>b__24_0(IMyEventOwner x) => 
                new Action<MyUnifiedChatItem>(MyChatSystem.OnFactionMessageRequest);
        }
    }
}

