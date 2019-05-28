namespace VRage.Game.ModAPI
{
    using System;

    public interface IMyModContext
    {
        string ModName { get; }

        string ModId { get; }

        string ModPath { get; }

        string ModPathData { get; }

        bool IsBaseGame { get; }
    }
}

