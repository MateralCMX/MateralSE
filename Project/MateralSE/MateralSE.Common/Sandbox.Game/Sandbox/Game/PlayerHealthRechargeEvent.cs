namespace Sandbox.Game
{
    using Sandbox.Game.GameSystems;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.VisualScripting;

    [VisualScriptingEvent(new bool[] { false, false, false })]
    public delegate void PlayerHealthRechargeEvent(long playerId, MyLifeSupportingBlockType blockType, float value);
}

