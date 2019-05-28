namespace Sandbox.Engine.Physics
{
    using Havok;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    internal class DisableGridTOIsOptimizer : PhysicsStepOptimizerBase
    {
        public static DisableGridTOIsOptimizer Static;
        private HashSet<MyGridPhysics> m_optimizedGrids = new HashSet<MyGridPhysics>();

        public DisableGridTOIsOptimizer()
        {
            Static = this;
        }

        public void DebugDraw()
        {
            foreach (MyGridPhysics physics in Static.m_optimizedGrids)
            {
                MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(physics.Entity.LocalAABB, physics.Entity.WorldMatrix), Color.Yellow, 1f, false, false, false);
            }
        }

        public override void DisableOptimizations()
        {
            while (this.m_optimizedGrids.Count > 0)
            {
                this.m_optimizedGrids.FirstElement<MyGridPhysics>().DisableTOIOptimization();
            }
        }

        public override void EnableOptimizations(List<MyTuple<HkWorld, MyTimeSpan>> timings)
        {
            ForEverySignificantWorld(timings, world => ForEveryActivePhysicsBodyOfType<MyGridPhysics>(world, body => body.ConsiderDisablingTOIs()));
        }

        public void Register(MyGridPhysics grid)
        {
            this.m_optimizedGrids.Add(grid);
        }

        public override void Unload()
        {
            Static = null;
        }

        public void Unregister(MyGridPhysics grid)
        {
            this.m_optimizedGrids.Remove(grid);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DisableGridTOIsOptimizer.<>c <>9 = new DisableGridTOIsOptimizer.<>c();
            public static Action<MyGridPhysics> <>9__4_1;
            public static Action<HkWorld> <>9__4_0;

            internal void <EnableOptimizations>b__4_0(HkWorld world)
            {
                PhysicsStepOptimizerBase.ForEveryActivePhysicsBodyOfType<MyGridPhysics>(world, body => body.ConsiderDisablingTOIs());
            }

            internal void <EnableOptimizations>b__4_1(MyGridPhysics body)
            {
                body.ConsiderDisablingTOIs();
            }
        }
    }
}

