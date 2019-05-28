namespace Sandbox.Engine.Voxels
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Library;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;

    public static class MyVoxelGenerator
    {
        private const int CELL_SIZE = 0x10;
        private const int VOXEL_CLAMP_BORDER_DISTANCE = 2;
        [ThreadStatic]
        private static MyStorageData m_cache;
        private static readonly List<MyEntity> m_overlapList = new List<MyEntity>();

        public static unsafe void ChangeMaterialsInShape(MyVoxelBase voxelMap, MyShape shape, byte materialIdx, bool[] materialsToChange)
        {
            if ((voxelMap != null) && (shape != null))
            {
                using (voxelMap.Pin())
                {
                    if (!voxelMap.MarkedForClose)
                    {
                        Vector3I vectori;
                        Vector3I vectori2;
                        Vector3I vectori5;
                        MatrixD xd = shape.Transformation * voxelMap.PositionComp.WorldMatrixInvScaled;
                        MatrixD* xdPtr1 = (MatrixD*) ref xd;
                        xdPtr1.Translation += voxelMap.SizeInMetresHalf;
                        shape.Transformation = xd;
                        BoundingBoxD worldBoundaries = shape.GetWorldBoundaries();
                        ComputeShapeBounds(voxelMap, ref worldBoundaries, Vector3.Zero, voxelMap.Storage.Size, out vectori, out vectori2);
                        Vector3I voxelCoord = vectori - 1;
                        Vector3I vectori4 = (Vector3I) (vectori2 + 1);
                        voxelMap.Storage.ClampVoxelCoord(ref voxelCoord, 1);
                        voxelMap.Storage.ClampVoxelCoord(ref vectori4, 1);
                        if (m_cache == null)
                        {
                            m_cache = new MyStorageData(MyStorageDataTypeFlags.All);
                        }
                        m_cache.Resize(voxelCoord, vectori4);
                        MyVoxelRequestFlags requestFlags = MyVoxelRequestFlags.AdviseCache | MyVoxelRequestFlags.ConsiderContent;
                        voxelMap.Storage.ReadRange(m_cache, MyStorageDataTypeFlags.Material, 0, voxelCoord, vectori4, ref requestFlags);
                        vectori5.X = vectori.X;
                        while (vectori5.X <= vectori2.X)
                        {
                            vectori5.Y = vectori.Y;
                            while (true)
                            {
                                if (vectori5.Y > vectori2.Y)
                                {
                                    int* numPtr3 = (int*) ref vectori5.X;
                                    numPtr3[0]++;
                                    break;
                                }
                                vectori5.Z = vectori.Z;
                                while (true)
                                {
                                    if (vectori5.Z > vectori2.Z)
                                    {
                                        int* numPtr2 = (int*) ref vectori5.Y;
                                        numPtr2[0]++;
                                        break;
                                    }
                                    Vector3I p = vectori5 - vectori;
                                    int linearIdx = m_cache.ComputeLinear(ref p);
                                    byte index = m_cache.Material(linearIdx);
                                    if (materialsToChange[index])
                                    {
                                        Vector3D vectord;
                                        MyVoxelCoordSystems.VoxelCoordToWorldPosition(voxelMap.PositionLeftBottomCorner, ref vectori5, out vectord);
                                        if ((shape.GetVolume(ref vectord) > 0.5f) && (m_cache.Material(ref p) != 0xff))
                                        {
                                            m_cache.Material(ref p, materialIdx);
                                        }
                                    }
                                    int* numPtr1 = (int*) ref vectori5.Z;
                                    numPtr1[0]++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static ClampingInfo CheckForClamping(Vector3I originalValue, Vector3I clampedValue)
        {
            ClampingInfo info = new ClampingInfo(false, false, false);
            if (originalValue.X != clampedValue.X)
            {
                info.X = true;
            }
            if (originalValue.Y != clampedValue.Y)
            {
                info.Y = true;
            }
            if (originalValue.Z != clampedValue.Z)
            {
                info.Z = true;
            }
            return info;
        }

        private static void ComputeShapeBounds(MyVoxelBase voxelMap, ref BoundingBoxD shapeAabb, Vector3D voxelMapMinCorner, Vector3I storageSize, out Vector3I voxelMin, out Vector3I voxelMax)
        {
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxelMapMinCorner, ref shapeAabb.Min, out voxelMin);
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxelMapMinCorner, ref shapeAabb.Max, out voxelMax);
            voxelMin = (Vector3I) (voxelMin + voxelMap.StorageMin);
            voxelMax = (Vector3I) (voxelMax + voxelMap.StorageMin);
            voxelMax = (Vector3I) (voxelMax + 1);
            storageSize -= 1;
            Vector3I.Clamp(ref voxelMin, ref Vector3I.Zero, ref storageSize, out voxelMin);
            Vector3I.Clamp(ref voxelMax, ref Vector3I.Zero, ref storageSize, out voxelMax);
        }

        public static void CutOutShape(MyVoxelBase voxelMap, MyShape shape, bool voxelHand = false)
        {
            if (MySession.Static.EnableVoxelDestruction || MySession.Static.HighSimulationQuality)
            {
                using (voxelMap.Pin())
                {
                    if (!voxelMap.MarkedForClose)
                    {
                        Vector3I vectori;
                        Vector3I maxCorner;
                        Vector3I minCorner;
                        GetVoxelShapeDimensions(voxelMap, shape, out minCorner, out maxCorner, out vectori);
                        ulong num = 0UL;
                        if (m_cache == null)
                        {
                            m_cache = new MyStorageData(MyStorageDataTypeFlags.All);
                        }
                        Vector3I_RangeIterator it = new Vector3I_RangeIterator(ref Vector3I.Zero, ref vectori);
                        while (true)
                        {
                            Vector3I vectori2;
                            Vector3I vectori3;
                            if (!it.IsValid())
                            {
                                if (num > 0L)
                                {
                                    BoundingBoxD cutOutBox = shape.GetWorldBoundaries();
                                    MySandboxGame.Static.Invoke(delegate {
                                        if (voxelMap.Storage != null)
                                        {
                                            voxelMap.Storage.NotifyChanged(minCorner, maxCorner, MyStorageDataTypeFlags.All);
                                            NotifyVoxelChanged(MyVoxelBase.OperationType.Cut, voxelMap, ref cutOutBox);
                                        }
                                    }, "CutOutShape notify");
                                }
                                break;
                            }
                            GetCellCorners(ref minCorner, ref maxCorner, ref it, out vectori2, out vectori3);
                            Vector3I voxelCoord = vectori2 - 1;
                            Vector3I vectori5 = (Vector3I) (vectori3 + 1);
                            voxelMap.Storage.ClampVoxelCoord(ref voxelCoord, 1);
                            voxelMap.Storage.ClampVoxelCoord(ref vectori5, 1);
                            ulong num2 = 0UL;
                            m_cache.Resize(voxelCoord, vectori5);
                            MyVoxelRequestFlags considerContent = MyVoxelRequestFlags.ConsiderContent;
                            voxelMap.Storage.ReadRange(m_cache, MyStorageDataTypeFlags.All, 0, voxelCoord, vectori5, ref considerContent);
                            Vector3I_RangeIterator iterator2 = new Vector3I_RangeIterator(ref vectori2, ref vectori3);
                            while (true)
                            {
                                if (!iterator2.IsValid())
                                {
                                    if (num2 > 0L)
                                    {
                                        RemoveSmallVoxelsUsingChachedVoxels();
                                        voxelMap.Storage.WriteRange(m_cache, MyStorageDataTypeFlags.All, voxelCoord, vectori5, false, true);
                                    }
                                    num += num2;
                                    it.MoveNext();
                                    break;
                                }
                                Vector3I p = iterator2.Current - voxelCoord;
                                byte num3 = m_cache.Content(ref p);
                                if (num3 != 0)
                                {
                                    Vector3D vectord;
                                    MyVoxelCoordSystems.VoxelCoordToWorldPosition(voxelMap.PositionLeftBottomCorner, ref iterator2.Current, out vectord);
                                    float volume = shape.GetVolume(ref vectord);
                                    if (volume != 0f)
                                    {
                                        int num5 = Math.Min((int) (255f - (volume * 255f)), num3);
                                        ulong num6 = (ulong) Math.Abs((int) (num3 - num5));
                                        m_cache.Content(ref p, (byte) num5);
                                        if (num5 == 0)
                                        {
                                            m_cache.Material(ref p, 0xff);
                                        }
                                        num2 += num6;
                                    }
                                }
                                iterator2.MoveNext();
                            }
                        }
                    }
                }
            }
        }

        public static unsafe void CutOutShapeWithProperties(MyVoxelBase voxelMap, MyShape shape, out float voxelsCountInPercent, out MyVoxelMaterialDefinition voxelMaterial, Dictionary<MyVoxelMaterialDefinition, int> exactCutOutMaterials = null, bool updateSync = false, bool onlyCheck = false, bool applyDamageMaterial = false, bool onlyApplyMaterial = false, bool skipCache = false)
        {
            if ((!MySession.Static.EnableVoxelDestruction || ((voxelMap == null) || (voxelMap.Storage == null))) || (shape == null))
            {
                voxelsCountInPercent = 0f;
                voxelMaterial = null;
            }
            else
            {
                Vector3I vectori3;
                Vector3I vectori4;
                Vector3I maxCorner;
                Vector3I minCorner;
                int num = 0;
                int num2 = 0;
                MatrixD transformation = shape.Transformation;
                MatrixD xd2 = transformation * voxelMap.PositionComp.WorldMatrixInvScaled;
                MatrixD* xdPtr1 = (MatrixD*) ref xd2;
                xdPtr1.Translation += voxelMap.SizeInMetresHalf;
                shape.Transformation = xd2;
                BoundingBoxD worldBoundaries = shape.GetWorldBoundaries();
                ComputeShapeBounds(voxelMap, ref worldBoundaries, Vector3.Zero, voxelMap.Storage.Size, out minCorner, out maxCorner);
                bool flag = (exactCutOutMaterials != null) | applyDamageMaterial;
                Vector3I voxelCoord = minCorner - 1;
                Vector3I vectori2 = (Vector3I) (maxCorner + 1);
                voxelMap.Storage.ClampVoxelCoord(ref voxelCoord, 1);
                voxelMap.Storage.ClampVoxelCoord(ref vectori2, 1);
                if (m_cache == null)
                {
                    m_cache = new MyStorageData(MyStorageDataTypeFlags.All);
                }
                m_cache.Resize(voxelCoord, vectori2);
                MyVoxelRequestFlags requestFlags = (skipCache ? ((MyVoxelRequestFlags) 0) : MyVoxelRequestFlags.AdviseCache) | (flag ? MyVoxelRequestFlags.ConsiderContent : ((MyVoxelRequestFlags) 0));
                voxelMap.Storage.ReadRange(m_cache, flag ? MyStorageDataTypeFlags.All : MyStorageDataTypeFlags.Content, 0, voxelCoord, vectori2, ref requestFlags);
                if (exactCutOutMaterials != null)
                {
                    vectori3 = (Vector3I) (m_cache.Size3D / 2);
                    voxelMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinition(m_cache.Material(ref vectori3));
                }
                else
                {
                    vectori3 = (Vector3I) ((voxelCoord + vectori2) / 2);
                    voxelMaterial = voxelMap.Storage.GetMaterialAt(ref vectori3);
                }
                MyVoxelMaterialDefinition key = null;
                vectori4.X = minCorner.X;
                while (vectori4.X <= maxCorner.X)
                {
                    vectori4.Y = minCorner.Y;
                    while (true)
                    {
                        if (vectori4.Y > maxCorner.Y)
                        {
                            int* numPtr3 = (int*) ref vectori4.X;
                            numPtr3[0]++;
                            break;
                        }
                        vectori4.Z = minCorner.Z;
                        while (true)
                        {
                            if (vectori4.Z > maxCorner.Z)
                            {
                                int* numPtr2 = (int*) ref vectori4.Y;
                                numPtr2[0]++;
                                break;
                            }
                            Vector3I p = vectori4 - voxelCoord;
                            int linearIdx = m_cache.ComputeLinear(ref p);
                            byte num4 = m_cache.Content(linearIdx);
                            if (num4 != 0)
                            {
                                Vector3D voxelPosition = (Vector3D) ((vectori4 - voxelMap.StorageMin) * 1.0);
                                float volume = shape.GetVolume(ref voxelPosition);
                                if (volume != 0f)
                                {
                                    int num7 = Math.Max(num4 - ((int) (volume * 255f)), 0);
                                    int num8 = num4 - num7;
                                    if ((num4 / 10) != (num7 / 10))
                                    {
                                        if (!onlyCheck && !onlyApplyMaterial)
                                        {
                                            m_cache.Content(linearIdx, (byte) num7);
                                        }
                                        num += num4;
                                        num2 += num8;
                                        byte materialIndex = m_cache.Material(linearIdx);
                                        if (num7 == 0)
                                        {
                                            m_cache.Material(linearIdx, 0xff);
                                        }
                                        if (materialIndex != 0xff)
                                        {
                                            if (flag)
                                            {
                                                key = MyDefinitionManager.Static.GetVoxelMaterialDefinition(materialIndex);
                                            }
                                            if (exactCutOutMaterials != null)
                                            {
                                                int num10;
                                                exactCutOutMaterials.TryGetValue(key, out num10);
                                                exactCutOutMaterials[key] = num10 + (MyFakes.ENABLE_REMOVED_VOXEL_CONTENT_HACK ? ((int) (num8 * 3.9f)) : num8);
                                            }
                                        }
                                    }
                                }
                            }
                            int* numPtr1 = (int*) ref vectori4.Z;
                            numPtr1[0]++;
                        }
                    }
                }
                if ((((num2 > 0) & updateSync) && Sync.IsServer) && !onlyCheck)
                {
                    shape.SendDrillCutOutRequest(voxelMap, applyDamageMaterial);
                }
                if ((num2 > 0) && !onlyCheck)
                {
                    RemoveSmallVoxelsUsingChachedVoxels();
                    MyStorageDataTypeFlags all = MyStorageDataTypeFlags.All;
                    if (MyFakes.LOG_NAVMESH_GENERATION && (MyAIComponent.Static.Pathfinding != null))
                    {
                        MyAIComponent.Static.Pathfinding.GetPathfindingLog().LogStorageWrite(voxelMap, m_cache, all, voxelCoord, vectori2);
                    }
                    voxelMap.Storage.WriteRange(m_cache, all, voxelCoord, vectori2, false, skipCache);
                }
                voxelsCountInPercent = (num > 0f) ? (((float) num2) / ((float) num)) : 0f;
                if (num2 > 0)
                {
                    BoundingBoxD cutOutBox = shape.GetWorldBoundaries();
                    MySandboxGame.Static.Invoke(delegate {
                        if (voxelMap.Storage != null)
                        {
                            voxelMap.Storage.NotifyChanged(minCorner, maxCorner, MyStorageDataTypeFlags.All);
                            NotifyVoxelChanged(MyVoxelBase.OperationType.Cut, voxelMap, ref cutOutBox);
                        }
                    }, "CutOutShapeWithProperties notify");
                }
                shape.Transformation = transformation;
            }
        }

        public static unsafe bool CutOutSphereFast(MyVoxelBase voxelMap, ref Vector3D center, float radius, out Vector3I cacheMin, out Vector3I cacheMax, bool notifyChanged)
        {
            Vector3I vectori;
            Vector3I vectori2;
            MatrixD worldMatrixInvScaled = voxelMap.PositionComp.WorldMatrixInvScaled;
            MatrixD* xdPtr1 = (MatrixD*) ref worldMatrixInvScaled;
            xdPtr1.Translation += voxelMap.SizeInMetresHalf;
            BoundingBoxD shapeAabb = BoundingBoxD.CreateFromSphere(new BoundingSphereD(center, (double) radius)).TransformFast(worldMatrixInvScaled);
            ComputeShapeBounds(voxelMap, ref shapeAabb, Vector3.Zero, voxelMap.Storage.Size, out vectori, out vectori2);
            cacheMin = vectori - 1;
            cacheMax = (Vector3I) (vectori2 + 1);
            voxelMap.Storage.ClampVoxelCoord(ref cacheMin, 1);
            voxelMap.Storage.ClampVoxelCoord(ref cacheMax, 1);
            CutOutSphere voxelOperator = new CutOutSphere {
                RadSq = radius * radius,
                Center = Vector3D.Transform(center, worldMatrixInvScaled) - (cacheMin - voxelMap.StorageMin)
            };
            voxelMap.Storage.ExecuteOperationFast<CutOutSphere>(ref voxelOperator, MyStorageDataTypeFlags.Content, ref cacheMin, ref cacheMax, notifyChanged);
            return voxelOperator.Changed;
        }

        public static void FillInShape(MyVoxelBase voxelMap, MyShape shape, byte materialIdx)
        {
            using (voxelMap.Pin())
            {
                if (!voxelMap.MarkedForClose)
                {
                    Vector3I vectori;
                    Vector3I maxCorner;
                    Vector3I minCorner;
                    ulong num = 0UL;
                    GetVoxelShapeDimensions(voxelMap, shape, out minCorner, out maxCorner, out vectori);
                    minCorner = Vector3I.Max(Vector3I.One, minCorner);
                    maxCorner = Vector3I.Max(minCorner, maxCorner - Vector3I.One);
                    if (m_cache == null)
                    {
                        m_cache = new MyStorageData(MyStorageDataTypeFlags.All);
                    }
                    Vector3I_RangeIterator it = new Vector3I_RangeIterator(ref Vector3I.Zero, ref vectori);
                    while (true)
                    {
                        Vector3I vectori2;
                        Vector3I vectori3;
                        if (!it.IsValid())
                        {
                            if (num > 0L)
                            {
                                BoundingBoxD cutOutBox = shape.GetWorldBoundaries();
                                MySandboxGame.Static.Invoke(delegate {
                                    if (voxelMap.Storage != null)
                                    {
                                        voxelMap.Storage.NotifyChanged(minCorner, maxCorner, MyStorageDataTypeFlags.All);
                                        NotifyVoxelChanged(MyVoxelBase.OperationType.Fill, voxelMap, ref cutOutBox);
                                    }
                                }, "FillInShape Notify");
                            }
                            break;
                        }
                        GetCellCorners(ref minCorner, ref maxCorner, ref it, out vectori2, out vectori3);
                        Vector3I originalValue = vectori3;
                        voxelMap.Storage.ClampVoxelCoord(ref vectori2, 0);
                        voxelMap.Storage.ClampVoxelCoord(ref vectori3, 0);
                        ClampingInfo info = CheckForClamping(vectori2, vectori2);
                        ClampingInfo info2 = CheckForClamping(originalValue, vectori3);
                        m_cache.Resize(vectori2, vectori3);
                        MyVoxelRequestFlags considerContent = MyVoxelRequestFlags.ConsiderContent;
                        voxelMap.Storage.ReadRange(m_cache, MyStorageDataTypeFlags.All, 0, vectori2, vectori3, ref considerContent);
                        ulong num2 = 0UL;
                        Vector3I_RangeIterator iterator2 = new Vector3I_RangeIterator(ref vectori2, ref vectori3);
                        while (true)
                        {
                            if (!iterator2.IsValid())
                            {
                                if (num2 > 0L)
                                {
                                    RemoveSmallVoxelsUsingChachedVoxels();
                                    voxelMap.Storage.WriteRange(m_cache, MyStorageDataTypeFlags.All, vectori2, vectori3, false, true);
                                }
                                num += num2;
                                it.MoveNext();
                                break;
                            }
                            Vector3I p = iterator2.Current - vectori2;
                            byte num3 = m_cache.Content(ref p);
                            if ((num3 != 0xff) || (m_cache.Material(ref p) != materialIdx))
                            {
                                if ((((iterator2.Current.X == vectori2.X) && info.X) || (((iterator2.Current.X == vectori3.X) && info2.X) || (((iterator2.Current.Y == vectori2.Y) && info.Y) || (((iterator2.Current.Y == vectori3.Y) && info2.Y) || ((iterator2.Current.Z == vectori2.Z) && info.Z))))) || ((iterator2.Current.Z == vectori3.Z) && info2.Z))
                                {
                                    if (num3 != 0)
                                    {
                                        m_cache.Material(ref p, materialIdx);
                                    }
                                }
                                else
                                {
                                    Vector3D vectord;
                                    MyVoxelCoordSystems.VoxelCoordToWorldPosition(voxelMap.PositionLeftBottomCorner, ref iterator2.Current, out vectord);
                                    float volume = shape.GetVolume(ref vectord);
                                    if (volume > 0f)
                                    {
                                        long num6 = Math.Max(num3, (int) (volume * 255f));
                                        m_cache.Content(ref p, (byte) num6);
                                        if (num6 != 0)
                                        {
                                            m_cache.Material(ref p, materialIdx);
                                        }
                                        num2 += ((ulong) num6) - num3;
                                    }
                                }
                            }
                            iterator2.MoveNext();
                        }
                    }
                }
            }
        }

        private static void GetCellCorners(ref Vector3I minCorner, ref Vector3I maxCorner, ref Vector3I_RangeIterator it, out Vector3I cellMinCorner, out Vector3I cellMaxCorner)
        {
            cellMinCorner = new Vector3I(Math.Min(maxCorner.X, minCorner.X + (it.Current.X * 0x10)), Math.Min(maxCorner.Y, minCorner.Y + (it.Current.Y * 0x10)), Math.Min(maxCorner.Z, minCorner.Z + (it.Current.Z * 0x10)));
            cellMaxCorner = new Vector3I(Math.Min(maxCorner.X, cellMinCorner.X + 0x10), Math.Min(maxCorner.Y, cellMinCorner.Y + 0x10), Math.Min(maxCorner.Z, cellMinCorner.Z + 0x10));
        }

        private static void GetVoxelShapeDimensions(MyVoxelBase voxelMap, MyShape shape, out Vector3I minCorner, out Vector3I maxCorner, out Vector3I numCells)
        {
            BoundingBoxD worldBoundaries = shape.GetWorldBoundaries();
            ComputeShapeBounds(voxelMap, ref worldBoundaries, voxelMap.PositionLeftBottomCorner, voxelMap.Storage.Size, out minCorner, out maxCorner);
            numCells = new Vector3I((maxCorner.X - minCorner.X) / 0x10, (maxCorner.Y - minCorner.Y) / 0x10, (maxCorner.Z - minCorner.Z) / 0x10);
        }

        public static void MakeCrater(MyVoxelBase voxelMap, BoundingSphereD sphere, Vector3 direction, MyVoxelMaterialDefinition material)
        {
            try
            {
                MakeCraterInternal(voxelMap, ref sphere, ref direction, material);
            }
            catch (NullReferenceException exception)
            {
                MyLog.Default.Error("NRE while creating asteroid crater." + MyEnvironment.NewLine + exception, Array.Empty<object>());
            }
        }

        private static unsafe void MakeCraterInternal(MyVoxelBase voxelMap, ref BoundingSphereD sphere, ref Vector3 direction, MyVoxelMaterialDefinition material)
        {
            Vector3I vectori;
            Vector3I vectori2;
            Vector3I vectori5;
            Vector3 vector = Vector3.Normalize(sphere.Center - voxelMap.RootVoxel.WorldMatrix.Translation);
            Vector3D worldPosition = sphere.Center - ((sphere.Radius - 1.0) * 1.2999999523162842);
            Vector3D vectord2 = sphere.Center + ((sphere.Radius + 1.0) * 1.2999999523162842);
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxelMap.PositionLeftBottomCorner, ref worldPosition, out vectori);
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxelMap.PositionLeftBottomCorner, ref vectord2, out vectori2);
            voxelMap.Storage.ClampVoxelCoord(ref vectori, 1);
            voxelMap.Storage.ClampVoxelCoord(ref vectori2, 1);
            Vector3I lodVoxelRangeMin = (Vector3I) (vectori + voxelMap.StorageMin);
            Vector3I lodVoxelRangeMax = (Vector3I) (vectori2 + voxelMap.StorageMin);
            bool flag = false;
            if (m_cache == null)
            {
                m_cache = new MyStorageData(MyStorageDataTypeFlags.All);
            }
            m_cache.Resize(vectori, vectori2);
            MyVoxelRequestFlags considerContent = MyVoxelRequestFlags.ConsiderContent;
            voxelMap.Storage.ReadRange(m_cache, MyStorageDataTypeFlags.All, 0, lodVoxelRangeMin, lodVoxelRangeMax, ref considerContent);
            int num = 0;
            Vector3I p = (Vector3I) ((vectori2 - vectori) / 2);
            byte materialIdx = m_cache.Material(ref p);
            float num3 = 1f - Vector3.Dot(vector, direction);
            Vector3 vector2 = ((Vector3) sphere.Center) - ((vector * ((float) sphere.Radius)) * 1.1f);
            float num4 = (float) (sphere.Radius * 1.5);
            float num5 = num4 * num4;
            float num6 = 0.5f * ((2f * num4) + 0.5f);
            float num7 = 0.5f * ((-2f * num4) + 0.5f);
            Vector3 vector3 = (vector2 + ((vector * ((float) sphere.Radius)) * (0.7f + num3))) + ((direction * ((float) sphere.Radius)) * 0.65f);
            float radius = (float) sphere.Radius;
            float num9 = radius * radius;
            float num10 = 0.5f * ((2f * radius) + 0.5f);
            float num11 = 0.5f * ((-2f * radius) + 0.5f);
            Vector3 vector4 = (vector2 + ((vector * ((float) sphere.Radius)) * num3)) + ((direction * ((float) sphere.Radius)) * 0.3f);
            float num12 = (float) (sphere.Radius * 0.10000000149011612);
            float num13 = num12 * num12;
            float num14 = 0.5f * ((2f * num12) + 0.5f);
            vectori5.Z = vectori.Z;
            p.Z = 0;
            goto TR_003C;
        TR_0005:
            int* numPtr1 = (int*) ref vectori5.X;
            numPtr1[0]++;
            int* numPtr2 = (int*) ref p.X;
            numPtr2[0]++;
        TR_0036:
            while (true)
            {
                Vector3D vectord3;
                byte num18;
                if (vectori5.X > vectori2.X)
                {
                    int* numPtr3 = (int*) ref vectori5.Y;
                    numPtr3[0]++;
                    int* numPtr4 = (int*) ref p.Y;
                    numPtr4[0]++;
                    break;
                }
                MyVoxelCoordSystems.VoxelCoordToWorldPosition(voxelMap.PositionLeftBottomCorner, ref vectori5, out vectord3);
                byte num15 = m_cache.Content(ref p);
                if (num15 != 0xff)
                {
                    byte num22;
                    float num20 = (float) (vectord3 - vector2).LengthSquared();
                    float num21 = num20 - num5;
                    if (num21 > num6)
                    {
                        num22 = 0;
                    }
                    else if (num21 < num7)
                    {
                        num22 = 0xff;
                    }
                    else
                    {
                        float num23 = (float) Math.Sqrt((num20 + num5) - ((2f * num4) * Math.Sqrt((double) num20)));
                        if (num21 < 0f)
                        {
                            num23 = -num23;
                        }
                        num22 = (byte) (127f - ((num23 / 0.5f) * 127f));
                    }
                    if (num22 > num15)
                    {
                        if (material != null)
                        {
                            m_cache.Material(ref p, materialIdx);
                        }
                        flag = true;
                        m_cache.Content(ref p, num22);
                    }
                }
                float num16 = (float) (vectord3 - vector3).LengthSquared();
                float num17 = num16 - num9;
                if (num17 > num10)
                {
                    num18 = 0;
                }
                else if (num17 < num11)
                {
                    num18 = 0xff;
                }
                else
                {
                    float num24 = (float) Math.Sqrt((num16 + num9) - ((2f * radius) * Math.Sqrt((double) num16)));
                    if (num17 < 0f)
                    {
                        num24 = -num24;
                    }
                    num18 = (byte) (127f - ((num24 / 0.5f) * 127f));
                }
                num15 = m_cache.Content(ref p);
                if ((num15 > 0) && (num18 > 0))
                {
                    flag = true;
                    int num25 = num15 - num18;
                    if (num25 < 0)
                    {
                        num25 = 0;
                    }
                    m_cache.Content(ref p, (byte) num25);
                    num += num15 - num25;
                }
                float num19 = ((float) (vectord3 - vector4).LengthSquared()) - num13;
                if (num19 <= 1.5f)
                {
                    MyVoxelMaterialDefinition voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition(m_cache.Material(ref p));
                    MyVoxelMaterialDefinition objB = material;
                    if (num19 > 0f)
                    {
                        byte num26 = m_cache.Content(ref p);
                        if (num26 == 0xff)
                        {
                            objB = voxelMaterialDefinition;
                        }
                        if ((num19 >= num14) && (num26 != 0))
                        {
                            objB = voxelMaterialDefinition;
                        }
                    }
                    if (ReferenceEquals(voxelMaterialDefinition, objB))
                    {
                        goto TR_0005;
                    }
                    else
                    {
                        m_cache.Material(ref p, objB.Index);
                        flag = true;
                    }
                }
                if ((((((float) (vectord3 - vector2).LengthSquared()) - num5) <= 0f) && (m_cache.Content(ref p) > 0)) && m_cache.WrinkleVoxelContent(ref p, 0.5f, 0.45f))
                {
                    flag = true;
                }
                goto TR_0005;
            }
        TR_0039:
            while (true)
            {
                if (vectori5.Y > vectori2.Y)
                {
                    int* numPtr5 = (int*) ref vectori5.Z;
                    numPtr5[0]++;
                    int* numPtr6 = (int*) ref p.Z;
                    numPtr6[0]++;
                    break;
                }
                vectori5.X = vectori.X;
                p.X = 0;
                goto TR_0036;
            }
        TR_003C:
            while (true)
            {
                if (vectori5.Z <= vectori2.Z)
                {
                    vectori5.Y = vectori.Y;
                    p.Y = 0;
                    break;
                }
                if (flag)
                {
                    RemoveSmallVoxelsUsingChachedVoxels();
                    vectori = (Vector3I) (vectori + voxelMap.StorageMin);
                    voxelMap.Storage.WriteRange(m_cache, MyStorageDataTypeFlags.All, vectori, (Vector3I) (vectori2 + voxelMap.StorageMin), true, false);
                    MyShapeSphere sphere1 = new MyShapeSphere();
                    sphere1.Center = sphere.Center;
                    sphere1.Radius = (float) (sphere.Radius * 1.5);
                    BoundingBoxD worldBoundaries = sphere1.GetWorldBoundaries();
                    NotifyVoxelChanged(MyVoxelBase.OperationType.Cut, voxelMap, ref worldBoundaries);
                }
                return;
            }
            goto TR_0039;
        }

        public static void NotifyVoxelChanged(MyVoxelBase.OperationType type, MyVoxelBase voxelMap, ref BoundingBoxD cutOutBox)
        {
            cutOutBox.Inflate((double) 0.25);
            MyGamePruningStructure.GetTopmostEntitiesInBox(ref cutOutBox, m_overlapList, MyEntityQueryType.Both);
            if (MyFakes.ENABLE_BLOCKS_IN_VOXELS_TEST)
            {
                foreach (MyEntity entity in m_overlapList)
                {
                    if (Sync.IsServer)
                    {
                        MyCubeGrid grid = entity as MyCubeGrid;
                        if ((grid != null) && grid.IsStatic)
                        {
                            if ((grid.Physics != null) && (grid.Physics.Shape != null))
                            {
                                grid.Physics.Shape.RecalculateConnectionsToWorld(grid.GetBlocks());
                            }
                            if (type == MyVoxelBase.OperationType.Cut)
                            {
                                grid.TestDynamic = MyCubeGrid.MyTestDynamicReason.GridSplit;
                            }
                        }
                    }
                    MyPhysicsBody physics = entity.Physics as MyPhysicsBody;
                    if (((physics != null) && !physics.IsStatic) && (physics.RigidBody != null))
                    {
                        physics.RigidBody.Activate();
                    }
                }
            }
            m_overlapList.Clear();
            if (Sync.IsServer)
            {
                MyPlanetEnvironmentComponent component = voxelMap.Components.Get<MyPlanetEnvironmentComponent>();
                if (component != null)
                {
                    component.GetSectorsInRange(ref cutOutBox, m_overlapList);
                    using (List<MyEntity>.Enumerator enumerator = m_overlapList.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            ((MyEnvironmentSector) enumerator.Current).DisableItemsInBox(ref cutOutBox);
                        }
                    }
                    m_overlapList.Clear();
                }
            }
        }

        public static void PaintInShape(MyVoxelBase voxelMap, MyShape shape, byte materialIdx)
        {
            using (voxelMap.Pin())
            {
                if (!voxelMap.MarkedForClose)
                {
                    Vector3I vectori;
                    Vector3I maxCorner;
                    Vector3I minCorner;
                    GetVoxelShapeDimensions(voxelMap, shape, out minCorner, out maxCorner, out vectori);
                    if (m_cache == null)
                    {
                        m_cache = new MyStorageData(MyStorageDataTypeFlags.All);
                    }
                    Vector3I_RangeIterator it = new Vector3I_RangeIterator(ref Vector3I.Zero, ref vectori);
                    while (true)
                    {
                        Vector3I vectori2;
                        Vector3I vectori3;
                        if (!it.IsValid())
                        {
                            MySandboxGame.Static.Invoke(delegate {
                                if (voxelMap.Storage != null)
                                {
                                    voxelMap.Storage.NotifyChanged(minCorner, maxCorner, MyStorageDataTypeFlags.All);
                                }
                            }, "PaintInShape notify");
                            break;
                        }
                        GetCellCorners(ref minCorner, ref maxCorner, ref it, out vectori2, out vectori3);
                        m_cache.Resize(vectori2, vectori3);
                        MyVoxelRequestFlags considerContent = MyVoxelRequestFlags.ConsiderContent;
                        voxelMap.Storage.ReadRange(m_cache, MyStorageDataTypeFlags.All, 0, vectori2, vectori3, ref considerContent);
                        Vector3I_RangeIterator iterator2 = new Vector3I_RangeIterator(ref vectori2, ref vectori3);
                        while (true)
                        {
                            Vector3D vectord;
                            if (!iterator2.IsValid())
                            {
                                voxelMap.Storage.WriteRange(m_cache, MyStorageDataTypeFlags.Material, vectori2, vectori3, false, true);
                                it.MoveNext();
                                break;
                            }
                            Vector3I p = iterator2.Current - vectori2;
                            MyVoxelCoordSystems.VoxelCoordToWorldPosition(voxelMap.PositionLeftBottomCorner, ref iterator2.Current, out vectord);
                            if ((shape.GetVolume(ref vectord) > 0.5f) && (m_cache.Material(ref p) != 0xff))
                            {
                                m_cache.Material(ref p, materialIdx);
                            }
                            iterator2.MoveNext();
                        }
                    }
                }
            }
        }

        private static unsafe void RemoveSmallVoxelsUsingChachedVoxels()
        {
            Vector3I vectori;
            Vector3I vectori2 = m_cache.Size3D;
            Vector3I max = vectori2 - 1;
            vectori.X = 0;
            goto TR_001E;
        TR_0003:
            int* numPtr4 = (int*) ref vectori.Z;
            numPtr4[0]++;
        TR_0018:
            while (true)
            {
                if (vectori.Z < vectori2.Z)
                {
                    bool flag;
                    int num = m_cache.Content(ref vectori);
                    if (num <= 0)
                    {
                        goto TR_0003;
                    }
                    else if (num >= 0x7f)
                    {
                        goto TR_0003;
                    }
                    else
                    {
                        Vector3I vectori4;
                        Vector3I result = vectori - 1;
                        Vector3I vectori6 = (Vector3I) (vectori + 1);
                        Vector3I* vectoriPtr1 = (Vector3I*) ref result;
                        Vector3I.Clamp(ref (Vector3I) ref vectoriPtr1, ref Vector3I.Zero, ref max, out result);
                        Vector3I* vectoriPtr2 = (Vector3I*) ref vectori6;
                        Vector3I.Clamp(ref (Vector3I) ref vectoriPtr2, ref Vector3I.Zero, ref max, out vectori6);
                        flag = false;
                        vectori4.X = result.X;
                        while (true)
                        {
                            if (vectori4.X > vectori6.X)
                            {
                                break;
                            }
                            vectori4.Y = result.Y;
                            while (true)
                            {
                                if (vectori4.Y <= vectori6.Y)
                                {
                                    vectori4.Z = result.Z;
                                    while (true)
                                    {
                                        if (vectori4.Z <= vectori6.Z)
                                        {
                                            if (m_cache.Content(ref vectori4) < 0x7f)
                                            {
                                                int* numPtr1 = (int*) ref vectori4.Z;
                                                numPtr1[0]++;
                                                continue;
                                            }
                                            flag = true;
                                            break;
                                        }
                                        else
                                        {
                                            int* numPtr2 = (int*) ref vectori4.Y;
                                            numPtr2[0]++;
                                        }
                                        break;
                                    }
                                    continue;
                                }
                                else
                                {
                                    int* numPtr3 = (int*) ref vectori4.X;
                                    numPtr3[0]++;
                                }
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        m_cache.Content(ref vectori, 0);
                        m_cache.Material(ref vectori, 0xff);
                    }
                    goto TR_0003;
                }
                else
                {
                    int* numPtr5 = (int*) ref vectori.Y;
                    numPtr5[0]++;
                }
                break;
            }
        TR_001B:
            while (true)
            {
                if (vectori.Y >= vectori2.Y)
                {
                    int* numPtr6 = (int*) ref vectori.X;
                    numPtr6[0]++;
                    break;
                }
                vectori.Z = 0;
                goto TR_0018;
            }
        TR_001E:
            while (true)
            {
                if (vectori.X >= vectori2.X)
                {
                    return;
                }
                vectori.Y = 0;
                break;
            }
            goto TR_001B;
        }

        public static void RequestCutOutShape(IMyVoxelBase voxelMap, IMyVoxelShape voxelShape)
        {
            MyVoxelBase voxelbool = voxelMap as MyVoxelBase;
            MyShape shape = voxelShape as MyShape;
            if ((voxelbool != null) && (shape != null))
            {
                shape.SendCutOutRequest(voxelbool);
            }
        }

        public static void RequestFillInShape(IMyVoxelBase voxelMap, IMyVoxelShape voxelShape, byte materialIdx)
        {
            MyVoxelBase voxel = voxelMap as MyVoxelBase;
            MyShape shape = voxelShape as MyShape;
            if ((voxel != null) && (shape != null))
            {
                shape.SendFillRequest(voxel, materialIdx);
            }
        }

        public static void RequestPaintInShape(IMyVoxelBase voxelMap, IMyVoxelShape voxelShape, byte materialIdx)
        {
            MyVoxelBase voxel = voxelMap as MyVoxelBase;
            MyShape shape = voxelShape as MyShape;
            if ((voxel != null) && (shape != null))
            {
                shape.SendPaintRequest(voxel, materialIdx);
            }
        }

        public static void RequestRevertShape(IMyVoxelBase voxelMap, IMyVoxelShape voxelShape)
        {
            MyVoxelBase voxel = voxelMap as MyVoxelBase;
            MyShape shape = voxelShape as MyShape;
            if ((voxel != null) && (shape != null))
            {
                shape.SendRevertRequest(voxel);
            }
        }

        public static void RevertShape(MyVoxelBase voxelMap, MyShape shape)
        {
            using (voxelMap.Pin())
            {
                if (!voxelMap.MarkedForClose)
                {
                    Vector3I vectori;
                    Vector3I maxCorner;
                    Vector3I minCorner;
                    GetVoxelShapeDimensions(voxelMap, shape, out minCorner, out maxCorner, out vectori);
                    minCorner = Vector3I.Max(Vector3I.One, minCorner);
                    maxCorner = Vector3I.Max(minCorner, maxCorner - Vector3I.One);
                    voxelMap.Storage.DeleteRange(MyStorageDataTypeFlags.All, minCorner, maxCorner, false);
                    BoundingBoxD cutOutBox = shape.GetWorldBoundaries();
                    MySandboxGame.Static.Invoke(delegate {
                        if (voxelMap.Storage != null)
                        {
                            voxelMap.Storage.NotifyChanged(minCorner, maxCorner, MyStorageDataTypeFlags.All);
                            NotifyVoxelChanged(MyVoxelBase.OperationType.Revert, voxelMap, ref cutOutBox);
                        }
                    }, "RevertShape notify");
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ClampingInfo
        {
            public bool X;
            public bool Y;
            public bool Z;
            public ClampingInfo(bool X, bool Y, bool Z)
            {
                this.X = X;
                this.Y = Y;
                this.Z = Z;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CutOutSphere : IVoxelOperator
        {
            public float RadSq;
            public Vector3D Center;
            public bool Changed;
            public VoxelOperatorFlags Flags =>
                VoxelOperatorFlags.Default;
            public void Op(ref Vector3I pos, MyStorageDataTypeEnum dataType, ref byte content)
            {
                if (content != 0)
                {
                    Vector3D vectord = (Vector3D) pos;
                    if (Vector3D.DistanceSquared(this.Center, vectord) < this.RadSq)
                    {
                        this.Changed = true;
                        content = 0;
                    }
                }
            }
        }
    }
}

