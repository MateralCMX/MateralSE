namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Weapons;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;

    public static class MyEntityInventoryOwnerExtensions
    {
        [Obsolete("IMyInventoryOwner interface and MyInventoryOwnerTypeEnum enum is obsolete. Use type checking and inventory methods on MyEntity or MyInventory. Inventories will have this attribute as member.")]
        public static MyInventoryOwnerTypeEnum InventoryOwnerType(this VRage.Game.Entity.MyEntity entity) => 
            (!IsSameOrSubclass(typeof(MyUserControllableGun), entity.GetType()) ? (!IsSameOrSubclass(typeof(MyProductionBlock), entity.GetType()) ? (!IsSameOrSubclass(typeof(MyConveyorSorter), entity.GetType()) ? (!IsSameOrSubclass(typeof(MyGasGenerator), entity.GetType()) ? (!IsSameOrSubclass(typeof(MyShipToolBase), entity.GetType()) ? (!IsSameOrSubclass(typeof(MyGasTank), entity.GetType()) ? (!IsSameOrSubclass(typeof(MyReactor), entity.GetType()) ? (!IsSameOrSubclass(typeof(MyCollector), entity.GetType()) ? (!IsSameOrSubclass(typeof(MyCargoContainer), entity.GetType()) ? (!IsSameOrSubclass(typeof(MyShipDrill), entity.GetType()) ? (!IsSameOrSubclass(typeof(MyCharacter), entity.GetType()) ? MyInventoryOwnerTypeEnum.Storage : MyInventoryOwnerTypeEnum.Character) : MyInventoryOwnerTypeEnum.System) : MyInventoryOwnerTypeEnum.Storage) : MyInventoryOwnerTypeEnum.Storage) : MyInventoryOwnerTypeEnum.Energy) : MyInventoryOwnerTypeEnum.System) : MyInventoryOwnerTypeEnum.System) : MyInventoryOwnerTypeEnum.System) : MyInventoryOwnerTypeEnum.Storage) : MyInventoryOwnerTypeEnum.System) : MyInventoryOwnerTypeEnum.System);

        private static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant) => 
            (potentialDescendant.IsSubclassOf(potentialBase) || (potentialDescendant == potentialBase));
    }
}

