namespace Sandbox.Game
{
    using Sandbox.Game.GameSystems;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { false, false })]
    public delegate void PlayerSuitRechargeEvent(long playerId, MyLifeSupportingBlockType blockType);
}

