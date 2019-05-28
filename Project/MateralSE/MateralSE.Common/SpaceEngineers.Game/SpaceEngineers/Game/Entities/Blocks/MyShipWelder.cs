namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.Weapons.Guns;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_ShipWelder)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyShipWelder), typeof(Sandbox.ModAPI.Ingame.IMyShipWelder) })]
    public class MyShipWelder : MyShipToolBase, Sandbox.ModAPI.IMyShipWelder, Sandbox.ModAPI.IMyShipToolBase, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyShipToolBase, Sandbox.ModAPI.Ingame.IMyShipWelder
    {
        private static MySoundPair METAL_SOUND = new MySoundPair("ToolLrgWeldMetal", true);
        private static MySoundPair IDLE_SOUND = new MySoundPair("ToolLrgWeldIdle", true);
        private const string PARTICLE_EFFECT = "ShipWelderArc";
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_helpOthers;
        public static readonly float WELDER_AMOUNT_PER_SECOND = 4f;
        public static readonly float WELDER_MAX_REPAIR_BONE_MOVEMENT_SPEED = 0.6f;
        private Dictionary<string, int> m_missingComponents;
        private List<MyWelder.ProjectionRaycastData> m_raycastData = new List<MyWelder.ProjectionRaycastData>();
        private HashSet<MySlimBlock> m_projectedBlock = new HashSet<MySlimBlock>();
        private MyParticleEffect m_particleEffect;
        private MyFlareDefinition m_flare;
        private MyShipWelderDefinition m_welderDef;
        private Matrix m_particleDummyMatrix1;

        public MyShipWelder()
        {
            this.CreateTerminalControls();
        }

        protected override bool Activate(HashSet<MySlimBlock> targets)
        {
            bool flag = false;
            int count = targets.Count;
            this.m_missingComponents.Clear();
            foreach (MySlimBlock block in targets)
            {
                if (block.IsFullIntegrity || ReferenceEquals(block, base.SlimBlock))
                {
                    count--;
                    continue;
                }
                MyCubeBlockDefinition.PreloadConstructionModels(block.BlockDefinition);
                block.GetMissingComponents(this.m_missingComponents);
            }
            foreach (KeyValuePair<string, int> pair in this.m_missingComponents)
            {
                MyDefinitionId contentId = new MyDefinitionId(typeof(MyObjectBuilder_Component), pair.Key);
                int amount = Math.Max(pair.Value - ((int) this.GetInventory(0).GetItemAmount(contentId, MyItemFlags.None, false)), 0);
                if ((amount != 0) && (Sync.IsServer && base.UseConveyorSystem))
                {
                    MyComponentSubstitutionDefinition definition;
                    if (MyDefinitionManager.Static.GetGroupForComponent(contentId, out amount) != null)
                    {
                        MyGridConveyorSystem.ItemPullRequest(this, this.GetInventory(0), base.OwnerId, contentId, new MyFixedPoint?(pair.Value), false);
                        continue;
                    }
                    if (MyDefinitionManager.Static.TryGetComponentSubstitutionDefinition(contentId, out definition))
                    {
                        foreach (KeyValuePair<MyDefinitionId, int> pair2 in definition.ProvidingComponents)
                        {
                            MyFixedPoint point = pair.Value / pair2.Value;
                            MyGridConveyorSystem.ItemPullRequest(this, this.GetInventory(0), base.OwnerId, pair2.Key, new MyFixedPoint?(point), false);
                        }
                        continue;
                    }
                    MyGridConveyorSystem.ItemPullRequest(this, this.GetInventory(0), base.OwnerId, contentId, new MyFixedPoint?(pair.Value), false);
                }
            }
            if (Sync.IsServer)
            {
                float num3 = 0.25f / ((float) Math.Min(4, (count > 0) ? count : 1));
                foreach (MySlimBlock block2 in targets)
                {
                    if (block2.CubeGrid.Physics == null)
                    {
                        continue;
                    }
                    if (block2.CubeGrid.Physics.Enabled && !ReferenceEquals(block2, base.SlimBlock))
                    {
                        float mountAmount = (MySession.Static.WelderSpeedMultiplier * WELDER_AMOUNT_PER_SECOND) * num3;
                        bool? nullable = block2.ComponentStack.WillFunctionalityRise(mountAmount);
                        if (((nullable == null) || !nullable.Value) || MySession.Static.CheckLimitsAndNotify(MySession.Static.LocalPlayerId, block2.BlockDefinition.BlockPairName, block2.BlockDefinition.PCU - MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST, 0, 0, null))
                        {
                            if (block2.CanContinueBuild(this.GetInventory(0)))
                            {
                                flag = true;
                            }
                            block2.MoveItemsToConstructionStockpile(this.GetInventory(0));
                            block2.MoveUnneededItemsFromConstructionStockpile(this.GetInventory(0));
                            if ((block2.HasDeformation || (block2.MaxDeformation > 0.0001f)) || !block2.IsFullIntegrity)
                            {
                                float maxAllowedBoneMovement = (WELDER_MAX_REPAIR_BONE_MOVEMENT_SPEED * 250f) * 0.001f;
                                block2.IncreaseMountLevel(mountAmount, base.OwnerId, this.GetInventory(0), maxAllowedBoneMovement, (bool) this.m_helpOthers, base.IDModule.ShareMode, false);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (MySlimBlock block3 in targets)
                {
                    if (ReferenceEquals(block3, base.SlimBlock))
                    {
                        continue;
                    }
                    if (block3.CanContinueBuild(this.GetInventory(0)))
                    {
                        flag = true;
                    }
                }
            }
            this.m_missingComponents.Clear();
            if (!flag && Sync.IsServer)
            {
                MyPlayer.PlayerId id2;
                MyPlayer player;
                MyWelder.ProjectionRaycastData[] dataArray = this.FindProjectedBlocks();
                if (base.UseConveyorSystem)
                {
                    MyWelder.ProjectionRaycastData[] dataArray2 = dataArray;
                    for (int i = 0; i < dataArray2.Length; i++)
                    {
                        MyCubeBlockDefinition.Component[] components = dataArray2[i].hitCube.BlockDefinition.Components;
                        if ((components != null) && (components.Length != 0))
                        {
                            MyDefinitionId id = components[0].Definition.Id;
                            MyGridConveyorSystem.ItemPullRequest(this, this.GetInventory(0), base.OwnerId, id, 1, false);
                        }
                    }
                }
                HashSet<MyCubeGrid.MyBlockLocation> set1 = new HashSet<MyCubeGrid.MyBlockLocation>();
                bool creativeMode = MySession.Static.CreativeMode;
                if (MySession.Static.Players.TryGetPlayerId(base.BuiltBy, out id2) && MySession.Static.Players.TryGetPlayerById(id2, out player))
                {
                    creativeMode |= MySession.Static.CreativeToolsEnabled(Sync.MyId);
                }
                foreach (MyWelder.ProjectionRaycastData data in dataArray)
                {
                    if (this.IsWithinWorldLimits(data.cubeProjector, data.hitCube.BlockDefinition.BlockPairName, creativeMode ? data.hitCube.BlockDefinition.PCU : MyCubeBlockDefinition.PCU_CONSTRUCTION_STAGE_COST) && (MySession.Static.CreativeMode || this.GetInventory(0).ContainItems(1, data.hitCube.BlockDefinition.Components[0].Definition.Id, MyItemFlags.None)))
                    {
                        MyWelder.ProjectionRaycastData invokedBlock = data;
                        MySandboxGame.Static.Invoke(delegate {
                            if ((!invokedBlock.cubeProjector.Closed && !invokedBlock.cubeProjector.CubeGrid.Closed) && ((invokedBlock.hitCube.FatBlock == null) || !invokedBlock.hitCube.FatBlock.Closed))
                            {
                                invokedBlock.cubeProjector.Build(invokedBlock.hitCube, this.OwnerId, this.EntityId, true, this.BuiltBy);
                            }
                        }, "ShipWelder BuildProjection");
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                base.SetBuildingMusic(150);
            }
            return flag;
        }

        public override bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            if (MySessionComponentSafeZones.IsActionAllowed(base.CubeGrid, MySafeZoneAction.Welding, shooter))
            {
                return base.CanShoot(action, shooter, out status);
            }
            status = MyGunStatusEnum.Failed;
            return false;
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyShipWelder>())
            {
                base.CreateTerminalControls();
                if (MyFakes.ENABLE_WELDER_HELP_OTHERS)
                {
                    MyStringId? on = null;
                    on = null;
                    MyTerminalControlCheckbox<MyShipWelder> checkbox1 = new MyTerminalControlCheckbox<MyShipWelder>("helpOthers", MyCommonTexts.ShipWelder_HelpOthers, MyCommonTexts.ShipWelder_HelpOthers, on, on);
                    MyTerminalControlCheckbox<MyShipWelder> checkbox2 = new MyTerminalControlCheckbox<MyShipWelder>("helpOthers", MyCommonTexts.ShipWelder_HelpOthers, MyCommonTexts.ShipWelder_HelpOthers, on, on);
                    checkbox2.Getter = x => x.HelpOthers;
                    MyTerminalControlCheckbox<MyShipWelder> local4 = checkbox2;
                    MyTerminalControlCheckbox<MyShipWelder> local5 = checkbox2;
                    local5.Setter = (x, v) => x.m_helpOthers.Value = v;
                    MyTerminalControlCheckbox<MyShipWelder> checkbox = local5;
                    checkbox.EnableAction<MyShipWelder>(null);
                    MyTerminalControlFactory.AddControl<MyShipWelder>(checkbox);
                }
            }
        }

        private MyWelder.ProjectionRaycastData[] FindProjectedBlocks()
        {
            BoundingSphereD boundingSphere = new BoundingSphereD(Vector3D.Transform(base.m_detectorSphere.Center, base.CubeGrid.WorldMatrix), (double) base.m_detectorSphere.Radius);
            List<MyWelder.ProjectionRaycastData> list = new List<MyWelder.ProjectionRaycastData>();
            List<MyEntity> entitiesInSphere = Sandbox.Game.Entities.MyEntities.GetEntitiesInSphere(ref boundingSphere);
            foreach (MyCubeGrid grid in entitiesInSphere)
            {
                if (grid == null)
                {
                    continue;
                }
                if (grid.Projector != null)
                {
                    grid.GetBlocksInsideSphere(ref boundingSphere, this.m_projectedBlock, false);
                    foreach (MySlimBlock block in this.m_projectedBlock)
                    {
                        if (grid.Projector.CanBuild(block, true) != BuildCheckResult.OK)
                        {
                            continue;
                        }
                        MySlimBlock cubeBlock = grid.GetCubeBlock(block.Position);
                        if (cubeBlock != null)
                        {
                            list.Add(new MyWelder.ProjectionRaycastData(BuildCheckResult.OK, cubeBlock, grid.Projector));
                        }
                    }
                    this.m_projectedBlock.Clear();
                }
            }
            this.m_projectedBlock.Clear();
            entitiesInSphere.Clear();
            return list.ToArray();
        }

        private Vector3 GetLightPosition() => 
            ((Vector3) (base.WorldMatrix.Translation + (base.WorldMatrix.Forward * ((base.CubeGrid.GridSizeEnum == MyCubeSize.Large) ? ((double) 2.7f) : ((double) 1.5f)))));

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_ShipWelder objectBuilderCubeBlock = (MyObjectBuilder_ShipWelder) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.HelpOthers = (bool) this.m_helpOthers;
            return objectBuilderCubeBlock;
        }

        public override PullInformation GetPullInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.Inventory = this.GetInventory(0);
            information1.OwnerID = base.OwnerId;
            information1.Constraint = new MyInventoryConstraint("Empty constraint", null, true);
            return information1;
        }

        public override PullInformation GetPushInformation() => 
            null;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            base.Init(objectBuilder, cubeGrid);
            this.m_missingComponents = new Dictionary<string, int>();
            this.m_welderDef = base.BlockDefinition as MyShipWelderDefinition;
            if (this.m_welderDef != null)
            {
                if (this.m_welderDef.Flare != "")
                {
                    MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_FlareDefinition), this.m_welderDef.Flare);
                    this.m_flare = MyDefinitionManager.Static.GetDefinition(id) as MyFlareDefinition;
                }
                if (this.m_welderDef.EmissiveColorPreset == MyStringHash.NullOrEmpty)
                {
                    this.m_welderDef.EmissiveColorPreset = MyStringHash.GetOrCompute("Welder");
                }
            }
            MyObjectBuilder_ShipWelder welder = (MyObjectBuilder_ShipWelder) objectBuilder;
            this.m_helpOthers.SetLocalValue(welder.HelpOthers);
            this.LoadParticleDummyMatrices();
        }

        private bool IsWithinWorldLimits(MyProjectorBase projector, string name, int pcuToBuild)
        {
            if (MySession.Static.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.NONE)
            {
                return true;
            }
            bool flag = true;
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(base.BuiltBy);
            MyBlockLimits blockLimits = null;
            if (identity != null)
            {
                blockLimits = identity.BlockLimits;
            }
            if (((MySession.Static.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.PER_FACTION) && (identity != null)) && (MySession.Static.Factions.GetPlayerFaction(identity.IdentityId) == null))
            {
                return false;
            }
            flag = (flag & ((base.BuiltBy == 0) || (base.IDModule.GetUserRelationToOwner(base.BuiltBy) != MyRelationsBetweenPlayerAndBlock.Enemies))) & ((projector.BuiltBy == 0) || (base.IDModule.GetUserRelationToOwner(projector.BuiltBy) != MyRelationsBetweenPlayerAndBlock.Enemies));
            if (identity != null)
            {
                if (MySession.Static.MaxBlocksPerPlayer > 0)
                {
                    flag &= blockLimits.BlocksBuilt < blockLimits.MaxBlocks;
                }
                if (MySession.Static.TotalPCU != 0)
                {
                    flag &= blockLimits.PCU >= pcuToBuild;
                }
            }
            flag &= (MySession.Static.MaxGridSize == 0) || (projector.CubeGrid.BlocksCount < MySession.Static.MaxGridSize);
            short blockTypeLimit = MySession.Static.GetBlockTypeLimit(name);
            if ((identity != null) && (blockTypeLimit > 0))
            {
                MyBlockLimits.MyTypeLimitData data;
                flag &= (blockLimits.BlockTypeBuilt.TryGetValue(name, out data) ? data.BlocksBuilt : 0) < blockTypeLimit;
            }
            return flag;
        }

        private void LoadParticleDummyMatrices()
        {
            foreach (KeyValuePair<string, MyModelDummy> pair in MyModels.GetModelOnlyDummies(base.BlockDefinition.Model).Dummies)
            {
                if (pair.Key.ToLower().Contains("particles1"))
                {
                    this.m_particleDummyMatrix1 = pair.Value.Matrix;
                }
            }
        }

        public override void OnControlAcquired(MyCharacter owner)
        {
            base.OnControlAcquired(owner);
            if (((owner != null) && (owner.Parent != null)) && (ReferenceEquals(owner, MySession.Static.LocalCharacter) && !owner.Parent.Components.Contains(typeof(MyCasterComponent))))
            {
                MyCasterComponent component = new MyCasterComponent(new MyDrillSensorRayCast(0f, base.DEFAULT_REACH_DISTANCE, base.BlockDefinition));
                owner.Parent.Components.Add<MyCasterComponent>(component);
                base.controller = owner;
            }
        }

        public override void OnControlReleased()
        {
            base.OnControlReleased();
            if (((base.controller != null) && (base.controller.Parent != null)) && (ReferenceEquals(base.controller, MySession.Static.LocalCharacter) && base.controller.Parent.Components.Contains(typeof(MyCasterComponent))))
            {
                base.controller.Parent.Components.Remove(typeof(MyCasterComponent));
            }
        }

        protected override void PlayLoopSound(bool activated)
        {
            if (base.m_soundEmitter != null)
            {
                bool? nullable;
                if (activated)
                {
                    nullable = null;
                    base.m_soundEmitter.PlaySingleSound(METAL_SOUND, true, false, false, nullable);
                }
                else
                {
                    nullable = null;
                    base.m_soundEmitter.PlaySingleSound(IDLE_SOUND, true, false, false, nullable);
                }
            }
        }

        public override bool SetEmissiveStateDamaged() => 
            base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Disabled, base.Render.RenderObjectIDs[0], null);

        public override bool SetEmissiveStateDisabled() => 
            base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Disabled, base.Render.RenderObjectIDs[0], null);

        public override bool SetEmissiveStateWorking() => 
            base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Disabled, base.Render.RenderObjectIDs[0], null);

        protected override void StartEffects()
        {
            Vector3D translation = base.WorldMatrix.Translation;
            MatrixD effectMatrix = this.m_particleDummyMatrix1 * base.PositionComp.LocalMatrix;
            MyParticlesManager.TryCreateParticleEffect("ShipWelderArc", ref effectMatrix, ref translation, base.Render.ParentIDs[0], out this.m_particleEffect);
        }

        protected override void StartShooting()
        {
            base.StartShooting();
            base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Working, base.Render.RenderObjectIDs[0], null);
        }

        protected override void StopEffects()
        {
            if (this.m_particleEffect != null)
            {
                this.m_particleEffect.Stop(true);
                this.m_particleEffect = null;
            }
        }

        protected override void StopLoopSound()
        {
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.StopSound(true, true);
            }
        }

        protected override void StopShooting()
        {
            base.StopShooting();
            base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Disabled, base.Render.RenderObjectIDs[0], null);
        }

        public bool HelpOthers
        {
            get => 
                ((bool) this.m_helpOthers);
            set => 
                (this.m_helpOthers.Value = value);
        }

        protected override bool CanInteractWithSelf =>
            true;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyShipWelder.<>c <>9 = new MyShipWelder.<>c();
            public static MyTerminalValueControl<MyShipWelder, bool>.GetterDelegate <>9__19_0;
            public static MyTerminalValueControl<MyShipWelder, bool>.SetterDelegate <>9__19_1;

            internal bool <CreateTerminalControls>b__19_0(MyShipWelder x) => 
                x.HelpOthers;

            internal void <CreateTerminalControls>b__19_1(MyShipWelder x, bool v)
            {
                x.m_helpOthers.Value = v;
            }
        }
    }
}

