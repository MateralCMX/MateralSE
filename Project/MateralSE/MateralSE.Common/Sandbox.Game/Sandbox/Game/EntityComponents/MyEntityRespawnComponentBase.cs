namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Utils;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyEntityRespawnComponentBase : MyEntityComponentBase, IMyCameraController, Sandbox.Game.Entities.IMyControllableEntity, VRage.Game.ModAPI.Interfaces.IMyControllableEntity
    {
        private static List<MyPhysics.HitInfo> m_raycastList;

        protected MyEntityRespawnComponentBase()
        {
        }

        void Sandbox.Game.Entities.IMyControllableEntity.BeginShoot(MyShootActionEnum action)
        {
        }

        bool Sandbox.Game.Entities.IMyControllableEntity.CanSwitchAmmoMagazine() => 
            false;

        bool Sandbox.Game.Entities.IMyControllableEntity.CanSwitchToWeapon(MyDefinitionId? weaponDefinition) => 
            false;

        void Sandbox.Game.Entities.IMyControllableEntity.EndShoot(MyShootActionEnum action)
        {
        }

        MyEntityCameraSettings Sandbox.Game.Entities.IMyControllableEntity.GetCameraEntitySettings() => 
            new MyEntityCameraSettings();

        void Sandbox.Game.Entities.IMyControllableEntity.OnBeginShoot(MyShootActionEnum action)
        {
        }

        void Sandbox.Game.Entities.IMyControllableEntity.OnEndShoot(MyShootActionEnum action)
        {
        }

        void Sandbox.Game.Entities.IMyControllableEntity.PickUpFinished()
        {
        }

        bool Sandbox.Game.Entities.IMyControllableEntity.ShouldEndShootingOnPause(MyShootActionEnum action) => 
            true;

        void Sandbox.Game.Entities.IMyControllableEntity.Sprint(bool enabled)
        {
        }

        void Sandbox.Game.Entities.IMyControllableEntity.SwitchAmmoMagazine()
        {
        }

        void Sandbox.Game.Entities.IMyControllableEntity.SwitchBroadcasting()
        {
        }

        void Sandbox.Game.Entities.IMyControllableEntity.SwitchToWeapon(MyToolbarItemWeapon weapon)
        {
        }

        void Sandbox.Game.Entities.IMyControllableEntity.SwitchToWeapon(MyDefinitionId weaponDefinition)
        {
        }

        void Sandbox.Game.Entities.IMyControllableEntity.UseFinished()
        {
        }

        void IMyCameraController.ControlCamera(MyCamera currentCamera)
        {
            if (MySession.Static.ControlledEntity == null)
            {
                MyThirdPersonSpectator.Static.Update();
                MyThirdPersonSpectator.Static.UpdateZoom();
            }
            if (!MyThirdPersonSpectator.Static.IsCameraForced())
            {
                MyPhysicsComponentBase physics = this.Entity.Physics;
                MatrixD viewMatrix = MyThirdPersonSpectator.Static.GetViewMatrix();
                currentCamera.SetViewMatrix(viewMatrix);
                currentCamera.CameraSpring.Enabled = false;
                currentCamera.CameraSpring.SetCurrentCameraControllerVelocity((physics != null) ? physics.LinearVelocity : Vector3.Zero);
            }
            else
            {
                MatrixD worldMatrix = this.Entity.PositionComp.WorldMatrix;
                Vector3D translation = worldMatrix.Translation;
                Vector3D to = translation + (((worldMatrix.Up + worldMatrix.Right) + worldMatrix.Forward) * 20.0);
                using (MyUtils.ReuseCollection<MyPhysics.HitInfo>(ref m_raycastList))
                {
                    MyPhysics.CastRay(translation, to, m_raycastList, 0);
                    float num = 1f;
                    foreach (MyPhysics.HitInfo info in m_raycastList)
                    {
                        IMyEntity hitEntity = info.HkHitInfo.GetHitEntity();
                        if (!ReferenceEquals(hitEntity, this.Entity) && (!(hitEntity is MyFloatingObject) && (!(hitEntity is MyCharacter) && (info.HkHitInfo.HitFraction < num))))
                        {
                            num = Math.Max((float) 0.1f, (float) (info.HkHitInfo.HitFraction - 0.1f));
                        }
                    }
                    to = translation + ((to - translation) * num);
                }
                MatrixD newViewMatrix = MatrixD.CreateLookAt(to, translation, worldMatrix.Up);
                currentCamera.SetViewMatrix(newViewMatrix);
            }
        }

        bool IMyCameraController.HandlePickUp() => 
            false;

        bool IMyCameraController.HandleUse() => 
            false;

        void IMyCameraController.OnAssumeControl(IMyCameraController previousCameraController)
        {
        }

        void IMyCameraController.OnReleaseControl(IMyCameraController newCameraController)
        {
        }

        void IMyCameraController.Rotate(Vector2 rotationIndicator, float rollIndicator)
        {
        }

        void IMyCameraController.RotateStopped()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Crouch()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Die()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Down()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.DrawHud(IMyCameraController camera, long playerId)
        {
        }

        MatrixD VRage.Game.ModAPI.Interfaces.IMyControllableEntity.GetHeadMatrix(bool includeY, bool includeX, bool forceHeadAnim, bool forceHeadBone)
        {
            MatrixD worldMatrix = this.Entity.PositionComp.WorldMatrix;
            Vector3D translation = worldMatrix.Translation;
            return MatrixD.Invert(MatrixD.Normalize(MatrixD.CreateLookAt(((translation + worldMatrix.Right) + worldMatrix.Forward) + worldMatrix.Up, translation, worldMatrix.Up)));
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Jump(Vector3 moveIndicator)
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.MoveAndRotate(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.MoveAndRotateStopped()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.PickUp()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.PickUpContinues()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ShowInventory()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ShowTerminal()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchDamping()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchHelmet()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchLandingGears()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchLights()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchReactors()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchThrusts()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.SwitchWalk()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Up()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Use()
        {
        }

        void VRage.Game.ModAPI.Interfaces.IMyControllableEntity.UseContinues()
        {
        }

        public MyEntity Entity =>
            ((MyEntity) base.Entity);

        bool IMyCameraController.AllowCubeBuilding =>
            false;

        bool IMyCameraController.ForceFirstPersonCamera
        {
            get => 
                false;
            set
            {
            }
        }

        bool IMyCameraController.IsInFirstPersonView
        {
            get => 
                false;
            set
            {
            }
        }

        bool IMyCameraController.EnableFirstPersonView
        {
            get => 
                false;
            set
            {
            }
        }

        MyEntity Sandbox.Game.Entities.IMyControllableEntity.Entity =>
            this.Entity;

        float Sandbox.Game.Entities.IMyControllableEntity.HeadLocalXAngle
        {
            get => 
                0f;
            set
            {
            }
        }

        float Sandbox.Game.Entities.IMyControllableEntity.HeadLocalYAngle
        {
            get => 
                0f;
            set
            {
            }
        }

        bool Sandbox.Game.Entities.IMyControllableEntity.EnabledBroadcasting =>
            false;

        MyToolbarType Sandbox.Game.Entities.IMyControllableEntity.ToolbarType =>
            MyToolbarType.None;

        MyStringId Sandbox.Game.Entities.IMyControllableEntity.ControlContext =>
            MyStringId.NullOrEmpty;

        MyToolbar Sandbox.Game.Entities.IMyControllableEntity.Toolbar =>
            null;

        MyEntity Sandbox.Game.Entities.IMyControllableEntity.RelativeDampeningEntity
        {
            get => 
                null;
            set
            {
            }
        }

        MyControllerInfo Sandbox.Game.Entities.IMyControllableEntity.ControllerInfo =>
            null;

        IMyEntity VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Entity =>
            this.Entity;

        IMyControllerInfo VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ControllerInfo =>
            null;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ForceFirstPersonCamera
        {
            get => 
                false;
            set
            {
            }
        }

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledThrusts =>
            false;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledDamping =>
            false;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledLights =>
            false;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledLeadingGears =>
            false;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledReactors =>
            false;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.EnabledHelmet =>
            false;

        bool VRage.Game.ModAPI.Interfaces.IMyControllableEntity.PrimaryLookaround =>
            false;
    }
}

