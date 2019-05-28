namespace Sandbox.Game.Entities
{
    using Sandbox.Game.GameSystems.Conveyors;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class MyUpgradableBlockComponent
    {
        public MyUpgradableBlockComponent(MyCubeBlock parent)
        {
            this.ConnectionPositions = new HashSet<ConveyorLinePosition>();
            this.Refresh(parent);
        }

        public void Refresh(MyCubeBlock parent)
        {
            if (parent.BlockDefinition.Model != null)
            {
                this.ConnectionPositions.Clear();
                foreach (ConveyorLinePosition position in MyMultilineConveyorEndpoint.GetLinePositions(parent, "detector_upgrade"))
                {
                    this.ConnectionPositions.Add(MyMultilineConveyorEndpoint.PositionToGridCoords(position, parent));
                }
            }
        }

        public HashSet<ConveyorLinePosition> ConnectionPositions { get; private set; }
    }
}

