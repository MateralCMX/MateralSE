namespace Sandbox.Game.Components
{
    using Sandbox.Game.GameSystems.Conveyors;
    using System;
    using System.Runtime.CompilerServices;

    public class MyDebugRenderComponentDrawConveyorEndpoint : MyDebugRenderComponent
    {
        public MyDebugRenderComponentDrawConveyorEndpoint(IMyConveyorEndpoint endpoint) : base(null)
        {
            this.ConveyorEndpoint = endpoint;
        }

        public override void DebugDraw()
        {
            this.ConveyorEndpoint.DebugDraw();
        }

        private IMyConveyorEndpoint ConveyorEndpoint { get; set; }
    }
}

