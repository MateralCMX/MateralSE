namespace Sandbox.ModAPI.Interfaces.Terminal
{
    using System;

    public interface IMyTerminalControlListbox : IMyTerminalControl, IMyTerminalControlTitleTooltip
    {
        bool Multiselect { get; set; }

        int VisibleRowsCount { get; set; }

        Action<IMyTerminalBlock, List<MyTerminalControlListBoxItem>, List<MyTerminalControlListBoxItem>> ListContent { set; }

        Action<IMyTerminalBlock, List<MyTerminalControlListBoxItem>> ItemSelected { set; }
    }
}

