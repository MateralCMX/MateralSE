namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true, true, true, true, true, true })]
    public delegate void RemoteControlChangedEvent(bool GotControlled, long playerId, string entityName, long entityId, string gridName, long gridId);
}

