namespace Sandbox.Game.Weapons
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Utils;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.ModAPI.Weapons;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_AngleGrinder), true), StaticEventOwner]
    public class MyAngleGrinder : MyEngineerToolBase, IMyAngleGrinder, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyEngineerToolBase, IMyHandheldGunObject<MyToolBase>, IMyGunObject<MyToolBase>
    {
        private MySoundPair m_idleSound;
        private MySoundPair m_actualSound;
        private MyStringHash m_source;
        private MyStringHash m_metal;
        private static readonly float GRINDER_AMOUNT_PER_SECOND = 2f;
        private static readonly float GRINDER_MAX_SPEED_RPM = 500f;
        private static readonly float GRINDER_ACCELERATION_RPMPS = 700f;
        private static readonly float GRINDER_DECELERATION_RPMPS = 500f;
        public static float GRINDER_MAX_SHAKE = 1.5f;
        private MyHudNotification m_safezoneNotification;
        private static int m_lastTimePlayedSound;
        private int m_lastUpdateTime;
        private float m_rotationSpeed;
        private int m_lastContactTime;
        private int m_lastItemId;
        private static MyDefinitionId m_physicalItemId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), "AngleGrinderItem");
        private double m_grinderCameraMeanShakeIntensity;

        public MyAngleGrinder() : base(250)
        {
            this.m_idleSound = new MySoundPair("ToolPlayGrindIdle", true);
            this.m_actualSound = new MySoundPair("ToolPlayGrindMetal", true);
            this.m_source = MyStringHash.GetOrCompute("Grinder");
            this.m_metal = MyStringHash.GetOrCompute("Metal");
            this.m_grinderCameraMeanShakeIntensity = 1.0;
            base.SecondaryLightIntensityLower = 0.4f;
            base.SecondaryLightIntensityUpper = 0.4f;
            base.EffectScale = 0.6f;
            base.HasCubeHighlight = true;
            base.HighlightColor = Color.Red * 0.3f;
            base.HighlightMaterial = MyStringId.GetOrCompute("GizmoDrawLineRed");
            this.m_rotationSpeed = 0f;
        }

        protected override void AddHudInfo()
        {
        }

        public override void BeginFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
            if (status == MyGunStatusEnum.SafeZoneDenied)
            {
                if (this.m_safezoneNotification == null)
                {
                    this.m_safezoneNotification = new MyHudNotification(MyCommonTexts.SafeZone_GrindingDisabled, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                }
                MyHud.Notifications.Add(this.m_safezoneNotification);
            }
        }

        public override void BeginShoot(MyShootActionEnum action)
        {
        }

        public bool CanDoubleClickToStick(MyShootActionEnum action) => 
            true;

        public override bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            if (MySessionComponentSafeZones.IsActionAllowed(base.Owner, MySafeZoneAction.Grinding, 0L))
            {
                return base.CanShoot(action, shooter, out status);
            }
            status = MyGunStatusEnum.SafeZoneDenied;
            return false;
        }

        public override void EndShoot(MyShootActionEnum action)
        {
            MyAnalyticsHelper.ReportActivityEnd(base.Owner, "Grinding");
            base.EndShoot(action);
        }

        protected override MatrixD GetEffectMatrix(float muzzleOffset, MyEngineerToolBase.EffectType effectType)
        {
            if ((effectType != MyEngineerToolBase.EffectType.Light) && ((base.m_raycastComponent.HitCubeGrid != null) && (base.m_raycastComponent.HitBlock != null)))
            {
                MyCharacter owner = base.Owner;
            }
            return MatrixD.CreateWorld(base.m_gunBase.GetMuzzleWorldPosition(), base.WorldMatrix.Forward, base.WorldMatrix.Up);
        }

        private void Grind()
        {
            MySlimBlock targetBlock = base.GetTargetBlock();
            MyStringHash metal = this.m_metal;
            base.m_effectId = null;
            if ((targetBlock == null) || MySessionComponentSafeZones.IsActionAllowed(targetBlock.WorldAABB, MySafeZoneAction.Grinding, 0L))
            {
                if ((targetBlock != null) && ((!MySession.Static.IsScenario && !MySession.Static.Settings.ScenarioEditMode) || targetBlock.CubeGrid.BlocksDestructionEnabled))
                {
                    MyCubeBlockDefinition.PreloadConstructionModels(targetBlock.BlockDefinition);
                    if (Sync.IsServer)
                    {
                        float hackSpeedMultiplier = 1f;
                        if (((targetBlock.FatBlock != null) && ((base.Owner != null) && (base.Owner.ControllerInfo.Controller != null))) && (base.Owner.ControllerInfo.Controller.Player != null))
                        {
                            MyRelationsBetweenPlayerAndBlock userRelationToOwner = targetBlock.FatBlock.GetUserRelationToOwner(base.Owner.ControllerInfo.Controller.Player.Identity.IdentityId);
                            if ((userRelationToOwner == MyRelationsBetweenPlayerAndBlock.Enemies) || (userRelationToOwner == MyRelationsBetweenPlayerAndBlock.Neutral))
                            {
                                hackSpeedMultiplier = MySession.Static.HackSpeedMultiplier;
                            }
                        }
                        MyDamageInformation info = new MyDamageInformation(false, this.GrinderAmount * hackSpeedMultiplier, MyDamageType.Grind, base.EntityId);
                        if (targetBlock.UseDamageSystem)
                        {
                            MyDamageSystem.Static.RaiseBeforeDamageApplied(targetBlock, ref info);
                        }
                        if (targetBlock.CubeGrid.Editable)
                        {
                            targetBlock.DecreaseMountLevel(info.Amount, base.CharacterInventory, false);
                            if ((targetBlock.MoveItemsFromConstructionStockpile(base.CharacterInventory, MyItemFlags.None) && ((base.Owner.ControllerInfo != null) && (base.Owner.ControllerInfo.Controller != null))) && (base.Owner.ControllerInfo.Controller.Player != null))
                            {
                                ulong steamId = base.Owner.ControllerInfo.Controller.Player.Id.SteamId;
                                this.SendInventoryFullNotification(steamId);
                            }
                        }
                        if (((MySession.Static != null) && ReferenceEquals(base.Owner, MySession.Static.LocalCharacter)) && (MyMusicController.Static != null))
                        {
                            MyMusicController.Static.Building(250);
                        }
                        if (targetBlock.UseDamageSystem)
                        {
                            MyDamageSystem.Static.RaiseAfterDamageApplied(targetBlock, info);
                        }
                        if (targetBlock.IsFullyDismounted)
                        {
                            if (targetBlock.UseDamageSystem)
                            {
                                MyDamageSystem.Static.RaiseDestroyed(targetBlock, info);
                            }
                            targetBlock.SpawnConstructionStockpile();
                            targetBlock.CubeGrid.RazeBlock(targetBlock.Min);
                        }
                    }
                    if (targetBlock.BlockDefinition.PhysicalMaterial.Id.SubtypeName.Length > 0)
                    {
                        metal = targetBlock.BlockDefinition.PhysicalMaterial.Id.SubtypeId;
                    }
                }
                IMyDestroyableObject targetDestroyable = base.GetTargetDestroyable();
                if (targetDestroyable != null)
                {
                    if ((targetDestroyable is VRage.Game.Entity.MyEntity) && !MySessionComponentSafeZones.IsActionAllowed((VRage.Game.Entity.MyEntity) targetDestroyable, MySafeZoneAction.Grinding, 0L))
                    {
                        return;
                    }
                    if ((targetDestroyable is MyCharacter) && ReferenceEquals(targetDestroyable as MyCharacter, base.Owner))
                    {
                        return;
                    }
                    if (Sync.IsServer)
                    {
                        if (((targetDestroyable is MyCharacter) && ReferenceEquals(MySession.Static.ControlledEntity, base.Owner)) && !(targetDestroyable as MyCharacter).IsDead)
                        {
                            MySession @static = MySession.Static;
                            @static.TotalDamageDealt += (uint) 20;
                        }
                        MyHitInfo? hitInfo = null;
                        targetDestroyable.DoDamage(20f, MyDamageType.Grind, true, hitInfo, (base.Owner != null) ? base.Owner.EntityId : 0L);
                    }
                    if (targetDestroyable is MyCharacter)
                    {
                        metal = MyStringHash.GetOrCompute((targetDestroyable as MyCharacter).Definition.PhysicalMaterial);
                    }
                }
                MyEnvironmentSector hitEnvironmentSector = base.m_raycastComponent.HitEnvironmentSector;
                if (hitEnvironmentSector != null)
                {
                    if (Sync.IsServer)
                    {
                        int environmentItem = base.m_raycastComponent.EnvironmentItem;
                        if (environmentItem != this.m_lastItemId)
                        {
                            this.m_lastItemId = environmentItem;
                            this.m_lastContactTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                        }
                        if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastContactTime) > (1500f / base.m_speedMultiplier))
                        {
                            Vector3D hitnormal = base.Owner.WorldMatrix.Right + base.Owner.WorldMatrix.Forward;
                            hitnormal.Normalize();
                            float mass = base.Owner.Physics.Mass;
                            hitEnvironmentSector.GetModule<MyBreakableEnvironmentProxy>().BreakAt(environmentItem, base.m_raycastComponent.HitPosition, hitnormal, (double) ((10f * 10f) * mass));
                            this.m_lastContactTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                            this.m_lastItemId = 0;
                        }
                    }
                    metal = MyStringHash.GetOrCompute("Wood");
                }
                if (((targetBlock != null) || (targetDestroyable != null)) || (hitEnvironmentSector != null))
                {
                    this.m_actualSound = MyMaterialPropertiesHelper.Static.GetCollisionCue(MyMaterialPropertiesHelper.CollisionType.Start, base.m_handItemDef.ToolMaterial, metal);
                    base.m_effectId = MyMaterialPropertiesHelper.Static.GetCollisionEffect(MyMaterialPropertiesHelper.CollisionType.Start, base.m_handItemDef.ToolMaterial, metal);
                }
            }
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            m_physicalItemId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), "AngleGrinderItem");
            if ((objectBuilder.SubtypeName != null) && (objectBuilder.SubtypeName.Length > 0))
            {
                m_physicalItemId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), objectBuilder.SubtypeName + "Item");
            }
            base.PhysicalObject = (MyObjectBuilder_PhysicalGunObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) m_physicalItemId);
            base.Init(objectBuilder, m_physicalItemId);
            base.m_effectId = MyMaterialPropertiesHelper.Static.GetCollisionEffect(MyMaterialPropertiesHelper.CollisionType.Start, base.m_handItemDef.ToolMaterial, this.m_metal);
            MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(m_physicalItemId);
            float? scale = null;
            this.Init(null, physicalItemDefinition.Model, null, scale, null);
            base.Render.CastShadows = true;
            base.Render.NeedsResolveCastShadow = false;
            base.PhysicalObject.GunEntity = (MyObjectBuilder_EntityBase) objectBuilder.Clone();
            base.PhysicalObject.GunEntity.EntityId = base.EntityId;
            foreach (ToolSound sound in base.m_handItemDef.ToolSounds)
            {
                if (sound.type == null)
                {
                    continue;
                }
                if ((sound.subtype != null) && ((sound.sound != null) && (sound.type.Equals("Main") && sound.subtype.Equals("Idle"))))
                {
                    this.m_idleSound = new MySoundPair(sound.sound, true);
                }
            }
        }

        [Event(null, 0x178), Reliable, Client]
        private static void OnInventoryFulfilled()
        {
            if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - m_lastTimePlayedSound) > 0x9c4)
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudVocInventoryFull);
                m_lastTimePlayedSound = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            }
            MyHud.Stats.GetStat<MyStatPlayerInventoryFull>().InventoryFull = true;
        }

        public void PerformCameraShake()
        {
            if (MySector.MainCamera != null)
            {
                float shakePower = MathHelper.Clamp(((float) (-Math.Log(MyRandom.Instance.NextDouble()) * this.m_grinderCameraMeanShakeIntensity)) * GRINDER_MAX_SHAKE, 0f, GRINDER_MAX_SHAKE);
                MySector.MainCamera.CameraShake.AddShake(shakePower);
            }
        }

        protected override void RemoveHudInfo()
        {
        }

        private void SendInventoryFullNotification(ulong clientId)
        {
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent(x => new Action(MyAngleGrinder.OnInventoryFulfilled), new EndpointId(clientId), position);
        }

        public override void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            MyAnalyticsHelper.ReportActivityStartIf(!base.m_activated, base.Owner, "Grinding", "Character", "HandTools", "AngleGrinder", true);
            base.Shoot(action, direction, overrideWeaponPos, gunAction);
            if (((action == MyShootActionEnum.PrimaryAction) && base.IsPreheated) && base.m_activated)
            {
                this.Grind();
            }
        }

        public bool ShouldEndShootOnPause(MyShootActionEnum action) => 
            (!base.m_isActionDoubleClicked.ContainsKey(action) ? true : !base.m_isActionDoubleClicked[action]);

        protected override void StartLoopSound(bool effect)
        {
            bool? nullable;
            int num1;
            if ((base.Owner == null) || !base.Owner.IsInFirstPersonView)
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) ReferenceEquals(base.Owner, MySession.Static.LocalCharacter);
            }
            bool flag = (bool) num1;
            MySoundPair soundId = effect ? this.m_actualSound : this.m_idleSound;
            if ((base.m_soundEmitter.Sound == null) || !base.m_soundEmitter.Sound.IsPlaying)
            {
                nullable = null;
                base.m_soundEmitter.PlaySound(soundId, true, false, flag, false, false, nullable);
            }
            else if (flag != base.m_soundEmitter.Force2D)
            {
                nullable = null;
                base.m_soundEmitter.PlaySound(soundId, true, true, flag, false, false, nullable);
            }
            else
            {
                nullable = null;
                base.m_soundEmitter.PlaySingleSound(soundId, true, false, false, nullable);
            }
        }

        protected override void StopLoopSound()
        {
            base.m_soundEmitter.StopSound(false, true);
        }

        protected override void StopSound()
        {
            if ((base.m_soundEmitter.Sound != null) && base.m_soundEmitter.Sound.IsPlaying)
            {
                base.m_soundEmitter.StopSound(true, true);
            }
        }

        public override bool SupressShootAnimation() => 
            false;

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            int num = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastUpdateTime;
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (!base.m_activated)
            {
                base.m_effectId = null;
            }
            if (base.m_activated && (this.m_rotationSpeed < GRINDER_MAX_SPEED_RPM))
            {
                this.m_rotationSpeed += (num * 0.001f) * GRINDER_ACCELERATION_RPMPS;
                if (this.m_rotationSpeed > GRINDER_MAX_SPEED_RPM)
                {
                    this.m_rotationSpeed = GRINDER_MAX_SPEED_RPM;
                }
            }
            else if (!base.m_activated && (this.m_rotationSpeed > 0f))
            {
                this.m_rotationSpeed -= (num * 0.001f) * GRINDER_DECELERATION_RPMPS;
                if (this.m_rotationSpeed < 0f)
                {
                    this.m_rotationSpeed = 0f;
                }
            }
            if (((base.m_effectId != null) && (base.Owner != null)) && base.Owner.ControllerInfo.IsLocallyControlled())
            {
                this.PerformCameraShake();
            }
            MyEntitySubpart subpart = base.Subparts["grinder"];
            subpart.PositionComp.LocalMatrix = Matrix.CreateRotationY((-num * this.m_rotationSpeed) * 0.0001047198f) * subpart.PositionComp.LocalMatrix;
        }

        public override bool IsSkinnable =>
            true;

        private float GrinderAmount =>
            ((((MySession.Static.GrinderSpeedMultiplier * base.m_speedMultiplier) * GRINDER_AMOUNT_PER_SECOND) * base.ToolCooldownMs) / 1000f);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAngleGrinder.<>c <>9 = new MyAngleGrinder.<>c();
            public static Func<IMyEventOwner, Action> <>9__32_0;

            internal Action <SendInventoryFullNotification>b__32_0(IMyEventOwner x) => 
                new Action(MyAngleGrinder.OnInventoryFulfilled);
        }
    }
}

