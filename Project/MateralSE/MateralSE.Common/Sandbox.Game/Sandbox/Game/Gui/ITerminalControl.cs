namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Graphics.GUI;
    using System;

    public interface ITerminalControl
    {
        MyGuiControlBase GetGuiControl();
        bool IsVisible(MyTerminalBlock block);
        void UpdateVisual();

        string Id { get; }

        bool SupportsMultipleBlocks { get; }

        MyTerminalBlock[] TargetBlocks { get; set; }

        ITerminalAction[] Actions { get; }
    }
}

