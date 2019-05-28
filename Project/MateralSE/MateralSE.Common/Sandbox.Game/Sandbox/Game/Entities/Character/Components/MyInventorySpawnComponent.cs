namespace Sandbox.Game.Entities.Character.Components
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders;
    using VRageMath;

    public class MyInventorySpawnComponent : MyCharacterComponent
    {
        private MyInventory m_spawnInventory;
        private const string INVENTORY_USE_DUMMY_NAME = "inventory";

        private void CloseComponent()
        {
        }

        public override bool IsSerialized() => 
            false;

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
        }

        public override void OnCharacterDead()
        {
            if (!base.Character.IsDead)
            {
                return;
            }
            else if (!base.Character.Definition.EnableSpawnInventoryAsContainer)
            {
                return;
            }
            else if (base.Character.Definition.InventorySpawnContainerId == null)
            {
                return;
            }
            else if (base.Character.Components.Has<MyInventoryBase>())
            {
                MyInventoryBase base2 = base.Character.Components.Get<MyInventoryBase>();
                if (base2 is MyInventoryAggregate)
                {
                    MyInventoryAggregate aggregate = base2 as MyInventoryAggregate;
                    List<MyComponentBase> output = new List<MyComponentBase>();
                    aggregate.GetComponentsFlattened(output);
                    foreach (MyComponentBase base3 in output)
                    {
                        MyContainerDefinition definition;
                        MyInventory component = base3 as MyInventory;
                        if ((component == null) || (component.GetItemsCount() <= 0))
                        {
                            aggregate.RemoveComponent(base3);
                            continue;
                        }
                        if (MyDefinitionManager.Static.TryGetContainerDefinition(base.Character.Definition.InventorySpawnContainerId.Value, out definition))
                        {
                            aggregate.RemoveComponent(component);
                            if (Sync.IsServer)
                            {
                                MyInventory inventory = new MyInventory();
                                inventory.Init(component.GetObjectBuilder());
                                this.SpawnInventoryContainer(base.Character.Definition.InventorySpawnContainerId.Value, inventory, true, 0L);
                            }
                        }
                    }
                }
                else if ((base2 is MyInventory) && base.Character.Definition.SpawnInventoryOnBodyRemoval)
                {
                    this.m_spawnInventory = base2 as MyInventory;
                    this.SpawnBackpack(base.Character);
                }
            }
            this.CloseComponent();
        }

        private void SpawnBackpack(MyEntity obj)
        {
            MyInventory inventory = new MyInventory();
            inventory.Init(this.m_spawnInventory.GetObjectBuilder());
            this.m_spawnInventory = inventory;
            if (this.m_spawnInventory != null)
            {
                MyContainerDefinition definition;
                if (!MyComponentContainerExtension.TryGetContainerDefinition(base.Character.Definition.InventorySpawnContainerId.Value.TypeId, base.Character.Definition.InventorySpawnContainerId.Value.SubtypeId, out definition))
                {
                    MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_InventoryBagEntity), base.Character.Definition.InventorySpawnContainerId.Value.SubtypeId);
                    MyComponentContainerExtension.TryGetContainerDefinition(id.TypeId, id.SubtypeId, out definition);
                }
                if (((definition != null) && Sync.IsServer) && !MyFakes.USE_GPS_AS_FRIENDLY_SPAWN_LOCATIONS)
                {
                    MyGps gps1 = new MyGps();
                    gps1.ShowOnHud = true;
                    gps1.Name = new StringBuilder().AppendStringBuilder(MyTexts.Get(MySpaceTexts.GPS_Body_Location_Name)).Append(" - ").AppendFormatedDateTime(DateTime.Now).ToString();
                    gps1.DisplayName = MyTexts.GetString(MySpaceTexts.GPS_Body_Location_Name);
                    gps1.DiscardAt = null;
                    gps1.Coords = base.Character.PositionComp.GetPosition();
                    gps1.Description = "";
                    gps1.AlwaysVisible = true;
                    gps1.GPSColor = new Color(0x75, 0xc9, 0xf1);
                    gps1.IsContainerGPS = true;
                    MyGps gps = gps1;
                    MySession.Static.Gpss.SendAddGps(base.Character.DeadPlayerIdentityId, ref gps, this.SpawnInventoryContainer(base.Character.Definition.InventorySpawnContainerId.Value, this.m_spawnInventory, false, base.Character.DeadPlayerIdentityId), false);
                }
            }
        }

        private unsafe long SpawnInventoryContainer(MyDefinitionId bagDefinition, MyInventory inventory, bool spawnAboveCharacter = true, long ownerIdentityId = 0L)
        {
            MyContainerDefinition definition;
            if ((MySession.Static == null) || !MySession.Static.Ready)
            {
                return 0L;
            }
            MyEntity character = base.Character;
            MatrixD worldMatrix = base.Character.WorldMatrix;
            if (spawnAboveCharacter)
            {
                MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                xdPtr1.Translation += worldMatrix.Up + worldMatrix.Forward;
            }
            else
            {
                MatrixD* xdPtr2 = (MatrixD*) ref worldMatrix;
                xdPtr2.Translation = base.Character.PositionComp.WorldAABB.Center + (worldMatrix.Backward * 0.40000000596046448);
            }
            if (!MyComponentContainerExtension.TryGetContainerDefinition(bagDefinition.TypeId, bagDefinition.SubtypeId, out definition))
            {
                return 0L;
            }
            MyEntity entity2 = MyEntities.CreateFromComponentContainerDefinitionAndAdd(definition.Id, false, true);
            if (entity2 == null)
            {
                return 0L;
            }
            MyInventoryBagEntity entity3 = entity2 as MyInventoryBagEntity;
            if (entity3 != null)
            {
                entity3.OwnerIdentityId = ownerIdentityId;
            }
            entity2.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
            entity2.Physics.LinearVelocity = character.Physics.LinearVelocity;
            entity2.Physics.AngularVelocity = character.Physics.AngularVelocity;
            entity2.Render.EnableColorMaskHsv = true;
            entity2.Render.ColorMaskHsv = base.Character.Render.ColorMaskHsv;
            inventory.RemoveEntityOnEmpty = true;
            entity2.Components.Add<MyInventoryBase>(inventory);
            return entity2.EntityId;
        }

        public override string ComponentTypeDebugString =>
            "Inventory Spawn Component";
    }
}

