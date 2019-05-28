namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true, true, true, true, true, false })]
    public delegate void WeaponBlockActivatedEvent(long entityId, long gridId, string entityName, string gridName, string blockType, string blockSubtype);
}

