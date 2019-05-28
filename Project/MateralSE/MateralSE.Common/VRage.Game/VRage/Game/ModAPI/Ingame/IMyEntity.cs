namespace VRage.Game.ModAPI.Ingame
{
    using System;
    using VRage.Game.Components;
    using VRageMath;

    public interface IMyEntity
    {
        IMyInventory GetInventory();
        IMyInventory GetInventory(int index);
        Vector3D GetPosition();

        MyEntityComponentContainer Components { get; }

        long EntityId { get; }

        string Name { get; }

        string DisplayName { get; }

        bool HasInventory { get; }

        int InventoryCount { get; }

        BoundingBoxD WorldAABB { get; }

        BoundingBoxD WorldAABBHr { get; }

        MatrixD WorldMatrix { get; }

        BoundingSphereD WorldVolume { get; }

        BoundingSphereD WorldVolumeHr { get; }
    }
}

