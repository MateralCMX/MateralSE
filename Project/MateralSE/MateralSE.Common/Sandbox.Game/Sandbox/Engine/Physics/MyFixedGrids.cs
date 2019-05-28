namespace Sandbox.Engine.Physics
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using VRage.Groups;

    public class MyFixedGrids : MyGroups<MyCubeGrid, MyFixedGrids.MyFixedGridsGroupData>, IMySceneComponent
    {
        private static MyFixedGrids m_static;
        private HashSet<MyCubeGrid> m_roots;

        public MyFixedGrids() : base(false, new MyGroups<MyCubeGrid, MyFixedGridsGroupData>.MajorGroupComparer(MyFixedGridsGroupData.MajorSelector))
        {
            this.m_roots = new HashSet<MyCubeGrid>();
            base.SupportsChildToChild = true;
        }

        private static void AssertThread()
        {
            Thread currentThread = Thread.CurrentThread;
            Thread updateThread = MySandboxGame.Static.UpdateThread;
        }

        public static void BreakLink(MyCubeGrid parent, MyCubeGrid child, MyCubeBlock linkingBlock)
        {
            AssertThread();
            Static.BreakLink(linkingBlock.EntityId, parent, child);
        }

        public static bool IsRooted(MyCubeGrid grid)
        {
            if (!MyPhysics.InsideSimulation)
            {
                AssertThread();
            }
            if (Static.m_roots.Contains(grid))
            {
                return true;
            }
            MyGroups<MyCubeGrid, MyFixedGridsGroupData>.Group group = Static.GetGroup(grid);
            return ((group != null) ? group.GroupData.IsRooted : false);
        }

        public static void Link(MyCubeGrid parent, MyCubeGrid child, MyCubeBlock linkingBlock)
        {
            AssertThread();
            Static.CreateLink(linkingBlock.EntityId, parent, child);
        }

        public void Load()
        {
            m_static = this;
        }

        public static void MarkGridRoot(MyCubeGrid grid)
        {
            AssertThread();
            if (Static.m_roots.Add(grid))
            {
                MyGroups<MyCubeGrid, MyFixedGridsGroupData>.Group group = Static.GetGroup(grid);
                if (group == null)
                {
                    MyFixedGridsGroupData.ConvertGrid(grid, true);
                }
                else
                {
                    group.GroupData.OnRootAdded();
                }
            }
        }

        public void Unload()
        {
            m_static = null;
        }

        public static void UnmarkGridRoot(MyCubeGrid grid)
        {
            AssertThread();
            if (Static.m_roots.Remove(grid))
            {
                MyGroups<MyCubeGrid, MyFixedGridsGroupData>.Group group = Static.GetGroup(grid);
                if (group == null)
                {
                    MyFixedGridsGroupData.ConvertGrid(grid, false);
                }
                else
                {
                    group.GroupData.OnRootRemoved();
                }
            }
        }

        private static MyFixedGrids Static =>
            m_static;

        public class MyFixedGridsGroupData : IGroupData<MyCubeGrid>
        {
            private MyGroups<MyCubeGrid, MyFixedGrids.MyFixedGridsGroupData>.Group m_group;
            private int m_rootedGrids;

            private void Convert(bool @static)
            {
                using (HashSet<MyGroups<MyCubeGrid, MyFixedGrids.MyFixedGridsGroupData>.Node>.Enumerator enumerator = this.m_group.Nodes.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ConvertGrid(enumerator.Current.NodeData, @static);
                    }
                }
            }

            public static void ConvertGrid(MyCubeGrid grid, bool @static)
            {
                grid.IsMarkedForEarlyDeactivation = @static;
            }

            public static bool MajorSelector(MyGroups<MyCubeGrid, MyFixedGrids.MyFixedGridsGroupData>.Group major, MyGroups<MyCubeGrid, MyFixedGrids.MyFixedGridsGroupData>.Group minor)
            {
                bool flag = minor.GroupData.m_rootedGrids > 0;
                if (major.GroupData.m_rootedGrids > 0)
                {
                    if (!flag)
                    {
                        return true;
                    }
                }
                else if (flag)
                {
                    return false;
                }
                return (major.Nodes.Count >= minor.Nodes.Count);
            }

            public void OnCreate<TGroupData>(MyGroups<MyCubeGrid, TGroupData>.Group group) where TGroupData: IGroupData<MyCubeGrid>, new()
            {
                this.m_group = group as MyGroups<MyCubeGrid, MyFixedGrids.MyFixedGridsGroupData>.Group;
            }

            public void OnNodeAdded(MyCubeGrid grid)
            {
                bool flag = false;
                if (MyFixedGrids.Static.m_roots.Contains(grid))
                {
                    this.OnRootAdded();
                    flag = true;
                }
                if (flag | (this.m_rootedGrids != 0))
                {
                    ConvertGrid(grid, true);
                }
            }

            public void OnNodeRemoved(MyCubeGrid grid)
            {
                if (MyFixedGrids.Static.m_roots.Contains(grid))
                {
                    this.OnRootRemoved();
                }
                else if (this.m_rootedGrids != 0)
                {
                    ConvertGrid(grid, false);
                }
            }

            public void OnRelease()
            {
                this.m_group = null;
            }

            public void OnRootAdded()
            {
                int rootedGrids = this.m_rootedGrids;
                this.m_rootedGrids = rootedGrids + 1;
                if (rootedGrids == 0)
                {
                    this.Convert(true);
                }
            }

            public void OnRootRemoved()
            {
                int num = this.m_rootedGrids - 1;
                this.m_rootedGrids = num;
                if (num == 0)
                {
                    this.Convert(false);
                }
            }

            public bool IsRooted =>
                (this.m_rootedGrids > 0);
        }
    }
}

