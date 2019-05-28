namespace Sandbox.ModAPI.Ingame
{
    using System;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    public interface IMyThrust : IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        float ThrustOverride { get; set; }

        float ThrustOverridePercentage { get; set; }

        float MaxThrust { get; }

        float MaxEffectiveThrust { get; }

        float CurrentThrust { get; }

        Vector3I GridThrustDirection { get; }
    }
}

