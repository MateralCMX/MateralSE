namespace VRage.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;

    public class MyGroups<TNode, TGroupData> : MyGroupsBase<TNode> where TNode: class where TGroupData: IGroupData<TNode>, new()
    {
        private Stack<Group<TNode, TGroupData>> m_groupPool;
        private Stack<Node<TNode, TGroupData>> m_nodePool;
        private Dictionary<TNode, Node<TNode, TGroupData>> m_nodes;
        private HashSet<Group<TNode, TGroupData>> m_groups;
        private HashSet<Node<TNode, TGroupData>> m_disconnectHelper;
        private MajorGroupComparer<TNode, TGroupData> m_groupSelector;
        private bool m_isRecalculating;
        private HashSet<Node<TNode, TGroupData>> m_tmpClosed;
        private Queue<Node<TNode, TGroupData>> m_tmpOpen;
        private List<Node<TNode, TGroupData>> m_tmpList;

        public MyGroups(bool supportOphrans = false, MajorGroupComparer<TNode, TGroupData> groupSelector = null)
        {
            this.m_groupPool = new Stack<Group<TNode, TGroupData>>(0x20);
            this.m_nodePool = new Stack<Node<TNode, TGroupData>>(0x20);
            this.m_nodes = new Dictionary<TNode, Node<TNode, TGroupData>>();
            this.m_groups = new HashSet<Group<TNode, TGroupData>>();
            this.m_disconnectHelper = new HashSet<Node<TNode, TGroupData>>();
            this.m_tmpClosed = new HashSet<Node<TNode, TGroupData>>();
            this.m_tmpOpen = new Queue<Node<TNode, TGroupData>>();
            this.m_tmpList = new List<Node<TNode, TGroupData>>();
            this.SupportsOphrans = supportOphrans;
            this.m_groupSelector = groupSelector ?? new MajorGroupComparer<TNode, TGroupData>(MyGroups<TNode, TGroupData>.IsMajorGroup);
        }

        private Group<TNode, TGroupData> AcquireGroup()
        {
            Group<TNode, TGroupData> item = (this.m_groupPool.Count > 0) ? this.m_groupPool.Pop() : new Group<TNode, TGroupData>();
            this.m_groups.Add(item);
            item.GroupData.OnCreate<TGroupData>(item);
            return item;
        }

        private Node<TNode, TGroupData> AcquireNode() => 
            ((this.m_nodePool.Count > 0) ? this.m_nodePool.Pop() : new Node<TNode, TGroupData>());

        private void AddLink(long linkId, Node<TNode, TGroupData> parent, Node<TNode, TGroupData> child)
        {
            parent.m_children[linkId] = child;
            child.m_parents[linkId] = parent;
        }

        private void AddNeighbours(HashSet<Node<TNode, TGroupData>> nodes, Node<TNode, TGroupData> nodeToAdd)
        {
            if (!nodes.Contains(nodeToAdd))
            {
                nodes.Add(nodeToAdd);
                foreach (KeyValuePair<long, Node<TNode, TGroupData>> pair in nodeToAdd.m_children)
                {
                    this.AddNeighbours(nodes, pair.Value);
                }
                foreach (KeyValuePair<long, Node<TNode, TGroupData>> pair2 in nodeToAdd.m_parents)
                {
                    this.AddNeighbours(nodes, pair2.Value);
                }
            }
        }

        public override void AddNode(TNode nodeToAdd)
        {
            if (!this.SupportsOphrans)
            {
                throw new InvalidOperationException("Cannot add/remove node when ophrans are not supported");
            }
            Node<TNode, TGroupData> orCreateNode = this.GetOrCreateNode(nodeToAdd);
            if (orCreateNode.m_group == null)
            {
                orCreateNode.m_group = this.AcquireGroup();
                orCreateNode.m_group.m_members.Add(orCreateNode);
            }
        }

        public void ApplyOnNodes(Action<TNode, Node<TNode, TGroupData>> action)
        {
            foreach (KeyValuePair<TNode, Node<TNode, TGroupData>> pair in this.m_nodes)
            {
                action(pair.Key, pair.Value);
            }
        }

        private void BreakAllLinks(Node<TNode, TGroupData> node)
        {
            while (node.m_parents.Count > 0)
            {
                Dictionary<long, Node<TNode, TGroupData>>.Enumerator enumerator = node.m_parents.GetEnumerator();
                enumerator.MoveNext();
                KeyValuePair<long, Node<TNode, TGroupData>> current = enumerator.Current;
                this.BreakLinkInternal(current.Key, current.Value, node);
            }
            while (node.m_children.Count > 0)
            {
                SortedDictionary<long, Node<TNode, TGroupData>>.Enumerator enumerator = node.m_children.GetEnumerator();
                enumerator.MoveNext();
                KeyValuePair<long, Node<TNode, TGroupData>> current = enumerator.Current;
                this.BreakLinkInternal(current.Key, node, current.Value);
            }
        }

        public void BreakAllLinks(TNode node)
        {
            Node<TNode, TGroupData> node2;
            if (this.m_nodes.TryGetValue(node, out node2))
            {
                this.BreakAllLinks(node2);
            }
        }

        public override bool BreakLink(long linkId, TNode parentNode, TNode childNode = null)
        {
            Node<TNode, TGroupData> node;
            Node<TNode, TGroupData> node2;
            return (this.m_nodes.TryGetValue(parentNode, out node) && (node.m_children.TryGetValue(linkId, out node2) && this.BreakLinkInternal(linkId, node, node2)));
        }

        private bool BreakLinkInternal(long linkId, Node<TNode, TGroupData> parent, Node<TNode, TGroupData> child)
        {
            bool flag = parent.m_children.Remove(linkId) & child.m_parents.Remove(linkId);
            if (!flag && this.SupportsChildToChild)
            {
                flag &= child.m_children.Remove(linkId);
            }
            this.RecalculateConnectivity(parent, child);
            return flag;
        }

        public override void CreateLink(long linkId, TNode parentNode, TNode childNode)
        {
            Node<TNode, TGroupData> orCreateNode = this.GetOrCreateNode(parentNode);
            Node<TNode, TGroupData> child = this.GetOrCreateNode(childNode);
            if ((orCreateNode.m_group != null) && (child.m_group != null))
            {
                if (ReferenceEquals(orCreateNode.m_group, child.m_group))
                {
                    this.AddLink(linkId, orCreateNode, child);
                }
                else
                {
                    this.MergeGroups(orCreateNode.m_group, child.m_group);
                    this.AddLink(linkId, orCreateNode, child);
                }
            }
            else if (orCreateNode.m_group != null)
            {
                child.m_group = orCreateNode.m_group;
                orCreateNode.m_group.m_members.Add(child);
                this.AddLink(linkId, orCreateNode, child);
            }
            else if (child.m_group != null)
            {
                orCreateNode.m_group = child.m_group;
                child.m_group.m_members.Add(orCreateNode);
                this.AddLink(linkId, orCreateNode, child);
            }
            else
            {
                Group<TNode, TGroupData> group = this.AcquireGroup();
                orCreateNode.m_group = group;
                group.m_members.Add(orCreateNode);
                child.m_group = group;
                group.m_members.Add(child);
                this.AddLink(linkId, orCreateNode, child);
            }
        }

        [Conditional("DEBUG")]
        private void DebugCheckConsistency(long linkId, Node<TNode, TGroupData> parent, Node<TNode, TGroupData> child)
        {
        }

        public Group<TNode, TGroupData> GetGroup(TNode Node)
        {
            MyGroups<TNode, TGroupData>.Node node;
            return (!this.m_nodes.TryGetValue(Node, out node) ? null : node.m_group);
        }

        public override List<TNode> GetGroupNodes(TNode nodeInGroup)
        {
            List<TNode> list = null;
            Group<TNode, TGroupData> group = this.GetGroup(nodeInGroup);
            if (group == null)
            {
                return new List<TNode>(1) { nodeInGroup };
            }
            list = new List<TNode>(group.Nodes.Count);
            foreach (Node<TNode, TGroupData> node in group.Nodes)
            {
                list.Add(node.NodeData);
            }
            return list;
        }

        public override void GetGroupNodes(TNode nodeInGroup, List<TNode> result)
        {
            Group<TNode, TGroupData> group = this.GetGroup(nodeInGroup);
            if (group != null)
            {
                foreach (Node<TNode, TGroupData> node in group.Nodes)
                {
                    result.Add(node.NodeData);
                }
            }
            else
            {
                result.Add(nodeInGroup);
            }
        }

        public Node<TNode, TGroupData> GetNode(TNode node) => 
            this.m_nodes.GetValueOrDefault<TNode, Node<TNode, TGroupData>>(node);

        private Group<TNode, TGroupData> GetNodeOrNull(TNode Node)
        {
            MyGroups<TNode, TGroupData>.Node node;
            this.m_nodes.TryGetValue(Node, out node);
            return node?.m_group;
        }

        private Node<TNode, TGroupData> GetOrCreateNode(TNode nodeData)
        {
            Node<TNode, TGroupData> node;
            if (!this.m_nodes.TryGetValue(nodeData, out node))
            {
                node = this.AcquireNode();
                node.m_node = nodeData;
                this.m_nodes[nodeData] = node;
            }
            return node;
        }

        public override bool HasSameGroup(TNode a, TNode b)
        {
            Group<TNode, TGroupData> objA = this.GetGroup(a);
            Group<TNode, TGroupData> group = this.GetGroup(b);
            return ((objA != null) && ReferenceEquals(objA, group));
        }

        public static bool IsMajorGroup(Group<TNode, TGroupData> groupA, Group<TNode, TGroupData> groupB) => 
            (groupA.m_members.Count >= groupB.m_members.Count);

        public override bool LinkExists(long linkId, TNode parentNode, TNode childNode = null)
        {
            Node<TNode, TGroupData> node;
            Node<TNode, TGroupData> node2;
            return (this.m_nodes.TryGetValue(parentNode, out node) && (node.m_children.TryGetValue(linkId, out node2) && ((childNode != null) ? (childNode == node2.m_node) : true)));
        }

        private void MergeGroups(Group<TNode, TGroupData> groupA, Group<TNode, TGroupData> groupB)
        {
            if (!this.m_groupSelector(groupA, groupB))
            {
                Group<TNode, TGroupData> group = groupA;
                groupA = groupB;
                groupB = group;
            }
            if (this.m_tmpList.Capacity < groupB.m_members.Count)
            {
                this.m_tmpList.Capacity = groupB.m_members.Count;
            }
            this.m_tmpList.AddHashset<Node<TNode, TGroupData>>(groupB.m_members);
            foreach (Node<TNode, TGroupData> node in this.m_tmpList)
            {
                groupB.m_members.Remove(node);
                node.m_group = groupA;
                groupA.m_members.Add(node);
            }
            this.m_tmpList.Clear();
            groupB.m_members.Clear();
            this.ReturnGroup(groupB);
        }

        private void RecalculateConnectivity(Node<TNode, TGroupData> parent, Node<TNode, TGroupData> child)
        {
            if (!this.m_isRecalculating && (((parent != null) && ((parent.Group != null) && (child != null))) && (child.Group != null)))
            {
                try
                {
                    this.m_isRecalculating = true;
                    if (this.SupportsOphrans || (!this.TryReleaseNode(parent) & !this.TryReleaseNode(child)))
                    {
                        this.AddNeighbours(this.m_disconnectHelper, parent);
                        if (!this.m_disconnectHelper.Contains(child))
                        {
                            if (this.m_disconnectHelper.Count > (((float) parent.Group.m_members.Count) / 2f))
                            {
                                foreach (Node<TNode, TGroupData> node in parent.Group.m_members)
                                {
                                    if (!this.m_disconnectHelper.Add(node))
                                    {
                                        this.m_disconnectHelper.Remove(node);
                                    }
                                }
                            }
                            Group<TNode, TGroupData> group = this.AcquireGroup();
                            foreach (Node<TNode, TGroupData> node2 in this.m_disconnectHelper)
                            {
                                if (node2.m_group == null)
                                {
                                    continue;
                                }
                                if (node2.m_group.m_members != null)
                                {
                                    bool flag = node2.m_group.m_members.Remove(node2);
                                    node2.m_group = group;
                                    group.m_members.Add(node2);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    this.m_disconnectHelper.Clear();
                    this.m_isRecalculating = false;
                }
            }
        }

        public override void RemoveNode(TNode nodeToRemove)
        {
            Node<TNode, TGroupData> node;
            if (!this.SupportsOphrans)
            {
                throw new InvalidOperationException("Cannot add/remove node when ophrans are not supported");
            }
            if (this.m_nodes.TryGetValue(nodeToRemove, out node))
            {
                this.BreakAllLinks(node);
                bool flag = this.TryReleaseNode(node);
            }
        }

        private void ReplaceParents(Node<TNode, TGroupData> newParent)
        {
            this.m_tmpOpen.Enqueue(newParent);
            this.m_tmpClosed.Add(newParent);
            while (this.m_tmpOpen.Count > 0)
            {
                Node<TNode, TGroupData> node = this.m_tmpOpen.Dequeue();
                foreach (KeyValuePair<long, Node<TNode, TGroupData>> pair in node.m_children)
                {
                    pair.Value.ChainLength = node.ChainLength + 1;
                    if (!this.m_tmpClosed.Contains(pair.Value) && !pair.Value.m_parents.ContainsKey(pair.Key))
                    {
                        pair.Value.m_parents.Add(pair.Key, node);
                        pair.Value.m_children.Remove(pair.Key);
                        this.m_tmpOpen.Enqueue(pair.Value);
                        this.m_tmpClosed.Add(pair.Value);
                    }
                }
            }
            this.m_tmpOpen.Clear();
            this.m_tmpClosed.Clear();
        }

        public void ReplaceRoot(TNode newRoot)
        {
            foreach (Node<TNode, TGroupData> node2 in this.GetGroup(newRoot).m_members)
            {
                foreach (KeyValuePair<long, Node<TNode, TGroupData>> pair in node2.m_parents)
                {
                    node2.m_children[pair.Key] = pair.Value;
                }
                node2.m_parents.Clear();
            }
            Node<TNode, TGroupData> newParent = this.GetNode(newRoot);
            newParent.ChainLength = 0;
            this.ReplaceParents(newParent);
        }

        private void ReturnGroup(Group<TNode, TGroupData> group)
        {
            group.GroupData.OnRelease();
            this.m_groups.Remove(group);
            this.m_groupPool.Push(group);
        }

        private void ReturnNode(Node<TNode, TGroupData> node)
        {
            this.m_nodePool.Push(node);
        }

        private bool TryReleaseNode(Node<TNode, TGroupData> node)
        {
            if (((node.m_node == null) || ((node.m_group == null) || (node.m_children.Count != 0))) || (node.m_parents.Count != 0))
            {
                return false;
            }
            Group<TNode, TGroupData> group = node.m_group;
            node.m_group.m_members.Remove(node);
            this.m_nodes.Remove(node.m_node);
            node.m_group = null;
            node.m_node = default(TNode);
            this.ReturnNode(node);
            if (group.m_members.Count == 0)
            {
                this.ReturnGroup(group);
            }
            return true;
        }

        public bool SupportsOphrans { get; protected set; }

        protected bool SupportsChildToChild { get; set; }

        public HashSetReader<Group<TNode, TGroupData>> Groups =>
            new HashSetReader<Group<TNode, TGroupData>>(this.m_groups);

        public class Group
        {
            internal readonly HashSet<MyGroups<TNode, TGroupData>.Node> m_members;
            public readonly TGroupData GroupData;

            public Group()
            {
                this.m_members = new HashSet<MyGroups<TNode, TGroupData>.Node>();
                this.GroupData = Activator.CreateInstance<TGroupData>();
            }

            public HashSetReader<MyGroups<TNode, TGroupData>.Node> Nodes =>
                new HashSetReader<MyGroups<TNode, TGroupData>.Node>(this.m_members);
        }

        public delegate bool MajorGroupComparer(MyGroups<TNode, TGroupData>.Group major, MyGroups<TNode, TGroupData>.Group minor);

        public class Node
        {
            private MyGroups<TNode, TGroupData>.Group m_currentGroup;
            internal TNode m_node;
            internal readonly SortedDictionary<long, MyGroups<TNode, TGroupData>.Node> m_children;
            internal readonly Dictionary<long, MyGroups<TNode, TGroupData>.Node> m_parents;

            public Node()
            {
                this.m_children = new SortedDictionary<long, MyGroups<TNode, TGroupData>.Node>();
                this.m_parents = new Dictionary<long, MyGroups<TNode, TGroupData>.Node>();
            }

            public override string ToString() => 
                this.m_node.ToString();

            internal MyGroups<TNode, TGroupData>.Group m_group
            {
                get => 
                    this.m_currentGroup;
                set
                {
                    MyGroups<TNode, TGroupData>.Group currentGroup = this.m_currentGroup;
                    this.m_currentGroup = null;
                    if (currentGroup != null)
                    {
                        currentGroup.GroupData.OnNodeRemoved(this.m_node);
                    }
                    this.m_currentGroup = value;
                    if (this.m_currentGroup != null)
                    {
                        this.m_currentGroup.GroupData.OnNodeAdded(this.m_node);
                    }
                }
            }

            public int LinkCount =>
                (this.m_children.Count + this.m_parents.Count);

            public TNode NodeData =>
                this.m_node;

            public MyGroups<TNode, TGroupData>.Group Group =>
                this.m_group;

            public int ChainLength { get; set; }

            public SortedDictionaryValuesReader<long, MyGroups<TNode, TGroupData>.Node> Children =>
                new SortedDictionaryValuesReader<long, MyGroups<TNode, TGroupData>.Node>(this.m_children);

            public SortedDictionaryReader<long, MyGroups<TNode, TGroupData>.Node> ChildLinks =>
                new SortedDictionaryReader<long, MyGroups<TNode, TGroupData>.Node>(this.m_children);

            public DictionaryReader<long, MyGroups<TNode, TGroupData>.Node> ParentLinks =>
                new DictionaryReader<long, MyGroups<TNode, TGroupData>.Node>(this.m_parents);
        }
    }
}

