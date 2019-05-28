namespace Sandbox.Game.Entities.Character
{
    using System;

    internal interface IMyNetworkCommand
    {
        void Apply();

        bool ExecuteBeforeMoveAndRotate { get; }
    }
}

