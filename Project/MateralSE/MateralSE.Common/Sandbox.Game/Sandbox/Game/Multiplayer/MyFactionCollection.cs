namespace Sandbox.Game.Multiplayer
{
    using ProtoBuf;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.World;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Serialization;

    [StaticEventOwner]
    public class MyFactionCollection : IEnumerable<KeyValuePair<long, MyFaction>>, IEnumerable, IMyFactionCollection
    {
        private Dictionary<long, MyFaction> m_factions = new Dictionary<long, MyFaction>();
        private Dictionary<string, MyFaction> m_factionsByTag = new Dictionary<string, MyFaction>();
        private Dictionary<long, HashSet<long>> m_factionRequests = new Dictionary<long, HashSet<long>>();
        private Dictionary<MyFactionPair, MyRelationsBetweenFactions> m_relationsBetweenFactions = new Dictionary<MyFactionPair, MyRelationsBetweenFactions>(MyFactionPair.Comparer);
        private Dictionary<long, long> m_playerFaction = new Dictionary<long, long>();
        [CompilerGenerated]
        private Action<MyFaction, long> OnPlayerJoined;
        [CompilerGenerated]
        private Action<MyFaction, long> OnPlayerLeft;
        [CompilerGenerated]
        private Action<MyFactionStateChange, long, long, long, long> FactionStateChanged;
        [CompilerGenerated]
        private Action<long, bool, bool> FactionAutoAcceptChanged;
        [CompilerGenerated]
        private Action<long> FactionEdited;
        [CompilerGenerated]
        private Action<long> FactionCreated;

        public event Action<long, bool, bool> FactionAutoAcceptChanged
        {
            [CompilerGenerated] add
            {
                Action<long, bool, bool> factionAutoAcceptChanged = this.FactionAutoAcceptChanged;
                while (true)
                {
                    Action<long, bool, bool> a = factionAutoAcceptChanged;
                    Action<long, bool, bool> action3 = (Action<long, bool, bool>) Delegate.Combine(a, value);
                    factionAutoAcceptChanged = Interlocked.CompareExchange<Action<long, bool, bool>>(ref this.FactionAutoAcceptChanged, action3, a);
                    if (ReferenceEquals(factionAutoAcceptChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<long, bool, bool> factionAutoAcceptChanged = this.FactionAutoAcceptChanged;
                while (true)
                {
                    Action<long, bool, bool> source = factionAutoAcceptChanged;
                    Action<long, bool, bool> action3 = (Action<long, bool, bool>) Delegate.Remove(source, value);
                    factionAutoAcceptChanged = Interlocked.CompareExchange<Action<long, bool, bool>>(ref this.FactionAutoAcceptChanged, action3, source);
                    if (ReferenceEquals(factionAutoAcceptChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<long> FactionCreated
        {
            [CompilerGenerated] add
            {
                Action<long> factionCreated = this.FactionCreated;
                while (true)
                {
                    Action<long> a = factionCreated;
                    Action<long> action3 = (Action<long>) Delegate.Combine(a, value);
                    factionCreated = Interlocked.CompareExchange<Action<long>>(ref this.FactionCreated, action3, a);
                    if (ReferenceEquals(factionCreated, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<long> factionCreated = this.FactionCreated;
                while (true)
                {
                    Action<long> source = factionCreated;
                    Action<long> action3 = (Action<long>) Delegate.Remove(source, value);
                    factionCreated = Interlocked.CompareExchange<Action<long>>(ref this.FactionCreated, action3, source);
                    if (ReferenceEquals(factionCreated, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<long> FactionEdited
        {
            [CompilerGenerated] add
            {
                Action<long> factionEdited = this.FactionEdited;
                while (true)
                {
                    Action<long> a = factionEdited;
                    Action<long> action3 = (Action<long>) Delegate.Combine(a, value);
                    factionEdited = Interlocked.CompareExchange<Action<long>>(ref this.FactionEdited, action3, a);
                    if (ReferenceEquals(factionEdited, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<long> factionEdited = this.FactionEdited;
                while (true)
                {
                    Action<long> source = factionEdited;
                    Action<long> action3 = (Action<long>) Delegate.Remove(source, value);
                    factionEdited = Interlocked.CompareExchange<Action<long>>(ref this.FactionEdited, action3, source);
                    if (ReferenceEquals(factionEdited, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyFactionStateChange, long, long, long, long> FactionStateChanged
        {
            [CompilerGenerated] add
            {
                Action<MyFactionStateChange, long, long, long, long> factionStateChanged = this.FactionStateChanged;
                while (true)
                {
                    Action<MyFactionStateChange, long, long, long, long> a = factionStateChanged;
                    Action<MyFactionStateChange, long, long, long, long> action3 = (Action<MyFactionStateChange, long, long, long, long>) Delegate.Combine(a, value);
                    factionStateChanged = Interlocked.CompareExchange<Action<MyFactionStateChange, long, long, long, long>>(ref this.FactionStateChanged, action3, a);
                    if (ReferenceEquals(factionStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyFactionStateChange, long, long, long, long> factionStateChanged = this.FactionStateChanged;
                while (true)
                {
                    Action<MyFactionStateChange, long, long, long, long> source = factionStateChanged;
                    Action<MyFactionStateChange, long, long, long, long> action3 = (Action<MyFactionStateChange, long, long, long, long>) Delegate.Remove(source, value);
                    factionStateChanged = Interlocked.CompareExchange<Action<MyFactionStateChange, long, long, long, long>>(ref this.FactionStateChanged, action3, source);
                    if (ReferenceEquals(factionStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyFaction, long> OnPlayerJoined
        {
            [CompilerGenerated] add
            {
                Action<MyFaction, long> onPlayerJoined = this.OnPlayerJoined;
                while (true)
                {
                    Action<MyFaction, long> a = onPlayerJoined;
                    Action<MyFaction, long> action3 = (Action<MyFaction, long>) Delegate.Combine(a, value);
                    onPlayerJoined = Interlocked.CompareExchange<Action<MyFaction, long>>(ref this.OnPlayerJoined, action3, a);
                    if (ReferenceEquals(onPlayerJoined, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyFaction, long> onPlayerJoined = this.OnPlayerJoined;
                while (true)
                {
                    Action<MyFaction, long> source = onPlayerJoined;
                    Action<MyFaction, long> action3 = (Action<MyFaction, long>) Delegate.Remove(source, value);
                    onPlayerJoined = Interlocked.CompareExchange<Action<MyFaction, long>>(ref this.OnPlayerJoined, action3, source);
                    if (ReferenceEquals(onPlayerJoined, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyFaction, long> OnPlayerLeft
        {
            [CompilerGenerated] add
            {
                Action<MyFaction, long> onPlayerLeft = this.OnPlayerLeft;
                while (true)
                {
                    Action<MyFaction, long> a = onPlayerLeft;
                    Action<MyFaction, long> action3 = (Action<MyFaction, long>) Delegate.Combine(a, value);
                    onPlayerLeft = Interlocked.CompareExchange<Action<MyFaction, long>>(ref this.OnPlayerLeft, action3, a);
                    if (ReferenceEquals(onPlayerLeft, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyFaction, long> onPlayerLeft = this.OnPlayerLeft;
                while (true)
                {
                    Action<MyFaction, long> source = onPlayerLeft;
                    Action<MyFaction, long> action3 = (Action<MyFaction, long>) Delegate.Remove(source, value);
                    onPlayerLeft = Interlocked.CompareExchange<Action<MyFaction, long>>(ref this.OnPlayerLeft, action3, source);
                    if (ReferenceEquals(onPlayerLeft, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<long, bool, bool> IMyFactionCollection.FactionAutoAcceptChanged
        {
            add
            {
                this.FactionAutoAcceptChanged += value;
            }
            remove
            {
                this.FactionAutoAcceptChanged -= value;
            }
        }

        event Action<long> IMyFactionCollection.FactionCreated
        {
            add
            {
                this.FactionCreated += value;
            }
            remove
            {
                this.FactionCreated -= value;
            }
        }

        event Action<long> IMyFactionCollection.FactionEdited
        {
            add
            {
                this.FactionEdited += value;
            }
            remove
            {
                this.FactionEdited -= value;
            }
        }

        event Action<MyFactionStateChange, long, long, long, long> IMyFactionCollection.FactionStateChanged
        {
            add
            {
                this.FactionStateChanged += value;
            }
            remove
            {
                this.FactionStateChanged -= value;
            }
        }

        public static void AcceptJoin(long factionId, long playerId)
        {
            SendFactionChange(MyFactionStateChange.FactionMemberAcceptJoin, factionId, factionId, playerId);
        }

        public static void AcceptPeace(long fromFactionId, long toFactionId)
        {
            SendFactionChange(MyFactionStateChange.AcceptPeace, fromFactionId, toFactionId, 0L);
        }

        private void Add(MyFaction faction)
        {
            this.m_factions.Add(faction.FactionId, faction);
            this.RegisterFactionTag(faction);
        }

        public void AddNewNPCToFaction(long factionId)
        {
            string name = this.m_factions[factionId].Tag + " NPC" + MyRandom.Instance.Next(0x3e8, 0x270f);
            Vector3? colorMask = null;
            MyIdentity identity = Sync.Players.CreateNewIdentity(name, null, colorMask, false);
            Sync.Players.MarkIdentityAsNPC(identity.IdentityId);
            this.AddPlayerToFaction(identity.IdentityId, factionId);
        }

        public void AddPlayerToFaction(long playerId, long factionId)
        {
            MyFaction faction;
            if (this.m_factions.TryGetValue(factionId, out faction))
            {
                faction.AcceptJoin(playerId, true);
            }
            else
            {
                this.AddPlayerToFactionInternal(playerId, factionId);
            }
            foreach (KeyValuePair<long, MyFaction> pair in this.m_factions)
            {
                pair.Value.CancelJoinRequest(playerId);
            }
        }

        internal void AddPlayerToFactionInternal(long playerId, long factionId)
        {
            this.m_playerFaction[playerId] = factionId;
        }

        private static void AfterFactionCreated(long founderId, long factionId)
        {
            foreach (KeyValuePair<long, MyFaction> pair in MySession.Static.Factions)
            {
                pair.Value.CancelJoinRequest(founderId);
            }
            Action<long> factionCreated = MySession.Static.Factions.FactionCreated;
            if (factionCreated != null)
            {
                factionCreated(factionId);
            }
        }

        private void ApplyFactionStateChange(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
        {
            switch (action)
            {
                case MyFactionStateChange.RemoveFaction:
                {
                    if (this.m_factions[fromFactionId].IsMember(MySession.Static.LocalPlayerId))
                    {
                        this.m_playerFaction.Remove(playerId);
                    }
                    foreach (KeyValuePair<long, MyFaction> pair in this.m_factions)
                    {
                        if (pair.Key != fromFactionId)
                        {
                            this.ClearRequest(fromFactionId, pair.Key);
                            this.RemoveRelation(fromFactionId, pair.Key);
                        }
                    }
                    MyFaction faction = null;
                    this.m_factions.TryGetValue(fromFactionId, out faction);
                    this.UnregisterFactionTag(faction);
                    this.m_factions.Remove(fromFactionId);
                    return;
                }
                case MyFactionStateChange.SendPeaceRequest:
                    HashSet<long> set;
                    if (this.m_factionRequests.TryGetValue(fromFactionId, out set))
                    {
                        set.Add(toFactionId);
                        return;
                    }
                    set = new HashSet<long> {
                        toFactionId
                    };
                    this.m_factionRequests.Add(fromFactionId, set);
                    return;

                case MyFactionStateChange.CancelPeaceRequest:
                    this.ClearRequest(fromFactionId, toFactionId);
                    return;

                case MyFactionStateChange.AcceptPeace:
                    this.ClearRequest(fromFactionId, toFactionId);
                    this.ChangeRelation(fromFactionId, toFactionId, MyRelationsBetweenFactions.Neutral);
                    return;

                case MyFactionStateChange.DeclareWar:
                    this.ClearRequest(fromFactionId, toFactionId);
                    this.ChangeRelation(fromFactionId, toFactionId, MyRelationsBetweenFactions.Enemies);
                    return;

                case MyFactionStateChange.FactionMemberSendJoin:
                    this.m_factions[fromFactionId].AddJoinRequest(playerId);
                    return;

                case MyFactionStateChange.FactionMemberCancelJoin:
                    this.m_factions[fromFactionId].CancelJoinRequest(playerId);
                    return;

                case MyFactionStateChange.FactionMemberAcceptJoin:
                {
                    int num1;
                    ulong steamId = MySession.Static.Players.TryGetSteamId(senderId);
                    if (!MySession.Static.IsUserSpaceMaster(steamId))
                    {
                        num1 = (int) (this.m_factions[fromFactionId].Members.Count == 0);
                    }
                    else
                    {
                        num1 = 1;
                    }
                    bool autoaccept = (bool) num1;
                    if (!autoaccept || !this.m_factions[fromFactionId].IsEveryoneNpc())
                    {
                        this.m_factions[fromFactionId].AcceptJoin(playerId, autoaccept);
                        return;
                    }
                    this.m_factions[fromFactionId].AcceptJoin(playerId, autoaccept);
                    this.m_factions[fromFactionId].PromoteMember(playerId);
                    return;
                }
                case MyFactionStateChange.FactionMemberKick:
                    if (Sync.IsServer && (playerId != this.m_factions[fromFactionId].FounderId))
                    {
                        MyBlockLimits.TransferBlockLimits(playerId, this.m_factions[fromFactionId].FounderId);
                    }
                    this.m_factions[fromFactionId].KickMember(playerId, true);
                    return;

                case MyFactionStateChange.FactionMemberPromote:
                    this.m_factions[fromFactionId].PromoteMember(playerId);
                    return;

                case MyFactionStateChange.FactionMemberDemote:
                    this.m_factions[fromFactionId].DemoteMember(playerId);
                    return;

                case MyFactionStateChange.FactionMemberLeave:
                    this.m_factions[fromFactionId].KickMember(playerId, true);
                    return;
            }
        }

        public bool AreFactionsEnemies(long factionId1, long factionId2) => 
            (this.GetRelationBetweenFactions(factionId1, factionId2) == MyRelationsBetweenFactions.Enemies);

        public static void CancelJoinRequest(long factionId, long playerId)
        {
            SendFactionChange(MyFactionStateChange.FactionMemberCancelJoin, factionId, factionId, playerId);
        }

        public static void CancelPeaceRequest(long fromFactionId, long toFactionId)
        {
            SendFactionChange(MyFactionStateChange.CancelPeaceRequest, fromFactionId, toFactionId, 0L);
        }

        public void ChangeAutoAccept(long factionId, long playerId, bool autoAcceptMember, bool autoAcceptPeace)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, long, bool, bool>(s => new Action<long, long, bool, bool>(MyFactionCollection.ChangeAutoAcceptRequest), factionId, playerId, autoAcceptMember, autoAcceptPeace, targetEndpoint, position);
        }

        [Event(null, 0x2f0), Reliable, Server]
        private static void ChangeAutoAcceptRequest(long factionId, long playerId, bool autoAcceptMember, bool autoAcceptPeace)
        {
            IMyFaction faction = MySession.Static.Factions.TryGetFactionById(factionId);
            ulong steamId = MySession.Static.Players.TryGetSteamId(playerId);
            if ((faction != null) && (faction.IsLeader(playerId) || ((steamId != 0) && MySession.Static.IsUserAdmin(steamId))))
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, bool, bool>(s => new Action<long, bool, bool>(MyFactionCollection.ChangeAutoAcceptSuccess), factionId, autoAcceptMember, autoAcceptPeace, targetEndpoint, position);
            }
            else if (!MyEventContext.Current.IsLocallyInvoked)
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
        }

        [Event(null, 0x300), Reliable, ServerInvoked, Broadcast]
        private static void ChangeAutoAcceptSuccess(long factionId, bool autoAcceptMember, bool autoAcceptPeace)
        {
            MySession.Static.Factions[factionId].AutoAcceptMember = autoAcceptMember;
            MySession.Static.Factions[factionId].AutoAcceptPeace = autoAcceptPeace;
            Action<long, bool, bool> factionAutoAcceptChanged = MySession.Static.Factions.FactionAutoAcceptChanged;
            if (factionAutoAcceptChanged != null)
            {
                factionAutoAcceptChanged(factionId, autoAcceptMember, autoAcceptPeace);
            }
        }

        private void ChangeRelation(long fromFactionId, long toFactionId, MyRelationsBetweenFactions relation)
        {
            this.m_relationsBetweenFactions[new MyFactionPair(fromFactionId, toFactionId)] = relation;
        }

        private bool CheckFactionStateChange(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
        {
            if (Sync.IsServer)
            {
                HashSet<long> set;
                if (!this.m_factions.ContainsKey(fromFactionId) || !this.m_factions.ContainsKey(toFactionId))
                {
                    return false;
                }
                MyFactionPair pair1 = new MyFactionPair(fromFactionId, toFactionId);
                if (((senderId != 0) && (((action <= MyFactionStateChange.DeclareWar) || ((action - 7) <= MyFactionStateChange.AcceptPeace)) && !this.m_factions[fromFactionId].IsLeader(senderId))) && !MySession.Static.IsUserAdmin(MySession.Static.Players.TryGetSteamId(senderId)))
                {
                    return false;
                }
                switch (action)
                {
                    case MyFactionStateChange.RemoveFaction:
                        return true;

                    case MyFactionStateChange.SendPeaceRequest:
                        return ((!this.m_factionRequests.TryGetValue(fromFactionId, out set) || !set.Contains(toFactionId)) && (this.GetRelationBetweenFactions(fromFactionId, toFactionId) == MyRelationsBetweenFactions.Enemies));

                    case MyFactionStateChange.CancelPeaceRequest:
                        return (this.m_factionRequests.TryGetValue(fromFactionId, out set) && set.Contains(toFactionId));

                    case MyFactionStateChange.AcceptPeace:
                        return (this.GetRelationBetweenFactions(fromFactionId, toFactionId) != MyRelationsBetweenFactions.Neutral);

                    case MyFactionStateChange.DeclareWar:
                        return (this.GetRelationBetweenFactions(fromFactionId, toFactionId) != MyRelationsBetweenFactions.Enemies);

                    case MyFactionStateChange.FactionMemberSendJoin:
                        if (MySession.Static.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.PER_FACTION)
                        {
                            return !this.m_factions[fromFactionId].JoinRequests.ContainsKey(playerId);
                        }
                        return (!this.m_factions[fromFactionId].IsMember(playerId) && !this.m_factions[fromFactionId].JoinRequests.ContainsKey(playerId));

                    case MyFactionStateChange.FactionMemberCancelJoin:
                        return (!this.m_factions[fromFactionId].IsMember(playerId) && this.m_factions[fromFactionId].JoinRequests.ContainsKey(playerId));

                    case MyFactionStateChange.FactionMemberAcceptJoin:
                        return this.m_factions[fromFactionId].JoinRequests.ContainsKey(playerId);

                    case MyFactionStateChange.FactionMemberKick:
                        return this.m_factions[fromFactionId].IsMember(playerId);

                    case MyFactionStateChange.FactionMemberPromote:
                        return this.m_factions[fromFactionId].IsMember(playerId);

                    case MyFactionStateChange.FactionMemberDemote:
                        return this.m_factions[fromFactionId].IsLeader(playerId);

                    case MyFactionStateChange.FactionMemberLeave:
                        return this.m_factions[fromFactionId].IsMember(playerId);
                }
            }
            return false;
        }

        private void ClearRequest(long fromFactionId, long toFactionId)
        {
            if (this.m_factionRequests.ContainsKey(fromFactionId))
            {
                this.m_factionRequests[fromFactionId].Remove(toFactionId);
            }
            if (this.m_factionRequests.ContainsKey(toFactionId))
            {
                this.m_factionRequests[toFactionId].Remove(fromFactionId);
            }
        }

        public bool Contains(long factionId) => 
            this.m_factions.ContainsKey(factionId);

        public void CreateDefaultFactions()
        {
            foreach (MyFactionDefinition definition in MyDefinitionManager.Static.GetDefaultFactions())
            {
                if (this.TryGetFactionByTag(definition.Tag, null) != null)
                {
                    continue;
                }
                Vector3? colorMask = null;
                MyIdentity identity = Sync.Players.CreateNewIdentity(definition.Founder, null, colorMask, false);
                if (identity != null)
                {
                    Sync.Players.MarkIdentityAsNPC(identity.IdentityId);
                    long factionId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.FACTION, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                    if (!CreateFactionInternal(identity.IdentityId, factionId, definition))
                    {
                        MyPlayer.PlayerId playerId = new MyPlayer.PlayerId();
                        Sync.Players.RemoveIdentity(identity.IdentityId, playerId);
                    }
                }
            }
        }

        public void CreateFaction(long founderId, string tag, string name, string desc, string privateInfo)
        {
            this.SendCreateFaction(founderId, tag, name, desc, privateInfo);
        }

        [Event(null, 0xba), Reliable, Server]
        public static void CreateFactionByDefinition(string tag)
        {
            string key = tag.ToUpperInvariant();
            if (!MySession.Static.Factions.m_factionsByTag.ContainsKey(key))
            {
                MyFactionDefinition factionDef = MyDefinitionManager.Static.TryGetFactionDefinition(key);
                if (factionDef != null)
                {
                    Vector3? colorMask = null;
                    MyIdentity identity = Sync.Players.CreateNewIdentity(factionDef.Founder, null, colorMask, false);
                    Sync.Players.MarkIdentityAsNPC(identity.IdentityId);
                    MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.FACTION, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                    CreateFactionServer(identity.IdentityId, key, factionDef.DisplayNameText, factionDef.DescriptionText, "", factionDef);
                }
            }
        }

        private static bool CreateFactionInternal(long founderId, long factionId, MyFactionDefinition factionDef)
        {
            if (MySession.Static.Factions.Contains(factionId))
            {
                return false;
            }
            if ((MySession.Static.MaxFactionsCount > 0) && (MySession.Static.Factions.HumansCount() >= MySession.Static.MaxFactionsCount))
            {
                return false;
            }
            MyFaction faction = new MyFaction(factionId, factionDef.Tag, factionDef.DisplayNameText, factionDef.DescriptionText, "", founderId);
            MySession.Static.Factions.Add(faction);
            MySession.Static.Factions.AddPlayerToFaction(founderId, factionId);
            faction.AcceptHumans = factionDef.AcceptHumans;
            faction.AutoAcceptMember = factionDef.AutoAcceptMember;
            faction.EnableFriendlyFire = factionDef.EnableFriendlyFire;
            AfterFactionCreated(founderId, factionId);
            return true;
        }

        private static bool CreateFactionInternal(long founderId, long factionId, string factionTag, string factionName, string factionDescription, string factionPrivateInfo)
        {
            if ((MySession.Static.MaxFactionsCount > 0) && (MySession.Static.Factions.HumansCount() >= MySession.Static.MaxFactionsCount))
            {
                return false;
            }
            MySession.Static.Factions.AddPlayerToFaction(founderId, factionId);
            MySession.Static.Factions.Add(new MyFaction(factionId, factionTag, factionName, factionDescription, factionPrivateInfo, founderId));
            AfterFactionCreated(founderId, factionId);
            return true;
        }

        [Event(null, 0x363), Reliable, Server]
        private static void CreateFactionRequest(AddFactionMsg msg)
        {
            if ((MySession.Static.MaxFactionsCount == 0) || ((MySession.Static.MaxFactionsCount > 0) && (MySession.Static.Factions.HumansCount() < MySession.Static.MaxFactionsCount)))
            {
                CreateFactionServer(msg.FounderId, msg.FactionTag, msg.FactionName, msg.FactionDescription, msg.FactionPrivateInfo, null);
            }
            else if (!MyEventContext.Current.IsLocallyInvoked)
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
        }

        private static void CreateFactionServer(long founderId, string factionTag, string factionName, string description, string privateInfo, MyFactionDefinition factionDef = null)
        {
            if (Sync.IsServer)
            {
                long factionId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.FACTION, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                IMyFaction faction = MySession.Static.Factions.TryGetFactionById(factionId);
                if (((MySession.Static.Factions.TryGetPlayerFaction(founderId) == null) && ((faction == null) && (!MySession.Static.Factions.FactionTagExists(factionTag, null) && !MySession.Static.Factions.FactionNameExists(factionName, null)))) && Sync.Players.HasIdentity(founderId))
                {
                    bool flag = factionDef != null;
                    if (!flag ? CreateFactionInternal(founderId, factionId, factionTag, factionName, description, privateInfo) : CreateFactionInternal(founderId, factionId, factionDef))
                    {
                        AddFactionMsg msg = new AddFactionMsg {
                            FactionId = factionId,
                            FounderId = founderId,
                            FactionTag = factionTag,
                            FactionName = factionName,
                            FactionDescription = description,
                            FactionPrivateInfo = privateInfo,
                            CreateFromDefinition = flag
                        };
                        EndpointId targetEndpoint = new EndpointId();
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<AddFactionMsg>(x => new Action<AddFactionMsg>(MyFactionCollection.CreateFactionSuccess), msg, targetEndpoint, position);
                        SetDefaultFactionStates(factionId);
                        targetEndpoint = new EndpointId();
                        position = null;
                        MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(MyFactionCollection.SetDefaultFactionStates), factionId, targetEndpoint, position);
                    }
                }
            }
        }

        [Event(null, 0x3a0), Reliable, Broadcast]
        private static void CreateFactionSuccess(AddFactionMsg msg)
        {
            if (!msg.CreateFromDefinition)
            {
                CreateFactionInternal(msg.FounderId, msg.FactionId, msg.FactionTag, msg.FactionName, msg.FactionDescription, msg.FactionPrivateInfo);
            }
            else
            {
                MyFactionDefinition factionDef = MyDefinitionManager.Static.TryGetFactionDefinition(msg.FactionTag);
                if (factionDef != null)
                {
                    CreateFactionInternal(msg.FounderId, msg.FactionId, factionDef);
                }
            }
        }

        public void CreateNPCFaction(string tag, string name, string desc, string privateInfo)
        {
            string str = tag + " NPC" + MyRandom.Instance.Next(0x3e8, 0x270f);
            Vector3? colorMask = null;
            MyIdentity identity = Sync.Players.CreateNewIdentity(str, null, colorMask, false);
            Sync.Players.MarkIdentityAsNPC(identity.IdentityId);
            this.SendCreateFaction(identity.IdentityId, tag, name, desc, privateInfo);
        }

        public static void DeclareWar(long fromFactionId, long toFactionId)
        {
            SendFactionChange(MyFactionStateChange.DeclareWar, fromFactionId, toFactionId, 0L);
        }

        public static void DemoteMember(long factionId, long playerId)
        {
            SendFactionChange(MyFactionStateChange.FactionMemberDemote, factionId, factionId, playerId);
        }

        private static MyFactionStateChange DetermineRequestFromRelation(MyRelationsBetweenFactions relation) => 
            ((relation != MyRelationsBetweenFactions.Enemies) ? MyFactionStateChange.SendPeaceRequest : MyFactionStateChange.DeclareWar);

        public void EditFaction(long factionId, string tag, string name, string desc, string privateInfo)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, string, string, string, string>(s => new Action<long, string, string, string, string>(MyFactionCollection.EditFactionRequest), factionId, tag, name, desc, privateInfo, targetEndpoint, position);
        }

        [Event(null, 0x317), Reliable, Server]
        private static void EditFactionRequest(long factionId, string tag, string name, [Serialize(MyObjectFlags.DefaultZero)] string desc, string privateInfo)
        {
            IMyFaction doNotCheck = MySession.Static.Factions.TryGetFactionById(factionId);
            long playerId = MySession.Static.Players.TryGetIdentityId(MyEventContext.Current.Sender.Value, 0);
            if (((doNotCheck != null) && (!MySession.Static.Factions.FactionTagExists(tag, doNotCheck) && !MySession.Static.Factions.FactionNameExists(name, doNotCheck))) && (doNotCheck.IsLeader(playerId) || MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value)))
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, string, string, string, string>(s => new Action<long, string, string, string, string>(MyFactionCollection.EditFactionSuccess), factionId, tag, name, desc, privateInfo, targetEndpoint, position);
            }
            else if (!MyEventContext.Current.IsLocallyInvoked)
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
        }

        [Event(null, 0x327), Reliable, ServerInvoked, Broadcast]
        private static void EditFactionSuccess(long factionId, string tag, string name, [Serialize(MyObjectFlags.DefaultZero)] string desc, string privateInfo)
        {
            MyFaction faction = MySession.Static.Factions.TryGetFactionById(factionId) as MyFaction;
            if (faction != null)
            {
                MySession.Static.Factions.UnregisterFactionTag(faction);
                faction.Tag = tag;
                faction.Name = name;
                faction.Description = desc;
                faction.PrivateInfo = privateInfo;
                MySession.Static.Factions.RegisterFactionTag(faction);
                Action<long> factionEdited = MySession.Static.Factions.FactionEdited;
                if (factionEdited != null)
                {
                    factionEdited(factionId);
                }
            }
        }

        public bool FactionNameExists(string name, IMyFaction doNotCheck = null)
        {
            using (Dictionary<long, MyFaction>.Enumerator enumerator = this.m_factions.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<long, MyFaction> current = enumerator.Current;
                    MyFaction faction = current.Value;
                    if (((doNotCheck == null) || (doNotCheck.FactionId != faction.FactionId)) && string.Equals(name, faction.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [Event(null, 0x25b), Reliable, Server]
        private static void FactionStateChangeRequest(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId)
        {
            IMyFaction faction = MySession.Static.Factions.TryGetFactionById(fromFactionId);
            IMyFaction faction2 = MySession.Static.Factions.TryGetFactionById(toFactionId);
            long senderId = MySession.Static.Players.TryGetIdentityId(MyEventContext.Current.Sender.Value, 0);
            if (((faction != null) && (faction2 != null)) && MySession.Static.Factions.CheckFactionStateChange(action, fromFactionId, toFactionId, playerId, senderId))
            {
                if ((((action == MyFactionStateChange.FactionMemberKick) || (action == MyFactionStateChange.FactionMemberLeave)) && (faction.Members.Count == 1)) && (MySession.Static.BlockLimitsEnabled != MyBlockLimitsEnabledEnum.PER_FACTION))
                {
                    action = MyFactionStateChange.RemoveFaction;
                }
                else if (action != MyFactionStateChange.FactionMemberSendJoin)
                {
                    if (action == MyFactionStateChange.FactionMemberAcceptJoin)
                    {
                        if (!MyBlockLimits.IsFactionChangePossible(playerId, faction2.FactionId))
                        {
                            action = MyFactionStateChange.FactionMemberNotPossibleJoin;
                        }
                    }
                    else if ((action == MyFactionStateChange.SendPeaceRequest) && faction2.AutoAcceptPeace)
                    {
                        action = MyFactionStateChange.AcceptPeace;
                        senderId = 0L;
                    }
                }
                else
                {
                    ulong steamId = MySession.Static.Players.TryGetSteamId(playerId);
                    bool flag = MySession.Static.IsUserSpaceMaster(steamId);
                    if (faction2.AutoAcceptMember || (faction2.Members.Count == 0))
                    {
                        flag = true;
                        if ((!faction2.AcceptHumans && (steamId != 0)) && (MySession.Static.Players.TryGetSerialId(playerId) == 0))
                        {
                            flag = false;
                            action = MyFactionStateChange.FactionMemberCancelJoin;
                        }
                    }
                    if ((MySession.Static.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.PER_FACTION) && !MyBlockLimits.IsFactionChangePossible(playerId, faction2.FactionId))
                    {
                        flag = false;
                        action = MyFactionStateChange.FactionMemberNotPossibleJoin;
                    }
                    if (flag)
                    {
                        action = MyFactionStateChange.FactionMemberAcceptJoin;
                    }
                }
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<MyFactionStateChange, long, long, long, long>(s => new Action<MyFactionStateChange, long, long, long, long>(MyFactionCollection.FactionStateChangeSuccess), action, fromFactionId, toFactionId, playerId, senderId, targetEndpoint, position);
                FactionStateChangeSuccess(action, fromFactionId, toFactionId, playerId, senderId);
            }
        }

        [Event(null, 0x2a1), Reliable, Broadcast]
        private static void FactionStateChangeSuccess(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
        {
            IMyFaction faction = MySession.Static.Factions.TryGetFactionById(toFactionId);
            if ((MySession.Static.Factions.TryGetFactionById(fromFactionId) != null) && (faction != null))
            {
                MySession.Static.Factions.ApplyFactionStateChange(action, fromFactionId, toFactionId, playerId, senderId);
                Action<MyFactionStateChange, long, long, long, long> factionStateChanged = MySession.Static.Factions.FactionStateChanged;
                if (factionStateChanged != null)
                {
                    factionStateChanged(action, fromFactionId, toFactionId, playerId, senderId);
                }
            }
        }

        public bool FactionTagExists(string tag, IMyFaction doNotCheck = null) => 
            (this.TryGetFactionByTag(tag, doNotCheck) != null);

        public IEnumerator<KeyValuePair<long, MyFaction>> GetEnumerator() => 
            this.m_factions.GetEnumerator();

        public MyObjectBuilder_FactionCollection GetObjectBuilder()
        {
            MyObjectBuilder_FactionCollection factions = new MyObjectBuilder_FactionCollection {
                Factions = new List<MyObjectBuilder_Faction>(this.m_factions.Count)
            };
            foreach (KeyValuePair<long, MyFaction> pair in this.m_factions)
            {
                factions.Factions.Add(pair.Value.GetObjectBuilder());
            }
            factions.Players = new SerializableDictionary<long, long>();
            foreach (KeyValuePair<long, long> pair2 in this.m_playerFaction)
            {
                factions.Players.Dictionary.Add(pair2.Key, pair2.Value);
            }
            factions.Relations = new List<MyObjectBuilder_FactionRelation>(this.m_relationsBetweenFactions.Count);
            foreach (KeyValuePair<MyFactionPair, MyRelationsBetweenFactions> pair3 in this.m_relationsBetweenFactions)
            {
                MyObjectBuilder_FactionRelation item = new MyObjectBuilder_FactionRelation {
                    FactionId1 = pair3.Key.FactionId1,
                    FactionId2 = pair3.Key.FactionId2,
                    Relation = pair3.Value
                };
                factions.Relations.Add(item);
            }
            factions.Requests = new List<MyObjectBuilder_FactionRequests>();
            foreach (KeyValuePair<long, HashSet<long>> pair4 in this.m_factionRequests)
            {
                List<long> list = new List<long>(pair4.Value.Count);
                foreach (long num in this.m_factionRequests[pair4.Key])
                {
                    list.Add(num);
                }
                MyObjectBuilder_FactionRequests item = new MyObjectBuilder_FactionRequests {
                    FactionId = pair4.Key,
                    FactionRequests = list
                };
                factions.Requests.Add(item);
            }
            return factions;
        }

        public MyFaction GetPlayerFaction(long playerId)
        {
            MyFaction faction = null;
            long num;
            if (this.m_playerFaction.TryGetValue(playerId, out num))
            {
                this.m_factions.TryGetValue(num, out faction);
            }
            return faction;
        }

        public MyRelationsBetweenFactions GetRelationBetweenFactions(long factionId1, long factionId2) => 
            this.GetRelationBetweenFactions(factionId1, factionId2, MyPerGameSettings.DefaultFactionRelationship);

        public MyRelationsBetweenFactions GetRelationBetweenFactions(long factionId1, long factionId2, MyRelationsBetweenFactions defaultState)
        {
            if ((factionId1 != factionId2) || (factionId1 == 0))
            {
                return this.m_relationsBetweenFactions.GetValueOrDefault<MyFactionPair, MyRelationsBetweenFactions>(new MyFactionPair(factionId1, factionId2), defaultState);
            }
            return MyRelationsBetweenFactions.Neutral;
        }

        public MyFactionPeaceRequestState GetRequestState(long myFactionId, long foreignFactionId)
        {
            if (this.m_factionRequests.ContainsKey(myFactionId) && this.m_factionRequests[myFactionId].Contains(foreignFactionId))
            {
                return MyFactionPeaceRequestState.Sent;
            }
            if (!this.m_factionRequests.ContainsKey(foreignFactionId) || !this.m_factionRequests[foreignFactionId].Contains(myFactionId))
            {
                return MyFactionPeaceRequestState.None;
            }
            return MyFactionPeaceRequestState.Pending;
        }

        public int HumansCount() => 
            (from x in this.Factions
                where x.Value.AcceptHumans
                select x).Count<KeyValuePair<long, IMyFaction>>();

        public void Init(MyObjectBuilder_FactionCollection builder)
        {
            foreach (MyObjectBuilder_Faction faction in builder.Factions)
            {
                MySession.Static.Factions.Add(new MyFaction(faction));
            }
            foreach (KeyValuePair<long, long> pair in builder.Players.Dictionary)
            {
                this.m_playerFaction.Add(pair.Key, pair.Value);
            }
            foreach (MyObjectBuilder_FactionRelation relation in builder.Relations)
            {
                this.m_relationsBetweenFactions.Add(new MyFactionPair(relation.FactionId1, relation.FactionId2), relation.Relation);
            }
            foreach (MyObjectBuilder_FactionRequests requests in builder.Requests)
            {
                HashSet<long> set = new HashSet<long>();
                foreach (long num in requests.FactionRequests)
                {
                    set.Add(num);
                }
                this.m_factionRequests.Add(requests.FactionId, set);
            }
        }

        public void InvokePlayerJoined(MyFaction faction, long identityId)
        {
            this.OnPlayerJoined.InvokeIfNotNull<MyFaction, long>(faction, identityId);
        }

        public void InvokePlayerLeft(MyFaction faction, long identityId)
        {
            this.OnPlayerLeft.InvokeIfNotNull<MyFaction, long>(faction, identityId);
        }

        public bool IsPeaceRequestStatePending(long myFactionId, long foreignFactionId) => 
            (this.GetRequestState(myFactionId, foreignFactionId) == MyFactionPeaceRequestState.Pending);

        public bool IsPeaceRequestStateSent(long myFactionId, long foreignFactionId) => 
            (this.GetRequestState(myFactionId, foreignFactionId) == MyFactionPeaceRequestState.Sent);

        public static void KickMember(long factionId, long playerId)
        {
            SendFactionChange(MyFactionStateChange.FactionMemberKick, factionId, factionId, playerId);
        }

        public void KickPlayerFromFaction(long playerId)
        {
            this.m_playerFaction.Remove(playerId);
        }

        internal void LoadFactions(List<MyObjectBuilder_Faction> factionBuilders, bool removeOldData = true)
        {
            if (removeOldData)
            {
                this.m_factions.Clear();
                this.m_factionRequests.Clear();
                this.m_relationsBetweenFactions.Clear();
                this.m_playerFaction.Clear();
                this.m_factionsByTag.Clear();
            }
            if (factionBuilders != null)
            {
                foreach (MyObjectBuilder_Faction faction in factionBuilders)
                {
                    if (!this.m_factions.ContainsKey(faction.FactionId))
                    {
                        MyFaction faction2 = new MyFaction(faction);
                        this.Add(faction2);
                        foreach (KeyValuePair<long, MyFactionMember> pair in faction2.Members)
                        {
                            this.AddPlayerToFaction(pair.Value.PlayerId, faction2.FactionId);
                        }
                    }
                }
            }
        }

        public static void MemberLeaves(long factionId, long playerId)
        {
            SendFactionChange(MyFactionStateChange.FactionMemberLeave, factionId, factionId, playerId);
        }

        public static void PromoteMember(long factionId, long playerId)
        {
            SendFactionChange(MyFactionStateChange.FactionMemberPromote, factionId, factionId, playerId);
        }

        private void RegisterFactionTag(MyFaction faction)
        {
            if (faction != null)
            {
                string key = faction.Tag.ToUpperInvariant();
                MyFaction faction2 = null;
                this.m_factionsByTag.TryGetValue(key, out faction2);
                this.m_factionsByTag[key] = faction;
            }
        }

        public static void RemoveFaction(long factionId)
        {
            SendFactionChange(MyFactionStateChange.RemoveFaction, factionId, factionId, 0L);
        }

        private void RemoveRelation(long fromFactionId, long toFactionId)
        {
            this.m_relationsBetweenFactions.Remove(new MyFactionPair(fromFactionId, toFactionId));
        }

        internal List<MyObjectBuilder_Faction> SaveFactions()
        {
            List<MyObjectBuilder_Faction> list = new List<MyObjectBuilder_Faction>();
            foreach (KeyValuePair<long, MyFaction> pair in this.m_factions)
            {
                MyObjectBuilder_Faction objectBuilder = pair.Value.GetObjectBuilder();
                list.Add(objectBuilder);
            }
            return list;
        }

        private void SendCreateFaction(long founderId, string factionTag, string factionName, string factionDesc, string factionPrivate)
        {
            AddFactionMsg msg = new AddFactionMsg {
                FounderId = founderId,
                FactionTag = factionTag,
                FactionName = factionName,
                FactionDescription = factionDesc,
                FactionPrivateInfo = factionPrivate
            };
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<AddFactionMsg>(s => new Action<AddFactionMsg>(MyFactionCollection.CreateFactionRequest), msg, targetEndpoint, position);
        }

        private static void SendFactionChange(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<MyFactionStateChange, long, long, long>(s => new Action<MyFactionStateChange, long, long, long>(MyFactionCollection.FactionStateChangeRequest), action, fromFactionId, toFactionId, playerId, targetEndpoint, position);
        }

        public static void SendJoinRequest(long factionId, long playerId)
        {
            SendFactionChange(MyFactionStateChange.FactionMemberSendJoin, factionId, factionId, playerId);
        }

        public static void SendPeaceRequest(long fromFactionId, long toFactionId)
        {
            SendFactionChange(MyFactionStateChange.SendPeaceRequest, fromFactionId, toFactionId, 0L);
        }

        private static void SetDefaultFactionStateInternal(long factionId, IMyFaction defaultFaction, MyFactionDefinition defaultFactionDef)
        {
            MyFactionStateChange change = DetermineRequestFromRelation(defaultFactionDef.DefaultRelation);
            MySession.Static.Factions.ApplyFactionStateChange(change, defaultFaction.FactionId, factionId, defaultFaction.FounderId, defaultFaction.FounderId);
            Action<MyFactionStateChange, long, long, long, long> factionStateChanged = MySession.Static.Factions.FactionStateChanged;
            if (factionStateChanged != null)
            {
                factionStateChanged(change, defaultFaction.FactionId, factionId, defaultFaction.FounderId, defaultFaction.FounderId);
            }
        }

        [Event(null, 0x40a), Reliable, Broadcast]
        private static void SetDefaultFactionStates(long recivedFactionId)
        {
            IMyFaction defaultFaction = MySession.Static.Factions.TryGetFactionById(recivedFactionId);
            MyFactionDefinition defaultFactionDef = MyDefinitionManager.Static.TryGetFactionDefinition(defaultFaction.Tag);
            foreach (KeyValuePair<long, MyFaction> pair in MySession.Static.Factions)
            {
                MyFaction faction2 = pair.Value;
                if (faction2.FactionId != recivedFactionId)
                {
                    if (defaultFactionDef != null)
                    {
                        SetDefaultFactionStateInternal(faction2.FactionId, defaultFaction, defaultFactionDef);
                        continue;
                    }
                    MyFactionDefinition definition2 = MyDefinitionManager.Static.TryGetFactionDefinition(faction2.Tag);
                    if (definition2 != null)
                    {
                        SetDefaultFactionStateInternal(recivedFactionId, faction2, definition2);
                    }
                }
            }
        }

        IEnumerator<KeyValuePair<long, MyFaction>> IEnumerable<KeyValuePair<long, MyFaction>>.GetEnumerator() => 
            this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public IMyFaction TryGetFactionById(long factionId)
        {
            MyFaction faction;
            return (!this.m_factions.TryGetValue(factionId, out faction) ? null : faction);
        }

        public MyFaction TryGetFactionByTag(string tag, IMyFaction doNotCheck = null)
        {
            string key = tag.ToUpperInvariant();
            MyFaction faction = null;
            this.m_factionsByTag.TryGetValue(key, out faction);
            if ((faction != null) && ((doNotCheck == null) || (faction.FactionId != doNotCheck.FactionId)))
            {
                return faction;
            }
            return null;
        }

        public MyFaction TryGetOrCreateFactionByTag(string tag)
        {
            MyFaction faction = this.TryGetFactionByTag(tag, null);
            if (faction == null)
            {
                string str = tag.ToUpperInvariant();
                if (MyDefinitionManager.Static.TryGetFactionDefinition(str) == null)
                {
                    return null;
                }
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<string>(x => new Action<string>(MyFactionCollection.CreateFactionByDefinition), tag, targetEndpoint, position);
                faction = this.TryGetFactionByTag(tag, null);
            }
            return faction;
        }

        public IMyFaction TryGetPlayerFaction(long playerId) => 
            this.GetPlayerFaction(playerId);

        private void UnregisterFactionTag(MyFaction faction)
        {
            if (faction != null)
            {
                this.m_factionsByTag.Remove(faction.Tag.ToUpperInvariant());
            }
        }

        void IMyFactionCollection.AcceptJoin(long factionId, long playerId)
        {
            AcceptJoin(factionId, playerId);
        }

        void IMyFactionCollection.AcceptPeace(long fromFactionId, long toFactionId)
        {
            AcceptPeace(fromFactionId, toFactionId);
        }

        void IMyFactionCollection.AddPlayerToFaction(long playerId, long factionId)
        {
            this.AddPlayerToFaction(playerId, factionId);
        }

        bool IMyFactionCollection.AreFactionsEnemies(long factionId1, long factionId2) => 
            this.AreFactionsEnemies(factionId1, factionId2);

        void IMyFactionCollection.CancelJoinRequest(long factionId, long playerId)
        {
            CancelJoinRequest(factionId, playerId);
        }

        void IMyFactionCollection.CancelPeaceRequest(long fromFactionId, long toFactionId)
        {
            CancelPeaceRequest(fromFactionId, toFactionId);
        }

        void IMyFactionCollection.ChangeAutoAccept(long factionId, long playerId, bool autoAcceptMember, bool autoAcceptPeace)
        {
            this.ChangeAutoAccept(factionId, playerId, autoAcceptMember, autoAcceptPeace);
        }

        void IMyFactionCollection.CreateFaction(long founderId, string tag, string name, string desc, string privateInfo)
        {
            this.CreateFaction(founderId, tag, name, desc, privateInfo);
        }

        void IMyFactionCollection.CreateNPCFaction(string tag, string name, string desc, string privateInfo)
        {
            this.CreateNPCFaction(tag, name, desc, privateInfo);
        }

        void IMyFactionCollection.DeclareWar(long fromFactionId, long toFactionId)
        {
            DeclareWar(fromFactionId, toFactionId);
        }

        void IMyFactionCollection.DemoteMember(long factionId, long playerId)
        {
            DemoteMember(factionId, playerId);
        }

        void IMyFactionCollection.EditFaction(long factionId, string tag, string name, string desc, string privateInfo)
        {
            this.EditFaction(factionId, tag, name, desc, privateInfo);
        }

        bool IMyFactionCollection.FactionNameExists(string name, IMyFaction doNotCheck) => 
            this.FactionNameExists(name, doNotCheck);

        bool IMyFactionCollection.FactionTagExists(string tag, IMyFaction doNotCheck) => 
            this.FactionTagExists(tag, doNotCheck);

        MyObjectBuilder_FactionCollection IMyFactionCollection.GetObjectBuilder() => 
            this.GetObjectBuilder();

        MyRelationsBetweenFactions IMyFactionCollection.GetRelationBetweenFactions(long factionId1, long factionId2) => 
            this.GetRelationBetweenFactions(factionId1, factionId2);

        bool IMyFactionCollection.IsPeaceRequestStatePending(long myFactionId, long foreignFactionId) => 
            this.IsPeaceRequestStatePending(myFactionId, foreignFactionId);

        bool IMyFactionCollection.IsPeaceRequestStateSent(long myFactionId, long foreignFactionId) => 
            this.IsPeaceRequestStateSent(myFactionId, foreignFactionId);

        void IMyFactionCollection.KickMember(long factionId, long playerId)
        {
            KickMember(factionId, playerId);
        }

        void IMyFactionCollection.KickPlayerFromFaction(long playerId)
        {
            this.KickPlayerFromFaction(playerId);
        }

        void IMyFactionCollection.MemberLeaves(long factionId, long playerId)
        {
            MemberLeaves(factionId, playerId);
        }

        void IMyFactionCollection.PromoteMember(long factionId, long playerId)
        {
            PromoteMember(factionId, playerId);
        }

        void IMyFactionCollection.RemoveFaction(long factionId)
        {
            RemoveFaction(factionId);
        }

        void IMyFactionCollection.SendJoinRequest(long factionId, long playerId)
        {
            SendJoinRequest(factionId, playerId);
        }

        void IMyFactionCollection.SendPeaceRequest(long fromFactionId, long toFactionId)
        {
            SendPeaceRequest(fromFactionId, toFactionId);
        }

        IMyFaction IMyFactionCollection.TryGetFactionById(long factionId) => 
            this.TryGetFactionById(factionId);

        IMyFaction IMyFactionCollection.TryGetFactionByName(string name)
        {
            using (Dictionary<long, MyFaction>.Enumerator enumerator = this.m_factions.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<long, MyFaction> current = enumerator.Current;
                    MyFaction faction = current.Value;
                    if (string.Equals(name, faction.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return faction;
                    }
                }
            }
            return null;
        }

        IMyFaction IMyFactionCollection.TryGetFactionByTag(string tag) => 
            this.TryGetFactionByTag(tag, null);

        IMyFaction IMyFactionCollection.TryGetPlayerFaction(long playerId) => 
            this.TryGetPlayerFaction(playerId);

        public bool JoinableFactionsPresent
        {
            get
            {
                using (Dictionary<long, MyFaction>.Enumerator enumerator = this.m_factions.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<long, MyFaction> current = enumerator.Current;
                        if (current.Value.AcceptHumans)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public MyFaction this[long factionId] =>
            this.m_factions[factionId];

        public Dictionary<long, IMyFaction> Factions =>
            this.m_factions.ToDictionary<KeyValuePair<long, MyFaction>, long, IMyFaction>(e => e.Key, ((Func<KeyValuePair<long, MyFaction>, IMyFaction>) (e => e.Value)));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyFactionCollection.<>c <>9 = new MyFactionCollection.<>c();
            public static Func<IMyEventOwner, Action<string>> <>9__20_0;
            public static Func<IMyEventOwner, Action<MyFactionStateChange, long, long, long>> <>9__58_0;
            public static Func<IMyEventOwner, Action<MyFactionStateChange, long, long, long, long>> <>9__59_0;
            public static Func<IMyEventOwner, Action<long, long, bool, bool>> <>9__68_0;
            public static Func<IMyEventOwner, Action<long, bool, bool>> <>9__69_0;
            public static Func<IMyEventOwner, Action<long, string, string, string, string>> <>9__74_0;
            public static Func<IMyEventOwner, Action<long, string, string, string, string>> <>9__75_0;
            public static Func<IMyEventOwner, Action<MyFactionCollection.AddFactionMsg>> <>9__83_0;
            public static Func<IMyEventOwner, Action<MyFactionCollection.AddFactionMsg>> <>9__85_0;
            public static Func<IMyEventOwner, Action<long>> <>9__85_1;
            public static Func<KeyValuePair<long, IMyFaction>, bool> <>9__93_0;
            public static Func<KeyValuePair<long, MyFaction>, long> <>9__137_0;
            public static Func<KeyValuePair<long, MyFaction>, IMyFaction> <>9__137_1;

            internal Action<long, long, bool, bool> <ChangeAutoAccept>b__68_0(IMyEventOwner s) => 
                new Action<long, long, bool, bool>(MyFactionCollection.ChangeAutoAcceptRequest);

            internal Action<long, bool, bool> <ChangeAutoAcceptRequest>b__69_0(IMyEventOwner s) => 
                new Action<long, bool, bool>(MyFactionCollection.ChangeAutoAcceptSuccess);

            internal Action<MyFactionCollection.AddFactionMsg> <CreateFactionServer>b__85_0(IMyEventOwner x) => 
                new Action<MyFactionCollection.AddFactionMsg>(MyFactionCollection.CreateFactionSuccess);

            internal Action<long> <CreateFactionServer>b__85_1(IMyEventOwner x) => 
                new Action<long>(MyFactionCollection.SetDefaultFactionStates);

            internal Action<long, string, string, string, string> <EditFaction>b__74_0(IMyEventOwner s) => 
                new Action<long, string, string, string, string>(MyFactionCollection.EditFactionRequest);

            internal Action<long, string, string, string, string> <EditFactionRequest>b__75_0(IMyEventOwner s) => 
                new Action<long, string, string, string, string>(MyFactionCollection.EditFactionSuccess);

            internal Action<MyFactionStateChange, long, long, long, long> <FactionStateChangeRequest>b__59_0(IMyEventOwner s) => 
                new Action<MyFactionStateChange, long, long, long, long>(MyFactionCollection.FactionStateChangeSuccess);

            internal long <get_Factions>b__137_0(KeyValuePair<long, MyFaction> e) => 
                e.Key;

            internal IMyFaction <get_Factions>b__137_1(KeyValuePair<long, MyFaction> e) => 
                e.Value;

            internal bool <HumansCount>b__93_0(KeyValuePair<long, IMyFaction> x) => 
                x.Value.AcceptHumans;

            internal Action<MyFactionCollection.AddFactionMsg> <SendCreateFaction>b__83_0(IMyEventOwner s) => 
                new Action<MyFactionCollection.AddFactionMsg>(MyFactionCollection.CreateFactionRequest);

            internal Action<MyFactionStateChange, long, long, long> <SendFactionChange>b__58_0(IMyEventOwner s) => 
                new Action<MyFactionStateChange, long, long, long>(MyFactionCollection.FactionStateChangeRequest);

            internal Action<string> <TryGetOrCreateFactionByTag>b__20_0(IMyEventOwner x) => 
                new Action<string>(MyFactionCollection.CreateFactionByDefinition);
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        private struct AddFactionMsg
        {
            [ProtoMember(0x47)]
            public long FounderId;
            [ProtoMember(0x4c)]
            public long FactionId;
            [ProtoMember(0x4e)]
            public string FactionTag;
            [ProtoMember(80)]
            public string FactionName;
            [Serialize(MyObjectFlags.DefaultZero), ProtoMember(0x53)]
            public string FactionDescription;
            [ProtoMember(0x55)]
            public string FactionPrivateInfo;
            [ProtoMember(0x57)]
            public bool CreateFromDefinition;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyFactionPair
        {
            public long FactionId1;
            public long FactionId2;
            public static readonly ComparerType Comparer;
            public MyFactionPair(long id1, long id2)
            {
                this.FactionId1 = id1;
                this.FactionId2 = id2;
            }

            static MyFactionPair()
            {
                Comparer = new ComparerType();
            }
            public class ComparerType : IEqualityComparer<MyFactionCollection.MyFactionPair>
            {
                public bool Equals(MyFactionCollection.MyFactionPair x, MyFactionCollection.MyFactionPair y)
                {
                    if ((x.FactionId1 != y.FactionId1) || (x.FactionId2 != y.FactionId2))
                    {
                        return ((x.FactionId1 == y.FactionId2) && (x.FactionId2 == y.FactionId1));
                    }
                    return true;
                }

                public int GetHashCode(MyFactionCollection.MyFactionPair obj) => 
                    (obj.FactionId1.GetHashCode() ^ obj.FactionId2.GetHashCode());
            }
        }

        public enum MyFactionPeaceRequestState
        {
            None,
            Pending,
            Sent
        }
    }
}

