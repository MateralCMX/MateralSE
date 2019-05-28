namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.ModAPI.Ingame;

    public interface IMyRadioAntenna : IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        [Obsolete("Use IMyIntergridCommunicationSystem instead")]
        bool TransmitMessage(string message, MyTransmitTarget target = 3);

        float Radius { get; set; }

        bool ShowShipName { get; set; }

        bool IsBroadcasting { get; }

        bool EnableBroadcasting { get; set; }

        long AttachedProgrammableBlock { get; set; }

        bool IgnoreAlliedBroadcast { get; set; }

        bool IgnoreOtherBroadcast { get; set; }

        string HudText { get; set; }
    }
}

