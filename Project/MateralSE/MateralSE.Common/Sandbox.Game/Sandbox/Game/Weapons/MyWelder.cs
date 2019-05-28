namespace Sandbox.Game.Weapons
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Weapons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_Welder), true)]
    public class MyWelder : MyEngineerToolBase, IMyWelder, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyEngineerToolBase, IMyHandheldGunObject<MyToolBase>, IMyGunObject<MyToolBase>
    {
        private MySoundPair m_weldSoundIdle;
        private MySoundPair m_weldSoundWeld;
        private MySoundPair m_weldSoundFlame;
        public static readonly float WELDER_AMOUNT_PER_SECOND = 1f;
        public static readonly float WELDER_MAX_REPAIR_BONE_MOVEMENT_SPEED = 0.6f;
        public static MatrixD WELDER_ANGLE = MatrixD.CreateRotationX(0.49000000953674316);
        private static int SUPRESS_TIME_LIMIT = 180;
        private static MyHudNotificationBase m_missingComponentNotification = new MyHudNotification(MyCommonTexts.NotificationMissingComponentToPlaceBlockFormat, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
        private MyHudNotification m_safezoneNotification;
        private static MyDefinitionId m_physicalItemId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), "WelderItem");
        private MySlimBlock m_failedBlock;
        private bool m_playedFailSound;
        private MySlimBlock m_failedBlockSound;
        private float m_lastWeldingDistance;
        private bool m_lastWeldingDistanceCheck;
        private int m_timedShootSupression;
        private Vector3I m_targetProjectionCube;
        private MyCubeGrid m_targetProjectionGrid;
        private MyParticleEffect m_flameEffect;
        private string m_flameEffectName;
        private bool m_showContactSpark;

        public MyWelder() : base(250)
        {
            this.m_weldSoundIdle = new MySoundPair("ToolPlayWeldIdle", true);
            this.m_weldSoundWeld = new MySoundPair("ToolPlayWeldMetal", true);
            this.m_weldSoundFlame = new MySoundPair("ArcShipSmNuclearLrg", true);
            this.m_lastWeldingDistance = float.MaxValue;
            this.m_flameEffectName = "WelderFlame";
            this.m_showContactSpark = true;
            base.HasCubeHighlight = true;
            base.HighlightColor = Color.Green * 0.75f;
            base.HighlightMaterial = MyStringId.GetOrCompute("GizmoDrawLine");
            base.SecondaryLightIntensityLower = 0.4f;
            base.SecondaryLightIntensityUpper = 0.4f;
            base.SecondaryEffectName = "WelderContactPoint";
            base.HasSecondaryEffect = false;
        }

        protected override void AddHudInfo()
        {
        }

        public override void BeginFailReaction(MyShootActionEnum action, MyGunStatusEnum status)
        {
            base.BeginFailReaction(action, status);
            this.FillStockpile();
        }

        public override void BeginFailReactionLocal(MyShootActionEnum action, MyGunStatusEnum status)
        {
            if (status == MyGunStatusEnum.SafeZoneDenied)
            {
                if (this.m_safezoneNotification == null)
                {
                    this.m_safezoneNotification = new MyHudNotification(MyCommonTexts.SafeZone_WeldingDisabled, 0x7d0, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                }
                MyHud.Notifications.Add(this.m_safezoneNotification);
            }
        }

        public bool CanDoubleClickToStick(MyShootActionEnum action) => 
            true;

        public override bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            if (!MySessionComponentSafeZones.IsActionAllowed(base.Owner, MySafeZoneAction.Welding, 0L))
            {
                status = MyGunStatusEnum.SafeZoneDenied;
                return false;
            }
            if (!base.CanShoot(action, shooter, out status))
            {
                return false;
            }
            MySlimBlock targetBlock = base.GetTargetBlock();
            MyCharacter owner = base.Owner;
            if (((targetBlock != null) && (!targetBlock.CanContinueBuild(owner.GetInventory(0)) && (!targetBlock.IsFullIntegrity && ((base.Owner != null) && (ReferenceEquals(base.Owner, MySession.Static.LocalCharacter) && (MySession.Static.Settings.GameMode == MyGameModeEnum.Survival)))))) && !MySession.Static.CreativeToolsEnabled(Sync.MyId))
            {
                int num;
                int num2;
                targetBlock.ComponentStack.GetMissingInfo(out num, out num2);
                MyComponentStack.GroupInfo groupInfo = targetBlock.ComponentStack.GetGroupInfo(num);
                base.MarkMissingComponent(num);
                object[] arguments = new object[] { $"{groupInfo.Component.DisplayNameText} ({num2}x)", targetBlock.BlockDefinition.DisplayNameText.ToString() };
                m_missingComponentNotification.SetTextFormatArguments(arguments);
                MyHud.Notifications.Add(m_missingComponentNotification);
                if ((this.m_playedFailSound && !ReferenceEquals(this.m_failedBlockSound, targetBlock)) || !this.m_playedFailSound)
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                    this.m_playedFailSound = true;
                    this.m_failedBlockSound = targetBlock;
                }
            }
            return true;
        }

        public override bool CanStartEffect() => 
            this.m_showContactSpark;

        private bool CanWeld(MySlimBlock block) => 
            (MySessionComponentSafeZones.IsActionAllowed(block.WorldAABB, MySafeZoneAction.Welding, 0L) ? (!block.IsFullIntegrity || block.HasDeformation) : false);

        private void CheckProjection()
        {
            MySlimBlock targetBlock = base.GetTargetBlock();
            if ((targetBlock != null) && this.CanWeld(targetBlock))
            {
                this.m_targetProjectionGrid = null;
            }
            else
            {
                ProjectionRaycastData data = this.FindProjectedBlock();
                if (data.raycastResult != BuildCheckResult.NotFound)
                {
                    VRageMath.Vector4? nullable;
                    MyStringId? nullable2;
                    if (data.raycastResult == BuildCheckResult.OK)
                    {
                        nullable = null;
                        MyCubeBuilder.DrawSemiTransparentBox(data.hitCube.CubeGrid, data.hitCube, Color.Green.ToVector4(), true, new MyStringId?(MyStringId.GetOrCompute("GizmoDrawLine")), nullable);
                        this.m_targetProjectionCube = data.hitCube.Position;
                        this.m_targetProjectionGrid = data.hitCube.CubeGrid;
                        return;
                    }
                    if ((data.raycastResult == BuildCheckResult.IntersectedWithGrid) || (data.raycastResult == BuildCheckResult.IntersectedWithSomethingElse))
                    {
                        nullable2 = null;
                        nullable = null;
                        MyCubeBuilder.DrawSemiTransparentBox(data.hitCube.CubeGrid, data.hitCube, Color.Red.ToVector4(), true, nullable2, nullable);
                    }
                    else if (data.raycastResult == BuildCheckResult.NotConnected)
                    {
                        nullable2 = null;
                        nullable = null;
                        MyCubeBuilder.DrawSemiTransparentBox(data.hitCube.CubeGrid, data.hitCube, Color.Yellow.ToVector4(), true, nullable2, nullable);
                    }
                }
                this.m_targetProjectionGrid = null;
            }
        }

        protected override void Closing()
        {
            base.Closing();
            if (this.m_flameEffect != null)
            {
                this.m_flameEffect.Stop(true);
                this.m_flameEffect = null;
            }
        }

        protected override void DrawHud()
        {
            MyHud.BlockInfo.Visible = false;
            Vector3I targetProjectionCube = this.m_targetProjectionCube;
            if (this.m_targetProjectionGrid == null)
            {
                base.DrawHud();
            }
            else
            {
                MySlimBlock cubeBlock = this.m_targetProjectionGrid.GetCubeBlock(this.m_targetProjectionCube);
                if (cubeBlock == null)
                {
                    base.DrawHud();
                }
                else
                {
                    if (MyFakes.ENABLE_COMPOUND_BLOCKS && (cubeBlock.FatBlock is MyCompoundCubeBlock))
                    {
                        MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                        if (fatBlock.GetBlocksCount() > 0)
                        {
                            cubeBlock = fatBlock.GetBlocks().First<MySlimBlock>();
                        }
                    }
                    MyHud.BlockInfo.Visible = true;
                    MyHud.BlockInfo.MissingComponentIndex = 0;
                    MyHud.BlockInfo.BlockName = cubeBlock.BlockDefinition.DisplayNameText;
                    MyHud.BlockInfo.SetContextHelp(cubeBlock.BlockDefinition);
                    MyHud.BlockInfo.PCUCost = cubeBlock.BlockDefinition.PCU;
                    MyHud.BlockInfo.BlockIcons = cubeBlock.BlockDefinition.Icons;
                    MyHud.BlockInfo.BlockIntegrity = 0.01f;
                    MyHud.BlockInfo.CriticalIntegrity = cubeBlock.BlockDefinition.CriticalIntegrityRatio;
                    MyHud.BlockInfo.CriticalComponentIndex = cubeBlock.BlockDefinition.CriticalGroup;
                    MyHud.BlockInfo.OwnershipIntegrity = cubeBlock.BlockDefinition.OwnershipIntegrityRatio;
                    MyHud.BlockInfo.BlockBuiltBy = cubeBlock.BuiltBy;
                    MyHud.BlockInfo.GridSize = cubeBlock.CubeGrid.GridSizeEnum;
                    MyHud.BlockInfo.Components.Clear();
                    for (int i = 0; i < cubeBlock.ComponentStack.GroupCount; i++)
                    {
                        MyComponentStack.GroupInfo groupInfo = cubeBlock.ComponentStack.GetGroupInfo(i);
                        MyHudBlockInfo.ComponentInfo item = new MyHudBlockInfo.ComponentInfo {
                            DefinitionId = groupInfo.Component.Id,
                            ComponentName = groupInfo.Component.DisplayNameText,
                            Icons = groupInfo.Component.Icons,
                            TotalCount = groupInfo.TotalCount,
                            MountedCount = 0,
                            StockpileCount = 0
                        };
                        MyHud.BlockInfo.Components.Add(item);
                    }
                }
            }
        }

        public override void EndShoot(MyShootActionEnum action)
        {
            if (base.m_activated)
            {
                MyAnalyticsHelper.ReportActivityEnd(base.Owner, "Welding");
            }
            this.m_playedFailSound = false;
            this.m_failedBlockSound = null;
            base.EndShoot(action);
        }

        private void FillStockpile()
        {
            MySlimBlock targetBlock = base.GetTargetBlock();
            if (targetBlock != null)
            {
                if (Sync.IsServer)
                {
                    targetBlock.MoveItemsToConstructionStockpile(base.CharacterInventory);
                }
                else
                {
                    targetBlock.RequestFillStockpile(base.CharacterInventory);
                }
            }
        }

        private ProjectionRaycastData FindProjectedBlock()
        {
            if (base.Owner != null)
            {
                MyCubeGrid grid;
                Vector3I vectori;
                double num2;
                Vector3D center = base.m_raycastComponent.Caster.Center;
                Vector3D vectord2 = base.m_raycastComponent.Caster.FrontPoint - base.m_raycastComponent.Caster.Center;
                vectord2.Normalize();
                Vector3D to = center + (vectord2 * (base.DEFAULT_REACH_DISTANCE * base.m_distanceMultiplier));
                LineD line = new LineD(center, to);
                if (MyCubeGrid.GetLineIntersection(ref line, out grid, out vectori, out num2) && (grid.Projector != null))
                {
                    MyProjectorBase projector = grid.Projector;
                    List<MyCube> list = grid.RayCastBlocksAllOrdered(center, to);
                    ProjectionRaycastData? nullable = null;
                    int num3 = list.Count - 1;
                    while (true)
                    {
                        if (num3 < 0)
                        {
                            if (nullable == null)
                            {
                                break;
                            }
                            return nullable.Value;
                        }
                        MyCube cube = list[num3];
                        BuildCheckResult result = projector.CanBuild(cube.CubeBlock, true);
                        if (result != BuildCheckResult.OK)
                        {
                            if (result == BuildCheckResult.AlreadyBuilt)
                            {
                                nullable = null;
                            }
                        }
                        else
                        {
                            ProjectionRaycastData data = new ProjectionRaycastData {
                                raycastResult = result,
                                hitCube = cube.CubeBlock,
                                cubeProjector = projector
                            };
                            nullable = new ProjectionRaycastData?(data);
                        }
                        num3--;
                    }
                }
            }
            return new ProjectionRaycastData { raycastResult = BuildCheckResult.NotFound };
        }

        protected override MatrixD GetEffectMatrix(float muzzleOffset, MyEngineerToolBase.EffectType effectType)
        {
            Vector3D vectord5;
            Vector3D forward = base.PositionComp.WorldMatrix.Forward;
            Vector3D muzzleWorldPosition = base.m_gunBase.GetMuzzleWorldPosition();
            if (effectType != MyEngineerToolBase.EffectType.Effect)
            {
                return MatrixD.CreateWorld(muzzleWorldPosition, forward, base.PositionComp.WorldMatrix.Up);
            }
            Vector3D vectord3 = Vector3D.Rotate(WELDER_ANGLE.Forward, base.PositionComp.WorldMatrix);
            Vector3D vectord4 = muzzleWorldPosition + (0.05000000074505806 * base.PositionComp.WorldMatrix.Up);
            this.m_lastWeldingDistance = Vector3.Dot((Vector3) (base.m_raycastComponent.HitPosition - vectord4), (Vector3) forward);
            MyPhysics.HitInfo? nullable = MyPhysics.CastRay(vectord4 - (0.5 * vectord3), vectord4 + (1.5 * vectord3), 15);
            if (nullable == null)
            {
                vectord5 = vectord4 + (0.10000000149011612 * vectord3);
            }
            else
            {
                float num = Vector3.Dot((Vector3) (nullable.Value.Position - vectord4), (Vector3) vectord3);
                vectord5 = (num <= 0.1f) ? (vectord4 + (num * vectord3)) : (vectord4 + (0.10000000149011612 * vectord3));
            }
            return MatrixD.CreateWorld(vectord5, -vectord3, base.PositionComp.WorldMatrix.Up);
        }

        private MyProjectorBase GetProjector(MySlimBlock block)
        {
            MySlimBlock block2 = block.CubeGrid.GetBlocks().FirstOrDefault<MySlimBlock>(b => b.FatBlock is MyProjectorBase);
            return ((block2 == null) ? null : (block2.FatBlock as MyProjectorBase));
        }

        protected override MySlimBlock GetTargetBlockForShoot()
        {
            MySlimBlock targetBlock = base.GetTargetBlock();
            if ((targetBlock == null) || !this.ShowContactSpark)
            {
                return null;
            }
            return targetBlock;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            m_physicalItemId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), "WelderItem");
            if ((objectBuilder.SubtypeName != null) && (objectBuilder.SubtypeName.Length > 0))
            {
                m_physicalItemId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), objectBuilder.SubtypeName + "Item");
            }
            base.PhysicalObject = (MyObjectBuilder_PhysicalGunObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) m_physicalItemId);
            base.Init(objectBuilder, m_physicalItemId);
            MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(m_physicalItemId);
            float? scale = null;
            this.Init(null, physicalItemDefinition.Model, null, scale, null);
            base.Render.CastShadows = true;
            base.Render.NeedsResolveCastShadow = false;
            base.PhysicalObject.GunEntity = (MyObjectBuilder_EntityBase) objectBuilder.Clone();
            base.PhysicalObject.GunEntity.EntityId = base.EntityId;
            MyWelderDefinition definition2 = MyDefinitionManager.Static.TryGetHandItemForPhysicalItem(m_physicalItemId) as MyWelderDefinition;
            if (definition2 != null)
            {
                this.m_flameEffectName = definition2.FlameEffect;
            }
            foreach (ToolSound sound in base.m_handItemDef.ToolSounds)
            {
                if (sound.type == null)
                {
                    continue;
                }
                if ((sound.subtype != null) && ((sound.sound != null) && sound.type.Equals("Main")))
                {
                    if (sound.subtype.Equals("Idle"))
                    {
                        this.m_weldSoundIdle = new MySoundPair(sound.sound, true);
                    }
                    if (sound.subtype.Equals("Weld"))
                    {
                        this.m_weldSoundWeld = new MySoundPair(sound.sound, true);
                    }
                    if (sound.subtype.Equals("Flame"))
                    {
                        this.m_weldSoundFlame = new MySoundPair(sound.sound, true);
                    }
                }
            }
        }

        protected override void RemoveHudInfo()
        {
        }

        public override void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            MyAnalyticsHelper.ReportActivityStartIf(!base.m_activated, base.Owner, "Welding", "Character", "HandTools", "Welder", true);
            base.Shoot(action, direction, overrideWeaponPos, gunAction);
            this.ShowContactSpark = false;
            if ((action != MyShootActionEnum.PrimaryAction) || !base.IsPreheated)
            {
                if ((action == MyShootActionEnum.SecondaryAction) && Sync.IsServer)
                {
                    this.FillStockpile();
                }
            }
            else
            {
                MySlimBlock targetBlock = base.GetTargetBlock();
                if (targetBlock == null)
                {
                    this.m_lastWeldingDistance = float.MaxValue;
                    this.m_lastWeldingDistanceCheck = false;
                }
                if (((targetBlock != null) && base.m_activated) && this.CanWeld(targetBlock))
                {
                    if (MySession.Static.CheckResearchAndNotify(base.Owner.GetPlayerIdentityId(), targetBlock.BlockDefinition.Id))
                    {
                        this.Weld();
                    }
                }
                else if (ReferenceEquals(base.Owner, MySession.Static.LocalCharacter))
                {
                    ProjectionRaycastData data = this.FindProjectedBlock();
                    if ((data.raycastResult == BuildCheckResult.OK) && MySession.Static.CheckResearchAndNotify(base.Owner.GetPlayerIdentityId(), data.hitCube.BlockDefinition.Id))
                    {
                        MyPlayer.PlayerId id;
                        MyPlayer player;
                        bool creativeMode = MySession.Static.CreativeMode;
                        if (MySession.Static.Players.TryGetPlayerId(base.OwnerIdentityId, out id) && MySession.Static.Players.TryGetPlayerById(id, out player))
                        {
                            creativeMode |= MySession.Static.CreativeToolsEnabled(Sync.MyId);
                        }
                        if (MySession.Static.CheckLimitsAndNotify((targetBlock != null) ? targetBlock.BuiltBy : base.Owner.ControllerInfo.Controller.Player.Identity.IdentityId, data.hitCube.BlockDefinition.BlockPairName, creativeMode ? data.hitCube.BlockDefinition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST, 0, data.cubeProjector.CubeGrid.BlocksCount, null))
                        {
                            if ((MySession.Static.CreativeMode || (MyBlockBuilderBase.SpectatorIsBuilding || base.Owner.CanStartConstruction(data.hitCube.BlockDefinition))) || MySession.Static.CreativeToolsEnabled(Sync.MyId))
                            {
                                data.cubeProjector.Build(data.hitCube, base.Owner.ControllerInfo.Controller.Player.Identity.IdentityId, base.Owner.EntityId, true, base.Owner.ControllerInfo.Controller.Player.Identity.IdentityId);
                            }
                            else
                            {
                                MyBlockPlacerBase.OnMissingComponents(data.hitCube.BlockDefinition);
                            }
                        }
                    }
                }
            }
        }

        protected override bool ShouldBePowered() => 
            base.ShouldBePowered();

        public bool ShouldEndShootOnPause(MyShootActionEnum action) => 
            (!base.m_isActionDoubleClicked.ContainsKey(action) ? true : !base.m_isActionDoubleClicked[action]);

        private void ShowContactSparkChanged()
        {
            if (!this.m_showContactSpark)
            {
                base.StopEffect();
            }
        }

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
            MySoundPair soundId = effect ? this.m_weldSoundWeld : this.m_weldSoundFlame;
            if ((base.m_soundEmitter.Sound == null) || !base.m_soundEmitter.Sound.IsPlaying)
            {
                nullable = null;
                base.m_soundEmitter.PlaySound(soundId, true, true, flag, false, false, nullable);
            }
            else if (flag != base.m_soundEmitter.Force2D)
            {
                nullable = null;
                base.m_soundEmitter.PlaySound(soundId, true, true, flag, false, false, nullable);
            }
            else
            {
                nullable = null;
                base.m_soundEmitter.PlaySingleSound(soundId, true, true, false, nullable);
            }
        }

        protected override void StopLoopSound()
        {
            this.StopSound();
        }

        protected override void StopSound()
        {
            base.m_soundEmitter.StopSound(true, true);
        }

        public override bool SupressShootAnimation()
        {
            bool flag = this.m_lastWeldingDistance < 0.05f;
            if ((this.m_lastWeldingDistanceCheck != flag) && (this.m_timedShootSupression < SUPRESS_TIME_LIMIT))
            {
                this.m_timedShootSupression = (this.m_timedShootSupression <= 0) ? (this.m_timedShootSupression + SUPRESS_TIME_LIMIT) : (this.m_timedShootSupression + ((int) (MyRandom.Instance.GetRandomFloat(0.8f, 1.6f) * SUPRESS_TIME_LIMIT)));
            }
            this.m_lastWeldingDistanceCheck = flag;
            return (flag || (this.m_timedShootSupression > SUPRESS_TIME_LIMIT));
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (this.m_timedShootSupression > 0)
            {
                this.m_timedShootSupression--;
            }
            if ((base.Owner != null) && ReferenceEquals(base.Owner, MySession.Static.LocalCharacter))
            {
                this.CheckProjection();
            }
            if ((base.Owner == null) || !ReferenceEquals(MySession.Static.ControlledEntity, base.Owner))
            {
                this.RemoveHudInfo();
            }
            this.UpdateFlameEffect();
        }

        private void UpdateFlameEffect()
        {
            MyShootActionEnum? effectAction = base.EffectAction;
            MyShootActionEnum primaryAction = MyShootActionEnum.PrimaryAction;
            if (!((((MyShootActionEnum) effectAction.GetValueOrDefault()) == primaryAction) & (effectAction != null)))
            {
                effectAction = base.EffectAction;
                primaryAction = MyShootActionEnum.SecondaryAction;
                if (!((((MyShootActionEnum) effectAction.GetValueOrDefault()) == primaryAction) & (effectAction != null)))
                {
                    if (this.m_flameEffect != null)
                    {
                        this.m_flameEffect.Stop(true);
                        this.m_flameEffect = null;
                    }
                    return;
                }
            }
            if (this.m_flameEffect == null)
            {
                MyParticlesManager.TryCreateParticleEffect("WelderFlame", this.GetEffectMatrix(0f, MyEngineerToolBase.EffectType.EffectSecondary), out this.m_flameEffect);
            }
            if (this.m_flameEffect != null)
            {
                this.m_flameEffect.WorldMatrix = this.GetEffectMatrix(0f, MyEngineerToolBase.EffectType.EffectSecondary);
            }
        }

        private void Weld()
        {
            bool flag = false;
            MySlimBlock targetBlock = base.GetTargetBlock();
            if (targetBlock != null)
            {
                MyCubeBlockDefinition.PreloadConstructionModels(targetBlock.BlockDefinition);
                if (Sync.IsServer)
                {
                    targetBlock.MoveItemsToConstructionStockpile(base.CharacterInventory);
                    targetBlock.MoveUnneededItemsFromConstructionStockpile(base.CharacterInventory);
                }
                bool hasDeformation = targetBlock.HasDeformation;
                if ((hasDeformation || (targetBlock.MaxDeformation > 0f)) || !targetBlock.IsFullIntegrity)
                {
                    float maxAllowedBoneMovement = (WELDER_MAX_REPAIR_BONE_MOVEMENT_SPEED * base.ToolCooldownMs) * 0.001f;
                    if ((base.Owner != null) && (base.Owner.ControllerInfo != null))
                    {
                        bool? nullable = targetBlock.ComponentStack.WillFunctionalityRise(this.WeldAmount);
                        if (((nullable != null) && nullable.Value) && !MySession.Static.CheckLimitsAndNotify(targetBlock.BuiltBy, targetBlock.BlockDefinition.BlockPairName, targetBlock.BlockDefinition.PCU - MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST, 0, 0, null))
                        {
                            return;
                        }
                        if (Sync.IsServer)
                        {
                            flag = targetBlock.IncreaseMountLevel(this.WeldAmount, base.Owner.ControllerInfo.ControllingIdentityId, base.CharacterInventory, maxAllowedBoneMovement, false, MyOwnershipShareModeEnum.Faction, true);
                        }
                        else
                        {
                            int num1;
                            if (!targetBlock.IsFullIntegrity || targetBlock.HasDeformation)
                            {
                                num1 = (int) ReferenceEquals(this.m_failedBlockSound, null);
                            }
                            else
                            {
                                num1 = 0;
                            }
                            flag = (bool) num1;
                        }
                        flag |= hasDeformation;
                        if (((MySession.Static != null) && ReferenceEquals(base.Owner, MySession.Static.LocalCharacter)) && (MyMusicController.Static != null))
                        {
                            MyMusicController.Static.Building(250);
                        }
                    }
                }
            }
            if (Sync.IsServer)
            {
                IMyDestroyableObject targetDestroyable = base.GetTargetDestroyable();
                if ((targetDestroyable is MyCharacter) && Sync.IsServer)
                {
                    MyHitInfo? hitInfo = null;
                    targetDestroyable.DoDamage(20f, MyDamageType.Weld, true, hitInfo, base.EntityId);
                }
            }
            this.ShowContactSpark = flag;
        }

        private bool ShowContactSpark
        {
            get => 
                this.m_showContactSpark;
            set
            {
                if (this.m_showContactSpark != value)
                {
                    this.m_showContactSpark = value;
                    this.ShowContactSparkChanged();
                }
            }
        }

        public override bool IsSkinnable =>
            true;

        private float WeldAmount =>
            ((((MySession.Static.WelderSpeedMultiplier * base.m_speedMultiplier) * WELDER_AMOUNT_PER_SECOND) * base.ToolCooldownMs) / 1000f);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyWelder.<>c <>9 = new MyWelder.<>c();
            public static Func<MySlimBlock, bool> <>9__35_0;

            internal bool <GetProjector>b__35_0(MySlimBlock b) => 
                (b.FatBlock is MyProjectorBase);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProjectionRaycastData
        {
            public BuildCheckResult raycastResult;
            public MySlimBlock hitCube;
            public MyProjectorBase cubeProjector;
            public ProjectionRaycastData(BuildCheckResult result, MySlimBlock cubeBlock, MyProjectorBase projector)
            {
                this.raycastResult = result;
                this.hitCube = cubeBlock;
                this.cubeProjector = projector;
            }
        }
    }
}

