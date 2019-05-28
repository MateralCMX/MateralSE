namespace SpaceEngineers.Game.ModAPI.Ingame
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.ModAPI.Ingame;

    public interface IMySpaceBall : IMyVirtualMass, IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        float Friction { get; set; }

        float Restitution { get; set; }

        [Obsolete("Use IMySpaceBall.Broadcasting")]
        bool IsBroadcasting { get; }

        bool Broadcasting { get; set; }

        float VirtualMass { get; set; }
    }
}

