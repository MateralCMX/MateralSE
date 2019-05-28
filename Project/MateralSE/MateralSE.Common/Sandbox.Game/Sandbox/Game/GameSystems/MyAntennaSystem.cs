namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Groups;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 0x24c, typeof(MyObjectBuilder_AntennaSessionComponent), (Type) null)]
    public class MyAntennaSystem : MySessionComponentBase
    {
        private static MyAntennaSystem m_static;
        private List<long> m_addedItems = new List<long>();
        private HashSet<BroadcasterInfo> m_output = new HashSet<BroadcasterInfo>(new BroadcasterInfoComparer());
        private HashSet<MyDataBroadcaster> m_tempPlayerRelayedBroadcasters = new HashSet<MyDataBroadcaster>();
        private List<MyDataBroadcaster> m_tempGridBroadcastersFromPlayer = new List<MyDataBroadcaster>();
        private HashSet<MyDataReceiver> m_tmpReceivers = new HashSet<MyDataReceiver>();
        private HashSet<MyDataBroadcaster> m_tmpBroadcasters = new HashSet<MyDataBroadcaster>();
        private HashSet<MyDataBroadcaster> m_tmpRelayedBroadcasters = new HashSet<MyDataBroadcaster>();
        private Dictionary<long, MyLaserBroadcaster> m_laserAntennas = new Dictionary<long, MyLaserBroadcaster>();
        private Dictionary<long, MyProxyAntenna> m_proxyAntennas = new Dictionary<long, MyProxyAntenna>();
        private Dictionary<long, HashSet<MyDataBroadcaster>> m_proxyGrids = new Dictionary<long, HashSet<MyDataBroadcaster>>();

        public void AddLaser(long id, MyLaserBroadcaster laser, bool register = true)
        {
            if (register)
            {
                this.RegisterAntenna(laser);
            }
            this.m_laserAntennas.Add(id, laser);
        }

        public bool CheckConnection(MyIdentity sender, MyIdentity receiver) => 
            (!ReferenceEquals(sender, receiver) ? ((sender.Character != null) && ((receiver.Character != null) && this.CheckConnection((MyDataReceiver) receiver.Character.RadioReceiver, (MyDataBroadcaster) sender.Character.RadioBroadcaster, receiver.IdentityId, false))) : true);

        public bool CheckConnection(MyDataReceiver receiver, MyDataBroadcaster broadcaster, long playerIdentityId, bool mutual) => 
            ((receiver != null) && ((broadcaster != null) && this.GetAllRelayedBroadcasters(receiver, playerIdentityId, mutual, null).Contains(broadcaster)));

        public bool CheckConnection(MyDataReceiver receiver, MyEntity broadcastingEntity, long playerIdentityId, bool mutual)
        {
            if ((receiver != null) && (broadcastingEntity != null))
            {
                this.m_tmpBroadcasters.Clear();
                this.m_tmpRelayedBroadcasters.Clear();
                this.GetAllRelayedBroadcasters(receiver, playerIdentityId, mutual, this.m_tmpRelayedBroadcasters);
                this.GetEntityBroadcasters(broadcastingEntity, ref this.m_tmpBroadcasters, playerIdentityId);
                using (HashSet<MyDataBroadcaster>.Enumerator enumerator = this.m_tmpRelayedBroadcasters.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyDataBroadcaster current = enumerator.Current;
                        if (this.m_tmpBroadcasters.Contains(current))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool CheckConnection(MyEntity receivingEntity, MyDataBroadcaster broadcaster, long playerIdentityId, bool mutual) => 
            ((receivingEntity != null) && ((broadcaster != null) && this.GetAllRelayedBroadcasters(receivingEntity, playerIdentityId, mutual, null).Contains(broadcaster)));

        public bool CheckConnection(MyEntity broadcastingEntity, MyEntity receivingEntity, MyPlayer player, bool mutual = true)
        {
            MyCubeGrid grid = broadcastingEntity as MyCubeGrid;
            if (grid != null)
            {
                broadcastingEntity = this.GetLogicalGroupRepresentative(grid);
            }
            MyCubeGrid grid2 = receivingEntity as MyCubeGrid;
            if (grid2 != null)
            {
                receivingEntity = this.GetLogicalGroupRepresentative(grid2);
            }
            using (HashSet<BroadcasterInfo>.Enumerator enumerator = this.GetConnectedGridsInfo(receivingEntity, player, mutual).GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.EntityId == broadcastingEntity.EntityId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public HashSet<MyDataBroadcaster> GetAllRelayedBroadcasters(MyDataReceiver receiver, long identityId, bool mutual, HashSet<MyDataBroadcaster> output = null)
        {
            if (output == null)
            {
                output = this.m_tmpBroadcasters;
                output.Clear();
            }
            foreach (MyDataBroadcaster broadcaster in receiver.BroadcastersInRange)
            {
                if (output.Contains(broadcaster))
                {
                    continue;
                }
                if (!broadcaster.Closed)
                {
                    if (mutual)
                    {
                        if (broadcaster.Receiver == null)
                        {
                            continue;
                        }
                        if (receiver.Broadcaster == null)
                        {
                            continue;
                        }
                        if (!broadcaster.Receiver.BroadcastersInRange.Contains(receiver.Broadcaster))
                        {
                            continue;
                        }
                    }
                    output.Add(broadcaster);
                    if ((broadcaster.Receiver != null) && broadcaster.CanBeUsedByPlayer(identityId))
                    {
                        this.GetAllRelayedBroadcasters(broadcaster.Receiver, identityId, mutual, output);
                    }
                }
            }
            return output;
        }

        public HashSet<MyDataBroadcaster> GetAllRelayedBroadcasters(MyEntity entity, long identityId, bool mutual = true, HashSet<MyDataBroadcaster> output = null)
        {
            if (output == null)
            {
                output = this.m_tmpBroadcasters;
                output.Clear();
            }
            this.m_tmpReceivers.Clear();
            this.GetEntityReceivers(entity, ref this.m_tmpReceivers, identityId);
            foreach (MyDataReceiver receiver in this.m_tmpReceivers)
            {
                this.GetAllRelayedBroadcasters(receiver, identityId, mutual, output);
            }
            return output;
        }

        public MyEntity GetBroadcasterParentEntity(MyDataBroadcaster broadcaster) => 
            (!(broadcaster.Entity is MyCubeBlock) ? (broadcaster.Entity as MyEntity) : (broadcaster.Entity as MyCubeBlock).CubeGrid);

        public HashSet<BroadcasterInfo> GetConnectedGridsInfo(MyEntity interactedEntityRepresentative, MyPlayer player = null, bool mutual = true)
        {
            this.m_output.Clear();
            if (player == null)
            {
                player = MySession.Static.LocalHumanPlayer;
                if (player == null)
                {
                    return this.m_output;
                }
            }
            MyIdentity identity = player.Identity;
            this.m_tmpReceivers.Clear();
            this.m_tmpRelayedBroadcasters.Clear();
            if (interactedEntityRepresentative != null)
            {
                BroadcasterInfo item = new BroadcasterInfo {
                    EntityId = interactedEntityRepresentative.EntityId,
                    Name = interactedEntityRepresentative.DisplayName
                };
                this.m_output.Add(item);
                this.GetAllRelayedBroadcasters(interactedEntityRepresentative, identity.IdentityId, mutual, this.m_tmpRelayedBroadcasters);
                foreach (MyDataBroadcaster broadcaster in this.m_tmpRelayedBroadcasters)
                {
                    this.m_output.Add(broadcaster.Info);
                }
            }
            return this.m_output;
        }

        public static void GetCubeGridGroupBroadcasters(MyCubeGrid grid, HashSet<MyDataBroadcaster> output, long playerId = 0L)
        {
            MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(grid);
            if (group != null)
            {
                using (HashSet<MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node>.Enumerator enumerator = group.m_members.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        foreach (MyDataBroadcaster broadcaster in enumerator.Current.NodeData.GridSystems.RadioSystem.Broadcasters)
                        {
                            if ((playerId == 0) || broadcaster.CanBeUsedByPlayer(playerId))
                            {
                                output.Add(broadcaster);
                            }
                        }
                    }
                    return;
                }
            }
            foreach (MyDataBroadcaster broadcaster2 in grid.GridSystems.RadioSystem.Broadcasters)
            {
                if ((playerId == 0) || broadcaster2.CanBeUsedByPlayer(playerId))
                {
                    output.Add(broadcaster2);
                }
            }
        }

        private void GetCubeGridGroupReceivers(MyCubeGrid grid, ref HashSet<MyDataReceiver> output, long playerId = 0L)
        {
            MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(grid);
            if (group != null)
            {
                using (HashSet<MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node>.Enumerator enumerator = group.m_members.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        foreach (MyDataReceiver receiver in enumerator.Current.NodeData.GridSystems.RadioSystem.Receivers)
                        {
                            if ((playerId == 0) || receiver.CanBeUsedByPlayer(playerId))
                            {
                                output.Add(receiver);
                            }
                        }
                    }
                    return;
                }
            }
            foreach (MyDataReceiver receiver2 in grid.GridSystems.RadioSystem.Receivers)
            {
                if ((playerId == 0) || receiver2.CanBeUsedByPlayer(playerId))
                {
                    output.Add(receiver2);
                }
            }
        }

        public void GetEntityBroadcasters(MyEntity entity, ref HashSet<MyDataBroadcaster> output, long playerId = 0L)
        {
            MyCharacter character = entity as MyCharacter;
            if (character != null)
            {
                output.Add(character.RadioBroadcaster);
                MyCubeGrid topMostParent = character.GetTopMostParent(null) as MyCubeGrid;
                if (topMostParent != null)
                {
                    GetCubeGridGroupBroadcasters(topMostParent, output, playerId);
                }
            }
            else
            {
                MyCubeBlock block = entity as MyCubeBlock;
                if (block != null)
                {
                    GetCubeGridGroupBroadcasters(block.CubeGrid, output, playerId);
                }
                else
                {
                    MyCubeGrid grid = entity as MyCubeGrid;
                    if (grid != null)
                    {
                        GetCubeGridGroupBroadcasters(grid, output, playerId);
                    }
                    else
                    {
                        MyProxyAntenna proxy = entity as MyProxyAntenna;
                        if (proxy != null)
                        {
                            this.GetProxyGridBroadcasters(proxy, ref output, playerId);
                        }
                    }
                }
            }
        }

        public void GetEntityReceivers(MyEntity entity, ref HashSet<MyDataReceiver> output, long playerId = 0L)
        {
            MyCharacter character = entity as MyCharacter;
            if (character != null)
            {
                output.Add(character.RadioReceiver);
                MyCubeGrid topMostParent = character.GetTopMostParent(null) as MyCubeGrid;
                if (topMostParent != null)
                {
                    this.GetCubeGridGroupReceivers(topMostParent, ref output, playerId);
                }
            }
            else
            {
                MyCubeBlock block = entity as MyCubeBlock;
                if (block != null)
                {
                    this.GetCubeGridGroupReceivers(block.CubeGrid, ref output, playerId);
                }
                else
                {
                    MyCubeGrid grid = entity as MyCubeGrid;
                    if (grid != null)
                    {
                        this.GetCubeGridGroupReceivers(grid, ref output, playerId);
                    }
                    else
                    {
                        MyProxyAntenna proxy = entity as MyProxyAntenna;
                        if (proxy != null)
                        {
                            this.GetProxyGridReceivers(proxy, ref output, playerId);
                        }
                    }
                }
            }
        }

        public MyCubeGrid GetLogicalGroupRepresentative(MyCubeGrid grid)
        {
            MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(grid);
            if ((group == null) || (group.Nodes.Count == 0))
            {
                return grid;
            }
            MyCubeGrid nodeData = group.Nodes.First().NodeData;
            foreach (MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node node in group.Nodes)
            {
                if (node.NodeData.GetBlocks().Count > nodeData.GetBlocks().Count)
                {
                    nodeData = node.NodeData;
                }
            }
            return nodeData;
        }

        private void GetProxyGridBroadcasters(MyProxyAntenna proxy, ref HashSet<MyDataBroadcaster> output, long playerId = 0L)
        {
            HashSet<MyDataBroadcaster> set;
            if (this.m_proxyGrids.TryGetValue(proxy.Info.EntityId, out set))
            {
                foreach (MyDataBroadcaster broadcaster in set)
                {
                    if ((playerId == 0) || broadcaster.CanBeUsedByPlayer(playerId))
                    {
                        output.Add(broadcaster);
                    }
                }
            }
        }

        private void GetProxyGridReceivers(MyProxyAntenna proxy, ref HashSet<MyDataReceiver> output, long playerId = 0L)
        {
            HashSet<MyDataBroadcaster> set;
            if (this.m_proxyGrids.TryGetValue(proxy.Info.EntityId, out set))
            {
                foreach (MyDataBroadcaster broadcaster in set)
                {
                    if (broadcaster.Receiver == null)
                    {
                        continue;
                    }
                    if ((playerId == 0) || broadcaster.CanBeUsedByPlayer(playerId))
                    {
                        output.Add(broadcaster.Receiver);
                    }
                }
            }
        }

        public override void LoadData()
        {
            m_static = this;
            base.LoadData();
        }

        public void RegisterAntenna(MyDataBroadcaster broadcaster)
        {
            if (broadcaster.Entity is MyProxyAntenna)
            {
                MyProxyAntenna entity = broadcaster.Entity as MyProxyAntenna;
                this.m_proxyAntennas[broadcaster.AntennaEntityId] = entity;
                this.RegisterProxyGrid(broadcaster);
                if (MyEntities.GetEntityById(broadcaster.AntennaEntityId, false) == null)
                {
                    entity.Active = true;
                }
            }
            else
            {
                MyProxyAntenna antenna2;
                if (this.m_proxyAntennas.TryGetValue(broadcaster.AntennaEntityId, out antenna2))
                {
                    antenna2.Active = false;
                }
            }
        }

        private void RegisterProxyGrid(MyDataBroadcaster broadcaster)
        {
            HashSet<MyDataBroadcaster> set;
            if (!this.m_proxyGrids.TryGetValue(broadcaster.Info.EntityId, out set))
            {
                set = new HashSet<MyDataBroadcaster>();
                this.m_proxyGrids.Add(broadcaster.Info.EntityId, set);
            }
            set.Add(broadcaster);
        }

        public void RemoveLaser(long id, bool register = true)
        {
            MyLaserBroadcaster broadcaster;
            if (this.m_laserAntennas.TryGetValue(id, out broadcaster))
            {
                this.m_laserAntennas.Remove(id);
                if (register)
                {
                    this.UnregisterAntenna(broadcaster);
                }
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            this.m_addedItems.Clear();
            this.m_addedItems = null;
            this.m_output.Clear();
            this.m_output = null;
            this.m_tempGridBroadcastersFromPlayer.Clear();
            this.m_tempGridBroadcastersFromPlayer = null;
            this.m_tempPlayerRelayedBroadcasters.Clear();
            this.m_tempPlayerRelayedBroadcasters = null;
            m_static = null;
        }

        public void UnregisterAntenna(MyDataBroadcaster broadcaster)
        {
            if (broadcaster.Entity is MyProxyAntenna)
            {
                this.m_proxyAntennas.Remove(broadcaster.AntennaEntityId);
                this.UnregisterProxyGrid(broadcaster);
                (broadcaster.Entity as MyProxyAntenna).Active = false;
            }
            else
            {
                MyProxyAntenna antenna;
                if (this.m_proxyAntennas.TryGetValue(broadcaster.AntennaEntityId, out antenna))
                {
                    antenna.Active = true;
                }
            }
        }

        private void UnregisterProxyGrid(MyDataBroadcaster broadcaster)
        {
            HashSet<MyDataBroadcaster> set;
            if (this.m_proxyGrids.TryGetValue(broadcaster.Info.EntityId, out set))
            {
                set.Remove(broadcaster);
                if (set.Count == 0)
                {
                    this.m_proxyGrids.Remove(broadcaster.Info.EntityId);
                }
            }
        }

        public static MyAntennaSystem Static =>
            m_static;

        public Dictionary<long, MyLaserBroadcaster> LaserAntennas =>
            this.m_laserAntennas;

        [StructLayout(LayoutKind.Sequential)]
        public struct BroadcasterInfo
        {
            public long EntityId;
            public string Name;
        }

        public class BroadcasterInfoComparer : IEqualityComparer<MyAntennaSystem.BroadcasterInfo>
        {
            public bool Equals(MyAntennaSystem.BroadcasterInfo x, MyAntennaSystem.BroadcasterInfo y) => 
                ((x.EntityId == y.EntityId) && string.Equals(x.Name, y.Name));

            public int GetHashCode(MyAntennaSystem.BroadcasterInfo obj)
            {
                int hashCode = obj.EntityId.GetHashCode();
                if (obj.Name != null)
                {
                    hashCode = (hashCode * 0x18d) ^ obj.Name.GetHashCode();
                }
                return hashCode;
            }
        }
    }
}

