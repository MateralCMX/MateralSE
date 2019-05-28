namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World.Generator;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.ObjectBuilders;

    public static class MyEntityExtensions
    {
        public static void AddNodeToWeldingGroups(this MyEntity thisEntity)
        {
            MyWeldingGroups.Static.AddNode(thisEntity);
        }

        public static void AddToGamePruningStructure(this MyEntity thisEntity)
        {
            MyGamePruningStructure.Add(thisEntity);
        }

        public static MySyncComponentBase CreateDefaultSyncEntity(this MyEntity thisEntity) => 
            new MySyncEntity(thisEntity);

        internal static void CreateStandardRenderComponents(this MyEntity thisEntity)
        {
            thisEntity.Render = new MyRenderComponent();
            thisEntity.AddDebugRenderComponent(new MyDebugRenderComponent(thisEntity));
        }

        public static MyObjectBuilder_EntityBase EntityFactoryCreateObjectBuilder(this MyEntity thisEntity) => 
            MyEntityFactory.CreateObjectBuilder(thisEntity);

        public static MyInventory GetInventory(this MyEntity thisEntity, int index = 0) => 
            (thisEntity.GetInventoryBase(index) as MyInventory);

        public static MyPhysicsBody GetPhysicsBody(this MyEntity thisEntity) => 
            (thisEntity.Physics as MyPhysicsBody);

        public static void GetWeldingGroupNodes(this MyEntity thisEntity, List<MyEntity> result)
        {
            MyWeldingGroups.Static.GetGroupNodes(thisEntity, result);
        }

        public static void ProceduralWorldGeneratorTrackEntity(this MyEntity thisEntity)
        {
            if (MyFakes.ENABLE_ASTEROID_FIELDS && (MyProceduralWorldGenerator.Static != null))
            {
                MyProceduralWorldGenerator.Static.TrackEntity(thisEntity);
            }
        }

        public static void RemoveFromGamePruningStructure(this MyEntity thisEntity)
        {
            MyGamePruningStructure.Remove(thisEntity);
        }

        public static void RemoveNodeFromWeldingGroups(this MyEntity thisEntity)
        {
            MyWeldingGroups.Static.RemoveNode(thisEntity);
        }

        internal static void SetCallbacks()
        {
            MyEntity.AddToGamePruningStructureExtCallBack = new Action<MyEntity>(MyEntityExtensions.AddToGamePruningStructure);
            MyEntity.RemoveFromGamePruningStructureExtCallBack = new Action<MyEntity>(MyEntityExtensions.RemoveFromGamePruningStructure);
            MyEntity.UpdateGamePruningStructureExtCallBack = new Action<MyEntity>(MyEntityExtensions.UpdateGamePruningStructure);
            MyEntity.MyEntityFactoryCreateObjectBuilderExtCallback = new MyEntity.MyEntityFactoryCreateObjectBuilderDelegate(MyEntityExtensions.EntityFactoryCreateObjectBuilder);
            MyEntity.CreateDefaultSyncEntityExtCallback = new MyEntity.CreateDefaultSyncEntityDelegate(MyEntityExtensions.CreateDefaultSyncEntity);
            MyEntity.MyWeldingGroupsGetGroupNodesExtCallback = new Action<MyEntity, List<MyEntity>>(MyEntityExtensions.GetWeldingGroupNodes);
            MyEntity.MyProceduralWorldGeneratorTrackEntityExtCallback = new Action<MyEntity>(MyEntityExtensions.ProceduralWorldGeneratorTrackEntity);
            MyEntity.CreateStandardRenderComponentsExtCallback = new Action<MyEntity>(MyEntityExtensions.CreateStandardRenderComponents);
            MyEntity.InitComponentsExtCallback = new Action<MyComponentContainer, MyObjectBuilderType, MyStringHash, MyObjectBuilder_ComponentContainer>(MyComponentContainerExtension.InitComponents);
            MyEntity.MyEntitiesCreateFromObjectBuilderExtCallback = new Func<MyObjectBuilder_EntityBase, bool, MyEntity>(MyEntities.CreateFromObjectBuilder);
        }

        public static bool TryGetInventory(this MyEntity thisEntity, out MyInventory inventory)
        {
            inventory = null;
            if (thisEntity.Components.Has<MyInventoryBase>())
            {
                inventory = thisEntity.GetInventory(0);
            }
            return (inventory != null);
        }

        public static bool TryGetInventory(this MyEntity thisEntity, out MyInventoryBase inventoryBase)
        {
            inventoryBase = null;
            return thisEntity.Components.TryGet<MyInventoryBase>(out inventoryBase);
        }

        public static void UpdateGamePruningStructure(this MyEntity thisEntity)
        {
            MyGamePruningStructure.Move(thisEntity);
        }

        public static bool WeldingGroupExists(this MyEntity thisEntity) => 
            (MyWeldingGroups.Static.GetGroup(thisEntity) != null);
    }
}

