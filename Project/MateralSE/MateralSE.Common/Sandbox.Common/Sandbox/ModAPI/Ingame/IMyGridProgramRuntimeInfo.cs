namespace Sandbox.ModAPI.Ingame
{
    using System;

    public interface IMyGridProgramRuntimeInfo
    {
        TimeSpan TimeSinceLastRun { get; }

        double LastRunTimeMs { get; }

        int MaxInstructionCount { get; }

        int CurrentInstructionCount { get; }

        int MaxCallChainDepth { get; }

        int CurrentCallChainDepth { get; }

        Sandbox.ModAPI.Ingame.UpdateFrequency UpdateFrequency { get; set; }
    }
}

