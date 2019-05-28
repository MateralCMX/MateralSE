namespace Sandbox.Game.Entities
{
    using ParallelTasks;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
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
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.Game.Voxels;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    public abstract class MyVoxelBase : MyEntity, IMyVoxelDrawable, IMyVoxelBase, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyDecalProxy, IMyEventProxy, IMyEventOwner
    {
        public int VoxelMapPruningProxyId = -1;
        protected Vector3I m_storageMin = new Vector3I(0, 0, 0);
        protected Vector3I m_storageMax;
        private VRage.Game.Voxels.IMyStorage m_storageInternal;
        public bool CreateStorageCopyOnWrite;
        private bool m_contentChanged;
        private bool m_beforeContentChanged;
        [ThreadStatic]
        protected static MyStorageData m_tempStorage;
        private static readonly MyShapeSphere m_sphereShape = new MyShapeSphere();
        private static readonly MyShapeBox m_boxShape = new MyShapeBox();
        private static readonly MyShapeRamp m_rampShape = new MyShapeRamp();
        private static readonly MyShapeCapsule m_capsuleShape = new MyShapeCapsule();
        private static readonly MyShapeEllipsoid m_ellipsoidShape = new MyShapeEllipsoid();
        private static readonly List<MyEntity> m_foundElements = new List<MyEntity>();
        [CompilerGenerated]
        private StorageChanged RangeChanged;
        private bool m_voxelShapeInProgress;

        public event StorageChanged RangeChanged
        {
            [CompilerGenerated] add
            {
                StorageChanged rangeChanged = this.RangeChanged;
                while (true)
                {
                    StorageChanged a = rangeChanged;
                    StorageChanged changed3 = (StorageChanged) Delegate.Combine(a, value);
                    rangeChanged = Interlocked.CompareExchange<StorageChanged>(ref this.RangeChanged, changed3, a);
                    if (ReferenceEquals(rangeChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                StorageChanged rangeChanged = this.RangeChanged;
                while (true)
                {
                    StorageChanged source = rangeChanged;
                    StorageChanged changed3 = (StorageChanged) Delegate.Remove(source, value);
                    rangeChanged = Interlocked.CompareExchange<StorageChanged>(ref this.RangeChanged, changed3, source);
                    if (ReferenceEquals(rangeChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyVoxelBase()
        {
            this.VoxelSize = 1f;
        }

        public bool AreAllAabbCornersInside(ref MatrixD aabbWorldTransform, BoundingBoxD aabb) => 
            (this.CountCornersInside(ref aabbWorldTransform, ref aabb) == 8);

        protected override void BeforeDelete()
        {
            base.BeforeDelete();
            this.RangeChanged = null;
            if (((this.Storage != null) && !this.Storage.Shared) && !(this is MyVoxelPhysics))
            {
                this.Storage.Close();
            }
        }

        private static bool CanPlaceInArea(OperationType type, MyShape Shape)
        {
            if ((type == OperationType.Fill) || (type == OperationType.Revert))
            {
                m_foundElements.Clear();
                BoundingBoxD worldBoundaries = Shape.GetWorldBoundaries();
                MyEntities.GetElementsInBox(ref worldBoundaries, m_foundElements);
                using (List<MyEntity>.Enumerator enumerator = m_foundElements.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyEntity current = enumerator.Current;
                        if (IsForbiddenEntity(current) && current.PositionComp.WorldAABB.Intersects(worldBoundaries))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private unsafe int CountCornersInside(ref MatrixD aabbWorldTransform, ref BoundingBoxD aabb)
        {
            Vector3D* corners = (Vector3D*) stackalloc byte[(((IntPtr) 8) * sizeof(Vector3D))];
            aabb.GetCornersUnsafe(corners);
            for (int i = 0; i < 8; i++)
            {
                Vector3D.Transform(ref (Vector3D) ref (corners + i), ref aabbWorldTransform, out (Vector3D) ref (corners + i));
            }
            return this.CountPointsInside(corners, 8);
        }

        public unsafe int CountPointsInside(Vector3D* worldPoints, int pointCount)
        {
            if (m_tempStorage == null)
            {
                m_tempStorage = new MyStorageData(MyStorageDataTypeFlags.All);
            }
            MatrixD worldMatrixInvScaled = base.PositionComp.WorldMatrixInvScaled;
            int num = 0;
            Vector3I vectori = new Vector3I(0x7fffffff);
            Vector3I vectori2 = new Vector3I(-2147483648);
            for (int i = 0; i < pointCount; i++)
            {
                Vector3D vectord;
                Vector3D.Transform(ref (Vector3D) ref (worldPoints + i), ref worldMatrixInvScaled, out vectord);
                Vector3D vectord2 = vectord + (this.Size / 2);
                Vector3I start = Vector3D.Floor(vectord2);
                Vector3D* vectordPtr1 = (Vector3D*) ref vectord2;
                Vector3D.Fract(ref (Vector3D) ref vectordPtr1, out vectord2);
                start = (Vector3I) (start + this.StorageMin);
                Vector3I end = (Vector3I) (start + 1);
                if ((start != vectori) && (end != vectori2))
                {
                    m_tempStorage.Resize(start, end);
                    this.Storage.ReadRange(m_tempStorage, MyStorageDataTypeFlags.Content, 0, start, end);
                    vectori = start;
                    vectori2 = end;
                }
                double num3 = m_tempStorage.Content(0, 0, 0);
                double num5 = m_tempStorage.Content(0, 1, 0);
                double num7 = m_tempStorage.Content(0, 0, 1);
                double num9 = m_tempStorage.Content(0, 1, 1);
                num3 += (m_tempStorage.Content(1, 0, 0) - num3) * vectord2.X;
                num7 += (m_tempStorage.Content(1, 0, 1) - num7) * vectord2.X;
                num3 += ((num5 + ((m_tempStorage.Content(1, 1, 0) - num5) * vectord2.X)) - num3) * vectord2.Y;
                if ((num3 + (((num7 + (((num9 + ((m_tempStorage.Content(1, 1, 1) - num9) * vectord2.X)) - num7) * vectord2.Y)) - num3) * vectord2.Z)) >= 127.0)
                {
                    num++;
                }
            }
            return num;
        }

        public void CreateVoxelMeteorCrater(Vector3D center, float radius, Vector3 direction, MyVoxelMaterialDefinition material)
        {
            this.BeforeContentChanged = true;
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyVoxelBase, Vector3D, float, Vector3, byte>(this.RootVoxel, x => new Action<Vector3D, float, Vector3, byte>(x.CreateVoxelMeteorCrater_Implementation), center, radius, direction, material.Index, targetEndpoint);
        }

        [Event(null, 0x47c), Reliable, Broadcast]
        private void CreateVoxelMeteorCrater_Implementation(Vector3D center, float radius, Vector3 direction, byte material)
        {
            this.BeforeContentChanged = true;
            MyVoxelGenerator.MakeCrater(this, new BoundingSphere((Vector3) center, radius), direction, MyDefinitionManager.Static.GetVoxelMaterialDefinition(material));
        }

        public void CutOutShapeWithProperties(MyShape shape, out float voxelsCountInPercent, out MyVoxelMaterialDefinition voxelMaterial, Dictionary<MyVoxelMaterialDefinition, int> exactCutOutMaterials = null, bool updateSync = false, bool onlyCheck = false, bool applyDamageMaterial = false, bool onlyApplyMaterial = false)
        {
            this.BeforeContentChanged = true;
            MyVoxelGenerator.CutOutShapeWithProperties(this, shape, out voxelsCountInPercent, out voxelMaterial, exactCutOutMaterials, updateSync, onlyCheck, applyDamageMaterial, onlyApplyMaterial, false);
        }

        public void CutOutShapeWithPropertiesAsync(OnCutOutResults results, MyShape shape, bool updateSync = false, bool onlyCheck = false, bool applyDamageMaterial = false, bool onlyApplyMaterial = false, bool skipCache = true)
        {
            this.BeforeContentChanged = true;
            float voxelsCountInPercent = 0f;
            MyVoxelMaterialDefinition voxelMaterial = null;
            Dictionary<MyVoxelMaterialDefinition, int> exactCutOutMaterials = new Dictionary<MyVoxelMaterialDefinition, int>();
            Parallel.Start(delegate {
                using (this.Pin())
                {
                    if (!this.MarkedForClose)
                    {
                        MyVoxelGenerator.CutOutShapeWithProperties(this, shape, out voxelsCountInPercent, out voxelMaterial, exactCutOutMaterials, updateSync, onlyCheck, applyDamageMaterial, onlyApplyMaterial, skipCache);
                    }
                }
            }, delegate {
                if (results != null)
                {
                    results(voxelsCountInPercent, voxelMaterial, exactCutOutMaterials);
                }
            });
        }

        public override bool DoOverlapSphereTest(float sphereRadius, Vector3D spherePos)
        {
            if (this.Storage.Closed)
            {
                return false;
            }
            Vector3D vectord1 = Vector3D.Transform(spherePos, base.PositionComp.WorldMatrixInvScaled);
            spherePos = vectord1;
            spherePos /= (double) this.VoxelSize;
            sphereRadius /= this.VoxelSize;
            spherePos += this.SizeInMetresHalf;
            return this.OverlapsSphereLocal(sphereRadius, spherePos);
        }

        public virtual Vector3D FindOutsidePosition(Vector3D localPosition, float radius)
        {
            Vector3D randomPerpendicularVector;
            Vector3D vectord3;
            Vector3D normal = MyGravityProviderSystem.CalculateTotalGravityInPoint(Vector3D.Transform(localPosition - this.SizeInMetresHalf, base.WorldMatrix));
            if (normal.LengthSquared() <= 0.01)
            {
                randomPerpendicularVector = localPosition - this.SizeInMetresHalf;
                randomPerpendicularVector.Normalize();
            }
            else
            {
                normal = Vector3D.TransformNormal(normal, base.PositionComp.WorldMatrixNormalizedInv);
                normal.Normalize();
                randomPerpendicularVector = MyUtils.GetRandomPerpendicularVector(ref normal);
            }
            for (double i = radius; this.OverlapsSphereLocal(radius, vectord3 = localPosition + (randomPerpendicularVector * i)); i *= 2.0)
            {
            }
            return vectord3;
        }

        public bool GetContainedVoxelCoords(ref BoundingBoxD worldAabb, out Vector3I min, out Vector3I max)
        {
            min = new Vector3I();
            max = new Vector3I();
            if (!this.IsBoxIntersectingBoundingBoxOfThisVoxelMap(ref worldAabb))
            {
                return false;
            }
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref worldAabb.Min, out min);
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref worldAabb.Max, out max);
            min = (Vector3I) (min + this.StorageMin);
            max = (Vector3I) (max + this.StorageMin);
            this.Storage.ClampVoxelCoord(ref min, 1);
            this.Storage.ClampVoxelCoord(ref max, 1);
            return true;
        }

        public void GetFilledStorageBounds(out Vector3I min, out Vector3I max)
        {
            min = Vector3I.MaxValue;
            max = Vector3I.MinValue;
            Vector3I size = this.Size;
            Vector3I lodVoxelRangeMax = this.Size - 1;
            if (m_tempStorage == null)
            {
                m_tempStorage = new MyStorageData(MyStorageDataTypeFlags.All);
            }
            m_tempStorage.Resize(this.Size);
            this.Storage.ReadRange(m_tempStorage, MyStorageDataTypeFlags.Content, 0, Vector3I.Zero, lodVoxelRangeMax);
            int z = 0;
            while (z < size.Z)
            {
                int y = 0;
                while (true)
                {
                    if (y >= size.Y)
                    {
                        z++;
                        break;
                    }
                    int x = 0;
                    while (true)
                    {
                        if (x >= size.X)
                        {
                            y++;
                            break;
                        }
                        if (m_tempStorage.Content(x, y, z) > 0x7f)
                        {
                            Vector3I vectori3 = Vector3I.Max(new Vector3I(x - 1, y - 1, z - 1), Vector3I.Zero);
                            min = Vector3I.Min(vectori3, min);
                            Vector3I vectori4 = Vector3I.Min(new Vector3I(x + 1, y + 1, z + 1), lodVoxelRangeMax);
                            max = Vector3I.Max(vectori4, max);
                        }
                        x++;
                    }
                }
            }
        }

        public override bool GetIntersectionWithLine(ref LineD worldLine, out MyIntersectionResultLineTriangleEx? t, IntersectionFlags flags = 3)
        {
            double num;
            bool flag;
            t = 0;
            if (!base.PositionComp.WorldAABB.Intersects(ref worldLine, out num))
            {
                return false;
            }
            try
            {
                MyIntersectionResultLineTriangle triangle;
                Line localLine = new Line((Vector3) (worldLine.From - this.PositionLeftBottomCorner), (Vector3) (worldLine.To - this.PositionLeftBottomCorner), true);
                if (!this.Storage.GetGeometry().Intersect(ref localLine, out triangle, flags))
                {
                    t = 0;
                    flag = false;
                }
                else
                {
                    t = new MyIntersectionResultLineTriangleEx(triangle, this, ref localLine);
                    MyIntersectionResultLineTriangleEx local1 = t.Value;
                    flag = true;
                }
            }
            finally
            {
            }
            return flag;
        }

        public override bool GetIntersectionWithLine(ref LineD worldLine, out Vector3D? v, bool useCollisionModel = true, IntersectionFlags flags = 3)
        {
            MyIntersectionResultLineTriangleEx? nullable;
            this.GetIntersectionWithLine(ref worldLine, out nullable, IntersectionFlags.ALL_TRIANGLES);
            v = 0;
            if (nullable == null)
            {
                return false;
            }
            v = new Vector3D?(nullable.Value.IntersectionPointInWorldSpace);
            return true;
        }

        public unsafe HashSet<byte> GetMaterialsInShape(MyShape shape, int lod = 0)
        {
            Vector3I vectori;
            Vector3I vectori2;
            Vector3I vectori5;
            BoundingBoxD worldBoundaries = shape.GetWorldBoundaries();
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref worldBoundaries.Min, out vectori);
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref worldBoundaries.Max, out vectori2);
            Vector3I voxelCoord = vectori - 1;
            Vector3I vectori4 = (Vector3I) (vectori2 + 1);
            this.Storage.ClampVoxelCoord(ref voxelCoord, 1);
            this.Storage.ClampVoxelCoord(ref vectori4, 1);
            voxelCoord = (voxelCoord >> lod) - 1;
            vectori4 = (Vector3I) ((vectori4 >> lod) + 1);
            if (m_tempStorage == null)
            {
                m_tempStorage = new MyStorageData(MyStorageDataTypeFlags.All);
            }
            m_tempStorage.Resize(voxelCoord, vectori4);
            using (this.Storage.Pin())
            {
                this.Storage.ReadRange(m_tempStorage, MyStorageDataTypeFlags.Material, lod, voxelCoord, vectori4);
            }
            HashSet<byte> set = new HashSet<byte>();
            vectori5.X = voxelCoord.X;
            while (vectori5.X <= vectori4.X)
            {
                vectori5.Y = voxelCoord.Y;
                while (true)
                {
                    if (vectori5.Y > vectori4.Y)
                    {
                        int* numPtr3 = (int*) ref vectori5.X;
                        numPtr3[0]++;
                        break;
                    }
                    vectori5.Z = voxelCoord.Z;
                    while (true)
                    {
                        if (vectori5.Z > vectori4.Z)
                        {
                            int* numPtr2 = (int*) ref vectori5.Y;
                            numPtr2[0]++;
                            break;
                        }
                        Vector3I p = vectori5 - voxelCoord;
                        int linearIdx = m_tempStorage.ComputeLinear(ref p);
                        byte item = m_tempStorage.Material(linearIdx);
                        if (item != 0xff)
                        {
                            set.Add(item);
                        }
                        int* numPtr1 = (int*) ref vectori5.Z;
                        numPtr1[0]++;
                    }
                }
            }
            return set;
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            Vector3D positionLeftBottomCorner = this.PositionLeftBottomCorner;
            this.PositionLeftBottomCorner = base.WorldMatrix.Translation - Vector3D.TransformNormal(this.SizeInMetresHalf, base.WorldMatrix);
            if (MyPerGameSettings.OffsetVoxelMapByHalfVoxel)
            {
                positionLeftBottomCorner -= 0.5;
            }
            MyObjectBuilder_VoxelMap objectBuilder = (MyObjectBuilder_VoxelMap) base.GetObjectBuilder(copy);
            objectBuilder.PositionAndOrientation = new MyPositionAndOrientation(positionLeftBottomCorner, (Vector3) base.WorldMatrix.Forward, (Vector3) base.WorldMatrix.Up);
            objectBuilder.StorageName = this.StorageName;
            objectBuilder.MutableStorage = true;
            objectBuilder.ContentChanged = new bool?(this.ContentChanged);
            return objectBuilder;
        }

        public virtual int GetOrePriority() => 
            1;

        public unsafe MyTuple<float, float> GetVoxelContentInBoundingBox_Fast(BoundingBoxD localAabb, MatrixD worldMatrix)
        {
            MatrixD xd2;
            Vector3I vectori4;
            Vector3I vectori5;
            MatrixD matrix = worldMatrix * base.PositionComp.WorldMatrixNormalizedInv;
            MatrixD.Invert(ref matrix, out xd2);
            BoundingBoxD xd3 = localAabb.TransformFast(matrix);
            xd3.Translate(this.SizeInMetresHalf + this.StorageMin);
            int lodIndex = Math.Max((MathHelper.Log2Ceiling((int) (localAabb.Volume / 1.0)) - MathHelper.Log2Ceiling(100)) / 3, 0);
            float num2 = 1f * (1 << (lodIndex & 0x1f));
            float num3 = (num2 * num2) * num2;
            Vector3I lodVoxelRangeMin = Vector3I.Floor(xd3.Min) >> lodIndex;
            Vector3I lodVoxelRangeMax = Vector3I.Ceiling((Vector3) xd3.Max) >> lodIndex;
            Vector3I vectori3 = (Vector3I) (((this.Size >> 1) + this.StorageMin) >> lodIndex);
            if (m_tempStorage == null)
            {
                m_tempStorage = new MyStorageData(MyStorageDataTypeFlags.All);
            }
            m_tempStorage.Resize((Vector3I) ((lodVoxelRangeMax - lodVoxelRangeMin) + 1));
            this.Storage.ReadRange(m_tempStorage, MyStorageDataTypeFlags.Content, lodIndex, lodVoxelRangeMin, lodVoxelRangeMax);
            float num4 = 0f;
            float num5 = 0f;
            int num6 = 0;
            MyOrientedBoundingBoxD xd4 = new MyOrientedBoundingBoxD(localAabb, worldMatrix);
            vectori4.Z = lodVoxelRangeMin.Z;
            vectori5.Z = 0;
            while (vectori4.Z <= lodVoxelRangeMax.Z)
            {
                vectori4.Y = lodVoxelRangeMin.Y;
                vectori5.Y = 0;
                while (true)
                {
                    if (vectori4.Y > lodVoxelRangeMax.Y)
                    {
                        int* numPtr5 = (int*) ref vectori4.Z;
                        numPtr5[0]++;
                        int* numPtr6 = (int*) ref vectori5.Z;
                        numPtr6[0]++;
                        break;
                    }
                    vectori4.X = lodVoxelRangeMin.X;
                    vectori5.X = 0;
                    while (true)
                    {
                        Vector3D vectord2;
                        if (vectori4.X > lodVoxelRangeMax.X)
                        {
                            int* numPtr3 = (int*) ref vectori4.Y;
                            numPtr3[0]++;
                            int* numPtr4 = (int*) ref vectori5.Y;
                            numPtr4[0]++;
                            break;
                        }
                        Vector3D position = (Vector3D) ((vectori4 - vectori3) * num2);
                        Vector3D.Transform(ref position, ref xd2, out vectord2);
                        MatrixD transform = base.WorldMatrix;
                        MatrixD* xdPtr1 = (MatrixD*) ref transform;
                        xdPtr1.Translation -= this.StorageMin + this.SizeInMetresHalf;
                        BoundingBoxD box = new BoundingBoxD {
                            Min = (Vector3D) ((vectori4 - 0.5) * num2),
                            Max = (vectori4 + 0.5) * num2
                        };
                        MyOrientedBoundingBoxD other = new MyOrientedBoundingBoxD(box, transform);
                        MyOrientedBoundingBoxD xd1 = new MyOrientedBoundingBoxD(box.GetInflated((double) -0.05000000074505806), transform);
                        if (xd4.Contains(ref other) != ContainmentType.Disjoint)
                        {
                            float num7 = ((float) m_tempStorage.Content(ref vectori5)) / 255f;
                            num4 += num7 * num3;
                            num5 += num7;
                            num6++;
                        }
                        int* numPtr1 = (int*) ref vectori4.X;
                        numPtr1[0]++;
                        int* numPtr2 = (int*) ref vectori5.X;
                        numPtr2[0]++;
                    }
                }
            }
            return new MyTuple<float, float>(num4, num5 / ((float) num6));
        }

        public MyVoxelContentConstitution GetVoxelRangeTypeInBoundingBox(BoundingBoxD worldAabb)
        {
            Vector3I vectori;
            Vector3I vectori2;
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref worldAabb.Min, out vectori);
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(this.PositionLeftBottomCorner, ref worldAabb.Max, out vectori2);
            vectori = (Vector3I) (vectori + this.StorageMin);
            vectori2 = (Vector3I) (vectori2 + this.StorageMin);
            this.Storage.ClampVoxelCoord(ref vectori, 1);
            this.Storage.ClampVoxelCoord(ref vectori2, 1);
            return MyVoxelContentConstitution.Mixed;
        }

        public abstract void Init(MyObjectBuilder_EntityBase builder, VRage.Game.Voxels.IMyStorage storage);
        public void Init(string storageName, VRage.Game.Voxels.IMyStorage storage, Vector3D positionMinCorner)
        {
            MatrixD worldMatrix = MatrixD.CreateTranslation(positionMinCorner + (storage.Size / 2));
            this.Init(storageName, storage, worldMatrix, true);
        }

        public virtual void Init(string storageName, VRage.Game.Voxels.IMyStorage storage, MatrixD worldMatrix, bool useVoxelOffset = true)
        {
            if (base.Name == null)
            {
                base.Init(null);
            }
            this.StorageName = storageName;
            this.m_storage = storage;
            this.InitVoxelMap(worldMatrix, storage.Size, useVoxelOffset);
        }

        protected virtual unsafe void InitVoxelMap(MatrixD worldMatrix, Vector3I size, bool useOffset = true)
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            this.SizeInMetres = (Vector3) (size * 1f);
            this.SizeInMetresHalf = this.SizeInMetres / 2f;
            base.PositionComp.LocalAABB = new BoundingBox(-this.SizeInMetresHalf, this.SizeInMetresHalf);
            if (MyPerGameSettings.OffsetVoxelMapByHalfVoxel & useOffset)
            {
                MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                xdPtr1.Translation += 0.5f;
                this.PositionLeftBottomCorner += 0.5f;
            }
            base.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
            this.ContentChanged = false;
        }

        public ContainmentType IntersectStorage(ref BoundingBox box, bool lazy = true)
        {
            box.Transform(base.PositionComp.WorldMatrixInvScaled);
            box.Translate(this.SizeInMetresHalf + this.StorageMin);
            return this.Storage.Intersect(ref box, lazy);
        }

        public bool IsAnyAabbCornerInside(ref MatrixD aabbWorldTransform, BoundingBoxD aabb) => 
            (this.CountCornersInside(ref aabbWorldTransform, ref aabb) > 0);

        public bool IsBoxIntersectingBoundingBoxOfThisVoxelMap(ref BoundingBoxD boundingBox)
        {
            bool flag;
            base.PositionComp.WorldAABB.Intersects(ref boundingBox, out flag);
            return flag;
        }

        public static bool IsForbiddenEntity(MyEntity entity)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            if (!(entity is MyCharacter) && (((grid == null) || grid.IsStatic) || grid.IsPreview))
            {
                return ((entity is MyCockpit) && ((entity as MyCockpit).Pilot != null));
            }
            return true;
        }

        public virtual bool IsOverlapOverThreshold(BoundingBoxD worldAabb, float thresholdPercentage = 0.9f) => 
            false;

        protected internal void OnRangeChanged(Vector3I voxelRangeMin, Vector3I voxelRangeMax, MyStorageDataTypeFlags changedData)
        {
            if (this.RangeChanged != null)
            {
                this.RangeChanged(this, voxelRangeMin, voxelRangeMax, changedData);
            }
        }

        protected unsafe bool OverlapsSphereLocal(float sphereRadius, Vector3D spherePos)
        {
            double num = sphereRadius * sphereRadius;
            Vector3I voxelCoord = new Vector3I(spherePos - sphereRadius);
            Vector3I vectori2 = new Vector3I(spherePos + sphereRadius);
            this.Storage.ClampVoxelCoord(ref voxelCoord, 1);
            this.Storage.ClampVoxelCoord(ref vectori2, 1);
            BoundingBoxI xi = new BoundingBoxI(voxelCoord, vectori2);
            if (this.Storage.Intersect(ref xi, 0, true) != ContainmentType.Disjoint)
            {
                Vector3I vectori3;
                Vector3I vectori4;
                if (m_tempStorage == null)
                {
                    m_tempStorage = new MyStorageData(MyStorageDataTypeFlags.All);
                }
                m_tempStorage.Resize(voxelCoord, vectori2);
                this.Storage.ReadRange(m_tempStorage, MyStorageDataTypeFlags.Content, 0, voxelCoord, vectori2);
                vectori3.Z = voxelCoord.Z;
                vectori4.Z = 0;
                while (vectori3.Z <= vectori2.Z)
                {
                    vectori3.Y = voxelCoord.Y;
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
                        vectori3.X = voxelCoord.X;
                        vectori4.X = 0;
                        while (true)
                        {
                            if (vectori3.X > vectori2.X)
                            {
                                int* numPtr3 = (int*) ref vectori3.Y;
                                numPtr3[0]++;
                                int* numPtr4 = (int*) ref vectori4.Y;
                                numPtr4[0]++;
                                break;
                            }
                            if (m_tempStorage.Content(ref vectori4) >= 0x7f)
                            {
                                Vector3 max = vectori3 + this.VoxelSize;
                                BoundingBox box = new BoundingBox((Vector3) vectori3, max);
                                if (box.Contains(spherePos) == ContainmentType.Contains)
                                {
                                    return true;
                                }
                                if (Vector3D.DistanceSquared((Vector3D) vectori3, spherePos) < num)
                                {
                                    return true;
                                }
                            }
                            int* numPtr1 = (int*) ref vectori3.X;
                            numPtr1[0]++;
                            int* numPtr2 = (int*) ref vectori4.X;
                            numPtr2[0]++;
                        }
                    }
                }
            }
            return false;
        }

        [Event(null, 0x3eb), Reliable, Broadcast]
        public void PerformCutOutSphereFast(Vector3D center, float radius, bool notify)
        {
            Vector3I vectori;
            Vector3I vectori2;
            MyVoxelGenerator.CutOutSphereFast(this, ref center, radius, out vectori, out vectori2, notify);
        }

        [Event(null, 0x36b), Reliable, Broadcast]
        private void PerformVoxelOperationBox_Implementation(BoundingBoxD box, MatrixD Transformation, byte material, OperationType Type)
        {
            this.BeforeContentChanged = true;
            m_boxShape.Transformation = Transformation;
            m_boxShape.Boundaries.Max = box.Max;
            m_boxShape.Boundaries.Min = box.Min;
            this.UpdateVoxelShape(Type, m_boxShape, material);
        }

        [Event(null, 0x30d), Reliable, Broadcast, RefreshReplicable]
        private void PerformVoxelOperationCapsule_Implementation(MyCapsuleShapeParams capsuleParams, OperationType Type)
        {
            this.BeforeContentChanged = true;
            m_capsuleShape.Transformation = capsuleParams.Transformation;
            m_capsuleShape.A = capsuleParams.A;
            m_capsuleShape.B = capsuleParams.B;
            m_capsuleShape.Radius = capsuleParams.Radius;
            this.UpdateVoxelShape(Type, m_capsuleShape, capsuleParams.Material);
        }

        [Event(null, 0x3d3), Reliable, Broadcast]
        private void PerformVoxelOperationElipsoid_Implementation(Vector3 radius, MatrixD Transformation, byte material, OperationType Type)
        {
            this.BeforeContentChanged = true;
            m_ellipsoidShape.Transformation = Transformation;
            m_ellipsoidShape.Radius = radius;
            this.UpdateVoxelShape(Type, m_ellipsoidShape, material);
        }

        [Event(null, 0x3a3), Reliable, Broadcast]
        private void PerformVoxelOperationRamp_Implementation(MyRampShapeParams shapeParams, OperationType Type)
        {
            this.BeforeContentChanged = true;
            m_rampShape.Transformation = shapeParams.Transformation;
            m_rampShape.Boundaries.Max = shapeParams.Box.Max;
            m_rampShape.Boundaries.Min = shapeParams.Box.Min;
            m_rampShape.RampNormal = shapeParams.RampNormal;
            m_rampShape.RampNormalW = shapeParams.RampNormalW;
            this.UpdateVoxelShape(Type, m_rampShape, shapeParams.Material);
        }

        [Event(null, 0x33c), Reliable, Broadcast, RefreshReplicable]
        private void PerformVoxelOperationSphere_Implementation(Vector3D center, float radius, byte material, OperationType Type)
        {
            m_sphereShape.Center = center;
            m_sphereShape.Radius = radius;
            this.BeforeContentChanged = true;
            this.UpdateVoxelShape(Type, m_sphereShape, material);
        }

        public void RequestVoxelCutoutSphere(Vector3D center, float radius, bool createDebris, bool damage)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MyVoxelBase, Vector3D, float, bool, bool>(this.RootVoxel, x => new Action<Vector3D, float, bool, bool>(x.VoxelCutoutSphere_Implementation), center, radius, createDebris, damage, targetEndpoint);
        }

        public void RequestVoxelOperationBox(BoundingBoxD box, MatrixD Transformation, byte material, OperationType Type)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, BoundingBoxD, MatrixD, byte, OperationType>(s => new Action<long, BoundingBoxD, MatrixD, byte, OperationType>(MyVoxelBase.VoxelOperationBox_Implementation), base.EntityId, box, Transformation, material, Type, targetEndpoint, position);
        }

        public void RequestVoxelOperationCapsule(Vector3D A, Vector3D B, float radius, MatrixD Transformation, byte material, OperationType Type)
        {
            MyCapsuleShapeParams @params = new MyCapsuleShapeParams {
                A = A,
                B = B,
                Radius = radius,
                Transformation = Transformation,
                Material = material
            };
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, MyCapsuleShapeParams, OperationType>(s => new Action<long, MyCapsuleShapeParams, OperationType>(MyVoxelBase.VoxelOperationCapsule_Implementation), base.EntityId, @params, Type, targetEndpoint, position);
        }

        public void RequestVoxelOperationElipsoid(Vector3 radius, MatrixD Transformation, byte material, OperationType Type)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector3, MatrixD, byte, OperationType>(s => new Action<long, Vector3, MatrixD, byte, OperationType>(MyVoxelBase.VoxelOperationElipsoid_Implementation), base.EntityId, radius, Transformation, material, Type, targetEndpoint, position);
        }

        public void RequestVoxelOperationRamp(BoundingBoxD box, Vector3D rampNormal, double rampNormalW, MatrixD Transformation, byte material, OperationType Type)
        {
            MyRampShapeParams @params = new MyRampShapeParams {
                Box = box,
                RampNormal = rampNormal,
                RampNormalW = rampNormalW,
                Transformation = Transformation,
                Material = material
            };
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, MyRampShapeParams, OperationType>(s => new Action<long, MyRampShapeParams, OperationType>(MyVoxelBase.VoxelOperationRamp_Implementation), base.EntityId, @params, Type, targetEndpoint, position);
        }

        public void RequestVoxelOperationSphere(Vector3D center, float radius, byte material, OperationType Type)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector3D, float, byte, OperationType>(s => new Action<long, Vector3D, float, byte, OperationType>(MyVoxelBase.VoxelOperationSphere_Implementation), base.EntityId, center, radius, material, Type, targetEndpoint, position);
        }

        [Event(null, 0x3dd), Reliable, ServerInvoked, BroadcastExcept]
        public void RevertVoxelAccess(Vector3I key, MyStorageDataTypeFlags flags)
        {
            if (this.Storage != null)
            {
                MyStorageBase storage = this.Storage as MyStorageBase;
                if (storage != null)
                {
                    storage.AccessDelete(ref key, flags, true);
                }
            }
        }

        private void UpdateVoxelShape(OperationType type, MyShape shape, byte Material)
        {
            MyShape localShape = shape.Clone();
            this.m_voxelShapeInProgress = true;
            switch (type)
            {
                case OperationType.Fill:
                    if (MyFakes.VOXELHAND_PARALLEL)
                    {
                        Parallel.Start((Action) (() => MyVoxelGenerator.FillInShape(this, localShape, Material)), (Action) (() => (this.m_voxelShapeInProgress = false)));
                        return;
                    }
                    MyVoxelGenerator.FillInShape(this, localShape, Material);
                    return;

                case OperationType.Paint:
                    if (MyFakes.VOXELHAND_PARALLEL)
                    {
                        Parallel.Start((Action) (() => MyVoxelGenerator.PaintInShape(this, localShape, Material)), (Action) (() => (this.m_voxelShapeInProgress = false)));
                        return;
                    }
                    MyVoxelGenerator.PaintInShape(this, localShape, Material);
                    return;

                case OperationType.Cut:
                    if (MyFakes.VOXELHAND_PARALLEL)
                    {
                        Parallel.Start((Action) (() => MyVoxelGenerator.CutOutShape(this, localShape, true)), (Action) (() => (this.m_voxelShapeInProgress = false)));
                        return;
                    }
                    MyVoxelGenerator.CutOutShape(this, localShape, true);
                    return;

                case OperationType.Revert:
                    if (MyFakes.VOXELHAND_PARALLEL)
                    {
                        Parallel.Start((Action) (() => MyVoxelGenerator.RevertShape(this, localShape)), (Action) (() => (this.m_voxelShapeInProgress = false)));
                        return;
                    }
                    MyVoxelGenerator.RevertShape(this, localShape);
                    return;
            }
            this.m_voxelShapeInProgress = false;
        }

        [Event(null, 0x2db), Reliable, Broadcast, RefreshReplicable]
        private void VoxelCutoutSphere_Implementation(Vector3D center, float radius, bool createDebris, bool damage = false)
        {
            this.BeforeContentChanged = true;
            MyExplosion.CutOutVoxelMap(radius, center, this, createDebris && MySession.Static.Ready, damage);
        }

        [Event(null, 0x34d), Reliable, Server, RefreshReplicable]
        private static void VoxelOperationBox_Implementation(long entityId, BoundingBoxD box, MatrixD Transformation, byte material, OperationType Type)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.GetVoxelHandAvailable(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                m_boxShape.Transformation = Transformation;
                m_boxShape.Boundaries.Max = box.Max;
                m_boxShape.Boundaries.Min = box.Min;
                if (CanPlaceInArea(Type, m_boxShape))
                {
                    MyEntity entity;
                    MyEntities.TryGetEntityById(entityId, out entity, false);
                    MyVoxelBase base2 = entity as MyVoxelBase;
                    if ((base2 != null) && !base2.m_voxelShapeInProgress)
                    {
                        base2.BeforeContentChanged = true;
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyVoxelBase, BoundingBoxD, MatrixD, byte, OperationType>(base2.RootVoxel, x => new Action<BoundingBoxD, MatrixD, byte, OperationType>(x.PerformVoxelOperationBox_Implementation), box, Transformation, material, Type, targetEndpoint);
                        base2.UpdateVoxelShape(Type, m_boxShape, material);
                    }
                }
            }
        }

        [Event(null, 750), Reliable, Server, RefreshReplicable]
        private static void VoxelOperationCapsule_Implementation(long entityId, MyCapsuleShapeParams capsuleParams, OperationType Type)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.GetVoxelHandAvailable(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                m_capsuleShape.Transformation = capsuleParams.Transformation;
                m_capsuleShape.A = capsuleParams.A;
                m_capsuleShape.B = capsuleParams.B;
                m_capsuleShape.Radius = capsuleParams.Radius;
                if (CanPlaceInArea(Type, m_capsuleShape))
                {
                    MyEntity entity;
                    MyEntities.TryGetEntityById(entityId, out entity, false);
                    MyVoxelBase base2 = entity as MyVoxelBase;
                    if ((base2 != null) && !base2.m_voxelShapeInProgress)
                    {
                        base2.BeforeContentChanged = true;
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyVoxelBase, MyCapsuleShapeParams, OperationType>(base2.RootVoxel, x => new Action<MyCapsuleShapeParams, OperationType>(x.PerformVoxelOperationCapsule_Implementation), capsuleParams, Type, targetEndpoint);
                        base2.UpdateVoxelShape(Type, m_capsuleShape, capsuleParams.Material);
                    }
                }
            }
        }

        [Event(null, 950), Reliable, Server, RefreshReplicable]
        private static void VoxelOperationElipsoid_Implementation(long entityId, Vector3 radius, MatrixD Transformation, byte material, OperationType Type)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.GetVoxelHandAvailable(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                m_ellipsoidShape.Transformation = Transformation;
                m_ellipsoidShape.Radius = radius;
                if (CanPlaceInArea(Type, m_ellipsoidShape))
                {
                    MyEntity entity;
                    MyEntities.TryGetEntityById(entityId, out entity, false);
                    MyVoxelBase base2 = entity as MyVoxelBase;
                    if ((base2 != null) && !base2.m_voxelShapeInProgress)
                    {
                        base2.BeforeContentChanged = true;
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyVoxelBase, Vector3, MatrixD, byte, OperationType>(base2.RootVoxel, x => new Action<Vector3, MatrixD, byte, OperationType>(x.PerformVoxelOperationElipsoid_Implementation), radius, Transformation, material, Type, targetEndpoint);
                        base2.UpdateVoxelShape(Type, m_ellipsoidShape, material);
                    }
                }
            }
        }

        [Event(null, 0x383), Reliable, Server, RefreshReplicable]
        private static void VoxelOperationRamp_Implementation(long entityId, MyRampShapeParams shapeParams, OperationType Type)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.GetVoxelHandAvailable(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                m_rampShape.Transformation = shapeParams.Transformation;
                m_rampShape.Boundaries.Max = shapeParams.Box.Max;
                m_rampShape.Boundaries.Min = shapeParams.Box.Min;
                m_rampShape.RampNormal = shapeParams.RampNormal;
                m_rampShape.RampNormalW = shapeParams.RampNormalW;
                if (CanPlaceInArea(Type, m_rampShape))
                {
                    MyEntity entity;
                    MyEntities.TryGetEntityById(entityId, out entity, false);
                    MyVoxelBase base2 = entity as MyVoxelBase;
                    if ((base2 != null) && !base2.m_voxelShapeInProgress)
                    {
                        base2.BeforeContentChanged = true;
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyVoxelBase, MyRampShapeParams, OperationType>(base2.RootVoxel, x => new Action<MyRampShapeParams, OperationType>(x.PerformVoxelOperationRamp_Implementation), shapeParams, Type, targetEndpoint);
                        base2.UpdateVoxelShape(Type, m_rampShape, shapeParams.Material);
                    }
                }
            }
        }

        [Event(null, 0x31f), Reliable, Server]
        private static void VoxelOperationSphere_Implementation(long entityId, Vector3D center, float radius, byte material, OperationType Type)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.GetVoxelHandAvailable(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                m_sphereShape.Center = center;
                m_sphereShape.Radius = radius;
                if (CanPlaceInArea(Type, m_sphereShape))
                {
                    MyEntity entity;
                    MyEntities.TryGetEntityById(entityId, out entity, false);
                    MyVoxelBase base2 = entity as MyVoxelBase;
                    if ((base2 != null) && !base2.m_voxelShapeInProgress)
                    {
                        base2.BeforeContentChanged = true;
                        EndpointId targetEndpoint = new EndpointId();
                        MyMultiplayer.RaiseEvent<MyVoxelBase, Vector3D, float, byte, OperationType>(base2.RootVoxel, x => new Action<Vector3D, float, byte, OperationType>(x.PerformVoxelOperationSphere_Implementation), center, radius, material, Type, targetEndpoint);
                        base2.UpdateVoxelShape(Type, m_sphereShape, material);
                    }
                }
            }
        }

        void IMyDecalProxy.AddDecals(ref MyHitInfo hitInfo, MyStringHash source, object customdata, IMyDecalHandler decalHandler, MyStringHash material)
        {
            MyDecalRenderInfo renderInfo = new MyDecalRenderInfo {
                Flags = MyDecalFlags.World,
                Position = hitInfo.Position,
                Normal = hitInfo.Normal,
                Source = source,
                RenderObjectIds = this.RootVoxel.Render.RenderObjectIDs,
                Material = base.Physics.GetMaterialAt(hitInfo.Position)
            };
            decalHandler.AddDecal(ref renderInfo, null);
        }

        protected void WorldPositionChanged(object source)
        {
            this.PositionLeftBottomCorner = base.WorldMatrix.Translation - Vector3D.TransformNormal(this.SizeInMetresHalf, base.WorldMatrix);
        }

        public Vector3I StorageMin =>
            this.m_storageMin;

        public Vector3I StorageMax =>
            this.m_storageMax;

        public string StorageName { get; protected set; }

        public float VoxelSize { get; private set; }

        protected VRage.Game.Voxels.IMyStorage m_storage
        {
            get => 
                this.m_storageInternal;
            set
            {
                if ((value != null) && !value.Shared)
                {
                    MyStorageBase base2 = value as MyStorageBase;
                    if ((base2 != null) && !base2.CachedWrites)
                    {
                        base2.InitWriteCache(0x80);
                    }
                }
                this.m_storageInternal = value;
            }
        }

        public virtual VRage.Game.Voxels.IMyStorage Storage
        {
            get => 
                this.m_storage;
            set
            {
            }
        }

        public bool DelayRigidBodyCreation { get; set; }

        public Vector3I Size =>
            (this.m_storageMax - this.m_storageMin);

        public Vector3I SizeMinusOne =>
            (this.Size - 1);

        public Vector3 SizeInMetres { get; protected set; }

        public Vector3 SizeInMetresHalf { get; protected set; }

        public virtual Vector3D PositionLeftBottomCorner { get; set; }

        public Matrix Orientation =>
            ((Matrix) base.PositionComp.WorldMatrix);

        public bool ContentChanged
        {
            get => 
                this.m_contentChanged;
            protected set
            {
                this.m_contentChanged = value;
                this.BeforeContentChanged = false;
            }
        }

        public abstract MyVoxelBase RootVoxel { get; }

        public bool BeforeContentChanged
        {
            get => 
                this.m_beforeContentChanged;
            protected set
            {
                if (this.m_beforeContentChanged != value)
                {
                    this.m_beforeContentChanged = value;
                    if ((this.m_beforeContentChanged && ((this.Storage != null) && this.Storage.Shared)) && (this.m_storage != null))
                    {
                        this.Storage = this.m_storage.Copy();
                        this.StorageName = MyVoxelMap.GetNewStorageName(this.StorageName, base.EntityId);
                    }
                }
            }
        }

        public bool CreatedByUser { get; set; }

        public string AsteroidName { get; set; }

        VRage.ModAPI.IMyStorage IMyVoxelBase.Storage =>
            this.Storage;

        string IMyVoxelBase.StorageName =>
            this.StorageName;

        public virtual MyClipmapScaleEnum ScaleGroup =>
            MyClipmapScaleEnum.Normal;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyVoxelBase.<>c <>9 = new MyVoxelBase.<>c();
            public static Func<MyVoxelBase, Action<Vector3D, float, bool, bool>> <>9__106_0;
            public static Func<IMyEventOwner, Action<long, MyVoxelBase.MyCapsuleShapeParams, MyVoxelBase.OperationType>> <>9__108_0;
            public static Func<MyVoxelBase, Action<MyVoxelBase.MyCapsuleShapeParams, MyVoxelBase.OperationType>> <>9__109_0;
            public static Func<IMyEventOwner, Action<long, Vector3D, float, byte, MyVoxelBase.OperationType>> <>9__111_0;
            public static Func<MyVoxelBase, Action<Vector3D, float, byte, MyVoxelBase.OperationType>> <>9__112_0;
            public static Func<IMyEventOwner, Action<long, BoundingBoxD, MatrixD, byte, MyVoxelBase.OperationType>> <>9__114_0;
            public static Func<MyVoxelBase, Action<BoundingBoxD, MatrixD, byte, MyVoxelBase.OperationType>> <>9__115_0;
            public static Func<IMyEventOwner, Action<long, MyVoxelBase.MyRampShapeParams, MyVoxelBase.OperationType>> <>9__117_0;
            public static Func<MyVoxelBase, Action<MyVoxelBase.MyRampShapeParams, MyVoxelBase.OperationType>> <>9__118_0;
            public static Func<IMyEventOwner, Action<long, Vector3, MatrixD, byte, MyVoxelBase.OperationType>> <>9__120_0;
            public static Func<MyVoxelBase, Action<Vector3, MatrixD, byte, MyVoxelBase.OperationType>> <>9__121_0;
            public static Func<MyVoxelBase, Action<Vector3D, float, Vector3, byte>> <>9__132_0;

            internal Action<Vector3D, float, Vector3, byte> <CreateVoxelMeteorCrater>b__132_0(MyVoxelBase x) => 
                new Action<Vector3D, float, Vector3, byte>(x.CreateVoxelMeteorCrater_Implementation);

            internal Action<Vector3D, float, bool, bool> <RequestVoxelCutoutSphere>b__106_0(MyVoxelBase x) => 
                new Action<Vector3D, float, bool, bool>(x.VoxelCutoutSphere_Implementation);

            internal Action<long, BoundingBoxD, MatrixD, byte, MyVoxelBase.OperationType> <RequestVoxelOperationBox>b__114_0(IMyEventOwner s) => 
                new Action<long, BoundingBoxD, MatrixD, byte, MyVoxelBase.OperationType>(MyVoxelBase.VoxelOperationBox_Implementation);

            internal Action<long, MyVoxelBase.MyCapsuleShapeParams, MyVoxelBase.OperationType> <RequestVoxelOperationCapsule>b__108_0(IMyEventOwner s) => 
                new Action<long, MyVoxelBase.MyCapsuleShapeParams, MyVoxelBase.OperationType>(MyVoxelBase.VoxelOperationCapsule_Implementation);

            internal Action<long, Vector3, MatrixD, byte, MyVoxelBase.OperationType> <RequestVoxelOperationElipsoid>b__120_0(IMyEventOwner s) => 
                new Action<long, Vector3, MatrixD, byte, MyVoxelBase.OperationType>(MyVoxelBase.VoxelOperationElipsoid_Implementation);

            internal Action<long, MyVoxelBase.MyRampShapeParams, MyVoxelBase.OperationType> <RequestVoxelOperationRamp>b__117_0(IMyEventOwner s) => 
                new Action<long, MyVoxelBase.MyRampShapeParams, MyVoxelBase.OperationType>(MyVoxelBase.VoxelOperationRamp_Implementation);

            internal Action<long, Vector3D, float, byte, MyVoxelBase.OperationType> <RequestVoxelOperationSphere>b__111_0(IMyEventOwner s) => 
                new Action<long, Vector3D, float, byte, MyVoxelBase.OperationType>(MyVoxelBase.VoxelOperationSphere_Implementation);

            internal Action<BoundingBoxD, MatrixD, byte, MyVoxelBase.OperationType> <VoxelOperationBox_Implementation>b__115_0(MyVoxelBase x) => 
                new Action<BoundingBoxD, MatrixD, byte, MyVoxelBase.OperationType>(x.PerformVoxelOperationBox_Implementation);

            internal Action<MyVoxelBase.MyCapsuleShapeParams, MyVoxelBase.OperationType> <VoxelOperationCapsule_Implementation>b__109_0(MyVoxelBase x) => 
                new Action<MyVoxelBase.MyCapsuleShapeParams, MyVoxelBase.OperationType>(x.PerformVoxelOperationCapsule_Implementation);

            internal Action<Vector3, MatrixD, byte, MyVoxelBase.OperationType> <VoxelOperationElipsoid_Implementation>b__121_0(MyVoxelBase x) => 
                new Action<Vector3, MatrixD, byte, MyVoxelBase.OperationType>(x.PerformVoxelOperationElipsoid_Implementation);

            internal Action<MyVoxelBase.MyRampShapeParams, MyVoxelBase.OperationType> <VoxelOperationRamp_Implementation>b__118_0(MyVoxelBase x) => 
                new Action<MyVoxelBase.MyRampShapeParams, MyVoxelBase.OperationType>(x.PerformVoxelOperationRamp_Implementation);

            internal Action<Vector3D, float, byte, MyVoxelBase.OperationType> <VoxelOperationSphere_Implementation>b__112_0(MyVoxelBase x) => 
                new Action<Vector3D, float, byte, MyVoxelBase.OperationType>(x.PerformVoxelOperationSphere_Implementation);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyCapsuleShapeParams
        {
            public Vector3D A;
            public Vector3D B;
            public float Radius;
            public MatrixD Transformation;
            public byte Material;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyRampShapeParams
        {
            public BoundingBoxD Box;
            public Vector3D RampNormal;
            public double RampNormalW;
            public MatrixD Transformation;
            public byte Material;
        }

        public delegate void OnCutOutResults(float voxelsCountInPercent, MyVoxelMaterialDefinition voxelMaterial, Dictionary<MyVoxelMaterialDefinition, int> exactCutOutMaterials);

        public enum OperationType : byte
        {
            Fill = 0,
            Paint = 1,
            Cut = 2,
            Revert = 3
        }

        public delegate void StorageChanged(MyVoxelBase storage, Vector3I minVoxelChanged, Vector3I maxVoxelChanged, MyStorageDataTypeFlags changedData);
    }
}

