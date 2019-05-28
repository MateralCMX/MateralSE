namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true, true, true })]
    public delegate void GridJumpedEvent(long playerId, string gridName, long gridId);
}

