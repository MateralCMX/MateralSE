namespace Sandbox.ModAPI.Ingame
{
    using System;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    public interface IMyLaserAntenna : IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        void Connect();
        void SetTargetCoords(string coords);
        [Obsolete("Use IMyIntergridCommunicationSystem instead")]
        bool TransmitMessage(string message);

        bool RequireLoS { get; }

        Vector3D TargetCoords { get; }

        bool IsPermanent { get; set; }

        [Obsolete("Check the Status property instead.")]
        bool IsOutsideLimits { get; }

        MyLaserAntennaStatus Status { get; }

        long AttachedProgrammableBlock { get; set; }

        float Range { get; set; }
    }
}

