namespace VRage.Game.ModAPI.Ingame
{
    using System;
    using System.Collections.Generic;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    public interface IMySlimBlock
    {
        void GetMissingComponents(Dictionary<string, int> addToDictionary);

        SerializableDefinitionId BlockDefinition { get; }

        float AccumulatedDamage { get; }

        float BuildIntegrity { get; }

        float BuildLevelRatio { get; }

        float CurrentDamage { get; }

        float DamageRatio { get; }

        IMyCubeBlock FatBlock { get; }

        bool HasDeformation { get; }

        bool IsDestroyed { get; }

        bool IsFullIntegrity { get; }

        bool IsFullyDismounted { get; }

        float MaxDeformation { get; }

        float MaxIntegrity { get; }

        float Mass { get; }

        long OwnerId { get; }

        bool ShowParts { get; }

        bool StockpileAllocated { get; }

        bool StockpileEmpty { get; }

        Vector3I Position { get; }

        IMyCubeGrid CubeGrid { get; }

        Vector3 ColorMaskHSV { get; }

        MyStringHash SkinSubtypeId { get; }
    }
}

