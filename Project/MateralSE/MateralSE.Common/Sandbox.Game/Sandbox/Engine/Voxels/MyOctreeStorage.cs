namespace Sandbox.Engine.Voxels
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using Sandbox.Game.World.Generator;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Utils;
    using VRage.Game.Voxels;
    using VRage.ModAPI;
    using VRage.Plugins;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    public class MyOctreeStorage : MyStorageBase, IMyCompositeShape
    {
        private const int CURRENT_FILE_VERSION = 1;
        public const int LeafLodCount = 4;
        public const int LeafSizeInVoxels = 0x10;
        private readonly Dictionary<byte, byte> m_oldToNewIndexMap;
        [ThreadStatic]
        private static MyStorageData m_temporaryCache;
        private int m_treeHeight;
        private readonly Dictionary<ulong, MyOctreeNode> m_contentNodes;
        private readonly Dictionary<ulong, IMyOctreeLeafNode> m_contentLeaves;
        private readonly Dictionary<ulong, MyOctreeNode> m_materialNodes;
        private readonly Dictionary<ulong, IMyOctreeLeafNode> m_materialLeaves;
        private IMyStorageDataProvider m_dataProvider;
        private readonly Dictionary<ulong, IMyOctreeLeafNode> m_tmpResetLeaves;
        private BoundingBoxD m_tmpResetLeavesBoundingBox;
        [ThreadStatic]
        private static MyStorageData m_storageCached;
        private static readonly Dictionary<int, MyStorageDataProviderAttribute> m_attributesById = new Dictionary<int, MyStorageDataProviderAttribute>();
        private static readonly Dictionary<Type, MyStorageDataProviderAttribute> m_attributesByType = new Dictionary<Type, MyStorageDataProviderAttribute>();
        private const int VERSION_OCTREE_NODES_32BIT_KEY = 1;
        private const int CURRENT_VERSION_OCTREE_NODES = 2;
        private const int VERSION_OCTREE_LEAVES_32BIT_KEY = 2;
        private const int CURRENT_VERSION_OCTREE_LEAVES = 3;

        static MyOctreeStorage()
        {
            RegisterTypes(Assembly.GetExecutingAssembly());
            RegisterTypes(MyPlugins.GameAssembly);
            RegisterTypes(MyPlugins.SandboxAssembly);
            RegisterTypes(MyPlugins.UserAssemblies);
        }

        public MyOctreeStorage()
        {
            this.m_oldToNewIndexMap = new Dictionary<byte, byte>();
            this.m_contentNodes = new Dictionary<ulong, MyOctreeNode>();
            this.m_contentLeaves = new Dictionary<ulong, IMyOctreeLeafNode>();
            this.m_materialNodes = new Dictionary<ulong, MyOctreeNode>();
            this.m_materialLeaves = new Dictionary<ulong, IMyOctreeLeafNode>();
            this.m_tmpResetLeaves = new Dictionary<ulong, IMyOctreeLeafNode>();
        }

        public MyOctreeStorage(IMyStorageDataProvider dataProvider, Vector3I size)
        {
            this.m_oldToNewIndexMap = new Dictionary<byte, byte>();
            this.m_contentNodes = new Dictionary<ulong, MyOctreeNode>();
            this.m_contentLeaves = new Dictionary<ulong, IMyOctreeLeafNode>();
            this.m_materialNodes = new Dictionary<ulong, MyOctreeNode>();
            this.m_materialLeaves = new Dictionary<ulong, IMyOctreeLeafNode>();
            this.m_tmpResetLeaves = new Dictionary<ulong, IMyOctreeLeafNode>();
            int v = MathHelper.Max(size.X, size.Y, size.Z);
            base.Size = new Vector3I(MathHelper.GetNearestBiggerPowerOfTwo(v));
            this.m_dataProvider = dataProvider;
            this.InitTreeHeight();
            this.ResetInternal(MyStorageDataTypeFlags.All);
            base.Geometry.Init(this);
        }

        private static void ComputeChildCoord(int childIdx, out Vector3I relativeCoord)
        {
            relativeCoord.X = childIdx & 1;
            relativeCoord.Y = (childIdx >> 1) & 1;
            relativeCoord.Z = (childIdx >> 2) & 1;
        }

        public void ComputeContent(MyStorageData storage, int lodIndex, Vector3I lodVoxelRangeMin, Vector3I lodVoxelRangeMax, int lodVoxelSize)
        {
            base.ReadRange(storage, MyStorageDataTypeFlags.Content, lodIndex, lodVoxelRangeMin, lodVoxelRangeMax);
        }

        private static void ConsiderContent(MyStorageData storage)
        {
            byte[] buffer = storage[MyStorageDataTypeEnum.Content];
            byte[] buffer2 = storage[MyStorageDataTypeEnum.Material];
            for (int i = 0; i < storage.SizeLinear; i++)
            {
                if (buffer[i] == 0)
                {
                    buffer2[i] = 0xff;
                }
            }
        }

        public ContainmentType Contains(ref BoundingBox queryBox, ref BoundingSphere querySphere, int lodVoxelSize) => 
            this.Intersect(ref queryBox, false);

        private TraverseArgs<TOperator> ContentArgs<TOperator>(ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax) where TOperator: struct, ITraverseOps => 
            new TraverseArgs<TOperator> { 
                Storage = this,
                Operator = default(TOperator),
                DataFilter = MyOctreeNode.ContentFilter,
                DataType = MyStorageDataTypeEnum.Content,
                Leaves = this.m_contentLeaves,
                Nodes = this.m_contentNodes,
                Min = voxelRangeMin,
                Max = voxelRangeMax
            };

        public override VRage.Game.Voxels.IMyStorage Copy()
        {
            byte[] buffer;
            base.Save(out buffer);
            MyStorageBase base1 = Load(buffer);
            base1.Shared = false;
            return base1;
        }

        public override void DebugDraw(ref MatrixD worldMatrix, MyVoxelDebugDrawMode mode)
        {
            base.DebugDraw(ref worldMatrix, mode);
            Color cornflowerBlue = Color.CornflowerBlue;
            cornflowerBlue.A = 0x19;
            this.DebugDraw(ref worldMatrix, mode, cornflowerBlue);
        }

        public void DebugDraw(ref MatrixD worldMatrix, Color color)
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_ASTEROID_COMPOSITION_CONTENT)
            {
                color.A = 10;
                this.DebugDraw(ref worldMatrix, MyVoxelDebugDrawMode.Content_MacroLeaves, color);
            }
        }

        public void DebugDraw(ref MatrixD worldMatrix, MyVoxelDebugDrawMode mode, Color color)
        {
            switch (mode)
            {
                case MyVoxelDebugDrawMode.Content_MicroNodes:
                case MyVoxelDebugDrawMode.Content_MicroNodesScaled:
                    DrawSparseOctrees(ref worldMatrix, color, mode, this.m_contentLeaves);
                    break;

                case MyVoxelDebugDrawMode.Content_MacroNodes:
                    DrawNodes(ref worldMatrix, color, this.m_contentNodes);
                    break;

                case MyVoxelDebugDrawMode.Content_MacroLeaves:
                    DrawLeaves(ref worldMatrix, color, this.m_contentLeaves);
                    break;

                case MyVoxelDebugDrawMode.Content_MacroScaled:
                    DrawScaledNodes(ref worldMatrix, color, this.m_contentNodes);
                    break;

                case MyVoxelDebugDrawMode.Materials_MacroNodes:
                    DrawNodes(ref worldMatrix, color, this.m_materialNodes);
                    break;

                case MyVoxelDebugDrawMode.Materials_MacroLeaves:
                    DrawLeaves(ref worldMatrix, color, this.m_materialLeaves);
                    break;

                case MyVoxelDebugDrawMode.Content_DataProvider:
                    if (this.m_dataProvider != null)
                    {
                        this.m_dataProvider.DebugDraw(ref worldMatrix);
                    }
                    break;

                default:
                    break;
            }
            if (this.m_tmpResetLeaves.Count > 0)
            {
                Color greenYellow = Color.GreenYellow;
                MyStringId? faceMaterial = null;
                faceMaterial = null;
                MySimpleObjectDraw.DrawTransparentBox(ref MatrixD.Identity, ref this.m_tmpResetLeavesBoundingBox, ref greenYellow, MySimpleObjectRasterizer.Wireframe, 1, 0.04f, faceMaterial, faceMaterial, false, -1, MyBillboard.BlendTypeEnum.Standard, 1f, null);
                greenYellow.A = 0x19;
                DrawLeaves(ref worldMatrix, greenYellow, this.m_tmpResetLeaves);
            }
        }

        protected override void DeleteRangeInternal(MyStorageDataTypeFlags dataToDelete, ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax)
        {
            if ((dataToDelete & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None)
            {
                Traverse<DeleteRangeOps>(ref this.ContentArgs<DeleteRangeOps>(ref voxelRangeMin, ref voxelRangeMax), 0, this.m_treeHeight + 4, Vector3I.Zero);
            }
            if ((dataToDelete & MyStorageDataTypeFlags.Material) != MyStorageDataTypeFlags.None)
            {
                Traverse<DeleteRangeOps>(ref this.MaterialArgs<DeleteRangeOps>(ref voxelRangeMin, ref voxelRangeMax), 0, this.m_treeHeight + 4, Vector3I.Zero);
            }
        }

        private static unsafe void DrawLeaves(ref MatrixD worldMatrix, Color color, Dictionary<ulong, IMyOctreeLeafNode> octree)
        {
            using (IMyDebugDrawBatchAabb aabb = MyRenderProxy.DebugDrawBatchAABB(worldMatrix, color, true, true))
            {
                MyCellCoord coord = new MyCellCoord();
                foreach (KeyValuePair<ulong, IMyOctreeLeafNode> pair in octree)
                {
                    coord.SetUnpack(pair.Key);
                    int* numPtr1 = (int*) ref coord.Lod;
                    numPtr1[0] += 4;
                    IMyOctreeLeafNode local1 = pair.Value;
                    Vector3I vectori = coord.CoordInLod << coord.Lod;
                    BoundingBoxD xd = new BoundingBoxD((Vector3D) (vectori * 1f), (vectori + (1 << (coord.Lod & 0x1f))) * 1f);
                    Color? nullable = null;
                    aabb.Add(ref xd, nullable);
                }
            }
        }

        private static unsafe void DrawNodes(ref MatrixD worldMatrix, Color color, Dictionary<ulong, MyOctreeNode> octree)
        {
            using (IMyDebugDrawBatchAabb aabb = MyRenderProxy.DebugDrawBatchAABB(worldMatrix, color, true, true))
            {
                MyCellCoord coord = new MyCellCoord();
                foreach (KeyValuePair<ulong, MyOctreeNode> pair in octree)
                {
                    coord.SetUnpack(pair.Key);
                    int* numPtr1 = (int*) ref coord.Lod;
                    numPtr1[0] += 4;
                    MyOctreeNode node = pair.Value;
                    for (int i = 0; i < 8; i++)
                    {
                        if (!node.HasChild(i))
                        {
                            Vector3I vectori;
                            ComputeChildCoord(i, out vectori);
                            float num2 = 1f * (1 << (coord.Lod & 0x1f));
                            Vector3 vector = (((coord.CoordInLod << (coord.Lod + 1)) + (vectori << coord.Lod)) * 1f) + (0.5f * num2);
                            BoundingBoxD xd = new BoundingBoxD(vector - (0.5f * num2), vector + (0.5f * num2));
                            Color? nullable = null;
                            aabb.Add(ref xd, nullable);
                        }
                    }
                }
            }
        }

        private static unsafe void DrawScaledNodes(ref MatrixD worldMatrix, Color color, Dictionary<ulong, MyOctreeNode> octree)
        {
            using (IMyDebugDrawBatchAabb aabb = MyRenderProxy.DebugDrawBatchAABB(worldMatrix, color, true, true))
            {
                MyCellCoord coord = new MyCellCoord();
                foreach (KeyValuePair<ulong, MyOctreeNode> pair in octree)
                {
                    coord.SetUnpack(pair.Key);
                    int* numPtr1 = (int*) ref coord.Lod;
                    numPtr1[0] += 4;
                    MyOctreeNode node = pair.Value;
                    for (int i = 0; i < 8; i++)
                    {
                        if (!node.HasChild(i) || (coord.Lod == 4))
                        {
                            Vector3I vectori;
                            ComputeChildCoord(i, out vectori);
                            float num2 = ((float) node.GetData(i)) / 255f;
                            if (num2 != 0f)
                            {
                                float num3 = 1f * (1 << (coord.Lod & 0x1f));
                                Vector3 vector = (((coord.CoordInLod << (coord.Lod + 1)) + (vectori << coord.Lod)) * 1f) + (0.5f * num3);
                                num3 *= (float) Math.Pow(num2 * 1.0, 0.3333);
                                BoundingBoxD xd = new BoundingBoxD(vector - (0.5f * num3), vector + (0.5f * num3));
                                Color? nullable = null;
                                aabb.Add(ref xd, nullable);
                            }
                        }
                    }
                }
            }
        }

        private static void DrawSparseOctrees(ref MatrixD worldMatrix, Color color, MyVoxelDebugDrawMode mode, Dictionary<ulong, IMyOctreeLeafNode> octree)
        {
            MyCamera mainCamera = MySector.MainCamera;
            if (mainCamera != null)
            {
                MatrixD xd = mainCamera.WorldMatrix;
                Vector3 vector = (Vector3) Vector3D.Transform(xd.Translation + (xd.Forward * 10.0), MatrixD.Invert((MatrixD) worldMatrix));
                using (IMyDebugDrawBatchAabb aabb = MyRenderProxy.DebugDrawBatchAABB(worldMatrix, color, true, true))
                {
                    MyCellCoord coord = new MyCellCoord();
                    foreach (KeyValuePair<ulong, IMyOctreeLeafNode> pair in octree)
                    {
                        MyMicroOctreeLeaf leaf = pair.Value as MyMicroOctreeLeaf;
                        if (leaf != null)
                        {
                            coord.SetUnpack(pair.Key);
                            Vector3 min = (Vector3) ((coord.CoordInLod << 4) * 1f);
                            Vector3 max = min + 16f;
                            if (vector.IsInsideInclusive(ref min, ref max))
                            {
                                leaf.DebugDraw(aabb, min, mode);
                            }
                        }
                    }
                }
            }
        }

        private void FillOutOfBounds(MyStorageData target, MyStorageDataTypeEnum type, ref Vector3I woffset, int lodIndex, Vector3I minInLod, Vector3I maxInLod)
        {
            byte content = MyVoxelDataConstants.DefaultValue(type);
            Vector3I max = new Vector3I((1 << (((this.m_treeHeight + 4) - lodIndex) & 0x1f)) - 1);
            Vector3I vectori2 = woffset - minInLod;
            BoundingBoxI xi = new BoundingBoxI(minInLod, maxInLod);
            BoundingBoxI box = new BoundingBoxI(Vector3I.Zero, max);
            if (!xi.Intersects(ref box))
            {
                target.BlockFill(type, (Vector3I) (vectori2 + minInLod), (Vector3I) (vectori2 + maxInLod), content);
            }
            else
            {
                if (minInLod.X < 0)
                {
                    Vector3I vectori3 = minInLod;
                    Vector3I vectori4 = maxInLod;
                    vectori4.X = -1;
                    minInLod.X = 0;
                    target.BlockFill(type, (Vector3I) (vectori3 + vectori2), (Vector3I) (vectori4 + vectori2), content);
                }
                if (maxInLod.X > max.X)
                {
                    Vector3I vectori5 = minInLod;
                    Vector3I vectori6 = maxInLod;
                    vectori5.X = max.X + 1;
                    minInLod.X = max.X;
                    target.BlockFill(type, (Vector3I) (vectori5 + vectori2), (Vector3I) (vectori6 + vectori2), content);
                }
                if (minInLod.Y < 0)
                {
                    Vector3I vectori7 = minInLod;
                    Vector3I vectori8 = maxInLod;
                    vectori8.Y = -1;
                    minInLod.Y = 0;
                    target.BlockFill(type, (Vector3I) (vectori7 + vectori2), (Vector3I) (vectori8 + vectori2), content);
                }
                if (maxInLod.Y > max.Y)
                {
                    Vector3I vectori9 = minInLod;
                    Vector3I vectori10 = maxInLod;
                    vectori9.Y = max.Y + 1;
                    minInLod.Y = max.Y;
                    target.BlockFill(type, (Vector3I) (vectori9 + vectori2), (Vector3I) (vectori10 + vectori2), content);
                }
                if (minInLod.Y < 0)
                {
                    Vector3I vectori11 = minInLod;
                    Vector3I vectori12 = maxInLod;
                    vectori12.Y = -1;
                    minInLod.Y = 0;
                    target.BlockFill(type, (Vector3I) (vectori11 + vectori2), (Vector3I) (vectori12 + vectori2), content);
                }
                if (maxInLod.Y > max.Y)
                {
                    Vector3I vectori13 = minInLod;
                    Vector3I vectori14 = maxInLod;
                    vectori13.Y = max.Y + 1;
                    minInLod.Y = max.Y;
                    target.BlockFill(type, (Vector3I) (vectori13 + vectori2), (Vector3I) (vectori14 + vectori2), content);
                }
            }
        }

        private void InitTreeHeight()
        {
            this.m_treeHeight = -1;
            Vector3I vectori = base.Size >> 4;
            while (vectori != Vector3I.Zero)
            {
                vectori = vectori >> 1;
                this.m_treeHeight++;
            }
            if (this.m_treeHeight < 0)
            {
                this.m_treeHeight = 1;
            }
        }

        public override ContainmentType Intersect(ref BoundingBox box, bool lazy)
        {
            ContainmentType disjoint;
            using (base.StorageLock.AcquireSharedUsing())
            {
                if (base.Closed)
                {
                    disjoint = ContainmentType.Disjoint;
                }
                else
                {
                    BoundingBoxI xi = new BoundingBoxI(new Vector3I(box.Min), new Vector3I(Vector3.Ceiling(box.Max)));
                    disjoint = this.IntersectInternal(ref xi, 0, !lazy);
                }
            }
            return disjoint;
        }

        public override unsafe bool IntersectInternal(ref LineD line)
        {
            LineD ed;
            MyCellCoord* coordPtr = (MyCellCoord*) stackalloc byte[(((IntPtr) MySparseOctree.EstimateStackSize(this.m_treeHeight)) * sizeof(MyCellCoord))];
            BoundingBoxD xd = BoundingBoxD.CreateInvalid();
            ed.From = line.To;
            ed.To = line.From;
            MyCellCoord coord = new MyCellCoord(this.m_treeHeight + 4, ref Vector3I.Zero);
            int index = 0 + 1;
            coordPtr[index] = coord;
            MyCellCoord coord2 = new MyCellCoord();
            BoundingBoxI boundingBox = (BoundingBoxI) line.GetBoundingBox();
            while (index > 0)
            {
                int lod;
                IMyOctreeLeafNode node;
                MyOctreeNode node2;
                coord = coordPtr[--index];
                coord2.Lod = Math.Max(coord.Lod - 4, 0);
                coord2.CoordInLod = coord.CoordInLod;
                if (this.m_contentLeaves.TryGetValue(coord2.PackId64(), out node))
                {
                    double num3;
                    double num4;
                    LineD ed2;
                    lod = coord.Lod;
                    Vector3I min = boundingBox.Min >> lod;
                    Vector3I max = boundingBox.Max >> lod;
                    if (!coord.CoordInLod.IsInsideInclusiveEnd(ref min, ref max))
                    {
                        continue;
                    }
                    Vector3I vectori7 = max << coord.Lod;
                    new BoundingBoxD((Vector3D) (min << coord.Lod), (Vector3D) vectori7).Intersect(ref line, out ed2);
                    if (!node.Intersect(ref ed2, out num3, out num4))
                    {
                        continue;
                    }
                    if (xd.Contains(ed2.From) == ContainmentType.Disjoint)
                    {
                        ed.From = ed2.From;
                        xd.Include(ed2.From);
                    }
                    if (xd.Contains(ed2.To) == ContainmentType.Disjoint)
                    {
                        ed.To = ed2.To;
                        xd.Include(ed2.To);
                    }
                    continue;
                }
                int* numPtr1 = (int*) ref coord2.Lod;
                numPtr1[0]--;
                lod = coord.Lod - 1;
                if (this.m_contentNodes.TryGetValue(coord2.PackId64(), out node2))
                {
                    Vector3I vectori3 = coord.CoordInLod << 1;
                    Vector3I min = (boundingBox.Min >> lod) - vectori3;
                    Vector3I max = (boundingBox.Max >> lod) - vectori3;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3I vectori8;
                        ComputeChildCoord(i, out vectori8);
                        if (vectori8.IsInsideInclusiveEnd(ref min, ref max))
                        {
                            BoundingBoxD xd3;
                            xd3.Min = (vectori8 + coord2.CoordInLod) << lod;
                            BoundingBoxD* xdPtr1 = (BoundingBoxD*) ref xd3;
                            xdPtr1->Max = xd3.Min + (1 << (coord.Lod & 0x1f));
                            if (xd.Contains(xd3) != ContainmentType.Contains)
                            {
                                if (!node2.HasChild(i))
                                {
                                    node2.GetData(i);
                                }
                                else
                                {
                                    index++;
                                    coordPtr[index] = new MyCellCoord(coord.Lod - 1, (Vector3I) (vectori3 + vectori8));
                                }
                            }
                        }
                    }
                }
            }
            if (!xd.Valid)
            {
                return false;
            }
            line.To = ed.To;
            line.From = ed.From;
            line.Length = (ed.To - ed.From).Length();
            return true;
        }

        protected override unsafe ContainmentType IntersectInternal(ref BoundingBoxI box, int lod, bool exhaustiveContainmentCheck)
        {
            MyCellCoord* coordPtr = (MyCellCoord*) stackalloc byte[(((IntPtr) MySparseOctree.EstimateStackSize(this.m_treeHeight)) * sizeof(MyCellCoord))];
            MyCellCoord coord = new MyCellCoord(this.m_treeHeight + 4, ref Vector3I.Zero);
            int index = 0 + 1;
            coordPtr[index] = coord;
            MyCellCoord coord2 = new MyCellCoord();
            BoundingBoxI xi = box;
            ContainmentType intersects = ~ContainmentType.Disjoint;
            while (index > 0)
            {
                int num3;
                IMyOctreeLeafNode node;
                MyOctreeNode node2;
                coord = coordPtr[--index];
                coord2.Lod = Math.Max(coord.Lod - 4, 0);
                coord2.CoordInLod = coord.CoordInLod;
                if (this.m_contentLeaves.TryGetValue(coord2.PackId64(), out node))
                {
                    num3 = coord.Lod;
                    Vector3I min = xi.Min >> num3;
                    Vector3I max = xi.Max >> num3;
                    if (!coord.CoordInLod.IsInsideInclusiveEnd(ref min, ref max))
                    {
                        continue;
                    }
                    Vector3I vectori6 = coord2.CoordInLod << num3;
                    BoundingBoxI xi2 = new BoundingBoxI(vectori6, (Vector3I) (vectori6 + (1 << (num3 & 0x1f))));
                    xi2.IntersectWith(ref xi);
                    ContainmentType type2 = node.Intersect(ref xi2, lod, true);
                    if (intersects == ~ContainmentType.Disjoint)
                    {
                        intersects = type2;
                    }
                    if (type2 == ContainmentType.Intersects)
                    {
                        return ContainmentType.Intersects;
                    }
                    if ((type2 != intersects) || ((intersects == ContainmentType.Contains) & exhaustiveContainmentCheck))
                    {
                        return ContainmentType.Intersects;
                    }
                    continue;
                }
                int* numPtr1 = (int*) ref coord2.Lod;
                numPtr1[0]--;
                num3 = coord.Lod - 1;
                if (this.m_contentNodes.TryGetValue(coord2.PackId64(), out node2))
                {
                    Vector3I vectori3 = coord.CoordInLod << 1;
                    Vector3I min = (xi.Min >> num3) - vectori3;
                    Vector3I max = (xi.Max >> num3) - vectori3;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3I vectori7;
                        ComputeChildCoord(i, out vectori7);
                        if (vectori7.IsInsideInclusiveEnd(ref min, ref max))
                        {
                            if (coord.Lod == 0)
                            {
                                return ContainmentType.Intersects;
                            }
                            if (node2.HasChild(i))
                            {
                                index++;
                                coordPtr[index] = new MyCellCoord(coord.Lod - 1, (Vector3I) (vectori3 + vectori7));
                            }
                            else if ((node2.GetData(i) != 0) && (intersects != ContainmentType.Contains))
                            {
                                intersects = ContainmentType.Intersects;
                            }
                        }
                    }
                }
            }
            if (intersects == ~ContainmentType.Disjoint)
            {
                intersects = ContainmentType.Disjoint;
            }
            return intersects;
        }

        protected override unsafe bool IsModifiedInternal(ref BoundingBoxI range)
        {
            MyCellCoord* coordPtr = (MyCellCoord*) stackalloc byte[(((IntPtr) MySparseOctree.EstimateStackSize(this.m_treeHeight)) * sizeof(MyCellCoord))];
            MyCellCoord coord = new MyCellCoord(this.m_treeHeight + 4, ref Vector3I.Zero);
            int index = 0 + 1;
            coordPtr[index] = coord;
            MyCellCoord coord2 = new MyCellCoord();
            BoundingBoxI xi = range;
            while (index > 0)
            {
                IMyOctreeLeafNode node;
                coord = coordPtr[--index];
                coord2.Lod = Math.Max(coord.Lod - 4, 0);
                coord2.CoordInLod = coord.CoordInLod;
                if (this.m_contentLeaves.TryGetValue(coord2.PackId64(), out node))
                {
                    if (!(node is MyProviderLeaf))
                    {
                        return true;
                    }
                    continue;
                }
                int* numPtr1 = (int*) ref coord2.Lod;
                numPtr1[0]--;
                int num3 = coord.Lod - 1;
                MyOctreeNode node2 = this.m_contentNodes[coord2.PackId64()];
                Vector3I vectori3 = coord.CoordInLod << 1;
                Vector3I min = (xi.Min >> num3) - vectori3;
                Vector3I max = (xi.Max >> num3) - vectori3;
                for (int i = 0; i < 8; i++)
                {
                    Vector3I vectori4;
                    ComputeChildCoord(i, out vectori4);
                    if (vectori4.IsInsideInclusiveEnd(ref min, ref max))
                    {
                        if (coord.Lod == 0)
                        {
                            return true;
                        }
                        if (node2.HasChild(i))
                        {
                            index++;
                            coordPtr[index] = new MyCellCoord(coord.Lod - 1, (Vector3I) (vectori3 + vectori4));
                        }
                    }
                }
            }
            return false;
        }

        protected override unsafe void LoadInternal(int fileVersion, Stream stream, ref bool isOldFormat)
        {
            if (fileVersion == 2)
            {
                base.ReadStorageAccess(stream);
            }
            ChunkHeader header = new ChunkHeader();
            Dictionary<byte, MyVoxelMaterialDefinition> dictionary = null;
            HashSet<ulong> outKeySet = new HashSet<ulong>();
            HashSet<ulong> set2 = new HashSet<ulong>();
            while (header.ChunkType != ChunkTypeEnum.EndOfFile)
            {
                ulong num;
                header.ReadFrom(stream);
                ChunkTypeEnum chunkType = header.ChunkType;
                switch (chunkType)
                {
                    case ChunkTypeEnum.StorageMetaData:
                    {
                        this.ReadStorageMetaData(stream, header, ref isOldFormat);
                        continue;
                    }
                    case ChunkTypeEnum.MaterialIndexTable:
                    {
                        dictionary = ReadMaterialTable(stream, header, ref isOldFormat);
                        continue;
                    }
                    case ChunkTypeEnum.MacroContentNodes:
                    {
                        Action<MyCellCoord> <>9__0;
                        Action<MyCellCoord> loadAccess = <>9__0;
                        if (<>9__0 == null)
                        {
                            Action<MyCellCoord> local1 = <>9__0;
                            loadAccess = <>9__0 = coord => this.LoadAccess(stream, coord);
                        }
                        ReadOctreeNodes(stream, header, ref isOldFormat, this.m_contentNodes, loadAccess);
                        continue;
                    }
                    case ChunkTypeEnum.MacroMaterialNodes:
                    {
                        ReadOctreeNodes(stream, header, ref isOldFormat, this.m_materialNodes, null);
                        continue;
                    }
                    case ChunkTypeEnum.ContentLeafProvider:
                    {
                        this.ReadProviderLeaf(stream, header, ref isOldFormat, set2);
                        continue;
                    }
                    case ChunkTypeEnum.ContentLeafOctree:
                    {
                        MyMicroOctreeLeaf leaf;
                        this.ReadOctreeLeaf(stream, header, ref isOldFormat, MyStorageDataTypeEnum.Content, out num, out leaf);
                        this.m_contentLeaves.Add(num, leaf);
                        continue;
                    }
                    case ChunkTypeEnum.MaterialLeafProvider:
                    {
                        this.ReadProviderLeaf(stream, header, ref isOldFormat, outKeySet);
                        continue;
                    }
                    case ChunkTypeEnum.MaterialLeafOctree:
                    {
                        MyMicroOctreeLeaf leaf2;
                        this.ReadOctreeLeaf(stream, header, ref isOldFormat, MyStorageDataTypeEnum.Material, out num, out leaf2);
                        this.m_materialLeaves.Add(num, leaf2);
                        continue;
                    }
                    case ChunkTypeEnum.DataProvider:
                    {
                        ReadDataProvider(stream, header, ref isOldFormat, out this.m_dataProvider);
                        continue;
                    }
                }
                if (chunkType != ChunkTypeEnum.EndOfFile)
                {
                    throw new InvalidBranchException();
                }
            }
            MyCellCoord cell = new MyCellCoord();
            foreach (ulong num2 in set2)
            {
                cell.SetUnpack(num2);
                int* numPtr1 = (int*) ref cell.Lod;
                numPtr1[0] += 4;
                this.m_contentLeaves.Add(num2, new MyProviderLeaf(this.m_dataProvider, MyStorageDataTypeEnum.Content, ref cell));
            }
            foreach (ulong num3 in outKeySet)
            {
                cell.SetUnpack(num3);
                int* numPtr2 = (int*) ref cell.Lod;
                numPtr2[0] += 4;
                this.m_materialLeaves.Add(num3, new MyProviderLeaf(this.m_dataProvider, MyStorageDataTypeEnum.Material, ref cell));
            }
            bool flag = false;
            foreach (KeyValuePair<byte, MyVoxelMaterialDefinition> pair in dictionary)
            {
                if (pair.Key != pair.Value.Index)
                {
                    flag = true;
                }
                this.m_oldToNewIndexMap.Add(pair.Key, pair.Value.Index);
            }
            if (flag)
            {
                if (this.m_dataProvider != null)
                {
                    this.m_dataProvider.ReindexMaterials(this.m_oldToNewIndexMap);
                }
                foreach (KeyValuePair<ulong, IMyOctreeLeafNode> pair2 in this.m_materialLeaves)
                {
                    pair2.Value.ReplaceValues(this.m_oldToNewIndexMap);
                }
                MySparseOctree.ReplaceValues<ulong>(this.m_materialNodes, this.m_oldToNewIndexMap);
            }
            this.m_oldToNewIndexMap.Clear();
        }

        private TraverseArgs<TOperator> MaterialArgs<TOperator>(ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax) where TOperator: struct, ITraverseOps => 
            new TraverseArgs<TOperator> { 
                Storage = this,
                Operator = default(TOperator),
                DataFilter = MyOctreeNode.MaterialFilter,
                DataType = MyStorageDataTypeEnum.Material,
                Leaves = this.m_materialLeaves,
                ContentLeaves = this.m_contentLeaves,
                Nodes = this.m_materialNodes,
                Min = voxelRangeMin,
                Max = voxelRangeMax
            };

        protected override void OverwriteAllMaterialsInternal(MyVoxelMaterialDefinition material)
        {
        }

        private static unsafe void ReadDataProvider(Stream stream, ChunkHeader header, ref bool isOldFormat, out IMyStorageDataProvider provider)
        {
            int version = header.Version;
            if (version == 1)
            {
                provider = (IMyStorageDataProvider) Activator.CreateInstance(m_attributesById[1].ProviderType);
                provider.ReadFrom(header.Version, stream, header.Size, ref isOldFormat);
                isOldFormat = true;
            }
            else
            {
                if (version != 2)
                {
                    throw new InvalidBranchException();
                }
                int num2 = stream.ReadInt32();
                provider = (IMyStorageDataProvider) Activator.CreateInstance(m_attributesById[num2].ProviderType);
                int* numPtr1 = (int*) ref header.Size;
                numPtr1[0] -= 4;
                provider.ReadFrom(header.Version, stream, header.Size, ref isOldFormat);
            }
        }

        private static Dictionary<byte, MyVoxelMaterialDefinition> ReadMaterialTable(Stream stream, ChunkHeader header, ref bool isOldFormat)
        {
            int capacity = stream.ReadInt32();
            Dictionary<byte, MyVoxelMaterialDefinition> dictionary = new Dictionary<byte, MyVoxelMaterialDefinition>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                int num3 = stream.Read7BitEncodedInt();
                string name = stream.ReadString(null);
                MyVoxelMaterialDefinition voxelMaterialDefinition = MyDefinitionManager.Static.GetVoxelMaterialDefinition(name);
                if (voxelMaterialDefinition == null)
                {
                    voxelMaterialDefinition = new MyVoxelMaterialDefinition();
                }
                dictionary.Add((byte) num3, voxelMaterialDefinition);
            }
            return dictionary;
        }

        private unsafe void ReadOctreeLeaf(Stream stream, ChunkHeader header, ref bool isOldFormat, MyStorageDataTypeEnum dataType, out ulong key, out MyMicroOctreeLeaf contentLeaf)
        {
            MyCellCoord coord = new MyCellCoord();
            if (header.Version > 2)
            {
                key = stream.ReadUInt64();
                coord.SetUnpack(key);
                int* numPtr2 = (int*) ref header.Size;
                numPtr2[0] -= 8;
            }
            else
            {
                coord.SetUnpack(stream.ReadUInt32());
                key = coord.PackId64();
                int* numPtr1 = (int*) ref header.Size;
                numPtr1[0] -= 4;
                isOldFormat = true;
            }
            contentLeaf = new MyMicroOctreeLeaf(dataType, 4, coord.CoordInLod << (coord.Lod + 4));
            contentLeaf.ReadFrom(header, stream);
        }

        private static unsafe void ReadOctreeNodes(Stream stream, ChunkHeader header, ref bool isOldFormat, Dictionary<ulong, MyOctreeNode> contentNodes, Action<MyCellCoord> loadAccess)
        {
            int version = header.Version;
            if (version == 1)
            {
                int num2 = header.Size / 13;
                MyCellCoord coord = new MyCellCoord();
                for (int i = 0; i < num2; i++)
                {
                    MyOctreeNode node;
                    coord.SetUnpack(stream.ReadUInt32());
                    node.ChildMask = stream.ReadByteNoAlloc();
                    stream.ReadNoAlloc(&node.Data.FixedElementField, 0, 8);
                    ulong key = coord.PackId64();
                    contentNodes.Add(key, node);
                    if (loadAccess != null)
                    {
                        MyCellCoord coord2 = new MyCellCoord(MyCellCoord.UnpackLod(key), MyCellCoord.UnpackCoord(key));
                        int* numPtr1 = (int*) ref coord2.Lod;
                        numPtr1[0] += 5;
                        loadAccess(coord2);
                    }
                }
                isOldFormat = true;
            }
            else
            {
                if (version != 2)
                {
                    throw new InvalidBranchException();
                }
                int num5 = header.Size / 0x11;
                for (int i = 0; i < num5; i++)
                {
                    MyOctreeNode node2;
                    ulong key = stream.ReadUInt64();
                    node2.ChildMask = stream.ReadByteNoAlloc();
                    stream.ReadNoAlloc(&node2.Data.FixedElementField, 0, 8);
                    contentNodes.Add(key, node2);
                    if (loadAccess != null)
                    {
                        MyCellCoord coord3 = new MyCellCoord(MyCellCoord.UnpackLod(key), MyCellCoord.UnpackCoord(key));
                        int* numPtr2 = (int*) ref coord3.Lod;
                        numPtr2[0] += 5;
                        loadAccess(coord3);
                    }
                }
            }
        }

        private unsafe void ReadProviderLeaf(Stream stream, ChunkHeader header, ref bool isOldFormat, HashSet<ulong> outKeySet)
        {
            ulong num;
            if (header.Version > 2)
            {
                num = stream.ReadUInt64();
                int* numPtr2 = (int*) ref header.Size;
                numPtr2[0] -= 8;
            }
            else
            {
                MyCellCoord coord = new MyCellCoord();
                coord.SetUnpack(stream.ReadUInt32());
                num = coord.PackId64();
                int* numPtr1 = (int*) ref header.Size;
                numPtr1[0] -= 4;
                isOldFormat = true;
            }
            outKeySet.Add(num);
            stream.SkipBytes(header.Size);
        }

        private unsafe void ReadRange(MyStorageData target, ref Vector3I targetWriteOffset, MyStorageDataTypeFlags types, int treeHeight, Dictionary<ulong, MyOctreeNode> nodes, Dictionary<ulong, IMyOctreeLeafNode> leaves, int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod, ref MyVoxelRequestFlags flags)
        {
            MyCellCoord* coordPtr = (MyCellCoord*) stackalloc byte[(((IntPtr) MySparseOctree.EstimateStackSize(treeHeight)) * sizeof(MyCellCoord))];
            MyCellCoord coord = new MyCellCoord(treeHeight + 4, ref Vector3I.Zero);
            int index = 0 + 1;
            coordPtr[index] = coord;
            MyCellCoord coord2 = new MyCellCoord();
            MyStorageDataTypeEnum type = types.Requests(MyStorageDataTypeEnum.Content) ? MyStorageDataTypeEnum.Content : MyStorageDataTypeEnum.Material;
            Vector3I step = target.Step;
            byte num3 = types.Requests(MyStorageDataTypeEnum.Content) ? ((byte) 0) : ((byte) 0xff);
            byte num4 = 0xff;
            flags |= MyVoxelRequestFlags.FullContent;
            this.FillOutOfBounds(target, type, ref targetWriteOffset, lodIndex, minInLod, maxInLod);
            while (index > 0)
            {
                int num5;
                IMyOctreeLeafNode node;
                MyOctreeNode node2;
                coord = coordPtr[--index];
                coord2.Lod = Math.Max(coord.Lod - 4, 0);
                coord2.CoordInLod = coord.CoordInLod;
                if (leaves.TryGetValue(coord2.PackId64(), out node))
                {
                    num5 = coord.Lod - lodIndex;
                    Vector3I min = minInLod >> num5;
                    Vector3I max = maxInLod >> num5;
                    if (!coord.CoordInLod.IsInsideInclusiveEnd(ref min, ref max))
                    {
                        continue;
                    }
                    Vector3I vectori7 = coord.CoordInLod << num5;
                    Vector3I result = vectori7 - minInLod;
                    Vector3I* vectoriPtr1 = (Vector3I*) ref result;
                    Vector3I.Max(ref (Vector3I) ref vectoriPtr1, ref Vector3I.Zero, out result);
                    result = (Vector3I) (result + targetWriteOffset);
                    Vector3I vectori9 = new Vector3I((1 << (num5 & 0x1f)) - 1);
                    Vector3I vectori10 = minInLod - vectori7;
                    Vector3I vectori11 = maxInLod - vectori7;
                    if (!vectori10.IsInsideInclusiveEnd(Vector3I.Zero, vectori9) || !vectori11.IsInsideInclusiveEnd(Vector3I.Zero, vectori9))
                    {
                        vectori10 = Vector3I.Clamp(minInLod - vectori7, Vector3I.Zero, vectori9);
                        vectori11 = Vector3I.Clamp(maxInLod - vectori7, Vector3I.Zero, vectori9);
                    }
                    MyVoxelRequestFlags flags2 = flags;
                    node.ReadRange(target, types, ref result, lodIndex, ref vectori10, ref vectori11, ref flags2);
                    flags = (flags & (flags2 & (MyVoxelRequestFlags.FullContent | MyVoxelRequestFlags.EmptyData))) | (flags & ~(MyVoxelRequestFlags.FullContent | MyVoxelRequestFlags.EmptyData));
                    continue;
                }
                int* numPtr1 = (int*) ref coord2.Lod;
                numPtr1[0]--;
                num5 = (coord.Lod - 1) - lodIndex;
                if (nodes.TryGetValue(coord2.PackId64(), out node2))
                {
                    Vector3I vectori4 = coord.CoordInLod << 1;
                    Vector3I min = (minInLod >> num5) - vectori4;
                    Vector3I max = (maxInLod >> num5) - vectori4;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3I vectori12;
                        ComputeChildCoord(i, out vectori12);
                        if (vectori12.IsInsideInclusiveEnd(ref min, ref max))
                        {
                            if ((lodIndex < coord.Lod) && node2.HasChild(i))
                            {
                                index++;
                                coordPtr[index] = new MyCellCoord(coord.Lod - 1, (Vector3I) (vectori4 + vectori12));
                            }
                            else
                            {
                                flags &= MyVoxelRequestFlags.RequestFlags;
                                Vector3I result = (Vector3I) ((vectori4 + vectori12) << num5);
                                Vector3I vectori14 = result - minInLod;
                                Vector3I* vectoriPtr2 = (Vector3I*) ref vectori14;
                                Vector3I.Max(ref (Vector3I) ref vectoriPtr2, ref Vector3I.Zero, out vectori14);
                                vectori14 = (Vector3I) (vectori14 + targetWriteOffset);
                                byte data = node2.GetData(i);
                                if (data != num3)
                                {
                                    flags &= ~MyVoxelRequestFlags.EmptyData;
                                }
                                if (data != num4)
                                {
                                    flags &= ~MyVoxelRequestFlags.FullContent;
                                }
                                if (num5 == 0)
                                {
                                    int num8 = vectori14.Dot(ref step);
                                    target[type][num8] = data;
                                }
                                else
                                {
                                    Vector3I vectori15 = (Vector3I) (result + ((1 << (num5 & 0x1f)) - 1));
                                    Vector3I* vectoriPtr3 = (Vector3I*) ref result;
                                    Vector3I.Max(ref (Vector3I) ref vectoriPtr3, ref minInLod, out result);
                                    Vector3I* vectoriPtr4 = (Vector3I*) ref vectori15;
                                    Vector3I.Min(ref (Vector3I) ref vectoriPtr4, ref maxInLod, out vectori15);
                                    int z = result.Z;
                                    while (z <= vectori15.Z)
                                    {
                                        int y = result.Y;
                                        while (true)
                                        {
                                            if (y > vectori15.Y)
                                            {
                                                z++;
                                                break;
                                            }
                                            int x = result.X;
                                            while (true)
                                            {
                                                if (x > vectori15.X)
                                                {
                                                    y++;
                                                    break;
                                                }
                                                Vector3I vectori16 = vectori14;
                                                int* numPtr2 = (int*) ref vectori16.X;
                                                numPtr2[0] += x - result.X;
                                                int* numPtr3 = (int*) ref vectori16.Y;
                                                numPtr3[0] += y - result.Y;
                                                int* numPtr4 = (int*) ref vectori16.Z;
                                                numPtr4[0] += z - result.Z;
                                                int num12 = vectori16.Dot(ref step);
                                                target[type][num12] = data;
                                                x++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void ReadRangeInternal(MyStorageData target, ref Vector3I targetWriteOffset, MyStorageDataTypeFlags dataToRead, int lodIndex, ref Vector3I lodVoxelCoordStart, ref Vector3I lodVoxelCoordEnd, ref MyVoxelRequestFlags flags)
        {
            bool flag = lodIndex <= (this.m_treeHeight + 4);
            MyVoxelRequestFlags flags2 = 0;
            MyVoxelRequestFlags flags3 = 0;
            if (((dataToRead & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None) && flag)
            {
                flags2 = flags;
                this.ReadRange(target, ref targetWriteOffset, MyStorageDataTypeFlags.Content, this.m_treeHeight, this.m_contentNodes, this.m_contentLeaves, lodIndex, ref lodVoxelCoordStart, ref lodVoxelCoordEnd, ref flags2);
            }
            if ((dataToRead.Requests(MyStorageDataTypeEnum.Material) && !flags2.HasFlags(MyVoxelRequestFlags.EmptyData)) && flag)
            {
                flags3 = flags;
                this.ReadRange(target, ref targetWriteOffset, dataToRead.Without(MyStorageDataTypeEnum.Content), this.m_treeHeight, this.m_materialNodes, this.m_materialLeaves, lodIndex, ref lodVoxelCoordStart, ref lodVoxelCoordEnd, ref flags3);
                if ((dataToRead & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None)
                {
                    flags3 &= ~MyVoxelRequestFlags.EmptyData;
                }
                if (flags.HasFlags(MyVoxelRequestFlags.ConsiderContent))
                {
                    ConsiderContent(target);
                }
            }
            flags = flags2 | flags3;
        }

        private void ReadStorageMetaData(Stream stream, ChunkHeader header, ref bool isOldFormat)
        {
            Vector3I vectori;
            stream.ReadInt32();
            vectori.X = stream.ReadInt32();
            vectori.Y = stream.ReadInt32();
            vectori.Z = stream.ReadInt32();
            base.Size = vectori;
            stream.ReadByteNoAlloc();
            this.InitTreeHeight();
            base.AccessReset();
        }

        public static void RegisterTypes(Assembly[] assemblies)
        {
            if (assemblies != null)
            {
                Assembly[] assemblyArray = assemblies;
                for (int i = 0; i < assemblyArray.Length; i++)
                {
                    RegisterTypes(assemblyArray[i]);
                }
            }
        }

        private static void RegisterTypes(Assembly assembly)
        {
            if (assembly != null)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    object[] customAttributes = type.GetCustomAttributes(typeof(MyStorageDataProviderAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length != 0))
                    {
                        MyStorageDataProviderAttribute attribute = (MyStorageDataProviderAttribute) customAttributes[0];
                        attribute.ProviderType = type;
                        m_attributesById.Add(attribute.ProviderTypeId, attribute);
                        m_attributesByType.Add(attribute.ProviderType, attribute);
                    }
                }
            }
        }

        protected override unsafe void ResetInternal(MyStorageDataTypeFlags dataToReset)
        {
            base.AccessReset();
            bool flag = (dataToReset & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None;
            bool flag2 = (dataToReset & MyStorageDataTypeFlags.Material) != MyStorageDataTypeFlags.None;
            if (flag)
            {
                this.m_contentLeaves.Clear();
                this.m_contentNodes.Clear();
            }
            if (flag2)
            {
                this.m_materialLeaves.Clear();
                this.m_materialNodes.Clear();
            }
            if (this.m_dataProvider != null)
            {
                MyCellCoord cell = new MyCellCoord(this.m_treeHeight, ref Vector3I.Zero);
                ulong key = cell.PackId64();
                int* numPtr1 = (int*) ref cell.Lod;
                numPtr1[0] += 4;
                Vector3I vectori1 = base.Size - 1;
                if (flag)
                {
                    this.m_contentLeaves.Add(key, new MyProviderLeaf(this.m_dataProvider, MyStorageDataTypeEnum.Content, ref cell));
                }
                if (flag2)
                {
                    this.m_materialLeaves.Add(key, new MyProviderLeaf(this.m_dataProvider, MyStorageDataTypeEnum.Material, ref cell));
                }
            }
            else
            {
                MyCellCoord coord = new MyCellCoord(this.m_treeHeight - 1, ref Vector3I.Zero);
                ulong key = coord.PackId64();
                int* numPtr2 = (int*) ref coord.Lod;
                numPtr2[0] += 5;
                if (flag)
                {
                    MyOctreeNode node = new MyOctreeNode();
                    this.m_contentNodes.Add(key, node);
                    base.AccessRange(MyStorageBase.MyAccessType.Write, MyStorageDataTypeEnum.Content, ref coord);
                }
                if (flag2)
                {
                    this.m_materialNodes.Add(key, new MyOctreeNode(0xff));
                    base.AccessRange(MyStorageBase.MyAccessType.Write, MyStorageDataTypeEnum.Material, ref coord);
                }
            }
        }

        void IMyCompositeShape.Close()
        {
            if (!base.Shared)
            {
                base.Close();
            }
        }

        protected override void SaveInternal(Stream stream)
        {
            base.WriteStorageAccess(stream);
            this.WriteStorageMetaData(stream);
            WriteMaterialTable(stream);
            WriteDataProvider(stream, this.m_dataProvider);
            WriteOctreeNodes(stream, ChunkTypeEnum.MacroContentNodes, this.m_contentNodes, new Action<Stream, MyCellCoord>(this.SaveAccess));
            WriteOctreeNodes(stream, ChunkTypeEnum.MacroMaterialNodes, this.m_materialNodes, null);
            WriteOctreeLeaves<IMyOctreeLeafNode>(stream, this.m_contentLeaves);
            WriteOctreeLeaves<IMyOctreeLeafNode>(stream, this.m_materialLeaves);
            new ChunkHeader { ChunkType = ChunkTypeEnum.EndOfFile }.WriteTo(stream);
        }

        public float SignedDistance(ref Vector3 localPos, int lodVoxelSize)
        {
            MyStorageData storageCached = m_storageCached;
            m_storageCached = null;
            if (storageCached == null)
            {
                storageCached = new MyStorageData(MyStorageDataTypeFlags.Content);
                storageCached.Resize(Vector3I.One);
            }
            Vector3I lodVoxelRangeMin = new Vector3I(localPos);
            base.ReadRange(storageCached, MyStorageDataTypeFlags.Content, 0, lodVoxelRangeMin, lodVoxelRangeMin);
            m_storageCached = storageCached;
            return (((((float) storageCached.Get(MyStorageDataTypeEnum.Content, 0)) / 255f) - 0.5f) * -2f);
        }

        protected override void SweepInternal(MyStorageDataTypeFlags dataToSweep)
        {
            SweepRangeOps ops = new SweepRangeOps {
                Storage = this
            };
            if ((dataToSweep & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None)
            {
                Vector3I size = base.Size;
                Traverse<SweepRangeOps>(ref this.ContentArgs<SweepRangeOps>(ref Vector3I.Zero, ref size), 0, this.m_treeHeight + 4, Vector3I.Zero);
            }
            if ((dataToSweep & MyStorageDataTypeFlags.Material) != MyStorageDataTypeFlags.None)
            {
                Vector3I size = base.Size;
                Traverse<SweepRangeOps>(ref this.MaterialArgs<SweepRangeOps>(ref Vector3I.Zero, ref size), 0, this.m_treeHeight + 4, Vector3I.Zero);
            }
        }

        private static unsafe ChildType Traverse<TOps>(ref TraverseArgs<TOps> args, byte defaultData, int lodIdx, Vector3I lodCoord) where TOps: struct, ITraverseOps
        {
            MyOctreeNode node;
            MyCellCoord coord = new MyCellCoord(lodIdx, ref lodCoord);
            ChildType nodeWithLeafReadonly = args.Operator.Init<TOps>(ref args, ref coord, defaultData, out node);
            if (nodeWithLeafReadonly == ChildType.Node)
            {
                int num = 0;
                if (lodIdx <= 5)
                {
                    nodeWithLeafReadonly = args.Operator.LeafOp<TOps>(ref args, ref coord, defaultData, ref node);
                }
                else
                {
                    MyCellCoord coord2 = new MyCellCoord {
                        Lod = (lodIdx - 2) - 4
                    };
                    Vector3I vectori = lodCoord << 1;
                    Vector3I min = (args.Min >> (lodIdx - 1)) - vectori;
                    Vector3I max = (args.Max >> (lodIdx - 1)) - vectori;
                    int childIdx = 0;
                    while (true)
                    {
                        Vector3I vectori4;
                        if (childIdx >= 8)
                        {
                            if (num == 8)
                            {
                                nodeWithLeafReadonly = ChildType.NodeWithLeafReadonly;
                            }
                            break;
                        }
                        ComputeChildCoord(childIdx, out vectori4);
                        coord2.CoordInLod = (Vector3I) (vectori + vectori4);
                        if (!vectori4.IsInsideInclusiveEnd(ref min, ref max))
                        {
                            IMyOctreeLeafNode node2;
                            MyCellCoord coord4 = coord2;
                            coord4.Lod = (lodIdx - 1) - 4;
                            ulong key = coord4.PackId64();
                            if (args.Leaves.TryGetValue(key, out node2) && node2.ReadOnly)
                            {
                                num++;
                            }
                        }
                        else
                        {
                            switch (Traverse<TOps>(ref args, node.GetData(childIdx), lodIdx - 1, coord2.CoordInLod))
                            {
                                case ChildType.NodeMissing:
                                    num++;
                                    break;

                                case ChildType.NodeEmpty:
                                {
                                    ulong key = coord2.PackId64();
                                    MyOctreeNode node3 = args.Nodes[key];
                                    node.SetChild(childIdx, false);
                                    node.SetData(childIdx, node3.GetData(0));
                                    args.Nodes.Remove(key);
                                    break;
                                }
                                case ChildType.Node:
                                {
                                    ulong num6 = coord2.PackId64();
                                    MyOctreeNode node4 = args.Nodes[num6];
                                    node.SetChild(childIdx, true);
                                    node.SetData(childIdx, node4.ComputeFilteredValue(args.DataFilter, coord2.Lod));
                                    break;
                                }
                                case ChildType.LeafReadonly:
                                    num++;
                                    break;

                                case ChildType.NodeWithLeafReadonly:
                                {
                                    num++;
                                    ulong key = coord2.PackId64();
                                    args.Nodes.Remove(key);
                                    int lod = (lodIdx - 2) - 4;
                                    Vector3I vectori5 = coord2.CoordInLod << 1;
                                    int num11 = 0;
                                    while (true)
                                    {
                                        Vector3I vectori6;
                                        if (num11 >= 8)
                                        {
                                            MyCellCoord coord5 = new MyCellCoord(lod + 1, ref coord2.CoordInLod);
                                            ulong num10 = coord5.PackId64();
                                            MyCellCoord cell = coord5;
                                            int* numPtr1 = (int*) ref cell.Lod;
                                            numPtr1[0] += 4;
                                            IMyOctreeLeafNode node5 = new MyProviderLeaf(args.Storage.DataProvider, args.DataType, ref cell);
                                            args.Leaves[num10] = node5;
                                            node.SetData(childIdx, node5.GetFilteredValue());
                                            args.Storage.AccessRange(MyStorageBase.MyAccessType.Delete, args.DataType, ref cell);
                                            break;
                                        }
                                        ComputeChildCoord(num11, out vectori6);
                                        ulong num12 = new MyCellCoord(lod, (Vector3I) (vectori5 + vectori6)).PackId64();
                                        args.Leaves.Remove(num12);
                                        num11++;
                                    }
                                    break;
                                }
                                default:
                                    break;
                            }
                        }
                        childIdx++;
                    }
                }
                args.Nodes[new MyCellCoord((lodIdx - 1) - 4, ref lodCoord).PackId64()] = node;
                if (!node.HasChildren && node.AllDataSame())
                {
                    nodeWithLeafReadonly = ChildType.NodeEmpty;
                }
            }
            return nodeWithLeafReadonly;
        }

        public void Voxelize(MyStorageDataTypeFlags data)
        {
            MyVoxelRequestFlags emptyData = MyVoxelRequestFlags.EmptyData;
            MyStorageData target = new MyStorageData(MyStorageDataTypeFlags.All);
            target.Resize(new Vector3I(0x10));
            Vector3I zero = Vector3I.Zero;
            Vector3I end = (Vector3I) ((base.Size / 0x10) - 1);
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref Vector3I.Zero, ref end);
            while (iterator.IsValid())
            {
                Vector3I lodVoxelRangeMin = zero * 0x10;
                Vector3I lodVoxelRangeMax = (Vector3I) (lodVoxelRangeMin + 15);
                this.ReadRangeInternal(target, ref Vector3I.Zero, data, 0, ref lodVoxelRangeMin, ref lodVoxelRangeMax, ref emptyData);
                MyStorageDataWriteOperator source = new MyStorageDataWriteOperator(target);
                this.WriteRangeInternal<MyStorageDataWriteOperator>(ref source, data, ref lodVoxelRangeMin, ref lodVoxelRangeMax);
                iterator.GetNext(out zero);
            }
            base.OnRangeChanged(Vector3I.Zero, base.Size - 1, data);
        }

        private static void WriteDataProvider(Stream stream, IMyStorageDataProvider provider)
        {
            if (provider != null)
            {
                new ChunkHeader { 
                    ChunkType = ChunkTypeEnum.DataProvider,
                    Version = 2,
                    Size = provider.SerializedSize + 4
                }.WriteTo(stream);
                stream.WriteNoAlloc(m_attributesByType[provider.GetType()].ProviderTypeId);
                provider.WriteTo(stream);
            }
        }

        private static void WriteMaterialTable(Stream stream)
        {
            MemoryStream stream2;
            DictionaryValuesReader<string, MyVoxelMaterialDefinition> voxelMaterialDefinitions = MyDefinitionManager.Static.GetVoxelMaterialDefinitions();
            using (stream2 = new MemoryStream(0x400))
            {
                stream2.WriteNoAlloc(voxelMaterialDefinitions.Count);
                foreach (MyVoxelMaterialDefinition definition in voxelMaterialDefinitions)
                {
                    stream2.Write7BitEncodedInt(definition.Index);
                    stream2.WriteNoAlloc(definition.Id.SubtypeName, null);
                }
            }
            byte[] buffer = stream2.ToArray();
            new ChunkHeader { 
                ChunkType = ChunkTypeEnum.MaterialIndexTable,
                Version = 1,
                Size = buffer.Length
            }.WriteTo(stream);
            stream.Write(buffer, 0, buffer.Length);
        }

        private static void WriteOctreeLeaves<TLeaf>(Stream stream, Dictionary<ulong, TLeaf> leaves) where TLeaf: IMyOctreeLeafNode
        {
            foreach (KeyValuePair<ulong, TLeaf> pair in leaves)
            {
                ChunkHeader header2 = new ChunkHeader {
                    ChunkType = pair.Value.SerializedChunkType
                };
                TLeaf local = pair.Value;
                header2.Size = local.SerializedChunkSize + 8;
                header2.Version = 3;
                ChunkHeader header = header2;
                header.WriteTo(stream);
                stream.WriteNoAlloc(pair.Key);
                ChunkTypeEnum chunkType = header.ChunkType;
                switch (chunkType)
                {
                    case ChunkTypeEnum.ContentLeafProvider:
                    case ChunkTypeEnum.MaterialLeafProvider:
                    {
                        continue;
                    }
                    case ChunkTypeEnum.ContentLeafOctree:
                    {
                        (pair.Value as MyMicroOctreeLeaf).WriteTo(stream);
                        continue;
                    }
                    case ChunkTypeEnum.MaterialLeafOctree:
                    {
                        (pair.Value as MyMicroOctreeLeaf).WriteTo(stream);
                        continue;
                    }
                }
                throw new InvalidBranchException();
            }
        }

        private static unsafe void WriteOctreeNodes(Stream stream, ChunkTypeEnum type, Dictionary<ulong, MyOctreeNode> nodes, Action<Stream, MyCellCoord> saveAccess)
        {
            new ChunkHeader { 
                ChunkType = type,
                Version = 2,
                Size = nodes.Count * 0x11
            }.WriteTo(stream);
            foreach (KeyValuePair<ulong, MyOctreeNode> pair in nodes)
            {
                stream.WriteNoAlloc(pair.Key);
                MyOctreeNode node = pair.Value;
                stream.WriteNoAlloc(node.ChildMask);
                stream.WriteNoAlloc(&node.Data.FixedElementField, 0, 8);
                if (saveAccess != null)
                {
                    MyCellCoord coord = new MyCellCoord(MyCellCoord.UnpackLod(pair.Key), MyCellCoord.UnpackCoord(pair.Key));
                    int* numPtr1 = (int*) ref coord.Lod;
                    numPtr1[0] += 5;
                    saveAccess(stream, coord);
                }
            }
        }

        protected override void WriteRangeInternal<TOperator>(ref TOperator source, MyStorageDataTypeFlags dataToWrite, ref Vector3I voxelRangeMin, ref Vector3I voxelRangeMax) where TOperator: struct, IVoxelOperator
        {
            if ((dataToWrite & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None)
            {
                TraverseArgs<WriteRangeOps<TOperator>> args = this.ContentArgs<WriteRangeOps<TOperator>>(ref voxelRangeMin, ref voxelRangeMax);
                args.Operator.Source = source;
                Traverse<WriteRangeOps<TOperator>>(ref args, 0, this.m_treeHeight + 4, Vector3I.Zero);
                source = args.Operator.Source;
            }
            if ((dataToWrite & MyStorageDataTypeFlags.Material) != MyStorageDataTypeFlags.None)
            {
                TraverseArgs<WriteRangeOps<TOperator>> args = this.MaterialArgs<WriteRangeOps<TOperator>>(ref voxelRangeMin, ref voxelRangeMax);
                args.Operator.Source = source;
                Traverse<WriteRangeOps<TOperator>>(ref args, 0, this.m_treeHeight + 4, Vector3I.Zero);
                source = args.Operator.Source;
            }
        }

        private void WriteStorageMetaData(Stream stream)
        {
            new ChunkHeader { 
                ChunkType = ChunkTypeEnum.StorageMetaData,
                Version = 1,
                Size = 0x11
            }.WriteTo(stream);
            stream.WriteNoAlloc(4);
            stream.WriteNoAlloc(base.Size.X);
            stream.WriteNoAlloc(base.Size.Y);
            stream.WriteNoAlloc(base.Size.Z);
            stream.WriteNoAlloc((byte) 0);
        }

        private static MyStorageData TempStorage =>
            (m_temporaryCache ?? (m_temporaryCache = new MyStorageData(MyStorageDataTypeFlags.All)));

        public override IMyStorageDataProvider DataProvider
        {
            get => 
                this.m_dataProvider;
            set
            {
                Dictionary<ulong, IMyOctreeLeafNode>.ValueCollection.Enumerator enumerator;
                this.m_dataProvider = value;
                using (enumerator = this.m_contentLeaves.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.OnDataProviderChanged(value);
                    }
                }
                using (enumerator = this.m_materialLeaves.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.OnDataProviderChanged(value);
                    }
                }
                base.OnRangeChanged(Vector3I.Zero, base.Size - 1, MyStorageDataTypeFlags.All);
            }
        }

        private enum ChildType
        {
            NodeMissing,
            NodeEmpty,
            Node,
            LeafReadonly,
            NodeWithLeafReadonly,
            Leaf
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ChunkHeader
        {
            public MyOctreeStorage.ChunkTypeEnum ChunkType;
            public int Version;
            public int Size;
            public void WriteTo(Stream stream)
            {
                stream.Write7BitEncodedInt((int) this.ChunkType);
                stream.Write7BitEncodedInt(this.Version);
                stream.Write7BitEncodedInt(this.Size);
            }

            public void ReadFrom(Stream stream)
            {
                this.ChunkType = (MyOctreeStorage.ChunkTypeEnum) ((ushort) stream.Read7BitEncodedInt());
                this.Version = stream.Read7BitEncodedInt();
                this.Size = stream.Read7BitEncodedInt();
            }
        }

        public enum ChunkTypeEnum : ushort
        {
            StorageMetaData = 1,
            MaterialIndexTable = 2,
            MacroContentNodes = 3,
            MacroMaterialNodes = 4,
            ContentLeafProvider = 5,
            ContentLeafOctree = 6,
            MaterialLeafProvider = 7,
            MaterialLeafOctree = 8,
            DataProvider = 9,
            EndOfFile = 0xffff
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DeleteRangeOps : MyOctreeStorage.ITraverseOps
        {
            public MyOctreeStorage Storage;
            public unsafe MyOctreeStorage.ChildType Init<TThis>(ref MyOctreeStorage.TraverseArgs<TThis> args, ref MyCellCoord coord, byte defaultData, out MyOctreeNode node) where TThis: struct, MyOctreeStorage.ITraverseOps
            {
                IMyOctreeLeafNode node2;
                node = new MyOctreeNode();
                MyCellCoord coord2 = coord;
                int* numPtr1 = (int*) ref coord2.Lod;
                numPtr1[0] -= 4;
                ulong key = coord2.PackId64();
                if (args.Leaves.TryGetValue(key, out node2) && (coord2.Lod > 0))
                {
                    return (node2.ReadOnly ? MyOctreeStorage.ChildType.LeafReadonly : MyOctreeStorage.ChildType.Leaf);
                }
                int* numPtr2 = (int*) ref coord2.Lod;
                numPtr2[0]--;
                ulong num2 = coord2.PackId64();
                return (args.Nodes.TryGetValue(num2, out node) ? MyOctreeStorage.ChildType.Node : MyOctreeStorage.ChildType.NodeMissing);
            }

            public unsafe MyOctreeStorage.ChildType LeafOp<TThis>(ref MyOctreeStorage.TraverseArgs<TThis> args, ref MyCellCoord coord, byte defaultData, ref MyOctreeNode node) where TThis: struct, MyOctreeStorage.ITraverseOps
            {
                MyCellCoord coord2 = new MyCellCoord();
                Vector3I vectori = coord.CoordInLod << 1;
                Vector3I inclusiveMin = args.Min >> 4;
                Vector3I exclusiveMax = args.Max >> 4;
                int num = 0;
                int childIdx = 0;
                while (true)
                {
                    while (true)
                    {
                        Vector3I vectori4;
                        if (childIdx >= 8)
                        {
                            return ((num != 8) ? MyOctreeStorage.ChildType.Node : MyOctreeStorage.ChildType.NodeWithLeafReadonly);
                        }
                        MyOctreeStorage.ComputeChildCoord(childIdx, out vectori4);
                        coord2.CoordInLod = (Vector3I) (vectori + vectori4);
                        ulong key = coord2.PackId64();
                        if (coord2.CoordInLod.IsInside(ref inclusiveMin, ref exclusiveMax))
                        {
                            if (node.HasChild(childIdx))
                            {
                                IMyOctreeLeafNode node3;
                                if (!args.Leaves.TryGetValue(key, out node3))
                                {
                                    num++;
                                    break;
                                }
                                if (node3.ReadOnly)
                                {
                                    num++;
                                    break;
                                }
                            }
                            MyCellCoord cell = coord2;
                            int* numPtr1 = (int*) ref cell.Lod;
                            numPtr1[0] += 4;
                            IMyOctreeLeafNode node2 = new MyProviderLeaf(args.Storage.DataProvider, args.DataType, ref cell);
                            args.Leaves[key] = node2;
                            node.SetData(childIdx, node2.GetFilteredValue());
                            node.SetChild(childIdx, true);
                            num++;
                        }
                        break;
                    }
                    childIdx++;
                }
            }
        }

        private interface ITraverseOps
        {
            MyOctreeStorage.ChildType Init<THis>(ref MyOctreeStorage.TraverseArgs<THis> args, ref MyCellCoord coord, byte defaultData, out MyOctreeNode node) where THis: struct, MyOctreeStorage.ITraverseOps;
            MyOctreeStorage.ChildType LeafOp<TThis>(ref MyOctreeStorage.TraverseArgs<TThis> args, ref MyCellCoord coord, byte defaultData, ref MyOctreeNode node) where TThis: struct, MyOctreeStorage.ITraverseOps;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SweepRangeOps : MyOctreeStorage.ITraverseOps
        {
            public MyOctreeStorage Storage;
            public unsafe MyOctreeStorage.ChildType Init<TThis>(ref MyOctreeStorage.TraverseArgs<TThis> args, ref MyCellCoord coord, byte defaultData, out MyOctreeNode node) where TThis: struct, MyOctreeStorage.ITraverseOps
            {
                IMyOctreeLeafNode node2;
                node = new MyOctreeNode();
                MyCellCoord coord2 = coord;
                int* numPtr1 = (int*) ref coord2.Lod;
                numPtr1[0] -= 4;
                ulong key = coord2.PackId64();
                if (!args.Leaves.TryGetValue(key, out node2) || (args.Storage.DataProvider == null))
                {
                    byte num3;
                    int* numPtr2 = (int*) ref coord2.Lod;
                    numPtr2[0]--;
                    ulong num2 = coord2.PackId64();
                    if (args.Nodes.TryGetValue(num2, out node))
                    {
                        return MyOctreeStorage.ChildType.Node;
                    }
                    MyCellCoord cell = coord2;
                    int* numPtr3 = (int*) ref cell.Lod;
                    numPtr3[0]++;
                    MyProviderLeaf leaf = new MyProviderLeaf(args.Storage.DataProvider, args.DataType, ref cell);
                    if (!leaf.TryGetUniformValue(out num3) || (defaultData != num3))
                    {
                        return MyOctreeStorage.ChildType.NodeMissing;
                    }
                    args.Leaves[key] = leaf;
                }
                return MyOctreeStorage.ChildType.LeafReadonly;
            }

            public unsafe MyOctreeStorage.ChildType LeafOp<TThis>(ref MyOctreeStorage.TraverseArgs<TThis> args, ref MyCellCoord coord, byte defaultData, ref MyOctreeNode node) where TThis: struct, MyOctreeStorage.ITraverseOps
            {
                MyCellCoord coord2 = new MyCellCoord();
                Vector3I vectori = coord.CoordInLod << 1;
                int num = 0;
                for (int i = 0; i < 8; i++)
                {
                    Vector3I vectori2;
                    MyOctreeStorage.ComputeChildCoord(i, out vectori2);
                    coord2.CoordInLod = (Vector3I) (vectori + vectori2);
                    ulong key = coord2.PackId64();
                    if (node.HasChild(i))
                    {
                        IMyOctreeLeafNode node2;
                        if (args.Leaves.TryGetValue(key, out node2) && node2.ReadOnly)
                        {
                            num++;
                        }
                    }
                    else
                    {
                        byte num4;
                        MyCellCoord cell = coord2;
                        int* numPtr1 = (int*) ref cell.Lod;
                        numPtr1[0] += 4;
                        MyProviderLeaf leaf = new MyProviderLeaf(args.Storage.DataProvider, args.DataType, ref cell);
                        if (leaf.TryGetUniformValue(out num4) && (node.GetData(i) == num4))
                        {
                            args.Leaves[key] = leaf;
                            num++;
                        }
                    }
                }
                return ((num != 8) ? MyOctreeStorage.ChildType.Node : MyOctreeStorage.ChildType.NodeWithLeafReadonly);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TraverseArgs<TOperator> where TOperator: struct, MyOctreeStorage.ITraverseOps
        {
            public TOperator Operator;
            public MyOctreeStorage Storage;
            public Dictionary<ulong, MyOctreeNode> Nodes;
            public Dictionary<ulong, IMyOctreeLeafNode> Leaves;
            public Dictionary<ulong, IMyOctreeLeafNode> ContentLeaves;
            public MyOctreeNode.FilterFunction DataFilter;
            public MyStorageDataTypeEnum DataType;
            public Vector3I Min;
            public Vector3I Max;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WriteRangeOps<TOperator> : MyOctreeStorage.ITraverseOps where TOperator: struct, IVoxelOperator
        {
            public TOperator Source;
            public unsafe MyOctreeStorage.ChildType Init<TThis>(ref MyOctreeStorage.TraverseArgs<TThis> args, ref MyCellCoord coord, byte defaultData, out MyOctreeNode node) where TThis: struct, MyOctreeStorage.ITraverseOps
            {
                args.Storage.AccessRange(MyStorageBase.MyAccessType.Write, args.DataType, ref coord);
                MyCellCoord coord2 = coord;
                int* numPtr1 = (int*) ref coord2.Lod;
                numPtr1[0] -= 4;
                ulong key = coord2.PackId64();
                if (!args.Leaves.ContainsKey(key) || (args.Storage.DataProvider == null))
                {
                    int* numPtr3 = (int*) ref coord2.Lod;
                    numPtr3[0]--;
                    ulong num3 = coord2.PackId64();
                    if (!args.Nodes.TryGetValue(num3, out node))
                    {
                        node = new MyOctreeNode();
                        node.SetAllData(defaultData);
                    }
                }
                else
                {
                    args.Leaves.Remove(key);
                    Vector3I vectori = coord2.CoordInLod << 1;
                    MyCellCoord coord3 = new MyCellCoord {
                        Lod = coord2.Lod - 1
                    };
                    node = new MyOctreeNode();
                    int childIdx = 0;
                    while (true)
                    {
                        Vector3I vectori2;
                        if (childIdx >= 8)
                        {
                            node.SetChildren();
                            break;
                        }
                        MyOctreeStorage.ComputeChildCoord(childIdx, out vectori2);
                        coord3.CoordInLod = (Vector3I) (vectori + vectori2);
                        MyCellCoord cell = coord3;
                        int* numPtr2 = (int*) ref cell.Lod;
                        numPtr2[0] += 4;
                        IMyOctreeLeafNode node2 = new MyProviderLeaf(args.Storage.DataProvider, args.DataType, ref cell);
                        args.Leaves.Add(coord3.PackId64(), node2);
                        node.SetData(childIdx, node2.GetFilteredValue());
                        childIdx++;
                    }
                }
                return MyOctreeStorage.ChildType.Node;
            }

            public unsafe MyOctreeStorage.ChildType LeafOp<TThis>(ref MyOctreeStorage.TraverseArgs<TThis> args, ref MyCellCoord coord, byte defaultData, ref MyOctreeNode node) where TThis: struct, MyOctreeStorage.ITraverseOps
            {
                int num = 0;
                MyCellCoord coord2 = new MyCellCoord();
                Vector3I vectori = coord.CoordInLod << 1;
                Vector3I min = args.Min >> 4;
                Vector3I max = args.Max >> 4;
                for (int i = 0; i < 8; i++)
                {
                    Vector3I vectori4;
                    IMyOctreeLeafNode node2;
                    MyOctreeStorage.ComputeChildCoord(i, out vectori4);
                    coord2.CoordInLod = (Vector3I) (vectori + vectori4);
                    ulong key = coord2.PackId64();
                    if (args.Leaves.TryGetValue(key, out node2) && node2.ReadOnly)
                    {
                        num++;
                    }
                    if (coord2.CoordInLod.IsInsideInclusiveEnd(ref min, ref max))
                    {
                        byte num4;
                        Vector3I result = coord2.CoordInLod << 4;
                        Vector3I vectori6 = (Vector3I) ((result + 0x10) - 1);
                        Vector3I* vectoriPtr1 = (Vector3I*) ref result;
                        Vector3I.Max(ref (Vector3I) ref vectoriPtr1, ref args.Min, out result);
                        Vector3I* vectoriPtr2 = (Vector3I*) ref vectori6;
                        Vector3I.Min(ref (Vector3I) ref vectoriPtr2, ref args.Max, out vectori6);
                        Vector3I readOffset = result - args.Min;
                        Vector3I vectori8 = result - (coord2.CoordInLod << 4);
                        Vector3I vectori9 = vectori6 - (coord2.CoordInLod << 4);
                        byte singleValue = node.GetData(i);
                        if (node2 == null)
                        {
                            MyMicroOctreeLeaf leaf1 = new MyMicroOctreeLeaf(args.DataType, 4, coord2.CoordInLod << (coord2.Lod + 4));
                            leaf1.BuildFrom(singleValue);
                            node2 = leaf1;
                        }
                        if (!node2.ReadOnly)
                        {
                            node2.ExecuteOperation<TOperator>(ref this.Source, ref readOffset, ref vectori8, ref vectori9);
                        }
                        else
                        {
                            num--;
                            MyStorageData tempStorage = MyOctreeStorage.TempStorage;
                            Vector3I end = new Vector3I(15);
                            tempStorage.Resize(Vector3I.Zero, end);
                            tempStorage.Clear(args.DataType, defaultData);
                            if (((vectori8 != Vector3I.Zero) || (vectori9 != end)) || (this.Source.Flags != VoxelOperatorFlags.WriteAll))
                            {
                                MyVoxelRequestFlags emptyData = MyVoxelRequestFlags.EmptyData;
                                if (args.DataType == MyStorageDataTypeEnum.Content)
                                {
                                    node2.ReadRange(tempStorage, args.DataType.ToFlags(), ref Vector3I.Zero, 0, ref Vector3I.Zero, ref end, ref emptyData);
                                }
                                else
                                {
                                    IMyOctreeLeafNode node3;
                                    tempStorage.Clear(MyStorageDataTypeEnum.Content, 0);
                                    if (args.ContentLeaves.TryGetValue(key, out node3))
                                    {
                                        node3.ReadRange(tempStorage, MyStorageDataTypeFlags.Content, ref Vector3I.Zero, 0, ref Vector3I.Zero, ref end, ref emptyData);
                                    }
                                    else
                                    {
                                        Vector3I lodVoxelRangeMin = coord2.CoordInLod << 4;
                                        Vector3I lodVoxelRangeMax = (Vector3I) ((lodVoxelRangeMin + 0x10) - 1);
                                        args.Storage.ReadRangeInternal(tempStorage, ref Vector3I.Zero, MyStorageDataTypeFlags.Content, 0, ref lodVoxelRangeMin, ref lodVoxelRangeMax, ref emptyData);
                                    }
                                    emptyData = MyVoxelRequestFlags.ConsiderContent;
                                    node2.ReadRange(tempStorage, args.DataType.ToFlags(), ref Vector3I.Zero, 0, ref Vector3I.Zero, ref end, ref emptyData);
                                }
                            }
                            byte[] buffer = tempStorage[args.DataType];
                            Vector3I p = vectori8;
                            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref vectori8, ref vectori9);
                            while (true)
                            {
                                if (!iterator.IsValid())
                                {
                                    MyMicroOctreeLeaf leaf2 = new MyMicroOctreeLeaf(args.DataType, 4, coord2.CoordInLod << (coord2.Lod + 4));
                                    leaf2.BuildFrom(tempStorage);
                                    node2 = leaf2;
                                    break;
                                }
                                Vector3I position = (Vector3I) (readOffset + (p - vectori8));
                                int index = tempStorage.ComputeLinear(ref p);
                                this.Source.Op(ref position, args.DataType, ref buffer[index]);
                                iterator.GetNext(out p);
                            }
                        }
                        if (!node2.TryGetUniformValue(out num4))
                        {
                            args.Leaves[key] = node2;
                            node.SetChild(i, true);
                        }
                        else if (args.Storage.DataProvider == null)
                        {
                            args.Leaves.Remove(key);
                            node.SetChild(i, false);
                        }
                        else
                        {
                            byte num7;
                            MyCellCoord cell = coord2;
                            int* numPtr1 = (int*) ref cell.Lod;
                            numPtr1[0] += 4;
                            MyProviderLeaf leaf = new MyProviderLeaf(args.Storage.DataProvider, args.DataType, ref cell);
                            if (!leaf.TryGetUniformValue(out num7) || (num4 != num7))
                            {
                                args.Leaves.Remove(key);
                                node.SetChild(i, false);
                            }
                            else
                            {
                                node2 = leaf;
                                args.Leaves[key] = node2;
                                node.SetChild(i, true);
                                num++;
                            }
                        }
                        node.SetData(i, node2.GetFilteredValue());
                    }
                }
                return ((num != 8) ? MyOctreeStorage.ChildType.Node : MyOctreeStorage.ChildType.NodeWithLeafReadonly);
            }
        }
    }
}

