namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Game.AI;
    using System;

    public static class MyCestmirPathfindingShorts
    {
        public static MyPathfinding Pathfinding =>
            (MyAIComponent.Static.Pathfinding as MyPathfinding);
    }
}

