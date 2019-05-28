namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics;
    using System;
    using System.Text;
    using VRageMath;

    public class MyRendererStatsComponent : MyDebugComponent
    {
        private static StringBuilder m_frameDebugText = new StringBuilder(0x400);
        private static StringBuilder m_frameDebugText2 = new StringBuilder(0x400);

        public override void Draw()
        {
        }

        public override string GetName() => 
            "RendererStats";

        public Vector2 GetScreenLeftTopPosition()
        {
            MyGuiManager.GetSafeFullscreenRectangle();
            return MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(new Vector2(25f * MyGuiManager.GetSafeScreenScale(), 25f * MyGuiManager.GetSafeScreenScale()));
        }
    }
}

