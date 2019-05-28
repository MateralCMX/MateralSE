namespace Sandbox.Game.GameSystems.Conveyors
{
    using System;

    public interface IMyConveyorSegmentBlock
    {
        void InitializeConveyorSegment();

        MyConveyorSegment ConveyorSegment { get; }
    }
}

