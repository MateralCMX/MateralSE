namespace SpaceEngineers.Game.EntityComponents.DebugRenders
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using VRageMath;
    using VRageRender;

    public class MyDebugRenderComponentGravityGeneratorSphere : MyDebugRenderComponent
    {
        private MyGravityGeneratorSphere m_gravityGenerator;

        public MyDebugRenderComponentGravityGeneratorSphere(MyGravityGeneratorSphere gravityGenerator) : base(gravityGenerator)
        {
            this.m_gravityGenerator = gravityGenerator;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_MISCELLANEOUS && this.m_gravityGenerator.IsWorking)
            {
                MyRenderProxy.DebugDrawSphere(this.m_gravityGenerator.PositionComp.WorldMatrix.Translation, this.m_gravityGenerator.Radius, Color.CadetBlue, 1f, false, false, true, false);
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_MISCELLANEOUS)
            {
                MyRenderProxy.DebugDrawAxis(this.m_gravityGenerator.PositionComp.WorldMatrix, 2f, false, false, false);
            }
        }
    }
}

