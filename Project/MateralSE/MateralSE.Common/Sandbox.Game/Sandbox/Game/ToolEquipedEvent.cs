namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true })]
    public delegate void ToolEquipedEvent(long playerId, string typeId, string subtypeId);
}

