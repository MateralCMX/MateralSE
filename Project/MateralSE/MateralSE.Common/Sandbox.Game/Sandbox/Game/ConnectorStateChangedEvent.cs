namespace Sandbox.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { true, true, true, true, true, true, true, true, false })]
    public delegate void ConnectorStateChangedEvent(long entityId, long gridId, string entityName, string gridName, long otherEntityId, long otherGridId, string otherEntityName, string otherGridName, bool isConnected);
}

