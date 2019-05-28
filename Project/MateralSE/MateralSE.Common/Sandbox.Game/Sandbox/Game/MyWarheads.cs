namespace Sandbox.Game
{
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    internal class MyWarheads : MySessionComponentBase
    {
        private static readonly HashSet<MyWarhead> m_warheads = new HashSet<MyWarhead>();
        private static readonly List<MyWarhead> m_warheadsToExplode = new List<MyWarhead>();
        public static List<BoundingSphere> DebugWarheadShrinks = new List<BoundingSphere>();
        public static List<BoundingSphere> DebugWarheadGroupSpheres = new List<BoundingSphere>();

        public static void AddWarhead(MyWarhead warhead)
        {
            if (m_warheads.Add(warhead))
            {
                warhead.OnMarkForClose += new Action<VRage.Game.Entity.MyEntity>(MyWarheads.warhead_OnClose);
            }
        }

        public override void BeforeStart()
        {
            base.BeforeStart();
        }

        public static bool Contains(MyWarhead warhead) => 
            m_warheads.Contains(warhead);

        public override void Draw()
        {
            base.Draw();
            foreach (BoundingSphere sphere in DebugWarheadShrinks)
            {
                MyRenderProxy.DebugDrawSphere(sphere.Center, sphere.Radius, Color.Blue, 1f, false, false, true, false);
            }
            foreach (BoundingSphere sphere2 in DebugWarheadGroupSpheres)
            {
                MyRenderProxy.DebugDrawSphere(sphere2.Center, sphere2.Radius, Color.Yellow, 1f, false, false, true, false);
            }
        }

        public static void RemoveWarhead(MyWarhead warhead)
        {
            if (m_warheads.Remove(warhead))
            {
                warhead.OnMarkForClose -= new Action<VRage.Game.Entity.MyEntity>(MyWarheads.warhead_OnClose);
            }
        }

        protected override void UnloadData()
        {
            m_warheads.Clear();
            m_warheadsToExplode.Clear();
            DebugWarheadShrinks.Clear();
            DebugWarheadGroupSpheres.Clear();
        }

        public override void UpdateBeforeSimulation()
        {
            int frameMs = 0x10;
            foreach (MyWarhead warhead in m_warheads)
            {
                if (!warhead.Countdown(frameMs))
                {
                    continue;
                }
                warhead.RemainingMS -= frameMs;
                if (warhead.RemainingMS <= 0)
                {
                    m_warheadsToExplode.Add(warhead);
                }
            }
            foreach (MyWarhead warhead2 in m_warheadsToExplode)
            {
                RemoveWarhead(warhead2);
                if (Sync.IsServer)
                {
                    warhead2.Explode();
                }
            }
            m_warheadsToExplode.Clear();
        }

        private static void warhead_OnClose(VRage.Game.Entity.MyEntity obj)
        {
            m_warheads.Remove(obj as MyWarhead);
        }
    }
}

