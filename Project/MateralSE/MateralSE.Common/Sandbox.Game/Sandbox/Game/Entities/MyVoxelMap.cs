namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Components;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Voxels;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_VoxelMap), true)]
    public class MyVoxelMap : MyVoxelBase, IMyVoxelMap, IMyVoxelBase, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity
    {
        public MyVoxelMap()
        {
            ((MyPositionComponent) base.PositionComp).WorldPositionChanged = new Action<object>(this.WorldPositionChanged);
            base.Render = new MyRenderComponentVoxelMap();
            base.Render.DrawOutsideViewDistance = true;
            base.AddDebugRenderComponent(new MyDebugRenderComponentVoxelMap(this));
        }

        protected override void BeforeDelete()
        {
            base.BeforeDelete();
            base.m_storage = null;
            MySession.Static.VoxelMaps.RemoveVoxelMap(this);
        }

        public override string GetFriendlyName() => 
            "MyVoxelMap";

        public override bool GetIntersectionWithAABB(ref BoundingBoxD aabb)
        {
            bool flag;
            try
            {
                if (!base.PositionComp.WorldAABB.Intersects(ref aabb))
                {
                    flag = false;
                }
                else
                {
                    BoundingSphere localSphere = new BoundingSphere((Vector3) (aabb.Center - this.PositionLeftBottomCorner), (float) aabb.HalfExtents.Length());
                    flag = this.Storage.GetGeometry().Intersects(ref localSphere);
                }
            }
            finally
            {
            }
            return flag;
        }

        public override bool GetIntersectionWithSphere(ref BoundingSphereD sphere)
        {
            bool flag;
            try
            {
                if (!base.PositionComp.WorldAABB.Intersects(ref sphere))
                {
                    flag = false;
                }
                else
                {
                    BoundingSphere localSphere = new BoundingSphere((Vector3) (sphere.Center - this.PositionLeftBottomCorner), (float) sphere.Radius);
                    flag = this.Storage.GetGeometry().Intersects(ref localSphere);
                }
            }
            finally
            {
            }
            return flag;
        }

        public static string GetNewStorageName(string storageName, long entityId) => 
            $"{storageName}-{entityId}";

        public override int GetOrePriority() => 
            1;

        public override void Init(MyObjectBuilder_EntityBase builder)
        {
            MyObjectBuilder_VoxelMap map = (MyObjectBuilder_VoxelMap) builder;
            if (map != null)
            {
                base.m_storage = MyStorageBase.Load(map.StorageName, false);
                if (base.m_storage != null)
                {
                    this.Init(builder, base.m_storage);
                    if (map.ContentChanged != null)
                    {
                        base.ContentChanged = map.ContentChanged.Value;
                    }
                    else
                    {
                        base.ContentChanged = true;
                    }
                }
            }
        }

        public override void Init(MyObjectBuilder_EntityBase builder, VRage.Game.Voxels.IMyStorage storage)
        {
            base.SyncFlag = true;
            base.Init(builder);
            float? scale = null;
            base.Init(null, null, null, scale, null);
            MyObjectBuilder_VoxelMap map = (MyObjectBuilder_VoxelMap) builder;
            if (map != null)
            {
                base.StorageName = !map.MutableStorage ? GetNewStorageName(map.StorageName, base.EntityId) : map.StorageName;
                base.m_storage = storage;
                base.m_storage.RangeChanged += new Action<Vector3I, Vector3I, MyStorageDataTypeFlags>(this.storage_RangeChanged);
                base.m_storageMax = base.m_storage.Size;
                this.InitVoxelMap(MatrixD.CreateWorld(((Vector3D) map.PositionAndOrientation.Value.Position) + Vector3D.TransformNormal((Vector3D) (base.m_storage.Size / 2.0), base.WorldMatrix), base.WorldMatrix.Forward, base.WorldMatrix.Up), base.m_storage.Size, true);
            }
        }

        public override void Init(string storageName, VRage.Game.Voxels.IMyStorage storage, MatrixD worldMatrix, bool useVoxelOffset = true)
        {
            base.m_storageMax = storage.Size;
            base.Init(storageName, storage, worldMatrix, useVoxelOffset);
            base.m_storage.RangeChanged += new Action<Vector3I, Vector3I, MyStorageDataTypeFlags>(this.storage_RangeChanged);
        }

        protected override void InitVoxelMap(MatrixD worldMatrix, Vector3I size, bool useOffset = true)
        {
            base.InitVoxelMap(worldMatrix, size, useOffset);
            ((MyStorageBase) this.Storage).InitWriteCache(8);
            this.Physics = new MyVoxelPhysicsBody(this, 3f, 3f, base.DelayRigidBodyCreation);
            this.Physics.Enabled = true;
        }

        public override unsafe bool IsOverlapOverThreshold(BoundingBoxD worldAabb, float thresholdPercentage)
        {
            Vector3I vectori;
            Vector3I vectori2;
            Vector3I vectori3;
            Vector3I vectori4;
            if (base.m_storage == null)
            {
                if (!ReferenceEquals(MyEntities.GetEntityByIdOrDefault(base.EntityId, null, false), this))
                {
                    MyDebug.FailRelease("Voxel map was deleted!");
                }
                else
                {
                    MyDebug.FailRelease("Voxel map is still in world but has null storage!");
                }
                return false;
            }
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref worldAabb.Min, out vectori);
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref worldAabb.Max, out vectori2);
            vectori = (Vector3I) (vectori + base.StorageMin);
            vectori2 = (Vector3I) (vectori2 + base.StorageMin);
            this.Storage.ClampVoxelCoord(ref vectori, 1);
            this.Storage.ClampVoxelCoord(ref vectori2, 1);
            if (MyVoxelBase.m_tempStorage == null)
            {
                MyVoxelBase.m_tempStorage = new MyStorageData(MyStorageDataTypeFlags.All);
            }
            MyVoxelBase.m_tempStorage.Resize(vectori, vectori2);
            this.Storage.ReadRange(MyVoxelBase.m_tempStorage, MyStorageDataTypeFlags.Content, 0, vectori, vectori2);
            double num = 0.00392156862745098;
            double num2 = 1.0;
            double num3 = 0.0;
            double volume = worldAabb.Volume;
            vectori3.Z = vectori.Z;
            vectori4.Z = 0;
            while (vectori3.Z <= vectori2.Z)
            {
                vectori3.Y = vectori.Y;
                vectori4.Y = 0;
                while (true)
                {
                    if (vectori3.Y > vectori2.Y)
                    {
                        int* numPtr5 = (int*) ref vectori3.Z;
                        numPtr5[0]++;
                        int* numPtr6 = (int*) ref vectori4.Z;
                        numPtr6[0]++;
                        break;
                    }
                    vectori3.X = vectori.X;
                    vectori4.X = 0;
                    while (true)
                    {
                        BoundingBoxD xd;
                        if (vectori3.X > vectori2.X)
                        {
                            int* numPtr3 = (int*) ref vectori3.Y;
                            numPtr3[0]++;
                            int* numPtr4 = (int*) ref vectori4.Y;
                            numPtr4[0]++;
                            break;
                        }
                        MyVoxelCoordSystems.VoxelCoordToWorldAABB(this.PositionLeftBottomCorner, ref vectori3, out xd);
                        if (worldAabb.Intersects(xd))
                        {
                            num3 += ((MyVoxelBase.m_tempStorage.Content(ref vectori4) * num) * num2) * worldAabb.Intersect(xd).Volume;
                        }
                        int* numPtr1 = (int*) ref vectori3.X;
                        numPtr1[0]++;
                        int* numPtr2 = (int*) ref vectori4.X;
                        numPtr2[0]++;
                    }
                }
            }
            return ((num3 / volume) >= thresholdPercentage);
        }

        private void storage_RangeChanged(Vector3I minChanged, Vector3I maxChanged, MyStorageDataTypeFlags dataChanged)
        {
            if (((dataChanged & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None) && (this.Physics != null))
            {
                this.Physics.InvalidateRange(minChanged, maxChanged);
            }
            if (base.Render is MyRenderComponentVoxelMap)
            {
                (base.Render as MyRenderComponentVoxelMap).InvalidateRange(minChanged, maxChanged);
            }
            base.OnRangeChanged(minChanged, maxChanged, dataChanged);
            base.ContentChanged = true;
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if (this.Physics != null)
            {
                this.Physics.UpdateAfterSimulation10();
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (this.Physics != null)
            {
                this.Physics.UpdateBeforeSimulation10();
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
        }

        void IMyVoxelMap.ClampVoxelCoord(ref Vector3I voxelCoord)
        {
            this.Storage.ClampVoxelCoord(ref voxelCoord, 1);
        }

        void IMyVoxelMap.Close()
        {
            base.Close();
        }

        bool IMyVoxelMap.DoOverlapSphereTest(float sphereRadius, Vector3D spherePos) => 
            this.DoOverlapSphereTest(sphereRadius, spherePos);

        bool IMyVoxelMap.GetIntersectionWithSphere(ref BoundingSphereD sphere) => 
            this.GetIntersectionWithSphere(ref sphere);

        MyObjectBuilder_EntityBase IMyVoxelMap.GetObjectBuilder(bool copy) => 
            this.GetObjectBuilder(copy);

        float IMyVoxelMap.GetVoxelContentInBoundingBox(BoundingBoxD worldAabb, out float cellCount)
        {
            cellCount = 0f;
            return 0f;
        }

        Vector3I IMyVoxelMap.GetVoxelCoordinateFromMeters(Vector3D pos)
        {
            Vector3I vectori;
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref pos, out vectori);
            return vectori;
        }

        void IMyVoxelMap.Init(MyObjectBuilder_EntityBase builder)
        {
            this.Init(builder);
        }

        public override VRage.Game.Voxels.IMyStorage Storage
        {
            get => 
                base.m_storage;
            set
            {
                if (base.m_storage != null)
                {
                    base.m_storage.RangeChanged -= new Action<Vector3I, Vector3I, MyStorageDataTypeFlags>(this.storage_RangeChanged);
                }
                base.m_storage = value;
                base.m_storage.RangeChanged += new Action<Vector3I, Vector3I, MyStorageDataTypeFlags>(this.storage_RangeChanged);
                base.m_storageMax = base.m_storage.Size;
            }
        }

        internal MyVoxelPhysicsBody Physics
        {
            get => 
                (base.Physics as MyVoxelPhysicsBody);
            set => 
                (base.Physics = value);
        }

        public override MyVoxelBase RootVoxel =>
            this;

        public bool IsStaticForCluster
        {
            get => 
                this.Physics.IsStaticForCluster;
            set => 
                (this.Physics.IsStaticForCluster = value);
        }
    }
}

