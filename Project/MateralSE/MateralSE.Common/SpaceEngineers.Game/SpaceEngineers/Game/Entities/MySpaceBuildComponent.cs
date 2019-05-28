namespace SpaceEngineers.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRageMath;

    [PreloadRequired, MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    internal class MySpaceBuildComponent : MyBuildComponentBase
    {
        public override void AfterSuccessfulBuild(VRage.Game.Entity.MyEntity builder, bool instantBuild)
        {
            if (!(ReferenceEquals(builder, null) | instantBuild) && MySession.Static.SurvivalMode)
            {
                this.TakeMaterialsFromBuilder(builder);
            }
        }

        public override void BeforeCreateBlock(MyCubeBlockDefinition definition, VRage.Game.Entity.MyEntity builder, MyObjectBuilder_CubeBlock ob, bool buildAsAdmin)
        {
            base.BeforeCreateBlock(definition, builder, ob, buildAsAdmin);
            if (((builder != null) && MySession.Static.SurvivalMode) && !buildAsAdmin)
            {
                ob.IntegrityPercent = 1.525902E-05f;
                ob.BuildPercent = 1.525902E-05f;
            }
        }

        private void ClearRequiredMaterials()
        {
            base.m_materialList.Clear();
        }

        public override void GetBlockAmountPlacementMaterials(MyCubeBlockDefinition definition, int amount)
        {
            this.ClearRequiredMaterials();
            GetMaterialsSimple(definition, base.m_materialList, amount);
        }

        public override void GetBlockPlacementMaterials(MyCubeBlockDefinition definition, Vector3I position, MyBlockOrientation orientation, MyCubeGrid grid)
        {
            this.ClearRequiredMaterials();
            GetMaterialsSimple(definition, base.m_materialList, 1);
        }

        public override void GetBlocksPlacementMaterials(HashSet<MyCubeGrid.MyBlockLocation> hashSet, MyCubeGrid grid)
        {
            this.ClearRequiredMaterials();
            foreach (MyCubeGrid.MyBlockLocation location in hashSet)
            {
                MyCubeBlockDefinition blockDefinition = null;
                if (MyDefinitionManager.Static.TryGetCubeBlockDefinition((MyDefinitionId) location.BlockDefinition, out blockDefinition))
                {
                    GetMaterialsSimple(blockDefinition, base.m_materialList, 1);
                }
            }
        }

        public override MyInventoryBase GetBuilderInventory(long entityId)
        {
            VRage.Game.Entity.MyEntity entity;
            if (MySession.Static.CreativeMode)
            {
                return null;
            }
            Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false);
            return ((entity != null) ? this.GetBuilderInventory(entity) : null);
        }

        public override MyInventoryBase GetBuilderInventory(VRage.Game.Entity.MyEntity entity)
        {
            if (MySession.Static.CreativeMode)
            {
                return null;
            }
            MyCharacter thisEntity = entity as MyCharacter;
            if (thisEntity != null)
            {
                return thisEntity.GetInventory(0);
            }
            MyShipWelder welder = entity as MyShipWelder;
            return ((welder == null) ? null : welder.GetInventory(0));
        }

        public override void GetGridSpawnMaterials(MyObjectBuilder_CubeGrid grid)
        {
            this.ClearRequiredMaterials();
            foreach (MyObjectBuilder_CubeBlock block in grid.CubeBlocks)
            {
                MyComponentStack.GetMountedComponents(base.m_materialList, block);
                if (block.ConstructionStockpile != null)
                {
                    foreach (MyObjectBuilder_StockpileItem item in block.ConstructionStockpile.Items)
                    {
                        if (item.PhysicalContent != null)
                        {
                            MyDefinitionId myDefinitionId = item.PhysicalContent.GetId();
                            base.m_materialList.AddMaterial(myDefinitionId, item.Amount, item.Amount, false);
                        }
                    }
                }
            }
        }

        public override void GetGridSpawnMaterials(MyCubeBlockDefinition definition, MatrixD worldMatrix, bool isStatic)
        {
            this.ClearRequiredMaterials();
            GetMaterialsSimple(definition, base.m_materialList, 1);
        }

        private static void GetMaterialsSimple(MyCubeBlockDefinition definition, MyComponentList output, int amount = 1)
        {
            for (int i = 0; i < definition.Components.Length; i++)
            {
                MyCubeBlockDefinition.Component component = definition.Components[i];
                output.AddMaterial(component.Definition.Id, component.Count * amount, (i == 0) ? 1 : 0, true);
            }
        }

        public override void GetMultiBlockPlacementMaterials(MyMultiBlockDefinition multiBlockDefinition)
        {
        }

        public override bool HasBuildingMaterials(VRage.Game.Entity.MyEntity builder, bool testTotal)
        {
            bool flag;
            if (MySession.Static.CreativeMode)
            {
                goto TR_0000;
            }
            else
            {
                if (MySession.Static.CreativeToolsEnabled(Sync.MyId) && ReferenceEquals(builder, MySession.Static.LocalCharacter))
                {
                    goto TR_0000;
                }
                if (builder == null)
                {
                    return false;
                }
                MyInventoryBase builderInventory = this.GetBuilderInventory(builder);
                if (builderInventory == null)
                {
                    return false;
                }
                MyInventory destinationInventory = null;
                MyCockpit thisEntity = null;
                long localPlayerId = MySession.Static.LocalPlayerId;
                if (builder is MyCharacter)
                {
                    thisEntity = (builder as MyCharacter).IsUsing as MyCockpit;
                    if (thisEntity != null)
                    {
                        destinationInventory = thisEntity.GetInventory(0);
                        localPlayerId = thisEntity.ControllerInfo.ControllingIdentityId;
                    }
                    else if ((builder as MyCharacter).ControllerInfo != null)
                    {
                        localPlayerId = (builder as MyCharacter).ControllerInfo.ControllingIdentityId;
                    }
                }
                flag = true;
                if (!testTotal)
                {
                    foreach (KeyValuePair<MyDefinitionId, int> pair in base.m_materialList.RequiredMaterials)
                    {
                        flag &= builderInventory.GetItemAmount(pair.Key, MyItemFlags.None, false) >= pair.Value;
                        if (!flag && (destinationInventory != null))
                        {
                            flag = destinationInventory.GetItemAmount(pair.Key, MyItemFlags.None, false) >= pair.Value;
                            if (!flag)
                            {
                                flag = MyGridConveyorSystem.ConveyorSystemItemAmount(thisEntity, destinationInventory, localPlayerId, pair.Key) >= pair.Value;
                            }
                        }
                        if (!flag)
                        {
                            break;
                        }
                    }
                    return flag;
                }
                foreach (KeyValuePair<MyDefinitionId, int> pair2 in base.m_materialList.TotalMaterials)
                {
                    flag &= builderInventory.GetItemAmount(pair2.Key, MyItemFlags.None, false) >= pair2.Value;
                    if (!flag && (destinationInventory != null))
                    {
                        flag = destinationInventory.GetItemAmount(pair2.Key, MyItemFlags.None, false) >= pair2.Value;
                        if (!flag)
                        {
                            flag = MyGridConveyorSystem.ConveyorSystemItemAmount(thisEntity, destinationInventory, localPlayerId, pair2.Key) >= pair2.Value;
                        }
                    }
                    if (!flag)
                    {
                        break;
                    }
                }
            }
            return flag;
        TR_0000:
            return true;
        }

        public override void LoadData()
        {
            base.LoadData();
            MyCubeBuilder.BuildComponent = this;
        }

        private void TakeMaterialsFromBuilder(VRage.Game.Entity.MyEntity builder)
        {
            if (builder != null)
            {
                MyInventoryBase builderInventory = this.GetBuilderInventory(builder);
                if (builderInventory != null)
                {
                    MyInventory destinationInventory = null;
                    MyCockpit thisEntity = null;
                    long playerId = 0x7fffffffffffffffL;
                    if (builder is MyCharacter)
                    {
                        thisEntity = (builder as MyCharacter).IsUsing as MyCockpit;
                        if (thisEntity != null)
                        {
                            destinationInventory = thisEntity.GetInventory(0);
                            playerId = thisEntity.ControllerInfo.ControllingIdentityId;
                        }
                        else if ((builder as MyCharacter).ControllerInfo != null)
                        {
                            playerId = (builder as MyCharacter).ControllerInfo.ControllingIdentityId;
                        }
                    }
                    foreach (KeyValuePair<MyDefinitionId, int> pair in base.m_materialList.RequiredMaterials)
                    {
                        MyFixedPoint amount = pair.Value;
                        MyFixedPoint point = builderInventory.GetItemAmount(pair.Key, MyItemFlags.None, false);
                        if (point > pair.Value)
                        {
                            builderInventory.RemoveItemsOfType(amount, pair.Key, MyItemFlags.None, false);
                            continue;
                        }
                        if (point > 0)
                        {
                            builderInventory.RemoveItemsOfType(point, pair.Key, MyItemFlags.None, false);
                            amount -= point;
                        }
                        if (destinationInventory != null)
                        {
                            MyFixedPoint point2 = destinationInventory.GetItemAmount(pair.Key, MyItemFlags.None, false);
                            if (point2 >= amount)
                            {
                                destinationInventory.RemoveItemsOfType(amount, pair.Key, MyItemFlags.None, false);
                            }
                            else
                            {
                                if (point2 > 0)
                                {
                                    destinationInventory.RemoveItemsOfType(point2, pair.Key, MyItemFlags.None, false);
                                    amount -= point2;
                                }
                                MyGridConveyorSystem.ItemPullRequest(thisEntity, destinationInventory, playerId, pair.Key, new MyFixedPoint?(amount), true);
                            }
                        }
                    }
                }
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            MyCubeBuilder.BuildComponent = null;
        }
    }
}

