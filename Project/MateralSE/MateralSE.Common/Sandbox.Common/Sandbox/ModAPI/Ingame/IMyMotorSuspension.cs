namespace Sandbox.ModAPI.Ingame
{
    using System;
    using VRage.Game.ModAPI.Ingame;

    public interface IMyMotorSuspension : IMyMotorBase, IMyMechanicalConnectionBlock, IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        bool Steering { get; set; }

        bool Propulsion { get; set; }

        bool InvertSteer { get; set; }

        bool InvertPropulsion { get; set; }

        [Obsolete]
        float Damping { get; }

        float Strength { get; set; }

        float Friction { get; set; }

        float Power { get; set; }

        float Height { get; set; }

        float SteerAngle { get; }

        float MaxSteerAngle { get; set; }

        [Obsolete]
        float SteerSpeed { get; }

        [Obsolete]
        float SteerReturnSpeed { get; }

        [Obsolete]
        float SuspensionTravel { get; }

        bool Brake { get; set; }

        bool AirShockEnabled { get; set; }
    }
}

