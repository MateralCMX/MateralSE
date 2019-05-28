namespace Sandbox.Game.WorldEnvironment.Modules
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.Game.WorldEnvironment.Definitions;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    public class MyVoxelMapEnvironmentProxy : IMyEnvironmentModuleProxy
    {
        protected MyEnvironmentSector m_sector;
        protected MyPlanet m_planet;
        protected readonly MyRandom m_random = new MyRandom();
        protected readonly MyVoxelBase.StorageChanged m_voxelMap_RangeChangedDelegate;
        protected List<int> m_items;
        protected List<VoxelMapInfo> m_voxelMapsToAdd = new List<VoxelMapInfo>();
        protected Dictionary<MyVoxelMap, int> m_voxelMaps = new Dictionary<MyVoxelMap, int>();
        private static List<MyEntity> m_entities = new List<MyEntity>();

        public MyVoxelMapEnvironmentProxy()
        {
            this.m_voxelMap_RangeChangedDelegate = new MyVoxelBase.StorageChanged(this.VoxelMap_RangeChanged);
        }

        private void AddVoxelMap(int item, string prefabName, MatrixD matrix, string name, long entityId, Dictionary<byte, byte> modifiers = null)
        {
            MyStorageBase storage = MyStorageBase.LoadFromFile(MyWorldGenerator.GetVoxelPrefabPath(prefabName), modifiers, true);
            if (storage != null)
            {
                MyOrientedBoundingBoxD other = new MyOrientedBoundingBoxD(matrix.Translation, (Vector3D) (storage.Size * 0.5f), Quaternion.CreateFromRotationMatrix(matrix));
                BoundingBoxD aABB = other.GetAABB();
                using (MyUtils.ReuseCollection<MyEntity>(ref m_entities))
                {
                    MyGamePruningStructure.GetTopMostEntitiesInBox(ref aABB, m_entities, MyEntityQueryType.Static);
                    using (List<MyEntity>.Enumerator enumerator = m_entities.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            MyEntity current = enumerator.Current;
                            if (!(current is MyVoxelPhysics) && (!(current is MyPlanet) && !(current is MyEnvironmentSector)))
                            {
                                MyPositionComponentBase positionComp = current.PositionComp;
                                if (MyOrientedBoundingBoxD.Create(positionComp.LocalAABB, positionComp.WorldMatrix).Intersects(ref other))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
                MyVoxelMap voxelMap = MyWorldGenerator.AddVoxelMap(name, storage, matrix, entityId, true, true);
                if (voxelMap != null)
                {
                    this.RegisterVoxelMap(item, voxelMap);
                }
            }
        }

        private void AddVoxelMaps()
        {
            if (this.m_voxelMaps.Count <= 0)
            {
                foreach (VoxelMapInfo info in this.m_voxelMapsToAdd)
                {
                    MyVoxelMap map;
                    if (MyEntities.TryGetEntityById<MyVoxelMap>(info.EntityId, out map, false))
                    {
                        if (map.Save)
                        {
                            continue;
                        }
                        this.RegisterVoxelMap(info.Item, map);
                        continue;
                    }
                    MyVoxelMaterialModifierDefinition definition = MyDefinitionManager.Static.GetDefinition<MyVoxelMaterialModifierDefinition>(info.Modifier);
                    Dictionary<byte, byte> modifiers = null;
                    if (definition != null)
                    {
                        modifiers = definition.Options.Sample(MyHashRandomUtils.UniformFloatFromSeed(info.Item + info.Matrix.GetHashCode())).Changes;
                    }
                    this.AddVoxelMap(info.Item, info.Storage.SubtypeName, info.Matrix, info.Name, info.EntityId, modifiers);
                }
            }
        }

        public void Close()
        {
            this.RemoveVoxelMaps();
        }

        public void CommitLodChange(int lodBefore, int lodAfter)
        {
            if (lodAfter >= 0)
            {
                this.AddVoxelMaps();
            }
            else if (!this.m_sector.HasPhysics)
            {
                this.RemoveVoxelMaps();
            }
        }

        public void CommitPhysicsChange(bool enabled)
        {
            if (enabled)
            {
                this.AddVoxelMaps();
            }
            else if (this.m_sector.LodLevel == -1)
            {
                this.RemoveVoxelMaps();
            }
        }

        public void DebugDraw()
        {
        }

        private unsafe void DisableOtherItemsInVMap(MyVoxelBase voxelMap)
        {
            MyOrientedBoundingBoxD obb = MyOrientedBoundingBoxD.Create(voxelMap.PositionComp.LocalAABB, voxelMap.PositionComp.WorldMatrix);
            Vector3D center = obb.Center;
            BoundingBoxD worldAABB = voxelMap.PositionComp.WorldAABB;
            using (MyUtils.ReuseCollection<MyEntity>(ref m_entities))
            {
                MyGamePruningStructure.GetAllEntitiesInBox(ref worldAABB, m_entities, MyEntityQueryType.Static);
                for (int i = 0; i < m_entities.Count; i++)
                {
                    MyEnvironmentSector sector = m_entities[i] as MyEnvironmentSector;
                    if ((sector != null) && (sector.DataView != null))
                    {
                        obb.Center = center - sector.SectorCenter;
                        for (int j = 0; j < sector.DataView.LogicalSectors.Count; j++)
                        {
                            MyLogicalEnvironmentSectorBase logicalSector = sector.DataView.LogicalSectors[j];
                            logicalSector.IterateItems(delegate (int i, Sandbox.Game.WorldEnvironment.ItemInfo* x) {
                                Vector3D worldPoints = x.Position + sector.SectorCenter;
                                if (((x.DefinitionIndex >= 0) && (obb.Contains(ref x.Position) && (voxelMap.CountPointsInside(&worldPoints, 1) > 0))) && !IsVoxelItem(sector, x.DefinitionIndex))
                                {
                                    logicalSector.EnableItem(i, false);
                                }
                            });
                        }
                    }
                }
            }
        }

        public void HandleSyncEvent(int item, object data, bool fromClient)
        {
        }

        public void Init(MyEnvironmentSector sector, List<int> items)
        {
            this.m_sector = sector;
            MyEntityComponentBase owner = (MyEntityComponentBase) this.m_sector.Owner;
            this.m_planet = owner.Entity as MyPlanet;
            this.m_items = items;
            this.LoadVoxelMapsInfo();
        }

        private static bool IsVoxelItem(MyEnvironmentSector sector, short definitionIndex)
        {
            MyItemTypeDefinition.Module[] proxyModules = sector.Owner.EnvironmentDefinition.Items[definitionIndex].Type.ProxyModules;
            if (proxyModules != null)
            {
                for (int i = 0; i < proxyModules.Length; i++)
                {
                    if (proxyModules[i].Type.IsSubclassOf(typeof(MyVoxelMapEnvironmentProxy)) || (proxyModules[i].Type == typeof(MyVoxelMapEnvironmentProxy)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void LoadVoxelMapsInfo()
        {
            this.m_voxelMapsToAdd.Clear();
            foreach (int num in this.m_items)
            {
                MyRuntimeEnvironmentItemInfo info2;
                Sandbox.Game.WorldEnvironment.ItemInfo info = this.m_sector.DataView.Items[num];
                this.m_sector.Owner.GetDefinition((ushort) info.DefinitionIndex, out info2);
                MyDefinitionId subtypeId = new MyDefinitionId(typeof(MyObjectBuilder_VoxelMapCollectionDefinition), info2.Subtype);
                MyVoxelMapCollectionDefinition definition = MyDefinitionManager.Static.GetDefinition<MyVoxelMapCollectionDefinition>(subtypeId);
                if (definition != null)
                {
                    int num2;
                    MyLogicalEnvironmentSectorBase base2;
                    this.m_sector.DataView.GetLogicalSector(num, out num2, out base2);
                    string uniqueString = $"P({this.m_sector.Owner.Entity.Name})S({base2.Id})A({info2.Subtype}__{num2})";
                    MatrixD xd = MatrixD.CreateFromQuaternion(info.Rotation);
                    xd.Translation = this.m_sector.SectorCenter + info.Position;
                    long num3 = MyEntityIdentifier.ConstructIdFromString(MyEntityIdentifier.ID_OBJECT_TYPE.PLANET_VOXEL_DETAIL, uniqueString);
                    using (this.m_random.PushSeed(info.Rotation.GetHashCode()))
                    {
                        VoxelMapInfo item = new VoxelMapInfo {
                            Name = uniqueString,
                            Storage = definition.StorageFiles.Sample(this.m_random),
                            Matrix = xd,
                            Item = num,
                            Modifier = definition.Modifier,
                            EntityId = num3
                        };
                        this.m_voxelMapsToAdd.Add(item);
                    }
                }
            }
        }

        public void OnItemChange(int index, short newModel)
        {
        }

        public void OnItemChangeBatch(List<int> items, int offset, short newModel)
        {
        }

        private void RegisterVoxelMap(int item, MyVoxelMap voxelMap)
        {
            MyEntityReferenceComponent component;
            voxelMap.Save = false;
            voxelMap.RangeChanged += this.m_voxelMap_RangeChangedDelegate;
            this.m_voxelMaps[voxelMap] = item;
            if (!voxelMap.Components.TryGet<MyEntityReferenceComponent>(out component))
            {
                voxelMap.Components.Add<MyEntityReferenceComponent>(component = new MyEntityReferenceComponent());
            }
            this.DisableOtherItemsInVMap(voxelMap);
            component.Ref();
        }

        private void RemoveVoxelMap(MyVoxelMap map)
        {
            map.Save = true;
            map.RangeChanged -= this.m_voxelMap_RangeChangedDelegate;
            if (this.m_voxelMaps.ContainsKey(map))
            {
                int itemId = this.m_voxelMaps[map];
                this.m_sector.EnableItem(itemId, false);
                this.m_voxelMaps.Remove(map);
            }
        }

        private void RemoveVoxelMaps()
        {
            foreach (KeyValuePair<MyVoxelMap, int> pair in this.m_voxelMaps)
            {
                MyVoxelMap key = pair.Key;
                if (!key.Closed)
                {
                    if (Sync.IsServer || !key.Save)
                    {
                        key.Components.Get<MyEntityReferenceComponent>().Unref();
                    }
                    key.RangeChanged -= this.m_voxelMap_RangeChangedDelegate;
                }
            }
            this.m_voxelMaps.Clear();
            this.m_voxelMapsToAdd.Clear();
        }

        private void VoxelMap_RangeChanged(MyVoxelBase voxel, Vector3I minVoxelChanged, Vector3I maxVoxelChanged, MyStorageDataTypeFlags changedData)
        {
            this.RemoveVoxelMap((MyVoxelMap) voxel);
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct VoxelMapInfo
        {
            public string Name;
            public MyDefinitionId Storage;
            public MatrixD Matrix;
            public int Item;
            public MyStringHash Modifier;
            public long EntityId;
        }
    }
}

