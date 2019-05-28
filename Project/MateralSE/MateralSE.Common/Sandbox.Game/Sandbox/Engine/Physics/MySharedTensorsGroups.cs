namespace Sandbox.Engine.Physics
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using VRage.Collections;
    using VRage.Groups;

    internal class MySharedTensorsGroups : MyGroups<MyCubeGrid, MySharedTensorData>, IMySceneComponent
    {
        private static MySharedTensorsGroups m_static;

        public MySharedTensorsGroups() : base(false, null)
        {
        }

        [DebuggerStepThrough, Conditional("DEBUG")]
        private static void AssertThread()
        {
            Thread currentThread = Thread.CurrentThread;
            Thread updateThread = MySandboxGame.Static.UpdateThread;
        }

        public static bool BreakLinkIfExists(MyCubeGrid parent, MyCubeGrid child, MyCubeBlock linkingBlock) => 
            Static.BreakLink(linkingBlock.EntityId, parent, child);

        public static HashSetReader<MyGroups<MyCubeGrid, MySharedTensorData>.Node> GetGridsInSameGroup(MyCubeGrid groupRepresentative)
        {
            MyGroups<MyCubeGrid, MySharedTensorData>.Group group = Static.GetGroup(groupRepresentative);
            if (group != null)
            {
                return group.Nodes;
            }
            return new HashSetReader<MyGroups<MyCubeGrid, MySharedTensorData>.Node>();
        }

        public static void Link(MyCubeGrid parent, MyCubeGrid child, MyCubeBlock linkingBlock)
        {
            Static.CreateLink(linkingBlock.EntityId, parent, child);
        }

        public void Load()
        {
            m_static = this;
        }

        public static void MarkGroupDirty(MyCubeGrid grid)
        {
            MyGroups<MyCubeGrid, MySharedTensorData>.Group group = Static.GetGroup(grid);
            if (group != null)
            {
                group.GroupData.MarkDirty();
            }
        }

        public void Unload()
        {
            m_static = null;
        }

        private static MySharedTensorsGroups Static =>
            m_static;
    }
}

