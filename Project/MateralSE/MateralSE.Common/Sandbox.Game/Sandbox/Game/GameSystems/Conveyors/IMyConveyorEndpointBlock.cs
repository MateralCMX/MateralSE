namespace Sandbox.Game.GameSystems.Conveyors
{
    using System;

    public interface IMyConveyorEndpointBlock
    {
        bool AllowSelfPulling();
        PullInformation GetPullInformation();
        PullInformation GetPushInformation();
        void InitializeConveyorEndpoint();

        IMyConveyorEndpoint ConveyorEndpoint { get; }
    }
}

