namespace Sandbox.ModAPI.Ingame
{
    using System;
    using VRage.Game.ModAPI.Ingame;

    public interface IMyPistonBase : IMyMechanicalConnectionBlock, IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        void Extend();
        void Retract();
        void Reverse();

        float Velocity { get; set; }

        float MaxVelocity { get; }

        float MinLimit { get; set; }

        float MaxLimit { get; set; }

        float LowestPosition { get; }

        float HighestPosition { get; }

        float CurrentPosition { get; }

        PistonStatus Status { get; }
    }
}

