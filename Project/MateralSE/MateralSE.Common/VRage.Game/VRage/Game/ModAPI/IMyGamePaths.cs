namespace VRage.Game.ModAPI
{
    using System;

    public interface IMyGamePaths
    {
        string ContentPath { get; }

        string ModsPath { get; }

        string UserDataPath { get; }

        string SavesPath { get; }

        string ModScopeName { get; }
    }
}

