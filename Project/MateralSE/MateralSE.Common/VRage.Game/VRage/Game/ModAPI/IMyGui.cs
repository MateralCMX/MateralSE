namespace VRage.Game.ModAPI
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.ModAPI;

    public interface IMyGui
    {
        event Action<object> GuiControlCreated;

        event Action<object> GuiControlRemoved;

        string ActiveGamePlayScreen { get; }

        IMyEntity InteractedEntity { get; }

        MyTerminalPageEnum GetCurrentScreen { get; }

        bool ChatEntryVisible { get; }

        bool IsCursorVisible { get; }
    }
}

