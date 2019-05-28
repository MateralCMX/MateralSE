namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true })]
    public delegate void ToolbarItemChangedEvent(long entityId, string typeId, string subtypeId, int page, int slot);
}

