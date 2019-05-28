namespace VRage.Game.ModAPI.Ingame
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;

    public interface IMyInventory
    {
        bool CanItemsBeAdded(MyFixedPoint amount, MyItemType itemType);
        bool CanTransferItemTo(IMyInventory otherInventory, MyItemType itemType);
        bool ContainItems(MyFixedPoint amount, MyItemType itemType);
        MyInventoryItem? FindItem(MyItemType itemType);
        void GetAcceptedItems(List<MyItemType> itemsTypes, Func<MyItemType, bool> filter = null);
        MyFixedPoint GetItemAmount(MyItemType itemType);
        MyInventoryItem? GetItemAt(int index);
        MyInventoryItem? GetItemByID(uint id);
        void GetItems(List<MyInventoryItem> items, Func<MyInventoryItem, bool> filter = null);
        bool IsConnectedTo(IMyInventory otherInventory);
        bool IsItemAt(int position);
        bool TransferItemFrom(IMyInventory sourceInventory, MyInventoryItem item, MyFixedPoint? amount = new MyFixedPoint?());
        bool TransferItemFrom(IMyInventory sourceInventory, int sourceItemIndex, int? targetItemIndex = new int?(), bool? stackIfPossible = new bool?(), MyFixedPoint? amount = new MyFixedPoint?());
        bool TransferItemTo(IMyInventory dstInventory, MyInventoryItem item, MyFixedPoint? amount = new MyFixedPoint?());
        bool TransferItemTo(IMyInventory dst, int sourceItemIndex, int? targetItemIndex = new int?(), bool? stackIfPossible = new bool?(), MyFixedPoint? amount = new MyFixedPoint?());

        IMyEntity Owner { get; }

        bool IsFull { get; }

        MyFixedPoint CurrentMass { get; }

        MyFixedPoint MaxVolume { get; }

        MyFixedPoint CurrentVolume { get; }

        int ItemCount { get; }
    }
}

