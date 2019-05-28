namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Cube;
    using System;
    using VRage.Library.Collections;

    public interface ITerminalControlSync
    {
        void Serialize(BitStream stream, MyTerminalBlock block);
    }
}

