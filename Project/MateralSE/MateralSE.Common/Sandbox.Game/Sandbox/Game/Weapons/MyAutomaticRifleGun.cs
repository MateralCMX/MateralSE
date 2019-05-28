namespace Sandbox.Game.Weapons
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.ModAPI.Weapons;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_AutomaticRifle), true)]
    public class MyAutomaticRifleGun : MyEntity, IMyHandheldGunObject<MyGunBase>, IMyGunObject<MyGunBase>, IMyGunBaseUser, IMyEventProxy, IMyEventOwner, IMyAutomaticRifleGun, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyMissileGunObject, IMySyncedEntity
    {
        private int m_lastTimeShoot;
        public static float RIFLE_MAX_SHAKE = 0.5f;
        public static float RIFLE_FOV_SHAKE = 0.0065f;
        private int m_lastDirectionChangeAnnounce;
        private MyParticleEffect m_smokeEffect;
        private MyGunBase m_gunBase;
        private static MyDefinitionId m_handItemDefId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), "AutomaticRifleGun");
        private MyPhysicalItemDefinition m_physicalItemDef;
        private MyCharacter m_owner;
        protected Dictionary<MyShootActionEnum, bool> m_isActionDoubleClicked = new Dictionary<MyShootActionEnum, bool>();
        private int m_shootingCounter;
        private bool m_canZoom = true;
        private MyEntity3DSoundEmitter m_soundEmitter;
        private int m_shotsFiredInBurst;
        private MyHudNotification m_outOfAmmoNotification;
        private MyHudNotification m_safezoneNotification;
        private bool m_isAfterReleaseFire;
        private MyEntity[] m_shootIgnoreEntities;

        public MyAutomaticRifleGun()
        {
            this.m_shootIgnoreEntities = new MyEntity[] { this };
            base.NeedsUpdate = MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            base.Render.NeedsDraw = true;
            this.m_gunBase = new MyGunBase();
            this.m_soundEmitter = new MyEntity3DSoundEmitter(this, false, 1f);
            (base.PositionComp as MyPositionComponent).WorldPositionChanged = new Action<object>(this.WorldPositionChanged);
            base.Render = new MyRenderComponentAutomaticRifle();
            this.SyncType = SyncHelpers.Compose(this, 0);
            this.SyncType.Append(this.m_gunBase);
        }

        public void BeginFailReaction(MyShootActionEnum action, MyGunStatusEnum status)
        {
            if (status == MyGunStatusEnum.OutOfAmmo)
            {
                this.m_gunBase.StartNoAmmoSound(this.m_soundEmitter);
            }
        }

        public void BeginFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
            if ((status == MyGunStatusEnum.Failed) || (status == MyGunStatusEnum.SafeZoneDenied))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
            }
            if (status == MyGunStatusEnum.OutOfAmmo)
            {
                if (this.m_outOfAmmoNotification == null)
                {
                    this.m_outOfAmmoNotification = new MyHudNotification(MyCommonTexts.OutOfAmmo, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                }
                object[] arguments = new object[] { base.DisplayName };
                this.m_outOfAmmoNotification.SetTextFormatArguments(arguments);
                MyHud.Notifications.Add(this.m_outOfAmmoNotification);
            }
            if (status == MyGunStatusEnum.SafeZoneDenied)
            {
                if (this.m_safezoneNotification == null)
                {
                    this.m_safezoneNotification = new MyHudNotification(MyCommonTexts.SafeZone_ShootingDisabled, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                }
                MyHud.Notifications.Add(this.m_safezoneNotification);
            }
        }

        public void BeginShoot(MyShootActionEnum action)
        {
        }

        public bool CanDoubleClickToStick(MyShootActionEnum action) => 
            false;

        public bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            status = MyGunStatusEnum.OK;
            if (this.m_owner == null)
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if (!MySessionComponentSafeZones.IsActionAllowed(this.m_owner, MySafeZoneAction.Shooting, 0L))
            {
                status = MyGunStatusEnum.SafeZoneDenied;
                return false;
            }
            if (action != MyShootActionEnum.PrimaryAction)
            {
                if (action != MyShootActionEnum.SecondaryAction)
                {
                    status = MyGunStatusEnum.Failed;
                    return false;
                }
                if (this.m_canZoom)
                {
                    return true;
                }
                status = MyGunStatusEnum.Cooldown;
                return false;
            }
            if (!this.m_gunBase.HasAmmoMagazines)
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if ((this.m_gunBase.ShotsInBurst > 0) && (this.m_shotsFiredInBurst >= this.m_gunBase.ShotsInBurst))
            {
                status = MyGunStatusEnum.BurstLimit;
                return false;
            }
            if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastTimeShoot) < this.m_gunBase.ShootIntervalInMiliseconds)
            {
                status = MyGunStatusEnum.Cooldown;
                return false;
            }
            if (this.m_owner.GetCurrentMovementState() == MyCharacterMovementEnum.Sprinting)
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if (MySession.Static.CreativeMode || ((this.m_owner.CurrentWeapon is MyAutomaticRifleGun) && this.m_gunBase.HasEnoughAmmunition()))
            {
                status = MyGunStatusEnum.OK;
                return true;
            }
            status = MyGunStatusEnum.OutOfAmmo;
            return false;
        }

        protected override void Closing()
        {
            this.IsShooting = false;
            this.m_gunBase.RemoveOldEffects(MyWeaponDefinition.WeaponEffectAction.Shoot);
            if (this.m_smokeEffect != null)
            {
                this.m_smokeEffect.Stop(true);
                this.m_smokeEffect = null;
            }
            if (this.m_soundEmitter.Loop)
            {
                this.m_soundEmitter.StopSound(false, true);
            }
            base.Closing();
        }

        private void CreateSmokeEffect()
        {
            if (((this.m_smokeEffect == null) && (MySector.MainCamera.GetDistanceFromPoint(base.PositionComp.GetPosition()) < 150.0)) && MyParticlesManager.TryCreateParticleEffect("Smoke_Autocannon", base.PositionComp.WorldMatrix, out this.m_smokeEffect))
            {
                this.m_smokeEffect.OnDelete += new EventHandler(this.OnSmokeEffectDelete);
            }
        }

        public Vector3 DirectionToTarget(Vector3D target)
        {
            Vector3D vectord;
            MyCharacterWeaponPositionComponent component = this.Owner.Components.Get<MyCharacterWeaponPositionComponent>();
            if ((component == null) || !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                vectord = Vector3D.Normalize(target - base.PositionComp.WorldMatrix.Translation);
            }
            else
            {
                vectord = Vector3D.Normalize(target - component.LogicalPositionWorld);
            }
            return (Vector3) vectord;
        }

        public void DoubleClicked(MyShootActionEnum action)
        {
            this.m_isActionDoubleClicked[action] = true;
        }

        public void DrawHud(IMyCameraController camera, long playerId)
        {
            MyHud.BlockInfo.Visible = true;
            MyHud.BlockInfo.MissingComponentIndex = -1;
            MyHud.BlockInfo.BlockName = this.PhysicalItemDefinition.DisplayNameText;
            MyHud.BlockInfo.SetContextHelp(this.PhysicalItemDefinition);
            MyHud.BlockInfo.PCUCost = 0;
            MyHud.BlockInfo.BlockIcons = this.PhysicalItemDefinition.Icons;
            MyHud.BlockInfo.BlockIntegrity = 1f;
            MyHud.BlockInfo.CriticalIntegrity = 0f;
            MyHud.BlockInfo.CriticalComponentIndex = 0;
            MyHud.BlockInfo.OwnershipIntegrity = 0f;
            MyHud.BlockInfo.BlockBuiltBy = 0L;
            MyHud.BlockInfo.GridSize = MyCubeSize.Small;
            MyHud.BlockInfo.Components.Clear();
        }

        public void DrawHud(IMyCameraController camera, long playerId, bool fullUpdate)
        {
            this.DrawHud(camera, playerId);
        }

        public void EndShoot(MyShootActionEnum action)
        {
            if (action == MyShootActionEnum.PrimaryAction)
            {
                this.IsShooting = false;
                this.m_shotsFiredInBurst = 0;
                this.m_gunBase.StopShoot();
            }
            else if (action == MyShootActionEnum.SecondaryAction)
            {
                this.m_canZoom = true;
            }
            this.m_isActionDoubleClicked[action] = false;
        }

        public int GetAmmunitionAmount() => 
            this.m_gunBase.GetTotalAmmunitionAmount();

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_AutomaticRifle objectBuilder = (MyObjectBuilder_AutomaticRifle) base.GetObjectBuilder(copy);
            objectBuilder.SubtypeName = this.DefinitionId.SubtypeName;
            objectBuilder.GunBase = this.m_gunBase.GetObjectBuilder();
            objectBuilder.CurrentAmmo = this.CurrentAmmunition;
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            MyDefinitionId weaponDefinitionId;
            if ((objectBuilder.SubtypeName != null) && (objectBuilder.SubtypeName.Length > 0))
            {
                m_handItemDefId = new MyDefinitionId(typeof(MyObjectBuilder_AutomaticRifle), objectBuilder.SubtypeName);
            }
            MyObjectBuilder_AutomaticRifle rifle = (MyObjectBuilder_AutomaticRifle) objectBuilder;
            MyHandItemDefinition definition = MyDefinitionManager.Static.TryGetHandItemDefinition(ref m_handItemDefId);
            this.m_physicalItemDef = MyDefinitionManager.Static.GetPhysicalItemForHandItem(m_handItemDefId);
            if (this.m_physicalItemDef is MyWeaponItemDefinition)
            {
                weaponDefinitionId = (this.m_physicalItemDef as MyWeaponItemDefinition).WeaponDefinitionId;
            }
            else
            {
                weaponDefinitionId = new MyDefinitionId(typeof(MyObjectBuilder_WeaponDefinition), "AutomaticRifleGun");
            }
            this.m_gunBase.Init(rifle.GunBase, weaponDefinitionId, this);
            base.Init(objectBuilder);
            float? scale = null;
            this.Init(MyTexts.Get(MySpaceTexts.DisplayName_Rifle), this.m_physicalItemDef.Model, null, scale, null);
            MyModel modelOnlyDummies = MyModels.GetModelOnlyDummies(this.m_physicalItemDef.Model);
            this.m_gunBase.LoadDummies(modelOnlyDummies.Dummies);
            if (!this.m_gunBase.HasDummies)
            {
                Matrix localMatrix = Matrix.CreateTranslation(definition.MuzzlePosition);
                this.m_gunBase.AddMuzzleMatrix(MyAmmoType.HighSpeed, localMatrix);
            }
            this.PhysicalObject = (MyObjectBuilder_PhysicalGunObject) MyObjectBuilderSerializer.CreateNewObject(this.m_physicalItemDef.Id.TypeId, this.m_physicalItemDef.Id.SubtypeName);
            this.PhysicalObject.GunEntity = (MyObjectBuilder_EntityBase) rifle.Clone();
            this.PhysicalObject.GunEntity.EntityId = base.EntityId;
            this.CurrentAmmunition = rifle.CurrentAmmo;
        }

        public void MissileShootEffect()
        {
            this.m_gunBase.CreateEffects(MyWeaponDefinition.WeaponEffectAction.Shoot);
        }

        private void MyAutomaticRifleGun_ContentsChanged(MyInventoryBase obj)
        {
            this.m_gunBase.RefreshAmmunitionAmount();
        }

        public void OnControlAcquired(MyCharacter owner)
        {
            this.m_owner = owner;
            if (this.m_owner != null)
            {
                this.m_shootIgnoreEntities = new MyEntity[] { this, this.m_owner };
                MyInventory inventory = this.m_owner.GetInventory(0);
                if (inventory != null)
                {
                    inventory.ContentsChanged += new Action<MyInventoryBase>(this.MyAutomaticRifleGun_ContentsChanged);
                }
            }
            this.m_gunBase.RefreshAmmunitionAmount();
        }

        public void OnControlReleased()
        {
            if (this.m_owner != null)
            {
                MyInventory inventory = this.m_owner.GetInventory(0);
                if (inventory != null)
                {
                    inventory.ContentsChanged -= new Action<MyInventoryBase>(this.MyAutomaticRifleGun_ContentsChanged);
                }
            }
            this.m_owner = null;
        }

        [Event(null, 0x2c3), Reliable, Broadcast]
        private void OnRemoveMissile(long entityId)
        {
            MyMissiles.Remove(entityId);
        }

        [Event(null, 0x2b6), Reliable, Server, Broadcast]
        private void OnShootMissile(MyObjectBuilder_Missile builder)
        {
            MyMissiles.Add(builder);
        }

        private void OnSmokeEffectDelete(object sender, EventArgs eventArgs)
        {
            this.m_smokeEffect = null;
        }

        public void RemoveMissile(long entityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyAutomaticRifleGun, long>(this, x => new Action<long>(x.OnRemoveMissile), entityId, targetEndpoint);
        }

        private void Shoot(Vector3 direction, Vector3D? overrideWeaponPos)
        {
            this.m_lastTimeShoot = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if ((overrideWeaponPos != null) && (this.m_gunBase.DummiesPerType(MyAmmoType.HighSpeed) <= 1))
            {
                this.m_gunBase.Shoot(overrideWeaponPos.Value + (direction * -0.25f), this.m_owner.Physics.LinearVelocity, direction, this.m_owner);
            }
            else if (this.m_owner != null)
            {
                this.m_gunBase.ShootWithOffset(this.m_owner.Physics.LinearVelocity, direction, -0.25f, this.m_owner);
            }
            else
            {
                this.m_gunBase.ShootWithOffset(Vector3.Zero, direction, -0.25f, null);
            }
            this.m_isAfterReleaseFire = false;
            if (this.m_gunBase.ShootSound != null)
            {
                this.StartLoopSound(this.m_gunBase.ShootSound);
            }
            this.m_gunBase.ConsumeAmmo();
        }

        public void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            if (action != MyShootActionEnum.PrimaryAction)
            {
                if ((action == MyShootActionEnum.SecondaryAction) && ReferenceEquals(MySession.Static.ControlledEntity, this.m_owner))
                {
                    this.m_owner.Zoom(true, true);
                    this.m_canZoom = false;
                }
            }
            else
            {
                this.Shoot(direction, overrideWeaponPos);
                this.m_shotsFiredInBurst++;
                this.IsShooting = true;
                if (this.m_owner.ControllerInfo.IsLocallyControlled() && !MySession.Static.IsCameraUserAnySpectator())
                {
                    MySector.MainCamera.CameraShake.AddShake(RIFLE_MAX_SHAKE);
                    MySector.MainCamera.AddFovSpring(RIFLE_FOV_SHAKE);
                }
            }
        }

        public void ShootFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public void ShootMissile(MyObjectBuilder_Missile builder)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyAutomaticRifleGun, MyObjectBuilder_Missile>(this, x => new Action<MyObjectBuilder_Missile>(x.OnShootMissile), builder, targetEndpoint);
        }

        public bool ShouldEndShootOnPause(MyShootActionEnum action) => 
            true;

        public void StartLoopSound(MySoundPair cueEnum)
        {
            int num1;
            if ((this.m_owner == null) || !this.m_owner.IsInFirstPersonView)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) ReferenceEquals(this.m_owner, MySession.Static.LocalCharacter);
            }
            bool flag = (bool) num1;
            this.m_gunBase.StartShootSound(this.m_soundEmitter, flag);
            this.UpdateSoundEmitter();
        }

        public void StopLoopSound()
        {
            if (this.m_soundEmitter.Loop)
            {
                this.m_soundEmitter.StopSound(false, true);
            }
        }

        public bool SupressShootAnimation() => 
            false;

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (this.m_smokeEffect != null)
            {
                float num = 0.2f;
                this.m_smokeEffect.WorldMatrix = MatrixD.CreateTranslation(this.m_gunBase.GetMuzzleWorldPosition() + (base.PositionComp.WorldMatrix.Forward * num));
                this.m_smokeEffect.UserBirthMultiplier = 50f;
            }
            this.m_gunBase.UpdateEffects();
            if (((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastTimeShoot) > this.m_gunBase.ReleaseTimeAfterFire) && !this.m_isAfterReleaseFire)
            {
                this.StopLoopSound();
                if (this.m_smokeEffect != null)
                {
                    this.m_smokeEffect.Stop(true);
                }
                this.m_isAfterReleaseFire = true;
                this.m_gunBase.RemoveOldEffects(MyWeaponDefinition.WeaponEffectAction.Shoot);
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            this.UpdateSoundEmitter();
        }

        public void UpdateSoundEmitter()
        {
            if (this.m_soundEmitter != null)
            {
                if (this.m_owner != null)
                {
                    Vector3 zero = Vector3.Zero;
                    this.m_owner.GetLinearVelocity(ref zero, true);
                    this.m_soundEmitter.SetVelocity(new Vector3?(zero));
                }
                this.m_soundEmitter.Update();
            }
        }

        private void WorldPositionChanged(object source)
        {
            this.m_gunBase.WorldMatrix = base.WorldMatrix;
        }

        public int LastTimeShoot =>
            this.m_lastTimeShoot;

        public MyCharacter Owner =>
            this.m_owner;

        public long OwnerId =>
            ((this.m_owner == null) ? 0L : this.m_owner.EntityId);

        public long OwnerIdentityId =>
            ((this.m_owner == null) ? 0L : this.m_owner.GetPlayerIdentityId());

        public bool IsShooting { get; private set; }

        public int ShootDirectionUpdateTime =>
            200;

        public bool ForceAnimationInsteadOfIK =>
            false;

        public bool IsBlocking =>
            false;

        public MyObjectBuilder_PhysicalGunObject PhysicalObject { get; set; }

        public VRage.Sync.SyncType SyncType { get; set; }

        public bool IsSkinnable =>
            true;

        public float BackkickForcePerSecond =>
            this.m_gunBase.BackkickForcePerSecond;

        public float ShakeAmount { get; protected set; }

        public bool EnabledInWorldRules =>
            MySession.Static.WeaponsEnabled;

        public MyDefinitionId DefinitionId =>
            m_handItemDefId;

        public MyGunBase GunBase =>
            this.m_gunBase;

        MyEntity[] IMyGunBaseUser.IgnoreEntities =>
            this.m_shootIgnoreEntities;

        MyEntity IMyGunBaseUser.Weapon =>
            this;

        MyEntity IMyGunBaseUser.Owner =>
            this.m_owner;

        IMyMissileGunObject IMyGunBaseUser.Launcher =>
            this;

        MyInventory IMyGunBaseUser.AmmoInventory =>
            ((this.m_owner == null) ? null : this.m_owner.GetInventory(0));

        MyDefinitionId IMyGunBaseUser.PhysicalItemId =>
            this.m_physicalItemDef.Id;

        MyInventory IMyGunBaseUser.WeaponInventory =>
            ((this.m_owner == null) ? null : this.m_owner.GetInventory(0));

        long IMyGunBaseUser.OwnerId =>
            ((this.m_owner == null) ? 0L : this.m_owner.ControllerInfo.ControllingIdentityId);

        string IMyGunBaseUser.ConstraintDisplayName =>
            null;

        public MyPhysicalItemDefinition PhysicalItemDefinition =>
            this.m_physicalItemDef;

        public int CurrentAmmunition
        {
            get => 
                this.m_gunBase.GetTotalAmmunitionAmount();
            set => 
                (this.m_gunBase.RemainingAmmo = value);
        }

        public int CurrentMagazineAmmunition
        {
            get => 
                this.m_gunBase.CurrentAmmo;
            set => 
                (this.m_gunBase.CurrentAmmo = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAutomaticRifleGun.<>c <>9 = new MyAutomaticRifleGun.<>c();
            public static Func<MyAutomaticRifleGun, Action<MyObjectBuilder_Missile>> <>9__116_0;
            public static Func<MyAutomaticRifleGun, Action<long>> <>9__118_0;

            internal Action<long> <RemoveMissile>b__118_0(MyAutomaticRifleGun x) => 
                new Action<long>(x.OnRemoveMissile);

            internal Action<MyObjectBuilder_Missile> <ShootMissile>b__116_0(MyAutomaticRifleGun x) => 
                new Action<MyObjectBuilder_Missile>(x.OnShootMissile);
        }
    }
}

