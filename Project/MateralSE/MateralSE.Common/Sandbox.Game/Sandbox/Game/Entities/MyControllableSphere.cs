namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Utils;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    internal class MyControllableSphere : MyEntity, IMyCameraController, Sandbox.Game.Entities.IMyControllableEntity, VRage.Game.ModAPI.Interfaces.IMyControllableEntity
    {
        private MyControllerInfo m_info = new MyControllerInfo();
        private MyToolbar m_toolbar;

        public MyControllableSphere()
        {
            this.ControllerInfo.ControlAcquired += new Action<MyEntityController>(this.OnControlAcquired);
            this.ControllerInfo.ControlReleased += new Action<MyEntityController>(this.OnControlReleased);
            this.m_toolbar = new MyToolbar(this.ToolbarType, 9, 9);
        }

        public void BeginShoot(MyShootActionEnum action)
        {
        }

        public bool CanSwitchAmmoMagazine() => 
            false;

        public bool CanSwitchToWeapon(MyDefinitionId? weaponDefinition) => 
            false;

        public void Crouch()
        {
        }

        public void Die()
        {
        }

        public void Down()
        {
        }

        public void DrawHud(IMyCameraController camera, long playerId)
        {
        }

        public void EnableDampeners(bool enable, bool updateSync = true)
        {
        }

        private void EnableIronsight(bool enable, bool newKeyPress, bool changeCamera, bool updateSync = true)
        {
        }

        public void EnableJetpack(bool enable, bool fromLoad = false, bool updateSync = true, bool fromInit = false)
        {
        }

        public void EndShoot(MyShootActionEnum action)
        {
        }

        private void EndShootAll()
        {
        }

        public MyEntityCameraSettings GetCameraEntitySettings() => 
            null;

        public unsafe MatrixD GetHeadMatrix(bool includeY, bool includeX = true, bool forceHeadAnim = false, bool forceHeadBone = false)
        {
            MatrixD worldMatrix = base.WorldMatrix;
            MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
            xdPtr1.Translation -= 4.0 * base.WorldMatrix.Forward;
            return worldMatrix;
        }

        private Matrix GetRotation(Vector2 rotationIndicator, float roll)
        {
            float num = 0.001f;
            return ((Matrix.CreateRotationY(-num * rotationIndicator.Y) * Matrix.CreateRotationX(-num * rotationIndicator.X)) * Matrix.CreateRotationZ((-num * roll) * 10f));
        }

        public override MatrixD GetViewMatrix() => 
            MatrixD.Invert(this.GetHeadMatrix(true, true, false, false));

        public void Init()
        {
            float? scale = null;
            base.Init(null, @"Models\Debug\Sphere", null, scale, null);
            base.WorldMatrix = MatrixD.Identity;
            this.InitSpherePhysics(MyMaterialType.METAL, Vector3.Zero, 0.5f, 100f, MyPerGameSettings.DefaultLinearDamping, MyPerGameSettings.DefaultAngularDamping, 15, RigidBodyFlag.RBF_DEFAULT);
            base.Render.SkipIfTooSmall = false;
            base.Save = false;
        }

        public void Jump(Vector3 moveIndicator)
        {
        }

        public void MoveAndRotate(Vector3 moveIndicator, Vector2 rotationIndicator, float roll)
        {
            float num = 0.1f;
            MatrixD xd = this.GetRotation(rotationIndicator, roll) * base.WorldMatrix;
            xd.Translation = ((base.WorldMatrix.Translation + ((num * base.WorldMatrix.Right) * moveIndicator.X)) + ((num * base.WorldMatrix.Up) * moveIndicator.Y)) - ((num * base.WorldMatrix.Forward) * moveIndicator.Z);
            base.WorldMatrix = xd;
        }

        public void MoveAndRotateStopped()
        {
        }

        public void OnAssumeControl(IMyCameraController previousCameraController)
        {
        }

        public void OnBeginShoot(MyShootActionEnum action)
        {
        }

        public void OnControlAcquired(MyEntityController controller)
        {
        }

        public void OnControlReleased(MyEntityController controller)
        {
        }

        public void OnEndShoot(MyShootActionEnum action)
        {
        }

        public void OnReleaseControl(IMyCameraController newCameraController)
        {
        }

        public void PickUp()
        {
        }

        public void PickUpContinues()
        {
        }

        public void PickUpFinished()
        {
        }

        public void Rotate(Vector2 rotationIndicator, float roll)
        {
            MatrixD rotation = this.GetRotation(rotationIndicator, roll);
            base.WorldMatrix = rotation * base.WorldMatrix;
        }

        public void RotateStopped()
        {
        }

        private void ShootBeginFailed(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        private void ShootFailedLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        private void ShootInternal()
        {
        }

        private void ShootSuccessfulLocal(MyShootActionEnum action)
        {
        }

        public bool ShouldEndShootingOnPause(MyShootActionEnum action) => 
            true;

        public void ShowInventory()
        {
        }

        public void ShowTerminal()
        {
        }

        public void Sprint(bool enabled)
        {
        }

        public void SwitchAmmoMagazine()
        {
        }

        public void SwitchBroadcasting()
        {
        }

        public void SwitchDamping()
        {
        }

        public void SwitchHelmet()
        {
        }

        public void SwitchLandingGears()
        {
        }

        public void SwitchLights()
        {
        }

        public void SwitchOnEndShoot(MyDefinitionId? weaponDefinition)
        {
        }

        public void SwitchReactors()
        {
        }

        public void SwitchThrusts()
        {
        }

        public void SwitchToWeapon(MyToolbarItemWeapon weapon)
        {
        }

        public void SwitchToWeapon(MyDefinitionId weaponDefinition)
        {
        }

        public void SwitchWalk()
        {
        }

        public void Up()
        {
        }

        public void Use()
        {
        }

        public void UseContinues()
        {
        }

        public void UseFinished()
        {
        }

        void IMyCameraController.ControlCamera(MyCamera currentCamera)
        {
            currentCamera.SetViewMatrix(this.GetViewMatrix());
        }

        bool IMyCameraController.HandlePickUp() => 
            false;

        bool IMyCameraController.HandleUse() => 
            false;

        void IMyCameraController.OnAssumeControl(IMyCameraController previousCameraController)
        {
            this.OnAssumeControl(previousCameraController);
        }

        void IMyCameraController.OnReleaseControl(IMyCameraController newCameraController)
        {
            this.OnReleaseControl(newCameraController);
        }

        void IMyCameraController.Rotate(Vector2 rotationIndicator, float rollIndicator)
        {
            this.Rotate(rotationIndicator, rollIndicator);
        }

        void IMyCameraController.RotateStopped()
        {
            this.RotateStopped();
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.DrawHud(IMyCameraController entity, long player)
        {
            if (entity != null)
            {
                this.DrawHud(entity, player);
            }
        }

        public void Zoom(bool newKeyPress)
        {
        }

        public MyControllerInfo ControllerInfo =>
            this.m_info;

        public bool IsInFirstPersonView { get; set; }

        public bool EnabledThrusts =>
            false;

        public bool EnabledDamping =>
            false;

        public bool EnabledLights =>
            false;

        public bool EnabledLeadingGears =>
            false;

        public bool EnabledReactors =>
            false;

        public bool EnabledBroadcasting =>
            false;

        public bool EnabledHelmet =>
            false;

        public bool PrimaryLookaround =>
            false;

        public MyEntity Entity =>
            this;

        public bool ForceFirstPersonCamera { get; set; }

        public float HeadLocalXAngle { get; set; }

        public float HeadLocalYAngle { get; set; }

        public MyToolbarType ToolbarType =>
            MyToolbarType.Spectator;

        public MyToolbar Toolbar =>
            this.m_toolbar;

        bool IMyCameraController.IsInFirstPersonView
        {
            get => 
                this.IsInFirstPersonView;
            set => 
                (this.IsInFirstPersonView = value);
        }

        bool IMyCameraController.ForceFirstPersonCamera
        {
            get => 
                this.ForceFirstPersonCamera;
            set => 
                (this.ForceFirstPersonCamera = value);
        }

        bool IMyCameraController.EnableFirstPersonView
        {
            get => 
                true;
            set
            {
            }
        }

        bool IMyCameraController.AllowCubeBuilding =>
            false;

        public MyStringId ControlContext =>
            MyStringId.NullOrEmpty;

        public MyEntity RelativeDampeningEntity { get; set; }

        IMyEntity VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Entity =>
            this.Entity;

        IMyControllerInfo VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ControllerInfo =>
            this.ControllerInfo;
    }
}

