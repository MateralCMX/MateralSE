namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Valve.VR;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Utils;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.OpenVRWrapper;
    using VRage.Utils;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_GhostCharacter), true)]
    public class MyGhostCharacter : MyEntity, Sandbox.Game.Entities.IMyControllableEntity, VRage.Game.ModAPI.Interfaces.IMyControllableEntity, IMyCameraController, IMyDestroyableObject
    {
        private List<MyVRWeaponInfo> m_leftWeapons = new List<MyVRWeaponInfo>();
        private List<MyVRWeaponInfo> m_rightWeapons = new List<MyVRWeaponInfo>();
        private bool m_weaponsInitialized;
        private MyControllerInfo m_info = new MyControllerInfo();
        private IMyHandheldGunObject<MyDeviceBase> m_leftWeapon;
        private IMyHandheldGunObject<MyDeviceBase> m_rightWeapon;
        private MyVRWeaponInfo m_leftWeaponInfo;
        private MyVRWeaponInfo m_rightWeaponInfo;
        private MatrixD m_worldMatrixOriginal;
        private float MyDamage;
        private static readonly Color COLOR_WHEN_HIT = new Color(1, 0, 0, 0);
        private static readonly float PLAYER_TARGET_SIZE = 1f;
        private MyEntityCameraSettings m_cameraSettings;

        public void BeginShoot(MyShootActionEnum action)
        {
        }

        public bool CanSwitchAmmoMagazine() => 
            true;

        public bool CanSwitchToWeapon(MyDefinitionId? weaponDefinition) => 
            true;

        private IMyHandheldGunObject<MyDeviceBase> CreateWeapon(MyDefinitionId? weaponDefinition, int lastMomentUpdateIndex)
        {
            MyObjectBuilder_EntityBase gunEntity = MyObjectBuilderSerializer.CreateNewObject(weaponDefinition.Value.TypeId) as MyObjectBuilder_EntityBase;
            if (gunEntity == null)
            {
                return null;
            }
            gunEntity.SubtypeName = weaponDefinition.Value.SubtypeId.String;
            uint? inventoryItemId = null;
            IMyHandheldGunObject<MyDeviceBase> obj1 = MyCharacter.CreateGun(gunEntity, inventoryItemId);
            MyEntity entity = (MyEntity) obj1;
            entity.Render.CastShadows = true;
            entity.Render.NeedsResolveCastShadow = false;
            entity.Render.LastMomentUpdateIndex = lastMomentUpdateIndex;
            entity.Save = false;
            entity.OnClose += new Action<MyEntity>(this.gunEntity_OnClose);
            MyEntities.Add(entity, true);
            if (entity.Model != null)
            {
                entity.InitBoxPhysics(MyMaterialType.METAL, entity.Model, 10f, MyPerGameSettings.DefaultAngularDamping, 15, RigidBodyFlag.RBF_KINEMATIC);
                entity.Physics.Enabled = true;
            }
            return obj1;
        }

        public void Crouch()
        {
        }

        public void Die()
        {
        }

        public bool DoDamage(float damage, MyStringHash damageSource, bool sync, MyHitInfo? hitInfo = new MyHitInfo?(), long attackerId = 0L)
        {
            this.MyDamage += damage;
            MyOpenVR.FadeToColor(0.1f, COLOR_WHEN_HIT);
            return true;
        }

        public void Down()
        {
        }

        public void DrawHud(IMyCameraController camera, long playerId)
        {
        }

        public void EndShoot(MyShootActionEnum action)
        {
        }

        public MyEntityCameraSettings GetCameraEntitySettings()
        {
            if ((this.ControllerInfo.Controller != null) && (this.ControllerInfo.Controller.Player != null))
            {
                MyPlayer.PlayerId id = this.ControllerInfo.Controller.Player.Id;
                if (!MySession.Static.Cameras.TryGetCameraSettings(this.ControllerInfo.Controller.Player.Id, base.EntityId, true, out this.m_cameraSettings) && this.ControllerInfo.IsLocallyHumanControlled())
                {
                    MyEntityCameraSettings settings1 = new MyEntityCameraSettings();
                    settings1.Distance = 0.0;
                    settings1.IsFirstPerson = true;
                    settings1.HeadAngle = new Vector2(this.HeadLocalXAngle, this.HeadLocalYAngle);
                    this.m_cameraSettings = settings1;
                }
            }
            return this.m_cameraSettings;
        }

        public MatrixD GetHeadMatrix(bool includeY, bool includeX, bool forceHeadAnim, bool forceHeadBone) => 
            (MyOpenVR.HeadsetMatrixD * this.m_worldMatrixOriginal);

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) => 
            ((MyObjectBuilder_Character) base.GetObjectBuilder(copy));

        private void gunEntity_OnClose(MyEntity obj)
        {
            if (ReferenceEquals(this.m_leftWeapon, obj))
            {
                this.m_leftWeapon = null;
            }
            if (ReferenceEquals(this.m_rightWeapon, obj))
            {
                this.m_rightWeapon = null;
            }
        }

        private void HandleButtons(ref IMyHandheldGunObject<MyDeviceBase> weapon, ref MyVRWeaponInfo weaponInfo, bool secondController, List<MyVRWeaponInfo> weapons)
        {
            if (weapon != null)
            {
                Vector3D? nullable;
                MyWeaponSharedActionsComponentBase component = null;
                ((MyEntity) weapon).Components.TryGet<MyWeaponSharedActionsComponentBase>(out component);
                if (component != null)
                {
                    component.Update();
                }
                if (MyOpenVR.GetControllerState(secondController).IsButtonPressed(EVRButtonId.k_EButton_Axis1))
                {
                    MyGunStatusEnum enum2;
                    MyGunBase gunBase = weapon.GunBase as MyGunBase;
                    weapon.CanShoot(MyShootActionEnum.PrimaryAction, base.EntityId, out enum2);
                    if ((enum2 != MyGunStatusEnum.Cooldown) && (enum2 != MyGunStatusEnum.BurstLimit))
                    {
                        Vector3 forward;
                        if (gunBase == null)
                        {
                            forward = Vector3.Forward;
                        }
                        else
                        {
                            forward = (Vector3) gunBase.GetMuzzleWorldMatrix().Forward;
                        }
                        nullable = null;
                        weapon.Shoot(MyShootActionEnum.PrimaryAction, forward, nullable, null);
                        if (component != null)
                        {
                            component.Shoot(MyShootActionEnum.PrimaryAction);
                        }
                    }
                }
                if (MyOpenVR.GetControllerState(secondController).WasButtonReleased(EVRButtonId.k_EButton_Axis1))
                {
                    weapon.EndShoot(MyShootActionEnum.PrimaryAction);
                    if (component != null)
                    {
                        component.EndShoot(MyShootActionEnum.PrimaryAction);
                    }
                }
                if (MyOpenVR.GetControllerState(secondController).WasButtonPressed(EVRButtonId.k_EButton_Grip))
                {
                    Vector3 forward;
                    MyGunBase gunBase = weapon.GunBase as MyGunBase;
                    if (gunBase == null)
                    {
                        forward = Vector3.Forward;
                    }
                    else
                    {
                        forward = (Vector3) gunBase.GetMuzzleWorldMatrix().Forward;
                    }
                    nullable = null;
                    weapon.Shoot(MyShootActionEnum.SecondaryAction, forward, nullable, null);
                    if (component != null)
                    {
                        component.Shoot(MyShootActionEnum.SecondaryAction);
                    }
                }
                if (MyOpenVR.GetControllerState(secondController).WasButtonReleased(EVRButtonId.k_EButton_Grip))
                {
                    weapon.EndShoot(MyShootActionEnum.SecondaryAction);
                    if (component != null)
                    {
                        component.EndShoot(MyShootActionEnum.SecondaryAction);
                    }
                }
                if (MyOpenVR.GetControllerState(secondController).WasButtonPressed(EVRButtonId.k_EButton_ApplicationMenu))
                {
                    Vector3 forward;
                    MyGunBase gunBase = weapon.GunBase as MyGunBase;
                    if (gunBase == null)
                    {
                        forward = Vector3.Forward;
                    }
                    else
                    {
                        forward = (Vector3) gunBase.GetMuzzleWorldMatrix().Forward;
                    }
                    nullable = null;
                    weapon.Shoot(MyShootActionEnum.TertiaryAction, forward, nullable, null);
                    if (component != null)
                    {
                        component.Shoot(MyShootActionEnum.TertiaryAction);
                    }
                }
                if (MyOpenVR.GetControllerState(secondController).WasButtonReleased(EVRButtonId.k_EButton_ApplicationMenu))
                {
                    weapon.EndShoot(MyShootActionEnum.TertiaryAction);
                    if (component != null)
                    {
                        component.EndShoot(MyShootActionEnum.TertiaryAction);
                    }
                }
                Vector2 zero = Vector2.Zero;
                bool touchpadXY = false;
                touchpadXY = MyOpenVR.GetControllerState(secondController).GetTouchpadXY(ref zero);
                if (!MyOpenVR.GetControllerState(secondController).WasButtonPressed(EVRButtonId.k_EButton_Axis0))
                {
                    if (touchpadXY && (weapon is ITouchPadListener))
                    {
                        (weapon as ITouchPadListener).TouchPadChanged(zero);
                    }
                }
                else
                {
                    Vector2? nullable1;
                    if (touchpadXY)
                    {
                        nullable1 = new Vector2?(zero);
                    }
                    else
                    {
                        nullable1 = null;
                    }
                    this.SwitchWeapon(ref weapon, ref weaponInfo, weapons, secondController ? ControllerRole.rightHand : ControllerRole.leftHand, nullable1);
                }
            }
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            MyObjectBuilder_GhostCharacter character1 = (MyObjectBuilder_GhostCharacter) objectBuilder;
            base.Init(objectBuilder);
            this.SetupPhysics(true);
            this.m_worldMatrixOriginal = base.WorldMatrix;
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            MatrixD? leftMult = null;
            MyOpenVR.LMUAdd(leftMult, base.WorldMatrix, ControllerRole.head, 1);
        }

        public void Jump(Vector3 moveIndicator)
        {
        }

        public void MoveAndRotate(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
        }

        public void MoveAndRotateStopped()
        {
        }

        public void OnBeginShoot(MyShootActionEnum action)
        {
        }

        public void OnDestroy()
        {
            throw new NotImplementedException();
        }

        public void OnEndShoot(MyShootActionEnum action)
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
            throw new NotImplementedException();
        }

        private void RigidBody_ContactPointCallback(ref HkContactPointEvent value)
        {
            if (((((this.Physics.CharacterProxy != null) && MySession.Static.Ready) && ((value.Base.BodyA != null) && (value.Base.BodyB != null))) && ((value.Base.BodyA.UserObject != null) && (value.Base.BodyB.UserObject != null))) && !value.Base.BodyA.HasProperty(HkCharacterRigidBody.MANIPULATED_OBJECT))
            {
                value.Base.BodyB.HasProperty(HkCharacterRigidBody.MANIPULATED_OBJECT);
            }
        }

        internal void SetupPhysics(bool isLocalPlayer)
        {
            this.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_KINEMATIC);
            this.Physics.IsPhantom = false;
            Vector3 center = base.PositionComp.LocalVolume.Center;
            HkMassProperties? massProperties = null;
            this.Physics.CreateFromCollisionObject((HkShape) new HkSphereShape(PLAYER_TARGET_SIZE), center, base.WorldMatrix, massProperties, 0);
            this.Physics.Enabled = true;
            this.Physics.RigidBody.ContactPointCallback += new ContactPointEventHandler(this.RigidBody_ContactPointCallback);
            this.Physics.RigidBody.ContactPointCallbackEnabled = true;
            base.PositionComp.LocalAABB = new BoundingBox(new Vector3(-PLAYER_TARGET_SIZE), new Vector3(PLAYER_TARGET_SIZE));
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

        private void SwitchWeapon(ref IMyHandheldGunObject<MyDeviceBase> weapon, ref MyVRWeaponInfo weaponInfo, List<MyVRWeaponInfo> weapons, ControllerRole role, Vector2? touchpadPos)
        {
            int num = -1;
            if (weapon != null)
            {
                for (int i = 0; i < weapons.Count; i++)
                {
                    if (weapons[i].DefinitionId == weapon.DefinitionId)
                    {
                        num = i;
                        break;
                    }
                }
            }
            int num2 = 0;
            if (touchpadPos != null)
            {
                Vector2 vector = touchpadPos.Value;
                Vector2? nullable = touchpadPos;
                Vector2 zero = Vector2.Zero;
                if ((nullable != null) ? ((nullable != null) ? (nullable.GetValueOrDefault() != zero) : false) : true)
                {
                    vector.Normalize();
                    float num4 = 360f / ((float) weapons.Count);
                    float num5 = Vector2.Dot(Vector2.UnitX, vector);
                    float num6 = (float) Math.Acos((double) Math.Abs(num5));
                    if (vector.Y < 0f)
                    {
                        num6 = (num5 >= 0f) ? (360f - num6) : (180f + num6);
                    }
                    else if (num5 < 0f)
                    {
                        num6 = 180f - num6;
                    }
                    num2 = (int) Math.Floor((double) (num6 / num4));
                }
            }
            if (num2 != num)
            {
                if (weapon != null)
                {
                    weapon.OnControlReleased();
                    ((MyEntity) weapon).Close();
                    weapon = null;
                }
                weaponInfo = weapons[num2];
                weapon = this.CreateWeapon(new MyDefinitionId?(weaponInfo.DefinitionId), weaponInfo.Reference);
                weapon.OnControlAcquired(null);
                MyEntity entity1 = (MyEntity) weapon;
                MyGunBase gunBase = weapon.GunBase as MyGunBase;
                MyOpenVR.LMUAdd(new MatrixD?((gunBase != null) ? gunBase.m_holdingDummyMatrix : Matrix.Identity), this.m_worldMatrixOriginal, role, weaponInfo.Reference);
            }
        }

        public void Up()
        {
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            base.WorldMatrix = MyOpenVR.HeadsetMatrixD * this.m_worldMatrixOriginal;
            this.Physics.RigidBody.SetWorldMatrix((Matrix) base.WorldMatrix);
            if (this.m_leftWeapon != null)
            {
                MyGunBase gunBase = this.m_leftWeapon.GunBase as MyGunBase;
                ((MyEntity) this.m_leftWeapon).WorldMatrix = (gunBase == null) ? (MyOpenVR.Controller1Matrix * this.m_worldMatrixOriginal) : ((gunBase.m_holdingDummyMatrix * MyOpenVR.Controller1Matrix) * this.m_worldMatrixOriginal);
                this.HandleButtons(ref this.m_leftWeapon, ref this.m_leftWeaponInfo, false, this.m_leftWeapons);
            }
            if (this.m_rightWeapon != null)
            {
                MyGunBase gunBase = this.m_rightWeapon.GunBase as MyGunBase;
                ((MyEntity) this.m_rightWeapon).WorldMatrix = (gunBase == null) ? (MyOpenVR.Controller2Matrix * this.m_worldMatrixOriginal) : ((gunBase.m_holdingDummyMatrix * MyOpenVR.Controller2Matrix) * this.m_worldMatrixOriginal);
                this.HandleButtons(ref this.m_rightWeapon, ref this.m_rightWeaponInfo, true, this.m_rightWeapons);
            }
            if (this.MyDamage > 0f)
            {
                this.MyDamage -= 80f;
                if (this.MyDamage <= 0f)
                {
                    MyOpenVR.UnFade(0.5f);
                }
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (!this.m_weaponsInitialized)
            {
                int num = 2;
                IEnumerable<MyGhostCharacterDefinition> definitions = MyDefinitionManager.Static.GetDefinitions<MyGhostCharacterDefinition>();
                if (definitions != null)
                {
                    foreach (MyGhostCharacterDefinition definition in definitions)
                    {
                        MyVRWeaponInfo info;
                        foreach (MyDefinitionId id in definition.LeftHandWeapons)
                        {
                            info = new MyVRWeaponInfo {
                                DefinitionId = id
                            };
                            num++;
                            info.Reference = num;
                            this.m_leftWeapons.Add(info);
                        }
                        Vector2? touchpadPos = null;
                        this.SwitchWeapon(ref this.m_leftWeapon, ref this.m_leftWeaponInfo, this.m_leftWeapons, ControllerRole.leftHand, touchpadPos);
                        foreach (MyDefinitionId id2 in definition.RightHandWeapons)
                        {
                            info = new MyVRWeaponInfo {
                                DefinitionId = id2
                            };
                            num++;
                            info.Reference = num;
                            this.m_rightWeapons.Add(info);
                        }
                        touchpadPos = null;
                        this.SwitchWeapon(ref this.m_rightWeapon, ref this.m_rightWeaponInfo, this.m_rightWeapons, ControllerRole.rightHand, touchpadPos);
                    }
                }
                this.m_weaponsInitialized = true;
            }
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

        public MyPhysicsBody Physics
        {
            get => 
                (base.Physics as MyPhysicsBody);
            set => 
                (base.Physics = value);
        }

        public MyControllerInfo ControllerInfo =>
            this.m_info;

        public MyEntity Entity =>
            this;

        public float HeadLocalXAngle
        {
            get => 
                0f;
            set
            {
            }
        }

        public float HeadLocalYAngle
        {
            get => 
                0f;
            set
            {
            }
        }

        public bool EnabledBroadcasting =>
            false;

        public MyToolbarType ToolbarType =>
            MyToolbarType.Character;

        public MyStringId ControlContext =>
            MySpaceBindingCreator.CX_CHARACTER;

        public MyToolbar Toolbar =>
            null;

        IMyEntity VRage.Game.ModAPI.Interfaces.IMyControllableEntity.Entity =>
            this;

        IMyControllerInfo VRage.Game.ModAPI.Interfaces.IMyControllableEntity.ControllerInfo =>
            this.ControllerInfo;

        public bool ForceFirstPersonCamera
        {
            get => 
                false;
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

        public bool EnabledHelmet =>
            false;

        public bool PrimaryLookaround =>
            false;

        bool IMyCameraController.IsInFirstPersonView
        {
            get => 
                true;
            set
            {
            }
        }

        bool IMyCameraController.ForceFirstPersonCamera
        {
            get => 
                false;
            set
            {
            }
        }

        bool IMyCameraController.AllowCubeBuilding =>
            false;

        public float Integrity =>
            100f;

        public bool UseDamageSystem
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public MyEntity RelativeDampeningEntity { get; set; }

        public interface ITouchPadListener
        {
            void TouchPadChanged(Vector2 position);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyVRWeaponInfo
        {
            public MyDefinitionId DefinitionId;
            public int Reference;
        }
    }
}

