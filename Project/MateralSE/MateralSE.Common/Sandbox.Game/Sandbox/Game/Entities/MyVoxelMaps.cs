namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.World;
    using Sandbox.Game.World.Generator;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.Game.Voxels;
    using VRage.ModAPI;
    using VRageMath;

    public class MyVoxelMaps : IMyVoxelMaps
    {
        private static MyShapeBox m_boxVoxelShape = new MyShapeBox();
        private static MyShapeCapsule m_capsuleShape = new MyShapeCapsule();
        private static MyShapeSphere m_sphereShape = new MyShapeSphere();
        private static MyShapeRamp m_rampShape = new MyShapeRamp();
        private static readonly List<MyVoxelBase> m_voxelsTmpStorage = new List<MyVoxelBase>();
        private readonly Dictionary<uint, MyRenderComponentVoxelMap> m_renderComponentsByClipmapId = new Dictionary<uint, MyRenderComponentVoxelMap>();
        private readonly Dictionary<long, MyVoxelBase> m_voxelMapsByEntityId = new Dictionary<long, MyVoxelBase>();
        private readonly List<MyVoxelBase> m_tmpVoxelMapsList = new List<MyVoxelBase>();

        public void Add(MyVoxelBase voxelMap)
        {
            if (!this.Exist(voxelMap))
            {
                this.m_voxelMapsByEntityId.Add(voxelMap.EntityId, voxelMap);
                MyRenderComponentBase render = voxelMap.Render;
                if (render is MyRenderComponentVoxelMap)
                {
                    uint clipmapId = (render as MyRenderComponentVoxelMap).ClipmapId;
                    this.m_renderComponentsByClipmapId[clipmapId] = render as MyRenderComponentVoxelMap;
                }
            }
        }

        public void Clear()
        {
            foreach (KeyValuePair<long, MyVoxelBase> pair in this.m_voxelMapsByEntityId)
            {
                pair.Value.Close();
            }
            MyStorageBase.ResetCache();
            this.m_voxelMapsByEntityId.Clear();
            this.m_renderComponentsByClipmapId.Clear();
        }

        public void DebugDraw(MyVoxelDebugDrawMode drawMode)
        {
            foreach (MyVoxelBase base2 in this.m_voxelMapsByEntityId.Values)
            {
                if (!(base2 is MyVoxelPhysics))
                {
                    MatrixD worldMatrix = base2.WorldMatrix;
                    worldMatrix.Translation = base2.PositionLeftBottomCorner;
                    base2.Storage.DebugDraw(ref worldMatrix, drawMode);
                }
            }
        }

        public bool Exist(MyVoxelBase voxelMap) => 
            this.m_voxelMapsByEntityId.ContainsKey(voxelMap.EntityId);

        internal void GetAllIds(ref List<long> list)
        {
            foreach (long num in this.m_voxelMapsByEntityId.Keys)
            {
                list.Add(num);
            }
        }

        public List<MyVoxelBase> GetAllOverlappingWithSphere(ref BoundingSphereD sphere)
        {
            List<MyVoxelBase> result = new List<MyVoxelBase>();
            MyGamePruningStructure.GetAllVoxelMapsInSphere(ref sphere, result);
            return result;
        }

        public void GetAllOverlappingWithSphere(ref BoundingSphereD sphere, List<MyVoxelBase> voxels)
        {
            MyGamePruningStructure.GetAllVoxelMapsInSphere(ref sphere, voxels);
        }

        public void GetCacheStats(out int cachedChuncks, out int pendingCachedChuncks)
        {
            cachedChuncks = pendingCachedChuncks = 0;
            foreach (KeyValuePair<long, MyVoxelBase> pair in this.m_voxelMapsByEntityId)
            {
                if (pair.Value is MyVoxelPhysics)
                {
                    continue;
                }
                MyOctreeStorage storage = pair.Value.Storage as MyOctreeStorage;
                if (storage != null)
                {
                    cachedChuncks += storage.CachedChunksCount;
                    pendingCachedChuncks += storage.PendingCachedChunksCount;
                }
            }
        }

        public MyVoxelBase GetOverlappingWithSphere(ref BoundingSphereD sphere)
        {
            MyVoxelBase base2 = null;
            MyGamePruningStructure.GetAllVoxelMapsInSphere(ref sphere, this.m_tmpVoxelMapsList);
            foreach (MyVoxelBase base3 in this.m_tmpVoxelMapsList)
            {
                if (base3.DoOverlapSphereTest((float) sphere.Radius, sphere.Center))
                {
                    base2 = base3;
                    break;
                }
            }
            this.m_tmpVoxelMapsList.Clear();
            return base2;
        }

        public Dictionary<string, byte[]> GetVoxelMapsArray(bool includeChanged)
        {
            Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>();
            foreach (MyVoxelBase base2 in this.m_voxelMapsByEntityId.Values)
            {
                if (base2.Storage != null)
                {
                    if (!includeChanged)
                    {
                        if (base2.ContentChanged)
                        {
                            continue;
                        }
                        if (base2.BeforeContentChanged)
                        {
                            continue;
                        }
                    }
                    if (base2.Save && !dictionary.ContainsKey(base2.StorageName))
                    {
                        byte[] outCompressedData = null;
                        base2.Storage.Save(out outCompressedData);
                        dictionary.Add(base2.StorageName, outCompressedData);
                    }
                }
            }
            return dictionary;
        }

        public Dictionary<string, byte[]> GetVoxelMapsData(bool includeChanged, bool cached, Dictionary<string, VRage.Game.Voxels.IMyStorage> voxelStorageNameCache = null)
        {
            Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>();
            foreach (MyVoxelBase base2 in this.m_voxelMapsByEntityId.Values)
            {
                if (base2.Storage != null)
                {
                    if (!includeChanged)
                    {
                        if (base2.ContentChanged)
                        {
                            continue;
                        }
                        if (base2.BeforeContentChanged)
                        {
                            continue;
                        }
                    }
                    if ((base2.Save && !dictionary.ContainsKey(base2.StorageName)) && (base2.Storage.AreCompressedDataCached == cached))
                    {
                        byte[] outCompressedData = null;
                        if (cached)
                        {
                            base2.Storage.Save(out outCompressedData);
                        }
                        else
                        {
                            outCompressedData = base2.Storage.GetVoxelData();
                        }
                        dictionary.Add(base2.StorageName, outCompressedData);
                        if (voxelStorageNameCache != null)
                        {
                            voxelStorageNameCache.Add(base2.StorageName, base2.Storage);
                        }
                    }
                }
            }
            return dictionary;
        }

        public MyVoxelBase GetVoxelMapWhoseBoundingBoxIntersectsBox(ref BoundingBoxD boundingBox, MyVoxelBase ignoreVoxelMap)
        {
            MyVoxelBase base2 = null;
            double maxValue = double.MaxValue;
            foreach (MyVoxelBase base3 in this.m_voxelMapsByEntityId.Values)
            {
                if (base3.MarkedForClose)
                {
                    continue;
                }
                if (!base3.Closed && (!ReferenceEquals(base3, ignoreVoxelMap) && base3.IsBoxIntersectingBoundingBoxOfThisVoxelMap(ref boundingBox)))
                {
                    double num2 = Vector3D.DistanceSquared(base3.PositionComp.WorldAABB.Center, boundingBox.Center);
                    if (num2 < maxValue)
                    {
                        maxValue = num2;
                        base2 = base3;
                    }
                }
            }
            return base2;
        }

        public void RemoveVoxelMap(MyVoxelBase voxelMap)
        {
            if (this.m_voxelMapsByEntityId.Remove(voxelMap.EntityId))
            {
                MyRenderComponentBase render = voxelMap.Render;
                if (render is MyRenderComponentVoxelMap)
                {
                    uint clipmapId = (render as MyRenderComponentVoxelMap).ClipmapId;
                    this.m_renderComponentsByClipmapId.Remove(clipmapId);
                }
            }
        }

        internal bool TryGetRenderComponent(uint clipmapId, out MyRenderComponentVoxelMap render) => 
            this.m_renderComponentsByClipmapId.TryGetValue(clipmapId, out render);

        public MyVoxelBase TryGetVoxelBaseById(long id) => 
            (this.m_voxelMapsByEntityId.ContainsKey(id) ? this.m_voxelMapsByEntityId[id] : null);

        public MyVoxelBase TryGetVoxelMapByName(string name)
        {
            using (Dictionary<long, MyVoxelBase>.ValueCollection.Enumerator enumerator = this.m_voxelMapsByEntityId.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyVoxelBase current = enumerator.Current;
                    if (current.StorageName == name)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public MyVoxelBase TryGetVoxelMapByNameStart(string name)
        {
            using (Dictionary<long, MyVoxelBase>.ValueCollection.Enumerator enumerator = this.m_voxelMapsByEntityId.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyVoxelBase current = enumerator.Current;
                    if ((current.StorageName != null) && current.StorageName.StartsWith(name))
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        void IMyVoxelMaps.Clear()
        {
            this.Clear();
        }

        VRage.ModAPI.IMyStorage IMyVoxelMaps.CreateStorage(Vector3I size) => 
            new MyOctreeStorage(null, size);

        VRage.ModAPI.IMyStorage IMyVoxelMaps.CreateStorage(byte[] data) => 
            MyStorageBase.Load(data);

        IMyVoxelMap IMyVoxelMaps.CreateVoxelMap(string storageName, VRage.ModAPI.IMyStorage storage, Vector3D position, long voxelMapId)
        {
            MyVoxelMap entity = new MyVoxelMap();
            entity.EntityId = voxelMapId;
            entity.Init(storageName, storage as VRage.Game.Voxels.IMyStorage, position);
            MyEntities.Add(entity, true);
            return entity;
        }

        IMyVoxelMap IMyVoxelMaps.CreateVoxelMapFromStorageName(string storageName, string prefabVoxelMapName, Vector3D position)
        {
            MyStorageBase storage = MyStorageBase.LoadFromFile(MyWorldGenerator.GetVoxelPrefabPath(prefabVoxelMapName), null, true);
            if (storage == null)
            {
                return null;
            }
            int? generator = null;
            storage.DataProvider = MyCompositeShapeProvider.CreateAsteroidShape(0, storage.Size.AbsMax() * 1f, 0, generator);
            return MyWorldGenerator.AddVoxelMap(storageName, storage, position, 0L);
        }

        void IMyVoxelMaps.CutOutShape(IMyVoxelBase voxelMap, IMyVoxelShape voxelShape)
        {
            MyVoxelGenerator.RequestCutOutShape(voxelMap, voxelShape);
        }

        bool IMyVoxelMaps.Exist(IMyVoxelBase voxelMap) => 
            this.Exist(voxelMap as MyVoxelBase);

        void IMyVoxelMaps.FillInShape(IMyVoxelBase voxelMap, IMyVoxelShape voxelShape, byte materialIdx)
        {
            MyVoxelGenerator.RequestFillInShape(voxelMap, voxelShape, materialIdx);
        }

        IMyVoxelShapeBox IMyVoxelMaps.GetBoxVoxelHand() => 
            m_boxVoxelShape;

        IMyVoxelShapeCapsule IMyVoxelMaps.GetCapsuleVoxelHand() => 
            m_capsuleShape;

        void IMyVoxelMaps.GetInstances(List<IMyVoxelBase> voxelMaps, Func<IMyVoxelBase, bool> collect)
        {
            foreach (MyVoxelBase base2 in this.Instances)
            {
                if ((collect == null) || collect(base2))
                {
                    voxelMaps.Add(base2);
                }
            }
        }

        IMyVoxelBase IMyVoxelMaps.GetOverlappingWithSphere(ref BoundingSphereD sphere)
        {
            m_voxelsTmpStorage.Clear();
            this.GetAllOverlappingWithSphere(ref sphere, m_voxelsTmpStorage);
            return ((m_voxelsTmpStorage.Count != 0) ? ((IMyVoxelBase) m_voxelsTmpStorage[0]) : null);
        }

        IMyVoxelShapeRamp IMyVoxelMaps.GetRampVoxelHand() => 
            m_rampShape;

        IMyVoxelShapeSphere IMyVoxelMaps.GetSphereVoxelHand() => 
            m_sphereShape;

        IMyVoxelBase IMyVoxelMaps.GetVoxelMapWhoseBoundingBoxIntersectsBox(ref BoundingBoxD boundingBox, IMyVoxelBase ignoreVoxelMap) => 
            this.GetVoxelMapWhoseBoundingBoxIntersectsBox(ref boundingBox, ignoreVoxelMap as MyVoxelBase);

        void IMyVoxelMaps.MakeCrater(IMyVoxelBase voxelMap, BoundingSphereD sphere, Vector3 direction, byte materialIdx)
        {
            MyVoxelMaterialDefinition voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition(materialIdx);
            MyVoxelGenerator.MakeCrater((MyVoxelBase) voxelMap, sphere, direction, voxelMaterialDefinition);
        }

        void IMyVoxelMaps.PaintInShape(IMyVoxelBase voxelMap, IMyVoxelShape voxelShape, byte materialIdx)
        {
            MyVoxelGenerator.RequestPaintInShape(voxelMap, voxelShape, materialIdx);
        }

        void IMyVoxelMaps.RevertShape(IMyVoxelBase voxelMap, IMyVoxelShape voxelShape)
        {
            MyVoxelGenerator.RequestRevertShape(voxelMap, voxelShape);
        }

        int IMyVoxelMaps.VoxelMaterialCount =>
            MyDefinitionManager.Static.VoxelMaterialCount;

        public DictionaryValuesReader<long, MyVoxelBase> Instances =>
            this.m_voxelMapsByEntityId;
    }
}

