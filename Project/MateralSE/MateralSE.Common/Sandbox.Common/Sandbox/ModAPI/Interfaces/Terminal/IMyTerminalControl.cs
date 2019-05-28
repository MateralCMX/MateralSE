namespace Sandbox.ModAPI.Interfaces.Terminal
{
    using System;

    public interface IMyTerminalControl
    {
        void RedrawControl();
        void UpdateVisual();

        string Id { get; }

        Func<IMyTerminalBlock, bool> Enabled { get; set; }

        Func<IMyTerminalBlock, bool> Visible { get; set; }

        bool SupportsMultipleBlocks { get; set; }
    }
}

