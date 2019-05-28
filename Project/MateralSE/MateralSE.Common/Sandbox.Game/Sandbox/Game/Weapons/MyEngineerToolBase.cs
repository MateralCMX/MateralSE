namespace Sandbox.Game.Weapons
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Lights;
    using Sandbox.Game.Weapons.Guns;
    using Sandbox.Game.World;
    using Sandbox.ModAPI.Weapons;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Lights;

    public abstract class MyEngineerToolBase : VRage.Game.Entity.MyEntity, IMyHandheldGunObject<MyToolBase>, IMyGunObject<MyToolBase>, IMyEngineerToolBase, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity
    {
        public static float GLARE_SIZE = 0.068f;
        protected float DEFAULT_REACH_DISTANCE = 2f;
        protected string m_effectId = "WelderContactPoint";
        protected float EffectScale = 1f;
        protected bool HasPrimaryEffect = true;
        protected bool HasSecondaryEffect;
        protected string SecondaryEffectName = "Dummy";
        protected Vector4 SecondaryLightColor = new Vector4(0.4f, 0.5f, 1f, 1f);
        protected float SecondaryLightFalloff = 2f;
        protected float SecondaryLightRadius = 7f;
        protected float SecondaryLightIntensityLower = 0.4f;
        protected float SecondaryLightIntensityUpper = 0.5f;
        protected float SecondaryLightGlareSize = GLARE_SIZE;
        protected MyShootActionEnum? EffectAction;
        private MyShootActionEnum? m_previousEffect;
        protected Dictionary<MyShootActionEnum, bool> m_isActionDoubleClicked = new Dictionary<MyShootActionEnum, bool>();
        protected MyEntity3DSoundEmitter m_soundEmitter;
        protected MyCharacter Owner;
        protected MyToolBase m_gunBase;
        private int m_lastTimeShoot;
        protected bool m_activated;
        private MyParticleEffect m_toolEffect;
        private MyParticleEffect m_toolSecondaryEffect;
        private MyLight m_toolEffectLight;
        private int m_lastMarkTime = -1;
        private int m_markedComponent = -1;
        private bool m_tryingToShoot;
        private bool m_wasPowered;
        protected MyCasterComponent m_raycastComponent;
        private MyResourceSinkComponent m_sinkComp;
        private int m_shootFrameCounter;
        private NumberFormatInfo m_oneDecimal;
        protected MyHandItemDefinition m_handItemDef;
        protected MyPhysicalItemDefinition m_physItemDef;
        protected float m_speedMultiplier;
        protected float m_distanceMultiplier;
        private MyFlareDefinition m_flare;

        public MyEngineerToolBase(int cooldownMs)
        {
            NumberFormatInfo info1 = new NumberFormatInfo();
            info1.NumberDecimalDigits = 1;
            info1.PercentDecimalDigits = 1;
            this.m_oneDecimal = info1;
            this.m_speedMultiplier = 1f;
            this.m_distanceMultiplier = 1f;
            this.ToolCooldownMs = cooldownMs;
            this.m_activated = false;
            this.m_wasPowered = false;
            base.NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            base.Render.NeedsDraw = true;
            (base.PositionComp as MyPositionComponent).WorldPositionChanged = new Action<object>(this.WorldPositionChanged);
            base.Render = new MyRenderComponentEngineerTool();
            base.AddDebugRenderComponent(new MyDebugRenderComponentEngineerTool(this));
        }

        protected abstract void AddHudInfo();
        public virtual void BeginFailReaction(MyShootActionEnum action, MyGunStatusEnum status)
        {
            if (ReferenceEquals(this.Owner, MySession.Static.LocalCharacter))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
            }
        }

        public virtual void BeginFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public virtual void BeginShoot(MyShootActionEnum action)
        {
        }

        protected float CalculateRequiredPower() => 
            (this.ShouldBePowered() ? this.SinkComp.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId) : 0f);

        public bool CanBeDrawn() => 
            ((this.Owner != null) && (ReferenceEquals(this.Owner, MySession.Static.ControlledEntity) && ((this.m_raycastComponent.HitCubeGrid != null) && ((this.m_raycastComponent.HitCubeGrid != null) && (this.HasCubeHighlight && !MyFakes.HIDE_ENGINEER_TOOL_HIGHLIGHT)))));

        public bool CanDoubleClickToStick(MyShootActionEnum action) => 
            false;

        public virtual bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            if (action != MyShootActionEnum.PrimaryAction)
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastTimeShoot) < this.ToolCooldownMs)
            {
                status = MyGunStatusEnum.Cooldown;
                return false;
            }
            status = MyGunStatusEnum.OK;
            return true;
        }

        public virtual bool CanStartEffect() => 
            true;

        private void CheckEffectType()
        {
            if ((this.m_previousEffect != null) && (this.m_toolEffect == null))
            {
                this.m_previousEffect = null;
            }
            MyShootActionEnum? effectAction = this.EffectAction;
            MyShootActionEnum? previousEffect = this.m_previousEffect;
            if (!((effectAction.GetValueOrDefault() == previousEffect.GetValueOrDefault()) & ((effectAction != null) == (previousEffect != null))))
            {
                if (this.m_previousEffect != null)
                {
                    this.StopEffect();
                }
                this.m_previousEffect = null;
                if ((this.EffectAction != null) && (MySector.MainCamera.GetDistanceFromPoint(base.PositionComp.GetPosition()) < 150.0))
                {
                    previousEffect = this.EffectAction;
                    MyShootActionEnum primaryAction = MyShootActionEnum.PrimaryAction;
                    if (((((MyShootActionEnum) previousEffect.GetValueOrDefault()) == primaryAction) & (previousEffect != null)) && this.HasPrimaryEffect)
                    {
                        this.StartEffect();
                        this.m_previousEffect = 0;
                    }
                    else
                    {
                        previousEffect = this.EffectAction;
                        primaryAction = MyShootActionEnum.SecondaryAction;
                        if (((((MyShootActionEnum) previousEffect.GetValueOrDefault()) == primaryAction) & (previousEffect != null)) && this.HasSecondaryEffect)
                        {
                            this.StartSecondaryEffect();
                            this.m_previousEffect = 1;
                        }
                    }
                }
            }
        }

        protected override void Closing()
        {
            this.StopEffect();
            this.StopSecondaryEffect();
            this.StopLoopSound();
            base.Closing();
        }

        private void CreateGlare(MyLight light)
        {
            light.GlareOn = light.LightOn;
            light.GlareQuerySize = 0.2f;
            light.GlareType = MyGlareTypeEnum.Normal;
            if (this.m_flare != null)
            {
                light.SubGlares = this.m_flare.SubGlares;
                light.GlareSize = this.m_flare.Size;
                light.GlareIntensity = this.m_flare.Intensity;
            }
        }

        protected virtual MyLight CreatePrimaryLight()
        {
            MyLight light = MyLights.AddLight();
            if (light != null)
            {
                light.Start(Vector3.Zero, this.m_handItemDef.LightColor, this.m_handItemDef.LightRadius, this.DisplayNameText + " Tool Primary");
                this.CreateGlare(light);
            }
            return light;
        }

        protected virtual MyLight CreateSecondaryLight()
        {
            MyLight light = MyLights.AddLight();
            if (light != null)
            {
                light.Start(Vector3.Zero, this.SecondaryLightColor, this.SecondaryLightRadius, this.DisplayNameText + " Tool Secondary");
                this.CreateGlare(light);
            }
            return light;
        }

        public Vector3 DirectionToTarget(Vector3D target)
        {
            Vector3D vectord;
            MyCharacterWeaponPositionComponent component = this.Owner.Components.Get<MyCharacterWeaponPositionComponent>();
            if (component != null)
            {
                vectord = Vector3D.Normalize(target - component.LogicalPositionWorld);
            }
            else
            {
                vectord = Vector3D.Normalize(target - base.PositionComp.WorldMatrix.Translation);
            }
            return (Vector3) vectord;
        }

        public void DoubleClicked(MyShootActionEnum action)
        {
            this.m_isActionDoubleClicked[action] = true;
        }

        protected virtual void DrawHud()
        {
            MyHud.BlockInfo.Visible = false;
            MySlimBlock hitBlock = this.m_raycastComponent.HitBlock;
            if (hitBlock == null)
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
            else
            {
                if (MyFakes.ENABLE_COMPOUND_BLOCKS && (hitBlock.FatBlock is MyCompoundCubeBlock))
                {
                    MyCompoundCubeBlock fatBlock = hitBlock.FatBlock as MyCompoundCubeBlock;
                    if (fatBlock.GetBlocksCount() > 0)
                    {
                        hitBlock = fatBlock.GetBlocks().First<MySlimBlock>();
                    }
                }
                MyHud.BlockInfo.Visible = true;
                MyHud.BlockInfo.MissingComponentIndex = -1;
                MySlimBlock.SetBlockComponents(MyHud.BlockInfo, hitBlock, null);
                MyHud.BlockInfo.BlockName = hitBlock.BlockDefinition.DisplayNameText;
                MyHud.BlockInfo.SetContextHelp(hitBlock.BlockDefinition);
                MyHud.BlockInfo.PCUCost = hitBlock.BlockDefinition.PCU;
                MyHud.BlockInfo.BlockIcons = hitBlock.BlockDefinition.Icons;
                MyHud.BlockInfo.BlockIntegrity = hitBlock.Integrity / hitBlock.MaxIntegrity;
                MyHud.BlockInfo.CriticalIntegrity = hitBlock.BlockDefinition.CriticalIntegrityRatio;
                MyHud.BlockInfo.CriticalComponentIndex = hitBlock.BlockDefinition.CriticalGroup;
                MyHud.BlockInfo.OwnershipIntegrity = hitBlock.BlockDefinition.OwnershipIntegrityRatio;
                MyHud.BlockInfo.BlockBuiltBy = hitBlock.BuiltBy;
                MyHud.BlockInfo.GridSize = hitBlock.CubeGrid.GridSizeEnum;
            }
        }

        public void DrawHud(IMyCameraController camera, long playerId)
        {
            MyHud.Crosshair.Recenter();
            this.DrawHud();
            this.UpdateHudComponentMark();
        }

        public void DrawHud(IMyCameraController camera, long playerId, bool fullUpdate)
        {
            this.DrawHud(camera, playerId);
        }

        public virtual void EndShoot(MyShootActionEnum action)
        {
            this.EffectAction = null;
            this.StopLoopSound();
            this.ShakeAmount = 0f;
            this.m_tryingToShoot = false;
            this.SinkComp.Update();
            this.m_activated = false;
            this.m_shootFrameCounter = 0;
            this.m_isActionDoubleClicked[action] = false;
        }

        public int GetAmmunitionAmount() => 
            0;

        protected virtual MatrixD GetEffectMatrix(float muzzleOffset, EffectType effectType)
        {
            Vector3D muzzleWorldPosition;
            if ((this.m_raycastComponent.HitCubeGrid == null) || (this.m_raycastComponent.HitBlock == null))
            {
                return MatrixD.CreateWorld(this.m_gunBase.GetMuzzleWorldPosition(), base.PositionComp.WorldMatrix.Forward, base.PositionComp.WorldMatrix.Up);
            }
            float num = Vector3.Dot((Vector3) (this.m_raycastComponent.HitPosition - this.m_gunBase.GetMuzzleWorldPosition()), (Vector3) base.PositionComp.WorldMatrix.Forward);
            Vector3D vectord = this.m_gunBase.GetMuzzleWorldPosition() + (base.PositionComp.WorldMatrix.Forward * (num * muzzleOffset));
            if ((num <= 0f) || (muzzleOffset != 0f))
            {
                muzzleWorldPosition = vectord;
            }
            else
            {
                muzzleWorldPosition = this.m_gunBase.GetMuzzleWorldPosition();
            }
            return MatrixD.CreateWorld(muzzleWorldPosition, base.PositionComp.WorldMatrix.Forward, base.PositionComp.WorldMatrix.Up);
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_EntityBase objectBuilder = base.GetObjectBuilder(copy);
            objectBuilder.SubtypeName = this.m_handItemDef.Id.SubtypeName;
            return objectBuilder;
        }

        public MySlimBlock GetTargetBlock()
        {
            if (!this.ReachesCube() || (this.m_raycastComponent.HitCubeGrid == null))
            {
                return null;
            }
            return this.m_raycastComponent.HitBlock;
        }

        protected virtual MySlimBlock GetTargetBlockForShoot() => 
            this.GetTargetBlock();

        protected IMyDestroyableObject GetTargetDestroyable() => 
            this.m_raycastComponent.HitDestroyableObj;

        public MyCubeGrid GetTargetGrid() => 
            this.m_raycastComponent.HitCubeGrid;

        public void Init(MyObjectBuilder_EntityBase builder, MyHandItemDefinition definition)
        {
            this.m_handItemDef = definition;
            if (definition == null)
            {
                this.m_gunBase = new MyToolBase(Vector3.Zero, base.WorldMatrix);
            }
            else
            {
                this.m_physItemDef = MyDefinitionManager.Static.GetPhysicalItemForHandItem(definition.Id);
                this.m_gunBase = new MyToolBase(this.m_handItemDef.MuzzlePosition, base.WorldMatrix);
            }
            base.Init(builder);
            if (this.PhysicalObject != null)
            {
                this.PhysicalObject.GunEntity = builder;
            }
            if (definition is MyEngineerToolBaseDefinition)
            {
                this.m_speedMultiplier = (this.m_handItemDef as MyEngineerToolBaseDefinition).SpeedMultiplier;
                this.m_distanceMultiplier = (this.m_handItemDef as MyEngineerToolBaseDefinition).DistanceMultiplier;
                string flare = (this.m_handItemDef as MyEngineerToolBaseDefinition).Flare;
                if (flare == "")
                {
                    this.m_flare = new MyFlareDefinition();
                }
                else
                {
                    MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_FlareDefinition), flare);
                    MyFlareDefinition definition1 = MyDefinitionManager.Static.GetDefinition(id) as MyFlareDefinition;
                    this.m_flare = definition1 ?? new MyFlareDefinition();
                }
            }
            MyDrillSensorRayCast caster = new MyDrillSensorRayCast(0f, this.DEFAULT_REACH_DISTANCE * this.m_distanceMultiplier, this.PhysicalItemDefinition);
            this.m_raycastComponent = new MyCasterComponent(caster);
            this.m_raycastComponent.SetPointOfReference(this.m_gunBase.GetMuzzleWorldPosition());
            base.Components.Add<MyCasterComponent>(this.m_raycastComponent);
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(MyStringHash.GetOrCompute("Utility"), 0.0001f, new Func<float>(this.CalculateRequiredPower));
            this.SinkComp = component;
            this.m_soundEmitter = new MyEntity3DSoundEmitter(this, false, 1f);
        }

        public void Init(MyObjectBuilder_EntityBase builder, MyDefinitionId id)
        {
            this.Init(builder, MyDefinitionManager.Static.TryGetHandItemForPhysicalItem(id));
        }

        protected void MarkMissingComponent(int componentIdx)
        {
            this.m_lastMarkTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            this.m_markedComponent = componentIdx;
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
        }

        public virtual void OnControlAcquired(MyCharacter owner)
        {
            this.Owner = owner;
            this.CharacterInventory = this.Owner.GetInventory(0);
            if (owner.ControllerInfo.IsLocallyHumanControlled())
            {
                this.AddHudInfo();
            }
        }

        public virtual void OnControlReleased()
        {
            this.RemoveHudInfo();
            this.Owner = null;
            this.CharacterInventory = null;
        }

        public virtual void OnFailShoot(MyGunStatusEnum status)
        {
            if (status == MyGunStatusEnum.Failed)
            {
                this.EffectAction = 1;
            }
        }

        public override void OnRemovedFromScene(object source)
        {
            this.RemoveHudInfo();
            base.OnRemovedFromScene(source);
            this.StopSecondaryEffect();
            this.StopEffect();
        }

        protected bool ReachesCube() => 
            (this.m_raycastComponent.HitBlock != null);

        protected abstract void RemoveHudInfo();
        public virtual void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            if (action == MyShootActionEnum.PrimaryAction)
            {
                this.m_lastTimeShoot = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_shootFrameCounter++;
                this.m_tryingToShoot = true;
                this.SinkComp.Update();
                if (!this.SinkComp.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
                {
                    this.EffectAction = null;
                }
                else
                {
                    this.m_activated = true;
                }
            }
        }

        public virtual void ShootFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
            if ((action == MyShootActionEnum.PrimaryAction) && (status == MyGunStatusEnum.Failed))
            {
                this.EffectAction = 1;
            }
        }

        protected virtual bool ShouldBePowered() => 
            this.m_tryingToShoot;

        public bool ShouldEndShootOnPause(MyShootActionEnum action) => 
            true;

        protected void StartEffect()
        {
            this.StopEffect();
            if (!string.IsNullOrEmpty(this.m_effectId) && this.CanStartEffect())
            {
                MyParticlesManager.TryCreateParticleEffect(this.m_effectId, this.GetEffectMatrix(0.1f, EffectType.Effect), out this.m_toolEffect);
                if (this.m_toolEffect != null)
                {
                    this.m_toolEffect.UserScale = this.EffectScale;
                }
                this.m_toolEffectLight = this.CreatePrimaryLight();
            }
            this.UpdateEffect();
        }

        protected virtual void StartLoopSound(bool effect)
        {
        }

        private void StartSecondaryEffect()
        {
            this.StopEffect();
            this.StopSecondaryEffect();
            MyParticlesManager.TryCreateParticleEffect(this.SecondaryEffectName, this.GetEffectMatrix(0.1f, EffectType.EffectSecondary), out this.m_toolSecondaryEffect);
            this.m_toolEffectLight = this.CreateSecondaryLight();
            this.UpdateEffect();
        }

        protected void StopEffect()
        {
            if (this.m_toolEffect != null)
            {
                this.m_toolEffect.Stop(true);
                this.m_toolEffect = null;
            }
            if (this.m_toolEffectLight != null)
            {
                MyLights.RemoveLight(this.m_toolEffectLight);
                this.m_toolEffectLight = null;
            }
        }

        protected virtual void StopLoopSound()
        {
        }

        protected void StopSecondaryEffect()
        {
            if (this.m_toolSecondaryEffect != null)
            {
                this.m_toolSecondaryEffect.Stop(true);
                this.m_toolSecondaryEffect = null;
            }
        }

        protected virtual void StopSound()
        {
        }

        public virtual bool SupressShootAnimation() => 
            false;

        protected void UnmarkMissingComponent()
        {
            this.m_lastMarkTime = -1;
            this.m_markedComponent = -1;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (this.Owner != null)
            {
                Vector3D vectord2;
                Vector3 localWeaponPosition = this.Owner.GetLocalWeaponPosition();
                Vector3D muzzleLocalPosition = this.m_gunBase.GetMuzzleLocalPosition();
                MatrixD worldMatrix = base.WorldMatrix;
                Vector3D.Rotate(ref muzzleLocalPosition, ref worldMatrix, out vectord2);
                this.m_raycastComponent.SetPointOfReference((this.Owner.PositionComp.GetPosition() + localWeaponPosition) + vectord2);
                if (this.IsShooting && this.IsPreheated)
                {
                    if (this.GetTargetBlockForShoot() == null)
                    {
                        this.EffectAction = 1;
                        this.ShakeAmount = this.m_handItemDef.ShakeAmountNoTarget;
                    }
                    else
                    {
                        this.EffectAction = 0;
                        this.ShakeAmount = this.m_handItemDef.ShakeAmountTarget;
                    }
                }
                this.SinkComp.Update();
                if (this.IsShooting && !this.SinkComp.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
                {
                    this.EndShoot(MyShootActionEnum.PrimaryAction);
                }
                this.UpdateEffect();
                this.CheckEffectType();
                if ((this.Owner != null) && this.Owner.ControllerInfo.IsLocallyHumanControlled())
                {
                    if (MySession.Static.SurvivalMode)
                    {
                        MySession.Static.GetCameraControllerEnum();
                        MyCharacter owner = (MyCharacter) this.CharacterInventory.Owner;
                        MyCubeBuilder.Static.MaxGridDistanceFrom = new Vector3D?(owner.PositionComp.GetPosition() + (owner.WorldMatrix.Up * 1.7999999523162842));
                    }
                    else
                    {
                        MyCubeBuilder.Static.MaxGridDistanceFrom = null;
                    }
                }
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            this.UpdateSoundEmitter();
        }

        private void UpdateEffect()
        {
            MyShootActionEnum? effectAction = this.EffectAction;
            MyShootActionEnum primaryAction = MyShootActionEnum.PrimaryAction;
            if (((((MyShootActionEnum) effectAction.GetValueOrDefault()) == primaryAction) & (effectAction != null)) && (this.m_raycastComponent.HitCubeGrid == null))
            {
                this.EffectAction = 1;
            }
            effectAction = this.EffectAction;
            primaryAction = MyShootActionEnum.SecondaryAction;
            if (((((MyShootActionEnum) effectAction.GetValueOrDefault()) == primaryAction) & (effectAction != null)) && ((this.m_raycastComponent.HitCharacter != null) || (this.m_raycastComponent.HitEnvironmentSector != null)))
            {
                this.EffectAction = 0;
            }
            if (this.EffectAction == null)
            {
                if (this.m_soundEmitter.IsPlaying)
                {
                    this.StopLoopSound();
                }
            }
            else
            {
                primaryAction = this.EffectAction.Value;
                if (primaryAction == MyShootActionEnum.PrimaryAction)
                {
                    this.StartLoopSound(true);
                }
                else if (primaryAction == MyShootActionEnum.SecondaryAction)
                {
                    this.StartLoopSound(false);
                }
            }
            if (this.m_toolEffect != null)
            {
                this.m_toolEffect.WorldMatrix = this.GetEffectMatrix(0.1f, EffectType.Effect);
            }
            if (this.m_toolSecondaryEffect != null)
            {
                this.m_toolSecondaryEffect.WorldMatrix = this.GetEffectMatrix(0.1f, EffectType.EffectSecondary);
            }
            if (this.m_toolEffectLight != null)
            {
                this.m_toolEffectLight.Position = this.GetEffectMatrix(0f, EffectType.Light).Translation;
                effectAction = this.EffectAction;
                primaryAction = MyShootActionEnum.PrimaryAction;
                if ((((MyShootActionEnum) effectAction.GetValueOrDefault()) == primaryAction) & (effectAction != null))
                {
                    this.m_toolEffectLight.Intensity = MyUtils.GetRandomFloat(this.m_handItemDef.LightIntensityLower, this.m_handItemDef.LightIntensityUpper);
                    if (this.m_flare != null)
                    {
                        this.m_toolEffectLight.GlareIntensity = (this.m_toolEffectLight.Intensity * this.m_handItemDef.LightGlareIntensity) * this.m_flare.Intensity;
                        this.m_toolEffectLight.GlareSize = (Vector2) ((this.m_toolEffectLight.Intensity * this.m_handItemDef.LightGlareSize) * this.m_flare.Size);
                    }
                }
                else
                {
                    this.m_toolEffectLight.Intensity = MyUtils.GetRandomFloat(this.SecondaryLightIntensityLower, this.SecondaryLightIntensityUpper);
                    if (this.m_flare != null)
                    {
                        this.m_toolEffectLight.GlareIntensity = (this.m_toolEffectLight.Intensity * this.m_handItemDef.LightGlareIntensity) * this.m_flare.Intensity;
                        this.m_toolEffectLight.GlareSize = (Vector2) ((this.m_toolEffectLight.Intensity * this.SecondaryLightGlareSize) * this.m_flare.Size);
                    }
                }
                if (this.m_flare != null)
                {
                    this.m_toolEffectLight.SubGlares = this.m_flare.SubGlares;
                }
                this.m_toolEffectLight.UpdateLight();
            }
        }

        private void UpdateHudComponentMark()
        {
            if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastMarkTime) > 0x9c4)
            {
                this.UnmarkMissingComponent();
            }
            else
            {
                MyHud.BlockInfo.MissingComponentIndex = this.m_markedComponent;
            }
        }

        private void UpdatePower()
        {
            bool flag = this.ShouldBePowered();
            if (flag != this.m_wasPowered)
            {
                this.m_wasPowered = flag;
                this.SinkComp.Update();
            }
        }

        public unsafe void UpdateSensorPosition()
        {
            if (this.Owner != null)
            {
                MyCharacter owner = this.Owner;
                MatrixD identity = MatrixD.Identity;
                identity.Translation = owner.WeaponPosition.LogicalPositionWorld;
                identity.Right = owner.WorldMatrix.Right;
                identity.Forward = owner.WeaponPosition.LogicalOrientationWorld;
                MatrixD* xdPtr1 = (MatrixD*) ref identity;
                xdPtr1.Up = Vector3.Cross((Vector3) identity.Right, (Vector3) identity.Forward);
                this.m_raycastComponent.OnWorldPosChanged(ref identity);
            }
        }

        public void UpdateSoundEmitter()
        {
            if (this.m_soundEmitter != null)
            {
                if (this.Owner != null)
                {
                    Vector3 zero = Vector3.Zero;
                    this.Owner.GetLinearVelocity(ref zero, true);
                    this.m_soundEmitter.SetVelocity(new Vector3?(zero));
                }
                this.m_soundEmitter.Update();
            }
        }

        private void WorldPositionChanged(object source)
        {
            this.m_gunBase.OnWorldPositionChanged(base.PositionComp.WorldMatrix);
            this.UpdateSensorPosition();
        }

        public bool IsDeconstructor =>
            false;

        public int ToolCooldownMs { get; private set; }

        public int EffectStopMs =>
            (this.ToolCooldownMs * 2);

        public string EffectId =>
            this.m_effectId;

        public long OwnerId =>
            ((this.Owner == null) ? 0L : this.Owner.EntityId);

        public long OwnerIdentityId =>
            ((this.Owner == null) ? 0L : this.Owner.GetPlayerIdentityId());

        public MyToolBase GunBase =>
            this.m_gunBase;

        public Vector3I TargetCube
        {
            get
            {
                if ((this.m_raycastComponent == null) || (this.m_raycastComponent.HitBlock == null))
                {
                    return Vector3I.Zero;
                }
                return this.m_raycastComponent.HitBlock.Position;
            }
        }

        public bool HasHitBlock =>
            (this.m_raycastComponent.HitBlock != null);

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

        public bool IsShooting =>
            this.m_activated;

        public bool ForceAnimationInsteadOfIK =>
            false;

        public bool IsBlocking =>
            false;

        public bool IsPreheated =>
            (this.m_shootFrameCounter >= 2);

        public Vector3 SensorDisplacement { get; set; }

        protected MyInventory CharacterInventory { get; private set; }

        public MyObjectBuilder_PhysicalGunObject PhysicalObject { get; protected set; }

        public float BackkickForcePerSecond =>
            0f;

        public float ShakeAmount { get; protected set; }

        protected bool HasCubeHighlight { get; set; }

        public Color HighlightColor { get; set; }

        public MyStringId HighlightMaterial { get; set; }

        public bool EnabledInWorldRules =>
            true;

        public abstract bool IsSkinnable { get; }

        public MyDefinitionId DefinitionId =>
            this.m_handItemDef.Id;

        int IMyGunObject<MyToolBase>.ShootDirectionUpdateTime =>
            200;

        public MyPhysicalItemDefinition PhysicalItemDefinition =>
            this.m_physItemDef;

        public int CurrentAmmunition { get; set; }

        public int CurrentMagazineAmmunition { get; set; }

        protected enum EffectType
        {
            Light,
            Effect,
            EffectSecondary
        }
    }
}

