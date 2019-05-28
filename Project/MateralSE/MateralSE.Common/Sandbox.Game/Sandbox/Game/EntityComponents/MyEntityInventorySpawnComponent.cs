namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;
    using VRageMath;

    [MyComponentType(typeof(MyEntityInventorySpawnComponent)), MyComponentBuilder(typeof(MyObjectBuilder_InventorySpawnComponent), true)]
    public class MyEntityInventorySpawnComponent : MyEntityComponentBase
    {
        private MyDefinitionId m_containerDefinition;

        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
            MyEntityInventorySpawnComponent_Definition definition2 = definition as MyEntityInventorySpawnComponent_Definition;
            this.m_containerDefinition = definition2.ContainerDefinition;
        }

        public override bool IsSerialized() => 
            true;

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            if (Sync.IsServer)
            {
                base.Entity.OnClosing += new Action<IMyEntity>(this.OnEntityClosing);
            }
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            if (Sync.IsServer)
            {
                base.Entity.OnClosing -= new Action<IMyEntity>(this.OnEntityClosing);
            }
        }

        private void OnEntityClosing(IMyEntity obj)
        {
            MyEntity entity = obj as MyEntity;
            if (entity.HasInventory && entity.InScene)
            {
                this.SpawnInventoryContainer(true);
            }
        }

        public unsafe bool SpawnInventoryContainer(bool spawnAboveEntity = true)
        {
            if ((MySession.Static != null) && MySession.Static.Ready)
            {
                MyEntity thisEntity = base.Entity as MyEntity;
                for (int i = 0; i < thisEntity.InventoryCount; i++)
                {
                    MyInventory component = thisEntity.GetInventory(i);
                    if ((component != null) && (component.GetItemsCount() > 0))
                    {
                        MyContainerDefinition definition;
                        MyEntity entity = base.Entity as MyEntity;
                        MatrixD worldMatrix = entity.WorldMatrix;
                        if (!spawnAboveEntity)
                        {
                            MyModel modelStorage = entity.Render.ModelStorage as MyModel;
                            if (modelStorage != null)
                            {
                                worldMatrix.Translation = Vector3.Transform(modelStorage.BoundingBox.Center, worldMatrix);
                            }
                        }
                        else
                        {
                            Vector3 v = -MyGravityProviderSystem.CalculateNaturalGravityInPoint(entity.PositionComp.GetPosition());
                            if (v == Vector3.Zero)
                            {
                                v = Vector3.Up;
                            }
                            v.Normalize();
                            Vector3 vector2 = Vector3.CalculatePerpendicularVector(v);
                            Vector3D translation = worldMatrix.Translation;
                            BoundingBoxD worldAABB = entity.PositionComp.WorldAABB;
                            int num2 = 0;
                            while (true)
                            {
                                if (num2 < 20)
                                {
                                    Vector3D vectord2 = (translation + ((0.1f * num2) * v)) + ((0.1f * num2) * vector2);
                                    BoundingBoxD xd3 = new BoundingBoxD(vectord2 - (0.25 * Vector3D.One), vectord2 + (0.25 * Vector3D.One));
                                    if (xd3.Intersects(ref worldAABB))
                                    {
                                        num2++;
                                        continue;
                                    }
                                    worldMatrix.Translation = vectord2 + (0.25f * v);
                                }
                                if (worldMatrix.Translation == translation)
                                {
                                    MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                                    xdPtr1.Translation += v + vector2;
                                }
                                break;
                            }
                        }
                        if (!MyComponentContainerExtension.TryGetContainerDefinition(this.m_containerDefinition.TypeId, this.m_containerDefinition.SubtypeId, out definition))
                        {
                            return false;
                        }
                        MyEntity entity3 = MyEntities.CreateFromComponentContainerDefinitionAndAdd(definition.Id, false, true);
                        if (entity3 == null)
                        {
                            return false;
                        }
                        entity3.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                        if (entity.InventoryCount == 1)
                        {
                            entity.Components.Remove<MyInventoryBase>();
                        }
                        else
                        {
                            MyInventoryAggregate inventoryBase = entity.GetInventoryBase() as MyInventoryAggregate;
                            if (inventoryBase == null)
                            {
                                return false;
                            }
                            inventoryBase.RemoveComponent(component);
                        }
                        entity3.Components.Add<MyInventoryBase>(component);
                        component.RemoveEntityOnEmpty = true;
                        entity3.Physics.LinearVelocity = Vector3.Zero;
                        entity3.Physics.AngularVelocity = Vector3.Zero;
                        if (thisEntity.Physics != null)
                        {
                            entity3.Physics.LinearVelocity = thisEntity.Physics.LinearVelocity;
                            entity3.Physics.AngularVelocity = thisEntity.Physics.AngularVelocity;
                        }
                        else if (thisEntity is MyCubeBlock)
                        {
                            MyCubeGrid cubeGrid = (thisEntity as MyCubeBlock).CubeGrid;
                            if (cubeGrid.Physics != null)
                            {
                                entity3.Physics.LinearVelocity = cubeGrid.Physics.LinearVelocity;
                                entity3.Physics.AngularVelocity = cubeGrid.Physics.AngularVelocity;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ComponentTypeDebugString =>
            "Inventory Spawn Component";
    }
}

