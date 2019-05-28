namespace Sandbox.Game.GameSystems.Conveyors
{
    using Sandbox.Game;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    public class PullInformation
    {
        public MyInventory Inventory { get; set; }

        public long OwnerID { get; set; }

        public MyInventoryConstraint Constraint { get; set; }

        public MyDefinitionId ItemDefinition { get; set; }
    }
}

