namespace VRage.Game.ModAPI.Interfaces
{
    using System;
    using VRage.Game.Entity;
    using VRage.Game.Utils;
    using VRageMath;

    public interface IMyCameraController
    {
        void ControlCamera(MyCamera currentCamera);
        bool HandlePickUp();
        bool HandleUse();
        void OnAssumeControl(IMyCameraController previousCameraController);
        void OnReleaseControl(IMyCameraController newCameraController);
        void Rotate(Vector2 rotationIndicator, float rollIndicator);
        void RotateStopped();

        bool IsInFirstPersonView { get; set; }

        bool ForceFirstPersonCamera { get; set; }

        bool EnableFirstPersonView { get; set; }

        bool AllowCubeBuilding { get; }

        MyEntity Entity { get; }
    }
}

