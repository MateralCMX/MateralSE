namespace Sandbox.Game.Weapons
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Weapons.Guns;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Import;

    [MyCubeBlockType(typeof(MyObjectBuilder_ShipGrinder)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyShipGrinder), typeof(Sandbox.ModAPI.Ingame.IMyShipGrinder) })]
    public class MyShipGrinder : MyShipToolBase, Sandbox.ModAPI.IMyShipGrinder, Sandbox.ModAPI.IMyShipToolBase, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyShipToolBase, Sandbox.ModAPI.Ingame.IMyShipGrinder
    {
        private static MySoundPair IDLE_SOUND = new MySoundPair("ToolPlayGrindIdle", true);
        private static MySoundPair METAL_SOUND = new MySoundPair("ToolPlayGrindMetal", true);
        private const string PARTICLE_EFFECT = "ShipGrinder";
        private static string[] BLADE_SUBPART_IDs = new string[] { "grinder1", "grinder2" };
        private MyParticleEffect m_particleEffect1;
        private MyParticleEffect m_particleEffect2;
        private MyFlareDefinition m_flare;
        private MyShipGrinderDefinition m_grinderDef;
        private const float RANDOM_IMPULSE_SCALE = 500f;
        private static List<MyPhysicalInventoryItem> m_tmpItemList = new List<MyPhysicalInventoryItem>();
        private bool m_wantsToShake;
        private MyCubeGrid m_otherGrid;
        private Matrix m_particleDummyMatrix1;
        private Matrix m_particleDummyMatrix2;
        private List<MyShipGrinderSubpart> m_bladeSubparts = new List<MyShipGrinderSubpart>();

        protected override bool Activate(HashSet<MySlimBlock> targets)
        {
            int count = targets.Count;
            this.m_otherGrid = null;
            if (targets.Count > 0)
            {
                this.m_otherGrid = targets.FirstElement<MySlimBlock>().CubeGrid;
            }
            float num2 = 0.25f / ((float) Math.Min(4, targets.Count));
            foreach (MySlimBlock block in targets)
            {
                if ((!MySession.Static.IsScenario && !MySession.Static.Settings.ScenarioEditMode) || block.CubeGrid.BlocksDestructionEnabled)
                {
                    this.m_otherGrid = block.CubeGrid;
                    if ((this.m_otherGrid.Physics == null) || !this.m_otherGrid.Physics.Enabled)
                    {
                        count--;
                    }
                    else
                    {
                        MyCubeBlockDefinition.PreloadConstructionModels(block.BlockDefinition);
                        if (Sync.IsServer)
                        {
                            MyDamageInformation info = new MyDamageInformation(false, (MySession.Static.GrinderSpeedMultiplier * 4f) * num2, MyDamageType.Grind, base.EntityId);
                            if (block.UseDamageSystem)
                            {
                                MyDamageSystem.Static.RaiseBeforeDamageApplied(block, ref info);
                            }
                            if (block.CubeGrid.Editable)
                            {
                                block.DecreaseMountLevel(info.Amount, this.GetInventory(0), false);
                                block.MoveItemsFromConstructionStockpile(this.GetInventory(0), MyItemFlags.None);
                            }
                            if (block.UseDamageSystem)
                            {
                                MyDamageSystem.Static.RaiseAfterDamageApplied(block, info);
                            }
                            if (block.IsFullyDismounted)
                            {
                                if ((block.FatBlock != null) && block.FatBlock.HasInventory)
                                {
                                    this.EmptyBlockInventories(block.FatBlock);
                                }
                                if (block.UseDamageSystem)
                                {
                                    MyDamageSystem.Static.RaiseDestroyed(block, info);
                                }
                                block.SpawnConstructionStockpile();
                                block.CubeGrid.RazeBlock(block.Min);
                            }
                        }
                        if (count > 0)
                        {
                            base.SetBuildingMusic(200);
                        }
                    }
                }
            }
            this.m_wantsToShake = count != 0;
            return (count != 0);
        }

        private void ApplyImpulse(MyCubeGrid grid, Vector3 force)
        {
            MyPlayer controllingPlayer = Sync.Players.GetControllingPlayer(grid);
            if (((Sync.IsServer && (controllingPlayer == null)) || ReferenceEquals(MySession.Static.LocalHumanPlayer, controllingPlayer)) && (grid.Physics != null))
            {
                grid.Physics.ApplyImpulse((force * base.CubeGrid.GridSize) * 500f, base.PositionComp.GetPosition());
            }
        }

        public override bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            if (MySessionComponentSafeZones.IsActionAllowed(base.CubeGrid, MySafeZoneAction.Grinding, shooter))
            {
                return base.CanShoot(action, shooter, out status);
            }
            status = MyGunStatusEnum.Failed;
            return false;
        }

        private void EmptyBlockInventories(MyCubeBlock block)
        {
            for (int i = 0; i < block.InventoryCount; i++)
            {
                MyInventory src = block.GetInventory(i);
                if (!src.Empty())
                {
                    m_tmpItemList.Clear();
                    m_tmpItemList.AddList<MyPhysicalInventoryItem>(src.GetItems());
                    foreach (MyPhysicalInventoryItem item in m_tmpItemList)
                    {
                        MyFixedPoint? amount = null;
                        MyInventory.Transfer(src, this.GetInventory(0), item.ItemId, -1, amount, false);
                    }
                }
            }
        }

        public override PullInformation GetPullInformation() => 
            null;

        public override PullInformation GetPushInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.Inventory = this.GetInventory(0);
            information1.OwnerID = base.OwnerId;
            information1.Constraint = information1.Inventory.Constraint;
            return information1;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            if (base.CubeGrid.GridSizeEnum == MyCubeSize.Large)
            {
                IDLE_SOUND.Init("ToolLrgGrindIdle", true);
                METAL_SOUND.Init("ToolLrgGrindMetal", true);
            }
            this.m_grinderDef = base.BlockDefinition as MyShipGrinderDefinition;
            if ((this.m_grinderDef != null) && (this.m_grinderDef.Flare != ""))
            {
                MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_FlareDefinition), this.m_grinderDef.Flare);
                this.m_flare = MyDefinitionManager.Static.GetDefinition(id) as MyFlareDefinition;
            }
            base.HeatUpFrames = 15;
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            this.LoadParticleDummyMatrices();
        }

        public override void InitComponents()
        {
            base.Render = new MyRenderComponentShipGrinder();
            base.InitComponents();
        }

        protected override MyEntitySubpart InstantiateSubpart(MyModelDummy subpartDummy, ref MyEntitySubpart.Data data)
        {
            if (!BLADE_SUBPART_IDs.Contains<string>(data.Name))
            {
                return base.InstantiateSubpart(subpartDummy, ref data);
            }
            MyShipGrinderSubpart item = new MyShipGrinderSubpart();
            this.m_bladeSubparts.Add(item);
            return item;
        }

        private void LoadParticleDummyMatrices()
        {
            foreach (KeyValuePair<string, MyModelDummy> pair in MyModels.GetModelOnlyDummies(base.BlockDefinition.Model).Dummies)
            {
                if (pair.Key.ToLower().Contains("particles1"))
                {
                    this.m_particleDummyMatrix1 = pair.Value.Matrix;
                    continue;
                }
                if (pair.Key.ToLower().Contains("particles2"))
                {
                    this.m_particleDummyMatrix2 = pair.Value.Matrix;
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
                bool? nullable = null;
                base.m_soundEmitter.PlaySingleSound(activated ? METAL_SOUND : IDLE_SOUND, true, (base.m_soundEmitter.Sound != null) && base.m_soundEmitter.Sound.IsPlaying, false, nullable);
            }
        }

        public override void RefreshModels(string model, string modelCollision)
        {
            this.m_bladeSubparts.Clear();
            base.RefreshModels(model, modelCollision);
        }

        protected override void StartAnimation()
        {
            base.StartAnimation();
            using (List<MyShipGrinderSubpart>.Enumerator enumerator = this.m_bladeSubparts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Render.UpdateBladeSpeed(15.70796f);
                }
            }
        }

        protected override void StartEffects()
        {
            Vector3D translation = base.WorldMatrix.Translation;
            MatrixD effectMatrix = this.m_particleDummyMatrix1 * base.PositionComp.LocalMatrix;
            MyParticlesManager.TryCreateParticleEffect("ShipGrinder", ref effectMatrix, ref translation, base.Render.ParentIDs[0], out this.m_particleEffect1);
            effectMatrix = this.m_particleDummyMatrix2 * base.PositionComp.LocalMatrix;
            MyParticlesManager.TryCreateParticleEffect("ShipGrinder", ref effectMatrix, ref translation, base.Render.ParentIDs[0], out this.m_particleEffect2);
        }

        protected override void StopAnimation()
        {
            base.StopAnimation();
            using (List<MyShipGrinderSubpart>.Enumerator enumerator = this.m_bladeSubparts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Render.UpdateBladeSpeed(0f);
                }
            }
        }

        protected override void StopEffects()
        {
            if (this.m_particleEffect1 != null)
            {
                this.m_particleEffect1.Stop(true);
                this.m_particleEffect1 = null;
            }
            if (this.m_particleEffect2 != null)
            {
                this.m_particleEffect2.Stop(true);
                this.m_particleEffect2 = null;
            }
        }

        protected override void StopLoopSound()
        {
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.StopSound(false, true);
            }
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if ((this.m_wantsToShake && ((this.m_otherGrid != null) && ((this.m_otherGrid.Physics != null) && (!this.m_otherGrid.Physics.IsStatic && MySession.Static.EnableToolShake)))) && MyFakes.ENABLE_TOOL_SHAKE)
            {
                Vector3 force = MyUtils.GetRandomVector3();
                this.ApplyImpulse(this.m_otherGrid, force);
                if ((base.CubeGrid.Physics != null) && !base.CubeGrid.Physics.IsStatic)
                {
                    this.ApplyImpulse(base.CubeGrid, force);
                }
            }
            if ((MySandboxGame.TotalGamePlayTimeInMilliseconds - base.m_lastTimeActivate) >= 250)
            {
                this.m_wantsToShake = false;
                this.m_otherGrid = null;
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            if ((Sync.IsServer && (base.IsFunctional && base.UseConveyorSystem)) && (this.GetInventory(0).GetItems().Count > 0))
            {
                MyGridConveyorSystem.PushAnyRequest(this, this.GetInventory(0), base.OwnerId);
            }
        }

        private class MyShipGrinderSubpart : MyEntitySubpart
        {
            public override void InitComponents()
            {
                base.Render = new MyRenderComponentShipGrinder.MyRenderComponentShipGrinderBlade();
                base.InitComponents();
            }

            public MyRenderComponentShipGrinder.MyRenderComponentShipGrinderBlade Render =>
                ((MyRenderComponentShipGrinder.MyRenderComponentShipGrinderBlade) base.Render);
        }
    }
}

