namespace Sandbox.Game.AI.Pathfinding
{
    using System;

    public interface IMyHighLevelComponent
    {
        bool Contains(MyNavigationPrimitive primitive);

        bool FullyExplored { get; }
    }
}

