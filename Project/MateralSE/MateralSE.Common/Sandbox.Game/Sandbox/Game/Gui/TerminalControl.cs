namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TerminalControl
    {
        public MyGuiControlBase Control;
        public Action RefreshHandler;
    }
}

