namespace Sandbox.ModAPI.Ingame
{
    using System;
    using VRage.Game.ModAPI.Ingame;

    public interface IMyAssembler : IMyProductionBlock, IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        [Obsolete("Use the Mode property")]
        bool DisassembleEnabled { get; }

        float CurrentProgress { get; }

        MyAssemblerMode Mode { get; set; }

        bool CooperativeMode { get; set; }

        bool Repeating { get; set; }
    }
}

