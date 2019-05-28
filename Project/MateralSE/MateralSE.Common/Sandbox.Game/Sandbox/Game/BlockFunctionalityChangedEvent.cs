namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true, true, true, true, true, true, false })]
    public delegate void BlockFunctionalityChangedEvent(long entityId, long gridId, string enitytName, string gridName, string typeId, string subtypeId, bool BecameFunctional);
}

