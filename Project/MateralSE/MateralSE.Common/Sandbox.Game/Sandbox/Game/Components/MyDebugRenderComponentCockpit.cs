namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using System;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyDebugRenderComponentCockpit : MyDebugRenderComponent
    {
        private MyCockpit m_cockpit;

        public MyDebugRenderComponentCockpit(MyCockpit cockpit) : base(cockpit)
        {
            this.m_cockpit = cockpit;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_COCKPIT)
            {
                if (this.m_cockpit.AiPilot != null)
                {
                    this.m_cockpit.AiPilot.DebugDraw();
                }
                MyRenderProxy.DebugDrawText3D(this.m_cockpit.PositionComp.WorldMatrix.Translation, this.m_cockpit.IsShooting() ? "PEW!" : "", Color.Red, 2f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                if (this.m_cockpit.Pilot != null)
                {
                    foreach (Vector3I vectori in this.m_cockpit.NeighbourPositions)
                    {
                        Vector3D vectord;
                        if (this.m_cockpit.IsNeighbourPositionFree(vectori, out vectord))
                        {
                            MyRenderProxy.DebugDrawSphere(vectord, 0.3f, Color.Green, 1f, false, false, true, false);
                        }
                        else
                        {
                            MyRenderProxy.DebugDrawSphere(vectord, 0.3f, Color.Red, 1f, false, false, true, false);
                        }
                    }
                }
            }
        }
    }
}

