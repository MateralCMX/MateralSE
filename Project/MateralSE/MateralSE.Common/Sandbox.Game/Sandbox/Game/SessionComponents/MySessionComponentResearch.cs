namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 0x29a, typeof(MyObjectBuilder_SessionComponentResearch), (System.Type) null)]
    public class MySessionComponentResearch : MySessionComponentBase
    {
        public bool DEBUG_SHOW_RESEARCH;
        public bool DEBUG_SHOW_RESEARCH_PRETTY = true;
        public static MySessionComponentResearch Static;
        private Dictionary<long, HashSet<MyDefinitionId>> m_unlockedResearch;
        private Dictionary<long, HashSet<MyDefinitionId>> m_unlockedBlocks;
        public List<MyDefinitionId> m_requiredResearch;
        private Dictionary<MyDefinitionId, List<MyDefinitionId>> m_unlocks;
        private Dictionary<long, bool> m_failedSent;
        private MyHudNotification m_unlockedResearchNotification;
        private MyHudNotification m_factionUnlockedResearchNotification;
        private MyHudNotification m_factionFailedResearchNotification;
        private MyHudNotification m_sharedResearchNotification;
        private MyHudNotification m_knownResearchNotification;

        public MySessionComponentResearch()
        {
            Static = this;
            this.m_unlockedResearch = new Dictionary<long, HashSet<MyDefinitionId>>();
            this.m_unlockedBlocks = new Dictionary<long, HashSet<MyDefinitionId>>();
            this.m_requiredResearch = new List<MyDefinitionId>();
            this.m_unlocks = new Dictionary<MyDefinitionId, List<MyDefinitionId>>();
            this.m_failedSent = new Dictionary<long, bool>();
            this.m_unlockedResearchNotification = new MyHudNotification(MyCommonTexts.NotificationResearchUnlocked, 0x9c4, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Normal);
            this.m_factionUnlockedResearchNotification = new MyHudNotification(MyCommonTexts.NotificationFactionResearchUnlocked, 0x9c4, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Normal);
            this.m_factionFailedResearchNotification = new MyHudNotification(MyCommonTexts.NotificationFactionResearchFailed, 0x9c4, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Normal);
            this.m_sharedResearchNotification = new MyHudNotification(MyCommonTexts.NotificationSharedResearch, 0x9c4, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Normal);
            this.m_knownResearchNotification = new MyHudNotification(MyCommonTexts.NotificationResearchKnown, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Normal);
        }

        public void AddRequiredResearch(MyDefinitionId itemId)
        {
            if (!itemId.TypeId.IsNull)
            {
                SerializableDefinitionId id = (SerializableDefinitionId) itemId;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<SerializableDefinitionId>(x => new Action<SerializableDefinitionId>(MySessionComponentResearch.AddRequiredResearchSync), id, targetEndpoint, position);
            }
        }

        [Event(null, 0x201), Reliable, Server, Broadcast]
        private static void AddRequiredResearchSync(SerializableDefinitionId itemId)
        {
            MyDefinitionBase base2;
            if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(itemId, out base2) && !Static.m_requiredResearch.Contains(base2.Id))
            {
                Static.m_requiredResearch.Add(base2.Id);
            }
        }

        [Event(null, 0x184), Reliable, Server]
        public static void CallShareResearch(long toIdentity)
        {
            long num = MySession.Static.Players.TryGetIdentityId(MyEventContext.Current.Sender.Value, 0);
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, long>(x => new Action<long, long>(MySessionComponentResearch.ShareResearch), toIdentity, num, targetEndpoint, position);
        }

        public bool CanUse(MyCharacter character, MyDefinitionId id) => 
            ((character != null) ? this.CanUse(character.GetPlayerIdentityId(), id) : true);

        public bool CanUse(long identityId, MyDefinitionId id) => 
            (!this.RequiresResearch(id) || this.IsBlockUnlocked(identityId, id));

        public void ClearRequiredResearch()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(x => new Action(MySessionComponentResearch.ClearRequiredResearchSync), targetEndpoint, position);
        }

        [Event(null, 550), Reliable, Server, Broadcast]
        private static void ClearRequiredResearchSync()
        {
            Static.m_requiredResearch.Clear();
        }

        public void DebugUnlockAllResearch(long identityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(MySessionComponentResearch.DebugUnlockAllResearchSync), identityId, targetEndpoint, position);
        }

        [Event(null, 0x1b7), Reliable, Server, Broadcast]
        private static void DebugUnlockAllResearchSync(long identityId)
        {
            foreach (MyDefinitionId id in Static.m_requiredResearch)
            {
                HashSet<MyDefinitionId> set;
                HashSet<MyDefinitionId> set2;
                if (!Static.m_unlockedBlocks.TryGetValue(identityId, out set))
                {
                    set = new HashSet<MyDefinitionId>();
                    Static.m_unlockedBlocks[identityId] = set;
                }
                if (!Static.m_unlockedResearch.TryGetValue(identityId, out set2))
                {
                    set2 = new HashSet<MyDefinitionId>();
                    Static.m_unlockedResearch[identityId] = set2;
                }
                if (!set.Contains(id))
                {
                    set.Add(id);
                }
                set2.Add(id);
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (this.DEBUG_SHOW_RESEARCH)
            {
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
                if (localCharacter != null)
                {
                    HashSet<MyDefinitionId> set;
                    long playerIdentityId = localCharacter.GetPlayerIdentityId();
                    if (this.m_unlockedResearch.TryGetValue(playerIdentityId, out set))
                    {
                        MyRenderProxy.DebugDrawText2D(new Vector2(10f, 180f), $"=== {MySession.Static.LocalHumanPlayer.DisplayName}'s Research ===", Color.DarkViolet, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        int num2 = 200;
                        foreach (MyDefinitionId id in set)
                        {
                            if (!this.DEBUG_SHOW_RESEARCH_PRETTY)
                            {
                                MyRenderProxy.DebugDrawText2D(new Vector2(10f, (float) num2), id.ToString(), Color.DarkViolet, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            }
                            else
                            {
                                MyDefinitionBase definition = MyDefinitionManager.Static.GetDefinition(id);
                                if (definition is MyResearchDefinition)
                                {
                                    MyRenderProxy.DebugDrawText2D(new Vector2(10f, (float) num2), $"[R] {definition.DisplayNameText}", Color.DarkViolet, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                }
                                else
                                {
                                    MyRenderProxy.DebugDrawText2D(new Vector2(10f, (float) num2), definition.DisplayNameText, Color.DarkViolet, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                                }
                            }
                            num2 += 0x10;
                        }
                    }
                }
            }
        }

        [Event(null, 0x13f), Reliable, Client]
        private static void FactionUnlockFailed()
        {
            MyHud.Notifications.Add(Static.m_factionFailedResearchNotification);
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_SessionComponentResearch research = new MyObjectBuilder_SessionComponentResearch {
                Researches = new List<MyObjectBuilder_SessionComponentResearch.ResearchData>()
            };
            foreach (KeyValuePair<long, HashSet<MyDefinitionId>> pair in this.m_unlockedResearch)
            {
                if (pair.Value.Count != 0)
                {
                    List<SerializableDefinitionId> list = new List<SerializableDefinitionId>();
                    foreach (MyDefinitionId id in pair.Value)
                    {
                        list.Add((SerializableDefinitionId) id);
                    }
                    MyObjectBuilder_SessionComponentResearch.ResearchData item = new MyObjectBuilder_SessionComponentResearch.ResearchData {
                        IdentityId = pair.Key,
                        Definitions = list
                    };
                    research.Researches.Add(item);
                }
            }
            return research;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            if (Static != null)
            {
                foreach (MyCubeBlockDefinition definition in MyDefinitionManager.Static.GetDefinitionsOfType<MyCubeBlockDefinition>())
                {
                    MyResearchBlockDefinition researchBlock = MyDefinitionManager.Static.GetResearchBlock(definition.Id);
                    if (researchBlock != null)
                    {
                        if (definition.CubeSize == MyCubeSize.Large)
                        {
                            MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(definition.BlockPairName);
                            if (definitionGroup.Small != null)
                            {
                                MyResearchBlockDefinition definition3 = MyDefinitionManager.Static.GetResearchBlock(definitionGroup.Small.Id);
                                if (definition3 != null)
                                {
                                    string[] unlockedByGroups = definition3.UnlockedByGroups;
                                }
                            }
                        }
                        if (definition.BlockStages != null)
                        {
                            foreach (MyDefinitionId id in definition.BlockStages)
                            {
                                MyResearchBlockDefinition definition4 = MyDefinitionManager.Static.GetResearchBlock(id);
                                if (definition4 != null)
                                {
                                    string[] unlockedByGroups = definition4.UnlockedByGroups;
                                }
                            }
                        }
                        foreach (string str in researchBlock.UnlockedByGroups)
                        {
                            MyResearchGroupDefinition researchGroup = MyDefinitionManager.Static.GetResearchGroup(str);
                            if (((researchGroup != null) && (researchGroup.Members != null)) && (researchGroup.Members.Length != 0))
                            {
                                this.m_requiredResearch.Add(definition.Id);
                                foreach (SerializableDefinitionId id2 in researchGroup.Members)
                                {
                                    List<MyDefinitionId> list;
                                    if (!this.m_unlocks.TryGetValue(id2, out list))
                                    {
                                        list = new List<MyDefinitionId>();
                                        this.m_unlocks.Add(id2, list);
                                    }
                                    list.Add(definition.Id);
                                }
                            }
                        }
                    }
                }
            }
            MyObjectBuilder_SessionComponentResearch research = sessionComponent as MyObjectBuilder_SessionComponentResearch;
            if ((research != null) && (research.Researches != null))
            {
                foreach (MyObjectBuilder_SessionComponentResearch.ResearchData data in research.Researches)
                {
                    HashSet<MyDefinitionId> set = new HashSet<MyDefinitionId>();
                    HashSet<MyDefinitionId> set2 = new HashSet<MyDefinitionId>();
                    foreach (SerializableDefinitionId id3 in data.Definitions)
                    {
                        List<MyDefinitionId> list2;
                        set.Add(id3);
                        if (Static.m_unlocks.TryGetValue(id3, out list2))
                        {
                            foreach (MyDefinitionId id4 in list2)
                            {
                                set2.Add(id4);
                            }
                        }
                    }
                    this.m_unlockedResearch.Add(data.IdentityId, set);
                    this.m_unlockedBlocks.Add(data.IdentityId, set2);
                }
            }
            if (Sync.IsServer && MySession.Static.ResearchEnabled)
            {
                MyCubeGrids.BlockFunctional += new Action<MyCubeGrid, MySlimBlock, bool>(this.OnBlockBuilt);
            }
        }

        public bool IsBlockUnlocked(long identityId, MyDefinitionId id)
        {
            HashSet<MyDefinitionId> set;
            return (this.m_unlockedBlocks.TryGetValue(identityId, out set) ? set.Contains(id) : false);
        }

        public void LockResearch(long characterId, MyDefinitionId itemId)
        {
            if (!itemId.TypeId.IsNull)
            {
                SerializableDefinitionId id = (SerializableDefinitionId) itemId;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, SerializableDefinitionId>(x => new Action<long, SerializableDefinitionId>(MySessionComponentResearch.LockResearchSync), characterId, id, targetEndpoint, position);
            }
        }

        [Event(null, 0x243), Reliable, Server, Broadcast]
        private static void LockResearchSync(long characterId, SerializableDefinitionId itemId)
        {
            MyDefinitionBase base2;
            if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(itemId, out base2) && Static.m_unlockedResearch.ContainsKey(characterId))
            {
                Static.m_unlockedResearch[characterId].Remove(base2.Id);
            }
        }

        private void OnBlockBuilt(MyCubeGrid grid, MySlimBlock block, bool handWelded)
        {
            if (handWelded)
            {
                long builtBy = block.BuiltBy;
                MyDefinitionId id = block.BlockDefinition.Id;
                IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(builtBy);
                if (faction != null)
                {
                    foreach (long num2 in faction.Members.Keys)
                    {
                        if (MySession.Static.Players.IsPlayerOnline(num2))
                        {
                            this.UnlockResearch(num2, id, builtBy);
                        }
                    }
                }
                else
                {
                    this.UnlockResearch(builtBy, id, builtBy);
                }
            }
        }

        public void RemoveRequiredResearch(MyDefinitionId itemId)
        {
            if (!itemId.TypeId.IsNull)
            {
                SerializableDefinitionId id = (SerializableDefinitionId) itemId;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<SerializableDefinitionId>(x => new Action<SerializableDefinitionId>(MySessionComponentResearch.RemoveRequiredResearchSync), id, targetEndpoint, position);
            }
        }

        [Event(null, 0x217), Reliable, Server, Broadcast]
        private static void RemoveRequiredResearchSync(SerializableDefinitionId itemId)
        {
            MyDefinitionBase base2;
            if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(itemId, out base2))
            {
                Static.m_requiredResearch.Remove(base2.Id);
            }
        }

        public bool RequiresResearch(MyDefinitionId id) => 
            this.m_requiredResearch.Contains(id);

        public void ResetResearch(MyCharacter character)
        {
            if (character != null)
            {
                this.ResetResearch(character.GetPlayerIdentityId());
            }
        }

        public void ResetResearch(long identityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(MySessionComponentResearch.ResetResearchSync), identityId, targetEndpoint, position);
        }

        public void ResetResearchForAll()
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(x => new Action(MySessionComponentResearch.ResetResearchForAllSync), targetEndpoint, position);
        }

        [Event(null, 0x231), Reliable, Server, Broadcast]
        private static void ResetResearchForAllSync()
        {
            Static.m_unlockedResearch.Clear();
            Static.m_unlockedBlocks.Clear();
        }

        [Event(null, 0x264), Reliable, Server, Broadcast]
        private static void ResetResearchSync(long identityId)
        {
            if (Static.m_unlockedResearch.ContainsKey(identityId))
            {
                Static.m_unlockedResearch[identityId].Clear();
            }
            if (Static.m_unlockedBlocks.ContainsKey(identityId))
            {
                Static.m_unlockedBlocks[identityId].Clear();
            }
        }

        [Event(null, 0x18b), Reliable, ServerInvoked, Broadcast]
        private static void ShareResearch(long toIdentity, long fromIdentityId)
        {
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(fromIdentityId);
            if (identity != null)
            {
                HashSet<MyDefinitionId> set;
                if (Static.m_unlockedResearch.TryGetValue(fromIdentityId, out set))
                {
                    foreach (MyDefinitionId id in set)
                    {
                        Static.UnlockBlocks(toIdentity, id);
                    }
                }
                if ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == toIdentity))
                {
                    object[] arguments = new object[] { identity.DisplayName };
                    Static.m_sharedResearchNotification.SetTextFormatArguments(arguments);
                    MyHud.Notifications.Add(Static.m_sharedResearchNotification);
                }
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            this.m_unlockedResearch = null;
            this.m_unlockedBlocks = null;
            this.m_requiredResearch = null;
            this.m_unlocks = null;
            if (Sync.IsServer && MySession.Static.ResearchEnabled)
            {
                MyCubeGrids.BlockFunctional -= new Action<MyCubeGrid, MySlimBlock, bool>(this.OnBlockBuilt);
            }
        }

        private bool UnlockBlocks(long identityId, MyDefinitionId researchedBlockId)
        {
            HashSet<MyDefinitionId> set;
            HashSet<MyDefinitionId> set2;
            List<MyDefinitionId> list;
            if (!this.m_unlockedBlocks.TryGetValue(identityId, out set))
            {
                set = new HashSet<MyDefinitionId>();
                this.m_unlockedBlocks[identityId] = set;
            }
            if (!this.m_unlockedResearch.TryGetValue(identityId, out set2))
            {
                set2 = new HashSet<MyDefinitionId>();
                this.m_unlockedResearch[identityId] = set2;
            }
            this.m_unlocks.TryGetValue(researchedBlockId, out list);
            bool flag = false;
            if (list != null)
            {
                foreach (MyDefinitionId id in list)
                {
                    if (!set.Contains(id))
                    {
                        flag = true;
                        set.Add(id);
                    }
                }
            }
            set2.Add(researchedBlockId);
            return flag;
        }

        public void UnlockResearch(long identityId, MyDefinitionId id, long unlockerId)
        {
            HashSet<MyDefinitionId> set;
            if (!this.m_unlockedResearch.TryGetValue(identityId, out set))
            {
                set = new HashSet<MyDefinitionId>();
                this.m_unlockedResearch.Add(identityId, set);
                this.m_unlockedBlocks.Add(identityId, new HashSet<MyDefinitionId>());
            }
            if (!set.Contains(id))
            {
                Vector3D? nullable;
                SerializableDefinitionId id2 = (SerializableDefinitionId) id;
                if (!this.CanUse(identityId, id))
                {
                    if (unlockerId == identityId)
                    {
                        if (!MySession.Static.HasPlayerCreativeRights(MySession.Static.Players.TryGetSteamId(identityId)))
                        {
                            return;
                        }
                    }
                    else
                    {
                        bool flag;
                        if (!this.m_failedSent.TryGetValue(identityId, out flag) || !flag)
                        {
                            ulong num = MySession.Static.Players.TryGetSteamId(identityId);
                            if (num != 0)
                            {
                                nullable = null;
                                MyMultiplayer.RaiseStaticEvent(x => new Action(MySessionComponentResearch.FactionUnlockFailed), new EndpointId(num), nullable);
                                this.m_failedSent[identityId] = true;
                            }
                            return;
                        }
                    }
                }
                EndpointId targetEndpoint = new EndpointId();
                nullable = null;
                MyMultiplayer.RaiseStaticEvent<long, SerializableDefinitionId, long>(x => new Action<long, SerializableDefinitionId, long>(MySessionComponentResearch.UnlockResearchSuccess), identityId, id2, unlockerId, targetEndpoint, nullable);
            }
        }

        public void UnlockResearchDirect(long characterId, MyDefinitionId itemId)
        {
            if (!itemId.TypeId.IsNull)
            {
                SerializableDefinitionId id = (SerializableDefinitionId) itemId;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, SerializableDefinitionId>(x => new Action<long, SerializableDefinitionId>(MySessionComponentResearch.UnlockResearchDirectSync), characterId, id, targetEndpoint, position);
            }
        }

        [Event(null, 600), Reliable, Server, Broadcast]
        private static void UnlockResearchDirectSync(long characterId, SerializableDefinitionId itemId)
        {
            MyDefinitionBase base2;
            if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(itemId, out base2) && (!Static.m_unlockedResearch.ContainsKey(characterId) || !Static.m_unlockedResearch[characterId].Contains(base2.Id)))
            {
                Static.UnlockBlocks(characterId, itemId);
            }
        }

        [Event(null, 0x11b), Reliable, Server, Broadcast]
        private static void UnlockResearchSuccess(long identityId, SerializableDefinitionId id, long unlockerId)
        {
            MyDefinitionBase base2;
            if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(id, out base2) && ((Static.UnlockBlocks(identityId, id) && (MySession.Static.LocalCharacter != null)) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == identityId)))
            {
                if (unlockerId != identityId)
                {
                    MyIdentity identity = MySession.Static.Players.TryGetIdentity(unlockerId);
                    if (identity != null)
                    {
                        object[] arguments = new object[] { base2.DisplayNameText, identity.DisplayName };
                        Static.m_factionUnlockedResearchNotification.SetTextFormatArguments(arguments);
                        MyHud.Notifications.Add(Static.m_factionUnlockedResearchNotification);
                    }
                }
                else
                {
                    object[] arguments = new object[] { base2.DisplayNameText };
                    Static.m_unlockedResearchNotification.SetTextFormatArguments(arguments);
                    MyHud.Notifications.Add(Static.m_unlockedResearchNotification);
                }
            }
        }

        public override bool IsRequiredByGame =>
            MyPerGameSettings.EnableResearch;

        public override System.Type[] Dependencies =>
            base.Dependencies;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySessionComponentResearch.<>c <>9 = new MySessionComponentResearch.<>c();
            public static Func<IMyEventOwner, Action> <>9__22_1;
            public static Func<IMyEventOwner, Action<long, SerializableDefinitionId, long>> <>9__22_0;
            public static Func<IMyEventOwner, Action<long, long>> <>9__30_0;
            public static Func<IMyEventOwner, Action<long>> <>9__33_0;
            public static Func<IMyEventOwner, Action<long>> <>9__34_0;
            public static Func<IMyEventOwner, Action<SerializableDefinitionId>> <>9__37_0;
            public static Func<IMyEventOwner, Action<SerializableDefinitionId>> <>9__39_0;
            public static Func<IMyEventOwner, Action> <>9__41_0;
            public static Func<IMyEventOwner, Action> <>9__43_0;
            public static Func<IMyEventOwner, Action<long, SerializableDefinitionId>> <>9__45_0;
            public static Func<IMyEventOwner, Action<long, SerializableDefinitionId>> <>9__47_0;

            internal Action<SerializableDefinitionId> <AddRequiredResearch>b__37_0(IMyEventOwner x) => 
                new Action<SerializableDefinitionId>(MySessionComponentResearch.AddRequiredResearchSync);

            internal Action<long, long> <CallShareResearch>b__30_0(IMyEventOwner x) => 
                new Action<long, long>(MySessionComponentResearch.ShareResearch);

            internal Action <ClearRequiredResearch>b__41_0(IMyEventOwner x) => 
                new Action(MySessionComponentResearch.ClearRequiredResearchSync);

            internal Action<long> <DebugUnlockAllResearch>b__34_0(IMyEventOwner x) => 
                new Action<long>(MySessionComponentResearch.DebugUnlockAllResearchSync);

            internal Action<long, SerializableDefinitionId> <LockResearch>b__45_0(IMyEventOwner x) => 
                new Action<long, SerializableDefinitionId>(MySessionComponentResearch.LockResearchSync);

            internal Action<SerializableDefinitionId> <RemoveRequiredResearch>b__39_0(IMyEventOwner x) => 
                new Action<SerializableDefinitionId>(MySessionComponentResearch.RemoveRequiredResearchSync);

            internal Action<long> <ResetResearch>b__33_0(IMyEventOwner x) => 
                new Action<long>(MySessionComponentResearch.ResetResearchSync);

            internal Action <ResetResearchForAll>b__43_0(IMyEventOwner x) => 
                new Action(MySessionComponentResearch.ResetResearchForAllSync);

            internal Action<long, SerializableDefinitionId, long> <UnlockResearch>b__22_0(IMyEventOwner x) => 
                new Action<long, SerializableDefinitionId, long>(MySessionComponentResearch.UnlockResearchSuccess);

            internal Action <UnlockResearch>b__22_1(IMyEventOwner x) => 
                new Action(MySessionComponentResearch.FactionUnlockFailed);

            internal Action<long, SerializableDefinitionId> <UnlockResearchDirect>b__47_0(IMyEventOwner x) => 
                new Action<long, SerializableDefinitionId>(MySessionComponentResearch.UnlockResearchDirectSync);
        }
    }
}

