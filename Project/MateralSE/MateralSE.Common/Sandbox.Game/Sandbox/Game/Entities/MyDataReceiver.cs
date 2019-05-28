namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Gui;
    using VRage.Groups;

    public abstract class MyDataReceiver : MyEntityComponentBase
    {
        protected List<MyDataBroadcaster> m_tmpBroadcasters = new List<MyDataBroadcaster>();
        protected HashSet<MyDataBroadcaster> m_broadcastersInRange = new HashSet<MyDataBroadcaster>();
        private HashSet<MyGridLogicalGroupData> m_broadcastersInRange_TopGrids = new HashSet<MyGridLogicalGroupData>();
        private HashSet<long> m_entitiesOnHud = new HashSet<long>();

        protected MyDataReceiver()
        {
        }

        public bool CanBeUsedByPlayer(long playerId) => 
            MyDataBroadcaster.CanBeUsedByPlayer(playerId, base.Entity);

        public void Clear()
        {
            foreach (long num in this.m_entitiesOnHud)
            {
                MyHud.LocationMarkers.UnregisterMarker(num);
            }
            this.m_entitiesOnHud.Clear();
            this.m_broadcastersInRange_TopGrids.Clear();
        }

        protected abstract void GetBroadcastersInMyRange(ref HashSet<MyDataBroadcaster> broadcastersInRange);
        public bool HasAccessToLogicalGroup(MyGridLogicalGroupData group) => 
            this.m_broadcastersInRange_TopGrids.Contains(group);

        public void UpdateBroadcastersInRange()
        {
            this.m_broadcastersInRange.Clear();
            if (MyFakes.ENABLE_RADIO_HUD && this.Enabled)
            {
                MyDataBroadcaster broadcaster;
                if (base.Entity.Components.TryGet<MyDataBroadcaster>(out broadcaster))
                {
                    this.m_broadcastersInRange.Add(broadcaster);
                }
                this.GetBroadcastersInMyRange(ref this.m_broadcastersInRange);
            }
        }

        public void UpdateHud(bool showMyself = false)
        {
            if ((!Game.IsDedicated && !MyHud.MinimalHud) && !MyHud.CutsceneHud)
            {
                this.Clear();
                foreach (MyDataBroadcaster broadcaster in MyAntennaSystem.Static.GetAllRelayedBroadcasters(this, MySession.Static.LocalPlayerId, false, null))
                {
                    bool allowBlink = broadcaster.CanBeUsedByPlayer(MySession.Static.LocalPlayerId);
                    MyCubeGrid topMostParent = broadcaster.Entity.GetTopMostParent(null) as MyCubeGrid;
                    if (topMostParent != null)
                    {
                        MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(topMostParent);
                        if (group != null)
                        {
                            MyGridLogicalGroupData groupData = group.GroupData;
                            this.m_broadcastersInRange_TopGrids.Add(groupData);
                        }
                    }
                    if (broadcaster.ShowOnHud)
                    {
                        foreach (MyHudEntityParams @params in broadcaster.GetHudParams(allowBlink))
                        {
                            if (this.m_entitiesOnHud.Contains(@params.EntityId))
                            {
                                continue;
                            }
                            this.m_entitiesOnHud.Add(@params.EntityId);
                            if (@params.BlinkingTime > 0f)
                            {
                                MyHud.HackingMarkers.RegisterMarker(@params.EntityId, @params);
                                continue;
                            }
                            if (!MyHud.HackingMarkers.MarkerEntities.ContainsKey(@params.EntityId))
                            {
                                MyHud.LocationMarkers.RegisterMarker(@params.EntityId, @params);
                            }
                        }
                    }
                }
                if (MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.ShowPlayers))
                {
                    using (IEnumerator<MyPlayer> enumerator3 = MySession.Static.Players.GetOnlinePlayers().GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            MyCharacter character = enumerator3.Current.Character;
                            if (character != null)
                            {
                                foreach (MyHudEntityParams params2 in character.GetHudParams(false))
                                {
                                    if (!this.m_entitiesOnHud.Contains(params2.EntityId))
                                    {
                                        this.m_entitiesOnHud.Add(params2.EntityId);
                                        MyHud.LocationMarkers.RegisterMarker(params2.EntityId, params2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool Enabled { get; set; }

        public HashSet<MyDataBroadcaster> BroadcastersInRange =>
            this.m_broadcastersInRange;

        public MyDataBroadcaster Broadcaster
        {
            get
            {
                MyDataBroadcaster component = null;
                if (base.Container != null)
                {
                    base.Container.TryGet<MyDataBroadcaster>(out component);
                }
                return component;
            }
        }

        public override string ComponentTypeDebugString =>
            "MyDataReciever";
    }
}

