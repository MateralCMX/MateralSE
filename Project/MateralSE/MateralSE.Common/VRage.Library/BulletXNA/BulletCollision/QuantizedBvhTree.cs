namespace BulletXNA.BulletCollision
{
    using BulletXNA;
    using BulletXNA.LinearMath;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    public class QuantizedBvhTree
    {
        private const int MAX_INDICES_PER_NODE = 6;
        private int m_num_nodes = 0;
        private GIM_QUANTIZED_BVH_NODE_ARRAY m_node_array = new GIM_QUANTIZED_BVH_NODE_ARRAY();
        private AABB m_global_bound;
        private IndexedVector3 m_bvhQuantization;

        internal QuantizedBvhTree()
        {
        }

        private void BuildSubTree(GIM_BVH_DATA_ARRAY primitive_boxes, int startIndex, int endIndex)
        {
            int nodeindex = this.m_num_nodes;
            this.m_num_nodes++;
            if ((endIndex - startIndex) <= 6)
            {
                int num3 = endIndex - startIndex;
                int[] indices = new int[num3];
                AABB bound = new AABB();
                bound.Invalidate();
                for (int i = 0; i < num3; i++)
                {
                    indices[i] = primitive_boxes[startIndex + i].m_data;
                    bound.Merge(primitive_boxes.GetRawArray()[startIndex + i].m_bound);
                }
                this.SetNodeBound(nodeindex, ref bound);
                this.m_node_array[nodeindex].SetDataIndices(indices);
            }
            else
            {
                int splitAxis = this.CalcSplittingAxis(primitive_boxes, startIndex, endIndex);
                splitAxis = this.SortAndCalcSplittingIndex(primitive_boxes, startIndex, endIndex, splitAxis);
                AABB bound = new AABB();
                bound.Invalidate();
                for (int i = startIndex; i < endIndex; i++)
                {
                    bound.Merge(ref primitive_boxes.GetRawArray()[i].m_bound);
                }
                this.SetNodeBound(nodeindex, ref bound);
                this.BuildSubTree(primitive_boxes, startIndex, splitAxis);
                this.BuildSubTree(primitive_boxes, splitAxis, endIndex);
                this.m_node_array.GetRawArray()[nodeindex].SetEscapeIndex(this.m_num_nodes - nodeindex);
            }
        }

        internal void BuildTree(GIM_BVH_DATA_ARRAY primitive_boxes)
        {
            this.CalcQuantization(primitive_boxes);
            this.m_num_nodes = 0;
            this.m_node_array.Resize(primitive_boxes.Count * 2);
            this.BuildSubTree(primitive_boxes, 0, primitive_boxes.Count);
        }

        private void CalcQuantization(GIM_BVH_DATA_ARRAY primitive_boxes)
        {
            this.CalcQuantization(primitive_boxes, 1f);
        }

        private void CalcQuantization(GIM_BVH_DATA_ARRAY primitive_boxes, float boundMargin)
        {
            AABB aabb = new AABB();
            aabb.Invalidate();
            int count = primitive_boxes.Count;
            for (int i = 0; i < count; i++)
            {
                aabb.Merge(ref primitive_boxes.GetRawArray()[i].m_bound);
            }
            GImpactQuantization.CalcQuantizationParameters(out this.m_global_bound.m_min, out this.m_global_bound.m_max, out this.m_bvhQuantization, ref aabb.m_min, ref aabb.m_max, boundMargin);
        }

        private int CalcSplittingAxis(GIM_BVH_DATA_ARRAY primitive_boxes, int startIndex, int endIndex)
        {
            int num;
            IndexedVector3 zero = IndexedVector3.Zero;
            IndexedVector3 vector2 = IndexedVector3.Zero;
            int num2 = endIndex - startIndex;
            for (num = startIndex; num < endIndex; num++)
            {
                IndexedVector3 vector3 = (IndexedVector3) (0.5f * (primitive_boxes[num].m_bound.m_max + primitive_boxes[num].m_bound.m_min));
                zero += vector3;
            }
            zero *= 1f / ((float) num2);
            for (num = startIndex; num < endIndex; num++)
            {
                IndexedVector3 vector4 = (IndexedVector3) (0.5f * (primitive_boxes[num].m_bound.m_max + primitive_boxes[num].m_bound.m_min));
                IndexedVector3 vector5 = vector4 - zero;
                vector2 += vector5 * vector5;
            }
            return MathUtil.MaxAxis(ref vector2 * (1f / (num2 - 1f)));
        }

        internal int GetEscapeNodeIndex(int nodeindex) => 
            this.m_node_array[nodeindex].GetEscapeIndex();

        internal void GetNodeBound(int nodeindex, out AABB bound)
        {
            bound.m_min = GImpactQuantization.Unquantize(ref this.m_node_array.GetRawArray()[nodeindex].m_quantizedAabbMin, ref this.m_global_bound.m_min, ref this.m_bvhQuantization);
            bound.m_max = GImpactQuantization.Unquantize(ref this.m_node_array.GetRawArray()[nodeindex].m_quantizedAabbMax, ref this.m_global_bound.m_min, ref this.m_bvhQuantization);
        }

        internal int GetNodeCount() => 
            this.m_num_nodes;

        internal int[] GetNodeData(int nodeindex) => 
            this.m_node_array[nodeindex].GetDataIndices();

        internal bool IsLeafNode(int nodeindex) => 
            this.m_node_array[nodeindex].IsLeafNode();

        internal void Load(byte[] byteArray)
        {
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    this.m_num_nodes = reader.ReadInt32();
                    IndexedVector3 min = ReadIndexedVector3(reader);
                    IndexedVector3 max = ReadIndexedVector3(reader);
                    this.m_global_bound = new AABB(ref min, ref max);
                    this.m_bvhQuantization = ReadIndexedVector3(reader);
                    this.m_node_array = new GIM_QUANTIZED_BVH_NODE_ARRAY(this.m_num_nodes);
                    int num = 0;
                    while (num < this.m_num_nodes)
                    {
                        int num2 = reader.ReadInt32();
                        BT_QUANTIZED_BVH_NODE item = new BT_QUANTIZED_BVH_NODE {
                            m_escapeIndexOrDataIndex = new int[num2]
                        };
                        int index = 0;
                        while (true)
                        {
                            if (index >= num2)
                            {
                                item.m_quantizedAabbMin = ReadUShortVector3(reader);
                                item.m_quantizedAabbMax = ReadUShortVector3(reader);
                                this.m_node_array.Add(item);
                                num++;
                                break;
                            }
                            item.m_escapeIndexOrDataIndex[index] = reader.ReadInt32();
                            index++;
                        }
                    }
                }
            }
        }

        internal void QuantizePoint(out UShortVector3 quantizedpoint, ref IndexedVector3 point)
        {
            GImpactQuantization.QuantizeClamp(out quantizedpoint, ref point, ref this.m_global_bound.m_min, ref this.m_global_bound.m_max, ref this.m_bvhQuantization);
        }

        private static IndexedVector3 ReadIndexedVector3(BinaryReader br) => 
            new IndexedVector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

        private static UShortVector3 ReadUShortVector3(BinaryReader br) => 
            new UShortVector3 { 
                X = br.ReadUInt16(),
                Y = br.ReadUInt16(),
                Z = br.ReadUInt16()
            };

        internal byte[] Save()
        {
            byte[] buffer;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(this.m_num_nodes);
                    WriteIndexedVector3(this.m_global_bound.m_min, writer);
                    WriteIndexedVector3(this.m_global_bound.m_max, writer);
                    WriteIndexedVector3(this.m_bvhQuantization, writer);
                    int num = 0;
                    while (true)
                    {
                        if (num >= this.m_num_nodes)
                        {
                            buffer = stream.ToArray();
                            break;
                        }
                        writer.Write(this.m_node_array[num].m_escapeIndexOrDataIndex.Length);
                        int index = 0;
                        while (true)
                        {
                            if (index >= this.m_node_array[num].m_escapeIndexOrDataIndex.Length)
                            {
                                WriteUShortVector3(this.m_node_array[num].m_quantizedAabbMin, writer);
                                WriteUShortVector3(this.m_node_array[num].m_quantizedAabbMax, writer);
                                num++;
                                break;
                            }
                            writer.Write(this.m_node_array[num].m_escapeIndexOrDataIndex[index]);
                            index++;
                        }
                    }
                }
            }
            return buffer;
        }

        private void SetNodeBound(int nodeindex, ref AABB bound)
        {
            GImpactQuantization.QuantizeClamp(out this.m_node_array.GetRawArray()[nodeindex].m_quantizedAabbMin, ref bound.m_min, ref this.m_global_bound.m_min, ref this.m_global_bound.m_max, ref this.m_bvhQuantization);
            GImpactQuantization.QuantizeClamp(out this.m_node_array.GetRawArray()[nodeindex].m_quantizedAabbMax, ref bound.m_max, ref this.m_global_bound.m_min, ref this.m_global_bound.m_max, ref this.m_bvhQuantization);
        }

        private int SortAndCalcSplittingIndex(GIM_BVH_DATA_ARRAY primitive_boxes, int startIndex, int endIndex, int splitAxis)
        {
            int num;
            int num2 = startIndex;
            int num3 = endIndex - startIndex;
            float num4 = 0f;
            IndexedVector3 zero = IndexedVector3.Zero;
            for (num = startIndex; num < endIndex; num++)
            {
                IndexedVector3 vector2 = (IndexedVector3) (0.5f * (primitive_boxes[num].m_bound.m_max + primitive_boxes[num].m_bound.m_min));
                zero += vector2;
            }
            num4 = (zero * (1f / ((float) num3)))[splitAxis];
            for (num = startIndex; num < endIndex; num++)
            {
                IndexedVector3 vector3 = (IndexedVector3) (0.5f * (primitive_boxes[num].m_bound.m_max + primitive_boxes[num].m_bound.m_min));
                if (vector3[splitAxis] > num4)
                {
                    primitive_boxes.Swap(num, num2);
                    num2++;
                }
            }
            int num5 = num3 / 3;
            if ((num2 <= (startIndex + num5)) || (num2 >= ((endIndex - 1) - num5)))
            {
                num2 = startIndex + (num3 >> 1);
            }
            return num2;
        }

        internal bool TestQuantizedBoxOverlap(int node_index, ref UShortVector3 quantizedMin, ref UShortVector3 quantizedMax) => 
            this.m_node_array[node_index].TestQuantizedBoxOverlapp(ref quantizedMin, ref quantizedMax);

        private static void WriteIndexedVector3(IndexedVector3 vector, BinaryWriter bw)
        {
            bw.Write(vector.X);
            bw.Write(vector.Y);
            bw.Write(vector.Z);
        }

        private static void WriteUShortVector3(UShortVector3 vector, BinaryWriter bw)
        {
            bw.Write(vector.X);
            bw.Write(vector.Y);
            bw.Write(vector.Z);
        }
    }
}

