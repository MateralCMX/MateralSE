namespace Sandbox.Game.World.Generator
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Noise;
    using VRage.Utils;
    using VRageMath;

    public class MyProceduralAsteroidCellGenerator : MyProceduralWorldModule
    {
        private MyAsteroidGeneratorDefinition m_data;
        private double m_seedTypeProbabilitySum;
        private double m_seedClusterTypeProbabilitySum;
        private bool m_isClosingEntities;
        private List<MyVoxelBase> m_tmpVoxelMapsList;
        private List<BoundingBoxD> m_tmpClusterBoxes;

        public MyProceduralAsteroidCellGenerator(int seed, double density, MyProceduralWorldModule parent = null) : base(GetSubCellInfo(), 1, seed, density, parent)
        {
            this.m_tmpVoxelMapsList = new List<MyVoxelBase>();
            this.m_data = GetData();
            base.AddDensityFunctionFilled(new MyInfiniteDensityFunction(MyRandom.Instance, 0.003));
            this.m_seedTypeProbabilitySum = 0.0;
            foreach (double num in this.m_data.SeedTypeProbability.Values)
            {
                this.m_seedTypeProbabilitySum += num;
            }
            this.m_seedClusterTypeProbabilitySum = 0.0;
            foreach (double num2 in this.m_data.SeedClusterTypeProbability.Values)
            {
                this.m_seedClusterTypeProbabilitySum += num2;
            }
        }

        protected override void CloseObjectSeed(MyObjectSeed objectSeed)
        {
            switch (objectSeed.Params.Type)
            {
                case MyObjectSeedType.Empty:
                    return;

                case MyObjectSeedType.Asteroid:
                case MyObjectSeedType.AsteroidCluster:
                {
                    MyGamePruningStructure.GetAllVoxelMapsInBox(ref objectSeed.BoundingVolume, this.m_tmpVoxelMapsList);
                    string str = $"Asteroid_{objectSeed.CellId.X}_{objectSeed.CellId.Y}_{objectSeed.CellId.Z}_{objectSeed.Params.Index}_{objectSeed.Params.Seed}";
                    foreach (MyVoxelBase base2 in this.m_tmpVoxelMapsList)
                    {
                        if (base2.StorageName == str)
                        {
                            if (!base2.Save)
                            {
                                this.m_isClosingEntities = true;
                                base2.Close();
                                this.m_isClosingEntities = false;
                            }
                            break;
                        }
                    }
                    this.m_tmpVoxelMapsList.Clear();
                    return;
                }
                case MyObjectSeedType.EncounterAlone:
                case MyObjectSeedType.EncounterSingle:
                    MyEncounterGenerator.Static.RemoveEncounter(objectSeed.BoundingVolume, objectSeed.Params.Seed);
                    return;
            }
            throw new InvalidBranchException();
        }

        private static MatrixD CreateAsteroidRotation(MyRandom random, Vector3D offset, Vector3I storageSize)
        {
            MatrixD xd = MatrixD.CreateTranslation(offset + (storageSize / 2));
            Matrix matrix = (Matrix.CreateRotationZ((float) ((random.NextFloat() * 3.1415926535897931) * 2.0)) * Matrix.CreateRotationX((float) ((random.NextFloat() * 3.1415926535897931) * 2.0))) * Matrix.CreateRotationY((float) ((random.NextFloat() * 3.1415926535897931) * 2.0));
            return ((MatrixD.CreateTranslation(new Vector3((Vector3I) (storageSize / 2))) * matrix) * xd);
        }

        private void GenerateObject(MyProceduralCell cell, MyObjectSeed objectSeed, ref int index, MyRandom random, IMyModule densityFunctionFilled, IMyModule densityFunctionRemoved)
        {
            if (this.m_data.UseGeneratorSeed && (objectSeed.Params.GeneratorSeed == 0))
            {
                objectSeed.Params.GeneratorSeed = random.Next();
            }
            if (this.m_data.UseClusterDefAsAsteroid)
            {
                cell.AddObject(objectSeed);
            }
            switch (objectSeed.Params.Type)
            {
                case MyObjectSeedType.Empty:
                    break;

                case MyObjectSeedType.Asteroid:
                case MyObjectSeedType.EncounterAlone:
                case MyObjectSeedType.EncounterSingle:
                    if (this.m_data.UseClusterDefAsAsteroid)
                    {
                        break;
                    }
                    cell.AddObject(objectSeed);
                    return;

                case MyObjectSeedType.AsteroidCluster:
                {
                    if (this.m_data.UseClusterDefAsAsteroid)
                    {
                        objectSeed.Params.Type = MyObjectSeedType.Asteroid;
                    }
                    using (MyUtils.ReuseCollection<BoundingBoxD>(ref this.m_tmpClusterBoxes))
                    {
                        int num = this.m_data.UseGeneratorSeed ? random.Next() : 0;
                        double objectMaxDistanceInClusterMin = this.m_data.ObjectMaxDistanceInClusterMin;
                        if (this.m_data.UseClusterVariableSize)
                        {
                            objectMaxDistanceInClusterMin = MathHelper.Lerp((double) this.m_data.ObjectMaxDistanceInClusterMin, (double) this.m_data.ObjectMaxDistanceInClusterMax, random.NextDouble());
                        }
                        int num3 = 0;
                        goto TR_0018;
                    TR_0005:
                        num3++;
                    TR_0018:
                        while (true)
                        {
                            double num1;
                            if (num3 >= this.m_data.ObjectMaxInCluster)
                            {
                                break;
                            }
                            Vector3D randomDirection = MyProceduralWorldGenerator.GetRandomDirection(random);
                            double clusterObjectSize = this.GetClusterObjectSize(random.NextDouble());
                            double num5 = MathHelper.Lerp((double) this.m_data.ObjectMinDistanceInCluster, objectMaxDistanceInClusterMin, random.NextDouble());
                            if (!this.m_data.ClusterDispersionAbsolute)
                            {
                                num1 = (clusterObjectSize + (objectSeed.BoundingVolume.HalfExtents.Length() * 2.0)) + num5;
                            }
                            else
                            {
                                num1 = num5;
                            }
                            Vector3D position = objectSeed.BoundingVolume.Center + (randomDirection * num1);
                            double num7 = -1.0;
                            if (densityFunctionRemoved != null)
                            {
                                num7 = densityFunctionRemoved.GetValue(position.X, position.Y, position.Z);
                                if (num7 <= -1.0)
                                {
                                    goto TR_0005;
                                }
                            }
                            double num8 = densityFunctionFilled.GetValue(position.X, position.Y, position.Z);
                            if (((densityFunctionRemoved == null) || (num7 >= num8)) && (num8 < this.m_data.ObjectDensityCluster))
                            {
                                MyObjectSeed seed = new MyObjectSeed(cell, position, clusterObjectSize) {
                                    Params = { Seed = random.Next() }
                                };
                                int num9 = index;
                                index = num9 + 1;
                                seed.Params.Index = num9;
                                seed.Params.Type = this.GetClusterSeedType(random.NextDouble());
                                seed.Params.GeneratorSeed = num;
                                BoundingBoxD hitBox = seed.BoundingVolume;
                                if (this.m_data.AllowPartialClusterObjectOverlap)
                                {
                                    Vector3D center = hitBox.Center;
                                    Vector3D halfExtents = hitBox.HalfExtents;
                                    hitBox = new BoundingBoxD(center - (halfExtents * 0.30000001192092896), center + (halfExtents * 0.30000001192092896));
                                }
                                if (this.m_tmpClusterBoxes.All<BoundingBoxD>(box => !hitBox.Intersects(box)))
                                {
                                    this.m_tmpClusterBoxes.Add(hitBox);
                                    this.GenerateObject(cell, seed, ref index, random, densityFunctionFilled, densityFunctionRemoved);
                                }
                            }
                            goto TR_0005;
                        }
                        break;
                    }
                }
                default:
                    throw new InvalidBranchException();
            }
        }

        public override void GenerateObjects(List<MyObjectSeed> objectsList, HashSet<MyObjectSeedParams> existingObjectsSeeds)
        {
            foreach (MyObjectSeed seed in objectsList)
            {
                if (seed.Params.Generated)
                {
                    continue;
                }
                if (!existingObjectsSeeds.Contains(seed.Params))
                {
                    seed.Params.Generated = true;
                    using (MyRandom.Instance.PushSeed(base.GetObjectIdSeed(seed)))
                    {
                        MyVoxelMap voxelMap;
                        switch (seed.Params.Type)
                        {
                            case MyObjectSeedType.Empty:
                            {
                                continue;
                            }
                            case MyObjectSeedType.Asteroid:
                            {
                                MyGamePruningStructure.GetAllVoxelMapsInBox(ref seed.BoundingVolume, this.m_tmpVoxelMapsList);
                                string storageName = $"Asteroid_{seed.CellId.X}_{seed.CellId.Y}_{seed.CellId.Z}_{seed.Params.Index}_{seed.Params.Seed}";
                                bool flag = false;
                                using (List<MyVoxelBase>.Enumerator enumerator2 = this.m_tmpVoxelMapsList.GetEnumerator())
                                {
                                    while (enumerator2.MoveNext())
                                    {
                                        if (enumerator2.Current.StorageName == storageName)
                                        {
                                            existingObjectsSeeds.Add(seed.Params);
                                            flag = true;
                                            break;
                                        }
                                    }
                                }
                                if (flag)
                                {
                                    goto TR_0004;
                                }
                                else
                                {
                                    int? generator = null;
                                    MyStorageBase storage = new MyOctreeStorage(MyCompositeShapeProvider.CreateAsteroidShape(seed.Params.Seed, seed.Size, this.m_data.UseGeneratorSeed ? seed.Params.GeneratorSeed : 0, generator), GetAsteroidVoxelSize((double) seed.Size));
                                    Vector3D offset = seed.BoundingVolume.Center - (MathHelper.GetNearestBiggerPowerOfTwo(seed.Size) / 2);
                                    if (this.m_data.RotateAsteroids)
                                    {
                                        using (MyRandom.Instance.PushSeed(seed.Params.Seed))
                                        {
                                            voxelMap = MyWorldGenerator.AddVoxelMap(storageName, storage, CreateAsteroidRotation(MyRandom.Instance, offset, storage.Size), GetAsteroidEntityId(storageName), false, true);
                                            break;
                                        }
                                    }
                                    voxelMap = MyWorldGenerator.AddVoxelMap(storageName, storage, offset, GetAsteroidEntityId(storageName));
                                }
                                break;
                            }
                            case MyObjectSeedType.EncounterAlone:
                            case MyObjectSeedType.EncounterSingle:
                            {
                                MyEncounterGenerator.Static.PlaceEncounterToWorld(seed.BoundingVolume, seed.Params.Seed);
                                continue;
                            }
                            default:
                                throw new InvalidBranchException();
                        }
                        if (voxelMap != null)
                        {
                            voxelMap.Save = false;
                            MyVoxelBase.StorageChanged OnStorageRangeChanged = null;
                            OnStorageRangeChanged = delegate (MyVoxelBase voxel, Vector3I minVoxelChanged, Vector3I maxVoxelChanged, MyStorageDataTypeFlags changedData) {
                                voxelMap.Save = true;
                                voxelMap.RangeChanged -= OnStorageRangeChanged;
                            };
                            voxelMap.RangeChanged += OnStorageRangeChanged;
                            MyObjectSeedParams voxelParams = seed.Params;
                            if (Sync.IsServer)
                            {
                                voxelMap.OnMarkForClose += delegate (MyEntity voxel) {
                                    if (!this.m_isClosingEntities)
                                    {
                                        EndpointId targetEndpoint = new EndpointId();
                                        Vector3D? position = null;
                                        MyMultiplayer.RaiseStaticEvent<MyObjectSeedParams>(x => new Action<MyObjectSeedParams>(MyProceduralWorldGenerator.AddExistingObjectsSeed), voxelParams, targetEndpoint, position);
                                    }
                                };
                            }
                        }
                    TR_0004:
                        this.m_tmpVoxelMapsList.Clear();
                    }
                }
            }
        }

        protected override MyProceduralCell GenerateProceduralCell(ref Vector3I cellId)
        {
            MyProceduralCell cell = new MyProceduralCell(cellId, base.CELL_SIZE);
            IMyModule cellDensityFunctionFilled = base.GetCellDensityFunctionFilled(cell.BoundingVolume);
            if (cellDensityFunctionFilled == null)
            {
                return null;
            }
            IMyModule cellDensityFunctionRemoved = base.GetCellDensityFunctionRemoved(cell.BoundingVolume);
            MyRandom instance = MyRandom.Instance;
            using (instance.PushSeed(base.GetCellSeed(ref cellId)))
            {
                int index = 0;
                Vector3I zero = Vector3I.Zero;
                Vector3I end = new Vector3I(this.m_data.SubCells - 1);
                Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref Vector3I.Zero, ref end);
                while (true)
                {
                    while (true)
                    {
                        if (iterator.IsValid())
                        {
                            Vector3D pos = new Vector3D(instance.NextDouble(), instance.NextDouble(), instance.NextDouble());
                            pos = ((pos + (zero / ((double) this.m_data.SubcellSize))) + cellId) * base.CELL_SIZE;
                            if (MyEntities.IsInsideWorld(pos))
                            {
                                double num3 = -1.0;
                                if (cellDensityFunctionRemoved != null)
                                {
                                    num3 = cellDensityFunctionRemoved.GetValue(pos.X, pos.Y, pos.Z);
                                    if (num3 <= -1.0)
                                    {
                                        break;
                                    }
                                }
                                double num4 = cellDensityFunctionFilled.GetValue(pos.X, pos.Y, pos.Z);
                                if (((cellDensityFunctionRemoved == null) || (num3 >= num4)) && (num4 < base.m_objectDensity))
                                {
                                    MyObjectSeed objectSeed = new MyObjectSeed(cell, pos, this.GetObjectSize(instance.NextDouble())) {
                                        Params = { 
                                            Type = this.GetSeedType(instance.NextDouble()),
                                            Seed = instance.Next()
                                        }
                                    };
                                    index++;
                                    objectSeed.Params.Index = index;
                                    this.GenerateObject(cell, objectSeed, ref index, instance, cellDensityFunctionFilled, cellDensityFunctionRemoved);
                                }
                            }
                        }
                        else
                        {
                            return cell;
                        }
                        break;
                    }
                    iterator.GetNext(out zero);
                }
            }
        }

        public static long GetAsteroidEntityId(string storageName) => 
            ((storageName.GetHashCode64() & 0xffffffffffffffL) | 0x600000000000000L);

        private static Vector3I GetAsteroidVoxelSize(double asteroidRadius) => 
            new Vector3I(Math.Max(0x40, (int) Math.Ceiling(asteroidRadius)));

        private double GetClusterObjectSize(double noise)
        {
            if (this.m_data.UseLinearPowOfTwoSizeDistribution)
            {
                return (double) (1 << (((int) Math.Round(MathHelper.Lerp(Math.Log((double) MathHelper.GetNearestBiggerPowerOfTwo(this.m_data.ObjectSizeMinCluster), 2.0), Math.Log((double) MathHelper.GetNearestBiggerPowerOfTwo(this.m_data.ObjectSizeMaxCluster), 2.0), noise))) & 0x1f));
            }
            return (this.m_data.ObjectSizeMinCluster + (noise * (this.m_data.ObjectSizeMaxCluster - this.m_data.ObjectSizeMinCluster)));
        }

        private MyObjectSeedType GetClusterSeedType(double d)
        {
            d *= this.m_seedClusterTypeProbabilitySum;
            using (Dictionary<MyObjectSeedType, double>.Enumerator enumerator = this.m_data.SeedClusterTypeProbability.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<MyObjectSeedType, double> current = enumerator.Current;
                    if (current.Value < d)
                    {
                        d -= current.Value;
                        continue;
                    }
                    return current.Key;
                }
            }
            return MyObjectSeedType.Asteroid;
        }

        private static MyAsteroidGeneratorDefinition GetData()
        {
            MyAsteroidGeneratorDefinition definition = null;
            int voxelGeneratorVersion = MySession.Static.Settings.VoxelGeneratorVersion;
            foreach (MyAsteroidGeneratorDefinition definition2 in MyDefinitionManager.Static.GetAsteroidGeneratorDefinitions().Values)
            {
                if (definition2.Version == voxelGeneratorVersion)
                {
                    definition = definition2;
                    break;
                }
            }
            if (definition == null)
            {
                MyLog.Default.WriteLine("Generator of version " + voxelGeneratorVersion + "not found!");
                foreach (MyAsteroidGeneratorDefinition definition3 in MyDefinitionManager.Static.GetAsteroidGeneratorDefinitions().Values)
                {
                    if (definition != null)
                    {
                        if (definition3.Version <= voxelGeneratorVersion)
                        {
                            continue;
                        }
                        if ((definition.Version >= voxelGeneratorVersion) && (definition3.Version >= definition.Version))
                        {
                            continue;
                        }
                    }
                    definition = definition3;
                }
            }
            return definition;
        }

        private double GetObjectSize(double noise)
        {
            if (this.m_data.UseLinearPowOfTwoSizeDistribution)
            {
                return (double) (1 << (((int) Math.Round(MathHelper.Lerp(Math.Log((double) MathHelper.GetNearestBiggerPowerOfTwo(this.m_data.ObjectSizeMin), 2.0), Math.Log((double) MathHelper.GetNearestBiggerPowerOfTwo(this.m_data.ObjectSizeMax), 2.0), noise))) & 0x1f));
            }
            return (this.m_data.ObjectSizeMin + ((noise * noise) * (this.m_data.ObjectSizeMax - this.m_data.ObjectSizeMin)));
        }

        private MyObjectSeedType GetSeedType(double d)
        {
            d *= this.m_seedTypeProbabilitySum;
            using (Dictionary<MyObjectSeedType, double>.Enumerator enumerator = this.m_data.SeedTypeProbability.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<MyObjectSeedType, double> current = enumerator.Current;
                    if (current.Value < d)
                    {
                        d -= current.Value;
                        continue;
                    }
                    return current.Key;
                }
            }
            return MyObjectSeedType.Asteroid;
        }

        private static double GetSubCellInfo()
        {
            MyAsteroidGeneratorDefinition data = GetData();
            return (double) (data.SubCells * data.SubcellSize);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyProceduralAsteroidCellGenerator.<>c <>9 = new MyProceduralAsteroidCellGenerator.<>c();
            public static Func<IMyEventOwner, Action<MyObjectSeedParams>> <>9__8_2;

            internal Action<MyObjectSeedParams> <GenerateObjects>b__8_2(IMyEventOwner x) => 
                new Action<MyObjectSeedParams>(MyProceduralWorldGenerator.AddExistingObjectsSeed);
        }
    }
}

