namespace Sandbox.ModAPI.Interfaces.Terminal
{
    using Sandbox.ModAPI.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IMyTerminalAction : ITerminalAction
    {
        Func<IMyTerminalBlock, bool> Enabled { get; set; }

        List<MyToolbarType> InvalidToolbarTypes { get; set; }

        bool ValidForGroups { get; set; }

        StringBuilder Name { get; set; }

        string Icon { get; set; }

        Action<IMyTerminalBlock> Action { get; set; }

        Action<IMyTerminalBlock, StringBuilder> Writer { get; set; }
    }
}

