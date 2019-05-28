namespace Sandbox.Game.GameSystems.Conveyors
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using VRage.Algorithms;

    public interface IMyConveyorEndpoint : IMyPathVertex<IMyConveyorEndpoint>, IEnumerable<IMyPathEdge<IMyConveyorEndpoint>>, IEnumerable
    {
        void DebugDraw();
        MyConveyorLine GetConveyorLine(ConveyorLinePosition position);
        MyConveyorLine GetConveyorLine(int index);
        int GetLineCount();
        ConveyorLinePosition GetPosition(int index);
        void SetConveyorLine(ConveyorLinePosition position, MyConveyorLine newLine);

        MyCubeBlock CubeBlock { get; }
    }
}

