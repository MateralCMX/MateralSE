namespace Sandbox.ModAPI.Ingame
{
    using System;
    using VRage.Game.ModAPI.Ingame;

    public interface IMyProgrammableBlock : IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity, IMyTextSurfaceProvider
    {
        bool TryRun(string argument);

        bool IsRunning { get; }

        string TerminalRunArgument { get; }
    }
}

