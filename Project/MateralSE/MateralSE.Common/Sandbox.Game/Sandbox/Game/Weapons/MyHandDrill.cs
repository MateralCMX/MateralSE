namespace Sandbox.Game.Weapons
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Utils;
    using Sandbox.Game.Weapons.Guns;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.ModAPI.Weapons;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_HandDrill), true)]
    public class MyHandDrill : VRage.Game.Entity.MyEntity, IMyHandheldGunObject<MyToolBase>, IMyGunObject<MyToolBase>, IMyGunBaseUser, IMyHandDrill, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity
    {
        private const float SPIKE_THRUST_DISTANCE_HALF = 0.03f;
        private const float SPIKE_THRUST_PERIOD_IN_SECONDS = 0.06f;
        private const float SPIKE_SLOWDOWN_TIME_IN_SECONDS = 0.5f;
        private const float SPIKE_MAX_ROTATION_SPEED = -25f;
        private int m_lastTimeDrilled;
        private MyDrillBase m_drillBase;
        private MyCharacter m_owner;
        private MyDefinitionId m_handItemDefId;
        private MyStringHash m_drillMat;
        private MyEntitySubpart m_spike;
        private Vector3 m_spikeBasePos;
        private float m_spikeRotationAngle;
        private float m_spikeThrustPosition;
        private int m_spikeLastUpdateTime;
        private MyOreDetectorComponent m_oreDetectorBase = new MyOreDetectorComponent();
        private VRage.Game.Entity.MyEntity[] m_shootIgnoreEntities;
        protected Dictionary<MyShootActionEnum, bool> m_isActionDoubleClicked = new Dictionary<MyShootActionEnum, bool>();
        private float m_speedMultiplier = 1f;
        private MyHudNotification m_safezoneNotification;
        private MyResourceSinkComponent m_sinkComp;
        private MyPhysicalItemDefinition m_physItemDef;
        private static MyDefinitionId m_physicalItemId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), "HandDrillItem");
        private bool m_tryingToDrill;
        private bool m_firstTimeHeatup = true;
        private bool m_objectInDrillingRange;

        public MyHandDrill()
        {
            this.m_shootIgnoreEntities = new VRage.Game.Entity.MyEntity[] { this };
            base.NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
        }

        public void BeginFailReaction(MyShootActionEnum action, MyGunStatusEnum status)
        {
            if (ReferenceEquals(this.Owner, MySession.Static.LocalCharacter))
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
                    this.m_safezoneNotification = new MyHudNotification(MyCommonTexts.SafeZone_DrillingDisabled, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                }
                MyHud.Notifications.Add(this.m_safezoneNotification);
            }
        }

        public void BeginShoot(MyShootActionEnum action)
        {
        }

        public bool CanDoubleClickToStick(MyShootActionEnum action) => 
            true;

        public bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastTimeDrilled) < (1000f * this.m_speedMultiplier))
            {
                status = MyGunStatusEnum.Cooldown;
                this.m_firstTimeHeatup = false;
                return false;
            }
            if (this.Owner == null)
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if (!MySessionComponentSafeZones.IsActionAllowed(this.Owner, MySafeZoneAction.Drilling, 0L))
            {
                status = MyGunStatusEnum.SafeZoneDenied;
                return false;
            }
            status = MyGunStatusEnum.OK;
            return true;
        }

        protected override void Closing()
        {
            base.Closing();
            this.m_drillBase.Close();
        }

        private Vector3 ComputeDrillSensorCenter() => 
            ((Vector3) ((base.PositionComp.WorldMatrix.Forward * 1.2999999523162842) + base.PositionComp.WorldMatrix.Translation));

        private void CreateCollisionSparks()
        {
            Vector3D center = this.m_drillBase.Sensor.Center;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            if (this.m_drillBase.DrilledEntity != null)
            {
                flag = this.m_drillBase.DrilledEntity is MyCubeGrid;
                flag2 = this.m_drillBase.DrilledEntity is MyVoxelBase;
                flag3 = this.m_drillBase.DrilledEntity is MyEnvironmentSector;
                if (flag)
                {
                    Vector3D position = Vector3D.Transform(this.m_drillBase.ParticleOffset, base.WorldMatrix);
                    if (this.m_drillBase.SparkEffect == null)
                    {
                        MyParticlesManager.TryCreateParticleEffect("Collision_Sparks_HandDrill", MatrixD.CreateWorld(position, base.PositionComp.WorldMatrix.Forward, base.PositionComp.WorldMatrix.Up), out this.m_drillBase.SparkEffect);
                    }
                    else
                    {
                        if (this.m_drillBase.SparkEffect.IsEmittingStopped)
                        {
                            this.m_drillBase.SparkEffect.Play();
                        }
                        this.m_drillBase.SparkEffect.WorldMatrix = MatrixD.CreateWorld(position, base.PositionComp.WorldMatrix.Forward, base.PositionComp.WorldMatrix.Up);
                    }
                }
                if (flag2)
                {
                    Vector3D position = Vector3D.Transform(this.m_drillBase.ParticleOffset, base.WorldMatrix);
                    string str = MyMaterialPropertiesHelper.Static.GetCollisionEffect(MyMaterialPropertiesHelper.CollisionType.Start, this.m_drillMat, (this.m_drillBase.DrilledEntity as MyVoxelBase).Physics.GetMaterialAt(this.m_drillBase.DrilledEntityPoint));
                    this.m_drillBase.CurrentDustEffectName = !string.IsNullOrEmpty(str) ? str : "Smoke_HandDrillDustStones";
                    if (((this.m_drillBase.DustParticles == null) || (this.m_drillBase.DustParticles.GetName() != this.m_drillBase.CurrentDustEffectName)) && (this.m_drillBase.DustParticles != null))
                    {
                        this.m_drillBase.DustParticles.Stop(false);
                        this.m_drillBase.DustParticles = null;
                    }
                    if (this.m_drillBase.DustParticles == null)
                    {
                        MyParticlesManager.TryCreateParticleEffect(this.m_drillBase.CurrentDustEffectName, MatrixD.CreateWorld(position, base.PositionComp.WorldMatrix.Forward, base.PositionComp.WorldMatrix.Up), out this.m_drillBase.DustParticles);
                    }
                    else
                    {
                        if (this.m_drillBase.DustParticles.IsEmittingStopped)
                        {
                            this.m_drillBase.DustParticles.Play();
                        }
                        this.m_drillBase.DustParticles.WorldMatrix = MatrixD.CreateWorld(position, base.PositionComp.WorldMatrix.Forward, base.PositionComp.WorldMatrix.Up);
                    }
                }
                if (flag3)
                {
                    Vector3D position = Vector3D.Transform(this.m_drillBase.ParticleOffset, base.WorldMatrix);
                    this.m_drillBase.CurrentDustEffectName = "Tree_Drill";
                    if ((this.m_drillBase.CurrentDustEffectName != this.m_drillBase.CurrentDustEffectName) && (this.m_drillBase.DustParticles != null))
                    {
                        this.m_drillBase.DustParticles.Stop(false);
                        this.m_drillBase.DustParticles = null;
                    }
                    if (this.m_drillBase.DustParticles == null)
                    {
                        MyParticlesManager.TryCreateParticleEffect(this.m_drillBase.CurrentDustEffectName, MatrixD.CreateWorld(position, base.PositionComp.WorldMatrix.Forward, base.PositionComp.WorldMatrix.Up), out this.m_drillBase.DustParticles);
                    }
                    else
                    {
                        if (this.m_drillBase.DustParticles.IsEmittingStopped)
                        {
                            this.m_drillBase.DustParticles.Play();
                        }
                        this.m_drillBase.DustParticles.WorldMatrix = MatrixD.CreateWorld(position, base.PositionComp.WorldMatrix.Forward, base.PositionComp.WorldMatrix.Up);
                    }
                }
            }
            if ((this.m_drillBase.SparkEffect != null) && !flag)
            {
                this.m_drillBase.SparkEffect.StopEmitting(0f);
            }
            if (((this.m_drillBase.DustParticles != null) && !flag2) && !flag3)
            {
                this.m_drillBase.DustParticles.StopEmitting(0f);
            }
        }

        public Vector3 DirectionToTarget(Vector3D target) => 
            ((Vector3) base.PositionComp.WorldMatrix.Forward);

        private bool DoDrillAction(bool collectOre)
        {
            this.m_tryingToDrill = true;
            this.SinkComp.Update();
            if (!this.SinkComp.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                this.m_tryingToDrill = false;
                return false;
            }
            this.m_lastTimeDrilled = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            float speedMultiplier = this.m_speedMultiplier;
            this.m_objectInDrillingRange = this.m_drillBase.Drill(collectOre, !this.m_firstTimeHeatup, true, speedMultiplier);
            this.m_spikeLastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            return true;
        }

        public void DoubleClicked(MyShootActionEnum action)
        {
            this.m_isActionDoubleClicked[action] = true;
        }

        public void DrawHud(IMyCameraController camera, long playerId)
        {
            MyHud.Crosshair.Recenter();
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
            MyAnalyticsHelper.ReportActivityEnd(this.Owner, "Drilling");
            this.m_drillBase.StopDrill();
            this.m_tryingToDrill = false;
            this.m_firstTimeHeatup = true;
            this.m_objectInDrillingRange = false;
            this.SinkComp.Update();
            this.m_isActionDoubleClicked[action] = false;
        }

        public int GetAmmunitionAmount() => 
            0;

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_EntityBase objectBuilder = base.GetObjectBuilder(copy);
            objectBuilder.SubtypeName = this.m_handItemDefId.SubtypeName;
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            m_physicalItemId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), "HandDrillItem");
            if ((objectBuilder.SubtypeName != null) && (objectBuilder.SubtypeName.Length > 0))
            {
                m_physicalItemId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), objectBuilder.SubtypeName + "Item");
            }
            this.PhysicalObject = (MyObjectBuilder_PhysicalGunObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) m_physicalItemId);
            (base.PositionComp as MyPositionComponent).WorldPositionChanged = new Action<object>(this.WorldPositionChanged);
            this.m_handItemDefId = objectBuilder.GetId();
            MyHandItemDefinition definition1 = MyDefinitionManager.Static.TryGetHandItemDefinition(ref this.m_handItemDefId);
            MyHandDrillDefinition definition = definition1 as MyHandDrillDefinition;
            this.m_drillMat = definition.ToolMaterial;
            this.m_speedMultiplier = 1f / definition.SpeedMultiplier;
            this.m_drillBase = new MyDrillBase(this, "Smoke_HandDrillDust", "Smoke_HandDrillDustStones", "Collision_Sparks_HandDrill", new MyDrillSensorRayCast(-0.5f, 2.15f, this.PhysicalItemDefinition), new MyDrillCutOut(1f, 0.35f * definition.DistanceMultiplier), 0.5f, -0.25f, 0.35f, 0f, null);
            this.m_drillBase.VoxelHarvestRatio = 0.009f * definition.HarvestRatioMultiplier;
            this.m_drillBase.ParticleOffset = definition.ParticleOffset;
            base.AddDebugRenderComponent(new MyDebugRenderCompomentDrawDrillBase(this.m_drillBase));
            base.Init(objectBuilder);
            this.m_physItemDef = MyDefinitionManager.Static.GetPhysicalItemDefinition(m_physicalItemId);
            float? scale = null;
            this.Init(null, this.m_physItemDef.Model, null, scale, null);
            base.Render.CastShadows = true;
            base.Render.NeedsResolveCastShadow = false;
            this.m_spike = base.Subparts["Spike"];
            this.m_spikeBasePos = this.m_spike.PositionComp.LocalMatrix.Translation;
            this.m_drillBase.IgnoredEntities.Add(this);
            this.m_drillBase.UpdatePosition(base.PositionComp.WorldMatrix);
            this.PhysicalObject.GunEntity = (MyObjectBuilder_EntityBase) objectBuilder.Clone();
            this.PhysicalObject.GunEntity.EntityId = base.EntityId;
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
            this.m_oreDetectorBase.DetectionRadius = 20f;
            this.m_oreDetectorBase.OnCheckControl = new MyOreDetectorComponent.CheckControlDelegate(this.OnCheckControl);
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(MyStringHash.GetOrCompute("Utility"), 4E-05f, () => this.m_tryingToDrill ? this.SinkComp.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId) : 0f);
            this.SinkComp = component;
            foreach (ToolSound sound in definition1.ToolSounds)
            {
                if (sound.type == null)
                {
                    continue;
                }
                if ((sound.subtype != null) && ((sound.sound != null) && sound.type.Equals("Main")))
                {
                    if (sound.subtype.Equals("Idle"))
                    {
                        this.m_drillBase.m_idleSoundLoop = new MySoundPair(sound.sound, true);
                    }
                    if (sound.subtype.Equals("Soundset"))
                    {
                        this.m_drillBase.m_drillMaterial = MyStringHash.GetOrCompute(sound.sound);
                    }
                }
            }
        }

        private bool OnCheckControl() => 
            ((MySession.Static.ControlledEntity != null) && ReferenceEquals((VRage.Game.Entity.MyEntity) MySession.Static.ControlledEntity, this.Owner));

        public void OnControlAcquired(MyCharacter owner)
        {
            this.m_owner = owner;
            if (owner != null)
            {
                this.m_shootIgnoreEntities = new VRage.Game.Entity.MyEntity[] { this, owner };
            }
            this.m_drillBase.OutputInventory = null;
            this.m_drillBase.IgnoredEntities.Add(this.m_owner);
        }

        public void OnControlReleased()
        {
            if (this.m_drillBase != null)
            {
                this.m_drillBase.IgnoredEntities.Remove(this.m_owner);
                this.m_drillBase.StopDrill();
                this.m_tryingToDrill = false;
                this.SinkComp.Update();
                this.m_drillBase.OutputInventory = null;
            }
            if ((this.m_owner != null) && (this.m_owner.ControllerInfo != null))
            {
                this.m_oreDetectorBase.Clear();
            }
            this.m_owner = null;
        }

        public void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            MyAnalyticsHelper.ReportActivityStartIf(!this.IsShooting, this.Owner, "Drilling", "Character", "HandTools", "HandDrill", true);
            if ((!this.DoDrillAction(action == MyShootActionEnum.PrimaryAction) && this.IsShooting) && (this.Owner != null))
            {
                this.Owner.EndShoot(action);
            }
        }

        public void ShootFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public bool ShouldEndShootOnPause(MyShootActionEnum action) => 
            (!this.m_isActionDoubleClicked.ContainsKey(action) ? true : !this.m_isActionDoubleClicked[action]);

        public bool SupressShootAnimation() => 
            false;

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            this.m_drillBase.UpdateAfterSimulation();
            if (this.IsShooting)
            {
                this.CreateCollisionSparks();
            }
            if (this.m_tryingToDrill || (this.m_drillBase.AnimationMaxSpeedRatio > 0f))
            {
                float num = ((float) (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_spikeLastUpdateTime)) / 1000f;
                if ((this.m_objectInDrillingRange && ((this.Owner != null) && this.Owner.ControllerInfo.IsLocallyControlled())) && !MySession.Static.IsCameraUserAnySpectator())
                {
                    this.m_drillBase.PerformCameraShake(1f);
                }
                this.m_spikeRotationAngle += (num * this.m_drillBase.AnimationMaxSpeedRatio) * -25f;
                if (this.m_spikeRotationAngle > 6.283185f)
                {
                    this.m_spikeRotationAngle -= 6.283185f;
                }
                if (this.m_spikeRotationAngle < 6.283185f)
                {
                    this.m_spikeRotationAngle += 6.283185f;
                }
                this.m_spikeThrustPosition += (num * this.m_drillBase.AnimationMaxSpeedRatio) / 0.06f;
                if (this.m_spikeThrustPosition > 1f)
                {
                    this.m_spikeThrustPosition -= 2f;
                    if ((this.Owner != null) && this.m_objectInDrillingRange)
                    {
                        this.Owner.WeaponPosition.AddBackkick(0.035f);
                    }
                }
                this.m_spikeLastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_spike.PositionComp.LocalMatrix = Matrix.CreateRotationZ(this.m_spikeRotationAngle) * Matrix.CreateTranslation(this.m_spikeBasePos + ((Math.Abs(this.m_spikeThrustPosition) * Vector3.UnitZ) * 0.03f));
            }
        }

        public override void UpdateBeforeSimulation()
        {
            int num1;
            base.UpdateBeforeSimulation();
            if ((this.m_owner == null) || !this.m_owner.IsInFirstPersonView)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) ReferenceEquals(this.m_owner, MySession.Static.LocalCharacter);
            }
            this.m_drillBase.Force2DSound = (bool) num1;
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            this.UpdateSoundEmitter();
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            this.m_oreDetectorBase.Update(base.PositionComp.GetPosition(), base.EntityId, true);
        }

        public void UpdateSoundEmitter()
        {
            Vector3 zero = Vector3.Zero;
            if (this.m_owner != null)
            {
                this.m_owner.GetLinearVelocity(ref zero, true);
            }
            this.m_drillBase.UpdateSoundEmitter(zero);
        }

        public unsafe void WorldPositionChanged(object source)
        {
            if (this.m_owner != null)
            {
                MatrixD identity = MatrixD.Identity;
                identity.Right = this.m_owner.WorldMatrix.Right;
                identity.Forward = this.m_owner.WeaponPosition.LogicalOrientationWorld;
                MatrixD* xdPtr1 = (MatrixD*) ref identity;
                xdPtr1.Up = Vector3D.Normalize(identity.Right.Cross(identity.Forward));
                identity.Translation = this.m_owner.WeaponPosition.LogicalPositionWorld;
                this.m_drillBase.UpdatePosition(identity);
            }
        }

        public MyResourceSinkComponent SinkComp
        {
            get => 
                this.m_sinkComp;
            set
            {
                if (base.Components.Contains(typeof(MyResourceSinkComponent)))
                {
                    base.Components.Remove<MyResourceSinkComponent>();
                }
                base.Components.Add<MyResourceSinkComponent>(value);
                this.m_sinkComp = value;
            }
        }

        public float BackkickForcePerSecond =>
            0f;

        public float ShakeAmount
        {
            get => 
                2.5f;
            protected set
            {
            }
        }

        public MyCharacter Owner =>
            this.m_owner;

        public long OwnerId =>
            ((this.m_owner == null) ? 0L : this.m_owner.EntityId);

        public long OwnerIdentityId =>
            ((this.m_owner == null) ? 0L : this.m_owner.GetPlayerIdentityId());

        public bool EnabledInWorldRules =>
            true;

        public MyObjectBuilder_PhysicalGunObject PhysicalObject { get; private set; }

        public bool IsShooting =>
            this.m_drillBase.IsDrilling;

        public VRage.Game.Entity.MyEntity DrilledEntity =>
            this.m_drillBase.DrilledEntity;

        public bool CollectingOre =>
            this.m_drillBase.CollectingOre;

        public bool ForceAnimationInsteadOfIK =>
            false;

        public bool IsBlocking =>
            false;

        public int ShootDirectionUpdateTime =>
            200;

        public bool IsSkinnable =>
            true;

        public MyDefinitionId DefinitionId =>
            this.m_handItemDefId;

        public MyToolBase GunBase =>
            null;

        public MyPhysicalItemDefinition PhysicalItemDefinition =>
            this.m_physItemDef;

        public int CurrentAmmunition { get; set; }

        public int CurrentMagazineAmmunition { get; set; }

        VRage.Game.Entity.MyEntity[] IMyGunBaseUser.IgnoreEntities =>
            this.m_shootIgnoreEntities;

        VRage.Game.Entity.MyEntity IMyGunBaseUser.Weapon =>
            this;

        VRage.Game.Entity.MyEntity IMyGunBaseUser.Owner =>
            this.m_owner;

        IMyMissileGunObject IMyGunBaseUser.Launcher =>
            null;

        MyInventory IMyGunBaseUser.AmmoInventory =>
            ((this.m_owner == null) ? null : this.m_owner.GetInventory(0));

        MyDefinitionId IMyGunBaseUser.PhysicalItemId =>
            new MyDefinitionId();

        MyInventory IMyGunBaseUser.WeaponInventory =>
            null;

        long IMyGunBaseUser.OwnerId =>
            ((this.m_owner == null) ? 0L : this.m_owner.ControllerInfo.ControllingIdentityId);

        string IMyGunBaseUser.ConstraintDisplayName =>
            null;
    }
}

