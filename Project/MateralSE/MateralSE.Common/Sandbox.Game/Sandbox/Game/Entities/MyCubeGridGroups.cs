namespace Sandbox.Game.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game.ModAPI;
    using VRage.Groups;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyCubeGridGroups : IMySceneComponent
    {
        public static MyCubeGridGroups Static;
        private MyGroupsBase<MyCubeGrid>[] m_groupsByType;
        public MyGroups<MyCubeGrid, MyGridLogicalGroupData> Logical = new MyGroups<MyCubeGrid, MyGridLogicalGroupData>(true, null);
        public MyGroups<MyCubeGrid, MyGridPhysicalGroupData> Physical = new MyGroups<MyCubeGrid, MyGridPhysicalGroupData>(true, new MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.MajorGroupComparer(MyGridPhysicalGroupData.IsMajorGroup));
        public MyGroups<MyCubeGrid, MyGridNoDamageGroupData> NoContactDamage = new MyGroups<MyCubeGrid, MyGridNoDamageGroupData>(true, null);
        public MyGroups<MyCubeGrid, MyGridMechanicalGroupData> Mechanical = new MyGroups<MyCubeGrid, MyGridMechanicalGroupData>(true, null);
        public MyGroups<MySlimBlock, MyBlockGroupData> SmallToLargeBlockConnections = new MyGroups<MySlimBlock, MyBlockGroupData>(false, null);
        public MyGroups<MyCubeGrid, MyGridPhysicalDynamicGroupData> PhysicalDynamic = new MyGroups<MyCubeGrid, MyGridPhysicalDynamicGroupData>(false, null);
        private static readonly HashSet<object> m_tmpBlocksDebugHelper = new HashSet<object>();

        public MyCubeGridGroups()
        {
            this.m_groupsByType = new MyGroupsBase<MyCubeGrid>[] { this.Logical, this.Physical, this.NoContactDamage, this.Mechanical };
        }

        public void AddNode(GridLinkTypeEnum type, MyCubeGrid grid)
        {
            this.GetGroups(type).AddNode(grid);
        }

        public bool BreakLink(GridLinkTypeEnum type, long linkId, MyCubeGrid parent, MyCubeGrid child = null)
        {
            if (type == GridLinkTypeEnum.Physical)
            {
                this.PhysicalDynamic.BreakLink(linkId, parent, child);
            }
            return this.GetGroups(type).BreakLink(linkId, parent, child);
        }

        public void CreateLink(GridLinkTypeEnum type, long linkId, MyCubeGrid parent, MyCubeGrid child)
        {
            this.GetGroups(type).CreateLink(linkId, parent, child);
            if (((type == GridLinkTypeEnum.Physical) && !parent.Physics.IsStatic) && !child.Physics.IsStatic)
            {
                this.PhysicalDynamic.CreateLink(linkId, parent, child);
            }
        }

        internal static void DebugDrawBlockGroups<TNode, TGroupData>(MyGroups<TNode, TGroupData> groups) where TNode: MySlimBlock where TGroupData: IGroupData<TNode>, new()
        {
            int num = 0;
            using (HashSet<MyGroups<TNode, TGroupData>.Group>.Enumerator enumerator = groups.Groups.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    num++;
                    Color colorFrom = new Vector3(((float) (num % 15)) / 15f, 1f, 1f).HSVtoColor();
                    HashSetReader<MyGroups<TNode, TGroupData>.Node> nodes = enumerator.Current.Nodes;
                    foreach (MyGroups<TNode, TGroupData>.Node node in nodes)
                    {
                        try
                        {
                            BoundingBoxD xd;
                            node.NodeData.GetWorldBoundingBox(out xd, false);
                            SortedDictionaryValuesReader<long, MyGroups<TNode, TGroupData>.Node> children = node.Children;
                            foreach (MyGroups<TNode, TGroupData>.Node node2 in children)
                            {
                                m_tmpBlocksDebugHelper.Add(node2);
                            }
                            foreach (object obj2 in m_tmpBlocksDebugHelper)
                            {
                                BoundingBoxD xd2;
                                MyGroups<TNode, TGroupData>.Node node3 = null;
                                int num2 = 0;
                                children = node.Children;
                                foreach (MyGroups<TNode, TGroupData>.Node node4 in children)
                                {
                                    if (obj2 == node4)
                                    {
                                        node3 = node4;
                                        num2++;
                                    }
                                }
                                node3.NodeData.GetWorldBoundingBox(out xd2, false);
                                MyRenderProxy.DebugDrawLine3D(xd.Center, xd2.Center, colorFrom, colorFrom, false, false);
                                MyRenderProxy.DebugDrawText3D((xd.Center + xd2.Center) * 0.5, num2.ToString(), colorFrom, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                            }
                            Color color = new Color(colorFrom.ToVector3() + 0.25f);
                            MyRenderProxy.DebugDrawSphere(xd.Center, 0.2f, color.ToVector3(), 0.5f, false, true, true, false);
                            MyRenderProxy.DebugDrawText3D(xd.Center, node.LinkCount.ToString(), color, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
                        }
                        finally
                        {
                            m_tmpBlocksDebugHelper.Clear();
                        }
                    }
                }
            }
        }

        public MyGroupsBase<MyCubeGrid> GetGroups(GridLinkTypeEnum type) => 
            this.m_groupsByType[(int) type];

        public void RemoveNode(GridLinkTypeEnum type, MyCubeGrid grid)
        {
            this.GetGroups(type).RemoveNode(grid);
        }

        void IMySceneComponent.Load()
        {
            Static = new MyCubeGridGroups();
        }

        void IMySceneComponent.Unload()
        {
            Static = null;
        }

        public void UpdateDynamicState(MyCubeGrid grid)
        {
            bool flag = this.PhysicalDynamic.GetGroup(grid) != null;
            bool flag2 = !grid.IsStatic;
            if (flag && !flag2)
            {
                this.PhysicalDynamic.BreakAllLinks(grid);
            }
            else if (!flag & flag2)
            {
                MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node node = this.Physical.GetNode(grid);
                if (node != null)
                {
                    foreach (KeyValuePair<long, MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node> pair in node.ChildLinks)
                    {
                        if (!pair.Value.NodeData.IsStatic)
                        {
                            this.PhysicalDynamic.CreateLink(pair.Key, grid, pair.Value.NodeData);
                        }
                    }
                    foreach (KeyValuePair<long, MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node> pair2 in node.ParentLinks)
                    {
                        if (!pair2.Value.NodeData.IsStatic)
                        {
                            this.PhysicalDynamic.CreateLink(pair2.Key, pair2.Value.NodeData, grid);
                        }
                    }
                }
            }
        }
    }
}

