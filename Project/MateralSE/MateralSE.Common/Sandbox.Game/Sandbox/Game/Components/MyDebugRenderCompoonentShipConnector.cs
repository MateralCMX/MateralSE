namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Cube;
    using System;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyDebugRenderCompoonentShipConnector : MyDebugRenderComponent
    {
        private MyShipConnector m_shipConnector;

        public MyDebugRenderCompoonentShipConnector(MyShipConnector shipConnector) : base(shipConnector)
        {
            this.m_shipConnector = shipConnector;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_CONNECTORS_AND_MERGE_BLOCKS)
            {
                MyRenderProxy.DebugDrawSphere(this.m_shipConnector.ConstraintPositionWorld(), 0.05f, Color.Red, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawText3D(this.m_shipConnector.PositionComp.WorldMatrix.Translation, this.m_shipConnector.DetectedGridCount.ToString(), Color.Red, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
        }
    }
}

