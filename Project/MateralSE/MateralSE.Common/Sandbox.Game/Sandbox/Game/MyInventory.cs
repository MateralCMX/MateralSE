namespace Sandbox.Game
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Interfaces;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyComponentBuilder(typeof(MyObjectBuilder_Inventory), true), StaticEventOwner]
    public class MyInventory : MyInventoryBase, VRage.Game.ModAPI.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory
    {
        public static MyStringHash INVENTORY_CHANGED = MyStringHash.GetOrCompute("InventoryChanged");
        [CompilerGenerated]
        private static Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint> OnTransferByUser;
        private static Dictionary<MyDefinitionId, int> m_tmpItemsToAdd = new Dictionary<MyDefinitionId, int>();
        private List<MyPhysicalInventoryItem> m_items;
        private MyFixedPoint m_maxMass;
        private MyFixedPoint m_maxVolume;
        private int m_maxItemCount;
        private MySoundPair dropSound;
        private readonly VRage.Sync.Sync<MyFixedPoint, SyncDirection.FromServer> m_currentVolume;
        private readonly VRage.Sync.Sync<MyFixedPoint, SyncDirection.FromServer> m_currentMass;
        private MyInventoryFlags m_flags;
        public object UserData;
        private uint m_nextItemID;
        private HashSet<uint> m_usedIds;
        public readonly VRage.Sync.SyncType SyncType;
        private bool m_multiplierEnabled;
        public MyInventoryConstraint Constraint;
        private MyObjectBuilder_InventoryDefinition myObjectBuilder_InventoryDefinition;
        private MyHudNotification m_inventoryNotEmptyNotification;
        private MyObjectBuilder_Inventory m_objectBuilder;
        private LRUCache<ConnectionKey, ConnectionData> m_connectionCache;

        public static  event Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint> OnTransferByUser
        {
            [CompilerGenerated] add
            {
                Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint> onTransferByUser = OnTransferByUser;
                while (true)
                {
                    Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint> a = onTransferByUser;
                    Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint> action3 = (Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint>) Delegate.Combine(a, value);
                    onTransferByUser = Interlocked.CompareExchange<Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint>>(ref OnTransferByUser, action3, a);
                    if (ReferenceEquals(onTransferByUser, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint> onTransferByUser = OnTransferByUser;
                while (true)
                {
                    Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint> source = onTransferByUser;
                    Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint> action3 = (Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint>) Delegate.Remove(source, value);
                    onTransferByUser = Interlocked.CompareExchange<Action<VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventory, VRage.Game.ModAPI.Ingame.IMyInventoryItem, MyFixedPoint>>(ref OnTransferByUser, action3, source);
                    if (ReferenceEquals(onTransferByUser, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyInventory() : this(MyFixedPoint.MaxValue, MyFixedPoint.MaxValue, Vector3.Zero, 0)
        {
        }

        public MyInventory(MyObjectBuilder_InventoryDefinition definition, MyInventoryFlags flags) : this(definition.InventoryVolume, definition.InventoryMass, new Vector3(definition.InventorySizeX, definition.InventorySizeY, definition.InventorySizeZ), flags)
        {
            this.myObjectBuilder_InventoryDefinition = definition;
        }

        public MyInventory(float maxVolume, Vector3 size, MyInventoryFlags flags) : this((MyFixedPoint) maxVolume, MyFixedPoint.MaxValue, size, flags)
        {
        }

        public MyInventory(float maxVolume, float maxMass, Vector3 size, MyInventoryFlags flags) : this((MyFixedPoint) maxVolume, (MyFixedPoint) maxMass, size, flags)
        {
        }

        public MyInventory(MyFixedPoint maxVolume, MyFixedPoint maxMass, Vector3 size, MyInventoryFlags flags) : base("Inventory")
        {
            this.m_items = new List<MyPhysicalInventoryItem>();
            this.m_maxMass = MyFixedPoint.MaxValue;
            this.m_maxVolume = MyFixedPoint.MaxValue;
            this.m_maxItemCount = 0x7fffffff;
            this.dropSound = new MySoundPair("PlayDropItem", true);
            this.m_usedIds = new HashSet<uint>();
            this.m_multiplierEnabled = true;
            this.m_maxVolume = maxVolume;
            this.m_maxMass = maxMass;
            this.m_flags = flags;
            this.SyncType = SyncHelpers.Compose(this, 0);
            this.m_currentVolume.ValueChanged += x => this.PropertiesChanged();
            this.m_currentVolume.AlwaysReject<MyFixedPoint, SyncDirection.FromServer>();
            this.m_currentMass.ValueChanged += x => this.PropertiesChanged();
            this.m_currentMass.AlwaysReject<MyFixedPoint, SyncDirection.FromServer>();
            this.m_inventoryNotEmptyNotification = new MyHudNotification(MyCommonTexts.NotificationInventoryNotEmpty, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 2, MyNotificationLevel.Normal);
            this.Clear(true);
        }

        public override bool Add(VRage.Game.ModAPI.Ingame.IMyInventoryItem item, MyFixedPoint amount)
        {
            uint? nullable1;
            if (!this.m_usedIds.Contains(item.ItemId))
            {
                nullable1 = new uint?(item.ItemId);
            }
            else
            {
                nullable1 = null;
            }
            uint? itemId = nullable1;
            return this.AddItems(amount, item.Content, itemId, -1);
        }

        private bool AddBlock(MySlimBlock block)
        {
            if ((!MyFakes.ENABLE_GATHERING_SMALL_BLOCK_FROM_GRID && (block.FatBlock != null)) && block.FatBlock.HasInventory)
            {
                return false;
            }
            MyObjectBuilder_BlockItem objectBuilder = new MyObjectBuilder_BlockItem {
                BlockDefId = (MyGridPickupComponent.Static == null) ? ((SerializableDefinitionId) block.BlockDefinition.Id) : ((SerializableDefinitionId) MyGridPickupComponent.Static.GetBaseBlock(block.BlockDefinition.Id))
            };
            if (this.ComputeAmountThatFits(objectBuilder.BlockDefId, 0f, 0f) < 1)
            {
                return false;
            }
            this.AddItems(1, objectBuilder);
            return true;
        }

        public bool AddBlockAndRemoveFromGrid(MySlimBlock block)
        {
            bool flag = false;
            if (block.FatBlock is MyCompoundCubeBlock)
            {
                foreach (MySlimBlock block2 in (block.FatBlock as MyCompoundCubeBlock).GetBlocks())
                {
                    if (this.AddBlock(block2))
                    {
                        flag = true;
                    }
                }
            }
            else if (this.AddBlock(block))
            {
                flag = true;
            }
            if (!flag)
            {
                return false;
            }
            block.CubeGrid.RazeBlock(block.Position);
            return true;
        }

        public bool AddBlocks(MyCubeBlockDefinition blockDef, MyFixedPoint amount)
        {
            MyObjectBuilder_BlockItem objectBuilder = new MyObjectBuilder_BlockItem {
                BlockDefId = (SerializableDefinitionId) blockDef.Id
            };
            if (this.ComputeAmountThatFits(objectBuilder.BlockDefId, 0f, 0f) < amount)
            {
                return false;
            }
            this.AddItems(amount, objectBuilder);
            return true;
        }

        public void AddEntity(VRage.ModAPI.IMyEntity entity, bool blockManipulatedEntity = true)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyInventory, long, bool>(this, x => new Action<long, bool>(x.AddEntity_Implementation), entity.EntityId, blockManipulatedEntity, targetEndpoint);
        }

        [Event(null, 0x777), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void AddEntity_Implementation(long entityId, bool blockManipulatedEntity)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false) && (entity != null))
            {
                this.AddEntityInternal(entity, blockManipulatedEntity);
            }
        }

        private void AddEntityInternal(VRage.ModAPI.IMyEntity ientity, bool blockManipulatedEntity = true)
        {
            VRage.Game.Entity.MyEntity entity = ientity as VRage.Game.Entity.MyEntity;
            if (entity != null)
            {
                MyDefinitionId id;
                Vector3D? hitPosition = null;
                MyCharacterDetectorComponent component = this.Owner.Components.Get<MyCharacterDetectorComponent>();
                if (component != null)
                {
                    hitPosition = new Vector3D?(component.HitPosition);
                }
                entity = this.TestEntityForPickup(entity, hitPosition, out id, blockManipulatedEntity);
                if (entity is MyCubeGrid)
                {
                    if (!this.AddGrid(entity as MyCubeGrid))
                    {
                        MyHud.Stats.GetStat<MyStatPlayerInventoryFull>().InventoryFull = true;
                    }
                }
                else if (!(entity is MyCubeBlock))
                {
                    if (entity is MyFloatingObject)
                    {
                        this.TakeFloatingObject(entity as MyFloatingObject);
                    }
                }
                else if (!this.AddBlockAndRemoveFromGrid((entity as MyCubeBlock).SlimBlock))
                {
                    MyHud.Stats.GetStat<MyStatPlayerInventoryFull>().InventoryFull = true;
                }
            }
        }

        public bool AddGrid(MyCubeGrid grid)
        {
            List<Vector3I> locations = new List<Vector3I>();
            foreach (MySlimBlock block in grid.GetBlocks())
            {
                if (block.FatBlock is MyCompoundCubeBlock)
                {
                    bool flag = false;
                    foreach (MySlimBlock block2 in (block.FatBlock as MyCompoundCubeBlock).GetBlocks())
                    {
                        if (this.AddBlock(block2))
                        {
                            if (!flag)
                            {
                                locations.Add(block.Position);
                            }
                            flag = true;
                        }
                    }
                    continue;
                }
                if (this.AddBlock(block))
                {
                    locations.Add(block.Position);
                }
            }
            if (locations.Count <= 0)
            {
                return false;
            }
            grid.RazeBlocks(locations, 0L);
            return true;
        }

        public void AddItemClient(int position, MyPhysicalInventoryItem item)
        {
            if (!Sync.IsServer)
            {
                if (position >= this.m_items.Count)
                {
                    this.m_items.Add(item);
                }
                else
                {
                    this.m_items.Insert(position, item);
                }
                this.m_usedIds.Add(item.ItemId);
                base.RaiseContentsAdded(item, item.Amount);
                this.NotifyHudChangedInventoryItem(item.Amount, ref item, true);
            }
        }

        public override bool AddItems(MyFixedPoint amount, MyObjectBuilder_Base objectBuilder)
        {
            uint? itemId = null;
            return this.AddItems(amount, objectBuilder, itemId, -1);
        }

        private bool AddItems(MyFixedPoint amount, MyObjectBuilder_Base objectBuilder, uint? itemId, int index = -1)
        {
            if (amount == 0)
            {
                return false;
            }
            MyObjectBuilder_PhysicalObject obj2 = objectBuilder as MyObjectBuilder_PhysicalObject;
            MyDefinitionId componentDefId = objectBuilder.GetId();
            if (MyFakes.ENABLE_COMPONENT_BLOCKS)
            {
                if (obj2 == null)
                {
                    obj2 = new MyObjectBuilder_BlockItem();
                    (obj2 as MyObjectBuilder_BlockItem).BlockDefId = (SerializableDefinitionId) componentDefId;
                }
                else
                {
                    MyCubeBlockDefinition definition = MyDefinitionManager.Static.TryGetComponentBlockDefinition(componentDefId);
                    if (definition != null)
                    {
                        obj2 = new MyObjectBuilder_BlockItem();
                        (obj2 as MyObjectBuilder_BlockItem).BlockDefId = (SerializableDefinitionId) definition.Id;
                    }
                }
            }
            if (obj2 == null)
            {
                return false;
            }
            componentDefId = obj2.GetObjectId();
            if (this.ComputeAmountThatFits(componentDefId, 0f, 0f) < amount)
            {
                return false;
            }
            if (Sync.IsServer)
            {
                if (this.IsConstrained)
                {
                    this.AffectAddBySurvival(ref amount, obj2);
                }
                if (amount == 0)
                {
                    return false;
                }
                this.AddItemsInternal(amount, obj2, itemId, index);
            }
            return true;
        }

        private void AddItemsInternal(MyFixedPoint amount, MyObjectBuilder_PhysicalObject objectBuilder, uint? itemId = new uint?(), int index = -1)
        {
            this.OnBeforeContentsChanged();
            MyFixedPoint maxValue = MyFixedPoint.MaxValue;
            MyInventoryItemAdapter @static = MyInventoryItemAdapter.Static;
            @static.Adapt(objectBuilder.GetObjectId());
            maxValue = @static.MaxStackAmount;
            if (!objectBuilder.CanStack(objectBuilder))
            {
                maxValue = 1;
            }
            if (MyFakes.ENABLE_DURABILITY_COMPONENT)
            {
                this.FixDurabilityForInventoryItem(objectBuilder);
            }
            bool flag = false;
            if (index >= 0)
            {
                if ((index >= this.m_items.Count) && (index < this.MaxItemCount))
                {
                    MyFixedPoint point1 = this.AddItemsToNewStack(amount, maxValue, objectBuilder, itemId, -1);
                    amount = point1;
                    flag = true;
                }
                else if (index < this.m_items.Count)
                {
                    if (this.m_items[index].Content.CanStack(objectBuilder))
                    {
                        MyFixedPoint point2 = this.AddItemsToExistingStack(index, amount, maxValue);
                        amount = point2;
                    }
                    else if (this.m_items.Count < this.MaxItemCount)
                    {
                        MyFixedPoint point3 = this.AddItemsToNewStack(amount, maxValue, objectBuilder, itemId, index);
                        amount = point3;
                        flag = true;
                    }
                }
            }
            for (int i = 0; i < this.MaxItemCount; i++)
            {
                if (i >= this.m_items.Count)
                {
                    MyFixedPoint point5 = amount;
                    amount = this.AddItemsToNewStack(point5, maxValue, flag ? ((MyObjectBuilder_PhysicalObject) objectBuilder.Clone()) : objectBuilder, itemId, -1);
                    flag = true;
                }
                else
                {
                    MyPhysicalInventoryItem item = this.m_items[i];
                    if (item.Content.CanStack(objectBuilder))
                    {
                        base.RaiseContentsAdded(item, amount);
                        MyFixedPoint point4 = this.AddItemsToExistingStack(i, amount, maxValue);
                        amount = point4;
                    }
                }
                if (amount == 0)
                {
                    break;
                }
            }
            this.RefreshVolumeAndMass();
            this.OnContentsChanged();
        }

        private unsafe MyFixedPoint AddItemsToExistingStack(int index, MyFixedPoint amount, MyFixedPoint maxStack)
        {
            MyPhysicalInventoryItem newItem = this.m_items[index];
            MyFixedPoint a = maxStack - newItem.Amount;
            if (a <= 0)
            {
                return amount;
            }
            MyFixedPoint point2 = MyFixedPoint.Min(a, amount);
            MyPhysicalInventoryItem* itemPtr1 = (MyPhysicalInventoryItem*) ref newItem;
            itemPtr1->Amount = newItem.Amount + point2;
            this.m_items[index] = newItem;
            if (Sync.IsServer)
            {
                this.NotifyHudChangedInventoryItem(point2, ref newItem, true);
            }
            return (amount - point2);
        }

        private unsafe MyFixedPoint AddItemsToNewStack(MyFixedPoint amount, MyFixedPoint maxStack, MyObjectBuilder_PhysicalObject objectBuilder, uint? itemId, int index = -1)
        {
            MyPhysicalInventoryItem* itemPtr1;
            MyFixedPoint point = MyFixedPoint.Min(amount, maxStack);
            MyPhysicalInventoryItem item = new MyPhysicalInventoryItem {
                Amount = point,
                Scale = 1f,
                Content = objectBuilder
            };
            itemPtr1->ItemId = (itemId != null) ? itemId.Value : this.GetNextItemID();
            if ((index < 0) || (index >= this.m_items.Count))
            {
                this.m_items.Add(item);
            }
            else
            {
                this.m_items.Add(this.m_items[this.m_items.Count - 1]);
                int num = this.m_items.Count - 3;
                while (true)
                {
                    if (num < index)
                    {
                        itemPtr1 = (MyPhysicalInventoryItem*) ref item;
                        this.m_items[index] = item;
                        break;
                    }
                    this.m_items[num + 1] = this.m_items[num];
                    num--;
                }
            }
            this.m_usedIds.Add(item.ItemId);
            if (Sync.IsServer)
            {
                this.NotifyHudChangedInventoryItem(point, ref item, true);
            }
            return (amount - point);
        }

        private void AffectAddBySurvival(ref MyFixedPoint amount, MyObjectBuilder_PhysicalObject objectBuilder)
        {
            MyFixedPoint point = this.ComputeAmountThatFits(objectBuilder.GetObjectId(), 0f, 0f);
            if (point < amount)
            {
                if (this.Owner is MyCharacter)
                {
                    MyCharacter c = this.Owner as MyCharacter;
                    Matrix m = (Matrix) c.GetHeadMatrix(true, true, false, false, false);
                    MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(amount - point, objectBuilder, 1f), m.Translation, m.Forward, m.Up, c.Physics, entity => entity.Physics.ApplyImpulse(m.Forward.Cross(m.Up), c.PositionComp.GetPosition()));
                }
                amount = point;
            }
        }

        public override unsafe void ApplyChanges(List<MyComponentChange> changes)
        {
            if (Sync.IsServer)
            {
                m_tmpItemsToAdd.Clear();
                bool flag = false;
                int index = 0;
                while (true)
                {
                    if (index < changes.Count)
                    {
                        MyComponentChange change = changes[index];
                        if (change.IsAddition())
                        {
                            throw new NotImplementedException();
                        }
                        if (change.Amount > 0)
                        {
                            for (int i = this.m_items.Count - 1; i >= 0; i--)
                            {
                                MyPhysicalInventoryItem item = this.m_items[i];
                                MyDefinitionId objectId = item.Content.GetId();
                                if (objectId.TypeId == typeof(MyObjectBuilder_BlockItem))
                                {
                                    objectId = item.Content.GetObjectId();
                                }
                                if (change.ToRemove == objectId)
                                {
                                    MyFixedPoint amount = change.Amount;
                                    if (amount > 0)
                                    {
                                        MyFixedPoint point2 = MyFixedPoint.Min(amount, item.Amount);
                                        amount -= point2;
                                        if (amount != 0)
                                        {
                                            change.Amount = (int) amount;
                                            changes[index] = change;
                                        }
                                        else
                                        {
                                            changes.RemoveAtFast<MyComponentChange>(index);
                                            change.Amount = 0;
                                            index--;
                                        }
                                        if ((item.Amount - point2) == 0)
                                        {
                                            this.m_usedIds.Remove(this.m_items[i].ItemId);
                                            this.m_items.RemoveAt(i);
                                        }
                                        else
                                        {
                                            MyPhysicalInventoryItem* itemPtr1 = (MyPhysicalInventoryItem*) ref item;
                                            itemPtr1->Amount = item.Amount - point2;
                                            this.m_items[i] = item;
                                        }
                                        if (change.IsChange())
                                        {
                                            int num3 = 0;
                                            m_tmpItemsToAdd.TryGetValue(change.ToAdd, out num3);
                                            num3 += (int) point2;
                                            if (num3 != 0)
                                            {
                                                m_tmpItemsToAdd[change.ToAdd] = num3;
                                            }
                                        }
                                        flag = true;
                                        this.RaiseEntityEvent(INVENTORY_CHANGED, new MyEntityContainerEventExtensions.InventoryChangedParams(item.ItemId, this, (float) item.Amount));
                                    }
                                }
                            }
                        }
                        index++;
                        continue;
                    }
                    this.RefreshVolumeAndMass();
                    using (Dictionary<MyDefinitionId, int>.Enumerator enumerator = m_tmpItemsToAdd.GetEnumerator())
                    {
                        while (true)
                        {
                            if (enumerator.MoveNext())
                            {
                                KeyValuePair<MyDefinitionId, int> current = enumerator.Current;
                                MyCubeBlockDefinition componentBlockDefinition = MyDefinitionManager.Static.GetComponentBlockDefinition(current.Key);
                                if (componentBlockDefinition != null)
                                {
                                    this.AddBlocks(componentBlockDefinition, current.Value);
                                    flag = true;
                                    this.RefreshVolumeAndMass();
                                    continue;
                                }
                            }
                            break;
                        }
                    }
                    if (flag)
                    {
                        this.RefreshVolumeAndMass();
                        this.OnContentsChanged();
                    }
                    break;
                }
            }
        }

        public bool CanItemsBeAdded(MyFixedPoint amount, MyDefinitionId contentId) => 
            ((amount != 0) ? ((base.Entity != null) && (!base.Entity.MarkedForClose && (this.CanItemsBeAdded(amount, contentId, this.MaxVolume, this.MaxMass, (MyFixedPoint) this.m_currentVolume, (MyFixedPoint) this.m_currentMass) && this.CheckConstraint(contentId)))) : true);

        public bool CanItemsBeAdded(MyFixedPoint amount, MyDefinitionId contentId, MyFixedPoint maxVolume, MyFixedPoint maxMass, MyFixedPoint currentVolume, MyFixedPoint currentMass)
        {
            MyInventoryItemAdapter @static = MyInventoryItemAdapter.Static;
            @static.Adapt(contentId);
            return ((!this.IsConstrained || (((amount * @static.Volume) + currentVolume) <= maxVolume)) && (((amount * @static.Mass) + currentMass) <= maxMass));
        }

        private bool CanTransferTo(MyInventory dstInventory, MyDefinitionId? itemType)
        {
            ConnectionData data;
            IMyConveyorEndpointBlock owner = this.Owner as IMyConveyorEndpointBlock;
            IMyConveyorEndpointBlock end = dstInventory.Owner as IMyConveyorEndpointBlock;
            if ((owner == null) || (end == null))
            {
                return false;
            }
            LRUCache<ConnectionKey, ConnectionData> connectionCache = this.m_connectionCache;
            if (connectionCache == null)
            {
                Interlocked.CompareExchange<LRUCache<ConnectionKey, ConnectionData>>(ref this.m_connectionCache, new LRUCache<ConnectionKey, ConnectionData>(0x19, null), null);
                connectionCache = this.m_connectionCache;
            }
            int gameplayFrameCounter = MySession.Static.GameplayFrameCounter;
            ConnectionKey key = new ConnectionKey(owner.ConveyorEndpoint.CubeBlock.EntityId, itemType);
            if (connectionCache.TryPeek(key, out data) && (data.Frame == gameplayFrameCounter))
            {
                return data.HasConnection;
            }
            bool flag = MyGridConveyorSystem.ComputeCanTransfer(owner, end, itemType);
            ConnectionData data2 = new ConnectionData {
                Frame = gameplayFrameCounter,
                HasConnection = flag
            };
            connectionCache.Write(key, data2);
            return flag;
        }

        public void ChangeItemClient(MyPhysicalInventoryItem item, int position)
        {
            if ((position >= 0) && (position < this.m_items.Count))
            {
                this.m_items[position] = item;
            }
        }

        public bool CheckConstraint(MyDefinitionId contentId) => 
            ((this.Constraint == null) || this.Constraint.Check(contentId));

        public void Clear(bool sync = true)
        {
            if (!sync)
            {
                this.m_items.Clear();
                this.m_usedIds.Clear();
                this.RefreshVolumeAndMass();
            }
            else
            {
                for (int i = this.m_items.Count - 1; i >= 0; i--)
                {
                    MyFixedPoint? amount = null;
                    MatrixD? spawnPos = null;
                    this.RemoveItems(this.m_items[i].ItemId, amount, true, false, spawnPos);
                }
            }
        }

        public void ClearItems()
        {
            this.m_items.Clear();
            this.m_usedIds.Clear();
        }

        public MyFixedPoint ComputeAmountThatFits(MyBlueprintDefinitionBase blueprint)
        {
            if (!this.IsConstrained)
            {
                return MyFixedPoint.MaxValue;
            }
            MyFixedPoint a = MyFixedPoint.Max((this.MaxVolume - this.m_currentVolume) * (1f / blueprint.OutputVolume), 0);
            if (blueprint.Atomic)
            {
                a = MyFixedPoint.Floor(a);
            }
            return a;
        }

        public override MyFixedPoint ComputeAmountThatFits(MyDefinitionId contentId, float volumeRemoved = 0f, float massRemoved = 0f)
        {
            if (!this.IsConstrained)
            {
                return MyFixedPoint.MaxValue;
            }
            MyInventoryItemAdapter @static = MyInventoryItemAdapter.Static;
            @static.Adapt(contentId);
            MyFixedPoint a = MyFixedPoint.Min(MyFixedPoint.Max((MyFixedPoint) ((((double) this.MaxVolume) - Math.Max((double) (((double) this.m_currentVolume.Value) - (volumeRemoved * @static.Volume)), (double) 0.0)) * (1.0 / ((double) @static.Volume))), 0), MyFixedPoint.Max((MyFixedPoint) ((((double) this.MaxMass) - Math.Max((double) (((double) this.m_currentMass.Value) - (massRemoved * @static.Mass)), (double) 0.0)) * (1.0 / ((double) @static.Mass))), 0));
            if (this.MaxItemCount != 0x7fffffff)
            {
                a = MyFixedPoint.Min(a, this.FindFreeSlotSpace(contentId, @static));
            }
            if (@static.HasIntegralAmounts)
            {
                a = MyFixedPoint.Floor(a);
            }
            return a;
        }

        public override void ConsumeItem(MyDefinitionId itemId, MyFixedPoint amount, long consumerEntityId = 0L)
        {
            SerializableDefinitionId id = (SerializableDefinitionId) itemId;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyInventory, MyFixedPoint, SerializableDefinitionId, long>(this, x => new Action<MyFixedPoint, SerializableDefinitionId, long>(x.InventoryConsumeItem_Implementation), amount, id, consumerEntityId, targetEndpoint);
        }

        public bool ContainItems(MyFixedPoint amount, MyObjectBuilder_PhysicalObject ob) => 
            ((ob != null) ? this.ContainItems(new MyFixedPoint?(amount), ob.GetObjectId(), MyItemFlags.None) : false);

        public bool ContainItems(MyFixedPoint? amount, MyDefinitionId contentId, MyItemFlags flags = 0)
        {
            MyFixedPoint point = this.GetItemAmount(contentId, flags, false);
            if (amount == null)
            {
                return (point > 0);
            }
            MyFixedPoint point2 = point;
            MyFixedPoint? nullable = amount;
            return ((nullable != null) ? (point2 >= nullable.GetValueOrDefault()) : false);
        }

        public override void CountItems(Dictionary<MyDefinitionId, MyFixedPoint> itemCounts)
        {
            foreach (MyPhysicalInventoryItem item in this.m_items)
            {
                MyDefinitionId objectId = item.Content.GetId();
                if (objectId.TypeId == typeof(MyObjectBuilder_BlockItem))
                {
                    objectId = item.Content.GetObjectId();
                }
                if (!objectId.TypeId.IsNull && (objectId.SubtypeId != MyStringHash.NullOrEmpty))
                {
                    if (MySessionComponentEquivalency.Static != null)
                    {
                        objectId = MySessionComponentEquivalency.Static.GetMainElement(objectId);
                    }
                    MyFixedPoint point = 0;
                    itemCounts.TryGetValue(objectId, out point);
                    itemCounts[objectId] = point + ((int) item.Amount);
                }
            }
        }

        public void DebugAddItems(MyFixedPoint amount, MyObjectBuilder_Base objectBuilder)
        {
        }

        [Event(null, 0x824), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void DebugAddItems_Implementation(MyFixedPoint amount, [DynamicObjectBuilder(false)] MyObjectBuilder_Base objectBuilder)
        {
            MyLog.Default.WriteLine("DebugAddItems not supported on OFFICIAL builds (it's cheating)");
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            MyObjectBuilder_Inventory objectBuilder = builder as MyObjectBuilder_Inventory;
            this.Init(objectBuilder);
        }

        public void DropItem(int itemIndex, MyFixedPoint amount)
        {
            uint itemId = this.m_items[itemIndex].ItemId;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyInventory, MyFixedPoint, uint>(this, x => new Action<MyFixedPoint, uint>(x.DropItem_Implementation), amount, itemId, targetEndpoint);
        }

        [Event(null, 0x831), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void DropItem_Implementation(MyFixedPoint amount, uint itemIndex)
        {
            if (MyVisualScriptLogicProvider.PlayerDropped != null)
            {
                MyCharacter owner = this.Owner as MyCharacter;
                if (owner != null)
                {
                    MyPhysicalInventoryItem? itemByID = this.GetItemByID(itemIndex);
                    long controllingIdentityId = owner.ControllerInfo.ControllingIdentityId;
                    MyVisualScriptLogicProvider.PlayerDropped(itemByID.Value.Content.TypeId.ToString(), itemByID.Value.Content.SubtypeName, controllingIdentityId, amount.ToIntSafe());
                }
            }
            MatrixD? spawnPos = null;
            this.RemoveItems(itemIndex, new MyFixedPoint?(amount), true, true, spawnPos);
        }

        public void DropItemById(uint itemId, MyFixedPoint amount)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyInventory, MyFixedPoint, uint>(this, x => new Action<MyFixedPoint, uint>(x.DropItem_Implementation), amount, itemId, targetEndpoint);
        }

        public bool Empty() => 
            (this.m_items.Count == 0);

        public bool FilterItemsUsingConstraint()
        {
            bool flag = false;
            for (int i = this.m_items.Count - 1; i >= 0; i--)
            {
                if (!this.CheckConstraint(this.m_items[i].Content.GetObjectId()))
                {
                    MyFixedPoint? amount = null;
                    MatrixD? spawnPos = null;
                    this.RemoveItems(this.m_items[i].ItemId, amount, false, false, spawnPos);
                    flag = true;
                }
            }
            if (flag)
            {
                this.OnContentsChanged();
            }
            return flag;
        }

        private int? FindFirstPositionOfType(MyDefinitionId contentId, MyItemFlags flags = 0)
        {
            for (int i = 0; i < this.m_items.Count; i++)
            {
                MyObjectBuilder_PhysicalObject content = this.m_items[i].Content;
                if ((content.GetObjectId() == contentId) && (content.Flags == flags))
                {
                    return new int?(i);
                }
            }
            return null;
        }

        private int? FindFirstStackablePosition(MyObjectBuilder_PhysicalObject toStack, MyFixedPoint wantedAmount)
        {
            for (int i = 0; i < this.m_items.Count; i++)
            {
                if (this.m_items[i].Content.CanStack(toStack) && (this.m_items[i].Amount <= wantedAmount))
                {
                    return new int?(i);
                }
            }
            return null;
        }

        public MyFixedPoint FindFreeSlotSpace(MyDefinitionId contentId, IMyInventoryItemAdapter adapter)
        {
            MyFixedPoint a = 0;
            MyFixedPoint maxStackAmount = adapter.MaxStackAmount;
            for (int i = 0; i < this.m_items.Count; i++)
            {
                if (this.m_items[i].Content.CanStack(contentId.TypeId, contentId.SubtypeId, MyItemFlags.None))
                {
                    a = MyFixedPoint.AddSafe(a, maxStackAmount - this.m_items[i].Amount);
                }
            }
            int num = this.MaxItemCount - this.m_items.Count;
            if (num > 0)
            {
                a = MyFixedPoint.AddSafe(a, maxStackAmount * num);
            }
            return a;
        }

        public MyPhysicalInventoryItem? FindItem(Func<MyPhysicalInventoryItem, bool> predicate)
        {
            using (List<MyPhysicalInventoryItem>.Enumerator enumerator = this.m_items.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyPhysicalInventoryItem current = enumerator.Current;
                    if (predicate(current))
                    {
                        return new MyPhysicalInventoryItem?(current);
                    }
                }
            }
            return null;
        }

        public MyPhysicalInventoryItem? FindItem(MyDefinitionId contentId)
        {
            int? nullable = this.FindFirstPositionOfType(contentId, MyItemFlags.None);
            if (nullable != null)
            {
                return new MyPhysicalInventoryItem?(this.m_items[nullable.Value]);
            }
            return null;
        }

        public MyPhysicalInventoryItem? FindUsableItem(MyDefinitionId contentId)
        {
            if (!MyFakes.ENABLE_DURABILITY_COMPONENT)
            {
                return this.FindItem(contentId);
            }
            int startPosition = -1;
            while (this.TryFindNextPositionOfTtype(contentId, startPosition, out startPosition) && this.m_items.IsValidIndex<MyPhysicalInventoryItem>(startPosition))
            {
                if (((this.m_items[startPosition].Content == null) || (this.m_items[startPosition].Content.DurabilityHP == null)) || (this.m_items[startPosition].Content.DurabilityHP.Value > 0f))
                {
                    return new MyPhysicalInventoryItem?(this.m_items[startPosition]);
                }
            }
            return null;
        }

        private void FixDurabilityForInventoryItem(MyObjectBuilder_PhysicalObject objectBuilder)
        {
            MyPhysicalItemDefinition definition = null;
            if (MyDefinitionManager.Static.TryGetPhysicalItemDefinition(objectBuilder.GetId(), out definition))
            {
                MyContainerDefinition definition2 = null;
                if (!MyComponentContainerExtension.TryGetContainerDefinition(definition.Id.TypeId, definition.Id.SubtypeId, out definition2) && (objectBuilder.GetObjectId().TypeId == typeof(MyObjectBuilder_PhysicalGunObject)))
                {
                    MyHandItemDefinition definition3 = MyDefinitionManager.Static.TryGetHandItemForPhysicalItem(objectBuilder.GetObjectId());
                    if (definition3 != null)
                    {
                        MyComponentContainerExtension.TryGetContainerDefinition(definition3.Id.TypeId, definition3.Id.SubtypeId, out definition2);
                    }
                }
                if (((definition2 != null) && definition2.HasDefaultComponent("MyObjectBuilder_EntityDurabilityComponent")) && (objectBuilder.DurabilityHP == null))
                {
                    objectBuilder.DurabilityHP = 100f;
                }
            }
        }

        public void FixInventoryVolume(float newValue)
        {
            if (this.m_maxVolume == MyFixedPoint.MaxValue)
            {
                this.m_maxVolume = (MyFixedPoint) newValue;
            }
        }

        private static void FixTransferAmount(MyInventory src, MyInventory dst, MyPhysicalInventoryItem? srcItem, bool spawn, ref MyFixedPoint remove, ref MyFixedPoint add)
        {
            if (srcItem.Value.Amount < remove)
            {
                remove = srcItem.Value.Amount;
                add = remove;
            }
            if (dst.IsConstrained && !ReferenceEquals(src, dst))
            {
                MyFixedPoint point = dst.ComputeAmountThatFits(srcItem.Value.Content.GetObjectId(), 0f, 0f);
                if (point < remove)
                {
                    if (!spawn)
                    {
                        remove = point;
                    }
                    else
                    {
                        VRage.Game.Entity.MyEntity owner = dst.Owner;
                        Matrix worldMatrix = (Matrix) owner.WorldMatrix;
                        MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(remove - point, srcItem.Value.Content, 1f), (owner.PositionComp.GetPosition() + worldMatrix.Forward) + worldMatrix.Up, worldMatrix.Forward, worldMatrix.Up, owner.Physics, null);
                    }
                    add = point;
                }
            }
        }

        public void GenerateContent(MyContainerTypeDefinition containerDefinition)
        {
            int randomInt = MyUtils.GetRandomInt(containerDefinition.CountMin, containerDefinition.CountMax);
            int num2 = 0;
            while (true)
            {
                while (true)
                {
                    if (num2 >= randomInt)
                    {
                        containerDefinition.DeselectAll();
                        return;
                    }
                    MyContainerTypeDefinition.ContainerTypeItem item = containerDefinition.SelectNextRandomItem();
                    MyFixedPoint a = (MyFixedPoint) MyRandom.Instance.NextFloat(((float) item.AmountMin), ((float) item.AmountMax));
                    if (this.ContainItems(1, item.DefinitionId, MyItemFlags.None))
                    {
                        a -= this.GetItemAmount(item.DefinitionId, MyItemFlags.None, false);
                        if (a <= 0)
                        {
                            break;
                        }
                    }
                    if (MyDefinitionManager.Static.GetPhysicalItemDefinition(item.DefinitionId).HasIntegralAmounts)
                    {
                        a = MyFixedPoint.Ceiling(a);
                    }
                    a = MyFixedPoint.Min(this.ComputeAmountThatFits(item.DefinitionId, 0f, 0f), a);
                    if (a > 0)
                    {
                        this.AddItems(a, (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) item.DefinitionId));
                    }
                    break;
                }
                num2++;
            }
        }

        public MyInventoryFlags GetFlags() => 
            this.m_flags;

        public override int GetInventoryCount() => 
            1;

        public override MyFixedPoint GetItemAmount(MyDefinitionId contentId, MyItemFlags flags = 0, bool substitute = false)
        {
            MyFixedPoint point = 0;
            foreach (MyPhysicalInventoryItem item in this.m_items)
            {
                MyDefinitionId objectId = item.Content.GetId();
                if ((contentId != objectId) && (item.Content.TypeId == typeof(MyObjectBuilder_BlockItem)))
                {
                    objectId = item.Content.GetObjectId();
                }
                if (substitute && (MySessionComponentEquivalency.Static != null))
                {
                    objectId = MySessionComponentEquivalency.Static.GetMainElement(objectId);
                    MyDefinitionId mainElement = MySessionComponentEquivalency.Static.GetMainElement(contentId);
                    contentId = mainElement;
                }
                if ((objectId == contentId) && (item.Content.Flags == flags))
                {
                    point += item.Amount;
                }
            }
            return point;
        }

        public MyInventoryItem? GetItemAt(int index)
        {
            if (this.IsItemAt(index))
            {
                return new MyInventoryItem?(this.m_items[index].MakeAPIItem());
            }
            return null;
        }

        public MyPhysicalInventoryItem? GetItemByID(uint id)
        {
            using (List<MyPhysicalInventoryItem>.Enumerator enumerator = this.m_items.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyPhysicalInventoryItem current = enumerator.Current;
                    if (current.ItemId == id)
                    {
                        return new MyPhysicalInventoryItem?(current);
                    }
                }
            }
            return null;
        }

        public MyPhysicalInventoryItem? GetItemByIndex(int id)
        {
            if ((id >= 0) && (id < this.m_items.Count))
            {
                return new MyPhysicalInventoryItem?(this.m_items[id]);
            }
            return null;
        }

        public int GetItemIndexById(uint id)
        {
            for (int i = 0; i < this.m_items.Count; i++)
            {
                if (this.m_items[i].ItemId == id)
                {
                    return i;
                }
            }
            return -1;
        }

        public override List<MyPhysicalInventoryItem> GetItems() => 
            this.m_items;

        public override int GetItemsCount() => 
            this.m_items.Count;

        public static void GetItemVolumeAndMass(MyDefinitionId contentId, out float itemMass, out float itemVolume)
        {
            MyInventoryItemAdapter @static = MyInventoryItemAdapter.Static;
            if (@static.TryAdapt(contentId))
            {
                itemMass = @static.Mass;
                itemVolume = @static.Volume;
            }
            else
            {
                itemMass = 0f;
                itemVolume = 0f;
            }
        }

        private uint GetNextItemID()
        {
            while (!this.IsUniqueId(this.m_nextItemID))
            {
                this.m_nextItemID = (this.m_nextItemID != uint.MaxValue) ? (this.m_nextItemID + 1) : 0;
            }
            uint nextItemID = this.m_nextItemID;
            this.m_nextItemID = nextItemID + 1;
            return nextItemID;
        }

        public MyObjectBuilder_Inventory GetObjectBuilder()
        {
            MyObjectBuilder_Inventory inventory = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Inventory>();
            inventory.Items.Clear();
            inventory.Mass = new MyFixedPoint?(this.m_maxMass);
            inventory.Volume = new MyFixedPoint?(this.m_maxVolume);
            inventory.MaxItemCount = new int?(this.m_maxItemCount);
            inventory.InventoryFlags = new MyInventoryFlags?(this.m_flags);
            inventory.nextItemId = this.m_nextItemID;
            inventory.RemoveEntityOnEmpty = base.RemoveEntityOnEmpty;
            foreach (MyPhysicalInventoryItem item in this.m_items)
            {
                inventory.Items.Add(item.GetObjectBuilder());
            }
            return inventory;
        }

        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
            MyInventoryComponentDefinition definition2 = definition as MyInventoryComponentDefinition;
            if (definition2 != null)
            {
                this.m_maxVolume = (MyFixedPoint) definition2.Volume;
                this.m_maxMass = (MyFixedPoint) definition2.Mass;
                base.RemoveEntityOnEmpty = definition2.RemoveEntityOnEmpty;
                this.m_multiplierEnabled = definition2.MultiplierEnabled;
                this.m_maxItemCount = definition2.MaxItemCount;
                this.Constraint = definition2.InputConstraint;
            }
        }

        public void Init(MyObjectBuilder_Inventory objectBuilder)
        {
            this.Clear(false);
            if (objectBuilder == null)
            {
                if (this.myObjectBuilder_InventoryDefinition != null)
                {
                    this.m_maxMass = (MyFixedPoint) this.myObjectBuilder_InventoryDefinition.InventoryMass;
                    this.m_maxVolume = (MyFixedPoint) this.myObjectBuilder_InventoryDefinition.InventoryVolume;
                    this.m_maxItemCount = this.myObjectBuilder_InventoryDefinition.MaxItemCount;
                }
            }
            else
            {
                if (objectBuilder.Mass != null)
                {
                    this.m_maxMass = objectBuilder.Mass.Value;
                }
                if (objectBuilder.Volume != null)
                {
                    MyFixedPoint point = objectBuilder.Volume.Value;
                    if ((point != MyFixedPoint.MaxValue) || !this.IsConstrained)
                    {
                        this.m_maxVolume = point;
                    }
                }
                if (objectBuilder.MaxItemCount != null)
                {
                    this.m_maxItemCount = objectBuilder.MaxItemCount.Value;
                }
                if (objectBuilder.InventoryFlags != null)
                {
                    this.m_flags = objectBuilder.InventoryFlags.Value;
                }
                base.RemoveEntityOnEmpty = objectBuilder.RemoveEntityOnEmpty;
                this.m_nextItemID = !(!Sync.IsServer || MySession.Static.Ready) ? 0 : objectBuilder.nextItemId;
                this.m_objectBuilder = objectBuilder;
                if (base.Entity != null)
                {
                    this.InitItems();
                }
            }
        }

        private void InitItems()
        {
            if (this.m_objectBuilder != null)
            {
                bool flag = !Sync.IsServer || MySession.Static.Ready;
                int index = 0;
                foreach (MyObjectBuilder_InventoryItem item in this.m_objectBuilder.Items)
                {
                    if (item.Amount <= 0)
                    {
                        continue;
                    }
                    if ((item.PhysicalContent != null) && MyInventoryItemAdapter.Static.TryAdapt(item.PhysicalContent.GetObjectId()))
                    {
                        MyDefinitionId objectId = item.PhysicalContent.GetObjectId();
                        MyFixedPoint amount = MyFixedPoint.Min(this.ComputeAmountThatFits(objectId, 0f, 0f), item.Amount);
                        if (amount != MyFixedPoint.Zero)
                        {
                            uint? nullable;
                            if (item.PhysicalContent.CanStack(item.PhysicalContent))
                            {
                                uint? nullable2;
                                if (flag)
                                {
                                    nullable2 = new uint?(item.ItemId);
                                }
                                else
                                {
                                    nullable = null;
                                    nullable2 = nullable;
                                }
                                this.AddItemsInternal(amount, item.PhysicalContent, nullable2, index);
                            }
                            else
                            {
                                MyFixedPoint point2 = 0;
                                while (point2 < amount)
                                {
                                    uint? nullable1;
                                    if (flag)
                                    {
                                        nullable1 = new uint?(item.ItemId);
                                    }
                                    else
                                    {
                                        nullable = null;
                                        nullable1 = nullable;
                                    }
                                    this.AddItemsInternal(1, item.PhysicalContent, nullable1, index);
                                    point2 += 1;
                                    index++;
                                }
                            }
                            index++;
                        }
                    }
                }
                this.m_objectBuilder = null;
            }
        }

        private void InitSpawned(VRage.Game.Entity.MyEntity spawned, VRage.Game.Entity.MyEntity owner, MatrixD? spawnPos)
        {
            if ((spawned != null) && (spawnPos != null))
            {
                if (ReferenceEquals(owner, MySession.Static.LocalCharacter))
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.PlayDropItem);
                }
                else
                {
                    MyEntity3DSoundEmitter emitter = MyAudioComponent.TryGetSoundEmitter();
                    if (emitter != null)
                    {
                        emitter.SetPosition(new Vector3D?(spawnPos.Value.Translation));
                        bool? nullable = null;
                        emitter.PlaySound(this.dropSound, false, false, false, false, false, nullable);
                    }
                }
            }
        }

        [Event(null, 0x8bd), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void InventoryConsumeItem_Implementation(MyFixedPoint amount, SerializableDefinitionId itemId, long consumerEntityId)
        {
            if ((consumerEntityId == 0) || Sandbox.Game.Entities.MyEntities.EntityExists(consumerEntityId))
            {
                MyFixedPoint point = this.GetItemAmount(itemId, MyItemFlags.None, false);
                if (point < amount)
                {
                    amount = point;
                }
                VRage.Game.Entity.MyEntity entityById = null;
                if (consumerEntityId != 0)
                {
                    entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(consumerEntityId, false);
                    if (entityById == null)
                    {
                        return;
                    }
                }
                if (entityById.Components != null)
                {
                    MyUsableItemDefinition definition = MyDefinitionManager.Static.GetDefinition(itemId) as MyUsableItemDefinition;
                    if (definition != null)
                    {
                        MyCharacter character = entityById as MyCharacter;
                        if (character != null)
                        {
                            character.SoundComp.StartSecondarySound(definition.UseSound, true);
                        }
                        MyConsumableItemDefinition definition2 = definition as MyConsumableItemDefinition;
                        if (definition2 != null)
                        {
                            MyCharacterStatComponent component = entityById.Components.Get<MyEntityStatComponent>() as MyCharacterStatComponent;
                            if (component != null)
                            {
                                component.Consume(amount, definition2);
                            }
                        }
                    }
                }
                if (1 != 0)
                {
                    this.RemoveItemsOfType(amount, itemId, MyItemFlags.None, false);
                }
            }
        }

        [Event(null, 0x819), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void InventoryTransferItem_Implementation(MyFixedPoint amount, uint itemId, long destinationOwnerId, byte destInventoryIndex, int destinationIndex)
        {
            if (Sandbox.Game.Entities.MyEntities.EntityExists(destinationOwnerId))
            {
                MyInventory dst = Sandbox.Game.Entities.MyEntities.GetEntityById(destinationOwnerId, false).GetInventory(destInventoryIndex);
                TransferItemsInternal(this, dst, itemId, false, destinationIndex, amount);
            }
        }

        public bool IsItemAt(int position) => 
            this.m_items.IsValidIndex<MyPhysicalInventoryItem>(position);

        public bool IsUniqueId(uint idToTest) => 
            !this.m_usedIds.Contains(idToTest);

        public override bool ItemsCanBeAdded(MyFixedPoint amount, VRage.Game.ModAPI.Ingame.IMyInventoryItem item) => 
            ((item != null) ? this.CanItemsBeAdded(amount, item.GetDefinitionId()) : false);

        public override bool ItemsCanBeRemoved(MyFixedPoint amount, VRage.Game.ModAPI.Ingame.IMyInventoryItem item)
        {
            if (amount == 0)
            {
                return true;
            }
            if (item == null)
            {
                return false;
            }
            MyPhysicalInventoryItem? itemByID = this.GetItemByID(item.ItemId);
            return ((itemByID != null) && (itemByID.Value.Amount >= amount));
        }

        public override MyInventoryBase IterateInventory(int searchIndex, int currentIndex = 0) => 
            ((currentIndex == searchIndex) ? this : null);

        private void NotifyHudChangedInventoryItem(MyFixedPoint amount, ref MyPhysicalInventoryItem newItem, bool added)
        {
            if ((MyFakes.ENABLE_HUD_PICKED_UP_ITEMS && ((base.Entity != null) && ((this.Owner is MyCharacter) && MyHud.ChangedInventoryItems.Visible))) && ((this.Owner as MyCharacter).GetPlayerIdentityId() == MySession.Static.LocalPlayerId))
            {
                MyHud.ChangedInventoryItems.AddChangedPhysicalInventoryItem(newItem, amount, added);
            }
        }

        public override void OnAddedToContainer()
        {
            this.InitItems();
            base.OnAddedToContainer();
        }

        public override void OnBeforeContentsChanged()
        {
            base.RaiseBeforeContentsChanged();
        }

        public override void OnContentsAdded(MyPhysicalInventoryItem item, MyFixedPoint amount)
        {
            base.RaiseContentsAdded(item, amount);
        }

        public override void OnContentsChanged()
        {
            base.RaiseContentsChanged();
            if ((Sync.IsServer && base.RemoveEntityOnEmpty) && (this.GetItemsCount() == 0))
            {
                base.Container.Entity.Close();
            }
        }

        public override void OnContentsRemoved(MyPhysicalInventoryItem item, MyFixedPoint amount)
        {
            base.RaiseContentsRemoved(item, amount);
        }

        public void PickupItem(MyFloatingObject obj, MyFixedPoint amount)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyInventory, long, MyFixedPoint>(this, x => new Action<long, MyFixedPoint>(x.PickupItem_Implementation), obj.EntityId, amount, targetEndpoint);
        }

        [Event(null, 0x2de), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void PickupItem_Implementation(long entityId, MyFixedPoint amount)
        {
            MyFloatingObject obj2;
            if ((Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyFloatingObject>(entityId, out obj2, false) && ((obj2 != null) && !obj2.MarkedForClose)) && !obj2.WasRemovedFromWorld)
            {
                MyFixedPoint point1 = MyFixedPoint.Min(amount, obj2.Item.Amount);
                amount = point1;
                MyFixedPoint point2 = MyFixedPoint.Min(amount, this.ComputeAmountThatFits(obj2.Item.Content.GetObjectId(), 0f, 0f));
                amount = point2;
                if (this.AddItems(amount, obj2.Item.Content))
                {
                    if (amount < obj2.Item.Amount)
                    {
                        MyFloatingObjects.AddFloatingObjectAmount(obj2, -amount);
                    }
                    else
                    {
                        MyFloatingObjects.RemoveFloatingObject(obj2, true);
                        if (MyVisualScriptLogicProvider.PlayerPickedUp != null)
                        {
                            MyCharacter owner = this.Owner as MyCharacter;
                            if (owner != null)
                            {
                                long controllingIdentityId = owner.ControllerInfo.ControllingIdentityId;
                                MyVisualScriptLogicProvider.PlayerPickedUp(obj2.ItemDefinition.Id.TypeId.ToString(), obj2.ItemDefinition.Id.SubtypeName, obj2.Name, controllingIdentityId, amount.ToIntSafe());
                            }
                        }
                    }
                }
            }
        }

        private void PropertiesChanged()
        {
            if (!Sync.IsServer)
            {
                this.OnContentsChanged();
            }
        }

        public void Refresh()
        {
            this.RefreshVolumeAndMass();
            this.OnContentsChanged();
        }

        private void RefreshVolumeAndMass()
        {
            if (Sync.IsServer)
            {
                this.m_currentMass.Value = 0;
                this.m_currentVolume.Value = 0;
                MyFixedPoint point = 0;
                MyFixedPoint point2 = 0;
                foreach (MyPhysicalInventoryItem item in this.m_items)
                {
                    MyInventoryItemAdapter @static = MyInventoryItemAdapter.Static;
                    @static.Adapt(item);
                    point += @static.Mass * item.Amount;
                    point2 += @static.Volume * item.Amount;
                }
                this.m_currentMass.Value = point;
                this.m_currentVolume.Value = point2;
            }
        }

        public override bool Remove(VRage.Game.ModAPI.Ingame.IMyInventoryItem item, MyFixedPoint amount)
        {
            if (!(item.Content is MyObjectBuilder_PhysicalObject))
            {
                return false;
            }
            int itemIndexById = this.GetItemIndexById(item.ItemId);
            if (itemIndexById == -1)
            {
                return this.RemoveItemsOfType(amount, item.Content as MyObjectBuilder_PhysicalObject, false, true);
            }
            MatrixD? spawnPos = null;
            this.RemoveItemsAt(itemIndexById, new MyFixedPoint?(amount), true, false, spawnPos);
            return true;
        }

        public void RemoveItemClient(uint itemId)
        {
            if (!Sync.IsServer)
            {
                int index = -1;
                int num2 = 0;
                while (true)
                {
                    if (num2 < this.m_items.Count)
                    {
                        if (this.m_items[num2].ItemId != itemId)
                        {
                            num2++;
                            continue;
                        }
                        index = num2;
                    }
                    if (index != -1)
                    {
                        MyPhysicalInventoryItem newItem = this.m_items[index];
                        this.NotifyHudChangedInventoryItem(newItem.Amount, ref newItem, false);
                        this.m_items.RemoveAt(index);
                        this.m_usedIds.Remove(itemId);
                    }
                    return;
                }
            }
        }

        public void RemoveItems(uint itemId, MyFixedPoint? amount = new MyFixedPoint?(), bool sendEvent = true, bool spawn = false, MatrixD? spawnPos = new MatrixD?())
        {
            MyPhysicalInventoryItem? itemByID = this.GetItemByID(itemId);
            MyFixedPoint? nullable2 = amount;
            MyFixedPoint point = (nullable2 != null) ? nullable2.GetValueOrDefault() : ((itemByID != null) ? itemByID.Value.Amount : 1);
            if ((Sync.IsServer && ((itemByID != null) && this.RemoveItemsInternal(itemId, point, sendEvent))) && spawn)
            {
                if (spawnPos == null)
                {
                    spawnPos = new MatrixD?(MatrixD.CreateWorld((this.Owner.PositionComp.GetPosition() + this.Owner.PositionComp.WorldMatrix.Forward) + this.Owner.PositionComp.WorldMatrix.Up, this.Owner.PositionComp.WorldMatrix.Forward, this.Owner.PositionComp.WorldMatrix.Up));
                }
                itemByID.Value.Spawn(point, spawnPos.Value, this.Owner, spawned => this.InitSpawned(spawned, this.Owner, spawnPos));
            }
        }

        public void RemoveItemsAt(int itemIndex, MyFixedPoint? amount = new MyFixedPoint?(), bool sendEvent = true, bool spawn = false, MatrixD? spawnPos = new MatrixD?())
        {
            if ((itemIndex >= 0) && (itemIndex < this.m_items.Count))
            {
                if (Sync.IsServer)
                {
                    this.RemoveItems(this.m_items[itemIndex].ItemId, amount, sendEvent, spawn, spawnPos);
                }
                else
                {
                    EndpointId targetEndpoint = new EndpointId();
                    MyMultiplayer.RaiseEvent<MyInventory, int, MyFixedPoint?, bool, bool, MatrixD?>(this, x => new Action<int, MyFixedPoint?, bool, bool, MatrixD?>(x.RemoveItemsAt_Request), itemIndex, amount, sendEvent, spawn, spawnPos, targetEndpoint);
                }
            }
        }

        [Event(null, 0x421), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void RemoveItemsAt_Request(int itemIndex, MyFixedPoint? amount = new MyFixedPoint?(), bool sendEvent = true, bool spawn = false, MatrixD? spawnPos = new MatrixD?())
        {
            this.RemoveItemsAt(itemIndex, amount, sendEvent, spawn, spawnPos);
        }

        public unsafe bool RemoveItemsInternal(uint itemId, MyFixedPoint amount, bool sendEvent = true)
        {
            if (sendEvent)
            {
                this.OnBeforeContentsChanged();
            }
            bool flag = false;
            int index = 0;
            while (true)
            {
                if (index < this.m_items.Count)
                {
                    if (this.m_items[index].ItemId != itemId)
                    {
                        index++;
                        continue;
                    }
                    MyPhysicalInventoryItem item = this.m_items[index];
                    MyFixedPoint point1 = MathHelper.Clamp(amount, 0, this.m_items[index].Amount);
                    amount = point1;
                    MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref item.Amount;
                    pointPtr1[0] -= amount;
                    if (item.Amount != 0)
                    {
                        this.m_items[index] = item;
                    }
                    else
                    {
                        this.m_usedIds.Remove(this.m_items[index].ItemId);
                        this.m_items.RemoveAt(index);
                    }
                    flag = true;
                    this.RaiseEntityEvent(INVENTORY_CHANGED, new MyEntityContainerEventExtensions.InventoryChangedParams(item.ItemId, this, (float) item.Amount));
                    if (sendEvent)
                    {
                        base.RaiseContentsRemoved(item, amount);
                    }
                    if (Sync.IsServer)
                    {
                        this.NotifyHudChangedInventoryItem(amount, ref item, false);
                    }
                }
                if (!flag)
                {
                    return false;
                }
                this.RefreshVolumeAndMass();
                if (sendEvent)
                {
                    this.OnContentsChanged();
                }
                return true;
            }
        }

        public override MyFixedPoint RemoveItemsOfType(MyFixedPoint amount, MyDefinitionId contentId, MyItemFlags flags = 0, bool spawn = false) => 
            TransferOrRemove(this, new MyFixedPoint?(amount), contentId, flags, null, spawn, false);

        public bool RemoveItemsOfType(MyFixedPoint amount, MyObjectBuilder_PhysicalObject objectBuilder, bool spawn = false, bool onlyWhole = true) => 
            (TransferOrRemove(this, new MyFixedPoint?(amount), objectBuilder.GetObjectId(), objectBuilder.Flags, null, spawn, onlyWhole) == amount);

        public void ResetVolume()
        {
            this.m_maxVolume = MyFixedPoint.MaxValue;
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false) => 
            this.GetObjectBuilder();

        public void SetFlags(MyInventoryFlags flags)
        {
            this.m_flags = flags;
        }

        private void SwapItems(int srcIndex, int dstIndex)
        {
            MyPhysicalInventoryItem item = this.m_items[dstIndex];
            this.m_items[dstIndex] = this.m_items[srcIndex];
            this.m_items[srcIndex] = item;
            this.OnContentsChanged();
        }

        public void TakeFloatingObject(MyFloatingObject obj)
        {
            MyFixedPoint amount = obj.Item.Amount;
            if (this.IsConstrained)
            {
                amount = MyFixedPoint.Min(this.ComputeAmountThatFits(obj.Item.Content.GetObjectId(), 0f, 0f), amount);
            }
            if (!obj.MarkedForClose && ((amount > 0) && Sync.IsServer))
            {
                MyFloatingObjects.RemoveFloatingObject(obj, amount);
                uint? itemId = null;
                this.AddItemsInternal(amount, obj.Item.Content, itemId, -1);
            }
        }

        public VRage.Game.Entity.MyEntity TestEntityForPickup(VRage.Game.Entity.MyEntity entity, Vector3D? hitPosition, out MyDefinitionId entityDefId, bool blockManipulatedEntity = true)
        {
            MyCubeBlock block;
            MyCubeGrid grid = MyItemsCollector.TryGetAsComponent(entity, out block, blockManipulatedEntity, hitPosition);
            MyUseObjectsComponentBase component = null;
            entityDefId = new MyDefinitionId(0);
            if (grid != null)
            {
                if (!MyCubeGrid.IsGridInCompleteState(grid))
                {
                    MyHud.Notifications.Add(MyNotificationSingletons.IncompleteGrid);
                    return null;
                }
                entityDefId = new MyDefinitionId(typeof(MyObjectBuilder_CubeGrid));
                return grid;
            }
            if ((MyFakes.ENABLE_GATHERING_SMALL_BLOCK_FROM_GRID && (block != null)) && (block.BlockDefinition.CubeSize == MyCubeSize.Small))
            {
                VRage.Game.Entity.MyEntity baseEntity = block.GetBaseEntity();
                if (((baseEntity == null) || !baseEntity.HasInventory) || baseEntity.GetInventory(0).Empty())
                {
                    entityDefId = block.BlockDefinition.Id;
                    return block;
                }
                MyHud.Notifications.Add(this.m_inventoryNotEmptyNotification);
                return null;
            }
            if (!(entity is MyFloatingObject))
            {
                entity.Components.TryGet<MyUseObjectsComponentBase>(out component);
                return null;
            }
            MyFloatingObject obj2 = entity as MyFloatingObject;
            if (MyFixedPoint.Min(obj2.Item.Amount, this.ComputeAmountThatFits(obj2.Item.Content.GetObjectId(), 0f, 0f)) == 0)
            {
                MyHud.Stats.GetStat<MyStatPlayerInventoryFull>().InventoryFull = true;
                return null;
            }
            entityDefId = obj2.Item.GetDefinitionId();
            return entity;
        }

        public static void Transfer(MyInventory src, MyInventory dst, uint srcItemId, int dstIdx = -1, MyFixedPoint? amount = new MyFixedPoint?(), bool spawn = false)
        {
            MyPhysicalInventoryItem? itemByID = src.GetItemByID(srcItemId);
            if (itemByID != null)
            {
                MyPhysicalInventoryItem item = itemByID.Value;
                if (((dst == null) || dst.CheckConstraint(item.Content.GetObjectId())) && Sync.IsServer)
                {
                    MyFixedPoint? nullable2 = amount;
                    MyFixedPoint point = (nullable2 != null) ? nullable2.GetValueOrDefault() : item.Amount;
                    if (dst == null)
                    {
                        MatrixD? spawnPos = null;
                        src.RemoveItems(srcItemId, amount, true, spawn, spawnPos);
                    }
                    else
                    {
                        TransferItemsInternal(src, dst, srcItemId, spawn, dstIdx, point);
                    }
                }
            }
        }

        public static MyFixedPoint Transfer(MyInventory src, MyInventory dst, MyDefinitionId contentId, MyItemFlags flags = 0, MyFixedPoint? amount = new MyFixedPoint?(), bool spawn = false) => 
            TransferOrRemove(src, amount, contentId, flags, dst, false, true);

        public static void TransferAll(MyInventory src, MyInventory dst)
        {
            if (Sync.IsServer)
            {
                int count = src.m_items.Count + 1;
                while ((src.m_items.Count != count) && (src.m_items.Count != 0))
                {
                    count = src.m_items.Count;
                    MyFixedPoint? amount = null;
                    Transfer(src, dst, src.m_items[0].ItemId, -1, amount, false);
                }
            }
        }

        public static void TransferByUser(MyInventory src, MyInventory dst, uint srcItemId, int dstIdx = -1, MyFixedPoint? amount = new MyFixedPoint?())
        {
            if (src != null)
            {
                MyPhysicalInventoryItem? itemByID = src.GetItemByID(srcItemId);
                if (itemByID != null)
                {
                    MyPhysicalInventoryItem item = itemByID.Value;
                    if ((dst == null) || dst.CheckConstraint(item.Content.GetObjectId()))
                    {
                        MyFixedPoint? nullable2 = amount;
                        MyFixedPoint point = (nullable2 != null) ? nullable2.GetValueOrDefault() : item.Amount;
                        if (dst == null)
                        {
                            MatrixD? spawnPos = null;
                            src.RemoveItems(srcItemId, amount, true, false, spawnPos);
                        }
                        else
                        {
                            byte num = 0;
                            byte index = 0;
                            while (true)
                            {
                                if (index < dst.Owner.InventoryCount)
                                {
                                    if (!dst.Owner.GetInventory(index).Equals(dst))
                                    {
                                        index = (byte) (index + 1);
                                        continue;
                                    }
                                    num = index;
                                }
                                if (OnTransferByUser != null)
                                {
                                    OnTransferByUser(src, dst, item, point);
                                }
                                EndpointId targetEndpoint = new EndpointId();
                                MyMultiplayer.RaiseEvent<MyInventory, MyFixedPoint, uint, long, byte, int>(src, x => new Action<MyFixedPoint, uint, long, byte, int>(x.InventoryTransferItem_Implementation), point, srcItemId, dst.Owner.EntityId, num, dstIdx, targetEndpoint);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public void TransferItemFrom(MyInventory sourceInventory, int sourceItemIndex, int? targetItemIndex = new int?(), bool? stackIfPossible = new bool?(), MyFixedPoint? amount = new MyFixedPoint?())
        {
            if (!ReferenceEquals(this, sourceInventory) && ((sourceItemIndex >= 0) && (sourceItemIndex < sourceInventory.m_items.Count)))
            {
                Transfer(sourceInventory, this, sourceInventory.GetItems()[sourceItemIndex].ItemId, (targetItemIndex != null) ? targetItemIndex.Value : -1, amount, false);
            }
        }

        private bool TransferItemsFrom(VRage.Game.ModAPI.Ingame.IMyInventory sourceInventory, int sourceItemIndex, int? targetItemIndex, bool? stackIfPossible, MyFixedPoint? amount, bool useConveyors)
        {
            if ((amount != null) && (amount.Value <= 0))
            {
                return true;
            }
            MyInventory src = sourceInventory as MyInventory;
            if ((src != null) && src.IsItemAt(sourceItemIndex))
            {
                MyPhysicalInventoryItem item = src.m_items[sourceItemIndex];
                if (!useConveyors || src.CanTransferTo(this, new MyDefinitionId?(item.Content.GetObjectId())))
                {
                    int? nullable = targetItemIndex;
                    Transfer(src, this, item.ItemId, (nullable != null) ? nullable.GetValueOrDefault() : -1, amount, false);
                    return true;
                }
            }
            return false;
        }

        private static void TransferItemsInternal(MyInventory src, MyInventory dst, uint srcItemId, bool spawn, int destItemIndex, MyFixedPoint amount)
        {
            MyFixedPoint remove = amount;
            MyPhysicalInventoryItem item = new MyPhysicalInventoryItem();
            int srcIndex = -1;
            int num2 = 0;
            while (true)
            {
                if (num2 < src.m_items.Count)
                {
                    if (src.m_items[num2].ItemId != srcItemId)
                    {
                        num2++;
                        continue;
                    }
                    srcIndex = num2;
                    item = src.m_items[num2];
                }
                if (srcIndex != -1)
                {
                    FixTransferAmount(src, dst, new MyPhysicalInventoryItem?(item), spawn, ref remove, ref amount);
                    if (amount != 0)
                    {
                        uint? nullable1;
                        if ((ReferenceEquals(src, dst) && ((destItemIndex >= 0) && (destItemIndex < dst.m_items.Count))) && !dst.m_items[destItemIndex].Content.CanStack(item.Content))
                        {
                            dst.SwapItems(srcIndex, destItemIndex);
                            return;
                        }
                        if (ReferenceEquals(dst, src) && (remove == 0))
                        {
                            nullable1 = new uint?(srcItemId);
                        }
                        else
                        {
                            nullable1 = null;
                        }
                        dst.AddItemsInternal(amount, item.Content, nullable1, destItemIndex);
                        if (remove != 0)
                        {
                            MatrixD? spawnPos = null;
                            src.RemoveItems(srcItemId, new MyFixedPoint?(remove), true, false, spawnPos);
                        }
                    }
                }
                return;
            }
        }

        private bool TransferItemsTo(VRage.Game.ModAPI.Ingame.IMyInventory dst, int sourceItemIndex, int? targetItemIndex, bool? stackIfPossible, MyFixedPoint? amount, bool useConveyor)
        {
            MyInventory inventory = dst as MyInventory;
            return ((inventory != null) && inventory.TransferItemsFrom(this, sourceItemIndex, targetItemIndex, stackIfPossible, amount, useConveyor));
        }

        private static MyFixedPoint TransferOrRemove(MyInventory src, MyFixedPoint? amount, MyDefinitionId contentId, MyItemFlags flags = 0, MyInventory dst = null, bool spawn = false, bool onlyWhole = true)
        {
            MyFixedPoint point = 0;
            if (!onlyWhole)
            {
                amount = new MyFixedPoint?(MyFixedPoint.Min(amount.Value, src.GetItemAmount(contentId, flags, false)));
            }
            if (!onlyWhole || src.ContainItems(amount, contentId, flags))
            {
                MyFixedPoint? nullable;
                bool flag = amount == null;
                MyFixedPoint point2 = flag ? 0 : amount.Value;
                if ((contentId.TypeId == typeof(MyObjectBuilder_OxygenContainerObject)) || (contentId.TypeId == typeof(MyObjectBuilder_GasContainerObject)))
                {
                    int num2 = 0;
                    while ((num2 < src.m_items.Count) && (flag || (point2 != 0)))
                    {
                        MyPhysicalInventoryItem item = src.m_items[num2];
                        MyObjectBuilder_GasContainerObject content = item.Content as MyObjectBuilder_GasContainerObject;
                        if ((content != null) && (content.GasLevel == 1f))
                        {
                            num2++;
                        }
                        else if (item.Content.GetObjectId() != contentId)
                        {
                            num2++;
                        }
                        else if (!flag && (point2 < item.Amount))
                        {
                            point += item.Amount;
                            Transfer(src, dst, item.ItemId, -1, new MyFixedPoint?(point2), spawn);
                            point2 = 0;
                        }
                        else
                        {
                            point += item.Amount;
                            point2 -= item.Amount;
                            nullable = null;
                            Transfer(src, dst, item.ItemId, -1, nullable, spawn);
                        }
                    }
                }
                int num = 0;
                while ((num < src.m_items.Count) && (flag || (point2 != 0)))
                {
                    MyPhysicalInventoryItem item2 = src.m_items[num];
                    MyDefinitionId objectId = item2.Content.GetId();
                    if ((objectId != contentId) && (item2.Content.TypeId == typeof(MyObjectBuilder_BlockItem)))
                    {
                        objectId = item2.Content.GetObjectId();
                    }
                    if (objectId != contentId)
                    {
                        num++;
                    }
                    else if (!flag && (point2 < item2.Amount))
                    {
                        point += point2;
                        Transfer(src, dst, item2.ItemId, -1, new MyFixedPoint?(point2), spawn);
                        point2 = 0;
                    }
                    else
                    {
                        point += item2.Amount;
                        point2 -= item2.Amount;
                        nullable = null;
                        Transfer(src, dst, item2.ItemId, -1, nullable, spawn);
                    }
                }
            }
            return point;
        }

        private bool TryFindNextPositionOfTtype(MyDefinitionId contentId, int startPosition, out int nextPosition)
        {
            if (this.m_items.IsValidIndex<MyPhysicalInventoryItem>(startPosition + 1))
            {
                for (int i = startPosition + 1; i < this.m_items.Count; i++)
                {
                    if (this.m_items[i].Content.GetObjectId() == contentId)
                    {
                        nextPosition = i;
                        return true;
                    }
                }
            }
            nextPosition = -1;
            return false;
        }

        public void UpdateGasAmount()
        {
            this.RefreshVolumeAndMass();
            this.OnContentsChanged();
        }

        public void UpdateItem(MyDefinitionId contentId, uint? itemId = new uint?(), float? amount = new float?(), float? itemHP = new float?())
        {
            if ((amount != null) || (itemHP != null))
            {
                int? nullable = null;
                if (itemId == null)
                {
                    nullable = this.FindFirstPositionOfType(contentId, MyItemFlags.None);
                }
                else
                {
                    int itemIndexById = this.GetItemIndexById(itemId.Value);
                    if (this.m_items.IsValidIndex<MyPhysicalInventoryItem>(itemIndexById))
                    {
                        nullable = new int?(itemIndexById);
                    }
                }
                bool flag = false;
                if ((nullable != null) && this.m_items.IsValidIndex<MyPhysicalInventoryItem>(nullable.Value))
                {
                    MyPhysicalInventoryItem item = this.m_items[nullable.Value];
                    if ((amount != null) && (amount.Value != ((float) item.Amount)))
                    {
                        item.Amount = (MyFixedPoint) amount.Value;
                        flag = true;
                    }
                    if (((itemHP != null) && (item.Content != null)) && ((item.Content.DurabilityHP == null) || (item.Content.DurabilityHP.Value != itemHP.Value)))
                    {
                        item.Content.DurabilityHP = new float?(itemHP.Value);
                        flag = true;
                    }
                    if (flag)
                    {
                        this.m_items[nullable.Value] = item;
                        this.OnContentsChanged();
                    }
                }
            }
        }

        public unsafe void UpdateItemAmoutClient(uint itemId, MyFixedPoint amount)
        {
            if (!Sync.IsServer)
            {
                MyPhysicalInventoryItem? nullable = null;
                int num = -1;
                int num2 = 0;
                while (true)
                {
                    if (num2 < this.m_items.Count)
                    {
                        if (this.m_items[num2].ItemId != itemId)
                        {
                            num2++;
                            continue;
                        }
                        nullable = new MyPhysicalInventoryItem?(this.m_items[num2]);
                        num = num2;
                    }
                    if (num != -1)
                    {
                        MyPhysicalInventoryItem item = nullable.Value;
                        MyObjectBuilder_GasContainerObject content = item.Content as MyObjectBuilder_GasContainerObject;
                        if (content != null)
                        {
                            content.GasLevel += (float) amount;
                        }
                        else
                        {
                            MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref item.Amount;
                            pointPtr1[0] += amount;
                        }
                        this.m_items[num] = item;
                        base.RaiseContentsAdded(item, amount);
                        this.NotifyHudChangedInventoryItem(amount, ref item, amount > 0);
                    }
                    return;
                }
            }
        }

        [Conditional("DEBUG")]
        private void VerifyIntegrity()
        {
            HashSet<uint> set = new HashSet<uint>();
            foreach (MyPhysicalInventoryItem item in this.m_items)
            {
                set.Add(item.ItemId);
                item.Content.CanStack(item.Content);
            }
        }

        void VRage.Game.ModAPI.IMyInventory.AddItems(MyFixedPoint amount, MyObjectBuilder_PhysicalObject objectBuilder, int index)
        {
            uint? itemId = null;
            this.AddItems(amount, objectBuilder, itemId, index);
        }

        bool VRage.Game.ModAPI.IMyInventory.CanAddItemAmount(VRage.Game.ModAPI.IMyInventoryItem item, MyFixedPoint amount) => 
            this.ItemsCanBeAdded(amount, item);

        VRage.Game.ModAPI.IMyInventoryItem VRage.Game.ModAPI.IMyInventory.FindItem(SerializableDefinitionId contentId)
        {
            MyPhysicalInventoryItem? nullable = this.FindItem(contentId);
            return ((nullable == null) ? null : ((VRage.Game.ModAPI.IMyInventoryItem) nullable.Value));
        }

        VRage.Game.ModAPI.IMyInventoryItem VRage.Game.ModAPI.IMyInventory.GetItemByID(uint id)
        {
            MyPhysicalInventoryItem? itemByID = this.GetItemByID(id);
            return ((itemByID == null) ? null : ((VRage.Game.ModAPI.IMyInventoryItem) itemByID.Value));
        }

        List<VRage.Game.ModAPI.IMyInventoryItem> VRage.Game.ModAPI.IMyInventory.GetItems() => 
            this.m_items.OfType<VRage.Game.ModAPI.IMyInventoryItem>().ToList<VRage.Game.ModAPI.IMyInventoryItem>();

        void VRage.Game.ModAPI.IMyInventory.RemoveItemAmount(VRage.Game.ModAPI.IMyInventoryItem item, MyFixedPoint amount)
        {
            this.Remove(item, amount);
        }

        void VRage.Game.ModAPI.IMyInventory.RemoveItems(uint itemId, MyFixedPoint? amount, bool sendEvent, bool spawn)
        {
            MatrixD? spawnPos = null;
            this.RemoveItems(itemId, amount, sendEvent, spawn, spawnPos);
        }

        void VRage.Game.ModAPI.IMyInventory.RemoveItemsAt(int itemIndex, MyFixedPoint? amount, bool sendEvent, bool spawn)
        {
            MatrixD? spawnPos = null;
            this.RemoveItemsAt(itemIndex, amount, sendEvent, spawn, spawnPos);
        }

        void VRage.Game.ModAPI.IMyInventory.RemoveItemsOfType(MyFixedPoint amount, MyObjectBuilder_PhysicalObject objectBuilder, bool spawn)
        {
            this.RemoveItemsOfType(amount, objectBuilder, spawn, true);
        }

        void VRage.Game.ModAPI.IMyInventory.RemoveItemsOfType(MyFixedPoint amount, SerializableDefinitionId contentId, MyItemFlags flags, bool spawn)
        {
            this.RemoveItemsOfType(amount, contentId, flags, spawn);
        }

        bool VRage.Game.ModAPI.IMyInventory.TransferItemFrom(VRage.Game.ModAPI.IMyInventory sourceInventory, VRage.Game.ModAPI.IMyInventoryItem item, MyFixedPoint amount)
        {
            if ((sourceInventory == null) || (item == null))
            {
                return false;
            }
            int itemIndexById = this.GetItemIndexById(item.ItemId);
            if (itemIndexById < 0)
            {
                return false;
            }
            int? targetItemIndex = null;
            bool? stackIfPossible = null;
            return this.TransferItemsFrom(sourceInventory, itemIndexById, targetItemIndex, stackIfPossible, new MyFixedPoint?(amount), true);
        }

        bool VRage.Game.ModAPI.IMyInventory.TransferItemFrom(VRage.Game.ModAPI.IMyInventory sourceInventory, int sourceItemIndex, int? targetItemIndex, bool? stackIfPossible, MyFixedPoint? amount, bool checkConnection) => 
            this.TransferItemsFrom(sourceInventory, sourceItemIndex, targetItemIndex, stackIfPossible, amount, checkConnection);

        bool VRage.Game.ModAPI.IMyInventory.TransferItemTo(VRage.Game.ModAPI.IMyInventory dst, int sourceItemIndex, int? targetItemIndex, bool? stackIfPossible, MyFixedPoint? amount, bool checkConnection) => 
            this.TransferItemsTo(dst, sourceItemIndex, targetItemIndex, stackIfPossible, amount, checkConnection);

        bool VRage.Game.ModAPI.Ingame.IMyInventory.CanItemsBeAdded(MyFixedPoint amount, MyItemType itemType) => 
            this.CanItemsBeAdded(amount, (MyDefinitionId) itemType);

        bool VRage.Game.ModAPI.Ingame.IMyInventory.CanTransferItemTo(VRage.Game.ModAPI.Ingame.IMyInventory dst, MyItemType itemType)
        {
            MyInventory dstInventory = dst as MyInventory;
            return ((dstInventory != null) ? this.CanTransferTo(dstInventory, new MyDefinitionId?((MyDefinitionId) itemType)) : false);
        }

        bool VRage.Game.ModAPI.Ingame.IMyInventory.ContainItems(MyFixedPoint amount, MyItemType itemType) => 
            this.ContainItems(new MyFixedPoint?(amount), (MyDefinitionId) itemType, MyItemFlags.None);

        MyInventoryItem? VRage.Game.ModAPI.Ingame.IMyInventory.FindItem(MyItemType itemType) => 
            this.FindItem((MyDefinitionId) itemType).MakeAPIItem();

        void VRage.Game.ModAPI.Ingame.IMyInventory.GetAcceptedItems(List<MyItemType> items, Func<MyItemType, bool> filter = null)
        {
            using (List<MyPhysicalItemDefinition>.Enumerator enumerator = MyDefinitionManager.Static.GetWeaponDefinitions().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyDefinitionId checkedId = enumerator.Current.Id;
                    if (this.Constraint.Check(checkedId))
                    {
                        MyItemType arg = checkedId;
                        if ((filter == null) || filter(arg))
                        {
                            items.Add(arg);
                        }
                    }
                }
            }
        }

        MyFixedPoint VRage.Game.ModAPI.Ingame.IMyInventory.GetItemAmount(MyItemType contentId) => 
            this.GetItemAmount((MyDefinitionId) contentId, MyItemFlags.None, false);

        MyInventoryItem? VRage.Game.ModAPI.Ingame.IMyInventory.GetItemByID(uint id) => 
            this.GetItemByID(id).MakeAPIItem();

        void VRage.Game.ModAPI.Ingame.IMyInventory.GetItems(List<MyInventoryItem> items, Func<MyInventoryItem, bool> filter = null)
        {
            using (List<MyPhysicalInventoryItem>.Enumerator enumerator = this.m_items.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyInventoryItem arg = enumerator.Current.MakeAPIItem();
                    if ((filter == null) || filter(arg))
                    {
                        items.Add(arg);
                    }
                }
            }
        }

        bool VRage.Game.ModAPI.Ingame.IMyInventory.IsConnectedTo(VRage.Game.ModAPI.Ingame.IMyInventory dst)
        {
            MyInventory dstInventory = dst as MyInventory;
            if (dstInventory == null)
            {
                return false;
            }
            MyDefinitionId? itemType = null;
            return this.CanTransferTo(dstInventory, itemType);
        }

        bool VRage.Game.ModAPI.Ingame.IMyInventory.TransferItemFrom(VRage.Game.ModAPI.Ingame.IMyInventory sourceInventory, MyInventoryItem item, MyFixedPoint? amount = new MyFixedPoint?()) => 
            ((sourceInventory is MyInventory) ? sourceInventory.TransferItemTo(this, item, amount) : false);

        bool VRage.Game.ModAPI.Ingame.IMyInventory.TransferItemFrom(VRage.Game.ModAPI.Ingame.IMyInventory sourceInventory, int sourceItemIndex, int? targetItemIndex, bool? stackIfPossible, MyFixedPoint? amount) => 
            this.TransferItemsFrom(sourceInventory, sourceItemIndex, targetItemIndex, stackIfPossible, amount, true);

        bool VRage.Game.ModAPI.Ingame.IMyInventory.TransferItemTo(VRage.Game.ModAPI.Ingame.IMyInventory dstInventory, MyInventoryItem item, MyFixedPoint? amount = new MyFixedPoint?())
        {
            int itemIndexById = this.GetItemIndexById(item.ItemId);
            if (itemIndexById < 0)
            {
                return false;
            }
            int? targetItemIndex = null;
            bool? stackIfPossible = null;
            return this.TransferItemsTo(dstInventory, itemIndexById, targetItemIndex, stackIfPossible, amount, true);
        }

        bool VRage.Game.ModAPI.Ingame.IMyInventory.TransferItemTo(VRage.Game.ModAPI.Ingame.IMyInventory dst, int sourceItemIndex, int? targetItemIndex, bool? stackIfPossible, MyFixedPoint? amount) => 
            this.TransferItemsTo(dst, sourceItemIndex, targetItemIndex, stackIfPossible, amount, true);

        public override float? ForcedPriority { get; set; }

        public bool IsConstrained =>
            (MyPerGameSettings.ConstrainInventory() || !this.IsCharacterOwner);

        public override MyFixedPoint MaxMass =>
            (!this.IsConstrained ? MyFixedPoint.MaxValue : (!this.m_multiplierEnabled ? this.m_maxMass : (!this.IsCharacterOwner ? MyFixedPoint.MultiplySafe(this.m_maxMass, MySession.Static.BlocksInventorySizeMultiplier) : MyFixedPoint.MultiplySafe(this.m_maxMass, MySession.Static.CharactersInventoryMultiplier))));

        public override MyFixedPoint MaxVolume =>
            (!this.IsConstrained ? MyFixedPoint.MaxValue : (!this.m_multiplierEnabled ? this.m_maxVolume : (!this.IsCharacterOwner ? MyFixedPoint.MultiplySafe(this.m_maxVolume, MySession.Static.BlocksInventorySizeMultiplier) : MyFixedPoint.MultiplySafe(this.m_maxVolume, MySession.Static.CharactersInventoryMultiplier))));

        public override int MaxItemCount
        {
            get
            {
                if (!this.IsConstrained)
                {
                    return 0x7fffffff;
                }
                if (!this.m_multiplierEnabled)
                {
                    return this.m_maxItemCount;
                }
                double num = this.IsCharacterOwner ? ((double) MySession.Static.CharactersInventoryMultiplier) : ((double) MySession.Static.BlocksInventorySizeMultiplier);
                long num2 = Math.Max(1L, (long) (this.m_maxItemCount * num));
                if (num2 > 0x7fffffffL)
                {
                    num2 = 0x7fffffffL;
                }
                return (int) num2;
            }
        }

        public override MyFixedPoint CurrentVolume =>
            ((MyFixedPoint) this.m_currentVolume);

        public float VolumeFillFactor =>
            (this.IsConstrained ? (((float) this.CurrentVolume) / ((float) this.MaxVolume)) : 0f);

        public override MyFixedPoint CurrentMass =>
            ((MyFixedPoint) this.m_currentMass);

        public VRage.Game.Entity.MyEntity Owner =>
            ((base.Entity != null) ? (base.Entity as VRage.Game.Entity.MyEntity) : null);

        public bool IsCharacterOwner =>
            (this.Owner is MyCharacter);

        public byte InventoryIdx
        {
            get
            {
                if (this.Owner != null)
                {
                    for (byte i = 0; i < this.Owner.InventoryCount; i = (byte) (i + 1))
                    {
                        if (this.Owner.GetInventory(i).Equals(this))
                        {
                            return i;
                        }
                    }
                }
                return 0;
            }
        }

        public bool IsFull =>
            ((this.m_currentVolume >= this.MaxVolume) || (this.m_currentMass >= this.MaxMass));

        public float CargoPercentage
        {
            get
            {
                if (!this.IsConstrained)
                {
                    return 0f;
                }
                return MyMath.Clamp(((float) this.m_currentVolume.Value) / ((float) this.MaxVolume), 0f, 1f);
            }
        }

        VRage.Game.ModAPI.Ingame.IMyEntity VRage.Game.ModAPI.Ingame.IMyInventory.Owner =>
            this.Owner;

        public int ItemCount =>
            this.m_items.Count;

        VRage.ModAPI.IMyEntity VRage.Game.ModAPI.IMyInventory.Owner =>
            this.Owner;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyInventory.<>c <>9 = new MyInventory.<>c();
            public static Func<MyInventory, Action<long, MyFixedPoint>> <>9__78_0;
            public static Func<MyInventory, Action<MyFixedPoint, MyObjectBuilder_Base>> <>9__80_0;
            public static Func<MyInventory, Action<MyFixedPoint, uint>> <>9__91_0;
            public static Func<MyInventory, Action<MyFixedPoint, uint>> <>9__92_0;
            public static Func<MyInventory, Action<int, MyFixedPoint?, bool, bool, MatrixD?>> <>9__93_0;
            public static Func<MyInventory, Action<MyFixedPoint, uint, long, byte, int>> <>9__106_0;
            public static Func<MyInventory, Action<long, bool>> <>9__130_0;
            public static Func<MyInventory, Action<MyFixedPoint, SerializableDefinitionId, long>> <>9__151_0;

            internal Action<long, bool> <AddEntity>b__130_0(MyInventory x) => 
                new Action<long, bool>(x.AddEntity_Implementation);

            internal Action<MyFixedPoint, SerializableDefinitionId, long> <ConsumeItem>b__151_0(MyInventory x) => 
                new Action<MyFixedPoint, SerializableDefinitionId, long>(x.InventoryConsumeItem_Implementation);

            internal Action<MyFixedPoint, MyObjectBuilder_Base> <DebugAddItems>b__80_0(MyInventory x) => 
                new Action<MyFixedPoint, MyObjectBuilder_Base>(x.DebugAddItems_Implementation);

            internal Action<MyFixedPoint, uint> <DropItem>b__92_0(MyInventory x) => 
                new Action<MyFixedPoint, uint>(x.DropItem_Implementation);

            internal Action<MyFixedPoint, uint> <DropItemById>b__91_0(MyInventory x) => 
                new Action<MyFixedPoint, uint>(x.DropItem_Implementation);

            internal Action<long, MyFixedPoint> <PickupItem>b__78_0(MyInventory x) => 
                new Action<long, MyFixedPoint>(x.PickupItem_Implementation);

            internal Action<int, MyFixedPoint?, bool, bool, MatrixD?> <RemoveItemsAt>b__93_0(MyInventory x) => 
                new Action<int, MyFixedPoint?, bool, bool, MatrixD?>(x.RemoveItemsAt_Request);

            internal Action<MyFixedPoint, uint, long, byte, int> <TransferByUser>b__106_0(MyInventory x) => 
                new Action<MyFixedPoint, uint, long, byte, int>(x.InventoryTransferItem_Implementation);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ConnectionData
        {
            public int Frame;
            public bool HasConnection;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ConnectionKey : IEquatable<MyInventory.ConnectionKey>
        {
            public long Id;
            public MyDefinitionId? ItemType;
            public ConnectionKey(long id, MyDefinitionId? itemType)
            {
                this.Id = id;
                this.ItemType = itemType;
            }

            public bool Equals(MyInventory.ConnectionKey other) => 
                ((this.Id == other.Id) ? (((this.ItemType != null) == (other.ItemType != null)) ? ((this.ItemType == null) || (this.ItemType.Value == other.ItemType.Value)) : false) : false);

            public override bool Equals(object obj) => 
                (!(obj is MyInventory.ConnectionKey) ? this.Equals(obj) : this.Equals((MyInventory.ConnectionKey) obj));

            public override int GetHashCode() => 
                MyTuple.CombineHashCodes(this.Id.GetHashCode(), this.ItemType.GetHashCode());
        }
    }
}

