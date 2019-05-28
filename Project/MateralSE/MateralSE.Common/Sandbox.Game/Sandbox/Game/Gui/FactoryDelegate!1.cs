namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Runtime.CompilerServices;

    internal delegate TerminalControl FactoryDelegate<in T>(T property, MyTerminalBlock[] blocks);
}

