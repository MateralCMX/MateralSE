namespace Sandbox.Game.Components
{
    using Sandbox.Game.GameSystems.Conveyors;
    using System;

    internal class MyDebugRenderComponentDrawConveyorSegment : MyDebugRenderComponent
    {
        private MyConveyorSegment m_conveyorSegment;

        public MyDebugRenderComponentDrawConveyorSegment(MyConveyorSegment conveyorSegment) : base(null)
        {
            this.m_conveyorSegment = conveyorSegment;
        }

        public override void DebugDraw()
        {
            this.m_conveyorSegment.DebugDraw();
        }
    }
}

