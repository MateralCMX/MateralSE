namespace Sandbox.Engine.Utils
{
    using System;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Utils;
    using VRageMath;

    internal class MyCutsceneCamera : MyEntity, IMyCameraController
    {
        public float FOV = 70f;

        public MyCutsceneCamera()
        {
            base.Init(null);
        }

        public void ControlCamera(MyCamera currentCamera)
        {
            currentCamera.FieldOfViewDegrees = this.FOV;
            currentCamera.SetViewMatrix(MatrixD.Invert(base.WorldMatrix));
        }

        public bool HandlePickUp() => 
            false;

        public bool HandleUse() => 
            false;

        public void OnAssumeControl(IMyCameraController previousCameraController)
        {
        }

        public void OnReleaseControl(IMyCameraController newCameraController)
        {
        }

        public void Rotate(Vector2 rotationIndicator, float rollIndicator)
        {
        }

        public void RotateStopped()
        {
        }

        public MyEntity Entity =>
            this;

        public bool IsInFirstPersonView
        {
            get => 
                true;
            set
            {
            }
        }

        public bool EnableFirstPersonView
        {
            get => 
                true;
            set
            {
            }
        }

        public bool ForceFirstPersonCamera
        {
            get => 
                true;
            set
            {
            }
        }

        public bool AllowCubeBuilding =>
            false;
    }
}

