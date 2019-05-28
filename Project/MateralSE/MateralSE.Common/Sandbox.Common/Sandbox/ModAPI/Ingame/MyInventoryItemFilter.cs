namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ModAPI.Ingame;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct MyInventoryItemFilter
    {
        public readonly bool AllSubTypes;
        public readonly MyDefinitionId ItemId;
        public static implicit operator MyInventoryItemFilter(MyItemType itemType) => 
            new MyInventoryItemFilter((MyDefinitionId) itemType, false);

        public static implicit operator MyInventoryItemFilter(MyDefinitionId definitionId) => 
            new MyInventoryItemFilter(definitionId, false);

        public MyItemType ItemType =>
            this.ItemId;
        public MyInventoryItemFilter(string itemId, bool allSubTypes = false)
        {
            this = new MyInventoryItemFilter();
            this.ItemId = MyDefinitionId.Parse(itemId);
            this.AllSubTypes = allSubTypes;
        }

        public MyInventoryItemFilter(MyDefinitionId itemId, bool allSubTypes = false)
        {
            this = new MyInventoryItemFilter();
            this.ItemId = itemId;
            this.AllSubTypes = allSubTypes;
        }
    }
}

