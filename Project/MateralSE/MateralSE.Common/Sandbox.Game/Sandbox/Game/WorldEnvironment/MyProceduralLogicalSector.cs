namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment.Definitions;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyProceduralLogicalSector : MyLogicalEnvironmentSectorBase
    {
        private readonly Dictionary<Type, MyObjectBuilder_EnvironmentModuleBase> m_moduleData = new Dictionary<Type, MyObjectBuilder_EnvironmentModuleBase>();
        private bool m_scanning;
        private bool m_serverOwned;
        private int m_itemOffset;
        private int m_locationOffset;
        private readonly int m_x;
        private readonly int m_y;
        private readonly int m_lod;
        private readonly int[] m_itemCountForLod = new int[0x10];
        private readonly MyProceduralEnvironmentProvider m_provider;
        private readonly HashSet<MyProceduralDataView> m_viewers = new HashSet<MyProceduralDataView>();
        private readonly Vector3 m_basisX;
        private readonly Vector3 m_basisY;
        internal bool Replicable;
        private readonly FastResourceLock m_lock = new FastResourceLock();
        private readonly List<Sandbox.Game.WorldEnvironment.ItemInfo> m_items;
        private readonly int m_itemCountTotal;
        private readonly MyProceduralEnvironmentDefinition m_environment;
        private int m_minimumScannedLod = 0x10;
        private int m_totalSpawned;
        private readonly Dictionary<Type, ModuleData> m_modules = new Dictionary<Type, ModuleData>();
        private readonly int m_seed;
        private readonly MyRandom m_itemPositionRng;
        private readonly List<MyDiscreteSampler<MyRuntimeEnvironmentItemInfo>> m_candidates = new List<MyDiscreteSampler<MyRuntimeEnvironmentItemInfo>>();
        private readonly ProgressiveScanHelper m_scanHelper;
        [CompilerGenerated]
        private Action<MyProceduralLogicalSector> OnViewerEmpty;

        public event Action<MyProceduralLogicalSector> OnViewerEmpty
        {
            [CompilerGenerated] add
            {
                Action<MyProceduralLogicalSector> onViewerEmpty = this.OnViewerEmpty;
                while (true)
                {
                    Action<MyProceduralLogicalSector> a = onViewerEmpty;
                    Action<MyProceduralLogicalSector> action3 = (Action<MyProceduralLogicalSector>) Delegate.Combine(a, value);
                    onViewerEmpty = Interlocked.CompareExchange<Action<MyProceduralLogicalSector>>(ref this.OnViewerEmpty, action3, a);
                    if (ReferenceEquals(onViewerEmpty, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyProceduralLogicalSector> onViewerEmpty = this.OnViewerEmpty;
                while (true)
                {
                    Action<MyProceduralLogicalSector> source = onViewerEmpty;
                    Action<MyProceduralLogicalSector> action3 = (Action<MyProceduralLogicalSector>) Delegate.Remove(source, value);
                    onViewerEmpty = Interlocked.CompareExchange<Action<MyProceduralLogicalSector>>(ref this.OnViewerEmpty, action3, source);
                    if (ReferenceEquals(onViewerEmpty, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyProceduralLogicalSector(MyProceduralEnvironmentProvider provider, int x, int y, int localLod, MyObjectBuilder_ProceduralEnvironmentSector moduleData)
        {
            this.m_provider = provider;
            base.Owner = provider.Owner;
            this.m_x = x;
            this.m_y = y;
            this.m_lod = localLod;
            provider.GeSectorWorldParameters(x, y, localLod * provider.LodFactor, out base.WorldPos, out this.m_basisX, out this.m_basisY);
            this.m_environment = (MyProceduralEnvironmentDefinition) provider.Owner.EnvironmentDefinition;
            this.m_seed = provider.GetSeed() ^ ((((x * 0x179) + y) * 0x179) + this.m_lod);
            this.m_itemPositionRng = new MyRandom(this.m_seed);
            float num = Vector3.Cross(this.m_basisX, this.m_basisY).Length() * 4f;
            this.m_itemCountTotal = (int) (num * this.m_environment.ItemDensity);
            this.m_scanHelper = new ProgressiveScanHelper(this.m_itemCountTotal, localLod * provider.LodFactor);
            base.Bounds = base.Owner.GetBoundingShape(ref base.WorldPos, ref this.m_basisX, ref this.m_basisY);
            this.m_items = new List<Sandbox.Game.WorldEnvironment.ItemInfo>();
            this.m_totalSpawned = 0;
            this.UpdateModuleBuilders(moduleData);
        }

        public unsafe void AddView(MyProceduralDataView view, Vector3D localOrigin, int logicalLod)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.ScanItems(logicalLod);
                this.m_viewers.Add(view);
                this.UpdateMinLod();
                view.AddSector(this);
                int num = this.m_itemCountForLod[logicalLod];
                view.Items.Capacity = view.Items.Count + this.m_items.Count;
                Vector3 vector = (Vector3) (base.WorldPos - localOrigin);
                for (int i = 0; i < num; i++)
                {
                    Sandbox.Game.WorldEnvironment.ItemInfo item = this.m_items[i];
                    Vector3* vectorPtr1 = (Vector3*) ref item.Position;
                    vectorPtr1[0] += vector;
                    view.Items.Add(item);
                }
            }
        }

        public override void Close()
        {
            using (Dictionary<Type, ModuleData>.ValueCollection.Enumerator enumerator = this.m_modules.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Module.Close();
                }
            }
            this.m_modules.Clear();
            this.m_items.Clear();
            base.Close();
        }

        private Vector3 ComputeRandomItemPosition() => 
            ((this.m_basisX * this.m_itemPositionRng.NextFloat(-1f, 1f)) + (this.m_basisY * this.m_itemPositionRng.NextFloat(-1f, 1f)));

        public override void DebugDraw(int lod)
        {
            Vector3D vectord = base.WorldPos + (MySector.MainCamera.UpVector * 1f);
            for (int i = 0; i < this.m_items.Count; i++)
            {
                MyRuntimeEnvironmentItemInfo info2;
                Sandbox.Game.WorldEnvironment.ItemInfo info = this.m_items[i];
                base.Owner.GetDefinition((ushort) info.DefinitionIndex, out info2);
                MyRenderProxy.DebugDrawText3D(info.Position + vectord, $"{info2.Type.Name} i{i} m{info.ModelIndex} d{info.DefinitionIndex}", Color.Purple, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            using (Dictionary<Type, ModuleData>.ValueCollection.Enumerator enumerator = this.m_modules.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Module.DebugDraw();
                }
            }
        }

        public override void DisableItemsInBox(Vector3D center, ref BoundingBoxD box)
        {
            for (int i = 0; i < this.m_items.Count; i++)
            {
                ContainmentType type;
                Vector3D point = center + this.m_items[i].Position;
                box.Contains(ref point, out type);
                if (type == ContainmentType.Contains)
                {
                    this.EnableItem(i, false);
                }
            }
        }

        public override void EnableItem(int itemId, bool enabled)
        {
            short definitionIndex = this.m_items[itemId].DefinitionIndex;
            if (definitionIndex != -1)
            {
                MyRuntimeEnvironmentItemInfo info;
                this.GetItemDefinition((ushort) definitionIndex, out info);
                IMyEnvironmentModule moduleForDefinition = this.GetModuleForDefinition(info);
                if (moduleForDefinition != null)
                {
                    moduleForDefinition.OnItemEnable(itemId, enabled);
                }
            }
        }

        public override void GetItem(int logicalItem, out Sandbox.Game.WorldEnvironment.ItemInfo item)
        {
            if ((logicalItem >= this.m_items.Count) || (logicalItem < 0))
            {
                item = new Sandbox.Game.WorldEnvironment.ItemInfo();
            }
            else
            {
                item = this.m_items[logicalItem];
            }
        }

        public override void GetItemDefinition(ushort key, out MyRuntimeEnvironmentItemInfo it)
        {
            it = this.m_environment.Items[key];
        }

        private MyRuntimeEnvironmentItemInfo GetItemForPosition(ref MySurfaceParams surface, int lod)
        {
            List<MyEnvironmentItemMapping> list;
            MyBiomeMaterial key = new MyBiomeMaterial(surface.Biome, surface.Material);
            this.m_candidates.Clear();
            if (this.m_environment.MaterialEnvironmentMappings.TryGetValue(key, out list))
            {
                foreach (MyEnvironmentItemMapping mapping in list)
                {
                    MyDiscreteSampler<MyRuntimeEnvironmentItemInfo> item = mapping.Sampler(lod);
                    if ((item != null) && mapping.Rule.Check(surface.HeightRatio, surface.Latitude, surface.Longitude, surface.Normal.Z))
                    {
                        this.m_candidates.Add(item);
                    }
                }
            }
            int hashCode = surface.Position.GetHashCode();
            float sample = MyHashRandomUtils.UniformFloatFromSeed(hashCode);
            int count = this.m_candidates.Count;
            return ((count == 0) ? null : ((count == 1) ? this.m_candidates[0].Sample(sample) : this.m_candidates[(int) (MyHashRandomUtils.UniformFloatFromSeed(~hashCode) * this.m_candidates.Count)].Sample(sample)));
        }

        public override void GetItemsInAabb(ref BoundingBoxD aabb, List<int> itemsInBox)
        {
            for (int i = 0; i < this.m_items.Count; i++)
            {
                if ((this.m_items[i].DefinitionIndex >= 0) && (aabb.Contains(this.m_items[i].Position) != ContainmentType.Disjoint))
                {
                    itemsInBox.Add(i);
                }
            }
        }

        private ModuleData GetModule(MyRuntimeEnvironmentItemInfo info)
        {
            ModuleData data;
            Type key = info.Type.StorageModule.Type;
            if (key == null)
            {
                return null;
            }
            if (!this.m_modules.TryGetValue(key, out data))
            {
                data = new ModuleData(key, info.Type.StorageModule.Definition);
                if ((this.m_moduleData == null) || !this.m_moduleData.ContainsKey(key))
                {
                    data.Module.Init(this, null);
                }
                else
                {
                    data.Module.Init(this, this.m_moduleData[key]);
                }
                this.m_modules[key] = data;
            }
            return data;
        }

        private IMyEnvironmentModule GetModuleForDefinition(MyRuntimeEnvironmentItemInfo def)
        {
            ModuleData data;
            return ((def.Type.StorageModule.Type != null) ? (!this.m_modules.TryGetValue(def.Type.StorageModule.Type, out data) ? null : data.Module) : null);
        }

        public override MyObjectBuilder_EnvironmentSector GetObjectBuilder()
        {
            List<MyObjectBuilder_ProceduralEnvironmentSector.Module> list = new List<MyObjectBuilder_ProceduralEnvironmentSector.Module>(this.m_modules.Count);
            foreach (ModuleData data in this.m_modules.Values)
            {
                MyObjectBuilder_EnvironmentModuleBase objectBuilder = data.Module.GetObjectBuilder();
                if (objectBuilder != null)
                {
                    MyObjectBuilder_ProceduralEnvironmentSector.Module item = new MyObjectBuilder_ProceduralEnvironmentSector.Module {
                        ModuleId = (SerializableDefinitionId) data.Definition,
                        Builder = objectBuilder
                    };
                    list.Add(item);
                }
            }
            if (list.Count <= 0)
            {
                return null;
            }
            list.Capacity = list.Count;
            MyObjectBuilder_ProceduralEnvironmentSector sector1 = new MyObjectBuilder_ProceduralEnvironmentSector();
            sector1.SavedModules = list.GetInternalArray<MyObjectBuilder_ProceduralEnvironmentSector.Module>();
            sector1.SectorId = base.Id;
            return sector1;
        }

        private static Vector3 GetRandomPerpendicularVector(ref Vector3 axis, int seed)
        {
            Vector3 vector2;
            Vector3 vector = Vector3.CalculatePerpendicularVector((Vector3) axis);
            Vector3.Cross(ref axis, ref vector, out vector2);
            double d = (MyHashRandomUtils.UniformFloatFromSeed(seed) * 2f) * 3.141593f;
            return (Vector3) ((((float) Math.Cos(d)) * vector) + (((float) Math.Sin(d)) * vector2));
        }

        private void HandleItemEvent(int logicalItem, SerializableDefinitionId def, object data, bool fromClient)
        {
            if (typeof(MyObjectBuilder_ProceduralEnvironmentModuleDefinition).IsAssignableFrom((Type) def.TypeId))
            {
                MyProceduralEnvironmentModuleDefinition definition = MyDefinitionManager.Static.GetDefinition<MyProceduralEnvironmentModuleDefinition>(def);
                if (definition == null)
                {
                    object[] args = new object[] { def };
                    MyLog.Default.Error("Received message about unknown logical module {0}", args);
                }
                else
                {
                    ModuleData data2;
                    if (this.m_modules.TryGetValue(definition.ModuleType, out data2))
                    {
                        data2.Module.HandleSyncEvent(logicalItem, data, fromClient);
                    }
                }
            }
            else
            {
                MyEnvironmentModuleProxyDefinition definition = MyDefinitionManager.Static.GetDefinition<MyEnvironmentModuleProxyDefinition>(def);
                if (definition == null)
                {
                    object[] args = new object[] { def };
                    MyLog.Default.Error("Received message about unknown module proxy {0}", args);
                }
                else
                {
                    foreach (MyProceduralDataView view in this.m_viewers)
                    {
                        if (view.Listener == null)
                        {
                            continue;
                        }
                        IMyEnvironmentModuleProxy module = view.Listener.GetModule(definition.ModuleType);
                        int sectorIndex = view.GetSectorIndex(this.m_x, this.m_y);
                        int num2 = view.SectorOffsets[sectorIndex];
                        if ((logicalItem < this.m_itemCountForLod[view.Lod]) && (module != null))
                        {
                            module.HandleSyncEvent(logicalItem + num2, data, fromClient);
                        }
                    }
                }
            }
        }

        [Event(null, 0x2cd), Reliable, Server]
        private void HandleItemEventClient(int logicalItem, SerializableDefinitionId def, [Serialize(MyObjectFlags.Dynamic | MyObjectFlags.DefaultZero, DynamicSerializerType=typeof(MyDynamicObjectResolver))] object data)
        {
            this.HandleItemEvent(logicalItem, def, data, true);
        }

        [Broadcast, Event(null, 0x2c4), Reliable]
        private void HandleItemEventServer(int logicalItem, SerializableDefinitionId def, [Serialize(MyObjectFlags.Dynamic | MyObjectFlags.DefaultZero, DynamicSerializerType=typeof(MyDynamicObjectResolver))] object data)
        {
            this.HandleItemEvent(logicalItem, def, data, false);
        }

        public override void Init(MyObjectBuilder_EnvironmentSector sectorBuilder)
        {
            this.UpdateModuleBuilders((MyObjectBuilder_ProceduralEnvironmentSector) sectorBuilder);
        }

        public override void InvalidateItem(int itemId)
        {
            Sandbox.Game.WorldEnvironment.ItemInfo info = this.m_items[itemId];
            info.DefinitionIndex = -1;
            this.m_items[itemId] = info;
        }

        public override unsafe void IterateItems(MyLogicalEnvironmentSectorBase.ItemIterator action)
        {
            Sandbox.Game.WorldEnvironment.ItemInfo* infoPtr;
            Sandbox.Game.WorldEnvironment.ItemInfo[] pinned infoArray;
            if (((infoArray = this.m_items.GetInternalArray<Sandbox.Game.WorldEnvironment.ItemInfo>()) == null) || (infoArray.Length == 0))
            {
                infoPtr = null;
            }
            else
            {
                infoPtr = infoArray;
            }
            for (int i = 0; i < this.m_items.Count; i++)
            {
                action(i, infoPtr + i);
            }
            infoArray = null;
        }

        public void RaiseItemEvent<TModule>(int logicalItem, object eventData, bool fromClient = false) where TModule: IMyEnvironmentModule
        {
            MyDefinitionId definition = this.m_modules[typeof(TModule)].Definition;
            this.RaiseItemEvent<object>(logicalItem, ref definition, eventData, fromClient);
        }

        public override void RaiseItemEvent<T>(int logicalItem, ref MyDefinitionId modDef, T eventData, bool fromClient)
        {
            EndpointId id;
            if (fromClient)
            {
                id = new EndpointId();
                MyMultiplayer.RaiseEvent<MyProceduralLogicalSector, int, SerializableDefinitionId, object>(this, x => new Action<int, SerializableDefinitionId, object>(x.HandleItemEventClient), logicalItem, (SerializableDefinitionId) modDef, eventData, id);
            }
            else
            {
                id = new EndpointId();
                MyMultiplayer.RaiseEvent<MyProceduralLogicalSector, int, SerializableDefinitionId, object>(this, x => new Action<int, SerializableDefinitionId, object>(x.HandleItemEventServer), logicalItem, (SerializableDefinitionId) modDef, eventData, id);
            }
        }

        public void RemoveView(MyProceduralDataView view)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_viewers.Remove(view);
                this.UpdateMinLod();
                if ((this.m_viewers.Count == 0) && (this.OnViewerEmpty != null))
                {
                    this.OnViewerEmpty(this);
                }
            }
        }

        private void ScanItems(int targetLod)
        {
            int num = this.m_minimumScannedLod - targetLod;
            if (num >= 1)
            {
                int changedLodMin = this.m_minimumScannedLod - 1;
                int capacity = 0;
                int[] numArray = new int[num];
                for (int i = changedLodMin; i >= targetLod; i--)
                {
                    int itemsForLod = this.m_scanHelper.GetItemsForLod(i);
                    numArray[i - targetLod] = capacity + this.m_totalSpawned;
                    capacity += itemsForLod;
                }
                List<Vector3> queries = new List<Vector3>(capacity);
                for (int j = 0; j < capacity; j++)
                {
                    queries.Add(this.ComputeRandomItemPosition());
                }
                BoundingBoxD queryBounds = BoundingBoxD.CreateFromPoints(base.Bounds);
                List<MySurfaceParams> results = new List<MySurfaceParams>(capacity);
                this.m_provider.Owner.QuerySurfaceParameters(base.WorldPos, ref queryBounds, queries, results);
                this.m_items.Capacity = this.m_items.Count + queries.Count;
                int num4 = 0;
                int index = changedLodMin;
                while (index >= targetLod)
                {
                    int num9 = index - targetLod;
                    int num10 = (index > targetLod) ? (numArray[num9 - 1] - numArray[num9]) : (capacity - numArray[num9]);
                    int num11 = 0;
                    while (true)
                    {
                        if (num11 >= num10)
                        {
                            this.m_itemCountForLod[index] = this.m_totalSpawned;
                            foreach (ModuleData data2 in this.m_modules.Values)
                            {
                                if (data2 != null)
                                {
                                    foreach (short num13 in data2.ItemsPerDefinition.Keys.ToArray<short>())
                                    {
                                        MyLodEnvironmentItemSet set3 = data2.ItemsPerDefinition[num13];
                                        data2.ItemsPerDefinition[num13] = set3;
                                    }
                                }
                            }
                            index--;
                            break;
                        }
                        MySurfaceParams surface = results[num4];
                        MyRuntimeEnvironmentItemInfo itemForPosition = this.GetItemForPosition(ref surface, index);
                        if ((itemForPosition != null) && (index >= itemForPosition.Type.LodTo))
                        {
                            ModuleData module = this.GetModule(itemForPosition);
                            if (module != null)
                            {
                                MyLodEnvironmentItemSet set;
                                if (!module.ItemsPerDefinition.TryGetValue(itemForPosition.Index, out set))
                                {
                                    MyLodEnvironmentItemSet set2 = new MyLodEnvironmentItemSet {
                                        Items = new List<int>()
                                    };
                                    module.ItemsPerDefinition[itemForPosition.Index] = set = set2;
                                }
                                set.Items.Add(this.m_totalSpawned);
                            }
                            Vector3 axis = -surface.Gravity;
                            Sandbox.Game.WorldEnvironment.ItemInfo info3 = new Sandbox.Game.WorldEnvironment.ItemInfo {
                                Position = surface.Position,
                                ModelIndex = -1
                            };
                            Vector3 randomPerpendicularVector = GetRandomPerpendicularVector(ref axis, surface.Position.GetHashCode());
                            info3.Rotation = Quaternion.CreateFromForwardUp(randomPerpendicularVector, axis);
                            info3.DefinitionIndex = itemForPosition.Index;
                            Sandbox.Game.WorldEnvironment.ItemInfo item = info3;
                            this.m_items.Add(item);
                            this.m_totalSpawned++;
                        }
                        num4++;
                        num11++;
                    }
                }
                this.m_scanning = true;
                foreach (ModuleData data3 in this.m_modules.Values)
                {
                    if (data3 != null)
                    {
                        data3.Module.ProcessItems(data3.ItemsPerDefinition, changedLodMin, targetLod);
                    }
                }
                this.m_scanning = false;
                this.m_minimumScannedLod = targetLod;
            }
        }

        public override string ToString() => 
            $"x{this.m_x} y{this.m_y} l{this.m_lod} : {this.m_items.Count}";

        public override void UpdateItemModel(int itemId, short modelId)
        {
            if (itemId < this.m_items.Count)
            {
                if (!this.m_scanning)
                {
                    foreach (MyProceduralDataView view in this.m_viewers)
                    {
                        if (view.Listener == null)
                        {
                            continue;
                        }
                        int sectorIndex = view.GetSectorIndex(this.m_x, this.m_y);
                        int num2 = view.SectorOffsets[sectorIndex];
                        if (itemId < this.m_itemCountForLod[view.Lod])
                        {
                            view.Listener.OnItemChange(itemId + num2, modelId);
                        }
                    }
                }
                Sandbox.Game.WorldEnvironment.ItemInfo info = this.m_items[itemId];
                info.ModelIndex = modelId;
                this.m_items[itemId] = info;
            }
        }

        public override void UpdateItemModelBatch(List<int> itemIds, short newModelId)
        {
            int count = itemIds.Count;
            if (!this.m_scanning)
            {
                foreach (MyProceduralDataView view in this.m_viewers)
                {
                    if (view.Listener != null)
                    {
                        int sectorIndex = view.GetSectorIndex(this.m_x, this.m_y);
                        view.Listener.OnItemsChange(sectorIndex, itemIds, newModelId);
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                Sandbox.Game.WorldEnvironment.ItemInfo info = this.m_items[itemIds[i]];
                info.ModelIndex = newModelId;
                this.m_items[itemIds[i]] = info;
            }
        }

        private void UpdateMinLod()
        {
            base.MinLod = 0x7fffffff;
            foreach (MyProceduralDataView view in this.m_viewers)
            {
                base.MinLod = Math.Min(view.Lod, base.MinLod);
            }
            bool flag = base.MinLod <= this.m_provider.SyncLod;
            if (flag != this.Replicable)
            {
                if (flag)
                {
                    this.m_provider.MarkReplicable(this);
                }
                else
                {
                    this.m_provider.UnmarkReplicable(this);
                }
                this.Replicable = flag;
            }
        }

        private void UpdateModuleBuilders(MyObjectBuilder_ProceduralEnvironmentSector moduleData)
        {
            this.m_moduleData.Clear();
            if (moduleData != null)
            {
                for (int i = 0; i < moduleData.SavedModules.Length; i++)
                {
                    MyObjectBuilder_ProceduralEnvironmentSector.Module module = moduleData.SavedModules[i];
                    MyProceduralEnvironmentModuleDefinition definition = MyDefinitionManager.Static.GetDefinition<MyProceduralEnvironmentModuleDefinition>(module.ModuleId);
                    if (definition != null)
                    {
                        ModuleData data;
                        this.m_moduleData.Add(definition.ModuleType, module.Builder);
                        if (this.m_modules.TryGetValue(definition.ModuleType, out data))
                        {
                            data.Module.Init(this, module.Builder);
                        }
                    }
                }
            }
        }

        public override bool ServerOwned
        {
            get => 
                this.m_serverOwned;
            internal set
            {
                this.m_serverOwned = value;
                if ((!Sync.IsServer && (this.m_viewers.Count == 0)) && (this.OnViewerEmpty != null))
                {
                    this.OnViewerEmpty(this);
                }
            }
        }

        public override string DebugData =>
            $"x:{this.m_x} y:{this.m_y} highLod:{this.m_lod} localLod:{this.m_minimumScannedLod} seed:{this.m_seed:X} count:{this.m_items.Count} ";

        [Serializable, CompilerGenerated]
        private sealed class <>c__52<T>
        {
            public static readonly MyProceduralLogicalSector.<>c__52<T> <>9;
            public static Func<MyProceduralLogicalSector, Action<int, SerializableDefinitionId, object>> <>9__52_0;
            public static Func<MyProceduralLogicalSector, Action<int, SerializableDefinitionId, object>> <>9__52_1;

            static <>c__52()
            {
                MyProceduralLogicalSector.<>c__52<T>.<>9 = new MyProceduralLogicalSector.<>c__52<T>();
            }

            internal Action<int, SerializableDefinitionId, object> <RaiseItemEvent>b__52_0(MyProceduralLogicalSector x) => 
                new Action<int, SerializableDefinitionId, object>(x.HandleItemEventClient);

            internal Action<int, SerializableDefinitionId, object> <RaiseItemEvent>b__52_1(MyProceduralLogicalSector x) => 
                new Action<int, SerializableDefinitionId, object>(x.HandleItemEventServer);
        }

        private class ModuleData
        {
            public readonly Dictionary<short, MyLodEnvironmentItemSet> ItemsPerDefinition = new Dictionary<short, MyLodEnvironmentItemSet>();
            public readonly IMyEnvironmentModule Module;
            public readonly MyDefinitionId Definition;

            public ModuleData(Type type, MyDefinitionId definition)
            {
                this.Module = (IMyEnvironmentModule) Activator.CreateInstance(type);
                this.Definition = definition;
            }
        }

        private class ProgressiveScanHelper
        {
            private readonly int m_itemsTotal;
            private readonly int m_offset;
            private const bool EXAGERATE = true;
            private readonly double m_base;
            private readonly double m_logMaxLodRecip;

            public ProgressiveScanHelper(int finalCount, int offset)
            {
                this.m_itemsTotal = finalCount;
                int num = 4;
                this.m_logMaxLodRecip = 1.0 / Math.Log((double) num);
                this.m_base = Math.Log(10.0) * this.m_logMaxLodRecip;
                this.m_offset = offset;
            }

            private double F(double x) => 
                (-Math.Pow(this.m_base, -x) * this.m_logMaxLodRecip);

            public int GetItemsForLod(int lod)
            {
                lod += this.m_offset;
                return (int) (this.m_itemsTotal * (this.F((double) (lod + 1)) - this.F((double) lod)));
            }
        }
    }
}

