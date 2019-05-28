namespace Sandbox.Game.Entities.Inventory
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    public static class MyPhysicalInventoryItemExtensions
    {
        private const float ITEM_SPAWN_RADIUS = 1f;
        private static List<HkBodyCollision> m_tmpCollisions = new List<HkBodyCollision>();

        private static void AddItemToLootBag(MyEntity itemOwner, MyPhysicalInventoryItem item, ref MyEntity lootBagEntity)
        {
            MyLootBagDefinition lootBagDefinition = MyDefinitionManager.Static.GetLootBagDefinition();
            if (lootBagDefinition != null)
            {
                MyDefinitionBase itemDefinition = item.GetItemDefinition();
                if (itemDefinition != null)
                {
                    if ((lootBagEntity == null) && (lootBagDefinition.SearchRadius > 0f))
                    {
                        Vector3D position = itemOwner.PositionComp.GetPosition();
                        BoundingSphereD boundingSphere = new BoundingSphereD(position, (double) lootBagDefinition.SearchRadius);
                        List<MyEntity> entitiesInSphere = MyEntities.GetEntitiesInSphere(ref boundingSphere);
                        double maxValue = double.MaxValue;
                        foreach (MyEntity entity in entitiesInSphere)
                        {
                            if (entity.MarkedForClose)
                            {
                                continue;
                            }
                            if ((entity.GetType() == typeof(MyEntity)) && ((entity.DefinitionId != null) && (entity.DefinitionId.Value == lootBagDefinition.ContainerDefinition)))
                            {
                                double num2 = (entity.PositionComp.GetPosition() - position).LengthSquared();
                                if (num2 < maxValue)
                                {
                                    lootBagEntity = entity;
                                    maxValue = num2;
                                }
                            }
                        }
                        entitiesInSphere.Clear();
                    }
                    if ((lootBagEntity == null) || (lootBagEntity.Components.Has<MyInventoryBase>() && !(lootBagEntity.Components.Get<MyInventoryBase>() as MyInventory).CanItemsBeAdded(item.Amount, itemDefinition.Id)))
                    {
                        MyContainerDefinition definition2;
                        lootBagEntity = null;
                        if (MyComponentContainerExtension.TryGetContainerDefinition(lootBagDefinition.ContainerDefinition.TypeId, lootBagDefinition.ContainerDefinition.SubtypeId, out definition2))
                        {
                            lootBagEntity = SpawnBagAround(itemOwner, definition2, 3, 2, 5, 1f);
                        }
                    }
                    if (lootBagEntity != null)
                    {
                        MyInventory inventory = lootBagEntity.Components.Get<MyInventoryBase>() as MyInventory;
                        if (inventory != null)
                        {
                            if (itemDefinition is MyCubeBlockDefinition)
                            {
                                inventory.AddBlocks(itemDefinition as MyCubeBlockDefinition, item.Amount);
                            }
                            else
                            {
                                inventory.AddItems(item.Amount, item.Content);
                            }
                        }
                    }
                }
            }
        }

        public static MyDefinitionBase GetItemDefinition(this MyPhysicalInventoryItem thisItem)
        {
            MyPhysicalItemDefinition definition2;
            if (thisItem.Content == null)
            {
                return null;
            }
            MyDefinitionBase base2 = null;
            if (thisItem.Content is MyObjectBuilder_BlockItem)
            {
                SerializableDefinitionId blockDefId = (thisItem.Content as MyObjectBuilder_BlockItem).BlockDefId;
                MyCubeBlockDefinition blockDefinition = null;
                if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(blockDefId, out blockDefinition))
                {
                    base2 = blockDefinition;
                }
            }
            else
            {
                base2 = MyDefinitionManager.Static.TryGetComponentBlockDefinition(thisItem.Content.GetId());
            }
            if ((base2 == null) && MyDefinitionManager.Static.TryGetPhysicalItemDefinition(thisItem.Content.GetId(), out definition2))
            {
                base2 = definition2;
            }
            return base2;
        }

        private static bool GetNonPenetratingTransformPosition(ref BoundingBox box, ref MatrixD transform)
        {
            bool flag;
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(transform);
            Vector3 halfExtents = box.HalfExtents;
            try
            {
                int num = 0;
                while (true)
                {
                    if (num >= 11)
                    {
                        flag = false;
                    }
                    else
                    {
                        float num2 = 0.3f * num;
                        Vector3D translation = transform.Translation + (Vector3D.UnitY * num2);
                        m_tmpCollisions.Clear();
                        MyPhysics.GetPenetrationsBox(ref halfExtents, ref translation, ref rotation, m_tmpCollisions, 15);
                        if (m_tmpCollisions.Count != 0)
                        {
                            num++;
                            continue;
                        }
                        transform.Translation = translation;
                        flag = true;
                    }
                    break;
                }
            }
            finally
            {
                m_tmpCollisions.Clear();
            }
            return flag;
        }

        private static void InitSpawned(MyEntity entity, BoundingBoxD box, Action<MyEntity> completionCallback)
        {
            if (entity != null)
            {
                float radius = entity.PositionComp.LocalVolume.Radius;
                Vector3D vectord = Vector3.Max(((Vector3) (box.Size / 2.0)) - new Vector3(radius), Vector3.Zero);
                box = new BoundingBoxD(box.Center - vectord, box.Center + vectord);
                Vector3D randomPosition = MyUtils.GetRandomPosition(ref box);
                Vector3 forward = MyUtils.GetRandomVector3Normalized();
                Vector3 vector2 = MyUtils.GetRandomVector3Normalized();
                while (forward == vector2)
                {
                    vector2 = MyUtils.GetRandomVector3Normalized();
                }
                entity.WorldMatrix = MatrixD.CreateWorld(randomPosition, forward, Vector3.Cross(Vector3.Cross(forward, vector2), forward));
                if (completionCallback != null)
                {
                    completionCallback(entity);
                }
            }
        }

        public static MyInventoryItem? MakeAPIItem(this MyPhysicalInventoryItem? item)
        {
            if (item != null)
            {
                return new MyInventoryItem?(item.Value.MakeAPIItem());
            }
            return null;
        }

        public static MyInventoryItem MakeAPIItem(this MyPhysicalInventoryItem item) => 
            new MyInventoryItem(item.Content.GetObjectId(), item.ItemId, item.Amount);

        public static void Spawn(this MyPhysicalInventoryItem thisItem, MyFixedPoint amount, BoundingBoxD box, MyEntity owner = null, Action<MyEntity> completionCallback = null)
        {
            if (amount >= 0)
            {
                MatrixD identity = MatrixD.Identity;
                identity.Translation = box.Center;
                thisItem.Spawn(amount, identity, owner, entity => InitSpawned(entity, box, completionCallback));
            }
        }

        public static void Spawn(this MyPhysicalInventoryItem thisItem, MyFixedPoint amount, MatrixD worldMatrix, MyEntity owner, Action<MyEntity> completionCallback)
        {
            if ((amount >= 0) && (thisItem.Content != null))
            {
                if (thisItem.Content is MyObjectBuilder_BlockItem)
                {
                    if (typeof(MyObjectBuilder_CubeBlock).IsAssignableFrom((System.Type) thisItem.Content.GetObjectId().TypeId))
                    {
                        MyCubeBlockDefinition definition;
                        MyObjectBuilder_BlockItem content = thisItem.Content as MyObjectBuilder_BlockItem;
                        MyDefinitionManager.Static.TryGetCubeBlockDefinition(content.BlockDefId, out definition);
                        if (definition != null)
                        {
                            MyObjectBuilder_CubeGrid objectBuilder = MyObjectBuilderSerializer.CreateNewObject(typeof(MyObjectBuilder_CubeGrid)) as MyObjectBuilder_CubeGrid;
                            objectBuilder.GridSizeEnum = definition.CubeSize;
                            objectBuilder.IsStatic = false;
                            objectBuilder.PersistentFlags |= MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
                            objectBuilder.PositionAndOrientation = new MyPositionAndOrientation(worldMatrix);
                            MyObjectBuilder_CubeBlock item = MyObjectBuilderSerializer.CreateNewObject(content.BlockDefId) as MyObjectBuilder_CubeBlock;
                            if (item != null)
                            {
                                item.Min = (SerializableVector3I) (((definition.Size / 2) - definition.Size) + Vector3I.One);
                                objectBuilder.CubeBlocks.Add(item);
                                for (int i = 0; i < amount; i++)
                                {
                                    objectBuilder.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                                    item.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                                    Vector3D? relativeOffset = null;
                                    MyEntities.CreateFromObjectBuilderParallel(objectBuilder, true, completionCallback, null, null, relativeOffset, false, false);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MyPhysicalItemDefinition definition = null;
                    if (MyDefinitionManager.Static.TryGetPhysicalItemDefinition(thisItem.Content.GetObjectId(), out definition))
                    {
                        MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(amount, thisItem.Content, 1f), worldMatrix, owner?.Physics, completionCallback);
                    }
                }
            }
        }

        private static MyEntity SpawnBagAround(MyEntity itemOwner, MyContainerDefinition bagDefinition, int sideCheckCount = 3, int frontCheckCount = 2, int upCheckCount = 5, float stepSize = 1f)
        {
            MatrixD xd;
            Vector3D? nullable = null;
            MyModel modelOnlyData = null;
            foreach (MyContainerDefinition.DefaultComponent component in bagDefinition.DefaultComponents)
            {
                if (typeof(MyObjectBuilder_ModelComponent).IsAssignableFrom((System.Type) component.BuilderType))
                {
                    MyComponentDefinitionBase componentDefinition = null;
                    MyStringHash subtypeId = bagDefinition.Id.SubtypeId;
                    if (component.SubtypeId != null)
                    {
                        subtypeId = component.SubtypeId.Value;
                    }
                    if (MyComponentContainerExtension.TryGetComponentDefinition(component.BuilderType, subtypeId, out componentDefinition))
                    {
                        MyModelComponentDefinition definition = componentDefinition as MyModelComponentDefinition;
                        if (definition != null)
                        {
                            modelOnlyData = MyModels.GetModelOnlyData(definition.Model);
                        }
                    }
                    break;
                }
            }
            if (modelOnlyData == null)
            {
                return null;
            }
            float radius = modelOnlyData.BoundingBox.HalfExtents.Max();
            HkShape shape = (HkShape) new HkSphereShape(radius);
            try
            {
                Vector3 vector3;
                Vector3 vector5;
                Vector3 vector6;
                int num4;
                Vector3D vectord3;
                int num5;
                Vector3D translation = itemOwner.PositionComp.WorldMatrix.Translation;
                float num2 = radius * stepSize;
                Vector3 up = -MyGravityProviderSystem.CalculateNaturalGravityInPoint(itemOwner.PositionComp.WorldMatrix.Translation);
                if (up == Vector3.Zero)
                {
                    up = Vector3.Up;
                }
                else
                {
                    up.Normalize();
                }
                up.CalculatePerpendicularVector(out vector3);
                Vector3 vector4 = Vector3.Cross(vector3, up);
                vector4.Normalize();
                Quaternion identity = Quaternion.Identity;
                Vector3[] vectorArray = new Vector3[] { vector3, vector4, -vector3, -vector4 };
                Vector3[] vectorArray2 = new Vector3[] { vector4, -vector3, -vector4, vector3 };
                int index = 0;
                goto TR_0026;
            TR_000D:
                if (nullable == null)
                {
                    MyOrientedBoundingBoxD xd3 = new MyOrientedBoundingBoxD(itemOwner.PositionComp.LocalAABB, itemOwner.PositionComp.WorldMatrix);
                    Vector3D[] corners = new Vector3D[8];
                    xd3.GetCorners(corners, 0);
                    float minValue = float.MinValue;
                    Vector3D[] vectordArray2 = corners;
                    int num8 = 0;
                    while (true)
                    {
                        if (num8 >= vectordArray2.Length)
                        {
                            nullable = new Vector3D?(itemOwner.PositionComp.WorldMatrix.Translation);
                            if (minValue > 0f)
                            {
                                nullable = new Vector3D?(xd3.Center + (minValue * up));
                            }
                            break;
                        }
                        float num9 = Vector3.Dot((Vector3) (vectordArray2[num8] - xd3.Center), up);
                        minValue = Math.Max(minValue, num9);
                        num8++;
                    }
                }
                goto TR_0003;
            TR_000E:
                index++;
                goto TR_0026;
            TR_000F:
                num4++;
                goto TR_0022;
            TR_0010:
                num5++;
            TR_001E:
                while (true)
                {
                    if (num5 >= sideCheckCount)
                    {
                        goto TR_000F;
                    }
                    else if (nullable == null)
                    {
                        for (int i = 0; (i < upCheckCount) && (nullable == null); i++)
                        {
                            Vector3D pos = (vectord3 + ((num5 * num2) * vector6)) + ((i * num2) * up);
                            if (MyEntities.IsInsideWorld(pos) && !MyEntities.IsShapePenetrating(shape, ref pos, ref identity, 15))
                            {
                                BoundingSphereD sphere = new BoundingSphereD(pos, (double) radius);
                                if (MySession.Static.VoxelMaps.GetOverlappingWithSphere(ref sphere) == null)
                                {
                                    nullable = new Vector3D?(pos);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        goto TR_000F;
                    }
                    break;
                }
                goto TR_0010;
            TR_0022:
                while (true)
                {
                    if (num4 >= frontCheckCount)
                    {
                        goto TR_000E;
                    }
                    else if (nullable == null)
                    {
                        vectord3 = (((translation + (0.25f * vector5)) + (radius * vector5)) + ((num4 * num2) * vector5)) - (((0.5f * (sideCheckCount - 1)) * num2) * vector6);
                        num5 = 0;
                    }
                    else
                    {
                        goto TR_000E;
                    }
                    break;
                }
                goto TR_001E;
            TR_0026:
                while (true)
                {
                    if (index >= vectorArray.Length)
                    {
                        goto TR_000D;
                    }
                    else if (nullable == null)
                    {
                        vector5 = vectorArray[index];
                        vector6 = vectorArray2[index];
                        num4 = 0;
                    }
                    else
                    {
                        goto TR_000D;
                    }
                    break;
                }
                goto TR_0022;
            }
            finally
            {
                shape.RemoveReference();
            }
        TR_0003:
            xd = itemOwner.PositionComp.WorldMatrix;
            xd.Translation = nullable.Value;
            MyEntity entity = MyEntities.CreateFromComponentContainerDefinitionAndAdd(bagDefinition.Id, false, true);
            if (entity == null)
            {
                return null;
            }
            entity.PositionComp.SetWorldMatrix(xd, null, false, true, true, false, false, false);
            entity.Physics.LinearVelocity = Vector3.Zero;
            entity.Physics.AngularVelocity = Vector3.Zero;
            return entity;
        }
    }
}

