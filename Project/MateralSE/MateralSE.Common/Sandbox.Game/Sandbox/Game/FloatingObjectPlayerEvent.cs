namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true, true, true, false, false })]
    public delegate void FloatingObjectPlayerEvent(string itemTypeName, string itemSubTypeName, string entityName, long playerId, int amount);
}

