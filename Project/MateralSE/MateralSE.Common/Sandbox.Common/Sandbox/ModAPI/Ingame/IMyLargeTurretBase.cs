namespace Sandbox.ModAPI.Ingame
{
    using System;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    public interface IMyLargeTurretBase : IMyUserControllableGun, IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        MyDetectedEntityInfo GetTargetedEntity();
        void ResetTargetingToDefault();
        void SetTarget(Vector3D pos);
        void SyncAzimuth();
        void SyncElevation();
        void SyncEnableIdleRotation();
        void TrackTarget(Vector3D pos, Vector3 velocity);

        bool IsUnderControl { get; }

        bool CanControl { get; }

        float Range { get; }

        bool IsAimed { get; }

        bool HasTarget { get; }

        float Elevation { get; set; }

        float Azimuth { get; set; }

        bool EnableIdleRotation { get; set; }

        bool AIEnabled { get; }
    }
}

