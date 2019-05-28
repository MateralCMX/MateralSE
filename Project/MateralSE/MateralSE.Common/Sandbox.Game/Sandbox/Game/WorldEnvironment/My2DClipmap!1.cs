namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox.Game.WorldEnvironment.__helper_namespace;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Library.Collections;
    using VRageMath;

    public class My2DClipmap<THandler> where THandler: class, IMy2DClipmapNodeHandler, new()
    {
        private int m_root;
        private double m_size;
        private int m_splits;
        private double[] m_lodSizes;
        private double[] m_keepLodSizes;
        private MyFreeList<THandler> m_leafHandlers;
        private Node[] m_nodes;
        private THandler[] m_nodeHandlers;
        private int m_firstFree;
        private const int NullNode = -2147483648;
        public int NodeAllocDeallocs;
        private readonly List<int> m_nodesToDealloc;
        private IMy2DClipmapManager m_manager;
        private readonly Stack<StackInfo<THandler>> m_nodesToScanNext;
        private BoundingBox2D[] m_queryBounds;
        private BoundingBox2D[] m_keepBounds;
        private readonly BoundingBox2D[] m_nodeBoundsTmp;
        private readonly IMy2DClipmapNodeHandler[] m_tmpNodeHandlerList;

        public My2DClipmap()
        {
            this.m_nodesToDealloc = new List<int>();
            this.m_nodesToScanNext = new Stack<StackInfo<THandler>>();
            this.m_nodeBoundsTmp = new BoundingBox2D[4];
            this.m_tmpNodeHandlerList = new IMy2DClipmapNodeHandler[4];
        }

        private unsafe int AllocNode()
        {
            Node[] pinned nodeArray;
            Node* nodePtr2;
            this.NodeAllocDeallocs++;
            if (this.m_firstFree == this.m_nodes.Length)
            {
                Node* nodePtr;
                int length = this.m_nodes.Length;
                Array.Resize<Node>(ref this.m_nodes, this.m_nodes.Length << 1);
                Array.Resize<THandler>(ref this.m_nodeHandlers, this.m_nodes.Length);
                if (((nodeArray = this.m_nodes) == null) || (nodeArray.Length == 0))
                {
                    nodePtr = null;
                }
                else
                {
                    nodePtr = nodeArray;
                }
                int index = length;
                while (true)
                {
                    if (index >= this.m_nodes.Length)
                    {
                        nodeArray = null;
                        this.m_firstFree = length;
                        break;
                    }
                    nodePtr[index].Lod = ~(index + 1);
                    index++;
                }
            }
            int firstFree = this.m_firstFree;
            if (((nodeArray = this.m_nodes) == null) || (nodeArray.Length == 0))
            {
                nodePtr2 = null;
            }
            else
            {
                nodePtr2 = nodeArray;
            }
            for (int i = 0; i < 4; i++)
            {
                &nodePtr2[this.m_firstFree].Children.FixedElementField[i] = -2147483648;
            }
            this.m_firstFree = ~nodePtr2[this.m_firstFree].Lod;
            nodeArray = null;
            return firstFree;
        }

        private unsafe int Child(int node, int index)
        {
            Node* nodePtr;
            Node[] pinned nodeArray;
            if (((nodeArray = this.m_nodes) == null) || (nodeArray.Length == 0))
            {
                nodePtr = null;
            }
            else
            {
                nodePtr = nodeArray;
            }
            return &nodePtr[node].Children.FixedElementField[index];
        }

        public void Clear()
        {
            this.CollapseRoot();
        }

        private unsafe void CollapseRoot()
        {
            Node* nodePtr;
            Node[] pinned nodeArray;
            if (((nodeArray = this.m_nodes) == null) || (nodeArray.Length == 0))
            {
                nodePtr = null;
            }
            else
            {
                nodePtr = nodeArray;
            }
            Node* nodePtr2 = nodePtr + this.m_root;
            if (nodePtr2->Children.FixedElementField != -2147483648)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (&nodePtr2->Children.FixedElementField[i] >= 0)
                    {
                        this.CollapseSubtree(this.m_root, i, nodePtr);
                    }
                }
                IMy2DClipmapNodeHandler[] tmpNodeHandlerList = this.m_tmpNodeHandlerList;
                for (int j = 0; j < 4; j++)
                {
                    tmpNodeHandlerList[j] = this.m_leafHandlers[~&nodePtr2->Children.FixedElementField[j]];
                }
                this.m_nodeHandlers[this.m_root].InitJoin(tmpNodeHandlerList);
                for (int k = 0; k < 4; k++)
                {
                    this.m_leafHandlers.Free(~&nodePtr2->Children.FixedElementField[k]);
                    tmpNodeHandlerList[k].Close();
                    &nodePtr2->Children.FixedElementField[k] = -2147483648;
                }
                nodeArray = null;
                foreach (int num4 in this.m_nodesToDealloc)
                {
                    this.FreeNode(num4);
                }
                this.m_nodesToDealloc.Clear();
            }
        }

        private unsafe void CollapseSubtree(int parent, int childIndex, Node* nodes)
        {
            int item = &nodes[parent].Children.FixedElementField[childIndex];
            this.m_nodesToDealloc.Add(item);
            Node* nodePtr = nodes + item;
            for (int i = 0; i < 4; i++)
            {
                if (&nodePtr->Children.FixedElementField[i] >= 0)
                {
                    this.CollapseSubtree(item, i, nodes);
                }
            }
            IMy2DClipmapNodeHandler[] tmpNodeHandlerList = this.m_tmpNodeHandlerList;
            for (int j = 0; j < 4; j++)
            {
                tmpNodeHandlerList[j] = this.m_leafHandlers[~&nodePtr->Children.FixedElementField[j]];
            }
            THandler local = this.m_nodeHandlers[item];
            local.InitJoin(tmpNodeHandlerList);
            for (int k = 0; k < 4; k++)
            {
                this.m_leafHandlers.Free(~&nodePtr->Children.FixedElementField[k]);
                tmpNodeHandlerList[k].Close();
            }
            int num2 = this.m_leafHandlers.Allocate();
            &nodes[parent].Children.FixedElementField[childIndex] = ~num2;
            this.m_leafHandlers[num2] = local;
        }

        private void Compact()
        {
        }

        private unsafe void FreeNode(int node)
        {
            Node* nodePtr;
            Node[] pinned nodeArray;
            this.NodeAllocDeallocs++;
            if (((nodeArray = this.m_nodes) == null) || (nodeArray.Length == 0))
            {
                nodePtr = null;
            }
            else
            {
                nodePtr = nodeArray;
            }
            nodePtr[node].Lod = ~this.m_firstFree;
            this.m_firstFree = node;
            this.m_nodeHandlers[node] = default(THandler);
            nodeArray = null;
        }

        public unsafe THandler GetHandler(Vector2D point)
        {
            Node* nodePtr;
            Node[] pinned nodeArray;
            this.m_nodesToScanNext.Push(new StackInfo<THandler>(this.m_root, Vector2D.Zero, this.m_size / 2.0, this.m_splits));
            int root = this.m_root;
            if (((nodeArray = this.m_nodes) == null) || (nodeArray.Length == 0))
            {
                nodePtr = null;
            }
            else
            {
                nodePtr = nodeArray;
            }
            while (this.m_nodesToScanNext.Count != 0)
            {
                StackInfo<THandler> info = this.m_nodesToScanNext.Pop();
                double size = info.Size / 2.0;
                int lod = info.Lod - 1;
                for (int i = 0; i < 4; i++)
                {
                    BoundingBox2D boxd;
                    boxd.Min = (info.Center + (My2DClipmapHelpers.CoordsFromIndex[i] * info.Size)) - info.Size;
                    boxd.Max = info.Center + (My2DClipmapHelpers.CoordsFromIndex[i] * info.Size);
                    if (boxd.Contains(point) != ContainmentType.Disjoint)
                    {
                        int num5 = &nodePtr[info.Node].Children.FixedElementField[i];
                        if (num5 != -2147483648)
                        {
                            root = num5;
                            if ((lod > 0) && (num5 >= 0))
                            {
                                this.m_nodesToScanNext.Push(new StackInfo<THandler>(root, (info.Center + (My2DClipmapHelpers.CoordsFromIndex[i] * info.Size)) - size, size, lod));
                            }
                        }
                    }
                }
            }
            nodeArray = null;
            return ((root >= 0) ? this.m_nodeHandlers[root] : this.m_leafHandlers[~root]);
        }

        public unsafe void Init(IMy2DClipmapManager manager, ref MatrixD worldMatrix, double sectorSize, double faceSize)
        {
            Node* nodePtr;
            Node[] pinned nodeArray;
            this.m_manager = manager;
            this.WorldMatrix = worldMatrix;
            this.InverseWorldMatrix = Matrix.Invert((Matrix) worldMatrix);
            this.m_size = faceSize;
            this.m_splits = Math.Max(MathHelper.Log2Floor((int) (faceSize / sectorSize)), 1);
            this.m_lodSizes = new double[this.m_splits + 1];
            for (int i = 0; i <= this.m_splits; i++)
            {
                this.m_lodSizes[this.m_splits - i] = faceSize / ((double) (1 << ((i + 1) & 0x1f)));
            }
            this.m_keepLodSizes = new double[this.m_splits + 1];
            for (int j = 0; j <= this.m_splits; j++)
            {
                this.m_keepLodSizes[j] = 1.5 * this.m_lodSizes[j];
            }
            this.m_queryBounds = new BoundingBox2D[this.m_splits + 1];
            this.m_keepBounds = new BoundingBox2D[this.m_splits + 1];
            this.PrepareAllocator();
            this.m_root = this.AllocNode();
            if (((nodeArray = this.m_nodes) == null) || (nodeArray.Length == 0))
            {
                nodePtr = null;
            }
            else
            {
                nodePtr = nodeArray;
            }
            nodePtr[this.m_root].Lod = this.m_splits;
            nodeArray = null;
            BoundingBox2D bounds = new BoundingBox2D(new Vector2D(-faceSize / 2.0), new Vector2D(faceSize / 2.0));
            this.m_nodeHandlers[this.m_root] = Activator.CreateInstance<THandler>();
            this.m_nodeHandlers[this.m_root].Init(this.m_manager, 0, 0, this.m_splits, ref bounds);
        }

        private unsafe void PrepareAllocator()
        {
            Node* nodePtr;
            Node[] pinned nodeArray;
            int num = 0x10;
            this.m_nodes = new Node[num];
            if (((nodeArray = this.m_nodes) == null) || (nodeArray.Length == 0))
            {
                nodePtr = null;
            }
            else
            {
                nodePtr = nodeArray;
            }
            for (int i = 0; i < num; i++)
            {
                nodePtr[i].Lod = ~(i + 1);
            }
            nodeArray = null;
            this.m_firstFree = 0;
            this.m_nodeHandlers = new THandler[num];
            THandler defaultValue = default(THandler);
            this.m_leafHandlers = new MyFreeList<THandler>(0x10, defaultValue);
        }

        public unsafe void Update(Vector3D localPosition)
        {
            double num = localPosition.Z * 0.1;
            num *= num;
            if (Vector3D.DistanceSquared(this.LastPosition, localPosition) >= num)
            {
                this.LastPosition = localPosition;
                Vector2D vectord = new Vector2D(localPosition.X, localPosition.Y);
                for (int i = this.m_splits; i >= 0; i--)
                {
                    this.m_queryBounds[i] = new BoundingBox2D(vectord - this.m_lodSizes[i], vectord + this.m_lodSizes[i]);
                    this.m_keepBounds[i] = new BoundingBox2D(vectord - this.m_keepLodSizes[i], vectord + this.m_keepLodSizes[i]);
                }
                if (localPosition.Z > this.m_keepLodSizes[this.m_splits])
                {
                    if (this.Child(this.m_root, 0) != -2147483648)
                    {
                        this.CollapseRoot();
                    }
                }
                else
                {
                    BoundingBox2D* boxdPtr;
                    BoundingBox2D[] pinned boxdArray;
                    BoundingBox2D* boxdPtr2;
                    BoundingBox2D[] pinned boxdArray2;
                    BoundingBox2D* boxdPtr3;
                    BoundingBox2D[] pinned boxdArray3;
                    this.m_nodesToScanNext.Push(new StackInfo<THandler>(this.m_root, Vector2D.Zero, this.m_size / 2.0, this.m_splits));
                    if (((boxdArray = this.m_nodeBoundsTmp) == null) || (boxdArray.Length == 0))
                    {
                        boxdPtr = null;
                    }
                    else
                    {
                        boxdPtr = boxdArray;
                    }
                    if (((boxdArray2 = this.m_keepBounds) == null) || (boxdArray2.Length == 0))
                    {
                        boxdPtr2 = null;
                    }
                    else
                    {
                        boxdPtr2 = boxdArray2;
                    }
                    if (((boxdArray3 = this.m_queryBounds) == null) || (boxdArray3.Length == 0))
                    {
                        boxdPtr3 = null;
                    }
                    else
                    {
                        boxdPtr3 = boxdArray3;
                    }
                    while (true)
                    {
                        if (this.m_nodesToScanNext.Count == 0)
                        {
                            boxdArray3 = null;
                            boxdArray2 = null;
                            boxdArray = null;
                            break;
                        }
                        StackInfo<THandler> info = this.m_nodesToScanNext.Pop();
                        double size = info.Size / 2.0;
                        int lod = info.Lod - 1;
                        int num5 = 0;
                        int index = 0;
                        while (true)
                        {
                            if (index >= 4)
                            {
                                Node[] pinned nodeArray;
                                if (this.Child(info.Node, 0) == -2147483648)
                                {
                                    Node* nodePtr;
                                    THandler local = this.m_nodeHandlers[info.Node];
                                    IMy2DClipmapNodeHandler[] tmpNodeHandlerList = this.m_tmpNodeHandlerList;
                                    if (((nodeArray = this.m_nodes) == null) || (nodeArray.Length == 0))
                                    {
                                        nodePtr = null;
                                    }
                                    else
                                    {
                                        nodePtr = nodeArray;
                                    }
                                    int num7 = 0;
                                    while (true)
                                    {
                                        if (num7 >= 4)
                                        {
                                            nodeArray = null;
                                            local.Split(boxdPtr, ref tmpNodeHandlerList);
                                            local.Close();
                                            break;
                                        }
                                        int num8 = this.m_leafHandlers.Allocate();
                                        this.m_leafHandlers[num8] = Activator.CreateInstance<THandler>();
                                        tmpNodeHandlerList[num7] = this.m_leafHandlers[num8];
                                        &nodePtr[info.Node].Children.FixedElementField[num7] = ~num8;
                                        num7++;
                                    }
                                }
                                if (info.Lod != 1)
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
                                        int num10 = this.Child(info.Node, j);
                                        if ((num5 & (1 << (j & 0x1f))) == 0)
                                        {
                                            if ((num10 >= 0) && (!(boxdPtr + j).Intersects(ref (ref BoundingBox2D) ((BoundingBox2D) ref (boxdPtr2 + lod))) || (localPosition.Z > (boxdPtr2 + lod).Height)))
                                            {
                                                Node* nodePtr3;
                                                if (((nodeArray = this.m_nodes) == null) || (nodeArray.Length == 0))
                                                {
                                                    nodePtr3 = null;
                                                }
                                                else
                                                {
                                                    nodePtr3 = nodeArray;
                                                }
                                                this.CollapseSubtree(info.Node, j, nodePtr3);
                                                nodeArray = null;
                                            }
                                        }
                                        else if (num10 < 0)
                                        {
                                            Node* nodePtr2;
                                            this.m_leafHandlers.Free(~num10);
                                            num10 = this.AllocNode();
                                            this.m_nodeHandlers[num10] = this.m_leafHandlers[~num10];
                                            if (((nodeArray = this.m_nodes) == null) || (nodeArray.Length == 0))
                                            {
                                                nodePtr2 = null;
                                            }
                                            else
                                            {
                                                nodePtr2 = nodeArray;
                                            }
                                            nodePtr2[num10].Lod = lod;
                                            &nodePtr2[info.Node].Children.FixedElementField[j] = num10;
                                            nodeArray = null;
                                        }
                                        if (num10 >= 0)
                                        {
                                            this.m_nodesToScanNext.Push(new StackInfo<THandler>(num10, (info.Center + (My2DClipmapHelpers.CoordsFromIndex[j] * info.Size)) - size, size, lod));
                                        }
                                    }
                                }
                                break;
                            }
                            boxdPtr[index].Min = (info.Center + (My2DClipmapHelpers.CoordsFromIndex[index] * info.Size)) - info.Size;
                            boxdPtr[index].Max = info.Center + (My2DClipmapHelpers.CoordsFromIndex[index] * info.Size);
                            if ((boxdPtr + index).Intersects(ref (ref BoundingBox2D) ((BoundingBox2D) ref (boxdPtr3 + lod))) && (localPosition.Z <= (boxdPtr3 + lod).Height))
                            {
                                num5 |= 1 << (index & 0x1f);
                            }
                            index++;
                        }
                    }
                }
                foreach (int num11 in this.m_nodesToDealloc)
                {
                    this.FreeNode(num11);
                }
                this.m_nodesToDealloc.Clear();
            }
        }

        public Vector3D LastPosition { get; set; }

        public MatrixD WorldMatrix { get; private set; }

        public MatrixD InverseWorldMatrix { get; private set; }

        public double FaceHalf =>
            this.m_lodSizes[this.m_splits];

        public double LeafSize =>
            this.m_lodSizes[1];

        public int Depth =>
            this.m_splits;

        [StructLayout(LayoutKind.Sequential)]
        private struct StackInfo
        {
            public int Node;
            public Vector2D Center;
            public double Size;
            public int Lod;
            public StackInfo(int node, Vector2D center, double size, int lod)
            {
                this.Node = node;
                this.Center = center;
                this.Size = size;
                this.Lod = lod;
            }
        }
    }
}

