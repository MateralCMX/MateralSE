namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.GameSystems.Conveyors;
    using System;

    public interface IMyGasConsumer : IMyGasBlock, IMyConveyorEndpointBlock
    {
        void Consume(float amount);
        float ConsumptionNeed(float deltaTime);
        int GetPriority();
    }
}

