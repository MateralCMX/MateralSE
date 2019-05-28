namespace Sandbox.Game.World.Generator
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.Noise;
    using VRage.Utils;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MyCompositeShapes
    {
        private List<MyVoxelMaterialDefinition> m_coreMaterials;
        private List<MyVoxelMaterialDefinition> m_surfaceMaterials;
        private List<MyVoxelMaterialDefinition> m_depositMaterials;
        public static readonly MyCompositeShapeGeneratorDelegate[] AsteroidGenerators;
        private List<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>> m_primarySelections;
        private List<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>> m_secondarySelections;
        static MyCompositeShapes()
        {
            int[] numArray1 = new int[3];
            numArray1[1] = 1;
            numArray1[2] = 2;
            int[] numArray2 = new int[] { 3, 4 };
            AsteroidGenerators = (from x in numArray1 select MyTuple.Create<int, bool>(x, false)).Concat<MyTuple<int, bool>>((from x in numArray2 select MyTuple.Create<int, bool>(x, true))).Select<MyTuple<int, bool>, MyCompositeShapeGeneratorDelegate>(delegate (MyTuple<int, bool> info) {
                int version = info.Item1;
                bool combined = info.Item2;
                return delegate (int generatorSeed, int seed, float size) {
                    if (size == 0f)
                    {
                        size = MyUtils.GetRandomFloat(128f, 512f);
                    }
                    MyCompositeShapes shapes = new MyCompositeShapes(generatorSeed, seed, version);
                    using (MyRandom.Instance.PushSeed(seed))
                    {
                        return (!combined ? shapes.ProceduralGenerator(version, seed, size) : shapes.CombinedGenerator(version, seed, size));
                    }
                };
            }).ToArray<MyCompositeShapeGeneratorDelegate>();
        }

        private MyCompositeShapes(int generatorSeed, int asteroidSeed, int version)
        {
            this = new MyCompositeShapes();
            if (version > 2)
            {
                this.m_coreMaterials = new List<MyVoxelMaterialDefinition>();
                this.m_depositMaterials = new List<MyVoxelMaterialDefinition>();
                this.m_surfaceMaterials = new List<MyVoxelMaterialDefinition>();
                using (MyRandom.Instance.PushSeed(generatorSeed))
                {
                    MyRandom instance = MyRandom.Instance;
                    this.FillMaterials(version);
                    FilterKindDuplicates(this.m_coreMaterials, instance);
                    FilterKindDuplicates(this.m_depositMaterials, instance);
                    FilterKindDuplicates(this.m_surfaceMaterials, instance);
                    ProcessMaterialSpawnProbabilities(this.m_coreMaterials);
                    ProcessMaterialSpawnProbabilities(this.m_depositMaterials);
                    ProcessMaterialSpawnProbabilities(this.m_surfaceMaterials);
                    if (instance.Next(100) < 1)
                    {
                        this.MakeIceAsteroid(version, instance);
                    }
                    else if (version >= 4)
                    {
                        int maxCount = (instance.NextDouble() > 0.800000011920929) ? 4 : 2;
                        int num2 = (instance.NextDouble() > 0.40000000596046448) ? 2 : 1;
                        LimitMaterials(this.m_coreMaterials, maxCount, instance);
                        LimitMaterials(this.m_depositMaterials, maxCount, instance);
                        using (MyRandom.Instance.PushSeed(asteroidSeed))
                        {
                            LimitMaterials(this.m_coreMaterials, num2, instance);
                            LimitMaterials(this.m_depositMaterials, num2, instance);
                        }
                    }
                }
            }
        }

        private IMyCompositionInfoProvider CombinedGenerator(int version, int seed, float size)
        {
            MyCompositeShapeProvider.MyProceduralCompositeInfoProvider.ConstructionData data;
            MyVoxelMaterialDefinition definition;
            MyCompositeShapeOreDeposit[] depositArray;
            MyRandom instance = MyRandom.Instance;
            data.DefaultMaterial = null;
            data.Deposits = Array.Empty<MyCompositeShapeOreDeposit>();
            data.FilledShapes = new MyCsgShapeBase[0];
            IMyCompositeShape[] array = new IMyCompositeShape[6];
            this.FillSpan(instance, size, array.Span<IMyCompositeShape>(0, 1), MyDefinitionManager.Static.GetVoxelMapStorageDefinitionsForProceduralPrimaryAdditions(), true);
            size = ((MyOctreeStorage) array[0]).Size.AbsMax();
            float idealSize = size / 2f;
            float num3 = size / 2f;
            int num4 = 5 / ((size > 200f) ? 1 : 2);
            int num5 = 1;
            if (size <= 64f)
            {
                num4 = 0;
                num5 = 0;
            }
            IMyCompositeShape[] shapes = new IMyCompositeShape[num4];
            data.RemovedShapes = new MyCsgShapeBase[num5];
            this.FillSpan(instance, num3, array, MyDefinitionManager.Static.GetVoxelMapStorageDefinitionsForProceduralAdditions(), false);
            this.FillSpan(instance, idealSize, shapes, MyDefinitionManager.Static.GetVoxelMapStorageDefinitionsForProceduralRemovals(), false);
            this.TranslateShapes(shapes, size, instance);
            int? count = null;
            this.TranslateShapes(array.Span<IMyCompositeShape>(1, count), size, instance);
            if (size > 512f)
            {
                size /= 2f;
            }
            float num6 = size * 0.5f;
            float storageOffset = (MathHelper.GetNearestBiggerPowerOfTwo(size) * 0.5f) - num6;
            GetProceduralModules(seed, size, instance, out data.MacroModule, out data.DetailModule);
            GenerateProceduralAdditions(version, size, data.FilledShapes, instance, storageOffset);
            GenerateProceduralRemovals(version, size, data.RemovedShapes, instance, storageOffset);
            MyCompositeShapeProvider.MyCombinedCompositeInfoProvider shapeInfo = new MyCompositeShapeProvider.MyCombinedCompositeInfoProvider(ref data, array, shapes);
            this.GenerateMaterials(version, size, instance, data.FilledShapes, storageOffset, out definition, out depositArray, shapeInfo);
            shapeInfo.UpdateMaterials(definition, depositArray);
            return shapeInfo;
        }

        private void TranslateShapes(Span<IMyCompositeShape> array, float size, MyRandom random)
        {
            for (int i = 0; i < array.Count; i++)
            {
                int num2 = 0;
                MyStorageBase base2 = array[i] as MyStorageBase;
                if (base2 != null)
                {
                    num2 = base2.Size.AbsMax();
                }
                array[i] = new MyCompositeTranslateShape(array[i], CreateRandomPointInBox(random, size - num2));
            }
        }

        private void FillSpan(MyRandom random, float idealSize, Span<IMyCompositeShape> shapes, ListReader<MyVoxelMapStorageDefinition> voxelMaps, bool prefferOnlyBestFittingSize = false)
        {
            bool flag = false;
            int num = 0;
            while (true)
            {
                if (num < shapes.Count)
                {
                    if (shapes[num] != null)
                    {
                        num++;
                        continue;
                    }
                    flag = true;
                }
                if (!flag)
                {
                    return;
                }
                using (MyUtils.ReuseCollection<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>>(ref this.m_primarySelections))
                {
                    using (MyUtils.ReuseCollection<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>>(ref this.m_secondarySelections))
                    {
                        this.m_primarySelections.EnsureCapacity<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>>(voxelMaps.Count);
                        this.m_secondarySelections.EnsureCapacity<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>>(voxelMaps.Count);
                        int num2 = -2147483648;
                        int num3 = 0x7fffffff;
                        foreach (MyVoxelMapStorageDefinition definition in voxelMaps)
                        {
                            MyOctreeStorage storage = CreateAsteroidStorage(definition);
                            int num5 = storage.Size.AbsMax();
                            if (num5 > idealSize)
                            {
                                if (num5 > num3)
                                {
                                    continue;
                                }
                                if (num5 < num3)
                                {
                                    num3 = num5;
                                    this.m_secondarySelections.Clear();
                                }
                                this.m_secondarySelections.Add(MyTuple.Create<MyVoxelMapStorageDefinition, MyOctreeStorage>(definition, storage));
                                continue;
                            }
                            if (prefferOnlyBestFittingSize)
                            {
                                if (num5 < num2)
                                {
                                    continue;
                                }
                                if (num5 > num2)
                                {
                                    num2 = num5;
                                    this.m_primarySelections.Clear();
                                }
                            }
                            this.m_primarySelections.Add(MyTuple.Create<MyVoxelMapStorageDefinition, MyOctreeStorage>(definition, storage));
                        }
                        List<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>> source = (this.m_primarySelections.Count > 0) ? this.m_primarySelections : this.m_secondarySelections;
                        float num4 = source.Sum<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>>(x => x.Item1.SpawnProbability);
                        int num6 = 0;
                        goto TR_0011;
                    TR_0004:
                        num6++;
                    TR_0011:
                        while (true)
                        {
                            if (num6 >= shapes.Count)
                            {
                                break;
                            }
                            if (shapes[num6] == null)
                            {
                                float num7 = num4 * random.NextFloat();
                                using (List<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>>.Enumerator enumerator2 = source.GetEnumerator())
                                {
                                    while (true)
                                    {
                                        if (!enumerator2.MoveNext())
                                        {
                                            break;
                                        }
                                        MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage> current = enumerator2.Current;
                                        float spawnProbability = current.Item1.SpawnProbability;
                                        if (num7 >= spawnProbability)
                                        {
                                            num7 -= spawnProbability;
                                            continue;
                                        }
                                        shapes[num6] = current.Item2;
                                        goto TR_0004;
                                    }
                                }
                                shapes[num6] = source.MaxBy<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>>(x => x.Item1.SpawnProbability).Item2;
                            }
                            goto TR_0004;
                        }
                        break;
                    }
                }
            }
        }

        public static MyOctreeStorage CreateAsteroidStorage(MyVoxelMapStorageDefinition definition) => 
            ((MyOctreeStorage) MyStorageBase.LoadFromFile(Path.Combine(definition.Context.IsBaseGame ? MyFileSystem.ContentPath : definition.Context.ModPath, definition.StorageFile), null, true));

        private IMyCompositionInfoProvider ProceduralGenerator(int version, int seed, float size)
        {
            MyCompositeShapeProvider.MyProceduralCompositeInfoProvider.ConstructionData data;
            MyCsgShapeBase base2;
            MyRandom instance = MyRandom.Instance;
            data.FilledShapes = new MyCsgShapeBase[2];
            data.RemovedShapes = new MyCsgShapeBase[2];
            GetProceduralModules(seed, size, instance, out data.MacroModule, out data.DetailModule);
            float num2 = MathHelper.GetNearestBiggerPowerOfTwo(size) * 0.5f;
            float storageOffset = num2 - (size * 0.5f);
            int num4 = instance.Next() % 3;
            if (num4 == 0)
            {
                float secondaryRadius = ((instance.NextFloat() * 0.05f) + 0.1f) * size;
                base2 = new MyCsgTorus(new Vector3(num2), CreateRandomRotation(instance), ((instance.NextFloat() * 0.1f) + 0.2f) * size, secondaryRadius, ((instance.NextFloat() * 0.4f) + 0.4f) * secondaryRadius, (instance.NextFloat() * 0.8f) + 0.2f, (instance.NextFloat() * 0.6f) + 0.4f);
            }
            else
            {
                if (num4 != 1)
                {
                }
                base2 = new MyCsgSphere(new Vector3(num2), (((instance.NextFloat() * 0.1f) + 0.35f) * size) * ((version > 2) ? 0.8f : 1f), (((instance.NextFloat() * 0.05f) + 0.05f) * size) + 1f, (instance.NextFloat() * 0.8f) + 0.2f, (instance.NextFloat() * 0.6f) + 0.4f);
            }
            data.FilledShapes[0] = base2;
            GenerateProceduralAdditions(version, size, data.FilledShapes, instance, storageOffset);
            GenerateProceduralRemovals(version, size, data.RemovedShapes, instance, storageOffset);
            this.GenerateMaterials(version, size, instance, data.FilledShapes, storageOffset, out data.DefaultMaterial, out data.Deposits, null);
            return new MyCompositeShapeProvider.MyProceduralCompositeInfoProvider(ref data);
        }

        private static void GetProceduralModules(int seed, float size, MyRandom random, out IMyModule macroModule, out IMyModule detailModule)
        {
            macroModule = new MySimplexFast(seed, (double) (7f / size));
            int num = random.Next() & 1;
            if (num == 0)
            {
                detailModule = new MyRidgedMultifractalFast(MyNoiseQuality.Low, 1, seed, (double) ((random.NextFloat() * 0.09f) + 0.11f), 2.0, 2.0, 1.0);
            }
            else
            {
                if (num != 1)
                {
                }
                detailModule = new MyBillowFast(MyNoiseQuality.Low, 1, seed, (double) ((random.NextFloat() * 0.07f) + 0.13f), 2.0, 0.5);
            }
        }

        private static void GenerateProceduralAdditions(int version, float size, MyCsgShapeBase[] filledShapes, MyRandom random, float storageOffset)
        {
            bool flag = version > 2;
            for (int i = 0; i < filledShapes.Length; i++)
            {
                if (filledShapes[i] == null)
                {
                    float num2 = (size * ((random.NextFloat() * 0.2f) + 0.1f)) + 2f;
                    float num3 = 2f * num2;
                    float boxSize = size - num3;
                    switch ((random.Next() % (flag ? 2 : 3)))
                    {
                        case 0:
                        {
                            float radius = (num2 * ((random.NextFloat() * 0.4f) + 0.35f)) * (flag ? 0.8f : 1f);
                            filledShapes[i] = new MyCsgSphere((CreateRandomPointOnBox(random, boxSize, version) + num2) + storageOffset, radius, radius * ((random.NextFloat() * 0.1f) + 0.1f), (random.NextFloat() * 0.8f) + 0.2f, (random.NextFloat() * 0.6f) + 0.4f);
                            break;
                        }
                        case 1:
                        {
                            Vector3 vector = CreateRandomPointOnBox(random, boxSize, version) + num2;
                            Vector3 vector2 = new Vector3(size) - vector;
                            if ((random.Next() % 2) == 0)
                            {
                                MyUtils.Swap<float>(ref vector.X, ref vector2.X);
                            }
                            if ((random.Next() % 2) == 0)
                            {
                                MyUtils.Swap<float>(ref vector.Y, ref vector2.Y);
                            }
                            if ((random.Next() % 2) == 0)
                            {
                                MyUtils.Swap<float>(ref vector.Z, ref vector2.Z);
                            }
                            float radius = (((random.NextFloat() * 0.25f) + 0.5f) * num2) * (flag ? 0.5f : 1f);
                            filledShapes[i] = new MyCsgCapsule(vector + storageOffset, vector2 + storageOffset, radius, ((random.NextFloat() * 0.25f) + 0.5f) * (flag ? 1f : radius), (random.NextFloat() * 0.4f) + 0.4f, (random.NextFloat() * 0.6f) + 0.4f);
                            break;
                        }
                        case 2:
                        {
                            Quaternion invRotation = CreateRandomRotation(random);
                            Vector3 point = CreateRandomPointInBox(random, boxSize) + num2;
                            float num8 = ComputeBoxSideDistance(point, size);
                            float secondaryRadius = ((random.NextFloat() * 0.15f) + 0.1f) * num8;
                            filledShapes[i] = new MyCsgTorus(point + storageOffset, invRotation, ((random.NextFloat() * 0.2f) + 0.5f) * num8, secondaryRadius, ((random.NextFloat() * 0.25f) + 0.2f) * secondaryRadius, (random.NextFloat() * 0.8f) + 0.2f, (random.NextFloat() * 0.6f) + 0.4f);
                            break;
                        }
                        default:
                            break;
                    }
                }
            }
        }

        private static void GenerateProceduralRemovals(int version, float size, MyCsgShapeBase[] removedShapes, MyRandom random, float storageOffset)
        {
            bool flag = version > 2;
            for (int i = 0; i < removedShapes.Length; i++)
            {
                if (removedShapes[i] == null)
                {
                    float num2 = (size * ((random.NextFloat() * 0.2f) + 0.1f)) + 2f;
                    float num3 = 2f * num2;
                    float boxSize = size - num3;
                    int num5 = random.Next() % 7;
                    if (num5 == 0)
                    {
                        Vector3 point = CreateRandomPointInBox(random, boxSize) + num2;
                        float num6 = ComputeBoxSideDistance(point, size);
                        float radius = ((random.NextFloat() * (flag ? 0.3f : 0.4f)) + (flag ? 0.1f : 0.3f)) * num6;
                        removedShapes[i] = new MyCsgSphere(point + storageOffset, radius, ((random.NextFloat() * (flag ? 0.2f : 0.3f)) + (flag ? 0.45f : 0.35f)) * radius, (random.NextFloat() * (flag ? 0.2f : 0.8f)) + (flag ? 1f : 0.2f), (random.NextFloat() * (flag ? 0.1f : 0.6f)) + 0.4f);
                    }
                    else if ((num5 - 1) <= 2)
                    {
                        Quaternion invRotation = CreateRandomRotation(random);
                        Vector3 point = CreateRandomPointInBox(random, boxSize) + num2;
                        float num8 = ComputeBoxSideDistance(point, size);
                        float secondaryRadius = ((random.NextFloat() * (flag ? 0.1f : 0.15f)) + (flag ? 0.2f : 0.1f)) * num8;
                        removedShapes[i] = new MyCsgTorus(point + storageOffset, invRotation, ((random.NextFloat() * 0.2f) + (flag ? 0.3f : 0.5f)) * num8, secondaryRadius, ((random.NextFloat() * (flag ? 0.2f : 0.25f)) + (flag ? 1f : 0.2f)) * secondaryRadius, (random.NextFloat() * (flag ? 0.2f : 0.8f)) + (flag ? 1f : 0.2f), (random.NextFloat() * (flag ? 0.2f : 0.6f)) + 0.4f);
                    }
                    else
                    {
                        Vector3 vector = CreateRandomPointOnBox(random, boxSize, version) + num2;
                        Vector3 vector2 = new Vector3(size) - vector;
                        if ((random.Next() % 2) == 0)
                        {
                            MyUtils.Swap<float>(ref vector.X, ref vector2.X);
                        }
                        if ((random.Next() % 2) == 0)
                        {
                            MyUtils.Swap<float>(ref vector.Y, ref vector2.Y);
                        }
                        if ((random.Next() % 2) == 0)
                        {
                            MyUtils.Swap<float>(ref vector.Z, ref vector2.Z);
                        }
                        float radius = ((random.NextFloat() * (flag ? 0.3f : 0.25f)) + (flag ? 0.1f : 0.5f)) * num2;
                        removedShapes[i] = new MyCsgCapsule(vector + storageOffset, vector2 + storageOffset, radius, ((random.NextFloat() * (flag ? 0.5f : 0.25f)) + (flag ? 1f : 0.5f)) * (flag ? 1f : radius), (random.NextFloat() * (flag ? 0.5f : 0.4f)) + (flag ? 1f : 0.4f), (random.NextFloat() * (flag ? 0.2f : 0.6f)) + 0.4f);
                    }
                }
            }
        }

        private void GenerateMaterials(int version, float size, MyRandom random, MyCsgShapeBase[] filledShapes, float storageOffset, out MyVoxelMaterialDefinition defaultMaterial, out MyCompositeShapeOreDeposit[] deposits, MyCompositeShapeProvider.MyCombinedCompositeInfoProvider shapeInfo = null)
        {
            int num;
            bool flag = version > 2;
            bool flag1 = flag;
            if (this.m_coreMaterials == null)
            {
                this.m_coreMaterials = new List<MyVoxelMaterialDefinition>();
                this.m_depositMaterials = new List<MyVoxelMaterialDefinition>();
                this.m_surfaceMaterials = new List<MyVoxelMaterialDefinition>();
                this.FillMaterials(version);
            }
            Action<List<MyVoxelMaterialDefinition>> action = delegate (List<MyVoxelMaterialDefinition> list) {
                int count = list.Count;
                while (count > 1)
                {
                    int num2 = random.Next() % count;
                    count--;
                    MyVoxelMaterialDefinition definition = list[num2];
                    list[num2] = list[count];
                    list[count] = definition;
                }
            };
            action(this.m_depositMaterials);
            defaultMaterial = (this.m_surfaceMaterials.Count != 0) ? this.m_surfaceMaterials[random.Next() % this.m_surfaceMaterials.Count] : ((this.m_depositMaterials.Count != 0) ? this.m_depositMaterials[random.Next() % this.m_depositMaterials.Count] : this.m_coreMaterials[random.Next() % this.m_coreMaterials.Count]);
            if (!flag)
            {
                num = (int) Math.Log((double) size);
            }
            else
            {
                num = (size > 64f) ? ((size > 128f) ? ((size > 256f) ? ((size > 512f) ? 10 : 8) : 6) : 4) : 2;
                if (this.m_depositMaterials.Count == 0)
                {
                    num = 0;
                }
            }
            num = Math.Max(num, filledShapes.Length);
            deposits = new MyCompositeShapeOreDeposit[num];
            float num2 = !flag ? (size / 10f) : ((size / 30f) + 8f);
            MyVoxelMaterialDefinition material = defaultMaterial;
            int num3 = 0;
            for (int i = 0; i < filledShapes.Length; i++)
            {
                if (i != 0)
                {
                    if (this.m_depositMaterials.Count != 0)
                    {
                        num3++;
                        material = this.m_depositMaterials[num3];
                    }
                    else if (this.m_surfaceMaterials.Count != 0)
                    {
                        material = this.m_surfaceMaterials[random.Next() % this.m_surfaceMaterials.Count];
                    }
                }
                else if (this.m_coreMaterials.Count != 0)
                {
                    material = this.m_coreMaterials[random.Next() % this.m_coreMaterials.Count];
                }
                else if (this.m_depositMaterials.Count != 0)
                {
                    num3++;
                    material = this.m_depositMaterials[num3];
                }
                else if (this.m_surfaceMaterials.Count != 0)
                {
                    material = this.m_surfaceMaterials[random.Next() % this.m_surfaceMaterials.Count];
                }
                deposits[i] = new MyCompositeShapeOreDeposit(filledShapes[i].DeepCopy(), material);
                deposits[i].Shape.ShrinkTo((random.NextFloat() * (flag ? 0.6f : 0.15f)) + (flag ? 0.1f : 0.6f));
                if (num3 == this.m_depositMaterials.Count)
                {
                    num3 = 0;
                    action(this.m_depositMaterials);
                }
            }
            int length = filledShapes.Length;
            while (length < num)
            {
                float radius = 0f;
                Vector3 zero = Vector3.Zero;
                int num7 = 0;
                while (true)
                {
                    if (num7 < 10)
                    {
                        zero = (CreateRandomPointInBox(random, size * (flag ? 0.6f : 0.7f)) + storageOffset) + (size * 0.15f);
                        radius = (random.NextFloat() * num2) + (flag ? 5f : 8f);
                        if (shapeInfo != null)
                        {
                            Vector3I vectori = new Vector3I((int) (Math.Sqrt((double) ((radius * radius) / 2f)) * 0.5));
                            BoundingBoxI box = new BoundingBoxI(((Vector3I) zero) - vectori, (Vector3I) (((Vector3I) zero) + vectori));
                            if (MyCompositeShapeProvider.Intersect(shapeInfo, box, 0) == ContainmentType.Disjoint)
                            {
                                num7++;
                                continue;
                            }
                        }
                    }
                    random.NextFloat();
                    random.NextFloat();
                    MyCsgShapeBase shape = new MyCsgSphere(zero, radius, 0f, 0f, 0f);
                    if (this.m_depositMaterials.Count == 0)
                    {
                        num3++;
                        material = this.m_surfaceMaterials[num3];
                    }
                    else
                    {
                        num3++;
                        material = this.m_depositMaterials[num3];
                    }
                    deposits[length] = new MyCompositeShapeOreDeposit(shape, material);
                    if (this.m_depositMaterials.Count == 0)
                    {
                        if (num3 == this.m_surfaceMaterials.Count)
                        {
                            num3 = 0;
                            action(this.m_surfaceMaterials);
                        }
                    }
                    else if (num3 == this.m_depositMaterials.Count)
                    {
                        num3 = 0;
                        action(this.m_depositMaterials);
                    }
                    length++;
                    break;
                }
            }
        }

        private void FillMaterials(int version)
        {
            foreach (MyVoxelMaterialDefinition definition in MyDefinitionManager.Static.GetVoxelMaterialDefinitions())
            {
                if (IsAcceptedAsteroidMaterial(definition, version))
                {
                    if (version > 2)
                    {
                        if (definition.MinedOre == "Stone")
                        {
                            this.m_surfaceMaterials.Add(definition);
                            continue;
                        }
                        this.m_depositMaterials.Add(definition);
                        continue;
                    }
                    if (definition.MinedOre == "Stone")
                    {
                        this.m_surfaceMaterials.Add(definition);
                        continue;
                    }
                    if (definition.MinedOre == "Iron")
                    {
                        this.m_coreMaterials.Add(definition);
                        continue;
                    }
                    if (definition.MinedOre == "Uranium")
                    {
                        this.m_depositMaterials.Add(definition);
                        this.m_depositMaterials.Add(definition);
                        continue;
                    }
                    if (definition.MinedOre != "Ice")
                    {
                        this.m_depositMaterials.Add(definition);
                        continue;
                    }
                    this.m_depositMaterials.Add(definition);
                    this.m_depositMaterials.Add(definition);
                }
            }
            if ((this.m_surfaceMaterials.Count == 0) && (this.m_depositMaterials.Count == 0))
            {
                throw new Exception("There are no voxel materials allowed to spawn in asteroids!");
            }
        }

        private static Vector3 CreateRandomPointInBox(MyRandom random, float boxSize) => 
            new Vector3(random.NextFloat() * boxSize, random.NextFloat() * boxSize, random.NextFloat() * boxSize);

        private static Vector3 CreateRandomPointOnBox(MyRandom random, float boxSize, int version)
        {
            Vector3 zero = Vector3.Zero;
            if (version <= 2)
            {
                switch ((random.Next() & 6))
                {
                    case 0:
                        return new Vector3(0f, random.NextFloat(), random.NextFloat());

                    case 1:
                        return new Vector3(1f, random.NextFloat(), random.NextFloat());

                    case 2:
                        return new Vector3(random.NextFloat(), 0f, random.NextFloat());

                    case 3:
                        return new Vector3(random.NextFloat(), 1f, random.NextFloat());

                    case 4:
                        return new Vector3(random.NextFloat(), random.NextFloat(), 0f);

                    case 5:
                        return new Vector3(random.NextFloat(), random.NextFloat(), 1f);

                    default:
                        break;
                }
            }
            else
            {
                float y = random.NextFloat();
                float z = random.NextFloat();
                switch ((random.Next() % 6))
                {
                    case 0:
                        zero = new Vector3(0f, y, z);
                        break;

                    case 1:
                        zero = new Vector3(1f, y, z);
                        break;

                    case 2:
                        zero = new Vector3(y, 0f, z);
                        break;

                    case 3:
                        zero = new Vector3(y, 1f, z);
                        break;

                    case 4:
                        zero = new Vector3(y, z, 0f);
                        break;

                    case 5:
                        zero = new Vector3(y, z, 1f);
                        break;

                    default:
                        break;
                }
            }
            return (zero * boxSize);
        }

        private static Quaternion CreateRandomRotation(MyRandom self)
        {
            Quaternion quaternion = new Quaternion((self.NextFloat() * 2f) - 1f, (self.NextFloat() * 2f) - 1f, (self.NextFloat() * 2f) - 1f, (self.NextFloat() * 2f) - 1f);
            quaternion.Normalize();
            return quaternion;
        }

        private static float ComputeBoxSideDistance(Vector3 point, float boxSize) => 
            Vector3.Min(point, new Vector3(boxSize) - point).Min();

        private static void FilterKindDuplicates(List<MyVoxelMaterialDefinition> materials, MyRandom random)
        {
            materials.SortNoAlloc<MyVoxelMaterialDefinition>((x, y) => string.Compare(x.MinedOre, y.MinedOre, StringComparison.InvariantCultureIgnoreCase));
            int minValue = 0;
            for (int i = 1; i <= materials.Count; i++)
            {
                if ((i == materials.Count) || (materials[i].MinedOre != materials[minValue].MinedOre))
                {
                    int num3 = random.Next(minValue, i);
                    int index = i - 1;
                    while (true)
                    {
                        if (index < minValue)
                        {
                            i = minValue + 1;
                            break;
                        }
                        if (index != num3)
                        {
                            materials.RemoveAt(index);
                        }
                        index--;
                    }
                }
            }
        }

        private static void LimitMaterials(List<MyVoxelMaterialDefinition> materials, int maxCount, MyRandom random)
        {
            while (materials.Count > maxCount)
            {
                materials.RemoveAt(random.Next(materials.Count));
            }
        }

        private static void ProcessMaterialSpawnProbabilities(List<MyVoxelMaterialDefinition> materials)
        {
            int count = materials.Count;
            int num2 = 0;
            while (num2 < count)
            {
                MyVoxelMaterialDefinition item = materials[num2];
                int num3 = item.AsteroidGeneratorSpawnProbabilityMultiplier - 1;
                int num4 = 0;
                while (true)
                {
                    if (num4 >= num3)
                    {
                        num2++;
                        break;
                    }
                    materials.Add(item);
                    num4++;
                }
            }
        }

        private void MakeIceAsteroid(int version, MyRandom random)
        {
            List<MyVoxelMaterialDefinition> list = new List<MyVoxelMaterialDefinition>();
            foreach (MyVoxelMaterialDefinition definition in MyDefinitionManager.Static.GetVoxelMaterialDefinitions())
            {
                if (!IsAcceptedAsteroidMaterial(definition, version))
                {
                    continue;
                }
                if (definition.MinedOre == "Ice")
                {
                    list.Add(definition);
                }
            }
            if (list.Count == 0)
            {
                MyLog.Default.Log(MyLogSeverity.Error, "No ice material suitable for ice cluster. Ice cluster will not be generated!", Array.Empty<object>());
            }
            else
            {
                this.m_coreMaterials.Clear();
                this.m_depositMaterials.Clear();
                this.m_surfaceMaterials = list;
                FilterKindDuplicates(this.m_surfaceMaterials, random);
            }
        }

        private static bool IsAcceptedAsteroidMaterial(MyVoxelMaterialDefinition material, int version) => 
            (material.SpawnsInAsteroids ? ((material.MinVersion <= version) && (material.MaxVersion >= version)) : false);
        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCompositeShapes.<>c <>9 = new MyCompositeShapes.<>c();
            public static Func<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>, float> <>9__10_0;
            public static Func<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>, float> <>9__10_1;
            public static Comparison<MyVoxelMaterialDefinition> <>9__22_0;

            internal MyTuple<int, bool> <.cctor>b__4_0(int x) => 
                MyTuple.Create<int, bool>(x, false);

            internal MyTuple<int, bool> <.cctor>b__4_1(int x) => 
                MyTuple.Create<int, bool>(x, true);

            internal MyCompositeShapeGeneratorDelegate <.cctor>b__4_2(MyTuple<int, bool> info)
            {
                int version = info.Item1;
                bool combined = info.Item2;
                return delegate (int generatorSeed, int seed, float size) {
                    if (size == 0f)
                    {
                        size = MyUtils.GetRandomFloat(128f, 512f);
                    }
                    MyCompositeShapes shapes = new MyCompositeShapes(generatorSeed, seed, version);
                    using (MyRandom.Instance.PushSeed(seed))
                    {
                        return (!combined ? shapes.ProceduralGenerator(version, seed, size) : shapes.CombinedGenerator(version, seed, size));
                    }
                };
            }

            internal float <FillSpan>b__10_0(MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage> x) => 
                x.Item1.SpawnProbability;

            internal float <FillSpan>b__10_1(MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage> x) => 
                x.Item1.SpawnProbability;

            internal int <FilterKindDuplicates>b__22_0(MyVoxelMaterialDefinition x, MyVoxelMaterialDefinition y) => 
                string.Compare(x.MinedOre, y.MinedOre, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

