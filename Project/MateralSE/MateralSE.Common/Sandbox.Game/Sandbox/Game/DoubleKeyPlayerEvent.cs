namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true, false })]
    public delegate void DoubleKeyPlayerEvent(string entityName, long playerId, string gridName);
}

