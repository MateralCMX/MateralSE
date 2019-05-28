namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true, true, true, true, true, true })]
    public delegate void LandingGearUnlockedEvent(long entityId, long gridId, string entityName, string gridName);
}

