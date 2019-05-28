namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.GameSystems.Conveyors;
    using System;

    public interface IMyGasBlock : IMyConveyorEndpointBlock
    {
        bool IsWorking();

        bool CanPressurizeRoom { get; }
    }
}

