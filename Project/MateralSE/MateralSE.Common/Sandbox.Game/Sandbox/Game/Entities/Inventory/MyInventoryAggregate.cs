namespace Sandbox.Game.Entities.Inventory
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyComponentBuilder(typeof(MyObjectBuilder_InventoryAggregate), true), StaticEventOwner]
    public class MyInventoryAggregate : MyInventoryBase, IMyComponentAggregate, IMyEventProxy, IMyEventOwner
    {
        private MyAggregateComponentList m_children;
        [CompilerGenerated]
        private Action<MyInventoryAggregate, MyInventoryBase> OnAfterComponentAdd;
        [CompilerGenerated]
        private Action<MyInventoryAggregate, MyInventoryBase> OnBeforeComponentRemove;
        private List<MyComponentBase> tmp_list;
        private List<MyPhysicalInventoryItem> m_allItems;
        private float? m_forcedPriority;
        private int m_inventoryCount;
        [CompilerGenerated]
        private Action<MyInventoryAggregate, int> OnInventoryCountChanged;

        public event Action<MyInventoryAggregate, MyInventoryBase> OnAfterComponentAdd
        {
            [CompilerGenerated] add
            {
                Action<MyInventoryAggregate, MyInventoryBase> onAfterComponentAdd = this.OnAfterComponentAdd;
                while (true)
                {
                    Action<MyInventoryAggregate, MyInventoryBase> a = onAfterComponentAdd;
                    Action<MyInventoryAggregate, MyInventoryBase> action3 = (Action<MyInventoryAggregate, MyInventoryBase>) Delegate.Combine(a, value);
                    onAfterComponentAdd = Interlocked.CompareExchange<Action<MyInventoryAggregate, MyInventoryBase>>(ref this.OnAfterComponentAdd, action3, a);
                    if (ReferenceEquals(onAfterComponentAdd, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyInventoryAggregate, MyInventoryBase> onAfterComponentAdd = this.OnAfterComponentAdd;
                while (true)
                {
                    Action<MyInventoryAggregate, MyInventoryBase> source = onAfterComponentAdd;
                    Action<MyInventoryAggregate, MyInventoryBase> action3 = (Action<MyInventoryAggregate, MyInventoryBase>) Delegate.Remove(source, value);
                    onAfterComponentAdd = Interlocked.CompareExchange<Action<MyInventoryAggregate, MyInventoryBase>>(ref this.OnAfterComponentAdd, action3, source);
                    if (ReferenceEquals(onAfterComponentAdd, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyInventoryAggregate, MyInventoryBase> OnBeforeComponentRemove
        {
            [CompilerGenerated] add
            {
                Action<MyInventoryAggregate, MyInventoryBase> onBeforeComponentRemove = this.OnBeforeComponentRemove;
                while (true)
                {
                    Action<MyInventoryAggregate, MyInventoryBase> a = onBeforeComponentRemove;
                    Action<MyInventoryAggregate, MyInventoryBase> action3 = (Action<MyInventoryAggregate, MyInventoryBase>) Delegate.Combine(a, value);
                    onBeforeComponentRemove = Interlocked.CompareExchange<Action<MyInventoryAggregate, MyInventoryBase>>(ref this.OnBeforeComponentRemove, action3, a);
                    if (ReferenceEquals(onBeforeComponentRemove, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyInventoryAggregate, MyInventoryBase> onBeforeComponentRemove = this.OnBeforeComponentRemove;
                while (true)
                {
                    Action<MyInventoryAggregate, MyInventoryBase> source = onBeforeComponentRemove;
                    Action<MyInventoryAggregate, MyInventoryBase> action3 = (Action<MyInventoryAggregate, MyInventoryBase>) Delegate.Remove(source, value);
                    onBeforeComponentRemove = Interlocked.CompareExchange<Action<MyInventoryAggregate, MyInventoryBase>>(ref this.OnBeforeComponentRemove, action3, source);
                    if (ReferenceEquals(onBeforeComponentRemove, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyInventoryAggregate, int> OnInventoryCountChanged
        {
            [CompilerGenerated] add
            {
                Action<MyInventoryAggregate, int> onInventoryCountChanged = this.OnInventoryCountChanged;
                while (true)
                {
                    Action<MyInventoryAggregate, int> a = onInventoryCountChanged;
                    Action<MyInventoryAggregate, int> action3 = (Action<MyInventoryAggregate, int>) Delegate.Combine(a, value);
                    onInventoryCountChanged = Interlocked.CompareExchange<Action<MyInventoryAggregate, int>>(ref this.OnInventoryCountChanged, action3, a);
                    if (ReferenceEquals(onInventoryCountChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyInventoryAggregate, int> onInventoryCountChanged = this.OnInventoryCountChanged;
                while (true)
                {
                    Action<MyInventoryAggregate, int> source = onInventoryCountChanged;
                    Action<MyInventoryAggregate, int> action3 = (Action<MyInventoryAggregate, int>) Delegate.Remove(source, value);
                    onInventoryCountChanged = Interlocked.CompareExchange<Action<MyInventoryAggregate, int>>(ref this.OnInventoryCountChanged, action3, source);
                    if (ReferenceEquals(onInventoryCountChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyInventoryAggregate() : base("Inventory")
        {
            this.m_children = new MyAggregateComponentList();
            this.tmp_list = new List<MyComponentBase>();
            this.m_allItems = new List<MyPhysicalInventoryItem>();
        }

        public MyInventoryAggregate(string inventoryId) : base(inventoryId)
        {
            this.m_children = new MyAggregateComponentList();
            this.tmp_list = new List<MyComponentBase>();
            this.m_allItems = new List<MyPhysicalInventoryItem>();
        }

        public override bool Add(IMyInventoryItem item, MyFixedPoint amount)
        {
            using (List<MyComponentBase>.Enumerator enumerator = this.m_children.Reader.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyInventoryBase current = (MyInventoryBase) enumerator.Current;
                    if (current.ItemsCanBeAdded(amount, item) && current.Add(item, amount))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool AddItems(MyFixedPoint amount, MyObjectBuilder_Base objectBuilder)
        {
            MyFixedPoint point = this.ComputeAmountThatFits(objectBuilder.GetId(), 0f, 0f);
            MyFixedPoint point2 = amount;
            if (amount <= point)
            {
                foreach (MyInventoryBase base2 in this.m_children.Reader)
                {
                    MyFixedPoint point3 = base2.ComputeAmountThatFits(objectBuilder.GetId(), 0f, 0f);
                    if (point3 > point2)
                    {
                        point3 = point2;
                    }
                    if ((point3 > 0) && base2.AddItems(point3, objectBuilder))
                    {
                        point2 -= point3;
                    }
                    if (point2 == 0)
                    {
                        break;
                    }
                }
            }
            return (point2 == 0);
        }

        public void AfterComponentAdd(MyComponentBase component)
        {
            MyInventoryBase base2 = component as MyInventoryBase;
            base2.ForcedPriority = this.ForcedPriority;
            base2.ContentsChanged += new Action<MyInventoryBase>(this.child_OnContentsChanged);
            if (component is MyInventory)
            {
                int inventoryCount = this.InventoryCount;
                this.InventoryCount = inventoryCount + 1;
            }
            else if (component is MyInventoryAggregate)
            {
                (component as MyInventoryAggregate).OnInventoryCountChanged += new Action<MyInventoryAggregate, int>(this.OnChildAggregateCountChanged);
                this.InventoryCount += (component as MyInventoryAggregate).InventoryCount;
            }
            if (this.OnAfterComponentAdd != null)
            {
                this.OnAfterComponentAdd(this, base2);
            }
        }

        public override void ApplyChanges(List<MyComponentChange> changes)
        {
            using (List<MyComponentBase>.Enumerator enumerator = this.m_children.Reader.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ((MyInventoryBase) enumerator.Current).ApplyChanges(changes);
                }
            }
        }

        public void BeforeComponentRemove(MyComponentBase component)
        {
            MyInventoryBase base2 = component as MyInventoryBase;
            base2.ForcedPriority = null;
            base2.ContentsChanged -= new Action<MyInventoryBase>(this.child_OnContentsChanged);
            if (this.OnBeforeComponentRemove != null)
            {
                this.OnBeforeComponentRemove(this, base2);
            }
            if (component is MyInventory)
            {
                int inventoryCount = this.InventoryCount;
                this.InventoryCount = inventoryCount - 1;
            }
            else if (component is MyInventoryAggregate)
            {
                (component as MyInventoryAggregate).OnInventoryCountChanged -= new Action<MyInventoryAggregate, int>(this.OnChildAggregateCountChanged);
                this.InventoryCount -= (component as MyInventoryAggregate).InventoryCount;
            }
        }

        private void child_OnContentsChanged(MyComponentBase obj)
        {
            this.OnContentsChanged();
        }

        public override MyFixedPoint ComputeAmountThatFits(MyDefinitionId contentId, float volumeRemoved = 0f, float massRemoved = 0f)
        {
            float num = 0f;
            foreach (MyInventoryBase base2 in this.m_children.Reader)
            {
                num += (float) base2.ComputeAmountThatFits(contentId, volumeRemoved, massRemoved);
            }
            return (MyFixedPoint) num;
        }

        public override void ConsumeItem(MyDefinitionId itemId, MyFixedPoint amount, long consumerEntityId = 0L)
        {
            SerializableDefinitionId id = (SerializableDefinitionId) itemId;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyInventoryAggregate, MyFixedPoint, SerializableDefinitionId, long>(this, x => new Action<MyFixedPoint, SerializableDefinitionId, long>(x.InventoryConsumeItem_Implementation), amount, id, consumerEntityId, targetEndpoint);
        }

        public override void CountItems(Dictionary<MyDefinitionId, MyFixedPoint> itemCounts)
        {
            using (List<MyComponentBase>.Enumerator enumerator = this.m_children.Reader.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ((MyInventoryBase) enumerator.Current).CountItems(itemCounts);
                }
            }
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            MyObjectBuilder_InventoryAggregate aggregate = builder as MyObjectBuilder_InventoryAggregate;
            if ((aggregate != null) && (aggregate.Inventories != null))
            {
                foreach (MyObjectBuilder_InventoryBase base2 in aggregate.Inventories)
                {
                    MyComponentBase component = MyComponentFactory.CreateInstanceByTypeId(base2.TypeId);
                    component.Deserialize(base2);
                    this.AddComponent(component);
                }
            }
        }

        public void DetachCallbacks()
        {
            using (List<MyComponentBase>.Enumerator enumerator = this.m_children.Reader.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ((MyInventoryBase) enumerator.Current).ContentsChanged -= new Action<MyInventoryBase>(this.child_OnContentsChanged);
                }
            }
        }

        ~MyInventoryAggregate()
        {
            this.DetachCallbacks();
        }

        public static MyInventoryAggregate FixInputOutputInventories(MyInventoryAggregate inventoryAggregate, MyInventoryConstraint inputInventoryConstraint, MyInventoryConstraint outputInventoryConstraint)
        {
            MyInventory objA = null;
            MyInventory component = null;
            foreach (MyInventory inventory3 in inventoryAggregate.ChildList.Reader)
            {
                if (inventory3 == null)
                {
                    continue;
                }
                if (inventory3.GetItemsCount() > 0)
                {
                    if (objA == null)
                    {
                        bool flag = true;
                        if (inputInventoryConstraint != null)
                        {
                            foreach (MyPhysicalInventoryItem item in inventory3.GetItems())
                            {
                                flag &= inputInventoryConstraint.Check(item.GetDefinitionId());
                            }
                        }
                        if (flag)
                        {
                            objA = inventory3;
                        }
                    }
                    if ((component == null) && !ReferenceEquals(objA, inventory3))
                    {
                        bool flag2 = true;
                        if (outputInventoryConstraint != null)
                        {
                            foreach (MyPhysicalInventoryItem item2 in inventory3.GetItems())
                            {
                                flag2 &= outputInventoryConstraint.Check(item2.GetDefinitionId());
                            }
                        }
                        if (flag2)
                        {
                            component = inventory3;
                        }
                    }
                }
            }
            if ((objA == null) || (component == null))
            {
                foreach (MyInventory inventory4 in inventoryAggregate.ChildList.Reader)
                {
                    if (inventory4 != null)
                    {
                        if (objA == null)
                        {
                            objA = inventory4;
                            continue;
                        }
                        if (component != null)
                        {
                            break;
                        }
                        component = inventory4;
                    }
                }
            }
            inventoryAggregate.RemoveComponent(objA);
            inventoryAggregate.RemoveComponent(component);
            MyInventoryAggregate aggregate = new MyInventoryAggregate();
            aggregate.AddComponent(objA);
            aggregate.AddComponent(component);
            return aggregate;
        }

        public MyInventoryBase GetInventory(MyStringHash id)
        {
            using (List<MyComponentBase>.Enumerator enumerator = this.m_children.Reader.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyInventoryBase current = enumerator.Current as MyInventoryBase;
                    if (current.InventoryId == id)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public override int GetInventoryCount() => 
            this.InventoryCount;

        public override MyFixedPoint GetItemAmount(MyDefinitionId contentId, MyItemFlags flags = 0, bool substitute = false)
        {
            float num = 0f;
            foreach (MyInventoryBase base2 in this.m_children.Reader)
            {
                num += (float) base2.GetItemAmount(contentId, flags, substitute);
            }
            return (MyFixedPoint) num;
        }

        public override List<MyPhysicalInventoryItem> GetItems()
        {
            this.m_allItems.Clear();
            foreach (MyInventoryBase base2 in this.m_children.Reader)
            {
                this.m_allItems.AddRange(base2.GetItems());
            }
            return this.m_allItems;
        }

        public void Init()
        {
            using (List<MyComponentBase>.Enumerator enumerator = this.m_children.Reader.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ((MyInventoryBase) enumerator.Current).ContentsChanged += new Action<MyInventoryBase>(this.child_OnContentsChanged);
                }
            }
        }

        [Event(null, 0x1dd), Reliable, Server]
        private void InventoryConsumeItem_Implementation(MyFixedPoint amount, SerializableDefinitionId itemId, long consumerEntityId)
        {
            if ((consumerEntityId == 0) || MyEntities.EntityExists(consumerEntityId))
            {
                MyFixedPoint point = this.GetItemAmount(itemId, MyItemFlags.None, false);
                if (point < amount)
                {
                    amount = point;
                }
                MyEntity entityById = null;
                if (consumerEntityId != 0)
                {
                    entityById = MyEntities.GetEntityById(consumerEntityId, false);
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

        public override bool ItemsCanBeAdded(MyFixedPoint amount, IMyInventoryItem item)
        {
            using (List<MyComponentBase>.Enumerator enumerator = this.m_children.Reader.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (((MyInventoryBase) enumerator.Current).ItemsCanBeAdded(amount, item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool ItemsCanBeRemoved(MyFixedPoint amount, IMyInventoryItem item)
        {
            using (List<MyComponentBase>.Enumerator enumerator = this.m_children.Reader.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (((MyInventoryBase) enumerator.Current).ItemsCanBeRemoved(amount, item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override MyInventoryBase IterateInventory(int searchIndex, int currentIndex)
        {
            using (List<MyComponentBase>.Enumerator enumerator = this.ChildList.Reader.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyInventoryBase current = enumerator.Current as MyInventoryBase;
                    if (current != null)
                    {
                        MyInventoryBase base3 = current.IterateInventory(searchIndex, currentIndex);
                        if (base3 == null)
                        {
                            if (!(current is MyInventory))
                            {
                                continue;
                            }
                            currentIndex++;
                            continue;
                        }
                        return base3;
                    }
                }
            }
            return null;
        }

        public override void OnBeforeContentsChanged()
        {
            base.RaiseBeforeContentsChanged();
        }

        private void OnChildAggregateCountChanged(MyInventoryAggregate obj, int change)
        {
            this.InventoryCount += change;
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

        public override bool Remove(IMyInventoryItem item, MyFixedPoint amount)
        {
            using (List<MyComponentBase>.Enumerator enumerator = this.m_children.Reader.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyInventoryBase current = (MyInventoryBase) enumerator.Current;
                    if (current.ItemsCanBeRemoved(amount, item) && current.Remove(item, amount))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override MyFixedPoint RemoveItemsOfType(MyFixedPoint amount, MyDefinitionId contentId, MyItemFlags flags = 0, bool spawn = false)
        {
            MyFixedPoint point = amount;
            foreach (MyInventoryBase base2 in this.m_children.Reader)
            {
                point -= base2.RemoveItemsOfType(point, contentId, flags, spawn);
            }
            return (amount - point);
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_InventoryAggregate aggregate = base.Serialize(false) as MyObjectBuilder_InventoryAggregate;
            ListReader<MyComponentBase> reader = this.m_children.Reader;
            if (reader.Count > 0)
            {
                aggregate.Inventories = new List<MyObjectBuilder_InventoryBase>(reader.Count);
                using (List<MyComponentBase>.Enumerator enumerator = reader.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyObjectBuilder_InventoryBase item = enumerator.Current.Serialize(false) as MyObjectBuilder_InventoryBase;
                        if (item != null)
                        {
                            aggregate.Inventories.Add(item);
                        }
                    }
                }
            }
            return aggregate;
        }

        public override MyFixedPoint CurrentMass
        {
            get
            {
                float num = 0f;
                foreach (MyInventoryBase base2 in this.m_children.Reader)
                {
                    num += (float) base2.CurrentMass;
                }
                return (MyFixedPoint) num;
            }
        }

        public override MyFixedPoint MaxMass
        {
            get
            {
                float num = 0f;
                foreach (MyInventoryBase base2 in this.m_children.Reader)
                {
                    num += (float) base2.MaxMass;
                }
                return (MyFixedPoint) num;
            }
        }

        public override MyFixedPoint CurrentVolume
        {
            get
            {
                float num = 0f;
                foreach (MyInventoryBase base2 in this.m_children.Reader)
                {
                    num += (float) base2.CurrentVolume;
                }
                return (MyFixedPoint) num;
            }
        }

        public override MyFixedPoint MaxVolume
        {
            get
            {
                float num = 0f;
                foreach (MyInventoryBase base2 in this.m_children.Reader)
                {
                    num += (float) base2.MaxVolume;
                }
                return (MyFixedPoint) num;
            }
        }

        public override int MaxItemCount
        {
            get
            {
                int num = 0;
                foreach (MyInventoryBase base2 in this.m_children.Reader)
                {
                    long num2 = num + base2.MaxItemCount;
                    num = (num2 <= 0x7fffffffL) ? ((int) num2) : 0x7fffffff;
                }
                return num;
            }
        }

        public override float? ForcedPriority
        {
            get => 
                this.m_forcedPriority;
            set
            {
                this.m_forcedPriority = value;
                using (List<MyComponentBase>.Enumerator enumerator = this.m_children.Reader.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        (enumerator.Current as MyInventoryBase).ForcedPriority = value;
                    }
                }
            }
        }

        public int InventoryCount
        {
            get => 
                this.m_inventoryCount;
            private set
            {
                if (this.m_inventoryCount != value)
                {
                    int num = value - this.m_inventoryCount;
                    this.m_inventoryCount = value;
                    if (this.OnInventoryCountChanged != null)
                    {
                        this.OnInventoryCountChanged(this, num);
                    }
                }
            }
        }

        public MyAggregateComponentList ChildList =>
            this.m_children;

        MyComponentContainer IMyComponentAggregate.ContainerBase =>
            base.ContainerBase;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyInventoryAggregate.<>c <>9 = new MyInventoryAggregate.<>c();
            public static Func<MyInventoryAggregate, Action<MyFixedPoint, SerializableDefinitionId, long>> <>9__59_0;

            internal Action<MyFixedPoint, SerializableDefinitionId, long> <ConsumeItem>b__59_0(MyInventoryAggregate x) => 
                new Action<MyFixedPoint, SerializableDefinitionId, long>(x.InventoryConsumeItem_Implementation);
        }
    }
}

