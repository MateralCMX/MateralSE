namespace Sandbox.Engine.Voxels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Game.Voxels;
    using VRage.ModAPI;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    internal class MySparseOctree
    {
        private readonly Dictionary<uint, MyOctreeNode> m_nodes = new Dictionary<uint, MyOctreeNode>();
        private MyOctreeNode.FilterFunction m_nodeFilter;
        private int m_treeHeight;
        private int m_treeWidth;
        private byte m_defaultContent;

        public MySparseOctree(int height, MyOctreeNode.FilterFunction nodeFilter, byte defaultContent = 0)
        {
            this.m_treeHeight = height;
            this.m_treeWidth = 1 << (height & 0x1f);
            this.m_defaultContent = defaultContent;
            this.m_nodeFilter = nodeFilter;
        }

        public unsafe void Build(byte singleValue)
        {
            MyOctreeNode node;
            this.m_nodes.Clear();
            node.ChildMask = 0;
            for (int i = 0; i < 8; i++)
            {
                &node.Data.FixedElementField[i] = singleValue;
            }
            this.m_nodes[this.ComputeRootKey()] = node;
        }

        public void Build<TDataEnum>(TDataEnum data) where TDataEnum: struct, IEnumerator<byte>
        {
            MyOctreeNode node;
            StackData<TDataEnum> data2;
            this.m_nodes.Clear();
            data2.Data = data;
            data2.Cell = new MyCellCoord(this.m_treeHeight - 1, ref Vector3I.Zero);
            data2.DefaultNode = new MyOctreeNode(this.m_defaultContent);
            this.BuildNode<TDataEnum>(ref data2, out node);
            this.m_nodes[this.ComputeRootKey()] = node;
        }

        private unsafe void BuildNode<TDataEnum>(ref StackData<TDataEnum> stack, out MyOctreeNode builtNode) where TDataEnum: struct, IEnumerator<byte>
        {
            MyOctreeNode defaultNode = stack.DefaultNode;
            if (stack.Cell.Lod == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    stack.Data.MoveNext();
                    &defaultNode.Data.FixedElementField[i] = stack.Data.Current;
                }
            }
            else
            {
                int* numPtr1 = (int*) ref stack.Cell.Lod;
                numPtr1[0]--;
                Vector3I coordInLod = stack.Cell.CoordInLod;
                Vector3I vectori2 = coordInLod << 1;
                int childIdx = 0;
                while (true)
                {
                    Vector3I vectori3;
                    MyOctreeNode node2;
                    if (childIdx >= 8)
                    {
                        int* numPtr2 = (int*) ref stack.Cell.Lod;
                        numPtr2[0]++;
                        stack.Cell.CoordInLod = coordInLod;
                        break;
                    }
                    this.ComputeChildCoord(childIdx, out vectori3);
                    stack.Cell.CoordInLod = (Vector3I) (vectori2 + vectori3);
                    this.BuildNode<TDataEnum>(ref stack, out node2);
                    if (!node2.HasChildren && MyOctreeNode.AllDataSame(&node2.Data.FixedElementField))
                    {
                        defaultNode.SetChild(childIdx, false);
                        &defaultNode.Data.FixedElementField[childIdx] = node2.Data.FixedElementField;
                    }
                    else
                    {
                        defaultNode.SetChild(childIdx, true);
                        &defaultNode.Data.FixedElementField[childIdx] = this.m_nodeFilter(&node2.Data.FixedElementField, stack.Cell.Lod);
                        this.m_nodes.Add(stack.Cell.PackId32(), node2);
                    }
                    childIdx++;
                }
            }
            builtNode = defaultNode;
        }

        [Conditional("DEBUG")]
        private void CheckData<T>(T data) where T: struct, IEnumerator<byte>
        {
        }

        [Conditional("DEBUG")]
        private void CheckData<T>(ref T data, MyCellCoord cell) where T: struct, IEnumerator<byte>
        {
            uint num = cell.PackId32();
            MyOctreeNode node = this.m_nodes[num];
            for (int i = 0; i < 8; i++)
            {
                if (node.HasChild(i))
                {
                    Vector3I vectori;
                    this.ComputeChildCoord(i, out vectori);
                }
                else
                {
                    int num3 = 1 << ((3 * cell.Lod) & 0x1f);
                    for (int j = 0; j < num3; j++)
                    {
                    }
                }
            }
        }

        private void ComputeChildCoord(int childIdx, out Vector3I relativeCoord)
        {
            relativeCoord.X = childIdx & 1;
            relativeCoord.Y = (childIdx >> 1) & 1;
            relativeCoord.Z = (childIdx >> 2) & 1;
        }

        private uint ComputeRootKey() => 
            new MyCellCoord(this.m_treeHeight - 1, ref Vector3I.Zero).PackId32();

        internal unsafe void DebugDraw(IMyDebugDrawBatchAabb batch, Vector3 worldPos, MyVoxelDebugDrawMode mode)
        {
            Color? nullable;
            if (mode == MyVoxelDebugDrawMode.Content_MicroNodes)
            {
                foreach (KeyValuePair<uint, MyOctreeNode> pair in this.m_nodes)
                {
                    MyCellCoord coord = new MyCellCoord();
                    coord.SetUnpack(pair.Key);
                    MyOctreeNode node = pair.Value;
                    for (int i = 0; i < 8; i++)
                    {
                        if (!node.HasChild(i) || (coord.Lod == 0))
                        {
                            Vector3I vectori;
                            BoundingBoxD xd;
                            this.ComputeChildCoord(i, out vectori);
                            Vector3I vectori2 = (Vector3I) ((coord.CoordInLod << (coord.Lod + 1)) + (vectori << coord.Lod));
                            xd.Min = worldPos + (vectori2 * 1f);
                            BoundingBoxD* xdPtr1 = (BoundingBoxD*) ref xd;
                            xdPtr1->Max = xd.Min + (1f * (1 << (coord.Lod & 0x1f)));
                            if (node.GetData(i) != 0)
                            {
                                nullable = null;
                                batch.Add(ref xd, nullable);
                            }
                        }
                    }
                }
                return;
            }
            else if (mode != MyVoxelDebugDrawMode.Content_MicroNodesScaled)
            {
                return;
            }
            foreach (KeyValuePair<uint, MyOctreeNode> pair2 in this.m_nodes)
            {
                MyCellCoord coord2 = new MyCellCoord();
                coord2.SetUnpack(pair2.Key);
                MyOctreeNode node2 = pair2.Value;
                for (int i = 0; i < 8; i++)
                {
                    if (!node2.HasChild(i))
                    {
                        Vector3I vectori3;
                        this.ComputeChildCoord(i, out vectori3);
                        float num3 = ((float) node2.GetData(i)) / 255f;
                        if (num3 != 0f)
                        {
                            BoundingBoxD xd2;
                            num3 = (float) Math.Pow(num3 * 1.0, 0.3333);
                            Vector3I vectori4 = (Vector3I) ((coord2.CoordInLod << (coord2.Lod + 1)) + (vectori3 << coord2.Lod));
                            float num4 = 1f * (1 << (coord2.Lod & 0x1f));
                            Vector3 vector = (worldPos + (vectori4 * 1f)) + (0.5f * num4);
                            xd2.Min = vector - ((0.5f * num3) * num4);
                            xd2.Max = vector + ((0.5f * num3) * num4);
                            nullable = null;
                            batch.Add(ref xd2, nullable);
                        }
                    }
                }
            }
        }

        internal static int EstimateStackSize(int treeHeight) => 
            (((treeHeight - 1) * 7) + 8);

        internal void ExecuteOperation<TOperator>(ref TOperator source, MyStorageDataTypeEnum type, ref Vector3I readOffset, ref Vector3I min, ref Vector3I max) where TOperator: struct, IVoxelOperator
        {
            if (source.Flags == VoxelOperatorFlags.Read)
            {
                this.ReadRange<TOperator>(ref source, type, ref readOffset, 0, ref min, ref max);
            }
            else
            {
                this.WriteRange<TOperator>(new MyCellCoord(this.m_treeHeight - 1, Vector3I.Zero), this.m_defaultContent, ref source, type, ref readOffset, ref min, ref max);
            }
        }

        internal unsafe byte GetFilteredValue()
        {
            MyOctreeNode node = this.m_nodes[this.ComputeRootKey()];
            return this.m_nodeFilter(&node.Data.FixedElementField, this.m_treeHeight);
        }

        internal unsafe ContainmentType Intersect(ref BoundingBoxI box, int lod, bool exhaustiveContainmentCheck = true)
        {
            MyCellCoord* coordPtr = (MyCellCoord*) stackalloc byte[(((IntPtr) EstimateStackSize(this.m_treeHeight)) * sizeof(MyCellCoord))];
            MyCellCoord coord = new MyCellCoord(this.m_treeHeight - 1, ref Vector3I.Zero);
            int index = 0 + 1;
            coordPtr[index] = coord;
            Vector3I min = box.Min;
            Vector3I max = box.Max;
            ContainmentType disjoint = ContainmentType.Disjoint;
            while (index > 0)
            {
                coord = coordPtr[--index];
                MyOctreeNode node = this.m_nodes[coord.PackId32()];
                int num3 = coord.Lod;
                Vector3I vectori6 = coord.CoordInLod << 1;
                Vector3I vectori4 = (min >> num3) - vectori6;
                Vector3I vectori5 = (max >> num3) - vectori6;
                for (int i = 0; i < 8; i++)
                {
                    Vector3I vectori3;
                    this.ComputeChildCoord(i, out vectori3);
                    if (vectori3.IsInsideInclusiveEnd(ref vectori4, ref vectori5))
                    {
                        if ((coord.Lod > 0) && node.HasChild(i))
                        {
                            index++;
                            coordPtr[index] = new MyCellCoord(coord.Lod - 1, (Vector3I) (vectori6 + vectori3));
                        }
                        else
                        {
                            byte num5 = &node.Data.FixedElementField[i];
                            if (num3 == 0)
                            {
                                if (num5 != 0)
                                {
                                    return ContainmentType.Intersects;
                                }
                            }
                            else
                            {
                                BoundingBoxI xi;
                                bool flag;
                                xi.Min = (Vector3I) (vectori6 + vectori3);
                                Vector3I* vectoriPtr1 = (Vector3I*) ref xi.Min;
                                vectoriPtr1[0] = vectoriPtr1[0] << num3;
                                BoundingBoxI* xiPtr1 = (BoundingBoxI*) ref xi;
                                xiPtr1->Max = (Vector3I) ((xi.Min + (1 << (num3 & 0x1f))) - 1);
                                Vector3I.Max(ref xi.Min, ref min, out xi.Min);
                                Vector3I.Min(ref xi.Max, ref max, out xi.Max);
                                ((BoundingBoxI*) ref xi).Intersects(ref xi, out flag);
                                if (flag)
                                {
                                    return ContainmentType.Intersects;
                                }
                            }
                        }
                    }
                }
            }
            return disjoint;
        }

        internal bool Intersect(ref LineD line, out double startOffset, out double endOffset)
        {
            startOffset = 0.0;
            endOffset = 1.0;
            return true;
        }

        internal unsafe void ReadFrom(MyOctreeStorage.ChunkHeader header, Stream stream)
        {
            this.m_treeHeight = stream.ReadInt32();
            this.m_treeWidth = 1 << (this.m_treeHeight & 0x1f);
            this.m_defaultContent = stream.ReadByteNoAlloc();
            int* numPtr1 = (int*) ref header.Size;
            numPtr1[0] -= 5;
            int num = header.Size / 13;
            this.m_nodes.Clear();
            for (int i = 0; i < num; i++)
            {
                MyOctreeNode node;
                uint key = stream.ReadUInt32();
                node.ChildMask = stream.ReadByteNoAlloc();
                stream.ReadNoAlloc(&node.Data.FixedElementField, 0, 8);
                this.m_nodes.Add(key, node);
            }
        }

        internal void ReadRange(MyStorageData target, MyStorageDataTypeEnum type, ref Vector3I writeOffset, int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod)
        {
            MyStorageReadOperator @operator = new MyStorageReadOperator(target);
            this.ReadRange<MyStorageReadOperator>(ref @operator, type, ref writeOffset, lodIndex, ref minInLod, ref maxInLod);
        }

        internal unsafe void ReadRange<TOperator>(ref TOperator target, MyStorageDataTypeEnum type, ref Vector3I writeOffset, int lodIndex, ref Vector3I minInLod, ref Vector3I maxInLod) where TOperator: struct, IVoxelOperator
        {
            try
            {
                MyCellCoord* coordPtr = (MyCellCoord*) stackalloc byte[(((IntPtr) EstimateStackSize(this.m_treeHeight)) * sizeof(MyCellCoord))];
                MyCellCoord coord = new MyCellCoord(this.m_treeHeight - 1, ref Vector3I.Zero);
                int index = 0 + 1;
                coordPtr[index] = coord;
                while (index > 0)
                {
                    coord = coordPtr[--index];
                    MyOctreeNode node = this.m_nodes[coord.PackId32()];
                    int num3 = coord.Lod - lodIndex;
                    Vector3I vectori4 = coord.CoordInLod << 1;
                    Vector3I min = (minInLod >> num3) - vectori4;
                    Vector3I max = (maxInLod >> num3) - vectori4;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3I vectori;
                        this.ComputeChildCoord(i, out vectori);
                        if (vectori.IsInsideInclusiveEnd(ref min, ref max))
                        {
                            if ((lodIndex < coord.Lod) && node.HasChild(i))
                            {
                                index++;
                                coordPtr[index] = new MyCellCoord(coord.Lod - 1, (Vector3I) (vectori4 + vectori));
                            }
                            else
                            {
                                byte inOutContent = &node.Data.FixedElementField[i];
                                Vector3I result = (Vector3I) (vectori4 + vectori);
                                if (num3 == 0)
                                {
                                    Vector3I position = (Vector3I) ((writeOffset + result) - minInLod);
                                    target.Op(ref position, type, ref inOutContent);
                                }
                                else
                                {
                                    result = result << num3;
                                    Vector3I vectori7 = (Vector3I) ((result + (1 << (num3 & 0x1f))) - 1);
                                    Vector3I* vectoriPtr1 = (Vector3I*) ref result;
                                    Vector3I.Max(ref (Vector3I) ref vectoriPtr1, ref minInLod, out result);
                                    Vector3I* vectoriPtr2 = (Vector3I*) ref vectori7;
                                    Vector3I.Min(ref (Vector3I) ref vectoriPtr2, ref maxInLod, out vectori7);
                                    int z = result.Z;
                                    while (z <= vectori7.Z)
                                    {
                                        int y = result.Y;
                                        while (true)
                                        {
                                            if (y > vectori7.Y)
                                            {
                                                z++;
                                                break;
                                            }
                                            int x = result.X;
                                            while (true)
                                            {
                                                if (x > vectori7.X)
                                                {
                                                    y++;
                                                    break;
                                                }
                                                Vector3I position = writeOffset;
                                                int* numPtr1 = (int*) ref position.X;
                                                numPtr1[0] += x - minInLod.X;
                                                int* numPtr2 = (int*) ref position.Y;
                                                numPtr2[0] += y - minInLod.Y;
                                                int* numPtr3 = (int*) ref position.Z;
                                                numPtr3[0] += z - minInLod.Z;
                                                target.Op(ref position, type, ref inOutContent);
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
            finally
            {
            }
        }

        public void ReplaceValues(Dictionary<byte, byte> oldToNewValueMap)
        {
            ReplaceValues<uint>(this.m_nodes, oldToNewValueMap);
        }

        public static unsafe void ReplaceValues<TKey>(Dictionary<TKey, MyOctreeNode> nodeCollection, Dictionary<byte, byte> oldToNewValueMap)
        {
            KeyValuePair<TKey, MyOctreeNode>[] pairArray = nodeCollection.ToArray<KeyValuePair<TKey, MyOctreeNode>>();
            int index = 0;
            while (index < pairArray.Length)
            {
                KeyValuePair<TKey, MyOctreeNode> pair = pairArray[index];
                MyOctreeNode node = pair.Value;
                int num2 = 0;
                while (true)
                {
                    byte num3;
                    if (num2 >= 8)
                    {
                        nodeCollection[pair.Key] = node;
                        index++;
                        break;
                    }
                    if (oldToNewValueMap.TryGetValue(&node.Data.FixedElementField[num2], out num3))
                    {
                        &node.Data.FixedElementField[num2] = num3;
                    }
                    num2++;
                }
            }
        }

        private unsafe void WriteRange<TOperator>(MyCellCoord cell, byte defaultData, ref TOperator source, MyStorageDataTypeEnum type, ref Vector3I readOffset, ref Vector3I min, ref Vector3I max) where TOperator: struct, IVoxelOperator
        {
            MyOctreeNode node;
            uint key = cell.PackId32();
            if (!this.m_nodes.TryGetValue(key, out node))
            {
                for (int i = 0; i < 8; i++)
                {
                    &node.Data.FixedElementField[i] = defaultData;
                }
            }
            if (cell.Lod == 0)
            {
                Vector3I vectori = cell.CoordInLod << 1;
                for (int i = 0; i < 8; i++)
                {
                    Vector3I vectori2;
                    this.ComputeChildCoord(i, out vectori2);
                    Vector3I position = (Vector3I) (vectori + vectori2);
                    if (position.IsInsideInclusiveEnd(ref min, ref max))
                    {
                        position = (Vector3I) ((position - min) + readOffset);
                        source.Op(ref position, type, ref (byte) ref (&node.Data.FixedElementField + i));
                    }
                }
                this.m_nodes[key] = node;
            }
            else
            {
                Vector3I vectori4 = cell.CoordInLod << 1;
                Vector3I vectori6 = (min >> cell.Lod) - vectori4;
                Vector3I vectori7 = (max >> cell.Lod) - vectori4;
                for (int i = 0; i < 8; i++)
                {
                    Vector3I vectori5;
                    this.ComputeChildCoord(i, out vectori5);
                    if (vectori5.IsInsideInclusiveEnd(ref vectori6, ref vectori7))
                    {
                        MyCellCoord coord = new MyCellCoord(cell.Lod - 1, (Vector3I) (vectori4 + vectori5));
                        this.WriteRange<TOperator>(coord, &node.Data.FixedElementField[i], ref source, type, ref readOffset, ref min, ref max);
                        uint num5 = coord.PackId32();
                        MyOctreeNode node2 = this.m_nodes[num5];
                        if (node2.HasChildren || !MyOctreeNode.AllDataSame(&node2.Data.FixedElementField))
                        {
                            node.SetChild(i, true);
                            &node.Data.FixedElementField[i] = this.m_nodeFilter(&node2.Data.FixedElementField, cell.Lod);
                        }
                        else
                        {
                            node.SetChild(i, false);
                            &node.Data.FixedElementField[i] = node2.Data.FixedElementField;
                            this.m_nodes.Remove(num5);
                        }
                    }
                }
                this.m_nodes[key] = node;
            }
        }

        internal unsafe void WriteTo(Stream stream)
        {
            stream.WriteNoAlloc(this.m_treeHeight);
            stream.WriteNoAlloc(this.m_defaultContent);
            foreach (KeyValuePair<uint, MyOctreeNode> pair in this.m_nodes)
            {
                stream.WriteNoAlloc(pair.Key);
                MyOctreeNode node = pair.Value;
                stream.WriteNoAlloc(node.ChildMask);
                stream.WriteNoAlloc(&node.Data.FixedElementField, 0, 8);
            }
        }

        public int TreeWidth =>
            this.m_treeWidth;

        public bool IsAllSame
        {
            get
            {
                MyOctreeNode node = this.m_nodes[this.ComputeRootKey()];
                return (!node.HasChildren && MyOctreeNode.AllDataSame(&node.Data.FixedElementField));
            }
        }

        public int SerializedSize =>
            (5 + (this.m_nodes.Count * 13));

        [StructLayout(LayoutKind.Sequential)]
        private struct StackData<T> where T: struct, IEnumerator<byte>
        {
            public T Data;
            public MyCellCoord Cell;
            public MyOctreeNode DefaultNode;
        }
    }
}

