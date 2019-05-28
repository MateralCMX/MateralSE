namespace Sandbox.Engine.Physics
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Groups;

    internal class MySharedTensorData : IGroupData<MyCubeGrid>
    {
        public void MarkDirty()
        {
            using (HashSet<MyGroups<MyCubeGrid, MySharedTensorData>.Node>.Enumerator enumerator = this.m_group.Nodes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MarkGridTensorDirty(enumerator.Current.NodeData);
                }
            }
        }

        public static void MarkGridTensorDirty(MyCubeGrid grid)
        {
            MyGridPhysics physics = grid.Physics;
            if (physics != null)
            {
                physics.Shape.MarkSharedTensorDirty();
            }
        }

        public void OnCreate<TGroupData>(MyGroups<MyCubeGrid, TGroupData>.Group group) where TGroupData: IGroupData<MyCubeGrid>, new()
        {
            this.m_group = group as MyGroups<MyCubeGrid, MySharedTensorData>.Group;
        }

        public void OnNodeAdded(MyCubeGrid grid)
        {
            this.MarkDirty();
            MarkGridTensorDirty(grid);
        }

        public void OnNodeRemoved(MyCubeGrid grid)
        {
            this.MarkDirty();
            MarkGridTensorDirty(grid);
        }

        public void OnRelease()
        {
            this.m_group = null;
        }

        public MyGroups<MyCubeGrid, MySharedTensorData>.Group m_group { get; set; }
    }
}

