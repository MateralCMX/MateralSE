namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Weapons;
    using System;

    public class MyDebugRenderCompomentDrawDrillBase : MyDebugRenderComponent
    {
        private MyDrillBase m_drillBase;

        public MyDebugRenderCompomentDrawDrillBase(MyDrillBase drillBase) : base(null)
        {
            this.m_drillBase = drillBase;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_DRILLS)
            {
                this.m_drillBase.DebugDraw();
            }
        }
    }
}

