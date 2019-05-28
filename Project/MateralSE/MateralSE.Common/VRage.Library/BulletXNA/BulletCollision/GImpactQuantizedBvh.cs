namespace BulletXNA.BulletCollision
{
    using BulletXNA.LinearMath;
    using System;
    using System.Runtime.InteropServices;

    public class GImpactQuantizedBvh
    {
        private QuantizedBvhTree m_box_tree;
        private IPrimitiveManagerBase m_primitive_manager;
        private int m_size;

        public GImpactQuantizedBvh()
        {
            this.m_box_tree = new QuantizedBvhTree();
        }

        public GImpactQuantizedBvh(IPrimitiveManagerBase primitive_manager)
        {
            this.m_primitive_manager = primitive_manager;
            this.m_box_tree = new QuantizedBvhTree();
        }

        public bool BoxQuery(ref AABB box, ProcessHandler handler)
        {
            UShortVector3 vector;
            UShortVector3 vector2;
            int num = 0;
            int nodeCount = this.GetNodeCount();
            this.m_box_tree.QuantizePoint(out vector, ref box.m_min);
            this.m_box_tree.QuantizePoint(out vector2, ref box.m_max);
            while (num < nodeCount)
            {
                bool flag = this.m_box_tree.TestQuantizedBoxOverlap(num, ref vector, ref vector2);
                bool flag2 = this.IsLeafNode(num);
                if (flag2 & flag)
                {
                    foreach (int num4 in this.GetNodeData(num))
                    {
                        if (handler(num4))
                        {
                            return true;
                        }
                    }
                }
                num = !(flag | flag2) ? (num + this.GetEscapeNodeIndex(num)) : (num + 1);
            }
            return false;
        }

        public void BuildSet()
        {
            int primitiveCount = this.m_primitive_manager.GetPrimitiveCount();
            GIM_BVH_DATA_ARRAY gim_bvh_data_array = new GIM_BVH_DATA_ARRAY(primitiveCount);
            gim_bvh_data_array.Resize(primitiveCount);
            GIM_BVH_DATA[] rawArray = gim_bvh_data_array.GetRawArray();
            for (int i = 0; i < primitiveCount; i++)
            {
                this.m_primitive_manager.GetPrimitiveBox(i, out rawArray[i].m_bound);
                rawArray[i].m_data = i;
            }
            this.m_box_tree.BuildTree(gim_bvh_data_array);
        }

        private int GetEscapeNodeIndex(int nodeindex) => 
            this.m_box_tree.GetEscapeNodeIndex(nodeindex);

        private void GetNodeBound(int nodeindex, out AABB bound)
        {
            this.m_box_tree.GetNodeBound(nodeindex, out bound);
        }

        private int GetNodeCount() => 
            this.m_box_tree.GetNodeCount();

        private int[] GetNodeData(int nodeindex) => 
            this.m_box_tree.GetNodeData(nodeindex);

        private bool IsLeafNode(int nodeindex) => 
            this.m_box_tree.IsLeafNode(nodeindex);

        public void Load(byte[] byteArray)
        {
            this.m_box_tree.Load(byteArray);
            this.m_size = byteArray.Length;
        }

        public bool RayQuery(ref IndexedVector3 ray_dir, ref IndexedVector3 ray_origin, ProcessCollisionHandler handler)
        {
            int nodeindex = 0;
            int nodeCount = this.GetNodeCount();
            bool flag = false;
            while (nodeindex < nodeCount)
            {
                AABB aabb;
                this.GetNodeBound(nodeindex, out aabb);
                bool flag2 = aabb.CollideRay(ref ray_origin, ref ray_dir);
                bool flag3 = this.IsLeafNode(nodeindex);
                if (flag3 & flag2)
                {
                    foreach (int num4 in this.GetNodeData(nodeindex))
                    {
                        handler(num4);
                        flag = true;
                    }
                }
                nodeindex = !(flag2 | flag3) ? (nodeindex + this.GetEscapeNodeIndex(nodeindex)) : (nodeindex + 1);
            }
            return flag;
        }

        public bool RayQueryClosest(ref IndexedVector3 ray_dir, ref IndexedVector3 ray_origin, ProcessCollisionHandler handler)
        {
            int nodeindex = 0;
            int nodeCount = this.GetNodeCount();
            float positiveInfinity = float.PositiveInfinity;
            while (nodeindex < nodeCount)
            {
                AABB aabb;
                this.GetNodeBound(nodeindex, out aabb);
                float? nullable = aabb.CollideRayDistance(ref ray_origin, ref ray_dir);
                bool flag = this.IsLeafNode(nodeindex);
                bool flag2 = (nullable != null) && (nullable.Value < positiveInfinity);
                if (flag2 & flag)
                {
                    foreach (int num5 in this.GetNodeData(nodeindex))
                    {
                        float? nullable2 = handler(num5);
                        if ((nullable2 != null) && (nullable2.Value < positiveInfinity))
                        {
                            positiveInfinity = nullable2.Value;
                        }
                    }
                }
                nodeindex = !(flag2 | flag) ? (nodeindex + this.GetEscapeNodeIndex(nodeindex)) : (nodeindex + 1);
            }
            return !(positiveInfinity == float.PositiveInfinity);
        }

        public byte[] Save() => 
            this.m_box_tree.Save();

        public int Size =>
            this.m_size;
    }
}

