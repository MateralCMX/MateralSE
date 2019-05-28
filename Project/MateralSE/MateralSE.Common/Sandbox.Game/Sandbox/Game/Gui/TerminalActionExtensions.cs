namespace Sandbox.Game.Gui
{
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Collections;

    public static class TerminalActionExtensions
    {
        public static void ApplyAction(this IMyTerminalBlock block, string name)
        {
            block.GetAction(name).Apply(block);
        }

        public static void ApplyAction(this IMyTerminalBlock block, string name, ListReader<TerminalActionParameter> parameters)
        {
            block.GetAction(name).Apply(block, parameters);
        }

        public static Sandbox.ModAPI.Interfaces.ITerminalAction GetAction(this IMyTerminalBlock block, string name) => 
            block.GetActionWithName(name);
    }
}

