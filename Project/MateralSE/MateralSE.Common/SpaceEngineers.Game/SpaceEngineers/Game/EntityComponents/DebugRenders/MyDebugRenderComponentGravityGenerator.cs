namespace SpaceEngineers.Game.EntityComponents.DebugRenders
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using VRageMath;
    using VRageRender;

    public class MyDebugRenderComponentGravityGenerator : MyDebugRenderComponent
    {
        private MyGravityGenerator m_gravityGenerator;

        public MyDebugRenderComponentGravityGenerator(MyGravityGenerator gravityGenerator) : base(gravityGenerator)
        {
            this.m_gravityGenerator = gravityGenerator;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_MISCELLANEOUS && this.m_gravityGenerator.IsWorking)
            {
                MyRenderProxy.DebugDrawOBB(Matrix.CreateScale(this.m_gravityGenerator.FieldSize) * this.m_gravityGenerator.PositionComp.WorldMatrix, Color.CadetBlue, 1f, true, false, true, false);
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_MISCELLANEOUS)
            {
                MyRenderProxy.DebugDrawAxis(this.m_gravityGenerator.PositionComp.WorldMatrix, 2f, false, false, false);
            }
        }
    }
}

