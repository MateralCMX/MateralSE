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
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
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

    [MyCubeBlockType(typeof(MyObjectBuilder_SmallGatlingGun)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMySmallGatlingGun), typeof(Sandbox.ModAPI.Ingame.IMySmallGatlingGun) })]
    public class MySmallGatlingGun : MyUserControllableGun, IMyGunObject<MyGunBase>, IMyInventoryOwner, IMyConveyorEndpointBlock, IMyGunBaseUser, Sandbox.ModAPI.IMySmallGatlingGun, Sandbox.ModAPI.IMyUserControllableGun, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyUserControllableGun, Sandbox.ModAPI.Ingame.IMySmallGatlingGun, IMyMissileGunObject
    {
        public const int SMOKE_OVERTIME_LENGTH = 120;
        private int m_lastTimeShoot;
        private float m_rotationTimeout;
        private bool m_cannonMotorEndPlayed;
        private ShootStateEnum currentState;
        private int m_shootOvertime;
        private int m_smokeOvertime;
        private readonly VRage.Sync.Sync<int, SyncDirection.FromServer> m_lateStartRandom;
        private int m_currentLateStart;
        private float m_muzzleFlashLength;
        private float m_muzzleFlashRadius;
        private int m_smokesToGenerate;
        private MyEntity3DSoundEmitter m_soundEmitterRotor;
        private VRage.Game.Entity.MyEntity m_barrel;
        private MyParticleEffect m_smokeEffect;
        private MyParticleEffect m_flashEffect;
        private MyGunBase m_gunBase;
        private Vector3D m_targetLocal = Vector3.Zero;
        private List<HkWorld.HitInfo> m_hits = new List<HkWorld.HitInfo>();
        private MyHudNotification m_safezoneNotification;
        private MyMultilineConveyorEndpoint m_conveyorEndpoint;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_useConveyorSystem;
        private VRage.Game.Entity.MyEntity[] m_shootIgnoreEntities;

        public MySmallGatlingGun()
        {
            this.m_shootIgnoreEntities = new VRage.Game.Entity.MyEntity[] { this };
            this.CreateTerminalControls();
            this.m_lastTimeShoot = -60000;
            this.m_smokesToGenerate = 0;
            this.m_cannonMotorEndPlayed = true;
            this.m_rotationTimeout = 2000f + MyUtils.GetRandomFloat(-500f, 500f);
            base.m_soundEmitter = new MyEntity3DSoundEmitter(this, true, 1f);
            this.m_gunBase = new MyGunBase();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            base.Render = new MyRenderComponentSmallGatlingGun();
            base.AddDebugRenderComponent(new MyDebugRenderComponentSmallGatlingGun(this));
            base.SyncType.Append(this.m_gunBase);
        }

        public bool AllowSelfPulling() => 
            false;

        private void AmmoInventory_ContentsChanged(MyInventoryBase obj)
        {
            this.m_gunBase.RefreshAmmunitionAmount();
        }

        public void BeginFailReaction(MyShootActionEnum action, MyGunStatusEnum status)
        {
            if (((status == MyGunStatusEnum.OutOfAmmo) && !MySession.Static.CreativeMode) && (this.GetInventory(0).GetItemAmount(this.m_gunBase.CurrentAmmoMagazineId, MyItemFlags.None, false) < 1))
            {
                this.StartNoAmmoSound();
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

        public override void BeginShoot(MyShootActionEnum action)
        {
            this.currentState = ShootStateEnum.Continuous;
            base.BeginShoot(action);
            this.StartLoopSound();
        }

        public override bool CanOperate() => 
            this.CheckIsWorking();

        public override bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            status = MyGunStatusEnum.OK;
            if (!MySessionComponentSafeZones.IsActionAllowed(base.CubeGrid, MySafeZoneAction.Shooting, 0L))
            {
                status = MyGunStatusEnum.SafeZoneDenied;
                return false;
            }
            if (action != MyShootActionEnum.PrimaryAction)
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if ((base.Parent == null) || (base.Parent.Physics == null))
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if (!this.m_gunBase.HasAmmoMagazines)
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastTimeShoot) < this.m_gunBase.ShootIntervalInMiliseconds)
            {
                status = MyGunStatusEnum.Cooldown;
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
            if (!MySession.Static.CreativeMode && !this.m_gunBase.HasEnoughAmmunition())
            {
                status = MyGunStatusEnum.OutOfAmmo;
                return false;
            }
            if (this.m_barrel != null)
            {
                return true;
            }
            status = MyGunStatusEnum.Failed;
            return false;
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        private void ClampSmokesToGenerate()
        {
            this.m_smokesToGenerate = MyUtils.GetClampInt(this.m_smokesToGenerate, 0, 50);
        }

        protected override void Closing()
        {
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.StopSound(true, true);
            }
            if (this.m_soundEmitterRotor != null)
            {
                this.m_soundEmitterRotor.StopSound(true, true);
            }
            if (this.m_smokeEffect != null)
            {
                this.m_smokeEffect.Stop(false);
                this.m_smokeEffect = null;
            }
            if (this.m_flashEffect != null)
            {
                this.m_flashEffect.Stop(true);
                this.m_flashEffect = null;
            }
            base.Closing();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MySmallGatlingGun>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MySmallGatlingGun> switch1 = new MyTerminalControlOnOffSwitch<MySmallGatlingGun>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MySmallGatlingGun> switch2 = new MyTerminalControlOnOffSwitch<MySmallGatlingGun>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                switch2.Getter = x => x.UseConveyorSystem;
                MyTerminalControlOnOffSwitch<MySmallGatlingGun> local4 = switch2;
                MyTerminalControlOnOffSwitch<MySmallGatlingGun> local5 = switch2;
                local5.Setter = (x, v) => x.UseConveyorSystem = v;
                MyTerminalControlOnOffSwitch<MySmallGatlingGun> onOff = local5;
                onOff.EnableToggleAction<MySmallGatlingGun>();
                MyTerminalControlFactory.AddControl<MySmallGatlingGun>(onOff);
            }
        }

        public Vector3 DirectionToTarget(Vector3D target) => 
            ((Vector3) base.PositionComp.WorldMatrix.Forward);

        public void DrawHud(IMyCameraController camera, long playerId)
        {
            MyGunStatusEnum enum2;
            this.CanShoot(MyShootActionEnum.PrimaryAction, playerId, out enum2);
            if ((enum2 == MyGunStatusEnum.OK) || (enum2 == MyGunStatusEnum.Cooldown))
            {
                Vector3D zero = Vector3D.Zero;
                if (MyHudCrosshair.GetTarget(base.PositionComp.GetPosition() + base.PositionComp.WorldMatrix.Forward, base.PositionComp.GetPosition() + (200.0 * base.PositionComp.WorldMatrix.Forward), ref zero))
                {
                    float num = (float) Vector3D.Distance(MySector.MainCamera.Position, zero);
                    MyTransparentGeometry.AddBillboardOriented(MyUserControllableGun.ID_RED_DOT, new VRageMath.Vector4(1f, 1f, 1f, 1f), zero, MySector.MainCamera.LeftVector, MySector.MainCamera.UpVector, num / 300f, MyBillboard.BlendTypeEnum.LDR, -1, 0f);
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
                    Vector3D zero = Vector3D.Zero;
                    if (!MyHudCrosshair.GetTarget(base.PositionComp.GetPosition() + base.PositionComp.WorldMatrix.Forward, base.PositionComp.GetPosition() + (200.0 * base.PositionComp.WorldMatrix.Forward), ref zero))
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

        public override void EndShoot(MyShootActionEnum action)
        {
            this.currentState = ShootStateEnum.Off;
            base.EndShoot(action);
            this.m_currentLateStart = 0;
            this.StopLoopSound();
            if (this.m_flashEffect != null)
            {
                this.m_flashEffect.Stop(true);
                this.m_flashEffect = null;
            }
        }

        public int GetAmmunitionAmount() => 
            this.m_gunBase.GetTotalAmmunitionAmount();

        private void GetBarrelAndMuzzle()
        {
            MyEntitySubpart subpart;
            if (base.Subparts.TryGetValue("Barrel", out subpart))
            {
                this.m_barrel = subpart;
                MyModel model = this.m_barrel.Model;
                this.m_gunBase.LoadDummies(model.Dummies);
                if (!this.m_gunBase.HasDummies)
                {
                    if (model.Dummies.ContainsKey("Muzzle"))
                    {
                        this.m_gunBase.AddMuzzleMatrix(MyAmmoType.HighSpeed, model.Dummies["Muzzle"].Matrix);
                    }
                    else
                    {
                        Matrix localMatrix = Matrix.CreateTranslation(new Vector3(0f, 0f, -1f));
                        this.m_gunBase.AddMuzzleMatrix(MyAmmoType.HighSpeed, localMatrix);
                    }
                }
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_SmallGatlingGun objectBuilderCubeBlock = (MyObjectBuilder_SmallGatlingGun) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Inventory = this.GetInventory(0).GetObjectBuilder();
            objectBuilderCubeBlock.GunBase = this.m_gunBase.GetObjectBuilder();
            objectBuilderCubeBlock.UseConveyorSystem = (bool) this.m_useConveyorSystem;
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

        public override Vector3D GetWeaponMuzzleWorldPosition() => 
            ((this.m_gunBase == null) ? base.GetWeaponMuzzleWorldPosition() : this.m_gunBase.GetMuzzleWorldPosition());

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            MyObjectBuilder_SmallGatlingGun gun = objectBuilder as MyObjectBuilder_SmallGatlingGun;
            MyWeaponBlockDefinition blockDefinition = base.BlockDefinition as MyWeaponBlockDefinition;
            if (MyFakes.ENABLE_INVENTORY_FIX)
            {
                base.FixSingleInventory();
            }
            this.m_soundEmitterRotor = new MyEntity3DSoundEmitter(this, false, 1f);
            if (this.GetInventory(0) == null)
            {
                MyInventory inventory = (blockDefinition == null) ? new MyInventory(0.064f, new Vector3(0.4f, 0.4f, 0.4f), MyInventoryFlags.CanReceive) : new MyInventory(blockDefinition.InventoryMaxVolume, new Vector3(0.4f, 0.4f, 0.4f), MyInventoryFlags.CanReceive);
                base.Components.Add<MyInventoryBase>(inventory);
                inventory.Init(gun.Inventory);
            }
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(blockDefinition.ResourceSinkGroup, 0.0002f, () => base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId));
            component.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink = component;
            this.m_gunBase.Init(gun.GunBase, base.BlockDefinition, this);
            base.Init(objectBuilder, cubeGrid);
            if (Sync.IsServer)
            {
                this.m_lateStartRandom.Value = MyUtils.GetRandomInt(0, 30);
            }
            base.ResourceSink.Update();
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawPowerReciever(base.ResourceSink, this));
            this.m_useConveyorSystem.SetLocalValue(gun.UseConveyorSystem);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MySmallGatlingGun_IsWorkingChanged);
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_conveyorEndpoint));
        }

        public override bool IsStationary() => 
            true;

        public void MissileShootEffect()
        {
            this.m_gunBase.CreateEffects(MyWeaponDefinition.WeaponEffectAction.Shoot);
        }

        private void MySmallGatlingGun_IsWorkingChanged(MyCubeBlock obj)
        {
            if (!base.IsWorking)
            {
                this.StopLoopSound();
            }
            else if (this.currentState == ShootStateEnum.Continuous)
            {
                this.StartLoopSound();
            }
        }

        public void OnControlAcquired(MyCharacter owner)
        {
        }

        public void OnControlReleased()
        {
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), true);
            base.OnDestroy();
        }

        protected override void OnInventoryComponentAdded(MyInventoryBase inventory)
        {
            base.OnInventoryComponentAdded(inventory);
            if (this.GetInventory(0) != null)
            {
                this.GetInventory(0).ContentsChanged += new Action<MyInventoryBase>(this.AmmoInventory_ContentsChanged);
            }
        }

        protected override void OnInventoryComponentRemoved(MyInventoryBase inventory)
        {
            base.OnInventoryComponentRemoved(inventory);
            MyInventory inventory2 = inventory as MyInventory;
            if (inventory2 != null)
            {
                inventory2.ContentsChanged -= new Action<MyInventoryBase>(this.AmmoInventory_ContentsChanged);
            }
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            if (base.IsBuilt)
            {
                this.GetBarrelAndMuzzle();
            }
            else
            {
                this.m_barrel = null;
            }
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        [Event(null, 0x433), Reliable, Broadcast]
        private void OnRemoveMissile(long entityId)
        {
            MyMissiles.Remove(entityId);
        }

        [Event(null, 0x426), Reliable, Server, Broadcast]
        private void OnShootMissile(MyObjectBuilder_Missile builder)
        {
            MyMissiles.Add(builder);
        }

        private void PlayShotSound()
        {
            this.m_gunBase.StartShootSound(base.m_soundEmitter, false);
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        public void RemoveMissile(long entityId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MySmallGatlingGun, long>(this, x => new Action<long>(x.OnRemoveMissile), entityId, targetEndpoint);
        }

        public void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            if (base.Parent.Physics != null)
            {
                if ((base.m_shootingBegun && (this.m_lateStartRandom > this.m_currentLateStart)) && (this.currentState == ShootStateEnum.Continuous))
                {
                    this.m_currentLateStart++;
                }
                else
                {
                    if (this.currentState == ShootStateEnum.Off)
                    {
                        this.currentState = ShootStateEnum.Once;
                        base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                    }
                    this.m_muzzleFlashLength = MyUtils.GetRandomFloat(3f, 4f) * base.CubeGrid.GridSize;
                    this.m_muzzleFlashRadius = MyUtils.GetRandomFloat(0.9f, 1.5f) * base.CubeGrid.GridSize;
                    base.Render.NeedsDrawFromParent = true;
                    this.SmokesToGenerateIncrease();
                    this.PlayShotSound();
                    this.m_gunBase.Shoot(base.Parent.Physics.LinearVelocity, null);
                    this.m_gunBase.ConsumeAmmo();
                    if ((this.BackkickForcePerSecond > 0f) && !base.CubeGrid.Physics.IsStatic)
                    {
                        Vector3? torque = null;
                        float? maxSpeed = null;
                        base.CubeGrid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, new Vector3?(-direction * this.BackkickForcePerSecond), new Vector3D?(base.PositionComp.GetPosition()), torque, maxSpeed, true, false);
                    }
                    this.m_cannonMotorEndPlayed = false;
                    this.m_lastTimeShoot = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                }
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

        public void ShootMissile(MyObjectBuilder_Missile builder)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MySmallGatlingGun, MyObjectBuilder_Missile>(this, x => new Action<MyObjectBuilder_Missile>(x.OnShootMissile), builder, targetEndpoint);
        }

        private void SmokesToGenerateDecrease()
        {
            this.m_smokesToGenerate--;
            this.ClampSmokesToGenerate();
        }

        private void SmokesToGenerateIncrease()
        {
            this.m_smokesToGenerate += 0x13;
            this.ClampSmokesToGenerate();
        }

        private void StartLoopSound()
        {
            if (((this.m_soundEmitterRotor != null) && (!ReferenceEquals(this.m_gunBase.SecondarySound, MySoundPair.Empty) && (!this.m_soundEmitterRotor.IsPlaying || !this.m_soundEmitterRotor.Loop))) && base.IsWorking)
            {
                if (this.m_soundEmitterRotor.IsPlaying)
                {
                    this.m_soundEmitterRotor.StopSound(true, true);
                }
                bool? nullable = null;
                this.m_soundEmitterRotor.PlaySound(this.m_gunBase.SecondarySound, true, false, false, false, false, nullable);
            }
        }

        public void StartNoAmmoSound()
        {
            this.m_gunBase.StartNoAmmoSound(base.m_soundEmitter);
        }

        private void StopLoopSound()
        {
            if (((base.m_soundEmitter != null) && base.m_soundEmitter.IsPlaying) && base.m_soundEmitter.Loop)
            {
                base.m_soundEmitter.StopSound(true, true);
            }
            if (((this.m_soundEmitterRotor != null) && this.m_soundEmitterRotor.IsPlaying) && this.m_soundEmitterRotor.Loop)
            {
                this.m_soundEmitterRotor.StopSound(true, true);
                bool? nullable = null;
                this.m_soundEmitterRotor.PlaySound(this.m_gunBase.SecondarySound, false, false, false, false, true, nullable);
            }
        }

        public override void StopShootFromTerminal()
        {
        }

        public bool SupressShootAnimation() => 
            false;

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (base.PositionComp != null)
            {
                bool flag = ReferenceEquals(this.m_flashEffect, null);
                if (flag == (this.currentState != ShootStateEnum.Off))
                {
                    if (flag)
                    {
                        MyParticlesManager.TryCreateParticleEffect("Muzzle_Flash_Large", base.PositionComp.WorldMatrix, out this.m_flashEffect);
                        if (this.currentState == ShootStateEnum.Once)
                        {
                            this.m_smokesToGenerate = 10;
                            this.m_shootOvertime = 5;
                            this.currentState = ShootStateEnum.Off;
                        }
                    }
                    else if (this.m_shootOvertime > 0)
                    {
                        this.m_shootOvertime--;
                    }
                    else if (this.m_flashEffect != null)
                    {
                        this.m_flashEffect.Stop(true);
                        this.m_flashEffect = null;
                    }
                }
                float radians = (MathHelper.SmoothStep((float) 0f, (float) 1f, (float) (1f - MathHelper.Clamp((float) (((float) (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastTimeShoot)) / this.m_rotationTimeout), (float) 0f, (float) 1f))) * 12.56637f) * 0.01666667f;
                if (((radians != 0f) && (this.m_barrel != null)) && (this.m_barrel.PositionComp != null))
                {
                    this.m_barrel.PositionComp.LocalMatrix = Matrix.CreateRotationZ(radians) * this.m_barrel.PositionComp.LocalMatrix;
                }
                if (((radians == 0f) && (!base.HasDamageEffect && (this.m_smokeOvertime <= 0))) && (this.currentState == ShootStateEnum.Off))
                {
                    base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                }
                this.SmokesToGenerateDecrease();
                if (this.m_smokesToGenerate <= 0)
                {
                    this.m_smokeOvertime--;
                    if ((this.m_smokeEffect != null) && !this.m_smokeEffect.IsEmittingStopped)
                    {
                        this.m_smokeEffect.StopEmitting(0f);
                    }
                    if (this.m_flashEffect != null)
                    {
                        this.m_flashEffect.Stop(true);
                        this.m_flashEffect = null;
                    }
                }
                else
                {
                    this.m_smokeOvertime = 120;
                    if (MySector.MainCamera.GetDistanceFromPoint(base.PositionComp.GetPosition()) < 150.0)
                    {
                        if (this.m_smokeEffect == null)
                        {
                            MyParticlesManager.TryCreateParticleEffect("Smoke_Autocannon", base.PositionComp.WorldMatrix, out this.m_smokeEffect);
                        }
                        else if (this.m_smokeEffect.IsEmittingStopped)
                        {
                            this.m_smokeEffect.Play();
                            this.m_smokeEffect.WorldMatrix = base.PositionComp.WorldMatrix;
                        }
                    }
                }
                if (this.m_smokeEffect != null)
                {
                    this.m_smokeEffect.WorldMatrix = MatrixD.CreateTranslation(this.GetWeaponMuzzleWorldPosition());
                    this.m_smokeEffect.UserBirthMultiplier = (this.m_smokesToGenerate / 10) * 10;
                }
                if (this.m_flashEffect != null)
                {
                    this.m_flashEffect.WorldMatrix = base.PositionComp.WorldMatrix;
                    this.m_flashEffect.SetTranslation(this.GetWeaponMuzzleWorldPosition());
                }
            }
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if ((MySession.Static.SurvivalMode && (Sync.IsServer && (base.IsWorking && (this.m_useConveyorSystem != null)))) && (this.GetInventory(0).VolumeFillFactor < 0.6f))
            {
                MyAmmoMagazineDefinition currentAmmoMagazineDefinition = this.m_gunBase.CurrentAmmoMagazineDefinition;
                if (currentAmmoMagazineDefinition != null)
                {
                    MyFixedPoint point = MyFixedPoint.Floor((this.GetInventory(0).MaxVolume - this.GetInventory(0).CurrentVolume) * (1f / currentAmmoMagazineDefinition.Volume));
                    if (point != 0)
                    {
                        MyGridConveyorSystem.ItemPullRequest(this, this.GetInventory(0), base.OwnerId, this.m_gunBase.CurrentAmmoMagazineId, new MyFixedPoint?(point), false);
                    }
                }
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
        }

        private void UpdatePower()
        {
            base.ResourceSink.Update();
            if (!base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                this.EndShoot(MyShootActionEnum.PrimaryAction);
            }
        }

        public void UpdateSoundEmitter()
        {
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.Update();
            }
        }

        public override void UpdateVisual()
        {
            MyEntitySubpart subpart;
            base.UpdateVisual();
            if (base.Subparts.TryGetValue("Barrel", out subpart))
            {
                this.m_barrel = subpart;
            }
        }

        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        protected override void WorldPositionChanged(object source)
        {
            base.WorldPositionChanged(source);
            if (this.m_barrel != null)
            {
                this.m_gunBase.WorldMatrix = this.m_barrel.PositionComp.WorldMatrix;
            }
        }

        public int LastTimeShoot =>
            this.m_lastTimeShoot;

        public int LateStartRandom =>
            this.m_lateStartRandom.Value;

        public float MuzzleFlashLength =>
            this.m_muzzleFlashLength;

        public float MuzzleFlashRadius =>
            this.m_muzzleFlashRadius;

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        public bool IsSkinnable =>
            false;

        public float BackkickForcePerSecond =>
            this.m_gunBase.BackkickForcePerSecond;

        public float ShakeAmount { get; protected set; }

        public float ProjectileCountMultiplier =>
            0f;

        public bool EnabledInWorldRules =>
            MySession.Static.WeaponsEnabled;

        public MyDefinitionId DefinitionId =>
            base.BlockDefinition.Id;

        public bool IsShooting =>
            ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastTimeShoot) < (this.m_gunBase.ShootIntervalInMiliseconds * 2));

        public int ShootDirectionUpdateTime =>
            0;

        private bool UseConveyorSystem
        {
            get => 
                ((bool) this.m_useConveyorSystem);
            set => 
                (this.m_useConveyorSystem.Value = value);
        }

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

        bool Sandbox.ModAPI.Ingame.IMySmallGatlingGun.UseConveyorSystem =>
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
            public static readonly MySmallGatlingGun.<>c <>9 = new MySmallGatlingGun.<>c();
            public static MyTerminalValueControl<MySmallGatlingGun, bool>.GetterDelegate <>9__40_0;
            public static MyTerminalValueControl<MySmallGatlingGun, bool>.SetterDelegate <>9__40_1;
            public static Func<MySmallGatlingGun, Action<MyObjectBuilder_Missile>> <>9__139_0;
            public static Func<MySmallGatlingGun, Action<long>> <>9__141_0;

            internal bool <CreateTerminalControls>b__40_0(MySmallGatlingGun x) => 
                x.UseConveyorSystem;

            internal void <CreateTerminalControls>b__40_1(MySmallGatlingGun x, bool v)
            {
                x.UseConveyorSystem = v;
            }

            internal Action<long> <RemoveMissile>b__141_0(MySmallGatlingGun x) => 
                new Action<long>(x.OnRemoveMissile);

            internal Action<MyObjectBuilder_Missile> <ShootMissile>b__139_0(MySmallGatlingGun x) => 
                new Action<MyObjectBuilder_Missile>(x.OnShootMissile);
        }
    }
}

