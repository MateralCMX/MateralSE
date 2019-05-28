namespace Sandbox.Game.Components
{
    using System;
    using VRage.ModAPI;
    using VRageRender;

    public class MyDebugRenderComponentLadder : MyDebugRenderComponent
    {
        private IMyEntity m_ladder;

        public MyDebugRenderComponentLadder(IMyEntity ladder) : base(ladder)
        {
            this.m_ladder = ladder;
        }

        public override void DebugDraw()
        {
            MyRenderProxy.DebugDrawAxis(this.m_ladder.PositionComp.WorldMatrix, 1f, false, false, false);
            base.DebugDraw();
        }
    }
}

