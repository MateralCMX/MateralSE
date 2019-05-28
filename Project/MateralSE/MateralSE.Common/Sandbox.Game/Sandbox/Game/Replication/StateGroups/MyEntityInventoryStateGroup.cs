namespace Sandbox.Game.Replication.StateGroups
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    internal class MyEntityInventoryStateGroup : IMyStateGroup, IMyNetObject, IMyEventOwner
    {
        private readonly int m_inventoryIndex;
        private Dictionary<Endpoint, InventoryClientData> m_clientInventoryUpdate;
        private List<MyPhysicalInventoryItem> m_itemsToSend;
        private HashSet<uint> m_foundDeltaItems;
        private uint m_nextExpectedPacketId;
        private readonly SortedList<uint, InventoryDeltaInformation> m_buffer;
        private Dictionary<int, MyPhysicalInventoryItem> m_tmpSwappingList;

        public MyEntityInventoryStateGroup(MyInventory entity, bool attach, IMyReplicable owner)
        {
            this.Inventory = entity;
            if (attach)
            {
                this.Inventory.ContentsChanged += new Action<MyInventoryBase>(this.InventoryChanged);
            }
            this.Owner = owner;
            if (!Sync.IsServer)
            {
                this.m_buffer = new SortedList<uint, InventoryDeltaInformation>();
            }
        }

        private void ApplyChangesOnClient(InventoryDeltaInformation changes)
        {
            if (changes.ChangedItems != null)
            {
                foreach (KeyValuePair<uint, MyFixedPoint> pair in changes.ChangedItems)
                {
                    this.Inventory.UpdateItemAmoutClient(pair.Key, pair.Value);
                }
            }
            if (changes.RemovedItems != null)
            {
                foreach (uint num in changes.RemovedItems)
                {
                    this.Inventory.RemoveItemClient(num);
                }
            }
            if (changes.NewItems != null)
            {
                foreach (KeyValuePair<int, MyPhysicalInventoryItem> pair2 in changes.NewItems)
                {
                    this.Inventory.AddItemClient(pair2.Key, pair2.Value);
                }
            }
            if (changes.SwappedItems != null)
            {
                if (this.m_tmpSwappingList == null)
                {
                    this.m_tmpSwappingList = new Dictionary<int, MyPhysicalInventoryItem>();
                }
                foreach (KeyValuePair<uint, int> pair3 in changes.SwappedItems)
                {
                    MyPhysicalInventoryItem? itemByID = this.Inventory.GetItemByID(pair3.Key);
                    if (itemByID != null)
                    {
                        this.m_tmpSwappingList.Add(pair3.Value, itemByID.Value);
                    }
                }
                foreach (KeyValuePair<int, MyPhysicalInventoryItem> pair4 in this.m_tmpSwappingList)
                {
                    this.Inventory.ChangeItemClient(pair4.Value, pair4.Key);
                }
                this.m_tmpSwappingList.Clear();
            }
        }

        private static void ApplyChangesToClientItems(InventoryClientData clientData, ref InventoryDeltaInformation delta)
        {
            if (delta.RemovedItems != null)
            {
                foreach (uint num in delta.RemovedItems)
                {
                    int index = -1;
                    int num3 = 0;
                    while (true)
                    {
                        if (num3 < clientData.ClientItems.Count)
                        {
                            if (clientData.ClientItems[num3].Item.ItemId != num)
                            {
                                num3++;
                                continue;
                            }
                            index = num3;
                        }
                        if (index != -1)
                        {
                            clientData.ClientItems.RemoveAt(index);
                        }
                        break;
                    }
                }
            }
            if (delta.NewItems != null)
            {
                foreach (KeyValuePair<int, MyPhysicalInventoryItem> pair in delta.NewItems)
                {
                    ClientInvetoryData item = new ClientInvetoryData {
                        Item = pair.Value,
                        Amount = pair.Value.Amount
                    };
                    if (pair.Key >= clientData.ClientItems.Count)
                    {
                        clientData.ClientItems.Add(item);
                        continue;
                    }
                    clientData.ClientItems.Insert(pair.Key, item);
                }
            }
        }

        private void CalculateAddsAndRemovals(InventoryClientData clientData, out InventoryDeltaInformation delta, List<MyPhysicalInventoryItem> items)
        {
            InventoryDeltaInformation information = new InventoryDeltaInformation {
                HasChanges = false
            };
            delta = information;
            int num = 0;
            foreach (MyPhysicalInventoryItem item in items)
            {
                ClientInvetoryData data;
                if (!clientData.ClientItemsSorted.TryGetValue(item.ItemId, out data))
                {
                    if (delta.NewItems == null)
                    {
                        delta.NewItems = new SortedDictionary<int, MyPhysicalInventoryItem>();
                    }
                    delta.NewItems[num] = item;
                    delta.HasChanges = true;
                }
                else if ((data.Item.Content.TypeId == item.Content.TypeId) && (data.Item.Content.SubtypeId == item.Content.SubtypeId))
                {
                    this.m_foundDeltaItems.Add(item.ItemId);
                    MyFixedPoint amount = item.Amount;
                    MyObjectBuilder_GasContainerObject content = item.Content as MyObjectBuilder_GasContainerObject;
                    if (content != null)
                    {
                        amount = (MyFixedPoint) content.GasLevel;
                    }
                    if (data.Amount != amount)
                    {
                        MyFixedPoint point2 = amount - data.Amount;
                        if (delta.ChangedItems == null)
                        {
                            delta.ChangedItems = new Dictionary<uint, MyFixedPoint>();
                        }
                        delta.ChangedItems[item.ItemId] = point2;
                        delta.HasChanges = true;
                    }
                }
                num++;
            }
            foreach (KeyValuePair<uint, ClientInvetoryData> pair in clientData.ClientItemsSorted)
            {
                if (delta.RemovedItems == null)
                {
                    delta.RemovedItems = new List<uint>();
                }
                if (!this.m_foundDeltaItems.Contains(pair.Key))
                {
                    delta.RemovedItems.Add(pair.Key);
                    delta.HasChanges = true;
                }
            }
        }

        private InventoryDeltaInformation CalculateInventoryDiff(ref InventoryClientData clientData)
        {
            InventoryDeltaInformation information;
            if (this.m_itemsToSend == null)
            {
                this.m_itemsToSend = new List<MyPhysicalInventoryItem>();
            }
            if (this.m_foundDeltaItems == null)
            {
                this.m_foundDeltaItems = new HashSet<uint>();
            }
            this.m_foundDeltaItems.Clear();
            List<MyPhysicalInventoryItem> items = this.Inventory.GetItems();
            this.CalculateAddsAndRemovals(clientData, out information, items);
            if (information.HasChanges)
            {
                ApplyChangesToClientItems(clientData, ref information);
            }
            for (int i = 0; i < items.Count; i++)
            {
                if (i < clientData.ClientItems.Count)
                {
                    uint itemId = clientData.ClientItems[i].Item.ItemId;
                    if (itemId != items[i].ItemId)
                    {
                        if (information.SwappedItems == null)
                        {
                            information.SwappedItems = new Dictionary<uint, int>();
                        }
                        for (int j = 0; j < items.Count; j++)
                        {
                            if (itemId == items[j].ItemId)
                            {
                                information.SwappedItems[itemId] = j;
                            }
                        }
                    }
                }
            }
            clientData.ClientItemsSorted.Clear();
            clientData.ClientItems.Clear();
            foreach (MyPhysicalInventoryItem item in items)
            {
                MyFixedPoint amount = item.Amount;
                MyObjectBuilder_GasContainerObject content = item.Content as MyObjectBuilder_GasContainerObject;
                if (content != null)
                {
                    amount = (MyFixedPoint) content.GasLevel;
                }
                ClientInvetoryData data = new ClientInvetoryData {
                    Item = item,
                    Amount = amount
                };
                clientData.ClientItemsSorted[item.ItemId] = data;
                clientData.ClientItems.Add(data);
            }
            return information;
        }

        public void ClientUpdate(MyTimeSpan clientTimestamp)
        {
        }

        public void CreateClientData(MyClientStateBase forClient)
        {
            InventoryClientData data;
            if (this.m_clientInventoryUpdate == null)
            {
                this.m_clientInventoryUpdate = new Dictionary<Endpoint, InventoryClientData>();
            }
            if (!this.m_clientInventoryUpdate.TryGetValue(forClient.EndpointId, out data))
            {
                this.m_clientInventoryUpdate[forClient.EndpointId] = new InventoryClientData();
                data = this.m_clientInventoryUpdate[forClient.EndpointId];
            }
            data.Dirty = false;
            foreach (MyPhysicalInventoryItem item in this.Inventory.GetItems())
            {
                MyFixedPoint amount = item.Amount;
                MyObjectBuilder_GasContainerObject content = item.Content as MyObjectBuilder_GasContainerObject;
                if (content != null)
                {
                    amount = (MyFixedPoint) content.GasLevel;
                }
                ClientInvetoryData data2 = new ClientInvetoryData {
                    Item = item,
                    Amount = amount
                };
                data.ClientItemsSorted[item.ItemId] = data2;
                data.ClientItems.Add(data2);
            }
        }

        private InventoryDeltaInformation CreateSplit(ref InventoryDeltaInformation originalData, ref InventoryDeltaInformation sentData)
        {
            InventoryDeltaInformation information = new InventoryDeltaInformation {
                MessageId = sentData.MessageId
            };
            if (originalData.ChangedItems != null)
            {
                if (sentData.ChangedItems == null)
                {
                    information.ChangedItems = new Dictionary<uint, MyFixedPoint>();
                    foreach (KeyValuePair<uint, MyFixedPoint> pair in originalData.ChangedItems)
                    {
                        information.ChangedItems[pair.Key] = pair.Value;
                    }
                }
                else if (originalData.ChangedItems.Count != sentData.ChangedItems.Count)
                {
                    information.ChangedItems = new Dictionary<uint, MyFixedPoint>();
                    foreach (KeyValuePair<uint, MyFixedPoint> pair2 in originalData.ChangedItems)
                    {
                        if (!sentData.ChangedItems.ContainsKey(pair2.Key))
                        {
                            information.ChangedItems[pair2.Key] = pair2.Value;
                        }
                    }
                }
            }
            if (originalData.RemovedItems != null)
            {
                if (sentData.RemovedItems == null)
                {
                    information.RemovedItems = new List<uint>();
                    foreach (uint num in originalData.RemovedItems)
                    {
                        information.RemovedItems.Add(num);
                    }
                }
                else if (originalData.RemovedItems.Count != sentData.RemovedItems.Count)
                {
                    information.RemovedItems = new List<uint>();
                    foreach (uint num2 in originalData.RemovedItems)
                    {
                        if (!sentData.RemovedItems.Contains(num2))
                        {
                            information.RemovedItems.Add(num2);
                        }
                    }
                }
            }
            if (originalData.NewItems != null)
            {
                if (sentData.NewItems == null)
                {
                    information.NewItems = new SortedDictionary<int, MyPhysicalInventoryItem>();
                    foreach (KeyValuePair<int, MyPhysicalInventoryItem> pair3 in originalData.NewItems)
                    {
                        information.NewItems[pair3.Key] = pair3.Value;
                    }
                }
                else if (originalData.NewItems.Count != sentData.NewItems.Count)
                {
                    information.NewItems = new SortedDictionary<int, MyPhysicalInventoryItem>();
                    foreach (KeyValuePair<int, MyPhysicalInventoryItem> pair4 in originalData.NewItems)
                    {
                        if (!sentData.NewItems.ContainsKey(pair4.Key))
                        {
                            information.NewItems[pair4.Key] = pair4.Value;
                        }
                    }
                }
            }
            if (originalData.SwappedItems != null)
            {
                if (sentData.SwappedItems == null)
                {
                    information.SwappedItems = new Dictionary<uint, int>();
                    foreach (KeyValuePair<uint, int> pair5 in originalData.SwappedItems)
                    {
                        information.SwappedItems[pair5.Key] = pair5.Value;
                    }
                    return information;
                }
                if (originalData.SwappedItems.Count != sentData.SwappedItems.Count)
                {
                    information.SwappedItems = new Dictionary<uint, int>();
                    foreach (KeyValuePair<uint, int> pair6 in originalData.SwappedItems)
                    {
                        if (!sentData.SwappedItems.ContainsKey(pair6.Key))
                        {
                            information.SwappedItems[pair6.Key] = pair6.Value;
                        }
                    }
                }
            }
            return information;
        }

        public void Destroy()
        {
        }

        public void DestroyClientData(MyClientStateBase forClient)
        {
            if (this.m_clientInventoryUpdate != null)
            {
                this.m_clientInventoryUpdate.Remove(forClient.EndpointId);
            }
        }

        private void FlushBuffer()
        {
            while (true)
            {
                if (this.m_buffer.Count > 0)
                {
                    InventoryDeltaInformation changes = this.m_buffer.Values[0];
                    if (changes.MessageId == this.m_nextExpectedPacketId)
                    {
                        this.m_nextExpectedPacketId++;
                        this.ApplyChangesOnClient(changes);
                        this.m_buffer.RemoveAt(0);
                        continue;
                    }
                }
                return;
            }
        }

        public void ForceSend(MyClientStateBase clientData)
        {
        }

        public float GetGroupPriority(int frameCountWithoutSync, MyClientInfo client)
        {
            InventoryClientData data = this.m_clientInventoryUpdate[client.EndpointId];
            if (!data.Dirty && (data.FailedIncompletePackets.Count == 0))
            {
                return -1f;
            }
            if (data.FailedIncompletePackets.Count > 0)
            {
                return (1f * frameCountWithoutSync);
            }
            MyClientState state = (MyClientState) client.State;
            if (this.Inventory.Owner is MyCharacter)
            {
                MyCharacter owner = this.Inventory.Owner as MyCharacter;
                MyPlayer playerFromCharacter = MyPlayer.GetPlayerFromCharacter(owner);
                if ((playerFromCharacter == null) && (owner.IsUsing != null))
                {
                    MyShipController isUsing = owner.IsUsing as MyShipController;
                    if ((isUsing != null) && (isUsing.ControllerInfo.Controller != null))
                    {
                        playerFromCharacter = isUsing.ControllerInfo.Controller.Player;
                    }
                }
                if ((playerFromCharacter != null) && (playerFromCharacter.Id.SteamId == client.EndpointId.Id.Value))
                {
                    return (1f * frameCountWithoutSync);
                }
            }
            if ((state.ContextEntity is MyCharacter) && ReferenceEquals(state.ContextEntity, this.Inventory.Owner))
            {
                return (1f * frameCountWithoutSync);
            }
            if (((state.Context == MyClientState.MyContextKind.Inventory) || (state.Context == MyClientState.MyContextKind.Building)) || ((state.Context == MyClientState.MyContextKind.Production) && (this.Inventory.Owner is MyAssembler)))
            {
                return (this.GetPriorityStateGroup(client) * frameCountWithoutSync);
            }
            return 0f;
        }

        private float GetPriorityStateGroup(MyClientInfo client)
        {
            MyClientState state = (MyClientState) client.State;
            if (this.Inventory.ForcedPriority != null)
            {
                return this.Inventory.ForcedPriority.Value;
            }
            if (state.ContextEntity != null)
            {
                if (ReferenceEquals(state.ContextEntity, this.Inventory.Owner))
                {
                    return 1f;
                }
                MyCubeGrid topMostParent = state.ContextEntity.GetTopMostParent(null) as MyCubeGrid;
                if (topMostParent != null)
                {
                    using (HashSet<MyTerminalBlock>.Enumerator enumerator = topMostParent.GridSystems.TerminalSystem.Blocks.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            MyTerminalBlock current = enumerator.Current;
                            if (ReferenceEquals(current, this.Inventory.Container.Entity) && ((state.Context != MyClientState.MyContextKind.Production) || (current is MyAssembler)))
                            {
                                return 1f;
                            }
                        }
                    }
                }
            }
            return 0f;
        }

        private void InventoryChanged(MyInventoryBase obj)
        {
            if (this.m_clientInventoryUpdate != null)
            {
                foreach (KeyValuePair<Endpoint, InventoryClientData> pair in this.m_clientInventoryUpdate)
                {
                    this.m_clientInventoryUpdate[pair.Key].Dirty = true;
                }
                MyMultiplayer.GetReplicationServer().AddToDirtyGroups(this);
            }
        }

        public MyStreamProcessingState IsProcessingForClient(Endpoint forClient) => 
            MyStreamProcessingState.None;

        public bool IsStillDirty(Endpoint forClient)
        {
            InventoryClientData data;
            return (!this.m_clientInventoryUpdate.TryGetValue(forClient, out data) || (data.Dirty || (data.FailedIncompletePackets.Count != 0)));
        }

        public void OnAck(MyClientStateBase forClient, byte packetId, bool delivered)
        {
            InventoryClientData data;
            InventoryDeltaInformation information;
            if (this.m_clientInventoryUpdate.TryGetValue(forClient.EndpointId, out data) && data.SendPackets.TryGetValue(packetId, out information))
            {
                if (!delivered)
                {
                    data.FailedIncompletePackets.Add(information);
                    MyMultiplayer.GetReplicationServer().AddToDirtyGroups(this);
                }
                data.SendPackets.Remove(packetId);
            }
        }

        private InventoryDeltaInformation PrepareSendData(ref InventoryDeltaInformation packetInfo, BitStream stream, int maxBitPosition, out bool needsSplit)
        {
            needsSplit = false;
            int bitPosition = stream.BitPosition;
            InventoryDeltaInformation information = new InventoryDeltaInformation {
                HasChanges = false
            };
            stream.WriteBool(false);
            stream.WriteUInt32(packetInfo.MessageId, 0x20);
            stream.WriteBool(packetInfo.ChangedItems != null);
            if (packetInfo.ChangedItems != null)
            {
                stream.WriteInt32(packetInfo.ChangedItems.Count, 0x20);
                if (stream.BitPosition > maxBitPosition)
                {
                    needsSplit = true;
                }
                else
                {
                    information.ChangedItems = new Dictionary<uint, MyFixedPoint>();
                    foreach (KeyValuePair<uint, MyFixedPoint> pair in packetInfo.ChangedItems)
                    {
                        stream.WriteUInt32(pair.Key, 0x20);
                        stream.WriteInt64(pair.Value.RawValue, 0x40);
                        if (stream.BitPosition > maxBitPosition)
                        {
                            needsSplit = true;
                            continue;
                        }
                        information.ChangedItems[pair.Key] = pair.Value;
                        information.HasChanges = true;
                    }
                }
            }
            stream.WriteBool(packetInfo.RemovedItems != null);
            if (packetInfo.RemovedItems != null)
            {
                stream.WriteInt32(packetInfo.RemovedItems.Count, 0x20);
                if (stream.BitPosition > maxBitPosition)
                {
                    needsSplit = true;
                }
                else
                {
                    information.RemovedItems = new List<uint>();
                    foreach (uint num2 in packetInfo.RemovedItems)
                    {
                        stream.WriteUInt32(num2, 0x20);
                        if (stream.BitPosition > maxBitPosition)
                        {
                            needsSplit = true;
                            continue;
                        }
                        information.RemovedItems.Add(num2);
                        information.HasChanges = true;
                    }
                }
            }
            stream.WriteBool(packetInfo.NewItems != null);
            if (packetInfo.NewItems != null)
            {
                stream.WriteInt32(packetInfo.NewItems.Count, 0x20);
                if (stream.BitPosition > maxBitPosition)
                {
                    needsSplit = true;
                }
                else
                {
                    information.NewItems = new SortedDictionary<int, MyPhysicalInventoryItem>();
                    foreach (KeyValuePair<int, MyPhysicalInventoryItem> pair2 in packetInfo.NewItems)
                    {
                        MyPhysicalInventoryItem item = pair2.Value;
                        stream.WriteInt32(pair2.Key, 0x20);
                        MySerializer.Write<MyPhysicalInventoryItem>(stream, ref item, MyObjectBuilderSerializer.Dynamic);
                        if (stream.BitPosition > maxBitPosition)
                        {
                            needsSplit = true;
                            continue;
                        }
                        information.NewItems[pair2.Key] = item;
                        information.HasChanges = true;
                    }
                }
            }
            stream.WriteBool(packetInfo.SwappedItems != null);
            if (packetInfo.SwappedItems != null)
            {
                stream.WriteInt32(packetInfo.SwappedItems.Count, 0x20);
                if (stream.BitPosition > maxBitPosition)
                {
                    needsSplit = true;
                }
                else
                {
                    information.SwappedItems = new Dictionary<uint, int>();
                    foreach (KeyValuePair<uint, int> pair3 in packetInfo.SwappedItems)
                    {
                        stream.WriteUInt32(pair3.Key, 0x20);
                        stream.WriteInt32(pair3.Value, 0x20);
                        if (stream.BitPosition > maxBitPosition)
                        {
                            needsSplit = true;
                            continue;
                        }
                        information.SwappedItems[pair3.Key] = pair3.Value;
                        information.HasChanges = true;
                    }
                }
            }
            stream.SetBitPositionWrite(bitPosition);
            return information;
        }

        private void ReadInventory(BitStream stream)
        {
            if (stream.ReadBool())
            {
                uint key = stream.ReadUInt32(0x20);
                bool flag = true;
                bool flag2 = false;
                InventoryDeltaInformation information = new InventoryDeltaInformation();
                if (key == this.m_nextExpectedPacketId)
                {
                    this.m_nextExpectedPacketId++;
                }
                else if ((key <= this.m_nextExpectedPacketId) || this.m_buffer.ContainsKey(key))
                {
                    flag = false;
                }
                else
                {
                    flag2 = true;
                    information.MessageId = key;
                }
                if (stream.ReadBool())
                {
                    int num2 = stream.ReadInt32(0x20);
                    for (int i = 0; i < num2; i++)
                    {
                        uint itemId = stream.ReadUInt32(0x20);
                        MyFixedPoint amount = new MyFixedPoint {
                            RawValue = stream.ReadInt64(0x40)
                        };
                        if (flag)
                        {
                            if (!flag2)
                            {
                                this.Inventory.UpdateItemAmoutClient(itemId, amount);
                            }
                            else
                            {
                                if (information.ChangedItems == null)
                                {
                                    information.ChangedItems = new Dictionary<uint, MyFixedPoint>();
                                }
                                information.ChangedItems.Add(itemId, amount);
                            }
                        }
                    }
                }
                if (stream.ReadBool())
                {
                    int num5 = stream.ReadInt32(0x20);
                    for (int i = 0; i < num5; i++)
                    {
                        uint itemId = stream.ReadUInt32(0x20);
                        if (flag)
                        {
                            if (!flag2)
                            {
                                this.Inventory.RemoveItemClient(itemId);
                            }
                            else
                            {
                                if (information.RemovedItems == null)
                                {
                                    information.RemovedItems = new List<uint>();
                                }
                                information.RemovedItems.Add(itemId);
                            }
                        }
                    }
                }
                if (stream.ReadBool())
                {
                    int num8 = stream.ReadInt32(0x20);
                    for (int i = 0; i < num8; i++)
                    {
                        MyPhysicalInventoryItem item;
                        int position = stream.ReadInt32(0x20);
                        MySerializer.CreateAndRead<MyPhysicalInventoryItem>(stream, out item, MyObjectBuilderSerializer.Dynamic);
                        if (flag)
                        {
                            if (!flag2)
                            {
                                this.Inventory.AddItemClient(position, item);
                            }
                            else
                            {
                                if (information.NewItems == null)
                                {
                                    information.NewItems = new SortedDictionary<int, MyPhysicalInventoryItem>();
                                }
                                information.NewItems.Add(position, item);
                            }
                        }
                    }
                }
                if (stream.ReadBool())
                {
                    if (this.m_tmpSwappingList == null)
                    {
                        this.m_tmpSwappingList = new Dictionary<int, MyPhysicalInventoryItem>();
                    }
                    int num11 = stream.ReadInt32(0x20);
                    int num12 = 0;
                    while (true)
                    {
                        if (num12 >= num11)
                        {
                            foreach (KeyValuePair<int, MyPhysicalInventoryItem> pair in this.m_tmpSwappingList)
                            {
                                this.Inventory.ChangeItemClient(pair.Value, pair.Key);
                            }
                            this.m_tmpSwappingList.Clear();
                            break;
                        }
                        uint num13 = stream.ReadUInt32(0x20);
                        int num14 = stream.ReadInt32(0x20);
                        if (flag)
                        {
                            if (flag2)
                            {
                                if (information.SwappedItems == null)
                                {
                                    information.SwappedItems = new Dictionary<uint, int>();
                                }
                                information.SwappedItems.Add(num13, num14);
                            }
                            else
                            {
                                MyPhysicalInventoryItem? itemByID = this.Inventory.GetItemByID(num13);
                                if (itemByID != null)
                                {
                                    this.m_tmpSwappingList.Add(num14, itemByID.Value);
                                }
                            }
                        }
                        num12++;
                    }
                }
                if (flag2)
                {
                    this.m_buffer.Add(key, information);
                }
                else if (flag)
                {
                    this.FlushBuffer();
                }
                this.Inventory.Refresh();
            }
        }

        public void Reset(bool reinit, MyTimeSpan clientTimestamp)
        {
        }

        public void Serialize(BitStream stream, Endpoint forClient, MyTimeSpan serverTimestamp, MyTimeSpan lastClientTimestamp, byte packetId, int maxBitPosition, HashSet<string> cachedData)
        {
            if (!stream.Writing)
            {
                this.ReadInventory(stream);
            }
            else
            {
                InventoryClientData clientData = this.m_clientInventoryUpdate[forClient];
                bool needsSplit = false;
                if (clientData.FailedIncompletePackets.Count <= 0)
                {
                    InventoryDeltaInformation packetInfo = this.CalculateInventoryDiff(ref clientData);
                    packetInfo.MessageId = clientData.CurrentMessageId;
                    clientData.MainSendingInfo = this.WriteInventory(ref packetInfo, stream, packetId, maxBitPosition, out needsSplit);
                    if (clientData.MainSendingInfo.HasChanges)
                    {
                        clientData.SendPackets[packetId] = clientData.MainSendingInfo;
                        clientData.CurrentMessageId++;
                    }
                    if (needsSplit)
                    {
                        InventoryDeltaInformation item = this.CreateSplit(ref packetInfo, ref clientData.MainSendingInfo);
                        item.MessageId = clientData.CurrentMessageId;
                        clientData.FailedIncompletePackets.Add(item);
                        clientData.CurrentMessageId++;
                    }
                    clientData.Dirty = false;
                }
                else
                {
                    InventoryDeltaInformation packetInfo = clientData.FailedIncompletePackets[0];
                    clientData.FailedIncompletePackets.RemoveAtFast<InventoryDeltaInformation>(0);
                    InventoryDeltaInformation sentData = this.WriteInventory(ref packetInfo, stream, packetId, maxBitPosition, out needsSplit);
                    sentData.MessageId = packetInfo.MessageId;
                    if (needsSplit)
                    {
                        clientData.FailedIncompletePackets.Add(this.CreateSplit(ref packetInfo, ref sentData));
                    }
                    if (sentData.HasChanges)
                    {
                        clientData.SendPackets[packetId] = sentData;
                    }
                }
            }
        }

        private InventoryDeltaInformation WriteInventory(ref InventoryDeltaInformation packetInfo, BitStream stream, byte packetId, int maxBitPosition, out bool needsSplit)
        {
            InventoryDeltaInformation information = this.PrepareSendData(ref packetInfo, stream, maxBitPosition, out needsSplit);
            if (!information.HasChanges)
            {
                stream.WriteBool(false);
                return information;
            }
            information.MessageId = packetInfo.MessageId;
            stream.WriteBool(true);
            stream.WriteUInt32(information.MessageId, 0x20);
            stream.WriteBool(information.ChangedItems != null);
            if (information.ChangedItems != null)
            {
                stream.WriteInt32(information.ChangedItems.Count, 0x20);
                foreach (KeyValuePair<uint, MyFixedPoint> pair in information.ChangedItems)
                {
                    stream.WriteUInt32(pair.Key, 0x20);
                    stream.WriteInt64(pair.Value.RawValue, 0x40);
                }
            }
            stream.WriteBool(information.RemovedItems != null);
            if (information.RemovedItems != null)
            {
                stream.WriteInt32(information.RemovedItems.Count, 0x20);
                foreach (uint num in information.RemovedItems)
                {
                    stream.WriteUInt32(num, 0x20);
                }
            }
            stream.WriteBool(information.NewItems != null);
            if (information.NewItems != null)
            {
                stream.WriteInt32(information.NewItems.Count, 0x20);
                foreach (KeyValuePair<int, MyPhysicalInventoryItem> pair2 in information.NewItems)
                {
                    stream.WriteInt32(pair2.Key, 0x20);
                    MyPhysicalInventoryItem item = pair2.Value;
                    MySerializer.Write<MyPhysicalInventoryItem>(stream, ref item, MyObjectBuilderSerializer.Dynamic);
                }
            }
            stream.WriteBool(information.SwappedItems != null);
            if (information.SwappedItems != null)
            {
                stream.WriteInt32(information.SwappedItems.Count, 0x20);
                foreach (KeyValuePair<uint, int> pair3 in information.SwappedItems)
                {
                    stream.WriteUInt32(pair3.Key, 0x20);
                    stream.WriteInt32(pair3.Value, 0x20);
                }
            }
            return information;
        }

        public bool IsHighPriority =>
            false;

        private MyInventory Inventory { get; set; }

        public IMyReplicable Owner { get; private set; }

        public bool IsValid =>
            ((this.Owner != null) && this.Owner.IsValid);

        public bool IsStreaming =>
            false;

        public bool NeedsUpdate =>
            false;

        [StructLayout(LayoutKind.Sequential)]
        private struct ClientInvetoryData
        {
            public MyPhysicalInventoryItem Item;
            public MyFixedPoint Amount;
        }

        private class InventoryClientData
        {
            public uint CurrentMessageId;
            public MyEntityInventoryStateGroup.InventoryDeltaInformation MainSendingInfo;
            public bool Dirty;
            public readonly Dictionary<byte, MyEntityInventoryStateGroup.InventoryDeltaInformation> SendPackets = new Dictionary<byte, MyEntityInventoryStateGroup.InventoryDeltaInformation>();
            public readonly List<MyEntityInventoryStateGroup.InventoryDeltaInformation> FailedIncompletePackets = new List<MyEntityInventoryStateGroup.InventoryDeltaInformation>();
            public readonly SortedDictionary<uint, MyEntityInventoryStateGroup.ClientInvetoryData> ClientItemsSorted = new SortedDictionary<uint, MyEntityInventoryStateGroup.ClientInvetoryData>();
            public readonly List<MyEntityInventoryStateGroup.ClientInvetoryData> ClientItems = new List<MyEntityInventoryStateGroup.ClientInvetoryData>();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct InventoryDeltaInformation
        {
            public bool HasChanges;
            public uint MessageId;
            public List<uint> RemovedItems;
            public Dictionary<uint, MyFixedPoint> ChangedItems;
            public SortedDictionary<int, MyPhysicalInventoryItem> NewItems;
            public Dictionary<uint, int> SwappedItems;
        }
    }
}

