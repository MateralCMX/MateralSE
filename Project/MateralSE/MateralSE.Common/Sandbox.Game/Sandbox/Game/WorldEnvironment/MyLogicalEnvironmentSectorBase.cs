namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox.Game.WorldEnvironment.Definitions;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Network;
    using VRageMath;

    public abstract class MyLogicalEnvironmentSectorBase : IMyEventProxy, IMyEventOwner
    {
        public long Id;
        public Vector3D WorldPos;
        public Vector3D[] Bounds;
        [CompilerGenerated]
        private Action OnClose;

        public event Action OnClose
        {
            [CompilerGenerated] add
            {
                Action onClose = this.OnClose;
                while (true)
                {
                    Action a = onClose;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onClose = Interlocked.CompareExchange<Action>(ref this.OnClose, action3, a);
                    if (ReferenceEquals(onClose, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onClose = this.OnClose;
                while (true)
                {
                    Action source = onClose;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onClose = Interlocked.CompareExchange<Action>(ref this.OnClose, action3, source);
                    if (ReferenceEquals(onClose, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MyLogicalEnvironmentSectorBase()
        {
        }

        public virtual void Close()
        {
            Action onClose = this.OnClose;
            if (onClose != null)
            {
                onClose();
            }
        }

        public abstract void DebugDraw(int lod);
        public abstract void DisableItemsInBox(Vector3D center, ref BoundingBoxD box);
        public abstract void EnableItem(int itemId, bool enabled);
        public abstract void GetItem(int logicalItem, out Sandbox.Game.WorldEnvironment.ItemInfo item);
        public abstract void GetItemDefinition(ushort index, out MyRuntimeEnvironmentItemInfo def);
        public abstract void GetItemsInAabb(ref BoundingBoxD aabb, List<int> itemsInBox);
        public abstract MyObjectBuilder_EnvironmentSector GetObjectBuilder();
        public abstract void Init(MyObjectBuilder_EnvironmentSector sectorBuilder);
        public abstract void InvalidateItem(int itemId);
        public abstract void IterateItems(ItemIterator action);
        public abstract void RaiseItemEvent<T>(int logicalItem, ref MyDefinitionId modDef, T eventData, bool fromClient);
        public abstract void UpdateItemModel(int itemId, short modelId);
        public abstract void UpdateItemModelBatch(List<int> items, short newModelId);

        public IMyEnvironmentOwner Owner { get; protected set; }

        public abstract string DebugData { get; }

        public abstract bool ServerOwned { get; internal set; }

        public int MinLod { get; protected set; }

        public unsafe delegate void ItemIterator(int index, Sandbox.Game.WorldEnvironment.ItemInfo* item);
    }
}

