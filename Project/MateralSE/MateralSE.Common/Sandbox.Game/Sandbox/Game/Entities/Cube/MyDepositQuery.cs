namespace Sandbox.Game.Entities.Cube
{
    using ParallelTasks;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Voxels;
    using VRage.Generics;
    using VRage.Profiler;
    using VRage.Voxels;
    using VRageMath;

    internal class MyDepositQuery : IPrioritizedWork, IWork
    {
        private static readonly MyObjectsPool<MyDepositQuery> m_instancePool = new MyObjectsPool<MyDepositQuery>(0x10, null);
        [ThreadStatic]
        private static MyStorageData m_cache;
        [ThreadStatic]
        private static MaterialPositionData[] m_materialData;
        public MyOreDetectorComponent OreDetectionComponent;
        private List<MyEntityOreDeposit> m_result;
        private List<Vector3I> m_emptyCells;
        private readonly Action m_onComplete;

        public MyDepositQuery()
        {
            this.m_onComplete = new Action(this.OnComplete);
        }

        private void OnComplete()
        {
            this.CompletionCallback(this.m_result, this.m_emptyCells, this.OreDetectionComponent);
            this.CompletionCallback = null;
            this.m_result = null;
            m_instancePool.Deallocate(this);
        }

        unsafe void IWork.DoWork(WorkData workData)
        {
            try
            {
                if (this.m_result == null)
                {
                    this.m_result = new List<MyEntityOreDeposit>();
                    this.m_emptyCells = new List<Vector3I>();
                }
                MyStorageData cache = Cache;
                cache.Resize(new Vector3I(8));
                IMyStorage storage = this.VoxelMap.Storage;
                if (storage != null)
                {
                    using (StoragePin pin = storage.Pin())
                    {
                        if (pin.Valid)
                        {
                            Vector3I vectori;
                            vectori.Z = this.Min.Z;
                            while (vectori.Z <= this.Max.Z)
                            {
                                vectori.Y = this.Min.Y;
                                while (true)
                                {
                                    if (vectori.Y > this.Max.Y)
                                    {
                                        int* numPtr3 = (int*) ref vectori.Z;
                                        numPtr3[0]++;
                                        break;
                                    }
                                    vectori.X = this.Min.X;
                                    while (true)
                                    {
                                        if (vectori.X > this.Max.X)
                                        {
                                            int* numPtr2 = (int*) ref vectori.Y;
                                            numPtr2[0]++;
                                            break;
                                        }
                                        this.ProcessCell(cache, storage, vectori, this.DetectorId);
                                        int* numPtr1 = (int*) ref vectori.X;
                                        numPtr1[0]++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
            }
        }

        private unsafe void ProcessCell(MyStorageData cache, IMyStorage storage, Vector3I cell, long detectorId)
        {
            Vector3I lodVoxelRangeMin = cell << 3;
            Vector3I lodVoxelRangeMax = (Vector3I) (lodVoxelRangeMin + 7);
            storage.ReadRange(cache, MyStorageDataTypeFlags.Content, 2, lodVoxelRangeMin, lodVoxelRangeMax);
            if (cache.ContainsVoxelsAboveIsoLevel())
            {
                Vector3I vectori3;
                MyVoxelRequestFlags preciseOrePositions = MyVoxelRequestFlags.PreciseOrePositions;
                storage.ReadRange(cache, MyStorageDataTypeFlags.Material, 2, lodVoxelRangeMin, lodVoxelRangeMax, ref preciseOrePositions);
                MaterialPositionData[] materialData = MaterialData;
                vectori3.Z = 0;
                while (vectori3.Z < 8)
                {
                    vectori3.Y = 0;
                    while (true)
                    {
                        if (vectori3.Y >= 8)
                        {
                            int* numPtr4 = (int*) ref vectori3.Z;
                            numPtr4[0]++;
                            break;
                        }
                        vectori3.X = 0;
                        while (true)
                        {
                            if (vectori3.X >= 8)
                            {
                                int* numPtr3 = (int*) ref vectori3.Y;
                                numPtr3[0]++;
                                break;
                            }
                            int linearIdx = cache.ComputeLinear(ref vectori3);
                            if (cache.Content(linearIdx) > 0x7f)
                            {
                                byte index = cache.Material(linearIdx);
                                Vector3D vectord = ((vectori3 + lodVoxelRangeMin) * 4f) + 2f;
                                Vector3* vectorPtr1 = (Vector3*) ref materialData[index].Sum;
                                vectorPtr1[0] += vectord;
                                int* numPtr1 = (int*) ref materialData[index].Count;
                                numPtr1[0]++;
                            }
                            int* numPtr2 = (int*) ref vectori3.X;
                            numPtr2[0]++;
                        }
                    }
                }
                MyEntityOreDeposit item = null;
                for (int i = 0; i < materialData.Length; i++)
                {
                    if (materialData[i].Count != 0)
                    {
                        MyVoxelMaterialDefinition voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition((byte) i);
                        if ((voxelMaterialDefinition != null) && voxelMaterialDefinition.IsRare)
                        {
                            if (item == null)
                            {
                                item = new MyEntityOreDeposit(this.VoxelMap, cell, detectorId);
                            }
                            MyEntityOreDeposit.Data data = new MyEntityOreDeposit.Data {
                                Material = voxelMaterialDefinition,
                                AverageLocalPosition = (Vector3) Vector3D.Transform((materialData[i].Sum / ((float) materialData[i].Count)) - this.VoxelMap.SizeInMetresHalf, Quaternion.CreateFromRotationMatrix(this.VoxelMap.WorldMatrix))
                            };
                            item.Materials.Add(data);
                        }
                    }
                }
                if (item != null)
                {
                    this.m_result.Add(item);
                }
                else
                {
                    this.m_emptyCells.Add(cell);
                }
                Array.Clear(materialData, 0, materialData.Length);
            }
        }

        public static void Start(Vector3I min, Vector3I max, long detectorId, MyVoxelBase voxelMap, Action<List<MyEntityOreDeposit>, List<Vector3I>, MyOreDetectorComponent> completionCallback, MyOreDetectorComponent detectorComp)
        {
            MyDepositQuery item = null;
            m_instancePool.AllocateOrCreate(out item);
            if (item != null)
            {
                item.Min = min;
                item.Max = max;
                item.DetectorId = detectorId;
                item.VoxelMap = voxelMap;
                item.CompletionCallback = completionCallback;
                item.OreDetectionComponent = detectorComp;
                Parallel.Start(item, item.m_onComplete);
            }
        }

        private static MyStorageData Cache
        {
            get
            {
                if (m_cache == null)
                {
                    m_cache = new MyStorageData(MyStorageDataTypeFlags.All);
                }
                return m_cache;
            }
        }

        private static MaterialPositionData[] MaterialData
        {
            get
            {
                if (m_materialData == null)
                {
                    m_materialData = new MaterialPositionData[0x100];
                }
                return m_materialData;
            }
        }

        public Vector3I Min { get; set; }

        public Vector3I Max { get; set; }

        public MyVoxelBase VoxelMap { get; set; }

        public long DetectorId { get; set; }

        public Action<List<MyEntityOreDeposit>, List<Vector3I>, MyOreDetectorComponent> CompletionCallback { get; set; }

        WorkPriority IPrioritizedWork.Priority =>
            WorkPriority.VeryLow;

        WorkOptions IWork.Options =>
            Parallel.DefaultOptions.WithDebugInfo(MyProfiler.TaskType.Block, "OreDetector");

        [StructLayout(LayoutKind.Sequential)]
        private struct MaterialPositionData
        {
            public Vector3 Sum;
            public int Count;
        }
    }
}

