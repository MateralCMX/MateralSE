namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Weapons;
    using System;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyDebugRenderComponentEngineerTool : MyDebugRenderComponent
    {
        private MyEngineerToolBase m_tool;

        public MyDebugRenderComponentEngineerTool(MyEngineerToolBase tool) : base(tool)
        {
            this.m_tool = tool;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_MISC && (this.m_tool.GetTargetGrid() != null))
            {
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, 0f), this.m_tool.TargetCube.ToString(), Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_MISC)
            {
                MyRenderProxy.DebugDrawSphere(this.m_tool.GunBase.GetMuzzleWorldPosition(), 0.01f, Color.Green, 1f, false, false, true, false);
            }
        }
    }
}

