namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.GameSystems.Conveyors;
    using System;

    internal interface IMyGasProducer : IMyGasBlock, IMyConveyorEndpointBlock
    {
        int GetPriority();
        void Produce(float amount);
        float ProductionCapacity(float deltaTime);
    }
}

