namespace Sandbox.Game.Entities.EnvironmentItems
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyEntityType(typeof(MyObjectBuilder_EnvironmentItems), true)]
    public class MyEnvironmentItems : MyEntity, IMyEventProxy, IMyEventOwner
    {
        private readonly MyInstanceFlagsEnum m_instanceFlags = (MyInstanceFlagsEnum.CastShadows | MyInstanceFlagsEnum.EnableColorMask | MyInstanceFlagsEnum.ShowLod1);
        protected readonly Dictionary<int, MyEnvironmentItemData> m_itemsData = new Dictionary<int, MyEnvironmentItemData>();
        protected readonly Dictionary<int, int> m_physicsShapeInstanceIdToLocalId = new Dictionary<int, int>();
        protected readonly Dictionary<int, int> m_localIdToPhysicsShapeInstanceId = new Dictionary<int, int>();
        protected static readonly Dictionary<MyStringHash, int> m_subtypeToModels = new Dictionary<MyStringHash, int>(MyStringHash.Comparer);
        protected readonly Dictionary<Vector3I, MyEnvironmentSector> m_sectors = new Dictionary<Vector3I, MyEnvironmentSector>(Vector3I.Comparer);
        protected List<HkdShapeInstanceInfo> m_childrenTmp = new List<HkdShapeInstanceInfo>();
        private HashSet<Vector3I> m_updatedSectorsTmp = new HashSet<Vector3I>();
        private List<HkdBreakableBodyInfo> m_tmpBodyInfos = new List<HkdBreakableBodyInfo>();
        protected static List<HkBodyCollision> m_tmpResults = new List<HkBodyCollision>();
        protected static List<MyEnvironmentSector> m_tmpSectors = new List<MyEnvironmentSector>();
        private List<int> m_tmpToDisable = new List<int>();
        private MyEnvironmentItemsDefinition m_definition = null;
        [CompilerGenerated]
        private Action<MyEnvironmentItems, ItemInfo> ItemAdded;
        [CompilerGenerated]
        private Action<MyEnvironmentItems, ItemInfo> ItemRemoved;
        [CompilerGenerated]
        private Action<MyEnvironmentItems, ItemInfo> ItemModified;
        private List<AddItemData> m_batchedAddItems = new List<AddItemData>();
        private List<ModifyItemData> m_batchedModifyItems = new List<ModifyItemData>();
        private List<RemoveItemData> m_batchedRemoveItems = new List<RemoveItemData>();
        private float m_batchTime;
        private const float BATCH_DEFAULT_TIME = 10f;
        [CompilerGenerated]
        private Action<MyEnvironmentItems> BatchEnded;
        public Vector3 BaseColor;
        public Vector2 ColorSpread;
        private Vector3D m_cellsOffset;

        public event Action<MyEnvironmentItems> BatchEnded
        {
            [CompilerGenerated] add
            {
                Action<MyEnvironmentItems> batchEnded = this.BatchEnded;
                while (true)
                {
                    Action<MyEnvironmentItems> a = batchEnded;
                    Action<MyEnvironmentItems> action3 = (Action<MyEnvironmentItems>) Delegate.Combine(a, value);
                    batchEnded = Interlocked.CompareExchange<Action<MyEnvironmentItems>>(ref this.BatchEnded, action3, a);
                    if (ReferenceEquals(batchEnded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEnvironmentItems> batchEnded = this.BatchEnded;
                while (true)
                {
                    Action<MyEnvironmentItems> source = batchEnded;
                    Action<MyEnvironmentItems> action3 = (Action<MyEnvironmentItems>) Delegate.Remove(source, value);
                    batchEnded = Interlocked.CompareExchange<Action<MyEnvironmentItems>>(ref this.BatchEnded, action3, source);
                    if (ReferenceEquals(batchEnded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyEnvironmentItems, ItemInfo> ItemAdded
        {
            [CompilerGenerated] add
            {
                Action<MyEnvironmentItems, ItemInfo> itemAdded = this.ItemAdded;
                while (true)
                {
                    Action<MyEnvironmentItems, ItemInfo> a = itemAdded;
                    Action<MyEnvironmentItems, ItemInfo> action3 = (Action<MyEnvironmentItems, ItemInfo>) Delegate.Combine(a, value);
                    itemAdded = Interlocked.CompareExchange<Action<MyEnvironmentItems, ItemInfo>>(ref this.ItemAdded, action3, a);
                    if (ReferenceEquals(itemAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEnvironmentItems, ItemInfo> itemAdded = this.ItemAdded;
                while (true)
                {
                    Action<MyEnvironmentItems, ItemInfo> source = itemAdded;
                    Action<MyEnvironmentItems, ItemInfo> action3 = (Action<MyEnvironmentItems, ItemInfo>) Delegate.Remove(source, value);
                    itemAdded = Interlocked.CompareExchange<Action<MyEnvironmentItems, ItemInfo>>(ref this.ItemAdded, action3, source);
                    if (ReferenceEquals(itemAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyEnvironmentItems, ItemInfo> ItemModified
        {
            [CompilerGenerated] add
            {
                Action<MyEnvironmentItems, ItemInfo> itemModified = this.ItemModified;
                while (true)
                {
                    Action<MyEnvironmentItems, ItemInfo> a = itemModified;
                    Action<MyEnvironmentItems, ItemInfo> action3 = (Action<MyEnvironmentItems, ItemInfo>) Delegate.Combine(a, value);
                    itemModified = Interlocked.CompareExchange<Action<MyEnvironmentItems, ItemInfo>>(ref this.ItemModified, action3, a);
                    if (ReferenceEquals(itemModified, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEnvironmentItems, ItemInfo> itemModified = this.ItemModified;
                while (true)
                {
                    Action<MyEnvironmentItems, ItemInfo> source = itemModified;
                    Action<MyEnvironmentItems, ItemInfo> action3 = (Action<MyEnvironmentItems, ItemInfo>) Delegate.Remove(source, value);
                    itemModified = Interlocked.CompareExchange<Action<MyEnvironmentItems, ItemInfo>>(ref this.ItemModified, action3, source);
                    if (ReferenceEquals(itemModified, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyEnvironmentItems, ItemInfo> ItemRemoved
        {
            [CompilerGenerated] add
            {
                Action<MyEnvironmentItems, ItemInfo> itemRemoved = this.ItemRemoved;
                while (true)
                {
                    Action<MyEnvironmentItems, ItemInfo> a = itemRemoved;
                    Action<MyEnvironmentItems, ItemInfo> action3 = (Action<MyEnvironmentItems, ItemInfo>) Delegate.Combine(a, value);
                    itemRemoved = Interlocked.CompareExchange<Action<MyEnvironmentItems, ItemInfo>>(ref this.ItemRemoved, action3, a);
                    if (ReferenceEquals(itemRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEnvironmentItems, ItemInfo> itemRemoved = this.ItemRemoved;
                while (true)
                {
                    Action<MyEnvironmentItems, ItemInfo> source = itemRemoved;
                    Action<MyEnvironmentItems, ItemInfo> action3 = (Action<MyEnvironmentItems, ItemInfo>) Delegate.Remove(source, value);
                    itemRemoved = Interlocked.CompareExchange<Action<MyEnvironmentItems, ItemInfo>>(ref this.ItemRemoved, action3, source);
                    if (ReferenceEquals(itemRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        static MyEnvironmentItems()
        {
            using (List<MyEnvironmentItemDefinition>.Enumerator enumerator = MyDefinitionManager.Static.GetEnvironmentItemDefinitions().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    CheckModelConsistency(enumerator.Current);
                }
            }
        }

        public MyEnvironmentItems()
        {
            base.Render = new MyRenderComponentEnvironmentItems(this);
            base.AddDebugRenderComponent(new MyEnviromentItemsDebugDraw(this));
        }

        private bool AddItem(MyEnvironmentItemDefinition itemDefinition, ref MatrixD worldMatrix, ref BoundingBoxD aabbWorld, int userData = -1, bool silentOverlaps = false)
        {
            if (MyFakes.ENABLE_ENVIRONMENT_ITEMS)
            {
                MyEnvironmentSector sector;
                if (!this.m_definition.ContainsItemDefinition(itemDefinition))
                {
                    return false;
                }
                if (itemDefinition.Model == null)
                {
                    return false;
                }
                int modelId = GetModelId(itemDefinition.Id.SubtypeId);
                MyModel modelOnlyData = MyModels.GetModelOnlyData(MyModel.GetById(modelId));
                if (modelOnlyData == null)
                {
                    return false;
                }
                CheckModelConsistency(itemDefinition);
                int hashCode = worldMatrix.Translation.GetHashCode();
                if (this.m_itemsData.ContainsKey(hashCode))
                {
                    if (!silentOverlaps)
                    {
                        MyLog.Default.WriteLine("WARNING: items are on the same place.");
                    }
                    return false;
                }
                MyEnvironmentItemData data = new MyEnvironmentItemData {
                    Id = hashCode,
                    SubtypeId = itemDefinition.Id.SubtypeId,
                    Transform = new MyTransformD(ref worldMatrix),
                    Enabled = true,
                    SectorInstanceId = -1,
                    Model = modelOnlyData,
                    UserData = userData
                };
                aabbWorld.Include(modelOnlyData.BoundingBox.Transform((MatrixD) worldMatrix));
                MatrixD transformMatrix = data.Transform.TransformMatrix;
                float sectorSize = MyFakes.ENVIRONMENT_ITEMS_ONE_INSTANCEBUFFER ? 20000f : this.m_definition.SectorSize;
                Vector3I sectorId = MyEnvironmentSector.GetSectorId(transformMatrix.Translation - this.CellsOffset, sectorSize);
                if (!this.m_sectors.TryGetValue(sectorId, out sector))
                {
                    sector = new MyEnvironmentSector(sectorId, (sectorId * sectorSize) + this.CellsOffset);
                    this.m_sectors.Add(sectorId, sector);
                }
                MatrixD xd2 = MatrixD.CreateTranslation(((Vector3D) (-sectorId * sectorSize)) - this.CellsOffset);
                Matrix localMatrix = (Matrix) (data.Transform.TransformMatrix * xd2);
                Color baseColor = this.BaseColor;
                if (this.ColorSpread.LengthSquared() > 0f)
                {
                    float randomFloat = MyUtils.GetRandomFloat(0f, this.ColorSpread.X);
                    float num5 = MyUtils.GetRandomFloat(0f, this.ColorSpread.Y);
                    baseColor = (MyUtils.GetRandomSign() > 0f) ? Color.Lighten(baseColor, (double) randomFloat) : Color.Darken(baseColor, (double) num5);
                }
                Vector3 colorMaskHsv = baseColor.ColorToHSVDX11();
                data.SectorInstanceId = sector.AddInstance(itemDefinition.Id.SubtypeId, modelId, hashCode, ref localMatrix, modelOnlyData.BoundingBox, this.m_instanceFlags, this.m_definition.MaxViewDistance, colorMaskHsv);
                data.Transform = new MyTransformD(transformMatrix);
                this.m_itemsData.Add(hashCode, data);
                if (this.ItemAdded != null)
                {
                    ItemInfo info = new ItemInfo {
                        LocalId = hashCode,
                        SubtypeId = data.SubtypeId,
                        Transform = data.Transform
                    };
                    this.ItemAdded(this, info);
                }
            }
            return true;
        }

        private bool AddPhysicsShape(MyStringHash subtypeId, MyModel model, ref MatrixD worldMatrix, HkStaticCompoundShape sectorRootShape, Dictionary<MyStringHash, HkShape> subtypeIdToShape, out int physicsShapeInstanceId)
        {
            HkShape shape;
            physicsShapeInstanceId = 0;
            if (!subtypeIdToShape.TryGetValue(subtypeId, out shape))
            {
                HkShape[] havokCollisionShapes = model.HavokCollisionShapes;
                if (havokCollisionShapes == null)
                {
                    goto TR_0000;
                }
                else if (havokCollisionShapes.Length != 0)
                {
                    shape = havokCollisionShapes[0];
                    shape.AddReference();
                    subtypeIdToShape[subtypeId] = shape;
                }
                else
                {
                    goto TR_0000;
                }
            }
            if (shape.ReferenceCount == 0)
            {
                return false;
            }
            physicsShapeInstanceId = sectorRootShape.AddInstance(shape, (Matrix) (worldMatrix * MatrixD.CreateTranslation(-this.CellsOffset)));
            return true;
        TR_0000:
            return false;
        }

        public void BatchAddItem(Vector3D position, MyStringHash subtypeId, bool sync)
        {
            if (this.m_definition.ContainsItemDefinition(subtypeId))
            {
                AddItemData item = new AddItemData {
                    Position = position,
                    SubtypeId = subtypeId
                };
                this.m_batchedAddItems.Add(item);
                if (sync)
                {
                    MySyncEnvironmentItems.SendBatchAddItemMessage(base.EntityId, position, subtypeId);
                }
            }
        }

        public void BatchModifyItem(int localId, MyStringHash subtypeId, bool sync)
        {
            if (this.m_itemsData.ContainsKey(localId))
            {
                ModifyItemData item = new ModifyItemData {
                    LocalId = localId,
                    SubtypeId = subtypeId
                };
                this.m_batchedModifyItems.Add(item);
                if (sync)
                {
                    MySyncEnvironmentItems.SendBatchModifyItemMessage(base.EntityId, localId, subtypeId);
                }
            }
        }

        public void BatchRemoveItem(int localId, bool sync)
        {
            if (this.m_itemsData.ContainsKey(localId))
            {
                RemoveItemData item = new RemoveItemData {
                    LocalId = localId
                };
                this.m_batchedRemoveItems.Add(item);
                if (sync)
                {
                    MySyncEnvironmentItems.SendBatchRemoveItemMessage(base.EntityId, localId);
                }
            }
        }

        public void BeginBatch(bool sync)
        {
            this.m_batchTime = 10f;
            if (sync)
            {
                MySyncEnvironmentItems.SendBeginBatchAddMessage(base.EntityId);
            }
        }

        public static MyEnvironmentItemsSpawnData BeginSpawn(MyEnvironmentItemsDefinition itemsDefinition, bool addToScene = true, long withEntityId = 0L)
        {
            MyObjectBuilder_EnvironmentItems objectBuilder = MyObjectBuilderSerializer.CreateNewObject(itemsDefinition.Id.TypeId, itemsDefinition.Id.SubtypeName) as MyObjectBuilder_EnvironmentItems;
            objectBuilder.EntityId = withEntityId;
            objectBuilder.PersistentFlags |= (2 | (addToScene ? 0x10 : 0)) | 4;
            MyEnvironmentItems items2 = !addToScene ? (MyEntities.CreateFromObjectBuilder(objectBuilder, true) as MyEnvironmentItems) : (MyEntities.CreateFromObjectBuilderAndAdd(objectBuilder, true) as MyEnvironmentItems);
            MyEnvironmentItemsSpawnData data1 = new MyEnvironmentItemsSpawnData();
            data1.EnvironmentItems = items2;
            return data1;
        }

        private static void CheckModelConsistency(MyEnvironmentItemDefinition itemDefinition)
        {
            int num;
            if (!m_subtypeToModels.TryGetValue(itemDefinition.Id.SubtypeId, out num) && (itemDefinition.Model != null))
            {
                m_subtypeToModels.Add(itemDefinition.Id.SubtypeId, MyModel.GetId(itemDefinition.Model));
            }
        }

        protected override void ClampToWorld()
        {
        }

        public void ClosePhysics(MyEnvironmentItemsSpawnData data)
        {
            if (this.Physics != null)
            {
                this.Physics.Close();
                this.Physics = null;
            }
        }

        protected virtual MyEntity DestroyItem(int itemInstanceId) => 
            null;

        public void DestroyItemAndCreateDebris(Vector3D position, Vector3 normal, double energy, int itemId)
        {
            if (MyPerGameSettings.Destruction)
            {
                this.DoDamage(100f, itemId, position, normal, MyStringHash.NullOrEmpty);
            }
            else
            {
                MyEntity entity = this.DestroyItem(itemId);
                if ((entity != null) && (entity.Physics != null))
                {
                    MyParticleEffect effect;
                    MyParticlesManager.TryCreateParticleEffect("Tree Destruction", MatrixD.CreateTranslation(position), out effect);
                    float mass = entity.Physics.Mass;
                    Vector3 vector = (Vector3) (((((float) Math.Sqrt(energy / ((double) mass))) / (0.01666667f * MyFakes.SIMULATION_SPEED)) * 0.8f) * normal);
                    Vector3D vectord = entity.Physics.CenterOfMassWorld + ((0.5 * Vector3D.Dot(position - entity.Physics.CenterOfMassWorld, entity.WorldMatrix.Up)) * entity.WorldMatrix.Up);
                    Vector3? torque = null;
                    float? maxSpeed = null;
                    entity.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, new Vector3?(vector), new Vector3D?(vectord), torque, maxSpeed, true, false);
                }
            }
        }

        private void DestructionBody_AfterReplaceBody(ref HkdReplaceBodyEvent e)
        {
            e.GetNewBodies(this.m_tmpBodyInfos);
            foreach (HkdBreakableBodyInfo info in this.m_tmpBodyInfos)
            {
                Matrix rigidBodyMatrix = info.Body.GetRigidBody().GetRigidBodyMatrix();
                Vector3 translation = rigidBodyMatrix.Translation;
                Quaternion rotation = Quaternion.CreateFromRotationMatrix(rigidBodyMatrix.GetOrientation());
                HkdBreakableShape breakableShape = info.Body.BreakableShape;
                this.Physics.HavokWorld.GetPenetrationsShape(breakableShape.GetShape(), ref translation, ref rotation, m_tmpResults, 15);
                using (List<HkBodyCollision>.Enumerator enumerator2 = m_tmpResults.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        if (enumerator2.Current.GetCollisionEntity() is MyVoxelMap)
                        {
                            info.Body.GetRigidBody().Quality = HkCollidableQualityType.Fixed;
                            break;
                        }
                    }
                }
                m_tmpResults.Clear();
                info.Body.GetRigidBody();
                info.Body.Dispose();
            }
        }

        private bool DisableRenderInstanceIfInRadius(Vector3D center, double radiusSq, int itemInstanceId, bool hasPhysics = false)
        {
            MyEnvironmentItemData data = this.m_itemsData[itemInstanceId];
            if (Vector3D.DistanceSquared(new Vector3D((Vector3) data.Transform.Position), center) <= radiusSq)
            {
                int num;
                bool flag = false;
                if (this.m_localIdToPhysicsShapeInstanceId.TryGetValue(itemInstanceId, out num))
                {
                    this.m_physicsShapeInstanceIdToLocalId.Remove(num);
                    this.m_localIdToPhysicsShapeInstanceId.Remove(itemInstanceId);
                    flag = true;
                }
                if (!hasPhysics | flag)
                {
                    MyEnvironmentSector sector;
                    Vector3I sectorId = MyEnvironmentSector.GetSectorId(data.Transform.TransformMatrix.Translation - this.m_cellsOffset, this.m_definition.SectorSize);
                    if (this.Sectors.TryGetValue(sectorId, out sector) && this.Sectors[sectorId].DisableInstance(data.SectorInstanceId, GetModelId(data.SubtypeId)))
                    {
                        this.m_updatedSectorsTmp.Add(sectorId);
                    }
                    return true;
                }
            }
            return false;
        }

        public virtual void DoDamage(float damage, int instanceId, Vector3D position, Vector3 normal, MyStringHash type)
        {
        }

        public void EndBatch(bool sync)
        {
            this.m_batchTime = 0f;
            if (((this.m_batchedAddItems.Count > 0) || (this.m_batchedModifyItems.Count > 0)) || (this.m_batchedRemoveItems.Count > 0))
            {
                this.ProcessBatch();
            }
            this.m_batchedAddItems.Clear();
            this.m_batchedModifyItems.Clear();
            this.m_batchedRemoveItems.Clear();
            if (sync)
            {
                MySyncEnvironmentItems.SendEndBatchAddMessage(base.EntityId);
            }
        }

        public static void EndSpawn(MyEnvironmentItemsSpawnData spawnData, bool updateGraphics = true, bool updatePhysics = true)
        {
            if (updatePhysics)
            {
                spawnData.EnvironmentItems.PrepareItemsPhysics(spawnData);
                spawnData.SubtypeToShapes.Clear();
                foreach (KeyValuePair<MyStringHash, HkShape> pair in spawnData.SubtypeToShapes)
                {
                    pair.Value.RemoveReference();
                }
                spawnData.SubtypeToShapes.Clear();
            }
            if (updateGraphics)
            {
                spawnData.EnvironmentItems.PrepareItemsGraphics();
            }
            spawnData.EnvironmentItems.UpdateGamePruningStructure();
        }

        public void GetAllItems(List<ItemInfo> output)
        {
            foreach (KeyValuePair<Vector3I, MyEnvironmentSector> pair in this.m_sectors)
            {
                pair.Value.GetItems(output);
            }
        }

        public void GetAllItemsInRadius(Vector3D point, float radius, List<ItemInfo> output)
        {
            this.GetSectorsInRadius(point, radius, m_tmpSectors);
            using (List<MyEnvironmentSector>.Enumerator enumerator = m_tmpSectors.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.GetItemsInRadius((Vector3) point, radius, output);
                }
            }
            m_tmpSectors.Clear();
        }

        public MyEnvironmentItemDefinition GetItemDefinition(int itemInstanceId)
        {
            MyEnvironmentItemData data = this.m_itemsData[itemInstanceId];
            MyDefinitionId id = new MyDefinitionId(this.m_definition.ItemDefinitionType, data.SubtypeId);
            return MyDefinitionManager.Static.GetEnvironmentItemDefinition(id);
        }

        public MyEnvironmentItemDefinition GetItemDefinitionFromShapeKey(uint shapeKey)
        {
            int itemInstanceId = this.GetItemInstanceId(shapeKey);
            if (itemInstanceId == -1)
            {
                return null;
            }
            MyEnvironmentItemData data = this.m_itemsData[itemInstanceId];
            MyDefinitionId id = new MyDefinitionId(this.m_definition.ItemDefinitionType, data.SubtypeId);
            return MyDefinitionManager.Static.GetEnvironmentItemDefinition(id);
        }

        public int GetItemInstanceId(uint shapeKey)
        {
            int num;
            uint num2;
            int num3;
            HkStaticCompoundShape shape = (HkStaticCompoundShape) this.Physics.RigidBody.GetShape();
            if (shapeKey == uint.MaxValue)
            {
                return -1;
            }
            shape.DecomposeShapeKey(shapeKey, out num, out num2);
            return (this.m_physicsShapeInstanceIdToLocalId.TryGetValue(num, out num3) ? num3 : -1);
        }

        public void GetItems(ref Vector3D point, List<Vector3D> output)
        {
            Vector3I sectorId = MyEnvironmentSector.GetSectorId(point, this.m_definition.SectorSize);
            MyEnvironmentSector sector = null;
            if (this.m_sectors.TryGetValue(sectorId, out sector))
            {
                sector.GetItems(output);
            }
        }

        public int GetItemsCount(MyStringHash id)
        {
            int num = 0;
            foreach (KeyValuePair<int, MyEnvironmentItemData> pair in this.m_itemsData)
            {
                if (pair.Value.SubtypeId == id)
                {
                    num++;
                }
            }
            return num;
        }

        public void GetItemsInRadius(ref Vector3D point, float radius, List<Vector3D> output)
        {
            Vector3I sectorId = MyEnvironmentSector.GetSectorId(point, this.m_definition.SectorSize);
            MyEnvironmentSector sector = null;
            if (this.m_sectors.TryGetValue(sectorId, out sector))
            {
                sector.GetItemsInRadius(point, radius, output);
            }
        }

        public void GetItemsInSector(Vector3I sectorId, List<ItemInfo> output)
        {
            if (this.m_sectors.ContainsKey(sectorId))
            {
                this.m_sectors[sectorId].GetItems(output);
            }
        }

        public void GetItemsInSector(ref Vector3D point, List<ItemInfo> output)
        {
            Vector3I sectorId = MyEnvironmentSector.GetSectorId(point, this.m_definition.SectorSize);
            MyEnvironmentSector sector = null;
            if (this.m_sectors.TryGetValue(sectorId, out sector))
            {
                sector.GetItems(output);
            }
        }

        public MyStringHash GetItemSubtype(int localId) => 
            this.m_itemsData[localId].SubtypeId;

        public bool GetItemWorldMatrix(int itemInstanceId, out MatrixD worldMatrix)
        {
            worldMatrix = MatrixD.Identity;
            MyEnvironmentItemData data = this.m_itemsData[itemInstanceId];
            worldMatrix = data.Transform.TransformMatrix;
            return true;
        }

        public static int GetModelId(MyStringHash subtypeId) => 
            m_subtypeToModels[subtypeId];

        public static string GetModelName(MyStringHash itemSubtype) => 
            MyModel.GetById(GetModelId(itemSubtype));

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_EnvironmentItems objectBuilder = (MyObjectBuilder_EnvironmentItems) base.GetObjectBuilder(copy);
            objectBuilder.SubtypeName = this.Definition.Id.SubtypeName;
            if (this.IsBatching)
            {
                this.EndBatch(true);
            }
            int num = 0;
            foreach (KeyValuePair<int, MyEnvironmentItemData> pair in this.m_itemsData)
            {
                if (pair.Value.Enabled)
                {
                    num++;
                }
            }
            objectBuilder.Items = new MyObjectBuilder_EnvironmentItems.MyOBEnvironmentItemData[num];
            int index = 0;
            foreach (KeyValuePair<int, MyEnvironmentItemData> pair2 in this.m_itemsData)
            {
                if (pair2.Value.Enabled)
                {
                    objectBuilder.Items[index].SubtypeName = pair2.Value.SubtypeId.ToString();
                    objectBuilder.Items[index].PositionAndOrientation = new MyPositionAndOrientation(pair2.Value.Transform.TransformMatrix);
                    index++;
                }
            }
            objectBuilder.CellsOffset = this.CellsOffset;
            return objectBuilder;
        }

        public void GetPhysicalItemsInRadius(Vector3D position, float radius, List<ItemInfo> result)
        {
            double num = radius * radius;
            if ((this.Physics != null) && (this.Physics.RigidBody != null))
            {
                HkStaticCompoundShape shape = (HkStaticCompoundShape) this.Physics.RigidBody.GetShape();
                HkShapeContainerIterator iterator = shape.GetIterator();
                while (iterator.IsValid)
                {
                    int num3;
                    int num4;
                    uint num5;
                    MyEnvironmentItemData data;
                    uint currentShapeKey = iterator.CurrentShapeKey;
                    shape.DecomposeShapeKey(currentShapeKey, out num3, out num5);
                    if ((this.m_physicsShapeInstanceIdToLocalId.TryGetValue(num3, out num4) && (this.m_itemsData.TryGetValue(num4, out data) && data.Enabled)) && (Vector3D.DistanceSquared(data.Transform.Position, position) < num))
                    {
                        ItemInfo item = new ItemInfo {
                            LocalId = num4,
                            SubtypeId = data.SubtypeId,
                            Transform = data.Transform
                        };
                        result.Add(item);
                    }
                    iterator.Next();
                }
            }
        }

        public MyEnvironmentSector GetSector(ref Vector3D worldPosition)
        {
            Vector3I sectorId = MyEnvironmentSector.GetSectorId(worldPosition, this.m_definition.SectorSize);
            MyEnvironmentSector sector = null;
            return (!this.m_sectors.TryGetValue(sectorId, out sector) ? null : sector);
        }

        public MyEnvironmentSector GetSector(ref Vector3I sectorId)
        {
            MyEnvironmentSector sector = null;
            return (!this.m_sectors.TryGetValue(sectorId, out sector) ? null : sector);
        }

        public Vector3I GetSectorId(ref Vector3D worldPosition) => 
            MyEnvironmentSector.GetSectorId(worldPosition, this.m_definition.SectorSize);

        public void GetSectorIdsInRadius(Vector3D position, float radius, List<Vector3I> sectorIds)
        {
            foreach (KeyValuePair<Vector3I, MyEnvironmentSector> pair in this.m_sectors)
            {
                if (!pair.Value.IsValid)
                {
                    continue;
                }
                BoundingBoxD sectorWorldBox = pair.Value.SectorWorldBox;
                sectorWorldBox.Inflate((double) radius);
                if (sectorWorldBox.Contains(position) == ContainmentType.Contains)
                {
                    sectorIds.Add(pair.Key);
                }
            }
        }

        public void GetSectorsInRadius(Vector3D position, float radius, List<MyEnvironmentSector> sectors)
        {
            foreach (KeyValuePair<Vector3I, MyEnvironmentSector> pair in this.m_sectors)
            {
                if (!pair.Value.IsValid)
                {
                    continue;
                }
                BoundingBoxD sectorWorldBox = pair.Value.SectorWorldBox;
                sectorWorldBox.Inflate((double) radius);
                if (sectorWorldBox.Contains(position) == ContainmentType.Contains)
                {
                    sectors.Add(pair.Value);
                }
            }
        }

        public bool HasItem(int localId) => 
            (this.m_itemsData.ContainsKey(localId) && this.m_itemsData[localId].Enabled);

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            float? scale = null;
            this.Init(null, null, null, scale, null);
            BoundingBoxD aabbWorld = BoundingBoxD.CreateInvalid();
            Dictionary<MyStringHash, HkShape> subtypeIdToShape = new Dictionary<MyStringHash, HkShape>(MyStringHash.Comparer);
            HkStaticCompoundShape sectorRootShape = new HkStaticCompoundShape(HkReferencePolicy.None);
            MyObjectBuilder_EnvironmentItems items = (MyObjectBuilder_EnvironmentItems) objectBuilder;
            MyDefinitionId defId = new MyDefinitionId(items.TypeId, items.SubtypeId);
            this.CellsOffset = items.CellsOffset;
            if (items.SubtypeId == MyStringHash.NullOrEmpty)
            {
                if (objectBuilder is MyObjectBuilder_Bushes)
                {
                    defId = new MyDefinitionId(typeof(MyObjectBuilder_DestroyableItems), "Bushes");
                }
                else if (objectBuilder is MyObjectBuilder_TreesMedium)
                {
                    defId = new MyDefinitionId(typeof(MyObjectBuilder_Trees), "TreesMedium");
                }
                else if (objectBuilder is MyObjectBuilder_Trees)
                {
                    defId = new MyDefinitionId(typeof(MyObjectBuilder_Trees), "Trees");
                }
            }
            if (MyDefinitionManager.Static.TryGetDefinition<MyEnvironmentItemsDefinition>(defId, out this.m_definition))
            {
                if (items.Items != null)
                {
                    foreach (MyObjectBuilder_EnvironmentItems.MyOBEnvironmentItemData data in items.Items)
                    {
                        MyStringHash orCompute = MyStringHash.GetOrCompute(data.SubtypeName);
                        if (this.m_definition.ContainsItemDefinition(orCompute))
                        {
                            MatrixD matrix = data.PositionAndOrientation.GetMatrix();
                            this.AddItem(this.m_definition.GetItemDefinition(orCompute), ref matrix, ref aabbWorld, -1, false);
                        }
                    }
                }
                this.PrepareItemsPhysics(sectorRootShape, ref aabbWorld, subtypeIdToShape);
                this.PrepareItemsGraphics();
                foreach (KeyValuePair<MyStringHash, HkShape> pair in subtypeIdToShape)
                {
                    pair.Value.RemoveReference();
                }
                sectorRootShape.Base.RemoveReference();
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            }
        }

        public bool IsItemEnabled(int localId) => 
            this.m_itemsData[localId].Enabled;

        public bool IsValidPosition(Vector3D position) => 
            !this.m_itemsData.ContainsKey(position.GetHashCode());

        public bool ModifyItemModel(int itemInstanceId, MyStringHash newSubtypeId, bool updateSector, bool sync)
        {
            MyEnvironmentItemData data;
            if (!this.m_itemsData.TryGetValue(itemInstanceId, out data))
            {
                return false;
            }
            int modelId = GetModelId(data.SubtypeId);
            int num2 = GetModelId(newSubtypeId);
            if (data.Enabled)
            {
                Matrix transformMatrix = (Matrix) data.Transform.TransformMatrix;
                Vector3I sectorId = MyEnvironmentSector.GetSectorId(transformMatrix.Translation - this.CellsOffset, this.Definition.SectorSize);
                MyEnvironmentSector sector = this.Sectors[sectorId];
                transformMatrix *= Matrix.Invert((Matrix) sector.SectorMatrix);
                sector.DisableInstance(data.SectorInstanceId, modelId);
                Vector3 colorMaskHsv = new Vector3();
                data.SubtypeId = newSubtypeId;
                data.SectorInstanceId = sector.AddInstance(newSubtypeId, num2, itemInstanceId, ref transformMatrix, MyModels.GetModelOnlyData(MyModel.GetById(modelId)).BoundingBox, this.m_instanceFlags, this.m_definition.MaxViewDistance, colorMaskHsv);
                this.m_itemsData[itemInstanceId] = data;
                if (updateSector)
                {
                    sector.UpdateRenderInstanceData();
                    sector.UpdateRenderEntitiesData(base.WorldMatrix, false, 0f);
                }
                if (this.ItemModified != null)
                {
                    ItemInfo info = new ItemInfo {
                        LocalId = data.Id,
                        SubtypeId = data.SubtypeId,
                        Transform = data.Transform
                    };
                    this.ItemModified(this, info);
                }
                if (sync)
                {
                    MySyncEnvironmentItems.SendModifyModelMessage(base.EntityId, itemInstanceId, newSubtypeId);
                }
            }
            return true;
        }

        protected virtual void OnRemoveItem(int localId, ref Matrix matrix, MyStringHash myStringId, int userData)
        {
            if (this.ItemRemoved != null)
            {
                ItemInfo info = new ItemInfo {
                    LocalId = localId,
                    SubtypeId = myStringId,
                    Transform = new MyTransformD(matrix),
                    UserData = userData
                };
                this.ItemRemoved(this, info);
            }
        }

        private void Physics_ContactPointCallback(ref MyPhysics.MyContactPointEvent e)
        {
            float num = Math.Abs(e.ContactPointEvent.SeparatingVelocity);
            IMyEntity otherEntity = e.ContactPointEvent.GetOtherEntity(this);
            if (((((otherEntity != null) && (otherEntity.Physics != null)) && !(otherEntity is MyFloatingObject)) && !(otherEntity is IMyHandheldGunObject<MyDeviceBase>)) && ((otherEntity.Physics.RigidBody == null) || (otherEntity.Physics.RigidBody.Layer != 20)))
            {
                float mass = MyDestructionHelper.MassFromHavok(otherEntity.Physics.Mass);
                if (otherEntity is MyCharacter)
                {
                    mass = otherEntity.Physics.Mass;
                }
                double energy = (num * num) * mass;
                if (energy > 200000.0)
                {
                    int bodyIdx = 0;
                    Vector3 normal = e.ContactPointEvent.ContactPoint.Normal;
                    if (!ReferenceEquals(e.ContactPointEvent.Base.BodyA.GetEntity(0), this))
                    {
                        bodyIdx = 1;
                        normal *= -1f;
                    }
                    uint shapeKey = e.ContactPointEvent.GetShapeKey(bodyIdx);
                    if (shapeKey != uint.MaxValue)
                    {
                        int num6;
                        uint num7;
                        int num8;
                        ((HkStaticCompoundShape) this.Physics.RigidBody.GetShape()).DecomposeShapeKey(shapeKey, out num6, out num7);
                        if (this.m_physicsShapeInstanceIdToLocalId.TryGetValue(num6, out num8))
                        {
                            Vector3D position = this.Physics.ClusterToWorld(e.ContactPointEvent.ContactPoint.Position);
                            this.DestroyItemAndCreateDebris(position, normal, energy, num8);
                        }
                    }
                }
            }
        }

        public void PrepareItemsGraphics()
        {
            foreach (KeyValuePair<Vector3I, MyEnvironmentSector> pair in this.m_sectors)
            {
                pair.Value.UpdateRenderInstanceData();
                pair.Value.UpdateRenderEntitiesData(base.WorldMatrix, false, 0f);
            }
        }

        public void PrepareItemsPhysics(MyEnvironmentItemsSpawnData spawnData)
        {
            spawnData.SectorRootShape = new HkStaticCompoundShape(HkReferencePolicy.None);
            spawnData.EnvironmentItems.PrepareItemsPhysics(spawnData.SectorRootShape, ref spawnData.AabbWorld, spawnData.SubtypeToShapes);
        }

        private void PrepareItemsPhysics(HkStaticCompoundShape sectorRootShape, ref BoundingBoxD aabbWorld, Dictionary<MyStringHash, HkShape> subtypeIdToShape)
        {
            foreach (KeyValuePair<int, MyEnvironmentItemData> pair in this.m_itemsData)
            {
                int num;
                if (!pair.Value.Enabled)
                {
                    continue;
                }
                MatrixD transformMatrix = pair.Value.Transform.TransformMatrix;
                if (this.AddPhysicsShape(pair.Value.SubtypeId, pair.Value.Model, ref transformMatrix, sectorRootShape, subtypeIdToShape, out num))
                {
                    this.m_physicsShapeInstanceIdToLocalId[num] = pair.Value.Id;
                    this.m_localIdToPhysicsShapeInstanceId[pair.Value.Id] = num;
                }
            }
            base.PositionComp.WorldAABB = aabbWorld;
            if (sectorRootShape.InstanceCount > 0)
            {
                MyPhysicsBody body1 = new MyPhysicsBody(this, RigidBodyFlag.RBF_STATIC);
                body1.MaterialType = this.m_definition.Material;
                body1.AngularDamping = MyPerGameSettings.DefaultAngularDamping;
                body1.LinearDamping = MyPerGameSettings.DefaultLinearDamping;
                body1.IsStaticForCluster = true;
                this.Physics = body1;
                sectorRootShape.Bake();
                HkMassProperties properties = new HkMassProperties();
                MatrixD worldTransform = MatrixD.CreateTranslation(this.CellsOffset);
                this.Physics.CreateFromCollisionObject((HkShape) sectorRootShape, Vector3.Zero, worldTransform, new HkMassProperties?(properties), 15);
                if (Sync.IsServer)
                {
                    this.Physics.ContactPointCallback += new MyPhysicsBody.PhysicsContactHandler(this.Physics_ContactPointCallback);
                    this.Physics.RigidBody.ContactPointCallbackEnabled = true;
                    this.Physics.RigidBody.IsEnvironment = true;
                }
                this.Physics.Enabled = true;
            }
        }

        private void ProcessBatch()
        {
            foreach (RemoveItemData data in this.m_batchedRemoveItems)
            {
                this.RemoveItem(data.LocalId, false, false);
            }
            foreach (ModifyItemData data2 in this.m_batchedModifyItems)
            {
                this.ModifyItemModel(data2.LocalId, data2.SubtypeId, false, false);
            }
            if (this.Physics != null)
            {
                if (Sync.IsServer)
                {
                    this.Physics.ContactPointCallback -= new MyPhysicsBody.PhysicsContactHandler(this.Physics_ContactPointCallback);
                }
                this.Physics.Close();
                this.Physics = null;
            }
            BoundingBoxD aabbWorld = BoundingBoxD.CreateInvalid();
            Dictionary<MyStringHash, HkShape> subtypeIdToShape = new Dictionary<MyStringHash, HkShape>(MyStringHash.Comparer);
            HkStaticCompoundShape sectorRootShape = new HkStaticCompoundShape(HkReferencePolicy.None);
            this.m_physicsShapeInstanceIdToLocalId.Clear();
            this.m_localIdToPhysicsShapeInstanceId.Clear();
            foreach (KeyValuePair<int, MyEnvironmentItemData> pair in this.m_itemsData)
            {
                if (pair.Value.Enabled)
                {
                    aabbWorld.Include(MyModels.GetModelOnlyData(MyModel.GetById(m_subtypeToModels[pair.Value.SubtypeId])).BoundingBox.Transform(pair.Value.Transform.TransformMatrix));
                }
            }
            foreach (AddItemData data4 in this.m_batchedAddItems)
            {
                MatrixD worldMatrix = MatrixD.CreateWorld(data4.Position, Vector3D.Forward, Vector3D.Up);
                MyEnvironmentItemDefinition itemDefinition = this.m_definition.GetItemDefinition(data4.SubtypeId);
                this.AddItem(itemDefinition, ref worldMatrix, ref aabbWorld, -1, false);
            }
            this.PrepareItemsPhysics(sectorRootShape, ref aabbWorld, subtypeIdToShape);
            this.PrepareItemsGraphics();
            foreach (KeyValuePair<MyStringHash, HkShape> pair2 in subtypeIdToShape)
            {
                pair2.Value.RemoveReference();
            }
            subtypeIdToShape.Clear();
        }

        public bool RemoveItem(int itemInstanceId, bool sync, bool immediateUpdate = true)
        {
            int num;
            return (!this.m_localIdToPhysicsShapeInstanceId.TryGetValue(itemInstanceId, out num) ? (this.m_itemsData.ContainsKey(itemInstanceId) && this.RemoveNonPhysicalItem(itemInstanceId, sync, immediateUpdate)) : this.RemoveItem(itemInstanceId, num, sync, immediateUpdate));
        }

        protected bool RemoveItem(int itemInstanceId, int physicsInstanceId, bool sync, bool immediateUpdate)
        {
            MyEnvironmentSector sector;
            this.m_physicsShapeInstanceIdToLocalId.Remove(physicsInstanceId);
            this.m_localIdToPhysicsShapeInstanceId.Remove(itemInstanceId);
            if (!this.m_itemsData.ContainsKey(itemInstanceId))
            {
                return false;
            }
            MyEnvironmentItemData data = this.m_itemsData[itemInstanceId];
            this.m_itemsData.Remove(itemInstanceId);
            if (this.Physics != null)
            {
                ((HkStaticCompoundShape) this.Physics.RigidBody.GetShape()).EnableInstance(physicsInstanceId, false);
            }
            Matrix transformMatrix = (Matrix) data.Transform.TransformMatrix;
            Vector3I sectorId = MyEnvironmentSector.GetSectorId(transformMatrix.Translation - this.m_cellsOffset, this.Definition.SectorSize);
            int modelId = GetModelId(data.SubtypeId);
            if (this.Sectors.TryGetValue(sectorId, out sector))
            {
                sector.DisableInstance(data.SectorInstanceId, modelId);
            }
            foreach (KeyValuePair<int, MyEnvironmentItemData> pair in this.m_itemsData)
            {
                if (pair.Value.SectorInstanceId == this.Sectors[sectorId].SectorItemCount)
                {
                    MyEnvironmentItemData data2 = pair.Value;
                    data2.SectorInstanceId = data.SectorInstanceId;
                    this.m_itemsData[pair.Key] = data2;
                    break;
                }
            }
            if (immediateUpdate && (sector != null))
            {
                sector.UpdateRenderInstanceData(modelId);
            }
            this.OnRemoveItem(itemInstanceId, ref transformMatrix, data.SubtypeId, data.UserData);
            if (sync)
            {
                MySyncEnvironmentItems.RemoveEnvironmentItem(base.EntityId, itemInstanceId);
            }
            return true;
        }

        public void RemoveItemsAroundPoint(Vector3D point, double radius)
        {
            double radiusSq = radius * radius;
            if ((this.Physics != null) && (this.Physics.RigidBody != null))
            {
                HkStaticCompoundShape shape = (HkStaticCompoundShape) this.Physics.RigidBody.GetShape();
                HkShapeContainerIterator iterator = shape.GetIterator();
                while (iterator.IsValid)
                {
                    int num3;
                    uint num4;
                    int num5;
                    uint currentShapeKey = iterator.CurrentShapeKey;
                    shape.DecomposeShapeKey(currentShapeKey, out num3, out num4);
                    if (this.m_physicsShapeInstanceIdToLocalId.TryGetValue(num3, out num5) && this.DisableRenderInstanceIfInRadius(point, radiusSq, num5, true))
                    {
                        shape.EnableInstance(num3, false);
                        this.m_tmpToDisable.Add(num5);
                    }
                    iterator.Next();
                }
            }
            else
            {
                foreach (KeyValuePair<int, MyEnvironmentItemData> pair in this.m_itemsData)
                {
                    if (!pair.Value.Enabled)
                    {
                        continue;
                    }
                    if (this.DisableRenderInstanceIfInRadius(point, radiusSq, pair.Key, false))
                    {
                        this.m_tmpToDisable.Add(pair.Key);
                    }
                }
            }
            foreach (int num6 in this.m_tmpToDisable)
            {
                MyEnvironmentItemData data = this.m_itemsData[num6];
                data.Enabled = false;
                this.m_itemsData[num6] = data;
            }
            this.m_tmpToDisable.Clear();
            foreach (Vector3I vectori in this.m_updatedSectorsTmp)
            {
                this.Sectors[vectori].UpdateRenderInstanceData();
            }
            this.m_updatedSectorsTmp.Clear();
        }

        public void RemoveItemsOfSubtype(HashSet<MyStringHash> subtypes)
        {
            this.BeginBatch(true);
            foreach (int num in new List<int>(this.m_itemsData.Keys))
            {
                MyEnvironmentItemData data = this.m_itemsData[num];
                if (data.Enabled && subtypes.Contains(data.SubtypeId))
                {
                    this.BatchRemoveItem(num, true);
                }
            }
            this.EndBatch(true);
        }

        protected bool RemoveNonPhysicalItem(int itemInstanceId, bool sync, bool immediateUpdate)
        {
            MyEnvironmentItemData data = this.m_itemsData[itemInstanceId];
            data.Enabled = false;
            this.m_itemsData[itemInstanceId] = data;
            Matrix transformMatrix = (Matrix) data.Transform.TransformMatrix;
            Vector3I sectorId = MyEnvironmentSector.GetSectorId(transformMatrix.Translation, this.Definition.SectorSize);
            int modelId = GetModelId(data.SubtypeId);
            this.Sectors[sectorId].DisableInstance(data.SectorInstanceId, modelId);
            if (immediateUpdate)
            {
                this.Sectors[sectorId].UpdateRenderInstanceData(modelId);
            }
            this.OnRemoveItem(itemInstanceId, ref transformMatrix, data.SubtypeId, data.UserData);
            if (sync)
            {
                MySyncEnvironmentItems.RemoveEnvironmentItem(base.EntityId, itemInstanceId);
            }
            return true;
        }

        public static bool SpawnItem(MyEnvironmentItemsSpawnData spawnData, MyEnvironmentItemDefinition itemDefinition, Vector3D position, Vector3D up, int userdata = -1, bool silentOverlaps = true)
        {
            if (!MyFakes.ENABLE_ENVIRONMENT_ITEMS)
            {
                return true;
            }
            if (((spawnData == null) || (spawnData.EnvironmentItems == null)) || (itemDefinition == null))
            {
                return false;
            }
            Vector3D randomPerpendicularVector = MyUtils.GetRandomPerpendicularVector(ref up);
            MatrixD worldMatrix = MatrixD.CreateWorld(position, randomPerpendicularVector, up);
            return spawnData.EnvironmentItems.AddItem(itemDefinition, ref worldMatrix, ref spawnData.AabbWorld, userdata, silentOverlaps);
        }

        public bool TryGetItemInfoById(int itemId, out ItemInfo result)
        {
            MyEnvironmentItemData data;
            result = new ItemInfo();
            if (!this.m_itemsData.TryGetValue(itemId, out data) || !data.Enabled)
            {
                return false;
            }
            ItemInfo info = new ItemInfo {
                LocalId = itemId,
                SubtypeId = data.SubtypeId,
                Transform = data.Transform
            };
            result = info;
            return true;
        }

        public void UnloadGraphics()
        {
            foreach (KeyValuePair<Vector3I, MyEnvironmentSector> pair in this.m_sectors)
            {
                pair.Value.UnloadRenderObjects();
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            if (Sync.IsServer && this.IsBatching)
            {
                this.m_batchTime -= 1.666667f;
                if (this.m_batchTime <= 0f)
                {
                    this.EndBatch(true);
                }
                if (this.BatchEnded != null)
                {
                    this.BatchEnded(this);
                }
            }
        }

        public Dictionary<Vector3I, MyEnvironmentSector> Sectors =>
            this.m_sectors;

        public MyEnvironmentItemsDefinition Definition =>
            this.m_definition;

        public bool IsBatching =>
            (this.m_batchTime > 0f);

        public float BatchTime =>
            this.m_batchTime;

        public MyPhysicsBody Physics
        {
            get => 
                (base.Physics as MyPhysicsBody);
            set => 
                (base.Physics = value);
        }

        public Vector3D CellsOffset
        {
            get => 
                this.m_cellsOffset;
            set
            {
                this.m_cellsOffset = value;
                base.PositionComp.SetPosition(this.m_cellsOffset, null, false, true);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AddItemData
        {
            public Vector3D Position;
            public MyStringHash SubtypeId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ItemInfo
        {
            public int LocalId;
            public MyTransformD Transform;
            public MyStringHash SubtypeId;
            public int UserData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ModifyItemData
        {
            public int LocalId;
            public MyStringHash SubtypeId;
        }

        private class MyEnviromentItemsDebugDraw : MyDebugRenderComponentBase
        {
            private MyEnvironmentItems m_items;

            public MyEnviromentItemsDebugDraw(MyEnvironmentItems items)
            {
                this.m_items = items;
            }

            public override void DebugDraw()
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_ENVIRONMENT_ITEMS)
                {
                    foreach (KeyValuePair<Vector3I, MyEnvironmentSector> pair in this.m_items.Sectors)
                    {
                        pair.Value.DebugDraw(pair.Key, this.m_items.m_definition.SectorSize);
                        if (pair.Value.IsValid)
                        {
                            Vector3D vectord = pair.Value.SectorBox.Center + pair.Value.SectorMatrix.Translation;
                            if (Vector3D.Distance(MySector.MainCamera.Position, vectord) < 1000.0)
                            {
                                MyRenderProxy.DebugDrawText3D(vectord, this.m_items.Definition.Id.SubtypeName + " Sector: " + pair.Key, Color.SaddleBrown, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                            }
                        }
                    }
                }
            }

            public override void DebugDrawInvalidTriangles()
            {
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct MyEnvironmentItemData
        {
            public int Id;
            public MyTransformD Transform;
            public MyStringHash SubtypeId;
            public bool Enabled;
            public int SectorInstanceId;
            public int UserData;
            public MyModel Model;
        }

        public class MyEnvironmentItemsSpawnData
        {
            public MyEnvironmentItems EnvironmentItems;
            public Dictionary<MyStringHash, HkShape> SubtypeToShapes = new Dictionary<MyStringHash, HkShape>(MyStringHash.Comparer);
            public HkStaticCompoundShape SectorRootShape;
            public BoundingBoxD AabbWorld = BoundingBoxD.CreateInvalid();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RemoveItemData
        {
            public int LocalId;
        }
    }
}

