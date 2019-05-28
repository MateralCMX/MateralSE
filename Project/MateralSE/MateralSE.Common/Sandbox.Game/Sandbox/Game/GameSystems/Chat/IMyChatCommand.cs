namespace Sandbox.Game.GameSystems.Chat
{
    using System;
    using VRage.Game.ModAPI;

    public interface IMyChatCommand
    {
        void Handle(string[] args);

        string CommandText { get; }

        string HelpText { get; }

        string HelpSimpleText { get; }

        MyPromoteLevel VisibleTo { get; }
    }
}

