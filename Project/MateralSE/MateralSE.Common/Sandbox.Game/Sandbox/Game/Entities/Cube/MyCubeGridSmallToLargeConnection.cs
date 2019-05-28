namespace Sandbox.Game.Entities.Cube
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.Groups;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyCubeGridSmallToLargeConnection : MySessionComponentBase
    {
        private static readonly HashSet<MyCubeBlock> m_tmpBlocks = new HashSet<MyCubeBlock>();
        private static readonly HashSet<MySlimBlock> m_tmpSlimBlocks = new HashSet<MySlimBlock>();
        private static readonly HashSet<MySlimBlock> m_tmpSlimBlocks2 = new HashSet<MySlimBlock>();
        private static readonly List<MySlimBlock> m_tmpSlimBlocksList = new List<MySlimBlock>();
        private static readonly HashSet<MyCubeGrid> m_tmpGrids = new HashSet<MyCubeGrid>();
        private static readonly List<MyCubeGrid> m_tmpGridList = new List<MyCubeGrid>();
        private static bool m_smallToLargeCheckEnabled = true;
        private static readonly List<MySlimBlockPair> m_tmpBlockConnections = new List<MySlimBlockPair>();
        public static MyCubeGridSmallToLargeConnection Static;
        private readonly Dictionary<MyCubeGrid, HashSet<MySlimBlockPair>> m_mapLargeGridToConnectedBlocks = new Dictionary<MyCubeGrid, HashSet<MySlimBlockPair>>();
        private readonly Dictionary<MyCubeGrid, HashSet<MySlimBlockPair>> m_mapSmallGridToConnectedBlocks = new Dictionary<MyCubeGrid, HashSet<MySlimBlockPair>>();

        public bool AddBlockSmallToLargeConnection(MySlimBlock block)
        {
            bool flag;
            if (!m_smallToLargeCheckEnabled)
            {
                return true;
            }
            if (!block.CubeGrid.IsStatic)
            {
                goto TR_0001;
            }
            else if ((block.CubeGrid.EnableSmallToLargeConnections && block.CubeGrid.SmallToLargeConnectionsInitialized) && ((block.FatBlock == null) || !block.FatBlock.Components.Has<MyFractureComponentBase>()))
            {
                BoundingBoxD xd;
                flag = false;
                if (block.FatBlock is MyCompoundCubeBlock)
                {
                    foreach (MySlimBlock block2 in (block.FatBlock as MyCompoundCubeBlock).GetBlocks())
                    {
                        bool flag2 = this.AddBlockSmallToLargeConnection(block2);
                        flag |= flag2;
                    }
                    return flag;
                }
                MyCubeSize cubeSizeEnum = (GetCubeSize(block) == MyCubeSize.Large) ? MyCubeSize.Small : MyCubeSize.Large;
                this.GetSurroundingBlocksFromStaticGrids(block, cubeSizeEnum, m_tmpSlimBlocks2);
                if (m_tmpSlimBlocks2.Count == 0)
                {
                    return false;
                }
                MyDefinitionManager.Static.GetCubeSize(MyCubeSize.Small);
                block.GetWorldBoundingBox(out xd, false);
                xd.Inflate((double) 0.05);
                if (GetCubeSize(block) == MyCubeSize.Large)
                {
                    foreach (MySlimBlock block3 in m_tmpSlimBlocks2)
                    {
                        BoundingBoxD xd2;
                        block3.GetWorldBoundingBox(out xd2, false);
                        if (xd2.Intersects(xd) && this.SmallBlockConnectsToLarge(block3, ref xd2, block, ref xd))
                        {
                            this.ConnectSmallToLargeBlock(block3, block);
                            flag = true;
                        }
                    }
                    return flag;
                }
                foreach (MySlimBlock block4 in m_tmpSlimBlocks2)
                {
                    BoundingBoxD xd3;
                    block4.GetWorldBoundingBox(out xd3, false);
                    if (xd3.Intersects(xd) && this.SmallBlockConnectsToLarge(block, ref xd, block4, ref xd3))
                    {
                        this.ConnectSmallToLargeBlock(block, block4);
                        flag = true;
                    }
                }
            }
            else
            {
                goto TR_0001;
            }
            return flag;
        TR_0001:
            return false;
        }

        internal bool AddGridSmallToLargeConnection(MyCubeGrid grid)
        {
            if (!grid.IsStatic)
            {
                return false;
            }
            if (!grid.EnableSmallToLargeConnections || !grid.SmallToLargeConnectionsInitialized)
            {
                return false;
            }
            bool flag = false;
            foreach (MySlimBlock block in grid.GetBlocks())
            {
                if (!(block.FatBlock is MyFracturedBlock))
                {
                    flag |= this.AddBlockSmallToLargeConnection(block);
                }
            }
            return flag;
        }

        internal void AfterGridMerge_SmallToLargeGridConnectivity(MyCubeGrid originalGrid)
        {
            m_smallToLargeCheckEnabled = true;
            if ((m_tmpGrids.Count != 0) && originalGrid.IsStatic)
            {
                if (originalGrid.GridSizeEnum == MyCubeSize.Large)
                {
                    foreach (MyCubeGrid grid in m_tmpGrids)
                    {
                        HashSet<MySlimBlockPair> set;
                        if (this.m_mapLargeGridToConnectedBlocks.TryGetValue(grid, out set))
                        {
                            m_tmpBlockConnections.Clear();
                            m_tmpBlockConnections.AddRange(set);
                            foreach (MySlimBlockPair pair in m_tmpBlockConnections)
                            {
                                this.DisconnectSmallToLargeBlock(pair.Child, pair.Child.CubeGrid, pair.Parent, grid);
                                this.ConnectSmallToLargeBlock(pair.Child, pair.Parent);
                            }
                        }
                    }
                }
                else
                {
                    foreach (MyCubeGrid grid2 in m_tmpGrids)
                    {
                        HashSet<MySlimBlockPair> set2;
                        if (this.m_mapSmallGridToConnectedBlocks.TryGetValue(grid2, out set2))
                        {
                            m_tmpBlockConnections.Clear();
                            m_tmpBlockConnections.AddRange(set2);
                            foreach (MySlimBlockPair pair2 in m_tmpBlockConnections)
                            {
                                this.DisconnectSmallToLargeBlock(pair2.Child, grid2, pair2.Parent, pair2.Parent.CubeGrid);
                                this.ConnectSmallToLargeBlock(pair2.Child, pair2.Parent);
                            }
                        }
                    }
                }
                m_tmpGrids.Clear();
                m_tmpBlockConnections.Clear();
            }
        }

        private void AfterGridSplit_Large(MyCubeGrid originalGrid, List<MyCubeGrid> gridSplits)
        {
            HashSet<MySlimBlockPair> set;
            if (originalGrid.IsStatic && this.m_mapLargeGridToConnectedBlocks.TryGetValue(originalGrid, out set))
            {
                m_tmpBlockConnections.Clear();
                foreach (MySlimBlockPair pair in set)
                {
                    if (!ReferenceEquals(pair.Parent.CubeGrid, originalGrid))
                    {
                        m_tmpBlockConnections.Add(pair);
                    }
                }
                foreach (MySlimBlockPair pair2 in m_tmpBlockConnections)
                {
                    this.DisconnectSmallToLargeBlock(pair2.Child, pair2.Child.CubeGrid, pair2.Parent, originalGrid);
                    this.ConnectSmallToLargeBlock(pair2.Child, pair2.Parent);
                }
                m_tmpBlockConnections.Clear();
            }
        }

        private void AfterGridSplit_Small(MyCubeGrid originalGrid, List<MyCubeGrid> gridSplits)
        {
            if (originalGrid.IsStatic)
            {
                HashSet<MySlimBlockPair> set;
                if (this.m_mapSmallGridToConnectedBlocks.TryGetValue(originalGrid, out set))
                {
                    m_tmpBlockConnections.Clear();
                    foreach (MySlimBlockPair pair in set)
                    {
                        if (!ReferenceEquals(pair.Child.CubeGrid, originalGrid))
                        {
                            m_tmpBlockConnections.Add(pair);
                        }
                    }
                    foreach (MySlimBlockPair pair2 in m_tmpBlockConnections)
                    {
                        this.DisconnectSmallToLargeBlock(pair2.Child, originalGrid, pair2.Parent, pair2.Parent.CubeGrid);
                        this.ConnectSmallToLargeBlock(pair2.Child, pair2.Parent);
                    }
                    m_tmpBlockConnections.Clear();
                }
                if (Sync.IsServer)
                {
                    if (!this.m_mapSmallGridToConnectedBlocks.TryGetValue(originalGrid, out set) || (set.Count == 0))
                    {
                        originalGrid.TestDynamic = MyCubeGrid.MyTestDynamicReason.GridSplit;
                    }
                    foreach (MyCubeGrid grid in gridSplits)
                    {
                        if (!this.m_mapSmallGridToConnectedBlocks.TryGetValue(grid, out set) || (set.Count == 0))
                        {
                            grid.TestDynamic = MyCubeGrid.MyTestDynamicReason.GridSplit;
                        }
                    }
                }
            }
        }

        internal void AfterGridSplit_SmallToLargeGridConnectivity(MyCubeGrid originalGrid, List<MyCubeGrid> gridSplits)
        {
            m_smallToLargeCheckEnabled = true;
            if (originalGrid.GridSizeEnum == MyCubeSize.Small)
            {
                this.AfterGridSplit_Small(originalGrid, gridSplits);
            }
            else
            {
                this.AfterGridSplit_Large(originalGrid, gridSplits);
            }
        }

        internal void BeforeGridMerge_SmallToLargeGridConnectivity(MyCubeGrid originalGrid, MyCubeGrid mergedGrid)
        {
            m_tmpGrids.Clear();
            if (originalGrid.IsStatic && mergedGrid.IsStatic)
            {
                m_tmpGrids.Add(mergedGrid);
            }
            m_smallToLargeCheckEnabled = false;
        }

        internal void BeforeGridSplit_SmallToLargeGridConnectivity(MyCubeGrid originalGrid)
        {
            m_smallToLargeCheckEnabled = false;
        }

        private static void CheckNeighborBlocks(MySlimBlock block, BoundingBoxD aabbForNeighbors, MyCubeGrid cubeGrid, List<MySlimBlock> blocks)
        {
            MatrixD m = block.CubeGrid.WorldMatrix * cubeGrid.PositionComp.WorldMatrixNormalizedInv;
            BoundingBoxD xd2 = aabbForNeighbors.TransformFast(ref m);
            Vector3I vectori = Vector3I.Round((Vector3D) (cubeGrid.GridSizeR * xd2.Max));
            Vector3I vectori1 = Vector3I.Round((Vector3D) (cubeGrid.GridSizeR * xd2.Min));
            Vector3I start = Vector3I.Min(vectori1, vectori);
            Vector3I end = Vector3I.Max(vectori1, vectori);
            int index = blocks.Count - 1;
            while (true)
            {
                bool flag;
                while (true)
                {
                    if (index < 0)
                    {
                        return;
                    }
                    MySlimBlock block2 = blocks[index];
                    flag = false;
                    Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref block2.Min, ref block2.Max);
                    Vector3I current = iterator.Current;
                    while (true)
                    {
                        if (!iterator.IsValid())
                        {
                            break;
                        }
                        Vector3I_RangeIterator iterator2 = new Vector3I_RangeIterator(ref start, ref end);
                        Vector3I next = iterator2.Current;
                        while (true)
                        {
                            if (iterator2.IsValid())
                            {
                                Vector3I vectori6 = Vector3I.Abs(current - next);
                                if ((next != current) && (((vectori6.X + vectori6.Y) + vectori6.Z) != 1))
                                {
                                    iterator2.GetNext(out next);
                                    continue;
                                }
                                flag = true;
                            }
                            if (flag)
                            {
                                break;
                            }
                            else
                            {
                                iterator.GetNext(out current);
                            }
                            break;
                        }
                    }
                    break;
                }
                if (!flag)
                {
                    blocks.RemoveAt(index);
                }
                index--;
            }
        }

        private void ConnectSmallToLargeBlock(MySlimBlock smallBlock, MySlimBlock largeBlock)
        {
            if (((GetCubeSize(smallBlock) == MyCubeSize.Small) && ((GetCubeSize(largeBlock) == MyCubeSize.Large) && !(smallBlock.FatBlock is MyCompoundCubeBlock))) && !(largeBlock.FatBlock is MyCompoundCubeBlock))
            {
                long linkId = (largeBlock.UniqueId << 0x20) + smallBlock.UniqueId;
                if (!MyCubeGridGroups.Static.SmallToLargeBlockConnections.LinkExists(linkId, largeBlock, null))
                {
                    HashSet<MySlimBlockPair> set;
                    HashSet<MySlimBlockPair> set2;
                    MyCubeGridGroups.Static.SmallToLargeBlockConnections.CreateLink(linkId, largeBlock, smallBlock);
                    MyCubeGridGroups.Static.Physical.CreateLink(linkId, largeBlock.CubeGrid, smallBlock.CubeGrid);
                    MyCubeGridGroups.Static.Logical.CreateLink(linkId, largeBlock.CubeGrid, smallBlock.CubeGrid);
                    MySlimBlockPair item = new MySlimBlockPair {
                        Parent = largeBlock,
                        Child = smallBlock
                    };
                    if (!this.m_mapLargeGridToConnectedBlocks.TryGetValue(largeBlock.CubeGrid, out set))
                    {
                        set = new HashSet<MySlimBlockPair>();
                        this.m_mapLargeGridToConnectedBlocks.Add(largeBlock.CubeGrid, set);
                        largeBlock.CubeGrid.OnClosing += new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnClosing);
                    }
                    set.Add(item);
                    if (!this.m_mapSmallGridToConnectedBlocks.TryGetValue(smallBlock.CubeGrid, out set2))
                    {
                        set2 = new HashSet<MySlimBlockPair>();
                        this.m_mapSmallGridToConnectedBlocks.Add(smallBlock.CubeGrid, set2);
                        smallBlock.CubeGrid.OnClosing += new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnClosing);
                    }
                    set2.Add(item);
                }
            }
        }

        internal void ConvertToDynamic(MyCubeGrid grid)
        {
            if (grid.GridSizeEnum == MyCubeSize.Small)
            {
                this.RemoveSmallGridConnections(grid);
            }
            else
            {
                this.RemoveLargeGridConnections(grid);
            }
        }

        private void CubeGrid_OnClosing(VRage.Game.Entity.MyEntity entity)
        {
            MyCubeGrid grid = (MyCubeGrid) entity;
            if (grid.GridSizeEnum == MyCubeSize.Small)
            {
                this.RemoveSmallGridConnections(grid);
            }
            else
            {
                this.RemoveLargeGridConnections(grid);
            }
        }

        private void DisconnectSmallToLargeBlock(MySlimBlock smallBlock, MySlimBlock largeBlock)
        {
            this.DisconnectSmallToLargeBlock(smallBlock, smallBlock.CubeGrid, largeBlock, largeBlock.CubeGrid);
        }

        private void DisconnectSmallToLargeBlock(MySlimBlock smallBlock, MyCubeGrid smallGrid, MySlimBlock largeBlock, MyCubeGrid largeGrid)
        {
            if (((GetCubeSize(smallBlock) == MyCubeSize.Small) && ((GetCubeSize(largeBlock) == MyCubeSize.Large) && !(smallBlock.FatBlock is MyCompoundCubeBlock))) && !(largeBlock.FatBlock is MyCompoundCubeBlock))
            {
                HashSet<MySlimBlockPair> set;
                HashSet<MySlimBlockPair> set2;
                long linkId = (largeBlock.UniqueId << 0x20) + smallBlock.UniqueId;
                MyCubeGridGroups.Static.SmallToLargeBlockConnections.BreakLink(linkId, largeBlock, null);
                MyCubeGridGroups.Static.Physical.BreakLink(linkId, largeGrid, null);
                MyCubeGridGroups.Static.Logical.BreakLink(linkId, largeGrid, null);
                MySlimBlockPair item = new MySlimBlockPair {
                    Parent = largeBlock,
                    Child = smallBlock
                };
                if (this.m_mapLargeGridToConnectedBlocks.TryGetValue(largeGrid, out set))
                {
                    set.Remove(item);
                    if (set.Count == 0)
                    {
                        this.m_mapLargeGridToConnectedBlocks.Remove(largeGrid);
                        largeGrid.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnClosing);
                    }
                }
                if (this.m_mapSmallGridToConnectedBlocks.TryGetValue(smallGrid, out set2))
                {
                    set2.Remove(item);
                    if (set2.Count == 0)
                    {
                        this.m_mapSmallGridToConnectedBlocks.Remove(smallGrid);
                        smallGrid.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnClosing);
                    }
                }
            }
        }

        private static MyCubeSize GetCubeSize(MySlimBlock block)
        {
            MyCubeBlockDefinition definition;
            if (block.CubeGrid != null)
            {
                return block.CubeGrid.GridSizeEnum;
            }
            MyFracturedBlock fatBlock = block.FatBlock as MyFracturedBlock;
            if (((fatBlock == null) || (fatBlock.OriginalBlocks.Count <= 0)) || !MyDefinitionManager.Static.TryGetCubeBlockDefinition(fatBlock.OriginalBlocks[0], out definition))
            {
                return block.BlockDefinition.CubeSize;
            }
            return definition.CubeSize;
        }

        private Vector3I GetSmallBlockAddDirection(ref BoundingBoxD smallBlockWorldAabb, ref BoundingBoxD smallBlockWorldAabbReduced, ref BoundingBoxD largeBlockWorldAabb)
        {
            if ((smallBlockWorldAabbReduced.Min.X > largeBlockWorldAabb.Max.X) && (smallBlockWorldAabb.Min.X <= largeBlockWorldAabb.Max.X))
            {
                return Vector3I.UnitX;
            }
            if ((smallBlockWorldAabbReduced.Max.X < largeBlockWorldAabb.Min.X) && (smallBlockWorldAabb.Max.X >= largeBlockWorldAabb.Min.X))
            {
                return -Vector3I.UnitX;
            }
            if ((smallBlockWorldAabbReduced.Min.Y > largeBlockWorldAabb.Max.Y) && (smallBlockWorldAabb.Min.Y <= largeBlockWorldAabb.Max.Y))
            {
                return Vector3I.UnitY;
            }
            if ((smallBlockWorldAabbReduced.Max.Y < largeBlockWorldAabb.Min.Y) && (smallBlockWorldAabb.Max.Y >= largeBlockWorldAabb.Min.Y))
            {
                return -Vector3I.UnitY;
            }
            if ((smallBlockWorldAabbReduced.Min.Z <= largeBlockWorldAabb.Max.Z) || (smallBlockWorldAabb.Min.Z > largeBlockWorldAabb.Max.Z))
            {
                return -Vector3I.UnitZ;
            }
            return Vector3I.UnitZ;
        }

        private void GetSurroundingBlocksFromStaticGrids(MySlimBlock block, MyCubeSize cubeSizeEnum, HashSet<MySlimBlock> outBlocks)
        {
            outBlocks.Clear();
            BoundingBoxD aabbForNeighbors = new BoundingBoxD((Vector3D) (block.Min * block.CubeGrid.GridSize), (Vector3D) (block.Max * block.CubeGrid.GridSize));
            BoundingBoxD box = new BoundingBoxD((Vector3D) ((block.Min * block.CubeGrid.GridSize) - (block.CubeGrid.GridSize / 2f)), (block.Max * block.CubeGrid.GridSize) + (block.CubeGrid.GridSize / 2f));
            if (block.FatBlock != null)
            {
                Matrix matrix;
                box = block.FatBlock.Model.BoundingBox;
                block.FatBlock.Orientation.GetMatrix(out matrix);
                box = box.TransformFast(matrix);
                box.Translate(box.Center);
            }
            box.Inflate((double) 0.125);
            BoundingBoxD boundingBox = box.TransformFast(block.CubeGrid.WorldMatrix);
            List<VRage.Game.Entity.MyEntity> foundElements = new List<VRage.Game.Entity.MyEntity>();
            Sandbox.Game.Entities.MyEntities.GetElementsInBox(ref boundingBox, foundElements);
            for (int i = 0; i < foundElements.Count; i++)
            {
                MyCubeGrid objA = foundElements[i] as MyCubeGrid;
                if (((objA != null) && (objA.IsStatic && (!ReferenceEquals(objA, block.CubeGrid) && (objA.EnableSmallToLargeConnections && objA.SmallToLargeConnectionsInitialized)))) && (objA.GridSizeEnum == cubeSizeEnum))
                {
                    m_tmpSlimBlocksList.Clear();
                    objA.GetBlocksIntersectingOBB(box, block.CubeGrid.WorldMatrix, m_tmpSlimBlocksList);
                    CheckNeighborBlocks(block, aabbForNeighbors, objA, m_tmpSlimBlocksList);
                    foreach (MySlimBlock block2 in m_tmpSlimBlocksList)
                    {
                        if (block2.FatBlock == null)
                        {
                            outBlocks.Add(block2);
                            continue;
                        }
                        if (!(block2.FatBlock is MyFracturedBlock) && !block2.FatBlock.Components.Has<MyFractureComponentBase>())
                        {
                            if (block2.FatBlock is MyCompoundCubeBlock)
                            {
                                foreach (MySlimBlock block3 in (block2.FatBlock as MyCompoundCubeBlock).GetBlocks())
                                {
                                    if (!block3.FatBlock.Components.Has<MyFractureComponentBase>())
                                    {
                                        outBlocks.Add(block3);
                                    }
                                }
                                continue;
                            }
                            outBlocks.Add(block2);
                        }
                    }
                    m_tmpSlimBlocksList.Clear();
                }
            }
            foundElements.Clear();
        }

        private void GetSurroundingBlocksFromStaticGrids(MySlimBlock block, MyCubeSize cubeSizeEnum, HashSet<MyCubeBlock> outBlocks)
        {
            outBlocks.Clear();
            BoundingBoxD boundingBox = new BoundingBoxD((Vector3D) ((block.Min * block.CubeGrid.GridSize) - (block.CubeGrid.GridSize / 2f)), (block.Max * block.CubeGrid.GridSize) + (block.CubeGrid.GridSize / 2f));
            if (block.FatBlock != null)
            {
                Matrix matrix;
                boundingBox = block.FatBlock.Model.BoundingBox;
                block.FatBlock.Orientation.GetMatrix(out matrix);
                boundingBox = boundingBox.TransformFast(matrix);
                boundingBox.Translate(boundingBox.Center);
            }
            boundingBox = boundingBox.TransformFast(block.CubeGrid.WorldMatrix);
            boundingBox.Inflate((double) 0.125);
            List<VRage.Game.Entity.MyEntity> foundElements = new List<VRage.Game.Entity.MyEntity>();
            Sandbox.Game.Entities.MyEntities.GetElementsInBox(ref boundingBox, foundElements);
            int num = 0;
            while (true)
            {
                while (true)
                {
                    if (num >= foundElements.Count)
                    {
                        foundElements.Clear();
                        return;
                    }
                    MyCubeBlock item = foundElements[num] as MyCubeBlock;
                    if (((item != null) && (!ReferenceEquals(item.SlimBlock, block) && (item.CubeGrid.IsStatic && (item.CubeGrid.EnableSmallToLargeConnections && (item.CubeGrid.SmallToLargeConnectionsInitialized && (!ReferenceEquals(item.CubeGrid, block.CubeGrid) && ((item.CubeGrid.GridSizeEnum == cubeSizeEnum) && !(item is MyFracturedBlock)))))))) && !item.Components.Has<MyFractureComponentBase>())
                    {
                        MyCompoundCubeBlock block3 = item as MyCompoundCubeBlock;
                        if (block3 != null)
                        {
                            foreach (MySlimBlock block4 in block3.GetBlocks())
                            {
                                if (ReferenceEquals(block4, block))
                                {
                                    continue;
                                }
                                if (!block4.FatBlock.Components.Has<MyFractureComponentBase>())
                                {
                                    outBlocks.Add(block4.FatBlock);
                                }
                            }
                            break;
                        }
                        outBlocks.Add(item);
                    }
                    break;
                }
                num++;
            }
        }

        public override void LoadData()
        {
            base.LoadData();
            Static = this;
        }

        internal void RemoveBlockSmallToLargeConnection(MySlimBlock block)
        {
            if (m_smallToLargeCheckEnabled && block.CubeGrid.IsStatic)
            {
                MyCompoundCubeBlock fatBlock = block.FatBlock as MyCompoundCubeBlock;
                if (fatBlock != null)
                {
                    foreach (MySlimBlock block3 in fatBlock.GetBlocks())
                    {
                        this.RemoveBlockSmallToLargeConnection(block3);
                    }
                }
                else
                {
                    m_tmpGrids.Clear();
                    if (GetCubeSize(block) == MyCubeSize.Large)
                    {
                        this.RemoveChangedLargeBlockConnectionToSmallBlocks(block, m_tmpGrids);
                        if (Sync.IsServer)
                        {
                            foreach (MyCubeGrid grid in m_tmpGrids)
                            {
                                if (grid.TestDynamic != MyCubeGrid.MyTestDynamicReason.NoReason)
                                {
                                    continue;
                                }
                                if (!this.SmallGridIsStatic(grid))
                                {
                                    grid.TestDynamic = MyCubeGrid.MyTestDynamicReason.GridSplit;
                                }
                            }
                        }
                        m_tmpGrids.Clear();
                    }
                    else
                    {
                        MyGroups<MySlimBlock, MyBlockGroupData>.Group group = MyCubeGridGroups.Static.SmallToLargeBlockConnections.GetGroup(block);
                        if (group == null)
                        {
                            if ((Sync.IsServer && ((block.CubeGrid.GetBlocks().Count > 0) && (block.CubeGrid.TestDynamic == MyCubeGrid.MyTestDynamicReason.NoReason))) && !this.SmallGridIsStatic(block.CubeGrid))
                            {
                                block.CubeGrid.TestDynamic = MyCubeGrid.MyTestDynamicReason.GridSplit;
                            }
                        }
                        else
                        {
                            HashSet<MySlimBlockPair> set;
                            m_tmpSlimBlocks.Clear();
                            foreach (MyGroups<MySlimBlock, MyBlockGroupData>.Node node in group.Nodes)
                            {
                                SortedDictionaryValuesReader<long, MyGroups<MySlimBlock, MyBlockGroupData>.Node> children = node.Children;
                                using (SortedDictionary<long, MyGroups<MySlimBlock, MyBlockGroupData>.Node>.ValueCollection.Enumerator enumerator4 = children.GetEnumerator())
                                {
                                    while (enumerator4.MoveNext())
                                    {
                                        if (enumerator4.Current.NodeData == block)
                                        {
                                            m_tmpSlimBlocks.Add(node.NodeData);
                                            break;
                                        }
                                    }
                                }
                            }
                            foreach (MySlimBlock block4 in m_tmpSlimBlocks)
                            {
                                this.DisconnectSmallToLargeBlock(block, block4);
                            }
                            m_tmpSlimBlocks.Clear();
                            if ((Sync.IsServer && (!this.m_mapSmallGridToConnectedBlocks.TryGetValue(block.CubeGrid, out set) && ((block.CubeGrid.GetBlocks().Count > 0) && (block.CubeGrid.TestDynamic == MyCubeGrid.MyTestDynamicReason.NoReason)))) && !this.SmallGridIsStatic(block.CubeGrid))
                            {
                                block.CubeGrid.TestDynamic = MyCubeGrid.MyTestDynamicReason.GridSplit;
                            }
                        }
                    }
                }
            }
        }

        private void RemoveChangedLargeBlockConnectionToSmallBlocks(MySlimBlock block, HashSet<MyCubeGrid> outSmallGrids)
        {
            MyGroups<MySlimBlock, MyBlockGroupData>.Group group = MyCubeGridGroups.Static.SmallToLargeBlockConnections.GetGroup(block);
            if (group != null)
            {
                m_tmpSlimBlocks.Clear();
                foreach (MyGroups<MySlimBlock, MyBlockGroupData>.Node node in group.Nodes)
                {
                    if (node.NodeData != block)
                    {
                        continue;
                    }
                    SortedDictionary<long, MyGroups<MySlimBlock, MyBlockGroupData>.Node>.ValueCollection.Enumerator enumerator = node.Children.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            MyGroups<MySlimBlock, MyBlockGroupData>.Node current = enumerator.Current;
                            m_tmpSlimBlocks.Add(current.NodeData);
                        }
                    }
                    finally
                    {
                        enumerator.Dispose();
                        continue;
                    }
                    break;
                }
                foreach (MySlimBlock block2 in m_tmpSlimBlocks)
                {
                    this.DisconnectSmallToLargeBlock(block2, block);
                    outSmallGrids.Add(block2.CubeGrid);
                }
                m_tmpSlimBlocks.Clear();
                m_tmpGridList.Clear();
                foreach (MyCubeGrid grid in outSmallGrids)
                {
                    HashSet<MySlimBlockPair> set;
                    if (this.m_mapSmallGridToConnectedBlocks.TryGetValue(grid, out set))
                    {
                        m_tmpGridList.Add(grid);
                    }
                }
                foreach (MyCubeGrid grid2 in m_tmpGridList)
                {
                    outSmallGrids.Remove(grid2);
                }
                m_tmpGridList.Clear();
            }
        }

        private void RemoveLargeGridConnections(MyCubeGrid grid)
        {
            HashSet<MySlimBlockPair> set;
            m_tmpGrids.Clear();
            if (this.m_mapLargeGridToConnectedBlocks.TryGetValue(grid, out set))
            {
                m_tmpBlockConnections.Clear();
                m_tmpBlockConnections.AddRange(set);
                foreach (MySlimBlockPair pair in m_tmpBlockConnections)
                {
                    this.DisconnectSmallToLargeBlock(pair.Child, pair.Parent);
                    m_tmpGrids.Add(pair.Child.CubeGrid);
                }
                m_tmpBlockConnections.Clear();
                if (Sync.IsServer)
                {
                    m_tmpGridList.Clear();
                    foreach (MyCubeGrid grid2 in m_tmpGrids)
                    {
                        if (this.m_mapSmallGridToConnectedBlocks.ContainsKey(grid2))
                        {
                            m_tmpGridList.Add(grid2);
                        }
                    }
                    foreach (MyCubeGrid grid3 in m_tmpGridList)
                    {
                        m_tmpGrids.Remove(grid3);
                    }
                    m_tmpGridList.Clear();
                    foreach (MyCubeGrid grid4 in m_tmpGrids)
                    {
                        if (!grid4.IsStatic)
                        {
                            continue;
                        }
                        if ((grid4.TestDynamic == MyCubeGrid.MyTestDynamicReason.NoReason) && !this.SmallGridIsStatic(grid4))
                        {
                            grid4.TestDynamic = MyCubeGrid.MyTestDynamicReason.GridSplit;
                        }
                    }
                }
                m_tmpGrids.Clear();
            }
        }

        private void RemoveSmallGridConnections(MyCubeGrid grid)
        {
            HashSet<MySlimBlockPair> set;
            if (this.m_mapSmallGridToConnectedBlocks.TryGetValue(grid, out set))
            {
                m_tmpBlockConnections.Clear();
                m_tmpBlockConnections.AddRange(set);
                foreach (MySlimBlockPair pair in m_tmpBlockConnections)
                {
                    this.DisconnectSmallToLargeBlock(pair.Child, pair.Parent);
                }
                m_tmpBlockConnections.Clear();
            }
        }

        private bool SmallBlockConnectsToLarge(MySlimBlock smallBlock, ref BoundingBoxD smallBlockWorldAabb, MySlimBlock largeBlock, ref BoundingBoxD largeBlockWorldAabb)
        {
            Quaternion quaternion;
            Vector3D vectord2;
            BoundingBoxD box = smallBlockWorldAabb;
            box.Inflate((double) (-smallBlock.CubeGrid.GridSize / 4f));
            if (!largeBlockWorldAabb.Intersects(box))
            {
                Quaternion quaternion2;
                Vector3I addNormal = this.GetSmallBlockAddDirection(ref smallBlockWorldAabb, ref box, ref largeBlockWorldAabb);
                smallBlock.Orientation.GetQuaternion(out quaternion2);
                quaternion2 = Quaternion.CreateFromRotationMatrix(smallBlock.CubeGrid.WorldMatrix) * quaternion2;
                if (!MyCubeGrid.CheckConnectivitySmallBlockToLargeGrid(largeBlock.CubeGrid, smallBlock.BlockDefinition, ref quaternion2, ref addNormal))
                {
                    return false;
                }
            }
            BoundingBoxD xd2 = smallBlockWorldAabb;
            xd2.Inflate((double) ((2f * smallBlock.CubeGrid.GridSize) / 3f));
            BoundingBoxD xd3 = xd2.Intersect(largeBlockWorldAabb);
            Vector3D center = xd3.Center;
            HkShape shape = (HkShape) new HkBoxShape((Vector3) xd3.HalfExtents);
            largeBlock.Orientation.GetQuaternion(out quaternion);
            quaternion = Quaternion.CreateFromRotationMatrix(largeBlock.CubeGrid.WorldMatrix) * quaternion;
            largeBlock.ComputeWorldCenter(out vectord2);
            bool flag = false;
            try
            {
                if (largeBlock.FatBlock == null)
                {
                    HkShape shape3 = (HkShape) new HkBoxShape((largeBlock.BlockDefinition.Size * largeBlock.CubeGrid.GridSize) / 2f);
                    flag = MyPhysics.IsPenetratingShapeShape(shape, ref center, ref Quaternion.Identity, shape3, ref vectord2, ref quaternion);
                    shape3.RemoveReference();
                }
                else
                {
                    MyModel model = largeBlock.FatBlock.Model;
                    if ((model == null) || (model.HavokCollisionShapes == null))
                    {
                        HkShape shape2 = (HkShape) new HkBoxShape((largeBlock.BlockDefinition.Size * largeBlock.CubeGrid.GridSize) / 2f);
                        flag = MyPhysics.IsPenetratingShapeShape(shape, ref center, ref Quaternion.Identity, shape2, ref vectord2, ref quaternion);
                        shape2.RemoveReference();
                    }
                    else
                    {
                        HkShape[] havokCollisionShapes = model.HavokCollisionShapes;
                        for (int i = 0; i < havokCollisionShapes.Length; i++)
                        {
                            flag = MyPhysics.IsPenetratingShapeShape(shape, ref center, ref Quaternion.Identity, havokCollisionShapes[i], ref vectord2, ref quaternion);
                            if (flag)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                shape.RemoveReference();
            }
            return flag;
        }

        private bool SmallGridIsStatic(MyCubeGrid smallGrid) => 
            this.TestGridSmallToLargeConnection(smallGrid);

        public bool TestGridSmallToLargeConnection(MyCubeGrid smallGrid)
        {
            HashSet<MySlimBlockPair> set;
            return (smallGrid.IsStatic ? (Sync.IsServer ? (this.m_mapSmallGridToConnectedBlocks.TryGetValue(smallGrid, out set) && (set.Count > 0)) : true) : false);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Static = null;
        }

        public override bool IsRequiredByGame =>
            (base.IsRequiredByGame && MyFakes.ENABLE_SMALL_BLOCK_TO_LARGE_STATIC_CONNECTIONS);

        [StructLayout(LayoutKind.Sequential)]
        private struct MySlimBlockPair : IEquatable<MyCubeGridSmallToLargeConnection.MySlimBlockPair>
        {
            public MySlimBlock Parent;
            public MySlimBlock Child;
            public override int GetHashCode() => 
                (this.Parent.GetHashCode() ^ this.Child.GetHashCode());

            public override bool Equals(object obj)
            {
                if (!(obj is MyCubeGridSmallToLargeConnection.MySlimBlockPair))
                {
                    return false;
                }
                MyCubeGridSmallToLargeConnection.MySlimBlockPair pair = (MyCubeGridSmallToLargeConnection.MySlimBlockPair) obj;
                return (ReferenceEquals(this.Parent, pair.Parent) && ReferenceEquals(this.Child, pair.Child));
            }

            public bool Equals(MyCubeGridSmallToLargeConnection.MySlimBlockPair other) => 
                (ReferenceEquals(this.Parent, other.Parent) && ReferenceEquals(this.Child, other.Child));
        }
    }
}

