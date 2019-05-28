namespace Sandbox.Game.Entities
{
    using Sandbox.Game;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;

    public interface IMyGunBaseUser
    {
        MyEntity[] IgnoreEntities { get; }

        MyEntity Weapon { get; }

        MyEntity Owner { get; }

        IMyMissileGunObject Launcher { get; }

        MyInventory AmmoInventory { get; }

        MyDefinitionId PhysicalItemId { get; }

        MyInventory WeaponInventory { get; }

        long OwnerId { get; }

        string ConstraintDisplayName { get; }
    }
}

