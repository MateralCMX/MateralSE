namespace Sandbox.ModAPI.Ingame
{
    using System;
    using VRage.Game.ModAPI.Ingame;

    public interface IMyMotorStator : IMyMotorBase, IMyMechanicalConnectionBlock, IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        float Angle { get; }

        float Torque { get; set; }

        float BrakingTorque { get; set; }

        float TargetVelocityRad { get; set; }

        float TargetVelocityRPM { get; set; }

        float LowerLimitRad { get; set; }

        float LowerLimitDeg { get; set; }

        float UpperLimitRad { get; set; }

        float UpperLimitDeg { get; set; }

        float Displacement { get; set; }

        bool RotorLock { get; set; }
    }
}

