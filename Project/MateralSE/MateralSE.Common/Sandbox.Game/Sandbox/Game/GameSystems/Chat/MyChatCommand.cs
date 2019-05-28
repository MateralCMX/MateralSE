namespace Sandbox.Game.GameSystems.Chat
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.ModAPI;

    internal class MyChatCommand : IMyChatCommand
    {
        private readonly Action<string[]> m_action;

        public MyChatCommand(string commandText, string helpText, string helpSimpleText, Action<string[]> action, MyPromoteLevel visibleTo = 0)
        {
            this.CommandText = commandText;
            this.HelpText = helpText;
            this.HelpSimpleText = helpSimpleText;
            this.m_action = action;
            this.VisibleTo = visibleTo;
        }

        public void Handle(string[] args)
        {
            this.m_action(args);
        }

        public string CommandText { get; private set; }

        public string HelpText { get; private set; }

        public string HelpSimpleText { get; private set; }

        public MyPromoteLevel VisibleTo { get; private set; }
    }
}

