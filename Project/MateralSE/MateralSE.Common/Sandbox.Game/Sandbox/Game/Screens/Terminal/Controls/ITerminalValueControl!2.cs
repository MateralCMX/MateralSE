namespace Sandbox.Game.Screens.Terminal.Controls
{
    using Sandbox.Game.Gui;
    using Sandbox.ModAPI.Interfaces;
    using System;
    using VRage.Library.Collections;

    internal interface ITerminalValueControl<TBlock, TValue> : ITerminalProperty<TValue>, ITerminalProperty, ITerminalControl, ITerminalControlSync where TBlock: MyTerminalBlock
    {
        TValue GetDefaultValue(TBlock block);
        TValue GetMaximum(TBlock block);
        TValue GetMinimum(TBlock block);
        [Obsolete("Use GetMinimum instead")]
        TValue GetMininum(TBlock block);
        TValue GetValue(TBlock block);
        void Serialize(BitStream stream, TBlock block);
        void SetValue(TBlock block, TValue value);
    }
}

