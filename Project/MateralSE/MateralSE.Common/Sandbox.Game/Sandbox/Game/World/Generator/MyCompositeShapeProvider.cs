namespace Sandbox.Game.World.Generator
{
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Noise;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    [MyStorageDataProvider(0x2712)]
    internal sealed class MyCompositeShapeProvider : IMyStorageDataProvider
    {
        private const uint CURRENT_VERSION = 3;
        private const uint VERSION_WITHOUT_PLANETS = 1;
        private const uint VERSION_WITHOUT_GENERATOR_SEED = 2;
        private State m_state;
        private IMyCompositionInfoProvider m_infoProvider;
        [ThreadStatic]
        private static List<IMyCompositeDeposit> m_overlappedDeposits;
        [ThreadStatic]
        private static List<IMyCompositeShape> m_overlappedFilledShapes;
        [ThreadStatic]
        private static List<IMyCompositeShape> m_overlappedRemovedShapes;
        [ThreadStatic]
        private static MyStorageData m_storageCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ContentToSignedDistance(byte content) => 
            (((((float) content) / 255f) - 0.5f) * -2f);

        public static MyCompositeShapeProvider CreateAsteroidShape(int seed, float size, int generatorSeed = 0, int? generator = new int?())
        {
            State state;
            state.Version = 3;
            state.Generator = generator.GetValueOrDefault(MySession.Static.Settings.VoxelGeneratorVersion);
            state.Seed = seed;
            state.Size = size;
            state.UnusedCompat = 0;
            state.GeneratorSeed = generatorSeed;
            MyCompositeShapeProvider provider1 = new MyCompositeShapeProvider();
            provider1.InitFromState(state);
            return provider1;
        }

        private static MyStorageData GetTempStorage(ref Vector3I min, ref Vector3I max)
        {
            MyStorageData storageCache = m_storageCache;
            if (storageCache == null)
            {
                m_storageCache = storageCache = new MyStorageData(MyStorageDataTypeFlags.Content);
            }
            storageCache.Resize(min, max);
            return storageCache;
        }

        private void InitFromState(State state)
        {
            this.m_state = state;
            MyCompositeShapeGeneratorDelegate delegate2 = MyCompositeShapes.AsteroidGenerators[state.Generator];
            this.m_infoProvider = delegate2(state.GeneratorSeed, state.Seed, state.Size);
        }

        public ContainmentType Intersect(BoundingBoxI box, int lod) => 
            Intersect(this.m_infoProvider, box, lod);

        public static ContainmentType Intersect(IMyCompositionInfoProvider infoProvider, BoundingBoxI box, int lod)
        {
            ContainmentType disjoint = ContainmentType.Disjoint;
            BoundingBox queryBox = new BoundingBox(box);
            BoundingSphere querySphere = new BoundingSphere(queryBox.Center, queryBox.Extents.Length() / 2f);
            IMyCompositeShape[] filledShapes = infoProvider.FilledShapes;
            int index = 0;
            while (true)
            {
                if (index < filledShapes.Length)
                {
                    ContainmentType type2 = filledShapes[index].Contains(ref queryBox, ref querySphere, 1);
                    if (type2 != ContainmentType.Contains)
                    {
                        if (type2 == ContainmentType.Intersects)
                        {
                            disjoint = ContainmentType.Intersects;
                        }
                        index++;
                        continue;
                    }
                    disjoint = type2;
                }
                if (disjoint != ContainmentType.Disjoint)
                {
                    filledShapes = infoProvider.RemovedShapes;
                    for (index = 0; index < filledShapes.Length; index++)
                    {
                        ContainmentType type3 = filledShapes[index].Contains(ref queryBox, ref querySphere, 1);
                        if (type3 == ContainmentType.Contains)
                        {
                            disjoint = ContainmentType.Disjoint;
                            break;
                        }
                        if (type3 == ContainmentType.Intersects)
                        {
                            disjoint = ContainmentType.Intersects;
                        }
                    }
                }
                return disjoint;
            }
        }

        internal MyVoxelRequestFlags ReadContentRange(MyStorageData target, ref Vector3I writeOffset, int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod, bool detectOnly)
        {
            int num;
            BoundingBox box;
            BoundingSphere sphere;
            SetupReading(lodIndex, ref minInLod, ref maxInLod, out num, out box, out sphere);
            using (MyUtils.ReuseCollection<IMyCompositeShape>(ref m_overlappedFilledShapes))
            {
                using (MyUtils.ReuseCollection<IMyCompositeShape>(ref m_overlappedRemovedShapes))
                {
                    List<IMyCompositeShape> overlappedFilledShapes = m_overlappedFilledShapes;
                    List<IMyCompositeShape> overlappedRemovedShapes = m_overlappedRemovedShapes;
                    ContainmentType disjoint = ContainmentType.Disjoint;
                    IMyCompositeShape[] removedShapes = this.m_infoProvider.RemovedShapes;
                    int index = 0;
                    while (true)
                    {
                        if (index < removedShapes.Length)
                        {
                            IMyCompositeShape item = removedShapes[index];
                            ContainmentType type3 = item.Contains(ref box, ref sphere, num);
                            if (type3 != ContainmentType.Contains)
                            {
                                if (type3 == ContainmentType.Intersects)
                                {
                                    disjoint = ContainmentType.Intersects;
                                    overlappedRemovedShapes.Add(item);
                                }
                                index++;
                                continue;
                            }
                            disjoint = ContainmentType.Contains;
                        }
                        if (disjoint != ContainmentType.Contains)
                        {
                            ContainmentType contains = ContainmentType.Disjoint;
                            removedShapes = this.m_infoProvider.FilledShapes;
                            index = 0;
                            while (true)
                            {
                                MyVoxelRequestFlags emptyData;
                                if (index < removedShapes.Length)
                                {
                                    IMyCompositeShape item = removedShapes[index];
                                    ContainmentType type4 = item.Contains(ref box, ref sphere, num);
                                    if (type4 != ContainmentType.Contains)
                                    {
                                        if (type4 == ContainmentType.Intersects)
                                        {
                                            overlappedFilledShapes.Add(item);
                                            contains = ContainmentType.Intersects;
                                        }
                                        index++;
                                        continue;
                                    }
                                    overlappedFilledShapes.Clear();
                                    contains = ContainmentType.Contains;
                                }
                                if (contains == ContainmentType.Disjoint)
                                {
                                    if (!detectOnly)
                                    {
                                        target.BlockFillContent(writeOffset, (Vector3I) (writeOffset + (maxInLod - minInLod)), 0);
                                    }
                                    emptyData = MyVoxelRequestFlags.EmptyData;
                                }
                                else if ((disjoint == ContainmentType.Disjoint) && (contains == ContainmentType.Contains))
                                {
                                    if (!detectOnly)
                                    {
                                        target.BlockFillContent(writeOffset, (Vector3I) (writeOffset + (maxInLod - minInLod)), 0xff);
                                    }
                                    emptyData = MyVoxelRequestFlags.FullContent;
                                }
                                else
                                {
                                    if (!detectOnly)
                                    {
                                        List<IMyCompositeShape>.Enumerator enumerator;
                                        MyStorageData tempStorage = GetTempStorage(ref minInLod, ref maxInLod);
                                        bool flag = contains == ContainmentType.Contains;
                                        target.BlockFillContent(writeOffset, (Vector3I) (writeOffset + (maxInLod - minInLod)), flag ? ((byte) 0xff) : ((byte) 0));
                                        if (!flag)
                                        {
                                            using (enumerator = overlappedFilledShapes.GetEnumerator())
                                            {
                                                while (enumerator.MoveNext())
                                                {
                                                    enumerator.Current.ComputeContent(tempStorage, lodIndex, minInLod, maxInLod, num);
                                                    target.OpRange<MaxOp>(tempStorage, Vector3I.Zero, maxInLod - minInLod, writeOffset, MyStorageDataTypeEnum.Content);
                                                }
                                            }
                                        }
                                        if (disjoint != ContainmentType.Disjoint)
                                        {
                                            using (enumerator = overlappedRemovedShapes.GetEnumerator())
                                            {
                                                while (enumerator.MoveNext())
                                                {
                                                    enumerator.Current.ComputeContent(tempStorage, lodIndex, minInLod, maxInLod, num);
                                                    target.OpRange<DiffOp>(tempStorage, Vector3I.Zero, maxInLod - minInLod, writeOffset, MyStorageDataTypeEnum.Content);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                    emptyData = 0;
                                }
                                return emptyData;
                            }
                        }
                        else
                        {
                            if (!detectOnly)
                            {
                                target.BlockFillContent(writeOffset, (Vector3I) (writeOffset + (maxInLod - minInLod)), 0);
                            }
                            return MyVoxelRequestFlags.EmptyData;
                        }
                        break;
                    }
                }
            }
            return 0;
        }

        internal unsafe MyVoxelRequestFlags ReadMaterialRange(MyStorageData target, ref Vector3I writeOffset, int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod, bool detectOnly, bool considerContent)
        {
            int num;
            BoundingBox box;
            BoundingSphere sphere;
            SetupReading(lodIndex, ref minInLod, ref maxInLod, out num, out box, out sphere);
            using (MyUtils.ReuseCollection<IMyCompositeDeposit>(ref m_overlappedDeposits))
            {
                List<IMyCompositeDeposit> overlappedDeposits = m_overlappedDeposits;
                MyVoxelMaterialDefinition defaultMaterial = this.m_infoProvider.DefaultMaterial;
                ContainmentType disjoint = ContainmentType.Disjoint;
                IMyCompositeDeposit[] deposits = this.m_infoProvider.Deposits;
                int index = 0;
                while (true)
                {
                    MyVoxelRequestFlags emptyData;
                    if (index < deposits.Length)
                    {
                        IMyCompositeDeposit item = deposits[index];
                        if (item.Contains(ref box, ref sphere, num) != ContainmentType.Disjoint)
                        {
                            overlappedDeposits.Add(item);
                            disjoint = ContainmentType.Intersects;
                        }
                        index++;
                        continue;
                    }
                    if (disjoint == ContainmentType.Disjoint)
                    {
                        if (!detectOnly)
                        {
                            if (considerContent)
                            {
                                target.BlockFillMaterialConsiderContent(writeOffset, (Vector3I) (writeOffset + (maxInLod - minInLod)), defaultMaterial.Index);
                            }
                            else
                            {
                                target.BlockFillMaterial(writeOffset, (Vector3I) (writeOffset + (maxInLod - minInLod)), defaultMaterial.Index);
                            }
                        }
                        emptyData = MyVoxelRequestFlags.EmptyData;
                    }
                    else
                    {
                        if (!detectOnly)
                        {
                            Vector3I vectori;
                            vectori.Z = minInLod.Z;
                            while (vectori.Z <= maxInLod.Z)
                            {
                                vectori.Y = minInLod.Y;
                                while (true)
                                {
                                    if (vectori.Y > maxInLod.Y)
                                    {
                                        int* numPtr3 = (int*) ref vectori.Z;
                                        numPtr3[0]++;
                                        break;
                                    }
                                    vectori.X = minInLod.X;
                                    while (true)
                                    {
                                        if (vectori.X > maxInLod.X)
                                        {
                                            int* numPtr2 = (int*) ref vectori.Y;
                                            numPtr2[0]++;
                                            break;
                                        }
                                        Vector3I p = (Vector3I) ((vectori - minInLod) + writeOffset);
                                        if (considerContent && (target.Content(ref p) == 0))
                                        {
                                            target.Material(ref p, 0xff);
                                        }
                                        else
                                        {
                                            Vector3 localPos = (Vector3) (vectori * num);
                                            float num3 = 1f;
                                            byte materialIdx = defaultMaterial.Index;
                                            if (!MyFakes.DISABLE_COMPOSITE_MATERIAL)
                                            {
                                                foreach (IMyCompositeDeposit deposit2 in overlappedDeposits)
                                                {
                                                    float num5 = deposit2.SignedDistance(ref localPos, 1);
                                                    if ((num5 < 0f) && (num5 <= num3))
                                                    {
                                                        num3 = num5;
                                                        MyVoxelMaterialDefinition materialForPosition = deposit2.GetMaterialForPosition(ref localPos, (float) num);
                                                        materialIdx = (materialForPosition == null) ? defaultMaterial.Index : materialForPosition.Index;
                                                    }
                                                }
                                            }
                                            target.Material(ref p, materialIdx);
                                        }
                                        int* numPtr1 = (int*) ref vectori.X;
                                        numPtr1[0]++;
                                    }
                                }
                            }
                            break;
                        }
                        emptyData = 0;
                    }
                    return emptyData;
                }
            }
            return 0;
        }

        private static void SetupReading(int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod, out int lodVoxelSize, out BoundingBox queryBox, out BoundingSphere querySphere)
        {
            Vector3 vector3;
            float num = 0.5f * (1 << (lodIndex & 0x1f));
            lodVoxelSize = (int) (num * 2f);
            Vector3I voxelCoord = minInLod << lodIndex;
            Vector3I vectori2 = maxInLod << lodIndex;
            MyVoxelCoordSystems.VoxelCoordToLocalPosition(ref voxelCoord, out vector3);
            MyVoxelCoordSystems.VoxelCoordToLocalPosition(ref vectori2, out vector3);
            Vector3 min = vector3 - num;
            queryBox = new BoundingBox(min, vector3 + num);
            BoundingSphere.CreateFromBoundingBox(ref queryBox, out querySphere);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte SignedDistanceToContent(float signedDistance)
        {
            float single1 = MathHelper.Clamp(signedDistance, -1f, 1f);
            signedDistance = single1;
            return (byte) (((signedDistance / -2f) + 0.5f) * 255f);
        }

        void IMyStorageDataProvider.Close()
        {
            using (IEnumerator<IMyCompositeShape> enumerator = this.m_infoProvider.Deposits.Concat<IMyCompositeShape>(this.m_infoProvider.FilledShapes).Concat<IMyCompositeShape>(this.m_infoProvider.RemovedShapes).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Close();
                }
            }
            this.m_infoProvider.Close();
            this.m_infoProvider = null;
        }

        void IMyStorageDataProvider.DebugDraw(ref MatrixD worldMatrix)
        {
            int num;
            if (MyDebugDrawSettings.DEBUG_DRAW_ASTEROID_SEEDS)
            {
                object[] objArray1 = new object[] { "Size: ", this.m_state.Size, Environment.NewLine, "Seed: ", this.m_state.Seed, Environment.NewLine, "GeneratorSeed: ", this.m_state.GeneratorSeed };
                MyRenderProxy.DebugDrawText3D(worldMatrix.Translation, string.Concat(objArray1), Color.Red, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            Color green = Color.Green;
            Color red = Color.Red;
            Color cornflowerBlue = Color.CornflowerBlue;
            IMyCompositeShape[] filledShapes = this.m_infoProvider.FilledShapes;
            for (num = 0; num < filledShapes.Length; num++)
            {
                filledShapes[num].DebugDraw(ref worldMatrix, green);
            }
            filledShapes = this.m_infoProvider.RemovedShapes;
            for (num = 0; num < filledShapes.Length; num++)
            {
                filledShapes[num].DebugDraw(ref worldMatrix, red);
            }
            IMyCompositeDeposit[] deposits = this.m_infoProvider.Deposits;
            for (num = 0; num < deposits.Length; num++)
            {
                deposits[num].DebugDraw(ref worldMatrix, cornflowerBlue);
            }
        }

        bool IMyStorageDataProvider.Intersect(ref LineD line, out double startOffset, out double endOffset)
        {
            startOffset = 0.0;
            endOffset = 0.0;
            return true;
        }

        void IMyStorageDataProvider.PostProcess(VrVoxelMesh mesh, MyStorageDataTypeFlags dataTypes)
        {
        }

        void IMyStorageDataProvider.ReadFrom(int storageVersion, Stream stream, int size, ref bool isOldFormat)
        {
            State state;
            state.Version = stream.ReadUInt32();
            if (state.Version != 3)
            {
                isOldFormat = true;
            }
            state.Generator = stream.ReadInt32();
            state.Seed = stream.ReadInt32();
            state.Size = stream.ReadFloat();
            if (state.Version == 1)
            {
                state.UnusedCompat = 0;
                state.GeneratorSeed = 0;
            }
            else
            {
                state.UnusedCompat = stream.ReadUInt32();
                if (state.UnusedCompat == 1)
                {
                    throw new InvalidBranchException();
                }
                if (state.Version > 2)
                {
                    state.GeneratorSeed = stream.ReadInt32();
                }
                else
                {
                    isOldFormat = true;
                    state.GeneratorSeed = 0;
                }
            }
            this.InitFromState(state);
            this.m_state.Version = 3;
        }

        unsafe void IMyStorageDataProvider.ReadRange(ref MyVoxelDataRequest req, bool detectOnly = false)
        {
            req.Flags = !req.RequestedData.Requests(MyStorageDataTypeEnum.Content) ? this.ReadMaterialRange(req.Target, ref req.Offset, req.Lod, ref req.MinInLod, ref req.MaxInLod, detectOnly, (req.RequestFlags & MyVoxelRequestFlags.ConsiderContent) > 0) : this.ReadContentRange(req.Target, ref req.Offset, req.Lod, ref req.MinInLod, ref req.MaxInLod, detectOnly);
            MyVoxelRequestFlags* flagsPtr1 = (MyVoxelRequestFlags*) ref req.Flags;
            *((int*) flagsPtr1) |= req.RequestFlags & MyVoxelRequestFlags.RequestFlags;
        }

        void IMyStorageDataProvider.ReadRange(MyStorageData target, MyStorageDataTypeFlags dataType, ref Vector3I writeOffset, int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod)
        {
            if (dataType.Requests(MyStorageDataTypeEnum.Content))
            {
                this.ReadContentRange(target, ref writeOffset, lodIndex, ref minInLod, ref maxInLod, false);
            }
            else
            {
                this.ReadMaterialRange(target, ref writeOffset, lodIndex, ref minInLod, ref maxInLod, false, false);
            }
        }

        void IMyStorageDataProvider.ReindexMaterials(Dictionary<byte, byte> oldToNewIndexMap)
        {
        }

        void IMyStorageDataProvider.WriteTo(Stream stream)
        {
            stream.WriteNoAlloc(this.m_state.Version);
            stream.WriteNoAlloc(this.m_state.Generator);
            stream.WriteNoAlloc(this.m_state.Seed);
            stream.WriteNoAlloc(this.m_state.Size);
            stream.WriteNoAlloc(this.m_state.UnusedCompat);
            stream.WriteNoAlloc(this.m_state.GeneratorSeed);
        }

        int IMyStorageDataProvider.SerializedSize =>
            sizeof(State);

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct DiffOp : MyStorageData.IOperator
        {
            public void Op(ref byte a, byte b)
            {
                a = (byte) Math.Min(a, 0xff - b);
            }
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct MaxOp : MyStorageData.IOperator
        {
            public void Op(ref byte a, byte b)
            {
                a = Math.Max(a, b);
            }
        }

        public class MyCombinedCompositeInfoProvider : MyCompositeShapeProvider.MyProceduralCompositeInfoProvider, IMyCompositionInfoProvider
        {
            private readonly IMyCompositeShape[] m_filledShapes;
            private readonly IMyCompositeShape[] m_removedShapes;

            public MyCombinedCompositeInfoProvider(ref MyCompositeShapeProvider.MyProceduralCompositeInfoProvider.ConstructionData data, IMyCompositeShape[] filledShapes, IMyCompositeShape[] removedShapes) : base(ref data)
            {
                this.m_filledShapes = base.m_filledShapes.Concat<IMyCompositeShape>(filledShapes).ToArray<IMyCompositeShape>();
                this.m_removedShapes = base.m_removedShapes.Concat<IMyCompositeShape>(removedShapes).ToArray<IMyCompositeShape>();
            }

            public void UpdateMaterials(MyVoxelMaterialDefinition defaultMaterial, MyCompositeShapeOreDeposit[] deposits)
            {
                base.UpdateMaterials(defaultMaterial, deposits);
            }

            IMyCompositeShape[] IMyCompositionInfoProvider.FilledShapes =>
                this.m_filledShapes;

            IMyCompositeShape[] IMyCompositionInfoProvider.RemovedShapes =>
                this.m_removedShapes;
        }

        public class MyProceduralCompositeInfoProvider : IMyCompositionInfoProvider
        {
            public readonly IMyModule MacroModule;
            public readonly IMyModule DetailModule;
            protected ProceduralCompositeOreDeposit[] m_deposits;
            protected MyVoxelMaterialDefinition m_defaultMaterial;
            protected readonly ProceduralCompositeShape[] m_filledShapes;
            protected readonly ProceduralCompositeShape[] m_removedShapes;

            public MyProceduralCompositeInfoProvider(ref ConstructionData data)
            {
                this.MacroModule = data.MacroModule;
                this.DetailModule = data.DetailModule;
                this.m_defaultMaterial = data.DefaultMaterial;
                this.m_deposits = (from x in data.Deposits select new ProceduralCompositeOreDeposit(this, x)).ToArray<ProceduralCompositeOreDeposit>();
                this.m_filledShapes = (from x in data.FilledShapes select new ProceduralCompositeShape(this, x)).ToArray<ProceduralCompositeShape>();
                this.m_removedShapes = (from x in data.RemovedShapes select new ProceduralCompositeShape(this, x)).ToArray<ProceduralCompositeShape>();
            }

            void IMyCompositionInfoProvider.Close()
            {
            }

            protected void UpdateMaterials(MyVoxelMaterialDefinition defaultMaterial, MyCompositeShapeOreDeposit[] deposits)
            {
                this.m_defaultMaterial = defaultMaterial;
                this.m_deposits = (from x in deposits select new ProceduralCompositeOreDeposit(this, x)).ToArray<ProceduralCompositeOreDeposit>();
            }

            IMyCompositeDeposit[] IMyCompositionInfoProvider.Deposits =>
                this.m_deposits;

            IMyCompositeShape[] IMyCompositionInfoProvider.FilledShapes =>
                this.m_filledShapes;

            IMyCompositeShape[] IMyCompositionInfoProvider.RemovedShapes =>
                this.m_removedShapes;

            MyVoxelMaterialDefinition IMyCompositionInfoProvider.DefaultMaterial =>
                this.m_defaultMaterial;

            [StructLayout(LayoutKind.Sequential)]
            public struct ConstructionData
            {
                public IMyModule MacroModule;
                public IMyModule DetailModule;
                public MyCsgShapeBase[] FilledShapes;
                public MyCsgShapeBase[] RemovedShapes;
                public MyCompositeShapeOreDeposit[] Deposits;
                public MyVoxelMaterialDefinition DefaultMaterial;
            }

            protected class ProceduralCompositeOreDeposit : MyCompositeShapeProvider.MyProceduralCompositeInfoProvider.ProceduralCompositeShape, IMyCompositeDeposit, IMyCompositeShape
            {
                private readonly MyCompositeShapeOreDeposit m_deposit;

                public ProceduralCompositeOreDeposit(MyCompositeShapeProvider.MyProceduralCompositeInfoProvider context, MyCompositeShapeOreDeposit deposit) : base(context, deposit.Shape)
                {
                    this.m_deposit = deposit;
                }

                public void DebugDraw(ref MatrixD worldMatrix, Color color)
                {
                    this.m_deposit.DebugDraw(ref worldMatrix, color);
                }

                public MyVoxelMaterialDefinition GetMaterialForPosition(ref Vector3 localPos, float lodVoxelSize) => 
                    this.m_deposit.GetMaterialForPosition(ref localPos, lodVoxelSize);
            }

            protected class ProceduralCompositeShape : IMyCompositeShape
            {
                private MyCsgShapeBase m_shape;
                private MyCompositeShapeProvider.MyProceduralCompositeInfoProvider m_context;

                public ProceduralCompositeShape(MyCompositeShapeProvider.MyProceduralCompositeInfoProvider context, MyCsgShapeBase shape)
                {
                    this.m_shape = shape;
                    this.m_context = context;
                }

                public void Close()
                {
                }

                public unsafe void ComputeContent(MyStorageData target, int lodIndex, Vector3I minInLod, Vector3I maxInLod, int lodVoxelSize)
                {
                    byte* numPtr;
                    byte[] pinned buffer;
                    Vector3I vectori = minInLod;
                    Vector3I vectori2 = vectori * lodVoxelSize;
                    Vector3I vectori3 = vectori2;
                    if (((buffer = target[MyStorageDataTypeEnum.Content]) == null) || (buffer.Length == 0))
                    {
                        numPtr = null;
                    }
                    else
                    {
                        numPtr = buffer;
                    }
                    byte* numPtr2 = numPtr;
                    int sizeLinear = target.SizeLinear;
                    vectori.Z = minInLod.Z;
                    while (vectori.Z <= maxInLod.Z)
                    {
                        vectori.Y = minInLod.Y;
                        while (true)
                        {
                            if (vectori.Y > maxInLod.Y)
                            {
                                int* numPtr6 = (int*) ref vectori2.Z;
                                numPtr6[0] += lodVoxelSize;
                                vectori2.Y = vectori3.Y;
                                int* numPtr7 = (int*) ref vectori.Z;
                                numPtr7[0]++;
                                break;
                            }
                            vectori.X = minInLod.X;
                            while (true)
                            {
                                if (vectori.X > maxInLod.X)
                                {
                                    int* numPtr4 = (int*) ref vectori2.Y;
                                    numPtr4[0] += lodVoxelSize;
                                    vectori2.X = vectori3.X;
                                    int* numPtr5 = (int*) ref vectori.Y;
                                    numPtr5[0]++;
                                    break;
                                }
                                Vector3 localPos = new Vector3(vectori2);
                                float signedDistance = this.SignedDistance(ref localPos, lodVoxelSize);
                                numPtr2[0] = MyCompositeShapeProvider.SignedDistanceToContent(signedDistance);
                                numPtr2 += target.StepLinear;
                                int* numPtr1 = (int*) ref vectori2.X;
                                numPtr1[0] += lodVoxelSize;
                                int* numPtr3 = (int*) ref vectori.X;
                                numPtr3[0]++;
                            }
                        }
                    }
                    buffer = null;
                }

                public ContainmentType Contains(ref BoundingBox queryBox, ref BoundingSphere querySphere, int lodVoxelSize) => 
                    this.m_shape.Contains(ref queryBox, ref querySphere, (float) lodVoxelSize);

                public void DebugDraw(ref MatrixD worldMatrix, Color color)
                {
                    this.m_shape.DebugDraw(ref worldMatrix, color);
                }

                public float SignedDistance(ref Vector3 localPos, int lodVoxelSize) => 
                    this.m_shape.SignedDistance(ref localPos, (float) lodVoxelSize, this.m_context.MacroModule, this.m_context.DetailModule);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct State
        {
            public uint Version;
            public int Generator;
            public int Seed;
            public float Size;
            public uint UnusedCompat;
            public int GeneratorSeed;
        }
    }
}

