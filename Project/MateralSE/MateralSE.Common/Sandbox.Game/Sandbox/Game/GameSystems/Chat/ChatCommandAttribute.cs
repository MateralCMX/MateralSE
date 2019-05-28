namespace Sandbox.Game.GameSystems.Chat
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.ModAPI;

    [AttributeUsage(AttributeTargets.Method)]
    public class ChatCommandAttribute : Attribute
    {
        public string CommandText;
        public string HelpText;
        public string HelpSimpleText;
        public MyPromoteLevel VisibleTo;
        internal bool DebugCommand;

        public ChatCommandAttribute(string commandText, string helpText, string helpSimpleText, MyPromoteLevel visibleTo = 0)
        {
            this.CommandText = commandText;
            this.HelpText = helpText;
            this.HelpSimpleText = helpSimpleText;
            this.VisibleTo = visibleTo;
        }

        internal ChatCommandAttribute(string commandText, string helpText, string helpSimpleText, bool debugCommand, MyPromoteLevel visibleTo = 0)
        {
            this.CommandText = commandText;
            this.HelpText = helpText;
            this.HelpSimpleText = helpSimpleText;
            this.DebugCommand = debugCommand;
            this.VisibleTo = visibleTo;
        }
    }
}

