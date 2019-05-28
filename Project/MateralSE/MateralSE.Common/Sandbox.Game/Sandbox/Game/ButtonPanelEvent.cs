namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true, true, false, false })]
    public delegate void ButtonPanelEvent(string name, int button, long playerId, long blockId);
}

