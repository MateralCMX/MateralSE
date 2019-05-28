namespace Sandbox.Game.Weapons
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyCubeBlockType(typeof(MyObjectBuilder_SmallMissileLauncher)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMySmallMissileLauncher), typeof(Sandbox.ModAPI.Ingame.IMySmallMissileLauncher) })]
    public class MySmallMissileLauncher : MyUserControllableGun, IMyMissileGunObject, IMyGunObject<MyGunBase>, IMyInventoryOwner, IMyConveyorEndpointBlock, IMyGunBaseUser, Sandbox.ModAPI.IMySmallMissileLauncher, Sandbox.ModAPI.IMyUserControllableGun, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyUserControllableGun, Sandbox.ModAPI.Ingame.IMySmallMissileLauncher
    {
        protected int m_shotsLeftInBurst;
        protected int m_nextShootTime;
        private int m_nextNotificationTime;
        private MyHudNotification m_reloadNotification;
        private MyGunBase m_gunBase;
        private bool m_shoot;
        private Vector3 m_shootDirection;
        private int m_currentBarrel;
        private int m_lateStartRandom = MyUtils.GetRandomInt(0, 3);
        private int m_currentLateStart;
        private Vector3D m_targetLocal = Vector3.Zero;
        private VRage.Game.Entity.MyEntity[] m_shootIgnoreEntities;
        private MyHudNotification m_safezoneNotification;
        private MyMultilineConveyorEndpoint m_endpoint;
        protected VRage.Sync.Sync<bool, SyncDirection.BothWays> m_useConveyorSystem;

        public MySmallMissileLauncher()
        {
            this.m_shootIgnoreEntities = new VRage.Game.Entity.MyEntity[] { this };
            this.CreateTerminalControls();
            this.m_gunBase = new MyGunBase();
            base.m_soundEmitter = new MyEntity3DSoundEmitter(this, true, 1f);
            base.SyncType.Append(this.m_gunBase);
        }

        public bool AllowSelfPulling() => 
            false;

        public void BeginFailReaction(MyShootActionEnum action, MyGunStatusEnum status)
        {
            if ((status == MyGunStatusEnum.OutOfAmmo) && !MySession.Static.CreativeMode)
            {
                this.m_gunBase.StartNoAmmoSound(base.m_soundEmitter);
            }
            if (status == MyGunStatusEnum.SafeZoneDenied)
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
            }
        }

        public void BeginFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
            if (status == MyGunStatusEnum.SafeZoneDenied)
            {
                if (this.m_safezoneNotification == null)
                {
                    this.m_safezoneNotification = new MyHudNotification(MyCommonTexts.SafeZone_ShootingDisabled, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                }
                MyHud.Notifications.Add(this.m_safezoneNotification);
            }
        }

        public override bool CanOperate() => 
            this.CheckIsWorking();

        public override bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            status = MyGunStatusEnum.OK;
            if (action != MyShootActionEnum.PrimaryAction)
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if (!MySessionComponentSafeZones.IsActionAllowed(base.CubeGrid, MySafeZoneAction.Shooting, 0L))
            {
                status = MyGunStatusEnum.SafeZoneDenied;
                return false;
            }
            if (!this.m_gunBase.HasAmmoMagazines)
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if (this.m_nextShootTime > MySandboxGame.TotalGamePlayTimeInMilliseconds)
            {
                status = MyGunStatusEnum.Cooldown;
                return false;
            }
            if ((this.m_shotsLeftInBurst == 0) && (this.m_gunBase.ShotsInBurst > 0))
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if (!base.HasPlayerAccess(shooter))
            {
                status = MyGunStatusEnum.AccessDenied;
                return false;
            }
            if (!base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                status = MyGunStatusEnum.OutOfPower;
                return false;
            }
            if (!base.IsFunctional)
            {
                status = MyGunStatusEnum.NotFunctional;
                return false;
            }
            if (!base.Enabled)
            {
                status = MyGunStatusEnum.Disabled;
                return false;
            }
            if (MySession.Static.CreativeMode || this.m_gunBase.HasEnoughAmmunition())
            {
                return true;
            }
            status = MyGunStatusEnum.OutOfAmmo;
            return false;
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        protected override void Closing()
        {
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.StopSound(true, true);
            }
            base.Closing();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MySmallMissileLauncher>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncher> switch1 = new MyTerminalControlOnOffSwitch<MySmallMissileLauncher>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MySmallMissileLauncher> switch2 = new MyTerminalControlOnOffSwitch<MySmallMissileLauncher>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                switch2.Getter = x => x.UseConveyorSystem;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncher> local7 = switch2;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncher> local8 = switch2;
                local8.Setter = (x, v) => x.UseConveyorSystem = v;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncher> local5 = local8;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncher> local6 = local8;
                local6.Visible = x => x.CubeGrid.GridSizeEnum == MyCubeSize.Large;
                MyTerminalControlOnOffSwitch<MySmallMissileLauncher> onOff = local6;
                onOff.EnableToggleAction<MySmallMissileLauncher>();
                MyTerminalControlFactory.AddControl<MySmallMissileLauncher>(onOff);
            }
        }

        public Vector3 DirectionToTarget(Vector3D target) => 
            ((Vector3) base.WorldMatrix.Forward);

        public void DrawHud(IMyCameraController camera, long playerId)
        {
            MyGunStatusEnum enum2;
            this.CanShoot(MyShootActionEnum.PrimaryAction, playerId, out enum2);
            if ((enum2 == MyGunStatusEnum.OK) || (enum2 == MyGunStatusEnum.Cooldown))
            {
                MatrixD muzzleWorldMatrix = this.m_gunBase.GetMuzzleWorldMatrix();
                Vector3D zero = Vector3D.Zero;
                Vector3D translation = muzzleWorldMatrix.Translation;
                if (MyHudCrosshair.GetTarget(translation, muzzleWorldMatrix.Translation + (200.0 * muzzleWorldMatrix.Forward), ref zero))
                {
                    float num = (float) Vector3D.Distance(MySector.MainCamera.Position, zero);
                    MyTransparentGeometry.AddBillboardOriented(MyUserControllableGun.ID_RED_DOT, VRageMath.Vector4.One, zero, MySector.MainCamera.LeftVector, MySector.MainCamera.UpVector, num / 300f, MyBillboard.BlendTypeEnum.LDR, -1, 0f);
                }
            }
        }

        public void DrawHud(IMyCameraController camera, long playerId, bool fullUpdate)
        {
            MyGunStatusEnum enum2;
            this.CanShoot(MyShootActionEnum.PrimaryAction, playerId, out enum2);
            if ((enum2 == MyGunStatusEnum.OK) || (enum2 == MyGunStatusEnum.Cooldown))
            {
                if (fullUpdate)
                {
                    MatrixD muzzleWorldMatrix = this.m_gunBase.GetMuzzleWorldMatrix();
                    Vector3D zero = Vector3D.Zero;
                    Vector3D translation = muzzleWorldMatrix.Translation;
                    if (!MyHudCrosshair.GetTarget(translation, muzzleWorldMatrix.Translation + (200.0 * muzzleWorldMatrix.Forward), ref zero))
                    {
                        this.m_targetLocal = Vector3.Zero;
                    }
                    else
                    {
                        MatrixD xd2;
                        MatrixD.Invert(ref base.WorldMatrix, out xd2);
                        Vector3D.Transform(ref zero, ref xd2, out this.m_targetLocal);
                    }
                }
                if (!Vector3.IsZero((Vector3) this.m_targetLocal))
                {
                    Vector3D zero = Vector3.Zero;
                    MatrixD worldMatrix = base.WorldMatrix;
                    Vector3D.Transform(ref this.m_targetLocal, ref worldMatrix, out zero);
                    float num = (float) Vector3D.Distance(MySector.MainCamera.Position, zero);
                    MyTransparentGeometry.AddBillboardOriented(MyUserControllableGun.ID_RED_DOT, fullUpdate ? VRageMath.Vector4.One : new VRageMath.Vector4(0.6f, 0.6f, 0.6f, 0.6f), zero, MySector.MainCamera.LeftVector, MySector.MainCamera.UpVector, num / 300f, MyBillboard.BlendTypeEnum.LDR, -1, 0f);
                }
            }
            MyHud.BlockInfo.Visible = true;
            MyHud.BlockInfo.MissingComponentIndex = -1;
            MyHud.BlockInfo.BlockName = base.BlockDefinition.DisplayNameText;
            MyHud.BlockInfo.SetContextHelp(base.BlockDefinition);
            MyHud.BlockInfo.PCUCost = 0;
            MyHud.BlockInfo.BlockIcons = base.BlockDefinition.Icons;
            MyHud.BlockInfo.BlockIntegrity = 1f;
            MyHud.BlockInfo.CriticalIntegrity = 0f;
            MyHud.BlockInfo.CriticalComponentIndex = 0;
            MyHud.BlockInfo.OwnershipIntegrity = 0f;
            MyHud.BlockInfo.BlockBuiltBy = 0L;
            MyHud.BlockInfo.GridSize = MyCubeSize.Small;
            MyHud.BlockInfo.Components.Clear();
        }

        public void EndShoot(MyShootActionEnum action)
        {
            base.EndShoot(action);
            this.m_currentLateStart = 0;
        }

        public int GetAmmunitionAmount() => 
            this.m_gunBase.GetTotalAmmunitionAmount();

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_SmallMissileLauncher objectBuilderCubeBlock = (MyObjectBuilder_SmallMissileLauncher) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Inventory = this.GetInventory(0).GetObjectBuilder();
            objectBuilderCubeBlock.UseConveyorSystem = (bool) this.m_useConveyorSystem;
            objectBuilderCubeBlock.GunBase = this.m_gunBase.GetObjectBuilder();
            return objectBuilderCubeBlock;
        }

        public PullInformation GetPullInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.Inventory = this.GetInventory(0);
            information1.OwnerID = base.OwnerId;
            information1.Constraint = information1.Inventory.Constraint;
            return information1;
        }

        public PullInformation GetPushInformation() => 
            null;

        private Vector3 GetSmokePosition() => 
            ((Vector3) (this.m_gunBase.GetMuzzleWorldPosition() - (base.WorldMatrix.Forward * 0.5)));

        public override Vector3D GetWeaponMuzzleWorldPosition() => 
            ((this.m_gunBase == null) ? base.GetWeaponMuzzleWorldPosition() : this.m_gunBase.GetMuzzleWorldPosition());

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            MyStringHash resourceSinkGroup;
            base.SyncFlag = true;
            MyObjectBuilder_SmallMissileLauncher launcher = builder as MyObjectBuilder_SmallMissileLauncher;
            MyWeaponBlockDefinition blockDefinition = base.BlockDefinition as MyWeaponBlockDefinition;
            if ((blockDefinition != null) && (this.GetInventory(0) == null))
            {
                MyInventory inventory = new MyInventory(blockDefinition.InventoryMaxVolume, new Vector3(1.2f, 0.98f, 0.98f), MyInventoryFlags.CanReceive);
                base.Components.Add<MyInventoryBase>(inventory);
                resourceSinkGroup = blockDefinition.ResourceSinkGroup;
            }
            else
            {
                if (this.GetInventory(0) == null)
                {
                    MyInventory inventory2 = null;
                    inventory2 = (cubeGrid.GridSizeEnum != MyCubeSize.Small) ? new MyInventory(1.14f, new Vector3(1.2f, 0.98f, 0.98f), MyInventoryFlags.CanReceive) : new MyInventory(0.24f, new Vector3(1.2f, 0.45f, 0.45f), MyInventoryFlags.CanReceive);
                    base.Components.Add<MyInventory>(inventory2);
                }
                resourceSinkGroup = MyStringHash.GetOrCompute("Defense");
            }
            this.GetInventory(0);
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(resourceSinkGroup, 0.0002f, delegate {
                if (!base.Enabled || !base.IsFunctional)
                {
                    return 0f;
                }
                return base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
            });
            base.ResourceSink = component;
            base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            this.m_gunBase.Init(launcher.GunBase, base.BlockDefinition, this);
            base.Init(builder, cubeGrid);
            if (MyFakes.ENABLE_INVENTORY_FIX)
            {
                base.FixSingleInventory();
            }
            base.ResourceSink.Update();
            this.GetInventory(0).Init(launcher.Inventory);
            this.m_shotsLeftInBurst = this.m_gunBase.ShotsInBurst;
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawPowerReciever(base.ResourceSink, this));
            if (base.CubeGrid.GridSizeEnum == MyCubeSize.Large)
            {
                this.m_useConveyorSystem.SetLocalValue(launcher.UseConveyorSystem);
            }
            else
            {
                this.m_useConveyorSystem.SetLocalValue(false);
            }
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            this.LoadDummies();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_endpoint = new MyMultilineConveyorEndpoint(this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_endpoint));
        }

        public override bool IsStationary() => 
            true;

        private void LoadDummies()
        {
            MyModel modelOnlyDummies = MyModels.GetModelOnlyDummies(base.BlockDefinition.Model);
            this.m_gunBase.LoadDummies(modelOnlyDummies.Dummies);
            if (!this.m_gunBase.HasDummies)
            {
                foreach (KeyValuePair<string, MyModelDummy> pair in modelOnlyDummies.Dummies)
                {
                    if (pair.Key.ToLower().Contains("barrel"))
                    {
                        this.m_gunBase.AddMuzzleMatrix(MyAmmoType.Missile, pair.Value.Matrix);
                    }
                }
            }
        }

        private void m_ammoInventory_ContentsChanged(MyInventoryBase obj)
        {
            this.m_gunBase.RefreshAmmunitionAmount();
        }

        public void MissileShootEffect()
        {
            this.m_gunBase.CreateEffects(MyWeaponDefinition.WeaponEffectAction.Shoot);
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
        }

        public void OnControlAcquired(MyCharacter owner)
        {
            this.Controller = owner;
        }

        public void OnControlReleased()
        {
            this.Controller = null;
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), true);
            base.OnDestroy();
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            base.ResourceSink.Update();
        }

        protected override void OnInventoryComponentAdded(MyInventoryBase inventory)
        {
            base.OnInventoryComponentAdded(inventory);
            if (this.GetInventory(0) != null)
            {
                this.GetInventory(0).ContentsChanged += new Action<MyInventoryBase>(this.m_ammoInventory_ContentsChanged);
            }
        }

        protected override void OnInventoryComponentRemoved(MyInventoryBase inventory)
        {
            base.OnInventoryComponentRemoved(inventory);
            MyInventory inventory2 = inventory as MyInventory;
            if (inventory2 != null)
            {
                inventory2.ContentsChanged -= new Action<MyInventoryBase>(this.m_ammoInventory_ContentsChanged);
            }
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        [Event(null, 0x35f), Reliable, Broadcast]
        private void OnRemoveMissile(long entityId)
        {
            MyMissiles.Remove(entityId);
        }

        [Event(null, 0x354), Reliable, Server, Broadcast]
        private void OnShootMissile(MyObjectBuilder_Missile builder)
        {
            MyMissiles.Add(builder);
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        public void RemoveMissile(long entityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MySmallMissileLauncher, long>(this, x => new Action<long>(x.OnRemoveMissile), entityId, targetEndpoint);
        }

        public virtual void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            if (base.m_shootingBegun && (this.m_lateStartRandom > this.m_currentLateStart))
            {
                this.m_currentLateStart++;
            }
            else
            {
                this.m_shoot = true;
                this.m_shootDirection = direction;
                this.m_gunBase.ConsumeAmmo();
                this.m_nextShootTime = MySandboxGame.TotalGamePlayTimeInMilliseconds + this.m_gunBase.ShootIntervalInMiliseconds;
                if (this.m_gunBase.ShotsInBurst > 0)
                {
                    this.m_shotsLeftInBurst--;
                    if (this.m_shotsLeftInBurst <= 0)
                    {
                        this.m_nextShootTime = MySandboxGame.TotalGamePlayTimeInMilliseconds + this.m_gunBase.ReloadTime;
                        this.m_shotsLeftInBurst = this.m_gunBase.ShotsInBurst;
                    }
                }
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        public void ShootFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public override void ShootFromTerminal(Vector3 direction)
        {
            base.ShootFromTerminal(direction);
            Vector3D? overrideWeaponPos = null;
            this.Shoot(MyShootActionEnum.PrimaryAction, direction, overrideWeaponPos, null);
        }

        public void ShootMissile()
        {
            if (this.m_gunBase == null)
            {
                MySandboxGame.Log.WriteLine("Missile launcher barrel null");
            }
            else if ((base.Parent.Physics == null) || (base.Parent.Physics.RigidBody == null))
            {
                MySandboxGame.Log.WriteLine("Missile launcher parent physics null");
            }
            else
            {
                Vector3 linearVelocity = base.Parent.Physics.LinearVelocity;
                this.ShootMissile(linearVelocity);
            }
        }

        public void ShootMissile(MyObjectBuilder_Missile builder)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MySmallMissileLauncher, MyObjectBuilder_Missile>(this, x => new Action<MyObjectBuilder_Missile>(x.OnShootMissile), builder, targetEndpoint);
        }

        public void ShootMissile(Vector3 velocity)
        {
            this.StartSound(this.m_gunBase.ShootSound);
            if (Sync.IsServer)
            {
                this.m_gunBase.Shoot(velocity, null);
            }
        }

        private void ShowReloadNotification(int duration)
        {
            int num = MySandboxGame.TotalGamePlayTimeInMilliseconds + duration;
            if (this.m_reloadNotification != null)
            {
                int timeStep = num - this.m_nextNotificationTime;
                this.m_reloadNotification.AddAliveTime(timeStep);
                this.m_nextNotificationTime = num;
            }
            else
            {
                duration = Math.Max(0, duration - 250);
                if (duration != 0)
                {
                    this.m_reloadNotification = new MyHudNotification(MySpaceTexts.LargeMissileTurretReloadingNotification, duration, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
                    MyHud.Notifications.Add(this.m_reloadNotification);
                    this.m_nextNotificationTime = num;
                }
            }
        }

        private void StartSound(MySoundPair cueEnum)
        {
            this.m_gunBase.StartShootSound(base.m_soundEmitter, false);
        }

        public override void StopShootFromTerminal()
        {
        }

        public bool SupressShootAnimation() => 
            false;

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (this.m_shoot)
            {
                this.ShootMissile();
            }
            this.UpdateReloadNotification();
            this.m_shoot = false;
            base.NeedsUpdate &= ~MyEntityUpdateEnum.NONE;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if ((Sync.IsServer && (base.IsFunctional && (this.UseConveyorSystem && MySession.Static.SurvivalMode))) && ((this.GetInventory(0).VolumeFillFactor * MySession.Static.BlocksInventorySizeMultiplier) < 1f))
            {
                int num = (this.m_gunBase.WeaponProperties.CurrentWeaponRateOfFire / 0x24) + 1;
                MyGridConveyorSystem.ItemPullRequest(this, this.GetInventory(0), base.OwnerId, this.m_gunBase.CurrentAmmoMagazineId, new MyFixedPoint?(num), false);
            }
        }

        private void UpdateReloadNotification()
        {
            if (MySandboxGame.TotalGamePlayTimeInMilliseconds > this.m_nextNotificationTime)
            {
                this.m_reloadNotification = null;
            }
            if (!ReferenceEquals(this.Controller, MySession.Static.LocalCharacter))
            {
                if (this.m_reloadNotification != null)
                {
                    MyHud.Notifications.Remove(this.m_reloadNotification);
                    this.m_reloadNotification = null;
                }
            }
            else if ((this.m_nextShootTime > MySandboxGame.TotalGamePlayTimeInMilliseconds) && ((this.m_nextShootTime - MySandboxGame.TotalGamePlayTimeInMilliseconds) > this.m_gunBase.ShootIntervalInMiliseconds))
            {
                this.ShowReloadNotification(this.m_nextShootTime - MySandboxGame.TotalGamePlayTimeInMilliseconds);
            }
        }

        public void UpdateSoundEmitter()
        {
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.Update();
            }
        }

        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        protected override void WorldPositionChanged(object source)
        {
            base.WorldPositionChanged(source);
            this.m_gunBase.WorldMatrix = base.WorldMatrix;
        }

        public bool Zoom(bool newKeyPress) => 
            false;

        protected MyHudNotification ReloadNotification
        {
            get
            {
                if (this.m_reloadNotification == null)
                {
                    this.m_reloadNotification = new MyHudNotification(MySpaceTexts.MissileLauncherReloadingNotification, this.m_gunBase.ReloadTime - 250, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
                }
                return this.m_reloadNotification;
            }
        }

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_endpoint;

        public bool IsSkinnable =>
            false;

        public float BackkickForcePerSecond =>
            this.m_gunBase.BackkickForcePerSecond;

        public float ShakeAmount { get; protected set; }

        public bool IsControlled =>
            (this.Controller != null);

        public MyCharacter Controller { get; protected set; }

        public bool EnabledInWorldRules =>
            MySession.Static.WeaponsEnabled;

        public MyDefinitionId DefinitionId =>
            base.BlockDefinition.Id;

        public bool UseConveyorSystem
        {
            get => 
                ((bool) this.m_useConveyorSystem);
            set => 
                (this.m_useConveyorSystem.Value = value);
        }

        public bool IsShooting =>
            (this.m_nextShootTime > MySandboxGame.TotalGamePlayTimeInMilliseconds);

        public int ShootDirectionUpdateTime =>
            0;

        public MyGunBase GunBase =>
            this.m_gunBase;

        VRage.Game.Entity.MyEntity[] IMyGunBaseUser.IgnoreEntities =>
            this.m_shootIgnoreEntities;

        VRage.Game.Entity.MyEntity IMyGunBaseUser.Weapon =>
            base.Parent;

        VRage.Game.Entity.MyEntity IMyGunBaseUser.Owner =>
            base.Parent;

        IMyMissileGunObject IMyGunBaseUser.Launcher =>
            this;

        MyInventory IMyGunBaseUser.AmmoInventory =>
            this.GetInventory(0);

        MyDefinitionId IMyGunBaseUser.PhysicalItemId =>
            new MyDefinitionId();

        MyInventory IMyGunBaseUser.WeaponInventory =>
            null;

        long IMyGunBaseUser.OwnerId =>
            base.OwnerId;

        string IMyGunBaseUser.ConstraintDisplayName =>
            base.BlockDefinition.DisplayNameText;

        bool Sandbox.ModAPI.Ingame.IMySmallMissileLauncher.UseConveyorSystem =>
            ((bool) this.m_useConveyorSystem);

        int IMyInventoryOwner.InventoryCount =>
            base.InventoryCount;

        long IMyInventoryOwner.EntityId =>
            base.EntityId;

        bool IMyInventoryOwner.HasInventory =>
            base.HasInventory;

        bool IMyInventoryOwner.UseConveyorSystem
        {
            get => 
                this.UseConveyorSystem;
            set => 
                (this.UseConveyorSystem = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySmallMissileLauncher.<>c <>9 = new MySmallMissileLauncher.<>c();
            public static MyTerminalValueControl<MySmallMissileLauncher, bool>.GetterDelegate <>9__25_0;
            public static MyTerminalValueControl<MySmallMissileLauncher, bool>.SetterDelegate <>9__25_1;
            public static Func<MySmallMissileLauncher, bool> <>9__25_2;
            public static Func<MySmallMissileLauncher, Action<MyObjectBuilder_Missile>> <>9__113_0;
            public static Func<MySmallMissileLauncher, Action<long>> <>9__115_0;

            internal bool <CreateTerminalControls>b__25_0(MySmallMissileLauncher x) => 
                x.UseConveyorSystem;

            internal void <CreateTerminalControls>b__25_1(MySmallMissileLauncher x, bool v)
            {
                x.UseConveyorSystem = v;
            }

            internal bool <CreateTerminalControls>b__25_2(MySmallMissileLauncher x) => 
                (x.CubeGrid.GridSizeEnum == MyCubeSize.Large);

            internal Action<long> <RemoveMissile>b__115_0(MySmallMissileLauncher x) => 
                new Action<long>(x.OnRemoveMissile);

            internal Action<MyObjectBuilder_Missile> <ShootMissile>b__113_0(MySmallMissileLauncher x) => 
                new Action<MyObjectBuilder_Missile>(x.OnShootMissile);
        }
    }
}

