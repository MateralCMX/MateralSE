namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Cube;
    using Sandbox.ModAPI.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage.Collections;
    using VRage.Game;

    public interface ITerminalAction : Sandbox.ModAPI.Interfaces.ITerminalAction
    {
        void Apply(MyTerminalBlock block);
        void Apply(MyTerminalBlock block, ListReader<TerminalActionParameter> parameters);
        ListReader<TerminalActionParameter> GetParameterDefinitions();
        bool IsEnabled(MyTerminalBlock block);
        bool IsValidForGroups();
        bool IsValidForToolbarType(MyToolbarType toolbarType);
        void RequestParameterCollection(IList<TerminalActionParameter> parameters, Action<bool> callback);
        void WriteValue(MyTerminalBlock block, StringBuilder appendTo);

        string Id { get; }

        string Icon { get; }

        StringBuilder Name { get; }
    }
}

