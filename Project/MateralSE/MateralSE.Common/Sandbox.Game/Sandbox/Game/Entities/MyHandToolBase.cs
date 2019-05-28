namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Utils;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Gui;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [StaticEventOwner, MyEntityType(typeof(MyObjectBuilder_HandToolBase), true)]
    public class MyHandToolBase : MyEntity, IMyHandheldGunObject<MyToolBase>, IMyGunObject<MyToolBase>, IStoppableAttackingTool
    {
        private static MyStringId m_startCue = MyStringId.GetOrCompute("Start");
        private static MyStringId m_hitCue = MyStringId.GetOrCompute("Hit");
        private const float AFTER_SHOOT_HIT_DELAY = 0.4f;
        private MyDefinitionId m_handItemDefinitionId;
        private Sandbox.Definitions.MyToolActionDefinition? m_primaryToolAction;
        private MyToolHitCondition m_primaryHitCondition;
        private Sandbox.Definitions.MyToolActionDefinition? m_secondaryToolAction;
        private MyToolHitCondition m_secondaryHitCondition;
        private Sandbox.Definitions.MyToolActionDefinition? m_shotToolAction;
        private MyToolHitCondition m_shotHitCondition;
        protected Dictionary<MyShootActionEnum, bool> m_isActionDoubleClicked = new Dictionary<MyShootActionEnum, bool>();
        private bool m_wasShooting;
        private bool m_swingSoundPlayed;
        private bool m_isHit;
        protected Dictionary<string, IMyHandToolComponent> m_toolComponents = new Dictionary<string, IMyHandToolComponent>();
        private MyCharacter m_owner;
        protected int m_lastShot;
        private int m_lastHit;
        private int m_hitDelay;
        private MyPhysicalItemDefinition m_physItemDef;
        protected MyToolItemDefinition m_toolItemDef;
        private MyEntity3DSoundEmitter m_soundEmitter;
        private Dictionary<string, MySoundPair> m_toolSounds = new Dictionary<string, MySoundPair>();
        private static MyStringId BlockId = MyStringId.Get("Block");
        private MyHudNotification m_notEnoughStatNotification;

        public MyHandToolBase()
        {
            this.m_soundEmitter = new MyEntity3DSoundEmitter(this, false, 1f);
            this.GunBase = new MyToolBase();
        }

        public void BeginFailReaction(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public void BeginFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public virtual void BeginShoot(MyShootActionEnum action)
        {
        }

        public bool CanDoubleClickToStick(MyShootActionEnum action) => 
            false;

        protected bool CanHit(IMyHandToolComponent toolComponent, MyCharacterDetectorComponent detectorComponent, ref bool isBlock, out float hitEfficiency)
        {
            MyTuple<ushort, MyStringHash> tuple;
            bool flag = true;
            hitEfficiency = 1f;
            if ((detectorComponent.HitBody != null) && (detectorComponent.HitBody.UserObject is MyBlockingBody))
            {
                MyBlockingBody userObject = detectorComponent.HitBody.UserObject as MyBlockingBody;
                if ((userObject.HandTool.IsBlocking && (userObject.HandTool.m_owner.StatComp != null)) && userObject.HandTool.m_owner.StatComp.CanDoAction(userObject.HandTool.m_shotHitCondition.StatsActionIfHit, out tuple, false))
                {
                    userObject.HandTool.m_owner.StatComp.DoAction(userObject.HandTool.m_shotHitCondition.StatsActionIfHit);
                    if (!string.IsNullOrEmpty(userObject.HandTool.m_shotHitCondition.StatsModifierIfHit))
                    {
                        userObject.HandTool.m_owner.StatComp.ApplyModifier(userObject.HandTool.m_shotHitCondition.StatsModifierIfHit);
                    }
                    isBlock = true;
                    if (!string.IsNullOrEmpty(userObject.HandTool.m_shotToolAction.Value.StatsEfficiency))
                    {
                        hitEfficiency = 1f - userObject.HandTool.m_owner.StatComp.GetEfficiencyModifier(userObject.HandTool.m_shotToolAction.Value.StatsEfficiency);
                    }
                    flag = hitEfficiency > 0f;
                    MyEntityContainerEventExtensions.RaiseEntityEventOn(userObject.HandTool, MyStringHash.GetOrCompute("Hit"), new MyEntityContainerEventExtensions.HitParams(MyStringHash.GetOrCompute("Block"), this.PhysicalItemDefinition.Id.SubtypeId));
                }
            }
            if (!flag)
            {
                hitEfficiency = 0f;
                return flag;
            }
            if (!string.IsNullOrEmpty(this.m_shotHitCondition.StatsActionIfHit))
            {
                flag = (this.m_owner.StatComp != null) && this.m_owner.StatComp.CanDoAction(this.m_shotHitCondition.StatsActionIfHit, out tuple, false);
                if (!flag)
                {
                    hitEfficiency = 0f;
                    return flag;
                }
            }
            flag = Vector3.Distance((Vector3) detectorComponent.HitPosition, (Vector3) detectorComponent.StartPosition) <= this.m_toolItemDef.HitDistance;
            if (!flag)
            {
                hitEfficiency = 0f;
                return flag;
            }
            MyEntity entity = this.m_owner.Entity;
            long playerIdentityId = this.m_owner.GetPlayerIdentityId();
            MyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(playerIdentityId) as MyFaction;
            if ((faction != null) && !faction.EnableFriendlyFire)
            {
                MyCharacter detectedEntity = detectorComponent.DetectedEntity as MyCharacter;
                if (detectedEntity != null)
                {
                    flag = !faction.IsMember(detectedEntity.GetPlayerIdentityId());
                    hitEfficiency = flag ? hitEfficiency : 0f;
                }
            }
            return flag;
        }

        public virtual bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastHit) < this.m_hitDelay)
            {
                status = MyGunStatusEnum.Failed;
                return false;
            }
            status = MyGunStatusEnum.OK;
            if (this.IsShooting)
            {
                status = MyGunStatusEnum.Cooldown;
            }
            if (this.m_owner == null)
            {
                status = MyGunStatusEnum.Failed;
            }
            return (status == MyGunStatusEnum.OK);
        }

        private void CloseBlockingPhysics()
        {
            if (this.Physics != null)
            {
                this.Physics.Close();
                this.Physics = null;
            }
        }

        public Vector3 DirectionToTarget(Vector3D target) => 
            ((Vector3) target);

        public void DoubleClicked(MyShootActionEnum action)
        {
            this.m_isActionDoubleClicked[action] = true;
        }

        public void DrawHud(IMyCameraController camera, long playerId)
        {
            if ((this.m_primaryToolAction != null) && this.m_toolComponents.ContainsKey(this.m_primaryHitCondition.Component))
            {
                this.m_toolComponents[this.m_primaryHitCondition.Component].DrawHud();
            }
        }

        public void DrawHud(IMyCameraController camera, long playerId, bool fullUpdate)
        {
            this.DrawHud(camera, playerId);
        }

        public virtual void EndShoot(MyShootActionEnum action)
        {
            if ((this.m_shotToolAction != null) && (this.m_shotToolAction.Value.HitDuration == 0f))
            {
                this.m_shotToolAction = null;
            }
            this.m_isActionDoubleClicked[action] = false;
        }

        public int GetAmmunitionAmount() => 
            0;

        private void GetMostEffectiveToolAction(List<Sandbox.Definitions.MyToolActionDefinition> toolActions, out Sandbox.Definitions.MyToolActionDefinition? bestAction, out MyToolHitCondition bestCondition)
        {
            MyCharacterDetectorComponent component = this.m_owner.Components.Get<MyCharacterDetectorComponent>();
            IMyEntity detectedEntity = null;
            uint shapeKey = 0;
            if (component != null)
            {
                detectedEntity = component.DetectedEntity;
                shapeKey = component.ShapeKey;
                if (Vector3.Distance((Vector3) component.HitPosition, (Vector3) component.StartPosition) > this.m_toolItemDef.HitDistance)
                {
                    detectedEntity = null;
                }
            }
            bestAction = 0;
            bestCondition = new MyToolHitCondition();
            using (List<Sandbox.Definitions.MyToolActionDefinition>.Enumerator enumerator = toolActions.GetEnumerator())
            {
                while (true)
                {
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            Sandbox.Definitions.MyToolActionDefinition current = enumerator.Current;
                            if (current.HitConditions == null)
                            {
                                continue;
                            }
                            MyToolHitCondition[] hitConditions = current.HitConditions;
                            int index = 0;
                            while (true)
                            {
                                if (index < hitConditions.Length)
                                {
                                    MyToolHitCondition condition = hitConditions[index];
                                    if (condition.EntityType != null)
                                    {
                                        if (detectedEntity != null)
                                        {
                                            string element = this.GetStateForTarget((MyEntity) detectedEntity, shapeKey, condition.Component);
                                            if (condition.EntityType.Contains<string>(element))
                                            {
                                                bestAction = new Sandbox.Definitions.MyToolActionDefinition?(current);
                                                bestCondition = condition;
                                                break;
                                            }
                                        }
                                        index++;
                                        continue;
                                    }
                                    bestAction = new Sandbox.Definitions.MyToolActionDefinition?(current);
                                    bestCondition = condition;
                                }
                                else
                                {
                                    continue;
                                }
                                break;
                            }
                        }
                        return;
                    }
                }
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_HandToolBase objectBuilder = base.GetObjectBuilder(copy) as MyObjectBuilder_HandToolBase;
            objectBuilder.SubtypeName = this.m_handItemDefinitionId.SubtypeName;
            objectBuilder.DeviceBase = this.GunBase.GetObjectBuilder();
            return objectBuilder;
        }

        private void GetPreferredToolAction(List<Sandbox.Definitions.MyToolActionDefinition> toolActions, string name, out Sandbox.Definitions.MyToolActionDefinition? bestAction, out MyToolHitCondition bestCondition)
        {
            bestAction = 0;
            bestCondition = new MyToolHitCondition();
            MyStringId orCompute = MyStringId.GetOrCompute(name);
            foreach (Sandbox.Definitions.MyToolActionDefinition definition in toolActions)
            {
                if ((definition.HitConditions.Length != 0) && (definition.Name == orCompute))
                {
                    bestAction = new Sandbox.Definitions.MyToolActionDefinition?(definition);
                    bestCondition = definition.HitConditions[0];
                    break;
                }
            }
        }

        private string GetStateForTarget(MyEntity targetEntity, uint shapeKey, string actionType)
        {
            if (targetEntity != null)
            {
                IMyHandToolComponent component;
                string stateForTarget = null;
                if (this.m_toolComponents.TryGetValue(actionType, out component))
                {
                    stateForTarget = component.GetStateForTarget(targetEntity, shapeKey);
                    if (!string.IsNullOrEmpty(stateForTarget))
                    {
                        return stateForTarget;
                    }
                }
                using (Dictionary<string, IMyHandToolComponent>.Enumerator enumerator = this.m_toolComponents.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<string, IMyHandToolComponent> current = enumerator.Current;
                        stateForTarget = current.Value.GetStateForTarget(targetEntity, shapeKey);
                        if (!string.IsNullOrEmpty(stateForTarget))
                        {
                            return stateForTarget;
                        }
                    }
                }
            }
            return null;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            this.m_handItemDefinitionId = objectBuilder.GetId();
            this.m_physItemDef = MyDefinitionManager.Static.GetPhysicalItemForHandItem(this.m_handItemDefinitionId);
            base.Init(objectBuilder);
            float? scale = null;
            this.Init(null, this.PhysicalItemDefinition.Model, null, scale, null);
            base.Save = false;
            this.PhysicalObject = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_PhysicalGunObject>(this.m_handItemDefinitionId.SubtypeName);
            this.PhysicalObject.GunEntity = (MyObjectBuilder_EntityBase) objectBuilder.Clone();
            this.PhysicalObject.GunEntity.EntityId = base.EntityId;
            this.m_toolItemDef = this.PhysicalItemDefinition as MyToolItemDefinition;
            this.m_notEnoughStatNotification = new MyHudNotification(MyCommonTexts.NotificationStatNotEnough, 0x3e8, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important);
            this.InitToolComponents();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            MyObjectBuilder_HandToolBase base2 = objectBuilder as MyObjectBuilder_HandToolBase;
            if (base2.DeviceBase != null)
            {
                this.GunBase.Init(base2.DeviceBase);
            }
        }

        private void InitBlockingPhysics(MyEntity owner)
        {
            this.CloseBlockingPhysics();
            this.Physics = new MyBlockingBody(this, owner);
            HkShape shape = (HkShape) new HkBoxShape((Vector3) (0.5f * new Vector3(0.5f, 0.7f, 0.25f)));
            HkMassProperties? massProperties = null;
            this.Physics.CreateFromCollisionObject(shape, new Vector3(0f, 0.9f, -0.5f), base.WorldMatrix, massProperties, 0x13);
            this.Physics.MaterialType = this.m_physItemDef.PhysicalMaterial;
            shape.RemoveReference();
            this.Physics.Enabled = false;
            this.m_owner.PositionComp.OnPositionChanged += new Action<MyPositionComponentBase>(this.PositionComp_OnPositionChanged);
        }

        protected virtual void InitToolComponents()
        {
        }

        public virtual void OnControlAcquired(MyCharacter owner)
        {
            this.m_owner = owner;
            this.InitBlockingPhysics(this.m_owner);
            using (Dictionary<string, IMyHandToolComponent>.ValueCollection.Enumerator enumerator = this.m_toolComponents.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.OnControlAcquired(owner);
                }
            }
            this.RaiseEntityEvent(MyStringHash.GetOrCompute("ControlAcquired"), new MyEntityContainerEventExtensions.ControlAcquiredParams(owner));
        }

        public virtual void OnControlReleased()
        {
            this.RaiseEntityEvent(MyStringHash.GetOrCompute("ControlReleased"), new MyEntityContainerEventExtensions.ControlReleasedParams(this.m_owner));
            if (this.m_owner != null)
            {
                this.m_owner.PositionComp.OnPositionChanged -= new Action<MyPositionComponentBase>(this.PositionComp_OnPositionChanged);
            }
            this.m_owner = null;
            this.CloseBlockingPhysics();
            using (Dictionary<string, IMyHandToolComponent>.ValueCollection.Enumerator enumerator = this.m_toolComponents.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.OnControlReleased();
                }
            }
        }

        private void PlaySound(string soundName)
        {
            MyPhysicalMaterialDefinition definition;
            if (MyDefinitionManager.Static.TryGetDefinition<MyPhysicalMaterialDefinition>(new MyDefinitionId(typeof(MyObjectBuilder_PhysicalMaterialDefinition), this.m_physItemDef.PhysicalMaterial), out definition))
            {
                MySoundPair pair;
                bool? nullable;
                if (definition.GeneralSounds.TryGetValue(MyStringId.GetOrCompute(soundName), out pair) && !pair.SoundId.IsNull)
                {
                    nullable = null;
                    this.m_soundEmitter.PlaySound(pair, false, false, false, false, false, nullable);
                }
                else
                {
                    MySoundPair pair2;
                    if (!this.m_toolSounds.TryGetValue(soundName, out pair2))
                    {
                        pair2 = new MySoundPair(soundName, true);
                        this.m_toolSounds.Add(soundName, pair2);
                    }
                    nullable = null;
                    this.m_soundEmitter.PlaySound(pair2, false, false, false, false, false, nullable);
                }
            }
        }

        private void PositionComp_OnPositionChanged(MyPositionComponentBase obj)
        {
        }

        public virtual void Shoot(MyShootActionEnum shootAction, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            MyTuple<ushort, MyStringHash> tuple;
            this.m_shotToolAction = null;
            this.m_wasShooting = false;
            this.m_swingSoundPlayed = false;
            this.m_isHit = false;
            if (!string.IsNullOrEmpty(gunAction))
            {
                if (shootAction == MyShootActionEnum.PrimaryAction)
                {
                    this.GetPreferredToolAction(this.m_toolItemDef.PrimaryActions, gunAction, out this.m_primaryToolAction, out this.m_primaryHitCondition);
                }
                else if (shootAction == MyShootActionEnum.SecondaryAction)
                {
                    this.GetPreferredToolAction(this.m_toolItemDef.SecondaryActions, gunAction, out this.m_secondaryToolAction, out this.m_secondaryHitCondition);
                }
            }
            if (shootAction == MyShootActionEnum.PrimaryAction)
            {
                this.m_shotToolAction = this.m_primaryToolAction;
                this.m_shotHitCondition = this.m_primaryHitCondition;
            }
            else if (shootAction == MyShootActionEnum.SecondaryAction)
            {
                this.m_shotToolAction = this.m_secondaryToolAction;
                this.m_shotHitCondition = this.m_secondaryHitCondition;
            }
            if ((!string.IsNullOrEmpty(this.m_shotHitCondition.StatsAction) && (this.m_owner.StatComp != null)) && !this.m_owner.StatComp.CanDoAction(this.m_shotHitCondition.StatsAction, out tuple, false))
            {
                if (((MySession.Static != null) && (ReferenceEquals(MySession.Static.LocalCharacter, this.m_owner) && (tuple.Item1 == 4))) && (tuple.Item2.String.CompareTo("Stamina") == 0))
                {
                    object[] arguments = new object[] { tuple.Item2 };
                    this.m_notEnoughStatNotification.SetTextFormatArguments(arguments);
                    MyHud.Notifications.Add(this.m_notEnoughStatNotification);
                }
            }
            else if (this.m_shotToolAction != null)
            {
                IMyHandToolComponent component;
                if (this.m_toolComponents.TryGetValue(this.m_shotHitCondition.Component, out component))
                {
                    component.Shoot();
                }
                MyFrameOption stayOnLastFrame = MyFrameOption.StayOnLastFrame;
                if (this.m_shotToolAction.Value.HitDuration == 0f)
                {
                    stayOnLastFrame = MyFrameOption.JustFirstFrame;
                }
                this.m_owner.StopUpperCharacterAnimation(0.1f);
                this.m_owner.PlayCharacterAnimation(this.m_shotHitCondition.Animation, MyBlendOption.Immediate, stayOnLastFrame, 0.2f, this.m_shotHitCondition.AnimationTimeScale, false, null, true);
                this.m_owner.TriggerCharacterAnimationEvent(this.m_shotHitCondition.Animation.ToLower(), false);
                if (this.m_owner.StatComp != null)
                {
                    if (!string.IsNullOrEmpty(this.m_shotHitCondition.StatsAction))
                    {
                        this.m_owner.StatComp.DoAction(this.m_shotHitCondition.StatsAction);
                    }
                    if (!string.IsNullOrEmpty(this.m_shotHitCondition.StatsModifier))
                    {
                        this.m_owner.StatComp.ApplyModifier(this.m_shotHitCondition.StatsModifier);
                    }
                }
                this.Physics.Enabled = this.m_shotToolAction.Value.Name == BlockId;
                this.m_lastShot = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            }
        }

        public void ShootFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public bool ShouldEndShootOnPause(MyShootActionEnum action) => 
            true;

        internal void StopShooting(float hitDelaySec)
        {
            if (this.IsShooting)
            {
                this.m_lastHit = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_hitDelay = (int) (hitDelaySec * 1000f);
                this.m_owner.PlayCharacterAnimation(this.m_shotHitCondition.Animation, MyBlendOption.Immediate, MyFrameOption.JustFirstFrame, 0.2f, this.m_shotHitCondition.AnimationTimeScale, false, null, true);
                this.m_shotToolAction = null;
                this.m_wasShooting = false;
            }
        }

        public void StopShooting(MyEntity attacker)
        {
            if (this.IsShooting)
            {
                float num = 0f;
                MyCharacter character = attacker as MyCharacter;
                if (character != null)
                {
                    MyHandToolBase currentWeapon = character.CurrentWeapon as MyHandToolBase;
                    if ((currentWeapon != null) && (currentWeapon.m_shotToolAction != null))
                    {
                        num = currentWeapon.m_shotToolAction.Value.HitDuration - (MySandboxGame.TotalGamePlayTimeInMilliseconds - (((float) currentWeapon.m_lastShot) / 1000f));
                    }
                }
                float num2 = (num > 0f) ? num : 0.4f;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, float>(s => new Action<long, float>(MyHandToolBase.StopShootingRequest), base.EntityId, num2, targetEndpoint, position);
                this.StopShooting(num2);
            }
        }

        [Event(null, 0x363), Reliable, Broadcast]
        private static void StopShootingRequest(long entityId, float attackDelay)
        {
            MyEntity entity = null;
            MyEntities.TryGetEntityById(entityId, out entity, false);
            MyHandToolBase base2 = entity as MyHandToolBase;
            if (base2 != null)
            {
                base2.StopShooting(attackDelay);
            }
        }

        public bool SupressShootAnimation() => 
            false;

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            bool isShooting = this.IsShooting;
            if ((!this.m_isHit && this.IsShooting) && ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastShot) > (this.m_shotToolAction.Value.HitStart * 1000f)))
            {
                IMyHandToolComponent component;
                if (this.m_toolComponents.TryGetValue(this.m_shotHitCondition.Component, out component))
                {
                    MyCharacterDetectorComponent detectorComponent = this.m_owner.Components.Get<MyCharacterDetectorComponent>();
                    if (detectorComponent != null)
                    {
                        if ((this.m_shotToolAction.Value.CustomShapeRadius > 0f) && (detectorComponent is MyCharacterShapecastDetectorComponent))
                        {
                            MyCharacterShapecastDetectorComponent component1 = detectorComponent as MyCharacterShapecastDetectorComponent;
                            component1.ShapeRadius = this.m_shotToolAction.Value.CustomShapeRadius;
                            component1.DoDetectionModel();
                            component1.ShapeRadius = 0.1f;
                        }
                        if (detectorComponent.DetectedEntity != null)
                        {
                            MyHitInfo hitInfo = new MyHitInfo {
                                Position = detectorComponent.HitPosition,
                                Normal = detectorComponent.HitNormal,
                                ShapeKey = detectorComponent.ShapeKey
                            };
                            bool isBlock = false;
                            float hitEfficiency = 1f;
                            bool flag3 = false;
                            bool flag1 = this.CanHit(component, detectorComponent, ref isBlock, out hitEfficiency);
                            if (flag1)
                            {
                                if (!string.IsNullOrEmpty(this.m_shotToolAction.Value.StatsEfficiency) && (this.Owner.StatComp != null))
                                {
                                    hitEfficiency *= this.Owner.StatComp.GetEfficiencyModifier(this.m_shotToolAction.Value.StatsEfficiency);
                                }
                                float efficiency = this.m_shotToolAction.Value.Efficiency * hitEfficiency;
                                MyHandToolBase detectedEntity = detectorComponent.DetectedEntity as MyHandToolBase;
                                if (!isBlock || (detectedEntity == null))
                                {
                                    flag3 = component.Hit((MyEntity) detectorComponent.DetectedEntity, hitInfo, detectorComponent.ShapeKey, efficiency);
                                }
                                else
                                {
                                    flag3 = component.Hit(detectedEntity.Owner, hitInfo, detectorComponent.ShapeKey, efficiency);
                                }
                                if ((flag3 && Sync.IsServer) && (this.Owner.StatComp != null))
                                {
                                    if (!string.IsNullOrEmpty(this.m_shotHitCondition.StatsActionIfHit))
                                    {
                                        this.Owner.StatComp.DoAction(this.m_shotHitCondition.StatsActionIfHit);
                                    }
                                    if (!string.IsNullOrEmpty(this.m_shotHitCondition.StatsModifierIfHit))
                                    {
                                        this.Owner.StatComp.ApplyModifier(this.m_shotHitCondition.StatsModifierIfHit);
                                    }
                                }
                            }
                            if (flag1 | isBlock)
                            {
                                if (!string.IsNullOrEmpty(this.m_shotToolAction.Value.HitSound))
                                {
                                    this.PlaySound(this.m_shotToolAction.Value.HitSound);
                                }
                                else
                                {
                                    MyStringId hit = MyMaterialPropertiesHelper.CollisionType.Hit;
                                    bool flag4 = false;
                                    if (MyAudioComponent.PlayContactSound(base.EntityId, m_hitCue, detectorComponent.HitPosition, this.m_toolItemDef.PhysicalMaterial, detectorComponent.HitMaterial, 1f, null, null, 0f))
                                    {
                                        flag4 = true;
                                    }
                                    else if (MyAudioComponent.PlayContactSound(base.EntityId, m_startCue, detectorComponent.HitPosition, this.m_toolItemDef.PhysicalMaterial, detectorComponent.HitMaterial, 1f, null, null, 0f))
                                    {
                                        flag4 = true;
                                        hit = MyMaterialPropertiesHelper.CollisionType.Start;
                                    }
                                    if (flag4)
                                    {
                                        MyMaterialPropertiesHelper.Static.TryCreateCollisionEffect(hit, detectorComponent.HitPosition, detectorComponent.HitNormal, this.m_toolItemDef.PhysicalMaterial, detectorComponent.HitMaterial, null);
                                    }
                                }
                                this.RaiseEntityEvent(MyStringHash.GetOrCompute("Hit"), new MyEntityContainerEventExtensions.HitParams(MyStringHash.GetOrCompute(this.m_shotHitCondition.Component), detectorComponent.HitMaterial));
                                this.m_soundEmitter.StopSound(true, true);
                            }
                        }
                    }
                }
                this.m_isHit = true;
            }
            if ((!this.m_swingSoundPlayed && (this.IsShooting && !this.m_isHit)) && ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastShot) > (this.m_shotToolAction.Value.SwingSoundStart * 1000f)))
            {
                if (!string.IsNullOrEmpty(this.m_shotToolAction.Value.SwingSound))
                {
                    this.PlaySound(this.m_shotToolAction.Value.SwingSound);
                }
                this.m_swingSoundPlayed = true;
            }
            if (!isShooting && this.m_wasShooting)
            {
                this.m_owner.TriggerCharacterAnimationEvent("stop_tool_action", false);
                this.m_owner.StopUpperCharacterAnimation(0.4f);
                this.m_shotToolAction = null;
            }
            this.m_wasShooting = isShooting;
            if (this.m_owner != null)
            {
                MatrixD worldMatrix = MatrixD.CreateWorld(((MyEntity) this.m_owner.CurrentWeapon).PositionComp.GetPosition(), this.m_owner.WorldMatrix.Forward, this.m_owner.WorldMatrix.Up);
                ((MyBlockingBody) this.Physics).SetWorldMatrix(worldMatrix);
            }
            using (Dictionary<string, IMyHandToolComponent>.ValueCollection.Enumerator enumerator = this.m_toolComponents.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Update();
                }
            }
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            this.GetMostEffectiveToolAction(this.m_toolItemDef.PrimaryActions, out this.m_primaryToolAction, out this.m_primaryHitCondition);
            this.GetMostEffectiveToolAction(this.m_toolItemDef.SecondaryActions, out this.m_secondaryToolAction, out this.m_secondaryHitCondition);
            if (ReferenceEquals(MySession.Static.ControlledEntity, this.m_owner))
            {
                MyCharacterDetectorComponent component = this.m_owner.Components.Get<MyCharacterDetectorComponent>();
                bool flag = false;
                float maxValue = float.MaxValue;
                if (component != null)
                {
                    flag = component.DetectedEntity != null;
                    maxValue = Vector3.Distance((Vector3) component.HitPosition, (Vector3) base.PositionComp.GetPosition());
                }
                if (maxValue > this.m_toolItemDef.HitDistance)
                {
                    flag = false;
                }
                if ((this.m_primaryToolAction != null) && ((this.m_primaryHitCondition.EntityType != null) | flag))
                {
                    MyHud.Crosshair.ChangeDefaultSprite(this.m_primaryToolAction.Value.Crosshair, 0f);
                }
                else if ((this.m_secondaryToolAction != null) && ((this.m_secondaryHitCondition.EntityType != null) | flag))
                {
                    MyHud.Crosshair.ChangeDefaultSprite(this.m_secondaryToolAction.Value.Crosshair, 0f);
                }
                else
                {
                    MyHud.Crosshair.ChangeDefaultSprite(MyHudTexturesEnum.crosshair, 0f);
                }
            }
        }

        public void UpdateSoundEmitter()
        {
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.Update();
            }
        }

        public MyObjectBuilder_PhysicalGunObject PhysicalObject { get; private set; }

        public MyPhysicsBody Physics
        {
            get => 
                (base.Physics as MyPhysicsBody);
            set => 
                (base.Physics = value);
        }

        public bool IsShooting =>
            ((this.m_shotToolAction != null) ? ((this.m_lastShot <= MySandboxGame.TotalGamePlayTimeInMilliseconds) && (((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastShot) < (this.m_shotToolAction.Value.HitDuration * 1000f)) || (this.m_shotToolAction.Value.HitDuration == 0f))) : false);

        public int ShootDirectionUpdateTime =>
            0;

        public bool EnabledInWorldRules =>
            true;

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

        public MyDefinitionId DefinitionId =>
            this.m_handItemDefinitionId;

        public MyToolBase GunBase { get; private set; }

        public virtual bool ForceAnimationInsteadOfIK =>
            true;

        public bool IsBlocking =>
            ((this.m_shotToolAction != null) && (this.m_shotToolAction.Value.Name == MyStringId.GetOrCompute("Block")));

        public MyPhysicalItemDefinition PhysicalItemDefinition =>
            this.m_physItemDef;

        public MyCharacter Owner =>
            this.m_owner;

        public long OwnerId =>
            ((this.m_owner == null) ? 0L : this.m_owner.EntityId);

        public long OwnerIdentityId =>
            ((this.m_owner == null) ? 0L : this.m_owner.GetPlayerIdentityId());

        public bool IsSkinnable =>
            false;

        public int CurrentAmmunition { get; set; }

        public int CurrentMagazineAmmunition { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyHandToolBase.<>c <>9 = new MyHandToolBase.<>c();
            public static Func<IMyEventOwner, Action<long, float>> <>9__93_0;

            internal Action<long, float> <StopShooting>b__93_0(IMyEventOwner s) => 
                new Action<long, float>(MyHandToolBase.StopShootingRequest);
        }

        public class MyBlockingBody : MyPhysicsBody
        {
            public MyBlockingBody(MyHandToolBase tool, MyEntity owner) : base(owner, RigidBodyFlag.RBF_KINEMATIC)
            {
                this.HandTool = tool;
            }

            public override void OnMotion(HkRigidBody rbo, float step, bool fromParent)
            {
            }

            public override void OnWorldPositionChanged(object source)
            {
            }

            public void SetWorldMatrix(MatrixD worldMatrix)
            {
                Vector3D objectOffset = MyPhysics.GetObjectOffset(base.ClusterObjectID);
                Matrix m = Matrix.CreateWorld((Vector3) (worldMatrix.Translation - objectOffset), (Vector3) worldMatrix.Forward, (Vector3) worldMatrix.Up);
                if (this.RigidBody != null)
                {
                    this.RigidBody.SetWorldMatrix(m);
                }
            }

            public MyHandToolBase HandTool { get; private set; }
        }
    }
}

