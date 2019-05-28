namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyUnsafeGridsSessionComponent : MySessionComponentBase
    {
        private static MyUnsafeGridsSessionComponent m_static;
        public Dictionary<long, MyCubeGrid> m_UnsafeGrids;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            this.m_UnsafeGrids = new Dictionary<long, MyCubeGrid>();
            m_static = this;
        }

        public static void OnGridChanged(MyCubeGrid grid)
        {
            RequestWarningUpdate();
        }

        public static void RegisterGrid(MyCubeGrid grid)
        {
            if (!grid.IsPreview)
            {
                Static.m_UnsafeGrids[grid.EntityId] = grid;
                RequestWarningUpdate();
            }
        }

        private static void RequestWarningUpdate()
        {
            MySessionComponentWarningSystem @static = MySessionComponentWarningSystem.Static;
            if (@static != null)
            {
                @static.RequestUpdate();
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            this.m_UnsafeGrids = null;
            m_static = null;
        }

        public static void UnregisterGrid(MyCubeGrid grid)
        {
            Static.m_UnsafeGrids.Remove(grid.EntityId);
            RequestWarningUpdate();
        }

        public static MyUnsafeGridsSessionComponent Static =>
            m_static;

        public static DictionaryReader<long, MyCubeGrid> UnsafeGrids =>
            ((Static == null) ? ((DictionaryReader<long, MyCubeGrid>) 0) : ((DictionaryReader<long, MyCubeGrid>) Static.m_UnsafeGrids));

        public override bool IsRequiredByGame =>
            true;
    }
}

