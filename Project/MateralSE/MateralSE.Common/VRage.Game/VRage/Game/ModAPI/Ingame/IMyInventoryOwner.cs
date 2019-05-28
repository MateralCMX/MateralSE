namespace VRage.Game.ModAPI.Ingame
{
    using System;

    [Obsolete("IMyInventoryOwner interface and MyInventoryOwnerTypeEnum enum is obsolete. Use type checking and inventory methods on MyEntity.")]
    public interface IMyInventoryOwner
    {
        IMyInventory GetInventory(int index);

        int InventoryCount { get; }

        long EntityId { get; }

        bool UseConveyorSystem { get; set; }

        bool HasInventory { get; }
    }
}

