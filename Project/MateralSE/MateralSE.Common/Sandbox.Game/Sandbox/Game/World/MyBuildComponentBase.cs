namespace Sandbox.Game.World
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Inventory;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRageMath;

    public abstract class MyBuildComponentBase : MySessionComponentBase
    {
        protected MyComponentList m_materialList = new MyComponentList();
        protected MyComponentCombiner m_componentCombiner = new MyComponentCombiner();

        protected MyBuildComponentBase()
        {
        }

        public virtual void AfterCharacterCreate(MyCharacter character)
        {
            if (MyFakes.ENABLE_MEDIEVAL_INVENTORY)
            {
                character.InventoryAggregate = new MyInventoryAggregate("CharacterInventories");
                character.InventoryAggregate.AddComponent(new MyInventoryAggregate("Internal"));
            }
        }

        public abstract void AfterSuccessfulBuild(VRage.Game.Entity.MyEntity builder, bool instantBuild);
        public virtual void BeforeCreateBlock(MyCubeBlockDefinition definition, VRage.Game.Entity.MyEntity builder, MyObjectBuilder_CubeBlock ob, bool buildAsAdmin)
        {
            if (definition.EntityComponents != null)
            {
                if (ob.ComponentContainer == null)
                {
                    ob.ComponentContainer = new MyObjectBuilder_ComponentContainer();
                }
                foreach (KeyValuePair<string, MyObjectBuilder_ComponentBase> pair in definition.EntityComponents)
                {
                    MyObjectBuilder_ComponentContainer.ComponentData item = new MyObjectBuilder_ComponentContainer.ComponentData {
                        TypeId = pair.Key.ToString(),
                        Component = pair.Value
                    };
                    ob.ComponentContainer.Components.Add(item);
                }
            }
        }

        public abstract void GetBlockAmountPlacementMaterials(MyCubeBlockDefinition definition, int amount);
        public abstract void GetBlockPlacementMaterials(MyCubeBlockDefinition definition, Vector3I position, MyBlockOrientation orientation, MyCubeGrid grid);
        public abstract void GetBlocksPlacementMaterials(HashSet<MyCubeGrid.MyBlockLocation> hashSet, MyCubeGrid grid);
        public abstract MyInventoryBase GetBuilderInventory(long entityId);
        public abstract MyInventoryBase GetBuilderInventory(VRage.Game.Entity.MyEntity builder);
        public abstract void GetGridSpawnMaterials(MyObjectBuilder_CubeGrid grid);
        public abstract void GetGridSpawnMaterials(MyCubeBlockDefinition definition, MatrixD worldMatrix, bool isStatic);
        protected internal MyFixedPoint GetItemAmountCombined(MyInventoryBase availableInventory, MyDefinitionId myDefinitionId) => 
            this.m_componentCombiner.GetItemAmountCombined(availableInventory, myDefinitionId);

        public abstract void GetMultiBlockPlacementMaterials(MyMultiBlockDefinition multiBlockDefinition);
        public abstract bool HasBuildingMaterials(VRage.Game.Entity.MyEntity builder, bool testTotal = false);
        protected internal void RemoveItemsCombined(MyInventoryBase inventory, int itemAmount, MyDefinitionId itemDefinitionId)
        {
            this.m_materialList.Clear();
            this.m_materialList.AddMaterial(itemDefinitionId, itemAmount, 0, true);
            this.m_componentCombiner.RemoveItemsCombined(inventory, this.m_materialList.TotalMaterials);
            this.m_materialList.Clear();
        }

        public DictionaryReader<MyDefinitionId, int> TotalMaterials =>
            this.m_materialList.TotalMaterials;
    }
}

