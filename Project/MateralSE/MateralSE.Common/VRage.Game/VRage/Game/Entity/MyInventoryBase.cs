namespace VRage.Game.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyComponentType(typeof(MyInventoryBase)), StaticEventOwner]
    public abstract class MyInventoryBase : MyEntityComponentBase, IMyEventProxy, IMyEventOwner
    {
        public bool RemoveEntityOnEmpty;
        [CompilerGenerated]
        private Action<MyInventoryBase> ContentsChanged;
        [CompilerGenerated]
        private Action<MyInventoryBase> BeforeContentsChanged;
        [CompilerGenerated]
        private Action<MyPhysicalInventoryItem, MyFixedPoint> ContentsAdded;
        [CompilerGenerated]
        private Action<MyPhysicalInventoryItem, MyFixedPoint> ContentsRemoved;
        [CompilerGenerated]
        private Action<MyInventoryBase, MyComponentContainer> OwnerChanged;

        public event Action<MyInventoryBase> BeforeContentsChanged
        {
            [CompilerGenerated] add
            {
                Action<MyInventoryBase> beforeContentsChanged = this.BeforeContentsChanged;
                while (true)
                {
                    Action<MyInventoryBase> a = beforeContentsChanged;
                    Action<MyInventoryBase> action3 = (Action<MyInventoryBase>) Delegate.Combine(a, value);
                    beforeContentsChanged = Interlocked.CompareExchange<Action<MyInventoryBase>>(ref this.BeforeContentsChanged, action3, a);
                    if (ReferenceEquals(beforeContentsChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyInventoryBase> beforeContentsChanged = this.BeforeContentsChanged;
                while (true)
                {
                    Action<MyInventoryBase> source = beforeContentsChanged;
                    Action<MyInventoryBase> action3 = (Action<MyInventoryBase>) Delegate.Remove(source, value);
                    beforeContentsChanged = Interlocked.CompareExchange<Action<MyInventoryBase>>(ref this.BeforeContentsChanged, action3, source);
                    if (ReferenceEquals(beforeContentsChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyPhysicalInventoryItem, MyFixedPoint> ContentsAdded
        {
            [CompilerGenerated] add
            {
                Action<MyPhysicalInventoryItem, MyFixedPoint> contentsAdded = this.ContentsAdded;
                while (true)
                {
                    Action<MyPhysicalInventoryItem, MyFixedPoint> a = contentsAdded;
                    Action<MyPhysicalInventoryItem, MyFixedPoint> action3 = (Action<MyPhysicalInventoryItem, MyFixedPoint>) Delegate.Combine(a, value);
                    contentsAdded = Interlocked.CompareExchange<Action<MyPhysicalInventoryItem, MyFixedPoint>>(ref this.ContentsAdded, action3, a);
                    if (ReferenceEquals(contentsAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyPhysicalInventoryItem, MyFixedPoint> contentsAdded = this.ContentsAdded;
                while (true)
                {
                    Action<MyPhysicalInventoryItem, MyFixedPoint> source = contentsAdded;
                    Action<MyPhysicalInventoryItem, MyFixedPoint> action3 = (Action<MyPhysicalInventoryItem, MyFixedPoint>) Delegate.Remove(source, value);
                    contentsAdded = Interlocked.CompareExchange<Action<MyPhysicalInventoryItem, MyFixedPoint>>(ref this.ContentsAdded, action3, source);
                    if (ReferenceEquals(contentsAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyInventoryBase> ContentsChanged
        {
            [CompilerGenerated] add
            {
                Action<MyInventoryBase> contentsChanged = this.ContentsChanged;
                while (true)
                {
                    Action<MyInventoryBase> a = contentsChanged;
                    Action<MyInventoryBase> action3 = (Action<MyInventoryBase>) Delegate.Combine(a, value);
                    contentsChanged = Interlocked.CompareExchange<Action<MyInventoryBase>>(ref this.ContentsChanged, action3, a);
                    if (ReferenceEquals(contentsChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyInventoryBase> contentsChanged = this.ContentsChanged;
                while (true)
                {
                    Action<MyInventoryBase> source = contentsChanged;
                    Action<MyInventoryBase> action3 = (Action<MyInventoryBase>) Delegate.Remove(source, value);
                    contentsChanged = Interlocked.CompareExchange<Action<MyInventoryBase>>(ref this.ContentsChanged, action3, source);
                    if (ReferenceEquals(contentsChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyPhysicalInventoryItem, MyFixedPoint> ContentsRemoved
        {
            [CompilerGenerated] add
            {
                Action<MyPhysicalInventoryItem, MyFixedPoint> contentsRemoved = this.ContentsRemoved;
                while (true)
                {
                    Action<MyPhysicalInventoryItem, MyFixedPoint> a = contentsRemoved;
                    Action<MyPhysicalInventoryItem, MyFixedPoint> action3 = (Action<MyPhysicalInventoryItem, MyFixedPoint>) Delegate.Combine(a, value);
                    contentsRemoved = Interlocked.CompareExchange<Action<MyPhysicalInventoryItem, MyFixedPoint>>(ref this.ContentsRemoved, action3, a);
                    if (ReferenceEquals(contentsRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyPhysicalInventoryItem, MyFixedPoint> contentsRemoved = this.ContentsRemoved;
                while (true)
                {
                    Action<MyPhysicalInventoryItem, MyFixedPoint> source = contentsRemoved;
                    Action<MyPhysicalInventoryItem, MyFixedPoint> action3 = (Action<MyPhysicalInventoryItem, MyFixedPoint>) Delegate.Remove(source, value);
                    contentsRemoved = Interlocked.CompareExchange<Action<MyPhysicalInventoryItem, MyFixedPoint>>(ref this.ContentsRemoved, action3, source);
                    if (ReferenceEquals(contentsRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyInventoryBase, MyComponentContainer> OwnerChanged
        {
            [CompilerGenerated] add
            {
                Action<MyInventoryBase, MyComponentContainer> ownerChanged = this.OwnerChanged;
                while (true)
                {
                    Action<MyInventoryBase, MyComponentContainer> a = ownerChanged;
                    Action<MyInventoryBase, MyComponentContainer> action3 = (Action<MyInventoryBase, MyComponentContainer>) Delegate.Combine(a, value);
                    ownerChanged = Interlocked.CompareExchange<Action<MyInventoryBase, MyComponentContainer>>(ref this.OwnerChanged, action3, a);
                    if (ReferenceEquals(ownerChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyInventoryBase, MyComponentContainer> ownerChanged = this.OwnerChanged;
                while (true)
                {
                    Action<MyInventoryBase, MyComponentContainer> source = ownerChanged;
                    Action<MyInventoryBase, MyComponentContainer> action3 = (Action<MyInventoryBase, MyComponentContainer>) Delegate.Remove(source, value);
                    ownerChanged = Interlocked.CompareExchange<Action<MyInventoryBase, MyComponentContainer>>(ref this.OwnerChanged, action3, source);
                    if (ReferenceEquals(ownerChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyInventoryBase(string inventoryId)
        {
            this.InventoryId = MyStringHash.GetOrCompute(inventoryId);
        }

        public abstract bool Add(IMyInventoryItem item, MyFixedPoint amount);
        public abstract bool AddItems(MyFixedPoint amount, MyObjectBuilder_Base objectBuilder);
        public abstract void ApplyChanges(List<MyComponentChange> changes);
        public abstract MyFixedPoint ComputeAmountThatFits(MyDefinitionId contentId, float volumeRemoved = 0f, float massRemoved = 0f);
        public abstract void ConsumeItem(MyDefinitionId itemId, MyFixedPoint amount, long consumerEntityId = 0L);
        public abstract void CountItems(Dictionary<MyDefinitionId, MyFixedPoint> itemCounts);
        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            MyObjectBuilder_InventoryBase base2 = builder as MyObjectBuilder_InventoryBase;
            this.InventoryId = MyStringHash.GetOrCompute(base2.InventoryId ?? "Inventory");
        }

        public abstract int GetInventoryCount();
        public abstract MyFixedPoint GetItemAmount(MyDefinitionId contentId, MyItemFlags flags = 0, bool substitute = false);
        public abstract List<MyPhysicalInventoryItem> GetItems();
        public virtual int GetItemsCount() => 
            0;

        public override bool IsSerialized() => 
            true;

        public abstract bool ItemsCanBeAdded(MyFixedPoint amount, IMyInventoryItem item);
        public abstract bool ItemsCanBeRemoved(MyFixedPoint amount, IMyInventoryItem item);
        public abstract MyInventoryBase IterateInventory(int searchIndex, int currentIndex = 0);
        public abstract void OnBeforeContentsChanged();
        public abstract void OnContentsAdded(MyPhysicalInventoryItem item, MyFixedPoint amount);
        public abstract void OnContentsChanged();
        public abstract void OnContentsRemoved(MyPhysicalInventoryItem item, MyFixedPoint amount);
        protected void OnOwnerChanged()
        {
            Action<MyInventoryBase, MyComponentContainer> ownerChanged = this.OwnerChanged;
            if (ownerChanged != null)
            {
                ownerChanged(this, base.Container);
            }
        }

        public void RaiseBeforeContentsChanged()
        {
            this.BeforeContentsChanged.InvokeIfNotNull<MyInventoryBase>(this);
        }

        public void RaiseContentsAdded(MyPhysicalInventoryItem item, MyFixedPoint amount)
        {
            this.ContentsAdded.InvokeIfNotNull<MyPhysicalInventoryItem, MyFixedPoint>(item, amount);
        }

        public void RaiseContentsChanged()
        {
            this.ContentsChanged.InvokeIfNotNull<MyInventoryBase>(this);
        }

        public void RaiseContentsRemoved(MyPhysicalInventoryItem item, MyFixedPoint amount)
        {
            this.ContentsRemoved.InvokeIfNotNull<MyPhysicalInventoryItem, MyFixedPoint>(item, amount);
        }

        public abstract bool Remove(IMyInventoryItem item, MyFixedPoint amount);
        public abstract MyFixedPoint RemoveItemsOfType(MyFixedPoint amount, MyDefinitionId contentId, MyItemFlags flags = 0, bool spawn = false);
        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_InventoryBase base1 = base.Serialize(false) as MyObjectBuilder_InventoryBase;
            base1.InventoryId = this.InventoryId.ToString();
            return base1;
        }

        public override string ToString() => 
            (base.ToString() + " - " + this.InventoryId.ToString());

        public MyStringHash InventoryId { get; private set; }

        public abstract MyFixedPoint CurrentMass { get; }

        public abstract MyFixedPoint MaxMass { get; }

        public abstract int MaxItemCount { get; }

        public abstract MyFixedPoint CurrentVolume { get; }

        public abstract MyFixedPoint MaxVolume { get; }

        public abstract float? ForcedPriority { get; set; }

        public override string ComponentTypeDebugString =>
            "Inventory";

        public override bool AttachSyncToEntity =>
            false;
    }
}

