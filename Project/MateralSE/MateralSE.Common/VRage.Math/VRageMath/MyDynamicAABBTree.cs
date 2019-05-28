namespace VRageMath
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;

    public class MyDynamicAABBTree
    {
        public const int NullNode = -1;
        private int m_freeList;
        private int m_nodeCapacity;
        private int m_nodeCount;
        private DynamicTreeNode[] m_nodes;
        private Dictionary<int, DynamicTreeNode> m_leafElementCache;
        private int m_root;
        [ThreadStatic]
        private static Stack<int> m_queryStack;
        private static List<Stack<int>> m_StackCacheCollection = new List<Stack<int>>();
        private Vector3 m_extension;
        private float m_aabbMultiplier;
        private FastResourceLock m_rwLock;

        public MyDynamicAABBTree()
        {
            this.m_rwLock = new FastResourceLock();
            this.Init(Vector3.One, 1f);
        }

        public MyDynamicAABBTree(Vector3 extension, float aabbMultiplier = 1f)
        {
            this.m_rwLock = new FastResourceLock();
            this.Init(extension, aabbMultiplier);
        }

        public unsafe int AddProxy(ref BoundingBox aabb, object userData, uint userFlags, bool rebalance = true)
        {
            using (this.m_rwLock.AcquireExclusiveUsing())
            {
                int index = this.AllocateNode();
                this.m_nodes[index].Aabb = aabb;
                Vector3* vectorPtr1 = (Vector3*) ref this.m_nodes[index].Aabb.Min;
                vectorPtr1[0] -= this.m_extension;
                Vector3* vectorPtr2 = (Vector3*) ref this.m_nodes[index].Aabb.Max;
                vectorPtr2[0] += this.m_extension;
                this.m_nodes[index].UserData = userData;
                this.m_nodes[index].UserFlag = userFlags;
                this.m_nodes[index].Height = 0;
                this.m_leafElementCache[index] = this.m_nodes[index];
                this.InsertLeaf(index, rebalance);
                return index;
            }
        }

        private int AllocateNode()
        {
            if (this.m_freeList == -1)
            {
                this.m_nodeCapacity *= 2;
                this.m_nodes = new DynamicTreeNode[this.m_nodeCapacity];
                Array.Copy(this.m_nodes, this.m_nodes, this.m_nodeCount);
                int nodeCount = this.m_nodeCount;
                while (true)
                {
                    if (nodeCount >= (this.m_nodeCapacity - 1))
                    {
                        DynamicTreeNode node2 = new DynamicTreeNode();
                        node2.ParentOrNext = -1;
                        node2.Height = 1;
                        this.m_nodes[this.m_nodeCapacity - 1] = node2;
                        this.m_freeList = this.m_nodeCount;
                        break;
                    }
                    DynamicTreeNode node1 = new DynamicTreeNode();
                    node1.ParentOrNext = nodeCount + 1;
                    node1.Height = 1;
                    this.m_nodes[nodeCount] = node1;
                    nodeCount++;
                }
            }
            int freeList = this.m_freeList;
            this.m_freeList = this.m_nodes[freeList].ParentOrNext;
            this.m_nodes[freeList].ParentOrNext = -1;
            this.m_nodes[freeList].Child1 = -1;
            this.m_nodes[freeList].Child2 = -1;
            this.m_nodes[freeList].Height = 0;
            this.m_nodes[freeList].UserData = null;
            this.m_nodeCount++;
            return freeList;
        }

        public int Balance(int iA)
        {
            DynamicTreeNode node = this.m_nodes[iA];
            if (node.IsLeaf() || (node.Height < 2))
            {
                return iA;
            }
            int index = node.Child1;
            int num2 = node.Child2;
            DynamicTreeNode node2 = this.m_nodes[index];
            DynamicTreeNode node3 = this.m_nodes[num2];
            int num3 = node3.Height - node2.Height;
            if (num3 > 1)
            {
                int num4 = node3.Child1;
                int num5 = node3.Child2;
                DynamicTreeNode node4 = this.m_nodes[num4];
                DynamicTreeNode node5 = this.m_nodes[num5];
                node3.Child1 = iA;
                node3.ParentOrNext = node.ParentOrNext;
                node.ParentOrNext = num2;
                if (node3.ParentOrNext == -1)
                {
                    this.m_root = num2;
                }
                else if (this.m_nodes[node3.ParentOrNext].Child1 == iA)
                {
                    this.m_nodes[node3.ParentOrNext].Child1 = num2;
                }
                else
                {
                    this.m_nodes[node3.ParentOrNext].Child2 = num2;
                }
                if (node4.Height > node5.Height)
                {
                    node3.Child2 = num4;
                    node.Child2 = num5;
                    node5.ParentOrNext = iA;
                    BoundingBox.CreateMerged(ref node2.Aabb, ref node5.Aabb, out node.Aabb);
                    BoundingBox.CreateMerged(ref node.Aabb, ref node4.Aabb, out node3.Aabb);
                    node.Height = 1 + Math.Max(node2.Height, node5.Height);
                    node3.Height = 1 + Math.Max(node.Height, node4.Height);
                }
                else
                {
                    node3.Child2 = num5;
                    node.Child2 = num4;
                    node4.ParentOrNext = iA;
                    BoundingBox.CreateMerged(ref node2.Aabb, ref node4.Aabb, out node.Aabb);
                    BoundingBox.CreateMerged(ref node.Aabb, ref node5.Aabb, out node3.Aabb);
                    node.Height = 1 + Math.Max(node2.Height, node4.Height);
                    node3.Height = 1 + Math.Max(node.Height, node5.Height);
                }
                return num2;
            }
            if (num3 >= -1)
            {
                return iA;
            }
            int num6 = node2.Child1;
            int num7 = node2.Child2;
            DynamicTreeNode node6 = this.m_nodes[num6];
            DynamicTreeNode node7 = this.m_nodes[num7];
            node2.Child1 = iA;
            node2.ParentOrNext = node.ParentOrNext;
            node.ParentOrNext = index;
            if (node2.ParentOrNext == -1)
            {
                this.m_root = index;
            }
            else if (this.m_nodes[node2.ParentOrNext].Child1 == iA)
            {
                this.m_nodes[node2.ParentOrNext].Child1 = index;
            }
            else
            {
                this.m_nodes[node2.ParentOrNext].Child2 = index;
            }
            if (node6.Height > node7.Height)
            {
                node2.Child2 = num6;
                node.Child1 = num7;
                node7.ParentOrNext = iA;
                BoundingBox.CreateMerged(ref node3.Aabb, ref node7.Aabb, out node.Aabb);
                BoundingBox.CreateMerged(ref node.Aabb, ref node6.Aabb, out node2.Aabb);
                node.Height = 1 + Math.Max(node3.Height, node7.Height);
                node2.Height = 1 + Math.Max(node.Height, node6.Height);
            }
            else
            {
                node2.Child2 = num7;
                node.Child1 = num6;
                node6.ParentOrNext = iA;
                BoundingBox.CreateMerged(ref node3.Aabb, ref node6.Aabb, out node.Aabb);
                BoundingBox.CreateMerged(ref node.Aabb, ref node7.Aabb, out node2.Aabb);
                node.Height = 1 + Math.Max(node3.Height, node6.Height);
                node2.Height = 1 + Math.Max(node.Height, node7.Height);
            }
            return index;
        }

        public void Clear()
        {
            using (this.m_rwLock.AcquireExclusiveUsing())
            {
                if ((this.m_nodeCapacity < 0x100) || (this.m_nodeCapacity > 0x200))
                {
                    this.m_nodeCapacity = 0x100;
                    this.m_nodes = new DynamicTreeNode[this.m_nodeCapacity];
                    this.m_leafElementCache = new Dictionary<int, DynamicTreeNode>(this.m_nodeCapacity / 4);
                    for (int i = 0; i < this.m_nodeCapacity; i++)
                    {
                        this.m_nodes[i] = new DynamicTreeNode();
                    }
                }
                this.ResetNodes();
            }
        }

        public int CountLeaves(int nodeId)
        {
            int num2;
            using (this.m_rwLock.AcquireSharedUsing())
            {
                if (nodeId == -1)
                {
                    num2 = 0;
                }
                else
                {
                    DynamicTreeNode node = this.m_nodes[nodeId];
                    if (node.IsLeaf())
                    {
                        num2 = 1;
                    }
                    else
                    {
                        num2 = this.CountLeaves(node.Child1) + this.CountLeaves(node.Child2);
                    }
                }
            }
            return num2;
        }

        public static void Dispose()
        {
            List<Stack<int>> stackCacheCollection = m_StackCacheCollection;
            lock (stackCacheCollection)
            {
                m_StackCacheCollection.Clear();
            }
        }

        private void FreeNode(int nodeId)
        {
            this.m_nodes[nodeId].ParentOrNext = this.m_freeList;
            this.m_nodes[nodeId].Height = -1;
            this.m_nodes[nodeId].UserData = null;
            this.m_freeList = nodeId;
            this.m_nodeCount--;
        }

        public BoundingBox GetAabb(int proxyId) => 
            this.m_nodes[proxyId].Aabb;

        public void GetAll<T>(List<T> elementsList, bool clear, List<BoundingBox> boxsList = null)
        {
            if (clear)
            {
                elementsList.Clear();
                if (boxsList != null)
                {
                    boxsList.Clear();
                }
            }
            using (this.m_rwLock.AcquireSharedUsing())
            {
                foreach (KeyValuePair<int, DynamicTreeNode> pair in this.m_leafElementCache)
                {
                    elementsList.Add((T) pair.Value.UserData);
                }
                if (boxsList != null)
                {
                    foreach (KeyValuePair<int, DynamicTreeNode> pair2 in this.m_leafElementCache)
                    {
                        boxsList.Add(pair2.Value.Aabb);
                    }
                }
            }
        }

        public void GetAllNodeBounds(List<BoundingBox> boxsList)
        {
            using (this.m_rwLock.AcquireSharedUsing())
            {
                int index = 0;
                int num2 = 0;
                while ((index < this.m_nodeCapacity) && (num2 < this.m_nodeCount))
                {
                    if (this.m_nodes[index].Height != -1)
                    {
                        num2++;
                        boxsList.Add(this.m_nodes[index].Aabb);
                    }
                    index++;
                }
            }
        }

        public void GetChildren(int proxyId, out int left, out int right)
        {
            left = this.m_nodes[proxyId].Child1;
            right = this.m_nodes[proxyId].Child2;
        }

        public void GetFatAABB(int proxyId, out BoundingBox fatAABB)
        {
            using (this.m_rwLock.AcquireSharedUsing())
            {
                fatAABB = this.m_nodes[proxyId].Aabb;
            }
        }

        public int GetHeight()
        {
            using (this.m_rwLock.AcquireSharedUsing())
            {
                return ((this.m_root != -1) ? this.m_nodes[this.m_root].Height : 0);
            }
        }

        public int GetLeafCount() => 
            this.m_leafElementCache.Count;

        public int GetLeafCount(int proxyId)
        {
            int num = 0;
            Stack<int> stack = this.GetStack();
            stack.Push(proxyId);
            while (stack.Count > 0)
            {
                int index = stack.Pop();
                if (index != -1)
                {
                    DynamicTreeNode node = this.m_nodes[index];
                    if (node.IsLeaf())
                    {
                        num++;
                        continue;
                    }
                    stack.Push(node.Child1);
                    stack.Push(node.Child2);
                }
            }
            this.PushStack(stack);
            return num;
        }

        public void GetNodeLeaves(int proxyId, List<int> children)
        {
            Stack<int> stack = this.GetStack();
            stack.Push(proxyId);
            while (stack.Count > 0)
            {
                int index = stack.Pop();
                if (index != -1)
                {
                    DynamicTreeNode node = this.m_nodes[index];
                    if (node.IsLeaf())
                    {
                        children.Add(index);
                        continue;
                    }
                    stack.Push(node.Child1);
                    stack.Push(node.Child2);
                }
            }
            this.PushStack(stack);
        }

        public int GetRoot() => 
            this.m_root;

        private Stack<int> GetStack()
        {
            Stack<int> currentThreadStack = this.CurrentThreadStack;
            currentThreadStack.Clear();
            return currentThreadStack;
        }

        public T GetUserData<T>(int proxyId) => 
            ((T) this.m_nodes[proxyId].UserData);

        private uint GetUserFlag(int proxyId) => 
            this.m_nodes[proxyId].UserFlag;

        private void Init(Vector3 extension, float aabbMultiplier)
        {
            this.m_extension = extension;
            this.m_aabbMultiplier = aabbMultiplier;
            Stack<int> currentThreadStack = this.CurrentThreadStack;
            this.Clear();
        }

        private void InsertLeaf(int leaf, bool rebalance)
        {
            if (this.m_root == -1)
            {
                this.m_root = leaf;
                this.m_nodes[this.m_root].ParentOrNext = -1;
            }
            else
            {
                BoundingBox aabb = this.m_nodes[leaf].Aabb;
                int root = this.m_root;
                while (true)
                {
                    if (!this.m_nodes[root].IsLeaf())
                    {
                        float num10;
                        float num11;
                        int num5 = this.m_nodes[root].Child1;
                        int num6 = this.m_nodes[root].Child2;
                        if (!rebalance)
                        {
                            BoundingBox box7;
                            BoundingBox box8;
                            BoundingBox.CreateMerged(ref aabb, ref this.m_nodes[num5].Aabb, out box7);
                            BoundingBox.CreateMerged(ref aabb, ref this.m_nodes[num6].Aabb, out box8);
                            root = (((this.m_nodes[num5].Height + 1) * box7.Perimeter) >= ((this.m_nodes[num6].Height + 1) * box8.Perimeter)) ? num6 : num5;
                            continue;
                        }
                        float perimeter = BoundingBox.CreateMerged(this.m_nodes[root].Aabb, aabb).Perimeter;
                        float num9 = 2f * (perimeter - this.m_nodes[root].Aabb.Perimeter);
                        if (this.m_nodes[num5].IsLeaf())
                        {
                            BoundingBox box3;
                            BoundingBox.CreateMerged(ref aabb, ref this.m_nodes[num5].Aabb, out box3);
                            num10 = box3.Perimeter + num9;
                        }
                        else
                        {
                            BoundingBox box4;
                            BoundingBox.CreateMerged(ref aabb, ref this.m_nodes[num5].Aabb, out box4);
                            float num12 = this.m_nodes[num5].Aabb.Perimeter;
                            num10 = (box4.Perimeter - num12) + num9;
                        }
                        if (this.m_nodes[num6].IsLeaf())
                        {
                            BoundingBox box5;
                            BoundingBox.CreateMerged(ref aabb, ref this.m_nodes[num6].Aabb, out box5);
                            num11 = box5.Perimeter + num9;
                        }
                        else
                        {
                            BoundingBox box6;
                            BoundingBox.CreateMerged(ref aabb, ref this.m_nodes[num6].Aabb, out box6);
                            float num13 = this.m_nodes[num6].Aabb.Perimeter;
                            num11 = (box6.Perimeter - num13) + num9;
                        }
                        if (((2f * perimeter) >= num10) || (num10 >= num11))
                        {
                            root = (num10 >= num11) ? num6 : num5;
                            continue;
                        }
                    }
                    int index = root;
                    int parentOrNext = this.m_nodes[root].ParentOrNext;
                    int num4 = this.AllocateNode();
                    this.m_nodes[num4].ParentOrNext = parentOrNext;
                    this.m_nodes[num4].UserData = null;
                    this.m_nodes[num4].Aabb = BoundingBox.CreateMerged(aabb, this.m_nodes[index].Aabb);
                    this.m_nodes[num4].Height = this.m_nodes[index].Height + 1;
                    if (parentOrNext == -1)
                    {
                        this.m_nodes[num4].Child1 = index;
                        this.m_nodes[num4].Child2 = leaf;
                        this.m_nodes[root].ParentOrNext = num4;
                        this.m_nodes[leaf].ParentOrNext = num4;
                        this.m_root = num4;
                    }
                    else
                    {
                        if (this.m_nodes[parentOrNext].Child1 == index)
                        {
                            this.m_nodes[parentOrNext].Child1 = num4;
                        }
                        else
                        {
                            this.m_nodes[parentOrNext].Child2 = num4;
                        }
                        this.m_nodes[num4].Child1 = index;
                        this.m_nodes[num4].Child2 = leaf;
                        this.m_nodes[root].ParentOrNext = num4;
                        this.m_nodes[leaf].ParentOrNext = num4;
                    }
                    for (root = this.m_nodes[leaf].ParentOrNext; root != -1; root = this.m_nodes[root].ParentOrNext)
                    {
                        if (rebalance)
                        {
                            root = this.Balance(root);
                        }
                        int num15 = this.m_nodes[root].Child1;
                        int num16 = this.m_nodes[root].Child2;
                        this.m_nodes[root].Height = 1 + Math.Max(this.m_nodes[num15].Height, this.m_nodes[num16].Height);
                        BoundingBox.CreateMerged(ref this.m_nodes[num15].Aabb, ref this.m_nodes[num16].Aabb, out this.m_nodes[root].Aabb);
                    }
                    return;
                }
            }
        }

        public unsafe bool MoveProxy(int proxyId, ref BoundingBox aabb, Vector3 displacement)
        {
            using (this.m_rwLock.AcquireExclusiveUsing())
            {
                if (this.m_nodes[proxyId].Aabb.Contains(aabb) != ContainmentType.Contains)
                {
                    this.RemoveLeaf(proxyId);
                    BoundingBox box = aabb;
                    Vector3 extension = this.m_extension;
                    Vector3* vectorPtr1 = (Vector3*) ref box.Min;
                    vectorPtr1[0] -= extension;
                    Vector3* vectorPtr2 = (Vector3*) ref box.Max;
                    vectorPtr2[0] += extension;
                    Vector3 vector2 = (Vector3) (this.m_aabbMultiplier * displacement);
                    if (vector2.X < 0f)
                    {
                        float* singlePtr1 = (float*) ref box.Min.X;
                        singlePtr1[0] += vector2.X;
                    }
                    else
                    {
                        float* singlePtr2 = (float*) ref box.Max.X;
                        singlePtr2[0] += vector2.X;
                    }
                    if (vector2.Y < 0f)
                    {
                        float* singlePtr3 = (float*) ref box.Min.Y;
                        singlePtr3[0] += vector2.Y;
                    }
                    else
                    {
                        float* singlePtr4 = (float*) ref box.Max.Y;
                        singlePtr4[0] += vector2.Y;
                    }
                    if (vector2.Z < 0f)
                    {
                        float* singlePtr5 = (float*) ref box.Min.Z;
                        singlePtr5[0] += vector2.Z;
                    }
                    else
                    {
                        float* singlePtr6 = (float*) ref box.Max.Z;
                        singlePtr6[0] += vector2.Z;
                    }
                    this.m_nodes[proxyId].Aabb = box;
                    this.InsertLeaf(proxyId, true);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public void OverlapAllBoundingBox<T>(ref BoundingBox bbox, List<T> elementsList, uint requiredFlags = 0, bool clear = true)
        {
            if (clear)
            {
                elementsList.Clear();
            }
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    while (true)
                    {
                        if (stack.Count <= 0)
                        {
                            this.PushStack(stack);
                            break;
                        }
                        int index = stack.Pop();
                        if (index != -1)
                        {
                            DynamicTreeNode node = this.m_nodes[index];
                            if (node.Aabb.Intersects((BoundingBox) bbox))
                            {
                                if (!node.IsLeaf())
                                {
                                    stack.Push(node.Child1);
                                    stack.Push(node.Child2);
                                    continue;
                                }
                                if ((this.GetUserFlag(index) & requiredFlags) == requiredFlags)
                                {
                                    elementsList.Add(this.GetUserData<T>(index));
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OverlapAllBoundingSphere<T>(ref BoundingSphere sphere, List<T> overlapElementsList, bool clear = true)
        {
            if (clear)
            {
                overlapElementsList.Clear();
            }
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    while (true)
                    {
                        if (stack.Count <= 0)
                        {
                            this.PushStack(stack);
                            break;
                        }
                        int index = stack.Pop();
                        if (index != -1)
                        {
                            DynamicTreeNode node = this.m_nodes[index];
                            if (node.Aabb.Intersects((BoundingSphere) sphere))
                            {
                                if (node.IsLeaf())
                                {
                                    overlapElementsList.Add(this.GetUserData<T>(index));
                                    continue;
                                }
                                stack.Push(node.Child1);
                                stack.Push(node.Child2);
                            }
                        }
                    }
                }
            }
        }

        public void OverlapAllFrustum<T>(ref BoundingFrustum frustum, Action<T, bool> add)
        {
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    while (true)
                    {
                        if (stack.Count <= 0)
                        {
                            this.PushStack(stack);
                            break;
                        }
                        int index = stack.Pop();
                        if (index != -1)
                        {
                            ContainmentType type;
                            DynamicTreeNode node = this.m_nodes[index];
                            frustum.Contains(ref node.Aabb, out type);
                            if (type != ContainmentType.Contains)
                            {
                                if (type != ContainmentType.Intersects)
                                {
                                    continue;
                                }
                                if (node.IsLeaf())
                                {
                                    add(this.GetUserData<T>(index), false);
                                    continue;
                                }
                                stack.Push(node.Child1);
                                stack.Push(node.Child2);
                                continue;
                            }
                            int count = stack.Count;
                            stack.Push(index);
                            while (stack.Count > count)
                            {
                                int num3 = stack.Pop();
                                DynamicTreeNode node2 = this.m_nodes[num3];
                                if (node2.IsLeaf())
                                {
                                    add(this.GetUserData<T>(num3), true);
                                    continue;
                                }
                                if (node2.Child1 != -1)
                                {
                                    stack.Push(node2.Child1);
                                }
                                if (node2.Child2 != -1)
                                {
                                    stack.Push(node2.Child2);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OverlapAllFrustum<T, Op>(ref BoundingFrustum frustum, ref Op add) where Op: struct, AddOp<T>
        {
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    while (true)
                    {
                        if (stack.Count <= 0)
                        {
                            this.PushStack(stack);
                            break;
                        }
                        int index = stack.Pop();
                        if (index != -1)
                        {
                            ContainmentType type;
                            DynamicTreeNode node = this.m_nodes[index];
                            frustum.Contains(ref node.Aabb, out type);
                            if (type != ContainmentType.Contains)
                            {
                                if (type != ContainmentType.Intersects)
                                {
                                    continue;
                                }
                                if (node.IsLeaf())
                                {
                                    add.Add(this.GetUserData<T>(index), false);
                                    continue;
                                }
                                stack.Push(node.Child1);
                                stack.Push(node.Child2);
                                continue;
                            }
                            int count = stack.Count;
                            stack.Push(index);
                            while (stack.Count > count)
                            {
                                int num3 = stack.Pop();
                                DynamicTreeNode node2 = this.m_nodes[num3];
                                if (node2.IsLeaf())
                                {
                                    add.Add(this.GetUserData<T>(num3), true);
                                    continue;
                                }
                                if (node2.Child1 != -1)
                                {
                                    stack.Push(node2.Child1);
                                }
                                if (node2.Child2 != -1)
                                {
                                    stack.Push(node2.Child2);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OverlapAllFrustum<T>(ref BoundingFrustum frustum, Action<T, bool> add, float tSqr)
        {
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    while (true)
                    {
                        if (stack.Count <= 0)
                        {
                            this.PushStack(stack);
                            break;
                        }
                        int index = stack.Pop();
                        if (index != -1)
                        {
                            ContainmentType type;
                            DynamicTreeNode node = this.m_nodes[index];
                            frustum.Contains(ref node.Aabb, out type);
                            if (type != ContainmentType.Contains)
                            {
                                if (type != ContainmentType.Intersects)
                                {
                                    continue;
                                }
                                if (!node.IsLeaf())
                                {
                                    stack.Push(node.Child1);
                                    stack.Push(node.Child2);
                                    continue;
                                }
                                if (node.Aabb.Size.LengthSquared() <= tSqr)
                                {
                                    continue;
                                }
                                add(this.GetUserData<T>(index), false);
                                continue;
                            }
                            int count = stack.Count;
                            stack.Push(index);
                            while (stack.Count > count)
                            {
                                int num3 = stack.Pop();
                                DynamicTreeNode node2 = this.m_nodes[num3];
                                if (node2.IsLeaf())
                                {
                                    if (node.Aabb.Size.LengthSquared() <= tSqr)
                                    {
                                        continue;
                                    }
                                    add(this.GetUserData<T>(num3), true);
                                    continue;
                                }
                                if (node2.Child1 != -1)
                                {
                                    stack.Push(node2.Child1);
                                }
                                if (node2.Child2 != -1)
                                {
                                    stack.Push(node2.Child2);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OverlapAllFrustum<T>(ref BoundingFrustum frustum, List<T> elementsList, bool clear = true)
        {
            this.OverlapAllFrustum<T>(ref frustum, elementsList, (uint) 0, clear);
        }

        public void OverlapAllFrustum<T, Op>(ref BoundingFrustum frustum, float tSqr, ref Op add) where Op: struct, AddOp<T>
        {
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    while (true)
                    {
                        if (stack.Count <= 0)
                        {
                            this.PushStack(stack);
                            break;
                        }
                        int index = stack.Pop();
                        if (index != -1)
                        {
                            ContainmentType type;
                            DynamicTreeNode node = this.m_nodes[index];
                            frustum.Contains(ref node.Aabb, out type);
                            if (type != ContainmentType.Contains)
                            {
                                if (type != ContainmentType.Intersects)
                                {
                                    continue;
                                }
                                if (!node.IsLeaf())
                                {
                                    stack.Push(node.Child1);
                                    stack.Push(node.Child2);
                                    continue;
                                }
                                if (node.Aabb.Size.LengthSquared() <= tSqr)
                                {
                                    continue;
                                }
                                add.Add(this.GetUserData<T>(index), false);
                                continue;
                            }
                            int count = stack.Count;
                            stack.Push(index);
                            while (stack.Count > count)
                            {
                                int num3 = stack.Pop();
                                DynamicTreeNode node2 = this.m_nodes[num3];
                                if (node2.IsLeaf())
                                {
                                    if (node.Aabb.Size.LengthSquared() <= tSqr)
                                    {
                                        continue;
                                    }
                                    add.Add(this.GetUserData<T>(num3), true);
                                    continue;
                                }
                                if (node2.Child1 != -1)
                                {
                                    stack.Push(node2.Child1);
                                }
                                if (node2.Child2 != -1)
                                {
                                    stack.Push(node2.Child2);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OverlapAllFrustum<T>(ref BoundingFrustum frustum, List<T> elementsList, List<bool> isInsideList, bool clear = true)
        {
            if (clear)
            {
                elementsList.Clear();
                isInsideList.Clear();
            }
            this.OverlapAllFrustum<T>(ref frustum, delegate (T x, bool y) {
                elementsList.Add(x);
                isInsideList.Add(y);
            });
        }

        public void OverlapAllFrustum<T>(ref BoundingFrustum frustum, List<T> elementsList, uint requiredFlags, bool clear = true)
        {
            if (clear)
            {
                elementsList.Clear();
            }
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    while (true)
                    {
                        if (stack.Count <= 0)
                        {
                            this.PushStack(stack);
                            break;
                        }
                        int index = stack.Pop();
                        if (index != -1)
                        {
                            ContainmentType type;
                            DynamicTreeNode node = this.m_nodes[index];
                            frustum.Contains(ref node.Aabb, out type);
                            if (type != ContainmentType.Contains)
                            {
                                if (type != ContainmentType.Intersects)
                                {
                                    continue;
                                }
                                if (!node.IsLeaf())
                                {
                                    stack.Push(node.Child1);
                                    stack.Push(node.Child2);
                                    continue;
                                }
                                if ((this.GetUserFlag(index) & requiredFlags) != requiredFlags)
                                {
                                    continue;
                                }
                                elementsList.Add(this.GetUserData<T>(index));
                                continue;
                            }
                            int count = stack.Count;
                            stack.Push(index);
                            while (stack.Count > count)
                            {
                                int num3 = stack.Pop();
                                DynamicTreeNode node2 = this.m_nodes[num3];
                                if (node2.IsLeaf())
                                {
                                    if ((this.GetUserFlag(num3) & requiredFlags) != requiredFlags)
                                    {
                                        continue;
                                    }
                                    elementsList.Add(this.GetUserData<T>(num3));
                                    continue;
                                }
                                if (node2.Child1 != -1)
                                {
                                    stack.Push(node2.Child1);
                                }
                                if (node2.Child2 != -1)
                                {
                                    stack.Push(node2.Child2);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OverlapAllFrustum<T>(ref BoundingFrustum frustum, List<T> elementsList, List<bool> isInsideList, float tSqr, bool clear = true)
        {
            if (clear)
            {
                elementsList.Clear();
                isInsideList.Clear();
            }
            this.OverlapAllFrustum<T>(ref frustum, delegate (T x, bool y) {
                elementsList.Add(x);
                isInsideList.Add(y);
            }, tSqr);
        }

        public void OverlapAllFrustumAny<T>(ref BoundingFrustum frustum, List<T> elementsList, bool clear = true)
        {
            if (clear)
            {
                elementsList.Clear();
            }
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    while (true)
                    {
                        if (stack.Count <= 0)
                        {
                            this.PushStack(stack);
                            break;
                        }
                        int index = stack.Pop();
                        if (index != -1)
                        {
                            ContainmentType type;
                            DynamicTreeNode node = this.m_nodes[index];
                            frustum.Contains(ref node.Aabb, out type);
                            if (type != ContainmentType.Contains)
                            {
                                if (type != ContainmentType.Intersects)
                                {
                                    continue;
                                }
                                if (node.IsLeaf())
                                {
                                    T userData = this.GetUserData<T>(index);
                                    elementsList.Add(userData);
                                    continue;
                                }
                                stack.Push(node.Child1);
                                stack.Push(node.Child2);
                                continue;
                            }
                            int count = stack.Count;
                            stack.Push(index);
                            while (stack.Count > count)
                            {
                                int num3 = stack.Pop();
                                DynamicTreeNode node2 = this.m_nodes[num3];
                                if (node2.IsLeaf())
                                {
                                    T userData = this.GetUserData<T>(num3);
                                    elementsList.Add(userData);
                                    continue;
                                }
                                if (node2.Child1 != -1)
                                {
                                    stack.Push(node2.Child1);
                                }
                                if (node2.Child2 != -1)
                                {
                                    stack.Push(node2.Child2);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OverlapAllFrustumConservative<T>(ref BoundingFrustum frustum, List<T> elementsList, uint requiredFlags, bool clear = true)
        {
            if (clear)
            {
                elementsList.Clear();
            }
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    BoundingBox box = BoundingBox.CreateInvalid();
                    box.Include(ref frustum);
                    while (true)
                    {
                        if (stack.Count <= 0)
                        {
                            this.PushStack(stack);
                            break;
                        }
                        int index = stack.Pop();
                        if (index != -1)
                        {
                            DynamicTreeNode node = this.m_nodes[index];
                            if (node.Aabb.Intersects(box))
                            {
                                ContainmentType type;
                                frustum.Contains(ref node.Aabb, out type);
                                if (type != ContainmentType.Contains)
                                {
                                    if (type != ContainmentType.Intersects)
                                    {
                                        continue;
                                    }
                                    if (!node.IsLeaf())
                                    {
                                        stack.Push(node.Child1);
                                        stack.Push(node.Child2);
                                        continue;
                                    }
                                    if ((this.GetUserFlag(index) & requiredFlags) != requiredFlags)
                                    {
                                        continue;
                                    }
                                    elementsList.Add(this.GetUserData<T>(index));
                                    continue;
                                }
                                int count = stack.Count;
                                stack.Push(index);
                                while (stack.Count > count)
                                {
                                    int num3 = stack.Pop();
                                    DynamicTreeNode node2 = this.m_nodes[num3];
                                    if (node2.IsLeaf())
                                    {
                                        if ((this.GetUserFlag(num3) & requiredFlags) != requiredFlags)
                                        {
                                            continue;
                                        }
                                        elementsList.Add(this.GetUserData<T>(num3));
                                        continue;
                                    }
                                    if (node2.Child1 != -1)
                                    {
                                        stack.Push(node2.Child1);
                                    }
                                    if (node2.Child2 != -1)
                                    {
                                        stack.Push(node2.Child2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OverlapAllLineSegment<T>(ref Line line, List<MyLineSegmentOverlapResult<T>> elementsList)
        {
            this.OverlapAllLineSegment<T>(ref line, elementsList, 0);
        }

        public void OverlapAllLineSegment<T>(ref Line line, List<MyLineSegmentOverlapResult<T>> elementsList, uint requiredFlags)
        {
            elementsList.Clear();
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    BoundingBox box = BoundingBox.CreateInvalid();
                    box.Include(ref line);
                    Ray ray = new Ray(line.From, line.Direction);
                    while (true)
                    {
                        if (stack.Count <= 0)
                        {
                            this.PushStack(stack);
                            break;
                        }
                        int index = stack.Pop();
                        if (index != -1)
                        {
                            DynamicTreeNode node = this.m_nodes[index];
                            if (node.Aabb.Intersects(box))
                            {
                                float? nullable = node.Aabb.Intersects(ray);
                                if ((nullable != null) && ((nullable.Value <= line.Length) && (nullable.Value >= 0f)))
                                {
                                    if (!node.IsLeaf())
                                    {
                                        stack.Push(node.Child1);
                                        stack.Push(node.Child2);
                                        continue;
                                    }
                                    if ((this.GetUserFlag(index) & requiredFlags) == requiredFlags)
                                    {
                                        MyLineSegmentOverlapResult<T> item = new MyLineSegmentOverlapResult<T> {
                                            Element = this.GetUserData<T>(index),
                                            Distance = (double) nullable.Value
                                        };
                                        elementsList.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool OverlapsAnyLeafBoundingBox(ref BoundingBox bbox)
        {
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    while (true)
                    {
                        if (stack.Count > 0)
                        {
                            int index = stack.Pop();
                            if (index == -1)
                            {
                                continue;
                            }
                            DynamicTreeNode node = this.m_nodes[index];
                            if (!node.Aabb.Intersects((BoundingBox) bbox))
                            {
                                continue;
                            }
                            if (!node.IsLeaf())
                            {
                                stack.Push(node.Child1);
                                stack.Push(node.Child2);
                                continue;
                            }
                            return true;
                        }
                        else
                        {
                            this.PushStack(stack);
                        }
                        break;
                    }
                }
            }
            return false;
        }

        public void OverlapSizeableClusters(ref BoundingBox bbox, List<BoundingBox> boundList, double minSize)
        {
            if (this.m_root != -1)
            {
                using (this.m_rwLock.AcquireSharedUsing())
                {
                    Stack<int> stack = this.GetStack();
                    stack.Push(this.m_root);
                    while (true)
                    {
                        if (stack.Count <= 0)
                        {
                            this.PushStack(stack);
                            break;
                        }
                        int index = stack.Pop();
                        if (index != -1)
                        {
                            DynamicTreeNode node = this.m_nodes[index];
                            if (node.Aabb.Intersects((BoundingBox) bbox))
                            {
                                if (node.IsLeaf() || (node.Aabb.Size.Max() <= minSize))
                                {
                                    boundList.Add(node.Aabb);
                                    continue;
                                }
                                stack.Push(node.Child1);
                                stack.Push(node.Child2);
                            }
                        }
                    }
                }
            }
        }

        private void PushStack(Stack<int> stack)
        {
        }

        public void Query(Func<int, bool> callback, ref BoundingBox aabb)
        {
            using (this.m_rwLock.AcquireSharedUsing())
            {
                Stack<int> stack = this.GetStack();
                stack.Push(this.m_root);
                while (stack.Count > 0)
                {
                    int index = stack.Pop();
                    if (index != -1)
                    {
                        DynamicTreeNode node = this.m_nodes[index];
                        if (node.Aabb.Intersects((BoundingBox) aabb))
                        {
                            if (!node.IsLeaf())
                            {
                                stack.Push(node.Child1);
                                stack.Push(node.Child2);
                                continue;
                            }
                            if (!callback(index))
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void RemoveLeaf(int leaf)
        {
            if (this.m_root != -1)
            {
                if (leaf == this.m_root)
                {
                    this.m_root = -1;
                }
                else
                {
                    int parentOrNext = this.m_nodes[leaf].ParentOrNext;
                    int index = this.m_nodes[parentOrNext].ParentOrNext;
                    int num3 = (this.m_nodes[parentOrNext].Child1 != leaf) ? this.m_nodes[parentOrNext].Child1 : this.m_nodes[parentOrNext].Child2;
                    if (index == -1)
                    {
                        this.m_root = num3;
                        this.m_nodes[num3].ParentOrNext = -1;
                        this.FreeNode(parentOrNext);
                    }
                    else
                    {
                        if (this.m_nodes[index].Child1 == parentOrNext)
                        {
                            this.m_nodes[index].Child1 = num3;
                        }
                        else
                        {
                            this.m_nodes[index].Child2 = num3;
                        }
                        this.m_nodes[num3].ParentOrNext = index;
                        this.FreeNode(parentOrNext);
                        for (int i = index; i != -1; i = this.m_nodes[i].ParentOrNext)
                        {
                            i = this.Balance(i);
                            int num5 = this.m_nodes[i].Child1;
                            int num6 = this.m_nodes[i].Child2;
                            this.m_nodes[i].Aabb = BoundingBox.CreateMerged(this.m_nodes[num5].Aabb, this.m_nodes[num6].Aabb);
                            this.m_nodes[i].Height = 1 + Math.Max(this.m_nodes[num5].Height, this.m_nodes[num6].Height);
                        }
                    }
                }
            }
        }

        public void RemoveProxy(int proxyId)
        {
            using (this.m_rwLock.AcquireExclusiveUsing())
            {
                this.m_leafElementCache.Remove(proxyId);
                this.RemoveLeaf(proxyId);
                this.FreeNode(proxyId);
            }
        }

        private void ResetNodes()
        {
            this.m_leafElementCache.Clear();
            this.m_root = -1;
            this.m_nodeCount = 0;
            for (int i = 0; i < (this.m_nodeCapacity - 1); i++)
            {
                this.m_nodes[i].ParentOrNext = i + 1;
                this.m_nodes[i].Height = 1;
                this.m_nodes[i].UserData = null;
            }
            this.m_nodes[this.m_nodeCapacity - 1].ParentOrNext = -1;
            this.m_nodes[this.m_nodeCapacity - 1].Height = 1;
            this.m_freeList = 0;
        }

        public DictionaryValuesReader<int, DynamicTreeNode> Leaves =>
            new DictionaryValuesReader<int, DynamicTreeNode>(this.m_leafElementCache);

        private Stack<int> CurrentThreadStack
        {
            get
            {
                if (m_queryStack == null)
                {
                    m_queryStack = new Stack<int>(0x20);
                    List<Stack<int>> stackCacheCollection = m_StackCacheCollection;
                    lock (stackCacheCollection)
                    {
                        m_StackCacheCollection.Add(m_queryStack);
                    }
                }
                return m_queryStack;
            }
        }

        public class DynamicTreeNode
        {
            public BoundingBox Aabb;
            public int Child1;
            public int Child2;
            public int Height;
            public int ParentOrNext;
            public object UserData;
            public uint UserFlag;

            public bool IsLeaf() => 
                (this.Child1 == -1);
        }
    }
}

