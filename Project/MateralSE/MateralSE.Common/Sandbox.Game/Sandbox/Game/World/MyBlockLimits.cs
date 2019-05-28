namespace Sandbox.Game.World
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRage.Serialization;

    [StaticEventOwner]
    public class MyBlockLimits
    {
        public static readonly MyBlockLimits Empty = new MyBlockLimits(0, 0);
        private int m_blocksBuilt;
        private int m_PCUBuilt;
        private int m_PCU;
        [CompilerGenerated]
        private Action BlockLimitsChanged;

        public event Action BlockLimitsChanged
        {
            [CompilerGenerated] add
            {
                Action blockLimitsChanged = this.BlockLimitsChanged;
                while (true)
                {
                    Action a = blockLimitsChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    blockLimitsChanged = Interlocked.CompareExchange<Action>(ref this.BlockLimitsChanged, action3, a);
                    if (ReferenceEquals(blockLimitsChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action blockLimitsChanged = this.BlockLimitsChanged;
                while (true)
                {
                    Action source = blockLimitsChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    blockLimitsChanged = Interlocked.CompareExchange<Action>(ref this.BlockLimitsChanged, action3, source);
                    if (ReferenceEquals(blockLimitsChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyBlockLimits(int initialPCU, int blockLimitModifier)
        {
            this.BlockLimitModifier = blockLimitModifier;
            this.BlockTypeBuilt = new ConcurrentDictionary<string, MyTypeLimitData>();
            foreach (string str in MySession.Static.BlockTypeLimits.Keys)
            {
                MyTypeLimitData data1 = new MyTypeLimitData();
                data1.BlockPairName = str;
                this.BlockTypeBuilt.TryAdd(str, data1);
            }
            this.BlocksBuiltByGrid = new ConcurrentDictionary<long, MyGridLimitData>();
            this.GridsRemoved = new ConcurrentDictionary<long, MyGridLimitData>();
            this.m_PCU = initialPCU;
        }

        public void CallLimitsChanged()
        {
            if (this.BlockLimitsChanged != null)
            {
                this.BlockLimitsChanged();
            }
        }

        public void DecreaseBlocksBuilt(string type, int pcu, MyCubeGrid grid, bool modifyBlockCount = true)
        {
            if (!ReferenceEquals(Empty, this) && Sync.IsServer)
            {
                MyTypeLimitData data;
                if (modifyBlockCount)
                {
                    Interlocked.Decrement(ref this.m_blocksBuilt);
                }
                if (grid != null)
                {
                    Interlocked.Add(ref this.m_PCUBuilt, -pcu);
                    Interlocked.Add(ref this.m_PCU, pcu);
                }
                if (((type != null) & modifyBlockCount) && this.BlockTypeBuilt.TryGetValue(type, out data))
                {
                    Interlocked.Decrement(ref data.BlocksBuilt);
                    data.Dirty = 1;
                }
                if (grid != null)
                {
                    MyGridLimitData data2;
                    long entityId = grid.EntityId;
                    if (this.BlocksBuiltByGrid.TryGetValue(entityId, out data2))
                    {
                        if (modifyBlockCount)
                        {
                            Interlocked.Decrement(ref data2.BlocksBuilt);
                        }
                        Interlocked.Add(ref data2.PCUBuilt, -pcu);
                        data2.Dirty = 1;
                        if (data2.BlocksBuilt == 0)
                        {
                            this.BlocksBuiltByGrid.Remove<long, MyGridLimitData>(entityId);
                            this.GridsRemoved.TryAdd(entityId, data2);
                            grid.OnNameChanged -= new Action<MyCubeGrid>(this.OnGridNameChangedServer);
                        }
                    }
                }
            }
        }

        private HashSet<MySlimBlock> GetBlocksBuiltByPlayer(long playerId)
        {
            HashSet<MySlimBlock> builtBlocks = new HashSet<MySlimBlock>();
            foreach (KeyValuePair<long, MyGridLimitData> pair in this.BlocksBuiltByGrid)
            {
                MyCubeGrid grid;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(pair.Key, out grid, false))
                {
                    grid.FindBlocksBuiltByID(playerId, builtBlocks);
                }
            }
            return builtBlocks;
        }

        private static MyCubeGrid GetGridFromId(long gridEntityId)
        {
            VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(gridEntityId, false);
            if (entityById == null)
            {
                return null;
            }
            MyCubeGrid grid = entityById as MyCubeGrid;
            return ((grid != null) ? grid : null);
        }

        public static int GetInitialPCU(long identityId = -1L)
        {
            switch (MySession.Static.BlockLimitsEnabled)
            {
                case MyBlockLimitsEnabledEnum.NONE:
                    return 0x7fffffff;

                case MyBlockLimitsEnabledEnum.PER_FACTION:
                    if (((MySession.Static.BlockLimitsEnabled != MyBlockLimitsEnabledEnum.PER_FACTION) || (identityId == -1L)) || (MySession.Static.Factions.GetPlayerFaction(identityId) != null))
                    {
                        return ((MySession.Static.MaxFactionsCount != 0) ? (MySession.Static.TotalPCU / MySession.Static.MaxFactionsCount) : MySession.Static.TotalPCU);
                    }
                    return 0;

                case MyBlockLimitsEnabledEnum.PER_PLAYER:
                    return (MySession.Static.TotalPCU / MySession.Static.MaxPlayers);
            }
            return MySession.Static.TotalPCU;
        }

        public void IncreaseBlocksBuilt(string type, int pcu, MyCubeGrid grid, bool modifyBlockCount = true)
        {
            if (!ReferenceEquals(Empty, this) && Sync.IsServer)
            {
                MyTypeLimitData data;
                if (modifyBlockCount)
                {
                    Interlocked.Increment(ref this.m_blocksBuilt);
                }
                if (grid != null)
                {
                    Interlocked.Add(ref this.m_PCUBuilt, pcu);
                    Interlocked.Add(ref this.m_PCU, -pcu);
                }
                if ((modifyBlockCount && (type != null)) && this.BlockTypeBuilt.TryGetValue(type, out data))
                {
                    Interlocked.Increment(ref data.BlocksBuilt);
                    data.Dirty = 1;
                }
                if (grid != null)
                {
                    long entityId = grid.EntityId;
                    bool flag = false;
                    while (true)
                    {
                        MyGridLimitData data2;
                        if (this.BlocksBuiltByGrid.TryGetValue(entityId, out data2))
                        {
                            if (modifyBlockCount)
                            {
                                Interlocked.Increment(ref data2.BlocksBuilt);
                            }
                            Interlocked.Add(ref data2.PCUBuilt, pcu);
                            data2.Dirty = 1;
                        }
                        else
                        {
                            MyGridLimitData data1 = new MyGridLimitData();
                            data1.EntityId = grid.EntityId;
                            data1.BlocksBuilt = 1;
                            data1.PCUBuilt = pcu;
                            string displayName = grid.DisplayName;
                            data1.GridName = displayName ?? "Unknown grid";
                            MyGridLimitData local2 = data1;
                            local2.Dirty = 1;
                            if (this.BlocksBuiltByGrid.TryAdd(entityId, local2))
                            {
                                grid.OnNameChanged += new Action<MyCubeGrid>(this.OnGridNameChangedServer);
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                        if (!flag)
                        {
                            this.GridsRemoved.Remove<long, MyGridLimitData>(entityId);
                            break;
                        }
                    }
                }
            }
        }

        public static bool IsFactionChangePossible(long playerId, long newFaction)
        {
            if (MySession.Static.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.PER_FACTION)
            {
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(playerId);
                if (identity != null)
                {
                    HashSet<MySlimBlock> blocksBuiltByPlayer = identity.BlockLimits.GetBlocksBuiltByPlayer(playerId);
                    int count = blocksBuiltByPlayer.Count;
                    int num2 = blocksBuiltByPlayer.Sum<MySlimBlock>(x => x.BlockDefinition.PCU);
                    MyFaction faction = MySession.Static.Factions.TryGetFactionById(newFaction) as MyFaction;
                    if (faction == null)
                    {
                        return false;
                    }
                    MyBlockLimits blockLimits = faction.BlockLimits;
                    if ((num2 > blockLimits.PCU) && (MySession.Static.Settings.TotalPCU > 0))
                    {
                        return false;
                    }
                    if ((count > blockLimits.MaxBlocks) && (blockLimits.MaxBlocks > 0))
                    {
                        return false;
                    }
                    using (Dictionary<string, short>.Enumerator enumerator = MySession.Static.BlockTypeLimits.GetEnumerator())
                    {
                        while (true)
                        {
                            MyTypeLimitData data;
                            MyTypeLimitData data2;
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            KeyValuePair<string, short> current = enumerator.Current;
                            if (identity.BlockLimits.BlockTypeBuilt.TryGetValue(current.Key, out data) && (blockLimits.BlockTypeBuilt.TryGetValue(current.Key, out data2) && ((data.BlocksBuilt + data2.BlocksBuilt) > current.Value)))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static bool IsTransferBlocksBuiltByIDPossible(long gridEntityId, long oldOwner, long newOwner, out int blocksCount, out int pcu)
        {
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(oldOwner);
            MyIdentity identity2 = MySession.Static.Players.TryGetIdentity(newOwner);
            blocksCount = 0;
            pcu = 0;
            if ((identity == null) || (identity2 == null))
            {
                return false;
            }
            if (MySession.Static.BlockLimitsEnabled != MyBlockLimitsEnabledEnum.NONE)
            {
                MyGridLimitData data;
                string blockPairName;
                if (!identity.BlockLimits.BlocksBuiltByGrid.TryGetValue(gridEntityId, out data))
                {
                    return false;
                }
                MyCubeGrid gridFromId = GetGridFromId(gridEntityId);
                if (gridFromId == null)
                {
                    return false;
                }
                HashSet<MySlimBlock> source = gridFromId.FindBlocksBuiltByID(oldOwner);
                blocksCount = source.Count;
                pcu = source.Sum<MySlimBlock>(x => x.BlockDefinition.PCU);
                if (MySession.Static.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.GLOBALLY)
                {
                    return true;
                }
                if ((identity2.BlockLimits.MaxBlocks > 0) && ((blocksCount + identity2.BlockLimits.BlocksBuilt) > identity2.BlockLimits.MaxBlocks))
                {
                    return false;
                }
                if (pcu > identity2.BlockLimits.PCU)
                {
                    return false;
                }
                Dictionary<string, short> dictionary = new Dictionary<string, short>();
                foreach (MySlimBlock block in gridFromId.FindBlocksBuiltByID(oldOwner))
                {
                    if (MySession.Static.BlockTypeLimits.ContainsKey(block.BlockDefinition.BlockPairName))
                    {
                        if (!dictionary.ContainsKey(block.BlockDefinition.BlockPairName))
                        {
                            dictionary[block.BlockDefinition.BlockPairName] = 1;
                            continue;
                        }
                        blockPairName = block.BlockDefinition.BlockPairName;
                        dictionary[blockPairName] += 1;
                    }
                }
                foreach (KeyValuePair<string, MyTypeLimitData> pair in identity2.BlockLimits.BlockTypeBuilt)
                {
                    if (!dictionary.ContainsKey(pair.Key))
                    {
                        dictionary[pair.Key] = (short) pair.Value.BlocksBuilt;
                        continue;
                    }
                    Dictionary<string, short> dictionary2 = dictionary;
                    blockPairName = pair.Key;
                    dictionary2[blockPairName] += (short) pair.Value.BlocksBuilt;
                }
                using (Dictionary<string, short>.Enumerator enumerator3 = dictionary.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator3.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<string, short> current = enumerator3.Current;
                        if (current.Value > MySession.Static.BlockTypeLimits[current.Key])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void OnGridNameChangedServer(MyCubeGrid grid)
        {
            MyGridLimitData data;
            if (this.BlocksBuiltByGrid.TryGetValue(grid.EntityId, out data))
            {
                data.GridName = grid.DisplayName;
                data.NameDirty = 1;
            }
        }

        [Event(null, 0x16f), Reliable, Client]
        private static void ReceiveTransferNotPossibleMessage(long identityId)
        {
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(identityId);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxTextNotEnoughFreeBlocksForTransfer, identity.DisplayName), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, false, size));
        }

        [Event(null, 0x15b), Reliable, Client]
        private static void ReceiveTransferRequestMessage(TransferMessageData gridData, long oldOwner, long newOwner, int blocksCount, int pcu)
        {
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(oldOwner);
            object[] args = new object[] { identity.DisplayName, blocksCount.ToString(), pcu, gridData.GridName };
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextConfirmAcceptTransferGrid), args), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long, long, long>(x => new Action<long, long, long>(MyBlockLimits.TransferBlocksBuiltByID), gridData.EntityId, oldOwner, newOwner, targetEndpoint, position);
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, false, size));
        }

        [Event(null, 0x1a7), Reliable, Server]
        public static void RemoveBlocksBuiltByID(long gridEntityId, long identityID)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (MySession.Static.Players.TryGetSteamId(identityID) != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyCubeGrid gridFromId = GetGridFromId(gridEntityId);
                if (gridFromId != null)
                {
                    gridFromId.RemoveBlocksBuiltByID(identityID);
                }
            }
        }

        [Event(null, 0x12e), Reliable, Server]
        public static void SendTransferRequestMessage(MyGridLimitData gridData, long oldOwner, long newOwner, ulong newOwnerSteamId)
        {
            int num;
            int num2;
            Vector3D? nullable;
            if (!IsTransferBlocksBuiltByIDPossible(gridData.EntityId, oldOwner, newOwner, out num, out num2))
            {
                nullable = null;
                MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(MyBlockLimits.ReceiveTransferNotPossibleMessage), newOwner, MyEventContext.Current.Sender, nullable);
            }
            else
            {
                TransferMessageData data2;
                MyGridLimitData data = MySession.Static.Players.TryGetIdentity(oldOwner).BlockLimits.BlocksBuiltByGrid[gridData.EntityId];
                if (MyEventContext.Current.IsLocallyInvoked)
                {
                    data2 = new TransferMessageData {
                        EntityId = data.EntityId,
                        GridName = data.GridName,
                        BlocksBuilt = data.BlocksBuilt,
                        PCUBuilt = data.PCUBuilt
                    };
                    ReceiveTransferRequestMessage(data2, oldOwner, newOwner, num, num2);
                }
                else
                {
                    data2 = new TransferMessageData {
                        EntityId = data.EntityId,
                        GridName = data.GridName,
                        BlocksBuilt = data.BlocksBuilt,
                        PCUBuilt = data.PCUBuilt
                    };
                    nullable = null;
                    MyMultiplayer.RaiseStaticEvent<TransferMessageData, long, long, int, int>(x => new Action<TransferMessageData, long, long, int, int>(MyBlockLimits.ReceiveTransferRequestMessage), data2, oldOwner, newOwner, num, num2, new EndpointId(newOwnerSteamId), nullable);
                }
            }
        }

        public void SetAllDirty()
        {
            using (IEnumerator<MyTypeLimitData> enumerator = this.BlockTypeBuilt.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Dirty = 1;
                }
            }
            foreach (MyGridLimitData local1 in this.BlocksBuiltByGrid.Values)
            {
                local1.Dirty = 1;
                local1.NameDirty = 1;
            }
        }

        public void SetGridLimitsFromServer(MyGridLimitData newLimit, int pcu, int pcuBuilt, int blocksBuilt)
        {
            MyGridLimitData data;
            Interlocked.Exchange(ref this.m_PCU, pcu);
            Interlocked.Exchange(ref this.m_PCUBuilt, pcuBuilt);
            Interlocked.Exchange(ref this.m_blocksBuilt, blocksBuilt);
            if (newLimit.BlocksBuilt == 0)
            {
                this.BlocksBuiltByGrid.TryRemove(newLimit.EntityId, out data);
            }
            else if (!this.BlocksBuiltByGrid.TryAdd(newLimit.EntityId, newLimit))
            {
                data = this.BlocksBuiltByGrid[newLimit.EntityId];
                data.BlocksBuilt = newLimit.BlocksBuilt;
                data.PCUBuilt = newLimit.PCUBuilt;
            }
            this.CallLimitsChanged();
        }

        [Event(null, 0x1b9), Reliable, Client]
        public static void SetGridNameFromServer(long gridEntityId, string newName)
        {
            MyGridLimitData data;
            MyIdentity identity = MySession.Static.LocalHumanPlayer.Identity;
            if (identity.BlockLimits.BlocksBuiltByGrid.TryGetValue(gridEntityId, out data))
            {
                data.GridName = newName;
            }
            identity.BlockLimits.CallLimitsChanged();
        }

        public void SetTypeLimitsFromServer(MyTypeLimitData newLimit)
        {
            if (!this.BlockTypeBuilt.ContainsKey(newLimit.BlockPairName))
            {
                this.BlockTypeBuilt[newLimit.BlockPairName] = new MyTypeLimitData();
            }
            this.BlockTypeBuilt[newLimit.BlockPairName].BlocksBuilt = newLimit.BlocksBuilt;
            this.CallLimitsChanged();
        }

        public static void TransferBlockLimits(long oldOwner, long newOwner)
        {
            foreach (KeyValuePair<long, MyGridLimitData> pair in MySession.Static.Players.TryGetIdentity(oldOwner).BlockLimits.BlocksBuiltByGrid)
            {
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, long, long>(x => new Action<long, long, long>(MyBlockLimits.TransferBlocksBuiltByID), pair.Key, oldOwner, newOwner, new EndpointId(Sync.MyId), position);
            }
        }

        public static void TransferBlockLimits(long playerId, MyBlockLimits oldLimits, MyBlockLimits newLimits)
        {
            foreach (KeyValuePair<long, MyGridLimitData> pair in oldLimits.BlocksBuiltByGrid)
            {
                MyCubeGrid entity = null;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(pair.Key, out entity, false))
                {
                    entity.TransferBlockLimitsBuiltByID(playerId, oldLimits, newLimits);
                }
            }
        }

        [Event(null, 380), Reliable, Server]
        private static void TransferBlocksBuiltByID(long gridEntityId, long oldOwner, long newOwner)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (MySession.Static.Players.TryGetSteamId(newOwner) != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyCubeGrid gridFromId = GetGridFromId(gridEntityId);
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(newOwner);
                if ((gridFromId != null) && (identity != null))
                {
                    gridFromId.TransferBlocksBuiltByID(oldOwner, newOwner);
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long, long, long>(x => new Action<long, long, long>(MyBlockLimits.TransferBlocksBuiltByIDClient), gridFromId.EntityId, oldOwner, newOwner, targetEndpoint, position);
                    ulong num = MySession.Static.Players.TryGetSteamId(identity.IdentityId);
                    if (num != 0)
                    {
                        position = null;
                        MyMultiplayer.RaiseStaticEvent<long, string>(x => new Action<long, string>(MyBlockLimits.SetGridNameFromServer), gridFromId.EntityId, gridFromId.DisplayName, new EndpointId(num), position);
                    }
                }
            }
        }

        [Event(null, 0x199), Reliable, BroadcastExcept]
        public static void TransferBlocksBuiltByIDClient(long gridEntityId, long oldOwner, long newOwner)
        {
            VRage.Game.Entity.MyEntity entity;
            Sandbox.Game.Entities.MyEntities.TryGetEntityById(gridEntityId, out entity, false);
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid != null)
            {
                grid.TransferBlocksBuiltByIDClient(oldOwner, newOwner);
            }
        }

        public int BlockLimitModifier { get; set; }

        public ConcurrentDictionary<string, MyTypeLimitData> BlockTypeBuilt { get; private set; }

        public ConcurrentDictionary<long, MyGridLimitData> BlocksBuiltByGrid { get; private set; }

        public ConcurrentDictionary<long, MyGridLimitData> GridsRemoved { get; private set; }

        public int BlocksBuilt =>
            this.m_blocksBuilt;

        public int PCU =>
            this.m_PCU;

        public int PCUBuilt =>
            this.m_PCUBuilt;

        public int MaxBlocks =>
            (MySession.Static.MaxBlocksPerPlayer + this.BlockLimitModifier);

        public bool HasRemainingPCU =>
            (this.m_PCU > 0);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyBlockLimits.<>c <>9 = new MyBlockLimits.<>c();
            public static Func<MySlimBlock, int> <>9__37_0;
            public static Func<IMyEventOwner, Action<long, long, long>> <>9__39_0;
            public static Func<MySlimBlock, int> <>9__40_0;
            public static Func<IMyEventOwner, Action<long>> <>9__42_0;
            public static Func<IMyEventOwner, Action<MyBlockLimits.TransferMessageData, long, long, int, int>> <>9__42_1;
            public static Func<IMyEventOwner, Action<long, long, long>> <>9__44_1;
            public static Func<IMyEventOwner, Action<long, long, long>> <>9__46_0;
            public static Func<IMyEventOwner, Action<long, string>> <>9__46_1;

            internal int <IsFactionChangePossible>b__37_0(MySlimBlock x) => 
                x.BlockDefinition.PCU;

            internal int <IsTransferBlocksBuiltByIDPossible>b__40_0(MySlimBlock x) => 
                x.BlockDefinition.PCU;

            internal Action<long, long, long> <ReceiveTransferRequestMessage>b__44_1(IMyEventOwner x) => 
                new Action<long, long, long>(MyBlockLimits.TransferBlocksBuiltByID);

            internal Action<long> <SendTransferRequestMessage>b__42_0(IMyEventOwner x) => 
                new Action<long>(MyBlockLimits.ReceiveTransferNotPossibleMessage);

            internal Action<MyBlockLimits.TransferMessageData, long, long, int, int> <SendTransferRequestMessage>b__42_1(IMyEventOwner x) => 
                new Action<MyBlockLimits.TransferMessageData, long, long, int, int>(MyBlockLimits.ReceiveTransferRequestMessage);

            internal Action<long, long, long> <TransferBlockLimits>b__39_0(IMyEventOwner x) => 
                new Action<long, long, long>(MyBlockLimits.TransferBlocksBuiltByID);

            internal Action<long, long, long> <TransferBlocksBuiltByID>b__46_0(IMyEventOwner x) => 
                new Action<long, long, long>(MyBlockLimits.TransferBlocksBuiltByIDClient);

            internal Action<long, string> <TransferBlocksBuiltByID>b__46_1(IMyEventOwner x) => 
                new Action<long, string>(MyBlockLimits.SetGridNameFromServer);
        }

        public class MyGridLimitData
        {
            public long EntityId;
            [NoSerialize]
            public string GridName;
            public int BlocksBuilt;
            public int PCUBuilt;
            [NoSerialize]
            public int Dirty;
            [NoSerialize]
            public int NameDirty;
        }

        public class MyTypeLimitData
        {
            public string BlockPairName;
            public int BlocksBuilt;
            [NoSerialize]
            public int Dirty;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TransferMessageData
        {
            public long EntityId;
            public string GridName;
            public int BlocksBuilt;
            public int PCUBuilt;
        }
    }
}

