namespace Sandbox.Engine.Physics
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.Groups;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGridPhysicalHierarchy : MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>, IMySceneComponent
    {
        public static MyGridPhysicalHierarchy Static;
        private readonly Dictionary<long, HashSet<MyEntity>> m_nonGridChildren;

        public MyGridPhysicalHierarchy() : base(false, null)
        {
            this.m_nonGridChildren = new Dictionary<long, HashSet<MyEntity>>();
        }

        public override void AddNode(MyCubeGrid nodeToAdd)
        {
            base.AddNode(nodeToAdd);
            this.UpdateRoot(nodeToAdd);
        }

        public void AddNonGridNode(MyCubeGrid parent, MyEntity entity)
        {
            if (base.GetGroup(parent) != null)
            {
                HashSet<MyEntity> set;
                if (!this.m_nonGridChildren.TryGetValue(parent.EntityId, out set))
                {
                    set = new HashSet<MyEntity>();
                    this.m_nonGridChildren.Add(parent.EntityId, set);
                    parent.OnClose += new Action<MyEntity>(this.RemoveAllNonGridNodes);
                }
                set.Add(entity);
            }
        }

        public void ApplyOnAllChildren(MyEntity entity, Action<MyEntity> action)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid != null)
            {
                HashSet<MyEntity> set;
                MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node = base.GetNode(grid);
                if ((node != null) && (node.Children.Count > 0))
                {
                    foreach (KeyValuePair<long, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node> pair in node.ChildLinks)
                    {
                        if (this.GetParentLinkId(pair.Value) == pair.Key)
                        {
                            action(pair.Value.NodeData);
                        }
                    }
                }
                if ((node != null) && this.m_nonGridChildren.TryGetValue(grid.EntityId, out set))
                {
                    foreach (MyEntity entity2 in set)
                    {
                        action(entity2);
                    }
                }
            }
        }

        public void ApplyOnChildren(MyCubeGrid grid, Action<MyCubeGrid> action)
        {
            MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node = base.GetNode(grid);
            if ((node != null) && (node.Children.Count > 0))
            {
                foreach (KeyValuePair<long, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node> pair in node.ChildLinks)
                {
                    if (this.GetParentLinkId(pair.Value) == pair.Key)
                    {
                        action(pair.Value.NodeData);
                    }
                }
            }
        }

        public override bool BreakLink(long linkId, MyCubeGrid parentNode, MyCubeGrid childNode = null)
        {
            if (childNode == null)
            {
                childNode = base.GetNode(parentNode).m_children[linkId].NodeData;
            }
            bool flag = base.BreakLink(linkId, parentNode, childNode);
            if (!flag)
            {
                flag = base.BreakLink(linkId, childNode, parentNode);
            }
            if (flag)
            {
                this.UpdateRoot(parentNode);
                if (!ReferenceEquals(base.GetGroup(parentNode), base.GetGroup(childNode)))
                {
                    this.UpdateRoot(childNode);
                }
            }
            return flag;
        }

        private MyCubeGrid CalculateNewRoot(MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Group group)
        {
            if (group.m_members.Count == 1)
            {
                return group.m_members.FirstElement<MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>().NodeData;
            }
            MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node = null;
            float num = 0f;
            List<MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node> list = new List<MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>();
            if (group.m_members.Count == 1)
            {
                return group.m_members.FirstElement<MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>().NodeData;
            }
            bool flag = false;
            foreach (MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node3 in group.Nodes)
            {
                if (node3.NodeData.IsStatic || MyFixedGrids.IsRooted(node3.NodeData))
                {
                    if (!flag)
                    {
                        list.Clear();
                        node = null;
                        flag = true;
                    }
                    list.Add(node3);
                }
                if (!flag)
                {
                    if (this.IsGridControlled(node3.NodeData))
                    {
                        node = node3;
                    }
                    if (node3.NodeData.Physics != null)
                    {
                        float mass = 0f;
                        HkMassProperties? massProperties = node3.NodeData.Physics.Shape.MassProperties;
                        if (massProperties != null)
                        {
                            mass = massProperties.Value.Mass;
                        }
                        if (mass > num)
                        {
                            num = mass;
                            list.Clear();
                            list.Add(node3);
                        }
                        else if (mass == num)
                        {
                            list.Add(node3);
                        }
                    }
                }
            }
            MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node2 = null;
            if (list.Count == 1)
            {
                node2 = list[0];
            }
            else if (list.Count > 1)
            {
                long entityId = list[0].NodeData.EntityId;
                MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node4 = list[0];
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node5 in list)
                {
                    if (!MyWeldingGroups.Static.IsEntityParent(node5.NodeData))
                    {
                        continue;
                    }
                    if (entityId > node5.NodeData.EntityId)
                    {
                        entityId = node5.NodeData.EntityId;
                        node4 = node5;
                    }
                }
                node2 = node4;
            }
            if (node != null)
            {
                if (node2 == null)
                {
                    node2 = node;
                }
                else if ((node.NodeData.Physics == null) || (node2.NodeData.Physics == null))
                {
                    node2 = node;
                }
                else
                {
                    float mass = 0f;
                    HkMassProperties? massProperties = node.NodeData.Physics.Shape.MassProperties;
                    if (massProperties != null)
                    {
                        mass = massProperties.Value.Mass;
                    }
                    float mass = 0f;
                    massProperties = node2.NodeData.Physics.Shape.MassProperties;
                    if (massProperties != null)
                    {
                        mass = massProperties.Value.Mass;
                    }
                    if ((mass / mass) < 2f)
                    {
                        node2 = node;
                    }
                }
            }
            return node2?.NodeData;
        }

        public override void CreateLink(long linkId, MyCubeGrid parentNode, MyCubeGrid childNode)
        {
            base.CreateLink(linkId, parentNode, childNode);
            this.UpdateRoot(parentNode);
        }

        public void Draw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_GRID_HIERARCHY)
            {
                base.ApplyOnNodes(new Action<MyCubeGrid, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>(this.DrawNode));
            }
        }

        private void DrawNode(MyCubeGrid grid, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node)
        {
            if (node.m_parents.Count <= 0)
            {
                MyRenderProxy.DebugDrawAxis(grid.PositionComp.WorldMatrix, 1f, false, false, false);
            }
            else
            {
                Color? colorTo = null;
                MyRenderProxy.DebugDrawArrow3D(grid.PositionComp.GetPosition(), node.m_parents.FirstPair<long, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>().Value.NodeData.PositionComp.GetPosition(), Color.Orange, colorTo, false, 0.1, null, 0.5f, false);
            }
        }

        public MyEntity GetEntityConnectingToParent(MyCubeGrid grid)
        {
            MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node = base.GetNode(grid);
            if (node == null)
            {
                return null;
            }
            if (node.m_parents.Count == 0)
            {
                return null;
            }
            return MyEntities.GetEntityById(node.m_parents.FirstPair<long, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>().Key, false);
        }

        public int GetNodeChainLength(MyCubeGrid grid)
        {
            MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node = base.GetNode(grid);
            return ((node == null) ? 0 : node.ChainLength);
        }

        public MyCubeGrid GetParent(MyCubeGrid grid)
        {
            MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node = base.GetNode(grid);
            return ((node != null) ? this.GetParent(node) : null);
        }

        public MyCubeGrid GetParent(MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node)
        {
            if (node.m_parents.Count == 0)
            {
                return null;
            }
            return node.m_parents.FirstPair<long, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>().Value.NodeData;
        }

        public long GetParentLinkId(MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node)
        {
            if (node.m_parents.Count == 0)
            {
                return 0L;
            }
            return node.m_parents.FirstPair<long, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>().Key;
        }

        public Vector3? GetPivot(MyCubeGrid grid, bool parent = false)
        {
            MyMechanicalConnectionBlockBase entityConnectingToParent = this.GetEntityConnectingToParent(grid) as MyMechanicalConnectionBlockBase;
            if (entityConnectingToParent != null)
            {
                return entityConnectingToParent.GetConstraintPosition(grid, parent);
            }
            return null;
        }

        public MyCubeGrid GetRoot(MyCubeGrid grid)
        {
            MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Group group = base.GetGroup(grid);
            if (group == null)
            {
                return grid;
            }
            MyCubeGrid root = group.GroupData.m_root;
            if (root == null)
            {
                root = grid;
            }
            return root;
        }

        public bool HasChildren(MyCubeGrid grid)
        {
            MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node = base.GetNode(grid);
            return ((node != null) && (node.Children.Count > 0));
        }

        public bool InSameHierarchy(MyCubeGrid first, MyCubeGrid second) => 
            ReferenceEquals(this.GetRoot(first), this.GetRoot(second));

        public bool IsChildOf(MyCubeGrid parentGrid, MyEntity entity)
        {
            HashSet<MyEntity> set;
            MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node = base.GetNode(parentGrid);
            if ((node != null) && (node.Children.Count > 0))
            {
                using (SortedDictionary<long, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>.Enumerator enumerator = node.ChildLinks.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<long, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node> current = enumerator.Current;
                        if ((this.GetParentLinkId(current.Value) == current.Key) && (current.Value.NodeData == entity))
                        {
                            return true;
                        }
                    }
                }
            }
            return ((node != null) && (this.m_nonGridChildren.TryGetValue(parentGrid.EntityId, out set) && set.Contains(entity)));
        }

        public bool IsCyclic(MyCubeGrid grid)
        {
            MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node node = base.GetNode(grid);
            if ((node != null) && (node.Children.Count > 0))
            {
                using (SortedDictionary<long, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>.Enumerator enumerator = node.ChildLinks.GetEnumerator())
                {
                    while (true)
                    {
                        bool flag;
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<long, MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node> current = enumerator.Current;
                        if (this.GetParentLinkId(current.Value) != current.Key)
                        {
                            flag = true;
                        }
                        else
                        {
                            if (!this.IsCyclic(current.Value.NodeData))
                            {
                                continue;
                            }
                            flag = true;
                        }
                        return flag;
                    }
                }
            }
            return false;
        }

        public bool IsEntityParent(MyEntity entity)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            return ((grid != null) ? ReferenceEquals(this.GetParent(grid), null) : true);
        }

        private bool IsGridControlled(MyCubeGrid grid)
        {
            MyShipController shipController = grid.GridSystems.ControlSystem.GetShipController();
            return ((shipController != null) && ReferenceEquals(shipController.CubeGrid, grid));
        }

        public void Load()
        {
            Static = this;
            base.SupportsOphrans = true;
            base.SupportsChildToChild = true;
        }

        public void Log(MyCubeGrid grid)
        {
            string text1;
            MyLog.Default.IncreaseIndent();
            object[] objArray1 = new object[6];
            objArray1[0] = grid.EntityId;
            objArray1[1] = grid.DisplayName;
            objArray1[2] = grid.Physics != null;
            object[] objArray2 = objArray1;
            if ((grid.Physics == null) || (grid.Physics.Shape.MassProperties == null))
            {
                text1 = "None";
            }
            else
            {
                text1 = grid.Physics.Shape.MassProperties.Value.Mass.ToString();
            }
            objArray2[3] = text1;
            object[] args = objArray2;
            args[4] = grid.IsStatic;
            args[5] = this.IsGridControlled(grid);
            MyLog.Default.WriteLine(string.Format("{0}: name={1} physics={2} mass={3} static={4} controlled={5}", args));
            this.ApplyOnChildren(grid, new Action<MyCubeGrid>(this.Log));
            MyLog.Default.DecreaseIndent();
        }

        public bool NonGridLinkExists(long parentId, MyEntity child)
        {
            HashSet<MyEntity> set;
            return (this.m_nonGridChildren.TryGetValue(parentId, out set) && set.Contains(child));
        }

        private void RemoveAllNonGridNodes(MyEntity parent)
        {
            this.m_nonGridChildren.Remove(parent.EntityId);
            parent.OnClose -= new Action<MyEntity>(this.RemoveAllNonGridNodes);
        }

        public void RemoveNonGridNode(MyCubeGrid parent, MyEntity entity)
        {
            HashSet<MyEntity> set;
            if ((base.GetGroup(parent) != null) && this.m_nonGridChildren.TryGetValue(parent.EntityId, out set))
            {
                set.Remove(entity);
                if (set.Count == 0)
                {
                    this.m_nonGridChildren.Remove(parent.EntityId);
                    parent.OnClose -= new Action<MyEntity>(this.RemoveAllNonGridNodes);
                }
            }
        }

        public void Unload()
        {
            Static = null;
        }

        public void UpdateRoot(MyCubeGrid node)
        {
            if (!MyEntities.IsClosingAll)
            {
                MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Group group = base.GetGroup(node);
                if (group != null)
                {
                    MyCubeGrid newRoot = this.CalculateNewRoot(group);
                    group.GroupData.m_root = newRoot;
                    if (newRoot != null)
                    {
                        base.ReplaceRoot(newRoot);
                        using (HashSet<MyGroups<MyCubeGrid, MyGridPhysicalHierarchyData>.Node>.Enumerator enumerator = group.Nodes.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                enumerator.Current.NodeData.HierarchyUpdated(newRoot);
                            }
                        }
                    }
                }
            }
        }
    }
}

