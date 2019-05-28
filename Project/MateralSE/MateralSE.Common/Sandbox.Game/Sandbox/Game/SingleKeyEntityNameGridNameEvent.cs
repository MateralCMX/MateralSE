namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true, true })]
    public delegate void SingleKeyEntityNameGridNameEvent(string entityName, string gridName, string typeId, string subtypeId);
}

