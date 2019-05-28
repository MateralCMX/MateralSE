namespace VRage.GameServices
{
    using System;
    using System.Runtime.CompilerServices;

    public class MyGameInventoryItem
    {
        public ulong ID { get; set; }

        public MyGameInventoryItemDefinition ItemDefinition { get; set; }

        public ushort Quantity { get; set; }

        public bool IsInUse { get; set; }

        public bool IsStoreFakeItem { get; set; }

        public bool IsNew { get; set; }
    }
}

