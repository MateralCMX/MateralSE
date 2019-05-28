namespace Sandbox.ModAPI
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;

    public interface IMyTerminalBlock : VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyTerminalBlock
    {
        event Action<Sandbox.ModAPI.IMyTerminalBlock, StringBuilder> AppendingCustomInfo;

        event Action<Sandbox.ModAPI.IMyTerminalBlock> CustomDataChanged;

        event Action<Sandbox.ModAPI.IMyTerminalBlock> CustomNameChanged;

        event Action<Sandbox.ModAPI.IMyTerminalBlock> OwnershipChanged;

        event Action<Sandbox.ModAPI.IMyTerminalBlock> PropertiesChanged;

        event Action<Sandbox.ModAPI.IMyTerminalBlock> ShowOnHUDChanged;

        event Action<Sandbox.ModAPI.IMyTerminalBlock> VisibilityChanged;

        bool IsInSameLogicalGroupAs(Sandbox.ModAPI.IMyTerminalBlock other);
        bool IsSameConstructAs(Sandbox.ModAPI.IMyTerminalBlock other);
        void RefreshCustomInfo();
    }
}

