namespace Sandbox.Game.World.Generator
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.Noise;
    using VRageMath;

    public class MyProceduralPlanetCellGenerator : MyProceduralWorldModule
    {
        public const int MOON_SIZE_MIN_LIMIT = 0xfa0;
        public const int MOON_SIZE_MAX_LIMIT = 0x7530;
        public const int PLANET_SIZE_MIN_LIMIT = 0x1f40;
        public const int PLANET_SIZE_MAX_LIMIT = 0x1d4c0;
        internal readonly float PLANET_SIZE_MIN;
        internal readonly float PLANET_SIZE_MAX;
        internal const int MOONS_MAX = 3;
        internal readonly float MOON_SIZE_MIN;
        internal readonly float MOON_SIZE_MAX;
        internal const int MOON_DISTANCE_MIN = 0xfa0;
        internal const int MOON_DISTANCE_MAX = 0x7d00;
        internal const double MOON_DENSITY = 0.0;
        internal const int FALLOFF = 0x3e80;
        internal const double GRAVITY_SIZE_MULTIPLIER = 1.1;
        internal readonly double OBJECT_SEED_RADIUS;
        private List<BoundingBoxD> m_tmpClusterBoxes;
        private List<MyVoxelBase> m_tmpVoxelMapsList;

        public MyProceduralPlanetCellGenerator(int seed, double density, float planetSizeMax, float planetSizeMin, float moonSizeMax, float moonSizeMin, MyProceduralWorldModule parent = null) : base(2048000.0, 250, seed, ((density + 1.0) / 2.0) - 1.0, parent)
        {
            this.m_tmpClusterBoxes = new List<BoundingBoxD>(4);
            this.m_tmpVoxelMapsList = new List<MyVoxelBase>();
            if (planetSizeMax < planetSizeMin)
            {
                float single1 = planetSizeMax;
                planetSizeMax = planetSizeMin;
                planetSizeMin = single1;
            }
            this.PLANET_SIZE_MAX = MathHelper.Clamp(planetSizeMax, 8000f, 120000f);
            this.PLANET_SIZE_MIN = MathHelper.Clamp(planetSizeMin, 8000f, planetSizeMax);
            if (moonSizeMax < moonSizeMin)
            {
                float single2 = moonSizeMax;
                moonSizeMax = moonSizeMin;
                moonSizeMin = single2;
            }
            this.MOON_SIZE_MAX = MathHelper.Clamp(moonSizeMax, 4000f, 30000f);
            this.MOON_SIZE_MIN = MathHelper.Clamp(moonSizeMin, 4000f, moonSizeMax);
            this.OBJECT_SEED_RADIUS = ((((double) this.PLANET_SIZE_MAX) / 2.0) * 1.1) + (2.0 * (((((double) this.MOON_SIZE_MAX) / 2.0) * 1.1) + 64000.0));
            base.AddDensityFunctionFilled(new MyInfiniteDensityFunction(MyRandom.Instance, 0.001));
        }

        protected override void CloseObjectSeed(MyObjectSeed objectSeed)
        {
            IMyAsteroidFieldDensityFunction userData = objectSeed.UserData as IMyAsteroidFieldDensityFunction;
            if (userData != null)
            {
                base.ChildrenRemoveDensityFunctionRemoved(userData);
            }
            MyGamePruningStructure.GetAllVoxelMapsInBox(ref objectSeed.BoundingVolume, this.m_tmpVoxelMapsList);
            string str = $"{objectSeed.Params.Type}_{objectSeed.CellId.X}_{objectSeed.CellId.Y}_{objectSeed.CellId.Z}_{objectSeed.Params.Index}_{objectSeed.Params.Seed}";
            foreach (MyVoxelBase base2 in this.m_tmpVoxelMapsList)
            {
                if (base2.StorageName == str)
                {
                    if (!base2.Save)
                    {
                        base2.Close();
                    }
                    break;
                }
            }
            this.m_tmpVoxelMapsList.Clear();
        }

        private void GenerateObject(MyProceduralCell cell, MyObjectSeed objectSeed, ref int index, MyRandom random, IMyModule densityFunctionFilled, IMyModule densityFunctionRemoved)
        {
            cell.AddObject(objectSeed);
            IMyAsteroidFieldDensityFunction userData = objectSeed.UserData as IMyAsteroidFieldDensityFunction;
            if (userData != null)
            {
                base.ChildrenAddDensityFunctionRemoved(userData);
            }
            MyObjectSeedType type = objectSeed.Params.Type;
            if (type != MyObjectSeedType.Empty)
            {
                if (type == MyObjectSeedType.Planet)
                {
                    this.m_tmpClusterBoxes.Add(objectSeed.BoundingVolume);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3D randomDirection = MyProceduralWorldGenerator.GetRandomDirection(random);
                        double size = MathHelper.Lerp((double) this.MOON_SIZE_MIN, (double) this.MOON_SIZE_MAX, random.NextDouble());
                        double num3 = MathHelper.Lerp(4000.0, 32000.0, random.NextDouble());
                        BoundingBoxD boundingVolume = objectSeed.BoundingVolume;
                        Vector3D halfExtents = boundingVolume.HalfExtents;
                        Vector3D position = objectSeed.BoundingVolume.Center + (randomDirection * ((size + (halfExtents.Length() * 2.0)) + num3));
                        if (densityFunctionFilled.GetValue(position.X, position.Y, position.Z) < 0.0)
                        {
                            MyObjectSeed seed = new MyObjectSeed(cell, position, size) {
                                Params = { 
                                    Seed = random.Next(),
                                    Type = MyObjectSeedType.Moon
                                }
                            };
                            int num4 = index;
                            index = num4 + 1;
                            seed.Params.Index = num4;
                            seed.UserData = new MySphereDensityFunction(position, ((((double) this.MOON_SIZE_MAX) / 2.0) * 1.1) + 16000.0, 16000.0);
                            bool flag = false;
                            foreach (BoundingBoxD xd2 in this.m_tmpClusterBoxes)
                            {
                                boundingVolume = seed.BoundingVolume;
                                if (flag |= boundingVolume.Intersects(xd2))
                                {
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                this.m_tmpClusterBoxes.Add(seed.BoundingVolume);
                                this.GenerateObject(cell, seed, ref index, random, densityFunctionFilled, densityFunctionRemoved);
                            }
                        }
                    }
                    this.m_tmpClusterBoxes.Clear();
                }
                else if (type != MyObjectSeedType.Moon)
                {
                    throw new InvalidBranchException();
                }
            }
        }

        public override void GenerateObjects(List<MyObjectSeed> objectsList, HashSet<MyObjectSeedParams> existingObjectsSeeds)
        {
            foreach (MyObjectSeed seed in objectsList)
            {
                if (!seed.Params.Generated)
                {
                    seed.Params.Generated = true;
                    using (MyRandom.Instance.PushSeed(base.GetObjectIdSeed(seed)))
                    {
                        MyGamePruningStructure.GetAllVoxelMapsInBox(ref seed.BoundingVolume, this.m_tmpVoxelMapsList);
                        string str = $"{seed.Params.Type}_{seed.CellId.X}_{seed.CellId.Y}_{seed.CellId.Z}_{seed.Params.Index}_{seed.Params.Seed}";
                        bool flag = false;
                        using (List<MyVoxelBase>.Enumerator enumerator2 = this.m_tmpVoxelMapsList.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                if (enumerator2.Current.StorageName == str)
                                {
                                    existingObjectsSeeds.Add(seed.Params);
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        this.m_tmpVoxelMapsList.Clear();
                        bool flag1 = flag;
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
                Vector3D pos = new Vector3D(instance.NextDouble(), instance.NextDouble(), instance.NextDouble());
                pos = (((pos * ((base.CELL_SIZE - (2.0 * this.OBJECT_SEED_RADIUS)) / base.CELL_SIZE)) + (this.OBJECT_SEED_RADIUS / base.CELL_SIZE)) + cellId) * base.CELL_SIZE;
                if (MyEntities.IsInsideWorld(pos) && (cellDensityFunctionFilled.GetValue(pos.X, pos.Y, pos.Z) < base.m_objectDensity))
                {
                    MyObjectSeed objectSeed = new MyObjectSeed(cell, pos, MathHelper.Lerp((double) this.PLANET_SIZE_MIN, (double) this.PLANET_SIZE_MAX, instance.NextDouble())) {
                        Params = { 
                            Type = MyObjectSeedType.Planet,
                            Seed = instance.Next(),
                            Index = 0
                        },
                        UserData = new MySphereDensityFunction(pos, ((((double) this.PLANET_SIZE_MAX) / 2.0) * 1.1) + 16000.0, 16000.0)
                    };
                    int index = 1;
                    this.GenerateObject(cell, objectSeed, ref index, instance, cellDensityFunctionFilled, cellDensityFunctionRemoved);
                }
            }
            return cell;
        }

        private long GetPlanetEntityId(MyObjectSeed objectSeed)
        {
            Vector3I cellId = objectSeed.CellId;
            return ((((((((((((((Math.Abs(cellId.X) * 0x18dL) ^ Math.Abs(cellId.Y)) * 0x18dL) ^ Math.Abs(cellId.Z)) * 0x18dL) ^ (Math.Sign(cellId.X) + 240)) * 0x18dL) ^ (Math.Sign(cellId.Y) + 0x138)) * 0x18dL) ^ (Math.Sign(cellId.Z) + 0x1ce)) * 0x18dL) ^ (objectSeed.Params.Index * 0x1001fffL)) & 0xffffffffffffffL) | 0x700000000000000L);
        }

        private static Vector3I GetPlanetVoxelSize(double size) => 
            new Vector3I(Math.Max(0x40, (int) Math.Ceiling(size)));
    }
}

