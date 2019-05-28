namespace Sandbox.ModAPI
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    public interface IMyProgrammableBlock : Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyProgrammableBlock, Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider
    {
        void Recompile();
        void Run();
        void Run(string argument);
        void Run(string argument, UpdateType updateSource);
        bool TryRun(string argument);

        string ProgramData { get; set; }

        string StorageData { get; set; }

        bool HasCompileErrors { get; }
    }
}

