namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true, true, false })]
    public delegate void GridPowerGenerationStateChangedEvent(long gridId, string gridName, bool IsPowered);
}

