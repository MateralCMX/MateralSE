namespace VRage.Game.VisualScripting
{
    using System;
    using System.Runtime.CompilerServices;

    [VisualScriptingEvent(new bool[] { true, true, true })]
    public delegate void TriggerEventComplex(string triggerName, long entityId, string entityName);
}

