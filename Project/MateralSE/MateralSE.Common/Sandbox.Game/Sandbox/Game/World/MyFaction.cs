namespace Sandbox.Game.World
{
    using Sandbox.Definitions;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.ModAPI;

    public class MyFaction : IMyFaction
    {
        private Dictionary<long, MyFactionMember> m_members;
        private Dictionary<long, MyFactionMember> m_joinRequests;
        private MyBlockLimits m_blockLimits;
        public string Tag;
        public string Name;
        public string Description;
        public string PrivateInfo;
        public bool AutoAcceptMember;
        public bool AutoAcceptPeace;
        public bool AcceptHumans;
        public bool EnableFriendlyFire;

        public MyFaction(MyObjectBuilder_Faction obj)
        {
            this.m_blockLimits = new MyBlockLimits(MyBlockLimits.GetInitialPCU(-1L), 0);
            this.EnableFriendlyFire = true;
            this.FactionId = obj.FactionId;
            this.Tag = obj.Tag;
            this.Name = obj.Name;
            this.Description = obj.Description;
            this.PrivateInfo = obj.PrivateInfo;
            this.AutoAcceptMember = obj.AutoAcceptMember;
            this.AutoAcceptPeace = obj.AutoAcceptPeace;
            this.EnableFriendlyFire = obj.EnableFriendlyFire;
            this.AcceptHumans = obj.AcceptHumans;
            this.m_members = new Dictionary<long, MyFactionMember>(obj.Members.Count);
            foreach (MyObjectBuilder_FactionMember member in obj.Members)
            {
                this.m_members.Add(member.PlayerId, member);
                if (member.IsFounder)
                {
                    this.FounderId = member.PlayerId;
                }
            }
            if (obj.JoinRequests != null)
            {
                this.m_joinRequests = new Dictionary<long, MyFactionMember>(obj.JoinRequests.Count);
                foreach (MyObjectBuilder_FactionMember member2 in obj.JoinRequests)
                {
                    this.m_joinRequests.Add(member2.PlayerId, member2);
                }
            }
            else
            {
                this.m_joinRequests = new Dictionary<long, MyFactionMember>();
            }
            MyFactionDefinition definition = MyDefinitionManager.Static.TryGetFactionDefinition(this.Tag);
            if (definition != null)
            {
                this.AutoAcceptMember = definition.AutoAcceptMember;
                this.AcceptHumans = definition.AcceptHumans;
                this.EnableFriendlyFire = definition.EnableFriendlyFire;
                this.Name = definition.DisplayNameText;
                this.Description = definition.DescriptionText;
            }
            this.CheckAndFixFactionRanks();
        }

        public MyFaction(long id, string tag, string name, string desc, string privateInfo, long creatorId)
        {
            this.m_blockLimits = new MyBlockLimits(MyBlockLimits.GetInitialPCU(-1L), 0);
            this.EnableFriendlyFire = true;
            this.FactionId = id;
            this.Tag = tag;
            this.Name = name;
            this.Description = desc;
            this.PrivateInfo = privateInfo;
            this.FounderId = creatorId;
            this.AutoAcceptMember = false;
            this.AutoAcceptPeace = false;
            this.AcceptHumans = true;
            this.m_members = new Dictionary<long, MyFactionMember>();
            this.m_joinRequests = new Dictionary<long, MyFactionMember>();
            this.m_members.Add(creatorId, new MyFactionMember(creatorId, true, true));
        }

        public void AcceptJoin(long playerId, bool autoaccept = false)
        {
            MyFaction oldFaction = null;
            oldFaction = MySession.Static.Factions.GetPlayerFaction(playerId);
            if (oldFaction != null)
            {
                oldFaction.KickMember(playerId, false);
                if (MySession.Static.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.PER_FACTION)
                {
                    MyBlockLimits.TransferBlockLimits(playerId, oldFaction.BlockLimits, this.BlockLimits);
                }
            }
            MySession.Static.Factions.AddPlayerToFactionInternal(playerId, this.FactionId);
            if (this.m_joinRequests.ContainsKey(playerId))
            {
                this.m_members[playerId] = this.m_joinRequests[playerId];
                this.m_joinRequests.Remove(playerId);
            }
            else if (this.AutoAcceptMember | autoaccept)
            {
                this.m_members[playerId] = new MyFactionMember(playerId, false, false);
            }
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(playerId);
            if (identity != null)
            {
                identity.BlockLimits.SetAllDirty();
                identity.RaiseFactionChanged(oldFaction, this);
            }
            MySession.Static.Factions.InvokePlayerJoined(this, playerId);
        }

        public void AddJoinRequest(long playerId)
        {
            this.m_joinRequests[playerId] = new MyFactionMember(playerId, false, false);
        }

        public void CancelJoinRequest(long playerId)
        {
            this.m_joinRequests.Remove(playerId);
        }

        public void CheckAndFixFactionRanks()
        {
            if (!this.HasFounder())
            {
                using (Dictionary<long, MyFactionMember>.Enumerator enumerator = this.m_members.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<long, MyFactionMember> current = enumerator.Current;
                        if (current.Value.IsLeader)
                        {
                            this.PromoteToFounder(current.Key);
                            return;
                        }
                    }
                }
                if (this.m_members.Count > 0)
                {
                    this.PromoteToFounder(this.m_members.Keys.FirstOrDefault<long>());
                }
            }
        }

        public void DemoteMember(long playerId)
        {
            MyFactionMember member;
            if (this.m_members.TryGetValue(playerId, out member))
            {
                member.IsLeader = false;
                this.m_members[playerId] = member;
            }
        }

        public MyObjectBuilder_Faction GetObjectBuilder()
        {
            MyObjectBuilder_Faction faction = new MyObjectBuilder_Faction {
                FactionId = this.FactionId,
                Tag = this.Tag,
                Name = this.Name,
                Description = this.Description,
                PrivateInfo = this.PrivateInfo,
                AutoAcceptMember = this.AutoAcceptMember,
                AutoAcceptPeace = this.AutoAcceptPeace,
                EnableFriendlyFire = this.EnableFriendlyFire,
                Members = new List<MyObjectBuilder_FactionMember>(this.Members.Count)
            };
            foreach (KeyValuePair<long, MyFactionMember> pair in this.Members)
            {
                faction.Members.Add(pair.Value);
            }
            faction.JoinRequests = new List<MyObjectBuilder_FactionMember>(this.JoinRequests.Count);
            foreach (KeyValuePair<long, MyFactionMember> pair2 in this.JoinRequests)
            {
                faction.JoinRequests.Add(pair2.Value);
            }
            return faction;
        }

        private bool HasFounder()
        {
            MyFactionMember member;
            return (this.m_members.TryGetValue(this.FounderId, out member) && member.IsFounder);
        }

        public bool IsEveryoneNpc()
        {
            using (Dictionary<long, MyFactionMember>.Enumerator enumerator = this.m_members.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<long, MyFactionMember> current = enumerator.Current;
                    if (!Sync.Players.IdentityIsNpc(current.Key))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsFounder(long playerId)
        {
            MyFactionMember member;
            return (this.m_members.TryGetValue(playerId, out member) && member.IsFounder);
        }

        public bool IsLeader(long playerId)
        {
            MyFactionMember member;
            return (this.m_members.TryGetValue(playerId, out member) && member.IsLeader);
        }

        public bool IsMember(long playerId)
        {
            MyFactionMember member;
            return this.m_members.TryGetValue(playerId, out member);
        }

        public bool IsNeutral(long playerId)
        {
            IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(playerId);
            return ((faction != null) && (MySession.Static.Factions.GetRelationBetweenFactions(this.FactionId, faction.FactionId) == MyRelationsBetweenFactions.Neutral));
        }

        public void KickMember(long playerId, bool raiseChanged = true)
        {
            this.m_members.Remove(playerId);
            MySession.Static.Factions.KickPlayerFromFaction(playerId);
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(playerId);
            if (raiseChanged && (identity != null))
            {
                identity.RaiseFactionChanged(this, null);
            }
            this.CheckAndFixFactionRanks();
            MySession.Static.Factions.InvokePlayerLeft(this, playerId);
        }

        public void PromoteMember(long playerId)
        {
            MyFactionMember member;
            if (this.m_members.TryGetValue(playerId, out member))
            {
                member.IsLeader = true;
                this.m_members[playerId] = member;
            }
        }

        public void PromoteToFounder(long playerId)
        {
            MyFactionMember member;
            if (this.m_members.TryGetValue(playerId, out member))
            {
                member.IsLeader = true;
                member.IsFounder = true;
                this.m_members[playerId] = member;
                this.FounderId = playerId;
            }
        }

        bool IMyFaction.IsFounder(long playerId) => 
            this.IsFounder(playerId);

        bool IMyFaction.IsLeader(long playerId) => 
            this.IsLeader(playerId);

        bool IMyFaction.IsMember(long playerId) => 
            this.IsMember(playerId);

        bool IMyFaction.IsNeutral(long playerId) => 
            this.IsNeutral(playerId);

        public long FactionId { get; private set; }

        public long FounderId { get; private set; }

        public DictionaryReader<long, MyFactionMember> Members =>
            new DictionaryReader<long, MyFactionMember>(this.m_members);

        public DictionaryReader<long, MyFactionMember> JoinRequests =>
            new DictionaryReader<long, MyFactionMember>(this.m_joinRequests);

        public bool IsAnyLeaderOnline
        {
            get
            {
                ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
                using (Dictionary<long, MyFactionMember>.Enumerator enumerator = this.m_members.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<long, MyFactionMember> member = enumerator.Current;
                        if (member.Value.IsLeader && onlinePlayers.Any<MyPlayer>(x => (x.Identity.IdentityId == member.Value.PlayerId)))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public MyBlockLimits BlockLimits =>
            this.m_blockLimits;

        long IMyFaction.FactionId =>
            this.FactionId;

        string IMyFaction.Tag =>
            this.Tag;

        string IMyFaction.Name =>
            this.Name;

        string IMyFaction.Description =>
            this.Description;

        string IMyFaction.PrivateInfo =>
            this.PrivateInfo;

        bool IMyFaction.AutoAcceptMember =>
            this.AutoAcceptMember;

        bool IMyFaction.AutoAcceptPeace =>
            this.AutoAcceptPeace;

        bool IMyFaction.AcceptHumans =>
            this.AcceptHumans;

        long IMyFaction.FounderId =>
            this.FounderId;

        DictionaryReader<long, MyFactionMember> IMyFaction.Members =>
            this.Members;

        DictionaryReader<long, MyFactionMember> IMyFaction.JoinRequests =>
            this.JoinRequests;
    }
}

