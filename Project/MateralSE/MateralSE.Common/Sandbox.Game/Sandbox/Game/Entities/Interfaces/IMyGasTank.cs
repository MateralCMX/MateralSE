namespace Sandbox.Game.Entities.Interfaces
{
    using System;
    using VRage.Game;

    public interface IMyGasTank
    {
        bool IsResourceStorage(MyDefinitionId resourceDefinition);

        float GasCapacity { get; }

        double FilledRatio { get; }
    }
}

